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
    public class SplitCtrl7KTests
    {
        private SplitCtrl7K? splitCtrl7K;
        private SplitCtrlVM vm;
        private PrivateObject? privateObject;

        [SetUp]
        public void Setup()
        {
            var settings = new ObservableCollection<GridLength>() { new GridLength(), new GridLength(), new GridLength(), new GridLength(), new GridLength(), new GridLength(), new GridLength(), new GridLength() };
            vm = new SplitCtrlVM() { Settings = settings };
            splitCtrl7K = new SplitCtrl7K();
            privateObject = new PrivateObject(splitCtrl7K);
        }

        [Test]
        public void TestConstructor_SplitCtrl7K()
        {
            // Assert
            Assert.That(splitCtrl7K, Is.Not.Null);
            Assert.That(splitCtrl7K.DataContext, Is.EqualTo(privateObject.GetFieldOrProperty("vm")));
        }

        [Test]
        public void TestNew()
        {
            // Act
            var result = splitCtrl7K.New();
            // Assert
            Assert.That(result, Is.InstanceOf<SplitCtrl7K>());
        }

        [Test]
        public void TestCellList()
        {
            // Act
            var CellList = new List<CellObj>();
            splitCtrl7K.CellList = CellList;
            // Assert
            Assert.That(splitCtrl7K.CellList, Is.Not.Null);

            vm.IsVertical = true;
            splitCtrl7K.CellList = CellList;
            // Assert
            Assert.That(splitCtrl7K.CellList, Is.Not.Null);
        }

        [Test]
        public void TestInitCellList()
        {
            try
            {
                splitCtrl7K.InitCellList();
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
            privateObject = new PrivateObject(splitCtrl7K);
            privateObject.SetFieldOrProperty("vm", vm);
            try
            {
                splitCtrl7K.UpdateRatioRectsFromSettings();
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
                splitCtrl7K.UpdateRatioRectsFromSettings();
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
                splitCtrl7K.UpdateRatioRectsFromSettings();
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
        //    splitCtrl7K.VSplitterList = VSplitterList;
        //    // Assert
        //    Assert.That(splitCtrl7K.VSplitterList, Is.Not.Null);
        //}

        //[Test]
        //public void TestHSplitterList()
        //{
        //    // Act
        //    var HSplitterList = new List<GridSplitter>();
        //    splitCtrl7K.HSplitterList = HSplitterList;
        //    // Assert
        //    Assert.That(splitCtrl7K.HSplitterList, Is.Not.Null);
        //}

        //[Test]
        //public void TestInitSplitterList()
        //{
        //    try
        //    {
        //        splitCtrl7K.InitSplitterList();
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
            Assert.That(splitCtrl7K.DefaultSettings, Is.Not.Null);
        }

        [Test]
        public void TestFriendlyName()
        {
            // Act
            splitCtrl7K.FriendlyName = "FriendlyName";
            // Assert
            Assert.That(splitCtrl7K.FriendlyName, Is.EqualTo("FriendlyName"));
        }

        [TearDown]
        public void TearDown()
        {
            if (splitCtrl7K != null)
            {
                splitCtrl7K.Dispose();
                splitCtrl7K = null;
            }
            if (vm != null)
            {
                vm.Dispose();
                vm = null;
            }
        }
    }
}
