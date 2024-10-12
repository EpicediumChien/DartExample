using Moq;

namespace VcpCore.Plugins.Test
{
    public class TestNodeFormatter
    {
        private NodeFormatter formatter = new NodeFormatter();

        [Test]
        public void TestFormatVCPControlName()
        {
            string vcpControlName00 = "VCP Code Page";
            string vcpControlName01 = "Degauss";
            string vcpControlName02 = "New Control value";
            string vcpControlName03 = "Soft Controls";
            string vcpControlName04 = "Restore Factory Defaults";
            string vcpControlName05 = "Restore Factory Brightness/Contrast Defaults";
            string vcpControlName06 = "Restore Factory Geometry Defaults";
            string vcpControlName08 = "Restore Factory Color Defaults";
            string vcpControlName0a = "Restore Factory TV Defaults";
            string vcpControlName0b = "Color Temperature Increment";
            string vcpControlName0c = "Color Temperature Request";
            string vcpControlName0e = "Clock";
            string vcpControlName10 = "Brightness";
            string vcpControlName11 = "Flesh Tone Enhancement";
            string vcpControlName12 = "Contrast";
            string vcpControlName13 = "Backlight Control";
            string vcpControlName14 = "Basic Color Preset Select";
            string vcpControlName16 = "Video Gain (Drive): Red";
            string vcpControlName17 = "User Color Vision Compensation";
            string vcpControlName18 = "Video Gain (Drive): Green";
            string vcpControlName1a = "Video Gain (Drive): Blue";
            string vcpControlName1c = "Focus";
            string vcpControlName1e = "Auto Setup";
            string vcpControlName1f = "Auto Color Setup";
            string vcpControlName20 = "Horizontal Position (Phase)";
            string vcpControlName22 = "Horizontal Size";
            string vcpControlName24 = "Horizontal Pincushion";
            string vcpControlName26 = "Horizontal Pincushion Balance";
            string vcpControlName28 = "Horizontal Convergence R / B";
            string vcpControlName29 = "Horizontal Convergence M / G";
            string vcpControlName2a = "Horizontal Linearity";
            string vcpControlName2c = "Horizontal Linearity Balance";
            string vcpControlName2e = "Gray Scale Expansion";
            string vcpControlName30 = "Vertical Position (Phase)";
            string vcpControlName32 = "Vertical Size";
            string vcpControlName34 = "Vertical Pincushion";
            string vcpControlName36 = "Vertical Pincushion Balance";
            string vcpControlName38 = "Vertical Convergence R/B";
            string vcpControlName39 = "Vertical Convergence M/G";
            string vcpControlName3a = "Vertical Linearity";
            string vcpControlName3c = "Vertical Linearity Balance";
            string vcpControlName3e = "Clock Phase";
            string vcpControlName40 = "Horizontal Parallelogram";
            string vcpControlName41 = "Vertical Parallelogram";
            string vcpControlName42 = "Horizontal Keystone";
            string vcpControlName43 = "Vertical Keystone";
            string vcpControlName44 = "Rotation";
            string vcpControlName46 = "Top Corner Flare";
            string vcpControlName48 = "Top Corner Hook";
            string vcpControlName4a = "Bottom Corner Flare";
            string vcpControlName4c = "Bottom Corner Hook";
            string vcpControlName50 = "Hue";
            string vcpControlName52 = "Active Control";
            string vcpControlName54 = "Performance Preservation";
            string vcpControlName56 = "Horizontal Moire";
            string vcpControlName58 = "Vertical Moire";
            string vcpControlName59 = "6 Axis Saturation Control: Red";
            string vcpControlName5a = "6 Axis Saturation Control: Yellow";
            string vcpControlName5b = "6 Axis Saturation Control: Green";
            string vcpControlName5c = "6 Axis Saturation Control: Cyan";
            string vcpControlName5d = "6 Axis Saturation Control: Blue";
            string vcpControlName5e = "6 Axis Saturation Control: Magenta";
            string vcpControlName60 = "Input Select";
            string vcpControlName62 = "Audio Speaker Volume";
            string vcpControlName63 = "Audio: Speaker Pair Select";
            string vcpControlName64 = "Audio Microphone Volume";
            string vcpControlName65 = "Audio: Jack Connection Status";
            string vcpControlName66 = "Ambient Light Sensor";
            string vcpControlName68 = "Language Select";
            string vcpControlName6b = "Backlight Level: White";
            string vcpControlName6c = "Video Black Level: Red";
            string vcpControlName6d = "Backlight Level: Red";
            string vcpControlName6e = "Video Black Level: Green";
            string vcpControlName6f = "Backlight Level: Green";
            string vcpControlName70 = "Video Black Level: Blue";
            string vcpControlName71 = "Backlight Level: Blue";
            string vcpControlName72 = "Gamma";
            string vcpControlName73 = "LUT Size";
            string vcpControlName74 = "Single Point LUT Operation";
            string vcpControlName75 = "Block LUT Operation";
            string vcpControlName76 = "Remote Procedure Call";
            string vcpControlName78 = "Display Identification Data Operation";
            string vcpControlName7c = "Adjust Zoom";
            string vcpControlName82 = "Horizontal Mirror (Flip)";
            string vcpControlName84 = "Vertical Mirror (Flip)";
            string vcpControlName86 = "Display Scaling";
            string vcpControlName87 = "Sharpness";
            string vcpControlName88 = "Velocity Scan Modulation";
            string vcpControlName8a = "Color Saturation";
            string vcpControlName8b = "TV Channel Up / Down";
            string vcpControlName8c = "TV Sharpness";
            string vcpControlName8d = "Audio Mute / Screen Blank";
            string vcpControlName8e = "TV Contrast";
            string vcpControlName8f = "Audio Treble";
            string vcpControlName90 = "Hue";
            string vcpControlName91 = "Audio Bass";
            string vcpControlName92 = "TV Black Level / Luminance";
            string vcpControlName93 = "Audio Balance L / R";
            string vcpControlName94 = "Audio Processor Mode";
            string vcpControlName95 = "Window Position (TL_X)";
            string vcpControlName96 = "Window Position (TL_Y)";
            string vcpControlName97 = "Window Position (BR_X)";
            string vcpControlName98 = "Window Position (BR_X)";
            string vcpControlName9a = "Window Background ";
            string vcpControlName9b = "6 Axis Color Control: Red";
            string vcpControlName9c = "6 Axis Color Control: Yellow";
            string vcpControlName9d = "6 Axis Color Control: Green";
            string vcpControlName9e = "6 Axis Color Control: Cyan";
            string vcpControlName9f = "6 Axis Color Control: Blue";
            string vcpControlNamea0 = "6 Axis Color Control: Magenta";
            string vcpControlNamea2 = "Auto Setup On / Off";
            string vcpControlNamea4 = "Window Mask Control";
            string vcpControlNamea5 = "Window Select";
            string vcpControlNamea6 = "Window Size";
            string vcpControlNamea7 = "Window Transparency";
            string vcpControlNamea8 = "Synchronization Type";
            string vcpControlNameaa = "Screen Orientation";
            string vcpControlNameac = "Horizontal Frequency";
            string vcpControlNameae = "Vertical Frequency";
            string vcpControlNameb0 = "Settings";
            string vcpControlNameb2 = "Flat Panel Sub-Pixel Layout";
            string vcpControlNameb4 = "Source Timing Mode";
            string vcpControlNameb5 = "Source Color Coding";
            string vcpControlNameb6 = "Display Technology Type";
            string vcpControlNameb7 = "DPVL : Display status";
            string vcpControlNameb8 = "DPVL : Packet count";
            string vcpControlNameb9 = "DPVL : Display X origin";
            string vcpControlNameba = "DPVL : Display Y origin";
            string vcpControlNamebb = "DPVL : Header CRC error count";
            string vcpControlNamebc = "DPVL : Body CRC error count";
            string vcpControlNamebd = "DPVL : Client ID";
            string vcpControlNamebe = "DPVL : Link control";
            string vcpControlNamec0 = "Display Usage Time";
            string vcpControlNamec2 = "Display Descriptor Length";
            string vcpControlNamec3 = "Transmit Display Descriptor";
            string vcpControlNamec4 = "Enable Display of‘Display Descriptor";
            string vcpControlNamec6 = "Application Enable Key";
            string vcpControlNamec7 = "Reserved";
            string vcpControlNamec8 = "Display Controller ID";
            string vcpControlNamec9 = "Display Firmware Level";
            string vcpControlNameca = "On Screen Display";
            string vcpControlNamecc = "On Screen Display Language";
            string vcpControlNamecd = "Status Indicators";
            string vcpControlNamece = "Auxiliary Display Size";
            string vcpControlNamecf = "Auxiliary Display Data";
            string vcpControlNamed0 = "Output Selection";
            string vcpControlNamed2 = "Asset Tag";
            string vcpControlNamed4 = "Stereo Video Mode";
            string vcpControlNamed6 = "Power Mode";
            string vcpControlNamed7 = "Auxiliary Power Output";
            string vcpControlNameda = "Scan Mode";
            string vcpControlNamedb = "Image Mode";
            string vcpControlNamedc = "Display Application";
            string vcpControlNamede = "Scratch Pad";
            string vcpControlNamedf = "VCP Version";
            string vcpControlNamee0 = "EnergySaver Modes: Dim or Mask";
            string vcpControlNamee1 = "EnergySaver Modes: Power Save";
            string vcpControlNamee2 = "Preset Modes Specific";
            string vcpControlNamee3 = "SpectraView Engine (SVE)";
            string vcpControlNamee5 = "PBP Mode Status";
            string vcpControlNamee8 = "PIP/PBP Input";
            string vcpControlNamee9 = "PIP/PBP Mode";
            string vcpControlNameea = "Dell Customize Specific";
            string vcpControlNamef0 = "HDR Modes Specific";
            string vcpControlNamef1 = "Specify Feature Support";
            string? vcpControlNamef2 = null;

            Assert.That(vcpControlName00, Is.EqualTo(NodeFormatter.FormatVCPControlName("00")));
            Assert.That(vcpControlName01, Is.EqualTo(NodeFormatter.FormatVCPControlName("01")));
            Assert.That(vcpControlName02, Is.EqualTo(NodeFormatter.FormatVCPControlName("02")));
            Assert.That(vcpControlName03, Is.EqualTo(NodeFormatter.FormatVCPControlName("03")));
            Assert.That(vcpControlName04, Is.EqualTo(NodeFormatter.FormatVCPControlName("04")));
            Assert.That(vcpControlName05, Is.EqualTo(NodeFormatter.FormatVCPControlName("05")));
            Assert.That(vcpControlName06, Is.EqualTo(NodeFormatter.FormatVCPControlName("06")));
            Assert.That(vcpControlName08, Is.EqualTo(NodeFormatter.FormatVCPControlName("08")));
            Assert.That(vcpControlName0a, Is.EqualTo(NodeFormatter.FormatVCPControlName("0a")));
            Assert.That(vcpControlName0b, Is.EqualTo(NodeFormatter.FormatVCPControlName("0b")));
            Assert.That(vcpControlName0c, Is.EqualTo(NodeFormatter.FormatVCPControlName("0c")));
            Assert.That(vcpControlName0e, Is.EqualTo(NodeFormatter.FormatVCPControlName("0e")));
            Assert.That(vcpControlName10, Is.EqualTo(NodeFormatter.FormatVCPControlName("10")));
            Assert.That(vcpControlName11, Is.EqualTo(NodeFormatter.FormatVCPControlName("11")));
            Assert.That(vcpControlName12, Is.EqualTo(NodeFormatter.FormatVCPControlName("12")));
            Assert.That(vcpControlName13, Is.EqualTo(NodeFormatter.FormatVCPControlName("13")));
            Assert.That(vcpControlName14, Is.EqualTo(NodeFormatter.FormatVCPControlName("14")));
            Assert.That(vcpControlName16, Is.EqualTo(NodeFormatter.FormatVCPControlName("16")));
            Assert.That(vcpControlName17, Is.EqualTo(NodeFormatter.FormatVCPControlName("17")));
            Assert.That(vcpControlName18, Is.EqualTo(NodeFormatter.FormatVCPControlName("18")));
            Assert.That(vcpControlName1a, Is.EqualTo(NodeFormatter.FormatVCPControlName("1a")));
            Assert.That(vcpControlName1c, Is.EqualTo(NodeFormatter.FormatVCPControlName("1c")));
            Assert.That(vcpControlName1e, Is.EqualTo(NodeFormatter.FormatVCPControlName("1e")));
            Assert.That(vcpControlName1f, Is.EqualTo(NodeFormatter.FormatVCPControlName("1f")));
            Assert.That(vcpControlName20, Is.EqualTo(NodeFormatter.FormatVCPControlName("20")));
            Assert.That(vcpControlName22, Is.EqualTo(NodeFormatter.FormatVCPControlName("22")));
            Assert.That(vcpControlName24, Is.EqualTo(NodeFormatter.FormatVCPControlName("24")));
            Assert.That(vcpControlName26, Is.EqualTo(NodeFormatter.FormatVCPControlName("26")));
            Assert.That(vcpControlName28, Is.EqualTo(NodeFormatter.FormatVCPControlName("28")));
            Assert.That(vcpControlName29, Is.EqualTo(NodeFormatter.FormatVCPControlName("29")));
            Assert.That(vcpControlName2a, Is.EqualTo(NodeFormatter.FormatVCPControlName("2a")));
            Assert.That(vcpControlName2c, Is.EqualTo(NodeFormatter.FormatVCPControlName("2c")));
            Assert.That(vcpControlName2e, Is.EqualTo(NodeFormatter.FormatVCPControlName("2e")));
            Assert.That(vcpControlName30, Is.EqualTo(NodeFormatter.FormatVCPControlName("30")));
            Assert.That(vcpControlName32, Is.EqualTo(NodeFormatter.FormatVCPControlName("32")));
            Assert.That(vcpControlName34, Is.EqualTo(NodeFormatter.FormatVCPControlName("34")));
            Assert.That(vcpControlName36, Is.EqualTo(NodeFormatter.FormatVCPControlName("36")));
            Assert.That(vcpControlName38, Is.EqualTo(NodeFormatter.FormatVCPControlName("38")));
            Assert.That(vcpControlName39, Is.EqualTo(NodeFormatter.FormatVCPControlName("39")));
            Assert.That(vcpControlName3a, Is.EqualTo(NodeFormatter.FormatVCPControlName("3a")));
            Assert.That(vcpControlName3c, Is.EqualTo(NodeFormatter.FormatVCPControlName("3c")));
            Assert.That(vcpControlName3e, Is.EqualTo(NodeFormatter.FormatVCPControlName("3e")));
            Assert.That(vcpControlName40, Is.EqualTo(NodeFormatter.FormatVCPControlName("40")));
            Assert.That(vcpControlName41, Is.EqualTo(NodeFormatter.FormatVCPControlName("41")));
            Assert.That(vcpControlName42, Is.EqualTo(NodeFormatter.FormatVCPControlName("42")));
            Assert.That(vcpControlName43, Is.EqualTo(NodeFormatter.FormatVCPControlName("43")));
            Assert.That(vcpControlName44, Is.EqualTo(NodeFormatter.FormatVCPControlName("44")));
            Assert.That(vcpControlName46, Is.EqualTo(NodeFormatter.FormatVCPControlName("46")));
            Assert.That(vcpControlName48, Is.EqualTo(NodeFormatter.FormatVCPControlName("48")));
            Assert.That(vcpControlName4a, Is.EqualTo(NodeFormatter.FormatVCPControlName("4a")));
            Assert.That(vcpControlName4c, Is.EqualTo(NodeFormatter.FormatVCPControlName("4c")));
            Assert.That(vcpControlName50, Is.EqualTo(NodeFormatter.FormatVCPControlName("50")));
            Assert.That(vcpControlName52, Is.EqualTo(NodeFormatter.FormatVCPControlName("52")));
            Assert.That(vcpControlName54, Is.EqualTo(NodeFormatter.FormatVCPControlName("54")));
            Assert.That(vcpControlName56, Is.EqualTo(NodeFormatter.FormatVCPControlName("56")));
            Assert.That(vcpControlName58, Is.EqualTo(NodeFormatter.FormatVCPControlName("58")));
            Assert.That(vcpControlName59, Is.EqualTo(NodeFormatter.FormatVCPControlName("59")));
            Assert.That(vcpControlName5a, Is.EqualTo(NodeFormatter.FormatVCPControlName("5a")));
            Assert.That(vcpControlName5b, Is.EqualTo(NodeFormatter.FormatVCPControlName("5b")));
            Assert.That(vcpControlName5c, Is.EqualTo(NodeFormatter.FormatVCPControlName("5c")));
            Assert.That(vcpControlName5d, Is.EqualTo(NodeFormatter.FormatVCPControlName("5d")));
            Assert.That(vcpControlName5e, Is.EqualTo(NodeFormatter.FormatVCPControlName("5e")));
            Assert.That(vcpControlName60, Is.EqualTo(NodeFormatter.FormatVCPControlName("60")));
            Assert.That(vcpControlName62, Is.EqualTo(NodeFormatter.FormatVCPControlName("62")));
            Assert.That(vcpControlName63, Is.EqualTo(NodeFormatter.FormatVCPControlName("63")));
            Assert.That(vcpControlName64, Is.EqualTo(NodeFormatter.FormatVCPControlName("64")));
            Assert.That(vcpControlName65, Is.EqualTo(NodeFormatter.FormatVCPControlName("65")));
            Assert.That(vcpControlName66, Is.EqualTo(NodeFormatter.FormatVCPControlName("66")));
            Assert.That(vcpControlName68, Is.EqualTo(NodeFormatter.FormatVCPControlName("68")));
            Assert.That(vcpControlName6b, Is.EqualTo(NodeFormatter.FormatVCPControlName("6b")));
            Assert.That(vcpControlName6c, Is.EqualTo(NodeFormatter.FormatVCPControlName("6c")));
            Assert.That(vcpControlName6d, Is.EqualTo(NodeFormatter.FormatVCPControlName("6d")));
            Assert.That(vcpControlName6e, Is.EqualTo(NodeFormatter.FormatVCPControlName("6e")));
            Assert.That(vcpControlName6f, Is.EqualTo(NodeFormatter.FormatVCPControlName("6f")));
            Assert.That(vcpControlName70, Is.EqualTo(NodeFormatter.FormatVCPControlName("70")));
            Assert.That(vcpControlName71, Is.EqualTo(NodeFormatter.FormatVCPControlName("71")));
            Assert.That(vcpControlName72, Is.EqualTo(NodeFormatter.FormatVCPControlName("72")));
            Assert.That(vcpControlName73, Is.EqualTo(NodeFormatter.FormatVCPControlName("73")));
            Assert.That(vcpControlName74, Is.EqualTo(NodeFormatter.FormatVCPControlName("74")));
            Assert.That(vcpControlName75, Is.EqualTo(NodeFormatter.FormatVCPControlName("75")));
            Assert.That(vcpControlName76, Is.EqualTo(NodeFormatter.FormatVCPControlName("76")));
            Assert.That(vcpControlName78, Is.EqualTo(NodeFormatter.FormatVCPControlName("78")));
            Assert.That(vcpControlName7c, Is.EqualTo(NodeFormatter.FormatVCPControlName("7c")));
            Assert.That(vcpControlName82, Is.EqualTo(NodeFormatter.FormatVCPControlName("82")));
            Assert.That(vcpControlName84, Is.EqualTo(NodeFormatter.FormatVCPControlName("84")));
            Assert.That(vcpControlName86, Is.EqualTo(NodeFormatter.FormatVCPControlName("86")));
            Assert.That(vcpControlName87, Is.EqualTo(NodeFormatter.FormatVCPControlName("87")));
            Assert.That(vcpControlName88, Is.EqualTo(NodeFormatter.FormatVCPControlName("88")));
            Assert.That(vcpControlName8a, Is.EqualTo(NodeFormatter.FormatVCPControlName("8a")));
            Assert.That(vcpControlName8b, Is.EqualTo(NodeFormatter.FormatVCPControlName("8b")));
            Assert.That(vcpControlName8c, Is.EqualTo(NodeFormatter.FormatVCPControlName("8c")));
            Assert.That(vcpControlName8d, Is.EqualTo(NodeFormatter.FormatVCPControlName("8d")));
            Assert.That(vcpControlName8e, Is.EqualTo(NodeFormatter.FormatVCPControlName("8e")));
            Assert.That(vcpControlName8f, Is.EqualTo(NodeFormatter.FormatVCPControlName("8f")));
            Assert.That(vcpControlName90, Is.EqualTo(NodeFormatter.FormatVCPControlName("90")));
            Assert.That(vcpControlName91, Is.EqualTo(NodeFormatter.FormatVCPControlName("91")));
            Assert.That(vcpControlName92, Is.EqualTo(NodeFormatter.FormatVCPControlName("92")));
            Assert.That(vcpControlName93, Is.EqualTo(NodeFormatter.FormatVCPControlName("93")));
            Assert.That(vcpControlName94, Is.EqualTo(NodeFormatter.FormatVCPControlName("94")));
            Assert.That(vcpControlName95, Is.EqualTo(NodeFormatter.FormatVCPControlName("95")));
            Assert.That(vcpControlName96, Is.EqualTo(NodeFormatter.FormatVCPControlName("96")));
            Assert.That(vcpControlName97, Is.EqualTo(NodeFormatter.FormatVCPControlName("97")));
            Assert.That(vcpControlName98, Is.EqualTo(NodeFormatter.FormatVCPControlName("98")));
            Assert.That(vcpControlName9a, Is.EqualTo(NodeFormatter.FormatVCPControlName("9a")));
            Assert.That(vcpControlName9b, Is.EqualTo(NodeFormatter.FormatVCPControlName("9b")));
            Assert.That(vcpControlName9c, Is.EqualTo(NodeFormatter.FormatVCPControlName("9c")));
            Assert.That(vcpControlName9d, Is.EqualTo(NodeFormatter.FormatVCPControlName("9d")));
            Assert.That(vcpControlName9e, Is.EqualTo(NodeFormatter.FormatVCPControlName("9e")));
            Assert.That(vcpControlName9f, Is.EqualTo(NodeFormatter.FormatVCPControlName("9f")));
            Assert.That(vcpControlNamea0, Is.EqualTo(NodeFormatter.FormatVCPControlName("a0")));
            Assert.That(vcpControlNamea2, Is.EqualTo(NodeFormatter.FormatVCPControlName("a2")));
            Assert.That(vcpControlNamea4, Is.EqualTo(NodeFormatter.FormatVCPControlName("a4")));
            Assert.That(vcpControlNamea5, Is.EqualTo(NodeFormatter.FormatVCPControlName("a5")));
            Assert.That(vcpControlNamea6, Is.EqualTo(NodeFormatter.FormatVCPControlName("a6")));
            Assert.That(vcpControlNamea7, Is.EqualTo(NodeFormatter.FormatVCPControlName("a7")));
            Assert.That(vcpControlNamea8, Is.EqualTo(NodeFormatter.FormatVCPControlName("a8")));
            Assert.That(vcpControlNameaa, Is.EqualTo(NodeFormatter.FormatVCPControlName("aa")));
            Assert.That(vcpControlNameac, Is.EqualTo(NodeFormatter.FormatVCPControlName("ac")));
            Assert.That(vcpControlNameae, Is.EqualTo(NodeFormatter.FormatVCPControlName("ae")));
            Assert.That(vcpControlNameb0, Is.EqualTo(NodeFormatter.FormatVCPControlName("b0")));
            Assert.That(vcpControlNameb2, Is.EqualTo(NodeFormatter.FormatVCPControlName("b2")));
            Assert.That(vcpControlNameb4, Is.EqualTo(NodeFormatter.FormatVCPControlName("b4")));
            Assert.That(vcpControlNameb5, Is.EqualTo(NodeFormatter.FormatVCPControlName("b5")));
            Assert.That(vcpControlNameb6, Is.EqualTo(NodeFormatter.FormatVCPControlName("b6")));
            Assert.That(vcpControlNameb7, Is.EqualTo(NodeFormatter.FormatVCPControlName("b7")));
            Assert.That(vcpControlNameb8, Is.EqualTo(NodeFormatter.FormatVCPControlName("b8")));
            Assert.That(vcpControlNameb9, Is.EqualTo(NodeFormatter.FormatVCPControlName("b9")));
            Assert.That(vcpControlNameba, Is.EqualTo(NodeFormatter.FormatVCPControlName("ba")));
            Assert.That(vcpControlNamebb, Is.EqualTo(NodeFormatter.FormatVCPControlName("bb")));
            Assert.That(vcpControlNamebc, Is.EqualTo(NodeFormatter.FormatVCPControlName("bc")));
            Assert.That(vcpControlNamebd, Is.EqualTo(NodeFormatter.FormatVCPControlName("bd")));
            Assert.That(vcpControlNamebe, Is.EqualTo(NodeFormatter.FormatVCPControlName("be")));
            Assert.That(vcpControlNamec0, Is.EqualTo(NodeFormatter.FormatVCPControlName("c0")));
            Assert.That(vcpControlNamec2, Is.EqualTo(NodeFormatter.FormatVCPControlName("c2")));
            Assert.That(vcpControlNamec3, Is.EqualTo(NodeFormatter.FormatVCPControlName("c3")));
            Assert.That(vcpControlNamec4, Is.EqualTo(NodeFormatter.FormatVCPControlName("c4")));
            Assert.That(vcpControlNamec6, Is.EqualTo(NodeFormatter.FormatVCPControlName("c6")));
            Assert.That(vcpControlNamec7, Is.EqualTo(NodeFormatter.FormatVCPControlName("c7")));
            Assert.That(vcpControlNamec8, Is.EqualTo(NodeFormatter.FormatVCPControlName("c8")));
            Assert.That(vcpControlNamec9, Is.EqualTo(NodeFormatter.FormatVCPControlName("c9")));
            Assert.That(vcpControlNameca, Is.EqualTo(NodeFormatter.FormatVCPControlName("ca")));
            Assert.That(vcpControlNamecc, Is.EqualTo(NodeFormatter.FormatVCPControlName("cc")));
            Assert.That(vcpControlNamecd, Is.EqualTo(NodeFormatter.FormatVCPControlName("cd")));
            Assert.That(vcpControlNamece, Is.EqualTo(NodeFormatter.FormatVCPControlName("ce")));
            Assert.That(vcpControlNamecf, Is.EqualTo(NodeFormatter.FormatVCPControlName("cf")));
            Assert.That(vcpControlNamed0, Is.EqualTo(NodeFormatter.FormatVCPControlName("d0")));
            Assert.That(vcpControlNamed2, Is.EqualTo(NodeFormatter.FormatVCPControlName("d2")));
            Assert.That(vcpControlNamed4, Is.EqualTo(NodeFormatter.FormatVCPControlName("d4")));
            Assert.That(vcpControlNamed6, Is.EqualTo(NodeFormatter.FormatVCPControlName("d6")));
            Assert.That(vcpControlNamed7, Is.EqualTo(NodeFormatter.FormatVCPControlName("d7")));
            Assert.That(vcpControlNameda, Is.EqualTo(NodeFormatter.FormatVCPControlName("da")));
            Assert.That(vcpControlNamedb, Is.EqualTo(NodeFormatter.FormatVCPControlName("db")));
            Assert.That(vcpControlNamedc, Is.EqualTo(NodeFormatter.FormatVCPControlName("dc")));
            Assert.That(vcpControlNamede, Is.EqualTo(NodeFormatter.FormatVCPControlName("de")));
            Assert.That(vcpControlNamedf, Is.EqualTo(NodeFormatter.FormatVCPControlName("df")));
            Assert.That(vcpControlNamee0, Is.EqualTo(NodeFormatter.FormatVCPControlName("e0")));
            Assert.That(vcpControlNamee1, Is.EqualTo(NodeFormatter.FormatVCPControlName("e1")));
            Assert.That(vcpControlNamee2, Is.EqualTo(NodeFormatter.FormatVCPControlName("e2")));
            Assert.That(vcpControlNamee3, Is.EqualTo(NodeFormatter.FormatVCPControlName("e3")));
            Assert.That(vcpControlNamee5, Is.EqualTo(NodeFormatter.FormatVCPControlName("e5")));
            Assert.That(vcpControlNamee8, Is.EqualTo(NodeFormatter.FormatVCPControlName("e8")));
            Assert.That(vcpControlNamee9, Is.EqualTo(NodeFormatter.FormatVCPControlName("e9")));
            Assert.That(vcpControlNameea, Is.EqualTo(NodeFormatter.FormatVCPControlName("ea")));
            Assert.That(vcpControlNamef0, Is.EqualTo(NodeFormatter.FormatVCPControlName("f0")));
            Assert.That(vcpControlNamef1, Is.EqualTo(NodeFormatter.FormatVCPControlName("f1")));
            Assert.That(vcpControlNamef2, Is.EqualTo(NodeFormatter.FormatVCPControlName("f2")));
        }

