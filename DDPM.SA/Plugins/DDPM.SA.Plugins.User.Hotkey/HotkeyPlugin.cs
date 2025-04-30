using DDPM.SA.Common;
using Dell.Client.Framework.Common;
using Dell.Client.Framework.Common.Annotations;
using Dell.Client.Framework.Common.PluginConditions;
using Dell.Client.Framework.Interfaces;
using Microsoft;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace DDPM.SA.Plugins.User.Hotkey
{
    [Plugin(Common.IDs.DDPM_HOTKEY_PLUGIN_ID, pluginName, PluginOrderGroupType.Core, Version = pluginVersion)]
    [Descriptor(Description = pluginDescription)]
    [Publisher(Name = publisherCompany, Website = publisherWebsite, Support = publisherSupport)]
    [PublishedUnelevatedInterface(new[] { typeof(IHotkey) })]
    [PluginRequires(Id = DDPM.SA.Common.IDs.Device_Manager_Plugin_ID, Version = "1.0.0", AllowDynamicResolving = true)]
    public class HotkeyPlugin : BaseAgentPlugin, IHotkey, IDisposableObservable
    {
        #region Private Members

        private const string pluginName = "HotkeyPlugin";
        private const string pluginVersion = "1.0.0";
        private const string pluginDescription = "This plugin implements Hotkey Plugin.";
        private const string publisherCompany = "Dell Technologies";
        private const string publisherWebsite = "https://www.dell.com";
        private const string publisherSupport = "This plugin implements Hotkey Plugin.";

        private IAgent _agent;
        public const string PluginLogId = "Hotkey";
        private Thread _hookThread;

        private string[] _str0to9Ary = { "D0", "D1", "D2", "D3", "D4", "D5", "D6", "D7", "D8", "D9" };
        private string[] _strNumPad0to9Ary = { "NUMPAD0", "NUMPAD1", "NUMPAD2", "NUMPAD3", "NUMPAD4", "NUMPAD5", "NUMPAD6", "NUMPAD7", "NUMPAD8", "NUMPAD9" };
        private string[] _strConverToNumPad0to9Ary = { "NUMBERPAD0", "NUMBERPAD1", "NUMBERPAD2", "NUMBERPAD3", "NUMBERPAD4", "NUMBERPAD5", "NUMBERPAD6", "NUMBERPAD7", "NUMBERPAD8", "NUMBERPAD9" };

        private enum log_type
        {
            info = 0,
            error
        }

        #endregion Private Members

        #region keyboard hook

        #region Constant, Structure and Delegate Definitions

        /// <summary>
        /// defines the callback type for the hook
        /// </summary>
        public delegate int keyboardHookProc(int code, int wParam, ref keyboardHookStruct lParam);

        public struct keyboardHookStruct
        {
            public int vkCode;
            public int scanCode;
            public int flags;
            public int time;
            public int dwExtraInfo;
        }

        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x100;
        private const int WM_KEYUP = 0x101;
        private const int WM_SYSKEYDOWN = 0x104;
        private const int WM_SYSKEYUP = 0x105;

        #endregion Constant, Structure and Delegate Definitions

        #region Instance Variables

        /// <summary>
        /// Handle to the hook, need this to unhook and call the next hook
        /// </summary>
        private IntPtr hhook { get; set; } = IntPtr.Zero;

        #endregion Instance Variables

        #region Events

        /// <summary>
        /// Occurs when one of the hooked keys is pressed
        /// </summary>
        public event KeyEventHandler KeyDown;

        /// <summary>
        /// Occurs when one of the hooked keys is released
        /// </summary>
        public event KeyEventHandler KeyUp;

        #endregion Events

        private static keyboardHookProc? callbackDelegate;
        private static readonly object hookLock = new object();

        #region Public Methods

        /// <summary>
        /// Installs the global hook
        /// </summary>
        public bool hook()
        {
            lock (hookLock)
            {
                if (callbackDelegate != null)
                {
                    Debug.WriteLine("Can't hook more than once");

                    return true;
                }

                //IntPtr hInstance = _LoadLibrary("User32");
                IntPtr hInstance = Marshal.GetHINSTANCE(System.Reflection.Assembly.GetExecutingAssembly().GetModules()[0]);

                callbackDelegate = new keyboardHookProc(hookProc);
                hhook = _SetWindowsHookEx(WH_KEYBOARD_LL, callbackDelegate, hInstance, 0);
                string errorMessage = new Win32Exception(Marshal.GetLastWin32Error()).Message;
                Debug.WriteLine($"HotkeyPlugin-hook(): {errorMessage}");
                return hhook == IntPtr.Zero ? false : true;
                /*if (hhook != IntPtr.Zero) throw new Win32Exception();
                Debug.WriteLine("Hook(); Success--------");*/
            }
        }

        /// <summary>
        /// Uninstalls the global hook
        /// </summary>
        public bool unhook()
        {
            lock (hookLock)
            {
                //UnhookWindowsHookEx(hhook);
                if (callbackDelegate == null) return true;
                bool ok = _UnhookWindowsHookEx(hhook);
                if (ok)
                {
                    callbackDelegate = null;
                    return true;
                }
                return ok;
            }
        }

        /// <summary>
        /// The callback for the keyboard hook
        /// </summary>
        /// <param name="code">The hook code, if it isn't >= 0, the function shouldn't do anyting</param>
        /// <param name="wParam">The event type</param>
        /// <param name="lParam">The keyhook event information</param>
        /// <returns></returns>
        public int hookProc(int code, int wParam, ref keyboardHookStruct lParam)
        {
            if (code >= 0)
            {
                Keys key = (Keys)lParam.vkCode;
                //if (HookedKeys.Contains(key))
                {
                    KeyEventArgs kea = new KeyEventArgs(key);
                    if ((wParam == WM_KEYDOWN || wParam == WM_SYSKEYDOWN) && (KeyDown != null))
                    {
                        KeyDown(this, kea);
                    }
                    else if ((wParam == WM_KEYUP || wParam == WM_SYSKEYUP) && (KeyUp != null))
                    {
                        KeyUp(this, kea);
                    }
                    if (kea.Handled)
                        return 1;
                }
            }
            return _CallNextHookEx(hhook, code, wParam, ref lParam);
        }

        #endregion Public Methods

        #endregion keyboard hook

        #region Constructor

        public HotkeyPlugin(IAgent agent) : base(agent, PluginLogId)
        {
            _agent = agent;
            writelog("HotkeyPlugin constructor ...");
        }

        #endregion Constructor

        #region IDisposableObservable

        /// <summary>
        /// unhook
        /// </summary>
        public bool IsDisposed { get; private set; }

        protected override void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                IsDisposed = true;
                if (disposing)
                {
                    unhook();
                    _hookThread.Interrupt();
                    _agent.PluginManager.PluginsStarted -= PluginManagerOnPluginsStarted;
                    _agent = null;
                    writelog($"[HotkeyPlugin Dispose] ===============");
                }
            }
            base.Dispose(disposing);
        }

        #endregion IDisposableObservable

        #region Private Methods

        /// <summary>
        /// //
        /// </summary>
        /// <param name="text"></param>
        /// <param name="log_type">0 means info, others means error</param>
        private void writelog(string text,
           [System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
           [System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "",
           [System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = 0,
           log_type log_type = log_type.info)
        {
            if (string.IsNullOrEmpty(text))
                text = "";

            text = $"[Hotkey] {text}, Caller Name:{memberName}, Source Line {sourceLineNumber}";
            Console.WriteLine(text);
            if (Log != null)
            {
                if (log_type == log_type.info)
                    Log.Info(text);
                else
                    Log.Error(text);
            }
        }


        private void Keyboard_KeyUpProc(object sender, KeyEventArgs e)
        {
            bool _altPressed = IsKeyPushedDown(System.Windows.Forms.Keys.Menu);
            bool _ctrlPressed = IsKeyPushedDown(System.Windows.Forms.Keys.ControlKey);
            bool _shiftPressed = IsKeyPushedDown(System.Windows.Forms.Keys.ShiftKey);

            string strKey = e.KeyCode.ToString().ToUpper();
            //AddDebugMsg(string.Format("KeyUp Event [{0}], Ctrl : {1}", , _ctrlPressed));
            int pos = Array.IndexOf(_strNumPad0to9Ary, strKey);
            if (pos > -1)
            {
                // the array contains the string and the pos variable will have its position in the array
                strKey = _strConverToNumPad0to9Ary[pos];
            }
            else if (_str0to9Ary.Contains(strKey))
                strKey = strKey.Substring(1);
            else if (strKey == "LWIN")
                strKey = "LEFTWINDOWS";
            else if (strKey == "RWIN")
                strKey = "RIGHTWINDOWS";
            else if (strKey == "APPS")
                strKey = "APPLICATION";

            writelog($"KeyUp Event, {strKey}, Alt:{_altPressed.ToString()}, Ctrl:{_ctrlPressed.ToString()}, Shift:{_shiftPressed.ToString()}");

            //var vKeyList = _RunShortcutKeysList.Where(c => "KEY_" + c.ShortcutKey.ToUpper() == strKey);
        }

        #endregion Private Methods

        #region Overriding methods

        protected override void OnPluginStarting()
        {
            _agent.PluginManager.PluginsStarted += PluginManagerOnPluginsStarted;

            PluginCondition = new PluginStartedCondition();
            writelog("Hotkey plugin started");
        }

        #endregion Overriding methods

        #region Event Handler

        private void PluginManagerOnPluginsStarted(object sender, PluginsStartedEventArgs e)
        {
            if (e == null)
                return;
            if (e.ChangedPlugins == null)
                return;
            if (e.ChangedPlugins.Any() == false)
                return;
        }

        #endregion Event Handler

        #region IHotkey implementation

        public bool Hook()
        {
            bool ok = false;
            // ThreadPool.QueueUserWorkItem
            try
            {
                _hookThread = new Thread(() =>
                {
                    ok = hook();
                    Debug.WriteLine("Hook(); Start--------");
                    // 啟動消息循環
                    System.Windows.Threading.Dispatcher.Run();
                    Debug.WriteLine("System.Windows.Threading.Dispatcher.Run(); end--------");
                    writelog("Hook()...");
                });

                // 設定為單線程單元（STA），WPF需要STA模式
                _hookThread.SetApartmentState(ApartmentState.STA);

                // 啟動執行緒
                _hookThread.Start();
            }
            catch (Exception ex)
            {
                writelog($"[HotkeyPlugin]Hook() Exception :{ex.Message}");
            }
            return ok;
        }

        public bool Unhook()
        {
            try
            {
                bool ok = unhook();
                Debug.WriteLine("Unhook()  ------exec--");
                Task.Run(() =>
                {
                    //System.Windows.Threading.Dispatcher.FromThread(_hookThread).BeginInvokeShutdown(DispatcherPriority.Send);
                    if (_hookThread != null && _hookThread.ThreadState == System.Threading.ThreadState.Running)
                        System.Windows.Threading.Dispatcher.FromThread(_hookThread).InvokeShutdown();
                    Debug.WriteLine("Unhook() --InvokeShutdown; end--------");
                });
                return ok;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Unhook();Exception: {ex.Message}");
            }
            Debug.WriteLine("Unhook(); end--------");
            writelog("hotkey UnHook()...");
            return false;
        }

        #endregion IHotkey implementation

        #region public Methods

        public bool IsKeyPushedDown(System.Windows.Forms.Keys vKey)
        {
            return 0 != (_GetAsyncKeyState(vKey) & 0x8000);
        }

        public IntPtr GetHookHandle()
        {

            return IntPtr.Zero;
        }
        #endregion public Methods

        #region DLL imports

        /// <summary>
        /// Sets the windows hook, do the desired event, one of hInstance or threadId must be non-null
        /// </summary>
        /// <param name="idHook">The id of the event you want to hook</param>
        /// <param name="callback">The callback.</param>
        /// <param name="hInstance">The handle you want to attach the event to, can be null</param>
        /// <param name="threadId">The thread you want to attach the event to, can be null</param>
        /// <returns>a handle to the desired hook</returns>
        [DllImport("user32.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        private static extern IntPtr SetWindowsHookEx(int idHook, keyboardHookProc callback, IntPtr hInstance, uint threadId);

        public static IntPtr _SetWindowsHookEx(int idHook, keyboardHookProc callback, IntPtr hInstance, uint threadId)
        {
            IntPtr rst = SetWindowsHookEx(idHook, callback, hInstance, threadId);

            if (rst == IntPtr.Zero)
            {
#if DEBUG
                Console.WriteLine("[HotkeyPlugin] SetWindowsHookEx failed.");
#endif
            }

            return rst;
        }

        /// <summary>
        /// Unhooks the windows hook.
        /// </summary>
        /// <param name="hInstance">The hook handle that was returned from SetWindowsHookEx</param>
        /// <returns>True if successful, false otherwise</returns>
        [DllImport("user32.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        private static extern bool UnhookWindowsHookEx(IntPtr hInstance);

        public static bool _UnhookWindowsHookEx(IntPtr hInstance)
        {
            bool rst = UnhookWindowsHookEx(hInstance);

            if (!rst)
            {
#if DEBUG
                Console.WriteLine("[HotkeyPlugin] UnhookWindowsHookEx failed.");
#endif
            }

            return rst;
        }

        /// <summary>
        /// Calls the next hook.
        /// </summary>
        /// <param name="idHook">The hook id</param>
        /// <param name="nCode">The hook code</param>
        /// <param name="wParam">The wparam.</param>
        /// <param name="lParam">The lparam.</param>
        /// <returns></returns>
        [DllImport("user32.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        private static extern int CallNextHookEx(IntPtr idHook, int nCode, int wParam, ref keyboardHookStruct lParam);

        public static int _CallNextHookEx(IntPtr idHook, int nCode, int wParam, ref keyboardHookStruct lParam)
        {
            return CallNextHookEx(idHook, nCode, wParam, ref lParam);
        }

        [DllImport("user32.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        private static extern short GetAsyncKeyState(System.Windows.Forms.Keys vKey);

        public static short _GetAsyncKeyState(System.Windows.Forms.Keys vKey)
        {
            return GetAsyncKeyState(vKey);
        }

        #endregion DLL imports
    }
}