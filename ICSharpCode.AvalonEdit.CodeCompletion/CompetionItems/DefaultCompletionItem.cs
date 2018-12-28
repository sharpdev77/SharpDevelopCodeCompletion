using ICSharpCode.AvalonEdit.CodeCompletion.Interface.Description;

namespace ICSharpCode.AvalonEdit.CodeCompletion.CompetionItems
{
    public class DefaultCompletionItem : ICompletionItem
    {
        public DefaultCompletionItem(string text)
        {
            Text = text;
        }

        #region ICompletionItem Members

        public string Text { get; private set; }
        public virtual Description Description { get; set; }
        public virtual IImage Image { get; set; }

        public virtual double Priority
        {
            get { return 0; }
        }

        public virtual void Complete(CompletionContext context)
        {
            CompleteText(context, Text);
        }

        protected void CompleteText(CompletionContext context, string text)
        {
            context.Editor.Document.Replace(context.StartOffset, context.Length, text);
            context.EndOffset = context.StartOffset + text.Length;
        }

        #endregion
    }
}