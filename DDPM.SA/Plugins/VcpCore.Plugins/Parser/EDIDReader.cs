using System;

namespace VcpCore.Plugins
{
    public class EDIDReader
    {
        public static string Information(byte[] EDID, string newline = "\r\n")
        {
            string displaySize = Display_Parameters.Max_Display_Size_CH(EDID);
            string manufacturerName = Vendor_Product_Identification.Manufacturer_Name(EDID);
            string year = Vendor_Product_Identification.Year_Of_Manufacture(EDID);
            string week = Vendor_Product_Identification.Week_Of_Manufacture(EDID);
            string Image_Size_Ratio = Preferred_Detailed_Timing.Active_Ratio(EDID);
            string Max_Horizontal_Image_Size = Display_Parameters.Max_Horizontal_Image_Size(EDID);
            string Max_Vertical_Image_Size = Display_Parameters.Max_Vertical_Image_Size(EDID);

            return string.Format("{2}年{3}周; {0}; {1}({6},{7}); {4};{5}", manufacturerName/*ModifyManufacturer(manufacturerName)*/, displaySize, year, week, Image_Size_Ratio, newline, Max_Horizontal_Image_Size, Max_Vertical_Image_Size);
        }

        /// <summary>
        /// 最大公约数
        /// </summary>
        /// <param name="num1"></param>
        /// <param name="num2"></param>
        /// <returns></returns>
        public static int MaximumCommonDivisor(int num1, int num2)
        {
            int tmp;
            if (num1 < num2)
            {
                tmp = num1; num1 = num2; num2 = tmp;
            }
            int a = num1; int b = num2;
            while (b != 0)
            {
                tmp = a % b;
                a = b;
                b = tmp;
            }
            return a;
            //Console.WriteLine("{0}和{1}的最大公约数为：{2}", num1, num2, a);
            // Console.WriteLine("{0}和{1}的最小公倍数为：{2}", num1, num2, num1 * num2 / a);
        }

        /// <summary>
        /// 计算比例
        /// </summary>
        /// <param name="num1"></param>
        /// <param name="num2"></param>
        /// <returns></returns>
        public static string Ratio(int num1, int num2)
        {
            int X = MaximumCommonDivisor(num1, num2);
            return string.Format("{0}:{1}", num1 / X, num2 / X); ;
            //Console.WriteLine("{0}和{1}的最大公约数为：{2}", num1, num2, a);
            // Console.WriteLine("{0}和{1}的最小公倍数为：{2}", num1, num2, num1 * num2 / a);
        }

        public static char ToCharByASCIIShort(int a)
        {
            return (char)(a + 64);
        }

        public static bool Contains(byte a, byte b)
        {
            if ((a & b) == b)
            {
                return true;
            }
            return false;
        }

        public static double ToCinch_By_ABcm(int a, int b)
        {
            double A = a;
            double B = b;
            double C = Math.Sqrt(a * a + b * b) / 2.54d;
            return C;
        }

        #region Vendor/Product Identification:

        public class Vendor_Product_Identification
        {
            public static string Monitor_Name(byte[] EDID)
            { return ""; }

            public static string Monitor_Serial_Number(byte[] EDID)
            { return ""; } //

            /// <summary>
            /// 大端
            /// </summary>
            /// <param name="EDID"></param>
            /// <returns></returns>
            public static string Manufacturer_Name(byte[] EDID)
            {
                if (EDID == null || EDID.Length < 128) return "";

                int ManufacturerInt = ((int)EDID[8] << 8) | ((int)EDID[9]);
                int a = (ManufacturerInt & 0x00007C00) >> 10;
                int b = (ManufacturerInt & 0x000003E0) >> 5;
                int c = (ManufacturerInt & 0x0000001F);
                string A = "" + ToCharByASCIIShort(a);
                string B = "" + ToCharByASCIIShort(b);
                string C = "" + ToCharByASCIIShort(c);
                return A + B + C;
            }

