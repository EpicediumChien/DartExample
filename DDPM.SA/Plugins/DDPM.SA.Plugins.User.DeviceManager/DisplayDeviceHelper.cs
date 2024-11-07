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
using System.IO;
using VcpCore.Common;
using DDPM.SA.Common.Settings;
using System.Windows.Shell;

namespace DDPM.SA.Plugins.User.DeviceManager
{ 
    public class DisplayWindowsToast
    {
        public string Title { get; set; } = string.Empty;
        public string Description {  get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public string ServiceTag { get; set; } = string.Empty;
        public string left_btn { get; set; } = string.Empty;
        public string right_btn { get; set;} = string.Empty;
        public string left_btn_action { get; set; } = "left_btn";
        public string right_btn_action { get; set; } = "right_btn";
    }

    public class DisplayDeviceHelper
    {
        private ISettingsManagerDev settingsManagerDev;
        private string path = string.Empty;

        public enum log_type
        {
            info = 0,
            error
        }

        private static ILog _log = null;
        public DisplayDeviceHelper(ILog Log)
        {
            _log = Log;
            ToastNotificationManagerCompat.OnActivated += toastArgs =>
            {
                AutoImport(toastArgs);
            };
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
                toastContentBuilder.AddButton(content.left_btn, ToastActivationType.Background, "Yes" + "," + content.Model + "," + content.ServiceTag/*content.left_btn_action*/);
                toastContentBuilder.AddButton(content.right_btn, ToastActivationType.Background, content.right_btn_action);

                toastContentBuilder.Show(); // 顯示Toast通知
                WriteLog("[DisplayImportToast] toast Show.");
            });
        }

        public void CheckAndTriggerToastWhileMonitorPlugged(int msec, List<MonitorInfo> mos, ISettingsManagerDev settingsManager)
        {
            if (msec == 8000)//means no UI pluged
            {
                if (mos != null && settingsManager != null)
                {
                    settingsManagerDev = settingsManager;
                    string localAppDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Dell");
                    path = localAppDataPath + "\\Dell Display and Peripheral Manager\\Export";
                    foreach (MonitorInfo monitorInfo in mos)
                    {
                        string model = monitorInfo.modelName;//"U2724DE";
                        string serviceTag = monitorInfo.edid.ServiceTag;
                        string desc = "The same monitor is detected, do you want to import settings for %1?"; //string table: ImpExp_Message.0
                        //
                        //Need jason to implement import/export check here
                        string exportpath = path + "\\" + model + ".json";
                        WriteLog("[CheckAndTriggerToastWhileMonitorPlugged] export path : " + exportpath);
                        //
                        if (File.Exists(exportpath))
                        {
                            DDPMImpExpSettings dDPMImpExpSettings = new DDPMImpExpSettings();
                            dDPMImpExpSettings = settingsManager.ReadImportSettingsFile(exportpath).Result;
                            if (dDPMImpExpSettings != null)
                            {
                                if (dDPMImpExpSettings.MonitorSettings.ImpExpSettings.SameModel)
                                {
                                    DDPMImpExpSettings ImpExpSettings = new DDPMImpExpSettings();
                                    if (settingsManagerDev.DisplayImportSettings(exportpath, true, serviceTag, out ImpExpSettings).Result)
                                    {
                                        WriteLog("[CheckAndTriggerToastWhileMonitorPlugged] Import is success");
                                    }
                                    else
                                    {
                                        WriteLog("[CheckAndTriggerToastWhileMonitorPlugged] Import is fail");
                                    }
                                }
                                else
                                {
                                    desc = desc.Replace("%1", model);
                                    DisplayImportToast(
                                        new DisplayWindowsToast()
                                        {
                                            Title = "Dell Display and Peripheral Manager", //string table: App_Name
                                            Description = desc,
                                            Model = model,
                                            ServiceTag = serviceTag,
                                            left_btn = "Yes",        //string table: Yes
                                            right_btn = "No"       //string table: No
                                        }
                                    );
                                }
                            }
                            else
                            {
                                WriteLog("[CheckAndTriggerToastWhileMonitorPlugged] dDPMImpExpSettings is null.");
                            }
                        }
                        else
                        {
                            WriteLog("[CheckAndTriggerToastWhileMonitorPlugged] exportpath file not found.");
                        }
                    }
                }
                else
                {
                    WriteLog("[CheckAndTriggerToastWhileMonitorPlugged] mos or settingsManager is null");
                }
            }
        }

        private void AutoImport(ToastNotificationActivatedEventArgsCompat e) 
        {
            string[] ret = e.Argument.Split(",");
            if (ret.Length >= 3)
            {
                if (e.Argument.StartsWith("Yes"))
                {
                    if (!string.IsNullOrEmpty(path))
                    {
                        string impPath = path + "\\" + ret[1] + ".json";
                        WriteLog("[CheckAndTriggerToastWhileMonitorPlugged] impPath : " + impPath);
                        if (File.Exists(impPath))
                        {
                            DDPMImpExpSettings ImpExpSettings = new DDPMImpExpSettings();
                            if (settingsManagerDev.DisplayImportSettings(impPath, true, ret[2], out ImpExpSettings).Result)
                            {
                                WriteLog("[CheckAndTriggerToastWhileMonitorPlugged] Import is success");
                            }
                            else
                            {
                                WriteLog("[CheckAndTriggerToastWhileMonitorPlugged] Import is fail");
                            }
                        }
                        else
                        {
                            WriteLog("[CheckAndTriggerToastWhileMonitorPlugged] impPath file not found.");
                        }
                    }
                    else
                    {
                        WriteLog("[CheckAndTriggerToastWhileMonitorPlugged] path is null or empty.");
                    }
                }
                else
                {
                    WriteLog("[CheckAndTriggerToastWhileMonitorPlugged] Not Yes");
                }
            }
            else
            {
                WriteLog("[CheckAndTriggerToastWhileMonitorPlugged] Length < 3");
            }
        }
    }
}
