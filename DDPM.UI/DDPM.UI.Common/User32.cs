using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace DDPM.UI.Common
{
    [Serializable]
    public static class User32
    {
        private const int MonitorinfofPrimary = 0x00000001;

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        [ResourceExposure(ResourceScope.None)]
        public static extern bool GetMonitorInfo(HandleRef hmonitor, [In, Out] MonitorInfoEx info);

        [DllImport("user32", SetLastError = true)]
        public static extern bool EnumDisplayMonitors(IntPtr hdc, IntPtr lpRect, MonitorEnumProc callback, int dwData);

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
    }
}