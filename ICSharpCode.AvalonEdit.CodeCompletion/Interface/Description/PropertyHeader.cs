using ICSharpCode.SharpDevelop.Dom;

namespace ICSharpCode.AvalonEdit.CodeCompletion.Interface.Description
{
    public class PropertyHeader : IHeader
    {
        public string Name { get; private set; }
        public string Type { get; private set; }
        public bool CanGet { get; private set; }
        public bool CanSet { get; private set; }

        public PropertyHeader(IProperty field)
        {
            Name = field.Name;
            Type = field.ReturnType.FormatName();
            CanGet = field.CanGet;
            CanSet = field.CanSet;
        }
        public Parameter[] Parameters
        {
            get { return new Parameter[0]; }
        }
    }
}