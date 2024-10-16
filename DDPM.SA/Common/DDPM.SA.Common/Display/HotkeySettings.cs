using Newtonsoft.Json;
using System.Collections.Generic;
using VcpCore.Common;
using Windows.System;

namespace DDPM.SA.Common.Display
{
    public class HotkeySettings
    {
        //public EDID DeviceInfo { get; set; }
        public string ModelName { get; set; }
        public string SerialNumber { get; set; }
        public string ServiceTag { get; set; }
        public List<HotkeyOption> HotkeyOptions { get; set; } = new List<HotkeyOption>();

        public List<HotkeyInfo> HotkeyInfo { get; set; }

        public HotkeySettings()
        {
            HotkeyInfo = new List<HotkeyInfo>();
        }
    }

    public class HotkeyPopWrap
    {
        public MonitorInfo monitorInfo { get; set; }
        public HotkeyType hotkeyType { get; set; }
    }

    public class HotkeyInfo
    {
        public string Description { get; set; }

        public List<VirtualKey> Hotkey { get; set; }

        [JsonIgnore]
        public HotkeyStatus Status { get; set; }

        public HotkeyType Job = HotkeyType.None;

        //[1006 Dean] since currently we using this field to be per user, so the real input source object should be recorded as monitor setting as well
        public List<InputSourceObj> InputSource { get; set; } = new List<InputSourceObj>();
    }

    public enum HotkeyOption
    {
        None,

        //kvm hotkey: auto swtich USB upstream port in PBP side-by-side mode
        KvmAutoApply,

        //automatically apply the settings when the same model is detected
        PowerNapAutoApply
    }

    public enum HotkeyStatus
    {
        Registered,
        Failed,
        NotConfigured
    }

    public enum HotkeyWarning
    {
        None,
        SingleKey,
        ConflictInbox,
        ConflictApps,
        NotConfigured
    }

    public enum HotkeyType
    {
        None,
        BrightnessReduce,
        BrightnessIncrease,
        ContrastReduce,
        ContrastIncrease,
        LuminanceReduce,
        LuminanceIncrease,
        ToggleInputSource,
        FavoriteInputSource,
        SwitchInputSource,
        SwapIputPIPPBP,
        ChangePIPPosition,
        KvmSwitchInputSource,
        KvmSwitchKbMsKey,
        KvmChangePIPPosition,
        DarkStabilizerToggle,
        DualResolutionToggle,
        VisionEngineToggle,
        NkvmConflict,
        LockBriCont,
        LockActiveInputSource,
        ToggleEzRecentSetting
    }

    public class DDMtoDDPM
    {
        public Dictionary<int, HotkeyType> HotkeyMap = new Dictionary<int, HotkeyType>()
        {
            {7, HotkeyType.SwapIputPIPPBP },
            {0, HotkeyType.ChangePIPPosition },
            {1, HotkeyType.None },
            {2, HotkeyType.None},
            {3, HotkeyType.VisionEngineToggle},
            {4, HotkeyType.DarkStabilizerToggle},
            {20, HotkeyType.DualResolutionToggle},
            {5, HotkeyType.KvmSwitchInputSource },
            {6, HotkeyType.KvmSwitchKbMsKey },
            //{8, HotkeyType.None},
            //{9, HotkeyType.None },
            {10, HotkeyType.None},
            {12, HotkeyType.SwitchInputSource},
            {13, HotkeyType.ToggleInputSource},
            {14, HotkeyType.FavoriteInputSource},
            {15, HotkeyType.BrightnessIncrease},
            {16, HotkeyType.BrightnessReduce},
            {17, HotkeyType.ContrastIncrease},
            {18, HotkeyType.ContrastReduce},
            //{11, HotkeyType.None },
            //{19, HotkeyType.None},
        };
    }
}