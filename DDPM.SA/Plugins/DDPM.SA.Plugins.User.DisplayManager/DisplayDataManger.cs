using DDPM.SA.Common;
using DDPM.SA.Common.Display;
using DDPM.SA.Common.Settings;
using Dell.Client.Framework.Common;
using System;
using System.Collections.Generic;
using System.Globalization;
using VcpCore.Common;
using static VcpCore.Common.User32;

namespace DDPM.SA.Plugins.User.DisplayManager
{
    public class DisplayDataManger
    {
        private List<DisplayData> _displayData { get; set; } = new List<DisplayData>();

        public enum log_type
        {
            info = 0,
            error
        }

        private static ILog _log = null;

        private void WriteLog(string text, log_type log_type = log_type.info,
            [System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
            [System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "",
            [System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = 0)
        {
            if (string.IsNullOrEmpty(text))
                text = "";

            string className = this.GetType().Name;
            text = $"{DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss.fff")}[DisplayDeviceHelper] {text}, Class:{className}, Caller Name:{memberName}, Source Line {sourceLineNumber}";
            Console.WriteLine(text);
            if (_log != null)
            {
                if (log_type == log_type.info)
                    _log.Info(text);
                else
                    _log.Error(text);
            }
        }

        public void InitDisplayData(List<MonitorInfo> monitorInfos)
        {
            WriteLog("[InitDisplayData] init...");
            if (monitorInfos != null)
            {
                //add DisplayData
                foreach (MonitorInfo info in monitorInfos)
                {
                    int mi = _displayData.FindIndex(x => x.Model == info.modelName &&
                                                        x.ServiceTag == info.edid.ServiceTag);
                    if (mi == -1)
                    {
                        WriteLog("[InitDisplayData] add monitor is " + info.modelName.ToString());
                        WriteLog("[InitDisplayData] add monitor is " + info.edid.ServiceTag.ToString());
                        DisplayData displayData = new DisplayData();
                        displayData.Model = info.modelName;
                        displayData.ServiceTag = info.edid.ServiceTag;
                        _displayData.Add(displayData);
                    }
                }

                //remove DisplayData
                for (int i = 0; i < _displayData.Count;)
                {
                    int di = monitorInfos.FindIndex(x => x.modelName == _displayData[i].Model &&
                                                         x.edid.ServiceTag == _displayData[i].ServiceTag);
                    if (di == -1)
                    {
                        WriteLog("[InitDisplayData] remove monitor is " + _displayData[i].Model.ToString());
                        WriteLog("[InitDisplayData] remove monitor is " + _displayData[i].ServiceTag.ToString());
                        _displayData.RemoveAt(i);
                    }
                    else
                    {
                        i++;
                    }
                }
            }
            else
            {
                WriteLog("[InitDisplayData] monitorInfos is null...");
            }
        }

        public bool ResetDisplayData(MonitorInfo monitorInfo, string reset)
        {
            WriteLog("[ResetDisplayData] init...");
            if (monitorInfo != null)
            {
                int mi = _displayData.FindIndex(x => x.Model == monitorInfo.modelName &&
                                                    x.ServiceTag == monitorInfo.edid.ServiceTag);
                if (mi != -1)
                {
                    switch (reset.ToUpper(CultureInfo.InvariantCulture))
                    {
                        case "ALL":
                            WriteLog("[ResetDisplayData]Reset all DisplayData");
                            DisplayData displayData = new DisplayData();
                            displayData.Model = monitorInfo.modelName;
                            displayData.ServiceTag = monitorInfo.edid.ServiceTag;
                            _displayData[mi] = displayData;
                            return true;
                        case "COLOR":
                            WriteLog("[ResetDisplayData]Reset DisplayData Color");
                            Color color = new Color();
                            _displayData[mi].Color = color;
                            return true;
                    }   
                }
                else
                {
                    WriteLog("[ResetDisplayData]_displayData is not find monitor");
                }
            }
            else
            {
                WriteLog("[ResetDisplayData] monitorInfo is null...");
            }
            return false;
        }

        public bool GetMonitorUSB(MonitorInfo monitorInfo, string inputSource, out string USB)
        {
            if (monitorInfo != null && !string.IsNullOrEmpty(inputSource))
            {
                WriteLog("[GetMonitorUSB]inputSource : " + inputSource);
                int mi = _displayData.FindIndex(x => x.Model == monitorInfo.modelName &&
                                                     x.ServiceTag == monitorInfo.edid.ServiceTag);
                if (mi != -1)
                {
                    int index = _displayData[mi].DisplayUSB.FindIndex(x => x.inputSource == inputSource);
                    if (index != -1)
                    {
                        USB = _displayData[mi].DisplayUSB[index].USB;
                        WriteLog("[GetMonitorUSB]USB : " + USB);
                        if (USB != string.Empty)
                        {
                            return true;
                        }
                    }
                }
                else
                {
                    WriteLog("[GetMonitorUSB]_displayData is not find monitor");
                }
            }
            else
            {
                WriteLog("[GetMonitorUSB]monitorInfo is null or inputSource is null or empty.");
            }
            USB = string.Empty;
            return false;
        }

        public bool GetMonitorUSB(MonitorInfo monitorInfo, out List<InputSource_USB> _USBs)
        {
            if (monitorInfo != null)
            {
                int mi = _displayData.FindIndex(x => x.Model == monitorInfo.modelName &&
                                                     x.ServiceTag == monitorInfo.edid.ServiceTag);
                if (mi != -1 && _displayData[mi].DisplayUSB != null && _displayData[mi].DisplayUSB.Count > 0)
                {
                    _USBs = _displayData[mi].DisplayUSB;
                    return true;
                }
                else
                {
                    WriteLog("[GetMonitorUSB]_displayData not find DisplayUSB.");
                }
            }
            else
            {
                WriteLog("[GetMonitorUSB]monitorInfo is null.");
            }
            _USBs = new List<InputSource_USB>();
            return false;
        }

        public bool SetMonitorUSB(MonitorInfo monitorInfo, InputSource_USB inputSourceUSB)
        {
            if (monitorInfo != null && inputSourceUSB != null)
            {
                int mi = _displayData.FindIndex(x => x.Model == monitorInfo.modelName &&
                                                     x.ServiceTag == monitorInfo.edid.ServiceTag);
                if (mi != -1)
                {
                    int index = _displayData[mi].DisplayUSB.FindIndex(x => x.inputSource == inputSourceUSB.inputSource);
                    if (index != -1)
                    {
                        _displayData[mi].DisplayUSB[index] = inputSourceUSB;
                        return true;
                    }
                    else
                    {
                        WriteLog("[SetMonitorUSB]_displayData is not find inputSource");
                        _displayData[mi].DisplayUSB.Add(inputSourceUSB);
                        return true;
                    }
                }
                else
                {
                    WriteLog("[SetMonitorUSB]_displayData is not find monitor");
                    DisplayData displayData = new DisplayData();
                    displayData.Model = monitorInfo.modelName;
                    displayData.ServiceTag = monitorInfo.edid.ServiceTag;
                    displayData.DisplayUSB.Add(inputSourceUSB);
                    _displayData.Add(displayData);
                    return true;
                }
            }
            else
            {
                WriteLog("[SetMonitorUSB]monitorInfo is null or inputSourceUSB is null.");
            }
            return false;
        }

        public bool SetMonitorUSB(MonitorInfo monitorInfo, List<InputSource_USB> inputSourceUSBs)
        {
            if (monitorInfo != null && inputSourceUSBs != null)
            {
                int mi = _displayData.FindIndex(x => x.Model == monitorInfo.modelName &&
                                                     x.ServiceTag == monitorInfo.edid.ServiceTag);
                if (mi != -1)
                {
                    _displayData[mi].DisplayUSB = inputSourceUSBs;
                    return true;
                }
                else
                {
                    WriteLog("[SetMonitorUSB]_displayData is not find monitor");
                    DisplayData displayData = new DisplayData();
                    displayData.Model = monitorInfo.modelName;
                    displayData.ServiceTag = monitorInfo.edid.ServiceTag;
                    displayData.DisplayUSB = inputSourceUSBs;
                    _displayData.Add(displayData);
                    return true;
                }
            }
            else
            {
                WriteLog("[SetMonitorUSB]monitorInfo is null.");
            }
            return false;
        }

        public bool GetMonitorInputSourceList(MonitorInfo monitorInfo, out List<InputCode> inputSourceList)
        {
            if (monitorInfo != null)
            {
                int mi = _displayData.FindIndex(x => x.Model == monitorInfo.modelName &&
                                                     x.ServiceTag == monitorInfo.edid.ServiceTag);
                if (mi != -1 &&
                    _displayData[mi].InputSourceList != null &&
                    _displayData[mi].InputSourceList.Count > 0)
                {
                    inputSourceList = _displayData[mi].InputSourceList;
                    return true;
                }
            }
            else
            {
                WriteLog("[GetMonitorInputSourceList]monitorInfo is null.");
            }
            inputSourceList = new List<InputCode>();
            return false;
        }

        public bool SetMonitorInputSourceList(MonitorInfo monitorInfo, List<InputCode> inputSourceList)
        {
            if (monitorInfo != null && inputSourceList != null)
            {
                int mi = _displayData.FindIndex(x => x.Model == monitorInfo.modelName &&
                                                     x.ServiceTag == monitorInfo.edid.ServiceTag);
                if (mi != -1)
                {
                    _displayData[mi].InputSourceList = inputSourceList;
                    return true;
                }
                else
                {
                    WriteLog("[SetMonitorUSB]_displayData is not find monitor");
                    DisplayData displayData = new DisplayData();
                    displayData.Model = monitorInfo.modelName;
                    displayData.ServiceTag = monitorInfo.edid.ServiceTag;
                    displayData.InputSourceList = inputSourceList;
                    _displayData.Add(displayData);
                    return true;
                }
            }
            else
            {
                WriteLog("[SetMonitorInputSourceList]monitorInfo is null.");
            }
            return false;
        }

        public bool GetMonitorUSBList(MonitorInfo monitorInfo, out List<USBPorts> usbList)
        {
            if (monitorInfo != null)
            {
                int mi = _displayData.FindIndex(x => x.Model == monitorInfo.modelName &&
                                                     x.ServiceTag == monitorInfo.edid.ServiceTag);
                if (mi != -1 &&
                    _displayData[mi].USBList != null &&
                    _displayData[mi].USBList.Count > 0)
                {
                    usbList = _displayData[mi].USBList;
                    return true;
                }
                else
                {
                    WriteLog("[GetMonitorUSBList]_displayData not find USBList.");
                }
            }
            else
            {
                WriteLog("[GetMonitorUSBList]monitorInfo is null.");
            }
            usbList = new List<USBPorts>();
            return false;
        }

        public bool SetMonitorUSBList(MonitorInfo monitorInfo, List<USBPorts> usbList)
        {
            if (monitorInfo != null && usbList != null)
            {
                int mi = _displayData.FindIndex(x => x.Model == monitorInfo.modelName &&
                                                     x.ServiceTag == monitorInfo.edid.ServiceTag);
                if (mi != -1)
                {
                    _displayData[mi].USBList = usbList;
                    return true;
                }
                else
                {
                    WriteLog("[SetMonitorUSB]_displayData is not find monitor");
                    DisplayData displayData = new DisplayData();
                    displayData.Model = monitorInfo.modelName;
                    displayData.ServiceTag = monitorInfo.edid.ServiceTag;
                    displayData.USBList = usbList;
                    _displayData.Add(displayData);
                    return true;
                }
            }
            else
            {
                WriteLog("[SetMonitorUSBList]monitorInfo is null.");
            }
            return false;
        }

        public bool GetMonitorE9(MonitorInfo monitorInfo, out uint vcpcode)
        {
            if (monitorInfo != null)
            {
                int mi = _displayData.FindIndex(x => x.Model == monitorInfo.modelName &&
                                                            x.ServiceTag == monitorInfo.edid.ServiceTag);
                if (mi != -1 && _displayData[mi].VCP_E9 != 1)
                {
                    vcpcode = _displayData[mi].VCP_E9;
                    return true;
                }
                else
                {
                    WriteLog("[GetMonitorE9]_displayData not find VCP_E9.");
                }
            }
            else
            {
                WriteLog("[GetMonitorE9]monitorInfo is null.");
            }
            vcpcode = 1;
            return false;
        }

        public bool SetMonitorE9(MonitorInfo monitorInfo, uint vcpcode)
        {
            if (monitorInfo != null)
            {
                if (vcpcode != 1 && vcpcode != 2)
                {
                    int mi = _displayData.FindIndex(x => x.Model == monitorInfo.modelName &&
                                                            x.ServiceTag == monitorInfo.edid.ServiceTag);
                    if (mi != -1)
                    {
                        _displayData[mi].VCP_E9 = vcpcode;
                        return true;
                    }
                }
            }
            else
            {
                WriteLog("[SetMonitorE9]monitorInfo is null.");
            }

            return false;
        }

        public bool GetMonitorDisplayPropertiesInfo(MonitorInfo monitorInfo, out DisplayPropertiesInfo displayPropertiesInfo)
        {
            if (monitorInfo != null)
            {
                int mi = _displayData.FindIndex(x => x.Model == monitorInfo.modelName &&
                                                     x.ServiceTag == monitorInfo.edid.ServiceTag);
                if (mi != -1 &&
                    _displayData[mi].DisplayPropertiesInfo != null &&
                    _displayData[mi].DisplayPropertiesInfo.SupportedProperties != null &&
                    _displayData[mi].DisplayPropertiesInfo.SupportedProperties.Properties != null &&
                    _displayData[mi].DisplayPropertiesInfo.SupportedProperties.Properties.Count > 0)
                {
                    displayPropertiesInfo = _displayData[mi].DisplayPropertiesInfo;
                    return true;
                }
            }
            else
            {
                WriteLog("[GetMonitorDisplayPropertiesInfo]monitorInfo is null.");
            }
            displayPropertiesInfo = new DisplayPropertiesInfo();
            return false;
        }

        public bool SetMonitorDisplayPropertiesInfo(MonitorInfo monitorInfo, DisplayPropertiesInfo displayPropertiesInfo)
        {
            if (monitorInfo != null && displayPropertiesInfo != null)
            {
                int mi = _displayData.FindIndex(x => x.Model == monitorInfo.modelName &&
                                                     x.ServiceTag == monitorInfo.edid.ServiceTag);
                if (mi != -1)
                {
                    _displayData[mi].DisplayPropertiesInfo = displayPropertiesInfo;
                    return true;
                }
                else
                {
                    WriteLog("[SetMonitorDisplayPropertiesInfo]_displayData is not find monitor");
                    DisplayData displayData = new DisplayData();
                    displayData.Model = monitorInfo.modelName;
                    displayData.ServiceTag = monitorInfo.edid.ServiceTag;
                    displayData.DisplayPropertiesInfo = displayPropertiesInfo;
                    _displayData.Add(displayData);
                    return true;
                }
            }
            else
            {
                WriteLog("[SetMonitorDisplayPropertiesInfo]monitorInfo is null.");
            }
            return false;
        }

        public bool GetMonitorGamingDisplayPropertiesInfo(MonitorInfo monitorInfo, out GamingDisplayPropertiesInfo gamingDisplayPropertiesInfo)
        {
            if (monitorInfo != null)
            {
                int mi = _displayData.FindIndex(x => x.Model == monitorInfo.modelName &&
                                                     x.ServiceTag == monitorInfo.edid.ServiceTag);
                if (mi != -1 && 
                    _displayData[mi].GamingDisplayPropertiesInfo != null &&
                    _displayData[mi].GamingDisplayPropertiesInfo.SupportedProperties != null &&
                    _displayData[mi].GamingDisplayPropertiesInfo.SupportedProperties.Properties != null &&
                    _displayData[mi].GamingDisplayPropertiesInfo.SupportedProperties.Properties.Count > 0)
                {
                    gamingDisplayPropertiesInfo = _displayData[mi].GamingDisplayPropertiesInfo;
                    return true;
                }
            }
            else
            {
                WriteLog("[GetMonitorDisplayPropertiesInfo]monitorInfo is null.");
            }
            gamingDisplayPropertiesInfo = new GamingDisplayPropertiesInfo();
            return false;
        }

        public bool SetMonitorGamingDisplayPropertiesInfo(MonitorInfo monitorInfo, GamingDisplayPropertiesInfo gamingDisplayPropertiesInfo)
        {
            if (monitorInfo != null && gamingDisplayPropertiesInfo != null)
            {
                int mi = _displayData.FindIndex(x => x.Model == monitorInfo.modelName &&
                                                     x.ServiceTag == monitorInfo.edid.ServiceTag);
                if (mi != -1)
                {
                    _displayData[mi].GamingDisplayPropertiesInfo = gamingDisplayPropertiesInfo;
                    return true;
                }
                else
                {
                    WriteLog("[SetMonitorGamingDisplayPropertiesInfo]_displayData is not find monitor");
                    DisplayData displayData = new DisplayData();
                    displayData.Model = monitorInfo.modelName;
                    displayData.ServiceTag = monitorInfo.edid.ServiceTag;
                    displayData.GamingDisplayPropertiesInfo = gamingDisplayPropertiesInfo;
                    _displayData.Add(displayData);
                    return true;
                }
            }
            else
            {
                WriteLog("[SetMonitorGamingDisplayPropertiesInfo]monitorInfo is null.");
            }
            return false;
        }

        public bool GetColor(MonitorInfo monitorInfo, bool isHDR, out string color)
        {
            if (GlobalDefinitions.enableCurrentColorCache)
            {
                if (monitorInfo != null)
                {
                    int mi = _displayData.FindIndex(x => x.Model == monitorInfo.modelName &&
                                                         x.ServiceTag == monitorInfo.edid.ServiceTag);
                    if (mi != -1)
                    {
                        WriteLog("[GetColor]modelName : " + monitorInfo.modelName);
                        WriteLog("[GetColor]ServiceTag : " + monitorInfo.edid.ServiceTag);
                        if (isHDR)
                        {
                            WriteLog("[GetColor]isHDR");
                            color = _displayData[mi].Color.color_EnHDR;
                        }
                        else
                        {
                            WriteLog("[GetColor]Not HDR");
                            color = _displayData[mi].Color.color_DisHDR;
                        }
                        WriteLog("[GetColor]color : " + color);
                        if (color != string.Empty)
                        {
                            return true;
                        }
                        else
                        {
                            WriteLog("[GetColor]_displayData is not get color");
                            return false;
                        }
                    }
                    else
                    {
                        WriteLog("[GetColor]_displayData is not find monitor");
                    }
                }
                else
                {
                    WriteLog("[GetColor]monitorInfo is null.");
                }
            }
            color = string.Empty;
            return false;
        }

        public bool SetColor(MonitorInfo monitorInfo, bool isHDR, string setColor)
        {
            if (monitorInfo != null && !string.IsNullOrEmpty(setColor))
            {
                int mi = _displayData.FindIndex(x => x.Model == monitorInfo.modelName &&
                                                     x.ServiceTag == monitorInfo.edid.ServiceTag);
                if (mi != -1)
                {
                    WriteLog("[SetColor]modelName : " + monitorInfo.modelName);
                    WriteLog("[SetColor]ServiceTag : " + monitorInfo.edid.ServiceTag);
                    if (isHDR)
                    {
                        WriteLog("[SetColor]isHDR");
                        _displayData[mi].Color.color_EnHDR = setColor;
                    }
                    else
                    {
                        WriteLog("[SetColor]Not HDR");
                        _displayData[mi].Color.color_DisHDR = setColor;
                    }
                    WriteLog("[SetColor]setColor : " + setColor);
                    return true;
                }
                else
                {
                    WriteLog("[SetColor]_displayData is not find monitor");
                }
            }
            else
            {
                WriteLog("[SetColor]monitorInfo is null.");
            }
            return false;
        }
    }
}