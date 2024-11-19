using DDPM.SA.Common;
using DDPM.SA.Common.Display;
using DDPM.SA.Common.Settings;
using DDPM.UI.Common;
using DDPM.UI.Common.Interfaces;
using DDPM.UI.Common.Models;
using Moq;
using System.Collections.ObjectModel;
using System.Windows;
using VcpCore.Common;
using EDID = VcpCore.Common.EDID;

namespace DDPM.UI.Module.DisplayHotkeys.Tests
{
    [TestFixture, Apartment(ApartmentState.STA)]
    public class DisplayHotkeysViewModelTests
    {
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
        }

        [Test]
        public void TestInputdisplayHotkeysModule()
        {
            var moduleOwnerMock = new Mock<IModuleOwner>();
            var moduleOwner = moduleOwnerMock!.Object;
            DdpmCommonHelper.ModuleOwner = moduleOwner;
            moduleOwnerMock.Setup(x => x.SelectedHomeDevice).Returns(new HomeDevice());
            DdpmCommonHelper.ModuleOwner.SelectedHomeDevice.MonitorInfo = new VcpCore.Common.MonitorInfo();
            DdpmCommonHelper.ModuleOwner.SelectedHomeDevice.MonitorInfo.CapabilityDic = new Dictionary<string, List<string>>() { { "JE", new List<string>() { "E9" } } };
            var inputSourceList = new InputSourceList();
            var displayHotkeysModule = new DisplayHotkeysModule();
            inputSourceList.displayHotkeysModule = displayHotkeysModule;
            Assert.That(inputSourceList.displayHotkeysModule, Is.EqualTo(displayHotkeysModule));
        }

        [Test]
        public void TestinputDisplayText()
        {
            var inputSourceList = new InputSourceList();
            Assert.That(inputSourceList.inputDisplayText, Is.EqualTo(inputSourceList.inputSource));
        }

        [Test]
        public void TestModuleOwner()
        {
            var mockModuleOwner = new Mock<IModuleOwner>();
            var displayHotkeysViewModel = new DisplayHotkeysViewModel();
            displayHotkeysViewModel.ModuleOwner = mockModuleOwner.Object;
            Assert.That(displayHotkeysViewModel.ModuleOwner, Is.EqualTo(mockModuleOwner.Object));
        }

        [Test]
        public void TestdisplayHotkeysModule()
        {
            var moduleOwnerMock = new Mock<IModuleOwner>();
            var moduleOwner = moduleOwnerMock!.Object;
            DdpmCommonHelper.ModuleOwner = moduleOwner;
            moduleOwnerMock.Setup(x => x.SelectedHomeDevice).Returns(new HomeDevice());
            DdpmCommonHelper.ModuleOwner.SelectedHomeDevice.MonitorInfo = new VcpCore.Common.MonitorInfo();
            DdpmCommonHelper.ModuleOwner.SelectedHomeDevice.MonitorInfo.CapabilityDic = new Dictionary<string, List<string>>() { { "JE", new List<string>() { "E9" } } };
            var deviceManagerMock = new Mock<IDeviceManagerSA>();
            var deviceManagerSA = deviceManagerMock.Object;
            DdpmCommonHelper.DeviceManagerSA = deviceManagerSA;
            deviceManagerMock.Setup(x => x.GetInputSourcelist(It.IsAny<MonitorInfo>())).Returns(Task.FromResult(new Dictionary<string, InputInfo>()));
            var monitorInfo = new MonitorInfo();
            var displayHotkeysModule = new DisplayHotkeysModule();
            displayHotkeysModule.SelectedHomeDevice.MonitorInfo = monitorInfo;
            var displayHotkeysViewModel = new DisplayHotkeysViewModel();
            displayHotkeysViewModel.DisplayHotkeysModule = displayHotkeysModule;
            Assert.That(displayHotkeysViewModel.DisplayHotkeysModule, Is.EqualTo(displayHotkeysModule));
        }

