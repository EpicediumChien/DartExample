using Microsoft.Win32.SafeHandles;
using PInvoke;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace DDPM.SA.Common
{
    public static class UserImpersonator
    {
        public record UserTokenInfo(IntPtr Token, SECURITY_ATTRIBUTES SecurityAttributes);

        public static void RunAsLoggedOnUser(Action action) => RunAsUser(GetTokenForLoggedOnUser().Token, action);

        public static T RunAsLoggedOnUser<T>(Func<T> func) => RunAsUser(GetTokenForLoggedOnUser().Token, func);

        public static async Task RunAsLoggedOnUserAsync(Func<Task> func) => await RunAsUserAsync(GetTokenForLoggedOnUser().Token, func);

        public static async Task<T> RunAsLoggedOnUserAsync<T>(Func<Task<T>> func) => await RunAsUserAsync(GetTokenForLoggedOnUser().Token, func);


        // Impersonate a user given the user's token to run a specific action
        // Note: If we are already running in that user's context, we will not further impersonate the user
        public static void RunAsUser(IntPtr token, Action action)
        {
            using var requested = new WindowsIdentity(token);
            if (GetCurrentUser().User == requested.User)
            {
                action();
                return;
            }

            using var handle = new SafeAccessTokenHandle(token);
            WindowsIdentity.RunImpersonated(handle, action);
        }

        public static T RunAsUser<T>(IntPtr token, Func<T> func)
        {
            using var requested = new WindowsIdentity(token);
            if (GetCurrentUser().User == requested.User) return func();

            using var handle = new SafeAccessTokenHandle(token);
            return WindowsIdentity.RunImpersonated(handle, func);
        }

        public static async Task RunAsUserAsync(IntPtr token, Func<Task> func)
        {
            using var requested = new WindowsIdentity(token);
            if (GetCurrentUser().User == requested.User)
            {
                await func();
                return;
            }

            using var handle = new SafeAccessTokenHandle(token);
            await WindowsIdentity.RunImpersonatedAsync(handle, func);
        }

        public static async Task<T> RunAsUserAsync<T>(IntPtr token, Func<Task<T>> func)
        {
            using var requested = new WindowsIdentity(token);
            if (GetCurrentUser().User == requested.User) return await func();

            using var handle = new SafeAccessTokenHandle(token);
            return await WindowsIdentity.RunImpersonatedAsync(handle, func);
        }

        public static UserTokenInfo GetTokenForLoggedOnUser()
        {
            // TODO: what happens when no one is logged in or logged in over RDP?
            var sessionId = Kernel32.WTSGetActiveConsoleSessionId();
            if (sessionId is Advapi32.InvalidSessionId) throw new InvalidOperationException($"Cannot get session id");

            var token = IntPtr.Zero;
            try
            {
                token = GetTokenFromSession(sessionId, systemUser: true);
                return DuplicateToken(token);
            }
            finally
            {
                Kernel32.CloseHandle(token);
            }
        }

        public static UserTokenInfo DuplicateToken(IntPtr token)
        {
            var dupToken = IntPtr.Zero;
            var sa = new SECURITY_ATTRIBUTES();
            sa.Length = Marshal.SizeOf(sa);
            if (!Advapi32.DuplicateTokenEx(token, (uint)Advapi32.TokenAllAccess, ref sa, SECURITY_IMPERSONATION_LEVEL.SecurityImpersonation, TOKEN_TYPE.TokenPrimary, ref dupToken)) throw new InvalidOperationException($"Cannot duplicate token: {Marshal.GetLastWin32Error()}");
            return new UserTokenInfo(dupToken, sa);
        }

        // Useful for validating that we are not in session 0
        public static WindowsIdentity GetCurrentUser() => WindowsIdentity.GetCurrent();

        public static WindowsIdentity GetSessionZero()
        {
            var token = IntPtr.Zero;
            try
            {
                if (Process.GetCurrentProcess() is var process && process.SessionId is 0)
                {
                    token = GetTokenFromProcess(process);
                    var (dupToken, _) = DuplicateToken(token);

                    return new WindowsIdentity(dupToken);
                }

                throw new InvalidOperationException("Unable to retrieve the SessionZero id from Session >= 1");
            }
            finally
            {
                Kernel32.CloseHandle(token);
            }
        }

        public static void ThrowIfNotSessionZero()
        {
            if (Process.GetCurrentProcess() is not Process process || process.SessionId is not 0) throw new InvalidOperationException("Not in session zero");
        }

        public static IntPtr GetTokenFromSession(uint sessionId, bool systemUser)
        {
            if (systemUser)
            {
                const string ProcessName = "winlogon";
                var process = Process.GetProcessesByName(ProcessName)?.FirstOrDefault(p => (uint)p.SessionId == sessionId);
                if (process is null) throw new InvalidOperationException($"process '{ProcessName}' is not found");

                return GetTokenFromProcess(process);
            }
            else // logon user
            {
                return Kernel32.WTSQueryUserToken(sessionId, out var token) ? token :
                    throw new InvalidOperationException($"failed to query the user information of session {sessionId}, err = {Marshal.GetLastWin32Error()}");
            }
        }

        private static IntPtr GetTokenFromProcess(Process process)
        {
            var processHandle = IntPtr.Zero;
            var token = IntPtr.Zero;
            try
            {
                // obtain a handle to the requested
                processHandle = Kernel32.OpenProcess(Kernel32.GENERIC_ALL_ACCESS, false, process.Id);

                // obtain a token of the process
                return processHandle == IntPtr.Zero ?
                    throw new InvalidOperationException($"failed to open the process (pid = {process.Id}), err = {Marshal.GetLastWin32Error()}") :
                    Advapi32.OpenProcessToken(processHandle, TOKEN_ACCESS.TokenDuplicate, ref token) ? token :
                    throw new InvalidOperationException($"failed to get the process token (pid = {process.Id}), err = {Marshal.GetLastWin32Error()}");
            }
            finally
            {
                Kernel32.CloseHandle(processHandle);
            }
        }
    }
}
