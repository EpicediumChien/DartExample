#region LicenceHeader

//
// Copyright © 2022, Dell Inc., All Rights Reserved.
// This material is confidential and a trade secret.  Permission to use this
// work for any purpose must be obtained in writing from Dell Inc.
//

#endregion

using System.Runtime.InteropServices;

namespace NGA.ThickClientCore
{
    /// <summary>
    /// This class contains implementation for user32.dll Native Methods.
    /// </summary>
    internal static class NativeMethods
    {
        [DllImport("USER32.DLL", CharSet = CharSet.Unicode)]
        internal static extern IntPtr FindWindow(string? lpClassName, string lpWindowName);
    }
}