        [Test]
        public void TestFormatVCP_F8()
        {
            string FormatVCP_F800 = "High Resolution";
            string FormatVCP_F801 = "High Data Speed";
            string FormatVCP_F810 = "FHD";
            string FormatVCP_F811 = "4K";
            string? FormatVCP_def = null;
            var expectvcpFormatVCP_F800 = NodeFormatter.FormatVCP_F8("F800");
            var expectFormatVCP_F801 = NodeFormatter.FormatVCP_F8("F801");
            var expectvcpFormatVCP_F810 = NodeFormatter.FormatVCP_F8("F810");
            var expectFormatVCP_F811 = NodeFormatter.FormatVCP_F8("F811");
            var expectvcpFormatVCP_def = NodeFormatter.FormatVCP_F8("F000");

            Assert.That(FormatVCP_F800, Is.EqualTo(expectvcpFormatVCP_F800));
            Assert.That(FormatVCP_F801, Is.EqualTo(expectFormatVCP_F801));
            Assert.That(FormatVCP_F810, Is.EqualTo(expectvcpFormatVCP_F810));
            Assert.That(FormatVCP_F811, Is.EqualTo(expectFormatVCP_F811));
            Assert.That(FormatVCP_def, Is.EqualTo(expectvcpFormatVCP_def));
        }

