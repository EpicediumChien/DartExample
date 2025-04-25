
using DDPM.UI.Common;

namespace DDPM.UI.WalkThroughData
{
    public class WalkThroughData
    {
        public class WalkThroughPageData
        {
            public string? MainText { get; set; }
            public string? SubText { get; set; }
            public string? MainImageSource { get; set; }
        }
        public static Dictionary<string, List<WalkThroughPageData>> GetDevicePages(int themeVar)
        {
            var devicePages = new Dictionary<string, List<WalkThroughPageData>>
            {
                //APP////////////////////////////////////////////////////////////////////////////////////////////////OK
                // Consent Page
                {
                    "CONSENT_PAGE", new List<WalkThroughPageData>()
                    { 
                        new WalkThroughPageData()
                    }
                },
                // DDPM
                { "DDPM", new List<WalkThroughPageData>
                    {
                        new WalkThroughPageData { MainText = Strings.WalkThroughDDPM_Main0, SubText = Strings.WalkThroughDDPM_Sub0, MainImageSource = "WalkThrough/DDPM/DDPM-1-.png" },
                        new WalkThroughPageData { MainText = Strings.WalkThroughDDPM_Main1, SubText = Strings.WalkThroughDDPM_Sub1, MainImageSource = "WalkThrough/DDPM/DDPM2.png" },
                        new WalkThroughPageData { MainText = Strings.WalkThroughDDPM_Main2, SubText = Strings.WalkThroughDDPM_Sub2, MainImageSource = "WalkThrough/DDPM/DDPM3.png" },
                        new WalkThroughPageData { MainText = Strings.WalkThroughDDPM_Main3, SubText = Strings.WalkThroughDDPM_Sub3, MainImageSource = "WalkThrough/DDPM/DDPM4.png" },
                        new WalkThroughPageData { MainText = Strings.WalkThroughDDPM_Main4, SubText = Strings.WalkThroughDDPM_Sub4, MainImageSource = "WalkThrough/DDPM/DDPM5.png" },
                        new WalkThroughPageData { MainText = Strings.WalkThroughDDPM_Main5, SubText = Strings.WalkThroughDDPM_Sub5, MainImageSource = "WalkThrough/DDPM/DDPM6.png" },
                        new WalkThroughPageData { MainText = "", SubText = "", MainImageSource = "WalkThrough/DDPM/DDPM-7.png" }
                    }
                },
                //Display////////////////////////////////////////////////////////////////////////////////////////////////OK
                // S3425DW
                { "S3425DW", new List<WalkThroughPageData>
                    {
                        new WalkThroughPageData { MainText = Strings.WalkThroughDisplay_Main0, SubText = Strings.WalkThroughDisplay_Sub0, MainImageSource = "WalkThrough/Display/S3425DW/S3425DW_1.png" },
                        new WalkThroughPageData { MainText = Strings.WalkThroughDisplay_Main1, SubText = Strings.WalkThroughDisplay_Sub1, MainImageSource = "WalkThrough/Display/S3425DW/S3425DW_2.png" },
                        new WalkThroughPageData { MainText = Strings.WalkThroughDisplay_Main2, SubText = Strings.WalkThroughDisplay_Sub2, MainImageSource = "WalkThrough/Display/S3425DW/S3425DW_3.png" }
                    }
                },
                
                // S3225QC
                { "S3225QC", new List<WalkThroughPageData>
                    {
                        new WalkThroughPageData { MainText = Strings.WalkThroughDisplay_Main0, SubText = Strings.WalkThroughDisplay_Sub0, MainImageSource = "WalkThrough/Display/S3225QC/S3225QC_1.png" },
                        new WalkThroughPageData { MainText = Strings.WalkThroughDisplay_Main1, SubText = Strings.WalkThroughDisplay_Sub1, MainImageSource = "WalkThrough/Display/S3225QC/S3225QC_2.png" },
                        new WalkThroughPageData { MainText = Strings.WalkThroughDisplay_Main2, SubText = Strings.WalkThroughDisplay_Sub2, MainImageSource = "WalkThrough/Display/S3225QC/S3225QC_3.png" }
                    }
                },
                
                // S2725QC
                { "S2725QC", new List<WalkThroughPageData>
                    {
                        new WalkThroughPageData { MainText = Strings.WalkThroughDisplay_Main0, SubText = Strings.WalkThroughDisplay_Sub0, MainImageSource = "WalkThrough/Display/S2725QC/S2725QC_1.png" },
                        new WalkThroughPageData { MainText = Strings.WalkThroughDisplay_Main1, SubText = Strings.WalkThroughDisplay_Sub1, MainImageSource = "WalkThrough/Display/S2725QC/S2725QC_2.png" },
                        new WalkThroughPageData { MainText = Strings.WalkThroughDisplay_Main2, SubText = Strings.WalkThroughDisplay_Sub2, MainImageSource = "WalkThrough/Display/S2725QC/S2725QC_3.png" }
                    }
                },
                
                // U2725QE
                { "U2725QE", new List<WalkThroughPageData>
                    {
                        new WalkThroughPageData { MainText = Strings.WalkThroughDisplay_Main0, SubText = Strings.WalkThroughDisplay_Sub0, MainImageSource = "WalkThrough/Display/U2725QE/U2725QE_1.png" },
                        new WalkThroughPageData { MainText = Strings.WalkThroughDisplay_Main1, SubText = Strings.WalkThroughDisplay_Sub1, MainImageSource = "WalkThrough/Display/U2725QE/U2725QE_2.png" },
                        new WalkThroughPageData { MainText = Strings.WalkThroughDisplay_Main2, SubText = Strings.WalkThroughDisplay_Sub2, MainImageSource = "WalkThrough/Display/U2725QE/U2725QE_3.png" }
                    }
                },

                // U2724DE For Test
                //{ "U2724DE", new List<WalkThroughPageData>
                //    {
                //        new WalkThroughPageData { MainText = Strings.WalkThroughDisplay_Main0, SubText = Strings.WalkThroughDisplay_Sub0, MainImageSource = "WalkThrough/Display/U2725QE/U2725QE_1.png" },
                //        new WalkThroughPageData { MainText = Strings.WalkThroughDisplay_Main1, SubText = Strings.WalkThroughDisplay_Sub1, MainImageSource = "WalkThrough/Display/U2725QE/U2725QE_2.png" },
                //        new WalkThroughPageData { MainText = Strings.WalkThroughDisplay_Main2, SubText = Strings.WalkThroughDisplay_Sub2, MainImageSource = "WalkThrough/Display/U2725QE/U2725QE_3.png" }
                //    }
                //},

                // U2723DE For Test
                //{ "U2723DE", new List<WalkThroughPageData>
                //    {
                //        new WalkThroughPageData { MainText = Strings.WalkThroughDisplay_Main0, SubText = Strings.WalkThroughDisplay_Sub0, MainImageSource = "WalkThrough/Display/U2725QE/U2725QE_1.png" },
                //        new WalkThroughPageData { MainText = Strings.WalkThroughDisplay_Main1, SubText = Strings.WalkThroughDisplay_Sub1, MainImageSource = "WalkThrough/Display/U2725QE/U2725QE_2.png" },
                //        new WalkThroughPageData { MainText = Strings.WalkThroughDisplay_Main2, SubText = Strings.WalkThroughDisplay_Sub2, MainImageSource = "WalkThrough/Display/U2725QE/U2725QE_3.png" }
                //    }
                //},

                // E2422H For Test
                //{ "E2422H", new List<WalkThroughPageData>
                //    {
                //        new WalkThroughPageData { MainText = Strings.WalkThroughDisplay_Main0, SubText = Strings.WalkThroughDisplay_Sub0, MainImageSource = "WalkThrough/Display/U2725QE/U2725QE_1.png" },
                //        new WalkThroughPageData { MainText = Strings.WalkThroughDisplay_Main1, SubText = Strings.WalkThroughDisplay_Sub1, MainImageSource = "WalkThrough/Display/U2725QE/U2725QE_2.png" },
                //        new WalkThroughPageData { MainText = Strings.WalkThroughDisplay_Main2, SubText = Strings.WalkThroughDisplay_Sub2, MainImageSource = "WalkThrough/Display/U2725QE/U2725QE_3.png" }
                //    }
                //},

                // P3225DE For Test
                //{ "P3225DE", new List<WalkThroughPageData>
                //    {
                //        new WalkThroughPageData { MainText = Strings.WalkThroughDisplay_Main0, SubText = Strings.WalkThroughDisplay_Sub0, MainImageSource = "WalkThrough/Display/U2725QE/U2725QE_1.png" },
                //        new WalkThroughPageData { MainText = Strings.WalkThroughDisplay_Main1, SubText = Strings.WalkThroughDisplay_Sub1, MainImageSource = "WalkThrough/Display/U2725QE/U2725QE_2.png" },
                //        new WalkThroughPageData { MainText = Strings.WalkThroughDisplay_Main2, SubText = Strings.WalkThroughDisplay_Sub2, MainImageSource = "WalkThrough/Display/U2725QE/U2725QE_3.png" }
                //    }
                //},

                // U3225QE
                { "U3225QE", new List<WalkThroughPageData>
                    {
                        new WalkThroughPageData { MainText = Strings.WalkThroughDisplay_Main0, SubText = Strings.WalkThroughDisplay_Sub0, MainImageSource = "WalkThrough/Display/U3225QE/U3225QE_1.png" },
                        new WalkThroughPageData { MainText = Strings.WalkThroughDisplay_Main1, SubText = Strings.WalkThroughDisplay_Sub1, MainImageSource = "WalkThrough/Display/U3225QE/U3225QE_2.png" },
                        new WalkThroughPageData { MainText = Strings.WalkThroughDisplay_Main2, SubText = Strings.WalkThroughDisplay_Sub2, MainImageSource = "WalkThrough/Display/U3225QE/U3225QE_3.png" }
                    }
                },
                
                //KB////////////////////////////////////////////////////////////////////////////////////////////////
                // KB900 (Trident)
                { "KB900", new List<WalkThroughPageData>
                    {
                        new WalkThroughPageData { MainText = Strings.WalkThroughKB525C_Main1, SubText = Strings.WalkThroughKB525C_Sub1, MainImageSource = "WalkThrough/Keyboard/KB900/KB900_1.png" },
                        new WalkThroughPageData { MainText = Strings.WalkThroughKB525C_Main0, SubText = Strings.WalkThroughKB525C_Sub0, MainImageSource = "WalkThrough/Keyboard/KB900/KB900_2.png" },
                        new WalkThroughPageData { MainText = Strings.WalkThroughKB_Main2, SubText = Strings.WalkThroughKB_Sub2, MainImageSource = "WalkThrough/Keyboard/KB900/KB900_3.png" }
                    }
                },
                
                // KB555 (Elrond)
                { "KB555", new List<WalkThroughPageData>
                    {
                        new WalkThroughPageData { MainText = Strings.WalkThroughKB525C_Main1, SubText = Strings.WalkThroughKB525C_Sub1, MainImageSource = "WalkThrough/Keyboard/KB555/KB555_1.png" }
                    }
                },
                
                // KB525C (Frodo)
                { "KB525C", new List<WalkThroughPageData>
                    {
                        new WalkThroughPageData { MainText = Strings.WalkThroughKB525C_Main0, SubText = Strings.WalkThroughKB525C_Sub0, MainImageSource = "WalkThrough/Keyboard/KB525C/KB525C_1.png" },
                        new WalkThroughPageData { MainText = Strings.WalkThroughKB525C_Main1, SubText = Strings.WalkThroughKB525C_Sub1, MainImageSource = "WalkThrough/Keyboard/KB525C/KB525C_2.png" }
                    }
                },
                
                //Mouse////////////////////////////////////////////////////////////////////////////////////////////////
                // Orion (MS700)
                { "MS700", new List<WalkThroughPageData>
                    {
                        new WalkThroughPageData { MainText = Strings.WalkThroughMouse_Main0, SubText = Strings.WalkThroughMouse_Sub0, MainImageSource = "WalkThrough/Mouse/MS700/MS700_1.png" },
                        new WalkThroughPageData { MainText = Strings.WalkThroughMouse_Main1, SubText = Strings.WalkThroughMouse_Sub1, MainImageSource = "WalkThrough/Mouse/MS700/MS700_2.png" }
                    }
                },
                
                // Misty Blue (MS700/7)
                { "MS700/7", new List<WalkThroughPageData>
                    {
                        new WalkThroughPageData { MainText = Strings.WalkThroughMouse_Main0, SubText = Strings.WalkThroughMouse_Sub0, MainImageSource = "WalkThrough/Mouse/BMS700/BMS700_1.png" },
                        new WalkThroughPageData { MainText = Strings.WalkThroughMouse_Main1, SubText = Strings.WalkThroughMouse_Sub1, MainImageSource = "WalkThrough/Mouse/BMS700/BMS700_2.png" }
                    }
                },
                
                // Arwen (MS355)
                { "MS355", new List<WalkThroughPageData>
                    {
                        new WalkThroughPageData { MainText = Strings.WalkThroughMouseMS355_Main0, SubText = Strings.WalkThroughMouseMS355_Sub0, MainImageSource = "WalkThrough/Mouse/MS355/MS355_1.png" },
                        new WalkThroughPageData { MainText = Strings.WalkThroughMouseMS355_Main1, SubText = Strings.WalkThroughMouseMS355_Sub1, MainImageSource = "WalkThrough/Mouse/MS355/MS355_2.png" },
                        new WalkThroughPageData { MainText = Strings.WalkThroughMouseMS900_Main2, SubText = Strings.WalkThroughMouseMS355_Sub2, MainImageSource = "WalkThrough/Mouse/MS355/MS355_3.png" }
                    }
                },
                
                // Excalibur (MS900)
                { "MS900", new List<WalkThroughPageData>
                    {
                        new WalkThroughPageData { MainText = Strings.WalkThroughMouseMS900_Main0, SubText = Strings.WalkThroughMouseMS900_Sub0, MainImageSource = "WalkThrough/Mouse/MS900/MS900_1.png" },
                        new WalkThroughPageData { MainText = Strings.WalkThroughMouseMS900_Main2, SubText = Strings.WalkThroughMouseMS900_Sub1, MainImageSource = "WalkThrough/Mouse/MS900/MS900_2.png" }
                    }
                },
                
                //Pen////////////////////////////////////////////////////////////////////////////////////////////////OK
                // Millenio DVT2 (XPS Stylus)
                { "PN9315A", new List<WalkThroughPageData>
                    {
                        new WalkThroughPageData { MainText = Strings.WalkThroughPen_Main0, SubText = Strings.WalkThroughPen_Sub0, MainImageSource = "WalkThrough/Pen/Millenio/Millenio_1.png" },
                        new WalkThroughPageData { MainText = Strings.WalkThroughPen_Main1, SubText = Strings.WalkThroughPen_Sub1_1, MainImageSource = "WalkThrough/Pen/Millenio/Millenio_2.png" },
                        new WalkThroughPageData { MainText = Strings.WalkThroughPen_Main2, SubText = Strings.WalkThroughPen_Sub2, MainImageSource = "WalkThrough/Pen/Millenio/Millenio_3.png" }
                    }
                },
                
                // Caspian - Dell Premier Active Pen
                { "PN7522W", new List<WalkThroughPageData>
                    {
                        new WalkThroughPageData { MainText = Strings.WalkThroughPen_Main0, SubText = Strings.WalkThroughPen_Sub0, MainImageSource = "WalkThrough/Pen/Caspian/Caspian_1.png" },
                        new WalkThroughPageData { MainText = Strings.WalkThroughPen_Main1, SubText = Strings.WalkThroughPen_Sub1, MainImageSource = "WalkThrough/Pen/Caspian/Caspian_2.png" },
                        new WalkThroughPageData { MainText = Strings.WalkThroughPen_Main2, SubText = Strings.WalkThroughPen_Sub2, MainImageSource = "WalkThrough/Pen/Caspian/Caspian_3.png" }
                    }
                },
                
                //WebCam////////////////////////////////////////////////////////////////////////////////////////////////
                // Shephard (WB3023)
                { "WB3023", new List<WalkThroughPageData>
                    {
                        new WalkThroughPageData { MainText = Strings.WalkThroughWebCamWB3023_Main0, SubText = Strings.WalkThroughWebCamWB3023_Sub0, MainImageSource = "WalkThrough/Webcam/WB3023/WB3023_1.png" },
                        new WalkThroughPageData { MainText = Strings.WalkThroughWebCamWB3023_Main1, SubText = Strings.WalkThroughWebCamWB3023_Sub1, MainImageSource = "WalkThrough/Webcam/WB3023/WB3023_2.png" },
                        new WalkThroughPageData { MainText = Strings.WalkThroughWebCamWB3023_Main2, SubText = Strings.WalkThroughWebCamWB3023_Sub2, MainImageSource = "WalkThrough/Webcam/WB3023/WB3023_3.png" }
                    }
                },
                
                // Falcon (WB5023)
                { "WB5023", new List<WalkThroughPageData>
                    {
                        new WalkThroughPageData { MainText = Strings.WalkThroughWebCamWB3023_Main0, SubText = Strings.WalkThroughWebCamWB3023_Sub0, MainImageSource = "WalkThrough/Webcam/WB5023/WB5023_1.png" },
                        new WalkThroughPageData { MainText = Strings.WalkThroughWebCamWB3023_Main1, SubText = Strings.WalkThroughWebCamWB3023_Sub1, MainImageSource = "WalkThrough/Webcam/WB5023/WB5023_2.png" },
                        new WalkThroughPageData { MainText = Strings.WalkThroughWebCam_Main2, SubText = Strings.WalkThroughWebCam_Sub2, MainImageSource = "WalkThrough/Webcam/WB5023/WB5023_3.png" },
                        new WalkThroughPageData { MainText = Strings.WalkThroughWebCamWB3023_Main2, SubText = Strings.WalkThroughWebCam_Sub3, MainImageSource = "WalkThrough/Webcam/WB5023/WB5023_4.png" }
                    }
                },
                
                // Dell UltraSharp Webcam (WB7022)
                { "WB7022", new List<WalkThroughPageData>
                    {
                        new WalkThroughPageData { MainText = Strings.WalkThroughWebCamWB3023_Main0, SubText = Strings.WalkThroughWebCamWB3023_Sub0, MainImageSource = "WalkThrough/Webcam/WB7022/WB7022_1.png" },
                        new WalkThroughPageData { MainText = Strings.WalkThroughWebCamWB3023_Main1, SubText = Strings.WalkThroughWebCamWB3023_Sub1, MainImageSource = "WalkThrough/Webcam/WB7022/WB7022_2.png" },
                        new WalkThroughPageData { MainText = Strings.WalkThroughWebCam_Main2, SubText = Strings.WalkThroughWebCam_Sub2, MainImageSource = "WalkThrough/Webcam/WB7022/WB7022_3.png" },
                        new WalkThroughPageData { MainText = Strings.WalkThroughWebCamWB3023_Main2, SubText = Strings.WalkThroughWebCam_Sub3, MainImageSource = "WalkThrough/Webcam/WB7022/WB7022_4.png" },
                        new WalkThroughPageData { MainText = Strings.WalkThroughWebCam_Main4, SubText = Strings.WalkThroughWebCam_Sub4, MainImageSource = "WalkThrough/Webcam/WB7022/WB7022_5.png" }
                    }
                },
                
                // Dell Conferencing Monitor (P2424HEB)
                { "P2424HEB", new List<WalkThroughPageData>
                    {
                        new WalkThroughPageData { MainText = Strings.WalkThroughWebCamWB3023_Main0, SubText = Strings.WalkThroughWebCamWB3023_Sub0, MainImageSource = "WalkThrough/Webcam/P2424HEB/P2424HEB_1.png" },
                        new WalkThroughPageData { MainText = Strings.WalkThroughWebCamWB3023_Main1, SubText = Strings.WalkThroughWebCamWB3023_Sub1, MainImageSource = "WalkThrough/Webcam/P2424HEB/P2424HEB_2.png" },
                        new WalkThroughPageData { MainText = Strings.WalkThroughWebCam_Main2, SubText = Strings.WalkThroughWebCam_Sub2, MainImageSource = "WalkThrough/Webcam/P2424HEB/P2424HEB_3.png" },
                        new WalkThroughPageData { MainText = Strings.WalkThroughWebCamWB3023_Main2, SubText = Strings.WalkThroughWebCam_Sub3, MainImageSource = "WalkThrough/Webcam/P2424HEB/P2424HEB_4.png" },
                        new WalkThroughPageData { MainText = Strings.WalkThroughWebCam_Main4, SubText = Strings.WalkThroughWebCam_Sub4, MainImageSource = "WalkThrough/Webcam/P2424HEB/P2424HEB_5.png" }
                    }
                },
                
                // Dell Conferencing Monitor (P2724DEB)
                { "P2724DEB", new List<WalkThroughPageData>
                    {
                        new WalkThroughPageData { MainText = Strings.WalkThroughWebCamWB3023_Main0, SubText = Strings.WalkThroughWebCamWB3023_Sub0, MainImageSource = "WalkThrough/Webcam/P2724DEB/P2724DEB_1.png" },
                        new WalkThroughPageData { MainText = Strings.WalkThroughWebCamWB3023_Main1, SubText = Strings.WalkThroughWebCamWB3023_Sub1, MainImageSource = "WalkThrough/Webcam/P2724DEB/P2724DEB_2.png" },
                        new WalkThroughPageData { MainText = Strings.WalkThroughWebCam_Main2, SubText = Strings.WalkThroughWebCam_Sub2, MainImageSource = "WalkThrough/Webcam/P2724DEB/P2724DEB_3.png" },
                        new WalkThroughPageData { MainText = Strings.WalkThroughWebCamWB3023_Main2, SubText = Strings.WalkThroughWebCam_Sub3, MainImageSource = "WalkThrough/Webcam/P2724DEB/P2724DEB_4.png" },
                        new WalkThroughPageData { MainText = Strings.WalkThroughWebCam_Main4, SubText = Strings.WalkThroughWebCam_Sub4, MainImageSource = "WalkThrough/Webcam/P2724DEB/P2724DEB_5.png" }
                    }
                },
                
                // Dell Conferencing Monitor (P3424WEB)
                { "P3424WEB", new List<WalkThroughPageData>
                    {
                        new WalkThroughPageData { MainText = Strings.WalkThroughWebCamWB3023_Main0, SubText = Strings.WalkThroughWebCamWB3023_Sub0, MainImageSource = "WalkThrough/Webcam/P3424WEB/P3424WEB_1.png" },
                        new WalkThroughPageData { MainText = Strings.WalkThroughWebCamWB3023_Main1, SubText = Strings.WalkThroughWebCamWB3023_Sub1, MainImageSource = "WalkThrough/Webcam/P3424WEB/P3424WEB_2.png" },
                        new WalkThroughPageData { MainText = Strings.WalkThroughWebCam_Main2, SubText = Strings.WalkThroughWebCam_Sub2, MainImageSource = "WalkThrough/Webcam/P3424WEB/P3424WEB_3.png" },
                        new WalkThroughPageData { MainText = Strings.WalkThroughWebCamWB3023_Main2, SubText = Strings.WalkThroughWebCam_Sub3, MainImageSource = "WalkThrough/Webcam/P3424WEB/P3424WEB_4.png" },
                        new WalkThroughPageData { MainText = Strings.WalkThroughWebCam_Main4, SubText = Strings.WalkThroughWebCam_Sub4, MainImageSource = "WalkThrough/Webcam/P3424WEB/P3424WEB_5.png" }
                    }
                },
                
                // Dell Conferencing Monitor (U3223QZ)
                { "U3223QZ", new List<WalkThroughPageData>
                    {
                        new WalkThroughPageData { MainText = Strings.WalkThroughWebCamWB3023_Main0, SubText = Strings.WalkThroughWebCamWB3023_Sub0, MainImageSource = "WalkThrough/Webcam/U3223QZ/U3223QZ_1.png" },
                        new WalkThroughPageData { MainText = Strings.WalkThroughWebCamWB3023_Main1, SubText = Strings.WalkThroughWebCamWB3023_Sub1, MainImageSource = "WalkThrough/Webcam/U3223QZ/U3223QZ_2.png" },
                        new WalkThroughPageData { MainText = Strings.WalkThroughWebCam_Main2, SubText = Strings.WalkThroughWebCam_Sub2, MainImageSource = "WalkThrough/Webcam/U3223QZ/U3223QZ_3.png" },
                        new WalkThroughPageData { MainText = Strings.WalkThroughWebCamWB3023_Main2, SubText = Strings.WalkThroughWebCam_Sub3, MainImageSource = "WalkThrough/Webcam/U3223QZ/U3223QZ_4.png" },
                        new WalkThroughPageData { MainText = Strings.WalkThroughWebCam_Main4, SubText = Strings.WalkThroughWebCam_Sub4, MainImageSource = "WalkThrough/Webcam/U3223QZ/U3223QZ_5.png" }
                    }
                },
                
                // Dell Conferencing Monitor (U3224KB/A)
                { "U3224KB", new List<WalkThroughPageData>
                    {
                        new WalkThroughPageData { MainText = Strings.WalkThroughWebCamWB3023_Main0, SubText = Strings.WalkThroughWebCamWB3023_Sub0, MainImageSource = "WalkThrough/Webcam/U3224KB/U3224KB_1.png" },
                        new WalkThroughPageData { MainText = Strings.WalkThroughWebCamWB3023_Main1, SubText = Strings.WalkThroughWebCamWB3023_Sub1, MainImageSource = "WalkThrough/Webcam/U3224KB/U3224KB_2.png" },
                        new WalkThroughPageData { MainText = Strings.WalkThroughWebCam_Main2, SubText = Strings.WalkThroughWebCam_Sub2, MainImageSource = "WalkThrough/Webcam/U3224KB/U3224KB_3.png" },
                        new WalkThroughPageData { MainText = Strings.WalkThroughWebCamWB3023_Main2, SubText = Strings.WalkThroughWebCam_Sub3, MainImageSource = "WalkThrough/Webcam/U3224KB/U3224KB_4.png" },
                        new WalkThroughPageData { MainText = Strings.WalkThroughWebCam_Main4, SubText = Strings.WalkThroughWebCam_Sub4, MainImageSource = "WalkThrough/Webcam/U3224KB/U3224KB_5.png" }
                    }
                },
                { "U3224KBA", new List<WalkThroughPageData>
                    {
                        new WalkThroughPageData { MainText = Strings.WalkThroughWebCamWB3023_Main0, SubText = Strings.WalkThroughWebCamWB3023_Sub0, MainImageSource = "WalkThrough/Webcam/U3224KBA/U3224KBA_1.png" },
                        new WalkThroughPageData { MainText = Strings.WalkThroughWebCamWB3023_Main1, SubText = Strings.WalkThroughWebCamWB3023_Sub1, MainImageSource = "WalkThrough/Webcam/U3224KBA/U3224KBA_2.png" },
                        new WalkThroughPageData { MainText = Strings.WalkThroughWebCam_Main2, SubText = Strings.WalkThroughWebCam_Sub2, MainImageSource = "WalkThrough/Webcam/U3224KBA/U3224KBA_3.png" },
                        new WalkThroughPageData { MainText = Strings.WalkThroughWebCamWB3023_Main2, SubText = Strings.WalkThroughWebCam_Sub3, MainImageSource = "WalkThrough/Webcam/U3224KBA/U3224KBA_4.png" },
                        new WalkThroughPageData { MainText = Strings.WalkThroughWebCam_Main4, SubText = Strings.WalkThroughWebCam_Sub4, MainImageSource = "WalkThrough/Webcam/U3224KBA/U3224KBA_5.png" }
                    }
                },
                { "U3224KB/A", new List<WalkThroughPageData>
                    {
                        new WalkThroughPageData { MainText = Strings.WalkThroughWebCamWB3023_Main0, SubText = Strings.WalkThroughWebCamWB3023_Sub0, MainImageSource = "WalkThrough/Webcam/U3224KBA/U3224KBA_1.png" },
                        new WalkThroughPageData { MainText = Strings.WalkThroughWebCamWB3023_Main1, SubText = Strings.WalkThroughWebCamWB3023_Sub1, MainImageSource = "WalkThrough/Webcam/U3224KBA/U3224KBA_2.png" },
                        new WalkThroughPageData { MainText = Strings.WalkThroughWebCam_Main2, SubText = Strings.WalkThroughWebCam_Sub2, MainImageSource = "WalkThrough/Webcam/U3224KBA/U3224KBA_3.png" },
                        new WalkThroughPageData { MainText = Strings.WalkThroughWebCamWB3023_Main2, SubText = Strings.WalkThroughWebCam_Sub3, MainImageSource = "WalkThrough/Webcam/U3224KBA/U3224KBA_4.png" },
                        new WalkThroughPageData { MainText = Strings.WalkThroughWebCam_Main4, SubText = Strings.WalkThroughWebCam_Sub4, MainImageSource = "WalkThrough/Webcam/U3224KBA/U3224KBA_5.png" }
                    }
                },
                //Headset////////////////////////////////////////////////////////////////////////////////////////////////
                // Vaporfly (WL3024)
                { "WL3024", new List<WalkThroughPageData>
                    {
                        new WalkThroughPageData { MainText = Strings.WalkThroughHeadsetWH5024_Main1, SubText = Strings.WalkThroughHeadsetWH5024_Sub1, MainImageSource = "WalkThrough/Headset/WL3024/WL3024_1.png" },
                        new WalkThroughPageData { MainText = Strings.WalkThroughHeadsetWL7024_Main1, SubText = Strings.WalkThroughHeadsetWL5024_Sub1, MainImageSource = "WalkThrough/Headset/WL3024/WL3024_2.png" }
                    }
                },
                
                // Airmax (WH3024)
                { "WH3024", new List<WalkThroughPageData>
                    {
                        new WalkThroughPageData { MainText = Strings.WalkThroughHeadsetWL7024_Main1, SubText = Strings.WalkThroughHeadsetWH3024_Sub0, MainImageSource = "WalkThrough/Headset/WH3024/WH3024_1.png" },
                        new WalkThroughPageData { MainText = Strings.WalkThroughHeadsetWH5024_Main1, SubText = Strings.WalkThroughHeadsetWH5024_Sub1, MainImageSource = "WalkThrough/Headset/WH3024/WH3024_2.png" }
                    }
                },
                
                // Pegasus (WL5024)
                { "WL5024", new List<WalkThroughPageData>
                    {
                        new WalkThroughPageData { MainText = Strings.WalkThroughHeadsetWH5024_Main1, SubText = Strings.WalkThroughHeadsetWH5024_Sub1, MainImageSource = "WalkThrough/Headset/WL5024/WL5024_1.png" },
                        new WalkThroughPageData { MainText = Strings.WalkThroughHeadsetWL7024_Main1, SubText = Strings.WalkThroughHeadsetWL5024_Sub1, MainImageSource = "WalkThrough/Headset/WL5024/WL5024_2.png" },
                        new WalkThroughPageData { MainText = Strings.WalkThroughHeadsetWL7024_Main2, SubText = Strings.WalkThroughHeadsetWL7024_Sub2, MainImageSource = "WalkThrough/Headset/WL5024/WL5024_3.png" }
                    }
                },
                
                // Winflo (WH5024)
                { "WH5024", new List<WalkThroughPageData>
                    {
                        new WalkThroughPageData { MainText = Strings.WalkThroughHeadsetWL7024_Main1, SubText = Strings.WalkThroughHeadsetWH5024_Sub0, MainImageSource = "WalkThrough/Headset/WH5024/WH5024_1.png" },
                        new WalkThroughPageData { MainText = Strings.WalkThroughHeadsetWH5024_Main1, SubText = Strings.WalkThroughHeadsetWH5024_Sub1, MainImageSource = "WalkThrough/Headset/WH5024/WH5024_2.png" },
                        new WalkThroughPageData { MainText = Strings.WalkThroughHeadsetWL7024_Main2, SubText = Strings.WalkThroughHeadsetWL7024_Sub2, MainImageSource = "WalkThrough/Headset/WH5024/WH5024_3.png" }
                    }
                },
                
                // Mito (WL7024)
                { "WL7024", new List<WalkThroughPageData>
                    {
                        new WalkThroughPageData { MainText = Strings.WalkThroughHeadsetWL7024_Main0, SubText = Strings.WalkThroughHeadsetWL7024_Sub0, MainImageSource = "WalkThrough/Headset/WL7024/WL7024_1.png" },
                        new WalkThroughPageData { MainText = Strings.WalkThroughHeadsetWL7024_Main1, SubText = Strings.WalkThroughHeadsetWL7024_Sub1, MainImageSource = "WalkThrough/Headset/WL7024/WL7024_2.png" },
                        new WalkThroughPageData { MainText = Strings.WalkThroughHeadsetWL7024_Main2, SubText = Strings.WalkThroughHeadsetWL7024_Sub2, MainImageSource = "WalkThrough/Headset/WL7024/WL7024_3.png" },
                        new WalkThroughPageData { MainText = Strings.WalkThroughHeadsetWL7024_Main3, SubText = Strings.WalkThroughHeadsetWL7024_Sub3, MainImageSource = "WalkThrough/Headset/WL7024/WL7024_4.png" }
                    }
                },

                // Air Audio (SB725)
                { "SB725", new List<WalkThroughPageData>
                    {
                        new WalkThroughPageData { MainText = Strings.WalkThroughAirAudio_Main0, SubText = Strings.WalkThroughAirAudio_Sub0, MainImageSource = "WalkThrough/AirAudio/SB725/SB725_1.png" },
                        new WalkThroughPageData { MainText = Strings.WalkThroughAirAudio_Main1, SubText = Strings.WalkThroughAirAudio_Sub1, MainImageSource = "WalkThrough/AirAudio/SB725/SB725_2.png" }
                    }
                },

                // Air Audio (SL525)
                { "SL525", new List<WalkThroughPageData>
                    {
                        new WalkThroughPageData { MainText = Strings.WalkThroughAirAudio_Main0, SubText = Strings.WalkThroughAirAudio_Sub0, MainImageSource = "WalkThrough/AirAudio/SL525/SL525_1.png" },
                        new WalkThroughPageData { MainText = Strings.WalkThroughAirAudio_Main1, SubText = Strings.WalkThroughAirAudio_Sub1, MainImageSource = "WalkThrough/AirAudio/SL525/SL525_2.png" },
                        new WalkThroughPageData { MainText = Strings.WalkThroughAirAudio_Main2, SubText = Strings.WalkThroughAirAudio_Sub2, MainImageSource = "WalkThrough/AirAudio/SL525/SL525_3.png" }
                    }
                },

                 // Air Audio (SP325)
                { "SP325", new List<WalkThroughPageData>
                    {
                        new WalkThroughPageData { MainText = Strings.WalkThroughAirAudio_Main0, SubText = Strings.WalkThroughAirAudio_Sub0, MainImageSource = "WalkThrough/AirAudio/SP325/SP325_1.png" },
                        new WalkThroughPageData { MainText = Strings.WalkThroughAirAudio_Main1, SubText = Strings.WalkThroughAirAudio_Sub1, MainImageSource = "WalkThrough/AirAudio/SP325/SP325_2.png" }
                    }
                }
            };
            if (themeVar != 1)
            {
                foreach (var pageList in devicePages.Values)
                {
                    foreach (var pageData in pageList)
                    {
                        if (!string.IsNullOrEmpty(pageData.MainImageSource))
                        {
                            // 將 圖片路徑改為 "xxx/xxx/Light_Mode/xxx/xxx.png"
                            var segments = pageData.MainImageSource.Split('/');
                            if (segments.Length > 2)
                            {
                                // 插入 "Light_Mode" 到第三個位置
                                var newSegments = segments.ToList();
                                newSegments.Insert(2, "Light_Mode");
                                pageData.MainImageSource = string.Join("/", newSegments);
                            }
                        }
                    }
                }
            }

            return devicePages;
        }
    }
}
