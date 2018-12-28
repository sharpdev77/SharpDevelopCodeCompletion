using System;
using System.Collections.Generic;
using System.Windows.Input;

namespace ICSharpCode.AvalonEdit.CodeCompletion
{
    /// <summary>
    /// Interface for text editors.
    /// </summary>
    public interface ITextEditor : IServiceProvider
    {
        /// <summary>
        /// Gets the primary view if split-view is active.
        /// If split-view is disabled, the current ITextEditor instance is returned.
        /// This property never returns null.
        /// </summary>
        /// <example>bool isSecondaryView = (editor != editor.PrimaryView);</example>
        ITextEditor PrimaryView { get; }

        /// <summary>
        /// Gets the document that is being edited.
        /// </summary>
        IDocument Document { get; }

        /// <summary>
        /// Gets an object that represents the caret inside this text editor.
        /// This property never returns null.
        /// </summary>
        ITextEditorCaret Caret { get; }

        /// <summary>
        /// Gets the set of options used in the text editor.
        /// This property never returns null.
        /// </summary>
        ITextEditorOptions Options { get; }

        /// <summary>
        /// Gets the language binding attached to this text editor.
        /// This property never returns null.
        /// </summary>
        ILanguageBinding Language { get; }

        /// <summary>
        /// Gets the start offset of the selection.
        /// </summary>
        int SelectionStart { get; }

        /// <summary>
        /// Gets the length of the selection.
        /// </summary>
        int SelectionLength { get; }

        /// <summary>
        /// Gets/Sets the selected text.
        /// </summary>
        string SelectedText { get; set; }

        FileName FileName { get; }

        /// <summary>
        /// Gets the completion window that is currently open.
        /// </summary>
        ICompletionListWindow ActiveCompletionWindow { get; }

        /// <summary>
        /// Gets the insight window that is currently open.
        /// </summary>
        IInsightWindow ActiveInsightWindow { get; }

        /// <summary>
        /// Sets the selection.
        /// </summary>
        /// <param name="selectionStart">Start offset of the selection</param>
        /// <param name="selectionLength">Length of the selection</param>
        void Select(int selectionStart, int selectionLength);

        /// <summary>
        /// Is raised when the selection changes.
        /// </summary>
        event EventHandler SelectionChanged;

        /// <summary>
        /// Is raised before a key is pressed.
        /// </summary>
        event KeyEventHandler KeyPress;

        /// <summary>
        /// Sets the caret to the specified line/column and brings the caret into view.
        /// </summary>
        void JumpTo(int line, int column);

        ICompletionListWindow ShowCompletionWindow(ICompletionItemList data);

        /// <summary>
        /// Open a new insight window showing the specified insight items.
        /// </summary>
        /// <param name="items">The insight items to show in the window.
        /// If this property is null or an empty list, the insight window will not be shown.</param>
        /// <returns>The insight window; or null if no insight window was opened.</returns>
        IInsightWindow ShowInsightWindow(IEnumerable<IInsightItem> items);

        /// <summary>
        /// Gets the list of available code snippets.
        /// </summary>
        IEnumerable<ICompletionItem> GetSnippets();
    }
}