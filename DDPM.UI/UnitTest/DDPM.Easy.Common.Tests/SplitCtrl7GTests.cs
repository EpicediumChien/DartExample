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
    public class SplitCtrl7GTests
    {
        private SplitCtrl7G? splitCtrl7G;
        private SplitCtrlVM vm;
        private PrivateObject? privateObject;

        [SetUp]
        public void Setup()
        {
            var settings = new ObservableCollection<GridLength>() { new GridLength(), new GridLength(), new GridLength(), new GridLength(), new GridLength(), new GridLength(), new GridLength(), new GridLength() };
            vm = new SplitCtrlVM() { Settings = settings };
            splitCtrl7G = new SplitCtrl7G();
            privateObject = new PrivateObject(splitCtrl7G);
        }

        [Test]
        public void TestConstructor_SplitCtrl7G()
        {
            // Assert
            Assert.That(splitCtrl7G, Is.Not.Null);
            Assert.That(splitCtrl7G.DataContext, Is.EqualTo(privateObject.GetFieldOrProperty("vm")));
        }

        [Test]
        public void TestNew()
        {
            // Act
            var result = splitCtrl7G.New();
            // Assert
            Assert.That(result, Is.InstanceOf<SplitCtrl7G>());
        }

        [Test]
        public void TestCellList()
        {
            // Act
            var CellList = new List<CellObj>();
            splitCtrl7G.CellList = CellList;
            // Assert
            Assert.That(splitCtrl7G.CellList, Is.Not.Null);

            vm.IsVertical = true;
            splitCtrl7G.CellList = CellList;
            // Assert
            Assert.That(splitCtrl7G.CellList, Is.Not.Null);
        }

        [Test]
        public void TestInitCellList()
        {
            try
            {
                splitCtrl7G.InitCellList();
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
            privateObject = new PrivateObject(splitCtrl7G);
            privateObject.SetFieldOrProperty("vm", vm);
            try
            {
                splitCtrl7G.UpdateRatioRectsFromSettings();
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
                splitCtrl7G.UpdateRatioRectsFromSettings();
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
                splitCtrl7G.UpdateRatioRectsFromSettings();
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
            splitCtrl7G.VSplitterList = VSplitterList;
            // Assert
            Assert.That(splitCtrl7G.VSplitterList, Is.Not.Null);
        }

        [Test]
        public void TestHSplitterList()
        {
            // Act
            var HSplitterList = new List<GridSplitter>();
            splitCtrl7G.HSplitterList = HSplitterList;
            // Assert
            Assert.That(splitCtrl7G.HSplitterList, Is.Not.Null);
        }

        [Test]
        public void TestInitSplitterList()
        {
            try
            {
                splitCtrl7G.InitSplitterList();
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
            Assert.That(splitCtrl7G.DefaultSettings, Is.Not.Null);
        }

        [Test]
        public void TestFriendlyName()
        {
            // Act
            splitCtrl7G.FriendlyName = "FriendlyName";
            // Assert
            Assert.That(splitCtrl7G.FriendlyName, Is.EqualTo("FriendlyName"));
        }
    }
}
