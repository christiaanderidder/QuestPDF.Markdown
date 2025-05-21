using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace QuestPDF.Markdown.Tests.Rendering;

public sealed class RenderTests
{
    [Fact]
    public async Task RendersTags()
    {
        var document = GenerateDocument(item => item.Markdown("This is page [**{currentPage}**](https://example.com) / *{totalPages}*.",
            options =>
            {
                options.AddTemplateTag("currentPage", t => t.CurrentPageNumber());
                options.AddTemplateTag("totalPages", t => t.TotalPages());
            }));

        await Verify(document);
    }
    
    [Fact]
    public async Task RendersHeadings()
    {
        const string md = """
                          # h1 Heading
                          ## h2 Heading
                          ### h3 Heading
                          #### h4 Heading
                          ##### h5 Heading
                          ###### h6 Heading
                          """;

        var document = GenerateDocument(item => item.Markdown(md));

        await Verify(document);
    }

    private static Document GenerateDocument(Action<IContainer> body)
    {
        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.PageColor(Colors.White);
                page.Size(PageSizes.A4);
                page.Margin(1, Unit.Centimetre);
                page.DefaultTextStyle(text =>
                {
                    text.FontFamily(Fonts.Lato);
                    text.LineHeight(1.5f);
                    return text;
                });

                page.Content()
                    .PaddingVertical(20)
                    .Column(main =>
                    {
                        main.Spacing(20);
                        body(main.Item());
                    });
            });
        }).WithMetadata(new DocumentMetadata
        {
            CreationDate = new DateTime(2023, 11, 15, 12, 00, 00),
            ModifiedDate = new DateTime(2023, 11, 15, 12, 00, 00),
        });
    }
}