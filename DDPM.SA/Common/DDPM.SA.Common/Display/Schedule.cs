using VcpCore.Common;
using static VcpCore.Common.User32;

namespace DDPM.SA.Common
{
    public class scheduleInfo
    {
        public bool IsEnable { get; set; } = false;
        public string model { get; set; }
        public string serviceTag { get; set; }
        public string Pre1Name { get; set; }
        public string Pre2Name { get; set; }
        public int Hours1 { get; set; }
        public int Mins1 { get; set; }
        public int Duration1 { get; set; }
        public int Hours2 { get; set; }
        public int Mins2 { get; set; }
        public int Duration2 { get; set; }
        public double Brightness1 { get; set; }
        public double Contrast1 { get; set; }
        public double Brightness2 { get; set; }
        public double Contrast2 { get; set; }
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