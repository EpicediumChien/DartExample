using System;
using System.Collections.Generic;

namespace VcpCore.Plugins
{
    public class NodeFormatter : INodeFormatter
    {
        private static Dictionary<string, Func<string, string>> lookupTables = new Dictionary<string, Func<string, string>>()
        {
            { "vcp", (i) => FormatVCPControlName(i) },
            { "vcp_14", (i) => FormatVCP_14(i) },
            { "vcp_d6", (i) => FormatVCP_D6(i) },
            { "vcp_60", (i) => FormatVCP_60(i) },
            { "vcp_dc", (i) => FormatVCP_DC(i) },
            { "vcp_f0", (i) => FormatVCP_F0(i) },
            { "vcp_e2", (i) => FormatVCP_E2(i) },
            { "vcp_cc", (i) => FormatVCP_CC(i) },
            { "vcp_8d", (i) => FormatVCP_8D(i) },
            { "vcp_aa", (i) => FormatVCP_AA(i) },
            { "vcp_66", (i) => FormatVCP_66(i) }
        };

        public static string FormatVCPControlName(string vcpControlName)
        {
            switch (vcpControlName)
            {
                case "00": return "VCP Code Page";
                case "01": return "Degauss";
                case "02": return "New Control value";
                case "03": return "Soft Controls";
                case "04": return "Restore Factory Defaults";
                case "05": return "Restore Factory Brightness/Contrast Defaults";
                case "06": return "Restore Factory Geometry Defaults";
                case "08": return "Restore Factory Color Defaults";
                case "0a": return "Restore Factory TV Defaults";
                case "0b": return "Color Temperature Increment";
                case "0c": return "Color Temperature Request";
                case "0e": return "Clock";
                case "10": return "Brightness";
                case "11": return "Flesh Tone Enhancement";
                case "12": return "Contrast";
                case "13": return "Backlight Control";
                case "14": return "Basic Color Preset Select";
                case "16": return "Video Gain (Drive): Red";
                case "17": return "User Color Vision Compensation";
                case "18": return "Video Gain (Drive): Green";
                case "1a": return "Video Gain (Drive): Blue";
                case "1c": return "Focus";
                case "1e": return "Auto Setup";
                case "1f": return "Auto Color Setup";
                case "20": return "Horizontal Position (Phase)";
                case "22": return "Horizontal Size";
                case "24": return "Horizontal Pincushion";
                case "26": return "Horizontal Pincushion Balance";
                case "28": return "Horizontal Convergence R / B";
                case "29": return "Horizontal Convergence M / G";
                case "2a": return "Horizontal Linearity";
                case "2c": return "Horizontal Linearity Balance";
                case "2e": return "Gray Scale Expansion";
                case "30": return "Vertical Position (Phase)";
                case "32": return "Vertical Size";
                case "34": return "Vertical Pincushion";
                case "36": return "Vertical Pincushion Balance";
                case "38": return "Vertical Convergence R/B";
                case "39": return "Vertical Convergence M/G";
                case "3a": return "Vertical Linearity";
                case "3c": return "Vertical Linearity Balance";
                case "3e": return "Clock Phase";
                case "40": return "Horizontal Parallelogram";
                case "41": return "Vertical Parallelogram";
                case "42": return "Horizontal Keystone";
                case "43": return "Vertical Keystone";
                case "44": return "Rotation";
                case "46": return "Top Corner Flare";
                case "48": return "Top Corner Hook";
                case "4a": return "Bottom Corner Flare";
                case "4c": return "Bottom Corner Hook";
                case "50": return "Hue";
                case "52": return "Active Control";
                case "54": return "Performance Preservation";
                case "56": return "Horizontal Moire";
                case "58": return "Vertical Moire";
                case "59": return "6 Axis Saturation Control: Red";
                case "5a": return "6 Axis Saturation Control: Yellow";
                case "5b": return "6 Axis Saturation Control: Green";
                case "5c": return "6 Axis Saturation Control: Cyan";
                case "5d": return "6 Axis Saturation Control: Blue";
                case "5e": return "6 Axis Saturation Control: Magenta";
                case "60": return "Input Select";
                case "62": return "Audio Speaker Volume";
                case "63": return "Audio: Speaker Pair Select";
                case "64": return "Audio Microphone Volume";
                case "65": return "Audio: Jack Connection Status";
                case "66": return "Ambient Light Sensor";
                case "68": return "Language Select";
                case "6b": return "Backlight Level: White";
                case "6c": return "Video Black Level: Red";
                case "6d": return "Backlight Level: Red";
                case "6e": return "Video Black Level: Green";
                case "6f": return "Backlight Level: Green";
                case "70": return "Video Black Level: Blue";
                case "71": return "Backlight Level: Blue";
                case "72": return "Gamma";
                case "73": return "LUT Size";
                case "74": return "Single Point LUT Operation";
                case "75": return "Block LUT Operation";
                case "76": return "Remote Procedure Call";
                case "78": return "Display Identification Data Operation";
                case "7c": return "Adjust Zoom";
                case "82": return "Horizontal Mirror (Flip)";
                case "84": return "Vertical Mirror (Flip)";
                case "86": return "Display Scaling";
                case "87": return "Sharpness";
                case "88": return "Velocity Scan Modulation";
                case "8a": return "Color Saturation";
                case "8b": return "TV Channel Up / Down";
                case "8c": return "TV Sharpness";
                case "8d": return "Audio Mute / Screen Blank";
                case "8e": return "TV Contrast";
                case "8f": return "Audio Treble";
                case "90": return "Hue";
                case "91": return "Audio Bass";
                case "92": return "TV Black Level / Luminance";
                case "93": return "Audio Balance L / R";
                case "94": return "Audio Processor Mode";
                case "95": return "Window Position (TL_X)";
                case "96": return "Window Position (TL_Y)";
                case "97": return "Window Position (BR_X)";
                case "98": return "Window Position (BR_X)";
                case "9a": return "Window Background ";
                case "9b": return "6 Axis Color Control: Red";
                case "9c": return "6 Axis Color Control: Yellow";
                case "9d": return "6 Axis Color Control: Green";
                case "9e": return "6 Axis Color Control: Cyan";
                case "9f": return "6 Axis Color Control: Blue";
                case "a0": return "6 Axis Color Control: Magenta";
                case "a2": return "Auto Setup On / Off";
                case "a4": return "Window Mask Control";
                case "a5": return "Window Select";
                case "a6": return "Window Size";
                case "a7": return "Window Transparency";
                case "a8": return "Synchronization Type";
                case "aa": return "Screen Orientation";
                case "ac": return "Horizontal Frequency";
                case "ae": return "Vertical Frequency";
                case "b0": return "Settings";
                case "b2": return "Flat Panel Sub-Pixel Layout";
                case "b4": return "Source Timing Mode";
                case "b5": return "Source Color Coding";
                case "b6": return "Display Technology Type";
                case "b7": return "DPVL : Display status";
                case "b8": return "DPVL : Packet count";
                case "b9": return "DPVL : Display X origin";
                case "ba": return "DPVL : Display Y origin";
                case "bb": return "DPVL : Header CRC error count";
                case "bc": return "DPVL : Body CRC error count";
                case "bd": return "DPVL : Client ID";
                case "be": return "DPVL : Link control";
                case "c0": return "Display Usage Time";
                case "c2": return "Display Descriptor Length";
                case "c3": return "Transmit Display Descriptor";
                case "c4": return "Enable Display of‘Display Descriptor";
                case "c6": return "Application Enable Key";
                case "c7": return "Reserved";
                case "c8": return "Display Controller ID";
                case "c9": return "Display Firmware Level";
                case "ca": return "On Screen Display";
                case "cc": return "On Screen Display Language";
                case "cd": return "Status Indicators";
                case "ce": return "Auxiliary Display Size";
                case "cf": return "Auxiliary Display Data";
                case "d0": return "Output Selection";
                case "d2": return "Asset Tag";
                case "d4": return "Stereo Video Mode";
                case "d6": return "Power Mode";
                case "d7": return "Auxiliary Power Output";
                case "da": return "Scan Mode";
                case "db": return "Image Mode";
                case "dc": return "Display Application";
                case "de": return "Scratch Pad";
                case "df": return "VCP Version";
                case "e0": return "EnergySaver Modes: Dim or Mask";
                case "e1": return "EnergySaver Modes: Power Save";
                case "e2": return "Preset Modes Specific";
                case "e3": return "SpectraView Engine (SVE)";
                case "e5": return "PBP Mode Status";
                case "e8": return "PIP/PBP Input";
                case "e9": return "PIP/PBP Mode";
                case "ea": return "Dell Customize Specific";
                case "f0": return "HDR Modes Specific";
                case "f1": return "Specify Feature Support";
                default: return null;
            }
        }

