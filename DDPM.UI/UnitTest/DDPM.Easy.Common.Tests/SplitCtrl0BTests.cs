using DDPM.UI.Common.EAEM;
using Moq;
using NGA.UnitTest.PrivateObject;
using System;
using System.Collections;
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
    public class SplitCtrl0BTests
    {
        private SplitCtrl0B? splitCtrl0B;
        private SplitCtrlVM vm;
        private PrivateObject? privateObject;

        [SetUp]
        public void Setup()
        {
            var settings = new ObservableCollection<GridLength>() { new GridLength(), new GridLength(), new GridLength(), new GridLength(), new GridLength(), new GridLength(), new GridLength(), new GridLength() };
            vm = new SplitCtrlVM() { Settings = settings };
            splitCtrl0B = new SplitCtrl0B();
            privateObject = new PrivateObject(splitCtrl0B);         
            privateObject.SetFieldOrProperty("vm", vm);
        }

        [Test]
        public void TestConstructor_SplitCtrl0B()
        {
            // Assert
            Assert.That(splitCtrl0B, Is.Not.Null);
        }

        [Test]
        public void TestNew()
        {
            // Act
            var result = splitCtrl0B.New();
            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.InstanceOf<SplitCtrl0B>());
        }

        [Test]
        public void TestCellList()
        {
            // Act
            var CellList = new List<CellObj>();
            splitCtrl0B.CellList=CellList;
            // Assert
            Assert.That(splitCtrl0B.CellList, Is.Not.Null);

            // Act
            vm.IsVertical = true;
            CellList = new List<CellObj>();
            splitCtrl0B.CellList = CellList;
            // Assert
            Assert.That(splitCtrl0B.CellList, Is.Not.Null);
        }

        [Test]
        public void TestInitCellList()
        {
            try
            {
                splitCtrl0B.InitCellList();
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
            try
            {
                splitCtrl0B.UpdateRatioRectsFromSettings();
                Assert.True(true);
            }
            catch (Exception ex)
            {
                Assert.Fail("not invoked");
            }
        }

        [Test]
        public void TestApplySettingsToCellList()
        {
            var rcView=new Rect(1,1,2,2);
            var reuslt=splitCtrl0B.ApplySettingsToCellList(rcView);
            Assert.True(reuslt);

            var settings = new ObservableCollection<GridLength>() { };
            vm = new SplitCtrlVM() { Settings = settings };
            privateObject.SetFieldOrProperty("vm", vm);
            reuslt = splitCtrl0B.ApplySettingsToCellList(rcView);
            vm.Dispose();
            Assert.False(reuslt);
        }

        [Test]
        public void TestConvertSettingsToRatioRects()
        {
            var rcView = new Rect(1, 1, 2, 2);
            var reuslt = splitCtrl0B.ConvertSettingsToRatioRects(rcView);
            Assert.NotNull(reuslt);

            var settings = new ObservableCollection<GridLength>() { };
            vm = new SplitCtrlVM() { Settings = settings };
            privateObject.SetFieldOrProperty("vm", vm);
            reuslt = splitCtrl0B.ConvertSettingsToRatioRects(rcView);
            vm.Dispose();
            Assert.Null(reuslt);
        }

        [Test]
        public void TestUpdateToCellListFromSettings()
        {
            try
            {
                splitCtrl0B.UpdateToCellListFromSettings();
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
            splitCtrl0B.CellBorders = cellBorders;
            Assert.That(splitCtrl0B.CellBorders,Is.Not.Null);

            vm.IsVertical = true;
            splitCtrl0B.CellBorders = cellBorders;
            Assert.That(splitCtrl0B.CellBorders, Is.Not.Null);
        }

        [Test]
        public void TestVSplitterList()
        {
            var vSplitterList = new List<GridSplitter>();
            splitCtrl0B.VSplitterList = vSplitterList;
            Assert.That(splitCtrl0B.VSplitterList, Is.EqualTo(vSplitterList));
        }

        [Test]
        public void TestHSplitterList()
        {
            var hSplitterList = new List<GridSplitter>();
            splitCtrl0B.HSplitterList = hSplitterList;
            Assert.That(splitCtrl0B.HSplitterList, Is.EqualTo(hSplitterList));
        }

        [Test]
        public void TestInitSplitterList()
        {
            try
            {
                splitCtrl0B.InitSplitterList();
                Assert.True(true);
                Assert.That(splitCtrl0B.VSplitterList.Count, Is.EqualTo(0));
                Assert.That(splitCtrl0B.HSplitterList.Count, Is.EqualTo(0));
            }
            catch (Exception ex)
            {
                Assert.Fail("not invoked");
            }
        }

        [Test]
        public void TestDefaultSettings()
        {
            Assert.That(splitCtrl0B.DefaultSettings.Count, Is.EqualTo(2));
        }

        [Test]
        public void TestFriendlyName()
        {
            splitCtrl0B.FriendlyName = "FriendlyName";
            Assert.That(splitCtrl0B.FriendlyName, Is.EqualTo("FriendlyName"));
        }

        [Test]
        public void TestHoveringCell()
        {
            splitCtrl0B.HoveringCell = "HoveringCell";
            Assert.That(splitCtrl0B.HoveringCell, Is.EqualTo("HoveringCell"));
        }


        [TearDown]
        public void TearDown()
        {
            if (splitCtrl0B != null)
            {
                splitCtrl0B.Dispose();
                splitCtrl0B = null;
            }
            if (vm != null)
            {
                vm.Dispose();
                vm = null;
            }
        }

    }
}
