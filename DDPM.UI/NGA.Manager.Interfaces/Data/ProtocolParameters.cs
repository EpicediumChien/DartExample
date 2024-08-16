#region LicenceHeader

//
// Copyright © 2023, Dell Inc., All Rights Reserved.
// This material is confidential and a trade secret.  Permission to use this
// work for any purpose must be obtained in writing from Dell Inc.
//

#endregion

namespace NGA.Manager.Interfaces;

/// <summary>
/// Plugin protocol parameters class.
/// </summary>
public class ProtocolParameters
{
    #region Private Variables

    private readonly Guid _startingPlugin;

    #endregion

    #region Properties

    /// <summary>
    /// Starting plugin Guid for protocol URI
    /// </summary>
    public Guid StartingPlugin
    { get { return _startingPlugin; } }

    /// <summary>
    /// Plugin parameter if any
    /// </summary>
    public string PluginParamter { get; set; }

    /// <summary>
    /// Plugin priority
    /// </summary>
    public int PluginPriority { get; set; }

    #endregion

    #region Constructor

    /// <summary>
    /// Constructor
    /// </summary>
    public ProtocolParameters(Guid startingPlugin = default)
    {
        _startingPlugin = startingPlugin;
    }

    #endregion
}