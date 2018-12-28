using ICSharpCode.SharpDevelop.Dom;

namespace ICSharpCode.AvalonEdit.CodeCompletion.CompetionItems
{
    /// <summary>
    /// CodeCompletionItem that inserts also generic arguments.
    /// Used only when suggesting items in CC (e.g. List&lt;int&gt; a = new => suggest List&lt;int&gt;).
    /// </summary>
    public class SuggestedCodeCompletionItem : CodeCompletionItem
    {
        public SuggestedCodeCompletionItem(IEntity entity, string nameWithSpecifiedGenericArguments, IProjectContent projectContent)
            : base(entity, projectContent)
        {
            Text = nameWithSpecifiedGenericArguments;
            Content = nameWithSpecifiedGenericArguments;
        }
    }
}