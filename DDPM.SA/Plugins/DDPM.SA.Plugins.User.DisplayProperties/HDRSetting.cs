using Microsoft.Win32;
using System;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using VcpCore.Common;
using static VcpCore.Common.User32;

namespace DDPM.SA.Plugins.User.DisplayProperties
{
    public class HDRSetting
    {
        public bool GetWindowsHDRStatus(EDID monitorEdid, out bool blOnOff)
        {
            DISPLAYCONFIG_PATH_INFO path = new DISPLAYCONFIG_PATH_INFO();
            GetDisplayConfigPath(monitorEdid, out path);
            bool result = false;
            blOnOff = false;

            try
            {
                var info = new DISPLAYCONFIG_GET_ADVANCED_COLOR_INFO();

                info.header.type = DISPLAYCONFIG_DEVICE_INFO_TYPE.DISPLAYCONFIG_DEVICE_INFO_GET_ADVANCED_COLOR_INFO;
                info.header.size = Marshal.SizeOf<DISPLAYCONFIG_GET_ADVANCED_COLOR_INFO>();
                info.header.adapterId = path.targetInfo.adapterId;
                info.header.id = path.targetInfo.id;

                if (DisplayConfigGetDeviceInfo(ref info) == 0)
                {
                    blOnOff = info.advancedColorEnabled;
                    result = true;
                }
            }
            catch (Exception)// ex)
            {
            }

            return result;
        }

        //https://csharp.hotexamples.com/es/examples/-/DISPLAYCONFIG_SET_ADVANCED_COLOR_STATE/-/php-displayconfig_set_advanced_color_state-class-examples.html
        //https://github.com/dumbie/ArnoldVinkCode/blob/master/Desktop/Functions/AVDisplayMonitor/AVDisplayMonitorDisplayConfig.cs
        public bool SetWindowsHDRStatus(EDID monitorEdid, bool bEnable)
        {
            DISPLAYCONFIG_PATH_INFO Path = new DISPLAYCONFIG_PATH_INFO();
            GetDisplayConfigPath(monitorEdid, out Path);
            bool result = false;
            try
            {
                DISPLAYCONFIG_SET_ADVANCED_COLOR_STATE deviceInfo = new DISPLAYCONFIG_SET_ADVANCED_COLOR_STATE();
                deviceInfo.header.type = DISPLAYCONFIG_DEVICE_INFO_TYPE.DISPLAYCONFIG_DEVICE_INFO_SET_ADVANCED_COLOR_STATE;
                deviceInfo.header.size = Marshal.SizeOf<DISPLAYCONFIG_SET_ADVANCED_COLOR_STATE>();
                deviceInfo.header.adapterId = Path.targetInfo.adapterId;
                deviceInfo.header.id = Path.targetInfo.id;
                deviceInfo.advancedColorEnabled = bEnable;

                if (DisplayConfigSetDeviceInfo(ref deviceInfo) == 0)
                    result = true;
            }
            catch (Exception)// ex)
            {
                //Console.WriteLine($"\t SetWindowsHDRStatus exception:{ex.ToString()}");
            }
            return result;
        }

