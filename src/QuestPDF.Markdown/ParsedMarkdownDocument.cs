using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using Markdig;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using QuestPDF.Infrastructure;
using SkiaSharp;

namespace QuestPDF.Markdown;

/// <summary>
/// Represents a parsed markdown text.
/// This helper is used to download external images before rendering them.
/// </summary>
public class ParsedMarkdownDocument
{
    private readonly MarkdownDocument _document;
    private readonly ConcurrentDictionary<string, ImageWithDimensions> _imageCache = new();

    private static readonly HttpClient HttpClient = new();

    private ParsedMarkdownDocument(string markdownText)
    {
        var pipeline = new MarkdownPipelineBuilder()
            .DisableHtml()
            .UseEmphasisExtras()
            .UseGridTables()
            .UsePipeTables()
            .UseTaskLists()
            .UseAutoLinks()
            .Build();
        
        _document = Markdig.Markdown.Parse(markdownText, pipeline);
    }

    /// <summary>
    /// Downloads all external images from the parsed markdown document and caches them for rendering
    /// </summary>
    /// <param name="imageDownloaderMaxParallelism">Optionally provide the maximum number of request to execute in parallel</param>
    /// <param name="httpClient">Optionally provide your own HttpClient instance used to download images</param>
    public async Task DownloadImages(int imageDownloaderMaxParallelism = 4, HttpClient? httpClient = null)
    {
        var parallelism = Math.Max(1, imageDownloaderMaxParallelism);
        var semaphore = new SemaphoreSlim(parallelism);
        
        var urls = _document.Descendants<LinkInline>()
            .Where(l => l.IsImage && l.Url != null && Uri.IsWellFormedUriString(l.Url, UriKind.Absolute))
            .Select(l => l.Url)
            .ToHashSet();

        // The semaphore is disposed after all tasks have completed, we can safely disable AccessToDisposedClosure
        var tasks = urls.Select([SuppressMessage("ReSharper", "AccessToDisposedClosure")] async (url) =>
        {
            if (url == null) return;

            try
            {
                if(url.StartsWith("data:image"))
                {
                    // ![base64_image](data:image/png;base64,..{{base64 string}}..)
                    var base64 = test.Split(",")[1];
                    var data = Convert.FromBase64String(base64);
                    using var skImage = SKImage.FromEncodedData(data);
                    var pdfImage = Image.FromBinaryData(data);

                    var image = new ImageWithDimensions(skImage.Width, skImage.Height, pdfImage);
                    _imageCache.TryAdd(url, image);

                    return;
                }
            }
            finally
            {
                return;
            }
            
            
            await semaphore.WaitAsync().ConfigureAwait(false);
            
            try
            {
                var client = httpClient ?? HttpClient;
                var stream = await client.GetStreamAsync(url).ConfigureAwait(false);
                
                // QuestPDF does not allow accessing image dimensions on loaded images
                // To work around this we will parse the image ourselves first and keep track of the dimensions
                using var skImage = SKImage.FromEncodedData(stream);
                var pdfImage = Image.FromBinaryData(skImage.EncodedData.ToArray());
                
                var image = new ImageWithDimensions(skImage.Width, skImage.Height, pdfImage);
                _imageCache.TryAdd(url, image);
            }
            finally
            {
                semaphore.Release();
            }
        });

        await Task.WhenAll(tasks).ConfigureAwait(false);

        // Dispose semaphore after completing all tasks
        semaphore.Dispose();
    }

    internal MarkdownDocument MarkdigDocument => _document;
    
    internal bool TryGetImageFromCache(string url, out ImageWithDimensions image) => _imageCache.TryGetValue(url, out image);
    
    /// <summary>
    /// Parses the provided markdown text into a ParsedMarkdownDocument instance
    /// </summary>
    /// <param name="markdownText">The markdown text</param>
    /// <returns>An instance of ParsesMarkdownDocument</returns>
    public static ParsedMarkdownDocument FromText(string markdownText) => new(markdownText);
}
