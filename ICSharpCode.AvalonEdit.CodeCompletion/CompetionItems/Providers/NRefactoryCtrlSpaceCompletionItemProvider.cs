using System;
using System.Collections.Generic;
using ICSharpCode.SharpDevelop.Dom;
using ICSharpCode.SharpDevelop.Dom.NRefactoryResolver;

namespace ICSharpCode.AvalonEdit.CodeCompletion.CompetionItems.Providers
{
    public class NRefactoryCtrlSpaceCompletionItemProvider : CtrlSpaceCompletionItemProvider
    {
        private readonly LanguageProperties _language;
        private readonly IProjectContent _projectContent;

        public NRefactoryCtrlSpaceCompletionItemProvider(LanguageProperties language, IProjectContent projectContent)
            : base(projectContent)
        {
            if (language == null)
                throw new ArgumentNullException("language");
            _language = language;
            _projectContent = projectContent;
        }

        public NRefactoryCtrlSpaceCompletionItemProvider(LanguageProperties language, ExpressionContext overrideContext, IProjectContent projectContent)
            : base(overrideContext, projectContent)
        {
            if (language == null)
                throw new ArgumentNullException("language");
            _language = language;
            _projectContent = projectContent;
        }

        protected override DefaultCompletionItemList CreateCompletionItemList()
        {
            return new NRefactoryCompletionItemList {ContainsItemsFromAllNamespaces = ShowItemsFromAllNamespaces};
        }

        protected override List<ICompletionEntry> CtrlSpace(ITextEditor editor, ExpressionContext context)
        {
            var resolver = new NRefactoryResolver(_language);
            return resolver.CtrlSpace(
                editor.Caret.Line, editor.Caret.Column,
                ParserService.GetParseInformation(editor.Document.Text, _projectContent),
                editor.Document.Text,
                context, ShowItemsFromAllNamespaces);
        }
    }
}