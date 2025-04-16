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
    public class SplitCtrl6JTests
    {
        private SplitCtrl6J? splitCtrl6J;
        private SplitCtrlVM vm;
        private PrivateObject? privateObject;

        [SetUp]
        public void Setup()
        {
            var settings = new ObservableCollection<GridLength>() { new GridLength(), new GridLength(), new GridLength(), new GridLength(), new GridLength(), new GridLength(), new GridLength(), new GridLength() };
            vm = new SplitCtrlVM() { Settings = settings };
            splitCtrl6J = new SplitCtrl6J();
            privateObject = new PrivateObject(splitCtrl6J);
        }

        [Test]
        public void TestConstructor_SplitCtrl6J()
        {
            // Assert
            Assert.That(splitCtrl6J, Is.Not.Null);
            Assert.That(splitCtrl6J.DataContext, Is.EqualTo(privateObject.GetFieldOrProperty("vm")));
        }

        [Test]
        public void TestNew()
        {
            // Act
            var result = splitCtrl6J.New();
            // Assert
            Assert.That(result, Is.InstanceOf<SplitCtrl6J>());
        }

        [Test]
        public void TestCellList()
        {
            // Act
            var CellList = new List<CellObj>();
            splitCtrl6J.CellList = CellList;
            // Assert
            Assert.That(splitCtrl6J.CellList, Is.Not.Null);

            vm.IsVertical = true;
            splitCtrl6J.CellList = CellList;
            // Assert
            Assert.That(splitCtrl6J.CellList, Is.Not.Null);
        }

        [Test]
        public void TestInitCellList()
        {
            try
            {
                splitCtrl6J.InitCellList();
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
            var settings = new ObservableCollection<GridLength>() { };
            vm = new SplitCtrlVM() { Settings = settings };
            privateObject = new PrivateObject(splitCtrl6J);
            privateObject.SetFieldOrProperty("vm", vm);
            try
            {
                splitCtrl6J.UpdateRatioRectsFromSettings();
                vm.Dispose();
                Assert.True(true);
            }
            catch (Exception ex)
            {
                vm.Dispose();
                Assert.Fail("not invoked");
            }

            settings = new ObservableCollection<GridLength>() { new GridLength(), new GridLength(), new GridLength(), new GridLength(), new GridLength() };
            vm = new SplitCtrlVM() { Settings = settings };
            vm.IsVertical = true;
            privateObject.SetFieldOrProperty("vm", vm);
            try
            {
                splitCtrl6J.UpdateRatioRectsFromSettings();
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
                splitCtrl6J.UpdateRatioRectsFromSettings();
                vm.Dispose();
                Assert.True(true);
            }
            catch (Exception ex)
            {
                vm.Dispose();
                Assert.Fail("not invoked");
            }
        }

        //[Test]
        //public void TestVSplitterList()
        //{
        //    // Act
        //    var VSplitterList = new List<GridSplitter>();
        //    splitCtrl6J.VSplitterList = VSplitterList;
        //    // Assert
        //    Assert.That(splitCtrl6J.VSplitterList, Is.Not.Null);
        //}

        //[Test]
        //public void TestHSplitterList()
        //{
        //    // Act
        //    var HSplitterList = new List<GridSplitter>();
        //    splitCtrl6J.HSplitterList = HSplitterList;
        //    // Assert
        //    Assert.That(splitCtrl6J.HSplitterList, Is.Not.Null);
        //}

        //[Test]
        //public void TestInitSplitterList()
        //{
        //    try
        //    {
        //        splitCtrl6J.InitSplitterList();
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
            Assert.That(splitCtrl6J.DefaultSettings, Is.Not.Null);
        }

        [Test]
        public void TestFriendlyName()
        {
            // Act
            splitCtrl6J.FriendlyName = "FriendlyName";
            // Assert
            Assert.That(splitCtrl6J.FriendlyName, Is.EqualTo("FriendlyName"));
        }

        [TearDown]
        public void TearDown()
        {
            if (splitCtrl6J != null)
            {
                splitCtrl6J.Dispose();
                splitCtrl6J = null;
            }
            if (vm != null)
            {
                vm.Dispose();
                vm = null;
            }
        }

    }
}
