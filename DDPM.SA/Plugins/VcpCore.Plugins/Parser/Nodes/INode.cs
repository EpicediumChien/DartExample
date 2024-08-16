using System.Collections.Generic;

namespace VcpCore.Plugins
{
    public interface INode
    {
        IEnumerable<INode> Nodes { get; set; }

        INode Parent { get; set; }

        string Value { get; set; }
    }
}