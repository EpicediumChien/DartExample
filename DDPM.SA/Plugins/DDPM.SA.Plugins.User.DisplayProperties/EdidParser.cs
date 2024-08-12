using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDPM.SA.Plugins.User.DisplayProperties
{
    public class EdidParser
    {
        private readonly string EDID_Header = "00FFFFFFFFFFFF00";

        private readonly string EDID_SerivceTag_Header = "000000FF00";

        private readonly string ModelName_Header = "000000FC00";

        private readonly int Manufacturer_ID_Len = 4;

        private readonly int VENDOR_ID_Len = 4;

        private readonly int SerialNum_Len = 8;

        private readonly int ManufactureDate_Len = 4;

        private readonly int EDIDVer_Len = 4;

        private readonly int VideoInputDef_Len = 2;

        private readonly int ScreenSize_Len = 4;

        private readonly int ModelName_Len = 13;

        private List<byte> edid = new List<byte>();

        public string HexString { get; private set; } = string.Empty;


        public void Push(byte[] blocks)
        {
            for (int i = 0; i < blocks.Length; i++)
            {
                edid.Add(blocks[i]);
            }
            HexString = BitConverter.ToString(edid.ToArray()).Replace("-", string.Empty);
        }

        public bool CheckIsBlock0()
        {
            return HexString.IndexOf(EDID_Header) >= 0;
        }


        public string GetManufacturerID()
        {
            string ManufactureID = string.Empty;

            if (HexString.Length < (EDID_Header.Length + Manufacturer_ID_Len))
            {
                return "";
            }

            string EDID = HexString.Substring(EDID_Header.Length, Manufacturer_ID_Len);
            if (EDID == "" || EDID.Length < 4)
            {
                return "";
            }
            int num = int.Parse(EDID.Substring(0, 2), NumberStyles.HexNumber);
            int num2 = int.Parse(EDID.Substring(2, 2), NumberStyles.HexNumber);
            int num3 = (num << 8) + num2;
            int a = (num3 & 0x7C00) >> 10;
            int a2 = (num3 & 0x3E0) >> 5;
            int a3 = num3 & 0x1F;
            string text = int2charByASCII(a);
            string text2 = int2charByASCII(a2);
            string text3 = int2charByASCII(a3);
            return text + text2 + text3;
        }

        public string GetVendorID()
        {
            string vendorid = "";
            int startIndex = 2;
            int startIndex2 = 0;
            int length = 2;
            if (HexString.Length >= (EDID_Header.Length + Manufacturer_ID_Len + VENDOR_ID_Len + 4))
            {
                string text = HexString.Substring(EDID_Header.Length + Manufacturer_ID_Len, VENDOR_ID_Len);

                vendorid = text.Substring(startIndex, length) + text.Substring(startIndex2, length);
            }
            return vendorid;

        }


        public string GetSerialNum()
        {
            if (HexString.Length < (EDID_Header.Length + Manufacturer_ID_Len + VENDOR_ID_Len + SerialNum_Len))
            {
                return "";
            }
            string text = HexString.Substring(EDID_Header.Length + Manufacturer_ID_Len + VENDOR_ID_Len, SerialNum_Len);

            if (text.Length < 8)
            {
                return "";
            }
            int num = 0;
            for (int i = 3; i >= 0; i--)
            {
                int num2 = int.Parse(text.Substring(2 * i, 2), NumberStyles.HexNumber);
                //SerialNum+= (char)num2;
                num |= num2 << i * 8;
            }
            return num.ToString();
        }

        public string GetServiceTag()
        {
            int num = HexString.IndexOf(EDID_SerivceTag_Header);
            if (num < 0 || HexString.Length < (num + EDID_SerivceTag_Header.Length + 26))
            {
                return "";
            }
            string text = HexString.Substring(num + EDID_SerivceTag_Header.Length, 26);
            if (text.Length < 26)
            {
                return "";
            }

            List<byte> list = new List<byte>();
            for (int i = 0; i < 13; i++)
            {
                string text2 = text.Substring(2 * i, 2);
                if (text2 == "0A")
                {
                    break;
                }
                byte[] bytes = BitConverter.GetBytes(int.Parse(text2, NumberStyles.HexNumber));
                list.AddRange(bytes);
            }
            byte[] bytes2 = list.ToArray();
            string text3 = "";
            string @string = Encoding.ASCII.GetString(bytes2);
            for (int j = 0; j < @string.Length; j++)
            {
                char value = @string[j];
                if (Convert.ToInt32(value) >= 48)
                {
                    text3 += value;
                }
            }
            return text3;
        }

        public int GetManufactureYearAndMonth(ref int nMonth)
        {
            nMonth = -1;
            int year = -1;
            int week = -1;
            if (HexString.Length < (EDID_Header.Length + Manufacturer_ID_Len + VENDOR_ID_Len + SerialNum_Len + ManufactureDate_Len + 4))
            {
                return 0;
            }

            try
            {
                year = int.Parse(HexString.Substring(EDID_Header.Length + Manufacturer_ID_Len + VENDOR_ID_Len + SerialNum_Len, ManufactureDate_Len).Substring(2, 2), NumberStyles.HexNumber) + 1990;
            }
            catch (Exception)
            {
                return -1;
            }

            try
            {
                week = int.Parse(HexString.Substring(EDID_Header.Length + Manufacturer_ID_Len + VENDOR_ID_Len + SerialNum_Len, ManufactureDate_Len).Substring(0, 2), NumberStyles.HexNumber);
            }
            catch (Exception)
            {
                return year;
            }

            nMonth = new DateTime(year, 1, 1).AddDays(7 * (week - 1)).Month;
            //DateTime dt = new DateTime(year, 1, 1).AddDays(7 * (week - 1);

            try
            {
                string strMonth = DateTimeFormatInfo.CurrentInfo.GetAbbreviatedMonthName(nMonth);
            }
            catch (Exception)
            {
                ;
            }


            return year;
        }

        public string GetModelName()
        {
            int num = HexString.IndexOf(ModelName_Header);
            if (num != -1)
            {
                string text = HexString.Substring(num + ModelName_Header.Length, 26);
                List<byte> list = new List<byte>();
                for (int i = 0; i < 13; i++)
                {
                    byte[] bytes = BitConverter.GetBytes(int.Parse(text.Substring(2 * i, 2), NumberStyles.HexNumber));
                    list.AddRange(bytes);
                }
                byte[] bytes2 = list.ToArray();
                string text2 = "";
                string @string = Encoding.ASCII.GetString(bytes2);
                for (int j = 0; j < @string.Length; j++)
                {
                    char value = @string[j];
                    if (Convert.ToInt32(value) >= 32) // 32 is Space of ASCII.
                    {
                        text2 += value;
                    }
                }
                return text2;
            }
            return "";
        }

        public string GetProductCode(string edid)
        {
            string result = "";
            if (edid != null && edid.Length > 24)
            {
                result = edid.Substring(22, 2) + edid.Substring(20, 2);
            }
            return result;
        }

        public float GetScreenSize()
        {
            if (HexString.Length < EDID_Header.Length + Manufacturer_ID_Len + VENDOR_ID_Len + SerialNum_Len + ManufactureDate_Len + EDIDVer_Len + VideoInputDef_Len + ScreenSize_Len + 4)
            {
                return 0;
            }
            string text = HexString.Substring(EDID_Header.Length + Manufacturer_ID_Len + VENDOR_ID_Len + SerialNum_Len + ManufactureDate_Len + EDIDVer_Len + VideoInputDef_Len, ScreenSize_Len);
            int num = 0;
            int num2 = 0;
            num = int.Parse(text.Substring(0, 2), NumberStyles.HexNumber);
            num2 = int.Parse(text.Substring(2, 2), NumberStyles.HexNumber);
            return (float)(Math.Sqrt(num * num + num2 * num2) / 2.45);
        }

        public int GetExtensionFlag()
        {
            int result = 0;
            try
            {
                string value = HexString.Substring(252, 2);
                result = Convert.ToInt32(value);
                return result;
            }
            catch
            {
                return result;
            }
        }

        private static string int2charByASCII(int a)
        {
            return ((char)(a + 64)).ToString() ?? "";
        }
    }
}
