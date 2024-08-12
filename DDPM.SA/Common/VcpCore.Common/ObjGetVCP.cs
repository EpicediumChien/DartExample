using System;
using System.Collections.Generic;
using static VcpCore.Common.User32;
using static VcpCore.Common.dxva2;

namespace VcpCore.Common
{
    [Serializable]

    public class ObjGetVCP
    {
        public bool result { get; set; }

        public object value { get; set; }
    }
}
