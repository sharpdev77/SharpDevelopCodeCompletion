// Copyright (c) AlphaSierraPapa for the SharpDevelop Team (for details please see \doc\copyright.txt)
// This code is distributed under the GNU LGPL (for details please see \doc\license.txt)

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;

namespace ICSharpCode.AvalonEdit.CodeCompletion
{
    /// <summary>
    /// Adapter between AvalonEdit InsightWindow and SharpDevelop IInsightWindow interface.
    /// </summary>
    public class SharpDevelopInsightWindow : OverloadInsightWindow, IInsightWindow
    {
        private readonly ObservableCollection<IInsightItem> items = new ObservableCollection<IInsightItem>();
        private Caret caret;
        private TextDocument document;

        public SharpDevelopInsightWindow(TextArea textArea) : base(textArea)
        {
            Provider = new SDItemProvider(this);
            Provider.PropertyChanged += delegate(object sender, System.ComponentModel.PropertyChangedEventArgs e)
                                            {
                                                if (e.PropertyName == "SelectedIndex")
                                                    OnSelectedItemChanged(EventArgs.Empty);
                                            };
            AttachEvents();
        }

        #region IInsightWindow Members

        public IList<IInsightItem> Items
        {
            get { return items; }
        }

        public IInsightItem SelectedItem
        {
            get
            {
                int index = Provider.SelectedIndex;
                if (index < 0 || index >= items.Count)
                    return null;
                else
                    return items[index];
            }
            set
            {
                Provider.SelectedIndex = items.IndexOf(value);
                OnSelectedItemChanged(EventArgs.Empty);
            }
        }

        public event EventHandler<TextChangeEventArgs> DocumentChanged;

        public event EventHandler SelectedItemChanged;
        public event EventHandler CaretPositionChanged;

        #endregion

        private void AttachEvents()
        {
            document = TextArea.Document;
            caret = TextArea.Caret;
            if (document != null)
                document.Changed += document_Changed;
            if (caret != null)
                caret.PositionChanged += caret_PositionChanged;
        }

        private void caret_PositionChanged(object sender, EventArgs e)
        {
            OnCaretPositionChanged(e);
        }

        /// <inheritdoc/>
        protected override void DetachEvents()
        {
            if (document != null)
                document.Changed -= document_Changed;
            if (caret != null)
                caret.PositionChanged -= caret_PositionChanged;
            base.DetachEvents();
        }

        private void document_Changed(object sender, DocumentChangeEventArgs e)
        {
            if (DocumentChanged != null)
                DocumentChanged(this, new TextChangeEventArgs(e.Offset, e.RemovedText, e.InsertedText));
        }

        protected virtual void OnSelectedItemChanged(EventArgs e)
        {
            if (SelectedItemChanged != null)
            {
                SelectedItemChanged(this, e);
            }
        }

        protected virtual void OnCaretPositionChanged(EventArgs e)
        {
            if (CaretPositionChanged != null)
            {
                CaretPositionChanged(this, e);
            }
        }

        #region Nested type: SDItemProvider

        private sealed class SDItemProvider : IOverloadProvider
        {
            private readonly SharpDevelopInsightWindow insightWindow;
            private int selectedIndex;

            public SDItemProvider(SharpDevelopInsightWindow insightWindow)
            {
                this.insightWindow = insightWindow;
                insightWindow.items.CollectionChanged += insightWindow_items_CollectionChanged;
            }

            #region IOverloadProvider Members

            public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

            public int SelectedIndex
            {
                get { return selectedIndex; }
                set
                {
                    if (selectedIndex != value)
                    {
                        selectedIndex = value;
                        OnPropertyChanged("SelectedIndex");
                        OnPropertyChanged("CurrentHeader");
                        OnPropertyChanged("CurrentContent");
                        OnPropertyChanged("CurrentIndexText");
                    }
                }
            }

            public int Count
            {
                get { return insightWindow.Items.Count; }
            }

            public string CurrentIndexText
            {
                get { return (selectedIndex + 1).ToString() + " of " + Count.ToString(); }
            }

            public object CurrentHeader
            {
                get
                {
                    IInsightItem item = insightWindow.SelectedItem;
                    return item != null ? item.Header : null;
                }
            }

            public object CurrentContent
            {
                get
                {
                    IInsightItem item = insightWindow.SelectedItem;
                    return item != null ? item.Content : null;
                }
            }
            
            #endregion

            private void insightWindow_items_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
            {
                OnPropertyChanged("Count");
                OnPropertyChanged("CurrentHeader");
                OnPropertyChanged("CurrentContent");
                OnPropertyChanged("CurrentIndexText");
            }

            private void OnPropertyChanged(string propertyName)
            {
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
                }
            }
        }

        #endregion
    }
}