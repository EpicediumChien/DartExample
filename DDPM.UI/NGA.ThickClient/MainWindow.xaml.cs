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
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using Screen = System.Windows.Forms.Screen;

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

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="formBuilder"></param>
        /// <param name="args"></param>
        public MainWindow(IFormBuilderBase formBuilder, string[]? args = null) : base(formBuilder, args)
        {
            var logCreator = formBuilder.GetSubsystem<ILogFactory>();

            if (logCreator != null)
                _log = logCreator.CreateLogger("MAINWIN", typeof(MainWindow));

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
            if (windowLayout != null)
            {
                if (windowLayout.Masthead != null)
                {
                    UXMasthead masthead = windowLayout.Masthead;
                    masthead.MaximizeButtonVisible = true;
                    masthead.MaximizeButtonEnabled = true;

                    //Robert_Lin, 2024-6-26 remove dell logo from left of titlebar
                    masthead.IconVisible = false;
                }
            }

            IConsole? console = formBuilder.GetSubsystem<IConsole>();
            if (console != null)
            {
                RegisterEvents(console);
            }
            _log?.Info($"{nameof(MainWindow)} - Constructed");
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
            }
        }

        private void UXSystemParametersChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
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
                        AdjustWindowSizeBasedOnMonitor();
                    break;

                case WM_QUERYENDSESSION: // Temporary fix: base class sets handled to true
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
            console.RegisterForEvent("MainWindow.SetToBottomWindow", SetToBottomWindow);
            console.RegisterForEvent("MainWindow.Activate", MainWindowActivate);
            console.RegisterForEvent("MainWindow.Hide", MainWindowHide);
            console.RegisterForEvent("MainWindow.Show", MainWindowShow);
            console.RegisterForEvent("MainWindow.Minimize", MainWindowMinimize);
            console.RegisterForEvent("MainWindow.Normal", MainWindowNormal);
        }

        private void SetToBottomWindow(object sender, EventManagerArgs e)
        {
            IntPtr hWnd = new WindowInteropHelper(this).Handle;
            SetWindowPos(hWnd, HWND_BOTTOM, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_NOACTIVATE);
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

        [DllImport("user32.dll")]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        private static readonly IntPtr HWND_BOTTOM = new IntPtr(1);
        private const UInt32 SWP_NOSIZE = 0x0001;
        private const UInt32 SWP_NOMOVE = 0x0002;
        private const UInt32 SWP_NOACTIVATE = 0x0010;

        #endregion
    }
}