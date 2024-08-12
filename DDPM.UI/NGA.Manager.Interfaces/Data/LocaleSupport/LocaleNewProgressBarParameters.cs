#region LicenceHeader
//
// Copyright © 2022, Dell Inc., All Rights Reserved.
// This material is confidential and a trade secret.  Permission to use this
// work for any purpose must be obtained in writing from Dell Inc.
//
#endregion

using System.Collections.Generic;
using System.Globalization;
using Microsoft;
using Newtonsoft.Json;

namespace NGA.Manager.Interfaces
{
    /// <summary>
    /// LocaleNewProgressBarParameters Class contains NewProgressBarParameters and localized texts
    /// </summary>
    public class LocaleNewProgressBarParameters
    {
        #region Properties

        /// <summary>
        /// Default NewProgressBarParameters
        /// </summary>
        [JsonProperty]
        public NewProgressBarParameters DefaultParameters { get; protected set; }

        /// <summary>
        /// Localized NewProgressBarParameters Dictionary
        /// </summary>
        [JsonProperty]
        public Dictionary<CultureInfo, NewProgressBarParameters> LocalizedParameters { get; protected set; }

        #endregion

        /// <summary>
        /// LocaleNewProgressBarParameters
        /// </summary>
        [JsonConstructor]
        protected LocaleNewProgressBarParameters()
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="defaultParameters"></param>
        /// <param name="localizedParameters"></param>
        public LocaleNewProgressBarParameters(NewProgressBarParameters defaultParameters,
            Dictionary<CultureInfo, NewProgressBarParameters> localizedParameters)
        {
            Requires.NotNull(defaultParameters, nameof(defaultParameters));

            DefaultParameters = defaultParameters;
            LocalizedParameters = localizedParameters;
        }
    }
}
