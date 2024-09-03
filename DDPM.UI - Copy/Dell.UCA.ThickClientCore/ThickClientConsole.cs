#region LicenceHeader

//
// Copyright © 2022, Dell Inc., All Rights Reserved.
// This material is confidential and a trade secret.  Permission to use this
// work for any purpose must be obtained in writing from Dell Inc.
//

#endregion

using Dell.Client.Framework.Common;
using Dell.Client.Framework.UX.WPF;
using NGA.ThickClient.Interfaces;

namespace NGA.ThickClientCore
{
    /// <summary>
    /// Custom thick client functionality provided to all plugins
    /// </summary>
#pragma warning disable CS0618 // Type or member is obsolete

    public sealed class ThickClientConsole : IThickClientConsole, IDisposable
    {
        private readonly ILog? _log;
        private readonly IPluginManager _pluginManager;
        private readonly object _syncObject = new();
        private readonly List<HomePageTileInfo> _homePageTiles = new();
        private IConsoleHomePagePlugin? _homePagePlugin;
        private bool _disposedValue;
        private bool _hasTiles;

        // Dispatcher wrapper instance
        private readonly IDispatcherWrapper _dispatcherWrapper;

        /// <summary>
        /// Has Tiles
        /// </summary>
        public bool HasTiles
        {
            get
            {
                GetHomePagePlugin();

                return _homePagePlugin is IThickClientHomePagePlugin { HasTiles: true } || _hasTiles;
            }
        }

        /// <summary>
        /// Constructor for the custom <see cref="IThickClientConsole"/> implementation
        /// </summary>
        /// <param name="pluginManager"></param>
        /// <param name="logFactory"></param>
        /// <param name="dispatcherWrapper"></param>
        public ThickClientConsole(IPluginManager pluginManager, ILogFactory logFactory, IDispatcherWrapper dispatcherWrapper)
        {
            _log = logFactory.CreateLogger("ThickCon", typeof(ThickClientConsole));
            _pluginManager = pluginManager;
            _dispatcherWrapper = dispatcherWrapper;

            _pluginManager.PluginsLoaded += MainWindow_PluginsLoadedEvent;
            _homePagePlugin = _pluginManager.FindPluginByType<IThickClientHomePagePlugin>();

            _log?.Info($"{nameof(ThickClientConsole)} - Constructed");
        }

        #region IThickClientConsole

        /// <summary>
        /// Adds a tile to the home page.
        /// </summary>
        /// <param name="tileModel">UX Tile Model.</param>
        /// <param name="position">Position of the tile</param>
        /// <param name="region">region the tile to be placed</param>
        public void AddTileToHomePage(TileModel tileModel, int position, Guid? region = null)
        {
            if (tileModel == null!)
                return;

            if (string.IsNullOrEmpty(tileModel.TitleText))
                return;

            region ??= Guid.Empty;
            if (FindTileOnHomePage(tileModel.TitleText, region) != null)
                return;

            GetHomePagePlugin();

            if (_homePagePlugin is IThickClientHomePagePlugin thickClientHomePagePlugin)
            {
                _dispatcherWrapper.Invoke(() => thickClientHomePagePlugin.AddTileToHomePage(tileModel, position, region));
            }
            else
            {
                lock (_syncObject)
                {
                    _homePageTiles.Add(new HomePageTileInfo { TileModel = tileModel, Position = position, TileRegion = region });
                    _hasTiles = _homePageTiles.Count > 0;
                }
            }
        }

        /// <summary>
        /// Finds a tile by titleText on the HomePage if one exists.
        /// </summary>
        /// <param name="titleText">Tile TitleText</param>
        /// <param name="region">region the tile to be placed</param>
        /// <returns>TileModel</returns>
        public TileModel? FindTileOnHomePage(string titleText, Guid? region = null)
        {
            region ??= Guid.Empty;
            TileModel? tileModel = null;

            GetHomePagePlugin();

            if (_homePagePlugin is IThickClientHomePagePlugin thickClientHomePagePlugin)
            {
                _dispatcherWrapper.Invoke(() =>
                {
                    tileModel = thickClientHomePagePlugin.FindTileOnHomePage(titleText, region);
                });

                return tileModel;
            }

            lock (_syncObject)
            {
                for (var index = _homePageTiles.Count - 1; index >= 0; index--)
                {
                    if (_homePageTiles[index].TileModel.TitleText == titleText && _homePageTiles[index].TileRegion == region)
                        tileModel = _homePageTiles[index].TileModel;
                }
            }

            return tileModel;
        }

