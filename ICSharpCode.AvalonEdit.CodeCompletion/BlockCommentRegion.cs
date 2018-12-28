namespace ICSharpCode.AvalonEdit.CodeCompletion
{
    public class BlockCommentRegion
    {
        /// <summary>
        /// The end offset is the offset where the comment end string starts from.
        /// </summary>
        public BlockCommentRegion(string commentStart, string commentEnd, int startOffset, int endOffset)
        {
            CommentStart = commentStart;
            CommentEnd = commentEnd;
            StartOffset = startOffset;
            EndOffset = endOffset;
        }

        public string CommentStart { get; private set; }
        public string CommentEnd { get; private set; }
        public int StartOffset { get; private set; }
        public int EndOffset { get; private set; }

        public override int GetHashCode()
        {
            int hashCode = 0;
            unchecked
            {
                if (CommentStart != null) hashCode += 1000000007*CommentStart.GetHashCode();
                if (CommentEnd != null) hashCode += 1000000009*CommentEnd.GetHashCode();
                hashCode += 1000000021*StartOffset.GetHashCode();
                hashCode += 1000000033*EndOffset.GetHashCode();
            }
            return hashCode;
        }

        public override bool Equals(object obj)
        {
            var other = obj as BlockCommentRegion;
            if (other == null) return false;
            return CommentStart == other.CommentStart &&
                   CommentEnd == other.CommentEnd &&
                   StartOffset == other.StartOffset &&
                   EndOffset == other.EndOffset;
        }
    }
}