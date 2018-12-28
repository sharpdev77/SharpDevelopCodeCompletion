// Copyright (c) AlphaSierraPapa for the SharpDevelop Team (for details please see \doc\copyright.txt)
// This code is distributed under the GNU LGPL (for details please see \doc\license.txt)

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Threading;

namespace ICSharpCode.AvalonEdit.CodeCompletion
{
    internal class CodeCompletionEditorAdapter : AvalonEditTextEditorAdapter
    {
        private readonly TextEditor textEditor;
        private SharpDevelopCompletionWindow _activeCompletionWindow;
        private SharpDevelopInsightWindow _activeInsightWindow;

        public CodeCompletionEditorAdapter(TextEditor textEditor)
            : base(textEditor)
        {
            this.textEditor = textEditor;
        }

        public override IInsightWindow ActiveInsightWindow
        {
            get { return SharpDevelopInsightWindow; }
        }

        public override ICompletionListWindow ActiveCompletionWindow
        {
            get { return SharpDevelopCompletionWindow; }
        }

        public SharpDevelopInsightWindow SharpDevelopInsightWindow
        {
            get { return _activeInsightWindow; }
        }

        public SharpDevelopCompletionWindow SharpDevelopCompletionWindow
        {
            get { return _activeCompletionWindow; }
        }

        public override ICompletionListWindow ShowCompletionWindow(ICompletionItemList data)
        {
            if (data == null || !data.Items.Any())
                return null;
            var window = new SharpDevelopCompletionWindow(this, TextEditor.TextArea, data);
            ShowCompletionWindow(textEditor, window);
            return window;
        }

        private void CloseExistingCompletionWindow()
        {
            if (SharpDevelopCompletionWindow != null)
            {
                SharpDevelopCompletionWindow.Close();
            }
        }

        private void ShowCompletionWindow(TextEditor data, SharpDevelopCompletionWindow window)
        {
            CloseExistingCompletionWindow();
            _activeCompletionWindow = window;
            window.Closed += (sender, args) => _activeCompletionWindow = null;
            data.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(
                                                                       () =>
                                                                           {
                                                                               if (SharpDevelopCompletionWindow ==
                                                                                   window)
                                                                               {
                                                                                   window.Show();
                                                                               }
                                                                           }
                                                                       ));
        }

        private void CloseExistingInsightWindow()
        {
            if (ActiveInsightWindow != null)
            {
                ActiveInsightWindow.Close();
            }
        }

        public override IInsightWindow ShowInsightWindow(IEnumerable<IInsightItem> items)
        {
            if (items == null)
                return null;
            var insightWindow = new SharpDevelopInsightWindow(TextEditor.TextArea);
            foreach (IInsightItem insightItem in items)
            {
                insightWindow.Items.Add(insightItem);
            }
            if (insightWindow.Items.Count > 0)
            {
                insightWindow.SelectedItem = insightWindow.Items[0];
            }
            else
            {
                return null;
            }
            ShowInsightWindow(textEditor, insightWindow);
            return insightWindow;
        }

        private void ShowInsightWindow(TextEditor items, SharpDevelopInsightWindow insightWindow)
        {
            CloseExistingInsightWindow();
            _activeInsightWindow = insightWindow;
            insightWindow.Closed += delegate { _activeInsightWindow = null; };
            items.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(
                                                                        delegate
                                                                            {
                                                                                if (ActiveInsightWindow ==
                                                                                    insightWindow)
                                                                                {
                                                                                    insightWindow.Show();
                                                                                }
                                                                            }
                                                                        ));
        }
    }
}