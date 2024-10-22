using DDPM.Easy.Common;
using DDPM.SA.Common;
using DDPM.UI.Common.Interfaces;
using DDPM.UI.Common.Models;
using DDPM.UI.Common.UserControls;
using DDPM.UI.Common.ViewModels;
using Moq;
using NGA.UnitTest.PrivateObject;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using Windows.Storage;

namespace DDPM.UI.Common.Tests
{
    [Apartment(ApartmentState.STA)]
    public class SplitItemTests
    {
        private SplitItem? splitItem;
        private PrivateObject?privateObject;
        private Mock<IDeviceManagerSA>? deviceManagerSAMock;

        [SetUp]
        public void Setup()
        {
            deviceManagerSAMock=new Mock<IDeviceManagerSA>();
            DdpmCommonHelper.DeviceManagerSA=deviceManagerSAMock.Object;
            splitItem = new SplitItem();
            privateObject = new PrivateObject(splitItem);
        }

        [Test]
        public void TestConstructor_SplitItem()
        {
            // Assert
            Assert.That(splitItem, Is.Not.Null);
            Assert.That(splitItem.DataContext, Is.EqualTo(privateObject.GetFieldOrProperty("vm")));
        }

        [Test]
        public void TestInnerContent()
        {
            var ISplitAMock = new Mock<ISplit>();
            splitItem.InnerContent= ISplitAMock.Object;
            SplitItemViewModel vm = (SplitItemViewModel)privateObject.GetFieldOrProperty("vm");

            // Assert
            Assert.That(vm.Split, Is.EqualTo(ISplitAMock.Object));
            Assert.That(splitItem.InnerContent, Is.EqualTo(ISplitAMock.Object));

            var ISplitCtrlMock = new Mock<ISplitCtrl>();
            splitItem.InnerContent = ISplitCtrlMock.Object;
            vm = (SplitItemViewModel)privateObject.GetFieldOrProperty("vm");

            // Assert
            Assert.That(vm.SplitCtrl, Is.EqualTo(ISplitCtrlMock.Object));
            Assert.That(splitItem.InnerContent, Is.EqualTo(ISplitCtrlMock.Object));
        }

        [Test]
        public void TestISplit()
        {
            // Act
            SplitItemViewModel vm = (SplitItemViewModel)privateObject.GetFieldOrProperty("vm");
            // Assert
            Assert.That(splitItem.ISplit, Is.EqualTo(vm.Split));
        }

        [Test]
        public void TestISplitCtrl()
        {
            // Act
            SplitItemViewModel vm = (SplitItemViewModel)privateObject.GetFieldOrProperty("vm");
            // Assert
            Assert.That(splitItem.ISplitCtrl, Is.EqualTo(vm.SplitCtrl));
        }

        [Test]
        public void TestClickCommand()
        {
            var clickCommandMock = new Mock<ICommand>();
            var clickCommand = clickCommandMock.Object;
            splitItem.ClickCommand = clickCommand;
            // Assert
            Assert.That(splitItem.ClickCommand, Is.EqualTo(clickCommand));
        }

        [Test]
        public void TestIsEditEnabled()
        {
            // Act
            splitItem.IsEditEnabled = true;
            // Assert
            Assert.That(splitItem.IsEditEnabled, Is.EqualTo(true));
        }

        [Test]
        public void TestEditClickCommand()
        {
            var editClickCommandMock = new Mock<ICommand>();
            splitItem.EditClickCommand = editClickCommandMock.Object;
            // Assert
            Assert.That(splitItem.EditClickCommand, Is.EqualTo(editClickCommandMock.Object));
        }

        [Test]
        public void TestIsDeleteEnabled()
        {
            // Act
            splitItem.IsDeleteEnabled = true;
            // Assert
            Assert.That(splitItem.IsDeleteEnabled, Is.EqualTo(true));
        }

