using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace QuestPDF.Markdown;

public static class MarkdownExtensions
{
    /// <summary>
    /// Renders a markdown text into a QuestPDF container
    /// </summary>
    /// <param name="container">The QuestPDF container to render in</param>
    /// <param name="markdown">The markdown text to render</param>
    /// <param name="options">Optional configuration of the renderer</param>
    /// <returns>The QuestPDF container that the markdown text was rendered in</returns>
    public static IContainer Markdown(this IContainer container, string markdown, MarkdownRendererOptions? options = null)
    {
        var renderer = new MarkdownRenderer(options);
        return renderer.ConvertMarkdown(markdown, container); 
    }
    
    internal static IContainer PaddedDebugArea(this IContainer container, string label, string color)
        => container.DebugArea(label, color).PaddingTop(20);
    
    internal static TextSpanDescriptor ApplyStyles(this TextSpanDescriptor span, IList<Func<TextSpanDescriptor, TextSpanDescriptor>> applyStyles)
    {
        foreach(var applyStyle in applyStyles)
            span = applyStyle(span);

        return span;
    }
}