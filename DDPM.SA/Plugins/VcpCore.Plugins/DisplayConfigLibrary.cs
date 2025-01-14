using Microsoft.Win32;
using System;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using VcpCore.Common;
using static VcpCore.Common.User32;

namespace VcpCore.Plugins
{
    //https://stackoverflow.com/questions/66155083/windows-api-to-get-whether-hdrhigh-dynamic-range-is-active
    public class DisplayConfigLibrary
    {
        //Common.MonitorInfo
        public static bool GetDisplayConfigPath(EDID monitorEdid, out DISPLAYCONFIG_PATH_INFO outPath)
        {
            bool result = false;
            outPath = new DISPLAYCONFIG_PATH_INFO();

            try
            {
                if (_GetDisplayConfigBufferSizes(QDC.QDC_ONLY_ACTIVE_PATHS, out var pathCount, out var modeCount) == 0)
                {
                    var paths = new DISPLAYCONFIG_PATH_INFO[pathCount];
                    var modes = new DISPLAYCONFIG_MODE_INFO[modeCount];

                    if (_QueryDisplayConfig(QDC.QDC_ONLY_ACTIVE_PATHS, ref pathCount, paths, ref modeCount, modes, DISPLAYCONFIG_TOPOLOGY_ID.Zero) == 0)
                    {
                        foreach (var path in paths)
                        {
                            EDID edid;
                            if (GetDisplayConfigEdidByDisplayConfigPath(path, out edid) &&
                                monitorEdid.VendorID == edid.VendorID &&
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
            catch (Exception) { }

            return result;
        }

        public static bool GetWindowsHDRStatus(DISPLAYCONFIG_PATH_INFO path, out bool blOnOff)
        {
            //DISPLAYCONFIG_DEVICE_INFO_SET_ADVANCED_COLOR_STATE = 10,
            bool result = false;
            blOnOff = false;

            try
            {
                var info = new DISPLAYCONFIG_GET_ADVANCED_COLOR_INFO();

                info.header.type = DISPLAYCONFIG_DEVICE_INFO_TYPE.DISPLAYCONFIG_DEVICE_INFO_GET_ADVANCED_COLOR_INFO;
                info.header.size = Marshal.SizeOf<DISPLAYCONFIG_GET_ADVANCED_COLOR_INFO>();
                info.header.adapterId = path.targetInfo.adapterId;
                info.header.id = path.targetInfo.id;

                if (_DisplayConfigGetDeviceInfo(ref info) == 0)
                {
                    blOnOff = info.advancedColorEnabled;
                    result = true;
                }
            }
            catch (Exception) { }

            return result;
        }

        //https://csharp.hotexamples.com/es/examples/-/DISPLAYCONFIG_SET_ADVANCED_COLOR_STATE/-/php-displayconfig_set_advanced_color_state-class-examples.html
        //https://github.com/dumbie/ArnoldVinkCode/blob/master/Desktop/Functions/AVDisplayMonitor/AVDisplayMonitorDisplayConfig.cs
        public static bool SetWindowsHDRStatus(DISPLAYCONFIG_PATH_INFO Path, bool bEnable)
        {
            bool result = false;
            try
            {
                DISPLAYCONFIG_SET_ADVANCED_COLOR_STATE deviceInfo = new DISPLAYCONFIG_SET_ADVANCED_COLOR_STATE();
                deviceInfo.header.type = DISPLAYCONFIG_DEVICE_INFO_TYPE.DISPLAYCONFIG_DEVICE_INFO_SET_ADVANCED_COLOR_STATE;
                deviceInfo.header.size = Marshal.SizeOf<DISPLAYCONFIG_SET_ADVANCED_COLOR_STATE>();
                deviceInfo.header.adapterId = Path.targetInfo.adapterId;
                deviceInfo.header.id = Path.targetInfo.id;
                deviceInfo.advancedColorEnabled = bEnable;

                if (_DisplayConfigSetDeviceInfo(ref deviceInfo) == 0)
                    result = true;
            }
            catch (Exception) { }
            return result;
        }

        private static bool GetDisplayConfigTargetDeviceName(DISPLAYCONFIG_PATH_INFO path, out DISPLAYCONFIG_TARGET_DEVICE_NAME info)
        {
            bool result = false;

            info = new DISPLAYCONFIG_TARGET_DEVICE_NAME();
            info.header.type = DISPLAYCONFIG_DEVICE_INFO_TYPE.DISPLAYCONFIG_DEVICE_INFO_GET_TARGET_NAME;
            info.header.size = Marshal.SizeOf<DISPLAYCONFIG_TARGET_DEVICE_NAME>();
            info.header.adapterId = path.targetInfo.adapterId;
            info.header.id = path.targetInfo.id;
            if (_DisplayConfigGetDeviceInfo(ref info) == 0)
            {
                string var = CommonFun.ConvertManufacturerID(info.edidManufactureId.ToString("X"));
                result = true;
            }

            return result;
        }

        private static bool GetDisplayConfigEdidByDisplayConfigPath(DISPLAYCONFIG_PATH_INFO path, out EDID outEdid)
        {
            bool result = false;
            outEdid = new EDID();
            DISPLAYCONFIG_TARGET_DEVICE_NAME info = new DISPLAYCONFIG_TARGET_DEVICE_NAME();
            info.header.type = DISPLAYCONFIG_DEVICE_INFO_TYPE.DISPLAYCONFIG_DEVICE_INFO_GET_TARGET_NAME;
            info.header.size = Marshal.SizeOf<DISPLAYCONFIG_TARGET_DEVICE_NAME>();
            info.header.adapterId = path.targetInfo.adapterId;
            info.header.id = path.targetInfo.id;
            if (_DisplayConfigGetDeviceInfo(ref info) == 0)
            {
                string var = CommonFun.ConvertManufacturerID(info.edidManufactureId.ToString("X"));
                string[] getDispName = info.monitorDevicePat.Split('#');

                if (getDispName.Length >= 3)
                {
                    getDispName[0] = Regex.Replace(getDispName[0], "[@,\\.\";'\\\\;?]", string.Empty);
                    string regPath = $"{getDispName[0]}\\{getDispName[1]}\\{getDispName[2]}";
                    string getMonitorIdPath;
                    if (GetMontitorIdPathForWMI(regPath, out getMonitorIdPath) &&
                        CommonFun.getEDID(getMonitorIdPath, ref outEdid))
                    {
                        result = true;
                    }
                }
            }

            return result;
        }

        private static bool GetMontitorIdPathForWMI(string RegPath, out string outValue)
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
                    RegistryKey registryKey2 = registryKey.OpenSubKey(strKey, writable: false);
                    if (registryKey2 != null)
                    {
                        if (registryKey2.GetValue("HardwareID") != null)
                        {
                            string[] valueStrings = (string[])registryKey2.GetValue("HardwareID");

                            foreach (string s in valueStrings)
                                outValue += (string)s;

                            string str = registryKey2.GetValue("ContainerID", "").ToString();

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