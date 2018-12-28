namespace ICSharpCode.AvalonEdit.CodeCompletion
{
    /// <summary>
    /// An item in the insight window.
    /// </summary>
    public interface IInsightItem
    {
        object Header { get; }
        object Content { get; }
    }
}