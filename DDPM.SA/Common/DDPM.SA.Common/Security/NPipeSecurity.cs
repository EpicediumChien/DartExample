using DDPM.SA.Common.Settings;
using Dell.Client.Framework.Common;
using Dell.Client.Framework.Security;
using Dell.RPC.Transport;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
using static Dell.Client.Framework.Common.Platform;

namespace DDPM.SA.Common.Security
{
    public class NPipeSecurity
    {
        //private static Log _log;
        [DllImport("kernel32.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        internal static extern bool GetNamedPipeClientProcessId(IntPtr Pipe, out UInt32 ClientProcessId);

        private static bool _GetNamedPipeClientProcessId(IntPtr Pipe, out UInt32 ClientProcessId)
        {
            bool rst = GetNamedPipeClientProcessId(Pipe, out ClientProcessId);

            if (!rst)
            {
#if DEBUG
                Console.WriteLine("[NPipeSecurity] GetNamedPipeClientProcessId Failed");
#endif
            }

            return rst;
        }

        /// <summary>
        /// For buildin user please make your decision for PipeAccessRights.ReadWrite or PipeAccessRights.FullControl
        /// </summary>
        /// <param name="privilegeForBuildInUser"></param>
        /// <returns></returns>
        public static PipeSecurity CreatePipeSecurity(PipeAccessRights privilegeForBuildInUser)
        {
            var pipeSecurity = new TransportPipeSecurity();
            // by default, this pipe security object is meant for an elevated pipe
            pipeSecurity.IsElevated = true;
            // Disable inherited permissions
            // Note, the first argument says to protect these rules from inheritance and the second argument is to remove current inherited rules
            // Add default account rights
            pipeSecurity.SetAccessRuleProtection(true, false);
            //Build in user: PipeAccessRights.ReadWrite or PipeAccessRights.FullControl
            var accessRule = new PipeAccessRule(LocalAccounts.Groups.BuiltinUsersSid, privilegeForBuildInUser, AccessControlType.Allow);
            pipeSecurity.AddAccessRule(accessRule);
            // - Allow System group Full Control
            // - Allow Administrators group Full Control
            accessRule = new PipeAccessRule(LocalAccounts.Users.LocalSystemSid, PipeAccessRights.FullControl, AccessControlType.Allow);
            pipeSecurity.AddAccessRule(accessRule);
            // Allow Admin since they could just PSExec us to get to System so just make
            // easier for debugging reasons
            accessRule = new PipeAccessRule(LocalAccounts.Groups.BuiltinAdminsSid, PipeAccessRights.FullControl, AccessControlType.Allow);
            pipeSecurity.AddAccessRule(accessRule);
            // Denying access to connections coming over the network.
            // Connections made from within a Remote Desktop (RDP) session still work. This is the behavior we want.
            var securityId = new SecurityIdentifier(WellKnownSidType.NetworkSid, null);
            accessRule = new PipeAccessRule(securityId, PipeAccessRights.FullControl, AccessControlType.Deny);
            pipeSecurity.AddAccessRule(accessRule);
            // Deny access to connections for AnonymousSid accounts
            securityId = new SecurityIdentifier(WellKnownSidType.AnonymousSid, null);
            accessRule = new PipeAccessRule(securityId, PipeAccessRights.FullControl, AccessControlType.Deny);
            pipeSecurity.AddAccessRule(accessRule);
            return pipeSecurity;
        }

        /// <summary>
        /// System permissions are used, buildin user will be rejected
        /// </summary>
        /// <returns></returns>
        public static PipeSecurity CreatePipeSecurity_System()
        {
            var pipeSecurity = new TransportPipeSecurity();
            // by default, this pipe security object is meant for an elevated pipe
            pipeSecurity.IsElevated = true;
            // Disable inherited permissions
            // Note, the first argument says to protect these rules from inheritance and the second argument is to remove current inherited rules
            // Add default account rights
            pipeSecurity.SetAccessRuleProtection(true, false);
            //Build in user: PipeAccessRights.ReadWrite or PipeAccessRights.FullControl
            // - Allow System group Full Control
            // - Allow Administrators group Full Control
            var accessRule = new PipeAccessRule(LocalAccounts.Users.LocalSystemSid, PipeAccessRights.FullControl, AccessControlType.Allow);
            pipeSecurity.AddAccessRule(accessRule);
            // Allow Admin since they could just PSExec us to get to System so just make
            // easier for debugging reasons
            accessRule = new PipeAccessRule(LocalAccounts.Groups.BuiltinAdminsSid, PipeAccessRights.FullControl, AccessControlType.Allow);
            pipeSecurity.AddAccessRule(accessRule);
            // Denying access to connections coming over the network.
            // Connections made from within a Remote Desktop (RDP) session still work. This is the behavior we want.
            var securityId = new SecurityIdentifier(WellKnownSidType.NetworkSid, null);
            accessRule = new PipeAccessRule(securityId, PipeAccessRights.FullControl, AccessControlType.Deny);
            pipeSecurity.AddAccessRule(accessRule);
            // Deny access to connections for AnonymousSid accounts
            securityId = new SecurityIdentifier(WellKnownSidType.AnonymousSid, null);
            accessRule = new PipeAccessRule(securityId, PipeAccessRights.FullControl, AccessControlType.Deny);
            // Deny access to connections for AnonymousSid accounts
            securityId = new SecurityIdentifier(WellKnownSidType.BuiltinUsersSid, null);
            accessRule = new PipeAccessRule(securityId, PipeAccessRights.FullControl, AccessControlType.Deny);
            pipeSecurity.AddAccessRule(accessRule);
            return pipeSecurity;
        }

        public static bool NamedPipeClientSecurity(NamedPipeServerStream pipeServer, out string info, string thumbPrint = null)
        {
            info = "success";
            IntPtr hPipe = default;
            uint pid = default;
            try
            {
                hPipe = pipeServer.SafePipeHandle.DangerousGetHandle();

                if (!_GetNamedPipeClientProcessId(hPipe, out pid))
                {
                    info = "[GetNamedPipeClientProcessId] failed";
                    return false;
                }
            }
            catch (Exception ex) 
            {
                info = $"[GetNamedPipeClientProcessId] exception, message :{ex.Message}";
                return false;
            }
            //this finally action cause NKVM/FW update namedpipe fail, remove it [Dean 1212]
            //finally
            //{
            //    pipeServer.SafePipeHandle.Close();
            //}

#if DEBUG
            Console.WriteLine("pid: " + pid);
#endif
            Process process = Process.GetProcessById((int)pid);
            string filePath = process.MainModule.FileName;
            //Console.WriteLine("File path: " + filePath);

            //check file path security
            if (!DDPMFileSecurity.IsFilePathValid(filePath, out info))
            {
                info = $"[NamedPipeClientSecurity][IsFilePathValid] {info}";
                return false;
            }

            if(thumbPrint == null) // check with inbox thumbPrint
            {
                if (!DDPMFileSecurity.VerifyFileCertWithInboxThumbprint(filePath, out info)) // VerifyFileCertWithoutThumbprint(filePath, out info))
                {
                    return false;
                }
            }
            else
            {
                if (!DDPMFileSecurity.VerifyFileCertWithThumbprint(filePath, thumbPrint, out info))
                {
                    return false;
                }
            }

            return true;
        }
    }
}