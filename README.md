<img src="/img/logo.svg?raw=true" width="128" align="right">

# QuestPDF.Markdown
QuestPDF.Markdown allows rendering markdown into a [QuestPDF](https://www.questpdf.com/) document using the [markdig](https://github.com/xoofx/markdig) parser.

[![Nuget](https://img.shields.io/nuget/v/QuestPDF.Markdown)](https://www.nuget.org/packages/QuestPDF.Markdown)
[![Nuget Prerelease](https://img.shields.io/nuget/vpre/QuestPDF.Markdown?label=nuget%20prerelease)](https://www.nuget.org/packages/QuestPDF.Markdown)

> [!IMPORTANT]  
> QuestPDF.Markdown is **not** a HTML-to-PDF conversion library and does intend to become one. It aims to use markdown to add basic user-provided content into PDFs without the pitfalls of HTML-to-PDF conversion.

## Usage
```csharp
var text = """
           # Hello, world!

           *Greetings* from **markdown**!

           > Hello, back!
           """;

var document = Document.Create(container =>
{
    container.Page(page =>
    {
        page.Margin(20);
        page.Content().Markdown(text);
    });
});
```

![Usage](/img/usage.png?raw=true)

### Styling the output
The styling used by QuestPDF.Markdown can be configured using the configure action.
```csharp
var text = @"> Hello, world!";

var document = Document.Create(container =>
{
    container.Page(page =>
    {
        page.Margin(20);
        page.Content().Markdown(text, options => 
        {
            BlockQuoteBorderColor = Colors.Red.Medium,
            BlockQuoteBorderThickness = 5
        });
    });
});
```

### Rendering images
By default, downloading and rendering external images is disabled.
Images can be downloaded using the `ParsedMarkdownDocument.DownloadImages()` method.
The parsed markdown document can then be passed to the `Markdown()` extension method.
```csharp
var text = @"![title](https://placehold.co/100x50.jpg)";

var markdown = ParsedMarkdownDocument.FromText(text);
await markdown.DownloadImages(httpClient: myHttpClient /* Optionally provide your own HttpClient */);

var document = Document.Create(container =>
{
    container.Page(page =>
    {
        page.Margin(20);
        page.Content().Markdown(markdown);
    });
});
```

### Rendering custom text
In some cases it can be helpful to render custom text content in the PDF (e.g. allowing a user to inject the page number).

This is possible by using the template tag feature:
```csharp
var text = @"This is page {currentPage}/{totalPages}";

var document = Document.Create(container =>
{
    container.Page(page =>
    {
        page.Margin(20);
        page.Content().Markdown(text, options =>
        {
            options.AddTemplateTag("currentPage", t => t.TotalPages());
            options.AddTemplateTag("totalPages", t => t.CurrentPageNumber());
        }));
    });
});
```
> [!NOTE]  
> Note that the template tags are only used to replace simple text content and not larger blocks like tables.

## What's supported?
The aim of this library is to support all basic markdown functionality and some of the extensions supported by markdig.

Currently the following features are supported:
- Headings
- Block quotes
- Code blocks
- Lists (ordered, unordered)
- Emphasis (bold, italic)
- Task lists
- Extra emphasis (subscript, superscript, strikethrough, marked, inserted)
- Tables
- Images
- HTML encoded entities (e.g. `&amp;`, `&lt;`, `&gt;`)
- Custom renderers for text content

Support for the following extensions is currently not planned:
- Raw HTML
- Math blocks
- Diagrams

A full sample can be found in [test.md](tests/QuestPDF.Markdown.Tests/test.md) and the resulting [test.pdf](tests/QuestPDF.Markdown.Tests/test.pdf).

## Contributing
To quickly test changes made in the library, you can make use of the excellent QuestPDF previewer in combination with the QuestPDF.Markdown.Tests project and `dotnet watch`

To render the test markdown file in the companion app, first start the companion app and then run the following command in the root directory of the repository:
```zsh
cd ./tests/QuestPDF.Markdown.Tests && dotnet watch test --filter Name=Render
```

To render the test markdown file in the previewer with additional background colors and margins, run the following command in the root directory of the repository:
```zsh
cd ./tests/QuestPDF.Markdown.Tests && dotnet watch test --filter Name=RenderDebug
```

Any changes made to the MarkdownRenderer class will be automatically reflected in the previewer.
