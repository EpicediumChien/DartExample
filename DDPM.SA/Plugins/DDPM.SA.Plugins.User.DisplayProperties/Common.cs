using System.Collections;
using VcpCore.Common;

namespace DDPM.SA.Plugins.User.DisplayProperties
{
    public class NoSroHashTable : Hashtable
    {
        private ArrayList list = new ArrayList();

        public override void Add(object key, object? value)
        {
            base.Add(key, value);
            list.Add(key);
        }

        public override void Clear()
        {
            base.Clear();
            list.Clear();
        }

        public override void Remove(object key)
        {
            base.Remove(key);
            list.Remove(key);
        }

        public override ICollection Keys
        {
            get
            {
                return list;
            }
        }
    }

    public enum VcpCode : byte
    {
        None = 0,
        RestoreFactoryDefaults = 4,
        RestoreFactoryLuminanceContrastDefaults = 5,
        RestoreFactoryColorDefaults = 8,
        Brightness = 16,
        Contrast = 18,
        BacklightControl = 19,
        ColorSpaceE2 = 226,
        ColorSpace14 = 20,
        ColorSpaceDC = 220,
        ColorSpaceF0 = 240,
        UserColorTemperatureIncrement = 11,
        UserColorTemperature = 12,
        Clock = 14,
        FleshToneEnhancement = 17,
        ImageMode = 219,
        VideoGainRed = 22,
        UserColorVisionCompensation = 23,
        VideoGainGreen = 24,
        VideoGainBlue = 26,
        Focus = 28,
        AutoSetup = 30,
        AutoColorSetup = 31,
        GrayScaleExpansion = 46,
        ClockPhase = 62,
        HorizontalMoire = 86,
        VerticalMoire = 88,
        SixAxisSaturationControlRed = 89,
        SixAxisSaturationControlYellow = 90,
        SixAxisSaturationControlGreen = 91,
        SixAxisSaturationControlCyan = 92,
        SixAxisSaturationControlBlue = 93,
        SixAxisSaturationControlMagenta = 94,
        BacklightLevelWhite = 107,
        BacklightLevelRed = 109,
        BacklightLevelGreen = 111,
        BacklightLevelBlue = 113,
        VideoBlackLevelRed = 108,
        VideoBlackLevelGreen = 110,
        VideoBlackLevelBlue = 112,
        Gamma = 114,
        BlockLUTOperation = 117,
        AdjustZoom = 124,
        Sharpness = 135,
        VelocityScanModulation = 136,
        ColorSaturation = 138,
        TVSharpness = 140,
        TVContrast = 142,
        Hue = 144,
        TVBlackLevelLuminance = 146,
        WindowBackground = 154,
        SixAxisHueControlRed = 155,
        SixAxisHueControlYellow = 156,
        SixAxisHueControlGreen = 157,
        SixAxisHueControlCyan = 158,
        SixAxisHueControlBlue = 159,
        SixAxisHueControlMagenta = 160,
        AutoSetupOnOff = 162,
        WindowMaskControl = 164,
        WindowSelect = 165,
        WindowSize = 166,
        WindowTransparency = 167,
        StereoVideoMode = 212,
        MiscBitSupport1 = 241,
        MiscBitSupport2 = 242,
        LUTSize = 115,
        SinglePointLUTOperation = 116,
        Rotate = 170,
        HorizontalFrequency = 172,
        VerticalFrequency = 174,

        //WindowPos = 232,
        DisplayUsageTime = 192,

        PowerMode = 214,
        OSDButtonControl = 202,
        ScratchPad = 222,
        VCPVersion = 223,
        TechnologyType = 182,
        SourceTimingMode = 180,
        ReduceBrightness = 224,
        SuspendMonitor = 225,
        USBUpstreamAssociation = 231,
        VisionEngineSwitch = 236,
        VisionEngineAdjustment = 237,
        USBUplinkPorts = 238,
        InputSelect = 96,
        OsdQuery = 2,
        ReadStack = 82,
        Version = 201,
        OSDLanguage = 204,
        Settings = 176,
        SourceColorCoding = 181,
        UniformityCalibrated = 228,
        PIPPBPInput = 232,
        PIPPBPMode = 233,
        PIPPBPVideoSwap = 229,
        SmartHDR = 234,
        MultiFunctionF5 = 254,
        FirmwareVersion = 253,
        DisplayControllerID = 200,
        SpeakerVolume = 98,
        Zoom = 229,
        AccessoryModuleCommand = 235,

        //VisionEngineControls = 236,
        //VisionEngineProfilesControls= 237,
        MultiMonitorSync = 239,

        GamingFeatures = 244,
        ModeSwitch = 245
    }

    /*public static class CommonApi
    {
        //[DllImport("Shcore.dll", SetLastError = true)]
        //internal static extern int GetDpiForMonitor(IntPtr hmonitor, Monitor_DPI_Type dpiType, out uint dpiX, out uint dpiY);

        internal enum Monitor_DPI_Type : int
        {
            MDT_Effective_DPI = 0,
            MDT_Angular_DPI = 1,
            MDT_Raw_DPI = 2,
            MDT_Default = MDT_Effective_DPI
        }
    }*/

    public class DDMiMessagingMsg
    {
        public string OpType;
        public string message;

        public DDMiMessagingMsg(string optype, string msg)
        {
            this.OpType = optype;
            this.message = msg;
        }
    }

    public class SendNameMsg : DDMiMessagingMsg
    {
        public string sender;

        public SendNameMsg(string _sender, string _op, string _message) : base(_op, _message)
        {
            sender = _sender;
        }
    }

    public class PipeNameMsg : SendNameMsg
    {
        //public int MonitorInfoIndex;
        public EDID MonitorEDID;

        public PipeNameMsg(string _sender, string _op, string _message, EDID _edid) : base(_sender, _op, _message)
        {
            MonitorEDID = _edid;
        }
    }

    public class DDMBorkerMsg : SendNameMsg
    {
        public EDID DeviceInfo;

        public DDMBorkerMsg(string _sender, string _op, string _message, EDID _devInfo) : base(_sender, _op, _message)
        {
            DeviceInfo = _devInfo;
        }

        public override string ToString()
        {
            return $"Sender:{sender}, OP:{OpType}, Msg:{message}, DevInfo:{DeviceInfo.ToString()}";
        }
    }
}