        public static string FormatVCP_F8(string vcpControlName)
        {
            switch (vcpControlName)
            {
                case "F800": return "High Resolution";
                case "F801": return "High Data Speed";
                case "F810": return "FHD";
                case "F811": return "4K";
                default: return null;
            }
        }

        public static string FormatVCP_14(string vcpControlName)
        {
            switch (vcpControlName)
            {
                case "01": return "sRGB";
                case "02": return "Display Native";
                case "03": return "4000K";
                case "04": return "5000K";
                case "05": return "6500K";
                case "06": return "7500K";
                case "07": return "8200K";
                case "08": return "9300K/Cool";
                case "09": return "10000K";
                case "0a": return "11500K";
                case "0b": return "5700K/Warm";
                case "0c": return "Custom Color 1";
                case "0d": return "Custom Color 2";
                default: return null;
            }
        }

        public static string FormatVCP_AA(string vcpControlName)
        {
            switch (vcpControlName)
            {
                case "00": return "Reserved";
                case "01": return "0 degrees";
                case "02": return "90 degrees";
                case "03": return "180 degrees";
                case "04": return "270 degrees";
                default: return null;
            }
        }

        public static string FormatVCP_D6(string vcpControlName)
        {
            switch (vcpControlName)
            {
                case "01": return "Power Normal";
                case "04": return "Power Saving";
                case "05": return "Power Off";
                default: return null;
            }
        }