        /// <summary>
        /// Finds a tileModel by position on the HomePage if one exists.
        /// </summary>
        /// <param name="position">position of tile</param>
        /// <param name="region">region the tile to be placed</param>
        /// <returns>TileModel</returns>
        public TileModel? FindTileOnHomePageAtPosition(int position, Guid? region = null)
        {
            region ??= Guid.Empty;
            TileModel? tileModel = null;

            GetHomePagePlugin();

            if (_homePagePlugin is IThickClientHomePagePlugin thickClientHomePagePlugin)
            {
                _dispatcherWrapper.Invoke(() =>
                {
                    tileModel = thickClientHomePagePlugin.FindTileOnHomePageAtPosition(position, region);
                });

                return tileModel;
            }

            lock (_syncObject)
            {
                foreach (var entry in _homePageTiles.Where(x => x.Position == position && x.TileRegion == region))
                {
                    tileModel = entry.TileModel;
                }
            }

            return tileModel;
        }

        /// <summary>
        /// Removes a tile from the home page.
        /// </summary>
        /// <param name="tileModel">tileModel</param>
        /// <param name="region">region the tile to be placed</param>
        public void RemoveTileFromHomePage(TileModel tileModel, Guid? region = null)
        {
            region ??= Guid.Empty;

            GetHomePagePlugin();

            if (_homePagePlugin is IThickClientHomePagePlugin thickClientHomePagePlugin)
            {
                _dispatcherWrapper.Invoke(() => thickClientHomePagePlugin.RemoveTileFromHomePage(tileModel, region));
            }
            else
            {
                lock (_syncObject)
                {
                    for (var index = _homePageTiles.Count - 1; index >= 0; index--)
                    {
                        if (_homePageTiles[index].TileModel == tileModel && _homePageTiles[index].TileRegion == region)
                            _homePageTiles.RemoveAt(index);
                    }
                    _hasTiles = _homePageTiles.Count > 0;
                }
            }
        }

        #endregion

        #region Classes

        /// <summary>
        /// Contains the Home page tile information
        /// </summary>
        private struct HomePageTileInfo
        {
            /// <summary>
            /// The tile control
            /// </summary>
            public TileModel TileModel;

            /// <summary>
            ///  The tile position
            /// </summary>
            public int Position;

            /// <summary>
            /// The region tile belongs to
            /// </summary>
            public Guid? TileRegion;
        }

        #endregion

        #region Private methods

        private void MainWindow_PluginsLoadedEvent(object? sender, PluginsLoadedEventArgs e)
        {
            var homePagePlugins = e.ChangedPlugins.OfType<IConsoleHomePagePlugin>().ToList();
            if (!homePagePlugins.Any())
                return;

            _homePagePlugin = homePagePlugins.First();

            lock (_syncObject)
            {
                _log?.Trace($"{nameof(MainWindow_PluginsLoadedEvent)} - Have {nameof(_homePagePlugin)} adding {_homePageTiles.Count} tiles");
                foreach (var entry in _homePageTiles)
                {
                    AddTileToHomePage(entry.TileModel, entry.Position, entry.TileRegion);
                }

                _homePageTiles.Clear();
                _hasTiles = _homePageTiles.Count > 0;
            }
        }

        private void GetHomePagePlugin()
        {
            if (_homePagePlugin != null)
                return;

            _log?.Trace($"{nameof(GetHomePagePlugin)} - {nameof(_homePagePlugin)} is null, Finding {nameof(_homePagePlugin)} in {nameof(_pluginManager)}");
            _homePagePlugin = _pluginManager.FindPluginByType<IThickClientHomePagePlugin>();
        }

        private void Dispose(bool disposing)
        {
            if (_disposedValue)
                return;

            if (disposing)
            {
                _log?.Trace($"{nameof(Dispose)} - Disposing of everything");
                _pluginManager.PluginsLoaded -= MainWindow_PluginsLoadedEvent;
            }

            _disposedValue = true;
        }

        #endregion

        /// <summary>
        /// Disposes of resources
        /// </summary>
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }

#pragma warning restore CS0618 // Type or member is obsolete
}