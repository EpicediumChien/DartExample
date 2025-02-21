using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;

namespace DDPM.ColorApp
{
    public class WindowFocusWatcher : IDisposable
    {
        #region Fields

        private readonly Native.WinEventDelegate _delegate;
        private readonly WindowFocusWatcherEvent _event;
        private readonly IntPtr _hook;

        #endregion Fields

        #region Constructors

        public WindowFocusWatcher(WindowFocusWatcherEvent e, uint HookEvent)
        {
            _event = e;
            _delegate = WinEventProc;
            _hook = Native._SetWinEventHook(HookEvent, HookEvent,/*Native.EVENT_OBJECT_FOCUS, Native.EVENT_OBJECT_FOCUS,*/
                IntPtr.Zero,
                _delegate, 0, 0, Native.WINEVENT_OUTOFCONTEXT | Native.WINEVENT_SKIPOWNPROCESS);
        }

        ~WindowFocusWatcher()
        {
            Dispose(false);
        }

        #endregion Constructors

        #region Delegates

        public delegate void WindowFocusWatcherEvent(IntPtr hwnd/*uint processId*/);

        #endregion Delegates

        #region Methods

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void Dispose(bool disposing)
        {
            if (disposing)
            {
                Native._UnhookWinEvent(_hook);
            }
        }

        private void WinEventProc(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild,
            uint dwEventThread, uint dwmsEventTime)
        {
            _event(hwnd);
        }

        #endregion Methods

        #region Nested Types

        public static class Native
        {
            #region Fields

            public const uint EVENT_OBJECT_FOCUS = 0x8005;
            public const uint EVENT_OBJECT_SELECTION = 0x8006;

            public const int EVENT_OBJECT_LOCATIONCHANGE = 0x800B;
            public const int EVENT_OBJECT_NAMECHANGE = 0x800C;
            public const int EVENT_OBJECT_VALUECHANGE = 0x800E;

            public const int EVENT_SYSTEM_MOVESIZEEND = 0x000B;

            public const uint WINEVENT_OUTOFCONTEXT = 0;
            public const int WINEVENT_SKIPOWNPROCESS = 2;

            #endregion Fields

            #region Delegates

            public delegate void WinEventDelegate(
                IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread,
                uint dwmsEventTime);

            #endregion Delegates

            #region Methods

            [DllImport("user32.dll", SetLastError = true)]
            [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
            private static extern IntPtr GetForegroundWindow();

            public static IntPtr _GetForegroundWindow()
            {
                return GetForegroundWindow();
            }

            [DllImport("user32.dll", SetLastError = true)]
            [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
            private static extern IntPtr GetWindowThreadProcessId(IntPtr hWnd, out uint nProcessId);

            public static IntPtr _GetWindowThreadProcessId(IntPtr hWnd, out uint nProcessId)
            {
                IntPtr rst = GetWindowThreadProcessId(hWnd, out nProcessId);
                if (rst == IntPtr.Zero)
                {
#if DEBUG
                    Console.WriteLine("[WindowFocusWatcher] GetWindowThreadProcessId failed");
#endif
                }
                return rst;
            }

            [DllImport("user32.dll", SetLastError = true)]
            [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
            private static extern IntPtr SetWinEventHook(uint eventMin, uint eventMax, IntPtr hmodWinEventProc,
                WinEventDelegate lpfnWinEventProc, uint idProcess, uint idThread, uint dwFlags);

            public static IntPtr _SetWinEventHook(uint eventMin, uint eventMax, IntPtr hmodWinEventProc,
                WinEventDelegate lpfnWinEventProc, uint idProcess, uint idThread, uint dwFlags)
            {
                IntPtr rst = SetWinEventHook(eventMin, eventMax, hmodWinEventProc, lpfnWinEventProc, idProcess, idThread, dwFlags);
                if (rst == IntPtr.Zero)
                {
#if DEBUG
                    Console.WriteLine("[WindowFocusWatcher] SetWinEventHook failed");
#endif
                }
                return rst;
            }

            [DllImport("user32.dll", SetLastError = true)]
            [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
            private static extern bool UnhookWinEvent(IntPtr hWinEventHook);

            public static bool _UnhookWinEvent(IntPtr hWinEventHook)
            {
                bool rst = UnhookWinEvent(hWinEventHook);
                if (!rst)
                {
#if DEBUG
                    Console.WriteLine("[WindowFocusWatcher] UnhookWinEvent failed");
#endif
                }
                return rst;
            }

            #endregion Methods
        }

        #endregion Nested Types
    }
}