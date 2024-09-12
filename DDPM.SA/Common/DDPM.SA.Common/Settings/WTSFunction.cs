using Dell.Client.Framework.Common;
using Microsoft.Win32;
using System;
using System.Runtime.InteropServices;
using System.Security.Principal;
using Microsoft.Win32.SafeHandles;
using Windows.Devices.Geolocation;
using System.IO;

namespace DDPM.SA.Common.Settings
{
    public class WTSFunction
    {
        private enum WTS_INFO_CLASS
        {
            WTSUserName = 5,
            WTSDomainName = 7,
        }

        [DllImport("Kernel32.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        private static extern int WTSGetActiveConsoleSessionId();
        private static int _WTSGetActiveConsoleSessionId()
        {
            return WTSGetActiveConsoleSessionId();
        }

        [DllImport("Wtsapi32.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        private static extern bool WTSQuerySessionInformation(IntPtr hServer, int sessionId, WTS_INFO_CLASS wtsInfoClass, out IntPtr ppBuffer, out int pBytesReturned);
        private static bool _WTSQuerySessionInformation(IntPtr hServer, int sessionId, WTS_INFO_CLASS wtsInfoClass, out IntPtr ppBuffer, out int pBytesReturned)
        {
            return WTSQuerySessionInformation(hServer, sessionId, wtsInfoClass, out ppBuffer, out pBytesReturned);
        }

        [DllImport("Wtsapi32.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        private static extern void WTSFreeMemory(IntPtr pointer);
        private static void _WTSFreeMemory(IntPtr pointer)
        {
            WTSFreeMemory(pointer);
        }

        [DllImport("Wtsapi32.dll")]
        private static extern bool WTSQueryUserToken(uint sessionId, out IntPtr Token);
        private static bool _WTSQueryUserToken(uint sessionId, out IntPtr Token)
        {
            return WTSQueryUserToken(sessionId, out Token);
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool CloseHandle(IntPtr hObject);
        public static bool _CloseHandle(IntPtr hObject)
        {
            return CloseHandle(hObject);
        }

        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private extern static bool ImpersonateLoggedOnUser(IntPtr hToken);
        private static bool _ImpersonateLoggedOnUser(IntPtr hToken)
        {
            return ImpersonateLoggedOnUser(hToken);
        }

        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern bool DuplicateTokenEx(IntPtr hExistingToken, uint dwDesiredAccess, IntPtr lpTokenAttributes,
                                        int ImpersonationLevel, int TokenType, out IntPtr phNewToken);
        private static bool _DuplicateTokenEx(IntPtr hExistingToken, uint dwDesiredAccess, IntPtr lpTokenAttributes,
                                        int ImpersonationLevel, int TokenType, out IntPtr phNewToken)
        {
            return DuplicateTokenEx(hExistingToken, dwDesiredAccess, lpTokenAttributes, ImpersonationLevel, TokenType, out phNewToken);
        }

        private enum log_type
        {
            info = 0,
            error
        }

        private static void WriteLog(ILog Log, string text, log_type log_type = log_type.info)
        {
            text = "[WTSFunction] " + text;
            Console.WriteLine(text);
            if (log_type == log_type.info)
                Log.Info(text);
            else
                Log.Error(text);
        }

        //
        //At system session (0) to query user's Sid from active session
        //
        private static string GetUserSid(ILog log, string userName)
        {
            NTAccount f_normal, f_domain = null;
            string accountName = $"{Environment.MachineName}\\{userName}";
            f_normal = new NTAccount(accountName);
            //WriteLog($"GetUserSid: Machine name: {Environment.MachineName}, User name:{userName}");
            if (!string.IsNullOrEmpty(Environment.UserDomainName))
            {
                accountName = $"{Environment.UserDomainName}\\{userName}";
                WriteLog(log, $"GetUserSid: find domain name: {Environment.UserDomainName}, User name:{userName}");
                f_domain = new NTAccount(Environment.UserDomainName, userName);
            }
            //NTAccount f = new NTAccount(accountName);
            //writelog($"GetUserSid: final using: {accountName}");
            String sidString;
            try
            {
                SecurityIdentifier s = (SecurityIdentifier)f_normal.Translate(typeof(SecurityIdentifier));
                sidString = s.ToString();
                WriteLog(log, $"GetUserSid(normal user): SID: {sidString}");
            }
            catch (Exception ex)
            {
                sidString = null;
                WriteLog(log, $"GetUserSid(normal user): try translate fail: {ex.Message}");

                //0724 add code that translate normal user and do translate domain user if fail.
                if (f_domain != null)
                {
                    try
                    {
                        SecurityIdentifier s = (SecurityIdentifier)f_domain.Translate(typeof(SecurityIdentifier));
                        sidString = s.ToString();
                        WriteLog(log, $"GetUserSid(domain user): SID: {sidString}");
                    }
                    catch (Exception e)
                    {
                        sidString = null;
                        WriteLog(log, $"GetUserSid(domain user): try translate fail: {e.Message}");
                    }
                }
            }
            return sidString;
        }

        //
        //At system session (0) to query user's local app data path from active session
        //
        private string GetActiveUserLocalAppDataPath(ILog log)
        {
            IntPtr buffer;
            int bytesReturned = 0;
            int sessionId = _WTSGetActiveConsoleSessionId(); // This gets the session ID of the user logged into the console
            WriteLog(log, $"WTSGetActiveConsoleSessionId: {sessionId}");

            if (_WTSQuerySessionInformation(IntPtr.Zero, sessionId, WTS_INFO_CLASS.WTSUserName, out buffer, out bytesReturned))
            {
                string userName = Marshal.PtrToStringAnsi(buffer);
                _WTSFreeMemory(buffer);
                WriteLog(log, $"WTSQuerySessionInformation: user name ({userName})");

                if (!string.IsNullOrEmpty(userName))
                {
                    string userSid = GetUserSid(log, userName);
                    if (!string.IsNullOrEmpty(userSid))
                    {
                        string regKey = $@"HKEY_USERS\{userSid}\Software\Microsoft\Windows\CurrentVersion\Explorer\Shell Folders";
                        string localAppDataPath = (string)Registry.GetValue(regKey, "Local AppData", null);
                        WriteLog(log, $"Local app data from registry: {localAppDataPath}");
                        return localAppDataPath;
                    }
                }
                else
                {
                    WriteLog(log, "Got null user name");
                }
            }
            else
            {
                WriteLog(log, "WTSQuerySessionInformation: return false");
            }

            return null;
        }

        //
        // Perform this action under system session 0
        // If return true, caller must call "_CloseHandle" to release user token handle
        /*public static bool ImpersonateUser_WithoutCloseHandle(ILog log, out IntPtr userToken)
        {
            userToken = IntPtr.Zero;
            try
            {
                uint id = (uint)_WTSGetActiveConsoleSessionId();
                WriteLog(log, $"_WTSGetActiveConsoleSessionId: {id}");
                if (_WTSQueryUserToken(id, out userToken))
                {
                    if (_ImpersonateLoggedOnUser(userToken))
                    {
                        WriteLog(log, $"ImpersonateUser_WithoutCloseHandle success");
                        return true;
                    }
                    _CloseHandle(userToken);
                }
            }
            catch (Exception e)
            {
                WriteLog(log, $"ImpersonateUser_WithoutCloseHandle failed: {e.Message}");
                if (userToken != IntPtr.Zero)
                    _CloseHandle(userToken);
                userToken = IntPtr.Zero;
            }
            return false;
        }*/

        public static void ImpersonateUser_WriteRegistry(ILog log, string subKey, string keyName, object value)
        {
            uint sessionId = (uint)_WTSGetActiveConsoleSessionId();
            if (WTSQueryUserToken(sessionId, out IntPtr userToken))
            {
                if (DuplicateTokenEx(userToken, 0xF01FF, IntPtr.Zero, 2, 1, out IntPtr duplicatedToken))
                {
                    WindowsIdentity.RunImpersonated(new SafeAccessTokenHandle(duplicatedToken), () =>
                    {
                        using (RegistryKey key = Registry.CurrentUser.OpenSubKey(subKey, true))
                        {
                            if (key != null)
                            {
                                key.SetValue(keyName, value);
                            }
                        }
                    });
                    CloseHandle(duplicatedToken);
                }
                CloseHandle(userToken);
            }
        }

        public static object ImpersonateUser_ReadRegistry(ILog log, string subKey, string keyName)
        {
            object obj = null;
            uint sessionId = (uint)_WTSGetActiveConsoleSessionId();
            if (WTSQueryUserToken(sessionId, out IntPtr userToken))
            {
                if (DuplicateTokenEx(userToken, 0xF01FF, IntPtr.Zero, 2, 1, out IntPtr duplicatedToken))
                {
                    WindowsIdentity.RunImpersonated(new SafeAccessTokenHandle(duplicatedToken), () =>
                    {
                        using (RegistryKey key = Registry.CurrentUser.OpenSubKey(subKey, true))
                        {
                            if (key != null)
                            {
                                obj = key.GetValue(keyName);
                            }
                        }
                    });
                    CloseHandle(duplicatedToken);
                }
                CloseHandle(userToken);
            }
            return obj;
        }
        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        private static extern bool CreateProcessAsUser(IntPtr hToken, string lpApplicationName, string lpCommandLine, IntPtr lpProcessAttributes, IntPtr lpThreadAttributes, bool bInheritHandles, uint dwCreationFlags, IntPtr lpEnvironment, string lpCurrentDirectory, [In] ref STARTUPINFO lpStartupInfo, out PROCESS_INFORMATION lpProcessInformation);
        private static bool _CreateProcessAsUser(IntPtr hToken, string lpApplicationName, string lpCommandLine, IntPtr lpProcessAttributes, IntPtr lpThreadAttributes, bool bInheritHandles, uint dwCreationFlags, IntPtr lpEnvironment, string lpCurrentDirectory, [In] ref STARTUPINFO lpStartupInfo, out PROCESS_INFORMATION lpProcessInformation)
        {
            return CreateProcessAsUser(hToken, lpApplicationName, lpCommandLine, lpProcessAttributes, lpThreadAttributes, bInheritHandles, dwCreationFlags, lpEnvironment, lpCurrentDirectory, ref lpStartupInfo, out lpProcessInformation);
        }
        [StructLayout(LayoutKind.Sequential)]
        private struct STARTUPINFO
        {
            public int cb;
            public string lpReserved;
            public string lpDesktop;
            public string lpTitle;
            public int dwX;
            public int dwY;
            public int dwXSize;
            public int dwYSize;
            public int dwXCountChars;
            public int dwYCountChars;
            public int dwFillAttribute;
            public int dwFlags;
            public short wShowWindow;
            public short cbReserved2;
            public IntPtr lpReserved2;
            public IntPtr hStdInput;
            public IntPtr hStdOutput;
            public IntPtr hStdError;
        }
        [StructLayout(LayoutKind.Sequential)]
        private struct PROCESS_INFORMATION
        {
            public IntPtr hProcess;
            public IntPtr hThread;
            public uint dwProcessId;
            public uint dwThreadId;
        }
        public static void RunElevatedProcess(string applicationPath, string arguments)
        {
            uint sessionId = (uint)_WTSGetActiveConsoleSessionId();
            if (sessionId == 0xFFFFFFFF)
            {
                throw new InvalidOperationException("No active session found.");
            }

            if (WTSQueryUserToken(sessionId, out IntPtr userToken))
            {
                if (_DuplicateTokenEx(userToken, 0xF01FF, IntPtr.Zero, 2, 1, out IntPtr duplicatedToken))
                {
                    STARTUPINFO startupInfo = new STARTUPINFO();
                    PROCESS_INFORMATION processInfo = new PROCESS_INFORMATION();

                    bool result = _CreateProcessAsUser(
                        duplicatedToken,
                        applicationPath,
                        arguments,
                        IntPtr.Zero,
                        IntPtr.Zero,
                        false,
                        0,
                        IntPtr.Zero,
                        Path.GetDirectoryName(applicationPath),
                        ref startupInfo,
                        out processInfo);

                    if (!result)
                    {
                        int errorCode = Marshal.GetLastWin32Error();
                        throw new System.ComponentModel.Win32Exception(errorCode);
                    }

                    CloseHandle(processInfo.hProcess);
                    CloseHandle(processInfo.hThread);
                    CloseHandle(duplicatedToken);
                }
                CloseHandle(userToken);
            }
            else
            {
                int errorCode = Marshal.GetLastWin32Error();
                throw new System.ComponentModel.Win32Exception(errorCode);
            }
        }
    }
}
