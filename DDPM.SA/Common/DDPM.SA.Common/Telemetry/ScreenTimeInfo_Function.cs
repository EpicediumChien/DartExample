using System.Collections.Generic;
using System.Linq;

namespace DDPM.SA.Common
{
    public class ScreenTimeInfo_Function
    {
        public ScreenTimeInfoBasic ScreenTimeInfo_Telementry(List<uint> screetimes, List<string> serviceTags, List<string> Models)
        {
            var TelemetryDta_ScreenTimeInfo = new ScreenTimeInfoBasic();

            TelemetryDta_ScreenTimeInfo.ScreenTimeInfo = screetimes.ToList();
            TelemetryDta_ScreenTimeInfo.DisplayModelname = Models.ToList();
            TelemetryDta_ScreenTimeInfo.DisplayServiceTag = serviceTags.ToList();

            return TelemetryDta_ScreenTimeInfo;
        }
    }
}