using DDPM.Easy.Common;
using DDPM.SA.Common;
using DDPM.Win32Lib;
using Dell.Client.Framework.Common;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using VcpCore.Common;
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
        public static IntPtr _GetWindowThreadProcessId(IntPtr hWnd, out uint nProcessId)
        {
            IntPtr rst = GetWindowThreadProcessId(hWnd, out nProcessId);
            if (rst == IntPtr.Zero)
            {
                Trace.WriteLine("[EzMemLauncherWindow] GetWindowThreadProcessId failed");
#if DEBUG
                Console.WriteLine("[EzMemLauncherWindow] GetWindowThreadProcessId failed");
#endif
            }
            return rst;
        }

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        private static extern bool IsWindowVisible(IntPtr hWnd);
        public static bool _IsWindowVisible(IntPtr hWnd)
        {
            bool rst = IsWindowVisible(hWnd);
            if (!rst)
            {
                Trace.WriteLine("[EzMemLauncherWindow] IsWindowVisible is false");
#if DEBUG
                Console.WriteLine("[EzMemLauncherWindow] IsWindowVisible is false");
#endif
            }
            return rst;
        }

        [DllImport("user32.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
        private static bool EzMemoryShowWindow(IntPtr hWnd, int nCmdShow)
        {
            bool rst = ShowWindow(hWnd, nCmdShow);

            if (!rst)
            {
                Debug.WriteLine("[EzMemLauncherWindow] Windows was hidden before.");
            }

            return rst;
        }
        private const int SW_SHOWNORMAL = 1;
        //private const int SW_SHOWMAXIMIZED = 3;
        //private const int SW_MAXIMIZE = 3;
        private const int SW_SHOWN = 5;
        //private const int SW_SHOWNA = 8;
        //private const int SW_RESTORE = 9;


        #region Private members
        private const string myName = "EzMemLauncherWin";
        private double _screenScale = 1.000;
        private ISplitCtrl _inputSplitCtrl = new SplitCtrl0A();
        private Rectangle _workingArea = Rectangle.Empty;
        private readonly ArrangeVM _vm;
        //private int _cellBorderCount = 0; //Cell count in the Layout (_inputSplitCtrl)
        private int _toBeArrangedCount = 0; //The count of app wait for arrange
        private int _alreadyArrangedCount = 0; //The count of app window has already been arraged
        private readonly IDeviceManagerSA _deviceManagerSA;
        #endregion

        #region Events
        //When (Phase I) the EA Layout (inputSplitCtrl) is created, show on window, and get the Rects.
        //Caller can start to Phase II, launch app and arrange their window into layout
        public EventHandler? LayoutReady = null;

        //When (Phase II) all (_arrangeCount) of app windows are launched and arranged.
        public EventHandler? ArrangeDone = null;
        #endregion Events

        #region Phase I - Assign Layout and Show Window
        //To do for Phase I
        // EzMemLauncherWindow emWin = new EzMemLauncherWindow(ispLayout, workingArea, _log);
        // emWin.
        // emWin.Show();
        //

        public EzMemLauncherWindow(ISplitCtrl inputSplitCtrl, Rectangle workingArea, int arrangeCount, 
                                    ArrangeVM vm, IDeviceManagerSA deviceManagerSA)
        {
            InitializeComponent();
            _inputSplitCtrl = inputSplitCtrl;
            _workingArea = workingArea;
            _toBeArrangedCount = arrangeCount;
            _vm = vm;
            _deviceManagerSA = deviceManagerSA;
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

        private void Window_ContentRendered(object? sender, EventArgs e)
        {
            RefreshCellRects();

            if (LayoutReady != null)
                LayoutReady(this, EventArgs.Empty);
        }

        //private void EAEditWindow_ContenRendered(object? sender, EventArgs e)
        //{
        //}

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

            _ = Task.Run(async () =>
            {
                List<(IntPtr handle, int idxCell)> appHandles = new List<(IntPtr, int)>();

                HashSet<IntPtr> existingHandles = new HashSet<IntPtr>();

                int idx = 0;

                Trace.WriteLine($"[LaunchAndArrange] Start App.Count = {apps.Count}, cellIndex = {cellIndex.ToString()}");
                _vm.WriteLog($"[LaunchAndArrange] Start App.Count = {apps.Count}, cellIndex = {cellIndex.ToString()}");
                foreach (var app in apps.Values)
                {
                    string winUWP = app.AppType == "True" ? "Win32" : "UWP";

                    string processName = string.Empty;

                    IntPtr handle = IntPtr.Zero;
                    int idxCell = idx;

                    //Trace.WriteLine($"[LaunchAndArrange] 1 => {winUWP} APP, No. = {idx.ToString()}, AppName = {app.AppName}, AppPath = {app.AppPath}, AppPath = {app.AppUserModelID}");
                    //_vm.WriteLog($"[LaunchAndArrange] 1 => {winUWP} APP, No. = {idx.ToString()}, AppName = {app.AppName}, AppPath = {app.AppPath}, AppPath = {app.AppUserModelID}");

                    // 檢查應用程式是否已經存在
                    //handle = SpecialGetHandle(app, processName);
                    handle = FindRunningWindowHandle(app);
                    if (handle != IntPtr.Zero)
                    {
                        //Trace.WriteLine($"[LaunchAndArrange] 2 => ArrangeWindow = {idx},{winUWP} APP already exit, SpecialGetHandle Find Handle = {handle}, GetWindowTitle(handle) = {GetWindowTitle(handle)}, EzMemorySetForegroundWindow");
                        _vm.WriteLog($"[LaunchAndArrange] 2 => App is already running.");
                        //if (app.AppType != "True")// 已經在而且為UWP
                        //{
                        //    //Process process = LaunchApp(app, _vm.Log);
                        //    //handle = GetWindowHandle(app, _vm.Log);
                        //    //Task.Delay(1500);
                        //    //WinEventHook._SetWindowPos(handle, new IntPtr(-1))
                        //}
                        //else
                        //ShowWindowAsync(handle, SW_MAXIMIZE);
                        //ShowWindowAsync(handle, SW_SHOWNORMAL);
                        //ShowWindow(handle, SW_RESTORE);
                        EzMemoryShowWindow(handle, SW_SHOWNORMAL);
                        EzMemoryShowWindow(handle, SW_SHOWN);
                        Win32._SetForegroundWindow(handle);
                        //EzMemorySetForegroundWindow(handle); // 把應用程式拉到前景
                        //ShowWindow(handle, SW_SHOWNORMAL);
                        appHandles.Add((handle, idx));
                    }
                    else // 不存在就啟動
                    {
                        _vm.WriteLog($"[LaunchAndArrange] 2 => App is not running, calling to LaunchApp.");
                        Process process = LaunchApp(app);
                        if (process == null)
                        {
                            idx++; // 找不到則++後continue
                            _alreadyArrangedCount++; // 找不到則++後continue
                            //_vm.WriteLog($"[LaunchAndArrange] Fail to launchApp: {app.AppName}");
                            _vm.WriteLog($"[LaunchAndArrange] Fail to launchApp.");
                            if (AreAllAppsArranged && ArrangeDone != null)
                            {
                                ArrangeWindow(handle, idxCell); // 如果都沒有Launch，仍須跑最後一次
                                _vm.WriteLog($"[LaunchAndArrange] Fail to launchApp. Send ArrangeDone event.");
                            }
                            continue;
                        }

                        processName = process.ProcessName;

                        for (int attempt = 0; attempt < 4; attempt++)
                        {
                            handle = app.AppType == "True" ? process.MainWindowHandle : GetWindowHandle(app);

                            //Trace.WriteLine($"[LaunchAndArrange] 2 => {winUWP} APP, No. = {idx.ToString()}, handle = {handle.ToString()}");
                            //_vm.WriteLog($"[LaunchAndArrange] 2 => {winUWP} APP, No. = {idx.ToString()}, handle = {handle.ToString()}");
                            await Task.Delay(1500);
                        }
                        //_vm.WriteLog($"[LaunchAndArrange] App is launched, hWnd={handle}=0x{handle:X}, pid={process.Id}, hProcess={process.Handle}");
                        _vm.WriteLog($"[LaunchAndArrange] App is launched.");


                        EzMemoryShowWindow(handle, SW_SHOWNORMAL);
                        Win32._SetForegroundWindow(handle);
                    }
                    //Trace.WriteLine($"[LaunchAndArrange] 3 => {winUWP} APP, handle = {handle.ToString()}, GetWindowTitle(handle) = {GetWindowTitle(handle)}, app.AppName = {app.AppName}");
                    //_vm.WriteLog($"[LaunchAndArrange] 3 => {winUWP} APP,  handle = {handle.ToString()}, GetWindowTitle(handle) = {GetWindowTitle(handle)}, app.AppName = {app.AppName}");

                    if (!appHandles.Any(item => item.handle == handle))
                    {
                        // 取得 Handle 且 Title Match
                        if (handle != IntPtr.Zero && GetWindowTitle(handle).Contains(app.AppName))
                        {
                            //Trace.WriteLine($"[LaunchAndArrange] 3 => ArrangeWindow = {idx}, True");
                            //_vm.WriteLog($"[LaunchAndArrange] 3 => ArrangeWindow = {idx}, True"); 
                            Trace.WriteLine($"[LaunchAndArrange] 3 => Get Handle and Title Match");
                            _vm.WriteLog($"[LaunchAndArrange] 3 => Get Handle and Title Match");
                            appHandles.Add((handle, idx));
                            //ArrangeWindow(handle, idx, VM);
                            //idx++;
                        }
                        else //  Handle = IntPtr.Zero 或 Title 不 Match
                        {
                            Trace.WriteLine($"[LaunchAndArrange] 3 => False");
                            _vm.WriteLog($"[LaunchAndArrange] 3 => False");
                            for (int attempt = 0; attempt < 3; attempt++)
                            {
                                Trace.WriteLine($"[LaunchAndArrange] 3 - 1 => SpecialGetHandle Check ... ");
                                _vm.WriteLog($"[LaunchAndArrange] 3 - 1 => SpecialGetHandle Check ... ");

                                handle = SpecialGetHandle(app, processName);

                                //Trace.WriteLine($"[LaunchAndArrange] 3 - 2 => SpecialGetHandle Done ... handle = {handle.ToString()}");
                                //_vm.WriteLog($"[LaunchAndArrange] 3 - 2 => SpecialGetHandle Done ... handle = {handle.ToString()}");
                                if (handle != IntPtr.Zero)
                                    break;
                                await Task.Delay(1500);
                            }
                            if (handle != IntPtr.Zero)
                            {
                                //Trace.WriteLine($"[LaunchAndArrange] 3 => ArrangeWindow = {idx}, True SpecialGetHandle handle != IntPtr.Zero => {handle.ToString()}, GetFilePathFromHandle = {GetFilePathFromHandle(handle)}, GetWindowTitle = {GetWindowTitle(handle)}");
                                //_vm.WriteLog($"[LaunchAndArrange] 3 => ArrangeWindow = {idx}, True SpecialGetHandle handle != IntPtr.Zero => {handle.ToString()}, GetFilePathFromHandle = {GetFilePathFromHandle(handle)}, GetWindowTitle = {GetWindowTitle(handle)}");

                                Trace.WriteLine($"[LaunchAndArrange] 3 => SpecialGetHandle handle != IntPtr.Zero");
                                _vm.WriteLog($"[LaunchAndArrange] 3 => SpecialGetHandle handle != IntPtr.Zero");
                                appHandles.Add((handle, idx));
                                //ArrangeWindow(handle, idx, VM);
                                //idx++;
                            }
                            else // Handle 還是 = 0
                            {
                                //Trace.WriteLine($"[LaunchAndArrange] Failed to get handle for app.AppName: {app.AppName}, app.AppPath: {app.AppPath}, app.AppUserModelID: {app.AppUserModelID}");
                                //_vm.WriteLog($"[LaunchAndArrange] Failed to get handle for app.AppName: {app.AppName}, app.AppPath: {app.AppPath}, app.AppUserModelID: {app.AppUserModelID}");

                                Trace.WriteLine($"[LaunchAndArrange] Failed to get handle for app");
                                _vm.WriteLog($"[LaunchAndArrange] Failed to get handle for app");
                            }
                        }
                    }
                    _vm.WriteLog($"[LaunchAndArrange] Calling to ArrangeWindow(hWnd={handle}=0x{handle:X}, idxCell={idxCell})");
                    ArrangeWindow(handle, idxCell);
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
        /// Find if the specified app in running, return it's hWnd if yes.
        /// </summary>
        /// <param name="app"></param>
        /// <returns></returns>
        private IntPtr FindRunningWindowHandle(Bind_AddFullPage_AppCollectionData app)
        {
            bool isTargetAppUwp = app.AppType.Equals("False", StringComparison.OrdinalIgnoreCase);
            //_vm.WriteLog($"@FindRunningProcess(Name={app.AppName}, Type={app.AppType}, UserModelId={app.AppUserModelID}, Path={app.AppPath})");

            IntPtr hWndApp = IntPtr.Zero;
            //Robert_Lin, 2024-12-26 changed, to use the same method of EA capture overlap windows
            //NEW:
            List<IntPtr> windowHandles = Win32.GetAltTabWindows();// list出現在桌面的handle
            //OLD:
            //List<IntPtr> windowHandles = GetVisibleWindowHandles();// list出現在桌面的handle
            _vm.WriteLog($"@FindRunningWindowHandle(), EnumWindows count={windowHandles.Count}");
            foreach (var hWnd in windowHandles)
            {
                //Get basic window properties
                string wndText = Win32._GetWindowText(hWnd);

                //Get the Process from the hWnd
                Process process;
                string msg;
                if (!Win32.GetProcessFromWindowHandle(hWnd, out process, out msg))
                {
                    _vm.WriteLog($"@FindRunningProcess, GetProcess error: {msg}");
                    continue;
                }
                if (process == null)
                {
                    _vm.WriteLog($"@FindRunningProcess, GetProcess return null.");
                    continue;
                }

                //If the target app is UWP app
                if (isTargetAppUwp)
                {
                    //Get Window ClassName
                    string className = Win32._GetClassName(hWnd);
                    //If the ClassName is
                    // "ApplicationFrameWindow" => UWP app in Normal state, or The frame window when in Minimized state
                    // "Windows.UI.Core.CorwWindow" => UWP app in Minimized state, need to find it's child window of "ApplicationFrameWindow" class

                    if (className.Equals("ApplicationFrameWindow"))
                    {
                        //hWnd is an UWP App, but targer is not => not the target app
                        if (!isTargetAppUwp)
                        {
                            continue;
                        }

                        //hWnd is an UWP app, try to get its AppUserModelId
                        string appUserModelId = "";
                        if (!Win32.GetUwpAppUserModelId(hWnd, out appUserModelId))
                        {
                            _vm.WriteLog($"@FindRunningProcess, GetUwpAppUserModelId error: {appUserModelId}");
                            continue;
                        }
                        if (string.IsNullOrEmpty(appUserModelId))
                        {
                            _vm.WriteLog($"@FindRunningProcess, GetUwpAppUserModelId output empty.");
                            continue;
                        }
                        //Compare AppUserModelId
                        if (appUserModelId.Equals(app.AppUserModelID))
                        {
                            //Found it
                            _vm.WriteLog($"@FindRunningProcess, Found UWP App.");
                            return hWnd;
                        }
                        else
                        {
                            //Not matched
                            continue ;
                        }
                    }
                    if (className.Equals("Windows.UI.Core.CoreWindow"))
                    {
                        //UWP app in Minimized state, but this hWnd can not activate the UWP app window. we should skip it
                        continue;
                    }

                    //Other window classes => not the mainwindow
                    //Robert_Lin, 2025-1-6, comment-out let it fo down for PathName check and special check
                    //continue;
                }

                //Target app is Non-UWP


                //Get the pathname of from hProcess
                string pathName = "";

                try
                {
                    uint lpdwSize = 2048;
                    StringBuilder? sb = new StringBuilder((int)lpdwSize);
                    if (Win32._QueryFullProcessImageName(process.Handle, 0, sb, ref lpdwSize))
                    {
                        pathName = sb.ToString();
                    }
                    else
                    {
                        //Try with another method
                        if ((process != null) && (process.MainModule != null))
                            pathName = process.MainModule.FileName;
                    }

                    sb.Clear();
                    sb = null;
                }
                catch (Exception ex1)
                {
                    _vm.WriteLog($"@FindRunningProcess, GetPathName from Process error: {ex1.Message}");
                    continue;
                }


                if (string.IsNullOrEmpty(pathName))
                {
                    _vm.WriteLog($"@FindRunningProcess, GetPathName from Process error: pathName is empty");
                    continue;
                }

                ////Special procedure for UWP app
                //string fileName = System.IO.Path.GetFileName(pathName);
                //IntPtr hProcessUwp = IntPtr.Zero;
                //if (fileName.Equals("ApplicationFrameHost.exe"))
                //{
                //    pathName = Win32.GetUwpAppPathName(hWnd, (uint)process.Id, out hProcessUwp);
                //    if (string.IsNullOrEmpty(pathName))
                //    {
                //        _vm.WriteLog($"@FindRunningProcess, hWnd={hWnd}=0x{hWnd:X}, GetUwpAppPathName return empty.");
                //        continue;
                //    }
                //    _vm.WriteLog($"@FindRunningProcess, UwpAppPathName={pathName}");
                //}

                //UWP App
                if (isTargetAppUwp)
                {
                    //if (pathName.Contains(app.AppPath))
                    //{
                    //    _vm.WriteLog($"@FindRunningProcess, Found the running UwpApp, hWnd={hWnd}=0x{hWnd:X}");
                    //    hWndApp = hWnd;

                    //    if (hProcessUwp != IntPtr.Zero)
                    //    {
                    //        Win32._CloseHandle(hProcessUwp);
                    //    }
                    //    break;
                    //}
                }
                else
                {
                    //Win32
                    if (app.AppPath.Equals(pathName, StringComparison.OrdinalIgnoreCase))
                    {
                        _vm.WriteLog($"@FindRunningProcess, Found the running Win32App.");
                        hWndApp = hWnd;
                        break;
                    }
                }

                //Finaly check some specical cases
                //

                //Microsoft Mail:
                //app:
                // Name="Mail"
                // Path="C:\\Program Files\\WindowsApps\\microsoft.windowscommunicationsapps_16005.14326.22113.0_x64__8wekyb3d8bbwe"
                // AppUserModelID="microsoft.windowscommunicationsapps_8wekyb3d8bbwe!microsoft.windowslive.mail"
                //process:
                // WindowClassName="OlkHost"
                // pathName="C:\\Program Files\\WindowsApps\\Microsoft.OutlookForWindows_1.2024.1216.300_x64_8wekyb3d8bbwe\olk.exe
                if (app.AppUserModelID.Equals("microsoft.windowscommunicationsapps_8wekyb3d8bbwe!microsoft.windowslive.mail", StringComparison.OrdinalIgnoreCase) && 
                    pathName.StartsWith("C:\\Program Files\\WindowsApps\\Microsoft.OutlookForWindows_"))
                {
                    string fileName = System.IO.Path.GetFileName(pathName);
                    if (fileName.Equals("olk.exe", StringComparison.OrdinalIgnoreCase))
                    {
                        hWndApp = hWnd;
                        break;
                    }
                }

            } //foreach

            return hWndApp;
        }

        /// <summary>
        /// List出桌面Handle再做比對
        /// </summary>
        /// <param name="app">欲尋找的Handle</param>
        /// <returns>Handle</returns>
        private IntPtr SpecialGetHandle(Bind_AddFullPage_AppCollectionData app, string processName)
        {
            Trace.WriteLine($"[LaunchAndArrange] SpecialGetHandle ... in");
            _vm.WriteLog($"[LaunchAndArrange] SpecialGetHandle ... in");
            IntPtr appHandle = IntPtr.Zero;
            string uniCode = string.Empty;
            int no = 0;
            try
            {
                List<IntPtr> exitsApp = GetVisibleWindowHandles();// list出現在桌面的handle
                foreach (var vapp in exitsApp)
                {
                    appHandle = IntPtr.Zero;
                    //uint processId;
                    string handlePath = GetFilePathFromHandle(vapp);// 從Handle找路徑
                    Trace.WriteLine($"[LaunchAndArrange] SpecialGetHandle HandlePath {handlePath}, Handle = {vapp.ToString()}");
                    if (handlePath != null)
                    {
                        if (app.AppType != "True")
                        {
                            // UWP比對路徑
                            if (handlePath.Contains(app.AppPath))
                            {
                                Trace.WriteLine($"[LaunchAndArrange] SpecialGetHandle HandlePath Contains UWPPath.");
                                _vm.WriteLog($"[LaunchAndArrange] SpecialGetHandle HandlePath Contains UWPPath.");
                                appHandle = vapp;
                                break;
                            }
                        }
                        else
                        {
                            // Win32比對包含
                            if (app.AppPath.ToUpper(CultureInfo.InvariantCulture) == handlePath.ToUpper(CultureInfo.InvariantCulture))
                            {
                                Trace.WriteLine($"[LaunchAndArrange] SpecialGetHandle HandlePath == Win32Path.");
                                _vm.WriteLog($"[LaunchAndArrange] SpecialGetHandle HandlePath == Win32Path.");
                                appHandle = vapp;
                                break;
                            }
                        }
                        no++;
                    }
                    else
                    {
                        Trace.WriteLine($"[LaunchAndArrange] SpecialGetHandle Zero: handlePath is null");
                        _vm.WriteLog($"[LaunchAndArrange] SpecialGetHandle Zero: handlePath is null");
                        continue;
                    }

                    if (processName != string.Empty && appHandle == IntPtr.Zero)
                    {
                        //Trace.WriteLine($"[LaunchAndArrange] 3 - 3 => Process Name : {processName}");
                        //_vm.WriteLog($"[LaunchAndArrange] 3 - 3 => Process Name : {processName}");

                        if (handlePath.Contains(processName))// 比對 process 啟動的程式名稱
                        {
                            //Trace.WriteLine($"[LaunchAndArrange] 3 - 4 => True Compare ProcessName => GetFilePathFromHandle = {GetFilePathFromHandle(vapp)}, GetWindowTitle = {GetWindowTitle(vapp)}");
                            //_vm.WriteLog($"[LaunchAndArrange] 3 - 4 => True Compare ProcessName => GetFilePathFromHandle = {GetFilePathFromHandle(vapp)}, GetWindowTitle = {GetWindowTitle(vapp)}");
                            appHandle = vapp;
                            break;
                        }
                    }

                }
                Trace.WriteLine($"[LaunchAndArrange] SpecialGetHandle Zero");
                _vm.WriteLog($"[LaunchAndArrange] SpecialGetHandle Zero");
                return appHandle;
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"[LaunchAndArrange] SpecialGetHandle Zero Exception {ex.Message}");
                _vm.WriteLog($"[LaunchAndArrange] SpecialGetHandle Zero Exception {ex.Message}");
                return IntPtr.Zero;
            }
        }

        /// <summary>
        /// Get before "!" and after "_" string
        /// </summary>
        /// <param name="input">string</param>
        /// <returns></returns>
        //Derek 2025/04/01
        //private string ExtractSubstring(string input)
        //{
        //    if (string.IsNullOrEmpty(input))
        //    {
        //        return string.Empty;
        //    }

        //    int underscoreIndex = input.IndexOf('_');
        //    int exclamationIndex = input.IndexOf('!');

        //    if (underscoreIndex != -1 && exclamationIndex != -1 && underscoreIndex < exclamationIndex)
        //    {
        //        return input.Substring(underscoreIndex + 1, exclamationIndex - underscoreIndex - 1);
        //    }

        //    return string.Empty;
        //}

        private static string? GetFilePathFromHandle(IntPtr hWnd)
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

            _EnumWindows((hWnd, lParam) =>
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

        //Derek 2025/04/01
        //private bool IsHandleBelongsToApp(IntPtr handle, string expectedAppName, ILog? log = null)
        //{
        //    try
        //    {
        //        uint processId;
        //        _GetWindowThreadProcessId(handle, out processId);
        //        Process process = Process.GetProcessById((int)processId);
        //        return process.ProcessName.Contains(expectedAppName, StringComparison.OrdinalIgnoreCase);
        //    }
        //    catch (Exception ex)
        //    {
        //        log?.Info($"[{myName}] IsHandleBelongsToApp, Exception : {ex.Message}");
        //        return false;
        //    }
        //}

        private Process? LaunchApp(Bind_AddFullPage_AppCollectionData appData, ILog? log = null)
        {
            Process? process = null;

            try
            {
                if (appData.AppType == "False") //UWP
                {
                    Dictionary<string, InstalledAppInfo> applist = new Dictionary<string, InstalledAppInfo>();
                    if (_deviceManagerSA != null)
                        applist = _deviceManagerSA.GetAllAppList().Result; // 取得所有applist比對UWP
                    else
                        return null;

                    if (!applist.Values.Any(app => app.AppUserModelID == appData.AppUserModelID)) // 檢查 AppUserModelID 是否存在
                    {
                        _vm?.WriteLog($"@LaunchApp error: UWP AppUserModelID not found - {appData.AppUserModelID}");
                        return null; // 找不到 UWP 應用程式
                    }
                    // UWP 應用程式
                    ProcessStartInfo startInfo = new ProcessStartInfo
                    {
                        FileName = "explorer.exe",
                        Arguments = $"shell:AppsFolder\\{appData.AppUserModelID}",
                        UseShellExecute = true
                    };
                    //log?.Info($"[{myName}] LaunchApp, Launching UWP app: {appData.AppName}");
                    process = Process.Start(startInfo);
                }
                else //Win32
                {
                    if (!File.Exists(appData.AppPath)) // Win32直接比對路徑檔案
                    {
                        _vm?.WriteLog($"@LaunchApp error: Win32 File not found - {appData.AppPath}");
                        return null; // 找不到路徑檔案
                    }
                    // Desktop exe或檔案
                    ProcessStartInfo startInfo = new ProcessStartInfo
                    {
                        FileName = appData.AppPath,
                        UseShellExecute = true,  // 系統自動選擇應用程式來開啟
                        Verb = "open"            // 指定開啟檔案的動作
                    };
                    //log?.Info($"[{myName}] LaunchApp, Launching desktop app or file: {appData.AppName}");
                    process = Process.Start(startInfo);

                }
            }
            catch (Exception ex)
            {
                _vm?.WriteLog($"@LaunchApp exception.", ex);
            }

            return process;
        }

        private static IntPtr GetWindowHandle(Bind_AddFullPage_AppCollectionData appData, ILog? log = null)
        {
            IntPtr windowHandle = IntPtr.Zero;
            Trace.WriteLine($"[LaunchAndArrange] GetWindowHandle ... in");
            //log?.Info($"[LaunchAndArrange] GetWindowHandle ... in");
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
                        //log?.Info($"[LaunchAndArrange] GetWindowHandle hWnd = {hWnd}, windowText = {windowText}");
                        windowHandle = hWnd;
                    }

                    return true;
                }, IntPtr.Zero);

                if (windowHandle == IntPtr.Zero)
                {
                    //log?.Error($"[{myName}] GetWindowHandle, Failed to get window handle for {appData.AppName}");
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
        private static bool _EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam)
        {
            return EnumWindows(lpEnumFunc, lParam);
        }
        private static bool EzMemoryEnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam)
        {
            bool rst = EnumWindows(lpEnumFunc, lParam);
            if (!rst)
            {
                Trace.WriteLine("[EzMemLauncherWindow] EnumWindows failed");
#if DEBUG
                Console.WriteLine("[EzMemLauncherWindow] EnumWindows failed");
#endif
            }
            return rst;
        }
        #endregion Phase II - Launch App and Arrange to Layout's CellBorder


        #region EzMemLaunch
        //Derek 2025/03/31
        private void ExitUIThread()
        {
            if (System.Windows.Threading.Dispatcher.CurrentDispatcher != null)
            {
                System.Windows.Threading.Dispatcher.CurrentDispatcher.InvokeShutdown();
            }
        }

        public bool ShowForEzMemLauncher(MonitorInfo mi, ISplitCtrl isp)
        {
            Screen? screen = Screen.AllScreens.FirstOrDefault(x => x.DeviceName.Equals(mi.DisplayName, StringComparison.OrdinalIgnoreCase));
            if (screen == null)
            {
                
                return false;
            }

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

            return true;
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
            _ = Dispatcher.BeginInvoke(new Action(() =>
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
                _ = Task.Delay(500);
                if (rcArrange.IsEmpty || (rcArrange.Width <= 0))
                {
                    rcArrange = GetFrameworkElementRect(celObj.CellBd);
                    if (rcArrange.IsEmpty)
                    {
                        return;
                    }
                }
                //_vm.WriteLog($"[{myName}] hWnd={hWnd}, Cell[{idxCell}], Rect(({rcArrange.Left},{rcArrange.Top}){rcArrange.Width}x{rcArrange.Height})");

                //Inflate the rect, because the rcArrange not include the border thickness(=6) of CellBorder
                //if (VM.IsWithoutGap)
                //{
                //    rcArrange.Inflate(6, 6);
                //}
                //Task.Delay(500);
                //WinEventHook.SetWindowPosition(hWnd, rcArrange);
                Rect rcActualArranged = _vm.SetEAWindowPos(hWnd, rcArrange, _workingArea);

                _alreadyArrangedCount++;
                if (AreAllAppsArranged &&
                    ArrangeDone != null)
                {
                    _vm.WriteLog($"Send ArrangeDone event.");
                    ArrangeDone(this, EventArgs.Empty);
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

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _vm.WriteLog($"EzMemLauncherWindow is closing");

            ContentRendered -= Window_ContentRendered;
            ExitUIThread(); //Derek 2025/03/31
        }
    }
}
