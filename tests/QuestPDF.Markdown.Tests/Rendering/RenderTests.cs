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
                          A paragraph is followed by a blank line.
                          
                          Newlines within paragraphs
                           are ignored.
                          
                          Text should be followed by two trailing spaces   
                          or a backlash \
                          to force a newline.
                          
                          Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua.
                          """;
        
        var document = GenerateDocument(item => item.Markdown(md));

        await Verify(document);
    }
    
    [Fact]
    public async Task RedersEmphasis()
    {
        const string md = """
                          **This is bold text**
                          
                          __This is bold text__
                          
                          *This is italic text*
                          
                          _This is italic text_
                          
                          **This is bold and _italic_ text**
                          
                          ~~This is strikethrough text~~
                          """;

        var document = GenerateDocument(item => item.Markdown(md));

        await Verify(document);
    }

    [Fact]
    public async Task RendersExtendedEmphasis()
    {
        const string md = """
                          19^th^

                          H~2~O

                          ++Inserted text++

                          ==Marked text==
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