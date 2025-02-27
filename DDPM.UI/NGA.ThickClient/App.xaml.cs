#region LicenceHeader

//
// Copyright © 2024, Dell Inc., All Rights Reserved.
// This material is confidential and a trade secret.  Permission to use this
// work for any purpose must be obtained in writing from Dell Inc.
//

#endregion

using DDPM.UI.Common;
using Dell.Client.Framework.Common;
using Dell.Client.Framework.UX.WPF;
using Dell.Client.Framework.UX.WPF.ResourceManager;
using Dell.Client.Framework.UX.WPF.ResourceManager.Enums;
using Dell.UnifiedAgent.RemotePlugin.Client.Console;
using NGA.ThickClient.Interfaces;
using NGA.ThickClientCore;
using System.Data.OleDb;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using Constants = NGA.Common.Constants;

namespace NGA.ThickClient
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public sealed partial class App : DucaThickClientCore
    {
        #region Variables
        /// <summary>
        ///  A uniqueId that identifies the Thick Client App.
        /// </summary>
        private static readonly Guid ThickClientUniqueGuid = new(Constants.ThickClientUniqueGuid);

        private const string LogFileName = "DDPMConsole";
        private const string ProductName = Constants.ThickClientProductName; //="Console"
        private const string SuiteName = Constants.SuiteName; //="MyDell"
        private const string AppStylesUriString = "pack://application:,,,/Resources/Styles.xaml";

        /// <summary>
        /// NGA SysTray Process Name.
        /// </summary>
        //private const string NgaSysTrayProcessName = "Dell.UCA.SysTray.exe";
        private const string NgaSysTrayProcessName = "";

        private static readonly string NgaSysTrayProcessFullPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, NgaSysTrayProcessName);
        private MainWindow? _mainWindow;
        private SplashScreen? _splashScreen;

        #endregion

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        private static extern int GetPrivateProfileInt(string lpAppName, string lpKeyName, int nDefault, string lpFileName);
        private static int _GetPrivateProfileInt(string lpAppName, string lpKeyName, int nDefault, string lpFileName)
        {
            int rst = GetPrivateProfileInt(lpAppName, lpKeyName, nDefault, lpFileName);

            if (rst == nDefault)
            {
#if DEBUG
                Console.WriteLine($"[App] GetPrivateProfileInt(): the key \"{lpKeyName}\" is not found, return def: {nDefault}");
#endif
            }

            return rst;
        }

        //2024-5-8 Robert_Lin, to fix the issue that will cause exception in filelock.cs,
        // FileLock ctor below code:
        //   PathCheckErrorCodes result = PathHelper.ValidateFilePath(filepath, pathCheckOptions);
        //   => result will be "ERROR_FILE_DOES_NOT_EXIST"
        // We should set sysTrayDetails to null, or not specified.
        // Origial App ctor is:
        //   public App() : base(NGA.Resources.Resources.ResourceManager, ThickClientUniqueGuid, new DucaSystrayDetails(NgaSysTrayProcessFullPath, new(Constants.SysTrayUniqueGuid)))
        // New ctor will remove regument of "new DucaSystrayDetail(...."
        /// <summary>
        /// Constructor
        /// </summary>
        public App() : base(NGA.Resources.Resources.ResourceManager, ThickClientUniqueGuid) { }

        /// <summary>
        /// Configures the ConsoleConfig.
        /// </summary>
        /// <returns><see cref="IConsoleConfig"/></returns>
        public override IConsoleConfig GetConsoleConfig()
        {
            return new UnifiedAgentConsoleConfig(ThickClientUniqueGuid, ProductName)
            {
                OobeAction = OOBEAction.TrackAndNotify,
                QuietPeriodAction = QuietPeriodAction.TrackAndNotify,
                TriggerEcosystemOnStart = true, // this flag if true, will trigger start all DTH sub agents, which are registered as delayed start
                HeaderText = string.Format(NGA.Resources.Resources.MYDELL_APP_THICKCLIENT_HEADER, NGA.Resources.Resources.ApplicationName),
                PluginWildcards = new[] { "Dell.UCA.ThickClient.*.dll", "NGA.ThickClient.*.dll", "Dell.UCA.ConsolePreferencesHelperPlugin.dll", "DDPM.UI.Plugin.*.dll" },
                ExternalConsolePluginType = typeof(IThickClientPlugin),
                WindowText = string.Format(NGA.Resources.Resources.MYDELL_APP_THICKCLIENT_CONSOLE_WINDOWTEXT, NGA.Resources.Resources.ApplicationName),
                IsEnabledEnhancedWindowTitle = true,
                LogPrefixName = LogFileName,
                CertificateStores = new[] { Constants.DellTrust },
                PluginValidationSchema = PluginValidationSchema.CustomCertStore,
                AutoLoadPlugins = true,
                SuiteName = SuiteName,
                LogFileScheme = LogFileWriter.RolloverScheme.CreateArchives,
                DisplayLanguage = DisplayLanguageEnum.GlobalizationPreferences,
                LogRootFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Dell\\Dell Display and Peripheral Manager\\Log\\DDPM.GUI")
            };
        }

        /// <inheritdoc/>
        public override ResourceManager CreateResourceManager()
        {
            return new ResourceManager(PreDefinedColorType.PreDefinedDarkUI/*PreDefinedLightUI*/);
        }

        /// <summary>
        /// Loads the custom library theme resources into the application
        /// </summary>
        public override ResourceManager LoadResources()
        {
            var resourceManager = base.LoadResources();
            //update dark/light mode
            DdpmCommonHelper.updateMergedDictionaries(resourceManager);
            try
            {
                //Robert_Lin 2025-1-20 add Custom Controls resoure file (for Narrator mode)
                //Based on DUCA team, Sharap Viswanathan, Karthik, suggestion.
                const string DdpmCustomControls = "pack://application:,,,/DDPM.UI.Common;component/UserControls/Generic.xaml";
                var resourceDictionaries = new[] {
                    new ResourceDictionary { Source = new Uri(AppStylesUriString, UriKind.RelativeOrAbsolute) }
                    , new ResourceDictionary { Source = new Uri(DdpmCustomControls, UriKind.RelativeOrAbsolute) }
                };
                //OLD:
                //var resourceDictionaries = new[] { 
                //    new ResourceDictionary { Source = new Uri(AppStylesUriString, UriKind.RelativeOrAbsolute) }
                //};
                resourceManager.AddCustomStyles(resourceDictionaries).StageAndCommitResources();
            }
            catch (Exception ex)
            {
                EventLogHelper.WriteEventLog(
                    $"{string.Format(NGA.Resources.Resources.MYDELL_APP_THICKCLIENT_CONSOLE_WINDOWTEXT, NGA.Resources.Resources.ApplicationName)} - Unable to set custom library themes. {ex}",
                    EventLogEntryType.Error);
            }

            return resourceManager;
        }

        /// <summary>
        /// Returns Splash screen instance
        /// </summary>
        /// <returns></returns>
        protected override SplashScreen? GetSplashScreen()
        {
            // default size
            var sz = string.Empty;

            // if __effective__ screen size is 4k or above, use the bigger splash asset
            if (SystemParameters.PrimaryScreenWidth >= 3840 && SystemParameters.PrimaryScreenHeight >= 2160)
                sz = Constants.SplashScreenResolution4K;

            _splashScreen = new SplashScreen(Assembly.GetExecutingAssembly(), string.Format(DdpmCommonHelper.SplashPath, sz));

            return _splashScreen;
        }

        /// <summary>
        /// Returns Main Window instance
        /// </summary>
        /// <param name="formBuilder"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        protected override Window? GetMainWindow(IFormBuilderBase formBuilder, string[]? args)
        {
            var logFactory = formBuilder.GetSubsystem<ILogFactory>();
            if (logFactory == null)
            {
                EventLogHelper.WriteEventLog($"{string.Format(NGA.Resources.Resources.MYDELL_APP_THICKCLIENT_CONSOLE_WINDOWTEXT, NGA.Resources.Resources.ApplicationName)} - {nameof(formBuilder)} was null", EventLogEntryType.Error);

                // We are in an error state close the application
                Dispatcher.Invoke(Shutdown);
                return null;
            }
            else
            {
                logFactory.CreateLogger("DDPMAPP", typeof(App));
            }

            _mainWindow = new MainWindow(formBuilder, args);


            return _mainWindow;
        }
    }
}