using Markdig.Syntax.Inlines;

namespace QuestPDF.Markdown.Parsing;

internal sealed class TemplateInline : LeafInline
{
    public TemplateInline(string tag) => Tag = tag;

    public string Tag { get; }
}