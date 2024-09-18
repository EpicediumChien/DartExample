using DDPM.SA.Common;
using DDPM.UI.Common;
using DDPM.UI.Common.Interfaces;
using DDPM.UI.Common.Models;
using DDPM.UI.Plugin.ViewModels;
using Moq;
using NGA.UnitTest.PrivateObject;
using System;
using Dell.Client.Framework.Common;
using Dell.Client.Framework.UX.WPF;
using System.Windows.Controls;

namespace DDPM.UI.Module.HeadsetAutomatedActions.Tests
{
    [Apartment(ApartmentState.STA)]
    public class HeadsetAutomatedActionsModuleTests
    {
        private HeadsetAutomatedActionsModule? headsetAutomatedActionsModule;
        private HeadsetViewModel? vm;
        private Mock<ILog>? logMock;
        private ILog? log;
        private Mock<IDeviceManagerSA>? deviceManagerSAMock;
        private IDeviceManagerSA? deviceManagerSA;
        private Mock<IConsole>? consoleMock;
        private IConsole? console;
        private PrivateObject? privateObject;
        private Mock<IModuleOwner>? moduleOwnerMock;
        private IModuleOwner? moduleOwner;

        [SetUp]
        public void Setup()
        {
            logMock = new Mock<ILog>();
            log = logMock.Object;
            var deviceManagerSAMock = new Mock<IDeviceManagerSA>();
            deviceManagerSA = deviceManagerSAMock.Object;
            DdpmCommonHelper.DeviceManagerSA = deviceManagerSA;
            consoleMock = new Mock<IConsole>();
            console = consoleMock.Object;
            vm = new HeadsetViewModel(console, log, deviceManagerSA);
            moduleOwnerMock = new Mock<IModuleOwner>();
            moduleOwner = moduleOwnerMock!.Object;
            DdpmCommonHelper.ModuleOwner = moduleOwner;
            var currentDeviceInfo = new DeviceInfo();
            vm.CurrentDeviceInfo = currentDeviceInfo;
            headsetAutomatedActionsModule = new HeadsetAutomatedActionsModule(vm);
            privateObject = new PrivateObject(headsetAutomatedActionsModule);
        }

        [Test]
        public void TestConstructor_HeadsetDeviceSettingsModule()
        {
            // Act
            var result = privateObject.GetFieldOrProperty("_rightView");

            // Assert
            Assert.That(headsetAutomatedActionsModule, Is.Not.Null);
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public void TestIsModuleActive()
        {
            // Act
            headsetAutomatedActionsModule.IsModuleActive = true;

            // Assert
            Assert.That(headsetAutomatedActionsModule.IsModuleActive, Is.EqualTo(true));
        }

        [Test]
        public void TestModuleName()
        {
            // Act
            var result = headsetAutomatedActionsModule!.ModuleName;

            // Assert
            Assert.That(result, Is.EqualTo("HeadsetAudioSettingsModule"));
        }

        [Test]
        public void TestGetLeftView()
        {
            // Act
            var result = headsetAutomatedActionsModule!.GetLeftView();

            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        public void TestGetRightView()
        {
            // Act
            var result = headsetAutomatedActionsModule!.GetRightView();

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
            headsetAutomatedActionsModule!.SelectedHomeDevice = homeDevice;
            var result = headsetAutomatedActionsModule.SelectedHomeDevice;

            // Assert
            Assert.That(result, Is.EqualTo(homeDevice));
        }

        [Test]
        public void TestModuleOwner()
        {
            headsetAutomatedActionsModule.ModuleOwner = moduleOwner;
            // Arrange
            Assert.That(headsetAutomatedActionsModule.ModuleOwner, Is.EqualTo(moduleOwner));
        }

        [Test]
        public void TestOnSelectedHomeDeviceChanged()
        {
            try
            {
                headsetAutomatedActionsModule.OnSelectedHomeDeviceChanged();
                Assert.True(true);
            }
            catch (Exception ex)
            {
                Assert.Fail("not invoked");
            }
        }

        [Test]
        public void TestOnActivated()
        {
            try
            {
                headsetAutomatedActionsModule.OnActivated();
                Assert.True(true);
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
                headsetAutomatedActionsModule.OnDeactivated();
                Assert.True(true);
            }
            catch (Exception ex)
            {
                Assert.Fail("not invoked");
            }
        }
    }
}