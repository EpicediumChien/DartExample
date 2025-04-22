using DDPM.SA.Common;
using DDPM.UI.Common.Interfaces;
using DDPM.UI.Common;
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
using static System.Net.Mime.MediaTypeNames;
using System.Globalization;

namespace DDPM.UI.Module.EzSettings.Tests
{
    [TestFixture, Apartment(ApartmentState.STA)]
    public class EzSettingsRightViewTests
    {
        private EzSettingsRightView? ezSettingsRightView;
        private PrivateObject? privateObject;
        private Mock<IModuleOwner>? moduleOwnerMock;
        private Mock<IDeviceManagerSA>? deviceManagerMock;
        private IDeviceManagerSA? deviceManager;
        private Mock<IConsole>? consoleMock;
        private IConsole? console;
        private Mock<ILog>? logMock;
        private ILog? log;
        private EzSettingsViewModel? vm;

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
            vm = new EzSettingsViewModel(moduleOwnerMock.Object);
            ezSettingsRightView = new EzSettingsRightView(vm);
            privateObject = new PrivateObject(ezSettingsRightView);
        }

        [Test]
        public void TestConstructor_EzSettingsRightView()
        {
            if (CultureInfo.CurrentCulture.Name == "es-US")
            {
                var hotkeyRecentTooltipText = privateObject.GetFieldOrProperty("RecentHotkeyTooltipText");
                var applicationWindowSnapTooltipText = privateObject.GetFieldOrProperty("ApplicationWindowSnapTooltipText");

                // Assert
                Assert.That(ezSettingsRightView, Is.Not.Null);
                Assert.That(ezSettingsRightView.DataContext, Is.EqualTo(vm));
                Assert.That(privateObject!.GetFieldOrProperty("_viewModel"), Is.EqualTo(vm));
                Assert.That(hotkeyRecentTooltipText, Is.EqualTo("Example: \"Alt + P\", \"Shift + F\", \"Ctrl + Shift + F\"."));
                Assert.That(applicationWindowSnapTooltipText, Is.EqualTo("Snap any application into a split screen layout easily by dragging into a partition"));
            }
        }
    }
}
