using Markdig.Syntax;
using NUnit.Framework;
using QuestPDF.Markdown.Parsing;

namespace QuestPDF.Markdown.Tests;

internal sealed class TemplateParsingTests
{
    [Test]
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
        
        Assert.That(block.Tag, Is.EqualTo("template"));
        Assert.That(block.Span.Start, Is.EqualTo(11));
        Assert.That(block.Span.End, Is.EqualTo(19));
    }
}