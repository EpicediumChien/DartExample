using DDPM.SA.Common;
using Dell.Client.Framework.Common;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Forms;
using System.Windows.Interop;
using Microsoft.Win32;
using System.Windows.Media.Imaging;

namespace DDPM.QAM
{
    /// <summary>
    /// Interaction logic for QAM.xaml
    /// </summary>
    public partial class QAMPage : Window
    {
        CameraSetting? CameraSetting;

        //public event EventHandler<UpdateUINotify> QAMUpdateUIHandler;

        [DllImport("user32.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        private static bool _ShowWindow(IntPtr hWnd, int nCmdShow)
        {
            bool rst = ShowWindow(hWnd, nCmdShow);

            if (!rst)
            {
                Debug.WriteLine("[QAMPage] Windows was hidden before.");
            }

            return rst;
        }
        [DllImport("user32.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        public enum log_type
        {
            info = 0,
            error
        }

        private ILog Log { get; set; }

        private static bool _SetForegroundWindow(IntPtr hWnd)
        {
            bool rst = SetForegroundWindow(hWnd);

            if (!rst)
            {
                Debug.WriteLine("[QAMPage] SetForegroundWindow failed.");
            }

            return rst;
        }
        /// <summary>
        /// NotificationFWupdate 呼叫DDPM UI事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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

                        //info DDPM navigate to webcam preview directly
                        //DdpmCommonHelper.DeviceManagerSA!.SetIsDDPMLaunchByQAMAsync(true);
                        //DdpmCommonHelper.DeviceManagerSA!.SetIsDDPMHomepageReadyAsync(true);

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

                        //Info SA that new DDPM instance launched by QAM
                        //DdpmCommonHelper.DeviceManagerSA!.SetIsDDPMLaunchByQAMAsync(true);
                    }
                }
                catch (Exception ex)
                {
                    WriteLog($"Catch exception[{ex.Message}]");
                    result = false;
                }
            }

            return result;
        }

        public QAMPage(IDeviceManagerSA deviceMangerPlugin, ILog log)
        {
            InitializeComponent();
            DdpmCommonHelper.DeviceManagerSA = deviceMangerPlugin;
            DdpmCommonHelper.Log = log;
            DdpmCommonHelper.QAMPageViewModel = new QAMPageViewModel(deviceMangerPlugin, log);
            DataContext = DdpmCommonHelper.QAMPageViewModel;

            Microsoft.Win32.SystemEvents.SessionSwitch += SystemEvents_SessionSwitch;
            Log = log;
        }

        private void SystemEvents_SessionSwitch(object sender, Microsoft.Win32.SessionSwitchEventArgs e)
        {
            if (e.Reason == Microsoft.Win32.SessionSwitchReason.SessionLock)
            {
                Microsoft.Win32.SystemEvents.SessionSwitch -= SystemEvents_SessionSwitch;

                CloseMyself();
            }
            //else if (e.Reason == Microsoft.Win32.SessionSwitchReason.SessionUnlock)
            //{
            //}
        }

        private void Close_Click(object sender, MouseButtonEventArgs e)
        {
            DdpmCommonHelper.DeviceManagerSA?.SetQAMOSDVisable(true);
            CloseMyself(true);
        }

        private void CloseMyself(bool closedByUser = false)
        {
            WriteLog($"CloseMyself start");

            try
            {
                if (CameraSetting != null)
                {
                    Dispatcher.Invoke(() =>
                    {
                        CameraSetting.Close();
                    });

                    CameraSetting = null;
                }

                if (closedByUser)
                {
                    this.Close();
                }
                else
                {
                    Dispatcher.Invoke(() =>
                    {
                        this.Close();
                    });
                }
            }
            catch (Exception e)
            {
                WriteLog($"Catch exception[{e.Message}]");
            }

            WriteLog($"CloseMyself end");
        }

