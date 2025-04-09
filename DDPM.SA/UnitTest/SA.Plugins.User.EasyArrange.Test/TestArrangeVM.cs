using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DDPM.SA.Plugins.User.EasyArrange;
using DDPM.SA.Common;
using DDPM.SA.Common.Settings;
using DDPM.SA.Common.Interfaces;
using Dell.Client.Framework.Interfaces;
using Moq;
using VcpCore.Common;
using VcpCore.Interfaces;
using DDPM.SA.Plugins.User.DisplayManager;
using Dell.Client.Framework.UnitTestShared.Tests;
using DDPM.EABroker;
using System.Windows.Automation;
using System.Windows.Forms;
using DDPM.Easy.Common;
using System.Windows.Controls;
using System.Windows;

namespace DDPM.SA.Plugins.User.EasyArrange.Test
{
    [Apartment(ApartmentState.STA)]
    public class TestArrangeVM
    {
        private Mock<IAgent> DisplayMangerAgent { get; } = new();
        private Mock<IAgent> VcpCoreAgent { get; } = new();
        private Mock<IAgent> EApluginAgent { get; } = new();

        private MonitorInfo monitorInfo = new MonitorInfo();
        private Mock<IVcpCoreService> VcpCoreService { get; } = new();
        private Mock<IDisplayService> DisplayManagerService { get; } = new();
        private Mock<IDeviceManagerSA> DeviceManagerService { get; } = new();
        private Mock<IEasyArrangeService> EasyArrangeService { get; } = new();

        private MonitorInfo monitorInfo1 = new MonitorInfo()
        {
            AliasDeviceName = "Dell U2724DE(HDMI)",
            IsDellMonitor = true,
            Index = 0,
            CapabilityString = "(prot(monitor)type(LCD)model(U2424H)cmds(01 02 03 07 0C E3 F3)vcp(02 04 05 08 10 12 14(01 04 05 06 08 09 0B 0C)E5 E7(02 03) E2(00 02 04 0C 0D 0F)",
            DisplayName = "TestDISPLAY7",
            DDCisON = true,
            FwVersion = "M3T101",
            inputSource = "HDMI-1",
            modelName = "TestU2724DE",
            series = "Dell UltraSharp (U) Series Monitors",
            //CapabilityDic = capabilityDic;
            CapabilityDic = new Dictionary<string, List<string>>(),
            edid = new EDID()
            {
                ManufactureID = "DEL",
                VendorID = "42DC",
                Year = 2023,
                Month = 5,
                Week = 22,
                EdidVersion = "V1.3",
                VideoInputType = "Digital Signal",
                Size = 27.1510868f,
                ServiceTag = "CN073K0",
                SerialNumber = "808597589",
                Edid = "00FFFFFFFFFFFF0010ACDC425538323016210103803C2278EA62A5AD5046AB240E5054A54B00714F8180A940D1C081C0A9C001010101565E00A0A0A029503020350055502100001A000000FF00434E3037334B300A2020202020000000FC0044454C4C20553237323444450A000000FD0030781EB23C000A20202020202001ED"

            },
        };
        private ArrangeVM arrangeVM;
        private PrivateObject privateArrangeVMObject;
        [SetUp]
        public void Setup()
        {
            arrangeVM = new ArrangeVM();
            privateArrangeVMObject = new PrivateObject(arrangeVM);
        }

        [Test]
        public void TestIsFunctionEnabled()
        {
            //Robert_Lin, 2025-1-7, most of classes in DDPM.SA.Plugins.User.EasyArrange are deleted,
            // Please use DDPM.SA/Common/DDPM.EABroker project instead.
            // _isFunctionEnabled is unused properties, to be deleted, please do not test it.

            //privateArrangeVMObject.SetFieldOrProperty("_isFunctionEnabled", false);
            //var Result1 = arrangeVM.IsFunctionEnabled;       //IsFunctionEnabled false
            //Assert.IsFalse(Result1);

            //privateArrangeVMObject.SetFieldOrProperty("_isFunctionEnabled", true);
            //var Result2 = arrangeVM.IsFunctionEnabled;      //IsFunctionEnabled true
            //Assert.IsTrue(Result2);
        }

