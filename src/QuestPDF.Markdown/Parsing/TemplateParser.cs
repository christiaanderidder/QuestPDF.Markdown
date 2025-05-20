using Markdig.Helpers;
using Markdig.Parsers;
using Markdig.Syntax;
using QuestPDF.Markdown.Compatibility;

namespace QuestPDF.Markdown.Parsing;

internal sealed class TemplateParser : InlineParser 
{
    private const char OpeningCharacter = '{';
    private const char ClosingCharacter = '}';
    
    public TemplateParser()
    {
        OpeningCharacters = [OpeningCharacter];
    }
    
    public override bool Match(InlineProcessor processor, ref StringSlice slice)
    {
        ExceptionHelper.ThrowIfNull(processor);
        
        var match = slice.CurrentChar;
        if (slice.PeekCharExtra(-1) == match) return false;

        var span = slice.AsSpan();

        for (var i = 1; i < span.Length; i++)
        {
            var c = span[i];

            if (c == ClosingCharacter)
            {
                var tag = span.Slice(1, i - 1).ToString();
                var start = processor.GetSourcePosition(slice.Start + 1, out var line, out var column);
                var end = processor.GetSourcePosition(slice.Start + i);
                
                var template = new TemplateInline(tag)
                {
                    Span = new SourceSpan(start, end),
                    Line = line,
                    Column = column
                };
                
                processor.Inline = template;
                slice.Start = end + 1;
                
                return true;
            }
            if (!c.IsAlphaNumeric()) return false;
        }

        return false;
    }
}