using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VcpCore.Plugins.Test
{
    public class TestINodeFormatter
    {
        [Test]
        public void TestFormatNode()
        {
            var mockNode = new Mock<INode>(); // 创建一个模拟的 INode 对象
            var formatter = new INodeFormatterTest();// 创建一个实现了 INodeFormatter 接口的测试类
            string expectedResult = "Formatted Node";
            string actualResult = formatter.FormatNode(mockNode.Object);
            Assert.That(expectedResult, Is.EqualTo(actualResult));
        }
    }

    class INodeFormatterTest : INodeFormatter
    {
        public string FormatNode(INode node)
        {
            return "Formatted Node";
        }
    }
}
