using System;

namespace VcpCore.Common
{
    [Serializable]
    public class MultiCommandArch
    {
        public MonitorInfo MonitorInfo = new MonitorInfo();

        public MultiCommandAction Action = MultiCommandAction.None;

        public object Function = Convert.ToByte(0x00);

        public object Value = string.Empty;

        public Guid Guid = default;

        public int Opt = 0;

        public Priority Priority = Priority.Low;

        public object Result = string.Empty;
    }

    public enum MultiCommandAction
    {
        None,
        GetMonitors,
        GetCapabilitiesString,
        GetVCPCapabilities,
        GetVCPCapability,
        SetVCPCapability,
        GetVCPCacheTable,
        CancelVcpTask,
    }
}