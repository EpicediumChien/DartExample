using DDPM.SA.Common;
using DDPM.SA.Common.Settings;
using DDPM.UI.Common;
using DDPM.UI.Common.Models;
using DDPM.UI.Plugin.Common.ViewModels;
using Dell.Client.Framework.Common;
using Dell.Client.Framework.UX.WPF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Windows.Management.Deployment;
using Microsoft.VisualBasic.Logging;

namespace DDPM.UI.Module.EzMemory
{
    /// <summary>
    /// EzMemoryLaunchOption.xaml 的互動邏輯
    /// </summary>
    public partial class EzMemoryLaunchOption : UserControl
    {      
        //For UWP
        [DllImport("user32.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);
        private bool EzMemoryEnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam)
        {
            return EnumWindows(lpEnumFunc, lParam);
        }

        [DllImport("user32.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        static extern int GetWindowTextLength(IntPtr hWnd);
        private int EzMemoryGetWindowTextLength(IntPtr hWnd)
        {
            return GetWindowTextLength(hWnd);
        }

        [DllImport("user32.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);
        private int EzMemoryGetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount)
        {
            return GetWindowText(hWnd, lpString, nMaxCount);
        }

        [DllImport("user32.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);
        private bool EzMemorySetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags)
        {
            return SetWindowPos(hWnd, hWndInsertAfter, X, Y, cx, cy, uFlags);
        }

        [DllImport("user32.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);
        private int EzMemoryGetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount)
        {
            return GetClassName(hWnd, lpClassName, nMaxCount);
        }
        [DllImport("user32.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        private bool EzMemoryGetWindowRect(IntPtr hWnd, out RECT lpRect)
        {
            return GetWindowRect(hWnd, out lpRect);
        }

        [DllImport("user32.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        private bool EzMemorySetForegroundWindow(IntPtr hWnd)
        {
            return SetForegroundWindow(hWnd);
        }

        // 检查窗口是否可见
        [DllImport("user32.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool IsWindowVisible(IntPtr hWnd);
        private bool EzMemoryIsWindowVisible(IntPtr hWnd)
        {
            return IsWindowVisible(hWnd);
        }
        //  DPI 
        [DllImport("user32.dll")]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        private static extern IntPtr MonitorFromWindow(IntPtr hwhWndnd, uint dwFlags);
        private IntPtr EzMemoryMonitorFromWindow(IntPtr hWnd, uint dwFlags)
        {
            return MonitorFromWindow(hWnd, dwFlags);
        }

        [DllImport("shcore.dll")]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        private static extern int GetDpiForMonitor(IntPtr hmonitor, MONITOR_DPI_TYPE dpiType, out uint dpiX, out uint dpiY);
        private IntPtr EzMemoryGetDpiForMonitor(IntPtr hmonitor, MONITOR_DPI_TYPE dpiType, out uint dpiX, out uint dpiY)
        {
            return GetDpiForMonitor(hmonitor, dpiType, out dpiX, out dpiY);
        }

        private const uint MONITOR_DEFAULTTONEAREST = 0x00000002;

        private enum MONITOR_DPI_TYPE
        {
            MDT_EFFECTIVE_DPI = 0,
            MDT_ANGULAR_DPI = 1,
            MDT_RAW_DPI = 2,
            MDT_DEFAULT = MDT_EFFECTIVE_DPI
        }
        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }
        private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

        const uint SWP_SHOWWINDOW = 0x0040;
        #region Private Members
        private HomeDevice _homeDevice;
        private IDeviceManagerSA _deviceManagerSA;
        private DDPM.UI.Common.ViewModels.EzMemoryViewModel _vm;
        private readonly DisplayViewModel _vmDisplay;
        private readonly IConsole _console;
        private readonly ILog _log;
        #endregion Private Members
        public EzMemoryLaunchOption(DisplayViewModel vmDisplay)
        {
            _vmDisplay = vmDisplay;
            _homeDevice = vmDisplay.SelectedHomeDevice;
            _console = vmDisplay.Console;
            _deviceManagerSA = HomeDevice.DeviceManagerSA;

            InitializeComponent();

            if (_homeDevice.vmEzMemory == null)
            {
                _homeDevice.vmEzMemory = new DDPM.UI.Common.ViewModels.EzMemoryViewModel(_homeDevice);
            }
            _vm = _homeDevice.vmEzMemory;
            DataContext = _homeDevice.vmEzMemory;

            InitializeComponent();

            InitializePage();

            _vm.ProgressValue = 3;
        }

    public void InitializePage()
    {
        _vm.ezPages = _vm.GetEzPages();

        if (_vm.ezPages.ContainsKey(_vm._currentDeviceModel))
        {
            _vm.CurrentAnimationPage = _vm.ezPages[_vm._currentDeviceModel].Count;
            var pageData = _vm.ezPages[_vm._currentDeviceModel][2];
            MainText.Text = pageData.MainText!;
            SubText.Text = pageData.SubText!;
        }
    }

    private void ArrowButton_Click(object sender, RoutedEventArgs e)
        {
            _vm.ProgressValue = 2;
            EzMemoryAssignProgram _ezMemoryAssignProgram = new EzMemoryAssignProgram(_vmDisplay);
            DdpmCommonHelper.ModuleOwner?.OpenFullView(_ezMemoryAssignProgram);
        }

        private void FinishBtn_Click(object sender, RoutedEventArgs e)
        {
            if (_vm._sortApps.Count >= 2)
            {
                LaunchAndArrangeApps();
            }

            DdpmCommonHelper.ModuleOwner?.CloseFullView();
        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            DdpmCommonHelper.ModuleOwner?.CloseFullView();
        }

        public void LaunchAndArrangeApps()
        {
            int appCount = _vm._sortApps.Count;
            if (appCount == 0)
                return;

            double screenWidth = SystemParameters.PrimaryScreenWidth;
            double screenHeight = SystemParameters.PrimaryScreenHeight;
            double widthPerApp = screenWidth / appCount; // 平均分配寬度

            var sortedByKey = _vm._sortApps.OrderBy(x => x.Key).ToList();
            _vm._seletcApps = sortedByKey.Select(x => x.Value).ToList();

            Task.Run(async () =>
            {
                List<IntPtr> windowHandles = new List<IntPtr>();

                for (int i = 0; i < appCount; i++)
                {
                    var app = _vm._seletcApps[i];
                    IntPtr handle = IntPtr.Zero;

                    Process[] processes = GetProcessesByName(app);
                    Trace.WriteLine("GetProcessesByName(app); " + app.AppName);

                    if (processes.Length > 0)
                    {
                        handle = processes[0].MainWindowHandle;
                        Trace.WriteLine("GetProcessesByName(app); " + app.AppName + " || " + handle.ToString());
                        EzMemorySetForegroundWindow(handle);
                    }
                    else
                    {
                        Process process = LaunchApp(app);
                        Trace.WriteLine("LaunchApp(app); " + app.AppName);
                        process.WaitForInputIdle();
                        for (int attempt = 0; attempt < 10; attempt++)
                        {
                            if (app.AppType == "True")
                            {
                                handle = process.MainWindowHandle;
                            }
                            else
                            {
                                handle = GetWindowHandle(app);
                            }
                            Trace.WriteLine("GetWindowHandle(app); " + app.AppName);
                            if (handle != IntPtr.Zero && IsWindowVisible(handle) && !windowHandles.Contains(handle))
                            {
                                EzMemorySetForegroundWindow(handle);
                                Trace.WriteLine("EzMemorySetForegroundWindow(app); " + app.AppName);
                                break;
                            }

                            await Task.Delay(2000);
                        }

                        if (handle == IntPtr.Zero)
                        {
                            Trace.WriteLine("Ghandle == IntPtr.Zero " + app.AppName);
                            _log.Info($"[EzMemoryLaunchOption], {i} handle null");
                            return;
                        }
                    }

                    float dpiScale = GetDpiScaleForWindow(handle);
                    Trace.WriteLine("GetDpiScaleForWindow(app); " + app.AppName);

                    EzMemorySetWindowPos(handle, IntPtr.Zero, (int)((i * widthPerApp) * dpiScale), 0, (int)(widthPerApp * dpiScale), (int)(screenHeight * dpiScale), SWP_SHOWWINDOW);
                    Trace.WriteLine("EzMemorySetWindowPos(app); " + app.AppName);


                    for (int checkAttempt = 0; checkAttempt < 10; checkAttempt++)
                    {
                        if (EzMemoryGetWindowRect(handle, out RECT rect))
                        {
                            Trace.WriteLine("EzMemoryGetWindowRect(app); " + app.AppName);
                            if (rect.Left == (int)((i * widthPerApp) * dpiScale) && rect.Top == 0 && rect.Right == (int)(((i + 1) * widthPerApp) * dpiScale) && rect.Bottom == (int)(screenHeight * dpiScale) && IsWindowVisible(handle)) // 检查窗口是否可见
                            {
                                //Trace.WriteLine("GetDpiScaleForWindow(app); " + app.AppName);
                                break;
                            }
                            else
                            {
                                Trace.WriteLine("XXXXXXXXXXXXXXXXXXXX " + app.AppName);
                            }
                        }

                        await Task.Delay(2000);
                    }

                    await Task.Delay(2000); // 额外的等待时间以确保每个应用程序的启动
                    Trace.WriteLine("結束; " + app.AppName);
                }
                _vm.ClearTextBlockAppName();
            });
        }

        //public void LaunchAndArrangeApps()
        //{
        //    int appCount = _vm._sortApps.Count;
        //    if (appCount == 0)
        //        return;

        //    double screenWidth = SystemParameters.PrimaryScreenWidth;
        //    double screenHeight = SystemParameters.PrimaryScreenHeight;
        //    double widthPerApp = screenWidth / appCount; // 平均分配寬度

        //    var sortedByKey = _vm._sortApps.OrderBy(x => x.Key).ToList();
        //    _vm._seletcApps = sortedByKey.Select(x => x.Value).ToList();

        //    Task.Run(async () =>
        //    {
        //        List<IntPtr> windowHandles = new List<IntPtr>();

        //        for (int i = 0; i < appCount; i++)
        //        {
        //            var app = _vm._seletcApps[i];
        //            IntPtr handle = IntPtr.Zero;

        //            // 檢查應用程式是否已經存在
        //            Process[] processes = GetProcessesByName(app);
        //            Trace.WriteLine("GetProcessesByName(app); " + app.AppName);
        //            //Process[] processes = GetProcessesByName(app.AppType == "True" ? System.IO.Path.GetFileNameWithoutExtension(app.AppPath) : app.AppUserModelID);
        //            if (processes.Length > 0)
        //            {
        //                handle = processes[0].MainWindowHandle;
        //                Trace.WriteLine("GetProcessesByName(app); " + app.AppName + " || " + handle.ToString());
        //                EzMemorySetForegroundWindow(handle); // 把應用程式拉到前景
        //            }
        //            else
        //            {
        //                Process process = LaunchApp(app);
        //                process.WaitForInputIdle();
        //                for (int attempt = 0; attempt < 10; attempt++)
        //                {
        //                    if (app.AppType == "True")
        //                    {
        //                        handle = process.MainWindowHandle;
        //                    }
        //                    else
        //                    {
        //                        handle = GetWindowHandle(app);
        //                    }

        //                    if (handle != IntPtr.Zero && !windowHandles.Contains(handle))
        //                        break;

        //                    await Task.Delay(2000);
        //                }

        //                if (handle == IntPtr.Zero)
        //                {
        //                    _log.Info($"[EzMemoryLaunchOption], {i} handle null");
        //                    return;
        //                }
        //            }

        //            // 取得視窗的 DPI 設定
        //            float dpiScale = GetDpiScaleForWindow(handle);

        //            // 調整視窗位置與大小，考慮 DPI 比例
        //            EzMemorySetWindowPos(handle, IntPtr.Zero,
        //                (int)((i * widthPerApp) * dpiScale),
        //                0,
        //                (int)(widthPerApp * dpiScale),
        //                (int)(screenHeight * dpiScale),
        //                SWP_SHOWWINDOW);

        //            // 確認視窗是否已移動到預期的位置
        //            for (int checkAttempt = 0; checkAttempt < 10; checkAttempt++)
        //            {
        //                if (EzMemoryGetWindowRect(handle, out RECT rect))
        //                {
        //                    if (rect.Left == (int)((i * widthPerApp) * dpiScale) && rect.Top == 0 &&
        //                        rect.Right == (int)(((i + 1) * widthPerApp) * dpiScale) && rect.Bottom == (int)(screenHeight * dpiScale))
        //                    {
        //                        break;
        //                    }
        //                }

        //                await Task.Delay(2000);
        //            }

        //            await Task.Delay(2000);
        //        }
        //        _vm.ClearTextBlockAppName();
        //    });
        //}

        private Process LaunchApp(Bind_AddFullPage_AppCollectionData appData)
        {
            if (appData.AppType == "False")
            {
                // UWP
                try
                {
                    ProcessStartInfo startInfo = new ProcessStartInfo
                    {
                        FileName = "explorer.exe",
                        Arguments = $"shell:AppsFolder\\{appData.AppUserModelID}",
                        UseShellExecute = true
                    };
                    return Process.Start(startInfo);
                }
                catch (Exception ex)
                {
                    _log.Info($"[EzMemoryLaunchOption], UWP Process.Start Exception {ex}");                   
                }
            }
            else
            {
                // Desktop
                try
                {
                    return Process.Start(appData.AppPath);
                }
                catch (Exception ex)
                {
                    _log.Info($"[EzMemoryLaunchOption], Desktop Process.Start Exception {ex}");
                }
            }

            return null;
        }

        private IntPtr GetWindowHandle(Bind_AddFullPage_AppCollectionData appData)
        {
            IntPtr windowHandle = IntPtr.Zero;

            EzMemoryEnumWindows((hWnd, lParam) =>
            {
                int length = EzMemoryGetWindowTextLength(hWnd);
                if (length == 0) return true;

                StringBuilder windowName = new StringBuilder(length);
                EzMemoryGetWindowText(hWnd, windowName, length + 1);

                if (appData.AppType == "False")
                {
                    string className = GetWindowClassName(hWnd);
                    if (className.Contains("ApplicationFrameWindow"))
                    {
                        windowHandle = hWnd;
                        return false;
                    }
                }

                return true;
            }, IntPtr.Zero);

            return windowHandle;
        }

        private Process[] GetProcessesByName(Bind_AddFullPage_AppCollectionData appData)
        {
            if (appData.AppType == "False")
            {
                // UWP 
                return Process.GetProcessesByName(appData.AppUserModelID);
            }
            else
            {
                // Desktop
                return Process.GetProcessesByName(System.IO.Path.GetFileNameWithoutExtension(appData.AppPath));
            }
        }

        private float GetDpiScaleForWindow(IntPtr hWnd)
        {
            // 預設的 DPI scaling 值是 1.0（即 100% scaling）
            float dpiScale = 1.0f;

            // 獲取螢幕 DPI，並轉換為比例
            IntPtr monitor = MonitorFromWindow(hWnd, MONITOR_DEFAULTTONEAREST);
            if (monitor != IntPtr.Zero)
            {
                uint dpiX, dpiY;
                if (GetDpiForMonitor(monitor, MONITOR_DPI_TYPE.MDT_EFFECTIVE_DPI, out dpiX, out dpiY) == 0)
                {
                    dpiScale = dpiX / 96.0f; // 96 DPI 是預設的 100% scaling
                }
            }

            return dpiScale;
        }

        private string GetWindowClassName(IntPtr hWnd)
        {
            StringBuilder className = new StringBuilder(256);
            EzMemoryGetClassName(hWnd, className, className.Capacity);
            return className.ToString();
        }
    }
}
