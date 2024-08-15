namespace VcpCore.Plugins.Test.ToKenizerTest
{
    public class TestTokenizer
    {
        [Test]
        public void TokenizerTest()
        {
            string inputString = "(dfagasdgs)";
            CapabilitiesTokenizer capabilitiesTokenizer = new CapabilitiesTokenizer();
            var result = capabilitiesTokenizer.GetTokens(inputString);
            Assert.IsNotNull(result);
        }
    }
}