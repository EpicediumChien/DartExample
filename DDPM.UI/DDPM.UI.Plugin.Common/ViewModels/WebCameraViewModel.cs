using DDPM.SA.Common;
using DDPM.SA.Common.Settings;
using DDPM.UI.Common;
using DDPM.UI.Plugin.Common;
using DDPM.UI.Resources.Helper;
using Dell.Client.Framework.Common;
using Dell.Client.Framework.UX.WPF;
using Microsoft;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media.Animation;
using Windows.Media.Capture;
using Windows.Media.Capture.Frames;
using Windows.Media.MediaProperties;
using Windows.Storage;
using WebcamProfile = DDPM.UI.Common.WebcamProfile;

namespace DDPM.UI.Plugin.ViewModels
{
    public class WebCameraViewModel : PeripheralViewModel, INotifyPropertyChanged
    {
        #region Variables
        private readonly ILog _log;
        private ObservableCollection<ProfileItem> _profileItems = new();
        private readonly Dictionary<string, WebcamProfile> Profiles = new();
        private List<string> _resolutions = new();

        // Query all properties [resolution and frame rate] of the webcam device
        public IEnumerable<StreamResolution> allProperties;

        public bool[] Resolution_IsSelected { get; set; } = new bool[4];
        public bool[] FPS_IsSelected { get; set; } = new bool[3];



        #endregion Variables

        public WebcamProfile CurrentProfile = new();
        public List<string> FPSs = new();


        // 20240926 jim add
        private bool showLockMask = false;

        public bool ShowLockMask
        {
            get { return showLockMask; }
            set
            {
                showLockMask = value;
                LockMaskVisible = showLockMask ? Visibility.Visible : Visibility.Collapsed;
                OnPropertyChanged("ShowLockMask");
            }
        }

        private Visibility lockMaskVisible = Visibility.Collapsed;

        public Visibility LockMaskVisible
        {
            get { return lockMaskVisible; }
            set
            {
                lockMaskVisible = value;
                OnPropertyChanged("LockMaskVisible");
            }
        }

        private bool _isTabStoppable;

        public bool isTabStoppable
        {
            get { return _isTabStoppable; }
            set
            {
                _isTabStoppable = value;
                OnPropertyChanged("isTabStoppable");
            }
        }

        private string _TabNavigation = "Cycle";

        public string TabNavigation
        {
            get { return _TabNavigation; }
            set
            {
                _TabNavigation = value;
                OnPropertyChanged("TabNavigation");
            }
        }


        public event EventHandler<EventArgs> WebcamSettingChanged;
        public new event PropertyChangedEventHandler? PropertyChanged;

        public WebCameraViewModel(IConsole console, ILog log) : base(console, log, DdpmCommonHelper.DeviceManagerSA!)
        {
            Requires.NotNull(console, nameof(console));
            Requires.NotNull(log, nameof(log));

            _log = log;
            //_deviceManager = deviceManager;

            IsChecked_FramingGrid = Visibility.Hidden;

            //strCurrent_Resolution = "1920x1080";
            //strCurrent_Framerate = "30FPS";

        }

        public void SetResolution_Selected(int index)
        {
            for (int j = 0; j < Resolution_IsSelected.Length; j++)
            {
                Resolution_IsSelected[j] = false;
            }
            Resolution_IsSelected[index] = true;
            WebcamSettings.SelectedResolution = _resolutions[index];
            if (!WebcamSettings.SelectedFPSs.ContainsKey(WebcamSettings.SelectedResolution))
            { WebcamSettings.SelectedFPSs.Add(WebcamSettings.SelectedResolution, WebcamSettings.SupportedFPSs[WebcamSettings.SelectedResolution][0]); }
            WebcamSettings.ExportWebcamSettings(WebcamSettings, Model);
            OnPropertyChanged(nameof(Resolution_IsSelected));
        }

        public void SetFPS_Selected(int index)
        {
            for (int j = 0; j < FPS_IsSelected.Length; j++)
            {
                FPS_IsSelected[j] = false;
            }
            FPS_IsSelected[index] = true;
            WebcamSettings.SelectedFPSs[WebcamSettings.SelectedResolution] = WebcamSettings.SupportedFPSs[WebcamSettings.SelectedResolution][index];
            WebcamSettings.ExportWebcamSettings(WebcamSettings, Model);
            OnPropertyChanged(nameof(FPS_IsSelected));
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

            OnPropertyChanged(nameof(IsMicEnumerationOn));
            OnPropertyChanged(nameof(IsMicEnumerationOnText));

            FPSs.Clear();

            InitializeWebcam();
            WebcamSettingChanged?.Invoke(this, EventArgs.Empty);

            IsMicEnumerationOnEnabled = true;
            AlertVisibility = Visibility.Collapsed;
            return true;
        }

