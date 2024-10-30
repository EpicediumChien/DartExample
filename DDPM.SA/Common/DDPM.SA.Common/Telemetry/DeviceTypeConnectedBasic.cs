using Newtonsoft.Json;
using System.Collections.Generic;

namespace DDPM.SA.Common
{
    public class DeviceTypeConnectedBasic
    {
        public List<string> DisplayServiceTag { get; set; }

        public List<string> DisplayModelname { get; set; }

        public List<string> ConnectedType { get; set; }

        public DeviceTypeConnectedBasic()
        {
            ConnectedType = new List<string>();
            DisplayModelname = new List<string>();
            DisplayServiceTag = new List<string>();
        }

        public string ToJson()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }
    }

    public class DeviceTypeConnected_VideoPortInUsed : DeviceTypeConnectedBasic
    {
        public int totalnum { get; set; }
    }
}