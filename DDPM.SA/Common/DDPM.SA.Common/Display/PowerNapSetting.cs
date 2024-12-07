using System.Text.Json.Serialization;

namespace DDPM.SA.Common.Display
{
    public class PowerNapSetting
    {
        //public EDID DeviceInfo { get; set; }
        public string ModelName { get; set; }
        public string SerialNumber { get; set; }
        public string ServiceTag { get; set; }
        public bool Status { get; set; } = false;
        public PowerNapType RunType { get; set; } = PowerNapType.Off;
    }

    public enum PowerNapType
    {
        Off,
        ReduceBrightness,
        SleepIfRunning
    }
}