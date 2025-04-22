using DDPM.SA.Common.Display;
using System; //For Array
using System.Collections.Generic;
using VcpCore.Common;

namespace DDPM.SA.Common.Settings
{
    public class ImportVCP
    {
        public List<int> NotImportVCPs = new List<int>() { 0x02, 0x04, 0x05, 0x06, 0x08, 0xA, 0x60, 0xF0, 0xF4, 0xEC, 0xE2, 0xF1, 0xF2 };
        public List<int> ImportVCPSequence = new List<int>() { 0x66, 0x10, 0x12, /*0xF0,*/ 0xE9 };
    }

    public class InputSource
    {
        public string strInputSourceList { get; set; } = string.Empty;
    }

    public class KVMSettings
    {
        public string strUSBKVMPCsList { get; set; } = string.Empty;
        public bool isOnUSBKVM { get; set; } = false;
        public bool isOnNKVM { get; set; } = false;
        public bool isNoKVM { get; set; } = false;
    }

    public class VCPCode
    {
        public int Code { get; set; } = 0;
        public List<int> Value { get; set; } = new List<int>();

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
        public Gaming_GameEnhancementMode Current_GameEnhancementMode { get; set; } = Gaming_GameEnhancementMode.Off;
        public Gaming_ResponseTime Current_ResponseTime { get; set; } = Gaming_ResponseTime.Disable;
        public Gaming_DarkStabilizer Current_DarkStabilizer { get; set; } = Gaming_DarkStabilizer.Disable;
        public Gaming_HDRType Current_HDRType { get; set; } = Gaming_HDRType.Off;
        public Gaming_DualResolutionType Current_DualResolutionType { get; set; } = Gaming_DualResolutionType.Unknow;
        public bool[] IsEnable_VisionEngineType { get; set; } = new bool[0];
    }

    /// <summary>
    /// EasyArrange per-monitor settings. The SplitJson class is defined in DDPM.SA.Common/Display folder.
    /// </summary>
    public class EAMonitorSettings
    {
        //Robert_Lin 2025-4-8 new added for DDPMMonitorSettings Version=1.0
        /// <summary>
        /// The property from MonitorInfo.edit.Instance. When a single monitor can be
        /// partition to multiple instances. (For example, U4323QE model.)
        /// </summary>
        public string Instance { get; set; } = String.Empty;

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

        //Robert_Lin 2025-4-21 added for checking migration
        /// <summary>
        /// Check if the specified Instance is migrated from DDM
        /// It will be "AAAAAAAA-AAAA-AAAA-AAAA-AAAAAAAAAAAA" if it's migrated from DDM.
        /// </summary>
        /// <param name="instance"></param>
        /// <returns></returns>
        public static bool IsMigratedFromDDM(string instance)
        {
            return (instance.Equals("AAAAAAAA-AAAA-AAAA-AAAA-AAAAAAAAAAAA"));
        }

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

    //Robert_Lin 2025-4-8 Change to Version=1 (from 0)
    //Add "EA1" property to replace "EA"
    public class DDPMMonitorSettings
    {
        public double Version { get; set; } = 1;
        public string Model { get; set; } = string.Empty;
        public string ServiceTag { get; set; } = string.Empty;
        public InputSource Input { get; set; } = new InputSource();
        public KVMSettings KVM { get; set; } = new KVMSettings();
        public List<VCPCode> VCPs { get; set; } = new List<VCPCode>();
        public EAMonitorSettings EA { get; set; } = new EAMonitorSettings();
        public ColorPresetSettings ColorPreset { get; set; } = new ColorPresetSettings();
        public DisplayCurrentPropertiesInfo DisplayPropertiesInfo { get; set; } = new DisplayCurrentPropertiesInfo();
        public List<HotkeyData> hotkeyData { get; set; } = new List<HotkeyData>();//1006 add for input source hotkey settings per monitor
        public scheduleInfo scheduleInfo { get; set; } = new scheduleInfo();
        public ImpExpSettings ImpExpSettings { get; set; } = new ImpExpSettings();
        public EasyArrangementDDPM easyArrangementDDPM { get; set; } = new EasyArrangementDDPM();
        public uint ALSConfig { get; set; } = 0;
        public Gaming Gaming { get; set; } = new Gaming();
        public PowerNapSetting PowerNap { get; set; } = new PowerNapSetting();//1126 move powerNap setting to here
        public HotkeyOption HotkeyOption { get; set; } = HotkeyOption.None; //20250408 move  USBkvm hotkey: ”auto swtich USB upstream port in PBP side-by-side mode“ setting to here
        //Robert_Lin 2025-4-8 new added for DDPMMonitorSettings Version=1.0
        public EAMonitorSettings[] EA1 { get; set; } = Array.Empty<EAMonitorSettings>();
       //Convert v0 to v1 which will copy EA to EA1[0] if EA1 is empty.
        public void ConvertV0ToV1()
        {
            Version = 1;
            if (EA != null)
            {
                if ((EA1 == null) || (EA1.Length == 0))
                {
                    EA1 = new EAMonitorSettings[1];
                    EA1[0] = EA;
                }
                //else
                //{
                //    EA1[0] = EA;
                //}
            }
        }
    }

    public class HotkeyData
    {
        public HotkeyType hotkeyType = HotkeyType.None;
        public List<InputSourceObj> inputSource { get; set; } = new List<InputSourceObj>();
    }
}