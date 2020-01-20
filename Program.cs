using System;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using BenchmarkDotNet.Running;

namespace XvsXml {
   class Program {
      static void Main(string[] args) {
         // Verify that methods return the same counts
         var queries = new QueryBenchmarks();
         queries.GlobalSetup();
         var resultXPathXml = queries.ItemByClassXPathXml();
         var resultXPathX = queries.ItemByClassXPathX();
         var resultLinqMethodsX = queries.ItemByClassLinqMethodsX();
         var resultLinqExpr = queries.ItemByClassLinqExprX();

         if (resultXPathXml != resultXPathX
               || resultXPathXml != resultLinqMethodsX
               || resultXPathXml != resultLinqExpr) {
            Console.WriteLine("{0}={1}, {2}={3}, {4}={5}, {6}={7}",
               nameof(resultXPathXml), resultXPathXml.ToString(),
               nameof(resultXPathX), resultXPathX.ToString(),
               nameof(resultLinqMethodsX), resultLinqMethodsX.ToString(),
               nameof(resultLinqExpr), resultLinqExpr.ToString());
            Environment.Exit(1);
         }

         // Run benchmarks
         BenchmarkRunner.Run<QueryBenchmarks>();
      }
   }

   [MemoryDiagnoser]
   [Orderer(SummaryOrderPolicy.FastestToSlowest)]
   public class QueryBenchmarks {
      private static readonly string[] queries = {
         "items/item[@class='a']",
         "items/item[@class='b']",
         "items/item[@class='c']",
         "items/item[@class='d']",
         "items/item[@class='e']"
      };
      private static readonly string[] classes = { "a", "b", "c", "d", "e" };

      private XElement _xroot;
      private XmlDocument _xmlDoc;

      [GlobalSetup]
      public void GlobalSetup() {
         const string fileName = "test-data.xml";
         var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName);

         _xroot = XElement.Load(filePath);
         _xmlDoc = new XmlDocument();
         _xmlDoc.Load(filePath);
      }

      [Benchmark(Description = "XPathXml")]
      public int ItemByClassXPathXml() {
         var total = 0;
         for (var index = 0; index < queries.Length; index++) {
            var nodes = _xmlDoc.DocumentElement.SelectNodes(queries[index]);
            total += nodes.Count;
         }
         return total;
      }

      [Benchmark(Description = "XPathX")]
      public int ItemByClassXPathX() {
         var total = 0;
         for (var index = 0; index < queries.Length; index++) {
            var elements = _xroot.XPathSelectElements(queries[index]);
            total += elements.Count();
         }
         return total;
      }

      [Benchmark(Description = "LinkMethodsX")]
      public int ItemByClassLinqMethodsX() {
         var total = 0;
         for (var index = 0; index < classes.Length; index++) {
            var comparand = classes[index];
            total += _xroot.Elements("items")
               .Elements("item")
               .Where(e => e.GetAttribute("class").Equals(comparand))
               .Count();
         }
         return total;
      }

      [Benchmark(Description = "LinkExprX")]
      public int ItemByClassLinqExprX() {
         var total = 0;
         for (var index = 0; index < classes.Length; index++) {
            var comparand = classes[index];
            total += (from items in _xroot.Elements("items")
                      from item in items.Elements("item")
                      where item.GetAttribute("class").Equals(comparand)
                      select item).Count();
         }
         return total;
      }
   }

   public static class XElementExtensions {
      public static string GetAttribute(this XElement element, string attributeName) {
         return element.Attribute(attributeName)?.Value ?? String.Empty;
      }
   }
}
