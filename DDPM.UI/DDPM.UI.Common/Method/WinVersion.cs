using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;


namespace DDPM.UI.Common.Method
{
    public struct VersionInfo
    {
        public uint Major;
        public uint Minor;
        public uint BuildNum;
    }

    public enum BuildNumber : uint
    {
        Windows_Vista = 6002,
        Windows_7 = 7601,
        Windows_8 = 9200,
        Windows_8_1 = 9600,
        Windows_10_1507 = 10240,
        Windows_10_1511 = 10586,
        Windows_10_1607 = 14393,
        Windows_10_1703 = 15063,
        Windows_10_1709 = 16299,
        Windows_10_1803 = 17134,
        Windows_10_1809 = 17763,
        Windows_10_1903 = 18362,
        Windows_10_1909 = 18363,
        Windows_10_2004 = 19041,
        Windows_10_20H2 = 19042,
        Windows_10_21H1 = 19043,
        Windows_10_21H2 = 19044,
        Windows_10_22H2 = 19045,
        Windows_11_21H2 = 22000,
        Windows_11_22H2 = 22621,
        Windows_11_23H2 = 22631,
    };

    public class WinVersion
    {
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct OSVERSIONINFOEXW
        {
            public uint dwOSVersionInfoSize;
            public uint dwMajorVersion;
            public uint dwMinorVersion;
            public uint dwBuildNumber;
            public uint dwPlatformId;
            [MarshalAs(UnmanagedType.LPWStr, SizeConst = 128)]
            public string szCSDVersion;
            public UInt16 wServicePackMajor;
            public UInt16 wServicePackMinor;
            public UInt16 wSuiteMask;
            public byte wProductType;
            public byte wReserved;
        }
        [DllImport("ntdll.dll", CallingConvention = CallingConvention.StdCall)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        private static extern int RtlGetVersion(out OSVERSIONINFOEXW osv);
        private static int _RtlGetVersion(out OSVERSIONINFOEXW osv)
        {
            return RtlGetVersion(out osv);
        }

        public static bool GetVersion(out VersionInfo info)
        {
            info.Major = 0;
            info.Minor = 0;
            info.BuildNum = 0;
            OSVERSIONINFOEXW osv = new OSVERSIONINFOEXW();
            osv.dwOSVersionInfoSize = 284;
            if (_RtlGetVersion(out osv) == 0)
            {
                info.Major = osv.dwMajorVersion;
                info.Minor = osv.dwMinorVersion;
                info.BuildNum = osv.dwBuildNumber;

                return true;
            }
            return false;
        }
        public static bool IsBuildNumGreaterOrEqual(uint buildNumber)
        {
            if (GetVersion(out var info))
            {
                return info.BuildNum >= buildNumber;
            }
            return false;
        }

        public static string GetComputerManufacturer()
        {
            //ManagementClass mc = new ManagementClass("Win32_ComputerSystem");
            //ManagementObjectCollection moc = mc.GetInstances();
            //if (moc.Count != 0)
            //{
            //    foreach (ManagementObject mo in moc.Cast<ManagementObject>())
            //    {
            //        try
            //        {
            //            return mo["Manufacturer"].ToString();
            //        }
            //        catch
            //        {
            //            return null;
            //        }
            //    }
            //}
            //return null;
            using (ManagementClass mc = new ManagementClass("Win32_ComputerSystem"))
            {
                using (ManagementObjectCollection moc = mc.GetInstances())
                {
                    foreach (ManagementObject mo in moc.Cast<ManagementObject>())
                    {
                        try
                        {
                            return mo["Manufacturer"]?.ToString();
                        }
                        catch (Exception ex)
                        {

                            Console.WriteLine($"Exception: {ex.Message}");
                            return null;
                        }
                        finally
                        {
                            mo.Dispose();
                        }
                    }
                }
                return null;
            }
        }
    }

}
