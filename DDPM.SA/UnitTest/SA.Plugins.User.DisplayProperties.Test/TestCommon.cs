using System.Collections;
using VcpCore.Common;

namespace DDPM.SA.Plugins.User.DisplayProperties.Test
{
    public class TestCommon
    {
        private ArrayList list1 = new ArrayList();

        public class TestNoSroHashTable
        {
            //NoSroHashTable HashTable = new NoSroHashTable();

            [Test]
            public void TestAddKeys()
            {
                NoSroHashTable HashTable = new NoSroHashTable();
                object key1 = "key1";
                object value1 = "Value1";
                HashTable.Add(key1, value1);
                Assert.That(value1, Is.EqualTo(HashTable[key1]));
                Assert.Contains(key1, (ICollection)HashTable.Keys);
            }

            [Test]
            public void TestClearKey()
            {
                NoSroHashTable HashTable = new NoSroHashTable();
                HashTable.Add("key1", "value1");
                HashTable.Add("key2", "value2");
                HashTable.Clear();
                Assert.That(0, Is.EqualTo(HashTable.Count));
                Assert.That(0, Is.EqualTo(((ICollection)HashTable.Keys).Count));
            }

            [Test]
            public void TestRemovekey()
            {
                NoSroHashTable HashTable = new NoSroHashTable();
                object key1 = "key1";
                object value1 = "Value1";
                HashTable.Remove(key1);
                Assert.That(0, Is.EqualTo(HashTable.Count));
                Assert.IsFalse(HashTable.ContainsKey(key1));
            }
        }

        public class DDMiMessagingMsgTest
        {
            [Test]
            public void TestDDMiMessagingMsg()
            {
                var OpType = "Type1";
                var message = "This is a message1";
                DDMiMessagingMsg dDMiMessagingMsg = new DDMiMessagingMsg(OpType, message);
                Assert.That(OpType, Is.EqualTo(dDMiMessagingMsg.OpType));
                Assert.That(message, Is.EqualTo(dDMiMessagingMsg.message));
            }
        }

        public class SendNameMsgTest
        {
            [Test]
            public void TestSendNameMsg()
            {
                string sender = "John";
                string op = "Type2";
                string message = "Message2";
                SendNameMsg sedmsg = new SendNameMsg(sender, op, message);
                Assert.That(sender, Is.EqualTo(sedmsg.sender));
                Assert.That(op, Is.EqualTo(sedmsg.OpType));
                Assert.That(message, Is.EqualTo(sedmsg.message));
            }
        }

        public class PipeNameMsgTest
        {
            [Test]
            public void TestPipeNameMsg()
            {
                string sender = "Alice";
                string op = "Type3";
                string message = "Message3";
                EDID monitoredid = new EDID() { Edid = "A123456" };
                PipeNameMsg pipeNameMsg = new PipeNameMsg(sender, op, message, monitoredid);
                Assert.That(monitoredid, Is.EqualTo(pipeNameMsg.MonitorEDID));
                Assert.That(sender, Is.EqualTo(pipeNameMsg.sender));
                Assert.That(op, Is.EqualTo(pipeNameMsg.OpType));
                Assert.That(message, Is.EqualTo(pipeNameMsg.message));
            }
        }

        public class DDMBorkerMsgTest
        {
            [Test]
            public void TestDDMBorkerMsg()
            {
                string sender = "Lucy";
                string op = "Type4";
                string message = "Message4";
                EDID deviceInfo = new EDID();
                DDMBorkerMsg dDMBorkerMsg = new DDMBorkerMsg(sender, op, message, deviceInfo);
                Assert.That(deviceInfo, Is.EqualTo(dDMBorkerMsg.DeviceInfo));
                Assert.That(sender, Is.EqualTo(dDMBorkerMsg.sender));
                Assert.That(op, Is.EqualTo(dDMBorkerMsg.OpType));
                Assert.That(message, Is.EqualTo(dDMBorkerMsg.message));
            }

            [Test]
            public void TestToString()
            {
                string sender = "Lucy";
                string op = "Type4";
                string message = "Message4";
                EDID deviceInfo = new EDID();
                DDMBorkerMsg dDMBorkerMsg = new DDMBorkerMsg(sender, op, message, deviceInfo);
                string actual = dDMBorkerMsg.ToString();
                string expected = $"Sender:{sender}, OP:{op}, Msg:{message}, DevInfo:{deviceInfo.ToString()}";
                Assert.That(actual, Is.EqualTo(expected));
            }
        }
    }
}