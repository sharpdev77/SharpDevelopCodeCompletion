using System.Collections.Generic;
using System.Linq;
using ICSharpCode.AvalonEdit.CodeCompletion.CompetionItems.Providers;
using ICSharpCode.SharpDevelop.Dom;

namespace ICSharpCode.AvalonEdit.CodeCompletion
{
    public abstract class CtrlSpaceCompletionItemProvider : CodeCompletionItemProvider
    {
        private readonly ExpressionContext overrideContext;
        private int preselectionLength;

        public CtrlSpaceCompletionItemProvider(IProjectContent projectContent) : base(projectContent)
        {
        }

        public CtrlSpaceCompletionItemProvider(ExpressionContext overrideContext, IProjectContent projectContent)
            : base(projectContent)
        {
            this.overrideContext = overrideContext;
        }

        /// <summary>
        /// Gets/Sets whether completing an old expression is allowed.
        /// You have to set this property to true to let the provider run FindExpression, when
        /// set to false it will use ExpressionContext.Default (unless the constructor with "overrideContext" was used).
        /// </summary>
        public bool AllowCompleteExistingExpression { get; set; }

        /// <summary>
        /// Gets/Sets whether code templates should be included in code completion.
        /// </summary>
        public bool ShowTemplates { get; set; }

        private void AddTemplates(ITextEditor editor, DefaultCompletionItemList list)
        {
            if (list == null)
                return;
            List<ICompletionItem> snippets = editor.GetSnippets().ToList();
            snippets.RemoveAll(item => !FitsToContext(item, list.Items));
            list.Items.RemoveAll(item => snippets.Exists(i => i.Text == item.Text));
            list.Items.AddRange(snippets);
            list.SortItems();
        }

        private bool FitsToContext(ICompletionItem item, List<ICompletionItem> list)
        {
            if (!(item is ISnippetCompletionItem))
                return false;

            var snippetItem = item as ISnippetCompletionItem;

            if (string.IsNullOrEmpty(snippetItem.Keyword))
                return true;

            return list.Any(x => x.Text == snippetItem.Keyword);
        }

        public override ICompletionItemList GenerateCompletionList(ITextEditor editor, IProjectContent projectContent)
        {
            ICompletionItemList list = GenerateCompletionListCore(editor);
            if (ShowTemplates)
                AddTemplates(editor, list as DefaultCompletionItemList);
            return list;
        }

        private ICompletionItemList GenerateCompletionListCore(ITextEditor editor)
        {
            preselectionLength = 0;
            if (!AllowCompleteExistingExpression)
            {
                ExpressionContext context = overrideContext ?? ExpressionContext.Default;
                List<ICompletionEntry> ctrlSpace = CtrlSpace(editor, context);
                return GenerateCompletionListForCompletionData(ctrlSpace, context, ProjectContent);
            }

            ExpressionResult expressionResult = GetExpression(editor);
            string expression = expressionResult.Expression;
            if (expression == null || expression.Length == 0)
            {
                List<ICompletionEntry> ctrlSpace = CtrlSpace(editor, expressionResult.Context);
                return GenerateCompletionListForCompletionData(ctrlSpace, expressionResult.Context, ProjectContent);
            }

            int idx = expression.LastIndexOf('.');
            if (idx > 0)
            {
                preselectionLength = expression.Length - (idx + 1);
                expressionResult.Expression = expression.Substring(0, idx);
                return GenerateCompletionListForExpression(editor, expressionResult);
            }
            else
            {
                preselectionLength = expression.Length;
                List<ICompletionEntry> results = CtrlSpace(editor, expressionResult.Context);
                return GenerateCompletionListForCompletionData(results, expressionResult.Context, ProjectContent);
            }
        }

        protected abstract List<ICompletionEntry> CtrlSpace(ITextEditor editor, ExpressionContext context);

        protected override void InitializeCompletionItemList(DefaultCompletionItemList list)
        {
            base.InitializeCompletionItemList(list);
            list.PreselectionLength = preselectionLength;
        }
    }
}