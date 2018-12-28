using System.Collections.Generic;
using System.Linq;
using ICSharpCode.SharpDevelop.Dom;

namespace ICSharpCode.AvalonEdit.CodeCompletion.Indexers
{
    public static class ParametersExtencions
    {
        public static ParametersKey GetKey(this IEnumerable<IParameter> parameters)
        {
            return new ParametersKey(parameters.Select(parameter => parameter.ReturnType.FullyQualifiedName));
        }
    }
}