            public static string Manufacturer_Name(byte byte8, byte byte9)
            {
                int ManufacturerInt = ((int)byte8 << 8) | ((int)byte9);
                int a = (ManufacturerInt & 0x00007C00) >> 10;
                int b = (ManufacturerInt & 0x000003E0) >> 5;
                int c = (ManufacturerInt & 0x0000001F);
                string A = "" + ToCharByASCIIShort(a);
                string B = "" + ToCharByASCIIShort(b);
                string C = "" + ToCharByASCIIShort(c);
                return A + B + C;
            }

            // 	LGD
            /// <summary>
            /// 小端
            /// </summary>
            /// <param name="EDID"></param>
            /// <returns></returns>
            public static string Product_Id(byte[] EDID)
            {
                if (EDID == null || EDID.Length < 128) return "";
                int ManufacturerInt = ((int)EDID[11] << 8) | ((int)EDID[10]);
                return ManufacturerInt.ToString("X");
            }

            /// <summary>
            /// 小端
            /// </summary>
            /// <param name="EDID"></param>
            /// <returns></returns>
            public static string Serial_Number(byte[] EDID)
            {
                if (EDID == null || EDID.Length < 128) return "";
                int ManufacturerInt = ((int)EDID[15] << 24) | (int)EDID[14] << 16 | (int)EDID[13] << 8 | ((int)EDID[12]);
                return ManufacturerInt.ToString("X");
            } // 0

            public static string Week_Of_Manufacture(byte[] EDID)
            {
                if (EDID == null || EDID.Length < 128) return "";

                return EDID[16].ToString();
            }

            public static string Year_Of_Manufacture(byte[] EDID)
            {
                if (EDID == null || EDID.Length < 128) return "";

                return (EDID[17] + 1990).ToString();
            }

            public static string EDIDVersion(byte[] EDID)
            {
                if (EDID == null || EDID.Length < 128) return "";
                return "V" + (EDID[18]).ToString() + "." + (EDID[19]).ToString();
            }

            public static string Number_Of_Extension_Flag(byte[] EDID)
            {
                if (EDID == null || EDID.Length < 256) return "0";

                return (EDID.Length / 128 - 1).ToString();
            }
        }

        #endregion Vendor/Product Identification:

        #region Display parameters:

        public class Display_Parameters
        {
            public static string Video_Input_Definition(byte[] EDID)
            {
                if (EDID == null || EDID.Length < 128) return "";

                if ((EDID[20] & 0x80) == 0x80)
                {
                    return "Digital Signal";
                }
                else
                {
                    return "Analog Signal";
                }
            }

            public static string DFP1X_Compatible_Interface(byte[] EDID)
            {
                if (EDID == null || EDID.Length < 128) return "";

                if ((EDID[20] & 0x80) == 0x80)
                {
                    if ((EDID[20] & 0x01) == 0x01)
                    {
                        return "True";
                    }
                    else
                    {
                        return "Flase";
                    }
                }
                else
                {
                    return "Invalid";
                }
            }

            public static string Video_White_and_Sync_Levels(byte[] EDID)
            {
                if (EDID == null || EDID.Length < 128) return "";

                if ((EDID[20] & 0x80) == 0x80)
                {
                    return "Invalid";
                }
                else
                {
                    if ((EDID[20] & 0x60) == 0x60)//11
                    {
                        return "+0.7/0 V";
                    }
                    else if ((EDID[20] & 0x40) == 0x40)//10
                    {
                        return "+1.0/−0.4 V";
                    }
                    else if ((EDID[20] & 0x20) == 0x20)//01
                    {
                        return "+0.714/−0.286 V";
                    }
                    else
                    {
                        return "+0.7/−0.3 V";
                    }
                }
            }

            public static string Blank_To_Black_Setup(byte[] EDID)
            {
                if (EDID == null || EDID.Length < 128) return "";

                if ((EDID[20] & 0x80) == 0x80)
                {
                    return "Invalid";
                }
                else
                {
                    if ((EDID[20] & 0x10) == 0x60)
                    {
                        return "True";
                    }
                    else
                    {
                        return "False";
                    }
                }
            }

