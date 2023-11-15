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
    /// To achieve this use <see cref="Markdown(QuestPDF.Infrastructure.IContainer,ParsedMarkdownDocument,QuestPDF.Markdown.MarkdownRendererOptions?)"/> instead.
    /// </remarks>
    /// <param name="container">The QuestPDF container to render in</param>
    /// <param name="markdown">The markdown text to render</param>
    /// <param name="options">Optional configuration of the renderer</param>
    /// <returns>The QuestPDF container that the markdown text was rendered in</returns>
    public static IContainer Markdown(this IContainer container, string markdown, MarkdownRendererOptions? options = null) =>
        MarkdownRenderer.Create(markdown, options).ConvertMarkdown(container);
    
    /// <summary>
    /// Renders a pre-parsed markdown document into a QuestPDF container
    /// </summary>
    /// <param name="container">The QuestPDF container to render in</param>
    /// <param name="markdown">An instance of ParsedMarkdownDocument, which allows for preloading external resources</param>
    /// <param name="options">Optional configuration of the renderer</param>
    /// <returns>The QuestPDF container that the markdown text was rendered in</returns>
    public static IContainer Markdown(this IContainer container, ParsedMarkdownDocument markdown, MarkdownRendererOptions? options = null) =>
        MarkdownRenderer.Create(markdown, options).ConvertMarkdown(container);

    internal static IContainer PaddedDebugArea(this IContainer container, string label, string color)
        => container.DebugArea(label, color).PaddingTop(20);
    
    internal static TextSpanDescriptor ApplyStyles(this TextSpanDescriptor span, IList<Func<TextSpanDescriptor, TextSpanDescriptor>> applyStyles)
    {
        foreach(var applyStyle in applyStyles)
            span = applyStyle(span);

        return span;
    }
}