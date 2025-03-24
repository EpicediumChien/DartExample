using DDPM.SA.Common;
using Dell.Client.Framework.Common;
using Dell.UnifiedAgent.DellTechHubSettings;
using System;
using System.Collections.Generic;
using System.Timers;
using System.Windows.Documents;
using VcpCore.Common;
using static System.Net.Mime.MediaTypeNames;

namespace DDPM.SA.Plugins.User.DeviceManager
{
    public class ColorProfileHelper : IDisposable
    {
        private bool isDisposed = false;
        private ILog _log = null;
        private IDeviceManagerSA _devMgr = null;
        private IColorPresetSA _colorPreset = null;
        private bool firstTimeDone = false;

        #region definition from device manager to here
        /// <summary>
        ///Check ICC profile update timers
        /// </summary>
        private System.Timers.Timer _checkICCProfileScheduleTimer = null;//Added 02/10 by Bruce

        private bool isDownloadingICC = false;//Added 02/13 by Bruce
        private bool isNeedPreDownloadingICC = false;//Added 02/13 by Bruce

        #endregion

        public void Dispose()
        {
            if (isDisposed)
            {
                return;
            }
            WriteLog("Do dispose");

            //outside object clear
            _log = null;
            _devMgr = null;

            //Timer
            if (_checkICCProfileScheduleTimer != null)
            {
                _checkICCProfileScheduleTimer.Elapsed -= new ElapsedEventHandler(CheckICCProfileScheduleTimer_Elapsed);
                _checkICCProfileScheduleTimer.Stop();
                _checkICCProfileScheduleTimer = null;
            }

            isDisposed = true;
        }

        public ColorProfileHelper(IDeviceManagerSA devMgr, ILog log)
        {
            _log = log;
            _devMgr = devMgr;
        }

        ~ColorProfileHelper()
        {
            Dispose();
        }

        public void UpdateColorPluginInstance(IColorPresetSA colorPlugin)
        {
            _colorPreset = colorPlugin;
        }

        public void InitColorProfileTimer()
        {
            if (_checkICCProfileScheduleTimer == null)
            {
                WriteLog($"_checkICCProfileScheduleTimer initialize");
                _checkICCProfileScheduleTimer = new System.Timers.Timer();
                _checkICCProfileScheduleTimer.Interval = TimeSpan.FromMinutes(1).TotalMilliseconds;
                _checkICCProfileScheduleTimer.Elapsed += new ElapsedEventHandler(CheckICCProfileScheduleTimer_Elapsed);
                _checkICCProfileScheduleTimer.Start();
            }
            isNeedPreDownloadingICC = true;
        }

        private void TriggerRealColorProfileTimer()
        {
            //Bruce 02/10 added timer to check icm
            if (_checkICCProfileScheduleTimer == null)
            {
                WriteLog($"_checkICCProfileScheduleTimer re-initialize");
                _checkICCProfileScheduleTimer = new System.Timers.Timer();                
            }
            WriteLog($"TriggerRealColorProfileTimer");
            _checkICCProfileScheduleTimer.Interval = TimeSpan.FromHours(24).TotalMilliseconds;
            _checkICCProfileScheduleTimer.Start();
            isNeedPreDownloadingICC = true;
        }

        /// <summary>
        /// 定期檢查color profile排程
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CheckICCProfileScheduleTimer_Elapsed(object? sender, ElapsedEventArgs e)//Bruce 02/10 added timer to check icm
        {
            if (_colorPreset == null)
            {
                WriteLog($"{nameof(CheckICCProfileScheduleTimer_Elapsed)} _colorPreset is null");
                return;
            }
            WriteLog($"{nameof(CheckICCProfileScheduleTimer_Elapsed)} start");
            if (_colorPreset != null)// && _SettingsPlugin != null)
            {
                List<MonitorInfo> mos = _devMgr.GetCurrentMonitorCache().Result;
                WriteLog($"{nameof(CheckICCProfileScheduleTimer_Elapsed)} _AllInfoMonitors.Count : {mos.Count}");
                for (int i = 0; i < mos.Count; i++)
                {
                    _colorPreset.DownloadICCData(mos[i], true, "", true).Wait();
                }
            }
            else
            {
                WriteLog($"{nameof(CheckICCProfileScheduleTimer_Elapsed)} _ColorPresetPlugin is null");
            }
            WriteLog($"{nameof(CheckICCProfileScheduleTimer_Elapsed)} done");

            if (!firstTimeDone)
            {
                _checkICCProfileScheduleTimer.Stop();
                firstTimeDone = true;
                TriggerRealColorProfileTimer();
                WriteLog($"{nameof(CheckICCProfileScheduleTimer_Elapsed)} first time done");
            }
        }

        public void PreDownloadICC(MonitorInfo newMontor)
        {
            WriteLog($"{nameof(PreDownloadICC)} start");
            WriteLog($"{nameof(PreDownloadICC)} isDownloadingICC : {isDownloadingICC}");
            if (!isDownloadingICC)
            {
                isDownloadingICC = true;
                WriteLog($"{nameof(PreDownloadICC)} DownloadICCData go, newMontor : {newMontor.modelName}");

                _colorPreset.DownloadICCData(/*_AllInfoMonitors[i]*/newMontor, true, "", isNeedPreDownloadingICC).Wait();

                if (isNeedPreDownloadingICC)
                {
                    isNeedPreDownloadingICC = false;
                }
                isDownloadingICC = false;
            }
            WriteLog($"{nameof(PreDownloadICC)} done");
        }

        private void WriteLog(string text,
            [System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
            [System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "",
            [System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = 0)
        {
            string className = this.GetType().Name;
            text = $"{DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss.fff")}[ColorProfileHelper] {text}, Class:{className}, Caller Name:{memberName}, Source Line {sourceLineNumber}";

            _log?.Info(text);
        }
    }
}