        [Test]
        public void TestFormatVCP_14()
        {
            string FormatVCP_1401 = "sRGB";
            string FormatVCP_1402 = "Display Native";
            string FormatVCP_1403 = "4000K";
            string FormatVCP_1404 = "5000K";
            string FormatVCP_1405 = "6500K";
            string FormatVCP_1406 = "7500K";
            string FormatVCP_1407 = "8200K";
            string FormatVCP_1408 = "9300K/Cool";
            string FormatVCP_1409 = "10000K";
            string FormatVCP_140a = "11500K";
            string FormatVCP_140b = "5700K/Warm";
            string FormatVCP_140c = "Custom Color 1";
            string FormatVCP_140d = "Custom Color 2";
            string? FormatVCP_140def = null;
            var expectFormatVCP_1401 = NodeFormatter.FormatVCP_14("01");
            var expectFormatVCP_1402 = NodeFormatter.FormatVCP_14("02");
            var expectFormatVCP_1403 = NodeFormatter.FormatVCP_14("03");
            var expectFormatVCP_1404 = NodeFormatter.FormatVCP_14("04");
            var expectFormatVCP_1405 = NodeFormatter.FormatVCP_14("05");
            var expectFormatVCP_1406 = NodeFormatter.FormatVCP_14("06");
            var expectFormatVCP_1407 = NodeFormatter.FormatVCP_14("07");
            var expectFormatVCP_1408 = NodeFormatter.FormatVCP_14("08");
            var expectFormatVCP_1409 = NodeFormatter.FormatVCP_14("09");
            var expectFormatVCP_140a = NodeFormatter.FormatVCP_14("0a");
            var expectFormatVCP_140b = NodeFormatter.FormatVCP_14("0b");
            var expectFormatVCP_140c = NodeFormatter.FormatVCP_14("0c");
            var expectFormatVCP_140d = NodeFormatter.FormatVCP_14("0d");
            var expectFormatVCP_140def = NodeFormatter.FormatVCP_14("0g");

            Assert.That(FormatVCP_1401, Is.EqualTo(expectFormatVCP_1401));
            Assert.That(FormatVCP_1402, Is.EqualTo(expectFormatVCP_1402));
            Assert.That(FormatVCP_1403, Is.EqualTo(expectFormatVCP_1403));
            Assert.That(FormatVCP_1404, Is.EqualTo(expectFormatVCP_1404));
            Assert.That(FormatVCP_1405, Is.EqualTo(expectFormatVCP_1405));
            Assert.That(FormatVCP_1406, Is.EqualTo(expectFormatVCP_1406));
            Assert.That(FormatVCP_1407, Is.EqualTo(expectFormatVCP_1407));
            Assert.That(FormatVCP_1408, Is.EqualTo(expectFormatVCP_1408));
            Assert.That(FormatVCP_1409, Is.EqualTo(expectFormatVCP_1409));
            Assert.That(FormatVCP_140a, Is.EqualTo(expectFormatVCP_140a));
            Assert.That(FormatVCP_140b, Is.EqualTo(expectFormatVCP_140b));
            Assert.That(FormatVCP_140c, Is.EqualTo(expectFormatVCP_140c));
            Assert.That(FormatVCP_140d, Is.EqualTo(expectFormatVCP_140d));
            Assert.That(FormatVCP_140def, Is.EqualTo(expectFormatVCP_140def));
        }

