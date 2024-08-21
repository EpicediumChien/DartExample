using System.Collections.Generic;

namespace VcpCore.Common
{
    public static class VcpCodeList
    {
        public static Dictionary<string, byte> VCPctr = new Dictionary<string, byte>
        {
            {"VCP Code Page" , 0x00 },
            {"Degauss" , 0x01 },
            {"New Control Value" , 0x02 },
            {"Soft Controls" , 0x03 },
            {"Restore Factory Defaults" , 0x04 },
            {"Restore Factory Brightness/Contrast Defaults" , 0x05 },
            {"Restore Factory Geometry Defaults" , 0x06 },
            {"Restore Factory Color Defaults" , 0x08 },
            {"Restore Factory TV Defaults" , 0x0A },
            {"Color Temperature Increment" , 0x0B },
            {"Color Temperature Request" , 0x0C },
            {"Clock" , 0x0E },
            {"Brightness" , 0x10 },
            {"Flesh Tone Enhancement" , 0x11 },
            {"Contrast" , 0x12 },
            {"Backlight Control" , 0x13 },
            {"Basic Color Preset Select" , 0x14 },
            {"Video Gain (Drive) Red" , 0x16 },
            {"User Color Vision Compensation" , 0x17 },
            {"Video Gain (Drive) Green" , 0x18 },
            {"Video Gain (Drive) Blue" , 0x1A },
            {"Focus" , 0x1C },
            {"Auto Setup" , 0x1E },
            {"Auto Color Setup" , 0x1F },
            {"Horizontal Position (Phase)" , 0x20 },
            {"Horizontal Size" , 0x22 },
            {"Horizontal Pincushion" , 0x24 },
            {"Horizontal Pincushion Balance" , 0x26 },
            {"Horizontal Convergence R / B" , 0x28 },
            {"Horizontal Convergence M / G" , 0x29 },
            {"Horizontal Linearity" , 0x2A },
            {"Horizontal Linearity Balance" , 0x2C },
            {"Gray Scale Expansion" , 0x2E },
            {"Vertical Position (Phase)" , 0x30 },
            {"Vertical Size" , 0x32 },
            {"Vertical Pincushion" , 0x34 },
            {"Vertical Pincushion Balance" , 0x36 },
            {"Vertical Convergence R/B" , 0x38 },
            {"Vertical Convergence M/G" , 0x39 },
            {"Vertical Linearity" , 0x3A },
            {"Vertical Linearity Balance" , 0x3C },
            {"Clock Phase" , 0x3E },
            {"Horizontal Parallelogram" , 0x40 },
            {"Vertical Parallelogram" , 0x41 },
            {"Horizontal Keystone" , 0x42 },
            {"Vertical Keystone" , 0x43 },
            {"Rotation" , 0x44 },
            {"Top Corner Flare" , 0x46 },
            {"Top Corner Hook" , 0x48 },
            {"Bottom Corner Flare" , 0x4A },
            {"Bottom Corner Hook" , 0x4C },
            {"Hue_50" , 0x50 },
            {"Active Control" , 0x52 },
            {"Performance Preservation" , 0x54 },
            {"Horizontal Moire" , 0x56 },
            {"Vertical Moire" , 0x58 },
            {"6 Axis Saturation Control Red" , 0x59 },
            {"6 Axis Saturation Control Yellow" , 0x5A },
            {"6 Axis Saturation Control Green" , 0x5B },
            {"6 Axis Saturation Control Cyan" , 0x5C },
            {"6 Axis Saturation Control Blue" , 0x5D },
            {"6 Axis Saturation Control Magenta" , 0x5E },
            {"Input Select" , 0x60 },
            {"Audio Speaker Volume" , 0x62 },
            {"Audio Speaker Pair Select" , 0x63 },
            {"Audio Microphone Volume" , 0x64 },
            {"Audio Jack Connection Status" , 0x65 },
            {"Ambient Light Sensor" , 0x66 },
            {"Language Select" , 0x68 },
            {"Backlight Level White" , 0x6B },
            {"Video Black Level Red" , 0x6C },
            {"Backlight Level Red" , 0x6D },
            {"Video Black Level Green" , 0x6E },
            {"Backlight Level Green" , 0x6F },
            {"Video Black Level Blue" , 0x70 },
            {"Backlight Level Blue" , 0x71 },
            {"Gamma" , 0x72 },
            {"LUT Size" , 0x73 },
            {"Single Point LUT Operation" , 0x74 },
            {"Block LUT Operation" , 0x75 },
            {"Remote Procedure Call" , 0x76 },
            {"Display Identification Data Operation" , 0x78 },
            {"Adjust Zoom" , 0x7C },
            {"Horizontal Mirror (Flip)" , 0x82 },
            {"Vertical Mirror (Flip)" , 0x84 },
            {"Display Scaling" , 0x86 },
            {"Sharpness" , 0x87 },
            {"Velocity Scan Modulation" , 0x88 },
            {"Color Saturation" , 0x8A },
            {"TV Channel Up / Down" , 0x8B },
            {"TV Sharpness" , 0x8C },
            {"Audio Mute / Screen Blank" , 0x8D },
            {"TV Contrast" , 0x8E },
            {"Audio Treble" , 0x8F },
            {"Hue_90" , 0x90 },
            {"Audio Bass" , 0x91 },
            {"TV Black Level / Luminance" , 0x92 },
            {"Audio Balance L / R" , 0x93 },
            {"Audio Processor Mode" , 0x94 },
            {"Window Position (TL_X)_95" , 0x95 },
            {"Window Position (TL_Y)_96" , 0x96 },
            {"Window Position (BR_X)_97" , 0x97 },
            {"Window Position (BR_X)_98" , 0x98 },
            {"Window Background{" , 0x9A },
            {"6 Axis Color Control Red" , 0x9B },
            {"6 Axis Color Control Yellow" , 0x9C },
            {"6 Axis Color Control Green" , 0x9D },
            {"6 Axis Color Control Cyan" , 0x9E },
            {"6 Axis Color Control Blue" , 0x9F },
            {"6 Axis Color Control Magenta" , 0xA0 },
            {"Auto Setup On / Off" , 0xA2 },
            {"Window Mask Control" , 0xA4 },
            {"Window Select" , 0xA5 },
            {"Window Size" , 0xA6 },
            {"Window Transparency" , 0xA7 },
            {"Synchronization Type" , 0xA8 },
            {"Screen Orientation" , 0xAA },
            {"Horizontal Frequency" , 0xAC },
            {"Vertical Frequency" , 0xAE },
            {"Settings" , 0xB0 },
            {"Flat Panel Sub-Pixel Layout" , 0xB2 },
            {"Source Timing Mode" , 0xB4 },
            {"Source Color Coding" , 0xB5 },
            {"Display Technology Type" , 0xB6 },
            {"DPVL  Display status" , 0xB7 },
            {"DPVL  Packet count" , 0xB8 },
            {"DPVL  Display X origin" , 0xB9 },
            {"DPVL  Display Y origin" , 0xBA },
            {"DPVL  Header CRC error count" , 0xBB },
            {"DPVL  Body CRC error count" , 0xBC },
            {"DPVL  Client ID" , 0xBD },
            {"DPVL  Link control" , 0xBE },
            {"Display Usage Time" , 0xC0 },
            {"Display Descriptor Length" , 0xC2 },
            {"Transmit Display Descriptor" , 0xC3 },
            {"Enable Display of¡¥Display Descriptor" , 0xC4 },
            {"Application Enable Key" , 0xC6 },
            {"Reserved" , 0xC7 },
            {"Display Controller ID" , 0xC8 },
            {"Display Firmware Level" , 0xC9 },
            {"On Screen Display" , 0xCA },
            {"On Screen Display Language" , 0xCC },
            {"Status Indicators" , 0xCD },
            {"Auxiliary Display Size" , 0xCE },
            {"Auxiliary Display Data" , 0xCF },
            {"Output Selection" , 0xD0 },
            {"Asset Tag" , 0xD2 },
            {"Stereo Video Mode" , 0xD4 },
            {"Power Mode" , 0xD6 },
            {"Auxiliary Power Output" , 0xD7 },
            {"Scan Mode" , 0xDA },
            {"Image Mode" , 0xDB },
            {"Display Application" , 0xDC },
            {"Scratch Pad" , 0xDE },
            {"VCP Version" , 0xDF },
            {"Preset Modes Specific" , 0xE2 },
            {"SpectraView Engine (SVE)" , 0xE3 },
            {"PBP Mode Status" , 0xE5 },
            {"PIP/PBP Input" , 0xE8 },
            {"PIP/PBP Mode" , 0xE9 },
            {"Dell Customize Specific" , 0xEA },
            {"USB-C Prioritization" , 0xEA },
            {"HDR Modes Specific" , 0xF0 },
            {"Specify Feature Support" , 0xF1 },
            {"Gaming" , 0xF4 },
        };