        private void InitializeWebcam()
        {
            WebcamSettings = WebcamSettings.ImportWebcamSettings(Model);
            if (string.IsNullOrEmpty(WebcamSettings.SelectedResolution))
            {
                foreach (var res in CurrentDeviceInfo!.SupportedResolutions)
                {
                    var sts = res.Split(';');
                    if (!WebcamSettings.SupportedFPSs.ContainsKey(sts[2]))
                    { WebcamSettings.SupportedFPSs.Add(sts[2], new List<string>()); }
                    if (!WebcamSettings.SupportedFPSs[sts[2]].Contains(sts[1]))
                    { WebcamSettings.SupportedFPSs[sts[2]].Add(sts[1]); }
                    if (!WebcamSettings.Resolutions.ContainsKey(sts[2]))
                    {
                        WebcamSettings.Resolutions.Add(sts[2], sts[0]);
                    }

                }
                WebcamSettings.SelectedResolution = WebcamSettings.SupportedFPSs.Keys.FirstOrDefault() ?? "";
                WebcamSettings.SelectedFPSs.Add(WebcamSettings.SelectedResolution, WebcamSettings.SupportedFPSs[WebcamSettings.SelectedResolution].FirstOrDefault() ?? "");

                foreach (var profile in CurrentDeviceInfo.PresetProfiles.ToObject<List<WebcamProfile>>()!.ToList().OrderBy(x => x.Name))
                {
                    WebcamSettings.PresetProfiles.Add(profile.Name, profile);
                }
                WebcamSettings.SelectedProfileName = WebcamSettings.PresetProfiles.Values.ToList()[0].Name;

                WebcamSettings.ExportWebcamSettings(WebcamSettings, Model);
            }
            SetProfile();

            _resolutions = WebcamSettings.Resolutions.Keys.ToList();
            var i = WebcamSettings.Resolutions.Keys.ToList().IndexOf(WebcamSettings.SelectedResolution);
            SetResolution_Selected(i);
            var j = WebcamSettings.SupportedFPSs[WebcamSettings.SelectedResolution].IndexOf(WebcamSettings.SelectedFPSs[WebcamSettings.SelectedResolution]);
            SetFPS_Selected(j);
        }

        public void SetProfile()
        {
            if (WebcamSettings.CustomProfiles.ContainsKey(CurrentProfileName))
                CurrentProfile = WebcamSettings.CustomProfiles[CurrentProfileName];
            else
                CurrentProfile = WebcamSettings.PresetProfiles[CurrentProfileName];

            if (CurrentDeviceInfo!.IsPropertyAutoFramingSensitivitySupported || CurrentDeviceInfo.IsPropertyAutoFramingSizeSupported || CurrentDeviceInfo.IsPropertyAutoFramingTransitionSupported)
            {
                DdpmCommonHelper.DeviceManagerSA!.SetIsAutoFramingOn(CurrentDeviceInfo!.ID.ToString(), CurrentProfile.IsAutoFramingOn);
                OnPropertyChanged(nameof(IsAutoFramingOnText));
                if (CurrentDeviceInfo.IsPropertyAutoFramingTransitionSupported)
                {
                    DdpmCommonHelper.DeviceManagerSA!.SetIsAutoFramingTransitionOn(CurrentDeviceInfo!.ID.ToString(), CurrentProfile.IsAutoFramingTransitionOn);
                    OnPropertyChanged(nameof(IsAutoFramingTransitionOnText));
                }
            }

            DdpmCommonHelper.DeviceManagerSA!.SetZoom(CurrentDeviceInfo!.ID.ToString(), CurrentProfile.Zoom);
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

        public MediaCapture? MediaCapture;
        public MediaFrameReader? MediaFrameReader;

        public string CurrentProfileName
        {
            get => WebcamSettings.SelectedProfileName;
            set
            {
                WebcamSettings.SelectedProfileName = value;
                WebcamSettings.ExportWebcamSettings(WebcamSettings, Model);
            }
        }

        private bool _isRecording = false;
        public bool IsRecording
        {
            get => _isRecording;
            set
            {
                _isRecording = value;
                OnPropertyChanged(nameof(IsNotRecording));
            }
        }
        public bool IsNotRecording { get => !IsRecording; }

        private bool _isChecked_Autofocus;

        public bool IsChecked_Autofocus
        {
            get { return _isChecked_Autofocus; }
            set
            {
                _isChecked_Autofocus = value;
                OnPropertyChanged();
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
                OnPropertyChanged();
            }
        }

        private string awbStatus_String = "";

        public string AWBStatus_String
        {
            get { return awbStatus_String; }
            set
            {
                awbStatus_String = value;
                OnPropertyChanged();
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

        public string VideoCaptureFolder
        {
            get => WebcamSettings.VideoCaptureFolder;
            set
            {
                WebcamSettings.VideoCaptureFolder = value;
                WebcamSettings.ExportWebcamSettings(WebcamSettings, Model);
            }
        }
        public bool WebcamCountdown
        {
            get => WebcamSettings.WebcamCountdown;
            set
            {
                WebcamSettings.WebcamCountdown = value;
                WebcamSettings.ExportWebcamSettings(WebcamSettings, Model);
            }
        }
        public bool WebcamGrid
        {
            get => WebcamSettings.WebcamGrid;
            set
            {
                WebcamSettings.WebcamGrid = value;
                WebcamSettings.ExportWebcamSettings(WebcamSettings, Model);
                OnPropertyChanged();
                WebcamSettingChanged?.Invoke(this, EventArgs.Empty);
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
                DdpmCommonHelper.DeviceManagerSA!.SetIsMicEnumerationOn(value, CurrentDeviceInfo!.ID);
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsMicEnumerationOnText));
            }
        }

        public string IsAutoFramingOnText
        {
            get => CurrentProfile.IsAutoFramingOn ? Strings.On : Strings.Off;
        }
        public bool IsAutoFramingOn
        {
            get => CurrentProfile.IsAutoFramingOn;
            set
            {
                DdpmCommonHelper.DeviceManagerSA!.SetIsAutoFramingOn(CurrentDeviceInfo!.ID.ToString(), value);
                SetProfileProperty(nameof(IsAutoFramingOn), value);
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsAutoFramingOnText));
            }
        }