            public static string Separate_Sync(byte[] EDID)
            {
                if (EDID == null || EDID.Length < 128) return "";

                if ((EDID[20] & 0x80) == 0x80)
                {
                    return "Invalid";
                }
                else
                {
                    if ((EDID[20] & 0x08) == 0x08)
                    {
                        return "True";
                    }
                    else
                    {
                        return "False";
                    }
                }
            }

            public static string HSync_Composite_Sync(byte[] EDID)
            {
                if (EDID == null || EDID.Length < 128) return "";

                if ((EDID[20] & 0x80) == 0x80)
                {
                    return "Invalid";
                }
                else
                {
                    if ((EDID[20] & 0x04) == 0x04)
                    {
                        return "True";
                    }
                    else
                    {
                        return "False";
                    }
                }
            }

            /// <summary>
            /// Sync-on-green
            /// </summary>
            /// <param name="EDID"></param>
            /// <returns></returns>
            public static string SOG(byte[] EDID)
            {
                if (EDID == null || EDID.Length < 128) return "";

                if ((EDID[20] & 0x80) == 0x80)
                {
                    return "Invalid";
                }
                else
                {
                    if ((EDID[20] & 0x02) == 0x02)
                    {
                        return "True";
                    }
                    else
                    {
                        return "False";
                    }
                }
            }

            public static string VSync_Pulse_Must_Be_Serrated(byte[] EDID)
            {
                if (EDID == null || EDID.Length < 128) return "";

                if ((EDID[20] & 0x80) == 0x80)
                {
                    return "Invalid";
                }
                else
                {
                    if (((EDID[20] & 0x02) == 0x02) || ((EDID[20] & 0x04) == 0x04))
                    {
                        if ((EDID[20] & 0x01) == 0x01)
                        {
                            return "True";
                        }
                        else
                        {
                            return "False";
                        }
                    }
                    else
                    {
                        return "Invalid. VSync pulse must be serrated when composite or sync-on-green is used.";
                    }
                }
            }

            public static string Max_Horizontal_Image_Size(byte[] EDID)
            {
                if (EDID == null || EDID.Length < 128) return "";

                int w = EDID[21] * 10;
                return w + " mm";
            }

            public static string Max_Vertical_Image_Size(byte[] EDID)
            {
                if (EDID == null || EDID.Length < 128) return "";

                int h = EDID[22] * 10;
                return h + " mm";
            }

            public static string Image_Size_Ratio(byte[] EDID)
            {
                if (EDID == null || EDID.Length < 128) return "";

                int H = EDID[21] * 10;
                int V = EDID[22] * 10;
                return Ratio(H, V);
            }

            public static string Max_Display_Size(byte[] EDID)
            {
                if (EDID == null || EDID.Length < 128) return "";
                return ToCinch_By_ABcm(EDID[21], EDID[22]).ToString("00.0") + " inches";
            }

            public static string Max_Display_Size_CH(byte[] EDID)
            {
                if (EDID == null || EDID.Length < 128) return "";
                return ToCinch_By_ABcm(EDID[21], EDID[22]).ToString("00.0") + "(寸)";
            }
        }

        #endregion Display parameters:

        #region Power Management and Features：

        public class Power_Management_and_Features
        {
            public static string Standby(byte[] EDID)
            {
                if ((EDID[24] & 0x80) == 0x80)
                {
                    return "Supported";
                }
                else
                {
                    return "Not Supported";
                }
            }

            public static string Suspend(byte[] EDID)
            {
                if ((EDID[24] & 0x40) == 0x40)
                {
                    return "Supported";
                }
                else
                {
                    return "Not Supported";
                }
            }

            public static string ActiveOff(byte[] EDID)
            {
                if ((EDID[24] & 0x20) == 0x20)
                {
                    return "Supported";
                }
                else
                {
                    return "Not Supported";
                }
            }

            public static string Video_Input_Display_Type(byte[] EDID)
            {
                if (EDID == null || EDID.Length < 128) return "";

                int a = (EDID[24] >> 3 & 0x03);
                return a.ToString();
            }

            public static string Video_Input(byte[] EDID)
            {
                if (EDID == null || EDID.Length < 128) return "";

                int a = (EDID[24] >> 7 & 0x01);
                return a.ToString();
            }

