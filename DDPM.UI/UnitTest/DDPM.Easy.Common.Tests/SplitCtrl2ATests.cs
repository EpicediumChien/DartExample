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
    public class SplitCtrl2ATests
    {
        private SplitCtrl2A? splitCtrl2A;
        private SplitCtrlVM vm;
        private PrivateObject? privateObject;

        [SetUp]
        public void Setup()
        {
            splitCtrl2A = new SplitCtrl2A();
            privateObject = new PrivateObject(splitCtrl2A);
        }

        [Test]
        public void TestConstructor_SplitCtrl2A()
        {
            // Assert
            Assert.That(splitCtrl2A, Is.Not.Null);
            Assert.That(splitCtrl2A.DataContext, Is.EqualTo(privateObject.GetFieldOrProperty("vm")));
        }

        [Test]
        public void TestNew()
        {
            // Act
            var result = splitCtrl2A.New();
            // Assert
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public void TestCellList()
        {
            // Act
            var CellList = new List<CellObj>();
            splitCtrl2A.CellList = CellList;
            // Assert
            Assert.That(splitCtrl2A.CellList, Is.Not.Null);
        }

        [Test]
        public void TestInitCellList()
        {
            try
            {
                splitCtrl2A.InitCellList();
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
            splitCtrl2A.VSplitterList = VSplitterList;
            // Assert
            Assert.That(splitCtrl2A.VSplitterList, Is.Not.Null);
        }

        [Test]
        public void TestHSplitterList()
        {
            // Act
            var HSplitterList = new List<GridSplitter>();
            splitCtrl2A.HSplitterList = HSplitterList;
            // Assert
            Assert.That(splitCtrl2A.HSplitterList, Is.Not.Null);
        }

        [Test]
        public void TestInitSplitterList()
        {
            try
            {
                splitCtrl2A.InitSplitterList();
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
            Assert.That(splitCtrl2A.DefaultSettings, Is.Not.Null);
        }

        [Test]
        public void TestFriendlyName()
        {
            // Act
            splitCtrl2A.FriendlyName = "FriendlyName";
            // Assert
            Assert.That(splitCtrl2A.FriendlyName, Is.EqualTo("FriendlyName"));
        }
    }
}
