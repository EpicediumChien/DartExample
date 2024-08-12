using System.Collections.Generic;

namespace VcpCore.Plugins
{
    public class RootNode : INode
    {
        public IEnumerable<INode> Nodes { get; set; }

        public INode Parent { get; set; }

        public string Value { get; set; }

        public override string ToString()
        {
            //return null; //Dean 0626 fix SAST issue, return empty string instead
            return string.Empty;
        }
    }
}
