#region LicenceHeader
//
// Copyright © 2022, Dell Inc., All Rights Reserved.
// This material is confidential and a trade secret.  Permission to use this
// work for any purpose must be obtained in writing from Dell Inc.
//
#endregion

using Microsoft;
using System;
using System.Linq;
using System.Security.Principal;

namespace NGA.Common.Helpers
{
    /// <summary>
    /// Helpers class
    /// </summary>
    public static class SessionHelpers
    {
        private static readonly WellKnownSidType[] WellKnownSids = (WellKnownSidType[])Enum.GetValues(typeof(WellKnownSidType));

        /// <summary>
        /// Method to get the SessionId of the current process
        /// </summary>
        /// <returns></returns>
        public static uint GetCurrentSessionId()
        {
            return (uint)System.Diagnostics.Process.GetCurrentProcess().SessionId;
        }

        /// <summary>
        /// Check for valid SID
        /// Sample format of sid is "S-1-5-21-1971345664-1559653683-1850952788-763819"
        /// </summary>
        /// <param name="userSid">UserSid</param>
        /// <returns>Returns True if <paramref name="userSid"/> is an account sid. Otherwise false is returned.</returns>  
        public static bool IsAccountSid(string userSid)
        {
            Requires.NotNull(userSid, nameof(userSid));
            Requires.NotNullOrWhiteSpace(userSid, nameof(userSid));

            SecurityIdentifier id = new(userSid);
            return id.IsAccountSid();
        }

        /// <summary>
        /// Is sid is of WellKnown sid
        /// </summary>
        /// <param name="userSid">UserSid</param>
        /// <returns>Returns True if <paramref name="userSid"/> is a well known sid. Otherwise false is returned.</returns>  
        public static bool IsWellKnownSid(string userSid)
        {
            Requires.NotNull(userSid, nameof(userSid));
            Requires.NotNullOrWhiteSpace(userSid, nameof(userSid));

            SecurityIdentifier id = new(userSid);
            return WellKnownSids.Any(x => id.IsWellKnown(x));
        }
    }
}