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
    public class SplitCtrl3DTests
    {
        private SplitCtrl3D? splitCtrl3D;
        private SplitCtrlVM vm;
        private PrivateObject? privateObject;

        [SetUp]
        public void Setup()
        {
            var settings = new ObservableCollection<GridLength>() { new GridLength(), new GridLength(), new GridLength(), new GridLength(), new GridLength(), new GridLength(), new GridLength(), new GridLength() };
            vm = new SplitCtrlVM() { Settings = settings };
            splitCtrl3D = new SplitCtrl3D();
            privateObject = new PrivateObject(splitCtrl3D);
        }

        [Test]
        public void TestConstructor_SplitCtrl3D()
        {
            // Assert
            Assert.That(splitCtrl3D, Is.Not.Null);
            Assert.That(splitCtrl3D.DataContext, Is.EqualTo(privateObject.GetFieldOrProperty("vm")));
        }

        [Test]
        public void TestNew()
        {
            // Act
            var result = splitCtrl3D.New();
            // Assert
            Assert.That(result, Is.InstanceOf<SplitCtrl3D>());
        }

        [Test]
        public void TestCellList()
        {
            // Act
            var CellList = new List<CellObj>();
            splitCtrl3D.CellList = CellList;
            // Assert
            Assert.That(splitCtrl3D.CellList, Is.Not.Null);

            vm.IsVertical = true;
            splitCtrl3D.CellList = CellList;
            // Assert
            Assert.That(splitCtrl3D.CellList, Is.Not.Null);
        }

        [Test]
        public void TestInitCellList()
        {
            try
            {
                splitCtrl3D.InitCellList();
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
            privateObject = new PrivateObject(splitCtrl3D);
            privateObject.SetFieldOrProperty("vm", vm);
            try
            {
                splitCtrl3D.UpdateRatioRectsFromSettings();
                vm.Dispose();
                Assert.True(true);
            }
            catch (Exception ex)
            {
                vm.Dispose();
                Assert.Fail("not invoked");
            }

            settings = new ObservableCollection<GridLength>() { new GridLength(), new GridLength(), new GridLength() };
            vm = new SplitCtrlVM() { Settings = settings };
            vm.IsVertical = true;
            privateObject.SetFieldOrProperty("vm", vm);
            try
            {
                splitCtrl3D.UpdateRatioRectsFromSettings();
                Assert.True(true);
            }
            catch (Exception ex)
            {
                vm.Dispose();
                Assert.Fail("not invoked");
            }

            vm.IsVertical = false;
            privateObject.SetFieldOrProperty("vm", vm);
            try
            {
                splitCtrl3D.UpdateRatioRectsFromSettings();
                vm.Dispose();
                Assert.True(true);
            }
            catch (Exception ex)
            {
                vm.Dispose();
                Assert.Fail("not invoked");
            }
        }

        [Test]
        public void TestCellBorders()
        {
            var cellBorders = new List<CellBorder>();
            splitCtrl3D.CellBorders = cellBorders;
            Assert.That(splitCtrl3D.CellBorders, Is.Not.Null);

            vm.IsVertical= true;
            splitCtrl3D.CellBorders = cellBorders;
            Assert.That(splitCtrl3D.CellBorders, Is.Not.Null);
        }

        //[Test]
        //public void TestVSplitterList()
        //{
        //    // Act
        //    var VSplitterList = new List<GridSplitter>();
        //    splitCtrl3D.VSplitterList = VSplitterList;
        //    // Assert
        //    Assert.That(splitCtrl3D.VSplitterList, Is.Not.Null);
        //}

        //[Test]
        //public void TestHSplitterList()
        //{
        //    // Act
        //    var HSplitterList = new List<GridSplitter>();
        //    splitCtrl3D.HSplitterList = HSplitterList;
        //    // Assert
        //    Assert.That(splitCtrl3D.HSplitterList, Is.Not.Null);
        //}

        //[Test]
        //public void TestInitSplitterList()
        //{
        //    try
        //    {
        //        splitCtrl3D.InitSplitterList();
        //        Assert.True(true);
        //    }
        //    catch (Exception ex)
        //    {
        //        Assert.Fail("not invoked");
        //    }
        //}

        [Test]
        public void TestDefaultSettings()
        {
            // Assert
            Assert.That(splitCtrl3D.DefaultSettings, Is.Not.Null);
        }

        [Test]
        public void TestFriendlyName()
        {
            // Act
            splitCtrl3D.FriendlyName = "FriendlyName";
            // Assert
            Assert.That(splitCtrl3D.FriendlyName, Is.EqualTo("FriendlyName"));
        }

        [TearDown]
        public void TearDown()
        {
            if (splitCtrl3D != null)
            {
                splitCtrl3D.Dispose();
                splitCtrl3D = null;
            }
            if (vm != null)
            {
                vm.Dispose();
                vm = null;
            }
        }
    }
}