        [Test]
        public void TestIsMoving()
        {
            privateArrangeVMObject.SetFieldOrProperty("_isMoving", true);
            var IsMoving_Result1 = arrangeVM.IsMoving;  //_isMoving true
            Assert.IsTrue(IsMoving_Result1);

            privateArrangeVMObject.SetFieldOrProperty("_isMoving", false);
            var IsMoving_Result2 = arrangeVM.IsMoving; //_isMoving false defalut
            Assert.IsFalse(IsMoving_Result2);
        }

        [Test]
        public void TestIsWorkUIEnabled()
        {
            privateArrangeVMObject.SetFieldOrProperty("_isWorkUIEnabled", false);
            var IsWorkUIEnabled_Result1 = arrangeVM.IsWorkUIEnabled;  //_isWorkUIEnabled false
            Assert.IsFalse(IsWorkUIEnabled_Result1);

            privateArrangeVMObject.SetFieldOrProperty("_isWorkUIEnabled", true);
            var IsWorkUIEnabled_Result2 = arrangeVM.IsWorkUIEnabled; //_isWorkUIEnabled true default
            Assert.IsTrue(IsWorkUIEnabled_Result2);
        }

        [Test]
        public void TestEzSettings()
        {
            //Robert_Lin, 2025-1-7, most of classes in DDPM.SA.Plugins.User.EasyArrange are deleted,
            // Please use DDPM.SA/Common/DDPM.EABroker project instead.
            /*
            EzSettings ezSettings = new EzSettings()
            {
                IsWidthoutGap = true,
                IsOnlyAllowWhenShiftKeyPressed = false,
                IsSpanAcrossMultiMonitors = false,
                IsAwsEnabled = false,
            };
            privateArrangeVMObject.SetFieldOrProperty("_ezSettings", ezSettings);
            var EzSettings_Result1 = arrangeVM.EzSettings;
            Assert.NotNull(EzSettings_Result1);
            Assert.That(ezSettings, Is.EqualTo(EzSettings_Result1));
            */
        }

        [Test]
        public void TestIsShiftPressed()
        {
            privateArrangeVMObject.SetFieldOrProperty("_isShiftPressed", true);
            var IsShiftPressed_Result1 = arrangeVM.IsShiftPressed;  //_isShiftPressed true
            Assert.IsTrue(IsShiftPressed_Result1);

            privateArrangeVMObject.SetFieldOrProperty("_isShiftPressed", false);
            var IsShiftPressed_Result2 = arrangeVM.IsShiftPressed; //_isShiftPressed false default
            Assert.IsFalse(IsShiftPressed_Result2);
        }

        [Test]
        public void TestIsWorkUIShowing()
        {
            //Robert_Lin, 2025-1-7, most of classes in DDPM.SA.Plugins.User.EasyArrange are deleted,
            // Please use DDPM.SA/Common/DDPM.EABroker project instead.

            /*
            privateArrangeVMObject.SetFieldOrProperty("_isMoving", false);
            var IsWorkUIShowing_Result1 = arrangeVM.IsWorkUIShowing;  //_isMoving false
            Assert.IsFalse(IsWorkUIShowing_Result1);

            privateArrangeVMObject.SetFieldOrProperty("_isMoving", true);
            privateArrangeVMObject.SetFieldOrProperty("_isWorkUIEnabled", false);
            var IsWorkUIShowing_Result2 = arrangeVM.IsWorkUIShowing; //_isMoving true, _isWorkUIEnabled false  
            Assert.IsFalse(IsWorkUIShowing_Result2);

            EzSettings ezSettings = new EzSettings()
            {
                IsWidthoutGap = true,
                IsOnlyAllowWhenShiftKeyPressed = false,
                IsSpanAcrossMultiMonitors = false,
                IsAwsEnabled = false,
            };
            privateArrangeVMObject.SetFieldOrProperty("_isWorkUIEnabled", true);
            privateArrangeVMObject.SetFieldOrProperty("_ezSettings", ezSettings);
            var IsWorkUIShowing_Result3 = arrangeVM.IsWorkUIShowing; //_isMoving true, _isWorkUIEnabled true,  IsOnlyAllowWhenShiftKeyPressed false
            Assert.IsTrue(IsWorkUIShowing_Result3);

            ezSettings.IsOnlyAllowWhenShiftKeyPressed = true;
            privateArrangeVMObject.SetFieldOrProperty("_ezSettings", ezSettings);
            privateArrangeVMObject.SetFieldOrProperty("_isShiftPressed", true);
            var IsWorkUIShowing_Result4 = arrangeVM.IsWorkUIShowing; //_isMoving true, _isWorkUIEnabled true,  IsOnlyAllowWhenShiftKeyPressed true, _isShiftPressed true
            Assert.IsTrue(IsWorkUIShowing_Result4);
            */
        }

