using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DDPM.SA.Common;
using DDPM.SA.Common.Settings;
using DDPM.UI.Common;
using DDPM.UI.Common.Models;
using DDPM.UI.Common.Views;
using DDPM.UI.Interfaces;
using Dell.Client.Framework.Common;
using Dell.Client.Framework.UX.WPF;
using DPeMPublic.Common.Enums;
using Microsoft;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using MessageBox = System.Windows.MessageBox;
using UserControl = System.Windows.Controls.UserControl;

namespace DDPM.UI.Plugin.ViewModels
{
    public class PeripheralViewModel : ObservableObject, IPeripheralViewModel
    {
        #region Variables

        private readonly IConsole _console;
        private readonly ILog _log;
        private readonly IDeviceManagerSA _deviceManager;
        private string _name = "";
        private string _model = "";
        private string _imageFilePath = "";
        private string _imageSFilePath = "";
        private string _firmwareVersion = "";
        private string _firmwareVersion2 = "";
        private string _rightFrameVisibility = "";
        private double _batteryLevel = 0;
        private string _batteryStatus = "";
        private string _connectionType = "";
        private int _rightFrameWidthFrom = 0;
        private int _rightFrameWidthTo = 0;
        public bool IsSliderDragging = false;
        private DispatcherTimer timer;
        private string instenceNo = "";

        #endregion Variables

        public DeviceInfo? CurrentDeviceInfo;

        public ICommand GoBackClickedCommand { get; private set; }
        public ICommand ShowInfoClickedCommand { get; private set; }
        public volatile Dictionary<Guid, DeviceInfo> DeviceInfos = new();
        public List<string> EOLList = new() { "WK636", "WK717", "KM714", "KM717", "WM126", "WM116", "WM326", "WM527", "WM514", "UV514" };
        //public DDPMSettings? DDPMSettings;
        //public WebcamSettings WebcamSettings = new();
        public PeripheralViewModel(IConsole console, ILog log, IDeviceManagerSA deviceManager)
        {
            Requires.NotNull(console, nameof(console));
            Requires.NotNull(log, nameof(log));
            Requires.NotNull(log, nameof(deviceManager));

            _console = console;
            _log = log;
            _deviceManager = deviceManager;

            GoBackClickedCommand = new RelayCommand(OnGoBackClicked);
            ShowInfoClickedCommand = new RelayCommand(OnShowInfoClicked);
            timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(0.5)
            };
            timer.Tick += Timer_Tick;
        }

        public void Unpair()
        {
            _deviceManager.UnPair(CurrentDeviceID);
            OnGoBackClicked();
        }

        public virtual void OnGoBackClicked()
        {
            VbarSelectedIndex = -1;
            _console.ShowHomePage();
        }

        private void OnShowInfoClicked()
        {
            MessageBox.Show(DeviceInfo, Name, MessageBoxButton.OK, MessageBoxImage.Information);
        }

        public Visibility MultiDevicesInfoVisibility { get; set; } = Visibility.Collapsed;
        public Visibility CopilotInfoVisibility { get; set; } = Visibility.Collapsed;

        private void CheckMultiDevice()
        {
            foreach (var info in DeviceInfos.Values)
            {
                if (info.ModelNumber == Model && info.ID != CurrentDeviceID)
                {
                    MultiDevicesInfoVisibility = Visibility.Visible;
                    OnPropertyChanged(nameof(MultiDevicesInfoVisibility));
                    return;
                }
            }
            MultiDevicesInfoVisibility = Visibility.Collapsed;
            OnPropertyChanged(nameof(MultiDevicesInfoVisibility));
        }

        public bool IsIDInvalid = false;

