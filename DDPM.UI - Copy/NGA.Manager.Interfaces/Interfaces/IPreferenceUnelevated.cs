#region LicenceHeader

//
// Copyright © 2022, Dell Inc., All Rights Reserved.
// This material is confidential and a trade secret.  Permission to use this
// work for any purpose must be obtained in writing from Dell Inc.
//

#endregion

namespace NGA.Manager.Interfaces;

/// <summary>
/// IPreferenceUnelevated interface
/// </summary>
public interface IPreferenceUnelevated
{
    /// <summary>
    /// Method to get the User preference
    /// </summary>
    /// <param name="userSid">SecurityIdentifier for the user</param>
    /// <param name="preferenceId">Id of the requested preference data</param>
    /// <returns>The string data</returns>
    /// <exception cref="ArgumentNullException">This exception would be thrown if <paramref name="userSid"/> is null</exception>
    /// <exception cref="ArgumentException">This exception would be thrown if <paramref name="userSid"/> is empty/whitespace.
    /// Or if <paramref name="preferenceId"/> is empty</exception>
    /// <exception cref="GetPreferenceException">This exception would be thrown if the data is not found for the passed in key or
    /// if the decoded/deserialized data is null</exception>
    /// <remarks>The combination of <paramref name="userSid"/> and <paramref name="preferenceId"/> is used as key</remarks>
    Task<string> GetUserPreferenceAsync(string userSid, Guid preferenceId);

    /// <summary>
    /// Method to Add or update the User preference
    /// </summary>
    /// <param name="userSid">SecurityIdentifier for the user</param>
    /// <param name="preferenceId">UniqueId to store the <paramref name="preferenceData"/></param>
    /// <param name="preferenceData">String data (Deserialized object) to store</param>
    /// <exception cref="ArgumentNullException">This exception would be thrown if <paramref name="userSid"/> is null.
    /// Or if <paramref name="preferenceData"/> is null</exception>
    /// <exception cref="ArgumentException">This exception would be thrown if <paramref name="userSid"/> is empty/whitespace.
    /// Or if <paramref name="preferenceId"/> is empty</exception>
    /// <exception cref="SetPreferenceException">This exception would be thrown if data already exists</exception>
    /// <remarks>The combination of <paramref name="userSid"/> and <paramref name="preferenceId"/> is used as key</remarks>
    Task SetUserPreferenceAsync(string userSid, Guid preferenceId, string preferenceData);

    /// <summary>
    /// Method to check whether the User preference exists
    /// </summary>
    /// <param name="userSid">SecurityIdentifier for the user</param>
    /// <param name="preferenceId">Id of the requested preference data</param>
    /// <exception cref="ArgumentNullException">This exception would be thrown if <paramref name="userSid"/> is null</exception>
    /// <exception cref="ArgumentException">This exception would be thrown if <paramref name="userSid"/> is empty/whitespace.
    /// Or if <paramref name="preferenceId"/> is empty</exception>
    /// <exception cref="PreferenceExistException">This exception would be thrown if any other error occurs</exception>
    /// <returns>Returns true if the preference data exists or returns false</returns>
    /// <remarks>The combination of <paramref name="userSid"/> and <paramref name="preferenceId"/> is used as key</remarks>
    Task<bool> UserPreferenceExistAsync(string userSid, Guid preferenceId);

    /// <summary>
    /// Method to delete the User preference
    /// </summary>
    /// <param name="userSid">SecurityIdentifier for the user</param>
    /// <param name="preferenceId">Id of the preference data</param>
    /// <exception cref="ArgumentNullException">This exception would be thrown if <paramref name="userSid"/> is null</exception>
    /// <exception cref="ArgumentException">This exception would be thrown if <paramref name="userSid"/> is empty/whitespace.
    /// Or if <paramref name="preferenceId"/> is empty</exception>
    /// <exception cref="DeletePreferenceException">This exception would be thrown if the data is not found</exception>
    /// <remarks>The combination of <paramref name="userSid"/> and <paramref name="preferenceId"/> is used as key</remarks>
    Task DeleteUserPreferenceAsync(string userSid, Guid preferenceId);

    /// <summary>
    /// Method to get the System preference
    /// </summary>
    /// <param name="preferenceId">Id of the requested preference data</param>
    /// <returns>The string data</returns>
    /// <exception cref="ArgumentException">This exception would be thrown if <paramref name="preferenceId"/> is empty</exception>
    /// <exception cref="GetPreferenceException">This exception would be thrown if the data is not found for the passed in key or
    /// if the decoded/deserialized data is null</exception>
    Task<string> GetSystemPreferenceAsync(Guid preferenceId);

    /// <summary>
    /// Method to check whether the System preference
    /// </summary>
    /// <param name="preferenceId">Id of the requested preference data</param>
    /// <exception cref="ArgumentException">This exception would be thrown if <paramref name="preferenceId"/> is empty</exception>
    /// <returns>>Returns true if the preference data exists or returns false</returns>
    Task<bool> SystemPreferenceExistAsync(Guid preferenceId);
}