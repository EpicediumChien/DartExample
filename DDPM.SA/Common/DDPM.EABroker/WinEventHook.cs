using DDPM.Win32Lib;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;

//Forms: Cursors

namespace nsWinEventHook
{
    public class WinEventHook
    {
        #region Hook, Unhook

        //Hook Handle, used to detect if hooked, and for Unhook()
        //
        private IntPtr hHook = IntPtr.Zero;

        public bool IsHooked
        { get { return (hHook != IntPtr.Zero); } }

        //Hook
        //
        public bool Hook()
        {
            uint myMinEvent = EVENT_SYSTEM_FOREGROUND;
            uint myMaxEvent = EVENT_OBJECT_LOCATIONCHANGE;

            evtDelegate = new WinEventDelegate(WinEventProc);
            hHook = _SetWinEventHook(myMinEvent, myMaxEvent, IntPtr.Zero, evtDelegate, 0, 0, WINEVENT_OUTOFCONTEXT);
            return IsHooked;
        }

        //Unhook
        //
        public bool Unhook()
        {
            _UnhookWinEvent(hHook);
            hHook = IntPtr.Zero;
            return true;
        }

        #endregion Hook, Unhook

        #region Runtime propeties

        //Will be refreshed before callback to OnStartMoving(),
        //True=Case of Window moving, False=Case of Window resizing
        //public bool IsWindowMoving { get; private set; } = true;
        public IntPtr hWnd_Foregrgound { get; set; } = IntPtr.Zero;

        #endregion Runtime propeties

        #region Class Callbacks

        public delegate void OnForegroundWindowChangedDelegate(IntPtr hWndNew, IntPtr hWndOld);

        public OnForegroundWindowChangedDelegate? OnForegroundWindowChanged = null;

        public delegate void OnStartMovingDelegate(IntPtr hWnd);

        public OnStartMovingDelegate? OnStartMoving = null;

        public delegate void OnEndMovingDelegate(IntPtr hWnd, bool isCanceled = false);

        public OnEndMovingDelegate? OnEndMoving = null;

        public delegate void OnLocationChangedDelegate(int x, int y);

        public OnLocationChangedDelegate? OnLocationChanged = null;

        #endregion Class Callbacks

        #region WinEvent Handlers

        //WinEventDelegate, used to handle the callbacks from System
        //
        private delegate void WinEventDelegate(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject,
                                        int idChild, uint dwEventThread, uint dwmsEventTime);

        private WinEventDelegate? evtDelegate = null;

        //Internal Callback Handler
        //
        public void WinEventProc(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject,
                                int idChild, uint dwEventThread, uint dwmsEventTime)
        {
            //Foreground Window has changed
            if (eventType == EVENT_SYSTEM_FOREGROUND)
            {
                IntPtr hWndOld = hWnd_Foregrgound;
                hWnd_Foregrgound = hwnd;

                if (OnForegroundWindowChanged != null)
                    OnForegroundWindowChanged(hWnd_Foregrgound, hWndOld);
                return;
            }

            //Start moving | resizing window
            if (eventType == EVENT_SYSTEM_MOVESIZESTART)
            {
                //If the cursor type is resizing, not Moveing behavior
                if (IsResizeCursor())
                    return;
                if (OnStartMoving != null)
                    OnStartMoving(hwnd);
                return;
            }

            //Stop moving | resizing window
            else if (eventType == EVENT_SYSTEM_MOVESIZEEND)
            {
                if (OnEndMoving != null)
                    OnEndMoving(hwnd, IsUserCancelMoving());
                return;
            }

            //Object location changed
            if (eventType == EVENT_OBJECT_LOCATIONCHANGE)
            {
                //Get current cursor position
                POINT ptCur;
                if (!_GetCursorPos(out ptCur))
                    return;

                if (OnLocationChanged != null)
                    OnLocationChanged(ptCur.X, ptCur.Y);

                return;
            }
        }

        #endregion WinEvent Handlers

        #region Detect if user cancel the window moving by pressing [Esc] key

