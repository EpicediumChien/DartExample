using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VcpCore.Common;
using static VcpCore.Common.User32;
using Microsoft.Win32;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static VcpCore.Common.dxva2;
using VcpCore.Plugins;
using DDPM.SA.Common;
using DDPM.SA.Plugins.User.DisplayManager;
using DDPM.SA.Plugins.User.DisplayProperties;
using DDPM.SA.Plugins.User.PipPbpManger;
using Dell.Client.Framework.Interfaces;
using Dell.Client.Framework.UnitTestShared.Tests;
using VcpCore.Interfaces;
using System.Windows.Documents;
using WinCopies;



namespace VcpCore.Plugins.Test
{
    public class TestDispalyConfigLirary
    {

        EDID eDID = new EDID()
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
        public void TestGetDisplayConfigPath()
        {
            //var monitors = displayPlugin.GetMonitors().Result;
            //Assert.Greater(monitors.Count, 0);
            //monitorInfo = monitors[0];
            //EDID edid = monitorInfo.edid;
            //Assert.IsNotNull(edid);
            //DISPLAYCONFIG_PATH_INFO outPath_ = new DISPLAYCONFIG_PATH_INFO();
            //var getDisplayConfigPath = DisplayConfigLibrary.GetDisplayConfigPath(edid, out outPath_);
            //Assert.IsTrue(getDisplayConfigPath);
            //Assert.IsNotNull(outPath_);
            string hexString = "00FFFFFFFFFFFF0042CBDF455528323016210103803C2278EA62A5AD5046AB240E5054A54B00714F8180A940D1C081C0A9C001010101565E00A0A0A029503020350055502100001A000000FF00434E3037334B300A2020202020000000FC0044454C4C20553237323444450A000000FD0030781EB23C000A20202020202001ED";
            EdidParser edidparser = new EdidParser();
            PrivateObject privateObject = new PrivateObject(edidparser);
            privateObject.SetFieldOrProperty("HexString", hexString);
            DISPLAYCONFIG_PATH_INFO outPath_ = new DISPLAYCONFIG_PATH_INFO();
            var getDisplayConfigPath = DisplayConfigLibrary.GetDisplayConfigPath(eDID, out outPath_);//fade monitior edid and real monitorinfo is not same, returnfalse
            Assert.IsFalse(getDisplayConfigPath);
            Assert.IsNotNull(outPath_);
        }


        [Test]
        public void TestGetWindowsHDRStatus()
        {
            DISPLAYCONFIG_PATH_INFO path = new DISPLAYCONFIG_PATH_INFO();
            bool Bol=false;
            var getWindowsHDRStatus = DisplayConfigLibrary.GetWindowsHDRStatus(path, out Bol);
            Assert.IsNotNull(getWindowsHDRStatus);
            if (Bol)
            {
                Assert.IsTrue(getWindowsHDRStatus);
            }
            else 
            {
                Assert.IsFalse(getWindowsHDRStatus);
            }

        }

        [Test]
        public void TestSetWindowsHDRStatus()
        {
            DISPLAYCONFIG_PATH_INFO path = new DISPLAYCONFIG_PATH_INFO();
            bool Bol = false;
            var getWindowsHDRStatus = DisplayConfigLibrary.GetWindowsHDRStatus(path, out Bol);
            Assert.IsNotNull(getWindowsHDRStatus);
            if (Bol)
            {
                Assert.IsTrue(getWindowsHDRStatus);
                var setWindowsHDRStatu=DisplayConfigLibrary.SetWindowsHDRStatus(path, Bol);
                Assert.IsTrue(setWindowsHDRStatu);
                bool Bol2=false;
                var setWindowsHDRStatu2 = DisplayConfigLibrary.SetWindowsHDRStatus(path, Bol2);
                Assert.IsTrue(setWindowsHDRStatu2);

            }
            else
            {
                Assert.IsFalse(getWindowsHDRStatus);
                var setWindowsHDRStatu2 = DisplayConfigLibrary.SetWindowsHDRStatus(path, Bol);
                Assert.IsFalse(setWindowsHDRStatu2);
            }

        }


    }

    
}
