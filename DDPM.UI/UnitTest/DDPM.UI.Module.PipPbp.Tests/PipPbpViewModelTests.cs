using DDPM.SA.Common;
using DDPM.SA.Common.Display;
using DDPM.UI.Common;
using DDPM.UI.Common.EAEM;
using DDPM.UI.Common.Interfaces;
using DDPM.UI.Common.Models;
using DDPM.UI.Common.UserControls;
using Dell.Client.Framework.Common;
using Moq;
using NGA.UnitTest.PrivateObject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using VcpCore.Common;

namespace DDPM.UI.Module.PipPbp.Tests
{
    [Apartment(ApartmentState.STA)]
    public class PipPbpViewModelTests
    {

        private PipPbpViewModel? pipPbpViewModel;
        private PrivateObject? privateObject;
        private Mock<ILog>? logMock;
        private ILog? log;
        private Mock<IModuleOwner> moduleOwnerMock;
        private IModuleOwner moduleOwner;
        private Mock<ICommand> commandMock;
        private ICommand command;

        [SetUp]
        public void SetUp()
        {
            logMock = new Mock<ILog>();
            log = logMock.Object;
            moduleOwnerMock=new Mock<IModuleOwner>();
            moduleOwner = moduleOwnerMock.Object;
            commandMock = new Mock<ICommand>();
            command = commandMock.Object;
            pipPbpViewModel = new PipPbpViewModel();
            privateObject = new PrivateObject(pipPbpViewModel);
        }

        [Test]
        public void TestConstructor_PipPbpViewModel()
        {
            // Assert
            Assert.That(pipPbpViewModel, Is.Not.Null);
        }

        [Test]
        public void TestLog()
        {
            pipPbpViewModel.Log = log;

            // Assert
            Assert.That(pipPbpViewModel.Log, Is.EqualTo(log));
        }

        [Test]
        public void TestSplitItem_Off()
        {
            var splitItem_Off = new SplitItem();
            pipPbpViewModel.SplitItem_Off = splitItem_Off;

            // Assert
            Assert.That(pipPbpViewModel.SplitItem_Off, Is.EqualTo(splitItem_Off));
        }

        [Test]
        public void TestSplitItem_PipSmall()
        {
            var splitItem_PipSmall = new SplitItem();
            pipPbpViewModel.SplitItem_PipSmall = splitItem_PipSmall;

            // Assert
            Assert.That(pipPbpViewModel.SplitItem_PipSmall, Is.EqualTo(splitItem_PipSmall));
        }

        [Test]
        public void TestSplitItem_PipLarge()
        {
            var splitItem_PipLarge = new SplitItem();
            pipPbpViewModel.SplitItem_PipLarge = splitItem_PipLarge;

            // Assert
            Assert.That(pipPbpViewModel.SplitItem_PipLarge, Is.EqualTo(splitItem_PipLarge));
        }


        [Test]
        public void TestSplitListView_Pbp()
        {
            var splitListView_Pbp = new SplitListView();
            pipPbpViewModel.SplitListView_Pbp = splitListView_Pbp;

            // Assert
            Assert.That(pipPbpViewModel.SplitListView_Pbp, Is.EqualTo(splitListView_Pbp));
        }

        [Test]
        public void TestModuleOwner()
        {
            pipPbpViewModel.ModuleOwner = moduleOwner;
            Assert.That(pipPbpViewModel.ModuleOwner, Is.EqualTo(moduleOwner));
        }

        [Test]
        public void TestTest1Value()
        {
            var test1Value = "bbb";
            pipPbpViewModel.Test1Value = test1Value;

            // Assert
            Assert.That(pipPbpViewModel.Test1Value, Is.EqualTo("bbb"));
        }

