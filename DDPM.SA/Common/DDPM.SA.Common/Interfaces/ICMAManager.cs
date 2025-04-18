using DDPM.RemoteManagement.Common.Interfaces;
using Dell.Client.Framework.Common;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Documents;
using VcpCore.Common;
using static DDPM.SA.Common.ICLICommandTable;

namespace DDPM.SA.Common
{
    #region Data definitions
    //for ICMAManagerSA that CMAProxy register CMAManager to get request over "CMARequestEvent" and feedback result by call "WriteResult"
    public class CMAEventArgs
    {
        public string input_param;
        public Guid cma_request_id;
    }
       

    public class CMADeviceChanges
    {
        public string type = string.Empty; //display or peripheral (lower case)
        public List<MonitorInfo> mos = null;
        public List<DeviceInfo> devices = null;
    }

    // add @ 2050417 stephen : object for device infoemation
    public class CmaDeviceInfo
    {
        public string type { get; set; } = string.Empty;
        public string guid { get; set; } = string.Empty;
        public string model { get; set; } = string.Empty;
        public string serialnumber { get; set; } = string.Empty;
        public string fwversion { get; set; } = string.Empty;

    }
    #endregion

    public interface ICMAManagerSA : IFrameworkPlugin
    {
        Task WriteResult(RemoteManagementResult result);
        event EventHandler<CMAEventArgs> CMARequestEvent;
        Task Update_DeviceChanged(CMADeviceChanges data);

        // add @ 20241129 stephen for receive fw update result
        Task UpdateFwStatus(List<FWUpdateInfo> datas);

        // add @ 20250417 stephen : for get device infomation
        public Task UpdateDtpDeviceInfo(CmaDeviceInfo data);
    }

    public interface ICMAProxy : IFrameworkPlugin
    {
        //no action need
    }

    
}