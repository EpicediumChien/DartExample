using DDPM.SA.Common;
using DDPM.UI.Common.Models;
using System.Collections.ObjectModel;
using VcpCore.Common;

namespace DDPM.UI.Plugin.DdpmHomePlugin.Interfaces
{
    internal interface IDdpmHomePageViewModel
    {
        /// <summary>
        /// Input list of MonitorInfo, and add to HomeDevices
        /// </summary>
        /// <param name="monitorInfos"></param>
        public void PrepareMonitorInfos(List<MonitorInfo> monitorInfos);

        public void PrepareDeviceInfos(List<DeviceInfo> deviceInfos);

        public void ResetDevices();

        //All devices
        public ObservableCollection<HomeDevice> HomeDevices { get; set; }

        public HomeDevice? SelectedHomeDevice { get; set; }

        //IDeviceManagerSA DevMgrSA { get; set; }
        //List<MonitorInfo> MonitorInfos { get; set; }

        //Debug purpose
        public void AddDemoHomeDevice(HomeDevice dev);

        public double cxItem { get; set; }

        public event EventHandler HomeDevicesChanged;

        public void RefreshCollectionView();

        //Robert_Lin, 2024-7-3 added for showing Add your first device
        public void RaiseShowAddDevicePlugin();

        public void Invoke_PleaseWait();

        public bool IsPleaseWaitVisible { get; set; }
        public void DumpDevicesToLog();
        public bool IsSmallScreenResolution { get; set; }
    }
}