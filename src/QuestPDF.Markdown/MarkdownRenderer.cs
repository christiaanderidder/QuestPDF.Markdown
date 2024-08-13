using Markdig.Extensions.Tables;
using Markdig.Extensions.TaskLists;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using QuestPDF.Markdown.Extensions;

namespace QuestPDF.Markdown;


/// <summary>
/// This class uses markdig to parse a provided markdown text and convert it to QuestPDF elements
/// </summary>
/// <remarks>
/// The markdig parser is documented in https://github.com/xoofx/markdig/blob/master/doc/parsing-ast.md
/// </remarks>
internal sealed class MarkdownRenderer : IComponent
{
    private readonly MarkdownRendererOptions _options;
    private readonly ParsedMarkdownDocument _document;
    private readonly TextProperties _textProperties;

    private MarkdownRenderer(ParsedMarkdownDocument document, MarkdownRendererOptions? options)
    {
        _document = document;
        _options = options ?? new MarkdownRendererOptions();
        _textProperties = new TextProperties();
    }

    public void Compose(IContainer pdf) => ProcessContainerBlock(_document.MarkdigDocument, pdf);

    /// <summary>
    /// Processes a Block, which can be a ContainerBlock or a LeafBlock.
    /// </summary>
    private void ProcessBlock(Block block, IContainer pdf)
    {
        switch (block)
        {
            case ContainerBlock container:
                ProcessContainerBlock(container, pdf);
                break;
            case LeafBlock leaf:
                ProcessLeafBlock(leaf, pdf);
                break;
        }
    }

    /// <summary>
    /// Processes a ContainerBlock. Containers blocks contain other containers blocks or regular blocks (LeafBlock).
    /// Container blocks are represented by a QuestPDF column with a row for each child item
    /// </summary>
    private IContainer ProcessContainerBlock(ContainerBlock block, IContainer pdf)
    {
        if (block.Count == 0) return pdf;

        if(_options.Debug && block is not MarkdownDocument) pdf = pdf.PaddedDebugArea(block.GetType().Name, Colors.Blue.Medium);
        
        // Push any styles that should be applied to the entire container on the stack
        switch (block)
        {
            case QuoteBlock:
                pdf = pdf.BorderLeft(_options.BlockQuoteBorderThickness)
                    .BorderColor(_options.BlockQuoteBorderColor)
                    .PaddingLeft(10);
                _textProperties.TextStyles.Push(t => t.FontColor(_options.BlockQuoteTextColor));
                break;
        }

        if (block is Table table)
        {
            pdf = ProcessTableBlock(table, pdf);
        }
        else
        {
            pdf.Column(col =>
            {
                foreach (var item in block)
                {
                    var container = col.Item();
                    if (block is ListBlock list && item is ListItemBlock listItem)
                    {
                        col.Spacing(_options.ListItemSpacing);
                        container.Row(li =>
                        {
                            li.Spacing(5);
                            li.AutoItem().PaddingLeft(10).Text(list.IsOrdered ? $"{listItem.Order}{list.OrderedDelimiter}" : _options.UnorderedListGlyph);
                            ProcessBlock(item, li.RelativeItem());
                        });
                    }
                    else
                    {
                        // Paragraphs inside a list get the same spacing as the list items themselves
                        col.Spacing(item.Parent is ListItemBlock ? _options.ListItemSpacing : _options.ParagraphSpacing);
                        ProcessBlock(item, container);
                    }
                }
            });
        }
        
        // Pop any styles that were applied to the entire container off the stack
        switch (block)
        {
            case QuoteBlock:
                _textProperties.TextStyles.Pop();
                break;
        }

        return pdf;
    }

    private IContainer ProcessTableBlock(Table table, IContainer pdf)
    {
        pdf.Table(td =>
        {
            td.ColumnsDefinition(cd =>
            {
                foreach(var col in table.ColumnDefinitions)
                {
                    // Widths are provided as a percentage
                    cd.RelativeColumn(col.Width > 0 ? col.Width : 1f);
                }
            });

            uint rowIdx = 0;
            var rows = table.OfType<TableRow>().ToList();
            foreach (var row in rows)
            {
                if (row.IsHeader) _textProperties.TextStyles.Push(t => t.Bold());
                
                var colIdx = 0;
                var cells = row.OfType<TableCell>().ToList();
                foreach (var cell in cells)
                {
                    var isLast = rowIdx == rows.Count - 1;
                    var colDef = table.ColumnDefinitions[colIdx];
                    var container = td.Cell()
                        .RowSpan((uint)cell.RowSpan)
                        .Row(rowIdx + 1)
                        .Column((uint)(cell.ColumnIndex >= 0 ? cell.ColumnIndex : colIdx) + 1)
                        .ColumnSpan((uint)cell.ColumnSpan)
                        .Border(_options, row.IsHeader, isLast)
                        .Background(rowIdx % 2 == 1 ? _options.TableEvenRowBackgroundColor : _options.TableOddRowBackgroundColor)
                        .Padding(5);
                    
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
                    
                    ProcessBlock(cell, container);

                    colIdx++;
                }
                
                if (row.IsHeader) _textProperties.TextStyles.Pop();
                
                rowIdx++;
            }
        });

        return pdf;
    }

