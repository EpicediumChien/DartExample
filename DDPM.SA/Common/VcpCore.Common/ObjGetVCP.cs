using System;

namespace VcpCore.Common
{
    [Serializable]
    public class ObjGetVCP
    {
        public bool result { get; set; }

        public object value { get; set; }
    }
}