            public static string sRGB_Default_ColorSpace(byte[] EDID)
            {
                if ((EDID[24] & 0x04) == 0x04)
                {
                    return "True";
                }
                else
                {
                    return "False";
                }
            }

            public static string Default_GTF(byte[] EDID)
            {
                if ((EDID[24] & 0x02) == 0x02)
                {
                    return "Not Supported";
                }
                else
                {
                    return "Supported";
                }
            }

            public static string Prefered_Timing_Mode(byte[] EDID)
            {
                if ((EDID[24] & 0x02) == 0x02)
                {
                    return "True";
                }
                else
                {
                    return "False";
                }
            }
        }

        #endregion Power Management and Features：

        #region Gamma/Color and Etablished Timings：

        public class Gamma_Color_and_Etablished_Timings
        {
            public static string Display_Gamma(byte[] EDID)
            {
                if (EDID == null || EDID.Length < 128) return "";
                double gamma = (double)EDID[23] / 100.0d + 1.0;
                return gamma.ToString("0.00");
            }

            //  public static string Display_Gamma(byte[] EDID) { return ""; }// 	2.2

            public static string Red(byte[] EDID)
            {
                if (EDID == null || EDID.Length < 128) return "";
                int lsbitx = (EDID[25] >> 6) & 0x0003;
                int lsbity = (EDID[25] >> 4) & 0x0003;
                int msbitx = EDID[27] << 2;
                int msbity = EDID[28] << 2;
                double x = (lsbitx | msbitx) / 1024d;
                double y = (lsbity | msbity) / 1024d;
                return string.Format("(x,y)({0},{1})", x.ToString("0.0000"), y.ToString("0.0000"));
            }// 	x = 0.58 - y = 0.35

            public static string Green(byte[] EDID)
            {
                if (EDID == null || EDID.Length < 128) return "";
                int lsbitx = (EDID[25] >> 2) & 0x0003;
                int lsbity = EDID[25] & 0x0003;
                int msbitx = EDID[29] << 2;
                int msbity = EDID[30] << 2;
                double x = (lsbitx | msbitx) / 1024d;
                double y = (lsbity | msbity) / 1024d;
                return string.Format("(x,y)({0},{1})", x.ToString("0.0000"), y.ToString("0.0000"));
            }

            public static string Blue(byte[] EDID)
            {
                if (EDID == null || EDID.Length < 128) return "";
                int lsbitx = (EDID[25] >> 6) & 0x0003;
                int lsbity = (EDID[26] >> 4) & 0x0003;
                int msbitx = EDID[31] << 2;
                int msbity = EDID[32] << 2;
                double x = (lsbitx | msbitx) / 1024d;
                double y = (lsbity | msbity) / 1024d;
                return string.Format("(x,y)({0},{1})", x.ToString("0.0000"), y.ToString("0.0000"));
            }// 	x = 0.155 - y = 0.125

            public static string White(byte[] EDID)
            {
                if (EDID == null || EDID.Length < 128) return "";
                int lsbitx = (EDID[25] >> 2) & 0x0003;
                int lsbity = EDID[26] & 0x0003;
                int msbitx = EDID[33] << 2;
                int msbity = EDID[34] << 2;
                double x = (lsbitx | msbitx) / 1024d;
                double y = (lsbity | msbity) / 1024d;
                return string.Format("(x,y)({0},{1})", x.ToString("0.0000"), y.ToString("0.0000"));
            }// 	x = 0.313 - y = 0.329

