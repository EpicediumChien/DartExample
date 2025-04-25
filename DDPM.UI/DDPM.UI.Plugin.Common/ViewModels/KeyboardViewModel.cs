using CommunityToolkit.Mvvm.Input;
using DDPM.SA.Common;
using DDPM.UI.Common;
using Dell.Client.Framework.Common;
using Dell.Client.Framework.UX.WPF;
using Microsoft;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using System.Text;
using DDPM.UI.Resources.Helper;
using System.Diagnostics;
using Microsoft.Toolkit.Uwp.Notifications;

namespace DDPM.UI.Plugin.ViewModels
{
    public class KeyboardViewModel : PeripheralViewModel, INotifyPropertyChanged
    {
        #region Variables

        private readonly ILog _log;
        private bool _tabOffFocused = false;
        private bool _tabAdaptiveLightFocused = false;
        private bool _tabManualFocused = false;
        private bool _isSliderVisible = false;
        private int _backLightingLevel = 0;
        //private string _backLightingLevelText = "0%";
        private bool _isCollaborationKeyEnable = false;
        private bool _isCollaborationCameraEnable = false;
        private bool _isCollaborationScreenShareEnable = false;
        private bool _isCollaborationChatEnable = false;
        private bool _isCollaborationMicEnable = false;
        private bool _isCollaborationBlinkEffectEnable = false;
        private bool _isCollaborationDoubleTapEnable = false;
        private bool _cameraToggleEnabled = false;
        private bool _screenShareToggleEnabled = false;
        private bool _chatToggleEnabled = false;
        private bool _micToggleEnabled = false;
        private bool _blinkEffectToggleEnabled = false;
        private bool _doubleTapToggleEnabled = false;
        private bool _isCollabShadowVisible = false;
        private string _isCollaborationKeyEnableText;
        private string _isCollaborationCameraEnableText;
        private string _isCollaborationScreenShareEnableText;
        private string _isCollaborationChatEnableText;
        private string _isCollaborationMicEnableText;
        private Visibility _isAllKeysVisible;

        private int IlluminationSelectedTabIndex = 0;

        #endregion Variables

        public new event PropertyChangedEventHandler? PropertyChanged;

        public ICommand TabOffClickedCommand { get; }
        public ICommand TabAdaptiveLightClickedCommand { get; }
        public ICommand TabManualClickedCommand { get; }

        public KeyboardViewModel(IConsole console, ILog log) : base(console, log, DdpmCommonHelper.DeviceManagerSA)
        {
            Requires.NotNull(console, nameof(console));
            Requires.NotNull(log, nameof(log));

            _log = log;

            TabOffClickedCommand = new RelayCommand(OnTabOffClicked);
            TabAdaptiveLightClickedCommand = new RelayCommand(OnTabAdaptiveLightClicked);
            TabManualClickedCommand = new RelayCommand(OnTabManualClicked);

            _isCollaborationKeyEnableText = Strings.Off;
            _isCollaborationCameraEnableText = Strings.Off;
            _isCollaborationScreenShareEnableText = Strings.Off;
            _isCollaborationChatEnableText = Strings.Off;
            _isCollaborationMicEnableText = Strings.Off;
            _log!.Info($"[KeyboardViewModel] KeyboardViewModel Start ...");
        }

        private void OnTabOffClicked()
        {
            if (IlluminationSelectedTabIndex == 0)
            { return; }
            IlluminationSelectedTabIndex = 0;
            SwitchTab(0, true);
        }

        private void OnTabAdaptiveLightClicked()
        {
            if (IlluminationSelectedTabIndex == 1)
            { return; }
            IlluminationSelectedTabIndex = 1;
            SwitchTab(1, true);
        }

        private void OnTabManualClicked()
        {
            if (IlluminationSelectedTabIndex == 2)
            { return; }
            IlluminationSelectedTabIndex = 2;
            SwitchTab(2, true);
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
                if ((deviceInfo.LogicalDeviceType.Contains("Keyboard") || EOLKBList.Contains(deviceInfo.ModelNumber)) && !DeviceInfos.ContainsKey(deviceInfo.ID))
                    DeviceInfos.Add(deviceInfo.ID, deviceInfo);
            }
        }

