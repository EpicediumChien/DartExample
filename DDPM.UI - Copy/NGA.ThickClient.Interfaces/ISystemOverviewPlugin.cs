#region LicenceHeader

//
// Copyright © 2022, Dell Inc., All Rights Reserved.
// This material is confidential and a trade secret.  Permission to use this
// work for any purpose must be obtained in writing from Dell Inc.
//

#endregion

namespace NGA.ThickClient.Interfaces
{
    /// <summary>
    ///  SystemOverviewPlugin Interface
    /// </summary>
    public interface ISystemOverviewPlugin : IThickClientPlugin
    {
        /// <summary>
        ///  SystemOverview Content already set or not
        /// </summary>
        bool AlreadySet { get; }

        /// <summary>
        ///   Sets the SystemOverview content
        /// </summary>
        /// <param name="systemOverviewModel">SystemOverviewModel object</param>
        /// <exception cref = "InvalidOperationException"> Thrown when plugin not in running condition</exception>
        /// <returns></returns>
        Task<bool> SetAsync(SystemOverviewModel systemOverviewModel);
    }
}