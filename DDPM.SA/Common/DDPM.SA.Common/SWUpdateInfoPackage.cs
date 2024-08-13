using DPeMPublic.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDPM.SA.Common
{
    /// <summary>
    /// 軟體更新資訊包
    /// </summary>
    public class SWUpdateInfoPackage
    {
        /// <summary>
        /// 將該軟體更新資訊包儲存進設定檔的日期，用於判斷使用者延遲更新的時間
        /// </summary>
        public DateTime? SaveTime { get; set; }
        /// <summary>
        /// 可延遲次數
        /// </summary>
        public int DelayTimesAvailable { get; set; }
        /// <summary>
        /// 該更新資訊包檢查更新的時間
        /// </summary>
        public DateTime TheLastCheckTime { get; set; }
        /// <summary>
        /// 軟體更新資訊
        /// </summary>
        public List<SWUpdateInfo> SWUpdateInfo { get; set; }
        public SWUpdateInfoPackage()
        {
            SaveTime = null;
            TheLastCheckTime = new DateTime();
            SWUpdateInfo = new List<SWUpdateInfo>();
        }
    }
    /// <summary>
    /// 軟體資訊
    /// </summary>
    public class SWUpdateInfo
    {
        /// <summary>
        /// 軟體更新的錯誤碼，安裝時使用
        /// </summary>
        public SWUErrorCode SWUErrorCode { get; set; }
        public string SoftwareName { get; set; }
        public string SoftwareVersion { get; set; }
        public string TheLatestVersion { get; set; }
        public bool NeedUpdated { get; set; }
        /// <summary>
        /// 安裝時使用，獲取目前的進度資訊
        /// </summary>
        public string ProcessName { get; set; }
        /// <summary>
        /// 安裝時使用，獲取安裝進度
        /// </summary>
        public double ProcessProgress { get; set; }
        public string ServerPath { get; set; }
        public string FileSavepath { get; set; }
        public string InstallPaths { get; set; }
        public bool Equals(SWUpdateInfo swUpdateInfo)
        {
            return swUpdateInfo.SoftwareName == SoftwareName;
        }
    }
    //0531 Bruce 因應IL的現有安裝包修改判斷，FWUpdateInfoPackage.cs中新增FWUErrorCode矩陣
    public enum SWUErrorCode
    {
        NoError = 0,
        SoftwareUpdateFailed = 1,
        SoftwareUpdatNotSupportedForThisOS = 2,
        SoftwareUpdateTimeout = 3,
        PCBatteryTooLow = 5,
        NetworkDisconnection = 7,
        UserAbortedFail = 8,
        UserAborted = 9,
        Unknow = 99
    }
    /// <summary>
    /// Metadata結構
    /// </summary>
    public class ChangeLog
    {
        public string Version { get; set; }
        public string ServerPath { get; set; }
        public List<string> SupportedOS { get; set; }
        public string MinimumSoftware { get; set; }
    }
    /// <summary>
    /// Metadata結構
    /// </summary>
    public class Software
    {
        public string SoftwareName { get; set; }
        public string SoftwareVersion { get; set; }
        public string ServerPath { get; set; }
        public string InstallPath { get; set; }
        public List<string> SupportedOS { get; set; }
        public string MinimumSoftware { get; set; }
    }
    /// <summary>
    /// Metadata結構
    /// </summary>
    public class SWUpdateHelper
    {
        public int Version { get; set; }
        public ChangeLog ChangeLog { get; set; }
        public List<Software> Softwares { get; set; }
    }
}
