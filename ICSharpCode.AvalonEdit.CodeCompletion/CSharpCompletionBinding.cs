using System.Collections.Generic;
using ICSharpCode.AvalonEdit.CodeCompletion.CompetionItems;
using ICSharpCode.AvalonEdit.CodeCompletion.CompetionItems.Providers;
using ICSharpCode.NRefactory;
using ICSharpCode.NRefactory.Visitors;
using ICSharpCode.SharpDevelop.Dom;
using ICSharpCode.SharpDevelop.Dom.CSharp;
using ICSharpCode.SharpDevelop.Dom.NRefactoryResolver;
using ICSharpCode.SharpDevelop.Dom.Refactoring;

namespace ICSharpCode.AvalonEdit.CodeCompletion
{
    public class CSharpCompletionBinding : NRefactoryCodeCompletionBinding
    {
        public CSharpCompletionBinding(IProjectContent projectContent, ICompletionItemProviderFactory completionItemProviderFactory)
            : base(SupportedLanguage.CSharp, projectContent, completionItemProviderFactory)
        {
        }


        public IFilterStrategy FilterStrategy { private get; set; }
        
        CSharpExpressionFinder CreateExpressionFinder(string fileName)
        {
            return new CSharpExpressionFinder(ParserService.GetParseInformation(fileName, ProjectContent));
        }

        public override CodeCompletionKeyPressResult HandleKeyPress(ITextEditor editor, char ch, IProjectContent projectContent)
        {
            CSharpExpressionFinder ef = CreateExpressionFinder(editor.Document.Text);
            int cursor = editor.Caret.Offset;
            CodeCompletionKeyPressResult? returnResult = null;
            switch (ch)
            {
                case '[':
                    {
                        var line = editor.Document.GetLineForOffset(cursor);
                        /* TODO: AVALONEDIT Reimplement this
                if (TextUtilities.FindPrevWordStart(editor.ActiveTextAreaControl.Document, cursor) <= line.Offset) {
                    // [ is first character on the line
                    // -> Attribute completion
                    editor.ShowCompletionWindow(new AttributesDataProvider(ParserService.CurrentProjectContent), ch);
                    return true;
                }*/
                    }
                    break;
                case ' ':
                    returnResult = CompleteSpace(editor, ef);
                    break;
                case ',':
                    returnResult = CompleteComma(editor, ch, projectContent);
                    break;
                case '=':
                    returnResult = CompleteAssigment(editor, ch, ef);
                    break;
                case '.':
                    returnResult = ShowDotCompletion(editor);
                    break;
                case '>':
                    {
                        if (IsInComment(editor))
                        {
                            returnResult = CodeCompletionKeyPressResult.None;
                            break;
                        }
                        returnResult = CompleteMore(editor, cursor, projectContent);
                    }
                    break;
            }

            if (returnResult.HasValue)
                return returnResult.Value;

            if (char.IsLetter(ch) || ch=='_')
            {
                if (editor.SelectionLength > 0)
                {
                    // allow code completion when overwriting an identifier
                    int endOffset = editor.SelectionStart + editor.SelectionLength;
                    // but block code completion when overwriting only part of an identifier
                    if (endOffset < editor.Document.TextLength && char.IsLetterOrDigit(editor.Document.GetCharAt(endOffset)))
                        return CodeCompletionKeyPressResult.None;

                    editor.Document.Remove(editor.SelectionStart, editor.SelectionLength);
                    // Read caret position again after removal - this is required because the document might change in other
                    // locations, too (e.g. bound elements in snippets).
                    cursor = editor.Caret.Offset;
                }
                var prevChar = GetPreviousChar(editor, cursor);
                var afterUnderscore = prevChar == '_';
                if (afterUnderscore)
                {
                    cursor--;
                    prevChar = GetPreviousChar(editor, cursor);
                }

                if (!char.IsLetterOrDigit(prevChar) && prevChar != '.' && !IsInComment(editor))
                {
                    var result = ef.FindExpression(editor.Document.Text, cursor);
                    if (result.Context != ExpressionContext.IdentifierExpected)
                    {
                        var ctrlSpaceProvider = new NRefactoryCtrlSpaceCompletionItemProvider(
                            LanguageProperties.CSharp, result.Context, ProjectContent)
                                                    {
                                                        ShowTemplates = true,
                                                        AllowCompleteExistingExpression = afterUnderscore
                                                    };
                        ShowCompletion(ctrlSpaceProvider, editor, projectContent);
                        return CodeCompletionKeyPressResult.CompletedIncludeKeyInCompletion;
                    }
                }
            }
            return base.HandleKeyPress(editor, ch, projectContent);
        }

