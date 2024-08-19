using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VcpCore.Plugins.Test.ToKenizerTest.TokensTest
{
    public class TestToken
    {
        [Test]
        public void OpenTokenTest()
        {
            string Type = "type1";
            string Value = "value1";
            Token token = new Token();
            Assert.IsNotNull(token);
            token.Type = "type1";
            token.Value = "value1";
            string expectedToString = "type1: value1";
            string actualToString = token.ToString();
            Assert.That(expectedToString, Is.EqualTo(actualToString));
            Assert.That(Type, Is.EqualTo(token.Type));
            Assert.That(Value, Is.EqualTo(token.Value));
        }

    }
}
