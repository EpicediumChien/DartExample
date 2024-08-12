using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        public string DisplayName { get; set; } = string.Empty;
        public string serialNumber { get; set; } = string.Empty;
        public int isSupportALS { get; set; } = 0;
        public bool isMMSEnable { get; set; } = false;
        public bool isPrimaryMonitorSync { get; set; } = false;
        public bool isAutoBrightness { get; set; } = false;
        public bool isAutoColorTemp { get; set; } = false;
        public int LiftTone { get; set; } = 0;
        public List<AutoBrightnessRangeLevel> AutoBrightnessRangeLevel { get; set; } = new List<AutoBrightnessRangeLevel>();

        public uint AllValue { get; set; } = 0;

        public bool result { get; set; } = false;

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
                        target.isSupportALS = source.isSupportALS;
                        target.isAutoBrightness = source.isAutoBrightness;
                        target.isAutoColorTemp = source.isAutoColorTemp;
                        target.isPrimaryMonitorSync = source.isPrimaryMonitorSync;
                        target.isMMSEnable = source.isMMSEnable;
                        target.serialNumber = source.serialNumber;
                        target.AutoBrightnessRangeLevel = source.AutoBrightnessRangeLevel;
                        target.AllValue = source.AllValue;
                        break;
                    }
                case ALSFeatureQueryType.no_SerialNumber:
                    {
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
    }
}
