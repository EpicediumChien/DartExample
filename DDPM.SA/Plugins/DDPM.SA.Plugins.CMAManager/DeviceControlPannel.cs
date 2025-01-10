using DDPM.RemoteManagement.Common.Interfaces;
using DDPM.SA.Common;
using System;
using System.Collections.Generic;
using VcpCore.Common;

namespace DDPM.SA.Plugins.CMAManager
{
    public class DeviceControlPannel
    {

        private const string DEVICE_DISPLAY = "display";
        private const string DEVICE_PERIPHERAL = "peripheral";

        private List<MonitorInfo> monitors;
        private List<DeviceInfo> deivces;

        public DeviceControlPannel()
        {
            monitors = new List<MonitorInfo>();
            deivces = new List<DeviceInfo>();
        }

        public NotifyArgs OnDeviceChnaged(CMADeviceChanges _CMADeviceChanges)
        {
            NotifyArgs args = new NotifyArgs();

            if (_CMADeviceChanges == null)
            {
                args.eventType = Params.EventType.UNKNOWN_ERROR.ToString();
                args.notification = "Device data is null.";

                return args;

            }

            if (_CMADeviceChanges.type.ToLower().Equals(DEVICE_DISPLAY))
            {
                args = CheckDisplay(_CMADeviceChanges.mos);
            }

            if (_CMADeviceChanges.type.ToLower().Equals(DEVICE_PERIPHERAL))
            {
                args = CheckPeripheral(_CMADeviceChanges.devices);
            }

            return args;
        }

        private NotifyArgs CheckDisplay(List<MonitorInfo> list)
        {
            NotifyArgs args = new NotifyArgs();

            /*            if (list == null) 
                        {
                            args.eventType = Params.EventType.DISPLAY_CONNECT.ToString();
                            args.notification = list.ToString();

                            return args;
                        }*/

            if (list.Count > monitors.Count)
            {
                args.eventType = Params.EventType.DISPLAY_CONNECT.ToString();
                args.notification = getMosDiffer(list, monitors);
            }
            else
            {
                args.eventType = Params.EventType.DISPLAY_DISCONNECT.ToString();
                args.notification = getMosDiffer(monitors, list);
            }

            monitors = list;


            return args;

        }

        private NotifyArgs CheckPeripheral(List<DeviceInfo> list)
        {
            NotifyArgs args = new NotifyArgs();

            return args;
        }

        private string getMosDiffer(List<MonitorInfo> src, List<MonitorInfo> des)
        {
            string result = string.Empty;

            List<MonitorInfo> diff = new List<MonitorInfo>();

            foreach (MonitorInfo info in src)
            {
                Boolean isExist = false;

                foreach (MonitorInfo other in des)
                {
                    if (other.Equals(info))
                    {
                        isExist = true;
                        break;
                        ;
                    }
                }

                if (!isExist)
                {
                    diff.Add(info);
                }
            }

            if (diff.Count > 0)
            {
                ItemMonitor item;
                foreach (MonitorInfo info in diff)
                {
                    item = new ItemMonitor()
                    {
                        Index = info.Index,
                        devicetype = Params.DeviceType.DISPLAY,
                        modelname = info.edid.ModelName,
                        fwversion = info.FwVersion,
                        pid = info.edid.PID,
                        serialnumber = info.edid.SerialNumber,
                        servicetag = info.edid.ServiceTag
                    };

                    result = result + "{" + item.ToString() + "}";
                }

            }

            return result;
        }




        private class ItemMonitor
        {
            public int Index { get; set; }
            public string devicetype { get; set; }
            public string modelname { get; set; }

            public string fwversion { get; set; }
            public string pid { get; set; }
            public string serialnumber { get; set; }
            public string servicetag { get; set; }


            public string ToString()
            {
                string result = string.Empty;
                bool hasData = false;

                //if (Index != null)
                //{
                //    if (hasData)
                //    {
                //      result = result + ",";
                //    }
                //
                //    result = result + $"\"Index\":{Index}";
                //    hasData = true;
                //}

                if (!String.IsNullOrEmpty(devicetype)) //as 1st item, hasData should be empty [Dean]
                {
                    //if (hasData)
                    //{
                    //    result = result + ",";
                    //}

                    result = result + $"\"DeviceType\":\"{devicetype}\"";
                    hasData = true;
                }

                if (!String.IsNullOrEmpty(modelname))
                {
                    if (hasData)
                    {
                        result = result + ",";
                    }

                    result = result + $"\"Model\":\"{modelname}\"";
                    hasData = true;
                }

                if (!String.IsNullOrEmpty(fwversion))
                {
                    if (hasData)
                    {
                        result = result + ",";
                    }

                    result = result + $"\"FirmwareVersion\":\"{fwversion}\"";
                    hasData = true;
                }

                if (!String.IsNullOrEmpty(pid))
                {
                    if (hasData)
                    {
                        result = result + ",";
                    }

                    result = result + $"\"PID\":\"{pid}\"";
                    hasData = true;
                }

                if (!String.IsNullOrEmpty(serialnumber))
                {
                    if (hasData)
                    {
                        result = result + ",";
                    }

                    result = result + $"\"SerialNumber\":\"{serialnumber}\"";
                    hasData = true;
                }

                if (!String.IsNullOrEmpty(servicetag))
                {
                    if (hasData)
                    {
                        result = result + ",";
                    }

                    result = result + $"\"ServiceTag\":\"{servicetag}\"";
                    hasData = true;
                }


                return result;
            }
        }

    }
}
