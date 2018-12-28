using System;
using ICSharpCode.NRefactory;

namespace ICSharpCode.AvalonEdit.CodeCompletion
{
    public interface ITextEditorCaret
    {
        /// <summary>
        /// Gets/Sets the caret offset;
        /// </summary>
        int Offset { get; set; }

        /// <summary>
        /// Gets/Sets the caret line number.
        /// Line numbers are counted starting from 1.
        /// </summary>
        int Line { get; set; }

        /// <summary>
        /// Gets/Sets the caret column number.
        /// Column numbers are counted starting from 1.
        /// </summary>
        int Column { get; set; }

        /// <summary>
        /// Gets/sets the caret position.
        /// </summary>
        Location Position { get; set; }

        /// <summary>
        /// Is raised whenever the position of the caret has changed.
        ///	</summary>
        event EventHandler PositionChanged;
    }
}