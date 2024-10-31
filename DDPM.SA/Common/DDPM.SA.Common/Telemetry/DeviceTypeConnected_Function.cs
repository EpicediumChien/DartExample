using System.Collections.Generic;
using System.Linq;
using VcpCore.Common;

namespace DDPM.SA.Common
{
    public class DeviceTypeConnected_Function
    {
        public bool DeviceTypeConnected_Telementry(ITelementryScheduler plugin, List<MonitorInfo> monitors)
        {
            var rt = false;
            if (monitors.Count > 0)
            {
                List<string> ConnectedTypes = new List<string>();
                List<string> Models = new List<string>();
                List<string> serviceTags = new List<string>();

                foreach (MonitorInfo monitor in monitors)
                {
                    ConnectedTypes.Add(monitor.inputCable);
                    Models.Add(monitor.modelName);
                    serviceTags.Add(monitor.edid.ServiceTag);
                }

                var TelemetryDta_DeviceTypeConnected = new DeviceTypeConnected_VideoPortInUsed();
                TelemetryDta_DeviceTypeConnected.totalnum = monitors.Count;
                TelemetryDta_DeviceTypeConnected.ConnectedType = ConnectedTypes.ToList();
                TelemetryDta_DeviceTypeConnected.DisplayModelname = Models.ToList();
                TelemetryDta_DeviceTypeConnected.DisplayServiceTag = serviceTags.ToList();

                if (plugin != null)
                    rt = plugin.ReceiveTelemetryInfo("Device_Type_Connected", TelemetryDta_DeviceTypeConnected.ToJson(), Telementry_Frequency.RealTime).Result;
            }
            else
                rt = true;

            return rt;
        }
    }
}