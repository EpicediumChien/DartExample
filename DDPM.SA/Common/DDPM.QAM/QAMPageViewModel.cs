using System.ComponentModel;
using System.Windows.Controls;
using System.Windows;
using DDPM.SA.Common;
using DPeMPublic.Common.Enums;
using System.Collections.ObjectModel;
using DDPM.SA.Resources.Helper;
using DDPM.SA.Common.Settings;
using Dell.Client.Framework.Common;

namespace DDPM.QAM
{
    public class QAMPageViewModel : INotifyPropertyChanged
    {
        public DeviceInfo? CurrentDeviceInfo { get; set; }
        public string DeviceModel { get; set; } = string.Empty;
        public bool[] Settings_IsEnable { get; set; } = { true, true, true, true };
        public bool[] Settings_IsSelected { get; set; } = { false, false, false, false };
        public Visibility[] Settings_IsVisibility { get; set; } = { Visibility.Visible, Visibility.Visible, Visibility.Visible, Visibility.Visible };

        public string selectedProfileName = string.Empty;

        public event PropertyChangedEventHandler? PropertyChanged;
        public void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        private ContentControl? _fullView;
        public ContentControl? FullView
        {
            get => _fullView;
            set => _fullView = value;
        }

        private bool isQAMPageViewModel_UIUpdateNotifyExist = false;

        private WebcamSettings webcamSettings; //Derek 2025/01/09
        private ILog? logger;

        public bool isStatusChangeByDDPM = false;
        public QAMPageViewModel(IDeviceManagerSA? devMgr = null, ILog? log = null)
        {
            try
            {
                logger = log;
                List<DeviceInfo> deviceInfos = DdpmCommonHelper.DeviceManagerSA.GetDevices().Result.deviceInfo;

                if (deviceInfos != null && deviceInfos.Count > 0)
                {
                    CurrentDeviceInfo = deviceInfos.FirstOrDefault(x => (x.PhysicalDeviceType.Equals(DeviceType.LogicalWebcam) || x.PhysicalDeviceType.Equals(DeviceType.PhysicalWebcam)));

                    if (CurrentDeviceInfo != null)
                    {
                        DdpmCommonHelper.QAMCameraID = CurrentDeviceInfo.ID;
                        //DeviceModel = CurrentDeviceInfo.Name;
                        DeviceModel = CurrentDeviceInfo.Name + " " + CurrentDeviceInfo.ModelNumber; //Derek 1213

                        //var filePath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                        //    @$"Dell\Dell Display and Peripheral Manager\WebcamSettings\{CurrentDeviceInfo.ModelNumber}.json");

                        //if (!File.Exists(filePath))
                        //{
                        //    //Derek 20250107 change to use WebcamSettings.ImportWebcamSettings function 
                        //    WebcamSettings ws = WebcamSettings.ImportWebcamSettings(CurrentDeviceInfo.ModelNumber, CurrentDeviceInfo, devMgr, log);

                        //    LogMsg($"Create webcam profile:{filePath} due to it's not exist, result is {ws}");
                        //}

                        webcamSettings = WebcamSettings.ImportWebcamSettings(CurrentDeviceInfo.ModelNumber, CurrentDeviceInfo, devMgr, log);

                        if (webcamSettings != null)
                        {
                            selectedProfileName = webcamSettings.SelectedProfileName?.ToString() ?? string.Empty;

                            UI_ProfileList = new ObservableCollection<UI_Profile>();
                            foreach (var profile in webcamSettings.PresetProfiles)
                            {
                                UI_ProfileList.Add(new UI_Profile
                                {
                                    //Profile_Name = profile.Key,
                                    Profile_Name = LangHelper.Instance[profile.Key],
                                    Profile_Name_Key = profile.Key,
                                    IsSelected = selectedProfileName == profile.Key,
                                });

                                LogMsg($"QAM ImportWebcamProfiles -> Add {profile.Key}/{profile.Value}");
                            }

                            //LogMsg($"ImportWebcamProfiles current SelectedProfileName: {selectedProfileName}, QAM ws.PresetProfiles = {webcamSettings.PresetProfiles.Count}");
                            LogMsg($"ImportWebcamProfiles current SelectedProfileName: {selectedProfileName}, " +
                                            $"QAM ws.PresetProfiles = {webcamSettings.PresetProfiles.Count}");

                            //for debug
                            //foreach (var profile in webcamSettings.CustomProfiles)
                            //{
                            //    LogMsg($"QAM CustomProfiles -> Add {profile.Key}/{profile.Value}");
                            //}

                            //LogMsg($"QAM CustomProfiles.count =  {webcamSettings.CustomProfiles.Count}");

                            LogMsg($"webcamSettings.NONE.Name =  {webcamSettings.NONE.Name}");
                        }

                        //ImportWebcamProfiles(CurrentDeviceInfo.ModelNumber, devMgr, log);// filePath);
                        ZoomMax = CurrentDeviceInfo.ZoomMax;
                        ZoomMin = CurrentDeviceInfo.ZoomMin;
                        //if (!CurrentDeviceInfo.IsPropertyAutoFramingSupported)
                        if (!IsAutoFramingVisable())
                        {
                            Settings_IsVisibility[1] = Visibility.Collapsed;
                        }

                        //added by Derek 1225 to get Webcam AutoFraming Property
                        LogMsg($"AutoFraming Property,IsAutoFramingVisable = {IsAutoFramingVisable()}, IsPropertyAutoFramingSupported = " +
                            $"{CurrentDeviceInfo.IsPropertyAutoFramingSupported}, GetIsPropertyAutoFramingSupported = " +
                            $"{DdpmCommonHelper.DeviceManagerSA?.GetIsPropertyAutoFramingSupported(DdpmCommonHelper.DeviceManagerSA?.GetWebcamDeviceID().Result).Result}");

                        for (int k = 0; k < CurrentDeviceInfo!.FOVValues.Length; k++)
                        {
                            if (int.TryParse(CurrentDeviceInfo!.FOVValues[k], out int parsedValue))
                            {
                                _fOVs[k] = parsedValue;
                            }
                            else
                            {
                                DdpmCommonHelper.DeviceManagerSA?.WriteLog($"Failed to parse FOV value: {CurrentDeviceInfo.FOVValues[k]}");
                            }

                            //_fOVs[k] = int.Parse(CurrentDeviceInfo!.FOVValues[k]);
                        }
                    }
                }

                //Derek 1210
                if (!isQAMPageViewModel_UIUpdateNotifyExist)
                {
                    DdpmCommonHelper.DeviceManagerSA.UIUpdateNotify += QAMPageViewModel_UIUpdateNotify;
                    isQAMPageViewModel_UIUpdateNotifyExist = true;

                    LogMsg($"Add event QAMPageViewModel_UIUpdateNotify, isQAMPageViewModel_UIUpdateNotifyExist = {isQAMPageViewModel_UIUpdateNotifyExist}");
                }

                LoadCurrentStatus();
            }
            catch (Exception e)
            {
                LogMsg($"QAMPageViewModel --> Catch exception {e.Message}");
            }
        }

