using Markdig.Extensions.Tables;
using Markdig.Extensions.TaskLists;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using QuestPDF.Markdown.Extensions;
using QuestPDF.Markdown.Compatibility;
using QuestPDF.Markdown.Parsing;

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
    private readonly TextProperties _textProperties = new();

    private MarkdownRenderer(ParsedMarkdownDocument document, MarkdownRendererOptions? options)
    {
        _document = document;
        _options = options ?? new MarkdownRendererOptions();
    }
    
    internal static MarkdownRenderer Create(string markdownText, MarkdownRendererOptions? options = null) =>
        new(ParsedMarkdownDocument.FromText(markdownText), options);

    internal static MarkdownRenderer Create(ParsedMarkdownDocument document, MarkdownRendererOptions? options = null) =>
        new(document, options);

    public void Compose(IContainer pdf) => Render(_document.MarkdigDocument, pdf);
    
    private void Render(Block block, IContainer pdf)
    {
        if(_options.Debug) pdf = pdf.PaddedDebugArea(block.GetType().Name, block is LeafBlock ? Colors.Red.Medium : Colors.Blue.Medium);
        
        _ = block switch
        {
            QuoteBlock quoteBlock => Render(quoteBlock, pdf),
            Table table => Render(table, pdf),
            ListBlock listBlock => Render(listBlock, pdf),
            ListItemBlock listItemBlock => Render(listItemBlock, pdf),
            HeadingBlock headingBlock => Render(headingBlock, pdf),
            ThematicBreakBlock thematicBreakBlock => Render(thematicBreakBlock, pdf),
            CodeBlock codeBlock => Render(codeBlock, pdf),
            TableRow tableRow => Render(tableRow, pdf),
            TableCell tableCell => Render(tableCell, pdf),
            ContainerBlock containerBlock => Render(containerBlock, pdf),
            LeafBlock leafBlock => Render(leafBlock, pdf),
            _ => throw new InvalidOperationException($"Unsupported block type {block.GetType().Name}")
        };
    }

    private IContainer Render(ContainerBlock block, IContainer pdf)
    {
        if (block.Count == 0) return pdf;

        pdf.Column(col =>
        {
            foreach (var item in block)
            {
                // Blocks inside a list get the same spacing as the list items themselves
                col.Spacing(item.Parent is ListBlock or ListItemBlock
                    ? _options.ListItemSpacing
                    : _options.ParagraphSpacing);

                Render(item, col.Item());
            }
        });

        return pdf;
    }

    private IContainer Render(Table table, IContainer pdf)
    {
        pdf.Table(td =>
        {
            td.ColumnsDefinition(cd =>
            {
                foreach (var col in table.ColumnDefinitions)
                {
                    // Widths are provided as a percentage
                    cd.RelativeColumn(col.Width > 0 ? col.Width : 1f);
                }
            });
            
            var rows = table.OfType<TableRow>().ToList();
            RenderTableRows(table, rows, td);
        });

        return pdf;
    }

    private void RenderTableRows(Table table, List<TableRow> rows, TableDescriptor td)
    {
        uint rowIdx = 0;
        foreach (var row in rows)
        {
            if (row.IsHeader) _textProperties.TextStyles.Push(t => t.Bold());
            var isLast = rowIdx + 1 == table.Count;
        
            var cells = row.OfType<TableCell>().ToList();
            RenderTableCells(table, td, cells, rowIdx, row, isLast);

            if (row.IsHeader) _textProperties.TextStyles.Pop();

            rowIdx++;
        }
    }

    private void RenderTableCells(Table table, TableDescriptor td, List<TableCell> cells, uint rowIdx, TableRow row, bool isLast)
    {
        uint columnIdx = 0;
        foreach (var cell in cells)
        {
            var cd = table.ColumnDefinitions[(int)columnIdx];
            RenderTableCell(cell, rowIdx, columnIdx, row.IsHeader, isLast, td, cd);
            
            columnIdx++;
        }
    }
    
    private IContainer RenderTableCell(TableCell cell, uint rowIdx, uint columnIdx, bool isHeader, bool isLast, TableDescriptor table, TableColumnDefinition columnDefinition)
    {
        var container = table.Cell()
            .RowSpan((uint)cell.RowSpan)
            .Row(rowIdx + 1)
            .Column((cell.ColumnIndex >= 0 ? (uint)cell.ColumnIndex : columnIdx) + 1)
            .ColumnSpan((uint)cell.ColumnSpan)
            .Border(_options, isHeader, isLast)
            .Background(rowIdx % 2 == 1
                ? _options.TableEvenRowBackgroundColor
                : _options.TableOddRowBackgroundColor)
            .Padding(5);
        
        switch (columnDefinition.Alignment)
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
        
        return Render(cell, container);
    }

    private IContainer Render(LeafBlock block, IContainer pdf)
    {
        // Some blocks don't contain any further inline elements, render the text directly
        if (block.Inline != null && block.Inline.Any())
        {
            pdf.Text(text =>
            {
                text.Align(_options.ParagraphAlignment);

                // Process the block's inline elements
                foreach (var item in block.Inline)
                {
                    Render(item, text);
                }
            });
            
        }
        else if (block.Lines.Count != 0)
        {
            pdf.Text(block.Lines.ToString())
                .ApplyStyles(_textProperties.TextStyles.ToList());
        }
        
        return pdf;
    }
    
    private TextDescriptor Render(ContainerInline inline, TextDescriptor text)
    {
        foreach (var item in inline)
        {
            Render(item, text);
        }

        return text;
    }

    private IContainer Render(QuoteBlock block, IContainer pdf)
    {
        pdf = pdf.BorderLeft(_options.BlockQuoteBorderThickness)
            .BorderColor(_options.BlockQuoteBorderColor)
            .PaddingLeft(10);

        // Push any styles that should be applied to the entire container on the stack
        _textProperties.TextStyles.Push(t => t.FontColor(_options.BlockQuoteTextColor));

        Render(block as ContainerBlock, pdf);

        // Pop any styles that were applied to the entire container off the stack
        _textProperties.TextStyles.Pop();

        return pdf;
    }

    private IContainer Render(ListBlock block, IContainer pdf)
    {
        return Render(block as ContainerBlock, pdf);
    }

    private IContainer Render(ListItemBlock block, IContainer pdf)
    {
        if (block.Parent is not ListBlock list) return pdf;

        pdf.Row(li =>
        {
            li.Spacing(5);
            li.AutoItem().PaddingLeft(10)
                .Text(list.IsOrdered ? $"{block.Order}{list.OrderedDelimiter}" : _options.UnorderedListGlyph);

            Render(block as ContainerBlock, li.RelativeItem());
        });

        return pdf;
    }

    private IContainer Render(HeadingBlock block, IContainer pdf)
    {
        // Push any styles that should be applied to the entire block on the stack
        _textProperties.TextStyles.Push(t =>
            t.FontSize(Math.Max(0, _options.CalculateHeadingSize(block.Level))).Bold());

        Render(block as LeafBlock, pdf);

        // Pop any styles that were applied to the entire block off the stack
        _textProperties.TextStyles.Pop();

        return pdf;
    }

    private IContainer Render(ThematicBreakBlock block, IContainer pdf)
    {
        pdf.LineHorizontal(_options.HorizontalRuleThickness)
            .LineColor(_options.HorizontalRuleColor);

        return pdf;
    }

    private IContainer Render(CodeBlock block, IContainer pdf)
    {
        // Push any styles that should be applied to the entire block on the stack
        _textProperties.TextStyles.Push(t => t.FontFamily(_options.CodeFont));
        
        pdf = pdf.Background(_options.CodeBlockBackground).Padding(5);
        pdf = Render(block as LeafBlock, pdf);
        
        // Pop any styles that were applied to the entire block off the stack
        _textProperties.TextStyles.Pop();

        return pdf;
    }
    
    private TextDescriptor Render(Inline inline, TextDescriptor text) => inline switch
    {
        TemplateInline templateInline => Render(templateInline, text),
        LinkInline linkInline => Render(linkInline, text),
        EmphasisInline emphasisInline => Render(emphasisInline, text),
        AutolinkInline autolinkInline => Render(autolinkInline, text),
        LineBreakInline lineBreakInline => Render(lineBreakInline, text),
        TaskList taskList => Render(taskList, text),
        LiteralInline literalInline => Render(literalInline, text),
        CodeInline codeInline => Render(codeInline, text),
        HtmlEntityInline htmlEntityInline => Render(htmlEntityInline, text),
        ContainerInline containerInline => Render(containerInline, text),
        LeafInline leafInline => Render(leafInline, text),
        _ => throw new InvalidOperationException($"Unsupported inline type {inline.GetType().Name}")
    };

    private static TextDescriptor Render(LeafInline inline, TextDescriptor text)
    {
        text.Span($"Unknown LeafInline: {inline.GetType()}").BackgroundColor(Colors.Orange.Medium);

        return text;
    }

    private TextDescriptor Render(TemplateInline inline, TextDescriptor text)
    {
        if (!_options.RenderTemplates.TryGetValue(inline.Tag, out var render) || render == null) return text;

        render(text);
        return text;
    }

    private TextDescriptor Render(LinkInline inline, TextDescriptor text)
    {
        // Push any styles that should be applied to the entire span on the stack
        _textProperties.TextStyles.Push(t => t
            .FontColor(_options.LinkTextColor)
            .DecorationColor(_options.LinkTextColor)
            .Underline()
        );
        _textProperties.LinkUrl = inline.Url;
        _textProperties.IsImage = inline.IsImage;

        Render(inline as ContainerInline, text);

        // Pop any styles that were applied to the entire span off the stack
        _textProperties.TextStyles.Pop();
        _textProperties.LinkUrl = null;
        _textProperties.IsImage = false;

        return text;
    }

    private TextDescriptor Render(EmphasisInline inline, TextDescriptor text)
    {
        _textProperties.TextStyles.Push(t =>
        {
            switch (inline.DelimiterChar, inline.DelimiterCount)
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

            return inline.DelimiterCount == 2 ? t.Bold() : t.Italic();
        });

        Render(inline as ContainerInline, text);

        _textProperties.TextStyles.Pop();
        
        return text;
    }

    private TextDescriptor Render(AutolinkInline inline, TextDescriptor text)
    {
        var linkSpan = text.Hyperlink(inline.Url, inline.Url);
        linkSpan.ApplyStyles(_textProperties.TextStyles.ToList());
        
        return text;
    }

    private static TextDescriptor Render(LineBreakInline inline, TextDescriptor text)
    {
        // Only add a line break within a paragraph if trailing spaces or a backslash are used.
        if (inline.IsBackslash || inline.IsHard) text.Span("\n");
        else text.Span(" ");
        
        return text;
    }

    private TextDescriptor Render(TaskList inline, TextDescriptor text)
    {
        text.Span(inline.Checked ? _options.TaskListCheckedGlyph : _options.TaskListUncheckedGlyph)
            .FontFamily(_options.UnicodeGlyphFont);

        return text;
    }

    private TextDescriptor Render(LiteralInline inline, TextDescriptor text)
    {
        // Plain text
        if (_textProperties.LinkUrl.IsNullOrEmpty())
        {
            text.Span(inline.ToString()).ApplyStyles(_textProperties.TextStyles.ToList());
            return text;
        }

        // Regular links, or images that could not be downloaded
        if (!_textProperties.IsImage || !_document.TryGetImageFromCache(_textProperties.LinkUrl, out var image))
        {
            text.Hyperlink(inline.ToString(), _textProperties.LinkUrl).ApplyStyles(_textProperties.TextStyles.ToList());
            return text;
        }

        // Images
        text.Element(e => e
            .Width(image.Width * _options.ImageScalingFactor)
            .Height(image.Height * _options.ImageScalingFactor)
            .Image(image.Image)
            .FitArea()
        );

        text.Span(string.Empty).ApplyStyles(_textProperties.TextStyles.ToList());

        return text;
    }

    private TextDescriptor Render(CodeInline inline, TextDescriptor text)
    {
        text.Span(inline.Content)
            .BackgroundColor(_options.CodeInlineBackground)
            .FontFamily(_options.CodeFont);

        return text;
    }

    private static TextDescriptor Render(HtmlEntityInline inline, TextDescriptor text)
    {
        text.Span(inline.Transcoded.ToString());
        
        return text;
    }
}