        [Test]
        public void TestTest2Value()
        {
            var test2Value = "ccc";
            pipPbpViewModel.Test2Value = test2Value;

            // Assert
            Assert.That(pipPbpViewModel.Test2Value, Is.EqualTo("ccc"));
        }

        
        [Test]
        public void TestCloseFullViewCommand()
        {
            var closeFullViewCommand = command;               
            pipPbpViewModel.CloseFullViewCommand = closeFullViewCommand;

            // Assert
            Assert.That(pipPbpViewModel.CloseFullViewCommand, Is.EqualTo(closeFullViewCommand));
        }

        [Test]
        public void TestGotoNextCommand()
        {
            var gotoNextCommand = command;      
            pipPbpViewModel.GotoNextCommand = gotoNextCommand;

            // Assert
            Assert.That(pipPbpViewModel.GotoNextCommand, Is.EqualTo(gotoNextCommand));
        }


        [Test]
        public void TestGotoPrevCommand()
        {
            var gotoPrevCommand = command;    
            pipPbpViewModel.GotoPrevCommand = gotoPrevCommand;

            // Assert
            Assert.That(pipPbpViewModel.GotoPrevCommand, Is.EqualTo(gotoPrevCommand));
        }

        [Test]
        public void TestFullScreenClickCommand()
        {
            var fullScreenClickCommand = command;
            pipPbpViewModel.FullScreenClickCommand = fullScreenClickCommand;

            // Assert
            Assert.That(pipPbpViewModel.FullScreenClickCommand, Is.EqualTo(fullScreenClickCommand));
        }

        [Test]
        public void TestPipSmallClickCommand()
        {
            var pipSmallClickCommand = command;    
            pipPbpViewModel.PipSmallClickCommand = pipSmallClickCommand;

            // Assert
            Assert.That(pipPbpViewModel.PipSmallClickCommand, Is.EqualTo(pipSmallClickCommand));
        }

        [Test]
        public void TestPipLargeClickCommand()
        {
            var pipLargeClickCommand = command;           
            pipPbpViewModel.PipLargeClickCommand = pipLargeClickCommand;

            // Assert
            Assert.That(pipPbpViewModel.PipLargeClickCommand, Is.EqualTo(pipLargeClickCommand));
        }

        [Test]
        public void TestPbpItemClickCommand()
        {
            var pbpItemClickCommand = command;            
            pipPbpViewModel.PbpItemClickCommand = pbpItemClickCommand;

            // Assert
            Assert.That(pipPbpViewModel.PbpItemClickCommand, Is.EqualTo(pbpItemClickCommand));
        }

        [Test]
        public void TestOnFullScreenClicked()
        {            
            var deviceManagerMock = new Mock<IDeviceManagerSA>();
            var deviceManagerSA = deviceManagerMock.Object;
            DdpmCommonHelper.DeviceManagerSA = deviceManagerSA;
            DdpmCommonHelper.ModuleOwner = moduleOwner;
            moduleOwnerMock.Setup(x => x.SelectedHomeDevice).Returns(new HomeDevice());
            var monitorInfo = new MonitorInfo();
            DdpmCommonHelper.ModuleOwner.SelectedHomeDevice.MonitorInfo=monitorInfo;

            var spititem = new SplitItem();
            try
            {
                pipPbpViewModel.OnFullScreenClicked(spititem);
                Assert.True(true);
                Assert.That(pipPbpViewModel.SelectedSplitItem, Is.EqualTo(spititem));
            }
            catch (Exception ex)
            {
                Assert.Fail("not invoked");
            }
        }


        [Test]
        public void TestHasPxpCap()
        {
            bool result= pipPbpViewModel.HasPxpCap(0x11);
            Assert.That(result, Is.False);
            privateObject.SetFieldOrProperty("_pipPbpCaps",new UInt16[2] { 0x11,0x22});
            result = pipPbpViewModel.HasPxpCap(0x11);
            Assert.That(result, Is.True);

        }


