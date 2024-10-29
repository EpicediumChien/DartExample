using Newtonsoft.Json;
using System.Collections.Generic;

namespace DDPM.SA.Common
{
    public class ScreenTimeInfoBasic
    {
        public List<uint> ScreenTimeInfo { get; set; }

        public List<string> DisplayModelname { get; set; }

        public List<string> DisplayServiceTag { get; set; }

        public ScreenTimeInfoBasic()
        {
            ScreenTimeInfo = new List<uint>();
            DisplayModelname = new List<string>();
            DisplayServiceTag = new List<string>();
        }

        public string ToJson()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }
    }
}