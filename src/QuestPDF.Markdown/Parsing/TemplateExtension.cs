using Markdig;
using Markdig.Renderers;

namespace QuestPDF.Markdown.Parsing;

internal sealed class TemplateExtension : IMarkdownExtension 
{
    public void Setup(MarkdownPipelineBuilder pipeline)
    {
        pipeline.InlineParsers.AddIfNotAlready<TemplateParser>();
    }

    public void Setup(MarkdownPipeline pipeline, IMarkdownRenderer renderer)
    {
        
    }
}