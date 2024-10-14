using DDPM.SA.Common;
using DDPM.SA.Common.Display;
using DDPM.SA.Common.Interfaces;
using DDPM.SA.Plugins.User.DisplayProperties;
using DDPM.SA.Plugins.User.EasyArrange;
using DDPM.SA.Plugins.User.PipPbpManger;
using DDPM.SA.Plugins.User.DisplayManager;
using NetworkKVM.Plugins;
using Dell.Client.Framework.Interfaces;
using Dell.Client.Framework.UnitTestShared.Tests;
using Moq;
using VcpCore.Common;
using VcpCore.Interfaces;
using VcpCore.Plugins;
//using WinCopies.Util;
using System.IO.Pipes;
using static VcpCore.Common.User32;
using System.Windows.Media.Animation;
using Windows.System;
using System.Globalization;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Security.Policy;
using static NetworkKVM.Plugins.NKVMPlugin;
using Dell.Client.Framework.Common;
using System.Windows;


namespace DDPM.SA.Plugins.User.NetworkKVM.Test;

public class TestNetworkKVM
{
    private Mock<IAgent> DisplayMangerAgent { get; } = new();
    private Mock<IAgent> VcpCoreAgent { get; } = new();
    private Mock<IAgent> NKVMPluginAgent { get; } = new();

