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
            privateArrangeVMObject.SetFieldOrProperty("_isFunctionEnabled", false);
            var Result1 = arrangeVM.IsFunctionEnabled;       //IsFunctionEnabled false
            Assert.IsFalse(Result1);

            privateArrangeVMObject.SetFieldOrProperty("_isFunctionEnabled", true);
            var Result2 = arrangeVM.IsFunctionEnabled;      //IsFunctionEnabled true
            Assert.IsTrue(Result2);
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

    }
}
