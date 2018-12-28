using ICSharpCode.SharpDevelop.Dom;

namespace ICSharpCode.AvalonEdit.CodeCompletion
{
    /// <summary>
    /// Interface that gives backend bindings the possibility to control what characters and
    /// keywords invoke code completion.
    /// </summary>
    public interface ICodeCompletionBinding
    {
        CodeCompletionKeyPressResult HandleKeyPress(ITextEditor editor, char ch, IProjectContent projectContent);
        bool CtrlSpace(ITextEditor editor);
    }
}