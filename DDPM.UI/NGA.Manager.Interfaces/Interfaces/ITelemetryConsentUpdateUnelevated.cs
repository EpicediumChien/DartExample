#region LicenceHeader

//
// Copyright © 2024, Dell Inc., All Rights Reserved.
// This material is confidential and a trade secret.  Permission to use this
// work for any purpose must be obtained in writing from Dell Inc.
//

#endregion

using Dell.Client.Framework.Common.PluginConditions;

namespace NGA.Manager.Interfaces
{
    /// <summary>
    /// Represents an interface for asynchronously updating telemetry consent, specifically for scenarios where elevation is not required.
    /// </summary>
    public interface ITelemetryConsentUpdateUnelevated : ITelemetryConsent
    {
        /// <summary>
        /// Asynchronously updates the telemetry consent for both global system preferences and user-specific preferences.
        /// Acquires a write lock using a ReaderWriterLockSlim object to ensure thread safety during the update process.
        /// Backwards compatibility is maintained by using both IPreference.SetSystemPreferenceAsync and
        /// IPreferenceUnelevated.SetUserPreferenceAsync methods to store the global and user telemetry consent values, respectively.
        /// If the ConsentConfirmed registry value is set, an InvalidOperationException is thrown to prevent unauthorized updates.
        /// </summary>
        /// <param name="customerConsent">A boolean representing the customer's agreement or disagreement to telemetry consent.</param>
        /// <param name="cancellationToken">A cancellation token to cancel the asynchronous operation if needed.</param>
        /// <remarks>
        /// This method ensures the atomicity of the telemetry consent update by acquiring a write lock before proceeding.
        /// It first checks if the ConsentConfirmed registry value is set, and if so, it prevents any further updates and throws an exception.
        /// The global telemetry consent value is then updated using IPreference.SetSystemPreferenceAsync.
        /// Additionally, to maintain compatibility, the user-specific telemetry consent value is updated using IPreferenceUnelevated.SetUserPreferenceAsync.
        /// </remarks>
        /// <exception cref="NullReferenceException">Thrown when the <see cref="IPreference"/> plugin is null</exception>
        /// <exception cref="InvalidOperationException">Thrown when <see cref="IPreference"/> plugin is in <see cref="PluginErrorCondition"/></exception>
        /// <exception cref="GetTelemetryConsentException">Thrown when there is no system preference data for the telemetry consent</exception>
        /// <exception cref="OperationCanceledException">Thrown when the telemetry consent plugin is stopped for unknown reason"></exception>
        /// <exception cref="SetTelemetryConsentException">Thrown when any error occurs during the execution</exception>
        /// <exception cref="SetPreferenceException">Thrown when there is while trying to write to preference.
        /// This exception will only be logged and null will be returned</exception>
        public Task UpdateTelemetryConsentUnelevatedAsync(bool customerConsent, CancellationToken cancellationToken);
    }
}