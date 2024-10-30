using VcpCore.Common;

namespace DDPM.SA.Common
{
    public class Displaysettings_Function
    {
        public bool Send_Brightness_Telementry(ITelementryScheduler plugin, MonitorInfo monitorInfo, uint val, string currentResolution, string maxResolution)
        {
            var rt = false;
            if (monitorInfo != null)
            {
                var TelemetryDta_Brightness = new Displaysettings_Brightness();
                TelemetryDta_Brightness.Brightness = val;
                TelemetryDta_Brightness.CommunicationPath = "Video";
                TelemetryDta_Brightness.MonitorName = monitorInfo.AliasDeviceName;
                TelemetryDta_Brightness.D_Ctrl = monitorInfo.D_Ctrl;
                TelemetryDta_Brightness.SupplierID = monitorInfo.SupplierID;
                TelemetryDta_Brightness.FirmwareVersion = monitorInfo.FwVersion;
                TelemetryDta_Brightness.DisplayModelname = monitorInfo.modelName;
                TelemetryDta_Brightness.DisplayServiceTag = monitorInfo.edid.ServiceTag;
                TelemetryDta_Brightness.DsiplayResolution = currentResolution;
                TelemetryDta_Brightness.MaxDisplayResolution = maxResolution;

                if (plugin != null)
                    rt = plugin.ReceiveTelemetryInfo("Displaysettings", TelemetryDta_Brightness.ToJson(), Telementry_Frequency.RealTime).Result;
            }
            return rt;
        }

        public bool Send_Luminance_Telementry(ITelementryScheduler plugin, MonitorInfo monitorInfo, uint val, string currentResolution, string maxResolution)
        {
            var rt = false;
            if (monitorInfo != null)
            {
                var TelemetryDta_Luminance = new Displaysettings_Luminance();
                TelemetryDta_Luminance.Luminance = val;
                TelemetryDta_Luminance.CommunicationPath = "Video";
                TelemetryDta_Luminance.MonitorName = monitorInfo.AliasDeviceName;
                TelemetryDta_Luminance.D_Ctrl = monitorInfo.D_Ctrl;
                TelemetryDta_Luminance.SupplierID = monitorInfo.SupplierID;
                TelemetryDta_Luminance.FirmwareVersion = monitorInfo.FwVersion;
                TelemetryDta_Luminance.DisplayModelname = monitorInfo.modelName;
                TelemetryDta_Luminance.DisplayServiceTag = monitorInfo.edid.ServiceTag;
                TelemetryDta_Luminance.DsiplayResolution = currentResolution;
                TelemetryDta_Luminance.MaxDisplayResolution = maxResolution;

                if (plugin != null)
                    rt = plugin.ReceiveTelemetryInfo("Displaysettings", TelemetryDta_Luminance.ToJson(), Telementry_Frequency.RealTime).Result;
            }
            return rt;
        }

        public bool Send_Contrast_Telementry(ITelementryScheduler plugin, MonitorInfo monitorInfo, uint val, string currentResolution, string maxResolution)
        {
            var rt = false;
            if (monitorInfo != null)
            {
                var TelemetryDta_Contrast = new Displaysettings_Contrast();
                TelemetryDta_Contrast.Contrast = val;
                TelemetryDta_Contrast.CommunicationPath = "Video";
                TelemetryDta_Contrast.MonitorName = monitorInfo.AliasDeviceName;
                TelemetryDta_Contrast.D_Ctrl = monitorInfo.D_Ctrl;
                TelemetryDta_Contrast.SupplierID = monitorInfo.SupplierID;
                TelemetryDta_Contrast.FirmwareVersion = monitorInfo.FwVersion;
                TelemetryDta_Contrast.DisplayModelname = monitorInfo.modelName;
                TelemetryDta_Contrast.DisplayServiceTag = monitorInfo.edid.ServiceTag;
                TelemetryDta_Contrast.DsiplayResolution = currentResolution;
                TelemetryDta_Contrast.MaxDisplayResolution = maxResolution;

                if (plugin != null)
                    rt = plugin.ReceiveTelemetryInfo("Displaysettings", TelemetryDta_Contrast.ToJson(), Telementry_Frequency.RealTime).Result;
            }
            return rt;
        }

