using DDPM.SA.Common.Settings;
using DDPM.SA.Resources.Helper;
using Dell.Client.Framework.UX.WPF.Controls;
using Microsoft.Win32;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Management;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using VcpCore.Common;

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
        ManagementEventWatcher watcher;
        Logs _Logs;
        private bool isInstalling = false;

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
        public BitmapSource ProgressBarImage { get; set; }
        public string TextForeground { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public UpdateProgress(Logs logs)
        {
            InitializeComponent();
            DataContext = this;
            _Logs = logs;
            _Logs?.DebugMsg_1($"[UpdateProgress] UpdateProgress go");
            GetSystemTheme();
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

        public void HideWindow()
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(HideWindow);
                return;
            }
            Hide();
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
                UpdateTitle = $"{LangHelper.Instance["Software_Update"]} - {e.DeviceName}";
                UpdateSubTitle = LangHelper.Instance["Updating_Software_Do_not_power_off_this_PC"];
            }
            else
            {
                UpdateTitle = $"{LangHelper.Instance["Firmware_Update"]} - {e.DeviceName}";
                UpdateSubTitle = LangHelper.Instance["Updating_firmware_Do_not_remove_or_power_off_the_device_Leave_the_device_undisturbed"];
            }
            UpdateVersion = $"{LangHelper.Instance["Version"]} {e.TheLatestVersion}";
            if (e.ProcessName.Equals(LangHelper.Instance["Installing"]))
            {
                isInstalling = true;
                ProgressValue = (int)100;
                ProgressStr = $"{LangHelper.Instance["Processing"]}... {(int)e.ProcessProgress}%";
                if (!e.DeviceName.Equals("DDPM"))
                {
                    ProgressStr_2 = $"{LangHelper.Instance["DDPM_will_reopen_soon_after_update"]}";
                    ProgressStr_2_Color = TextForeground;
                }
                AlertVisibility = Visibility.Collapsed;
                Progress_IsAnimated = true;
            }
            else if (e.ProcessName.Equals(LangHelper.Instance["Downloading_and_installing"]))
            {
                isInstalling = false;
                ProgressValue = (int)e.ProcessProgress;
                ProgressStr = $"{LangHelper.Instance["Downloading_and_installing"]}... {ProgressValue}%";
                ProgressStr_2 = "";
                Progress_IsAnimated = false;
                AlertVisibility = Visibility.Collapsed;
            }
            else if (e.ProcessName.Equals(LangHelper.Instance["M1_Please_double_click_mouse_left_button_to_start_firmware_update"]) || e.ProcessName.Equals(LangHelper.Instance["M2_Please_press_key_on_keyboard_to_start_firmware_update"]))
            {
                isInstalling = false;
                ProgressValue = (int)e.ProcessProgress;
                ProgressStr = $"{LangHelper.Instance["Downloading_and_installing"]}... {ProgressValue}%";
                Progress_IsAnimated = false;
                AlertMessage = e.ProcessName;
                AlertVisibility = Visibility.Visible;
            }
            else if (e.ProcessName.Equals(LangHelper.Instance["Timeout"]) && !isInstalling)
            {
                ProgressStr_2 = $"{LangHelper.Instance["Unable_to_detect_target_device"]}… {(int)e.ProcessProgress}s";
                ProgressStr_2_Color = "#E6AC28";
            }
            _Logs?.DebugMsg_1($"[UpdateProgress] {nameof(_FWUpdatePlugin_ProgressUpdate)} {e.DeviceName} {e.TheLatestVersion} {e.ProcessName} {e.ProcessProgress} {DateTime.Now}");
        }
        int GetSystemTheme()
        {
            string key = @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize";
            string value = "AppsUseLightTheme";
            int ret = 0;
            try
            {
                using (RegistryKey regKey = Registry.CurrentUser.OpenSubKey(key))
                {
                    if (regKey != null)
                    {
                        object regValue = regKey.GetValue(value);
                        ret = (int)regValue;
                    }
                }
            }
            catch (Exception ex)
            {
                _Logs?.DebugMsg_1($"[UpdateProgress] Error reading registry: {ex.Message}");
            }
            _Logs?.DebugMsg_1($"[UpdateProgress] GetSystemTheme ret : {ret}");
            if (ret == 1)
            {
                ProgressBarImage = new BitmapImage(new Uri("ProgressBackground_Light.png", UriKind.RelativeOrAbsolute));
                TextForeground = "#0E0E0E";
            }
            else
            {
                ProgressBarImage = new BitmapImage(new Uri("ProgressBackground.png", UriKind.RelativeOrAbsolute));
                TextForeground = "#FFFFFF";
            }
            OnPropertyChanged("ProgressBarImage");
            OnPropertyChanged("TextForeground");
            return ret;
        }
    }
}