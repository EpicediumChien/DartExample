using DDPM.Easy.Common;
using DDPM.SA.Common;
using Dell.Client.Framework.Common;
using nsWinEventHook;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using VcpCore.Common;
using Windows.ApplicationModel.Contacts;
using static DDPM.Win32Lib.Win32;
using Rectangle = System.Drawing.Rectangle;

namespace DDPM.EABroker
{
    /// <summary>
    /// Interaction logic for EzMemLauncherWindow.xaml
    /// </summary>
    public partial class EzMemLauncherWindow : Window
    {
        [DllImport("user32.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        private static extern IntPtr GetWindowThreadProcessId(IntPtr hWnd, out uint nProcessId);

        #region Private members
        private const string myName = "EzMemLauncherWin";
        private double _screenScale = 1.000;
        private ISplitCtrl _inputSplitCtrl = new SplitCtrl0A();
        private Rectangle _workingArea = Rectangle.Empty;
        private readonly ILog _log;
        private int _cellBorderCount = 0; //Cell count in the Layout (_inputSplitCtrl)
        private int _toBeArrangedCount = 0; //The count of app wait for arrange
        private int _alreadyArrangedCount = 0; //The count of app window has already been arraged
        #endregion

        #region Events
        //When (Phase I) the EA Layout (inputSplitCtrl) is created, show on window, and get the Rects.
        //Caller can start to Phase II, launch app and arrange their window into layout
        public EventHandler LayoutReady;

        //When (Phase II) all (_arrangeCount) of app windows are launched and arranged.
        public EventHandler ArrangeDone;
        #endregion Events

        #region Phase I - Assign Layout and Show Window
        //To do for Phase I
        // EzMemLauncherWindow emWin = new EzMemLauncherWindow(ispLayout, workingArea, _log);
        // emWin.
        // emWin.Show();
        //

        public EzMemLauncherWindow(ISplitCtrl inputSplitCtrl, Rectangle workingArea, int arrangeCount, ILog log)
        {
            InitializeComponent();
            _inputSplitCtrl = inputSplitCtrl;
            _workingArea = workingArea;
            _toBeArrangedCount = arrangeCount;
            _log = log;

            RefreshScreenScale();

            Left = workingArea.Left / _screenScale;
            Top = workingArea.Top / _screenScale;
            Width = workingArea.Width / _screenScale;
            Height = workingArea.Height / _screenScale;

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            System.Windows.Interop.WindowInteropHelper wndHelper = new System.Windows.Interop.WindowInteropHelper(this);
            Win32Lib.Win32.HideWinFromAltTab(wndHelper.Handle);

            _inputSplitCtrl.SplitMode = eSplitModes.Work;
            splitCtrl.Content = _inputSplitCtrl.UC;
        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            RefreshCellRects();
            if (LayoutReady != null)
                LayoutReady(this, EventArgs.Empty);
        }

        private void EAEditWindow_ContenRendered(object? sender, EventArgs e)
        {
        }

        private double RefreshScreenScale()
        {
            //Robert_Lin, 2024-12-6, use the method in CommonFunctions
            _screenScale = CommonFunctions.GetDpiX();
            return _screenScale;
            //double dpiX = 1.000;
            //var dpiXProperty = typeof(SystemParameters).GetProperty("DpiX", BindingFlags.NonPublic | BindingFlags.Static);
            //if (dpiXProperty != null)
            //{
            //    var varX = (int)dpiXProperty.GetValue(null, null);
            //    dpiX = (double)varX / (double)96;
            //}
            //_screenScale = dpiX;
            //return dpiX;
        }


        private Rect GetFrameworkElementRect(FrameworkElement ele)
        {
            if (ele == null)
                return Rect.Empty;

            if ((ele.ActualWidth == 0) && (ele.ActualHeight == 0))
                return Rect.Empty;

            PresentationSource preSrc = PresentationSource.FromVisual(ele);
            if (preSrc == null)
                return Rect.Empty;

            //Robert_Lin, 2024-12-6, use the method in CommonFunctions
            double screenScale = CommonFunctions.GetDpiX();
            //double screenScale = 1.000;
            //var dpiXProperty = typeof(SystemParameters).GetProperty("DpiX", BindingFlags.NonPublic | BindingFlags.Static);
            //if (dpiXProperty != null)
            //{
            //    var varX = (int)dpiXProperty.GetValue(null, null);
            //    double dpiX = (double)varX / (double)96;
            //    if (dpiX >= 1.0000)
            //        screenScale = dpiX;
            //}

            System.Windows.Point ptTopLeft = ele.PointToScreen(new System.Windows.Point(0, 0));
            double w = ele.ActualWidth * screenScale;
            double h = ele.ActualHeight * screenScale;
            //Trace.WriteLine($"ctrlActual={ele.ActualWidth}x{ele.ActualHeight}; Scale={_vm.ScreenScale} => {w}x{h}");
            return new Rect(ptTopLeft.X, ptTopLeft.Y, w, h);
        }

        public void RefreshCellRects()
        {
            if (_inputSplitCtrl == null)
            {
                return;
            }

            bool _areCellRectsRefreshed = true;
            foreach (CellObj objCell in _inputSplitCtrl.CellList)
            {
                if (objCell.CellBd == null)
                    continue;

                objCell.rc = GetFrameworkElementRect(objCell.CellBd);

                if (objCell.rc.IsEmpty)
                    _areCellRectsRefreshed = false;
            }

            if (!_areCellRectsRefreshed)
            {
                //System.Threading.Timer timer1 = new System.Threading.Timer((obj) => { RefreshCellRects(); }, null, 100, Timeout.Infinite);
            }
            else
            {
                string d = "";
            }

        }

        #endregion Phase I - Assign Layout and Show Window

        #region Phase II - Launch App and Arrange to Layout's CellBorder
        public int ToBeArrangedCount => _toBeArrangedCount;
        public int AlreadyArrangedCount => _alreadyArrangedCount;
        public bool AreAllAppsArranged => (AlreadyArrangedCount >= ToBeArrangedCount);

        public void LaunchAndArrange(Dictionary<string, Bind_AddFullPage_AppCollectionData> apps, int cellIndex)
        {
            if (apps.Count == 0)
                return;

            Task.Run(async () =>
            {
                List<(IntPtr handle, int idxCell)> appHandles = new List<(IntPtr, int)>();

                HashSet<IntPtr> existingHandles = new HashSet<IntPtr>();

                int idx = 0;
                foreach (var app in apps.Values)
                {
                    IntPtr handle = IntPtr.Zero;

                    Process process = LaunchApp(app, _log);

                    for (int attempt = 0; attempt < 5; attempt++)
                    {

                        handle = app.AppType == "True" ? process.MainWindowHandle : GetWindowHandle(app);
                        Trace.WriteLine($"************************ handle {app.AppType} || " + handle.ToString());
                        _log?.Error($"app.AppName = {app.AppName}, AppType = {app.AppType}, handle = {handle.ToString()}");
                        if (app.AppType == "False")// UWP need to check handle again
                        {
                            if (handle != IntPtr.Zero && !existingHandles.Contains(handle))
                            {
                                if (IsHandleBelongsToApp(handle, app.AppName))
                                {
                                    existingHandles.Add(handle);
                                    break;
                                }
                            }
                            await Task.Delay(500);
                        }
                        else
                            await Task.Delay(1000);// win32 need to wait long
                    }

                    if (handle != IntPtr.Zero)
                    {
                        appHandles.Add((handle, idx));
                        idx++;
                    }
                    else
                    {
                        _log?.Error($"Failed to get handle for app: {app.AppName}");
                    }
                }

                if (appHandles.Count == 0)
                    return;

                foreach (var (handle, idxCell) in appHandles)
                {
                    ArrangeWindow(handle, idxCell);
                }
            });
        }

        public static IntPtr _GetWindowThreadProcessId(IntPtr hWnd, out uint nProcessId)
        {
            return GetWindowThreadProcessId(hWnd, out nProcessId);
        }
        private bool IsHandleBelongsToApp(IntPtr handle, string expectedAppName)
        {
            try
            {
                uint processId;
                _GetWindowThreadProcessId(handle, out processId);

                Process process = Process.GetProcessById((int)processId);

                return process.ProcessName.Contains(expectedAppName, StringComparison.OrdinalIgnoreCase);
            }
            catch
            {
                return false;
            }
        }

        private static Process[] GetProcessesByName(Bind_AddFullPage_AppCollectionData appData, ILog? log = null)
        {
            Process[] processes = Array.Empty<Process>();
            try
            {
                if (appData.AppType == "False")
                {
                    // UWP 
                    processes = Process.GetProcessesByName(appData.AppUserModelID);
                    log?.Info($"[{myName}] GetProcessesByName, UWP app {appData.AppName} process count: {processes.Length}");
                }
                else
                {
                    // Desktop
                    processes = Process.GetProcessesByName(System.IO.Path.GetFileNameWithoutExtension(appData.AppPath));
                    log?.Info($"[{myName}] GetProcessesByName, Desktop app {appData.AppName} process count: {processes.Length}");
                }
            }
            catch (Exception ex)
            {
                log?.Error(ex,
                    $"[{myName}] GetProcessesByName({appData.AppName}), UWP app exceptoin.");
            }

            return processes;
        }

        private static Process LaunchApp(Bind_AddFullPage_AppCollectionData appData, ILog? log = null)
        {
            Process process = null;
            try
            {
                if (appData.AppType == "False")
                {
                    // UWP 應用程式
                    ProcessStartInfo startInfo = new ProcessStartInfo
                    {
                        FileName = "explorer.exe",
                        Arguments = $"shell:AppsFolder\\{appData.AppUserModelID}",
                        UseShellExecute = true
                    };
                    log?.Info($"[{myName}] LaunchApp, Launching UWP app: {appData.AppName}");
                    process = Process.Start(startInfo);
                }
                else
                {
                    // Desktop exe或檔案
                    ProcessStartInfo startInfo = new ProcessStartInfo
                    {
                        FileName = appData.AppPath,
                        UseShellExecute = true,  // 系統自動選擇應用程式來開啟
                        Verb = "open"            // 指定開啟檔案的動作
                    };
                    log?.Info($"[{myName}] LaunchApp, Launching desktop app or file: {appData.AppName}");
                    process = Process.Start(startInfo);
                }

                //if (process != null)
                //{
                //    if (!appData.AppPath.EndsWith(".png") && !appData.AppPath.EndsWith(".jpg") && !appData.AppPath.EndsWith(".txt"))
                //    {
                //        process.WaitForInputIdle();
                //        log?.Info($"[{myName}] LaunchApp, App {appData.AppName} is now idle.");
                //    }
                //}
                //else
                //{
                //    log?.Error($"[{myName}] LaunchApp, Failed to launch app or file: {appData.AppName}");
                //}
            }
            catch (Exception ex)
            {
                log?.Error(ex,
                    $"[{myName}] LaunchApp, Exception while launching app or file: {appData.AppName}, Error: {ex}");
            }

            return process;
        }

        private static IntPtr GetWindowHandle(Bind_AddFullPage_AppCollectionData appData, ILog? log = null)
        {
            IntPtr windowHandle = IntPtr.Zero;

            try
            {
                EzMemoryEnumWindows((hWnd, lParam) =>
                {
                    int length = Win32Lib.Win32._GetWindowTextLength(hWnd);
                    if (length == 0) return true;

                    //StringBuilder windowName = new StringBuilder(length);
                    //EzMemoryGetWindowText(hWnd, windowName, length + 1);
                    string windowText = Win32Lib.Win32._GetWindowText(hWnd);


                    if (appData.AppType == "False")
                    {
                        //string className = GetWindowClassName(hWnd);
                        string className = Win32Lib.Win32._GetClassName(hWnd);
                        if (className.Contains("ApplicationFrameWindow"))
                        {
                            windowHandle = hWnd;
                            log?.Info($"[{myName}] GetWindowHandle, Exception while retrieving window handle for r {appData.AppName}, handle: {windowHandle}");
                            return false;
                        }
                    }

                    return true;
                }, IntPtr.Zero);

                if (windowHandle == IntPtr.Zero)
                {
                    log?.Error($"[{myName}] GetWindowHandle, Failed to get window handle for {appData.AppName}");
                }
            }
            catch (Exception ex)
            {
                log?.Error(ex,
                    $"[{myName}] GetWindowHandle, Exception while retrieving window handle for {appData.AppName}, Error: {ex}");
            }

            return windowHandle;
        }


        //For UWP
        [DllImport("user32.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);
        private static bool EzMemoryEnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam)
        {
            return EnumWindows(lpEnumFunc, lParam);
        }
        #endregion Phase II - Launch App and Arrange to Layout's CellBorder


        #region EzMemLaunch
        public void ShowForEzMemLauncher(MonitorInfo mi, ISplitCtrl isp)
        {
            Screen? screen = Screen.AllScreens.FirstOrDefault(x => x.DeviceName.Equals(mi.DisplayName, StringComparison.OrdinalIgnoreCase));
            if (screen == null)
                return;

            splitCtrl.Visibility = Visibility.Visible;

            _inputSplitCtrl = isp.Clone();
            double screenScale = GetScreenScale();

            Rect rcScreen = new Rect();
            rcScreen.X = screen.WorkingArea.Left / screenScale;
            rcScreen.Y = screen.WorkingArea.Top / screenScale;
            rcScreen.Width = screen.WorkingArea.Width / screenScale;
            rcScreen.Height = screen.WorkingArea.Height / screenScale;

            Left = rcScreen.X;
            Top = rcScreen.Y;
            Width = rcScreen.Width;
            Height = rcScreen.Height;

            if (isp.IsOverlapCustomLayout)
            {
                SplitCtrl0B sp0B = (SplitCtrl0B)_inputSplitCtrl;
                sp0B.ApplySettingsToCellList(rcScreen);
            }
            _inputSplitCtrl.SplitMode = eSplitModes.Work;
            _inputSplitCtrl.IsVertical = (rcScreen.Width < rcScreen.Height);
            splitCtrl.Content = _inputSplitCtrl.UC;

            ContentRendered += Window_ContentRendered;
            Topmost = true;
            Show();
        }


        private double GetScreenScale()
        {
            //Robert_Lin, 2024-12-6, use the method in CommonFunctions
            return CommonFunctions.GetDpiX();
            //double screenScale = 1.000;
            //var dpiXProperty = typeof(SystemParameters).GetProperty("DpiX", BindingFlags.NonPublic | BindingFlags.Static);
            //if (dpiXProperty != null)
            //{
            //    var varX = (int)dpiXProperty.GetValue(null, null);
            //    double dpiX = (double)varX / (double)96;
            //    if (dpiX >= 1.0000)
            //        screenScale = dpiX;
            //}
            //return screenScale;
        }


        public void ArrangeWindow(IntPtr hWnd, int idxCell)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                if (_inputSplitCtrl == null)
                return;

            int cellBoderCount = _inputSplitCtrl.CellList.Count;
            if ((idxCell < 0) || (idxCell >= cellBoderCount))
            {
                return;
            }
            CellObj celObj = _inputSplitCtrl.CellList[idxCell];
            Rect rcArrange = celObj.rc;
            Task.Delay(500);
            if (rcArrange.IsEmpty || (rcArrange.Width <= 0))
            {
                rcArrange = GetFrameworkElementRect(celObj.CellBd);
                if (rcArrange.IsEmpty)
                {
                    return;
                }
            }
            _log?.Info($"[{myName}] hWnd={hWnd}, Cell[{idxCell}], Rect(({rcArrange.Left},{rcArrange.Top}){rcArrange.Width}x{rcArrange.Height})");

            WinEventHook.SetWindowPosition(hWnd, rcArrange);

            _alreadyArrangedCount++;
            if (AreAllAppsArranged)
            {
                if (ArrangeDone != null)
                {
                    _log?.Info($"[{myName}] hWnd={hWnd}, Cell[{idxCell}], Send ArrangeDone event.");
                    ArrangeDone(this, EventArgs.Empty);
                }
            }
            }));

        }
        public void Dispatcher_Close()
        {
            this.Dispatcher.Invoke(() =>
            {
                Close();
            });
        }
        #endregion

 
    }
}
