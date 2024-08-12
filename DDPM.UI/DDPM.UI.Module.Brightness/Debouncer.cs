using System;
using System.Timers;

namespace DDPM.UI.Module.Brightness
{
    public class Debouncer
    {
        private readonly System.Timers.Timer _timer;
        private readonly int _delayMilliseconds;
        private Action<double> _action;
        private double value;

        public Debouncer(int delayMilliseconds, Action<double> action)
        {
            _delayMilliseconds = delayMilliseconds;
            _action = action;
            _timer = new System.Timers.Timer(delayMilliseconds);
            _timer.AutoReset = false;
            _timer.Elapsed += TimerElapsed;
        }

        private void TimerElapsed(object sender, ElapsedEventArgs e)
        {
            _action(value);
        }

        public void Debounce(double d)
        {
            value = d;
            _timer.Stop();
            _timer.Start();
        }
    }
}
