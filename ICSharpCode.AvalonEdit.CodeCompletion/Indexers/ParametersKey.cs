using System.Collections.Generic;
using System.Linq;

namespace ICSharpCode.AvalonEdit.CodeCompletion.Indexers
{
    public struct ParametersKey
    {
        public readonly IEnumerable<string> Types;

        public ParametersKey(IEnumerable<string> types)
            : this()
        {
            Types = types;
        }

        public bool Equals(ParametersKey other)
        {
            return other.Types.SequenceEqual(Types);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (obj.GetType() != typeof (ParametersKey)) return false;
            return Equals((ParametersKey) obj);
        }

        public override int GetHashCode()
        {
            return (Types != null ? Types.Select(type => type.GetHashCode()*397).Aggregate(0, (i, i1) => i ^ i1) : 0);
        }
    }
}