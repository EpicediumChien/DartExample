#region LicenceHeader
//
// Copyright © 2022, Dell Inc., All Rights Reserved.
// This material is confidential and a trade secret.  Permission to use this
// work for any purpose must be obtained in writing from Dell Inc.
//
#endregion

namespace NGA.Manager.Interfaces;

/// <summary>
/// BodyParametersExtension
/// </summary>
public static class LatestResponseTypeExtension
{
    /// <summary>
    /// IsValidateTransition: True if latestResponseType to newLatestResponseType is a validate transition
    /// </summary>
    /// <param name="latestResponseType"></param>
    /// <param name="newLatestResponseType"></param>
    /// <returns></returns>
    public static bool IsValidateTransition(this LatestResponseType? latestResponseType, LatestResponseType newLatestResponseType)
    {
        return
            latestResponseType == LatestResponseType.UserActivated && newLatestResponseType == LatestResponseType.UserActivated ||           // Probably won't happen, but won't hurt
            latestResponseType == LatestResponseType.RemoveAcknowledged && newLatestResponseType == LatestResponseType.RemoveAcknowledged || // Probably won't happen, but won't hurt
            latestResponseType == LatestResponseType.RemoveAcknowledged && newLatestResponseType == LatestResponseType.UserActivated ||
            latestResponseType == LatestResponseType.UpdateAcknowledged && newLatestResponseType == LatestResponseType.UpdateAcknowledged ||
            latestResponseType == LatestResponseType.UpdateAcknowledged && newLatestResponseType == LatestResponseType.RemoveAcknowledged ||
            latestResponseType == LatestResponseType.UpdateAcknowledged && newLatestResponseType == LatestResponseType.UserActivated ||
            latestResponseType == LatestResponseType.NewAcknowledged && newLatestResponseType == LatestResponseType.UpdateAcknowledged ||
            latestResponseType == LatestResponseType.NewAcknowledged && newLatestResponseType == LatestResponseType.RemoveAcknowledged ||
            latestResponseType == LatestResponseType.NewAcknowledged && newLatestResponseType == LatestResponseType.UserActivated ||
            latestResponseType == null && newLatestResponseType == LatestResponseType.NewAcknowledged ||
            latestResponseType == null && newLatestResponseType == LatestResponseType.UserActivated;
    }
}