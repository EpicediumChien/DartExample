using DDPM.SA.Common;
using Dell.Client.Framework.Common.Annotations;
using Dell.Client.Framework.Interfaces;
using DPeMPublic.Common.Enums;
using Microsoft;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static DDPM.SA.Common.ICLICommandTable;
using Console = System.Console;

namespace DDPM.CLI.Plugins.Peripherals
{
    [Plugin(IDs.CLI_Plugin_Peripherals, pluginName, PluginOrderGroupType.Core, Version = pluginVersion)]
    [Descriptor(Description = pluginDescription)]
    [Publisher(Name = publisherCompany, Website = publisherWebsite, Support = publisherSupport)]
    [PublishedInterface(new[] { typeof(ICLIPeripherals) })]
    [DependencyKnownTypes(new[] { typeof(ICLIPeripherals) })]
    public class CLIPeripheralsPlugins : BaseAgentPlugin, IDisposableObservable, ICLIPeripherals
    {
        #region Private Members

        private const string pluginName = "CLIPeripheralsPlugin";
        private const string pluginVersion = "1.0.0";
        private const string pluginDescription = "This plugin implements CLI Peripherals Plugin.";
        private const string publisherCompany = "Wistron";
        private const string publisherWebsite = "https://www.wistron.com";
        private const string publisherSupport = "This plugin implements CLI Peripherals Plugin.";

        //private IAgent _agent; //Dean 0626 fix SAST issue
        public const string PluginLogId = "CLIPeripherals";

        //private readonly object _PluginConditionLock = new object();

        private enum log_type
        {
            info = 0,
            error
        }

        private IDeviceManagerSA _devMgr;
        private List<DeviceInfo> _deviceinfo = null;
        private CommandLineInput _commandLineInput = null;
        private readonly List<CLI_PeripheralRESPONSE> GetResults = new();
        private readonly List<CLI_PeripheralRESPONSE> SetResults = new();
        private string value = "";
        private List<Guid> Guids = new();
        private Func<int, Guid, Task> taskA;
        private Func<bool, Guid, Task> taskB;
        private Func<int, Guid, String, Task> taskC;

        #endregion Private Members

        #region Constructor

        public CLIPeripheralsPlugins(IAgent agent) : base(agent, PluginLogId)
        {
            //_agent = agent;
            writelog("CLIPeripheralsPlugins constructor ...");
        }

        #endregion Constructor

        #region interface implementation

        public CLIEventResult SetCommandArgs(CLIEventArgs input, IDeviceManagerSA devMgr)
        {
            _devMgr = devMgr;
            CommandLineInput commandLineInput = input.commandLineInput;
            _commandLineInput = commandLineInput;//set to global
            CLIEventResult result = new CLIEventResult();
            result.command_guid_string = input.command_guid_string;
            result.ticket = DateTime.Now;
            int exitcode = 0;
            if (commandLineInput != null)
            {
                if (_commandLineInput.PluginsType.Equals("AUDIO"))
                    _commandLineInput.PluginsType = "HEADSET";

                if (commandLineInput.Command.Equals("SET"))
                {
                    exitcode = SetPeripheralProperty();
                    string json = JsonConvert.SerializeObject(SetResults, Formatting.Indented);
                    Console.WriteLine(json);
                    result.ExitCode = exitcode;
                    result.serialize_Json_response = json;
                    return result;
                }
                else if (commandLineInput.Command.Equals("GET"))
                {
                    exitcode = GetPeripheralProperty();
                    string json = JsonConvert.SerializeObject(GetResults, Formatting.Indented);
                    Console.WriteLine(json);
                    result.ExitCode = exitcode;
                    result.serialize_Json_response = json;
                    return result;
                }
                else // for firmware update.
                {
                    switch (commandLineInput.TargetFeature)
                    {
                        case "FWUPDATE":
                        case "UODFWUPDATE":
                        case "LOCKUIUPDATE":
                        case "UNLOCKUIUPDATE":
                            var ret = FWUpdate(commandLineInput);
                            result.ExitCode = ret.code;
                            result.serialize_Json_response = ret.json;
                            return result;
                    }
                }
                CLI_RESPONSE rsp = new CLI_RESPONSE()
                {
                    Command = commandLineInput.Command,
                    Result = "Un-supported command",
                    Message = "Un-supported command",
                    TargetFeature = commandLineInput.TargetFeature
                };
                result.serialize_Json_response = JsonConvert.SerializeObject(rsp, Formatting.Indented);
                result.ExitCode = (int)CLI_ExitCode.empty_command_input;
                return result;
            }
            else
            {
                CLI_RESPONSE rsp = new CLI_RESPONSE()
                {
                    Result = "Empty command input",
                    Message = "Empty command input"
                };
                result.serialize_Json_response = JsonConvert.SerializeObject(rsp, Formatting.Indented);
                result.ExitCode = (int)CLI_ExitCode.empty_command_input;
                return result;
            }
        }

        #endregion interface implementation

        #region Private methods

