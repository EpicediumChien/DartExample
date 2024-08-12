using System;
using System.Collections.Generic;

namespace DDPM.UI.Common {
    [Serializable]
    public class VCPCapability
    {
        public string Name { get; set; }

        public char OptCode { get; set; }

        public uint Value { get; set; }

        public uint MaxValue { get; set; }
    }

    public static class VcpCodeList
    {

        public static Dictionary<string, byte> VCPDC = new Dictionary<string, byte>
        {
            //{ "Standard/Native", 0x00 },
            { "Standard", 0x00 },
            { "Native", 0x00 },
            { "Multimedia", 0x02 },
            { "Movie", 0x3 },
            { "Nature", 0x4 },
            { "Game/Game1", 0x5 },
            { "Sport", 0x6 }
        };

        public static Dictionary<string, byte> VCPF0 = new Dictionary<string, byte>
        {
            { "Text", 1 },
            { "AdobeRGB", 2 },
            { "AdobeRGB1", 0x21 },
            { "AdobeRGB2", 0x22 },
            { "Adobe RGB D65 G2.2 L160", 0x21 },
            { "Adobe RGB D50 G2.2 L160", 0x22 },
            { "Adobe RGB D65 G2.2 L250", 0x21 },
            { "Adobe RGB D50 G2.2 L250", 0x22 },
            { "xvMode", 0x3 },
            { "DICOM", 0x4 },
            { "CAL1", 0x5 },
            { "Custom 1 / User 1", 0xC1 },
            { "Custom 2 / User 2", 0xC2 },
            { "Custom 3 / User 3", 0xC3 },
            { "CAL2", 0x6 },
            { "Metro", 0x7 },
            { "Paper", 0x8 },
            { "Rec 709", 0x9 },
            { "DCI-P3", 0x0A },
            { "Rec2020", 0x0B },
            { "ComfortView", 0x0C },
            { "Game2", 0x0D },
            { "Game3", 0x0E },
            { "FPS Game", 0xF },
            { "RTS Game", 0x10 },
            { "RPG Game", 0x11 },
            { "SPORTS Game", 0x13 },
            { "Standard HDR", 0x30 },
            { "Movie HDR", 0x31 },
            { "Game HDR", 0x32 },
            { "Vivid HDR", 0x33 },
            { "Desktop", 0x34 },
            { "Reference", 0x35 },
            { "Multiscreen Match", 0x12 },
            { "DisplayHDR", 0x36 },
            { "HDR10", 0x37 },
            { "HLG", 0x38 }
        };

        public static Dictionary<string, byte> VCP14 = new Dictionary<string, byte>
        {
            { "sRGB", 0x01 },
            { "5000K", 0x04 },
            { "5700K", 0x0B },
            { "Warm", 0x0B },
            { "6500K", 0x05 },
            { "7500K", 0x06 },
            { "9300K", 0x08 },
            { "Cool", 0x08 },
            { "10000K", 0x09 },
            { "Custom Color", 0x0C }
        };

        public static Dictionary<int, string> VCPE2 = new Dictionary<int, string>
        {
            //{ 0x00, "Standard/Native" },
            { 0x00, "Standard" },
            { 0x00, "Native" },
            { 0x01, "Multimedia" },
            { 0x02, "Movie" },
            { 0x03, "Nature" },
            { 0x04, "Game/Game1" },
            { 0x05, "Sport" },
            { 0x06, "Text" },
            { 0x07, "AdobeRGB" },
            { 0x2A, "AdobeRGB1 (D65G2.2L250)" },
            { 0x2B, "AdobeRGB2 (D50G2.2L250)" },
            { 0x08, "xvMode" },
            { 0x09, "DICOM" },
            { 0x0A, "CAL1" },
            { 0x0B, "sRGB" },
            { 0x0C, "5000K" },
            { 0x0D, "5700K" },
            { 0x0E, "Warm" },
            { 0x0F, "6500K" },
            { 0x10, "7500K" },
            { 0x11, "9300K" },
            { 0x12, "Cool" },
            { 0x13, "10000K" },
            { 0x14, "Custom Color" },
            { 0x2C, "Custom 1 / User 1" },
            { 0x2D, "Custom 2 / User 2" },
            { 0x2E, "Custom 3 / User 3" },
            { 0x15, "CAL2" },
            { 0x18, "Metro" },
            { 0x19, "Paper" },
            { 0x1A, "Rec 709" },
            { 0x1B, "DCI-P3" },
            { 0x1C, "Rec2020" },
            { 0x1D, "ComfortView" },
            { 0x1E, "Game2" },
            { 0x1F, "Game3" },
            { 0x20, "FPS Game" },
            { 0x21, "RTS Game" },
            { 0x22, "RPG Game" },
            { 0x2F, "SPORTS Game" },
            { 0x25, "Standard HDR" },
            { 0x23, "Movie HDR" },
            { 0x24, "Game HDR" },
            { 0x26, "Vivid HDR" },
            { 0x27, "Desktop" },
            { 0x28, "Reference" },
            { 0x29, "Multiscreen Match" },
            { 0x3A, "DisplayHDR" },
            { 0x3B, "HDR10" },
            { 0x3C, "HLG" },
            { 0x7F, "Presets Disabled" }
        };


        public struct VcpValue
        {
            public byte Vcp;
            public byte Value;

            public override string ToString()
            {
                return $"VCP: 0x{Vcp.ToString("X")}, Value:{Value}";
            }
        }


        public static VcpValue? getVcpAndValue(string presetName)
        {
            VcpValue retValue = new VcpValue();

            string lowpresetName = presetName.ToLower();

            foreach (KeyValuePair<string, byte> item2 in VCPDC)
            {
                if (item2.Key.ToLower().Equals(lowpresetName))
                {
                    retValue.Vcp = 0xDC;
                    retValue.Value = (byte)item2.Value;
                    return retValue;
                }
            }

            foreach (KeyValuePair<string, byte> item3 in VCPF0)
            {
                if (item3.Key.ToLower().Equals(lowpresetName))
                {
                    retValue.Vcp = 0xF0;
                    retValue.Value = (byte)item3.Value;
                    return retValue;
                }
            }

            foreach (KeyValuePair<string, byte> item4 in VCP14)
            {
                if (item4.Key.ToLower().Equals(lowpresetName))
                {
                    retValue.Vcp = 0x14;
                    retValue.Value = (byte)item4.Value;
                    return retValue;
                }
            }

            return null;
        }
    }
}
