using Dell.Client.Framework.Common;
using System;

namespace Dell.UnifyingAgent.Tests.Helpers
{
    public class TestHelper
    {
        /// <summary>
        ///     Creates an agent config based off parameters
        /// </summary>
        /// <param name="productName"></param>
        /// <param name="serviceName"></param>
        /// <param name="productVersion"></param>
        /// <param name="logFileNamePrefix"></param>
        /// <param name="agentUniqueId"></param>
        /// <param name="userProcessExecutable"></param>
        /// <returns></returns>
        public static AgentConfig CreateAgentConfig(string productName, string serviceName,
            string productVersion, string logFileNamePrefix, Guid agentUniqueId, Guid userProcessMutexGuid, string userProcessExecutable = "")
        {
            var config = new AgentConfig(agentUniqueId, userProcessMutexGuid)
            {
                CompanyName = GetCompanyName(),
                ProductName = productName,
                ServiceName = serviceName,
                ProductVersion = productVersion,
                AllowUnelevatedExecution = true,
                LogPrefixName = logFileNamePrefix,
                UserProcessExecutable = userProcessExecutable,
                PluginWildcards = new[] { "Dell.Client.Framework.Core.Tests*", "Dell.Client.Framework.UserProcess.Tests.*", "Dell.Client.Framework.Plugin*.dll", "Dell.UnifyingAgent.Tests.*" }
            };
            return config;
        }

        public static string GetCompanyName()
        {
            return string.Format("{0}-{1}", "Acme2", Platform.Instance.IsRunningPrivileged() ? "admin" : "user");
        }

        /// <summary>
        /// Returns true if the current process is running as an administrator.
        /// </summary>
        /// <returns></returns>
        public static bool IsRunAsAdmin()
        {
            return Platform.Instance.IsRunningPrivileged();
        }
    }
}
