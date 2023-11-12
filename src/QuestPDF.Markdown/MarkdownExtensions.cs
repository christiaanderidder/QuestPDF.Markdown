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
    /// <param name="debug">Adds background colors and margins to elements to help debugging</param>
    /// <returns>The QuestPDF container that the markdown text was rendered in</returns>
    public static IContainer Markdown(this IContainer container, string markdown, bool debug = false)
    {
        var renderer = new MarkdownRenderer(debug);
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