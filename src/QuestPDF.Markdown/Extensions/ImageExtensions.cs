using System.Reflection;
using QuestPDF.Infrastructure;

namespace QuestPDF.Markdown.Extensions;

internal static class ImageExtensions
{
    public static ImageSize GetSize(this Image image)
    {
        // QuestPDF does not expose image size as public
        // Use reflection until https://github.com/QuestPDF/QuestPDF/pull/1341 has been merged
        // return image.Size;
        var property = image
            .GetType()
            .GetProperty("Size", BindingFlags.Instance | BindingFlags.NonPublic);

        if (property == null || property.GetValue(image) is not ImageSize size)
            return new ImageSize(0, 0);

        return size;
    }
}
