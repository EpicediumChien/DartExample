#region LicenceHeader

//
// Copyright © 2022, Dell Inc., All Rights Reserved.
// This material is confidential and a trade secret.  Permission to use this
// work for any purpose must be obtained in writing from Dell Inc.
//

#endregion

using Microsoft;
using Newtonsoft.Json;
using System.Globalization;

namespace NGA.Manager.Interfaces
{
    /// <summary>
    /// LocaleNewProgressBarWithButtonsParameters Class contains NewProgressBarWithButtonsParameters and localized texts
    /// </summary>
    public class LocaleNewProgressBarWithButtonsParameters
    {
        #region Properties

        /// <summary>
        /// Default NewProgressBarWithButtonsParameters
        /// </summary>
        [JsonProperty]
        public NewProgressBarWithButtonsParameters DefaultParameters { get; protected set; }

        /// <summary>
        /// Localized NewProgressBarWithButtonsParameters Dictionary
        /// </summary>
        [JsonProperty]
        public Dictionary<CultureInfo, NewProgressBarWithButtonsParameters> LocalizedParameters { get; protected set; }

        #endregion

        /// <summary>
        /// LocaleNewProgressBarWithButtonsParameters
        /// </summary>
        [JsonConstructor]
        protected LocaleNewProgressBarWithButtonsParameters()
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="defaultParameters"></param>
        /// <param name="localizedParameters"></param>
        public LocaleNewProgressBarWithButtonsParameters(NewProgressBarWithButtonsParameters defaultParameters,
            Dictionary<CultureInfo, NewProgressBarWithButtonsParameters> localizedParameters)
        {
            Requires.NotNull(defaultParameters, nameof(defaultParameters));

            DefaultParameters = defaultParameters;
            LocalizedParameters = localizedParameters;
        }
    }
}