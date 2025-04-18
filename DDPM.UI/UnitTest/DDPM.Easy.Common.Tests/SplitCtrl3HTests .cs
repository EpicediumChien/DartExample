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
    public class SplitCtrl3HTests
    {
        private SplitCtrl3H? splitCtrl3H;
        private SplitCtrlVM vm;
        private PrivateObject? privateObject;

        [SetUp]
        public void Setup()
        {
            var settings = new ObservableCollection<GridLength>() { new GridLength(), new GridLength(), new GridLength(), new GridLength(), new GridLength(), new GridLength(), new GridLength(), new GridLength() };
            vm = new SplitCtrlVM() { Settings = settings };
            splitCtrl3H = new SplitCtrl3H();
            privateObject = new PrivateObject(splitCtrl3H);
        }

        [Test]
        public void TestConstructor_SplitCtrl3H()
        {
            // Assert
            Assert.That(splitCtrl3H, Is.Not.Null);
            Assert.That(splitCtrl3H.DataContext, Is.EqualTo(privateObject.GetFieldOrProperty("vm")));
        }

        [Test]
        public void TestNew()
        {
            // Act
            var result = splitCtrl3H.New();
            // Assert
            Assert.That(result, Is.InstanceOf<SplitCtrl3H>());
        }

        [Test]
        public void TestCellList()
        {
            // Act
            var CellList = new List<CellObj>();
            splitCtrl3H.CellList = CellList;
            // Assert
            Assert.That(splitCtrl3H.CellList, Is.Not.Null);

            vm.IsVertical = true;
            splitCtrl3H.CellList = CellList;
            // Assert
            Assert.That(splitCtrl3H.CellList, Is.Not.Null);
        }

        [Test]
        public void TestInitCellList()
        {
            try
            {
                splitCtrl3H.InitCellList();
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
            privateObject = new PrivateObject(splitCtrl3H);
            privateObject.SetFieldOrProperty("vm", vm);
            try
            {
                splitCtrl3H.UpdateRatioRectsFromSettings();
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
                splitCtrl3H.UpdateRatioRectsFromSettings();
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
                splitCtrl3H.UpdateRatioRectsFromSettings();
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
            splitCtrl3H.CellBorders = cellBorders;
            Assert.That(splitCtrl3H.CellBorders, Is.Not.Null);

            vm.IsVertical= true;
            splitCtrl3H.CellBorders = cellBorders;
            Assert.That(splitCtrl3H.CellBorders, Is.Not.Null);
        }

        //[Test]
        //public void TestVSplitterList()
        //{
        //    // Act
        //    var VSplitterList = new List<GridSplitter>();
        //    splitCtrl3H.VSplitterList = VSplitterList;
        //    // Assert
        //    Assert.That(splitCtrl3H.VSplitterList, Is.Not.Null);
        //}

        //[Test]
        //public void TestHSplitterList()
        //{
        //    // Act
        //    var HSplitterList = new List<GridSplitter>();
        //    splitCtrl3H.HSplitterList = HSplitterList;
        //    // Assert
        //    Assert.That(splitCtrl3H.HSplitterList, Is.Not.Null);
        //}

        //[Test]
        //public void TestInitSplitterList()
        //{
        //    try
        //    {
        //        splitCtrl3H.InitSplitterList();
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
            Assert.That(splitCtrl3H.DefaultSettings, Is.Not.Null);
        }

        [Test]
        public void TestFriendlyName()
        {
            // Act
            splitCtrl3H.FriendlyName = "FriendlyName";
            // Assert
            Assert.That(splitCtrl3H.FriendlyName, Is.EqualTo("FriendlyName"));
        }

        [TearDown]
        public void TearDown()
        {
            if (splitCtrl3H != null)
            {
                splitCtrl3H.Dispose();
                splitCtrl3H = null;
            }
            if (vm != null)
            {
                vm.Dispose();
                vm = null;
            }
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            if (splitCtrl3H != null)
            {
                splitCtrl3H.Dispose();
                splitCtrl3H = null;
            }
            if (vm != null)
            {
                vm.Dispose();
                vm = null;
            }

            privateObject = null;
        }
    }
}
