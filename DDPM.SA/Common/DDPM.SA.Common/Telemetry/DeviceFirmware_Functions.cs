using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VcpCore.Common;

namespace DDPM.SA.Common.Telemetry
{
    public class DeviceFirmware_Functions
    {
        public bool Send_DisplayDeviceFirmware_Telementry(ITelementryScheduler plugin, List<FWUpdateInfo> fWUpdateInfos)
        {
            var rt = false;
            if (fWUpdateInfos != null)
            {
                DisplayFirmware_List TelemetryDta_DisplayDeviceFirmware = new DisplayFirmware_List();
                TelemetryDta_DisplayDeviceFirmware.DisplayFirmwareList = new List<DisplayFirmware>();
                foreach (FWUpdateInfo fWUpdateInfo in fWUpdateInfos)
                {
                    if (fWUpdateInfo.IsDisplay)
                    {
                        DisplayFirmware displayFirmware = new DisplayFirmware();
                        displayFirmware.FWVersionFrom = fWUpdateInfo.DeviceVersion;
                        displayFirmware.FWVersionTo = fWUpdateInfo.TheLatestVersion;
                        displayFirmware.Results = fWUpdateInfo.FWUErrorCode == FWUErrorCode.NoError ? "success" : "failed";
                        displayFirmware.DisplayModelname = fWUpdateInfo.Model;
                        displayFirmware.DisplayServiceTag = fWUpdateInfo.ServiceTag;
                        displayFirmware.D_Ctrl = fWUpdateInfo.D_Ctrl;
                        displayFirmware.SupplierID = fWUpdateInfo.SupplierID;
                        displayFirmware.FW_Available_date = fWUpdateInfo.Available_date;
                        displayFirmware.FW_Update_date = fWUpdateInfo.Update_date;
                        TelemetryDta_DisplayDeviceFirmware.DisplayFirmwareList.Add(displayFirmware);
                    }
                }
                if (plugin != null)
                    rt = plugin.ReceiveTelemetryInfo("MonitorFW_Update", TelemetryDta_DisplayDeviceFirmware.ToJson(), Telementry_Frequency.RealTime).Result;
            }
            return rt;
        }
        public bool Send_PeripheralsDeviceFirmware_Telementry(ITelementryScheduler plugin, List<FWUpdateInfo> fWUpdateInfos)
        {
            var rt = false;
            if (fWUpdateInfos != null)
            {
                PeripheralsFirmware_List TelemetryDta_PeripheralsDeviceFirmware = new PeripheralsFirmware_List();
                TelemetryDta_PeripheralsDeviceFirmware.PeripheralsFirmwareList = new List<PeripheralsFirmware>();
                foreach (FWUpdateInfo fWUpdateInfo in fWUpdateInfos)
                {
                    if (!fWUpdateInfo.IsDisplay)
                    {
                        PeripheralsFirmware peripheralsFirmware = new PeripheralsFirmware();
                        peripheralsFirmware.FWVersionFrom = fWUpdateInfo.DeviceVersion;
                        peripheralsFirmware.FWVersionTo = fWUpdateInfo.TheLatestVersion;
                        peripheralsFirmware.Results = fWUpdateInfo.FWUErrorCode == FWUErrorCode.NoError ? "success" : "failed";
                        peripheralsFirmware.DeviceModelNumber = fWUpdateInfo.Model;
                        peripheralsFirmware.Connectivity = fWUpdateInfo.Connectivity;
                        peripheralsFirmware.SupplierID = fWUpdateInfo.SupplierID;
                        peripheralsFirmware.FW_Available_date = fWUpdateInfo.Available_date;
                        peripheralsFirmware.FW_Update_date = fWUpdateInfo.Update_date;
                        TelemetryDta_PeripheralsDeviceFirmware.PeripheralsFirmwareList.Add(peripheralsFirmware);
                    }
                }
                if (plugin != null)
                    rt = plugin.ReceiveTelemetryInfo("Complex", TelemetryDta_PeripheralsDeviceFirmware.ToJson(), Telementry_Frequency.RealTime).Result;
            }
            return rt;
        }
    }
}
