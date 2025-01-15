namespace DDPM.SA.Common
{
    public class scheduleInfo
    {
        public bool IsEnable { get; set; } = false;
        public string model { get; set; } = string.Empty;
        public string serviceTag { get; set; } = string.Empty;
        public string Pre1Name { get; set; } = "Day";
        public string Pre2Name { get; set; } = "Night";
        public int Hours1 { get; set; } = 8;
        public int Mins1 { get; set; } = 0;
        public int Duration1 { get; set; } = 60;
        public int Hours2 { get; set; } = 5;
        public int Mins2 { get; set; } = 0;
        public int Duration2 { get; set; } = 60;
        public double Brightness1 { get; set; } = 75;
        public double Contrast1 { get; set; } = 75;
        public double Brightness2 { get; set; } = 75;
        public double Contrast2 { get; set; } = 75;

        public scheduleInfo()
        {
            IsEnable = false;
            model = string.Empty;
            serviceTag = string.Empty;
            Pre1Name = "Day";
            Pre2Name = "Night";
            Hours1 = 8;
            Mins1 = 0;
            Duration1 = 60;
            Hours2 = 5;
            Mins2 = 0;
            Duration2 = 60;
            Brightness1 = 75;
            Contrast1 = 75;
            Brightness2 = 75;
            Contrast2 = 75;
        }

        public scheduleInfo(scheduleInfo exist)
        {
            IsEnable = exist.IsEnable;
            model = exist.model;
            serviceTag = exist.serviceTag;
            Pre1Name = exist.Pre1Name;
            Pre2Name = exist.Pre2Name;
            Hours1 = exist.Hours1;
            Mins1 = exist.Mins1;
            Duration1 = exist.Duration1;
            Hours2 = exist.Hours2;
            Mins2 = exist.Mins2;
            Duration2 = exist.Duration2;
            Brightness1 = exist.Brightness1;
            Contrast1 = exist.Contrast1;
            Brightness2 = exist.Brightness2;
            Contrast2 = exist.Contrast2;
        }
    }

    //#region DDM Schedule Brightness/Contrast
    //public class BriConProfile
    //{
    //    public bool IsOverwrittenPresetName { get; set; }
    //    public string PresetName { get; set; }
    //    /// <summary>
    //    /// "hh:mm tt" format
    //    /// </summary>
    //    public string Time { get; set; }
    //    /// <summary>
    //    /// unit is minute
    //    /// </summary>
    //    public byte Duration { get; set; }
    //    public ushort Brightness { get; set; }
    //    public ushort Contrast { get; set; }
    //}

    //public class BriConSchedule
    //{
    //    public bool IsOverwrittenTargetValue { get; set; }
    //    public bool IsEnabled { get; set; }
    //    public bool IsSync { get; set; }
    //    public BriConProfile Profile1 { get; set; }
    //    public BriConProfile Profile2 { get; set; }
    //}
    //#endregion
}