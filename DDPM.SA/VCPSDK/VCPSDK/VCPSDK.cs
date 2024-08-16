using System.IO.Pipes;
using System.Text;

namespace VCPSDK
{
    public class EventArgsjson : EventArgs
    {
        public EventArgsjson(string jsonstring)
        {
            jsonString = jsonstring;
        }
        public string jsonString { get; set; }
    }
    public class NamedPipeClient
    {
        private NamedPipeClientStream pipeClient;
        private CancellationTokenSource cancellationTokenSource;
        public delegate void VCPEventHandler(object sender, EventArgsjson eventArgsjson);
        public event VCPEventHandler DDPMEvent;
        public NamedPipeClient(string NamedpipeName)
        {
#if DEBUG
            pipeClient = new NamedPipeClientStream(".", "VCPNamedPipe", PipeDirection.InOut, PipeOptions.Asynchronous | PipeOptions.WriteThrough);
#else
            pipeClient = new NamedPipeClientStream(".", NamedpipeName, PipeDirection.InOut, PipeOptions.Asynchronous | PipeOptions.WriteThrough);
#endif
            //cancellationTokenSource = new CancellationTokenSource();
        }
        public async Task ConnectAsync(int timeout)
        {
            if (!pipeClient.IsConnected)
            {
                await pipeClient.ConnectAsync(timeout/*cancellationTokenSource.Token*/).ConfigureAwait(false);
            }
            //Task.Run(() => ReadAsync(), cancellationTokenSource.Token);
        }
        public async Task NKVMtoDDPM(string message) //NKVM->DDPM json file
        {
            //await ConnectAsync();

            try
            {
                Console.WriteLine("NKVMCommand : " + message);
                byte[] buffer = Encoding.UTF8.GetBytes(message);
                await pipeClient.WriteAsync(buffer, 0, buffer.Length/*, cancellationTokenSource.Token*/).ConfigureAwait(false);
                await pipeClient.FlushAsync().ConfigureAwait(false);
                pipeClient.WaitForPipeDrain();
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        public async Task<string> DDPMtoNKVM() //DDPM->NKVM json file
        {
            //await ConnectAsync();

            byte[] buffer = new byte[2048];
            int bytesRead = await pipeClient.ReadAsync(buffer, 0, buffer.Length/*, cancellationTokenSource.Token*/).ConfigureAwait(false);
            //VCPResponse(Encoding.UTF8.GetString(buffer, 0, bytesRead));
            return Encoding.UTF8.GetString(buffer, 0, bytesRead);
        }
        public async void Disconnect()
        {
            //if (pipeClient.IsConnected)
            //{
            //    pipeClient.Close();
            //}

            await NKVMtoDDPM("Disconnect").ConfigureAwait(false);
            pipeClient.Close();
            pipeClient.Dispose();
        }
        public bool IsConnected()
        {
            return pipeClient.IsConnected;
        }
        public void VCPEvent(string response)
        {
            DDPMEvent?.Invoke(this, new EventArgsjson(response));
        }
    }
}
