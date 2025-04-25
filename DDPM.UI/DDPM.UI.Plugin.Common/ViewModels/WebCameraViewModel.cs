using DDPM.SA.Common;
using DDPM.SA.Common.Settings;
using DDPM.UI.Common;
using DDPM.UI.Plugin.Common;
using DDPM.UI.Resources.Helper;
using Dell.Client.Framework.Common;
using Dell.Client.Framework.UX.WPF;
using Microsoft;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Media3D;
using System.Windows.Threading;
using Windows.Media.Capture;
using Windows.Media.Capture.Frames;
using Windows.Media.MediaProperties;
using Windows.Storage;
using static System.Runtime.InteropServices.JavaScript.JSType;
using JsonSerializer = System.Text.Json.JsonSerializer;
using WebcamProfile = DDPM.SA.Common.Settings.WebcamProfile;

namespace DDPM.UI.Plugin.ViewModels
{
    public class UI_Delay_WalkAwayLock
    {
        public int Delay { get; set; }

        public string DisplayText
        {
            get
            {
                return $"{Delay} {LangHelper.Instance["WebCameraPresenceDetection.12"]}";
            }
        }
    }

    public class UI_SnoozeLength
    {
        public int SnoozeLength { get; set; }
        public int SnoozeLength_sec { get; set; }

        public string DisplayText
        {
            get
            {
                return $"{SnoozeLength} {LangHelper.Instance["WebCameraPresenceDetection.13"]}";
            }
        }
    }

    public class WebCameraViewModel : PeripheralViewModel, INotifyPropertyChanged
    {
        #region Variables
        public readonly ILog _log;
        private List<ProfileItem> _profileItems = new();
        private List<string> _resolutions = new();

        // Query all properties [resolution and frame rate] of the webcam device
        public IEnumerable<StreamResolution> allProperties;

        public bool[] Resolution_IsSelected { get; set; } = new bool[4];
        public bool[] FPS_IsSelected { get; set; } = new bool[3];
        public bool[] FOV_IsSelected { get; set; } = new bool[3];
        public Dictionary<string, string> ProfileCaptions = new();


        public List<UI_Delay_WalkAwayLock> Delay_ItemsCollection { get; set; }

        public List<UI_SnoozeLength> SnoozeLength_ItemsCollection { get; set; }

        #endregion Variables

        public WebcamSettings WebcamSettings = new();
        public WebcamProfile CurrentProfile = new();
        public List<WebcamOperation> WCOperations = new();
        private int OPIndex = -1;
        const int MaxOPs = 30;
        public List<string> MicList = new() { "WB5023", "WB3023" };

        //Derek 2025/01/22 for PIMS-341594
        public bool isUIHasUpdateByQAM = false;

        // 20240926 jim add
        private bool showLockMask = false;

        public ManualResetEvent mre = new(false);

        public bool close_app = false;

        public List<Thread> thread_list = new();
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
        //Derek 2025/02/14 for PIMS 338464 Observe WAL occurs during Recording of DDPM (No one in Field of View)
        private bool bCurrentProximitySensorCheckStatus = false;
        private bool _isChecked_ProximitySensor = false;
        public bool IsChecked_ProximitySensor
        {
            get { return _isChecked_ProximitySensor; }
            set
            {
                //if (value != _isChecked_ProximitySensor) //Add by Derek 11/12
                _isChecked_ProximitySensor = value;
                DdpmCommonHelper.DeviceManagerSA?.SetIsProximitySensorEnable(CurrentDeviceInfo?.ID.ToString(), _isChecked_ProximitySensor);

                // jim add for PIMS-328195
                _isEnable_WalkAwayLock = _isChecked_WalkAwayLock && _isChecked_ProximitySensor;
                _isEnable_Snooze = _isChecked_WalkAwayLock && _isChecked_ProximitySensor; // jim modify for PIMS - 328195
                _isEnable_SnoozeLength = _isChecked_Snooze && _isChecked_ProximitySensor;

                OnPropertyChanged(nameof(IsChecked_ProximitySensor));
                OnPropertyChanged(nameof(ProximitySensorStatus_String));
                OnPropertyChanged(nameof(IsEnable_WalkAwayLock));
                OnPropertyChanged(nameof(IsEnable_Snooze));
                OnPropertyChanged(nameof(IsEnable_SnoozeLength));

                //Derek 11/12
                //PIMS - 319099
                //Find Presence Detection Setting is available, when SUT does not support HPD_MPS and
                //Internal Presence Sensor. DUT is with HPD_MPS FW
                ChangeUPDStatus();
            }
        }

        private bool _isWALTimerEnable = false;
        public bool IsWALTimerEnable
        {
            get { return _isWALTimerEnable; }

            set
            {
                if (value != _isWALTimerEnable)
                {
                    _isWALTimerEnable = value;

                    OnPropertyChanged("IsWALTimerEnable");
                }
            }
        }

        private bool _isSnoozeEnable = false;
        public bool IsSnoozeEnable
        {
            get { return _isSnoozeEnable; }

            set
            {
                if (value != _isSnoozeEnable)
                {
                    _isSnoozeEnable = value;

                    OnPropertyChanged("IsSnoozeEnable");
                }
            }
        }

        private void ChangeUPDStatus()
        {
            if (_isChecked_ProximitySensor)
            {
                IsWALTimerEnable = IsChecked_WalkAwayLock;
                IsSnoozeEnable = IsChecked_WalkAwayLock;
            }
            else
            {
                IsWALTimerEnable = false;
                IsChecked_Snooze = false;
                IsSnoozeEnable = false;
            }
        }

        public string ProximitySensorStatus_String
        {
            get
            {
                return IsChecked_ProximitySensor ? Strings.On : Strings.Off;
            }
        }

        //Derek 2025/02/14 for PIMS 338464 Observe WOA occurs during Recording of DDPM (No one in Field of View)
        private bool bCurrentWOACheckStatus = false;

        private bool _isChecked_WakeOnApproach = false;
        public bool IsChecked_WakeOnApproach
        {
            get { return _isChecked_WakeOnApproach; }
            set
            {
                _isChecked_WakeOnApproach = value;
                DdpmCommonHelper.DeviceManagerSA?.SetIsWakeonApproachEnable(CurrentDeviceInfo?.ID.ToString(), _isChecked_WakeOnApproach);
                //DdpmCommonHelper.DeviceManagerSA?.SetIsWakeonApproachEnable(value, CurrentDeviceInfo.ID);
                OnPropertyChanged("IsChecked_WakeOnApproach");
                OnPropertyChanged("WakeOnApproachStatus_String");
            }
        }

        public string WakeOnApproachStatus_String
        {
            get
            {
                return IsChecked_WakeOnApproach ? Strings.On : Strings.Off;
            }
        }

        //Derek 2025/02/14 for PIMS 338464 Observe WAL occurs during Recording of DDPM (No one in Field of View)
        private bool bCurrentWALCheckStatus = false;

        private bool _isChecked_WalkAwayLock = false;
        public bool IsChecked_WalkAwayLock
        {
            get { return _isChecked_WalkAwayLock; }
            set
            {
                _isChecked_WalkAwayLock = value;
                DdpmCommonHelper.DeviceManagerSA?.SetIsWalkAwayLockEnable(CurrentDeviceInfo?.ID.ToString(), _isChecked_WalkAwayLock);
                OnPropertyChanged(nameof(IsChecked_WalkAwayLock));
                OnPropertyChanged(nameof(WalkAwayLockStatus_String));

                IsWALTimerEnable = value;
                IsSnoozeEnable = value;

                // jim modify for PIMS - PIMS-344510
                if (_isChecked_WalkAwayLock == false)
                    IsChecked_Snooze = value;

                // jim add for PIMS-328195
                _isEnable_WalkAwayLock = _isChecked_WalkAwayLock && _isChecked_ProximitySensor;

                _isEnable_Snooze = _isChecked_WalkAwayLock && _isChecked_ProximitySensor; // jim modify for PIMS - 328195

                OnPropertyChanged(nameof(IsEnable_WalkAwayLock));
                OnPropertyChanged(nameof(IsEnable_Snooze)); // jim modify for PIMS - 328195
                OnPropertyChanged(nameof(IsChecked_Snooze)); // jim modify for PIMS - PIMS-344510
            }
        }

        private bool _isEnable_WalkAwayLock = false;
        public bool IsEnable_WalkAwayLock
        {
            get { return _isEnable_WalkAwayLock; }
            set
            {
                _isEnable_WalkAwayLock = value;
                OnPropertyChanged(nameof(IsEnable_WalkAwayLock));
            }
        }

        // jim 20250102 add for PIMS-328195 - [DDPM WIN 2.0][R17_ Webcam] Delay and Snooze still selectable when Proximity Sensor is off and WAL is on.
        private bool _isEnable_Snooze = false;
        public bool IsEnable_Snooze
        {
            get { return _isEnable_Snooze; }
            set
            {
                _isEnable_Snooze = value;
                OnPropertyChanged(nameof(IsEnable_Snooze));
            }
        }

        // jim add for PIMS-328195
        private bool _isEnable_SnoozeLength = false;
        public bool IsEnable_SnoozeLength
        {
            get { return _isEnable_SnoozeLength; }
            set
            {
                _isEnable_SnoozeLength = value;
                OnPropertyChanged(nameof(IsEnable_SnoozeLength));
            }
        }

        public string WalkAwayLockStatus_String
        {
            get
            {
                return IsChecked_WalkAwayLock ? Strings.On : Strings.Off;
            }
        }