        private void WriteLog(string text,
            [System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
            [System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "",
            [System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = 0,
            log_type log_type = log_type.info)
        {
            if (string.IsNullOrEmpty(text))
                text = "";

            text = $"[QAMPage] {text}, Caller Name:{memberName}, Source Line {sourceLineNumber}";
            Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss.fff") + " " + text);

            if (Log != null)
            {
                if (log_type == log_type.info)
                    Log.Info(text);
                else
                    Log.Error(text);
            }
        }

        private void CameraSetting_Click(object sender, MouseButtonEventArgs e)
        {
            if (CameraSetting != null)
            {
                CameraSetting.Close();
                CameraSetting = null;
            }
            else
            {
                CameraSetting = new CameraSetting(this);

                CameraSetting.Width = 288;
                CameraSetting.Height = 128;

                //Make sure CameraSetting & QAMPage in same screen
                var scalingRatio = Screen.PrimaryScreen.Bounds.Width / SystemParameters.PrimaryScreenWidth;
                System.Drawing.Point cursorPosition = System.Windows.Forms.Cursor.Position;
                Screen screen = Screen.FromPoint(cursorPosition);
                if (this.Left + this.Width + CameraSetting.Width > screen.Bounds.Right / scalingRatio)
                {
                    this.Left = screen.Bounds.Right / scalingRatio - this.Width - CameraSetting.Width;
                }

                CameraSetting.Left = this.Left + this.Width;
                CameraSetting.Top = this.Top;

                CameraSetting.Show();

                //Derek 2025/02/13 auto open present page
                CameraSetting.OpenPresetsFullView();
            }


            if (DataContext is QAMPageViewModel vm && vm.CurrentDeviceInfo != null)
                vm.IsCameraSettingSelected = !vm.IsCameraSettingSelected;
        }

        private void CallDDPM_Click(object sender, MouseButtonEventArgs e)
        {
            //ShowDDPM();
            //SendMessageToDDPM(10);

            if (ShowDDPM())
            {
                //Info SA that DDPM launched by QAM
                //DdpmCommonHelper.DeviceManagerSA!.SetIsDDPMLaunchByQAMAsync(true);

                //Close_Click(this, null); //Derek 1209
                WriteLog($"Lanuch DDPM successfully!");
            }
            else
            {
                WriteLog($"Going to run SetIsDDPMLaunchByQAMAsync()");
                DdpmCommonHelper.DeviceManagerSA?.SetIsDDPMLaunchByQAMAsync(false);
            }
        }

        //private void SendMessageToDDPM(int timeout)
        //{
        //    int i = 0;

        //    while (true)
        //    {
        //        Task.Delay(1000).Wait();
        //        ++i;

        //        //wait for homepage is available
        //        if (2000 == DdpmCommonHelper.DeviceManagerSA!.GetCurrentPollingRate().Result)
        //        {
        //            Thread.Sleep(2000);
        //            OnUpdateUINotify($"QAMEvent_StartPreview[{i}]");

        //            break;
        //        }

        //        if (i >= 10)
        //            break;
        //    }
        //}

        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                QAMPageViewModel? vm = DataContext as QAMPageViewModel;
                if (vm != null && vm.CurrentDeviceInfo != null)
                {
                    vm.IsDragging = true;
                }

                this.DragMove();
                if (CameraSetting != null)
                {
                    //Make sure CameraSetting & QAMPage in same screen
                    var scalingRatio = Screen.PrimaryScreen.Bounds.Width / SystemParameters.PrimaryScreenWidth;
                    System.Drawing.Point cursorPosition = System.Windows.Forms.Cursor.Position;
                    Screen screen = Screen.FromPoint(cursorPosition);
                    if (this.Left + this.Width + CameraSetting.Width > screen.Bounds.Right / scalingRatio)
                    {
                        this.Left = screen.Bounds.Right / scalingRatio - this.Width - CameraSetting.Width;
                    }

                    CameraSetting.Left = this.Left + this.Width;
                    CameraSetting.Top = this.Top;
                }

                if (vm != null && vm.CurrentDeviceInfo != null)
                {
                    vm.IsDragging = false;
                }
            }
        }

        //public void OnUpdateUINotify(string msg)
        //{
        //    UpdateUINotify e = new UpdateUINotify();
        //    e.UI_Field_Name = msg;

        //    QAMUpdateUIHandler?.Invoke(this, e);
        //}

        [DllImport("user32.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);
        public static bool _SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags)
        {
            bool rst = SetWindowPos(hWnd, hWndInsertAfter, X, Y, cx, cy, uFlags);

            if (!rst)
            {
                Debug.WriteLine("[QAMPage] SetWindowPos failed.");
            }

            return rst;
        }
        private static readonly IntPtr HWND_BOTTOM = new IntPtr(1);
        private const UInt32 SWP_NOSIZE = 0x0001;
        private const UInt32 SWP_NOMOVE = 0x0002;
        private const UInt32 SWP_NOACTIVATE = 0x0010;
        public void SetToBottomWindow()
        {
            IntPtr hWnd = new WindowInteropHelper(this).Handle;

            _SetWindowPos(hWnd, HWND_BOTTOM, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_NOACTIVATE);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                Dispatcher.Invoke(() =>
                {
                    CameraSetting?.Close();
                });

                CameraSetting = null;

                if (DataContext is QAMPageViewModel vm)
                    vm.RemoveQAMWebcamEvent();
            }
            catch (Exception ex)
            {
                WriteLog($"Catch exception {ex.Message} when QAM Window_Closing");
            }

        }

        //如果右边的window有出来，也要跟着隐藏  Derek 2025/01/23
        private void Window_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            //WriteLog($"Window_IsVisibleChanged --> NewValue =  {e.NewValue}, OldValue =  {e.OldValue}");

            bool isQAMVisible = true;
            try
            {
                bool.TryParse(e.NewValue.ToString(), out isQAMVisible);

                if (isQAMVisible && CameraSetting != null)
                {
                    Dispatcher.Invoke(() =>
                    {
                        CameraSetting?.Show();

                        WriteLog($"Window_IsVisibleChanged show QAM setting window");
                    });
                }
                else if (CameraSetting != null)
                {
                    Dispatcher.Invoke(() =>
                    {
                        CameraSetting?.Hide();

                        WriteLog($"Window_IsVisibleChanged hide QAM setting window");
                    });
                }
            }
            catch (Exception ex)
            {
                WriteLog($"Window_IsVisibleChanged catch exception; {ex.Message}");
            }
        }
    }
}