using VcpCore.Common;

namespace DDPM.SA.Common
{
    public class scheduleInfo
    {
        public bool IsEnable { get; set; } = false;
        public EDID Monitor { get; set; }
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
}