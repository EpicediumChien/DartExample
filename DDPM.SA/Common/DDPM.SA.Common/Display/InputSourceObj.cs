using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDPM.SA.Common.Display
{
    //Robert_Lin 2024-6-5 created, used by PipPbpManager plugin.
    //I need original code of input source for the Sub Input source.
    //
    public class InputSourceObj
    {
        public InputSourceObj()
        {

        }
        public InputSourceObj(UInt16 code)
        {
            Code = code;
            int idx = Array.FindIndex<InputSourceObj>(InputSourceMappingTable, x => x.Code == code);
            if (idx >= 0)
                Name = InputSourceMappingTable[idx].Name;
        }
        public InputSourceObj(string name)
        {
            Name = name;
            int idx = Array.FindIndex<InputSourceObj>(InputSourceMappingTable, x => x.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
            if (idx >= 0)
                Code = InputSourceMappingTable[idx].Code;
        }

        public InputSourceObj(UInt16 code, string name)
        {
            Code = code;
            Name = name;
        }
        public UInt16 Code { get; set; } = 0;
        public string Name { get; set; } = string.Empty;


        //InputSource {VCP 0x60 Code, Input Source Name} mapping table
        // Sample code to use:
        //  InputSourceObj sub1 = Array.Find(InputSourceObj.InputSourceMappingTable, x => x.Code == 0x11);
        public static readonly InputSourceObj[] InputSourceMappingTable = new InputSourceObj[]
        {
            new InputSourceObj(0x01, "VGA-1"),
            new InputSourceObj(0x02, "VGA-2"),
            new InputSourceObj(0x03, "DVI-1"),
            new InputSourceObj(0x04, "DVI-2"),
            new InputSourceObj(0x05, "Composite video 1"),
            new InputSourceObj(0x06, "Composite video 2"),
            new InputSourceObj(0x07, "S-Video-1"),
            new InputSourceObj(0x08, "S-Video-2"),
            new InputSourceObj(0x09, "Tuner-1"),
            new InputSourceObj(0x0A, "Tuner-2"),
            new InputSourceObj(0x0B, "Tuner-3"),
            new InputSourceObj(0x0C, "Component video (YPrPb/YCrCb) 1"),
            new InputSourceObj(0x0D, "Component video (YPrPb/YCrCb) 2"),
            new InputSourceObj(0x0E, "Component video (YPrPb/YCrCb) 3"),
            new InputSourceObj(0x0F, "DisplayPort-1"),
            new InputSourceObj(0x10, "Mini DisplayPort-1"),
            new InputSourceObj(0x11, "HDMI-1"),
            new InputSourceObj(0x12, "HDMI-2"),
            new InputSourceObj(0x13, "DisplayPort-2"),
            new InputSourceObj(0x14, "Mini DisplayPort-2"),
            new InputSourceObj(0x15, "HDMI3"),
            new InputSourceObj(0x16, "HDMI4"),
            new InputSourceObj(0x17, "DisplayPort-3"),
            new InputSourceObj(0x18, "Mini DisplayPort-3"),
            new InputSourceObj(0x19, "Thunderbolt-1"),
            new InputSourceObj(0x1A, "Thunderbolt-2"),
            new InputSourceObj(0x1B, "USB-C1"),
            new InputSourceObj(0x1C, "USB-C2"),
            new InputSourceObj(0x1D, "USB-C3"),
            new InputSourceObj(0x1E, "USB-C4"),
            new InputSourceObj(0x80, "USB Comm from USB1 (Type-B, port 1)"),
            new InputSourceObj(0x81, "USB Comm from USB2 (Type-B, port 2)"),
            new InputSourceObj(0x82, "USB Comm from USB-C1 (Type-C, port 1)"),
            new InputSourceObj(0x83, "USB Comm from USB-C2 (Type-C, port 2)"),
            new InputSourceObj(0x84, "USB Comm from USB-C3 (Type-C, port 3)"),
            new InputSourceObj(0x85, "USB Comm from USB-C4 (Type-C, port 4)"),
        };

        /// <summary>
        /// Find the matched InputSourceObj by name. It's compared with StartWith()
        /// Compare rules:
        /// 1 Remove white-space, and '-', then compare with String.StartWith()
        /// 2 Return the first item it found.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static InputSourceObj? FindFirstByName(string name)
        {
            //Remove ' ' and '-' in name
            string src = name.Replace(" ", String.Empty);
            src = src.Replace("-", String.Empty);

            //Search for matched
            foreach (InputSourceObj obj in InputSourceObj.InputSourceMappingTable)
            {
                string dst = obj.Name;
                dst = dst.Replace(" ", String.Empty);
                dst = dst.Replace("-", String.Empty);
                if (dst.StartsWith(src, StringComparison.OrdinalIgnoreCase))
                    return obj;
            }
            return null;
        }
    }
}
