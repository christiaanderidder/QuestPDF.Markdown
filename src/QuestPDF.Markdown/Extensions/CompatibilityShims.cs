using System.Diagnostics.CodeAnalysis;

namespace QuestPDF.Markdown.Extensions
{
    public static class CompatibilityShims
    {
        public static HashSet<T> ToHashSetShim<T>(this IEnumerable<T> source) => [..source];
        public static bool IsNullOrEmpty([NotNullWhen(false)] this string? data) => string.IsNullOrEmpty(data);
    }
}

#if NETSTANDARD2_0
namespace System.Diagnostics.CodeAnalysis
{
    [AttributeUsage(AttributeTargets.Parameter)]
    public sealed class NotNullWhenAttribute : Attribute
    {
        public NotNullWhenAttribute(bool returnValue) => ReturnValue = returnValue;
        public bool ReturnValue { get; }
    }
    
    [AttributeUsage(AttributeTargets.Parameter)]
    public sealed class MaybeNullWhenAttribute : Attribute
    {
        public MaybeNullWhenAttribute(bool returnValue) => ReturnValue = returnValue;
        
        public bool ReturnValue { get; }
    }

}
#endif