using DDPM.SA.Common;
using Dell.Client.Framework.Common;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Forms;
using System.Windows.Interop;

namespace DDPM.QAM
{
    /// <summary>
    /// Interaction logic for QAM.xaml
    /// </summary>
    public partial class QAMPage : Window
    {
        CameraSetting? CameraSetting;
        //Derek 2025/04/11 add timer to monitor zoom meeting window state for PIMS-351206
        private System.Threading.Timer? timer = null;
        private bool _isTopmost = true;
        private int fullScreenModeGetFailCount = 0;

        //public event EventHandler<UpdateUINotify> QAMUpdateUIHandler;
        public enum log_type
        {
            info = 0,
            error
        }

        private ILog Log { get; set; }

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

#if DEBUG
                        ddpmExePath = $"{AppContext.BaseDirectory}..\\..\\..\\..\\..\\DDPM.UI\\bin\\net8.0-windows10.0.19041.0\\DDPM.exe";
#endif

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
            //timer = new System.Threading.Timer(TimerCallback, null, 5000, 3000);
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

                //Derek 2025/04/11
                DdpmCommonHelper.QAMPageViewModel = null;
                if (timer != null)
                {
                    try
                    {
                        timer.Dispose();
                    }
                    catch (Exception ex)
                    {
                        WriteLog($"Exception while disposing timer: {ex.Message}");
                    }
                    finally
                    {
                        timer = null;
                    }
                }
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

        #region Get Full Screen State of Zoom and move to bottom of layer
        // Structure to hold window's position and size
        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        private void TimerCallback(object? state)
        {
            MonitorZoomMeetingWindowState();
        }

        private void MonitorZoomMeetingWindowState()
        {
            string processName = "Zoom"; //"notepad";
            Process[] processes = Process.GetProcessesByName(processName);

            if (processes.Length > 0)
            {
                foreach (Process process in processes)
                {
                    //System.Windows.MessageBox.Show($"{process.Id}， {process.MainWindowHandle}，{process.MainWindowTitle}, {process.MainModule?.FileName}, {process.MainModule?.ModuleName}");
                    try
                    {
                        IntPtr hwnd = process.MainWindowHandle;
                        if (hwnd != IntPtr.Zero)
                        {
                            RECT rect = new RECT();
                            // Get the window's position and size
                            GetWindowRect(hwnd, ref rect);

                            // Get screen size (Working area of the screen excluding taskbar)
                            var screen = Screen.PrimaryScreen?.Bounds ?? new System.Drawing.Rectangle();

                            // Compare window size to screen size (not considering the position)
                            int windowWidth = rect.Right - rect.Left;
                            int windowHeight = rect.Bottom - rect.Top;

                            if (Math.Abs(windowWidth - screen.Width) <= 10 && Math.Abs(windowHeight - screen.Height) <= 10
                                && _isTopmost)
                            {
                                fullScreenModeGetFailCount = 0;
                                WriteLog($"[MonitorZoomMeetingWindowState] Zoom is in full-screen mode!");
                                SetToBottomWindow();
                            }
                            else
                            {
                                if (fullScreenModeGetFailCount > 3 && !_isTopmost)
                                {
                                    WriteLog($"[MonitorZoomMeetingWindowState] Zoom is NOT in full-screen mode.");
                                    SetToTopWindow();
                                }
                                fullScreenModeGetFailCount++;
                            }
                        }
                        else
                        {
                            WriteLog($"[MonitorZoomMeetingWindowState] Process {processName} doesn't have window.");
                        }
                    }
                    catch (Exception ex)
                    {
                        WriteLog($"[MonitorZoomMeetingWindowState] Throws exception on process [{processName}] {ex.ToString()}, StackTrace: {ex.StackTrace}.");
                    }
                    finally
                    {
                        process.Dispose();
                    }
                }
            }
        }

        #region Win32API
        [DllImport("user32.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        public static IntPtr _FindWindow(string lpClassName, string lpWindowName)
        {
            IntPtr rst = FindWindow(lpClassName, lpWindowName);

            if (rst == IntPtr.Zero)
            {
#if DEBUG
                Console.WriteLine("[CallUser32dll] FindWindow failed.");
#endif
            }

            return rst;
        }

        [DllImport("user32.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        private static extern bool GetWindowPlacement(IntPtr hWnd, ref WINDOWPLACEMENT lpwndpl);

        // 定义WINDOWPLACEMENT结构体
        [Serializable]
        [StructLayout(LayoutKind.Sequential)]
        public struct WINDOWPLACEMENT
        {
            public int length;
            public int flags;
            public int showCmd;
            public System.Drawing.Point ptMinPosition;
            public System.Drawing.Point ptMaxPosition;
            public System.Drawing.Rectangle rcNormalPosition;
        }

        const int SW_SHOWMINIMIZED = 2; // 最小化
        const int SW_SHOWMAXIMIZED = 3; // 最大化
        const int SW_SHOWNORMAL = 1;    // 正常显示

        public static int _GetWindowPlacement(IntPtr hWnd)
        {
            try
            {
                WINDOWPLACEMENT wp = new WINDOWPLACEMENT();
                wp.length = Marshal.SizeOf(wp);

                if (GetWindowPlacement(hWnd, ref wp))
                {
                    //switch (wp.showCmd)
                    //{
                    //    case SW_SHOWMINIMIZED:
                    //        Console.WriteLine($"窗口处于最小化状态。");
                    //        break;
                    //    case SW_SHOWMAXIMIZED:
                    //        Console.WriteLine($"窗口处于最大化状态。");
                    //        break;
                    //    case SW_SHOWNORMAL:
                    //        Console.WriteLine($"窗口处于正常显示状态。");
                    //        break;

                    //    default:
                    //        Console.WriteLine($"窗口状态未知。");
                    //        break;
                    //}
                    return wp.showCmd;
                }
                else
                {
                    Console.WriteLine($"无法获取窗口状态。");
                    return -1;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"_GetWindowPlacement catch exception; {e.Message}");

                return -2;
            }
            
        }

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
        private static readonly IntPtr HWND_TOP = new IntPtr(0);
        private const UInt32 SWP_NOSIZE = 0x0001;
        private const UInt32 SWP_NOMOVE = 0x0002;
        private const UInt32 SWP_NOACTIVATE = 0x0010;

        public void SetToBottomWindow()
        {
            Dispatcher.Invoke(() =>
            {
                this._isTopmost = false;
                this.Topmost = false;
                IntPtr hWnd = new WindowInteropHelper(this).Handle;
                _SetWindowPos(hWnd, HWND_BOTTOM, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_NOACTIVATE);
            });
        }
        public void SetToTopWindow()
        {
            Dispatcher.Invoke(() =>
            {
                this._isTopmost = true;
                this.Topmost = true;
                IntPtr hWnd = new WindowInteropHelper(this).Handle;
                _SetWindowPos(hWnd, HWND_TOP, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_NOACTIVATE);
                this.Activate();
            });
        }

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

        private static bool _SetForegroundWindow(IntPtr hWnd)
        {
            bool rst = SetForegroundWindow(hWnd);

            if (!rst)
            {
                Debug.WriteLine("[QAMPage] SetForegroundWindow failed.");
            }

            return rst;
        }

        [DllImport("user32.dll")]
        private static extern int GetWindowRect(IntPtr hWnd, ref RECT rect);
        #endregion Win32API

        #endregion Get Full Screen State of Zoom and move to bottom of layer
    }
}