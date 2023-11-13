using Markdig;
using Markdig.Extensions.Tables;
using Markdig.Extensions.TaskLists;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SkiaSharp;

namespace QuestPDF.Markdown;


/// <summary>
/// This class uses markdig to parse a provided markdown text and convert it to QuestPDF elements
/// </summary>
/// <remarks>
/// The markdig parser is documented in https://github.com/xoofx/markdig/blob/master/doc/parsing-ast.md
/// </remarks>
internal class MarkdownRenderer
{
    private readonly RenderConfig _config;
    private readonly MarkdownPipeline _pipeline;

    internal MarkdownRenderer(RenderConfig? config = null)
    {
        _config = config ?? new RenderConfig();
        _pipeline = new MarkdownPipelineBuilder()
            .DisableHtml()
            .UseEmphasisExtras()
            .UseGridTables()
            .UsePipeTables()
            .UseTaskLists()
            .UseAutoLinks()
            .Build();
    }

    internal IContainer ConvertMarkdown(string markdownText, IContainer pdf)
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
                pdf = pdf.BorderLeft(_config.BlockQuoteBorderThickness)
                    .BorderColor(_config.BlockQuoteBorderColor)
                    .PaddingLeft(10);
                properties.TextStyles.Push(t => t.FontColor(_config.BlockQuoteTextColor));
                break;
        }

        if (block is Table table)
        {
            pdf = ProcessTableBlock(table, pdf, properties);
        }
        else
        {
            pdf.RenderDebug(Colors.Red.Medium, _config.Debug).Column(col =>
            {
                col.Spacing(10);
                
                foreach (var item in block)
                {
                    var container = col.Item();
                    if (block is ListBlock list && item is ListItemBlock listItem)
                    {
                        container.Row(li =>
                        {
                            li.Spacing(5);
                            li.AutoItem().PaddingLeft(10).Text(list.IsOrdered ? $"{listItem.Order}{list.OrderedDelimiter}" : "•");
                            ProcessBlock(item, li.RelativeItem(), properties);
                        });
                    }
                    else
                    {
                        ProcessBlock(item, container, properties);
                    }
                }
            });
        }
        
        // Pop any styles that were applied to the entire container off the stack
        switch (block)
        {
            case QuoteBlock:
                properties.TextStyles.Pop();
                break;
        }

        return pdf;
    }

    private IContainer ProcessTableBlock(Table table, IContainer pdf, TextProperties properties)
    {
        pdf.RenderDebug(Colors.Green.Medium, _config.Debug).Table(td =>
        {
            td.ColumnsDefinition(cd =>
            {
                foreach(var col in table.ColumnDefinitions)
                {
                    // Width is set to 0 for relative columns
                    if (col.Width > 0)
                    {
                        cd.ConstantColumn(col.Width);
                    }
                    else
                    {
                        cd.RelativeColumn();
                    }
                }
            });

            uint rowIdx = 1;
            var rows = table.OfType<TableRow>().ToList();
            foreach (var row in rows)
            {
                if (row.IsHeader) properties.TextStyles.Push(t => t.Bold());
                
                var colIdx = 0;
                var cells = row.OfType<TableCell>().ToList();
                foreach (var cell in cells)
                {
                    var colDef = table.ColumnDefinitions[colIdx];
                    var container = td.Cell()
                        .RowSpan((uint)cell.RowSpan)
                        .Row(rowIdx + 1)
                        .Column((uint)(cell.ColumnIndex >= 0 ? cell.ColumnIndex : colIdx) + 1)
                        .ColumnSpan((uint)cell.RowSpan)
                        .BorderBottom(rowIdx < rows.Count ? (row.IsHeader ? _config.TableHeaderBorderThickness : _config.TableBorderThickness) : 0)
                        .BorderColor(_config.TableBorderColor)
                        .Background(rowIdx % 2 == 0 ? _config.TableEvenRowBackgroundColor : _config.TableOddRowBackgroundColor)
                        .Padding(5)
                        .RenderDebug(Colors.Orange.Medium, _config.Debug);
                    
                    switch (colDef.Alignment)
                    {
                        case TableColumnAlign.Left:
                            container = container.AlignLeft();
                            break;
                        case TableColumnAlign.Center:
                            container = container.AlignCenter();
                            break;
                        case TableColumnAlign.Right:
                            container = container.AlignRight();
                            break;
                    }
                    
                    ProcessBlock(cell, container, properties);

                    colIdx++;
                }
                
                if (row.IsHeader) properties.TextStyles.Pop();
                
                rowIdx++;
            }
        });

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
            pdf.RenderDebug(Colors.Yellow.Medium, _config.Debug).Text(text =>
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
        else if (block is ThematicBreakBlock)
        {
            pdf.RenderDebug(Colors.Green.Medium, _config.Debug)
                .LineHorizontal(_config.HorizontalRuleThickness)
                .LineColor(_config.HorizontalRuleColor);
        }
        else if (block is CodeBlock code)
        {
            pdf.RenderDebug(Colors.Yellow.Medium, _config.Debug)
                .Background(_config.CodeBlockBackground)
                .Padding(5)
                .Text(code.Lines.ToString())
                .FontFamily(_config.CodeFont);
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
                    properties.TextStyles.Push(t => t.FontColor(_config.LinkTextColor).Underline());
                    properties.LinkUrl = link.Url;
                    properties.IsImage = link.IsImage;
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
                                return t.BackgroundColor(_config.MarkedTextBackgroundColor);
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
            properties.IsImage = false;
        }
    }

    private (Stream Bytes, int Width, int Height) DownloadImage(Uri uri)
    {
        var ms = new MemoryStream();
        var httpClient = _config.HttpClientFactory?.Invoke();
        if (httpClient == null) return (ms, 0, 0);
        
        var response = httpClient.GetAsync(uri)
            .GetAwaiter()
            .GetResult();
        
        if (!response.IsSuccessStatusCode) return (ms, 0, 0);
        
        using var stream = response.Content.ReadAsStreamAsync()
            .GetAwaiter()
            .GetResult();

        stream.CopyTo(ms);
        ms.Position = 0;

        var skData = SKData.Create(ms);
        var image = SKBitmap.Decode(skData);
        ms.Position = 0;
        return (ms, image.Width, image.Height);
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
                text.Span(task.Checked ? _config.TaskListCheckedGlyph : _config.TaskListUncheckedGlyph)
                    .FontFamily(_config.UnicodeGlyphFont);
                break;
            case LiteralInline literal:
                ProcessLiteralInline(literal, text, properties)
                    .ApplyStyles(properties.TextStyles.ToList());
                break;
            case CodeInline code:
                text.Span(code.Content)
                    .BackgroundColor(_config.CodeInlineBackground)
                    .FontFamily(_config.CodeFont);
                break;
            default:
                text.Span($"Unknown LeafInline: {inline.GetType()}").BackgroundColor(Colors.Orange.Medium);
                break;
        }
    }

    private TextSpanDescriptor ProcessLiteralInline(LiteralInline literal, TextDescriptor text, TextProperties properties)
    {
        if (string.IsNullOrEmpty(properties.LinkUrl) || !Uri.TryCreate(properties.LinkUrl, UriKind.Absolute, out var uri)) return text.Span(literal.ToString());

        if (!properties.IsImage || !_config.DownloadImages) return text.Hyperlink(literal.ToString(), properties.LinkUrl);
        
        var (image, width, height) = DownloadImage(uri);
        text.Element(e => e.Width(width).Height(height).Image(image));
        return text.Span(string.Empty);
    }
}