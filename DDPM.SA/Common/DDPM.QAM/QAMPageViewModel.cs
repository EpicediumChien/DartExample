using CommunityToolkit.Mvvm.ComponentModel;
using Dell.Client.Framework.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;
using DDPM.SA.Common;
using DPeMPublic.Common.Enums;
using System.Windows.Media;
using Newtonsoft.Json;
using System.IO;
using System.Windows.Shapes;
using System.Collections.ObjectModel;
using DDPM.SA.Resources.Helper;
using System.Windows.Input;
using System.Diagnostics;
using System.Windows.Interop;
using Windows.Data.Text;
using System.Windows.Media.Media3D;
using System.Linq.Expressions;
using MS.WindowsAPICodePack.Internal;

namespace DDPM.QAM
{
    public class QAMPageViewModel : INotifyPropertyChanged
    {
        public DeviceInfo CurrentDeviceInfo { get; set; }
        public string DeviceModel { get; set; }
        public bool[] Settings_IsEnable { get; set; } = { true, true, true, true };
        public bool[] Settings_IsSelected { get; set; } = { false, false, false, false };
        public Visibility[] Settings_IsVisibility { get; set; } = { Visibility.Visible, Visibility.Visible, Visibility.Visible, Visibility.Visible };
        
        private string selectedProfileName = string.Empty;

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


        public bool isStatusChagneByDDPM = false;
        public QAMPageViewModel()
        {
            List<DeviceInfo> deviceInfos = DdpmCommonHelper.DeviceManagerSA!.GetDevices().Result.deviceInfo;

            if (deviceInfos != null && deviceInfos.Count > 0)
            {
                CurrentDeviceInfo = deviceInfos.FirstOrDefault(x => (x.PhysicalDeviceType.Equals(DeviceType.LogicalWebcam) || x.PhysicalDeviceType.Equals(DeviceType.PhysicalWebcam)));
                if (CurrentDeviceInfo != null)
                {
                    //DeviceModel = CurrentDeviceInfo.Name;
                    DeviceModel = CurrentDeviceInfo.Name + " " + CurrentDeviceInfo.ModelNumber; //Derek 1213
                    ImportWebcamProfiles(CurrentDeviceInfo.ModelNumber);
                    ZoomMax = CurrentDeviceInfo.ZoomMax;
                    ZoomMin = CurrentDeviceInfo.ZoomMin;
                    if (!CurrentDeviceInfo.IsPropertyAutoFramingSupported)
                    {
                        Settings_IsVisibility[1] = Visibility.Collapsed;
                    }

                    for (int k = 0; k < CurrentDeviceInfo!.FOVValues.Length; k++)
                    {
                        _fOVs[k] = int.Parse(CurrentDeviceInfo!.FOVValues[k]);
                    }
                }
            }

            //Derek 1210
            if (!isQAMPageViewModel_UIUpdateNotifyExist)
            {
                DdpmCommonHelper.DeviceManagerSA!.UIUpdateNotify += QAMPageViewModel_UIUpdateNotify;
                isQAMPageViewModel_UIUpdateNotifyExist = true;

                LogMsg($"Add event QAMPageViewModel_UIUpdateNotify, isQAMPageViewModel_UIUpdateNotifyExist={isQAMPageViewModel_UIUpdateNotifyExist}");                
            }
            LoadCurrentStatus();
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
                isStatusChagneByDDPM = true;

                ZoomValue = DdpmCommonHelper.DeviceManagerSA!.GetZoom(CurrentDeviceInfo!.ID.ToString()).Result;
                AutoFramingStatus = DdpmCommonHelper.DeviceManagerSA!.GetIsAutoFramingOn(CurrentDeviceInfo!.ID.ToString()).Result;

                FieldOfView = DdpmCommonHelper.DeviceManagerSA!.GetFieldOfView(CurrentDeviceInfo!.ID.ToString()).Result;

                LogMsg($"ZoomValue = {ZoomValue}, AutoFramingStatus = {AutoFramingStatus}, selFOVIdx = {FieldOfView}");

                if (FieldOfView != -1)
                    FOV_Selected(ChangeFOVToSelectIndex(FieldOfView));
            }
            catch (Exception e)
            {
                LogMsg($"Catch exception[{e.Message}]");
            }

