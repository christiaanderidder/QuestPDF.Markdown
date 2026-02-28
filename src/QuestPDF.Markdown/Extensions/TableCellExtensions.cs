using QuestPDF.Elements.Table;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace QuestPDF.Markdown.Extensions;

internal static class TableCellExtensions
{
    internal static IContainer Border(
        this ITableCellContainer cell,
        MarkdownRendererOptions options,
        bool isHeader,
        bool isLast
    ) =>
        options.TableBorderStyle switch
        {
            TableBorderStyle.None => cell,
            TableBorderStyle.Horizontal => cell.BorderBottom(
                    isLast
                        ? 0
                        : (
                            isHeader
                                ? options.TableHeaderBorderThickness
                                : options.TableBorderThickness
                        )
                )
                .BorderColor(options.TableBorderColor),
            TableBorderStyle.Full => cell.Border(options.TableBorderThickness)
                .BorderBottom(
                    isHeader ? options.TableHeaderBorderThickness : options.TableBorderThickness
                )
                .BorderColor(options.TableBorderColor),
            _ => throw new InvalidOperationException(),
        };
}