        [Test]
        public void TestSelectedSplitItem()
        {
            //_selectedSplitItem != null&&_selectedSplitItem == value
            var selectedSplitItem = new SplitItem();
            privateObject.SetFieldOrProperty("_selectedSplitItem", selectedSplitItem);
            pipPbpViewModel.SelectedSplitItem = selectedSplitItem;
            Assert.That(pipPbpViewModel.SelectedSplitItem, Is.EqualTo(selectedSplitItem));

            //_selectedSplitItem != null&&_selectedSplitItem != value
            privateObject.SetFieldOrProperty("_selectedSplitItem", new SplitItem() { IsSelected=true});
            pipPbpViewModel.SelectedSplitItem=selectedSplitItem;
            // Assert
            Assert.That(pipPbpViewModel.SelectedSplitItem, Is.EqualTo(selectedSplitItem));
            Assert.That(pipPbpViewModel.SelectedSplitItem.IsSelected, Is.EqualTo(true));

            //_selectedSplitItem != null&&value==null
            privateObject.SetFieldOrProperty("_selectedSplitItem", new SplitItem() { IsSelected = true });
            pipPbpViewModel.SelectedSplitItem = null;
            // Assert
            Assert.That(pipPbpViewModel.SelectedSplitItem, Is.EqualTo(null));

            //_selectedSplitItem == null&&value!=null
            privateObject.SetFieldOrProperty("_selectedSplitItem", null);
            pipPbpViewModel.SelectedSplitItem = selectedSplitItem;
            Assert.That(pipPbpViewModel.SelectedSplitItem.IsSelected, Is.EqualTo(true));

            //_selectedSplitItem == null&&value==null
            privateObject.SetFieldOrProperty("_selectedSplitItem", null);
            pipPbpViewModel.SelectedSplitItem = null;
            Assert.That(pipPbpViewModel.SelectedSplitItem, Is.EqualTo(null));
        }


        [Test]
        public void TestPipTogglePositionClickCommand()
        {
            var pipTogglePositionClickCommand = command;
            pipPbpViewModel.PipTogglePositionClickCommand = pipTogglePositionClickCommand;

            // Assert
            Assert.That(pipPbpViewModel.PipTogglePositionClickCommand, Is.EqualTo(pipTogglePositionClickCommand));
        }

        [Test]
        public void TestIsPipItemSelectedy()
        {
           // Assert
            Assert.That(pipPbpViewModel.IsPipItemSelected, Is.EqualTo(false));

            pipPbpViewModel.SelectedSplitItem = new SplitItem();
            pipPbpViewModel.SelectedSplitItem.SplitOwner = eSplitOwner.PipList;
            Assert.That(pipPbpViewModel.IsPipItemSelected, Is.EqualTo(true));

            pipPbpViewModel.SelectedSplitItem.SplitOwner = eSplitOwner.PxpOff;
            Assert.That(pipPbpViewModel.IsPipItemSelected, Is.EqualTo(false));
        }


        [Test]
        public void TestInputSourceList()
        {
            var selectedSplitItem = new List<InputSourceObj>();
            pipPbpViewModel.InputSourceList = selectedSplitItem;
            Assert.That(pipPbpViewModel.InputSourceList, Is.EqualTo(selectedSplitItem));
        }


        [Test]
        public void TestMainInputSource()
        {
            var mainInputSource = new InputSourceObj();
            pipPbpViewModel.MainInputSource = mainInputSource;
            Assert.That(pipPbpViewModel.MainInputSource, Is.EqualTo(mainInputSource));
        }


        [Test]
        public void TestSubInputs()
        {
            var subInputs = new List<InputSourceObj>();
            pipPbpViewModel.SubInputs = subInputs;
            Assert.That(pipPbpViewModel.SubInputs, Is.EqualTo(subInputs));
        }


        [Test]
        public void TestIsBusy()
        {
            var isBusy=true;
            pipPbpViewModel.IsBusy = isBusy;

            // Assert
            Assert.That(pipPbpViewModel.IsBusy, Is.EqualTo(true));
        }
        
    }
}
