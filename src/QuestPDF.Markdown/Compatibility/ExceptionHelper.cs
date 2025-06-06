using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace QuestPDF.Markdown.Compatibility;

internal static class ExceptionHelper
{
    public static void ThrowIfNull([NotNull] object? argument, [CallerArgumentExpression(nameof(argument))] string? paramName = null)
    {
        if (argument is null) throw new ArgumentNullException(paramName);
    }
}