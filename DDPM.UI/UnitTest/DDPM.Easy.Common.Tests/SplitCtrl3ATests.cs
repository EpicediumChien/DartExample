using DDPM.UI.Common.EAEM;
using NGA.UnitTest.PrivateObject;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace DDPM.Easy.Common.Tests
{
    [Apartment(ApartmentState.STA)]
    public class SplitCtrl3ATests
    {
        private SplitCtrl3A? splitCtrl3A;
        private SplitCtrlVM vm;
        private PrivateObject? privateObject;

        [SetUp]
        public void Setup()
        {
            var settings = new ObservableCollection<GridLength>() { new GridLength(), new GridLength(), new GridLength(), new GridLength(), new GridLength(), new GridLength(), new GridLength(), new GridLength() };
            vm = new SplitCtrlVM() { Settings = settings };
            splitCtrl3A = new SplitCtrl3A();
            privateObject = new PrivateObject(splitCtrl3A);
        }

        [Test]
        public void TestConstructor_SplitCtrl3A()
        {
            // Assert
            Assert.That(splitCtrl3A, Is.Not.Null);
            Assert.That(splitCtrl3A.DataContext, Is.EqualTo(privateObject.GetFieldOrProperty("vm")));
        }

        [Test]
        public void TestNew()
        {
            // Act
            var result = splitCtrl3A.New();
            // Assert
            Assert.That(result, Is.InstanceOf<SplitCtrl3A>());
        }

        [Test]
        public void TestCellList()
        {
            // Act
            var CellList = new List<CellObj>();
            splitCtrl3A.CellList = CellList;
            // Assert
            Assert.That(splitCtrl3A.CellList, Is.Not.Null);

            vm.IsVertical = true;
            splitCtrl3A.CellList = CellList;
            // Assert
            Assert.That(splitCtrl3A.CellList, Is.Not.Null);
        }

        [Test]
        public void TestInitCellList()
        {
            try
            {
                splitCtrl3A.InitCellList();
                Assert.True(true);
            }
            catch (Exception ex)
            {
                Assert.Fail("not invoked");
            }
        }

        [Test]
        public void TestUpdateRatioRectsFromSettings()
        {
            var settings = new ObservableCollection<GridLength>() {};
            vm = new SplitCtrlVM() { Settings = settings };
            privateObject = new PrivateObject(splitCtrl3A);
            privateObject.SetFieldOrProperty("vm", vm);
            try
            {
                splitCtrl3A.UpdateRatioRectsFromSettings();
                Assert.True(true);
            }
            catch (Exception ex)
            {
                Assert.Fail("not invoked");
            }

            settings = new ObservableCollection<GridLength>() { new GridLength(), new GridLength(), new GridLength() };
            vm = new SplitCtrlVM() { Settings = settings };
            vm.IsVertical = true;
            privateObject.SetFieldOrProperty("vm", vm);
            try
            {
                splitCtrl3A.UpdateRatioRectsFromSettings();
                Assert.True(true);
            }
            catch (Exception ex)
            {
                Assert.Fail("not invoked");
            }

            vm.IsVertical = false;
            privateObject.SetFieldOrProperty("vm", vm);
            try
            {
                splitCtrl3A.UpdateRatioRectsFromSettings();
                Assert.True(true);
            }
            catch (Exception ex)
            {
                Assert.Fail("not invoked");
            }
        }

        [Test]
        public void TestCellBorders()
        {
            var cellBorders = new List<CellBorder>();
            splitCtrl3A.CellBorders = cellBorders;
            Assert.That(splitCtrl3A.CellBorders, Is.Not.Null);

            vm.IsVertical = true;
            splitCtrl3A.CellBorders = cellBorders;
            Assert.That(splitCtrl3A.CellBorders, Is.Not.Null);
        }

        [Test]
        public void TestVSplitterList()
        {
            // Act
            var VSplitterList = new List<GridSplitter>();
            splitCtrl3A.VSplitterList = VSplitterList;
            // Assert
            Assert.That(splitCtrl3A.VSplitterList, Is.Not.Null);
        }

        [Test]
        public void TestHSplitterList()
        {
            // Act
            var HSplitterList = new List<GridSplitter>();
            splitCtrl3A.HSplitterList = HSplitterList;
            // Assert
            Assert.That(splitCtrl3A.HSplitterList, Is.Not.Null);
        }

        [Test]
        public void TestInitSplitterList()
        {
            try
            {
                splitCtrl3A.InitSplitterList();
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
            Assert.That(splitCtrl3A.DefaultSettings, Is.Not.Null);
        }

        [Test]
        public void TestFriendlyName()
        {
            // Act
            splitCtrl3A.FriendlyName = "FriendlyName";
            // Assert
            Assert.That(splitCtrl3A.FriendlyName, Is.EqualTo("FriendlyName"));
        }
    }
}
