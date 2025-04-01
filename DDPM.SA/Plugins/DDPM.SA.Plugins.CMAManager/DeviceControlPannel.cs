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

        private static List<MonitorInfo> monitors;
        private static List<DeviceInfo> deivces;

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

        // add @ 20250212 stephen
        private bool compare(MonitorInfo info1, MonitorInfo info2)
        {
            bool result = false;

            if (info1.edid.ServiceTag.ToLower().Equals(info2.edid.ServiceTag.ToLower()))
            {
                result = true;
            }

            return result;
        }

        private string getMosDiffer(List<MonitorInfo> src, List<MonitorInfo> des)
        {
            string result = string.Empty;

            int count = 0; // add @ 2020328 stephen

            List<MonitorInfo> diff = new List<MonitorInfo>();

            foreach (MonitorInfo info1 in src)
            {
                Boolean isExist = false;

                foreach (MonitorInfo info2 in des)
                {
                    // modified @ 20250212 stephen
                    if (compare(info1, info2))
                    {
                        isExist = true;
                        break;
                        ;
                    }
                }

                if (!isExist)
                {
                    diff.Add(info1);
                }
            }

            if (diff.Count > 0)
            {
                ItemMonitor item;
                count = 0;  // add @ 20250328 stephen
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

                    // add @ 20250327 stephen
                    if (count > 0)
                    {
                        result = result + ",\n";
                    }

                    result = result + "{" + item.ToString() + "}";
                    count++;
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


            public override string ToString()
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
