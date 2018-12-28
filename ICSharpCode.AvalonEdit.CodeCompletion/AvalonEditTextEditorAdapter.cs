using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using ICSharpCode.AvalonEdit.Editing;
using ICSharpCode.NRefactory;

namespace ICSharpCode.AvalonEdit.CodeCompletion
{
    /// <summary>
    /// Wraps AvalonEdit to provide the ITextEditor interface.
    /// </summary>
    public class AvalonEditTextEditorAdapter : ITextEditor, IWeakEventListener
    {
        private readonly TextEditor _textEditor;
        private AvalonEditDocumentAdapter _document;

        protected AvalonEditTextEditorAdapter(TextEditor textEditor)
        {
            if (textEditor == null)
                throw new ArgumentNullException("textEditor");
            _textEditor = textEditor;
            Caret = new CaretAdapter(textEditor.TextArea.Caret);
            Options = new OptionsAdapter(textEditor.Options);
            TextEditorWeakEventManager.DocumentChanged.AddListener(textEditor, this);
            OnDocumentChanged();
        }

        protected TextEditor TextEditor
        {
            get { return _textEditor; }
        }

        #region ITextEditor Members

        public IDocument Document
        {
            get { return _document; }
        }

        public ITextEditorCaret Caret { get; private set; }
        public virtual ITextEditorOptions Options { get; private set; }

        public virtual ILanguageBinding Language
        {
            get { return AggregatedLanguageBinding.NullLanguageBinding; }
        }

        public virtual FileName FileName
        {
            get { return null; }
        }

        public object GetService(Type serviceType)
        {
            return _textEditor.TextArea.GetService(serviceType);
        }

        public int SelectionStart
        {
            get { return _textEditor.SelectionStart; }
        }

        public int SelectionLength
        {
            get { return _textEditor.SelectionLength; }
        }

        public string SelectedText
        {
            get { return _textEditor.SelectedText; }
            set { _textEditor.SelectedText = value; }
        }

        public event KeyEventHandler KeyPress
        {
            add { _textEditor.TextArea.PreviewKeyDown += value; }
            remove { _textEditor.TextArea.PreviewKeyDown -= value; }
        }

        public event EventHandler SelectionChanged
        {
            add { _textEditor.TextArea.SelectionChanged += value; }
            remove { _textEditor.TextArea.SelectionChanged -= value; }
        }

        public void Select(int selectionStart, int selectionLength)
        {
            _textEditor.Select(selectionStart, selectionLength);
            _textEditor.TextArea.Caret.BringCaretToView();
        }

        public void JumpTo(int line, int column)
        {
            _textEditor.TextArea.ClearSelection();
            _textEditor.TextArea.Caret.Position = new TextViewPosition(line, column);
            // might have jumped to a different location if column was outside the valid range
            var actualLocation = _textEditor.TextArea.Caret.Location;
            if (_textEditor.ActualHeight > 0)
            {
                _textEditor.ScrollTo(actualLocation.Line, actualLocation.Column);
            }
            else
            {
                // we have to delay the scrolling if the text editor is not yet loaded
                _textEditor.Dispatcher.BeginInvoke(DispatcherPriority.Loaded, new Action(
                                                                                 () => _textEditor.ScrollTo(
                                                                                     actualLocation.Line,
                                                                                     actualLocation.Column)));
            }
        }

        public virtual IInsightWindow ActiveInsightWindow
        {
            get { return null; }
        }

        public virtual IInsightWindow ShowInsightWindow(IEnumerable<IInsightItem> items)
        {
            return null;
        }

        public virtual ICompletionListWindow ActiveCompletionWindow
        {
            get { return null; }
        }

        public virtual ICompletionListWindow ShowCompletionWindow(ICompletionItemList data)
        {
            return null;
        }

        public virtual IEnumerable<ICompletionItem> GetSnippets()
        {
            return Enumerable.Empty<ICompletionItem>();
        }

        public virtual ITextEditor PrimaryView
        {
            get { return this; }
        }

        #endregion

        #region IWeakEventListener Members

        bool IWeakEventListener.ReceiveWeakEvent(Type managerType, object sender, EventArgs e)
        {
            return ReceiveWeakEvent(managerType);
        }

        #endregion

        private bool ReceiveWeakEvent(Type managerType)
        {
            if (managerType == typeof(TextEditorWeakEventManager.DocumentChanged))
            {
                OnDocumentChanged();
                return true;
            }
            return false;
        }

        private void OnDocumentChanged()
        {
            _document = _textEditor.Document != null ? new AvalonEditDocumentAdapter(_textEditor.Document, this) : null;
        }

        #region Nested type: CaretAdapter

        private sealed class CaretAdapter : ITextEditorCaret
        {
            private readonly Caret _caret;

            public CaretAdapter(Caret caret)
            {
                Debug.Assert(caret != null);
                _caret = caret;
            }

            #region ITextEditorCaret Members

            public int Offset
            {
                get { return _caret.Offset; }
                set { _caret.Offset = value; }
            }

            public int Line
            {
                get { return _caret.Line; }
                set { _caret.Line = value; }
            }

            public int Column
            {
                get { return _caret.Column; }
                set { _caret.Column = value; }
            }

            public Location Position
            {
                get { return AvalonEditDocumentAdapter.ToLocation(_caret.Location); }
                set { _caret.Location = AvalonEditDocumentAdapter.ToPosition(value); }
            }

            public event EventHandler PositionChanged
            {
                add { _caret.PositionChanged += value; }
                remove { _caret.PositionChanged -= value; }
            }

            #endregion
        }

        #endregion

        #region Nested type: OptionsAdapter

        private sealed class OptionsAdapter : ITextEditorOptions
        {
            private readonly TextEditorOptions _avalonEditOptions;

            public OptionsAdapter(TextEditorOptions avalonEditOptions)
            {
                _avalonEditOptions = avalonEditOptions;
            }

            #region ITextEditorOptions Members

            public string IndentationString
            {
                get { return _avalonEditOptions.IndentationString; }
            }

            public bool AutoInsertBlockEnd
            {
                get { return true; }
            }

            public bool ConvertTabsToSpaces
            {
                get { return _avalonEditOptions.ConvertTabsToSpaces; }
            }

            public int IndentationSize
            {
                get { return _avalonEditOptions.IndentationSize; }
            }

            public int VerticalRulerColumn
            {
                get { return 120; }
            }

            public bool UnderlineErrors
            {
                get { return true; }
            }

            public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged
            {
                add { _avalonEditOptions.PropertyChanged += value; }
                remove { _avalonEditOptions.PropertyChanged -= value; }
            }

            public string FontFamily
            {
                get { return "Consolas"; }
            }

            #endregion
        }

        #endregion
    }
}