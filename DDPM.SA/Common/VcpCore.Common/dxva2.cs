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
        public static extern bool GetVCPFeatureAndVCPFeatureReply(
            IntPtr hMonitor, byte code, IntPtr i, out uint currentValue, out uint maxValue);

        [DllImport("dxva2.dll", SetLastError = true)]
        public static extern bool GetPhysicalMonitorsFromHMONITOR(IntPtr hMonitor, uint dwPhysicalMonitorArraySize, [Out] PHYSICAL_MONITOR[] pPhysicalMonitorArray);

        [DllImport("dxva2.dll", SetLastError = true)]
        public static extern bool GetNumberOfPhysicalMonitorsFromHMONITOR(IntPtr hMonitor, ref uint pdwNumberOfPhysicalMonitors);

        [DllImport("dxva2.dll", SetLastError = true)]
        public static extern bool GetCapabilitiesStringLength(IntPtr hMonitor, out uint length);

        [DllImport("dxva2.dll", SetLastError = true)]
        public static extern bool CapabilitiesRequestAndCapabilitiesReply(IntPtr hMonitor, StringBuilder output, uint length);

        [DllImport("dxva2.dll", SetLastError = true)]
        public static extern bool SetVCPFeature(IntPtr hMonitor, byte code, uint val);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct PHYSICAL_MONITOR
        {
            public IntPtr hPhysicalMonitor;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string szPhysicalMonitorDescription;
        }
    }
}