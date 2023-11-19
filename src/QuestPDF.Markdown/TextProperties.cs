using QuestPDF.Fluent;

namespace QuestPDF.Markdown;

internal sealed class TextProperties
{
    public Stack<Func<TextSpanDescriptor, TextSpanDescriptor>> TextStyles { get; } = new();
    public string? LinkUrl { get; set; }
    public bool IsImage { get; set; }
}