#region LicenceHeader

//
// Copyright © 2024, Dell Inc., All Rights Reserved.
// This material is confidential and a trade secret.  Permission to use this
// work for any purpose must be obtained in writing from Dell Inc.
//

#endregion

using Dell.Client.Framework.Common;
using Dell.Client.Framework.UX.Common;
using Dell.Client.Framework.UX.Common.DataModel;
using Dell.Client.Framework.UX.WPF;
using Dell.Client.Framework.UX.WPF.Console;
using Dell.Client.Framework.UX.WPF.ResourceManager;
using Microsoft;
using NGA.BaseClientCore;
using NGA.Common;
using NGA.ThickClient.Interfaces;
using System.Diagnostics;
using System.Windows;

namespace NGA.ThickClientCore
{
    /// <summary>
    /// Abstract class for common functionality of Thick client console
    /// </summary>
    public abstract class DucaThickClientCore : DucaBaseClientCore
    {
        #region private members

        private const string StylesUriString = "pack://application:,,,/Dell.UCA.ThickClientCore;component/Resources/ThickClientCoreStyles.xaml";

        private readonly ISystrayDetails? _systrayDetails;
        private SplashScreen? _splashScreen;

        private readonly bool _bFirstInstance;
        private readonly Mutex? _instanceMutex;
        private readonly string? _applicationName;

        private ILog? _log;
        private IPluginManager? _pluginManager;
        private IFormApplication? _formApplication;
        private IFormBuilderBase? _formBuilder;
        private Window? _mainWindow;

        private readonly TimeSpan _splashScreenFade = new(0, 0, 0, 0, 0);

        private bool _cleanupOnExit = true;
        private bool _disposeResources = true;

        #endregion

        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="resourceManager">Application Name</param>
        /// <param name="thickClientUniqueGuid">The unique ID for the thick client. This corresponds to the <see cref="IConsoleConfig.ConsoleUniqueGuid"/></param>
        /// <param name="systrayDetails">Details about the systray that allow the console to know about and control the systray</param>
        /// <exception cref="ArgumentNullException">This exception is Thrown if <paramref name="resourceManager"/> is null</exception>
        protected DucaThickClientCore(System.Resources.ResourceManager resourceManager, Guid thickClientUniqueGuid, ISystrayDetails? systrayDetails = null) : base(thickClientUniqueGuid)
        {
            Requires.NotNull(resourceManager, nameof(resourceManager));
            _systrayDetails = systrayDetails;
            _applicationName = resourceManager.GetString("ApplicationName");
            if (string.IsNullOrWhiteSpace(_applicationName))
            {
                throw new ArgumentException($"ApplicationName key is missing in {nameof(resourceManager)} or its value is null or whitespace");
            }

            LocalizationManager.Instance = new()
            {
                ResourceManager = resourceManager
            };

            //Ensure Thick Client has only one instance running.
            _instanceMutex = new Mutex(false, "Local\\{" + thickClientUniqueGuid + "}", out _bFirstInstance);
        }

        #endregion

        #region Virtual and abstract methods

        /// <summary>
        ///  TimeSpan for closure of Splash Screen
        /// </summary>
        protected virtual TimeSpan SplashScreenFade => _splashScreenFade;

        /// <summary>
        /// Gets Splash screen instance
        /// </summary>
        protected abstract SplashScreen? GetSplashScreen();

        /// <summary>
        /// Launch the splash screen
        /// </summary>
        protected virtual void ShowSplashScreen()
        {
            _splashScreen = GetSplashScreen();
            _splashScreen?.Show(false);
        }

        /// <summary>
        /// Closes the splash screen
        /// </summary>
        protected virtual void CloseSplashScreen()
        {
            _splashScreen?.Close(SplashScreenFade);
            _splashScreen = null;
        }

        /// <summary>
        /// Abstract method for loading Main window instance
        /// </summary>
        protected abstract Window? GetMainWindow(IFormBuilderBase formBuilder, string[]? args);

        /// <summary>
        /// Application startup method
        /// </summary>
        protected virtual void AppStartup(object sender, StartupEventArgs e)
        {
            var config = GetConsoleConfig();
#if DEBUG
            // We can optionally override the culture via the registry, for testing purposes,
            // when in debug mode.
            var productRegKey = $"SOFTWARE\\{config.CompanyName}\\{config.SuiteName}\\{config.ProductName}";
            DebugHelper.SetCulture(null, productRegKey);
#endif
            var pluginCommand = FetchPluginCommand(e);

            if (_bFirstInstance && pluginCommand == null)
                ShowSplashScreen();

            _ = Task.Factory.StartNew(async () =>
            {
                try
                {
                    await ShowThickClientConsoleAsync(pluginCommand, config);
                }
                catch (Exception ex)
                {
                    EventLogHelper.WriteEventLog($"Unhandled exception in {string.Format(NGA.Resources.Resources.MYDELL_APP_THICKCLIENT_CONSOLE_WINDOWTEXT, _applicationName)} - {ex}", EventLogEntryType.Error);
                    throw;
                }
            }, CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Current);
        }

