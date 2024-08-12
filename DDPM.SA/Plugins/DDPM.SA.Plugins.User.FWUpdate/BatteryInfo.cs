using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace DDPM.SA.Plugins.User.FWUpdate
{
    public class BatteryInfo
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct SYSTEM_POWER_STATUS
        {
            public ACLineStatus ACLineStatus;
            public BatteryFlag BatteryFlag;
            public byte BatteryLifePercent;
            public SystemStatusFlag SystemStatusFlag;
            public int BatteryLifeTime;
            public int BatteryFullLifeTime;
        }
        public enum ACLineStatus : byte
        {
            Offline = 0,
            Online = 1,
            UnknowStatus = 255
        }
        public enum BatteryFlag : byte
        {
            /// <summary>
            /// 電池未充電且電池容量介於高和低電量之間
            /// </summary>
            Middle = 0,
            /// <summary>
            /// 電池電量超過66%
            /// </summary>
            High = 1,
            /// <summary>
            /// 電池電量不足33%
            /// </summary>
            Low = 2,
            /// <summary>
            /// 電池電量不足5%
            /// </summary>
            Critical = 4,
            /// <summary>
            /// 充電中
            /// </summary>
            Charging = 8,
            /// <summary>
            /// 無系統電池
            /// </summary>
            NoSystemBattery = 128,
            /// <summary>
            /// 無法取得電池資訊
            /// </summary>
            UnknowStatus = 255
        }
        public enum SystemStatusFlag : byte
        {
            /// <summary>
            /// 省電功能已關閉
            /// </summary>
            Off = 0,
            /// <summary>
            /// 省電功能已開啟
            /// </summary>
            On = 1
        }
        [DllImport("kernel32.dll")]
        public static extern bool GetSystemPowerStatus(out SYSTEM_POWER_STATUS status);
        public bool GetBatteryInfo(out SYSTEM_POWER_STATUS powerStatus)
        {
            if (GetSystemPowerStatus(out powerStatus))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
