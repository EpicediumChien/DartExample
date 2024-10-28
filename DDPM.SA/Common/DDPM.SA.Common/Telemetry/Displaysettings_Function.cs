using DdmLibrary;
using System;
using System.Management;
using VcpCore.Common;

namespace DDPM.SA.Common
{
    public class Displaysettings_Function
    {
        public bool Send_Brightness_Telementry(ITelementryScheduler plugin, MonitorInfo monitorInfo, uint val)
        {
            var rt = false;

            var TelemetryDta_Brightness = new Displaysettings_Brightness();
            TelemetryDta_Brightness.Brightness = val;
            TelemetryDta_Brightness.CommunicationPath = "Video";
            TelemetryDta_Brightness.GraphicCardName = string.Empty;
            TelemetryDta_Brightness.MonitorName = monitorInfo.AliasDeviceName;
            TelemetryDta_Brightness.D_Ctrl = monitorInfo.D_Ctrl;
            TelemetryDta_Brightness.SupplierID = monitorInfo.SupplierID;
            TelemetryDta_Brightness.FirmwareVersion = monitorInfo.FwVersion;
            TelemetryDta_Brightness.DisplayModelname = monitorInfo.modelName;
            TelemetryDta_Brightness.DisplayServiceTag = monitorInfo.edid.ServiceTag;
            TelemetryDta_Brightness.DsiplayResolution = string.Empty;
            TelemetryDta_Brightness.MaxDisplayResolution = string.Empty;
            
            rt = plugin.ReceiveTelemetryInfo("Displaysettings", TelemetryDta_Brightness.ToJson(), Telementry_Frequency.RealTime).Result;

            return rt;
        }

        public bool Send_Luminance_Telementry(ITelementryScheduler plugin, MonitorInfo monitorInfo, uint val)
        {
            var rt = false;

            var TelemetryDta_Luminance = new Displaysettings_Luminance();
            TelemetryDta_Luminance.Luminance = val;
            TelemetryDta_Luminance.CommunicationPath = "Video";
            TelemetryDta_Luminance.GraphicCardName = string.Empty;
            TelemetryDta_Luminance.MonitorName = monitorInfo.AliasDeviceName;
            TelemetryDta_Luminance.D_Ctrl = monitorInfo.D_Ctrl;
            TelemetryDta_Luminance.SupplierID = monitorInfo.SupplierID;
            TelemetryDta_Luminance.FirmwareVersion = monitorInfo.FwVersion;
            TelemetryDta_Luminance.DisplayModelname = monitorInfo.modelName;
            TelemetryDta_Luminance.DisplayServiceTag = monitorInfo.edid.ServiceTag;
            TelemetryDta_Luminance.DsiplayResolution = string.Empty;
            TelemetryDta_Luminance.MaxDisplayResolution = string.Empty;

            rt = plugin.ReceiveTelemetryInfo("Displaysettings", TelemetryDta_Luminance.ToJson(), Telementry_Frequency.RealTime).Result;

            return rt;
        }

        public bool Send_Contrast_Telementry(ITelementryScheduler plugin, MonitorInfo monitorInfo, uint val)
        {
            var rt = false;

            var TelemetryDta_Contrast = new Displaysettings_Contrast();
            TelemetryDta_Contrast.Contrast = val;
            TelemetryDta_Contrast.CommunicationPath = "Video";
            TelemetryDta_Contrast.GraphicCardName = string.Empty;
            TelemetryDta_Contrast.MonitorName = monitorInfo.AliasDeviceName;
            TelemetryDta_Contrast.D_Ctrl = monitorInfo.D_Ctrl;
            TelemetryDta_Contrast.SupplierID = monitorInfo.SupplierID;
            TelemetryDta_Contrast.FirmwareVersion = monitorInfo.FwVersion;
            TelemetryDta_Contrast.DisplayModelname = monitorInfo.modelName;
            TelemetryDta_Contrast.DisplayServiceTag = monitorInfo.edid.ServiceTag;
            TelemetryDta_Contrast.DsiplayResolution = string.Empty;
            TelemetryDta_Contrast.MaxDisplayResolution = string.Empty;

            rt = plugin.ReceiveTelemetryInfo("Displaysettings", TelemetryDta_Contrast.ToJson(), Telementry_Frequency.RealTime).Result;

            return rt;
        }
    }    
}