using Markdig.Syntax;
using QuestPDF.Markdown.Parsing;

namespace QuestPDF.Markdown.Tests.Parsing;

internal sealed class TemplateTests
{
    [Test]
    public async Task ParsesTemplate()
    {
        const string markdown = "This is a {template} test";
        var document = ParsedMarkdownDocument.FromText(markdown);

        var block = document
            .MarkdigDocument.OfType<ParagraphBlock>()
            .Single()
            .Inline!.OfType<TemplateInline>()
            .Single();

        await Assert.That(block.Tag).IsEqualTo("template");
        await Assert.That(block.Span.Start).IsEqualTo(11);
        await Assert.That(block.Span.End).IsEqualTo(19);
    }
}
