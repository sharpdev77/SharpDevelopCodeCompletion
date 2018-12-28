namespace ICSharpCode.AvalonEdit.CodeCompletion
{
    public interface ISnippetCompletionItem : ICompletionItem
    {
        string Keyword { get; }
    }
}