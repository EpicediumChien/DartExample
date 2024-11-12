using System;
using System.Collections.Generic;
using static VcpCore.Common.User32;

namespace VcpCore.Common
{
    [Serializable]
    public class MonitorInfo_complex
    {
        public List<string> UnDefinedColorPreset = new List<string>();
        public Dictionary<string, Dictionary<string, string>> ColorPresentDescription = new Dictionary<string, Dictionary<string, string>>();
        public Dictionary<string, List<string>> CapabilityDic = new Dictionary<string, List<string>>();
        public string AliasDeviceName = string.Empty;
        public IntPtr Handle { get; set; } = new IntPtr();
        public bool IsDellMonitor { get; set; } = false;
        public int Index { get; set; } = 0x0;
        public string CapabilityString { get; set; } = string.Empty;
        public IntPtr hMonitor { get; set; } = new IntPtr();
        public IntPtr hPhysicalMonitor { get; set; } = new IntPtr();
        public string szPhysicalMonitorDescription { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public bool DDCisON { get; set; } = false;
        public int DDCCIFail { get; set; } = 0x0;
        public EDID edid { get; set; } = new EDID();
        public DISPLAY_DEVICE displaydevice { get; set; } = new DISPLAY_DEVICE();
        public MonitorInfoEx pMonitorInfoEx { get; set; } = new MonitorInfoEx();
        public DEVMODE pDevmode { get; set; } = new DEVMODE();
        public List<string> ColorPresetSupportList { get; set; } = new List<string>();
        public string FwVersion { get; set; } = string.Empty;
        public string inputSource { get; set; } = string.Empty;
        public string inputCable { get; set; } = string.Empty;
        public DISPLAYCONFIG_PATH_INFO pathInfoTarget { get; set; } = new DISPLAYCONFIG_PATH_INFO();
        public List<string> SmartHDRSupportList { get; set; } = new List<string>();
        public string modelName { get; set; } = string.Empty;
        public string series { get; set; } = string.Empty;
        public string MarketingName { get; set; } = string.Empty;
        public string ImageFileName { get; set; } = string.Empty;
        public string SupplierID { get; set; } = string.Empty;
        public string D_Ctrl { get; set; } = string.Empty;
        public double scalingFactor { get; set; } = 0x0;
    }
}