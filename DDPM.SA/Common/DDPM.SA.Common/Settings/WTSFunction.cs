using Dell.Client.Framework.Common;
using Microsoft.Win32;
using System;
using System.Runtime.InteropServices;
using System.Security.Principal;
using Microsoft.Win32.SafeHandles;
using Windows.Devices.Geolocation;
using System.IO;
using PInvoke;
using System.Diagnostics;
using System.Security;
using System.Collections.Generic;

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
        public static int _WTSGetActiveConsoleSessionId()
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
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        private static extern bool WTSQueryUserToken(uint sessionId, out IntPtr Token);
        private static bool _WTSQueryUserToken(uint sessionId, out IntPtr Token)
        {
            return WTSQueryUserToken(sessionId, out Token);
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        private static extern bool CloseHandle(IntPtr hObject);
        public static bool _CloseHandle(IntPtr hObject)
        {
            return CloseHandle(hObject);
        }

        [DllImport("advapi32.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        private static extern bool DuplicateTokenEx(IntPtr hExistingToken, uint dwDesiredAccess, ref SECURITY_ATTRIBUTES lpTokenAttributes,
                                        int ImpersonationLevel, int TokenType, out IntPtr phNewToken);
        private static bool _DuplicateTokenEx(IntPtr hExistingToken, uint dwDesiredAccess, ref SECURITY_ATTRIBUTES lpTokenAttributes,
                                        int ImpersonationLevel, int TokenType, out IntPtr phNewToken)
        {
            return DuplicateTokenEx(hExistingToken, dwDesiredAccess, ref lpTokenAttributes, ImpersonationLevel, TokenType, out phNewToken);
        }

        [DllImport("advapi32", SetLastError = true), SuppressUnmanagedCodeSecurityAttribute]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        private static extern bool OpenProcessToken(IntPtr ProcessHandle, int DesiredAccess, ref IntPtr TokenHandle);
        public static bool _OpenProcessToken(IntPtr ProcessHandle, int DesiredAccess, ref IntPtr TokenHandle)
        {
            return OpenProcessToken(ProcessHandle, DesiredAccess, ref TokenHandle);
        }

        [DllImport("kernel32.dll")]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        private static extern IntPtr OpenProcess(uint dwDesiredAccess, bool bInheritHandle, uint dwProcessId);
        private static IntPtr _OpenProcess(uint dwDesiredAccess, bool bInheritHandle, uint dwProcessId)
        {
            return OpenProcess(dwDesiredAccess, bInheritHandle, dwProcessId);
        }

        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        private static extern bool CreateProcessAsUser(IntPtr hToken, string lpApplicationName, string lpCommandLine, ref SECURITY_ATTRIBUTES lpProcessAttributes, ref SECURITY_ATTRIBUTES lpThreadAttributes, bool bInheritHandles, uint dwCreationFlags, IntPtr lpEnvironment, string lpCurrentDirectory, [In] ref STARTUPINFO lpStartupInfo, out PInvoke.PROCESS_INFORMATION lpProcessInformation);
        private static bool _CreateProcessAsUser(IntPtr hToken, string lpApplicationName, string lpCommandLine, ref SECURITY_ATTRIBUTES lpProcessAttributes, ref SECURITY_ATTRIBUTES lpThreadAttributes, bool bInheritHandles, uint dwCreationFlags, IntPtr lpEnvironment, string lpCurrentDirectory, [In] ref STARTUPINFO lpStartupInfo, out PInvoke.PROCESS_INFORMATION lpProcessInformation)
        {
            return CreateProcessAsUser(hToken, lpApplicationName, lpCommandLine, ref lpProcessAttributes, ref lpThreadAttributes, bInheritHandles, dwCreationFlags, lpEnvironment, lpCurrentDirectory, ref lpStartupInfo, out lpProcessInformation);
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

        private const uint MAXIMUM_ALLOWED = 0x2000000;
        private const int TOKEN_DUPLICATE = 0x0002;
        private const int CREATE_NEW_CONSOLE = 0x00000010;
        private const int NORMAL_PRIORITY_CLASS = 0x20;

        private enum log_type
        {
            info = 0,
            error
        }

        private static void WriteLog(ILog Log, string text, log_type log_type = log_type.info)
        {
            text = "[WTSFunction] " + text;
            Console.WriteLine(text);
            if (Log != null)
            {
                if (log_type == log_type.info)
                    Log.Info(text);
                else
                    Log.Error(text);
            }
        }

        public static string DirectGetUserID(ILog log)
        {
            IntPtr buffer;
            int bytesReturned = 0;
            int sessionId = _WTSGetActiveConsoleSessionId(); // This gets the session ID of the user logged into the console
            WriteLog(log, $"DirectGetUserID: {sessionId}");

            if (_WTSQuerySessionInformation(IntPtr.Zero, sessionId, WTS_INFO_CLASS.WTSUserName, out buffer, out bytesReturned))
            {
                string userName = Marshal.PtrToStringAnsi(buffer);
                _WTSFreeMemory(buffer);
                WriteLog(log, $"DirectGetUserID: user name ({userName})");

                if (!string.IsNullOrEmpty(userName))
                {
                    string userSid = GetUserSid(log, userName);
                    if (!string.IsNullOrEmpty(userSid))
                    {
                        WriteLog(log, $"userSid : {userSid}");
                        return userSid;
                    }
                }
                else
                {
                    WriteLog(log, "Got null user name");
                }
            }
            else
            {
                WriteLog(log, "DirectGetUserID: return false");
            }

            return null;
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
        public static string GetActiveUserLocalAppDataPath(ILog log)
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
            if (_WTSQueryUserToken(sessionId, out IntPtr userToken))
            {
                SECURITY_ATTRIBUTES sa = new SECURITY_ATTRIBUTES();
                sa.Length = Marshal.SizeOf(sa);
                if (_DuplicateTokenEx(userToken, 0xF01FF, ref sa, 2, 1, out IntPtr duplicatedToken))
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
                    _CloseHandle(duplicatedToken);
                }
                _CloseHandle(userToken);
            }
        }

        public static object ImpersonateUser_ReadRegistry(ILog log, string subKey, string keyName)
        {
            object obj = null;
            uint sessionId = (uint)_WTSGetActiveConsoleSessionId();
            if (_WTSQueryUserToken(sessionId, out IntPtr userToken))
            {
                SECURITY_ATTRIBUTES sa = new SECURITY_ATTRIBUTES();
                sa.Length = Marshal.SizeOf(sa);
                if (_DuplicateTokenEx(userToken, 0xF01FF, ref sa, 2, 1, out IntPtr duplicatedToken))
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
                    _CloseHandle(duplicatedToken);
                }
                _CloseHandle(userToken);
            }
            return obj;
        }
        
        /*public static void RunElevatedProcess(string applicationPath, string arguments)
        {
            uint sessionId = (uint)_WTSGetActiveConsoleSessionId();
            if (sessionId == 0xFFFFFFFF)
            {
                throw new InvalidOperationException("No active session found.");
            }

            if (_WTSQueryUserToken(sessionId, out IntPtr userToken))
            {
                SECURITY_ATTRIBUTES sa = new SECURITY_ATTRIBUTES();
                sa.Length = Marshal.SizeOf(sa);
                if (_DuplicateTokenEx(userToken, 0xF01FF, ref sa, 2, 1, out IntPtr duplicatedToken))
                {
                    STARTUPINFO startupInfo = new STARTUPINFO();
                    PROCESS_INFORMATION processInfo = new PROCESS_INFORMATION();

                    bool result = _CreateProcessAsUser(
                        duplicatedToken,
                        applicationPath,
                        arguments,
                        ref processAttributes,
                        ref threadAttributes,
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

                    _CloseHandle(processInfo.hProcess);
                    _CloseHandle(processInfo.hThread);
                    _CloseHandle(duplicatedToken);
                }
                _CloseHandle(userToken);
            }
            else
            {
                int errorCode = Marshal.GetLastWin32Error();
                throw new System.ComponentModel.Win32Exception(errorCode);
            }
        }*/

        public static void LaunchProcessWithUserAccountAndElevated(string applicationPath)
        {
            IntPtr userToken = IntPtr.Zero;
            IntPtr duplicatedToken = IntPtr.Zero;

            try
            {
                int sessionId = _WTSGetActiveConsoleSessionId();
                if (_WTSQueryUserToken((uint)sessionId, out userToken))
                {
                    SECURITY_ATTRIBUTES sa = new SECURITY_ATTRIBUTES();
                    sa.Length = Marshal.SizeOf(sa);
                    if (_DuplicateTokenEx(
                        userToken, 0xF01FF, ref sa,
                        (int)PInvoke.SECURITY_IMPERSONATION_LEVEL.SecurityImpersonation,
                        (int)PInvoke.TOKEN_TYPE.TokenPrimary,
                        out duplicatedToken))
                    {
                        STARTUPINFO si = new STARTUPINFO();
                        PInvoke.PROCESS_INFORMATION pi = new PInvoke.PROCESS_INFORMATION();
                        SECURITY_ATTRIBUTES processAttributes = new SECURITY_ATTRIBUTES();
                        SECURITY_ATTRIBUTES threadAttributes = new SECURITY_ATTRIBUTES();
                        

                        if (!_CreateProcessAsUser(duplicatedToken, applicationPath, null, ref processAttributes, ref threadAttributes, false, 0, IntPtr.Zero, null, ref si, out pi))
                        {
                            throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error());
                        }
                    }
                }
            }
            finally
            {
                if (userToken != IntPtr.Zero)
                    _CloseHandle(userToken);
                if (duplicatedToken != IntPtr.Zero)
                    _CloseHandle(duplicatedToken);
            }
        }

        /// <summary>
        /// Launches the given application with full admin rights, and in addition bypasses the Vista UAC prompt
        /// </summary>
        /// <param name="applicationName">The name of the application to launch</param>
        /// <param name="procInfo">Process information regarding the launched application that gets returned to the caller</param>
        /// <returns></returns>
        public static bool StartProcessAndBypassUACWithAdmin(string applicationName,string workingDirectory, out PInvoke.PROCESS_INFORMATION procInfo)
        {
            uint winlogonPid = 0;
            IntPtr hUserTokenDup = IntPtr.Zero, hPToken = IntPtr.Zero, hProcess = IntPtr.Zero;
            procInfo = new PInvoke.PROCESS_INFORMATION();

            // obtain the currently active session id; every logged on user in the system has a unique session id
            uint dwSessionId = (uint)_WTSGetActiveConsoleSessionId();

            // obtain the process id of the winlogon process that is running within the currently active session
            Process[] processes = Process.GetProcessesByName("winlogon");
            foreach (Process p in processes)
            {
                if ((uint)p.SessionId == dwSessionId)
                {
                    winlogonPid = (uint)p.Id;
                }
            }

            // obtain a handle to the winlogon process
            hProcess = _OpenProcess(MAXIMUM_ALLOWED, false, winlogonPid);

            // obtain a handle to the access token of the winlogon process
            if (!_OpenProcessToken(hProcess, TOKEN_DUPLICATE, ref hPToken))
            {
                _CloseHandle(hProcess);
                return false;
            }

            // Security attibute structure used in DuplicateTokenEx and CreateProcessAsUser
            // I would prefer to not have to use a security attribute variable and to just 
            // simply pass null and inherit (by default) the security attributes
            // of the existing token. However, in C# structures are value types and therefore
            // cannot be assigned the null value.
            SECURITY_ATTRIBUTES sa = new SECURITY_ATTRIBUTES();
            sa.Length = Marshal.SizeOf(sa);

            // copy the access token of the winlogon process; the newly created token will be a primary token
            if (!_DuplicateTokenEx(
                hPToken, MAXIMUM_ALLOWED, ref sa,
                (int)PInvoke.SECURITY_IMPERSONATION_LEVEL.SecurityIdentification,
                (int)PInvoke.TOKEN_TYPE.TokenPrimary,
                out hUserTokenDup))
            {
                _CloseHandle(hProcess);
                _CloseHandle(hPToken);
                return false;
            }

            // By default CreateProcessAsUser creates a process on a non-interactive window station, meaning
            // the window station has a desktop that is invisible and the process is incapable of receiving
            // user input. To remedy this we set the lpDesktop parameter to indicate we want to enable user 
            // interaction with the new process.
            STARTUPINFO si = new STARTUPINFO();
            si.cb = (int)Marshal.SizeOf(si);
            //si.lpDesktop = @"winsta0\default"; // interactive window station parameter; basically this indicates that the process created can display a GUI on the desktop

            // flags that specify the priority and creation method of the process
            int dwCreationFlags = NORMAL_PRIORITY_CLASS | CREATE_NEW_CONSOLE;

            // create a new process in the current user's logon session
            bool result = _CreateProcessAsUser(hUserTokenDup,        // client's access token
                                            null,                   // file to execute
                                            applicationName,        // command line
                                            ref sa,                 // pointer to process SECURITY_ATTRIBUTES
                                            ref sa,                 // pointer to thread SECURITY_ATTRIBUTES
                                            false,                  // handles are not inheritable
                                            (uint)dwCreationFlags,        // creation flags
                                            IntPtr.Zero,            // pointer to new environment block 
                                            workingDirectory,                   // name of current directory 
                                            ref si,                 // pointer to STARTUPINFO structure
                                            out procInfo            // receives information about new process
                                            );

            // invalidate the handles
            _CloseHandle(hProcess);
            _CloseHandle(hPToken);
            _CloseHandle(hUserTokenDup);

            return result;
        }

        public static int GetCurrentActiveSessionId()
        {
            return _WTSGetActiveConsoleSessionId();
        }

        public static int GetCurrentUserSessionId()
        {
            Process currentProcess = Process.GetCurrentProcess();
            int sessionId = currentProcess.SessionId;
            return sessionId;
        }

        public static int GetSessionIdFromProcessId(int processId)
        {
            try
            {
                // Get the process by ID
                Process process = Process.GetProcessById(processId);
                int sessionId = process.SessionId;
                Console.WriteLine($"Process with ID {processId} has session ID: {sessionId}");
                return sessionId;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}. Process ID {processId} might not exist.");
                return -1;
            }
        }

        public static List<int> GetSessionIdFromProcessId(string processName)
        {
            List<int> result = new List<int>();
            if(string.IsNullOrEmpty(processName))
            {
                result.Clear();
                return result;
            }
            Process[] processes = Process.GetProcessesByName(processName);
            foreach (Process process in processes)
            {
                Console.WriteLine($"Process {process.ProcessName} with ID {process.Id} has session ID: {process.SessionId}");
                result.Add(process.SessionId);
            }
            return result;
        }

        public static bool IsYourProcessInActiveSession(ILog log)
        {
            int act = GetCurrentActiveSessionId();
            int cur = GetCurrentUserSessionId();
            WriteLog(log, $"Current active Session is: {act}, Process created in session: {cur}");
            return (cur == act);
        }
    }
}
