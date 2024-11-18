using DDPM.SA.Common;
using Dell.Client.Framework.Common;
using Microsoft.Toolkit.Uwp.Notifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDPM.SA.Plugins.User.DeviceManager
{
    public class DTPHelper
    {
        private static IDeviceManagerSA devManagerSA = null;
        private static IDTPProxyPlugin dtpProxy = null;

        public enum log_type
        {
            info = 0,
            error
        }

        public void UpdateDDPMPluginInstances(IDTPProxyPlugin dtpPlugin = null, IDeviceManagerSA devMgr = null)
        {
            if (dtpPlugin != null)
                dtpProxy = dtpPlugin;
            if (devMgr != null)
                devManagerSA = devMgr;
        }

        private static ILog _log = null; //put ILog object from Device Manager plugin as well
        public DTPHelper(ILog Log)
        {
            _log = Log;
        }

        private void WriteLog(string text, log_type log_type = log_type.info,
            [System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
            [System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "",
            [System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = 0)
        {
            if (string.IsNullOrEmpty(text))
                text = "";

            string className = this.GetType().Name;
            text = $"{DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss.fff")}[DTPHelper] {text}, Class:{className}, Caller Name:{memberName}, Source Line {sourceLineNumber}";
            Console.WriteLine(text);
            if (_log != null)
            {
                if (log_type == log_type.info)
                    _log.Info(text);
                else
                    _log.Error(text);
            }
        }
    }
}
