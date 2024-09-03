using System.Timers;

namespace DDPM.UI.Module.Brightness
{
    public class Debouncer
    {
        private readonly System.Timers.Timer _timer;
        private readonly int _delayMilliseconds;
        private Action<object> _action;
        private object value;

        public Debouncer(int delayMilliseconds, Action<object> action)
        {
            _delayMilliseconds = delayMilliseconds;
            _action = action;
            _timer = new System.Timers.Timer(delayMilliseconds);
            _timer.AutoReset = false;
            _timer.Elapsed += TimerElapsed;
        }

        private void TimerElapsed(object sender, ElapsedEventArgs e)
        {
            if (_action != null)
                _action(value);
        }

        public void Debounce(object d)
        {
            value = d;
            _timer.Stop();
            _timer.Start();
        }
    }
}