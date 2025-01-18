using DDPM.SA.Common;
using DDPM.SA.Common.Display;
using Dell.Client.Framework.Common;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Xml;
using Windows.System;

namespace DDPM.PowerMon
{
    /// <summary>
    /// Interaction logic for PowerMon.xaml
    /// </summary>
    public partial class PowerMonitor : Window
    {
        public bool isWindowLoaded { get; private set; } = false;
        public bool isHotkeyHooked { get; private set; } = false;

        private enum log_type
        {
            info = 0,
            error
        }
        private void WriteLog(string text, log_type log_type = log_type.info)
        {
            if (string.IsNullOrEmpty(text))
                text = "";

            Console.WriteLine($"{DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss.fff")} [PowerMonitor] {text}");
            if (log != null)
            {
                if (log_type == log_type.info)
                    log.Info(text);
                else
                    log.Error(text);
            }
        }

        #region native APIs
        [StructLayout(LayoutKind.Sequential)]
        private struct POWERBROADCAST_SETTING
        {
            public Guid PowerSetting;
            public uint DataLength;
            public byte Data;
        }

        private static Guid GUID_MONITOR_POWER_ON = new Guid("02731015-4510-4526-99E6-E5A17EBD1AEA");

        private IntPtr m_hPowerNotify = IntPtr.Zero;

