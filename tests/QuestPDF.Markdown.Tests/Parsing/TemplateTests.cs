using Markdig.Syntax;
using QuestPDF.Markdown.Parsing;

namespace QuestPDF.Markdown.Tests.Parsing;

public sealed class TemplateTests
{
    [Fact]
    public void ParsesTemplate()
    {
        const string markdown = "This is a {template} test";
        var document = ParsedMarkdownDocument.FromText(markdown);

        var block = document.MarkdigDocument
            .OfType<ParagraphBlock>()
            .Single()
            .Inline!
            .OfType<TemplateInline>()
            .Single();
        
        Assert.Equal("template", block.Tag);
        Assert.Equal(11, block.Span.Start);
        Assert.Equal(19, block.Span.End);
    }
}