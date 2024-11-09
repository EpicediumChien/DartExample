using DDPM.SA.Common.Settings;
using Dell.Client.Framework.Common;
using DPeMPublic.Common.Enums;
using MS.WindowsAPICodePack.Internal;
using System;
using System.Diagnostics;
using System.Linq;
using VcpCore.Common;
using static DDPM.SA.Common.DisplayFeaturesBasic;

namespace DDPM.SA.Common.Telemetry
{
    public class DisplayFeatures_Functions
    {
        private DisplayFeatures_EasyMemory CreateBasicDisplayInfo(MonitorInfo monitorInfo, Properties currentProperties, DisplayPropertiesInfo displayInfo)
        {
            if (monitorInfo == null)
                return null;
            if (displayInfo == null)
                return null;
            if (currentProperties == null)
                return null;
            var basicInfo = new DisplayFeatures_EasyMemory();
            basicInfo.CommunicationPath = "Video";
            basicInfo.DisplayModelname = monitorInfo.edid.ModelName;
            basicInfo.DisplayServiceTag = monitorInfo.edid.ServiceTag;
            basicInfo.FirmwareVersion = monitorInfo.FwVersion;
            basicInfo.ScreenSize = monitorInfo.edid.Size;
            basicInfo.Scalefactor = monitorInfo.scalingFactor;
            basicInfo.CurrentResolution = $"{currentProperties.Resolutions_Width}x{currentProperties.Resolutions_High}";
            basicInfo.MaxResolution = $"{displayInfo.SupportedProperties.Properties[0].Resolutions_Width}x{displayInfo.SupportedProperties.Properties[0].Resolutions_High}";

            return basicInfo;
        }

        public bool SentInfoToTelementry(ILog _log, ITelementryScheduler _TelementryScheduler, MonitorInfo monitorInfo, DisplayPropertiesInfo displayInfo, EasyArrangementDDPM easyArrangementDDPM, DDPMSettings ddpmsetting, string ez)
        {
            bool result = false;
            try
            {
                if (_TelementryScheduler == null)
                    return result;
                Properties currentProperties = displayInfo.SupportedProperties.Properties.FirstOrDefault(p => p.isCurrent);
                var basicInfo = CreateBasicDisplayInfo(monitorInfo, currentProperties, displayInfo);
                if (basicInfo == null)
                    return result;
                _log.Info($"[DeviceManagerPlugin] [DisplayFeatures_Functions] SentInfoToTelementry Start.");
                switch (ez)
                {
                    case "EasyMemory":
                        var DisplayFeatures_EasyMemory = new DisplayFeatures_EasyMemory
                        {
                            manual = easyArrangementDDPM.Desktops[0].ProfileSettings[0].StartUpLaunch,
                            auto = easyArrangementDDPM.Desktops[0].ProfileSettings[0].Auto,
                            scheduled = (uint)easyArrangementDDPM.Desktops[0].ProfileSettings[0].AutoStartTime
                        };

                        CopyBasicInfo(DisplayFeatures_EasyMemory, basicInfo);
                        result = _TelementryScheduler.ReceiveTelemetryInfo("DisplayFeatures", DisplayFeatures_EasyMemory.ToJson(), Telementry_Frequency.RealTime).Result;
                        Trace.WriteLine("EasyMemory : " + DisplayFeatures_EasyMemory.ToJson());
                        _log.Info($"[DeviceManagerPlugin] [DisplayFeatures_Functions] SentInfoToTelementry EasyMemory Success.");
                        break;

                    case "EasyMemoryProfileCount":
                        var DisplayFeatures_EasyMemoryProfileCount = new DisplayFeatures_EasyMemoryProfileCount
                        {
                            profile_count_value = (uint)ddpmsetting.UserSettings.EAProfile.Count,
                        };
                        CopyBasicInfo(DisplayFeatures_EasyMemoryProfileCount, basicInfo);
                        result = _TelementryScheduler.ReceiveTelemetryInfo("DisplayFeatures", DisplayFeatures_EasyMemoryProfileCount.ToJson(), Telementry_Frequency.RealTime).Result;
                        Trace.WriteLine("EasyMemoryProfileCount : " + DisplayFeatures_EasyMemoryProfileCount.ToJson());
                        _log.Info($"[DeviceManagerPlugin] [DisplayFeatures_Functions] SentInfoToTelementry EasyMemoryProfileCount Success.");
                        break;

                    case "MaxEasyMemoryLayoutUsed":
                        EAProfileDDPM maxIDProfile = ddpmsetting.UserSettings.EAProfile.OrderByDescending(p => p.ID).FirstOrDefault();
                        var DisplayFeatures_MaxEasyMemoryLayoutUsed = new DisplayFeatures_MaxEasyMemoryLayoutUsed
                        {
                            maximum_widows_among_profile = (uint)maxIDProfile.ID + 1,
                        };

                        CopyBasicInfo(DisplayFeatures_MaxEasyMemoryLayoutUsed, basicInfo);
                        result = _TelementryScheduler.ReceiveTelemetryInfo("DisplayFeatures", DisplayFeatures_MaxEasyMemoryLayoutUsed.ToJson(), Telementry_Frequency.RealTime).Result;
                        Trace.WriteLine("MaxEasyMemoryLayoutUsed : " + DisplayFeatures_MaxEasyMemoryLayoutUsed.ToJson());
                        _log.Info($"[DeviceManagerPlugin] [DisplayFeatures_Functions] SentInfoToTelementry MaxEasyMemoryLayoutUsed Success.");
                        break;
                }
                result = true;
            }
            catch (Exception ex)
            {
                _log.Info($"[DeviceManagerPlugin] [DisplayFeatures_Functions] SentInfoToTelementry failed - Exception: {ex.Message}");
                return result;
            }
            return result;
        }

