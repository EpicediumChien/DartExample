#region LicenceHeader
//
// Copyright © 2024, Dell Inc., All Rights Reserved.
// This material is confidential and a trade secret.  Permission to use this
// work for any purpose must be obtained in writing from Dell Inc.
//
#endregion

using System;
using System.Threading.Tasks;
using Dell.Client.Framework.Common.PluginConditions;

namespace NGA.Manager.Interfaces
{
    /// <summary>
    /// This allows plugins to retrieve the Telemetry Consent value.
    /// </summary>
    public interface ITelemetryConsent
    {
        /// <summary>
        /// EventHandler for Telemetry Consent Changed event
        /// </summary>
        event EventHandler TelemetryConsentChanged;

        /// <summary>
        /// Returns the telemetry consent value
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception">Thrown when the <see cref="IPreference"/> plugin is null or is in <see cref="PluginErrorCondition"/></exception>
        /// <exception cref="OperationCanceledException">Thrown when the telemetry consent plugin is stopped for unknown reason"></exception>
        /// <exception cref="GetTelemetryConsentException">Thrown when there is no system preference data available for the telemetry consent</exception>
        Task<bool> GetTelemetryConsentAsync();
    }
}