        private bool GetDisplayConfigPath(EDID monitorEdid, out DISPLAYCONFIG_PATH_INFO outPath)
        {
            bool result = false;
            outPath = new DISPLAYCONFIG_PATH_INFO();

            try
            {
                if (GetDisplayConfigBufferSizes(QDC.QDC_ONLY_ACTIVE_PATHS, out var pathCount, out var modeCount) == 0)
                {
                    var paths = new DISPLAYCONFIG_PATH_INFO[pathCount];
                    var modes = new DISPLAYCONFIG_MODE_INFO[modeCount];

                    if (QueryDisplayConfig(QDC.QDC_ONLY_ACTIVE_PATHS, ref pathCount, paths, ref modeCount, modes, IntPtr.Zero) == 0)
                    {
                        foreach (var path in paths)
                        {
                            EDID edid;
                            if (GetDisplayConfigEdidByDisplayConfigPath(path, out edid))
                            {
                                if (monitorEdid.VendorID == edid.VendorID &&
                                    monitorEdid.SerialNumber == edid.SerialNumber &&
                                    monitorEdid.ManufactureID == edid.ManufactureID)
                                {
                                    outPath = path;
                                    result = true;
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception)// ex)
            {
                //Console.WriteLine($"\tGetDisplayConfigPath exception:{ex.ToString()}");
            }
            return result;
        }

        private bool GetDisplayConfigTargetDeviceName(DISPLAYCONFIG_PATH_INFO path, out DISPLAYCONFIG_TARGET_DEVICE_NAME info)
        {
            bool result = false;

            info = new DISPLAYCONFIG_TARGET_DEVICE_NAME();
            info.header.type = DISPLAYCONFIG_DEVICE_INFO_TYPE.DISPLAYCONFIG_DEVICE_INFO_GET_TARGET_NAME;
            info.header.size = Marshal.SizeOf<DISPLAYCONFIG_TARGET_DEVICE_NAME>();
            info.header.adapterId = path.targetInfo.adapterId;
            info.header.id = path.targetInfo.id;
            if (DisplayConfigGetDeviceInfo(ref info) == 0)
            {
                string var = CommonFun.ConvertManufacturerID(info.edidManufactureId.ToString("X"));

                result = true;
            }

            return result;
        }

        private bool GetDisplayConfigEdidByDisplayConfigPath(DISPLAYCONFIG_PATH_INFO path, out EDID outEdid)
        {
            bool result = false;
            outEdid = new EDID();
            DISPLAYCONFIG_TARGET_DEVICE_NAME info = new DISPLAYCONFIG_TARGET_DEVICE_NAME();
            info.header.type = DISPLAYCONFIG_DEVICE_INFO_TYPE.DISPLAYCONFIG_DEVICE_INFO_GET_TARGET_NAME;
            info.header.size = Marshal.SizeOf<DISPLAYCONFIG_TARGET_DEVICE_NAME>();
            info.header.adapterId = path.targetInfo.adapterId;
            info.header.id = path.targetInfo.id;
            if (DisplayConfigGetDeviceInfo(ref info) == 0)
            {
                string var = CommonFun.ConvertManufacturerID(info.edidManufactureId.ToString("X"));

                string[] getDispName = info.monitorDevicePat.Split('#');

                if (getDispName.Length >= 3)
                {
                    getDispName[0] = Regex.Replace(getDispName[0], "[@,\\.\";'\\\\;?]", string.Empty);
                    string regPath = $"{getDispName[0]}\\{getDispName[1]}\\{getDispName[2]}";
                    string getMonitorIdPath;
                    if (GetMontitorIdPathForWMI(regPath, out getMonitorIdPath))
                    {
                        if (CommonFun.getEDID(getMonitorIdPath, ref outEdid))
                            result = true;
                    }
                }
            }

            return result;
        }

        private bool GetMontitorIdPathForWMI(string RegPath, out string outValue)
        {
            bool blResult = false;
            outValue = "";
            try
            {
                RegistryKey registryKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
                if (registryKey != null)
                {
                    string strKey = $"SYSTEM\\CurrentControlSet\\Enum\\{RegPath}\\";
                    //ConsoleOutput(strKey);
                    RegistryKey? registryKey2 = registryKey.OpenSubKey(strKey, writable: false);
                    if (registryKey2 != null)
                    {
                        if (registryKey2.GetValue("HardwareID") != null)
                        {
                            var temp = registryKey2.GetValue("HardwareID");
                            if (temp == null)
                            {
                                return false;
                            }
                            string[] valueStrings = (string[])temp;

                            foreach (string s in valueStrings)
                                outValue += (string)s;

                            var tmp = registryKey2.GetValue("ContainerID", "");
                            if (tmp == null)
                            {
                                return false;
                            }
                            string? str = tmp.ToString();

                            if (!string.IsNullOrEmpty(str))
                            {
                                outValue += $"\\{str}";
                                blResult = true;
                            }
                        }
                        registryKey2.Close();
                    }
                    registryKey.Close();
                }
            }
            catch (Exception)
            {
                ;
            }

            return blResult;
        }
    }
}