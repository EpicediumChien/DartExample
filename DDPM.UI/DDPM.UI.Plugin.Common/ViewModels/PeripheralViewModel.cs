using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DDPM.SA.Common;
using DDPM.SA.Common.Settings;
using DDPM.UI.Common;
using DDPM.UI.Common.Interfaces;
using DDPM.UI.Common.Models;
using DDPM.UI.Common.UserControls;
using DDPM.UI.Common.Views;
using DDPM.UI.Interfaces;
using Dell.Client.Framework.Common;
using Dell.Client.Framework.UX.WPF;
using Dell.Client.Framework.UX.WPF.Controls;
using DPeMPublic.Common.Enums;
using Microsoft;
using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Windows.Devices.Geolocation;
using static System.Net.Mime.MediaTypeNames;
using MessageBox = System.Windows.MessageBox;
using UserControl = System.Windows.Controls.UserControl;

namespace DDPM.UI.Plugin.ViewModels
{
    public class PeripheralViewModel : ObservableObject, IPeripheralViewModel
    {
        #region Variables

        private readonly IConsole _console;
        private readonly ILog _log;
        private readonly IDeviceManagerSA? _deviceManager;
        private string _name = "";
        private string _model = "";
        private string _imageFilePath = "";
        private string _deviceId = "";
        private string _deviceId2 = "";
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

        public DeviceInfo? CurrentDeviceInfo = null;

        public ICommand GoBackClickedCommand { get; private set; }
        public ICommand ShowInfoClickedCommand { get; private set; }
        public volatile Dictionary<Guid, DeviceInfo> DeviceInfos = new();
        public List<string> EOLKBList = DDPM.SA.Common.UI.SAUICommonHelper.EOLKBList;// new () { "WK636", "KM713", "WK717", "KM714", "KM717" };
        public List<string> EOLMouseList = DDPM.SA.Common.UI.SAUICommonHelper.EOLMouseList;// new () { "WM116", "WM514", "UV514", "WM126", "WM326", "WM527" };
        //public DDPMSettings? DDPMSettings;
        public bool IsCopilotEnabled = true;
        public bool IsDTPReady = false;
        public int CurrentVersion = 0;

        public PeripheralViewModel(IConsole console, ILog log, IDeviceManagerSA? deviceManager)
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

            if (CurrentVersion == 0)
            {
                string regPath = $@"SOFTWARE\Microsoft\Windows NT\CurrentVersion";
                string regKey = $"CurrentBuild";
                //var regValue = DdpmCommonHelper.DeviceManagerSA.ReadRegistryData(SA.Common.Settings.RegistryHive.LocalMachine, regPath, regKey).Result;
                var regValue = DdpmCommonHelper.ReadRegistryData(SA.Common.Settings.RegistryHive.LocalMachine, regPath, regKey);
                if (int.TryParse((string)regValue, out int build))
                {
                    CurrentVersion = build >= 22000 ? 11 : 10;
                }
                else
                {
                    CurrentVersion = 10;
                }
            }
            _log!.Info($"[PeripheralViewModel] PeripheralViewModel Start ...");
        }

        public void Unpair()
        {
            _deviceManager.UnPair(CurrentDeviceID);
            OnGoBackClicked();
        }

        public virtual void OnGoBackClicked()
        {
            DdpmCommonHelper.BitmapImageUpdated -= OnVBarThemeChange;
            VbarSelectedIndex = -1;
            _console.ShowHomePage();
        }

        private void OnShowInfoClicked()
        {
            //MessageBox.Show(DeviceInfo, Name, MessageBoxButton.OK, MessageBoxImage.Information);
        }

        public Visibility MultiDevicesInfoVisibility { get; set; } = Visibility.Collapsed;
        public Visibility CopilotInfoVisibility { get; set; } = Visibility.Collapsed;

        private void CheckMultiDevice()
        {
            int i = 0;
            foreach (var info in DeviceInfos.Values)
            {
                if (DDPM.SA.Common.UI.SAUICommonHelper.MappingModel(info.ModelNumber) == Model)
                {
                    i++;
                    //OnPropertyChanged(nameof(MultiDevicesInfoVisibility));
                    //return;
                }
            }
            if (i > 1)
                MultiDevicesInfoVisibility = Visibility.Visible;
            else
                MultiDevicesInfoVisibility = Visibility.Collapsed;
            OnPropertyChanged(nameof(MultiDevicesInfoVisibility));
        }

        private void CheckCopilot()
        {
            //string regPath2 = $@"SOFTWARE\Dell\Dell Display And Peripheral Manager\UserSettings\Local";
            //string regKey2 = $"IsFirstTimeWalkThroughDone_com.dell.DPM.Plugin.LogicalDevice.DDPM";
            //var regValue2 = DdpmCommonHelper.DeviceManagerSA.ReadRegistryData(RegistryHive.LocalMachine, regPath2, regKey2).Result;
            string regPath = $@"SOFTWARE\Policies\Microsoft\Windows\WindowsCopilot";
            string regKey = $"TurnOffWindowsCopilot";
            //var regValue = DdpmCommonHelper.DeviceManagerSA.ReadRegistryData(RegistryHive.CurrentUser, regPath, regKey).Result;
            try
            {
                // Open the registry key under the current user
                using (RegistryKey? key = Registry.CurrentUser.OpenSubKey(regPath))
                {
                    if (key != null)
                    {
                        // Read the value
                        object value = key.GetValue(regKey);

                        if (value != null && Convert.ToInt32(value) == 1)
                        {
                            IsCopilotEnabled = false;
                        }
                        else
                        {
                            IsCopilotEnabled = true;
                        }
                    }
                    else
                    {
                        IsCopilotEnabled = true;
                    }
                }
            }
            catch (Exception ex)
            {
                IsCopilotEnabled = true;
            }
            //Actions.IsCopilotEnabled = regValue == null || Convert.ToInt32(regValue) != 1;
        }

