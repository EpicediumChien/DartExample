using Dell.Client.Framework.Common;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VcpCore.Common;

namespace DDPM.SA.Common
{
    public interface IEzMemoryPlugin : IFrameworkPlugin
    {
        Task <Dictionary<string, InstalledAppInfo>> GetAllAppList();
        Task<bool> LaunchAndArrangeApps(Dictionary<String, Bind_AddFullPage_AppCollectionData> sortApps);

        Task<bool> CheckEAIDExit(MonitorInfo moinfo, int eAID);
        Task<bool> DeleteEAID(MonitorInfo moinfo, int eAID);

    }
}