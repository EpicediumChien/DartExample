using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDPM.SA.Common
{
    public class DisplayFeaturesBasic
    {
        public string CommunicationPath { get; set; }
        public string DisplayModelname { get; set; }
        public string DisplayServiceTag { get; set; }
        public string FirmwareVersion { get; set; }
        public float ScreenSize { get; set; }
        public double Scalefactor { get; set; }
        public string CurrentResolution { get; set; }
        public string MaxResolution { get; set; }

        public DisplayFeaturesBasic()
        {
            CommunicationPath = "Video";
            DisplayModelname = string.Empty;
            DisplayServiceTag = string.Empty;
            FirmwareVersion = string.Empty;
            ScreenSize = 0x0;
            Scalefactor = 0x0;
            CurrentResolution = string.Empty;
            MaxResolution = string.Empty;
        }

        public string ToJson()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }

        public class DisplayFeatures_EasyMemory : DisplayFeaturesBasic
        {
            public bool manual { get; set; } = false;
            public bool auto { get; set; } = false;

            public uint scheduled { get; set; } = 0x0;
        }
        public class DisplayFeatures_EasyMemoryProfileCount : DisplayFeaturesBasic
        {
            public uint profile_count_value { get; set; } = 0x0;
        }
        public class DisplayFeatures_KVM : DisplayFeaturesBasic
        {
            public string KVMMode { get; set; } = string.Empty;
        }
        public class DisplayFeatures_MaxEasyMemoryLayoutUsed : DisplayFeaturesBasic
        {
            public uint maximum_widows_among_profile { get; set; } = 0x0;
        }
        public class DisplayFeatures_USBKVMMode : DisplayFeaturesBasic
        {
            public string USBKVMMode { get; set; } = string.Empty;
        }
    }

}