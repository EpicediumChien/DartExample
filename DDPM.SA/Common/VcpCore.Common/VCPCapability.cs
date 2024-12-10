using System;

namespace VcpCore.Common
{
    [Serializable]
    public class VCPCapability
    {
        public string Name { get; set; } = string.Empty;

        public char OptCode { get; set; } = default(char);

        public uint Value { get; set; } = 0x0;

        public uint MaxValue { get; set; } = 0x0;
    }
}