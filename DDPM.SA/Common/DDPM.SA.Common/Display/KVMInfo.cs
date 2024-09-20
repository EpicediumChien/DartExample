using DDPM.SA.Common.Display;

namespace DDPM.SA.Common
{
    internal class KVMInfo
    {
    }

    public class PCsInfo
    {
        public string InputType { get; set; }
        public string InputName { get; set; }
        public string USBUpstream { get; set; }
    }

    public class NKVMRespone
    {
        public string CLIName { get; set; }
        public string Respone { get; set; }
    }

    public class NKVMSetHotkey
    {
        //public int monitorIndex { get; set; }
        public string jsonstring { get; set; }
        public HotkeyInfo HotkeyInfo { get; set; }
    }
}