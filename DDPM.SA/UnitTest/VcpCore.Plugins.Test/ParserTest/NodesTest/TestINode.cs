using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VcpCore.Plugins.Test.ParserTest.NodesTest
{
    public class TestINode
    {
        [Test]
        public void TestINodes()
        {
            TestNode2 myNode = new TestNode2();

            myNode.Nodes = new List<INode>
            {
            new TestNode2 { Value = "Child1" },
            new TestNode2 { Value = "Child2" }
            };

            string value1 = "Child1";
            string value2 = "Child2";
            string value3 = "Parent";
            string value4 = "Node Value";

            myNode.Parent = new TestNode2 { Value = "Parent" };
            myNode.Value = "Node Value";

            // 验证 Nodes 属性
            Assert.That(2, Is.EqualTo(myNode.Nodes.Count()));
            Assert.That(value1, Is.EqualTo(myNode.Nodes.ElementAt(0).Value));
            Assert.That(value2, Is.EqualTo(myNode.Nodes.ElementAt(1).Value));

            // 验证 Parent 属性
            Assert.That(value3, Is.EqualTo(myNode.Parent.Value));

            // 验证 Value 属性
            Assert.That(value4, Is.EqualTo(myNode.Value));
        }
    }

     class TestNode2 : INode
     {
        private IEnumerable<INode> _nodes;
        private INode _parent;
        private string _value;

        public IEnumerable<INode> Nodes
        {
            get { return _nodes; }
            set { _nodes = value; }
        }

        public INode Parent
        {
            get { return _parent; }
            set { _parent = value; }
        }

        public string Value
        {
            get { return _value; }
            set { _value = value; }
        }

     }


}
