using NUnit.Framework;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using QuestPDF.Previewer;

namespace QuestPDF.Markdown.Tests;

[Explicit("These tests are disabled for automated workflows because they open a QuestPDF previewer window")]
public class RenderTests
{
    private string _markdown = string.Empty;
    private readonly HttpClient _httpClient = new();
    
    [SetUp]
    public void Setup()
    {
        Settings.License = LicenseType.Community;
        _markdown = File.ReadAllText(Path.Combine(TestContext.CurrentContext.TestDirectory, "test.md"));
    }

    [Test]
    public async Task Render()
    {
        var config = new RenderConfig
        {
            Debug = false,
        };
        var document = GenerateDocument(item => item.Markdown(_markdown, config));
        
        try
        {
            await document.ShowInPreviewerAsync();    
        }
        catch(OperationCanceledException)
        {
            // Ignore
        }
    }
    
    [Test]
    public async Task RenderDebug()
    {
        var config = new RenderConfig
        {
            Debug = true,
        };
        var document = GenerateDocument(item => item.Markdown(_markdown, config));
        
        try
        {
            await document.ShowInPreviewerAsync();    
        }
        catch(OperationCanceledException)
        {
            // Ignore
        }
    }
    
    private Document GenerateDocument(Action<IContainer> body)
    {
        return Document.Create(container =>
        {
            container.Page(page =>
            {
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
        });
    }
}