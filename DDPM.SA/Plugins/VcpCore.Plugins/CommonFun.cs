using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;
using VcpCore.Common;
using static VcpCore.Plugins.EDIDReader;

namespace VcpCore.Plugins
{
    internal class CommonFun
    {
        public static bool getEDID(string MontitorID, ref EDID edid)
        {
            bool result = false;

            try
            {
                ManagementObjectSearcher searcher = new ManagementObjectSearcher("Root\\WMI", "SELECT * FROM WmiMonitorDescriptorMethods");
                foreach (ManagementObject TempMonitor in searcher.Get())
                {
                    string InstanceName = TempMonitor.GetPropertyValue("InstanceName").ToString();
                    if (InstanceName.Split("\\").Count() < 1 || InstanceName.Split("\\")[1] != MontitorID.Split("\\")[1])
                    {
                        continue;
                    }

                    EdidParser classEdidParser = new EdidParser();
                    ManagementBaseObject methodParameters = TempMonitor.GetMethodParameters("WmiGetMonitorRawEEdidV1Block");
                    methodParameters["BlockId"] = 0;
                    try
                    {
                        ManagementBaseObject managementBaseObject = TempMonitor.InvokeMethod("WmiGetMonitorRawEEdidV1Block", methodParameters, null);

                        byte[] blocks = (byte[])managementBaseObject["BlockContent"];
                        edid.VideoInputType = Display_Parameters.Video_Input_Definition(blocks);
                        edid.EdidVersion = Vendor_Product_Identification.EDIDVersion(blocks);
                        classEdidParser.Push(blocks);
                        edid.Edid = classEdidParser.HexString;
                        edid.ManufactureID = classEdidParser.GetManufacturerID();
                        edid.VendorID = classEdidParser.GetVendorID();
                        edid.PID = classEdidParser.GetPID(blocks);
                        //  string text2 = classEdidParser.GetManufacturerID() + classEdidParser.GetVendorID();
                        edid.SerialNumber = classEdidParser.GetSerialNum();
                        edid.Year = classEdidParser.GetManufactureYearAndMonth(ref edid.Month, ref edid.Week);
                        edid.ServiceTag = classEdidParser.GetServiceTag();
                        edid.ModelName = classEdidParser.GetModelName();
                        edid.Size = classEdidParser.GetScreenSize();

                        try
                        {
                            var strEDID = BitConverter.ToString(StringToByteArray(edid.Edid));
                        }
                        catch (Exception) { }

                        {
                            result = true;
                        }

                    }
                    catch (Exception) { }
                }

            }
            catch (Exception) { result = false; }

            return result;
        }


        public static string ConvertManufacturerID(string hexManufacturerID)
        {
            if (string.IsNullOrEmpty(hexManufacturerID) || hexManufacturerID.Length < 4)
                return "";


            int num = int.Parse(hexManufacturerID.Substring(0, 2), NumberStyles.HexNumber);
            int num2 = int.Parse(hexManufacturerID.Substring(2, 2), NumberStyles.HexNumber);
            int num3 = (num2 << 8) + num;
            int a = (num3 & 0x7C00) >> 10;
            int a2 = (num3 & 0x3E0) >> 5;
            int a3 = num3 & 0x1F;
            string text = int2charByASCII(a);
            string text2 = int2charByASCII(a2);
            string text3 = int2charByASCII(a3);
            return text + text2 + text3;
        }

        public static string int2charByASCII(int a)
        {
            return ((char)(a + 64)).ToString() ?? "";
        }

        public static byte[] StringToByteArray(string hex)
        {
            return Enumerable.Range(0, hex.Length)
                             .Where(x => x % 2 == 0)
                             .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                             .ToArray();
        }
    }
}
