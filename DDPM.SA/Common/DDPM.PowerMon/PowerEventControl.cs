using DDPM.SA.Common.Display;
using Dell.Client.Framework.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DDPM.PowerMon
{
    public class PowerEventControl
    {
        private static PowerMonitor _pwr_Mon = null;
        private static ILog _log = null;
        public event EventHandler MonitorTurnedOn = null;

        public event EventHandler<KeyPressedEventArgs> HotkeyPressed = null;

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

        private void HotkeyEvent_Pressed(object sender, KeyPressedEventArgs e)
        {
            writelog("GOT Hotkey Pressed EVENT");
            Task.Run(() =>
                HotkeyPressed?.Invoke(this, e)
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

        public void RegisterHotKey(List<HotkeyInfo> hotkeyInfos)
        {
            if (_pwr_Mon != null)
                _pwr_Mon.RegisterHotKey(hotkeyInfos);
        }

        public void UnRegisterHotKey(ushort id)
        {

            if (_pwr_Mon != null)
                _pwr_Mon.UnRegisterHotKey(id);
        }
        public void UnRegisterAllHotKey()
        {
            if (_pwr_Mon != null)
                _pwr_Mon.UnRegisterAllHotKey();

        }

        //For monitor event only
        public void Enable_Event()
        {
            if (_pwr_Mon == null)
            {
                _pwr_Mon = new PowerMonitor(_log);
            }
            _pwr_Mon.MonitorTurnedOn += MonitorEvent_On;            
            _pwr_Mon.ShowDialog();            
        }

        //For monitor event only
        public void Close_Event()
        {
            if (_pwr_Mon != null)
            {
                _pwr_Mon.MonitorTurnedOn -= MonitorEvent_On;
                _pwr_Mon.HotkeyPressed -= HotkeyEvent_Pressed;
                _pwr_Mon.CloseByCaller();
                _pwr_Mon = null;
            }
        }

        //For hotkey event
        public void Enable_HotkeyHook()
        {
            if (_pwr_Mon == null)
            {
                _pwr_Mon = new PowerMonitor(_log);
            }

            if (!_pwr_Mon.isWindowLoaded)
                _pwr_Mon.ShowDialog();

            int count = 0;
            while (!_pwr_Mon.isWindowLoaded && count <= 10000)//wait 10 sec
            {
                Thread.Sleep(1000);
                count += 1000;
            }
            if (count >= 10000)
            {
                writelog("Enable Hotkey got timeout result.");
                return;
            }
            _pwr_Mon.HotkeyPressed += HotkeyEvent_Pressed;
            _pwr_Mon.Enable_HotkeyHook();
        }
    }
}
