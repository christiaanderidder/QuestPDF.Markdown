using Markdig;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using QuestPDF.Infrastructure;
using QuestPDF.Markdown.Compatibility;
using QuestPDF.Markdown.Parsing;
using SkiaSharp;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace QuestPDF.Markdown;

/// <summary>
/// Represents a parsed markdown text.
/// This helper is used to download external images before rendering them.
/// </summary>
public class ParsedMarkdownDocument
{
    private readonly MarkdownDocument _document;
    private readonly string? _documentPath;
    private readonly ConcurrentDictionary<string, ImageWithDimensions> _imageCache = new();
    private static readonly Regex DataUri = new(@"data:image\/.+?;base64,(?<data>.+)", RegexOptions.Compiled, TimeSpan.FromMilliseconds(100));

    private static readonly HttpClient HttpClient = new();

    private ParsedMarkdownDocument(string markdownText, string? documentPath)
    {
        var pipeline = new MarkdownPipelineBuilder()
            .DisableHtml()
            .UseEmphasisExtras()
            .UseGridTables()
            .UsePipeTables()
            .UseTaskLists()
            .UseAutoLinks()
            .Use<TemplateExtension>()
            .Build();

        _document = Markdig.Markdown.Parse(markdownText, pipeline);
        _documentPath = documentPath;

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
            .Where(l => l.IsImage && l.Url != null)
            .Select(l => l.Url)
            .ToHashSetShim();

        // The semaphore is disposed after all tasks have completed, we can safely disable AccessToDisposedClosure
        var tasks = urls.Select([SuppressMessage("ReSharper", "AccessToDisposedClosure")] async (url) =>
        {
            if (url == null) return;

            await semaphore.WaitAsync().ConfigureAwait(false);

            try
            {
                var (success, imageData) = await GetImageData(httpClient, url).ConfigureAwait(false);
                if (!success) return;

                // QuestPDF does not allow accessing image dimensions on loaded images
                // To work around this we will parse the image ourselves first and keep track of the dimensions
                using var skImage = SKImage.FromEncodedData(imageData);
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

    private async Task<(bool Success, byte[] ImageData)> GetImageData(HttpClient? httpClient, string url)
    {
        // Check for Base64 data URI first, as this might exceed Uri.
        var base64Match = DataUri.Match(url);
        if (base64Match.Success && base64Match.Groups["data"].Success)
            return (true, Convert.FromBase64String(base64Match.Groups["data"].Value));

        if (Uri.TryCreate(url, UriKind.Absolute, out var uri))
            return (true, await DownloadImage(httpClient, uri).ConfigureAwait(false));

        if (_documentPath is not null && !Path.IsPathRooted(url))
            url = Path.GetFullPath(url, Path.GetDirectoryName(_documentPath) ?? "");
        if (File.Exists(url))
        {
            using var stream = File.OpenRead(url);
            var result = new byte[stream.Length];
            await stream.ReadAsync(result, 0, (int)stream.Length).ConfigureAwait(false);
            return (true, result);
        }

        return (false, []);
    }

    private static async Task<byte[]> DownloadImage(HttpClient? httpClient, Uri uri)
    {
        var client = httpClient ?? HttpClient;
        var stream = await client.GetStreamAsync(uri).ConfigureAwait(false);

        using var ms = new MemoryStream();
        await stream.CopyToAsync(ms).ConfigureAwait(false);
        return ms.ToArray();
    }

    internal MarkdownDocument MarkdigDocument => _document;

    internal bool TryGetImageFromCache(string url, [MaybeNullWhen(false)] out ImageWithDimensions image)
    {
        return _imageCache.TryGetValue(url, out image);
    }

    /// <summary>
    /// Parses the provided markdown text into a ParsedMarkdownDocument instance
    /// </summary>
    /// <param name="markdownText">The markdown text</param>
    /// <returns>An instance of ParsesMarkdownDocument</returns>
    public static ParsedMarkdownDocument FromText(string markdownText, string? documentPath) => new(markdownText, documentPath);

    public static ParsedMarkdownDocument FromFile(string filePath) => new(File.ReadAllText(filePath), filePath);
}
