using ICSharpCode.AvalonEdit.CodeCompletion.CompetionItems.Providers;
using ICSharpCode.SharpDevelop.Dom;

namespace ICSharpCode.AvalonEdit.CodeCompletion
{
    class CompletionItemProviderFactory : ICompletionItemProviderFactory
    {
        public CtrlSpaceCompletionItemProvider Create(LanguageProperties languageProperties, IProjectContent projectContent)
        {
            return new NRefactoryCtrlSpaceCompletionItemProvider(languageProperties, projectContent);
        }

        public CtrlSpaceCompletionItemProvider Create(LanguageProperties languageProperties, ExpressionContext expressionContext, IProjectContent projectContent)
        {
            return new NRefactoryCtrlSpaceCompletionItemProvider(languageProperties, expressionContext, projectContent);
        }
    }
}