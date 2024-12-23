using DDPM.Easy.Common;
using DDPM.SA.Common;
using Dell.Client.Framework.Common;
using Microsoft.VisualBasic.Logging;
using nsWinEventHook;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
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
using Windows.Media.Devices.Core;
using static DDPM.RemoteManagement.Common.Interfaces.Params;
using static DDPM.Win32Lib.Win32;
using static System.Reflection.Metadata.BlobBuilder;
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

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        private static extern bool IsWindowVisible(IntPtr hWnd);
        public static bool _IsWindowVisible(IntPtr hWnd)
        {
            return IsWindowVisible(hWnd);
        }

        [DllImport("user32.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        private bool EzMemorySetForegroundWindow(IntPtr hWnd)
        {
            return SetForegroundWindow(hWnd);
        }

        [DllImport("user32.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        private const int SW_RESTORE = 9;
        private const int SW_SHOWNA = 8;
        private const int SW_MAXIMIZE = 3;
        private const int SW_SHOWNORMAL = 1;
        private const int SW_SHOWMAXIMIZED = 3;

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

        public void LaunchAndArrange(Dictionary<string, Bind_AddFullPage_AppCollectionData> apps, int cellIndex, ArrangeVM VM)
        {
            if (apps.Count == 0)
                return;

            Task.Run(async () =>
            {
                List<(IntPtr handle, int idxCell)> appHandles = new List<(IntPtr, int)>();

                HashSet<IntPtr> existingHandles = new HashSet<IntPtr>();

                int idx = 0;

                Trace.WriteLine($"[LaunchAndArrange] Start App.Count = {apps.Count}, cellIndex = {cellIndex.ToString()}");
                _log?.Info($"[LaunchAndArrange] Start App.Count = {apps.Count}, cellIndex = {cellIndex.ToString()}");
                foreach (var app in apps.Values)
                {
                    string winUWP = app.AppType == "True" ? "Win32" : "UWP";

                    string processName = string.Empty;

                    IntPtr handle = IntPtr.Zero;

                    Trace.WriteLine($"[LaunchAndArrange] 1 => {winUWP} APP, No. = {idx.ToString()}, AppName = {app.AppName}, AppPath = {app.AppPath}, AppPath = {app.AppUserModelID}");
                    _log?.Info($"[LaunchAndArrange] 1 => {winUWP} APP, No. = {idx.ToString()}, AppName = {app.AppName}, AppPath = {app.AppPath}, AppPath = {app.AppUserModelID}");

                    // 檢查應用程式是否已經存在
                    handle = SpecialGetHandle(app, processName);
                    if (handle != IntPtr.Zero)
                    {
                        Trace.WriteLine($"[LaunchAndArrange] 2 => ArrangeWindow = {idx},{winUWP} APP already exit, SpecialGetHandle Find Handle = {handle}, GetWindowTitle(handle) = {GetWindowTitle(handle)}, EzMemorySetForegroundWindow");
                        _log?.Info($"[LaunchAndArrange] 2 => ArrangeWindow = {idx}, {winUWP} APPalready exit, SpecialGetHandle Find Handle = {handle}, GetWindowTitle(handle) = {GetWindowTitle(handle)}, EzMemorySetForegroundWindow");
                        if(app.AppType != "True")// 已經在而且為UWP
                        {
                            Process process = LaunchApp(app, _log);
                            handle = GetWindowHandle(app, _log);
                            Task.Delay(1500);
                        }
                        //else
                        ShowWindow(handle, SW_RESTORE);
                        EzMemorySetForegroundWindow(handle); // 把應用程式拉到前景
                        appHandles.Add((handle, idx));
                    }
                    else // 不存在就啟動
                    {
                        Process process = LaunchApp(app, _log);

                        processName = process.ProcessName;

                        for (int attempt = 0; attempt < 4; attempt++)
                        {
                            handle = app.AppType == "True" ? process.MainWindowHandle : GetWindowHandle(app, _log);

                            Trace.WriteLine($"[LaunchAndArrange] 2 => {winUWP} APP, No. = {idx.ToString()}, handle = {handle.ToString()}");
                            _log?.Info($"[LaunchAndArrange] 2 => {winUWP} APP, No. = {idx.ToString()}, handle = {handle.ToString()}");
                            await Task.Delay(1500);
                        }
                    }
                    Trace.WriteLine($"[LaunchAndArrange] 3 => {winUWP} APP, handle = {handle.ToString()}, GetWindowTitle(handle) = {GetWindowTitle(handle)}, app.AppName = {app.AppName}");
                    _log?.Info($"[LaunchAndArrange] 3 => {winUWP} APP,  handle = {handle.ToString()}, GetWindowTitle(handle) = {GetWindowTitle(handle)}, app.AppName = {app.AppName}");

                    if (!appHandles.Any(item => item.handle == handle))
                    {
                        // 取得 Handle 且 Title Match
                        if (handle != IntPtr.Zero && GetWindowTitle(handle).Contains(app.AppName))
                        {
                            Trace.WriteLine($"[LaunchAndArrange] 3 => ArrangeWindow = {idx}, True");
                            _log?.Info($"[LaunchAndArrange] 3 => ArrangeWindow = {idx}, True");
                            appHandles.Add((handle, idx));
                            //ArrangeWindow(handle, idx, VM);
                            //idx++;
                        }
                        else //  Handle = IntPtr.Zero 或 Title 不 Match
                        {
                            Trace.WriteLine($"[LaunchAndArrange] 3 => False");
                            _log?.Info($"[LaunchAndArrange] 3 => False");
                            for (int attempt = 0; attempt < 3; attempt++)
                            {
                                Trace.WriteLine($"[LaunchAndArrange] 3 - 1 => SpecialGetHandle Check ... ");
                                _log?.Info($"[LaunchAndArrange] 3 - 1 => SpecialGetHandle Check ... ");

                                handle = SpecialGetHandle(app, processName);

                                Trace.WriteLine($"[LaunchAndArrange] 3 - 2 => SpecialGetHandle Done ... handle = {handle.ToString()}");
                                _log?.Info($"[LaunchAndArrange] 3 - 2 => SpecialGetHandle Done ... handle = {handle.ToString()}");
                                if (handle != IntPtr.Zero)
                                    break;
                                await Task.Delay(1500);
                            }
                            if (handle != IntPtr.Zero)
                            {
                                Trace.WriteLine($"[LaunchAndArrange] 3 => ArrangeWindow = {idx}, True SpecialGetHandle handle != IntPtr.Zero => {handle.ToString()}, GetFilePathFromHandle = {GetFilePathFromHandle(handle)}, GetWindowTitle = {GetWindowTitle(handle)}");
                                _log?.Info($"[LaunchAndArrange] 3 => ArrangeWindow = {idx}, True SpecialGetHandle handle != IntPtr.Zero => {handle.ToString()}, GetFilePathFromHandle = {GetFilePathFromHandle(handle)}, GetWindowTitle = {GetWindowTitle(handle)}");
                                appHandles.Add((handle, idx));
                                //ArrangeWindow(handle, idx, VM);
                                //idx++;
                            }
                            else // Handle 還是 = 0
                            {
                                Trace.WriteLine($"[LaunchAndArrange] Failed to get handle for app.AppName: {app.AppName}, app.AppPath: {app.AppPath}, app.AppUserModelID: {app.AppUserModelID}");
                                _log?.Error($"[LaunchAndArrange] Failed to get handle for app.AppName: {app.AppName}, app.AppPath: {app.AppPath}, app.AppUserModelID: {app.AppUserModelID}");
                            }
                        }
                    }
                    ArrangeWindow(handle, idx, VM);
                    idx++;
                    //Task.Delay(2000);
                }

                //if (appHandles.Count == 0)
                //    return;
                //idx = 0;
                //foreach (var (handle, idxCell) in appHandles)
                //{
                //    Trace.WriteLine($"[LaunchAndArrange] ArrangeWindow No. = {idx.ToString()}, handle = {handle.ToString()}");
                //    _log?.Error($"[LaunchAndArrange] ArrangeWindow No. = {idx.ToString()}, handle = {handle.ToString()}");
                //    ArrangeWindow(handle, idxCell, VM);
                //    idx++;
                //}
            });
        }

        /// <summary>
        /// List出桌面Handle再做比對
        /// </summary>
        /// <param name="app">欲尋找的Handle</param>
        /// <returns>Handle</returns>
        private IntPtr SpecialGetHandle(Bind_AddFullPage_AppCollectionData app, string processName)
        {
            Trace.WriteLine($"[LaunchAndArrange] SpecialGetHandle ... in");
            _log?.Info($"[LaunchAndArrange] SpecialGetHandle ... in");
            IntPtr appHandle = IntPtr.Zero;
            string uniCode = string.Empty;
            int no = 0;
            try
            {
                List<IntPtr> exitsApp = GetVisibleWindowHandles();// list出現在桌面的handle
                foreach (var vapp in exitsApp)
                {
                    appHandle = IntPtr.Zero;
                    uint processId;
                    string handlePath = GetFilePathFromHandle(vapp);// 從Handle找路徑
                    Trace.WriteLine($"[LaunchAndArrange] SpecialGetHandle HandlePath {handlePath}, Handle = {vapp.ToString()}");
                    if (handlePath != null)
                    {
                        if (app.AppType != "True")
                        {
                            // UWP比對路徑
                            if (handlePath.Contains(app.AppPath))
                            {
                                Trace.WriteLine($"[LaunchAndArrange] SpecialGetHandle HandlePath Contains UWPPath {app.AppPath}");
                                _log?.Info($"[LaunchAndArrange] SpecialGetHandle HandlePath Contains UWPPath {app.AppPath}");
                                appHandle = vapp;
                                break;
                            }
                        }
                        else
                        {
                            // Win32比對包含
                            if (app.AppPath.ToUpper() == handlePath.ToUpper())
                            {
                                Trace.WriteLine($"[LaunchAndArrange] SpecialGetHandle HandlePath == Win32Path {app.AppPath}");
                                _log?.Info($"[LaunchAndArrange] SpecialGetHandle HandlePath == Win32Path {app.AppPath}");
                                appHandle = vapp;
                                break;
                            }
                        }
                        no++;
                    }


                    if (processName != string.Empty && appHandle == IntPtr.Zero)
                    {
                        Trace.WriteLine($"[LaunchAndArrange] 3 - 3 => Process Name : {processName}");
                        _log?.Info($"[LaunchAndArrange] 3 - 3 => Process Name : {processName}");
                        if (handlePath.Contains(processName))// 比對 process 啟動的程式名稱
                        {
                            Trace.WriteLine($"[LaunchAndArrange] 3 - 4 => True Compare ProcessName => GetFilePathFromHandle = {GetFilePathFromHandle(vapp)}, GetWindowTitle = {GetWindowTitle(vapp)}");
                            _log?.Info($"[LaunchAndArrange] 3 - 4 => True Compare ProcessName => GetFilePathFromHandle = {GetFilePathFromHandle(vapp)}, GetWindowTitle = {GetWindowTitle(vapp)}");
                            appHandle = vapp;
                            break;
                        }
                    }

                }
                Trace.WriteLine($"[LaunchAndArrange] SpecialGetHandle Zero");
                _log?.Info($"[LaunchAndArrange] SpecialGetHandle Zero");
                return appHandle;
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"[LaunchAndArrange] SpecialGetHandle Zero Exception {ex.Message}");
                _log?.Info($"[LaunchAndArrange] SpecialGetHandle Zero Exception {ex.Message}");
                return IntPtr.Zero;
            }
        }

        /// <summary>
        /// Get before "!" and after "_" string
        /// </summary>
        /// <param name="input">string</param>
        /// <returns></returns>
        private string ExtractSubstring(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return string.Empty;
            }

            int underscoreIndex = input.IndexOf('_');
            int exclamationIndex = input.IndexOf('!');

            if (underscoreIndex != -1 && exclamationIndex != -1 && underscoreIndex < exclamationIndex)
            {
                return input.Substring(underscoreIndex + 1, exclamationIndex - underscoreIndex - 1);
            }

            return string.Empty;
        }

        private static string GetFilePathFromHandle(IntPtr hWnd)
        {
            _GetWindowThreadProcessId(hWnd, out uint processId);

            if (processId == 0)
            {
                return null;
            }

            try
            {
                Process process = Process.GetProcessById((int)processId);
                return process.MainModule?.FileName;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return null;
            }
        }

        private static List<IntPtr> GetVisibleWindowHandles()
        {
            List<IntPtr> windowHandles = new List<IntPtr>();

            EnumWindows((hWnd, lParam) =>
            {
                if (_IsWindowVisible(hWnd) && GetWindowTitle(hWnd).Length > 0)
                {
                    windowHandles.Add(hWnd);
                }
                return true; // Continue enumeration
            }, IntPtr.Zero);

            return windowHandles;
        }

        private static string GetWindowTitle(IntPtr hWnd)
        {
            //StringBuilder title = new StringBuilder(256);
            return Win32Lib.Win32._GetWindowText(hWnd);
            //return title.ToString();
        }
        public static IntPtr _GetWindowThreadProcessId(IntPtr hWnd, out uint nProcessId)
        {
            return GetWindowThreadProcessId(hWnd, out nProcessId);
        }
        private bool IsHandleBelongsToApp(IntPtr handle, string expectedAppName, ILog? log = null)
        {
            try
            {
                uint processId;
                _GetWindowThreadProcessId(handle, out processId);
                Process process = Process.GetProcessById((int)processId);
                return process.ProcessName.Contains(expectedAppName, StringComparison.OrdinalIgnoreCase);
            }
            catch (Exception ex)
            {
                log?.Info($"[{myName}] IsHandleBelongsToApp, Exception : {ex.Message}");
                return false;
            }
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
            Trace.WriteLine($"[LaunchAndArrange] GetWindowHandle ... in");
            log?.Info($"[LaunchAndArrange] GetWindowHandle ... in");
            try
            {
                EzMemoryEnumWindows((hWnd, lParam) =>
                {
                    int length = Win32Lib.Win32._GetWindowTextLength(hWnd);
                    if (length == 0) return true;
                    string windowText = Win32Lib.Win32._GetWindowText(hWnd);

                    if (appData.AppName == windowText)
                    {
                        Trace.WriteLine($"[LaunchAndArrange] GetWindowHandle hWnd = {hWnd}, windowText = {windowText}");
                        log?.Info($"[LaunchAndArrange] GetWindowHandle hWnd = {hWnd}, windowText = {windowText}");
                        windowHandle = hWnd;
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


        public void ArrangeWindow(IntPtr hWnd, int idxCell, ArrangeVM VM)
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

                //Inflate the rect, because the rcArrange not include the border thickness(=6) of CellBorder
                if (VM.IsWithoutGap)
                {
                    rcArrange.Inflate(6, 6);
                }
                Task.Delay(500);
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
