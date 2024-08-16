#region LicenceHeader

//
// Copyright © 2022, Dell Inc., All Rights Reserved.
// This material is confidential and a trade secret.  Permission to use this
// work for any purpose must be obtained in writing from Dell Inc.
//

#endregion

using Dell.Client.Framework.UX.WPF;
using System.Windows;

namespace NGA.ThickClient.Interfaces
{
    /// <summary>
    /// IThickClientHomePagePlugin extends interface of IConsoleHomePagePlugin
    /// </summary>
    public interface IThickClientHomePagePlugin : IConsoleHomePagePlugin
    {
        /// <summary>
        /// HadTiles
        /// </summary>
        [Obsolete("This class is deprecated and will be removed in a future release. Please use IBasicTilePlugin")]
        bool HasTiles { get; }

        /// <summary>
        /// Adds a tile to the home page.
        /// </summary>
        /// <param name="tileModel">TileModel</param>
        /// <param name="position">position of the tile</param>
        /// <param name="region">region the tile to be placed</param>
        [Obsolete("This class is deprecated and will be removed in a future release. Please use IBasicTilePlugin")]
        void AddTileToHomePage(TileModel tileModel, int position, Guid? region = null);

        /// <summary>
        /// Finds a tile by titleText on the HomePage if one exists.
        /// </summary>
        /// <param name="titleText">Tile TitleText</param>
        /// <param name="region">region the tile to be placed</param>
        /// <returns>TileModel</returns>
        [Obsolete("This class is deprecated and will be removed in a future release. Please use IBasicTilePlugin")]
        TileModel? FindTileOnHomePage(string titleText, Guid? region = null);

        /// <summary>
        /// Finds a tileModel by position on the HomePage if one exists.
        /// </summary>
        /// <param name="position">position of tile</param>
        /// <param name="region">region the tile to be placed</param>
        /// <returns>TileModel</returns>
        [Obsolete("This class is deprecated and will be removed in a future release. Please use IBasicTilePlugin")]
        TileModel? FindTileOnHomePageAtPosition(int position, Guid? region = null);

        /// <summary>
        /// Removes a tile from the home page.
        /// </summary>
        /// <param name="tileModel">tileModel</param>
        /// <param name="region">region the tile to be placed</param>
        [Obsolete("This class is deprecated and will be removed in a future release. Please use IBasicTilePlugin")]
        void RemoveTileFromHomePage(TileModel tileModel, Guid? region = null);

        /// <summary>
        ///   Add the content to HomePage
        /// </summary>
        /// <param name="content">Content to be added</param>
        /// <returns>Returns true if the content is added</returns>
        /// <exception cref = "ArgumentNullException"> Thrown when content is null</exception>
        public Task<bool> AddContentAsync(FrameworkElement content);
    }
}