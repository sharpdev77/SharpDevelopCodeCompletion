using System;

namespace ICSharpCode.AvalonEdit.CodeCompletion
{
    /// <summary>
    /// Describes a change of the document text.
    /// This class is thread-safe.
    /// </summary>
    public class TextChangeEventArgs : EventArgs
    {
        /// <summary>
        /// Creates a new TextChangeEventArgs object.
        /// </summary>
        public TextChangeEventArgs(int offset, string removedText, string insertedText)
        {
            Offset = offset;
            RemovedText = removedText ?? string.Empty;
            InsertedText = insertedText ?? string.Empty;
        }

        /// <summary>
        /// The offset at which the change occurs.
        /// </summary>
        public int Offset { get; private set; }

        /// <summary>
        /// The text that was inserted.
        /// </summary>
        public string RemovedText { get; private set; }

        /// <summary>
        /// The number of characters removed.
        /// </summary>
        public int RemovalLength
        {
            get { return RemovedText.Length; }
        }

        /// <summary>
        /// The text that was inserted.
        /// </summary>
        public string InsertedText { get; private set; }

        /// <summary>
        /// The number of characters inserted.
        /// </summary>
        public int InsertionLength
        {
            get { return InsertedText.Length; }
        }
    }
}