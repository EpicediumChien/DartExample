using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDPM.UI.Plugin.DdpmHomePlugin.Interfaces
{
    public class WalkThroughInfo
    {
        public string ModelName { get; set; }
        public string ModelType { get; set; }

        public object DeviceInfo { get; set; }

        public WalkThroughInfo(string modelName, string modelType, object deviceInfo)
        {
            ModelName = modelName;
            ModelType = modelType;
            DeviceInfo = deviceInfo;
        }
    }
}