        public string IsAutoFramingTransitionOnText
        {
            get => CurrentProfile.IsAutoFramingTransitionOn ? Strings.On : Strings.Off;
        }
        public bool IsAutoFramingTransitionOn
        {
            get => CurrentProfile.IsAutoFramingTransitionOn;
            set
            {
                DdpmCommonHelper.DeviceManagerSA!.SetIsAutoFramingTransitionOn(CurrentDeviceInfo!.ID.ToString(), value);
                SetProfileProperty(nameof(IsAutoFramingTransitionOn), value);
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsAutoFramingTransitionOnText));
            }
        }

        public int AutoFramingSensitivity
        {
            get => CurrentProfile.AutoFramingSensitivity;
            set
            {
                //DdpmCommonHelper.DeviceManagerSA!.SetIsAutoFramingTransitionOn(CurrentDeviceInfo!.ID.ToString(), value);
                SetProfileProperty(nameof(AutoFramingSensitivity), value);
                OnPropertyChanged();
                OnPropertyChanged(nameof(AutoFramingSensitivity));
            }
        }

        private bool isMicEnumerationOnEnabled = true;
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
                OnPropertyChanged(nameof(FunctionsVisibility));
            }
        }

        public Visibility AutoFramingVisibility
        {
            get => CurrentDeviceInfo!.IsPropertyAutoFramingSensitivitySupported || CurrentDeviceInfo.IsPropertyAutoFramingSizeSupported || CurrentDeviceInfo.IsPropertyAutoFramingTransitionSupported ? Visibility.Visible : Visibility.Collapsed;
        }
        public Visibility AutoFramingSensitivityVisibility
        {
            get => CurrentDeviceInfo!.IsPropertyAutoFramingSensitivitySupported ? Visibility.Visible : Visibility.Collapsed;
        }
        public Visibility AutoFramingSizeVisibility
        {
            get => CurrentDeviceInfo!.IsPropertyAutoFramingSizeSupported ? Visibility.Visible : Visibility.Collapsed;
        }
        public Visibility AutoFramingTransitionVisibility
        {
            get => CurrentDeviceInfo!.IsPropertyAutoFramingTransitionSupported ? Visibility.Visible : Visibility.Collapsed;
        }

        public Visibility FunctionsVisibility
        {
            get => AlertVisibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
            //set
            //{
            //    alertVisibility = value;
            //    OnPropertyChanged();
            //}
        }
        private WebcamAlert alertType;
        public WebcamAlert AlertType
        {
            get => alertType;
            set
            {
                alertType = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(AlertText));
            }
        }
        public string AlertText => AlertType switch
        {
            WebcamAlert.Alert1 => LangHelper.Instance["Camera.Alert.1"],
            WebcamAlert.Alert2 => LangHelper.Instance["Camera.Alert.2"],
            WebcamAlert.Alert3 => LangHelper.Instance["Camera.Alert.3"],
            WebcamAlert.Alert4 => LangHelper.Instance["Camera.Alert.4"],
            _ => ""
        };
        public ObservableCollection<ProfileItem> ProfileItems { get => _profileItems; }
        public override void OnGoBackClicked()
        {
            if (MediaCapture != null)
            {
                try
                {
                    _ = MediaCapture.StopRecordAsync();
                }
                catch { }
            }
            base.OnGoBackClicked();
        }
        public async Task CleanupMediaCapture()
        {
            if (MediaCapture != null)
            {
                try
                {
                    await MediaFrameReader?.StopAsync();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error stopping MediaFrameReader: {ex.Message}");
                }
                MediaFrameReader?.Dispose();
                MediaCapture.Dispose();
                MediaCapture = null;
            }
        }

        private void SetProfileProperty(string propertyName, object value)
        {
            var type = CurrentProfile.GetType();
            var propertyInfo = type.GetProperty(propertyName);
            object convertedValue = Convert.ChangeType(value, propertyInfo!.PropertyType);
            propertyInfo.SetValue(CurrentProfile, convertedValue);
            WebcamSettings.ExportWebcamSettings(WebcamSettings, Model);
        }
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
    public enum WebcamAlert
    {
        Alert1, Alert2, Alert3, Alert4
    }
}