        public virtual bool SetCurrentDevice(string deviceID)
        {
            IsIDInvalid = false;
            if (deviceID.Substring(deviceID.Length - 2, 1) == "-")
            {
                instenceNo = deviceID.Substring(deviceID.Length - 1, 1);
                deviceID = deviceID.Substring(0, deviceID.Length - 2);
            }
            else
            {
                instenceNo = "";
            }
            CurrentDeviceID = new Guid(deviceID);

            if (DeviceInfos.ContainsKey(CurrentDeviceID))
            {
                var di = DeviceInfos[CurrentDeviceID];
                _log.Info($"[PeripheralViewModel] SetCurrentDevice ... InstanceId = {di.InstanceId.ToString()}");
                if (di.DeviceName == "Headset Settings" || di.DeviceName == "Wired Audio Settings")
                {
                    CurrentInstanceID = di.ID.GetHashCode();
                }
                else
                {
                    CurrentInstanceID = di.InstanceId;
                }
                if (di.PhysicalDeviceType == DPeMPublic.Common.Enums.DeviceType.PhysicalDongle || di.PhysicalDeviceType == DPeMPublic.Common.Enums.DeviceType.PhysicalAudioDongle)
                {
                    foreach (var info in DeviceInfos.Values)
                    {
                        if (info.InstanceId == CurrentInstanceID && info.PhysicalDeviceType == DPeMPublic.Common.Enums.DeviceType.PhysicalBluetooth)
                            CurrentDeviceID = info.ID;
                    }
                }
                CurrentDeviceInfo = DeviceInfos[CurrentDeviceID];
            }
            else
            {
                OnGoBackClicked();
                IsIDInvalid = true;
                return false;
            }

            {
                //var arr = CurrentDeviceInfo.Name.Split(' ');
                //if (arr.Length > 0)
                //{
                //    Model = arr[arr.Length - 1];
                //}
                //else
                //{
                Model = CurrentDeviceInfo.ModelNumber;
                //}
                //ID = CurrentDeviceInfo.ID.Replace(CurrentDeviceInfo.ModelNumber, "").Trim();
                Name = CurrentDeviceInfo.Name;
            }
            if (instenceNo == "")
            {
                Model2 = Model;
            }
            else
            {
                Model2 = $"{Model} ({instenceNo})";
            }

            var colorCode = CurrentDeviceInfo.ColorCode == 0 ? "" : $"_{CurrentDeviceInfo.ColorCode}";
            ImageFilePath = $"/DDPM.UI.Resources;component/Resources/Images/{Model}{colorCode}.png";
            FirmwareVersion = CurrentDeviceInfo.FirmwareVersion;
            var fv = CurrentDeviceInfo.FirmwareVersion.PadLeft(4, '0');
            FirmwareVersion2 = $"Firmware Version {fv.Substring(0, 1)}.{fv.Substring(1, 1)}.{fv.Substring(2, 1)}.{fv.Substring(3, 1)}";
            //ConnectionType = CurrentDeviceInfo.PhysicalDeviceType.ToString() == "PhysicalDongle" ? "Dongle" : "Bluetooth";
            switch (CurrentDeviceInfo.PhysicalDeviceType)
            {
                case DeviceType.PhysicalWebcam:
                    ConnectionType = "Wired";
                    break;
                case DeviceType.PhysicalAudioDongle:
                case DeviceType.PhysicalBluetoothAudio:
                    ConnectionType = CurrentDeviceInfo.PhysicalDeviceType.ToString().Replace("Physical", "").Replace("Audio", "");
                    break;

                case DeviceType.PhysicalBluetooth:
                case DeviceType.PhysicalDongle:
                    ConnectionType = CurrentDeviceInfo.PhysicalDeviceType.ToString().Replace("Physical", "");
                    break;

                case DeviceType.PhysicalPen:
                    ConnectionType = "Bluetooth";
                    break;
                //0617 Bruce 新增Dock連線方式的濾字串的方式
                case DeviceType.PhysicalWiredDock:
                    if (CurrentDeviceInfo.ModelNumber.ToString().Contains("TB 5"))
                    {
                        ConnectionType = "USB-C (TB 5)";
                    }
                    else if (CurrentDeviceInfo.ModelNumber.ToString().Contains("TB4"))
                    {
                        ConnectionType = "USB-C (TB 4)";
                    }
                    else if (CurrentDeviceInfo.ModelNumber.ToString().Contains("DCS"))
                    {
                        ConnectionType = "Dual USB-C (DP 1.4)";
                    }
                    else
                    {
                        ConnectionType = "USB-C (DP 1.4)";
                    }
                    break;
                case DeviceType.PhysicalWiredAudio:
                    ConnectionType = CurrentDeviceInfo.PhysicalDeviceType.ToString().Replace("Physical", "").Replace("Speaker", "");
                    break;

                default:
                    ConnectionType = CurrentDeviceInfo.PhysicalDeviceType.ToString().Replace("Physical", "");
                    break;
            }
            fv = CurrentDeviceInfo.PhysicalDeviceFirmwareVersion.PadLeft(4, '0');
            PhysicalDeviceFWVersion = $"{fv.Substring(0, 1)}.{fv.Substring(1, 1)}.{fv.Substring(2, 1)}.{fv.Substring(3, 1)}";
            //BatteryLevel = CurrentDeviceInfo.BatteryLevel < 0 ? 0 : CurrentDeviceInfo.BatteryLevel;
            BatteryLevel = CurrentDeviceInfo.BatteryLevel;
            BatteryStatus = CurrentDeviceInfo.BatteryStatus;
            //BatteryStatus = "Charging";
            IsCollabsKeysSupported = CurrentDeviceInfo.IsCollabsKeysSupported;
            IsIlluminationSupported = CurrentDeviceInfo.IsIlluminationSupported;
            PairedHostName1 = CurrentDeviceInfo.PairedHostName1;
            PairedHostName2 = CurrentDeviceInfo.PairedHostName2;
            PairedHostName3 = CurrentDeviceInfo.PairedHostName3;
            VisiblePairedHostName1 = CurrentDeviceInfo.VisiblePairedHostName1;
            VisiblePairedHostName2 = CurrentDeviceInfo.VisiblePairedHostName2;
            VisiblePairedHostName3 = CurrentDeviceInfo.VisiblePairedHostName3;

            CheckMultiDevice();
            GenerateInfo();

            CurrentCursor = Cursors.Arrow;
            return true;
        }

