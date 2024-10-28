using DDPM.SA.Common;
using DDPM.SA.Common.Settings;
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
using VcpCore.Common;
using static DDPM.SA.Common.ICLICommandTable;
using Console = System.Console;

namespace DDPM.CLI.Plugins.Peripherals
{
    [Plugin(DDPM.SA.Common.IDs.CLI_Plugin_Peripherals, pluginName, PluginOrderGroupType.Core, Version = pluginVersion)]
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

        bool _recode_head = false;
        bool _recode_speak = false;

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
                {

                    List<DeviceInfo> _deviceinfo = null;
                    _deviceinfo = _devMgr.GetDevices().Result.deviceInfo;
                    foreach (var g in _deviceinfo)
                    {
                        if (g.LogicalDeviceType == "LogicalHeadset")
                        {
                            _commandLineInput.PluginsType = "HEADSET";
                            _recode_head = true;
                        }
                        if (g.LogicalDeviceType == "LogicalWiredAudio")
                        {
                            _commandLineInput.PluginsType = "LOGICALWIREDAUDIO";
                            _recode_speak = true;
                        }

                    }

                }

                if (commandLineInput.Command.Equals("SET"))
                {
                    if (commandLineInput.TargetType.Equals("APP"))
                    {
                        if (commandLineInput.TargetFeature.Equals("FIRMWAREUPDATE") || commandLineInput.TargetFeature.Equals("UODFWUPDATE") || commandLineInput.TargetFeature.Equals("LOCKUIUPDATE") || commandLineInput.TargetFeature.Equals("UNLOCKUIUPDATE"))// for firmware update.
                        {
                            switch (commandLineInput.TargetFeature)
                            {
                                case "FIRMWAREUPDATE":
                                case "UODFWUPDATE":
                                case "LOCKUIUPDATE":
                                case "UNLOCKUIUPDATE":
                                    var ret = FWUpdate(commandLineInput);
                                    result.ExitCode = ret.code;
                                    result.serialize_Json_response = ret.json;
                                    return result;
                            }
                        }
                    }

                }
                if (commandLineInput.Command.Equals("SET"))
                {
                    if (commandLineInput.TargetType.Equals("DOCK"))
                    {
                        if (commandLineInput.TargetFeature.Equals("SILENTFWUPDATE"))// for dock firmware update.
                        {
                            //switch (commandLineInput.TargetFeature)
                            //{
                            //    case "FIRMWAREUPDATE":
                            //    case "UODFWUPDATE":
                            //    case "LOCKUIUPDATE":
                            //    case "UNLOCKUIUPDATE":
                            var ret = FWUpdate(commandLineInput);
                            result.ExitCode = ret.code;
                            result.serialize_Json_response = ret.json;
                            return result;
                            //}
                        }
                    }

                }

                if (commandLineInput.Command.Equals("SET"))
                {
                    if (commandLineInput.TargetType.Equals("APP"))
                    {
                        if (commandLineInput.TargetFeature.Equals("UPDATE"))// for dock firmware update.
                        {
                            //switch (commandLineInput.TargetFeature)
                            //{
                            //    case "FIRMWAREUPDATE":
                            //    case "UODFWUPDATE":
                            //    case "LOCKUIUPDATE":
                            //    case "UNLOCKUIUPDATE":
                            var ret = SWAPPUpdate(commandLineInput);
                            result.ExitCode = ret.code;
                            result.serialize_Json_response = ret.json;
                            return result;
                            //}
                        }
                    }

                }
                else if (commandLineInput.Command.Equals("GET"))
                {
                    if (commandLineInput.TargetType.Equals("APP"))
                    {
                        if (commandLineInput.TargetFeature.Equals("UPDATE"))// for dock firmware update.
                        {
                            //switch (commandLineInput.TargetFeature)
                            //{
                            //    case "FIRMWAREUPDATE":
                            //    case "UODFWUPDATE":
                            //    case "LOCKUIUPDATE":
                            //    case "UNLOCKUIUPDATE":
                            var ret = SWAPPUpdate_get(commandLineInput);
                            result.ExitCode = ret.code;
                            result.serialize_Json_response = ret.json;
                            return result;
                            //}
                        }
                    }
                }
                if (commandLineInput.Command.Equals("GET"))
                {
                    if (commandLineInput.TargetType.Equals("APP"))
                    {
                        if (commandLineInput.TargetFeature.Equals("UPDATESOURCELOCATION"))// for dock firmware update.
                        {
                            //switch (commandLineInput.TargetFeature)
                            //{
                            //    case "FIRMWAREUPDATE":
                            //    case "UODFWUPDATE":
                            //    case "LOCKUIUPDATE":
                            //    case "UNLOCKUIUPDATE":
                            var ret = SWAPPUpdate_get(commandLineInput);
                            result.ExitCode = ret.code;
                            result.serialize_Json_response = ret.json;
                            return result;
                            //}
                        }
                    }
                }
                if (commandLineInput.Command.Equals("SET"))
                {
                    if (commandLineInput.TargetType.Equals("APP"))
                    {
                        if (commandLineInput.TargetFeature.Equals("UPDATESOURCELOCATION"))// for dock firmware update.
                        {
                            //switch (commandLineInput.TargetFeature)
                            //{
                            //    case "FIRMWAREUPDATE":
                            //    case "UODFWUPDATE":
                            //    case "LOCKUIUPDATE":
                            //    case "UNLOCKUIUPDATE":
                            var ret = SWAPPUpdate(commandLineInput);
                            result.ExitCode = ret.code;
                            result.serialize_Json_response = ret.json;
                            return result;
                            //}
                        }
                    }

                }
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
                int go = 0;
                Debug.WriteLine($"{_commandLineInput.PluginsType}");
                _deviceinfo.ForEach(x =>
                {
                    if (x.LogicalDeviceType.ToUpper().Contains(_commandLineInput.PluginsType))
                    {
                        //if (_commandLineInput.TargetFeature == "HDR" || _commandLineInput.TargetFeature == "ANTIFLICKER" || _commandLineInput.TargetFeature == "AIAUTOFRAMING")
                        GetResults.Add(new CLI_PeripheralRESPONSE(_devMgr, x, _commandLineInput.PluginsType, _commandLineInput.TargetFeature));
                        //else
                        //GetResults.Add(new CLI_PeripheralRESPONSE(x, _commandLineInput.PluginsType, _commandLineInput.TargetFeature));
                        go++;
                    }
                });
                if (go == 0)
                {
                    GetResults.Add(new CLI_PeripheralRESPONSE("N/A", "GET", _commandLineInput.TargetFeature, "Fail", "Device not found"));
                }
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
                            GetResults.Add(new CLI_PeripheralRESPONSE(_devMgr, di, _commandLineInput.PluginsType, _commandLineInput.TargetFeature));
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
            int retvalue = 0;
            bool bl = false;
            //string ItemId = string.Empty;
            string GUID = string.Empty;
            bool retcode = false;
            bool retcode_ = false;
            //bool target = false;
            DDPMSettings data = _devMgr.ReloadAppConfigData().Result;

