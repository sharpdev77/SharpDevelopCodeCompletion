using System;
using System.Windows.Media;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;
using ICSharpCode.SharpDevelop.Dom;

namespace ICSharpCode.AvalonEdit.CodeCompletion
{
    public class CompletionData : ICompletionData
    {
        public CompletionData(ICompletionEntry text, ExpressionResult expressionResult)
        {
            ExpressionResult = expressionResult;
            Text = text.Name;
            Content = text.Name;
            Description = text.ToString();
        }

        public CompletionData(ICompletionItem item)
        {
            Text = item.Text;
            Description = item.Description;
            Content = item.Text;
        }

        public ExpressionResult ExpressionResult { get; set; }

        #region ICompletionData Members

        public void Complete(TextArea textArea, ISegment completionSegment, EventArgs insertionRequestEventArgs)
        {
            textArea.Document.Replace(completionSegment, Text);
        }

        public ImageSource Image
        {
            get { return null; }
        }

        public string Text { get; private set; }

        public object Content { get; private set; }

        public object Description { get; private set; }

        public double Priority
        {
            get { return 1.0; }
        }

        #endregion
    }
}