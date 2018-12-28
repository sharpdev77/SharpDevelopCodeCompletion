namespace ICSharpCode.AvalonEdit.CodeCompletion.Interface.Description
{
    public class SimpleHeader : IHeader
    {
        private readonly string _header;

        public SimpleHeader(string header)
        {
            _header = header;
        }
        public override string ToString()
        {
            return _header;
        }
        public Parameter[] Parameters
        {
            get { return new Parameter[0]; }
        }
    }
}