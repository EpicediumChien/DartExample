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
    public class SplitCtrl5HTests
    {
        private SplitCtrl5H? splitCtrl5H;
        private SplitCtrlVM vm;
        private PrivateObject? privateObject;

        [SetUp]
        public void Setup()
        {
            var settings = new ObservableCollection<GridLength>() { new GridLength(), new GridLength(), new GridLength(), new GridLength(), new GridLength(), new GridLength(), new GridLength(), new GridLength() };
            vm = new SplitCtrlVM() { Settings = settings };
            splitCtrl5H = new SplitCtrl5H();
            privateObject = new PrivateObject(splitCtrl5H);
        }

        [Test]
        public void TestConstructor_SplitCtrl5H()
        {
            // Assert
            Assert.That(splitCtrl5H, Is.Not.Null);
            Assert.That(splitCtrl5H.DataContext, Is.EqualTo(privateObject.GetFieldOrProperty("vm")));
        }

        [Test]
        public void TestNew()
        {
            // Act
            var result = splitCtrl5H.New();
            // Assert
            Assert.That(result, Is.InstanceOf<SplitCtrl5H>());
        }

        [Test]
        public void TestCellList()
        {
            // Act
            var CellList = new List<CellObj>();
            splitCtrl5H.CellList = CellList;
            // Assert
            Assert.That(splitCtrl5H.CellList, Is.Not.Null);

            vm.IsVertical = true;
            splitCtrl5H.CellList = CellList;
            // Assert
            Assert.That(splitCtrl5H.CellList, Is.Not.Null);
        }

        [Test]
        public void TestInitCellList()
        {
            try
            {
                splitCtrl5H.InitCellList();
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
            privateObject = new PrivateObject(splitCtrl5H);
            privateObject.SetFieldOrProperty("vm", vm);
            try
            {
                splitCtrl5H.UpdateRatioRectsFromSettings();
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
                splitCtrl5H.UpdateRatioRectsFromSettings();
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
                splitCtrl5H.UpdateRatioRectsFromSettings();
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
            splitCtrl5H.VSplitterList = VSplitterList;
            // Assert
            Assert.That(splitCtrl5H.VSplitterList, Is.Not.Null);
        }

        [Test]
        public void TestHSplitterList()
        {
            // Act
            var HSplitterList = new List<GridSplitter>();
            splitCtrl5H.HSplitterList = HSplitterList;
            // Assert
            Assert.That(splitCtrl5H.HSplitterList, Is.Not.Null);
        }

        [Test]
        public void TestInitSplitterList()
        {
            try
            {
                splitCtrl5H.InitSplitterList();
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
            Assert.That(splitCtrl5H.DefaultSettings, Is.Not.Null);
        }

        [Test]
        public void TestFriendlyName()
        {
            // Act
            splitCtrl5H.FriendlyName = "FriendlyName";
            // Assert
            Assert.That(splitCtrl5H.FriendlyName, Is.EqualTo("FriendlyName"));
        }
    }
}
