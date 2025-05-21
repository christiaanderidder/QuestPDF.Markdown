using System.Reflection;
using QuestPDF.Companion;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Xunit;

namespace QuestPDF.Markdown.Tests;

public sealed class RenderTests
{
    private readonly string _markdown;

    public RenderTests()
    {
        Settings.License = LicenseType.Community;
        Settings.EnableDebugging = true;
        var assembly = Assembly.GetExecutingAssembly();
        const string resourceName = "QuestPDF.Markdown.Tests.test.md";
        using var stream = assembly.GetManifestResourceStream(resourceName);
        using var reader = new StreamReader(stream!);
        _markdown = reader.ReadToEnd();
    }

    [Fact]
    public async Task RenderToFile()
    {
        var markdown = ParsedMarkdownDocument.FromText(_markdown);
        await markdown.DownloadImages();

        var document = GenerateDocument(item => item.Markdown(markdown));
        document.GeneratePdf(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "test.pdf"));
    }
    
    [Fact]
    public async Task RenderImageLink()
    {
        var markdown = ParsedMarkdownDocument.FromText("""
                                                       Images can also be links:

                                                       [![200x200 image](https://placehold.co/200.jpg)](https://example.com)
                                                       """);
        
        await markdown.DownloadImages();
        
        var document = GenerateDocument(item => item.Markdown(markdown));

        try
        {
            await document.ShowInCompanionAsync(cancellationToken: TestContext.Current.CancellationToken);
        }
        catch (OperationCanceledException)
        {
            // Ignore
        }
    }

    [Fact]
    public async Task Render()
    {
        var markdown = ParsedMarkdownDocument.FromText(_markdown);
        await markdown.DownloadImages();

        var document = GenerateDocument(item => item.Markdown(markdown));

        try
        {
            await document.ShowInCompanionAsync(cancellationToken: TestContext.Current.CancellationToken);
        }
        catch (OperationCanceledException)
        {
            // Ignore
        }
    }

    [Fact]
    public async Task RenderDebug()
    {
        var markdown = ParsedMarkdownDocument.FromText(_markdown);
        await markdown.DownloadImages();

        var document = GenerateDocument(item => item.Markdown(markdown, options => options.Debug = true));

        try
        {
            await document.ShowInCompanionAsync(cancellationToken: TestContext.Current.CancellationToken);
        }
        catch (OperationCanceledException)
        {
            // Ignore
        }
    }

    [Fact]
    public async Task RenderTag()
    {
        var document = GenerateDocument(item => item.Markdown("This is page **{currentPage}** / *{totalPages}*.",
            options =>
            {
                options.AddTemplateTag("currentPage", t => t.CurrentPageNumber());
                options.AddTemplateTag("totalPages", t => t.TotalPages());
            }));

        try
        {
            await document.ShowInCompanionAsync(cancellationToken: TestContext.Current.CancellationToken);
        }
        catch (OperationCanceledException)
        {
            // Ignore
        }
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
                    text.FontFamily(Fonts.Arial);
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
            Author = "QuestPDF.Markdown",
            Title = "QuestPDF.Markdown",
            Subject = "QuestPDF.Markdown",
            Keywords = "questpdf, markdown, pdf",
            CreationDate = new DateTime(2023, 11, 15, 12, 00, 00),
            ModifiedDate = new DateTime(2023, 11, 15, 12, 00, 00),
        });
    }
}