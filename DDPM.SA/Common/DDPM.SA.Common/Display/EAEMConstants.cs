using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDPM.SA.Common.Display
{
    /// <summary>
    /// The Constnats for EasyArrange and EasyMemory
    /// </summary>
    public class EAEMConstants
    {
        public const int MaxCustomItems = 5; //DDPMW-843
        public const int MaxCustomNameLenth = 30; //DDPMW-843
        public const int MaxRecentItems = 5; //DDPMW-840, 1 static item (Off) + 4 per-monitor MRU items.

        //The EAID of the first Custom Layout, =1000 => The EAID of the first Custom layout is 1000
        public const int EAID_FirstCustom = 1000;

        //The Command strings of EAArgs.Command
        //SetIsSpanEnabled: When IsSpaneEnabled flag changed, provide new value in EAArgs.Result
        public const string EACommand_SetIsSpanEnabled = "SetIsSpanEnabled";

    }
}
