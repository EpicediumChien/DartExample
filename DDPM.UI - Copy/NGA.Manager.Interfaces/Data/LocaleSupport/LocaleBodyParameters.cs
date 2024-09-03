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
    /// LocaleBodyParameters Class contains BodyParameters and localized texts
    /// </summary>
    public class LocaleBodyParameters
    {
        #region Properties

        /// <summary>
        /// Default BodyParameters
        /// </summary>
        [JsonProperty]
        public BodyParameters DefaultParameters { get; protected set; }

        /// <summary>
        /// Localized BodyParameters
        /// </summary>
        [JsonProperty]
        public Dictionary<CultureInfo, BodyParameters> LocalizedParameters { get; protected set; }

        #endregion

        /// <summary>
        /// LocaleBodyParameters
        /// </summary>
        [JsonConstructor]
        protected LocaleBodyParameters()
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="defaultParameters"></param>
        /// <param name="localizedParameters"></param>
        public LocaleBodyParameters(BodyParameters defaultParameters, Dictionary<CultureInfo, BodyParameters> localizedParameters)
        {
            Requires.NotNull(defaultParameters, nameof(defaultParameters));

            DefaultParameters = defaultParameters;
            LocalizedParameters = localizedParameters;
        }
    }
}