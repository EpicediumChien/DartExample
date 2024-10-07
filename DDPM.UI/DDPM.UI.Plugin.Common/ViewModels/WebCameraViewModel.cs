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
        public bool[] FOV_IsSelected { get; set; } = new bool[3];

        #endregion Variables

        public WebcamProfile CurrentProfile = new();
        public List<string> FPSs = new();
        public List<WebcamOperation> WCOperations = new();
        private int OPIndex = -1;
        const int MaxOPs = 30;


        // 20240926 jim add
        private bool showLockMask = false;

        public bool ShowLockMask
        {
            get { return showLockMask; }
            set
            {
                showLockMask = value;
                LockMaskVisible = showLockMask ? Visibility.Visible : Visibility.Collapsed;
                OnPropertyChanged();
            }
        }

        private Visibility lockMaskVisible = Visibility.Collapsed;

        public Visibility LockMaskVisible
        {
            get { return lockMaskVisible; }
            set
            {
                lockMaskVisible = value;
                OnPropertyChanged();
            }
        }

        private bool _isTabStoppable;

        public bool IsTabStoppable
        {
            get { return _isTabStoppable; }
            set
            {
                _isTabStoppable = value;
                OnPropertyChanged();
            }
        }

        private string _TabNavigation = "Cycle";

        public string TabNavigation
        {
            get { return _TabNavigation; }
            set
            {
                _TabNavigation = value;
                OnPropertyChanged();
            }
        }

        // webcam presence sensing

        private bool _isChecked_ProximitySensor = false;
        public bool IsChecked_ProximitySensor
        {
            get { return _isChecked_ProximitySensor; }
            set
            {
                _isChecked_ProximitySensor = value;
                DdpmCommonHelper.DeviceManagerSA!.SetIsProximitySensorEnable(CurrentDeviceInfo!.ID.ToString(), _isChecked_ProximitySensor);               
                OnPropertyChanged("IsChecked_ProximitySensor");            
            }
        }

        private bool _isChecked_WakeOnApproach = false;
        public bool IsChecked_WakeOnApproach
        {
            get { return _isChecked_WakeOnApproach; }
            set
            {
                _isChecked_WakeOnApproach = value;
                DdpmCommonHelper.DeviceManagerSA!.SetIsWakeonApproachEnable(CurrentDeviceInfo!.ID.ToString(), _isChecked_WakeOnApproach);
                OnPropertyChanged("IsChecked_WakeOnApproach");
            }
        }

        private bool _isChecked_WalkAwayLock = false;
        public bool IsChecked_WalkAwayLock
        {
            get { return _isChecked_WalkAwayLock; }
            set
            {
                _isChecked_WalkAwayLock = value;
                DdpmCommonHelper.DeviceManagerSA!.SetIsWakeonApproachEnable(CurrentDeviceInfo!.ID.ToString(), _isChecked_WalkAwayLock);
                //DdpmCommonHelper.DeviceManagerSA!.SetWALTime(CurrentDeviceInfo!.ID.ToString(), 30);
                OnPropertyChanged("IsChecked_WalkAwayLock");
            }
        }



        public event EventHandler<EventArgs> WebcamSettingChanged;
        public new event PropertyChangedEventHandler? PropertyChanged;

        public WebCameraViewModel(IConsole console, ILog log) : base(console, log, DdpmCommonHelper.DeviceManagerSA!)
        {
            Requires.NotNull(console, nameof(console));
            Requires.NotNull(log, nameof(log));

            _log = log;
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
        public void SetFOV_Selected(int index)
        {
            for (int j = 0; j < FOV_IsSelected.Length; j++)
            {
                FOV_IsSelected[j] = false;
            }
            FOV_IsSelected[index] = true;
            WebcamSettings.ExportWebcamSettings(WebcamSettings, Model);
            OnPropertyChanged(nameof(FOV_IsSelected));
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
                    profile.Focus = CurrentDeviceInfo.FocusMin;
                    WebcamSettings.PresetProfiles.Add(profile.Name, profile);
                }
                WebcamSettings.SelectedProfileName = WebcamSettings.PresetProfiles.Values.ToList()[0].Name;

                WebcamSettings.ExportWebcamSettings(WebcamSettings, Model);
            }

            for (int k = 0; k < CurrentDeviceInfo!.FOVValues.Length; k++)
            {
                _fOVs[k] = int.Parse(CurrentDeviceInfo!.FOVValues[k]);
            }

            SetProfile();

            _resolutions = WebcamSettings.Resolutions.Keys.ToList();
            var i = WebcamSettings.Resolutions.Keys.ToList().IndexOf(WebcamSettings.SelectedResolution);
            SetResolution_Selected(i);
            var j = WebcamSettings.SupportedFPSs[WebcamSettings.SelectedResolution].IndexOf(WebcamSettings.SelectedFPSs[WebcamSettings.SelectedResolution]);
            SetFPS_Selected(j);

            WCOperations.Clear();
            OPIndex = -1;
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
                OnPropertyChanged(nameof(IsAutoFramingOn));
                OnPropertyChanged(nameof(IsAutoFramingOnText));
                if (CurrentDeviceInfo.IsPropertyAutoFramingTransitionSupported)
                {
                    DdpmCommonHelper.DeviceManagerSA!.SetIsAutoFramingTransitionOn(CurrentDeviceInfo!.ID.ToString(), CurrentProfile.IsAutoFramingTransitionOn);
                    OnPropertyChanged(nameof(IsAutoFramingTransitionOn));
                    OnPropertyChanged(nameof(IsAutoFramingTransitionOnText));
                }
                if (CurrentDeviceInfo.IsPropertyAutoFramingSensitivitySupported)
                {
                    DdpmCommonHelper.DeviceManagerSA!.SetAutoFramingSensitivity(CurrentDeviceInfo!.ID.ToString(), CurrentProfile.AutoFramingSensitivity);
                    OnPropertyChanged(nameof(AutoFramingSensitivity));
                }
                if (CurrentDeviceInfo.IsPropertyAutoFramingSizeSupported)
                {
                    DdpmCommonHelper.DeviceManagerSA!.SetAutoFramingFrameSize(CurrentDeviceInfo!.ID.ToString(), CurrentProfile.AutoFramingFrameSize);
                    OnPropertyChanged(nameof(AutoFramingFrameSize));
                    OnPropertyChanged(nameof(IsAutoFramingTransitionOnText));
                }
            }

            if (CurrentDeviceInfo.IsPropertyFOVSupported)
            {
                if (_fOVs[0] == CurrentProfile.FieldOfView)
                    SetFOV_Selected(0);
                else if (_fOVs[1] == CurrentProfile.FieldOfView)
                    SetFOV_Selected(1);
                else
                    SetFOV_Selected(2);

            }

            if (CurrentDeviceInfo.IsPropertyZoomSupported)
            {
                DdpmCommonHelper.DeviceManagerSA!.SetZoom(CurrentDeviceInfo!.ID.ToString(), CurrentProfile.Zoom);
                Zoom = CurrentProfile.Zoom;
                //OnPropertyChanged(nameof(Zoom));
            }

            if (CurrentDeviceInfo.IsPropertyFocusSupported)
            {
                DdpmCommonHelper.DeviceManagerSA!.SetIsFocusOn(CurrentDeviceInfo!.ID.ToString(), CurrentProfile.IsFocusOn);
                DdpmCommonHelper.DeviceManagerSA!.SetFocus(CurrentDeviceInfo!.ID.ToString(), CurrentProfile.Focus);
                Focus = CurrentProfile.Focus;
                OnPropertyChanged(nameof(IsFocusOn));
                OnPropertyChanged(nameof(IsFocusOnText));
                //OnPropertyChanged(nameof(Focus));
            }

            if (CurrentDeviceInfo.IsPropertyPrioritySupported)
            {
                DdpmCommonHelper.DeviceManagerSA!.SetPriority(CurrentDeviceInfo!.ID.ToString(), CurrentProfile.Priority);
                OnPropertyChanged(nameof(Priority));
            }

            if (CurrentDeviceInfo.IsPropertyHDRSupported)
            {
                DdpmCommonHelper.DeviceManagerSA!.SetIsHDROn(CurrentDeviceInfo!.ID.ToString(), CurrentProfile.IsHDROn);
                OnPropertyChanged(nameof(IsHDROn));
            }

            if (CurrentDeviceInfo.IsPropertyWhiteBalanceSupported)
            {
                DdpmCommonHelper.DeviceManagerSA!.SetIsAutoWhiteBalanceOn(CurrentDeviceInfo!.ID.ToString(), CurrentProfile.IsAutoWhiteBalanceOn);
                DdpmCommonHelper.DeviceManagerSA!.SetAutoWhiteBalance(CurrentDeviceInfo!.ID.ToString(), CurrentProfile.AutoWhiteBalance);
                AutoWhiteBalance = CurrentProfile.AutoWhiteBalance;
                OnPropertyChanged(nameof(IsAutoWhiteBalanceOn));
                OnPropertyChanged(nameof(IsAutoWhiteBalanceOnText));
                //OnPropertyChanged(nameof(AutoWhiteBalance));
            }


            if (CurrentDeviceInfo.IsPropertyBrightnessSupported)
            {
                DdpmCommonHelper.DeviceManagerSA!.SetBrightness(CurrentDeviceInfo!.ID.ToString(), CurrentProfile.Brightness);
                Brightness = CurrentProfile.Brightness;
                //OnPropertyChanged(nameof(Brightness));
            }

            if (CurrentDeviceInfo.IsPropertySharpnessSupported)
            {
                DdpmCommonHelper.DeviceManagerSA!.SetSharpness(CurrentDeviceInfo!.ID.ToString(), CurrentProfile.Sharpness);
                Sharpness = CurrentProfile.Sharpness;
                //OnPropertyChanged(nameof(Sharpness));
            }

            if (CurrentDeviceInfo.IsPropertyContrastSupported)
            {
                DdpmCommonHelper.DeviceManagerSA!.SetContrast(CurrentDeviceInfo!.ID.ToString(), CurrentProfile.Contrast);
                Contrast = CurrentProfile.Contrast;
                //OnPropertyChanged(nameof(Contrast));
            }

            if (CurrentDeviceInfo.IsPropertySaturationSupported)
            {
                DdpmCommonHelper.DeviceManagerSA!.SetSaturation(CurrentDeviceInfo!.ID.ToString(), CurrentProfile.Saturation);
                Saturation = CurrentProfile.Saturation;
                //OnPropertyChanged(nameof(Saturation));
            }

            if (CurrentDeviceInfo.IsPropertyAntiFlickerSupported)
            {
                DdpmCommonHelper.DeviceManagerSA!.SetAntiFlicker(CurrentDeviceInfo!.ID.ToString(), CurrentProfile.AntiFlicker);
                OnPropertyChanged(nameof(AntiFlicker));
            }
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

        private int[] _fOVs = [0, 0, 0];
        public int[] FOVs
        {
            get => _fOVs;
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
                SetProfileProperty(nameof(IsAutoFramingOn), value, OperationModule.CameraControl);
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsAutoFramingOnText));
                OnPropertyChanged(nameof(PanArrowVisibility));
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
                SetProfileProperty(nameof(IsAutoFramingTransitionOn), value, OperationModule.CameraControl);
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsAutoFramingTransitionOnText));
            }
        }

        public int AutoFramingSensitivity
        {
            get => CurrentProfile.AutoFramingSensitivity;
            set
            {
                DdpmCommonHelper.DeviceManagerSA!.SetAutoFramingSensitivity(CurrentDeviceInfo!.ID.ToString(), value);
                SetProfileProperty(nameof(AutoFramingSensitivity), value, OperationModule.CameraControl);
                OnPropertyChanged();
            }
        }

        public int AutoFramingFrameSize
        {
            get => CurrentProfile.AutoFramingFrameSize;
            set
            {
                DdpmCommonHelper.DeviceManagerSA!.SetAutoFramingFrameSize(CurrentDeviceInfo!.ID.ToString(), value);
                SetProfileProperty(nameof(AutoFramingFrameSize), value, OperationModule.CameraControl);
                OnPropertyChanged();
            }
        }

        public int FieldOfView
        {
            get => CurrentProfile.FieldOfView;
            set
            {
                DdpmCommonHelper.DeviceManagerSA!.SetFieldOfView(CurrentDeviceInfo!.ID.ToString(), value);
                SetProfileProperty(nameof(FieldOfView), value, OperationModule.CameraControl);
                OnPropertyChanged();
            }
        }

        public Visibility ZoomVisibility
        {
            get => CurrentDeviceInfo!.IsPropertyZoomSupported ? Visibility.Visible : Visibility.Collapsed;
        }

        private int _zoom = 0;
        public int Zoom
        {
            get => _zoom;
            set
            {
                _zoom = value;
                if (value != CurrentProfile.Zoom)
                {
                    if (!IsSliderDragging)
                    {
                        SetZoom();
                    }
                }
                OnPropertyChanged();
            }
        }
        public void SetZoom()
        {
            DdpmCommonHelper.DeviceManagerSA!.SetZoom(CurrentDeviceInfo!.ID.ToString(), _zoom);
            SetProfileProperty(nameof(Zoom), _zoom, OperationModule.CameraControl);
            OnPropertyChanged(nameof(PanArrowVisibility));
        }

        public Visibility AutofocusVisibility
        {
            get => CurrentDeviceInfo!.IsPropertyFocusSupported ? Visibility.Visible : Visibility.Collapsed;
        }
        public bool IsFocusOn
        {
            get => CurrentProfile.IsFocusOn;
            set
            {
                DdpmCommonHelper.DeviceManagerSA!.SetIsFocusOn(CurrentDeviceInfo!.ID.ToString(), value);
                SetProfileProperty(nameof(IsFocusOn), value, OperationModule.CameraControl);
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsFocusOnText));
            }
        }
        public string IsFocusOnText
        {
            get => CurrentProfile.IsFocusOn ? Strings.On : Strings.Off;
        }

        private int _focus = 0;
        public int Focus
        {
            get => _focus;
            set
            {
                _focus = value;
                if (value != CurrentProfile.Focus)
                {
                    if (!IsSliderDragging)
                    {
                        SetFocus();
                    }
                }
                OnPropertyChanged();
            }
        }
        public void SetFocus()
        {
            DdpmCommonHelper.DeviceManagerSA!.SetFocus(CurrentDeviceInfo!.ID.ToString(), _focus);
            SetProfileProperty(nameof(Focus), _focus, OperationModule.CameraControl);
        }

        public Visibility PriorityVisibility
        {
            get => CurrentDeviceInfo!.IsPropertyPrioritySupported ? Visibility.Visible : Visibility.Collapsed;
        }
        public int Priority
        {
            get => CurrentProfile.Priority;
            set
            {
                DdpmCommonHelper.DeviceManagerSA!.SetPriority(CurrentDeviceInfo!.ID.ToString(), value);
                SetProfileProperty(nameof(Priority), value, OperationModule.CameraControl);
                OnPropertyChanged();
            }
        }
        public Visibility HDRVisibility
        {
            get => CurrentDeviceInfo!.IsPropertyHDRSupported ? Visibility.Visible : Visibility.Collapsed;
        }
        public bool IsHDROn
        {
            get => CurrentProfile.IsHDROn;
            set
            {
                DdpmCommonHelper.DeviceManagerSA!.SetIsHDROn(CurrentDeviceInfo!.ID.ToString(), value);
                SetProfileProperty(nameof(IsHDROn), value, OperationModule.ColorAndImage);
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsHDROnText));
            }
        }
        public string IsHDROnText
        {
            get => CurrentProfile.IsHDROn ? Strings.On : Strings.Off;
        }

        public Visibility AutoWhiteBalanceVisibility
        {
            get => CurrentDeviceInfo!.IsPropertyWhiteBalanceSupported ? Visibility.Visible : Visibility.Collapsed;
        }
        public bool IsAutoWhiteBalanceOn
        {
            get => CurrentProfile.IsAutoWhiteBalanceOn;
            set
            {
                DdpmCommonHelper.DeviceManagerSA!.SetIsAutoWhiteBalanceOn(CurrentDeviceInfo!.ID.ToString(), value);
                SetProfileProperty(nameof(IsAutoWhiteBalanceOn), value, OperationModule.ColorAndImage);
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsAutoWhiteBalanceOnText));
            }
        }
        public string IsAutoWhiteBalanceOnText
        {
            get => CurrentProfile.IsAutoWhiteBalanceOn ? Strings.On : Strings.Off;
        }

        private int _autoWhiteBalance = 0;
        public int AutoWhiteBalance
        {
            get => CurrentProfile.AutoWhiteBalance;
            set
            {
                _autoWhiteBalance = value;
                if (value != CurrentProfile.AutoWhiteBalance)
                {
                    if (!IsSliderDragging)
                    {
                        SetAutoWhiteBalance();
                    }
                }
                OnPropertyChanged();
            }
        }
        public void SetAutoWhiteBalance()
        {
            DdpmCommonHelper.DeviceManagerSA!.SetAutoWhiteBalance(CurrentDeviceInfo!.ID.ToString(), _autoWhiteBalance);
            SetProfileProperty(nameof(AutoWhiteBalance), _autoWhiteBalance, OperationModule.ColorAndImage);
        }

        private int _brightness = 0;
        public int Brightness
        {
            get => CurrentProfile.Brightness;
            set
            {
                _brightness = value;
                if (value != CurrentProfile.Brightness)
                {
                    if (!IsSliderDragging)
                    {
                        SetBrightness();
                    }
                }
                OnPropertyChanged();
            }
        }
        public void SetBrightness()
        {
            DdpmCommonHelper.DeviceManagerSA!.SetBrightness(CurrentDeviceInfo!.ID.ToString(), _brightness);
            SetProfileProperty(nameof(Brightness), _brightness, OperationModule.ColorAndImage);
        }

        private int _sharpness = 0;
        public int Sharpness
        {
            get => _sharpness;
            set
            {
                _sharpness = value;
                if (value != CurrentProfile.Sharpness)
                {
                    if (!IsSliderDragging)
                    {
                        SetSharpness();
                    }
                }
                OnPropertyChanged();
            }
        }
        public void SetSharpness()
        {
            DdpmCommonHelper.DeviceManagerSA!.SetSharpness(CurrentDeviceInfo!.ID.ToString(), _sharpness);
            SetProfileProperty(nameof(Sharpness), _sharpness, OperationModule.ColorAndImage);
        }

        private int _contrast = 0;
        public int Contrast
        {
            get => CurrentProfile.Contrast;
            set
            {
                _contrast = value;
                if (value != CurrentProfile.Contrast)
                {
                    if (!IsSliderDragging)
                    {
                        SetContrast();
                    }
                }
                OnPropertyChanged();
            }
        }
        public void SetContrast()
        {
            DdpmCommonHelper.DeviceManagerSA!.SetContrast(CurrentDeviceInfo!.ID.ToString(), _contrast);
            SetProfileProperty(nameof(Contrast), _contrast, OperationModule.ColorAndImage);
        }

        private int _saturation = 0;
        public int Saturation
        {
            get => CurrentProfile.Saturation;
            set
            {
                _saturation = value;
                if (value != CurrentProfile.Saturation)
                {
                    if (!IsSliderDragging)
                    {
                        SetSaturation();
                    }
                }
                OnPropertyChanged();
            }
        }
        public void SetSaturation()
        {
            DdpmCommonHelper.DeviceManagerSA!.SetSaturation(CurrentDeviceInfo!.ID.ToString(), _saturation);
            SetProfileProperty(nameof(Saturation), _saturation, OperationModule.ColorAndImage);
        }

        public int AntiFlicker
        {
            get => CurrentProfile.AntiFlicker;
            set
            {
                if (value != CurrentProfile.AntiFlicker)
                {
                    CurrentProfile.AntiFlicker = value;
                    if (!IsSliderDragging)
                    {
                        SetAntiFlicker();
                    }
                }
                OnPropertyChanged();
            }
        }
        public void SetAntiFlicker()
        {
            DdpmCommonHelper.DeviceManagerSA!.SetAntiFlicker(CurrentDeviceInfo!.ID.ToString(), AntiFlicker);
            SetProfileProperty(nameof(AntiFlicker), AntiFlicker, OperationModule.ColorAndImage);
        }
        public void SetTilt(int value)
        {
            DdpmCommonHelper.DeviceManagerSA!.SetTilt(CurrentDeviceInfo!.ID.ToString(), value);
            SetProfileProperty("Tilt", value, OperationModule.Other, false);
        }
        public void SetPan(int value)
        {
            DdpmCommonHelper.DeviceManagerSA!.SetPan(CurrentDeviceInfo!.ID.ToString(), value);
            SetProfileProperty("Pan", value, OperationModule.Other, false);
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

        public Visibility PanArrowVisibility
        {
            get => CurrentProfile.Zoom != CurrentDeviceInfo!.ZoomMin && !CurrentProfile.IsAutoFramingOn ? Visibility.Visible : Visibility.Collapsed;
        }

        public Visibility UndoVisibility
        {
            get => OPIndex == -1 ? Visibility.Collapsed : Visibility.Visible;
        }
        public Visibility Undo2Visibility
        {
            get => WCOperations.Count > 0 && UndoVisibility == Visibility.Collapsed ? Visibility.Visible : Visibility.Collapsed;
        }
        public Visibility RedoVisibility
        {
            get => WCOperations.Count - OPIndex > 1 ? Visibility.Visible : Visibility.Collapsed;
        }
        public Visibility Redo2Visibility
        {
            get => WCOperations.Count > 0 && RedoVisibility == Visibility.Collapsed ? Visibility.Visible : Visibility.Collapsed;
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

        public Visibility FOVVisibility
        {
            get => CurrentDeviceInfo!.IsPropertyFOVSupported ? Visibility.Visible : Visibility.Collapsed;
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

        private void SetProfileProperty(string propertyName, object value, OperationModule opModule, bool undoable = true)
        {
            var type = CurrentProfile.GetType();
            var propertyInfo = type.GetProperty(propertyName);
            object convertedValue = Convert.ChangeType(value, propertyInfo!.PropertyType);
            if (undoable)
            {
                while (WCOperations.Count > OPIndex + 1)
                {
                    WCOperations.RemoveAt(OPIndex + 1);
                }
                WCOperations.Add(new WebcamOperation
                {
                    OPModule = opModule,
                    Property = propertyName,
                    OldValue = propertyInfo.GetValue(CurrentProfile)!,
                    NewValue = value,
                });
                OPIndex += 1;
                if (WCOperations.Count > MaxOPs)
                {
                    WCOperations.RemoveAt(0);
                    OPIndex -= 1;
                }
            }
            propertyInfo.SetValue(CurrentProfile, convertedValue);
            WebcamSettings.ExportWebcamSettings(WebcamSettings, Model);
            OnPropertyChanged(nameof(UndoVisibility));
            OnPropertyChanged(nameof(Undo2Visibility));
            OnPropertyChanged(nameof(RedoVisibility));
            OnPropertyChanged(nameof(Redo2Visibility));
        }

        public void Undo()
        {
            var op = WCOperations[OPIndex];
            VbarSelectedIndex = (int)op.OPModule;
            SelectVBar();
            OPIndex -= 1;
            var type = CurrentProfile.GetType();
            var propertyInfo = type.GetProperty(op.Property);
            object convertedValue = Convert.ChangeType(op.OldValue, propertyInfo!.PropertyType);
            propertyInfo.SetValue(CurrentProfile, convertedValue);
            UPdateProperty(op.Property, convertedValue);
        }
        public void Redo()
        {
            var op = WCOperations[OPIndex + 1];
            VbarSelectedIndex = (int)op.OPModule;
            SelectVBar();
            OPIndex += 1;
            var type = CurrentProfile.GetType();
            var propertyInfo = type.GetProperty(op.Property);
            object convertedValue = Convert.ChangeType(op.NewValue, propertyInfo!.PropertyType);
            propertyInfo.SetValue(CurrentProfile, convertedValue);
            UPdateProperty(op.Property, convertedValue);
        }

        private void UPdateProperty(string property, object value)
        {
            WebcamSettings.ExportWebcamSettings(WebcamSettings, Model);
            switch (property)
            {
                case "IsFocusOn":
                    DdpmCommonHelper.DeviceManagerSA!.SetIsFocusOn(CurrentDeviceInfo!.ID.ToString(), (bool)value);
                    OnPropertyChanged(nameof(IsFocusOnText));
                    break;
                case "Focus":
                    DdpmCommonHelper.DeviceManagerSA!.SetFocus(CurrentDeviceInfo!.ID.ToString(), (int)value);
                    Focus = (int)value;
                    break;
                case "Priority":
                    DdpmCommonHelper.DeviceManagerSA!.SetPriority(CurrentDeviceInfo!.ID.ToString(), (int)value);
                    break;
                case "Zoom":
                    DdpmCommonHelper.DeviceManagerSA!.SetZoom(CurrentDeviceInfo!.ID.ToString(), (int)value);
                    Zoom = (int)value;
                    break;
                case "Brightness":
                    DdpmCommonHelper.DeviceManagerSA!.SetBrightness(CurrentDeviceInfo!.ID.ToString(), (int)value);
                    Brightness = (int)value;
                    break;
                case "Contrast":
                    DdpmCommonHelper.DeviceManagerSA!.SetContrast(CurrentDeviceInfo!.ID.ToString(), (int)value);
                    Contrast = (int)value;
                    break;
                case "AntiFlicker":
                    DdpmCommonHelper.DeviceManagerSA!.SetAntiFlicker(CurrentDeviceInfo!.ID.ToString(), (int)value);
                    break;
                case "Saturation":
                    DdpmCommonHelper.DeviceManagerSA!.SetSaturation(CurrentDeviceInfo!.ID.ToString(), (int)value);
                    Saturation = (int)value;
                    break;
                case "Sharpness":
                    DdpmCommonHelper.DeviceManagerSA!.SetSharpness(CurrentDeviceInfo!.ID.ToString(), (int)value);
                    Sharpness = (int)value;
                    break;
                case "IsAutoWhiteBalanceOn":
                    DdpmCommonHelper.DeviceManagerSA!.SetIsAutoWhiteBalanceOn(CurrentDeviceInfo!.ID.ToString(), (bool)value);
                    OnPropertyChanged(nameof(IsAutoWhiteBalanceOnText));
                    break;
                case "AutoWhiteBalance":
                    DdpmCommonHelper.DeviceManagerSA!.SetAutoWhiteBalance(CurrentDeviceInfo!.ID.ToString(), (int)value);
                    AutoWhiteBalance = (int)value;
                    break;
                case "IsAutoFramingOn":
                    DdpmCommonHelper.DeviceManagerSA!.SetIsAutoFramingOn(CurrentDeviceInfo!.ID.ToString(), (bool)value);
                    OnPropertyChanged(nameof(IsAutoFramingOnText));
                    break;
                case "IsAutoFramingTransitionOn":
                    DdpmCommonHelper.DeviceManagerSA!.SetIsAutoFramingTransitionOn(CurrentDeviceInfo!.ID.ToString(), (bool)value);
                    OnPropertyChanged(nameof(IsAutoFramingTransitionOnText));
                    break;
                case "AutoFramingSensitivity":
                    DdpmCommonHelper.DeviceManagerSA!.SetAutoFramingSensitivity(CurrentDeviceInfo!.ID.ToString(), (int)value);
                    break;
                case "AutoFramingFrameSize":
                    DdpmCommonHelper.DeviceManagerSA!.SetAutoFramingFrameSize(CurrentDeviceInfo!.ID.ToString(), (int)value);
                    break;
                case "FieldOfView":
                    DdpmCommonHelper.DeviceManagerSA!.SetFieldOfView(CurrentDeviceInfo!.ID.ToString(), (int)value);
                    if (_fOVs[0] == CurrentProfile.FieldOfView)
                        SetFOV_Selected(0);
                    else if (_fOVs[1] == CurrentProfile.FieldOfView)
                        SetFOV_Selected(1);
                    else
                        SetFOV_Selected(2);
                    break;
            }
            OnPropertyChanged(property);

            OnPropertyChanged(nameof(UndoVisibility));
            OnPropertyChanged(nameof(Undo2Visibility));
            OnPropertyChanged(nameof(RedoVisibility));
            OnPropertyChanged(nameof(Redo2Visibility));
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