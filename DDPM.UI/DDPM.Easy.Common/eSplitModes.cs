using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDPM.Easy.Common
{
    public enum eSplitModes
    {
         /// <summary>
        /// The SplitCtrl is used as an Icon in GUI, for example, in a SplitListView.
        /// It's default mode.
        /// </summary>
        Icon,

        /// <summary>
        /// The SplitCtrl is under Editing.
        /// </summary>
        Edit,

        /// <summary>
        /// The SplitCtrl are working in WorkWindow
        /// </summary>
        Work
    }
}
