using DDPM.SA.Common.Display;
using Dell.Client.Framework.Common;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VcpCore.Common;

namespace DDPM.SA.Common.Interfaces
{
    public interface IEasyArrangeService : IFrameworkPlugin
    {
        public bool IsFunctionEnabled { get; set; }

        public Task<bool> SetEAWrokSplit(MonitorInfo monitorInfo, int cellCount, char splitKey, List<double>? settings);

        //Robert_Lin, 2024-9-13 Remove unused interfaces
        //public Task<bool> RequestEditSplit(MonitorInfo monitorInfo, int cellCount, char splitKey, string customName, List<double>? settings = null);

        //Robert_Lin, 2024-9-13 Remove unused interfaces
        //public event EventHandler<string> EditCompleted;

        public event EventHandler<string> EditStarted;

        //public Task<bool> RequestEditSplit(MonitorInfo monitorInfo, SplitJson);

        //Robert_Lin, 2024-8-4 new added
        public Task<bool> EditCommand(MonitorInfo monitorInfo, EAArgs args);

        public event EventHandler<EAArgs> EditReturn;
        //public Task<bool> EAReloadMonitorSettings(MonitorInfo monitorInfo);
        public Task<bool> ReloadEzSettings();
        public Task<bool> SetEASelectedLayout(MonitorInfo monitorInfo, SplitJson spJson);
     }
}