        #endregion

        #region PublicMethods

        /// <summary>
        /// Loads the default ThickClient core library theme resources into the application
        /// </summary>
        public override ResourceManager LoadResources()
        {
            var resourceManager = base.LoadResources();

            try
            {
                var resourceDictionaries = new[] { new ResourceDictionary { Source = new Uri(StylesUriString, UriKind.RelativeOrAbsolute) } };
                resourceManager.AddCustomStyles(resourceDictionaries).StageAndCommitResources();
            }
            catch (Exception ex)
            {
                EventLogHelper.WriteEventLog(
                    $"{string.Format(NGA.Resources.Resources.MYDELL_APP_THICKCLIENT_CONSOLE_WINDOWTEXT, _applicationName)} - Unable to set the Thick Client core styles. {ex}",
                    EventLogEntryType.Error);
            }

            return resourceManager;
        }

        #endregion

        #region private methods

        private static ShowPluginCommand? FetchPluginCommand(StartupEventArgs e, ILog? log = null)
        {
            ShowPluginCommand? showPluginCommand = null;

            if (e.Args.Length > 0 && !string.IsNullOrEmpty(e.Args[0]))
                showPluginCommand = ProtocolStringParser.ParseShowPluginCommand(e.Args[0], log);

            return showPluginCommand;
        }

        private async Task ShowThickClientConsoleAsync(ShowPluginCommand? pluginCommand, IConsoleConfig config)
        {
            //If first time, creates the instance of MainWindow and show.
            if (!_bFirstInstance)
            {
                HandleInstanceAlreadyRunning(pluginCommand, config);
                return;
            }

            // Load UX theme resources only in first instance
            LoadResources();

#pragma warning disable CS0618 // Type or member is obsolete
            _formBuilder = FormBuilder.CreateDefaultConsoleBuilder(config)
                .AddService<IThickClientConsole, ThickClientConsole>()
                .AddService<ICustomWindowLayout, CustomWindowLayout>();
#pragma warning restore CS0618 // Type or member is obsolete
            if (_systrayDetails != null)
                _formBuilder.AddService(_systrayDetails);

            _formBuilder.Build();

#pragma warning disable CS0618 // Type or member is obsolete
            _formBuilder.GetSubsystem<IThickClientConsole>();
#pragma warning restore CS0618 // Type or member is obsolete

            _formApplication = _formBuilder.GetSubsystem<IFormApplication>();

            if (_formApplication == null)
            {
                EventLogHelper.WriteEventLog($"{string.Format(NGA.Resources.Resources.MYDELL_APP_THICKCLIENT_CONSOLE_WINDOWTEXT, _applicationName)} - {nameof(_formApplication)} was null", EventLogEntryType.Error);

                // We are in an error state close the application
                Dispatcher.Invoke(Shutdown);
                return;
            }

            await _formApplication.StartAsync();

            _pluginManager = _formBuilder.GetSubsystem<IPluginManager>();

            var dispatchWrapper = _formBuilder.GetSubsystem<IDispatcherWrapper>();
            if (dispatchWrapper == null)
            {
                EventLogHelper.WriteEventLog($"{string.Format(NGA.Resources.Resources.MYDELL_APP_THICKCLIENT_CONSOLE_WINDOWTEXT, _applicationName)} - {nameof(dispatchWrapper)} was null", EventLogEntryType.Error);

                // We are in an error state close the application
                Dispatcher.Invoke(Shutdown);
                return;
            }

            var logFactory = _formBuilder.GetSubsystem<ILogFactory>();
            if (logFactory != null)
            {
                _log = logFactory.CreateLogger("APP", typeof(DucaThickClientCore));
            }
            var args = ParamBuilderHelper.CreateConsoleWindowArguments(pluginCommand, _log);

            await dispatchWrapper.InvokeAsync(() =>
            {
                _mainWindow = GetMainWindow(_formBuilder, args.ToArray());
                if (_mainWindow != null)
                {
                    _mainWindow.Closed += MainWindow_Closed;
                    _mainWindow.Show();
                }
                else
                {
                    EventLogHelper.WriteEventLog($"{string.Format(NGA.Resources.Resources.MYDELL_APP_THICKCLIENT_CONSOLE_WINDOWTEXT, _applicationName)} - {nameof(_mainWindow)} was null", EventLogEntryType.Error);
                    Dispatcher.Invoke(Shutdown);
                    return;
                }

                CloseSplashScreen();
            });

            ValidateAndStartSystray();
        }

        private void MainWindow_Closed(object? sender, EventArgs e)
        {
            _log?.Info("MainWindow_Closed");

            if (!_cleanupOnExit)
                Cleanup();
        }

