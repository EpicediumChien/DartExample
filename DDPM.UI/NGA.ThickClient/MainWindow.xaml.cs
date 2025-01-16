#region LicenceHeader

//
// Copyright © 2022, Dell Inc., All Rights Reserved.
// This material is confidential and a trade secret.  Permission to use this
// work for any purpose must be obtained in writing from Dell Inc.
//

#endregion

using DDPM.UI.Common;
using Dell.Client.Framework.Common;
using Dell.Client.Framework.UX.WPF;
using Dell.Client.Framework.UX.WPF.Console;
using Dell.Client.Framework.UX.WPF.Controls;
using Dell.Client.Framework.UX.WPF.ResourceManager;
using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using Screen = System.Windows.Forms.Screen;
using ResourceManager = Dell.Client.Framework.UX.WPF.ResourceManager.ResourceManager;
using System.Reflection.Metadata;
using System.Windows.Forms;

namespace NGA.ThickClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : ConsoleWindow
    {
        /// <summary>
        /// Log object specific to MainWindow
        /// </summary>
        private readonly ILog? _log;

        private readonly IConsole? _Console;

        /// <summary>
        ///  75% of the height of the usable area
        /// </summary>
        private const double UsableHeightPercentage = 0.75;

        /// <summary>
        ///  75% of the width of the usable area
        /// </summary>
        private const double UsableWidthPercentage = 0.75;

        //Robert_Lin 2024-6-19 a flag for switch Resizable MainWindow
        private bool _isMainWindowResizable = false;

        /// <summary>
        ///  Ratio of Default size of the window with Height = 782 and Width = 1132
        ///  Used in calculating the height and width when readjusting the window
        /// </summary>
        private readonly double HeightWidthRatio;

        private readonly double WindowHeight = 735;
        private readonly double WindowWidth = 1378;

        private const int WM_EXITSIZEMOVE = 0x0232;
        private const int WM_QUERYENDSESSION = 0x11;
        private readonly ResourceManager resourceManager;

        private const int WM_EXITBYMYSELF = 0xFF30;

        [DllImport("kernel32.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        private static extern bool SetProcessShutdownParameters(uint dwLevel, uint dwFlags);
        private static bool _SetProcessShutdownParameters(uint dwLevel, uint dwFlags)
        {
            return SetProcessShutdownParameters(dwLevel, dwFlags);
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="formBuilder"></param>
        /// <param name="args"></param>
        public MainWindow(IFormBuilderBase formBuilder, string[]? args = null) : base(formBuilder, args)
        {
            var logCreator = formBuilder.GetSubsystem<ILogFactory>();

            if (logCreator != null)
            {
                _log = logCreator.CreateLogger("MAINWIN", typeof(MainWindow));
                _log.Info("DDPM MainWindow ctor");
            }

            InitializeComponent();
            DataContext = this;
            SubscribeMainWindowEvents();

            if (System.Windows.Application.Current?.TryFindResource("DefaultWindowHeight") is double height)
                WindowHeight = height;

            if (System.Windows.Application.Current?.TryFindResource("DefaultWindowWidth") is double width)
                WindowWidth = width;

            UxLocalizationManager.Instance = new()
            {
                ResourceManager = NGA.Resources.Resources.ResourceManager
            };

            HeightWidthRatio = WindowHeight / WindowWidth;

            //2024-6-19 Robert_Lin, to show Maximize button on main window titlebar
            IWindowLayout? windowLayout = formBuilder.GetSubsystem<IWindowLayout>();
            if (windowLayout != null &&
                windowLayout.Masthead != null)
            {
                UXMasthead masthead = windowLayout.Masthead;
                masthead.MaximizeButtonVisible = true;
                masthead.MaximizeButtonEnabled = true;

                //Robert_Lin, 2024-6-26 remove dell logo from left of titlebar
                masthead.IconVisible = false;                
            }

            IConsole? console = formBuilder.GetSubsystem<IConsole>();
            if (console != null)
            {
                RegisterEvents(console);
            }
            _log?.Info($"{nameof(MainWindow)} - Constructed");
            resourceManager = new ResourceManager();
            _Console = console;

            //Make sure UI shutdown earlier than SA
            _SetProcessShutdownParameters(0x4FF, 0);
        }

        #region Private Methods

        /// <summary>
        ///  Events subscribed in MainWindow
        /// </summary>
        private void SubscribeMainWindowEvents()
        {
            this.Loaded += MainWindow_Loaded;
            this.KeyUp += MainWindow_KeyUp;
            SystemEvents.DisplaySettingsChanged += SystemEvents_DisplaySettingsChanged;
            UXSystemParameters.Instance.ParameterChangedEvent += UXSystemParametersChanged;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Screen screen = Screen.FromHandle(new System.Windows.Interop.WindowInteropHelper(this).Handle);
            DdpmCommonHelper.IsMainWindowAtPrimaryScreen = screen.Primary;
            ReAdjustWindowSize();

            //Derek 10/26
            if (System.Windows.Application.Current?.TryFindResource("breakPoint") is Int16 width)
                this.MinWidth = width;

            if (System.Windows.Application.Current?.TryFindResource("minHeight") is Int16 height)
                this.MinHeight = height;
        }

        private void SystemEvents_DisplaySettingsChanged(object? sender, EventArgs e)
        {
            ReAdjustWindowSize();
        }

        private void MainWindow_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            // When user tries to move window to another monitor using keyboard (Shift + Windows + Left or Right Arrow)
            if ((e.Key == (Key.Left) || (e.Key == (Key.Right)) &&
                (e.KeyboardDevice.Modifiers & ModifierKeys.Shift | ModifierKeys.Windows) > 0) && Screen.AllScreens.Length > 1)
            {
                AdjustWindowSizeBasedOnMonitor();
                RaiseEvent_MoveToNewPosition(true);
            }
        }

        private void UXSystemParametersChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {

            if (e.PropertyName == nameof(UXSystemParameters.Instance.OSTheme))
            {
                //update dark/light mode
                DdpmCommonHelper.updateMergedDictionaries(resourceManager);
            }
            if (e.PropertyName != nameof(UXSystemParameters.Instance.HighContrast))
                return;
            OnApplyTemplate();
        }

        /// <summary>
        ///  When multiple monitors available identifies in which monitor
        ///  window is there and calculates height and width of window
        /// </summary>
        /// <param name="isCenterOfScreen">boolean value to make window appear center of screen</param>
        private void AdjustWindowSizeBasedOnMonitor(bool isCenterOfScreen = false)
        {
            if (WindowState == WindowState.Maximized)
            {
                SetMainWindowSizeToMaximized();
                return;
            }
            Screen screen = Screen.FromHandle(new System.Windows.Interop.WindowInteropHelper(this).Handle);
            DdpmCommonHelper.IsMainWindowAtPrimaryScreen = screen.Primary;

            //2024-5-8 Robert_Lin, to support resizeable MainWindow,
            //Sharap Viswanathan, Karthik suggest to comment out the method
            if (_isMainWindowResizable)
            {
                double screenHeight;
                double screenWidth;
                //Screen screen = Screen.FromHandle(new System.Windows.Interop.WindowInteropHelper(this).Handle);
                // PrimaryScreen Scaling info required because screen workarea when app launched in Secondary montior
                // gives resolution of Secondary monitor by multiplying the PrimaryScreenScaling ratio
                var primaryScreenScalingRatio = Screen.PrimaryScreen.Bounds.Width / SystemParameters.PrimaryScreenWidth;

                if (screen.Primary)
                {
                    screenHeight = SystemParameters.WorkArea.Height;
                    screenWidth = SystemParameters.WorkArea.Width;
                }
                else
                {
                    screenHeight = screen.WorkingArea.Height / primaryScreenScalingRatio;
                    screenWidth = screen.WorkingArea.Width / primaryScreenScalingRatio;
                }

                AdjustWindowSize(screenHeight, screenWidth);
                if (isCenterOfScreen)
                {
                    double screenLeft = screen.Primary ? SystemParameters.WorkArea.Left : (screen.WorkingArea.Left / primaryScreenScalingRatio);
                    double screenTop = screen.Primary ? SystemParameters.WorkArea.Top : (screen.WorkingArea.Top / primaryScreenScalingRatio);
                    MoveWindowToCenter(screenLeft, screenTop, screenWidth, screenHeight);
                }
            }

        }

        /// <summary>
        ///  Shrinks the app window down so that it's height is
        ///  never more than 75% of the height and width of the usable area.
        ///  Even in cases where when the app window height would fit,
        ///  if the fit is greater than 75%, it shrinks the app window down.
        /// </summary>
        /// <param name="screenHeight">Usable screenHeight</param>
        /// <param name="screenWidth">Usable screenWidth</param>
        private void AdjustWindowSize(double screenHeight, double screenWidth)
        {
             //2024-5-8 Robert_Lin, to support resizeable MainWindow,
            //Sharap Viswanathan, Karthik suggest to comment out the method
            if (_isMainWindowResizable)
            {
                screenHeight *= UsableHeightPercentage;
                screenWidth *= UsableWidthPercentage;
                if (screenHeight > 0 && screenHeight < WindowHeight)
                {
                    MinHeight = screenHeight;
                    Height = screenHeight;
                    var calculatedWidth = screenHeight / HeightWidthRatio;
                    if (calculatedWidth > screenWidth)
                        calculatedWidth = screenWidth;
                    MinWidth = calculatedWidth;
                    Width = calculatedWidth;
                }
                else if (screenWidth > 0 && screenWidth < WindowWidth)
                {
                    MinWidth = screenWidth;
                    Width = screenWidth;
                    var calculatedHeight = screenWidth * HeightWidthRatio;
                    if (calculatedHeight > screenHeight)
                        calculatedHeight = screenHeight;
                    MinHeight = calculatedHeight;
                    Height = calculatedHeight;
                }
                else
                {
                    MinHeight = WindowHeight;
                    Height = WindowHeight;
                    MinWidth = WindowWidth;
                    Width = WindowWidth;
                }
            }
        }

        /// <summary>
        ///  Method to re adjust the window size
        /// </summary>
        private void ReAdjustWindowSize()
        {
            //2024-5-8 Robert_Lin, to support resizeable MainWindow,
            //Sharap Viswanathan, Karthik suggest to comment out the method
            if (_isMainWindowResizable)
            {
                if (Screen.AllScreens.Length > 1)
                    AdjustWindowSizeBasedOnMonitor(true);
                else
                {
                    AdjustWindowSize(SystemParameters.WorkArea.Height, SystemParameters.WorkArea.Width);
                    MoveWindowToCenter(SystemParameters.WorkArea.Left, SystemParameters.WorkArea.Top, SystemParameters.WorkArea.Width, SystemParameters.WorkArea.Height);
                }
            }
        }

        /// <summary>
        ///   Moves Window to the center of screen
        /// </summary>
        /// <param name="screenLeft">screen left</param>
        /// <param name="screenTop">screen top</param>
        /// <param name="screenWidth">screen width</param>
        /// <param name="screenHeight">screen height</param>
        private void MoveWindowToCenter(double screenLeft, double screenTop, double screenWidth, double screenHeight)
        {
            this.Left = (screenLeft + (screenWidth - this.Width) / 2);
            this.Top = (screenTop + (screenHeight - this.Height) / 2);
        }

        #endregion

        #region Protected

        /// <summary>
        ///  Represents the method that handles Win32 window messages.
        /// </summary>
        /// <param name="hwnd">The window handle.</param>
        /// <param name="msg">The message ID.</param>
        /// <param name="wParam">The message's wParam value.</param>
        /// <param name="lParam">The message's lParam value.</param>
        /// <param name="handled">A value that indicates whether the message was handled.</param>
        /// <returns></returns>
        protected override IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            switch (msg)
            {
                case WM_EXITSIZEMOVE: // Occurs when Dragging of Window using mouse is completed
                                      //Robert_Lin, 2024-5-7 remark it for supporting resize Window
                    if (Screen.AllScreens.Length > 1)
                    {
                        AdjustWindowSizeBasedOnMonitor();
                        RaiseEvent_MoveToNewPosition();
                    }
                    break;

                case WM_QUERYENDSESSION: // Temporary fix: base class sets handled to true
                    break;

                case WM_EXITBYMYSELF:
                    this.Close();
                    break;

                default:
                    base.WndProc(hwnd, msg, wParam, lParam, ref handled);
                    break;
            }

            return IntPtr.Zero;
        }

        #endregion

        #region MainWindow State Event Handlers

        private void RegisterEvents(IConsole console)
        {
            console.RegisterForEvent(ConsoleEventNames.MainWindow_SetToBottomWindow, SetToBottomWindow);
            console.RegisterForEvent(ConsoleEventNames.MainWindow_Activate, MainWindowActivate);

            console.RegisterForEvent("MainWindow.Hide", MainWindowHide);
            console.RegisterForEvent("MainWindow.Show", MainWindowShow);
            console.RegisterForEvent("MainWindow.Minimize", MainWindowMinimize);
            console.RegisterForEvent("MainWindow.Normal", MainWindowNormal);
        }

        private void SetToBottomWindow(object sender, EventManagerArgs e)
        {
            IntPtr hWnd = new WindowInteropHelper(this).Handle;
            _SetWindowPos(hWnd, HWND_BOTTOM, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_NOACTIVATE);
        }

        private void MainWindowActivate(object sender, EventManagerArgs e)
        {
            Dispatcher.Invoke(new Action(() =>
            {
                this.Activate();
            }));
        }

        private void MainWindowHide(object sender, EventManagerArgs e)
        {
            Dispatcher.Invoke(new Action(() =>
            {
                this.Hide();
            }));
        }

        private void MainWindowShow(object sender, EventManagerArgs e)
        {
            Dispatcher.Invoke(new Action(() =>
            {
                this.Show();
            }));
        }

        private void MainWindowMinimize(object sender, EventManagerArgs e)
        {
            Dispatcher.Invoke(new Action(() =>
            {
                this.WindowState = WindowState.Minimized;
            }));
        }

        private void MainWindowNormal(object sender, EventManagerArgs e)
        {
            Dispatcher.Invoke(new Action(() =>
            {
                this.WindowState = WindowState.Normal;
                this.Activate();
            }));
        }

        #endregion

        private void ConsoleWindow_Closed(object sender, EventArgs e)
        {
            _log?.Info($"{nameof(MainWindow)} - ConsoleWindow_Closed");
        }

        private void ConsoleWindow_Activated(object sender, EventArgs e)
        {
            _log?.Info($"{nameof(MainWindow)} - ConsoleWindow_Activated");
            if (_Console != null)
                _Console.RaiseEvent(ConsoleEventNames.MainWindow_Activate, this, new EventManagerArgs());
        }

        private void ConsoleWindow_DeActivated(object sender, EventArgs e)
        {
            _log?.Info($"{nameof(MainWindow)} - ConsoleWindow_Deactivated");
            if (_Console != null)
                _Console.RaiseEvent(ConsoleEventNames.MainWindow_DeActivate, this, new EventManagerArgs());
        }

        //PIMS-291471 Maximize DDPM app will cover windows taskbar
        //Below solution was provided from Dell DUCA team, Sharap Viswanathan, Karthik 2024-10-30
        //private bool _firstTimeMaximim = true;
        private void MainWIndow_StateChanged(object sender, EventArgs e)
        {
            if (WindowState == WindowState.Maximized)
            {
                //Robert_Lin, 2024-12-30 Using Win32.SetWindowsPos solution to fit MainWindow to current screen.WorkingArea
                //IntPtr hWnd = new WindowInteropHelper(this).Handle;
                //_SetWindowPos(hWnd, HWND_TOP, (int)screen.WorkingArea.Left, (int)screen.WorkingArea.Top,
                //     (int)screen.WorkingArea.Width, (int)screen.WorkingArea.Height, SWP_SHOWWINDOW | SWP_ASYNCWINDOWPOS);
                SetMainWindowSizeToMaximized();

                //Screen screen = Screen.FromHandle(new System.Windows.Interop.WindowInteropHelper(this).Handle);
                // PrimaryScreen Scaling info required because screen workarea when app launched in Secondary montior
                // gives resolution of Secondary monitor by multiplying the PrimaryScreenScaling ratio
                //if (Screen.PrimaryScreen == null) return;
                /*
                // this logic is required for secondary monitor scaling ratio calculation
                var primaryScreenScalingRatio = Screen.PrimaryScreen.Bounds.Width / SystemParameters.PrimaryScreenWidth;
                double screenHeight;
                double screenWidth;
                if (screen.Primary)
                {
                    screenHeight = SystemParameters.WorkArea.Height;
                    screenWidth = SystemParameters.WorkArea.Width;
                }
                else
                {
                    screenHeight = screen.WorkingArea.Height / primaryScreenScalingRatio;
                    screenWidth = screen.WorkingArea.Width / primaryScreenScalingRatio;
                }

                this.MaxWidth = screenWidth;
                this.MaxHeight = screenHeight;
                */


                //RefreshWindowTaskbar();

                //Robert_Lin, 2024-12-2 workaround, I found the firstime maximized will also has a
                //glass-effect on taskbar. so force it restore to normal then maximized again.
                //if (_firstTimeMaximim)
                //{
                //    _firstTimeMaximim = false;
                //    WindowState = WindowState.Normal;
                //    WindowState = WindowState.Maximized;
                //}

                //int x = screen.WorkingArea.Left + (int)screenWidth / 2;
                //int y = screen.WorkingArea.Top + (int)screenHeight / 2;
                //SetCursorPos(x, y);
                //DoMouseClick();
            }
        }

        //Robert_Lin, 2024-12-31, Win32.SetWindowPos() solution for 
        //PIMS-291471 Maximize DDPM app will cover windows taskbar
        private void SetMainWindowSizeToMaximized()
        {
            IntPtr hWnd = new WindowInteropHelper(this).Handle;
            Screen screen = Screen.FromHandle(hWnd);
            _SetWindowPos(hWnd, HWND_TOP, (int)screen.WorkingArea.Left, (int)screen.WorkingArea.Top,
                 (int)screen.WorkingArea.Width, (int)screen.WorkingArea.Height, SWP_SHOWWINDOW | SWP_ASYNCWINDOWPOS);
        }
        #region Move to new position event
        private void RaiseEvent_MoveToNewPosition(bool isMovedByHotkey = false)
        {
            //Robert_Lin, 2024-12-20 To show ProductName OSD on the target screen
            if (_Console != null)
            {
                EventManagerArgs args = new EventManagerArgs();
                if (isMovedByHotkey)
                {
                    Screen screen = Screen.FromHandle(new System.Windows.Interop.WindowInteropHelper(this).Handle);
                    args.Tag = screen.DeviceName;
                }
                _Console.RaiseEvent(ConsoleEventNames.MainWindow_MoveToNewPosition, this, args);
            }
        }
        #endregion  Move to new position event

        #region Workaround solution - Robert_Lin 2024-12-03, can be removed
        /*private void RefreshWindowTaskbar()
        {
            const int HWND_BROADCAST = 0xffff;
            const uint WM_SETTINGCHANGE = 0x001A;
            bool result = PostMessage((IntPtr)HWND_BROADCAST, WM_SETTINGCHANGE, IntPtr.Zero, IntPtr.Zero);
        }*/
        /*[DllImport("user32.dll")]
        private static extern int SendMessage(IntPtr hWnd, int wMsg, IntPtr wParam, IntPtr lParam);*/

        /*[DllImport("user32.dll")]
        static extern bool PostMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);*/


        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        private static extern void mouse_event(long dwFlags, long dx, long dy, long cButtons, long dwExtraInfo);
        private static void _mouse_event(long dwFlags, long dx, long dy, long cButtons, long dwExtraInfo)
        {
            mouse_event(dwFlags, dx, dy, cButtons, dwExtraInfo);
        }

        private const int MOUSEEVENTF_LEFTDOWN = 0x02;
        private const int MOUSEEVENTF_LEFTUP = 0x04;
        private const int MOUSEEVENTF_RIGHTDOWN = 0x08;
        private const int MOUSEEVENTF_RIGHTUP = 0x10;

        private static void DoMouseClick()
        {
            _mouse_event(MOUSEEVENTF_LEFTDOWN | MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);
        }

        /*[DllImport("user32.dll")]
        static extern bool SetCursorPos(int X, int Y);

        private static void MoveCursorToPoint(int x, int y)
        {
            SetCursorPos(x, y);
        }*/

        //public const short SWP_NOMOVE = 0X2;
        //public const short SWP_NOSIZE = 1;
        //public const short SWP_NOZORDER = 0X4;
        //public const int SWP_SHOWWINDOW = 0x0040;

        //[DllImport("user32.dll", EntryPoint = "SetWindowPos", SetLastError = true)]
        //[DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        //private static extern IntPtr SetWindowPos(IntPtr hWnd, int hWndInsertAfter, int x, int Y, int cx, int cy, int wFlags);

        //public static IntPtr _SetWindowPos(IntPtr hWnd, int hWndInsertAfter, int x, int Y, int cx, int cy, int wFlags)
        //{
        //    return SetWindowPos(hWnd, hWndInsertAfter, x, Y, cx, cy, wFlags);
        //}

        #endregion


        #region Win32
        const int HWND_TOP = 0;
        private static readonly IntPtr HWND_BOTTOM = new IntPtr(1);

        private const UInt32 SWP_NOSIZE = 0x0001;
        private const UInt32 SWP_NOMOVE = 0x0002;
        private const UInt32 SWP_NOACTIVATE = 0x0010;
        private const UInt32 SWP_SHOWWINDOW = 0x0040;
        private const UInt32 SWP_ASYNCWINDOWPOS = 0x4000;

        //SetWindowPos()
        [DllImport("user32.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);
        private static bool _SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags)
        {
            return SetWindowPos(hWnd, hWndInsertAfter, X, Y, cx, cy, uFlags);
        }
        #endregion Win32

        private void ConsoleWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
        }

        private void ConsoleWindow_Closed_1(object sender, EventArgs e)
        {

            _log?.Info($"{nameof(MainWindow)} - ConsoleWindow_Closed");
            if (_Console != null)
                _Console.RaiseEvent(ConsoleEventNames.MainWindow_ConsoleWindow_Closed, this, new EventManagerArgs());

        }
    }
}