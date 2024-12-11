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

namespace DDPM.QAM
{
    public class QAMPageViewModel : INotifyPropertyChanged
    {
        public DeviceInfo CurrentDeviceInfo { get; set; }
        public string DeviceModel { get; set; }
        public bool[] Settings_IsEnable { get; set; } = { true, true, true, true };
        public bool[] Settings_IsSelected { get; set; } = { false, false, false, false };
        public Visibility[] Settings_IsVisibility { get; set; } = { Visibility.Visible, Visibility.Visible, Visibility.Visible, Visibility.Visible };

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
        public QAMPageViewModel()
        {
            List<DeviceInfo> deviceInfos = DdpmCommonHelper.DeviceManagerSA!.GetDevices().Result.deviceInfo;
            if (deviceInfos != null && deviceInfos.Count > 0)
            {
                CurrentDeviceInfo = deviceInfos.FirstOrDefault(x => (x.PhysicalDeviceType.Equals(DeviceType.LogicalWebcam) || x.PhysicalDeviceType.Equals(DeviceType.PhysicalWebcam)));
                if (CurrentDeviceInfo != null)
                {
                    DeviceModel = CurrentDeviceInfo.Name;
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
            DdpmCommonHelper.DeviceManagerSA!.UIUpdateNotify += QAMPageViewModel_UIUpdateNotify;
            LoadCurrentStatus();
        }

        private void LoadCurrentStatus()
        {
            try
            {
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
                                ZoomValue = currentValue;

                        break;


                        case "Webcam_FieldOfViewChanged":
                            if (int.TryParse(eventMsg.NewValue, out currentValue))
                            {
                                FieldOfView = currentValue;
                                FOV_Selected(ChangeFOVToSelectIndex(FieldOfView));
                            }

                            break;


                        case "Webcam_IsAutoFramingOnChanged":
                            bool result = false;
                            if (bool.TryParse(eventMsg.NewValue, out result))
                                AutoFramingStatus = result;
                            break;
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
                DdpmCommonHelper.DeviceManagerSA!.WriteLog($"Catch exception: {e.Message}");
            }
        }

        public void SetProfile(UI_Profile CurrentProfileName)
        {
            if (Profiles.ContainsKey(CurrentProfileName.Profile_Name_Key))
            {
                CurrentProfile = Profiles[CurrentProfileName.Profile_Name_Key];

                if (CurrentDeviceInfo!.IsPropertyAutoFramingSensitivitySupported || CurrentDeviceInfo.IsPropertyAutoFramingSizeSupported || CurrentDeviceInfo.IsPropertyAutoFramingTransitionSupported)
                {
                    DdpmCommonHelper.DeviceManagerSA!.SetIsAutoFramingOn(CurrentDeviceInfo!.ID.ToString(), CurrentProfile.IsAutoFramingOn);
                    _AutoFramingStatus = CurrentProfile.IsAutoFramingOn;
                    if (CurrentDeviceInfo.IsPropertyAutoFramingTransitionSupported)
                    {
                        DdpmCommonHelper.DeviceManagerSA!.SetIsAutoFramingTransitionOn(CurrentDeviceInfo!.ID.ToString(), CurrentProfile.IsAutoFramingTransitionOn);
                    }
                    if (CurrentDeviceInfo.IsPropertyAutoFramingSensitivitySupported)
                    {
                        DdpmCommonHelper.DeviceManagerSA!.SetAutoFramingSensitivity(CurrentDeviceInfo!.ID.ToString(), CurrentProfile.AutoFramingSensitivity);
                    }
                    if (CurrentDeviceInfo.IsPropertyAutoFramingSizeSupported)
                    {
                        DdpmCommonHelper.DeviceManagerSA!.SetAutoFramingFrameSize(CurrentDeviceInfo!.ID.ToString(), CurrentProfile.AutoFramingFrameSize);
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
                    DdpmCommonHelper.DeviceManagerSA!.SetZoom(CurrentDeviceInfo!.ID.ToString(), CurrentProfile.Zoom);
                    _ZoomValue = CurrentProfile.Zoom;
                }
                if (CurrentDeviceInfo.IsPropertyFocusSupported)
                {
                    DdpmCommonHelper.DeviceManagerSA!.SetIsFocusOn(CurrentDeviceInfo!.ID.ToString(), CurrentProfile.IsFocusOn);
                    DdpmCommonHelper.DeviceManagerSA!.SetFocus(CurrentDeviceInfo!.ID.ToString(), CurrentProfile.Focus);
                }
                if (CurrentDeviceInfo.IsPropertyPrioritySupported)
                {
                    DdpmCommonHelper.DeviceManagerSA!.SetPriority(CurrentDeviceInfo!.ID.ToString(), CurrentProfile.Priority);
                }
                if (CurrentDeviceInfo.IsPropertyHDRSupported)
                {
                    DdpmCommonHelper.DeviceManagerSA!.SetIsHDROn(CurrentDeviceInfo!.ID.ToString(), CurrentProfile.IsHDROn);
                }
                if (CurrentDeviceInfo.IsPropertyWhiteBalanceSupported)
                {
                    DdpmCommonHelper.DeviceManagerSA!.SetIsAutoWhiteBalanceOn(CurrentDeviceInfo!.ID.ToString(), CurrentProfile.IsAutoWhiteBalanceOn);
                    DdpmCommonHelper.DeviceManagerSA!.SetAutoWhiteBalance(CurrentDeviceInfo!.ID.ToString(), CurrentProfile.AutoWhiteBalance);
                }
                List<UI_Profile> temp = UI_ProfileList.ToList();
                UI_ProfileList = new ObservableCollection<UI_Profile>();
                foreach (var profile in temp)
                {
                    profile.IsSelected = false;
                    if (profile.Profile_Name.Equals(CurrentProfileName.Profile_Name))
                    {
                        profile.IsSelected = true;
                    }
                    UI_ProfileList.Add(profile);
                }
                RefreshUI();
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
                DdpmCommonHelper.DeviceManagerSA!.SetIsAutoFramingOn(CurrentDeviceInfo!.ID.ToString(), _AutoFramingStatus);
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
                DdpmCommonHelper.DeviceManagerSA!.SetFieldOfView(CurrentDeviceInfo!.ID.ToString(), _FieldOfView);
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
            DdpmCommonHelper.DeviceManagerSA!.SetZoom(CurrentDeviceInfo!.ID.ToString(), _ZoomValue);
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