        private bool _isChecked_Snooze = false;
        public bool IsChecked_Snooze
        {
            get { return _isChecked_Snooze; }
            set
            {

                try
                {

                    _isChecked_Snooze = value;

                    // jim add for PIMS-328195
                    _isEnable_SnoozeLength = _isChecked_Snooze && _isChecked_ProximitySensor;

                    if (_isChecked_Snooze &&
                        _SelectedSnoozeLength != null)
                    {
                        if (_SelectedSnoozeLength.SnoozeLength == 30)
                        {
                            //DdpmCommonHelper.DeviceManagerSA?.SetSnooze(CurrentDeviceInfo.ID.ToString(), -1); // jim 20241221 modify for 和DPeM 行為對齊 
                            DdpmCommonHelper.DeviceManagerSA?.SetSnooze(CurrentDeviceInfo?.ID.ToString(), 0);
                            //DdpmCommonHelper.DeviceManagerSA?.SetSnooze(0, CurrentDeviceInfo.ID);
                            //_SelectedSnoozeLength.SnoozeLength = 1800;
                            //DdpmCommonHelper.DeviceManagerSA?.SetSnoozeLength(CurrentDeviceInfo.ID.ToString(), _SelectedSnoozeLength.SnoozeLength_sec);
                        }
                        else if (_SelectedSnoozeLength.SnoozeLength == 60)
                        {
                            //DdpmCommonHelper.DeviceManagerSA?.SetSnooze(CurrentDeviceInfo.ID.ToString(), -1); // jim 20241221 modify for 和DPeM 行為對齊 
                            DdpmCommonHelper.DeviceManagerSA?.SetSnooze(CurrentDeviceInfo?.ID.ToString(), 1);
                            //DdpmCommonHelper.DeviceManagerSA?.SetSnooze(1, CurrentDeviceInfo.ID);
                            //_SelectedSnoozeLength.SnoozeLength = 3600;
                            //DdpmCommonHelper.DeviceManagerSA?.SetSnoozeLength(CurrentDeviceInfo.ID.ToString(), _SelectedSnoozeLength.SnoozeLength_sec);
                        }
                        else if (_SelectedSnoozeLength.SnoozeLength == 90)
                        {
                            //DdpmCommonHelper.DeviceManagerSA?.SetSnooze(CurrentDeviceInfo.ID.ToString(), -1); // jim 20241221 modify for 和DPeM 行為對齊 
                            DdpmCommonHelper.DeviceManagerSA?.SetSnooze(CurrentDeviceInfo?.ID.ToString(), 2);
                            //DdpmCommonHelper.DeviceManagerSA?.SetSnooze(2, CurrentDeviceInfo.ID);
                            //_SelectedSnoozeLength.SnoozeLength = 5400;
                            //DdpmCommonHelper.DeviceManagerSA?.SetSnoozeLength(CurrentDeviceInfo.ID.ToString(), _SelectedSnoozeLength.SnoozeLength_sec);
                        }
                        else if (_SelectedSnoozeLength.SnoozeLength == 120)
                        {
                            //DdpmCommonHelper.DeviceManagerSA?.SetSnooze(CurrentDeviceInfo.ID.ToString(), -1); // jim 20241221 modify for 和DPeM 行為對齊 
                            DdpmCommonHelper.DeviceManagerSA?.SetSnooze(CurrentDeviceInfo?.ID.ToString(), 3);
                            //DdpmCommonHelper.DeviceManagerSA?.SetSnooze(3, CurrentDeviceInfo.ID);
                            //_SelectedSnoozeLength.SnoozeLength = 7200;
                            //DdpmCommonHelper.DeviceManagerSA?.SetSnoozeLength(CurrentDeviceInfo.ID.ToString(), _SelectedSnoozeLength.SnoozeLength_sec);
                        }
                    }
                    //    DdpmCommonHelper.DeviceManagerSA?.SetSnooze(CurrentDeviceInfo.ID.ToString(), 1);
                    //DdpmCommonHelper.DeviceManagerSA?.SetSnooze(1, CurrentDeviceInfo.ID);
                    //else
                    //    DdpmCommonHelper.DeviceManagerSA?.SetSnooze(CurrentDeviceInfo.ID.ToString(), 0);
                    //DdpmCommonHelper.DeviceManagerSA?.SetSnooze(0, CurrentDeviceInfo.ID);

                    else //(_isChecked_Snooze == false)
                    {
                        DdpmCommonHelper.DeviceManagerSA?.SetSnooze(CurrentDeviceInfo?.ID.ToString(), -1);
                        //DdpmCommonHelper.DeviceManagerSA?.SetSnooze(-1, CurrentDeviceInfo.ID);
                    }

                    OnPropertyChanged(nameof(IsChecked_Snooze));
                    OnPropertyChanged(nameof(SnoozeStatus_String));
                    OnPropertyChanged(nameof(IsEnable_SnoozeLength)); // jim add for PIMS-328195
                }
                catch (Exception ex)
                {
                    DdpmCommonHelper.WriteUILog("DDPM.UI.Plugin.Common\\ViewModels\\WebCameraViewModel.cs IsChecked_Snooze set ex:" + ex.Message);
                }
            }
        }

        public string SnoozeStatus_String
        {
            get
            {
                return IsChecked_Snooze ? Strings.On : Strings.Off;
            }
        }

        private UI_Delay_WalkAwayLock? _SelectedDelay;
        public UI_Delay_WalkAwayLock SelectedDelay
        {
            get => _SelectedDelay;
            set
            {
                SetProperty(ref _SelectedDelay, value);
                DdpmCommonHelper.DeviceManagerSA?.SetWALTime(CurrentDeviceInfo?.ID.ToString(), _SelectedDelay.Delay);
                //DdpmCommonHelper.DeviceManagerSA?.SetIsWalkAwayLockEnable(CurrentDeviceInfo.ID.ToString(), _isChecked_WalkAwayLock);
                //DdpmCommonHelper.DeviceManagerSA?.SetIsWakeonApproachEnable(CurrentDeviceInfo.ID.ToString(), _isChecked_WakeOnApproach);
                //DdpmCommonHelper.DeviceManagerSA?.SetIsProximitySensorEnable(CurrentDeviceInfo.ID.ToString(), _isChecked_ProximitySensor);
                //DdpmCommonHelper.DeviceManagerSA?.SetWALTime(30, CurrentDeviceInfo.ID);
                OnPropertyChanged(nameof(SelectedDelay));
            }
        }

        private UI_SnoozeLength? _SelectedSnoozeLength;
        public UI_SnoozeLength SelectedSnoozeLength
        {
            get => _SelectedSnoozeLength;
            set
            {
                try
                {
                    SetProperty(ref _SelectedSnoozeLength, value);

                    //int nRes = DdpmCommonHelper.DeviceManagerSA?.GetSnooze(CurrentDeviceInfo.ID.ToString()).Result;

                    if (_SelectedSnoozeLength.SnoozeLength == 30)
                    {
                        if (_isChecked_Snooze) // Jim 20241223 add a judgment condition to set snoozewhen SnoozeLength is selected. 
                            DdpmCommonHelper.DeviceManagerSA?.SetSnooze(CurrentDeviceInfo?.ID.ToString(), 0);
                        //DdpmCommonHelper.DeviceManagerSA?.SetSnooze(0, CurrentDeviceInfo.ID);
                        //_SelectedSnoozeLength.SnoozeLength = 1800;
                        //DdpmCommonHelper.DeviceManagerSA?.SetSnoozeLength(CurrentDeviceInfo.ID.ToString(), _SelectedSnoozeLength.SnoozeLength_sec);
                    }
                    else if (_SelectedSnoozeLength.SnoozeLength == 60)
                    {
                        if (_isChecked_Snooze) // Jim 20241223 add a judgment condition to set snoozewhen SnoozeLength is selected. 
                            DdpmCommonHelper.DeviceManagerSA?.SetSnooze(CurrentDeviceInfo?.ID.ToString(), 1);
                        //DdpmCommonHelper.DeviceManagerSA?.SetSnooze(1, CurrentDeviceInfo.ID);
                        //_SelectedSnoozeLength.SnoozeLength = 3600;
                        //DdpmCommonHelper.DeviceManagerSA?.SetSnoozeLength(CurrentDeviceInfo.ID.ToString(), _SelectedSnoozeLength.SnoozeLength_sec);
                    }
                    else if (_SelectedSnoozeLength.SnoozeLength == 90)
                    {
                        if (_isChecked_Snooze) // Jim 20241223 add a judgment condition to set snoozewhen SnoozeLength is selected. 
                            DdpmCommonHelper.DeviceManagerSA?.SetSnooze(CurrentDeviceInfo?.ID.ToString(), 2);
                        //DdpmCommonHelper.DeviceManagerSA?.SetSnooze(2, CurrentDeviceInfo.ID);
                        //_SelectedSnoozeLength.SnoozeLength = 5400;
                        //DdpmCommonHelper.DeviceManagerSA?.SetSnoozeLength(CurrentDeviceInfo.ID.ToString(), _SelectedSnoozeLength.SnoozeLength_sec);
                    }
                    else if (_SelectedSnoozeLength.SnoozeLength == 120)
                    {
                        if (_isChecked_Snooze) // Jim 20241223 add a judgment condition to set snoozewhen SnoozeLength is selected. 
                            DdpmCommonHelper.DeviceManagerSA?.SetSnooze(CurrentDeviceInfo?.ID.ToString(), 3);
                        //DdpmCommonHelper.DeviceManagerSA?.SetSnooze(3, CurrentDeviceInfo.ID);
                        //_SelectedSnoozeLength.SnoozeLength = 7200;
                        //DdpmCommonHelper.DeviceManagerSA?.SetSnoozeLength(CurrentDeviceInfo.ID.ToString(), _SelectedSnoozeLength.SnoozeLength_sec);
                    }

                    //DdpmCommonHelper.DeviceManagerSA?.SetSnoozeLength(CurrentDeviceInfo.ID.ToString(), _SelectedSnoozeLength.SnoozeLength);
                    //DdpmCommonHelper.DeviceManagerSA?.SetSnoozeLength(_SelectedSnoozeLength.SnoozeLength, CurrentDeviceInfo.ID);
                    OnPropertyChanged(nameof(SelectedSnoozeLength));
                }
                catch (Exception ex)
                {
                    DdpmCommonHelper.WriteUILog("DDPM.UI.Plugin.Common\\ViewModels\\WebCameraViewModel.cs SelectedSnoozeLength set ex:" + ex.Message);
                }

            }
        }


        private string? _WALSnoozeTimeLeft;
        public string WALSnoozeTimeLeft
        {
            get => _WALSnoozeTimeLeft;
            set
            {
                SetProperty(ref _WALSnoozeTimeLeft, value);
                OnPropertyChanged(nameof(WALSnoozeTimeLeft));
            }
        }

        private Visibility _UPD_Visibility = Visibility.Visible;

        public Visibility UPD_Visibility
        {
            get
            {
                return _UPD_Visibility;
            }
            set
            {
                _UPD_Visibility = value;
                OnPropertyChanged(nameof(UPD_Visibility));
            }
        }

        private Visibility _MPS_Setting_Visibility = Visibility.Collapsed;

        public Visibility MPS_Setting_Visibility
        {
            get
            {
                return _MPS_Setting_Visibility;
            }
            set
            {
                _MPS_Setting_Visibility = value;
                OnPropertyChanged(nameof(MPS_Setting_Visibility));
            }
        }

        private Visibility _MPS_UpdateFW_Visibility = Visibility.Collapsed;

        public Visibility MPS_UpdateFW_Visibility
        {
            get
            {
                return _MPS_UpdateFW_Visibility;
            }
            set
            {
                _MPS_UpdateFW_Visibility = value;
                OnPropertyChanged(nameof(MPS_UpdateFW_Visibility));
            }
        }


        public event EventHandler<EventArgs> WebcamSettingChanged;
        public event EventHandler<EventArgs> ProfilePropertyChanged;
        public new event PropertyChangedEventHandler? PropertyChanged;

        public WebCameraViewModel(IConsole console, ILog log) : base(console, log, DdpmCommonHelper.DeviceManagerSA)
        {
            Requires.NotNull(console, nameof(console));
            Requires.NotNull(log, nameof(log));

            _log = log;
            _log!.Info($"[WebCameraViewModel] WebCameraViewModel Start ...");
        }

        public void SetResolution_Selected(int index)
        {
            try
            {
                if (index < 0 || index >= Resolution_IsSelected.Length)
                {
                    //throw new ArgumentOutOfRangeException(nameof(index), "Index is out of range.");
                    _log.Info(nameof(index), "Index is out of range.");
                    index = 0;
                }
                for (int j = 0; j < Resolution_IsSelected.Length; j++)
                {
                    Resolution_IsSelected[j] = false;
                }
                Resolution_IsSelected[index] = true;
                WebcamSettings.SelectedResolution = _resolutions[index];
                //if (!WebcamSettings.SelectedFPSs.ContainsKey(WebcamSettings.SelectedResolution))
                //    WebcamSettings.SelectedFPSs.Add(WebcamSettings.SelectedResolution, "30");
                WebcamSettings.NONE = CurrentProfile;
                WebcamSettings.ExportWebcamSettings(WebcamSettings, Model, DdpmCommonHelper.DeviceManagerSA, DdpmCommonHelper.Log);
                OnPropertyChanged(nameof(Resolution_IsSelected));
            }
            catch (Exception ex)
            {
                DdpmCommonHelper.WriteUILog("DDPM.UI.Plugin.Common\\ViewModels\\WebCameraViewModel.cs SetResolution_Selected ex:" + ex.Message);
            }
        }

