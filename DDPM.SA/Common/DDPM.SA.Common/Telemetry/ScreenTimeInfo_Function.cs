using System.Collections.Generic;

namespace DDPM.SA.Common
{
    public class ScreenTimeInfo_Function
    {
        public ScreenTimeInfoBasic ScreenTimeInfo_Telementry(List<uint> screetimes, List<string> serviceTags, List<string> Models)
        {
            var TelemetryDta_ScreenTimeInfo = new ScreenTimeInfoBasic();

            foreach (var tmp in screetimes)
                TelemetryDta_ScreenTimeInfo.ScreenTimeInfo += tmp.ToString() + ", ";
            foreach (var tmp in Models)
                TelemetryDta_ScreenTimeInfo.DisplayModelname += tmp + ", ";
            foreach (var tmp in serviceTags)
                TelemetryDta_ScreenTimeInfo.DisplayServiceTag += tmp + ", ";

            return TelemetryDta_ScreenTimeInfo;
        }
    }
}