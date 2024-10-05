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
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using Windows.ApplicationModel;
using VcpCore.Common;

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
        private DDPM.UI.Common.ViewModels.EzArrangeViewModel _vm;
        private readonly DisplayViewModel _vmDisplay;
        private readonly IConsole _console;
        private readonly ILog _log;
        #endregion Private Members

        String ezMemoryStartupErrorTitleString = "Error";
        String ezMemoryStartupErrorString = "Another profile is set to launch during PC startup. Do you want to replace it with this profile?";

        public EzMemoryLaunchOption(DisplayViewModel vmDisplay)
        {
            _vmDisplay = vmDisplay;
            _homeDevice = vmDisplay.SelectedHomeDevice;
            _console = vmDisplay.Console;
            _deviceManagerSA = HomeDevice.DeviceManagerSA;

            InitializeComponent();

            if (_homeDevice.vmEzArrange == null)
            {
                _homeDevice.vmEzArrange = new DDPM.UI.Common.ViewModels.EzArrangeViewModel(_homeDevice);
            }
            _vm = _homeDevice.vmEzArrange;
            DataContext = _homeDevice.vmEzArrange;

            InitializeComponent();

            InitializePage();

            _vm.ProgressValue = 3;
        }

        public void InitializePage()
        {
            TitleTB.Text = "Select a launch option";
            StartupCB.Content = "Launch during PC startup";
            ManulRB.Content = "Manually select the profiles created";
            AutoRB.Content = "Automatically launch by time";
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
            bool result = false;
            if (_vm._sortApps.Count >= 2)
            {
                int profileID = 0;
                string profileName = _vm.InputText;
                int layout = 0; // Layout 可以先設為 0 
                int _number = 0;
                List<EAProfileDDPM> newEAProfileDDPM = new List<EAProfileDDPM>();
                newEAProfileDDPM = DdpmCommonHelper.DeviceManagerSA.ReadUserEAProfileDDPM().Result;
                if(newEAProfileDDPM != null)
                {
                    _number = newEAProfileDDPM.Count;
                }


                List<EAAppInfoDDPM> _appInfos = new List<EAAppInfoDDPM>();
                foreach (var app in _vm._sortApps.Values)
                {
                    _appInfos.Add(new EAAppInfoDDPM(app.AppName, app.AppPath, bool.Parse(app.AppType), app.AppUserModelID, String.Empty));
                }


                EAProfileDDPM _eAProfileDDPM = new EAProfileDDPM(
                    id: _number,
                    name: _vm.InputText,
                    layout: layout,
                    apps: _appInfos);
                //newEAProfileDDPM.Add(_eAProfileDDPM);
                result = DdpmCommonHelper.DeviceManagerSA.WriteUserEAProfileDDPM(_eAProfileDDPM).Result;

                List<EAProfileDDPM> newEAProfileDDPM11 = new List<EAProfileDDPM>();
                newEAProfileDDPM11 = DdpmCommonHelper.DeviceManagerSA.ReadUserEAProfileDDPM().Result;

                // 計算 AutoStartTime
                long autoLaunchtime = default(long);
                if (_vm.IsAutoLaunch)
                {
                    int hour = int.TryParse(_vm.SelectedHour, out var h) ? h : 0;
                    int minute = int.TryParse(_vm.SelectedMinute, out var m) ? m : 0;
                    autoLaunchtime = (long)(hour * 3600 + minute * 60); // 將小時和分鐘轉換為秒數
                }

                EasyArrangementDDPM _easyArrangementDDPM = new EasyArrangementDDPM();
                EzProfileSettingDDPM _ezProfileSettingDDPM = new EzProfileSettingDDPM(
                    id: profileID,
                    auto: _vm.IsAutoLaunch,
                    autostarttime: autoLaunchtime,
                    startuplaunch: _vm.IsLaunchAtStartup
                    );

                DesktopDDPM _desktopDDPM = new DesktopDDPM(string.Empty ,0);
                _easyArrangementDDPM.Desktops.Add( _desktopDDPM );
                //_easyArrangementDDPM.Desktops = _ezProfileSettingDDPM.
                //_easyArrangementDDPM.Desktops

                if (_easyArrangementDDPM.Desktops[0].ProfileSettings == null)
                {
                    List<EzProfileSettingDDPM> profileSettings = new List<EzProfileSettingDDPM>();
                    _easyArrangementDDPM.Desktops[0].ProfileSettings = profileSettings;
                }
                else
                {
                    _easyArrangementDDPM.Desktops[0].ProfileSettings.Add(_ezProfileSettingDDPM);
                }

                result = DdpmCommonHelper.DeviceManagerSA.WriteMonitorEasyArrangement(_homeDevice.MonitorInfo, _easyArrangementDDPM).Result;

                //DdpmCommonHelper.DeviceManagerSA.WriteEzProfiles(profile);

                _deviceManagerSA.LaunchAndArrangeApps(_vm._sortApps);

            }
            _vm.ClearTextBlockAppName();
            DdpmCommonHelper.ModuleOwner?.CloseFullView();
        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            DdpmCommonHelper.ModuleOwner?.CloseFullView();
        }
        /// <summary>
        /// 兩個UWPOK
        /// </summary>
        /// <param name="appData"></param>
        /// <returns></returns>
        //public void LaunchAndArrangeApps()
        //{
        //    //if (_vm._seletcApps.Count < 2)
        //    //    return;


        //    var sortedByKey = _vm._sortApps.OrderBy(x => x.Key).ToList();
        //    _vm._seletcApps = sortedByKey.Select(x => x.Value).ToList();

        //    var firstApp = _vm._seletcApps[0];
        //    var secondApp = _vm._seletcApps[1];

        //    Task.Delay(3000).ContinueWith(async t =>
        //    {
        //        IntPtr firstHandle = IntPtr.Zero;
        //        IntPtr secondHandle = IntPtr.Zero;
        //        Process firstProcess = LaunchApp(firstApp);
        //        for (int i = 0; i < 10; i++)
        //        {
        //            firstHandle = GetWindowHandle(firstApp);
        //            //secondHandle = GetWindowHandle(secondApp);

        //            if (firstHandle != IntPtr.Zero)
        //                break;

        //            await Task.Delay(1000);
        //        }
        //        Process secondProcess = LaunchApp(secondApp);
        //        for (int i = 0; i < 10; i++)
        //        {
        //            secondHandle = GetWindowHandle(secondApp);

        //            if (secondHandle != IntPtr.Zero && secondHandle != firstHandle)
        //                break;

        //            await Task.Delay(1000);
        //        }

        //        if (firstHandle == IntPtr.Zero || secondHandle == IntPtr.Zero)
        //        {
        //            //Debug
        //            return;
        //        }

        //        Application.Current.Dispatcher.Invoke(() =>
        //        {
        //            double screenWidth = SystemParameters.PrimaryScreenWidth;
        //            double screenHeight = SystemParameters.PrimaryScreenHeight;

        //            SetWindowPos(firstHandle, IntPtr.Zero, 0, 0, (int)(screenWidth / 2), (int)screenHeight, SWP_SHOWWINDOW);

        //            SetWindowPos(secondHandle, IntPtr.Zero, (int)(screenWidth / 2), 0, (int)(screenWidth / 2), (int)screenHeight, SWP_SHOWWINDOW);
        //        });

        //    });
        //}


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

                    // 檢查應用程式是否已經存在
                    Process[] processes = GetProcessesByName(app);
                    Trace.WriteLine("GetProcessesByName(app); " + app.AppName);
                    //Process[] processes = GetProcessesByName(app.AppType == "True" ? System.IO.Path.GetFileNameWithoutExtension(app.AppPath) : app.AppUserModelID);
                    if (processes.Length > 0)
                    {
                        handle = processes[0].MainWindowHandle;
                        Trace.WriteLine("GetProcessesByName(app); " + app.AppName + " || " + handle.ToString());
                        EzMemorySetForegroundWindow(handle); // 把應用程式拉到前景
                    }
                    else
                    {
                        Process process = LaunchApp(app);
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

                            if (handle != IntPtr.Zero && !windowHandles.Contains(handle))
                                break;

                            await Task.Delay(2000);
                        }

                        if (handle == IntPtr.Zero)
                        {
                            _log.Info($"[EzMemoryLaunchOption], {i} handle null");
                            return;
                        }
                    }

                    // 取得視窗的 DPI 設定
                    float dpiScale = GetDpiScaleForWindow(handle);

                    // 調整視窗位置與大小，考慮 DPI 比例
                    EzMemorySetWindowPos(handle, IntPtr.Zero,
                        (int)((i * widthPerApp) * dpiScale),
                        0,
                        (int)(widthPerApp * dpiScale),
                        (int)(screenHeight * dpiScale),
                        SWP_SHOWWINDOW);

                    // 確認視窗是否已移動到預期的位置
                    for (int checkAttempt = 0; checkAttempt < 10; checkAttempt++)
                    {
                        if (EzMemoryGetWindowRect(handle, out RECT rect))
                        {
                            if (rect.Left == (int)((i * widthPerApp) * dpiScale) && rect.Top == 0 &&
                                rect.Right == (int)(((i + 1) * widthPerApp) * dpiScale) && rect.Bottom == (int)(screenHeight * dpiScale))
                            {
                                break;
                            }
                        }

                        await Task.Delay(2000);
                    }

                    await Task.Delay(2000);
                }
                //_vm.ClearTextBlockAppName();
            });
        }

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

        private void StartupCB_Checked(object sender, RoutedEventArgs e)
        {
            DdpmCommonHelper.DDPMMesssageBox(ezMemoryStartupErrorTitleString, ezMemoryStartupErrorString);
        }
    }
}
