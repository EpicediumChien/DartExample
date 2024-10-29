using Newtonsoft.Json;

namespace DDPM.SA.Common
{
    public class ScreenTimeInfoBasic
    {
        public string ScreenTimeInfo { get; set; }

        public string DisplayModelname { get; set; }

        public string DisplayServiceTag { get; set; }

        public ScreenTimeInfoBasic()
        {
            ScreenTimeInfo = string.Empty;
            DisplayModelname = string.Empty;
            DisplayServiceTag = string.Empty;
        }

        public string ToJson()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }
    }
}