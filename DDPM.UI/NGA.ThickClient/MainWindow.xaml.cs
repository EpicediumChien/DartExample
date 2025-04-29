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
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using Screen = System.Windows.Forms.Screen;
using ResourceManager = Dell.Client.Framework.UX.WPF.ResourceManager.ResourceManager;
using System.Windows.Automation.Peers;

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

        //Robert_Lin 2025-2-8 DDPM support resizable, should change this flag to true
        //Robert_Lin 2024-6-19 a flag for switch Resizable MainWindow
        private bool _isMainWindowResizable = false;

        private CancellationTokenSource _ensureWindowIsVisibleDebounceCts;
        private bool _isEnsureWindowIsVisibleRunning = false;
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
            bool rst = SetProcessShutdownParameters(dwLevel, dwFlags);

            if(!rst)
            {
#if DEBUG
                Console.WriteLine("[NGA.ThickClient MainWindow] SetProcessShutdownParameters failed.");
#endif
            }

            return rst;
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

        [System.Diagnostics.CodeAnalysis.SuppressMessage("AsyncUsage.CSharp", "VSTHRD100:Avoid async void methods", Justification = "Event handler signature required")]
        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                Screen screen = Screen.FromHandle(new System.Windows.Interop.WindowInteropHelper(this).Handle);
                DdpmCommonHelper.IsMainWindowAtPrimaryScreen = screen.Primary;
                ReAdjustWindowSize();
                await EnsureWindowIsVisibleAsync(this);
            }
            catch (Exception ex)
            {
                _log?.Error($"{nameof(MainWindow)} - MainWindow_Loaded exception: {ex.Message}");
            }
        }

        private void SystemEvents_DisplaySettingsChanged(object? sender, EventArgs e)
        {
            ReAdjustWindowSize();
        }

        private void MainWindow_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            try
            {
                // When user tries to move window to another monitor using keyboard (Shift + Windows + Left or Right Arrow)
                if ((e.Key == (Key.Left) || (e.Key == (Key.Right)) &&
                    (e.KeyboardDevice.Modifiers & ModifierKeys.Shift | ModifierKeys.Windows) > 0) && Screen.AllScreens.Length > 1)
                {
                    _log?.Info($"{nameof(MainWindow)} - MainWindow_KeyUp in ...");
                    AdjustWindowSizeBasedOnMonitor();
                    RaiseEvent_MoveToNewPosition(true);
                    _log?.Info($"{nameof(MainWindow)} - MainWindow_KeyUp out ...");
                }
            }
            catch (Exception ex)
            {
                _log?.Error($"{nameof(MainWindow)} - MainWindow_KeyUp Exception: {ex.Message}");
            }
        }

        private void UXSystemParametersChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            _log?.Info($"{nameof(MainWindow)} - UXSystemParametersChanged in ...");
            try
            {
                if (e.PropertyName == nameof(UXSystemParameters.Instance.OSTheme))
                {
                    //update dark/light mode
                    DdpmCommonHelper.updateMergedDictionaries(resourceManager);
                }
                if (e.PropertyName != nameof(UXSystemParameters.Instance.HighContrast))
                    return;
                OnApplyTemplate();
                _log?.Info($"{nameof(MainWindow)} - UXSystemParametersChanged out ...");
            }
            catch (Exception ex)
            {
                _log?.Error($"{nameof(MainWindow)} - UXSystemParametersChanged Exception: {ex.Message}");
            }
        }

        /// <summary>
        ///  When multiple monitors available identifies in which monitor
        ///  window is there and calculates height and width of window
        /// </summary>
        /// <param name="isCenterOfScreen">boolean value to make window appear center of screen</param>
        private void AdjustWindowSizeBasedOnMonitor(bool isCenterOfScreen = false)
        {
            _log?.Info($"{nameof(MainWindow)} - AdjustWindowSizeBasedOnMonitor in ...");
            try
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
                _log?.Info($"{nameof(MainWindow)} - AdjustWindowSizeBasedOnMonitor out ...");
            }
            catch (Exception ex)
            {
                _log?.Error($"{nameof(MainWindow)} - AdjustWindowSizeBasedOnMonitor Exception: {ex.Message}");
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
            _log?.Info($"{nameof(MainWindow)} - AdjustWindowSize in ...");
            try
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
                _log?.Info($"{nameof(MainWindow)} - AdjustWindowSize out ...");
            }
            catch (Exception ex)
            {
                _log?.Error($"{nameof(MainWindow)} - AdjustWindowSize Exception: {ex.Message}");
            }
        }

        /// <summary>
        ///  Method to re adjust the window size
        /// </summary>
        private void ReAdjustWindowSize()
        {
            _log?.Info($"{nameof(MainWindow)} - ReAdjustWindowSize in ...");
            try
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
                _log?.Info($"{nameof(MainWindow)} - ReAdjustWindowSize out ...");
            }
            catch (Exception ex)
            {
                _log?.Error($"{nameof(MainWindow)} - ReAdjustWindowSize Exception: {ex.Message}");
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
            _log?.Info($"{nameof(MainWindow)} - MoveWindowToCenter in ...");
            try
            {
                this.Left = (screenLeft + (screenWidth - this.Width) / 2);
                this.Top = (screenTop + (screenHeight - this.Height) / 2);
                _log?.Info($"{nameof(MainWindow)} - MoveWindowToCenter out ...");
            }
            catch (Exception ex)
            {
                _log?.Error($"{nameof(MainWindow)} - MoveWindowToCenter Exception: {ex.Message}");
            }
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
            _log?.Info($"{nameof(MainWindow)} - RegisterEvents in ...");
            console.RegisterForEvent(ConsoleEventNames.MainWindow_SetToBottomWindow, SetToBottomWindow);
            console.RegisterForEvent(ConsoleEventNames.MainWindow_Activate, MainWindowActivate);

            console.RegisterForEvent("MainWindow.Hide", MainWindowHide);
            console.RegisterForEvent("MainWindow.Show", MainWindowShow);
            console.RegisterForEvent("MainWindow.Minimize", MainWindowMinimize);
            console.RegisterForEvent("MainWindow.Normal", MainWindowNormal);
            _log?.Info($"{nameof(MainWindow)} - RegisterEvents out ...");
        }

        private void SetToBottomWindow(object sender, EventManagerArgs e)
        {
            _log?.Info($"{nameof(MainWindow)} - SetToBottomWindow in ...");
            try
            {
                IntPtr hWnd = new WindowInteropHelper(this).Handle;
                _SetWindowPos(hWnd, HWND_BOTTOM, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_NOACTIVATE);
                _log?.Info($"{nameof(MainWindow)} - SetToBottomWindow out ...");
            }
            catch (Exception ex)
            {
                _log?.Error($"{nameof(MainWindow)} - SetToBottomWindow Exception: {ex.Message}");
            }
        }

        private void MainWindowActivate(object sender, EventManagerArgs e)
        {
            _log?.Info($"{nameof(MainWindow)} - MainWindowActivate in ...");
            Dispatcher.Invoke(new Action(() =>
            {
                this.Activate();
            }));
            _log?.Info($"{nameof(MainWindow)} - MainWindowActivate out ...");
        }

        private void MainWindowHide(object sender, EventManagerArgs e)
        {
            _log?.Info($"{nameof(MainWindow)} - MainWindowHide in ...");
            try
            {
                Dispatcher.Invoke(new Action(() =>
                {
                    this.Hide();
                }));
                _log?.Info($"{nameof(MainWindow)} - MainWindowHide out ...");
            }
            catch (Exception ex)
            {
                _log?.Error($"{nameof(MainWindow)} - MainWindowHide Exception: {ex.Message}");
            }
        }

        private void MainWindowShow(object sender, EventManagerArgs e)
        {
            _log?.Info($"{nameof(MainWindow)} - MainWindowShow in ...");
            try
            {
                Dispatcher.Invoke(new Action(() =>
                {
                    this.Show();
                }));
                _log?.Info($"{nameof(MainWindow)} - MainWindowShow out ...");
            }
            catch (Exception ex)
            {
                _log?.Error($"{nameof(MainWindow)} - MainWindowShow Exception: {ex.Message}");
            }
        }

        private void MainWindowMinimize(object sender, EventManagerArgs e)
        {
            _log?.Info($"{nameof(MainWindow)} - MainWindowMinimize in ...");
            try
            {
                Dispatcher.Invoke(new Action(() =>
                {
                    this.WindowState = WindowState.Minimized;
                }));
                _log?.Info($"{nameof(MainWindow)} - MainWindowMinimize out ...");
            }
            catch (Exception ex) 
            { 
                _log?.Error($"{nameof(MainWindow)} - MainWindowMinimize Exception: {ex.Message}"); 
            }
        }

        private void MainWindowNormal(object sender, EventManagerArgs e)
        {
            _log?.Info($"{nameof(MainWindow)} - MainWindowNormal in ...");
            try
            {
                Dispatcher.Invoke(new Action(() =>
                {
                    this.WindowState = WindowState.Normal;
                    this.Activate();
                }));
                _log?.Info($"{nameof(MainWindow)} - MainWindowNormal out ...");
            }
            catch (Exception ex)
            {
                _log?.Error($"{nameof(MainWindow)} - MainWindowNormal Exception: {ex.Message}");
            }
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
            {
                _log?.Info($"{nameof(MainWindow)} - MainWindow_Activate in ...");
                _Console.RaiseEvent(ConsoleEventNames.MainWindow_Activate, this, new EventManagerArgs());
                _log?.Info($"{nameof(MainWindow)} - MainWindow_Activate out ...");
            }
            _log?.Info($"{nameof(MainWindow)} - ConsoleWindow_Activated out ...");
        }

        private void ConsoleWindow_DeActivated(object sender, EventArgs e)
        {
            _log?.Info($"{nameof(MainWindow)} - ConsoleWindow_Deactivated");
            if (_Console != null)
            {
                _log?.Info($"{nameof(MainWindow)} - MainWindow_DeActivate in ...");
                _Console.RaiseEvent(ConsoleEventNames.MainWindow_DeActivate, this, new EventManagerArgs());
                _log?.Info($"{nameof(MainWindow)} - MainWindow_DeActivate out ...");
            }
            _log?.Info($"{nameof(MainWindow)} - ConsoleWindow_Deactivated out ...");
        }

        //PIMS-291471 Maximize DDPM app will cover windows taskbar
        //Below solution was provided from Dell DUCA team, Sharap Viswanathan, Karthik 2024-10-30
        //private bool _firstTimeMaximim = true;
        private void MainWIndow_StateChanged(object sender, EventArgs e)
        {
            _log?.Info($"{nameof(MainWindow)} - MainWIndow_StateChanged in ...");
            try
            {
                if (WindowState == WindowState.Maximized)
                {
                    _log?.Info($"{nameof(MainWindow)} - MainWIndow_StateChanged WindowState.Maximized in ...");
                    //Robert_Lin, 2024-12-30 Using Win32.SetWindowsPos solution to fit MainWindow to current screen.WorkingArea
                    //IntPtr hWnd = new WindowInteropHelper(this).Handle;
                    //_SetWindowPos(hWnd, HWND_TOP, (int)screen.WorkingArea.Left, (int)screen.WorkingArea.Top,
                    //     (int)screen.WorkingArea.Width, (int)screen.WorkingArea.Height, SWP_SHOWWINDOW | SWP_ASYNCWINDOWPOS);
                    SetMainWindowSizeToMaximized();
                    _log?.Info($"{nameof(MainWindow)} - MainWIndow_StateChanged WindowState.Maximized out ...");
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
                _log?.Info($"{nameof(MainWindow)} - MainWIndow_StateChanged out ...");
            }
            catch (Exception ex)
            {
                _log?.Error($"{nameof(MainWindow)} - MainWIndow_StateChanged Exception: {ex.Message}");
            }
        }

        //Robert_Lin, 2024-12-31, Win32.SetWindowPos() solution for 
        //PIMS-291471 Maximize DDPM app will cover windows taskbar
        private void SetMainWindowSizeToMaximized()
        {
            _log?.Info($"{nameof(MainWindow)} - SetMainWindowSizeToMaximized in ...");
            try
            {
                IntPtr hWnd = new WindowInteropHelper(this).Handle;
                Screen screen = Screen.FromHandle(hWnd);
                _SetWindowPos(hWnd, HWND_TOP, (int)screen.WorkingArea.Left, (int)screen.WorkingArea.Top,
                     (int)screen.WorkingArea.Width, (int)screen.WorkingArea.Height, SWP_SHOWWINDOW | SWP_ASYNCWINDOWPOS);
                _log?.Info($"{nameof(MainWindow)} - SetMainWindowSizeToMaximized out ...");
            }
            catch (Exception ex)
            {
                _log?.Error($"{nameof(MainWindow)} - SetMainWindowSizeToMaximized Exception: {ex.Message}");
            }         
        }
        #region Move to new position event
        private void RaiseEvent_MoveToNewPosition(bool isMovedByHotkey = false)
        {
            _log?.Info($"{nameof(MainWindow)} - RaiseEvent_MoveToNewPosition in ...");
            try
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
                _log?.Info($"{nameof(MainWindow)} - RaiseEvent_MoveToNewPosition out ...");
            }
            catch (Exception ex)
            {
                _log?.Error($"{nameof(MainWindow)} - RaiseEvent_MoveToNewPosition Exception: {ex.Message}");
            }
        }
        #endregion  Move to new position event

        #region Workaround solution - Robert_Lin 2024-12-03, can be removed
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

        /*private static void DoMouseClick()
        {
            _mouse_event(MOUSEEVENTF_LEFTDOWN | MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);
        }*/

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
            DdpmCommonHelper.WriteUILog($"[NGA.ThickClient MainWindow] SetWindowPos in ... ");
            try
            {
                bool rst = SetWindowPos(hWnd, hWndInsertAfter, X, Y, cx, cy, uFlags);

                if (!rst)
                {
#if DEBUG
                    Console.WriteLine("[NGA.ThickClient MainWindow] SetWindowPos failed.");
#endif
                    DdpmCommonHelper.WriteUILog("[NGA.ThickClient MainWindow] SetWindowPos failed.");
                }

                DdpmCommonHelper.WriteUILog($"[NGA.ThickClient MainWindow] SetWindowPos out ... ");
                return rst;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[NGA.ThickClient MainWindow] SetWindowPos exception: {ex.Message}");
                DdpmCommonHelper.WriteUILog($"[NGA.ThickClient MainWindow] SetWindowPos exception: {ex.Message}");
                return false;
            }
        }
        #endregion Win32

        private void ConsoleWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //DdpmCommonHelper.WriteUILog("[NGA.ThickClient] ConsoleWindow_Closing ");
            //_Console?.RaiseEvent(ConsoleEventNames.MainWindow_Force_Camera_Unlock, null, null);
            //DdpmCommonHelper.WriteUILog("[NGA.ThickClient] MainWindow_Force_Camera_Unlock event raised");
        }

        private void ConsoleWindow_Closed_1(object sender, EventArgs e)
        {
            DdpmCommonHelper.WriteUILog("[NGA.ThickClient] ConsoleWindow_Closed_1 ");
            //GC.Collect();
            //Environment.Exit(0);

            /* _log?.Info($"{nameof(MainWindow)} - ConsoleWindow_Closed");
             if (_Console != null)
                 _Console.RaiseEvent(ConsoleEventNames.MainWindow_ConsoleWindow_Closed, this, new EventManagerArgs());*/
        }

        private void AdjustWindowMinSize(object sender, MouseButtonEventArgs e)
        {
            _ = EnsureWindowIsVisibleAsync(this);
        }

        public (double Width, double Height, double WorkingWidth, double WorkingHeight) GetScreenResolution(Window window)
        {
            _log?.Info($"{nameof(MainWindow)} - GetScreenResolution in ...");
            try
            {
                // Get the top-left position of the window
                var windowPosition = new System.Drawing.Point(
                    (int)(window.Left + window.Width / 2),
                    (int)(window.Top + window.Height / 2));

                // Find the screen containing the window
                var screen = Screen.FromPoint(windowPosition);

                // Get screen resolution and working area
                var screenBounds = screen.Bounds;
                var workingArea = screen.WorkingArea;

                _log?.Info($"{nameof(MainWindow)} - GetScreenResolution out ...");

                return (
                    Width: screenBounds.Width,
                    Height: screenBounds.Height,
                    WorkingWidth: workingArea.Width,
                    WorkingHeight: workingArea.Height
                );
            }
            catch (Exception ex)
            {
                _log?.Error($"{nameof(MainWindow)} - GetScreenResolution exception: {ex.Message}");
                return (0, 0, 0, 0);
            }
        }

        private double GetScalingFactor(Window window)
        {
            _log?.Info($"{nameof(MainWindow)} - GetScalingFactor in ...");
            try
            {
                // Get the PresentationSource for the window
                var source = PresentationSource.FromVisual(window);

                if (source != null && source.CompositionTarget != null)
                {
                    // Get the matrix that represents the DPI scaling
                    var transform = source.CompositionTarget.TransformToDevice;

                    // Extract the scaling factors (X)
                    return transform.M11;
                }
                _log?.Info($"{nameof(MainWindow)} - GetScalingFactor out ...");
                // Default scaling is 1.0 (100%)
                return 1.0;
            }
            catch (Exception ex)
            {
                _log?.Error($"{nameof(MainWindow)} - GetScalingFactor exception: {ex.Message}");
                return 1.0;
            }
        }

        private void AdjustWindowPosition(Window window, Screen screen, double factor = 1.0)
        {
            _log?.Info($"{nameof(MainWindow)} - AdjustWindowPosition in ...");
            try
            {
                // Get screen working area
                var screenWorkingArea = screen.WorkingArea;

                double adjustedLeft = window.Left * factor;
                // Adjust window position if it goes out of the working area
                // Adjust X
                if (screenWorkingArea.Right < (window.Left + window.Width) * factor)
                {
                    adjustedLeft = screenWorkingArea.Right - window.Width * factor;
                }
                else if (screenWorkingArea.Left > window.Left * factor)
                {
                    //adjustedLeft = screenWorkingArea.Left;
                    // To top right
                    adjustedLeft = screenWorkingArea.Right - window.Width * factor;
                }

                // Adjust Y
                double adjustedTop = window.Top * factor;
                if (screenWorkingArea.Top > window.Top * factor)
                {
                    adjustedTop = screenWorkingArea.Top;
                }
                else if (screenWorkingArea.Bottom < (window.Top + window.Height) * factor)
                {
                    //adjustedTop = screenWorkingArea.Bottom - window.Height * factor;
                    // To top right
                    adjustedTop = screenWorkingArea.Top;
                }

                // Apply the adjusted position
                window.Left = adjustedLeft / factor;
                window.Top = adjustedTop / factor;

                _log?.Info($"{nameof(MainWindow)} - AdjustWindowPosition out ...");
            }
            catch (Exception ex)
            {
                _log?.Error($"{nameof(MainWindow)} - AdjustWindowPosition exception: {ex.Message}");
            }
        }

        private async Task EnsureWindowIsVisibleAsync(Window window)
        {
            _log?.Info($"{nameof(MainWindow)} - EnsureWindowIsVisible in ...");
            try
            {
                // first check, if EnsureWindowIsVisible is running
                if (_isEnsureWindowIsVisibleRunning)
                {
                    _log?.Info($"{nameof(MainWindow)} - EnsureWindowIsVisible _isEnsureWindowIsVisibleRunning, true.");
                    return;
                }
                _isEnsureWindowIsVisibleRunning = true;

                // second check, EnsureWindowIsVisible Debounce
                if (_ensureWindowIsVisibleDebounceCts != null)
                {
                    await _ensureWindowIsVisibleDebounceCts.CancelAsync();
                }
                _ensureWindowIsVisibleDebounceCts = new CancellationTokenSource();
                var token = _ensureWindowIsVisibleDebounceCts.Token;
                try
                {
                    await Task.Delay(500, token);
                    _log?.Info($"{nameof(MainWindow)} - after Task.Delay");
                }
                catch (TaskCanceledException)
                {
                    _log?.Info($"{nameof(MainWindow)} - EnsureWindowIsVisible TaskCanceledException executed.");
                    return;
                }

                //Derek 10/26
                Int16 width = (Int16?)System.Windows.Application.Current?.TryFindResource("breakPoint") ?? 0;
                Int16 height = (Int16?)System.Windows.Application.Current?.TryFindResource("minHeight") ?? 0;

                // Get the PresentationSource for the window
                double factor = GetScalingFactor(window);

                // Get the window's current position
                var windowTopLeft = new System.Drawing.Point(
                    (int)window.Left,
                    (int)window.Top);

                var (actualWidth, actualHeight, workingWidth, workingHeight) = GetScreenResolution(this);

                Debug.WriteLine($"Screen Resolution: {actualWidth / factor}x{actualHeight / factor}\n" +
                                $"Working Area: {workingWidth}x{workingHeight}");
                DdpmCommonHelper.WriteUILog($"Screen Resolution: {actualWidth / factor}x{actualHeight / factor}\n" +
                                            $"Working Area: {workingWidth}x{workingHeight}");
                workingWidth = workingWidth / factor;
                workingHeight = workingHeight / factor;

                // Get the window's position and size
                var windowRect = new System.Drawing.Rectangle(
                    (int)window.Left,
                    (int)window.Top,
                    (int)window.Width,
                    (int)window.Height);
                // Find the screen containing the window
                var screen = Screen.FromHandle(new System.Windows.Interop.WindowInteropHelper(this).Handle); // Default to primary screen

                var workingArea = screen.WorkingArea;

                bool isOutOfBounds = window.Left + window.Width - 200 < workingArea.Left / factor
                        || window.Top + 64 < workingArea.Top / factor
                        || window.Left + window.Width > workingArea.Right / factor
                        || window.Top + 64 > workingArea.Bottom / factor;

                this.MinWidth = width;
                this.MinHeight = height;

                // Resize the window

                if (width > workingWidth)
                {
                    this.MinWidth = workingWidth;
                    this.Width = workingWidth;
                    isOutOfBounds = true;
                }

                if (height > workingHeight)
                {
                    this.MinHeight = workingHeight;
                    this.Height = workingHeight;
                    isOutOfBounds = true;
                }

                if (isOutOfBounds)
                {
                    AdjustWindowPosition(window, screen, factor);
                }
                _log?.Info($"{nameof(MainWindow)} - EnsureWindowIsVisible out ...");
            }
            catch (Exception ex)
            {
                _log?.Error($"{nameof(MainWindow)} - EnsureWindowIsVisible exception: {ex.Message}");
            }
            finally
            {
                _isEnsureWindowIsVisibleRunning = false;
                _log?.Info($"{nameof(MainWindow)} - EnsureWindowIsVisible finished.");
            }
        }

        #region Narrator
        /// <summary>
        /// Notify Narrator to recalculate the UI coordinates.
        /// It's called when the main window move to another screen.
        /// This method is designed for MainWindow only. If you would like use it from
        /// other UI elements (Page, UserCOntrol,...), please remove the comments of
        /// 'Find the main window' section in this method. And change 'this' to 'mainWindow'
        /// </summary>
        private void NotifyNarratorToRecalculateUI()
        {
            //// Find the main window
            //var mainWindow = Application.Current.MainWindow;
            //if (mainWindow == null) return;

            // Get the AutomationPeer for the main window
            var peer = UIElementAutomationPeer.FromElement(this) ?? UIElementAutomationPeer.CreatePeerForElement(this);
            if (peer == null) return;

            // Raise the AutomationPropertyChangedEvent to notify Narrator
            peer.RaiseAutomationEvent(AutomationEvents.LiveRegionChanged);
        }        
        #endregion Narrator

    }
}