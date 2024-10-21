using DDPM.Easy.Common;
using DDPM.SA.Common;
using DDPM.SA.Common.Display;
using DDPM.UI.Common.EAEM;
using DDPM.UI.Common.Interfaces;
using DDPM.UI.Common.Models;
using DDPM.UI.Common.UserControls;
using DDPM.UI.Common.ViewModels;
using Dell.Client.Framework.UX.WPF.Controls;
using Moq;
using NGA.UnitTest.PrivateObject;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Drawing;
using System.Linq;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using Windows.Storage;

namespace DDPM.UI.Common.Tests
{
    [Apartment(ApartmentState.STA)]
    public class SplitListViewTests
    {
        private SplitListView? splitListView;
        private PrivateObject?privateObject;
        private Mock<IDeviceManagerSA>? deviceManagerSAMock;
        private SplitListViewModel? vm;

        [SetUp]
        public void Setup()
        {
            deviceManagerSAMock=new Mock<IDeviceManagerSA>();
            DdpmCommonHelper.DeviceManagerSA=deviceManagerSAMock.Object;
            splitListView = new SplitListView();
            privateObject = new PrivateObject(splitListView);
            vm = new SplitListViewModel();
        }

        [Test]
        public void TestConstructor_SplitListView()
        {
            // Assert
            Assert.That(splitListView, Is.Not.Null);
            Assert.That(splitListView.DataContext, Is.EqualTo(privateObject.GetFieldOrProperty("vm")));
            Assert.That(splitListView.addButton.ClickCommand, Is.Not.Null);
        }

        [Test]
        public void TestSplitOwner()
        {
            // Act
            var splitOwner = new eSplitOwner();
            splitListView.SplitOwner = splitOwner;
            // Assert
            Assert.That(splitListView.SplitOwner, Is.EqualTo(splitOwner));
        }

        [Test]
        public void TestSplitList()
        {
            // Act
            SplitListViewModel vm = (SplitListViewModel) privateObject.GetFieldOrProperty("vm");
            // Assert
            Assert.That(splitListView.SplitList, Is.EqualTo(vm.SplitList));
        }

        [Test]
        public void TestItemCount()
        {
            // Act
            SplitListViewModel vm = (SplitListViewModel)privateObject.GetFieldOrProperty("vm");
            // Assert
            Assert.That(splitListView.ItemCount, Is.EqualTo(vm.ItemCount));
        }

        [Test]
        public void TestGetAt()
        {
            // Act
            //index < 0
            var result = splitListView.GetAt(-1);
            // Assert
            Assert.That(result, Is.EqualTo(null));

            // Act
            //index >= ItemCount
            result = splitListView.GetAt(3);
            // Assert
            Assert.That(result, Is.EqualTo(null));

            // Act
            SplitListViewModel vm = new SplitListViewModel();
            vm.SplitList=new ObservableCollection<SplitItem>() { new SplitItem() ,new SplitItem() { SplitOwner= eSplitOwner.EaWin} };
            privateObject.SetFieldOrProperty("vm", vm);
            result = splitListView.GetAt(1);
            // Assert
            Assert.That(result, Is.EqualTo(vm.SplitList[1]));
        }

        [Test]
        public void TestClearList()
        {
            try
            {
                splitListView.ClearList();
                Assert.True(true);
            }
            catch (Exception ex)
            {
                Assert.Fail("not invoked");
            }
        }

