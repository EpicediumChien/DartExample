using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using VcpCore.Common;

namespace DDPM.SA.Common.Display
{
    public class HotkeyJobInfo
    {
        private Action<MonitorInfo, Object[]> _action;
        private Object[] _param;
        private MonitorInfo _monitorInfo;

        public HotkeyJobInfo(MonitorInfo monitor,Object[] param, Action<MonitorInfo, Object[]> action)
        {
            _action = action;
            _param = param;
            _monitorInfo = monitor;
        }


        public void Invoke()
        {
            _action(_monitorInfo,_param);
        }

    }
}
