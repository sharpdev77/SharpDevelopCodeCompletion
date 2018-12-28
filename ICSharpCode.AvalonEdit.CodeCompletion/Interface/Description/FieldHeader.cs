using ICSharpCode.SharpDevelop.Dom;

namespace ICSharpCode.AvalonEdit.CodeCompletion.Interface.Description
{
    public class FieldHeader : IHeader
    {
        public string Name { get; private set; }
        public string Type { get; private set; }
        public bool IsReadOnly { get; private set; }


        public FieldHeader(IField field)
        {
            Name = field.Name;
            Type = field.ReturnType.FormatName();
            IsReadOnly = field.IsReadonly;
        }

        public Parameter[] Parameters
        {
            get { return new Parameter[0]; }
        }
    }
}