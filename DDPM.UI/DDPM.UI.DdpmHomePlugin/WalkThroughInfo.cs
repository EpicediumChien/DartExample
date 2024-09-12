using DDPM.SA.Common;
using Dell.Client.Framework.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDPM.UI.Plugin.DdpmHomePlugin
{
    class WalkThroughInfo
    {
        // 定義一個公共佇列讓其他類別可以訪問
        public static Queue<DeviceInfo> DeviceQueue { get; private set; } = new Queue<DeviceInfo>();

        private readonly IDeviceManagerSA _deviceManager;
        private readonly string _userId;
        private readonly ILog _log;

        public WalkThroughInfo(IDeviceManagerSA deviceManager, string userId, ILog log)
        {
            _deviceManager = deviceManager;
            _userId = userId;
            _log = log;

            // 初始化設備變更事件
            _deviceManager.DeviceChanged += _deviceManager_DeviceChanged;
        }

        // 蒐集當前已連接的設備並比對註冊表值
        public async Task CollectAndCompareDevicesAsync()
        {
            try
            {
                var deviceHelper = await _deviceManager.GetDevices();
                if (deviceHelper?.deviceInfo != null)
                {
                    foreach (var device in deviceHelper.deviceInfo)
                    {
                        await CheckAndQueueDevice(device);
                    }
                }
            }
            catch (Exception ex)
            {
                _log.Error($"Error collecting devices: {ex.Message}");
            }
        }

        // 當設備變更時觸發的事件
        private async void _deviceManager_DeviceChanged(object sender, DeviceChangedEventArgs e)
        {
            if (e?.device_peripherals != null)
            {
                // 當偵測到設備插入或拔出時，檢查並處理該設備
                await CheckAndQueueDevice(e.device_peripherals);
            }
        }

        // 檢查設備是否已完成 WalkThrough，並將新設備加入佇列
        private async Task CheckAndQueueDevice(DeviceInfo device)
        {
            string regPath = $@"SOFTWARE\Dell\Dell Peripheral Manager\UserSettings\Local\{_userId}";
            string regKey = $"IsFirstTimeWalkThroughDone_com.dell.DPM.Plugin.LogicalDevice.{device.ModelNumber}";

            try
            {
                // 讀取註冊表中的值
                object regValue = await _deviceManager.ReadRegistryData(DDPM.SA.Common.Settings.RegistryHive.LocalMachine, regPath, regKey);

                if (regValue is bool isWalkThroughDone && isWalkThroughDone)
                {
                    // 若註冊表中 WalkThrough 已完成，將設備加入佇列
                    DeviceQueue.Enqueue(device);
                    _log.Info($"Device {device.ModelNumber} added to the queue.");

                    // 將註冊表中的值設為 false，表示設備已處理
                    await _deviceManager.WriteRegistryData(DDPM.SA.Common.Settings.RegistryHive.LocalMachine, regPath, regKey, false);
                }
            }
            catch (Exception ex)
            {
                _log.Error($"Error processing device {device.ModelNumber}: {ex.Message}");
            }
        }
    }
}
