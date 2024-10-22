using DDPM.SA.Common;
using DDPM.UI.Common;
using DDPM.UI.Common.Interfaces;
using DDPM.UI.Common.Models;
using Moq;
using System.Collections.ObjectModel;
using System.Windows;
using VcpCore.Common;

namespace DDPM.UI.Module.InputSource.Tests
{
    [Apartment(ApartmentState.STA)]
    public class InputSourceViewModelTests
    {
        [SetUp]
        public void Setup()
        {
            if (!UriParser.IsKnownScheme("pack"))
            {
                new System.Windows.Application();
            }

            var moduleOwerMock = new Mock<IModuleOwner>();
            var moduleOwer = moduleOwerMock.Object;
            moduleOwerMock.Setup(x => x.SelectedHomeDevice).Returns(new HomeDevice());
            DdpmCommonHelper.ModuleOwner = moduleOwerMock.Object;
        }

        [Test]
        public void TestinputSourceModule()
        {
            var inputSourceList = new InputSourceList();
            //var inputSourceModule = new InputSourceModule();
            //inputSourceList.inputSourceModule = inputSourceModule;
            //Assert.That(inputSourceList.inputSourceModule, Is.EqualTo(inputSourceModule));
        }

        [Test]
        public void TestinputDisplayText()
        {
            var inputSourceList = new InputSourceList();
            Assert.That(inputSourceList.inputDisplayText, Is.EqualTo(String.Empty));
        }

        //class InputSourceViewModel
        [Test]
        public void TestModuleOwner()
        {
            var moduleOwerMock = new Mock<IModuleOwner>();
            var inputSourceViewModel = new InputSourceViewModel();
            var moduleOwer = moduleOwerMock.Object;
            inputSourceViewModel.ModuleOwner = moduleOwer;
            Assert.That(inputSourceViewModel.ModuleOwner, Is.EqualTo(moduleOwer));
        }

        [Test]
        public void TestInputSourceModule()
        {
            var inputSourceViewModel = new InputSourceViewModel();
            var inputSourceModule = new InputSourceModule();
            inputSourceViewModel.InputSourceModule = inputSourceModule;
            Assert.That(inputSourceViewModel.InputSourceModule, Is.EqualTo(inputSourceModule));
        }

        [Test]
        public void Testitems()
        {
            var inputSourceViewModel = new InputSourceViewModel();
            var items = new ObservableCollection<Item>();
            inputSourceViewModel.items = items;
            Assert.That(inputSourceViewModel.items, Is.EqualTo(items));
        }

        [Test]
        public void TestinputList()
        {
            var inputSourceViewModel = new InputSourceViewModel();
            var inputList = new Dictionary<string, InputInfo>();
            inputSourceViewModel.inputList = inputList;
            Assert.That(inputSourceViewModel.inputList, Is.EqualTo(inputList));
        }

        [Test]
        public void TestusbUpstream()
        {
            var inputSourceViewModel = new InputSourceViewModel();
            var usbUpstream = new List<string>();
            inputSourceViewModel.usbUpstream = usbUpstream;
            Assert.That(inputSourceViewModel.usbUpstream, Is.EqualTo(usbUpstream));
        }

        [Test]
        public void TestIsUSBH()
        {
            var inputSourceViewModel = new InputSourceViewModel();
            inputSourceViewModel.IsUSBH = Visibility.Collapsed;
            Assert.That(inputSourceViewModel.IsUSBH, Is.EqualTo(Visibility.Collapsed));
        }

        [Test]
        public void TestNameHWidth()
        {
            var inputSourceViewModel = new InputSourceViewModel();
            inputSourceViewModel.NameHWidth = "0.5*";
            Assert.That(inputSourceViewModel.NameHWidth, Is.EqualTo("0.5*"));
        }

        [Test]
        public void TestUSBHWidth()
        {
            var inputSourceViewModel = new InputSourceViewModel();
            inputSourceViewModel.USBHWidth = "1.5*";
            Assert.That(inputSourceViewModel.USBHWidth, Is.EqualTo("1.5*"));
        }

        [Test]
        public void TestNameHColumn()
        {
            var inputSourceViewModel = new InputSourceViewModel();
            inputSourceViewModel.NameHColumn = "1";
            Assert.That(inputSourceViewModel.NameHColumn, Is.EqualTo("1"));
        }

        [Test]
        public void TestInputsList()
        {
            var inputSourceViewModel = new InputSourceViewModel();
            var inputsList = new List<InputSourceList>();
            inputSourceViewModel.InputsList = inputsList;
            Assert.That(inputSourceViewModel.InputsList, Is.EqualTo(inputsList));
        }

        [Test]
        public void TestItems_Selected()
        {
            var deviceManagerMock = new Mock<IDeviceManagerSA>();
            var deviceManagerSA = deviceManagerMock.Object;
            DdpmCommonHelper.DeviceManagerSA = deviceManagerSA;
            var monitorInfo = new MonitorInfo();
            deviceManagerMock.Setup(x => x.SetVCPCapability(It.IsAny<MonitorInfo>(), It.IsAny<string>(), It.IsAny<string>())).Returns(Task.FromResult(true));
            var inputSourceModule = new InputSourceModule();
            inputSourceModule.SelectedHomeDevice.MonitorInfo = monitorInfo;
            inputSourceModule.SelectedHomeDevice.MonitorInfo.CapabilityDic = new Dictionary<string, List<string>>();
            var inputSourceViewModel = new InputSourceViewModel();
            inputSourceViewModel.InputSourceModule = inputSourceModule;
            var items_Selected = new InputSourceList();
            var ModuleOwnerMock = new Mock<IModuleOwner>();
            var moduleOwer = ModuleOwnerMock.Object;
            ModuleOwnerMock.Setup(x => x.HomeDevices).Returns(new List<HomeDevice>());
            inputSourceViewModel.ModuleOwner = moduleOwer;
            inputSourceViewModel.Items_Selected = items_Selected;
            Assert.That(inputSourceViewModel.Items_Selected, Is.EqualTo(items_Selected));
        }

        [Test]
        public void TestInputSourceImage()
        {
            var inputSourceViewModel = new InputSourceViewModel();
            var inputSourceImage = DdpmCommonHelper.GetImageSourceFromCommonResource("Resources/HDMI.png");
            //inputSourceViewModel.InputSourceImage = inputSourceImage;
            //Assert.That(inputSourceViewModel.InputSourceImage, Is.EqualTo(inputSourceImage));
        }

        [Test]
        public void TestIsBusy()
        {
            var inputSourceViewModel = new InputSourceViewModel();
            inputSourceViewModel.IsBusy = true;
            Assert.That(inputSourceViewModel.IsBusy, Is.EqualTo(true));
        }
    }
}