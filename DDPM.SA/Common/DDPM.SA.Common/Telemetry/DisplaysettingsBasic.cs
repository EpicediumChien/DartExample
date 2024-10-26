using Newtonsoft.Json;

namespace DDPM.SA.Common
{
    public class DisplaysettingsBasic
    {
        public string CommunicationPath { get; set; }
        public string GraphicCardName { get; set; }
        public string MonitorName { get; set; }
        public string D_Ctrl { get; set; }
        public string SupplierID { get; set; }
        public string FirmwareVersion { get; set; }
        public string DisplayModelname { get; set; }
        public string DisplayServiceTag { get; set; }
        public string DsiplayResolution { get; set; }
        public string MaxDisplayResolution { get; set; }

        public DisplaysettingsBasic()
        {
            CommunicationPath = "Video";
            GraphicCardName = string.Empty;
            MonitorName = string.Empty;
            D_Ctrl = string.Empty;
            SupplierID = string.Empty;
            FirmwareVersion = string.Empty;
            DisplayModelname = string.Empty;
            DisplayServiceTag = string.Empty;
            DsiplayResolution = string.Empty;
            MaxDisplayResolution = string.Empty;
        }

        public string ToJson()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }
    }

    public class Displaysettings_Brightness : DisplaysettingsBasic
    {
        public uint Brightness { get; set; } = 0x0;
    }

    public class Displaysettings_Contrast : DisplaysettingsBasic
    {
        public uint Contrast { get; set; } = 0x0;
    }

    public class Displaysettings_Luminance : DisplaysettingsBasic
    {
        public uint Luminance { get; set; } = 0x0;
    }
}