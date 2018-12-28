using System.Collections.Generic;

namespace ICSharpCode.AvalonEdit.CodeCompletion
{
    public interface ICompletionItemList
    {
        /// <summary>
        /// Gets the items in the list.
        /// </summary>
        IEnumerable<ICompletionItem> Items { get; }

        /// <summary>
        /// Gets the suggested item.
        /// This item will be pre-selected in the completion list.
        /// </summary>
        ICompletionItem SuggestedItem { get; }

        /// <summary>
        /// Gets the length of the preselection (text in front of the completion list that
        /// should be included as completed expression).
        /// </summary>
        int PreselectionLength { get; }

        /// <summary>
        /// True if this list contains all items that were available.
        /// False if this list could contain even more items 
        /// (e.g. by including items from all referenced projects, regardless of imports).
        /// </summary>
        bool ContainsAllAvailableItems { get; }

        /// <summary>
        /// Processes the specified key press.
        /// </summary>
        CompletionItemListKeyResult ProcessInput(char key);

        /// <summary>
        /// Performs code completion for the selected item.
        /// </summary>
        void Complete(CompletionContext context, ICompletionItem item);
    }
}