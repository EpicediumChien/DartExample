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

        public WalkThroughInfo(string modelName, string modelType)
        {
            ModelName = modelName;
            ModelType = modelType;
        }
    }
}
