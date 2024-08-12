#region LicenceHeader
//
// Copyright © 2022, Dell Inc., All Rights Reserved.
// This material is confidential and a trade secret.  Permission to use this
// work for any purpose must be obtained in writing from Dell Inc.
//
#endregion

using Microsoft;
using System.Windows;

namespace NGA.ThickClient.Interfaces
{
    /// <summary>
    ///  Model for SystemOverview
    /// </summary>
    public class SystemOverviewModel
    {
        /// <summary>
        ///  SystemOverviewModel constructor
        /// </summary>
        /// <param name="content">SystemOverview content</param>
        public SystemOverviewModel(FrameworkElement content)
        {
            Requires.NotNull(content, nameof(content));
            Content = content;
        }

        /// <summary>
        ///  Content to be displayed
        /// </summary>
        public FrameworkElement Content { get; private set; } 

    }
}
