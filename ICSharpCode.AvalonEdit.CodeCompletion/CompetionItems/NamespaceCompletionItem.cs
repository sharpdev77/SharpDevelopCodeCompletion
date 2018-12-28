using ICSharpCode.SharpDevelop.Dom;

namespace ICSharpCode.AvalonEdit.CodeCompletion.CompetionItems
{
    public class NamespaceCompletionItem : DefaultCompletionItem
    {
        public NamespaceCompletionItem(NamespaceEntry entry) : base(entry.Name) {}
    }
}