            public static string Etablished_Timings(byte[] EDID)
            {
                if (EDID == null || EDID.Length < 128) return "";

                string supports = "\r\n";
                if (Contains(EDID[35], 0x80)) supports += "\t\t720×400 @ 70 Hz\r\n";
                if (Contains(EDID[35], 0x40)) supports += "\t\t720×400 @ 88 Hz\r\n";
                if (Contains(EDID[35], 0x20)) supports += "\t\t640×480 @ 60 Hz\r\n";
                if (Contains(EDID[35], 0x10)) supports += "\t\t640×480 @ 67 Hz\r\n";
                if (Contains(EDID[35], 0x08)) supports += "\t\t640×480 @ 72 Hz\r\n";
                if (Contains(EDID[35], 0x04)) supports += "\t\t640×480 @ 75 Hz\r\n";
                if (Contains(EDID[35], 0x02)) supports += "\t\t800×600 @ 56 Hz\r\n";
                if (Contains(EDID[35], 0x01)) supports += "\t\t800×600 @ 60 Hz\r\n";

                if (Contains(EDID[36], 0x80)) supports += "\t\t800×600 @ 72 Hz\r\n";
                if (Contains(EDID[36], 0x40)) supports += "\t\t800×600 @ 75 Hz\r\n";
                if (Contains(EDID[36], 0x20)) supports += "\t\t832×624 @ 75 Hz\r\n";
                if (Contains(EDID[36], 0x10)) supports += "\t\t1024×768 @ 87 Hz, interlaced (1024×768i)\r\n";
                if (Contains(EDID[36], 0x08)) supports += "\t\t1024×768 @ 60 Hz\r\n";
                if (Contains(EDID[36], 0x04)) supports += "\t\t1024×768 @ 72 Hz\r\n";
                if (Contains(EDID[36], 0x02)) supports += "\t\t1024×768 @ 75 Hz\r\n";
                if (Contains(EDID[36], 0x01)) supports += "\t\t1280×1024 @ 75 Hz\r\n";

                if (Contains(EDID[37], 0x80)) supports += "\t\t1152x870 @ 75 Hz (Apple Macintosh II)\r\n";
                if (Contains(EDID[37], 0x40)) supports += "\t\tOther manufacturer-specific display modes 1 | 0100 0000\r\n";
                if (Contains(EDID[37], 0x20)) supports += "\t\tOther manufacturer-specific display modes 2 | 0010 0000\r\n";
                if (Contains(EDID[37], 0x10)) supports += "\t\tOther manufacturer-specific display modes 3 | 0001 0000\r\n";
                if (Contains(EDID[37], 0x08)) supports += "\t\tOther manufacturer-specific display modes 4 | 0000 1000\r\n";
                if (Contains(EDID[37], 0x04)) supports += "\t\tOther manufacturer-specific display modes 5 | 0000 0100\r\n";
                if (Contains(EDID[37], 0x02)) supports += "\t\tOther manufacturer-specific display modes 6 | 0000 0010\r\n";
                if (Contains(EDID[37], 0x01)) supports += "\t\tOther manufacturer-specific display modes 7 | 0000 0001\r\n";

                return supports;
            } //

            public static string Display_Type(byte[] EDID)
            {
                if (EDID == null || EDID.Length < 128) return "";

                if ((EDID[20] & 0x80) == 0x80)
                {
                    if ((EDID[24] & 0x18) == 0x18)
                    {
                        return "(Digital) RGB 4:4:4 + YCrCb 4:4:4 + YCrCb 4:2:2";
                    }
                    else if ((EDID[24] & 0x10) == 0x10)
                    {
                        return "(Digital) RGB 4:4:4 + YCrCb 4:2:2";
                    }
                    else if ((EDID[24] & 0x08) == 0x08)
                    {
                        return "(Digital) RGB 4:4:4 + YCrCb 4:4:4";
                    }
                    else
                    {
                        return "(Digital) RGB 4:4:4";
                    }
                }
                else
                {
                    if ((EDID[24] & 0x18) == 0x18)
                    {
                        return "(Analog) Undefined";
                    }
                    else if ((EDID[24] & 0x10) == 0x10)
                    {
                        return "(Analog) Non-RGB color";
                    }
                    else if ((EDID[24] & 0x08) == 0x08)
                    {
                        return "(Analog) RGB color";
                    }
                    else
                    {
                        return "(Analog) Monochrome or Grayscale";
                    }
                }
            }

