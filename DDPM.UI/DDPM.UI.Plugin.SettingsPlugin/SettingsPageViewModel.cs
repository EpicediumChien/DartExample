using CommunityToolkit.Mvvm.ComponentModel;
using DDPM.SA.Common;
using DDPM.UI.Common;
using DDPM.UI.Common.Interfaces;
using DDPM.UI.Common.Models;
using DPeMPublic.Common.Enums;
using System.Windows;
using System.Windows.Controls;

namespace DDPM.UI.Plugin.SettingsPlugin
{
    public class SettingsPageViewModel : ObservableObject, ISettingsPageViewModel
    {
        private List<HomeDevice> _homeDevices = new List<HomeDevice>();
        public bool[] IsSelected { get; set; } = new bool[5];

        public List<HomeDevice> HomeDevices
        {
            get => _homeDevices;
            set
            {
                SetProperty(ref _homeDevices, value);
                OnPropertyChanged("HomeDeviceCount");
            }
        }

        private HomeDevice? _selectedHomeDevice;

        public HomeDevice? SelectedHomeDevice
        {
            get => _selectedHomeDevice;
            set => SetProperty(ref _selectedHomeDevice, value);
        }

        public IModuleOwner? ModuleOwner { get; set; }
        private ContentControl? _fullView;

        public ContentControl? FullView
        {
            get => _fullView;
            set => SetProperty(ref _fullView, value);
        }

        //public ICommand? OpenFullViewCommand { get; set; }
        //public ICommand? CloseFullViewCommand { get; set; }
        public void SetSelected(int index)
        {
            for (int j = 0; j < IsSelected.Length; j++)
            {
                IsSelected[j] = false;
            }
            IsSelected[index] = true;
            OnPropertyChanged("IsSelected");
        }

        public void OpenFullView(ContentControl content)
        {
            //if (OpenFullViewCommand != null)
            //    OpenFullViewCommand?.Execute(this);
            FullView = content;
            FullView.Visibility = Visibility.Visible;
        }

        public void CloseFullView()
        {
            FullView = null;
        }

        public FWUpdateInfoPackage FWUpdateInfoPackage { get; set; }
        public SWUpdateInfoPackage SWUpdateInfoPackage { get; set; }
        public List<UIUpdateInfo> Critical_UpdateList_UI { get; set; }
        public List<UIUpdateInfo> Recommended_UpdateList_UI { get; set; }
        public List<UIUpdateInfo> Optional_UpdateList_UI { get; set; }
        public string LastCheckDate { get; set; }
        private bool _UpdatesPageUI_Enable;

        public bool UpdatesPageUI_Enable
        {
            get
            {
                _UpdatesPageUI_Enable = !DdpmCommonHelper.DeviceManagerSA.GetUILockStatus().Result;
                return _UpdatesPageUI_Enable;
            }
        }

        public string UpdateTitle { get; set; }
        public string UpdateVersion { get; set; }
        public string ProgressStr { get; set; }
        public int ProgressValue { get; set; }
        public bool Progress_IsAnimated { get; set; }

        public Visibility NoUpdateAlert
        {
            get => (Critical_UpdateList_UI?.Count <= 0 &&
                Recommended_UpdateList_UI?.Count <= 0 &&
                Optional_UpdateList_UI?.Count <= 0) ? Visibility.Visible : Visibility.Collapsed;
        }

        public Visibility NoNetwork { get => Visibility.Collapsed; }
        public Visibility Critical_UpdateList { get => Critical_UpdateList_UI?.Count >= 1 ? Visibility.Visible : Visibility.Collapsed; }
        public Visibility Recommended_UpdateList { get => Recommended_UpdateList_UI?.Count >= 1 ? Visibility.Visible : Visibility.Collapsed; }
        public Visibility Optional_UpdateList { get => Optional_UpdateList_UI?.Count >= 1 ? Visibility.Visible : Visibility.Collapsed; }