            isStatusChagneByDDPM = false;
        }

        private int ChangeFOVToSelectIndex(int fov)
        {
            if (90 == fov)
                return 2;
            else if(78 == fov)
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
                        case "Webcam_ZoomChanged":
                            if (int.TryParse(eventMsg.NewValue, out currentValue))
                                isStatusChagneByDDPM = true;
                                ZoomValue = currentValue;

                            break;


                        case "Webcam_FieldOfViewChanged":
                            if (int.TryParse(eventMsg.NewValue, out currentValue))
                            {
                                isStatusChagneByDDPM = true;
                                FieldOfView = currentValue;
                                FOV_Selected(ChangeFOVToSelectIndex(FieldOfView));
                            }

                            break;


                        case "Webcam_IsAutoFramingOnChanged":
                            bool result = false;
                            if (bool.TryParse(eventMsg.NewValue, out result))
                            {
                                isStatusChagneByDDPM = true;
                                AutoFramingStatus = result;
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
                        if (SetProfile(msgs[1]))
                            LogMsg($"QAMPageViewModel_UIUpdateNotify: set webcam profile:{msgs[1]} successfully.");
                        else
                            LogMsg($"QAMPageViewModel_UIUpdateNotify: set webcam profile:{msgs[1]} fail!");
                    }
                }

            }
            catch (Exception ex)
            {
                LogMsg($"Catch exception in QAMPageViewModel_UIUpdateNotify: {ex.Message}");
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
        public void Settings_Selected(int index)
        {
            for (int j = 0; j < Settings_IsSelected.Length; j++)
            {
                Settings_IsSelected[j] = false;
            }
            Settings_IsSelected[index] = true;
            OnPropertyChanged(nameof(Settings_IsSelected));
        }
        #region Presets
        public ObservableCollection<UI_Profile> UI_ProfileList { get; set; }
        public Dictionary<string, WebcamProfile> Profiles = new Dictionary<string, WebcamProfile>();
        private WebcamProfile CurrentProfile;
        public void ImportWebcamProfiles(string model)
        {
            try
            {
                var filePath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @$"Dell\Dell Display and Peripheral Manager\WebcamSettings\{model}.json");
                
                if (File.Exists(filePath))
                {
                    Dictionary<string, WebcamProfile> presetProfiles = new();
                    //Dictionary<string, WebcamProfile> customProfiles = new();
                    string json = File.ReadAllText(filePath);
                    var jsonObject = Newtonsoft.Json.Linq.JObject.Parse(json);
                    string presetProfilesString = jsonObject["PresetProfiles"].ToString();
                    //Derek 1212
                    selectedProfileName = jsonObject["SelectedProfileName"].ToString();
                    LogMsg($"ImportWebcamProfiles current SelectedProfileName: {selectedProfileName}");

                    if (!string.IsNullOrEmpty(presetProfilesString))
                    {
                        presetProfiles = JsonConvert.DeserializeObject<Dictionary<string, WebcamProfile>>(presetProfilesString);

                        if (presetProfiles != null)
                        {
                            foreach (var profile in presetProfiles)
                            {
                                Profiles.Add(profile.Key, profile.Value);
                            }
                        }
                    }
                    //string customProfilesString = jsonObject["CustomProfiles"].ToString();
                    //if (!string.IsNullOrEmpty(customProfilesString))
                    //{
                    //    customProfiles = JsonConvert.DeserializeObject<Dictionary<string, WebcamProfile>>(customProfilesString);
                    //    if (customProfiles != null)
                    //    {
                    //        foreach (var profile in customProfiles)
                    //        {
                    //            Profiles.Add(profile.Key, profile.Value);
                    //        }
                    //    }
                    //}
                    UI_ProfileList = new ObservableCollection<UI_Profile>();
                    foreach (var profile in Profiles)
                    {
                        UI_ProfileList.Add(new UI_Profile
                        {
                            //Profile_Name = profile.Key,
                            Profile_Name = LangHelper.Instance[profile.Key],
                            Profile_Name_Key = profile.Key,
                        });
                    }
                }
            }
            catch (Exception e)
            {
                LogMsg($"Catch exception: {e.Message}");
            }
        }

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
                    
                LogMsg($"Could not found {name} in current UI_ProfileList.");

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

        public void SetProfile(UI_Profile CurrentProfileName)
        {
            try
            {
                if (Profiles.ContainsKey(CurrentProfileName.Profile_Name_Key))
                {
                    CurrentProfile = Profiles[CurrentProfileName.Profile_Name_Key];

                    if (CurrentDeviceInfo!.IsPropertyAutoFramingSensitivitySupported || CurrentDeviceInfo.IsPropertyAutoFramingSizeSupported || CurrentDeviceInfo.IsPropertyAutoFramingTransitionSupported)
                    {
                        bool result = DdpmCommonHelper.DeviceManagerSA!.SetIsAutoFramingOn(CurrentDeviceInfo!.ID.ToString(), CurrentProfile.IsAutoFramingOn).Result;
                        LogMsg($"SetProfile --> SetIsAutoFramingOn result is {result}");
                        _AutoFramingStatus = CurrentProfile.IsAutoFramingOn;

                        if (CurrentDeviceInfo.IsPropertyAutoFramingTransitionSupported)
                        {
                            result = DdpmCommonHelper.DeviceManagerSA!.SetIsAutoFramingTransitionOn(CurrentDeviceInfo!.ID.ToString(), CurrentProfile.IsAutoFramingTransitionOn).Result;

                            LogMsg($"SetProfile --> SetIsAutoFramingTransitionOn result is {result}");
                        }

                        if (CurrentDeviceInfo.IsPropertyAutoFramingSensitivitySupported)
                        {
                            result = DdpmCommonHelper.DeviceManagerSA!.SetAutoFramingSensitivity(CurrentDeviceInfo!.ID.ToString(), CurrentProfile.AutoFramingSensitivity).Result;
                            
                            LogMsg($"SetProfile --> SetAutoFramingSensitivity result is {result}");
                        }

                        if (CurrentDeviceInfo.IsPropertyAutoFramingSizeSupported)
                        {
                            result = DdpmCommonHelper.DeviceManagerSA!.SetAutoFramingFrameSize(CurrentDeviceInfo!.ID.ToString(), CurrentProfile.AutoFramingFrameSize).Result;

                            LogMsg($"SetProfile --> SetAutoFramingFrameSize result is {result}");
                        }
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
                    }

                    if (CurrentDeviceInfo.IsPropertyZoomSupported)
                    {
                        bool result = DdpmCommonHelper.DeviceManagerSA!.SetZoom(CurrentDeviceInfo!.ID.ToString(), CurrentProfile.Zoom).Result;
                        _ZoomValue = CurrentProfile.Zoom;

                        LogMsg($"SetProfile --> SetZoom result is {result}");
                    }

                    if (CurrentDeviceInfo.IsPropertyFocusSupported)
                    {
                        bool result = DdpmCommonHelper.DeviceManagerSA!.SetIsFocusOn(CurrentDeviceInfo!.ID.ToString(), CurrentProfile.IsFocusOn).Result;
                        LogMsg($"SetProfile --> SetIsFocusOn result is {result}");

                        result = DdpmCommonHelper.DeviceManagerSA!.SetFocus(CurrentDeviceInfo!.ID.ToString(), CurrentProfile.Focus).Result;
                        LogMsg($"SetProfile --> SetFocus result is {result}");
                    }

                    if (CurrentDeviceInfo.IsPropertyPrioritySupported)
                    {
                        bool result = DdpmCommonHelper.DeviceManagerSA!.SetPriority(CurrentDeviceInfo!.ID.ToString(), CurrentProfile.Priority).Result;
                        LogMsg($"SetProfile --> SetPriority result is {result}");
                    }

                    if (CurrentDeviceInfo.IsPropertyHDRSupported)
                    {
                        bool result = DdpmCommonHelper.DeviceManagerSA!.SetIsHDROn(CurrentDeviceInfo!.ID.ToString(), CurrentProfile.IsHDROn).Result;

                        LogMsg($"SetProfile --> SetIsHDROn result is {result}");
                    }

                    if (CurrentDeviceInfo.IsPropertyWhiteBalanceSupported)
                    {
                        bool result = DdpmCommonHelper.DeviceManagerSA!.SetIsAutoWhiteBalanceOn(CurrentDeviceInfo!.ID.ToString(), CurrentProfile.IsAutoWhiteBalanceOn).Result;
                        LogMsg($"SetProfile --> SetIsAutoWhiteBalanceOn result is {result}");

                        result = DdpmCommonHelper.DeviceManagerSA!.SetAutoWhiteBalance(CurrentDeviceInfo!.ID.ToString(), CurrentProfile.AutoWhiteBalance).Result;
                        LogMsg($"SetProfile --> SetAutoWhiteBalance result is {result}");
                    }

                    List<UI_Profile> temp = UI_ProfileList.ToList();
                    UI_ProfileList = new ObservableCollection<UI_Profile>();
                    foreach (var profile in temp)
                    {
                        profile.IsSelected = false;

                        if (profile.Profile_Name.Equals(CurrentProfileName.Profile_Name))
                        {
                            profile.IsSelected = true;
                            selectedProfileName = profile.Profile_Name_Key;
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

        private void SaveSelectProfile(string profileName)
        {
            try
            {
                //var filePath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @$"Dell\Dell Display and Peripheral Manager\WebcamSettings\{model}.json");

                //if (File.Exists(filePath))
                //{
                //    Dictionary<string, WebcamProfile> presetProfiles = new();
                //    //Dictionary<string, WebcamProfile> customProfiles = new();
                //    string json = File.ReadAllText(filePath);
                //    var jsonObject = Newtonsoft.Json.Linq.JObject.Parse(json);
                //    string presetProfilesString = jsonObject["PresetProfiles"].ToString();
                //    //Derek 1212
                //    selectedProfileName = jsonObject["SelectedProfileName"].ToString();
                //    LogMsg($"ImportWebcamProfiles current SelectedProfileName: {selectedProfileName}");
                //}
            }
            catch (Exception e)
            {
                LogMsg($"Catch execption: {e.Message} when SaveSelectProfile");
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

                if (!isStatusChagneByDDPM)
                {
                    bool result = DdpmCommonHelper.DeviceManagerSA!.SetIsAutoFramingOn(CurrentDeviceInfo!.ID.ToString(), _AutoFramingStatus).Result;

                    LogMsg($"QAM SetIsAutoFramingOn value to {_AutoFramingStatus}, result is {result}");
                }
                else
                    LogMsg($"QAM SetIsAutoFramingOn value has modified by UI");

                System.Windows.MessageBox.Show($"AutoFramingStatus -> {isStatusChagneByDDPM}");

                RefreshUI();

                OnPropertyChanged(nameof(AutoFramingStatus));
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

                if (!isStatusChagneByDDPM)
                {
                    bool result = DdpmCommonHelper.DeviceManagerSA!.SetFieldOfView(CurrentDeviceInfo!.ID.ToString(), _FieldOfView).Result;
                    
                    LogMsg($"QAM SetFieldOfView value to {_FieldOfView}, result is {result}");
                }
                else
                    LogMsg($"QAM SetFieldOfView value has modified by UI");
            }
        }
        public void FOV_Selected(int index)
        {
            for (int j = 0; j < FOV_IsSelected.Length; j++)
            {
                FOV_IsSelected[j] = false;
            }
            FOV_IsSelected[index] = true;
            OnPropertyChanged(nameof(FOV_IsSelected));
        }
        #endregion
        #region Zoom
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
            if (!isStatusChagneByDDPM)
            {
                bool result = DdpmCommonHelper.DeviceManagerSA!.SetZoom(CurrentDeviceInfo!.ID.ToString(), _ZoomValue).Result;
                LogMsg($"QAM set Zoom value to {_ZoomValue}, result is {result}");
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
        }
    }
    public class UI_Profile
    {
        public string Profile_Name { get; set; }
        public bool IsSelected { get; set; }
        public string Profile_Name_Key { get; set; } //Derek added for QAM PIMS-325190
    }
}