    private MonitorInfo monitorInfo = new MonitorInfo();
    private Mock<IVcpCoreService> VcpCoreService { get; } = new();
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
        //CapabilityDic = capabilityDic;
        CapabilityDic = new Dictionary<string, List<string>>(),
        edid = new EDID()
        {
            ManufactureID = "DEL",
            VendorID = "42DC",
            Year = 2023,
            Month = 5,
            Week = 22,
            ModelName = "DELLU2724DE",
            EdidVersion = "V1.3",
            VideoInputType = "Digital Signal",
            Size = 27.1510868f,
            ServiceTag = "CN073K0",
            SerialNumber = "808597589",
            Edid = "00FFFFFFFFFFFF0010ACDC425538323016210103803C2278EA62A5AD5046AB240E5054A54B00714F8180A940D1C081C0A9C001010101565E00A0A0A029503020350055502100001A000000FF00434E3037334B300A2020202020000000FC0044454C4C20553237323444450A000000FD0030781EB23C000A20202020202001ED"

        },
    };
    private List<MonitorInfo> monitorInfos = new List<MonitorInfo>();

    private DisplayMangerPlugin CreateInitializeDisplayMangerPlugin()
    {
        DisplayMangerAgent.Setup(x => x.PluginManager.FindPluginByGuid(Guid.Parse(VcpCore.Common.IDs.Display_Manager_PLUGIN_ID)));

        return new DisplayMangerPlugin(DisplayMangerAgent.Object);
    }

    private VcpCorePlugin CreateInitializeVcpCorePlugin()
    {
        VcpCoreAgent.Setup(x => x.PluginManager.FindPluginByGuid(Guid.Parse(VcpCore.Common.IDs.VCP_CORE_PLUGIN_ID)));

        return new VcpCorePlugin(VcpCoreAgent.Object);
    }

    private NKVMPlugin CreateInitializeNkvmPlugin()
    {
        NKVMPluginAgent.Setup(x => x.PluginManager.FindPluginByGuid(Guid.Parse(DDPM.SA.Common.IDs.DDPM_NKVM_PLUGIN_ID)));

        return new NKVMPlugin(NKVMPluginAgent.Object);
    }

    private DisplayMangerPlugin displayPlugin;
    private VcpCorePlugin vcpCorePlugin;
    private Dictionary<string, Dictionary<string, string>> getstr;
    private NKVMPlugin NkvmPlugin;

    [OneTimeSetUp]
    public void Setup()
    {
        displayPlugin = CreateInitializeDisplayMangerPlugin();
        vcpCorePlugin = CreateInitializeVcpCorePlugin();
        NkvmPlugin = CreateInitializeNkvmPlugin();
        PrivateObject privateObject = new PrivateObject(displayPlugin);
        PrivateObject privatevcp = new PrivateObject(vcpCorePlugin);
        privateObject.SetField("_VcpCorePlugin", vcpCorePlugin as IVcpCoreService);
        getstr = (Dictionary<string, Dictionary<string, string>>)privatevcp.GetField("_ColorPresets");
    }

    private class testpipclent
    {
        public void Testpipclient()
        {
            using (NamedPipeClientStream pipeClient =
                new NamedPipeClientStream(".", "VCPNamedPipe", PipeDirection.In))
            {
                // Connect to the pipe or wait until the pipe is available.
                pipeClient.Connect();
            }
        }
    }

    [Test]
    public void TestNetworkKVMPlugin()
    {
        Assert.IsNotNull(NkvmPlugin);
        PrivateObject privatevcolorPreset = new PrivateObject(NkvmPlugin);
        var agent2 = privatevcolorPreset.GetField("_agent") as IAgent;
        Assert.That(agent2, Is.EqualTo(NKVMPluginAgent.Object));
    }

    [Test]
    public void TestCreatNewNamedpipe()
    {
        NamedPipeServerStream pipeServer_;
        pipeServer_ = new NamedPipeServerStream("VCPNamedPipe");
        PrivateObject privatevNkvmPluginObject = new PrivateObject(NkvmPlugin);
        privatevNkvmPluginObject.SetFieldOrProperty("pipeServer", pipeServer_);
        List<MonitorInfo> _allInfoMonitors = new List<MonitorInfo>();
        _allInfoMonitors.Add(monitorInfo1);
        VcpCoreService.Setup(x => x.GetMonitors(It.IsAny<bool>())).Returns(Task.FromResult(_allInfoMonitors));
        var VcpCoreServiceObject = VcpCoreService.Object;
        privatevNkvmPluginObject.SetFieldOrProperty("_VcpCorePlugin", VcpCoreServiceObject);
        try
        {
            var t1 = new Task(() => { var result = NkvmPlugin.CreatNewNamedpipe(); });  //Task.Run(() => { var result = NkvmPlugin.CreatNewNamedpipe(); });
            t1.Start();  
            testpipclent testpipclent1 = new testpipclent();
            testpipclent1.Testpipclient();
            t1.Wait(3000);
            Assert.IsTrue(true);       
        }
        catch
        {
            Assert.Fail("not invoked");
        }    
    }

    [Test]
    public void TestIsNamedpipeConnected()
    {
        NamedPipeServerStream pipeServer_;
        pipeServer_ = new NamedPipeServerStream("testNamedpipeConnected");
        PrivateObject privatevNkvmPluginObject = new PrivateObject(NkvmPlugin);
        privatevNkvmPluginObject.SetFieldOrProperty("pipeServer", pipeServer_);
        var result= NkvmPlugin.IsNamedpipeConnected().Result;
        Assert.That(result, Is.False);
    }

    [Test]
    public void TestUpdateMonitorInfo()
    {
        List<MonitorInfo> monitorInfos1 = new List<MonitorInfo>();
        var UpdateMonitorInfoResult1 = NkvmPlugin.UpdateMonitorInfo(monitorInfos1); //Monitor no change
        Assert.IsNotNull(UpdateMonitorInfoResult1);

        List<MonitorInfo> monitorInfos2= new List<MonitorInfo>();
        monitorInfos2.Add(monitorInfo1);
        PrivateObject privatevNkvmPluginObject = new PrivateObject(NkvmPlugin);
        privatevNkvmPluginObject.SetFieldOrProperty("_AllInfoMonitors", monitorInfos2);
        var UpdateMonitorInfoResult2 = NkvmPlugin.UpdateMonitorInfo(monitorInfos2);  //No Supported KVM Monitors
        var allInfoMonitors=privatevNkvmPluginObject.GetFieldOrProperty("_AllInfoMonitors");
        Assert.IsNotNull (UpdateMonitorInfoResult2);
        Assert.That(monitorInfos2,Is.EqualTo(allInfoMonitors));

        MonitorInfo monitorInfo3 = new MonitorInfo()
        {
            AliasDeviceName = "Dell U2725DE(HDMI)",
            IsDellMonitor = true,
            Index = 0,
            CapabilityString = "(prot(monitor)type(LCD)model(U2424H)cmds(01 02 03 07 0C E3 F3)vcp(02 04 05 08 10 12 14(01 04 05 06 08 09 0B 0C)E5 E7(02 03) E2(00 02 04 0C 0D 0F) AC AE B2 B6 C6(01) C8 C9 CA CC(02 0A 03 04 08 09 0D 06 )",
            DisplayName = "DISPLAY8",
            DDCisON = true,
            FwVersion = "M3T101",
            inputSource = "HDMI-1",
            modelName = "U2725DE",
            series = "Dell UltraSharp (U) Series Monitors",
            //CapabilityDic = capabilityDic;
            CapabilityDic = new Dictionary<string, List<string>>() { { "C6", new List<string> { "01" } } },
            edid = new EDID()
            {
                ManufactureID = "DEL",
                VendorID = "42DC",
                Year = 2023,
                Month = 5,
                Week = 22,
                ModelName = "DELLU2725DE",
                EdidVersion = "V1.3",
                VideoInputType = "Digital Signal",
                Size = 27.1510868f,
                ServiceTag = "CN073K0",
                SerialNumber = "808597590",
                Edid = "00FFFFFFFFFFFF0010ACDC425538323016210103803C2278EA62A5AD5046AB240E5054A54B00714F8180A940D1C081C0A9C001010101565E00A0A0A029503020350055502100001A000000FF00434E3037334B300A2020202020000000FC0044454C4C20553237323444450A000000FD0030781EB23C000A20202020202001ED"

            },
        };
        List<string> supportedMonitorList_ = new List<string> { "Monitor1" };
        List<MonitorInfo> monitorInfos3 = new List<MonitorInfo>();
        monitorInfos3.Add(monitorInfo3);
        List<MonitorInfo> monitorInfos4 = new List<MonitorInfo>();
        monitorInfos4.Add(monitorInfo1);
        NamedPipeServerStream pipeServer_;
        pipeServer_ = new NamedPipeServerStream("testpipe1");
        privatevNkvmPluginObject.SetFieldOrProperty("_AllInfoMonitors", monitorInfos3);
        privatevNkvmPluginObject.SetFieldOrProperty("pipeServer", pipeServer_);
        privatevNkvmPluginObject.SetFieldOrProperty("_SupportedMonitors", supportedMonitorList_);
        var UpdateMonitorInfoResult3 = NkvmPlugin.UpdateMonitorInfo(monitorInfos4);  //Supported KVM Monitors
        var allInfoMonitors3 = privatevNkvmPluginObject.GetFieldOrProperty("_AllInfoMonitors");
        Assert.IsNotNull(UpdateMonitorInfoResult3);
        Assert.That(monitorInfos4, Is.EqualTo(allInfoMonitors3));
    }

    [Test]
    public void TestMonitorPlug()
    {
        NamedPipeServerStream pipeServer_;
        pipeServer_ = new NamedPipeServerStream("TestMonitorPlug");
        PrivateObject privatevNkvmPluginObject = new PrivateObject(NkvmPlugin);
        privatevNkvmPluginObject.SetFieldOrProperty("pipeServer", pipeServer_);
        string message = "One or more errors occurred. (Pipe hasn't been connected yet.)";
        try
        {
            var MonitorPlugResult = NkvmPlugin.MonitorPlug();
            Assert.IsNotNull(MonitorPlugResult);
        }
        catch (Exception ex) 
        {
            Assert.That(message,Is.EqualTo(ex.Message));
        }
    }

    [Test]
    public void TestToNKVM_SupportedMonitorList()
    {
        List<string> supportedMonitorList = new List<string> { "Monitor1", "Monitor2", };
        PrivateObject privatevNkvmPluginObject = new PrivateObject(NkvmPlugin);
        privatevNkvmPluginObject.SetFieldOrProperty("_SupportedMonitors", supportedMonitorList);
        var ToNKVM_SupportedMonitorListResult = NkvmPlugin.ToNKVM_SupportedMonitorList(supportedMonitorList);
        var supportedMonitorList_ = (List<string>)privatevNkvmPluginObject.GetFieldOrProperty("_SupportedMonitors");
        Assert.IsNotNull(ToNKVM_SupportedMonitorListResult);
        Assert.That(supportedMonitorList, Is.EqualTo(supportedMonitorList_));
    }

    [Test]
    public void TestUpdateSupportMonitors()
    {
        int count = 3;
        List<string> supportedMonitorList2 = new List<string> { "Monitor1", "Monitor2", "Monitor3" };
        PrivateObject privatevNkvmPluginObject = new PrivateObject(NkvmPlugin);
        privatevNkvmPluginObject.SetFieldOrProperty("_SupportedMonitors", supportedMonitorList2);
        var UpdateSupportMonitorsResult2 = NkvmPlugin.UpdateSupportMonitors().Result;    //_SupportedMonitors is not null
        var supportedMonitorList_ = (List<string>)privatevNkvmPluginObject.GetFieldOrProperty("_SupportedMonitors");
        Assert.That(count, Is.EqualTo(UpdateSupportMonitorsResult2.Count));
        Assert.That(supportedMonitorList2, Is.EqualTo(supportedMonitorList_));
    }

    [Test]
    public void TestGetSupportedNKVM()
    {
        List<MonitorInfo> GetSupportedNKVMmonitorInfos1 = new List<MonitorInfo>();
        GetSupportedNKVMmonitorInfos1.Add(monitorInfo1);
        List<string> SupportedMonitors1 = new List<string> { "Monitor1" };
        PrivateObject privatevNkvmPluginObject = new PrivateObject(NkvmPlugin);
        privatevNkvmPluginObject.SetFieldOrProperty("_AllInfoMonitors", GetSupportedNKVMmonitorInfos1);
        privatevNkvmPluginObject.SetFieldOrProperty("_SupportedMonitors", SupportedMonitors1);
        var GetSupportedNKVMResult1 = NkvmPlugin.GetSupportedNKVM().Result;  //No Supported KVM Monitors monitorInfo.CapabilityDic.No ContainsKey("C6")
        Assert.Greater(GetSupportedNKVMResult1.Count, 0);
        Assert.That(SupportedMonitors1, Is.EqualTo(GetSupportedNKVMResult1));

        MonitorInfo monitorInfo2 = new MonitorInfo()
        {
            AliasDeviceName = "Dell U2725DE(HDMI)",
            IsDellMonitor = true,
            Index = 0,
            CapabilityString = "(prot(monitor)type(LCD)model(U2424H)cmds(01 02 03 07 0C E3 F3)vcp(02 04 05 08 10 12 14(01 04 05 06 08 09 0B 0C)E5 E7(02 03) E2(00 02 04 0C 0D 0F) AC AE B2 B6 C6(01) C8 C9 CA CC(02 0A 03 04 08 09 0D 06 )",
            DisplayName = "DISPLAY8",
            DDCisON = true,
            FwVersion = "M3T101",
            inputSource = "HDMI-1",
            modelName = "U2725DE",
            series = "Dell UltraSharp (U) Series Monitors",
            CapabilityDic = new Dictionary<string, List<string>>() { { "C6", new List<string> { "01" } } },
            edid = new EDID()
        };
        List<string> GetSupportedNKVMmonitorInfos2 = new List<string> { "Monitor1", "U2725DE" };
        List<MonitorInfo> monitorInfos2 = new List<MonitorInfo>();
        monitorInfos2.Add(monitorInfo2);
        privatevNkvmPluginObject.SetFieldOrProperty("_AllInfoMonitors", monitorInfos2);
        var GetSupportedNKVMResult2 = NkvmPlugin.GetSupportedNKVM().Result;  //Supported KVM Monitors monitorInfo.CapabilityDic.ContainsKey("C6")
        Assert.IsNotNull(GetSupportedNKVMResult2);
        Assert.Greater(GetSupportedNKVMResult2.Count, 0);
        Assert.That(GetSupportedNKVMmonitorInfos2, Is.EqualTo(GetSupportedNKVMResult2));
    }

    [Test]
    public void TestOnNKVM()
    {
        NamedPipeServerStream pipeServer_;
        pipeServer_ = new NamedPipeServerStream("TestOnNKVM");
        PrivateObject privatevNkvmPluginObject = new PrivateObject(NkvmPlugin);
        privatevNkvmPluginObject.SetFieldOrProperty("pipeServer", pipeServer_);
        var OnNKVMResult = NkvmPlugin.OnNKVM();
        Assert.IsNotNull(OnNKVMResult);
    }

    [Test]
    public void TestOffNKVM()
    {
        NamedPipeServerStream pipeServer_;
        pipeServer_ = new NamedPipeServerStream("testOffNKVM");
        PrivateObject privatevNkvmPluginObject = new PrivateObject(NkvmPlugin);
        privatevNkvmPluginObject.SetFieldOrProperty("pipeServer", pipeServer_);
        var OffNKVMResult = NkvmPlugin.OffNKVM();
        Assert.IsNotNull(OffNKVMResult);
    }

    [Test]
    public void TestIsSupportMonitor()
    {
        bool isSupportMonitor_ = true;
        bool isNoSupportMonitor_ = false;
        monitorInfo1.modelName = "P5524Q";
        string[] validModelNames = { "P2424HEB", "P2725DEB", "P3424WEB", "P5524Q", "P5524QT", "P6524QT", "P7524QT", "P8624QT", "P5525QC" }; //isSupportMonitor
        if (validModelNames.Contains(monitorInfo1.modelName))
        {
            var isSupportMonitorResult = NkvmPlugin.isSupportMonitor(monitorInfo1).Result;
            Assert.That(isSupportMonitor_, Is.EqualTo(isSupportMonitorResult));
        }

        List<string> isNoSupportMonitor_1 = new List<string>() { };
        PrivateObject privatevNkvmPluginObject = new PrivateObject(NkvmPlugin);
        privatevNkvmPluginObject.SetFieldOrProperty("_SupportedMonitors", isNoSupportMonitor_1);
        monitorInfo1.modelName = "P5524E";
        var isNoSupportMonitorResult = NkvmPlugin.isSupportMonitor(monitorInfo1).Result;  //is NOt SupportMonitor not ContainsKey "C6"
        Assert.That(isNoSupportMonitor_, Is.EqualTo(isNoSupportMonitorResult));

        List<string> isSupportMonitor_2 = new List<string>() { "P5525Q1" };
        monitorInfo1.CapabilityString = "(prot(monitor)type(LCD)model(U2424H)cmds(01 02 03 07 0C E3 F3)vcp(02 04 05 08 10 12 14(01 04 05 06 08 09 0B 0C)E5 E7(02 03) E2(00 02 04 0C 0D 0F) AC AE B2 B6 C6(01) C8 C9 CA CC(02 0A 03 04 08 09 0D 06 )";
        string capabilityString_2 = monitorInfo1.CapabilityString;
        monitorInfo1.CapabilityDic = new Dictionary<string, List<string>>() { { "C6", new List<string> { "01" } } };
        privatevNkvmPluginObject.SetFieldOrProperty("_SupportedMonitors", isSupportMonitor_2);
        var isSupportMonitorResult2 = NkvmPlugin.isSupportMonitor(monitorInfo1).Result; //is SupportMonitor ContainsKey "C6"
        Assert.That(isSupportMonitor_, Is.EqualTo(isSupportMonitorResult2));
    }

    [Test]
    public void TestSetVCPNotify()
    {
        int Vcpcode = 0x60;
        int Value = 100;
        List<MonitorInfo> _allInfoMonitors = new List<MonitorInfo>();
        _allInfoMonitors.Add(monitorInfo1);
        VcpCoreService.Setup(x => x.GetMonitors(It.IsAny<bool>())).Returns(Task.FromResult(_allInfoMonitors));
        var VcpCoreServiceObject = VcpCoreService.Object;
        PrivateObject privatevNkvmPluginObject = new PrivateObject(NkvmPlugin);
        privatevNkvmPluginObject.SetField("_VcpCorePlugin", VcpCoreServiceObject);

        NamedPipeServerStream pipeServerSetVCPNotify_;
        pipeServerSetVCPNotify_ = new NamedPipeServerStream("SetVCPNotify");
        privatevNkvmPluginObject.SetFieldOrProperty("pipeServer", pipeServerSetVCPNotify_);

        var SetVCPNotifyResult = NkvmPlugin.SetVCPNotify(monitorInfo1, Vcpcode, Value);
        var get_allInfoMonitors = privatevNkvmPluginObject.GetFieldOrProperty("_AllInfoMonitors");
        Assert.IsNotNull(SetVCPNotifyResult);
        Assert.That(_allInfoMonitors, Is.EqualTo(get_allInfoMonitors));
    }

    [Test]
    public void TestToNKVM_HotkeySettings()
    {
        List<HotkeySettings> hotkeySettings = new List<HotkeySettings>()
        {
            new HotkeySettings()
            {
                HotkeyInfo = new List<HotkeyInfo>(),
                //DeviceInfo=monitorInfo1.edid,
                SerialNumber ="808597589",
                ServiceTag ="CN073K0",
                ModelName ="U2724DE",
                HotkeyOptions=new List<HotkeyOption>()
            }
        };
        var ToNKVM_HotkeySettingsResult = NkvmPlugin.ToNKVM_HotkeySettings(hotkeySettings);
        PrivateObject privatevNkvmPluginObject = new PrivateObject(NkvmPlugin);
        var get_HotkeySettings = privatevNkvmPluginObject.GetFieldOrProperty("_HotkeySettings");
        Assert.IsNotNull(ToNKVM_HotkeySettingsResult);
        Assert.That(hotkeySettings, Is.EqualTo(get_HotkeySettings));
    }

    [Test]
    public void TestSetHotkey()
    {
        HotkeyInfo hotkeyinfo1 = null;
        bool SetHotkey1 = false;
        bool SetHotkey2 = true;
        PrivateObject privatevNkvmPluginObject = new PrivateObject(NkvmPlugin);
        NamedPipeServerStream pipeServerSetHotkey_;
        pipeServerSetHotkey_ = new NamedPipeServerStream("SetHotkey");
        privatevNkvmPluginObject.SetFieldOrProperty("pipeServer", pipeServerSetHotkey_);

        if (hotkeyinfo1 == null)
        {
            var SetHotkeyResult1 = NkvmPlugin.SetHotkey(hotkeyinfo1).Result;
            Assert.That(SetHotkey1, Is.EqualTo(SetHotkeyResult1));
        }
        HotkeyInfo hotkeyinfo2 = new HotkeyInfo()
        {
            Description = "Hotkey key",
            Hotkey = new List<VirtualKey>() { VirtualKey.Control, VirtualKey.Shift, VirtualKey.A },
            Status = HotkeyStatus.Registered,
            InputSource = new List<InputSourceObj> { },
        };

        if (hotkeyinfo2 != null)
        {
            var SetHotkeyResult2 = NkvmPlugin.SetHotkey(hotkeyinfo2).Result;
            var get_HotkeyInfos = privatevNkvmPluginObject.GetFieldOrProperty("_HotkeyInfo");
            Assert.That(SetHotkey1, Is.EqualTo(SetHotkeyResult2));
            Assert.That(hotkeyinfo2, Is.EqualTo(get_HotkeyInfos));
        }
    }

    [Test]
    public void TestNKVM_ChangeLimitedSW()
    {
        bool isOn = false;
        List<MonitorInfo> _allInfoMonitors = new List<MonitorInfo>();
        _allInfoMonitors.Add(monitorInfo1);
        PrivateObject privatevNkvmPluginObject = new PrivateObject(NkvmPlugin);
        NamedPipeServerStream pipeServerChangeLimitedSW_;
        pipeServerChangeLimitedSW_ = new NamedPipeServerStream("ChangeLimitedSW");
        privatevNkvmPluginObject.SetFieldOrProperty("pipeServer", pipeServerChangeLimitedSW_);

        VcpCoreService.Setup(x => x.GetMonitors(It.IsAny<bool>())).Returns(Task.FromResult(_allInfoMonitors));
        var VcpCoreServiceObject = VcpCoreService.Object;
        privatevNkvmPluginObject.SetField("_VcpCorePlugin", VcpCoreServiceObject);

        var NKVM_ChangeLimitedSWResult = NkvmPlugin.NKVM_ChangeLimitedSW(monitorInfo1, isOn);
        var monitors = privatevNkvmPluginObject.GetFieldOrProperty("_AllInfoMonitors");
        Assert.IsNotNull(NKVM_ChangeLimitedSWResult);
        Assert.That(_allInfoMonitors, Is.EqualTo(monitors));
    }


    [OneTimeTearDown]
    public void TearDown()
    {
        displayPlugin.Dispose();
        vcpCorePlugin.Dispose();
        NkvmPlugin.Dispose();
    }

}