        [Test]
        public void TestDeleteCommand()
        {
            var deleteCommandMock = new Mock<ICommand>();
            splitItem.DeleteCommand = deleteCommandMock.Object;
            // Assert
            Assert.That(splitItem.DeleteCommand, Is.EqualTo(deleteCommandMock.Object));
        }

        [Test]
        public void TestCustomId()
        {
            // Act
            splitItem.CustomId = 111111;
            // Assert
            Assert.That(splitItem.CustomId, Is.EqualTo(111111));
        }

        [Test]
        public void TestBuddy()
        {
            // Act
            var buddy = new SplitItem();
            splitItem.Buddy = buddy;
            // Assert
            Assert.That(splitItem.Buddy, Is.EqualTo(buddy));
        }

        [Test]
        public void TestCellCount()
        {
            // Act

            // Assert
            Assert.That(splitItem.CellCount, Is.EqualTo(0));

            // Act
            var splitItemViewModel=new SplitItemViewModel();
            var SplitCtrlMock = new Mock<ISplitCtrl>();
            SplitCtrlMock.Setup(x=>x.CellCount).Returns(1);
            var splitCtrl = SplitCtrlMock.Object;
            splitItemViewModel.SplitCtrl = splitCtrl;
            SplitItemViewModel vm = new SplitItemViewModel() { SplitCtrl = splitCtrl };
            privateObject.SetFieldOrProperty("vm", vm);
            // Assert
            Assert.That(splitItem.CellCount, Is.EqualTo(1));
        }

        [Test]
        public void TestSplitKey()
        {
            // Act

            // Assert
            Assert.That(splitItem.SplitKey, Is.EqualTo('A'));

            // Act
            var splitItemViewModel = new SplitItemViewModel();
            var SplitCtrlMock = new Mock<ISplitCtrl>();
            SplitCtrlMock.Setup(x => x.SplitKey).Returns('B');
            var splitCtrl = SplitCtrlMock.Object;
            splitItemViewModel.SplitCtrl = splitCtrl;
            SplitItemViewModel vm = new SplitItemViewModel() { SplitCtrl = splitCtrl };
            privateObject.SetFieldOrProperty("vm", vm);
            // Assert
            Assert.That(splitItem.SplitKey, Is.EqualTo('B'));
        }

        [Test]
        public void TestSettings()
        {
            // Act

            // Assert
            Assert.That(splitItem.Settings, Is.Not.Null);

            // Act
            var splitItemViewModel = new SplitItemViewModel();
            var SplitCtrlMock = new Mock<ISplitCtrl>();
            var result = new List<double>() { 2.0, 2.1 };
            SplitCtrlMock.Setup(x => x.Settings).Returns(result);
            var splitCtrl = SplitCtrlMock.Object;
            splitItemViewModel.SplitCtrl = splitCtrl;
            SplitItemViewModel vm = new SplitItemViewModel() { SplitCtrl = splitCtrl };
            privateObject.SetFieldOrProperty("vm", vm);
            // Assert
            Assert.That(splitItem.Settings, Is.EqualTo(result));
        }

        [Test]
        public void TestCustomName()
        {
            // Act

            // Assert
            Assert.That(splitItem.CustomName, Is.EqualTo(""));

            // Act
            var splitItemViewModel = new SplitItemViewModel();
            var SplitCtrlMock = new Mock<ISplitCtrl>();
            SplitCtrlMock.Setup(x => x.FriendlyName).Returns("FriendlyName");
            var splitCtrl = SplitCtrlMock.Object;
            splitItemViewModel.SplitCtrl = splitCtrl;
            SplitItemViewModel vm = new SplitItemViewModel() { SplitCtrl = splitCtrl };
            privateObject.SetFieldOrProperty("vm", vm);
            // Assert
            Assert.That(splitItem.CustomName, Is.EqualTo("FriendlyName"));
        }

        [Test]
        public void TestToSplitJson()
        {
            // Act

            // Assert
            Assert.That(splitItem.ToSplitJson, Is.Not.Null);
        }