        private CodeCompletionKeyPressResult? CompleteSpace(ITextEditor editor, IExpressionFinder ef)
        {
            CodeCompletionKeyPressResult? returnResult = null;
            var documentText = editor.Document.Text;
            var position = editor.Caret.Offset - 3;

            if (position > 0 && (documentText[position + 1] == '+') && documentText[position + 2] == '=')
            {
                var result = ef.FindFullExpression(documentText, position);

                if (result.Expression != null)
                {
                    var resolveResult = ParserService.Resolve(result, editor.Caret.Line, editor.Caret.Column,
                                                                        editor.Document.Text, documentText, ProjectContent);
                    if (resolveResult != null && resolveResult.ResolvedType != null)
                    {
                        var underlyingClass = resolveResult.ResolvedType.GetUnderlyingClass();
                        if (underlyingClass != null &&
                            underlyingClass.IsTypeInInheritanceTree(
                                ProjectContent.GetClass("System.MulticastDelegate", 0)))
                        {
                            var eventHandlerProvider =
                                new EventHandlerCompletionItemProvider(result.Expression, resolveResult);
                            ShowCompletion(eventHandlerProvider, editor, ProjectContent);
                            returnResult = CodeCompletionKeyPressResult.Completed;
                        }
                    }
                }
            }
            return returnResult;
        }

        private static char GetPreviousChar(ITextEditor editor, int cursor)
        {
            return cursor > 1 ? editor.Document.GetCharAt(cursor - 1) : ' ';
        }

        private CodeCompletionKeyPressResult? CompleteMore(ITextEditor editor, int cursor, IProjectContent projectContent)
        {
            CodeCompletionKeyPressResult? returnResult=null;
            char prevChar = cursor > 1 ? editor.Document.GetCharAt(cursor - 1) : ' ';
            if (prevChar == '-')
            {
                ShowCompletion(new PointerArrowCompletionDataProvider(projectContent), editor, projectContent);

                returnResult = CodeCompletionKeyPressResult.Completed;
            }
            return returnResult;
        }

        private CodeCompletionKeyPressResult? CompleteAssigment(ITextEditor editor, char ch, CSharpExpressionFinder ef)
        {
            CodeCompletionKeyPressResult? returnResult = null;
            string documentText = editor.Document.Text;
            int position = editor.Caret.Offset - 2;

            if (position > 0)
            {
                ExpressionResult result = ef.FindFullExpression(documentText, position);

                if (result.Expression != null)
                {
                    ResolveResult resolveResult = ParserService.Resolve(result, editor.Caret.Line, editor.Caret.Column,
                                                                        editor.Document.Text, documentText, ProjectContent);
                    if (resolveResult != null && resolveResult.ResolvedType != null)
                    {
                        if (ProvideContextCompletion(editor, resolveResult.ResolvedType, ch))
                        {
                            returnResult = CodeCompletionKeyPressResult.Completed;
                        }
                    }
                }
            }
            return returnResult;
        }

        private CodeCompletionKeyPressResult? CompleteComma(ITextEditor editor, char ch, IProjectContent projectContent)
        {
            IInsightWindow insightWindow;
            if (insightHandler.InsightRefreshOnComma(editor, ch, out insightWindow, projectContent))
            {
                return CodeCompletionKeyPressResult.Completed;
            }
            return null;
        }

        private CodeCompletionKeyPressResult ShowDotCompletion(ITextEditor editor)
        {
            ShowCompletion(new CSharpCodeCompletionDataProvider(ProjectContent), editor, ProjectContent);
            return CodeCompletionKeyPressResult.Completed;
        }

        class CSharpCodeCompletionDataProvider : DotCodeCompletionItemProvider
        {
            public CSharpCodeCompletionDataProvider(IProjectContent projectContent)
                : base(projectContent)
            {
                
            }

            public override ResolveResult Resolve(ITextEditor editor, ExpressionResult expressionResult)
            {
                // bypass ParserService.Resolve and set resolver.LimitMethodExtractionUntilCaretLine
                ParseInformation parseInfo = ParserService.GetParseInformation(editor.Document.Text, ProjectContent);
                var resolver = new NRefactoryResolver(LanguageProperties.CSharp)
                                   {
                                       LimitMethodExtractionUntilLine = editor.Caret.Line
                                   };
                return resolver.Resolve(expressionResult, parseInfo, editor.Document.Text);
            }
        }

