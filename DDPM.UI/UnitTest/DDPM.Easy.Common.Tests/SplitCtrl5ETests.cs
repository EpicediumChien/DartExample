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
    public class SplitCtrl5ETests
    {
        private SplitCtrl5E? splitCtrl5E;
        private SplitCtrlVM vm;
        private PrivateObject? privateObject;

        [SetUp]
        public void Setup()
        {
            var settings = new ObservableCollection<GridLength>() { new GridLength(), new GridLength(), new GridLength(), new GridLength(), new GridLength(), new GridLength(), new GridLength(), new GridLength() };
            vm = new SplitCtrlVM() { Settings = settings };
            splitCtrl5E = new SplitCtrl5E();
            privateObject = new PrivateObject(splitCtrl5E);
        }

        [Test]
        public void TestConstructor_SplitCtrl5E()
        {
            // Assert
            Assert.That(splitCtrl5E, Is.Not.Null);
            Assert.That(splitCtrl5E.DataContext, Is.EqualTo(privateObject.GetFieldOrProperty("vm")));
        }

        [Test]
        public void TestNew()
        {
            // Act
            var result = splitCtrl5E.New();
            // Assert
            Assert.That(result, Is.InstanceOf<SplitCtrl5E>());
        }

        [Test]
        public void TestCellList()
        {
            // Act
            var CellList = new List<CellObj>();
            splitCtrl5E.CellList = CellList;
            // Assert
            Assert.That(splitCtrl5E.CellList, Is.Not.Null);

            vm.IsVertical = true;
            splitCtrl5E.CellList = CellList;
            // Assert
            Assert.That(splitCtrl5E.CellList, Is.Not.Null);
        }

        [Test]
        public void TestInitCellList()
        {
            try
            {
                splitCtrl5E.InitCellList();
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
            privateObject = new PrivateObject(splitCtrl5E);
            privateObject.SetFieldOrProperty("vm", vm);
            try
            {
                splitCtrl5E.UpdateRatioRectsFromSettings();
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
                splitCtrl5E.UpdateRatioRectsFromSettings();
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
                splitCtrl5E.UpdateRatioRectsFromSettings();
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
        //    splitCtrl5E.VSplitterList = VSplitterList;
        //    // Assert
        //    Assert.That(splitCtrl5E.VSplitterList, Is.Not.Null);
        //}

        //[Test]
        //public void TestHSplitterList()
        //{
        //    // Act
        //    var HSplitterList = new List<GridSplitter>();
        //    splitCtrl5E.HSplitterList = HSplitterList;
        //    // Assert
        //    Assert.That(splitCtrl5E.HSplitterList, Is.Not.Null);
        //}

        //[Test]
        //public void TestInitSplitterList()
        //{
        //    try
        //    {
        //        splitCtrl5E.InitSplitterList();
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
            Assert.That(splitCtrl5E.DefaultSettings, Is.Not.Null);
        }

        [Test]
        public void TestFriendlyName()
        {
            // Act
            splitCtrl5E.FriendlyName = "FriendlyName";
            // Assert
            Assert.That(splitCtrl5E.FriendlyName, Is.EqualTo("FriendlyName"));
        }

        [TearDown]
        public void TearDown()
        {
            if (splitCtrl5E != null)
            {
                splitCtrl5E.Dispose();
                splitCtrl5E = null;
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
            if (splitCtrl5E != null)
            {
                splitCtrl5E.Dispose();
                splitCtrl5E = null;
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
