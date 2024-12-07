using DDPM.SA.Common;
using DDPM.SA.Common.Display;
using DDPM.SA.Common.Settings;
using DDPM.UI.Common;
using DDPM.UI.Common.Interfaces;
using DDPM.UI.Common.Models;
using Moq;
using NGA.UnitTest.PrivateObject;
using System.Windows;
using System.Windows.Controls;
using VcpCore.Common;
using Dell.Client.Framework.UX.WPF.ResourceManager;

namespace DDPM.UI.Module.DisplayHotkeys.Tests
{
    [TestFixture, Apartment(ApartmentState.STA)]
    public class DisplayHotkeysModuleTests
    {
        private DisplayHotkeysModule? displayHotkeysModule;
        private PrivateObject? privateObject;
        private Mock<IModuleOwner>? moduleOwnerMock;
        private IModuleOwner? moduleOwner;
        private Mock<IDeviceManagerSA>? deviceManagerMock;

         

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
            deviceManagerMock = new Mock<IDeviceManagerSA>();
            DdpmCommonHelper.DeviceManagerSA = deviceManagerMock.Object;
            deviceManagerMock.Setup(x => x.ReadCurrentHotkey(It.IsAny<MonitorInfo>())).Returns(Task.FromResult(new ValueTuple<HotkeySettings, List<HotkeyData>>()));
            moduleOwnerMock = new Mock<IModuleOwner>();
            moduleOwner = moduleOwnerMock!.Object;
            DdpmCommonHelper.ModuleOwner = moduleOwner;
            moduleOwnerMock.Setup(x => x.SelectedHomeDevice).Returns(new HomeDevice());
            DdpmCommonHelper.ModuleOwner.SelectedHomeDevice.MonitorInfo=new VcpCore.Common.MonitorInfo();
            DdpmCommonHelper.ModuleOwner.SelectedHomeDevice.MonitorInfo.CapabilityDic = new Dictionary<string, List<string>>() { { "JE", new List<string>() { "E9" } } };
            displayHotkeysModule = new DisplayHotkeysModule();
            privateObject = new PrivateObject(displayHotkeysModule);

        }

        [Test]
        public void TestConstructor_DisplayHotkeysModule()
        {

            // Assert
            Assert.That(displayHotkeysModule, Is.Not.Null);
        }

        [Test]
        [Apartment(ApartmentState.STA)]
        public void TestViewModelInitialization()
        {
            // Act        
            var viewModel = privateObject!.GetFieldOrProperty("vm");

            // Assert
            Assert.That(viewModel, Is.Not.Null);
            Assert.That(viewModel, Is.InstanceOf<DisplayHotkeysViewModel>());
        }

        [Test]
        public void TestModuleName()
        {
            // Act
            var result = displayHotkeysModule!.ModuleName;

            // Assert
            Assert.That(result, Is.EqualTo("DisplayHotkeysModule"));
        }

        [Test]//Test method
        public void TestSelectedHomeDevice()
        {

            moduleOwnerMock.Setup(x => x.SelectedHomeDevice).Returns(new HomeDevice());
            var slectedHomeDevice = new HomeDevice();
            displayHotkeysModule.SelectedHomeDevice = slectedHomeDevice;
            Assert.That(displayHotkeysModule!.SelectedHomeDevice, Is.EqualTo(moduleOwner.SelectedHomeDevice));

            DdpmCommonHelper.ModuleOwner = null;
            displayHotkeysModule.SelectedHomeDevice = slectedHomeDevice;
            Assert.That(displayHotkeysModule!.SelectedHomeDevice, Is.EqualTo(null));
        }

        [Test]
        public void TestGetLeftView()
        {
            var mockHomeDevice = new Mock<HomeDevice>();
            var displayHotkeysModule = new DisplayHotkeysModule(/*mockHomeDevice.Object*/);

            //Act:Call the methods to test
            var leftView = displayHotkeysModule.GetLeftView();

            Assert.That(leftView, Is.Null);
        }

        [Test]
        public void TestGetRightView()
        {
            var mockHomeDevice = new Mock<HomeDevice>();
            var displayHotkeysModule = new DisplayHotkeysModule(/*mockHomeDevice.Object*/);

            var rightView = displayHotkeysModule.GetRightView();

            Assert.That(rightView, Is.InstanceOf<UserControl>());
        }

        [Test]
        public void TestModuleOwner()
        {
            displayHotkeysModule.ModuleOwner = moduleOwner;

            Assert.That(displayHotkeysModule.ModuleOwner, Is.EqualTo(moduleOwner));
        }

        [Test]
        [Apartment(ApartmentState.STA)]
        public void TestOnSelectedHomeDeviceChanged()
        {
            var displayHotkeysModule = new DisplayHotkeysModule();
            displayHotkeysModule.OnSelectedHomeDeviceChanged();
            Assert.Pass();
        }

        [Test]
        [Apartment(ApartmentState.STA)]
        public void TestOnActivated()
        {
            var displayHotkeysModule = new DisplayHotkeysModule();
            displayHotkeysModule.OnActivated();
            Assert.Pass();
        }

        [Test]
        [Apartment(ApartmentState.STA)]
        public void TestOnDeactivated()
        {
            var displayHotkeysModule = new DisplayHotkeysModule();
            displayHotkeysModule.OnDeactivated();
            Assert.Pass();
        }
    }
}