using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace DDPM.SA.Common
{
    public static class JudgmentList
    {
        //0913 Add by Bruce
        /// <summary>
        /// For monitors that automatically rotate OS, there is no need to set the screen orientation.
        /// </summary>
        public static readonly List<string> AutoRotateOSMonitorList = new List<string>
        {
            "P1425"
        };
        /// <summary>
        /// The keyboard and mouse need to have their model names replaced
        /// </summary>
        public static string ModelRename(string OriginalModel)
        {
            string newModel = "";
            if (!string.IsNullOrEmpty(OriginalModel))
            {
                newModel = OriginalModel;
                switch (OriginalModel)
                {
                    //Keyboard
                    case "KB740":
                    case "KB7120W":
                        newModel = "KB740";
                        break;
                    case "KB500":
                    case "KB3121W":
                        newModel = "KB500";
                        break;
                    case "KB700":
                    case "KB7221W":
                        newModel = "KB700";
                        break;

                    //Mouse
                    case "MS300":
                    case "MS3121W":
                        newModel = "MS300";
                        break;

                    default:
                        break;
                }
            }
            return newModel;
        }
    }
}