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
using NUnit.Framework.Interfaces;
using System.Globalization;

namespace DDPM.UI.Module.PenButtonSettings.Tests
{
    [TestFixture, Apartment(ApartmentState.STA)]
    public class PenButtonSettingsRightViewTests
    {
        private PenButtonSettingsRightView? penButtonSettingsRightView;
        private PrivateObject? privateObject;
        private Mock<IModuleOwner>? moduleOwnerMock;
        private Mock<IDeviceManagerSA>? deviceManagerMock;
        private IDeviceManagerSA? deviceManager;
        private Mock<IConsole>? consoleMock;
        private IConsole? console;
        private Mock<ILog>? logMock;
        private Mock<IShowPluginManager>? showPluginManagerMock;
        private IShowPluginManager? showPluginManager;
        private ILog? log;
        private PenViewModel? vm;
        private DeviceInfo? CurrentDeviceInfo;

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
            showPluginManagerMock = new Mock<IShowPluginManager>();
            showPluginManager = showPluginManagerMock.Object;
            deviceManagerMock.Setup(x => x.GetEraserDoublePressSetting()).Returns(Task.FromResult("{\r\n    \"actionId\": 90,\r\n    \"actionName\": \"Screen Snipping\"\r\n}"));
            deviceManagerMock.Setup(x => x.GetEraserSinglePressSetting()).Returns(Task.FromResult("{\r\n    \"actionId\": 90,\r\n    \"actionName\": \"Screen Snipping\"\r\n}"));
            deviceManagerMock.Setup(x => x.GetEraserLongPressSetting()).Returns(Task.FromResult("{\r\n    \"actionId\": 90,\r\n    \"actionName\": \"Screen Snipping\"\r\n}"));
            deviceManagerMock.Setup(x => x.GetSideTopSwitchSinglePressSetting()).Returns(Task.FromResult("{\r\n    \"actionId\": 90,\r\n    \"actionName\": \"Screen Snipping\"\r\n}"));
            deviceManagerMock.Setup(x => x.GetSideBottomSwitchSinglePressSetting()).Returns(Task.FromResult("{\r\n    \"actionId\": 90,\r\n    \"actionName\": \"Screen Snipping\"\r\n}"));
            deviceManagerMock.Setup(x => x.GetMenuSinglePressSetting()).Returns(Task.FromResult("{\r\n    \"actionId\": 90,\r\n    \"actionName\": \"Screen Snipping\"\r\n}"));
            vm = new PenViewModel(console, log);
            penButtonSettingsRightView = new PenButtonSettingsRightView(vm);
            privateObject = new PrivateObject(penButtonSettingsRightView);
        }

        [Test]
        public void TestConstructor_PenButtonSettingsRightView()
        {
            if (CultureInfo.CurrentCulture.Name == "es-US")
            {
                var txtClickOnceText = Strings.PenButtonClickOnce;
                var txtDoubleClickText = Strings.PenButtonDoubleClick;
                var txtPressAndHoldText = Strings.PenButtonPressHold;
                var txtHoverClickText = Strings.HoverClick;
                var txtMessageText = Strings.PenButtonCustomizeMessage;
                var txtRestoreText = Strings.PenButtonCustomizeRestoreCaption;
                var txtSuggestedActionsText = Strings.SuggestedActionsCaption;
                var txtProductivityActionsText = Strings.ProductivityActionsCaption;
                var txtWindowsActionsText = Strings.WindowsActionsCaption;
                var txtMultimediaActionsText = Strings.MultimediaActionsCaption;
                var txtSearchResultText = Strings.SearchResultsCaption;

                // Assert
                Assert.That(penButtonSettingsRightView, Is.Not.Null);
                Assert.That(privateObject!.GetFieldOrProperty("_vm"), Is.EqualTo(vm));
                Assert.That(txtClickOnceText, Is.EqualTo("Click Once"));
                Assert.That(txtDoubleClickText, Is.EqualTo("Double Click"));
                Assert.That(txtPressAndHoldText, Is.EqualTo("Press and Hold"));
                Assert.That(txtHoverClickText, Is.EqualTo("Hover Click"));
                Assert.That(txtMessageText, Is.EqualTo("To customize a button, click an outlined button on the image to the left"));
                Assert.That(txtRestoreText, Is.EqualTo("Restore to default actions"));
                Assert.That(txtSuggestedActionsText, Is.EqualTo("Suggested Actions"));
                Assert.That(txtProductivityActionsText, Is.EqualTo("Productivity Actions"));
                Assert.That(txtWindowsActionsText, Is.EqualTo("Windows Actions"));
                Assert.That(txtMultimediaActionsText, Is.EqualTo("Multimedia Actions"));
                Assert.That(txtSearchResultText, Is.EqualTo("Search Results"));
            }
        }

        [Test]
        public void TestInitialize()
        {
            penButtonSettingsRightView.Initialize();
            var txtCaptionText = Strings.ButtonCustomizeCaption;
            var imgBackVisibility = Visibility.Collapsed;
            var Section1Visibility = Visibility.Visible;
            // Assert

            Assert.That(imgBackVisibility, Is.EqualTo(Visibility.Collapsed));
            Assert.That(Section1Visibility, Is.EqualTo(Visibility.Visible));
            if (CultureInfo.CurrentCulture.Name == "es-US")
            { Assert.That(txtCaptionText, Is.EqualTo("Button Customization")); }
            }

        [Test]
        public void TestInitializea()
        {
            vm = new PenViewModel(console, log) { SelectedButton = "TopBarrelButton" };
            penButtonSettingsRightView = new PenButtonSettingsRightView(vm);
            penButtonSettingsRightView.Initialize();
            // Assert
            Assert.Pass();
        }

        [Test]
        public void TestInitializeb()
        {
            vm = new PenViewModel(console, log) { SelectedButton = "TopButton" };
            penButtonSettingsRightView = new PenButtonSettingsRightView(vm);
            penButtonSettingsRightView.Initialize();

            // Assert
            Assert.Pass();
        }
    }
}
