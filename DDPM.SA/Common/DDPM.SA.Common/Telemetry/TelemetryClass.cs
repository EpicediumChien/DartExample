using System;

namespace DDPM.SA.Common
{
    public class FrequencyDateTime
    {
        public DateTime Month1stDay { get; set; }
        public DateTime PerDay { get; set; }
        public DateTime Weekly { get; set; }
    }

    public enum Telementry_Frequency
    {
        FirstDayofMonth,
        PerDay,
        Weekly,
        RealTime,
    }
}