        private Task<int> ProcessListPeripheralsOptionAsync(IDeviceManagerSA devMgr)
        {
            if (devMgr == null)
            {
                writelog("ProcessListPeripheralsOptionAsync: input null IDeviceManagerSA");
                return Task.FromResult((int)CLI_ExitCode.null_device_manager);
            }
            if (_deviceinfo == null)
                _deviceinfo = devMgr.GetDevices().Result.deviceInfo;

            int index = 0;
            foreach (var g in _deviceinfo)
            {
                Console.WriteLine("[" + index.ToString() + "] : " + g.Name);
                index++;
            }

            return Task.FromResult((int)CLI_ExitCode.success);
        }

        private int GetPeripheralProperty()
        {
            GetResults.Clear();
            if (_devMgr == null)
            {
                writelog("GetPeripheralProperty: input null IDeviceManagerSA");
                return (int)CLI_ExitCode.null_device_manager;
            }

            _deviceinfo = _devMgr.GetDevices().Result.deviceInfo;
            if (_deviceinfo == null || _deviceinfo.Count == 0)
            {
                GetResults.Add(new CLI_PeripheralRESPONSE("N/A", "GET", _commandLineInput.TargetFeature, "Fail", "Device not found", "N/A", "N/A"));
                return (int)CLI_ExitCode.fail_GetPeripheralProperty_NoConnectDevice;
            }
           
            if (_commandLineInput.GuidString.Count == 0)
            {
                _deviceinfo.ForEach(x =>
                {
                    if (x.LogicalDeviceType.ToUpper().Contains(_commandLineInput.PluginsType))
                    {
                        GetResults.Add(new CLI_PeripheralRESPONSE(x, _commandLineInput.PluginsType, _commandLineInput.TargetFeature));
                    }
                });
            }
            else
            {
                _commandLineInput.GuidString.ForEach(x =>
                {
                    if (Guid.TryParse(x, out Guid guid))
                    {
                        var di = _deviceinfo.Where(x => x.ID == guid).FirstOrDefault();
                        if (di == null)
                        {
                            GetResults.Add(new CLI_PeripheralRESPONSE(x, "GET", _commandLineInput.TargetFeature, "Fail", "Device not found"));
                        }
                        else
                        {
                            if (_commandLineInput.TargetFeature.Equals("FWVERSION"))
                                _commandLineInput.TargetFeature = "FIRMWAREVERSION";
                            GetResults.Add(new CLI_PeripheralRESPONSE(di, _commandLineInput.PluginsType, _commandLineInput.TargetFeature));
                        }
                    }
                    else
                    {
                        GetResults.Add(new CLI_PeripheralRESPONSE(x, "GET", _commandLineInput.TargetFeature, "Fail", "Invalid Guid"));
                    }
                });
            }
            return (int)CLI_ExitCode.success;
        }

