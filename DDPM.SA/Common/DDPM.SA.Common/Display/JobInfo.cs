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
        private readonly System.Timers.Timer _timer;
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
        }

        public void Invoke()
        {
            _timer.Stop();
            _timer.Start();
            //_action(_monitorInfo, _param);
        }
    }
}