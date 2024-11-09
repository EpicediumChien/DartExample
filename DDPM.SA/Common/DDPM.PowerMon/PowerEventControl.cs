using Dell.Client.Framework.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDPM.PowerMon
{
    public class PowerEventControl
    {
        private static PowerMonitor _pwr_Mon = null;
        private static ILog _log = null;
        public event EventHandler MonitorTurnedOn = null;

        public PowerEventControl(ILog Log)
        {
            _log = Log;            
        }

        private void MonitorEvent_On(object sender, EventArgs e)
        {
            writelog("GOT MONITOR ON EVENT");
            Task.Run(() =>
                MonitorTurnedOn?.Invoke(this, EventArgs.Empty)
            );
        }

        private enum log_type
        {
            info = 0,
            error
        }

        private void writelog(string text, log_type log_type = log_type.info)
        {
            if (string.IsNullOrEmpty(text))
                text = "";

            text = "[PowerEventControl] " + text;
            Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss.fff") + " " + text);

            if (_log != null)
            {
                if (log_type == log_type.info)
                    _log.Info(text);
                else
                    _log.Error(text);
            }
        }

        public void Enable_Event()
        {
            if (_pwr_Mon == null)
            {
                _pwr_Mon = new PowerMonitor(_log);
                _pwr_Mon.MonitorTurnedOn += MonitorEvent_On;
                _pwr_Mon.ShowDialog();
            }
        }

        public void Close_Event()
        {
            if (_pwr_Mon != null)
            {
                _pwr_Mon.MonitorTurnedOn -= MonitorEvent_On;
                _pwr_Mon.CloseByCaller();
                _pwr_Mon = null;
            }
        }
    }
}