        private void HandleInstanceAlreadyRunning(ShowPluginCommand? command, IConsoleConfig config)
        {
            /* If already an instance is opened finding the window and bring to front */
            var windowName = string.IsNullOrEmpty(config.WindowText) ? config.HeaderText : config.WindowText;
            var hWnd = NativeMethods._FindWindow(null, windowName);

            if (hWnd == IntPtr.Zero)
            {
                var appHandles = NgaAppHelper.FindWindowsByWindowTitlesThatStartWithText(windowName).ToList();

                if (appHandles.Count == 1)
                    hWnd = appHandles.First();
            }

            if (hWnd != IntPtr.Zero)
            {
                var paramData = ParamBuilderHelper.CreatePluginData(command, _log);

                if (paramData != null)
                {
                    WindowHelper.SendMessageToWindow(hWnd, (uint)ConsoleWindow.SendMessageCommands.ShowPluginAndParameter, paramData);
                }

                WindowHelper.BringAppToFront(hWnd);
            }

            Dispatcher.Invoke(Shutdown);
        }

        /// <summary>
        /// Starts Systray if it's not running and Updates the status if Thick client has started it.
        /// </summary>
        private void ValidateAndStartSystray()
        {
            if (_systrayDetails == null || string.IsNullOrWhiteSpace(_systrayDetails.SystrayFullPath))
            {
                _log?.Info("Systray path is not provided");
                return;
            }

            try
            {
                if (_pluginManager == null)
                {
                    _log?.Error($"{nameof(ValidateAndStartSystray)} - {nameof(_pluginManager)} was null");
                    return;
                }

                _log?.Info($"Systray path is {_systrayDetails.SystrayFullPath}");

                NgaAppHelper.ValidateAndStartAppProcess(_systrayDetails.SystrayFullPath, _pluginManager, _log);
            }
            catch (ArgumentNullException ex)
            {
                const string message = $"{nameof(ValidateAndStartSystray)} - Please make sure to pass correct parameters.";
                _log?.Error(ex, message);
            }
            catch (Exception ex)
            {
                _log?.Error(ex, $"exception while starting the systray process - {ex.Message}");
            }
        }

        /// <summary>
        /// Exit event handler.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnExit(ExitEventArgs e)
        {
            try
            {
                // Clean up any splash screen hanging around
                CloseSplashScreen();
            }
            catch (ArgumentNullException ex)
            {
                const string message = $"{nameof(OnExit)} - Please make sure to pass correct parameters.";
                EventLogHelper.WriteEventLog($"{string.Format(NGA.Resources.Resources.MYDELL_APP_THICKCLIENT_CONSOLE_WINDOWTEXT, _applicationName)} - {message} {ex}", EventLogEntryType.Error);
                _log?.Error(ex, message);
            }
            catch (Exception ex)
            {
                EventLogHelper.WriteEventLog($"{string.Format(NGA.Resources.Resources.MYDELL_APP_THICKCLIENT_CONSOLE_WINDOWTEXT, _applicationName)} - exception while closing the application process {ex}", EventLogEntryType.Error);
                _log?.Error(ex, $"exception while closing the application process - {ex.Message}");
            }

            _log?.Info($"{nameof(DucaThickClientCore)} - OnExit called");
            Cleanup();

            base.OnExit(e);
        }

        /// <summary>
        ///     OnSessionEnding is called to raise the SessionEnding event. The developer will
        ///     typically override this method if they want to take action when the OS is ending
        ///     a session ( or they may choose to attach an event). This method will be called when
        ///     the user has chosen to either logoff or shutdown. These events are equivalent
        ///     to receiving a WM_QUERYSESSION window event. Windows will send it when user is
        ///     logging out/shutting down. ( See http://msdn.microsoft.com/library/default.asp?url=/library/en-us/sysinfo/base/wm_queryendsession.asp ).
        ///     By default if this event is not cancelled - Avalon will then call Application.Shutdown.
        /// </summary>
        /// <remarks>
        ///     This method follows the .Net programming guideline of having a protected virtual
        ///     method that raises an event, to provide a convenience for developers that subclass
        ///     the event.
        /// </remarks>
        /// <param name="e"></param>
        protected override void OnSessionEnding(SessionEndingCancelEventArgs e)
        {
            _log?.Info($"{nameof(DucaThickClientCore)} - OnSessionEnding called");
            _cleanupOnExit = false;
            base.OnSessionEnding(e);
        }

        /// <summary>
        /// Dispose items and stop the UXForm application
        /// </summary>
        private void Cleanup()
        {
            if (!_disposeResources)
            {
                _log?.Info($"Resources are already disposed, skipping {nameof(Cleanup)} execution");
                return;
            }

            if (_mainWindow != null)
            {
                _mainWindow.Closed -= MainWindow_Closed;
            }

            base.Dispose();

            _instanceMutex?.Dispose();

            _formApplication?.Stop();
            _formBuilder?.Dispose();

            _log?.Info("Resources are disposed");
            _disposeResources = false;
        }

        #endregion
    }
}