using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VcpCore.Plugins.Test.ToKenizerTest.TokensTest
{
    public class TestOpenToken
    {
        [Test]
        public void OpenTokenTest()
        {
            string Type = "type1";
            string Value = "value1";
            OpenToken token = new OpenToken();
            Assert.IsNotNull(token);
            token.Type = "type1";
            token.Value = "value1";
            Assert.That(Type, Is.EqualTo(token.Type));
            Assert.That(Value, Is.EqualTo(token.Value));
        }
    }
}
