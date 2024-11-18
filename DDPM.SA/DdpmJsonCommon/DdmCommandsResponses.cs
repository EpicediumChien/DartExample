using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// 以 stream 傳送 command/response/event, 這裡定義 command/response 的格式
/// 格式是 4 bytes json message 長度, 隨後 json message, 如下
/// 0x0000003D
/// {
///   "type": "SET_VCP",
///   "cid": 789,
///   "MonitorIndex": 1,
///   "VcpCode": 123,
///   "Value": 456
/// }
/// 以下 暫時不顯示前方的 4 bytes
/// </summary>
namespace DdpmJsonCommon
{
    /// <summary>
    /// NKVM --> DDPM
    /// 因為可以同時接多台Dell monitor, current monitor index 是目前 DDPM 選擇的 monitor index
    /// <code>
    /// {
    ///   "type": "GET_CURRENT_MONITOR_INDEX",    // 因為可以同時接多台Dell monitor, current monitor index 是目前 DDPM 選擇的 monitor index
    ///   "cid": 789,
    /// }
    /// </code>
    /// </summary>
    [JsonTypeIdentifier]
    public class GET_CURRENT_MONITOR_INDEX : DDM_COMMAND
    {
    }

    /// <summary>
    /// DDPM --> NKVM. Response from <see cref="GET_CURRENT_MONITOR_INDEX">GET_CURRENT_MONITOR_INDEX</see>
    /// <code>
    /// {
    ///   "type": "GET_CURRENT_MONITOR_INDEX_RESPONSE",
    ///   "cid": 789,
    ///   "Success": true,    // 表示執行指令有無成功
    ///   "MonitorIndex": 1
    /// }
    /// </code>
    /// </summary>
    [JsonTypeIdentifier]
    public class GET_CURRENT_MONITOR_INDEX_RESPONSE : DDM_RESPONSE
    {
        public int MonitorIndex { get; set; }
    }

    /// <summary>
    /// NKVM --> DDPM
    /// <code>
    /// {
    ///   "type": "GET_MONITOR_INFO" 
    ///   "cid": 789,
    /// }
    /// </code>
    /// </summary>
    [JsonTypeIdentifier]
    public class GET_MONITOR_INFO : DDM_COMMAND
    {
    }

    /// <summary>
    /// GET_MONITOR_INFO_RESPONSE 內的 Monitors array 的元素(item)內容
    /// </summary>
    public class Monitor
    {
        /// <summary>
        /// 用來區分是那一台 monitor
        /// </summary>
        public int Index { get; set; }

        /// <summary>
        /// ex: "U4320Q"
        /// </summary>
        public string ModelName { get; set; }

        /// <summary>
        /// ex: "875640396"
        /// </summary>
        public string SerialNumber { get; set; }

        /// <summary>
        /// ex: "JRK1TS2"
        /// </summary>
        public string ServiceTag { get; set; }

        /// <summary>
        /// ex: "\\\\.\\DISPLAY3"
        /// </summary>
        public string DeviceName { get; set; }

        /// <summary>
        /// ex: "(prot(monitor)type(lcd)model(U4320Q)cmds(01 02 03 07 0C E3 F3)vcp(02 04 05 08 10 12 14(04 05 06 08 09 0B 0C) 16 18 1A 52 60( 1B 0F 13 11 12) 62 AC AE B2 B6 C6 C8 C9 CC(02 03 04 06 09 0A 0D 0E) D6(01 04 05) DC(00 ) DF E0 E1 E2(00 1D 0C 0D 0F 10 11 13 14) E5 E8 E9(00 01 02 21 22 24 41 34 33 32) F0(00 0C) F1 F2 FD)mccs_ver(2.1)mswhql(1))"
        /// </summary>
        public string CapabilityString { get; set; }

        /// <summary>
        /// 表示無法對此 monitor 下 ddcci 指令, 有可能是 ddcci off, iMST on, ...
        /// </summary>
        public bool LimitedSW { get; set; }

        /// <summary>
        /// 內部多重串流傳輸 On or Off
        /// </summary>
        public bool iMST { get; set; }

