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