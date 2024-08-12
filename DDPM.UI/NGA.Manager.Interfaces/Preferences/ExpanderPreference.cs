using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NGA.Manager.Interfaces
{
    /// <summary>
    /// Expander Preference class
    /// </summary>
    public class ExpanderPreference
    {
        /// <summary>
        /// IsPrivacyNoticeExpanded property
        /// </summary>
        [JsonProperty]
        public bool IsPrivacyNoticeExpanded{ get; set; }

        /// <summary>
        /// IsNotificationsExpanded property
        /// </summary>
        [JsonProperty]
        public bool IsNotificationsExpanded { get; set; }

        /// <summary>
        /// IsCriticalAlertsExpanded property
        /// </summary>
        [JsonProperty]
        public bool IsCriticalAlertsExpanded { get; set; }
    }
}
