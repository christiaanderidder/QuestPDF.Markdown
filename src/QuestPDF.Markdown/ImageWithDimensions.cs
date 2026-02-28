using QuestPDF.Infrastructure;

namespace QuestPDF.Markdown;

internal sealed class ImageWithDimensions
{
    internal int Width { get; }
    internal int Height { get; }
    internal Image Image { get; }

    internal ImageWithDimensions(int width, int height, Image image)
    {
        Width = width;
        Height = height;
        Image = image;
    }
}
