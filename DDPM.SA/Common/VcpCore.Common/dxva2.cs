using System;
using System.Runtime.InteropServices;
using System.Text;

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
            return GetVCPFeatureAndVCPFeatureReply(hMonitor, code, i, out currentValue, out maxValue);
        }

        [DllImport("dxva2.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        private static extern bool GetPhysicalMonitorsFromHMONITOR(IntPtr hMonitor, uint dwPhysicalMonitorArraySize, [Out] PHYSICAL_MONITOR[] pPhysicalMonitorArray);

        public static bool _GetPhysicalMonitorsFromHMONITOR(IntPtr hMonitor, uint dwPhysicalMonitorArraySize, [Out] PHYSICAL_MONITOR[] pPhysicalMonitorArray)
        {
            return GetPhysicalMonitorsFromHMONITOR(hMonitor, dwPhysicalMonitorArraySize, pPhysicalMonitorArray);
        }

        [DllImport("dxva2.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        private static extern bool GetNumberOfPhysicalMonitorsFromHMONITOR(IntPtr hMonitor, ref uint pdwNumberOfPhysicalMonitors);

        public static bool _GetNumberOfPhysicalMonitorsFromHMONITOR(IntPtr hMonitor, ref uint pdwNumberOfPhysicalMonitors)
        {
            return GetNumberOfPhysicalMonitorsFromHMONITOR(hMonitor, ref pdwNumberOfPhysicalMonitors);
        }

        [DllImport("dxva2.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        private static extern bool GetCapabilitiesStringLength(IntPtr hMonitor, out uint length);

        public static bool _GetCapabilitiesStringLength(IntPtr hMonitor, out uint length)
        {
            return GetCapabilitiesStringLength(hMonitor, out length);
        }

        [DllImport("dxva2.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        private static extern bool CapabilitiesRequestAndCapabilitiesReply(IntPtr hMonitor, StringBuilder output, uint length);

        public static bool _CapabilitiesRequestAndCapabilitiesReply(IntPtr hMonitor, StringBuilder output, uint length)
        {
            return CapabilitiesRequestAndCapabilitiesReply(hMonitor, output, length);
        }

        [DllImport("dxva2.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        private static extern bool SetVCPFeature(IntPtr hMonitor, byte code, uint val);

        public static bool _SetVCPFeature(IntPtr hMonitor, byte code, uint val)
        {
            return SetVCPFeature(hMonitor, code, val);
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