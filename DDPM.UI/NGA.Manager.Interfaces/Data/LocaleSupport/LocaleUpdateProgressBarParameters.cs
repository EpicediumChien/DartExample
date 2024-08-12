#region LicenceHeader
//
// Copyright © 2022, Dell Inc., All Rights Reserved.
// This material is confidential and a trade secret.  Permission to use this
// work for any purpose must be obtained in writing from Dell Inc.
//
#endregion

using Microsoft;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Globalization;

namespace NGA.Manager.Interfaces
{
    /// <summary>
    /// LocaleUpdateProgressBarParameters Class contains UpdateProgressBarParameters and localized texts
    /// </summary>
    public class LocaleUpdateProgressBarParameters
    {
        #region Properties

        /// <summary>
        /// Default UpdateProgressBarParameters
        /// </summary>
        [JsonProperty]
        public UpdateProgressBarParameters DefaultParameters { get; protected set; }

        /// <summary>
        /// Localized UpdateProgressBarParameters Dictionary
        /// </summary>
        [JsonProperty]
        public Dictionary<CultureInfo, UpdateProgressBarParameters> LocalizedParameters { get; protected set;}

        #endregion

        /// <summary>
        /// LocaleUpdateProgressBarParameters
        /// </summary>
        [JsonConstructor]
        protected LocaleUpdateProgressBarParameters()
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="defaultParameters"></param>
        /// <param name="localizedParameters"></param>
        public LocaleUpdateProgressBarParameters(UpdateProgressBarParameters defaultParameters, 
            Dictionary<CultureInfo, UpdateProgressBarParameters> localizedParameters = null)
        {
            Requires.NotNull(defaultParameters, nameof(defaultParameters));

            DefaultParameters = defaultParameters;
            LocalizedParameters = localizedParameters;
        }
    }
}
