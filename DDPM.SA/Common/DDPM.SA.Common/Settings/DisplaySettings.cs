using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace DDPM.SA.Common.Settings
{
    /// <summary>
    /// This class is used to save settings like EA/EM/... and the key is monitor's SerialNumber
    /// </summary>
    public class ModelSettings
    {
        public ModelSettings() { 
        }

        public string SerialNumber { get; set; }

        public override string ToString()
        {
            string output = $"SerialNumber : {SerialNumber}";

            return output;
        }
    }

    /// <summary>
    /// This class is uesd to save the settings to specific monitor Model, it allow several different SerialNumbers in this data
    /// </summary>
    public class DisplaySettings
    {
        public DisplaySettings()
        {
        }

        public string ModelName { get; set; }

        public List<ModelSettings> DUTs { get; set; } = new List<ModelSettings>();

        public override string ToString()
        {
            string output = "{\n" + $" {ModelName}\n";
            foreach(var dut in DUTs)
            {
                output += " {\n";
                output += "  " + dut.ToString() + "\n";
                output += " }\n";
            }
            output += "}\n";

            return output;
        }
    }
}
