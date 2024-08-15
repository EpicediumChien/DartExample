#region LicenceHeader

//
// Copyright © 2023, Dell Inc., All Rights Reserved.
// This material is confidential and a trade secret.  Permission to use this
// work for any purpose must be obtained in writing from Dell Inc.
//

#endregion

namespace NGA.Manager.Interfaces;

/// <summary>
/// INotificationProtocolURI Interface
/// </summary>
public interface INotificationProtocolURI
{
    /// <summary>
    /// returns the list of <see cref="ProtocolInformation"/> items for installed products
    /// </summary>
    /// <returns></returns>
    Task<List<ProtocolInformation>> GetInstalledProtocolURIs();

    /// <summary>
    /// Builds Launch String for protocol URI
    /// </summary>
    /// <param name="protocolParameters">Protocol parameters such as StartingPlugin, Plugin parameter and priority</param>
    /// <param name="protocolInformation">Protocol information such as Protocol uri, version, .etc</param>
    /// <returns>Base64Encode string</returns>
    Task<string> BuildProtocolURI(ProtocolParameters protocolParameters, ProtocolInformation protocolInformation);
}