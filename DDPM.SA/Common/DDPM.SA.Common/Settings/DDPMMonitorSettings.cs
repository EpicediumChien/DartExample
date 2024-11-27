using DDPM.SA.Common.Display;
using System.Collections.Generic;
using VcpCore.Common;

namespace DDPM.SA.Common.Settings
{
    public class ImportVCP
    {
        public List<int> NotImportVCPs = new List<int>() { 0x02, 0x04, 0x05, 0x06, 0x08, 0xA, 0x60, 0xF0, 0xF4, 0xEC };
        public List<int> ImportVCPSequence = new List<int>() { 0x66, 0x10, 0x12, /*0xF0,*/ 0xE9 };
    }

    public class InputSource
    {
        public string strInputSourceList { get; set; }
    }

    public class KVMSettings
    {
        public string strUSBKVMPCsList { get; set; }
        public bool isOnUSBKVM { get; set; }
        public bool isOnNKVM { get; set; }
    }

    public class VCPCode
    {
        public int Code { get; set; }
        public List<int> Value { get; set; }

        public VCPCode()
        {
        }

        public VCPCode(int code, int value)
        {
            Code = code;
            Value = new List<int>();
            Value.Add(value);
        }

        public VCPCode(int code, List<int> value)
        {
            Code = code;
            Value = ((value == null) ? new List<int>() : new List<int>(value));
        }
    }

    public class Gaming()
    {
        public Gaming_GameEnhancementMode Current_GameEnhancementMode { get; set; } = new Gaming_GameEnhancementMode();
        public Gaming_ResponseTime Current_ResponseTime { get; set; } = new Gaming_ResponseTime();
        public Gaming_DarkStabilizer Current_DarkStabilizer { get; set; } = new Gaming_DarkStabilizer();
        public Gaming_HDRType Current_HDRType { get; set; } = new Gaming_HDRType();
        public Gaming_DualResolutionType Current_DualResolutionType { get; set; } = new Gaming_DualResolutionType();
        public bool[] IsEnable_VisionEngineType { get; set; } = new bool[0];
    }

    /// <summary>
    /// EasyArrange per-monitor settings. The SplitJson class is defined in DDPM.SA.Common/Display folder.
    /// </summary>
    public class EAMonitorSettings
    {
        /// <summary>
        /// Current user selected Split item. Defaul is (CellCount=0, SplitKey='A')
        /// </summary>
        public SplitJson SelectedSplit { get; set; } = new SplitJson(); //Default will be '0A'

        //Robert_Lin, 2024-10-12 move to UserSerrings
        /// <summary>
        /// Custom layout items (up to 5 items), Default is empty.
        /// </summary>
        //public List<SplitJson> CustomList { get; set; }

        //Robert_Lin, 2024-10-10, dont provide default list in a get/set property, it would cause double items issue
        // https://stackoverflow.com/questions/13394401/json-net-deserializing-list-gives-duplicate-items
        //In , if we found that RecentList is empty, then return the default list.
        /// <summary>
        /// The Recent list, the first item should be the SelectedSplit.
        /// So the SelectedSplit could be removed.
        /// </summary>
        //public List<SplitJson> RecentList { get; set; } //= SplitJson.DefaultRecentList;
        public SplitJson[] RecentList { get; set; }

        //Robert_Lin, 2024-9-18 Move these flags to DDPMUserSettings
        /*
        /// <summary>
        /// The setting of "Allow app to split side by side without gap" in Easy Arrange / Settings page.
        /// The defualt value is True.
        /// </summary>
        public bool IsWidthoutGap { get; set; } = true;

        /// <summary>
        /// The setting of "Only allow zone positioning when SHIFT is pressed" in Easy Arrange / Settings page.
        /// The defualt value is False.
        /// </summary>
        public bool IsOnlyAllowWhenShiftKeyPressed { get; set; } = false;

        /// <summary>
        /// The setting of "Span across multiple monitors" in Easy Arrange / Settings page.
        /// The defualt value is False.
        /// </summary>
        public bool? IsSpanAcrossMultiMonitors { get; set; } = false;

        /// <summary>
        /// The settings of "Application Window Snap" in Easy Arrange / Settings page.
        /// </summary>
        public bool IsAwsEnabled { get; set; } = false;
        */
    }

    public class ImpExpSettings
    {
        public bool SameModel { get; set; } = false;
    }

    public class DDPMMonitorSettings
    {
        public double Version { get; set; }
        public string Model { get; set; }
        public string ServiceTag { get; set; }
        public InputSource Input { get; set; } = new InputSource();
        public KVMSettings KVM { get; set; } = new KVMSettings();
        public List<VCPCode> VCPs { get; set; } = new List<VCPCode>();
        public EAMonitorSettings EA { get; set; } = new EAMonitorSettings();
        public ColorPresetSettings ColorPreset { get; set; } = new ColorPresetSettings();
        public DisplayCurrentPropertiesInfo DisplayPropertiesInfo { get; set; }
        public HotkeySettings hotkeySettings { get; set; }
        public List<HotkeyData> hotkeyData { get; set; } = new List<HotkeyData>();//1006 add for input source hotkey settings per monitor
        public scheduleInfo scheduleInfo { get; set; }
        public ImpExpSettings ImpExpSettings { get; set; }
        public EasyArrangementDDPM easyArrangementDDPM { get; set; }
        public uint ALSConfig { get; set; } = 0;
        public Gaming Gaming { get; set; } = new Gaming();
        public PowerNapSetting PowerNap { get; set; }//1126 move powerNap setting to here
    }

    public class HotkeyData
    {
        public HotkeyType hotkeyType = HotkeyType.None;
        public List<InputSourceObj> inputSource { get; set; } = new List<InputSourceObj>();
    }
}