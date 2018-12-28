using System;
using System.ComponentModel;
using System.Linq;
using ICSharpCode.AvalonEdit.CodeCompletion.Interface.Description;
using ICSharpCode.SharpDevelop.Dom;

namespace ICSharpCode.AvalonEdit.CodeCompletion
{
    public interface ICompletionItem
    {
        string Text { get; }
        Description Description { get; }
        IImage Image { get; }

        /// <summary>
        /// Gets a priority value for the completion data item.
        /// When selecting items by their start characters, the item with the highest
        /// priority is selected first.
        /// </summary>
        double Priority { get; }

        /// <summary>
        /// Performs code completion for the item.
        /// </summary>
        void Complete(CompletionContext context);
    }

}