        public bool Send_PowerNap_Telementry(ITelementryScheduler plugin, MonitorInfo monitorInfo, string val, string currentResolution, string maxResolution)
        {
            var rt = false;
            if (monitorInfo != null)
            {
                var TelemetryDta_PowerNap = new Displaysettings_PowerNap();
                TelemetryDta_PowerNap.PowerNap = val;
                TelemetryDta_PowerNap.CommunicationPath = "Video";
                TelemetryDta_PowerNap.GraphicCardName = string.Empty;
                TelemetryDta_PowerNap.MonitorName = monitorInfo.AliasDeviceName;
                TelemetryDta_PowerNap.D_Ctrl = monitorInfo.D_Ctrl;
                TelemetryDta_PowerNap.SupplierID = monitorInfo.SupplierID;
                TelemetryDta_PowerNap.FirmwareVersion = monitorInfo.FwVersion;
                TelemetryDta_PowerNap.DisplayModelname = monitorInfo.modelName;
                TelemetryDta_PowerNap.DisplayServiceTag = monitorInfo.edid.ServiceTag;
                TelemetryDta_PowerNap.DsiplayResolution = currentResolution;
                TelemetryDta_PowerNap.MaxDisplayResolution = maxResolution;

                if (plugin != null)
                    rt = plugin.ReceiveTelemetryInfo("Displaysettings", TelemetryDta_PowerNap.ToJson(), Telementry_Frequency.RealTime).Result;
            }
            return rt;
        }

        public bool Send_Color_Preset_Manual_Telementry(ITelementryScheduler plugin, MonitorInfo monitorInfo, string val, string currentResolution, string maxResolution)
        {
            var rt = false;
            if (monitorInfo != null)
            {
                var TelemetryDta_Color_Preset_Manual = new Displaysettings_Color_Preset_Manual();
                TelemetryDta_Color_Preset_Manual.Color_Preset_Manual = val;
                TelemetryDta_Color_Preset_Manual.CommunicationPath = "Video";
                TelemetryDta_Color_Preset_Manual.MonitorName = monitorInfo.AliasDeviceName;
                TelemetryDta_Color_Preset_Manual.D_Ctrl = monitorInfo.D_Ctrl;
                TelemetryDta_Color_Preset_Manual.SupplierID = monitorInfo.SupplierID;
                TelemetryDta_Color_Preset_Manual.FirmwareVersion = monitorInfo.FwVersion;
                TelemetryDta_Color_Preset_Manual.DisplayModelname = monitorInfo.modelName;
                TelemetryDta_Color_Preset_Manual.DisplayServiceTag = monitorInfo.edid.ServiceTag;
                TelemetryDta_Color_Preset_Manual.DsiplayResolution = currentResolution;
                TelemetryDta_Color_Preset_Manual.MaxDisplayResolution = maxResolution;

                if (plugin != null)
                    rt = plugin.ReceiveTelemetryInfo("Displaysettings", TelemetryDta_Color_Preset_Manual.ToJson(), Telementry_Frequency.RealTime).Result;
            }
            return rt;
        }

        public bool Send_Color_Preset_Auto_Telementry(ITelementryScheduler plugin, MonitorInfo monitorInfo, string val, string currentResolution, string maxResolution)
        {
            var rt = false;
            if (monitorInfo != null)
            {
                var TelemetryDta_Color_Preset_Auto = new Displaysettings_Color_Preset_Auto();
                TelemetryDta_Color_Preset_Auto.Color_Preset_Auto = val;
                TelemetryDta_Color_Preset_Auto.CommunicationPath = "Video";
                TelemetryDta_Color_Preset_Auto.MonitorName = monitorInfo.AliasDeviceName;
                TelemetryDta_Color_Preset_Auto.D_Ctrl = monitorInfo.D_Ctrl;
                TelemetryDta_Color_Preset_Auto.SupplierID = monitorInfo.SupplierID;
                TelemetryDta_Color_Preset_Auto.FirmwareVersion = monitorInfo.FwVersion;
                TelemetryDta_Color_Preset_Auto.DisplayModelname = monitorInfo.modelName;
                TelemetryDta_Color_Preset_Auto.DisplayServiceTag = monitorInfo.edid.ServiceTag;
                TelemetryDta_Color_Preset_Auto.DsiplayResolution = currentResolution;
                TelemetryDta_Color_Preset_Auto.MaxDisplayResolution = maxResolution;

                if (plugin != null)
                    rt = plugin.ReceiveTelemetryInfo("Displaysettings", TelemetryDta_Color_Preset_Auto.ToJson(), Telementry_Frequency.RealTime).Result;
            }
            return rt;
        }

