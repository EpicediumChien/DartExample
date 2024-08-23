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
        public class TestLinkedListQueue
        {

            [Test]
            public void TestEnqueue()
            {
                var node1 = new LinkedNode<int> { Data = 1, Next = null };
                int count1 = 1;

                var node2 = new LinkedNode<int> { Data = 2, Next = null };
                int count2 = 2;

                var node3 = new LinkedNode<int> { Data = 3, Next = null };
                int count3 = 3;

                LinkedListQueue<int> linkedListQueue = new LinkedListQueue<int>();
                linkedListQueue.Enqueue(1);

                PrivateObject privateObject = new PrivateObject(linkedListQueue);
                var getcount1 = privateObject.GetFieldOrProperty("_count");
                var getnode1 = (LinkedNode<int>)privateObject.GetFieldOrProperty("_node");

                Assert.That(count1, Is.EqualTo(getcount1));
                Assert.That(node1.Data, Is.EqualTo(getnode1.Data));


                linkedListQueue.Enqueue(2);
                var getcount2 = privateObject.GetFieldOrProperty("_count");
                var getnode2 = (LinkedNode<int>)privateObject.GetFieldOrProperty("_node");

                Assert.That(count2, Is.EqualTo(getcount2));
                Assert.That(node1.Data, Is.EqualTo(getnode2.Data));
                Assert.That(node2.Data, Is.EqualTo(getnode2.Next.Data));

                linkedListQueue.Enqueue(3);
                var getcount3 = privateObject.GetFieldOrProperty("_count");
                var getnode3 = (LinkedNode<int>)privateObject.GetFieldOrProperty("_node");

                Assert.That(count3, Is.EqualTo(getcount3));
                Assert.That(node1.Data, Is.EqualTo(getnode2.Data));
                Assert.That(node2.Data, Is.EqualTo(getnode2.Next.Data));
                Assert.That(node3.Data, Is.EqualTo(getnode2.Next.Next.Data));

            }

            [Test]
            public void TestLinkednodeIsEmpty()
            {
                int count = 0;
                LinkedListQueue<int> linkedListQueue = new LinkedListQueue<int>();
                var resullt = linkedListQueue.IsEmpty();
                Assert.IsTrue(resullt);
                PrivateObject privateObject = new PrivateObject(linkedListQueue);
                var getcount = privateObject.GetFieldOrProperty("_count");
                Assert.That(count, Is.EqualTo(getcount));
            }

            [Test]
            public void TestLinkednodeCount()
            {
                int count = 2;
                LinkedListQueue<int> linkedListQueue = new LinkedListQueue<int>();
                PrivateObject privateObject = new PrivateObject(linkedListQueue);
                privateObject.SetFieldOrProperty("_count", 2);
                var resullt = linkedListQueue.Count();
                Assert.That(count, Is.EqualTo(resullt));
            }

            [Test]
            public void TestDequeue()
            {
                LinkedListQueue<int> linkedListQueue = new LinkedListQueue<int>();
                PrivateObject privateObject = new PrivateObject(linkedListQueue);

                LinkedNode<int> node = new LinkedNode<int>();
                var resullt1 = linkedListQueue.Dequeue();
                int nodedefault = default(int);
                Assert.That(nodedefault, Is.EqualTo(resullt1));  //_node=null

                var node2 = new LinkedNode<int> { Data = 10, Next = null };
                int count2 = 0;

                linkedListQueue.Enqueue(10);
                int result2 = linkedListQueue.Dequeue();  //_node!=null

                var getcount2 = privateObject.GetFieldOrProperty("_count");
                var getnode2 = (LinkedNode<int>)privateObject.GetFieldOrProperty("_node");

                Assert.That(node2.Data, Is.EqualTo(result2));
                Assert.That(count2, Is.EqualTo(getcount2));
                Assert.IsNull(getnode2);

            }

            [Test]
            public void TestinkedNodeClear()
            {
                LinkedListQueue<int> linkedListQueue = new LinkedListQueue<int>();
                PrivateObject privateObject = new PrivateObject(linkedListQueue);

                LinkedNode<int> node = new LinkedNode<int>();
                var resullt1 = linkedListQueue.Clear();
                bool clearResult = true;
                Assert.That(clearResult, Is.EqualTo(resullt1));  //_node=null

                var node2 = new LinkedNode<int> { Data = 10, Next = null };
                int count2 = 0;

                linkedListQueue.Enqueue(1);
                linkedListQueue.Enqueue(2);
                linkedListQueue.Enqueue(3);

                bool result2 = linkedListQueue.Clear();
                var getcount2 = privateObject.GetFieldOrProperty("_count");
                var getnode2 = (LinkedNode<int>)privateObject.GetFieldOrProperty("_node");

                Assert.IsTrue(result2);
                Assert.That(count2, Is.EqualTo(getcount2));
                Assert.IsNull(getnode2);

            }
        }

        public class TestTaskLockQueueOverride
        {
            [Test]
            public void TestOverrideEnqueue()
            {
                TaskLockQueue<int> queue = new TaskLockQueue<int>();
                LinkedListQueue<int> linkedListQueue = new LinkedListQueue<int>();
                queue.Enqueue(1);
                queue.Enqueue(2);
                var Resultcount = queue.Count();
                int count = 2;
                Assert.That(count, Is.EqualTo(Resultcount));
            }

            [Test]
            public void TestOverrideIsEmpty()
            {
                TaskLockQueue<int> queue = new TaskLockQueue<int>();
                LinkedListQueue<int> linkedListQueue = new LinkedListQueue<int>();
                queue.IsEmpty();
                var Resultcount = queue.Count();
                int count = 0;
                Assert.That(count, Is.EqualTo(Resultcount));
            }

            [Test]
            public void TestOverrideCount()
            {
                TaskLockQueue<int> queue = new TaskLockQueue<int>();
                LinkedListQueue<int> linkedListQueue = new LinkedListQueue<int>();
                queue.Count();
                int count = 0;
                PrivateObject privateObject = new PrivateObject(linkedListQueue);
                var getcount1 = privateObject.GetFieldOrProperty("_count");
                Assert.That(count, Is.EqualTo(getcount1));
            }

            [Test]
            public void TestOverrideDequeue()
            {
                TaskLockQueue<int> queue = new TaskLockQueue<int>();
                LinkedListQueue<int> linkedListQueue = new LinkedListQueue<int>();
                int defaultint = default(int);
                var result = queue.Dequeue();
                Assert.That(defaultint, Is.EqualTo(result));

                int in1 = 1;
                queue.Enqueue(1);
                var result2 = queue.Dequeue();
                Assert.That(in1, Is.EqualTo(result2));
            }

            [Test]
            public void TestOverrideClear()
            {
                TaskLockQueue<int> queue = new TaskLockQueue<int>();
                LinkedListQueue<int> linkedListQueue = new LinkedListQueue<int>();
                var result = queue.Clear();
                bool clearresult = true;
                Assert.That(clearresult, Is.EqualTo(result));
            }
        }
    }
}
