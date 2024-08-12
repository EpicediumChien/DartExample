#region LicenceHeader
//
// Copyright © 2022, Dell Inc., All Rights Reserved.
// This material is confidential and a trade secret.  Permission to use this
// work for any purpose must be obtained in writing from Dell Inc.
//
#endregion

using System;
using Dell.Client.Framework.Common;
using Dell.Client.Framework.UX.WPF;
using Dell.Client.Framework.UX.WPF.Console;
using Dell.Client.Framework.UX.WPF.ResourceManager;
using NGA.Systray.Interfaces;

namespace NGA.Systray
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
                _log = logCreator.CreateLogger("SYSTRAY", typeof(MainWindow));

            _log?.Trace($"{nameof(ConsoleWindow)} - Constructor Enter");

            /*
             * Use the ISystrayConsole so it gets loaded and used by something
             * If not we would depend on a plugin using this to kick off the constructor
             */
            var systray = formBuilder.GetSubsystem<ISystrayConsole>();
            if (systray == null)
            {
                _log?.Warning($"{nameof(MainWindow)} - {nameof(systray)} was null");
            }

            InitializeComponent();

            UxLocalizationManager.Instance = new()
            {
                ResourceManager = NGA.Resources.Resources.ResourceManager
            };

            DataContext = this;

            _log?.Trace($"{nameof(ConsoleWindow)} - Constructor Exit");
        }

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
                case WM_QUERYENDSESSION: // Temporary fix: base class sets handled to true
                    break;
                default:
                    base.WndProc(hwnd, msg, wParam, lParam, ref handled);
                    break;
            }

            return IntPtr.Zero;
        }
    }
}
