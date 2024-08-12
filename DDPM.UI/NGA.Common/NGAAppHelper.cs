#region LicenceHeader
//
// Copyright © 2024, Dell Inc., All Rights Reserved.
// This material is confidential and a trade secret.  Permission to use this
// work for any purpose must be obtained in writing from Dell Inc.
//
#endregion

using Dell.Client.Framework.Common;
using Dell.Client.Framework.Security;
using Dell.Client.Framework.Security.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NGA.Common
{
    /// <summary>
    /// Class to handle Systray and ThickClient at Process level. 
    /// </summary>
    public static class NgaAppHelper
    {
        private const int WmClose = 0x0010;

        /// <summary>
        /// Close Application using WinApi Messages.
        /// </summary>
        /// <param name="processFullPath">Full Path of Application Process to be closed (Systray or ThickClient)</param>
        /// <param name="appWindowTitle">Application Window Title</param>
        /// <param name="log">Log Instance for logging warnings and errors</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="processFullPath"/> and <paramref name="appWindowTitle"/> are null or empty</exception>
        /// <exception cref="InvalidOperationException">This exception is thrown when we specify incorrect process path/file name.</exception>
        public static Task CloseProcessAsync(string processFullPath, string appWindowTitle, ILog? log = null)
        {
            if (string.IsNullOrEmpty(processFullPath))
                throw new ArgumentNullException(paramName: nameof(processFullPath), $"{nameof(processFullPath)} is cannot be null or empty.");
            if (string.IsNullOrEmpty(appWindowTitle))
                throw new ArgumentNullException(paramName: nameof(appWindowTitle), $"{nameof(appWindowTitle)} is cannot be null or empty.");

            return CloseProcessInternalAsync(processFullPath, appWindowTitle, log);
        }

        /// <summary>
        /// Starts the process with Application Name.
        /// </summary>
        /// <param name="processFullPath">Full Path of Application Process to be Started( Systray or ThickClient)</param>
        /// <param name="pluginManager">PluginManager Instance</param>
        /// <param name="log">Log Instance for logging warnings and errors</param>
        /// <param name="arguments">Arguments to be passed while starting the process</param>
        /// <returns><see cref="Boolean"/></returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="processFullPath"/> and <paramref name="pluginManager"/> are null or empty</exception>
        /// <exception cref="InvalidOperationException">This exception is thrown when we specify incorrect process path/file name.</exception>
        public static bool ValidateAndStartAppProcess(
            string processFullPath,
            IPluginManager pluginManager,
            ILog? log = null, string? arguments = null)
        {
            var applicationStarted = false;

            if (string.IsNullOrEmpty(processFullPath))
                throw new ArgumentNullException(nameof(processFullPath));

            if (pluginManager == null)
                throw new ArgumentNullException(nameof(pluginManager));

            try
            {
                var appName = Path.GetFileName(processFullPath);

                // PathHelper.CheckPathRedirection does all the validations on processFullPath, also throws appropriate exceptions
                var pathRedirection = PathHelper.CheckPathRedirection(processFullPath);
                
                if (pathRedirection != PathRedirectionReturn.PathIsNormal)
                    throw new ArgumentException($"appName is {appName}, PathRedirection value is {pathRedirection}");

                // Verify that the file is trusted.
                using var fileLock = new FileLock(processFullPath, PathCheckOption.IgnoreUnrootedPath | PathCheckOption.IgnoreRelativePath,
                    true, new FileLockOptions(FileAccess.Read, FileShare.Read));

                if (!pluginManager.IsAssemblySignedAndTrusted(fileLock, true))
                    throw new InvalidOperationException($"File {Path.GetFileName(processFullPath)} is not trusted.");

                //Check whether NGA Thick client is running for current user
                var currentAppSessionId = Process.GetCurrentProcess().SessionId;

                var appProcessName = Process.
                    GetProcessesByName(Path.GetFileNameWithoutExtension(processFullPath)).
                    SingleOrDefault(p => p.SessionId == currentAppSessionId);

                if (appProcessName == null)
                    applicationStarted = true;

                if (arguments != null)
                    Process.Start(processFullPath, arguments);
                else
                    Process.Start(processFullPath);

                return applicationStarted;
            }
            catch (InvalidOperationException ex)
            {
                const string message = $"{nameof(ValidateAndStartAppProcess)} - Please check the process file path/file name.";
                log?.Error(ex, message);
                return false;
            }
            catch (Exception ex)
            {
                const string message = $"{nameof(ValidateAndStartAppProcess)} - exception starting user process.";
                log?.Error(ex, message);
                return false;
            }
        }

        /// <summary>
        /// Find all windows whose window title starts with the given text.
        /// </summary>
        /// <param name="titleText">Text to be searched.</param>
        public static IEnumerable<IntPtr> FindWindowsByWindowTitlesThatStartWithText(string titleText)
        {
            return FindWindowsWithMatchingText(delegate (IntPtr wnd, IntPtr param)
            {
                return GetWindowTitleText(wnd).StartsWith(titleText);
            });
        }

        /// <summary>
        /// Returns Window Handle on passing window title. 
        /// </summary>
        /// <returns cref="IntPtr">Windows Handle</returns>
        ///<exception cref = "ArgumentNullException" > Thrown if <paramref name="mainWindowTitle"/> is null or empty</exception>
        private static IntPtr FindWindowByWindowTitle(string mainWindowTitle)
        {
            if (string.IsNullOrEmpty(mainWindowTitle))
                throw new ArgumentNullException(nameof(mainWindowTitle));

            return WinApi.NativeMethods.FindWindow(null, mainWindowTitle);
        }

        /// <summary>
        /// Close Application using WinApi Messages.
        /// </summary>
        /// <param name="processFullPath">Full Path of Application Process to be closed( Systray or ThickClient)</param>
        /// <param name="appWindowTitle">Application Window Title</param>
        /// <param name="log">Log Instance for logging warnings and errors</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="processFullPath"/> and <paramref name="appWindowTitle"/> are null or empty</exception>
        /// <exception cref="InvalidOperationException">This exception is thrown when we specify incorrect process path/file name.</exception>
        private static async Task CloseProcessInternalAsync(string processFullPath, string appWindowTitle, ILog? log = null)
        {
            await Task.Run(() =>
            {
                try
                {
                    var appName = Path.GetFileName(processFullPath);

                    // Check whether NGA App is running for current user
                    var currentSessionId = Process.GetCurrentProcess().SessionId;
                    var appProcess = Process.
                            GetProcessesByName(Path.GetFileNameWithoutExtension(appName)).
                            SingleOrDefault(p => p.SessionId == currentSessionId);

                    if (appProcess == null)
                        return;

                    appProcess.Refresh();
                    appProcess.WaitForInputIdle();

                    var appHandle = FindWindowByWindowTitle(appWindowTitle);

                    if (appHandle == IntPtr.Zero)
                    {
                        var appHandles = FindWindowsByWindowTitlesThatStartWithText(appWindowTitle);

                        // Return unless there is exactly one match
                        if (appHandles == null || appHandles.Count() != 1)
                        {
                            log?.Error("Unable to get Handle - Windows Handle Error");
                            return;
                        }

                        appHandle = appHandles.First();
                    }

                    WinApi.NativeMethods.SendMessage(appHandle, WmClose, IntPtr.Zero, IntPtr.Zero);

                    if (!appProcess.HasExited)
                        appProcess.WaitForExit();
                }
                catch (InvalidOperationException ex)
                {
                    const string message = $"{nameof(CloseProcessAsync)} - Please check the process file path/file name";
                    log?.Error(ex, message);
                }
                catch (Exception ex)
                {
                    const string message = $" {nameof(CloseProcessAsync)} - exception while sending wnd_proc close message";
                    log?.Error(ex, message);
                }
            });
        }

        /// <summary>
        /// Find all windows that match the given text matching criteria defined
        /// within a delegate.
        /// </summary>
        /// <param name="TextMatchingProc">
        /// A delegate that checks if a window matches some given criteria.
        /// If so, it reurns true. Else it returns false.
        /// </param>
        private static IEnumerable<IntPtr> FindWindowsWithMatchingText(WinApi.NativeMethods.EnumWindowsProc TextMatchingProc)
        {
            List<IntPtr> windows = new();

            WinApi.NativeMethods.EnumWindows(delegate (IntPtr wnd, IntPtr param)
            {
                if (TextMatchingProc(wnd, param))
                {
                    // Window passed the crieria. Add it to the lsit.
                    windows.Add(wnd);
                }

                // Return true here so that we iterate over all windows.
                return true;
            }, IntPtr.Zero);

            return windows;
        }

        /// <summary>
        /// Get the text of the specified window's title into a buffer.
        /// </summary>
        private static string GetWindowTitleText(IntPtr hWnd)
        {
            int length = WinApi.NativeMethods.GetWindowTextLength(hWnd);
            if (length > 0)
            {
                var sb = new StringBuilder(length + 1);
                if (WinApi.NativeMethods.GetWindowText(hWnd, sb, sb.Capacity) != 0)
                    return sb.ToString();
            }

            return string.Empty;
        }
    }
}
