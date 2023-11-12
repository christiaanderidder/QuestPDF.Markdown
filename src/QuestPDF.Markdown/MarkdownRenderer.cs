using Markdig;
using Markdig.Extensions.TaskLists;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace QuestPDF.Markdown;


/// <summary>
/// This class uses markdig to parse a provided markdown text and convert it to QuestPDF elements
/// </summary>
/// <remarks>
/// The markdig parser is documented in https://github.com/xoofx/markdig/blob/master/doc/parsing-ast.md
/// </remarks>
internal class MarkdownRenderer
{
    private readonly bool _debug;
    private readonly MarkdownPipeline _pipeline;

    internal MarkdownRenderer(bool debug)
    {
        _debug = debug;
        _pipeline = new MarkdownPipelineBuilder()
            .DisableHtml()
            .UseAdvancedExtensions()
            .Build();
    }
    
    internal IContainer ConvertMarkdown(string markdownText, IContainer pdf, bool debug = false)
    {
        var document = Markdig.Markdown.Parse(markdownText, _pipeline);
        var properties = new TextProperties();
        return ProcessContainerBlock(document, pdf, properties);
    }
    
    /// <summary>
    /// Processes a Block, which can be a ContainerBlock or a LeafBlock.
    /// </summary>
    private void ProcessBlock(Block block, IContainer pdf, TextProperties properties)
    {
        switch (block)
        {
            case ContainerBlock container:
                ProcessContainerBlock(container, pdf, properties);
                break;
            case LeafBlock leaf:
                ProcessLeafBlock(leaf, pdf, properties);
                break;
        }
    }

    /// <summary>
    /// Processes a ContainerBlock. Containers blocks contain other containers blocks or regular blocks (LeafBlock).
    /// Container blocks are represented by a QuestPDF column with a row for each child item
    /// </summary>
    private IContainer ProcessContainerBlock(ContainerBlock block, IContainer pdf, TextProperties properties)
    {
        if (!block.Any()) return pdf;
        
        // Push any styles that should be applied to the entire container on the stack
        switch (block)
        {
            case QuoteBlock:
                pdf = pdf.BorderLeft(2).BorderColor(Colors.Grey.Lighten2).PaddingLeft(10);
                properties.TextStyles.Push(t => t.FontColor(Colors.Grey.Darken1));
                break;
        }
        
        pdf.RenderDebug(Colors.Red.Medium, _debug).Column(col =>
        {
            col.Spacing(10);
            
            var rowNum = 1;
            foreach (var item in block)
            {
                if (block is ListBlock list)
                {
                    var num = rowNum;
                    col.Item().Row(li =>
                    {
                        li.Spacing(5);
                        li.AutoItem().PaddingLeft(10).Text(list.IsOrdered ? $"{num}{list.OrderedDelimiter}" : "•");
                        ProcessBlock(item, li.RelativeItem(), properties);
                    });
                }
                else
                {
                    ProcessBlock(item, col.Item(), properties);
                }
                rowNum++;
            }
            
            
        });
        
        // Pop any styles that were applied to the entire container off the stack
        switch (block)
        {
            case QuoteBlock:
                properties.TextStyles.Pop();
                break;
        }

        return pdf;
    }

    /// <summary>
    /// Processes a LeafBlock. Blocks contain container inlines (ContainerInline) or regular inline elements (LeafInline).
    /// Leaf blocks are represented by a QuestPDF text element with a text span for each child element
    /// </summary>
    private void ProcessLeafBlock(LeafBlock block, IContainer pdf, TextProperties properties)
    {
        // Push any styles that should be applied to the entire block on the stack
        switch (block)
        {
            case HeadingBlock heading:
                properties.TextStyles.Push(t => t.FontSize(-2 * (heading.Level - 1) + 28).Bold());
                break;
        }

        if (block.Inline != null && block.Inline.Any())
        {
            pdf.RenderDebug(Colors.Yellow.Medium, _debug).Text(text =>
            {
                // Process the block's inline elements
                foreach (var item in block.Inline)
                {
                    switch (item)
                    {
                        case ContainerInline container:
                            ProcessContainerInline(container, text, properties);
                            break;
                        case LeafInline leaf:
                            ProcessLeafInline(leaf, text, properties);
                            break;
                    }
                }
            });
        }
        else if(block is CodeBlock code)
        {
            pdf.RenderDebug(Colors.Yellow.Medium, _debug)
                .Background(Colors.Grey.Lighten3)
                .Padding(5)
                .Text(code.Lines.ToString())
                .FontFamily(Fonts.CourierNew);
        }

        
        // Pop any styles that were applied to the entire block off the stack
        switch (block)
        {
            case HeadingBlock:
                properties.TextStyles.Pop();
                break;
        }
    }

