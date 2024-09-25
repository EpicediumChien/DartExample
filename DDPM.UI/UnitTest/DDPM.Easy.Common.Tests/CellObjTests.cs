using System.Windows;
using System.Windows.Controls;

namespace DDPM.Easy.Common.Tests
{
    [Apartment(ApartmentState.STA)]
    public class CellObjTests
    {
        private CellObj? cellObj;

        [SetUp]
        public void Setup()
        {
            cellObj=new CellObj("aa", new Border());
        }

        [Test]
        public void TestConstructor_CellObj()
        {
            // Assert
            Assert.That(cellObj, Is.Not.Null);
        }

        [Test]
        public void TestName()
        {
            // Act        
            cellObj.Name = "A";
            // Assert
            Assert.That(cellObj.Name, Is.EqualTo("A"));
        }

        [Test]
        public void Testrc()
        {
            // Act        
            var rc=new Rect();
            cellObj.rc = rc;
            // Assert
            Assert.That(cellObj.rc, Is.EqualTo(rc));
        }

        [Test]
        public void Testbd()
        {
            // Act        
            var bd = new Border();
            cellObj.bd = bd;
            // Assert
            Assert.That(cellObj.bd, Is.EqualTo(bd));
        }
    }
}