        public static string FormatVCP_8D(string vcpControlName)
        {
            switch (vcpControlName)
            {
                case "01": return "Mute the Mic";
                case "02": return "UnMute the Mic";
                default: return null;
            }
        }

        public static string FormatVCP_CC(string vcpControlName)
        {
            switch (vcpControlName)
            {
                case "01": return "Chinese";
                case "02": return "English";
                case "03": return "French";   // "Francais";
                case "04": return "German";   // "Deutschi";
                case "05": return "Italian";
                case "06": return "Japanese"; // "Japan";
                case "07": return "Korean";
                case "08": return "Portuguese";
                case "09": return "Russian";
                case "0a": return "Spanish";  // "Espanol";
                case "0b": return "Swedish";
                case "0c": return "Turkish";
                case "0d": return "Chinese-Simplified";
                case "0e": return "BrazilianPortuguese";
                case "0f": return "Arabic";
                case "10": return "Bulgarian";
                case "11": return "Croatian";
                case "12": return "Czech";
                case "13": return "Danish";
                case "14": return "Dutch";
                case "15": return "Estonian";
                case "16": return "Finnish";
                case "17": return "Greek";
                case "18": return "Hebrew";
                case "19": return "Hindi";
                case "1a": return "Hungarian";
                case "1b": return "Latvian";
                case "1c": return "Lithuanian";
                case "1d": return "Norwegian";
                case "1e": return "Polish";
                case "1f": return "Romanian";
                case "20": return "Serbian";
                case "21": return "Slovak";
                case "22": return "Slovenian";
                case "23": return "Thai";
                case "24": return "Ukrainian";
                case "25": return "Vietnamese";
                default: return null;
            }
        }

