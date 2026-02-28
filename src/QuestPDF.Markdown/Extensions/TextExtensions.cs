using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace QuestPDF.Markdown.Extensions;

internal static class TextExtensions
{
    internal static void Align(this TextDescriptor text, TextHorizontalAlignment alignment) =>
        GetAlignment(text, alignment).Invoke();

    private static Action GetAlignment(TextDescriptor text, TextHorizontalAlignment alignment) =>
        alignment switch
        {
            TextHorizontalAlignment.Left => text.AlignLeft,
            TextHorizontalAlignment.Center => text.AlignCenter,
            TextHorizontalAlignment.Right => text.AlignRight,
            TextHorizontalAlignment.Justify => text.Justify,
            TextHorizontalAlignment.Start => text.AlignStart,
            TextHorizontalAlignment.End => text.AlignEnd,
            _ => throw new ArgumentOutOfRangeException(nameof(alignment), alignment, null),
        };
}
