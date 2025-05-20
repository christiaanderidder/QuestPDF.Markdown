using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace QuestPDF.Markdown;

public static class MarkdownExtensions
{
    /// <summary>
    /// Parses and renders a markdown text into a QuestPDF container
    /// </summary>
    /// <remarks>
    /// This method will not download and render external images.
    /// </remarks>
    /// <param name="container">The QuestPDF container to render in</param>
    /// <param name="markdown">The markdown text to render</param>
    public static void Markdown(this IContainer container, string markdown) =>
        container.Component(MarkdownRenderer.Create(markdown));
    
    /// <summary>
    /// Parses and renders a markdown text into a QuestPDF container
    /// </summary>
    /// <remarks>
    /// This method will not download and render external images.
    /// </remarks>
    /// <param name="container">The QuestPDF container to render in</param>
    /// <param name="markdown">The markdown text to render</param>
    /// <param name="configure">Action to configure the renderer</param>
    public static void Markdown(this IContainer container, string markdown, Action<MarkdownRendererOptions> configure) =>
        container.Component(MarkdownRenderer.Create(markdown, configure));
    
    /// <summary>
    /// Renders a pre-parsed markdown document into a QuestPDF container
    /// </summary>
    /// <param name="container">The QuestPDF container to render in</param>
    /// <param name="markdown">An instance of ParsedMarkdownDocument, which allows for preloading external resources</param>
    public static void Markdown(this IContainer container, ParsedMarkdownDocument markdown) =>
        container.Component(MarkdownRenderer.Create(markdown));
    
    /// <summary>
    /// Renders a pre-parsed markdown document into a QuestPDF container
    /// </summary>
    /// <param name="container">The QuestPDF container to render in</param>
    /// <param name="markdown">An instance of ParsedMarkdownDocument, which allows for preloading external resources</param>
    /// <param name="configure">Action to configure the renderer</param>
    public static void Markdown(this IContainer container, ParsedMarkdownDocument markdown, Action<MarkdownRendererOptions> configure) =>
        container.Component(MarkdownRenderer.Create(markdown, configure));

    internal static IContainer PaddedDebugArea(this IContainer container, string label, string color) =>
        container.DebugArea(label, color).PaddingTop(20);
    
    internal static TextSpanDescriptor ApplyStyles(this TextSpanDescriptor span, IList<Func<TextSpanDescriptor, TextSpanDescriptor>> applyStyles)
    {
        foreach(var applyStyle in applyStyles)
            span = applyStyle(span);

        return span;
    }
}