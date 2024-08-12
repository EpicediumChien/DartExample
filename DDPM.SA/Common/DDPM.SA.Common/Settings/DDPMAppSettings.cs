using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDPM.SA.Common.Settings
{
    public class DDPMAppSettings
    {
        private static DDPMAppSettings appSettings;

        private static readonly object Lock = new object();

        public double Version { get; set; } = 1.0; //consider how to control the setting's version in the feature

        [JsonConstructor]
        public DDPMAppSettings(double ver)
        {
            Version = ver;
        }

        public DDPMAppSettings()
        {
        }
    }
}
