namespace DDPM.SA.Plugins.User.FWUpdate
{
    using DDPM.SA.Common.Security;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO.Pipes;
    using System.Text;

    public class NamedPipeStreamServer : NamedPipeStreamBase
    {
        private List<NamedPipeStreamConnection> _Connections;

        public event EventHandler? ClientConnectedEvent;

        public event EventHandler? ClientDisconnectedEvent;

        public bool IsNamedPipeServerIsNoSafe = false;

        public NamedPipeStreamServer(string pipeName) : base(pipeName)
        {
            PipeSecurity pipeSecurity = NPipeSecurity.CreatePipeSecurity(PipeAccessRights.FullControl);
            this._Connections = new List<NamedPipeStreamConnection>();
            NamedPipeServerStream state = NamedPipeServerStreamAcl.Create(base.PipeName, PipeDirection.InOut, NamedPipeServerStream.MaxAllowedServerInstances, PipeTransmissionMode.Message, PipeOptions.Asynchronous, 0, 0, pipeSecurity);
            state.BeginWaitForConnection(new AsyncCallback(this.ClientConnected), state);
        }

        private void ClientConnected(IAsyncResult result)
        {
            PipeSecurity pipeSecurity = NPipeSecurity.CreatePipeSecurity(PipeAccessRights.FullControl);
            NamedPipeServerStream? asyncState = result.AsyncState as NamedPipeServerStream;
            if (asyncState != null)
            {
                asyncState.EndWaitForConnection(result);
                if (asyncState.IsConnected)
                {
                    string info;
                    if (!NPipeSecurity.NamedPipeClientSecurity(asyncState, out info))
                    {
                        Trace.WriteLine($"[NamedPipeStreamServer] NamedPipeClientSecurity failed ({info})");
                        IsNamedPipeServerIsNoSafe = true;
                        asyncState.Disconnect();
                        return;
                    }
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