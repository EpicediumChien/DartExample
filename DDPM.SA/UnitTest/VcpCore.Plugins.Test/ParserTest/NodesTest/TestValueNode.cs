using Moq;

namespace VcpCore.Plugins.Test.ParserTest.NodesTest
{
    public class TestValueNode
    {
        [Test]
        public void ValueTest()
        {
            string Value = "value2";
            ValueNode valueNode = new ValueNode();
            valueNode.Value = Value;
            var ValueResult = valueNode.Value;
            Assert.That(Value, Is.EqualTo(ValueResult));
        }

        [Test]
        public void NodesTest()
        {
            List<INode> nodeList = new List<INode>();
            IEnumerable<INode> nodes = nodeList;
            ValueNode valueNode = new ValueNode();
            valueNode.Nodes = nodes;
            var NodeseResult = valueNode.Nodes;
            Assert.That(nodes, Is.EqualTo(NodeseResult));
        }

        [Test]
        public void ParentTest()
        {
            INode parent = null;
            ValueNode valueNode = new ValueNode();
            valueNode.Parent = parent;
            var ParentResult = valueNode.Parent;
            Assert.That(parent, Is.EqualTo(ParentResult));
        }

        [Test]
        public void ToStringTest()
        {
            var parentNode = new Mock<INode>();
            parentNode.Setup(p => p.ToString()).Returns("Parent1");

            ValueNode valueNode = new ValueNode()
            {
                Value = "Child",
                Parent = parentNode.Object
            };

            string expected = "Parent1_Child";
            string actual = valueNode.ToString();
            Assert.That(expected, Is.EqualTo(actual));
        }
    }
}