        //~QAMPageViewModel()
        //{
        //    RemoveQAMWebcamEvent();
        //}

        public void RemoveQAMWebcamEvent()
        {
            DdpmCommonHelper.DeviceManagerSA!.UIUpdateNotify -= QAMPageViewModel_UIUpdateNotify;
            isQAMPageViewModel_UIUpdateNotifyExist = false;

            LogMsg($"Remove event QAMPageViewModel_UIUpdateNotify");
        }

        private void LoadCurrentStatus()
        {
            try
            {
                isStatusChangeByDDPM = true;

                ZoomValue = DdpmCommonHelper.DeviceManagerSA!.GetZoom(CurrentDeviceInfo!.ID.ToString()).Result;

                if (CurrentDeviceInfo!.IsPropertyAutoFramingSupported) //Derek 1225
                    AutoFramingStatus = DdpmCommonHelper.DeviceManagerSA!.GetIsAutoFramingOn(CurrentDeviceInfo!.ID.ToString()).Result;

                FieldOfView = DdpmCommonHelper.DeviceManagerSA!.GetFieldOfView(CurrentDeviceInfo!.ID.ToString()).Result;

                LogMsg($"LoadCurrentStatus --> ZoomValue = {ZoomValue}, AutoFramingStatus = {AutoFramingStatus}, selFOV = {FieldOfView}");

                if (FieldOfView != -1)
                    FOV_Selected(ChangeFOVToSelectIndex(FieldOfView));
            }
            catch (Exception e)
            {
                LogMsg($"LoadCurrentStatus Catch exception [{e.Message}]");
            }

            isStatusChangeByDDPM = false;
        }

        private int ChangeFOVToSelectIndex(int fov)
        {
            if (90 == fov)
                return 2;
            else if (78 == fov)
                return 1;
            else
                return 0;
        }

