using System;
using System.Collections.Generic;
using static VcpCore.Common.User32;

namespace VcpCore.Common
{
    [Serializable]
    public class MonitorInfo_complex
    {
        public List<string> UnDefinedColorPreset;
        public Dictionary<string, Dictionary<string, string>> ColorPresentDescription;
        public Dictionary<string, List<string>> CapabilityDic;
        public string AliasDeviceName;
        public IntPtr Handle { get; set; }
        public bool IsDellMonitor { get; set; }
        public int Index { get; set; }
        public string CapabilityString { get; set; }
        public IntPtr hMonitor { get; set; }
        public IntPtr hPhysicalMonitor { get; set; }
        public string szPhysicalMonitorDescription { get; set; }
        public string DisplayName { get; set; }
        public bool DDCisON { get; set; }
        public int DDCCIFail { get; set; } = 0;
        public EDID edid { get; set; }
        public DISPLAY_DEVICE displaydevice { get; set; }
        public MonitorInfoEx pMonitorInfoEx { get; set; }
        public DEVMODE pDevmode { get; set; }
        public List<string> ColorPresetSupportList { get; set; }
        public string FwVersion { get; set; }
        public string inputSource { get; set; }
        public string inputCable { get; set; }
        public DISPLAYCONFIG_PATH_INFO pathInfoTarget { get; set; }
        public List<string> SmartHDRSupportList { get; set; }
        public string modelName { get; set; }
        public string series { get; set; }
        public string MarketingName { get; set; }
        public string ImageFileName { get; set; }
        public string SupplierID { get; set; }
        public string D_Ctrl { get; set; }
    }
}