        public void SetFPS_Selected(int index)
        {
            try
            {
                if (index < 0 || index >= FPS_IsSelected.Length)
                {
                    //throw new ArgumentOutOfRangeException(nameof(index), "Index is out of range.");
                    _log.Info(nameof(index), "Index is out of range.");
                    index = 0;
                }
                for (int j = 0; j < FPS_IsSelected.Length; j++)
                {
                    FPS_IsSelected[j] = false;
                }
                FPS_IsSelected[index] = true;
                WebcamSettings.SelectedFPSs[WebcamSettings.SelectedResolution] = WebcamSettings.SupportedFPSs[WebcamSettings.SelectedResolution][index];
                WebcamSettings.NONE = CurrentProfile;
                WebcamSettings.ExportWebcamSettings(WebcamSettings, Model, DdpmCommonHelper.DeviceManagerSA, DdpmCommonHelper.Log);
                OnPropertyChanged(nameof(FPS_IsSelected));
            }
            catch (Exception ex)
            {
                DdpmCommonHelper.WriteUILog("DDPM.UI.Plugin.Common\\ViewModels\\WebCameraViewModel.cs SetFPS_Selected ex:" + ex.Message);
            }
        }

        public int SelectedFovIndex = 0;
        public void SetFOV_Selected(int index, bool isUndo = false)
        {
            try
            {
                if (!IsAutoFramingOn && !isUndo)
                    FieldOfView = FOVs[index];

                for (int j = 0; j < FOV_IsSelected.Length; j++)
                {
                    FOV_IsSelected[j] = false;
                }
                FOV_IsSelected[index] = true;
                WebcamSettings.NONE = CurrentProfile;
                WebcamSettings.ExportWebcamSettings(WebcamSettings, Model, DdpmCommonHelper.DeviceManagerSA, DdpmCommonHelper.Log);
                OnPropertyChanged(nameof(FOV_IsSelected));
            }
            catch (Exception ex)
            {
                DdpmCommonHelper.WriteUILog("DDPM.UI.Plugin.Common\\ViewModels\\WebCameraViewModel.cs SetFOV_Selected ex:" + ex.Message);
            }
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
        public override bool SetCurrentDevice(string instanceIDs)
        {
            try
            {
                _log.Info("WebCameraViewModel SetCurrentDevice");
                instanceIDs ??= DeviceInfos.Values.FirstOrDefault()!.ID.ToString();

                if (!base.SetCurrentDevice(instanceIDs))
                { return false; }


                Application.Current.Dispatcher.Invoke(() =>
                {
                    InitializeWebcam();
                    PrepareProfileItems();
                });
                //if (!IsDTPReady)
                //    return false;

                OnPropertyChanged(nameof(IsMicEnumerationOn));
                OnPropertyChanged(nameof(IsMicEnumerationOnText));

                WebcamSettingChanged?.Invoke(this, EventArgs.Empty);

                if (DdpmCommonHelper.DeviceManagerSA == null)
                    _isChecked_ProximitySensor = CurrentDeviceInfo?.IsProximitySensorEnable ?? false;
                else
                    _isChecked_ProximitySensor = DdpmCommonHelper.DeviceManagerSA.GetIsProximitySensorEnable(CurrentDeviceInfo?.ID.ToString()).Result;
                if (WebcamSettings.IsFirstTime)
                {
                    IsChecked_ProximitySensor = false;
                    WebcamSettings.IsFirstTime = false;
                    WebcamSettings.NONE = CurrentProfile;
                    WebcamSettings.ExportWebcamSettings(WebcamSettings, Model, DdpmCommonHelper.DeviceManagerSA, DdpmCommonHelper.Log);
                    _log.Info("WebCameraViewModel ExportWebcamSettings Finish");
                }

                if (GlobalDefinitions.isSupport210)
                {
                    if (DdpmCommonHelper.DeviceManagerSA != null)
                        is_BackgroundBlurVisibility = DdpmCommonHelper.DeviceManagerSA.GetIsPropertyBgBlurSupported(CurrentDeviceInfo?.ID.ToString()).Result;
                    if (is_BackgroundBlurVisibility && DdpmCommonHelper.DeviceManagerSA != null)
                    {
                        IsBgBlurOn = DdpmCommonHelper.DeviceManagerSA.GetIsBgBlurEnable(CurrentDeviceInfo?.ID.ToString()).Result;
                        int DtpBgBlur = DdpmCommonHelper.DeviceManagerSA.GetBgBlur(CurrentDeviceInfo?.ID.ToString()).Result;
                        BgBlur = IsBgBlurOn == false ? 7 : DtpBgBlur == 0 ? 7 : DtpBgBlur;
                    }
                }

                IsMicEnumerationOnEnabled = true;
                AlertVisibility = Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                DdpmCommonHelper.WriteUILog("DDPM.UI.Plugin.Common\\ViewModels\\WebCameraViewModel.cs  SetCurrentDevice ex:" + ex.Message);
            }
            return true;
        }

        public bool IsUSB3 = false;
        private void InitializeWebcam()
        {
            //Model = "U3224KB";
            //CurrentDeviceInfo!.ModelNumber = "U3224KB";
            try
            {
                WebcamSettings = WebcamSettings.ImportWebcamSettings(Model, CurrentDeviceInfo, DdpmCommonHelper.DeviceManagerSA, DdpmCommonHelper.Log);

                //if (CurrentDeviceInfo.IsPropertyZoomSupported)
                //{
                //    var zoom = DdpmCommonHelper.DeviceManagerSA?.GetZoom(CurrentDeviceInfo.ID.ToString()).Result;
                //    if (zoom == -1)
                //    {
                //        _log.Error("DTP GetZoom fail!");
                //        _zoom = CurrentDeviceInfo.ZoomMin;
                //    }
                //    else
                //        _zoom = zoom;
                //    OnPropertyChanged(nameof(PanArrowVisibility));
                //}
                //if (CurrentDeviceInfo.IsPropertyFocusSupported)
                //{
                //    var isFocusOn = DdpmCommonHelper.DeviceManagerSA?.GetIsFocusOn(CurrentDeviceInfo.ID.ToString()).Result;
                //    if (isFocusOn == null)
                //    {
                //        _log.Error("DTP GetIsFocusOn fail!");
                //        _isFocusOn = false;
                //    }
                //    else
                //        _isFocusOn = isFocusOn.Value;

                //    var focus = DdpmCommonHelper.DeviceManagerSA?.GetFocus(CurrentDeviceInfo.ID.ToString()).Result;
                //    if (focus == -1)
                //    {
                //        _log.Error("DTP GetFocus fail!");
                //        _focus = CurrentDeviceInfo.FocusMin;
                //    }
                //    else
                //        _focus = focus;
                //}
                //if (CurrentDeviceInfo.IsPropertyPrioritySupported)
                //{
                //    var priority = DdpmCommonHelper.DeviceManagerSA?.GetPriority(CurrentDeviceInfo.ID.ToString()).Result;
                //    if (priority == -1)
                //    {
                //        _log.Error("DTP GetPriority fail!");
                //        _priority = 0;
                //    }
                //    else
                //        _priority = priority;
                //}
                //if (CurrentDeviceInfo.IsPropertyAntiFlickerSupported)
                //{
                //    var antiFlicker = DdpmCommonHelper.DeviceManagerSA?.GetAntiFlicker(CurrentDeviceInfo.ID.ToString()).Result;
                //    if (antiFlicker == -1)
                //    {
                //        _log.Error("DTP GetAntiFlicker fail!");
                //        _antiFlicker = 1;
                //    }
                //    else
                //        _antiFlicker = antiFlicker;
                //}
                //if (CurrentDeviceInfo.IsPropertyAutoFramingTransitionSupported)
                //{
                //    var isAutoFramingTransitionOn = DdpmCommonHelper.DeviceManagerSA?.GetIsAutoFramingTransitionOn(CurrentDeviceInfo.ID.ToString()).Result;
                //    if (isAutoFramingTransitionOn == null)
                //    {
                //        _log.Error("DTP GetIsAutoFramingTransitionOn fail!");
                //        _isAutoFramingTransitionOn = false;
                //    }
                //    else
                //        _isAutoFramingTransitionOn = isAutoFramingTransitionOn.Value;
                //}
                //if (CurrentDeviceInfo.IsPropertyAutoFramingSensitivitySupported)
                //{
                //    var autoFramingSensitivity = DdpmCommonHelper.DeviceManagerSA?.GetAutoFramingSensitivity(CurrentDeviceInfo.ID.ToString()).Result;
                //    if (autoFramingSensitivity == -1)
                //    {
                //        _log.Error("DTP GetAutoFramingSensitivity fail!");
                //        _autoFramingSensitivity = 1;
                //    }
                //    else
                //        _autoFramingSensitivity = autoFramingSensitivity;
                //}
                //if (CurrentDeviceInfo.IsPropertyAutoFramingSizeSupported)
                //{
                //    var autoFramingFrameSize = DdpmCommonHelper.DeviceManagerSA?.GetAutoFramingFrameSize(CurrentDeviceInfo.ID.ToString()).Result;
                //    if (autoFramingFrameSize == -1)
                //    {
                //        _log.Error("DTP GetAutoFramingFrameSize fail!");
                //        _autoFramingFrameSize = 1;
                //    }
                //    else
                //        _autoFramingFrameSize = autoFramingFrameSize;
                //}

                for (int k = 0; k < CurrentDeviceInfo?.FOVValues?.Length; k++)
                {
                    _fOVs[k] = int.Parse(CurrentDeviceInfo.FOVValues[k]);
                }
                _log.Info($"GetIsAllSupportedResolutionsFound Before :{IsUSB3}");
                if (Model == "WB5023" || Model == "WB3023")
                {
                    IsUSB3 = true;
                }
                else
                {
                    if (DdpmCommonHelper.DeviceManagerSA != null)
                        IsUSB3 = DdpmCommonHelper.DeviceManagerSA.GetIsAllSupportedResolutionsFound(CurrentDeviceInfo?.ID.ToString()).Result;
                }
                _log.Info($"GetIsAllSupportedResolutionsFound After :{IsUSB3}");
                if (!IsUSB3)
                {
                    _ = DdpmCommonHelper.DeviceManagerSA?.SetIsHDROn(CurrentDeviceInfo?.ID.ToString(), false);
                    OnPropertyChanged(nameof(IsHDROn));
                    OnPropertyChanged(nameof(IsHDROnText));
                }
                SetProfile();
                //if (!IsDTPReady)
                //    return;

                _resolutions = WebcamSettings.Resolutions.Keys.ToList();
                var i = WebcamSettings.Resolutions.Keys.ToList().IndexOf(WebcamSettings.SelectedResolution);
                _log.Info($"DTP _resolutions:{JsonConvert.SerializeObject(_resolutions)}!");
                _log.Info($"DTP (WebcamSettings.Selected_Resolution:{WebcamSettings.SelectedResolution}!");
                _log.Info($"DTP i:{i}!");

                SetResolution_Selected(i);
                _log.Info($"DTP _resolutions:{JsonConvert.SerializeObject(WebcamSettings.SupportedFPSs)}!");
                var j = WebcamSettings.SupportedFPSs[WebcamSettings.SelectedResolution].IndexOf(WebcamSettings.SelectedFPSs[WebcamSettings.SelectedResolution]);
                _log.Info($"DTP j:{j}!");

                SetFPS_Selected(j);
                foreach (var sf in WebcamSettings.SelectedFPSs)
                {
                    if (sf.Key != WebcamSettings.SelectedResolution)
                        WebcamSettings.SelectedFPSs[sf.Key] = "30";
                }
                IsResolutionSectionEnable = true;

                if (CurrentDeviceInfo?.IsWindowsHelloSupported ?? false)
                {
                    Task<bool?>? task = DdpmCommonHelper.DeviceManagerSA?.GetIsPrioritizeExternalWebcam(CurrentDeviceID.ToString());
                    var result = task?.Result;
                    if (result == null)
                    {
                        _log.Error("DTP GetIsPrioritizeExternalWebcam fail!");
                        _isPrioritizeExternalWebcam = CurrentDeviceInfo.IsPrioritizeExternalWebcam;
                    }
                    else
                    {
                        _isPrioritizeExternalWebcam = result.Value;
                    }
                }
            }
            catch (Exception ex)
            {
                DdpmCommonHelper.WriteUILog("DDPM.UI.Plugin.Common\\ViewModels\\WebCameraViewModel.cs  InitializeWebcam ex:" + ex.Message);
            }
        }

        public bool IsSettingProfile = false;
        public void SetProfile()
        {
            IsSettingProfile = true;
            try
            {
                if (CurrentProfileName.ToUpper() == "NONE")
                    CurrentProfile = WebcamSettings.NONE;
                else
                {
                    if (WebcamSettings.CustomProfiles.TryGetValue(CurrentProfileName, out WebcamProfile? value))
                        CurrentProfile = JsonConvert.DeserializeObject<WebcamProfile>(JsonConvert.SerializeObject(value))!;
                    else
                        CurrentProfile = JsonConvert.DeserializeObject<WebcamProfile>(JsonConvert.SerializeObject(WebcamSettings.PresetProfiles[CurrentProfileName]))!;
                }
                if (!IsUSB3)
                    CurrentProfile.IsHDROn = false;


                if (CurrentDeviceInfo?.IsPropertyFOVSupported ?? false)
                {
                    //task = DdpmCommonHelper.DeviceManagerSA?.SetFieldOfView(CurrentDeviceInfo.ID.ToString(), CurrentProfile.FieldOfView);
                    //if (!task.Result)
                    //{
                    //    _log.Error("DTP SetFieldOfView fail!");
                    //}
                    if (_fOVs[0] == CurrentProfile.FieldOfView)
                    {
                        SetFOV_Selected(0);
                        SelectedFovIndex = 0;
                    }
                    else if (_fOVs[1] == CurrentProfile.FieldOfView)
                    {
                        SetFOV_Selected(1);
                        SelectedFovIndex = 1;
                    }
                    else
                    {
                        SetFOV_Selected(2);
                        SelectedFovIndex = 2;
                    }
                }
                Task<bool>? task;
                if (CurrentDeviceInfo?.IsPropertyAutoFramingSupported ?? false)
                {
                    //task = DdpmCommonHelper.DeviceManagerSA?.SetIsAutoFramingOn(CurrentDeviceInfo.ID.ToString(), CurrentProfile.IsAutoFramingOn);
                    //if (!task.Result)
                    //{
                    //    _log.Error("DTP SetIsAutoFramingOn fail!");
                    //}
                    //OnPropertyChanged(nameof(IsAutoFramingOn));
                    //OnPropertyChanged(nameof(IsAutoFramingOnText));
                    IsAutoFramingOn = CurrentProfile.IsAutoFramingOn;
                }
                if (CurrentDeviceInfo?.IsPropertyAutoFramingTransitionSupported ?? false && DdpmCommonHelper.DeviceManagerSA != null)
                {
                    task = DdpmCommonHelper.DeviceManagerSA?.SetIsAutoFramingTransitionOn(CurrentDeviceInfo?.ID.ToString(), CurrentProfile.IsAutoFramingTransitionOn);
                    if (task == null || !task.Result)
                    {
                        _log.Error("DTP SetIsAutoFramingTransitionOn fail!");
                    }
                    _isAutoFramingTransitionOn = CurrentProfile.IsAutoFramingTransitionOn;
                    OnPropertyChanged(nameof(IsAutoFramingTransitionOn));
                    OnPropertyChanged(nameof(IsAutoFramingTransitionOnText));
                }
                if (CurrentDeviceInfo?.IsPropertyAutoFramingSensitivitySupported ?? false)
                {
                    AutoFramingSensitivity = CurrentProfile.AutoFramingSensitivity;
                    //OnPropertyChanged(nameof(AutoFramingSensitivity));
                }
                if (CurrentDeviceInfo?.IsPropertyAutoFramingSizeSupported ?? false)
                {
                    AutoFramingFrameSize = CurrentProfile.AutoFramingFrameSize;
                    //OnPropertyChanged(nameof(AutoFramingFrameSize));
                }
                if (CurrentDeviceInfo?.IsPropertyAntiFlickerSupported ?? false)
                {
                    AntiFlicker = CurrentProfile.AntiFlicker;
                    //OnPropertyChanged(nameof(AntiFlicker));
                }
                if (CurrentDeviceInfo?.IsPropertyPrioritySupported ?? false)
                {
                    Priority = CurrentProfile.Priority;
                    //OnPropertyChanged(nameof(Priority));
                }
                if (CurrentDeviceInfo?.IsPropertyFocusSupported ?? false)
                {
                    IsFocusOn = CurrentProfile.IsFocusOn;
                    Focus = CurrentProfile.Focus;

                }

                if (CurrentDeviceInfo?.IsPropertyHDRSupported ?? false && IsUSB3 && DdpmCommonHelper.DeviceManagerSA != null)
                {
                    task = DdpmCommonHelper.DeviceManagerSA?.SetIsHDROn(CurrentDeviceInfo?.ID.ToString(), CurrentProfile.IsHDROn);
                    if (task == null || !task.Result)
                    {
                        _log.Error("DTP SetIsHDROn fail!");
                    }
                    OnPropertyChanged(nameof(IsHDROn));
                    OnPropertyChanged(nameof(IsHDROnText));
                }

                if (CurrentDeviceInfo?.IsPropertyWhiteBalanceSupported ?? false && DdpmCommonHelper.DeviceManagerSA != null)
                {
                    task = DdpmCommonHelper.DeviceManagerSA?.SetIsAutoWhiteBalanceOn(CurrentDeviceInfo?.ID.ToString(), CurrentProfile.IsAutoWhiteBalanceOn);
                    if (task == null || !task.Result)
                    {
                        _log.Error("DTP SetIsAutoWhiteBalanceOn fail!");
                    }
                    OnPropertyChanged(nameof(IsAutoWhiteBalanceOn));
                    OnPropertyChanged(nameof(IsAutoWhiteBalanceOnText));
                    AutoWhiteBalance = CurrentProfile.AutoWhiteBalance;
                }

                if (CurrentDeviceInfo?.IsPropertyBrightnessSupported ?? false)
                {
                    Brightness = CurrentProfile.Brightness;
                }

                if (CurrentDeviceInfo?.IsPropertySharpnessSupported ?? false)
                {
                    Sharpness = CurrentProfile.Sharpness;
                }

                if (CurrentDeviceInfo?.IsPropertyContrastSupported ?? false)
                {
                    Contrast = CurrentProfile.Contrast;
                }

                if (CurrentDeviceInfo?.IsPropertySaturationSupported ?? false)
                {
                    Saturation = CurrentProfile.Saturation;
                }
                if (CurrentDeviceInfo?.IsPropertyZoomSupported ?? false)
                {
                    Zoom = CurrentProfile.Zoom;
                }
                if (CurrentDeviceInfo?.IsPropertyPanSupported ?? false)
                {
                    SetPan(CurrentProfile.Pan);
                }
                if (CurrentDeviceInfo?.IsPropertyTiltSupported ?? false)
                {
                    SetTilt(CurrentProfile.Tilt);
                }
                WebcamSettings.NONE = CurrentProfile;
                WebcamSettings.ExportWebcamSettings(WebcamSettings, Model, DdpmCommonHelper.DeviceManagerSA, DdpmCommonHelper.Log);

                ClearUndo();
            }
            catch (Exception ex)
            {
                DdpmCommonHelper.WriteUILog("DDPM.UI.Plugin.Common\\ViewModels\\WebCameraViewModel.cs  SetProfile ex:" + ex.Message);
            }
            IsSettingProfile = false;
        }

        public override void HandleNotification(DeviceChangedType changeType, DeviceInfo di, string property = "")
        {

            try
            {

                base.HandleNotification(changeType, di, property);

                switch (changeType)
                {
                    case DeviceChangedType.Peripherals_SettingsChange:
                        if (di.ID == CurrentDeviceID)
                        {
                            switch (property)
                            {
                                case "IsHDROnChanged":
                                    if (bool.TryParse(di.Message, out bool isHDROn) && IsHDROn != isHDROn)
                                    {
                                        Application.Current.Dispatcher.Invoke(() =>
                                        {
                                            IsHDROn = isHDROn;
                                        });
                                    }
                                    break;
                                case "IsAutoFramingOnChanged":
                                    if (bool.TryParse(di.Message, out bool isAFOn) && _isAutoFramingOn != isAFOn)
                                    {
                                        Application.Current.Dispatcher.Invoke(() =>
                                        {
                                            IsAutoFramingOn = isAFOn;
                                        });
                                    }
                                    break;
                                case "IsProximitySensorEnableChanged":
                                    Application.Current.Dispatcher.Invoke(() =>
                                    {
                                        _isChecked_ProximitySensor = di.IsProximitySensorEnable;
                                        OnPropertyChanged(nameof(IsChecked_ProximitySensor));
                                    });
                                    break;
                                default:
                                    break;
                            }
                            //GenerateInfo();
                        }
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                DdpmCommonHelper.WriteUILog("DDPM.UI.Plugin.Common\\ViewModels\\WebCameraViewModel.cs  HandleNotification ex:" + ex.Message);
            }
        }

        public MediaCapture? MediaCapture;
        public MediaFrameReader? MediaFrameReader;

        // 20250425 Chewlin for PIMS-358978 add PrioritizeExternalWebcamStatus_Text property
        public string PrioritizeExternalWebcamStatus_Text
        {
            get => IsPrioritizeExternalWebcam ? Strings.On : Strings.Off;
        }

        private bool _isPrioritizeExternalWebcam = false;
        public bool IsPrioritizeExternalWebcam
        {
            get => _isPrioritizeExternalWebcam;
            set
            {
                _isPrioritizeExternalWebcam = value;
                DdpmCommonHelper.DeviceManagerSA?.SetIsPrioritizeExternalWebcam(CurrentDeviceID.ToString(), value);
                OnPropertyChanged();
                // 20250425 Chewlin for PIMS-358978 add OnPropertyChanged(nameof(PrioritizeExternalWebcamStatus_Text));
                OnPropertyChanged(nameof(PrioritizeExternalWebcamStatus_Text));
            }
        }
        public string CurrentProfileName
        {
            get => WebcamSettings.SelectedProfileName;
            set
            {
                WebcamSettings.SelectedProfileName = value;
                WebcamSettings.NONE = CurrentProfile;
                WebcamSettings.ExportWebcamSettings(WebcamSettings, Model, DdpmCommonHelper.DeviceManagerSA, DdpmCommonHelper.Log);
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
                OnPropertyChanged(nameof(IsFPS1Enable));
                OnPropertyChanged(nameof(IsFPS2Enable));
                OnPropertyChanged(nameof(hdr_enable));
            }
        }

        public bool IsNotAutoFramingOn { get => !IsAutoFramingOn; }
        public bool IsNotRecording { get => !IsRecording; }
        //public bool IsFPS1Enable { get => !IsRecording && !(IsAutoFramingOn && WebcamSettings.SupportedFPSs[WebcamSettings.SelectedResolution].Count > 1 && WebcamSettings.SupportedFPSs[WebcamSettings.SelectedResolution][1] == "60"); }
        public bool IsFPS1Enable
        {
            get
            {
                if (WebcamSettings?.SupportedFPSs == null || !WebcamSettings.SupportedFPSs.ContainsKey(WebcamSettings.SelectedResolution))
                {
                    return false;
                }

                var supportedFPS = WebcamSettings.SupportedFPSs[WebcamSettings.SelectedResolution];
                return !IsRecording && !(IsAutoFramingOn && supportedFPS.Count > 1 && supportedFPS[1] == "60");
            }
        }
        //public bool IsFPS2Enable { get => !IsRecording && !(IsAutoFramingOn && WebcamSettings.SupportedFPSs[WebcamSettings.SelectedResolution].Count > 2 && WebcamSettings.SupportedFPSs[WebcamSettings.SelectedResolution][2] == "60"); }
        public bool IsFPS2Enable
        {
            get
            {
                if (WebcamSettings?.SupportedFPSs == null || !WebcamSettings.SupportedFPSs.ContainsKey(WebcamSettings.SelectedResolution))
                {
                    return false;
                }

                var supportedFPS = WebcamSettings.SupportedFPSs[WebcamSettings.SelectedResolution];
                return !IsRecording && !(IsAutoFramingOn && supportedFPS.Count > 2 && supportedFPS[2] == "60");
            }
        }
        public int btnRes0_width { get; set; }
        public int btnRes1_width { get; set; }
        public int btnRes2_width { get; set; }
        public int btnRes3_width { get; set; }
        public bool IsResolutionSectionEnable { get; set; } = true;


        public CornerRadius btnRes0_radius { get => btnRes0_radius_v; }
        public CornerRadius btnRes0_radius_v = new CornerRadius(5, 0, 0, 5);
        public CornerRadius btnRes1_radius { get => btnRes1_radius_v; }
        public CornerRadius btnRes1_radius_v = new CornerRadius(0, 0, 0, 0);

        public Visibility btnRes0_show { get; set; } = Visibility.Visible;
        public Visibility btnRes1_show { get; set; } = Visibility.Visible;
        public Visibility btnRes2_show { get; set; } = Visibility.Visible;
        public Visibility btnRes3_show { get; set; } = Visibility.Visible;


        public Visibility bdrPrioritize_show_value = Visibility.Visible;
        public Visibility bdrPrioritize_show
        {
            get => bdrPrioritize_show_value;
            set
            {
                bdrPrioritize_show_value = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(bdrPrioritize_show));
            }
        }




        public Visibility brdHello_show { get; set; } = Visibility.Visible;
        public Visibility brdHello_show_control { get; set; } = Visibility.Visible;

        public bool is_hdr_enable = true;
        public bool usb_hdr_enable = true;
        public bool hdr_enable
        {
            get => IsNotRecording && is_hdr_enable;
            set
            {
                is_hdr_enable = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsNotRecording));
                OnPropertyChanged(nameof(IsFPS1Enable));
                OnPropertyChanged(nameof(IsFPS2Enable));
                //OnPropertyChanged(nameof(is_hdr_enable));
                OnPropertyChanged(nameof(hdr_enable));
            }
        }