        public string ToJson()
        {
            return JsonConvert.SerializeObject(this);
        }
    }

    /// <summary>
    /// DDPM --> NKVM. Response from <see cref="GET_MONITOR_INFO">GET_MONITOR_INFO</see>
    /// <code>
    /// {
    ///   "type": "GET_MONITOR_INFO_RESPONSE",
    ///   "cid": 789,
    ///   "Success": true,
    ///   // 因為可以同時接多台Dell monitor, 以 array 列出
    ///   "Monitors": [
    ///     {
    ///       "Index": 0,    // 用來區分是那一台 monitor
    ///       "ModelName": "U4320Q",
    ///       "SerialNumber": "875640396",
    ///       "ServiceTag": "JRK1TS2",
    ///       "DeviceName": "\\\\.\\DISPLAY3",
    ///       "CapabilityString": "(prot(monitor)type(lcd)model(U4320Q)cmds(01 02 03 07 0C E3 F3)vcp(02 04 05 08 10 12 14(04 05 06 08 09 0B 0C) 16 18 1A 52 60( 1B 0F 13 11 12) 62 AC AE B2 B6 C6 C8 C9 CC(02 03 04 06 09 0A 0D 0E) D6(01 04 05) DC(00 ) DF E0 E1 E2(00 1D 0C 0D 0F 10 11 13 14) E5 E8 E9(00 01 02 21 22 24 41 34 33 32) F0(00 0C) F1 F2 FD)mccs_ver(2.1)mswhql(1))" 
    ///       "LimitedSW": false,    // 表示無法對此 monitor 下 ddcci 指令, 有可能是 ddcci off, iMST on, …
    ///       "iMST": false    // 內部多重串流傳輸 On or Off
    ///     },
    ///     // More monitors...
    ///   ]
    /// }
    /// </code>>
    /// </summary>
    [JsonTypeIdentifier]
    public class GET_MONITOR_INFO_RESPONSE : DDM_RESPONSE
    {
        /// <summary>
        /// 因為可以同時接多台Dell monitor, 以 array 列出
        /// </summary>
        public List<Monitor> Monitors { get; set; }
    }

    /// <summary>
    /// NKVM --> DDPM
    /// <code>
    /// {
    ///   "type": "SET_VCP",
    ///   "cid": 789,
    ///   "MonitorIndex": 1,    // 指定對那一台 monitor 下 ddcci 指令
    ///   "VcpCode": 123,
    ///   "Value": 456
    /// }
    /// </code>
    /// </summary>
    [JsonTypeIdentifier]
    public class SET_VCP : DDM_COMMAND
    {
        public int MonitorIndex { get; set; }

        /// <summary>
        /// 指定對那一台 monitor 下 ddcci 指令
        /// </summary>
        public int VcpCode { get; set; }
        
        public int Value { get; set; }
    }

    /// <summary>
    /// DDPM --> NKVM. Response from <see cref="SET_VCP">SET_VCP</see>
    /// <code>
    /// {
    ///   "type": "SET_VCP_RESPONSE",
    ///   "cid": 789,
    ///   "MonitorIndex": 1,
    ///   "VcpCode": 123,
    ///   "Value": 456,
    ///   "Success": true
    /// }
    /// </code>
    /// </summary>
    [JsonTypeIdentifier]
    public class SET_VCP_RESPONSE : DDM_RESPONSE
    {
        public int MonitorIndex { get; set; }
        public int VcpCode { get; set; }
        public int Value { get; set; }
    }

    /// <summary>
    /// NKVM --> DDPM
    /// <code>
    /// {
    ///   "type": "GET_VCP",
    ///   "cid": 789,
    ///   "MonitorIndex": 1,
    ///   "VcpCode": 123
    /// }
    /// </code>
    /// </summary>
    [JsonTypeIdentifier]
    public class GET_VCP : DDM_COMMAND
    {
        public int MonitorIndex { get; set; }
        public int VcpCode { get; set; }
    }

