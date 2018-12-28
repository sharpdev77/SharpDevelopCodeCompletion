using System;
using ICSharpCode.AvalonEdit.CodeCompletion.Interface.Description;
using ICSharpCode.SharpDevelop.Dom;
using ICSharpCode.SharpDevelop.Dom.Refactoring;

namespace ICSharpCode.AvalonEdit.CodeCompletion.CompetionItems
{
    public sealed class OverrideCompletionItem : ICompletionItem
    {
        IMember member;
        private readonly IProjectContent _projectContent;

        public IMember Member
        {
            get { return member; }
        }

        string text;
        IImage image;


        public OverrideCompletionItem(IMember member, IProjectContent projectContent)
        {
            if (member == null)
                throw new ArgumentNullException("member");
            this.member = member;
            _projectContent = projectContent;

            this.text = GetName(member, ConversionFlags.ShowParameterList);
        }

        public string Text
        {
            get { return text; }
        }

        public IImage Image
        {
            get { return image; }
        }

        public Description Description
        {
            get
            {
                return new SimpleDescription("override " + GetName(member, ConversionFlags.ShowReturnType
                    | ConversionFlags.ShowParameterList
                        | ConversionFlags.ShowAccessibility)
                            + "\n\n" + CodeCompletionItem.GetDescription(member));
            }
        }

        double ICompletionItem.Priority { get { return 0; } }

        static string GetName(IMember member, ConversionFlags flags)
        {
            IAmbience ambience = AmbienceService.GetCurrentAmbience();
            ambience.ConversionFlags = flags | ConversionFlags.ShowParameterNames | ConversionFlags.ShowTypeParameterList;
            return ambience.Convert(member);
        }

        public void Complete(CompletionContext context)
        {
            ITextEditor editor = context.Editor;
            ClassFinder classFinder = new ClassFinder(ParserService.GetParseInformation(editor.Document.Text, _projectContent),
                editor.Caret.Line, editor.Caret.Column);
            int caretPosition = editor.Caret.Offset;
            IDocumentLine line = editor.Document.GetLine(editor.Caret.Line);
            string lineText = editor.Document.GetText(line.Offset, caretPosition - line.Offset);
            foreach (char c in lineText)
            {
                if (!char.IsWhiteSpace(c) && !char.IsLetterOrDigit(c))
                {
                    editor.Document.Replace(context.StartOffset, context.Length, this.Text);
                    context.EndOffset = context.StartOffset + this.Text.Length;
                    return;
                }
            }

            string indentation = lineText.Substring(0, lineText.Length - lineText.TrimStart().Length);

            editor.Document.Remove(line.Offset, caretPosition - line.Offset);


            CodeGenerator codeGen = _projectContent.Language.CodeGenerator;

            string text = codeGen.GenerateCode(codeGen.GetOverridingMethod(member, classFinder), indentation);
            text = text.TrimEnd(); // remove newline from end

            editor.Document.Insert(line.Offset, text);

            int endPos = line.Offset + text.Length;
            line = editor.Document.GetLineForOffset(endPos);
            editor.JumpTo(line.LineNumber, endPos - line.Offset + 1);
        }
    }
}