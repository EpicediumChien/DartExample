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
    public class SplitCtrl7FTests
    {
        private SplitCtrl7F? splitCtrl7F;
        private SplitCtrlVM vm;
        private PrivateObject? privateObject;

        [SetUp]
        public void Setup()
        {
            var settings = new ObservableCollection<GridLength>() { new GridLength(), new GridLength(), new GridLength(), new GridLength(), new GridLength(), new GridLength(), new GridLength(), new GridLength() };
            vm = new SplitCtrlVM() { Settings = settings };
            splitCtrl7F = new SplitCtrl7F();
            privateObject = new PrivateObject(splitCtrl7F);
        }

        [Test]
        public void TestConstructor_SplitCtrl7F()
        {
            // Assert
            Assert.That(splitCtrl7F, Is.Not.Null);
            Assert.That(splitCtrl7F.DataContext, Is.EqualTo(privateObject.GetFieldOrProperty("vm")));
        }

        [Test]
        public void TestNew()
        {
            // Act
            var result = splitCtrl7F.New();
            // Assert
            Assert.That(result, Is.InstanceOf<SplitCtrl7F>());
        }

        [Test]
        public void TestCellList()
        {
            // Act
            var CellList = new List<CellObj>();
            splitCtrl7F.CellList = CellList;
            // Assert
            Assert.That(splitCtrl7F.CellList, Is.Not.Null);

            vm.IsVertical = true;
            splitCtrl7F.CellList = CellList;
            // Assert
            Assert.That(splitCtrl7F.CellList, Is.Not.Null);
        }

        [Test]
        public void TestInitCellList()
        {
            try
            {
                splitCtrl7F.InitCellList();
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
            privateObject = new PrivateObject(splitCtrl7F);
            privateObject.SetFieldOrProperty("vm", vm);
            try
            {
                splitCtrl7F.UpdateRatioRectsFromSettings();
                Assert.True(true);
            }
            catch (Exception ex)
            {
                Assert.Fail("not invoked");
            }

            settings = new ObservableCollection<GridLength>() { new GridLength(), new GridLength(), new GridLength(), new GridLength(), new GridLength() };
            vm = new SplitCtrlVM() { Settings = settings };
            vm.IsVertical = true;
            privateObject.SetFieldOrProperty("vm", vm);
            try
            {
                splitCtrl7F.UpdateRatioRectsFromSettings();
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
                splitCtrl7F.UpdateRatioRectsFromSettings();
                Assert.True(true);
            }
            catch (Exception ex)
            {
                Assert.Fail("not invoked");
            }
        }

        [Test]
        public void TestVSplitterList()
        {
            // Act
            var VSplitterList = new List<GridSplitter>();
            splitCtrl7F.VSplitterList = VSplitterList;
            // Assert
            Assert.That(splitCtrl7F.VSplitterList, Is.Not.Null);
        }

        [Test]
        public void TestHSplitterList()
        {
            // Act
            var HSplitterList = new List<GridSplitter>();
            splitCtrl7F.HSplitterList = HSplitterList;
            // Assert
            Assert.That(splitCtrl7F.HSplitterList, Is.Not.Null);
        }

        [Test]
        public void TestInitSplitterList()
        {
            try
            {
                splitCtrl7F.InitSplitterList();
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
            Assert.That(splitCtrl7F.DefaultSettings, Is.Not.Null);
        }

        [Test]
        public void TestFriendlyName()
        {
            // Act
            splitCtrl7F.FriendlyName = "FriendlyName";
            // Assert
            Assert.That(splitCtrl7F.FriendlyName, Is.EqualTo("FriendlyName"));
        }

        [TearDown]
        public void TearDown()
        {
            if (splitCtrl7F != null)
            {
                splitCtrl7F.Dispose();
                splitCtrl7F = null;
            }
            if (vm != null)
            {
                vm.Dispose();
                vm = null;
            }
        }
    }
}
