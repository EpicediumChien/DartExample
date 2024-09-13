using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDPM.SA.Common
{
    public class MonitorAssetReport
    {
        public string ModelName { get; set; }
        public string Manufacturer { get; set; }
        public string PlugandPlayID { get; set; }
        public string SerialNumber { get; set; }
        public string DateOfManufacture { get; set; }
        public string Age { get; set; }
        public string ScreenSize { get; set; }
        public string InputFrequency { get; set; }
        public string PhysicalOrientation { get; set; }
        public string TechnologyType { get; set; }
        public string UsageTime { get; set; }
        public string ControllerID { get; set; }
        public string FirmwareVersion { get; set; }
        public string PowerState { get; set; }
        public string OptimalResolution { get; set; }
        public string OptimalAspectRatio { get; set; }
        public string Connection { get; set; }
    }
}
