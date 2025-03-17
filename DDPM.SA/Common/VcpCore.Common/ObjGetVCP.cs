using System;

namespace VcpCore.Common
{
    [Serializable]
    public class ObjGetVCP
    {
        public bool result { get; set; } = false;

        public object value { get; set; } = null;
    }
}