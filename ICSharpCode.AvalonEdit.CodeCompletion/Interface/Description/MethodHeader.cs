using System.Linq;
using ICSharpCode.SharpDevelop.Dom;

namespace ICSharpCode.AvalonEdit.CodeCompletion.Interface.Description
{
    public class MethodHeader : IHeader
    {
        public string Name { get; private set; }
        public string ReturnType { get; private set; }
        public Parameter[] Parameters { get; set; }
        public GenericParameter[] GenericParameters { get; set; }

        public MethodHeader(IMethod method)
        {


            Name = method.Name;
            ReturnType = method.ReturnType.FormatName();
            Parameters = method.Parameters
                .Select(parameter => new Parameter(parameter.Name, "", parameter.ReturnType.FormatName(), parameter.IsOptional))
                .ToArray();
            GenericParameters = method.TypeParameters
                .Select(parameter => new GenericParameter(parameter.Name))
                .ToArray();
        }
    }
}