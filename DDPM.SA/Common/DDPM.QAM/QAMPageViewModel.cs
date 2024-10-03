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

namespace DDPM.QAM
{
    public class QAMPageViewModel : INotifyPropertyChanged
    {
        public string DeviceModel { get; set; }
        public bool[] IsEnable { get; set; } = { true, true, true, true };
        public bool[] Settings_IsSelected { get; set; } = { false, false, false, false };
        public string QAMPage_Height { get; set; } = "128";
        public string FullView_Height { get; set; } = "0";
        public int ZoomValue { get; set; }
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
                OnPropertyChanged(nameof(AutoFramingStatus_String));
            }
        }

        public string AutoFramingStatus_String
        {
            get
            {
                return AutoFramingStatus ? "ON" : "OFF";
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
            OnPropertyChanged(nameof(IsEnable));
            OnPropertyChanged(nameof(ZoomValue));
            OnPropertyChanged(nameof(QAMPage_Height));
            OnPropertyChanged(nameof(FullView_Height));
        }
    }
}
