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
        public string ScreenSize { get; set; }
        public string scalefactor { get; set; }
        public string CurrentResolution { get; set; }
        public string MaxResolution { get; set; }

        public DisplayFeaturesBasic()
        {
            CommunicationPath = "Video";
            DisplayModelname = string.Empty;
            DisplayServiceTag = string.Empty;
            FirmwareVersion = string.Empty;
            ScreenSize = string.Empty;
            scalefactor = string.Empty;
            CurrentResolution = string.Empty;
            MaxResolution = string.Empty;
        }

        public string ToJson()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }
    }

    public class DisplayFeatures_KVM : DisplayFeaturesBasic
    {
        public string KVMMode { get; set; } = string.Empty;
    }

    public class DisplayFeatures_USBKVMMode : DisplayFeaturesBasic
    {
        public string USBKVMMode { get; set; } = string.Empty;
    }
}
