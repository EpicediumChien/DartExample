using Newtonsoft.Json;
using System.Collections.Generic;

namespace DDPM.SA.Common
{
    public class ApplicationSettingsBasic
    {
        public List<string> DisplayModelname { get; set; }
        public List<string> DisplayServiceTag { get; set; }
        public List<string> D_Ctrl { get; set; }

        public ApplicationSettingsBasic()
        {
            DisplayModelname = new List<string>();
            D_Ctrl = new List<string>();
            DisplayServiceTag = new List<string>();
        }

        public string ToJson()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }
    }

    public class ApplicationSettings_LockRotation : ApplicationSettingsBasic
    {
        public string LockRotation { get; set; }
    }

    public class ApplicationSettings_Settings : ApplicationSettingsBasic 
    {
        public string App_Copy_Settings { get; set; }
    }
}