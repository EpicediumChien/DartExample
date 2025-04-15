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
    public class SplitCtrl5FTests
    {
        private SplitCtrl5F? splitCtrl5F;
        private SplitCtrlVM vm;
        private PrivateObject? privateObject;

        [SetUp]
        public void Setup()
        {
            var settings = new ObservableCollection<GridLength>() { new GridLength(), new GridLength(), new GridLength(), new GridLength(), new GridLength(), new GridLength(), new GridLength(), new GridLength() };
            vm = new SplitCtrlVM() { Settings = settings };
            splitCtrl5F = new SplitCtrl5F();
            privateObject = new PrivateObject(splitCtrl5F);
        }

        [Test]
        public void TestConstructor_SplitCtrl5F()
        {
            // Assert
            Assert.That(splitCtrl5F, Is.Not.Null);
            Assert.That(splitCtrl5F.DataContext, Is.EqualTo(privateObject.GetFieldOrProperty("vm")));
        }

        [Test]
        public void TestNew()
        {
            // Act
            var result = splitCtrl5F.New();
            // Assert
            Assert.That(result, Is.InstanceOf<SplitCtrl5F>());
        }

        [Test]
        public void TestCellList()
        {
            // Act
            var CellList = new List<CellObj>();
            splitCtrl5F.CellList = CellList;
            // Assert
            Assert.That(splitCtrl5F.CellList, Is.Not.Null);

            vm.IsVertical = true;
            splitCtrl5F.CellList = CellList;
            // Assert
            Assert.That(splitCtrl5F.CellList, Is.Not.Null);
        }

        [Test]
        public void TestInitCellList()
        {
            try
            {
                splitCtrl5F.InitCellList();
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
            privateObject = new PrivateObject(splitCtrl5F);
            privateObject.SetFieldOrProperty("vm", vm);
            try
            {
                splitCtrl5F.UpdateRatioRectsFromSettings();
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
                splitCtrl5F.UpdateRatioRectsFromSettings();
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
                splitCtrl5F.UpdateRatioRectsFromSettings();
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
        public void TestVSplitterList()
        {
            // Act
            var VSplitterList = new List<GridSplitter>();
            splitCtrl5F.VSplitterList = VSplitterList;
            // Assert
            Assert.That(splitCtrl5F.VSplitterList, Is.Not.Null);
        }

        [Test]
        public void TestHSplitterList()
        {
            // Act
            var HSplitterList = new List<GridSplitter>();
            splitCtrl5F.HSplitterList = HSplitterList;
            // Assert
            Assert.That(splitCtrl5F.HSplitterList, Is.Not.Null);
        }

        [Test]
        public void TestInitSplitterList()
        {
            try
            {
                splitCtrl5F.InitSplitterList();
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
            Assert.That(splitCtrl5F.DefaultSettings, Is.Not.Null);
        }

        [Test]
        public void TestFriendlyName()
        {
            // Act
            splitCtrl5F.FriendlyName = "FriendlyName";
            // Assert
            Assert.That(splitCtrl5F.FriendlyName, Is.EqualTo("FriendlyName"));
        }

        [TearDown]
        public void TearDown()
        {
            if (splitCtrl5F != null)
            {
                splitCtrl5F.Dispose();
                splitCtrl5F = null;
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
            if (splitCtrl5F != null)
            {
                splitCtrl5F.Dispose();
                splitCtrl5F = null;
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
