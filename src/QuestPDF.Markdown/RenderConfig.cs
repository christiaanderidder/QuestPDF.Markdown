namespace QuestPDF.Markdown;

public class RenderConfig
{
    public bool Debug { get; set; }
    public bool DownloadImages { get; set; }
    public Func<HttpClient>? HttpClientFactory { get; set; }
}