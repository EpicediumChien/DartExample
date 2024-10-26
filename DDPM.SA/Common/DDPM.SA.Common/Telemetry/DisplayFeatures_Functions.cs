using DdmLibrary.Utility;
using DDPM.SA.Common.Settings;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VcpCore.Common;
using static DDPM.SA.Common.DisplayFeaturesBasic;

namespace DDPM.SA.Common
{
    public class DisplayFeatures_Functions
    {
        private DisplayFeatures_EasyMemory CreateBasicDisplayInfo(MonitorInfo monitorInfo, Properties currentProperties, DisplayPropertiesInfo displayInfo)
        {
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

        public bool SentInfoToTelementry(ITelementryScheduler _TelementryScheduler, MonitorInfo monitorInfo, DisplayPropertiesInfo displayInfo, EasyArrangementDDPM easyArrangementDDPM, DDPMSettings ddpmsetting, string ez)
        {
            Properties currentProperties = displayInfo.SupportedProperties.Properties.FirstOrDefault(p => p.isCurrent);
            var basicInfo = CreateBasicDisplayInfo(monitorInfo, currentProperties, displayInfo);
            bool result = false;
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
                    break;

                case "EasyMemoryProfileCount":
                    var DisplayFeatures_EasyMemoryProfileCount = new DisplayFeatures_EasyMemoryProfileCount
                    {
                        profile_count_value = (uint)ddpmsetting.UserSettings.EAProfile.Count,
                    };
                    CopyBasicInfo(DisplayFeatures_EasyMemoryProfileCount, basicInfo);
                    result = _TelementryScheduler.ReceiveTelemetryInfo("DisplayFeatures", DisplayFeatures_EasyMemoryProfileCount.ToJson(), Telementry_Frequency.RealTime).Result;
                    Trace.WriteLine("EasyMemoryProfileCount : " + DisplayFeatures_EasyMemoryProfileCount.ToJson());
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
                    break;
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
