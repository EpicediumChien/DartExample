using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VcpCore.Common;
using DDPM.SA.Common.Display;
using static VcpCore.Common.User32;

namespace DDPM.SA.Plugins.User.DisplayManager
{
    public class DisplayDataManger
    {
        public List<DisplayData> _displayData { get; set; } = new List<DisplayData>();

        public void InitDisplayData(List<MonitorInfo> monitorInfos)
        {
            if (monitorInfos != null)
            {
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
        }
        public bool GetMonitorUSB(MonitorInfo monitorInfo, string inputSource, out string USB)
        {
            if (monitorInfo != null && !string.IsNullOrEmpty(inputSource))
            {
                int mi = _displayData.FindIndex(x => x.Model == monitorInfo.modelName &&
                                                     x.ServiceTag == monitorInfo.edid.ServiceTag);
                if (mi != -1)
                {
                    int index = _displayData[mi].DisplayUSB.FindIndex(x => x.inputSource == inputSource);
                    if (index != -1)
                    {
                        USB = _displayData[mi].DisplayUSB[index].USB;
                        return true;
                    }
                }
            }
            USB = string.Empty;
            return false;
        }
        public bool GetMonitorUSB(MonitorInfo monitorInfo, out List<InputSource_USB> _USBs)
        {
            _USBs = new List<InputSource_USB>();
            return false;
        }
        public bool SetMonitorUSB(MonitorInfo monitorInfo, string inputSource)
        {
            return false;
        }
    }
}
