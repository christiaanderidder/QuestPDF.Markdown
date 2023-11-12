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