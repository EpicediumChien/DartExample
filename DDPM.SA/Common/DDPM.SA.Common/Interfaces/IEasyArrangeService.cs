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
        public Task<bool> NotifyEASelectedLayoutChanged(MonitorInfo monitorInfo, SplitJson spJson);

        public Task<bool> EditCommand(MonitorInfo monitorInfo, EAArgs args);

        public Task<bool> ReloadEzSettings();

        public Task<bool> SetEASelectedLayout(MonitorInfo monitorInfo, SplitJson spJson);

        /// <summary>
        /// Return current Span across multiple monitor option is Enabled/Disabled;
        /// Note that it's different with EzSettings.IsSpanAcrossMultiMonitors (=ON|OFF)
        /// </summary>
        /// <returns>True=Enabled; False=Disabled</returns>
        public Task<bool> GetIsSpanEnabled();

        /// <summary>
        /// General notification  to EAPlugin from other Plugins inside DDPM.SA.User
        /// </summary>
        /// <param name="eaArgs"></param>
        /// <returns></returns>
        public Task<bool> NotifyEAMessage(EAArgs eaArgs);

        #endregion

        #region Telemetry - Used by EABroker only
        public void SendEasyArrangeLayoutTelemetry(string eventValue, MonitorInfo? mi = null, Telementry_Frequency frequency = Telementry_Frequency.RealTime);
        #endregion
    }
}