        [Test]
        public void TestFormatVCP_AA()
        {
            string FormatVCP_AA00 = "Reserved";
            string FormatVCP_AA01 = "0 degrees";
            string FormatVCP_AA02 = "90 degrees";
            string FormatVCP_AA03 = "180 degrees";
            string FormatVCP_AA04 = "270 degrees";
            string? FormatVCP_AAdef = null;
            var expectFormatVCP_AA00 = NodeFormatter.FormatVCP_AA("00");
            var expectFormatVCP_AA01 = NodeFormatter.FormatVCP_AA("01");
            var expectFormatVCP_AA02 = NodeFormatter.FormatVCP_AA("02");
            var expectFormatVCP_AA03 = NodeFormatter.FormatVCP_AA("03");
            var expectFormatVCP_AA04 = NodeFormatter.FormatVCP_AA("04");
            var expectFormatVCP_AAdef = NodeFormatter.FormatVCP_AA("05");

            Assert.That(FormatVCP_AA00, Is.EqualTo(expectFormatVCP_AA00));
            Assert.That(FormatVCP_AA01, Is.EqualTo(expectFormatVCP_AA01));
            Assert.That(FormatVCP_AA02, Is.EqualTo(expectFormatVCP_AA02));
            Assert.That(FormatVCP_AA03, Is.EqualTo(expectFormatVCP_AA03));
            Assert.That(FormatVCP_AA04, Is.EqualTo(expectFormatVCP_AA04));
            Assert.That(FormatVCP_AAdef, Is.EqualTo(expectFormatVCP_AAdef));
        }

        [Test]
        public void TestFormatVCP_D6()
        {
            string FormatVCP_D601 = "Power Normal";
            string FormatVCP_D604 = "Power Saving";
            string FormatVCP_D605 = "Power Off";
            string? FormatVCP_D6def = null;
            var expectFormatVCP_D601 = NodeFormatter.FormatVCP_D6("01");
            var expectFormatVCP_D604 = NodeFormatter.FormatVCP_D6("04");
            var expectFormatVCP_D605 = NodeFormatter.FormatVCP_D6("05");
            var expectFormatVCP_D6def = NodeFormatter.FormatVCP_D6("06");

            Assert.That(FormatVCP_D601, Is.EqualTo(expectFormatVCP_D601));
            Assert.That(FormatVCP_D604, Is.EqualTo(expectFormatVCP_D604));
            Assert.That(FormatVCP_D605, Is.EqualTo(expectFormatVCP_D605));
            Assert.That(FormatVCP_D6def, Is.EqualTo(expectFormatVCP_D6def));
        }

        [Test]
        public void TestFormatVCP_8D()
        {
            string FormatVCP_8D01 = "Mute the Mic";
            string FormatVCP_8D02 = "UnMute the Mic";
            string? FormatVCP_8Ddef = null;
            var expectFormatVCP_8D01 = NodeFormatter.FormatVCP_8D("01");
            var expectFormatVCP_8D02 = NodeFormatter.FormatVCP_8D("02");
            var expectFormatVCP_8Ddef = NodeFormatter.FormatVCP_8D("08");

            Assert.That(FormatVCP_8D01, Is.EqualTo(expectFormatVCP_8D01));
            Assert.That(FormatVCP_8D02, Is.EqualTo(expectFormatVCP_8D02));
            Assert.That(FormatVCP_8Ddef, Is.EqualTo(expectFormatVCP_8Ddef));
        }

