using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDPM.SA.Common.Display
{
    /// <summary>
    /// Record ALS command type
    /// </summary>
    public enum DisplayImportResultCode
    {
        Error = -1,
        Done = 1,
        DoneWithEzMemoryCleared = 2
    }
}
