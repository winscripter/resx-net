# ResX.NET
NuGet: https://nuget.org/packages/ResX.NET

Easy manipulation of ResX files.

Runs on .NET 6.0 and later. Cross platform and high performance.

To load an existing ResX file:
```cs
var resx = new ResXFile(ResXFile.Parse(File.ReadAllText("SomeFile.resx")));
foreach (ResXData data in resx.DataList)
{
    // Do something with data
}
```

To write a ResX file:
```cs
var resx = new ResXFile();
resx.DataList.Add(new ResXData(Name: "Greeting", Value: "Hello, World!", Comment: null));

string result = resx.Build(); // Returns the string representation
File.WriteAllText("result.resx", result);
```

To convert ResX files:
```cs
ResXFile resx = /*...*/;

Console.WriteLine(ResXConvert.ToCSharp(resx));

Console.WriteLine(ResXConvert.ToMarkdown(resx));

Console.WriteLine(await ResXConvert.ToJsonAsync(resx));
```
