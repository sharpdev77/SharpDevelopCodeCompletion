using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interactivity;
using ICSharpCode.SharpDevelop.Dom;
using ICSharpCode.SharpDevelop.Dom.CSharp;

namespace ICSharpCode.AvalonEdit.CodeCompletion
{
    public class ExecuteCommandOnKeyPressWithCurrentCompletionEntryAsParameterBehavior : Behavior<TextEditor>
    {
        public static readonly DependencyProperty CommandProperty = DependencyProperty.Register("Command", typeof (ICommand), typeof (ExecuteCommandOnKeyPressWithCurrentCompletionEntryAsParameterBehavior), new PropertyMetadata(default(ICommand)));
        public static readonly DependencyProperty KeyProperty = DependencyProperty.Register("Key", typeof (Key), typeof (ExecuteCommandOnKeyPressWithCurrentCompletionEntryAsParameterBehavior), new PropertyMetadata(default(Key)));

        public ICommand Command
        {
            get { return (ICommand) GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }

        public Key Key
        {
            get { return (Key) GetValue(KeyProperty); }
            set { SetValue(KeyProperty, value); }
        }

                public static readonly DependencyProperty ProjectContentProperty =
            DependencyProperty.Register("ProjectContent", typeof(IProjectContent), typeof(ExecuteCommandOnKeyPressWithCurrentCompletionEntryAsParameterBehavior), new PropertyMetadata(default(IProjectContent)));

        public IProjectContent ProjectContent
        {
            get { return (IProjectContent) GetValue(ProjectContentProperty); }
            set { SetValue(ProjectContentProperty, value); }
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.PreviewKeyDown += OnPreviewKeyPressed;
        }

        private void OnPreviewKeyPressed(object sender, KeyEventArgs e)
        {
            if (e.Key == Key && Command != null)
            {
                var parameter = GetParameter();
                if (Command.CanExecute(parameter))
                {
                    e.Handled = true;
                    Command.Execute(parameter);
                }
            }
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.PreviewKeyDown -= OnPreviewKeyPressed;
        }


        private object GetParameter()
        {
            var expressionFinder = new CSharpExpressionFinder(ParserService.GetParseInformation(AssociatedObject.Text, ProjectContent));
            var caret = AssociatedObject.TextArea.Caret;
            var result = expressionFinder.FindFullExpression(AssociatedObject.Text, caret.Offset);

            var resolveResult = ParserService.Resolve(result, caret.Line, caret.Column, AssociatedObject.Text, ProjectContent);

            if (resolveResult==null || !resolveResult.IsValid) return null;
            
            if(resolveResult is MemberResolveResult)
            {
                return ((MemberResolveResult) resolveResult).ResolvedMember;
            }
            if(resolveResult is TypeResolveResult)
            {
                return ((TypeResolveResult)resolveResult).ResolvedClass;
            }            
            if(resolveResult is NamespaceResolveResult)
            {
                return new NamespaceEntry(((NamespaceResolveResult)resolveResult).Name);
            }
            return null;
        }
    }
}