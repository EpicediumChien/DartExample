using System.Windows;
using System.Windows.Controls;

namespace DDPM.Easy.Common.Tests
{
    [Apartment(ApartmentState.STA)]
    public class CellObjTests
    {
        private CellObj? cellObja;
        private CellObj? cellObjb;
        private CellObj? cellObjc;

        [SetUp]
        public void Setup()
        {
            cellObja = new CellObj("aa", new Border());
            cellObjb = new CellObj("A");
            cellObjc = new CellObj("B",new CellBorder());
        }

        [Test]
        public void TestConstructor_CellObj()
        {
            // Assert
            Assert.That(cellObja, Is.Not.Null);
            Assert.That(cellObjb, Is.Not.Null);
            Assert.That(cellObjc, Is.Not.Null);
        }

        [Test]
        public void TestName()
        {
            // Act        
            cellObja.Name = "A";
            // Assert
            Assert.That(cellObja.Name, Is.EqualTo("A"));
        }

        [Test]
        public void Testrc()
        {
            // Act        
            var rc=new Rect();
            cellObja.rc = rc;
            // Assert
            Assert.That(cellObja.rc, Is.EqualTo(rc));
        }

        [Test]
        public void Testbd()
        {
            // Act        
            var bd = new Border();
            cellObja.bd = bd;
            // Assert
            Assert.That(cellObja.bd, Is.EqualTo(bd));
        }

        [Test]
        public void TestrcRatio()
        {
            // Act        
            var rcRatio = new Rect();
            cellObja.rcRatio = rcRatio;
            // Assert
            Assert.That(cellObja.rcRatio, Is.EqualTo(rcRatio));
        }

        [Test]
        public void TestCellBd()
        {
            // Act        
            var cellBd = new CellBorder();
            cellObja.CellBd = cellBd;
            // Assert
            Assert.That(cellObja.CellBd, Is.EqualTo(cellBd));
        }

        [TearDown]
        public void TearDown()
        {
            if (cellObja != null)
            {
                cellObja.Dispose();
                cellObja = null;
            }
            if (cellObjb != null)
            {
                cellObjb.Dispose();
                cellObjb = null;
            }
            if (cellObjc != null)
            {
                cellObjc.Dispose();
                cellObjc = null;
            }
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            if (cellObja != null)
            {
                cellObja.Dispose();
                cellObja = null;
            }
            if (cellObjb != null)
            {
                cellObjb.Dispose();
                cellObjb = null;
            }
            if (cellObjc != null)
            {
                cellObjc.Dispose();
                cellObjc = null;
            }
        }
    }
}