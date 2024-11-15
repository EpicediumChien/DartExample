using DDPM.UI.Common.EAEM;
using DDPM.UI.Common.Interfaces;
using DDPM.UI.Common.UserControls;
using Dell.Client.Framework.Common;
using Dell.Client.Framework.UX.WPF;
using NSubstitute;
using System.Runtime.InteropServices;
using Windows.Devices.Display.Core;
using Windows.Devices.Spi;
using static DDPM.UI.Common.Tests.User32Tests;
using static DDPM.UI.Common.User32;
using static VcpCore.Common.User32;
using DEVMODE = DDPM.UI.Common.User32.DEVMODE;
using DISPLAY_DEVICE = DDPM.UI.Common.User32.DISPLAY_DEVICE;
using MonitorEnumProc = DDPM.UI.Common.User32.MonitorEnumProc;
using Rect = DDPM.UI.Common.User32.Rect;

namespace DDPM.UI.Common.Tests
{
    [Apartment(ApartmentState.STA)]
    public class User32Tests
    {


        [SetUp]
        public void Setup()
        {

        }

        [Test]
        public void Test_GetMonitorInfo()
        {
            var result = User32._GetMonitorInfo(new System.Runtime.InteropServices.HandleRef(),new User32.MonitorInfoEx());
            // Assert
            Assert.That(result, Is.EqualTo(false));
        }

        [Test]
        public void Test_EnumDisplayMonitors()
        {
            var result = User32._EnumDisplayMonitors(new IntPtr(), new IntPtr(), MonitorEnum, 1);
            // Assert
            Assert.That(result, Is.EqualTo(true));
        }

        public bool MonitorEnum(IntPtr hDesktop, IntPtr hdc, ref Rect pRect, int dwData) 
        {
            return true;
        }

        [Test]
        public void Test_EnumDisplaySettings()
        {
            var lpDevMode =new DDPM.UI.Common.User32.DEVMODE();
            var result = User32._EnumDisplaySettings( "lpszDeviceName",  1, ref lpDevMode);
            // Assert
            Assert.That(result, Is.EqualTo(false));
        }

        [Test]
        public void Test_EnumDisplayDevices()
        {
            var lpDisplayDevice = new DISPLAY_DEVICE();
            var result = User32._EnumDisplayDevices("lpDevice",  1, ref lpDisplayDevice, 2);
            // Assert
            Assert.That(result, Is.EqualTo(false));
        }
    }
}