namespace ICSharpCode.AvalonEdit.CodeCompletion.Interface.Description
{
    public class Parameter
    {
        public string Name { get; private set; }
        public string Type { get; private set; }
        public bool IsOptional { get; private set; }

        public bool IsHighlighted { get; set; }
        public string Description { get; set; }

        public Parameter(string name, string description, string type, bool isOptional)
        {
            Name = name;
            Description = description;
            Type = type;
            IsOptional = isOptional;
        }
    }
}