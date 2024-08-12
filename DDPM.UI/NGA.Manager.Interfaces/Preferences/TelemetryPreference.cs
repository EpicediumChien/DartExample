#region LicenceHeader
//
// Copyright © 2022, Dell Inc., All Rights Reserved.
// This material is confidential and a trade secret.  Permission to use this
// work for any purpose must be obtained in writing from Dell Inc.
//
#endregion

using System;
using Newtonsoft.Json;

namespace NGA.Manager.Interfaces
{
    /// <summary>
    /// TelemetryPreference class
    /// </summary>
    /// Commented below to get build pass
    [Obsolete("TelemetryPreference in NGA.Manager.Interfaces is deprecated. Please use the ITelemetryConsent plugin instead.")]
    public class TelemetryPreference
    {
        /// <summary>
        /// CustomerExperienceImprovement property
        /// </summary>
        [JsonProperty]
        public bool CustomerExperienceImprovement { get; set; }
    }
}