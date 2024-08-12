#region LicenceHeader
//
// Copyright © 2022, Dell Inc., All Rights Reserved.
// This material is confidential and a trade secret.  Permission to use this
// work for any purpose must be obtained in writing from Dell Inc.
//
#endregion

using System;
using System.Threading.Tasks;

namespace NGA.ConsolePreferencesHelperPlugin.Interfaces
{
    /// <summary>
    /// IConsolePreferenceHelper interface
    /// </summary>
    public interface IConsolePreferenceHelper
    {
        /// <summary>
        /// Helps in uniquely setting the preference information to a specific user
        /// </summary>
        /// <param name="preferenceId">preferenceId</param>
        /// <param name="preferenceData">preferenceData</param>
        /// <returns></returns>
        Task SetUserPreference(Guid preferenceId, object preferenceData);

        /// <summary>
        /// Helps in uniquely getting the preference information to a specific user
        /// </summary>
        /// <typeparam name="T">Type of PreferenceData object</typeparam>
        /// <param name="preferenceId">preferenceId</param>
        /// <returns></returns>
        Task<object> GetUserPreference<T>(Guid preferenceId);

        /// <summary>
        /// Helps in uniquely deleting the preference information to a specific user
        /// </summary>
        /// <param name="preferenceId">preferenceId</param>
        /// <returns></returns>
        Task DeleteUserPreference(Guid preferenceId);

        /// <summary>
        /// Helps in checking the preference information exists to a specific user
        /// </summary>
        /// <param name="preferenceId">preferenceId</param>
        /// <returns></returns>
        Task<bool> UserPreferenceExist(Guid preferenceId);

        /// <summary>
        /// Gets global preference that apply to all users
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="preferenceId">preferenceId</param>
        /// <returns></returns>
        Task<object> GetSystemPreference<T>(Guid preferenceId);

        /// <summary>
        /// Checks global preference exists
        /// </summary>
        /// <param name="preferenceId">preferenceId</param>
        /// <returns></returns>
        Task<bool> SystemPreferenceExist(Guid preferenceId);
    }
}