using System.Drawing;
using System.Windows.Media;

namespace ICSharpCode.AvalonEdit.CodeCompletion
{
    /// <summary>
    /// Represents an image.
    /// </summary>
    public interface IImage
    {
        /// <summary>
        /// Gets the image as WPF ImageSource.
        /// </summary>
        ImageSource ImageSource { get; }

        /// <summary>
        /// Gets the image as System.Drawing.Bitmap.
        /// </summary>
        Bitmap Bitmap { get; }

        /// <summary>
        /// Gets the image as System.Drawing.Icon.
        /// </summary>
        Icon Icon { get; }
    }
}