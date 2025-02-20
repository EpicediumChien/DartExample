using DDPM.SA.Common;
using DDPM.UI.Common;
using Dell.Client.Framework.Common;
using Dell.Client.Framework.UX.WPF;
using Microsoft;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Windows.Input;

namespace DDPM.UI.Plugin.ViewModels
{
    public class DockPageViewModel : PeripheralViewModel, INotifyPropertyChanged
    {
        #region Variables

        private readonly ILog _log;
        private readonly IDeviceManagerSA _deviceManager;
        private bool _isEnableUpdate = false;

        #endregion Variables

        public new event PropertyChangedEventHandler? PropertyChanged;

        public ICommand TabOffClickedCommand { get; }
        public ICommand TabAdaptiveLightClickedCommand { get; }
        public ICommand TabManualClickedCommand { get; }

        //0614 Bruce 判斷是否需要顯示更新按鈕
        public bool IsEnableUpdate { get => _isEnableUpdate; }

        public DockPageViewModel(IConsole console, ILog log, IDeviceManagerSA deviceManager) : base(console, log, deviceManager)
        {
            Requires.NotNull(console, nameof(console));
            Requires.NotNull(log, nameof(log));

            _log = log;
            _deviceManager = deviceManager;
        }

        public override void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void PrepareDeviceInfo(List<DeviceInfo> deviceInfos)
        {
            DeviceInfos.Clear();
            foreach (DeviceInfo deviceInfo in deviceInfos)
            {
                if (deviceInfo.LogicalDeviceType.Contains("Dock") &&
                    !DeviceInfos.ContainsKey(deviceInfo.ID))
                {
                    DeviceInfos.Add(deviceInfo.ID, deviceInfo);
                }
            }
        }

        public override bool SetCurrentDevice(string instanceIDs)
        {
            if (!base.SetCurrentDevice(instanceIDs))
                return false;
            //0821 Bruce Add show Dock Fw Version
            Model = Model.Replace("_", " ");
            //_deviceManager.GetDockData(instanceID).Wait();
            FirmwareVersion2 = $"{Strings.FirmwareVersion} {CurrentDeviceInfo.FirmwareVersion}";
            //if (!string.IsNullOrEmpty(CurrentDeviceInfo.DockPackageFwVersion))
            //{
            //    var fv = Regex.Replace(CurrentDeviceInfo.DockPackageFwVersion, @"(\d{2})(?=\d)", "$1.");
            //    FirmwareVersion2 += $" {fv}";
            //}
            FirmwareVersion2 += $"\n{Strings.ServiceTag} {CurrentDeviceInfo.DockServiceTag}";
            //if (!string.IsNullOrEmpty(CurrentDeviceInfo.DockServiceTag))
            //{
            //    FirmwareVersion2 += $" {CurrentDeviceInfo.DockServiceTag}";
            //}
            FWUpdateInfoPackage fwUpdateInfoPackage = _deviceManager.GetFWUpdateInfo(false, false).Result;
            _isEnableUpdate = false;
            foreach (FWUpdateInfo fWUpdateInfo in fwUpdateInfoPackage.FWUpdateInfo)
            {
                if (!fWUpdateInfo.IsDisplay)
                {
                    if (fWUpdateInfo.DeviceId.Replace("{", "").Replace("}", "").Equals(instanceIDs))
                    {
                        _isEnableUpdate = true;
                        break;
                    }
                }
            }
            return true;
        }

        public override void HandleNotification(DeviceChangedType changeType, DeviceInfo di, string property = "")
        {
            base.HandleNotification(changeType, di, property);
            switch (changeType)
            {
                case DeviceChangedType.Peripherals_SettingsChange:
                    if (DeviceInfos.ContainsKey(di.ID))
                    {
                        DeviceInfos.Remove(di.ID);
                        DeviceInfos.Add(di.ID, di);
                    }
                    else
                    {
                        return;
                    }
                    if (di.ID == CurrentDeviceID)
                    {
                        CurrentDeviceInfo = DeviceInfos[CurrentDeviceID];

                        //GenerateInfo();
                    }
                    break;

                default:
                    break;
            }
        }
    }
}