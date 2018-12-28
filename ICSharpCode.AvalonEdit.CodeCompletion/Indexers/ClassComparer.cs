using System.Collections.Generic;
using ICSharpCode.SharpDevelop.Dom;

namespace ICSharpCode.AvalonEdit.CodeCompletion.Indexers
{
    public class ClassComparer : IComparer<IClass>
    {
        public int Compare(IClass x, IClass y)
        {
            if (x.FullyQualifiedName == y.FullyQualifiedName)
                return 0;

            if(x.IsTypeInInheritanceTree(y))
            {
                return 1;
            }

            if(y.IsTypeInInheritanceTree(x))
            {
                return -1;
            }
            return 0;
        }
    }
}