using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDPM.SA.Common
{
    public class DeviceFirmwareBasic
    {
        public string FWVersionFrom { get; set; }
        public string FWVersionTo { get; set; }
        public string Results { get; set; }
        public string SupplierID { get; set; }
        public string FW_Available_date { get; set; }
        public string FW_Update_date { get; set; }

        public DeviceFirmwareBasic()
        {
            FWVersionFrom = string.Empty;
            FWVersionTo = string.Empty;
            Results = string.Empty;
            SupplierID = string.Empty;
            FW_Available_date = string.Empty;
            FW_Update_date = string.Empty;
        }

        public string ToJson()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }
    }
    public class DisplayFirmware : DeviceFirmwareBasic
    {
        public string DisplayModelname { get; set; }
        public string DisplayServiceTag { get; set; }
        public string D_Ctrl { get; set; }
    }
    public class DisplayFirmware_List
    {
        public List<DisplayFirmware> DisplayFirmwareList { get; set; }
        public string ToJson()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }
    }
    public class PeripheralsFirmware : DeviceFirmwareBasic
    {
        public string DeviceModelNumber { get; set; }
        public string Connectivity { get; set; }
    }
    public class PeripheralsFirmware_List
    {
        public List<PeripheralsFirmware> PeripheralsFirmwareList { get; set; }
        public string ToJson()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }
    }
}
