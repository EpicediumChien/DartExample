using Moq;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using  VcpCore.Plugins;

namespace VcpCore.Plugins.Test.ParserTest
{
    public class TestCapabilitiesParser
    {
        [Test]
        public void ParseTest()
        {
            CapabilitiesParser capabilitiesParser = new CapabilitiesParser();

            var mockTokens = new Mock<IEnumerable<IToken>>();
            var parser = new CapabilitiesParser();
            var tokens = new List<IToken>()
            {
               new Token { Type = "Type1", Value = "Value1" },
               new Token { Type = "Type2", Value = "Value2" },
            };

            TokenImplementation token1 = new TokenImplementation();
            token1.Type = "type1";
            token1.Value = "value1";

            var rootNode = capabilitiesParser.Parse(tokens);
            Assert.IsNotNull(rootNode);
        }

        class TokenImplementation : IToken
        {
            public string Type { get; set; }
            public string Value { get; set; }
        }

    }
}
