using System;
using System.Collections.Generic;

namespace VcpCore.Common
{
    [Serializable]
    public class VCPCapability
    {
        public string Name { get; set; }

        public char OptCode { get; set; }

        public uint Value { get; set; }

        public uint MaxValue { get; set; }
    }
}
