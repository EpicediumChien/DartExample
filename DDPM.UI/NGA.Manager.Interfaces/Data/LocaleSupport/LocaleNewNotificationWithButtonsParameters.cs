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
    /// LocaleNewNotificationWithButtonsParameters Class contains NewNotificationWithButtonsParameters and localized texts
    /// </summary>
    public class LocaleNewNotificationWithButtonsParameters
    {
        #region Properties

        /// <summary>
        /// Default NewNotificationWithButtonsParameters
        /// </summary>
        [JsonProperty]
        public NewNotificationWithButtonsParameters DefaultParameters { get; protected set; }

        /// <summary>
        /// Localized NewNotificationWithButtonsParameters Dictionary
        /// </summary>
        [JsonProperty]
        public Dictionary<CultureInfo, NewNotificationWithButtonsParameters> LocalizedParameters 
        { 
            get; 
            protected set; 
        }

        #endregion

        /// <summary>
        /// LocaleNewNotificationWithButtonsParameters
        /// </summary>
        [JsonConstructor]
        protected LocaleNewNotificationWithButtonsParameters()
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="defaultParameters"></param>
        /// <param name="localizedParameters"></param>
        public LocaleNewNotificationWithButtonsParameters(NewNotificationWithButtonsParameters defaultParameters, 
            Dictionary<CultureInfo, NewNotificationWithButtonsParameters> localizedParameters)
        {
            Requires.NotNull(defaultParameters, nameof(defaultParameters));

            DefaultParameters = defaultParameters;
            LocalizedParameters = localizedParameters;
        }
    }
}
