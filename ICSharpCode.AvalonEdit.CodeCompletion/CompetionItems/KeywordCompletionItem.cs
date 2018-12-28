namespace ICSharpCode.AvalonEdit.CodeCompletion.CompetionItems
{
    public sealed class KeywordCompletionItem : DefaultCompletionItem
    {
        private readonly double priority;

        public KeywordCompletionItem(string text)
            : base(text)
        {
            priority = CodeCompletionDataUsageCache.GetPriority("keyword." + Text, true);
        }

        public override double Priority
        {
            get { return priority; }
        }

        public override void Complete(CompletionContext context)
        {
            CodeCompletionDataUsageCache.IncrementUsage("keyword." + Text);
            base.Complete(context);
        }
    }
}