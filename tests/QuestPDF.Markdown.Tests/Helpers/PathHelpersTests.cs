using QuestPDF.Markdown.Helpers;

namespace QuestPDF.Markdown.Tests.Helpers;

internal sealed class PathHelpersTests
{
    [Test]
    [Arguments("./image.png", "/safe/root", "/safe/root/image.png", true)]
    [Arguments("image.png", "/safe/root", "/safe/root/image.png", true)]
    [Arguments("subdir/../image.png", "/safe/root", "/safe/root/image.png", true)]
    [Arguments("subdir/image.png", "/safe/root", "/safe/root/subdir/image.png", true)]
    [Arguments("../image.png", "/safe/root", "", false)]
    [Arguments("/absolute/path/image.png", "/safe/root", "", false)]
    [Arguments("invaid\0.png", "/safe/root", "", false)]
    public async Task ResolvesSafeLocalPath(
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

        await Assert.That(safeImagePath).IsEqualTo(expectedSafeImagePath);
        await Assert.That(result).IsEqualTo(expectedResult);
    }
}
