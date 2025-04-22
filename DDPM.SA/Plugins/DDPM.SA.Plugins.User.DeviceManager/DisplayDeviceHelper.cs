using DDPM.SA.Common;
using DDPM.SA.Common.Display;
using DDPM.SA.Common.Settings;
using DDPM.SA.Resources.Helper;
using Dell.Client.Framework.Common;
using Microsoft.Toolkit.Uwp.Notifications;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Security;
using System.Threading.Tasks;
using VcpCore.Common;

namespace DDPM.SA.Plugins.User.DeviceManager
{
    public class DisplayWindowsToast
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public string ServiceTag { get; set; } = string.Empty;
        public string left_btn { get; set; } = string.Empty;
        public string right_btn { get; set; } = string.Empty;
        public string left_btn_action { get; set; } = "left_btn";
        public string right_btn_action { get; set; } = "right_btn";
    }

    public class DisplayDeviceHelper
    {
        private static ISettingsManagerDev settingsManagerDev = null;
        private static IDeviceManagerSA devManagerSA = null;
        private static IDisplayService displayService = null;
        private static IDeviceManagerSA _baseDeviceManagerSA;

        private string path = string.Empty;

        public enum log_type
        {
            info = 0,
            error
        }

        public void UpdateDDPMPluginInstances(ISettingsManagerDev settings = null, IDeviceManagerSA devMgr = null, IDisplayService displaySrv = null)
        {
            if (settings != null)
                settingsManagerDev = settings;
            if (devMgr != null)
                devManagerSA = devMgr;
            if (displaySrv != null)
                displayService = displaySrv;
            Debug.WriteLine("[UpdateDDPMPluginInstances] devManagerSA is " + (devMgr == null ? "NULL" : "NOTNULL"));
            WriteLog("[UpdateDDPMPluginInstances] devManagerSA is " + (devMgr == null ? "NULL" : "NOTNULL"));
        }

        private static ILog _log = null;

        public DisplayDeviceHelper(ILog Log, IDeviceManagerSA baseDeviceManagerSA = null)
        {
            _log = Log;
            _baseDeviceManagerSA = baseDeviceManagerSA;
            ToastNotificationManagerCompat.OnActivated += toastArgs =>
            {
                AutoImport(toastArgs);
            };
        }

        private void WriteLog(string text, log_type log_type = log_type.info,
            [System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
            [System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "",
            [System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = 0)
        {
            if (string.IsNullOrEmpty(text))
                text = "";

            string className = this.GetType().Name;
            text = $"{DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss.fff")}[DisplayDeviceHelper] {text}, Class:{className}, Caller Name:{memberName}, Source Line {sourceLineNumber}";
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
                try
                {
                    ToastContentBuilder toastContentBuilder = new ToastContentBuilder();

                    toastContentBuilder.AddArgument(content.Title);
                    toastContentBuilder.AddText(content.Title);
                    toastContentBuilder.AddText(content.Description);
                    toastContentBuilder.AddButton(content.left_btn, ToastActivationType.Background, "Yes" + "," + content.Model + "," + content.ServiceTag/*content.left_btn_action*/);
                    toastContentBuilder.AddButton(content.right_btn, ToastActivationType.Background, content.right_btn_action);

                    toastContentBuilder.Show(); // 顯示Toast通知
                    WriteLog("[DisplayImportToast] toast Show.");
                }
                catch (Exception ex) 
                {
                    WriteLog($"[DisplayImportToast] throws exception {ex.Message}, StackTrace: {ex.StackTrace}.");
                }
            });
        }

        public void CheckAndTriggerToastWhileMonitorPlugged(int msec, List<MonitorInfo> mos, ISettingsManagerDev settingsManager)
        {
            WriteLog($"[CheckAndTriggerToastWhileMonitorPlugged] Entrance.");
            string processName = "DDPM"; //"notepad";
            Process[] processes = Process.GetProcessesByName(processName);
            if (processes != null && processes.Length == 0)//means no UI pluged
            {
                WriteLog($"[CheckAndTriggerToastWhileMonitorPlugged] is monitor list null: {mos == null}, is settingsManager null: {settingsManager == null}");
                if (mos != null && settingsManager != null)
                {
                    settingsManagerDev = settingsManager;
                    string localAppDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Dell");
                    path = localAppDataPath + "\\Dell Display and Peripheral Manager\\Export";
                    WriteLog($"[CheckAndTriggerToastWhileMonitorPlugged] monitor list count: {mos.Count}");
                    foreach (MonitorInfo monitorInfo in mos)
                    {
                        bool isSameModelFlag = false;
                        string model = monitorInfo.modelName;//"U2724DE";
                        string serviceTag = monitorInfo.edid.ServiceTag;
                        string desc = LangHelper.Instance["ImpExp_Message.0"]; //string table: ImpExp_Message.0
                        //
                        //Need jason to implement import/export check here
                        string exportpath = path + "\\" + model + ".json";
                        string displayProfilePath = $"{localAppDataPath}\\Dell Display and Peripheral Manager\\Display\\{model}.json";
                        // try fix cannot devManagerSA didn't initialize issue
                        if (devManagerSA == null)
                            devManagerSA = _baseDeviceManagerSA;
                        if (devManagerSA != null)
                            isSameModelFlag = devManagerSA.ReadSameModelAutoApplySameModelFlag(displayProfilePath, model).Result;
                        else
                            WriteLog("[CheckAndTriggerToastWhileMonitorPlugged] Error devManagerSA not initialized.");
                        WriteLog("[CheckAndTriggerToastWhileMonitorPlugged] export path : " + exportpath);

                        // Guess due to permission issue file cannot read here properly
                        //if (File.Exists(exportpath.Trim()))
                        //{
                        DDPMImpExpSettings dDPMImpExpSettings = new DDPMImpExpSettings();
                        // ReadImportSettingsFile will check file existence
                        dDPMImpExpSettings = settingsManager.ReadImportSettingsFile(exportpath).Result;
                        if (!string.IsNullOrEmpty(dDPMImpExpSettings?.MonitorSettings?.Model))
                        {
                            WriteLog("[CheckAndTriggerToastWhileMonitorPlugged] Checked exported file exists!");
                            if (dDPMImpExpSettings.MonitorSettings.ServiceTag != serviceTag)
                            {
                                WriteLog("[CheckAndTriggerToastWhileMonitorPlugged] Checked import monitor serviceTag is different.");
                                if (isSameModelFlag)
                                {
                                    DDPMImpExpSettings ImpExpSettings = new DDPMImpExpSettings();
                                    if ((int)settingsManagerDev.DisplayImportSettings(exportpath, true, serviceTag, out ImpExpSettings).Result > 0)
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
                                    WriteLog("[CheckAndTriggerToastWhileMonitorPlugged] isSameModelFlag is false.");
                                    desc = desc.Replace("%1", model);
                                    DisplayImportToast(
                                        new DisplayWindowsToast()
                                        {
                                            Title = LangHelper.Instance["App_Name"], //string table: App_Name
                                            Description = desc,
                                            Model = model,
                                            ServiceTag = serviceTag,
                                            left_btn = LangHelper.Instance["Yes"],        //string table: Yes
                                            right_btn = LangHelper.Instance["No"]       //string table: No
                                        }
                                    );
                                }
                            }
                            else
                            {
                                WriteLog($"[CheckAndTriggerToastWhileMonitorPlugged] dDPMImpExpSettings.MonitorSettings.ServiceTag is same: {serviceTag}.");
                            }
                        }
                        else
                        {
                            WriteLog("[CheckAndTriggerToastWhileMonitorPlugged] dDPMImpExpSettings is null.");
                        }
                        //}
                        //else
                        //{
                        //    WriteLog("[CheckAndTriggerToastWhileMonitorPlugged] exportpath file not found.");
                        //    try
                        //    {
                        //        var files = Directory.GetFiles(exportpath.Trim()); // This will throw if access is denied
                        //        WriteLog($"Found {files.Length} files.");
                        //    }
                        //    catch (UnauthorizedAccessException ex)
                        //    {
                        //        WriteLog($"[CheckAndTriggerToastWhileMonitorPlugged] [Permission Error] You don't have access: {ex.Message}");
                        //    }
                        //    catch (SecurityException ex)
                        //    {
                        //        WriteLog($"[CheckAndTriggerToastWhileMonitorPlugged] [Security Error] Access denied due to security policy: {ex.Message}");
                        //    }
                        //    catch (PathTooLongException ex)
                        //    {
                        //        WriteLog($"[CheckAndTriggerToastWhileMonitorPlugged] [Path Error] Path too long: {ex.Message}");
                        //    }
                        //    catch (Exception ex)
                        //    {
                        //        WriteLog($"[CheckAndTriggerToastWhileMonitorPlugged] [Other Error] {ex.Message}.");
                        //    }
                        //}
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
                            if ((int)settingsManagerDev.DisplayImportSettings(impPath, true, ret[2], out ImpExpSettings).Result > 0)
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

        //for PIMS-288826
        public bool isHotkeySyncBrightnessContrastToAllMonitors(HotkeyType job, List<MonitorInfo> moLists, MonitorInfo currentMoInfo, List<ALSConfig> alsSynchronizeList)
        {
            if (moLists == null || moLists.Count <= 1)
            {
                return false;
            }
            if (job == HotkeyType.BrightnessIncrease || job == HotkeyType.LuminanceIncrease || job == HotkeyType.ContrastIncrease ||
                job == HotkeyType.BrightnessReduce || job == HotkeyType.LuminanceReduce || job == HotkeyType.ContrastReduce)
            {
                bool isSyncEnable = false;
                DeviceMangerPlugin? devMgr = (DeviceMangerPlugin)devManagerSA;
                if (devMgr == null)
                {
                    WriteLog("[isHotkeySyncBrightnessContrastToAllMonitors] default is false since null deviceManager");
                    return false;
                }
                DDPMSettings data = devMgr.ReloadAppConfigData().Result;
                isSyncEnable = data.UserSettings.IsSynchronizemonitor;
                string actionType = CheckisShowSynchronize(displayService, moLists, currentMoInfo, alsSynchronizeList).Result;
                if (actionType.Equals("A") || actionType.Equals("B") || actionType.Equals("C"))
                {
                    if (isSyncEnable)
                        WriteLog($"[isHotkeySyncBrightnessContrastToAllMonitors] should sync {job} between monitor");
                    else
                        WriteLog($"[isHotkeySyncBrightnessContrastToAllMonitors] should *not* sync {job} between monitor");
                    return isSyncEnable;
                }
                else
                {
                    WriteLog($"[isHotkeySyncBrightnessContrastToAllMonitors] sync option isn't active ({actionType})");
                }
            }
            else
            {
                WriteLog($"[isHotkeySyncBrightnessContrastToAllMonitors] job is not belong to Bri/Con/Lum");
            }

            return false;
        }

        //for PIMS-288826
        public void PerformHotKeyBrightnessContrastLuminanceAction(HotkeyType job, List<MonitorInfo> moLists, MonitorInfo currentMoInfo, List<ALSConfig> alsSynchronizeList)
        {
            if (devManagerSA == null)
            {
                WriteLog("[PerformHotKeyBrightnessContrastLuminanceAction]monitor[{currentMoInfo.AliasDeviceName};{currentMoInfo.edid.ServiceTag}] devManagerSA is null");
                Debug.WriteLine("[PerformHotKeyBrightnessContrastLuminanceAction]monitor[{currentMoInfo.AliasDeviceName};{currentMoInfo.edid.ServiceTag}] devManagerSA is null");
                return;
            }
            bool doSync = isHotkeySyncBrightnessContrastToAllMonitors(job, moLists, currentMoInfo, alsSynchronizeList);
            byte code = 0x10;

            if (job == HotkeyType.BrightnessIncrease || job == HotkeyType.BrightnessReduce ||
                job == HotkeyType.LuminanceIncrease || job == HotkeyType.LuminanceReduce)
            {
                code = 0x10;
            }
            else if (job == HotkeyType.ContrastIncrease || job == HotkeyType.ContrastReduce)
            {
                code = 0x12;
            }
            else
            {
                WriteLog($"[PerformHotKeyBrightnessContrastLuminanceAction]monitor[{{currentMoInfo.AliasDeviceName}};{{currentMoInfo.edid.ServiceTag}}], un-supported job: {job}");
                return;
            }
            ObjGetVCP obVCPValue = devManagerSA.GetVCPCapability(currentMoInfo, code, opt: 0, priority: Priority.High).Result;
            Debug.WriteLine($"PerformHotKeyBrightnessContrastLuminanceAction,monitor[{currentMoInfo.AliasDeviceName};{currentMoInfo.edid.ServiceTag}] ,current: code={code},value={obVCPValue.value}");
            WriteLog($"PerformHotKeyBrightnessContrastLuminanceAction,monitor[{currentMoInfo.AliasDeviceName};{currentMoInfo.edid.ServiceTag}] ,current: code={code},value={obVCPValue.value}");
            uint targetValue = (uint)obVCPValue.value;
            uint maxLuminace = 100;
            if (job == HotkeyType.LuminanceIncrease || job == HotkeyType.LuminanceReduce)
            {
                ObjGetVCP obMaxValue = devManagerSA.GetVCPCapability(currentMoInfo, code, opt: 1, priority: Priority.High).Result;
                if (obMaxValue.result)
                    maxLuminace = (uint)obMaxValue.value;
                Debug.WriteLine($"PerformHotKeyBrightnessContrastLuminanceAction,monitor[{currentMoInfo.AliasDeviceName};{currentMoInfo.edid.ServiceTag}] ,max Luminace value={maxLuminace}");
                WriteLog($"PerformHotKeyBrightnessContrastLuminanceAction,monitor[{currentMoInfo.AliasDeviceName};{currentMoInfo.edid.ServiceTag}] ,max Luminace value={maxLuminace}");
            }
            if (job == HotkeyType.BrightnessIncrease || job == HotkeyType.ContrastIncrease)
                targetValue = ((uint)obVCPValue.value) >= 95 ? 100 : ((uint)obVCPValue.value + 5);
            else if (job == HotkeyType.LuminanceIncrease)
                targetValue = ((uint)obVCPValue.value) >= (maxLuminace - 5) ? maxLuminace : ((uint)obVCPValue.value + 5);
            else if (job == HotkeyType.BrightnessReduce || job == HotkeyType.LuminanceReduce || job == HotkeyType.ContrastReduce)
                targetValue = ((uint)obVCPValue.value) <= 5 ? 0 : ((uint)obVCPValue.value - 5);
            else
            {
                WriteLog($"[PerformHotKeyBrightnessContrastLuminanceAction] un-support job: {job}");
                return;
            }
            //PIMS-298672 Use UP3221Q and enter hotkey that assigned for Luminance from keyboard, the luminance value is increasing/decreasing by 15 nits per interval.
            if ("UP3221Q".Equals(currentMoInfo.modelName, StringComparison.OrdinalIgnoreCase))
            {
                if (job == HotkeyType.LuminanceIncrease)
                    targetValue = ((uint)obVCPValue.value) >= (maxLuminace - 15) ? maxLuminace : ((uint)obVCPValue.value + 15);
                else if (job == HotkeyType.LuminanceReduce)
                    targetValue = ((uint)obVCPValue.value) <= 15 ? 0 : ((uint)obVCPValue.value - 15);
                else
                {
                    WriteLog($"[PerformHotKeyBrightnessContrastLuminanceAction] un-support job: {job}");
                    return;
                }
                WriteLog($"PerformHotKeyBrightnessContrastLuminanceAction,monitor[{currentMoInfo.AliasDeviceName};{currentMoInfo.edid.ServiceTag}] ,UP3221Q Luminance targetValue={targetValue}");
            }
            //PIMS-298672 Use UP2720Q and enter hotkey that assigned for Luminance from keyboard, the luminance value is increasing/decreasing by 10 nits per interval.
            if ("UP2720Q".Equals(currentMoInfo.modelName, StringComparison.OrdinalIgnoreCase))
            {
                if (job == HotkeyType.LuminanceIncrease)
                    targetValue = ((uint)obVCPValue.value) >= (maxLuminace - 10) ? maxLuminace : ((uint)obVCPValue.value + 10);
                else if (job == HotkeyType.LuminanceReduce)
                    targetValue = ((uint)obVCPValue.value) <= 10 ? 0 : ((uint)obVCPValue.value - 10);
                else
                {
                    WriteLog($"[PerformHotKeyBrightnessContrastLuminanceAction] un-support job: {job}");
                    return;
                }
                WriteLog($"PerformHotKeyBrightnessContrastLuminanceAction,monitor[{currentMoInfo.AliasDeviceName};{currentMoInfo.edid.ServiceTag}] ,UP2720Q Luminance targetValue={targetValue}");
            }
            WriteLog($"PerformHotKeyBrightnessContrastLuminanceAction,monitor[{currentMoInfo.AliasDeviceName};{currentMoInfo.edid.ServiceTag}] ,before SetVCPCapability:code={code}; targetValue={targetValue}");
            bool ret = devManagerSA.SetVCPCapability(currentMoInfo, code, targetValue, priority: Priority.High).Result;
            WriteLog($"PerformHotKeyBrightnessContrastLuminanceAction;{job}:[{currentMoInfo.edid.ModelName}:{currentMoInfo.edid.SerialNumber}] from [{(uint)obVCPValue.value}] to [{targetValue}]" + (ret ? "success" : "fail"));

            if (doSync)
            {
                foreach (MonitorInfo mi in moLists)
                {
                    if (currentMoInfo.modelName.Equals(mi.modelName) && currentMoInfo.edid.ServiceTag.Equals(mi.edid.ServiceTag))
                    {
                        //already set, next loop
                        continue;
                    }
                    //obVCPValue = devManagerSA.GetVCPCapability(mi, code, 0).Result;
                    if (obVCPValue.result)
                    {
                        //targetValue = (uint)obVCPValue.value;
                        if (job == HotkeyType.BrightnessIncrease || job == HotkeyType.LuminanceIncrease || job == HotkeyType.ContrastIncrease)
                            targetValue = ((uint)obVCPValue.value) >= 95 ? 100 : ((uint)obVCPValue.value + 5);
                        else if (job == HotkeyType.BrightnessReduce || job == HotkeyType.LuminanceReduce || job == HotkeyType.ContrastReduce)
                            targetValue = ((uint)obVCPValue.value) <= 5 ? 0 : ((uint)obVCPValue.value - 5);
                        else
                            continue;

                        var r = devManagerSA.CheckIsSyncBriCon(currentMoInfo, mi).Result;
                        if (r) ret = devManagerSA.SetVCPCapability(mi, code, targetValue, priority: Priority.High).Result;
                        WriteLog($"{job}:[{mi.edid.ModelName}:{mi.edid.SerialNumber}] from [{(uint)obVCPValue.value}] to [{targetValue}]" + (ret ? "success" : "fail"));
                    }
                }
            }
        }

        public Task<string> CheckisShowSynchronize(IDisplayService _displayManagerPlugin, List<MonitorInfo> moLists, MonitorInfo currentMoInfo, List<ALSConfig> alsSynchronizeList)//PIMS-285802 PIMS-285804
        {
            WriteLog($"[DisplayDeviceHelper] CheckisShowSynchronize in ... ");
            try
            {
                if (moLists == null || _displayManagerPlugin == null)
                    return Task.FromResult("null");

                ALSConfig Current_ALSConfig = new ALSConfig();
                Current_ALSConfig = _displayManagerPlugin.GetALSFeatureValue(currentMoInfo, ALSFeatureQueryType.All, 0).Result;

                //Re-Check isShowSynchronize
                if (moLists.Count > 1)//Only check if there is more than one monitor.
                {
                    WriteLog($"[DisplayDeviceHelper] CheckisShowSynchronize Monitor Count = {moLists.Count.ToString()} ... ");
                    if (alsSynchronizeList.Count == 0)
                    {
                        alsSynchronizeList = _displayManagerPlugin.GetAllExistAlsConfig().Result;
                    }
                    //It is mean over 2 monitors.
                    else if (alsSynchronizeList.Count == 2)//Test case for 2 monitors
                    {
                        WriteLog($"[DisplayDeviceHelper] CheckisShowSynchronize ALS Monitor Count = 2 ... ");
                        //25 Test Scenario : 2 same monitors with ALS Function
                        if (alsSynchronizeList[0].ModelName == alsSynchronizeList[1].ModelName &&
                            alsSynchronizeList[0].isSupportALS == 2 &&
                            alsSynchronizeList[1].isSupportALS == 2)
                        {
                            if (CheckALSOnOff(alsSynchronizeList) == false)
                            {
                                //SynchronizeBtnExpectedResult("C");
                                return Task.FromResult("C");
                            }
                            else
                            {
                                //SynchronizeBtnExpectedResult("D");
                                return Task.FromResult("D");
                            }
                        }
                        //26 Test Scenario : 2 different monitors with ALS Function
                        if (alsSynchronizeList[0].ModelName != alsSynchronizeList[1].ModelName &&
                            alsSynchronizeList[0].isSupportALS == 2 &&
                            alsSynchronizeList[1].isSupportALS == 2)
                        {
                            if (CheckALSOnOff(alsSynchronizeList) == false)
                            {
                                //SynchronizeBtnExpectedResult("C");
                                return Task.FromResult("C");
                            }
                            else
                            {
                                //SynchronizeBtnExpectedResult("D");
                                return Task.FromResult("D");
                            }
                        }
                        //13 Test Scenario : 2 same UP series monitors
                        if (alsSynchronizeList[0].ModelName.Contains("UP") && alsSynchronizeList[1].ModelName.Contains("UP") &&
                            alsSynchronizeList[0].ModelName == alsSynchronizeList[1].ModelName)
                        {
                            //SynchronizeBtnExpectedResult("A");
                            return Task.FromResult("A");
                        }
                        //14 Test Scenario : 2 same non UP series monitors without ALS function
                        if (!alsSynchronizeList[0].ModelName.Contains("UP") && !alsSynchronizeList[1].ModelName.Contains("UP") &&
                            alsSynchronizeList[0].ModelName == alsSynchronizeList[1].ModelName &&
                            alsSynchronizeList[0].isSupportALS == 0 && alsSynchronizeList[1].isSupportALS == 0)
                        {
                            //Expected Result B:
                            //SynchronizeBtnExpectedResult("B");
                            return Task.FromResult("B");
                        }
                        //15 Test Scenario : 2 different UP series monitors
                        if (alsSynchronizeList[0].ModelName.Contains("UP") && alsSynchronizeList[1].ModelName.Contains("UP") &&
                            alsSynchronizeList[0].ModelName != alsSynchronizeList[1].ModelName)
                        {
                            //Expected Result E.
                            //SynchronizeBtnExpectedResult("E");
                            return Task.FromResult("E");
                        }
                        //16 Test Scenario : 2 different Non UP series monitors without ALS function
                        if (!alsSynchronizeList[0].ModelName.Contains("UP") && !alsSynchronizeList[1].ModelName.Contains("UP") &&
                            alsSynchronizeList[0].isSupportALS == 0 && alsSynchronizeList[1].isSupportALS == 0)
                        {
                            //Expected Result B.
                            //SynchronizeBtnExpectedResult("B");
                            return Task.FromResult("B");
                        }
                        //17 Test Scenario : UP monitor and Non UP series monitor without ALS function
                        if ((alsSynchronizeList[0].ModelName.Contains("UP") || alsSynchronizeList[1].ModelName.Contains("UP")) &&
                            (!alsSynchronizeList[0].ModelName.Contains("UP") || !alsSynchronizeList[1].ModelName.Contains("UP")) &&
                            alsSynchronizeList[0].isSupportALS == 0 && alsSynchronizeList[1].isSupportALS == 0)
                        {
                            //"Synchronize between monitors" is NOT displayed.
                            //SynchronizeBtnExpectedResult("E");
                            return Task.FromResult("E");
                        }
                        //18 Test Scenario : UP monitor and ALS function monitor
                        if (alsSynchronizeList[0].ModelName.Contains("UP") || alsSynchronizeList[1].ModelName.Contains("UP") &&
                            ((alsSynchronizeList[0].isSupportALS == 2) ^ (alsSynchronizeList[1].isSupportALS == 2)))
                        {
                            //"Synchronize between monitors" is NOT displayed.
                            //SynchronizeBtnExpectedResult("E");
                            return Task.FromResult("E");
                        }
                        //else
                        //{
                        //19 Test Scenario : G series monitor and S series monitor
                        //20 Test Scenario : AW series Freesync monitor and U series monitor
                        //21 Test Scenario : C series, SE series, E series and P series monitors
                        //Expected Result B:
                        //SynchronizeBtnExpectedResult("B");
                        return Task.FromResult("B");
                        //}
                    }
                    else if (alsSynchronizeList.Count == 3)//Test case for 3 monitors
                    {
                        WriteLog($"[DisplayDeviceHelper] CheckisShowSynchronize ALS Monitor Count = 3 ... ");
                        //0x12 = non Luminance
                        //22 Test Scenario : 2 monitors with Brightness/Contrast and 1 monitor with Luminance
                        if (CheckNoneLuminanceMonitorCount(moLists) == 2)
                        {
                            bool obj = currentMoInfo.CapabilityDic.ContainsKey("12");
                            if (obj)
                            {
                                //Result same as Expected Result B and not apply to DUT3.
                                //SynchronizeBtnExpectedResult("B");
                                return Task.FromResult("B");
                            }
                            else
                            {
                                //"Synchronize between monitors" is NOT displayed.
                                //SynchronizeBtnExpectedResult("E");
                                return Task.FromResult("E");
                            }
                        }
                        //23 Test Scenario : 1 monitor with Brightness/Contrast and 2 monitors with Luminance
                        if (CheckNoneLuminanceMonitorCount(moLists) == 1)
                        {
                            bool obj = currentMoInfo.CapabilityDic.ContainsKey("12");
                            if (obj)
                            {
                                //Result same as Expected Result B and not apply to DUT3.
                                //SynchronizeBtnExpectedResult("A");
                                return Task.FromResult("A");
                            }
                            else
                            {
                                //"Synchronize between monitors" is NOT displayed.
                                //SynchronizeBtnExpectedResult("E");
                                return Task.FromResult("E");
                            }
                        }
                        //27 Test Scenario : 1 ALS monitor(ALS = ON) and 2 non ALS monitors
                        //28 Test Scenario : 1 ALS monitor(ALS = OFF) and 2 non ALS monitors
                        if (CheckALSMonitorCount(alsSynchronizeList) == 1)
                        {
                            if (Current_ALSConfig.isSupportALS == 2)
                            {
                                if (Current_ALSConfig.isAutoBrightness == true || Current_ALSConfig.isAutoColorTemp == true)
                                {
                                    //a) DUT3 is monitor with ALS function.
                                    //"Synchronize between monitors" is displayed on DUT3 but greyed out.
                                    //SynchronizeBtnExpectedResult("D");
                                    return Task.FromResult("D");
                                }
                                else
                                {
                                    //SynchronizeBtnExpectedResult("B");
                                    return Task.FromResult("B");
                                }
                            }
                            else
                            {
                                //b) DUT1 and DUT2 are monitors without ALS function.
                                //SynchronizeBtnExpectedResult("B");
                                return Task.FromResult("B");
                            }
                        }
                        //29 Test Scenario : 2 ALS monitors(ALS = ON) and 1 non ALS monitor
                        //30 Test Scenario : 2 ALS monitors(ALS = OFF) and 1 non ALS monitor
                        if (CheckALSMonitorCount(alsSynchronizeList) == 2)
                        {
                            if (CheckALSOnOff(alsSynchronizeList))
                            {
                                //a) DUT1 and DUT2 are monitors with ALS function.
                                //"Synchronize between monitors" is displayed on DUT1 and DUT2 but greyed out.
                                //SynchronizeBtnExpectedResult("D");
                                return Task.FromResult("D");
                            }
                            else
                            {
                                //"Synchronize between monitors" is displayed.no greyed out.
                                //SynchronizeBtnExpectedResult("B");
                                return Task.FromResult("B");
                            }
                        }
                    }
                    else//Test case for 4 monitors
                    {
                        WriteLog($"[DisplayDeviceHelper] CheckisShowSynchronize ALS Monitor Count = 4 ... ");
                        //24 Test Scenario : 2 monitors with Brightness/Contrast and 2 monitors with Luminance
                        if (CheckNoneLuminanceMonitorCount(moLists) == 2)
                        {
                            bool obj = currentMoInfo.CapabilityDic.ContainsKey("12");
                            if (obj)
                            {
                                //Result same as Expected Result B and not apply to DUT3.
                                //SynchronizeBtnExpectedResult("B");
                                return Task.FromResult("B");
                            }
                            else
                            {
                                //"Synchronize between monitors" is NOT displayed.
                                //SynchronizeBtnExpectedResult("A");
                                return Task.FromResult("A");
                            }
                        }
                        //31 Test Scenario : 2 ALS monitors(ALS = ON) and 2 non ALS monitor
                        if (CheckALSMonitorCount(alsSynchronizeList) == 2)
                        {
                            //d) Turn on ALS Function on DUT1 and DUT2. Go to Software > Brightness / Contrast > Auto > Turn On Auto Brightness / Auto Color Temperature.
                            if (CheckALSOnOff(alsSynchronizeList))
                            {
                                //"Synchronize between monitors" is displayed but greyed out on both DUT1 and DUT2.
                                //SynchronizeBtnExpectedResult("D");
                                return Task.FromResult("D");
                            }
                            else
                            {
                                //SynchronizeBtnExpectedResult("B");
                                return Task.FromResult("B");
                            }
                        }
                        //32 Test Scenario : 4 same non-UP models without ALS function
                        //33 Test Scenario : 4 same UP models
                        if (CheckALSMonitorCount(alsSynchronizeList) == 0)
                        {
                            //"Synchronize between monitors" is displayed and not greyed out with default is OFF.
                            //"Synchronize between monitors" is displayed and not greyed out with default is OFF.
                            //SynchronizeBtnExpectedResult("B");
                            return Task.FromResult("B");
                        }
                    }
                }
                else//It is mean only 1 monitors.
                {
                    WriteLog($"[DisplayDeviceHelper] CheckisShowSynchronize Monitor Count = 1 ... ");
                    //3 Test Scenario : Non UP series Monitor does not support ALS
                    if (!alsSynchronizeList[0].ModelName.Contains("UP") && alsSynchronizeList[0].isSupportALS == 0)
                    {
                        //Make sure "Synchronize between monitors" is NOT displayed on both Manual and Schedule.
                        //SynchronizeBtnExpectedResult("E");
                        return Task.FromResult("E");
                    }
                    //4 Test Scenario : Monitor support ALS
                    if (alsSynchronizeList[0].isSupportALS == 2)
                    {
                        //Make sure "Synchronize between monitors" is NOT displayed.
                        //SynchronizeBtnExpectedResult("E");
                        return Task.FromResult("E");
                    }
                    //5 Test Scenario : UP series Monitor
                    if (alsSynchronizeList[0].ModelName.Contains("UP"))
                    {
                        //Make sure "Synchronize between monitors" is NOT displayed on both Manual and Schedule.
                        //SynchronizeBtnExpectedResult("E");
                        return Task.FromResult("E");
                    }
                }
                //SynchronizeBtnExpectedResult("default");
                return Task.FromResult("default");
            }
            catch (Exception ex)
            {
                WriteLog($"[DisplayDeviceHelper] CheckisShowSynchronize Exception : {ex.Message} ... ");
                return Task.FromResult("default");
            }
        }

        /// <summary>
        /// Check if AutoBrightness/AutoColorTemp is enabled.
        /// </summary>
        /// <param name="aLSList">ALS value list</param>
        /// <returns>Return true or false</returns>
        private bool CheckALSOnOff(List<ALSConfig> aLSList)
        {
            bool _isAlSON = false;
            try
            {
                for (int i = 0; i < aLSList.Count; i++)
                {
                    if (aLSList[i].isAutoBrightness == true || aLSList[i].isAutoColorTemp == true)
                    {
                        _isAlSON = true;
                        break;
                    }
                }
            }
            catch (Exception e)
            {
                WriteLog($"[DisplayDeviceHelper] CheckALSOnOff Exception: {e.Message}");
            }
            return _isAlSON;
        }

        /// <summary>
        ///  Check the number of Luminance Monitor.0x12 = non Luminance
        /// </summary>
        /// <returns>Return Luminance count</returns>
        private int CheckNoneLuminanceMonitorCount(List<MonitorInfo> moLists)
        {
            int _isNoneLuminanceCount = 0;
            try
            {
                for (int i = 0; i < moLists.Count; i++)
                {
                    if (moLists[i].CapabilityDic.ContainsKey("12"))
                    {
                        _isNoneLuminanceCount++;
                    }
                }
            }
            catch (Exception e)
            {
                WriteLog($"[DisplayDeviceHelper] CheckNoneLuminanceMonitorCount Exception: {e.Message}");
            }
            return _isNoneLuminanceCount;
        }

        /// <summary>
        /// Check the number of ALS Monitor.
        /// </summary>
        /// <param name="aLSList">ALS value list</param>
        /// <returns>Return ALS Monitor count</returns>
        private int CheckALSMonitorCount(List<ALSConfig> aLSList)
        {
            int _isMutliAlsMonitorCount = 0;
            try
            {
                for (int i = 0; i < aLSList.Count; i++)
                {
                    if (aLSList[i].isSupportALS == 2)
                        _isMutliAlsMonitorCount++;
                }
            }
            catch (Exception e)
            {
                WriteLog($"[DisplayDeviceHelper] CheckALSMonitorCount Exception: {e.Message}");
            }
            return _isMutliAlsMonitorCount;
        }
    }
}