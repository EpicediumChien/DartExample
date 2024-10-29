using VcpCore.Common;

namespace DDPM.SA.Common
{
    public class DisplayInformation_Function
    {
        public DisplayInformationBasic DisplayInformation_Telementry(MonitorInfo monitorInfo, DisplayOrientation Orientation, string RefreshRate, bool HDRStatus, string currentResolution, string maxResolution)
        {
            System.Windows.Forms.Screen[] screenList = System.Windows.Forms.Screen.AllScreens;

            var count = 0;
            foreach (var screen in screenList)
            {
                if ((monitorInfo.DisplayName.ToLower()).Equals(screen.DeviceName.ToLower()))
                    count++;
            }

            var TelemetryDta_DisplayInformation = new DisplayInformationBasic();
            if (monitorInfo != null)
            {
                TelemetryDta_DisplayInformation.MonitorName = monitorInfo.modelName;
                TelemetryDta_DisplayInformation.ProjectionMode = (count > 1) ? "Duplicate" : "Extended";
                TelemetryDta_DisplayInformation.Text_app_size = (monitorInfo.scalingFactor * 100).ToString() + "%";
                TelemetryDta_DisplayInformation.Orientation = nameof(Orientation);
                TelemetryDta_DisplayInformation.RefreshRate = RefreshRate;
                TelemetryDta_DisplayInformation.SmartHDR = HDRStatus ? "On" : "Off";
                TelemetryDta_DisplayInformation.CurrentDsiplayResolution = currentResolution;
                TelemetryDta_DisplayInformation.MaxDsiplayResolution = maxResolution;
            }
            return TelemetryDta_DisplayInformation;
        }
    }
}