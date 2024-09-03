using DDPM.SA.Common.Settings;
using System;
using System.Runtime.InteropServices;
using System.Text;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace DDPM.Win32Lib
{
    public class Win32
    {
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

        public static void HideWinFromAltTab(IntPtr hWnd)
        {
            int exStyle = (int)Win32Lib.Win32._GetWindowLong(hWnd, (int)Win32Lib.Win32.WindowLongFlags.GWL_EXSTYLE);

            exStyle |= (int)Win32Lib.Win32.WindowStylesEx.WS_EX_TOOLWINDOW;
            Win32Lib.Win32.SetWindowLong(hWnd, (int)Win32Lib.Win32.WindowLongFlags.GWL_EXSTYLE, (IntPtr)exStyle);
        }

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
            //0903 Elsa Add Security
            string FileInfo;
            if (!DDPMFileSecurity.IsFilePathValid(filePath, out FileInfo))
            {
                throw new ArgumentException("Invalid file path.");
            }
            return GetPrivateProfileInt(section, key, def, filePath);
        }

        //Uage:
        // //allocate string buffer, for large string you can allocate 4096 chars.
        // StringBuilder sb1=new StringBuilder(255);
        // int charsRet=GetPrivateProfileString("secName","key","defValue",sb1,sb1.Capacity,@"C:\temp\a.ini");
        // string result=sb1.ToString();
        [DllImport("kernel32", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        private static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);

        public static int _GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath)
        {
            //0903 Elsa Add Security
            string FileInfo;
            if (!DDPMFileSecurity.IsFilePathValid(filePath, out FileInfo))
            {
                throw new ArgumentException("Invalid file path.");
            }
            return GetPrivateProfileString(section, key, def, retVal, size, filePath);
        }

        #endregion Read/Write INI file

        #region Actions (Wayn)

        // Import keybd_event function
        [DllImport("user32.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        private static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, UIntPtr dwExtraInfo);

        public static void _keybd_event(byte bVk, byte bScan, uint dwFlags, UIntPtr dwExtraInfo)
        {
            keybd_event(bVk, bScan, dwFlags, dwExtraInfo);
        }

        // Import mouse_event function
        [DllImport("user32.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        private static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint dwData, UIntPtr dwExtraInfo);

        public static void _mouse_event(uint dwFlags, uint dx, uint dy, uint dwData, UIntPtr dwExtraInfo)
        {
            mouse_event(dwFlags, dx, dy, dwData, dwExtraInfo);
        }

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
    }
}