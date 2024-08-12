using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;
using VcpCore.Common;
using Windows.System;

namespace DDPM.SA.Common.Display
{
    public class HotkeySettings
    {
        public EDID DeviceInfo { get; set; }
        public List<HotkeyOption> HotkeyOptions { get; set; } = new List<HotkeyOption>();

        public List<HotkeyInfo> HotkeyInfo { get; set; }

        public HotkeySettings()
        {
            HotkeyInfo = new List<HotkeyInfo>();
        }
    }

    public class HotkeyInfo
    {
        public string Description { get; set; }

        public List<VirtualKey> Hotkey { get; set; }

        [JsonIgnore]
        public HotkeyStatus Status { get; set; }

        public HotkeyType Job = HotkeyType.None;

        public List<InputSourceObj>  InputSource { get; set; } = new List<InputSourceObj>();

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
        KvmChangePIPPosition
    }

}
