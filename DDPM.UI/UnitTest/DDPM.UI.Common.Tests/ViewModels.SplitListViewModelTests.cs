using DDPM.Easy.Common;
using DDPM.SA.Common;
using DDPM.SA.Common.Display;
using DDPM.UI.Common.EAEM;
using DDPM.UI.Common.Interfaces;
using DDPM.UI.Common.Models;
using DDPM.UI.Common.UserControls;
using DDPM.UI.Common.ViewModels;
using Dell.Client.Framework.Common;
using Dell.Client.Framework.UX.WPF;
using Moq;
using NGA.UnitTest.PrivateObject;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using VcpCore.Common;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Menu;

namespace DDPM.UI.Common.Tests
{
    [Apartment(ApartmentState.STA)]
    public class SplitListViewModelTests
    {
        private SplitListViewModel? splitListViewModel;
        private PrivateObject? privateObject;


        [SetUp]
        public void Setup()
        {
            if (System.Windows.Application.Current == null)
            {
                new System.Windows.Application();
            }
            var resourceDictionary = new ResourceDictionary();
            resourceDictionary.Source = new Uri("pack://application:,,,/DDPM.UI.Common;component/ModuleStyle.xaml");
            System.Windows.Application.Current.Resources.MergedDictionaries.Add(resourceDictionary);
            splitListViewModel = new SplitListViewModel();
            privateObject = new PrivateObject(splitListViewModel);
        }

        [Test]
        public void TestConstructor_SplitListViewModel()
        {
            // Assert
            Assert.That(splitListViewModel, Is.Not.Null);
        }

        [Test]
        public void TestAddSplitItemToList()
        {
            var spItem = new SplitItem() { Visibility = Visibility.Visible };
            try
            {
                splitListViewModel.AddSplitItemToList(spItem);
                Assert.True(true);
            }
            catch (Exception ex)
            {
                Assert.Fail("not invoked");
            }
        }

        [Test]
        public void TestClearList()
        {
            try
            {
                splitListViewModel.ClearList();
                Assert.True(true);
                Assert.That(splitListViewModel.SplitList, Is.Not.Null);
            }
            catch (Exception ex)
            {
                Assert.Fail("not invoked");
            }
        }

