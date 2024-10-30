using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VcpCore.Common;

namespace DDPM.SA.Common.Telemetry
{
    public class UsedLanguage_Telementry
    {
        public string Language { get; set; } = string.Empty;

        public UsedLanguage_Telementry()
        {
            Language = string.Empty;
        }

        public string ToJson()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }
    }
    public class UsedLanguage_Function
    {
        public bool Send_UsedLanguage_Telementry(ITelementryScheduler plugin, string val)
        {
            var rt = false;
            if (string.IsNullOrEmpty(val))
                return rt;
            var TelemetryDta_UsedLanguage = new UsedLanguage_Telementry();
            TelemetryDta_UsedLanguage.Language = val;

            if (plugin != null)
                rt = plugin.ReceiveTelemetryInfo("UsedLanguage", TelemetryDta_UsedLanguage.ToJson(), Telementry_Frequency.FirstDayofMonth).Result;

            return rt;
        }
    }



}
