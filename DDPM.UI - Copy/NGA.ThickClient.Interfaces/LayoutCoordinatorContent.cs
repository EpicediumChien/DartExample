#region LicenceHeader

//
// Copyright © 2022, Dell Inc., All Rights Reserved.
// This material is confidential and a trade secret.  Permission to use this
// work for any purpose must be obtained in writing from Dell Inc.
//

#endregion

using Microsoft;
using System.Windows;

namespace NGA.ThickClient.Interfaces;

/// <summary>
/// Class for Layout Coordinator content
/// </summary>
public class LayoutCoordinatorContent : ILayoutCoordinatorContent
{
    /// <inheritdoc/>
    public FrameworkElement Content { get; }

    /// <inheritdoc/>
    public Guid ContentId { get; }

    /// <summary>
    /// Creates a LayoutCoordinatorContent object
    /// </summary>
    /// <param name="content"></param>
    /// <param name="contentId"></param>
    public LayoutCoordinatorContent(FrameworkElement content, Guid contentId)
    {
        Requires.NotNull(content, nameof(content));
        Requires.NotEmpty(contentId, nameof(contentId));

        Content = content;
        ContentId = contentId;
    }
}