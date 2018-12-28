using ICSharpCode.SharpDevelop.Dom;

namespace ICSharpCode.AvalonEdit.CodeCompletion.CompetionItems.Providers
{
    public class DotCodeCompletionItemProvider : CodeCompletionItemProvider
    {
        public DotCodeCompletionItemProvider(IProjectContent projectContent)
            : base(projectContent)
        {
            
        }

        protected override DefaultCompletionItemList CreateCompletionItemList()
        {
            return new NRefactoryCompletionItemList {ContainsItemsFromAllNamespaces = ShowItemsFromAllNamespaces};
        }
    }
}