        private void LogMsg(string msg)
        {
            DdpmCommonHelper.DeviceManagerSA!.WriteLog(msg);
        }

        private void QAMPageViewModel_UIUpdateNotify(object? sender, UpdateUINotify e)
        {
            try
            {
                if (e == null || e == EventArgs.Empty || e.UI_Field_Name == string.Empty)
                    return;

                LogMsg($"QAMPageViewModel_UIUpdateNotify receive msg is {e.UI_Field_Name}");

                if (e.UI_Field_Name.StartsWith("WebcamEvent"))
                {
                    EventMsg? eventMsg = EventMsg.CreateEventObjectFromEventMsg(e.UI_Field_Name);

                    if (null == eventMsg)
                    {
                        LogMsg($"Webcam event message is null");

                        return;
                    }

                    int currentValue = 0;
                    switch (eventMsg.EventType)
                    {
                        //add event by leo 2025/01/14 start
                        //Marked by Derek 2025/01/16 use another solution to implement
                        //case "Webcam_IsHDROnChanged":
                        //{
                        //    SetNoneProfile();
                        //}
                        //break;
                        //case "Webcam_BrightnessChanged":
                        //{
                        //    SetNoneProfile();
                        //}
                        //break;
                        //case "Webcam_ContrastChanged":
                        //{
                        //    SetNoneProfile();
                        //}
                        //break;
                        //case "Webcam_SaturationChanged":
                        //{
                        //    SetNoneProfile();
                        //}
                        //break;
                        //case "Webcam_SharpnessChanged":
                        //{
                        //    SetNoneProfile();
                        //}
                        //break;
                        //case "Webcam_AutoWhiteBalanceChanged":
                        //{
                        //    SetNoneProfile();
                        //}
                        //break;
                        //case "Webcam_IsAutoWhiteBalanceOnChanged":
                        //{
                        //    SetNoneProfile();
                        //}
                        //break;
                        //add event by leo 2025/01/14 end

                        case "Webcam_ZoomChanged":
                        { //leo fixed 2025/01/14
                            if (int.TryParse(eventMsg.NewValue, out currentValue))
                            {
                                isStatusChangeByDDPM = true;
                                ZoomValue = currentValue;
                            }
                        }
                        break;

                        case "Webcam_FieldOfViewChanged":
                            if (int.TryParse(eventMsg.NewValue, out currentValue))
                            {
                                isStatusChangeByDDPM = true;
                                FieldOfView = currentValue;
                                FOV_Selected(ChangeFOVToSelectIndex(FieldOfView));

                                //add by leo 2025/01/14
                                //SetNoneProfile();
                            }

                            break;


                        case "Webcam_IsAutoFramingOnChanged":
                            bool result = false;
                            if (bool.TryParse(eventMsg.NewValue, out result))
                            {
                                isStatusChangeByDDPM = true;
                                AutoFramingStatus = result;

                                //Derek 2025/02/21 align QAM UI with DDPM when auto frame changed by DDPM
                                ChangeUIWhenAutoFramingStatusChange(AutoFramingStatus);
                            }
                            break;
                    }
                }
                else if (e.UI_Field_Name.StartsWith("WebcamProfileFromDDPM")) //1212 Derek
                {
                    //format "WebcamProfileFromDDPM:{profileName}"
                    string[] msgs = e.UI_Field_Name.Split(':');

                    if (null != msgs && msgs.Length == 2)
                    {
                        isStatusChangeByDDPM = true; //Derek 1227

                        if (SetProfile(msgs[1]))
                            LogMsg($"QAMPageViewModel_UIUpdateNotify: set webcam profile:{msgs[1]} successfully.");
                        else
                            LogMsg($"QAMPageViewModel_UIUpdateNotify: set webcam profile:{msgs[1]} fail!");
                    }
                }
                else if (e.UI_Field_Name.StartsWith("DDPMSetProfileToNone")) //Derek 2025/01/16
                {
                    SetNoneProfile();
                }
                //else if (e.UI_Field_Name.StartsWith("DDPMSetProfileToCurrent")) //Derek 2025/01/18 don't need to use this message
                //{
                //    SetProfile();
                //}

            }
            catch (Exception ex)
            {
                LogMsg($"Catch exception in QAMPageViewModel_UIUpdateNotify: {ex.Message}");
            }

        }

