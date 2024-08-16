using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace DDPM.SA.Common.Display
{
    public class HotkeyJobQueue
    {
        private readonly ConcurrentQueue<HotkeyJobInfo> _invokeQueue = new();
        private bool _disposed = false;
        private readonly object _lockObject = new();

        public HotkeyJobQueue()
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

        public void Enqueue(HotkeyJobInfo jobInfo)
        {
            _invokeQueue.Enqueue(jobInfo);
        }

        public void Dispose()
        {
            lock (_lockObject)
            {
                _disposed = true;
                _invokeQueue.Clear();
            }
        }
    }
}