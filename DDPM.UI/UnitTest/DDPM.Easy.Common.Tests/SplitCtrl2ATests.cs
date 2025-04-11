using DDPM.UI.Common.EAEM;
using DDPM.UI.Common.UserControls;
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
    public class SplitCtrl2ATests
    {
        private SplitCtrl2A? splitCtrl2A;
        private SplitCtrlVM vm;
        private PrivateObject? privateObject;

        [SetUp]
        public void Setup()
        {
            var settings = new ObservableCollection<GridLength>() { new GridLength(), new GridLength(), new GridLength(), new GridLength(), new GridLength(), new GridLength(), new GridLength(), new GridLength() };
            vm = new SplitCtrlVM() { Settings = settings };
            splitCtrl2A = new SplitCtrl2A();
            privateObject = new PrivateObject(splitCtrl2A);
        }

        [Test]
        public void TestConstructor_SplitCtrl2A()
        {
            // Assert
            Assert.That(splitCtrl2A, Is.Not.Null);
            Assert.That(splitCtrl2A.DataContext, Is.EqualTo(privateObject.GetFieldOrProperty("vm")));
        }

        [Test]
        public void TestCtrlClass()
        {
            // Act
            var result = splitCtrl2A.CtrlClass;
            // Assert
            Assert.That(result, Is.EqualTo("SplitCtrl2A"));
        }

        [Test]
        public void TestCellCount()
        {
            // Act
            var result = splitCtrl2A.CellCount;
            // Assert
            Assert.That(result, Is.EqualTo(2));
        }

        [Test]
        public void TestSplitKey()
        {
            // Act
            var result = splitCtrl2A.SplitKey;
            // Assert
            Assert.That(result, Is.EqualTo('A'));
        }

        [Test]
        public void TestUC()
        {
            // Act
            var result = splitCtrl2A.UC;
            // Assert
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public void TestEAID()
        {
            // Act
            splitCtrl2A.EAID = 2;
            var result = splitCtrl2A.EAID;
            // Assert
            Assert.That(result, Is.EqualTo(2));
        }

        [Test]
        public void TestVM()
        {
            // Act
            var result = splitCtrl2A.VM;
            // Assert
            Assert.That(result, Is.InstanceOf<SplitCtrlVM>());
        }

        [Test]
        public void TestNew()
        {
            // Act
            var result = splitCtrl2A.New();
            // Assert
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public void TestCellList()
        {
            // Act
            var CellList = new List<CellObj>();
            splitCtrl2A.CellList = CellList;
            // Assert
            Assert.That(splitCtrl2A.CellList, Is.Not.Null);

            vm.IsVertical = true;
            splitCtrl2A.CellList = CellList;
            // Assert
            Assert.That(splitCtrl2A.CellList, Is.Not.Null);
        }

        [Test]
        public void TestInitCellList()
        {
            try
            {
                splitCtrl2A.InitCellList();
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
            privateObject = new PrivateObject(splitCtrl2A);
            privateObject.SetFieldOrProperty("vm", vm);
            try
            {
                splitCtrl2A.UpdateRatioRectsFromSettings();
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
                splitCtrl2A.UpdateRatioRectsFromSettings();
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
                splitCtrl2A.UpdateRatioRectsFromSettings();
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
        public void TestUpdateSettingsToCells()
        {
            try
            {
                splitCtrl2A.UpdateSettingsToCells();
                Assert.True(true);
            }
            catch (Exception ex)
            {
                Assert.Fail("not invoked");
            }

            var settings = new ObservableCollection<GridLength>() {};
            vm = new SplitCtrlVM() { Settings = settings };
            privateObject = new PrivateObject(splitCtrl2A);
            privateObject.SetFieldOrProperty("vm", vm);
            try
            {
                splitCtrl2A.UpdateSettingsToCells();
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
            splitCtrl2A.CellBorders = cellBorders;
            Assert.That(splitCtrl2A.CellBorders, Is.Not.Null);

            vm.IsVertical = true;
            splitCtrl2A.CellBorders = cellBorders;
            Assert.That(splitCtrl2A.CellBorders, Is.Not.Null);
        }

        [Test]
        public void TestVSplitterList()
        {
            // Act
            var VSplitterList = new List<GridSplitter>();
            splitCtrl2A.VSplitterList = VSplitterList;
            // Assert
            Assert.That(splitCtrl2A.VSplitterList, Is.Not.Null);
        }

        [Test]
        public void TestHSplitterList()
        {
            // Act
            var HSplitterList = new List<GridSplitter>();
            splitCtrl2A.HSplitterList = HSplitterList;
            // Assert
            Assert.That(splitCtrl2A.HSplitterList, Is.Not.Null);
        }

        [Test]
        public void TestInitSplitterList()
        {
            try
            {
                splitCtrl2A.InitSplitterList();
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
            Assert.That(splitCtrl2A.DefaultSettings, Is.Not.Null);
        }

        [Test]
        public void TestFriendlyName()
        {
            // Act
            splitCtrl2A.FriendlyName = "FriendlyName";
            // Assert
            Assert.That(splitCtrl2A.FriendlyName, Is.EqualTo("FriendlyName"));
        }

        [TearDown]
        public void TearDown()
        {
            if (splitCtrl2A != null)
            {
                splitCtrl2A.Dispose();
                splitCtrl2A = null;
            }
            if (vm != null)
            {
                vm.Dispose();
                vm = null;
            }
        }

    }
}
