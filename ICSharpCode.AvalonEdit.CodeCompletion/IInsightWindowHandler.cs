using ICSharpCode.SharpDevelop.Dom;

namespace ICSharpCode.AvalonEdit.CodeCompletion
{
    public interface IInsightWindowHandler
    {
        void InitializeOpenedInsightWindow(ITextEditor editor, IInsightWindow insightWindow);
        bool InsightRefreshOnComma(ITextEditor editor, char ch, out IInsightWindow insightWindow, IProjectContent projectContent);
        void HighlightParameter(IInsightWindow window, int index);
    }
}