using Dell.Client.Framework.Common;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using static DDPM.ColorApp.WindowFocusWatcher;

//using static System.Runtime.InteropServices.JavaScript.JSType;

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

        [DllImport("USER32.DLL", CharSet = CharSet.Auto, SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        private static extern int GetWindowThreadProcessId(IntPtr hWnd, out uint nProcessId);

        public static int _GetWindowThreadProcessId(IntPtr hWnd, out uint nProcessId)
        {
            return GetWindowThreadProcessId(hWnd, out nProcessId);
        }

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

        private WindowFocusWatcher? focusWatcher = default;
        private WindowFocusWatcher? moveWatcher = default;

        private static string _LastforgroundTitle = string.Empty;
        private static string _LastLocatedScreen = string.Empty;

        public static event EventHandler? SendValue;

        //////private static Logger logger = new Logger("ColorApp");

        // 20240823 jim add - declare log variable
        private static ILog Log { get; set; }

        public AppStatusQuery(ILog log)
        {
            Log = log;
            writelog("AppStatusQuery()");

            ///////logger.SetLogModule("ColorApp");
            try
            {
                focusWatcher = new WindowFocusWatcher(WindowFocusWatcherEvent, Native.EVENT_OBJECT_FOCUS);
                moveWatcher = new WindowFocusWatcher(WindowMoveResizeWatcherEvent, Native.EVENT_SYSTEM_MOVESIZEEND);
            }
            catch(Exception ex)
            {
                writelog($"[AppStatusQuery] AppStatusQuery initial failed, message: {ex.Message}");
            }
        }

    ~AppStatusQuery()
        {
        }

        public static AppStatusQuery GetInstance(ILog log)
        {
            if (INSTANCE == null)
            {
                INSTANCE = new AppStatusQuery(log);
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

            string strlog;

            var process = Process.GetProcessById((int)pid);

            if (process.ProcessName != "ApplicationFrameHost")
            {
                _realProcess = process;

                //logger.WriteLog($"[Watcher-callback] real process: ProcessName[{_realProcess.ProcessName}]ModuleName[{_realProcess.MainModule.ModuleName}]Title[{_realProcess.MainWindowTitle}]");

                //string strlog;
                strlog = String.Format($"[Watcher-callback] real process: ProcessName[{_realProcess.ProcessName}]ModuleName[{_realProcess.MainModule.ModuleName}]Title[{_realProcess.MainWindowTitle}]");
                writelog(strlog);

                //return false;
            }

            //string strlog;
            strlog = String.Format($"[Watcher-callback] process: ProcessName[{process.ProcessName}]ModuleName[{process.MainModule.ModuleName}]Title[{process.MainWindowTitle}]");
            writelog(strlog);

            return true;
        }

        #endregion get real process id for uwp kind app

        public static int GetWindowProcessId(IntPtr hwnd)
        {
            uint pid;
            _GetWindowThreadProcessId(hwnd, out pid);
            return (int)pid;
        }

        private static void pass_process_info_to_callback(uint pid, IntPtr hWnd, string forgroundTitle)
        {
            Process? forgroundProcess;
            string strProcessName = "";
            string strFilePath = "";
            string strlog;

            try
            {
                forgroundProcess = Process.GetProcessById((int)pid);

                if (forgroundProcess.ProcessName == "ApplicationFrameHost")
                {
                    ///////logger.WriteLog($"[Watcher-callback] Got sandbox app, retrieve process info by process id");
                    ///

                    //string strlog;
                    strlog = String.Format($"[Watcher-callback] Got sandbox app, retrieve process info by process id");
                    writelog(strlog);

                    for (int i = 0; i < 4; i++)
                    {
                        Thread.Sleep(1000);
                        forgroundProcess = Process.GetProcessById(GetWindowProcessId(Native._GetForegroundWindow()));
                        forgroundProcess = GetRealProcess(forgroundProcess);

                        if (forgroundProcess.ProcessName != "ApplicationFrameHost")
                            break;
                    }

                    //forgroundProcess = GetRealProcess(forgroundProcess);

                    //forgroundProcess = _realProcess;
                }

                strlog = String.Format($"[Watcher-callback] foregroundProcess.ProcessName = {forgroundProcess.ProcessName} process id({pid}) Title({forgroundTitle}) ");
                writelog(strlog);

                //check process content
                if (forgroundProcess == null || forgroundProcess.MainModule == null ||
                    forgroundProcess.MainModule.ModuleName == null || forgroundProcess.MainModule.FileName == null)
                {
                    ///////logger.WriteLog($"[Watcher-callback] process id({pid}) Title({forgroundTitle}) to Process object got null content, drop it");

                    //string strlog;
                    strlog = String.Format($"[Watcher-callback] process id({pid}) Title({forgroundTitle}) to Process object got null content, drop it");
                    writelog(strlog);

                    return;
                }
                strProcessName = forgroundProcess.MainModule.ModuleName;
                strFilePath = forgroundProcess.MainModule.FileName;

                ///////logger.WriteLog($"[Watcher-callback] {strProcessName}:>Title({forgroundTitle}):PID({pid}):hWnd({hWnd}):Path({strFilePath})");

                //string strlog;
                strlog = String.Format($"[Watcher-callback] {strProcessName}:>Title({forgroundTitle}):PID({pid}):hWnd({hWnd}):Path({strFilePath})");
                writelog(strlog);

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
            //string activeTitle = GetAWindowTitle(hwnd);
            //var hWnd = Native._GetForegroundWindow();
            uint pid;
            //Native._GetWindowThreadProcessId(hWnd, out pid);
            Native._GetWindowThreadProcessId(hwnd, out pid);

            if (pid == 0)
                return;

            //string forgroundTitle = GetAWindowTitle(hWnd);
            string forgroundTitle = GetAWindowTitle(hwnd);

            if (string.IsNullOrEmpty(forgroundTitle))
                return;

            ///////logger.WriteLog($"[Watcher-Focus] found:{forgroundTitle}");
            ///
            string strlog;
            //strlog = String.Format($"[Watcher-Focus] found:{forgroundTitle}");
            strlog =$"[Watcher-Focus] found:{forgroundTitle}";
            writelog(strlog);

            //Screen screen = Screen.FromHandle(hWnd);
            Screen screen = Screen.FromHandle(hwnd);

            if (!string.IsNullOrEmpty(_LastforgroundTitle) && String.Compare(_LastforgroundTitle, forgroundTitle) == 0)
            {
                //string strlog;
                strlog = $"[Watcher-Focus]  Same as last app, drop event";
                writelog(strlog);

                ///////logger.WriteLog($"[Watcher-Focus]  Same as last app, drop event");
                return;
            }

            _LastforgroundTitle = forgroundTitle;
            _LastLocatedScreen = screen.DeviceName;

            //pass_process_info_to_callback(pid, hWnd, forgroundTitle);
            pass_process_info_to_callback(pid, hwnd, forgroundTitle);
        }

        private static void WindowMoveResizeWatcherEvent(IntPtr hwnd)
        {
            //uint uid = 0;
            //GetWindowThreadProcessId(hwnd, out uid);
            //string activeTitle = GetAWindowTitle(hwnd);
            //var hWnd = Native._GetForegroundWindow();
            uint pid;
            //Native._GetWindowThreadProcessId(hWnd, out pid);
            Native._GetWindowThreadProcessId(hwnd, out pid);

            if (pid == 0)
                return;

            //string forgroundTitle = GetAWindowTitle(hWnd);
            string forgroundTitle = GetAWindowTitle(hwnd);

            if (string.IsNullOrEmpty(forgroundTitle))
                return;

            ///////logger.WriteLog($"[Watcher-Move] found:{forgroundTitle}");

            string strlog;
            strlog = String.Format($"[Watcher-Move] found:{forgroundTitle}");
            writelog(strlog);

            //Screen screen = Screen.FromHandle(hWnd);
            Screen screen = Screen.FromHandle(hwnd);

            if (String.Compare(_LastforgroundTitle, forgroundTitle) == 0)
            {
                //string strlog;
                strlog = String.Format($"[Watcher-Move]  Same as last app, check screen location");
                writelog(strlog);

                ///////logger.WriteLog($"[Watcher-Move]  Same as last app, check screen location");

                //check if differenct screen
                if (String.Compare(_LastLocatedScreen, screen.DeviceName) == 0)
                {
                    //string strlog;
                    strlog = String.Format($"[Watcher-Move]  Same as last monitor, drop move event");
                    writelog(strlog);

                    ///////logger.WriteLog($"[Watcher-Move]  Same as last monitor, drop move event");
                    return;
                }
            }

            _LastforgroundTitle = forgroundTitle;
            _LastLocatedScreen = screen.DeviceName;

            //pass_process_info_to_callback(pid, hWnd, forgroundTitle);
            pass_process_info_to_callback(pid, hwnd, forgroundTitle);
        }

        public void ClearLastAppRecord(string requestor)
        {
            _LastforgroundTitle = string.Empty;
            //logger.WriteLog($"[{requestor}]  Clear app record by requestor");

            string strlog;
            strlog = String.Format($"[{requestor}]  Clear app record by requestor");
            writelog(strlog);
        }

        private enum log_type
        {
            info = 0,
            error
        }

        private static void writelog(string? text, log_type log_type = log_type.info)
        {
            text = "[AppStatusQuery] " + text;
            System.Console.WriteLine(text);

            if (Log != null) // Elie, the instance of Log is from DTH. So we just check if it's null or not.
            {
                if (log_type == log_type.info)
                    Log.Info(text);
                else
                    Log.Error(text);
            }
        }
    }
}