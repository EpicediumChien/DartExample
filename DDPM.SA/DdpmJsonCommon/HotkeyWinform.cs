using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DdpmJsonCommon
{
    public class HotkeyWinform
    {
        public enum HotkeyId : int
        {
            None = 0,
            KvmToggleInputSource = 1,
            KvmRestoreMouseCursor = 2,
        }

        [JsonConverter(typeof(StringEnumConverter))]
        public HotkeyId Id { get; set; }

        public bool Control { get; set; }
        public bool Alt { get; set; }
        public bool Shift { get; set; }

        /// <summary>
        /// same value as System.Windows.Forms.Keys enum
        /// </summary>
        public int Key { get; set; }
    }
}