        public override bool SetCurrentDevice(string instanceIDs)
        {
            if (!base.SetCurrentDevice(instanceIDs))
                return false;

            if (!IsCopilotEnabled)
            {
                DdpmCommonHelper.WriteUILog($"[KeyboardViewModel] RemoveCopilotAction() : {IsCopilotEnabled}");
                RemoveCopilotAction();
            }

            try
            {
                if (CurrentDeviceInfo == null)
                {
                    DdpmCommonHelper.WriteUILog("[KeyboardViewModel] CurrentDeviceInfo is null.");
                    return false;
                }

                DdpmCommonHelper.WriteUILog($"[KeyboardViewModel] CurrentDeviceInfo : {JsonConvert.SerializeObject(CurrentDeviceInfo)}");

                if (DdpmCommonHelper.DeviceManagerSA == null)
                {
                    DdpmCommonHelper.WriteUILog("[KeyboardViewModel] DeviceManagerSA is null.");
                    return false;
                }

                if (instanceIDs == null)
                {
                    DdpmCommonHelper.WriteUILog("[KeyboardViewModel] deviceID is null.");
                    return false;
                }

                // << 250419 by Hess. removed unused code to improve performance
                //Task<JArray> task1 = DdpmCommonHelper.DeviceManagerSA.GetKbProgrammableKeys(instanceIDs.ToString());
                //var jArray = JArray.FromObject(task1.Result);
                //DdpmCommonHelper.WriteUILog($"[KeyboardViewModel] CurrentDeviceInfo : {JsonConvert.SerializeObject(jArray)}");
                // >>
            }
            catch (Exception ex)
            {
                DdpmCommonHelper.WriteUILog($"[KeyboardViewModel] Exception : {ex}");
            }

            if (CurrentDeviceInfo == null)
                return false;

            InitializeKey();
            if (CurrentDeviceInfo.IsCollabsKeysSupported)
            {
                IsCollaborationDoubleTapEnable = CurrentDeviceInfo.IsCollaborationDoubleTapEnable;
                IsCollaborationKeyEnable = CurrentDeviceInfo.IsCollaborationKeyEnable;
                IsCollabShadowVisible = !IsCollaborationKeyEnable;
                IsCollaborationCameraEnable = CurrentDeviceInfo.IsCollaborationCameraEnable;
                IsCollaborationScreenShareEnable = CurrentDeviceInfo.IsCollaborationScreenShareEnable;
                IsCollaborationChatEnable = CurrentDeviceInfo.IsCollaborationChatEnable;
                IsCollaborationMicEnable = CurrentDeviceInfo.IsCollaborationMicEnable;
                IsCollaborationBlinkEffectEnable = CurrentDeviceInfo.IsCollaborationBlinkEffectEnable;
                //if (KeyboardAction.IsCollaborationChecked)
                //{
                //    IsCollaborationDoubleTapEnable = CurrentDeviceInfo.IsCollaborationDoubleTapEnable;
                //    IsCollaborationKeyEnable = CurrentDeviceInfo.IsCollaborationKeyEnable;
                //    IsCollabShadowVisible = !IsCollaborationKeyEnable;
                //    IsCollaborationCameraEnable = CurrentDeviceInfo.IsCollaborationCameraEnable;
                //    IsCollaborationScreenShareEnable = CurrentDeviceInfo.IsCollaborationScreenShareEnable;
                //    IsCollaborationChatEnable = CurrentDeviceInfo.IsCollaborationChatEnable;
                //    IsCollaborationMicEnable = CurrentDeviceInfo.IsCollaborationMicEnable;
                //    IsCollaborationBlinkEffectEnable = CurrentDeviceInfo.IsCollaborationBlinkEffectEnable;
                //}
                //else
                //{
                //    IsCollaborationDoubleTapEnable = true;
                //    IsCollaborationKeyEnable = true;
                //    IsCollabShadowVisible = false;
                //    IsCollaborationCameraEnable = true;
                //    IsCollaborationScreenShareEnable = true;
                //    IsCollaborationChatEnable = true;
                //    IsCollaborationMicEnable = true;
                //    IsCollaborationBlinkEffectEnable = true;
                //    IsCollaborationBlinkEffectEnable = false;
                //    IsCollaborationDoubleTapEnable = true;
                //    IsCollaborationDoubleTapEnable = false;
                //    KeyboardAction.IsCollaborationChecked = true;
                //    ActionList.ExportActionList(KeyboardAction, Model);
                //}
            }

            if (CurrentDeviceInfo.IsIlluminationSupported)
            {
                TabOffCaption = LangHelper.Instance["Off2"];
                //TabOffInfoTip = Resources.Resources.Keyboard_Illumination_Off_ToolTip;
                TabOffInfoTip = LangHelper.Instance["Illumination.2"];
                TabAdaptiveLightCaption = Strings.AdaptiveLight;
                TabAdaptiveLightInfoTip = LangHelper.Instance["Illumination.3"];
                TabManualCaption = Strings.Manual;
                TabManualInfoTip = LangHelper.Instance["Illumination.4"];

                IlluminationSelectedTabIndex = CurrentDeviceInfo.BackLightingControls switch
                {
                    6 => 1,
                    3 => 2,
                    _ => 0
                };
                BackLightingLevel = CurrentDeviceInfo.BackLightingLevel;
                SwitchTab(IlluminationSelectedTabIndex);
            }
            if (Model == "KB500" || Model == "KB700" || Model == "KB740")
                CopilotInfoVisibility = Visibility.Visible;
            else
                CopilotInfoVisibility = Visibility.Collapsed;
            // << 250220 update by Hess handeld by PeripheralViewmodel
            ////Elsa 20250122 add for PIMS-332048 All langs tooltip EN in UI issue found in stringID：FirmwareVersion
            //FirmwareVersion2 = FirmwareVersion2.Replace("Firmware Version", Strings.FirmwareVersion);
            // >>
            return true;
        }

