using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

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
        public SWUErrorCode SWUErrorCode { get; set; } = SWUErrorCode.Unknow;

        public string SoftwareName { get; set; } = string.Empty;
        public string SoftwareVersion { get; set; } = string.Empty;
        public string TheLatestVersion { get; set; } = string.Empty;
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
        public string SHA512 { get; set; } = string.Empty;
        public string Thumbprint { get; set; } = string.Empty;
        public string Available_date { get; set; } = string.Empty;

        public bool Equals(SWUpdateInfo swUpdateInfo)
        {
            return swUpdateInfo.SoftwareName == SoftwareName;
        }
    }

    //0531 Bruce 因應IL的現有安裝包修改判斷，FWUpdateInfoPackage.cs中新增FWUErrorCode矩陣
    public enum SWUErrorCode
    {
        NoError = 0,
        DeviceDisconnected = 1,
        SoftwareUpdateFailed = 2,
        SoftwareUpdatNotSupportedForThisDevice = 3,
        SoftwareUpdateTimeout = 4,
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
        ServiceNotRunning = 15,

        #region InstallScript error code
        ERROR_SUCCESS_REBOOT_REQUIRED = 3010,
        ERROR_SUCCESS_REBOOT_INITIATED = 1641,
        /// <summary>
        /// 0x80042000
        /// </summary>
        ISERR_SETUP_CANCELED = 16,
        /// <summary>
        /// 0x80040708
        /// </summary>
        CheckPrivileges = 17,
        GenericError = -5001,
        Failed_reading_media_header = -5002,
        Failed_installing_kernel = -5003,
        Failed_starting_kernel = -5004,
        Failed_opening_CAB = -5005,
        Failed_installing_support = -5006,
        Failed_setting_text_substitution = -5007,
        Failed_initializing_installation_information = -5008,
        Failed_getting_installation_driver = -5009,
        Failed_initializing_properties = -5010,
        Failed_running_installation_driver = -5011,
        Failed_uninstalling_support = -5012,
        Failed_to_extract_file_from_setup_boot_file = -5013,
        Failed_to_download_file = -5014,
        Could_not_clone_the_installation = -5017,
        Failed_starting_the_setup_launcher = -6001,
        Failed_finding_the_setup_launcher = -6002,
        Failed_loading_the_setup_launcher = -6003,
        Failed_verifying_the_signature_of_setup_launcher = -6004,
        Failed_installing_the_setup_launcher_to_proper_location = -6005,
        Failed_extracting_setup_launcher = -6006,
        #endregion InstallScript error code 

        Unknow = 99
    }

    /// <summary>
    /// Metadata結構
    /// </summary>
    public class ChangeLog
    {
        [JsonPropertyName("version")]
        public string Version { get; set; }
        [JsonPropertyName("server_path")]
        public string ServerPath { get; set; }
        [JsonPropertyName("supportedOS")]
        public List<string> SupportedOS { get; set; }
        [JsonPropertyName("minimumSoftware")]
        public string MinimumSoftware { get; set; }
    }

    /// <summary>
    /// Metadata結構
    /// </summary>
    public class Software
    {
        public string SoftwareName { get; set; }
        public string SoftwareVersion { get; set; }
        [JsonPropertyName("server_path")]
        public string ServerPath { get; set; }
        public string SHA256 { get; set; }
        public string SHA512 { get; set; }
        public string Thumbprint { get; set; }
        public string DdpmSwUpdaterServer_path { get; set; }
        public string DdpmSwUpdater_SHA256 { get; set; }
        public string DdpmSwUpdater_SHA512 { get; set; }
        public string DdpmSwUpdater_Thumbprint { get; set; }
        [JsonPropertyName("install_path")]
        public string InstallPath { get; set; }
        [JsonPropertyName("supportedOS")]
        public List<string> SupportedOS { get; set; }
        [JsonPropertyName("minimumSoftware")]
        public string MinimumSoftware { get; set; }
    }
    /// <summary>
    /// Metadata結構
    /// </summary>
    public class SWUpdateHelper
    {
        [JsonPropertyName("version")]
        public string Version { get; set; }
        public ChangeLog ChangeLog { get; set; }
        public List<Software> Softwares { get; set; }
    }
}