        public void SetNoneProfile()
        {
            try
            {
                List<UI_Profile> temp = UI_ProfileList.ToList();
                UI_ProfileList = new ObservableCollection<UI_Profile>();

                foreach (var profile in temp)
                {
                    profile.IsSelected = false;
                    UI_ProfileList.Add(profile);
                }

                OnPropertyChanged(nameof(UI_ProfileList));

                selectedProfileName = "NONE"; //Derek 2025/01/20
                webcamSettings.SelectedProfileName = selectedProfileName;

                // << 250321 added by hess
                webcamSettings.NONE.IsAutoFramingOn = _AutoFramingStatus;
                webcamSettings.NONE.FieldOfView = FOVs[SelectedFOVIndex];
                webcamSettings.NONE.Zoom = _ZoomValue;
                // >>

                SaveSelectProfile(); //Derek 2025/02/11 for PIMS 330862 FOV test fail
            }
            catch (Exception ex)
            {
                LogMsg($"Catch exception in  SetNoneProfile : {ex.Message}");
            }
        }

        public void OpenFullView(ContentControl content)
        {
            FullView = content;
            FullView.Visibility = Visibility.Visible;
            OnPropertyChanged(nameof(FullView));
        }

        public void CloseFullView()
        {
            FullView = null;
        }

        private int selIndex = 0;
        public void Settings_Selected(int index)
        {
            for (int j = 0; j < Settings_IsSelected.Length; j++)
            {
                Settings_IsSelected[j] = false;
            }

            Settings_IsSelected[index] = true;
            selIndex = index;

            OnPropertyChanged(nameof(Settings_IsSelected));
        }

        #region Presets
        public ObservableCollection<UI_Profile> UI_ProfileList { get; set; }
        //public Dictionary<string, WebcamProfile> Profiles = new Dictionary<string, WebcamProfile>();
        private WebcamProfile CurrentProfile;

        private bool SetProfile(string name)
        {
            try
            {
                foreach (var profile in UI_ProfileList)
                {
                    //LogMsg($"Profile_Name_Key: {profile.Profile_Name_Key}, Profile_Name: {profile.Profile_Name}");

                    if (profile.Profile_Name_Key.Equals(name))
                    {
                        SetProfile(profile);

                        return true;
                    }
                }

                LogMsg($"Could not found {name} in current UI_ProfileList, set profile to none");
                SetNoneProfile(); //Derek 2025/01/18

                return false;
            }
            catch (Exception e)
            {
                LogMsg($"Catch exception: {e.Message}");

                return false;
            }
        }

        public void SendSelectProfileToDDPM()
        {
            DdpmCommonHelper.DeviceManagerSA!.SyncWebcamProfile(selectedProfileName);
        }

        public void SetProfile()
        {
            if (selectedProfileName != null && selectedProfileName != string.Empty)
                SetProfile(selectedProfileName);
        }

