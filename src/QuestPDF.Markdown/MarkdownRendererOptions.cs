using System.Runtime.InteropServices;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace QuestPDF.Markdown;

public class MarkdownRendererOptions
{
    /// <summary>
    /// Render DebugAreas in the output
    /// </summary>
    public bool Debug { get; set; }

    public TextHorizontalAlignment ParagraphAlignment { get; set; } = TextHorizontalAlignment.Left;

    public Color LinkTextColor { get; set; } = Colors.Blue.Medium;
    public Color MarkedTextBackgroundColor { get; set; } = Colors.Yellow.Lighten2;

    public Color BlockQuoteBorderColor { get; set; } = Colors.Grey.Lighten2;
    public Color BlockQuoteTextColor { get; set; } = Colors.Grey.Darken1;
    public int BlockQuoteBorderThickness { get; set; } = 2;

    public string CodeFont { get; set; } = Fonts.CourierNew;
    public Color CodeBlockBackground { get; set; } = Colors.Grey.Lighten4;
    public Color CodeInlineBackground { get; set; } = Colors.Grey.Lighten3;

    public string TaskListCheckedGlyph { get; set; } = "☑";
    public string TaskListUncheckedGlyph { get; set; } = "☐";
    public string UnicodeGlyphFont { get; set; } = RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ? "Arial Unicode MS" : Fonts.Arial;

    public Color TableBorderColor { get; set; } = Colors.Grey.Lighten2;
    public Color TableEvenRowBackgroundColor { get; set; } = Colors.Grey.Lighten4;
    public Color TableOddRowBackgroundColor { get; set; } = Colors.White;
    public int TableHeaderBorderThickness { get; set; } = 3;
    public int TableBorderThickness { get; set; } = 1;
    public TableBorderStyle TableBorderStyle { get; set; } = TableBorderStyle.Horizontal;

    public Color HorizontalRuleColor { get; set; } = Colors.Grey.Lighten2;
    public int HorizontalRuleThickness { get; set; } = 2;

    /// <summary>
    /// The conversion factor used to scale images from pixel size to point size
    /// </summary>
    public float ImageScalingFactor { get; set; } = 0.5f;

    /// <summary>
    /// The maximum allowed width for rendered images, in points.
    /// A value of 0 (the default) indicates no maximum width limit.
    /// </summary>
    public float MaxImageWidth { get; set; }

    /// <summary>
    /// The maximum allowed height for rendered images, in points.
    /// A value of 0 (the default) indicates no maximum height limit.
    /// </summary>
    public float MaxImageHeight { get; set; }

    public float ParagraphSpacing { get; set; } = 10;
    public float ListItemSpacing { get; set; } = 5;
    public string UnorderedListGlyph { get; set; } = "•";

    public Color HeadingTextColor { get; set; } = Colors.Black;
    
    /// <summary>
    /// The formula used to calculate heading sizes based on their level.
    /// </summary>
    /// <remarks>
    /// Level is non zero-indexed and starts at 1 for the largest heading
    /// </remarks>
    public Func<int, int> CalculateHeadingSize { get; set; } = level => 28 - 2 * (level - 1);
    public float HeadingSizeLevel1 { get; set; }
    public float HeadingSizeLevel2 { get; set; }
    public float HeadingSizeLevel3 { get; set; }
    public float HeadingSizeLevel4 { get; set; }

    public Dictionary<string, Func<TextDescriptor, TextSpanDescriptor>> RenderTemplates { get; } = [];

    /// <summary>
    /// Register a render function to replace a template tag in the markdown text with custom content
    /// The tag 'my_tag' can be used in the markdown as {my_tag}
    /// </summary>
    /// <remarks>The rendered content must fit within a single line. Larger block elements are currently not supported</remarks>
    /// <param name="tag">The tag to replace.</param>
    /// <param name="render">The render function to render custom text in place of the template tag</param>
    public MarkdownRendererOptions AddTemplateTag(string tag, Func<TextDescriptor, TextSpanDescriptor> render)
    {
        RenderTemplates[tag] = render;
        return this;
    }
}

public enum TableBorderStyle
{
    None,
    Horizontal,
    Full
}