            public static string Display_Type(byte[] EDID, byte videoInputType)
            {
                if (EDID == null || EDID.Length < 128) return "";

                if ((videoInputType & 0x80) == 0x80)
                {
                    if ((EDID[24] & 0x18) == 0x18)
                    {
                        return "(Digital) RGB 4:4:4 + YCrCb 4:4:4 + YCrCb 4:2:2";
                    }
                    else if ((EDID[24] & 0x10) == 0x10)
                    {
                        return "(Digital) RGB 4:4:4 + YCrCb 4:2:2";
                    }
                    else if ((EDID[24] & 0x08) == 0x08)
                    {
                        return "(Digital) RGB 4:4:4 + YCrCb 4:4:4";
                    }
                    else
                    {
                        return "(Digital) RGB 4:4:4";
                    }
                }
                else
                {
                    if ((EDID[24] & 0x18) == 0x18)
                    {
                        return "(Analog) Undefined";
                    }
                    else if ((EDID[24] & 0x10) == 0x10)
                    {
                        return "(Analog) Non-RGB color";
                    }
                    else if ((EDID[24] & 0x08) == 0x08)
                    {
                        return "(Analog) RGB color";
                    }
                    else
                    {
                        return "(Analog) Monochrome or Grayscale";
                    }
                }
            }
        }

        #endregion Gamma/Color and Etablished Timings：



        #region Preferred Detailed Timing：

        public class Preferred_Detailed_Timing
        {
            public static string Active_Ratio(byte[] EDID)
            {
                if (EDID == null || EDID.Length < 128) return "";
                int vaH = ((((int)EDID[58] << 4) & 0x0F00) | (int)EDID[56]);
                int vaV = ((((int)EDID[61] << 4) & 0x0F00) | (int)EDID[59]);
                return Ratio(vaH, vaV);
            }

            public static string Pixel_Clock(byte[] EDID)
            {
                if (EDID == null || EDID.Length < 128) return "";
                double val = (((int)EDID[55] << 8) | (int)EDID[54]) / 100.0d;
                return val.ToString("0.00") + "MHz";
            }

            public static string Horizontal_Active(byte[] EDID)
            {
                if (EDID == null || EDID.Length < 128) return "";
                int val = ((((int)EDID[58] << 4) & 0x0F00) | (int)EDID[56]);
                return val.ToString() + " pixels";
            }

            public static string Horizontal_Blanking(byte[] EDID)
            {
                if (EDID == null || EDID.Length < 128) return "";
                int val = ((((int)EDID[58] << 8) & 0x0F00) | (int)EDID[57]);
                return val.ToString() + " pixels";
            }

            public static string Horizontal_Sync_Offset(byte[] EDID)
            {
                if (EDID == null || EDID.Length < 128) return "";
                int val = ((((int)EDID[65] << 2) & 0x0300) | (int)EDID[62]);
                return val.ToString() + " pixels";
            }

            public static string Horizontal_Sync_Pulse_Width(byte[] EDID)
            {
                if (EDID == null || EDID.Length < 128) return "";
                int val = ((((int)EDID[65] << 4) & 0x0300) | (int)EDID[63]);
                return val.ToString() + " pixels";
            }

            public static string Horizontal_Border(byte[] EDID)
            {
                return EDID[69].ToString() + " pixels";
            }

            public static string Horizontal_Size(byte[] EDID)
            {
                if (EDID == null || EDID.Length < 128) return "";
                int val = ((((int)EDID[68] << 4) & 0x0F00) | (int)EDID[66]);
                return val.ToString() + " mm";
            }

            public static string Vertical_Active(byte[] EDID)
            {
                if (EDID == null || EDID.Length < 128) return "";
                int val = ((((int)EDID[61] << 4) & 0x0F00) | (int)EDID[59]);
                return val.ToString() + " lines";
            }

            public static string Vertical_Blanking(byte[] EDID)
            {
                if (EDID == null || EDID.Length < 128) return "";
                int val = ((((int)EDID[61] << 8) & 0x0F00) | (int)EDID[60]);
                return val.ToString() + " lines";
            }

            public static string Vertical_Sync_Offset(byte[] EDID)
            {
                if (EDID == null || EDID.Length < 128) return "";
                int val = ((((int)EDID[65] << 2) & 0x0030) | ((((int)EDID[64] >> 4) & 0x000F)));
                return val.ToString() + " lines";
            }

