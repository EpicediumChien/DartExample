using WinCopies;

namespace VcpCore.Plugins.Test.ToKenizerTest
{
    public class TestTokenFilter
    {
        [Test]
        public void TokenFilterTest()
        {
            string pattern = "#";
            string Name = "Name1";
            TokenFilter<Token> tokenizer = new TokenFilter<Token>(pattern);
            tokenizer.Pattern = pattern;
            tokenizer.Name = Name;
            Assert.IsNotNull(tokenizer);
            Assert.That(pattern, Is.EqualTo(tokenizer.Pattern));
            Assert.That(Name, Is.EqualTo(tokenizer.Name));
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