using System;
using System.Collections.Generic;
using System.Linq;
using ICSharpCode.SharpDevelop.Dom;

namespace ICSharpCode.AvalonEdit.CodeCompletion
{
    /// <summary>
    /// Aggregates multiple ILanguageBinding instances to allow more
    /// than one language binding for a filename extension.
    /// </summary>
    internal sealed class AggregatedLanguageBinding : ILanguageBinding
    {
        public static readonly AggregatedLanguageBinding NullLanguageBinding =
            new AggregatedLanguageBinding(Enumerable.Empty<ILanguageBinding>());

        private readonly IEnumerable<ILanguageBinding> allBindings;

        public AggregatedLanguageBinding(IEnumerable<ILanguageBinding> bindings)
        {
            if (bindings == null)
                throw new ArgumentNullException("bindings");
            allBindings = bindings;
        }

        #region ILanguageBinding Members

        public IFormattingStrategy FormattingStrategy
        {
            get
            {
                foreach (ILanguageBinding binding in allBindings)
                {
                    if (binding.FormattingStrategy != null)
                        return binding.FormattingStrategy;
                }

                return DefaultFormattingStrategy.DefaultInstance;
            }
        }

        public LanguageProperties Properties
        {
            get
            {
                foreach (ILanguageBinding binding in allBindings)
                {
                    if (binding.Properties != null)
                        return binding.Properties;
                }

                return LanguageProperties.None;
            }
        }

        public void Attach(ITextEditor editor)
        {
            foreach (ILanguageBinding binding in allBindings)
                binding.Attach(editor);
        }

        public void Detach()
        {
            foreach (ILanguageBinding binding in allBindings)
                binding.Detach();
        }

        public IBracketSearcher BracketSearcher
        {
            get
            {
                foreach (ILanguageBinding binding in allBindings)
                {
                    if (binding.BracketSearcher != null)
                        return binding.BracketSearcher;
                }

                return DefaultBracketSearcher.DefaultInstance;
            }
        }

        #endregion
    }
}