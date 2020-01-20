# XvsXml

I have to convert some legacy code from System.Xml to System.Xml.Linq. The code uses many XPath queries. I was curious how XPath performed on XElements given its optimized for Linq access so I used BenchmarkDotNet to evaluate a few ways of counting a filtered set of elements.

- **XPathXml**: Filter XmlElements using XPath. This is the baseline.
- **XPathX**: Filter XElements using XPath.
- **LinkMethodsX**: Filter XElements using System.Xml.Link methods.
- **LinkExprX**: Filter XElements using Linq query expressions.

This is my first System.Xml.Linq code and I am not confident I am using it appropriately. I'm pretty confident that the methods being compared are producing the correct result, and doing so in a similar way, but I am not sure that I am using the best and/or idiomatic Linq tactics.

``` ini
BenchmarkDotNet=v0.12.0, OS=Windows 10.0.18363
Intel Core i7-5600U CPU 2.60GHz (Broadwell), 1 CPU, 4 logical and 2 physical cores
.NET Core SDK=3.1.100
  [Host]     : .NET Core 3.1.0 (CoreCLR 4.700.19.56402, CoreFX 4.700.19.56404), X64 RyuJIT
  DefaultJob : .NET Core 3.1.0 (CoreCLR 4.700.19.56402, CoreFX 4.700.19.56404), X64 RyuJIT
```
|       Method |       Mean |   StdDev |    Gen 0 | Allocated |
|------------- |-----------:|---------:|---------:|----------:|
| LinkMethodsX |   372.0 us |  2.38 us |   0.4883 |   1.45 KB |
|    LinkExprX |   485.2 us |  2.49 us |  77.6367 | 158.71 KB |
|     XPathXml | 1,668.0 us | 11.70 us | 187.5000 | 383.01 KB |
|       XPathX | 1,978.7 us | 19.33 us | 332.0313 | 683.48 KB |

I am curious about the Allocated memory differences. I tried to avoid allocating any memory in the code that varies the filters, and the LinkMethodsX result suggests I was successful. It uses the same method as LinkExprX. I used a different--but similar--technique for the XPath variations. I am assuming that parsing the XPath expression accounts for some of the difference between the XPath-based methods and the Linq-based methods.
