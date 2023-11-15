<img src="/img/logo.svg?raw=true" width="128" align="right">

# QuestPDF.Markdown
QuestPDF.Markdown allows rendering markdown into a [QuestPDF](https://www.questpdf.com/) document using the [markdig](https://github.com/xoofx/markdig) parser.

[![Nuget](https://img.shields.io/nuget/v/QuestPDF.Markdown)](https://www.nuget.org/packages/QuestPDF.Markdown)

## Usage
```csharp
var text = 
@"# Hello, world!
*Greetings* from **markdown**!
> Hello, back!";

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
The styling used by QuestPDF.Markdown can be configured using `MarkdownRendererOptions`.
```csharp
var text = @"> Hello, world!";

var options = new MarkdownRendererOptions
{
    BlockQuoteBorderColor = Colors.Red.Medium,
    BlockQuoteBorderThickness = 5
};

var document = Document.Create(container =>
{
    container.Page(page =>
    {
        page.Margin(20);
        page.Content().Markdown(text, options);
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
        page.Content().Markdown(markdown, options);
    });
});
```

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

Support for the following extensions is currently not planned:
- Raw HTML
- Math blocks
- Diagrams

A full sample can be found in [test.md](tests/QuestPDF.Markdown.Tests/test.md) and the resulting [test.pdf](tests/QuestPDF.Markdown.Tests/test.pdf).

## Contributing
To quickly test changes made in the library, you can make use of the excellent QuestPDF previewer in combination with the QuestPDF.Markdown.Tests project and `dotnet watch`

To render the test markdown file in the previewer, run the following command in the root directory of the repository:
```zsh
dotnet watch test --project ./tests/QuestPDF.Markdown.Tests -- --filter Name=Render
```

To render the test markdown file in the previewer with additional background colors and margins, run the following command in the root directory of the repository:
```zsh
dotnet watch test --project ./tests/QuestPDF.Markdown.Tests -- --filter Name=RenderDebug
```

Any changes made to the MarkdownRenderer class will be automatically reflected in the previewer.
