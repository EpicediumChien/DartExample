using DDPM.SA.Common;
using DDPM.UI.Common.Interfaces;
using DDPM.UI.Common;
using DDPM.UI.Plugin.ViewModels;
using Dell.Client.Framework.Common;
using Dell.Client.Framework.UX.WPF;
using Moq;
using NGA.UnitTest.PrivateObject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Dell.Client.Framework.UX.WPF.ResourceManager;
using System.Globalization;

namespace DDPM.UI.Module.MouseSettings.Tests
{
    [TestFixture, Apartment(ApartmentState.STA)]
    public class MouseSettingsRightViewTests
    {
        private MouseSettingsRightView? mouseSettingsRightView;
        private PrivateObject? privateObject;
        private Mock<IModuleOwner>? moduleOwnerMock;
        private Mock<IDeviceManagerSA>? deviceManagerMock;
        private IDeviceManagerSA? deviceManager;
        private Mock<IConsole>? consoleMock;
        private IConsole? console;
        private Mock<ILog>? logMock;
        private ILog? log;
        private MouseViewModel? vm;

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
            var moduleOwner = moduleOwnerMock!.Object;
            DdpmCommonHelper.ModuleOwner = moduleOwner;
            deviceManagerMock = new Mock<IDeviceManagerSA>();
            deviceManager = deviceManagerMock.Object;
            DdpmCommonHelper.DeviceManagerSA = deviceManager;
            consoleMock = new Mock<IConsole>();
            console = consoleMock.Object;
            DdpmCommonHelper.MyConsole = console;
            logMock = new Mock<ILog>();
            log = logMock.Object;
            vm = new MouseViewModel(console, log);
            mouseSettingsRightView = new MouseSettingsRightView(vm);
            privateObject = new PrivateObject(mouseSettingsRightView);
        }

        [Test]
        public void TestConstructor_MouseSettingsRightView()
        {
            if (CultureInfo.CurrentCulture.Name == "es-US")
            {
                var txtDPIMessageText = Strings.DPIMessage;
                var txtPollingRateMessageText = Strings.PollingRateMessage;

                // Assert
                Assert.That(mouseSettingsRightView, Is.Not.Null);
                Assert.That(privateObject!.GetFieldOrProperty("_vm"), Is.EqualTo(vm));
                Assert.That(txtDPIMessageText, Is.EqualTo("Move your mouse to complete the change to the DPI value"));
                Assert.That(txtPollingRateMessageText, Is.EqualTo("Increasing polling rate may affect mouse’s battery life."));
            }
        }
    }
}
