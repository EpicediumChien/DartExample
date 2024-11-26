#region LicenceHeader

//
// Copyright © 2024, Dell Inc., All Rights Reserved.
// This material is confidential and a trade secret.  Permission to use this
// work for any purpose must be obtained in writing from Dell Inc.
//
// Program.cs created on 22/4/2022T11:25 AM
//

#endregion

using DDPM.SA.Common;
using Dell.Client.Framework.Agent;
using Dell.UnifiedAgent.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using DDPM.SA.Obfuscation;
using System.IO;
using System.Windows;

namespace DDPM.Subagent.User
{
    /// <summary>
    /// Nuget packages required to make this process a DTH subagent:
    ///     - Dell.UnifiedAgent.DellTechHubSetting
    ///     - Dell.UnifiedAgent.Client
    ///
    /// Grab latest version from https://confluence.cpg.dell.com/pages/viewpage.action?pageId=389065517
    /// </summary>
    internal class Program
    {
        //[DllImport("kernel32.dll")]
        //static extern IntPtr GetConsoleWindow();
        //
        //[DllImport("user32.dll")]
        //static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
        //
        //const int SW_HIDE = 0;

        /// <summary>
        ///     A name for your product.
        /// </summary>
        private const string ProductName = "DDPM Subagent User";

        /// <summary>
        ///     Service name for the product. Even if you are not running as a service always provide this because DCF provides you
        /// </summary>
        private const string ServiceName = "DDPMSubagentUser";

        /// <summary>
        ///     A uniqueId that identifies the agent. This value should be unique for your product.
        /// </summary>
        private static readonly Guid UniqueAgentGuid = new(IDs.DDPM_USER_AGENT_ID);

        /// <summary>
        ///     A unique value that make sure your user process is only executed once per user and once per instance of your
        ///     application. This should be unique to your product.
        /// </summary>
        private static readonly Guid UserProcessMutexGuid = new(IDs.DDPM_USER_MUTEX_ID);

        //SDL to require log folder locate at user profile (user mode subagent)
        private static readonly string LogLocation = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Dell\\Dell Display and Peripheral Manager\\Log\\DDPM.Subagent.User");

        private static void Main(string[] args)
        {
            //Dean 0626 remove to fix SAST issue
            //var handle = GetConsoleWindow();
            //
            // Hide
            //ShowWindow(handle, SW_HIDE);

            /*
             * Only allow this application in debug mode
             */
            //#if DEBUG
            /*
             * Get the executing assembly so the ProductVersion can be populated
             */
            var assembly = Assembly.GetExecutingAssembly();

            /*
             * Create a UnifiedAgentConfigWindows to pass into the Agent constructor
             */
            UnifiedAgentConfigWindows agentConfig = new UnifiedAgentConfigWindows(UniqueAgentGuid, UserProcessMutexGuid)
            {
                /*
                 * Set the product name
                 */
                ProductName = ProductName,
                /*
                 * Populate the product version
                 */
                ProductVersion = FileVersionInfo.GetVersionInfo(assembly.Location).FileVersion,
                /*
                 * Populate the service name
                 */
                ServiceName = ServiceName,
                /*
                 * Populate any plugin wildcards. If you do not provides wildcards that match your plugin naming schema DCF will not load the plugins
                 */
                PluginWildcards = new[] {
                  "VcpCore.Plugins.dll",
                  "DDPM.SA.Plugins.User.*.dll",
                  "Dell.Client.Framework.Plugin.*.dll",   // Required DCF plugin loading
                  "Dell.UnifiedAgent.*.dll",              // Required UA plugin loading
                  "DtpInstrumentationUtil.Plugin.dll",
                  "Dell.TechHub.Commodity.Sdk.dll",
                  "Dell.TechHub.Instrumentation.Sdk.dll",
                  "CLI.Plugins.*.dll" },
                /*
                 * Populate your user process mutex guid so it does not collide with any existing DCF products on the machine
                 */
                UserProcessMutexGuid = UserProcessMutexGuid,
                /*
                 * The list of all of the plugins you wish to publish in the Dell TechHub ecosystem
                 */
                PluginsToPublish = new List<Guid>
                {
                    new Guid(IDs.VCP_CORE_PLUGIN_ID),
                    new Guid(IDs.Scheduler_Manager_Plugin_ID),
                    new Guid(IDs.Telementry_Scheduler_Plugin_ID),
                    new Guid(IDs.Display_Manager_PLUGIN_ID),
                    new Guid(IDs.DDPM_PERIPHERALS_PLUGIN_ID),
                    new Guid(IDs.Device_Manager_Plugin_ID),
                    new Guid(IDs.DisplayProperties_PLUGIN_ID),
                    new Guid(IDs.DDPM_COLOR_PRESET_PLUGIN_ID),
                    new Guid(IDs.DDPM_USBKVM_PLUGIN_ID),
                    new Guid(IDs.DDPM_NKVM_PLUGIN_ID),
                    new Guid(IDs.DDPM_HOTKEY_PLUGIN_ID),
                    new Guid(IDs.DDPM_EAPlugin_PLUGIN_ID),
                    new Guid(IDs.PipPbp_Manager_PLUGIN_ID),
                    new Guid(IDs.DDPM_SETTINGSMANAGER_SA_PLUGIN_ID),
                    new Guid(IDs.DDPM_CLI_Proxy_Plugin),
                    new Guid(IDs.CLI_Plugin_Display),
                    new Guid(IDs.CLI_Plugin_Peripherals),
                    new Guid(IDs.DDPM_DTP_Proxy_Plugin),
                    new Guid(IDs.DDPM_EMPlugin_PLUGIN_ID),
                },
                /*
                 * This is the name used in the log file
                 */
                LogPrefixName = ServiceName,
                /*
                 * True is the agent can run unelevated. False otherwise.
                 * Agent implementation will check this value and throw an exception if agent behavior is not the expected one
                 * More info: https://confluence.cpg.dell.com/display/DCF/DCF+%7C+Support+User-Mode%2C+Multi-Session+and+Dual-Execution+Agents
                 */
                AllowUnelevatedExecution = true,
                /*
                 * True is the agent can run in multiple sessions. False otherwise.
                 * Agent implementation will check this value and throw an exception if agent behavior is not the expected one
                 * More info: https://confluence.cpg.dell.com/display/DCF/DCF+%7C+Support+User-Mode%2C+Multi-Session+and+Dual-Execution+Agents
                 */
                MultiSessionAgent = true,

                //SDL requirement to set logs folder at user profile.
                LogDirectory = LogLocation
#if RELEASE
                ,
                ValidCertificateHashes = ThumbprintHash_CICD.certificateHash
#endif
            };

            Console.WriteLine("DDPM.Subagent.User starting...");


            using (var agent = new Agent(agentConfig))
            {
                agent.RunAndBlock();
            }

            Console.WriteLine("DDPM.Subagent.User exiting...");

            //#else
            //            Console.WriteLine("\nERROR: Only the DEBUG build is supported");
            //            Console.WriteLine("Hit any key to exit");
            //            Console.ReadLine();
            //#endif
        }
    }
}