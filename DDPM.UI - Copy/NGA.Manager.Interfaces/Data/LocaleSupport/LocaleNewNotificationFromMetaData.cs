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
    /// LocaleNewNotificationFromMetaData Class contains NewNotificationFromMetaDataAsync and localized texts
    /// </summary>
    public class LocaleNewNotificationFromMetaData
    {
        #region Properties

        /// <summary>
        /// Default XmlPayload
        /// </summary>
        [JsonProperty]
        public string DefaultXmlPayload { get; protected set; }

        /// <summary>
        /// Localized XmlPayload Dictionary
        /// </summary>
        [JsonProperty]
        public Dictionary<CultureInfo, string> LocalizedXmlPayload { get; protected set; }

        /// <summary>
        /// Default BindValues
        /// </summary>
        [JsonProperty]
        public Dictionary<string, string> DefaultBindValues { get; protected set; }

        /// <summary>
        /// Localized BindValues Dictionary
        /// </summary>
        [JsonProperty]
        public Dictionary<CultureInfo, Dictionary<string, string>> LocalizedBindValues { get; protected set; }

        #endregion

        /// <summary>
        /// LocaleNewNotificationFromMetaData
        /// </summary>
        [JsonConstructor]
        protected LocaleNewNotificationFromMetaData()
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="defaultXmlPayload"></param>
        /// <param name="localizedXmlPayload"></param>
        /// <param name="defaultBindValues"></param>
        /// <param name="localizedBindValues"></param>
        public LocaleNewNotificationFromMetaData(string defaultXmlPayload, Dictionary<CultureInfo, string> localizedXmlPayload,
            Dictionary<string, string> defaultBindValues = null,
            Dictionary<CultureInfo, Dictionary<string, string>> localizedBindValues = null)
        {
            Requires.NotNullOrWhiteSpace(defaultXmlPayload, nameof(defaultXmlPayload));

            DefaultXmlPayload = defaultXmlPayload;
            LocalizedXmlPayload = localizedXmlPayload;
            DefaultBindValues = defaultBindValues;
            LocalizedBindValues = localizedBindValues;
        }
    }
}