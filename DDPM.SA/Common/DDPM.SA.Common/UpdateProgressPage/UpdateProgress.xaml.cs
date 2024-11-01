using Dell.Client.Framework.UX.WPF.Controls;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;

namespace DDPM.SA.Common.UpdateProgressPage
{
    /// <summary>
    /// Interaction logic for UpdateProgress.xaml
    /// </summary>
    public partial class UpdateProgress : UXWindow, INotifyPropertyChanged
    {
        private string _UpdateTitle;
        private string _UpdateVersion;
        private string _UpdateSubTitle;
        private string _ProgressStr, _ProgressStr_2;
        private int _ProgressValue;
        private bool _Progress_IsAnimated;
        private string _AlertMessage;
        private Visibility _AlertVisibility = Visibility.Collapsed;
        private string _ProgressStr_2_Color;

        public string UpdateTitle
        {
            get { return _UpdateTitle; }
            set
            {
                _UpdateTitle = value;
                OnPropertyChanged(nameof(UpdateTitle));
            }
        }

        public string UpdateVersion
        {
            get { return _UpdateVersion; }
            set
            {
                _UpdateVersion = value;
                OnPropertyChanged(nameof(UpdateVersion));
            }
        }

        public string UpdateSubTitle
        {
            get { return _UpdateSubTitle; }
            set
            {
                _UpdateSubTitle = value;
                OnPropertyChanged(nameof(UpdateSubTitle));
            }
        }

        public string ProgressStr
        {
            get { return _ProgressStr; }
            set
            {
                _ProgressStr = value;
                OnPropertyChanged(nameof(ProgressStr));
            }
        }

        public string ProgressStr_2
        {
            get { return _ProgressStr_2; }
            set
            {
                _ProgressStr_2 = value;
                OnPropertyChanged(nameof(ProgressStr_2));
            }
        }

        public string ProgressStr_2_Color
        {
            get { return _ProgressStr_2_Color; }
            set
            {
                _ProgressStr_2_Color = value;
                OnPropertyChanged(nameof(ProgressStr_2_Color));
            }
        }

        public int ProgressValue
        {
            get { return _ProgressValue; }
            set
            {
                _ProgressValue = value;
                OnPropertyChanged(nameof(ProgressValue));
            }
        }

        public bool Progress_IsAnimated
        {
            get { return _Progress_IsAnimated; }
            set
            {
                _Progress_IsAnimated = value;
                OnPropertyChanged(nameof(Progress_IsAnimated));
            }
        }

        public string AlertMessage
        {
            get { return _AlertMessage; }
            set
            {
                _AlertMessage = value;
                OnPropertyChanged(nameof(AlertMessage));
            }
        }

        public Visibility AlertVisibility
        {
            get { return _AlertVisibility; }
            set
            {
                _AlertVisibility = value;
                OnPropertyChanged(nameof(AlertVisibility));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public UpdateProgress()
        {
            InitializeComponent();
            DataContext = this;
        }

        public void ShowWindow()
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(ShowWindow);
                return;
            }

            Show();
        }

        public void CloseWindow()
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(CloseWindow);
                return;
            }
            Close();
        }

        private void UXWindow_Loaded(object sender, RoutedEventArgs e)
        {
            this.Width = 800;
            this.Height = 440;
            this.Topmost = true;
            this.MinWidth = 800;
            this.MinHeight = 440;
            this.MaxWidth = 800;
            this.MaxHeight = 440;
            this.ResizeMode = ResizeMode.NoResize;
        }

        private void Grid_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }

        public void _FWUpdatePlugin_ProgressUpdate(object sender, UpdateProgressInfo e)
        {
            if (e.DeviceName.Equals("DDPM"))
            {
                UpdateTitle = "Software Update - " + e.DeviceName;
                UpdateSubTitle = "Updating Software. Do not power off this PC.";
            }
            else
            {
                UpdateTitle = "Firmware Update - " + e.DeviceName;
                UpdateSubTitle = "Updating firmware. Do not remove or power off the device. Leave the device undisturbed.";
            }
            UpdateVersion = $"Version {e.TheLatestVersion}";
            AlertVisibility = Visibility.Collapsed;
            if (e.ProcessName.Equals("Installing"))
            {
                ProgressValue = (int)100;
                ProgressStr = $"Processing... {(int)e.ProcessProgress}%";
                if (!e.DeviceName.Equals("DDPM"))
                {
                    ProgressStr_2 = $"DDPM will reopen soon after update";
                    ProgressStr_2_Color = "#FFFFFF";
                }
                Progress_IsAnimated = true;
            }
            else if (e.ProcessName.Equals("Downloading"))
            {
                ProgressValue = (int)e.ProcessProgress;
                ProgressStr = $"Downloading and installing... {ProgressValue}%";
                ProgressStr_2 = "";
                Progress_IsAnimated = false;
            }
            else if (e.ProcessName.Contains("keyboard") || e.ProcessName.Contains("mouse"))
            {
                ProgressValue = (int)e.ProcessProgress;
                ProgressStr = $"Downloading and installing... {ProgressValue}%";
                Progress_IsAnimated = false;
                AlertMessage = e.ProcessName;
                AlertVisibility = Visibility.Visible;
            }
            else if (e.ProcessName.Contains("Timeout"))
            {
                //ProgressValue = 0;
                //ProgressStr = $"Downloading and installing... 0%";
                ProgressStr_2 = $"Unable to detect target device… {ProgressValue}s";
                ProgressStr_2_Color = "#E6AC28";
                Progress_IsAnimated = false;
            }
            Debug.WriteLine(nameof(_FWUpdatePlugin_ProgressUpdate) + " " + e.ProcessName + " " + e.ProcessProgress + " " + DateTime.Now);
        }
    }
}