namespace DDPM.SA.Common.Display
{
    public class PowerNapSetting
    {
        //public EDID DeviceInfo { get; set; }

        public string ModelName { get; set; }
        public string SerialNumber { get; set; }
        public bool Status { get; set; }
        public PowerNapType RunType { get; set; }
    }

    public enum PowerNapType
    {
        Off,
        ReduceBrightness,
        SleepIfRunning
    }
}