        [Test]
        public void TestxCursor()
        {
            int xCursor1 = 1;
            privateArrangeVMObject.SetFieldOrProperty("_xCursor", xCursor1);
            var xCursor_Result1 = arrangeVM.xCursor;       //_xCursor 1
            Assert.That(xCursor1, Is.EqualTo(xCursor_Result1));

            int xCursor2 = 0;
            privateArrangeVMObject.SetFieldOrProperty("_xCursor", xCursor2);
            var xCursor_Result2 = arrangeVM.xCursor;       //_xCursor 1
            Assert.That(xCursor2, Is.EqualTo(xCursor_Result2));
        }

        [Test]
        public void TestyCursor()
        {
            int yCursor1 = 1;
            privateArrangeVMObject.SetFieldOrProperty("_yCursor", yCursor1);
            var yCursor_Result1 = arrangeVM.yCursor;          //_yCursor 1
            Assert.That(yCursor1, Is.EqualTo(yCursor_Result1));

            int yCursor2 = 0;
            privateArrangeVMObject.SetFieldOrProperty("_yCursor", yCursor2);
            var yCursor_Result2 = arrangeVM.yCursor;         //_yCursor 0
            Assert.That(yCursor2, Is.EqualTo(yCursor_Result2));
        }

        [Test]
        public void TestScreenScale()
        {
            double screenScale1 = 2.00;
            privateArrangeVMObject.SetFieldOrProperty("_screenScale", screenScale1);
            var ScreenScale_Result1 = arrangeVM.ScreenScale;          //_screenScale 2.00
            Assert.That(screenScale1, Is.EqualTo(ScreenScale_Result1));

            double screenScale2 = 1.00;
            privateArrangeVMObject.SetFieldOrProperty("_screenScale", screenScale2);
            var ScreenScale_Result2 = arrangeVM.ScreenScale;          //_screenScale 1.00
            Assert.That(screenScale2, Is.EqualTo(ScreenScale_Result2));
        }

        [Test]
        public void TestWorkingScreen()
        {
            //Robert_Lin, 2025-1-7, most of classes in DDPM.SA.Plugins.User.EasyArrange are deleted,
            // Please use DDPM.SA/Common/DDPM.EABroker project instead.

            /*
            Screen? Primaryscreen;
            Primaryscreen = Screen.PrimaryScreen;
            privateArrangeVMObject.SetFieldOrProperty("_workingScreen", null);
            var WorkingScreen_Result1 = arrangeVM.WorkingScreen;          //_workingScreen as null
            Assert.IsNull(WorkingScreen_Result1);

            Primaryscreen = Screen.PrimaryScreen;
            privateArrangeVMObject.SetFieldOrProperty("_workingScreen", Primaryscreen);
            var WorkingScreen_Result2 = arrangeVM.WorkingScreen;         //_workingScreen as Primaryscreen
            Assert.IsNotNull(WorkingScreen_Result2);
            Assert.That(Primaryscreen, Is.EqualTo(WorkingScreen_Result2));
            */
        }

        [Test]
        public void TestRefreshScreenScale()
        {
            double screenScale1 = 1.00;
            var result = arrangeVM.RefreshScreenScale();
            Assert.IsNotNull(result);
        }

