using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDPM.SA.Common.Telemetry
{
    public class SoftwareFailureBasic
    {
        public string ErrorCode { get; set; }
        public string FailureMessage { get; set; }
        public string DisplayModelname { get; set; }
        public string DisplayServiceTag { get; set; }
        public string D_Ctrl { get; set; }
        public string SW_Available_date { get; set; }
        public string SW_Update_date { get; set; }

        public SoftwareFailureBasic()
        {
            ErrorCode = string.Empty;
            FailureMessage = string.Empty;
            DisplayModelname = string.Empty;
            DisplayServiceTag = string.Empty;
            D_Ctrl = string.Empty;
            DisplayServiceTag = string.Empty;
            SW_Available_date = string.Empty;
            SW_Update_date = string.Empty;
        }

        public string ToJson()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }
    }
}
