using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace VcpCore.Plugins.Test.ParserTest
{
    public class TestIParser
    {
        [Test]
        public void TestParser()
        {
            MockParser tokenizer = new MockParser();
            var rootNode2 = new RootNode();
            IEnumerable<IToken> tokens2 = null;
            var tokens = tokenizer.Parse(tokens2);
            Assert.IsNotNull(tokens);
        }
       public class MockParser : IParser
        {
            public INode Parse(IEnumerable<IToken> tokens)    // 创建一个模拟的 IParser 实现类
            {
                var nodeStack = new Stack<INode>();
                var rootNode = new RootNode();
                rootNode.Nodes = nodeStack;
                return rootNode;

            }
        }

    }
}
