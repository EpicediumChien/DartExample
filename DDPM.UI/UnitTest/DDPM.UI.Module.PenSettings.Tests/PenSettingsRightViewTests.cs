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
using static System.Net.Mime.MediaTypeNames;
using Dell.Client.Framework.UX.WPF.ResourceManager;
using System.Windows;
using System.Globalization;
namespace DDPM.UI.Module.PenSettings.Tests
{
    [TestFixture, Apartment(ApartmentState.STA)]
    public class PenSettingsRightViewTests
    {
        private PenSettingsRightView? penSettingsRightView;
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
        }

        [Test]
        public void TestConstructor_PenSettingsRightView()
        {
            if (CultureInfo.CurrentCulture.Name == "es-US")
            {
                penSettingsRightView = new PenSettingsRightView(vm);
                privateObject = new PrivateObject(penSettingsRightView);
                var txtCaptionText = Strings.PenSettings;
                var txtTipSensitivityText = Strings.TipSensitivity;
                var txtTipTooltipText = Strings.TipTooltip;
                var txtTiltSensitivityText = Strings.TiltSensitivity;
                var txtTiltTooltipText = Strings.TiltTooltip;
                var txtPairWithTileText = Strings.PairWithTile;
                var txtPairTooltipText = Strings.PairTooltip;
                var btnGetStartContent = Strings.GetStarted2;
                // Assert
                Assert.That(penSettingsRightView, Is.Not.Null);
                Assert.That(privateObject!.GetFieldOrProperty("_vm"), Is.EqualTo(vm));
                Assert.That(txtCaptionText, Is.EqualTo("Pen Settings"));
                Assert.That(txtTipSensitivityText, Is.EqualTo("Tip Sensitivity"));
                Assert.That(txtTipTooltipText, Is.EqualTo("Moving the slider to the right will gradually decrease the sensitivity to pressure and you need to apply firmer pen pressure"));
                Assert.That(txtTiltSensitivityText, Is.EqualTo("Tilt Sensitivity"));
                Assert.That(txtTiltTooltipText, Is.EqualTo("Moving the slider to the right will gradually increase the tilting effect, and you need to apply less tilting angle"));
                Assert.That(txtPairWithTileText, Is.EqualTo("Pair with Tile"));
                Assert.That(txtPairTooltipText, Is.EqualTo("Pair your pen to your mobile device using the Tile app"));
                Assert.That(btnGetStartContent, Is.EqualTo("Get started"));
            }
        }
    }
}