        public static Dictionary<string, uint> VCP60 = new Dictionary<string, uint>
        {
            {"VGA-1" , 0x01 },
            {"VGA-2" , 0x02 },
            {"DVI-1" , 0x03 },
            {"DVI-2" , 0x04 },
            {"Composite video 1" , 0x05 },
            {"Composite video 2" , 0x06 },
            {"S-Video-1" , 0x07 },
            {"S-Video-2" , 0x08 },
            {"Tuner-1" , 0x09 },
            {"Tuner-2" , 0x0A },
            {"Tuner-3" , 0x0B },
            {"Component video (YPrPb/YCrCb) 1" , 0x0C },
            {"Component video (YPrPb/YCrCb) 2" , 0x0D },
            {"Component video (YPrPb/YCrCb) 3" , 0x0E },
            {"DisplayPort-1" , 0x0F },
            {"Mini DisplayPort-1" , 0x10 },
            {"HDMI-1" , 0x11 },
            {"HDMI-2" , 0x12 },
            {"DisplayPort-2" , 0x13 },
            {"Mini DisplayPort-2" , 0x14 },
            {"HDMI3" , 0x15 },
            {"HDMI4" , 0x16 },
            {"DisplayPort-3" , 0x17 },
            {"Mini DisplayPort-3" , 0x18 },
            {"Thunderbolt-1" , 0x19 },
            {"Thunderbolt-2" , 0x1A },
            {"USB-C1" , 0x1B },
            {"USB-C2" , 0x1C },
            {"USB-C3" , 0x1D },
            {"USB-C4" , 0x1E },
            {"USB Comm from USB1 (Type-B, port 1)" , 0x80 },
            {"USB Comm from USB2 (Type-B, port 2)" , 0x81 },
            {"USB Comm from USB-C1 (Type-C, port 1)" , 0x82 },
            {"USB Comm from USB-C2 (Type-C, port 2)" , 0x83 },
            {"USB Comm from USB-C3 (Type-C, port 3)" , 0x84 },
            {"USB Comm from USB-C4 (Type-C, port 4)" , 0x85 }
        };

