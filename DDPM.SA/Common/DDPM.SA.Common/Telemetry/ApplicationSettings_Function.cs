using System.Collections.Generic;
using System.Linq;
using VcpCore.Common;

namespace DDPM.SA.Common
{
    public class ApplicationSettings_Function
    {
        public bool Send_LockRotation_Telementry(ITelementryScheduler plugin, List<MonitorInfo> monitorInfos, bool LockRotation)
        {
            var rt = false;
            List<string> models = new List<string>();
            List<string> D_Ctrls = new List<string>();
            List<string> DisplayServiceTags = new List<string>();

            foreach (var monitorInfo in monitorInfos)
            {
                models.Add(monitorInfo.modelName);
                D_Ctrls.Add(monitorInfo.D_Ctrl);
                DisplayServiceTags.Add(monitorInfo.edid.ServiceTag);
            }

            var TelemetryDta_LockRotation = new ApplicationSettings_LockRotation();
            TelemetryDta_LockRotation.DisplayModelname = models.ToList();
            TelemetryDta_LockRotation.D_Ctrl = D_Ctrls.ToList();
            TelemetryDta_LockRotation.DisplayServiceTag = DisplayServiceTags.ToList();
            TelemetryDta_LockRotation.LockRotation = LockRotation ? "On" : "Off";

            if (plugin != null)
                rt = plugin.ReceiveTelemetryInfo("ApplicationSettings", TelemetryDta_LockRotation.ToJson(), Telementry_Frequency.RealTime).Result;

            return rt;
        }
    }
}