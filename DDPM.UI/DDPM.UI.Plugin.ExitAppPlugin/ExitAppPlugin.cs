
using Dell.Client.Framework.Common.Annotations;
using Dell.Client.Framework.Common;
using System.Diagnostics.CodeAnalysis;
using Dell.Client.Framework.UX.WPF;
using CommunityToolkit.Mvvm.DependencyInjection;
using NGA.ThickClient.Interfaces;
using DDPM.UI.Plugin.ExitAppPlugin.Views;
using System.Windows.Input;
using DDPM.UI.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace DDPM.UI.Plugin.ExitAppPlugin
{
    /// <summary>
    /// A ConsolePage plugin will show error message and then exit DDPM when user click "Close" button
    /// </summary>
    [Plugin(PluginId, PluginName, Version = PluginVersion, Category = Category.Utility)]
    [Descriptor(Description = TileDetailText)]
    [Publisher(Name = "DDPM BootloaderPlugin", Support = "Wistron DDPM Team")]
    [ExcludeFromCodeCoverage]
    public class ExitAppPlugin : IConsolePagePlugin, IConsolePluginSupportsActivations, IThickClientPlugin
    {
        #region Private members
        //My Plugin constants
        private const string PluginId = DDPM.UI.Common.Constants.ExitAppPluginId;
        private const string PluginName = "DDPM.UI.Plugin.ExitAppPlugin";
        private const string PluginVersion = "1.0";
        private const string TileDetailText = "DDPM.UI.Plugin.ExitAppPlugin";

        //Static data
        internal static readonly Ioc PluginIoc = new();

        //DCF related members
        private readonly IWindowLayout _windowLayout;
        private readonly ILog _log;
        private readonly IConsole _console;
        IDispatcherWrapper _dispatcherWrapper;
        private readonly IPluginManager _pluginManager;

        //Plugin flags
        private bool _isConfigured;
        private bool _isActivated = false;

        #endregion  Private members

        #region IConsolePagePlugin Implementation
        public string HeaderText => "Exit App";
        public Type PageType => typeof(ExitAppView);
        #endregion IConsolePagePlugin Implementation

        #region ctor
        public ExitAppPlugin(IWindowLayout windowLayout, IConsole console, IDispatcherWrapper dispatcherWrapper, IPluginManager pluginManager)
        {
            _windowLayout = windowLayout;
            _console = console;
            _dispatcherWrapper = dispatcherWrapper;
            _pluginManager = pluginManager;

            //Create log for my plugin
            _log = console.CreateLog("ExitApp");
            _log.Info($"{nameof(ExitAppPlugin)} - Constructed");

            _pluginManager.PluginsStarted += PluginManager_PluginsStarted;

        }
        #endregion ctor

        #region PluginManager Related
        private void PluginManager_PluginsStarted(object? sender, PluginsStartedEventArgs pluginsStartedEventArgs)
        {
            _log.Info($"{nameof(PluginManager_PluginsStarted)} started");
            if (_pluginManager != null)
            {
                try
                {
                    //TO DO: Initialize DDPM.SA Plugins 
                    //InitializeDeviceManagerPlugin();
                }
                catch (Exception e1)
                {
                    var message = $"{nameof(PluginManager_PluginsStarted)} exception: {e1.Message}";
                    _log.Error(e1, message);
                }
            }
        }
        #endregion PluginManager Related

        #region IConsolePluginSupportsActivations Implementation

        /// <inheritdoc/>
        public void OnActivated()
        {
            Mouse.OverrideCursor = null;
        }

        /// <inheritdoc/>
        public void OnDeactivated()
        {
        }

        /// <inheritdoc/>
        public void OnShown(string pluginParameter)
        {
            _log.Info($"OnShown is called.");
            ConfigureServices();
        }
        #endregion IConsolePluginSupportsActivations Implementation

        #region Plugin Init
        /// <summary>
        /// Initialize or register services
        /// </summary>
        /// <remarks>Below code will be removed when <see cref="IConsole"/> provides the bootstrapper support</remarks>
        private void ConfigureServices()
        {
            if (_isConfigured)
            {
                _log.Info("@ ConfigureServices(), it\'s configured already.");
                return;
            }
            _log.Info("@ ConfigureServices(), starting configure.");

            // Marked all the instances as singleton
            // Pass the existing _console and _log instance so that Ioc doesn't new'up them
            ServiceCollection services = new ServiceCollection();
            if (_windowLayout != null)
                services.AddSingleton(_windowLayout);
            if (_log != null)
                services.AddSingleton(_log);
            if (_console != null)
                services.AddSingleton(_console);
            if (_dispatcherWrapper != null)
                services.AddSingleton(_dispatcherWrapper);
            if (_pluginManager != null)
                services.AddSingleton(_pluginManager);

            //Adding my ViewModel
            //services.AddSingleton<IExitAppViewModel, ExitAppViewModel>();

            PluginIoc.ConfigureServices(services.BuildServiceProvider());

            //Create the ViewModel right now
            //_viewModel = (ExitAppViewModel?)PluginIoc.GetService<IExitAppViewModel>();
            _isConfigured = true;
            _log.Info("@ ConfigureServices() exit.");
        }
        #endregion Plugin Init
    }

}
