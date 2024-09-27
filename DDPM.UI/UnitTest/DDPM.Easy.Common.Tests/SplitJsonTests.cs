using NGA.UnitTest.PrivateObject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDPM.Easy.Common.Tests
{
    [Apartment(ApartmentState.STA)]
    public class SplitJsonTests
    {
        private SplitJson_Unused? splitJson_Unused;
        private SplitCtrlVM vm;
        private PrivateObject? privateObject;

        [SetUp]
        public void Setup()
        {
            splitJson_Unused = new SplitJson_Unused();
            privateObject = new PrivateObject(splitJson_Unused);
        }

        [Test]
        public void TestConstructor_SplitJson_Unused()
        {
            // Assert
            Assert.That(splitJson_Unused, Is.Not.Null);
        }

        [Test]
        public void TestCellCount()
        {
            // Act
            splitJson_Unused.CellCount = 1;
            // Assert
            Assert.That(splitJson_Unused.CellCount, Is.EqualTo(1));
        }

        [Test]
        public void TestSplitKey()
        {
            // Act
            splitJson_Unused.SplitKey = 'b';
            // Assert
            Assert.That(splitJson_Unused.SplitKey, Is.EqualTo('b'));
        }

        [Test]
        public void TestSettings()
        {
            // Act
            splitJson_Unused.Settings = new List<double>();
            // Assert
            Assert.That(splitJson_Unused.Settings, Is.Not.Null);
        }

        [Test]
        public void TestFriendlyName()
        {
            // Act
            splitJson_Unused.FriendlyName = "a";
            // Assert
            Assert.That(splitJson_Unused.FriendlyName, Is.EqualTo("a"));
        }

        [Test]
        public void TestCustomId()
        {
            // Act
            splitJson_Unused.CustomId = 2;
            // Assert
            Assert.That(splitJson_Unused.CustomId, Is.EqualTo(2));
        }

        [Test]
        public void TestMessage()
        {
            // Act
            splitJson_Unused.Message = "Message";
            // Assert
            Assert.That(splitJson_Unused.Message, Is.EqualTo("Message"));
        }

    }
}
