using DDPM.UI.Common.Interfaces;
using DDPM.UI.Common.Models;
using DDPM.UI.Common;
using NGA.UnitTest.PrivateObject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Moq;
using DDPM.SA.Common;
using VcpCore.Common;
using System.Collections.ObjectModel;
using Dell.Client.Framework.UX.WPF.ResourceManager;
using System.Globalization;
namespace DDPM.UI.Module.GamingVisionEngine.Tests
{
    [Apartment(ApartmentState.STA)]
    public class VisionEngineViewModelTests
    {
        private VisionEngineViewModel? visionEngineViewModel;
        private Mock<IModuleOwner>? moduleOwnerMock;
        private IModuleOwner? moduleOwner;
        private Mock<IDeviceManagerSA>? deviceManagerMock;
        private IDeviceManagerSA? deviceManagerSA;
        private HomeDevice? selectedHomeDevice;
        private PrivateObject? privateObject;
        private VisionEngineModule? myModule;


        [SetUp]
        public void Setup()
        {
            if (System.Windows.Application.Current == null)
            {
                new System.Windows.Application();
            }
            ResourceManager res = new ResourceManager();
            var resourceDictionary = new ResourceDictionary();
            resourceDictionary.Source = new Uri("pack://application:,,,/DDPM.UI.Common;component/ModuleStyle.xaml");
            System.Windows.Application.Current.Resources.MergedDictionaries.Add(resourceDictionary);
            moduleOwnerMock = new Mock<IModuleOwner>();
            moduleOwnerMock.Setup(m => m.SelectedHomeDevice).Returns(new HomeDevice() { MonitorInfo = new MonitorInfo() });
            moduleOwner = moduleOwnerMock!.Object;
            DdpmCommonHelper.ModuleOwner = moduleOwner;

            deviceManagerMock = new Mock<IDeviceManagerSA>();
            deviceManagerSA = deviceManagerMock.Object;
            deviceManagerMock.Setup(x => x.GetGamingProperties_SupportedList(It.IsAny<MonitorInfo>())).Returns(Task.FromResult(new GamingDisplayPropertiesInfo()));
            DdpmCommonHelper.DeviceManagerSA = deviceManagerSA;

            myModule = new VisionEngineModule();
            selectedHomeDevice = new HomeDevice();
            visionEngineViewModel = new VisionEngineViewModel();
            visionEngineViewModel.MyModule = myModule;
            visionEngineViewModel.MyModule.SelectedHomeDevice = selectedHomeDevice;
            visionEngineViewModel.MyModule.SelectedHomeDevice.MonitorInfo = new MonitorInfo() { modelName = "gx" };
            privateObject = new PrivateObject(visionEngineViewModel);
        }

        [Test]
        public void TestModuleOwner()
        {
            // Arrange
            visionEngineViewModel.ModuleOwner = moduleOwner;
            Assert.That(visionEngineViewModel.ModuleOwner, Is.EqualTo(moduleOwner));
        }

        [Test]
        public void TestMyModule()
        {
            visionEngineViewModel.MyModule = myModule;
            Assert.That(visionEngineViewModel.MyModule, Is.EqualTo(myModule));
        }

        [Test]
        public void TesVisionEngineList()
        {
            var visionEngineList = new ObservableCollection<UI_VisionEngine>();
            visionEngineViewModel.VisionEngineList = visionEngineList;
            Assert.That(visionEngineViewModel.VisionEngineList, Is.EqualTo(visionEngineList));
        }

        [Test]
        public void TestVisionEngineIsEnable()
        {
            visionEngineViewModel.VisionEngineIsEnable = false;
            Assert.That(visionEngineViewModel.VisionEngineIsEnable, Is.EqualTo(false));
        }

        [Test]
        public void TestIsBusy()
        {
            visionEngineViewModel.IsBusy = false;
            Assert.That(visionEngineViewModel.IsBusy, Is.EqualTo(false));
        }

        [Test]
        public void TestGamingParamChang()
        {
            visionEngineViewModel.VisionEngineList = new ObservableCollection<UI_VisionEngine>() {new UI_VisionEngine(true,new Gaming_VisionEngineType()),new UI_VisionEngine(true,new Gaming_VisionEngineType()) };
            var e = new GamingDisplayPropertiesInfo() { IsEnable_VisionEngineType = new bool[] { true, false } };
            try
            {
                visionEngineViewModel.GamingParamChang(null, e);
                Assert.True(true);
            }
            catch (Exception ex)
            {
                Assert.Fail("not invoked");
            }
        }

        [Test]
        public void TestVisionEngineToggleKey()
        {
            visionEngineViewModel.VisionEngineToggleKey = "VisionEngineToggleKey";
            Assert.That(visionEngineViewModel.VisionEngineToggleKey, Is.EqualTo("VisionEngineToggleKey"));
        }

        [Test]
        public void TestRefreshUI()
        {
            var VisionEngineListbef = visionEngineViewModel.VisionEngineList;
            //var VisionEngineList_UIbef = visionEngineViewModel.VisionEngineList;
            var VisionEngineIsEnablebef = visionEngineViewModel.VisionEngineIsEnable;

            visionEngineViewModel.VisionEngineList = new ObservableCollection<UI_VisionEngine>() { new UI_VisionEngine(true, new Gaming_VisionEngineType()) };
            visionEngineViewModel.VisionEngineIsEnable = false;
            visionEngineViewModel.RefreshUI();

            var VisionEngineListaft = visionEngineViewModel.VisionEngineList;
            var VisionEngineIsEnableaft = visionEngineViewModel.VisionEngineIsEnable;

            Assert.That(VisionEngineListbef, Is.Not.EqualTo(VisionEngineListaft));
            Assert.That(VisionEngineIsEnablebef, Is.Not.EqualTo(VisionEngineIsEnableaft));
        }


        //class UI_VisionEngine
        [Test]
        public void TestVisionEngine_Enable()
        {
            var uI_VisionEngine = new UI_VisionEngine(true, new Gaming_VisionEngineType());
            uI_VisionEngine.VisionEngine_Enable = true;
            Assert.That(uI_VisionEngine.VisionEngine_Enable, Is.EqualTo(true));
        }

        [Test]
        public void TestVisionEngineType()
        {
            var uI_VisionEngine = new UI_VisionEngine(true, new Gaming_VisionEngineType());
            var visionEngineType = new Gaming_VisionEngineType();
            uI_VisionEngine.VisionEngineType = visionEngineType;
            Assert.That(uI_VisionEngine.VisionEngineType, Is.EqualTo(visionEngineType));
        }

        [Test]
        public void TestDisplayText()
        {
            if (CultureInfo.CurrentCulture.Name == "es-US")
            {
                var uI_VisionEngine = new UI_VisionEngine(true, new Gaming_VisionEngineType());
                var visionEngineType = new Gaming_VisionEngineType();
                uI_VisionEngine.VisionEngineType = visionEngineType;
                var result = uI_VisionEngine.DisplayText;
                Assert.That(uI_VisionEngine.DisplayText, Is.EqualTo("OFF"));
            }
        }
    }
}