        public void SetProfile(UI_Profile selProfile)
        {
            LogMsg($"QAM user want to change profile to: {selProfile.Profile_Name}, {selProfile.Profile_Name_Key}, current is {selectedProfileName}");

            try
            {
                if (webcamSettings.PresetProfiles.ContainsKey(selProfile.Profile_Name_Key))
                {
                    CurrentProfile = webcamSettings.PresetProfiles[selProfile.Profile_Name_Key];

                    LogMsg($"CurrentProfile settings are: Name = {CurrentProfile.Name}, Sharpness = {CurrentProfile.Sharpness}, Brightness = {CurrentProfile.Brightness}" +
                        $", Saturation = {CurrentProfile.Saturation}, AutoFramingFrameSize = {CurrentProfile.AutoFramingFrameSize}, Name = {CurrentProfile.Name}, IsAutoWhiteBalanceOn = {CurrentProfile.IsAutoWhiteBalanceOn}" +
                        $", IsAutoFramingOn = {CurrentProfile.IsAutoFramingOn}, FieldOfView = {CurrentProfile.FieldOfView}, IsHDROn = {CurrentProfile.IsHDROn}, AutoWhiteBalance = {CurrentProfile.AutoWhiteBalance}" +
                        $", Contrast = {CurrentProfile.Contrast}");

                    if (CurrentDeviceInfo!.IsPropertyAutoFramingSensitivitySupported || CurrentDeviceInfo.IsPropertyAutoFramingSizeSupported || CurrentDeviceInfo.IsPropertyAutoFramingTransitionSupported)
                    {
                        _AutoFramingStatus = CurrentProfile.IsAutoFramingOn;

                        if (!isStatusChangeByDDPM) //Derek 1227 to improve performance
                        {
                            bool result = DdpmCommonHelper.DeviceManagerSA!.SetIsAutoFramingOn(CurrentDeviceInfo!.ID.ToString(), CurrentProfile.IsAutoFramingOn).Result;
                            LogMsg($"SetProfile --> SetIsAutoFramingOn to {CurrentProfile.IsAutoFramingOn} result is {result}");

                            //Derek 2025/01/18  DDPM doesn't set this property
                            //if (CurrentDeviceInfo.IsPropertyAutoFramingTransitionSupported)
                            //{
                            //    result = DdpmCommonHelper.DeviceManagerSA!.SetIsAutoFramingTransitionOn(CurrentDeviceInfo!.ID.ToString(), CurrentProfile.IsAutoFramingTransitionOn).Result;

                            //    LogMsg($"SetProfile --> SetIsAutoFramingTransitionOn result is {result}");
                            //}

                            //Derek 2025/01/18  DDPM doesn't set this property
                            //if (CurrentDeviceInfo.IsPropertyAutoFramingSensitivitySupported)
                            //{
                            //    result = DdpmCommonHelper.DeviceManagerSA!.SetAutoFramingSensitivity(CurrentDeviceInfo!.ID.ToString(), CurrentProfile.AutoFramingSensitivity).Result;

                            //    LogMsg($"SetProfile --> SetAutoFramingSensitivity to {CurrentProfile.AutoFramingSensitivity} result is {result}");
                            //}

                            //Derek 2025/01/18  DDPM doesn't set this property
                            //if (CurrentDeviceInfo.IsPropertyAutoFramingSizeSupported)
                            //{
                            //    result = DdpmCommonHelper.DeviceManagerSA!.SetAutoFramingFrameSize(CurrentDeviceInfo!.ID.ToString(), CurrentProfile.AutoFramingFrameSize).Result;

                            //    LogMsg($"SetProfile --> SetAutoFramingFrameSize result is {result}");
                            //}
                        }
                        else
                            LogMsg($"SetProfile SetIsAutoFramingOn/SetIsAutoFramingTransitionOn... has modified by UI");
                    }

                    if (CurrentDeviceInfo.IsPropertyFOVSupported)
                    {
                        if (_fOVs[0] == CurrentProfile.FieldOfView)
                        {
                            FOV_Selected(0);
                        }
                        else if (_fOVs[1] == CurrentProfile.FieldOfView)
                        {
                            FOV_Selected(1);
                        }
                        else
                        {
                            FOV_Selected(2);
                        }

                        //Derek 1227 should set FOV
                        FieldOfView = CurrentProfile.FieldOfView;
                        LogMsg($"SetProfile change FieldOfView to {FieldOfView}");
                    }

                    //Derek 2025/01/07 don't change the zoom value(same action with DDPM)
                    //if (CurrentDeviceInfo.IsPropertyZoomSupported)
                    //{
                    //    //1227
                    //    //bool result = DdpmCommonHelper.DeviceManagerSA!.SetZoom(CurrentDeviceInfo!.ID.ToString(), CurrentProfile.Zoom).Result;
                    //    //_ZoomValue = CurrentProfile.Zoom;

                    //    //LogMsg($"SetProfile --> SetZoom result is {result}");

                    //    //Derek 1227
                    //    if (-1 != CurrentProfile.Zoom)
                    //    {
                    //        ZoomValue = CurrentProfile.Zoom;
                    //        SetZoom();
                    //    }
                    //}

                    //Derek 2025/01/18  DDPM doesn't set this property
                    //if (CurrentDeviceInfo.IsPropertyFocusSupported)
                    //{
                    //    if (!isStatusChangeByDDPM)
                    //    {
                    //        bool result = DdpmCommonHelper.DeviceManagerSA!.SetIsFocusOn(CurrentDeviceInfo!.ID.ToString(), CurrentProfile.IsFocusOn).Result;
                    //        LogMsg($"SetProfile --> SetIsFocusOn result is {result}");

                    //        result = DdpmCommonHelper.DeviceManagerSA!.SetFocus(CurrentDeviceInfo!.ID.ToString(), CurrentProfile.Focus).Result;
                    //        LogMsg($"SetProfile --> SetFocus result is {result}");
                    //    }
                    //    else
                    //        LogMsg($"SetProfile --> SetIsFocusOn/SetFocus has modified by UI");
                    //}

                    //Derek 2025/01/18  DDPM doesn't set this property
                    //if (CurrentDeviceInfo.IsPropertyPrioritySupported)
                    //{
                    //    if (!isStatusChangeByDDPM)
                    //    {
                    //        bool result = DdpmCommonHelper.DeviceManagerSA!.SetPriority(CurrentDeviceInfo!.ID.ToString(), CurrentProfile.Priority).Result;
                    //        LogMsg($"SetProfile --> SetPriority result is {result}");
                    //    }
                    //    else
                    //        LogMsg($"SetProfile --> SetPriority has modified by UI");
                    //}

                    if (CurrentDeviceInfo.IsPropertyHDRSupported)
                    {
                        if (!isStatusChangeByDDPM)
                        {
                            bool result = DdpmCommonHelper.DeviceManagerSA!.SetIsHDROn(CurrentDeviceInfo!.ID.ToString(), CurrentProfile.IsHDROn).Result;

                            LogMsg($"SetProfile --> SetIsHDROn to {CurrentProfile.IsHDROn} result is {result}");
                        }
                        else
                            LogMsg($"SetProfile --> SetIsHDROn has modified by UI");
                    }

                    if (CurrentDeviceInfo.IsPropertyWhiteBalanceSupported)
                    {
                        if (!isStatusChangeByDDPM)
                        {
                            bool result = DdpmCommonHelper.DeviceManagerSA!.SetIsAutoWhiteBalanceOn(CurrentDeviceInfo!.ID.ToString(), CurrentProfile.IsAutoWhiteBalanceOn).Result;
                            LogMsg($"SetProfile --> SetIsAutoWhiteBalanceOn to {CurrentProfile.IsAutoWhiteBalanceOn} result is {result}");

                            result = DdpmCommonHelper.DeviceManagerSA!.SetAutoWhiteBalance(CurrentDeviceInfo!.ID.ToString(), CurrentProfile.AutoWhiteBalance).Result;
                            LogMsg($"SetProfile --> SetAutoWhiteBalance to {CurrentProfile.AutoWhiteBalance} result is {result}");
                        }
                        else
                            LogMsg($"SetProfile --> SetIsAutoWhiteBalanceOn/SetAutoWhiteBalance has modified by UI");
                    }

                    //Derek 2025/01/18 Add DDPM extra setting properties
                    if (CurrentDeviceInfo.IsPropertyBrightnessSupported)
                    {
                        if (!isStatusChangeByDDPM)
                        {
                            bool result = DdpmCommonHelper.DeviceManagerSA!.SetBrightness(CurrentDeviceInfo!.ID.ToString(), CurrentProfile.Brightness).Result;
                            LogMsg($"SetProfile --> SetBrightness to {CurrentProfile.Brightness} result is {result}");
                        }
                        else
                            LogMsg($"SetProfile --> SetBrightness has modified by UI");
                    }

                    if (CurrentDeviceInfo.IsPropertySharpnessSupported)
                    {
                        if (!isStatusChangeByDDPM)
                        {
                            bool result = DdpmCommonHelper.DeviceManagerSA!.SetSharpness(CurrentDeviceInfo!.ID.ToString(), CurrentProfile.Sharpness).Result;
                            LogMsg($"SetProfile --> SetSharpness to {CurrentProfile.Sharpness} result is {result}");
                        }
                        else
                            LogMsg($"SetProfile --> SetSharpness has modified by UI");
                    }

                    if (CurrentDeviceInfo.IsPropertyContrastSupported)
                    {
                        if (!isStatusChangeByDDPM)
                        {
                            bool result = DdpmCommonHelper.DeviceManagerSA!.SetContrast(CurrentDeviceInfo!.ID.ToString(), CurrentProfile.Contrast).Result;
                            LogMsg($"SetProfile --> SetContrast to {CurrentProfile.Contrast} result is {result}");
                        }
                        else
                            LogMsg($"SetProfile --> SetContrast has modified by UI");
                    }

                    if (CurrentDeviceInfo.IsPropertySaturationSupported)
                    {
                        if (!isStatusChangeByDDPM)
                        {
                            bool result = DdpmCommonHelper.DeviceManagerSA!.SetSaturation(CurrentDeviceInfo!.ID.ToString(), CurrentProfile.Saturation).Result;
                            LogMsg($"SetProfile --> SetSaturation to {CurrentProfile.Saturation} result is {result}");
                        }
                        else
                            LogMsg($"SetProfile --> SetSaturation has modified by UI");
                    }
                    //end Added

                    List<UI_Profile> temp = UI_ProfileList.ToList();
                    UI_ProfileList = new ObservableCollection<UI_Profile>();
                    foreach (var profile in temp)
                    {
                        profile.IsSelected = false;

                        if (profile.Profile_Name.Equals(selProfile.Profile_Name))
                        {
                            profile.IsSelected = true;
                            selectedProfileName = profile.Profile_Name_Key;

                            //Derek 2025/01/09
                            webcamSettings.SelectedProfileName = selectedProfileName;
                            SaveSelectProfile();
                        }
                        UI_ProfileList.Add(profile);
                    }

                    RefreshUI();

                    //SaveSelectProfile(CurrentProfileName.Profile_Name_Key);
                    LogMsg($"QAM SetProfile successfully");
                }
            }
            catch (Exception e)
            {
                LogMsg($"QAM SetProfile Catch exception: {e.Message}");
            }

        }

