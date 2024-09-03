#region LicenceHeader

//
// Copyright © 2022, Dell Inc., All Rights Reserved.
// This material is confidential and a trade secret.  Permission to use this
// work for any purpose must be obtained in writing from Dell Inc.
//

#endregion

using NGA.NET.Common.Interfaces;
using System.Diagnostics;

namespace NGA.NET.Common
{
    /// <summary>
    /// ProtocolLaunchBuilder class
    /// </summary>
    public sealed class ProtocolLaunchBuilder : IProtocolLaunchBuilder
    {
#pragma warning disable 0612, 0618

        /// <summary>
        /// Launch thick client application async with specified plugin
        /// </summary>
        /// <param name="builder">IProtocolStringBuilder</param>
        /// <returns>Task</returns>
        public void LaunchApplicationAsync(IProtocolStringBuilder builder)
        {
            Process.Start(new ProcessStartInfo(builder.BuildLaunchString()) { UseShellExecute = true });
        }

#pragma warning restore 0612, 0618

        /// <inheritdoc/>
        public void LaunchApplicationAsync(string protocolUri)
        {
            Process.Start(new ProcessStartInfo(protocolUri) { UseShellExecute = true });
        }
    }
}