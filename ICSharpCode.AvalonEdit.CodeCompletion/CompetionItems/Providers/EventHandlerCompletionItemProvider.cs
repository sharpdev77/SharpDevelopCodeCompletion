using System.Collections.Generic;
using System.Linq;
using System.Text;
using ICSharpCode.AvalonEdit.CodeCompletion.Interface.Description;
using ICSharpCode.NRefactory.Parser.CSharp;
using ICSharpCode.SharpDevelop.Dom;
using ICSharpCode.SharpDevelop.Dom.CSharp;

namespace ICSharpCode.AvalonEdit.CodeCompletion.CompetionItems.Providers
{
    public sealed class EventHandlerCompletionItemProvider : ICompletionItemProvider
    {
        private static readonly CSharpAmbience DefaulAmbience = new CSharpAmbience
                                                                    {
                                                                        ConversionFlags =
                                                                            ConversionFlags.ShowParameterNames |
                                                                            ConversionFlags.ShowTypeParameterList
                                                                    };

        private static readonly CSharpAmbience FullyQualifiedAmbience = new CSharpAmbience
                                                                            {
                                                                                ConversionFlags =
                                                                                    ConversionFlags.ShowParameterNames |
                                                                                    ConversionFlags.
                                                                                        ShowTypeParameterList |
                                                                                    ConversionFlags.
                                                                                        UseFullyQualifiedTypeNames
                                                                            };

        private readonly ResolveResult _resolveResult;
        private readonly IClass _resolvedClass;
        private readonly IReturnType _resolvedReturnType;
        private string _expression;

        public EventHandlerCompletionItemProvider(string expression, ResolveResult resolveResult)
        {
            _expression = expression;
            _resolveResult = resolveResult;
            _resolvedReturnType = resolveResult.ResolvedType;
            _resolvedClass = _resolvedReturnType.GetUnderlyingClass();
        }

        #region ICompletionItemProvider Members

        public ICompletionItemList GenerateCompletionList(ITextEditor editor, IProjectContent projectContent)
        {
            var result = new DefaultCompletionItemList {InsertSpace = false};

            var delegateSignature = _resolvedReturnType.GetMethods().Find(m => m.Name == "Invoke");

            if (delegateSignature != null)
            {
                result.Items.Add(GenerateCreateNewMember(delegateSignature, _resolveResult));

                result.Items.AddRange(GenerateFromCallingClass(projectContent, delegateSignature));
            }
            return result;
        }

        private IEnumerable<CodeCompletionItem> GenerateFromCallingClass(IProjectContent projectContent,
                                                                        IMethod delegateSignature)
        {
            var codeCompletionItemsFromCallingClass = Enumerable.Empty<CodeCompletionItem>();
            if (_resolveResult.CallingClass != null)
            {
                var matchedMethods = from method in _resolveResult.CallingClass.DefaultReturnType.GetMethods()
                                     where !MemberLookupHelper.IsSpecialMember(method)
                                           && MemberLookupHelper.IsAccesibleInCurrentContext(method, _resolveResult)
                                           && MemberLookupHelper.SignatureMatch(delegateSignature, method)
                                     select method;

                codeCompletionItemsFromCallingClass = from matchedMethod in matchedMethods
                                                      select new CodeCompletionItem(matchedMethod, projectContent);
            }
            return codeCompletionItemsFromCallingClass;
        }

        #endregion

        private NewEventHandlerCompletionItem GenerateCreateNewMember(IMethodOrProperty delegateSignature, ResolveResult resolveResult)
        {
            var parametersAsString = GenerateParametersString(DefaulAmbience, delegateSignature);
            var callingClass = resolveResult.CallingClass;
            var inStatic = false;

            if (_resolveResult.CallingMember != null)
                inStatic = _resolveResult.CallingMember.IsStatic;

            var eventHandlerFullyQualifiedTypeName = FullyQualifiedAmbience.Convert(_resolvedReturnType);

            var newHandlerName = BuildHandlerName();
            if (newHandlerName == null)
            {
                var mrr = _resolveResult as MemberResolveResult;
                var eventMember = (mrr != null ? mrr.ResolvedMember as IEvent : null);
                newHandlerName =
                    ((callingClass != null) ? callingClass.Name : "callingClass")
                    + "_"
                    + ((eventMember != null) ? eventMember.Name : "eventMember");
            }

            var newHandlerCodeBuilder = new StringBuilder();
            newHandlerCodeBuilder.AppendLine().AppendLine();
            if (inStatic)
                newHandlerCodeBuilder.Append("static ");
            newHandlerCodeBuilder
                .Append(FullyQualifiedAmbience.Convert(delegateSignature.ReturnType)).Append(" ").Append(newHandlerName).Append("(")
                .Append(parametersAsString).AppendLine(")")
                .AppendLine("{")
                .AppendLine("\t")
                .Append("}");

            var createNewMethod = new NewEventHandlerCompletionItem("generate " + newHandlerName + "(...);",
                                                                    newHandlerName,
                                                                    newHandlerName.Length,
                                                                    newHandlerName.Length,
                                                                    "new " + eventHandlerFullyQualifiedTypeName + "(" +
                                                                    newHandlerName + ")\nGenerate new handler\n" +
                                                                    CodeCompletionItem.GetDescription(_resolvedClass),
                                                                    _resolveResult, newHandlerCodeBuilder.ToString());
            return createNewMethod;
        }