        public static Dictionary<string, uint> VCPF8 = new Dictionary<string, uint> // USB-C Prioritization
        {
            { "High Resolution", 0xF800 },
            { "High Data Speed", 0xF801 },
            { "4K", 0xF811 },
            { "FHD", 0xF810 }
        };

        public static Dictionary<string, byte> VCPDC = new Dictionary<string, byte>
        {
            { "Standard/Native", 0 }, // 20240731 jim add  "Game/Game1"
            { "Standard", 0 },
            { "Native", 0 },
            { "Multimedia", 2 },
            { "Movie", 3 },
            { "Nature", 4 },
            { "Game/Game1", 5 },
            { "Game", 5 },
            { "Game1", 5 },
            { "Sport", 6 }
        };

        public static Dictionary<string, byte> VCPF0 = new Dictionary<string, byte>
        {
            { "Text", 1 },
            { "AdobeRGB", 2 },
            { "AdobeRGB1", 33 },
            { "AdobeRGB2", 34 },
            { "Adobe RGB D65 G2.2 L160", 33 },
            { "Adobe RGB D50 G2.2 L160", 34 },
            { "Adobe RGB D65 G2.2 L250", 33 },
            { "Adobe RGB D50 G2.2 L250", 34 },
            { "xvMode", 3 },
            { "DICOM", 4 },
            { "CAL1", 5 },
            { "Custom 1 / User 1", 193 },   // 20240731 jim add
            { "Custom 2 / User 2", 194 },  // 20240731 jim add
            { "Custom 3 / User 3", 195 }, // 20240731 jim add
            { "Custom 1", 193 },
            { "Custom 2", 194 },
            { "Custom 3", 195 },
            { "User 1", 193 },
            { "User 2", 194 },
            { "User 3", 195 },
            { "CAL2", 6 },
            { "Metro", 7 },
            { "Paper", 8 },
            { "Rec. 709 / BT.709", 9 },
            { "Rec. 709/BT.709", 9 },  // 20240731 jim remove
            { "Rec.709/BT.709", 9 }, // 20240808 jim remove
            { "Rec. 709", 9 }, // 20240808 jim add back
            { "Rec.709", 9 }, // 20240731 jim add
            { "Rec709", 9 },
            { "Rec 709", 9 },
            { "BT.709", 9 },
            { "BT.709 D65 BT1886 L100", 9 },
            { "DCI-P3", 10 },
            { "DCI P3 D65 G2.4 L100", 10 },
            { "Rec2020", 11 },
            { "BT.2020", 11 },
            { "BT.2020 D65 BT1886 L100", 11 },
            { "ComfortView", 12 },
            { "Game2", 13 },
            { "Game3", 14 },
            { "FPS Game", 15 },
            { "RTS Game", 16 },
            { "RPG Game", 17 },
            { "SPORTS Game", 19 },
            { "Standard HDR", 48 },
            { "Movie HDR", 49 },
            { "Game HDR", 50 },
            { "Vivid HDR", 51 },
            { "Desktop", 52 },
            { "Reference", 53 },
            { "Multiscreen Match", 18 },
            { "DisplayHDR", 54 },
            { "HDR10", 55 },
            { "HLG", 56 },
            { "Display P3", 161 }
        };

