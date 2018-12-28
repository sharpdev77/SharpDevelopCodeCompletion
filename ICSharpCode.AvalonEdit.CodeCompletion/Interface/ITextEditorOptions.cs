using System.ComponentModel;

namespace ICSharpCode.AvalonEdit.CodeCompletion
{
    public interface ITextEditorOptions : INotifyPropertyChanged
    {
        /// <summary>
        /// Gets the text used for one indentation level.
        /// </summary>
        string IndentationString { get; }

        /// <summary>
        /// Gets whether a '}' should automatically be inserted when a block is opened.
        /// </summary>
        bool AutoInsertBlockEnd { get; }

        /// <summary>
        /// Gets if tabs should be converted to spaces.
        /// </summary>
        bool ConvertTabsToSpaces { get; }

        /// <summary>
        /// Gets the size of an indentation level.
        /// </summary>
        int IndentationSize { get; }

        /// <summary>
        /// Gets the column of the vertical ruler (line that signifies the maximum line length
        /// defined by the coding style)
        /// This property returns a valid value even if the vertical ruler is set to be invisible.
        /// </summary>
        int VerticalRulerColumn { get; }

        /// <summary>
        /// Gets whether errors should be underlined.
        /// </summary>
        bool UnderlineErrors { get; }

        /// <summary>
        /// Gets the name of the currently used font.
        /// </summary>
        string FontFamily { get; }
    }
}