using Dell.Client.Framework.UnitTestShared.Tests;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;

namespace VcpCore.Plugins.Test
{
    public class TestTaskLockQueue
    {
        public class TestLinkedNode
        {
            [Test]
            public void TestLinkednode()
            {
                LinkedNode<int> node1 = new LinkedNode<int>();
                node1.Data = 1;

                LinkedNode<int> node2 = new LinkedNode<int>();
                node2.Data = 2;

                LinkedNode<int> node3 = new LinkedNode<int>();
                node3.Data = 3;

                // 建立链接
                node1.Next = node2;
                node2.Next = node3;
                Assert.That(node2, Is.EqualTo(node1.Next));
                Assert.That(node3, Is.EqualTo(node2.Next));
                Assert.IsNull(node3.Next);
            }

        }
    }
}