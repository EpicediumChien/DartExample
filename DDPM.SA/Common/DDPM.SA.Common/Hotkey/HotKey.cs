using Dell.Client.Framework.Common;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Threading;
using Windows.System;

namespace DDPM.SA.Common
{
    public sealed class HotKey : IDisposable
    {
        private ILog Log;
        private enum log_type
        {
            info = 0,
            error
        }


        private readonly IntPtr _handle;

        private readonly int _id;

        public bool isKeyRegistered;

        private Dispatcher _currentDispatcher;

        //[DllImport("user32.dll")]
        //static extern IntPtr GetForegroundWindow();

        public HotKey(ModifierKeys modifierKeys, VirtualKey key, Window window)
            : this(modifierKeys, key, new WindowInteropHelper(window), null)
        {
        }

        public HotKey(ModifierKeys modifierKeys, VirtualKey key, WindowInteropHelper window)
            : this(modifierKeys, key, window.Handle, null)
        {
        }

        public HotKey(ModifierKeys modifierKeys, VirtualKey key, Window window, Action<HotKey> onKeyAction)
            : this(modifierKeys, key, new WindowInteropHelper(window), onKeyAction)
        {
        }

        public HotKey(ModifierKeys modifierKeys, VirtualKey key, WindowInteropHelper window, Action<HotKey> onKeyAction)
            : this(modifierKeys, key, window.Handle, onKeyAction)
        {
        }

        public HotKey(ModifierKeys modifierKeys, VirtualKey key, IntPtr windowHandle, Action<HotKey> onKeyAction = null)
        {
            Key = key;
            KeyModifier = modifierKeys;
            _id = GetHashCode();
            //_handle = windowHandle == IntPtr.Zero ? GetForegroundWindow() : windowHandle;
            //_handle = windowHandle == IntPtr.Zero ? HotKeyWinApi.GetModuleHandle("DDPM.Subagent.User.exe") : windowHandle;
            //_handle = IntPtr.Zero;
            _handle = windowHandle;
            _currentDispatcher = Dispatcher.CurrentDispatcher;
            RegisterHotKey();
            ComponentDispatcher.ThreadPreprocessMessage += ThreadPreprocessMessageMethod;

            if (onKeyAction != null)
                HotKeyPressed += onKeyAction;
        }


        ~HotKey()
        {
            Dispose();
        }

        public event Action<HotKey> HotKeyPressed;

        public VirtualKey Key { get; private set; }

        public ModifierKeys KeyModifier { get; private set; }

        private int InteropKey => (int)Key;

        public void Dispose()
        {
            try
            {
                ComponentDispatcher.ThreadPreprocessMessage -= ThreadPreprocessMessageMethod;
            }
            catch (Exception)
            {
                // ignored
            }
            finally
            {
                UnregisterHotKey();
            }
        }

        private void OnHotKeyPressed()
        {
            _currentDispatcher.Invoke(
                delegate
                {
                    HotKeyPressed?.Invoke(this);
                });
        }

        private void RegisterHotKey()
        {

            if (Key == VirtualKey.None)
            {
                return;
            }

            if (isKeyRegistered)
            {
                UnregisterHotKey();
            }

            isKeyRegistered = HotKeyWinApi._RegisterHotKey(_handle, _id, KeyModifier, InteropKey);
            int lastError = Marshal.GetLastWin32Error();
            //ulong v = HotKeyWinApi.GetLastError();
            Debug.WriteLine($"HotKeyWinApi.GetLastError v1={lastError}");
            if (!isKeyRegistered)
            {
                switch (lastError.ToString())
                {
                    case "1409":
                        writelog($"_id{_id},Hot key is already registered.");
                        break;
                    case "1408":
                        writelog($"_id{_id},Invalid window; it belongs to other thread.");
                        break;
                    case "1400":
                        writelog($"_id{_id},Invalid window handle.");
                        break;
                }

            }
            else
            {
                writelog($"_id{_id},The operation RegisterHotKey successfully.");
            }
        }

        private void ThreadPreprocessMessageMethod(ref MSG msg, ref bool handled)
        {
            if (handled)
            {
                return;
            }

            if (msg.message != HotKeyWinApi.WmHotKey || (int)(msg.wParam) != _id)
            {
                return;
            }

            OnHotKeyPressed();
            handled = true;
        }

        private void UnregisterHotKey()
        {
            isKeyRegistered = !HotKeyWinApi._UnregisterHotKey(_handle, _id);
        }

        private void writelog(string text, log_type log_type = log_type.info)
        {
            if (string.IsNullOrEmpty(text))
                text = "";

            text = "[HotkeyPlugin] " + text;
            Console.WriteLine(text);
            if (Log != null)
            {
                if (log_type == log_type.info)
                    Log.Info(text);
                else
                    Log.Error(text);
            }
        }
    }
}