        private static DelegateCompletionItem GenerateLambdaWithParameters(string parametersAsString)
        {
            var anonMethodWithParametersBuilder =
                new StringBuilder("delegate(").Append(parametersAsString).Append(") {  };");
            var delegateCompletionItem = new DelegateCompletionItem(anonMethodWithParametersBuilder.ToString(), 3,
                                                                    "${res:CSharpBinding.InsertAnonymousMethodWithParameters}");
            return delegateCompletionItem;
        }

        private static string GenerateParametersString(IAmbience ambience, IMethodOrProperty invoke)
        {
            return string.Join(", ", invoke.Parameters.Select(ambience.Convert));
        }

        private static DelegateCompletionItem GenerateLambdaWithoutParameters()
        {
            return new DelegateCompletionItem("() => {  };", 3,
                                              "${res:CSharpBinding.InsertAnonymousMethod}");
        }

        private string BuildHandlerName()
        {
            if (_expression != null)
                _expression = _expression.Trim().Trim('_');
            if (string.IsNullOrEmpty(_expression))
                return null;
            if (!(char.IsLetter(_expression[0])))
                return null;
            var handlerNameBuilder = new StringBuilder("On");
            handlerNameBuilder.Append(_expression.ToUpperInvariant()[0]);
            foreach (var t in _expression.Skip(1))
            {
                if (char.IsLetterOrDigit(t) || t == '_')
                {
                    handlerNameBuilder.Append(t);
                }
                else if (t == '.')
                {
                    if (Keywords.IsNonIdentifierKeyword(handlerNameBuilder.ToString()))
                    {
                        return null;
                    }
                }
                else
                {
                    return null;
                }
            }
            return handlerNameBuilder.ToString();
        }

        #region Nested type: DelegateCompletionItem

        private sealed class DelegateCompletionItem : DefaultCompletionItem
        {
            private readonly int _cursorOffset;

            public DelegateCompletionItem(string text, int cursorOffset, string documentation)
                : base(text)
            {
                _cursorOffset = cursorOffset;
                Description = new SimpleDescription(documentation);
            }

            public override void Complete(CompletionContext context)
            {
                base.Complete(context);
                context.Editor.Caret.Column -= _cursorOffset;
            }
        }

        #endregion

        #region Nested type: NewEventHandlerCompletionItem

        private sealed class NewEventHandlerCompletionItem : DefaultCompletionItem
        {
            private readonly string _methodCode;
            private readonly string _methodName;
            private readonly ResolveResult _resolveResult;
            private readonly int _selectionBeginOffset;
            private readonly int _selectionLength;

            private ITextEditor _editor;

            public NewEventHandlerCompletionItem(string completionTitle,
                                                 string methodName,
                                                 int selectionBeginOffset, int selectionLength,
                                                 string documentation, ResolveResult resolveResult,
                                                 string methodCode)
                : base(completionTitle)
            {
                _methodName = methodName;
                _selectionBeginOffset = selectionBeginOffset;
                _selectionLength = selectionLength;
                _resolveResult = resolveResult;
                _methodCode = methodCode;

                Description = new SimpleDescription(documentation);
            }

            public override void Complete(CompletionContext context)
            {
                _editor = context.Editor;
                using (_editor.Document.OpenUndoGroup())
                {
                    CompleteText(context, _methodName + ";");

                    _editor.Caret.Column -= _selectionBeginOffset;
                    _editor.Select(_editor.Caret.Offset - 1, _selectionLength);

                    var region = _resolveResult.CallingMember.BodyRegion;

                    _editor.Caret.Line = region.EndLine;
                    _editor.Caret.Column = region.EndColumn;

                    _editor.Document.Insert(_editor.Caret.Offset, _methodCode);

                    _editor.Language.FormattingStrategy.IndentLines(_editor, region.EndLine + 1, _editor.Caret.Line);

                    var line = _editor.Document.GetLine(_editor.Caret.Line - 1);
                    _editor.Document.Insert(line.EndOffset, "    ");
                    _editor.Caret.Offset = line.EndOffset;
                }
            }
        }

        #endregion
    }
}