# Update

Issue got fixed in puppeteer sharp 9.1.0

## Minimal repro PuppeteerSharp issue 

PuppeteerSharp 9.0.0 invented a bug that will cause to fail the parameterless ctor of BrowserFetcher when using in some published exe with "Produce single file" enabled.

``` powershell
PS C:\source\repos\PuppeteerSharpMinimalRepro> dotnet publish ./PuppeteerSharpMinimalRepro --configuration Release -f net6.0 -p:PublishProfile=FolderProfile
MSBuild version 17.4.1+9a89d02ff for .NET
  Determining projects to restore...
  Restored C:\source\repos\PuppeteerSharpMinimalRepro\PuppeteerSharpMinimalRepro\PuppeteerSharpMinimalRepr
  o.csproj (in 252 ms).
C:\source\repos\PuppeteerSharpMinimalRepro\PuppeteerSharpMinimalRepro\Program.cs(4,59): warning IL3000: 'S
ystem.Reflection.Assembly.Location' always returns an empty string for assemblies embedded in a single-file app. If the
 path to the app directory is needed, consider calling 'System.AppContext.BaseDirectory'. [C:\source\repos
\PuppeteerSharpMinimalRepro\PuppeteerSharpMinimalRepro\PuppeteerSharpMinimalRepro.csproj]
  PuppeteerSharpMinimalRepro -> C:\source\repos\PuppeteerSharpMinimalRepro\PuppeteerSharpMinimalRepro\bin\
  Release\net6.0\win-x86\PuppeteerSharpMinimalRepro.dll
  PuppeteerSharpMinimalRepro -> C:\source\repos\PuppeteerSharpMinimalRepro\PuppeteerSharpMinimalRepro\bin\
  Release\net6.0\publish\win-x86\
PS C:\source\repos\PuppeteerSharpMinimalRepro> .\PuppeteerSharpMinimalRepro\bin\Release\net6.0\publish\win-x86\PuppeteerSharpMinimalRepro.exe
AppDomain.BaseDirectory: C:\source\repos\PuppeteerSharpMinimalRepro\PuppeteerSharpMinimalRepro\bin\Release\net6.0\publish\win-x86\
typeof(Puppeteer).Assembly.Location:
Unhandled exception. System.ArgumentException: The path is empty. (Parameter 'path')
   at System.IO.Path.GetFullPath(String path)
   at System.IO.FileInfo..ctor(String originalPath, String fullPath, String fileName, Boolean isNormalized)
   at System.IO.FileInfo..ctor(String fileName)
   at PuppeteerSharp.BrowserFetcher.GetExecutablePath() in C:\projects\puppeteer-sharp\lib\PuppeteerSharp\BrowserFetcher.cs:line 354
   at PuppeteerSharp.BrowserFetcher..ctor() in C:\projects\puppeteer-sharp\lib\PuppeteerSharp\BrowserFetcher.cs:line 53
   at Program.<Main>$(String[] args) in C:\source\repos\PuppeteerSharpMinimalRepro\PuppeteerSharpMinimalRepro\Program.cs:line 5
   at Program.<Main>(String[] args)
PS C:\source\repos\PuppeteerSharpMinimalRepro>

```

Overloads of BrowserFetcher..ctor() can be used to work around the issue. Seems like AppContext.BaseDirectory and typeof(Puppeteer).Assembly.Location in https://github.com/hardkoded/puppeteer-sharp/blob/master/lib/PuppeteerSharp/BrowserFetcher.cs#L354 are both something unusable.

https://learn.microsoft.com/en-us/dotnet/core/deploying/single-file/overview?tabs=cli#output-differences-from-net-3x seems to be misleading. It claims that "To access files next to the executable, use AppContext.BaseDirectory." but does not mean the extracted executable but the single file exe thing.

https://learn.microsoft.com/en-us/dotnet/core/deploying/single-file/warnings/il3000 makes clear that the fall back "Assembly.Location" isnt the right thing either. 

Be warned. Things may break if you use PuppeteerSharp 9.0.0 and 9.0.1 with "Produce single file" enabled. For now you can use overloads of the BrowserFetcher ctor or PuppeteerSharp 8.0.0 . This was tested using .net 6 and .net 7.

e.g.

```
var options = new BrowserFetcherOptions();
options.Path = Path.GetTempPath();
using var browserFetcher = new BrowserFetcher(options);
var revisionInfo = await browserFetcher.DownloadAsync();
await using var browser = await Puppeteer.LaunchAsync(new LaunchOptions { Headless = true, ExecutablePath = revisionInfo.ExecutablePath });
await using var page = await browser.NewPageAsync();
await page.GoToAsync("http://www.google.com");
await page.ScreenshotAsync("google.jpg");
```

## Update 9.0.3

```
.\PuppeteerSharpMinimalRepro.exe
AppDomain.BaseDirectory: C:\PuppeteerSharpMinimalRepro\PuppeteerSharpMinimalRepro\bin\Release\net6.0\publish\
typeof(Puppeteer).Assembly.Location:
Unhandled exception. System.NotSupportedException: CodeBase is not supported on assemblies loaded from a single-file bundle.
   at System.Reflection.RuntimeAssembly.get_CodeBase()
   at PuppeteerSharp.BrowserFetcher.GetExecutablePath() in C:\projects\puppeteer-sharp\lib\PuppeteerSharp\BrowserFetcher.cs:line 356
   at PuppeteerSharp.BrowserFetcher..ctor() in C:\projects\puppeteer-sharp\lib\PuppeteerSharp\BrowserFetcher.cs:line 53
   at Program.<Main>$(String[] args) in C:\PuppeteerSharpMinimalRepro\PuppeteerSharpMinimalRepro\Program.cs:line 5
   at Program.<Main>(String[] args)
PS C:\PuppeteerSharpMinimalRepro\PuppeteerSharpMinimalRepro\bin\Release\net6.0\publish>
```
