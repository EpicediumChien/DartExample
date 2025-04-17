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
    public class SplitCtrl4FTests
    {
        private SplitCtrl4F? splitCtrl4F;
        private SplitCtrlVM vm;
        private PrivateObject? privateObject;

        [SetUp]
        public void Setup()
        {
            var settings = new ObservableCollection<GridLength>() { new GridLength(), new GridLength(), new GridLength(), new GridLength(), new GridLength(), new GridLength(), new GridLength(), new GridLength() };
            vm = new SplitCtrlVM() { Settings = settings };
            splitCtrl4F = new SplitCtrl4F();
            privateObject = new PrivateObject(splitCtrl4F);
        }

        [Test]
        public void TestConstructor_SplitCtrl4F()
        {
            // Assert
            Assert.That(splitCtrl4F, Is.Not.Null);
            Assert.That(splitCtrl4F.DataContext, Is.EqualTo(privateObject.GetFieldOrProperty("vm")));
        }

        [Test]
        public void TestNew()
        {
            // Act
            var result = splitCtrl4F.New();
            // Assert
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public void TestCellList()
        {
            // Act
            var CellList = new List<CellObj>();
            splitCtrl4F.CellList = CellList;
            // Assert
            Assert.That(splitCtrl4F.CellList, Is.Not.Null);

            vm.IsVertical = true;
            splitCtrl4F.CellList = CellList;
            // Assert
            Assert.That(splitCtrl4F.CellList, Is.Not.Null);
        }

        [Test]
        public void TestInitCellList()
        {
            try
            {
                splitCtrl4F.InitCellList();
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
            privateObject = new PrivateObject(splitCtrl4F);
            privateObject.SetFieldOrProperty("vm", vm);
            try
            {
                splitCtrl4F.UpdateRatioRectsFromSettings();
                vm.Dispose();
                Assert.True(true);
            }
            catch (Exception ex)
            {
                vm.Dispose();
                Assert.Fail("not invoked");
            }

            settings = new ObservableCollection<GridLength>() { new GridLength(), new GridLength(), new GridLength(), new GridLength(), new GridLength(),new GridLength() };
            vm = new SplitCtrlVM() { Settings = settings };
            //vm.IsVertical = true;
            //privateObject.SetFieldOrProperty("vm", vm);

            //splitCtrl4F.UpdateRatioRectsFromSettings();
            //try
            //{
            //    splitCtrl4F.UpdateRatioRectsFromSettings();
            //    Assert.True(true);
            //}
            //catch (Exception ex)
            //{
            //    vm.Dispose();
            //    Assert.Fail("not invoked");
            //}

            vm.IsVertical = false;
            privateObject.SetFieldOrProperty("vm", vm);
            try
            {
                splitCtrl4F.UpdateRatioRectsFromSettings();
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
        //    splitCtrl4F.VSplitterList = VSplitterList;
        //    // Assert
        //    Assert.That(splitCtrl4F.VSplitterList, Is.Not.Null);
        //}

        //[Test]
        //public void TestHSplitterList()
        //{
        //    // Act
        //    var HSplitterList = new List<GridSplitter>();
        //    splitCtrl4F.HSplitterList = HSplitterList;
        //    // Assert
        //    Assert.That(splitCtrl4F.HSplitterList, Is.Not.Null);
        //}

        //[Test]
        //public void TestInitSplitterList()
        //{
        //    try
        //    {
        //        splitCtrl4F.InitSplitterList();
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
            Assert.That(splitCtrl4F.DefaultSettings, Is.Not.Null);
        }

        [Test]
        public void TestFriendlyName()
        {
            // Act
            splitCtrl4F.FriendlyName = "FriendlyName";
            // Assert
            Assert.That(splitCtrl4F.FriendlyName, Is.EqualTo("FriendlyName"));
        }

        [TearDown]
        public void TearDown()
        {
            if (splitCtrl4F != null)
            {
                splitCtrl4F.Dispose();
                splitCtrl4F = null;
            }
            if (vm != null)
            {
                vm.Dispose();
                vm = null;
            }
        }
    }
}
