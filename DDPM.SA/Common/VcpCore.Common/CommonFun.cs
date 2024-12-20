using Microsoft.Win32;
using System;
using System.Globalization;
using System.Linq;
using System.Management;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using static VcpCore.Common.EDIDReader;

[assembly: InternalsVisibleTo("VcpCore.Common.Test")]

namespace VcpCore.Common
{
    public class CommonFun
    {
        public static bool getEDID(string MontitorID, ref EDID edid, Logs _logs = null)
        {
            bool result = false;

            try
            {
                ManagementObjectSearcher searcher = new ManagementObjectSearcher("Root\\WMI", "SELECT * FROM WmiMonitorDescriptorMethods");
                foreach (ManagementObject TempMonitor in searcher.Get().Cast<ManagementObject>())
                {
                    string InstanceName = TempMonitor.GetPropertyValue("InstanceName").ToString();
                    string[] InstanceName_spilit = InstanceName.Split("\\");
                    if (InstanceName_spilit.Length == 3)
                    {
                        InstanceName_spilit[(InstanceName_spilit.Length - 1)] = Regex.Replace(InstanceName_spilit[(InstanceName_spilit.Length - 1)], @"_\d", string.Empty) ?? string.Empty;
                        string name = "SYSTEM\\CurrentControlSet\\Enum\\DISPLAY\\" + InstanceName_spilit[1] + "\\" + InstanceName_spilit[2];
                        using (var registryKey = Registry.LocalMachine.OpenSubKey(name))
                        {
                            if (_logs != null)
                                _logs.DebugMsg("[VcpCorePlugin] _Get_Monitors collection EDID open registryKey");

                            try
                            {
                                if (registryKey != null)
                                {
                                    string Path_Instance_DeviceID = (string)registryKey.GetValue("Driver");
                                    if (string.IsNullOrEmpty(Path_Instance_DeviceID) || !(MontitorID.Contains(Path_Instance_DeviceID)))
                                        continue;
                                    //if (InstanceName.Split("\\").Count() < 1 || InstanceName.Split("\\")[1] != MontitorID.Split("\\")[1])
                                    //    continue;

                                    name += "\\Device Parameters";

                                    //ManagementBaseObject methodParameters = TempMonitor.GetMethodParameters("WmiGetMonitorRawEEdidV1Block");
                                    //methodParameters["BlockId"] = 0;
                                    using (var registryKeyII = Registry.LocalMachine.OpenSubKey(name))
                                    {
                                        if (_logs != null)
                                            _logs.DebugMsg("[VcpCorePlugin] _Get_Monitors collection EDID open registryKeyII");

                                        try
                                        {
                                            //ManagementBaseObject managementBaseObject = TempMonitor.InvokeMethod("WmiGetMonitorRawEEdidV1Block", methodParameters, null);
                                            //byte[] blocks = (byte[])managementBaseObject["BlockContent"];

                                            if (registryKeyII != null)
                                            {
                                                byte[] blocks = (byte[])registryKeyII.GetValue("EDID");
                                                if (blocks != null)
                                                {
                                                    EdidParser classEdidParser = new EdidParser();
                                                    edid.VideoInputType = Display_Parameters.Video_Input_Definition(blocks);
                                                    edid.EdidVersion = Vendor_Product_Identification.EDIDVersion(blocks);
                                                    classEdidParser.Push(blocks);
                                                    edid.Edid = classEdidParser.HexString;
                                                    edid.ManufactureID = classEdidParser.GetManufacturerID();
                                                    edid.VendorID = classEdidParser.GetVendorID();
                                                    edid.PID = classEdidParser.GetPID(blocks);
                                                    //  string text2 = classEdidParser.GetManufacturerID() + classEdidParser.GetVendorID();
                                                    edid.SerialNumber = classEdidParser.GetSerialNum();
                                                    edid.Year = classEdidParser.GetManufactureYearAndMonth(ref edid);
                                                    edid.ServiceTag = classEdidParser.GetServiceTag();
                                                    edid.ModelName = classEdidParser.GetModelName();
                                                    edid.Size = classEdidParser.GetScreenSize();
                                                    try
                                                    {
                                                        var strEDID = BitConverter.ToString(StringToByteArray(edid.Edid));

                                                        if (_logs != null)
                                                            _logs.DebugMsg("[VcpCorePlugin] _Get_Monitors collection EDID strEDID : " + strEDID);
                                                    }
                                                    catch (Exception)
                                                    {
                                                        if (_logs != null)
                                                            _logs.DebugMsg("[VcpCorePlugin] _Get_Monitors collection EDID strEDID in BitConverter.ToString Exception");
                                                    }

                                                    result = true;
                                                }
                                            }
                                            else
                                            {
                                                if (_logs != null)
                                                    _logs.DebugMsg("[VcpCorePlugin] _Get_Monitors collection EDID registryKeyII is null");
                                            }
                                        }
                                        catch (Exception e)
                                        {
                                            if (_logs != null)
                                                _logs.DebugMsg("[VcpCorePlugin] _Get_Monitors collection EDID registryKeyII exception : " + e.Message);
                                        }
                                        //finally
                                        //{
                                        //    registryKeyII.Close();
                                        //    registryKeyII.Dispose();
                                        //}
                                    }
                                }
                                else
                                {
                                    if (_logs != null)
                                        _logs.DebugMsg("[VcpCorePlugin] _Get_Monitors collection EDID registryKey is null");
                                }
                            }
                            catch (Exception e)
                            {
                                if (_logs != null)
                                    _logs.DebugMsg("[VcpCorePlugin] _Get_Monitors collection EDID registryKey exception : " + e.Message);
                            }
                            //finally
                            //{
                            //    registryKey.Close();
                            //    registryKey.Dispose();
                            //}
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                if (_logs != null)
                    _logs.DebugMsg("[VcpCorePlugin] _Get_Monitors collection getEDID exception : " + ex.Message);

                result = false;
            }

            return result;
        }

        public static EDID getEDID(byte[] edid_byte)
        {
            EDID edid = new EDID();
            try
            {
                EdidParser classEdidParser = new EdidParser();
                edid.VideoInputType = Display_Parameters.Video_Input_Definition(edid_byte);
                edid.EdidVersion = Vendor_Product_Identification.EDIDVersion(edid_byte);
                classEdidParser.Push(edid_byte);
                edid.Edid = classEdidParser.HexString;
                edid.ManufactureID = classEdidParser.GetManufacturerID();
                edid.VendorID = classEdidParser.GetVendorID();
                edid.PID = classEdidParser.GetPID(edid_byte);
                edid.SerialNumber = classEdidParser.GetSerialNum();
                edid.Year = classEdidParser.GetManufactureYearAndMonth(ref edid);
                edid.ServiceTag = classEdidParser.GetServiceTag();
                edid.ModelName = classEdidParser.GetModelName();
                edid.Size = classEdidParser.GetScreenSize();
                try
                {
                    var strEDID = BitConverter.ToString(StringToByteArray(edid.Edid));
                }
                catch (Exception) { }
            }
            catch (Exception) { }

            return edid;
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