        public bool SentKVMToTelementry(ILog _log, ITelementryScheduler _TelementryScheduler, MonitorInfo monitorInfo, DisplayPropertiesInfo displayInfo, string mode, string val)
        {
            bool result = false;
            try
            {
                if (_TelementryScheduler == null)
                    return result;
                Properties currentProperties = displayInfo.SupportedProperties.Properties.FirstOrDefault(p => p.isCurrent);
                var basicInfo = CreateBasicDisplayInfo(monitorInfo, currentProperties, displayInfo);
                if (basicInfo == null)
                    return result;
                _log.Info($"[DeviceManagerPlugin] [DisplayFeatures_Functions] SentKVMToTelementry Start.");
                if (!string.IsNullOrEmpty(mode))
                {
                    switch (mode)
                    {
                        case "KVMMode":
                            var DisplayFeatures_Kvm = new DisplayFeatures_KVM();
                            DisplayFeatures_Kvm.KVMMode = val;

                            CopyBasicInfo(DisplayFeatures_Kvm, basicInfo);
                            if (_TelementryScheduler.ReceiveTelemetryInfo("DisplayFeatures", DisplayFeatures_Kvm.ToJson(), Telementry_Frequency.RealTime).Result)
                            {
                                result = true;
                                Trace.WriteLine("KVM : " + DisplayFeatures_Kvm.ToJson());
                                _log.Info($"[DeviceManagerPlugin] [DisplayFeatures_Functions] SentKVMToTelementry KVMMode Success.");
                            }
                            else
                            {
                                Trace.WriteLine("KVM : " + DisplayFeatures_Kvm.ToJson());
                                _log.Info($"[DeviceManagerPlugin] [DisplayFeatures_Functions] SentKVMToTelementry KVMMode Fail.");
                            }
                            break;
                        case "USBKVMMode":
                            var DisplayFeatures_UsbKvmmode = new DisplayFeatures_USBKVMMode();
                            DisplayFeatures_UsbKvmmode.USBKVMMode = val;

                            CopyBasicInfo(DisplayFeatures_UsbKvmmode, basicInfo);
                            if (_TelementryScheduler.ReceiveTelemetryInfo("DisplayFeatures", DisplayFeatures_UsbKvmmode.ToJson(), Telementry_Frequency.RealTime).Result)
                            {
                                result = true;
                                Trace.WriteLine("USBKVMMode : " + DisplayFeatures_UsbKvmmode.ToJson());
                                _log.Info($"[DeviceManagerPlugin] [DisplayFeatures_Functions] SentKVMToTelementry USBKVMMode Success.");
                            }
                            else
                            {
                                Trace.WriteLine("USBKVMMode : " + DisplayFeatures_UsbKvmmode.ToJson());
                                _log.Info($"[DeviceManagerPlugin] [DisplayFeatures_Functions] SentKVMToTelementry USBKVMMode Fail.");
                            }
                            break;
                    }
                }
                else
                {
                    _log.Info($"[DeviceManagerPlugin] [DisplayFeatures_Functions] mode is null or empty.");
                }
            }
            catch (Exception ex)
            {
                _log.Info($"[DeviceManagerPlugin] [DisplayFeatures_Functions] SentInfoToTelementry failed - Exception: {ex.Message}");
                return result;
            }
            return result;
        }

        private void CopyBasicInfo(DisplayFeaturesBasic target, DisplayFeaturesBasic basicInfo)
        {
            target.CommunicationPath = basicInfo.CommunicationPath;
            target.DisplayModelname = basicInfo.DisplayModelname;
            target.DisplayServiceTag = basicInfo.DisplayServiceTag;
            target.FirmwareVersion = basicInfo.FirmwareVersion;
            target.ScreenSize = basicInfo.ScreenSize;
            target.Scalefactor = basicInfo.Scalefactor;
            target.CurrentResolution = basicInfo.CurrentResolution;
            target.MaxResolution = basicInfo.MaxResolution;
        }
    }
}
