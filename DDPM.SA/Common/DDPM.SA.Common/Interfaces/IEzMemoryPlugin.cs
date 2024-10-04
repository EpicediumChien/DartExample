using Dell.Client.Framework.Common;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DDPM.SA.Common
{
    public interface IEzMemoryPlugin : IFrameworkPlugin
    {
        Task <Dictionary<string, InstalledAppInfo>> GetAllAppList();
        Task<bool> LaunchAndArrangeApps(Dictionary<String, Bind_AddFullPage_AppCollectionData> sortApps);
    }
}