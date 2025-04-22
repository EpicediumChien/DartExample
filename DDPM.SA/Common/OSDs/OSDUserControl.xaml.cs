using CommunityToolkit.Mvvm.ComponentModel;
using DDPM.QAM;
using DDPM.SA.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace DDPM.OSDs
{
    public class OSDWinInfo : ObservableObject
    {
        public required string GUID { get; set; }
        public OSDType_Device OSDType_Device { get; set; }
        public OSDType_Op OSDType_Op { get; set; } = OSDType_Op.None;
        public required string ShowStringTitle { get; set; }
        public required string showStringContent = string.Empty;
        public string ShowStringContent
        {
            get { return showStringContent; }
            set
            {
                showStringContent = value;
                OnPropertyChanged("ShowStringContent");
            }
        }

        private bool isFadeOut = false;
        public bool IsFadeOut
        {
            get { return isFadeOut; }
            set
            {
                isFadeOut = value;
                OnPropertyChanged("IsFadeOut");
            }
        }
    }

    /// <summary>
    /// Interaction logic for OSDUserControl.xaml
    /// </summary>
    public partial class OSDUserControl : UserControl
    {
        private DispatcherTimer? animationTimer = null;
        private TimeSpan time;

        public static readonly RoutedEvent OSDUserControl_Closed = EventManager.RegisterRoutedEvent(
           "OSDUserControl_Closed", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(OSDUserControl));

        public event RoutedEventHandler OSDUserControl_ClosedHanddler
        {
            add { AddHandler(OSDUserControl_Closed, value); }
            remove { RemoveHandler(OSDUserControl_Closed, value); }
        }

        protected void RaiseMyCustomEvent()
        {
            RoutedEventArgs args = new RoutedEventArgs(OSDUserControl_Closed);
            RaiseEvent(args);
        }

        public string GUID
        {
            get { return (string)GetValue(GUIDProperty); }
            set { SetValue(GUIDProperty, value); }
        }

        // Using a DependencyProperty as the backing store for GUID.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty GUIDProperty =
            DependencyProperty.Register("GUID", typeof(string), typeof(OSDUserControl), new PropertyMetadata(""));


        public OSDType_Device OSDType_Device
        {
            get { return (OSDType_Device)GetValue(OSDType_DeviceProperty); }
            set { SetValue(OSDType_DeviceProperty, value); }
        }

        // Using a DependencyProperty as the backing store for OSDType.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty OSDType_DeviceProperty =
            DependencyProperty.Register("OSDType_Device", typeof(OSDType_Device), typeof(OSDUserControl), new PropertyMetadata(OSDType_Device.Unknown));


        public string ShowStringTitle
        {
            get { return (string)GetValue(ShowStringTitleProperty); }
            set { SetValue(ShowStringTitleProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ShowStringTitle.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ShowStringTitleProperty =
            DependencyProperty.Register("ShowStringTitle", typeof(string), typeof(OSDUserControl), new PropertyMetadata(""));


        public string ShowStringContent
        {
            get { return (string)GetValue(ShowStringContentProperty); }
            set { SetValue(ShowStringContentProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ShowStringContent.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ShowStringContentProperty =
            DependencyProperty.Register("ShowStringContent", typeof(string), typeof(OSDUserControl), new PropertyMetadata(""));

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public bool IsFadeOut
        {
            get { return (bool)GetValue(IsFadeOutProperty); }
            set { SetValue(IsFadeOutProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsFadeOut.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsFadeOutProperty =
            DependencyProperty.Register("IsFadeOut", typeof(bool), typeof(OSDUserControl), new PropertyMetadata(false, OnPropertyChanged));

        private static void OnPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            OSDUserControl osdUserControl = (OSDUserControl)d;
            osdUserControl.InvokeFadeOutAnimation();
        }

        public ICommand? Closed
        {
            get { return (ICommand)GetValue(ClosedProperty); }
            set { SetValue(ClosedProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Closed.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ClosedProperty =
            DependencyProperty.Register("Closed", typeof(ICommand), typeof(OSDUserControl), new PropertyMetadata(null));

        public OSDUserControl()
        {
            InitializeComponent();
            //DataContext = this;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            /* time = TimeSpan.FromMilliseconds(3000);
             animationTimer = new DispatcherTimer();
             animationTimer.Interval = TimeSpan.FromMilliseconds(1000);
             animationTimer.Tick += RunTimerTick;
             animationTimer.Start();*/
            switch (OSDType_Device)
            {
                case OSDType_Device.QAM:
                    time = TimeSpan.FromMilliseconds(5000);
                    break;
                case OSDType_Device.EasyMemory:
                    time = TimeSpan.FromMilliseconds(3000);
                    break;
                case OSDType_Device.FingerPrint:
                    time = TimeSpan.FromMilliseconds(5000);
                    break;
                case OSDType_Device.CapsLockOn:
                    time = TimeSpan.FromMilliseconds(3000);
                    break;
                case OSDType_Device.CapsLockOff:
                    time = TimeSpan.FromMilliseconds(3000);
                    break;
                case OSDType_Device.NumLockOn:
                    time = TimeSpan.FromMilliseconds(3000);
                    break;
                case OSDType_Device.NumLockOff:
                    time = TimeSpan.FromMilliseconds(3000);
                    break;
                case OSDType_Device.ScrollLockOn:
                    time = TimeSpan.FromMilliseconds(3000);
                    break;
                case OSDType_Device.ScrollLockOff:
                    time = TimeSpan.FromMilliseconds(3000);
                    break;
                case OSDType_Device.Mute:
                    time = TimeSpan.FromMilliseconds(3000);
                    break;
                case OSDType_Device.UnMute:
                    time = TimeSpan.FromMilliseconds(3000);
                    break;
                case OSDType_Device.StartRecording:
                    time = TimeSpan.FromMilliseconds(3000);
                    break;
                case OSDType_Device.WalkAwayLock:
                    time = TimeSpan.FromMilliseconds(5000);
                    break;
                default:
                    return;
            }
            //RaiseMyCustomEvent();
            animationTimer = new DispatcherTimer();
            animationTimer.Interval = TimeSpan.FromMilliseconds(1000);
            animationTimer.Tick += RunTimerTick;
            animationTimer.Start();
        }
        private void RunTimerTick(object? sender, EventArgs e)
        {
            if (time == TimeSpan.Zero)
            {
                animationTimer?.Stop();
                this.Dispatcher.Invoke(() =>
                {
                    Closed?.Execute(GUID);
                });
            }
            else
            {
                time = time.Add(TimeSpan.FromMilliseconds(-1000));
                switch (OSDType_Device)
                {
                    case OSDType_Device.StartRecording:
                        this.Dispatcher.Invoke(() =>
                        {
                            this.ShowStringContent = (Convert.ToInt32(this.ShowStringContent) - 1).ToString();
                            OnPropertyChanged("ShowStringContent");
                        });
                        break;
                    case OSDType_Device.WalkAwayLock:
                        this.Dispatcher.Invoke(() =>
                        {
                            this.ShowStringContent = (Convert.ToInt32(this.ShowStringContent) - 1).ToString();
                            OnPropertyChanged("ShowStringContent");
                        });
                        break;
                }
            }
        }
        private void InvokeFadeOutAnimation()
        {
            this.Dispatcher.Invoke(() =>
            {
                Storyboard? sb = Resources["FadeOut"] as Storyboard;
                if (sb == null)
                    return;

                sb.Completed += (o, s) =>
                {
                    Closed?.Execute(GUID);
                };

                sb.Begin();
            });
        }

        public void StopFadeOutAnimation()
        {
            this.Dispatcher.Invoke(() =>
            {
                Storyboard? sb = Resources["FadeOut"] as Storyboard;

                if (sb == null)
                    return;

                sb.Stop();
            });
        }

        private void close_Click(object sender, MouseButtonEventArgs e)
        {
            Closed?.Execute(GUID);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //System.Windows.MessageBox.Show("Button_MouseLeftButtonDown");
            try
            {
                //PIMS 332041 need launch DDPM if DDPM not launched
                if (ShowDDPM())
                {
                    DdpmCommonHelper.DeviceManagerSA!.WriteLog($"QAM OSD Lanuch DDPM successfully!");
                }
                else
                    DdpmCommonHelper.DeviceManagerSA!.WriteLog($"OAM OSD launch DDPM fail");
            }
            catch (Exception ex)
            {
                DdpmCommonHelper.DeviceManagerSA!.WriteLog($"OAM OSD launch DDPM catch exception: {ex.Message}");
            }
        }
        [DllImport("user32.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        private static bool _ShowWindow(IntPtr hWnd, int nCmdShow)
        {
            bool rst = ShowWindow(hWnd, nCmdShow);

            if (!rst)
            {
#if DEBUG
                Console.WriteLine("[QAMHotKeyWin] Windos was hidden before.");
#endif
            }

            return rst;
        }
        [DllImport("user32.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        private static extern bool SetForegroundWindow(IntPtr hWnd);
        private static bool _SetForegroundWindow(IntPtr hWnd)
        {
            bool rst = SetForegroundWindow(hWnd);

            if (!rst)
            {
#if DEBUG
                Console.WriteLine("[QAMHotKeyWin] SetForegroundWindow failed.");
#endif
            }

            return rst;
        }

        private bool ShowDDPM()
        {
            const int SW_SHOWNORMALSW_NORMAL = 1;
            string processName = "DDPM";
            bool result = false;

            if (OperatingSystem.IsWindows())
            {
                try
                {
                    Process[] processes = Process.GetProcessesByName(processName);

                    if (processes.Length > 0)
                    {
                        IntPtr mainWindowHandle = processes[0].MainWindowHandle;
                        // 將窗口最大化
                        _ShowWindow(mainWindowHandle, SW_SHOWNORMALSW_NORMAL);
                        // 顯示到前景
                        _SetForegroundWindow(mainWindowHandle);

                        //info DDPM navigate to widget setting page directly
                        DdpmCommonHelper.DeviceManagerSA!.SetIsWidgetSettingPageLoadedByQAMAsync(true);

                        result = true;
                    }
                    else
                    {
                        string ddpmExePath = @"C:\Program Files\Dell\Dell Display and Peripheral Manager\DDPM.exe";
                        //string debugPath = "D:\\DDPM\\DDPM.UI\\bin\\net8.0-windows10.0.19041.0\\DDPM.exe";

                        result = DDPM.SA.Common.Settings.DDPMFileSecurity.StartProcessSafely(
                            null,
                            new ProcessStartInfo
                            {
                                FileName = ddpmExePath,
                                UseShellExecute = true
                            });

                        //Info SA that new DDPM instance launched by OSD
                        DdpmCommonHelper.DeviceManagerSA!.SetIsDDPMLaunchByQAMAsync(true);
                    }
                }
                catch (Exception ex)
                {
                    DdpmCommonHelper.DeviceManagerSA!.WriteLog($"Catch exception[{ex.Message}]");

                    result = false;
                }
            }

            return result;
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            if (animationTimer != null)
            {
                animationTimer.Stop();
                animationTimer.Tick -= RunTimerTick;
                animationTimer = null;
            }
        }
    }
}