            public static string Vertical_Sync_Pulse_Width(byte[] EDID)
            {
                if (EDID == null || EDID.Length < 128) return "";
                int val = ((((int)EDID[65] << 4) & 0x0030) | ((((int)EDID[64]) & 0x000F)));
                return val.ToString() + " lines";
            }

            public static string Vertical_Border(byte[] EDID)
            {
                return EDID[70].ToString() + " lines";
            }

            public static string Vertical_Size(byte[] EDID)
            {
                if (EDID == null || EDID.Length < 128) return "";
                int val = ((((int)EDID[68] << 8) & 0x0F00) | (int)EDID[67]);
                return val.ToString() + " mm";
            }

            public static string Input_Type_Sync_type(byte[] EDID)
            {
                if (EDID == null || EDID.Length < 128) return "";
                int val = ((int)EDID[71] >> 3) & 0x0003;
                switch (val)
                {
                    case 3: return "Digital separate";
                    case 2: return "Digital composite (on HSync)";
                    case 1: return "Bipolar analog composite";
                    case 0: return "Analog composite";
                    default: return "";
                }
            }

            public static string Interlaced(byte[] EDID)
            {
                if (EDID == null || EDID.Length < 128) return "";
                if (Contains(EDID[71], 0x80))
                {
                    return "True";
                }
                else
                {
                    return "False";
                }
            }

            public static string VerticalPolarity(byte[] EDID)
            {
                if (EDID == null || EDID.Length < 128) return "";
                if (Contains(EDID[71], 0x04))
                {
                    return "True";
                }
                else
                {
                    return "False";
                }
            }// 	False

            public static string HorizontalPolarity(byte[] EDID)
            {
                if (EDID == null || EDID.Length < 128) return "";
                if (Contains(EDID[71], 0x02))
                {
                    return "True";
                }
                else
                {
                    return "False";
                }
            }// 	True
        }

        #endregion Preferred Detailed Timing：

        #region Detailed Timing #2：

        public class Detailed_Timing_Sharp2
        {
            public static string Pixel_Clock(byte[] EDID)
            { return ""; }// 	114.46 Mhz

            public static string Horizontal_Active(byte[] EDID)
            { return ""; }// 	1920 pixels

            public static string Horizontal_Blanking(byte[] EDID)
            { return ""; }// 	244 pixels

            public static string Horizontal_Sync_Offset(byte[] EDID)
            { return ""; }// 	48 pixels

            public static string Horizontal_Sync_Pulse_Width(byte[] EDID)
            { return ""; }// 	32 pixels

            public static string Horizontal_Border(byte[] EDID)
            { return ""; }// 	0 pixels

            public static string Horizontal_Size(byte[] EDID)
            { return ""; }// 	344 mm

            public static string Vertical_Active(byte[] EDID)
            { return ""; }// 	1080 lines

            public static string Vertical_Blanking(byte[] EDID)
            { return ""; }// 	22 lines

            public static string Vertical_Sync_Offset(byte[] EDID)
            { return ""; }// 	3 lines

            public static string Vertical_Sync_Pulse_Width(byte[] EDID)
            { return ""; }// 	5 lines

            public static string Vertical_Border(byte[] EDID)
            { return ""; }// 	0 lines

            public static string Vertical_Size(byte[] EDID)
            { return ""; }// 	194 mm

            public static string Input_Type(byte[] EDID)
            { return ""; }// 	Digital Separate

            public static string Interlaced(byte[] EDID)
            { return ""; }// 	False

            public static string VerticalPolarity(byte[] EDID)
            { return ""; }// 	False

            public static string HorizontalPolarity(byte[] EDID)
            { return ""; }// 	True
        }

        #endregion Detailed Timing #2：

        #region Monitor Range Limit: //

        public class Monitor_Range_Limit
        {
            public static string Maximum_Vertical_Frequency(byte[] EDID)
            {
                if (EDID == null || EDID.Length < 128) return "";
                return EDID[96] + " Hz";
            }

            public static string Minimum_Vertical_Frequency(byte[] EDID)
            {
                if (EDID == null || EDID.Length < 128) return "";
                return EDID[95] + " Hz";
            }

