using System;
using ICSharpCode.AvalonEdit.CodeCompletion.CompetionItems.Providers;
using ICSharpCode.AvalonEdit.CodeCompletion.Indexers;
using ICSharpCode.SharpDevelop.Dom;

namespace ICSharpCode.AvalonEdit.CodeCompletion
{
    public class DefaultCodeCompletionBinding : ICodeCompletionBinding
    {
        bool enableMethodInsight = true;
        bool enableIndexerInsight = true;
        bool enableXmlCommentCompletion = true;
        bool enableDotCompletion = true;
        protected IInsightWindowHandler insightHandler;

        public bool EnableMethodInsight
        {
            get
            {
                return enableMethodInsight;
            }
            set
            {
                enableMethodInsight = value;
            }
        }

        public bool EnableIndexerInsight
        {
            get
            {
                return enableIndexerInsight;
            }
            set
            {
                enableIndexerInsight = value;
            }
        }

        public bool EnableXmlCommentCompletion
        {
            get
            {
                return enableXmlCommentCompletion;
            }
            set
            {
                enableXmlCommentCompletion = value;
            }
        }

        public bool EnableDotCompletion
        {
            get
            {
                return enableDotCompletion;
            }
            set
            {
                enableDotCompletion = value;
            }
        }

        public virtual CodeCompletionKeyPressResult HandleKeyPress(ITextEditor editor, char ch, IProjectContent projectContent)
        {
            IInsightWindow insightWindow = null;
            switch (ch)
            {
                case '(':
                    insightWindow = editor.ShowInsightWindow(new MethodInsightProvider(projectContent).ProvideInsight(editor));
                    if (insightWindow != null && insightHandler != null)
                    {
                        insightHandler.InitializeOpenedInsightWindow(editor, insightWindow);
                        insightHandler.HighlightParameter(insightWindow, 0);
                    }
                    return CodeCompletionKeyPressResult.Completed;
                    break;
                case '[':
                    insightWindow = editor.ShowInsightWindow(new IndexerInsightProvider(projectContent).ProvideInsight(editor));
                    if (insightWindow != null && insightHandler != null)
                        insightHandler.InitializeOpenedInsightWindow(editor, insightWindow);
                    return CodeCompletionKeyPressResult.Completed;
                    break;
                case '<':
                    ShowCompletion(new CommentCompletionItemProvider(), editor,projectContent);
                    return CodeCompletionKeyPressResult.Completed;
                    break;
                case '.':
                    ShowCompletion(new DotCodeCompletionItemProvider(projectContent), editor,projectContent);
                    return CodeCompletionKeyPressResult.Completed;
                    break;
                case ' ':
                    string word = editor.GetWordBeforeCaret();
                    if (!String.IsNullOrEmpty(word))
                    {
                        if (HandleKeyword(editor, word))
                            return CodeCompletionKeyPressResult.Completed;
                    }
                    break;
            }
            return CodeCompletionKeyPressResult.None;
        }

        public virtual bool HandleKeyword(ITextEditor editor, string word)
        {
            // DefaultCodeCompletionBinding does not support Keyword handling, but this
            // method can be overridden
            return false;
        }

        public virtual bool CtrlSpace(ITextEditor editor)
        {
            return false;
        }

        /// <summary>
        /// Shows code completion for the specified editor.
        /// </summary>
        protected void ShowCompletion(ICompletionItemProvider completionItemProvider, ITextEditor editor, IProjectContent projectContent)
        {
            if (editor == null)
                throw new ArgumentNullException("editor");
            ICompletionItemList itemList = completionItemProvider.GenerateCompletionList(editor, projectContent);

            if (itemList != null)
                editor.ShowCompletionWindow(FilterList(itemList));
        }

        protected virtual ICompletionItemList FilterList(ICompletionItemList itemList)
        {
            return itemList;
        }
    }
}