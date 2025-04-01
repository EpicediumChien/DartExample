using DDPM.QAM;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace DDPM.OSDs
{
    /// <summary>
    /// Interaction logic for QAMHotKeyWin.xaml
    /// </summary>
    public partial class QAMHotKeyWin : Window
    {
        /*private DispatcherTimer? animationTimer = null;
        private TimeSpan time;*/


        public QAMHotKeyWin()
        {
            InitializeComponent();
            DataContext = this;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //System.Windows.MessageBox.Show("Window_Loaded");

            System.Windows.Interop.WindowInteropHelper wndHelper = new System.Windows.Interop.WindowInteropHelper(this);
            Win32Lib.Win32.HideWinFromAltTab(wndHelper.Handle);

            //if (!string.IsNullOrWhiteSpace(showString))
            {
                /*this.WindowState = WindowState.Maximized;
                this.Topmost = true;*/

                //Derek 1209 OSD don't need to auto close
                //time = TimeSpan.FromMilliseconds(5000);
                //animationTimer = new DispatcherTimer();
                //animationTimer.Interval = TimeSpan.FromMilliseconds(1000);
                //animationTimer.Tick += RunTimerTick;
                //animationTimer.Start();
            }
            //else
            //{
            //    this.Close();
            //    return;
            //}
        }

        /*private void RunTimerTick(object sender, EventArgs e)
        {
            if (time == TimeSpan.Zero)
            {
                animationTimer?.Stop();
                this.Dispatcher.Invoke(() =>
                {
                    this.Close();
                });
            }
            else
            {
                time = time.Add(TimeSpan.FromMilliseconds(-1000));
            }
        }*/

        private void close_Click(object sender, MouseButtonEventArgs e)
        {
            this.Close();
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

        private void Window_Closed(object sender, EventArgs e)
        {
            if (System.Windows.Threading.Dispatcher.CurrentDispatcher != null)
            {
                System.Windows.Threading.Dispatcher.CurrentDispatcher.InvokeShutdown();
            }
        }
    }
}