        [Test]
        public void TestFormatVCP_CC()
        {
            string FormatVCP_CC01 = "Chinese";
            string FormatVCP_CC02 = "English";
            string FormatVCP_CC03 = "French";
            string FormatVCP_CC04 = "German";
            string FormatVCP_CC05 = "Italian";
            string FormatVCP_CC06 = "Japanese";
            string FormatVCP_CC07 = "Korean";
            string FormatVCP_CC08 = "Portuguese";
            string FormatVCP_CC09 = "Russian";
            string FormatVCP_CC0a = "Spanish";
            string FormatVCP_CC0b = "Swedish";
            string FormatVCP_CC0c = "Turkish";
            string FormatVCP_CC0d = "Chinese-Simplified";
            string FormatVCP_CC0e = "BrazilianPortuguese";
            string FormatVCP_CC0f = "Arabic";
            string FormatVCP_CC10 = "Bulgarian";
            string FormatVCP_CC11 = "Croatian";
            string FormatVCP_CC12 = "Czech";
            string FormatVCP_CC13 = "Danish";
            string FormatVCP_CC14 = "Dutch";
            string FormatVCP_CC15 = "Estonian";
            string FormatVCP_CC16 = "Finnish";
            string FormatVCP_CC17 = "Greek";
            string FormatVCP_CC18 = "Hebrew";
            string FormatVCP_CC19 = "Hindi";
            string FormatVCP_CC1a = "Hungarian";
            string FormatVCP_CC1b = "Latvian";
            string FormatVCP_CC1c = "Lithuanian";
            string FormatVCP_CC1d = "Norwegian";
            string FormatVCP_CC1e = "Polish";
            string FormatVCP_CC1f = "Romanian";
            string FormatVCP_CC20 = "Serbian";
            string FormatVCP_CC21 = "Slovak";
            string FormatVCP_CC22 = "Slovenian";
            string FormatVCP_CC23 = "Thai";
            string FormatVCP_CC24 = "Ukrainian";
            string FormatVCP_CC25 = "Vietnamese";
            string? FormatVCP_CCdef = null;

            Assert.That(FormatVCP_CC01, Is.EqualTo(NodeFormatter.FormatVCP_CC("01")));
            Assert.That(FormatVCP_CC02, Is.EqualTo(NodeFormatter.FormatVCP_CC("02")));
            Assert.That(FormatVCP_CC03, Is.EqualTo(NodeFormatter.FormatVCP_CC("03")));
            Assert.That(FormatVCP_CC04, Is.EqualTo(NodeFormatter.FormatVCP_CC("04")));
            Assert.That(FormatVCP_CC05, Is.EqualTo(NodeFormatter.FormatVCP_CC("05")));
            Assert.That(FormatVCP_CC06, Is.EqualTo(NodeFormatter.FormatVCP_CC("06")));
            Assert.That(FormatVCP_CC07, Is.EqualTo(NodeFormatter.FormatVCP_CC("07")));
            Assert.That(FormatVCP_CC08, Is.EqualTo(NodeFormatter.FormatVCP_CC("08")));
            Assert.That(FormatVCP_CC09, Is.EqualTo(NodeFormatter.FormatVCP_CC("09")));
            Assert.That(FormatVCP_CC0a, Is.EqualTo(NodeFormatter.FormatVCP_CC("0a")));
            Assert.That(FormatVCP_CC0b, Is.EqualTo(NodeFormatter.FormatVCP_CC("0b")));
            Assert.That(FormatVCP_CC0c, Is.EqualTo(NodeFormatter.FormatVCP_CC("0c")));
            Assert.That(FormatVCP_CC0d, Is.EqualTo(NodeFormatter.FormatVCP_CC("0d")));
            Assert.That(FormatVCP_CC0e, Is.EqualTo(NodeFormatter.FormatVCP_CC("0e")));
            Assert.That(FormatVCP_CC0f, Is.EqualTo(NodeFormatter.FormatVCP_CC("0f")));
            Assert.That(FormatVCP_CC10, Is.EqualTo(NodeFormatter.FormatVCP_CC("10")));
            Assert.That(FormatVCP_CC11, Is.EqualTo(NodeFormatter.FormatVCP_CC("11")));
            Assert.That(FormatVCP_CC12, Is.EqualTo(NodeFormatter.FormatVCP_CC("12")));
            Assert.That(FormatVCP_CC13, Is.EqualTo(NodeFormatter.FormatVCP_CC("13")));
            Assert.That(FormatVCP_CC14, Is.EqualTo(NodeFormatter.FormatVCP_CC("14")));
            Assert.That(FormatVCP_CC15, Is.EqualTo(NodeFormatter.FormatVCP_CC("15")));
            Assert.That(FormatVCP_CC16, Is.EqualTo(NodeFormatter.FormatVCP_CC("16")));
            Assert.That(FormatVCP_CC17, Is.EqualTo(NodeFormatter.FormatVCP_CC("17")));
            Assert.That(FormatVCP_CC18, Is.EqualTo(NodeFormatter.FormatVCP_CC("18")));
            Assert.That(FormatVCP_CC19, Is.EqualTo(NodeFormatter.FormatVCP_CC("19")));
            Assert.That(FormatVCP_CC1a, Is.EqualTo(NodeFormatter.FormatVCP_CC("1a")));
            Assert.That(FormatVCP_CC1b, Is.EqualTo(NodeFormatter.FormatVCP_CC("1b")));
            Assert.That(FormatVCP_CC1c, Is.EqualTo(NodeFormatter.FormatVCP_CC("1c")));
            Assert.That(FormatVCP_CC1d, Is.EqualTo(NodeFormatter.FormatVCP_CC("1d")));
            Assert.That(FormatVCP_CC1e, Is.EqualTo(NodeFormatter.FormatVCP_CC("1e")));
            Assert.That(FormatVCP_CC1f, Is.EqualTo(NodeFormatter.FormatVCP_CC("1f")));
            Assert.That(FormatVCP_CC20, Is.EqualTo(NodeFormatter.FormatVCP_CC("20")));
            Assert.That(FormatVCP_CC21, Is.EqualTo(NodeFormatter.FormatVCP_CC("21")));
            Assert.That(FormatVCP_CC22, Is.EqualTo(NodeFormatter.FormatVCP_CC("22")));
            Assert.That(FormatVCP_CC23, Is.EqualTo(NodeFormatter.FormatVCP_CC("23")));
            Assert.That(FormatVCP_CC24, Is.EqualTo(NodeFormatter.FormatVCP_CC("24")));
            Assert.That(FormatVCP_CC25, Is.EqualTo(NodeFormatter.FormatVCP_CC("25")));
            Assert.That(FormatVCP_CCdef, Is.EqualTo(NodeFormatter.FormatVCP_CC("26")));
        }

        [Test]
        public void TestFormatVCP_60()
        {
            string FormatVCP_6001 = "VGA1";
            string FormatVCP_6002 = "VGA2";
            string FormatVCP_6003 = "DVI1";
            string FormatVCP_6004 = "DVI2";
            string FormatVCP_6005 = "Composite video1";
            string FormatVCP_6006 = "Composite video2";
            string FormatVCP_6007 = "S-Video1";
            string FormatVCP_6008 = "S-Video2";
            string FormatVCP_6009 = "Tuner1";
            string FormatVCP_600a = "Tuner2";
            string FormatVCP_600b = "Tuner3";
            string FormatVCP_600c = "Component video (YPrPb/YCrCb)1";
            string FormatVCP_600d = "Component video (YPrPb/YCrCb)2";
            string FormatVCP_600e = "Component video (YPrPb/YCrCb)3";
            string FormatVCP_600f = "DisplayPort1";
            string FormatVCP_6010 = "Mini DisplayPort1";
            string FormatVCP_6011 = "HDMI1";
            string FormatVCP_6012 = "HDMI2";
            string FormatVCP_6013 = "DisplayPort2";
            string FormatVCP_6014 = "Mini DisplayPort2";
            string FormatVCP_6015 = "HDMI3";
            string FormatVCP_6016 = "HDMI4";
            string FormatVCP_6017 = "DisplayPort3";
            string FormatVCP_6018 = "Mini DisplayPort3";
            string FormatVCP_6019 = "Thunderbolt1";
            string FormatVCP_601a = "Thunderbolt2";
            string FormatVCP_601b = "USB-C1";
            string FormatVCP_601c = "USB-C2";
            string FormatVCP_601d = "USB-C3";
            string FormatVCP_601e = "USB-C4";
            string FormatVCP_6080 = "USB Comm from USB1, Type-B, port 1";
            string FormatVCP_6081 = "USB Comm from USB2, Type-B, port 2";
            string FormatVCP_6082 = "USB Comm from USB-C1, Type-C, port 1";
            string FormatVCP_6083 = "USB Comm from USB-C2, Type-C, port 2";
            string FormatVCP_6084 = "USB Comm from USB-C3, Type-C, port 3";
            string FormatVCP_6085 = "USB Comm from USB-C4, Type-C, port 4";
            string? FormatVCP_60def = null;

            Assert.That(FormatVCP_6001, Is.EqualTo(NodeFormatter.FormatVCP_60("01")));
            Assert.That(FormatVCP_6002, Is.EqualTo(NodeFormatter.FormatVCP_60("02")));
            Assert.That(FormatVCP_6003, Is.EqualTo(NodeFormatter.FormatVCP_60("03")));
            Assert.That(FormatVCP_6004, Is.EqualTo(NodeFormatter.FormatVCP_60("04")));
            Assert.That(FormatVCP_6005, Is.EqualTo(NodeFormatter.FormatVCP_60("05")));
            Assert.That(FormatVCP_6006, Is.EqualTo(NodeFormatter.FormatVCP_60("06")));
            Assert.That(FormatVCP_6007, Is.EqualTo(NodeFormatter.FormatVCP_60("07")));
            Assert.That(FormatVCP_6008, Is.EqualTo(NodeFormatter.FormatVCP_60("08")));
            Assert.That(FormatVCP_6009, Is.EqualTo(NodeFormatter.FormatVCP_60("09")));
            Assert.That(FormatVCP_600a, Is.EqualTo(NodeFormatter.FormatVCP_60("0a")));
            Assert.That(FormatVCP_600b, Is.EqualTo(NodeFormatter.FormatVCP_60("0b")));
            Assert.That(FormatVCP_600c, Is.EqualTo(NodeFormatter.FormatVCP_60("0c")));
            Assert.That(FormatVCP_600d, Is.EqualTo(NodeFormatter.FormatVCP_60("0d")));
            Assert.That(FormatVCP_600e, Is.EqualTo(NodeFormatter.FormatVCP_60("0e")));
            Assert.That(FormatVCP_600f, Is.EqualTo(NodeFormatter.FormatVCP_60("0f")));
            Assert.That(FormatVCP_6010, Is.EqualTo(NodeFormatter.FormatVCP_60("10")));
            Assert.That(FormatVCP_6011, Is.EqualTo(NodeFormatter.FormatVCP_60("11")));
            Assert.That(FormatVCP_6012, Is.EqualTo(NodeFormatter.FormatVCP_60("12")));
            Assert.That(FormatVCP_6013, Is.EqualTo(NodeFormatter.FormatVCP_60("13")));
            Assert.That(FormatVCP_6014, Is.EqualTo(NodeFormatter.FormatVCP_60("14")));
            Assert.That(FormatVCP_6015, Is.EqualTo(NodeFormatter.FormatVCP_60("15")));
            Assert.That(FormatVCP_6016, Is.EqualTo(NodeFormatter.FormatVCP_60("16")));
            Assert.That(FormatVCP_6017, Is.EqualTo(NodeFormatter.FormatVCP_60("17")));
            Assert.That(FormatVCP_6018, Is.EqualTo(NodeFormatter.FormatVCP_60("18")));
            Assert.That(FormatVCP_6019, Is.EqualTo(NodeFormatter.FormatVCP_60("19")));
            Assert.That(FormatVCP_601a, Is.EqualTo(NodeFormatter.FormatVCP_60("1a")));
            Assert.That(FormatVCP_601b, Is.EqualTo(NodeFormatter.FormatVCP_60("1b")));
            Assert.That(FormatVCP_601c, Is.EqualTo(NodeFormatter.FormatVCP_60("1c")));
            Assert.That(FormatVCP_601d, Is.EqualTo(NodeFormatter.FormatVCP_60("1d")));
            Assert.That(FormatVCP_601e, Is.EqualTo(NodeFormatter.FormatVCP_60("1e")));
            Assert.That(FormatVCP_6080, Is.EqualTo(NodeFormatter.FormatVCP_60("80")));
            Assert.That(FormatVCP_6081, Is.EqualTo(NodeFormatter.FormatVCP_60("81")));
            Assert.That(FormatVCP_6082, Is.EqualTo(NodeFormatter.FormatVCP_60("82")));
            Assert.That(FormatVCP_6083, Is.EqualTo(NodeFormatter.FormatVCP_60("83")));
            Assert.That(FormatVCP_6084, Is.EqualTo(NodeFormatter.FormatVCP_60("84")));
            Assert.That(FormatVCP_6085, Is.EqualTo(NodeFormatter.FormatVCP_60("85")));
            Assert.That(FormatVCP_60def, Is.EqualTo(NodeFormatter.FormatVCP_60("86")));
        }