        private void SaveSelectProfile()
        {
            try
            {
                bool result = WebcamSettings.ExportWebcamSettings(webcamSettings,
                                                        CurrentDeviceInfo?.ModelNumber,
                                                        DdpmCommonHelper.DeviceManagerSA,
                                                        logger);

                LogMsg($"QAM SaveSelectProfile result = {result}");
            }
            catch (Exception e)
            {
                LogMsg($"Catch exception: {e.Message} when SaveSelectProfile");
            }
        }

        #endregion
        #region AutoFraming
        bool _AutoFramingStatus;
        public bool AutoFramingStatus
        {
            get => _AutoFramingStatus;
            set
            {
                _AutoFramingStatus = value;

                RefreshUI();

                OnPropertyChanged(nameof(AutoFramingStatus));

                SetAutoFramingStatus();
                if (!value)
                    FOV_Selected(SelectedFOVIndex);
            }
        }

        public void SetAutoFramingStatus()
        {
            if (!isStatusChangeByDDPM)
            {
                bool result = DdpmCommonHelper.DeviceManagerSA!.SetIsAutoFramingOn(CurrentDeviceInfo!.ID.ToString(), _AutoFramingStatus).Result;

                LogMsg($"QAM SetIsAutoFramingOn value to {_AutoFramingStatus}, result is {result}");
            }
            else
                LogMsg($"QAM SetIsAutoFramingOn value has modified by UI");

            //LogMsg($"AutoFramingStatus -> {isStatusChagneByDDPM}");
        }