        private const short VK_ESCAPE = 0x1b;
        private const short VK_LBUTTON = 0x01;
        private const short VK_SHIFT = 0x10;

        //Check if user cancel the window moving by pressing [Esc] key
        //Assumption:
        // When user moving window, the mouse [LeftButton] is pressed and hold.
        // When user canceling the moving, he/she press [Esc] key and the
        //     mouse [LeftButton] is strll pressed and hold.
        //
        public static bool IsUserCancelMoving()
        {
            short sEsc = _GetAsyncKeyState(VK_ESCAPE);
            short sLbtn = _GetAsyncKeyState(VK_LBUTTON);

            //Check the hightest bit: 1=Down; 0=Up
            bool isEscDown = ((sEsc & 0x8000) == 0x8000);
            bool isLBtnDown = ((sLbtn & 0x8000) == 0x8000);

            //If [Esc] is down  and [LBtn} is down => User cancel the moving
            return (isEscDown && isLBtnDown);
        }

        #endregion Detect if user cancel the window moving by pressing [Esc] key

        #region Detect [Shift] pressed
        public static bool IsShiftPressed()
        {
            short sShift = _GetAsyncKeyState(VK_SHIFT);
            //Check the highest bit: 1=Down; 0=Up
            bool isShiftDown = ((sShift & 0x8000) == 0x8000);
            return isShiftDown;
        }
        #endregion

        #region GetProcessFromWindowHandle

        //Description: Get the Process from WindowHandle
        //Return:
        //  True: succsseed. output the Process to p. msg will be "OK"
        //  False: failed. output the error message to msg. p will be null.
        //Remark:
        //  It may need RunAsAdmin if the hWnd owner is running as Admin.
        public static bool GetProcessFromWindowHandle(IntPtr hWnd, out Process? p, out string msg)
        {
            if (hWnd == IntPtr.Zero)
            {
                p = null;
                msg = "ERR, Window handle is null";
                return false;
            }

            //Get ProcessId from window handle
            uint processId = 0;
            uint threadId = _GetWindowThreadProcessId(hWnd, out processId);

            //Get Process from ProcessId
            p = Process.GetProcessById((int)processId);

            if (p == null)
            {
                msg = $"ERR, GetProcessById(), ProcessId={processId}";
                return false;
            }
            msg = "OK";
            return true;
        }

        //Robert_Lin, to fix set window position can not fit to smaller height issue.
        //Below function is referenced from Internet, but it's not help for the issue we met.
        //Comment-out
        //public static void FixWindowPosition(IntPtr hWnd, Rect rect)
        //{
        //    Win32.RECT wrect;
        //    bool isOK = Win32._GetWindowRect(hWnd, out wrect);

        //    Win32.RECT xrect;
        //    int hRes = DwmGetWindowAttribute(hWnd, DWMWINDOWATTRIBUTE.ExtendedFrameBounds, out xrect, Marshal.SizeOf(typeof(Win32.RECT)));

        //    Win32.POINT wtl = new Win32.POINT((int)wrect.Left, (int)wrect.Top);
        //    Win32.POINT wbr = new Win32.POINT((int)wrect.Right, (int)wrect.Bottom);

        //    Win32.POINT xtl = new Win32.POINT((int)xrect.Left, (int)xrect.Top);
        //    Win32.POINT xbr = new Win32.POINT((int)xrect.Right, (int)xrect.Bottom);

        //    PhysicalToLogicalPointForPerMonitorDPI(hWnd, ref xtl);
        //    PhysicalToLogicalPointForPerMonitorDPI(hWnd, ref xbr);

        //    RECT rcNew = new RECT((int)rect.Left, (int)rect.Top, (int)rect.Width, (int)rect.Height);

        //    int dLeft = xtl.X - wtl.X;
        //    int dTop = xtl.Y - wtl.Y;
        //    int dRight = xbr.X - wbr.X;
        //    int dBottom = xbr.Y - wbr.Y;

