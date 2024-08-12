using DDPM.UI.Common;
using static DDPM.UI.Common.User32;

namespace DDPM.UI.Common {
  [Serializable]

  //TO be deleted
  public class MonitorInfo_Unused {
    //public string monitorName { get; set; }

    public List<string> UnDefinedColorPreset;
    public NoSroHashTable ColorPresentDescription;
    public Dictionary<string, List<string>> CapabilityDic;
    public string AliasDeviceName;
    public IntPtr Handle { get; set; }
    public bool IsDellMonitor { get; set; }
    public string CapabilityString { get; set; }
    public IntPtr hMonitor { get; set; }
    public IntPtr hPhysicalMonitor { get; set; }
    public string szPhysicalMonitorDescription { get; set; }
    public string DisplayName { get; set; }
    public bool DDCisON { get; set; }
    public EDID edid { get; set; }
    public DISPLAY_DEVICE displaydevice { get; set; }
    public MonitorInfoEx pMonitorInfoEx { get; set; }
    public DEVMODE pDevmode { get; set; }
    public List<string> ColorPresetSupportList { get; set; }
  }
}
