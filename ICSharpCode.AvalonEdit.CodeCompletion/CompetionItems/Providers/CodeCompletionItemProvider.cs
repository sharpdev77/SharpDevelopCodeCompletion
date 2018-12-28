using System;
using System.Collections.Generic;
using System.Linq;
using ICSharpCode.SharpDevelop.Dom;
using ICSharpCode.SharpDevelop.Dom.NRefactoryResolver;

namespace ICSharpCode.AvalonEdit.CodeCompletion.CompetionItems.Providers
{
    /// <summary>
    ///     Allows creating a <see cref="ICompletionDataList" /> from code-completion information.
    /// </summary>
    public class CodeCompletionItemProvider : ICompletionItemProvider
    {
        protected readonly IProjectContent ProjectContent;

        protected CodeCompletionItemProvider(IProjectContent projectContent)
        {
            ProjectContent = projectContent;
        }

        /// <summary>
        ///     Gets/Sets whether items from all namespaces should be included in code completion, regardless of imports.
        /// </summary>
        public virtual bool ShowItemsFromAllNamespaces { get; set; }

        /// <inheritdoc />
        public virtual ICompletionItemList GenerateCompletionList(ITextEditor editor, IProjectContent projectContent)
        {
            if (editor == null)
                throw new ArgumentNullException("textEditor");
            var expression = GetExpression(editor);
            return GenerateCompletionListForExpression(editor, expression);
        }

        public virtual ExpressionResult GetExpression(ITextEditor editor)
        {
            return GetExpressionFromOffset(editor, editor.Caret.Offset);
        }

        protected ExpressionResult GetExpressionFromOffset(ITextEditor editor, int offset)
        {
            if (editor == null)
                throw new ArgumentNullException("editor");
            var document = editor.Document;
            var expressionFinder = ParserService.GetExpressionFinder(editor.Document.Text, ProjectContent);
            if (expressionFinder == null)
            {
                return ExpressionResult.Empty;
            }
            else
            {
                return expressionFinder.FindExpression(document.Text, offset);
            }
        }

        public virtual ICompletionItemList GenerateCompletionListForExpression(ITextEditor editor,
                                                                               ExpressionResult expressionResult)
        {
            if (expressionResult.Expression == null)
            {
                return null;
            }
            var rr = Resolve(editor, expressionResult);
            return GenerateCompletionListForResolveResult(rr, expressionResult.Context);
        }

        public virtual ResolveResult Resolve(ITextEditor editor, ExpressionResult expressionResult)
        {
            if (editor == null)
                throw new ArgumentNullException("editor");
            return ParserService.Resolve(expressionResult, editor.Caret.Line, editor.Caret.Column, editor.Document.Text,
                                         editor.Document.Text, ProjectContent);
        }

        public virtual ICompletionItemList GenerateCompletionListForResolveResult(ResolveResult rr,
                                                                                  ExpressionContext context)
        {
            if (rr == null)
                return null;
            var callingContent = rr.CallingClass != null ? rr.CallingClass.ProjectContent : null;
            var arr = rr.GetCompletionData(callingContent ?? ProjectContent,
                                           ShowItemsFromAllNamespaces);
            return GenerateCompletionListForCompletionData(arr, context, ProjectContent);
        }

        protected virtual DefaultCompletionItemList CreateCompletionItemList()
        {
            // This is overriden in DotCodeCompletionItemProvider (C# and VB dot completion)
            // and NRefactoryCtrlSpaceCompletionItemProvider (C# and VB Ctrl+Space completion)
            return new DefaultCompletionItemList();
        }

        protected virtual void InitializeCompletionItemList(DefaultCompletionItemList list)
        {
            list.SortItems();
        }

        public virtual ICompletionItemList GenerateCompletionListForCompletionData(List<ICompletionEntry> entries,
                                                                                   ExpressionContext context,
                                                                                   IProjectContent projectContent) {
            entries = GetButObsoleteItems(entries);

            var list = ConvertCompletionData(CreateCompletionItemList(), entries, context, projectContent);
            InitializeCompletionItemList(list);

            return list;
        }

        private List<ICompletionEntry> GetButObsoleteItems(IEnumerable<ICompletionEntry> entries){
            return entries == null ? null : (entries.Where(entry => !(entry is IEntity && ((IEntity) entry).IsObsolete))).ToList();
        }

        public static DefaultCompletionItemList ConvertCompletionData(DefaultCompletionItemList result,
                                                                      IEnumerable<ICompletionEntry> arr,
                                                                      ExpressionContext context,
                                                                      IProjectContent projectContent)
        {
            if (arr == null)
                return result;

            var methodItems = new Dictionary<string, CodeCompletionItem>();
            foreach (var o in arr)
            {
                if (context != null && !context.ShowEntry(o))
                    continue;

                var method = o as IMethod;
                var item = CreateCompletionItem(o, context, projectContent);

                if (method != null)
                {
                    CodeCompletionItem codeItem;
                    if (methodItems.TryGetValue(method.Name, out codeItem))
                    {
                        codeItem.Overloads++;

                        codeItem.Description.OverloadHeaders.Add(item.Description.Header);
                        continue;
                    }
                }

                if (item != null)
                {
                    result.Items.Add(item);
                    var codeItem = item as CodeCompletionItem;
                    if (method != null && codeItem != null)
                    {
                        methodItems[method.Name] = codeItem;
                    }
                }
            }

            // Suggested entry (List<int> a = new => suggest List<int>).
            if (context.SuggestedItem is SuggestedCodeCompletionItem)
            {
                result.SuggestedItem = (SuggestedCodeCompletionItem) context.SuggestedItem;
                result.Items.Insert(0, result.SuggestedItem);
            }
            return result;
        }

        private static ICompletionItem CreateCompletionItem(object o, ExpressionContext context,
                                                            IProjectContent projectContent)
        {
            if (o is IEntity) return new CodeCompletionItem((IEntity) o, projectContent);
            if (o is KeywordEntry) return new KeywordCompletionItem(o.ToString());
            if (o is NamespaceEntry) return new NamespaceCompletionItem((NamespaceEntry) o);

            return new DefaultCompletionItem(o.ToString());
        }
    }
}