        private int SetPeripheralProperty()
        {
            SetResults.Clear();
            int val = 0;
            bool bl = false;
            if (_devMgr == null)
            {
                writelog("SetPeripheralProperty: input null IDeviceManagerSA");
                return (int)CLI_ExitCode.null_device_manager;
            }
            _deviceinfo = _devMgr.GetDevices().Result.deviceInfo;
            if (_commandLineInput.GuidString.Count == 0)
            {
                if (_deviceinfo == null || _deviceinfo.Count == 0)
                {
                    SetResults.Add(new CLI_PeripheralRESPONSE("N/A", "SET", _commandLineInput.TargetFeature, "FAIL", "Device not found", "N/A", "N/A"));
                    return (int)CLI_ExitCode.fail_SetPeripheralProperty;
                }
                _deviceinfo.ForEach(x =>
                {
                    if (x.LogicalDeviceType.ToUpper().Contains(_commandLineInput.PluginsType))
                    {
                        SetResults.Add(new CLI_PeripheralRESPONSE($"{{{x.ID}}}", "SET", _commandLineInput.TargetFeature, "", "", $"{x.Name}", $"{x.ModelNumber}"));
                    }
                });
            }
            else
            {
                _commandLineInput.GuidString.ForEach(x =>
                {
                    if (Guid.TryParse(x, out Guid guid))
                    {
                        var found = false;
                        _deviceinfo?.ForEach(x =>
                {
                    if (x.LogicalDeviceType.ToUpper().Contains(_commandLineInput.PluginsType) && x.ID == guid)
                    {
                        SetResults.Add(new CLI_PeripheralRESPONSE($"{{{x.ID}}}", "SET", _commandLineInput.TargetFeature, "", "", $"{x.Name}", $"{x.ModelNumber}"));
                        found = true;
                    }
                });
                        if (!found)
                        {
                            SetResults.Add(new CLI_PeripheralRESPONSE($"{{{guid}}}", "SET", _commandLineInput.TargetFeature, "FAIL", "Device not found"));
                        }
                    }
                    else
                    {
                        SetResults.Add(new CLI_PeripheralRESPONSE(x, "SET", _commandLineInput.TargetFeature, "FAIL", "Invalid Guid"));
                    }
                });
            }
            foreach (var op in _commandLineInput.Options)
            {
                if (op.Option_Name.ToUpper() == "VALUE")
                {
                    value = op.Option_Value;
                    switch (value)
                    {
                        case "ENABLE":
                        case "ON":
                            val = 1;
                            bl = true;
                            break;                            
                        case "DISABLE":
                        case "OFF":
                            val = 0;
                            bl = false;
                            break;
                        default:
                            value = op.Option_Value;
                            break;

                    }
                    break;
                }
            }
            if (_commandLineInput.TargetFeature != "UNPAIR" && value == "")
            {
                SetResults.ForEach(x =>
                        {
                            x.Result = "FAIL";
                            x.Message += (x.Message == "" ? "" : ", ") + "Missing setting value";
                        });
                return (int)CLI_ExitCode.fail_SetPeripheralProperty_Value;
            }

            switch (_commandLineInput.TargetFeature)
            {
                case "BACKLIGHTINGCONTROLS":
                        taskA = _devMgr.SetBackLightingControls;
                        RunTaskA(val);
                        return (int)CLI_ExitCode.success;
                case "BACKLIGHTINGLEVEL":
                        taskA = _devMgr.SetBackLightingLevel;
                        RunTaskA(val);
                        return (int)CLI_ExitCode.success;
                case "COLLABORATIONBLINKEFFECTENABLE":
                        taskB = _devMgr.SetCollaborationBlinkEffectEnable;
                        RunTaskB(bl);
                        return (int)CLI_ExitCode.success;
                case "COLLABORATIONCAMERAENABLE":
                        taskB = _devMgr.SetCollaborationCameraEnable;
                        RunTaskB(bl);
                        return (int)CLI_ExitCode.success;
                case "COLLABORATIONCHATENABLE":
                        taskB = _devMgr.SetCollaborationChatEnable;
                        RunTaskB(bl);
                        return (int)CLI_ExitCode.success;
                case "COLLABORATIONDOUBLETAPENABLE":
                        taskB = _devMgr.SetCollaborationDoubleTapEnable;
                        RunTaskB(bl);
                        return (int)CLI_ExitCode.success;
                case "COLLABORATIONKEYENABLE":
                        taskB = _devMgr.SetCollaborationKeyEnable;
                        RunTaskB(bl);
                        return (int)CLI_ExitCode.success;
                case "COLLABORATIONMICENABLE":
                        taskB = _devMgr.SetCollaborationMicEnable;
                        RunTaskB(bl);
                        return (int)CLI_ExitCode.success;
                case "COLLABORATIONSCREENSHAREENABLE":
                        taskB = _devMgr.SetCollaborationScreenShareEnable;
                        RunTaskB(bl);
                        return (int)CLI_ExitCode.success;
                case "DPILEVEL":
                        taskA = _devMgr.SetDPILevel;
                        RunTaskA(val);
                        return (int)CLI_ExitCode.success;
                case "DPIVALUE":
                        taskA = _devMgr.SetDPIValue;
                        RunTaskA(val);
                        return (int)CLI_ExitCode.success;
                case "PRIMARYMOUSEBUTTON":
                    MouseButton button;
                    switch (value.ToUpper())
                    {
                        case "L":
                            button = MouseButton.Left;
                            break;

                        case "R":
                            button = MouseButton.Right;
                            break;

                        default:
                            SetFailResults("Invalid setting value");
                            return (int)CLI_ExitCode.fail_SetPeripheralProperty_Value;
                    }
                    SetResults.ForEach(x =>
                    {
                        x.Value = value;
                        if (x.Result == "")
                        {
                            var result = RunAsyncTimeout(_devMgr.SetPrimaryMouseButton(button, Guid.Parse(x.Guid))).Result;
                            if (result == "0")
                            {
                                x.Result = "PASS";
                                x.Message = "N/A";
                            }
                            else if (result == "1")
                            {
                                x.Result = "FAIL";
                                x.Message = "Timeout";
                            }
                            else
                            {
                                x.Result = "FAIL";
                                x.Message = result;
                            }
                        }
                    });
                    return (int)CLI_ExitCode.success;

                case "TOUCHSCROLLSENSITIVITYLEVEL":
                        taskA = _devMgr.SetTouchScrollSensitivityLevel;
                        RunTaskA(val);
                        return (int)CLI_ExitCode.success;
                case "UNPAIR":
                    SetResults.ForEach(x =>
                    {
                        x.Value = "";
                        if (x.Result == "")
                        {
                            var result = RunAsyncTimeout(_devMgr.UnPair(Guid.Parse(x.Guid))).Result;
                            if (result == "0")
                            {
                                x.Result = "PASS";
                                x.Message = "N/A";
                            }
                            else if (result == "1")
                            {
                                x.Result = "FAIL";
                                x.Message = "Timeout";
                            }
                            else
                            {
                                x.Result = "FAIL";
                                x.Message = result;
                            }
                        }
                    });
                    return (int)CLI_ExitCode.success;
                //Headset&Speaker
                case "SETWIREDAUDIOIMICNSENABLE":
                        taskB = _devMgr.SetWiredAudioIMicNSEnable;
                        RunTaskB(bl);
                        return (int)CLI_ExitCode.success;
                case "SETWIREDAUDIOMICMUTESOUNDENABLE":
                        taskB = _devMgr.SetWiredAudioMicMuteSoundEnable;
                        RunTaskB(bl);
                        return (int)CLI_ExitCode.success;
                case "SETWIREDAUDIOVOLUMEADJUSTMENTTONE":
                        taskA = _devMgr.SetWiredAudioVolumeAdjustmentTone;
                        RunTaskA(val);
                        return (int)CLI_ExitCode.success;
                case "ANCMODE":
                        taskA = _devMgr.SetAncMode;
                        RunTaskA(val);
                        return (int)CLI_ExitCode.success;
                case "SETANCGAIN":
                        taskA = _devMgr.SetAncGain;
                        RunTaskA(val);
                        return (int)CLI_ExitCode.success;
                case "SETSELECTEDPRESET":
                        taskA = _devMgr.SetSelectedPreset;
                        RunTaskA(val);
                        return (int)CLI_ExitCode.success;
                //case "SETBANDSGAIN":
                //    if (!int.TryParse(value, out val))
                //    {
                //        taskC = _devMgr.SetBandsGain;
                //        RunTaskC(val);
                //        return (int)CLI_ExitCode.success;
                //    }
                //    else
                //    {
                //        SetFailResults("invalid setting value");
                //        return (int)CLI_ExitCode.fail_SetPeripheralProperty_Value;
                //    }
                case "MICNOISECANCELLATION":
                        taskB = _devMgr.SetMicNoiseCancellation;
                        RunTaskB(bl);
                        return (int)CLI_ExitCode.success;
                case "SETSIDETONE":
                        taskB = _devMgr.SetSidetone;
                        RunTaskB(bl);
                        return (int)CLI_ExitCode.success;
                case "SETSIDETONELEVEL":
                        taskA = _devMgr.SetSidetoneLevel;
                        RunTaskA(val);
                        return (int)CLI_ExitCode.success;
                case "WEARDETECTION":
                        taskA = _devMgr.SetWearDetection;
                        RunTaskA(val);
                        return (int)CLI_ExitCode.success;
                case "SETBUSYLIGHT":
                        taskB = _devMgr.SetBusyLight;
                        RunTaskB(bl);
                        return (int)CLI_ExitCode.success;
                case "SETVOICEGUIDANCE":
                        taskB = _devMgr.SetVoiceGuidance;
                        RunTaskB(bl);
                        return (int)CLI_ExitCode.success;
                case "SETMICNCINCOMING":
                        taskB = _devMgr.SetMicNCIncoming;
                        RunTaskB(bl);
                        return (int)CLI_ExitCode.success;
                //case "SETEQUALIZERVALUES":
                //    if (!bool.TryParse(value, out bl))
                //    {
                //        taskB = _devMgr.SetEqualizerValues;
                //        RunTaskB(bl);
                //        return (int)CLI_ExitCode.success;
                //    }
                //    else
                //    {
                //        SetFailResults("Invalid setting value");
                //        return (int)CLI_ExitCode.fail_SetPeripheralProperty_Value;
                //    }
                case "SETISMICENUMERATIONON":
                        taskB = _devMgr.SetIsMicEnumerationOn;
                        RunTaskB(bl);
                        return (int)CLI_ExitCode.success;
                default:
                    SetFailResults("Invalid TargetFeature");
                    return (int)CLI_ExitCode.fail_SetPeripheralProperty_Property;
            }
        }

