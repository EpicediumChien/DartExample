using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace DDPM.UI.Common
{
    public class WindowsServiceMonitor
    {
        #region Private members
        private readonly DispatcherTimer? _timer;
        private readonly string _serviceName;
        private ServiceControllerStatus? _lastStatus = null;
        private const double _defaultIntervalSeconds = 15.000;
        #endregion Private members

        #region Events
        public event Action<ServiceControllerStatus> ServiceStatusChanged;
        public event Action<Exception> MonitorError;
        #endregion Events

        #region ctor
        public WindowsServiceMonitor(string serviceName, TimeSpan? interval=null)
        {
            _serviceName = serviceName;
            _timer = new DispatcherTimer
            {
                Interval = interval ?? TimeSpan.FromSeconds(_defaultIntervalSeconds)
            };
            _timer.Tick += _timer_Tick;
        }

        #endregion

        #region Timer
        private void _timer_Tick(object? sender, EventArgs e)
        {
            try
            {
                using var sc = new ServiceController(_serviceName);
                var currentStatus = sc.Status;
                if (_lastStatus != currentStatus)
                {
                    _lastStatus = currentStatus;
                    ServiceStatusChanged?.Invoke(currentStatus);
                }
            }
            catch (Exception ex)
            {
                MonitorError?.Invoke(ex);
            }
        }
        #endregion Timer

        #region Methods
        public void Start()
        {
            _timer.Start();
        }

        public void Stop()
        {
            _timer.Stop();
        }
        #endregion Methods
    }
}
