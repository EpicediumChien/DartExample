using NGA.UnitTest.PrivateObject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace DDPM.Easy.Common.Tests
{
    [Apartment(ApartmentState.STA)]
    public class SplitCtrl2CTests
    {
        private SplitCtrl2C? splitCtrl2C;
        private SplitCtrlVM vm;
        private PrivateObject? privateObject;

        [SetUp]
        public void Setup()
        {
            splitCtrl2C = new SplitCtrl2C();
            privateObject = new PrivateObject(splitCtrl2C);
        }

        [Test]
        public void TestConstructor_SplitCtrl2C()
        {
            // Assert
            Assert.That(splitCtrl2C, Is.Not.Null);
            Assert.That(splitCtrl2C.DataContext, Is.EqualTo(privateObject.GetFieldOrProperty("vm")));
        }

        [Test]
        public void TestNew()
        {
            // Act
            var result = splitCtrl2C.New();
            // Assert
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public void TestCellList()
        {
            // Act
            var CellList = new List<CellObj>();
            splitCtrl2C.CellList = CellList;
            // Assert
            Assert.That(splitCtrl2C.CellList, Is.Not.Null);
        }

        [Test]
        public void TestInitCellList()
        {
            try
            {
                splitCtrl2C.InitCellList();
                Assert.True(true);
            }
            catch (Exception ex)
            {
                Assert.Fail("not invoked");
            }
        }

        [Test]
        public void TestVSplitterList()
        {
            // Act
            var VSplitterList = new List<GridSplitter>();
            splitCtrl2C.VSplitterList = VSplitterList;
            // Assert
            Assert.That(splitCtrl2C.VSplitterList, Is.Not.Null);
        }

        [Test]
        public void TestHSplitterList()
        {
            // Act
            var HSplitterList = new List<GridSplitter>();
            splitCtrl2C.HSplitterList = HSplitterList;
            // Assert
            Assert.That(splitCtrl2C.HSplitterList, Is.Not.Null);
        }

        [Test]
        public void TestInitSplitterList()
        {
            try
            {
                splitCtrl2C.InitSplitterList();
                Assert.True(true);
            }
            catch (Exception ex)
            {
                Assert.Fail("not invoked");
            }
        }

        [Test]
        public void TestDefaultSettings()
        {
            // Assert
            Assert.That(splitCtrl2C.DefaultSettings, Is.Not.Null);
        }

        [Test]
        public void TestFriendlyName()
        {
            // Act
            splitCtrl2C.FriendlyName = "FriendlyName";
            // Assert
            Assert.That(splitCtrl2C.FriendlyName, Is.EqualTo("FriendlyName"));
        }
    }
}
