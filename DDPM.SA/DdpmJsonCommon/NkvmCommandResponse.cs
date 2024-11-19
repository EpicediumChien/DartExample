using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace DdpmJsonCommon
{
    /// <summary>
    /// DDPM -> NKVM. NKVM 的功能開啟
    /// <code>
    /// {
    ///   "type": "ON_NKVM",
    ///   "cid": 789,
    /// }
    /// </code>
    /// </summary>
    [JsonTypeIdentifier]
    public class ON_NKVM : DDM_COMMAND
    {
    }

    /// <summary>
    /// NKVM -> DDPM. Response from <see cref="ON_NKVM">START_NKVM</see>
    /// <code>
    /// {
    ///   "type": "ON_NKVM_RESPONSE",
    ///   "cid": 789,
    ///   "Success": true,
    /// }
    /// </code>
    /// </summary>
    [JsonTypeIdentifier]
    public class ON_NKVM_RESPONSE : DDM_RESPONSE
    {
    }

    /// <summary>
    /// DDPM -> NKVM. NKVM 的功能關閉
    /// <code>
    /// {
    ///   "type": "OFF_NKVM",
    ///   "cid": 789,
    /// }
    /// </code>
    /// </summary>
    [JsonTypeIdentifier]
    public class OFF_NKVM : DDM_COMMAND
    {
    }

    /// <summary>
    /// NKVM -> DDPM. Response from <see cref="OFF_NKVM">STOP_NKVM</see>
    /// <code>
    /// {
    ///   "type": "OFF_NKVM_RESPONSE",
    ///   "cid": 789,
    ///   "Success": true,
    /// }
    /// </code>
    /// </summary>
    [JsonTypeIdentifier]
    public class OFF_NKVM_RESPONSE : DDM_RESPONSE
    {
    }

    /// <summary>
    /// DDPM -> NKVM. Get NKVM version
    /// <code>
    /// {
    ///   "type": "GET_NKVM_VERSION",
    ///   "cid": 123,
    /// }
    /// </code>
    /// </summary>
    [JsonTypeIdentifier]
    public class GET_NKVM_VERSION : DDM_COMMAND
    {
    }

    [JsonTypeIdentifier]
    public class GET_NKVM_VERSION_RESPONSE : DDM_RESPONSE
    {
        public string Version { get; set; }
    }

    [JsonTypeIdentifier]
    public class GET_NKVM_STATUS : DDM_COMMAND
    {
    }

    [JsonTypeIdentifier]
    public class GET_NKVM_STATUS_RESPONSE : DDM_RESPONSE
    {
        public static readonly string StatusOn = "On";
        public static readonly string StatusOff = "Off";

        public string Status { get; set; }
    }

    [JsonTypeIdentifier]
    public class GET_NKVM_AUTO_CONNECT : DDM_COMMAND
    {
    }

    [JsonTypeIdentifier]
    public class GET_NKVM_AUTO_CONNECT_RESPONSE : DDM_RESPONSE
    {
        public bool Enable { get; set; }
    }

    [JsonTypeIdentifier]
    public class GET_NKVM_CONTENT_TRANSFER : DDM_COMMAND
    {
    }

    [JsonTypeIdentifier]
    public class GET_NKVM_CONTENT_TRANSFER_RESPONSE : DDM_RESPONSE
    {
        public bool Enable { get; set; }
    }

    [JsonTypeIdentifier]
    public class GET_NKVM_INCOMMING_PORT : DDM_COMMAND
    {
    }

    [JsonTypeIdentifier]
    public class GET_NKVM_INCOMMING_PORT_RESPONSE : DDM_RESPONSE
    {
        public int Port { get; set; }
    }

    [JsonTypeIdentifier]
    public class GET_NKVM_OUTGOING_PORT : DDM_COMMAND
    {
    }

    [JsonTypeIdentifier]
    public class GET_NKVM_OUTGOING_PORT_RESPONSE : DDM_RESPONSE
    {
        public int Port { get; set; }
    }

    [JsonTypeIdentifier]
    public class GET_NKVM_CONTENT_TRANSFER_PORT : DDM_COMMAND
    {
    }

    [JsonTypeIdentifier]
    public class GET_NKVM_CONTENT_TRANSFER_PORT_RESPONSE : DDM_RESPONSE
    {
        public int Port { get; set; }
    }

    /// <summary>
    /// DDPM -> NKVM. Get NKVM settings
    /// <code>
    /// {
    ///   "type": "GET_NKVM_SETTINGS",
    ///   "cid": 123,
    /// }
    /// </code>
    /// </summary>
    [JsonTypeIdentifier]
    public class GET_NKVM_SETTINGS : DDM_COMMAND
    {
    }

    /// <summary>
    /// NKVM -> DDPM. Response from <see cref="GET_NKVM_SETTINGS">GET_NKVM_SETTINGS</see>
    /// Response is limited to 4096 bytes.
    /// <code>
    /// {
    ///   "type": "GET_NKVM_SETTINGS_RESPONSE",
    ///   "cid": 123,
    ///   "Success": true,
    ///   "Version": "2.3.4.5",
    ///   "AutoConnect": false,
    ///   "ContentTransfer": true,
    ///   "IncommingPort": 5566,
    ///   "OutgoingPort": 5567,
    ///   "ContentTransferPort": 5568
    /// }
    /// </code>>
    /// </summary>
    [JsonTypeIdentifier]
    public class GET_NKVM_SETTINGS_RESPONSE : DDM_RESPONSE
    {
        public string Version { get; set; }
        public bool AutoConnect { get; set; }
        public bool ContentTransfer { get; set; }
        public int IncommingPort { get; set; }
        public int OutgoingPort { get; set; }
        public int ContentTransferPort { get; set; }
    }

    /// <summary>
    /// DDPM -> NKVM. Get NKVM hotkey settings
    /// <code>
    /// {
    ///   "type": "GET_NKVM_HOTKEY_SETTINGS",
    ///   "cid": 123,
    /// }
    /// </code>
    /// </summary>
    [JsonTypeIdentifier]
    public class GET_NKVM_HOTKEY_SETTINGS : DDM_COMMAND
    {
    }

    /// <summary>
    /// NKVM -> DDPM. Response from <see cref="GET_NKVM_HOTKEY_SETTINGS">GET_NKVM_HOTKEY_SETTINGS</see>
    /// Response is limited to 4096 bytes.
    /// <code>
    /// {
    ///   "type": "GET_NKVM_HOTKEY_SETTINGS_RESPONSE",
    ///   "cid": 123,
    ///   "Success": true,
    ///   "Hotkey": [
    ///     {
    ///       "Id": "KvmToggleInputSource",
    ///       "Control": true,
    ///       "Alt": false,
    ///       "Shift": true,
    ///       "Key": 65
    ///     },
    ///     {
    ///       "Id": "KvmRestoreMouseCursor",
    ///       "Control": false,
    ///       "Alt": true,
    ///       "Shift": false,
    ///       "Key": 66
    ///     }
    ///     // More hotkeys can be listed here
    ///   ]
    /// }
    /// </code>
    /// </summary>
    [JsonTypeIdentifier]
    public class GET_NKVM_HOTKEY_SETTINGS_RESPONSE : DDM_RESPONSE
    {
        public List<HotkeyWinform> Hotkey { get; set; }
    }

    /// <summary>
    /// DDPM --> NKVM
    /// 列出 DDPM 曾接過的支援的 monitor list
    /// Please use inside methods, CalculateChecksum and IsChecksumValid, to calculate and validate checksum
    /// <code>
    /// {
    ///     "type": "UPDATE_SUPPORTED_MONITOR_LIST",
    ///     "Monitors": ["U4320Q", "U2720Q"]
    ///     "Checksum": [...]
    /// }
    /// </code>>
    /// </summary>
    [JsonTypeIdentifier]
    public class UPDATE_SUPPORTED_MONITOR_LIST : DDM_COMMAND
    {
        public List<string> Monitors { get; set; }
    }

    [JsonTypeIdentifier]
    public class UPDATE_SUPPORTED_MONITOR_LIST_RESPONSE : DDM_RESPONSE
    {
    }
    /// <summary>
    /// DDPM --> NKVM
    /// DDPM ShowNkvm
    /// <code>
    /// {
    ///   "type": "SHOW_NKVM" 
    /// }
    /// </code>
    /// </summary>
    [JsonTypeIdentifier]
    public class SHOW_NKVM : DDM_COMMAND
    {
        public int MonitorNum { get; set; }
        public int MonitorX { get; set; }
        public int MonitorY { get; set; }
    }
    [JsonTypeIdentifier]
    public class SHOW_NKVM_RESPONSE : DDM_RESPONSE
    {
    }
}
