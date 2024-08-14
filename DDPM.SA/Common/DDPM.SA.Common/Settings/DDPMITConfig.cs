using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDPM.SA.Common.Settings
{
    public class DDPMITConfig
    {
        public double Version { get; set; } = 1.0;

        //global setting -> Analytics page -> checkbox enable/disable
        public bool isTelemetryConsentAllow { get; set; } = true; 
    }
}
