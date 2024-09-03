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
    /// IThickClientConsole(Specific to ThickClient) implements IConsole and consumed at MainWindow.
    /// </summary>
    [Obsolete("This interface is deprecated and will be removed in a future release. Please use IBasicTilePlugin")]
    public interface IThickClientConsole
    {
        /// <summary>
        /// Has Tiles
        /// </summary>
        [Obsolete("This interface is deprecated and will be removed in a future release. Please use IBasicTilePlugin")]
        bool HasTiles { get; }

        /// <summary>
        /// Adds a tile to the home page.
        /// </summary>
        /// <param name="tileModel">TileModel</param>
        /// <param name="position">position of the tile
        /// Warning: position is not a fixed position and should not be relied on. The position
        /// is purely a weighting mechanism on how to order two or more tiles in the list.
        /// For example if TileModel A is added with a position of 100 and TileModel B
        /// is added with a position of 200. A will be placed before B.
        ///
        /// If however TileModel A and TileModel B have the same position. The order of
        /// TileModel A and TileModel B is random as it depends on which order the plugins that add
        /// TileModel A and TileModel B make the API call.
        ///
        /// If TileModel A and TileModel B have the same position yet they are added
        /// to a different region the two positions have no impact on each other since
        /// they are in different <paramref name="region"/>.</param>
        /// <param name="region">region the tile to be placed.</param>
        [Obsolete("This interface is deprecated and will be removed in a future release. Please use IBasicTilePlugin")]
        void AddTileToHomePage(TileModel tileModel, int position, Guid? region = null);

        /// <summary>
        /// Finds a tile by titleText on the HomePage if one exists.
        /// </summary>
        /// <param name="titleText">Tile TitleText</param>
        /// <param name="region">region the tile to be placed</param>
        /// <returns>TileModel
        /// Warning: Always validate the return value as it is possible two
        /// TileModel objects have the same <see cref="TileModel.TitleText"/>
        /// </returns>
        [Obsolete("This interface is deprecated and will be removed in a future release. Please use IBasicTilePlugin")]
        TileModel? FindTileOnHomePage(string titleText, Guid? region = null);

        /// <summary>
        /// Finds a tileModel by position on the HomePage if one exists.
        /// </summary>
        /// <param name="position">position of tile
        /// Warning: position is not a fixed position and should not be relied on. The position
        /// is purely a weighting mechanism on how to order two or more tiles in the list.
        /// For example if TileModel A is added with a position of 100 and TileModel B
        /// is added with a position of 200. A will be placed before B.
        ///
        /// If however TileModel A and TileModel B have the same position. The order of
        /// TileModel A and TileModel B is random as it depends on which order the plugins that add
        /// TileModel A and TileModel B make the API call.
        ///
        /// If TileModel A and TileModel B have the same position yet they are added
        /// to a different region the two positions have no impact on each other since
        /// they are in different <paramref name="region"/>.</param>
        /// <param name="region">region the tile to be placed</param>
        /// <returns>TileModel
        /// Warning: Always validate the return value as it is possible two
        /// TileModel objects have the same <paramref name="position"/>
        /// </returns>
        [Obsolete("This interface is deprecated and will be removed in a future release. Please use IBasicTilePlugin")]
        TileModel? FindTileOnHomePageAtPosition(int position, Guid? region = null);

        /// <summary>
        /// Removes a tile from the home page.
        /// </summary>
        /// <param name="tileModel">tileModel</param>
        /// <param name="region">region the tile to be placed</param>
        [Obsolete("This interface is deprecated and will be removed in a future release. Please use IBasicTilePlugin")]
        void RemoveTileFromHomePage(TileModel tileModel, Guid? region = null);
    }
}