        //    int newLeft = (int)rect.Left - dLeft;
        //    int newTop = (int)rect.Top - dTop;
        //    int newRight = (int)rect.Right - dRight;
        //    int newBottom = (int)rect.Bottom - dBottom;

        //    rcNew.Left -= dLeft;
        //    rcNew.Right -= dRight;
        //    rcNew.Bottom -= dBottom;

        //    _MoveWindow(hWnd, (int)rcNew.Left, (int)rcNew.Top,
        //        (int)rcNew.Width, (int)rcNew.Height, true);


        //    Win32.RECT adjusted_rect = new Win32.RECT(
        //       (int)rect.Left - (xtl.X - wtl.X),
        //       (int)rect.Top - (xtl.Y - wtl.Y),
        //       (int)rect.Width + (xtl.X - wtl.X) + (wbr.X - xbr.X),
        //       (int)rect.Height + (xtl.Y - wtl.Y) + (wbr.Y - xbr.Y));


        //    //_SetWindowPos(hWnd, 0, (int)adjusted_rect.Left, (int)adjusted_rect.Top,
        //    //    (int)adjusted_rect.Width, (int)adjusted_rect.Height, SWP_NOZORDER | SWP_SHOWWINDOW | SWP_FRAMECHANGED);

        //    //_SetWindowPos(hWnd, HWND_TOP, (int)adjusted_rect.Left, (int)adjusted_rect.Top,
        //    //    (int)adjusted_rect.Width, (int)adjusted_rect.Height, SWP_SHOWWINDOW);

        //    _MoveWindow(hWnd, (int)adjusted_rect.Left, (int)adjusted_rect.Top,
        //        (int)adjusted_rect.Width, (int)adjusted_rect.Height, true);
        //}

