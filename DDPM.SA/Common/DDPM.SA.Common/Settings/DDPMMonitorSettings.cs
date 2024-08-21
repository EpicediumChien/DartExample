using System.Collections.Generic;

namespace DDPM.SA.Common.Settings
{
    public class Input
    {
        public string strInputSourceList { get; set; } = string.Empty;
    }

    public class KVM
    {
        public string strUSBKVMPCsList { get; set; } = string.Empty;
        public bool isOnUSBKVM { get; set; } = false;
        public bool isOnNKVM { get; set; } = false;
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

    public class DDPMMonitorSettings
    {
        public string Model { get; set; }
        public string ServiceTag { get; set; }
        public Input Input { get; set; } = new Input();
        public KVM KVM { get; set; } = new KVM();
        public List<VCP> VCPs { get; set; } = new List<VCP>();
    }
}