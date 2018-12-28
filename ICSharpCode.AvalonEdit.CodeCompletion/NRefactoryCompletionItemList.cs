using ICSharpCode.AvalonEdit.CodeCompletion.CompetionItems.Providers;

namespace ICSharpCode.AvalonEdit.CodeCompletion
{
    /// <summary>
    /// <see cref="ICompletionItemList" /> created by <see cref="NRefactoryCtrlSpaceCompletionItemProvider" />.
    /// </summary>
    public class NRefactoryCompletionItemList : DefaultCompletionItemList
    {
        /// <summary>
        /// <see cref="NRefactoryCtrlSpaceCompletionItemProvider" /> sets this to true if this list contains items
        /// from all namespaces, regardless of current imports.
        /// </summary>
        public bool ContainsItemsFromAllNamespaces { get; set; }

        /// <inheritdoc />
        public override bool ContainsAllAvailableItems
        {
            get { return ContainsItemsFromAllNamespaces; }
        }
    }
}