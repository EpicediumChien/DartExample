using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VcpCore.Plugins;

namespace VcpCore.Plugins.Test
{
    public class TestRecursiveSelect
    {

        [Test]
        public void TestRecursiveSelect1()
        {
            // 构建一个简单的树结构数据
            var root = new Node { Value = 1 };
            root.Children = new List<Node>
            {
            new Node { Value = 2 },
            new Node { Value = 3, Children = new List<Node> { new Node { Value = 4 } } }
            };
            var source = new List<Node> { root };
            var result = source.RecursiveSelect(node => node.Children);

            // 验证结果是否包含预期的节点值
            CollectionAssert.Contains(result.Select(n => n.Value), 1);
            CollectionAssert.Contains(result.Select(n => n.Value), 2);
            CollectionAssert.Contains(result.Select(n => n.Value), 3);
            CollectionAssert.Contains(result.Select(n => n.Value), 4);

        }

        [Test]
        public void TestRecursiveSelect2()
        {
            // 定义一个简单的源数据
            var source = new List<Node>
            {
            new Node { Value = 1 },
            new Node { Value = 2, Children = new List<Node>
            {
                new Node { Value = 3 },
                new Node { Value = 4 }
            }}
            };

            // 定义选择子元素的函数
            Func<Node, IEnumerable<Node>> childSelector = node => node.Children;

            // 定义投影函数，将节点值乘以 2
            Func<Node, int> selector = node => node.Value * 2;

            var result = source.RecursiveSelect(childSelector, selector);

            // 验证结果是否符合预期
            CollectionAssert.AreEqual(new[] { 2, 4, 6, 8 }, result);

        }

        [Test]
        public void TestRecursiveSelect3()
        {
            // 定义源数据
            var source = new List<Node>
            {
            new Node { Value = 1 },
            new Node { Value = 2 },
            new Node { Value = 3 }
            };

            // 定义获取子元素的函数（这里假设没有子元素）
            Func<Node, IEnumerable<Node>> childSelector = node => new List<Node>();

            // 定义投影函数，将节点值与索引相加
            Func<Node, int, int> selector = (node, index) => node.Value + index;

            var result = source.RecursiveSelect(childSelector, selector);

            // 验证结果是否符合预期
            CollectionAssert.AreEqual(new[] { 1, 3, 5 }, result);

        }

        [Test]
        public void TestRecursiveSelect4()
        {
            // 定义源数据
            var source = new List<Node>
            {
            new Node { Value = 1 },
            new Node { Value = 2, Children = new List<Node>
              {
                new Node { Value = 3 },
                new Node { Value = 4 }
              }}
            };

            // 定义获取子元素的函数
            Func<Node, IEnumerable<Node>> childSelector = node => node.Children;

            // 定义投影函数，将节点值与索引和深度相加
            Func<Node, int, int, int> selector = (node, index, depth) => node.Value + index + depth;

            var result = source.RecursiveSelect(childSelector, selector);

            // 验证结果是否符合预期
            CollectionAssert.AreEqual(new[] { 1, 3, 4, 6 }, result);

        }

        [Test]
        public void TestRecursiveSelect5()
        {
            // 构建源数据
            var source = new List<Node>
        {
            new Node { Value = 1 },
            new Node { Value = 2, Children = new List<Node>
            {
                new Node { Value = 3 },
                new Node { Value = 4, Children = new List<Node>
                {
                    new Node { Value = 5 }
                }}
            }}
        };

            // 子元素选择函数
            Func<Node, IEnumerable<Node>> childSelector = node => node.Children;

            // 投影函数
            Func<Node, int, int, int> selector = (node, index, depth) => node.Value + index + depth;

            var result = source.RecursiveSelect(childSelector, selector);

            // 预期结果
            var expected = new List<int> { 1, 3, 4, 6, 7 };

            CollectionAssert.AreEqual(expected, result);

        }


        class Node
        {
            public int Value { get; set; }
            public List<Node> ? Children { get; set; }
        }

    }
}
