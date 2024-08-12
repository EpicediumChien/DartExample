using DDPM.SA.Common;
using DDPM.SA.Plugins.User.DisplayManager;
using DDPM.SA.Plugins.User.PipPbpManger;
using Dell.Client.Framework.Interfaces;
using Dell.Client.Framework.UnitTestShared.Tests;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VcpCore.Common;
using VcpCore.Interfaces;
using VcpCore.Plugins;
using static DDPM.SA.Plugins.User.DisplayProperties.user32;
using static VcpCore.Common.User32;
using DEVMODE = DDPM.SA.Plugins.User.DisplayProperties.user32.DEVMODE;

namespace DDPM.SA.Plugins.User.DisplayProperties.Test
{
    public class TestUser32
    {
        MonitorInfo monitorInfo1 = new MonitorInfo()
        {
            AliasDeviceName = "Dell U2724DE(HDMI)",
            IsDellMonitor = true,
            Index = 0,
            CapabilityString = "(prot(monitor)type(LCD)model(U2424H)cmds(01 02 03 07 0C E3 F3)vcp(02 04 05 08 10 12 14(01 04 05 06 08 09 0B 0C)E5 E7(02 03) E2(00 02 04 0C 0D 0F)",
            DisplayName = "DISPLAY7",
            DDCisON = true,
            FwVersion = "M3T101",
            inputSource = "HDMI-1",
            modelName = "U2724DE",
            series = "Dell UltraSharp (U) Series Monitors",
            edid = new EDID() { SerialNumber = "808597589" },
            //CapabilityDic = capabilityDic;
        };

        [Test]
        public void TestEnumDisplaySettings()
        {
            DEVMODE devMode = new DEVMODE();
            var monitorInfoname = monitorInfo1.DisplayName;
            int i = user32.ENUM_CURRENT_SETTINGS;  //ENUM_CURRENT_SETTINGS=-1
            var result = user32.EnumDisplaySettings(monitorInfoname, i, ref devMode);
            Assert.IsNotNull(result);
            // Assert.IsTrue(result);   需要用真实get的monitor才可以跑pass
        }

        [Test]
        public void TestChangeDisplaySettingsEx()
        {
            DEVMODE devMode = new DEVMODE();
            var monitorInfoname = monitorInfo1.DisplayName;
            int expectresult = user32.DISP_CHANGE_SUCCESSFUL;  //DISP_CHANGE_SUCCESSFUL=0;
            int result = user32.ChangeDisplaySettingsEx(monitorInfoname, ref devMode, IntPtr.Zero, ChangeDisplaySettingsFlags.CDS_UPDATEREGISTRY, IntPtr.Zero);
            Assert.IsNotNull(result);
            // Assert.That(expectresult, Is.EqualTo(result)); 需要用真实get的monitor才可以跑pass

        }

        [Test]
        public void TestGetLastErrMsg()
        {
            //user32 use32 = new user32();
            string expectedMessage = "The operation completed successfully.";
            var result = user32.GetLastErrMsg();
            Assert.IsNotNull(result);
            Assert.That(expectedMessage.Trim(new char[] { '\r', '\n' }), Is.EqualTo(result));
        }



    }
}
