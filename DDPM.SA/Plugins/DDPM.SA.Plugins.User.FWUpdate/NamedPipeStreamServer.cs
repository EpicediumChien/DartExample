namespace DDPM.SA.Plugins.User.FWUpdate
{
    using Dell.Client.Framework.Security;
    using Dell.RPC.Transport;
    using System;
    using System.Collections.Generic;
    using System.IO.Pipes;
    using System.Security.AccessControl;
    using System.Security.Principal;
    using System.Text;

    public class NamedPipeStreamServer : NamedPipeStreamBase
    {
        private List<NamedPipeStreamConnection> _Connections;

        public event EventHandler? ClientConnectedEvent;
        public event EventHandler? ClientDisconnectedEvent;

        public NamedPipeStreamServer(string pipeName) : base(pipeName)
        {
            PipeSecurity pipeSecurity = CreatePipeSecurity();
            this._Connections = new List<NamedPipeStreamConnection>();
            NamedPipeServerStream state = NamedPipeServerStreamAcl.Create(base.PipeName, PipeDirection.InOut, NamedPipeServerStream.MaxAllowedServerInstances, PipeTransmissionMode.Message, PipeOptions.Asynchronous, 0, 0, pipeSecurity);
            state.BeginWaitForConnection(new AsyncCallback(this.ClientConnected), state);
        }

        private void ClientConnected(IAsyncResult result)
        {
            PipeSecurity pipeSecurity = CreatePipeSecurity();
            NamedPipeServerStream? asyncState = result.AsyncState as NamedPipeServerStream;
            if (asyncState != null)
            {
                asyncState.EndWaitForConnection(result);
                if (asyncState.IsConnected)
                {
                    NamedPipeStreamConnection item = new NamedPipeStreamConnection(asyncState, base.PipeName);
                    item.MessageReceived += new MessageEventHandler(this.Connection_MessageReceived);
                    item.DisconnectedEvent += Connection_DisconnectedEvent;
                    lock (this._Connections)
                    {
                        this._Connections.Add(item);
                        ClientConnectedEvent?.Invoke(this, new EventArgs());
                    }
                }
                NamedPipeServerStream state = NamedPipeServerStreamAcl.Create(base.PipeName, PipeDirection.InOut, NamedPipeServerStream.MaxAllowedServerInstances, PipeTransmissionMode.Message, PipeOptions.Asynchronous, 0, 0, pipeSecurity);
                state.BeginWaitForConnection(new AsyncCallback(this.ClientConnected), state);
            }
        }
        private PipeSecurity CreatePipeSecurity()
        {
            var pipeSecurity = new TransportPipeSecurity();
            // by default, this pipe security object is meant for an elevated pipe
            pipeSecurity.IsElevated = true;
            // Disable inherited permissions             
            // Note, the first argument says to protect these rules from inheritance and the second argument is to remove current inherited rules
            pipeSecurity.SetAccessRuleProtection(true, false);
            var accessRule = new PipeAccessRule(LocalAccounts.Groups.BuiltinUsersSid, PipeAccessRights.FullControl, AccessControlType.Allow);
            pipeSecurity.AddAccessRule(accessRule);
            // Add default account rights             
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
        private void Connection_DisconnectedEvent(object? sender, EventArgs e)
        {
            ClientDisconnectedEvent?.Invoke(this, e);
        }

        private void Connection_MessageReceived(object sender, MessageEventArgs args)
        {
            this.OnMessageReceived(args);
        }

        public override void Disconnect()
        {
            lock (this._Connections)
            {
                foreach (NamedPipeStreamConnection connection in this._Connections)
                {
                    try
                    {
                        connection.Disconnect();
                    }
                    catch
                    {
                    }
                    this._Connections.Clear();
                }
            }
        }

        ~NamedPipeStreamServer()
        {
            this.Dispose(false);
        }

        public override void SendMessage(string message)
        {
            SendMessage(Encoding.UTF8.GetBytes(message));
        }
        public override void SendMessage(byte[] message)
        {
            List<NamedPipeStreamConnection>? list = null;
            bool flag = false;
            lock (this._Connections)
            {
                foreach (NamedPipeStreamConnection connection in this._Connections)
                {
                    try
                    {
                        flag = !connection.IsConnected;
                        if (!flag)
                        {
                            connection.SendMessage(message);
                        }
                    }
                    catch
                    {
                        flag = true;
                    }
                    if (flag)
                    {
                        connection.Disconnect();
                        if (list == null)
                        {
                            list = new List<NamedPipeStreamConnection>();
                        }
                        list.Add(connection);
                    }
                }
                if (list != null)
                {
                    foreach (NamedPipeStreamConnection connection in list)
                    {
                        this._Connections.Remove(connection);
                    }
                }
            }
        }
    }
}
