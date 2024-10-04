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

namespace DDPM.QAM
{
    public class QAMPageViewModel : INotifyPropertyChanged
    {
        public DeviceInfo CurrentDeviceInfo { get; set; }
        public string DeviceModel { get; set; }
        public bool[] Settings_IsEnable { get; set; } = { true, true, true, true };
        public bool[] Settings_IsSelected { get; set; } = { false, false, false, false };
        public Visibility[] Settings_IsVisibility { get; set; } = { Visibility.Visible, Visibility.Visible, Visibility.Visible, Visibility.Visible };
        public string QAMPage_Height { get; set; } = "128";
        public string FullView_Height { get; set; } = "0";
        public Visibility[] FOV_Visibility { get; set; } = { Visibility.Collapsed, Visibility.Collapsed, Visibility.Collapsed };
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
                OnPropertyChanged();
            }
        }
        public void SetZoom()
        {
            DdpmCommonHelper.DeviceManagerSA!.SetZoom(CurrentDeviceInfo!.ID.ToString(), _ZoomValue);
        }
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
        public new event PropertyChangedEventHandler? PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        private ContentControl? _fullView;

        public ContentControl? FullView
        {
            get => _fullView;
            set => _fullView = value;
        }
        bool _AutoFramingStatus;
        public bool AutoFramingStatus
        {
            get => _AutoFramingStatus;
            set
            {
                _AutoFramingStatus = value;
                DdpmCommonHelper.DeviceManagerSA!.SetIsAutoFramingOn(CurrentDeviceInfo!.ID.ToString(), _AutoFramingStatus);
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
                RefreshUI();
            }
        }
        public QAMPageViewModel()
        {
            List<DeviceInfo> deviceInfos = DdpmCommonHelper.DeviceManagerSA.GetDevices().Result.deviceInfo;
            if (deviceInfos != null && deviceInfos.Count > 0)
            {
                CurrentDeviceInfo = deviceInfos.FirstOrDefault(x => (x.PhysicalDeviceType.Equals(DeviceType.LogicalWebcam) || x.PhysicalDeviceType.Equals(DeviceType.PhysicalWebcam)));
                DeviceModel = CurrentDeviceInfo.Name;
                ZoomMax = CurrentDeviceInfo.ZoomMax;
                ZoomMin = CurrentDeviceInfo.ZoomMin;
                if (CurrentDeviceInfo.IsPropertyAutoFramingSupported)
                {
                    Settings_IsVisibility[1] = Visibility.Collapsed;
                }
                for (int k = 0; k < CurrentDeviceInfo!.FOVValues.Length; k++)
                {
                    _fOVs[k] = int.Parse(CurrentDeviceInfo!.FOVValues[k]);
                }
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
        public void RefreshUI()
        {
            OnPropertyChanged(nameof(DeviceModel));
            OnPropertyChanged(nameof(Settings_IsEnable));
            OnPropertyChanged(nameof(ZoomValue));
            OnPropertyChanged(nameof(QAMPage_Height));
            OnPropertyChanged(nameof(FullView_Height));
        }
    }
}
