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
using VcpCore.Common;
using Windows.Devices.HumanInterfaceDevice;
using Windows.Gaming.Input;
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
        private const string publisherCompany = "Dell Technologies";
        private const string publisherWebsite = "https://www.dell.com";
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
        private List<MonitorInfo> _AllInfoMonitors;
        private GlobalSettingParam _GlobalSettingParam = new GlobalSettingParam();

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
        bool _recode_air_audio = false;

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

                    List<DeviceInfo> deviceInfoList = null;
                    deviceInfoList = _devMgr.GetDevices().Result.deviceInfo;
                    foreach (var g in deviceInfoList)
                    {
                        if (g.LogicalDeviceType == "LogicalHeadset")
                        {
                            writelog("LogicalHeadset Entry");
                            _commandLineInput.PluginsType = "HEADSET";
                            _recode_head = true;
                        }
                        if (g.LogicalDeviceType == "LogicalWiredAudio")
                        {
                            writelog("LogicalWiredAudio Entry");
                            _commandLineInput.PluginsType = "LOGICALWIREDAUDIO";
                            _recode_speak = true;
                        }
                        if (g.LogicalDeviceType == "LogicalAirAudio")
                        {
                            writelog("LogicalAirAudio Entry");
                            _commandLineInput.PluginsType = "LOGICALAIRAUDIO";
                            _recode_air_audio = true;
                        }
                    }

                }

                if (commandLineInput.Command.Equals("SET") &&
                    commandLineInput.TargetType.Equals("APP"))
                {
                    //if (commandLineInput.TargetFeature.Equals("FIRMWAREUPDATE") || commandLineInput.TargetFeature.Equals("UODFWUPDATE") || commandLineInput.TargetFeature.Equals("LOCKUIUPDATE") || commandLineInput.TargetFeature.Equals("UNLOCKUIUPDATE"))// for firmware update.
                    if (commandLineInput.TargetFeature.Equals("FIRMWAREUPDATE"))// for firmware update.
                    {
                        switch (commandLineInput.TargetFeature)
                        {
                            case "FIRMWAREUPDATE":
                                //case "UODFWUPDATE":
                                //case "LOCKUIUPDATE":
                                //case "UNLOCKUIUPDATE":
                                writelog("FIRMWAREUPDATE Entry");
                                var ret = FWUpdate(commandLineInput);
                                writelog("FIRMWAREUPDATE Done");
                                result.ExitCode = ret.code;
                                result.serialize_Json_response = ret.json;
                                return result;
                        }
                    }


                }
                if (commandLineInput.Command.Equals("SET") &&
                    commandLineInput.TargetType.Equals("DOCK") &&
                    commandLineInput.TargetFeature.Equals("SILENTFWUPDATE"))
                {
                    //switch (commandLineInput.TargetFeature)
                    //{
                    //    case "FIRMWAREUPDATE":
                    //    case "UODFWUPDATE":
                    //    case "LOCKUIUPDATE":
                    //    case "UNLOCKUIUPDATE":
                    writelog("FIRMWAREUPDATE SET DOCK SILENTFWUPDATE Entry");
                    var ret = FWUpdate(commandLineInput);
                    writelog("FIRMWAREUPDATE SET DOCK SILENTFWUPDATE DONE");
                    result.ExitCode = ret.code;
                    result.serialize_Json_response = ret.json;
                    return result;
                    //}
                }

                if (commandLineInput.Command.Equals("SET"))
                {
                    if (commandLineInput.TargetType.Equals("APP") &&
                        commandLineInput.TargetFeature.Equals("UPDATE"))
                    {
                        //switch (commandLineInput.TargetFeature)
                        //{
                        //    case "FIRMWAREUPDATE":
                        //    case "UODFWUPDATE":
                        //    case "LOCKUIUPDATE":
                        //    case "UNLOCKUIUPDATE":
                        writelog("FIRMWAREUPDATE SET UPDATE Entry");
                        var ret = SWAPPUpdate(commandLineInput);
                        writelog("FIRMWAREUPDATE SET UPDATE DONE");
                        result.ExitCode = ret.code;
                        result.serialize_Json_response = ret.json;
                        return result;
                        //}
                    }

                }
                else if (commandLineInput.Command.Equals("GET"))
                {
                    if (commandLineInput.TargetType.Equals("APP") &&
                        commandLineInput.TargetFeature.Equals("UPDATE"))
                    {
                        //switch (commandLineInput.TargetFeature)
                        //{
                        //    case "FIRMWAREUPDATE":
                        //    case "UODFWUPDATE":
                        //    case "LOCKUIUPDATE":
                        //    case "UNLOCKUIUPDATE":
                        writelog("FIRMWAREUPDATE GET UPDATE Entry");
                        var ret = SWAPPUpdate_get(commandLineInput);
                        writelog("FIRMWAREUPDATE GET UPDATE DONE");
                        result.ExitCode = ret.code;
                        result.serialize_Json_response = ret.json;
                        return result;
                        //}
                    }
                }
                if (commandLineInput.Command.Equals("GET") && commandLineInput.TargetType.Equals("APP") &&
                    commandLineInput.TargetFeature.Equals("UPDATESOURCELOCATION"))
                {
                    //switch (commandLineInput.TargetFeature)
                    //{
                    //    case "FIRMWAREUPDATE":
                    //    case "UODFWUPDATE":
                    //    case "LOCKUIUPDATE":
                    //    case "UNLOCKUIUPDATE":
                    writelog("FIRMWAREUPDATE GET UPDATESOURCELOCATION Entry");
                    var ret = SWAPPUpdate_get(commandLineInput);
                    writelog("FIRMWAREUPDATE GET UPDATESOURCELOCATION DONE");
                    result.ExitCode = ret.code;
                    result.serialize_Json_response = ret.json;
                    return result;
                    //}
                }
                if (commandLineInput.Command.Equals("SET") &&
                    commandLineInput.TargetType.Equals("APP") &&
                    commandLineInput.TargetFeature.Equals("UPDATESOURCELOCATION"))
                {
                    //switch (commandLineInput.TargetFeature)
                    //{
                    //    case "FIRMWAREUPDATE":
                    //    case "UODFWUPDATE":
                    //    case "LOCKUIUPDATE":
                    //    case "UNLOCKUIUPDATE":
                    writelog("FIRMWAREUPDATE GET UPDATESOURCELOCATION Entry");
                    var ret = SWAPPUpdate(commandLineInput);
                    writelog("FIRMWAREUPDATE GET UPDATESOURCELOCATION DONE");
                    result.ExitCode = ret.code;
                    result.serialize_Json_response = ret.json;
                    return result;
                    //}
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
                writelog("FIRMWAREUPDATE Un-supported command");
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
                writelog("FIRMWAREUPDATE Empty command input");
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
                GetResults.Add(new CLI_PeripheralRESPONSE("N/A", "GET", _commandLineInput.TargetFeature, "Fail", "Device not found"));
                writelog("GetPeripheralProperty: " + _commandLineInput.TargetFeature + "Device not found");
                return (int)CLI_ExitCode.fail_GetPeripheralProperty_NoConnectDevice;
            }

            if (_commandLineInput.GuidString.Count == 0 && _commandLineInput.Model.Count == 0 && _commandLineInput.ServiceTag.Count == 0 && _commandLineInput.PPID.Count == 0 && _commandLineInput.SerialNumber.Count == 0)
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
                    writelog("GetPeripheralProperty: " + _commandLineInput.TargetFeature + "Device not found");
                }
            }
            else if (_commandLineInput.GuidString.Count != 0)
            {
                _commandLineInput.GuidString.ForEach(x =>
                {
                    if (Guid.TryParse(x, out Guid guid))
                    {
                        var di = _deviceinfo.FirstOrDefault(x => x.ID == guid && x.LogicalDeviceType.ToUpper().Contains(_commandLineInput.PluginsType));
                        if (di == null)
                        {
                            GetResults.Add(new CLI_PeripheralRESPONSE(x, "GET", _commandLineInput.TargetFeature, "Fail", "Device not found"));
                            writelog("GetPeripheralProperty: " + _commandLineInput.TargetFeature + "Device not found");
                        }
                        else
                        {
                            GetResults.Add(new CLI_PeripheralRESPONSE(_devMgr, di, _commandLineInput.PluginsType, _commandLineInput.TargetFeature));
                        }
                    }
                    else
                    {
                        GetResults.Add(new CLI_PeripheralRESPONSE(x, "GET", _commandLineInput.TargetFeature, "Fail", "Invalid Guid"));
                        writelog("GetPeripheralProperty: " + _commandLineInput.TargetFeature + "Invalid Guid");
                    }
                });
            }
            else if (_commandLineInput.Model.Count != 0)
            {
                _commandLineInput.Model.ForEach(model =>
                {
                    var di = _deviceinfo.FirstOrDefault(_ => _.ModelNumber.ToLower() == model.ToLower() && _.LogicalDeviceType.ToUpper().Contains(_commandLineInput.PluginsType));
                    if (di == null)
                    {
                        GetResults.Add(new CLI_PeripheralRESPONSE("N/A", "GET", _commandLineInput.TargetFeature, "Fail", "Device not found", null, model));
                        writelog("GetPeripheralProperty: " + _commandLineInput.TargetFeature + "Device not found");
                    }
                    else
                    {
                        GetResults.Add(new CLI_PeripheralRESPONSE(_devMgr, di, _commandLineInput.PluginsType, _commandLineInput.TargetFeature));
                    }
                });
            }
            else if (_commandLineInput.ServiceTag.Count != 0)
            {
                _commandLineInput.ServiceTag.ForEach(serviceTag =>
                {
                    var di = _deviceinfo.FirstOrDefault(_ => _.DockServiceTag == serviceTag && _.LogicalDeviceType.ToUpper().Contains(_commandLineInput.PluginsType));
                    if (di == null)
                    {
                        GetResults.Add(new CLI_PeripheralRESPONSE("N/A", "GET", _commandLineInput.TargetFeature, "Fail", "Device not found", null, null, serviceTag));
                        writelog("GetPeripheralProperty: " + _commandLineInput.TargetFeature + "Device not found");
                    }
                    else
                    {
                        GetResults.Add(new CLI_PeripheralRESPONSE(_devMgr, di, _commandLineInput.PluginsType, _commandLineInput.TargetFeature));
                    }
                });
            }
            else if (_commandLineInput.PPID.Count != 0)
            {

            }
            else if (_commandLineInput.SerialNumber.Count != 0)
            {

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
            //string GUID = string.Empty;
            bool retcode = false;
            bool retcode_ = false;
            //bool target = false;
            DDPMSettings data = _devMgr.ReloadAppConfigData().Result;

            if (data == null)
            {
                writelog("SetPeripheralProperty: DDPMSettings is null");
                return (int)CLI_ExitCode.functional_error;
            }

            if (_devMgr == null)
            {
                writelog("SetPeripheralProperty: input null IDeviceManagerSA");
                return (int)CLI_ExitCode.null_device_manager;
            }
            _deviceinfo = _devMgr.GetDevices().Result.deviceInfo;
            if (_commandLineInput.GuidString.Count == 0 && _commandLineInput.Model.Count == 0 && _commandLineInput.ServiceTag.Count == 0 && _commandLineInput.PPID.Count == 0 && _commandLineInput.SerialNumber.Count == 0)
            {
                int go = 0;
                if (_deviceinfo == null || _deviceinfo.Count == 0)
                {
                    writelog("SetPeripheralProperty: Device not found");
                    SetResults.Add(new CLI_PeripheralRESPONSE("N/A", _commandLineInput.Command, _commandLineInput.TargetFeature, "FAIL", "Device not found"));
                    return (int)CLI_ExitCode.fail_SetPeripheralProperty;
                }
                _deviceinfo.ForEach(x =>
                {
                    if (x.LogicalDeviceType.ToUpper().Contains(_commandLineInput.PluginsType))
                    {
                        SetResults.Add(new CLI_PeripheralRESPONSE($"{x.ID}", _commandLineInput.Command, _commandLineInput.TargetFeature, "", "", x.Name, x.ModelNumber, x.DockServiceTag));
                        //GUID = x.ID.ToString();
                        go++;
                    }
                });
                if (go == 0)
                {
                    writelog("SetPeripheralProperty: Device not found");
                    SetResults.Add(new CLI_PeripheralRESPONSE("N/A", "SET", _commandLineInput.TargetFeature, "Fail", "Device not found"));
                    return (int)CLI_ExitCode.fail_GetPeripheralProperty;
                }
            }
            else if (_commandLineInput.GuidString.Count != 0)
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
                                SetResults.Add(new CLI_PeripheralRESPONSE($"{x.ID}", _commandLineInput.Command, _commandLineInput.TargetFeature, "", "", x.Name, x.ModelNumber, x.DockServiceTag));
                                found = true;
                                //GUID = x.ID.ToString();
                            }
                        });
                        if (!found)
                        {
                            writelog("SetPeripheralProperty: " + guid + "FAIL Device not found");
                            SetResults.Add(new CLI_PeripheralRESPONSE($"{guid}", _commandLineInput.Command, _commandLineInput.TargetFeature, "FAIL", "Device not found"));
                        }
                    }
                    else
                    {
                        writelog("SetPeripheralProperty: FAIL Invalid Guid");
                        SetResults.Add(new CLI_PeripheralRESPONSE(x, _commandLineInput.Command, _commandLineInput.TargetFeature, "FAIL", "Invalid Guid"));
                    }
                });
            }
            else if (_commandLineInput.Model.Count != 0)
            {
                _commandLineInput.Model.ForEach(model =>
                {
                    var found = false;
                    _deviceinfo?.ForEach(x =>
                    {
                        if (x.LogicalDeviceType.ToUpper().Contains(_commandLineInput.PluginsType) && x.ModelNumber.ToLower() == model.ToLower())
                        {
                            SetResults.Add(new CLI_PeripheralRESPONSE($"{x.ID}", _commandLineInput.Command, _commandLineInput.TargetFeature, "", "", x.Name, x.ModelNumber, x.DockServiceTag));
                            found = true;
                            //GUID = x.ID.ToString();
                        }
                    });
                    if (!found)
                    {
                        writelog("SetPeripheralProperty: FAIL Device not found");
                        SetResults.Add(new CLI_PeripheralRESPONSE("N/A", _commandLineInput.Command, _commandLineInput.TargetFeature, "FAIL", "Device not found", null, model));
                    }
                });
            }
            else if (_commandLineInput.ServiceTag.Count != 0)
            {
                _commandLineInput.ServiceTag.ForEach(serviceTag =>
                {
                    var found = false;
                    _deviceinfo?.ForEach(x =>
                    {
                        if (x.LogicalDeviceType.ToUpper().Contains(_commandLineInput.PluginsType) && x.DockServiceTag == serviceTag)
                        {
                            SetResults.Add(new CLI_PeripheralRESPONSE($"{x.ID}", _commandLineInput.Command, _commandLineInput.TargetFeature, "", "", x.Name, x.ModelNumber, x.DockServiceTag));
                            found = true;
                            //GUID = x.ID.ToString();
                        }
                    });
                    if (!found)
                    {
                        writelog("SetPeripheralProperty: FAIL Device not found");
                        SetResults.Add(new CLI_PeripheralRESPONSE("N/A", _commandLineInput.Command, _commandLineInput.TargetFeature, "FAIL", "Device not found", null, null, serviceTag));
                    }
                });
            }
            else if (_commandLineInput.PPID.Count != 0)
            {
                _commandLineInput.PPID.ForEach(ppid =>
                {
                    var found = false;

                    // waiting for DeviceInfo.PPID implement

                    //_deviceinfo?.ForEach(x =>
                    //{
                    //    if (x.LogicalDeviceType.ToUpper().Contains(_commandLineInput.PluginsType) && x.PPID == ppid)
                    //    {
                    //        SetResults.Add(new CLI_PeripheralRESPONSE($"{x.ID}", _commandLineInput.Command, _commandLineInput.TargetFeature, "", "", x.Name, x.ModelNumber, x.DockServiceTag));
                    //        found = true;
                    //        //GUID = x.ID.ToString();
                    //    }
                    //});

                    if (!found)
                    {
                        writelog("SetPeripheralProperty: FAIL Device not found");
                        SetResults.Add(new CLI_PeripheralRESPONSE("N/A", _commandLineInput.Command, _commandLineInput.TargetFeature, "FAIL", "Device not found"));
                    }
                });
            }
            else if (_commandLineInput.SerialNumber.Count != 0)
            {
                _commandLineInput.SerialNumber.ForEach(serialNumber =>
                {
                    var found = false;

                    // waiting for DeviceInfo.SerialNumber implement

                    //_deviceinfo?.ForEach(x =>
                    //{
                    //    if (x.LogicalDeviceType.ToUpper().Contains(_commandLineInput.PluginsType) && x.SerialNumber == serialNumber)
                    //    {
                    //        SetResults.Add(new CLI_PeripheralRESPONSE($"{x.ID}", _commandLineInput.Command, _commandLineInput.TargetFeature, "", "", x.Name, x.ModelNumber, x.DockServiceTag));
                    //        found = true;
                    //        //GUID = x.ID.ToString();
                    //    }
                    //});

                    if (!found)
                    {
                        writelog("SetPeripheralProperty: FAIL Device not found");
                        SetResults.Add(new CLI_PeripheralRESPONSE("N/A", _commandLineInput.Command, _commandLineInput.TargetFeature, "FAIL", "Device not found"));
                    }
                });
            }

            if (_commandLineInput.TargetFeature.Equals("RESTOREFACTORYDEFAULTS") && _commandLineInput.TargetType.Equals("AUDIO"))// for audio headset RESTOREFACTORYDEFAULTS.
            {
                if (_recode_head)
                {
                    SetResults.ForEach(x =>
                    {
                        x.Value = "";
                        if (x.Result == "" && _deviceinfo.FirstOrDefault(_ => _.ID.ToString() == x.Guid).LogicalDeviceType.Equals("LogicalHeadset", StringComparison.OrdinalIgnoreCase))
                        {
                            writelog("SetPeripheralProperty: RESTOREFACTORYDEFAULTS HEADSET Entry");
                            var result = RunAsyncTimeout(_devMgr.SetFactoryResetAsyncValueForHeadsetForCLI(x.Guid, true)).Result;
                            writelog("SetPeripheralProperty: RESTOREFACTORYDEFAULTS HEADSET DONE");
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

                if (_recode_speak)
                {
                    SetResults.ForEach(x =>
                    {
                        x.Value = "";
                        if (x.Result == "" && _deviceinfo.FirstOrDefault(_ => _.ID.ToString() == x.Guid).LogicalDeviceType.Equals("LogicalWiredAudio", StringComparison.OrdinalIgnoreCase))
                        {
                            writelog("SetPeripheralProperty: RESTOREFACTORYDEFAULTS LOGICALWIREDAUDIO Entry");
                            var result = RunAsyncTimeout(_devMgr.SetResetToDefaultAsyncForSoundbar(x.Guid, true)).Result;
                            writelog("SetPeripheralProperty: RESTOREFACTORYDEFAULTS LOGICALWIREDAUDIO DONE");
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

                if (_recode_air_audio)
                {
                    SetResults.ForEach(x =>
                    {
                        x.Value = "";
                        if (x.Result == "" && _deviceinfo.FirstOrDefault(_ => _.ID.ToString() == x.Guid).LogicalDeviceType.Equals("LogicalAirAudio", StringComparison.OrdinalIgnoreCase))
                        {
                            writelog("SetPeripheralProperty: RESTOREFACTORYDEFAULTS LOGICALAIRAUDIO Entry");
                            var result = RunAsyncTimeout(_devMgr.SetFactoryResetAsyncValueForAirAudioAsync(x.Guid, true)).Result;
                            writelog("SetPeripheralProperty: RESTOREFACTORYDEFAULTS LOGICALAIRAUDIO DONE");
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
                }

                writelog("SetPeripheralProperty: RESTOREFACTORYDEFAULTS" + (retcode ? "SUCCESS" : "FAIL"));
                return (retcode) ? (int)CLI_ExitCode.success : (int)CLI_ExitCode.functional_error;
            }

            if (_commandLineInput.TargetFeature.Equals("RESTOREFACTORYDEFAULTS") && _commandLineInput.TargetType.Equals("WEBCAM"))// for audio headset RESTOREFACTORYDEFAULTS.
            {
                SetResults.ForEach(x =>
                {
                    x.Value = "";
                    if (x.Result == "")
                    {
                        writelog("SetPeripheralProperty: RESTOREFACTORYDEFAULTS WEBCAM Entry");
                        var result = RunAsyncTimeout(_devMgr.ResetToDefault_webcam(x.Guid, true)).Result;
                        writelog("SetPeripheralProperty: RESTOREFACTORYDEFAULTS WEBCAM DONE");
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
                writelog("SetPeripheralProperty: RESTOREFACTORYDEFAULTS" + (retcode ? "SUCCESS" : "FAIL"));
                return (retcode) ? (int)CLI_ExitCode.success : (int)CLI_ExitCode.functional_error;
            }

            if (_commandLineInput.TargetFeature.Equals("RESTOREFACTORYDEFAULTS") && _commandLineInput.TargetType.Equals("KEYBOARD"))
            {
                SetResults.ForEach(x =>
                {
                    x.Value = "";
                    if (x.Result == "")
                    {
                        writelog("SetPeripheralProperty: RESTOREFACTORYDEFAULTS KEBOARD Entry");
                        var result = RunAsyncTimeout(_devMgr.RestoreToDefaultKB(x.Guid)).Result;
                        writelog("SetPeripheralProperty: RESTOREFACTORYDEFAULTS KEBOARD Entry");
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
                writelog("SetPeripheralProperty: RESTOREFACTORYDEFAULTS" + (retcode ? "SUCCESS" : "FAIL"));
                return (retcode) ? (int)CLI_ExitCode.success : (int)CLI_ExitCode.functional_error;
            }

            if (_commandLineInput.TargetFeature.Equals("RESTOREFACTORYDEFAULTS") && _commandLineInput.TargetType.Equals("MOUSE"))
            {
                SetResults.ForEach(x =>
                {
                    x.Value = "";
                    if (x.Result == "")
                    {
                        writelog("SetPeripheralProperty: RESTOREFACTORYDEFAULTS Mouse Entry");
                        var result = RunAsyncTimeout(_devMgr.RestoreToDefaultMouse(x.Guid)).Result;
                        writelog("SetPeripheralProperty: RESTOREFACTORYDEFAULTS Mouse Entry");
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
                writelog("SetPeripheralProperty: RESTOREFACTORYDEFAULTS" + (retcode ? "SUCCESS" : "FAIL"));
                return (retcode) ? (int)CLI_ExitCode.success : (int)CLI_ExitCode.functional_error;
            }

            if (_commandLineInput.TargetFeature.Equals("RESTOREFACTORYDEFAULTS") && _commandLineInput.TargetType.Equals("PEN"))
            {
                SetResults.ForEach(x =>
                {
                    x.Value = "";
                    if (x.Result == "")
                    {
                        writelog("SetPeripheralProperty: RESTOREFACTORYDEFAULTS Pen Entry");
                        var result = RunAsyncTimeout(_devMgr.RestoreToDefaultPen()).Result;
                        writelog("SetPeripheralProperty: RESTOREFACTORYDEFAULTS Pen Entry");
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
                writelog("SetPeripheralProperty: RESTOREFACTORYDEFAULTS" + (retcode ? "SUCCESS" : "FAIL"));
                return (retcode) ? (int)CLI_ExitCode.success : (int)CLI_ExitCode.functional_error;
            }

            foreach (var op in _commandLineInput.Options)
            {
                if (op.Option_Name.ToUpper() == "VALUE")
                {
                    //op.Option_Value.Replace(".", ",");
                    List<string> op_value = op.Option_Value.Replace(".", ",").Split(",").ToList();
                    //value = op_value[0];
                    foreach (string stringValue in op_value)
                    {
                        Debug.WriteLine($"{stringValue}, {_commandLineInput.Options[0].Option_Value}");
                        if (stringValue == null) // if there is no option value
                        {
                            SetFailResults("no setting value");
                            writelog("SetPeripheralProperty: no setting value");
                            return (int)CLI_ExitCode.fail_SetPeripheralProperty_Value;
                        }
                        else if (int.TryParse(stringValue, out int tmp)) // for the function argument is number
                        {
                            val = tmp;
                        }
                        else //for the function argument is int(0/1) or bool
                        {
                            switch (stringValue)
                            {
                                //case "ENABLE": //spec is only defined enable/disable, on/off
                                case "ON":
                                    val = 1;
                                    bl = true;
                                    break;
                                //case "DISABLE":
                                case "OFF":
                                    val = 0;
                                    bl = false;
                                    break;
                                case "LOCK":
                                case "UNLOCK":
                                    if (stringValue.ToUpper().Equals("LOCK"))
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
                                    if (stringValue.ToUpper().Equals("UNLOCK"))
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
                                    if (int.TryParse(stringValue, out val))
                                        break;
                                    else
                                    {
                                        SetFailResults("Invalid setting value");
                                        writelog("SetPeripheralProperty: no setting value");
                                        return (int)CLI_ExitCode.fail_SetPeripheralProperty_Value;
                                    }
                            }
                        }
                    }
                    break;
                }
            }

            if (_commandLineInput.TargetFeature != "UNPAIR" && _commandLineInput.Options.Count != 0 && _commandLineInput.Options[0].Option_Value == "")
            {
                SetResults.ForEach(x =>
                        {
                            x.Result = "FAIL";
                            x.Message += (x.Message == "" ? "" : ", ") + "Missing setting value";
                        });
                writelog("SetPeripheralProperty: Missing setting value");
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
                    writelog("SetPeripheralProperty: COLLABCAMERAENABLE Entry");
                    SetResults.ForEach(x =>
                    {
                        x.Value = "";
                        if (x.Result == "")
                        {
                            x.Value = "Not supported";
                            x.Result = "FAIL";
                            x.Message = "Keyboard not support COLLABCAMERAENABLE";
                            retcode = false;
                            writelog("SetPeripheralProperty: FAIL Keyboard not support COLLABCAMERAENABLE");
                            //var di = _deviceinfo.Where(_ => _.ID.ToString() == x.Guid && _.LogicalDeviceType.ToUpper().Contains(_commandLineInput.PluginsType)).FirstOrDefault();
                            //if (di.IsCollabsKeysSupported)
                            //{
                            //    var result = RunAsyncTimeout(_devMgr.SetCollaborationCameraEnable(bl, Guid.Parse(x.Guid))).Result;
                            //    if (result == "0")
                            //    {
                            //        x.Result = "PASS";
                            //        retcode = di.IsCollaborationCameraEnable;
                            //        x.Value = (retcode) ? "ON" : "OFF";
                            //        x.Message = "N/A";
                            //    }
                            //    else if (result == "1")
                            //    {
                            //        x.Result = "FAIL";
                            //        x.Message = "Timeout";
                            //    }
                            //    else
                            //    {
                            //        x.Result = "FAIL";
                            //        x.Message = result;
                            //    }
                            //    retcode = (result == "0") ? true : false;
                            //}
                            //else
                            //{
                            //    x.Value = "Not supported";
                            //    x.Result = "FAIL";
                            //    x.Message = "Keyboard not support COLLABCAMERAENABLE";
                            //    retcode = false;
                            //    writelog("SetPeripheralProperty: FAIL Keyboard not support COLLABCAMERAENABLE");
                            //}
                        }
                    });
                    writelog("SetPeripheralProperty: COLLABCAMERAENABLE" + (retcode ? "SUCCESS" : "FAIL"));
                    return retcode ? (int)CLI_ExitCode.success : (int)CLI_ExitCode.functional_error;
                case "COLLABCHATENABLE":
                    writelog("SetPeripheralProperty: COLLABCHATENABLE Entry");
                    SetResults.ForEach(x =>
                    {
                        x.Value = "";
                        if (x.Result == "")
                        {
                            x.Value = "Not supported";
                            x.Result = "FAIL";
                            x.Message = "Keyboard not support COLLABCHATENABLE";
                            retcode = false;
                            writelog("SetPeripheralProperty: FAIL Keyboard not support COLLABCHATENABLE");
                            //var di = _deviceinfo.Where(_ => _.ID.ToString() == x.Guid && _.LogicalDeviceType.ToUpper().Contains(_commandLineInput.PluginsType)).FirstOrDefault();
                            //if (di.IsCollabsKeysSupported)
                            //{
                            //    var result = RunAsyncTimeout(_devMgr.SetCollaborationChatEnable(bl, Guid.Parse(x.Guid))).Result;
                            //    if (result == "0")
                            //    {
                            //        x.Result = "PASS";
                            //        retcode = di.IsCollaborationChatEnable;
                            //        x.Value = (retcode) ? "ON" : "OFF";
                            //        x.Message = "N/A";
                            //    }
                            //    else if (result == "1")
                            //    {
                            //        x.Result = "FAIL";
                            //        x.Message = "Timeout";
                            //    }
                            //    else
                            //    {
                            //        x.Result = "FAIL";
                            //        x.Message = result;
                            //    }
                            //    retcode = (result == "0") ? true : false;
                            //}
                            //else
                            //{
                            //    x.Value = "Not supported";
                            //    x.Result = "FAIL";
                            //    x.Message = "Keyboard not support COLLABCHATENABLE";
                            //    retcode = false;
                            //    writelog("SetPeripheralProperty: FAIL Keyboard not support COLLABCHATENABLE");
                            //}
                        }
                    });
                    writelog("SetPeripheralProperty: COLLABCHATENABLE" + (retcode ? "SUCCESS" : "FAIL"));
                    return retcode ? (int)CLI_ExitCode.success : (int)CLI_ExitCode.functional_error;
                //case "COLLABORATIONDOUBLETAPENABLE":
                //    taskB = _devMgr.SetCollaborationDoubleTapEnable;
                //    RunTaskB(bl);
                //    return (int)CLI_ExitCode.success;
                //case "COLLABORATIONKEYENABLE":
                //    taskB = _devMgr.SetCollaborationKeyEnable;
                //    RunTaskB(bl);
                //    return (int)CLI_ExitCode.success;
                case "COLLABMICMUTE":
                    writelog("SetPeripheralProperty: COLLABMICMUTE Entry");
                    SetResults.ForEach(x =>
                    {
                        x.Value = "";
                        if (x.Result == "")
                        {
                            x.Value = "Not supported";
                            x.Result = "FAIL";
                            x.Message = "Keyboard not support COLLABMICMUTE";
                            retcode = false;
                            writelog("SetPeripheralProperty: FAIL Keyboard not support COLLABMICMUTE");
                            //var di = _deviceinfo.Where(_ => _.ID.ToString() == x.Guid && _.LogicalDeviceType.ToUpper().Contains(_commandLineInput.PluginsType)).FirstOrDefault();
                            //if (di.IsCollabsKeysSupported)
                            //{
                            //    var result = RunAsyncTimeout(_devMgr.SetCollaborationMicEnable(bl, Guid.Parse(x.Guid))).Result;
                            //    if (result == "0")
                            //    {
                            //        x.Result = "PASS";
                            //        retcode = di.IsCollaborationMicEnable;
                            //        x.Value = (retcode) ? "ON" : "OFF";
                            //        x.Message = "N/A";
                            //    }
                            //    else if (result == "1")
                            //    {
                            //        x.Result = "FAIL";
                            //        x.Message = "Timeout";
                            //    }
                            //    else
                            //    {
                            //        x.Result = "FAIL";
                            //        x.Message = result;
                            //    }
                            //    retcode = (result == "0") ? true : false;
                            //}
                            //else
                            //{
                            //    x.Value = "Not supported";
                            //    x.Result = "FAIL";
                            //    x.Message = "Keyboard not support COLLABMICMUTE";
                            //    retcode = false;
                            //    writelog("SetPeripheralProperty: FAIL Keyboard not support COLLABMICMUTE");
                            //}
                        }
                    });
                    writelog("SetPeripheralProperty: COLLABMICMUTE" + (retcode ? "SUCCESS" : "FAIL"));
                    return retcode ? (int)CLI_ExitCode.success : (int)CLI_ExitCode.functional_error;
                case "COLLABSCREENSHARE":
                    writelog("SetPeripheralProperty: COLLABSCREENSHARE Entry");
                    SetResults.ForEach(x =>
                    {
                        x.Value = "";
                        if (x.Result == "")
                        {
                            var di = _deviceinfo.FirstOrDefault(_ => _.ID.ToString() == x.Guid && _.LogicalDeviceType.ToUpper().Contains(_commandLineInput.PluginsType));
                            if (di.IsCollabsKeysSupported)
                            {
                                var result = RunAsyncTimeout(_devMgr.SetCollaborationScreenShareEnable(bl, Guid.Parse(x.Guid))).Result;
                                if (result == "0")
                                {
                                    x.Result = "PASS";
                                    retcode = di.IsCollaborationScreenShareEnable;
                                    x.Value = (retcode) ? "ON" : "OFF";
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
                            else
                            {
                                x.Value = "Not supported";
                                x.Result = "FAIL";
                                x.Message = "Keyboard not support COLLABSCREENSHARE";
                                retcode = false;
                                writelog("SetPeripheralProperty: FAIL Keyboard not support COLLABSCREENSHARE");
                            }
                        }
                    });
                    writelog("SetPeripheralProperty: COLLABSCREENSHARE" + (retcode ? "SUCCESS" : "FAIL"));
                    return retcode ? (int)CLI_ExitCode.success : (int)CLI_ExitCode.functional_error;
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
                    writelog("SetPeripheralProperty: ANCMODE Entry");
                    SetResults.ForEach(x =>
                    {
                        x.Value = "";
                        if (x.Result == "")
                        {
                            if (_devMgr.GetIsANCSupportedAsync(x.Guid).Result)
                            {
                                if (val == 1)
                                    data.LockSettings.Lock_Audio_ancMode = false;
                                else
                                    data.LockSettings.Lock_Audio_ancMode = true;
                                _devMgr.SetAppConfigData(data);
                                taskA = _devMgr.SetAncMode;
                                RunTaskA(val);
                                retcode = true;
                                //var result = RunAsyncTimeout(_devMgr.SetIsHDROn(x.Guid, bl)).Result;
                                //if (result == "0")
                                //{
                                //    x.Result = "PASS";
                                //    retcode_ = _devMgr.GetIsHDROn(x.Guid).Result;
                                //    x.Value = (retcode_) ? "ON" : "OFF";
                                //    x.Value += "," + (data.LockSettings.Lock_Webcam_hdr ? "LOCK" : "UNLOCK");
                                //    x.Message = "N/A";
                                //}
                                //else if (result == "1")
                                //{
                                //    x.Result = "FAIL";
                                //    x.Message = "Timeout";
                                //}
                                //else
                                //{
                                //    x.Result = "FAIL";
                                //    x.Message = result;
                                //}
                                //retcode = (result == "0") ? true : false;
                            }
                            else
                            {
                                x.Value = "Not supported";
                                x.Result = "FAIL";
                                x.Message = "HeadSet not support ANC";
                                retcode = false;
                                writelog("SetPeripheralProperty: FAIL HeadSet not support ANC");
                            }
                        }
                    });
                    writelog("SetPeripheralProperty: ANCMODE" + (retcode ? "SUCCESS" : "FAIL"));
                    return retcode ? (int)CLI_ExitCode.success : (int)CLI_ExitCode.functional_error;
                //if (val == 1)
                //    data.LockSettings.Lock_Audio_ancMode = false;
                //else
                //    data.LockSettings.Lock_Audio_ancMode = true;
                //_devMgr.SetAppConfigData(data);
                //taskA = _devMgr.SetAncMode;
                //RunTaskA(val);
                //return (int)CLI_ExitCode.success;
                case "SETANCGAIN":
                    writelog("SetPeripheralProperty: SETANCGAIN Entry");
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
                    SetResults.ForEach(x =>
                    {
                        x.Value = "";
                        if (x.Result == "")
                        {
                            var isAirAudio = _deviceinfo.FirstOrDefault(_ => _.ID.ToString() == x.Guid).LogicalDeviceType.Equals("LogicalAirAudio", StringComparison.OrdinalIgnoreCase);
                            writelog("MICNOISECANCELLATION Entry");

                            var support = isAirAudio
                                ? _devMgr.GetAirAudioIsMicNoiseCancellationSupportedAsync(x.Guid).Result
                                : _devMgr.GetIsMicNoiseCancellationSupportedAsync(x.Guid).Result;

                            if (support)
                            {
                                writelog($"{x.Model} support MICNOISECANCELLATION");
                                var result = string.Empty;
                                if (x.Model == "WL7024")
                                {
                                    writelog("Entry _devMgr.SetMicNoiseCancellationForMito");
                                    result = RunAsyncTimeout(_devMgr.SetMicNoiseCancellationForMito(bl, Guid.Parse(x.Guid))).Result;
                                    writelog("Exit _devMgr.SetMicNoiseCancellationForMito");
                                }
                                else if (isAirAudio)
                                {
                                    writelog("Entry _devMgr.SetAirAudioMicNoiseCancellationAsync");
                                    result = RunAsyncTimeout(_devMgr.SetAirAudioMicNoiseCancellationAsync(x.Guid, bl)).Result;
                                    writelog("Exit _devMgr.SetAirAudioMicNoiseCancellationAsync");
                                }
                                else
                                {
                                    writelog("Entry _devMgr.SetMicNoiseCancellation");
                                    result = RunAsyncTimeout(_devMgr.SetMicNoiseCancellation(bl, Guid.Parse(x.Guid))).Result;
                                    writelog("Exit _devMgr.SetMicNoiseCancellation");
                                }

                                if (result == "0")
                                {
                                    x.Result = "PASS";
                                    x.Value = (isAirAudio
                                        ? _devMgr.GetAirAudioMicNoiseCancellationAsync(x.Guid).Result
                                        : _devMgr.GetMicNoiseCancellationAsync(x.Guid).Result) ? "ON" : "OFF";
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
                                writelog($"Set MICNOISECANCELLATION Result: {x.Result}, Message: {x.Message}");
                                retcode = result == "0";
                            }
                            else
                            {
                                writelog($"{x.Model} not support MICNOISECANCELLATION");
                                x.Value = "Not supported";
                                x.Result = "FAIL";
                                x.Message = "Audio not support MICNOISECANCELLATION";
                                retcode = false;
                            }
                        }
                    });
                    return retcode ? (int)CLI_ExitCode.success : (int)CLI_ExitCode.functional_error;
                //taskB = _devMgr.SetMicNoiseCancellation;
                //RunTaskB(bl);
                //return (int)CLI_ExitCode.success;
                //case "SETSIDETONE":
                //    taskB = _devMgr.SetSidetone;
                //    RunTaskB(bl);
                //    return (int)CLI_ExitCode.success;
                //case "SETSIDETONELEVEL":
                //    taskA = _devMgr.SetSidetoneLevel;
                //    RunTaskA(val);
                //    return (int)CLI_ExitCode.success;
                case "WEARDETECTION":
                    writelog("SetPeripheralProperty: WEARDETECTION Entry");
                    SetResults.ForEach(x =>
                    {
                        x.Value = "";
                        if (x.Result == "")
                        {
                            if (_devMgr.GetIsWearDetectionSupportedAsync(x.Guid).Result)
                            {
                                if (val == 1)
                                    data.LockSettings.Lock_Audio_wearDetection = false;
                                else
                                    data.LockSettings.Lock_Audio_wearDetection = true;
                                _devMgr.SetAppConfigData(data);
                                taskA = _devMgr.SetWearDetectionForCLI;
                                RunTaskA(val);
                                retcode = true;
                            }
                            else
                            {
                                x.Value = "Not supported";
                                x.Result = "FAIL";
                                x.Message = "HeadSet not support WearDetection";
                                retcode = false;
                                writelog("SetPeripheralProperty: FAIL HeadSet not support WearDetection");
                            }
                        }
                    });
                    writelog("SetPeripheralProperty: WEARDETECTION" + (retcode ? "SUCCESS" : "FAIL"));
                    return retcode ? (int)CLI_ExitCode.success : (int)CLI_ExitCode.functional_error;
                //if (val == 1)
                //    data.LockSettings.Lock_Audio_wearDetection = false;
                //else
                //    data.LockSettings.Lock_Audio_wearDetection = true;
                //_devMgr.SetAppConfigData(data);
                //taskA = _devMgr.SetWearDetectionForCLI;
                //RunTaskA(val);
                //return (int)CLI_ExitCode.success;
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
                case "MICSWITCH":
                    writelog("SetPeripheralProperty: MICSWITCH Entry");
                    SetResults.ForEach(x =>
                    {
                        x.Value = "";
                        if (x.Result == "")
                        {
                            var di = _deviceinfo.FirstOrDefault(_ => _.ID.ToString() == x.Guid && _.LogicalDeviceType.ToUpper().Contains(_commandLineInput.PluginsType));
                            if (di.IsMicEnumerationSupported)
                            {
                                var result = RunAsyncTimeout(_devMgr.SetIsMicEnumerationOn(bl, Guid.Parse(x.Guid))).Result;
                                if (result == "0")
                                {
                                    x.Result = "PASS";
                                    retcode = di.IsMicEnumerationOn;
                                    x.Value = (retcode) ? "ON" : "OFF";
                                    //x.Value += "," + (data.LockSettings.Lock_Webcam_MicSwitch ? "LOCK" : "UNLOCK");
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
                            else
                            {
                                x.Value = "Not supported";
                                x.Result = "FAIL";
                                x.Message = "Webcam not support MicSwitch";
                                retcode = false;
                                writelog("SetPeripheralProperty: FAIL Webcam not support MicSwitch");
                            }
                        }
                    });
                    writelog("SetPeripheralProperty: MICSWITCH" + (retcode ? "SUCCESS" : "FAIL"));
                    return retcode ? (int)CLI_ExitCode.success : (int)CLI_ExitCode.functional_error;
                //taskB = _devMgr.SetIsMicEnumerationOn;
                //RunTaskD(bl);
                //return (int)CLI_ExitCode.success;

                case "HDR":
                    writelog("SetPeripheralProperty: HDR Entry");
                    SetResults.ForEach(x =>
                    {
                        x.Value = "";
                        if (x.Result == "")
                        {
                            if (_devMgr.GetIsPropertyHDRSupported(x.Guid).Result)
                            {
                                var result = RunAsyncTimeout(_devMgr.SetIsHDROn(x.Guid, bl)).Result;
                                if (result == "0")
                                {
                                    x.Result = "PASS";
                                    retcode_ = _devMgr.GetIsHDROn(x.Guid).Result;
                                    x.Value = (retcode_) ? "ON" : "OFF";
                                    //x.Value += "," + (data.LockSettings.Lock_Webcam_hdr ? "LOCK" : "UNLOCK");
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
                            else
                            {
                                x.Value = "Not supported";
                                x.Result = "FAIL";
                                x.Message = "Webcam not support HDR";
                                retcode = false;
                                writelog("SetPeripheralProperty: FAIL Webcam not support HDR");
                            }
                        }
                    });
                    writelog("SetPeripheralProperty: HDR" + (retcode ? "SUCCESS" : "FAIL"));
                    return retcode ? (int)CLI_ExitCode.success : (int)CLI_ExitCode.functional_error;
                //ItemId = "DellPeripheral.Webcam.0";
                //if (_devMgr.GetIsPropertyHDRSupported(GUID).Result)
                //{
                //    SetResults.ForEach((Action<CLI_PeripheralRESPONSE>)(x =>
                //    {
                //        x.Value = "";
                //        if (x.Result == "")
                //        {
                //            var result = RunAsyncTimeout(_devMgr.SetIsHDROn(GUID, bl)).Result;
                //            if (result == "0")
                //            {
                //                x.Result = "PASS";
                //                retcode_ = _devMgr.GetIsHDROn(GUID).Result;
                //                x.Value = (retcode_) ? "ON" : "OFF";
                //                x.Value += "," + (data.LockSettings.Lock_Webcam_hdr ? "LOCK" : "UNLOCK");
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
                //            retcode = (result == "0") ? true : false;
                //        }
                //    }));
                //    return (retcode) ? (int)CLI_ExitCode.success : (int)CLI_ExitCode.functional_error;
                //}
                //else
                //{
                //    SetResults.ForEach(x =>
                //    {
                //        x.Value = "N/A";
                //        x.Result = "FAIL";
                //        x.Message = "Webcam not support HDR";
                //    });
                //    return (int)CLI_ExitCode.command_targetfeature_not_support;
                //}

                case "ANTIFLICKER":
                    writelog("SetPeripheralProperty: ANTIFLICKER Entry");
                    SetResults.ForEach(x =>
                    {
                        x.Value = "";
                        if (x.Result == "")
                        {
                            if (val == 50 || val == 60 || val == 1 || val == 2)
                            {
                                if (val == 50)
                                    val = 1;
                                else if (val == 60)
                                    val = 2;

                                if (_devMgr.GetIsPropertyAntiFlickerSupported(x.Guid).Result)
                                {
                                    var result = RunAsyncTimeout(_devMgr.SetAntiFlicker(x.Guid, val)).Result;
                                    if (result == "0")
                                    {
                                        x.Result = "PASS";

                                        try
                                        {
                                            var retvalue = _devMgr.GetAntiFlicker(x.Guid).Result;
                                            if (!string.IsNullOrEmpty(retvalue.ToString()))
                                            {
                                                switch (retvalue.ToString())
                                                {
                                                    case "1":
                                                        x.Value = "50";
                                                        break;
                                                    case "2":
                                                        x.Value = "60";
                                                        break;
                                                    default:
                                                        break;
                                                }
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            x.Value = "Interface return null";
                                            writelog("SetPeripheralProperty: _devMgr.GetAntiFlicker FAIL, message: " + ex.Message);
                                        }

                                        //x.Value += "," + (data.LockSettings.Lock_Webcam_AntiFlicker ? "LOCK" : "UNLOCK");
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
                                else
                                {
                                    x.Value = "Not supported";
                                    x.Result = "FAIL";
                                    x.Message = "Webcam not support AntiFlicker";
                                    retcode = false;
                                    writelog("SetPeripheralProperty: FAIL Webcam not support AntiFlicker");
                                }
                            }
                            else
                            {
                                x.Result = "FAIL";
                                x.Message = "Wrong option value: ";
                                x.Message += $"{val}";
                                retcode = false;
                                writelog("SetPeripheralProperty: Wrong option value:" + val.ToString());
                            }
                        }
                    });
                    writelog("SetPeripheralProperty: ANTIFLICKER" + (retcode ? "SUCCESS" : "FAIL"));
                    return (retcode) ? (int)CLI_ExitCode.success : (int)CLI_ExitCode.functional_error;
                //ItemId = "DellPeripheral.Webcam.0";
                //if (_devMgr.GetIsPropertyAntiFlickerSupported(GUID).Result)
                //{
                //    SetResults.ForEach(x =>
                //    {
                //        x.Value = "";
                //        if (x.Result == "")
                //        {
                //            var result = RunAsyncTimeout(_devMgr.SetAntiFlicker(GUID, val)).Result;
                //            if (result == "0")
                //            {
                //                x.Result = "PASS";
                //                retvalue = _devMgr.GetAntiFlicker(GUID).Result;
                //                x.Value = retvalue.ToString();
                //                x.Value += "," + (data.LockSettings.Lock_Webcam_AntiFlicker ? "LOCK" : "UNLOCK");
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
                //            retcode = (result == "0") ? true : false;
                //        }
                //    });
                //    return (retcode) ? (int)CLI_ExitCode.success : (int)CLI_ExitCode.functional_error;
                //}
                //else
                //{
                //    SetResults.ForEach(x =>
                //    {
                //        x.Value = "N/A";
                //        x.Result = "FAIL";
                //        x.Message = "Webcam not support AntiFlicker";
                //    });
                //    return (int)CLI_ExitCode.command_targetfeature_not_support;
                //}

                case "AIAUTOFRAMING":
                    writelog("SetPeripheralProperty: AIAUTOFRAMING Entry");
                    SetResults.ForEach(x =>
                    {
                        x.Value = "";
                        if (x.Result == "")
                        {
                            if (_devMgr.GetIsPropertyAutoFramingSupported(x.Guid).Result)
                            {
                                var result = RunAsyncTimeout(_devMgr.SetIsAutoFramingOn(x.Guid, bl)).Result;
                                if (result == "0")
                                {
                                    x.Result = "PASS";
                                    retcode_ = _devMgr.GetIsAutoFramingOn(x.Guid).Result;
                                    x.Value = (retcode_) ? "ON" : "OFF";
                                    //x.Value += "," + (data.LockSettings.Lock_Webcam_AIAutoFraming ? "LOCK" : "UNLOCK");
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
                            else
                            {
                                x.Value = "Not supported";
                                x.Result = "FAIL";
                                x.Message = "Webcam not support AI AutoFraming";
                                retcode = false;
                                writelog("SetPeripheralProperty: FAIL Webcam not support AI AutoFraming");
                            }
                        }
                    });
                    writelog("SetPeripheralProperty: AIAUTOFRAMING" + (retcode ? "SUCCESS" : "FAIL"));
                    return (retcode) ? (int)CLI_ExitCode.success : (int)CLI_ExitCode.functional_error;
                //ItemId = "DellPeripheral.Webcam.0";
                //if (_devMgr.GetIsPropertyAutoFramingSupported(GUID).Result)
                //{
                //    SetResults.ForEach(x =>
                //    {
                //        x.Value = "";
                //        if (x.Result == "")
                //        {
                //            var result = RunAsyncTimeout(_devMgr.SetIsAutoFramingOn(GUID, bl)).Result;
                //            if (result == "0")
                //            {
                //                x.Result = "PASS";
                //                retcode_ = _devMgr.GetIsAutoFramingOn(GUID).Result;
                //                x.Value = (retcode_) ? "ON" : "OFF";
                //                x.Value += "," + (data.LockSettings.Lock_Webcam_AIAutoFraming ? "LOCK" : "UNLOCK");
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
                //            retcode = (result == "0") ? true : false;
                //        }
                //    });
                //    return (retcode) ? (int)CLI_ExitCode.success : (int)CLI_ExitCode.functional_error;
                //}
                //else
                //{
                //    SetResults.ForEach(x =>
                //    {
                //        x.Value = "N/A";
                //        x.Result = "FAIL";
                //        x.Message = "Webcam not support AI AutoFraming";
                //    });
                //    return (int)CLI_ExitCode.command_targetfeature_not_support;
                //}
                case "PRESENCEDETECTION":
                    //ItemId = "DellPeripheral.Webcam.0";
                    writelog("SetPeripheralProperty: PRESENCEDETECTION Entry");
                    SetResults.ForEach(x =>
                    {
                        x.Value = "";
                        if (x.Result == "")
                        {
                            var di = _deviceinfo.FirstOrDefault(_ => _.ID.ToString() == x.Guid && _.LogicalDeviceType.ToUpper().Contains(_commandLineInput.PluginsType));
                            if (di.IsESISupported)
                            {
                                var result = RunAsyncTimeout(_devMgr.SetIsProximitySensorEnable(x.Guid, bl)).Result;
                                if (result == "0")
                                {
                                    x.Result = "PASS";
                                    retcode_ = _devMgr.GetIsProximitySensorEnable(x.Guid).Result;
                                    x.Value = (retcode_) ? "ON" : "OFF";
                                    //x.Value += "," + (data.LockSettings.Lock_Webcam_PresenceDetection ? "LOCK" : "UNLOCK");
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
                            else
                            {
                                x.Value = "Not supported";
                                x.Result = "FAIL";
                                x.Message = "Webcam not support PresenceDetection";
                                retcode = false;
                                writelog("SetPeripheralProperty: FAIL Webcam not support PresenceDetection");
                            }
                        }
                    });
                    writelog("SetPeripheralProperty: PRESENCEDETECTION" + (retcode ? "SUCCESS" : "FAIL"));
                    return (retcode) ? (int)CLI_ExitCode.success : (int)CLI_ExitCode.functional_error;

                //case "MICSWITCH_":
                //    //var ItemId_ = "DellPeripheral.Webcam.0";
                //    if (_deviceinfo.FirstOrDefault(x => x.ID.ToString() == GUID)?.IsMicEnumerationSupported == true)
                //    {
                //        SetResults.ForEach(x =>
                //        {
                //            x.Value = "";
                //            if (x.Result == "")
                //            {
                //                var result = RunAsyncTimeout(_devMgr.SetIsMicEnumerationOn(GUID, bl)).Result;
                //                if (result == "0")
                //                {
                //                    x.Result = "PASS";
                //                    retcode_ = _deviceinfo.FirstOrDefault(x => x.ID.ToString() == GUID).IsMicEnumerationOn;
                //                    x.Value = (retcode_) ? "ON" : "OFF";
                //                    x.Value += "," + (data.LockSettings.Lock_Webcam_MicSwitch ? "LOCK" : "UNLOCK");
                //                    x.Message = _deviceinfo.FirstOrDefault(x => x.ID.ToString() == GUID).IsMicEnumerationOn.ToString();
                //                }
                //                else if (result == "1")
                //                {
                //                    x.Result = "FAIL";
                //                    x.Message = "Timeout";
                //                }
                //                else
                //                {
                //                    x.Result = "FAIL";
                //                    x.Message = result;
                //                }
                //                retcode = (result == "0") ? true : false;
                //            }
                //        });
                //        return (retcode) ? (int)CLI_ExitCode.success : (int)CLI_ExitCode.functional_error;
                //    }
                //    else
                //    {
                //        SetResults.ForEach(x =>
                //        {
                //            x.Value = "N/A";
                //            x.Result = "FAIL";
                //            x.Message = "Webcam not support MicSwitch";
                //        });
                //        return (int)CLI_ExitCode.command_targetfeature_not_support;
                //    }
                default:
                    SetFailResults("Invalid TargetFeature");
                    writelog("SetPeripheralProperty: Invalid TargetFeature");
                    return (int)CLI_ExitCode.fail_SetPeripheralProperty_Property;
            }
        }

        private void RunTaskA(int val)
        {
            DDPMSettings data = _devMgr.ReloadAppConfigData().Result;
            SetResults.ForEach(x =>
            {
                x.Value = _commandLineInput.Options[0].Option_Value;

                if (data == null)
                {
                    x.Result = "FAIL";
                    writelog("RunTaskA: DDPMSettings is null");
                }
                else
                {
                    //if (_commandLineInput.TargetFeature.ToUpper().Equals("ANCMODE"))
                    //    x.Value += "," + (data.LockSettings.Lock_Audio_ancMode ? "LOCK" : "UNLOCK");
                    //if (_commandLineInput.TargetFeature.ToUpper().Equals("WEARDETECTION"))
                    //    x.Value += "," + (data.LockSettings.Lock_Audio_wearDetection ? "LOCK" : "UNLOCK");
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
        private void RunTaskD(bool val)
        {
            DDPMSettings data = _devMgr.ReloadAppConfigData().Result;
            SetResults.ForEach(x =>
            {
                x.Value = _commandLineInput.Options[0].Option_Value;

                if (data == null)
                {
                    x.Result = "FAIL";
                    writelog("RunTaskD: DDPMSettings is null");
                }
                else
                {
                    if (x.Result == "")
                    {
                        var result = RunAsyncTimeout(taskB(val, Guid.Parse(x.Guid))).Result;
                        if (result == "0")
                        {
                            x.Result = "PASS";
                            //x.Value += "," + (data.LockSettings.Lock_Webcam_MicSwitch ? "LOCK" : "UNLOCK");
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
            if (Log != null)
            {
                if (log_type == log_type.info)
                    Log.Info(text);
                else
                    Log.Error(text);
            }
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

        private (int code, string json) NoDeviceConnectResponse(CommandLineInput commandLineInput)
        {
            CLI_RESPONSE rsp = new CLI_RESPONSE()
            {
                Command = commandLineInput.Command,
                TargetFeature = commandLineInput.TargetFeature,
                Result = "FAIL",
                Message = "No device connected",
            };
            writelog("No Device Connect");
            System.Console.WriteLine(JsonConvert.SerializeObject(rsp, Formatting.Indented));
            return ((int)CLI_ExitCode.fail_FWUpdate, JsonConvert.SerializeObject(rsp, Formatting.Indented));
        }

        private (int code, string json) NoupdateResponse(CommandLineInput commandLineInput)
        {
            CLI_RESPONSE rsp = new CLI_RESPONSE()
            {
                Command = commandLineInput.Command,
                TargetFeature = commandLineInput.TargetFeature,
                Result = "FAIL",
                Message = "No updates available",
            };
            writelog("No Device Connect");
            System.Console.WriteLine(JsonConvert.SerializeObject(rsp, Formatting.Indented));
            return ((int)CLI_ExitCode.fail_FWUpdate, JsonConvert.SerializeObject(rsp, Formatting.Indented));
        }

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

            List<DeviceInfo> deviceInfoList = null;
            deviceInfoList = _devMgr.GetDevices().Result.deviceInfo;

            if (!commandLineInput.isCliRunAdmin)
            {
                cLI_RESPONSE.Result = "FAIL";
                cLI_RESPONSE.Message = "Not Admin";
                Console.WriteLine(JsonConvert.SerializeObject(cLI_RESPONSE, Formatting.Indented));
                writelog("FIRMWAREUPDATE FAIL Not Admin");
                return ((int)CLI_ExitCode.fail_FWUpdate, JsonConvert.SerializeObject(cLI_RESPONSE, Formatting.Indented));
            }
            bool? ret = null;
            bool isShowInfo = true;
            bool isDefer = false;
            bool isForce = true;
            string installPath = "";
            bool somethingError = false;
            bool somethingnull = false;

            bool _recode_mouse = false;
            bool _recode_kb = false;
            bool _recode_dock = false;
            bool _recode_headset = false;
            bool _recode_webcam = false;
            bool _recode_speaker = false;
            bool _record_soundbar = false;
            bool _recode_pen = false;
            bool _recode_dongle = false;
            bool _dock_silent = false;

            string miniver = null;
            List<string> model = null;
            List<string> guid = null;
            List<string> serviceTag = null;
            bool isUod = false;

            CLI_FWU_RESPONSE cLI_FWU_RESPONSE = null;
            try
            {
                cLI_FWU_RESPONSE = new CLI_FWU_RESPONSE(cLI_RESPONSE);
                //if (commandLineInput.Options.Count > 5)
                //{
                //    cLI_FWU_RESPONSE.Result = "FAIL";
                //    cLI_FWU_RESPONSE.Message = "Bring in extra strings:";
                //    for (int i = 0; i < commandLineInput.Options.Count; i++)
                //    {
                //        cLI_FWU_RESPONSE.Message += $"{commandLineInput.Options[i].Option_Name}={commandLineInput.Options[i].Option_Value}\n";
                //    }
                //}

                #region Parse Dock SilentFwUpdate command to unified format
                if (commandLineInput.TargetType == "DOCK")
                {
                    writelog("FW update DOCK,FORCEWITHNONOTICE");
                    var dockNoCommaOptions = commandLineInput.Options.Where(_ => !_.Option_Value.Contains(',')).ToList();

                    foreach (var dockNoCommaOption in dockNoCommaOptions)
                    {
                        if (dockNoCommaOption.Option_Value.Equals("UOD", StringComparison.OrdinalIgnoreCase))
                        {
                            dockNoCommaOption.Option_Value = "TRUE,UOD";
                        }
                        else if (dockNoCommaOption.Option_Value.Contains(':'))
                        {
                            dockNoCommaOption.Option_Value = dockNoCommaOption.Option_Value + ",FILEPATH";
                        }
                    }
                    _dock_silent = true;
                    commandLineInput.Options.Insert(0, new CommandType_Option("VALUE", "DOCK,FORCEWITHNONOTICE"));
                }
                #endregion

                if (commandLineInput.Options.Count > 0)
                {
                    cLI_FWU_RESPONSE.Value = commandLineInput.Options[0].Option_Value;

                    if (!commandLineInput.Options[0].Option_Value.Contains(','))
                    {
                        commandLineInput.Options[0].Option_Value += ",FORCEWITHNOTICE";
                    }
                    else if(_dock_silent)
                    {
                        commandLineInput.Options.Insert(0, new CommandType_Option("VALUE", "DOCK,FORCEWITHNONOTICE"));
                    }
                    else
                    {
                        commandLineInput.Options[0].Option_Value = commandLineInput.Options[0].Option_Value.Split(",")[0] + ",FORCEWITHNOTICE";
                    }
                    string[] ss_1 = commandLineInput.Options[0].Option_Value.Split(",");

                    if (ss_1.Length == 2)
                    {
                        if (!string.IsNullOrEmpty(ss_1[0]) && !string.IsNullOrEmpty(ss_1[1]))
                        {
                            if (ss_1[0].ToUpper().Equals("DISPLAY"))
                            {
                                _AllInfoMonitors = _devMgr.GetMonitors().Result;

                                var fwUpdateMonitorInfos = _AllInfoMonitors.Select(_ => _).ToList();

                                if ((_AllInfoMonitors == null || _AllInfoMonitors.Count == 0))
                                {
                                    writelog("FWUpdate_Line 1471");
                                    CLI_RESPONSE rsp = new CLI_RESPONSE()
                                    {
                                        Command = commandLineInput.Command,
                                        TargetFeature = commandLineInput.TargetFeature,
                                        Result = "FAIL",
                                        Message = "No monitor connected",
                                    };
                                    writelog("FIRMWAREUPDATE DISPLAY No monitorconnected");
                                    Console.WriteLine(JsonConvert.SerializeObject(rsp, Formatting.Indented));
                                    return ((int)CLI_ExitCode.fail_FWUpdate, JsonConvert.SerializeObject(rsp, Formatting.Indented));
                                }
                                else if (commandLineInput.Options.Count > 1)
                                {
                                    for (int i = 0; i < commandLineInput.Options.Count; i++)
                                    {
                                        if (commandLineInput.Options[i].Option_Value.ToUpper().Contains("SERVICETAG"))
                                        {
                                            string[] ss_tag = commandLineInput.Options[i].Option_Value.Split(",");

                                            if (ss_tag.Length == 2 &&
                                                !string.IsNullOrEmpty(ss_tag[0]) &&
                                                !string.IsNullOrEmpty(ss_tag[1]))
                                            {
                                                var findDevice = false;
                                                if (_AllInfoMonitors.Any(x => x.edid.ServiceTag.ToUpper().Equals(ss_tag[0].ToUpper())))
                                                {
                                                    serviceTag = new List<string> { ss_tag[0] };
                                                    fwUpdateMonitorInfos = fwUpdateMonitorInfos.Where(x => x.edid.ServiceTag.ToUpper().Equals(ss_tag[0].ToUpper())).ToList();
                                                    findDevice = true;
                                                }
                                                if (!findDevice)
                                                {
                                                    return NoDeviceConnectResponse(commandLineInput);
                                                }

                                            }
                                        }
                                        if (commandLineInput.Options[i].Option_Value.ToUpper().Contains("MINIVERSION"))
                                        {
                                            string[] ss_min = commandLineInput.Options[i].Option_Value.Split(",");

                                            if (ss_min.Length == 2 &&
                                                !string.IsNullOrEmpty(ss_min[0]) &&
                                                !string.IsNullOrEmpty(ss_min[1]))
                                            {
                                                //var findDevice = false;
                                                //if (!_AllInfoMonitors.Any(x => x.FwVersion.ToUpper().Equals(ss_min[0].ToUpper())))
                                                //{
                                                miniver = ss_min[0];
                                                //fwUpdateMonitorInfos = fwUpdateMonitorInfos.Where(x => x.FwVersion.ToUpper().Equals(ss_min[0].ToUpper())).ToList();
                                                //    findDevice = true;
                                                //}
                                                //if (!findDevice)
                                                //{
                                                //    return NoupdateResponse(commandLineInput);
                                                //}

                                            }
                                        }
                                        if (commandLineInput.Options[i].Option_Value.ToUpper().Contains("MODEL"))
                                        {
                                            string[] ss_mod = commandLineInput.Options[i].Option_Value.Split(",");

                                            if (ss_mod.Length == 2 &&
                                                !string.IsNullOrEmpty(ss_mod[0]) &&
                                                !string.IsNullOrEmpty(ss_mod[1]))
                                            {
                                                var findDevice = false;
                                                if (_AllInfoMonitors.Any(x => x.modelName.ToUpper().Equals(ss_mod[0].ToUpper())))
                                                {
                                                    model = new List<string> { ss_mod[0] };
                                                    fwUpdateMonitorInfos = fwUpdateMonitorInfos.Where(x => x.modelName.ToUpper().Equals(ss_mod[0].ToUpper())).ToList();
                                                    findDevice = true;
                                                }
                                                if (!findDevice)
                                                {
                                                    return NoDeviceConnectResponse(commandLineInput);
                                                }

                                            }
                                        }
                                        if (commandLineInput.Options[i].Option_Value.ToUpper().Contains("FILEPATH"))
                                        {
                                            string[] ss_filepath = commandLineInput.Options[i].Option_Value.Split(",");

                                            if (ss_filepath.Length == 2 &&
                                                !string.IsNullOrEmpty(ss_filepath[0]) &&
                                                !string.IsNullOrEmpty(ss_filepath[1]))
                                            {
                                                installPath = ss_filepath[0];
                                                Trace.WriteLine($"installPath = {installPath}");
                                            }
                                        }
                                    }
                                }

                                if (commandLineInput.Model.Count > 0)
                                {
                                    if (model == null)
                                    {
                                        model = commandLineInput.Model;
                                    }
                                    else
                                    {
                                        model.AddRange(commandLineInput.Model);
                                        model = model.Select(x => x.ToLower()).Distinct().ToList();
                                    }
                                    fwUpdateMonitorInfos = fwUpdateMonitorInfos.Where(x => model.Contains(x.modelName, StringComparer.OrdinalIgnoreCase)).ToList();
                                }
                                if (commandLineInput.ServiceTag.Count > 0)
                                {
                                    if (serviceTag == null)
                                    {
                                        serviceTag = commandLineInput.ServiceTag;
                                    }
                                    else
                                    {
                                        serviceTag.AddRange(commandLineInput.ServiceTag);
                                        serviceTag = serviceTag.Select(x => x.ToLower()).Distinct().ToList();
                                    }
                                    fwUpdateMonitorInfos = fwUpdateMonitorInfos.Where(x => serviceTag.Contains(x.edid.ServiceTag, StringComparer.OrdinalIgnoreCase)).ToList();
                                }

                                switch (commandLineInput.TargetFeature)
                                {
                                    case "FIRMWAREUPDATE":
                                        switch (ss_1[1].ToUpper())
                                        {
                                            case "FORCEWITHNOTICE":
                                                isShowInfo = true;
                                                isForce = true;
                                                isDefer = false;
                                                break;

                                            case "FORCEWITHNONOTICE":
                                                isShowInfo = false;
                                                isForce = true;
                                                isDefer = false;
                                                break;

                                            case "DEFER":
                                                isShowInfo = true;
                                                isForce = false;
                                                isDefer = true;
                                                break;

                                            default:
                                                cLI_FWU_RESPONSE.Message = "Input FAIL";
                                                ret = false;
                                                break;
                                        }
                                        break;
                                    default:
                                        cLI_FWU_RESPONSE.Message = "TargetFeature error";
                                        ret = false;
                                        break;
                                }

                                var fwupdate = Auto_FWUpdate_display(commandLineInput, cLI_FWU_RESPONSE, fwUpdateMonitorInfos, installPath, isShowInfo, isForce, serviceTag, model, miniver, isDefer);
                                result.ExitCode = fwupdate.code;
                                result.serialize_Json_response = fwupdate.result;
                                ret = true;
                            }
                            else if (!ss_1[0].ToUpper().Equals("DISPLAY"))
                            {
                                deviceInfoList = _devMgr.GetDevices().Result.deviceInfo;

                                var fwUpdateDeviceInfos = deviceInfoList.Select(_ => _).ToList();

                                foreach (var g in deviceInfoList)
                                {
                                    if (g.LogicalDeviceType == "LogicalMouse" && ss_1[0].ToUpper().Equals("MOUSE"))
                                    {
                                        _recode_mouse = true;
                                        fwUpdateDeviceInfos = fwUpdateDeviceInfos.Where(x => x.LogicalDeviceType == "LogicalMouse").ToList();
                                    }
                                    else if (g.LogicalDeviceType == "LogicalKeyboard" && ss_1[0].ToUpper().Equals("KEYBOARD"))
                                    {
                                        _recode_kb = true;
                                        fwUpdateDeviceInfos = fwUpdateDeviceInfos.Where(x => x.LogicalDeviceType == "LogicalKeyboard").ToList();
                                    }
                                    else if (g.LogicalDeviceType == "LogicalHeadset" && ss_1[0].ToUpper().Equals("HEADSET"))
                                    {
                                        _recode_headset = true;
                                        fwUpdateDeviceInfos = fwUpdateDeviceInfos.Where(x => x.LogicalDeviceType == "LogicalHeadset").ToList();
                                    }
                                    else if (g.LogicalDeviceType == "LogicalWebcam" && ss_1[0].ToUpper().Equals("WEBCAM"))
                                    {
                                        _recode_webcam = true;
                                        fwUpdateDeviceInfos = fwUpdateDeviceInfos.Where(x => x.LogicalDeviceType == "LogicalWebcam").ToList();
                                    }
                                    else if (g.LogicalDeviceType == "LogicalWiredAudio" && ss_1[0].ToUpper().Equals("SPEAKER"))
                                    {
                                        _recode_speaker = true;
                                        fwUpdateDeviceInfos = fwUpdateDeviceInfos.Where(x => x.LogicalDeviceType == "LogicalWiredAudio").ToList();
                                    }
                                    else if (g.LogicalDeviceType == "LogicalWiredAudio" && ss_1[0].ToUpper().Equals("SOUNDBAR"))
                                    {
                                        _record_soundbar = true;
                                        fwUpdateDeviceInfos = fwUpdateDeviceInfos.Where(x => x.LogicalDeviceType == "LogicalWiredAudio").ToList();
                                    }
                                    else if (g.LogicalDeviceType == "LogicalAirAudio" && ss_1[0].ToUpper().Equals("SPEAKER"))
                                    {
                                        _recode_air_audio = true;
                                        fwUpdateDeviceInfos = fwUpdateDeviceInfos.Where(x => x.LogicalDeviceType == "LogicalAirAudio").ToList();
                                    }
                                    else if (g.LogicalDeviceType == "LogicalAirAudio" && ss_1[0].ToUpper().Equals("SOUNDBAR"))
                                    {
                                        _recode_air_audio = true;
                                        fwUpdateDeviceInfos = fwUpdateDeviceInfos.Where(x => x.LogicalDeviceType == "LogicalAirAudio").ToList();
                                    }
                                    else if (g.LogicalDeviceType == "LogicalPen" && ss_1[0].ToUpper().Equals("PEN"))
                                    {
                                        _recode_pen = true;
                                        fwUpdateDeviceInfos = fwUpdateDeviceInfos.Where(x => x.LogicalDeviceType == "LogicalPen").ToList();
                                    }
                                    else if (g.LogicalDeviceType == "LogicalDock" && ss_1[0].ToUpper().Equals("DOCK"))
                                    {
                                        _recode_dock = true;
                                        fwUpdateDeviceInfos = fwUpdateDeviceInfos.Where(x => x.LogicalDeviceType == "LogicalDock").ToList();
                                    }
                                    else if (ss_1[0].ToUpper().Equals("DONGLE"))
                                    {
                                        _recode_dongle = true;
                                    }
                                    else if (ss_1[0].ToUpper().Equals("AUDIO"))
                                    {
                                        fwUpdateDeviceInfos = fwUpdateDeviceInfos.Where(x => x.LogicalDeviceType == "LogicalHeadset" || x.LogicalDeviceType == "LogicalWiredAudio" || x.LogicalDeviceType == "LogicalAirAudio").ToList();

                                        switch (g.LogicalDeviceType)
                                        {
                                            case "LogicalHeadset":
                                                _recode_headset = true;
                                                break;
                                            case "LogicalWiredAudio":
                                                _recode_speaker = true;
                                                _record_soundbar = true;
                                                break;
                                            case "LogicalAirAudio":
                                                _recode_speaker = true;
                                                _record_soundbar = true;
                                                break;
                                            default:
                                                break;
                                        }
                                    }
                                    Trace.WriteLine($"g.PhyscialDeviceID  =  {g.LogicalDeviceType}");
                                }

                                if (commandLineInput.Options.Count > 0)
                                {
                                    for (int i = 0; i < commandLineInput.Options.Count; i++)
                                    {
                                        Trace.WriteLine(commandLineInput.Options[i].Option_Value.ToUpper());
                                        if (commandLineInput.Options[i].Option_Value.ToUpper().Contains("GUID"))
                                        {
                                            string[] ss_guid = commandLineInput.Options[i].Option_Value.Split(",");

                                            if (ss_guid.Length == 2 &&
                                                !string.IsNullOrEmpty(ss_guid[0]) &&
                                                !string.IsNullOrEmpty(ss_guid[1]))
                                            {
                                                if (ss_1[0].ToUpper() == "DOCK" && commandLineInput.TargetType == "APP") // Checking for TargetType=APP
                                                {
                                                    CLI_RESPONSE rsp = new CLI_RESPONSE()
                                                    {
                                                        Command = commandLineInput.Command,
                                                        TargetFeature = commandLineInput.TargetFeature,
                                                        Result = "FAIL",
                                                        Message = "Dock not support GUID option",
                                                    };
                                                    writelog("FAIL Dock not support GUID option");
                                                    Console.WriteLine(JsonConvert.SerializeObject(rsp, Formatting.Indented));
                                                    return ((int)CLI_ExitCode.fail_FWUpdate, JsonConvert.SerializeObject(rsp, Formatting.Indented));
                                                }

                                                var findDevice = false;
                                                foreach (var g in deviceInfoList)
                                                {
                                                    if (g.ID.ToString().ToUpper() == ss_guid[0].ToUpper())
                                                    {
                                                        guid = new List<string>() { ss_guid[0] };
                                                        fwUpdateDeviceInfos = fwUpdateDeviceInfos.Where(x => x.ID.ToString().Equals(ss_guid[0], StringComparison.OrdinalIgnoreCase)).ToList();
                                                        findDevice = true;
                                                    }
                                                }
                                                if (!findDevice)
                                                {
                                                    return NoDeviceConnectResponse(commandLineInput);
                                                }
                                            }
                                        }
                                        else if (commandLineInput.Options[i].Option_Value.ToUpper().Contains("MINIVERSION"))
                                        {
                                            string[] ss_min = commandLineInput.Options[i].Option_Value.Split(",");

                                            if (ss_min.Length == 2 &&
                                                !string.IsNullOrEmpty(ss_min[0]) &&
                                                !string.IsNullOrEmpty(ss_min[1]))
                                            {
                                                var findDevice = false;
                                                //foreach (var g in deviceInfoList)
                                                //{
                                                //if (g.FirmwareVersion.ToUpper() != ss_min[0].ToUpper())
                                                //{
                                                miniver = ss_min[0];
                                                //fwUpdateDeviceInfos = fwUpdateDeviceInfos.Where(x => x.FirmwareVersion.Equals(ss_min[0], StringComparison.OrdinalIgnoreCase)).ToList();
                                                //findDevice = true;
                                                //    }
                                                //}
                                                //if (!findDevice)
                                                //{
                                                //    return NoDeviceConnectResponse(commandLineInput);
                                                //}
                                            }
                                        }
                                        else if (commandLineInput.Options[i].Option_Value.ToUpper().Contains("MODEL"))
                                        {
                                            string[] ss_mod = commandLineInput.Options[i].Option_Value.Split(",");

                                            if (ss_mod.Length == 2 &&
                                                !string.IsNullOrEmpty(ss_mod[0]) &&
                                                !string.IsNullOrEmpty(ss_mod[1]))
                                            {
                                                var findDevice = false;
                                                string NewModel = JudgmentList.ModelRename(ss_mod[0].ToUpper());
                                                foreach (var g in deviceInfoList)
                                                {
                                                    if (g.ModelNumber.ToUpper() == NewModel)
                                                    {
                                                        model = new List<string> { NewModel };
                                                        fwUpdateDeviceInfos = fwUpdateDeviceInfos.Where(x => x.ModelNumber.Equals(NewModel, StringComparison.OrdinalIgnoreCase)).ToList();
                                                        findDevice = true;
                                                    }
                                                }
                                                if (!findDevice)
                                                {
                                                    return NoDeviceConnectResponse(commandLineInput);
                                                }
                                            }
                                        }
                                        else if (commandLineInput.Options[i].Option_Value.ToUpper().Contains("SERVICETAG"))
                                        {
                                            string[] serviceTaInputValues = commandLineInput.Options[i].Option_Value.Split(",");

                                            if (serviceTaInputValues.Length == 2 &&
                                                !string.IsNullOrEmpty(serviceTaInputValues[0]) &&
                                                !string.IsNullOrEmpty(serviceTaInputValues[1]))
                                            {
                                                var findDevice = false;
                                                foreach (var g in deviceInfoList)
                                                {
                                                    if (!string.IsNullOrWhiteSpace(g.DockServiceTag) && g.DockServiceTag.ToUpper() == serviceTaInputValues[0].ToUpper())
                                                    {
                                                        serviceTag = new List<string> { serviceTaInputValues[0] };
                                                        fwUpdateDeviceInfos = fwUpdateDeviceInfos.Where(x => x.DockServiceTag.Equals(serviceTaInputValues[0], StringComparison.OrdinalIgnoreCase)).ToList();
                                                        findDevice = true;
                                                    }
                                                }
                                                if (!findDevice)
                                                {
                                                    return NoDeviceConnectResponse(commandLineInput);
                                                }

                                            }
                                        }
                                        else if (commandLineInput.Options[i].Option_Value.ToUpper().Contains("UOD"))
                                        {
                                            string[] uodInputValues = commandLineInput.Options[i].Option_Value.Split(",");

                                            if (uodInputValues.Length == 2 &&
                                                !string.IsNullOrEmpty(uodInputValues[0]) &&
                                                !string.IsNullOrEmpty(uodInputValues[1]))
                                            {
                                                if (ss_1[0].ToUpper() == "DOCK")
                                                {
                                                    if (uodInputValues[0].ToUpper() != "TRUE" && uodInputValues[0].ToUpper() != "FALSE")
                                                    {
                                                        CLI_RESPONSE rsp = new CLI_RESPONSE()
                                                        {
                                                            Command = commandLineInput.Command,
                                                            TargetFeature = commandLineInput.TargetFeature,
                                                            Result = "FAIL",
                                                            Message = "Not correct UOD option",
                                                        };
                                                        writelog("FAIL Not correct UOD option");
                                                        Console.WriteLine(JsonConvert.SerializeObject(rsp, Formatting.Indented));
                                                        return ((int)CLI_ExitCode.fail_FWUpdate, JsonConvert.SerializeObject(rsp, Formatting.Indented));
                                                    }
                                                    isUod = uodInputValues[0].ToUpper() == "TRUE";
                                                }
                                                else
                                                {
                                                    CLI_RESPONSE rsp = new CLI_RESPONSE()
                                                    {
                                                        Command = commandLineInput.Command,
                                                        TargetFeature = commandLineInput.TargetFeature,
                                                        Result = "FAIL",
                                                        Message = "Only dock supports UOD update mode",
                                                    };
                                                    writelog("FAIL Only dock supports UOD update mode");
                                                    Console.WriteLine(JsonConvert.SerializeObject(rsp, Formatting.Indented));
                                                    return ((int)CLI_ExitCode.fail_FWUpdate, JsonConvert.SerializeObject(rsp, Formatting.Indented));
                                                }
                                            }
                                        }
                                        else if (commandLineInput.Options[i].Option_Value.ToUpper().Contains("FILEPATH"))
                                        {
                                            string[] ss_filepath = commandLineInput.Options[i].Option_Value.Split(",");

                                            if (ss_filepath.Length == 2 &&
                                                !string.IsNullOrEmpty(ss_filepath[0]) &&
                                                !string.IsNullOrEmpty(ss_filepath[1]))
                                            {
                                                installPath = ss_filepath[0];
                                            }
                                        }
                                    }
                                    if (_recode_mouse || _recode_kb || _recode_dock || _recode_headset || _recode_webcam || _recode_speaker || _record_soundbar || _recode_pen || _recode_dongle)
                                    {
                                        string[] ss_2 = commandLineInput.Options[0].Option_Value.Split(",");

                                        if (ss_2.Length == 2)
                                        {
                                            if (!string.IsNullOrEmpty(ss_2[0]) && !string.IsNullOrEmpty(ss_2[1]))
                                            {
                                                if (ss_2[0].ToUpper().Equals("MOUSE") && !_recode_mouse)
                                                {
                                                    CLI_RESPONSE rsp = new CLI_RESPONSE()
                                                    {
                                                        Command = commandLineInput.Command,
                                                        TargetFeature = commandLineInput.TargetFeature,
                                                        Result = "FAIL",
                                                        Message = "No MOUSE connected",
                                                    };
                                                    writelog("FAIL No MOUSE connected");
                                                    System.Console.WriteLine(JsonConvert.SerializeObject(rsp, Formatting.Indented));
                                                    return ((int)CLI_ExitCode.fail_FWUpdate, JsonConvert.SerializeObject(rsp, Formatting.Indented));
                                                }
                                                else if (ss_2[0].ToUpper().Equals("KEYBOARD") && !_recode_kb)
                                                {
                                                    CLI_RESPONSE rsp = new CLI_RESPONSE()
                                                    {
                                                        Command = commandLineInput.Command,
                                                        TargetFeature = commandLineInput.TargetFeature,
                                                        Result = "FAIL",
                                                        Message = "No KEYBOARD connected",
                                                    };
                                                    writelog("FAIL No KEYBOARD connected");
                                                    System.Console.WriteLine(JsonConvert.SerializeObject(rsp, Formatting.Indented));
                                                    return ((int)CLI_ExitCode.fail_FWUpdate, JsonConvert.SerializeObject(rsp, Formatting.Indented));
                                                }
                                                else if (ss_2[0].ToUpper().Equals("DOCK") && !_recode_dock)
                                                {
                                                    CLI_RESPONSE rsp = new CLI_RESPONSE()
                                                    {
                                                        Command = commandLineInput.Command,
                                                        TargetFeature = commandLineInput.TargetFeature,
                                                        Result = "FAIL",
                                                        Message = "No DOCK connected",
                                                    };
                                                    writelog("FAIL No DOCK connected");
                                                    System.Console.WriteLine(JsonConvert.SerializeObject(rsp, Formatting.Indented));
                                                    return ((int)CLI_ExitCode.fail_FWUpdate, JsonConvert.SerializeObject(rsp, Formatting.Indented));
                                                }
                                                else if (ss_2[0].ToUpper().Equals("HEADSET") && !_recode_headset)
                                                {
                                                    CLI_RESPONSE rsp = new CLI_RESPONSE()
                                                    {
                                                        Command = commandLineInput.Command,
                                                        TargetFeature = commandLineInput.TargetFeature,
                                                        Result = "FAIL",
                                                        Message = "No HEADSET connected",
                                                    };
                                                    writelog("FAIL No HEADSET connected");
                                                    Console.WriteLine(JsonConvert.SerializeObject(rsp, Formatting.Indented));
                                                    return ((int)CLI_ExitCode.fail_FWUpdate, JsonConvert.SerializeObject(rsp, Formatting.Indented));
                                                }
                                                else if (ss_2[0].ToUpper().Equals("WEBCAM") && !_recode_webcam)
                                                {
                                                    CLI_RESPONSE rsp = new CLI_RESPONSE()
                                                    {
                                                        Command = commandLineInput.Command,
                                                        TargetFeature = commandLineInput.TargetFeature,
                                                        Result = "FAIL",
                                                        Message = "No WEBCAM connected",
                                                    };
                                                    writelog("FAIL No WEBCAM connected");
                                                    Console.WriteLine(JsonConvert.SerializeObject(rsp, Formatting.Indented));
                                                    return ((int)CLI_ExitCode.fail_FWUpdate, JsonConvert.SerializeObject(rsp, Formatting.Indented));
                                                }
                                                else if (ss_2[0].ToUpper().Equals("SPEAKER") && !_recode_speaker)
                                                {
                                                    CLI_RESPONSE rsp = new CLI_RESPONSE()
                                                    {
                                                        Command = commandLineInput.Command,
                                                        TargetFeature = commandLineInput.TargetFeature,
                                                        Result = "FAIL",
                                                        Message = "No SPEAKER connected",
                                                    };
                                                    writelog("FAIL No SPEAKER connected");
                                                    Console.WriteLine(JsonConvert.SerializeObject(rsp, Formatting.Indented));
                                                    return ((int)CLI_ExitCode.fail_FWUpdate, JsonConvert.SerializeObject(rsp, Formatting.Indented));
                                                }
                                                else if (ss_2[0].ToUpper().Equals("SOUNDBAR") && !_record_soundbar)
                                                {
                                                    CLI_RESPONSE rsp = new CLI_RESPONSE()
                                                    {
                                                        Command = commandLineInput.Command,
                                                        TargetFeature = commandLineInput.TargetFeature,
                                                        Result = "FAIL",
                                                        Message = "No SOUNDBAR connected",
                                                    };
                                                    writelog("FAIL No SOUNDBAR connected");
                                                    Console.WriteLine(JsonConvert.SerializeObject(rsp, Formatting.Indented));
                                                    return ((int)CLI_ExitCode.fail_FWUpdate, JsonConvert.SerializeObject(rsp, Formatting.Indented));
                                                }
                                                else if (ss_2[0].ToUpper().Equals("PEN") && !_recode_pen)
                                                {
                                                    CLI_RESPONSE rsp = new CLI_RESPONSE()
                                                    {
                                                        Command = commandLineInput.Command,
                                                        TargetFeature = commandLineInput.TargetFeature,
                                                        Result = "FAIL",
                                                        Message = "No PEN connected",
                                                    };
                                                    writelog("FAIL No PEN connected");
                                                    Console.WriteLine(JsonConvert.SerializeObject(rsp, Formatting.Indented));
                                                    return ((int)CLI_ExitCode.fail_FWUpdate, JsonConvert.SerializeObject(rsp, Formatting.Indented));
                                                }
                                                else if (ss_2[0].ToUpper().Equals("DONGLE") && !_recode_dongle)
                                                {
                                                    CLI_RESPONSE rsp = new CLI_RESPONSE()
                                                    {
                                                        Command = commandLineInput.Command,
                                                        TargetFeature = commandLineInput.TargetFeature,
                                                        Result = "FAIL",
                                                        Message = "No DONGLE connected",
                                                    };
                                                    writelog("FAIL No DONGLE connected");
                                                    Console.WriteLine(JsonConvert.SerializeObject(rsp, Formatting.Indented));
                                                    return ((int)CLI_ExitCode.fail_FWUpdate, JsonConvert.SerializeObject(rsp, Formatting.Indented));
                                                }
                                                else if (ss_2[0].ToUpper().Equals("AUDIO") && !_recode_headset && !_recode_speaker && !_record_soundbar)
                                                {
                                                    CLI_RESPONSE rsp = new CLI_RESPONSE()
                                                    {
                                                        Command = commandLineInput.Command,
                                                        TargetFeature = commandLineInput.TargetFeature,
                                                        Result = "FAIL",
                                                        Message = "No AUDIO connected",
                                                    };
                                                    writelog("FAIL No AUDIO connected");
                                                    Console.WriteLine(JsonConvert.SerializeObject(rsp, Formatting.Indented));
                                                    return ((int)CLI_ExitCode.fail_FWUpdate, JsonConvert.SerializeObject(rsp, Formatting.Indented));
                                                }

                                                if (commandLineInput.Model.Count > 0)
                                                {                                            
                                                    string newModel = String.Empty;
                                                    bool findDevices = false;
                                                    foreach (string Model in commandLineInput.Model)
                                                    {
                                                        newModel = JudgmentList.ModelRename(Model.ToString().ToUpper());
                                                        if (model == null)
                                                        {
                                                            foreach (var dev_info in deviceInfoList)
                                                            {
                                                                if (dev_info.ModelNumber.ToString().ToUpper().Equals(newModel))
                                                                {
                                                                    List<string> tempList = new List<string> { newModel };
                                                                    model = tempList;
                                                                    findDevices = true;
                                                                    //Console.WriteLine($"------------- models: {model[0]} -------------");
                                                                }
                                                            }
                                                        }
                                                        else
                                                        {
                                                            foreach (var dev_info in deviceInfoList)
                                                            {
                                                                if (dev_info.ModelNumber.ToString().ToUpper().Equals(newModel))
                                                                {
                                                                    //Console.WriteLine($"------------- models: {model[0]} -------------");
                                                                    //model.AddRange(commandLineInput.Model);
                                                                    model.Add(newModel);
                                                                    model = model.Select(x => x.ToUpper()).Distinct().ToList();
                                                                    findDevices = true;
                                                                }
                                                            }
                                                        }
                                                        if (!findDevices)
                                                        {
                                                            return NoDeviceConnectResponse(commandLineInput);
                                                        }
                                                        //Console.WriteLine($"------------- models: {model[0]}-------------");
                                                        fwUpdateDeviceInfos = fwUpdateDeviceInfos.Where(x => model.Contains(x.ModelNumber, StringComparer.OrdinalIgnoreCase)).ToList();
                                                    }
                                                }
                                                if (commandLineInput.ServiceTag.Count > 0)
                                                {
                                                    if (serviceTag == null)
                                                    {
                                                        serviceTag = commandLineInput.ServiceTag;
                                                    }
                                                    else
                                                    {
                                                        serviceTag.AddRange(commandLineInput.ServiceTag);
                                                        serviceTag = serviceTag.Select(x => x.ToLower()).Distinct().ToList();
                                                    }
                                                    fwUpdateDeviceInfos = fwUpdateDeviceInfos.Where(x => serviceTag.Contains(x.DockServiceTag, StringComparer.OrdinalIgnoreCase)).ToList();
                                                }

                                                switch (commandLineInput.TargetFeature)
                                                {
                                                    case "FIRMWAREUPDATE":
                                                        switch (ss_2[1].ToUpper())
                                                        {
                                                            case "FORCEWITHNOTICE":
                                                                isShowInfo = true;
                                                                isForce = true;
                                                                isDefer = false;
                                                                break;

                                                            case "FORCEWITHNONOTICE":
                                                                isShowInfo = false;
                                                                isForce = true;
                                                                isDefer = false;
                                                                break;

                                                            case "DEFER":
                                                                isShowInfo = true;
                                                                isForce = false;
                                                                isDefer = true;
                                                                break;

                                                            default:
                                                                cLI_FWU_RESPONSE.Message = "Input FAIL";
                                                                ret = false;
                                                                break;
                                                        }
                                                        
                                                        var fwupdate = Auto_FWUpdate2(commandLineInput, cLI_FWU_RESPONSE, fwUpdateDeviceInfos, isUod, installPath, isShowInfo, isForce, guid, model, miniver, isDefer, serviceTag);
                                                        result.ExitCode = fwupdate.code;
                                                        result.serialize_Json_response = fwupdate.result;
                                                        ret = true;
                                                        break;
                                                    case "SILENTFWUPDATE":
                                                        isUod = true;
                                                        switch (ss_2[1].ToUpper())
                                                        {
                                                            case "FORCEWITHNOTICE":
                                                                isShowInfo = true;
                                                                isForce = true;
                                                                isDefer = false;
                                                                break;

                                                            case "FORCEWITHNONOTICE":
                                                                isShowInfo = false;
                                                                isForce = true;
                                                                isDefer = false;
                                                                break;

                                                            case "DEFER":
                                                                isShowInfo = true;
                                                                isForce = false;
                                                                isDefer = true;
                                                                break;

                                                            default:
                                                                cLI_FWU_RESPONSE.Message = "Input FAIL";
                                                                ret = false;
                                                                break;

                                                        }
                                                        var fwupdate2 = Auto_FWUpdate2(commandLineInput, cLI_FWU_RESPONSE, fwUpdateDeviceInfos, isUod, installPath, isShowInfo, isForce, guid, model, miniver, isDefer, serviceTag);
                                                        result.ExitCode = fwupdate2.code;
                                                        result.serialize_Json_response = fwupdate2.result;
                                                        ret = true;
                                                        break;
                                                    default:
                                                        cLI_FWU_RESPONSE.Message = "TargetFeature error";
                                                        ret = false;
                                                        break;
                                                }
                                            }
                                            else
                                            {
                                                somethingnull = true;
                                            }
                                        }
                                        else
                                        {
                                            return NoDeviceConnectResponse(commandLineInput);
                                        }
                                    }
                                    else
                                    {
                                        return NoDeviceConnectResponse(commandLineInput);
                                    }
                                }
                            }
                        }
                        else
                        {
                            somethingnull = true;
                        }
                    }
                    else
                    {
                        somethingnull = true;
                    }
                }
                else
                {
                    CLI_RESPONSE rsp = new CLI_RESPONSE()
                    {
                        Command = commandLineInput.Command,
                        TargetFeature = commandLineInput.TargetFeature,
                        Result = "FAIL",
                        Message = "Input fail, option count < 0",
                    };
                    writelog("FW update Input fail, option count < 0");
                    Console.WriteLine(JsonConvert.SerializeObject(rsp, Formatting.Indented));
                    return ((int)CLI_ExitCode.fail_FWUpdate, JsonConvert.SerializeObject(rsp, Formatting.Indented));
                }

                if (cLI_FWU_RESPONSE != null)
                {
                    output = JsonConvert.SerializeObject(cLI_FWU_RESPONSE, Formatting.Indented);
                }

                if (somethingnull)
                {
                    CLI_RESPONSE rsp = new CLI_RESPONSE()
                    {
                        Command = commandLineInput.Command,
                        TargetFeature = commandLineInput.TargetFeature,
                        Result = "FAIL",
                        Message = "Input null value",
                    };
                    writelog("FW update FAIL Input null Value");
                    Console.WriteLine(JsonConvert.SerializeObject(rsp, Formatting.Indented));
                    return ((int)CLI_ExitCode.fail_FWUpdate, JsonConvert.SerializeObject(rsp, Formatting.Indented));
                }
                else if (somethingError)
                {
                    CLI_RESPONSE rsp = new CLI_RESPONSE()
                    {
                        Command = commandLineInput.Command,
                        TargetFeature = commandLineInput.TargetFeature,
                        Result = "FAIL",
                        Message = "FW update failure",
                    };
                    writelog("FW update failurre FAIL");
                    Console.WriteLine(JsonConvert.SerializeObject(rsp, Formatting.Indented));
                    return ((int)CLI_ExitCode.fail_FWUpdate, JsonConvert.SerializeObject(rsp, Formatting.Indented));
                }
                else if (ret == true)
                {
                    return ((int)CLI_ExitCode.success, output);
                }
                else
                {
                    CLI_RESPONSE rsp = new CLI_RESPONSE()
                    {
                        Command = commandLineInput.Command,
                        TargetFeature = commandLineInput.TargetFeature,
                        Result = "FAIL",
                        Message = "FW update failure exception x",
                    };
                    writelog("FW update failure exception x");
                    Console.WriteLine(JsonConvert.SerializeObject(rsp, Formatting.Indented));
                    return ((int)CLI_ExitCode.fail_FWUpdate, JsonConvert.SerializeObject(rsp, Formatting.Indented));
                }
            }
            catch
            {
                CLI_RESPONSE rsp = new CLI_RESPONSE()
                {
                    Command = commandLineInput.Command,
                    TargetFeature = commandLineInput.TargetFeature,
                    Result = "FAIL",
                    Message = "FW update failure exception",
                };
                writelog("FW update failure exception");
                Console.WriteLine(JsonConvert.SerializeObject(rsp, Formatting.Indented));
                return ((int)CLI_ExitCode.fail_FWUpdate, JsonConvert.SerializeObject(rsp, Formatting.Indented));
            }
        }
        /*
        private bool? GetFWUpdateList(CommandLineInput commandLineInput, CLI_FWU_RESPONSE cli_FWU_RESPONSE, bool isShowInfo = true, bool isDefer = false)
        {
            List<DeviceType> deviceTypes = new List<DeviceType>();
            DeviceType deviceType = DeviceType.Unknown;
            bool dock_recode = false;

            _deviceinfo = _devMgr.GetDevices().Result.deviceInfo;
            foreach (var g in _deviceinfo)
            {
                if (g.LogicalDeviceType == "LogicalDock" && commandLineInput.TargetType.Equals("DOCK"))
                {
                    dock_recode = true;
                }
            }


            if (commandLineInput.Options.Count > 0 && !dock_recode)
            {
                (deviceType, deviceTypes) = SetDevice(commandLineInput);

            }
            else if (dock_recode)
            {
                deviceTypes.Add(DeviceType.LogicalDock);
                deviceTypes.Add(DeviceType.PhysicalWiredDock);
                deviceType = DeviceType.LogicalDock;
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
                    List<FWUpdateInfo> retFWUpdateInfosList = _devMgr.DownloadAndInstall(fwUpdateInfo).Result;
                    bool b = true;
                    foreach (FWUpdateInfo retFWUpdateInfo in retFWUpdateInfosList)
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
        */
        //0531 Bruce 因應IL的現有安裝包修改判斷，CLIPeripheralsPlugins.cs中Auto_FWUpdate方法修改回傳值型態和新增判斷
        private List<FWUpdateInfo> retFWUpdateInfos;

        private class FWUpdateResponseInfo
        {
            public string Model { get; set; }
            public string ServiceTag { get; set; }
            public string Version { get; set; }
        }

        private (int code, string result) Auto_FWUpdate2(CommandLineInput commandLineInput, CLI_FWU_RESPONSE cli_FWU_RESPONSE, List<DeviceInfo> fwUpdateDeviceInfos, bool isUODMode, string installPath, bool isShowInfo = true, bool isForce = false, List<string> guid = null, List<string> model = null, string miniver = null, bool isDefer = false, List<string> serviceTag = null)
        {
            try
            {
                List<DeviceInfo> deviceInfoList = null;
                deviceInfoList = _devMgr.GetDevices().Result.deviceInfo;
                string[] ss_1 = null;
                List<DeviceType> deviceTypes = new List<DeviceType>();
                int _isheadsetready = 0;
                if (commandLineInput.Options.Count > 0)
                {
                    deviceTypes = SetDevice(commandLineInput).deviceTypes;
                }

                _devMgr.ProgressUpdate_Notify -= _FWUpdatePlugin_ProgressUpdate;
                _devMgr.ProgressUpdate_Notify += _FWUpdatePlugin_ProgressUpdate;
                //_devMgr.DownloadAndInstall_Result_Notify -= Download_Event;
                //_devMgr.DownloadAndInstall_Result_Notify += Download_Event;
                if (installPath != "")
                {
                    cli_FWU_RESPONSE.Result = "PASS";
                    Task.Run(new Action(() =>
                    {
                        writelog("Auto_FWUpdate2 Install");
                        FWUErrorCode ret = _devMgr.Install(installPath, false).Result;
                        Trace.WriteLine($"ret = {ret}");

                        if (ret == FWUErrorCode.NoError)
                        {
                            cli_FWU_RESPONSE.Result = "PASS";
                        }
                        else
                        {
                            cli_FWU_RESPONSE.FWUpdateRESPONSE.Add($"Fail message:{ret.ToString()}");
                            cli_FWU_RESPONSE.Result = "FAIL";
                        }
                        FWResultReceived?.Invoke(this, (ret, JsonConvert.SerializeObject(cli_FWU_RESPONSE, Formatting.Indented)));
                        _devMgr.ProgressUpdate_Notify -= _FWUpdatePlugin_ProgressUpdate;
                        _devMgr.DownloadAndInstall_Result_Notify -= Download_Event;
                    }));
                }
                else
                {
                    FWUpdateInfoPackage allFWUpdateInfo = _devMgr.GetFWUpdateInfo(true).Result;
                    var fwUpdateInfoPackage = Filter(allFWUpdateInfo, guid, serviceTag, model, miniver, deviceTypes);

                    var allFWUpdateResponseInfos = fwUpdateInfoPackage.FWUpdateInfo
                                                                      .Select(_ => new FWUpdateResponseInfo
                                                                      {
                                                                          Model = _.Model,
                                                                          ServiceTag = _.ServiceTag,
                                                                          Version = _.DeviceVersion
                                                                      })
                                                                      .Concat(fwUpdateDeviceInfos.Where(_ => !fwUpdateInfoPackage.FWUpdateInfo
                                                                                                                                 .Select(x => x.DeviceId
                                                                                                                                               .Replace("{", "")
                                                                                                                                               .Replace("}", ""))
                                                                                                                                 .Contains(_.ID.ToString()))
                                                                                                 .Select(_ => new FWUpdateResponseInfo
                                                                                                 {
                                                                                                     Model = _.ModelNumber,
                                                                                                     ServiceTag = _.DockServiceTag,
                                                                                                     Version = _.FirmwareVersion
                                                                                                 }))
                                                                      .ToList();

                    var serialNumbers = new List<string>();
                    string ss0 = null;
                    if (commandLineInput.Options.Count > 0)
                        ss_1 = commandLineInput.Options[0].Option_Value.Split(",");
                    if (ss_1.Length > 0)
                    {
                        foreach (var info in allFWUpdateResponseInfos)
                        {
                            var device0 = fwUpdateDeviceInfos.FirstOrDefault(_ => _.LogicalDeviceType.ToString().ToUpper().Contains("LOGICALHEADSET"));
                            if(device0 == null)
                                ss0 = ss_1[0];
                            else if (ss_1[0].ToUpper() == "AUDIO" && device0.LogicalDeviceType.ToString().ToUpper().Contains("HEADSET")) 
                                ss0 = "HEADSET";
                            else if(device0.LogicalDeviceType.ToString().ToUpper().Contains("HEADSET"))
                                ss0 = "HEADSET";

                            var device = fwUpdateDeviceInfos.FirstOrDefault(_ => _.ModelNumber.Equals(info.Model, StringComparison.OrdinalIgnoreCase) &&
                                                            _.LogicalDeviceType.ToString().ToUpper().Contains(ss0));

                            //Console.WriteLine($"-----Model: {info.Model} Service Tag: {ss_1[0]} Version: {info.Version}-----");
                            //Console.WriteLine($"-----DeviceType: {device.LogicalDeviceType} FW Version: {device.FirmwareVersion}-----");
                            //Trace.WriteLine($@"LogicalDeviceType = {device.LogicalDeviceType.ToString().ToUpper()}");
                            if (device != null && device.LogicalDeviceType.ToString().ToUpper().Contains("HEADSET"))
                            {
                                while (_isheadsetready < 10)
                                {
                                    Thread.Sleep(1000);
                                    if (_devMgr.GetIsReadyAsync(device.ID.ToString()).Result)
                                        break;
                                    _isheadsetready++;
                                }
                                                               
                                serialNumbers.Add(_devMgr.GetHeadsetSerialNumberAsync(device.ID.ToString()).Result ?? "N/A");
                            }
                            else if (device != null && device.LogicalDeviceType.ToString().ToUpper().Contains("WEBCAM"))
                            {
                                serialNumbers.Add(_devMgr.GetWebcamSerialNumber(device.ID.ToString()).Result ?? "N/A");
                            }
                            else
                            {
                                serialNumbers.Add("N/A");
                            }
                        }
                    }
                    cli_FWU_RESPONSE.Model = string.Join(",", allFWUpdateResponseInfos.Select(_ => _.Model));
                    cli_FWU_RESPONSE.SerialNumber = string.Join(",", serialNumbers);
                    cli_FWU_RESPONSE.ServiceTag = string.Join(",", allFWUpdateResponseInfos.Select(_ => !string.IsNullOrWhiteSpace(_.ServiceTag) ? _.ServiceTag : "N/A"));
                    cli_FWU_RESPONSE.FWVersion = string.Join(",", allFWUpdateResponseInfos.Select(_ => $"[{_.Version}]"));
                    cli_FWU_RESPONSE.Result = "PASS";

                    //Console.WriteLine($"------------- device type: {ss_1.Length}{ss_1[0]}{ss_1[1]} -------------");
                    if (fwUpdateInfoPackage.FWUpdateInfo.Count <= 0)
                    {
                        fwUpdateDeviceInfos.ForEach(_ => cli_FWU_RESPONSE.FWUpdateRESPONSE.Add($"No updates available: {_.ModelNumber}, ServiceTag: {(string.IsNullOrEmpty(_.DockServiceTag) ? "N/A" : _.DockServiceTag)} to Version: {_.FirmwareVersion}"));
                        //if (ss_1.Length == 2)
                        //{
                        //    foreach (var g in deviceInfoList)
                        //    {
                        //        if (g.LogicalDeviceType == "LogicalMouse" && ss_1[0].ToUpper().Equals("MOUSE"))
                        //        {
                        //            cli_FWU_RESPONSE.Model = g.ModelNumber;
                        //            cli_FWU_RESPONSE.ServiceTag = g.DockServiceTag;
                        //            cli_FWU_RESPONSE.FWVersion = g.FirmwareVersion;
                        //            cli_FWU_RESPONSE.FWUpdateRESPONSE.Add($"No updates available: {g.ModelNumber}, ServiceTag: {(string.IsNullOrEmpty(g.DockServiceTag) ? "N/A" : g.DockServiceTag)} Version: {g.FirmwareVersion}");
                        //            cli_FWU_RESPONSE.Result = "PASS";
                        //        }
                        //        else if (g.LogicalDeviceType == "LogicalKeyboard" && ss_1[0].ToUpper().Equals("KEYBOARD"))
                        //        {
                        //            cli_FWU_RESPONSE.Model = g.ModelNumber;
                        //            cli_FWU_RESPONSE.ServiceTag = g.DockServiceTag;
                        //            cli_FWU_RESPONSE.FWVersion = g.FirmwareVersion;
                        //            cli_FWU_RESPONSE.FWUpdateRESPONSE.Add($"No updates available: {g.ModelNumber}, ServiceTag: {(string.IsNullOrEmpty(g.DockServiceTag) ? "N/A" : g.DockServiceTag)} Version: {g.FirmwareVersion}");
                        //            cli_FWU_RESPONSE.Result = "PASS";
                        //        }
                        //        else if (g.LogicalDeviceType == "LogicalHeadset" && ss_1[0].ToUpper().Equals("HEADSET"))
                        //        {
                        //            cli_FWU_RESPONSE.Model = g.ModelNumber;
                        //            cli_FWU_RESPONSE.ServiceTag = g.DockServiceTag;
                        //            cli_FWU_RESPONSE.FWVersion = g.FirmwareVersion;
                        //            cli_FWU_RESPONSE.FWUpdateRESPONSE.Add($"No updates available: {g.ModelNumber}, ServiceTag: {(string.IsNullOrEmpty(g.DockServiceTag) ? "N/A" : g.DockServiceTag)} Version: {g.FirmwareVersion}");
                        //            cli_FWU_RESPONSE.Result = "PASS";
                        //        }
                        //        else if (g.LogicalDeviceType == "LogicalWebcam" && ss_1[0].ToUpper().Equals("WEBCAM"))
                        //        {
                        //            cli_FWU_RESPONSE.Model = g.ModelNumber;
                        //            cli_FWU_RESPONSE.ServiceTag = g.DockServiceTag;
                        //            cli_FWU_RESPONSE.FWVersion = g.FirmwareVersion;
                        //            cli_FWU_RESPONSE.FWUpdateRESPONSE.Add($"No updates available: {g.ModelNumber}, ServiceTag: {(string.IsNullOrEmpty(g.DockServiceTag) ? "N/A" : g.DockServiceTag)} Version: {g.FirmwareVersion}");
                        //            cli_FWU_RESPONSE.Result = "PASS";
                        //        }
                        //        else if (g.LogicalDeviceType == "LogicalWiredAudio" && ss_1[0].ToUpper().Equals("SPEAKER"))
                        //        {
                        //            cli_FWU_RESPONSE.Model = g.ModelNumber;
                        //            cli_FWU_RESPONSE.ServiceTag = g.DockServiceTag;
                        //            cli_FWU_RESPONSE.FWVersion = g.FirmwareVersion;
                        //            cli_FWU_RESPONSE.FWUpdateRESPONSE.Add($"No updates available: {g.ModelNumber}, ServiceTag: {(string.IsNullOrEmpty(g.DockServiceTag) ? "N/A" : g.DockServiceTag)} Version: {g.FirmwareVersion}");
                        //            cli_FWU_RESPONSE.Result = "PASS";
                        //        }
                        //        else if (g.LogicalDeviceType == "LogicalPen" && ss_1[0].ToUpper().Equals("PEN"))
                        //        {
                        //            cli_FWU_RESPONSE.Model = g.ModelNumber;
                        //            cli_FWU_RESPONSE.ServiceTag = g.DockServiceTag;
                        //            cli_FWU_RESPONSE.FWVersion = g.FirmwareVersion;
                        //            cli_FWU_RESPONSE.FWUpdateRESPONSE.Add($"No updates available: {g.ModelNumber}, ServiceTag: {(string.IsNullOrEmpty(g.DockServiceTag) ? "N/A" : g.DockServiceTag)} Version: {g.FirmwareVersion}");
                        //            cli_FWU_RESPONSE.Result = "PASS";
                        //        }
                        //        else if (g.LogicalDeviceType == "LogicalDock" && ss_1[0].ToUpper().Equals("DOCK"))
                        //        {
                        //            cli_FWU_RESPONSE.Model = g.ModelNumber;
                        //            cli_FWU_RESPONSE.ServiceTag = g.DockServiceTag;
                        //            cli_FWU_RESPONSE.FWVersion = g.FirmwareVersion;
                        //            cli_FWU_RESPONSE.FWUpdateRESPONSE.Add($"No updates available: {g.ModelNumber}, ServiceTag: {(string.IsNullOrEmpty(g.DockServiceTag) ? "N/A" : g.DockServiceTag)} Version: {g.FirmwareVersion}");
                        //            cli_FWU_RESPONSE.Result = "PASS";
                        //        }
                        //        else if (ss_1[0].ToUpper().Equals("DONGLE"))
                        //        {
                        //        }
                        //        else if (ss_1[0].ToUpper().Equals("AUDIO"))
                        //        {
                        //            cli_FWU_RESPONSE.Model = g.ModelNumber;
                        //            cli_FWU_RESPONSE.ServiceTag = g.DockServiceTag;
                        //            cli_FWU_RESPONSE.FWVersion = g.FirmwareVersion;
                        //            cli_FWU_RESPONSE.FWUpdateRESPONSE.Add($"No updates available: {g.ModelNumber}, ServiceTag: {(string.IsNullOrEmpty(g.DockServiceTag) ? "N/A" : g.DockServiceTag)} Version: {g.FirmwareVersion}");
                        //            cli_FWU_RESPONSE.Result = "PASS";
                        //}
                        //else
                        //{
                        //    cli_FWU_RESPONSE.Model = g.ModelNumber;
                        //    cli_FWU_RESPONSE.ServiceTag = g.DockServiceTag;
                        //    cli_FWU_RESPONSE.FWVersion = g.FirmwareVersion;
                        //    cli_FWU_RESPONSE.FWUpdateRESPONSE.Add($"No updates available: {g.ModelNumber.ToString()}, ServiceTag: {commandLineInput.Model.ToString()}");
                        //    cli_FWU_RESPONSE.Result = "PASS";
                        //}
                        //else
                        //{
                        //    allFWUpdateResponseInfos.ForEach(_ =>
                        //    {
                        //        var msg = $"No updates available: {_.Model}" + (!string.IsNullOrWhiteSpace(_.ServiceTag) ? $", ServiceTag: {_.ServiceTag}" : "") + $" Version: {_.Version}";
                        //        cli_FWU_RESPONSE.FWUpdateRESPONSE.Add(msg);
                        //    });
                        //}
                        cli_FWU_RESPONSE.Message = "No updates available";
                        writelog("Auto_FWUpdate2 No updates available");
                        return ((int)CLI_ExitCode.NoUpdate, JsonConvert.SerializeObject(cli_FWU_RESPONSE, Formatting.Indented));
                    }

                    // add start @ 20241202 stephen
                    foreach (FWUpdateInfo info in fwUpdateInfoPackage.FWUpdateInfo)
                    {
                        info.Guid = commandLineInput.remote_mgr_guid;
                        info.IsUOD = isUODMode &&
                             (info.DeviceType == DeviceType.PhysicalWiredDock ||
                              info.DeviceType == DeviceType.LogicalDock);
                    }

                    _devMgr.updateFWUpdateInfoPackage(fwUpdateInfoPackage);
                    // add end @ 20241202

                    if (deviceTypes.Count != 0)
                    {
                        fwUpdateInfoPackage.FWUpdateInfo.ForEach(_ =>
                        {
                            var msg = $"Ready to start updating Device: {_.Model}" + $" ServiceTag: {(!string.IsNullOrWhiteSpace(_.ServiceTag) ? _.ServiceTag : "N/A")}" + $" to Version: {_.TheLatestVersion}";
                            cli_FWU_RESPONSE.FWUpdateRESPONSE.Add(msg);
                        });

                        fwUpdateDeviceInfos.Where(_ => !fwUpdateInfoPackage.FWUpdateInfo.Select(x => x.DeviceId.Replace("{", "").Replace("}", "")).Contains(_.ID.ToString()))
                                           .ToList()
                                           .ForEach(_ =>
                                           {
                                               var msg = $"No updates available: {_.ModelNumber}" + $" ServiceTag: {(!string.IsNullOrWhiteSpace(_.DockServiceTag) ? _.DockServiceTag : "N/A")}" + $" to Version: {_.FirmwareVersion}";
                                               cli_FWU_RESPONSE.FWUpdateRESPONSE.Add(msg);
                                           });

                        Task.Run(new Action(() =>
                        {
                            retFWUpdateInfos = _devMgr.DownloadAndInstall(fwUpdateInfoPackage.FWUpdateInfo, false, isShowInfo).Result;

                            foreach (FWUpdateInfo retFWUpdateInfo in retFWUpdateInfos)
                            {
                                if (retFWUpdateInfo.FWUErrorCode == FWUErrorCode.NoError)
                                {
                                    cli_FWU_RESPONSE.FWUpdateRESPONSE.Add($"{retFWUpdateInfo.Model} update success.");
                                    cli_FWU_RESPONSE.Result = "PASS";
                                }
                                else
                                {
                                    cli_FWU_RESPONSE.FWUpdateRESPONSE.Add($"{retFWUpdateInfo.Model} update fail. Fail message:{retFWUpdateInfo.FWUErrorCode.ToString()}");
                                    cli_FWU_RESPONSE.Result = "FAIL";
                                }
                            }
                            FWResultReceived_List?.Invoke(this, (retFWUpdateInfos, JsonConvert.SerializeObject(cli_FWU_RESPONSE, Formatting.Indented)));
                            _devMgr.ProgressUpdate_Notify -= _FWUpdatePlugin_ProgressUpdate;
                            //_devMgr.DownloadAndInstall_Result_Notify -= Download_Event;
                        }));
                        writelog("Auto_FWUpdate2 SUCCESS");
                        return ((int)CLI_ExitCode.success, JsonConvert.SerializeObject(cli_FWU_RESPONSE, Formatting.Indented));
                    }
                    else
                    {
                        cli_FWU_RESPONSE.Result = "FAIL";
                        writelog("Auto_FWUpdate2 FAIL");
                        return ((int)CLI_ExitCode.fail_NotSupport, JsonConvert.SerializeObject(cli_FWU_RESPONSE, Formatting.Indented));
                    }
                }
                writelog("Auto_FWUpdate2 SUCCESS");
                return ((int)CLI_ExitCode.success, JsonConvert.SerializeObject(cli_FWU_RESPONSE, Formatting.Indented));
            }
            catch
            {
                writelog("Auto_FWUpdate2 FAIL");
                cli_FWU_RESPONSE.Result = "FAIL";
                return ((int)CLI_ExitCode.fail_FWUpdate, JsonConvert.SerializeObject(cli_FWU_RESPONSE, Formatting.Indented));
            }
        }

        private (int code, string result) Auto_FWUpdate_display(CommandLineInput commandLineInput, CLI_FWU_RESPONSE cli_FWU_RESPONSE, List<MonitorInfo> fwUpdateMonitorInfos, string installPath, bool isShowInfo = true, bool isForce = false, List<string> serviceTags = null, List<string> model = null, string miniver = null, bool isDefer = false)
        {
            try
            {
                _devMgr.ProgressUpdate_Notify -= _FWUpdatePlugin_ProgressUpdate;
                _devMgr.ProgressUpdate_Notify += _FWUpdatePlugin_ProgressUpdate;
                //_devMgr.DownloadAndInstall_Result_Notify -= Download_Event;
                //_devMgr.DownloadAndInstall_Result_Notify += Download_Event;
                if (installPath != "")
                {
                    cli_FWU_RESPONSE.Result = "PASS";
                    Task.Run(new Action(() =>
                    {
                        writelog("Auto_FWUpdate_display Install");
                        FWUErrorCode ret = _devMgr.Install(installPath, true).Result;
                        Trace.WriteLine($"ret = {ret}");

                        if (ret == FWUErrorCode.NoError)
                        {
                            cli_FWU_RESPONSE.Result = "PASS";
                        }
                        else
                        {
                            cli_FWU_RESPONSE.FWUpdateRESPONSE.Add($"Fail message:{ret.ToString()}");
                            cli_FWU_RESPONSE.Result = "FAIL";
                        }
                        FWResultReceived?.Invoke(this, (ret, JsonConvert.SerializeObject(cli_FWU_RESPONSE, Formatting.Indented)));
                        _devMgr.ProgressUpdate_Notify -= _FWUpdatePlugin_ProgressUpdate;
                        _devMgr.DownloadAndInstall_Result_Notify -= Download_Event;
                    }));
                }
                else
                {
                    FWUpdateInfoPackage allFWUpdateInfo = _devMgr.GetFWUpdateInfo(true).Result;
                    var fwUpdateInfoPackage = Filter(allFWUpdateInfo, null, null, model, miniver, null);

                    var allFWUpdateResponseInfos = fwUpdateInfoPackage.FWUpdateInfo
                                                                      .Select(_ => new FWUpdateResponseInfo
                                                                      {
                                                                          Model = _.Model,
                                                                          ServiceTag = _.ServiceTag,
                                                                          Version = _.DeviceVersion
                                                                      })
                                                                      .Concat(fwUpdateMonitorInfos.Where(_ => !fwUpdateInfoPackage.FWUpdateInfo
                                                                                                                                  .Select(x => x.ServiceTag)
                                                                                                                                  .Contains(_.edid.ServiceTag))
                                                                                                 .Select(_ => new FWUpdateResponseInfo
                                                                                                 {
                                                                                                     Model = _.modelName,
                                                                                                     ServiceTag = _.edid.ServiceTag,
                                                                                                     Version = _.FwVersion
                                                                                                 }))
                                                                      .ToList();

                    cli_FWU_RESPONSE.Model = string.Join(",", allFWUpdateResponseInfos.Select(_ => _.Model));
                    cli_FWU_RESPONSE.ServiceTag = string.Join(",", allFWUpdateResponseInfos.Select(_ => !string.IsNullOrWhiteSpace(_.ServiceTag) ? _.ServiceTag : "N/A"));
                    cli_FWU_RESPONSE.FWVersion = string.Join(",", allFWUpdateResponseInfos.Select(_ => $"[{_.Version}]"));

                    var serialNumbers = new List<string>();
                    var marketingNames = new List<string>();
                    var indexs = new List<int>();
                    foreach (var info in allFWUpdateResponseInfos)
                    {
                        var monitor = fwUpdateMonitorInfos.FirstOrDefault(_ => _.modelName.Equals(info.Model, StringComparison.OrdinalIgnoreCase) && _.edid.ServiceTag.Equals(info.ServiceTag, StringComparison.OrdinalIgnoreCase));

                        if (monitor != null)
                        {
                            serialNumbers.Add(monitor.edid.SerialNumber);
                            marketingNames.Add(monitor.MarketingName);
                            indexs.Add(monitor.Index + 1);
                        }
                    }
                    cli_FWU_RESPONSE.SerialNumber = string.Join(",", serialNumbers);
                    cli_FWU_RESPONSE.MarketingName = string.Join(",", marketingNames);
                    cli_FWU_RESPONSE.Index = string.Join(",", indexs);
                    cli_FWU_RESPONSE.Result = "PASS";

                    if (fwUpdateInfoPackage.FWUpdateInfo.Count <= 0)
                    {
                        fwUpdateMonitorInfos.ForEach(_ => cli_FWU_RESPONSE.FWUpdateRESPONSE.Add($"No updates available: {_.modelName}, ServiceTag: {_.edid.ServiceTag} Version: {_.FwVersion}"));
                        cli_FWU_RESPONSE.Message = "No updates available";
                        writelog("Auto_FWUpdate_display No updates available");
                        return ((int)CLI_ExitCode.NoUpdate, JsonConvert.SerializeObject(cli_FWU_RESPONSE, Formatting.Indented));
                    }


                    // add start @ 20250401 stephen
                    foreach (FWUpdateInfo info in fwUpdateInfoPackage.FWUpdateInfo)
                    {
                        info.Guid = commandLineInput.remote_mgr_guid;
                    }

                    _devMgr.updateFWUpdateInfoPackage(fwUpdateInfoPackage);
                    // add end @ 20250401

                    cli_FWU_RESPONSE.FWUpdateRESPONSE.AddRange(fwUpdateInfoPackage.FWUpdateInfo.Select(_ => $"Ready to start updating Device:{_.Model}, ServiceTag: {_.ServiceTag} to Version: {_.TheLatestVersion}"));

                    fwUpdateMonitorInfos.Where(_ => !fwUpdateInfoPackage.FWUpdateInfo.Select(x => x.ServiceTag).Contains(_.edid.ServiceTag))
                            .ToList()
                            .ForEach(_ => cli_FWU_RESPONSE.FWUpdateRESPONSE.Add($"No updates available: {_.modelName}, ServiceTag: {_.edid.ServiceTag} Version: {_.FwVersion}"));

                    Task.Run(new Action(() =>
                    {
                        retFWUpdateInfos = _devMgr.DownloadAndInstall(fwUpdateInfoPackage.FWUpdateInfo, false, isShowInfo).Result;

                        foreach (FWUpdateInfo retFWUpdateInfo in retFWUpdateInfos)
                        {
                            if (retFWUpdateInfo.FWUErrorCode == FWUErrorCode.NoError)
                            {
                                cli_FWU_RESPONSE.FWUpdateRESPONSE.Add($"{retFWUpdateInfo.Model} update success.");
                                cli_FWU_RESPONSE.Result = "PASS";
                            }
                            else
                            {
                                cli_FWU_RESPONSE.FWUpdateRESPONSE.Add($"{retFWUpdateInfo.Model} update fail. Fail message:{retFWUpdateInfo.FWUErrorCode.ToString()}");
                                cli_FWU_RESPONSE.Result = "FAIL";
                            }
                        }
                        FWResultReceived_List?.Invoke(this, (retFWUpdateInfos, JsonConvert.SerializeObject(cli_FWU_RESPONSE, Formatting.Indented)));
                        _devMgr.ProgressUpdate_Notify -= _FWUpdatePlugin_ProgressUpdate;
                        //_devMgr.DownloadAndInstall_Result_Notify -= Download_Event;
                    }));
                }
                writelog("Auto_FWUpdate_display success");
                return ((int)CLI_ExitCode.success, JsonConvert.SerializeObject(cli_FWU_RESPONSE, Formatting.Indented));
            }
            catch
            {
                writelog("Auto_FWUpdate_display FAIL");
                cli_FWU_RESPONSE.Result = "FAIL";
                return ((int)CLI_ExitCode.fail_FWUpdate, JsonConvert.SerializeObject(cli_FWU_RESPONSE, Formatting.Indented));
            }
        }

        private FWUpdateInfoPackage Filter(FWUpdateInfoPackage _fWUpdateInfoPackage, List<string> giuds, List<string> serviceTags, List<string> models, string minVersion, List<DeviceType> deviceTypes)
        {
            writelog($"{nameof(Filter)} start");
            FWUpdateInfoPackage _forCLI_FWUpdateInfoPackage = new FWUpdateInfoPackage();
            _forCLI_FWUpdateInfoPackage.FWUpdateInfo = new List<FWUpdateInfo>();
            List<FWUpdateInfo> FWU_List = new List<FWUpdateInfo>();
            if (FWU_List != null)
            {
                if (giuds != null)
                {
                    writelog($"{nameof(Filter)} Giuds go");
                    foreach (string s in giuds)
                    {
                        writelog($"{nameof(Filter)} Giuds : {s}");
                        foreach (FWUpdateInfo fWUpdateInfo in _fWUpdateInfoPackage.FWUpdateInfo.FindAll(o => o.DeviceId.ToLower().Replace("{", "").Replace("}", "").Equals(s.ToLower())))
                        {
                            writelog($"{nameof(Filter)} fWUpdateInfo.DeviceId : {fWUpdateInfo.DeviceId}");
                            FWU_List.Add(fWUpdateInfo);
                        }
                    }
                    writelog($"{nameof(Filter)} Giuds done");
                }
                else if (serviceTags != null)
                {
                    writelog($"{nameof(Filter)} serviceTags go");
                    foreach (string s in serviceTags)
                    {
                        writelog($"{nameof(Filter)} serviceTags : {s}");
                        foreach (FWUpdateInfo fWUpdateInfo in _fWUpdateInfoPackage.FWUpdateInfo.FindAll(o => o.ServiceTag.ToLower().Equals(s.ToLower())))
                        {
                            writelog($"{nameof(Filter)} fWUpdateInfo.ServiceTag : {fWUpdateInfo.ServiceTag}");
                            FWU_List.Add(fWUpdateInfo);
                        }
                    }
                    writelog($"{nameof(Filter)} serviceTags go");
                }
                else
                {
                    writelog($"{nameof(Filter)} no filter start");
                    foreach (FWUpdateInfo fWUpdateInfo in _fWUpdateInfoPackage.FWUpdateInfo)
                    {
                        FWU_List.Add(fWUpdateInfo);
                    }
                    writelog($"{nameof(Filter)} no filter done");
                }
                if (models != null)
                {
                    writelog($"{nameof(Filter)} models go");
                    if (FWU_List.Count > 0)
                    {
                        List<FWUpdateInfo> FWU_ListByModel = new List<FWUpdateInfo>();
                        foreach (string s in models)
                        {
                            writelog($"{nameof(Filter)} models : {s}");
                            foreach (FWUpdateInfo fWUpdateInfo in _fWUpdateInfoPackage.FWUpdateInfo.FindAll(o => o.Model.ToLower().Equals(s.ToLower())))
                            {
                                writelog($"{nameof(Filter)} fWUpdateInfo.Model : {fWUpdateInfo.Model}");
                                FWU_ListByModel.Add(fWUpdateInfo);
                            }
                        }
                        FWU_List.Clear();
                        FWU_List = FWU_ListByModel;
                    }
                    writelog($"{nameof(Filter)} models done");
                }
                if (!string.IsNullOrEmpty(minVersion))
                {
                    writelog($"{nameof(Filter)} minVersion go");
                    writelog($"{nameof(Filter)} minVersion : {minVersion}");
                    if (FWU_List.Count > 0)
                    {
                        foreach (FWUpdateInfo fWUpdateInfo in FWU_List)
                        {
                            int currentVersion = -1;
                            int new_MinVersion = -1;
                            if (fWUpdateInfo.IsDisplay)
                            {
                                writelog($"{nameof(Filter)} minVersion IsDisplay");
                                for (int j = minVersion.Length - 1; j >= 0; j--)
                                {
                                    if (char.IsLetter(minVersion[j]))
                                    {
                                        int index = j + 1;
                                        int.TryParse(minVersion.Substring(index, minVersion.Length - index), out new_MinVersion);
                                        break;
                                    }
                                }
                                for (int j = fWUpdateInfo.DeviceVersion.Length - 1; j >= 0; j--)
                                {
                                    if (char.IsLetter(fWUpdateInfo.DeviceVersion[j]))
                                    {
                                        int index = j + 1;
                                        int.TryParse(fWUpdateInfo.DeviceVersion.Substring(index, fWUpdateInfo.DeviceVersion.Length - index), out currentVersion);
                                        break;
                                    }
                                }
                            }
                            else
                            {
                                writelog($"{nameof(Filter)} minVersion Is not Display");
                                string temp = string.Empty;
                                if (fWUpdateInfo.DeviceVersion.Contains("."))
                                {
                                    temp = fWUpdateInfo.DeviceVersion.Replace(".", "");
                                }
                                else
                                {
                                    temp = fWUpdateInfo.DeviceVersion;
                                }
                                if (int.TryParse(temp, out currentVersion))
                                {

                                }
                                string temp_NewVersion = string.Empty;
                                if (minVersion.Contains("."))
                                {
                                    temp_NewVersion = minVersion.Replace(".", "");
                                }
                                else
                                {
                                    temp_NewVersion = minVersion;
                                }
                                if (int.TryParse(temp_NewVersion, out new_MinVersion))
                                {

                                }
                            }
                            writelog($"{nameof(Filter)} minVersion currentVersion:{currentVersion}");
                            writelog($"{nameof(Filter)} minVersion new_MinVersion:{new_MinVersion}");
                            if (currentVersion > 0 && new_MinVersion > 0 &&
                                currentVersion < new_MinVersion)
                            {
                                _forCLI_FWUpdateInfoPackage.FWUpdateInfo.Add(fWUpdateInfo);
                            }
                        }
                    }
                    writelog($"{nameof(Filter)} minVersion done");
                }
                else
                {
                    writelog($"{nameof(Filter)} no minVersion");
                    _forCLI_FWUpdateInfoPackage.FWUpdateInfo = FWU_List;
                }

                if (deviceTypes != null)
                {
                    writelog($"{nameof(Filter)} deviceTypes go");
                    if (_forCLI_FWUpdateInfoPackage.FWUpdateInfo.Count > 0)
                    {
                        List<FWUpdateInfo> FWU_ListByDeviceType = new List<FWUpdateInfo>();
                        foreach (var deviceType in deviceTypes)
                        {
                            writelog($"{nameof(Filter)} deviceTypes : {deviceType}");
                            foreach (FWUpdateInfo fWUpdateInfo in _forCLI_FWUpdateInfoPackage.FWUpdateInfo.FindAll(o => o.DeviceType.Equals(deviceType)))
                            {
                                writelog($"{nameof(Filter)} fWUpdateInfo.DeviceType : {fWUpdateInfo.DeviceType}");
                                FWU_ListByDeviceType.Add(fWUpdateInfo);
                            }
                        }
                        _forCLI_FWUpdateInfoPackage.FWUpdateInfo = FWU_ListByDeviceType;
                    }
                    writelog($"{nameof(Filter)} deviceTypes done");
                }
                else
                {
                    writelog($"{nameof(Filter)} no deviceTypes, is display");
                    if (_forCLI_FWUpdateInfoPackage.FWUpdateInfo.Count > 0)
                    {
                        _forCLI_FWUpdateInfoPackage.FWUpdateInfo = _forCLI_FWUpdateInfoPackage.FWUpdateInfo.FindAll(o => o.IsDisplay == true);
                    }
                }
            }
            writelog($"{nameof(Filter)} done");
            return _forCLI_FWUpdateInfoPackage;
        }

        private (DeviceType deviceType, List<DeviceType> deviceTypes) SetDevice(CommandLineInput commandLineInput)
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
                case "SPEAKER":
                    deviceTypes.Add(DeviceType.LogicalWiredAudio);
                    deviceTypes.Add(DeviceType.PhysicalWiredAudio);
                    deviceTypes.Add(DeviceType.PhysicalAudioDongle);
                    deviceTypes.Add(DeviceType.PhysicalBluetoothAudio);
                    deviceType = DeviceType.LogicalWiredAudio;
                    break;
                case "SOUNDBAR":
                    deviceTypes.Add(DeviceType.LogicalWiredAudio);
                    deviceTypes.Add(DeviceType.PhysicalWiredAudio);
                    deviceTypes.Add(DeviceType.PhysicalAudioDongle);
                    deviceTypes.Add(DeviceType.PhysicalBluetoothAudio);
                    deviceType = DeviceType.LogicalWiredAudio;
                    break;
                case "AUDIO":
                    deviceTypes.Add(DeviceType.LogicalWiredAudio);
                    deviceTypes.Add(DeviceType.LogicalHeadset);
                    deviceTypes.Add(DeviceType.PhysicalWiredAudio);
                    deviceTypes.Add(DeviceType.PhysicalAudioDongle);
                    deviceTypes.Add(DeviceType.PhysicalBluetoothAudio);
                    deviceTypes.Add(DeviceType.LogicalAirAudio);
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
                case "DONGLE":
                    deviceTypes.Add(DeviceType.PhysicalAudioDongle);
                    deviceTypes.Add(DeviceType.PhysicalDongle);
                    deviceType = DeviceType.PhysicalDongle;
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

            List<DeviceInfo> deviceInfoList = null;
            deviceInfoList = _devMgr.GetDevices().Result.deviceInfo;


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
            bool noupdtae = false;
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
                        if (commandLineInput.Options.Count == 0)
                        {
                            commandLineInput.Options.Insert(0, new CommandType_Option("VALUE", "MANUAL,FORCEWITHNOTICE"));
                        }
                        if (!commandLineInput.Options[0].Option_Value.Contains(','))
                        {
                            commandLineInput.Options[0].Option_Value += ",FORCEWITHNOTICE"; //2025/1/1 Elie, fix Cli for SW update with Automatic command is going to add a DEFER. (Anfernee's comment)
                        }
                        if (commandLineInput.Options.Count > 0)
                        {
                            writelog("FWUpdate_Line 3902");
                            string[] ss_1 = commandLineInput.Options[0].Option_Value.Split(",");
                            if (ss_1.Length == 2)
                            {
                                writelog("FWUpdate_Line 3906");
                                if (!string.IsNullOrEmpty(ss_1[0]) && !string.IsNullOrEmpty(ss_1[1]))
                                {
                                    writelog("FWUpdate_Line 3909");
                                    if (ss_1[1].ToUpper().Equals("FORCEWITHNOTICE") && commandLineInput.Options.Count == 1)
                                    {
                                        writelog("FWUpdate_Line 3912");
                                        SWUpdateInfoPackage swUpdateInfoPackage = _devMgr.SW_GetSWUpdateInfo(true, true).Result;
                                        if (swUpdateInfoPackage.SWUpdateInfo.Count == 0)
                                            noupdtae = true;
                                        else
                                        {
                                            cLI_SWU_RESPONSE.SWname = string.Join(",", swUpdateInfoPackage.SWUpdateInfo.Select(_ => _.SoftwareName));
                                            cLI_SWU_RESPONSE.SWVersion = string.Join(",", swUpdateInfoPackage.SWUpdateInfo.Select(_ => $"[{_.SoftwareVersion}]"));
                                            cLI_SWU_RESPONSE.SWUpdateRESPONSE.AddRange(swUpdateInfoPackage.SWUpdateInfo.Select(_ => $"Ready to start updating Device:{_.SoftwareName} to Version:{_.TheLatestVersion}"));
                                            //cLI_SWU_RESPONSE.Result = "PASS";
                                        }
                                        Trace.WriteLine($"swUpdateInfoPackage {swUpdateInfoPackage}");
                                        _devMgr.SW_DownloadAndInstall(swUpdateInfoPackage.SWUpdateInfo, false, installPath);
                                        ret = true;
                                    }
                                    //peripherals no guid minversion model
                                    else if (ss_1[1].ToUpper().Equals("FORCEWITHNONOTICE") && commandLineInput.Options.Count == 1)
                                    {
                                        writelog("FWUpdate_Line 3922");
                                        SWUpdateInfoPackage swUpdateInfoPackage = _devMgr.SW_GetSWUpdateInfo(false, true).Result;
                                        if (swUpdateInfoPackage.SWUpdateInfo.Count == 0)
                                            noupdtae = true;
                                        else
                                        {
                                            cLI_SWU_RESPONSE.SWname = string.Join(",", swUpdateInfoPackage.SWUpdateInfo.Select(_ => _.SoftwareName));
                                            cLI_SWU_RESPONSE.SWVersion = string.Join(",", swUpdateInfoPackage.SWUpdateInfo.Select(_ => $"[{_.SoftwareVersion}]"));
                                            cLI_SWU_RESPONSE.SWUpdateRESPONSE.AddRange(swUpdateInfoPackage.SWUpdateInfo.Select(_ => $"Ready to start updating Device:{_.SoftwareName} to Version:{_.TheLatestVersion}"));
                                            //cLI_SWU_RESPONSE.Result = "PASS";
                                        }
                                        Trace.WriteLine($"swUpdateInfoPackage {swUpdateInfoPackage}");
                                        _devMgr.SW_DownloadAndInstall(swUpdateInfoPackage.SWUpdateInfo, false, installPath);
                                        ret = true;
                                    }
                                    else if (ss_1[1].ToUpper().Equals("DEFER") && commandLineInput.Options.Count == 1)
                                    {
                                        writelog("FWUpdate_Line 3931");
                                        SWUpdateInfoPackage swUpdateInfoPackage = _devMgr.SW_GetSWUpdateInfo(true, false).Result;
                                        if (swUpdateInfoPackage.SWUpdateInfo.Count == 0)
                                            noupdtae = true;
                                        else
                                        {
                                            cLI_SWU_RESPONSE.SWname = string.Join(",", swUpdateInfoPackage.SWUpdateInfo.Select(_ => _.SoftwareName));
                                            cLI_SWU_RESPONSE.SWVersion = string.Join(",", swUpdateInfoPackage.SWUpdateInfo.Select(_ => $"[{_.SoftwareVersion}]"));
                                            cLI_SWU_RESPONSE.SWUpdateRESPONSE.AddRange(swUpdateInfoPackage.SWUpdateInfo.Select(_ => $"Ready to start updating Device:{_.SoftwareName} to Version:{_.TheLatestVersion}"));
                                            //cLI_SWU_RESPONSE.Result = "PASS";
                                        }
                                        Trace.WriteLine($"swUpdateInfoPackage {swUpdateInfoPackage}");
                                        ret = true;
                                    }
                                    else
                                    {
                                        writelog("FWUpdate_Line 3938");
                                        somethingError = true;
                                    }
                                }
                                else
                                {
                                    writelog("FWUpdate_Line 3944");
                                    somethingError = true;
                                }
                            }
                            else
                            {
                                writelog("FWUpdate_Line 3950");
                                somethingError = true;
                            }
                            if (noupdtae)
                            {
                                _GlobalSettingParam = _devMgr.GetGlobalSettingParam().Result;
                                cLI_SWU_RESPONSE.SWname = "DDPM";
                                cLI_SWU_RESPONSE.SWVersion = $"[{_GlobalSettingParam.GlobalSetting_About.SWVersion}]";
                                cLI_SWU_RESPONSE.Message = "No update availible";
                                cLI_SWU_RESPONSE.Result = "PASS";
                            }
                        }
                        //else if (commandLineInput.Options.Count == 0)
                        //{
                        //    writelog("FWUpdate_Line 3957");
                        //    SWUpdateInfoPackage swUpdateInfoPackage = _devMgr.SW_GetSWUpdateInfo(true, false, true).Result;
                        //    Trace.WriteLine($"swUpdateInfoPackage {swUpdateInfoPackage}");
                        //    ret = true;
                        //    _devMgr.SW_DownloadAndInstall(swUpdateInfoPackage.SWUpdateInfo, false, installPath);
                        //}
                        else
                        {
                            writelog("FWUpdate_Line 3965");
                            cLI_SWU_RESPONSE.Message = "Input FAIL";
                            ret = false;
                        }
                        cLI_SWU_RESPONSE.Result = ret == true ? "PASS" : "FAIL";
                        break;

                    case "UPDATESOURCELOCATION":

                        if (commandLineInput.Options.Count > 0)
                        {
                            writelog("FWUpdate_Line 3976");
                            if (commandLineInput.Options[0].Option_Value.ToUpper() == "ON" || commandLineInput.Options[0].Option_Value.ToUpper() == "OFF")
                            {
                                writelog("FWUpdate_Line 3979");
                                ret = _devMgr.SetServerURL(commandLineInput.Options[0].Option_Value.ToString()).Result;
                            }
                            else if (commandLineInput.Options[0].Option_Value.ToUpper().Contains(":\\"))
                            {
                                writelog("FWUpdate_Line 3984");
                                string path = File.ReadAllText($"{commandLineInput.Options[0].Option_Value}");
                                ret = _devMgr.SetServerURL(path).Result;
                            }
                            else
                            {
                                writelog("FWUpdate_Line 3990");
                                somethingError = true;
                            }
                        }
                        else
                        {
                            writelog("FWUpdate_Line 3996");
                            somethingError = true;
                        }
                        break;

                    default:
                        writelog("FWUpdate_Line 4002");
                        cLI_SWU_RESPONSE.Message = "Input FAIL";
                        ret = false;
                        break;
                }
                cLI_RESPONSE.Result = ret == true ? "PASS" : "FAIL";
                if (cLI_SWU_RESPONSE != null)
                {
                    writelog("FWUpdate_Line 4010");
                    output = JsonConvert.SerializeObject(cLI_SWU_RESPONSE, Formatting.Indented);
                    //output = cLI_SWU_RESPONSE.OutputLog(cLI_SWU_RESPONSE, commandLineInput);
                }
                else
                {
                    writelog("FWUpdate_Line 4015");
                    output = JsonConvert.SerializeObject(cLI_RESPONSE, Formatting.Indented);
                    //output = cLI_RESPONSE.OutputLog(cLI_RESPONSE, commandLineInput);
                }
                if (ret == true)
                {
                    writelog("FWUpdate_Line 4020");
                    return ((int)CLI_ExitCode.success, output);
                }
                else if (somethingError)
                {
                    writelog("FWUpdate_Line 4025");
                    CLI_RESPONSE rsp = new CLI_RESPONSE()
                    {
                        Command = commandLineInput.Command,
                        TargetFeature = commandLineInput.TargetFeature,
                        Result = "FAIL",
                        Message = "FW update failure",
                    };
                    System.Console.WriteLine(JsonConvert.SerializeObject(rsp, Formatting.Indented));
                    return ((int)CLI_ExitCode.fail_FWUpdate, JsonConvert.SerializeObject(rsp, Formatting.Indented));
                }
                else
                {
                    writelog("FWUpdate_Line 4038");
                    CLI_RESPONSE rsp = new CLI_RESPONSE()
                    {
                        Command = commandLineInput.Command,
                        TargetFeature = commandLineInput.TargetFeature,
                        Result = "FAIL",
                        Message = "FW update failure",
                    };
                    System.Console.WriteLine(JsonConvert.SerializeObject(rsp, Formatting.Indented));
                    return ((int)CLI_ExitCode.fail_FWUpdate, JsonConvert.SerializeObject(rsp, Formatting.Indented));
                }
            }
            catch
            {
                writelog("FWUpdate_Line 4052");
                CLI_RESPONSE rsp = new CLI_RESPONSE()
                {
                    Command = commandLineInput.Command,
                    TargetFeature = commandLineInput.TargetFeature,
                    Result = "FAIL",
                    Message = "FW update failure",
                };
                System.Console.WriteLine(JsonConvert.SerializeObject(rsp, Formatting.Indented));
                return ((int)CLI_ExitCode.fail_FWUpdate, JsonConvert.SerializeObject(rsp, Formatting.Indented));
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

            List<DeviceInfo> deviceInfoList = null;
            deviceInfoList = _devMgr.GetDevices().Result.deviceInfo;


            if (!commandLineInput.isCliRunAdmin)
            {
                cLI_RESPONSE.Result = "FAIL";
                cLI_RESPONSE.Message = "Not Admin";
                writelog("SWAPPUpdate_get FAIL Not Admin");
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
                        if (commandLineInput.Options.Count == 0)
                        {
                            SWUpdateInfoPackage swUpdateInfoPackage = _devMgr.SW_GetSWUpdateInfo(true, false).Result;
                            Trace.WriteLine($"swUpdateInfoPackage {swUpdateInfoPackage}");
                            if (swUpdateInfoPackage.SWUpdateInfo.Count == 0)
                            {
                                _GlobalSettingParam = _devMgr.GetGlobalSettingParam().Result;
                                cLI_SWU_RESPONSE.SWname = "DDPM";
                                cLI_SWU_RESPONSE.SWVersion = $"[{_GlobalSettingParam.GlobalSetting_About.SWVersion}]";
                                cLI_SWU_RESPONSE.Message = "No update availible";
                                //cLI_SWU_RESPONSE.Result = "PASS";
                                ret = true;
                            }
                            else
                            {
                                cLI_SWU_RESPONSE.SWname = string.Join(",", swUpdateInfoPackage.SWUpdateInfo.Select(_ => _.SoftwareName));
                                cLI_SWU_RESPONSE.SWVersion = string.Join(",", swUpdateInfoPackage.SWUpdateInfo.Select(_ => $"[{_.SoftwareVersion}]"));
                                cLI_SWU_RESPONSE.SWUpdateRESPONSE.AddRange(swUpdateInfoPackage.SWUpdateInfo.Select(_ => $"Ready to start updating Device:{_.SoftwareName} to Version:{_.TheLatestVersion}"));
                                //cLI_SWU_RESPONSE.Result = "PASS";
                                ret = true;
                            }
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
                    output = JsonConvert.SerializeObject(cLI_SWU_RESPONSE, Formatting.Indented);
                }
                else
                {
                    output = JsonConvert.SerializeObject(cLI_RESPONSE, Formatting.Indented);
                }
                if (ret == true)
                {
                    writelog("SWAPPUpdate_get SUCCESS");
                    return ((int)CLI_ExitCode.success, output);
                }
                else
                {
                    writelog("SWAPPUpdate_get FAIL");
                    Console.WriteLine(output);
                    return ((int)CLI_ExitCode.fail_SWUpdate, output);
                }
            }
            catch
            {
                writelog("SWAPPUpdate_get FAIL");
                return ((int)CLI_ExitCode.fail_SWUpdate, output);
            }

        }
    }
}