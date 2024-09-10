using DDPM.SA.Common.Display;
using System.Collections.Generic;

namespace DDPM.SA.Common.Settings
{
    public class ImportVCP
    {
        public List<int> NotImportVCPs = new List<int>() { 0x02, 0x04, 0x05, 0x06, 0x08, 0xA, 0x60, 0xF0 };
        public List<int> ImportVCPSequence = new List<int>() { 0x66, 0x10, 0x12, /*0xF0,*/ 0xE9 };
    }

    public class Input
    {
        public string strInputSourceList { get; set; }
    }

    public class KVM
    {
        public string strUSBKVMPCsList { get; set; }
        public bool isOnUSBKVM { get; set; }
        public bool isOnNKVM { get; set; }
    }

    public class VCP
    {
        public int Code { get; set; }
        public List<int> Value { get; set; }

        public VCP()
        {
        }

        public VCP(int code, int value)
        {
            Code = code;
            Value = new List<int>();
            Value.Add(value);
        }

        public VCP(int code, List<int> value)
        {
            Code = code;
            Value = ((value == null) ? new List<int>() : new List<int>(value));
        }
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

        /// <summary>
        /// Custom layout items (up to 5 items), Default is empty.
        /// </summary>
        public List<SplitJson> CustomList { get; set; }

        /// <summary>
        /// The Recent list, the first item should be the SelectedSplit.
        /// So the SelectedSplit could be removed.
        /// </summary>
        public List<SplitJson> RecentList { get; set; }

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
    }

    public class DDPMMonitorSettings
    {
        public double Version { get; set; }
        public string Model { get; set; }
        public string ServiceTag { get; set; }
        public Input Input { get; set; } = new Input();
        public KVM KVM { get; set; } = new KVM();
        public List<VCP> VCPs { get; set; } = new List<VCP>();
        public EAMonitorSettings EA { get; set; } = new EAMonitorSettings();
    }
}