        public Visibility IsAnyUpdate
        {
            get => (Critical_UpdateList_UI?.Count >= 1 ||
                Recommended_UpdateList_UI?.Count >= 1 ||
                Optional_UpdateList_UI?.Count >= 1) ? Visibility.Visible : Visibility.Collapsed;
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

        public void RefreshUI()
        {
            OnPropertyChanged("Critical_UpdateList_UI");
            OnPropertyChanged("Recommended_UpdateList_UI");
            OnPropertyChanged("Optional_UpdateList_UI");
            OnPropertyChanged("LastCheckDate");
            OnPropertyChanged("UpdatesPageUI_Enable");
            OnPropertyChanged("NoUpdateAlert");
            OnPropertyChanged("NoNetwork");
            OnPropertyChanged("Critical_UpdateList");
            OnPropertyChanged("Recommended_UpdateList");
            OnPropertyChanged("Optional_UpdateList");
            OnPropertyChanged("IsAnyUpdate");
            OnPropertyChanged("LockMaskVisible");
        }

        public void RefreshProcessUI()
        {
            OnPropertyChanged("UpdateTitle");
            OnPropertyChanged("UpdateVersion");
            OnPropertyChanged("ProgressValue");
            OnPropertyChanged("Progress_IsAnimated");
            OnPropertyChanged("ProgressStr");
        }

        public void SetUpdateInfoUI(FWUpdateInfoPackage fwUpdateInfoPackage, SWUpdateInfoPackage swUpdateInfoPackage)
        {
            LastCheckDate = fwUpdateInfoPackage.TheLastCheckTime.ToString();
            FWUpdateInfoPackage = fwUpdateInfoPackage;
            SWUpdateInfoPackage = swUpdateInfoPackage;
            Critical_UpdateList_UI = new List<UIUpdateInfo>();
            Recommended_UpdateList_UI = new List<UIUpdateInfo>();
            Optional_UpdateList_UI = new List<UIUpdateInfo>();
            foreach (FWUpdateInfo fwUpdateInfo in fwUpdateInfoPackage.FWUpdateInfo)
            {
                UIUpdateInfo uiUpdateInfo = new UIUpdateInfo(fwUpdateInfo);
                if ((!uiUpdateInfo.IsEnableCheckBox) && uiUpdateInfo.IsCheckUpdate)//如果不能選擇是否更新為強制更新
                {
                    Critical_UpdateList_UI.Add(uiUpdateInfo);
                }
                else if (uiUpdateInfo.IsCheckUpdate)//如果為true為建議更新
                {
                    Recommended_UpdateList_UI.Add(uiUpdateInfo);
                }
                else//剩下的為選用更新
                {
                    Optional_UpdateList_UI.Add(uiUpdateInfo);
                }
            }
            foreach (SWUpdateInfo swUpdateInfo in swUpdateInfoPackage.SWUpdateInfo)
            {
                UIUpdateInfo uiUpdateInfo = new UIUpdateInfo(swUpdateInfo);
                if ((!uiUpdateInfo.IsEnableCheckBox) && uiUpdateInfo.IsCheckUpdate)
                {
                    Critical_UpdateList_UI.Add(uiUpdateInfo);
                }
            }
        }

        public bool IsCanUpdate()
        {
            if ((Critical_UpdateList_UI != null && Critical_UpdateList_UI.Count > 0) ||
                (Recommended_UpdateList_UI != null && Recommended_UpdateList_UI.Count > 0) ||
                (Optional_UpdateList_UI != null && Optional_UpdateList_UI.Count > 0))
            {
                return true;
            }
            return false;
        }

        public void SetVMUpdate()
        {
            List<FWUpdateInfo> fwUpdateInfos = new List<FWUpdateInfo>();
            List<SWUpdateInfo> swUpdateInfos = new List<SWUpdateInfo>();
            foreach (UIUpdateInfo uiUpdateInfo in Critical_UpdateList_UI)
            {
                if (uiUpdateInfo.IsCheckUpdate)
                {
                    if (uiUpdateInfo.SWUpdateInfo != null)
                    {
                        swUpdateInfos.Add(uiUpdateInfo.SWUpdateInfo);
                    }
                    else
                    {
                        fwUpdateInfos.Add(uiUpdateInfo.FWUpdateInfo);
                    }
                }
            }
            foreach (UIUpdateInfo uiUpdateInfo in Recommended_UpdateList_UI)
            {
                if (uiUpdateInfo.IsCheckUpdate)
                {
                    fwUpdateInfos.Add(uiUpdateInfo.FWUpdateInfo);
                }
            }
            foreach (UIUpdateInfo uiUpdateInfo in Optional_UpdateList_UI)
            {
                if (uiUpdateInfo.IsCheckUpdate)
                {
                    fwUpdateInfos.Add(uiUpdateInfo.FWUpdateInfo);
                }
            }
            FWUpdateInfoPackage.FWUpdateInfo.Clear();
            FWUpdateInfoPackage.FWUpdateInfo = fwUpdateInfos;
            SWUpdateInfoPackage.SWUpdateInfo.Clear();
            SWUpdateInfoPackage.SWUpdateInfo = swUpdateInfos;
        }
    }

