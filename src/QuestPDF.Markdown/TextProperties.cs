using QuestPDF.Fluent;

namespace QuestPDF.Markdown;

internal sealed class TextProperties
{
    public Stack<Func<TextSpanDescriptor, TextSpanDescriptor>> TextStyles { get; } = new();
    public string? LinkUrl { get; set; }
    public string? ImageUrl { get; set; }
}