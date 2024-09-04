using CommunityToolkit.Mvvm.ComponentModel;
using DDPM.SA.Common;
using DDPM.UI.Common;
using DDPM.UI.Common.Interfaces;
using DDPM.UI.Common.Models;
using DPeMPublic.Common.Enums;
using System.Windows;
using System.Windows.Controls;

namespace DDPM.UI.Plugin.WalkThroughPlugin
{
    public class WalkThroughPageViewModel : ObservableObject
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

        private bool _UpdatesPageUI_Enable;

        public bool UpdatesPageUI_Enable
        {
            get
            {
                _UpdatesPageUI_Enable = !DdpmCommonHelper.DeviceManagerSA.GetUILockStatus().Result;
                return _UpdatesPageUI_Enable;
            }
        }
    }
}