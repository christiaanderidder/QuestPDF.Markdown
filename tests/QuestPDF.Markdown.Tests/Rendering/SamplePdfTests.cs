using System.Reflection;
using QuestPDF.Companion;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace QuestPDF.Markdown.Tests.Rendering;

public class SamplePdfTests
{
    private readonly string _markdown;

    public SamplePdfTests()
    {
        var assembly = Assembly.GetExecutingAssembly();
        const string resourceName = "QuestPDF.Markdown.Tests.test.md";
        using var stream = assembly.GetManifestResourceStream(resourceName);
        using var reader = new StreamReader(stream!);
        _markdown = reader.ReadToEnd();
    }

    [Fact(Skip = "This test is not run in CI")]
    public async Task SaveToFile()
    {
        var markdown = ParsedMarkdownDocument.FromText(_markdown);
        await markdown.DownloadImages();

        var document = GenerateDocument(item => item.Markdown(markdown));
        document.GeneratePdf(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "test.pdf"));
    }

    [Fact(Skip = "This test is not run in CI")]
    public async Task ShowInCompanion()
    {
        var markdown = ParsedMarkdownDocument.FromText(_markdown);
        await markdown.DownloadImages();

        var document = GenerateDocument(item => item.Markdown(markdown));

        try
        {
            await document.ShowInCompanionAsync(
                cancellationToken: TestContext.Current.CancellationToken
            );
        }
        catch (OperationCanceledException)
        {
            // Ignore
        }
    }

    private static Document GenerateDocument(Action<IContainer> body)
    {
        return Document
            .Create(container =>
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
            })
            .WithMetadata(
                new DocumentMetadata
                {
                    Author = "QuestPDF.Markdown",
                    Title = "QuestPDF.Markdown",
                    Subject = "QuestPDF.Markdown",
                    Keywords = "questpdf, markdown, pdf",
                    CreationDate = new DateTime(2023, 11, 15, 12, 00, 00),
                    ModifiedDate = new DateTime(2023, 11, 15, 12, 00, 00),
                }
            );
    }
}
