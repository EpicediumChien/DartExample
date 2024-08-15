using DPeMPublic.Common.Enums;
using System;
using System.Collections.Generic;

namespace DDPM.SA.Common
{
    /// <summary>
    /// 韌體更新資訊包
    /// </summary>
    public class FWUpdateInfoPackage
    {
        /// <summary>
        /// 將該韌體更新資訊包儲存進設定檔的日期，用於判斷使用者延遲更新的時間
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
        /// 韌體更新資訊
        /// </summary>
        public List<FWUpdateInfo> FWUpdateInfo { get; set; }

        public FWUpdateInfoPackage()
        {
            SaveTime = null;
            TheLastCheckTime = new DateTime();
            FWUpdateInfo = new List<FWUpdateInfo>();
        }
    }

    public class DokcUODUpdateInfoPackage
    {
        /// <summary>
        /// 將該韌體更新資訊包儲存進設定檔的日期
        /// </summary>
        public DateTime? SaveTime { get; set; }

        /// <summary>
        /// 可延遲次數
        /// </summary>
        public int DelayTimesAvailable { get; set; }

        /// <summary>
        /// 裝置資訊
        /// </summary>
        public FWUpdateInfo FWUpdateInfo { get; set; }

        public DokcUODUpdateInfoPackage()
        {
            SaveTime = null;
            FWUpdateInfo = new FWUpdateInfo();
        }
    }

    /// <summary>
    /// 裝置資訊
    /// </summary>
    public class FWUpdateInfo
    {
        /// <summary>
        /// 韌體更新的錯誤碼，安裝時使用
        /// </summary>
        public FWUErrorCode FWUErrorCode { get; set; }

        //0614 Bruce 將原本DeviceType型態是字串改成跟IL一樣這樣可以直接使用IL提供的矩陣做判斷，UI有個地方也會跟著異動
        public DeviceType DeviceType { get; set; }

        public string DeviceName { get; set; }
        public string DeviceId { get; set; }
        public string DevicePath { get; set; }
        public string Model { get; set; }
        public string DeviceVersion { get; set; }
        public string TheLatestVersion { get; set; }

        /// <summary>
        /// Dock專用，判斷是否斷線更新
        /// </summary>
        public bool IsUOD { get; set; }

        /// <summary>
        /// 裝置例項路徑，Dock專用，用於判斷UOD的Dock再次連接時是否更新完成
        /// </summary>
        public string PNPDeviceID { get; set; }

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

        public bool Equals(FWUpdateInfo fwUpdateInfo)
        {
            return fwUpdateInfo.DevicePath == DevicePath;
        }
    }

    //0531 Bruce 因應IL的現有安裝包修改判斷，FWUpdateInfoPackage.cs中新增FWUErrorCode矩陣
    public enum FWUErrorCode
    {
        NoError = 0,
        DeviceDisconnected = 1,
        FirmwareUpdateFailed = 2,
        FirmwareUpdatNotSupportedForThisDevice = 3,
        FirmwareUpdateTimeout = 4,
        PCBatteryTooLow = 5,
        ConnectMultipleDocks = 6,
        NetworkDisconnection = 7,
        UserAborted = 8,
        UserAbortedFail = 9,
        Unknow = 99
    }
}