            public static string Maximum_Horizontal_Frequency(byte[] EDID)
            {
                if (EDID == null || EDID.Length < 128) return "";
                return EDID[98] + " KHz";
            }

            public static string Minimum_Horizontal_Frequency(byte[] EDID)
            {
                if (EDID == null || EDID.Length < 128) return "";
                return EDID[97] + " KHz";
            }

            public static string Maximum_Pixel_Clock(byte[] EDID)
            {
                if (EDID == null || EDID.Length < 128) return "";
                return EDID[99] * 10 + " MHz";
            }

            public static string Extended_Timing_Information_Type(byte[] EDID)
            {
                if (EDID == null || EDID.Length < 128) return "";
                switch (EDID[100])
                {
                    case 0x00: return "No information, padded with 0A 20 20 20 20 20 20.";
                    case 0x02:
                        return "Secondary GTF supported" +
                        EDID[100].ToString("X2") + " " +
                        EDID[101].ToString("X2") + " " +
                        EDID[102].ToString("X2") + " " +
                        EDID[103].ToString("X2") + " " +
                        EDID[104].ToString("X2") + " " +
                        EDID[105].ToString("X2") + " " +
                        EDID[106].ToString("X2") + " " +
                        EDID[107].ToString("X2") + " " +
                        ".";

                    default:
                        return "Unknow information, padded with " +
                        EDID[100].ToString("X2") + " " +
                        EDID[101].ToString("X2") + " " +
                        EDID[102].ToString("X2") + " " +
                        EDID[103].ToString("X2") + " " +
                        EDID[104].ToString("X2") + " " +
                        EDID[105].ToString("X2") + " " +
                        EDID[106].ToString("X2") + " " +
                        EDID[107].ToString("X2") + " " +
                        ".";
                }
            }

            public class Secondary_GTF
            {
                public const byte ETIT = 0x02;

                public static string Secondary_Curve_Start_Frequency(byte[] EDID)
                {
                    if (EDID == null || EDID.Length < 128) return "";
                    if (EDID[100] != ETIT) return "Invalid";
                    return EDID[102] * 2 + " KHz";
                }

                public static string GTF_C(byte[] EDID)
                {
                    if (EDID == null || EDID.Length < 128) return "";
                    if (EDID[100] != ETIT) return "Invalid";
                    return (EDID[103] / 2.0d).ToString("0.0");
                }

                public static string GTF_M(byte[] EDID)
                {
                    if (EDID == null || EDID.Length < 128) return "";
                    if (EDID[100] != ETIT) return "Invalid";

                    int val = ((EDID[105] << 8) & 0xFF00) | EDID[104];
                    return val.ToString();
                }

                public static string GTF_K(byte[] EDID)
                {
                    if (EDID == null || EDID.Length < 128) return "";
                    if (EDID[100] != ETIT) return "Invalid";
                    return EDID[106].ToString();
                }

                public static string GTF_J(byte[] EDID)
                {
                    if (EDID == null || EDID.Length < 128) return "";
                    if (EDID[100] != ETIT) return "Invalid";
                    return (EDID[103] / 2.0d).ToString("0.0");
                }
            }
        }

        #endregion Monitor Range Limit: //

        #region Stereo Display{        return "";    }//

        public class Stereo_Display
        {
            public static string Stereo_Mode(byte[] EDID)
            {
                if (EDID == null || EDID.Length < 128) return "";
                int val = ((int)EDID[71] >> 5) & 0x0003;
                if (Contains(EDID[71], 0x01))
                {
                    switch (val)
                    {
                        case 3: return "side-by-side";
                        case 2: return "Left image on even lines";
                        case 1: return "Right image on even lines";
                        case 0: return "Normal display (no stereo)";
                        default: return "";
                    }
                }
                else
                {
                    switch (val)
                    {
                        case 3: return "4-way interleaved stereo";
                        case 2: return "similar, sync=1 during left";
                        case 1: return "Field sequential, sync=1 during right";
                        case 0: return "Normal display (no stereo)";
                        default: return "";
                    }
                }
            }
        }

        #endregion Stereo Display{        return "";    }//
    }
}