        [Test]
        public void TestHoveringCellObj()
        {
            Border border = new Border()
            {
                BorderThickness = new Thickness(2),
                BorderBrush = System.Windows.Media.Brushes.Black  // set Border Property
            };
            CellObj hoveringCellObj1 = new CellObj("Cell 1", border)
            {
                Name = "Cell 1",
                rc = new Rect(10, 20, 50, 60),
                rcRatio = new Rect(0.2, 0.3, 0.4, 0.5),
            };
            privateArrangeVMObject.SetFieldOrProperty("_hoveringCellObj", hoveringCellObj1);
            var HoveringCellObj_Result1 = arrangeVM.HoveringCellObj;          //_hoveringCellObj as new hoveringCellObj1
            Assert.That(hoveringCellObj1, Is.EqualTo(HoveringCellObj_Result1));

            CellObj? hoveringCellObj2 = null;
            privateArrangeVMObject.SetFieldOrProperty("_hoveringCellObj", hoveringCellObj2);
            var HoveringCellObj_Result2 = arrangeVM.HoveringCellObj;          //_hoveringCellObj as default
            Assert.That(hoveringCellObj2, Is.EqualTo(HoveringCellObj_Result2));
        }

        /*[Test]
        public void TestHoveringCell()
        {
            string hoveringCell1 = "Test HoveringCell";
            privateArrangeVMObject.SetFieldOrProperty("_hoveringCell", hoveringCell1);
            var HoveringCell_Result1 = arrangeVM.HoveringCell;          //_hoveringCell "Test HoveringCell"
            Assert.That(hoveringCell1, Is.EqualTo(HoveringCell_Result1));

            string hoveringCell2 = "";
            privateArrangeVMObject.SetFieldOrProperty("_hoveringCell", hoveringCell2);
            var HoveringCell_Result2 = arrangeVM.HoveringCell;          //_hoveringCell default
            Assert.That(hoveringCell2, Is.EqualTo(HoveringCell_Result2));
        }*/

        [Test]
        public void TestHoveringScreen()
        {
            //Robert_Lin, 2025-1-7, most of classes in DDPM.SA.Plugins.User.EasyArrange are deleted,
            // Please use DDPM.SA/Common/DDPM.EABroker project instead.
            /*
            string HoveringScreen1 = "Test HoveringScreen";
            privateArrangeVMObject.SetFieldOrProperty("_hoveringScreen", HoveringScreen1);
            var HoveringScreen_Result1 = arrangeVM.HoveringScreen;          //_hoveringScreen "Test HoveringScreen";
            Assert.That(HoveringScreen1, Is.EqualTo(HoveringScreen_Result1));

            string HoveringScreen2 = "";
            privateArrangeVMObject.SetFieldOrProperty("_hoveringScreen", HoveringScreen2);
            var HoveringScreen_Result2 = arrangeVM.HoveringScreen;          //_hoveringScreen default
            Assert.That(HoveringScreen2, Is.EqualTo(HoveringScreen_Result2));
            */
        }

