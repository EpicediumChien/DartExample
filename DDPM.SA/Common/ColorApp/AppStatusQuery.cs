using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using static DDPM.ColorApp.WindowFocusWatcher;

namespace DDPM.ColorApp
{
    public class ActiveWindowData
    {
        public string ActiveWindowTitle { get; set; } = string.Empty;

        public uint ActiveWindowProcessId { get; set; } = 0;

        public string ActiveWindowProcessModuleName { get; set; } = string.Empty;

        public IntPtr ActiveWindowHandle { get; set; } = IntPtr.Zero;

        public string ActiveWindowFilePath { get; set; } = string.Empty;
    }

    public class AppStatusQuery
    {
        #region Native Win32 APIs

        /*[DllImport("USER32.DLL", CharSet = CharSet.Auto)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        private static extern int GetWindowThreadProcessId(IntPtr hWnd, out uint nProcessId);
        public static int _GetWindowThreadProcessId(IntPtr hWnd, out uint nProcessId)
        {
            return GetWindowThreadProcessId(hWnd, out nProcessId);
        }*/

        [DllImport("USER32.DLL", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);
        public static int _GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount)
        {
            return GetWindowText(hWnd, lpString, nMaxCount);
        }

        [DllImport("USER32.DLL", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        private static extern int GetWindowTextLength(IntPtr hWnd);
        public static int _GetWindowTextLength(IntPtr hWnd)
        {
            return GetWindowTextLength(hWnd);
        }

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        private static extern bool EnumChildWindows(IntPtr hwnd, WindowEnumProc callback, IntPtr lParam);
        public static bool _EnumChildWindows(IntPtr hwnd, WindowEnumProc callback, IntPtr lParam)
        {
            return EnumChildWindows(hwnd, callback, lParam);
        }

        public delegate bool WindowEnumProc(IntPtr hwnd, IntPtr lparam);

        #endregion Native Win32 APIs

        private static AppStatusQuery? INSTANCE = null;

        private WindowFocusWatcher focusWatcher = new WindowFocusWatcher(WindowFocusWatcherEvent, Native.EVENT_OBJECT_FOCUS /*| Native.WINEVENT_SKIPOWNPROCESS | Native.EVENT_OBJECT_LOCATIONCHANGE | Native.EVENT_OBJECT_SELECTION*/);
        private WindowFocusWatcher moveWatcher = new WindowFocusWatcher(WindowMoveResizeWatcherEvent, Native.EVENT_SYSTEM_MOVESIZEEND);

        private static string _LastforgroundTitle = string.Empty;
        private static string _LastLocatedScreen = string.Empty;

        public static event EventHandler? SendValue;

        //////private static Logger logger = new Logger("ColorApp");

        public AppStatusQuery()
        {
            ///////logger.SetLogModule("ColorApp");
        }

        ~AppStatusQuery()
        {
        }

        public static AppStatusQuery GetInstance()
        {
            if (INSTANCE == null)
            {
                INSTANCE = new AppStatusQuery();
            }
            return INSTANCE;
        }

        private static string GetAWindowTitle(IntPtr hWnd)
        {
            string Title = string.Empty;
            int length = _GetWindowTextLength(hWnd);
            if (length == 0) return
                    string.Empty;

            StringBuilder builder = new StringBuilder(length);
            _GetWindowText(hWnd, builder, length + 1);

            Title = builder.ToString();
            return Title;
        }

        #region get real process id for uwp kind app

        private static Process? _realProcess = null;

        private static Process? GetRealProcess(Process foregroundProcess)
        {
            _EnumChildWindows(foregroundProcess.MainWindowHandle, ChildWindowCallback, IntPtr.Zero);
            return _realProcess;
        }

        private static bool ChildWindowCallback(IntPtr hwnd, IntPtr lparam)
        {
            uint pid = 0;
            Native._GetWindowThreadProcessId(hwnd, out pid);
            var process = Process.GetProcessById((int)pid);
            if (process.ProcessName != "ApplicationFrameHost")
            {
                _realProcess = process;
                //logger.WriteLog($"[Watcher-callback] real process: ProcessName[{_realProcess.ProcessName}]ModuleName[{_realProcess.MainModule.ModuleName}]Title[{_realProcess.MainWindowTitle}]");
                return true;
            }
            return true;
        }

        #endregion get real process id for uwp kind app

        private static void pass_process_info_to_callback(uint pid, IntPtr hWnd, string forgroundTitle)
        {
            Process? forgroundProcess;
            string strProcessName = "";
            string strFilePath = "";

            try
            {
                forgroundProcess = Process.GetProcessById((int)pid);
                if (forgroundProcess.ProcessName == "ApplicationFrameHost")
                {
                    ///////logger.WriteLog($"[Watcher-callback] Got sandbox app, retrieve process info by process id");
                    forgroundProcess = GetRealProcess(forgroundProcess);
                }
                //check process content
                if (forgroundProcess == null || forgroundProcess.MainModule == null ||
                    forgroundProcess.MainModule.ModuleName == null || forgroundProcess.MainModule.FileName == null)
                {
                    ///////logger.WriteLog($"[Watcher-callback] process id({pid}) Title({forgroundTitle}) to Process object got null content, drop it");
                    return;
                }
                strProcessName = forgroundProcess.MainModule.ModuleName;
                strFilePath = forgroundProcess.MainModule.FileName;

                ///////logger.WriteLog($"[Watcher-callback] {strProcessName}:>Title({forgroundTitle}):PID({pid}):hWnd({hWnd}):Path({strFilePath})");

                if (SendValue != null)
                {
                    SendValue(
                        new ActiveWindowData()
                        {
                            ActiveWindowTitle = forgroundTitle,
                            ActiveWindowProcessId = pid,
                            ActiveWindowProcessModuleName = strProcessName,
                            ActiveWindowHandle = hWnd,
                            ActiveWindowFilePath = strFilePath
                        },
                        new EventArgs());
                }
            }
            catch (System.Exception)
            {
            }
        }

        private static void WindowFocusWatcherEvent(IntPtr hwnd)
        {
            //uint uid = 0;
            //_GetWindowThreadProcessId(hwnd, out uid);
            string activeTitle = GetAWindowTitle(hwnd);
            var hWnd = Native._GetForegroundWindow();
            uint pid;
            Native._GetWindowThreadProcessId(hWnd, out pid);

            if (pid == 0)
                return;

            string forgroundTitle = GetAWindowTitle(hWnd);
            if (string.IsNullOrEmpty(forgroundTitle))
                return;

            ///////logger.WriteLog($"[Watcher-Focus] found:{forgroundTitle}");

            Screen screen = Screen.FromHandle(hWnd);

            /* Jim remove 20240816
            if (!string.IsNullOrEmpty(_LastforgroundTitle) && String.Compare(_LastforgroundTitle, forgroundTitle) == 0)
            {
                ///////logger.WriteLog($"[Watcher-Focus]  Same as last app, drop event");
                return;
            }
            */

            _LastforgroundTitle = forgroundTitle;
            _LastLocatedScreen = screen.DeviceName;

            pass_process_info_to_callback(pid, hWnd, forgroundTitle);
        }

        private static void WindowMoveResizeWatcherEvent(IntPtr hwnd)
        {
            //uint uid = 0;
            //GetWindowThreadProcessId(hwnd, out uid);
            string activeTitle = GetAWindowTitle(hwnd);
            var hWnd = Native._GetForegroundWindow();
            uint pid;
            Native._GetWindowThreadProcessId(hWnd, out pid);

            if (pid == 0)
                return;

            string forgroundTitle = GetAWindowTitle(hWnd);
            if (string.IsNullOrEmpty(forgroundTitle))
                return;

            ///////logger.WriteLog($"[Watcher-Move] found:{forgroundTitle}");

            Screen screen = Screen.FromHandle(hWnd);

            /* Jim remove 20240816
            if (String.Compare(_LastforgroundTitle, forgroundTitle) == 0)
            {
                ///////logger.WriteLog($"[Watcher-Move]  Same as last app, check screen location");

                //check if differenct screen
                if (String.Compare(_LastLocatedScreen, screen.DeviceName) == 0)
                {
                    ///////logger.WriteLog($"[Watcher-Move]  Same as last monitor, drop move event");
                    return;
                }
            }
            */

            _LastforgroundTitle = forgroundTitle;
            _LastLocatedScreen = screen.DeviceName;

            pass_process_info_to_callback(pid, hWnd, forgroundTitle);
        }

        public void ClearLastAppRecord(string requestor)
        {
            _LastforgroundTitle = string.Empty;
            //logger.WriteLog($"[{requestor}]  Clear app record by requestor");
        }
    }
}