        private void RunTaskA(int val)
        {
            SetResults.ForEach(x =>
            {
                x.Value = value;
                if (x.Result == "")
                {
                    var result = RunAsyncTimeout(taskA(val, Guid.Parse(x.Guid))).Result;
                    if (result == "0")
                    {
                        x.Result = "PASS";
                        x.Message = "N/A";
                    }
                    else if (result == "1")
                    {
                        x.Result = "FAIL";
                        x.Message = "Timeout";
                    }
                    else
                    {
                        x.Result = "FAIL";
                        x.Message = result;
                    }
                }
            });
        }

        private void RunTaskB(bool val)
        {
            SetResults.ForEach(x =>
            {
                x.Value = value;
                if (x.Result == "")
                {
                    var result = RunAsyncTimeout(taskB(val, Guid.Parse(x.Guid))).Result;
                    if (result == "0")
                    {
                        x.Result = "PASS";
                        x.Message = "N/A";
                    }
                    else if (result == "1")
                    {
                        x.Result = "FAIL";
                        x.Message = "Timeout";
                    }
                    else
                    {
                        x.Result = "FAIL";
                        x.Message = result;
                    }
                }
            });
        }

        private void RunTaskC(int val, string str)
        {
            SetResults.ForEach(x =>
            {
                x.Value = value;
                if (x.Result == "")
                {
                    var result = RunAsyncTimeout(taskC(val, Guid.Parse(x.Guid), str)).Result;
                    if (result == "0")
                    {
                        x.Result = "PASS";
                        x.Message = "N/A";
                    }
                    else if (result == "1")
                    {
                        x.Result = "FAIL";
                        x.Message = "timeout";
                    }
                    else
                    {
                        x.Result = "FAIL";
                        x.Message = result;
                    }
                }
            });
        }

