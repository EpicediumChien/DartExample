using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VcpCore.Common;

namespace DDPM.SA.Plugins.User.EasyArrange
{
    /// <summary>
    /// Implement the Forms.Screen and attached MonitorInfos management
    /// </summary>
    public class EAScreen
    {
        private readonly Screen _screen;
        private List<MonitorInfo>? _monitors = null;

        public EAScreen(Screen scr, List<MonitorInfo>? attachedMonitors)
        {
            _screen = scr;
            _monitors = attachedMonitors;
        }

        public int AttachedMonitorCount
        {
            get
            {
                if (_monitors == null)
                    return 0;
                return _monitors.Count;
            }
        }

        /// <summary>
        /// Get the main (first) attached MonitorInfo
        /// </summary>
        public MonitorInfo? AttachedMonitor
        {
            get
            {
                if (AttachedMonitorCount > 0)
                    return _monitors[0];
                return null;
            }
        }
    }
}
