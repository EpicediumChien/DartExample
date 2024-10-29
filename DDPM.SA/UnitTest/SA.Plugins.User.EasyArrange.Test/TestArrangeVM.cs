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
    

    }
}