        public static string FormatVCP_60(string vcpControlName)
        {
            switch (vcpControlName)
            {
                case "01": return "VGA1";
                case "02": return "VGA2";
                case "03": return "DVI1";
                case "04": return "DVI2";
                case "05": return "Composite video1";
                case "06": return "Composite video2";
                case "07": return "S-Video1";
                case "08": return "S-Video2";
                case "09": return "Tuner1";
                case "0a": return "Tuner2";
                case "0b": return "Tuner3";
                case "0c": return "Component video (YPrPb/YCrCb)1";
                case "0d": return "Component video (YPrPb/YCrCb)2";
                case "0e": return "Component video (YPrPb/YCrCb)3";
                case "0f": return "DisplayPort1";
                case "10": return "Mini DisplayPort1";
                case "11": return "HDMI1";
                case "12": return "HDMI2";
                case "13": return "DisplayPort2";
                case "14": return "Mini DisplayPort2";
                case "15": return "HDMI3";
                case "16": return "HDMI4";
                case "17": return "DisplayPort3";
                case "18": return "Mini DisplayPort3";
                case "19": return "Thunderbolt1";
                case "1a": return "Thunderbolt2";
                case "1b": return "USB-C1";
                case "1c": return "USB-C2";
                case "1d": return "USB-C3";
                case "1e": return "USB-C4";
                case "80": return "USB Comm from USB1, Type-B, port 1";
                case "81": return "USB Comm from USB2, Type-B, port 2";
                case "82": return "USB Comm from USB-C1, Type-C, port 1";
                case "83": return "USB Comm from USB-C2, Type-C, port 2";
                case "84": return "USB Comm from USB-C3, Type-C, port 3";
                case "85": return "USB Comm from USB-C4, Type-C, port 4";
                default: return null;
            }
        }

        public static string FormatVCP_66(string vcpControlName)
        {
            switch (vcpControlName)
            {
                case "00f2": return "ALS full function";
                case "0f02": return "ALS full function";
                case "00d2": return "ALS without ALS_Primary";
                case "0d02": return "ALS without ALS_Primary";
                case "0012": return "ALS without sensor";
                case "0102": return "ALS without sensor";
                default: return null;
            }
        }

        public static string FormatVCP_DC(string vcpControlName)
        {
            switch (vcpControlName)
            {
                case "00": return "Standard/Native";
                //case "00": return "Standard";
                //case "00": return "Native";
                case "02": return "Multimedia";
                case "03": return "Movie";
                case "04": return "Nature";
                case "05": return "Game/Game1";
                //case "05": return "Game";
                case "06": return "Sport";
                default: return null;
            }
        }

