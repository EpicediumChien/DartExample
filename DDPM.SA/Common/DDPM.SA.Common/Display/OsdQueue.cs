using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using VcpCore.Common;

namespace DDPM.SA.Common.Display
{

    public class OsdInfo
    {
        private Action<Object[]> _action;
        private Object[] _param;
        private System.Timers.Timer _timer = null;
        private readonly int _delayMilliseconds;

        public OsdInfo(int delayMilliseconds, Object[] param, Action<Object[]> action)
        {
            _action = action;
            _param = param;
            _delayMilliseconds = delayMilliseconds;
            _timer = new System.Timers.Timer(delayMilliseconds);
            _timer.AutoReset = false;
            _timer.Elapsed += TimerElapsed;
        }

        private void TimerElapsed(object sender, ElapsedEventArgs e)
        {
            if (_action != null)
                _action(_param);
            if (_timer != null)
            {
                _timer.Stop();
                _timer.Elapsed -= TimerElapsed;
                _timer.Dispose();
                _timer = null;
            }
        }

        public void Invoke()
        {
            if (_timer != null)
            {
                _timer.Stop();
                _timer.Start();
            }
        }
    }
    public class OsdQueue
    {
        private readonly ConcurrentQueue<OsdInfo> _invokeQueue = new();
        private bool _disposed = false;
        private readonly object _lockObject = new();
        public OsdQueue()
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
                            Task.Delay(100).Wait();
                        }
                        ;
                    }
                    catch (Exception ex)
                    {
                        //_logger.Error(ex, @"Unhandled exception in invoked method");
                    }
                }
            });

        }
        public void Enqueue(OsdInfo osdInfo)
        {
             _invokeQueue.Enqueue(osdInfo);         
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
    }
}
