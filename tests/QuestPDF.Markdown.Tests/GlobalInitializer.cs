using System.Runtime.CompilerServices;
using ImageMagick;
using QuestPDF.Infrastructure;

namespace QuestPDF.Markdown.Tests;

internal static class GlobalInitializer
{
    [ModuleInitializer]
    public static void Init()
    {
        Settings.License = LicenseType.Community;
        Settings.EnableDebugging = true;
        
        UseProjectRelativeDirectory("Verify");
        
        VerifyImageMagick.RegisterComparers(threshold: 0.015);
        VerifyQuestPdf.Initialize();
    }
}