        [Test]
        public void TestFormatVCP_66()
        {
            string FormatVCP_6600f2 = "ALS full function";
            string FormatVCP_660f02 = "ALS full function";
            string FormatVCP_6600d2 = "ALS without ALS_Primary";
            string FormatVCP_660d02 = "ALS without ALS_Primary";
            string FormatVCP_660012 = "ALS without sensor";
            string FormatVCP_660102 = "ALS without sensor";
            string? FormatVCP_66def = null;
            var expectFormatVCP_6600f2 = NodeFormatter.FormatVCP_66("00f2");
            var expectFormatVCP_660f02 = NodeFormatter.FormatVCP_66("0f02");
            var expectFormatVCP_6600d2 = NodeFormatter.FormatVCP_66("00d2");
            var expectFormatVCP_660d02 = NodeFormatter.FormatVCP_66("0d02");
            var expectFormatVCP_660012 = NodeFormatter.FormatVCP_66("0012");
            var expectFormatVCP_660102 = NodeFormatter.FormatVCP_66("0102");
            var expectFormatVCP_66def = NodeFormatter.FormatVCP_66("0111");

            Assert.That(FormatVCP_6600f2, Is.EqualTo(expectFormatVCP_6600f2));
            Assert.That(FormatVCP_660f02, Is.EqualTo(expectFormatVCP_660f02));
            Assert.That(FormatVCP_6600d2, Is.EqualTo(expectFormatVCP_6600d2));
            Assert.That(FormatVCP_660d02, Is.EqualTo(expectFormatVCP_660d02));
            Assert.That(FormatVCP_660012, Is.EqualTo(expectFormatVCP_660012));
            Assert.That(FormatVCP_660102, Is.EqualTo(expectFormatVCP_660102));
            Assert.That(FormatVCP_66def, Is.EqualTo(expectFormatVCP_66def));
        }

        [Test]
        public void TestFormatVCP_DC()
        {
            string FormatVCP_DC00 = "Standard/Native";
            string FormatVCP_DC02 = "Multimedia";
            string FormatVCP_DC03 = "Movie";
            string FormatVCP_DC04 = "Nature";
            string FormatVCP_DC05 = "Game/Game1";
            string FormatVCP_DC06 = "Sport";
            string? FormatVCP_DCdef = null;
            var expectFormatVCP_DC00 = NodeFormatter.FormatVCP_DC("00");
            var expectFormatVCP_DC02 = NodeFormatter.FormatVCP_DC("02");
            var expectFormatVCP_DC03 = NodeFormatter.FormatVCP_DC("03");
            var expectFormatVCP_DC04 = NodeFormatter.FormatVCP_DC("04");
            var expectFormatVCP_DC05 = NodeFormatter.FormatVCP_DC("05");
            var expectFormatVCP_DC06 = NodeFormatter.FormatVCP_DC("06");
            var expectFormatVCP_DCdef = NodeFormatter.FormatVCP_DC("07");

            Assert.That(FormatVCP_DC00, Is.EqualTo(expectFormatVCP_DC00));
            Assert.That(FormatVCP_DC02, Is.EqualTo(expectFormatVCP_DC02));
            Assert.That(FormatVCP_DC03, Is.EqualTo(expectFormatVCP_DC03));
            Assert.That(FormatVCP_DC04, Is.EqualTo(expectFormatVCP_DC04));
            Assert.That(FormatVCP_DC05, Is.EqualTo(expectFormatVCP_DC05));
            Assert.That(FormatVCP_DC06, Is.EqualTo(expectFormatVCP_DC06));
            Assert.That(FormatVCP_DCdef, Is.EqualTo(expectFormatVCP_DCdef));
        }

        [Test]
        public void TestFormatVCP_F0()
        {
            string FormatVCP_F001 = "Text";
            string FormatVCP_F002 = "AdobeRGB";
            string FormatVCP_F003 = "xvMode";
            string FormatVCP_F004 = "DICOM";
            string FormatVCP_F005 = "CAL1";
            string FormatVCP_F021 = "AdobeRGB1/Adobe RGB D65 G2.2 L160/Adobe RGB D65 G2.2 L250";
            string FormatVCP_F022 = "AdobeRGB2/Adobe RGB D50 G2.2 L160/Adobe RGB D50 G2.2 L250";
            string FormatVCP_F006 = "CAL2";
            string FormatVCP_F007 = "Metro";
            string FormatVCP_F008 = "Paper";
            string FormatVCP_F009 = "Rec.709 / BT.709";
            string FormatVCP_F00a = "DCI-P3";
            string FormatVCP_F00b = "Rec2020";
            string FormatVCP_F00c = "ComfortView";
            string FormatVCP_F00d = "Game2";
            string FormatVCP_F00e = "Game3";
            string FormatVCP_F00f = "FPS Game";
            string FormatVCP_F010 = "RTS Game";
            string FormatVCP_F011 = "RPG Game";
            string FormatVCP_F012 = "Multiscreen Match";
            string FormatVCP_F013 = "SPORTS Game";
            string FormatVCP_F030 = "Standard HDR";
            string FormatVCP_F031 = "Movie HDR";
            string FormatVCP_F032 = "Game HDR";
            string FormatVCP_F033 = "Vivid HDR";
            string FormatVCP_F034 = "Desktop";
            string FormatVCP_F035 = "Reference";
            string FormatVCP_F036 = "DisplayHDR";
            string FormatVCP_F037 = "HDR10";
            string FormatVCP_F038 = "HLG";
            string FormatVCP_F0a1 = "Display P3";
            string FormatVCP_F0c1 = "Custom 1 / User 1";
            string FormatVCP_F0c2 = "Custom 2 / User 2";
            string FormatVCP_F0c3 = "Custom 3 / User 3";
            string? FormatVCP_F0def = null;

            Assert.That(FormatVCP_F001, Is.EqualTo(NodeFormatter.FormatVCP_F0("01")));
            Assert.That(FormatVCP_F002, Is.EqualTo(NodeFormatter.FormatVCP_F0("02")));
            Assert.That(FormatVCP_F003, Is.EqualTo(NodeFormatter.FormatVCP_F0("03")));
            Assert.That(FormatVCP_F004, Is.EqualTo(NodeFormatter.FormatVCP_F0("04")));
            Assert.That(FormatVCP_F005, Is.EqualTo(NodeFormatter.FormatVCP_F0("05")));
            Assert.That(FormatVCP_F021, Is.EqualTo(NodeFormatter.FormatVCP_F0("21")));
            Assert.That(FormatVCP_F022, Is.EqualTo(NodeFormatter.FormatVCP_F0("22")));
            Assert.That(FormatVCP_F006, Is.EqualTo(NodeFormatter.FormatVCP_F0("06")));
            Assert.That(FormatVCP_F007, Is.EqualTo(NodeFormatter.FormatVCP_F0("07")));
            Assert.That(FormatVCP_F008, Is.EqualTo(NodeFormatter.FormatVCP_F0("08")));
            Assert.That(FormatVCP_F009, Is.EqualTo(NodeFormatter.FormatVCP_F0("09")));
            Assert.That(FormatVCP_F00a, Is.EqualTo(NodeFormatter.FormatVCP_F0("0a")));
            Assert.That(FormatVCP_F00b, Is.EqualTo(NodeFormatter.FormatVCP_F0("0b")));
            Assert.That(FormatVCP_F00c, Is.EqualTo(NodeFormatter.FormatVCP_F0("0c")));
            Assert.That(FormatVCP_F00d, Is.EqualTo(NodeFormatter.FormatVCP_F0("0d")));
            Assert.That(FormatVCP_F00e, Is.EqualTo(NodeFormatter.FormatVCP_F0("0e")));
            Assert.That(FormatVCP_F00f, Is.EqualTo(NodeFormatter.FormatVCP_F0("0f")));
            Assert.That(FormatVCP_F010, Is.EqualTo(NodeFormatter.FormatVCP_F0("10")));
            Assert.That(FormatVCP_F011, Is.EqualTo(NodeFormatter.FormatVCP_F0("11")));
            Assert.That(FormatVCP_F012, Is.EqualTo(NodeFormatter.FormatVCP_F0("12")));
            Assert.That(FormatVCP_F013, Is.EqualTo(NodeFormatter.FormatVCP_F0("13")));
            Assert.That(FormatVCP_F030, Is.EqualTo(NodeFormatter.FormatVCP_F0("30")));
            Assert.That(FormatVCP_F031, Is.EqualTo(NodeFormatter.FormatVCP_F0("31")));
            Assert.That(FormatVCP_F032, Is.EqualTo(NodeFormatter.FormatVCP_F0("32")));
            Assert.That(FormatVCP_F033, Is.EqualTo(NodeFormatter.FormatVCP_F0("33")));
            Assert.That(FormatVCP_F034, Is.EqualTo(NodeFormatter.FormatVCP_F0("34")));
            Assert.That(FormatVCP_F035, Is.EqualTo(NodeFormatter.FormatVCP_F0("35")));
            Assert.That(FormatVCP_F036, Is.EqualTo(NodeFormatter.FormatVCP_F0("36")));
            Assert.That(FormatVCP_F037, Is.EqualTo(NodeFormatter.FormatVCP_F0("37")));
            Assert.That(FormatVCP_F038, Is.EqualTo(NodeFormatter.FormatVCP_F0("38")));
            Assert.That(FormatVCP_F0a1, Is.EqualTo(NodeFormatter.FormatVCP_F0("a1")));
            Assert.That(FormatVCP_F0c1, Is.EqualTo(NodeFormatter.FormatVCP_F0("c1")));
            Assert.That(FormatVCP_F0c2, Is.EqualTo(NodeFormatter.FormatVCP_F0("c2")));
            Assert.That(FormatVCP_F0c3, Is.EqualTo(NodeFormatter.FormatVCP_F0("c3")));
            Assert.That(FormatVCP_F0def, Is.EqualTo(NodeFormatter.FormatVCP_F0("c4")));
        }

