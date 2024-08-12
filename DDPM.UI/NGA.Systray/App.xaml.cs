#region LicenceHeader
//
// Copyright © 2024, Dell Inc., All Rights Reserved.
// This material is confidential and a trade secret.  Permission to use this
// work for any purpose must be obtained in writing from Dell Inc.
//
#endregion

#nullable enable
using Dell.Client.Framework.Common;
using Dell.Client.Framework.UX.WPF;
using NGA.SystrayCore;
using Dell.UnifiedAgent.RemotePlugin.Client.Console;
using NGA.Common;
using NGA.Systray.Interfaces;
using System.IO;
using System.Windows;
using Dell.Client.Framework.UX.WPF.ResourceManager;
using Dell.Client.Framework.UX.WPF.ResourceManager.Enums;
using System;
using System.Linq;

namespace NGA.Systray
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : DucaSystrayCore
    {
        #region Variables        

        /// <summary>
        /// NGA ThickClient string text
        /// </summary>
        private const string NgaThickClient = "NGA.ThickClient.exe";
        private const string ProductName = Constants.SysTrayProductName;
        private const string SuiteName = Constants.SuiteName;

        /// <summary>
        ///  A uniqueId that identifies the SysTray App.
        /// </summary>
        internal static readonly Guid SysTrayUniqueGuid = new(Constants.SysTrayUniqueGuid);
        private const string LogFileName = "DDPMSysTrayConsole";
        private MainWindow? _mainWindow;

        private static readonly string _ngaThickClientProcessFullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, NgaThickClient);

        #endregion

        #region Constructor

        /// <summary>
        /// App constructor
        /// </summary>
        public App() : base(NGA.Resources.Resources.ResourceManager, SysTrayUniqueGuid, new DucaThickClientDetails(_ngaThickClientProcessFullPath, new(Constants.ThickClientUniqueGuid)))
        {
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Configures the ConsoleConfig.
        /// </summary>
        /// <returns><see cref="IConsoleConfig"/></returns>
        public override IConsoleConfig GetConsoleConfig()
        {
            return new UnifiedAgentConsoleConfig(SysTrayUniqueGuid, ProductName)
            {
                HeaderText = string.Format(NGA.Resources.Resources.MYDELL_APP_SYSTRAY_HEADER, NGA.Resources.Resources.ApplicationName),
                PluginWildcards = new[] { "Dell.Client.Framework.Plugin.*dll", "NGA.SysTray.*.dll", "Dell.UCA.SysTray.*.dll", "Dell.UCA.ConsolePreferencesHelperPlugin.dll", "Dell.UnifiedAgent.*.dll" },
                ExternalConsolePluginType = typeof(ISystrayPlugin),
                WindowText = string.Format(NGA.Resources.Resources.MYDELL_APP_SYSTRAY_WINDOW, NGA.Resources.Resources.ApplicationName),
                OobeAction = OOBEAction.TrackAndNotify,
                QuietPeriodAction = QuietPeriodAction.TrackAndNotify,
                LogPrefixName = LogFileName,
                CertificateStores = new[] { Constants.DellTrust },
                PluginValidationSchema = PluginValidationSchema.CustomCertStore,
                AutoLoadPlugins = true,
                SuiteName = SuiteName,
                LogFileScheme = LogFileWriter.RolloverScheme.CreateArchives,
                DisplayLanguage = DisplayLanguageEnum.GlobalizationPreferences
            };
        }

        #endregion

        #region Protected Methods

        /// <inheritdoc/>
        protected override Window? GetMainWindow(IFormBuilderBase formBuilder, string[]? args)
        {
            _mainWindow = new MainWindow(formBuilder, args?.ToArray());
            return _mainWindow;
        }
        
        /// <inheritdoc/>        
        public override ResourceManager CreateResourceManager()
        {
            return new ResourceManager(PreDefinedColorType.PreDefinedLightUI);
        }

        #endregion
    }
}
