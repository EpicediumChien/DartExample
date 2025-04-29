using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using static DDPM.Win32Lib.Win32;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace DDPM.Win32Lib
{
    public class Win32
    {
        #region Window General
        public static void HideWinFromAltTab(IntPtr hWnd)
        {
            int exStyle = (int)Win32Lib.Win32._GetWindowLong(hWnd, (int)Win32Lib.Win32.WindowLongFlags.GWL_EXSTYLE);

            exStyle |= (int)Win32Lib.Win32.WindowStylesEx.WS_EX_TOOLWINDOW;
            Win32Lib.Win32.SetWindowLong(hWnd, (int)Win32Lib.Win32.WindowLongFlags.GWL_EXSTYLE, (IntPtr)exStyle);
        }


        public static int IntPtrToInt32(IntPtr intPtr)
        {
            return unchecked((int)intPtr.ToInt64());
        }

        [DllImport("kernel32.dll", EntryPoint = "SetLastError")]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        private static extern void SetLastError(int dwErrorCode);

        public static void _SetLastError(int dwErrorCode)
        {
            SetLastError(dwErrorCode);
        }
        #endregion Window General

        #region WindowLong
        [DllImport("user32.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        private static extern IntPtr GetWindowLong(IntPtr hWnd, int nIndex);

        public static IntPtr _GetWindowLong(IntPtr hWnd, int nIndex)
        {
            return GetWindowLong(hWnd, nIndex);
        }

        public static IntPtr SetWindowLong(IntPtr hWnd, int nIndex, IntPtr dwNewLong)
        {
            int error = 0;
            IntPtr result = IntPtr.Zero;
            // Win32 SetWindowLong doesn't clear error on success
            _SetLastError(0);

            if (IntPtr.Size == 4)
            {
                // use SetWindowLong
                Int32 tempResult = _IntSetWindowLong(hWnd, nIndex, IntPtrToInt32(dwNewLong));
                error = Marshal.GetLastWin32Error();
                result = new IntPtr(tempResult);
            }
            else
            {
                // use SetWindowLongPtr
                result = _IntSetWindowLongPtr(hWnd, nIndex, dwNewLong);
                error = Marshal.GetLastWin32Error();
            }

            if ((result == IntPtr.Zero) && (error != 0))
            {
                throw new System.ComponentModel.Win32Exception(error);
            }

            return result;
        }

        [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        private static extern IntPtr IntSetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

        public static IntPtr _IntSetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong)
        {
            return IntSetWindowLongPtr(hWnd, nIndex, dwNewLong);
        }

        [DllImport("user32.dll", EntryPoint = "SetWindowLong", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        private static extern Int32 IntSetWindowLong(IntPtr hWnd, int nIndex, Int32 dwNewLong);

        public static Int32 _IntSetWindowLong(IntPtr hWnd, int nIndex, Int32 dwNewLong)
        {
            return IntSetWindowLong(hWnd, nIndex, dwNewLong);
        }
        #endregion WindowLong

        #region Window Styles
        [Flags]
        public enum WindowStylesEx : uint
        {
            /// <summary>Specifies a window that accepts drag-drop files.</summary>
            WS_EX_ACCEPTFILES = 0x00000010,

            /// <summary>Forces a top-level window onto the taskbar when the window is visible.</summary>
            WS_EX_APPWINDOW = 0x00040000,

            /// <summary>Specifies a window that has a border with a sunken edge.</summary>
            WS_EX_CLIENTEDGE = 0x00000200,

            /// <summary>
            /// Specifies a window that paints all descendants in bottom-to-top painting order using double-buffering.
            /// This cannot be used if the window has a class style of either CS_OWNDC or CS_CLASSDC. This style is not supported in Windows 2000.
            /// </summary>
            /// <remarks>
            /// With WS_EX_COMPOSITED set, all descendants of a window get bottom-to-top painting order using double-buffering.
            /// Bottom-to-top painting order allows a descendent window to have translucency (alpha) and transparency (color-key) effects,
            /// but only if the descendent window also has the WS_EX_TRANSPARENT bit set.
            /// Double-buffering allows the window and its descendents to be painted without flicker.
            /// </remarks>
            WS_EX_COMPOSITED = 0x02000000,

            /// <summary>
            /// Specifies a window that includes a question mark in the title bar. When the user clicks the question mark,
            /// the cursor changes to a question mark with a pointer. If the user then clicks a child window, the child receives a WM_HELP message.
            /// The child window should pass the message to the parent window procedure, which should call the WinHelp function using the HELP_WM_HELP command.
            /// The Help application displays a pop-up window that typically contains help for the child window.
            /// WS_EX_CONTEXTHELP cannot be used with the WS_MAXIMIZEBOX or WS_MINIMIZEBOX styles.
            /// </summary>
            WS_EX_CONTEXTHELP = 0x00000400,

            /// <summary>
            /// Specifies a window which contains child windows that should take part in dialog box navigation.
            /// If this style is specified, the dialog manager recurses into children of this window when performing navigation operations
            /// such as handling the TAB key, an arrow key, or a keyboard mnemonic.
            /// </summary>
            WS_EX_CONTROLPARENT = 0x00010000,

            /// <summary>Specifies a window that has a double border.</summary>
            WS_EX_DLGMODALFRAME = 0x00000001,

            /// <summary>
            /// Specifies a window that is a layered window.
            /// This cannot be used for child windows or if the window has a class style of either CS_OWNDC or CS_CLASSDC.
            /// </summary>
            WS_EX_LAYERED = 0x00080000,

            /// <summary>
            /// Specifies a window with the horizontal origin on the right edge. Increasing horizontal values advance to the left.
            /// The shell language must support reading-order alignment for this to take effect.
            /// </summary>
            WS_EX_LAYOUTRTL = 0x00400000,

            /// <summary>Specifies a window that has generic left-aligned properties. This is the default.</summary>
            WS_EX_LEFT = 0x00000000,

            /// <summary>
            /// Specifies a window with the vertical scroll bar (if present) to the left of the client area.
            /// The shell language must support reading-order alignment for this to take effect.
            /// </summary>
            WS_EX_LEFTSCROLLBAR = 0x00004000,

            /// <summary>
            /// Specifies a window that displays text using left-to-right reading-order properties. This is the default.
            /// </summary>
            WS_EX_LTRREADING = 0x00000000,

            /// <summary>
            /// Specifies a multiple-document interface (MDI) child window.
            /// </summary>
            WS_EX_MDICHILD = 0x00000040,

            /// <summary>
            /// Specifies a top-level window created with this style does not become the foreground window when the user clicks it.
            /// The system does not bring this window to the foreground when the user minimizes or closes the foreground window.
            /// The window does not appear on the taskbar by default. To force the window to appear on the taskbar, use the WS_EX_APPWINDOW style.
            /// To activate the window, use the SetActiveWindow or SetForegroundWindow function.
            /// </summary>
            WS_EX_NOACTIVATE = 0x08000000,

            /// <summary>
            /// Specifies a window which does not pass its window layout to its child windows.
            /// </summary>
            WS_EX_NOINHERITLAYOUT = 0x00100000,

            /// <summary>
            /// Specifies that a child window created with this style does not send the WM_PARENTNOTIFY message to its parent window when it is created or destroyed.
            /// </summary>
            WS_EX_NOPARENTNOTIFY = 0x00000004,

            /// <summary>
            /// The window does not render to a redirection surface.
            /// This is for windows that do not have visible content or that use mechanisms other than surfaces to provide their visual.
            /// </summary>
            WS_EX_NOREDIRECTIONBITMAP = 0x00200000,

            /// <summary>Specifies an overlapped window.</summary>
            WS_EX_OVERLAPPEDWINDOW = WS_EX_WINDOWEDGE | WS_EX_CLIENTEDGE,

            /// <summary>Specifies a palette window, which is a modeless dialog box that presents an array of commands.</summary>
            WS_EX_PALETTEWINDOW = WS_EX_WINDOWEDGE | WS_EX_TOOLWINDOW | WS_EX_TOPMOST,

            /// <summary>
            /// Specifies a window that has generic "right-aligned" properties. This depends on the window class.
            /// The shell language must support reading-order alignment for this to take effect.
            /// Using the WS_EX_RIGHT style has the same effect as using the SS_RIGHT (static), ES_RIGHT (edit), and BS_RIGHT/BS_RIGHTBUTTON (button) control styles.
            /// </summary>
            WS_EX_RIGHT = 0x00001000,

            /// <summary>Specifies a window with the vertical scroll bar (if present) to the right of the client area. This is the default.</summary>
            WS_EX_RIGHTSCROLLBAR = 0x00000000,

            /// <summary>
            /// Specifies a window that displays text using right-to-left reading-order properties.
            /// The shell language must support reading-order alignment for this to take effect.
            /// </summary>
            WS_EX_RTLREADING = 0x00002000,

            /// <summary>Specifies a window with a three-dimensional border style intended to be used for items that do not accept user input.</summary>
            WS_EX_STATICEDGE = 0x00020000,

            /// <summary>
            /// Specifies a window that is intended to be used as a floating toolbar.
            /// A tool window has a title bar that is shorter than a normal title bar, and the window title is drawn using a smaller font.
            /// A tool window does not appear in the taskbar or in the dialog that appears when the user presses ALT+TAB.
            /// If a tool window has a system menu, its icon is not displayed on the title bar.
            /// However, you can display the system menu by right-clicking or by typing ALT+SPACE.
            /// </summary>
            WS_EX_TOOLWINDOW = 0x00000080,

            /// <summary>
            /// Specifies a window that should be placed above all non-topmost windows and should stay above them, even when the window is deactivated.
            /// To add or remove this style, use the SetWindowPos function.
            /// </summary>
            WS_EX_TOPMOST = 0x00000008,

            /// <summary>
            /// Specifies a window that should not be painted until siblings beneath the window (that were created by the same thread) have been painted.
            /// The window appears transparent because the bits of underlying sibling windows have already been painted.
            /// To achieve transparency without these restrictions, use the SetWindowRgn function.
            /// </summary>
            WS_EX_TRANSPARENT = 0x00000020,

            /// <summary>Specifies a window that has a border with a raised edge.</summary>
            WS_EX_WINDOWEDGE = 0x00000100
        }

        /// <summary>
        /// Window Styles.
        /// The following styles can be specified wherever a window style is required. After the control has been created, these styles cannot be modified, except as noted.
        /// </summary>
        [Flags()]
        public enum WindowStyles : uint
        {
            /// <summary>The window has a thin-line border.</summary>
            WS_BORDER = 0x800000,

            /// <summary>The window has a title bar (includes the WS_BORDER style).</summary>
            WS_CAPTION = 0xc00000,

            /// <summary>The window is a child window. A window with this style cannot have a menu bar. This style cannot be used with the WS_POPUP style.</summary>
            WS_CHILD = 0x40000000,

            /// <summary>Excludes the area occupied by child windows when drawing occurs within the parent window. This style is used when creating the parent window.</summary>
            WS_CLIPCHILDREN = 0x2000000,

            /// <summary>
            /// Clips child windows relative to each other; that is, when a particular child window receives a WM_PAINT message, the WS_CLIPSIBLINGS style clips all other overlapping child windows out of the region of the child window to be updated.
            /// If WS_CLIPSIBLINGS is not specified and child windows overlap, it is possible, when drawing within the client area of a child window, to draw within the client area of a neighboring child window.
            /// </summary>
            WS_CLIPSIBLINGS = 0x4000000,

            /// <summary>The window is initially disabled. A disabled window cannot receive input from the user. To change this after a window has been created, use the EnableWindow function.</summary>
            WS_DISABLED = 0x8000000,

            /// <summary>The window has a border of a style typically used with dialog boxes. A window with this style cannot have a title bar.</summary>
            WS_DLGFRAME = 0x400000,

            /// <summary>
            /// The window is the first control of a group of controls. The group consists of this first control and all controls defined after it, up to the next control with the WS_GROUP style.
            /// The first control in each group usually has the WS_TABSTOP style so that the user can move from group to group. The user can subsequently change the keyboard focus from one control in the group to the next control in the group by using the direction keys.
            /// You can turn this style on and off to change dialog box navigation. To change this style after a window has been created, use the SetWindowLong function.
            /// </summary>
            WS_GROUP = 0x20000,

            /// <summary>The window has a horizontal scroll bar.</summary>
            WS_HSCROLL = 0x100000,

            /// <summary>The window is initially maximized.</summary>
            WS_MAXIMIZE = 0x1000000,

            /// <summary>The window has a maximize button. Cannot be combined with the WS_EX_CONTEXTHELP style. The WS_SYSMENU style must also be specified.</summary>
            WS_MAXIMIZEBOX = 0x10000,

            /// <summary>The window is initially minimized.</summary>
            WS_MINIMIZE = 0x20000000,

            /// <summary>The window has a minimize button. Cannot be combined with the WS_EX_CONTEXTHELP style. The WS_SYSMENU style must also be specified.</summary>
            WS_MINIMIZEBOX = 0x20000,

            /// <summary>The window is an overlapped window. An overlapped window has a title bar and a border.</summary>
            WS_OVERLAPPED = 0x0,

            /// <summary>The window is an overlapped window.</summary>
            WS_OVERLAPPEDWINDOW = WS_OVERLAPPED | WS_CAPTION | WS_SYSMENU | WS_SIZEFRAME | WS_MINIMIZEBOX | WS_MAXIMIZEBOX,

            /// <summary>The window is a pop-up window. This style cannot be used with the WS_CHILD style.</summary>
            WS_POPUP = 0x80000000u,

            /// <summary>The window is a pop-up window. The WS_CAPTION and WS_POPUPWINDOW styles must be combined to make the window menu visible.</summary>
            WS_POPUPWINDOW = WS_POPUP | WS_BORDER | WS_SYSMENU,

            /// <summary>The window has a sizing border.</summary>
            WS_SIZEFRAME = 0x40000,

            /// <summary>The window has a window menu on its title bar. The WS_CAPTION style must also be specified.</summary>
            WS_SYSMENU = 0x80000,

            /// <summary>
            /// The window is a control that can receive the keyboard focus when the user presses the TAB key.
            /// Pressing the TAB key changes the keyboard focus to the next control with the WS_TABSTOP style.
            /// You can turn this style on and off to change dialog box navigation. To change this style after a window has been created, use the SetWindowLong function.
            /// For user-created windows and modeless dialogs to work with tab stops, alter the message loop to call the IsDialogMessage function.
            /// </summary>
            WS_TABSTOP = 0x10000,

            /// <summary>The window is initially visible. This style can be turned on and off by using the ShowWindow or SetWindowPos function.</summary>
            WS_VISIBLE = 0x10000000,

            /// <summary>The window has a vertical scroll bar.</summary>
            WS_VSCROLL = 0x200000
        }

        [Flags()]
        public enum WindowLongFlags : int
        {
            GWL_EXSTYLE = -20,
            GWLP_HINSTANCE = -6,
            GWLP_HWNDPARENT = -8,
            GWL_ID = -12,
            GWL_STYLE = -16,
            GWL_USERDATA = -21,
            GWL_WNDPROC = -4,
            DWLP_USER = 0x8,
            DWLP_MSGRESULT = 0x0,
            DWLP_DLGPROC = 0x4
        }
        #endregion Window Styles

        #region Read/Write INI file

        //Robert_Lin 2024-7-5 copy from VCPCorePlugin.cs, shared with other projects
        public static int IniReadInt(string sec, string key, int def, string pathName)
        {
            return _GetPrivateProfileInt(sec, key, def, pathName);
        }

        //Usage: int value=GetPrivateProfileInt("sectionName", "key", 3, @"C:\temp\a.ini");
        [DllImport("kernel32", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        private static extern int GetPrivateProfileInt(string section, string key, int def, string filePath);

        private static int _GetPrivateProfileInt(string section, string key, int def, string filePath)
        {
            return GetPrivateProfileInt(section, key, def, filePath);
        }

        //Uage:
        // //allocate string buffer, for large string you can allocate 4096 chars.
        // StringBuilder sb1=new StringBuilder(255);
        // int charsRet=GetPrivateProfileString("secName","key","defValue",sb1,sb1.Capacity,@"C:\temp\a.ini");
        // string result=sb1.ToString();
        /*[DllImport("kernel32", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        private static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);

        public static int _GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath)
        {
            return GetPrivateProfileString(section, key, def, retVal, size, filePath);
        }*/

        #endregion Read/Write INI file

        #region Actions (Wayn)

        // Import keybd_event function
        /*[DllImport("user32.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        private static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, UIntPtr dwExtraInfo);

        public static void _keybd_event(byte bVk, byte bScan, uint dwFlags, UIntPtr dwExtraInfo)
        {
            keybd_event(bVk, bScan, dwFlags, dwExtraInfo);
        }*/

        // Import mouse_event function
        /*[DllImport("user32.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        private static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint dwData, UIntPtr dwExtraInfo);

        public static void _mouse_event(uint dwFlags, uint dx, uint dy, uint dwData, UIntPtr dwExtraInfo)
        {
            mouse_event(dwFlags, dx, dy, dwData, dwExtraInfo);
        }*/

        // Constants for keybd events
        public const uint KEYEVENTF_KEYDOWN = 0x0000;

        public const uint KEYEVENTF_KEYUP = 0x0002;

        // Constants for mouse events
        public const uint MOUSEEVENTF_MIDDLEDOWN = 0x0020;

        public const uint MOUSEEVENTF_MIDDLEUP = 0x0040;

        public const uint MOUSEEVENTF_WHEEL = 0x0800;
        public const uint WHEEL_DELTA = 120;

        public const int VK_LWIN = 0x5B;
        public const int VK_SHIFT = 0x10;
        public const int VK_S = 0x53;
        public const int VK_A = 0x41;
        public const int VK_TAB = 0x09;
        public const int VK_L = 0x4C;
        public const int VK_E = 0x45;
        public const int VK_D = 0x44;
        public const int VK_M = 0x4D;

        public const byte VK_CONTROL = 0x11;
        public const byte VK_ALT = 0x12;
        public const byte VK_F5 = 0x74;
        public const byte VK_F6 = 0x75;
        public const byte VK_F7 = 0x76;
        public const byte VK_F8 = 0x77;
        public const byte VK_F9 = 0x78;
        public const byte VK_F12 = 0x7B;
        public const byte VK_INSERT = 0x2D;
        public const byte VK_DELETE = 0x2E;

        #endregion Actions (Wayn)

        #region EnumWindows

        public static List<IntPtr> GetAltTabWindows()
        {
            List<IntPtr> hWnds = Win32.GetWindowHandles(IsAltTabWindow);
            return hWnds;
        }

        //Reference: https://stackoverflow.com/questions/210504/enumerate-windows-like-alt-tab-does
        //Try to get the Windows like [Alt]+[Tab] key
        private static bool IsAltTabWindow(IntPtr hWnd, IntPtr lParam)
        {
            //1 The window must be visible
            if (!Win32._IsWindowVisible(hWnd))
                return false;

            //2 The window must not be a toolwindow
            uint winStyle = (uint)Win32._GetWindowLong(hWnd, (int)Win32.WindowLongFlags.GWL_EXSTYLE);
            if ((winStyle & (uint)Win32.WindowStylesEx.WS_EX_TOOLWINDOW) != 0)
            {
                return false;
            }

            if (Win32._GetAncestor(hWnd, Win32.eGaFlags.GA_ROOTOWNER) != hWnd)
            {
                return false;
            }

            uint cloaked;
            Win32._DwmGetWindowAttribute(hWnd, Win32.eDwmWindowAttribute.Cloaked, out cloaked, sizeof(uint));
            if (cloaked == Win32.DWM_CLOAKED_SHELL)
            {
                return false;
            }

            //Check if the window is minimized
            uint uiStyles = (uint)Win32._GetWindowLong(hWnd, (int)Win32.WindowLongFlags.GWL_STYLE);
            uint uiMinimizeStyle = (uint)Win32.WindowStyles.WS_MINIMIZE;
            bool isMinimized = ((uiStyles & uiMinimizeStyle) == uiMinimizeStyle);
            //if (isMinimized) //Remove by pass Minimized 
            //    return false;

            //Check if the window across screen boundary
            //It need Screen rect, will be check after returned

            return true;
        }
        //The major (high-level) method to Enumerate Windows is GetWindowHandles()

        /// <summary>
        /// EnumWindows with the 'proc' as the firter, and then add all acceptable WindowHandles as output.
        /// find the sample code in DDPM.SA.Plugins.User.EasyArrange / EAEditWindow.xaml.cs / CaptureCustomLayout().
        /// </summary>
        /// <param name="proc"></param>
        /// <returns>A list of hWnd which is accepted by 'proc'
        /// If proc is null, then all enumerated handles will be output.</returns>
        public static List<IntPtr> GetWindowHandles(EnumWindowsProc? proc = null)
        {
            List<IntPtr> listOut = new List<IntPtr>();
            Win32._EnumWindows(delegate (IntPtr hWnd, IntPtr lParam)
            {
                if (proc != null)
                {
                    if (!proc(hWnd, lParam))
                        return true;
                }
                listOut.Add(hWnd);
                return true;
            }, IntPtr.Zero);
            return listOut;
        }

        //The callback delegate for EnumWindows( )
        public delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        private static extern bool EnumWindows(EnumWindowsProc enumProc, IntPtr lParam);

        //Set to private currently, you can change it to public if you need.
        private static bool _EnumWindows(EnumWindowsProc proc, IntPtr lParam)
        {
            return EnumWindows(proc, lParam);
        }

        //EnumChildWindows( )
        [DllImport("user32.dll")]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool EnumChildWindows(IntPtr hwndParent, EnumWindowsProc lpEnumFunc, IntPtr lParam);
        public static bool _EnumChildWindows(IntPtr hwndParent, EnumWindowsProc proc, IntPtr lParam)
        {
            return EnumChildWindows(hwndParent, proc, lParam);
        }

        //IsWindowVisible()
        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        private static extern bool IsWindowVisible(IntPtr hWnd);
        public static bool _IsWindowVisible(IntPtr hWnd)
        {
            return IsWindowVisible(hWnd);
        }

        //GetAncestor()
        //
        //GA Falgs
        public enum eGaFlags : uint
        {
            GA_PARENT = 1,
            GA_ROOT = 2,
            GA_ROOTOWNER = 3
        }

        /// <summary>Retrieves the handle to the ancestor of the specified window.</summary>
        /// <param name="hWnd">
        ///     A handle to the window whose ancestor is to be retrieved. If this parameter is the desktop window,
        ///     the function returns <see cref="IntPtr.Zero" />.
        /// </param>
        /// <param name="gaFlags">The ancestor to be retrieved.</param>
        /// <returns>The handle to the ancestor window.</returns>
        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        private static extern IntPtr GetAncestor(IntPtr hWnd, eGaFlags gaFlags);
        public static IntPtr _GetAncestor(IntPtr hWnd, eGaFlags gaFlags)
        {
            return GetAncestor(hWnd, gaFlags);
        }

        //DwmGetWindowAttribute()
        //
        public enum eDwmWindowAttribute : uint
        {
            NCRenderingEnabled = 1,
            NCRenderingPolicy,
            TransitionsForceDisabled,
            AllowNCPaint,
            CaptionButtonBounds,
            NonClientRtlLayout,
            ForceIconicRepresentation,
            Flip3DPolicy,
            ExtendedFrameBounds,
            HasIconicBitmap,
            DisallowPeek,
            ExcludedFromPeek,
            Cloak,
            Cloaked,
            FreezeRepresentation,
            PassiveUpdateMode,
            UseHostBackdropBrush,
            UseImmersiveDarkMode = 20,
            WindowCornerPreference = 33,
            BorderColor,
            CaptionColor,
            TextColor,
            VisibleFrameBorderThickness,
            SystemBackdropType,
            Last
        }

        public const uint DWM_CLOAKED_APP = 0x00000001; //視窗是由其擁有者應用程式所遮蔽。
        public const uint DWM_CLOAKED_SHELL = 0x00000002; //視窗已由殼層遮蔽。
        public const uint DWM_CLOAKED_INHERITED = 0x00000004; //封閉值繼承自其擁有者視窗。

        [DllImport("dwmapi.dll")]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        private static extern int DwmGetWindowAttribute(IntPtr hwnd, eDwmWindowAttribute dwAttribute, out uint pvAttribute, int cbAttribute);
        public static int _DwmGetWindowAttribute(IntPtr hwnd, eDwmWindowAttribute dwAttribute, out uint pvAttribute, int cbAttribute)
        {
            return DwmGetWindowAttribute(hwnd, dwAttribute, out pvAttribute, cbAttribute);
        }

        [DllImport("dwmapi.dll")]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        private static extern int DwmGetWindowAttribute(IntPtr hWnd, eDwmWindowAttribute dwAttribute, ref RECT pvAttribute, int cbAttribute);
        
        public static int _DwmGetWindowAttribute(IntPtr hwnd, eDwmWindowAttribute dwAttribute, ref RECT pvAttribute, int cbAttribute)
        {
            return DwmGetWindowAttribute(hwnd, dwAttribute, ref pvAttribute, cbAttribute);
        }

        //GetParent()
        [DllImport("user32.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        private static extern IntPtr GetParent(IntPtr hWnd);
        public static IntPtr _GetParent(IntPtr hWnd)
        {
            return GetParent(hWnd);
        }

        #endregion EnumWindows

        #region RECT, POINT
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

            public static implicit operator System.Drawing.Point(POINT p)
            {
                return new System.Drawing.Point(p.X, p.Y);
            }

            public static implicit operator POINT(System.Drawing.Point p)
            {
                return new POINT(p.X, p.Y);
            }

            public override string ToString()
            {
                return $"X: {X}, Y: {Y}";
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left, Top, Right, Bottom;

            public RECT(int left, int top, int right, int bottom)
            {
                Left = left;
                Top = top;
                Right = right;
                Bottom = bottom;
            }

            public RECT(System.Drawing.Rectangle r) : this(r.Left, r.Top, r.Right, r.Bottom) { }

            public int X
            {
                get { return Left; }
                set { Right -= (Left - value); Left = value; }
            }

            public int Y
            {
                get { return Top; }
                set { Bottom -= (Top - value); Top = value; }
            }

            public int Height
            {
                get { return Bottom - Top; }
                set { Bottom = value + Top; }
            }

            public int Width
            {
                get { return Right - Left; }
                set { Right = value + Left; }
            }

            public System.Drawing.Point Location
            {
                get { return new System.Drawing.Point(Left, Top); }
                set { X = value.X; Y = value.Y; }
            }

            public System.Drawing.Size Size
            {
                get { return new System.Drawing.Size(Width, Height); }
                set { Width = value.Width; Height = value.Height; }
            }

            public static implicit operator System.Drawing.Rectangle(RECT r)
            {
                return new System.Drawing.Rectangle(r.Left, r.Top, r.Width, r.Height);
            }

            public static implicit operator RECT(System.Drawing.Rectangle r)
            {
                return new RECT(r);
            }

            public static bool operator ==(RECT r1, RECT r2)
            {
                return r1.Equals(r2);
            }

            public static bool operator !=(RECT r1, RECT r2)
            {
                return !r1.Equals(r2);
            }

            public bool Equals(RECT r)
            {
                return r.Left == Left && r.Top == Top && r.Right == Right && r.Bottom == Bottom;
            }

            public override bool Equals(object obj)
            {
                if (obj is RECT)
                    return Equals((RECT)obj);
                else if (obj is System.Drawing.Rectangle)
                    return Equals(new RECT((System.Drawing.Rectangle)obj));
                return false;
            }

            public override int GetHashCode()
            {
                return ((System.Drawing.Rectangle)this).GetHashCode();
            }

            public override string ToString()
            {
                //return string.Format(System.Globalization.CultureInfo.CurrentCulture, "{{Left={0},Top={1},Right={2},Bottom={3}}}", Left, Top, Right, Bottom);
                return string.Format(System.Globalization.CultureInfo.CurrentCulture, "({0},{1})-({2},{3}){4}x{5}", Left, Top, Right, Bottom, Width, Height);
            }
        }

        //GetWindowRect()
        [DllImport("user32.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        private static extern bool GetWindowRect(IntPtr hwnd, out RECT lpRect);
        public static bool _GetWindowRect(IntPtr hwnd, out RECT lpRect)
        {
            return GetWindowRect(hwnd, out lpRect);
        }

        #endregion

        #region Window Text
        //GetWindowText()
        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder strText, int maxCount);
        public static string _GetWindowText(IntPtr hWnd)
        {
            // Allocate correct string length first
            int length = _GetWindowTextLength(hWnd);
            StringBuilder sb = new StringBuilder(length + 1);
            GetWindowText(hWnd, sb, sb.Capacity);
            return sb.ToString();
        }

        //GetWindowTextLength()
        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        private static extern int GetWindowTextLength(IntPtr hWnd);
        public static int _GetWindowTextLength(IntPtr hWnd)
        {
            return GetWindowTextLength(hWnd);
        }

        #endregion

        #region Window Position, Placement
        //Robert_Lin, 2024-12-5 added from EasyMemory
        [DllImport("user32.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetForegroundWindow(IntPtr hWnd);
        public static bool _SetForegroundWindow(IntPtr hWnd)
        {
            return SetForegroundWindow(hWnd);
        }


        /// <summary>
        /// Sets the show state and the restored, minimized, and maximized positions of the specified window.
        /// </summary>
        /// <param name="hWnd">
        /// A handle to the window.
        /// </param>
        /// <param name="lpwndpl">
        /// A pointer to a WINDOWPLACEMENT structure that specifies the new show state and window positions.
        /// <para>
        /// Before calling SetWindowPlacement, set the length member of the WINDOWPLACEMENT structure to sizeof(WINDOWPLACEMENT). SetWindowPlacement fails if the length member is not set correctly.
        /// </para>
        /// </param>
        /// <returns>
        /// If the function succeeds, the return value is nonzero.
        /// <para>
        /// If the function fails, the return value is zero. To get extended error information, call GetLastError.
        /// </para>
        /// </returns>
        
        /*[DllImport("user32.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetWindowPlacement(IntPtr hWnd, [In] ref WINDOWPLACEMENT lpwndpl);
        public static bool _SetWindowPlacement(IntPtr hWnd, [In] ref WINDOWPLACEMENT lpwndpl)
        {
            return SetWindowPlacement(hWnd, ref lpwndpl);
        }*/

        [DllImport("user32.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetWindowPlacement(IntPtr hWnd, ref WINDOWPLACEMENT lpwndpl);
        public static bool _GetWindowPlacement(IntPtr hWnd, ref WINDOWPLACEMENT lpwndpl)
        {
            return GetWindowPlacement(hWnd, ref lpwndpl);
        }

        /// <summary>
        /// Contains information about the placement of a window on the screen.
        /// </summary>
        [Serializable]
        [StructLayout(LayoutKind.Sequential)]
        public struct WINDOWPLACEMENT
        {
            /// <summary>
            /// The length of the structure, in bytes. Before calling the GetWindowPlacement or SetWindowPlacement functions, set this member to sizeof(WINDOWPLACEMENT).
            /// <para>
            /// GetWindowPlacement and SetWindowPlacement fail if this member is not set correctly.
            /// </para>
            /// </summary>
            public int Length;

            /// <summary>
            /// Specifies flags that control the position of the minimized window and the method by which the window is restored.
            /// </summary>
            public int Flags;

            /// <summary>
            /// The current show state of the window.
            /// </summary>
            public ShowWindowCommands ShowCmd;

            /// <summary>
            /// The coordinates of the window's upper-left corner when the window is minimized.
            /// </summary>
            public POINT MinPosition;

            /// <summary>
            /// The coordinates of the window's upper-left corner when the window is maximized.
            /// </summary>
            public POINT MaxPosition;

            /// <summary>
            /// The window's coordinates when the window is in the restored position.
            /// </summary>
            public RECT NormalPosition;

            /// <summary>
            /// Gets the default (empty) value.
            /// </summary>
            public static WINDOWPLACEMENT Default
            {
                get
                {
                    WINDOWPLACEMENT result = new WINDOWPLACEMENT();
                    result.Length = Marshal.SizeOf(result);
                    return result;
                }
            }
        }


        public enum ShowWindowCommands 
        {
            /// <summary>
            /// Hides the window and activates another window.
            /// </summary>
            Hide = 0,
            /// <summary>
            /// Activates and displays a window. If the window is minimized or
            /// maximized, the system restores it to its original size and position.
            /// An application should specify this flag when displaying the window
            /// for the first time.
            /// </summary>
            Normal = 1,
            /// <summary>
            /// Activates the window and displays it as a minimized window.
            /// </summary>
            ShowMinimized = 2,
            /// <summary>
            /// Maximizes the specified window.
            /// </summary>
            Maximize = 3, // is this the right value?
            /// <summary>
            /// Activates the window and displays it as a maximized window.
            /// </summary>      
            ShowMaximized = 3,
            /// <summary>
            /// Displays a window in its most recent size and position. This value
            /// is similar to <see cref="Win32.ShowWindowCommand.Normal"/>, except
            /// the window is not activated.
            /// </summary>
            ShowNoActivate = 4,
            /// <summary>
            /// Activates the window and displays it in its current size and position.
            /// </summary>
            Show = 5,
            /// <summary>
            /// Minimizes the specified window and activates the next top-level
            /// window in the Z order.
            /// </summary>
            Minimize = 6,
            /// <summary>
            /// Displays the window as a minimized window. This value is similar to
            /// <see cref="Win32.ShowWindowCommand.ShowMinimized"/>, except the
            /// window is not activated.
            /// </summary>
            ShowMinNoActive = 7,
            /// <summary>
            /// Displays the window in its current size and position. This value is
            /// similar to <see cref="Win32.ShowWindowCommand.Show"/>, except the
            /// window is not activated.
            /// </summary>
            ShowNA = 8,
            /// <summary>
            /// Activates and displays the window. If the window is minimized or
            /// maximized, the system restores it to its original size and position.
            /// An application should specify this flag when restoring a minimized window.
            /// </summary>
            Restore = 9,
            /// <summary>
            /// Sets the show state based on the SW_* value specified in the
            /// STARTUPINFO structure passed to the CreateProcess function by the
            /// program that started the application.
            /// </summary>
            ShowDefault = 10,
            /// <summary>
            ///  <b>Windows 2000/XP:</b> Minimizes a window, even if the thread
            /// that owns the window is not responding. This flag should only be
            /// used when minimizing windows from a different thread.
            /// </summary>
            ForceMinimize = 11
        }

        //ShowWindow()
        [DllImport("user32.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        public static bool _ShowWindow(IntPtr hWnd, ShowWindowCommands nCmdShow)
        {
            return ShowWindow(hWnd, (int) nCmdShow);
        }
        #endregion

        #region Window ClassName
        [DllImport("user32.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        private static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);
        public static string _GetClassName(IntPtr hWnd)
        {
            StringBuilder className = new StringBuilder(256);
            int retChars = GetClassName(hWnd, className, className.Capacity);
            if (retChars <= 0)
            {
                return "";
            }
            return className.ToString();
        }
        #endregion

        #region Process

        private static bool EnumChildProc(IntPtr hWnd, IntPtr lParam)
        {
            return true;
        }

        /// <summary>
        /// Get Process from window handle (hWnd)
        /// </summary>
        /// <param name="hWnd">[IN] Window handle</param>
        /// <param name="p">[OUT] Process</param>
        /// <param name="msg">[OUT] The error message if return false</param>
        /// <returns></returns>
        public static bool GetProcessFromWindowHandle(IntPtr hWnd, out Process p, out string msg)
        {
            if (hWnd == IntPtr.Zero)
            {
                p = null;
                msg = "ERR, Window handle is null";
                return false;
            }

            try
            {
                //Get ProcessId from window handle
                uint processId = 0;
                uint threadId = Win32._GetWindowThreadProcessId(hWnd, out processId);

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
            catch (Exception ex)
            {
                msg = ex.Message;
                p = null;
            }
            return false;
        }

        //GetWindowThreadProcessId()
        [DllImport("user32.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);

        public static uint _GetWindowThreadProcessId(IntPtr hWnd, out uint processId)
        {
            return GetWindowThreadProcessId(hWnd, out processId);
        }

        //QueryFullProcessImageName()
        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        private static extern bool QueryFullProcessImageName(IntPtr hProcess, uint dwFlags, [Out, MarshalAs(UnmanagedType.LPTStr)] StringBuilder lpExeName, ref uint lpdwSize);

        public static bool _QueryFullProcessImageName(IntPtr hProcess, uint dwFlags, [Out, MarshalAs(UnmanagedType.LPTStr)] StringBuilder lpExeName, ref uint lpdwSize)
        {
            return QueryFullProcessImageName(hProcess, dwFlags, lpExeName, ref lpdwSize);
        }

        //OpenProcess()
        [DllImport("kernel32.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        private static extern IntPtr OpenProcess(uint processAccess, bool bInheritHandle, uint processId);
        public static IntPtr _OpenProcess(uint processAccess, bool bInheritHandle, uint processId)
        {
            return OpenProcess(processAccess, bInheritHandle, processId);
        }

        #endregion


        #region PropertyStore
        public enum HRESULT : int
        {
            S_OK = 0,
            S_FALSE = 1,
            E_NOINTERFACE = unchecked((int)0x80004002),
            E_NOTIMPL = unchecked((int)0x80004001),
            E_FAIL = unchecked((int)0x80004005)
        }

        public struct PROPERTYKEY
        {
            public PROPERTYKEY(Guid InputId, UInt32 InputPid)
            {
                fmtid = InputId;
                pid = InputPid;
            }
            Guid fmtid;
            uint pid;
        };

        [StructLayout(LayoutKind.Sequential, Pack = 0)]
        public struct PROPARRAY
        {
            public UInt32 cElems;
            public IntPtr pElems;
        }

        [ComImport, Guid("886D8EEB-8CF2-4446-8D02-CDBA1DBDCF99"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        public interface IPropertyStore
        {
            HRESULT GetCount([Out] out uint propertyCount);
            HRESULT GetAt([In] uint propertyIndex, [Out, MarshalAs(UnmanagedType.Struct)] out PROPERTYKEY key);
            HRESULT GetValue([In, MarshalAs(UnmanagedType.Struct)] ref PROPERTYKEY key, [Out, MarshalAs(UnmanagedType.Struct)] out PROPVARIANT pv);
            HRESULT SetValue([In, MarshalAs(UnmanagedType.Struct)] ref PROPERTYKEY key, [In, MarshalAs(UnmanagedType.Struct)] ref PROPVARIANT pv);
            HRESULT Commit();
        }

        [StructLayout(LayoutKind.Explicit, Pack = 1)]
        public struct PROPVARIANT
        {
            [FieldOffset(0)]
            public ushort varType;
            [FieldOffset(2)]
            public ushort wReserved1;
            [FieldOffset(4)]
            public ushort wReserved2;
            [FieldOffset(6)]
            public ushort wReserved3;

            [FieldOffset(8)]
            public byte bVal;
            [FieldOffset(8)]
            public sbyte cVal;
            [FieldOffset(8)]
            public ushort uiVal;
            [FieldOffset(8)]
            public short iVal;
            [FieldOffset(8)]
            public UInt32 uintVal;
            [FieldOffset(8)]
            public Int32 intVal;
            [FieldOffset(8)]
            public UInt64 ulVal;
            [FieldOffset(8)]
            public Int64 lVal;
            [FieldOffset(8)]
            public float fltVal;
            [FieldOffset(8)]
            public double dblVal;
            [FieldOffset(8)]
            public short boolVal;
            [FieldOffset(8)]
            public IntPtr pclsidVal; // GUID ID pointer
            [FieldOffset(8)]
            public IntPtr pszVal; // Ansi string pointer
            [FieldOffset(8)]
            public IntPtr pwszVal; // Unicode string pointer
            [FieldOffset(8)]
            public IntPtr punkVal; // punkVal (interface pointer)
            [FieldOffset(8)]
            public PROPARRAY ca;
            [FieldOffset(8)]
            public System.Runtime.InteropServices.ComTypes.FILETIME filetime;
        }

        public static PROPERTYKEY PKEY_AppUserModel_ID = new PROPERTYKEY(new Guid("9F4C2855-9F79-4B39-A8D0-E1D42DE1D5F3"), 5);

        //SHGetPropertyStoreForWindow()
        [DllImport("Shell32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        private static extern HRESULT SHGetPropertyStoreForWindow(IntPtr hwnd, ref Guid iid, [Out(), MarshalAs(UnmanagedType.Interface)] out IPropertyStore propertyStore);
        private static HRESULT _SHGetPropertyStoreForWindow(IntPtr hwnd, ref Guid iid, [Out(), MarshalAs(UnmanagedType.Interface)] out IPropertyStore propertyStore)
        {
            return SHGetPropertyStoreForWindow(hwnd, ref iid, out propertyStore);
        }
        #endregion

        #region UWP
        public const uint PROCESS_QUERY_INFORMATION = 0x0400;
        public const uint PROCESS_VM_READ = 0x0010;

        public static string GetUwpAppPathName(IntPtr hWndParent, uint processId, out IntPtr hProcessChild)
        {
            List<uint> childPids = new List<uint>();
            hProcessChild = IntPtr.Zero;
            //EnumWindowsProc funcEnumWin = EnumChildProc;
            //_EnumChildWindows(hWndParent, funcEnumWin, IntPtr.Zero);

            _EnumChildWindows(hWndParent, delegate (IntPtr hWnd, IntPtr lParam)
            {
                try
                {
                    uint pid;
                    _GetWindowThreadProcessId(hWnd, out pid);
                    if (pid != processId)
                        childPids.Add(pid);
                }
                catch (Exception ex2)
                {

                }
                return true;
            }
            , IntPtr.Zero);

            if (childPids.Count > 0)
            {
                uint dwDesiredAccess = Win32.PROCESS_QUERY_INFORMATION | Win32.PROCESS_VM_READ;
                IntPtr hProcess = _OpenProcess(dwDesiredAccess, false, childPids[0]);
                if (hProcess == IntPtr.Zero)
                {
                    return string.Empty;
                }
                hProcessChild = hProcess;
                uint lpdwSize = 2048;
                StringBuilder sb = new StringBuilder((int)lpdwSize);
                if (Win32._QueryFullProcessImageName(hProcess, 0, sb, ref lpdwSize))
                {
                    return sb.ToString();
                }
            }
            return String.Empty;
        }

        /// <summary>
        /// Get the AppUserModelId from the WindowHanlde of an UWP App.
        /// </summary>
        /// <param name="hWnd">[IN] The window handle to get</param>
        /// <param name="outString">[OUT] the AppUserModelId of the UWP app if return true. Or the error message if return false.</param>
        /// <returns></returns>
        public static bool GetUwpAppUserModelId(IntPtr hWnd, out string outString)
        {
            IPropertyStore propertyStore;
            Guid guid = new Guid("{886D8EEB-8CF2-4446-8D02-CDBA1DBDCF99}");
            HRESULT hr = _SHGetPropertyStoreForWindow(hWnd, ref guid, out propertyStore);
            if (hr == HRESULT.S_OK)
            {
                PROPVARIANT propVar = new PROPVARIANT();
                hr = propertyStore.GetValue(ref PKEY_AppUserModel_ID, out propVar);
                outString = Marshal.PtrToStringUni(propVar.pwszVal);
                return true;
            }
            else
            {
                outString = $"SHGetPropertyStoreForWindow(hWnd=0x{hWnd:X}) return {hr}";
                return false;
            }
        }
        #endregion UWP

    }
}