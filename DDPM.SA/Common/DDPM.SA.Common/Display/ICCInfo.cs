using System.Collections.Generic;

namespace DDPM.SA.Common
{
    public class IIC_Metadata
    {
        // jim add 20240611
        public Dictionary<string, List<ICC_SupportDeviceName>> _support_ICC_DeviceName = new Dictionary<string, List<ICC_SupportDeviceName>>() { };

        public bool Is_Support_ICC_DeviceName { get; set; } = false;
        public string strICC_Folder { get; set; } = string.Empty;
    }

    // jim add 20240611
    public class ICC_SupportDeviceName
    {
        public string File { get; set; } = string.Empty;
        public string ColorPreset { get; set; } = string.Empty;
        public string SHA256 { get; set; } = string.Empty;
    }
}