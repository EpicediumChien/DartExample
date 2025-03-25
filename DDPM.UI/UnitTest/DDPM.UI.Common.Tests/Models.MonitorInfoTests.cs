using DDPM.SA.Common;
using DDPM.UI.Common.EAEM;
using DDPM.UI.Common.Interfaces;
using DDPM.UI.Common.Models;
using DDPM.UI.Common.ViewModels;
using Dell.Client.Framework.Common;
using Dell.Client.Framework.UX.WPF;
using DPeMPublic.Common.Enums;
using Moq;
using System.Reflection.Metadata;
using System.Runtime.Intrinsics.X86;
using System.Security.Policy;
using System.Windows.Media;
using System.Xml.Linq;
using VcpCore.Common;
using Windows.Devices.Input;
using static DDPM.UI.Common.User32;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;

namespace DDPM.UI.Common.Tests
{
    [Apartment(ApartmentState.STA)]
    public class MonitorInfo_UnusedTests
    {
        private MonitorInfo_Unused? monitorInfo_Unused;

        //[SetUp]
        //public void Setup()
        //{
        //    monitorInfo_Unused = new MonitorInfo_Unused();
        //}

        //[Test]
        //public void TestConstructor_MonitorInfo_Unused()
        //{
        //    // Assert
        //    Assert.That(monitorInfo_Unused, Is.Not.Null);
        //}

        //[Test]
        //public void TestHandle()
        //{
        //    // Act
        //    monitorInfo_Unused.Handle = new IntPtr();
        //    // Assert
        //    Assert.That(monitorInfo_Unused.Handle, Is.InstanceOf<IntPtr>());
        //}

        //[Test]
        //public void TestIsDellMonitor()
        //{
        //    // Act
        //    monitorInfo_Unused.IsDellMonitor = true;
        //    // Assert
        //    Assert.That(monitorInfo_Unused.IsDellMonitor, Is.EqualTo(true));
        //}

        //[Test]
        //public void TestCapabilityString()
        //{
        //    // Act
        //    monitorInfo_Unused.CapabilityString = "CapabilityString";
        //    // Assert
        //    Assert.That(monitorInfo_Unused.CapabilityString, Is.EqualTo("CapabilityString"));
        //}

        //[Test]
        //public void TesthMonitor()
        //{
        //    // Act
        //    monitorInfo_Unused.hMonitor = new IntPtr();
        //    // Assert
        //    Assert.That(monitorInfo_Unused.hMonitor, Is.InstanceOf<IntPtr>());
        //}

        //[Test]
        //public void TesthPhysicalMonitor()
        //{
        //    // Act
        //    monitorInfo_Unused.hPhysicalMonitor = new IntPtr();
        //    // Assert
        //    Assert.That(monitorInfo_Unused.hPhysicalMonitor, Is.InstanceOf<IntPtr>());
        //}


        //[Test]
        //public void TestszPhysicalMonitorDescription()
        //{
        //    // Act
        //    monitorInfo_Unused.szPhysicalMonitorDescription = "szPhysicalMonitorDescription";
        //    // Assert
        //    Assert.That(monitorInfo_Unused.szPhysicalMonitorDescription, Is.EqualTo("szPhysicalMonitorDescription"));
        //}

        //[Test]
        //public void TestDisplayName()
        //{
        //    // Act
        //    monitorInfo_Unused.DisplayName = "DisplayName";
        //    // Assert
        //    Assert.That(monitorInfo_Unused.DisplayName, Is.EqualTo("DisplayName"));
        //}

        //[Test]
        //public void TestDDCisON()
        //{
        //    // Act
        //    monitorInfo_Unused.DDCisON = true;
        //    // Assert
        //    Assert.That(monitorInfo_Unused.DDCisON, Is.EqualTo(true));
        //}

        //[Test]
        //public void Testedid()
        //{
        //    // Act
        //    monitorInfo_Unused.edid = new EDID();
        //    // Assert
        //    Assert.That(monitorInfo_Unused.edid, Is.InstanceOf<EDID>());
        //}

        //[Test]
        //public void Testdisplaydevice()
        //{
        //    // Act
        //    monitorInfo_Unused.displaydevice = new DISPLAY_DEVICE();
        //    // Assert
        //    Assert.That(monitorInfo_Unused.displaydevice, Is.InstanceOf<DISPLAY_DEVICE>());
        //}

        //[Test]
        //public void TestpMonitorInfoEx()
        //{
        //    // Act
        //    monitorInfo_Unused.pMonitorInfoEx = new MonitorInfoEx();
        //    // Assert
        //    Assert.That(monitorInfo_Unused.pMonitorInfoEx, Is.InstanceOf<MonitorInfoEx>());
        //}


        //[Test]
        //public void TestpDevmode()
        //{
        //    // Act
        //    monitorInfo_Unused.pDevmode = new DEVMODE();
        //    // Assert
        //    Assert.That(monitorInfo_Unused.pDevmode, Is.InstanceOf<DEVMODE>());
        //}

        //[Test]
        //public void TestColorPresetSupportList()
        //{
        //    // Act
        //    monitorInfo_Unused.ColorPresetSupportList = new List<string>();
        //    // Assert
        //    Assert.That(monitorInfo_Unused.ColorPresetSupportList, Is.Not.Null);
        //}

    }
}