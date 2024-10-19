using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dell.Client.Framework.UnitTestShared.Tests;
using NetworkKVM.Plugins;

namespace DDPM.SA.Plugins.User.NetworkKVM.Test
{
    public class TestTaskLockQueue
    {
        public class TestLinkedNode
        {
            [Test]
            public void TestLinkednode()
            {
                LinkedNode<int> Node1 = new LinkedNode<int>();
                Node1.Data = 1;
                LinkedNode<int> Node2 = new LinkedNode<int>();
                Node2.Data = 2;
                LinkedNode<int> Node3 = new LinkedNode<int>();
                Node3.Data = 3;

                Node1.Next = Node2; 
                Node2.Next = Node3;
                Assert.That(Node2, Is.EqualTo(Node1.Next));
                Assert.That(Node3, Is.EqualTo(Node2.Next));
                Assert.IsNull(Node3.Next);
            }
        }

        public class TestLinkedListQueue
        {
            [Test]
            public void TestEnqueue()
            {
                var Node1 = new LinkedNode<int> { Data = 11, Next = null };
                int count1 = 1;

                var Node2 = new LinkedNode<int> { Data = 22, Next = null };
                int count2 = 2;

                var Node3 = new LinkedNode<int> { Data = 33, Next = null };
                int count3 = 3;

                LinkedListQueue<int> linkedListQueue = new LinkedListQueue<int>();
                linkedListQueue.Enqueue(11);

                PrivateObject privateObject = new PrivateObject(linkedListQueue);
                var getcount1 = privateObject.GetFieldOrProperty("_count");
                var getnode1 = (LinkedNode<int>)privateObject.GetFieldOrProperty("_node");

                Assert.That(count1, Is.EqualTo(getcount1));
                Assert.That(Node1.Data, Is.EqualTo(getnode1.Data));

                linkedListQueue.Enqueue(22);
                var getcount2 = privateObject.GetFieldOrProperty("_count");
                var getnode2 = (LinkedNode<int>)privateObject.GetFieldOrProperty("_node");

                Assert.That(count2, Is.EqualTo(getcount2));
                Assert.That(Node1.Data, Is.EqualTo(getnode2.Data));
                Assert.That(Node2.Data, Is.EqualTo(getnode2.Next.Data));

                linkedListQueue.Enqueue(33);
                var getcount3 = privateObject.GetFieldOrProperty("_count");
                var getnode3 = (LinkedNode<int>)privateObject.GetFieldOrProperty("_node");

                Assert.That(count3, Is.EqualTo(getcount3));
                Assert.That(Node1.Data, Is.EqualTo(getnode2.Data));
                Assert.That(Node2.Data, Is.EqualTo(getnode2.Next.Data));
                Assert.That(Node3.Data, Is.EqualTo(getnode2.Next.Next.Data));
            }

            [Test]
            public void TestLinkedListQueueIsEmpty()
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
            public void TestLinkedListQueueClear()
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
                var Node1 = new LinkedNode<int> { Data = 11, Next = null };
                int count1 = 1;
                TaskLockQueue<int> queue = new TaskLockQueue<int>();
                LinkedListQueue<int> linkedListQueue = new LinkedListQueue<int>();
                queue.Enqueue(11);
                linkedListQueue.Enqueue(11);
                PrivateObject privateObject = new PrivateObject(linkedListQueue);
                var getcount1 = privateObject.GetFieldOrProperty("_count");
                var getnode1 = (LinkedNode<int>)privateObject.GetFieldOrProperty("_node");
                Assert.That(count1, Is.EqualTo(getcount1));
                Assert.That(Node1.Data, Is.EqualTo(getnode1.Data));
            }

            [Test]
            public void TestOverrideIsEmpty()
            {
                int count = 0;
                TaskLockQueue<int> queue = new TaskLockQueue<int>();
                LinkedListQueue<int> linkedListQueue = new LinkedListQueue<int>();
                var result=queue.IsEmpty();
                PrivateObject privateObject = new PrivateObject(linkedListQueue);
                var getcount=privateObject.GetFieldOrProperty("_count");
                Assert.IsTrue(result);
                Assert.That(count, Is.EqualTo(getcount));
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
