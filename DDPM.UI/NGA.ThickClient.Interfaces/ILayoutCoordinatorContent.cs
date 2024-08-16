#region LicenceHeader

//
// Copyright © 2022, Dell Inc., All Rights Reserved.
// This material is confidential and a trade secret.  Permission to use this
// work for any purpose must be obtained in writing from Dell Inc.
//

#endregion

using System.Windows;

namespace NGA.ThickClient.Interfaces;

/// <summary>
/// Interface for LayoutCoordinator
/// </summary>
public interface ILayoutCoordinatorContent
{
    /// <summary>
    /// This property holds the view to be displayed
    /// </summary>
    public FrameworkElement Content { get; }

    /// <summary>
    /// This property is the unique id for the content that is displayed
    /// </summary>
    public Guid ContentId { get; }
}