        [Test]
        public void TestFormatVCP_E2()
        {
            string FormatVCP_E200 = "Standard/Native";
            string FormatVCP_E201 = "Multimedia";
            string FormatVCP_E202 = "Movie";
            string FormatVCP_E203 = "Nature";
            string FormatVCP_E204 = "Game/Game1";
            string FormatVCP_E205 = "Sport";
            string FormatVCP_E206 = "Text";
            string FormatVCP_E207 = "AdobeRGB";
            string FormatVCP_E208 = "xvMode";
            string FormatVCP_E209 = "DICOM";
            string FormatVCP_E20a = "CAL1";
            string FormatVCP_E20b = "sRGB";
            string FormatVCP_E20c = "5000K";
            string FormatVCP_E20d = "5700K";
            string FormatVCP_E20e = "Warm";
            string FormatVCP_E20f = "6500K";
            string FormatVCP_E210 = "7500K";
            string FormatVCP_E211 = "9300K";
            string FormatVCP_E212 = "Cool";
            string FormatVCP_E213 = "10000K";
            string FormatVCP_E214 = "Custom Color";
            string FormatVCP_E215 = "CAL2";
            string FormatVCP_E218 = "Metro";
            string FormatVCP_E219 = "Paper";
            string FormatVCP_E21a = "Rec.709 / BT.709";
            string FormatVCP_E21b = "DCI-P3";
            string FormatVCP_E21c = "Rec2020";
            string FormatVCP_E21d = "ComfortView";
            string FormatVCP_E21e = "Game2";
            string FormatVCP_E21f = "Game3";
            string FormatVCP_E220 = "FPS Game";
            string FormatVCP_E221 = "RTS Game";
            string FormatVCP_E222 = "RPG Game";
            string FormatVCP_E22f = "SPORTS Game";
            string FormatVCP_E225 = "Standard HDR";
            string FormatVCP_E223 = "Movie HDR";
            string FormatVCP_E224 = "Game HDR";
            string FormatVCP_E226 = "Vivid HDR";
            string FormatVCP_E227 = "Desktop";
            string FormatVCP_E228 = "Reference";
            string FormatVCP_E229 = "Multiscreen Match";
            string FormatVCP_E22a = "AdobeRGB1";
            string FormatVCP_E22b = "AdobeRGB2";
            string FormatVCP_E22c = "Custom 1 / User 1";
            string FormatVCP_E22d = "Custom 2 / User 2";
            string FormatVCP_E22e = "Custom 3 / User 3";
            string FormatVCP_E23a = "DisplayHDR";
            string FormatVCP_E23b = "HDR10";
            string FormatVCP_E23c = "HLG";
            string FormatVCP_E27f = "Presets Disabled";
            string? FormatVCP_E2def = null;

            Assert.That(FormatVCP_E200, Is.EqualTo(NodeFormatter.FormatVCP_E2("00")));
            Assert.That(FormatVCP_E201, Is.EqualTo(NodeFormatter.FormatVCP_E2("01")));
            Assert.That(FormatVCP_E202, Is.EqualTo(NodeFormatter.FormatVCP_E2("02")));
            Assert.That(FormatVCP_E203, Is.EqualTo(NodeFormatter.FormatVCP_E2("03")));
            Assert.That(FormatVCP_E204, Is.EqualTo(NodeFormatter.FormatVCP_E2("04")));
            Assert.That(FormatVCP_E205, Is.EqualTo(NodeFormatter.FormatVCP_E2("05")));
            Assert.That(FormatVCP_E206, Is.EqualTo(NodeFormatter.FormatVCP_E2("06")));
            Assert.That(FormatVCP_E207, Is.EqualTo(NodeFormatter.FormatVCP_E2("07")));
            Assert.That(FormatVCP_E208, Is.EqualTo(NodeFormatter.FormatVCP_E2("08")));
            Assert.That(FormatVCP_E209, Is.EqualTo(NodeFormatter.FormatVCP_E2("09")));
            Assert.That(FormatVCP_E20a, Is.EqualTo(NodeFormatter.FormatVCP_E2("0a")));
            Assert.That(FormatVCP_E20b, Is.EqualTo(NodeFormatter.FormatVCP_E2("0b")));
            Assert.That(FormatVCP_E20c, Is.EqualTo(NodeFormatter.FormatVCP_E2("0c")));
            Assert.That(FormatVCP_E20d, Is.EqualTo(NodeFormatter.FormatVCP_E2("0d")));
            Assert.That(FormatVCP_E20e, Is.EqualTo(NodeFormatter.FormatVCP_E2("0e")));
            Assert.That(FormatVCP_E20f, Is.EqualTo(NodeFormatter.FormatVCP_E2("0f")));
            Assert.That(FormatVCP_E210, Is.EqualTo(NodeFormatter.FormatVCP_E2("10")));
            Assert.That(FormatVCP_E211, Is.EqualTo(NodeFormatter.FormatVCP_E2("11")));
            Assert.That(FormatVCP_E212, Is.EqualTo(NodeFormatter.FormatVCP_E2("12")));
            Assert.That(FormatVCP_E213, Is.EqualTo(NodeFormatter.FormatVCP_E2("13")));
            Assert.That(FormatVCP_E214, Is.EqualTo(NodeFormatter.FormatVCP_E2("14")));
            Assert.That(FormatVCP_E215, Is.EqualTo(NodeFormatter.FormatVCP_E2("15")));
            Assert.That(FormatVCP_E218, Is.EqualTo(NodeFormatter.FormatVCP_E2("18")));
            Assert.That(FormatVCP_E219, Is.EqualTo(NodeFormatter.FormatVCP_E2("19")));
            Assert.That(FormatVCP_E21a, Is.EqualTo(NodeFormatter.FormatVCP_E2("1a")));
            Assert.That(FormatVCP_E21b, Is.EqualTo(NodeFormatter.FormatVCP_E2("1b")));
            Assert.That(FormatVCP_E21c, Is.EqualTo(NodeFormatter.FormatVCP_E2("1c")));
            Assert.That(FormatVCP_E21d, Is.EqualTo(NodeFormatter.FormatVCP_E2("1d")));
            Assert.That(FormatVCP_E21e, Is.EqualTo(NodeFormatter.FormatVCP_E2("1e")));
            Assert.That(FormatVCP_E21f, Is.EqualTo(NodeFormatter.FormatVCP_E2("1f")));
            Assert.That(FormatVCP_E220, Is.EqualTo(NodeFormatter.FormatVCP_E2("20")));
            Assert.That(FormatVCP_E221, Is.EqualTo(NodeFormatter.FormatVCP_E2("21")));
            Assert.That(FormatVCP_E222, Is.EqualTo(NodeFormatter.FormatVCP_E2("22")));
            Assert.That(FormatVCP_E22f, Is.EqualTo(NodeFormatter.FormatVCP_E2("2f")));
            Assert.That(FormatVCP_E225, Is.EqualTo(NodeFormatter.FormatVCP_E2("25")));
            Assert.That(FormatVCP_E223, Is.EqualTo(NodeFormatter.FormatVCP_E2("23")));
            Assert.That(FormatVCP_E224, Is.EqualTo(NodeFormatter.FormatVCP_E2("24")));
            Assert.That(FormatVCP_E226, Is.EqualTo(NodeFormatter.FormatVCP_E2("26")));
            Assert.That(FormatVCP_E227, Is.EqualTo(NodeFormatter.FormatVCP_E2("27")));
            Assert.That(FormatVCP_E228, Is.EqualTo(NodeFormatter.FormatVCP_E2("28")));
            Assert.That(FormatVCP_E229, Is.EqualTo(NodeFormatter.FormatVCP_E2("29")));
            Assert.That(FormatVCP_E22a, Is.EqualTo(NodeFormatter.FormatVCP_E2("2a")));
            Assert.That(FormatVCP_E22b, Is.EqualTo(NodeFormatter.FormatVCP_E2("2b")));
            Assert.That(FormatVCP_E22c, Is.EqualTo(NodeFormatter.FormatVCP_E2("2c")));
            Assert.That(FormatVCP_E22d, Is.EqualTo(NodeFormatter.FormatVCP_E2("2d")));
            Assert.That(FormatVCP_E22e, Is.EqualTo(NodeFormatter.FormatVCP_E2("2e")));
            Assert.That(FormatVCP_E23a, Is.EqualTo(NodeFormatter.FormatVCP_E2("3a")));
            Assert.That(FormatVCP_E23b, Is.EqualTo(NodeFormatter.FormatVCP_E2("3b")));
            Assert.That(FormatVCP_E23c, Is.EqualTo(NodeFormatter.FormatVCP_E2("3c")));
            Assert.That(FormatVCP_E27f, Is.EqualTo(NodeFormatter.FormatVCP_E2("7f")));
            Assert.That(FormatVCP_E2def, Is.EqualTo(NodeFormatter.FormatVCP_E2("7g")));
        }

