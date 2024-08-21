using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VcpCore.Plugins.Test.ParserTest.NodesTest
{
    public class TestGroupValueNoes
    {
        [Test]
        public void ValueTest()
        {
            string Value = "value1";
            GroupValueNode groupValueNode = new GroupValueNode();
            groupValueNode.Value = Value;
            var ValueResult = groupValueNode.Value;
            Assert.That(Value, Is.EqualTo(ValueResult));
        }

        [Test]
        public void NodesTest()
        {
            List<INode> nodeList = new List<INode>();
            IEnumerable<INode> nodes = nodeList;
            GroupValueNode groupValueNode = new GroupValueNode();
            groupValueNode.Nodes = nodes;
            var NodeseResult = groupValueNode.Nodes;
            Assert.That(nodes, Is.EqualTo(NodeseResult));
        }

        [Test]
        public void ParentTest()
        {
            INode parent=null;
            GroupValueNode groupValueNode = new GroupValueNode();
            groupValueNode.Parent = parent;
            var ParentResult = groupValueNode.Parent;
            Assert.That(parent, Is.EqualTo(ParentResult));
        }

        [Test]
        public void ToStringTest()
        {
            var parentNode = new Mock<INode>();
            parentNode.Setup(p => p.ToString()).Returns("Parent");

            var groupValueNode = new GroupValueNode
            {
                Value = "Child",
                Parent = parentNode.Object
            };

            string expected = "Parent_Child";
            string actual = groupValueNode.ToString();
            Assert.That(expected, Is.EqualTo(actual));
        }

    }
}
