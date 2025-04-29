using DDPM.SA.Common;
using Dell.Client.Framework.Common;
using Dell.UnifiedAgent.DellTechHubSettings;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Documents;
using VcpCore.Common;
using static System.Net.Mime.MediaTypeNames;

namespace DDPM.SA.Plugins.User.DeviceManager
{
    public class MonitorPresetCache
    {
        public string modelName { get; set; } = string.Empty;
        public string displayName { get; set; } = string.Empty;
        public string serviceTag { get; set; } = string.Empty;
        public bool isHDROn { get; set; } = false;
        public List<string> preset_list { get; set; } = new List<string>();
        public List<string> presets_list_HDR { get; set;} = new List<string>();
    }        

    public class ColorProfileHelper : IDisposable
    {
        private bool isDisposed = false;
        private ILog _log = null;
        private IDeviceManagerSA _devMgr = null;
        private IColorPresetSA _colorPreset = null;
        private bool firstTimeDone = false;
        private List<MonitorPresetCache> preset_cache = new List<MonitorPresetCache>();

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

            //internal data object clear
            if (preset_cache.Count > 0)
            {
                foreach (var item in preset_cache)
                {
                    item.preset_list.Clear();
                    item.preset_list = null;
                }
                preset_cache.Clear();
            }
            preset_cache = null;

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
                    _colorPreset.DownloadICCData(mos[i].modelName, mos[i].DisplayName, true, "", true).Wait();
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

                _colorPreset.DownloadICCData(newMontor.modelName, newMontor.DisplayName, true, "", isNeedPreDownloadingICC).Wait();

                if (isNeedPreDownloadingICC)
                {
                    isNeedPreDownloadingICC = false;
                }
                isDownloadingICC = false;
            }
            WriteLog($"{nameof(PreDownloadICC)} done");
        }

        public Task<DDPM.SA.Common.IIC_Metadata> DownloadICCData(MonitorInfo m, bool blICCProfile = false, string savelPath = "")
        {
            DDPM.SA.Common.IIC_Metadata _ICC_Metadata = new DDPM.SA.Common.IIC_Metadata();

            if (_colorPreset == null)
            {
                WriteLog("null _ColorPresetPlugin in [DownloadICCData]");
                return Task.FromResult(_ICC_Metadata);
            }
            else
            {
                _ICC_Metadata = _colorPreset.DownloadICCData(m.modelName, m.DisplayName, blICCProfile, savelPath).Result;
            }

            return Task.FromResult(_ICC_Metadata);
        }

        #region Color Preset cache management
        //check if new then add to cache
        public void AddPresetListToCache(MonitorInfo mo, bool isHDR_On, List<string> PresetList)
        {
            if (mo == null)
                return;
            int idx = preset_cache.FindIndex(x => x.serviceTag.Equals(mo.edid.ServiceTag, StringComparison.OrdinalIgnoreCase) &&
                                                  x.modelName.Equals(mo.modelName, StringComparison.OrdinalIgnoreCase) &&
                                                  x.isHDROn == isHDR_On);
            if (idx < 0)
            {
                MonitorPresetCache mpc = new MonitorPresetCache();
                mpc.displayName = mo.DisplayName;
                mpc.modelName = mo.modelName;
                mpc.serviceTag = mo.edid.ServiceTag;
                mpc.preset_list = PresetList;
                mpc.isHDROn = isHDR_On;
                preset_cache.Add(mpc);
            }
            else
            {
                //if display name changed or HDR different, might means monitor changed
                //remove old one and add new one
                if (!preset_cache[idx].displayName.Equals(mo.DisplayName, StringComparison.OrdinalIgnoreCase) ||
                    preset_cache[idx].isHDROn != isHDR_On)
                {
                    preset_cache.RemoveAt(idx);
                    MonitorPresetCache mpc = new MonitorPresetCache();
                    mpc.displayName = mo.DisplayName;
                    mpc.modelName = mo.modelName;
                    mpc.serviceTag = mo.edid.ServiceTag;
                    mpc.isHDROn = isHDR_On;
                    mpc.preset_list = PresetList;
                    preset_cache.Add(mpc);
                }
            }
        }

        public MonitorPresetCache GetPresetListFromCache(MonitorInfo mo, bool isHDR_On)
        {
            if (mo == null)
                return null;
            MonitorPresetCache mpc = preset_cache.Find(x => x.serviceTag.Equals(mo.edid.ServiceTag, StringComparison.OrdinalIgnoreCase) &&
                                                            x.modelName.Equals(mo.modelName, StringComparison.OrdinalIgnoreCase) &&
                                                            x.displayName.Equals(mo.DisplayName, StringComparison.OrdinalIgnoreCase) &&
                                                            x.isHDROn == isHDR_On);
            return mpc;
        }
        #endregion

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
