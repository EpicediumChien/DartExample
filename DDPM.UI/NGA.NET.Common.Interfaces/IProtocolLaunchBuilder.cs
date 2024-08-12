#region LicenceHeader
//
// Copyright © 2022, Dell Inc., All Rights Reserved.
// This material is confidential and a trade secret.  Permission to use this
// work for any purpose must be obtained in writing from Dell Inc.
//
#endregion

namespace NGA.NET.Common.Interfaces
{
    /// <summary>
    /// Interface IProtocolLaunchBuilder
    /// </summary>
    public interface IProtocolLaunchBuilder
    {
#pragma warning disable 0612, 0618
        /// <summary>
        /// Launch thick client application async with specified plugin
        /// </summary>
        /// <param name="builder">IProtocolStringBuilder</param>
        /// <returns>Task</returns>
        void LaunchApplicationAsync(IProtocolStringBuilder builder);
#pragma warning restore 0612, 0618

        /// <summary>
        /// Launch thick client application async with specified protocol uri string
        /// </summary>
        /// <param name="protocolUri">Protocol Uri string</param>
        /// <returns></returns>
        void LaunchApplicationAsync(string protocolUri);
    }
}
