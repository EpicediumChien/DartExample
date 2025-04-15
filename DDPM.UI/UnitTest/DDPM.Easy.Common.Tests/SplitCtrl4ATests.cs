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
    public class SplitCtrl4ATests
    {
        private SplitCtrl4A? splitCtrl4A;
        private SplitCtrlVM vm;
        private PrivateObject? privateObject;

        [SetUp]
        public void Setup()
        {
            var settings = new ObservableCollection<GridLength>() { new GridLength(), new GridLength(), new GridLength(), new GridLength(), new GridLength(), new GridLength(), new GridLength(), new GridLength() };
            vm = new SplitCtrlVM() { Settings = settings };
            splitCtrl4A = new SplitCtrl4A();
            privateObject = new PrivateObject(splitCtrl4A);
        }

        [Test]
        public void TestConstructor_SplitCtrl4A()
        {
            // Assert
            Assert.That(splitCtrl4A, Is.Not.Null);
            Assert.That(splitCtrl4A.DataContext, Is.EqualTo(privateObject.GetFieldOrProperty("vm")));
        }

        [Test]
        public void TestNew()
        {
            // Act
            var result = splitCtrl4A.New();
            // Assert
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public void TestCellList()
        {
            // Act
            var CellList = new List<CellObj>();
            splitCtrl4A.CellList = CellList;
            // Assert
            Assert.That(splitCtrl4A.CellList, Is.Not.Null);

            vm.IsVertical = true;
            splitCtrl4A.CellList = CellList;
            // Assert
            Assert.That(splitCtrl4A.CellList, Is.Not.Null);
        }

        [Test]
        public void TestInitCellList()
        {
            try
            {
                splitCtrl4A.InitCellList();
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
            privateObject = new PrivateObject(splitCtrl4A);
            privateObject.SetFieldOrProperty("vm", vm);
            try
            {
                splitCtrl4A.UpdateRatioRectsFromSettings();
                vm.Dispose();
                Assert.True(true);
            }
            catch (Exception ex)
            {
                vm.Dispose();
                Assert.Fail("not invoked");
            }

            settings = new ObservableCollection<GridLength>() { new GridLength(), new GridLength(), new GridLength() ,new GridLength(),new GridLength()};
            vm = new SplitCtrlVM() { Settings = settings };
            vm.IsVertical = true;
            privateObject.SetFieldOrProperty("vm", vm);
            try
            {
                splitCtrl4A.UpdateRatioRectsFromSettings();
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
                splitCtrl4A.UpdateRatioRectsFromSettings();
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
        public void TestUpdateToCellListFromSettings()
        {
            //var cellListH = new List<CellObj>() { new CellObj("name1"),new CellObj("name2"),new CellObj("name3"),new CellObj("name4"),new CellObj("name5") };
            //var cellListV = new List<CellObj>() { new CellObj("name1"), new CellObj("name2"), new CellObj("name3"), new CellObj("name4"), new CellObj("name5") };
            //privateObject = new PrivateObject(splitCtrl4A);
            //privateObject.SetFieldOrProperty("cellListH", cellListH);
            //privateObject.SetFieldOrProperty("cellListV", cellListV);
            try
            {
                splitCtrl4A.UpdateToCellListFromSettings();
                Assert.True(true);
            }
            catch (Exception ex)
            {
                Assert.Fail("not invoked");
            }
        }

        //[Test]
        //public void TestVSplitterList()
        //{
        //    // Act
        //    var VSplitterList = new List<GridSplitter>();
        //    splitCtrl4A.VSplitterList = VSplitterList;
        //    // Assert
        //    Assert.That(splitCtrl4A.VSplitterList, Is.Not.Null);
        //}

        //[Test]
        //public void TestHSplitterList()
        //{
        //    // Act
        //    var HSplitterList = new List<GridSplitter>();
        //    splitCtrl4A.HSplitterList = HSplitterList;
        //    // Assert
        //    Assert.That(splitCtrl4A.HSplitterList, Is.Not.Null);
        //}

        //[Test]
        //public void TestInitSplitterList()
        //{
        //    try
        //    {
        //        splitCtrl4A.InitSplitterList();
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
            Assert.That(splitCtrl4A.DefaultSettings, Is.Not.Null);
        }

        [Test]
        public void TestFriendlyName()
        {
            // Act
            splitCtrl4A.FriendlyName = "FriendlyName";
            // Assert
            Assert.That(splitCtrl4A.FriendlyName, Is.EqualTo("FriendlyName"));
        }

        [TearDown]
        public void TearDown()
        {
            if (splitCtrl4A != null)
            {
                splitCtrl4A.Dispose();
                splitCtrl4A = null;
            }
            if (vm != null)
            {
                vm.Dispose();
                vm = null;
            }
        }
    }
}
