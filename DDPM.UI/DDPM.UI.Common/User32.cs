using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Text;
using static VcpCore.Common.User32;

namespace DDPM.UI.Common
{
    [Serializable]
    public static class User32
    {
        private const int MonitorinfofPrimary = 0x00000001;

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [ResourceExposure(ResourceScope.None)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        private static extern bool GetMonitorInfo(HandleRef hmonitor, [In, Out] MonitorInfoEx info);
        public static bool _GetMonitorInfo(HandleRef hmonitor, [In, Out] MonitorInfoEx info)
        {
            return GetMonitorInfo(hmonitor, info);
        }

        [DllImport("user32", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        private static extern bool EnumDisplayMonitors(IntPtr hdc, IntPtr lpRect, MonitorEnumProc callback, int dwData);
        public static bool _EnumDisplayMonitors(IntPtr hdc, IntPtr lpRect, MonitorEnumProc callback, int dwData)
        {
            return EnumDisplayMonitors(hdc, lpRect, callback, dwData);
        }

        [DllImport("user32.dll", CharSet = CharSet.Ansi, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        private static extern bool EnumDisplaySettings(string lpszDeviceName, int iModeNum, ref DEVMODE lpDevMode);
        public static bool _EnumDisplaySettings(string lpszDeviceName, int iModeNum, ref DEVMODE lpDevMode)
        {
            return EnumDisplaySettings(lpszDeviceName, iModeNum, ref lpDevMode);
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        private static extern bool EnumDisplayDevices(string lpDevice, uint iDevNum, ref DISPLAY_DEVICE lpDisplayDevice, uint dwFlags);
        public static bool _EnumDisplayDevices(string lpDevice, uint iDevNum, ref DISPLAY_DEVICE lpDisplayDevice, uint dwFlags)
        {
            return EnumDisplayDevices(lpDevice, iDevNum, ref lpDisplayDevice, dwFlags);
        }

        // << 250102 added by Hess to change MousePrimaryButton
        private const uint SPI_GETMOUSEBUTTONSWAP = 0x0023;
        private const uint SPI_SETMOUSEBUTTONSWAP = 0x0021;
        private const uint SPIF_SENDCHANGE = 0x0002;
        private const int SM_SWAPBUTTON = 0x0017;
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        private static extern int GetSystemMetrics(int nIndex);

        [DllImport("user32.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        private static extern bool SystemParametersInfo(uint uiAction, uint uiParam, bool pvParam, uint fWinIni);
        public static bool IsPrimaryButtonLeft()
        {
            var value = GetSystemMetrics(SM_SWAPBUTTON);
            return value == 0;
        }
        public static void SetPrimaryButtonToLeft(bool isLeftPrimary)
        {
            SystemParametersInfo(SPI_SETMOUSEBUTTONSWAP, (uint)(isLeftPrimary ? 0 : 1), false, SPIF_SENDCHANGE);
        }
        // >>

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


        #region Read/Write INI file

        //Robert_Lin 2024-7-5 copy from VCPCorePlugin.cs, shared with other projects
        public static int IniReadInt(string sec, string key, int def, string pathName)
        {
            return _GetPrivateProfileInt(sec, key, def, pathName);
        }

        //Usage: int value=GetPrivateProfileInt("sectionName", "key", 3, @"C:\temp\a.ini");
        [DllImport("kernel32", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        private static extern int GetPrivateProfileInt(string section, string key, int def, string filePath);

        private static int _GetPrivateProfileInt(string section, string key, int def, string filePath)
        {
            return GetPrivateProfileInt(section, key, def, filePath);
        }

        //Uage:
        // //allocate string buffer, for large string you can allocate 4096 chars.
        // StringBuilder sb1=new StringBuilder(255);
        // int charsRet=GetPrivateProfileString("secName","key","defValue",sb1,sb1.Capacity,@"C:\temp\a.ini");
        // string result=sb1.ToString();
        [DllImport("kernel32", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        private static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);

        public static int _GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath)
        {
            return GetPrivateProfileString(section, key, def, retVal, size, filePath);
        }

        #endregion Read/Write INI file

    }
}