        public bool IsAutoFramingVisable()
        {
            try
            {
                //Derek 1226 use the same check condition as DDPM
                return (CurrentDeviceInfo!.IsPropertyAutoFramingSensitivitySupported ||
                        CurrentDeviceInfo!.IsPropertyAutoFramingSizeSupported ||
                        CurrentDeviceInfo!.IsPropertyAutoFramingTransitionSupported);
            }
            catch (Exception e)
            {
                LogMsg($"IsAutoFramingVisable get exception {e.Message}");

                return false;
            }

        }

        #endregion
        #region FOV
        public bool[] FOV_IsSelected { get; set; } = { false, false, false };
        private int[] _fOVs = [0, 0, 0];
        public int[] FOVs
        {
            get => _fOVs;
        }
        private int _FieldOfView;
        public int FieldOfView
        {
            get => _FieldOfView;
            set
            {
                _FieldOfView = value;

                if (!isStatusChangeByDDPM)
                {
                    bool result = DdpmCommonHelper.DeviceManagerSA!.SetFieldOfView(CurrentDeviceInfo!.ID.ToString(), _FieldOfView).Result;

                    LogMsg($"QAM SetFieldOfView value to {_FieldOfView}, result is {result}");
                }
                else
                    LogMsg($"QAM SetFieldOfView value has modified by UI");
            }
        }

        private int SelectedFOVIndex = 0;
        public void FOV_Selected(int index)
        {
            SelectedFOVIndex = index;
            for (int j = 0; j < FOV_IsSelected.Length; j++)
            {
                FOV_IsSelected[j] = false;
            }
            FOV_IsSelected[index] = true;

            OnPropertyChanged(nameof(FOV_IsSelected));
        }
        #endregion
        #region Zoom
        private bool is_ZoomEnableStatus = true;
        public bool ZoomEnableStatus
        {
            get => is_ZoomEnableStatus;
            set
            {
                is_ZoomEnableStatus = value;
                OnPropertyChanged(nameof(ZoomEnableStatus));
            }
        }

