using System.Collections.Generic;
using System.Linq;

namespace ICSharpCode.AvalonEdit.CodeCompletion.Interface.Description
{
    public class Description
    {
        public bool IsInsisghtWindow { get; set; }

        public IHeader Header { get; private set; }

        public string Summary { get; set; }

        public Description(IHeader header)
        {
            Header = header;

            Parameters = Header.Parameters;
            OverloadHeaders = new List<IHeader>();
        }

        public Parameter[] Parameters { get; private set; }

        public List<IHeader> OverloadHeaders { get; private set; }

        public override string ToString()
        {
            return Header + "\n" + Summary + "\n" + string.Join("\n",Parameters.Select(parameter => "Parameter: " + parameter.Name + " " + parameter.Description));
        }
    }
}