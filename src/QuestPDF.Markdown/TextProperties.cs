using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace QuestPDF.Markdown;

internal class TextProperties
{
    public Stack<Func<IContainer, IContainer>> BlockStyles { get; } = new();
    public Stack<Func<TextSpanDescriptor, TextSpanDescriptor>> TextStyles { get; } = new();
    public string? LinkUrl { get; set; }
}