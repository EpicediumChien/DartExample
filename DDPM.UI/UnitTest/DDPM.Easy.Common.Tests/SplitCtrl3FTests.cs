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
    public class SplitCtrl3FTests
    {
        private SplitCtrl3F? splitCtrl3F;
        private SplitCtrlVM vm;
        private PrivateObject? privateObject;

        [SetUp]
        public void Setup()
        {
            var settings = new ObservableCollection<GridLength>() { new GridLength(), new GridLength(), new GridLength(), new GridLength(), new GridLength(), new GridLength(), new GridLength(), new GridLength() };
            vm = new SplitCtrlVM() { Settings = settings };
            splitCtrl3F = new SplitCtrl3F();
            privateObject = new PrivateObject(splitCtrl3F);
        }

        [Test]
        public void TestConstructor_SplitCtrl3F()
        {
            // Assert
            Assert.That(splitCtrl3F, Is.Not.Null);
            Assert.That(splitCtrl3F.DataContext, Is.EqualTo(privateObject.GetFieldOrProperty("vm")));
        }

        [Test]
        public void TestNew()
        {
            // Act
            var result = splitCtrl3F.New();
            // Assert
            Assert.That(result, Is.InstanceOf<SplitCtrl3F>());
        }

        [Test]
        public void TestCellList()
        {
            // Act
            var CellList = new List<CellObj>();
            splitCtrl3F.CellList = CellList;
            // Assert
            Assert.That(splitCtrl3F.CellList, Is.Not.Null);

            vm.IsVertical = true;
            splitCtrl3F.CellList = CellList;
            // Assert
            Assert.That(splitCtrl3F.CellList, Is.Not.Null);
        }

        [Test]
        public void TestInitCellList()
        {
            try
            {
                splitCtrl3F.InitCellList();
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
            privateObject = new PrivateObject(splitCtrl3F);
            privateObject.SetFieldOrProperty("vm", vm);
            try
            {
                splitCtrl3F.UpdateRatioRectsFromSettings();
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
                splitCtrl3F.UpdateRatioRectsFromSettings();
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
                splitCtrl3F.UpdateRatioRectsFromSettings();
                Assert.True(true);
            }
            catch (Exception ex)
            {
                Assert.Fail("not invoked");
            }
        }

        [Test]
        public void TestCellBorders()
        {
            var cellBorders = new List<CellBorder>();
            splitCtrl3F.CellBorders = cellBorders;
            Assert.That(splitCtrl3F.CellBorders, Is.Not.Null);

            vm.IsVertical= true;
            splitCtrl3F.CellBorders = cellBorders;
            Assert.That(splitCtrl3F.CellBorders, Is.Not.Null);
        }

        [Test]
        public void TestVSplitterList()
        {
            // Act
            var VSplitterList = new List<GridSplitter>();
            splitCtrl3F.VSplitterList = VSplitterList;
            // Assert
            Assert.That(splitCtrl3F.VSplitterList, Is.Not.Null);
        }

        [Test]
        public void TestHSplitterList()
        {
            // Act
            var HSplitterList = new List<GridSplitter>();
            splitCtrl3F.HSplitterList = HSplitterList;
            // Assert
            Assert.That(splitCtrl3F.HSplitterList, Is.Not.Null);
        }

        [Test]
        public void TestInitSplitterList()
        {
            try
            {
                splitCtrl3F.InitSplitterList();
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
            Assert.That(splitCtrl3F.DefaultSettings, Is.Not.Null);
        }

        [Test]
        public void TestFriendlyName()
        {
            // Act
            splitCtrl3F.FriendlyName = "FriendlyName";
            // Assert
            Assert.That(splitCtrl3F.FriendlyName, Is.EqualTo("FriendlyName"));
        }
    }
}
