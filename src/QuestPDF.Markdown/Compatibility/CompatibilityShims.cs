using System.Diagnostics.CodeAnalysis;

namespace QuestPDF.Markdown.Compatibility;

public static class CompatibilityShims
{
    public static bool EndsWith(this string source, char value)
    {
        ExceptionHelper.ThrowIfNull(source);
#if NETSTANDARD2_0
        return source.EndsWith(value.ToString(), StringComparison.Ordinal);
#else
        return source.EndsWith(value);
#endif
    }
    
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