using CommunityToolkit.Mvvm.DependencyInjection;
using DDPM.SA.Common;
using DDPM.UI.Common;
using DDPM.UI.Common.Models;
using Dell.Client.Framework.Common;
using Dell.Client.Framework.Common.Annotations;
using Dell.Client.Framework.UX.WPF;
using Microsoft.Extensions.DependencyInjection;
using NGA.ThickClient.Interfaces;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Input;
using DDPMConstants = DDPM.UI.Common.Constants;

namespace DDPM.UI.Plugin.WalkThroughPlugin
{
    /// <summary>
    /// Interaction logic for AboutView Plugin.xaml
    /// </summary>
    [Plugin(PluginId, PluginName, Version = PluginVersion, Category = Category.Utility)]
    [Descriptor(Description = Description)]
    [Publisher(Name = "DDPM WalkThroughPlugin", Support = "Wistron DDPM Team")]
    //[PluginRequires(Id = DDPM.SA.Common.IDs.Device_Manager_Plugin_ID, AllowDynamicResolving = true)]
    [ExcludeFromCodeCoverage]
    public class WalkThroughPlugin : IConsoleTakeoverPagePlugin, IConsolePluginSupportsActivations, IThickClientPlugin
    {
        private const string PluginId = DDPMConstants.WalkThroughPluginId;
        private const string PluginName = "DDPM WalkThrough Plugin";
        private const string PluginVersion = "1.0";
        private const string Description = "Display DDPM.WalkThroughpage";

        public static readonly Ioc PluginIoc = new();

        private readonly ILog _log;
        private readonly IConsole _console;

        private bool _isConfigured;

        /// <summary>
        /// This property is required by the IConsolePagePlugin. It specifies the text to display when the page is shown.
        /// </summary>
        public string HeaderText => "DDPM WalkThroughpage";

        /// <summary>
        /// Page Type
        /// </summary>
        public Type PageType => typeof(WalkThroughPage);

        /// <summary>
        /// Default constructor
        /// </summary>
        public WalkThroughPlugin(IConsole console, IGearMenu gearMenu)
        {
            _console = console;
            _log = console.CreateLog("WalkThroughPLG");
            _log.Info($"{nameof(WalkThroughPlugin)} - Constructed");
        }

        public void OnActivated()
        {
            Mouse.OverrideCursor = null;
        }

        public void OnDeactivated()
        {
            Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;
        }
        public void OnShown()
        {
            ConfigureServices();
            Mouse.OverrideCursor = null;
        }
        private void ConfigureServices()
        {
            if (_isConfigured)
                return;

            PluginIoc.ConfigureServices(new ServiceCollection()
                .AddSingleton(_console)
                .AddSingleton(_log)
                //.AddSingleton<WalkThroughPageViewModel, WalkThroughPageViewModel>()
                .BuildServiceProvider());
            _isConfigured = true;
        }
    }
}