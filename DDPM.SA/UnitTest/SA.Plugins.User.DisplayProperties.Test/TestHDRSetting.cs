using VcpCore.Common;

//using static VcpCore.Common.User32;

namespace DDPM.SA.Plugins.User.DisplayProperties.Test
{
    public class TestHDRSetting
    {
        private MonitorInfo monitorInfo1 = new MonitorInfo()
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

        private EDID eDID = new EDID()
        {
            ManufactureID = "DELL",
            VendorID = "42DD",
            Year = 2023,
            Month = 5,
            Week = 22,
            ModelName = "DELLU2724DD",
            EdidVersion = "V1.3",
            VideoInputType = "Digital Signal",
            Size = 27.1510868f,
            ServiceTag = "CN073K0",
            SerialNumber = "808597688",
            Edid = "00FFFFFFFFFFFF0010ACDC425538323016210103803C2278EA62A5AD5046AB240E5054A54B00714F8180A940D1C081C0A9C001010101565E00A0A0A029503020350055502100001A000000FF00434E3037334B300A2020202020000000FC0044454C4C20553237323444450A000000FD0030781EB23C000A20202020202001ED"
        };

        [Test]
        public void TestGetWindowsHDRStatus()
        {
            HDRSetting hDRSetting = new HDRSetting();
            //var monitors = displayPlugin.GetMonitors().Result;
            //Assert.Greater(monitors.Count, 0);
            //monitorInfo = monitors[0];
            //EDID edid = monitorInfo.edid;
            //Assert.IsNotNull(edid);
            //bool blOn = true;
            //bool blOffon = false;
            //bool boff = false;
            //var result = hDRSetting.GetWindowsHDRStatus(edid, out blOffon);
            //Assert.IsNotNull(result);
            //Assert.IsTrue(result);
            //if (blOffon)
            //{
            //    var result1 = hDRSetting.SetWindowsHDRStatus(edid, blOffon);
            //    Assert.IsNotNull(result1);
            //    Assert.IsTrue(result1);
            //    var result2 = hDRSetting.GetWindowsHDRStatus(edid, out blOffon);
            //    Assert.IsTrue(result2);
            //    Assert.IsTrue(blOffon);
            //    result = hDRSetting.SetWindowsHDRStatus(edid, boff);
            //    Assert.IsTrue(result);

            //}
            //else
            //{
            //    var result3 = hDRSetting.SetWindowsHDRStatus(edid, boff);
            //    Assert.IsNotNull(result3);
            //}

            bool blOn = true;
            bool blOffon = false;
            bool boff = false;
            var result = hDRSetting.GetWindowsHDRStatus(null, eDID, out blOffon);
            Assert.IsNotNull(result);
            Assert.IsFalse(result);
            if (blOffon)
            {
                var result1 = hDRSetting.SetWindowsHDRStatus(null, eDID, blOffon);
                Assert.IsNotNull(result1);
                Assert.IsTrue(result1);
                var result2 = hDRSetting.GetWindowsHDRStatus(null, eDID, out blOffon);
                Assert.IsTrue(result2);
                Assert.IsTrue(blOffon);
                result = hDRSetting.SetWindowsHDRStatus(null, eDID, boff);
                Assert.IsTrue(result);
            }
            else
            {
                var result3 = hDRSetting.SetWindowsHDRStatus(null, eDID, boff);
                Assert.IsNotNull(result3);
            }
        }

        [Test]
        public void TestSetWindowsHDRStatus()
        {
            //HDRSetting hDRSetting = new HDRSetting();
            //var monitors = displayPlugin.GetMonitors().Result;
            //Assert.Greater(monitors.Count, 0);
            //monitorInfo = monitors[0];
            //EDID edid = monitorInfo.edid;
            //Assert.IsNotNull(edid);
            //bool blOff = false;
            //bool blOn = true;
            //var result = hDRSetting.GetWindowsHDRStatus(edid, out blOff);
            //Assert.IsTrue(result);
            //if (blOff)
            //{
            //    var result1 = hDRSetting.SetWindowsHDRStatus(edid, blOn);
            //    Assert.IsNotNull(result1);
            //    Assert.IsTrue(result1);

            //    result = hDRSetting.SetWindowsHDRStatus(edid, blOff);
            //    Assert.IsNotNull(result);
            //    Assert.IsTrue(result);
            //}
            //else
            //{
            //    var result2 = hDRSetting.SetWindowsHDRStatus(edid, blOff);
            //    Assert.IsNotNull(result2);
            //}

            HDRSetting hDRSetting = new HDRSetting();
            bool blOff = false;
            bool blOn = true;
            var result = hDRSetting.GetWindowsHDRStatus(null, eDID, out blOff);
            Assert.IsNotNull(result);
            Assert.IsFalse(result);
            if (blOff)
            {
                var result1 = hDRSetting.SetWindowsHDRStatus(null, eDID, blOn);
                Assert.IsNotNull(result1);
                Assert.IsTrue(result1);

                result = hDRSetting.SetWindowsHDRStatus(null, eDID, blOff);
                Assert.IsNotNull(result);
                Assert.IsTrue(result);
            }
            else
            {
                var result3 = hDRSetting.SetWindowsHDRStatus(null, eDID, blOff);
                Assert.IsNotNull(result3);
            }
        }
    }
}