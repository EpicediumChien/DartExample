using Microsoft.WindowsAPICodePack.PortableDevices.CommandSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace VcpCore.Plugins.Test.ToKenizerTest.TokensTest
{
    public class TestIToken
    {
        [Test]
        public void ITokenTest()
        {
            string Type="type1";
            string Value = "value1";
            TokenImplementation token = new TokenImplementation();
            token.Type = "type1";
            token.Value = "value1";
            Assert.That(Type, Is.EqualTo(token.Type));
            Assert.That(Value, Is.EqualTo(token.Value));
        }
        class TokenImplementation : IToken
        {
            public string Type { get; set; }
            public string Value { get; set; }
        }

    }
}
