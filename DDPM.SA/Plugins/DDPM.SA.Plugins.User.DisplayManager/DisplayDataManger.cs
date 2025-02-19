using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VcpCore.Common;
using DDPM.SA.Common.Display;
using static VcpCore.Common.User32;
using DDPM.SA.Common.Settings;
using DDPM.SA.Common;
using Dell.Client.Framework.Common;
using Microsoft.Toolkit.Uwp.Notifications;
using System.Diagnostics;

namespace DDPM.SA.Plugins.User.DisplayManager
{
    public class DisplayDataManger
    {
        public List<DisplayData> _displayData { get; set; } = new List<DisplayData>();

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
                        return true;
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
                if (mi != -1)
                {
                    _USBs = _displayData[mi].DisplayUSB;
                    return true;
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
                    return true;
                }
            }
            else
            {
                WriteLog("[SetMonitorUSB]monitorInfo is null.");
            }
            return false;
        }
    }
}
