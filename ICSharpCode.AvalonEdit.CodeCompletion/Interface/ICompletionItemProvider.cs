// Copyright (c) AlphaSierraPapa for the SharpDevelop Team (for details please see \doc\copyright.txt)
// This code is distributed under the GNU LGPL (for details please see \doc\license.txt)

using System;
using ICSharpCode.SharpDevelop.Dom;

namespace ICSharpCode.AvalonEdit.CodeCompletion
{
    /// <summary>
    /// Base class for completion item providers.
    /// </summary>
    /// <remarks>A completion item provider is not necessary to use code completion - it's
    /// just a helper class.</remarks>
    public interface ICompletionItemProvider
    {
        /// <summary>
        ///  Generates the completion list.
        ///  </summary>
        ICompletionItemList GenerateCompletionList(ITextEditor editor, IProjectContent projectContent);
    }
}