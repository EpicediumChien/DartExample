using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            Assert.That("MyFilter", Is.EqualTo(filter.Name));
            Assert.That(".*", Is.EqualTo(filter.Pattern));

            Token token = filter.GetToken("TestValue");
            Assert.That("TestType", Is.EqualTo(token.Type));
            Assert.That("TestValue", Is.EqualTo(token.Value));
        }

        // 创建一个实现 ITokenFilter<Token> 接口的模拟类
        class TokenFilter : ITokenFilter<Token>
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