        class PointerArrowCompletionDataProvider : DotCodeCompletionItemProvider
        {
            public PointerArrowCompletionDataProvider(IProjectContent projectContent) : base(projectContent)
            {
            }

            public override ResolveResult Resolve(ITextEditor editor, ExpressionResult expressionResult)
            {
                var rr = base.Resolve(editor, expressionResult);
                if (rr != null && rr.ResolvedType != null)
                {
                    var prt = rr.ResolvedType.CastToDecoratingReturnType<PointerReturnType>();
                    if (prt != null)
                        return new ResolveResult(rr.CallingClass, rr.CallingMember, prt.BaseType);
                }
                return null;
            }

            public override ExpressionResult GetExpression(ITextEditor editor)
            {
                // - 1 because the "-" is already inserted (the ">" is about to be inserted)
                return GetExpressionFromOffset(editor, editor.Caret.Offset - 1);
            }
        }

        bool IsInComment(ITextEditor editor)
        {
            CSharpExpressionFinder ef = CreateExpressionFinder(editor.Document.Text);
            int cursor = editor.Caret.Offset - 1;
            return ef.FilterComments(editor.Document.GetText(0, cursor + 1), ref cursor) == null;
        }

        public override bool HandleKeyword(ITextEditor editor, string word)
        {
            switch (word)
            {
                case "using":
                    if (IsInComment(editor)) return false;

                    ParseInformation parseInfo = ParserService.GetParseInformation(editor.Document.Text, ProjectContent);
                    if (parseInfo != null)
                    {
                        IClass innerMostClass = parseInfo.CompilationUnit.GetInnermostClass(editor.Caret.Line, editor.Caret.Column);
                        if (innerMostClass == null)
                        {
                            ShowCompletion(CompletionItemProviderFactory.Create(LanguageProperties.CSharp, ExpressionContext.Namespace, ProjectContent), editor, ProjectContent);
                            return true;
                        }
                    }
                    break;
                case "as":
                case "is":
                    if (IsInComment(editor)) return false;
                    ShowCompletion(CompletionItemProviderFactory.Create(LanguageProperties.CSharp, ExpressionContext.Type, ProjectContent), editor, ProjectContent);
                    return true;
                case "override":
                    if (IsInComment(editor)) return false;
                    ShowCompletion(new OverrideCompletionItemProvider(ProjectContent), editor, ProjectContent);
                    return true;
                case "new":
                    return ShowNewCompletion(editor);
                case "case":
                    if (IsInComment(editor)) return false;
                    return DoCaseCompletion(editor);
                case "return":
                    if (IsInComment(editor)) return false;
                    IMember m = GetCurrentMember(editor);
                    if (m != null)
                    {
                        return ProvideContextCompletion(editor, m.ReturnType, ' ');
                    }
                    break;
            }
            return false;
        }

        protected override ICompletionItemList FilterList(ICompletionItemList itemList)
        {
            if (FilterStrategy == null)
                return itemList;
            var defaultCompletionItemList = new DefaultCompletionItemList
                {SuggestedItem = itemList.SuggestedItem, PreselectionLength = itemList.PreselectionLength};
            defaultCompletionItemList.Items.AddRange(FilterStrategy.Filter(itemList.Items));
            return defaultCompletionItemList;
        }

        bool ShowNewCompletion(ITextEditor editor)
        {
            CSharpExpressionFinder ef = CreateExpressionFinder(editor.Document.Text);
            int cursor = editor.Caret.Offset;
            string documentToCursor = editor.Document.GetText(0, cursor);
            ExpressionResult expressionResult = ef.FindExpression(documentToCursor, cursor);

            if (expressionResult.Context.IsObjectCreation)
            {
                var currentLine = editor.Document.GetLineForOffset(cursor);
                string lineText = editor.Document.GetText(currentLine.Offset, cursor - currentLine.Offset);
                // when the new follows an assignment, improve code-completion by detecting the
                // type of the variable that is assigned to
                if (lineText.Replace(" ", "").EndsWith("=new"))
                {
                    int pos = lineText.LastIndexOf('=');
                    ExpressionContext context = FindExactContextForNewCompletion(editor, documentToCursor,
                        currentLine, pos);
                    if (context != null)
                        expressionResult.Context = context;
                }
                ShowCompletion(new NRefactoryCtrlSpaceCompletionItemProvider(LanguageProperties.CSharp, expressionResult.Context, ProjectContent), editor, ProjectContent);
                return true;
            }
            return false;
        }

