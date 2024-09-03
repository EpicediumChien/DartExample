#region LicenceHeader

//
// Copyright © 2022, Dell Inc., All Rights Reserved.
// This material is confidential and a trade secret.  Permission to use this
// work for any purpose must be obtained in writing from Dell Inc.
//

#endregion

using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft;
using System.Windows.Input;
using System.Windows.Media;

namespace NGA.ThickClient.Interfaces;

/// <summary>
/// This class is model for (WPF UXSuggestion control).
/// </summary>
public sealed class SuggestionModel : ObservableObject
{
    #region Private variables

    private string? _buttonTitle;
    private string? _description;
    private ICommand? _clickCommand;
    private ICommand? _closeCommand;
    private Geometry? _buttonIcon;
    private Geometry? _geometryIcon;
    private string _automationId = string.Empty;

    #endregion

    #region Properties

    /// <summary>
    /// Gets or sets the Button text string.
    /// </summary>
    public string? ButtonTitle
    {
        get => _buttonTitle;
        set => SetProperty(ref _buttonTitle, value);
    }

    /// <summary>
    /// Gets or sets the Description text string.
    /// </summary>
    public string? Description
    {
        get => _description;
        set => SetProperty(ref _description, value);
    }

    /// <summary>
    /// Suggestion icon for the Suggestion control. In SVG format.
    /// </summary>
    public Geometry? Icon
    {
        get => _geometryIcon;
        set => SetProperty(ref _geometryIcon, value);
    }

    /// <summary>
    /// Button icon for the Suggestion control. In SVG format.
    /// </summary>
    public Geometry? ButtonIcon
    {
        get => _buttonIcon;
        set => SetProperty(ref _buttonIcon, value);
    }

    /// <summary>
    /// Gets or sets Command
    /// </summary>
    public ICommand? ClickCommand
    {
        get => _clickCommand;
        set => SetProperty(ref _clickCommand, value);
    }

    /// <summary>
    /// Gets or sets CloseCommand
    /// </summary>
    public ICommand? CloseCommand
    {
        get => _closeCommand;
        set => SetProperty(ref _closeCommand, value);
    }

    /// <summary>
    /// Gets or sets the AutomationId
    /// </summary>
    public string AutomationId
    {
        get => _automationId;
        set => SetProperty(ref _automationId, value);
    }

    #endregion

    #region Constructors

    /// <summary>
    /// Initializes a new instance of the class.
    /// Use properties to set non-default values
    /// </summary>
    public SuggestionModel()
    {
    }

    /// <summary>
    /// Creates a SuggestionModel object
    /// </summary>
    /// <param name="buttonTitle"></param>
    /// <param name="description"></param>
    /// <param name="icon"></param>
    /// <param name="clickCommand"></param>
    public SuggestionModel(string buttonTitle, string description, Geometry icon, ICommand clickCommand)
    {
        Requires.NotNull(buttonTitle, nameof(buttonTitle));
        Requires.NotNull(description, nameof(description));
        Requires.NotNull(icon, nameof(icon));
        Requires.NotNull(clickCommand, nameof(clickCommand));

        ButtonTitle = buttonTitle;
        Description = description;
        Icon = icon;
        ClickCommand = clickCommand;
    }

    /// <summary>
    /// Creates a SuggestionModel object
    /// </summary>
    /// <param name="buttonTitle"></param>
    /// <param name="description"></param>
    /// <param name="icon"></param>
    /// <param name="clickCommand"></param>
    /// <param name="closeCommand"></param>
    public SuggestionModel(string buttonTitle, string description, Geometry icon, ICommand clickCommand, ICommand closeCommand)
    {
        Requires.NotNull(buttonTitle, nameof(buttonTitle));
        Requires.NotNull(description, nameof(description));
        Requires.NotNull(icon, nameof(icon));
        Requires.NotNull(clickCommand, nameof(clickCommand));
        Requires.NotNull(closeCommand, nameof(closeCommand));

        ButtonTitle = buttonTitle;
        Description = description;
        Icon = icon;
        ClickCommand = clickCommand;
        CloseCommand = closeCommand;
    }

    #endregion
}