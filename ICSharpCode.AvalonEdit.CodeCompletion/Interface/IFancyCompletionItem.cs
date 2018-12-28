namespace ICSharpCode.AvalonEdit.CodeCompletion
{
    /// <summary>
    /// Completion item that supports complex content and description.
    /// </summary>
    public interface IFancyCompletionItem : ICompletionItem
    {
        object Content { get; }
        new object Description { get; }
    }
}