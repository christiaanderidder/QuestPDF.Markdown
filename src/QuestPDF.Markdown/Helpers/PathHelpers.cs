using System.Runtime.InteropServices;
using QuestPDF.Markdown.Compatibility;

namespace QuestPDF.Markdown.Helpers;

internal static class PathHelpers
{
    public static bool TryResolveSafeLocalPath(
        string imagePath,
        string safeRootPath,
        out string safeImagePath
    )
    {
        safeImagePath = string.Empty;

        // Check for invalid path characters
        if (imagePath.IndexOfAny(Path.GetInvalidPathChars()) != -1)
            return false;

        // Ensure safe root path is absolute and normalized
        safeRootPath = NormalizePath(safeRootPath);

        // Make sure that a trailing separator is present
        if (!safeRootPath.EndsWith(Path.DirectorySeparatorChar))
            safeRootPath += Path.DirectorySeparatorChar;

        // Resolve absolute image path relative to safe root
        imagePath = NormalizePath(Path.Combine(safeRootPath, imagePath));

        // Windows paths are case-insensitive
        var pathComparison = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            ? StringComparison.OrdinalIgnoreCase
            : StringComparison.Ordinal;

        // Ensure the combined path is within the safe root
        if (!imagePath.StartsWith(safeRootPath, pathComparison))
            return false;

        safeImagePath = imagePath;
        return true;
    }

    private static string NormalizePath(string path) =>
        Path.GetFullPath(path).Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
}
