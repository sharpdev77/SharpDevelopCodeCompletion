namespace ICSharpCode.AvalonEdit.CodeCompletion
{
    /// <summary>
    /// The result of <see cref="ICodeCompletionBinding.HandleKeyPress"/>.
    /// </summary>
    public enum CodeCompletionKeyPressResult
    {
        /// <summary>
        /// The binding did not run code completion. The pressed key will be handled normally.
        /// </summary>
        None,
        /// <summary>
        /// The binding handled code completion, the pressed key will be handled normally.
        /// The pressed key will not be included in the completion expression, but will be
        /// in front of it (this is usually used when the key is '.').
        /// </summary>
        Completed,
        /// <summary>
        /// The binding handled code completion, the pressed key will be handled normally.
        /// The pressed key will be included in the completion expression.
        /// This is used when starting to type any character starts code completion.
        /// </summary>
        CompletedIncludeKeyInCompletion,
        /// <summary>
        /// The binding handled code completion, and the key will not be handled by the text editor.
        /// </summary>
        EatKey,
    }
}