using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VcpCore.Plugins.Test.ToKenizerTest.TokensTest
{
    public class TestWhitespaceToken
    {
        [Test]
        public void WhitespaceTokenTest()
        {
            string Type = "Whitespacetype1";
            string Value = "Whitespacevalue1";
            WhitespaceToken token = new WhitespaceToken();
            Assert.IsNotNull(token);
            token.Type = "Whitespacetype1";
            token.Value = "Whitespacevalue1";
            Assert.That(Type, Is.EqualTo(token.Type));
            Assert.That(Value, Is.EqualTo(token.Value));
        }
    }
}
