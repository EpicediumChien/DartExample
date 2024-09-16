using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDPM.SA.Common
{
    public static class JudgmentList
    {
        //0913 Add by Bruce
        /// <summary>
        /// For monitors that automatically rotate OS, there is no need to set the screen orientation.
        /// </summary>
        public static List<string> AutoRotateOSMonitorList = new List<string>
        {
            "P1425"
        };

    }
}