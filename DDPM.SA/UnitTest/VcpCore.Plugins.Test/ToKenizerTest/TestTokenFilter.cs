using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Power;

namespace VcpCore.Plugins.Test.ToKenizerTest
{
    public class TestTokenFilter
    {
        [Test]
        public void TokenFilterTest()
        {
            string pattern = "#";
            TokenFilter<Token> tokenizer = new TokenFilter<Token>(pattern);
            tokenizer.Pattern = pattern;
            tokenizer.Name = "Name1";
            Assert.IsNotNull(tokenizer);

            string value = "value1";
            tokenizer.GetToken(value);
        }

        [Test]
        public void GetTokenTest()
        {
            string pattern = "*";
            TokenFilter<Token> tokenizer = new TokenFilter<Token>(pattern);
            tokenizer.Pattern = pattern;
            tokenizer.Name = "Name2";
            Assert.IsNotNull(tokenizer);

            string value = "value2";
            Token result = tokenizer.GetToken(value);
            Assert.That(value, Is.EqualTo(result.Value));
            Assert.That(tokenizer.Name, Is.EqualTo(result.Type));
        }
    }
}
