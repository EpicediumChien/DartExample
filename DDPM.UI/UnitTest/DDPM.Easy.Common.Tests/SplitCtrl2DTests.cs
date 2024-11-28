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
    public class SplitCtrl2DTests
    {
        private SplitCtrl2D? splitCtrl2D;
        private SplitCtrlVM vm;
        private PrivateObject? privateObject;

        [SetUp]
        public void Setup()
        {
            var settings = new ObservableCollection<GridLength>() { new GridLength(), new GridLength(), new GridLength(), new GridLength(), new GridLength(), new GridLength(), new GridLength(), new GridLength() };
            vm = new SplitCtrlVM() { Settings = settings };
            splitCtrl2D = new SplitCtrl2D();
            privateObject = new PrivateObject(splitCtrl2D);
        }

        [Test]
        public void TestConstructor_SplitCtrl2D()
        {
            // Assert
            Assert.That(splitCtrl2D, Is.Not.Null);
            Assert.That(splitCtrl2D.DataContext, Is.EqualTo(privateObject.GetFieldOrProperty("vm")));
        }

        [Test]
        public void TestNew()
        {
            // Act
            var result = splitCtrl2D.New();
            // Assert
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public void TestCellList()
        {
            // Act
            var CellList = new List<CellObj>();
            splitCtrl2D.CellList = CellList;
            // Assert
            Assert.That(splitCtrl2D.CellList, Is.Not.Null);

            vm.IsVertical = true;
            splitCtrl2D.CellList = CellList;
            // Assert
            Assert.That(splitCtrl2D.CellList, Is.Not.Null);
        }

        [Test]
        public void TestInitCellList()
        {
            try
            {
                splitCtrl2D.InitCellList();
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
            privateObject = new PrivateObject(splitCtrl2D);
            privateObject.SetFieldOrProperty("vm", vm);
            try
            {
                splitCtrl2D.UpdateRatioRectsFromSettings();
                Assert.True(true);
            }
            catch (Exception ex)
            {
                Assert.Fail("not invoked");
            }

            settings = new ObservableCollection<GridLength>() { new GridLength(), new GridLength(), new GridLength() };
            vm = new SplitCtrlVM() { Settings = settings };
            vm.IsVertical = true;
            privateObject.SetFieldOrProperty("vm", vm);
            try
            {
                splitCtrl2D.UpdateRatioRectsFromSettings();
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
                splitCtrl2D.UpdateRatioRectsFromSettings();
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
            splitCtrl2D.VSplitterList = VSplitterList;
            // Assert
            Assert.That(splitCtrl2D.VSplitterList, Is.Not.Null);
        }

        [Test]
        public void TestHSplitterList()
        {
            // Act
            var HSplitterList = new List<GridSplitter>();
            splitCtrl2D.HSplitterList = HSplitterList;
            // Assert
            Assert.That(splitCtrl2D.HSplitterList, Is.Not.Null);
        }

        [Test]
        public void TestInitSplitterList()
        {
            try
            {
                splitCtrl2D.InitSplitterList();
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
            Assert.That(splitCtrl2D.DefaultSettings, Is.Not.Null);
        }

        [Test]
        public void TestFriendlyName()
        {
            // Act
            splitCtrl2D.FriendlyName = "FriendlyName";
            // Assert
            Assert.That(splitCtrl2D.FriendlyName, Is.EqualTo("FriendlyName"));
        }
    }
}
