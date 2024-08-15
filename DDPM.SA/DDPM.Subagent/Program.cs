#region LicenceHeader

//
// Copyright © 2022, Dell Inc., All Rights Reserved.
// This material is confidential and a trade secret.  Permission to use this
// work for any purpose must be obtained in writing from Dell Inc.
//
// Program.cs created on 10/4/2022T3:37 PM
//

#endregion

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using DDPM.SA.Common;
using Dell.Client.Framework.Agent;
using Dell.UnifiedAgent.Common;

namespace DDPM.Subagent
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
        [DllImport("kernel32.dll")]
        private static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        private const int SW_HIDE = 0;

        /// <summary>
        ///     A name for your product.
        /// </summary>
        private const string ProductName = "DDPM Subagent";

        /// <summary>
        ///     Service name for the product. Even if you are not running as a service always provide this because DCF provides you
        /// </summary>
        private const string ServiceName = "DDPMSubagent";

        /// <summary>
        ///     A uniqueId that identifies the agent. This value should be unique for your product.
        /// </summary>
        private static readonly Guid UniqueAgentGuid = new(IDs.DDPM_AGENT_ID);

        /// <summary>
        ///     A unique value that make sure your user process is only executed once per user and once per instance of your
        ///     application. This should be unique to your product.
        /// </summary>
        private static readonly Guid UserProcessMutexGuid = new(IDs.DDPM_MUTEX_ID);

#if RELEASE
        private static byte[][] certificateHash = { IDs.WST_Hash };
#endif


        private static void Main(string[] args)
        {
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
                PluginWildcards = new[] { "DDPM.SA.Plugins.*.dll" },
                /*
                 * Populate your user process mutex guid so it does not collide with any existing DCF products on the machine
                 */
                UserProcessMutexGuid = UserProcessMutexGuid,
                /*
                 * The list of all of the plugins you wish to publish in the Dell TechHub ecosystem
                 */
                PluginsToPublish = new List<Guid>
                {
                    new Guid(IDs.DDPM_SETTINGS_MANAGER_PLUGIN_ID),
                    new Guid(IDs.CLI_Manager_Plugin),
                    new Guid(IDs.SWUpdate_PLUGIN_ID),
                    new Guid(IDs.FWUPDATE_PLUGIN_ID)
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
                AllowUnelevatedExecution = false,
                /*
                 * True is the agent can run in multiple sessions. False otherwise.
                 * Agent implementation will check this value and throw an exception if agent behavior is not the expected one
                 * More info: https://confluence.cpg.dell.com/display/DCF/DCF+%7C+Support+User-Mode%2C+Multi-Session+and+Dual-Execution+Agents
                 */
                MultiSessionAgent = false
#if RELEASE
                ,
                ValidCertificateHashes = certificateHash
#endif
            };

            Console.WriteLine("DDPM.Subagent starting...");
            using (var agent = new Agent(agentConfig))
            {
                agent.RunAndBlock();
            }
            Console.WriteLine("DDPM.Subagent exiting...");
            //#else
            //            Console.WriteLine("\nERROR: Only the DEBUG build is supported");
            //            Console.WriteLine("Hit any key to exit");
            //            Console.ReadLine();
            //#endif
        }
    }
}