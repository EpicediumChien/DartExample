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
    public class SplitCtrl6FTests
    {
        private SplitCtrl6F? splitCtrl6F;
        private SplitCtrlVM vm;
        private PrivateObject? privateObject;

        [SetUp]
        public void Setup()
        {
            var settings = new ObservableCollection<GridLength>() { new GridLength(), new GridLength(), new GridLength(), new GridLength(), new GridLength(), new GridLength(), new GridLength(), new GridLength() };
            vm = new SplitCtrlVM() { Settings = settings };
            splitCtrl6F = new SplitCtrl6F();
            privateObject = new PrivateObject(splitCtrl6F);
        }

        [Test]
        public void TestConstructor_SplitCtrl6F()
        {
            // Assert
            Assert.That(splitCtrl6F, Is.Not.Null);
            Assert.That(splitCtrl6F.DataContext, Is.EqualTo(privateObject.GetFieldOrProperty("vm")));
        }

        [Test]
        public void TestNew()
        {
            // Act
            var result = splitCtrl6F.New();
            // Assert
            Assert.That(result, Is.InstanceOf<SplitCtrl6F>());
        }

        [Test]
        public void TestCellList()
        {
            // Act
            var CellList = new List<CellObj>();
            splitCtrl6F.CellList = CellList;
            // Assert
            Assert.That(splitCtrl6F.CellList, Is.Not.Null);

            vm.IsVertical = true;
            splitCtrl6F.CellList = CellList;
            // Assert
            Assert.That(splitCtrl6F.CellList, Is.Not.Null);
        }

        [Test]
        public void TestInitCellList()
        {
            try
            {
                splitCtrl6F.InitCellList();
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
            privateObject = new PrivateObject(splitCtrl6F);
            privateObject.SetFieldOrProperty("vm", vm);
            try
            {
                splitCtrl6F.UpdateRatioRectsFromSettings();
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
                splitCtrl6F.UpdateRatioRectsFromSettings();
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
                splitCtrl6F.UpdateRatioRectsFromSettings();
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
        //    splitCtrl6F.VSplitterList = VSplitterList;
        //    // Assert
        //    Assert.That(splitCtrl6F.VSplitterList, Is.Not.Null);
        //}

        //[Test]
        //public void TestHSplitterList()
        //{
        //    // Act
        //    var HSplitterList = new List<GridSplitter>();
        //    splitCtrl6F.HSplitterList = HSplitterList;
        //    // Assert
        //    Assert.That(splitCtrl6F.HSplitterList, Is.Not.Null);
        //}

        //[Test]
        //public void TestInitSplitterList()
        //{
        //    try
        //    {
        //        splitCtrl6F.InitSplitterList();
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
            Assert.That(splitCtrl6F.DefaultSettings, Is.Not.Null);
        }

        [Test]
        public void TestFriendlyName()
        {
            // Act
            splitCtrl6F.FriendlyName = "FriendlyName";
            // Assert
            Assert.That(splitCtrl6F.FriendlyName, Is.EqualTo("FriendlyName"));
        }

        [TearDown]
        public void TearDown()
        {
            if (splitCtrl6F != null)
            {
                splitCtrl6F.Dispose();
                splitCtrl6F = null;
            }
            if (vm != null)
            {
                vm.Dispose();
                vm = null;
            }
        }
    }
}