        public static string FormatVCP_F0(string vcpControlName)
        {
            switch (vcpControlName)
            {
                case "01": return "Text";
                case "02": return "AdobeRGB";
                case "03": return "xvMode";
                case "04": return "DICOM";
                case "05": return "CAL1";
                case "21": return "AdobeRGB1/Adobe RGB D65 G2.2 L160/Adobe RGB D65 G2.2 L250";
                case "22": return "AdobeRGB2/Adobe RGB D50 G2.2 L160/Adobe RGB D50 G2.2 L250";
                case "06": return "CAL2";
                case "07": return "Metro";
                case "08": return "Paper";
                //case "09": return "Rec 709";
                //case "09": return "Rec.709";
                //case "09": return "Rec. 709 / BT.709";
                case "09": return "Rec.709 / BT.709"; // 1004 jim add
                case "0a": return "DCI-P3";
                case "0b": return "Rec2020";
                case "0c": return "ComfortView";
                case "0d": return "Game2";
                case "0e": return "Game3";
                case "0f": return "FPS Game";
                case "10": return "RTS Game";
                case "11": return "RPG Game";
                case "12": return "Multiscreen Match";
                case "13": return "SPORTS Game";
                case "30": return "Standard HDR";
                case "31": return "Movie HDR";
                case "32": return "Game HDR";
                case "33": return "Vivid HDR";
                case "34": return "Desktop";
                case "35": return "Reference";
                case "36": return "DisplayHDR";
                case "37": return "HDR10";
                case "38": return "HLG";
                case "a1": return "Display P3";
                case "c1": return "Custom 1 / User 1";
                case "c2": return "Custom 2 / User 2";
                case "c3": return "Custom 3 / User 3";
                //case "c1": return "Custom 1";
                //case "c2": return "Custom 2";
                //case "c3": return "Custom 3";
                default: return null;
            }
        }

        public static string FormatVCP_E2(string vcpControlName)
        {
            switch (vcpControlName)
            {
                case "00": return "Standard/Native";
                //case "00": return "Standard";
                //case "00": return "Native";
                case "01": return "Multimedia";
                case "02": return "Movie";
                case "03": return "Nature";
                //case "04": return "Game";
                case "04": return "Game/Game1";
                case "05": return "Sport";
                case "06": return "Text";
                case "07": return "AdobeRGB";
                case "08": return "xvMode";
                case "09": return "DICOM";
                case "0a": return "CAL1";
                case "0b": return "sRGB";
                case "0c": return "5000K";
                case "0d": return "5700K";
                case "0e": return "Warm";
                case "0f": return "6500K";
                case "10": return "7500K";
                case "11": return "9300K";
                case "12": return "Cool";
                case "13": return "10000K";
                case "14": return "Custom Color";
                case "15": return "CAL2";
                case "18": return "Metro";
                case "19": return "Paper";
                //case "1a": return "Rec. 709"; // 20240731 jim remove
                //case "1a": return "Rec.709"; // 20240731 jim add
                //case "1a": return "Rec. 709 / BT.709";
                case "1a": return "Rec.709 / BT.709"; // 10/04 add
                case "1b": return "DCI-P3";
                case "1c": return "Rec2020";
                case "1d": return "ComfortView";
                case "1e": return "Game2";
                case "1f": return "Game3";
                case "20": return "FPS Game";
                case "21": return "RTS Game";
                case "22": return "RPG Game";
                case "2f": return "SPORTS Game";
                case "25": return "Standard HDR";
                case "23": return "Movie HDR";
                case "24": return "Game HDR";
                case "26": return "Vivid HDR";
                case "27": return "Desktop";
                case "28": return "Reference";
                case "29": return "Multiscreen Match";
                case "2a": return "AdobeRGB1 (D65G2.2L250)";
                case "2b": return "AdobeRGB2 (D50G2.2L250)";
                //case "2a": return "AdobeRGB1";
                //case "2b": return "AdobeRGB2";
                case "2c": return "Custom 1 / User 1";
                case "2d": return "Custom 2 / User 2";
                case "2e": return "Custom 3 / User 3";
                //case "2c": return "Custom 1";
                //case "2d": return "Custom 2";
                //case "2e": return "Custom 3";
                case "3a": return "DisplayHDR";
                case "3b": return "HDR10";
                case "3c": return "HLG";
                case "7f": return "Presets Disabled";
                default: return null;
            }
        }

        public string FormatNode(INode node)
        {
            var parentKey = node.Parent?.ToString()?.ToLower();
            string result = null;

            //if (parentKey != null && lookupTables.ContainsKey(parentKey))
            if (!string.IsNullOrEmpty(parentKey) && lookupTables.ContainsKey(parentKey))
                result = lookupTables[parentKey](node.Value.ToLower());

            return result;
        }
    }
}