        [Test]
        public void TestAddSplitToList()
        {
            // Act
            var splitMock = new Mock<ISplit>();
            var result = splitListView.AddSplitToList(splitMock.Object);
            // Assert
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public void TestAddItemToList()
        {
            // Act
            var contentControl = new ContentControl();
            var vma = new SplitListViewModel() { SplitOwner = eSplitOwner.EaCustom };
            privateObject.SetFieldOrProperty("vm",vma);
            var result = splitListView.AddItemToList(contentControl);
            // Assert
            Assert.That(result, Is.Not.Null);

            vma = new SplitListViewModel() { SplitOwner = eSplitOwner.EaWin };
            privateObject.SetFieldOrProperty("vm", vma);
            result = splitListView.AddItemToList(contentControl);
            // Assert
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public void TestInsertSplitCtrlToList()
        {
            // Act
            var ispMock = new Mock<ISplitCtrl>();
            var vma = new SplitListViewModel() { SplitOwner = eSplitOwner.EaCustom };
            privateObject.SetFieldOrProperty("vm", vma);
            var result = splitListView.InsertSplitCtrlToList(ispMock.Object,-1);
            // Assert
            Assert.That(result, Is.Not.Null);


            vma = new SplitListViewModel() { SplitOwner = eSplitOwner.EaWin };
            privateObject.SetFieldOrProperty("vm", vma);
            result = splitListView.InsertSplitCtrlToList(ispMock.Object, 1);
            // Assert
            Assert.That(result, Is.Not.Null);
        }


        [Test]
        public void TestGetCustomFriendlyNames()
        {
            // Act
            var result = splitListView.GetCustomFriendlyNames();
            // Assert
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public void TestItemClickCommand()
        {
            // Act
            var itemClickCommandMock = new Mock<ICommand>();
            splitListView.ItemClickCommand= itemClickCommandMock.Object;
            // Assert
            Assert.That(splitListView.ItemClickCommand, Is.EqualTo(itemClickCommandMock.Object));
        }

        [Test]
        public void TestGotoFirstSelectedItemPage()
        {
            // Act
            //SplitList==null
            var result = splitListView.GotoFirstSelectedItemPage();
            // Assert
            Assert.That(result, Is.EqualTo(false));

            //SplitList!=null
            var vm=new SplitListViewModel();
            vm.SplitList= new ObservableCollection<SplitItem> { new SplitItem() { CustomId = 1 }, new SplitItem() { CustomId = 0,IsSelected=true } };         
            privateObject.SetFieldOrProperty("vm", vm);
            result = splitListView.GotoFirstSelectedItemPage();
            // Assert
            Assert.That(result, Is.EqualTo(false));
        }

        [Test]
        public void TestItemEditCommand()
        {
            // Act
            var itemEditCommandMock = new Mock<ICommand>();
            splitListView.ItemEditCommand = itemEditCommandMock.Object;
            // Assert
            Assert.That(splitListView.ItemEditCommand, Is.EqualTo(itemEditCommandMock.Object));
        }

        [Test]
        public void TestItemDeleteCommand()
        {
            // Act
            var itemDeleteCommandMock = new Mock<ICommand>();
            splitListView.ItemDeleteCommand = itemDeleteCommandMock.Object;
            // Assert
            Assert.That(splitListView.ItemDeleteCommand, Is.EqualTo(itemDeleteCommandMock.Object));
        }

        [Test]
        public void TestFindSplitItem()
        {
            //_splitList.Count == 0
            var result = splitListView.FindSplitItem(1, 'A');
            // Assert
            Assert.That(result, Is.EqualTo(null));

            // Act
            //_splitList==null
            var vma = new SplitListViewModel();
            PrivateObject privateObjecta = new PrivateObject(vma);
            privateObjecta.SetFieldOrProperty("_splitList", null);
            privateObject.SetFieldOrProperty("vm", vma);
            result = splitListView.FindSplitItem(1, 'A');
            // Assert
            Assert.That(result, Is.EqualTo(null));

            // Act
            //spItem.ISplitCtrl == null
            var _splitList = new ObservableCollection<SplitItem> { new SplitItem() { }, new SplitItem() { } };
            privateObjecta.SetFieldOrProperty("_splitList", _splitList);
            privateObject.SetFieldOrProperty("vm", vma);
            result = splitListView.FindSplitItem(1, 'A');
            // Assert
            Assert.That(result, Is.EqualTo(null));

            // Act
            //spItem.ISplitCtrl != null
            var SplitCtrlMock = new Mock<ISplitCtrl>();
            var vmSplitItemViewModel = new SplitItemViewModel() { SplitCtrl = SplitCtrlMock.Object };
            SplitCtrlMock.Setup(x => x.CellCount).Returns(1);
            SplitCtrlMock.Setup(x => x.SplitKey).Returns('A');
            SplitItem splitItem = new SplitItem();
            PrivateObject privateObjectsplitItem = new PrivateObject(splitItem);
            privateObjectsplitItem.SetFieldOrProperty("vm", vmSplitItemViewModel);
            _splitList = new ObservableCollection<SplitItem> { new SplitItem() { }, splitItem };
            privateObjecta.SetFieldOrProperty("_splitList", _splitList);
            var ISplitCtrl = splitItem.ISplitCtrl;
            result = splitListView.FindSplitItem(1, 'A');
            // Assert
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public void TestFindIndexOfSelectedItem()
        {
            // Act
            var result = splitListView.FindIndexOfSelectedItem();
            // Assert
            Assert.That(result, Is.EqualTo(-1));

            var vma = new SplitListViewModel() { SplitList = new ObservableCollection<SplitItem>() { new SplitItem() { SplitOwner = eSplitOwner.EaWin, IsSelected = false }, new SplitItem() { SplitOwner = eSplitOwner.EaCustom, IsSelected = true } } };
            privateObject.SetFieldOrProperty("vm", vma);
            result = splitListView.FindIndexOfSelectedItem();
            Assert.That(result, Is.EqualTo(1));

            vma = new SplitListViewModel() { SplitList =new ObservableCollection<SplitItem>() { new SplitItem() { SplitOwner= eSplitOwner.EaWin,IsSelected=true},new SplitItem() { SplitOwner=eSplitOwner.EaCustom,IsSelected= true } } };
            privateObject.SetFieldOrProperty("vm", vma);
            result = splitListView.FindIndexOfSelectedItem();
            Assert.That(result, Is.EqualTo(0));
        }

        [Test]
        public void TestFindItemByFriendlyName()
        {
            //_splitList.Count == 0
            var result = splitListView.FindItemByFriendlyName("A");
            // Assert
            Assert.That(result, Is.EqualTo(null));

            // Act
            //_splitList==null
            var vma = new SplitListViewModel();
            PrivateObject privateObjecta = new PrivateObject(vma);
            privateObjecta.SetFieldOrProperty("_splitList", null);
            privateObject.SetFieldOrProperty("vm", vma);
            result = splitListView.FindItemByFriendlyName("A");
            // Assert
            Assert.That(result, Is.EqualTo(null));

            // Act
            //spItem.ISplitCtrl == null
            var _splitList = new ObservableCollection<SplitItem> { new SplitItem() { }, new SplitItem() { } };
            privateObjecta.SetFieldOrProperty("_splitList", _splitList);
            privateObject.SetFieldOrProperty("vm", vma);
            result = splitListView.FindItemByFriendlyName("A");
            // Assert
            Assert.That(result, Is.EqualTo(null));

            // Act
            //spItem.ISplitCtrl != null
            var SplitCtrlMock = new Mock<ISplitCtrl>();
            var vmSplitItemViewModel = new SplitItemViewModel() { SplitCtrl = SplitCtrlMock.Object };
            SplitCtrlMock.Setup(x => x.FriendlyName).Returns("A");
            SplitItem splitItem = new SplitItem();
            PrivateObject privateObjectsplitItem = new PrivateObject(splitItem);
            privateObjectsplitItem.SetFieldOrProperty("vm", vmSplitItemViewModel);
            _splitList = new ObservableCollection<SplitItem> { new SplitItem() { }, splitItem };
            privateObjecta.SetFieldOrProperty("_splitList", _splitList);
            var ISplitCtrl= splitItem.ISplitCtrl;        
            result = splitListView.FindItemByFriendlyName("A");
            // Assert
            Assert.That(result, Is.Not.Null);

        }

        [Test]
        public void TestFindItemByCustomId()
        {
            //_splitList.Count == 0
            var result = splitListView.FindItemByCustomId(2);
            // Assert
            Assert.That(result, Is.EqualTo(null));

            // Act
            //_splitList==null
            var vma = new SplitListViewModel();
            PrivateObject privateObjecta = new PrivateObject(vma);
            privateObjecta.SetFieldOrProperty("_splitList", null);
            privateObject.SetFieldOrProperty("vm", vma);
            result = splitListView.FindItemByCustomId(2);
            // Assert
            Assert.That(result, Is.EqualTo(null));

            // Act
            vma = new SplitListViewModel() { };
            privateObjecta = new PrivateObject(vma);
            var _splitList=new ObservableCollection<SplitItem> { new SplitItem() { } ,new SplitItem() { CustomId=2} };
            privateObjecta.SetFieldOrProperty("_splitList", _splitList);
            privateObject.SetFieldOrProperty("vm", vma);
            result = splitListView.FindItemByCustomId(2);
            // Assert
            Assert.That(result, Is.Not.Null);

            _splitList = new ObservableCollection<SplitItem> { new SplitItem() { }, new SplitItem() { } };
            privateObjecta.SetFieldOrProperty("_splitList", _splitList);
            privateObject.SetFieldOrProperty("vm", vma);
            result = splitListView.FindItemByCustomId(2);
            // Assert
            Assert.That(result, Is.EqualTo(null));
        }

        [Test]
        public void TestFindItemBySplitJson()
        {
            // Act
            //_splitList.Count == 0
            SplitJson spj = new SplitJson() { CustomId = 2 };
            var result = splitListView.FindItemBySplitJson(spj);
            // Assert
            Assert.That(result, Is.EqualTo(null));

            // Act
            //_splitList==null
            var vma = new SplitListViewModel();
            PrivateObject privateObjecta = new PrivateObject(vma);
            privateObjecta.SetFieldOrProperty("_splitList", null);
            privateObject.SetFieldOrProperty("vm", vma);
            result = splitListView.FindItemBySplitJson(spj);
            // Assert
            Assert.That(result, Is.EqualTo(null));

            // Act
            //spItem.CustomId == 0
            spj = new SplitJson() { CustomId = 0 ,CellCount=0,SplitKey= 'A' };
            var _splitList = new ObservableCollection<SplitItem> { new SplitItem() { CustomId = 1 }, new SplitItem() { CustomId = 0, } };
            privateObjecta.SetFieldOrProperty("_splitList", _splitList);
            result = splitListView.FindItemBySplitJson(spj);
            // Assert
            Assert.That(result, Is.Not.Null);

            // Act
            //spItem.CustomId == 0
            spj = new SplitJson() { CustomId = 1, CellCount = 1, SplitKey = 'B' };
            result = splitListView.FindItemBySplitJson(spj);
            // Assert
            Assert.That(result, Is.Not.Null);

            // Act
            //spItem.CustomId == 0
            spj = new SplitJson() { CustomId = 0, CellCount = 1, SplitKey = 'A' };
            result = splitListView.FindItemBySplitJson(spj);
            // Assert
            Assert.That(result, Is.EqualTo(null));

        }

        [Test]
        public void TestGetLatestItem()
        {
            // Act
            //_splitList.Count == 0
            var result = splitListView.GetLatestItem();
            // Assert
            Assert.That(result, Is.EqualTo(null));

            // Act
            //_splitList==null
            var vma = new SplitListViewModel();
            PrivateObject privateObjecta = new PrivateObject(vma);
            privateObjecta.SetFieldOrProperty("_splitList", null);
            privateObject.SetFieldOrProperty("vm", vma);
            result = splitListView.GetLatestItem();
            // Assert
            Assert.That(result, Is.EqualTo(null));

            // Act
            var _splitList = new ObservableCollection<SplitItem> { new SplitItem() { CustomId = 1 }, new SplitItem() { CustomId = 0, } };
            privateObjecta.SetFieldOrProperty("_splitList", _splitList);
            result = splitListView.GetLatestItem();
            // Assert
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public void TestMoveSelectedItemToSecondPosition()
        {
            try
            {
                splitListView.MoveSelectedItemToSecondPosition();
                Assert.True(true);
            }
            catch (Exception ex)
            {
                Assert.Fail("not invoked");
            }
        }

        [Test]
        public void TestAddSplitCtrlTo2ndPosition()
        {
            // Act
            var ispMock = new Mock<ISplitCtrl>();
            var vma = new SplitListViewModel() { SplitOwner = eSplitOwner.EaCustom ,SplitList=new ObservableCollection<SplitItem>() { new SplitItem(),new SplitItem()} };
            privateObject.SetFieldOrProperty("vm", vma);
            var result = splitListView.AddSplitCtrlTo2ndPosition(ispMock.Object);
            // Assert
            Assert.That(result, Is.Not.Null);

            // ActS
            vma = new SplitListViewModel() { SplitOwner = eSplitOwner.EaWin, SplitList = new ObservableCollection<SplitItem>() { new SplitItem(), new SplitItem() } };
            privateObject.SetFieldOrProperty("vm", vma);
            result = splitListView.AddSplitCtrlTo2ndPosition(ispMock.Object);
            // Assert
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public void TestDeleteSplitItem()
        {
            // Act
            var vma = new SplitListViewModel() { SplitOwner = eSplitOwner.EaWin, SplitList = new ObservableCollection<SplitItem>() { new SplitItem() } };
            privateObject.SetFieldOrProperty("vm", vma);
            var SplitItem= vma.SplitItem0 as SplitItem;
            var result = splitListView.DeleteSplitItem(SplitItem);
            // Assert
            Assert.That(result, Is.EqualTo(true));

            // Act
            vma = new SplitListViewModel() { SplitOwner = eSplitOwner.EaWin, SplitList = new ObservableCollection<SplitItem>() { new SplitItem() } };
            privateObject.SetFieldOrProperty("vm", vma);
            SplitItem = vma.SplitItem2 as SplitItem;
            result = splitListView.DeleteSplitItem(SplitItem);
            // Assert
            Assert.That(result, Is.EqualTo(false));
        }

        [Test]
        public void TestIsVertical()
        {
            // Act
            splitListView.IsVertical = true;
            // Assert
            Assert.That(splitListView.IsVertical, Is.EqualTo(true));
        }
      
        [Test]
        public void TestHasAddButton()
        {
            // Act
            splitListView.HasAddButton=true;
            // Assert
            Assert.That(splitListView.HasAddButton, Is.EqualTo(true));
        }
        
        [Test]
        public void TestAddButtonClickCommand()
        {
            // Act
            var addButtonClickCommandMock = new Mock<ICommand>();
            splitListView.AddButtonClickCommand = addButtonClickCommandMock.Object;
            // Assert
            Assert.That(splitListView.AddButtonClickCommand, Is.EqualTo(addButtonClickCommandMock.Object));
        }
    }
}