        public static Dictionary<string, byte> VCP14 = new Dictionary<string, byte>
        {
            { "sRGB", 1 },
            { "sRGB D65 sRGB L120", 1 },
            { "sRGB D65 sRGB L250", 1 },
            { "5000K", 4 },
            { "5700K", 11 },
            { "Warm", 11 },
            { "6500K", 5 },
            { "7500K", 6 },
            { "9300K", 8 },
            { "Cool", 8 },
            { "10000K", 9 },
            { "Custom Color", 12 }
        };

        public static Dictionary<int, string> VCPF4 = new Dictionary<int, string>
        {
            { 0x40, "Off" },
            { 0x41, "Desktop" },
            { 0x42, "Movie HDR" },
            { 0x43, "Game HDR" },
            { 0x44, "DisplayHDR" },
            { 0x45, "Custom Color HDR" },
            { 0x46, "HDR Peak 1000" }
        };

        public static Dictionary<int, string> VCPE2 = new Dictionary<int, string>
        {
            { 0, "Standard/Native" },  // 20240731 jim add
            //{ 0, "Standard" },
            //{ 0, "Native" },  // 20240731 jim add
            { 1, "Multimedia" },
            { 2, "Movie" },
            { 3, "Nature" },
            { 4, "Game/Game1" },  // 20240731 jim add
            //{ 4, "Game" },
            { 5, "Sport" },
            { 6, "Text" },
            { 7, "AdobeRGB" },
            { 8, "xvMode" },
            { 9, "DICOM" },
            { 10, "CAL1" },
            { 11, "sRGB" },
            { 12, "5000K" },
            { 13, "5700K" },
            { 14, "Warm" },
            { 15, "6500K" },
            { 16, "7500K" },
            { 17, "9300K" },
            { 18, "Cool" },
            { 19, "10000K" },
            { 20, "Custom Color" },
            { 21, "CAL2" },
            { 24, "Metro" },
            { 25, "Paper" },
            { 26, "Rec. 709 / BT.709" }, // 20240731 jim remove
            //{ 26, "Rec. 709/BT.709" }, // 20240731 jim remove
            //{ 26, "Rec. 709" }, // 20240731 jim remove
            //{ 26, "Rec.709" }, // 20240731 jim add
            { 27, "DCI-P3" },
            { 28, "Rec2020" },
            { 29, "ComfortView" },
            { 30, "Game2" },
            { 31, "Game3" },
            { 32, "FPS Game" },
            { 33, "RTS Game" },
            { 34, "RPG Game" },
            { 37, "Standard HDR" },
            { 35, "Movie HDR" },
            { 36, "Game HDR" },
            { 38, "Vivid HDR" },
            { 39, "Desktop" },
            { 40, "Reference" },
            { 41, "Multiscreen Match" },
            { 58, "DisplayHDR" },
            { 59, "HDR10" },
            { 60, "HLG" },
            { 127, "Presets Disabled" },
            { 61, "Display P3" },
            { 42, "AdobeRGB1" },
            { 43, "AdobeRGB2" },
            { 44, "Custom 1 / User 1" },
            { 45, "Custom 2 / User 2" },
            { 46, "Custom 3 / User 3" },
            //{ 44, "Custom 1" },
            //{ 45, "Custom 2" },
            //{ 46, "Custom 3" },
            { 47, "SPORTS Game" },
        };

        public static Dictionary<string, int> VCPE2_HardCode = new Dictionary<string, int>
        {
            { "Native", 0 },
            { "User 1", 44 },
            { "User 2", 45 },
            { "User 3", 46 },
            { "Game1", 4 },
            { "Adobe RGB D65 G2.2 L160", 42 },
            { "Adobe RGB D50 G2.2 L160", 43 },
            { "BT.709 D65 BT1886 L100", 26 },
            { "BT.2020 D65 BT1886 L100", 28 },
            { "sRGB D65 sRGB L120", 11 },
            { "Adobe RGB D65 G2.2 L250", 42 },
            { "Adobe RGB D50 G2.2 L250", 43 },
            { "sRGB D65 sRGB L250", 11 },
            { "DCI P3 D65 G2.4 L100", 27 },
            { "Rec. 709 / BT.709", 26 }, // 20240731 jim remove
            { "Rec. 709/BT.709", 26 }, // 20240731 jim remove
            { "Rec.709/BT.709", 26 }, // 20240731 jim add
            { "BT.709", 26 },
            { "Rec709", 26 }
        };

