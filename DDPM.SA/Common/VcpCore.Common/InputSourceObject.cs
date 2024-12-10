using System;

namespace VcpCore.Common
{
    [Serializable]
    public class InputSourceObject
    {
        public string Name { get; set; } = string.Empty;

        public uint value { get; set; } = 0x0;
    }
}