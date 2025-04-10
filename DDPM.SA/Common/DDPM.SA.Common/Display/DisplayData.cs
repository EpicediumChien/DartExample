using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VcpCore.Common;

namespace DDPM.SA.Common.Display
{
    public class DisplayData
    {
        public string Model = string.Empty;
        public string ServiceTag = string.Empty;
        public List<InputCode> InputSourceList = new List<InputCode>();
        public List<USBPorts> USBList = new List<USBPorts>();
        public List<InputSource_USB> DisplayUSB = new List<InputSource_USB>();
        public uint VCP_E9 = 1;
        public DisplayPropertiesInfo DisplayPropertiesInfo = new DisplayPropertiesInfo();
        public GamingDisplayPropertiesInfo GamingDisplayPropertiesInfo = new GamingDisplayPropertiesInfo();
        public Color Color = new Color();
    }
    public class InputSource_USB
    {
        public string inputSource = string.Empty;
        public string USB = string.Empty;
    }
    public class USBPorts
    {
        public string USBName = string.Empty;
        public string USBPort = string.Empty;
    }
    public class InputCode
    {
        public string InputSource = string.Empty;
        public uint Code = 0;
    }
    public class Color
    {
        public string color_DisHDR = string.Empty;
        public string color_EnHDR = string.Empty;
    }
}
