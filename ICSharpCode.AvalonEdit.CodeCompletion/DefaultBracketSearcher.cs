namespace ICSharpCode.AvalonEdit.CodeCompletion
{
    public class DefaultBracketSearcher : IBracketSearcher
    {
        public static readonly DefaultBracketSearcher DefaultInstance = new DefaultBracketSearcher();

        #region IBracketSearcher Members

        public BracketSearchResult SearchBracket(IDocument document, int offset)
        {
            return null;
        }

        #endregion
    }
}