    /// <summary>
    /// Processes a ContainerInline. A container inline contains other container inlines or regular inline elements (LeafInline) that are part of the same span of text.
    /// This method receives an existing PDF text element and adds text span elements to it for each child item
    /// </summary>
    private void ProcessContainerInline(ContainerInline inline, TextDescriptor text, TextProperties properties)
    {
        foreach (var item in inline)
        {
            // Push any styles that should be applied to the entire span on the stack
            switch (inline)
            {
                case LinkInline link:
                    properties.TextStyles.Push(t => t.FontColor(Colors.Blue.Medium).Underline());
                    properties.LinkUrl = link.Url;
                    break;
                case EmphasisInline emphasis:
                    properties.TextStyles.Push(t =>
                    {
                        switch (emphasis.DelimiterChar, emphasis.DelimiterCount)
                        {
                            case ('^', 1):
                                return t.Superscript();
                            case ('~', 1):
                                return t.Subscript();
                            case ('~', 2):
                                return t.Strikethrough();
                            case ('+', 2):
                                return t.Underline();
                            case ('=', 2):
                                return t.BackgroundColor(Colors.Yellow.Lighten2);
                        }
                        return emphasis.DelimiterCount == 2 ? t.Bold() : t.Italic();
                    });
                    break;
            }

            switch (item)
            {
                case ContainerInline container:
                    ProcessContainerInline(container, text, properties);
                    break;
                case LeafInline leaf:
                    ProcessLeafInline(leaf, text, properties);
                    break;
            }

            // Pop any styles that were applied to the entire span off the stack
            switch (inline)
            {
                case EmphasisInline:
                case LinkInline:
                    properties.TextStyles.Pop();
                    break;
            }

            // Reset the link URL
            properties.LinkUrl = null;
        }
    }

    /// <summary>
    /// Processes a LeafInline. Regular inline elements (LeafInline) contain plain text.
    /// This method receives an existing PDF text element and adds a text span containing the plain text to it.
    /// </summary>
    private void ProcessLeafInline(LeafInline inline, TextDescriptor text, TextProperties properties)
    {
        switch (inline)
        {
            case AutolinkInline autoLink:
                var linkSpan = text.Hyperlink(autoLink.Url, autoLink.Url);
                linkSpan.ApplyStyles(properties.TextStyles.ToList());
                break;
            case LineBreakInline:
                // Ignore markdown line breaks, they are used for formatting the source code.
                //span = text.Span("\n");
                break;
            case TaskList task:
                var taskSpan = task.Checked ? text.Span("\u25a0") : text.Span("\u25a1");
                taskSpan.FontFamily(Fonts.CourierNew);
                break;
            case LiteralInline literal:
                var literalSpan = !string.IsNullOrEmpty(properties.LinkUrl)
                    ? text.Hyperlink(literal.ToString(), properties.LinkUrl)
                    : text.Span(literal.ToString());
                literalSpan.ApplyStyles(properties.TextStyles.ToList());
                break;
            case CodeInline code:
                text.Span(code.Content).BackgroundColor(Colors.Grey.Lighten3).FontFamily(Fonts.CourierNew);
                break;
            default:
                text.Span($"Unknown LeafInline: {inline.GetType()}").BackgroundColor(Colors.Orange.Medium);
                break;
        }
    }
}