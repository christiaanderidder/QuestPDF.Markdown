using QuestPDF.Markdown.Helpers;

namespace QuestPDF.Markdown.Tests.Helpers;

public class PathHelpersTests
{
    [Theory]
    [InlineData("./image.png", "/safe/root", "/safe/root/image.png", true)]
    [InlineData("image.png", "/safe/root", "/safe/root/image.png", true)]
    [InlineData("subdir/../image.png", "/safe/root", "/safe/root/image.png", true)]
    [InlineData("subdir/image.png", "/safe/root", "/safe/root/subdir/image.png", true)]
    [InlineData("../image.png", "/safe/root", "", false)]
    [InlineData("/absolute/path/image.png", "/safe/root", "", false)]
    [InlineData("invaid\0.png", "/safe/root", "", false)]
    public void ResolvesSafeLocalPath(
        string imagePath,
        string safeRootPath,
        string expectedSafeImagePath,
        bool expectedResult
    )
    {
        var result = PathHelpers.TryResolveSafeLocalPath(
            imagePath,
            safeRootPath,
            out var safeImagePath
        );

        Assert.Equal(expectedSafeImagePath, safeImagePath);
        Assert.Equal(expectedResult, result);
    }
}
