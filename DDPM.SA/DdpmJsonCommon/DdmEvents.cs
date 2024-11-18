using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// 以 stream 傳送 command/response/event, 這裡定義 event 的格式
/// 格式是 4 bytes json message 長度, 隨後 json message, 如下
/// 0x0000003D
/// {
///   "type": "SET_VCP_NOTIFY",
///   "MonitorIndex": 1,
///   "VcpCode": 123,
///   "Value": 456
/// }
/// 以下 暫時不顯示前方的 4 bytes
/// </summary>
namespace DdpmJsonCommon
{
    /// <summary>
    /// DDPM --> NKVM
    /// DDPM 有設定 VCP 要通知 NKVM, SET VCP 成功再通知, 如果是 NKVM 送 SET_VCP command 給 DDPM 的不用通知
    /// NKVM 只需要知道有設定下列 VCP codes
    ///     0x60(InputSelect), 
    ///     0xE8(PIPPBPInput), 
    ///     0xE9(PIPPBPMode), 
    ///     0xE5(PIPPBPVideoSwap), 
    ///     0x04(RestoreFactoryDefaults)
    /// <code>
    /// {
    ///   "type": "SET_VCP_NOTIFY",
    ///   "MonitorIndex": 1,
    ///   "VcpCode": 96,
    ///   "Value": 3857
    /// }
    /// </code>
    /// </summary>
    [JsonTypeIdentifier]
    public class SET_VCP_NOTIFY : DDM_EVENT
    {
        public int MonitorIndex { get; set; }
        public int VcpCode { get; set; }
        public int Value { get; set; }
    }

    /// <summary>
    /// DDPM --> NKVM
    /// 螢幕插拔
    /// <code>
    /// {
    ///   "type": "MONITOR_PLUG_DETECTION" 
    /// }
    /// </code>
    /// </summary>
    [JsonTypeIdentifier]
    public class MONITOR_PLUG_DETECTION : DDM_EVENT
    {
    }

    /// <summary>
    /// DDPM --> NKVM
    /// DDPM UI選擇螢幕
    /// <code>
    /// {
    ///   "type": "CHANGE_MONITOR_ID",
    ///   "MonitorId": 1    // 新選的 monitor index
    /// }
    /// </code>
    /// </summary>
    [JsonTypeIdentifier]
    public class CHANGE_MONITOR_ID : DDM_EVENT
    {
        public int MonitorId { get; set; }
    }

    /// <summary>
    /// DDPM --> NKVM
    /// 有螢幕的 limted SW 改變
    /// <code>
    /// {
    ///   "type": "CHANGE_LIMITED_SW",
    ///   "MonitorIndex": 0,    // 那個 monitor index 的 limited SW 改變
    ///   "LimitedSW": true,    // 
    /// }
    /// </code>
    /// </summary>
    [JsonTypeIdentifier]
    public class CHANGE_LIMITED_SW : DDM_EVENT
    {
        public int MonitorIndex { get; set; }

        public bool LimitedSW { get; set; }
    }

}
