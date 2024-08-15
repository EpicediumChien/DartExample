namespace VcpCore.Plugins.Test.ToKenizerTest
{
    public class TestITokenizer
    {
        [Test]
        public void TestGetTokens()
        {
            MockTokenizer tokenizer = new MockTokenizer();
            int count = 2;
            IEnumerable<IToken> tokens = tokenizer.GetTokens("TestInput");
            Assert.IsNotNull(tokens);
            Assert.That(count, Is.EqualTo(tokens.Count()));
        }

        private class MockTokenizer : ITokenizer
        {
            public IEnumerable<IToken> GetTokens(string inputString)    // 创建一个模拟的 ITokenizer 实现类
            {
                return new List<IToken>
                {
                        new Token { Type = "Type1", Value = "Value1" },
                        new Token { Type = "Type2", Value = "Value2" }
                };
            }
        }
    }
}