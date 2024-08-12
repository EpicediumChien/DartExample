using System.Collections.Generic;

namespace VcpCore.Plugins
{
    public interface ITokenizer
    {
        IEnumerable<IToken> GetTokens(string inputString);
    }
}
