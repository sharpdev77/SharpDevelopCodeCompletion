using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interactivity;
using ICSharpCode.SharpDevelop.Dom;

namespace ICSharpCode.AvalonEdit.CodeCompletion
{
    public class CodeCompletionBeahvior : Behavior<TextEditor>
    {
        private CodeCompletionEditorAdapter _avalonEditTextEditorAdapter;

        public static readonly DependencyProperty CodeCompletionBindingsProperty =
            DependencyProperty.Register("CodeCompletionBindings", typeof (IEnumerable), typeof (CodeCompletionBeahvior), new PropertyMetadata(default(IEnumerable)));

        public IEnumerable CodeCompletionBindings
        {
            get { return (IEnumerable) GetValue(CodeCompletionBindingsProperty); }
            set { SetValue(CodeCompletionBindingsProperty, value); }
        }

        public static readonly DependencyProperty AssembliesProperty =
            DependencyProperty.Register("Assemblies", typeof (IEnumerable), typeof (CodeCompletionBeahvior), new PropertyMetadata(default(IEnumerable)));
        
        public IEnumerable Assemblies
        {
            get { return (IEnumerable) GetValue(AssembliesProperty); }
            set { SetValue(AssembliesProperty, value); }
        }

        public static readonly DependencyProperty ProjectContentProperty =
            DependencyProperty.Register("ProjectContent", typeof (IProjectContent), typeof (CodeCompletionBeahvior), new PropertyMetadata(default(IProjectContent),ProjectContentChangedCallback));

        private static void ProjectContentChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var cSharpCompletionBinding = new CSharpCompletionBinding(dependencyPropertyChangedEventArgs.NewValue as IProjectContent,
                new CompletionItemProviderFactory());
            var codeCompletionBeahvior = dependencyObject as CodeCompletionBeahvior;
            
            if (codeCompletionBeahvior == null) return;
            
            if(codeCompletionBeahvior.FilterStrategy != null)
            {
                cSharpCompletionBinding.FilterStrategy = codeCompletionBeahvior.FilterStrategy;

            }
            codeCompletionBeahvior.CodeCompletionBindings = new[] { cSharpCompletionBinding };
        }

        public IProjectContent ProjectContent
        {
            get { return (IProjectContent) GetValue(ProjectContentProperty); }
            set { SetValue(ProjectContentProperty, value); }
        }

        public static readonly DependencyProperty FilterStrategyProperty =
            DependencyProperty.Register("FilterStrategy", typeof (IFilterStrategy), typeof (CodeCompletionBeahvior), new PropertyMetadata(default(IFilterStrategy),FilterStrategyChanged));

        private static void FilterStrategyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var codeCompletionBeahvior = (CodeCompletionBeahvior)d;
            var filterStrategy = (IFilterStrategy)e.NewValue;
            if(codeCompletionBeahvior.CodeCompletionBindings == null) return;

            var cSharpCompletionBinding = codeCompletionBeahvior.CodeCompletionBindings.OfType<CSharpCompletionBinding>().FirstOrDefault();
            
            if (cSharpCompletionBinding != null)
            {
                cSharpCompletionBinding.FilterStrategy = filterStrategy;
            }
        }

        private readonly CSharpCompletionBinding _cSharpCompletionBinding;

        public IFilterStrategy FilterStrategy
        {
            get { return (IFilterStrategy) GetValue(FilterStrategyProperty); }
            set { SetValue(FilterStrategyProperty, value); }
        }

        protected override void OnAttached()
        {
            AssociatedObject.TextArea.TextEntering += TextAreaTextEntering;
            AssociatedObject.TextArea.TextEntered += TextAreaTextEntered;
            
            _avalonEditTextEditorAdapter = new CodeCompletionEditorAdapter(AssociatedObject);
            AssociatedObject.KeyUp += TextEditorKeyUp;
            AssociatedObject.KeyDown += TextEditorKeyUp;
        }

        void TextAreaTextEntering(object sender, TextCompositionEventArgs compositionEventArgs)
        {
            if (CompletionWindow != null)
                return;

            if (compositionEventArgs.Handled)
                return;
            
            var textArea = AssociatedObject.TextArea;
            if (textArea.ActiveInputHandler != textArea.DefaultInputHandler)
                return;

            SetCompletionForUserInput(compositionEventArgs);
        }

        private void SetCompletionForUserInput(TextCompositionEventArgs compositionEventArgs)
        {
            var completionKeyPressResult = (from @char in compositionEventArgs.Text
                                            from completionBinding in CompletionBindings
                                            select completionBinding.HandleKeyPress(_avalonEditTextEditorAdapter, @char, ProjectContent))
                .FirstOrDefault(result => result != CodeCompletionKeyPressResult.None);

            switch (completionKeyPressResult)
            {
                case CodeCompletionKeyPressResult.Completed:
                    if (CompletionWindow != null)
                    {
                        CompletionWindow.ExpectInsertionBeforeStart = true;
                    }
                    if (InsightWindow != null)
                    {
                        InsightWindow.ExpectInsertionBeforeStart = true;
                    }
                    return;
                case CodeCompletionKeyPressResult.CompletedIncludeKeyInCompletion:
                    if (CompletionWindow != null)
                    {
                        if (CompletionWindow.StartOffset == CompletionWindow.EndOffset)
                        {
                            CompletionWindow.CloseWhenCaretAtBeginning = true;
                        }
                    }
                    return;
                case CodeCompletionKeyPressResult.EatKey:
                    compositionEventArgs.Handled = true;
                    return;
            }
        }

        private IEnumerable<ICodeCompletionBinding> CompletionBindings
        {
            get { return CodeCompletionBindings.OfType<ICodeCompletionBinding>(); }
        }


        SharpDevelopCompletionWindow CompletionWindow
        {
            get
            {
                return _avalonEditTextEditorAdapter.SharpDevelopCompletionWindow;
            }
        }

        SharpDevelopInsightWindow InsightWindow
        {
            get
            {
                return _avalonEditTextEditorAdapter.SharpDevelopInsightWindow;
            }
        }

        void TextAreaTextEntered(object sender, TextCompositionEventArgs e)
        {
            if (e.Text.Length > 0 && !e.Handled)
            {
                ILanguageBinding languageBinding = _avalonEditTextEditorAdapter.Language;
                if (languageBinding != null && languageBinding.FormattingStrategy != null)
                {
                    char c = e.Text[0];
                    // When entering a newline, AvalonEdit might use either "\r\n" or "\n", depending on
                    // what was passed to TextArea.PerformTextInput. We'll normalize this to '\n'
                    // so that formatting strategies don't have to handle both cases.
                    if (c == '\r')
                        c = '\n';
                    languageBinding.FormattingStrategy.FormatLine(_avalonEditTextEditorAdapter, c);
                }
            }
        }

        private void TextEditorKeyUp(object sender, KeyEventArgs e)
        {
            if (!(e.Key == Key.Space && Keyboard.Modifiers == ModifierKeys.Control))
                return;
            if (e.IsUp)
            {
                foreach (var codeCompletionBinding in CodeCompletionBindings.OfType<ICodeCompletionBinding>())
                {
                    if (codeCompletionBinding.CtrlSpace(_avalonEditTextEditorAdapter))
                    {
                        break;
                    }
                }
            }
            e.Handled = true;
        }
    }
}