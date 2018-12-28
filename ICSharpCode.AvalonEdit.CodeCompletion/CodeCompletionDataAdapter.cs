using System;
using System.ComponentModel;
using System.Windows.Input;
using System.Windows.Media;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;

namespace ICSharpCode.AvalonEdit.CodeCompletion
{
    public sealed class CodeCompletionDataAdapter : ICompletionData, INotifyPropertyChanged
    {
        private readonly IFancyCompletionItem fancyCompletionItem;
        private readonly ICompletionItem item;
        private readonly SharpDevelopCompletionWindow window;

        public CodeCompletionDataAdapter(SharpDevelopCompletionWindow window, ICompletionItem item)
        {
            if (window == null)
                throw new ArgumentNullException("window");
            if (item == null)
                throw new ArgumentNullException("item");
            this.window = window;
            this.item = item;
            fancyCompletionItem = item as IFancyCompletionItem;
        }

        public ICompletionItem Item
        {
            get { return item; }
        }

        #region ICompletionData Members

        public string Text
        {
            get { return item.Text; }
        }

        public object Content
        {
            get { return (fancyCompletionItem != null) ? fancyCompletionItem.Content : item.Text; }
        }

        public object Description
        {
            get { return (fancyCompletionItem != null) ? fancyCompletionItem.Description : item.Description; }
        }

        public ImageSource Image
        {
            get
            {
                IImage image = item.Image;
                return image != null ? image.ImageSource : null;
            }
        }

        public double Priority
        {
            get { return item.Priority; }
        }

        public void Complete(TextArea textArea, ISegment completionSegment, EventArgs insertionRequestEventArgs)
        {
            var context = new CompletionContext
                              {
                                  Editor = window.Editor,
                                  StartOffset = window.StartOffset,
                                  EndOffset = window.EndOffset
                              };
            var txea = insertionRequestEventArgs as TextCompositionEventArgs;
            var kea = insertionRequestEventArgs as KeyEventArgs;
            if (txea != null && txea.Text.Length > 0)
                context.CompletionChar = txea.Text[0];
            else if (kea != null && kea.Key == Key.Tab)
                context.CompletionChar = '\t';
            window.ItemList.Complete(context, item);
            if (context.CompletionCharHandled && txea != null)
                txea.Handled = true;
        }

        #endregion

        // This is required to work around http://support.microsoft.com/kb/938416/en-us

        #region INotifyPropertyChanged Members

        event System.ComponentModel.PropertyChangedEventHandler INotifyPropertyChanged.PropertyChanged
        {
            add { }
            remove { }
        }

        #endregion
    }
}