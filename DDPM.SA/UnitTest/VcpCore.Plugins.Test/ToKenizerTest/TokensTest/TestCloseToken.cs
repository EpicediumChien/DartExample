using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VcpCore.Plugins.Test.ToKenizerTest.TokensTest
{
    public class TestCloseToken
    {

        [Test]
        public void CloseTokenTest()
        {
            string type = "ABC";
            string value = "0x22";
            CloseToken token = new CloseToken() {Type="ABC",Value="0x22" };
            Assert.IsNotNull(token);
            Assert.That(type,Is.EqualTo(token.Type));
            Assert.That(value, Is.EqualTo(token.Value));
        }
    }
}
