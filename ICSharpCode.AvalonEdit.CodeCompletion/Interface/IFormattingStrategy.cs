namespace ICSharpCode.AvalonEdit.CodeCompletion
{
    /// <summary>
    /// Indentation and formatting strategy.
    /// </summary>
    public interface IFormattingStrategy
    {
        /// <summary>
        /// This function formats a specific line after <code>charTyped</code> is pressed.
        /// </summary>
        void FormatLine(ITextEditor editor, char charTyped);

        /// <summary>
        /// This function sets the indentation level in a specific line
        /// </summary>
        void IndentLine(ITextEditor editor, IDocumentLine line);

        /// <summary>
        /// This function sets the indentation in a range of lines.
        /// </summary>
        void IndentLines(ITextEditor editor, int beginLine, int endLine);

        /// <summary>
        /// This function surrounds the selected text with a comment.
        /// </summary>
        void SurroundSelectionWithComment(ITextEditor editor);
    }
}