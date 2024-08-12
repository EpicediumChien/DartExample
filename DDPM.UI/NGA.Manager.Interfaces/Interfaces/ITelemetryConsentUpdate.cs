#region LicenceHeader
//
// Copyright © 2024, Dell Inc., All Rights Reserved.
// This material is confidential and a trade secret.  Permission to use this
// work for any purpose must be obtained in writing from Dell Inc.
//
#endregion

using System;
using System.Threading;
using System.Threading.Tasks;
using Dell.Client.Framework.Common.PluginConditions;

namespace NGA.Manager.Interfaces
{
    /// <summary>
    /// Represents an interface for asynchronously updating telemetry consent for both global system preferences and user-specific preferences.
    /// </summary>
    public interface ITelemetryConsentUpdate : ITelemetryConsentUpdateUnelevated
    {
        /// <summary>
        /// Asynchronously updates the telemetry consent for both global system preferences and user-specific preferences.
        /// Acquires a write lock using a ReaderWriterLockSlim object to ensure thread safety during the update process.
        /// Backwards compatibility is maintained by using both IPreference.SetSystemPreferenceAsync and
        /// IPreferenceUnelevated.SetUserPreferenceAsync methods to store the global and user telemetry consent values, respectively.
        /// </summary> 
        /// <param name="customerConsent">A boolean representing the customer's agreement or disagreement to telemetry consent.</param>
        /// <param name="cancellationToken">A cancellation token to cancel the asynchronous operation if needed.</param>
        /// <remarks>
        /// This method ensures the atomicity of the telemetry consent update by acquiring a write lock before proceeding.
        /// The global telemetry consent value is updated using IPreference.SetSystemPreferenceAsync.
        /// Additionally, to maintain compatibility, the user-specific telemetry consent value is updated using IPreferenceUnelevated.SetUserPreferenceAsync. 
        /// </remarks>
        /// <exception cref="Exception">Thrown when the <see cref="IPreference"/> plugin is null or is in <see cref="PluginErrorCondition"/></exception>
        /// <exception cref="GetTelemetryConsentException">Thrown when there is no system preference data for the telemetry consent</exception>
        /// <exception cref="OperationCanceledException">Thrown when the telemetry consent plugin is stopped for unknown reason"></exception>
        /// <exception cref="SetTelemetryConsentException">Thrown when any error occurs during the execution</exception>
        /// <exception cref="SetPreferenceException">Thrown when there is while trying to write to preference.
        /// This exception will only be logged and null will be returned</exception>
        public Task UpdateTelemetryConsentAsync(bool customerConsent, CancellationToken cancellationToken);
    }
}
