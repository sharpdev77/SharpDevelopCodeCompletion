using System;
using System.IO;
using System.Windows;
using System.Xml;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;

namespace DemoApplication
{
    public class SyntaxHighlightingSource
    {
        public SyntaxHighlightingSource()
        {
            CurrentScheme = GetSyntaxHighlighting();
        }

        public IHighlightingDefinition CurrentScheme { get; private set; }

        private static IHighlightingDefinition GetSyntaxHighlighting()
        {
            var customHighlighting = HighlightingManager.Instance.GetDefinition("C#");
            var uri = new Uri(@"pack://application:,,,/DemoApplication;component/CSModeWithDotnetKeywords.xshd");
            var templateStream = Application.GetResourceStream(uri);
            if (templateStream != null)
            {
                using (Stream s = templateStream.Stream)
                {
                    using (XmlReader reader = new XmlTextReader(s))
                    {
                        customHighlighting = HighlightingLoader.Load(reader, HighlightingManager.Instance);
                    }
                }
            }
            return customHighlighting;
        }
    }
}