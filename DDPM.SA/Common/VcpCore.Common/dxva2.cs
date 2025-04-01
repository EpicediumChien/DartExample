using System;
using System.Runtime.InteropServices;

namespace VcpCore.Common
{
    [Serializable]
    public static class dxva2
    {
        private const int PHYSICAL_MONITOR_DESCRIPTION_SIZE = 128;

        [DllImport("dxva2.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        private static extern bool GetVCPFeatureAndVCPFeatureReply(IntPtr hMonitor, byte code, IntPtr i, out uint currentValue, out uint maxValue);

        public static bool _GetVCPFeatureAndVCPFeatureReply(IntPtr hMonitor, byte code, IntPtr i, out uint currentValue, out uint maxValue)
        {
            bool rst = GetVCPFeatureAndVCPFeatureReply(hMonitor, code, i, out currentValue, out maxValue);

            if (!rst)
            {
#if DEBUG
                Console.WriteLine("[dxva2] GetVCPFeatureAndVCPFeatureReply failed.");
#endif
            }

            return rst;
        }

        [DllImport("dxva2.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        private static extern bool GetPhysicalMonitorsFromHMONITOR(IntPtr hMonitor, uint dwPhysicalMonitorArraySize, [Out] PHYSICAL_MONITOR[] pPhysicalMonitorArray);

        public static bool _GetPhysicalMonitorsFromHMONITOR(IntPtr hMonitor, uint dwPhysicalMonitorArraySize, [Out] PHYSICAL_MONITOR[] pPhysicalMonitorArray)
        {
            bool rst = GetPhysicalMonitorsFromHMONITOR(hMonitor, dwPhysicalMonitorArraySize, pPhysicalMonitorArray);

            if (!rst)
            {
#if DEBUG
                Console.WriteLine("[dxva2] GetPhysicalMonitorsFromHMONITOR failed.");
#endif
            }

            return rst;
        }

        [DllImport("dxva2.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        private static extern bool GetNumberOfPhysicalMonitorsFromHMONITOR(IntPtr hMonitor, ref uint pdwNumberOfPhysicalMonitors);

        public static bool _GetNumberOfPhysicalMonitorsFromHMONITOR(IntPtr hMonitor, ref uint pdwNumberOfPhysicalMonitors)
        {
            bool rst = GetNumberOfPhysicalMonitorsFromHMONITOR(hMonitor, ref pdwNumberOfPhysicalMonitors);

            if (!rst)
            {
#if DEBUG
                Console.WriteLine("[dxva2] GetNumberOfPhysicalMonitorsFromHMONITOR failed.");
#endif
            }

            return rst;
        }

        [DllImport("dxva2.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        private static extern bool GetCapabilitiesStringLength(IntPtr hMonitor, out uint length);

        public static bool _GetCapabilitiesStringLength(IntPtr hMonitor, out uint length)
        {
            bool rst = GetCapabilitiesStringLength(hMonitor, out length);

            if (!rst)
            {
#if DEBUG
                Console.WriteLine("[dxva2] GetCapabilitiesStringLength failed.");
#endif
            }

            return rst;
        }

        [DllImport("dxva2.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool CapabilitiesRequestAndCapabilitiesReply(IntPtr hMonitor, byte[] pszASCIICapabilitiesString, uint dwCapabilitiesStringLengthInCharacters);

        public static bool _CapabilitiesRequestAndCapabilitiesReply(IntPtr hMonitor, byte[] output, uint length)
        {
            bool rst = CapabilitiesRequestAndCapabilitiesReply(hMonitor, output, length);

            if (!rst)
            {
#if DEBUG
                Console.WriteLine("[dxva2] CapabilitiesRequestAndCapabilitiesReply failed.");
#endif
            }

            return rst;
        }

        [DllImport("dxva2.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        private static extern bool SetVCPFeature(IntPtr hMonitor, byte code, uint val);

        public static bool _SetVCPFeature(IntPtr hMonitor, byte code, uint val)
        {
            bool rst = SetVCPFeature(hMonitor, code, val);

            if (!rst)
            {
#if DEBUG
                Console.WriteLine("[dxva2] SetVCPFeature failed.");
#endif
            }

            return rst;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct PHYSICAL_MONITOR
        {
            public IntPtr hPhysicalMonitor;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string szPhysicalMonitorDescription;
        }
    }
}