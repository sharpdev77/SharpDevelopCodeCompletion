namespace ICSharpCode.AvalonEdit.CodeCompletion.Interface.Description
{
    public class SimpleDescription : Description
    {
        private readonly string _simpleDescription;

        public SimpleDescription(string simpleDescription)
            : base(new SimpleHeader(simpleDescription))
        {
            _simpleDescription = simpleDescription;
        }

        public override string ToString()
        {
            return _simpleDescription;
        }
    }
}