using Dell.Client.Framework.Security;
using System.Diagnostics;
using System.IO.Pipes;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
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
        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern bool GetNamedPipeServerProcessId(IntPtr Pipe, out UInt32 ClientProcessId);
        private NamedPipeClientStream pipeClient;
        private CancellationTokenSource cancellationTokenSource;
        public delegate void VCPEventHandler(object sender, EventArgsjson eventArgsjson);
        public event VCPEventHandler DDPMEvent;
        public NamedPipeClient(string NamedpipeName)
        {
            //pipeClient = new NamedPipeClientStream(".", "VCPNamedPipe", PipeDirection.InOut, PipeOptions.Asynchronous | PipeOptions.WriteThrough);
            pipeClient = new NamedPipeClientStream(".", NamedpipeName, PipeDirection.InOut, PipeOptions.Asynchronous | PipeOptions.WriteThrough);
            //cancellationTokenSource = new CancellationTokenSource();
        }
        public async Task ConnectAsync(int timeout)
        {
            if (!pipeClient.IsConnected)
            {
                await pipeClient.ConnectAsync(timeout/*cancellationTokenSource.Token*/).ConfigureAwait(false);
#if RELEASE
                if (!NamedPipeServerSecurity(pipeClient))
                {
                    Disconnect();
                }
#endif
            }
            //Task.Run(() => ReadAsync(), cancellationTokenSource.Token);
        }
        public async Task NKVMtoDDPM(string message) //NKVM->DDPM json file
        { 
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
            byte[] buffer = new byte[2048];
            int bytesRead = default;

            try
            {
                bytesRead = await pipeClient.ReadAsync(buffer, 0, buffer.Length/*, cancellationTokenSource.Token*/).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Console.WriteLine("[DDPMtoNKVM] exception, message: " + ex.Message);
                return string.Empty;
            }

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
        /*public static X509Certificate2 LoadCertificate(string filePath)
        {
            byte[] certBytes = default;

            try
            {
               certBytes = File.ReadAllBytes(filePath);
            }
            catch 
            { 
                return default;
            }

            return new X509Certificate2(certBytes);
        }*/

        private bool NamedPipeServerSecurity(NamedPipeClientStream pipeServer)
        {
            string filePath = string.Empty;
            string info = string.Empty;
            if (!GetNamedPipeServerProcessId(pipeServer.SafePipeHandle.DangerousGetHandle(), out uint pid))
            {
                Console.WriteLine("Get server process id over pipeline failed");
                return false;
            }
            Console.WriteLine("pid: " + pid);
            Process process = Process.GetProcessById((int)pid);
            filePath = process.MainModule.FileName;
            Console.WriteLine("File path: " + filePath);
            //check file path security
            if(!IsFilePathValid(filePath, out info))
            {
                Console.WriteLine($"File({filePath}) path check failed ({info}).");
                return false;
            }

            //Need to check dll/exe thumbprint
            X509Certificate2 cert = new X509Certificate2(filePath);//LoadCertificate(filePath);
            if (cert == null)
            {
                Console.WriteLine("Can't retrieve cert from file.");
                return false;
            }


            try
            {
                X509Chain x509Chain = new X509Chain();
                x509Chain.ChainPolicy.RevocationFlag = X509RevocationFlag.EntireChain;
                x509Chain.ChainPolicy.RevocationMode = X509RevocationMode.Online;
                x509Chain.ChainPolicy.UrlRetrievalTimeout = new System.TimeSpan(0, 1, 0);
                x509Chain.ChainPolicy.VerificationFlags = X509VerificationFlags.NoFlag;
                if (!x509Chain.Build(cert))
                {
                    Console.WriteLine("[NamedPipeServerSecurity] Certificate is invaild!");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[NamedPipeServerSecurity] error: {ex.Message}");
                return false;
            }

            //compare thumbprint
            //source array DDPM.SA.Obfuscation.ThumbprintHash.certificateHash
            //Target cert.Thumbprint
            try
            {
                bool contains = DDPM.SA.Obfuscation.ThumbprintHash.certificateHash.Any(arr => arr.SequenceEqual(ConvertThumbprintToByteArray(cert.Thumbprint)));
                if (!contains)
                {
                    Console.WriteLine($"No matched cert. thumbprint in file is {cert.Thumbprint}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
            return true;
        }

        public static bool IsFilePathValid(string filePath, out string info)
        {
            info = "Valid";
            //check return code with Enum PathCheckErrorCodes
            PathCheckErrorCodes result = PathHelper.ValidateFilePath(filePath);
            if (result != PathCheckErrorCodes.SUCCESS)
            {
                info = $"IsFilePathValid: {nameof(result)}";
                return false;
            }
            return true;
        }

        private static byte[] ConvertThumbprintToByteArray(string thumbprint)
        {
            return Enumerable.Range(0, thumbprint.Length)
                             .Where(x => x % 2 == 0)
                             .Select(x => Convert.ToByte(thumbprint.Substring(x, 2), 16))
                             .ToArray();
        }
    }
}
