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
    public class SplitCtrl4ETests
    {
        private SplitCtrl4E? splitCtrl4E;
        private SplitCtrlVM vm;
        private PrivateObject? privateObject;

        [SetUp]
        public void Setup()
        {
            splitCtrl4E = new SplitCtrl4E();
            privateObject = new PrivateObject(splitCtrl4E);
        }

        [Test]
        public void TestConstructor_SplitCtrl4E()
        {
            // Assert
            Assert.That(splitCtrl4E, Is.Not.Null);
            Assert.That(splitCtrl4E.DataContext, Is.EqualTo(privateObject.GetFieldOrProperty("vm")));
        }

        [Test]
        public void TestNew()
        {
            // Act
            var result = splitCtrl4E.New();
            // Assert
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public void TestCellList()
        {
            // Act
            var CellList = new List<CellObj>();
            splitCtrl4E.CellList = CellList;
            // Assert
            Assert.That(splitCtrl4E.CellList, Is.Not.Null);
        }

        [Test]
        public void TestInitCellList()
        {
            try
            {
                splitCtrl4E.InitCellList();
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
            splitCtrl4E.VSplitterList = VSplitterList;
            // Assert
            Assert.That(splitCtrl4E.VSplitterList, Is.Not.Null);
        }

        [Test]
        public void TestHSplitterList()
        {
            // Act
            var HSplitterList = new List<GridSplitter>();
            splitCtrl4E.HSplitterList = HSplitterList;
            // Assert
            Assert.That(splitCtrl4E.HSplitterList, Is.Not.Null);
        }

        [Test]
        public void TestInitSplitterList()
        {
            try
            {
                splitCtrl4E.InitSplitterList();
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
            Assert.That(splitCtrl4E.DefaultSettings, Is.Not.Null);
        }

        [Test]
        public void TestFriendlyName()
        {
            // Act
            splitCtrl4E.FriendlyName = "FriendlyName";
            // Assert
            Assert.That(splitCtrl4E.FriendlyName, Is.EqualTo("FriendlyName"));
        }
    }
}