        ExpressionContext FindExactContextForNewCompletion(ITextEditor editor, string documentToCursor,
                                                           IDocumentLine currentLine, int pos)
        {
            CSharpExpressionFinder ef = CreateExpressionFinder(editor.Document.Text);
            // find expression on left hand side of the assignment
            ExpressionResult lhsExpr = ef.FindExpression(documentToCursor, currentLine.Offset + pos);
            if (lhsExpr.Expression != null)
            {
                ResolveResult rr = ParserService.Resolve(lhsExpr, currentLine.LineNumber, pos, editor.Document.Text, editor.Document.Text, ProjectContent);
                if (rr != null && rr.ResolvedType != null)
                {
                    ExpressionContext context;
                    IClass c;
                    if (rr.ResolvedType.IsArrayReturnType)
                    {
                        // when creating an array, all classes deriving from the array's element type are allowed
                        IReturnType elementType = rr.ResolvedType.CastToArrayReturnType().ArrayElementType;
                        c = elementType != null ? elementType.GetUnderlyingClass() : null;
                        context = ExpressionContext.TypeDerivingFrom(elementType, false);
                    }
                    else
                    {
                        // when creating a normal instance, all non-abstract classes deriving from the type
                        // are allowed
                        c = rr.ResolvedType.GetUnderlyingClass();
                        context = ExpressionContext.TypeDerivingFrom(rr.ResolvedType, true);
                    }
                    if (c != null && context.ShowEntry(c))
                    {
                        // Try to suggest an entry (List<int> a = new => suggest List<int>).

                        string suggestedClassName = LanguageProperties.CSharp.CodeGenerator.GenerateCode(
                            CodeGenerator.ConvertType(
                                rr.ResolvedType,
                                new ClassFinder(ParserService.GetParseInformation(editor.Document.Text, ProjectContent), editor.Caret.Line, editor.Caret.Column)
                                ), "");
                        context.SuggestedItem = suggestedClassName != c.Name 
                            ? new SuggestedCodeCompletionItem(c, suggestedClassName, ProjectContent) 
                            : new CodeCompletionItem(c, ProjectContent);
                    }
                    return context;
                }
            }
            return null;
        }

        #region "case"-keyword completion
        bool DoCaseCompletion(ITextEditor editor)
        {
            ITextEditorCaret caret = editor.Caret;
            var r = new NRefactoryResolver(LanguageProperties.CSharp);
            if (r.Initialize(ParserService.GetParseInformation(editor.Document.Text, ProjectContent), caret.Line, caret.Column))
            {
                NRefactory.Ast.INode currentMember = r.ParseCurrentMember(editor.Document.Text);
                if (currentMember != null)
                {
                    var ccsf = new CaseCompletionSwitchFinder(caret.Line, caret.Column);
                    currentMember.AcceptVisitor(ccsf, null);
                    if (ccsf.BestStatement != null)
                    {
                        r.RunLookupTableVisitor(currentMember);
                        ResolveResult rr = r.ResolveInternal(ccsf.BestStatement.SwitchExpression, ExpressionContext.Default);
                        if (rr != null && rr.ResolvedType != null)
                        {
                            return ProvideContextCompletion(editor, rr.ResolvedType, ' ');
                        }
                    }
                }
            }
            return false;
        }

        private class CaseCompletionSwitchFinder : AbstractAstVisitor
        {
            readonly Location _caretLocation;
            internal NRefactory.Ast.SwitchStatement BestStatement;

            public CaseCompletionSwitchFinder(int caretLine, int caretColumn)
            {
                _caretLocation = new Location(caretColumn, caretLine);
            }

            public override object VisitSwitchStatement(NRefactory.Ast.SwitchStatement switchStatement, object data)
            {
                if (switchStatement.StartLocation < _caretLocation && _caretLocation < switchStatement.EndLocation)
                {
                    BestStatement = switchStatement;
                }
                return base.VisitSwitchStatement(switchStatement, data);
            }
        }
        #endregion
    }

    public interface IFilterStrategy
    {
        IEnumerable<ICompletionItem> Filter(IEnumerable<ICompletionItem> completionItems);
    }
}