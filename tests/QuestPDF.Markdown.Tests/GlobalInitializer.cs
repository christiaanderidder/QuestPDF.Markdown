using System.Runtime.CompilerServices;
using QuestPDF.Drawing;
using QuestPDF.Infrastructure;

namespace QuestPDF.Markdown.Tests;

internal static class GlobalInitializer
{
    [ModuleInitializer]
    public static void Init()
    {
        Settings.License = LicenseType.Community;
        Settings.EnableDebugging = true;

        FontManager.RegisterFontFromEmbeddedResource("QuestPDF.Markdown.Tests.Fonts.NotoSansMono-Regular.ttf");
        FontManager.RegisterFontFromEmbeddedResource("QuestPDF.Markdown.Tests.Fonts.NotoSansSymbols2-Regular.ttf");
        
        UseProjectRelativeDirectory("Verify");
        
        VerifyImageMagick.RegisterComparers();
        VerifyQuestPdf.Initialize();
    }
}