        public static void SetWindowPosition(IntPtr hWnd, Rect rect)
        {
            //const int HWND_NOTOPMOST = -2;
            const int HWND_TOP = 0;
            //const int HWND_TOPMOST = -1;
            const int SWP_FRAMECHANGED = 0x0020;

            bool isNoSize = (rect.Width ==0 || rect.Height == 0);

            if (isNoSize)
            {
                _SetWindowPos(hWnd, 0, (int)rect.Left, (int)rect.Top,
                    (int)rect.Width, (int)rect.Height, SWP_NOZORDER | SWP_SHOWWINDOW | SWP_FRAMECHANGED | SWP_NOSIZE);

                _SetWindowPos(hWnd, HWND_TOP, (int)rect.Left, (int)rect.Top,
                    (int)rect.Width, (int)rect.Height, SWP_SHOWWINDOW | SWP_NOSIZE);

                //_MoveWindow(hWnd, (int)rect.Left, (int)rect.Top,
                //    (int)rect.Width, (int)rect.Height, true);
            }
            else
            {
                /*
                Win32.RECT wrect;
                Win32._GetWindowRect(hWnd, out wrect);

                Win32.RECT xrect = new Win32.RECT();
               DwmGetWindowAttribute(hWnd, DWMWINDOWATTRIBUTE.ExtendedFrameBounds, out xrect, Marshal.SizeOf(typeof(Win32.RECT)));

                Win32.POINT wtl = new Win32.POINT((int)wrect.Left, (int)wrect.Top);
                Win32.POINT wbr = new Win32.POINT((int)wrect.Right, (int)wrect.Bottom);

                Win32.POINT xtl = new Win32.POINT((int)xrect.Left, (int)xrect.Top);
                Win32.POINT xbr = new Win32.POINT((int)xrect.Right, (int)xrect.Bottom);

                PhysicalToLogicalPointForPerMonitorDPI(hWnd, ref xtl);
                PhysicalToLogicalPointForPerMonitorDPI(hWnd, ref xbr);

                Win32.RECT adjusted_rect = new Win32.RECT(
                   (int)rect.Left - (xtl.X - wtl.X),
                   (int)rect.Top - (xtl.Y - wtl.Y),
                   (int)rect.Width + (xtl.X - wtl.X) + (wbr.X - xbr.X),
                   (int)rect.Height + (xtl.Y - wtl.Y) + (wbr.Y - xbr.Y));


                _SetWindowPos(hWnd, 0, (int)adjusted_rect.Left, (int)adjusted_rect.Top,
                    (int)adjusted_rect.Width, (int)adjusted_rect.Height, SWP_NOZORDER | SWP_SHOWWINDOW | SWP_FRAMECHANGED);

                _SetWindowPos(hWnd, HWND_TOP, (int)adjusted_rect.Left, (int)adjusted_rect.Top,
                    (int)adjusted_rect.Width, (int)adjusted_rect.Height, SWP_SHOWWINDOW);

                _MoveWindow(hWnd, (int)adjusted_rect.Left, (int)adjusted_rect.Top,
                    (int)adjusted_rect.Width, (int)adjusted_rect.Height, true);
                */

                Win32.WINDOWPLACEMENT wpl= new Win32.WINDOWPLACEMENT();
                wpl.Length = Marshal.SizeOf(wpl);

                Win32._GetWindowPlacement(hWnd, ref wpl);

                _SetWindowPos(hWnd, 0, (int)rect.Left, (int)rect.Top,
                    (int)rect.Width, (int)rect.Height, SWP_NOZORDER | SWP_SHOWWINDOW | SWP_FRAMECHANGED);

                _SetWindowPos(hWnd, HWND_TOP, (int)rect.Left, (int)rect.Top,
                    (int)rect.Width, (int)rect.Height, SWP_SHOWWINDOW);

                _MoveWindow(hWnd, (int)rect.Left, (int)rect.Top,
                    (int)rect.Width, (int)rect.Height, true);


                _SetWindowPos(hWnd, HWND_TOP, (int)rect.Left, (int)rect.Top,
                    (int)rect.Width, (int)rect.Height, SWP_SHOWWINDOW | SWP_NOSIZE);

                //Robert_Lin, 2024-12-10 The final solution to fix the issue cannot set window position with smaller height
                _SetWindowPos(hWnd, HWND_TOP, (int)rect.Left, (int)rect.Top,
                    (int)rect.Width, (int)rect.Height, SWP_SHOWWINDOW|SWP_NOMOVE);

                //Win32._GetWindowPlacement(hWnd, ref wpl);

                //wpl.NormalPosition.Left = (int)rect.Left;
                //wpl.NormalPosition.Top = (int)rect.Top;
                //wpl.NormalPosition.Width = (int)rect.Width;
                //wpl.NormalPosition.Height = (int)rect.Height;

                //wpl.ShowCmd = ShowWindowCommands.Restore;
                //wpl.Flags |= 4;
                //Win32._SetWindowPlacement(hWnd, ref wpl);


                //Win32._GetWindowPlacement(hWnd, ref wpl);

                //Thread.Sleep(50);
                //wpl.NormalPosition.Left = (int)rect.Left;
                //wpl.NormalPosition.Top = (int)rect.Top;
                //wpl.NormalPosition.Width = (int)rect.Width;
                //wpl.NormalPosition.Height = (int)rect.Height;

                //wpl.ShowCmd = ShowWindowCommands.Restore;
                //wpl.Flags |= 4;

                //Win32._SetWindowPlacement(hWnd, ref wpl);
                //  FixWindowPosition(hWnd, rect);
            }

            /*
            Rectangle rcWnd = new Rectangle();
            GetWindowRect(hWnd, ref rcWnd);

            Rectangle rcX = new Rectangle();
            DwmGetWindowAttribute(hWnd, 9, ref rcX, Marshal.SizeOf(typeof(Rectangle)));

            System.Drawing.Point wtl = new System.Drawing.Point(rcWnd.Left, rcWnd.Top);
            System.Drawing.Point wbr = new System.Drawing.Point(rcWnd.Right, rcWnd.Bottom);

            System.Drawing.Point xtl = new System.Drawing.Point(rcX.Left, rcX.Top);
            System.Drawing.Point xbr = new System.Drawing.Point(rcX.Right, rcX.Bottom);

            PhysicalToLogicalPointForPerMonitorDPI(hWnd, ref xtl);
            PhysicalToLogicalPointForPerMonitorDPI(hWnd, ref xbr);

            Rectangle rcAdiust = new Rectangle(
                (int)rect.X - (xtl.X - wtl.X), (int)rect.Y - (xtl.Y - wtl.Y),
                (int)rect.Width + (xtl.X - wtl.X) + (wbr.X - xbr.X),
                (int)rect.Height + (xtl.Y - wtl.Y) + (wbr.Y - wbr.Y));

            MoveWindow(hWnd, rcAdiust.X, rcAdiust.Y, rcAdiust.Width, rcAdiust.Height, true);
            */
        }
                