        [Test]
        public void TestFindSplitCtrl()
        {
            //_splitList == null
            privateObject.SetFieldOrProperty("_splitList", null);
            var result = splitListViewModel.FindSplitCtrl(1, 'A');
            Assert.That(result,Is.EqualTo(null));

            //_splitList.Count == 0
            var splitList = new ObservableCollection<SplitItem>() { };
            privateObject.SetFieldOrProperty("_splitList", splitList);
            result = splitListViewModel.FindSplitCtrl(1, 'A');
            Assert.That(result, Is.EqualTo(null));

            //_splitList.Count != 0
            splitList = new ObservableCollection<SplitItem>() { new SplitItem()};
            privateObject.SetFieldOrProperty("_splitList", splitList);
            result = splitListViewModel.FindSplitCtrl(1, 'A');
            Assert.That(result, Is.EqualTo(null));

            //spItem.ISplitCtrl != null
            var splitctrlMock = new Mock<ISplitCtrl>();
            splitctrlMock.Setup(x=>x.CellCount).Returns(1);
            splitctrlMock.Setup(x => x.SplitKey).Returns('A');
            var vm = new SplitItemViewModel() { SplitCtrl = splitctrlMock.Object };
            var splitItem = new SplitItem();
            var prio = new PrivateObject(splitItem);
            prio.SetFieldOrProperty("vm", vm);
            splitList = new ObservableCollection<SplitItem>() {new SplitItem(), splitItem };
            privateObject.SetFieldOrProperty("_splitList", splitList);
            result = splitListViewModel.FindSplitCtrl(1, 'A');
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public void TestFindSplitCtrlByFriendlyName()
        {
            //_splitList == null
            privateObject.SetFieldOrProperty("_splitList", null);
            var result = splitListViewModel.FindSplitCtrlByFriendlyName("friendlyName");
            Assert.That(result, Is.EqualTo(null));

            //_splitList.Count == 0
            var splitList = new ObservableCollection<SplitItem>() { };
            privateObject.SetFieldOrProperty("_splitList", splitList);
            result = splitListViewModel.FindSplitCtrlByFriendlyName("friendlyName");
            Assert.That(result, Is.EqualTo(null));

            //_splitList.Count != 0
            splitList = new ObservableCollection<SplitItem>() { new SplitItem() };
            privateObject.SetFieldOrProperty("_splitList", splitList);
            result = splitListViewModel.FindSplitCtrlByFriendlyName("friendlyName");
            Assert.That(result, Is.EqualTo(null));

            //spItem.ISplitCtrl != null
            var splitctrlMock = new Mock<ISplitCtrl>();
            splitctrlMock.Setup(x => x.FriendlyName).Returns("friendlyName");
            var vm = new SplitItemViewModel() { SplitCtrl = splitctrlMock.Object };
            var splitItem = new SplitItem();
            var prio = new PrivateObject(splitItem);
            prio.SetFieldOrProperty("vm", vm);
            splitList = new ObservableCollection<SplitItem>() { new SplitItem(), splitItem };
            privateObject.SetFieldOrProperty("_splitList", splitList);
            result = splitListViewModel.FindSplitCtrlByFriendlyName("friendlyName");
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public void TestFindSplitItemByCustomId()
        {
            //_splitList == null
            privateObject.SetFieldOrProperty("_splitList", null);
            var result = splitListViewModel.FindSplitItemByCustomId(2);
            Assert.That(result, Is.EqualTo(null));

            //_splitList.Count == 0
            var splitList = new ObservableCollection<SplitItem>() { };
            privateObject.SetFieldOrProperty("_splitList", splitList);
            result = splitListViewModel.FindSplitItemByCustomId(2);
            Assert.That(result, Is.EqualTo(null));

            //_splitList.Count != 0
            splitList = new ObservableCollection<SplitItem>() { new SplitItem() };
            privateObject.SetFieldOrProperty("_splitList", splitList);
            result = splitListViewModel.FindSplitItemByCustomId(2);
            Assert.That(result, Is.EqualTo(null));

            //spItem.CustomId == customId
            var vm = new SplitItemViewModel() { CustomId=2 };
            var splitItem = new SplitItem();
            var prio = new PrivateObject(splitItem);
            prio.SetFieldOrProperty("vm", vm);
            splitList = new ObservableCollection<SplitItem>() { new SplitItem(), splitItem };
            privateObject.SetFieldOrProperty("_splitList", splitList);
            result = splitListViewModel.FindSplitItemByCustomId(2);
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public void TestGetCustomFriendlyNameList()
        {
            //_splitList != null
            privateObject.SetFieldOrProperty("_splitList", new ObservableCollection<SplitItem>());
            var result = splitListViewModel.GetCustomFriendlyNameList();
            Assert.That(result.Count, Is.EqualTo(0));

            //spItem.ISplitCtrl != null
            var splitctrlMock = new Mock<ISplitCtrl>();
            splitctrlMock.Setup(x => x.FriendlyName).Returns("friendlyName");
            var vm = new SplitItemViewModel() { SplitCtrl = splitctrlMock.Object };
            var splitItem = new SplitItem();
            var prio = new PrivateObject(splitItem);
            prio.SetFieldOrProperty("vm", vm);
            var splitList = new ObservableCollection<SplitItem>() { new SplitItem(), splitItem };
            privateObject.SetFieldOrProperty("_splitList", splitList);
            result = splitListViewModel.GetCustomFriendlyNameList();
            Assert.That(result.Count, Is.EqualTo(1));
        }

        [Test]
        public void TestFindItemBySplitJson()
        {
            var spj = new SplitJson() { CustomId=2};
            //_splitList == null
            privateObject.SetFieldOrProperty("_splitList", null);
            var result = splitListViewModel.FindItemBySplitJson(spj);
            Assert.That(result, Is.EqualTo(null));

            //_splitList.Count == 0
            var splitList = new ObservableCollection<SplitItem>() { };
            privateObject.SetFieldOrProperty("_splitList", splitList);
            result = splitListViewModel.FindItemBySplitJson(spj);
            Assert.That(result, Is.EqualTo(null));

            //spItem.CustomId != spj.CustomId
            splitList = new ObservableCollection<SplitItem>() { new SplitItem() };
            privateObject.SetFieldOrProperty("_splitList", splitList);
            result = splitListViewModel.FindItemBySplitJson(spj);
            Assert.That(result, Is.EqualTo(null));

            //spItem.CustomId == customId
            var vm = new SplitItemViewModel() { CustomId = 2 };
            var splitItem = new SplitItem();
            var prio = new PrivateObject(splitItem);
            prio.SetFieldOrProperty("vm", vm);
            splitList = new ObservableCollection<SplitItem>() { new SplitItem(), splitItem };
            privateObject.SetFieldOrProperty("_splitList", splitList);
            result = splitListViewModel.FindItemBySplitJson(spj);
            Assert.That(result, Is.Not.Null);

            //spItem.CustomId == 0
            var splitctrlMock = new Mock<ISplitCtrl>();
            splitctrlMock.Setup(x => x.CellCount).Returns(1);
            splitctrlMock.Setup(x => x.SplitKey).Returns('A');
            vm = new SplitItemViewModel() { CustomId = 0, SplitCtrl = splitctrlMock.Object };
            splitItem = new SplitItem() { };
            prio = new PrivateObject(splitItem);
            prio.SetFieldOrProperty("vm", vm);
            splitList = new ObservableCollection<SplitItem>() { new SplitItem(), splitItem };
            privateObject.SetFieldOrProperty("_splitList", splitList);
            spj = new SplitJson() { CustomId = 0, CellCount = 1, SplitKey = 'A' };
            result = splitListViewModel.FindItemBySplitJson(spj);
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public void TestGetLatestItem()
        {
            //_splitList == null
            privateObject.SetFieldOrProperty("_splitList", null);
            var result = splitListViewModel.GetLatestItem();
            Assert.That(result, Is.EqualTo(null));

            //_splitList.Count == 0
            var splitList = new ObservableCollection<SplitItem>() { };
            privateObject.SetFieldOrProperty("_splitList", splitList);
            result = splitListViewModel.GetLatestItem();
            Assert.That(result, Is.EqualTo(null));

            //_splitList.Count != 0
            var splitctrlMock = new Mock<ISplitCtrl>();
            var vm = new SplitItemViewModel() { CustomId = 0, SplitCtrl = splitctrlMock.Object };
            var splitItem = new SplitItem() { };
            var prio = new PrivateObject(splitItem);
            prio.SetFieldOrProperty("vm", vm);
            splitList = new ObservableCollection<SplitItem>() { new SplitItem(), splitItem };
            privateObject.SetFieldOrProperty("_splitList", splitList);   
            result = splitListViewModel.GetLatestItem();
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public void TestIndexToItem0()
        {
            splitListViewModel.IndexToItem0 = 1;

            // Assert
            Assert.That(splitListViewModel.IndexToItem0, Is.EqualTo(1));
        }

        [Test]
        public void TestIsIndexValid()
        {
            //idx < 0
            var result = splitListViewModel.IsIndexValid(-1);
            Assert.That(result, Is.EqualTo(false));

            result = splitListViewModel.IsIndexValid(1);
            Assert.That(result, Is.EqualTo(false));

            //idx >= ItemCount
            var splitctrlMock = new Mock<ISplitCtrl>();
            splitctrlMock.Setup(x => x.FriendlyName).Returns("friendlyName");
            var vm = new SplitItemViewModel() { SplitCtrl = splitctrlMock.Object };
            var splitItem = new SplitItem();
            var prio = new PrivateObject(splitItem);
            prio.SetFieldOrProperty("vm", vm);
            var splitList = new ObservableCollection<SplitItem>() { new SplitItem(), splitItem };
            privateObject.SetFieldOrProperty("_splitList", splitList);
            result = splitListViewModel.IsIndexValid(1);
            Assert.That(result, Is.EqualTo(true));
        }

        [Test]
        public void TestSplitItem0()
        {
            // Act
            var result = splitListViewModel.SplitItem0;
            // Assert
            Assert.That(result, Is.EqualTo(null));

            // Act
            var splitList = new ObservableCollection<SplitItem>() { new SplitItem(), new SplitItem() };
            privateObject.SetFieldOrProperty("_splitList", splitList);
            splitListViewModel.IndexToItem0 = 0;
            result = splitListViewModel.SplitItem0;
            // Assert
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public void TestSplitItem1()
        {
            // Act
            var result = splitListViewModel.SplitItem1;
            // Assert
            Assert.That(result, Is.EqualTo(null));

            // Act
            var splitList = new ObservableCollection<SplitItem>() { new SplitItem(), new SplitItem() };
            privateObject.SetFieldOrProperty("_splitList", splitList);
            splitListViewModel.IndexToItem0 = 0;
            result = splitListViewModel.SplitItem1;
            // Assert
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public void TestSplitItem2()
        {
            // Act
            var result = splitListViewModel.SplitItem2;
            // Assert
            Assert.That(result, Is.EqualTo(null));

            // Act
            var splitList = new ObservableCollection<SplitItem>() { new SplitItem(), new SplitItem(),new SplitItem() };
            privateObject.SetFieldOrProperty("_splitList", splitList);
            splitListViewModel.IndexToItem0 = 0;
            result = splitListViewModel.SplitItem2;
            // Assert
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public void TestSplitItem3()
        {
            // Act
            var result = splitListViewModel.SplitItem3;
            // Assert
            Assert.That(result, Is.EqualTo(null));

            // Act
            var splitList = new ObservableCollection<SplitItem>() { new SplitItem(), new SplitItem(), new SplitItem(),new SplitItem() };
            privateObject.SetFieldOrProperty("_splitList", splitList);
            splitListViewModel.IndexToItem0 = 0;
            result = splitListViewModel.SplitItem3;
            // Assert
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public void TestSplitItem4()
        {
            // Act
            var result = splitListViewModel.SplitItem4;
            // Assert
            Assert.That(result, Is.EqualTo(null));

            // Act
            var splitList = new ObservableCollection<SplitItem>() { new SplitItem(), new SplitItem(), new SplitItem(), new SplitItem(),new SplitItem() };
            privateObject.SetFieldOrProperty("_splitList", splitList);
            splitListViewModel.IndexToItem0 = 0;
            result = splitListViewModel.SplitItem4;
            // Assert
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public void TestRefreshDisplayItems()
        {
            try
            {
                splitListViewModel.RefreshDisplayItems();
                Assert.True(true);
            }
            catch (Exception ex)
            {
                Assert.Fail("not invoked");
            }
        }

        [Test]
        public void TestItemsPerPage()
        {
            splitListViewModel.ItemsPerPage = 1;

            // Assert
            Assert.That(splitListViewModel.ItemsPerPage, Is.EqualTo(1));
        }

        [Test]
        public void TestIsPrevButtonEnabled()
        {
            // Act
            var result = splitListViewModel.IsPrevButtonEnabled;
            // Assert
            Assert.That(result, Is.EqualTo(false));

            // Act
            var splitList = new ObservableCollection<SplitItem>() { new SplitItem(), new SplitItem(), new SplitItem() };
            privateObject.SetFieldOrProperty("_splitList", splitList);
            splitListViewModel.IndexToItem0 = 0;
            result = splitListViewModel.IsPrevButtonEnabled;
            // Assert
            Assert.That(result, Is.EqualTo(false));

            // Act
            splitList = new ObservableCollection<SplitItem>() { new SplitItem(), new SplitItem() };
            privateObject.SetFieldOrProperty("_splitList", splitList);
            splitListViewModel.IndexToItem0 = 2;
            result = splitListViewModel.IsPrevButtonEnabled;
            // Assert
            Assert.That(result, Is.EqualTo(true));
        }

        [Test]
        public void TestIsNextButtonEnabled()
        {
            // Act
            var result = splitListViewModel.IsNextButtonEnabled;
            // Assert
            Assert.That(result, Is.EqualTo(false));

            // Act
            splitListViewModel.HasAddButton = true;
            var splitList = new ObservableCollection<SplitItem>() { new SplitItem(), new SplitItem(), new SplitItem() };
            privateObject.SetFieldOrProperty("_splitList", splitList);
            splitListViewModel.IndexToItem0 = 0;
            result = splitListViewModel.IsNextButtonEnabled;
            // Assert
            Assert.That(result, Is.EqualTo(false));

            // Act
            splitList = new ObservableCollection<SplitItem>() { new SplitItem(), new SplitItem(), new SplitItem(), new SplitItem(),new SplitItem() };
            privateObject.SetFieldOrProperty("_splitList", splitList);
            splitListViewModel.IndexToItem0 = 0;
            result = splitListViewModel.IsNextButtonEnabled;
            // Assert
            Assert.That(result, Is.EqualTo(true));
        }

        [Test]
        public void TestRefreshPrevNextButtons()
        {
            try
            {
                splitListViewModel.RefreshPrevNextButtons();
                Assert.True(true);
            }
            catch (Exception ex)
            {
                Assert.Fail("not invoked");
            }
        }

        [Test]
        public void TestGoToNextPage()
        {
            // Act
            var result = splitListViewModel.GoToNextPage();
            // Assert
            Assert.That(result, Is.EqualTo(false));

            // Act
            var splitList = new ObservableCollection<SplitItem>() { new SplitItem(), new SplitItem(), new SplitItem(), new SplitItem(), new SplitItem(),new SplitItem() };
            privateObject.SetFieldOrProperty("_splitList", splitList);
            splitListViewModel.IndexToItem0 = 0;
            result = splitListViewModel.GoToNextPage();
            // Assert
            Assert.That(result, Is.EqualTo(true));
        }

        [Test]
        public void TestGoToPrevPage()
        {
            // Act
            var result = splitListViewModel.GoToPrevPage();
            // Assert
            Assert.That(result, Is.EqualTo(false));

            // Act
            var splitList = new ObservableCollection<SplitItem>() { new SplitItem(), new SplitItem(), new SplitItem(), new SplitItem() };
            privateObject.SetFieldOrProperty("_splitList", splitList);
            splitListViewModel.IndexToItem0 = 8;
            result = splitListViewModel.GoToPrevPage();
            // Assert
            Assert.That(result, Is.EqualTo(true));
        }

        [Test]
        public void TestGotoFirstPage()
        {
            try
            {
                splitListViewModel.GotoFirstPage();
                Assert.True(true);
                Assert.That(splitListViewModel.IndexToItem0, Is.EqualTo(0));
            }
            catch (Exception ex)
            {
                Assert.Fail("not invoked");
            }
        }

        [Test]
        public void TestGotoFirstSelectedItemPage()
        {
            // Act
            var splitList = new ObservableCollection<SplitItem>() { new SplitItem(), new SplitItem() };
            privateObject.SetFieldOrProperty("_splitList", splitList);
            var result = splitListViewModel.GotoFirstSelectedItemPage();
            // Assert
            Assert.That(result, Is.EqualTo(false));

            // Act
            splitList = new ObservableCollection<SplitItem>() { new SplitItem(), new SplitItem() { IsSelected =true} };
            privateObject.SetFieldOrProperty("_splitList", splitList);
            result = splitListViewModel.GotoFirstSelectedItemPage();
            // Assert
            Assert.That(result, Is.EqualTo(false));
        }

        [Test]
        public void TestIsVertical()
        {
            splitListViewModel.IsVertical = true;

            // Assert
            Assert.That(splitListViewModel.IsVertical, Is.EqualTo(true));
        }

        [Test]
        public void TestHasAddButton()
        {
            splitListViewModel.HasAddButton = true;

            // Assert
            Assert.That(splitListViewModel.HasAddButton, Is.EqualTo(true));
        }

        [Test]
        public void TestAddButtonColumn()
        {
            splitListViewModel.AddButtonColumn = 1;

            // Assert
            Assert.That(splitListViewModel.AddButtonColumn, Is.EqualTo(1));
        }


        [Test]
        public void TestIsAddButtonVisible()
        {
            splitListViewModel.IsAddButtonVisible = true;

            // Assert
            Assert.That(splitListViewModel.IsAddButtonVisible, Is.EqualTo(true));
        }
    }
}