        public bool is_ProximitySensor_enable = true;
        public bool IsProximitySensorEnable
        {
            get => is_ProximitySensor_enable;

            set
            {
                is_ProximitySensor_enable = value;
                OnPropertyChanged();
            }

        }

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
                OnPropertyChanged(nameof(AutofocusStatus_String));
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
                WebcamSettings.NONE = CurrentProfile;
                WebcamSettings.ExportWebcamSettings(WebcamSettings, Model, DdpmCommonHelper.DeviceManagerSA, DdpmCommonHelper.Log);
            }
        }
        public bool WebcamCountdown
        {
            get => WebcamSettings.WebcamCountdown;
            set
            {
                WebcamSettings.WebcamCountdown = value;
                WebcamSettings.NONE = CurrentProfile;
                WebcamSettings.ExportWebcamSettings(WebcamSettings, Model, DdpmCommonHelper.DeviceManagerSA, DdpmCommonHelper.Log);
            }
        }
        public bool WebcamGrid
        {
            get => WebcamSettings.WebcamGrid;
            set
            {
                if (value == WebcamSettings.WebcamGrid)
                    return;

                WebcamSettings.WebcamGrid = value;
                WebcamSettings.NONE = CurrentProfile;
                WebcamSettings.ExportWebcamSettings(WebcamSettings, Model, DdpmCommonHelper.DeviceManagerSA, DdpmCommonHelper.Log);
                OnPropertyChanged();
                OnPropertyChanged(nameof(WebcamGridVisibity));
                //WebcamSettingChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        private bool _showGrid = true;
        public bool ShowGrid
        {
            get => _showGrid;
            set
            {
                _showGrid = value;
                OnPropertyChanged(nameof(WebcamGridVisibity));
            }
        }
        public Visibility WebcamGridVisibity
        {
            get => ShowGrid && WebcamGrid ? Visibility.Visible : Visibility.Collapsed;
        }
        public string IsMicEnumerationOnText
        {
            get => CurrentDeviceInfo?.IsMicEnumerationOn ?? false ? Strings.On : Strings.Off;
        }
        public bool isIsMicEnumerationOnChanged_event = false;
        public bool IsMicEnumerationOn
        {
            get => CurrentDeviceInfo?.IsMicEnumerationOn ?? false;
            set
            {
                DdpmCommonHelper.WriteUILog("WebCameraMicrophone Action 2 : " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                if (value == IsMicEnumerationOn)
                    return;
                DdpmCommonHelper.WriteUILog("WebCameraMicrophone Action 3 : " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

                if (!isIsMicEnumerationOnChanged_event)
                {
                    DdpmCommonHelper.WriteUILog("WebCameraMicrophone Action 4 : " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                    DdpmCommonHelper.DeviceManagerSA?.SetIsMicEnumerationOn(value, CurrentDeviceInfo?.ID ?? new());
                }
                DdpmCommonHelper.WriteUILog("WebCameraMicrophone Action 5 : " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsMicEnumerationOnText));
            }
        }

        public string IsAutoFramingOnText
        {
            get => CurrentProfile.IsAutoFramingOn ? Strings.On : Strings.Off;
        }

        private bool OriginalAutoFocus = true;
        private bool _isAutoFramingOn = false;
        public bool IsAutoFramingOn
        {
            get => _isAutoFramingOn;
            set
            {
                try
                {
                    _isAutoFramingOn = value;
                    //if (value && WebcamSettings.SupportedFPSs[WebcamSettings.SelectedResolution].Count > 1)
                    //{
                    //    if (WebcamSettings.SupportedFPSs[WebcamSettings.SelectedResolution][1] == "60")
                    //        SetFPS_Selected(0);
                    //    else if (WebcamSettings.SupportedFPSs[WebcamSettings.SelectedResolution].Count > 2 && WebcamSettings.SupportedFPSs[WebcamSettings.SelectedResolution][2] == "60")
                    //        SetFPS_Selected(1);
                    //}
                    if (value && WebcamSettings?.SupportedFPSs != null && WebcamSettings.SupportedFPSs.ContainsKey(WebcamSettings.SelectedResolution))
                    {
                        var supportedFPS = WebcamSettings.SupportedFPSs[WebcamSettings.SelectedResolution];
                        if (supportedFPS.Count > 1 && supportedFPS[1] == "60")
                            SetFPS_Selected(0);
                        else if (supportedFPS.Count > 2 && supportedFPS[2] == "60")
                            SetFPS_Selected(1);
                    }

                    //if (!isUIHasUpdateByQAM)
                    DdpmCommonHelper.DeviceManagerSA?.SetIsAutoFramingOn(CurrentDeviceInfo?.ID.ToString(), value);

                    SetProfileProperty(nameof(IsAutoFramingOn), value, OperationModule.CameraControl);
                    //CurrentProfile.IsAutoFramingOn = value;
                    SetAutoFraming();
                }
                catch (Exception ex)
                {
                    DdpmCommonHelper.WriteUILog("DDPM.UI.Plugin.Common\\ViewModels\\WebCameraViewModel.cs  IsAutoFramingOn set ex:" + ex.Message);
                }
            }
        }

        private void SetAutoFraming()
        {
            if (CurrentDeviceInfo == null)
                return;

            if (_isAutoFramingOn)
            {
                var FOV = CurrentDeviceInfo.FOVValues;
                SetFOV_Selected(FOV.Length - 1, true);

                //Derek 2024/11/06 Webcam PIMS-317629 
                //On Turned on Auto Frame AI option, autofocus should be on and be greyed out. (can't select)
                OriginalAutoFocus = IsFocusOn;
                _isFocusOn = true;
            }
            else
            {
                SetFOV_Selected(SelectedFovIndex, true);
                if (!OriginalAutoFocus)
                    _isFocusOn = false;
            }
            OnPropertyChanged(nameof(IsAutoFramingOn));
            OnPropertyChanged(nameof(IsAutoFramingOnText));
            OnPropertyChanged(nameof(IsFocusOn));
            OnPropertyChanged(nameof(IsFocusOnText));
            OnPropertyChanged(nameof(PanArrowVisibility));
            OnPropertyChanged(nameof(IsNotAutoFramingOn));
            OnPropertyChanged(nameof(IsFPS1Enable));
            OnPropertyChanged(nameof(IsFPS2Enable));
        }

        public string IsAutoFramingTransitionOnText
        {
            get => CurrentProfile.IsAutoFramingTransitionOn ? Strings.On : Strings.Off;
        }

        private bool _isAutoFramingTransitionOn = false;
        public bool IsAutoFramingTransitionOn
        {
            get => _isAutoFramingTransitionOn;
            set
            {
                if (value != _isAutoFramingTransitionOn)
                {
                    _isAutoFramingTransitionOn = value;
                    //CurrentProfile.IsAutoFramingTransitionOn = _isAutoFramingTransitionOn = value; // 20250205 Kidd to fix AutoFramingTransition text is always "On"  

                    DdpmCommonHelper.DeviceManagerSA?.SetIsAutoFramingTransitionOn(CurrentDeviceInfo?.ID.ToString(), value);
                    SetProfileProperty(nameof(IsAutoFramingTransitionOn), value, OperationModule.CameraControl);
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(IsAutoFramingTransitionOnText));
                }
            }
        }

        private int _autoFramingSensitivity = 0;
        public int AutoFramingSensitivity
        {
            get => _autoFramingSensitivity;
            set
            {
                if (value != _autoFramingSensitivity)
                {
                    _autoFramingSensitivity = value;
                    DdpmCommonHelper.DeviceManagerSA?.SetAutoFramingSensitivity(CurrentDeviceInfo?.ID.ToString(), value);
                    SetProfileProperty(nameof(AutoFramingSensitivity), value, OperationModule.CameraControl);
                    OnPropertyChanged();
                }
            }
        }

        private int _autoFramingFrameSize = 0;
        public int AutoFramingFrameSize
        {
            get => _autoFramingFrameSize;
            set
            {
                if (value != _autoFramingFrameSize)
                {
                    _autoFramingFrameSize = value;
                    DdpmCommonHelper.DeviceManagerSA?.SetAutoFramingFrameSize(CurrentDeviceInfo?.ID.ToString(), value);
                    SetProfileProperty(nameof(AutoFramingFrameSize), value, OperationModule.CameraControl);
                    OnPropertyChanged();
                }
            }
        }

        private int _fieldOfView = 0;
        public int FieldOfView
        {
            get => _fieldOfView;
            set
            {
                if (value == _fieldOfView)
                    return;

                _fieldOfView = value;
                if (!isUIHasUpdateByQAM)
                    DdpmCommonHelper.DeviceManagerSA?.SetFieldOfView(CurrentDeviceID.ToString(), value);

                SetProfileProperty(nameof(FieldOfView), value, OperationModule.CameraControl);
                OnPropertyChanged();
            }
        }

        public Visibility ZoomVisibility
        {
            get => CurrentDeviceInfo?.IsPropertyZoomSupported ?? false ? Visibility.Visible : Visibility.Collapsed;
        }

        private int _zoom = -1;
        public int Zoom
        {
            get => _zoom;
            set
            {
                if (value != _zoom)
                {
                    _zoom = value;
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
            try
            {
                DdpmCommonHelper.DeviceManagerSA?.SetZoom(CurrentDeviceInfo?.ID.ToString(), _zoom);

                SetProfileProperty(nameof(Zoom), _zoom, OperationModule.CameraControl);
                OnPropertyChanged(nameof(PanArrowVisibility));
            }
            catch (Exception ex)
            {
                DdpmCommonHelper.WriteUILog("DDPM.UI.Plugin.Common\\ViewModels\\WebCameraViewModel.cs SetZoom() ex:" + ex.Message);
            }
        }

        private int _bgBlur = 0;
        public int BgBlur
        {
            get => _bgBlur;
            set
            {
                if (value != _bgBlur)
                {
                    _bgBlur = value;
                    DdpmCommonHelper.DeviceManagerSA?.SetBgBlur(CurrentDeviceInfo?.ID.ToString(), value);
                    SetProfileProperty(nameof(BgBlur), value, OperationModule.CameraControl);
                    OnPropertyChanged();
                }
            }
        }
        private bool _isBgBlurOn = false;
        public bool IsBgBlurOn
        {
            get => _isBgBlurOn;
            set
            {
                if (value != _isBgBlurOn)
                {
                    //_isAutoFramingTransitionOn = value;
                    CurrentProfile.IsBgBlurEnable = _isBgBlurOn = value;

                    DdpmCommonHelper.DeviceManagerSA?.SetIsBgBlurEnable(CurrentDeviceInfo?.ID.ToString(), value);
                    if (value == false)
                    {
                        DdpmCommonHelper.DeviceManagerSA?.SetBgBlur(CurrentDeviceInfo?.ID.ToString(), 0);
                    }
                    SetProfileProperty(nameof(IsBgBlurOn), value, OperationModule.CameraControl);
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(IsBgBlurText));
                }
            }
        }

        public bool is_BackgroundBlurVisibility = false;
        public Visibility BackgroundBlurVisibility
        {
            get => is_BackgroundBlurVisibility ? Visibility.Visible : Visibility.Collapsed;

        }
        public string IsBgBlurText
        {
            get => CurrentProfile.IsBgBlurEnable ? Strings.On : Strings.Off;
        }

        public Visibility AutofocusVisibility
        {
            get => CurrentDeviceInfo.IsPropertyFocusSupported ? Visibility.Visible : Visibility.Collapsed;
        }

        private bool _isFocusOn = false;
        public bool IsFocusOn
        {
            get => _isFocusOn;
            set
            {
                if (value != _isFocusOn) //Derek 1108 for Webcam PIMS-317629 
                {
                    _isFocusOn = value;

                    DdpmCommonHelper.DeviceManagerSA?.SetIsFocusOn(CurrentDeviceInfo?.ID.ToString(), value);

                    SetProfileProperty(nameof(IsFocusOn), value, OperationModule.CameraControl);
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(IsFocusOnText));
                    if (!value)
                        SetFocus();
                }
            }
        }
        public string IsFocusOnText
        {
            get => IsFocusOn ? Strings.On : Strings.Off;
        }

        private int _focus = -1;
        public int Focus
        {
            get => _focus;
            set
            {
                if (value != _focus)
                {
                    _focus = value;
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
            DdpmCommonHelper.DeviceManagerSA?.SetFocus(CurrentDeviceInfo?.ID.ToString(), _focus);

            SetProfileProperty(nameof(Focus), _focus, OperationModule.CameraControl);
        }

        public Visibility PriorityVisibility
        {
            get => CurrentDeviceInfo?.IsPropertyPrioritySupported ?? false ? Visibility.Visible : Visibility.Collapsed;
        }

        private int _priority = 0;
        public int Priority
        {
            get => _priority;
            set
            {
                if (value != _priority)
                {
                    _priority = value;
                    DdpmCommonHelper.DeviceManagerSA?.SetPriority(CurrentDeviceInfo?.ID.ToString(), value);
                    SetProfileProperty(nameof(Priority), value, OperationModule.CameraControl);
                    OnPropertyChanged();
                }
            }
        }
        public Visibility HDRVisibility
        {
            get => CurrentDeviceInfo?.IsPropertyHDRSupported ?? false ? Visibility.Visible : Visibility.Collapsed;
        }
        public bool hdr_change = false;

        //public void OnUpdateIsHDROn()
        //{
        //    try
        //    {
        //        WebcamProfile? tmp = JsonConvert.DeserializeObject<WebcamProfile>(JsonConvert.SerializeObject(WebcamSettings.PresetProfiles[CurrentProfileName]));
        //        if (tmp != null)
        //        {
        //            CurrentProfile = tmp;
        //            OnPropertyChanged(nameof(IsHDROn));
        //            OnPropertyChanged(nameof(IsHDROnText));
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        DdpmCommonHelper.WriteUILog($"[WebCameraViewModel][OnUpdateIsHDROn] exception: {ex.Message}");
        //    }
        //}

        public bool IsHDROn
        {
            get => CurrentProfile.IsHDROn;
            set
            {
                try
                {
                    //OnPropertyChanged(nameof(hdr_enable));
                    //OnPropertyChanged(nameof(IsHDROnText));

                    if (value == CurrentProfile.IsHDROn)
                        return;

                    AlertType = WebcamAlert.Alert1;
                    AlertVisibility = Visibility.Visible;
                    hdr_change = true;
                    is_hdr_enable = false;
                    OnPropertyChanged(nameof(hdr_enable));
                    if (!isUIHasUpdateByQAM)
                        DdpmCommonHelper.DeviceManagerSA?.SetIsHDROn(CurrentDeviceInfo?.ID.ToString(), value);
                    SetProfileProperty(nameof(IsHDROn), value, OperationModule.ColorAndImage);
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(IsHDROn));
                    OnPropertyChanged(nameof(IsHDROnText));
                    CurrentProfile.IsHDROn = value;

                    //new Thread(() =>
                    //{
                    //    Thread.Sleep(3000);
                    //    AlertVisibility = Visibility.Collapsed;

                    //    //is_hdr_enable = true;
                    //    is_hdr_enable = usb_hdr_enable;
                    //    OnPropertyChanged(nameof(hdr_enable));

                    //}).Start();
                    Task.Run(async () =>
                    {
                        await Task.Delay(3000);
                        AlertVisibility = Visibility.Collapsed;

                        //is_hdr_enable = true;
                        is_hdr_enable = usb_hdr_enable;
                        OnPropertyChanged(nameof(hdr_enable));
                    });

                }
                catch (Exception ex)
                {
                    DdpmCommonHelper.WriteUILog("DDPM.UI.Plugin.Common\\ViewModels\\WebCameraViewModel.cs IsHDROn set ex:" + ex.Message);
                }
            }
        }
        public string IsHDROnText
        {
            get => CurrentProfile.IsHDROn ? Strings.On : Strings.Off;
        }

        public Visibility AutoWhiteBalanceVisibility
        {
            get => CurrentDeviceInfo.IsPropertyWhiteBalanceSupported ? Visibility.Visible : Visibility.Collapsed;
        }
        public bool IsAutoWhiteBalanceOn
        {
            get => CurrentProfile.IsAutoWhiteBalanceOn;
            set
            {
                if (value == IsAutoWhiteBalanceOn)
                    return;

                if (!isUIHasUpdateByQAM)
                    DdpmCommonHelper.DeviceManagerSA?.SetIsAutoWhiteBalanceOn(CurrentDeviceInfo?.ID.ToString(), value);
                SetProfileProperty(nameof(IsAutoWhiteBalanceOn), value, OperationModule.ColorAndImage);
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsAutoWhiteBalanceOnText));
            }
        }
        public string IsAutoWhiteBalanceOnText
        {
            get => CurrentProfile.IsAutoWhiteBalanceOn ? Strings.On : Strings.Off;
        }

        private int _autoWhiteBalance = -1;
        public int AutoWhiteBalance
        {
            get => _autoWhiteBalance;
            set
            {
                if (value != _autoWhiteBalance)
                {
                    _autoWhiteBalance = value;
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
            if (!isUIHasUpdateByQAM)
                DdpmCommonHelper.DeviceManagerSA?.SetAutoWhiteBalance(CurrentDeviceInfo?.ID.ToString(), _autoWhiteBalance);
            SetProfileProperty(nameof(AutoWhiteBalance), _autoWhiteBalance, OperationModule.ColorAndImage);
        }

        public string BrightnessText { get; set; } = "";
        public double[] BrightnessMargin { get; set; } = { 0 };
        private int _brightness = -1;
        public int Brightness
        {
            get => _brightness;
            set
            {
                try
                {

                    if (value != _brightness)
                    {
                        _brightness = value;
                        if (!IsSliderDragging)
                        {
                            SetBrightness();
                        }
                        OnPropertyChanged();
                        UpdateBrightnessMargin(value);
                    }
                }
                catch (Exception ex)
                {
                    DdpmCommonHelper.WriteUILog("DDPM.UI.Plugin.Common\\ViewModels\\WebCameraViewModel.cs Brightness set ex:" + ex.Message);
                }
            }
        }
        private void UpdateBrightnessMargin(int value)
        {
            //var v = (value * 1.0 - CurrentDeviceInfo.BrightnessMin) / (CurrentDeviceInfo.BrightnessMax - CurrentDeviceInfo.BrightnessMin);
            BrightnessMargin = GetTextmargin(value, CurrentDeviceInfo?.BrightnessMax, CurrentDeviceInfo?.BrightnessMin, out string text);
            BrightnessText = text;
            OnPropertyChanged(nameof(BrightnessText));
            OnPropertyChanged(nameof(BrightnessMargin));
        }
        public void SetBrightness()
        {
            if (!isUIHasUpdateByQAM)
                DdpmCommonHelper.DeviceManagerSA?.SetBrightness(CurrentDeviceInfo?.ID.ToString(), _brightness);
            SetProfileProperty(nameof(Brightness), _brightness, OperationModule.ColorAndImage);
        }

        public string SharpnessText { get; set; } = "";
        public double[] SharpnessMargin { get; set; } = { 0 };
        private int _sharpness = -1;
        public int Sharpness
        {
            get => _sharpness;
            set
            {
                if (value != _sharpness)
                {
                    _sharpness = value;
                    if (!IsSliderDragging)
                    {
                        SetSharpness();
                    }
                    OnPropertyChanged();
                    UpdateSharpnessMargin(value);
                }
            }
        }
        private void UpdateSharpnessMargin(int value)
        {
            SharpnessMargin = GetTextmargin(value, CurrentDeviceInfo?.SharpnessMax, CurrentDeviceInfo?.SharpnessMin, out string text);
            SharpnessText = text;
            OnPropertyChanged(nameof(SharpnessText));
            OnPropertyChanged(nameof(SharpnessMargin));
        }
        public void SetSharpness()
        {
            if (!isUIHasUpdateByQAM)
                DdpmCommonHelper.DeviceManagerSA?.SetSharpness(CurrentDeviceInfo?.ID.ToString(), _sharpness);
            SetProfileProperty(nameof(Sharpness), _sharpness, OperationModule.ColorAndImage);
        }

        public string ContrastText { get; set; } = "";
        public double[] ContrastMargin { get; set; } = { 0 };
        private int _contrast = -1;
        public int Contrast
        {
            get => _contrast;
            set
            {
                if (value != _contrast)
                {
                    _contrast = value;
                    if (!IsSliderDragging)
                    {
                        SetContrast();
                    }
                    OnPropertyChanged();
                    UpdateContrastMargin(value);
                }
            }
        }
        private void UpdateContrastMargin(int value)
        {
            ContrastMargin = GetTextmargin(value, CurrentDeviceInfo?.ContrastMax, CurrentDeviceInfo?.ContrastMin, out string text);
            ContrastText = text;
            OnPropertyChanged(nameof(ContrastText));
            OnPropertyChanged(nameof(ContrastMargin));
        }
        public void SetContrast()
        {
            if (!isUIHasUpdateByQAM)
                DdpmCommonHelper.DeviceManagerSA?.SetContrast(CurrentDeviceInfo?.ID.ToString(), _contrast);
            SetProfileProperty(nameof(Contrast), _contrast, OperationModule.ColorAndImage);
        }

        private int _saturation = -1;
        public int Saturation
        {
            get => _saturation;
            set
            {
                if (value != _saturation)
                {
                    _saturation = value;
                    if (!IsSliderDragging)
                    {
                        SetSaturation();
                    }
                    OnPropertyChanged();
                    UpdateSaturationMargin(value);
                }
            }
        }
        private void UpdateSaturationMargin(int value)
        {
            SaturationMargin = GetTextmargin(value, CurrentDeviceInfo?.SaturationMax, CurrentDeviceInfo?.SaturationMin, out string text);
            SaturationText = text;
            OnPropertyChanged(nameof(SaturationText));
            OnPropertyChanged(nameof(SaturationMargin));
        }

        private static double[] GetTextmargin(double value, double? max, double? min, out string text)
        {
            if (max == null || min == null || max.Value == min.Value)
            {
                text = "0%";
                return new double[] { 0, 2, 0, 0 };
            }

            var v = (value - min.Value) / (max.Value - min.Value);
            text = v.ToString("##0%");
            var width = Utility.GetTextWidth(text, 14);
            var m = 380 * v - width / 2 + 10;
            return new double[] { m, 2, 0, 0 };
        }

        public string SaturationText { get; set; } = "";
        public double[] SaturationMargin { get; set; } = { 0 };
        public void SetSaturation()
        {
            if (!isUIHasUpdateByQAM)
                DdpmCommonHelper.DeviceManagerSA?.SetSaturation(CurrentDeviceInfo?.ID.ToString(), _saturation);
            SetProfileProperty(nameof(Saturation), _saturation, OperationModule.ColorAndImage);
        }

        private int _antiFlicker = 0;
        public int AntiFlicker
        {
            get => _antiFlicker;
            set
            {
                if (value != _antiFlicker)
                {
                    _antiFlicker = value;
                    DdpmCommonHelper.DeviceManagerSA?.SetAntiFlicker(CurrentDeviceInfo?.ID.ToString(), AntiFlicker);
                    SetProfileProperty(nameof(AntiFlicker), AntiFlicker, OperationModule.ColorAndImage);
                    OnPropertyChanged();
                }
            }
        }

        public void SetTilt(int value)
        {
            DdpmCommonHelper.DeviceManagerSA?.SetTilt(CurrentDeviceInfo?.ID.ToString(), value);
            SetProfileProperty("Tilt", value, OperationModule.Other, false);
        }
        public void SetPan(int value)
        {
            DdpmCommonHelper.DeviceManagerSA?.SetPan(CurrentDeviceInfo?.ID.ToString(), value);
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
            get => Zoom != (CurrentDeviceInfo?.ZoomMin ?? 100) && !CurrentProfile.IsAutoFramingOn ? Visibility.Visible : Visibility.Collapsed;
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

        public bool is_AutoFramingVisibility = true;
        public Visibility AutoFramingVisibility
        {
            get => is_AutoFramingVisibility && (CurrentDeviceInfo.IsPropertyAutoFramingSensitivitySupported || CurrentDeviceInfo.IsPropertyAutoFramingSizeSupported || CurrentDeviceInfo.IsPropertyAutoFramingTransitionSupported) ? Visibility.Visible : Visibility.Collapsed;

        }
        public Visibility AutoFramingSensitivityVisibility
        {
            get => CurrentDeviceInfo.IsPropertyAutoFramingSensitivitySupported ? Visibility.Visible : Visibility.Collapsed;
        }
        public Visibility AutoFramingSizeVisibility
        {
            get => CurrentDeviceInfo.IsPropertyAutoFramingSizeSupported ? Visibility.Visible : Visibility.Collapsed;
        }
        public Visibility AutoFramingTransitionVisibility
        {
            get => CurrentDeviceInfo.IsPropertyAutoFramingTransitionSupported ? Visibility.Visible : Visibility.Collapsed;
        }

        public Visibility FOVVisibility
        {
            get => CurrentDeviceInfo.IsPropertyFOVSupported ? Visibility.Visible : Visibility.Collapsed;
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
        public List<ProfileItem> ProfileItems { get => _profileItems; }

        public void PrepareProfileItems()
        {
            //RefreshProfiles();

            try
            {

                ProfileCaptions.Clear();
                _profileItems.Clear();
                foreach (var pofile in WebcamSettings.CustomProfiles.Values)
                {
                    _profileItems.Add(new ProfileItem
                    {
                        ID = pofile.Name,
                        Caption = Utility.CheckTextLength(pofile.Name, 120, 14),
                        Tooltip = "",
                        TooltipVisibility = Visibility.Collapsed,
                        ButtonVisibility = Visibility.Visible
                    });
                    ProfileCaptions.Add(pofile.Name, pofile.Name);
                }
                foreach (var pofile in WebcamSettings.PresetProfiles.Values)
                {
                    ProfileCaptions.Add(pofile.Name, LangHelper.Instance[pofile.Name]);
                }

                _profileItems.Add(new ProfileItem
                {
                    ID = "Default",
                    Caption = LangHelper.Instance["Default"],
                    Tooltip = Strings.DefaultProfileTooltip,
                    TooltipVisibility = Visibility.Visible,
                    ButtonVisibility = Visibility.Collapsed
                });
                _profileItems.Add(new ProfileItem
                {
                    ID = "Smooth",
                    Caption = LangHelper.Instance["Smooth"],
                    Tooltip = Strings.SmoothProfileTooltip,
                    TooltipVisibility = Visibility.Visible,
                    ButtonVisibility = Visibility.Collapsed
                });
                _profileItems.Add(new ProfileItem
                {
                    ID = "Vibrant",
                    Caption = LangHelper.Instance["Vibrant"],
                    Tooltip = Strings.VibrantProfileTooltip,
                    TooltipVisibility = Visibility.Visible,
                    ButtonVisibility = Visibility.Collapsed
                });
                _profileItems.Add(new ProfileItem
                {
                    ID = "Warm",
                    Caption = LangHelper.Instance["Warm"],
                    Tooltip = Strings.WarmProfileTooltip,
                    TooltipVisibility = Visibility.Visible,
                    ButtonVisibility = Visibility.Collapsed
                });
            }
            catch (Exception ex)
            {
                DdpmCommonHelper.WriteUILog("DDPM.UI.Plugin.Common\\ViewModels\\WebCameraViewModel.cs PrepareProfileItems() ex:" + ex.Message);
            }
        }
        public override void OnGoBackClicked()
        {
            if (MediaCapture != null)
            {
                try
                {
                    _ = MediaCapture.StopRecordAsync();

                    //Derek 2025/02/20 for PIMS 338464 Observe WOA occurs during Recording of DDPM (No one in Field of View)
                    RecoverWALSettings();
                }
                catch { }
            }
            base.OnGoBackClicked();
        }

        //Derek 2025/02/20 Save current WAL status and stop it
        private bool bNeedToRecoverWALStatus = false;
        public void SaveWALSettings()
        {
            if (!IsProximitySensorEnable)
            {
                DdpmCommonHelper.WriteUILog($"Don't need to save current WAL status due to IsProximitySensorEnable = {IsProximitySensorEnable}");

                return;
            }

            bNeedToRecoverWALStatus = true;

            if (IsChecked_WalkAwayLock)
            {
                bCurrentWALCheckStatus = IsChecked_WalkAwayLock;
                IsChecked_WalkAwayLock = false;
            }

            if (IsChecked_WakeOnApproach)
            {
                bCurrentWOACheckStatus = IsChecked_WakeOnApproach;
                IsChecked_WakeOnApproach = false;
            }

            if (IsChecked_ProximitySensor)
            {
                bCurrentProximitySensorCheckStatus = IsChecked_ProximitySensor;
                IsChecked_ProximitySensor = false;
            }

            IsProximitySensorEnable = false;  //grey out UI
            DdpmCommonHelper.WriteUILog("Save current WAL status and stop it done");
        }

        //Derek 2025/02/20 recover previous WAL status
        public void RecoverWALSettings()
        {

            if (!bNeedToRecoverWALStatus)
            {
                DdpmCommonHelper.WriteUILog($"Don't need to recover previous WAL status due to bNeedToRecoverWALStatus = {bNeedToRecoverWALStatus}");

                return;
            }

            IsProximitySensorEnable = true;

            if (bCurrentProximitySensorCheckStatus)
            {
                IsChecked_ProximitySensor = bCurrentProximitySensorCheckStatus;
                bCurrentProximitySensorCheckStatus = false;
            }

            if (bCurrentWALCheckStatus)
            {
                IsChecked_WalkAwayLock = bCurrentWALCheckStatus;
                bCurrentWALCheckStatus = false;
            }

            if (bCurrentWOACheckStatus)
            {
                IsChecked_WakeOnApproach = bCurrentWOACheckStatus;
                bCurrentWOACheckStatus = false;
            }

            // Done recovery
            bNeedToRecoverWALStatus = false;
            DdpmCommonHelper.WriteUILog("Recover previous WAL status done");
        }

        public void OnMainWindowClosed()
        {
            DdpmCommonHelper.WriteUILog("Webcam view model receive DDPM main window closing event start");

            if (MediaCapture != null)
            {
                try
                {
                    MediaCapture.StopRecordAsync().Wait();
                    RecoverWALSettings();
                }
                catch (Exception e)
                {
                    DdpmCommonHelper.WriteUILog($"Catch exception[{e.Message}] when try to RecoverWALSettings");
                }
            }

            DdpmCommonHelper.WriteUILog("Webcam view model receive DDPM main window closing event end");
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
            try
            {
                if (!IsSettingProfile)
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
                        OnPropertyChanged(nameof(UndoVisibility));
                        OnPropertyChanged(nameof(Undo2Visibility));
                        OnPropertyChanged(nameof(RedoVisibility));
                        OnPropertyChanged(nameof(Redo2Visibility));
                    }
                    switch (propertyName)
                    {
                        // << 250314 add by Hess for V2.0.2 new requirement
                        case nameof(Priority):
                        case nameof(IsFocusOn):
                        case nameof(Zoom):
                        case nameof(AntiFlicker):
                        case nameof(AutoFramingSensitivity):
                        case nameof(AutoFramingFrameSize):
                        case nameof(IsAutoFramingTransitionOn):
                        case "Pan":
                        case "Tilt":
                        // >>
                        case nameof(IsAutoFramingOn):
                        case nameof(FieldOfView):
                        case nameof(IsHDROn):
                        case nameof(IsAutoWhiteBalanceOn):
                        case nameof(Brightness):
                        case nameof(Contrast):
                        case nameof(Saturation):
                        case nameof(Sharpness):
                            ProfilePropertyChanged?.Invoke(this, EventArgs.Empty);
                            WebcamSettings.NONE = CurrentProfile;
                            CurrentProfileName = "NONE";
                            if (propertyName == "IsHDROn")
                                WebcamSettingChanged?.Invoke(this, EventArgs.Empty);
                            break;
                        default:
                            break;
                    }
                    propertyInfo.SetValue(CurrentProfile, convertedValue);
                    WebcamSettings.NONE = CurrentProfile;
                    WebcamSettings.ExportWebcamSettings(WebcamSettings, Model, DdpmCommonHelper.DeviceManagerSA, DdpmCommonHelper.Log);
                }
            }
            catch (Exception ex)
            {
                DdpmCommonHelper.WriteUILog("DDPM.UI.Plugin.Common\\ViewModels\\WebCameraViewModel.cs SetProfileProperty() ex:" + ex.Message);
            }
        }

        public void ClearUndo()
        {
            try
            {
                WCOperations.Clear();
                OPIndex = -1;
                OnPropertyChanged(nameof(UndoVisibility));
                OnPropertyChanged(nameof(Undo2Visibility));
                OnPropertyChanged(nameof(RedoVisibility));
                OnPropertyChanged(nameof(Redo2Visibility));
            }
            catch (Exception ex)
            {
                DdpmCommonHelper.WriteUILog("DDPM.UI.Plugin.Common\\ViewModels\\WebCameraViewModel.cs ClearUndo() ex:" + ex.Message);
            }

        }

        public void Undo()
        {
            try
            {
                var op = WCOperations[OPIndex];
                VbarSelectedIndex = (int)op.OPModule;
                SelectVBar();
                OPIndex -= 1;
                var type = CurrentProfile.GetType();
                var propertyInfo = type.GetProperty(op.Property);
                object convertedValue = Convert.ChangeType(op.OldValue, propertyInfo!.PropertyType);
                propertyInfo.SetValue(CurrentProfile, convertedValue);
                UpdateProperty(op.Property, convertedValue);
            }
            catch (Exception ex)
            {
                DdpmCommonHelper.WriteUILog("DDPM.UI.Plugin.Common\\ViewModels\\WebCameraViewModel.cs Undo() ex:" + ex.Message);
            }
        }
        public void Redo()
        {
            try
            {
                var op = WCOperations[OPIndex + 1];
                VbarSelectedIndex = (int)op.OPModule;
                SelectVBar();
                OPIndex += 1;
                var type = CurrentProfile.GetType();
                var propertyInfo = type.GetProperty(op.Property);
                object convertedValue = Convert.ChangeType(op.NewValue, propertyInfo!.PropertyType);
                propertyInfo.SetValue(CurrentProfile, convertedValue);
                UpdateProperty(op.Property, convertedValue);
            }
            catch (Exception ex)
            {
                DdpmCommonHelper.WriteUILog("DDPM.UI.Plugin.Common\\ViewModels\\WebCameraViewModel.cs Redo() ex:" + ex.Message);
            }
        }

        private void UpdateProperty(string property, object value)
        {
            try
            {
                WebcamSettings.NONE = CurrentProfile;
                WebcamSettings.ExportWebcamSettings(WebcamSettings, Model, DdpmCommonHelper.DeviceManagerSA, DdpmCommonHelper.Log);
                switch (property)
                {
                    case "IsFocusOn":
                        DdpmCommonHelper.DeviceManagerSA?.SetIsFocusOn(CurrentDeviceInfo?.ID.ToString(), (bool)value);
                        _isFocusOn = (bool)value;
                        OnPropertyChanged(nameof(IsFocusOnText));
                        break;
                    case "IsHDROn":
                        AlertType = WebcamAlert.Alert1;
                        AlertVisibility = Visibility.Visible;
                        hdr_change = true;
                        is_hdr_enable = false;
                        OnPropertyChanged(nameof(hdr_enable));
                        DdpmCommonHelper.DeviceManagerSA?.SetIsHDROn(CurrentDeviceInfo?.ID.ToString(), (bool)value);
                        WebcamSettingChanged?.Invoke(this, EventArgs.Empty);
                        //new Thread(() =>
                        //{
                        //    Thread.Sleep(3000);
                        //    AlertVisibility = Visibility.Collapsed;

                        //    //is_hdr_enable = true;
                        //    is_hdr_enable = usb_hdr_enable;
                        //    OnPropertyChanged(nameof(hdr_enable));

                        //}).Start();
                        Task.Run(async () =>
                        {
                            await Task.Delay(3000);
                            AlertVisibility = Visibility.Collapsed;

                            //is_hdr_enable = true;
                            is_hdr_enable = usb_hdr_enable;
                            OnPropertyChanged(nameof(hdr_enable));
                        });
                        OnPropertyChanged(nameof(IsHDROnText));
                        break;
                    case "Focus":
                        DdpmCommonHelper.DeviceManagerSA?.SetFocus(CurrentDeviceInfo?.ID.ToString(), (int)value);
                        _focus = (int)value;
                        OnPropertyChanged(nameof(Focus));
                        break;
                    case "Priority":
                        DdpmCommonHelper.DeviceManagerSA?.SetPriority(CurrentDeviceInfo?.ID.ToString(), (int)value);
                        _priority = (int)value;
                        OnPropertyChanged(nameof(Priority));
                        break;
                    case "Zoom":
                        DdpmCommonHelper.DeviceManagerSA?.SetZoom(CurrentDeviceInfo?.ID.ToString(), (int)value);
                        _zoom = (int)value;
                        OnPropertyChanged(nameof(Zoom));
                        OnPropertyChanged(nameof(PanArrowVisibility));
                        break;
                    case "Brightness":
                        DdpmCommonHelper.DeviceManagerSA?.SetBrightness(CurrentDeviceInfo?.ID.ToString(), (int)value);
                        _brightness = (int)value;
                        UpdateBrightnessMargin(_brightness);
                        break;
                    case "Contrast":
                        DdpmCommonHelper.DeviceManagerSA?.SetContrast(CurrentDeviceInfo?.ID.ToString(), (int)value);
                        _contrast = (int)value;
                        UpdateContrastMargin(_contrast);
                        break;
                    case "AntiFlicker":
                        DdpmCommonHelper.DeviceManagerSA?.SetAntiFlicker(CurrentDeviceInfo?.ID.ToString(), (int)value);
                        _antiFlicker = (int)value;
                        OnPropertyChanged(nameof(AntiFlicker));
                        break;
                    case "Saturation":
                        DdpmCommonHelper.DeviceManagerSA?.SetSaturation(CurrentDeviceInfo?.ID.ToString(), (int)value);
                        _saturation = (int)value;
                        UpdateSaturationMargin(_saturation);
                        break;
                    case "Sharpness":
                        DdpmCommonHelper.DeviceManagerSA?.SetSharpness(CurrentDeviceInfo?.ID.ToString(), (int)value);
                        _sharpness = (int)value;
                        UpdateSharpnessMargin(_sharpness);
                        break;
                    case "IsAutoWhiteBalanceOn":
                        DdpmCommonHelper.DeviceManagerSA?.SetIsAutoWhiteBalanceOn(CurrentDeviceInfo?.ID.ToString(), (bool)value);
                        OnPropertyChanged(nameof(IsAutoWhiteBalanceOnText));
                        break;
                    case "AutoWhiteBalance":
                        DdpmCommonHelper.DeviceManagerSA?.SetAutoWhiteBalance(CurrentDeviceInfo?.ID.ToString(), (int)value);
                        _autoWhiteBalance = (int)value;
                        OnPropertyChanged(nameof(AutoWhiteBalance));
                        break;
                    case "IsAutoFramingOn":
                        DdpmCommonHelper.DeviceManagerSA?.SetIsAutoFramingOn(CurrentDeviceInfo?.ID.ToString(), (bool)value);
                        _isAutoFramingOn = (bool)value;
                        SetAutoFraming();
                        break;
                    case "IsAutoFramingTransitionOn":
                        DdpmCommonHelper.DeviceManagerSA?.SetIsAutoFramingTransitionOn(CurrentDeviceInfo?.ID.ToString(), (bool)value);
                        _isAutoFramingTransitionOn = (bool)value;
                        OnPropertyChanged(nameof(IsAutoFramingTransitionOnText));
                        break;
                    case "AutoFramingSensitivity":
                        DdpmCommonHelper.DeviceManagerSA?.SetAutoFramingSensitivity(CurrentDeviceInfo?.ID.ToString(), (int)value);
                        _autoFramingSensitivity = (int)value;
                        OnPropertyChanged(nameof(AutoFramingSensitivity));
                        break;
                    case "AutoFramingFrameSize":
                        DdpmCommonHelper.DeviceManagerSA?.SetAutoFramingFrameSize(CurrentDeviceInfo?.ID.ToString(), (int)value);
                        _autoFramingFrameSize = (int)value;
                        OnPropertyChanged(nameof(AutoFramingFrameSize));
                        break;
                    case "FieldOfView":
                        DdpmCommonHelper.DeviceManagerSA?.SetFieldOfView(CurrentDeviceInfo?.ID.ToString(), (int)value);
                        _fieldOfView = (int)value;
                        if (_fOVs[0] == _fieldOfView)
                            SetFOV_Selected(0, true);
                        else if (_fOVs[1] == _fieldOfView)
                            SetFOV_Selected(1, true);
                        else
                            SetFOV_Selected(2, true);
                        OnPropertyChanged(nameof(FieldOfView));
                        break;
                }
                OnPropertyChanged(property);

                OnPropertyChanged(nameof(UndoVisibility));
                OnPropertyChanged(nameof(Undo2Visibility));
                OnPropertyChanged(nameof(RedoVisibility));
                OnPropertyChanged(nameof(Redo2Visibility));
            }
            catch (Exception ex)
            {
                DdpmCommonHelper.WriteUILog("DDPM.UI.Plugin.Common\\ViewModels\\WebCameraViewModel.cs UpdateProperty ex:" + ex.Message);
            }

        }

        private Visibility _tooltipVisibility = Visibility.Collapsed;
        public Visibility TooltipVisibility
        {
            get => _tooltipVisibility;
            set
            {
                _tooltipVisibility = value;
                OnPropertyChanged();
                MessageBoxVisibility = value;
                OnPropertyChanged(nameof(MessageBoxVisibility));
            }
        }
        public Visibility MessageBoxVisibility { get; set; } = Visibility.Collapsed;
        public Visibility MessageBoxVisibilityUsbType { get; set; } = Visibility.Collapsed;

        public string usbtype_info_v = LangHelper.Instance["Camera.25"];

        public string usbtype_info { get => usbtype_info_v; }

        public bool running_state = true;
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
                if (_properties is VideoEncodingProperties &&
                    (_properties as VideoEncodingProperties).FrameRate.Denominator != 0)
                {
                    return (_properties as VideoEncodingProperties).FrameRate.Numerator / (_properties as VideoEncodingProperties).FrameRate.Denominator;
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

            return "";
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