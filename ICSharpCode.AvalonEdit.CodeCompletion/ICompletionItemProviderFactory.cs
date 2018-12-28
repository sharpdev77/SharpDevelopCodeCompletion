using ICSharpCode.SharpDevelop.Dom;

namespace ICSharpCode.AvalonEdit.CodeCompletion
{
    public interface ICompletionItemProviderFactory
    {
        CtrlSpaceCompletionItemProvider Create(LanguageProperties languageProperties, IProjectContent projectContent);
        CtrlSpaceCompletionItemProvider Create(LanguageProperties languageProperties, ExpressionContext expressionContext, IProjectContent projectContent);
    }
}