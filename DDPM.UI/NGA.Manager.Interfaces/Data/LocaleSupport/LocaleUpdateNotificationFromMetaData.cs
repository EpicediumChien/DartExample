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
    /// LocaleUpdateNotificationFromMetaData Class contains UpdateNotificationFromMetaDataAsync and localized texts
    /// </summary>
    public class LocaleUpdateNotificationFromMetaData
    {
        #region Properties

        /// <summary>
        /// Default BindValues
        /// </summary>
        [JsonProperty]
        public Dictionary<string, string> DefaultBindValues { get; protected set; }

        /// <summary>
        /// Localized BindValues Dictionary
        /// </summary>
        [JsonProperty]
        public Dictionary<CultureInfo, Dictionary<string, string>> LocalizedBindValues { get; protected set;}

        #endregion

        /// <summary>
        /// LocaleUpdateNotificationFromMetaData
        /// </summary>
        [JsonConstructor]
        protected LocaleUpdateNotificationFromMetaData()
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="defaultBindValues"></param>
        /// <param name="localizedBindValues"></param>
        public LocaleUpdateNotificationFromMetaData(Dictionary<string, string> defaultBindValues,
            Dictionary<CultureInfo, Dictionary<string, string>> localizedBindValues = null)
        {
            Requires.NotNull(defaultBindValues, nameof(defaultBindValues));

            DefaultBindValues = defaultBindValues;
            LocalizedBindValues = localizedBindValues;
        }
    }
}
