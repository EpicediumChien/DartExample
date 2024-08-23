using Dell.Client.Framework.Security;
using Dell.RPC.Transport;
using System;
using System.Diagnostics;
using System.IO.Pipes;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Security.Principal;

namespace DDPM.SA.Common.Security
{
    public class NPipeSecurity
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern bool GetNamedPipeClientProcessId(IntPtr Pipe, out UInt32 ClientProcessId);
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
        public static bool NamedPipeClientSecurity(NamedPipeServerStream pipeServer)
        {
            IntPtr hPipe = pipeServer.SafePipeHandle.DangerousGetHandle();
            if (GetNamedPipeClientProcessId(hPipe, out uint pid))
            {
                Console.WriteLine("pid: " + pid);
                Process process = Process.GetProcessById((int)pid);
                string filePath = process.MainModule.FileName;
                Console.WriteLine("File path: " + filePath);
                //check file path security
            }
            return true; // temporarily
            //return false;
        }
    }
}