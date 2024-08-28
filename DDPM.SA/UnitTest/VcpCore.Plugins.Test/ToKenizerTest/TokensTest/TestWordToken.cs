namespace VcpCore.Plugins.Test.ToKenizerTest.TokensTest
{
    public class TestWordToken
    {
        [Test]
        public void WordTokenTest()
        {
            string Type = "WordTokentype1";
            string Value = "WordTokenvalue1";
            WordToken token = new WordToken();
            Assert.IsNotNull(token);
            token.Type = "WordTokentype1";
            token.Value = "WordTokenvalue1";
            Assert.That(Type, Is.EqualTo(token.Type));
            Assert.That(Value, Is.EqualTo(token.Value));
        }
    }
}