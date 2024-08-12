#region LicenceHeader
//
// Copyright © 2023, Dell Inc., All Rights Reserved.
// This material is confidential and a trade secret.  Permission to use this
// work for any purpose must be obtained in writing from Dell Inc.
//
#endregion

using Microsoft;
using System.Windows.Input;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;

namespace NGA.ThickClient.Interfaces;

/// <summary>
/// This class is model for UXBasicTile control
/// </summary>
public class BasicTileModel : ObservableObject
{
    private Geometry? _icon;
    private Geometry? _secondaryIcon;
    private string? _title;
    private string? _details;
    private ICommand? _command;
    private string _automationId = string.Empty;

    /// <summary>
    /// Gets or sets Icon        
    /// </summary>
    public Geometry? Icon
    {
        get => _icon;
        set => SetProperty(ref _icon, value);
    }

    /// <summary>
    /// Gets or sets SecondaryIcon
    /// </summary>
    public Geometry? SecondaryIcon
    {
        get => _secondaryIcon;
        set => SetProperty(ref _secondaryIcon, value);
    }

    /// <summary>
    /// Gets or sets Title
    /// </summary>
    public string? Title
    {
        get => _title;
        set => SetProperty(ref _title, value);
    }

    /// <summary>
    /// Gets or sets Details
    /// </summary>
    public string? Details
    {
        get => _details;
        set => SetProperty(ref _details, value);
    }

    /// <summary>
    /// Gets or sets Command
    /// </summary>
    public ICommand? Command
    {
        get => _command;
        set => SetProperty(ref _command, value);
    }

    /// <summary>
    /// Gets or sets AutomationId from AutomationProperties
    /// </summary>
    public string AutomationId
    {
        get => _automationId;
        set => SetProperty(ref _automationId, value);
    }

    /// <summary>
    /// Creates a BasicTileModel object
    /// </summary>
    /// <param name="title">Title string</param>
    /// <param name="details">Details string</param>
    /// <param name="icon">Icon for the BasicTile</param>
    /// <param name="secondaryIcon">SecondaryIcon for the BasicTile</param>
    /// <param name="command">Action to invoke when BasicTile is clicked</param>
    public BasicTileModel(string title, string details, Geometry icon, Geometry secondaryIcon, ICommand command)
    {
        Requires.NotNull(title, nameof(title));
        Requires.NotNull(details, nameof(details));
        Requires.NotNull(icon, nameof(icon));
        Requires.NotNull(secondaryIcon, nameof(secondaryIcon));
        Requires.NotNull(command, nameof(command));

        Title = title;
        Details = details;
        Icon = icon;
        SecondaryIcon = secondaryIcon;
        Command = command;
    }
}