namespace VcpCore.Plugins.Test.ToKenizerTest
{
    public class TestITokenFilter
    {
        [Test]
        public void ITokenFilterTest()
        {
            TokenFilter filter = new TokenFilter();

            filter.Name = "MyFilter";
            filter.Pattern = ".*";
            string myFilter = "MyFilter";
            string myFilter2 = ".*";
            string type = "TestType";
            string valuse1 = "TestValue";
            Assert.That(myFilter, Is.EqualTo(filter.Name));
            Assert.That(myFilter2, Is.EqualTo(filter.Pattern));

            Token token = filter.GetToken("TestValue");
            Assert.That(type, Is.EqualTo(token.Type));
            Assert.That(valuse1, Is.EqualTo(token.Value));
        }

        // 创建一个实现 ITokenFilter<Token> 接口的模拟类
        private class TokenFilter : ITokenFilter<Token>
        {
            public string Name { get; set; }
            public string Pattern { get; set; }

            public Token GetToken(string value)
            {
                return new Token { Type = "TestType", Value = value };
            }
        }
    }
}