            if (_devMgr == null)
            {
                writelog("SetPeripheralProperty: input null IDeviceManagerSA");
                return (int)CLI_ExitCode.null_device_manager;
            }
            _deviceinfo = _devMgr.GetDevices().Result.deviceInfo;
            if (_commandLineInput.GuidString.Count == 0)
            {
                int go = 0;
                if (_deviceinfo == null || _deviceinfo.Count == 0)
                {
                    SetResults.Add(new CLI_PeripheralRESPONSE("N/A", _commandLineInput.Command, _commandLineInput.TargetFeature, "FAIL", "Device not found", "N/A", "N/A"));
                    return (int)CLI_ExitCode.fail_SetPeripheralProperty;
                }
                _deviceinfo.ForEach(x =>
                {
                    if (x.LogicalDeviceType.ToUpper().Contains(_commandLineInput.PluginsType))
                    {
                        SetResults.Add(new CLI_PeripheralRESPONSE($"{{{x.ID}}}", _commandLineInput.Command, _commandLineInput.TargetFeature, "", "", $"{x.Name}", $"{x.ModelNumber}"));
                        GUID = x.ID.ToString();
                        go++;
                    }
                });
                if (go == 0)
                {
                    SetResults.Add(new CLI_PeripheralRESPONSE("N/A", "SET", _commandLineInput.TargetFeature, "Fail", "Device not found"));
                    return (int)CLI_ExitCode.fail_GetPeripheralProperty;
                }
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
                        SetResults.Add(new CLI_PeripheralRESPONSE($"{{{x.ID}}}", _commandLineInput.Command, _commandLineInput.TargetFeature, "", "", $"{x.Name}", $"{x.ModelNumber}"));
                        found = true;
                        GUID = x.ID.ToString();
                    }
                });
                        if (!found)
                        {
                            SetResults.Add(new CLI_PeripheralRESPONSE($"{{{guid}}}", _commandLineInput.Command, _commandLineInput.TargetFeature, "FAIL", "Device not found"));
                        }
                    }
                    else
                    {
                        SetResults.Add(new CLI_PeripheralRESPONSE(x, _commandLineInput.Command, _commandLineInput.TargetFeature, "FAIL", "Invalid Guid"));
                    }
                });
            }

            if (_commandLineInput.TargetFeature.Equals("RESTOREFACTORYDEFAULTS") && _commandLineInput.TargetType.Equals("AUDIO"))// for audio headset RESTOREFACTORYDEFAULTS.
            {
                if (_commandLineInput.PluginsType.Equals("HEADSET"))
                {
                    SetResults.ForEach(x =>
                    {
                        x.Value = "";
                        if (x.Result == "")
                        {
                            var result = RunAsyncTimeout(_devMgr.SetFactoryResetAsyncValueForHeadset(GUID, true)).Result;
                            if (result == "0")
                            {
                                x.Result = "PASS";
                                //retcode_ = _devMgr.GetIsAutoFramingOn(ItemId).Result;
                                x.Value = "SUCCESS";
                                //x.Value += "," + (data.LockSettings.Lock_Audio_RestoreFactoryDefaults ? "LOCK" : "UNLOCK");
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
                            retcode = (result == "0") ? true : false;
                        }
                    });
                }

                if (_commandLineInput.PluginsType.Equals("LOGICALWIREDAUDIO"))
                {
                    SetResults.ForEach(x =>
                    {
                        x.Value = "";
                        if (x.Result == "")
                        {
                            var result = RunAsyncTimeout(_devMgr.SetResetToDefaultAsyncForSoundbar(GUID, true)).Result;
                            if (result == "0")
                            {
                                x.Result = "PASS";
                                //retcode_ = _devMgr.GetIsAutoFramingOn(ItemId).Result;
                                x.Value = "SUCCESS";
                                //x.Value += "," + (data.LockSettings.Lock_Audio_RestoreFactoryDefaults ? "LOCK" : "UNLOCK");
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
                            retcode = (result == "0") ? true : false;
                        }
                    });
                }

                if (_recode_speak && _recode_head)
                {
                    SetResults.ForEach(x =>
                    {
                        x.Value = "";
                        if (x.Result == "")
                        {
                            var result = RunAsyncTimeout(_devMgr.SetResetToDefaultAsyncForSoundbar(GUID, true)).Result;
                            if (result == "0")
                            {
                                x.Result = "PASS";
                                //retcode_ = _devMgr.GetIsAutoFramingOn(ItemId).Result;
                                x.Value = "SUCCESS";
                                //x.Value += "," + (data.LockSettings.Lock_Audio_RestoreFactoryDefaults ? "LOCK" : "UNLOCK");
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
                            retcode = (result == "0") ? true : false;
                        }
                    });
                    SetResults.ForEach(x =>
                    {
                        x.Value = "";
                        if (x.Result == "")
                        {
                            var result = RunAsyncTimeout(_devMgr.SetFactoryResetAsyncValueForHeadset(GUID, true)).Result;
                            if (result == "0")
                            {
                                x.Result = "PASS";
                                //retcode_ = _devMgr.GetIsAutoFramingOn(ItemId).Result;
                                x.Value = "SUCCESS";
                                //x.Value += "," + (data.LockSettings.Lock_Audio_RestoreFactoryDefaults ? "LOCK" : "UNLOCK");
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
                            retcode = (result == "0") ? true : false;
                        }
                    });
                }

                return (retcode) ? (int)CLI_ExitCode.success : (int)CLI_ExitCode.functional_error;
            }

            if (_commandLineInput.TargetFeature.Equals("RESTOREFACTORYDEFAULTS") && _commandLineInput.TargetType.Equals("WEBCAM"))// for audio headset RESTOREFACTORYDEFAULTS.
            {
                SetResults.ForEach(x =>
                {
                    x.Value = "";
                    if (x.Result == "")
                    {
                        var result = RunAsyncTimeout(_devMgr.ResetToDefault_webcam(GUID, true)).Result;
                        if (result == "0")
                        {
                            x.Result = "PASS";
                            //retcode_ = _devMgr.GetIsAutoFramingOn(ItemId).Result;
                            x.Value = "SUCCESS";
                            //x.Value += "," + (data.LockSettings.Lock_Audio_RestoreFactoryDefaults ? "LOCK" : "UNLOCK");
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
                        retcode = (result == "0") ? true : false;
                    }
                });
                return (retcode) ? (int)CLI_ExitCode.success : (int)CLI_ExitCode.functional_error;
            }

            if (_commandLineInput.TargetFeature.Equals("RESTOREFACTORYDEFAULTS") && _commandLineInput.TargetType.Equals("KEYBOARD"))
            {
                SetResults.ForEach(x =>
                {
                    x.Value = "";
                    if (x.Result == "")
                    {
                        var result = "0"; //RunAsyncTimeout(_devMgr.(GUID, true)).Result;
                        if (result == "0")
                        {
                            x.Result = "PASS";
                            x.Value = "SUCCESS";
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
                        retcode = (result == "0") ? true : false;
                    }
                });
                return (retcode) ? (int)CLI_ExitCode.success : (int)CLI_ExitCode.functional_error;
            }

            if (_commandLineInput.TargetFeature.Equals("RESTOREFACTORYDEFAULTS") && _commandLineInput.TargetType.Equals("MOUSE"))
            {
                SetResults.ForEach(x =>
                {
                    x.Value = "";
                    if (x.Result == "")
                    {
                        var result = "0"; //RunAsyncTimeout(_devMgr.(GUID, true)).Result;
                        if (result == "0")
                        {
                            x.Result = "PASS";
                            x.Value = "SUCCESS";
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
                        retcode = (result == "0") ? true : false;
                    }
                });
                return (retcode) ? (int)CLI_ExitCode.success : (int)CLI_ExitCode.functional_error;
            }

            if (_commandLineInput.TargetFeature.Equals("RESTOREFACTORYDEFAULTS") && _commandLineInput.TargetType.Equals("PEN"))
            {
                SetResults.ForEach(x =>
                {
                    x.Value = "";
                    if (x.Result == "")
                    {
                        var result = "0"; //RunAsyncTimeout(_devMgr.(GUID, true)).Result;
                        if (result == "0")
                        {
                            x.Result = "PASS";
                            x.Value = "SUCCESS";
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
                        retcode = (result == "0") ? true : false;
                    }
                });
                return (retcode) ? (int)CLI_ExitCode.success : (int)CLI_ExitCode.functional_error;
            }

            foreach (var op in _commandLineInput.Options)
            {
                if (op.Option_Name.ToUpper() == "VALUE")
                {
                    op.Option_Value.Replace(".", ",");
                    List<string> op_value = op.Option_Value.Split(",").ToList();
                    //value = op_value[0];
                    foreach (var value in op_value)
                    {
                        Debug.WriteLine($"{value}, {_commandLineInput.Options[0].Option_Value}");
                        if (value == null) // if there is no option value
                        {
                            SetFailResults("no setting value");
                            return (int)CLI_ExitCode.fail_SetPeripheralProperty_Value;
                        }
                        else if (int.TryParse(value, out int tmp)) // for the function argument is number
                        {
                            val = tmp;
                        }
                        else //for the function argument is int(0/1) or bool
                        {
                            switch (value)
                            {
                                case "ENABLE": //spec is only defined enable/disable, on/off
                                case "ON":
                                    val = 1;
                                    bl = true;
                                    break;
                                case "DISABLE":
                                case "OFF":
                                    val = 0;
                                    bl = false;
                                    break;
                                case "LOCK":
                                case "UNLOCK":
                                    if (value.ToUpper().Equals("LOCK"))
                                    {
                                        switch (_commandLineInput.TargetFeature.ToUpper())
                                        {
                                            case "COLLABSCREENSHARE":
                                                data.LockSettings.Lock_Keyboard_CollabScreenShare = true;
                                                break;
                                            case "MICNOISECANCELLATION":
                                                data.LockSettings.Lock_Audio_micNoiseCancellation = true;
                                                break;
                                            case "HDR":
                                                data.LockSettings.Lock_Webcam_hdr = true;
                                                break;
                                            case "ANTIFLICKER":
                                                data.LockSettings.Lock_Webcam_AntiFlicker = true;
                                                break;
                                            case "AIAUTOFRAMING":
                                                data.LockSettings.Lock_Webcam_AIAutoFraming = true;
                                                break;
                                        }
                                    }
                                    if (value.ToUpper().Equals("UNLOCK"))
                                    {
                                        switch (_commandLineInput.TargetFeature.ToUpper())
                                        {
                                            case "COLLABSCREENSHARE":
                                                data.LockSettings.Lock_Keyboard_CollabScreenShare = false;
                                                break;
                                            case "MICNOISECANCELLATION":
                                                data.LockSettings.Lock_Audio_micNoiseCancellation = false;
                                                break;
                                            case "HDR":
                                                data.LockSettings.Lock_Webcam_hdr = false;
                                                break;
                                            case "ANTIFLICKER":
                                                data.LockSettings.Lock_Webcam_AntiFlicker = false;
                                                break;
                                            case "AIAUTOFRAMING":
                                                data.LockSettings.Lock_Webcam_AIAutoFraming = false;
                                                break;
                                        }
                                    }
                                    _devMgr.SetAppConfigData(data);
                                    break;
                                default: // currently, CLI peripheral didn't accept others setting type
                                    if (int.TryParse(value, out val))
                                        break;
                                    else
                                    {
                                        SetFailResults("Invalid setting value");
                                        return (int)CLI_ExitCode.fail_SetPeripheralProperty_Value;
                                    }
                            }
                        }
                    }
                    break;
                }
            }
            if (_commandLineInput.TargetFeature != "UNPAIR" && _commandLineInput.Options[0].Option_Value == "")
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
                //case "BACKLIGHTINGCONTROLS":
                //    taskA = _devMgr.SetBackLightingControls;
                //    RunTaskA(val);
                //    return (int)CLI_ExitCode.success;
                //case "BACKLIGHTINGLEVEL":
                //    taskA = _devMgr.SetBackLightingLevel;
                //    RunTaskA(val);
                //    return (int)CLI_ExitCode.success;
                //case "COLLABORATIONBLINKEFFECTENABLE":
                //    taskB = _devMgr.SetCollaborationBlinkEffectEnable;
                //    RunTaskB(bl);
                //    return (int)CLI_ExitCode.success;
                case "COLLABCAMERAENABLE":
                    taskB = _devMgr.SetCollaborationCameraEnable;
                    RunTaskB(bl);
                    return (int)CLI_ExitCode.success;
                case "COLLABCHATENABLE":
                    taskB = _devMgr.SetCollaborationChatEnable;
                    RunTaskB(bl);
                    return (int)CLI_ExitCode.success;
                //case "COLLABORATIONDOUBLETAPENABLE":
                //    taskB = _devMgr.SetCollaborationDoubleTapEnable;
                //    RunTaskB(bl);
                //    return (int)CLI_ExitCode.success;
                //case "COLLABORATIONKEYENABLE":
                //    taskB = _devMgr.SetCollaborationKeyEnable;
                //    RunTaskB(bl);
                //    return (int)CLI_ExitCode.success;
                case "COLLABMICMUTE":
                    taskB = _devMgr.SetCollaborationMicEnable;
                    RunTaskB(!bl);
                    return (int)CLI_ExitCode.success;
                case "COLLABSCREENSHARE":
                    taskB = _devMgr.SetCollaborationScreenShareEnable;
                    RunTaskB(bl);
                    return (int)CLI_ExitCode.success;
                //case "DPILEVEL":
                //    taskA = _devMgr.SetDPILevel;
                //    RunTaskA(val);
                //    return (int)CLI_ExitCode.success;
                //case "DPIVALUE":
                //    taskA = _devMgr.SetDPIValue;
                //    RunTaskA(val);
                //    return (int)CLI_ExitCode.success;
                //case "PRIMARYMOUSEBUTTON":
                //    MouseButton button;
                //    switch (value.ToUpper())
                //    {
                //        case "L":
                //            button = MouseButton.Left;
                //            break;

                //        case "R":
                //            button = MouseButton.Right;
                //            break;

                //        default:
                //            SetFailResults("Invalid setting value");
                //            return (int)CLI_ExitCode.fail_SetPeripheralProperty_Value;
                //    }
                //    SetResults.ForEach(x =>
                //    {
                //        x.Value = value;
                //        if (x.Result == "")
                //        {
                //            var result = RunAsyncTimeout(_devMgr.SetPrimaryMouseButton(button, Guid.Parse(x.Guid))).Result;
                //            if (result == "0")
                //            {
                //                x.Result = "PASS";
                //                x.Message = "N/A";
                //            }
                //            else if (result == "1")
                //            {
                //                x.Result = "FAIL";
                //                x.Message = "Timeout";
                //            }
                //            else
                //            {
                //                x.Result = "FAIL";
                //                x.Message = result;
                //            }
                //        }
                //    });
                //    return (int)CLI_ExitCode.success;

                //case "TOUCHSCROLLSENSITIVITYLEVEL":
                //    taskA = _devMgr.SetTouchScrollSensitivityLevel;
                //    RunTaskA(val);
                //    return (int)CLI_ExitCode.success;
                //case "UNPAIR":
                //    SetResults.ForEach(x =>
                //    {
                //        x.Value = "";
                //        if (x.Result == "")
                //        {
                //            var result = RunAsyncTimeout(_devMgr.UnPair(Guid.Parse(x.Guid))).Result;
                //            if (result == "0")
                //            {
                //                x.Result = "PASS";
                //                x.Message = "N/A";
                //            }
                //            else if (result == "1")
                //            {
                //                x.Result = "FAIL";
                //                x.Message = "Timeout";
                //            }
                //            else
                //            {
                //                x.Result = "FAIL";
                //                x.Message = result;
                //            }
                //        }
                //    });
                //    return (int)CLI_ExitCode.success;
                //Headset&Speaker
                //case "SETWIREDAUDIOIMICNSENABLE":
                //    taskB = _devMgr.SetWiredAudioIMicNSEnable;
                //    RunTaskB(bl);
                //    return (int)CLI_ExitCode.success;
                //case "SETWIREDAUDIOMICMUTESOUNDENABLE":
                //    taskB = _devMgr.SetWiredAudioMicMuteSoundEnable;
                //    RunTaskB(bl);
                //    return (int)CLI_ExitCode.success;
                //case "SETWIREDAUDIOVOLUMEADJUSTMENTTONE":
                //    taskA = _devMgr.SetWiredAudioVolumeAdjustmentTone;
                //    RunTaskA(val);
                //    return (int)CLI_ExitCode.success;
                case "ANCMODE":
                    if (val == 1)
                        data.LockSettings.Lock_Audio_ancMode = false;
                    else
                        data.LockSettings.Lock_Audio_ancMode = true;
                    _devMgr.SetAppConfigData(data);
                    taskA = _devMgr.SetAncMode;
                    RunTaskA(val);
                    return (int)CLI_ExitCode.success;
                case "SETANCGAIN":
                    taskA = _devMgr.SetAncGain;
                    RunTaskA(val);
                    return (int)CLI_ExitCode.success;
                //case "SETSELECTEDPRESET":
                //    taskA = _devMgr.SetSelectedPreset;
                //    RunTaskA(val);
                //    return (int)CLI_ExitCode.success;
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
                //case "SETSIDETONE":
                //    taskB = _devMgr.SetSidetone;
                //    RunTaskB(bl);
                //    return (int)CLI_ExitCode.success;
                //case "SETSIDETONELEVEL":
                //    taskA = _devMgr.SetSidetoneLevel;
                //    RunTaskA(val);
                //    return (int)CLI_ExitCode.success;
                case "WEARDETECTION":
                    if (val == 1)
                        data.LockSettings.Lock_Audio_wearDetection = false;
                    else
                        data.LockSettings.Lock_Audio_wearDetection = true;
                    _devMgr.SetAppConfigData(data);
                    taskA = _devMgr.SetWearDetectionForCLI;
                    RunTaskA(val);
                    return (int)CLI_ExitCode.success;
                //case "SETBUSYLIGHT":
                //    taskB = _devMgr.SetBusyLight;
                //    RunTaskB(bl);
                //    return (int)CLI_ExitCode.success;
                //case "SETVOICEGUIDANCE":
                //    taskB = _devMgr.SetVoiceGuidance;
                //    RunTaskB(bl);
                //    return (int)CLI_ExitCode.success;
                //case "SETMICNCINCOMING":
                //    taskB = _devMgr.SetMicNCIncoming;
                //    RunTaskB(bl);
                //    return (int)CLI_ExitCode.success;
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
                //case "SETISMICENUMERATIONON":
                //    taskB = _devMgr.SetIsMicEnumerationOn;
                //    RunTaskB(bl);
                //    return (int)CLI_ExitCode.success;

                case "HDR":
                    //ItemId = "DellPeripheral.Webcam.0";
                    if (_devMgr.GetIsPropertyHDRSupported(GUID).Result)
                    {
                        SetResults.ForEach((Action<CLI_PeripheralRESPONSE>)(x =>
                        {
                            x.Value = "";
                            if (x.Result == "")
                            {
                                var result = RunAsyncTimeout(_devMgr.SetIsHDROn(GUID, bl)).Result;
                                if (result == "0")
                                {
                                    x.Result = "PASS";
                                    retcode_ = _devMgr.GetIsHDROn(GUID).Result;
                                    x.Value = (retcode_) ? "ON" : "OFF";
                                    x.Value += "," + (data.LockSettings.Lock_Webcam_hdr ? "LOCK" : "UNLOCK");
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
                                retcode = (result == "0") ? true : false;
                            }
                        }));
                        return (retcode) ? (int)CLI_ExitCode.success : (int)CLI_ExitCode.functional_error;
                    }
                    else
                    {
                        SetResults.ForEach(x =>
                        {
                            x.Value = "N/A";
                            x.Result = "FAIL";
                            x.Message = "Webcam not support HDR";
                        });
                        return (int)CLI_ExitCode.command_targetfeature_not_support;
                    }

                case "ANTIFLICKER":
                    //ItemId = "DellPeripheral.Webcam.0";
                    if (_devMgr.GetIsPropertyAntiFlickerSupported(GUID).Result)
                    {
                        SetResults.ForEach(x =>
                        {
                            x.Value = "";
                            if (x.Result == "")
                            {
                                var result = RunAsyncTimeout(_devMgr.SetAntiFlicker(GUID, val)).Result;
                                if (result == "0")
                                {
                                    x.Result = "PASS";
                                    retvalue = _devMgr.GetAntiFlickerValueByDTP(GUID).Result;
                                    x.Value = retvalue.ToString();
                                    x.Value += "," + (data.LockSettings.Lock_Webcam_AntiFlicker ? "LOCK" : "UNLOCK");
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
                                retcode = (result == "0") ? true : false;
                            }
                        });
                        return (retcode) ? (int)CLI_ExitCode.success : (int)CLI_ExitCode.functional_error;
                    }
                    else
                    {
                        SetResults.ForEach(x =>
                        {
                            x.Value = "N/A";
                            x.Result = "FAIL";
                            x.Message = "Webcam not support AntiFlicker";
                        });
                        return (int)CLI_ExitCode.command_targetfeature_not_support;
                    }

                case "AIAUTOFRAMING":
                    //ItemId = "DellPeripheral.Webcam.0";
                    if (_devMgr.GetIsPropertyAutoFramingSupported(GUID).Result)
                    {
                        SetResults.ForEach(x =>
                        {
                            x.Value = "";
                            if (x.Result == "")
                            {
                                var result = RunAsyncTimeout(_devMgr.SetIsAutoFramingOn(GUID, bl)).Result;
                                if (result == "0")
                                {
                                    x.Result = "PASS";
                                    retcode_ = _devMgr.GetIsAutoFramingOn(GUID).Result;
                                    x.Value = (retcode_) ? "ON" : "OFF";
                                    x.Value += "," + (data.LockSettings.Lock_Webcam_AIAutoFraming ? "LOCK" : "UNLOCK");
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
                                retcode = (result == "0") ? true : false;
                            }
                        });
                        return (retcode) ? (int)CLI_ExitCode.success : (int)CLI_ExitCode.functional_error;
                    }
                    else
                    {
                        SetResults.ForEach(x =>
                        {
                            x.Value = "N/A";
                            x.Result = "FAIL";
                            x.Message = "Webcam not support AI AutoFraming";
                        });
                        return (int)CLI_ExitCode.command_targetfeature_not_support;
                    }
                case "PRESENCEDETECTION":
                    //ItemId = "DellPeripheral.Webcam.0";
                    SetResults.ForEach(x =>
                    {
                        x.Value = "";
                        if (x.Result == "")
                        {
                            var result = RunAsyncTimeout(_devMgr.SetIsProximitySensorEnable(GUID, bl)).Result;
                            if (result == "0")
                            {
                                x.Result = "PASS";
                                retcode_ = _devMgr.GetIsProximitySensorEnable(GUID).Result;
                                x.Value = (retcode_) ? "ON" : "OFF";
                                x.Value += "," + (data.LockSettings.Lock_Webcam_PresenceDetection ? "LOCK" : "UNLOCK");
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
                            retcode = (result == "0") ? true : false;
                        }
                    });
                    return (retcode) ? (int)CLI_ExitCode.success : (int)CLI_ExitCode.functional_error;

                case "MICSWITCH":
                    //var ItemId_ = "DellPeripheral.Webcam.0";
                    if (_deviceinfo.FirstOrDefault(x => x.ID.ToString() == GUID)?.IsMicEnumerationSupported == true)
                    {
                        SetResults.ForEach(x =>
                        {
                            x.Value = "";
                            if (x.Result == "")
                            {
                                var result = RunAsyncTimeout(_devMgr.SetIsMicEnumerationOn(GUID, bl)).Result;
                                if (result == "0")
                                {
                                    x.Result = "PASS";
                                    retcode_ = _deviceinfo.FirstOrDefault(x => x.ID.ToString() == GUID).IsMicEnumerationOn;
                                    x.Value = (retcode_) ? "ON" : "OFF";
                                    x.Value += "," + (data.LockSettings.Lock_Webcam_MicSwitch ? "LOCK" : "UNLOCK");
                                    x.Message = _deviceinfo.FirstOrDefault(x => x.ID.ToString() == GUID).IsMicEnumerationOn.ToString();
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
                                retcode = (result == "0") ? true : false;
                            }
                        });
                        return (retcode) ? (int)CLI_ExitCode.success : (int)CLI_ExitCode.functional_error;
                    }
                    else
                    {
                        SetResults.ForEach(x =>
                        {
                            x.Value = "N/A";
                            x.Result = "FAIL";
                            x.Message = "Webcam not support MicSwitch";
                        });
                        return (int)CLI_ExitCode.command_targetfeature_not_support;
                    }
                default:
                    SetFailResults("Invalid TargetFeature");
                    return (int)CLI_ExitCode.fail_SetPeripheralProperty_Property;
            }
        }

        private void RunTaskA(int val)
        {
            DDPMSettings data = _devMgr.ReloadAppConfigData().Result;
            SetResults.ForEach(x =>
            {
                x.Value = _commandLineInput.Options[0].Option_Value;
                if (_commandLineInput.TargetFeature.ToUpper().Equals("ANCMODE"))
                    x.Value += "," + (data.LockSettings.Lock_Audio_ancMode ? "LOCK" : "UNLOCK");
                if (_commandLineInput.TargetFeature.ToUpper().Equals("WEARDETECTION"))
                    x.Value += "," + (data.LockSettings.Lock_Audio_wearDetection ? "LOCK" : "UNLOCK");
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
                x.Value = _commandLineInput.Options[0].Option_Value;
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
                x.Value = _commandLineInput.Options[0].Option_Value;
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
        //public event EventHandler<(List<FWUpdateInfo>, string)> FWResultReceived;
        public event EventHandler<(List<FWUpdateInfo>, string)> FWResultReceived_List;
        public event EventHandler<(FWUErrorCode, string)> FWResultReceived;
        private DeviceHelper _deviceHelper;
        private (int code, string json) FWUpdate(CommandLineInput commandLineInput)
        {
            string output = string.Empty;
            List<List<string>> report = new List<List<string>>();
            List<string> tmpReport = new List<string>();
            CLI_RESPONSE cLI_RESPONSE = new CLI_RESPONSE();
            cLI_RESPONSE.Command = commandLineInput.Command;
            cLI_RESPONSE.TargetFeature = commandLineInput.TargetFeature;
            CLIEventResult result = new CLIEventResult();

            _deviceHelper = new DeviceHelper
            {
                deviceInfo = new List<DeviceInfo>()
            };

            List<DeviceInfo> _deviceinfo = null;
            _deviceinfo = _devMgr.GetDevices().Result.deviceInfo;

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
            try
            {
                switch (commandLineInput.TargetFeature)
                {
                    case "FIRMWAREUPDATE":
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
                            string[] ss_1 = commandLineInput.Options[0].Option_Value.Split(",");

                            if (ss_1[0].ToUpper().Equals("DISPLAY") && ss_1[1].ToUpper().Equals("FORCEWITHNOTICE"))
                            {
                                isShowInfo = true;
                                var fwupdate = Auto_FWUpdate_display(commandLineInput, cLI_FWU_RESPONSE, installPath, isShowInfo, true);

                                result.ExitCode = fwupdate.code;
                                result.serialize_Json_response = fwupdate.result;
                                ret = true;
                            }
                            else if (ss_1[0].ToUpper().Equals("DISPLAY") && ss_1[1].ToUpper().Equals("FORCEWITHNONOTICE"))
                            {
                                isShowInfo = false;
                                var fwupdate = Auto_FWUpdate_display(commandLineInput, cLI_FWU_RESPONSE, installPath, isShowInfo, true);
                                FWResultReceived_List += Download_Event_2;
                                result.ExitCode = fwupdate.code;
                                result.serialize_Json_response = fwupdate.result;
                                ret = true;
                            }
                            else if (ss_1[1].ToUpper().Equals("FORCEWITHNOTICE"))
                            {
                                isShowInfo = true;
                                var fwupdate = Auto_FWUpdate2(commandLineInput, cLI_FWU_RESPONSE, false, installPath, isShowInfo, true);

                                result.ExitCode = fwupdate.code;
                                result.serialize_Json_response = fwupdate.result;
                                ret = true;
                            }
                            else if (ss_1[1].ToUpper().Equals("FORCEWITHNONOTICE"))
                            {
                                isShowInfo = false;
                                var fwupdate = Auto_FWUpdate2(commandLineInput, cLI_FWU_RESPONSE, false, installPath, isShowInfo, true);
                                FWResultReceived_List += Download_Event_2;
                                result.ExitCode = fwupdate.code;
                                result.serialize_Json_response = fwupdate.result;
                                ret = true;
                            }
                            else if (ss_1[1].ToUpper().Contains(":\\"))
                            {
                                installPath = Path.GetFullPath(ss_1[1]);
                                Trace.WriteLine($"installPath = {installPath}");
                                isShowInfo = true;
                                var fwupdate = Auto_FWUpdate_display(commandLineInput, cLI_FWU_RESPONSE, installPath, isShowInfo, true);

                                result.ExitCode = fwupdate.code;
                                result.serialize_Json_response = fwupdate.result;
                                ret = true;

                            }
                            else if (ss_1[1].ToUpper().Equals("DEFER"))
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
                            else
                            {
                                somethingError = true;
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


                        }
                        else
                        {
                            cLI_FWU_RESPONSE.Message = "Input FAIL";
                            ret = false;
                        }
                        cLI_FWU_RESPONSE.Result = ret == true ? "PASS" : "FAIL";
                        break;
                    case "SILENTFWUPDATE":
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
                            string[] ss_1 = commandLineInput.Options[0].Option_Value.Split(",");

                            if (commandLineInput.Options[0].Option_Value.ToUpper().Equals("UOD"))
                            {
                                isShowInfo = true;
                                var fwupdate = Auto_FWUpdate2(commandLineInput, cLI_FWU_RESPONSE, true, installPath, isShowInfo, true);

                                result.ExitCode = fwupdate.code;
                                result.serialize_Json_response = fwupdate.result;
                                ret = true;
                            }
                            else if (commandLineInput.Options[0].Option_Value.ToUpper().Contains(":\\"))
                            {
                                installPath = Path.GetFullPath(ss_1[1]);
                                Trace.WriteLine($"installPath = {installPath}");
                                isShowInfo = true;
                                var fwupdate = Auto_FWUpdate2(commandLineInput, cLI_FWU_RESPONSE, false, installPath, isShowInfo, true);

                                result.ExitCode = fwupdate.code;
                                result.serialize_Json_response = fwupdate.result;
                                ret = true;

                            }
                            else
                            {
                                somethingError = true;
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


                        }
                        else if (commandLineInput.Options.Count == 0)
                        {
                            isShowInfo = true;
                            var fwupdate = Auto_FWUpdate2(commandLineInput, cLI_FWU_RESPONSE, true, installPath, isShowInfo, true);

                            result.ExitCode = fwupdate.code;
                            result.serialize_Json_response = fwupdate.result;
                            ret = true;
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
            catch
            {
                return ((int)CLI_ExitCode.fail_FWUpdate, output);
            }

        }

        private bool? GetFWUpdateList(CommandLineInput commandLineInput, CLI_FWU_RESPONSE cli_FWU_RESPONSE, bool isShowInfo = true, bool isDefer = false)
        {
            List<DeviceType> deviceTypes = new List<DeviceType>();
            DeviceType deviceType = DeviceType.Unknown;
            if (commandLineInput.Options.Count > 0)
            {
                (deviceType, deviceTypes) = SetDevice(commandLineInput);

            }
            List<string> tmpReport = new List<string>();
            FWUpdateInfoPackage fwUpdateInfoPackage;
            if (deviceType == DeviceType.Unknown)
            {
                fwUpdateInfoPackage = _devMgr.GetFWUpdateInfo(isShowInfo, false, isDefer, null, false, true).Result;
            }
            else
            {
                fwUpdateInfoPackage = _devMgr.GetFWUpdateInfo(isShowInfo, false, isDefer, deviceTypes).Result;
            }
            if (fwUpdateInfoPackage.FWUpdateInfo.Count > 0)
            {
                int index = 1;
                foreach (FWUpdateInfo fwUpdateInfo in fwUpdateInfoPackage.FWUpdateInfo)
                {
                    cli_FWU_RESPONSE.Model = fwUpdateInfo.Model;
                    //cli_FWU_RESPONSE.GUID.Add(fwUpdateInfo.DeviceId);
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
                if (commandLineInput.Options.Count > 0)
                {
                    (deviceType, deviceTypes) = SetDevice(commandLineInput);

                    if (isUODMode && deviceType != DeviceType.LogicalDock)
                    {
                        cli_FWU_RESPONSE.Message = "Only dock supports UOD update mode.";
                        return false;
                    }
                }
                _devMgr.ProgressUpdate_Notify -= _FWUpdatePlugin_ProgressUpdate;
                _devMgr.ProgressUpdate_Notify += _FWUpdatePlugin_ProgressUpdate;
                _devMgr.DownloadAndInstall_Result_Notify -= Download_Event;
                _devMgr.DownloadAndInstall_Result_Notify += Download_Event;
                FWUpdateInfoPackage fwUpdateInfoPackage;
                if (deviceType == DeviceType.Unknown)
                {
                    fwUpdateInfoPackage = _devMgr.GetFWUpdateInfo(isShowInfo, isForce, false, null, false,true,true).Result;
                }
                else
                {
                    fwUpdateInfoPackage = _devMgr.GetFWUpdateInfo(isShowInfo, isForce, false, deviceTypes, isUODMode).Result;
                }
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
                        //cli_FWU_RESPONSE.GUID.Add(fwUpdateInfo.DeviceId);
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
        private (int code, string result) Auto_FWUpdate2(CommandLineInput commandLineInput, CLI_FWU_RESPONSE cli_FWU_RESPONSE, bool isUODMode, string installPath, bool isShowInfo = true, bool isForce = false)
        {
            try
            {
                List<DeviceType> deviceTypes = new List<DeviceType>();
                DeviceType deviceType = DeviceType.Unknown;
                if (commandLineInput.Options.Count > 0)
                {
                    (deviceType, deviceTypes) = SetDevice(commandLineInput);

                    if (isUODMode && deviceType != DeviceType.LogicalDock)
                    {
                        cli_FWU_RESPONSE.Message = "Only dock supports UOD update mode.";
                        return ((int)CLI_ExitCode.fail_NotSupport, JsonConvert.SerializeObject(cli_FWU_RESPONSE, Formatting.Indented));
                    }
                }

                _devMgr.ProgressUpdate_Notify -= _FWUpdatePlugin_ProgressUpdate;
                _devMgr.ProgressUpdate_Notify += _FWUpdatePlugin_ProgressUpdate;
                _devMgr.DownloadAndInstall_Result_Notify -= Download_Event;
                _devMgr.DownloadAndInstall_Result_Notify += Download_Event;
                if (installPath != "")
                {

                    Task.Run(new Action(() =>
                    {
                        FWUErrorCode ret = _devMgr.Install(installPath, false).Result;
                        Trace.WriteLine($"ret = {ret}");

                        cli_FWU_RESPONSE.Model = "retFWUpdateInfo.Model";
                        if (ret == FWUErrorCode.NoError)
                        {
                            cli_FWU_RESPONSE.FWUpdateRESPONSE.Add($"{"retFWUpdateInfo.DeviceName"} update success.");
                        }
                        else
                        {
                            cli_FWU_RESPONSE.FWUpdateRESPONSE.Add($"{"retFWUpdateInfo.DeviceName"} update fail. Fail message:{ret.ToString()}");
                        }
                        FWResultReceived?.Invoke(this, (ret, JsonConvert.SerializeObject(cli_FWU_RESPONSE, Formatting.Indented)));
                        _devMgr.ProgressUpdate_Notify -= _FWUpdatePlugin_ProgressUpdate;
                        _devMgr.DownloadAndInstall_Result_Notify -= Download_Event;
                    }));
                }
                else
                {
                    FWUpdateInfoPackage fwUpdateInfoPackage = _devMgr.GetFWUpdateInfo(isShowInfo, isForce, false, deviceTypes, isUODMode).Result;
                    if (fwUpdateInfoPackage.FWUpdateInfo.Count <= 0)
                    {
                        cli_FWU_RESPONSE.Message = "No updates available";
                        return ((int)CLI_ExitCode.NoUpdate, JsonConvert.SerializeObject(cli_FWU_RESPONSE, Formatting.Indented));
                    }
                    if (deviceType != DeviceType.Unknown)
                    {
                        foreach (FWUpdateInfo fwUpdateInfo in fwUpdateInfoPackage.FWUpdateInfo)
                        {
                            cli_FWU_RESPONSE.Model = fwUpdateInfo.Model;
                            //cli_FWU_RESPONSE.GUID.Add(fwUpdateInfo.DeviceId);
                            cli_FWU_RESPONSE.FWUpdateRESPONSE.Add($"Ready to start updating Device:{fwUpdateInfo.DeviceName} to Version:{fwUpdateInfo.TheLatestVersion}");
                        }
                        cli_FWU_RESPONSE.OutputLog(cli_FWU_RESPONSE, commandLineInput);
                        Task.Run(new Action(() =>
                        {
                            do
                            {
                                Thread.Sleep(100);
                            } while (retFWUpdateInfos == null);
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
                                }
                            }
                            FWResultReceived_List?.Invoke(this, (retFWUpdateInfos, JsonConvert.SerializeObject(cli_FWU_RESPONSE, Formatting.Indented)));
                            _devMgr.ProgressUpdate_Notify -= _FWUpdatePlugin_ProgressUpdate;
                            _devMgr.DownloadAndInstall_Result_Notify -= Download_Event;
                        }));
                        return ((int)CLI_ExitCode.success, JsonConvert.SerializeObject(cli_FWU_RESPONSE, Formatting.Indented));
                    }
                    else
                    {
                        return ((int)CLI_ExitCode.fail_NotSupport, JsonConvert.SerializeObject(cli_FWU_RESPONSE, Formatting.Indented));
                    }
                }
                return ((int)CLI_ExitCode.success, JsonConvert.SerializeObject(cli_FWU_RESPONSE, Formatting.Indented));

            }
            catch
            {
                return ((int)CLI_ExitCode.fail_FWUpdate, JsonConvert.SerializeObject(cli_FWU_RESPONSE, Formatting.Indented));
            }
        }

        private (int code, string result) Auto_FWUpdate_display(CommandLineInput commandLineInput, CLI_FWU_RESPONSE cli_FWU_RESPONSE, string installPath, bool isShowInfo = true, bool isForce = false)
        {
            if (commandLineInput.ServiceTag.Count > 0)
            {
                try
                {
                    _devMgr.ProgressUpdate_Notify -= _FWUpdatePlugin_ProgressUpdate;
                    _devMgr.ProgressUpdate_Notify += _FWUpdatePlugin_ProgressUpdate;
                    _devMgr.DownloadAndInstall_Result_Notify -= Download_Event;
                    _devMgr.DownloadAndInstall_Result_Notify += Download_Event;
                    if (installPath != "")
                    {

                        Task.Run(new Action(() =>
                        {
                            FWUErrorCode ret = _devMgr.Install(installPath, true).Result;
                            Trace.WriteLine($"ret = {ret}");

                            cli_FWU_RESPONSE.Model = "retFWUpdateInfo.Model";
                            if (ret == FWUErrorCode.NoError)
                            {
                                cli_FWU_RESPONSE.FWUpdateRESPONSE.Add($"{"retFWUpdateInfo.DeviceName"} update success.");
                            }
                            else
                            {
                                cli_FWU_RESPONSE.FWUpdateRESPONSE.Add($"{"retFWUpdateInfo.DeviceName"} update fail. Fail message:{ret.ToString()}");
                            }
                            FWResultReceived?.Invoke(this, (ret, JsonConvert.SerializeObject(cli_FWU_RESPONSE, Formatting.Indented)));
                            _devMgr.ProgressUpdate_Notify -= _FWUpdatePlugin_ProgressUpdate;
                            _devMgr.DownloadAndInstall_Result_Notify -= Download_Event;
                        }));
                    }
                    else
                    {
                        FWUpdateInfoPackage fwUpdateInfoPackage = _devMgr.GetFWUpdateInfo(isShowInfo, isForce, false, null, false, true).Result;
                        FWUpdateInfo ret = fwUpdateInfoPackage.FWUpdateInfo.Find(x => x.ServiceTag.Equals(commandLineInput.Options[1].Option_Value));
                        Trace.WriteLine($"ret {ret}");
                        Trace.WriteLine($"commandLineInput.Options[1].Option_Value {commandLineInput.Options[1].Option_Value}");
                        if (ret.ServiceTag != "")
                        {
                            //Trace.WriteLine($"ret {ret.ToString()}");
                            if (fwUpdateInfoPackage.FWUpdateInfo.Count <= 0)
                            {
                                cli_FWU_RESPONSE.Message = "No updates available";
                                return ((int)CLI_ExitCode.NoUpdate, JsonConvert.SerializeObject(cli_FWU_RESPONSE, Formatting.Indented));
                            }
                            foreach (FWUpdateInfo fwUpdateInfo in fwUpdateInfoPackage.FWUpdateInfo)
                            {
                                if (ret.ToString() != "")
                                {
                                    cli_FWU_RESPONSE.Model = fwUpdateInfo.Model;
                                    //cli_FWU_RESPONSE.GUID.Add(fwUpdateInfo.DeviceId);
                                    cli_FWU_RESPONSE.FWUpdateRESPONSE.Add($"Ready to start updating Device:{fwUpdateInfo.DeviceName} to Version:{fwUpdateInfo.TheLatestVersion}");
                                }
                            }
                            cli_FWU_RESPONSE.OutputLog(cli_FWU_RESPONSE, commandLineInput);
                            System.Threading.Tasks.Task.Run(new Action(() =>
                            {
                                do
                                {
                                    Thread.Sleep(100);
                                } while (retFWUpdateInfos == null);
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
                                    }
                                }
                                FWResultReceived_List?.Invoke(this, (retFWUpdateInfos, JsonConvert.SerializeObject(cli_FWU_RESPONSE, Formatting.Indented)));
                                _devMgr.ProgressUpdate_Notify -= _FWUpdatePlugin_ProgressUpdate;
                                _devMgr.DownloadAndInstall_Result_Notify -= Download_Event;
                            }));
                        }
                    }
                    return ((int)CLI_ExitCode.success, JsonConvert.SerializeObject(cli_FWU_RESPONSE, Formatting.Indented));

                }
                catch
                {
                    return ((int)CLI_ExitCode.fail_FWUpdate, JsonConvert.SerializeObject(cli_FWU_RESPONSE, Formatting.Indented));
                }

            }
            try
            {
                _devMgr.ProgressUpdate_Notify -= _FWUpdatePlugin_ProgressUpdate;
                _devMgr.ProgressUpdate_Notify += _FWUpdatePlugin_ProgressUpdate;
                _devMgr.DownloadAndInstall_Result_Notify -= Download_Event;
                _devMgr.DownloadAndInstall_Result_Notify += Download_Event;
                if (installPath != "")
                {

                    Task.Run(new Action(() =>
                    {
                        FWUErrorCode ret = _devMgr.Install(installPath, true).Result;
                        Trace.WriteLine($"ret = {ret}");

                        cli_FWU_RESPONSE.Model = "retFWUpdateInfo.Model";
                        if (ret == FWUErrorCode.NoError)
                        {
                            cli_FWU_RESPONSE.FWUpdateRESPONSE.Add($"{"retFWUpdateInfo.DeviceName"} update success.");
                        }
                        else
                        {
                            cli_FWU_RESPONSE.FWUpdateRESPONSE.Add($"{"retFWUpdateInfo.DeviceName"} update fail. Fail message:{ret.ToString()}");
                        }
                        FWResultReceived?.Invoke(this, (ret, JsonConvert.SerializeObject(cli_FWU_RESPONSE, Formatting.Indented)));
                        _devMgr.ProgressUpdate_Notify -= _FWUpdatePlugin_ProgressUpdate;
                        _devMgr.DownloadAndInstall_Result_Notify -= Download_Event;
                    }));
                }
                else
                {
                    FWUpdateInfoPackage fwUpdateInfoPackage = _devMgr.GetFWUpdateInfo(isShowInfo, isForce, false, null, false, true).Result;

                    //Trace.WriteLine($"ret {ret.ToString()}");
                    if (fwUpdateInfoPackage.FWUpdateInfo.Count <= 0)
                    {
                        cli_FWU_RESPONSE.Message = "No updates available";
                        return ((int)CLI_ExitCode.NoUpdate, JsonConvert.SerializeObject(cli_FWU_RESPONSE, Formatting.Indented));
                    }
                    foreach (FWUpdateInfo fwUpdateInfo in fwUpdateInfoPackage.FWUpdateInfo)
                    {
                        cli_FWU_RESPONSE.Model = fwUpdateInfo.Model;
                        //cli_FWU_RESPONSE.GUID.Add(fwUpdateInfo.DeviceId);
                        cli_FWU_RESPONSE.FWUpdateRESPONSE.Add($"Ready to start updating Device:{fwUpdateInfo.DeviceName} to Version:{fwUpdateInfo.TheLatestVersion}");
                    }
                    cli_FWU_RESPONSE.OutputLog(cli_FWU_RESPONSE, commandLineInput);
                    System.Threading.Tasks.Task.Run(new Action(() =>
                    {
                        do
                        {
                            Thread.Sleep(100);
                        } while (retFWUpdateInfos == null);
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
                            }
                        }
                        FWResultReceived_List?.Invoke(this, (retFWUpdateInfos, JsonConvert.SerializeObject(cli_FWU_RESPONSE, Formatting.Indented)));
                        _devMgr.ProgressUpdate_Notify -= _FWUpdatePlugin_ProgressUpdate;
                        _devMgr.DownloadAndInstall_Result_Notify -= Download_Event;
                    }));
                }
                return ((int)CLI_ExitCode.success, JsonConvert.SerializeObject(cli_FWU_RESPONSE, Formatting.Indented));

            }
            catch
            {
                return ((int)CLI_ExitCode.fail_FWUpdate, JsonConvert.SerializeObject(cli_FWU_RESPONSE, Formatting.Indented));
            }

        }

        private (DeviceType, List<DeviceType>) SetDevice(CommandLineInput commandLineInput)
        {
            List<DeviceType> deviceTypes = new List<DeviceType>();
            DeviceType deviceType = DeviceType.Unknown;
            string[] ss_1 = commandLineInput.Options[0].Option_Value.Split(",");
            switch (ss_1[0].ToUpper())
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
                case "HEADSET":
                    deviceTypes.Add(DeviceType.LogicalHeadset);
                    deviceType = DeviceType.LogicalHeadset;
                    break;
                case "AUDIO":
                    deviceTypes.Add(DeviceType.LogicalWiredAudio);
                    deviceTypes.Add(DeviceType.PhysicalWiredAudio);
                    deviceTypes.Add(DeviceType.PhysicalAudioDongle);
                    deviceTypes.Add(DeviceType.PhysicalBluetoothAudio);
                    deviceType = DeviceType.LogicalWiredAudio;
                    break;
                case "WEBCAM":
                    deviceTypes.Add(DeviceType.LogicalWebcam);
                    deviceTypes.Add(DeviceType.PhysicalWebcam);
                    deviceType = DeviceType.LogicalWebcam;
                    break;
                case "PEN":
                    deviceTypes.Add(DeviceType.PhysicalPen);
                    deviceTypes.Add(DeviceType.LogicalPen);
                    deviceType = DeviceType.LogicalPen;
                    break;
                default:
                    deviceType = DeviceType.Unknown;
                    break;
            }
            return (deviceType, deviceTypes);
        }
        private void Download_Event(object o, List<FWUpdateInfo> e)
        {
            retFWUpdateInfos = e;
        }
        private void Download_Event_2(object o, (List<FWUpdateInfo>, string) e)
        {

        }
        private bool isDownload, isInstalling;

        private void _FWUpdatePlugin_ProgressUpdate(object sender, UpdateProgressInfo e)
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
        private (int code, string json) SWAPPUpdate(CommandLineInput commandLineInput)
        {
            string output = string.Empty;
            List<List<string>> report = new List<List<string>>();
            List<string> tmpReport = new List<string>();
            CLI_RESPONSE cLI_RESPONSE = new CLI_RESPONSE();
            cLI_RESPONSE.Command = commandLineInput.Command;
            cLI_RESPONSE.TargetFeature = commandLineInput.TargetFeature;
            CLIEventResult result = new CLIEventResult();

            _deviceHelper = new DeviceHelper
            {
                deviceInfo = new List<DeviceInfo>()
            };

            List<DeviceInfo> _deviceinfo = null;
            _deviceinfo = _devMgr.GetDevices().Result.deviceInfo;


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
            CLI_SWU_RESPONSE cLI_SWU_RESPONSE = null;
            try
            {
                switch (commandLineInput.TargetFeature)
                {
                    case "UPDATE":
                        cLI_SWU_RESPONSE = new CLI_SWU_RESPONSE(cLI_RESPONSE);
                        if (commandLineInput.Options.Count > 2)
                        {
                            cLI_SWU_RESPONSE.Result = "FAIL";
                            cLI_SWU_RESPONSE.Message = "Bring in extra strings:";
                            for (int i = 0; i < commandLineInput.Options.Count; i++)
                            {
                                cLI_SWU_RESPONSE.Message += $"{commandLineInput.Options[i].Option_Name}={commandLineInput.Options[i].Option_Value}\n";
                            }
                            break;
                        }
                        if (commandLineInput.Options.Count > 0)
                        {



                        }
                        else if (commandLineInput.Options.Count == 0)
                        {
                            SWUpdateInfoPackage swUpdateInfoPackage = _devMgr.SW_GetSWUpdateInfo(true, false, true).Result;
                            Trace.WriteLine($"swUpdateInfoPackage {swUpdateInfoPackage}");
                            ret = true;
                            _devMgr.SW_DownloadAndInstall(swUpdateInfoPackage.SWUpdateInfo, false, installPath);
                        }
                        else
                        {
                            cLI_SWU_RESPONSE.Message = "Input FAIL";
                            ret = false;
                        }
                        cLI_SWU_RESPONSE.Result = ret == true ? "PASS" : "FAIL";
                        break;

                    case "UPDATESOURCELOCATION":

                        if (commandLineInput.Options[0].Option_Value.ToUpper() == "ON" || commandLineInput.Options[0].Option_Value.ToUpper().Contains("/") || commandLineInput.Options[0].Option_Value.ToUpper() == "OFF")
                        {
                            ret = _devMgr.SetServerURL(commandLineInput.Options[0].Option_Value.ToString()).Result;
                        }

                        break;

                    default:
                        cLI_SWU_RESPONSE.Message = "Input FAIL";
                        ret = false;
                        break;
                }
                cLI_RESPONSE.Result = ret == true ? "PASS" : "FAIL";
                if (cLI_SWU_RESPONSE != null)
                {
                    output = cLI_SWU_RESPONSE.OutputLog(cLI_SWU_RESPONSE, commandLineInput);
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
                    return ((int)CLI_ExitCode.fail_SWUpdate, output);
                }
            }
            catch
            {
                return ((int)CLI_ExitCode.fail_SWUpdate, output);
            }

        }

        private ISWUpdateService _SWUpdatePlugin;
        private (int code, string json) SWAPPUpdate_get(CommandLineInput commandLineInput)
        {
            string output = string.Empty;
            List<List<string>> report = new List<List<string>>();
            List<string> tmpReport = new List<string>();
            CLI_RESPONSE cLI_RESPONSE = new CLI_RESPONSE();
            cLI_RESPONSE.Command = commandLineInput.Command;
            cLI_RESPONSE.TargetFeature = commandLineInput.TargetFeature;
            CLIEventResult result = new CLIEventResult();

            _deviceHelper = new DeviceHelper
            {
                deviceInfo = new List<DeviceInfo>()
            };

            List<DeviceInfo> _deviceinfo = null;
            _deviceinfo = _devMgr.GetDevices().Result.deviceInfo;


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
            string path = "";
            bool somethingError = false;
            CLI_SWU_RESPONSE cLI_SWU_RESPONSE = null;
            try
            {
                switch (commandLineInput.TargetFeature)
                {
                    case "UPDATE":
                        cLI_SWU_RESPONSE = new CLI_SWU_RESPONSE(cLI_RESPONSE);
                        if (commandLineInput.Options.Count > 2)
                        {
                            cLI_SWU_RESPONSE.Result = "FAIL";
                            cLI_SWU_RESPONSE.Message = "Bring in extra strings:";
                            for (int i = 0; i < commandLineInput.Options.Count; i++)
                            {
                                cLI_SWU_RESPONSE.Message += $"{commandLineInput.Options[i].Option_Name}={commandLineInput.Options[i].Option_Value}\n";
                            }
                            break;
                        }
                        if (commandLineInput.Options.Count > 0)
                        {



                        }
                        else if (commandLineInput.Options.Count == 0)
                        {
                            SWUpdateInfoPackage swUpdateInfoPackage = _devMgr.SW_GetSWUpdateInfo(true, false, true).Result;
                            Trace.WriteLine($"swUpdateInfoPackage {swUpdateInfoPackage}");
                            ret = true;
                            //_devMgr.SW_DownloadAndInstall(swUpdateInfoPackage.SWUpdateInfo, installPath);
                        }
                        else
                        {
                            cLI_SWU_RESPONSE.Message = "Input FAIL";
                            ret = false;
                        }
                        cLI_SWU_RESPONSE.Result = ret == true ? "PASS" : "FAIL";
                        break;

                    case "UPDATESOURCELOCATION":

                        path = _devMgr.GetServerURL().Result;
                        if (!string.IsNullOrEmpty(path))
                            ret = true;

                        cLI_RESPONSE.Value = path;
                        Trace.WriteLine($"path = {path}");

                        break;

                    default:
                        cLI_SWU_RESPONSE.Message = "Input FAIL";
                        ret = false;
                        break;
                }
                cLI_RESPONSE.Result = ret == true ? "PASS" : "FAIL";
                if (cLI_SWU_RESPONSE != null)
                {
                    output = cLI_SWU_RESPONSE.OutputLog(cLI_SWU_RESPONSE, commandLineInput);
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
                    return ((int)CLI_ExitCode.fail_SWUpdate, output);
                }
            }
            catch
            {
                return ((int)CLI_ExitCode.fail_SWUpdate, output);
            }

        }
    }
}