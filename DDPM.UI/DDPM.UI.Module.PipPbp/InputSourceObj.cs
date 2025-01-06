namespace DDPM.UI.Module.PipPbp
{
    //InputSource {VCP 0x60 Code, Input Source Name} mapping table
    //
    //Robert_Lin 2024-6, temporary remove by change its className
    public class InputSourceObj_Unused
    {
        public InputSourceObj_Unused()
        {
        }

        public InputSourceObj_Unused(UInt16 code)
        {
            Code = code;
            int idx = Array.FindIndex<InputSourceObj_Unused>(InputSourceMappingTable, x => x.Code == code);
            if (idx >= 0)
                Name = InputSourceMappingTable[idx].Name;
        }

        public InputSourceObj_Unused(UInt16 code, string name)
        {
            Code = code;
            Name = name;
        }

        public UInt16 Code { get; set; } = 0;
        public string Name { get; set; } = string.Empty;

        public readonly static InputSourceObj_Unused[] InputSourceMappingTable = new InputSourceObj_Unused[]
        {
            new InputSourceObj_Unused(0x01, "VGA-1"),
            new InputSourceObj_Unused(0x02, "VGA-2"),
            new InputSourceObj_Unused(0x03, "DVI-1"),
            new InputSourceObj_Unused(0x04, "DVI-2"),
            new InputSourceObj_Unused(0x05, "Composite video 1"),
            new InputSourceObj_Unused(0x06, "Composite video 2"),
            new InputSourceObj_Unused(0x07, "S-Video-1"),
            new InputSourceObj_Unused(0x08, "S-Video-2"),
            new InputSourceObj_Unused(0x09, "Tuner-1"),
            new InputSourceObj_Unused(0x0A, "Tuner-2"),
            new InputSourceObj_Unused(0x0B, "Tuner-3"),
            new InputSourceObj_Unused(0x0C, "Component video (YPrPb/YCrCb) 1"),
            new InputSourceObj_Unused(0x0D, "Component video (YPrPb/YCrCb) 2"),
            new InputSourceObj_Unused(0x0E, "Component video (YPrPb/YCrCb) 3"),
            new InputSourceObj_Unused(0x0F, "DisplayPort-1"),
            new InputSourceObj_Unused(0x10, "Mini DisplayPort-1"),
            new InputSourceObj_Unused(0x11, "HDMI-1"),
            new InputSourceObj_Unused(0x12, "HDMI-2"),
            new InputSourceObj_Unused(0x13, "DisplayPort-2"),
            new InputSourceObj_Unused(0x14, "Mini DisplayPort-2"),
            new InputSourceObj_Unused(0x15, "HDMI3"),
            new InputSourceObj_Unused(0x16, "HDMI4"),
            new InputSourceObj_Unused(0x17, "DisplayPort-3"),
            new InputSourceObj_Unused(0x18, "Mini DisplayPort-3"),
            new InputSourceObj_Unused(0x19, "Thunderbolt-1"),
            new InputSourceObj_Unused(0x1A, "Thunderbolt-2"),
            new InputSourceObj_Unused(0x1B, "USB-C1"),
            new InputSourceObj_Unused(0x1C, "USB-C2"),
            new InputSourceObj_Unused(0x1D, "USB-C3"),
            new InputSourceObj_Unused(0x1E, "USB-C4"),
            new InputSourceObj_Unused(0x80, "USB Comm from USB1 (Type-B, port 1)"),
            new InputSourceObj_Unused(0x81, "USB Comm from USB2 (Type-B, port 2)"),
            new InputSourceObj_Unused(0x82, "USB Comm from USB-C1 (Type-C, port 1)"),
            new InputSourceObj_Unused(0x83, "USB Comm from USB-C2 (Type-C, port 2)"),
            new InputSourceObj_Unused(0x84, "USB Comm from USB-C3 (Type-C, port 3)"),
            new InputSourceObj_Unused(0x85, "USB Comm from USB-C4 (Type-C, port 4)"),
        };
    }
}