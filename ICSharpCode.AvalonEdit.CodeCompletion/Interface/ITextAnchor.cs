using System;
using ICSharpCode.NRefactory;

namespace ICSharpCode.AvalonEdit.CodeCompletion
{
    /// <summary>
    /// Represents an anchored location inside an <see cref="IDocument"/>.
    /// </summary>
    public interface ITextAnchor
    {
        /// <summary>
        /// Gets the text location of this anchor.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when trying to get the Offset from a deleted anchor.</exception>
        Location Location { get; }

        /// <summary>
        /// Gets the offset of the text anchor.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when trying to get the Offset from a deleted anchor.</exception>
        int Offset { get; }

        /// <summary>
        /// Controls how the anchor moves.
        /// </summary>
        AnchorMovementType MovementType { get; set; }

        /// <summary>
        /// Specifies whether the anchor survives deletion of the text containing it.
        /// <c>false</c>: The anchor is deleted when the a selection that includes the anchor is deleted.
        /// <c>true</c>: The anchor is not deleted.
        /// </summary>
        bool SurviveDeletion { get; set; }

        /// <summary>
        /// Gets whether the anchor was deleted.
        /// </summary>
        bool IsDeleted { get; }

        /// <summary>
        /// Gets the line number of the anchor.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when trying to get the Offset from a deleted anchor.</exception>
        int Line { get; }

        /// <summary>
        /// Gets the column number of this anchor.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when trying to get the Offset from a deleted anchor.</exception>
        int Column { get; }

        /// <summary>
        /// Occurs after the anchor was deleted.
        /// </summary>
        event EventHandler Deleted;
    }
}