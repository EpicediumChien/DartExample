#region LicenceHeader

//
// Copyright © 2022, Dell Inc., All Rights Reserved.
// This material is confidential and a trade secret.  Permission to use this
// work for any purpose must be obtained in writing from Dell Inc.
//

#endregion

using Dell.Client.Framework.Agent;
using Dell.Client.Framework.Common;
using Dell.Client.Framework.Security;
using Dell.UnifiedAgent.Common;
using System.Diagnostics;
using System.Reflection;
using System.ServiceProcess;
using Constants = NGA.Common.Constants;
using DDPM.SA.Obfuscation;


//using DDPMConstants = DDPM.UI.Common.Constants;

namespace NGA.Manager
{
    internal static class Program
    {
        /// <summary>
        ///     NGA Manager product name
        /// </summary>
        private const string ProductName = Constants.ManagerProductName;

        /// <summary>
        ///     Service name for the NGA Manager
        /// </summary>
        private const string ServiceName = "MyDell Notification Manager";

        /// <summary>
        ///     A uniqueId that identifies the agent
        /// </summary>
        private static readonly Guid UniqueAgentGuid = new("{45bd6892-af9d-43d1-bddb-abdc62de5521}");

        /// <summary>
        ///     A uniqueId that identifies the User Process Mutex
        /// </summary>
        private static readonly Guid UniqueUserProcessMutexGuid = new("{23bd6462-af9d-55d1-bddb-abde62de4528}");

        private static void Main(string[] args)
        {
            var config = GetUnifiedAgentConfig();
            using var agent = new Agent(config);
            agent.RunAndBlock();
        }

        #region Public Methods

        /// <summary>
        /// Configures the UnifiedAgentConfig.
        /// </summary>
        /// <returns></returns>
        public static UnifiedAgentConfigWindows GetUnifiedAgentConfig()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            return new UnifiedAgentConfigWindows(UniqueAgentGuid, UniqueUserProcessMutexGuid)
            {
                ProductName = ProductName,
                ProductVersion = FileVersionInfo.GetVersionInfo(assembly.Location).FileVersion,
                ServiceName = ServiceName,
                StopAgentOnShutdown = true,
                /*
                 * This is needed because DCF 6.3 did not provide
                 * a non-obsolete version
                 */
#pragma warning disable CS0618 // Type or member is obsolete
                LogFileScheme = LogFile.RolloverScheme.CreateArchives,
#pragma warning restore CS0618 // Type or member is obsolete
                PluginWildcards = new[] { "Dell.Client.Framework.Plugin.*dll", "Dell.UCA.*.dll" },
                FailureRecoveryAction = FailureAction.None,
                StartMode = ServiceStartMode.Automatic,
                AllowUnelevatedExecution = false,
                PluginsToPublish = new List<Guid>
                {
                    new(Constants.ManagerNotificationPluginId),
                    new(Constants.PreferencesPluginId),
                    new(Constants.DiscoveryNotificationPluginId),
                    new(Constants.ManagerNotificationProtocolPluginId),
                    new(Constants.TelemetryConsentPluginId),
                    //new(DDPM.UI.Common.Constants.KeyboardKB900PluginId)
                },
                LogPrefixName = ProductName,
                OobeAction = OOBEAction.TrackAndNotify,
                QuietPeriodAction = QuietPeriodAction.TrackAndNotify,
                CertificateStores = new[] { Constants.DellTrust },
                PluginValidationSchema = PluginValidationSchema.CustomCertStore
#if RELEASE
                ,
                ValidCertificateHashes = ThumbprintHash.certificateHash
#endif
            };
        }

        #endregion
    }
}