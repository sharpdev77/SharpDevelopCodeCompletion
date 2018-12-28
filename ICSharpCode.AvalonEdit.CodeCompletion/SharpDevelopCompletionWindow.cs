// Copyright (c) AlphaSierraPapa for the SharpDevelop Team (for details please see \doc\copyright.txt)
// This code is distributed under the GNU LGPL (for details please see \doc\license.txt)

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ICSharpCode.AvalonEdit.Editing;

namespace ICSharpCode.AvalonEdit.CodeCompletion
{
    /// <summary>
    /// The code completion window.
    /// </summary>
    public class SharpDevelopCompletionWindow : CompletionWindow, ICompletionListWindow
    {
        public static readonly DependencyProperty EmptyTextProperty =
            DependencyProperty.Register("EmptyText", typeof (string), typeof (SharpDevelopCompletionWindow),
                                        new FrameworkPropertyMetadata());

        private readonly ICompletionItemList itemList;

        public SharpDevelopCompletionWindow(ITextEditor editor, TextArea textArea, ICompletionItemList itemList)
            : base(textArea)
        {
            if (editor == null)
                throw new ArgumentNullException("editor");
            if (itemList == null)
                throw new ArgumentNullException("itemList");

            if (!itemList.ContainsAllAvailableItems)
            {
                // If more items are available (Ctrl+Space wasn't pressed), show this hint
                EmptyText = "Empty";
            }

            Editor = editor;
            this.itemList = itemList;
            ICompletionItem suggestedItem = itemList.SuggestedItem;
            foreach (var item in itemList.Items)
            {
                ICompletionData adapter = new CodeCompletionDataAdapter(this, item);
                CompletionList.CompletionData.Add(adapter);
                if (item == suggestedItem)
                    CompletionList.SelectedItem = adapter;
            }
            StartOffset -= itemList.PreselectionLength;
            Width = 250;
        }

        public ICompletionItemList ItemList
        {
            get { return itemList; }
        }

        public ITextEditor Editor { get; private set; }

        /// <summary>
        /// The text thats is displayed when the <see cref="ItemList" /> is empty.
        /// </summary>
        public string EmptyText
        {
            get { return (string) GetValue(EmptyTextProperty); }
            set { SetValue(EmptyTextProperty, value); }
        }

        #region ICompletionListWindow Members

        public ICompletionItem SelectedItem
        {
            get { return ((CodeCompletionDataAdapter) CompletionList.SelectedItem).Item; }
            set
            {
                IEnumerable<CodeCompletionDataAdapter> itemAdapters =
                    CompletionList.CompletionData.Cast<CodeCompletionDataAdapter>();
                CompletionList.SelectedItem = itemAdapters.FirstOrDefault(a => a.Item == value);
            }
        }

        double ICompletionWindow.Width
        {
            get { return Width; }
            set
            {
                // Disable virtualization if we use automatic width - this prevents the window from resizing
                // when the user scrolls.
                VirtualizingStackPanel.SetIsVirtualizing(CompletionList.ListBox, !double.IsNaN(value));
                Width = value;
                if (double.IsNaN(value))
                {
                    // enable size-to-width:
                    if (SizeToContent == SizeToContent.Manual)
                        SizeToContent = SizeToContent.Width;
                    else if (SizeToContent == SizeToContent.Height)
                        SizeToContent = SizeToContent.WidthAndHeight;
                }
                else
                {
                    // disable size-to-width:
                    if (SizeToContent == SizeToContent.Width)
                        SizeToContent = SizeToContent.Manual;
                    else if (SizeToContent == SizeToContent.WidthAndHeight)
                        SizeToContent = SizeToContent.Height;
                }
            }
        }

        #endregion

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            if (itemList.PreselectionLength > 0 && itemList.SuggestedItem == null)
            {
                string preselection = TextArea.Document.GetText(StartOffset, EndOffset - StartOffset);
                CompletionList.SelectItem(preselection);
            }
        }

        protected override void OnTextInput(TextCompositionEventArgs e)
        {
            base.OnTextInput(e);
            if (e.Handled) return;
            foreach (var c in e.Text)
            {
                switch (itemList.ProcessInput(c))
                {
                    case CompletionItemListKeyResult.BeforeStartKey:
                        ExpectInsertionBeforeStart = true;
                        break;
                    case CompletionItemListKeyResult.NormalKey:
                        break;
                    case CompletionItemListKeyResult.InsertionKey:
                        CompletionList.RequestInsertion(e);
                        return;
                }
            }
        }
    }
}