    public class UIUpdateInfo
    {
        public bool IsCheckUpdate { get; set; }
        public bool IsEnableCheckBox { get; set; }
        public string UpdateInfo { get; set; }
        public FWUpdateInfo FWUpdateInfo { get; set; }
        public SWUpdateInfo SWUpdateInfo { get; set; }
        public Visibility UXAlertItemVisibility { get; set; }
        public string UXAlertItemMessage { get; set; }

        public UIUpdateInfo(FWUpdateInfo fwUpdateInfo)
        {
            //0614 Bruce 將原本DeviceType型態是字串改成跟IL一樣這樣可以直接使用IL提供的矩陣做判斷
            DeviceType[] CriticalUpdates = new DeviceType[] { DeviceType.PhysicalAudioDongle, DeviceType.PhysicalDongle },
                     RecommendedUpdates = new DeviceType[] { DeviceType.LogicalMouse, DeviceType.LogicalKeyboard, DeviceType.LogicalDock, DeviceType.PhysicalPen };
            FWUpdateInfo = fwUpdateInfo;
            this.IsCheckUpdate = true;
            this.IsEnableCheckBox = true;
            bool? b = null;
            switch (fwUpdateInfo.DeviceType)
            {
                case DeviceType.LogicalMouse:
                    UXAlertItemVisibility = Visibility.Visible;
                    UXAlertItemMessage = "Press a button or a key on the device to enable this update";
                    break;

                case DeviceType.LogicalKeyboard:
                    UXAlertItemVisibility = Visibility.Collapsed;
                    break;

                case DeviceType.LogicalDock:
                    UXAlertItemVisibility = Visibility.Visible;
                    UXAlertItemMessage = "Ensure only one dock is connected to your system. Devices connected to dock may not be available during update.";
                    break;

                case DeviceType.PhysicalPen:
                    UXAlertItemVisibility = Visibility.Visible;
                    UXAlertItemMessage = "Battery level on the device is low. Replace/recharge battery to enable this update.";
                    break;

                default:
                    UXAlertItemVisibility = Visibility.Collapsed;
                    UXAlertItemMessage = "";
                    break;
            }
            foreach (DeviceType s in CriticalUpdates)
            {
                if (fwUpdateInfo.DeviceType.Equals(s))
                {
                    b = true;
                    break;
                }
            }
            if (b == null)
            {
                foreach (DeviceType s in RecommendedUpdates)
                {
                    if (fwUpdateInfo.DeviceType.Equals(s))
                    {
                        b = false;
                        break;
                    }
                }
            }
            if (b == true)
            {
                this.IsEnableCheckBox = false;
            }
            else if (b == null)
            {
                this.IsCheckUpdate = false;
            }
            UpdateInfo = $"Firmware update {fwUpdateInfo.TheLatestVersion} - {fwUpdateInfo.DeviceName}";
        }

        public UIUpdateInfo(SWUpdateInfo swUpdateInfo)
        {
            SWUpdateInfo = swUpdateInfo;
            this.IsCheckUpdate = true;
            this.IsEnableCheckBox = false;
            UXAlertItemVisibility = Visibility.Collapsed;
            UpdateInfo = $"Software update {swUpdateInfo.TheLatestVersion} - {swUpdateInfo.SoftwareName}";
        }
    }
}