    /// <summary>
    /// DDPM --> NKVM. Response from <see cref="GET_VCP">GET_VCP</see>
    /// <code>
    /// {
    ///   "type": "GET_VCP_RESPONSE",
    ///   "cid": 789,
    ///   "MonitorIndex": 1,
    ///   "VcpCode": 123,
    ///   "Success": true,
    ///   "Value": 456
    /// }
    /// </code>
    /// </summary>
    [JsonTypeIdentifier]
    public class GET_VCP_RESPONSE : DDM_RESPONSE
    {
        public int MonitorIndex { get; set; }
        public int VcpCode { get; set; }
        public int Value { get; set; }
    }

    /// <summary>
    /// NKVM -> DDPM.
    /// <code>
    /// {
    ///   "type": "IS_HOTKEY_AVAILABLE",
    ///   "cid": 123,
    ///   "Hotkey": {
    ///     "Id": "KvmRestoreMouseCursor",
    ///     "Control": true,
    ///     "Alt": false,
    ///     "Shift": true,
    ///     "Key": 65
    ///   }
    /// }
    /// </code>
    /// </summary>
    [JsonTypeIdentifier]
    public class IS_HOTKEY_AVAILABLE : DDM_COMMAND
    {
        public HotkeyWinform Hotkey { get; set; }
    }

    /// <summary>
    /// DDPM -> NKVM. Response from <see cref="IS_HOTKEY_AVAILABLE">IS_HOTKEY_AVAILABLE</see>
    /// <code>
    /// {
    ///   "type": "IS_HOTKEY_AVAILABLE_RESPONSE",
    ///   "cid": 123,
    ///   "Success": true,
    ///   "Available": true,
    ///   "Hotkey": {
    ///     "Id": "None",
    ///     "Control": true,
    ///     "Alt": false,
    ///     "Shift": true,
    ///     "Key": 65
    ///   },
    /// }
    /// </code>
    /// </summary>
    [JsonTypeIdentifier]
    public class IS_HOTKEY_AVAILABLE_RESPONSE : DDM_RESPONSE
    {
        public bool Available { get; set; }
        public HotkeyWinform Hotkey { get; set; }
    }

    /// <summary>
    /// NKVM -> DDPM.
    /// <code>
    /// {
    ///   "type": "SET_HOTKEY",
    ///   "cid": 123,
    ///   "Hotkey": {
    ///     "Id": "KvmToggleInputSource",
    ///     "Control": true,
    ///     "Alt": false,
    ///     "Shift": true,
    ///     "Key": 65
    ///   }
    /// }
    /// </code>
    /// </summary>
    [JsonTypeIdentifier]
    public class SET_HOTKEY : DDM_COMMAND
    {
        public HotkeyWinform Hotkey { get; set; }
    }

    /// <summary>
    /// DDPM -> NKVM. Response from <see cref="SET_HOTKEY">SET_HOTKEY</see>
    /// <code>
    /// {
    ///   "type": "SET_HOTKEY_RESPONSE",
    ///   "cid": 123,
    ///   "Success": true,
    ///   "Hotkey": {
    ///     "Id": "KvmToggleInputSource",
    ///     "Control": true,
    ///     "Alt": false,
    ///     "Shift": true,
    ///     "Key": 65
    ///   }
    /// }
    /// </code>
    /// </summary>
    [JsonTypeIdentifier]
    public class SET_HOTKEY_RESPONSE : DDM_RESPONSE
    {
        public HotkeyWinform Hotkey { get; set; }
    }

    /// <summary>
    /// NKVM --> DDPM
    /// NKVM 在關閉之前會送出這個 command
    /// <code>
    /// {
    ///     "type": "DISCONNECT",
    ///     "cid": 123,
    /// }
    /// </code>
    /// </summary>
    [JsonTypeIdentifier]
    public class DISCONNECT : DDM_COMMAND
    {
    }

    /// <summary>
    /// DDPM --> NKVM. Response from <see cref="DISCONNECT">DISCONNECT</see>
    /// </summary>
    [JsonTypeIdentifier]
    public class DISCONNECT_RESPONSE : DDM_RESPONSE
    {
    }
}
