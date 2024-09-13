using DDPM.SA.Common;
using DDPM.UI.Common;
using DDPM.UI.Resources.Helper;
using Dell.Client.Framework.Common;
using Dell.Client.Framework.UX.WPF;
using Microsoft;
using Newtonsoft.Json.Linq;
using System;
using System.Buffers;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Threading;
using Windows.Media.Capture;
using Windows.Media.Capture.Frames;
using Windows.Storage;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace DDPM.UI.Plugin.ViewModels
{
    public class WebCameraViewModel : PeripheralViewModel, INotifyPropertyChanged
    {
        #region Variables
        private readonly ILog _log;
        private readonly IDeviceManagerSA _deviceManager;

        private readonly ObservableCollection<ProfileItem> _profileItems = new();
        private readonly Dictionary<string, WebcamProfile> Profiles = new();

        #endregion Variables

        public string CurrentProfileName = "";

        public new event PropertyChangedEventHandler? PropertyChanged;

        public WebCameraViewModel(IConsole console, ILog log, IDeviceManagerSA deviceManager) : base(console, log, deviceManager)
        {
            Requires.NotNull(console, nameof(console));
            Requires.NotNull(log, nameof(log));

            _log = log;
            _deviceManager = deviceManager;

            IsChecked_FramingGrid = Visibility.Hidden;
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
                if (deviceInfo.LogicalDeviceType.Contains("Webcam"))
                {
                    DeviceInfos.Add(deviceInfo.ID, deviceInfo);
                }
            }
        }
        public override bool SetCurrentDevice(string deviceID)
        {
            deviceID ??= DeviceInfos.Values.ToList().FirstOrDefault()!.ID.ToString();

            if (!base.SetCurrentDevice(deviceID))
            { return false; }

            _profileItems.Clear();
            Profiles.Clear();
            foreach (var profile in CurrentDeviceInfo!.CustomProfiles.ToObject<List<WebcamProfile>>()!)
            {
                Profiles.Add(profile.Name, profile);
            }
            _profileItems.Add(new ProfileItem
            {
                Caption = "Custom Profile: Profile 1",
                Tooltip = "",
                TooltipVisibility = Visibility.Collapsed,
                ButtonVisibility = Visibility.Visible
            });
            _profileItems.Add(new ProfileItem
            {
                Caption = "Custom Profile: Profile 2",
                Tooltip = "",
                TooltipVisibility = Visibility.Collapsed,
                ButtonVisibility = Visibility.Visible
            });
            _profileItems.Add(new ProfileItem
            {
                Caption = "Custom Profile: Profile 3",
                Tooltip = "",
                TooltipVisibility = Visibility.Collapsed,
                ButtonVisibility = Visibility.Visible
            });
            _profileItems.Add(new ProfileItem
            {
                Caption = "Custom Profile: Profile 4",
                Tooltip = "",
                TooltipVisibility = Visibility.Collapsed,
                ButtonVisibility = Visibility.Visible
            });


            foreach (var profile in CurrentDeviceInfo.PresetProfiles.ToObject<List<WebcamProfile>>()!.ToList().OrderBy(x => x.Name))
            {
                Profiles.Add(profile.Name, profile);
            }
            _profileItems.Add(new ProfileItem
            {
                Caption = LangHelper.Instance["Default"],
                Tooltip = Strings.DefaultProfileTooltip,
                TooltipVisibility = Visibility.Visible,
                ButtonVisibility = Visibility.Collapsed
            });
            _profileItems.Add(new ProfileItem
            {
                Caption = Strings.Smooth,
                Tooltip = Strings.SmoothProfileTooltip,
                TooltipVisibility = Visibility.Visible,
                ButtonVisibility = Visibility.Collapsed
            });
            _profileItems.Add(new ProfileItem
            {
                Caption = Strings.Vibrant,
                Tooltip = Strings.VibrantProfileTooltip,
                TooltipVisibility = Visibility.Visible,
                ButtonVisibility = Visibility.Collapsed
            });
            _profileItems.Add(new ProfileItem
            {
                Caption = Strings.Warm,
                Tooltip = Strings.WarmProfileTooltip,
                TooltipVisibility = Visibility.Visible,
                ButtonVisibility = Visibility.Collapsed
            });

            CurrentProfileName = CurrentDeviceInfo.ProfileName;

            OnPropertyChanged(nameof(IsMicEnumerationOn));
            OnPropertyChanged(nameof(IsMicEnumerationOnText));

            IsMicEnumerationOnEnabled = true;

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
                        switch (property)
                        {
                            //case "MousePrimaryButtonChanged":
                            //  PrimaryButtonIndex = (int)di.MousePrimaryButton;
                            //  break;
                            //case "TouchScrollSensitivityLevelChanged":
                            //  TouchScrollSensitivityLevel = di.TouchScrollSensitivityLevel;
                            //  break;
                            //case "DpiValueChanged":
                            //  DPIValue = int.Parse(di.DPIValue);
                            //  break;
                            default:
                                break;
                        }
                        GenerateInfo();
                    }
                    break;

                default:
                    break;
            }
        }

        public MediaCapture? _mediaCapture;
        public MediaFrameReader _mediaFrameReader;

        public bool captureManagerInitialized = false;
        public bool _running = false;
        public bool _isRecording;
        public StorageFolder _captureFolder;
        private bool isChecked_Autofocus;

        public bool IsChecked_Autofocus
        {
            get { return isChecked_Autofocus; }
            set
            {
                isChecked_Autofocus = value;
                OnPropertyChanged("IsChecked_Autofocus");
            }
        }

        private string autofocusStatus_String = "";

        public string AutofocusStatus_String
        {
            get { return autofocusStatus_String; }
            set
            {
                autofocusStatus_String = value;
                OnPropertyChanged("AutofocusStatus_String");
            }
        }

        private bool isChecked_AWB;

        public bool IsChecked_AWB
        {
            get { return isChecked_AWB; }
            set
            {
                isChecked_AWB = value;
                OnPropertyChanged(nameof(IsChecked_AWB));
            }
        }

        private string awbStatus_String = "";

        public string AWBStatus_String
        {
            get { return awbStatus_String; }
            set
            {
                awbStatus_String = value;
                OnPropertyChanged("AWBStatus_String");
            }
        }

        private bool[] _fOV_IsSelected = new bool[3];

        public bool[] FOV_IsSelected
        {
            get { return _fOV_IsSelected; }
            set
            {
                _fOV_IsSelected = value;
                OnPropertyChanged("FOV_IsSelected");
            }
        }

        private Visibility isChecked_FramingGrid;

        public Visibility IsChecked_FramingGrid
        {
            get { return isChecked_FramingGrid; }
            set
            {
                isChecked_FramingGrid = value;
                OnPropertyChanged("IsChecked_FramingGrid");
            }
        }

        private bool framingGrid_isChecked;

        public bool FramingGrid_IsChecked
        {
            get { return framingGrid_isChecked; }
            set
            {
                framingGrid_isChecked = value;
                OnPropertyChanged("FramingGrid_IsChecked");
            }
        }
        public string IsMicEnumerationOnText
        {
            get => CurrentDeviceInfo!.IsMicEnumerationOn ? Strings.On : Strings.Off;
        }
        public bool IsMicEnumerationOn
        {
            get => CurrentDeviceInfo!.IsMicEnumerationOn;
            set
            {
                //_deviceManager.SetIsMicEnumerationOn(CurrentDeviceInfo!.ID.ToString(), value);
                _deviceManager.SetIsMicEnumerationOn(value, CurrentDeviceInfo!.ID);
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsMicEnumerationOnText));
            }
        }
        private bool isMicEnumerationOnEnabled;
        public bool IsMicEnumerationOnEnabled
        {
            get => isMicEnumerationOnEnabled;
            set
            {
                isMicEnumerationOnEnabled = value;
                OnPropertyChanged();
            }
        }
        public ObservableCollection<ProfileItem> ProfileItems { get => _profileItems; }
    }

    public class ProfileItem
    {
        public required string Caption { get; set; }
        public required string Tooltip { get; set; }
        public required Visibility TooltipVisibility { get; set; }
        public required Visibility ButtonVisibility { get; set; }
    }
}