    /// <summary>
    /// Processes a LeafBlock. Blocks contain container inlines (ContainerInline) or regular inline elements (LeafInline).
    /// Leaf blocks are represented by a QuestPDF text element with a text span for each child element
    /// </summary>
    private void ProcessLeafBlock(LeafBlock block, IContainer pdf)
    {
        if(_options.Debug) pdf = pdf.PaddedDebugArea(block.GetType().Name, Colors.Red.Medium);
        
        // Push any styles that should be applied to the entire block on the stack
        switch (block)
        {
            case HeadingBlock heading:
                _textProperties.TextStyles.Push(t => t.FontSize(Math.Max(0, _options.CalculateHeadingSize(heading.Level))).Bold());
                break;
        }

        if (block.Inline != null && block.Inline.Any())
        {
            pdf.Text(text =>
            {
                // Process the block's inline elements
                foreach (var item in block.Inline)
                {
                    switch (item)
                    {
                        case ContainerInline container:
                            ProcessContainerInline(container, text);
                            break;
                        case LeafInline leaf:
                            ProcessLeafInline(leaf, text);
                            break;
                    }
                }
            });
        }
        else if (block is ThematicBreakBlock)
        {
            pdf.LineHorizontal(_options.HorizontalRuleThickness)
                .LineColor(_options.HorizontalRuleColor);
        }
        else if (block is CodeBlock code)
        {
            pdf.Background(_options.CodeBlockBackground)
                .Padding(5)
                .Text(code.Lines.ToString())
                .FontFamily(_options.CodeFont);
        }

        
        // Pop any styles that were applied to the entire block off the stack
        switch (block)
        {
            case HeadingBlock:
                _textProperties.TextStyles.Pop();
                break;
        }
    }

    /// <summary>
    /// Processes a ContainerInline. A container inline contains other container inlines or regular inline elements (LeafInline) that are part of the same span of text.
    /// This method receives an existing PDF text element and adds text span elements to it for each child item
    /// </summary>
    private void ProcessContainerInline(ContainerInline inline, TextDescriptor text)
    {
        foreach (var item in inline)
        {
            // Push any styles that should be applied to the entire span on the stack
            switch (inline)
            {
                case LinkInline link:
                    _textProperties.TextStyles.Push(t => t
                        .FontColor(_options.LinkTextColor)
                        .DecorationColor(_options.LinkTextColor)
                        .Underline()
                    );
                    _textProperties.LinkUrl = link.Url;
                    _textProperties.IsImage = link.IsImage;
                    break;
                case EmphasisInline emphasis:
                    _textProperties.TextStyles.Push(t =>
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
                                return t.BackgroundColor(_options.MarkedTextBackgroundColor);
                        }
                        return emphasis.DelimiterCount == 2 ? t.Bold() : t.Italic();
                    });
                    break;
            }

            switch (item)
            {
                case ContainerInline container:
                    ProcessContainerInline(container, text);
                    break;
                case LeafInline leaf:
                    ProcessLeafInline(leaf, text);
                    break;
            }

            // Pop any styles that were applied to the entire span off the stack
            switch (inline)
            {
                case EmphasisInline:
                case LinkInline:
                    _textProperties.TextStyles.Pop();
                    break;
            }

            // Reset the link URL
            _textProperties.LinkUrl = null;
            _textProperties.IsImage = false;
        }
    }

    /// <summary>
    /// Processes a LeafInline. Regular inline elements (LeafInline) contain plain text.
    /// This method receives an existing PDF text element and adds a text span containing the plain text to it.
    /// </summary>
    private void ProcessLeafInline(LeafInline inline, TextDescriptor text)
    {
        switch (inline)
        {
            case AutolinkInline autoLink:
                var linkSpan = text.Hyperlink(autoLink.Url, autoLink.Url);
                linkSpan.ApplyStyles(_textProperties.TextStyles.ToList());
                break;
            case LineBreakInline lineBreak:
                // Only add a line break within a paragraph if trailing spaces or a backslash are used.
                if(lineBreak.IsBackslash || lineBreak.IsHard) text.Span("\n");
                break;
            case TaskList task: 
                text.Span(task.Checked ? _options.TaskListCheckedGlyph : _options.TaskListUncheckedGlyph)
                    .FontFamily(_options.UnicodeGlyphFont);
                break;
            case LiteralInline literal:
                ProcessLiteralInline(literal, text)
                    .ApplyStyles(_textProperties.TextStyles.ToList());
                break;
            case CodeInline code:
                text.Span(code.Content)
                    .BackgroundColor(_options.CodeInlineBackground)
                    .FontFamily(_options.CodeFont);
                break;
            case HtmlEntityInline htmlEntity:
                text.Span(htmlEntity.Transcoded.ToString());
                break;
            default:
                text.Span($"Unknown LeafInline: {inline.GetType()}").BackgroundColor(Colors.Orange.Medium);
                break;
        }
    }

    private TextSpanDescriptor ProcessLiteralInline(LiteralInline literal, TextDescriptor text)
    {
        // Plain text
        if (string.IsNullOrEmpty(_textProperties.LinkUrl)) return text.Span(literal.ToString());

        // Regular links, or images that could not be downloaded
        if (!_textProperties.IsImage || !_document.TryGetImageFromCache(_textProperties.LinkUrl, out var image))
            return text.Hyperlink(literal.ToString(), _textProperties.LinkUrl);

        // Images
        text.Element(e => e
            .Width(image.Width * _options.ImageScalingFactor)
            .Height(image.Height * _options.ImageScalingFactor)
            .Image(image.Image)
            .FitArea()
        );
 
        return text.Span(string.Empty);
    }
    
    internal static MarkdownRenderer Create(string markdownText, MarkdownRendererOptions? options = null) =>
        new(ParsedMarkdownDocument.FromText(markdownText), options);
    
    internal static MarkdownRenderer Create(ParsedMarkdownDocument document, MarkdownRendererOptions? options = null) =>
        new(document, options);
}