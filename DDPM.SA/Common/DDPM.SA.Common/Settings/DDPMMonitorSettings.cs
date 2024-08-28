using System.Collections.Generic;

namespace DDPM.SA.Common.Settings
{
    public class ImportVCP
    {
        public List<int> NotImportVCPs = new List<int>() { 0x02, 0x04, 0x05, 0x06, 0x08, 0xA };
        public List<int> ImportVCPSequence = new List<int>() { 0x10, 0x12, 0x66, 0xAA, 0x60 };
    }
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
        public double Version { get; set; }
        public string Model { get; set; }
        public string ServiceTag { get; set; }
        public Input Input { get; set; } = new Input();
        public KVM KVM { get; set; } = new KVM();
        public List<VCP> VCPs { get; set; } = new List<VCP>();
    }
}