        /*[Test]
        public void TestIsAwsEnabled()
        {
            bool isAwsEnabled = false;
            EzSettings ezSettings = new EzSettings()
            {
                IsWidthoutGap = true,
                IsOnlyAllowWhenShiftKeyPressed = false,
                IsSpanAcrossMultiMonitors = false,
                IsAwsEnabled = false,
            };
            privateArrangeVMObject.SetFieldOrProperty("_ezSettings", null);
            var IsAwsEnabled_Result1 = arrangeVM.IsAwsEnabled;          //_ezSettings null;
            Assert.That(isAwsEnabled, Is.EqualTo(IsAwsEnabled_Result1));

            privateArrangeVMObject.SetFieldOrProperty("_ezSettings", ezSettings);
            var IsAwsEnabled_Result2 = arrangeVM.IsAwsEnabled;          //_ezSettings not null;IsAwsEnabled = false,
            Assert.That(isAwsEnabled, Is.EqualTo(IsAwsEnabled_Result2));
        }
        */
        /* [Test]
       public void TestIsAwsWindowVisible()
        {
            bool IsAwsWindowVisible1 = true;
            bool IsAwsWindowVisible2 = false;
            EzSettings ezSettings = new EzSettings()
            {
                IsWidthoutGap = true,
                IsOnlyAllowWhenShiftKeyPressed = false,
                IsSpanAcrossMultiMonitors = false,
                IsAwsEnabled = true,
            };
            privateArrangeVMObject.SetFieldOrProperty("_isAwsWindowVisible", true);
            privateArrangeVMObject.SetFieldOrProperty("_isMoving", true);
            privateArrangeVMObject.SetFieldOrProperty("_ezSettings", ezSettings);
            var IsAwsWindowVisible_Result1 = arrangeVM.IsAwsWindowVisible;          //_isAwsWindowVisible,  _isMoving,IsAwsEnabled,true, _ezSettings not null, IsOnlyAllowWhenShiftKeyPressed = false,newValue = true;
            Assert.That(IsAwsWindowVisible1, Is.EqualTo(IsAwsWindowVisible_Result1));

            privateArrangeVMObject.SetFieldOrProperty("_isAwsWindowVisible", false);
            privateArrangeVMObject.SetFieldOrProperty("_isMoving", false);
            privateArrangeVMObject.SetFieldOrProperty("_ezSettings", ezSettings);
            var IsAwsWindowVisible_Result2 = arrangeVM.IsAwsWindowVisible;          //_isAwsWindowVisible, _isMoving, default false
            Assert.That(IsAwsWindowVisible2, Is.EqualTo(IsAwsWindowVisible_Result2));
        }*/

        [Test]
        public void TestAwsWindow()
        {
            //Robert_Lin, 2025-1-7, DDPM.SA.Plugins.User.EasyArrange.ArrangeVM and AwsWindow is removed.
            //Please use DDPM.SA/Common/DDPM.EABroker project instead.

            //ArrangeVM vm = new ArrangeVM();
            //AwsWindow awsWindow;
            //awsWindow = new AwsWindow(vm);
            //arrangeVM.AwsWindow = awsWindow;  //set AwsWindow
            //var AwsWindow_Result1 = arrangeVM.AwsWindow;     //get AwsWindow     
            //Assert.IsNotNull(AwsWindow_Result1);
        }

        [Test]
        public void TestHoveringSplit()
        {
            Mock<ISplitCtrl> mockSplitCtrl = new Mock<ISplitCtrl>();
            var mockSplitCtrlObj = mockSplitCtrl.Object;
            privateArrangeVMObject.SetFieldOrProperty("_hoveringSplit", mockSplitCtrlObj);
            var HoveringSplit_Result1 = arrangeVM.HoveringSplit;
            Assert.IsNotNull(HoveringSplit_Result1);
        }

        [Test]
        public void TestHoveringWindow()
        {
            string TestHoveringWindow = "";
            arrangeVM.HoveringWindow = TestHoveringWindow;  //set HoveringWindow
            var HoveringWindow_Result1 = arrangeVM.HoveringWindow;     //get HoveringWindow     
            Assert.IsNotNull(HoveringWindow_Result1);
        }

        [Test]
        public void TestAddWorkWindow()
        {
            //Robert_Lin, 2025-1-7, EAWorkWindow in DDPM.SA.Plugins.User.EasyArrange is deleted,
            // Please use DDPM.SA/Common/DDPM.EABroker project instead
            //

            //List<EAWorkWindow> workWindows2 = new List<EAWorkWindow>();
            //privateArrangeVMObject.SetFieldOrProperty("_workWindows2", workWindows2);
            //List<MonitorInfo> _monitors = new List<MonitorInfo>();
            //Screen screen = Screen.PrimaryScreen;
            //string displayName = Screen.PrimaryScreen.DeviceName;
            //monitorInfo1.DisplayName = displayName;
            //_monitors.Add(monitorInfo1);

            //ArrangeVM arrangeVM1 = new ArrangeVM();
            //EAWorkWindow workWindow1 = new EAWorkWindow(arrangeVM1, screen, _monitors);

            //arrangeVM.AddWorkWindow(workWindow1);   //Add workWindow1 to WorkWindow list
            //var AddWorkWindow_Result = (List<EAWorkWindow>)privateArrangeVMObject.GetFieldOrProperty("_workWindows2");
            //Assert.IsNotNull(AddWorkWindow_Result);
        }

    }
}
