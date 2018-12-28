namespace ICSharpCode.AvalonEdit.CodeCompletion
{
    public enum CompletionItemListKeyResult
    {
        /// <summary>
        /// Normal key, used to choose an entry from the completion list
        /// </summary>
        NormalKey,

        /// <summary>
        /// This key triggers insertion of the completed expression
        /// </summary>
        InsertionKey,

        /// <summary>
        /// Increment both start and end offset of completion region when inserting this
        /// key. Can be used to insert whitespace (or other characters) in front of the expression
        /// while the completion window is open.
        /// </summary>
        BeforeStartKey
    }
}