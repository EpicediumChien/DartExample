#region LicenceHeader
//
// Copyright © 2022, Dell Inc., All Rights Reserved.
// This material is confidential and a trade secret.  Permission to use this
// work for any purpose must be obtained in writing from Dell Inc.
//
#endregion

using System.Collections;

namespace NGA.ThickClient.Interfaces;

/// <summary>
/// Interface for LayoutCoordinator
/// </summary>
// ReSharper disable once TypeParameterCanBeVariant
public interface ILayoutCoordinator<T>
{
    /// <summary>
    /// Holds the accepted content ids
    /// </summary>
    IEnumerable<Guid> AcceptedContentIds { get; }

    /// <summary>
    /// Returns true if it is ready to accept content
    /// </summary>
    /// <param name="contentId"></param>
    /// <returns></returns>
    Task<bool> WillAcceptContentAsync(Guid contentId);

    /// <summary>
    /// Removes content from the layout coordinator
    /// </summary>
    /// <param name="contentId"></param>
    /// <returns>Returns true if the content associated with the content id is removed</returns>
    /// <exception cref = "ArgumentException"> Thrown when contentId is empty</exception>
    Task<bool> RemoveContentAsync(Guid contentId);

    /// <summary>
    /// Adds content to the layout coordinator
    /// </summary>
    /// <param name="content"></param>
    /// <returns>Returns true if the content is added</returns>
    /// <exception cref = "ArgumentNullException"> Thrown when content is null</exception>
    Task<bool> AddContentAsync(T  content);
}