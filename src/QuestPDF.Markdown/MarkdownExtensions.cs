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
    /// <param name="config">Optional configuration of the renderer</param>
    /// <returns>The QuestPDF container that the markdown text was rendered in</returns>
    public static IContainer Markdown(this IContainer container, string markdown, RenderConfig? config = null)
    {
        var renderer = new MarkdownRenderer(config);
        return renderer.ConvertMarkdown(markdown, container); 
    }
    
    internal static IContainer RenderDebug(this IContainer container, string color, bool debug)
        => debug ? container.Background(color).Padding(5) : container;
    
    internal static TextSpanDescriptor RenderDebug(this TextSpanDescriptor span, string color, bool debug)
        => debug ? span.BackgroundColor(color) : span;
    
    internal static TextSpanDescriptor ApplyStyles(this TextSpanDescriptor span, IList<Func<TextSpanDescriptor, TextSpanDescriptor>> applyStyles)
    {
        foreach(var applyStyle in applyStyles)
            span = applyStyle(span);

        return span;
    }
}