using System;
using System.Drawing;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace VcpCore.Common
{
    [Serializable]
    public static class User32
    {
        public delegate bool MonitorEnumDelegate(IntPtr hMonitor, IntPtr hdcMonitor, ref Rectangle lprcMonitor, IntPtr dwData);

        private const int MonitorinfofPrimary = 0x00000001;

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        [ResourceExposure(ResourceScope.None)]
        public static extern bool GetMonitorInfo(HandleRef hmonitor, [In, Out] MonitorInfoEx info);

        [DllImport("user32.dll", SetLastError = true)]
        //public static extern bool EnumDisplayMonitors(IntPtr hdc, IntPtr lpRect, MonitorEnumProc callback, int dwData);
        public static extern bool EnumDisplayMonitors(IntPtr hdc, IntPtr lprcClip, MonitorEnumDelegate lpfnEnum, IntPtr dwData);

        [DllImport("user32.dll", CharSet = CharSet.Ansi)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool EnumDisplaySettings(string lpszDeviceName, int iModeNum, ref DEVMODE lpDevMode);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern bool EnumDisplayDevices(string lpDevice, uint iDevNum, ref DISPLAY_DEVICE lpDisplayDevice, uint dwFlags);

        [Flags]
        public enum DisplayDeviceStateFlags
        {
            AttachedToDesktop = 1,
            MultiDriver = 2,
            PrimaryDevice = 4,
            MirroringDriver = 8,
            VGACompatible = 0x16,
            Removable = 0x20,
            ModesPruned = 0x8000000,
            Remote = 0x4000000,
            Disconnect = 0x2000000
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct Rect
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto, Pack = 4)]
        public class MonitorInfoEx
        {
            public int cbSize = Marshal.SizeOf(typeof(MonitorInfoEx));
            public Rect rcMonitor = new Rect();
            public Rect rcWork = new Rect();
            public int dwFlags = 0;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string szDevice;
        }

        [Flags]
        public enum DM
        {
            Orientation = 1,
            PaperSize = 2,
            PaperLength = 4,
            PaperWidth = 8,
            Scale = 0x10,
            Position = 0x20,
            NUP = 0x40,
            DisplayOrientation = 0x80,
            Copies = 0x100,
            DefaultSource = 0x200,
            PrintQuality = 0x400,
            Color = 0x800,
            Duplex = 0x1000,
            YResolution = 0x2000,
            TTOption = 0x4000,
            Collate = 0x8000,
            FormName = 0x10000,
            LogPixels = 0x20000,
            BitsPerPixel = 0x40000,
            PelsWidth = 0x80000,
            PelsHeight = 0x100000,
            DisplayFlags = 0x200000,
            DisplayFrequency = 0x400000,
            ICMMethod = 0x800000,
            ICMIntent = 0x1000000,
            MediaType = 0x2000000,
            DitherType = 0x4000000,
            PanningWidth = 0x8000000,
            PanningHeight = 0x10000000,
            DisplayFixedOutput = 0x20000000
        }

        public struct POINTL
        {
            public int x;

            public int y;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public struct DISPLAY_DEVICE
        {
            [MarshalAs(UnmanagedType.U4)]
            public int cb;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string DeviceName;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string DeviceString;

            [MarshalAs(UnmanagedType.U4)]
            public DisplayDeviceStateFlags StateFlags;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string DeviceID;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string DeviceKey;
        }

        [StructLayout(LayoutKind.Explicit)]
        public struct DEVMODE
        {
            public const int CCHDEVICENAME = 32;

            public const int CCHFORMNAME = 32;

            [FieldOffset(0)]
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string dmDeviceName;

            [FieldOffset(32)]
            public short dmSpecVersion;

            [FieldOffset(34)]
            public short dmDriverVersion;

            [FieldOffset(36)]
            public short dmSize;

            [FieldOffset(38)]
            public short dmDriverExtra;

            [FieldOffset(40)]
            public DM dmFields;

            [FieldOffset(44)]
            private short dmOrientation;

            [FieldOffset(46)]
            private short dmPaperSize;

            [FieldOffset(48)]
            private short dmPaperLength;

            [FieldOffset(50)]
            private short dmPaperWidth;

            [FieldOffset(52)]
            private short dmScale;

            [FieldOffset(54)]
            private short dmCopies;

            [FieldOffset(56)]
            private short dmDefaultSource;

            [FieldOffset(58)]
            private short dmPrintQuality;

            [FieldOffset(44)]
            public POINTL dmPosition;

            [FieldOffset(52)]
            public int dmDisplayOrientation;

            [FieldOffset(56)]
            public int dmDisplayFixedOutput;

            [FieldOffset(60)]
            public short dmColor;

            [FieldOffset(62)]
            public short dmDuplex;

            [FieldOffset(64)]
            public short dmYResolution;

            [FieldOffset(66)]
            public short dmTTOption;

            [FieldOffset(68)]
            public short dmCollate;

            [FieldOffset(72)]
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string dmFormName;

            [FieldOffset(102)]
            public short dmLogPixels;

            [FieldOffset(104)]
            public int dmBitsPerPel;

            [FieldOffset(108)]
            public int dmPelsWidth;

            [FieldOffset(112)]
            public int dmPelsHeight;

            [FieldOffset(116)]
            public int dmDisplayFlags;

            [FieldOffset(116)]
            public int dmNup;

            [FieldOffset(120)]
            public int dmDisplayFrequency;
        }

        public delegate bool MonitorEnumProc(IntPtr hDesktop, IntPtr hdc, ref Rect pRect, int dwData);

        public const int ENUM_CURRENT_SETTINGS = -1;

        #region DisplayConfig APIs
        [DllImport("user32")]
        public static extern int GetDisplayConfigBufferSizes(QDC flags, out int numPathArrayElements, out int numModeInfoArrayElements);

        [DllImport("user32")]
        public static extern int QueryDisplayConfig(QDC flags, ref int numPathArrayElements, [In, Out] DISPLAYCONFIG_PATH_INFO[] pathArray, ref int numModeInfoArrayElements, [In, Out] DISPLAYCONFIG_MODE_INFO[] modeInfoArray, out DISPLAYCONFIG_TOPOLOGY_ID currentTopologyId);

        [DllImport("user32")]
        public static extern int QueryDisplayConfig(QDC flags, ref int numPathArrayElements, [In, Out] DISPLAYCONFIG_PATH_INFO[] pathArray, ref int numModeInfoArrayElements, [In, Out] DISPLAYCONFIG_MODE_INFO[] modeInfoArray, IntPtr currentTopologyId);

        [DllImport("user32")]
        public static extern int DisplayConfigGetDeviceInfo(ref DISPLAYCONFIG_GET_ADVANCED_COLOR_INFO requestPacket);

        [DllImport("user32")]
        public static extern int DisplayConfigGetDeviceInfo(ref DISPLAYCONFIG_SOURCE_DEVICE_NAME requestPacket);

        [DllImport("user32")]
        public static extern int DisplayConfigGetDeviceInfo(ref DISPLAYCONFIG_TARGET_DEVICE_NAME requestPacket);

        [DllImport("user32.dll")]
        public static extern int DisplayConfigSetDeviceInfo(ref DISPLAYCONFIG_SET_ADVANCED_COLOR_STATE requestPacket);
        #endregion

        #region DisplayConfig Enum/Sturct
        public enum DISPLAYCONFIG_DEVICE_INFO_TYPE
        {
            DISPLAYCONFIG_DEVICE_INFO_GET_SOURCE_NAME = 1,
            DISPLAYCONFIG_DEVICE_INFO_GET_TARGET_NAME = 2,
            DISPLAYCONFIG_DEVICE_INFO_GET_TARGET_PREFERRED_MODE = 3,
            DISPLAYCONFIG_DEVICE_INFO_GET_ADAPTER_NAME = 4,
            DISPLAYCONFIG_DEVICE_INFO_SET_TARGET_PERSISTENCE = 5,
            DISPLAYCONFIG_DEVICE_INFO_GET_TARGET_BASE_TYPE = 6,
            DISPLAYCONFIG_DEVICE_INFO_GET_SUPPORT_VIRTUAL_RESOLUTION = 7,
            DISPLAYCONFIG_DEVICE_INFO_SET_SUPPORT_VIRTUAL_RESOLUTION = 8,
            DISPLAYCONFIG_DEVICE_INFO_GET_ADVANCED_COLOR_INFO = 9,
            DISPLAYCONFIG_DEVICE_INFO_SET_ADVANCED_COLOR_STATE = 10,
            DISPLAYCONFIG_DEVICE_INFO_GET_SDR_WHITE_LEVEL = 11,
            //Undocumented device info types
            DISPLAYCONFIG_DEVICE_INFO_GET_DPI_SCALE = -3,
            DISPLAYCONFIG_DEVICE_INFO_SET_DPI_SCALE = -4
        }

        public enum DISPLAYCONFIG_COLOR_ENCODING
        {
            DISPLAYCONFIG_COLOR_ENCODING_RGB = 0,
            DISPLAYCONFIG_COLOR_ENCODING_YCBCR444 = 1,
            DISPLAYCONFIG_COLOR_ENCODING_YCBCR422 = 2,
            DISPLAYCONFIG_COLOR_ENCODING_YCBCR420 = 3,
            DISPLAYCONFIG_COLOR_ENCODING_INTENSITY = 4,
        }

        public enum DISPLAYCONFIG_SCALING
        {
            DISPLAYCONFIG_SCALING_IDENTITY = 1,
            DISPLAYCONFIG_SCALING_CENTERED = 2,
            DISPLAYCONFIG_SCALING_STRETCHED = 3,
            DISPLAYCONFIG_SCALING_ASPECTRATIOCENTEREDMAX = 4,
            DISPLAYCONFIG_SCALING_CUSTOM = 5,
            DISPLAYCONFIG_SCALING_PREFERRED = 128,
        }

        public enum DISPLAYCONFIG_ROTATION
        {
            DISPLAYCONFIG_ROTATION_IDENTITY = 1,
            DISPLAYCONFIG_ROTATION_ROTATE90 = 2,
            DISPLAYCONFIG_ROTATION_ROTATE180 = 3,
        }

        public enum DISPLAYCONFIG_VIDEO_OUTPUT_TECHNOLOGY
        {
            DISPLAYCONFIG_OUTPUT_TECHNOLOGY_OTHER = -1,
            DISPLAYCONFIG_OUTPUT_TECHNOLOGY_HD15 = 0,
            DISPLAYCONFIG_OUTPUT_TECHNOLOGY_SVIDEO = 1,
            DISPLAYCONFIG_OUTPUT_TECHNOLOGY_COMPOSITE_VIDEO = 2,
            DISPLAYCONFIG_OUTPUT_TECHNOLOGY_COMPONENT_VIDEO = 3,
            DISPLAYCONFIG_OUTPUT_TECHNOLOGY_DVI = 4,
            DISPLAYCONFIG_OUTPUT_TECHNOLOGY_HDMI = 5,
            DISPLAYCONFIG_OUTPUT_TECHNOLOGY_LVDS = 6,
            DISPLAYCONFIG_OUTPUT_TECHNOLOGY_D_JPN = 8,
            DISPLAYCONFIG_OUTPUT_TECHNOLOGY_SDI = 9,
            DISPLAYCONFIG_OUTPUT_TECHNOLOGY_DISPLAYPORT_EXTERNAL = 10,
            DISPLAYCONFIG_OUTPUT_TECHNOLOGY_DISPLAYPORT_EMBEDDED = 11,
            DISPLAYCONFIG_OUTPUT_TECHNOLOGY_UDI_EXTERNAL = 12,
            DISPLAYCONFIG_OUTPUT_TECHNOLOGY_UDI_EMBEDDED = 13,
            DISPLAYCONFIG_OUTPUT_TECHNOLOGY_SDTVDONGLE = 14,
            DISPLAYCONFIG_OUTPUT_TECHNOLOGY_MIRACAST = 15,
            DISPLAYCONFIG_OUTPUT_TECHNOLOGY_INDIRECT_WIRED = 16,
            DISPLAYCONFIG_OUTPUT_TECHNOLOGY_INDIRECT_VIRTUAL = 17,
            DISPLAYCONFIG_OUTPUT_TECHNOLOGY_INTERNAL = unchecked((int)0x80000000),
        }

        public enum DISPLAYCONFIG_TOPOLOGY_ID
        {
            DISPLAYCONFIG_TOPOLOGY_INTERNAL = 0x00000001,
            DISPLAYCONFIG_TOPOLOGY_CLONE = 0x00000002,
            DISPLAYCONFIG_TOPOLOGY_EXTEND = 0x00000004,
            DISPLAYCONFIG_TOPOLOGY_EXTERNAL = 0x00000008,
        }

        public enum DISPLAYCONFIG_PATH
        {
            DISPLAYCONFIG_PATH_ACTIVE = 0x00000001,
            DISPLAYCONFIG_PATH_PREFERRED_UNSCALED = 0x00000004,
            DISPLAYCONFIG_PATH_SUPPORT_VIRTUAL_MODE = 0x00000008,
        }

        public enum DISPLAYCONFIG_SOURCE_FLAGS
        {
            DISPLAYCONFIG_SOURCE_IN_USE = 0x00000001,
        }

        public enum DISPLAYCONFIG_TARGET_FLAGS
        {
            DISPLAYCONFIG_TARGET_IN_USE = 0x00000001,
            DISPLAYCONFIG_TARGET_FORCIBLE = 0x00000002,
            DISPLAYCONFIG_TARGET_FORCED_AVAILABILITY_BOOT = 0x00000004,
            DISPLAYCONFIG_TARGET_FORCED_AVAILABILITY_PATH = 0x00000008,
            DISPLAYCONFIG_TARGET_FORCED_AVAILABILITY_SYSTEM = 0x00000010,
            DISPLAYCONFIG_TARGET_IS_HMD = 0x00000020,
        }

        public enum QDC
        {
            QDC_ALL_PATHS = 0x00000001,
            QDC_ONLY_ACTIVE_PATHS = 0x00000002,
            QDC_DATABASE_CURRENT = 0x00000004,
            QDC_VIRTUAL_MODE_AWARE = 0x00000010,
            QDC_INCLUDE_HMD = 0x00000020,
        }

        public enum DISPLAYCONFIG_SCANLINE_ORDERING
        {
            DISPLAYCONFIG_SCANLINE_ORDERING_UNSPECIFIED = 0,
            DISPLAYCONFIG_SCANLINE_ORDERING_PROGRESSIVE = 1,
            DISPLAYCONFIG_SCANLINE_ORDERING_INTERLACED = 2,
            DISPLAYCONFIG_SCANLINE_ORDERING_INTERLACED_UPPERFIELDFIRST = DISPLAYCONFIG_SCANLINE_ORDERING_INTERLACED,
            DISPLAYCONFIG_SCANLINE_ORDERING_INTERLACED_LOWERFIELDFIRST = 3,
        }

        public enum DISPLAYCONFIG_PIXELFORMAT
        {
            DISPLAYCONFIG_PIXELFORMAT_8BPP = 1,
            DISPLAYCONFIG_PIXELFORMAT_16BPP = 2,
            DISPLAYCONFIG_PIXELFORMAT_24BPP = 3,
            DISPLAYCONFIG_PIXELFORMAT_32BPP = 4,
            DISPLAYCONFIG_PIXELFORMAT_NONGDI = 5,
        }

        public enum DISPLAYCONFIG_MODE_INFO_TYPE
        {
            DISPLAYCONFIG_MODE_INFO_TYPE_SOURCE = 1,
            DISPLAYCONFIG_MODE_INFO_TYPE_TARGET = 2,
            DISPLAYCONFIG_MODE_INFO_TYPE_DESKTOP_IMAGE = 3,
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct DISPLAYCONFIG_DEVICE_INFO_HEADER
        {
            public DISPLAYCONFIG_DEVICE_INFO_TYPE type;
            public int size;
            public LUID adapterId;
            public uint id;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct DISPLAYCONFIG_GET_ADVANCED_COLOR_INFO
        {
            public DISPLAYCONFIG_DEVICE_INFO_HEADER header;
            public uint value;
            public DISPLAYCONFIG_COLOR_ENCODING colorEncoding;
            public int bitsPerColorChannel;

            public bool advancedColorSupported => (value & 0x1) == 0x1;
            public bool advancedColorEnabled => (value & 0x2) == 0x2;
            public bool wideColorEnforced => (value & 0x4) == 0x4;
            public bool advancedColorForceDisabled => (value & 0x8) == 0x8;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct LUID
        {
            public uint LowPart;
            public int HighPart;

            public long Value => ((long)HighPart << 32) | LowPart;
            public override string ToString() => Value.ToString();
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct DISPLAYCONFIG_SOURCE_MODE
        {
            public uint width;
            public uint height;
            public DISPLAYCONFIG_PIXELFORMAT pixelFormat;
            public POINTL position;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct DISPLAYCONFIG_RATIONAL
        {
            public uint Numerator;
            public uint Denominator;

            public override string ToString() => Numerator + " / " + Denominator;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct DISPLAYCONFIG_2DREGION
        {
            public uint cx;
            public uint cy;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct DISPLAYCONFIG_DESKTOP_IMAGE_INFO
        {
            public POINTL PathSourceSize;
            public RECT DesktopImageRegion;
            public RECT DesktopImageClip;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct DISPLAYCONFIG_VIDEO_SIGNAL_INFO
        {
            public ulong pixelRate;
            public DISPLAYCONFIG_RATIONAL hSyncFreq;
            public DISPLAYCONFIG_RATIONAL vSyncFreq;
            public DISPLAYCONFIG_2DREGION activeSize;
            public DISPLAYCONFIG_2DREGION totalSize;
            public uint videoStandard;
            public DISPLAYCONFIG_SCANLINE_ORDERING scanLineOrdering;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct DISPLAYCONFIG_TARGET_MODE
        {
            public DISPLAYCONFIG_VIDEO_SIGNAL_INFO targetVideoSignalInfo;
        }

        [StructLayout(LayoutKind.Explicit)]
        public struct DISPLAYCONFIG_MODE_INFO_union
        {
            [FieldOffset(0)]
            public DISPLAYCONFIG_TARGET_MODE targetMode;

            [FieldOffset(0)]
            public DISPLAYCONFIG_SOURCE_MODE sourceMode;

            [FieldOffset(0)]
            public DISPLAYCONFIG_DESKTOP_IMAGE_INFO desktopImageInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct DISPLAYCONFIG_PATH_SOURCE_INFO
        {
            public LUID adapterId;
            public uint id;
            public uint modeInfoIdx;
            public DISPLAYCONFIG_SOURCE_FLAGS statusFlags;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct DISPLAYCONFIG_PATH_TARGET_INFO
        {
            public LUID adapterId;
            public uint id;
            public uint modeInfoIdx;
            public DISPLAYCONFIG_VIDEO_OUTPUT_TECHNOLOGY outputTechnology;
            public DISPLAYCONFIG_ROTATION rotation;
            public DISPLAYCONFIG_SCALING scaling;
            public DISPLAYCONFIG_RATIONAL refreshRate;
            public DISPLAYCONFIG_SCANLINE_ORDERING scanLineOrdering;
            public bool targetAvailable;
            public DISPLAYCONFIG_TARGET_FLAGS statusFlags;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct DISPLAYCONFIG_PATH_INFO
        {
            public DISPLAYCONFIG_PATH_SOURCE_INFO sourceInfo;
            public DISPLAYCONFIG_PATH_TARGET_INFO targetInfo;
            public DISPLAYCONFIG_PATH flags;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct DISPLAYCONFIG_MODE_INFO
        {
            public DISPLAYCONFIG_MODE_INFO_TYPE infoType;
            public uint id;
            public LUID adapterId;
            public DISPLAYCONFIG_MODE_INFO_union info;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct DISPLAYCONFIG_SOURCE_DEVICE_NAME
        {
            public DISPLAYCONFIG_DEVICE_INFO_HEADER header;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string viewGdiDeviceName;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct DISPLAYCONFIG_TARGET_DEVICE_NAME_FLAGS
        {
            public uint value;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct DISPLAYCONFIG_TARGET_DEVICE_NAME
        {
            public DISPLAYCONFIG_DEVICE_INFO_HEADER header;
            public DISPLAYCONFIG_TARGET_DEVICE_NAME_FLAGS flags;
            public DISPLAYCONFIG_VIDEO_OUTPUT_TECHNOLOGY outputTechnology;
            public ushort edidManufactureId; // Hex.
            public ushort edidProductCodeId; // int
            public uint connectorInstance;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
            public string monitorFriendlyDeviceName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string monitorDevicePat;
        }

        public struct DISPLAYCONFIG_SET_ADVANCED_COLOR_STATE
        {
            public DISPLAYCONFIG_DEVICE_INFO_HEADER header;
            public bool advancedColorEnabled;
        }


        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }
        #endregion

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public extern static int FormatMessage(int flag, ref IntPtr source, int msgid, int langid, ref string buff, int size, ref IntPtr args);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern long GetLastError();

        private static readonly char[] TrimChar = { '\r', '\n' };// Dean 0626 SAST issue
        public static string GetLastErrMsg()
        {
            IntPtr TempPtr = IntPtr.Zero;
            string Msg = null;
            FormatMessage(0x1300, ref TempPtr, (int)GetLastError(), 0, ref Msg, 255, ref TempPtr);
            return Msg.Trim(TrimChar);
        }
    }
}
