using System.Collections.Generic;

namespace VcpCore.Plugins
{
    public interface IParser
    {
        INode Parse(IEnumerable<IToken> tokens);
    }
}