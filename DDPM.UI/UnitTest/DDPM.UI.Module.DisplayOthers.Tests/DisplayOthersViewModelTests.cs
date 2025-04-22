using DDPM.SA.Common;
using DDPM.SA.Common.Display;
using DDPM.UI.Common;
using DDPM.UI.Common.Interfaces;
using DDPM.UI.Common.Models;
using Dell.Client.Framework.UX.WPF.ResourceManager;
using Moq;
using NGA.UnitTest.PrivateObject;
using NUnit.Framework;
using System.Globalization;
using System.Windows;
using VcpCore.Common;

namespace DDPM.UI.Module.DisplayOthers.Tests
{
    [TestFixture, Apartment(ApartmentState.STA)]
    public class DisplayOthersViewModelTests
    {
        private DisplayOthersViewModel? viewModel;
        private PrivateObject? privateObject;
        private Mock<IDeviceManagerSA>? deviceManagerMock;
        private Mock<IModuleOwner>? moduleOwnerMock;
        private IModuleOwner? moduleOwner;

        [SetUp]
        public void SetUp()
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
            viewModel = new DisplayOthersViewModel();
            privateObject = new PrivateObject(viewModel);
            moduleOwnerMock = new Mock<IModuleOwner>();
            moduleOwner = moduleOwnerMock.Object;
        }

        [Test]
        public void TestModuleOwner()
        {
            // Arrange
            var expectedModuleOwner = moduleOwnerMock!.Object;

            // Act
            privateObject!.SetProperty("ModuleOwner", expectedModuleOwner);
            var result = privateObject.GetProperty("ModuleOwner");

            // Assert
            Assert.That(result, Is.EqualTo(expectedModuleOwner));
        }

        [Test]
        public void TestDisplayOthersModule()
        {
            DdpmCommonHelper.ModuleOwner = moduleOwnerMock!.Object;
            moduleOwnerMock.Setup(x => x.SelectedHomeDevice).Returns(new HomeDevice());
            var displayOthersModule = new DisplayOthersModule();
            viewModel.DisplayOthersModule = displayOthersModule;
            var result = viewModel.DisplayOthersModule;

            // Assert
            Assert.That(result, Is.EqualTo(displayOthersModule));
        }

        [Test]
        public void TestPowerNap_text()
        {
            viewModel.PowerNap_text = "A";
            var result = viewModel.PowerNap_text;

            // Assert
            Assert.That(result, Is.EqualTo("A"));
        }

        [Test]
        public void TestPowerNap_Enable()
        {
            DdpmCommonHelper.ModuleOwner = moduleOwnerMock!.Object;
            moduleOwnerMock.Setup(x => x.SelectedHomeDevice).Returns(new HomeDevice());

            var DeviceManagerSAMock = new Mock<IDeviceManagerSA>();
            var deviceManagerSA = DeviceManagerSAMock.Object;
            DeviceManagerSAMock.Setup(x => x.ReadPowerNapSettings()).Returns(Task.FromResult(new List<PowerNapSetting>()));

            var displayOthersModule = new DisplayOthersModule();
            displayOthersModule.SelectedHomeDevice = new HomeDevice();
            displayOthersModule.SelectedHomeDevice.MonitorInfo = new MonitorInfo();
            displayOthersModule.SelectedHomeDevice.MonitorInfo.edid = new VcpCore.Common.EDID();
            viewModel.DisplayOthersModule = displayOthersModule;
            DdpmCommonHelper.DeviceManagerSA = deviceManagerSA;

            viewModel.PowerNap_Enable = true;
            var result = viewModel.PowerNap_Enable;

            // Assert
            Assert.That(result, Is.EqualTo(true));
            if (CultureInfo.CurrentCulture.Name == "es-US")
            {
                Assert.That(viewModel.PowerNap_text, Is.EqualTo("ON"));
            }
        }

