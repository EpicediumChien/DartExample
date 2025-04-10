using System;
using System.Threading;
using System.Timers;
using VcpCore.Common;

namespace DDPM.SA.Common.Display
{
    public class JobInfo
    {
        private Action<MonitorInfo, Object[]> _action;
        private Object[] _param;
        private MonitorInfo _monitorInfo;
        private System.Timers.Timer _timer = null;
        private readonly int _delayMilliseconds;

        public JobInfo(int delayMilliseconds, MonitorInfo monitor, Object[] param, Action<MonitorInfo, Object[]> action)
        {
            _action = action;
            _param = param;
            _monitorInfo = monitor;
            _delayMilliseconds = delayMilliseconds;
            _timer = new System.Timers.Timer(delayMilliseconds);
            _timer.AutoReset = false;
            _timer.Elapsed += TimerElapsed;
        }

        private void TimerElapsed(object sender, ElapsedEventArgs e)
        {
            if (_action != null)
                _action(_monitorInfo, _param);
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
            //_action(_monitorInfo, _param);
        }
    }
}