using System.Linq;
using ICSharpCode.AvalonEdit.CodeCompletion.CompetionItems;
using ICSharpCode.AvalonEdit.CodeCompletion.CompetionItems.Providers;
using ICSharpCode.NRefactory;
using ICSharpCode.SharpDevelop.Dom;
using ICSharpCode.SharpDevelop.Dom.NRefactoryResolver;

namespace ICSharpCode.AvalonEdit.CodeCompletion
{
    /// <summary>
    /// Base class for C# and VB Code Completion Binding.
    /// </summary>
    public abstract class NRefactoryCodeCompletionBinding : DefaultCodeCompletionBinding
    {
        readonly LanguageProperties _languageProperties;
        protected readonly IProjectContent ProjectContent;
        protected readonly ICompletionItemProviderFactory CompletionItemProviderFactory;

        protected NRefactoryCodeCompletionBinding(SupportedLanguage language, 
            IProjectContent projectContent, 
            ICompletionItemProviderFactory completionItemProviderFactory)
        {
            ProjectContent = projectContent;
            CompletionItemProviderFactory = completionItemProviderFactory;
            _languageProperties = language == SupportedLanguage.CSharp ? LanguageProperties.CSharp : LanguageProperties.VBNet;

            insightHandler = new NRefactoryInsightWindowHandler(language, ProjectContent);
        }

        public override bool CtrlSpace(ITextEditor editor)
        {
            var provider = CompletionItemProviderFactory.Create(_languageProperties, ProjectContent);
            provider.AllowCompleteExistingExpression = true;
            provider.ShowItemsFromAllNamespaces = false;
            // on Ctrl+Space, include items (e.g. types / extension methods) from all namespaces, regardless of imports
            ShowCompletion(provider, editor, ProjectContent);
            return true;
        }

        protected bool ProvideContextCompletion(ITextEditor editor, IReturnType expected, char charTyped)
        {
            if (expected == null) return false;
            var c = expected.GetUnderlyingClass();
            if (c == null) return false;
            if (c.ClassType == ClassType.Enum)
            {
                CtrlSpaceCompletionItemProvider cdp = new NRefactoryCtrlSpaceCompletionItemProvider(_languageProperties, ProjectContent);
                var ctrlSpaceList = cdp.GenerateCompletionList(editor, ProjectContent);
                if (ctrlSpaceList == null) return false;
                var contextList = new ContextCompletionItemList();
                contextList.Items.AddRange(ctrlSpaceList.Items);
                contextList.ActivationKey = charTyped;
                foreach (CodeCompletionItem item in contextList.Items.OfType<CodeCompletionItem>())
                {
                    var itemClass = item.Entity as IClass;
                    if (itemClass != null && c.FullyQualifiedName == itemClass.FullyQualifiedName && c.TypeParameters.Count == itemClass.TypeParameters.Count)
                    {
                        contextList.SuggestedItem = item;
                        break;
                    }
                }
                if (contextList.SuggestedItem != null)
                {
                    if (charTyped != ' ') contextList.InsertSpace = true;
                    editor.ShowCompletionWindow(contextList);
                    return true;
                }
            }
            return false;
        }

        private class ContextCompletionItemList : DefaultCompletionItemList
        {
            internal char ActivationKey;

            public override CompletionItemListKeyResult ProcessInput(char key)
            {
                if (key == '=' && ActivationKey == '=')
                    return CompletionItemListKeyResult.BeforeStartKey;
                ActivationKey = '\0';
                return base.ProcessInput(key);
            }
        }

        protected IMember GetCurrentMember(ITextEditor editor)
        {
            var caret = editor.Caret;
            var r = new NRefactoryResolver(_languageProperties);
            var parseInformation = ParserService.GetParseInformation(editor.Document.Text, ProjectContent);
            return r.Initialize(parseInformation, caret.Line, caret.Column) ? r.CallingMember : null;
        }
    }
}