using System.Diagnostics.CodeAnalysis;

namespace QuestPDF.Markdown.Compatibility;

public static class CompatibilityShims
{
    public static HashSet<T> ToHashSetShim<T>(this IEnumerable<T> source) => [..source];
    public static bool IsNullOrEmpty([NotNullWhen(false)] this string? data) => string.IsNullOrEmpty(data);
    public static Task<byte[]> ReadAllBytesAsync(string path)
    {
#if NETSTANDARD2_0
        return Task.FromResult(File.ReadAllBytes(path));
#else
        return File.ReadAllBytesAsync(path);
#endif
    }
}