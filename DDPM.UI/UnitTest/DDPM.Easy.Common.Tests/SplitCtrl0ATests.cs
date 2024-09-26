using Moq;
using NGA.UnitTest.PrivateObject;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace DDPM.Easy.Common.Tests
{
    [Apartment(ApartmentState.STA)]
    public class SplitCtrl0ATests
    {
        private SplitCtrl0A? splitCtrl0A;
        private SplitCtrlVM vm;
        private PrivateObject? privateObject;

        [SetUp]
        public void Setup()
        {
            splitCtrl0A = new SplitCtrl0A();
            privateObject = new PrivateObject(splitCtrl0A);
        }

        [Test]
        public void TestConstructor_SplitCtrl0A()
        {
            // Assert
            Assert.That(splitCtrl0A, Is.Not.Null);
            Assert.That(splitCtrl0A.DataContext, Is.EqualTo(privateObject.GetFieldOrProperty("vm")));
        }

        [Test]
        public void TestNew()
        {
            // Act
            var result = splitCtrl0A.New();
            // Assert
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public void TestCellList()
        {
            // Act
            var CellList = new List<CellObj>();
            splitCtrl0A.CellList=CellList;
            // Assert
            Assert.That(splitCtrl0A.CellList, Is.Not.Null);
        }

        [Test]
        public void TestInitCellList()
        {
            try
            {
                splitCtrl0A.InitCellList();
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
            splitCtrl0A.VSplitterList = VSplitterList;
            // Assert
            Assert.That(splitCtrl0A.VSplitterList, Is.Not.Null);
        }

        [Test]
        public void TestHSplitterList()
        {
            // Act
            var HSplitterList = new List<GridSplitter>();
            splitCtrl0A.HSplitterList = HSplitterList;
            // Assert
            Assert.That(splitCtrl0A.HSplitterList, Is.Not.Null);
        }

                [Test]
        public void TestInitSplitterList()
        {
            try
            {
                splitCtrl0A.InitSplitterList();
                Assert.True(true);
            }
            catch (Exception ex)
            {
                Assert.Fail("not invoked");
            }
        }

        [Test]
        public void TestSettings()
        {
            // Act
            var Settings = new List<double>();
            splitCtrl0A.Settings = Settings;
            // Assert
            Assert.That(splitCtrl0A.Settings, Is.Not.Null);
        }

        [Test]
        public void TestFriendlyName()
        {
            // Act
            splitCtrl0A.FriendlyName = "FriendlyName";
            // Assert
            Assert.That(splitCtrl0A.FriendlyName, Is.EqualTo("FriendlyName"));
        }

        [Test]
        public void TestIsEditable()
        {
            // Act
            splitCtrl0A.IsEditable = true;
            // Assert
            Assert.That(splitCtrl0A.IsEditable, Is.EqualTo(true));
        }

        [Test]
        public void TestSplitMode()
        {
            // Act
            var SplitMode = new eSplitModes();
            splitCtrl0A.SplitMode = SplitMode;
            // Assert
            Assert.That(splitCtrl0A.SplitMode, Is.EqualTo(SplitMode));
        }


    }
}
