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
        #region CLI Flags: Enabled/Locked
        public bool IsFunctionEnabled { get; set; }
        #endregion CLI Flags: Enabled/Locked

        #region Events
        public event EventHandler<string> EditStarted;

        //Robert_Lin, 2024-8-4 new added
        public event EventHandler<EAArgs> EditReturn;

        //Robert_Lin, 2024-10-8 added
        public event EventHandler<EAArgs> EASettingsChanged;
        #endregion Events

        #region Methods
        public Task<bool> SetEAWrokSplit(MonitorInfo monitorInfo, int cellCount, char splitKey, List<double>? settings);

        public Task<bool> EditCommand(MonitorInfo monitorInfo, EAArgs args);

        public Task<bool> ReloadEzSettings();

        public Task<bool> SetEASelectedLayout(MonitorInfo monitorInfo, SplitJson spJson);
        #endregion




        //public Task<bool> RequestEditSplit(MonitorInfo monitorInfo, SplitJson);


        //public Task<bool> EAReloadMonitorSettings(MonitorInfo monitorInfo);
    }
}