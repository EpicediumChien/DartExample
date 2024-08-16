#region LicenceHeader

//
// Copyright © 2023, Dell Inc., All Rights Reserved.
// This material is confidential and a trade secret.  Permission to use this
// work for any purpose must be obtained in writing from Dell Inc.
//

#endregion

namespace NGA.ThickClient.Interfaces;

/// <summary>
/// Interface for IBasicTilePlugin
/// </summary>
public interface IBasicTilePlugin
{
    /// <summary>
    /// This method adds <see cref="BasicTileModel"/> to BasicTilePlugin
    /// </summary>
    /// <param name="basicTileModel"></param>
    /// <param name="position"></param>
    /// <returns>True if <paramref name="basicTileModel"/> is added successfully</returns>
    /// <exception cref="InvalidOperationException">This exception is thrown if the tile plugin is not in running condition</exception>
    /// <exception cref="ArgumentNullException">This exception is thrown if <paramref name="basicTileModel"/> is null</exception>
    Task<bool> AddTileAsync(BasicTileModel basicTileModel, int position);

    /// <summary>
    /// This method removes <see cref="BasicTileModel"/> from BasicTilePlugin
    /// </summary>
    /// <param name="basicTileModel"></param>
    /// <returns>True if <paramref name="basicTileModel"/> was removed successfully</returns>
    /// <exception cref="InvalidOperationException">This exception is thrown if the tile plugin is not in running condition</exception>
    /// <exception cref="ArgumentNullException">This exception is thrown if <paramref name="basicTileModel"/> is null</exception>
    Task<bool> RemoveTileAsync(BasicTileModel basicTileModel);

    /// <summary>
    /// This method checks if the <see cref="BasicTileModel"/> exists in BasicTilePlugin
    /// </summary>
    /// <param name="basicTileModel"></param>
    /// <returns>True is returned if <paramref name="basicTileModel"/> exists</returns>
    /// <exception cref="InvalidOperationException">This exception is thrown if the tile plugin is not in running condition</exception>
    /// <exception cref="ArgumentNullException">This exception is thrown if <paramref name="basicTileModel"/> is null</exception>
    Task<bool> TileExistsAsync(BasicTileModel basicTileModel);
}