        public bool Send_NightLightStatus_Telementry(ITelementryScheduler plugin, MonitorInfo monitorInfo, string val, string currentResolution, string maxResolution)
        {
            var rt = false;
            if (monitorInfo != null)
            {
                var TelemetryDta_NightLightStatus = new Displaysettings_NightLightStatus();
                TelemetryDta_NightLightStatus.NightLightStatus = val;
                TelemetryDta_NightLightStatus.CommunicationPath = "Video";
                TelemetryDta_NightLightStatus.MonitorName = monitorInfo.AliasDeviceName;
                TelemetryDta_NightLightStatus.D_Ctrl = monitorInfo.D_Ctrl;
                TelemetryDta_NightLightStatus.SupplierID = monitorInfo.SupplierID;
                TelemetryDta_NightLightStatus.FirmwareVersion = monitorInfo.FwVersion;
                TelemetryDta_NightLightStatus.DisplayModelname = monitorInfo.modelName;
                TelemetryDta_NightLightStatus.DisplayServiceTag = monitorInfo.edid.ServiceTag;
                TelemetryDta_NightLightStatus.DsiplayResolution = currentResolution;
                TelemetryDta_NightLightStatus.MaxDisplayResolution = maxResolution;

                if (plugin != null)
                    rt = plugin.ReceiveTelemetryInfo("Displaysettings", TelemetryDta_NightLightStatus.ToJson(), Telementry_Frequency.RealTime).Result;
            }
            return rt;
        }


        public bool Send_InputSource_Telementry(ITelementryScheduler plugin, MonitorInfo monitorInfo, string val, string currentResolution, string maxResolution)
        {
            var rt = false;
            if (monitorInfo != null)
            {
                var TelemetryDta_InputSource = new Displaysettings_InputSource();
                TelemetryDta_InputSource.InputSource = val;
                TelemetryDta_InputSource.CommunicationPath = "Video";
                TelemetryDta_InputSource.GraphicCardName = string.Empty;
                TelemetryDta_InputSource.MonitorName = monitorInfo.AliasDeviceName;
                TelemetryDta_InputSource.D_Ctrl = monitorInfo.D_Ctrl;
                TelemetryDta_InputSource.SupplierID = monitorInfo.SupplierID;
                TelemetryDta_InputSource.FirmwareVersion = monitorInfo.FwVersion;
                TelemetryDta_InputSource.DisplayModelname = monitorInfo.modelName;
                TelemetryDta_InputSource.DisplayServiceTag = monitorInfo.edid.ServiceTag;
                TelemetryDta_InputSource.DsiplayResolution = currentResolution;
                TelemetryDta_InputSource.MaxDisplayResolution = maxResolution;

                if (plugin != null)
                    rt = plugin.ReceiveTelemetryInfo("Displaysettings", TelemetryDta_InputSource.ToJson(), Telementry_Frequency.RealTime).Result;
            }
            return rt;
        }

        public bool Send_USB_Telementry(ITelementryScheduler plugin, MonitorInfo monitorInfo, string val, string currentResolution, string maxResolution)
        {
            var rt = false;
            if (monitorInfo != null)
            {
                var TelemetryDta_USB = new Displaysettings_USB();
                TelemetryDta_USB.USBAssociation = val;
                TelemetryDta_USB.CommunicationPath = "Video";
                TelemetryDta_USB.GraphicCardName = string.Empty;
                TelemetryDta_USB.MonitorName = monitorInfo.AliasDeviceName;
                TelemetryDta_USB.D_Ctrl = monitorInfo.D_Ctrl;
                TelemetryDta_USB.SupplierID = monitorInfo.SupplierID;
                TelemetryDta_USB.FirmwareVersion = monitorInfo.FwVersion;
                TelemetryDta_USB.DisplayModelname = monitorInfo.modelName;
                TelemetryDta_USB.DisplayServiceTag = monitorInfo.edid.ServiceTag;
                TelemetryDta_USB.DsiplayResolution = currentResolution;
                TelemetryDta_USB.MaxDisplayResolution = maxResolution;

                if (plugin != null)
                    rt = plugin.ReceiveTelemetryInfo("Displaysettings", TelemetryDta_USB.ToJson(), Telementry_Frequency.RealTime).Result;
            }
            return rt;
        }
    }
}