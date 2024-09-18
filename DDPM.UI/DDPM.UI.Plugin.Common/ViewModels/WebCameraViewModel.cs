using DDPM.SA.Common;
using DDPM.SA.Common.Settings;
using DDPM.UI.Common;
using DDPM.UI.Plugin.Common;
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
using Windows.Media.MediaProperties;
using Windows.Storage;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace DDPM.UI.Plugin.ViewModels
{
    public class WebCameraViewModel : PeripheralViewModel, INotifyPropertyChanged
    {
        #region Variables
        private readonly ILog _log;
        private readonly IDeviceManagerSA _deviceManager;

        // Query all properties [resolution and frame rate] of the webcam device
        public IEnumerable<StreamResolution> allProperties;

        public bool[] Resolution_IsSelected { get; set; } = new bool[3];
        public bool[] FPS_IsSelected { get; set; } = new bool[3];


        private ObservableCollection<ProfileItem> _profileItems = new();
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

            strCurrent_Resolution = "1920x1080";
            strCurrent_Framerate = "30FPS";

            SetResolution_Selected(1);
            SetFPS_Selected(1);

        }

        public void SetResolution_Selected(int index)
        {
            for (int j = 0; j < Resolution_IsSelected.Length; j++)
            {
                Resolution_IsSelected[j] = false;
            }
            Resolution_IsSelected[index] = true;
            OnPropertyChanged("Resolution_IsSelected");
        }

        public void SetFPS_Selected(int index)
        {
            for (int j = 0; j < FPS_IsSelected.Length; j++)
            {
                FPS_IsSelected[j] = false;
            }
            FPS_IsSelected[index] = true;
            OnPropertyChanged("FPS_IsSelected");
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

            //_profileItems.Clear();
            Application.Current.Dispatcher.Invoke(() =>
            {
                _profileItems.Clear();
                _profileItems.Add(new ProfileItem
                {
                    ID = "Custom Profile: Profile 1",
                    Caption = Utility.CheckTextLength("Custom Profile: Profile 1", 120, 14),
                    Tooltip = "",
                    TooltipVisibility = Visibility.Collapsed,
                    ButtonVisibility = Visibility.Visible
                });
                _profileItems.Add(new ProfileItem
                {
                    ID = "Custom Profile: Profile 2",
                    Caption = Utility.CheckTextLength("Custom Profile: Profile 2", 120, 14),
                    Tooltip = "",
                    TooltipVisibility = Visibility.Collapsed,
                    ButtonVisibility = Visibility.Visible
                });
                _profileItems.Add(new ProfileItem
                {
                    ID = "Custom Profile: Profile 3",
                    Caption = Utility.CheckTextLength("Custom Profile: Profile 3", 120, 14),
                    Tooltip = "",
                    TooltipVisibility = Visibility.Collapsed,
                    ButtonVisibility = Visibility.Visible
                });
                _profileItems.Add(new ProfileItem
                {
                    ID = "Custom Profile: Profile 4",
                    Caption = Utility.CheckTextLength("Custom Profile: Profile 4", 120, 14),
                    Tooltip = "",
                    TooltipVisibility = Visibility.Collapsed,
                    ButtonVisibility = Visibility.Visible
                });
                Profiles.Clear();
                foreach (var profile in CurrentDeviceInfo!.CustomProfiles.ToObject<List<WebcamProfile>>()!)
                {
                    Profiles.Add(profile.Name, profile);
                }

                foreach (var profile in CurrentDeviceInfo.PresetProfiles.ToObject<List<WebcamProfile>>()!.ToList().OrderBy(x => x.Name))
                {
                    Profiles.Add(profile.Name, profile);
                }
                _profileItems.Add(new ProfileItem
                {
                    ID = LangHelper.Instance["Default"],
                    Caption = LangHelper.Instance["Default"],
                    Tooltip = Strings.DefaultProfileTooltip,
                    TooltipVisibility = Visibility.Visible,
                    ButtonVisibility = Visibility.Collapsed
                });
                _profileItems.Add(new ProfileItem
                {
                    ID = Strings.Smooth,
                    Caption = Strings.Smooth,
                    Tooltip = Strings.SmoothProfileTooltip,
                    TooltipVisibility = Visibility.Visible,
                    ButtonVisibility = Visibility.Collapsed
                });
                _profileItems.Add(new ProfileItem
                {
                    ID = Strings.Vibrant,
                    Caption = Strings.Vibrant,
                    Tooltip = Strings.VibrantProfileTooltip,
                    TooltipVisibility = Visibility.Visible,
                    ButtonVisibility = Visibility.Collapsed
                });
                _profileItems.Add(new ProfileItem
                {
                    ID = Strings.Warm,
                    Caption = Strings.Warm,
                    Tooltip = Strings.WarmProfileTooltip,
                    TooltipVisibility = Visibility.Visible,
                    ButtonVisibility = Visibility.Collapsed
                });

            });

            CurrentProfileName = CurrentDeviceInfo!.ProfileName;

            OnPropertyChanged(nameof(IsMicEnumerationOn));
            OnPropertyChanged(nameof(IsMicEnumerationOnText));

            IsMicEnumerationOnEnabled = true;
            AlertVisibility = Visibility.Collapsed;
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

        // 20240911 jim add
        public string strCurrent_Resolution;
        public string strCurrent_Framerate;



        //20240702
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

        private bool countdown_isChecked;

        public bool Countdown_IsChecked
        {
            get { return countdown_isChecked; }
            set
            {
                countdown_isChecked = value;
                OnPropertyChanged("Countdown_IsChecked");
            }
        }

        private string media_file_location;

        public string Media_File_Location
        {
            get { return media_file_location; }
            set
            {
                media_file_location = value;
                OnPropertyChanged("Media_File_Location");
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
        private Visibility alertVisibility = Visibility.Collapsed;
        public Visibility AlertVisibility
        {
            get => alertVisibility;
            set
            {
                alertVisibility = value;
                OnPropertyChanged();
            }
        }
        public ObservableCollection<ProfileItem> ProfileItems { get => _profileItems; }
    }

    public class StreamResolution
    {
        private IMediaEncodingProperties _properties;

        public StreamResolution(IMediaEncodingProperties properties)
        {
            if (properties == null)
            {
                throw new ArgumentNullException(nameof(properties));
            }

            // Only handle ImageEncodingProperties and VideoEncodingProperties, which are the two types that GetAvailableMediaStreamProperties can return
            if (!(properties is ImageEncodingProperties) && !(properties is VideoEncodingProperties))
            {
                throw new ArgumentException("Argument is of the wrong type. Required: " + typeof(ImageEncodingProperties).Name
                    + " or " + typeof(VideoEncodingProperties).Name + ".", nameof(properties));
            }

            // Store the actual instance of the IMediaEncodingProperties for setting them later
            _properties = properties;
        }

        public uint Width
        {
            get
            {
                if (_properties is ImageEncodingProperties)
                {
                    return (_properties as ImageEncodingProperties).Width;
                }
                else if (_properties is VideoEncodingProperties)
                {
                    return (_properties as VideoEncodingProperties).Width;
                }

                return 0;
            }
        }

        public uint Height
        {
            get
            {
                if (_properties is ImageEncodingProperties)
                {
                    return (_properties as ImageEncodingProperties).Height;
                }
                else if (_properties is VideoEncodingProperties)
                {
                    return (_properties as VideoEncodingProperties).Height;
                }

                return 0;
            }
        }

        public uint FrameRate
        {
            get
            {
                if (_properties is VideoEncodingProperties)
                {
                    if ((_properties as VideoEncodingProperties).FrameRate.Denominator != 0)
                    {
                        return (_properties as VideoEncodingProperties).FrameRate.Numerator / (_properties as VideoEncodingProperties).FrameRate.Denominator;
                    }
                }

                return 0;
            }
        }

        public double AspectRatio
        {
            get { return Math.Round((Height != 0) ? (Width / (double)Height) : double.NaN, 2); }
        }

        public IMediaEncodingProperties EncodingProperties
        {
            get { return _properties; }
        }

        /// <summary>
        /// Output properties to a readable format for UI purposes
        /// eg. 1920x1080 [1.78] 30fps MPEG
        /// </summary>
        /// <returns>Readable string</returns>
        public string GetFriendlyName(bool showFrameRate = true)
        {
            if (_properties is ImageEncodingProperties ||
                !showFrameRate)
            {
                return Width + "x" + Height + " [" + AspectRatio + "] " + _properties.Subtype;
            }
            else if (_properties is VideoEncodingProperties)
            {
                return Width + "x" + Height + " [" + AspectRatio + "] " + FrameRate + "FPS " + _properties.Subtype;
            }

            return string.Empty;
        }
    }

    public class ProfileItem
    {
        public required string ID { get; set; }
        public required string Caption { get; set; }
        public required string Tooltip { get; set; }
        public required Visibility TooltipVisibility { get; set; }
        public required Visibility ButtonVisibility { get; set; }
    }
}