using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    public class DDPMMonitorSettings
    {
        public string Model { get; set; }
        public string ServiceTag { get; set; }
        public Input Input { get; set; } = new Input();
        public KVM KVM { get; set; } = new KVM();
    }
}