        private void SetFailResults(string message)
        {
            SetResults.ForEach(x =>
            {
                x.Value = value.ToString();
                x.Result = "FAIL";
                x.Message += (x.Message == "" ? "" : ", ") + message;
            });
        }

        private async Task<string> RunAsyncTimeout(Task task)
        {
            try
            {
                Task completedTask = await Task.WhenAny(task, Task.Delay(TimeSpan.FromSeconds(5)));

                if (completedTask == task)
                {
                    await task;
                    return "0";
                }
                else
                {
                    return "1";
                }
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        /// <summary>
        /// //
        /// </summary>
        /// <param name="text"></param>
        /// <param name="log_type">0 means info, others means error</param>
        private void writelog(string? text, log_type log_type = log_type.info)
        {
            text = "[CLI Plugin Peripherals] " + text;
            //Console.WriteLine(text);
            if (log_type == log_type.info)
                Log.Info(text);
            else
                Log.Error(text);
        }

        #endregion Private methods

        #region IDisposableObservable Support

        /// <summary>
        /// To detect redundant calls
        /// </summary>
        public bool IsDisposed { get; private set; }

        /// <summary>
        /// Override for Dispose
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            writelog($"Dispose: {disposing}");
            if (!IsDisposed)
            {
                //if (disposing)//Dean 0626 fix SAST issue
                //{
                //    _agent = null;
                //}

                IsDisposed = true;
            }
            base.Dispose(disposing);
        }

        #endregion IDisposableObservable Support

        #region FW Update

        private (int code, string json) FWUpdate(CommandLineInput commandLineInput)
        {
            string output = string.Empty;
            List<List<string>> report = new List<List<string>>();
            List<string> tmpReport = new List<string>();
            CLI_RESPONSE cLI_RESPONSE = new CLI_RESPONSE();
            cLI_RESPONSE.Command = commandLineInput.Command;
            cLI_RESPONSE.TargetFeature = commandLineInput.TargetFeature;

            if (!commandLineInput.isCliRunAdmin)
            {
                cLI_RESPONSE.Result = "FAIL";
                cLI_RESPONSE.Message = "Not Admin";
                System.Console.WriteLine(JsonConvert.SerializeObject(cLI_RESPONSE, Formatting.Indented));
                return ((int)CLI_ExitCode.fail_FWUpdate, JsonConvert.SerializeObject(cLI_RESPONSE, Formatting.Indented));
            }
            bool? ret = null;
            bool isShowInfo = true;
            string installPath = "";
            bool somethingError = false;
            CLI_FWU_RESPONSE cLI_FWU_RESPONSE = null;
            switch (commandLineInput.TargetFeature)
            {
                case "FWUPDATE":
                    cLI_FWU_RESPONSE = new CLI_FWU_RESPONSE(cLI_RESPONSE);
                    if (commandLineInput.Options.Count > 3)
                    {
                        cLI_FWU_RESPONSE.Result = "FAIL";
                        cLI_FWU_RESPONSE.Message = "Bring in extra strings:";
                        for (int i = 0; i < commandLineInput.Options.Count; i++)
                        {
                            cLI_FWU_RESPONSE.Message += $"{commandLineInput.Options[i].Option_Name}={commandLineInput.Options[i].Option_Value}\n";
                        }
                        break;
                    }
                    if (commandLineInput.Options.Count > 0)
                    {
                        cLI_FWU_RESPONSE.Value = commandLineInput.Options[0].Option_Value;
                        for (int i = 1; i < commandLineInput.Options.Count; i++)
                        {
                            if (commandLineInput.Options[i].Option_Name.ToUpper().Equals("ISSHOWINFO"))
                            {
                                isShowInfo = commandLineInput.Options[i].Option_Value.ToUpper() == "ON" ? true : false;
                            }
                            else if (commandLineInput.Options[i].Option_Name.ToUpper().Equals("INSTALLPATH"))
                            {
                                installPath = Path.GetFullPath(commandLineInput.Options[i].Option_Value);
                            }
                            else
                            {
                                somethingError = true;
                            }
                        }
                        if (somethingError)
                        {
                            cLI_FWU_RESPONSE.Result = "FAIL";
                            cLI_FWU_RESPONSE.Message = "Bring in extra strings:";
                            for (int i = 0; i < commandLineInput.Options.Count; i++)
                            {
                                cLI_FWU_RESPONSE.Message += $"{commandLineInput.Options[i].Option_Name}={commandLineInput.Options[i].Option_Value}\n";
                            }
                            break;
                        }
                        if (commandLineInput.Options[0].Option_Value.ToUpper().Equals("DEFER"))
                        {
                            if (commandLineInput.Options.Count > 2)
                            {
                                cLI_FWU_RESPONSE.Result = "FAIL";
                                cLI_FWU_RESPONSE.Message = "Bring in extra strings:";
                                for (int i = 0; i < commandLineInput.Options.Count; i++)
                                {
                                    cLI_FWU_RESPONSE.Message += $"{commandLineInput.Options[i].Option_Name}={commandLineInput.Options[i].Option_Value}\n";
                                }
                                break;
                            }
                            ret = GetFWUpdateList(commandLineInput, cLI_FWU_RESPONSE, isShowInfo, true);
                        }
                        else if (commandLineInput.Options[0].Option_Value.ToUpper().Equals("FORCE"))
                        {
                            if (commandLineInput.Options.Count > 3)
                            {
                                cLI_FWU_RESPONSE.Result = "FAIL";
                                cLI_FWU_RESPONSE.Message = "Input FAIL.\n";
                                cLI_FWU_RESPONSE.Message += $"{commandLineInput.Options[1].Option_Name}={commandLineInput.Options[1].Option_Value}\n";
                                break;
                            }
                            ret = Auto_FWUpdate(commandLineInput, cLI_FWU_RESPONSE, false, installPath, isShowInfo, true);
                        }
                        else
                        {
                            cLI_FWU_RESPONSE.Message = "Input FAIL";
                            cLI_FWU_RESPONSE.Message = "Input FAIL. should get: Value=FORCE/DEFER\n";
                            cLI_FWU_RESPONSE.Message += $"{commandLineInput.Options[0].Option_Name}={commandLineInput.Options[0].Option_Value}\n";
                            ret = false;
                        }
                    }
                    else
                    {
                        cLI_FWU_RESPONSE.Message = "Input FAIL";
                        ret = false;
                    }
                    cLI_FWU_RESPONSE.Result = ret == true ? "PASS" : "FAIL";
                    break;

                case "UODFWUPDATE":
                    cLI_FWU_RESPONSE = new CLI_FWU_RESPONSE(cLI_RESPONSE);
                    if (commandLineInput.Options.Count > 2)
                    {
                        cLI_FWU_RESPONSE.Result = "FAIL";
                        cLI_FWU_RESPONSE.Message = "Bring in extra strings:";
                        for (int i = 0; i < commandLineInput.Options.Count; i++)
                        {
                            cLI_FWU_RESPONSE.Message += $"{commandLineInput.Options[i].Option_Name}={commandLineInput.Options[i].Option_Value}\n";
                        }
                        break;
                    }
                    for (int i = 1; i < commandLineInput.Options.Count; i++)
                    {
                        if (commandLineInput.Options[i].Option_Name.ToUpper().Equals("ISSHOWINFO"))
                        {
                            isShowInfo = commandLineInput.Options[i].Option_Value.ToUpper() == "ON" ? true : false;
                        }
                        else if (commandLineInput.Options[i].Option_Name.ToUpper().Equals("INSTALLPATH"))
                        {
                            installPath = Path.GetFullPath(commandLineInput.Options[i].Option_Value);
                        }
                        else
                        {
                            somethingError = true;
                        }
                    }
                    if (somethingError)
                    {
                        cLI_FWU_RESPONSE.Result = "FAIL";
                        cLI_FWU_RESPONSE.Message = "Bring in extra strings:";
                        for (int i = 0; i < commandLineInput.Options.Count; i++)
                        {
                            cLI_FWU_RESPONSE.Message += $"{commandLineInput.Options[i].Option_Name}={commandLineInput.Options[i].Option_Value}\n";
                        }
                        break;
                    }
                    ret = Auto_FWUpdate(commandLineInput, cLI_FWU_RESPONSE, true, installPath, isShowInfo, true);
                    cLI_FWU_RESPONSE.Result = ret == true ? "PASS" : "FAIL";
                    break;

                case "LOCKUIUPDATE":
                    if (commandLineInput.Options.Count > 0)
                    {
                        cLI_FWU_RESPONSE.Result = "FAIL";
                        cLI_FWU_RESPONSE.Message = "Bring in extra strings:";
                        for (int i = 0; i < commandLineInput.Options.Count; i++)
                        {
                            cLI_FWU_RESPONSE.Message += $"{commandLineInput.Options[i].Option_Name}={commandLineInput.Options[i].Option_Value}\n";
                        }
                        break;
                    }
                    _devMgr.SetUILockStatus(true);
                    ret = true;
                    break;

                case "UNLOCKUIUPDATE":
                    if (commandLineInput.Options.Count > 0)
                    {
                        cLI_FWU_RESPONSE.Result = "FAIL";
                        cLI_FWU_RESPONSE.Message = "Bring in extra strings:";
                        for (int i = 0; i < commandLineInput.Options.Count; i++)
                        {
                            cLI_FWU_RESPONSE.Message += $"{commandLineInput.Options[i].Option_Name}={commandLineInput.Options[i].Option_Value}\n";
                        }
                        break;
                    }
                    _devMgr.SetUILockStatus(false);
                    ret = true;
                    break;

                default:
                    cLI_FWU_RESPONSE.Message = "Input FAIL";
                    ret = false;
                    break;
            }
            cLI_RESPONSE.Result = ret == true ? "PASS" : "FAIL";
            if (cLI_FWU_RESPONSE != null)
            {
                output = cLI_FWU_RESPONSE.OutputLog(cLI_FWU_RESPONSE, commandLineInput);
            }
            else
            {
                output = cLI_RESPONSE.OutputLog(cLI_RESPONSE, commandLineInput);
            }
            if (ret == true)
            {
                return ((int)CLI_ExitCode.success, output);
            }
            else
            {
                return ((int)CLI_ExitCode.fail_FWUpdate, output);
            }
        }

        private bool? GetFWUpdateList(CommandLineInput commandLineInput, CLI_FWU_RESPONSE cli_FWU_RESPONSE, bool isShowInfo = true, bool isDefer = false)
        {
            List<DeviceType> deviceType = new List<DeviceType>();
            switch (commandLineInput.PluginsType)
            {
                case "KEYBOARD":
                    deviceType.Add(DeviceType.LogicalKeyboard);
                    deviceType.Add(DeviceType.PhysicalDongle);
                    break;

                case "MOUSE":
                    deviceType.Add(DeviceType.LogicalMouse);
                    deviceType.Add(DeviceType.PhysicalDongle);
                    break;

                case "DOCK":
                    deviceType.Add(DeviceType.LogicalDock);
                    deviceType.Add(DeviceType.PhysicalWiredDock);
                    break;
            }
            List<string> tmpReport = new List<string>();
            FWUpdateInfoPackage fwUpdateInfoPackage = _devMgr.GetFWUpdateInfo(isShowInfo, false, isDefer, deviceType).Result;
            if (fwUpdateInfoPackage.FWUpdateInfo.Count > 0)
            {
                int index = 1;
                foreach (FWUpdateInfo fwUpdateInfo in fwUpdateInfoPackage.FWUpdateInfo)
                {
                    cli_FWU_RESPONSE.Model = fwUpdateInfo.Model;
                    cli_FWU_RESPONSE.GUID.Add(fwUpdateInfo.DeviceId);
                    cli_FWU_RESPONSE.FWUpdateRESPONSE.Add($"{index++} Firmware update {fwUpdateInfo.TheLatestVersion} - {fwUpdateInfo.DeviceName}");
                }
                return true;
            }
            else
            {
                cli_FWU_RESPONSE.Message = "No updates available";
                return null;
            }
        }

        //0531 Bruce 因應IL的現有安裝包修改判斷，CLIPeripheralsPlugins.cs中FWUpdate方法修改回傳值型態和新增判斷
        private bool FWUpdate(string s)
        {
            try
            {
                int index = int.Parse(s);
                FWUpdateInfoPackage fwUpdateInfoPackage = _devMgr.GetFWUpdateInfo().Result;
                List<FWUpdateInfo> fwUpdateInfo = new List<FWUpdateInfo>
            {
                fwUpdateInfoPackage.FWUpdateInfo[index - 1]
            };
                Console.WriteLine($"Ready to start updating Device:{fwUpdateInfo[0].DeviceName} to Version:{fwUpdateInfo[0].TheLatestVersion}");
                Console.WriteLine($"To continue, press C, to cancel, press any key.");
                string rs = Console.ReadLine();
                if (!rs.ToUpper().Equals("C"))
                {
                    Console.WriteLine($"Cancel update.");
                    return true;
                }
                else
                {
                    _devMgr.ProgressUpdate_Notify -= _FWUpdatePlugin_ProgressUpdate;
                    _devMgr.ProgressUpdate_Notify += _FWUpdatePlugin_ProgressUpdate;
                    List<FWUpdateInfo> retFWUpdateInfos = _devMgr.DownloadAndInstall(fwUpdateInfo).Result;
                    bool b = true;
                    foreach (FWUpdateInfo retFWUpdateInfo in retFWUpdateInfos)
                    {
                        if (retFWUpdateInfo.FWUErrorCode == FWUErrorCode.NoError)
                        {
                            Console.WriteLine($"{retFWUpdateInfo.DeviceName} update success.");
                        }
                        else
                        {
                            Console.WriteLine($"{retFWUpdateInfo.DeviceName} update fail.");
                        }
                    }
                    return b;
                }
            }
            catch
            {
                return false;
            }
        }

        //0531 Bruce 因應IL的現有安裝包修改判斷，CLIPeripheralsPlugins.cs中Auto_FWUpdate方法修改回傳值型態和新增判斷
        private List<FWUpdateInfo> retFWUpdateInfos;

        private bool? Auto_FWUpdate(CommandLineInput commandLineInput, CLI_FWU_RESPONSE cli_FWU_RESPONSE, bool isUODMode, string installPath, bool isShowInfo = true, bool isForce = false)
        {
            try
            {
                List<DeviceType> deviceTypes = new List<DeviceType>();
                DeviceType deviceType = DeviceType.Unknown;
                switch (commandLineInput.PluginsType)
                {
                    case "KEYBOARD":
                        deviceTypes.Add(DeviceType.LogicalKeyboard);
                        deviceTypes.Add(DeviceType.PhysicalDongle);
                        deviceType = DeviceType.LogicalKeyboard;
                        break;

                    case "MOUSE":
                        deviceTypes.Add(DeviceType.LogicalMouse);
                        deviceTypes.Add(DeviceType.PhysicalDongle);
                        deviceType = DeviceType.LogicalMouse;
                        break;

                    case "DOCK":
                        deviceTypes.Add(DeviceType.LogicalDock);
                        deviceTypes.Add(DeviceType.PhysicalWiredDock);
                        deviceType = DeviceType.LogicalDock;
                        break;

                    default:
                        deviceType = DeviceType.Unknown;
                        break;
                }
                if (isUODMode && deviceType != DeviceType.LogicalDock)
                {
                    cli_FWU_RESPONSE.Message = "Only dock supports UOD update mode.";
                    return null;
                }
                _devMgr.ProgressUpdate_Notify -= _FWUpdatePlugin_ProgressUpdate;
                _devMgr.ProgressUpdate_Notify += _FWUpdatePlugin_ProgressUpdate;
                _devMgr.DownloadAndInstall_Result_Notify -= Download_Event;
                _devMgr.DownloadAndInstall_Result_Notify += Download_Event;
                FWUpdateInfoPackage fwUpdateInfoPackage = _devMgr.GetFWUpdateInfo(isShowInfo, isForce, false, deviceTypes, isUODMode).Result;
                if (fwUpdateInfoPackage.FWUpdateInfo.Count <= 0)
                {
                    cli_FWU_RESPONSE.Message = "No updates available";
                    return null;
                }
                if (deviceType != DeviceType.Unknown)
                {
                    foreach (FWUpdateInfo fwUpdateInfo in fwUpdateInfoPackage.FWUpdateInfo)
                    {
                        cli_FWU_RESPONSE.Model = fwUpdateInfo.Model;
                        cli_FWU_RESPONSE.GUID.Add(fwUpdateInfo.DeviceId);
                        cli_FWU_RESPONSE.FWUpdateRESPONSE.Add($"Ready to start updating Device:{fwUpdateInfo.DeviceName} to Version:{fwUpdateInfo.TheLatestVersion}");
                    }
                    cli_FWU_RESPONSE.OutputLog(cli_FWU_RESPONSE, commandLineInput);
                    cli_FWU_RESPONSE.FWUpdateRESPONSE.Clear();
                    do
                    {
                        Thread.Sleep(100);
                    } while (retFWUpdateInfos == null);
                    bool b = true;
                    foreach (FWUpdateInfo retFWUpdateInfo in retFWUpdateInfos)
                    {
                        cli_FWU_RESPONSE.Model = retFWUpdateInfo.Model;
                        if (retFWUpdateInfo.FWUErrorCode == FWUErrorCode.NoError)
                        {
                            cli_FWU_RESPONSE.FWUpdateRESPONSE.Add($"{retFWUpdateInfo.DeviceName} update success.");
                        }
                        else
                        {
                            cli_FWU_RESPONSE.FWUpdateRESPONSE.Add($"{retFWUpdateInfo.DeviceName} update fail. Fail message:{retFWUpdateInfo.FWUErrorCode.ToString()}");
                            b = false;
                        }
                    }
                    _devMgr.ProgressUpdate_Notify -= _FWUpdatePlugin_ProgressUpdate;
                    _devMgr.DownloadAndInstall_Result_Notify -= Download_Event;
                    return b;
                }
                else
                {
                    return false;
                }
            }
            catch
            {
                return false;
            }
        }

        private void Download_Event(object o, List<FWUpdateInfo> e)
        {
            retFWUpdateInfos = e;
        }

        private bool isDownload, isInstalling;

        private void _FWUpdatePlugin_ProgressUpdate(object sender, FWUpdateInfo e)
        {
            if (e.ProcessName.Equals("Installing"))
            {
                if (!isInstalling)
                {
                    Console.WriteLine($"DeviceName:{e.DeviceName} to Version:{e.TheLatestVersion}, {e.ProcessName}");
                    isInstalling = true;
                }
            }
            else if (e.ProcessName.Equals("Downloading"))
            {
                if (!isDownload)
                {
                    Console.WriteLine($"DeviceName:{e.DeviceName} to Version:{e.TheLatestVersion}, {e.ProcessName}");
                    isDownload = true;
                }
            }
            else
            {
                Console.WriteLine($"DeviceName:{e.DeviceName} to Version:{e.TheLatestVersion}, {e.ProcessName}");
            }
        }

        #endregion FW Update
    }
}