using Newtonsoft.Json;

namespace DDPM.SA.Common
{
    public class DisplayInformationBasic
    {
        public string CommunicationPath { get; set; }
        public string MonitorName { get; set; }
        public string ProjectionMode { get; set; }
        public string Text_app_size { get; set; }
        public string AdapterStrings { get; set; }
        public string AdapterName { get; set; }
        public string Orientation { get; set; }
        public string RefreshRate { get; set; }
        public string SmartHDR { get; set; }
        public string CurrentDsiplayResolution { get; set; }
        public string MaxDsiplayResolution { get; set; }

        public DisplayInformationBasic()
        {
            CommunicationPath = "Video";
            MonitorName = string.Empty;
            ProjectionMode = string.Empty;
            Text_app_size = string.Empty;
            AdapterStrings = (new Telementry_GeneralFunction()).GetMonitorAdapter();
            AdapterName = (new Telementry_GeneralFunction()).GetMonitorAdapter();
            Orientation = string.Empty;
            RefreshRate = string.Empty;
            SmartHDR = string.Empty;
            CurrentDsiplayResolution = string.Empty;
            MaxDsiplayResolution = string.Empty;
        }

        public string ToJson()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }
    }
}