        [Test]
        public void TestinputList()
        {
            var displayHotkeysViewModel = new DisplayHotkeysViewModel();
            var inputList = new Dictionary<string, InputInfo>();
            displayHotkeysViewModel.inputList = inputList;
            Assert.That(displayHotkeysViewModel.inputList, Is.EqualTo(inputList));
        }

        [Test]
        public void TestToggleInputSourceKey()
        {
            var displayHotkeysViewModel = new DisplayHotkeysViewModel();
            var toggleInputSourceKey = "Yes";
            displayHotkeysViewModel.ToggleInputSourceKey = toggleInputSourceKey;
            Assert.That(displayHotkeysViewModel.ToggleInputSourceKey, Is.EqualTo(toggleInputSourceKey));
        }

        [Test]
        public void TestFavoriteInputSourceKey()
        {
            var displayHotkeysViewModel = new DisplayHotkeysViewModel();
            var favoriteInputSourceKey = "Yes";
            displayHotkeysViewModel.FavoriteInputSourceKey = favoriteInputSourceKey;
            Assert.That(displayHotkeysViewModel.FavoriteInputSourceKey, Is.EqualTo(favoriteInputSourceKey));
        }

        [Test]
        public void TestSwitchInputSourceKey()
        {
            var displayHotkeysViewModel = new DisplayHotkeysViewModel();
            var switchInputSourceKey = "Yes";
            displayHotkeysViewModel.SwitchInputSourceKey = switchInputSourceKey;
            Assert.That(displayHotkeysViewModel.SwitchInputSourceKey, Is.EqualTo(switchInputSourceKey));
        }

        [Test]
        public void TestChangePIPPositionKey()
        {
            var displayHotkeysViewModel = new DisplayHotkeysViewModel();
            var changePIPPositionKey = "Yes";
            displayHotkeysViewModel.ChangePIPPositionKey = changePIPPositionKey;
            Assert.That(displayHotkeysViewModel.ChangePIPPositionKey, Is.EqualTo(changePIPPositionKey));
        }

        [Test]
        public void TestSwapPIPPBPInputSourceKey()
        {
            var displayHotkeysViewModel = new DisplayHotkeysViewModel();
            var swapPIPPBPInputSourceKey = "Yes";
            displayHotkeysViewModel.SwapPIPPBPInputSourceKey = swapPIPPBPInputSourceKey;
            Assert.That(displayHotkeysViewModel.SwapPIPPBPInputSourceKey, Is.EqualTo(swapPIPPBPInputSourceKey));
        }

        [Test]
        public void TestInputsList()
        {
            var displayHotkeysViewModel = new DisplayHotkeysViewModel();
            var inputsList = new ObservableCollection<InputSourceList>();
            displayHotkeysViewModel.InputsList = inputsList;
            Assert.That(displayHotkeysViewModel.InputsList, Is.EqualTo(inputsList));
        }

        [Test]
        public void TestSwitchInput1_Selected()
        {
            var displayHotkeysViewModel = new DisplayHotkeysViewModel();
            var SwitchInput1_Selected = new InputSourceList();
            displayHotkeysViewModel.SwitchInput1_Selected = SwitchInput1_Selected;
            Assert.That(displayHotkeysViewModel.SwitchInput1_Selected, Is.EqualTo(SwitchInput1_Selected));
        }

        [Test]
        public void TestSwitchInput2_Selected()
        {
            var displayHotkeysViewModel = new DisplayHotkeysViewModel();
            var switchInput2_Selected = new InputSourceList();
            displayHotkeysViewModel.SwitchInput2_Selected = switchInput2_Selected;
            Assert.That(displayHotkeysViewModel.SwitchInput2_Selected, Is.EqualTo(switchInput2_Selected));
        }

        [Test]
        public void TestFavoriteInput_Selected()
        {
            var displayHotkeysViewModel = new DisplayHotkeysViewModel();
            var favoriteInput_Selected = new InputSourceList();
            displayHotkeysViewModel.FavoriteInput_Selected = favoriteInput_Selected;
            Assert.That(displayHotkeysViewModel.FavoriteInput_Selected, Is.EqualTo(favoriteInput_Selected));
        }
    }
}