        #endregion GetProcessFromWindowHandle

        #region Win32 Constants

        //Derek 2025/04/02
        //private const uint EVENT_MIN = 0x00000001;
        //private const uint EVENT_MAX = 0x7FFFFFFF;

        private const uint EVENT_SYSTEM_FOREGROUND = 0x0003;

        private const uint EVENT_SYSTEM_MOVESIZESTART = 0x000A;
        private const uint EVENT_SYSTEM_MOVESIZEEND = 0x000B;

        //Derek 2025/04/02
        //private const uint EVENT_SYSTEM_MINIMIZESTART = 0x0016;
        //private const uint EVENT_SYSTEM_MINIMIZEEND = 0x0017;

        private const uint EVENT_OBJECT_LOCATIONCHANGE = 0x800B;

        // Object IDs
        //
        //private const uint OBJID_WINDOW = 0x00000000; //Derek 2025/04/02

        //private const uint OBJID_CURSOR = 0xFFFFFFF7; //Derek 2025/04/02

        //dwFlags
        private const uint WINEVENT_OUTOFCONTEXT = 0;

        #endregion Win32 Constants

        #region Win32 P-Invoke

        //GetAsyncKeyState
        [DllImport("User32.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        private static extern short GetAsyncKeyState(System.Int32 vKey);

        private static short _GetAsyncKeyState(System.Int32 vKey)
        {
            return GetAsyncKeyState(vKey);
        }

        //SetWinEventHook()
        [DllImport("user32.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        private static extern IntPtr SetWinEventHook(uint eventMin, uint eventMax, IntPtr hmodWinEventProc,
                                            WinEventDelegate lpfnWinEventProc, uint idProcess,
                                            uint idThread, uint dwFlags);

        private static IntPtr _SetWinEventHook(uint eventMin, uint eventMax, IntPtr hmodWinEventProc,
                                            WinEventDelegate lpfnWinEventProc, uint idProcess,
                                            uint idThread, uint dwFlags)
        {
            IntPtr rst = SetWinEventHook(eventMin, eventMax, hmodWinEventProc,
                                            lpfnWinEventProc, idProcess,
                                            idThread, dwFlags);
            if (rst == IntPtr.Zero)
            {
#if DEBUG
                Console.WriteLine("[WinEventHook] SetWinEventHook failed");
#endif
            }
            return rst;
        }

        //UnhookWinEvent()
        [DllImport("user32.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        private static extern bool UnhookWinEvent(IntPtr hWinEventHook);

        private static bool _UnhookWinEvent(IntPtr hWinEventHook)
        {
            bool rst = UnhookWinEvent(hWinEventHook);
            if (!rst)
            {
#if DEBUG
                Console.WriteLine("[WinEventHook] UnhookWinEvent failed");
#endif
            }
            return rst;
        }

        //GetWindowThreadProcessId()
        [DllImport("user32.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);

        public static uint _GetWindowThreadProcessId(IntPtr hWnd, out uint processId)
        {
            uint rst = GetWindowThreadProcessId(hWnd, out processId);
            if (rst == 0)
            {
#if DEBUG
                Console.WriteLine("[WinEventHook] GetWindowThreadProcessId failed");
#endif
            }

            return rst;
        }

        public const short SWP_NOMOVE = 0X2;
        public const short SWP_NOSIZE = 1;
        public const short SWP_NOZORDER = 0X4;
        public const int SWP_SHOWWINDOW = 0x0040;

        [DllImport("user32.dll", EntryPoint = "SetWindowPos", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        private static extern IntPtr SetWindowPos(IntPtr hWnd, int hWndInsertAfter, int x, int Y, int cx, int cy, int wFlags);

        public static IntPtr _SetWindowPos(IntPtr hWnd, int hWndInsertAfter, int x, int Y, int cx, int cy, int wFlags)
        {
            IntPtr rst = SetWindowPos(hWnd, hWndInsertAfter, x, Y, cx, cy, wFlags);
            if (rst == IntPtr.Zero)
            {
#if DEBUG
                Console.WriteLine("[WinEventHook] SetWindowPos failed");
#endif
            }

            return rst;
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        private static extern int MoveWindow(IntPtr hWnd, int x, int y, int nWidth, int nHeight, bool bRepaint);

        public static int _MoveWindow(IntPtr hWnd, int x, int y, int nWidth, int nHeight, bool bRepaint)
        {
            int rst = MoveWindow(hWnd, x, y, nWidth, nHeight, bRepaint);

            if (rst == 0)
            {
#if DEBUG
                Console.WriteLine("[WinEventHook] MoveWindow failed");
#endif
            }
            return rst;
        }

        #endregion Win32 P-Invoke

        #region Win32 - GetCursorType

        //private static bool IsWaitCursor()
        //{
        //    //AddReference: Window.Forms
        //    var h = Cursors.WaitCursor.Handle;

        //    CURSORINFO pci;
        //    pci.cbSize = Marshal.SizeOf(typeof(CURSORINFO));
        //    GetCursorInfo(out pci);

        //    return pci.hCursor == h;
        //}

        private static bool IsResizeCursor()
        {
            CURSORINFO pci;
            pci.cbSize = Marshal.SizeOf(typeof(CURSORINFO));
            _GetCursorInfo(out pci);

            if (pci.hCursor == Cursors.SizeNESW.Handle)// "/"
                return true;
            if (pci.hCursor == Cursors.SizeNS.Handle)  // "|"
                return true;
            if (pci.hCursor == Cursors.SizeNWSE.Handle) // "\"
                return true;
            if (pci.hCursor == Cursors.SizeWE.Handle)   // "-"
                return true;

            return false;
        }

        //struct POINT
        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int X;
            public int Y;

            public POINT(int x, int y)
            {
                this.X = x;
                this.Y = y;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct CURSORINFO
        {
            public Int32 cbSize;        // Specifies the size, in bytes, of the structure.

            // The caller must set this to Marshal.SizeOf(typeof(CURSORINFO)).
            public Int32 flags;         // Specifies the cursor state. This parameter can be one of the following values:

            //    0             The cursor is hidden.
            //    CURSOR_SHOWING    The cursor is showing.
            public IntPtr hCursor;          // Handle to the cursor.

            public POINT ptScreenPos;       // A POINT structure that receives the screen coordinates of the cursor.
        }

        [DllImport("user32.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        private static extern bool GetCursorInfo(out CURSORINFO pci);

        private static bool _GetCursorInfo(out CURSORINFO pci)
        {
            bool rst = GetCursorInfo(out pci);

            if (!rst)
            {
#if DEBUG
                Console.WriteLine("[WinEventHook] GetCursorInfo failed");
#endif
            }
            return rst;
        }

        //GetCursorPos()
        [DllImport("user32.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetCursorPos(out POINT lpPoint);

        public static bool _GetCursorPos(out POINT lpPoint)
        {
            bool rst = GetCursorPos(out lpPoint);

            if (!rst)
            {
#if DEBUG
                Console.WriteLine("[WinEventHook] GetCursorPos failed");
#endif
            }
            return rst;
        }

        #endregion Win32 - GetCursorType
    }
}