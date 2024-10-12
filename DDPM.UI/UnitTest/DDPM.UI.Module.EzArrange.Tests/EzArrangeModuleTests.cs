using DDPM.UI.Common.Interfaces;
using DDPM.UI.Common;
using DDPM.UI.Plugin.ViewModels;
using Dell.Client.Framework.Common;
using Dell.Client.Framework.UX.WPF;
using NGA.UnitTest.PrivateObject;
using Moq;
using DDPM.UI.Plugin.Common.ViewModels;
using DDPM.SA.Common;
using Microsoft.VisualBasic.Logging;
using DDPM.SA.Common.Interfaces;
using WinRT;
using DDPM.UI.Common.Models;
using DDPM.UI.Common.ViewModels;
using System.Windows.Controls;
using VcpCore.Common;
using DDPM.SA.Common.Settings;

namespace DDPM.UI.Module.EzArrange.Tests
{
    [Apartment(ApartmentState.STA)]
    public class EzArrangeModuleTests
    {
        private EzArrangeModule? ezArrangeModule;
        private PrivateObject? privateObject;
        private Mock<IModuleOwner>? moduleOwnerMock;
        private IModuleOwner? moduleOwner;
        private DisplayViewModel? vmDisplay;
        private IConsole? console;
        private Mock<IConsole>? consoleMock;
        private IEasyArrangeService? easyArrange;
        private Mock<IEasyArrangeService>? easyArrangeMock;
        private IDeviceManagerSA? deviceManagerSA;
        private Mock<IDeviceManagerSA>? deviceManagerSAMock;
        private ILog? log;
        private Mock<ILog>? logMock;


        [SetUp]
        public void Setup()
        {
            moduleOwnerMock = new Mock<IModuleOwner>();
            moduleOwner = moduleOwnerMock!.Object;
            DdpmCommonHelper.ModuleOwner = moduleOwner;
            consoleMock = new Mock<IConsole>();
            console = consoleMock.Object;
            easyArrangeMock = new Mock<IEasyArrangeService>();
            easyArrange = easyArrangeMock.Object;
            deviceManagerSAMock = new Mock<IDeviceManagerSA>();
            deviceManagerSA = deviceManagerSAMock.Object;
            logMock = new Mock<ILog>();
            log = logMock.Object;
            vmDisplay = new DisplayViewModel(console, log, deviceManagerSA, easyArrange);
            HomeDevice.DeviceManagerSA= deviceManagerSAMock.Object;
            deviceManagerSAMock.Setup(x => x.GetEAFunctionEnabled()).Returns(Task.FromResult(new ObjGetVCP() { result = true }));
            ezArrangeModule = new EzArrangeModule(new DisplayViewModel(console, log, deviceManagerSA, easyArrange) { SelectedHomeDevice = new HomeDevice() { MonitorInfo = new MonitorInfo() { DisplayName = "AA" } } });
            privateObject = new PrivateObject(ezArrangeModule);
        }

        [Test]
        public void TestConstructor_EzArrangeModule()
        {
            // Assert
            Assert.That(ezArrangeModule, Is.Not.Null);
        }

        [Test]
        public void TestModuleName()
        {
            // Act
            var result = ezArrangeModule!.ModuleName;

            // Assert
            Assert.That(result, Is.EqualTo("EzArrangeModule"));
        }

        [Test]
        public void TestGetLeftView()
        {
            // Act
            var result = ezArrangeModule!.GetLeftView();

            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        public void TestGetRightView()
        {


            // Act
            var result = ezArrangeModule!.GetRightView();

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.InstanceOf<UserControl>());
        }

        [Test]
        public void TestSelectedHomeDevice()
        {
            // Arrange
            var homeDevice = new HomeDevice();

            // Act
            ezArrangeModule!.SelectedHomeDevice = homeDevice;
            var result = ezArrangeModule.SelectedHomeDevice;

            // Assert
            Assert.That(result, Is.EqualTo(homeDevice));
        }

        [Test]
        public void TestModuleOwner()
        {
            // Arrange
            ezArrangeModule.ModuleOwner = moduleOwner;
            Assert.That(ezArrangeModule.ModuleOwner, Is.EqualTo(moduleOwner));
        }

        [Test]
        public void TestOnSelectedHomeDeviceChanged()
        {
            //IsModuleActive==false
            try
            {
                ezArrangeModule.OnSelectedHomeDeviceChanged();
                Assert.True(true);
                Assert.That(privateObject.GetFieldOrProperty("isSelectChanged"), Is.EqualTo(true));
            }
            catch (Exception ex)
            {
                Assert.Fail("not invoked");
            }
            //IsModuleActive==true
            privateObject.SetFieldOrProperty("IsModuleActive",true);
            try
            {
                ezArrangeModule.OnSelectedHomeDeviceChanged();
                Assert.True(true);
                Assert.That(privateObject.GetFieldOrProperty("isSelectChanged"), Is.EqualTo(false));
            }
            catch (Exception ex)
            {
                Assert.Fail("not invoked");
            }


        }

        [Test]
        public void TestOnActivated()
        {
            privateObject.SetFieldOrProperty("isSelectChanged", true);
            try
            {               
                ezArrangeModule.OnActivated();
                Assert.True(true);
                Assert.That(privateObject.GetFieldOrProperty("isSelectChanged"), Is.EqualTo(false));
            }
            catch (Exception ex)
            {
                Assert.Fail("not invoked");
            }
        }

        [Test]
        public void TestOnDeactivated()
        {
            try
            {
                ezArrangeModule.OnDeactivated();
                Assert.True(true);
            }
            catch (Exception ex)
            {
                Assert.Fail("not invoked");
            }
        }
    }
}