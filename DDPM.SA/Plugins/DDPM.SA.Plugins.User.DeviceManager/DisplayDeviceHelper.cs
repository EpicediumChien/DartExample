using DDPM.SA.Common;
using Dell.Client.Framework.Common;
using Microsoft.Toolkit.Uwp.Notifications;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VcpCore.Common;

namespace DDPM.SA.Plugins.User.DeviceManager
{
    public class DisplayWindowsToast
    {
        public string Title { get; set; } = string.Empty;
        public string Description {  get; set; } = string.Empty;
        public string left_btn { get; set; } = string.Empty;
        public string right_btn { get; set;} = string.Empty;
        public string left_btn_action { get; set; } = "left_btn";
        public string right_btn_action { get; set; } = "right_btn";
    }

    public class DisplayDeviceHelper
    {        
        public enum log_type
        {
            info = 0,
            error
        }

        private static ILog _log = null;
        public DisplayDeviceHelper(ILog Log) 
        {
            _log = Log;
        }

        private void WriteLog(string text, log_type log_type = log_type.info)
        {
            if (string.IsNullOrEmpty(text))
                text = "";

            text = $"{DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss.fff")}[DeviceManager][Display] {text}";
            Console.WriteLine(text);
            if (_log != null)
            {
                if (log_type == log_type.info)
                    _log.Info(text);
                else
                    _log.Error(text);
            }
        }

        public void DisplayImportToast(DisplayWindowsToast content)
        {
            WriteLog("[DisplayImportToast] Start.");

            Task.Run(() =>
            {
                ToastContentBuilder toastContentBuilder = new ToastContentBuilder();

                toastContentBuilder.AddArgument(content.Title);
                toastContentBuilder.AddText(content.Title);
                toastContentBuilder.AddText(content.Description);
                toastContentBuilder.AddButton(content.left_btn, ToastActivationType.Background, content.left_btn_action);
                toastContentBuilder.AddButton(content.right_btn, ToastActivationType.Background, content.right_btn_action);

                toastContentBuilder.Show(); // 顯示Toast通知
                WriteLog("[DisplayImportToast] toast Show.");
            });
        }

        public void CheckAndTriggerToastWhileMonitorPlugged(int msec, List<MonitorInfo> mos)
        {
            if (msec == 8000)//means no UI pluged
            {
                string model = "U2724DE";
                string desc = "The same monitor is detected, do you want to import settings for %1?"; //string table: ImpExp_Message.0
                //
                //Need jason to implement import/export check here
                //
                //if(your criterial)
                {
                    desc = desc.Replace("%1", model);
                    DisplayImportToast(
                        new DisplayWindowsToast()
                        {
                            Title = "Dell Display and Peripheral Manager", //string table: App_Name
                            Description = desc,
                            left_btn = "Yes",        //string table: Yes
                            right_btn = "No"       //string table: No
                        }
                    );
                }
            }
        }
    }
}
