#region LicenceHeader
//
// Copyright © 2022, Dell Inc., All Rights Reserved.
// This material is confidential and a trade secret.  Permission to use this
// work for any purpose must be obtained in writing from Dell Inc.
//
#endregion

using System;
using System.Threading.Tasks;

namespace NGA.Manager.Interfaces;

/// <summary>
/// IPreference interface
/// </summary>
/// <remarks>
/// At the moment IPreference would be part of NGA.Manager.Interfaces package. Otherwise we need to produce a separate nuget package 
/// for IPreference interfaces, we'll do it later if required. Teams would still consume 
/// NGA.Manager.Interfaces nuget package to access IPreference features
/// </remarks>
public interface IPreference : IPreferenceUnelevated
{
    /// <summary>
    /// Method to add or update the system preference
    /// </summary>
    /// <param name="preferenceId">UniqueId to store the <paramref name="preferenceData"/></param>
    /// <param name="preferenceData">String data (Deserialized object) to store</param>
    /// <exception cref="ArgumentNullException">This exception would be thrown if <paramref name="preferenceData"/> is null</exception>
    /// <exception cref="ArgumentException">This exception would be thrown if <paramref name="preferenceId"/> is empty</exception>
    /// <exception cref="SetPreferenceException">This exception would be thrown if data already exists</exception>
    Task SetSystemPreferenceAsync(Guid preferenceId, string preferenceData);

    /// <summary>
    /// Method to delete the system preference
    /// </summary>
    /// <param name="preferenceId">Id of the preference data</param>
    /// <exception cref="ArgumentException">This exception would be thrown if <paramref name="preferenceId"/> is empty</exception>
    Task DeleteSystemPreferenceAsync(Guid preferenceId);
}