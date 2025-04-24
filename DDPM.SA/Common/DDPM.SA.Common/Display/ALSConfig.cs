using System.Collections.Generic;
using VcpCore.Common;

namespace DDPM.SA.Common
{
    /// <summary>
    /// Record ALS command type
    /// </summary>
    public enum ALSFeatureQueryType
    {
        not_use = 0,
        MMS = 1,
        PrimaryMonitorSync = 2,
        AutoBrightness = 3,
        AutoBrightnessRangeLevel = 4,
        AutoColorTemperature = 5,
        All = 6,
        no_SerialNumber = 7,
        BrightnessValue = 8,
        ContrastValue = 9,
        ColorPresetString = 10,
        no_PrimaryMonitorSyncCheck = 11,
        ALSValueSyncCheck = 12,
        InAppAutoBriTemp= 13,
    }

    public class AutoBrightnessRangeLevel
    {
        public string level_name { get; set; } = string.Empty;
        public double level_value { get; set; } = 0.0;
    }

    /// <summary>
    /// Record ALS value
    /// </summary>
    public class ALSConfig
    {
        public MonitorInfo MoInfo { get; set; } = new MonitorInfo();
        public EDID Edid { get; set; } = new EDID();
        public Dictionary<string, List<string>> CapDict { get; set; } = new Dictionary<string, List<string>>();
        public string ModelName { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string serialNumber { get; set; } = string.Empty;
        public int isSupportALS { get; set; } = 0;
        public bool isMMSEnable { get; set; } = false;
        public bool isPrimaryMonitorSync { get; set; } = false;
        public bool isAutoBrightness { get; set; } = false;
        public bool isAutoColorTemp { get; set; } = false;
        public int LiftTone { get; set; } = 0;
        public int BrightnessValue { get; set; } = 0;
        public int ContrastValue { get; set; } = 0;
        //public int ColorTempValue { get; set; } = 0;
        public string ColorPresetString { get; set; } = string.Empty;
        public bool isSupportLum { get; set; } = false;
        public AutoBrightnessRangeLevel AutoBrightnessRangeLevel = new AutoBrightnessRangeLevel { level_value = 0, level_name = "Low" };

        public uint AllValue { get; set; } = 0;

        public bool result { get; set; } = false;

        public bool isBusy { get; set; } = false;
        public bool copyByType(ALSFeatureQueryType type, ALSConfig source, ref ALSConfig target)
        {
            switch (type)
            {
                case ALSFeatureQueryType.AutoBrightness:
                    target.isAutoBrightness = source.isAutoBrightness; break;
                case ALSFeatureQueryType.AutoColorTemperature:
                    target.isAutoColorTemp = source.isAutoColorTemp; break;
                case ALSFeatureQueryType.PrimaryMonitorSync:
                    target.isPrimaryMonitorSync = source.isPrimaryMonitorSync; break;
                case ALSFeatureQueryType.MMS:
                    target.isMMSEnable = source.isMMSEnable; break;
                case ALSFeatureQueryType.All:
                    {
                        target.MoInfo = source.MoInfo;
                        target.Edid = source.Edid;
                        target.CapDict = source.CapDict;
                        target.isSupportALS = source.isSupportALS;
                        target.isAutoBrightness = source.isAutoBrightness;
                        target.isAutoColorTemp = source.isAutoColorTemp;
                        target.isPrimaryMonitorSync = source.isPrimaryMonitorSync;
                        target.isMMSEnable = source.isMMSEnable;
                        target.serialNumber = source.serialNumber;
                        target.AutoBrightnessRangeLevel = source.AutoBrightnessRangeLevel;
                        target.AllValue = source.AllValue;
                        target.ModelName = source.ModelName;
                        break;
                    }
                case ALSFeatureQueryType.no_SerialNumber:
                    {
                        target.MoInfo = source.MoInfo;
                        target.Edid = source.Edid;
                        target.CapDict = source.CapDict;
                        target.isSupportALS = source.isSupportALS;
                        target.isAutoBrightness = source.isAutoBrightness;
                        target.isAutoColorTemp = source.isAutoColorTemp;
                        target.isPrimaryMonitorSync = source.isPrimaryMonitorSync;
                        target.isMMSEnable = source.isMMSEnable;
                        target.AutoBrightnessRangeLevel = source.AutoBrightnessRangeLevel;
                        target.AllValue = source.AllValue;
                        break;
                    }
            }
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <param name="source"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public bool checkValue(ALSFeatureQueryType type, ALSConfig source, ALSConfig target)
        {
            bool checkValueResult = false;
            switch (type)
            {
                case ALSFeatureQueryType.AutoBrightness:
                    if(source.isAutoBrightness == target.isAutoBrightness)
                        checkValueResult = true;
                    break;
                case ALSFeatureQueryType.AutoColorTemperature:
                    if (source.isAutoColorTemp == target.isAutoColorTemp)
                        checkValueResult = true;
                    break;
                case ALSFeatureQueryType.PrimaryMonitorSync:
                    if (source.isPrimaryMonitorSync == target.isPrimaryMonitorSync)
                        checkValueResult = true;
                    break;
                case ALSFeatureQueryType.AutoBrightnessRangeLevel:
                    if (source.AutoBrightnessRangeLevel == target.AutoBrightnessRangeLevel)
                        checkValueResult = true;
                    break;
                case ALSFeatureQueryType.BrightnessValue:
                    if (source.BrightnessValue == target.BrightnessValue)
                        checkValueResult = true;
                    break;
                case ALSFeatureQueryType.ContrastValue:
                    if (source.ContrastValue == target.ContrastValue)
                        checkValueResult = true;
                    break;
               //case ALSFeatureQueryType.ColorTempValue://vcp 68
               //     if (source.ColorTempValue == target.ColorTempValue)
               //         checkValueResult = true;
               //     break;
                case ALSFeatureQueryType.ColorPresetString:
                    if (source.ColorPresetString == target.ColorPresetString)
                        checkValueResult = true;
                    break;
                case ALSFeatureQueryType.no_PrimaryMonitorSyncCheck:
                    if (source.isAutoBrightness == target.isAutoBrightness &&
                        source.isAutoColorTemp == target.isAutoColorTemp &&
                        source.AutoBrightnessRangeLevel == target.AutoBrightnessRangeLevel &&
                        source.BrightnessValue == target.BrightnessValue &&
                        source.ContrastValue == target.ContrastValue &&
                        source.ColorPresetString == target.ColorPresetString)
                        checkValueResult = true;
                    break;
                case ALSFeatureQueryType.ALSValueSyncCheck:
                    {
                        if (source.isAutoBrightness == target.isAutoBrightness &&
                            source.isAutoColorTemp == target.isAutoColorTemp &&
                            source.AutoBrightnessRangeLevel == target.AutoBrightnessRangeLevel)
                            checkValueResult = true;
                        break;
                    }
            }
            return checkValueResult;
        }
    }
}