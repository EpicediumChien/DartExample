#region LicenceHeader
//
// Copyright © 2022, Dell Inc., All Rights Reserved.
// This material is confidential and a trade secret.  Permission to use this
// work for any purpose must be obtained in writing from Dell Inc.
//
#endregion

using Microsoft;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace NGA.ThickClient.Interfaces
{
    /// <summary>
    /// This class is model for (WPF UXTile control).
    /// </summary>
    [Obsolete("This class is deprecated and will be removed in a future release. Please use BasicTileModel")]
    public sealed class TileModel : ObservableObject
    {
        #region Private variables

        private string? _titleText;
        private string? _detailText;
        private string? _subTitleText;
        private string? _tileButtonText;
        private ImageSource? _tileIcon;
        private ImageSource? _backgroundImage;
        private SolidColorBrush? _separatorColor;
        private ICommand? _tileClickCommand;
        private bool _useCustomContent;
        private FrameworkElement? _customContent;
        private ImageSource? _heroImage;
        private bool _enableSecondaryTileIcon;
        private Geometry? _secondaryTileIcon;
        private Geometry? _geometryIcon;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the title text string.
        /// </summary>
        public string? TitleText
        {
            get => _titleText;
            set => SetProperty(ref _titleText, value);
        }

        /// <summary>
        /// Gets or sets the detail text string.
        /// </summary>
        public string? DetailText
        {
            get => _detailText;
            set => SetProperty(ref _detailText, value);
        }

        /// <summary>
        /// Gets or sets the sub detail text string.
        /// </summary>
        public string? SubDetailText
        {
            get => _subTitleText;
            set => SetProperty(ref _subTitleText, value);
        }

        /// <summary>
        /// The button text for a tile
        /// </summary>
        public string? TileButtonText
        {
            get => _tileButtonText;
            set => SetProperty(ref _tileButtonText, value);
        }

        /// <summary>
        /// Icon for the tile control
        /// </summary>
        public ImageSource? TileIcon
        {
            get => _tileIcon;
            set => SetProperty(ref _tileIcon, value);
        }

        /// <summary>
        /// Background image for the tile
        /// </summary>
        public ImageSource? BackgroundImage
        {
            get => _backgroundImage;
            set => SetProperty(ref _backgroundImage, value);
        }

        /// <summary>
        /// Separator background color
        /// </summary>
        public SolidColorBrush? SeparatorColor
        {
            get => _separatorColor;
            set => SetProperty(ref _separatorColor, value);
        }

        /// <summary>
        ///  Gets or Sets Command for TileOnClick
        /// </summary>
        public ICommand? TileClickCommand
        {
            get => _tileClickCommand;
            set => SetProperty(ref _tileClickCommand, value);
        }

        /// <summary>
        /// Tile icon for the tile control. In SVG format.        
        /// </summary>
        public Geometry? Icon
        {
            get => _geometryIcon;
            set => SetProperty(ref _geometryIcon, value);
        }

        /// <summary>
        ///  Gets or Sets UseCustomContent
        /// </summary>
        public bool UseCustomContent
        {
            get => _useCustomContent;
            set => SetProperty(ref _useCustomContent, value);
        }

        /// <summary>
        ///  Gets or Sets CustomContent
        /// </summary>
        public FrameworkElement? CustomContent
        {
            get => _customContent;
            set => SetProperty(ref _customContent, value);
        }

        /// <summary>
        /// Gets or Sets HeroImage of Tile
        /// </summary>
        public ImageSource? HeroImage
        {
            get => _heroImage;
            set => SetProperty(ref _heroImage, value);
        }

        /// <summary>
        /// Gets or sets EnableSecondaryTileIcon
        /// </summary>
        public bool EnableSecondaryTileIcon
        {
            get => _enableSecondaryTileIcon;
            set => SetProperty(ref _enableSecondaryTileIcon, value);
        }

        /// <summary>
        /// Gets or sets SecondaryTileIcon
        /// </summary>
        public Geometry? SecondaryTileIcon
        {
            get => _secondaryTileIcon;
            set => SetProperty(ref _secondaryTileIcon, value);
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the class.
        /// Use properties to set non-default values
        /// </summary>
        public TileModel()
        {
        }

        /// <summary>
        /// Creates a TileModel object for use with HomePagePlugin and UXTile.
        /// Use properties to set non-default values for UseCustomContent, 
        /// CustomContent, HeroImage, EnableSecondaryTileIcon and SecondaryTileIcon
        /// </summary>
        /// <param name="tileDetails">Tile string details</param>
        /// <param name="tileIcon">An Icon to show as the tile</param>
        /// <param name="backgroundImage">A background image to show</param>
        /// <param name="separatorColor">separator Color</param>
        /// <param name="tileClickCommand">What happens when the tile is clicked</param>
        public TileModel(TileTextDetails tileDetails, ImageSource tileIcon,
            ImageSource backgroundImage, SolidColorBrush separatorColor, ICommand tileClickCommand) :
            this(tileDetails, backgroundImage, separatorColor, tileClickCommand)
        {
            Requires.NotNull(tileIcon, nameof(tileIcon));
            TileIcon = tileIcon;
        }

        /// <summary>
        /// Creates a TileModel object for use with HomePagePlugin and UXTile.
        /// Use properties to set non-default values for UseCustomContent, 
        /// CustomContent, HeroImage, EnableSecondaryTileIcon and SecondaryTileIcon
        /// </summary>
        /// <param name="tileDetails">Tile string details</param>
        /// <param name="geometryIcon">An Icon to show as the tile</param>
        /// <param name="backgroundImage">A background image to show</param>
        /// <param name="separatorColor">separator Color</param>
        /// <param name="tileClickCommand">What happens when the tile is clicked</param>
        public TileModel(TileTextDetails tileDetails, Geometry geometryIcon,
            ImageSource backgroundImage, SolidColorBrush separatorColor, ICommand tileClickCommand) :
            this(tileDetails, backgroundImage, separatorColor, tileClickCommand)
        {
            Requires.NotNull(geometryIcon, nameof(geometryIcon));
            Icon = geometryIcon;
        }

        private TileModel(TileTextDetails tileDetails, ImageSource backgroundImage,
            SolidColorBrush separatorColor, ICommand tileClickCommand)
        {
            Requires.NotNull(tileDetails, nameof(tileDetails));
            Requires.NotNull(backgroundImage, nameof(backgroundImage));
            Requires.NotNull(separatorColor, nameof(separatorColor));
            Requires.NotNull(tileClickCommand, nameof(tileClickCommand));

            TitleText = tileDetails.TitleText;
            DetailText = tileDetails.DetailText;
            SubDetailText = tileDetails.SubDetailText;
            TileButtonText = tileDetails.TileButtonText;
            BackgroundImage = backgroundImage;
            SeparatorColor = separatorColor;
            TileClickCommand = tileClickCommand;
        }

        #endregion
    }

    /// <summary>
    /// Record for Tile texts
    /// </summary>
    [Obsolete("This class is deprecated and will be removed in a future release. Please use BasicTileModel")]
    public sealed record TileTextDetails
    {
        /// <summary>
        /// Gets or sets the title text string.
        /// </summary>
        public string? TitleText { get; set; }

        /// <summary>
        ///  Gets or sets the detail text string.
        /// </summary>
        public string? DetailText { get; set; }

        /// <summary>
        /// Gets or sets the sub detail text string.
        /// </summary>
        public string? SubDetailText { get; set; }

        /// <summary>
        /// The button text for a tile
        /// </summary>
        public string? TileButtonText { get; set; }
    }
}