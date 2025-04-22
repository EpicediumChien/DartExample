using DPeMPublic.Common.Enums;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace DDPM.SA.Common
{
    /// <summary>
    /// 韌體更新資訊包
    /// </summary>
    public class FWUpdateInfoPackage
    {
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
            TheLastCheckTime = new DateTime();
            FWUpdateInfo = new List<FWUpdateInfo>();
        }
        public void Clear()
        {
            FWUpdateInfo.Clear();
            TheLastCheckTime = new DateTime();
        }
    }

    public class DokcUODUpdateInfoPackage
    {
        /// <summary>
        /// 將該韌體更新資訊包儲存進設定檔的日期
        /// </summary>
        public DateTime? SaveTime { get; set; }

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
        /// add @ 20241201 stephen: for CMA feedback
        /// </summary>
        public string Guid { get; set; } = string.Empty;

        /// <summary>
        /// 韌體更新的錯誤碼，安裝時使用
        /// </summary>
        public FWUErrorCode FWUErrorCode { get; set; } = FWUErrorCode.Unknow;
        public DeviceType DeviceType { get; set; } = DeviceType.Unknown;
        public bool IsDisplay { get; set; } = false;
        public string DeviceName { get; set; } = string.Empty;
        public string DeviceId { get; set; } = string.Empty;
        public int DeviceIndex { get; set; } = 0;
        public string DevicePath { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public string DeviceVersion { get; set; } = string.Empty;
        public string TheLatestVersion { get; set; } = string.Empty;

        /// <summary>
        /// Dock專用，判斷是否斷線更新
        /// </summary>
        public bool IsUOD { get; set; } = false;
        /// <summary>
        /// Only for webcam true is HPD can name pipa update, false is MPS need client update
        /// </summary>
        public bool IsESISupported { get; set; } = false;

        /// <summary>
        /// 裝置例項路徑，Dock專用，用於判斷UOD的Dock再次連接時是否更新完成
        /// </summary>
        public string PNPDeviceID { get; set; } = string.Empty;

        public bool NeedUpdated { get; set; } = false;

        /// <summary>
        /// 安裝時使用，獲取目前的進度資訊
        /// </summary>
        public string ProcessName { get; set; } = string.Empty;

        /// <summary>
        /// 安裝時使用，獲取安裝進度
        /// </summary>
        public double ProcessProgress { get; set; } = 0.0;

        public string ServerPath { get; set; } = string.Empty;
        public string FileSavepath { get; set; } = string.Empty;
        public string InstallPaths { get; set; } = string.Empty;
        public string SHA256 { get; set; } = string.Empty;
        //public string SHA512 { get; set; }
        public string Thumbprint { get; set; } = string.Empty;
        public string ServiceTag { get; set; } = string.Empty;
        public string SupplierID { get; set; } = string.Empty;
        public int InstanceId { get; set; } = 0;
        public string D_Ctrl { get; set; } = string.Empty;
        public string Connectivity { get; set; } = string.Empty;
        public string Update_date { get; set; } = string.Empty;
        public string Available_date { get; set; } = string.Empty;
        public string UpdateTime { get; set; } = string.Empty;

        public bool Equals(FWUpdateInfo fwUpdateInfo)
        {
            if (fwUpdateInfo.IsDisplay)
            {
                return fwUpdateInfo.Model == Model && fwUpdateInfo.ServiceTag == ServiceTag;
            }
            else
            {
                return fwUpdateInfo.DevicePath == DevicePath && fwUpdateInfo.Model == Model;
            }
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
        FolderIsNotSafe = 10,
        FileIsNoSafe = 11,
        CAFail = 12,
        NamedPipeServerIsNoSafe = 13,
        FileCheckFail = 14,
        ConnectMultipleSameModels = 15,
        DeviceBatteryTooLow = 16,
        Service_not_running_Try_again = 17,
        DeviceIsEnterSleepMode = 18,
        Unknow = 99
    }

    /// <summary>
    /// Display FWU Metadata結構
    /// </summary>
    public class Display_Firmwares_item
    {
        public string id { get; set; }
        [JsonPropertyName("version")]
        public string TheLastVersion { get; set; }
        public string CurrentVersion { get; set; }
        public string fileName { get; set; }
        public string SHA256 { get; set; }
        public string SHA512 { get; set; }
        [JsonPropertyName("thumbprint")]
        public string Thumbprint { get; set; }
        public string url { get; set; }
        public string date { get; set; }
        [JsonPropertyName("support_platform")]
        public string SupportedPlatform { get; set; }
        public string ServiceTag { get; set; }
        public string SupplierID { get; set; }
        public string D_Ctrl { get; set; }
        [JsonPropertyName("UpdateTime")]
        public string UpdateTime { get; set; }
    }

    /// <summary>
    /// Display FWU Metadata結構
    /// </summary>
    public class DisplayUpdateHelper
    {
        public List<Display_Firmwares_item> Firmwares { get; set; }
        public DisplayUpdateHelper()
        {
            Firmwares = new List<Display_Firmwares_item>();
        }
    }
    public class UpdateProgressInfo
    {
        public string DeviceName { get; set; } = string.Empty;
        public string DeviceId { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public string DeviceVersion { get; set; } = string.Empty;
        public string TheLatestVersion { get; set; } = string.Empty;

        /// <summary>
        /// 安裝時使用，獲取目前的進度資訊
        /// </summary>
        public string ProcessName { get; set; } = string.Empty;

        /// <summary>
        /// 安裝時使用，獲取安裝進度
        /// </summary>
        public double ProcessProgress { get; set; } = 0.0;
        public bool IsDisplay { get; set; } = false;
        public string UpdateTime { get; set; } = string.Empty;
    }
}