#region LicenceHeader
//
// Copyright © 2022, Dell Inc., All Rights Reserved.
// This material is confidential and a trade secret.  Permission to use this
// work for any purpose must be obtained in writing from Dell Inc.
//
#endregion

using Microsoft;

namespace NGA.NET.Common
{
    /// <summary>
    /// Root Model class
    /// </summary>
    public  class Root
    {
        #region properties

        /// <summary>
        /// Model version
        /// </summary>
        public int ModelVersion { get; }

        /// <summary>
        /// Json serialized Model (i.e. an object of ParamModel)
        /// </summary>
        public string ModelJson { get; }

        #endregion

        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="modelVersion"></param>
        /// <param name="modelJson"></param>
        public Root(int modelVersion, string modelJson)
        {
            Requires.NotDefault(modelVersion, nameof(modelVersion));        
    
            ModelVersion = modelVersion;
            ModelJson = modelJson;
        }

        #endregion
    }
}