        public struct VcpValue
        {
            public byte Vcp;
            public byte Value;

            public override string ToString()
            {
                return $"VCP: 0x{Vcp.ToString("X")}, value:{Value}";
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

    public struct ColorPreset
    {
        public VcpCode vcpode;
        public uint codeValue;
    }

    public enum VcpCode : byte
    {
        None = 0,
        RestoreFactoryDefaults = 4,
        RestoreFactoryLuminanceContrastDefaults = 5,
        RestoreFactoryColorDefaults = 8,
        Brightness = 16,
        Contrast = 18,
        BacklightControl = 19,
        ColorSpaceE2 = 226,
        ColorSpace14 = 20,
        ColorSpaceDC = 220,
        ColorSpaceF0 = 240,
        UserColorTemperatureIncrement = 11,
        UserColorTemperature = 12,
        Clock = 14,
        FleshToneEnhancement = 17,
        ImageMode = 219,
        VideoGainRed = 22,
        UserColorVisionCompensation = 23,
        VideoGainGreen = 24,
        VideoGainBlue = 26,
        Focus = 28,
        AutoSetup = 30,
        AutoColorSetup = 31,
        GrayScaleExpansion = 46,
        ClockPhase = 62,
        HorizontalMoire = 86,
        VerticalMoire = 88,
        SixAxisSaturationControlRed = 89,
        SixAxisSaturationControlYellow = 90,
        SixAxisSaturationControlGreen = 91,
        SixAxisSaturationControlCyan = 92,
        SixAxisSaturationControlBlue = 93,
        SixAxisSaturationControlMagenta = 94,
        BacklightLevelWhite = 107,
        BacklightLevelRed = 109,
        BacklightLevelGreen = 111,
        BacklightLevelBlue = 113,
        VideoBlackLevelRed = 108,
        VideoBlackLevelGreen = 110,
        VideoBlackLevelBlue = 112,
        Gamma = 114,
        BlockLUTOperation = 117,
        AdjustZoom = 124,
        Sharpness = 135,
        VelocityScanModulation = 136,
        ColorSaturation = 138,
        TVSharpness = 140,
        TVContrast = 142,
        Hue = 144,
        TVBlackLevelLuminance = 146,
        WindowBackground = 154,
        SixAxisHueControlRed = 155,
        SixAxisHueControlYellow = 156,
        SixAxisHueControlGreen = 157,
        SixAxisHueControlCyan = 158,
        SixAxisHueControlBlue = 159,
        SixAxisHueControlMagenta = 160,
        AutoSetupOnOff = 162,
        WindowMaskControl = 164,
        WindowSelect = 165,
        WindowSize = 166,
        WindowTransparency = 167,
        StereoVideoMode = 212,
        MiscBitSupport1 = 241,
        MiscBitSupport2 = 242,
        LUTSize = 115,
        SinglePointLUTOperation = 116,
        Rotate = 170,
        HorizontalFrequency = 172,
        VerticalFrequency = 174,
        WindowPos = 232,
        DisplayUsageTime = 192,
        PowerMode = 214,
        OSDButtonControl = 202,
        ScratchPad = 222,
        VCPVersion = 223,
        TechnologyType = 182,
        SourceTimingMode = 180,
        ReduceBrightness = 224,
        SuspendMonitor = 225,
        USBUpstreamAssociation = 231,
        VisionEngineSwitch = 236,
        VisionEngineAdjustment = 237,
        USBUplinkPorts = 238,
        InputSelect = 96,
        OsdQuery = 2,
        ReadStack = 82,
        Version = 201,
        OSDLanguage = 204,
        Settings = 176,
        SourceColorCoding = 181,
        UniformityCalibrated = 228,
        PIPPBPInput = 232,
        PIPPBPMode = 233,
        PIPPBPVideoSwap = 229,
        MonitorPrivacyControls = 234,
        ISPNewFirmwareVersion = 254,
        FirmwareVersion = 253,
        DisplayControllerID = 200,
        SpeakerVolume = 98,
        Zoom = 229,
        AccessoryModuleCommand = 235,
        GamingProcessingControls = 236,
        GamingProfileControls = 237,
        MultiMonitorSync = 239,
        GamingFeatures = 244
    }
}