        [Test]
        public void TestIsEquals()
        {
            //ISplitCtrl == null
            // Act
            SplitItem other = new SplitItem() { CustomId= 1 };
            var result=splitItem.IsEquals(other);
            // Assert
            Assert.That(result, Is.EqualTo(true));

            //other.ISplitCtrl == null
            // Act
            var splitItemViewModel = new SplitItemViewModel();
            var SplitCtrlMock = new Mock<ISplitCtrl>();
            var splitCtrl = SplitCtrlMock.Object;
            splitItemViewModel.SplitCtrl = splitCtrl;
            SplitItemViewModel vm = new SplitItemViewModel() { SplitCtrl = splitCtrl };
            privateObject.SetFieldOrProperty("vm", vm);
            result = splitItem.IsEquals(other);
            Assert.That(result, Is.EqualTo(true));

            //CustomId == 0
            result = splitItem.IsEquals(splitItem);
            // Assert
            Assert.That(result, Is.EqualTo(true));

            //CustomId != other.CustomId
            splitItem.CustomId = 2;
            result = splitItem.IsEquals(splitItem);
            Assert.That(result, Is.EqualTo(true));
        }

        [Test]
        public void TestReplaceByEAArgs()
        {
            //ISplitCtrl == null
            var args = new EAArgs() { CellCount = 0, SplitKey = 'A' };
            try
            {
                splitItem.ReplaceByEAArgs(args);
                Assert.True(true);
            }
            catch (Exception ex)
            {
                Assert.Fail("not invoked");
            }
            //(args.CellCount == 0) && (args.SplitKey == 'B')
            var splitItemViewModel = new SplitItemViewModel();
            var SplitCtrlMock = new Mock<ISplitCtrl>();
            SplitCtrlMock.Setup(x => x.CellCount).Returns(1);
            var splitCtrl = SplitCtrlMock.Object;
            splitItemViewModel.SplitCtrl = splitCtrl;
            SplitItemViewModel vm = new SplitItemViewModel() { SplitCtrl = splitCtrl };
            privateObject.SetFieldOrProperty("vm", vm);
            args = new EAArgs() { CellCount = 0,SplitKey='B',CustomName="name",Settings=new List<double>() { 2.2,2.3} };
            try
            {
                splitItem.ReplaceByEAArgs(args);
                Assert.True(true);
            }
            catch (Exception ex)
            {
                Assert.Fail("not invoked");
            }

            args = new EAArgs() { CellCount = 0, SplitKey = 'A', CustomName = "name", Settings = new List<double>() { 2.2, 2.3 } };
            try
            {
                splitItem.ReplaceByEAArgs(args);
                Assert.True(true);
            }
            catch (Exception ex)
            {
                Assert.Fail("not invoked");
            }



        }

        [Test]
        public void TestIsHoverable()
        {
            // Act
            splitItem.IsHoverable=true;
            // Assert
            Assert.That(splitItem.IsHoverable, Is.EqualTo(true));
        }

        [Test]
        public void TestIsAddedCustomLayout()
        {
            // Act

            // Assert
            Assert.That(splitItem.IsAddedCustomLayout, Is.EqualTo(false));

            //ISplitCtrl != null
            var splitItemViewModel = new SplitItemViewModel();
            var SplitCtrlMock = new Mock<ISplitCtrl>();
            SplitCtrlMock.Setup(x=>x.IsAddedCustomLayout).Returns(true);
            var splitCtrl = SplitCtrlMock.Object;
            splitItemViewModel.SplitCtrl = splitCtrl;
            SplitItemViewModel vm = new SplitItemViewModel() { SplitCtrl = splitCtrl };
            privateObject.SetFieldOrProperty("vm", vm);
            var result = splitItem.IsAddedCustomLayout;
            Assert.That(result, Is.EqualTo(true));
        }

        [Test]
        public void TestLayoutID()
        {
            // Act
            splitItem.LayoutID = 1;
            // Assert
            Assert.That(splitItem.LayoutID, Is.EqualTo(1));
        }
    }
}