        [DllImport("user32.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        private static extern IntPtr RegisterPowerSettingNotification(IntPtr hRecipient, ref Guid PowerSettingGuid, uint Flags);
        private static IntPtr _RegisterPowerSettingNotification(IntPtr hRecipient, ref Guid PowerSettingGuid, uint Flags)
        {
            return RegisterPowerSettingNotification(hRecipient, ref PowerSettingGuid, Flags);
        }

        [DllImport("user32.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        private static extern bool UnregisterPowerSettingNotification(IntPtr Handle);
        private static bool _UnregisterPowerSettingNotification(IntPtr Handle)
        {
            return UnregisterPowerSettingNotification(Handle);
        }

        #region For System Suspend/Resume Notify
        //https://learn.microsoft.com/en-us/windows/win32/w8cookbook/desktop-activity-moderator
        private const int DEVICE_NOTIFY_WINDOW_HANDLE = 0;
        private const int DEVICE_NOTIFY_CALLBACK = 2;

        [DllImport("user32.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        private static extern IntPtr RegisterSuspendResumeNotification(IntPtr hRecipient, uint Flags);
        private static IntPtr _RegisterSuspendResumeNotification(IntPtr hRecipient, uint Flags)
        {
            return RegisterSuspendResumeNotification(hRecipient, Flags);
        }

        [DllImport("user32.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        private static extern bool UnregisterSuspendResumeNotification(IntPtr Handle);
        private static bool _UnregisterSuspendResumeNotification(IntPtr Handle)
        {
            return UnregisterSuspendResumeNotification(Handle);
        }

        IntPtr m_hSuspendResumeNotify = IntPtr.Zero;
        #endregion

        //for hotkey

        [DllImport("user32.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, ModifierKeys fsModifiers, int vk);
        private static bool _RegisterHotKey(IntPtr hWnd, int id, ModifierKeys fsModifiers, int vk)
        {
            return RegisterHotKey(hWnd, id, fsModifiers, vk);
        }

        [DllImport("user32.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);
        private static bool _UnregisterHotKey(IntPtr hWnd, int id)
        {
            return UnregisterHotKey(hWnd, id);
        }


        [DllImport("kernel32.dll")]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        private static extern ushort GlobalAddAtom(string lpString);
        private static ushort _GlobalAddAtom(string lpString)
        {
            return GlobalAddAtom(lpString);
        }
        #endregion

        private List<ushort> _hotkeyIds = new List<ushort>();
        public event EventHandler<KeyPressedEventArgs> HotkeyPressed;

        private ushort _currentId;
        public bool isKeyRegistered;
        private IntPtr _handle;
        public void RegisterHotKey(List<HotkeyInfo> hotkeyInfos)
        {
            foreach (HotkeyInfo hotkeyInfo in hotkeyInfos)
            {
                if (hotkeyInfo != null && hotkeyInfo.Hotkey.Count > 0)
                {
                    if (hotkeyInfo.KeyCode.Equals(VirtualKey.None))
                    {
                        continue;
                    }
                    string uniqueID = Guid.NewGuid().ToString("N");
                    _currentId = _GlobalAddAtom(uniqueID);
                    int lastError = -1;
                    Dispatcher.Invoke((() =>
                    {
                        isKeyRegistered = _RegisterHotKey(_handle, _currentId, hotkeyInfo.ModifiersEnum, (int)hotkeyInfo.KeyCode);
                        lastError = Marshal.GetLastWin32Error();
                    }));
                    if (!isKeyRegistered)
                    {
                        switch (lastError)
                        {
                            case 1409:
                                WriteLog($"RegisterHotKey:[{hotkeyInfo.ModifiersEnum} + {hotkeyInfo.KeyCode}],Id:{_currentId},Hot key is already registered.");
                                break;
                            case 1408:
                                WriteLog($"RegisterHotKey:[{hotkeyInfo.ModifiersEnum} + {hotkeyInfo.KeyCode}],Id:{_currentId},Invalid window; it belongs to other thread.");
                                break;
                            case 1400:
                                WriteLog($"RegisterHotKey:[{hotkeyInfo.ModifiersEnum} + {hotkeyInfo.KeyCode}],Id:{_currentId},Invalid window handle.");
                                break;
                        }
                    }
                    else
                    {
                        _hotkeyIds.Add(_currentId);
                        hotkeyInfo.ID = _currentId;
                        Debug.WriteLine($"RegisterHotKey:[{hotkeyInfo.ModifiersEnum} + {hotkeyInfo.KeyCode}],id:{_currentId},The operation RegisterHotKey successfully.");
                        WriteLog($"RegisterHotKey:[{hotkeyInfo.ModifiersEnum} + {hotkeyInfo.KeyCode}],id:{_currentId},The operation RegisterHotKey successfully.");
                    }
                }
            }

        }

        public void UnRegisterHotKey(ushort id)
        {
            bool v = false;
            int lastError = -1;
            Dispatcher.Invoke((() =>
            {
                v = _UnregisterHotKey(_handle, id);
                lastError = Marshal.GetLastWin32Error();
            }));
            if (!v)
            {
                switch (lastError)
                {
                    case 1409:
                        WriteLog($"UnRegisterHotKey[Id:{id}],Hot key is already registered.");
                        break;
                    case 1408:
                        WriteLog($"UnRegisterHotKey[Id:{id}],Invalid window; it belongs to other thread.");
                        break;
                    case 1400:
                        WriteLog($"UnRegisterHotKey[Id:{id}],Invalid window handle.");
                        break;
                }

            }
            else
            {
                WriteLog($"id:UnRegisterHotKey[Id:{id}],The operation RegisterHotKey successfully.");
            }
            isKeyRegistered = !v;
        }
        public void UnRegisterAllHotKey()
        {
            bool v = false;
            int lastError = -1;
            foreach (ushort id in _hotkeyIds)
            {
                Dispatcher.Invoke((() =>
                {
                    v = _UnregisterHotKey(_handle, id);
                    lastError = Marshal.GetLastWin32Error();
                }));
                if (!v)
                {
                    switch (lastError)
                    {
                        case 1409:
                            WriteLog($"UnRegisterHotKey[Id:{id}],Hot key is already registered.");
                            break;
                        case 1408:
                            WriteLog($"UnRegisterHotKey[Id:{id}],Invalid window; it belongs to other thread.");
                            break;
                        case 1400:
                            WriteLog($"UnRegisterHotKey[Id:{id}],Invalid window handle.");
                            break;
                    }

                }
                else
                {
                    WriteLog($"id:UnRegisterHotKey[Id:{id}],The operation RegisterHotKey successfully.");
                }
            }
        }
        //for hotkey end

        public event EventHandler MonitorTurnedOn;
        public event EventHandler SystemSuspend;
        public event EventHandler SystemResume;
        //public event EventHandler MonitorTurnedOff;
        //public event EventHandler PowerSettingChanged;
        public event EventHandler CurrentSessionActived;
        public event EventHandler CurrentSessionInactived;
        private static ILog log = null;


        public PowerMonitor(ILog Log)
        {
            InitializeComponent();
            log = Log;
        }

        private void SystemEvents_SessionSwitch(object sender, SessionSwitchEventArgs e)
        {
            switch (e.Reason)
            {
                case SessionSwitchReason.SessionLock:
                    WriteLog("Session locked.");
                    break;
                case SessionSwitchReason.SessionUnlock:
                    WriteLog("Session unlocked.");
                    break;
                case SessionSwitchReason.SessionLogon:
                    WriteLog("Session logon.");
                    break;
                case SessionSwitchReason.SessionLogoff:
                    WriteLog("Session logoff.");
                    break;
                default:
                    WriteLog($"Session switch event: {e.Reason}");
                    break;
            }
            CheckCurrentSessionState();
        }

        private void CheckCurrentSessionState()
        {
            bool isSessionActive = SystemInformation.UserInteractive;
            WriteLog($"Current session({Process.GetCurrentProcess().SessionId}) is {(isSessionActive ? "active" : "inactive")}.");
            if (isSessionActive)
            {
                CurrentSessionActived?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                CurrentSessionInactived?.Invoke(this, EventArgs.Empty);
            }
        }

        //add function to handle current session

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            WindowInteropHelper helper = new WindowInteropHelper(this);
            //_handle = helper.Handle;
            //HwndSource source = HwndSource.FromHwnd(helper.Handle);
            //source.AddHook(WndProc);

            m_hPowerNotify = _RegisterPowerSettingNotification(helper.Handle, ref GUID_MONITOR_POWER_ON, 0);

            m_hSuspendResumeNotify = _RegisterSuspendResumeNotification(helper.Handle, DEVICE_NOTIFY_WINDOW_HANDLE);

            isWindowLoaded = true;

            // Add code to support session change event hook, and check if current session is active
            SystemEvents.SessionSwitch += new SessionSwitchEventHandler(SystemEvents_SessionSwitch);
            CheckCurrentSessionState();
        }

        private void Window_Unloaded(object sender, RoutedEventArgs e)
        {
            try
            {
                if (m_hPowerNotify != IntPtr.Zero)
                {
                    _UnregisterPowerSettingNotification(m_hPowerNotify);
                    m_hPowerNotify = IntPtr.Zero;
                }

                if (m_hSuspendResumeNotify != IntPtr.Zero)
                {
                    _UnregisterSuspendResumeNotification(m_hSuspendResumeNotify);
                    m_hSuspendResumeNotify = IntPtr.Zero;
                }

                if (isHotkeyHooked)
                    UnRegisterAllHotKey();

                isWindowLoaded = false;
                isHotkeyHooked = false;

                SystemEvents.SessionSwitch -= new SessionSwitchEventHandler(SystemEvents_SessionSwitch);
            }
            catch (Exception ex)
            {
                WriteLog($"[Window_Unloaded] Exception occurred: {ex.Message}");
            }
        }

        public void Enable_HotkeyHook()
        {
            if (!isWindowLoaded)
            {
                WriteLog("Window isn't active, drop enable hotkey hook");
                return;
            }
            WindowInteropHelper helper = new WindowInteropHelper(this);
            _handle = helper.Handle;
            HwndSource source = HwndSource.FromHwnd(helper.Handle);
            source.AddHook(WndProc);
            isHotkeyHooked = true;
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            const int WM_POWERBROADCAST = 0x0218;
            const int PBT_POWERSETTINGCHANGE = 0x8013;
            const int PBT_APMPOWERSTATUSCHANGE = 10;
            const int PBT_APMRESUMEAUTOMATIC = 18;
            const int PBT_APMRESUMESUSPEND = 7;
            const int PBT_APMSUSPEND = 4;

            const int WM_HOTKEY = 0x0312;

            //WriteLog($"Power event {msg.ToString()}");
            switch (msg)
            {
                case WM_POWERBROADCAST:
                    //if (wParam.ToInt32() == PBT_POWERSETTINGCHANGE)
                    //{
                    //    POWERBROADCAST_SETTING pPwrSetting = (POWERBROADCAST_SETTING)Marshal.PtrToStructure(lParam, typeof(POWERBROADCAST_SETTING));
                    //    if (pPwrSetting.PowerSetting == GUID_MONITOR_POWER_ON && pPwrSetting.DataLength == sizeof(uint))
                    //    {
                    //        WriteLog($"Power event {msg.ToString()} PBT_POWERSETTINGCHANGE handled");
                    //        WriteLog($"Power event PBT_POWERSETTINGCHANGE data: {pPwrSetting.Data}");
                    //        if (pPwrSetting.Data == 0)
                    //        {
                    //            //MonitorTurnedOff?.Invoke(this, EventArgs.Empty);
                    //        }
                    //        else if (pPwrSetting.Data == 1)
                    //        {
                    //            Task.Run(() =>
                    //                MonitorTurnedOn?.Invoke(this, EventArgs.Empty)
                    //            );
                    //        }
                    //    }
                    //    //PowerSettingChanged?.Invoke(this, EventArgs.Empty);
                    //    handled = true;
                    //}

                    int eventId = wParam.ToInt32();
                    switch (eventId)
                    {
                        case PBT_POWERSETTINGCHANGE:
                            POWERBROADCAST_SETTING pPwrSetting = (POWERBROADCAST_SETTING)Marshal.PtrToStructure(lParam, typeof(POWERBROADCAST_SETTING));
                            if (pPwrSetting.PowerSetting == GUID_MONITOR_POWER_ON && pPwrSetting.DataLength == sizeof(uint))
                            {
                                WriteLog($"Power event {msg.ToString()} PBT_POWERSETTINGCHANGE handled");
                                WriteLog($"Power event PBT_POWERSETTINGCHANGE data: {pPwrSetting.Data}");
                                if (pPwrSetting.Data == 0)
                                {
                                    //MonitorTurnedOff?.Invoke(this, EventArgs.Empty);
                                }
                                else if (pPwrSetting.Data == 1)
                                {
                                    Task.Run(() =>
                                        MonitorTurnedOn?.Invoke(this, EventArgs.Empty)
                                    );
                                }
                            }
                            //PowerSettingChanged?.Invoke(this, EventArgs.Empty);
                            handled = true;
                            break;
                        case PBT_APMPOWERSTATUSCHANGE:
                            WriteLog($"Power event {msg.ToString()} PBT_APMPOWERSTATUSCHANGE handled");
                            handled = true;
                            break;
                        case PBT_APMRESUMEAUTOMATIC:
                            WriteLog($"Power event {msg.ToString()} PBT_APMRESUMEAUTOMATIC handled");
                            Task.Run(() =>
                                SystemResume?.Invoke(this, EventArgs.Empty)
                            );
                            handled = true;
                            break;
                        case PBT_APMRESUMESUSPEND:
                            WriteLog($"Power event {msg.ToString()} PBT_APMRESUMESUSPEND handled");
                            handled = true;
                            break;
                        case PBT_APMSUSPEND:
                            WriteLog($"Power event {msg.ToString()} PBT_APMSUSPEND handled");
                            Task.Run(() =>
                                SystemSuspend?.Invoke(this, EventArgs.Empty)
                            );
                            handled = true;
                            break;
                        default:
                            WriteLog($"Power event {msg.ToString()} Unknown {wParam} handled");
                            handled = true;
                            break;
                    }
                    break;

                case WM_HOTKEY:
                    KeyPressedEventArgs keyPressedEventArgs = new KeyPressedEventArgs();
                    VirtualKey key = (VirtualKey)(((int)lParam >> 16) & 0xFFFF);
                    ModifierKeys modifier = (ModifierKeys)((int)lParam & 0xFFFF);
                    int hotkeyId = (int)wParam;
                    keyPressedEventArgs.HotkeyInfo.ID = (ushort)hotkeyId;
                    keyPressedEventArgs.KeyString = $"{modifier.ToString()} + {key.ToString()}";
                    Task.Run(() =>
                    {
                        HotkeyPressed?.Invoke(this, keyPressedEventArgs);
                    });
                    handled = true;
                    break;
                default:
                    break;
            }
            return IntPtr.Zero;
        }

        public void CloseByCaller()
        {
            Close();
            //Environment.Exit(0);
        }
    }
}
