using Dell.Client.Framework.Security;
using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using Dell.RPC.Transport.Interfaces;
using Dell.RPC.Transport.Security;
using Dell.RPC.Transport;

namespace DDPM.SA.Common.Security
{
    public class NPipeSecurity
    {
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
    }
}