        public void RemoveCopilotAction()
        {
            var key = SelectedKey;
            foreach (var ka in KeyboardAction.KeyActions)
            {
                if (ka.Value.AssignedAction.ID == 1)
                {
                    SelectedKey = ka.Key.ToString();
                    UpdateAction(ka.Value.DefaultActionID, "", false);
                }
            }
            SelectedKey = key;
            InitializeKey();
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
                    else if (di.ModelNumber == Model)
                    {
                        switch (property)
                        {
                            case "RestoreToDefault":
                                ResetAction();
                                break;
                        }
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
                            case "BackLightingControlsChanged":
                                if (di.BackLightTabIndex != IlluminationSelectedTabIndex)
                                    SwitchTab(di.BackLightTabIndex);
                                break;

                            case "BackLightingLevelChanged":
                                //if (di.BackLightingLevel == BackLightingLevel)
                                //    return;
                                Debug.WriteLine($"BackLightingLevelChanged: value:{di.BackLightingLevel}.....................{DateTime.Now:HH:mm:ss.ff}");
                                _backLightingLevel = di.BackLightingLevel;
                                CurrentDeviceInfo.BackLightingLevel = di.BackLightingLevel;
                                OnPropertyChanged(nameof(BackLightingLevel));
                                OnPropertyChanged(nameof(BackLightingLevelText));
                                break;
                            case "CollaborationScreenShareEnable":
                                _isCollaborationScreenShareEnable = di.IsCollaborationScreenShareEnable;
                                OnPropertyChanged(nameof(IsCollaborationScreenShareEnable));
                                IsCollaborationScreenShareEnableText = _isCollaborationScreenShareEnable ? Strings.On : Strings.Off;
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

        private void SwitchTab(int index, bool NeedSetting = false)
        {
            int value = 0;
            switch (index)
            {
                case 0:
                    TabOffFocused = true;
                    TabAdaptiveLightFocused = false;
                    TabManualFocused = false;
                    IsSliderVisible = false;
                    value = 1;
                    // << 250107 updated by Hess to fix PIMS-340197
                    //if (NeedSetting)
                    //    BackLightingLevel = 0;
                    // >>
                    break;

                case 1:
                    TabOffFocused = false;
                    TabAdaptiveLightFocused = true;
                    TabManualFocused = false;
                    IsSliderVisible = false;
                    value = 6;
                    break;

                case 2:
                    TabOffFocused = false;
                    TabAdaptiveLightFocused = false;
                    TabManualFocused = true;
                    IsSliderVisible = true;
                    value = 3;
                    break;
            }
            if (NeedSetting && CurrentDeviceInfo != null)
                DdpmCommonHelper.DeviceManagerSA?.SetBackLightingControls(value, CurrentDeviceInfo.ID);

            IlluminationSelectedTabIndex = index;
        }

        public string TabOffCaption { get; set; } = "";
        public string TabOffInfoTip { get; set; } = "";
        public string TabAdaptiveLightCaption { get; set; } = "";
        public string TabAdaptiveLightInfoTip { get; set; } = "";
        public string TabManualCaption { get; set; } = "";
        public string TabManualInfoTip { get; set; } = "";

        public bool TabOffFocused
        {
            get => _tabOffFocused;
            set
            {
                if (_tabOffFocused != value)
                {
                    _tabOffFocused = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool TabAdaptiveLightFocused
        {
            get => _tabAdaptiveLightFocused;
            set
            {
                if (_tabAdaptiveLightFocused != value)
                {
                    _tabAdaptiveLightFocused = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool TabManualFocused
        {
            get => _tabManualFocused;
            set
            {
                if (_tabManualFocused != value)
                {
                    _tabManualFocused = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsSliderVisible
        {
            get => _isSliderVisible;
            set
            {
                if (_isSliderVisible != value)
                {
                    _isSliderVisible = value;
                    OnPropertyChanged();
                }
            }
        }

        public int BackLightingLevel
        {
            get => _backLightingLevel;
            set
            {
                //BackLightingLevelText = _backLightingLevel == 0 ? "0%" : (_backLightingLevel == 1 ? "25%" : (_backLightingLevel == 2 ? "50%" : (_backLightingLevel == 3 ? "75%" : "100%")));
                if (_backLightingLevel != value)
                {
                    _backLightingLevel = value;
                    if (!IsSliderDragging)
                        SetDBackLightingLevel();

                    OnPropertyChanged();
                }
                OnPropertyChanged(nameof(BackLightingLevelText));
            }
        }

        public string BackLightingLevelText
        {
            get => _backLightingLevel == 0 ? "0%" : (_backLightingLevel == 1 ? "25%" : (_backLightingLevel == 2 ? "50%" : (_backLightingLevel == 3 ? "75%" : "100%")));
            //set
            //{
            //    if (_backLightingLevelText != value)
            //    {
            //        _backLightingLevelText = value;
            //        OnPropertyChanged();
            //    }
            //}
        }

        public void SetDBackLightingLevel()
        {
            if (CurrentDeviceInfo == null)
                return;

            if (_backLightingLevel != CurrentDeviceInfo.BackLightingLevel)
            {
                DdpmCommonHelper.DeviceManagerSA?.SetBackLightingLevel(_backLightingLevel, CurrentDeviceInfo.ID);
                Debug.WriteLine($"Set BackLightingLevel: value:{_backLightingLevel}.....................{DateTime.Now:HH:mm:ss.ff}");
            }

            OnPropertyChanged(nameof(BackLightingLevel));
            OnPropertyChanged(nameof(BackLightingLevelText));
        }

        public bool IsCollaborationKeyEnable
        {
            get => _isCollaborationKeyEnable;
            set
            {
                if (_isCollaborationKeyEnable != value)
                {
                    _isCollaborationKeyEnable = value;
                    EnableCollaborationKey(value);
                    OnPropertyChanged();
                    if (CurrentDeviceInfo != null)
                        DdpmCommonHelper.DeviceManagerSA?.SetCollaborationKeyEnable(value, CurrentDeviceInfo.ID);
                }
            }
        }

        public string IsCollaborationKeyEnableText
        {
            get => _isCollaborationKeyEnableText;
            set
            {
                if (_isCollaborationKeyEnableText != value)
                {
                    _isCollaborationKeyEnableText = value;
                    OnPropertyChanged();
                }
            }
        }

        public string IsCollaborationCameraEnableText
        {
            get => _isCollaborationCameraEnableText;
            set
            {
                if (_isCollaborationCameraEnableText != value)
                {
                    _isCollaborationCameraEnableText = value;
                    OnPropertyChanged();
                }
            }
        }

        public string IsCollaborationScreenShareEnableText
        {
            get => _isCollaborationScreenShareEnableText;
            set
            {
                if (_isCollaborationScreenShareEnableText != value)
                {
                    _isCollaborationScreenShareEnableText = value;
                    OnPropertyChanged();
                }
            }
        }

        public string IsCollaborationChatEnableText
        {
            get => _isCollaborationChatEnableText;
            set
            {
                if (_isCollaborationChatEnableText != value)
                {
                    _isCollaborationChatEnableText = value;
                    OnPropertyChanged();
                }
            }
        }

        public string IsCollaborationMicEnableText
        {
            get => _isCollaborationMicEnableText;
            set
            {
                if (_isCollaborationMicEnableText != value)
                {
                    _isCollaborationMicEnableText = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsCollaborationCameraEnable
        {
            get => _isCollaborationCameraEnable;
            set
            {
                if (_isCollaborationCameraEnable != value)
                {
                    _isCollaborationCameraEnable = value;
                    IsCollaborationCameraEnableText = value ? Strings.On : Strings.Off;
                    OnPropertyChanged();
                    if (CurrentDeviceInfo != null)
                        DdpmCommonHelper.DeviceManagerSA?.SetCollaborationCameraEnable(value, CurrentDeviceInfo.ID);
                }
            }
        }

        public bool IsCollaborationScreenShareEnable
        {
            get => _isCollaborationScreenShareEnable;
            set
            {
                if (_isCollaborationScreenShareEnable != value)
                {
                    _isCollaborationScreenShareEnable = value;
                    IsCollaborationScreenShareEnableText = value ? Strings.On : Strings.Off;
                    OnPropertyChanged();
                    if (CurrentDeviceInfo != null)
                        DdpmCommonHelper.DeviceManagerSA?.SetCollaborationScreenShareEnable(value, CurrentDeviceInfo.ID);
                }
            }
        }

        public bool IsCollaborationChatEnable
        {
            get => _isCollaborationChatEnable;
            set
            {
                if (_isCollaborationChatEnable != value)
                {
                    _isCollaborationChatEnable = value;
                    IsCollaborationChatEnableText = value ? Strings.On : Strings.Off;
                    OnPropertyChanged();
                    if (CurrentDeviceInfo != null)
                        DdpmCommonHelper.DeviceManagerSA?.SetCollaborationChatEnable(value, CurrentDeviceInfo.ID);
                }
            }
        }

        public bool IsCollaborationMicEnable
        {
            get => _isCollaborationMicEnable;
            set
            {
                if (_isCollaborationMicEnable != value)
                {
                    _isCollaborationMicEnable = value;
                    IsCollaborationMicEnableText = value ? Strings.On : Strings.Off;
                    OnPropertyChanged();
                    if (CurrentDeviceInfo != null)
                        DdpmCommonHelper.DeviceManagerSA?.SetCollaborationMicEnable(value, CurrentDeviceInfo.ID);
                }
            }
        }

        public bool IsCollaborationBlinkEffectEnable
        {
            get => _isCollaborationBlinkEffectEnable;
            set
            {
                if (_isCollaborationBlinkEffectEnable != value)
                {
                    _isCollaborationBlinkEffectEnable = value;
                    OnPropertyChanged();
                    if (CurrentDeviceInfo != null)
                        DdpmCommonHelper.DeviceManagerSA?.SetCollaborationBlinkEffectEnable(value, CurrentDeviceInfo.ID);
                }
            }
        }

        public bool IsCollaborationDoubleTapEnable
        {
            get => _isCollaborationDoubleTapEnable;
            set
            {
                if (_isCollaborationDoubleTapEnable != value)
                {
                    _isCollaborationDoubleTapEnable = value;
                    OnPropertyChanged();
                    if (CurrentDeviceInfo != null)
                        DdpmCommonHelper.DeviceManagerSA?.SetCollaborationDoubleTapEnable(value, CurrentDeviceInfo.ID);
                }
            }
        }

        public bool CameraToggleEnabled
        {
            get => _cameraToggleEnabled;
            set
            {
                if (_cameraToggleEnabled != value)
                {
                    _cameraToggleEnabled = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool ScreenShareToggleEnabled
        {
            get => _screenShareToggleEnabled;
            set
            {
                if (_screenShareToggleEnabled != value)
                {
                    _screenShareToggleEnabled = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool ChatToggleEnabled
        {
            get => _chatToggleEnabled;
            set
            {
                if (_chatToggleEnabled != value)
                {
                    _chatToggleEnabled = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool MicToggleEnabled
        {
            get => _micToggleEnabled;
            set
            {
                if (_micToggleEnabled != value)
                {
                    _micToggleEnabled = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool BlinkEffectToggleEnabled
        {
            get => _blinkEffectToggleEnabled;
            set
            {
                if (_blinkEffectToggleEnabled != value)
                {
                    _blinkEffectToggleEnabled = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool DoubleTapToggleEnabled
        {
            get => _doubleTapToggleEnabled;
            set
            {
                if (_doubleTapToggleEnabled != value)
                {
                    _doubleTapToggleEnabled = value;
                    OnPropertyChanged();
                }
            }
        }

        private void EnableCollaborationKey(bool enable)
        {
            IsCollabShadowVisible = !enable;
            CameraToggleEnabled = enable;
            ScreenShareToggleEnabled = enable;
            ChatToggleEnabled = enable;
            MicToggleEnabled = enable;
            BlinkEffectToggleEnabled = enable;
            DoubleTapToggleEnabled = enable;
            IsCollaborationKeyEnableText = enable ? Strings.On : Strings.Off;
        }

        public bool IsCollabShadowVisible
        {
            get => _isCollabShadowVisible;
            set
            {
                if (_isCollabShadowVisible != value)
                {
                    _isCollabShadowVisible = value;
                    OnPropertyChanged();
                }
            }
        }

        public KeyboardActions KeyboardAction = new();

        private void InitializeKey()
        {
            //Model = "KB3121W";
            //Model = "KB500";
            //Model = "KB700";
            //Model = "KB900";
            //Model = "KB7221W";
            //Model = "KB740";
            //Model = "KB7120W";
            //Model = "KB7221W";
            //Model = "KM714";
            //ImageFilePath = $"/DDPM.UI.Resources;component/Resources/Images/{Model}.png";

            //KeyboardAction = (KeyboardActions)ActionList.ImportActionList(eDeviceCategory.KB, Model);
            KeyboardAction = (KeyboardActions)ActionList.ImportActionList(eDeviceCategory.KB, Model, CurrentDeviceID.ToString());

            //foreach(var keyAction in KeyboardActions.KeyActions.Values) {
            //  keyAction.AssignedAction = new AssignedAction(keyAction.DefaultActionID + 1);
            //}

            KeyboardAction.KeyActions.Keys.ToList().ForEach(x => RefreshKeyImageFile(x.ToString()));
            CheckRestoreStatus();
            OnPropertyChanged(nameof(IsRestoreEnable));

            OnPropertyChanged(nameof(IsF8Visible));
            OnPropertyChanged(nameof(IsPrtScVisible));
            OnPropertyChanged(nameof(IsScrollLockVisible));
            OnPropertyChanged(nameof(IsPauseBreakVisible));
            OnPropertyChanged(nameof(IsHomeVisible));
            OnPropertyChanged(nameof(IsEndVisible));
            OnPropertyChanged(nameof(IsPgUpVisible));
            OnPropertyChanged(nameof(IsPgDownVisible));
            OnPropertyChanged(nameof(IsAllKeysVisible));
            OnPropertyChanged(nameof(IsRestoreEnable));

            //ActionList.ExportActionList(KeyboardActions, Model, CurrentInstanceID);
        }

        public bool IsF8Visible
        {
            get
            {
                if (IsAllKeysVisible == Visibility.Hidden)
                    return false;
                else
                    return KeyboardAction.KeyActions.ContainsKey(KeyName.F8);
            }
            set
            {
                OnPropertyChanged();
            }
        }

        public bool IsPrtScVisible
        {
            get
            {
                if (IsAllKeysVisible == Visibility.Hidden)
                    return false;
                else
                    return KeyboardAction.KeyActions.ContainsKey(KeyName.PrtSc);
            }
            set
            {
                OnPropertyChanged();
            }
        }

        public bool IsScrollLockVisible
        {
            get
            {
                if (IsAllKeysVisible == Visibility.Hidden)
                    return false;
                else
                    return KeyboardAction.KeyActions.ContainsKey(KeyName.ScrollLock);
            }
            set
            {
                OnPropertyChanged();
            }
        }

        public bool IsPauseBreakVisible
        {
            get
            {
                if (IsAllKeysVisible == Visibility.Hidden)
                    return false;
                else
                    return KeyboardAction.KeyActions.ContainsKey(KeyName.PauseBreak);
            }
            set
            {
                OnPropertyChanged();
            }
        }

        public bool IsCalculatorVisible
        {
            get
            {
                if (IsAllKeysVisible == Visibility.Hidden)
                    return false;
                else
                    return KeyboardAction.KeyActions.ContainsKey(KeyName.Calculator);
            }
            set
            {
                OnPropertyChanged();
            }
        }

        public bool IsHomeVisible
        {
            get
            {
                if (IsAllKeysVisible == Visibility.Hidden)
                    return false;
                else
                    return KeyboardAction.KeyActions.ContainsKey(KeyName.Home);
            }
            set
            {
                OnPropertyChanged();
            }
        }

        public bool IsEndVisible
        {
            get
            {
                if (IsAllKeysVisible == Visibility.Hidden)
                    return false;
                else
                    return KeyboardAction.KeyActions.ContainsKey(KeyName.End);
            }
            set
            {
                OnPropertyChanged();
            }
        }

        public bool IsPgUpVisible
        {
            get
            {
                if (IsAllKeysVisible == Visibility.Hidden)
                    return false;
                else
                    return KeyboardAction.KeyActions.ContainsKey(KeyName.PgUp);
            }
            set
            {
                OnPropertyChanged();
            }
        }

        public bool IsPgDownVisible
        {
            get
            {
                if (IsAllKeysVisible == Visibility.Hidden)
                    return false;
                else
                    return KeyboardAction.KeyActions.ContainsKey(KeyName.PgDown);
            }
            set
            {
                OnPropertyChanged();
            }
        }

        public Visibility IsAllKeysVisible
        {
            get => _isAllKeysVisible;
            set
            {
                if (_isAllKeysVisible != value)
                {
                    _isAllKeysVisible = value;
                    if (value == Visibility.Visible)
                    {
                        OnPropertyChanged(nameof(IsF8Visible));
                        OnPropertyChanged(nameof(IsPrtScVisible));
                        OnPropertyChanged(nameof(IsScrollLockVisible));
                        OnPropertyChanged(nameof(IsPauseBreakVisible));
                        OnPropertyChanged(nameof(IsCalculatorVisible));
                        OnPropertyChanged(nameof(IsHomeVisible));
                        OnPropertyChanged(nameof(IsEndVisible));
                        OnPropertyChanged(nameof(IsPgUpVisible));
                        OnPropertyChanged(nameof(IsPgDownVisible));
                    }
                    else
                    {
                        IsF8Visible = false;
                        IsPrtScVisible = false;
                        IsScrollLockVisible = false;
                        IsPauseBreakVisible = false;
                        IsCalculatorVisible = false;
                        IsHomeVisible = false;
                        IsEndVisible = false;
                        IsPgUpVisible = false;
                        IsPgDownVisible = false;
                    }
                    OnPropertyChanged();
                }
            }
        }

        public string F1Tooltip
        {
            get
            {
                return GetKeyTooltip(KeyName.F1);
            }
        }

        public string F2Tooltip
        {
            get
            {
                return GetKeyTooltip(KeyName.F2);
            }
        }

        public string F3Tooltip
        {
            get
            {
                return GetKeyTooltip(KeyName.F3);
            }
        }

        public string F4Tooltip
        {
            get
            {
                return GetKeyTooltip(KeyName.F4);
            }
        }

        public string F5Tooltip
        {
            get
            {
                return GetKeyTooltip(KeyName.F5);
            }
        }

        public string F6Tooltip
        {
            get
            {
                return GetKeyTooltip(KeyName.F6);
            }
        }

        public string F7Tooltip
        {
            get
            {
                return GetKeyTooltip(KeyName.F7);
            }
        }

        public string F8Tooltip
        {
            get
            {
                return GetKeyTooltip(KeyName.F8);
            }
        }

        public string F9Tooltip
        {
            get
            {
                return GetKeyTooltip(KeyName.F9);
            }
        }

        public string F10Tooltip
        {
            get
            {
                return GetKeyTooltip(KeyName.F10);
            }
        }

        public string F11Tooltip
        {
            get
            {
                return GetKeyTooltip(KeyName.F11);
            }
        }

        public string F12Tooltip
        {
            get
            {
                return GetKeyTooltip(KeyName.F12);
            }
        }

        public string PrtScTooltip
        {
            get
            {
                return GetKeyTooltip(KeyName.PrtSc);
            }
        }

        public string ScrollLockTooltip
        {
            get
            {
                return GetKeyTooltip(KeyName.ScrollLock);
            }
        }

        public string PauseBreakTooltip
        {
            get
            {
                return GetKeyTooltip(KeyName.PauseBreak);
            }
        }

        public string CalculatorTooltip
        {
            get
            {
                return GetKeyTooltip(KeyName.Calculator);
            }
        }

        public string HomeTooltip
        {
            get
            {
                return GetKeyTooltip(KeyName.Home);
            }
        }

        public string EndTooltip
        {
            get
            {
                return GetKeyTooltip(KeyName.End);
            }
        }

        public string PgUpTooltip
        {
            get
            {
                return GetKeyTooltip(KeyName.PgUp);
            }
        }

        public string PgDownTooltip
        {
            get
            {
                return GetKeyTooltip(KeyName.PgDown);
            }
        }

        private string GetKeyTooltip(KeyName keyName)
        {
            if (KeyboardAction.KeyActions.TryGetValue(keyName, out SelectedAction? action))
            {
                ActionItem actionItem;
                var parameter = "";
                if (action.AssignedAction.ID == -1)
                {
                    if (action.DefaultActionID == -1)
                    {
                        return Strings.NullActionTooltip1;
                    }
                    actionItem = Actions.KnMActions[action.DefaultActionID];
                }
                else
                {
                    actionItem = Actions.KnMActions[action.AssignedAction.ID];
                    parameter = action.AssignedAction.Parameter;
                }
                string tooltip = actionItem.Caption!;
                if (!string.IsNullOrEmpty(parameter))
                    tooltip += " : " + parameter;
                return tooltip;
            }
            return string.Empty;
        }

        public string SelectedKey { get; set; } = "";
        public SelectedAction? SelectedAction => string.IsNullOrEmpty(SelectedKey) ? null : KeyboardAction.KeyActions[(KeyName)Enum.Parse(typeof(KeyName), SelectedKey, true)];
        public int SelectedActionID => string.IsNullOrEmpty(SelectedKey) ? -1 : SelectedAction?.AssignedAction.ID ?? -1;
        public string F1ImageFile { get; set; } = "";
        public string F2ImageFile { get; set; } = "";
        public string F3ImageFile { get; set; } = "";
        public string F4ImageFile { get; set; } = "";
        public string F5ImageFile { get; set; } = "";
        public string F6ImageFile { get; set; } = "";
        public string F7ImageFile { get; set; } = "";
        public string F8ImageFile { get; set; } = "";
        public string F9ImageFile { get; set; } = "";
        public string F10ImageFile { get; set; } = "";
        public string F11ImageFile { get; set; } = "";
        public string F12ImageFile { get; set; } = "";
        public string PrtScImageFile { get; set; } = "";
        public string ScrollLockImageFile { get; set; } = "";
        public string PauseBreakImageFile { get; set; } = "";
        public string CalculatorImageFile { get; set; } = "";
        public string HomeImageFile { get; set; } = "";
        public string EndImageFile { get; set; } = "";
        public string PgUpImageFile { get; set; } = "";
        public string PgDownImageFile { get; set; } = "";

        public void RefreshKeyImageFile(string keyName, bool IsHover = false, bool IsSelected = false)
        {
            KeyName _keyName = (KeyName)Enum.Parse(typeof(KeyName), keyName, true);
            var property = typeof(KeyboardViewModel).GetProperty($"{keyName}ImageFile");

            var action = KeyboardAction.KeyActions[_keyName];
            if (action.AssignedAction.ID == action.DefaultActionID)
            {
                if (IsSelected)
                {
                    property!.SetValue(this, "/DDPM.UI.Resources;component/Resources/Images/Key5.png");
                }
                else
                {
                    if (IsHover)
                    {
                        property!.SetValue(this, "/DDPM.UI.Resources;component/Resources/Images/Key2.png");
                    }
                    else
                    {
                        property!.SetValue(this, "/DDPM.UI.Resources;component/Resources/Images/Key1.png");
                    }
                }
            }
            else
            {
                if (IsSelected)
                {
                    property!.SetValue(this, "/DDPM.UI.Resources;component/Resources/Images/Key6.png");
                }
                else
                {
                    if (IsHover)
                    {
                        property!.SetValue(this, "/DDPM.UI.Resources;component/Resources/Images/Key4.png");
                    }
                    else
                    {
                        property!.SetValue(this, "/DDPM.UI.Resources;component/Resources/Images/Key3.png");
                    }
                }
            }
            var a = property.GetValue(this);
            OnPropertyChanged(property!.Name);
        }

        public bool IsRestoreEnable { get; set; } = true;

        public void CheckRestoreStatus()
        {
            IsRestoreEnable = false;
            foreach (var keyAction in KeyboardAction.KeyActions.Values)
            {
                if (keyAction.DefaultActionID != keyAction.AssignedAction.ID)
                {
                    IsRestoreEnable = true;
                    break;
                }
            }
            OnPropertyChanged(nameof(IsRestoreEnable));
        }

        public bool RestoreToDefault()
        {
            if (DdpmCommonHelper.DeviceManagerSA == null || !DdpmCommonHelper.DeviceManagerSA.RestoreToDefaultKB(CurrentDeviceID.ToString()).Result)
                return false;

            //ResetAction();
            return true;
        }

        private void ResetAction()
        {
            KeyboardAction = new(Model);
            foreach (var key in KeyboardAction.KeyActions.Keys)
            {
                RefreshKeyImageFile(key.ToString());
                OnPropertyChanged($"{key}Tooltip");
            }
            IsRestoreEnable = false;
            OnPropertyChanged(nameof(IsRestoreEnable));
        }

        public void ClearSelectedKey()
        {
            if (!string.IsNullOrEmpty(SelectedKey))
            {
                RefreshKeyImageFile(SelectedKey);
                SelectedKey = "";
            }
        }

        public ObservableCollection<int> SuggestedActions { get => new(Actions.SuggestedActionsK()); }
        public ObservableCollection<int> ProductivityActions { get; set; } = new(Actions.ProductivityActionsKnM());
        public ObservableCollection<int> WindowsActions { get; set; } = new(Actions.WindowsActionsKnM());
        public ObservableCollection<int> MultimediaActions { get; set; } = new(Actions.MultimediaActionsKnM());

        public void UpdateAction(int actionID, string parameter = "", bool RefreshImage = true)
        {
            if (!string.IsNullOrEmpty(SelectedKey))
            {
                int pkId = (int)(KeyName)Enum.Parse(typeof(KeyName), SelectedKey, true);
                pkId = CheckPKID(pkId);

                //string json_str = JsonConvert.SerializeObject(json_obj);
                if (actionID == -1 || actionID > 40)
                {
                    DdpmCommonHelper.DeviceManagerSA?.DeleteKeyboardAssignedAction(CurrentDeviceID.ToString(), pkId);
                }
                else
                {
                    JObject jobj = new()
                    {
                        { "PkId", pkId },
                        { "ActionId", Actions.ActionIdToGuid[actionID] }
                    };

                    if (string.IsNullOrEmpty(parameter))
                    {
                        byte[] newValue = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(jobj));
                        DdpmCommonHelper.DeviceManagerSA?.SetKbAssignedAction(CurrentDeviceID.ToString(), newValue);
                    }
                    else
                    {
                        jobj.Add("Command", parameter);
                        byte[] newValue = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(jobj));
                        if (actionID == 14)
                            DdpmCommonHelper.DeviceManagerSA?.SetKbAssignKeystrokeAction(CurrentDeviceID.ToString(), newValue);
                        else
                            DdpmCommonHelper.DeviceManagerSA?.SetKbAssignDialogAction(CurrentDeviceID.ToString(), newValue);
                    }
                }

                SelectedAction!.AssignedAction.ID = actionID;
                SelectedAction!.AssignedAction.Parameter = parameter;
                OnPropertyChanged($"{SelectedKey}Tooltip");
                if (RefreshImage)
                    RefreshKeyImageFile(SelectedKey, false, true);
                CheckRestoreStatus();
                ActionList.ExportActionList(KeyboardAction, Model);
            }
        }

        private int CheckPKID(int pkID)
        {
            switch (Model)
            {
                case "KB525C":
                case "KB900":
                    if (pkID == 19)      // ScrollLock
                        return 14;       // M2
                    else if (pkID == 20) // PauseBreak
                        return 15;       // M3
                    else
                        return pkID;
                default:                 // KB555
                    return pkID;
            }
        }
        public CTKMessageHelper CTKMessageHelper { get; set; } = new();

        private bool isBatteryUnavailable { get; set; } = false;

        public bool IsBatteryUnavailable
        {
            get { return isBatteryUnavailable; }
            set
            {
                isBatteryUnavailable = value;
                OnPropertyChanged(nameof(IsBatteryUnavailable));
            }
        }
    }
}