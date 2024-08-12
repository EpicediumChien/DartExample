using System.Collections.Generic;

namespace VcpCore.Plugins
{
    public class GroupValueNode : INode
    {
        public string Value { get; set; }

        public IEnumerable<INode> Nodes { get; set; }

        public INode Parent { get; set; }

        public override string ToString() =>
            //Parent.ToString() != null
            !string.IsNullOrEmpty(Parent.ToString())
                ? Parent.ToString() + "_" + Value
                : Value;
    }
}
