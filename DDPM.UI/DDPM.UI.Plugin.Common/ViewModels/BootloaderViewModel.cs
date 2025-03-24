using DDPM.SA.Common;
using DDPM.UI.Common;
using Dell.Client.Framework.Common;
using Dell.Client.Framework.UX.WPF;
using DPeMPublic.Common.Enums;
using Microsoft;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Windows.Input;

namespace DDPM.UI.Plugin.ViewModels
{
    public class BootloaderViewModel : PeripheralViewModel, INotifyPropertyChanged
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

        public BootloaderViewModel(IConsole console, ILog log, IDeviceManagerSA deviceManager) : base(console, log, deviceManager)
        {
            Requires.NotNull(console, nameof(console));
            Requires.NotNull(log, nameof(log));

            _log = log;
            _deviceManager = deviceManager;
            _log!.Info($"[BootloaderViewModel] BootloaderViewModel Start ...");
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
                if ((deviceInfo.PhysicalDeviceType.Equals(DeviceType.LogicalBootloader) || deviceInfo.PhysicalDeviceType.Equals(DeviceType.PhysicalBootloader)) &&
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
            /*FWUpdateInfoPackage fwUpdateInfoPackage = _deviceManager.GetFWUpdateInfo(false).Result;
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
            }*/
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