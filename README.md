# QuestPDF.Markdown
A helper that allows rendering markdown into a QuestPDF document 

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