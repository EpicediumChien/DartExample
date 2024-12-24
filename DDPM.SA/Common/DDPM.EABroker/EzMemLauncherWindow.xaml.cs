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
using static DDPM.RemoteManagement.Common.Interfaces.Params;
using static DDPM.Win32Lib.Win32;
using Rectangle = System.Drawing.Rectangle;

namespace DDPM.EABroker
{
    /// <summary>
    /// Interaction logic for EzMemLauncherWindow.xaml
    /// </summary>
    public partial class EzMemLauncherWindow : Window
    {
        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        private static extern int GetWindowThreadProcessId(IntPtr hWnd, StringBuilder strText, int maxCount);
        public static int _GetWindowThreadProcessId(IntPtr hWnd, StringBuilder strText, int maxCount)
        {
            return GetWindowThreadProcessId(hWnd, strText, maxCount);
        }

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        private static extern bool IsWindowVisible(IntPtr hWnd);
        public static bool _IsWindowVisible(IntPtr hWnd)
        {
            return IsWindowVisible(hWnd);
        }

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
                IntPtr handleTemp = IntPtr.Zero;
                foreach (var app in apps.Values)
                {
                    IntPtr handle = IntPtr.Zero;

                    Process process = LaunchApp(app, _log);

                    for (int attempt = 0; attempt < 6; attempt++)
                    {
                        handle = app.AppType == "True" ? process.MainWindowHandle : GetWindowHandle(app);

                        Trace.WriteLine($"app.AppName = {app.AppName}, AppType = {app.AppType}, handle = {handle.ToString()}");
                        _log?.Info($"app.AppName = {app.AppName}, AppType = {app.AppType}, handle = {handle.ToString()}");
                        if (app.AppType == "False")// UWP need to check handle again
                        {
                            if (handle != IntPtr.Zero && !existingHandles.Contains(handle))
                            {
                                if (IsHandleBelongsToApp(handle, app.AppName))
                                {
                                    Trace.WriteLine($"IsHandleBelongsToApp True");
                                    _log?.Info($"IsHandleBelongsToApp True");
                                    existingHandles.Add(handle);
                                    break;
                                }
                            }
                            await Task.Delay(1000);
                        }
                        else
                            await Task.Delay(1000);// win32 need to wait long
                    }

                    handleTemp = IntPtr.Zero;
                    if (handle != IntPtr.Zero && GetWindowTitle(handle).Contains(app.AppName))
                    {
                        Trace.WriteLine($" LaunchAndArrange normal = {handle.ToString()}, handleTemp = {handleTemp.ToString()}, GetFilePathFromHandle = {GetFilePathFromHandle(handle)}, GetWindowTitle = {GetWindowTitle(handle)}");
                        _log?.Info($" LaunchAndArrange normal = {handle.ToString()}, handleTemp = {handleTemp.ToString()}, GetFilePathFromHandle = {GetFilePathFromHandle(handle)}, GetWindowTitle = {GetWindowTitle(handle)}");
                        handleTemp = handle;
                        appHandles.Add((handle, idx));
                        idx++;
                    }
                    else
                    {
                        Trace.WriteLine($"LaunchAndArrange Special = {handle.ToString()}, handleTemp = {handleTemp.ToString()}, GetFilePathFromHandle = {GetFilePathFromHandle(handle)}, GetWindowTitle = {GetWindowTitle(handle)}");
                        _log?.Info($"LaunchAndArrange Special = {handle.ToString()}, handleTemp = {handleTemp.ToString()}, GetFilePathFromHandle = {GetFilePathFromHandle(handle)}, GetWindowTitle = {GetWindowTitle(handle)}");
                        for (int attempt = 0; attempt < 3; attempt++)
                        {
                            Trace.WriteLine($"SpecialGetHandle Check ... ");
                            _log?.Info($"SpecialGetHandle Check ... ");
                            handle = SpecialGetHandle(app);
                            if (handle != IntPtr.Zero)
                                break;
                            await Task.Delay(500);
                        }
                        if (handle != IntPtr.Zero)
                        {
                            Trace.WriteLine($"LaunchAndArrange SpecialGetHandle handle != IntPtr.Zero => {handle.ToString()}, handleTemp = {handleTemp.ToString()}, GetFilePathFromHandle = {GetFilePathFromHandle(handle)}, GetWindowTitle = {GetWindowTitle(handle)}");
                            _log?.Info($"LaunchAndArrange SpecialGetHandle handle != IntPtr.Zero => {handle.ToString()}, handleTemp = {handleTemp.ToString()}, GetFilePathFromHandle = {GetFilePathFromHandle(handle)}, GetWindowTitle = {GetWindowTitle(handle)}");
                            handleTemp = handle;
                            appHandles.Add((handle, idx));
                            idx++;
                        }
                        else
                        {
                            _log?.Error($"Failed to get handle for app.AppName: {app.AppName}, app.AppPath: {app.AppPath}, app.AppUserModelID: {app.AppUserModelID}");
                        }
                    }
                }

                if (appHandles.Count == 0)
                    return;

                foreach (var (handle, idxCell) in appHandles)
                {
                    Trace.WriteLine($"handle = {handle.ToString()}, GetWindowTitle = {GetWindowTitle(handle)} || GetFilePathFromHandle = {GetFilePathFromHandle(handle)}");
                    ArrangeWindow(handle, idxCell, VM);
                }
            });
        }

        private IntPtr SpecialGetHandle(Bind_AddFullPage_AppCollectionData app)
        {
            _log?.Info($"SpecialGetHandle ... in");
            IntPtr appHandle = IntPtr.Zero;
            string uniCode = string.Empty;
            if (app.AppType != "True")
            {               
                uniCode = ExtractSubstring(app.AppUserModelID);// Profile的AppUserModelID找UWP識別碼
                _log?.Info($"SpecialGetHandle ... UWP uniCode = {uniCode}");
            }            
            List<IntPtr> exitsApp = GetVisibleWindowHandles();// list出現在桌面的handle
            foreach (var vapp in exitsApp)
            {
                string handlePath = GetFilePathFromHandle(vapp);// 從Handle找路徑                
                if (app.AppType != "True")
                {
                    _log?.Info($"SpecialGetHandle ... UWP {uniCode} compare {handlePath}");
                    if (handlePath.Contains(uniCode))// 從路徑比對UWP識別碼
                    {
                        Trace.WriteLine($" SpecialGetHandle GetFilePathFromHandle Contains PATH = {handlePath} and {uniCode}");
                        _log?.Info($"SpecialGetHandle ... UWP uniCode = {uniCode}");
                        appHandle = vapp;
                        break;
                    }
                }
                else
                {
                    _log?.Info($"SpecialGetHandle ... win32 {app.AppPath.ToUpper()} compare {handlePath.ToUpper()}");
                    if (app.AppPath.ToUpper() == handlePath.ToUpper())
                    {
                        Trace.WriteLine($" SpecialGetHandle GetFilePathFromHandle Contains PATH = {handlePath} and {uniCode}");
                        _log?.Info($"SpecialGetHandle ... win32 uniCode = {uniCode}");
                        appHandle = vapp;
                        break;
                    }
                }
            }
            Trace.WriteLine($" GetFilePathFromHandle Return Handle = {appHandle.ToString()}");
            _log?.Info($"SpecialGetHandle GetFilePathFromHandle Return Handle = {appHandle.ToString()} ... out");
            return appHandle;
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
            return _GetWindowThreadProcessId(hWnd, out nProcessId);
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