        public string FullView_Height { get; set; } = "0";
        private int _ZoomValue { get; set; }
        public bool IsSliderDragging = false;
        public int ZoomMax { get; set; }
        public int ZoomMin { get; set; }
        public int ZoomValue
        {
            get => _ZoomValue;
            set
            {
                if (value != _ZoomValue)
                {
                    _ZoomValue = value;
                    if (!IsSliderDragging)
                    {
                        SetZoom();
                    }
                }

                RefreshUI();
            }
        }

        public void SetZoom()
        {
            if (!isStatusChangeByDDPM)
            {
                bool result = DdpmCommonHelper.DeviceManagerSA!.SetZoom(CurrentDeviceInfo!.ID.ToString(), _ZoomValue).Result;
                LogMsg($"QAM set Zoom value to {_ZoomValue}, result is {result}");
                SetNoneProfile();
            }
            else
                LogMsg($"QAM Zoom value has modified by UI");
        }
        #endregion

        private bool isCameraSettingSelected = false;
        public bool IsCameraSettingSelected
        {
            get => isCameraSettingSelected;

            set
            {
                isCameraSettingSelected = value;
                OnPropertyChanged(nameof(isCameraSettingSelected));
            }
        }

        #region Dragging
        private bool _isDragging = false;
        public bool IsDragging
        {
            get { return _isDragging; }
            set
            {
                _isDragging = value;
                OnPropertyChanged("IsDragging");
            }
        }
        #endregion


        public void RefreshUI()
        {
            if (_AutoFramingStatus)
            {
                Settings_IsEnable[2] = false;
                Settings_IsEnable[3] = false;
            }
            else
            {
                Settings_IsEnable[2] = true;
                Settings_IsEnable[3] = true;
            }

            OnPropertyChanged(nameof(DeviceModel));
            OnPropertyChanged(nameof(Settings_IsEnable));
            OnPropertyChanged(nameof(ZoomValue));
            OnPropertyChanged(nameof(FullView_Height));
            OnPropertyChanged(nameof(UI_ProfileList));

            //DdpmCommonHelper.DeviceManagerSA!.WriteLog($"RefreshUI  --> ZoomValue = {ZoomValue}, FieldOfView = {FieldOfView}, _AutoFramingStatus = {_AutoFramingStatus}");
        }

        private void ChangeUIWhenAutoFramingStatusChange(bool statusIsOn)
        {
            //MessageBox.Show("ChangeUIWhenAutoFramingStatusChange");

            //if (selIndex < 2)
            //{
            //    LogMsg($"ChangeUIWhenAutoFramingStatusChange return due to page FOV or ZOOM not selected");

            //    return;
            //}

            //disable/enable FOV and zoom slider bar status
            ZoomEnableStatus = !statusIsOn;

            try
            {
                if (statusIsOn)
                {
                    //1. UI FOV change to largest
                    FOV_Selected(CurrentDeviceInfo!.FOVValues.Length - 1);
                }
                else
                {
                    FieldOfView = DdpmCommonHelper.DeviceManagerSA!.GetFieldOfView(CurrentDeviceInfo!.ID.ToString()).Result;
                    if (FieldOfView != -1)
                        FOV_Selected(ChangeFOVToSelectIndex(FieldOfView));
                }
            }
            catch (Exception e)
            {
                LogMsg($"ChangeUIWhenAutoFramingStatusChange catch exception: {e.Message}");
            }
        }

        public bool SaveNoneProfileFOV(int fov)
        {
            try
            {
                if (null != webcamSettings.NONE && selectedProfileName == "NONE" &&
                    (65 == fov || 78 == fov || 90 == fov))
                {
                    webcamSettings.NONE.FieldOfView = fov;

                    SaveSelectProfile();

                    return true;
                }
            }
            catch (Exception e)
            {
                LogMsg($"SaveNoneProfileFOV catch exception: {e.Message}");
            }

            return false;
        }

        //public bool SaveNoneProfileZOOM()
        //{
        //    try
        //    {
        //        if (null != webcamSettings.NONE && selectedProfileName != "NONE")
        //        {
        //            webcamSettings.NONE.Zoom = ZoomValue;

        //            SaveSelectProfile();

        //            return true;
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        LogMsg($"SaveNoneProfileZOOM catch exception: {e.Message}");
        //    }

        //    return false;
        //}
    }
    public class UI_Profile
    {
        public string Profile_Name { get; set; }
        public bool IsSelected { get; set; }
        public string Profile_Name_Key { get; set; } //Derek added for QAM PIMS-325190
    }
}