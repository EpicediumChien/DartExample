#define IL_Ready
namespace DDPM.SA.Plugins.User.FWUpdate
{
    using DDPM.SA.Common.Security;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO.Pipes;
    using System.Security.AccessControl;
    using System.Security.Principal;
    using System.Text;
    using VcpCore.Common;
    using Windows.Foundation;

    public class NamedPipeStreamServer : NamedPipeStreamBase
    {
        private List<NamedPipeStreamConnection> _Connections;

        public event EventHandler? ClientConnectedEvent;

        public event EventHandler? ClientDisconnectedEvent;

        public bool IsNamedPipeServerIsNoSafe = false;
        private string thumbPrint;
        private bool skipSHA;
        private Logs _Log;

        public NamedPipeStreamServer(string pipeName, string thumbPrint, bool skipSHA, Logs logs) : base(pipeName)
        {
            this._Log = logs;
            PipeSecurity pipeSecurity = NPipeSecurity.CreatePipeSecurity_System();
            this._Connections = new List<NamedPipeStreamConnection>();
            NamedPipeServerStream state = NamedPipeServerStreamAcl.Create(base.PipeName, PipeDirection.InOut, NamedPipeServerStream.MaxAllowedServerInstances, PipeTransmissionMode.Message, PipeOptions.Asynchronous, 0, 0, pipeSecurity);
            _Log.DebugMsg_1("NamedPipeStreamServer PrintPipeAcl state go");
            PrintPipeAcl(state);
            _Log.DebugMsg_1("NamedPipeStreamServer PrintPipeAcl state done");
            state.BeginWaitForConnection(new AsyncCallback(this.ClientConnected), state);
            this.thumbPrint = thumbPrint;
            this.skipSHA = skipSHA;
        }

        private void ClientConnected(IAsyncResult result)
        {
            PipeSecurity pipeSecurity = NPipeSecurity.CreatePipeSecurity_System();
            NamedPipeServerStream? asyncState = result.AsyncState as NamedPipeServerStream;
            if (asyncState != null)
            {
                _Log.DebugMsg_1("ClientConnected PrintPipeAcl asyncState go");
                PrintPipeAcl(asyncState);
                _Log.DebugMsg_1("ClientConnected PrintPipeAcl asyncState done");
                asyncState.EndWaitForConnection(result);
                if (asyncState.IsConnected)
                {
                    string info;
                    if (!NPipeSecurity.NamedPipeClientSecurity(asyncState, out info, thumbPrint))
                    {

                        _Log.DebugMsg_1($"ClientConnected [NamedPipeStreamServer] NamedPipeClientSecurity failed ({info})");
                        IsNamedPipeServerIsNoSafe = true;
                        if (!skipSHA)
                        {
                            asyncState.Disconnect();
                            return;
                        }
                    }

                    NamedPipeStreamConnection item = default;

                    try
                    {
                        item = new NamedPipeStreamConnection(asyncState, base.PipeName);
                        item.MessageReceived += new MessageEventHandler(this.Connection_MessageReceived);
                        item.DisconnectedEvent += Connection_DisconnectedEvent;
                    }
                    catch (Exception ex)
                    {
                        _Log.DebugMsg_1($"ClientConnected [NamedPipeStreamServer] NamedPipeClientSecurity failed ({ex.Message})");
                        return;
                    }

                    lock (this._Connections)
                    {
                        this._Connections.Add(item);
                        ClientConnectedEvent?.Invoke(this, new EventArgs());
                    }
                }
                NamedPipeServerStream state = NamedPipeServerStreamAcl.Create(base.PipeName, PipeDirection.InOut, NamedPipeServerStream.MaxAllowedServerInstances, PipeTransmissionMode.Message, PipeOptions.Asynchronous, 0, 0, pipeSecurity);
                _Log.DebugMsg_1("ClientConnected PrintPipeAcl state go");
                PrintPipeAcl(state);
                _Log.DebugMsg_1("ClientConnected PrintPipeAcl state done");
                state.BeginWaitForConnection(new AsyncCallback(this.ClientConnected), state);
                
            }
        }
        private void PrintPipeAcl(NamedPipeServerStream pipeServer)
        {
            PipeSecurity pipeSecurity = pipeServer.GetAccessControl();
            AuthorizationRuleCollection acl = pipeSecurity.GetAccessRules(true, true, typeof(NTAccount));
            _Log.DebugMsg_1("[PrintPipeAcl]Access Control List for the pipe:");
            foreach (AuthorizationRule rule in acl)
            {
                _Log.DebugMsg_1($"[PrintPipeAcl]rule.IdentityReference.Value : {rule.IdentityReference.Value}");
                PipeAccessRule pipeRule = rule as PipeAccessRule;
                if (pipeRule != null)
                {
                    _Log.DebugMsg_1($"[PrintPipeAcl]Access Rights: {pipeRule.PipeAccessRights}");
                    _Log.DebugMsg_1($"[PrintPipeAcl]Access Control Type: {pipeRule.AccessControlType}");
                }
            }
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