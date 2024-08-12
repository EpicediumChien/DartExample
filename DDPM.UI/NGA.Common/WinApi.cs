#region LicenceHeader
//
// Copyright © 2022, Dell Inc., All Rights Reserved.
// This material is confidential and a trade secret.  Permission to use this
// work for any purpose must be obtained in writing from Dell Inc.
//
#endregion

using System;
using System.Runtime.InteropServices;
using System.Text;

namespace NGA.Common
{
    /// <summary>
    /// Provides Windows internal definitions for invoking native functions.
    /// </summary>
    internal static class WinApi
    {
        /// <summary>
        /// Defines NativeMethods that are not exposed out to the application.
        /// </summary>
        public static class NativeMethods
        {
            /// <summary>
            /// Send message
            /// </summary>
            /// <param name="windowHandle"></param>
            /// <param name="msg"></param>
            /// <param name="wParam"></param>
            /// <param name="lParam"></param>
            /// <returns></returns>
            [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            internal static extern IntPtr SendMessage(IntPtr windowHandle, uint msg, IntPtr wParam, IntPtr lParam);

            /// <summary>
            /// Finds a window based on name
            /// </summary>
            /// <param name="lpClassName"></param>
            /// <param name="lpWindowName"></param>
            /// <returns>The window handle</returns>
            /// <remarks>We have this method defined in Dell.Client.Framework.UX.WPF.WinApi.NativeMethods but, 
            /// copying it here as it's internal to use in WPF and we need this at Application level. </remarks>
            [DllImport("USER32.DLL", CharSet = CharSet.Unicode)]
            internal static extern IntPtr FindWindow(string? lpClassName, string lpWindowName);

            /// <summary>
            /// Copies the text of the specified window's title bar (if it has one) into a buffer.
            /// </summary>
            /// <param name="hWnd"></param>
            /// <param name="lpString"></param>
            /// <param name="maxCount"></param>
            /// <returns>
            /// If the function succeeds, the return value is the length, in characters, of the copied string,
            /// not including the terminating null character. If the window has no title bar or text,
            /// if the title bar is empty, or if the window or control handle is invalid, the return value is zero.
            /// </returns>
            [DllImport("user32.dll", CharSet = CharSet.Unicode)]
            internal static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int maxCount);

            /// <summary>
            /// Retrieves the length, in characters, of the specified window's title bar text
            /// (if the window has a title bar). 
            /// </summary>
            /// <param name="hWnd"></param>
            /// <returns>
            /// If the function succeeds, the return value is the length, in characters, of the text.
            /// If the window has no text, the return value is zero.
            /// </returns>
            [DllImport("user32.dll", CharSet = CharSet.Unicode)]
            internal static extern int GetWindowTextLength(IntPtr hWnd);

            /// <summary>
            /// Enumerates all top-level windows on the screen by passing the handle to each window,
            /// in turn, to an application-defined callback function.
            /// </summary>
            /// <param name="lpEnumFunc"></param>
            /// <param name="lParam"></param>
            /// <returns>Value indicating if the function succeeded or failed.</returns>
            [DllImport("user32.dll")]
            internal static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);

            /// <summary>
            /// Delegate function that filters windows based on some criteria.
            /// </summary>
            /// <param name="hWnd"></param>
            /// <param name="lParam"></param>
            /// <returns></returns>
            internal delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);
        }
    }
}