        [Test]
        public void TestFormatNode()
        {
            INode node;
            var nodeMock = new Mock<INode>();
            node = nodeMock.Object;
            nodeMock.Setup(n => n.Parent).Returns(nodeMock.Object);
            nodeMock.Setup(n => n.Value).Returns("SOME_VALUE");
            var lookupTables = new Dictionary<string, Func<string, string>>
            {
                { "parentkey", value => "FORMATTED_" + value }
            };
            // Act
            var result = formatter.FormatNode(nodeMock.Object);  //result is null
            // Assert
            Assert.IsNull(result);

            /*
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
            { "vcp_66", (i) => FormatVCP_66(i) }*/

            Mock<INode> mockNode2 = new Mock<INode>();
            mockNode2.Setup(n => n.Value).Returns("05");
            Mock<INode> mockParent2 = new Mock<INode>();
            mockParent2.Setup(p => p.ToString()).Returns("vcp");
            mockNode2.Setup(n => n.Parent).Returns(mockParent2.Object);
            string vcpControlName01 = "Restore Factory Brightness/Contrast Defaults";
            var formatter2 = new NodeFormatter();
            // Act
            var result2 = formatter2.FormatNode(mockNode2.Object); //"vcp", (i) => FormatVCPControlName(i), "05": return "Restore Factory Brightness/Contrast Defaults";
            // Assert
            Assert.IsNotNull(result2);
            Assert.That(vcpControlName01, Is.EqualTo(result2)); //result is not null

            Mock<INode> mockNode3 = new Mock<INode>();
            mockNode3.Setup(n => n.Value).Returns("02");
            Mock<INode> mockParent3 = new Mock<INode>();
            mockParent3.Setup(p => p.ToString()).Returns("vcp_14");
            mockNode3.Setup(n => n.Parent).Returns(mockParent3.Object);
            string FormatVCP_1402 = "Display Native";
            var formatter3 = new NodeFormatter();
            // Act
            var result3 = formatter3.FormatNode(mockNode3.Object); //"vcp_14", (i) => FormatVCP_14(i), "02": return "Display Native";
            // Assert
            Assert.IsNotNull(result3);
            Assert.That(FormatVCP_1402, Is.EqualTo(result3)); //result is not null

            Mock<INode> mockNode4 = new Mock<INode>();
            mockNode4.Setup(n => n.Value).Returns("01");
            Mock<INode> mockParent4 = new Mock<INode>();
            mockParent4.Setup(p => p.ToString()).Returns("vcp_d6");
            mockNode4.Setup(n => n.Parent).Returns(mockParent4.Object);
            string FormatVCP_D601 = "Power Normal";
            var formatter4 = new NodeFormatter();
            // Act
            var result4 = formatter4.FormatNode(mockNode4.Object); //"vcp_d6", (i) => FormatVCP_D6(i),"01": return "Power Normal";
            // Assert
            Assert.IsNotNull(result4);
            Assert.That(FormatVCP_D601, Is.EqualTo(result4)); //result is not null

            Mock<INode> mockNode5 = new Mock<INode>();
            mockNode5.Setup(n => n.Value).Returns("0f");
            Mock<INode> mockParent5 = new Mock<INode>();
            mockParent5.Setup(p => p.ToString()).Returns("vcp_60");
            mockNode5.Setup(n => n.Parent).Returns(mockParent5.Object);
            string FormatVCP_600f = "DisplayPort1";
            var formatter5 = new NodeFormatter();
            // Act
            var result5 = formatter5.FormatNode(mockNode5.Object); //"vcp_60", (i) => FormatVCP_60(i),"0f": return "DisplayPort1";;
            // Assert
            Assert.IsNotNull(result5);
            Assert.That(FormatVCP_600f, Is.EqualTo(result5)); //result is not null

            Mock<INode> mockNode6 = new Mock<INode>();
            mockNode6.Setup(n => n.Value).Returns("05");
            Mock<INode> mockParent6 = new Mock<INode>();
            mockParent6.Setup(p => p.ToString()).Returns("vcp_dc");
            mockNode6.Setup(n => n.Parent).Returns(mockParent6.Object);
            string FormatVCP_DC05 = "Game/Game1";
            var formatter6 = new NodeFormatter();
            // Act
            var result6 = formatter6.FormatNode(mockNode6.Object); //"vcp_dc", (i) => FormatVCP_DC(i),"05": return "Game/Game1";;;
            // Assert
            Assert.IsNotNull(result6);
            Assert.That(FormatVCP_DC05, Is.EqualTo(result6)); //result is not null

            Mock<INode> mockNode7 = new Mock<INode>();
            mockNode7.Setup(n => n.Value).Returns("30");
            Mock<INode> mockParent7 = new Mock<INode>();
            mockParent7.Setup(p => p.ToString()).Returns("vcp_f0");
            mockNode7.Setup(n => n.Parent).Returns(mockParent7.Object);
            string FormatVCP_F030 = "Standard HDR";
            var formatter7 = new NodeFormatter();
            // Act
            var result7 = formatter7.FormatNode(mockNode7.Object); //"vcp_f0", (i) => FormatVCP_F0(i),"30": return "Standard HDR";;
            // Assert
            Assert.IsNotNull(result7);
            Assert.That(FormatVCP_F030, Is.EqualTo(result7)); //result is not null

            Mock<INode> mockNode8 = new Mock<INode>();
            mockNode8.Setup(n => n.Value).Returns("0e");
            Mock<INode> mockParent8 = new Mock<INode>();
            mockParent8.Setup(p => p.ToString()).Returns("vcp_e2");
            mockNode8.Setup(n => n.Parent).Returns(mockParent8.Object);
            string FormatVCP_E20e = "Warm";
            var formatter8 = new NodeFormatter();
            // Act
            var result8 = formatter8.FormatNode(mockNode8.Object); //"vcp_e2", (i) => FormatVCP_E2(i),"0e": return "Warm";
            // Assert
            Assert.IsNotNull(result8);
            Assert.That(FormatVCP_E20e, Is.EqualTo(result8)); //result is not null

            Mock<INode> mockNode9 = new Mock<INode>();
            mockNode9.Setup(n => n.Value).Returns("02");
            Mock<INode> mockParent9 = new Mock<INode>();
            mockParent9.Setup(p => p.ToString()).Returns("vcp_cc");
            mockNode9.Setup(n => n.Parent).Returns(mockParent9.Object);
            string FormatVCP_CC02 = "English";
            var formatter9 = new NodeFormatter();
            // Act
            var result9 = formatter9.FormatNode(mockNode9.Object); //"vcp_cc", (i) => FormatVCP_CC(i) ,"02": return "English";;
            // Assert
            Assert.IsNotNull(result9);
            Assert.That(FormatVCP_CC02, Is.EqualTo(result9)); //result is not null

            Mock<INode> mockNode10 = new Mock<INode>();
            mockNode10.Setup(n => n.Value).Returns("01");
            Mock<INode> mockParent10 = new Mock<INode>();
            mockParent10.Setup(p => p.ToString()).Returns("vcp_8d");
            mockNode10.Setup(n => n.Parent).Returns(mockParent10.Object);
            string FormatVCP_8D01 = "Mute the Mic";
            var formatter10 = new NodeFormatter();
            // Act
            var result10 = formatter10.FormatNode(mockNode10.Object); //"vcp_8d", (i) => FormatVCP_8D(i) ,"01": return "Mute the Mic";
            // Assert
            Assert.IsNotNull(result10);
            Assert.That(FormatVCP_8D01, Is.EqualTo(result10)); //result is not null

            Mock<INode> mockNode11 = new Mock<INode>();
            mockNode11.Setup(n => n.Value).Returns("02");
            Mock<INode> mockParent11 = new Mock<INode>();
            mockParent11.Setup(p => p.ToString()).Returns("vcp_aa");
            mockNode11.Setup(n => n.Parent).Returns(mockParent11.Object);
            string FormatVCP_AA02 = "90 degrees";
            var formatter11 = new NodeFormatter();
            // Act
            var result11 = formatter11.FormatNode(mockNode11.Object); //"vcp_aa", (i) => FormatVCP_AA(i) ,"02": return "90 degrees";
            // Assert
            Assert.IsNotNull(result11);
            Assert.That(FormatVCP_AA02, Is.EqualTo(result11)); //result is not null

            Mock<INode> mockNode12 = new Mock<INode>();
            mockNode12.Setup(n => n.Value).Returns("0f02");
            Mock<INode> mockParent12 = new Mock<INode>();
            mockParent12.Setup(p => p.ToString()).Returns("vcp_66");
            mockNode12.Setup(n => n.Parent).Returns(mockParent12.Object);
            string FormatVCP_660f02 = "ALS full function";
            var formatter12 = new NodeFormatter();
            // Act
            var result12 = formatter12.FormatNode(mockNode12.Object); //"vcp_66", (i) => FormatVCP_66(i),"0f02": return "ALS full function";;
            // Assert
            Assert.IsNotNull(result12);
            Assert.That(FormatVCP_660f02, Is.EqualTo(result12)); //result is not null
        }
    }
}