        [Test]
        public void TestReducebrt_Checked()
        {
            var DeviceManagerSAMock = new Mock<IDeviceManagerSA>();
            var DeviceManagerSA = DeviceManagerSAMock.Object;
            DdpmCommonHelper.DeviceManagerSA = DeviceManagerSA;
            DdpmCommonHelper.ModuleOwner = moduleOwner;
            moduleOwnerMock.Setup(x => x.SelectedHomeDevice).Returns(new HomeDevice());
            var DisplayOthersModule = new DisplayOthersModule();
            viewModel.DisplayOthersModule = DisplayOthersModule;
            viewModel.DisplayOthersModule.SelectedHomeDevice = new HomeDevice();
            viewModel.DisplayOthersModule.SelectedHomeDevice.MonitorInfo = new MonitorInfo();
            viewModel.DisplayOthersModule.SelectedHomeDevice.MonitorInfo.edid = new VcpCore.Common.EDID() { SerialNumber = "aaa" };
            DeviceManagerSAMock.Setup(x => x.ReadPowerNapSettings()).Returns(Task.FromResult(new List<PowerNapSetting>()));
            viewModel.Reducebrt_Checked = true;
            var result = viewModel.Reducebrt_Checked;

            // Assert
            Assert.That(result, Is.EqualTo(true));
        }

        [Test]
        public void TestPutTosleep_Checked()
        {
            DdpmCommonHelper.ModuleOwner = moduleOwnerMock!.Object;
            moduleOwnerMock.Setup(x => x.SelectedHomeDevice).Returns(new HomeDevice());

            var displayOthersModule = new DisplayOthersModule();
            displayOthersModule.SelectedHomeDevice = new HomeDevice();
            displayOthersModule.SelectedHomeDevice.MonitorInfo = new MonitorInfo();
            displayOthersModule.SelectedHomeDevice.MonitorInfo.edid = new VcpCore.Common.EDID();
            viewModel.DisplayOthersModule = displayOthersModule;
            var DeviceManagerSAMock = new Mock<IDeviceManagerSA>();
            var deviceManagerSA = DeviceManagerSAMock.Object;
            DdpmCommonHelper.DeviceManagerSA = deviceManagerSA;
            DeviceManagerSAMock.Setup(x => x.ReadPowerNapSettings()).Returns(Task.FromResult(new List<PowerNapSetting>()));
            DeviceManagerSAMock.Setup(x => x.SavePowerNapSetting(It.IsAny<PowerNapSetting>())).Returns(Task.FromResult(true));
            viewModel.PutTosleep_Checked = true;
            var result = viewModel.PutTosleep_Checked;

            // Assert
            Assert.That(result, Is.EqualTo(true));
        }

        //[Test]
        //public void TestAutoApply_Checked()
        //{
        //    DdpmCommonHelper.ModuleOwner = moduleOwnerMock!.Object;
        //    moduleOwnerMock.Setup(x => x.SelectedHomeDevice).Returns(new HomeDevice());

        //    var displayOthersModule = new DisplayOthersModule();
        //    displayOthersModule.SelectedHomeDevice = new HomeDevice();
        //    displayOthersModule.SelectedHomeDevice.MonitorInfo = new MonitorInfo();
        //    displayOthersModule.SelectedHomeDevice.MonitorInfo.edid = new VcpCore.Common.EDID();
        //    viewModel.DisplayOthersModule = displayOthersModule;
        //    var DeviceManagerSAMock = new Mock<IDeviceManagerSA>();
        //    var deviceManagerSA = DeviceManagerSAMock.Object;
        //    DdpmCommonHelper.DeviceManagerSA = deviceManagerSA;
        //    DeviceManagerSAMock.Setup(x => x.ReadPowerNapSettings()).Returns(Task.FromResult(new List<PowerNapSetting>()));
        //    DeviceManagerSAMock.Setup(x => x.SavePowerNapSetting(It.IsAny<PowerNapSetting>())).Returns(Task.FromResult(true));
        //    viewModel.AutoApply_Checked = true;
        //    var result = viewModel.AutoApply_Checked;

        //    // Assert
        //    Assert.That(result, Is.EqualTo(true));
        //}
    }
}