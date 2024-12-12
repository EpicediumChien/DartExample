using System;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace DDPM.SA.Common.Display
{
    public class JobQueue
    {
        private readonly ConcurrentQueue<JobInfo> _invokeQueue = new();
        private bool _disposed = false;
        private readonly object _lockObject = new();

        public JobQueue()
        {
            _ = Task.Run(() =>
            {
                while (!_disposed)
                {
                    if (_disposed) return;
                    try
                    {
                        if (_invokeQueue.TryDequeue(out var job))
                        {
                            job.Invoke();
                        }
                        else
                        {
                            Thread.Sleep(100);
                        };
                    }
                    catch (Exception ex)
                    {
                        //_logger.Error(ex, @"Unhandled exception in invoked method");
                    }
                }
            });
        }

        public void Enqueue(JobInfo jobInfo)
        {
            _invokeQueue.Enqueue(jobInfo);
        }

        public void Clear()
        {
            _invokeQueue.Clear();
        }

        public void DisposeJobQueue()
        {
            lock (_lockObject)
            {
                _disposed = true;
                _invokeQueue.Clear();
            }
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        private static extern bool SystemParametersInfo(uint uAction, uint uParam, ref bool lpvParam, int fWinIni);

        private static bool _SystemParametersInfo(uint uAction, uint uParam, ref bool lpvParam, int fWinIni)
        {
            return SystemParametersInfo(uAction, uParam, ref lpvParam, fWinIni);
        }

        public const uint SPI_GETSCREENSAVEACTIVE = 0x10;   //check if screen saver is actived
        public const uint SPI_GETSCREENSAVERRUNNING = 0x72; //check if screen saver is running now

        public static bool GetScreensaverCurrentStatus(uint param)
        {
            bool lparam = false;
            if (!_SystemParametersInfo(param, 0, ref lparam, 0))
            {
                return false;
            }
            return lparam;
        }
    }
}