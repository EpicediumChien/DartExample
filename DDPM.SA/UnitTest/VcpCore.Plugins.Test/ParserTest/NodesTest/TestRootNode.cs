using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VcpCore.Plugins.Test.ParserTest.NodesTest
{
    public class TestRootNode
    {
        [Test]
        public void ValueTest()
        {
            string Value = "value1";
            RootNode rootNode=new RootNode();
            rootNode.Value = Value;
            var ValueResult = rootNode.Value;
            Assert.That(Value, Is.EqualTo(ValueResult));
        }

        [Test]
        public void NodesTest()
        {
            List<INode> nodeList = new List<INode>();
            IEnumerable<INode> nodes = nodeList;
            RootNode rootNode = new RootNode();
            rootNode.Nodes = nodes;
            var NodeseResult = rootNode.Nodes;
            Assert.That(nodes, Is.EqualTo(NodeseResult));
        }

        [Test]
        public void ParentTest()
        {
            INode parent = null;
            RootNode rootNode = new RootNode();
            rootNode.Parent = parent;
            var ParentResult = rootNode.Parent;
            Assert.That(parent, Is.EqualTo(ParentResult));
        }

        [Test]
        public void ToStringTest()
        {
            RootNode rootNode = new RootNode()
            {
                Value = "Child"
            }; 
            string expected = string.Empty;
            string actual = rootNode.ToString();
            Assert.That(expected, Is.EqualTo(actual));
        }
    }
}