        public virtual void HandleNotification(DeviceChangedType changeType, DeviceInfo di, string property = "")
        {
            switch (changeType)
            {
                case DeviceChangedType.Peripherals_PlugIn:
                    if (DeviceInfos.ContainsKey(di.ID))
                    { DeviceInfos.Remove(di.ID); }
                    DeviceInfos.Add(di.ID, di);
                    if (CurrentInstanceID == di.InstanceId)
                    {
                        CurrentDeviceID = di.ID;
                        SetCurrentDevice(CurrentDeviceID.ToString());
                    }
                    CheckMultiDevice();
                    break;

                case DeviceChangedType.Peripherals_UnPlug:
                    CheckMultiDevice();
                    if (CurrentInstanceID == di.InstanceId)
                    {
                        //Thread.Sleep(5000);
                        var hasDevice = false;
                        foreach (var info in DeviceInfos.Values)
                        {
                            if (info.InstanceId == CurrentInstanceID && info.IsConnected)
                            {
                                CurrentDeviceID = info.ID;
                                hasDevice = true;
                            }
                        }
                        if (hasDevice)
                        {
                            SetCurrentDevice(CurrentDeviceID.ToString());
                        }
                        else
                        {
                            timer.Start();
                        }
                    }
                    break;

                case DeviceChangedType.Peripherals_SettingsChange:
                    if (DeviceInfos.Keys.Contains(di.ID))
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
                        switch (property)
                        {
                            case "BatteryStatusChanged":
                                BatteryStatus = di.BatteryStatus;
                                break;

                            case "BatteryLevelChanged":
                                if (BatteryLevel == -1)
                                    SetCurrentDevice(CurrentDeviceID.ToString());
                                else
                                    BatteryLevel = di.BatteryLevel;
                                break;

                            default:
                                break;
                        }
                    }
                    break;

                default:
                    break;
            }
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            if (!DeviceInfos.ContainsKey(CurrentDeviceID))
                OnGoBackClicked();
            timer.Stop();
        }

        protected void GenerateInfo()
        {
            StringBuilder DeviceInfo = new();
            if (CurrentDeviceInfo!.Name.ToUpper().Contains("HEADSET"))
            {
                DeviceInfo.Append($"ID : {CurrentDeviceInfo!.ID}");
                DeviceInfo.Append(Environment.NewLine);
                DeviceInfo.Append($"IsReady : {CurrentDeviceInfo!.IsReady}");
                DeviceInfo.Append(Environment.NewLine);
                DeviceInfo.Append($"PhyscialDeviceID : {CurrentDeviceInfo.PhyscialDeviceID}");
                DeviceInfo.Append(Environment.NewLine);
                DeviceInfo.Append($"Name : {CurrentDeviceInfo.Name}");
                DeviceInfo.Append(Environment.NewLine);
                DeviceInfo.Append($"BatteryLevel : {CurrentDeviceInfo.BatteryLevel}");
                DeviceInfo.Append(Environment.NewLine);
                DeviceInfo.Append($"BatteryStatus : {CurrentDeviceInfo.BatteryStatus}");
                DeviceInfo.Append(Environment.NewLine);
                DeviceInfo.Append($"InterfaceType : {CurrentDeviceInfo.InterfaceType}");
                DeviceInfo.Append(Environment.NewLine);
                DeviceInfo.Append($"IsBatteryLevelSupported : {CurrentDeviceInfo.IsBatteryLevelSupported}");
                DeviceInfo.Append(Environment.NewLine);
                DeviceInfo.Append($"IsConnected : {CurrentDeviceInfo.IsConnected}");
                DeviceInfo.Append(Environment.NewLine);
                DeviceInfo.Append($"IsPhysicalDeviceDongle : {CurrentDeviceInfo.IsPhysicalDeviceDongle}");
                DeviceInfo.Append(Environment.NewLine);
                DeviceInfo.Append($"LogicalDeviceType : {CurrentDeviceInfo.LogicalDeviceType}");
                DeviceInfo.Append(Environment.NewLine);
                DeviceInfo.Append($"ModelNumber : {CurrentDeviceInfo.ModelNumber}");
                DeviceInfo.Append(Environment.NewLine);
                DeviceInfo.Append($"MuteStatus : {CurrentDeviceInfo.MuteStatus}");
                DeviceInfo.Append(Environment.NewLine);
                DeviceInfo.Append($"OdmId : {CurrentDeviceInfo.OdmId}");
                DeviceInfo.Append(Environment.NewLine);
                DeviceInfo.Append($"PairedDeviceCount : {CurrentDeviceInfo.PairedDeviceCount}");
                DeviceInfo.Append(Environment.NewLine);
                DeviceInfo.Append($"PhysicalDeviceFirmwareVersion : {CurrentDeviceInfo.PhysicalDeviceFirmwareVersion}");
                DeviceInfo.Append(Environment.NewLine);
                DeviceInfo.Append($"PhysicalDeviceType : {CurrentDeviceInfo.PhysicalDeviceType}");
                DeviceInfo.Append(Environment.NewLine);
                DeviceInfo.Append($"PluginId : {CurrentDeviceInfo.PluginId}");
                DeviceInfo.Append(Environment.NewLine);
                DeviceInfo.Append($"TotalNumberOfPairedHostName : {CurrentDeviceInfo.TotalNumberOfPairedHostName}");
                DeviceInfo.Append(Environment.NewLine);
                DeviceInfo.Append($"TouchScrollSensitivityLevel : {CurrentDeviceInfo.TouchScrollSensitivityLevel}");
                DeviceInfo.Append(Environment.NewLine);
                DeviceInfo.Append($"TouchSensitivityLevelValue : {CurrentDeviceInfo.TouchSensitivityLevelValue}");
                DeviceInfo.Append(Environment.NewLine);
                DeviceInfo.Append($"Type : {CurrentDeviceInfo.Type}");
                DeviceInfo.Append(Environment.NewLine);
                DeviceInfo.Append($"VisiblePairedHostName1 : {CurrentDeviceInfo.VisiblePairedHostName1}");
                DeviceInfo.Append(Environment.NewLine);
                DeviceInfo.Append($"VisiblePairedHostName2 : {CurrentDeviceInfo.VisiblePairedHostName2}");
                DeviceInfo.Append(Environment.NewLine);
                DeviceInfo.Append($"VisiblePairedHostName3 : {CurrentDeviceInfo.VisiblePairedHostName3}");
                DeviceInfo.Append(Environment.NewLine);
                DeviceInfo.Append($"AncGain : {CurrentDeviceInfo.AncGain}");
                DeviceInfo.Append(Environment.NewLine);
                DeviceInfo.Append($"AncMode : {CurrentDeviceInfo.AncMode}");
                DeviceInfo.Append(Environment.NewLine);
                DeviceInfo.Append($"Band1Gain : {CurrentDeviceInfo.Band1Gain}");
                DeviceInfo.Append(Environment.NewLine);
                DeviceInfo.Append($"Band2Gain : {CurrentDeviceInfo.Band2Gain}");
                DeviceInfo.Append(Environment.NewLine);
                DeviceInfo.Append($"Band3Gain : {CurrentDeviceInfo.Band3Gain}");
                DeviceInfo.Append(Environment.NewLine);
                DeviceInfo.Append($"Band4Gain : {CurrentDeviceInfo.Band4Gain}");
                DeviceInfo.Append(Environment.NewLine);
                DeviceInfo.Append($"Band5Gain : {CurrentDeviceInfo.Band5Gain}");
                DeviceInfo.Append(Environment.NewLine);
                DeviceInfo.Append($"BusyLight : {CurrentDeviceInfo.BusyLight}");
                DeviceInfo.Append(Environment.NewLine);
                DeviceInfo.Append($"IsMuteMicrophoneChecked : {CurrentDeviceInfo.IsMuteMicrophoneChecked}");
                DeviceInfo.Append(Environment.NewLine);
                DeviceInfo.Append($"IsPauseMusicChecked : {CurrentDeviceInfo.IsPauseMusicChecked}");
                DeviceInfo.Append(Environment.NewLine);
                DeviceInfo.Append($"IsQuickPauseChecked : {CurrentDeviceInfo.IsQuickPauseChecked}");
                DeviceInfo.Append(Environment.NewLine);
                DeviceInfo.Append($"IsWearDetectionChecked : {CurrentDeviceInfo.IsWearDetectionChecked}");
                DeviceInfo.Append(Environment.NewLine);
                DeviceInfo.Append($"MicNCIncoming : {CurrentDeviceInfo.MicNCIncoming}");
                DeviceInfo.Append(Environment.NewLine);
                DeviceInfo.Append($"MicNoiseCancellation : {CurrentDeviceInfo.MicNoiseCancellation}");
                DeviceInfo.Append(Environment.NewLine);
                DeviceInfo.Append($"SelectedPreset : {CurrentDeviceInfo.SelectedPreset}");
                DeviceInfo.Append(Environment.NewLine);
                DeviceInfo.Append($"Sidetone : {CurrentDeviceInfo.Sidetone}");
                DeviceInfo.Append(Environment.NewLine);
                DeviceInfo.Append($"SidetoneLevel : {CurrentDeviceInfo.SidetoneLevel}");
                DeviceInfo.Append(Environment.NewLine);
                DeviceInfo.Append($"VoiceGuidance : {CurrentDeviceInfo.VoiceGuidance}");
                DeviceInfo.Append(Environment.NewLine);
                DeviceInfo.Append($"WearDetection : {CurrentDeviceInfo.WearDetection}");
                DeviceInfo.Append(Environment.NewLine);
            }
            else if (CurrentDeviceInfo!.Name.ToUpper().Contains("SPEAKER"))
            {
                DeviceInfo.Append($"ID : {CurrentDeviceInfo!.ID}");
                DeviceInfo.Append(Environment.NewLine);
                DeviceInfo.Append($"PhyscialDeviceID : {CurrentDeviceInfo.PhyscialDeviceID}");
                DeviceInfo.Append(Environment.NewLine);
                DeviceInfo.Append($"Name : {CurrentDeviceInfo.Name}");
                DeviceInfo.Append(Environment.NewLine);
                DeviceInfo.Append($"BatteryLevel : {CurrentDeviceInfo.BatteryLevel}");
                DeviceInfo.Append(Environment.NewLine);
                DeviceInfo.Append($"BatteryStatus : {CurrentDeviceInfo.BatteryStatus}");
                DeviceInfo.Append(Environment.NewLine);
                DeviceInfo.Append($"InterfaceType : {CurrentDeviceInfo.InterfaceType}");
                DeviceInfo.Append(Environment.NewLine);
                DeviceInfo.Append($"IsBatteryLevelSupported : {CurrentDeviceInfo.IsBatteryLevelSupported}");
                DeviceInfo.Append(Environment.NewLine);
                DeviceInfo.Append($"IsConnected : {CurrentDeviceInfo.IsConnected}");
                DeviceInfo.Append(Environment.NewLine);
                DeviceInfo.Append($"IsPhysicalDeviceDongle : {CurrentDeviceInfo.IsPhysicalDeviceDongle}");
                DeviceInfo.Append(Environment.NewLine);
                DeviceInfo.Append($"LogicalDeviceType : {CurrentDeviceInfo.LogicalDeviceType}");
                DeviceInfo.Append(Environment.NewLine);
                DeviceInfo.Append($"ModelNumber : {CurrentDeviceInfo.ModelNumber}");
                DeviceInfo.Append(Environment.NewLine);
                DeviceInfo.Append($"MuteStatus : {CurrentDeviceInfo.MuteStatus}");
                DeviceInfo.Append(Environment.NewLine);
                DeviceInfo.Append($"OdmId : {CurrentDeviceInfo.OdmId}");
                DeviceInfo.Append(Environment.NewLine);
                DeviceInfo.Append($"PairedDeviceCount : {CurrentDeviceInfo.PairedDeviceCount}");
                DeviceInfo.Append(Environment.NewLine);
                DeviceInfo.Append($"PhysicalDeviceFirmwareVersion : {CurrentDeviceInfo.PhysicalDeviceFirmwareVersion}");
                DeviceInfo.Append(Environment.NewLine);
                DeviceInfo.Append($"PhysicalDeviceType : {CurrentDeviceInfo.PhysicalDeviceType}");
                DeviceInfo.Append(Environment.NewLine);
                DeviceInfo.Append($"PluginId : {CurrentDeviceInfo.PluginId}");
                DeviceInfo.Append(Environment.NewLine);
                DeviceInfo.Append($"TotalNumberOfPairedHostName : {CurrentDeviceInfo.TotalNumberOfPairedHostName}");
                DeviceInfo.Append(Environment.NewLine);
                DeviceInfo.Append($"TouchScrollSensitivityLevel : {CurrentDeviceInfo.TouchScrollSensitivityLevel}");
                DeviceInfo.Append(Environment.NewLine);
                DeviceInfo.Append($"TouchSensitivityLevelValue : {CurrentDeviceInfo.TouchSensitivityLevelValue}");
                DeviceInfo.Append(Environment.NewLine);
                DeviceInfo.Append($"Type : {CurrentDeviceInfo.Type}");
                DeviceInfo.Append(Environment.NewLine);
                DeviceInfo.Append($"VisiblePairedHostName1 : {CurrentDeviceInfo.VisiblePairedHostName1}");
                DeviceInfo.Append(Environment.NewLine);
                DeviceInfo.Append($"VisiblePairedHostName2 : {CurrentDeviceInfo.VisiblePairedHostName2}");
                DeviceInfo.Append(Environment.NewLine);
                DeviceInfo.Append($"VisiblePairedHostName3 : {CurrentDeviceInfo.VisiblePairedHostName3}");
                DeviceInfo.Append(Environment.NewLine);
                DeviceInfo.Append($"SelectedPreset : {CurrentDeviceInfo.SelectedPreset}");
                DeviceInfo.Append(Environment.NewLine);
                DeviceInfo.Append($"IsEqualizerSupported : {CurrentDeviceInfo.IsEqualizerSupported}");
                DeviceInfo.Append(Environment.NewLine);
                DeviceInfo.Append($"WiredAudioVolumeAdjustmentTone : {CurrentDeviceInfo.WiredAudioVolumeAdjustmentTone}");
                DeviceInfo.Append(Environment.NewLine);
                DeviceInfo.Append($"IsWiredAudioIMicNSEnable : {CurrentDeviceInfo.IsWiredAudioIMicNSEnable}");
                DeviceInfo.Append(Environment.NewLine);
                DeviceInfo.Append($"IsWiredAudioMicMuteSoundEnable : {CurrentDeviceInfo.IsWiredAudioMicMuteSoundEnable}");
                DeviceInfo.Append(Environment.NewLine);
                DeviceInfo.Append($"MuteStatus : {CurrentDeviceInfo.MuteStatus}");
                DeviceInfo.Append(Environment.NewLine);
            }
            else
            {
                DeviceInfo.Append($"ID : {CurrentDeviceInfo!.ID}");
                DeviceInfo.Append(Environment.NewLine);
                DeviceInfo.Append($"PhyscialDeviceID : {CurrentDeviceInfo.PhyscialDeviceID}");
                DeviceInfo.Append(Environment.NewLine);
                DeviceInfo.Append($"Name : {CurrentDeviceInfo.Name}");
                DeviceInfo.Append(Environment.NewLine);
                DeviceInfo.Append($"BackLightingControls : {CurrentDeviceInfo.BackLightingControls}");
                DeviceInfo.Append(Environment.NewLine);
                DeviceInfo.Append($"BackLightingLevel : {CurrentDeviceInfo.BackLightingLevel}");
                DeviceInfo.Append(Environment.NewLine);
                DeviceInfo.Append($"BackLightTabIndex : {CurrentDeviceInfo.BackLightTabIndex}");
                DeviceInfo.Append(Environment.NewLine);
                DeviceInfo.Append($"BatteryLevel : {CurrentDeviceInfo.BatteryLevel}");
                DeviceInfo.Append(Environment.NewLine);
                DeviceInfo.Append($"BatteryStatus : {CurrentDeviceInfo.BatteryStatus}");
                DeviceInfo.Append(Environment.NewLine);
                DeviceInfo.Append($"CollabsKeysSupported : {CurrentDeviceInfo.CollabsKeysSupported}");
                DeviceInfo.Append(Environment.NewLine);
                DeviceInfo.Append($"ColorCode : {CurrentDeviceInfo.ColorCode}");
                DeviceInfo.Append(Environment.NewLine);
                DeviceInfo.Append($"DeviceName : {CurrentDeviceInfo.DeviceName}");
                DeviceInfo.Append(Environment.NewLine);
                DeviceInfo.Append($"DpiDelta : {CurrentDeviceInfo.DpiDelta}");
                DeviceInfo.Append(Environment.NewLine);
                DeviceInfo.Append($"DPILevel : {CurrentDeviceInfo.DPILevel}");
                DeviceInfo.Append(Environment.NewLine);
                DeviceInfo.Append($"DpiLevel : {CurrentDeviceInfo.DpiLevel}");
                DeviceInfo.Append(Environment.NewLine);
                DeviceInfo.Append($"DPIValue : {CurrentDeviceInfo.DpiValue}");
                DeviceInfo.Append(Environment.NewLine);
                DeviceInfo.Append($"DpiLevelValues : {CurrentDeviceInfo.DpiLevelValues}");
                DeviceInfo.Append(Environment.NewLine);
                DeviceInfo.Append($"DpiMax : {CurrentDeviceInfo.DpiMax}");
                DeviceInfo.Append(Environment.NewLine);
                DeviceInfo.Append($"DpiMin : {CurrentDeviceInfo.DpiMin}");
                DeviceInfo.Append(Environment.NewLine);
                DeviceInfo.Append($"FirmwareVersion : {CurrentDeviceInfo.FirmwareVersion}");
                DeviceInfo.Append(Environment.NewLine);
                DeviceInfo.Append($"InstanceId : {CurrentDeviceInfo.InstanceId}");
                DeviceInfo.Append(Environment.NewLine);
                DeviceInfo.Append($"InstanceNumber : {CurrentDeviceInfo.InstanceNumber}");
                DeviceInfo.Append(Environment.NewLine);
                DeviceInfo.Append($"InterfaceType : {CurrentDeviceInfo.InterfaceType}");
                DeviceInfo.Append(Environment.NewLine);
                DeviceInfo.Append($"IsBatteryLevelSupported : {CurrentDeviceInfo.IsBatteryLevelSupported}");
                DeviceInfo.Append(Environment.NewLine);
                DeviceInfo.Append($"IsCollaborationBlinkEffectEnable : {CurrentDeviceInfo.IsCollaborationBlinkEffectEnable}");
                DeviceInfo.Append(Environment.NewLine);
                DeviceInfo.Append($"IsCollaborationCameraEnable : {CurrentDeviceInfo.IsCollaborationCameraEnable}");
                DeviceInfo.Append(Environment.NewLine);
                DeviceInfo.Append($"IsCollaborationChatEnable : {CurrentDeviceInfo.IsCollaborationChatEnable}");
                DeviceInfo.Append(Environment.NewLine);
                DeviceInfo.Append($"IsCollaborationDoubleTapEnable : {CurrentDeviceInfo.IsCollaborationDoubleTapEnable}");
                DeviceInfo.Append(Environment.NewLine);
                DeviceInfo.Append($"IsCollaborationKeyEnable : {CurrentDeviceInfo.IsCollaborationKeyEnable}");
                DeviceInfo.Append(Environment.NewLine);
                DeviceInfo.Append($"IsCollaborationMicEnable : {CurrentDeviceInfo.IsCollaborationMicEnable}");
                DeviceInfo.Append(Environment.NewLine);
                DeviceInfo.Append($"IsCollaborationScreenShareEnable : {CurrentDeviceInfo.IsCollaborationScreenShareEnable}");
                DeviceInfo.Append(Environment.NewLine);
                DeviceInfo.Append($"IsCollabsKeysSupported : {CurrentDeviceInfo.IsCollabsKeysSupported}");
                DeviceInfo.Append(Environment.NewLine);
                DeviceInfo.Append($"IsConnected : {CurrentDeviceInfo.IsConnected}");
                DeviceInfo.Append(Environment.NewLine);
                DeviceInfo.Append($"IsDPILevelChangePending : {CurrentDeviceInfo.IsDPILevelChangePending}");
                DeviceInfo.Append(Environment.NewLine);
                DeviceInfo.Append($"IsDPILevelSupported : {CurrentDeviceInfo.IsDPILevelSupported}");
                DeviceInfo.Append(Environment.NewLine);
                DeviceInfo.Append($"IsDPIValueChangePending : {CurrentDeviceInfo.IsDPIValueChangePending}");
                DeviceInfo.Append(Environment.NewLine);
                DeviceInfo.Append($"IsDPIValueSupported : {CurrentDeviceInfo.IsDPIValueSupported}");
                DeviceInfo.Append(Environment.NewLine);
                DeviceInfo.Append($"IsIlluminationSupported : {CurrentDeviceInfo.IsIlluminationSupported}");
                DeviceInfo.Append(Environment.NewLine);
                DeviceInfo.Append($"IsPhysicalDeviceDongle : {CurrentDeviceInfo.IsPhysicalDeviceDongle}");
                DeviceInfo.Append(Environment.NewLine);
                DeviceInfo.Append($"IsReportRateSupported : {CurrentDeviceInfo.IsReportRateSupported}");
                DeviceInfo.Append(Environment.NewLine);
                DeviceInfo.Append($"IsTouchScrollSensitivitySupported : {CurrentDeviceInfo.IsTouchScrollSensitivitySupported}");
                DeviceInfo.Append(Environment.NewLine);
                DeviceInfo.Append($"LogicalDeviceType : {CurrentDeviceInfo.LogicalDeviceType}");
                DeviceInfo.Append(Environment.NewLine);
                DeviceInfo.Append($"MaxPairingSlots : {CurrentDeviceInfo.MaxPairingSlots}");
                DeviceInfo.Append(Environment.NewLine);
                DeviceInfo.Append($"ModelNumber : {CurrentDeviceInfo.ModelNumber}");
                DeviceInfo.Append(Environment.NewLine);
                DeviceInfo.Append($"MousePrimaryButton : {CurrentDeviceInfo.MousePrimaryButton}");
                DeviceInfo.Append(Environment.NewLine);
                DeviceInfo.Append($"MuteStatus : {CurrentDeviceInfo.MuteStatus}");
                DeviceInfo.Append(Environment.NewLine);
                DeviceInfo.Append($"OdmId : {CurrentDeviceInfo.OdmId}");
                DeviceInfo.Append(Environment.NewLine);
                DeviceInfo.Append($"PairedDeviceCount : {CurrentDeviceInfo.PairedDeviceCount}");
                DeviceInfo.Append(Environment.NewLine);
                DeviceInfo.Append($"PairedHostName1 : {CurrentDeviceInfo.PairedHostName1}");
                DeviceInfo.Append(Environment.NewLine);
                DeviceInfo.Append($"PairedHostName2 : {CurrentDeviceInfo.PairedHostName2}");
                DeviceInfo.Append(Environment.NewLine);
                DeviceInfo.Append($"PairedHostName3 : {CurrentDeviceInfo.PairedHostName3}");
                DeviceInfo.Append(Environment.NewLine);
                DeviceInfo.Append($"PairingStatusName : {CurrentDeviceInfo.PairingStatusName}");
                DeviceInfo.Append(Environment.NewLine);
                DeviceInfo.Append($"PhysicalDeviceFirmwareVersion : {CurrentDeviceInfo.PhysicalDeviceFirmwareVersion}");
                DeviceInfo.Append(Environment.NewLine);
                DeviceInfo.Append($"PhysicalDeviceType : {CurrentDeviceInfo.PhysicalDeviceType}");
                DeviceInfo.Append(Environment.NewLine);
                DeviceInfo.Append($"PluginId : {CurrentDeviceInfo.PluginId}");
                DeviceInfo.Append(Environment.NewLine);
                DeviceInfo.Append($"ReportRate : {CurrentDeviceInfo.ReportRate}");
                DeviceInfo.Append(Environment.NewLine);
                DeviceInfo.Append($"Status : {CurrentDeviceInfo.Status}");
                DeviceInfo.Append(Environment.NewLine);
                DeviceInfo.Append($"TotalNumberOfPairedHostName : {CurrentDeviceInfo.TotalNumberOfPairedHostName}");
                DeviceInfo.Append(Environment.NewLine);
                DeviceInfo.Append($"TouchScrollSensitivityLevel : {CurrentDeviceInfo.TouchScrollSensitivityLevel}");
                DeviceInfo.Append(Environment.NewLine);
                DeviceInfo.Append($"TouchSensitivityLevelValue : {CurrentDeviceInfo.TouchSensitivityLevelValue}");
                DeviceInfo.Append(Environment.NewLine);
                DeviceInfo.Append($"Type : {CurrentDeviceInfo.Type}");
                DeviceInfo.Append(Environment.NewLine);
                DeviceInfo.Append($"VisiblePairedHostName1 : {CurrentDeviceInfo.VisiblePairedHostName1}");
                DeviceInfo.Append(Environment.NewLine);
                DeviceInfo.Append($"VisiblePairedHostName2 : {CurrentDeviceInfo.VisiblePairedHostName2}");
                DeviceInfo.Append(Environment.NewLine);
                DeviceInfo.Append($"VisiblePairedHostName3 : {CurrentDeviceInfo.VisiblePairedHostName3}");
            }
            this.DeviceInfo = DeviceInfo.ToString();
        }

        public virtual new void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
        }

        public string Name
        {
            get => _name;
            set
            {
                if (_name != value)
                {
                    _name = value;
                    OnPropertyChanged();
                }
            }
        }

        public string Model
        {
            get => _model;
            set
            {
                if (_model != value)
                {
                    _model = value;
                    OnPropertyChanged();
                }
            }
        }

        public string Model2 { get; set; } = "";

        public string? DeviceInfo { get; set; }
        public Guid CurrentDeviceID { get; set; }
        public int CurrentInstanceID { get; set; }
        public bool IsCollabsKeysSupported { get; set; }
        public bool IsIlluminationSupported { get; set; }
        public string PairedHostName1 { get; set; } = "";
        public string PairedHostName2 { get; set; } = "";
        public string PairedHostName3 { get; set; } = "";
        public string VisiblePairedHostName1 { get; set; } = "";
        public string VisiblePairedHostName2 { get; set; } = "";
        public string VisiblePairedHostName3 { get; set; } = "";
        public string PhysicalDeviceFWVersion { get; set; } = "";
        public string MultiDeviceTooltip { get; set; } = Strings.MultiDeviceTooltip;
        public string CopilotTooltip { get; set; } = Strings.CopilotTooltip;
        public string USBWirelessReceiverVersion
        { get { return PhysicalDeviceFWVersion; } }

        public string ImageFilePath
        {
            get => _imageFilePath;
            set
            {
                _imageFilePath = value;
                OnPropertyChanged();
            }
        }

        public string FirmwareVersion
        {
            get => _firmwareVersion;
            set
            {
                if (_firmwareVersion != value)
                {
                    _firmwareVersion = value;
                    OnPropertyChanged();
                }
            }
        }

        public string FirmwareVersion2
        {
            get => _firmwareVersion2;
            set
            {
                if (_firmwareVersion2 != value)
                {
                    _firmwareVersion2 = value;
                    OnPropertyChanged();
                }
            }
        }

        public string RightFrameVisibility
        {
            get => _rightFrameVisibility;
            set
            {
                if (_rightFrameVisibility != value)
                {
                    _rightFrameVisibility = value;
                    OnPropertyChanged();
                }
            }
        }

        public double BatteryLevel
        {
            get => _batteryLevel;
            set
            {
                if (_batteryLevel != value)
                {
                    _batteryLevel = value;
                    OnPropertyChanged();
                }
            }
        }

        public string BatteryStatus
        {
            get => _batteryStatus;
            set
            {
                if (_batteryStatus != value)
                {
                    _batteryStatus = value;
                    OnPropertyChanged();
                }
            }
        }

        public string ConnectionType
        {
            get => _connectionType;
            set
            {
                _connectionType = value;
                OnPropertyChanged();
            }
        }

        public int RightFrameWidthFrom
        {
            get => _rightFrameWidthFrom;
            set
            {
                _rightFrameWidthFrom = value;
                OnPropertyChanged();
            }
        }

        public int RightFrameWidthTo
        {
            get => _rightFrameWidthTo;
            set
            {
                _rightFrameWidthTo = value;
                OnPropertyChanged();
            }
        }

        //0617 Bruce 新增判斷是否支援電池，無支援電池回傳true，把預設電池圖片隱藏
        public bool NoBattery
        {
            get
            {
                if (CurrentDeviceInfo != null)
                {
                    return !CurrentDeviceInfo.IsBatteryLevelSupported;
                }
                return false;
            }
        }

        #region ModuleManager

        public List<ModuleGroup> _moduleGroups = new();
        private int _groupSelIdx = -1; //-1 = no selection, the DisplayPage is in Landing Mode

        public List<ModuleGroup> ModuleGroups
        {
            get => _moduleGroups;
            set
            {
                SetProperty(ref _moduleGroups, value);

                //Will rebuild all ModuleManager, reset to Landing mode
                //

                //1 Reebuld VbarItems for varList ListControl.ItemsSource
                RebuildVbarItems();
                _groupSelIdx = -1;
            }
        }

        public int GroupSelIdx
        {
            get => _groupSelIdx;

            //When VbarItem is clicked, DisplayPage will set to this property
            set
            {
                SetProperty(ref _groupSelIdx, value);

                //Validate value, allow set to -1 for reset to Landing mode, but should avoid
                //to access to Groups
                if ((_groupSelIdx < 0) || (_groupSelIdx >= GroupCount))
                    return;

                //When ModuleGroup selection changed, need to update Headers and its selection,
                ModuleGroup mg = ModuleGroups[_groupSelIdx];
                RightViewHeaders = mg.Headers;
            }
        }

        public int GroupCount
        {
            get
            {
                return _moduleGroups.Count;
            }
        }

        #endregion ModuleManager

        #region Vbar

        private int _vbarSelectedIndex = -1;
        private ICommand? _vbarItemClickCommand;

        /// <summary>
        /// Used by varList ListControl.ItemsSource only
        /// </summary>
        private readonly List<VbarItem> _vbarItems = new();

        /// <summary>
        /// Called when the ModuleGroups reset, will return to Landing Mode
        ///
        /// </summary>
        private void RebuildVbarItems()
        {
            _vbarItems.Clear();

            int idx = 0;
            foreach (ModuleGroup mg in ModuleGroups)
            {
                if (mg.GroupIcon != null)
                {
                    VbarItem vbarItem = new(idx, mg.GroupIcon, mg.GroupName)
                    {
                        ClickCommand = VbarItemClickCommand
                    };
                    _vbarItems.Add(vbarItem);
                    idx++;
                }
            }
            OnPropertyChanged(nameof(VbarItems));
        }

        public List<VbarItem> VbarItems
        {
            get => _vbarItems;
        }

        public int VbarSelectedIndex
        {
            get => _vbarSelectedIndex;
            set
            {
                SetProperty(ref _vbarSelectedIndex, value);
                OnPropertyChanged(nameof(ConnectionPopupMargin));

                if ((value < 0) || (value >= GroupCount))
                    return;

                ModuleGroup mg = ModuleGroups[VbarSelectedIndex];
                RightViewHeaders = mg.Headers;
            }
        }

        public Thickness ConnectionPopupMargin
        {
            get
            {
                return VbarSelectedIndex == -1 ? new Thickness(-70, 0, 0, 8) : new Thickness(0, 0, 0, 8);
            }
        }

        public ICommand? VbarItemClickCommand
        {
            get => _vbarItemClickCommand;
            set => SetProperty(ref _vbarItemClickCommand, value);
        }

        #endregion Vbar

        #region LeftView

        private UserControl? _defaultLeftView;

        public UserControl DefaultLeftView
        {
            get
            {
                _defaultLeftView ??= new DefaultLeftView();
                return _defaultLeftView;
            }
        }

        private UserControl? _leftView;

        public UserControl? LeftView
        {
            get
            {
                if (_leftView == null)
                    return DefaultLeftView;

                ModuleGroup? selGroup = SelectedGroup;
                if (selGroup != null)
                {
                    RightViewHeader selHeader = selGroup.Headers[RightViewHeaderSelectedIndex];
                    if (selHeader != null)
                    {
                        IDdpmModule? mod = selHeader.DdpmModule;
                        if (mod != null)
                            return mod?.GetLeftView();
                    }
                }
                return _leftView;
            }
            set
            {
                SetProperty(ref _leftView, value);
            }
        }

        #endregion LeftView

        #region RightView

        //Robert_Lin, 2024-6-26, fix SAST issue: [Bug] Refactor this getter so that it actiually refers to field '_rightView'.
        //It seems that RightView is used by get never used by set.
        //OLD Code:
        /*
    private UserControl? _rightView;
    public UserControl? RightView {
      get {
        ModuleGroup? selGroup = SelectedGroup;
        if (selGroup != null) {
          RightViewHeader selHeader = selGroup.Headers[RightViewHeaderSelectedIndex];
          if (selHeader != null) {
            IDdpmModule? mod = selHeader.DdpmModule;
            if (mod != null)
              ActiveModule = mod;
            return mod?.GetRightView();
          }
        }
        return null;
      }
      set {
        SetProperty(ref _rightView, value);
      }
    }
        */

        //NEW Code: _rightView is removed
        public UserControl? RightView
        {
            get
            {
                ModuleGroup? selGroup = SelectedGroup;
                if (selGroup != null)
                {
                    RightViewHeader selHeader = selGroup.Headers[RightViewHeaderSelectedIndex];
                    if (selHeader != null)
                    {
                        IDdpmModule? mod = selHeader.DdpmModule;
                        if (mod != null)
                            ActiveModule = mod;
                        return mod?.GetRightView();
                    }
                }
                return null;
            }
            //We should be able to comment out this setter, but need to remove setter from IPeripheralViewModel
            set
            {
                //    SetProperty(ref _rightView, value);
            }
        }

        public string RightViewModuleName
        {
            get
            {
                RightViewHeader? header = SelRightViewHeader;
                if (header != null)
                {
                    if (header.DdpmModule != null)
                        return header.DdpmModule.ModuleName;
                }
                return "(ERROR)";
            }
        }

        #endregion RightView

        #region RightViewHeader

        public int RightViewHeaderSelectedIndex
        {
            get
            {
                ModuleGroup? selGroup = SelectedGroup;
                if (selGroup != null)
                {
                    return selGroup.HeaderSelectedIndex;
                }
                return 0;
            }
            set
            {
                ModuleGroup? selGroup = SelectedGroup;
                if (selGroup != null)
                {
                    selGroup.HeaderSelectedIndex = value;
                }
                OnPropertyChanged(nameof(RightViewHeaderSelectedIndex));
                OnPropertyChanged(nameof(LeftView));
                OnPropertyChanged(nameof(RightView));
            }
        }

        public RightViewHeader? SelRightViewHeader
        {
            get
            {
                if (SelectedGroup != null)
                {
                    return SelectedGroup.Headers[SelectedGroup.HeaderSelectedIndex];
                }
                return null;
            }
        }

        #endregion RightViewHeader

        /*

        #region ModuleGroups

        private List<ModuleGroup> _moduleGroups = new List<ModuleGroup>();
        public List<ModuleGroup> ModuleGroups
        {
            get => _moduleGroups;
            set => SetProperty(ref _moduleGroups, value);
        }

        #endregion ModuleGroups

        */

        // After ModuleGroups is build,

        private ObservableCollection<RightViewHeader> _rightViewHeaders = new();

        public ObservableCollection<RightViewHeader> RightViewHeaders
        {
            get
            {
                if (_rightViewHeaders.Count == 0)
                {
                    //If _vbarItems is empty, will build the list from ModuleGroups
                    if ((_rightViewHeaders.Count == 0) && (ModuleGroups.Count > 0))
                    {
                        //Get the selected ModuleGroup
                        if ((VbarSelectedIndex >= 0) || (VbarSelectedIndex < (ModuleGroups.Count)))
                        {
                            ModuleGroup mg = ModuleGroups[VbarSelectedIndex];
                            _rightViewHeaders = mg.Headers;
                        }
                    }
                }
                return _rightViewHeaders;
            }
            set
            {
                SetProperty(ref _rightViewHeaders, value);
                OnPropertyChanged(nameof(RightViewHeaderSelectedIndex));
                OnPropertyChanged(nameof(LeftView));
                OnPropertyChanged(nameof(RightView));
            }
        }

        public ModuleGroup? SelectedGroup
        {
            get
            {
                if (ModuleGroups.Count <= 0)
                    return null;

                if ((VbarSelectedIndex >= 0) && (VbarSelectedIndex < (ModuleGroups.Count)))
                {
                    return ModuleGroups[VbarSelectedIndex];
                }
                return null;
            }
        }

        public void Reset()
        {
            foreach (ModuleGroup mg in _moduleGroups)
            {
                mg.Headers.Clear();
            }
            _moduleGroups.Clear();
            GroupSelIdx = -1;
            VbarSelectedIndex = -1;
        }

        public void SetLadningMode(bool isLandingMode)
        {
            foreach (VbarItem vbarItem in _vbarItems)
            {
                vbarItem.SetLadningMode(isLandingMode);
            }
        }

        public void SelectVBar()
        {
            foreach (VbarItem vbarItem in _vbarItems)
            {
                vbarItem.IsSelected = vbarItem.Id == VbarSelectedIndex;
            }
        }
        public void DisableVBar()
        {
            for (var i = 2; i < _vbarItems.Count; i++)
            {
                _vbarItems[i].IsEnabled = false;
                _vbarItems[i].TooltipVisibility = Visibility.Visible;
            }
            IsNotAddingProfile = false;
            OnPropertyChanged(nameof(IsNotAddingProfile));
        }
        public void EnableVBar()
        {
            for (var i = 2; i < _vbarItems.Count; i++)
            {
                _vbarItems[i].IsEnabled = true;
                _vbarItems[i].TooltipVisibility = Visibility.Collapsed;
            }
            IsNotAddingProfile = true;
            OnPropertyChanged(nameof(IsNotAddingProfile));
        }
        public bool IsNotAddingProfile { get; set; } = true;

        private Cursor _currentCursor = Cursors.Arrow;
        public Cursor CurrentCursor
        {
            get { return _currentCursor; }
            set
            {
                if (_currentCursor != value)
                {
                    _currentCursor = value;
                    OnPropertyChanged();
                }
            }
        }

        #region Handle Module Activated/Deactivated

        private IDdpmModule? _activeModule;

        public IDdpmModule? ActiveModule
        {
            get => _activeModule;
            set
            {
                if (_activeModule == value)
                    return;
                if (_activeModule != null)
                    _activeModule.OnDeactivated();
                SetProperty(ref _activeModule, value);
                _activeModule?.OnActivated();
            }
        }

        #endregion Handle Module Activated/Deactivated
    }
}