using DDPM.SA.Common;
using DDPM.UI.Common;
using DDPM.UI.Common.UserControls;
using DDPM.UI.Plugin.ViewModels;
using Dell.Client.Framework.Common;
using Dell.Client.Framework.UX.WPF;
using Moq;
using NGA.UnitTest.PrivateObject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace DDPM.UI.Plugin.Common.Tests
{
    [Apartment(ApartmentState.STA)]
    public class RadialMenuModalDialogTests
    {
        private RadialMenuModalDialog? radialMenuModalDialog;
        private PenViewModel? _vm;
        private IConsole? console;
        private Mock<IConsole>? consoleMock;
        private ILog? log;
        private Mock<ILog>? logMock;
        private Mock<IDeviceManagerSA>? DeviceManagerSAMock;


        [SetUp]
        public void Setup()
        {
            consoleMock = new Mock<IConsole>();
            console = consoleMock.Object;
            logMock = new Mock<ILog>();
            log = logMock.Object;
            DeviceManagerSAMock = new Mock<IDeviceManagerSA>();
            DdpmCommonHelper.DeviceManagerSA = DeviceManagerSAMock.Object;
            DeviceManagerSAMock.Setup(x => x.GetEraserDoublePressSetting()).Returns(Task.FromResult("{\r\n    \"actionId\": 90,\r\n    \"actionName\": \"Screen Snipping\"\r\n}"));
            DeviceManagerSAMock.Setup(x => x.GetEraserSinglePressSetting()).Returns(Task.FromResult("{\r\n    \"actionId\": 90,\r\n    \"actionName\": \"Screen Snipping\"\r\n}"));
            DeviceManagerSAMock.Setup(x => x.GetEraserLongPressSetting()).Returns(Task.FromResult("{\r\n    \"actionId\": 90,\r\n    \"actionName\": \"Screen Snipping\"\r\n}"));

            DeviceManagerSAMock.Setup(x => x.GetSideTopSwitchSinglePressSetting()).Returns(Task.FromResult("{\r\n    \"actionId\": 90,\r\n    \"actionName\": \"Screen Snipping\"\r\n}"));
            DeviceManagerSAMock.Setup(x => x.GetSideBottomSwitchSinglePressSetting()).Returns(Task.FromResult("{\r\n    \"actionId\": 90,\r\n    \"actionName\": \"Screen Snipping\"\r\n}"));
            DeviceManagerSAMock.Setup(x => x.GetMenuSinglePressSetting()).Returns(Task.FromResult("{\r\n    \"actionId\": 90,\r\n    \"actionName\": \"Screen Snipping\"\r\n}"));

            DeviceManagerSAMock.Setup(x => x.GetMenuCenterRightClickSetting()).Returns(Task.FromResult(true));
            DeviceManagerSAMock.Setup(x => x.GetIsSideTopButtonHoverClick()).Returns(Task.FromResult(true));
            DeviceManagerSAMock.Setup(x => x.GetIsSideBottomButtonHoverClick()).Returns(Task.FromResult(true));
            _vm =new PenViewModel(console, log) { ActionNames=new Dictionary<int, string>() { {81,"A" },{82,"B" } } };
            radialMenuModalDialog = new RadialMenuModalDialog(1.0, 2.0,_vm);
        }

        [Test]
        public void TestConstructor_RadialMenuModalDialog()
        {
            // Assert
            Assert.That(radialMenuModalDialog, Is.Not.Null);
            Assert.That(radialMenuModalDialog.Width, Is.EqualTo(1.0));
            Assert.That(radialMenuModalDialog.Height, Is.EqualTo(2.0));
        }

    }
}