        public bool IsIDInvalid = false;

        public virtual bool SetCurrentDevice(string instanceIDs)
        {
            _console.RaiseEvent(ConsoleEventNames.Masthead_ShowAddDeviceIcon, this, new EventManagerArgs() { Tag = new List<bool> { true, true } });
            IsIDInvalid = false;
            if (instanceIDs.Substring(instanceIDs.Length - 2, 1) == "-")
            {
                instenceNo = instanceIDs.Substring(instanceIDs.Length - 1, 1);
                instanceIDs = instanceIDs.Substring(0, instanceIDs.Length - 2);
            }
            else
            {
                instenceNo = "";
            }
            CurrentDeviceID = new Guid(instanceIDs);

            if (DeviceInfos.TryGetValue(CurrentDeviceID, out DeviceInfo? di))
            {
                _log.Info($"[PeripheralViewModel] SetCurrentDevice ... InstanceId = {di.InstanceId.ToString()}");
                if (di.DeviceName == "Headset Settings" || di.DeviceName == "Wired Audio Settings")
                {
                    CurrentInstanceID = di.ID.GetHashCode();
                    _log.Info($"[PeripheralViewModel] SetCurrentDevice Headset/Wired Audio Settings... InstanceId = {CurrentInstanceID.ToString()}");
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
                if (di == null)
                {
                    return false;
                }
                CurrentDeviceInfo = di;
            }
            else
            {
                OnGoBackClicked();
                IsIDInvalid = true;
                return false;
            }

            CheckCopilot();
            Model = DDPM.SA.Common.UI.SAUICommonHelper.MappingModel(CurrentDeviceInfo.ModelNumber);
            //Model = "WK717";
            if (EOLKBList.Contains(Model) || EOLMouseList.Contains(Model))
            {
                _name = DDPM.SA.Common.UI.SAUICommonHelper.MappingEOLName(Model);
                _name = _name.Replace(Model, "").Trim();
                //_name = _name.Replace("  ", " ");
            }
            else
                Name = DDPM.SA.Common.UI.SAUICommonHelper.MappingName(Model, CurrentDeviceInfo.Name.Trim());

            if (CurrentDeviceInfo.Type == DeviceType.PhysicalWiredDock || CurrentDeviceInfo.Type == DeviceType.LogicalDock)
            {
                string[] s = CurrentDeviceInfo.Name.Split(" ");
                Name = "";
                foreach (string temps in s)
                {
                    Name += temps + " ";
                    if (temps.ToUpper().Equals("DOCK"))
                    {
                        break;
                    }
                }
            }
            if (string.IsNullOrEmpty(instenceNo))
            {
                Model2 = Model;
            }
            else
            {
                Model2 = $"{Model} ({instenceNo})";
            }

            var colorCode = CurrentDeviceInfo.ColorCode == 0 ? "" : $"_{CurrentDeviceInfo.ColorCode}";
            string imageFileName = DdpmCommonHelper.DeterminePeripheralProductImageFileName(CurrentDeviceInfo);
            if (!String.IsNullOrEmpty(imageFileName))
            {
                if (CurrentDeviceInfo.Type == DeviceType.LogicalNotSupported)
                {
                    ImageFilePath = $"/DDPM.UI.Resources;component/Resources/Images/";
                    if (!DdpmCommonHelper.isDarkMode())
                    {
                        ImageFilePath += "LightMode/";
                    }
                    if (DdpmCommonHelper.EOLKBList.Contains(CurrentDeviceInfo?.ModelNumber ?? ""))
                    {
                        ImageFilePath += "Lineart-kb.png";
                    }
                    if (DdpmCommonHelper.EOLMouseList.Contains(CurrentDeviceInfo?.ModelNumber ?? ""))
                    {
                        ImageFilePath += "Lineart-ms.png";
                    }
                }
                else
                    ImageFilePath = $"/DDPM.UI.Resources;component/Resources/Images/{Model}{colorCode}.png";
            }

            if (CurrentDeviceInfo == null)
                return false;

            FirmwareVersion = CurrentDeviceInfo.FirmwareVersion;
            var fv = CurrentDeviceInfo.FirmwareVersion.PadLeft(4, '0');
            //--Bruce 0221 The firmware version has been processed in SA, so there is no need to process the string again.
            FirmwareVersion2 = $"{Strings.FirmwareVersion} {fv}";

            DeviceID2 = $"{Strings.DeviceID} {CurrentDeviceID}";
            //FirmwareVersion2 = $"{Strings.FirmwareVersion} {fv.Substring(0, 1)}.{fv.Substring(1, 1)}.{fv.Substring(2, 1)}.{fv.Substring(3, 1)}";
            //--Bruce 0221
            //ConnectionType = CurrentDeviceInfo.PhysicalDeviceType.ToString() == "PhysicalDongle" ? "Dongle" : "Bluetooth";
            switch (CurrentDeviceInfo.PhysicalDeviceType)
            {
                case DeviceType.PhysicalWebcam:
                    ConnectionType = "Wired";
                    //Name = Name.Replace(Model, "").Trim();
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
                    ConnectionType = Model == "PN5122W" ? "Pandora" : "Bluetooth";
                    break;
                //0617 Bruce 新增Dock連線方式的濾字串的方式
                case DeviceType.PhysicalWiredDock:
                    if (CurrentDeviceInfo.ModelNumber.ToString().Contains("TB5"))
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
            //GenerateInfo();

            CurrentCursor = Cursors.Arrow;
            return true;
        }

        //using this function from DDPM.SA.Common.UI.SAUICommonHelper [Dean]0115
        /*private string MappingModel(string modelNumber)
        {
            //[#PeripheralModelMap] This mapping table has a duplicate code in
            //1 DdpmCommonHelpers.cs    DeterminePeripheralProductImageFileName()
            //2 HomeDevices             TooltipModelName property
            //3 PeripheralViewModel.cs  MappingModel()
            //If you need to modify, please also modify them.
            switch (modelNumber)
            {
                case "KB740":
                case "KB7120W":
                    return "KB740";

                case "KB500":
                case "KB3121W":
                    return "KB500";

                case "KB700":
                case "KB7221W":
                    return "KB700";

                case "MS300":
                case "MS3121W":
                    return "MS300";

                default:
                    return modelNumber;
            }
        }*/

        public virtual void HandleNotification(DeviceChangedType changeType, DeviceInfo di, string property = "")
        {
            if (di == null || CurrentDeviceInfo == null)
            {
                DdpmCommonHelper.WriteUILog($"Error: DeviceChanged Event with no device info!");
                return;
            }
            DdpmCommonHelper.WriteUILog($"DeviceChanged Event: Type: {changeType} ID: {di.ID} Property: {property}");

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
                    //if (DeviceInfos.Keys.Contains(di.ID))
                    //{
                    //    DeviceInfos.Remove(di.ID);
                    //    DeviceInfos.Add(di.ID, di);
                    //}
                    //else
                    //{
                    //    return;
                    //}
                    if (di.ID == CurrentDeviceID)
                    {
                        //CurrentDeviceInfo = DeviceInfos[CurrentDeviceID];
                        switch (property)
                        {
                            case "BatteryStatusChanged":
                                BatteryStatus = di.BatteryStatus;
                                CurrentDeviceInfo.BatteryStatus = di.BatteryStatus;
                                break;
                            case "DeviceNameChanged":
                                Name = di.Name.Replace(Model, "").Trim();
                                break;

                            case "BatteryLevelChanged":
                                BatteryLevel = di.BatteryLevel;
                                CurrentDeviceInfo.BatteryLevel = di.BatteryLevel;
                                //if (BatteryLevel == -1)
                                //    SetCurrentDevice(CurrentDeviceID.ToString());
                                //else
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
#if DEBUG
            if (CurrentDeviceInfo == null)
                return;

            StringBuilder localDeviceInfo = new();
            if (CurrentDeviceInfo.Name.ToUpper().Contains("HEADSET"))
            {
                localDeviceInfo.Append($"ID : {CurrentDeviceInfo.ID}");
                localDeviceInfo.Append(Environment.NewLine);
                localDeviceInfo.Append($"IsReady : {CurrentDeviceInfo.IsReady}");
                localDeviceInfo.Append(Environment.NewLine);
                localDeviceInfo.Append($"PhyscialDeviceID : {CurrentDeviceInfo.PhyscialDeviceID}");
                localDeviceInfo.Append(Environment.NewLine);
                localDeviceInfo.Append($"Name : {CurrentDeviceInfo.Name}");
                localDeviceInfo.Append(Environment.NewLine);
                localDeviceInfo.Append($"BatteryLevel : {CurrentDeviceInfo.BatteryLevel}");
                localDeviceInfo.Append(Environment.NewLine);
                localDeviceInfo.Append($"BatteryStatus : {CurrentDeviceInfo.BatteryStatus}");
                localDeviceInfo.Append(Environment.NewLine);
                localDeviceInfo.Append($"InterfaceType : {CurrentDeviceInfo.InterfaceType}");
                localDeviceInfo.Append(Environment.NewLine);
                localDeviceInfo.Append($"IsBatteryLevelSupported : {CurrentDeviceInfo.IsBatteryLevelSupported}");
                localDeviceInfo.Append(Environment.NewLine);
                localDeviceInfo.Append($"IsConnected : {CurrentDeviceInfo.IsConnected}");
                localDeviceInfo.Append(Environment.NewLine);
                localDeviceInfo.Append($"IsPhysicalDeviceDongle : {CurrentDeviceInfo.IsPhysicalDeviceDongle}");
                localDeviceInfo.Append(Environment.NewLine);
                localDeviceInfo.Append($"LogicalDeviceType : {CurrentDeviceInfo.LogicalDeviceType}");
                localDeviceInfo.Append(Environment.NewLine);
                localDeviceInfo.Append($"ModelNumber : {CurrentDeviceInfo.ModelNumber}");
                localDeviceInfo.Append(Environment.NewLine);
                localDeviceInfo.Append($"MuteStatus : {CurrentDeviceInfo.MuteStatus}");
                localDeviceInfo.Append(Environment.NewLine);
                localDeviceInfo.Append($"OdmId : {CurrentDeviceInfo.OdmId}");
                localDeviceInfo.Append(Environment.NewLine);
                localDeviceInfo.Append($"PairedDeviceCount : {CurrentDeviceInfo.PairedDeviceCount}");
                localDeviceInfo.Append(Environment.NewLine);
                localDeviceInfo.Append($"PhysicalDeviceFirmwareVersion : {CurrentDeviceInfo.PhysicalDeviceFirmwareVersion}");
                localDeviceInfo.Append(Environment.NewLine);
                localDeviceInfo.Append($"PhysicalDeviceType : {CurrentDeviceInfo.PhysicalDeviceType}");
                localDeviceInfo.Append(Environment.NewLine);
                localDeviceInfo.Append($"PluginId : {CurrentDeviceInfo.PluginId}");
                localDeviceInfo.Append(Environment.NewLine);
                localDeviceInfo.Append($"TotalNumberOfPairedHostName : {CurrentDeviceInfo.TotalNumberOfPairedHostName}");
                localDeviceInfo.Append(Environment.NewLine);
                localDeviceInfo.Append($"TouchScrollSensitivityLevel : {CurrentDeviceInfo.TouchScrollSensitivityLevel}");
                localDeviceInfo.Append(Environment.NewLine);
                localDeviceInfo.Append($"TouchSensitivityLevelValue : {CurrentDeviceInfo.TouchSensitivityLevelValue}");
                localDeviceInfo.Append(Environment.NewLine);
                localDeviceInfo.Append($"Type : {CurrentDeviceInfo.Type}");
                localDeviceInfo.Append(Environment.NewLine);
                localDeviceInfo.Append($"VisiblePairedHostName1 : {CurrentDeviceInfo.VisiblePairedHostName1}");
                localDeviceInfo.Append(Environment.NewLine);
                localDeviceInfo.Append($"VisiblePairedHostName2 : {CurrentDeviceInfo.VisiblePairedHostName2}");
                localDeviceInfo.Append(Environment.NewLine);
                localDeviceInfo.Append($"VisiblePairedHostName3 : {CurrentDeviceInfo.VisiblePairedHostName3}");
                localDeviceInfo.Append(Environment.NewLine);
                localDeviceInfo.Append($"AncGain : {CurrentDeviceInfo.AncGain}");
                localDeviceInfo.Append(Environment.NewLine);
                localDeviceInfo.Append($"AncMode : {CurrentDeviceInfo.AncMode}");
                localDeviceInfo.Append(Environment.NewLine);
                localDeviceInfo.Append($"Band1Gain : {CurrentDeviceInfo.Band1Gain}");
                localDeviceInfo.Append(Environment.NewLine);
                localDeviceInfo.Append($"Band2Gain : {CurrentDeviceInfo.Band2Gain}");
                localDeviceInfo.Append(Environment.NewLine);
                localDeviceInfo.Append($"Band3Gain : {CurrentDeviceInfo.Band3Gain}");
                localDeviceInfo.Append(Environment.NewLine);
                localDeviceInfo.Append($"Band4Gain : {CurrentDeviceInfo.Band4Gain}");
                localDeviceInfo.Append(Environment.NewLine);
                localDeviceInfo.Append($"Band5Gain : {CurrentDeviceInfo.Band5Gain}");
                localDeviceInfo.Append(Environment.NewLine);
                localDeviceInfo.Append($"BusyLight : {CurrentDeviceInfo.BusyLight}");
                localDeviceInfo.Append(Environment.NewLine);
                localDeviceInfo.Append($"IsMuteMicrophoneChecked : {CurrentDeviceInfo.IsMuteMicrophoneChecked}");
                localDeviceInfo.Append(Environment.NewLine);
                localDeviceInfo.Append($"IsPauseMusicChecked : {CurrentDeviceInfo.IsPauseMusicChecked}");
                localDeviceInfo.Append(Environment.NewLine);
                localDeviceInfo.Append($"IsQuickPauseChecked : {CurrentDeviceInfo.IsQuickPauseChecked}");
                localDeviceInfo.Append(Environment.NewLine);
                localDeviceInfo.Append($"IsWearDetectionChecked : {CurrentDeviceInfo.IsWearDetectionChecked}");
                localDeviceInfo.Append(Environment.NewLine);
                localDeviceInfo.Append($"MicNCIncoming : {CurrentDeviceInfo.MicNCIncoming}");
                localDeviceInfo.Append(Environment.NewLine);
                localDeviceInfo.Append($"MicNoiseCancellation : {CurrentDeviceInfo.MicNoiseCancellation}");
                localDeviceInfo.Append(Environment.NewLine);
                localDeviceInfo.Append($"SelectedPreset : {CurrentDeviceInfo.SelectedPreset}");
                localDeviceInfo.Append(Environment.NewLine);
                localDeviceInfo.Append($"Sidetone : {CurrentDeviceInfo.Sidetone}");
                localDeviceInfo.Append(Environment.NewLine);
                localDeviceInfo.Append($"SidetoneLevel : {CurrentDeviceInfo.SidetoneLevel}");
                localDeviceInfo.Append(Environment.NewLine);
                localDeviceInfo.Append($"VoiceGuidance : {CurrentDeviceInfo.VoiceGuidance}");
                localDeviceInfo.Append(Environment.NewLine);
                localDeviceInfo.Append($"WearDetection : {CurrentDeviceInfo.WearDetection}");
                localDeviceInfo.Append(Environment.NewLine);
            }
            else if (CurrentDeviceInfo.Name.ToUpper().Contains("SPEAKER"))
            {
                localDeviceInfo.Append($"ID : {CurrentDeviceInfo.ID}");
                localDeviceInfo.Append(Environment.NewLine);
                localDeviceInfo.Append($"PhyscialDeviceID : {CurrentDeviceInfo.PhyscialDeviceID}");
                localDeviceInfo.Append(Environment.NewLine);
                localDeviceInfo.Append($"Name : {CurrentDeviceInfo.Name}");
                localDeviceInfo.Append(Environment.NewLine);
                localDeviceInfo.Append($"BatteryLevel : {CurrentDeviceInfo.BatteryLevel}");
                localDeviceInfo.Append(Environment.NewLine);
                localDeviceInfo.Append($"BatteryStatus : {CurrentDeviceInfo.BatteryStatus}");
                localDeviceInfo.Append(Environment.NewLine);
                localDeviceInfo.Append($"InterfaceType : {CurrentDeviceInfo.InterfaceType}");
                localDeviceInfo.Append(Environment.NewLine);
                localDeviceInfo.Append($"IsBatteryLevelSupported : {CurrentDeviceInfo.IsBatteryLevelSupported}");
                localDeviceInfo.Append(Environment.NewLine);
                localDeviceInfo.Append($"IsConnected : {CurrentDeviceInfo.IsConnected}");
                localDeviceInfo.Append(Environment.NewLine);
                localDeviceInfo.Append($"IsPhysicalDeviceDongle : {CurrentDeviceInfo.IsPhysicalDeviceDongle}");
                localDeviceInfo.Append(Environment.NewLine);
                localDeviceInfo.Append($"LogicalDeviceType : {CurrentDeviceInfo.LogicalDeviceType}");
                localDeviceInfo.Append(Environment.NewLine);
                localDeviceInfo.Append($"ModelNumber : {CurrentDeviceInfo.ModelNumber}");
                localDeviceInfo.Append(Environment.NewLine);
                localDeviceInfo.Append($"MuteStatus : {CurrentDeviceInfo.MuteStatus}");
                localDeviceInfo.Append(Environment.NewLine);
                localDeviceInfo.Append($"OdmId : {CurrentDeviceInfo.OdmId}");
                localDeviceInfo.Append(Environment.NewLine);
                localDeviceInfo.Append($"PairedDeviceCount : {CurrentDeviceInfo.PairedDeviceCount}");
                localDeviceInfo.Append(Environment.NewLine);
                localDeviceInfo.Append($"PhysicalDeviceFirmwareVersion : {CurrentDeviceInfo.PhysicalDeviceFirmwareVersion}");
                localDeviceInfo.Append(Environment.NewLine);
                localDeviceInfo.Append($"PhysicalDeviceType : {CurrentDeviceInfo.PhysicalDeviceType}");
                localDeviceInfo.Append(Environment.NewLine);
                localDeviceInfo.Append($"PluginId : {CurrentDeviceInfo.PluginId}");
                localDeviceInfo.Append(Environment.NewLine);
                localDeviceInfo.Append($"TotalNumberOfPairedHostName : {CurrentDeviceInfo.TotalNumberOfPairedHostName}");
                localDeviceInfo.Append(Environment.NewLine);
                localDeviceInfo.Append($"TouchScrollSensitivityLevel : {CurrentDeviceInfo.TouchScrollSensitivityLevel}");
                localDeviceInfo.Append(Environment.NewLine);
                localDeviceInfo.Append($"TouchSensitivityLevelValue : {CurrentDeviceInfo.TouchSensitivityLevelValue}");
                localDeviceInfo.Append(Environment.NewLine);
                localDeviceInfo.Append($"Type : {CurrentDeviceInfo.Type}");
                localDeviceInfo.Append(Environment.NewLine);
                localDeviceInfo.Append($"VisiblePairedHostName1 : {CurrentDeviceInfo.VisiblePairedHostName1}");
                localDeviceInfo.Append(Environment.NewLine);
                localDeviceInfo.Append($"VisiblePairedHostName2 : {CurrentDeviceInfo.VisiblePairedHostName2}");
                localDeviceInfo.Append(Environment.NewLine);
                localDeviceInfo.Append($"VisiblePairedHostName3 : {CurrentDeviceInfo.VisiblePairedHostName3}");
                localDeviceInfo.Append(Environment.NewLine);
                localDeviceInfo.Append($"SelectedPreset : {CurrentDeviceInfo.SelectedPreset}");
                localDeviceInfo.Append(Environment.NewLine);
                localDeviceInfo.Append($"IsEqualizerSupported : {CurrentDeviceInfo.IsEqualizerSupported}");
                localDeviceInfo.Append(Environment.NewLine);
                localDeviceInfo.Append($"WiredAudioVolumeAdjustmentTone : {CurrentDeviceInfo.WiredAudioVolumeAdjustmentTone}");
                localDeviceInfo.Append(Environment.NewLine);
                localDeviceInfo.Append($"IsWiredAudioIMicNSEnable : {CurrentDeviceInfo.IsWiredAudioIMicNSEnable}");
                localDeviceInfo.Append(Environment.NewLine);
                localDeviceInfo.Append($"IsWiredAudioMicMuteSoundEnable : {CurrentDeviceInfo.IsWiredAudioMicMuteSoundEnable}");
                localDeviceInfo.Append(Environment.NewLine);
                localDeviceInfo.Append($"MuteStatus : {CurrentDeviceInfo.MuteStatus}");
                localDeviceInfo.Append(Environment.NewLine);
            }
            else
            {
                localDeviceInfo.Append($"ID : {CurrentDeviceInfo.ID}");
                localDeviceInfo.Append(Environment.NewLine);
                localDeviceInfo.Append($"PhyscialDeviceID : {CurrentDeviceInfo.PhyscialDeviceID}");
                localDeviceInfo.Append(Environment.NewLine);
                localDeviceInfo.Append($"Name : {CurrentDeviceInfo.Name}");
                localDeviceInfo.Append(Environment.NewLine);
                localDeviceInfo.Append($"BackLightingControls : {CurrentDeviceInfo.BackLightingControls}");
                localDeviceInfo.Append(Environment.NewLine);
                localDeviceInfo.Append($"BackLightingLevel : {CurrentDeviceInfo.BackLightingLevel}");
                localDeviceInfo.Append(Environment.NewLine);
                localDeviceInfo.Append($"BackLightTabIndex : {CurrentDeviceInfo.BackLightTabIndex}");
                localDeviceInfo.Append(Environment.NewLine);
                localDeviceInfo.Append($"BatteryLevel : {CurrentDeviceInfo.BatteryLevel}");
                localDeviceInfo.Append(Environment.NewLine);
                localDeviceInfo.Append($"BatteryStatus : {CurrentDeviceInfo.BatteryStatus}");
                localDeviceInfo.Append(Environment.NewLine);
                localDeviceInfo.Append($"CollabsKeysSupported : {CurrentDeviceInfo.CollabsKeysSupported}");
                localDeviceInfo.Append(Environment.NewLine);
                localDeviceInfo.Append($"ColorCode : {CurrentDeviceInfo.ColorCode}");
                localDeviceInfo.Append(Environment.NewLine);
                localDeviceInfo.Append($"DeviceName : {CurrentDeviceInfo.DeviceName}");
                localDeviceInfo.Append(Environment.NewLine);
                localDeviceInfo.Append($"DpiDelta : {CurrentDeviceInfo.DpiDelta}");
                localDeviceInfo.Append(Environment.NewLine);
                localDeviceInfo.Append($"DPILevel : {CurrentDeviceInfo.DPILevel}");
                localDeviceInfo.Append(Environment.NewLine);
                localDeviceInfo.Append($"DpiLevel : {CurrentDeviceInfo.DpiLevel}");
                localDeviceInfo.Append(Environment.NewLine);
                localDeviceInfo.Append($"DPIValue : {CurrentDeviceInfo.DpiValue}");
                localDeviceInfo.Append(Environment.NewLine);
                localDeviceInfo.Append($"DpiLevelValues : {CurrentDeviceInfo.DpiLevelValues}");
                localDeviceInfo.Append(Environment.NewLine);
                localDeviceInfo.Append($"DpiMax : {CurrentDeviceInfo.DpiMax}");
                localDeviceInfo.Append(Environment.NewLine);
                localDeviceInfo.Append($"DpiMin : {CurrentDeviceInfo.DpiMin}");
                localDeviceInfo.Append(Environment.NewLine);
                localDeviceInfo.Append($"FirmwareVersion : {CurrentDeviceInfo.FirmwareVersion}");
                localDeviceInfo.Append(Environment.NewLine);
                localDeviceInfo.Append($"InstanceId : {CurrentDeviceInfo.InstanceId}");
                localDeviceInfo.Append(Environment.NewLine);
                localDeviceInfo.Append($"InstanceNumber : {CurrentDeviceInfo.InstanceNumber}");
                localDeviceInfo.Append(Environment.NewLine);
                localDeviceInfo.Append($"InterfaceType : {CurrentDeviceInfo.InterfaceType}");
                localDeviceInfo.Append(Environment.NewLine);
                localDeviceInfo.Append($"IsBatteryLevelSupported : {CurrentDeviceInfo.IsBatteryLevelSupported}");
                localDeviceInfo.Append(Environment.NewLine);
                localDeviceInfo.Append($"IsCollaborationBlinkEffectEnable : {CurrentDeviceInfo.IsCollaborationBlinkEffectEnable}");
                localDeviceInfo.Append(Environment.NewLine);
                localDeviceInfo.Append($"IsCollaborationCameraEnable : {CurrentDeviceInfo.IsCollaborationCameraEnable}");
                localDeviceInfo.Append(Environment.NewLine);
                localDeviceInfo.Append($"IsCollaborationChatEnable : {CurrentDeviceInfo.IsCollaborationChatEnable}");
                localDeviceInfo.Append(Environment.NewLine);
                localDeviceInfo.Append($"IsCollaborationDoubleTapEnable : {CurrentDeviceInfo.IsCollaborationDoubleTapEnable}");
                localDeviceInfo.Append(Environment.NewLine);
                localDeviceInfo.Append($"IsCollaborationKeyEnable : {CurrentDeviceInfo.IsCollaborationKeyEnable}");
                localDeviceInfo.Append(Environment.NewLine);
                localDeviceInfo.Append($"IsCollaborationMicEnable : {CurrentDeviceInfo.IsCollaborationMicEnable}");
                localDeviceInfo.Append(Environment.NewLine);
                localDeviceInfo.Append($"IsCollaborationScreenShareEnable : {CurrentDeviceInfo.IsCollaborationScreenShareEnable}");
                localDeviceInfo.Append(Environment.NewLine);
                localDeviceInfo.Append($"IsCollabsKeysSupported : {CurrentDeviceInfo.IsCollabsKeysSupported}");
                localDeviceInfo.Append(Environment.NewLine);
                localDeviceInfo.Append($"IsConnected : {CurrentDeviceInfo.IsConnected}");
                localDeviceInfo.Append(Environment.NewLine);
                localDeviceInfo.Append($"IsDPILevelChangePending : {CurrentDeviceInfo.IsDPILevelChangePending}");
                localDeviceInfo.Append(Environment.NewLine);
                localDeviceInfo.Append($"IsDPILevelSupported : {CurrentDeviceInfo.IsDPILevelSupported}");
                localDeviceInfo.Append(Environment.NewLine);
                localDeviceInfo.Append($"IsDPIValueChangePending : {CurrentDeviceInfo.IsDPIValueChangePending}");
                localDeviceInfo.Append(Environment.NewLine);
                localDeviceInfo.Append($"IsDPIValueSupported : {CurrentDeviceInfo.IsDPIValueSupported}");
                localDeviceInfo.Append(Environment.NewLine);
                localDeviceInfo.Append($"IsIlluminationSupported : {CurrentDeviceInfo.IsIlluminationSupported}");
                localDeviceInfo.Append(Environment.NewLine);
                localDeviceInfo.Append($"IsPhysicalDeviceDongle : {CurrentDeviceInfo.IsPhysicalDeviceDongle}");
                localDeviceInfo.Append(Environment.NewLine);
                localDeviceInfo.Append($"IsReportRateSupported : {CurrentDeviceInfo.IsReportRateSupported}");
                localDeviceInfo.Append(Environment.NewLine);
                localDeviceInfo.Append($"IsTouchScrollSensitivitySupported : {CurrentDeviceInfo.IsTouchScrollSensitivitySupported}");
                localDeviceInfo.Append(Environment.NewLine);
                localDeviceInfo.Append($"LogicalDeviceType : {CurrentDeviceInfo.LogicalDeviceType}");
                localDeviceInfo.Append(Environment.NewLine);
                localDeviceInfo.Append($"MaxPairingSlots : {CurrentDeviceInfo.MaxPairingSlots}");
                localDeviceInfo.Append(Environment.NewLine);
                localDeviceInfo.Append($"ModelNumber : {CurrentDeviceInfo.ModelNumber}");
                localDeviceInfo.Append(Environment.NewLine);
                localDeviceInfo.Append($"MousePrimaryButton : {CurrentDeviceInfo.MousePrimaryButton}");
                localDeviceInfo.Append(Environment.NewLine);
                localDeviceInfo.Append($"MuteStatus : {CurrentDeviceInfo.MuteStatus}");
                localDeviceInfo.Append(Environment.NewLine);
                localDeviceInfo.Append($"OdmId : {CurrentDeviceInfo.OdmId}");
                localDeviceInfo.Append(Environment.NewLine);
                localDeviceInfo.Append($"PairedDeviceCount : {CurrentDeviceInfo.PairedDeviceCount}");
                localDeviceInfo.Append(Environment.NewLine);
                localDeviceInfo.Append($"PairedHostName1 : {CurrentDeviceInfo.PairedHostName1}");
                localDeviceInfo.Append(Environment.NewLine);
                localDeviceInfo.Append($"PairedHostName2 : {CurrentDeviceInfo.PairedHostName2}");
                localDeviceInfo.Append(Environment.NewLine);
                localDeviceInfo.Append($"PairedHostName3 : {CurrentDeviceInfo.PairedHostName3}");
                localDeviceInfo.Append(Environment.NewLine);
                localDeviceInfo.Append($"PairingStatusName : {CurrentDeviceInfo.PairingStatusName}");
                localDeviceInfo.Append(Environment.NewLine);
                localDeviceInfo.Append($"PhysicalDeviceFirmwareVersion : {CurrentDeviceInfo.PhysicalDeviceFirmwareVersion}");
                localDeviceInfo.Append(Environment.NewLine);
                localDeviceInfo.Append($"PhysicalDeviceType : {CurrentDeviceInfo.PhysicalDeviceType}");
                localDeviceInfo.Append(Environment.NewLine);
                localDeviceInfo.Append($"PluginId : {CurrentDeviceInfo.PluginId}");
                localDeviceInfo.Append(Environment.NewLine);
                localDeviceInfo.Append($"ReportRate : {CurrentDeviceInfo.ReportRate}");
                localDeviceInfo.Append(Environment.NewLine);
                localDeviceInfo.Append($"Status : {CurrentDeviceInfo.Status}");
                localDeviceInfo.Append(Environment.NewLine);
                localDeviceInfo.Append($"TotalNumberOfPairedHostName : {CurrentDeviceInfo.TotalNumberOfPairedHostName}");
                localDeviceInfo.Append(Environment.NewLine);
                localDeviceInfo.Append($"TouchScrollSensitivityLevel : {CurrentDeviceInfo.TouchScrollSensitivityLevel}");
                localDeviceInfo.Append(Environment.NewLine);
                localDeviceInfo.Append($"TouchSensitivityLevelValue : {CurrentDeviceInfo.TouchSensitivityLevelValue}");
                localDeviceInfo.Append(Environment.NewLine);
                localDeviceInfo.Append($"Type : {CurrentDeviceInfo.Type}");
                localDeviceInfo.Append(Environment.NewLine);
                localDeviceInfo.Append($"VisiblePairedHostName1 : {CurrentDeviceInfo.VisiblePairedHostName1}");
                localDeviceInfo.Append(Environment.NewLine);
                localDeviceInfo.Append($"VisiblePairedHostName2 : {CurrentDeviceInfo.VisiblePairedHostName2}");
                localDeviceInfo.Append(Environment.NewLine);
                localDeviceInfo.Append($"VisiblePairedHostName3 : {CurrentDeviceInfo.VisiblePairedHostName3}");
            }
            //this.DeviceInfo = localDeviceInfo.ToString();
#endif
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

        //public string? DeviceInfo { get; set; }
        public Guid CurrentDeviceID { get; set; } = Guid.NewGuid();
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

        public string DeviceID
        {
            get => _deviceId;
            set
            {
                if (_deviceId != value)
                {
                    _deviceId = value;
                    OnPropertyChanged();
                }
            }
        }

        public string DeviceID2
        {
            get => _deviceId2;
            set
            {
                if (_deviceId2 != value)
                {
                    _deviceId2 = value;
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
        private readonly List<VbarItem1> _vbarItems = new();

        /// <summary>
        /// Called when the ModuleGroups reset, will return to Landing Mode
        ///
        /// </summary>
        private void RebuildVbarItems()
        {
            // Vbar animation dark\light mode
            DdpmCommonHelper.BitmapImageUpdated -= OnVBarThemeChange;
            DdpmCommonHelper.BitmapImageUpdated += OnVBarThemeChange;

            _vbarItems.Clear();

            int idx = 0;
            foreach (ModuleGroup mg in ModuleGroups)
            {
                if (mg.GroupIcon != null)
                {
                    VbarItem1 vbarItem = new(idx, mg.GroupIcon, mg.GroupName, mg.GroupIconCanvas)
                    {
                        ClickCommand = VbarItemClickCommand
                    };
                    _vbarItems.Add(vbarItem);
                    idx++;
                }
            }
            OnPropertyChanged(nameof(VbarItems));
        }

        public List<VbarItem1> VbarItems
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
                if (header != null &&
                    header.DdpmModule != null)
                {
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
                if (_rightViewHeaders.Count == 0 &&
                    ModuleGroups.Count > 0 && // If _vbarItems is empty, will build the list from ModuleGroups
                    ((VbarSelectedIndex >= 0) || (VbarSelectedIndex < ModuleGroups.Count))) //Get the selected ModuleGroup
                {
                    ModuleGroup mg = ModuleGroups[VbarSelectedIndex];
                    _rightViewHeaders = mg.Headers;
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
            foreach (VbarItem1 vbarItem in _vbarItems)
            {
                vbarItem.SetLadningMode(isLandingMode);
            }
        }

        public void SelectVBar()
        {
            foreach (VbarItem1 vbarItem in _vbarItems)
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

        public bool CheckChar(string ch)
        {
            //if (char.IsLetterOrDigit(ch))
            //    return true;
            //if (ch == ' ' || ch == '@' || ch == 'e')
            //    return true;
            //return false;
            Regex regex = new Regex("^[0-9a-zA-Z @-]+$");
            return regex.IsMatch(ch);
        }

        private bool _imgBL1;
        public bool ImgBL1
        {
            get => _imgBL1;
            set
            {
                _imgBL1 = value;
                OnPropertyChanged();
            }
        }

        private bool _imgBL2;
        public bool ImgBL2
        {
            get => _imgBL2;
            set
            {
                _imgBL2 = value;
                OnPropertyChanged();
            }
        }

        private bool _imgBL3;
        public bool ImgBL3
        {
            get => _imgBL3;
            set
            {
                _imgBL3 = value;
                OnPropertyChanged();
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

        private void OnVBarThemeChange(OSThemeEnum oSThemeEnum)
        {
            foreach (var vbar in _vbarItems)
            {
                vbar.OnThemeChangeRefresh();
            }
        }

        ~PeripheralViewModel()
        {
            DdpmCommonHelper.BitmapImageUpdated -= OnVBarThemeChange;
        }
    }
}