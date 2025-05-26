using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace QuestPDF.Markdown.Tests.Rendering;

public sealed class RenderTests
{
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
    
    [Fact]
    public async Task RendersAlternativeHeadings()
    {
        const string md = """
                          h1 heading
                          ======
                          h2 heading
                          ------
                          """;

        var document = GenerateDocument(item => item.Markdown(md));

        await Verify(document);
    }

    [Fact]
    public async Task RendersParagraphs()
    {
        const string md = """
                          Lorem ipsum dolor sit amet, consectetur adipiscing elit. Fusce tincidunt ullamcorper tellus quis rutrum. Mauris in nisl ornare, luctus mauris ut, scelerisque neque. Sed et lacinia dui. Pellentesque nec sagittis enim.
                          
                          Fusce egestas, tortor sit amet dictum malesuada, dui tortor venenatis tortor, et iaculis mi turpis in augue. Donec lobortis erat risus, sed viverra arcu aliquam vel.
                          
                          Nulla viverra, dui ut finibus feugiat, ante purus sagittis velit, eget dignissim erat mi at mi. Sed bibendum eu ipsum eget facilisis. Ut luctus lorem et tortor lacinia convallis. Nam malesuada ornare facilisis.
                          
                          Nullam scelerisque maximus pulvinar. Phasellus nec est venenatis, tincidunt ex vel, dictum nibh. Duis vel bibendum nibh. Praesent vehicula, tortor sit amet pharetra dapibus, tellus lacus lacinia mi, ac maximus metus ligula at sem.
                          """;
        
        var document = GenerateDocument(item => item.Markdown(md));

        await Verify(document);
    }
    
    [Fact]
    public async Task RendersLinks()
    {
        const string md = """
                          Inline style link: [link](https://example.com)
                          
                          Inline-style link with title: [link](https://example.com "example title")
                          
                          Reference-style link with text: [link][ref]

                          Reference-style link with number: [link][1]
                          
                          Reference-style link with link text: [link]
                          
                          Plaintext URL: https://example.com
                          
                          [ref]: https://example.com
                          [1]: https://example.com
                          [link]: https://example.com
                          """;

        var document = GenerateDocument(item => item.Markdown(md));

        await Verify(document);
    }
    
    [Fact]
    public async Task RendersTags()
    {
        const string md = """
                          This is page [**{currentPage}**](https://example.com) / *{totalPages}*.
                          """;
        
        var document = GenerateDocument(item => item.Markdown(md,
            options =>
            {
                options.AddTemplateTag("currentPage", t => t.CurrentPageNumber());
                options.AddTemplateTag("totalPages", t => t.TotalPages());
            }));

        await Verify(document);
    }

    private static Document GenerateDocument(Action<IContainer> body)
    {
        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.PageColor(Colors.White);
                page.Size(PageSizes.A6);
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