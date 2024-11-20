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
        private const string publisherCompany = "Dell Inc.";
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
                                //case "UNLOCKUIUPDATE":
                                    var ret = FWUpdate(commandLineInput);
                                    result.ExitCode = ret.code;
                                    result.serialize_Json_response = ret.json;
                                    return result;
                            }
                        }
                    }

                }
                //if (commandLineInput.Command.Equals("SET"))
                //{
                //    if (commandLineInput.TargetType.Equals("DOCK"))
                //    {
                //        if (commandLineInput.TargetFeature.Equals("SILENTFWUPDATE"))// for dock firmware update.
                //        {
                //            //switch (commandLineInput.TargetFeature)
                //            //{
                //            //    case "FIRMWAREUPDATE":
                //            //    case "UODFWUPDATE":
                //            //    case "LOCKUIUPDATE":
                //            //    case "UNLOCKUIUPDATE":
                //            var ret = FWUpdate(commandLineInput);
                //            result.ExitCode = ret.code;
                //            result.serialize_Json_response = ret.json;
                //            return result;
                //            //}
                //        }
                //    }

                //}

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
                GetResults.Add(new CLI_PeripheralRESPONSE("N/A", "GET", _commandLineInput.TargetFeature, "Fail", "Device not found"));
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
                }
            }
            else if (_commandLineInput.GuidString.Count != 0)
            {
                _commandLineInput.GuidString.ForEach(x =>
                {
                    if (Guid.TryParse(x, out Guid guid))
                    {
                        var di = _deviceinfo.Where(x => x.ID == guid && x.LogicalDeviceType.ToUpper().Contains(_commandLineInput.PluginsType)).FirstOrDefault();
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
            else if (_commandLineInput.Model.Count != 0)
            {
                _commandLineInput.Model.ForEach(model =>
                {
                    var di = _deviceinfo.Where(_ => _.ModelNumber == model && _.LogicalDeviceType.ToUpper().Contains(_commandLineInput.PluginsType)).FirstOrDefault();
                    if (di == null)
                    {
                        GetResults.Add(new CLI_PeripheralRESPONSE("N/A", "GET", _commandLineInput.TargetFeature, "Fail", "Device not found", null, model));
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
                    var di = _deviceinfo.Where(_ => _.DockServiceTag == serviceTag && _.LogicalDeviceType.ToUpper().Contains(_commandLineInput.PluginsType)).FirstOrDefault();
                    if (di == null)
                    {
                        GetResults.Add(new CLI_PeripheralRESPONSE("N/A", "GET", _commandLineInput.TargetFeature, "Fail", "Device not found", null, null, serviceTag));
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
                            SetResults.Add(new CLI_PeripheralRESPONSE($"{guid}", _commandLineInput.Command, _commandLineInput.TargetFeature, "FAIL", "Device not found"));
                        }
                    }
                    else
                    {
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
                        if (x.LogicalDeviceType.ToUpper().Contains(_commandLineInput.PluginsType) && x.ModelNumber == model)
                        {
                            SetResults.Add(new CLI_PeripheralRESPONSE($"{x.ID}", _commandLineInput.Command, _commandLineInput.TargetFeature, "", "", x.Name, x.ModelNumber, x.DockServiceTag));
                            found = true;
                            //GUID = x.ID.ToString();
                        }
                    });
                    if (!found)
                    {
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
                        SetResults.Add(new CLI_PeripheralRESPONSE("N/A", _commandLineInput.Command, _commandLineInput.TargetFeature, "FAIL", "Device not found"));
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
                            var result = RunAsyncTimeout(_devMgr.SetFactoryResetAsyncValueForHeadset(x.Guid, true)).Result;
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
                            var result = RunAsyncTimeout(_devMgr.SetResetToDefaultAsyncForSoundbar(x.Guid, true)).Result;
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
                            var result = RunAsyncTimeout(_devMgr.SetResetToDefaultAsyncForSoundbar(x.Guid, true)).Result;
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
                            var result = RunAsyncTimeout(_devMgr.SetFactoryResetAsyncValueForHeadset(x.Guid, true)).Result;
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
                        var result = RunAsyncTimeout(_devMgr.ResetToDefault_webcam(x.Guid, true)).Result;
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
                        var result = "0"; //RunAsyncTimeout(_devMgr.(x.Guid, true)).Result;
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
                        var result = "0"; //RunAsyncTimeout(_devMgr.(x.Guid, true)).Result;
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
                        var result = "0"; //RunAsyncTimeout(_devMgr.(x.Guid, true)).Result;
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
                                x.Value = "N/A";
                                x.Result = "FAIL";
                                x.Message = "HeadSet not support ANC";
                                retcode = false;
                            }
                        }
                    });
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
                            if (_devMgr.GetIsMicNoiseCancellationSupportedAsync(x.Guid).Result)
                            {
                                taskB = _devMgr.SetMicNoiseCancellation;
                                RunTaskB(bl);
                                retcode = true;
                            }
                            else
                            {
                                x.Value = "N/A";
                                x.Result = "FAIL";
                                x.Message = "HeadSet not support MICNOISECANCELLATION";
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
                                x.Value = "N/A";
                                x.Result = "FAIL";
                                x.Message = "HeadSet not support WearDetection";
                                retcode = false;
                            }
                        }
                    });
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
                    SetResults.ForEach(x =>
                    {
                        x.Value = "";
                        if (x.Result == "")
                        {
                            var di = _deviceinfo.Where(_ => _.ID.ToString() == x.Guid && _.LogicalDeviceType.ToUpper().Contains(_commandLineInput.PluginsType)).FirstOrDefault();
                            if (di.IsMicEnumerationSupported)
                            {
                                var result = RunAsyncTimeout(_devMgr.SetIsMicEnumerationOn(bl, Guid.Parse(x.Guid))).Result;
                                if (result == "0")
                                {
                                    x.Result = "PASS";
                                    retcode = di.IsMicEnumerationOn;
                                    x.Value = (retcode) ? "ON" : "OFF";
                                    x.Value += "," + (data.LockSettings.Lock_Webcam_MicSwitch ? "LOCK" : "UNLOCK");
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
                                x.Value = "N/A";
                                x.Result = "FAIL";
                                x.Message = "Webcam not support MicSwitch";
                                retcode = false;
                            }
                        }
                    });
                    return retcode ? (int)CLI_ExitCode.success : (int)CLI_ExitCode.functional_error;
                    taskB = _devMgr.SetIsMicEnumerationOn;
                    RunTaskD(bl);
                    return (int)CLI_ExitCode.success;

                case "HDR":
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
                            else
                            {
                                x.Value = "N/A";
                                x.Result = "FAIL";
                                x.Message = "Webcam not support HDR";
                                retcode = false;
                            }
                        }
                    });
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
                    SetResults.ForEach(x =>
                    {
                        x.Value = "";
                        if (x.Result == "")
                        {
                            if (_devMgr.GetIsPropertyAntiFlickerSupported(x.Guid).Result)
                            {
                                var result = RunAsyncTimeout(_devMgr.SetAntiFlicker(x.Guid, val)).Result;
                                if (result == "0")
                                {
                                    x.Result = "PASS";
                                    retvalue = _devMgr.GetAntiFlickerValueByDTP(x.Guid).Result;
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
                            else
                            {
                                x.Value = "N/A";
                                x.Result = "FAIL";
                                x.Message = "Webcam not support AntiFlicker";
                                retcode = false;
                            }
                        }
                    });
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
                    //                retvalue = _devMgr.GetAntiFlickerValueByDTP(GUID).Result;
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
                            else
                            {
                                x.Value = "N/A";
                                x.Result = "FAIL";
                                x.Message = "Webcam not support AI AutoFraming";
                                retcode = false;
                            }
                        }
                    });
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
                    SetResults.ForEach(x =>
                    {
                        x.Value = "";
                        if (x.Result == "")
                        {
                            var di = _deviceinfo.Where(_ => _.ID.ToString() == x.Guid && _.LogicalDeviceType.ToUpper().Contains(_commandLineInput.PluginsType)).FirstOrDefault();
                            if (di.IsESISupported)
                            {
                                var result = RunAsyncTimeout(_devMgr.SetIsProximitySensorEnable(x.Guid, bl)).Result;
                                if (result == "0")
                                {
                                    x.Result = "PASS";
                                    retcode_ = _devMgr.GetIsProximitySensorEnable(x.Guid).Result;
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
                            else
                            {
                                x.Value = "N/A";
                                x.Result = "FAIL";
                                x.Message = "Webcam not support PresenceDetection";
                                retcode = false;
                            }
                        }
                    });
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
        private (int code, string json) FWUpdate_v1(CommandLineInput commandLineInput)
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
                        bool _recode = false;
                        if (commandLineInput.Options.Count > 5)
                        {
                            cLI_FWU_RESPONSE.Result = "FAIL";
                            cLI_FWU_RESPONSE.Message = "Bring in extra strings:";
                            for (int i = 0; i < commandLineInput.Options.Count; i++)
                            {
                                cLI_FWU_RESPONSE.Message += $"{commandLineInput.Options[i].Option_Name}={commandLineInput.Options[i].Option_Value}\n";
                            }
                            break;
                        }
                        for (int i = 0; i < commandLineInput.Options.Count; i++)
                        {
                            Trace.WriteLine($"{commandLineInput.Options[i].Option_Name}={commandLineInput.Options[i].Option_Value}\n");
                        }
                        if (commandLineInput.Options.Count > 0)
                        {
                            cLI_FWU_RESPONSE.Value = commandLineInput.Options[0].Option_Value;
                            string[] ss_1 = commandLineInput.Options[0].Option_Value.Split(",");

                            if (ss_1.Length == 2)
                            {
                                writelog("FWUpdate_Line 1460");
                                if (!string.IsNullOrEmpty(ss_1[0]) && !string.IsNullOrEmpty(ss_1[1]))
                                {
                                    writelog("FWUpdate_Line 1463");
                                    if (ss_1[0].ToUpper().Equals("DISPLAY"))
                                    {
                                        writelog("FWUpdate_Line 1466");
                                        //Check Display is exist
                                        _AllInfoMonitors = _devMgr.GetMonitors().Result;
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
                                            System.Console.WriteLine(JsonConvert.SerializeObject(rsp, Formatting.Indented));
                                            return ((int)CLI_ExitCode.fail_FWUpdate, JsonConvert.SerializeObject(rsp, Formatting.Indented));
                                        }
                                        else
                                        {
                                            writelog("FWUpdate_Line 1484");
                                            //display servicetag & miniver & model
                                            if (commandLineInput.Options.Count == 4)
                                            {
                                                string[] ss_2 = commandLineInput.Options[1].Option_Value.Split(",");
                                                string[] ss_3 = commandLineInput.Options[2].Option_Value.Split(",");
                                                string[] ss_4 = commandLineInput.Options[3].Option_Value.Split(",");
                                                writelog("FWUpdate_Line 1491");
                                                if (ss_2.Length == 2 && ss_3.Length == 2 && ss_4.Length == 2)
                                                {
                                                    writelog("FWUpdate_Line 1494");
                                                    if (!string.IsNullOrEmpty(ss_2[0]) && !string.IsNullOrEmpty(ss_2[1]))
                                                    {
                                                        writelog("FWUpdate_Line 1494");
                                                        if (!string.IsNullOrEmpty(ss_3[0]) && !string.IsNullOrEmpty(ss_3[1]))
                                                        {
                                                            writelog("FWUpdate_Line 1500");
                                                            if (!string.IsNullOrEmpty(ss_4[0]) && !string.IsNullOrEmpty(ss_4[1]))
                                                            {
                                                                writelog("FWUpdate_Line 1503");
                                                                if (ss_1[1].ToUpper().Equals("FORCEWITHNOTICE") && ss_2[1].ToUpper().Equals("SERVICETAG") && ss_3[1].ToUpper().Equals("MINIVERSION") && ss_4[1].ToUpper().Equals("MODEL"))
                                                                {
                                                                    writelog("FWUpdate_Line 1504");
                                                                    List<string> stag = new List<string> { ss_2[0] };
                                                                    string min = ss_3[0];
                                                                    List<string> model = new List<string> { ss_4[0] };
                                                                    isShowInfo = true;

                                                                    var fwupdate = Auto_FWUpdate_display(commandLineInput, cLI_FWU_RESPONSE, installPath, isShowInfo, true, stag, model, min);

                                                                    result.ExitCode = fwupdate.code;
                                                                    result.serialize_Json_response = fwupdate.result;
                                                                    ret = true;
                                                                }
                                                                else if (ss_1[1].ToUpper().Equals("FORCEWITHNONOTICE") && ss_2[1].ToUpper().Equals("SERVICETAG") && ss_3[1].ToUpper().Equals("MINIVERSION") && ss_4[1].ToUpper().Equals("MODEL"))
                                                                {
                                                                    writelog("FWUpdate_Line 1520");
                                                                    List<string> stag = new List<string> { ss_2[0] };
                                                                    string min = ss_3[0];
                                                                    List<string> model = new List<string> { ss_4[0] };
                                                                    isShowInfo = false;
                                                                    var fwupdate = Auto_FWUpdate_display(commandLineInput, cLI_FWU_RESPONSE, installPath, isShowInfo, true, stag, model, min);
                                                                    FWResultReceived_List += Download_Event_2;
                                                                    result.ExitCode = fwupdate.code;
                                                                    result.serialize_Json_response = fwupdate.result;
                                                                    ret = true;
                                                                }
                                                                else if (ss_1[1].ToUpper().Contains(":\\") && ss_2[1].ToUpper().Equals("SERVICETAG") && ss_3[1].ToUpper().Equals("MINIVERSION") && ss_4[1].ToUpper().Equals("MODEL"))
                                                                {
                                                                    writelog("FWUpdate_Line 1533");
                                                                    List<string> stag = new List<string> { ss_2[0] };
                                                                    string min = ss_3[0];
                                                                    List<string> model = new List<string> { ss_4[0] };
                                                                    installPath = Path.GetFullPath(ss_1[1]);
                                                                    Trace.WriteLine($"installPath = {installPath}");
                                                                    isShowInfo = true;
                                                                    var fwupdate = Auto_FWUpdate_display(commandLineInput, cLI_FWU_RESPONSE, installPath, isShowInfo, true, stag, model, min);

                                                                    result.ExitCode = fwupdate.code;
                                                                    result.serialize_Json_response = fwupdate.result;
                                                                    ret = true;

                                                                }
                                                            }
                                                            else
                                                            {
                                                                writelog("FWUpdate_Line 1550");
                                                                somethingError = true;
                                                            }

                                                        }
                                                        else
                                                        {
                                                            writelog("FWUpdate_Line 1557");
                                                            somethingError = true;
                                                        }

                                                    }
                                                    else
                                                    {
                                                        writelog("FWUpdate_Line 1564");
                                                        somethingError = true;
                                                    }
                                                }
                                                else
                                                {
                                                    writelog("FWUpdate_Line 1570");
                                                    somethingError = true;
                                                }


                                            }
                                            //display (servicetag  && miniver) or (servicetag  && model) or (miniver  && model)
                                            else if (commandLineInput.Options.Count == 3)
                                            {
                                                writelog("FWUpdate_Line 1579");
                                                string[] ss_2 = commandLineInput.Options[1].Option_Value.Split(",");
                                                string[] ss_3 = commandLineInput.Options[2].Option_Value.Split(",");

                                                if (ss_2.Length == 2 && ss_3.Length == 2)
                                                {
                                                    writelog("FWUpdate_Line 1585");
                                                    if (!string.IsNullOrEmpty(ss_2[0]) && !string.IsNullOrEmpty(ss_2[1]))
                                                    {
                                                        writelog("FWUpdate_Line 1588");
                                                        if (!string.IsNullOrEmpty(ss_3[0]) && !string.IsNullOrEmpty(ss_3[1]))
                                                        {
                                                            writelog("FWUpdate_Line 1591");
                                                            //display servicetag & miniver 
                                                            if (ss_1[1].ToUpper().Equals("FORCEWITHNOTICE") && ss_2[1].ToUpper().Equals("SERVICETAG") && ss_3[1].ToUpper().Equals("MINIVERSION"))
                                                            {
                                                                writelog("FWUpdate_Line 1595");
                                                                List<string> stag = new List<string> { ss_2[0] };
                                                                string min = ss_3[0];
                                                                isShowInfo = true;

                                                                var fwupdate = Auto_FWUpdate_display(commandLineInput, cLI_FWU_RESPONSE, installPath, isShowInfo, true, stag, null, min);

                                                                result.ExitCode = fwupdate.code;
                                                                result.serialize_Json_response = fwupdate.result;
                                                                ret = true;
                                                            }
                                                            //display servicetag & miniver 
                                                            else if (ss_1[1].ToUpper().Equals("FORCEWITHNONOTICE") && ss_2[1].ToUpper().Equals("SERVICETAG") && ss_3[1].ToUpper().Equals("MINIVERSION"))
                                                            {
                                                                writelog("FWUpdate_Line 1609");
                                                                List<string> stag = new List<string> { ss_2[0] };
                                                                string min = ss_3[0];
                                                                isShowInfo = false;
                                                                var fwupdate = Auto_FWUpdate_display(commandLineInput, cLI_FWU_RESPONSE, installPath, isShowInfo, true, stag, null, min);
                                                                FWResultReceived_List += Download_Event_2;
                                                                result.ExitCode = fwupdate.code;
                                                                result.serialize_Json_response = fwupdate.result;
                                                                ret = true;
                                                            }
                                                            //display servicetag &  miniver 
                                                            else if (ss_1[1].ToUpper().Contains(":\\") && ss_2[1].ToUpper().Equals("SERVICETAG") && ss_3[1].ToUpper().Equals("MINIVERSION"))
                                                            {
                                                                writelog("FWUpdate_Line 1622");
                                                                string min = ss_2[0];
                                                                List<string> stag = new List<string> { ss_2[0] };
                                                                installPath = Path.GetFullPath(ss_1[1]);
                                                                Trace.WriteLine($"installPath = {installPath}");
                                                                isShowInfo = true;
                                                                var fwupdate = Auto_FWUpdate_display(commandLineInput, cLI_FWU_RESPONSE, installPath, isShowInfo, true, stag, null, min);

                                                                result.ExitCode = fwupdate.code;
                                                                result.serialize_Json_response = fwupdate.result;
                                                                ret = true;

                                                            }
                                                            //display servicetag & model
                                                            if (ss_1[1].ToUpper().Equals("FORCEWITHNOTICE") && ss_2[1].ToUpper().Equals("SERVICETAG") && ss_3[1].ToUpper().Equals("MODEL"))
                                                            {
                                                                writelog("FWUpdate_Line 1638");
                                                                List<string> stag = new List<string> { ss_2[0] };
                                                                List<string> model = new List<string> { ss_3[0] };
                                                                isShowInfo = true;

                                                                var fwupdate = Auto_FWUpdate_display(commandLineInput, cLI_FWU_RESPONSE, installPath, isShowInfo, true, stag, model, null);

                                                                result.ExitCode = fwupdate.code;
                                                                result.serialize_Json_response = fwupdate.result;
                                                                ret = true;
                                                            }
                                                            //display servicetag & model
                                                            else if (ss_1[1].ToUpper().Equals("FORCEWITHNONOTICE") && ss_2[1].ToUpper().Equals("SERVICETAG") && ss_3[1].ToUpper().Equals("MODEL"))
                                                            {
                                                                writelog("FWUpdate_Line 1652");
                                                                List<string> stag = new List<string> { ss_2[0] };
                                                                List<string> model = new List<string> { ss_3[0] };
                                                                isShowInfo = false;
                                                                var fwupdate = Auto_FWUpdate_display(commandLineInput, cLI_FWU_RESPONSE, installPath, isShowInfo, true, stag, model, null);
                                                                FWResultReceived_List += Download_Event_2;
                                                                result.ExitCode = fwupdate.code;
                                                                result.serialize_Json_response = fwupdate.result;
                                                                ret = true;
                                                            }
                                                            //display servicetag & model
                                                            else if (ss_1[1].ToUpper().Contains(":\\") && ss_2[1].ToUpper().Equals("SERVICETAG") && ss_3[1].ToUpper().Equals("MODEL"))
                                                            {
                                                                writelog("FWUpdate_Line 1665");
                                                                List<string> stag = new List<string> { ss_2[0] };
                                                                List<string> model = new List<string> { ss_3[0] };
                                                                installPath = Path.GetFullPath(ss_1[1]);
                                                                Trace.WriteLine($"installPath = {installPath}");
                                                                isShowInfo = true;
                                                                var fwupdate = Auto_FWUpdate_display(commandLineInput, cLI_FWU_RESPONSE, installPath, isShowInfo, true, stag, model, null);

                                                                result.ExitCode = fwupdate.code;
                                                                result.serialize_Json_response = fwupdate.result;
                                                                ret = true;

                                                            }
                                                            //display miniver  && model
                                                            if (ss_1[1].ToUpper().Equals("FORCEWITHNOTICE") && ss_2[1].ToUpper().Equals("MINIVERSION") && ss_3[1].ToUpper().Equals("MODEL"))
                                                            {
                                                                writelog("FWUpdate_Line 1681");
                                                                List<string> model = new List<string> { ss_3[0] };
                                                                string min = ss_3[0];
                                                                isShowInfo = true;

                                                                var fwupdate = Auto_FWUpdate_display(commandLineInput, cLI_FWU_RESPONSE, installPath, isShowInfo, true, null, model, min);

                                                                result.ExitCode = fwupdate.code;
                                                                result.serialize_Json_response = fwupdate.result;
                                                                ret = true;
                                                            }
                                                            //display miniver  && model
                                                            else if (ss_1[1].ToUpper().Equals("FORCEWITHNONOTICE") && ss_2[1].ToUpper().Equals("MINIVERSION") && ss_3[1].ToUpper().Equals("MODEL"))
                                                            {
                                                                writelog("FWUpdate_Line 1695");
                                                                string min = ss_3[0];
                                                                List<string> model = new List<string> { ss_3[0] };
                                                                isShowInfo = false;
                                                                var fwupdate = Auto_FWUpdate_display(commandLineInput, cLI_FWU_RESPONSE, installPath, isShowInfo, true, null, model, min);
                                                                FWResultReceived_List += Download_Event_2;
                                                                result.ExitCode = fwupdate.code;
                                                                result.serialize_Json_response = fwupdate.result;
                                                                ret = true;
                                                            }
                                                            //display miniver  && model
                                                            else if (ss_1[1].ToUpper().Contains(":\\") && ss_2[1].ToUpper().Equals("MINIVERSION") && ss_3[1].ToUpper().Equals("MODEL"))
                                                            {

                                                                writelog("FWUpdate_Line 1709"); 
                                                                string min = ss_2[0];
                                                                List<string> model = new List<string> { ss_3[0] };
                                                                installPath = Path.GetFullPath(ss_1[1]);
                                                                Trace.WriteLine($"installPath = {installPath}");
                                                                isShowInfo = true;
                                                                var fwupdate = Auto_FWUpdate_display(commandLineInput, cLI_FWU_RESPONSE, installPath, isShowInfo, true, null, model, min);

                                                                result.ExitCode = fwupdate.code;
                                                                result.serialize_Json_response = fwupdate.result;
                                                                ret = true;

                                                            }
                                                        }
                                                        else
                                                        {
                                                            writelog("FWUpdate_Line 1725");
                                                            somethingError = true;
                                                        }
                                                    }
                                                    else
                                                    {
                                                        writelog("FWUpdate_Line 1731");
                                                        somethingError = true;
                                                    }
                                                }
                                                else
                                                {
                                                    writelog("FWUpdate_Line 1737");
                                                    somethingError = true;
                                                }
                                            }

                                            //display servicetag  or  miniver or model
                                            else if (commandLineInput.Options.Count == 2)
                                            {
                                                writelog("FWUpdate_Line 1745");
                                                string[] ss_2 = commandLineInput.Options[1].Option_Value.Split(",");
                                                if (ss_2.Length == 2)
                                                {
                                                    writelog("FWUpdate_Line 1749");
                                                    if (!string.IsNullOrEmpty(ss_2[0]) && !string.IsNullOrEmpty(ss_2[1]))
                                                    {
                                                        writelog("FWUpdate_Line 1752");
                                                        //display   only servicetag
                                                        if (ss_1[1].ToUpper().Equals("FORCEWITHNOTICE") && commandLineInput.Options.Count == 2 && ss_2[1].ToUpper().Equals("SERVICETAG"))
                                                        {
                                                            writelog("FWUpdate_Line 1756");
                                                            List<string> stag = new List<string> { ss_2[0] };
                                                            isShowInfo = true;
                                                            var fwupdate = Auto_FWUpdate_display(commandLineInput, cLI_FWU_RESPONSE, installPath, isShowInfo, true, stag, null, null);

                                                            result.ExitCode = fwupdate.code;
                                                            result.serialize_Json_response = fwupdate.result;
                                                            ret = true;
                                                        }
                                                        //display   only servicetag
                                                        else if (ss_1[1].ToUpper().Equals("FORCEWITHNONOTICE") && commandLineInput.Options.Count == 2 && ss_2[1].ToUpper().Equals("SERVICETAG"))
                                                        {
                                                            writelog("FWUpdate_Line 1768");
                                                            List<string> stag = new List<string> { ss_2[0] };
                                                            isShowInfo = false;
                                                            var fwupdate = Auto_FWUpdate_display(commandLineInput, cLI_FWU_RESPONSE, installPath, isShowInfo, true, stag, null, null);
                                                            FWResultReceived_List += Download_Event_2;
                                                            result.ExitCode = fwupdate.code;
                                                            result.serialize_Json_response = fwupdate.result;
                                                            ret = true;
                                                        }
                                                        //display   only servicetag
                                                        else if (ss_1[1].ToUpper().Contains(":\\") && commandLineInput.Options.Count == 2 && ss_2[1].ToUpper().Equals("SERVICETAG"))
                                                        {
                                                            writelog("FWUpdate_Line 1780");
                                                            List<string> stag = new List<string> { ss_2[0] };
                                                            installPath = Path.GetFullPath(ss_1[1]);
                                                            Trace.WriteLine($"installPath = {installPath}");
                                                            isShowInfo = true;
                                                            var fwupdate = Auto_FWUpdate_display(commandLineInput, cLI_FWU_RESPONSE, installPath, isShowInfo, true, stag, null, null);

                                                            result.ExitCode = fwupdate.code;
                                                            result.serialize_Json_response = fwupdate.result;
                                                            ret = true;

                                                        }
                                                        //display  only miniver
                                                        else if (ss_1[1].ToUpper().Equals("FORCEWITHNOTICE") && commandLineInput.Options.Count == 2 && ss_2[1].ToUpper().Equals("MINIVERSION"))
                                                        {
                                                            writelog("FWUpdate_Line 1795");
                                                            string min = ss_2[0];
                                                            isShowInfo = true;
                                                            var fwupdate = Auto_FWUpdate_display(commandLineInput, cLI_FWU_RESPONSE, installPath, isShowInfo, true, null, null, min);

                                                            result.ExitCode = fwupdate.code;
                                                            result.serialize_Json_response = fwupdate.result;
                                                            ret = true;
                                                        }
                                                        //display  only miniver
                                                        else if (ss_1[1].ToUpper().Equals("FORCEWITHNONOTICE") && commandLineInput.Options.Count == 2 && ss_2[1].ToUpper().Equals("MINIVERSION"))
                                                        {
                                                            writelog("FWUpdate_Line 1807");
                                                            string min = ss_2[0];
                                                            isShowInfo = false;
                                                            var fwupdate = Auto_FWUpdate_display(commandLineInput, cLI_FWU_RESPONSE, installPath, isShowInfo, true, null, null, min);
                                                            FWResultReceived_List += Download_Event_2;
                                                            result.ExitCode = fwupdate.code;
                                                            result.serialize_Json_response = fwupdate.result;
                                                            ret = true;
                                                        }
                                                        //display  only miniver
                                                        else if (ss_1[1].ToUpper().Contains(":\\") && commandLineInput.Options.Count == 2 && ss_2[1].ToUpper().Equals("MINIVERSION"))
                                                        {
                                                            writelog("FWUpdate_Line 1819");
                                                            string min = ss_2[0];
                                                            installPath = Path.GetFullPath(ss_1[1]);
                                                            Trace.WriteLine($"installPath = {installPath}");
                                                            isShowInfo = true;
                                                            var fwupdate = Auto_FWUpdate_display(commandLineInput, cLI_FWU_RESPONSE, installPath, isShowInfo, true, null, null, min);

                                                            result.ExitCode = fwupdate.code;
                                                            result.serialize_Json_response = fwupdate.result;
                                                            ret = true;

                                                        }
                                                        //display  only model
                                                        else if (ss_1[1].ToUpper().Equals("FORCEWITHNOTICE") && commandLineInput.Options.Count == 2 && ss_2[1].ToUpper().Equals("MODEL"))
                                                        {

                                                            writelog("FWUpdate_Line 1835");
                                                            List<string> model = new List<string> { ss_2[0] };
                                                            isShowInfo = true;
                                                            var fwupdate = Auto_FWUpdate_display(commandLineInput, cLI_FWU_RESPONSE, installPath, isShowInfo, true, null, model, null);

                                                            result.ExitCode = fwupdate.code;
                                                            result.serialize_Json_response = fwupdate.result;
                                                            ret = true;
                                                        }
                                                        //display  only model
                                                        else if (ss_1[1].ToUpper().Equals("FORCEWITHNONOTICE") && commandLineInput.Options.Count == 2 && ss_2[1].ToUpper().Equals("MODEL"))
                                                        {
                                                            writelog("FWUpdate_Line 1847");
                                                            List<string> model = new List<string> { ss_2[0] };
                                                            isShowInfo = false;
                                                            var fwupdate = Auto_FWUpdate_display(commandLineInput, cLI_FWU_RESPONSE, installPath, isShowInfo, true, null, model, null);
                                                            FWResultReceived_List += Download_Event_2;
                                                            result.ExitCode = fwupdate.code;
                                                            result.serialize_Json_response = fwupdate.result;
                                                            ret = true;
                                                        }
                                                        //display  only model
                                                        else if (ss_1[1].ToUpper().Contains(":\\") && commandLineInput.Options.Count == 2 && ss_2[1].ToUpper().Equals("MODEL"))
                                                        {
                                                            writelog("FWUpdate_Line 1859");
                                                            List<string> model = new List<string> { ss_2[0] };
                                                            installPath = Path.GetFullPath(ss_1[1]);
                                                            Trace.WriteLine($"installPath = {installPath}");
                                                            isShowInfo = true;
                                                            var fwupdate = Auto_FWUpdate_display(commandLineInput, cLI_FWU_RESPONSE, installPath, isShowInfo, true, null, model, null);

                                                            result.ExitCode = fwupdate.code;
                                                            result.serialize_Json_response = fwupdate.result;
                                                            ret = true;

                                                        }
                                                    }
                                                    else
                                                    {
                                                        writelog("FWUpdate_Line 1874");
                                                        somethingError = true;
                                                    }
                                                }
                                                else
                                                {
                                                    writelog("FWUpdate_Line 1880");
                                                    somethingError = true;
                                                }

                                            }

                                            //display  no servicetag miniver model
                                            else if (ss_1[1].ToUpper().Equals("FORCEWITHNOTICE") && commandLineInput.Options.Count == 1)
                                            {

                                                writelog("FWUpdate_Line 1890");
                                                isShowInfo = true;
                                                var fwupdate = Auto_FWUpdate_display(commandLineInput, cLI_FWU_RESPONSE, installPath, isShowInfo, true, null, null, null);

                                                result.ExitCode = fwupdate.code;
                                                result.serialize_Json_response = fwupdate.result;
                                                ret = true;
                                            }
                                            //display  no servicetag miniver model
                                            else if (ss_1[1].ToUpper().Equals("FORCEWITHNONOTICE") && commandLineInput.Options.Count == 1)
                                            {

                                                writelog("FWUpdate_Line 1902");
                                                isShowInfo = false;
                                                var fwupdate = Auto_FWUpdate_display(commandLineInput, cLI_FWU_RESPONSE, installPath, isShowInfo, true, null, null, null);
                                                FWResultReceived_List += Download_Event_2;
                                                result.ExitCode = fwupdate.code;
                                                result.serialize_Json_response = fwupdate.result;
                                                ret = true;
                                            }
                                            //display  no servicetag miniver model
                                            else if (ss_1[1].ToUpper().Contains(":\\") && commandLineInput.Options.Count == 1)
                                            {
                                                writelog("FWUpdate_Line 1913");
                                                installPath = Path.GetFullPath(ss_1[1]);
                                                Trace.WriteLine($"installPath = {installPath}");
                                                isShowInfo = true;
                                                var fwupdate = Auto_FWUpdate_display(commandLineInput, cLI_FWU_RESPONSE, installPath, isShowInfo, true, null, null, null);

                                                result.ExitCode = fwupdate.code;
                                                result.serialize_Json_response = fwupdate.result;
                                                ret = true;

                                            }
                                            else if (ss_1[1].ToUpper().Equals("DEFER") && commandLineInput.Options.Count == 1)
                                            {
                                                writelog("FWUpdate_Line 1926");
                                                if (commandLineInput.Options.Count > 2)
                                                {
                                                    writelog("FWUpdate_Line 1929");
                                                    cLI_FWU_RESPONSE.Result = "FAIL";
                                                    cLI_FWU_RESPONSE.Message = "Bring in extra strings:";
                                                    for (int i = 0; i < commandLineInput.Options.Count; i++)
                                                    {
                                                        writelog("FWUpdate_Line 1934");
                                                        cLI_FWU_RESPONSE.Message += $"{commandLineInput.Options[i].Option_Name}={commandLineInput.Options[i].Option_Value}\n";
                                                    }
                                                    break;
                                                }
                                                ret = GetFWUpdateList(commandLineInput, cLI_FWU_RESPONSE, isShowInfo, true);
                                            }
                                            else
                                            {
                                                writelog("FWUpdate_Line 1943");
                                                somethingError = true;
                                            }
                                        }
                                    }
                                    else if (!ss_1[0].ToUpper().Equals("DISPLAY"))
                                    {

                                        writelog("FWUpdate_Line 1951");
                                        _deviceinfo = _devMgr.GetDevices().Result.deviceInfo;
                                        foreach (var g in _deviceinfo)
                                        {
                                            writelog("FWUpdate_Line 1955");
                                            if (g.LogicalDeviceType == "LogicalMouse" && ss_1[0].ToUpper().Equals("MOUSE"))
                                            {
                                                writelog("FWUpdate_Line 1958");
                                                _recode = true;
                                            }
                                            if (g.LogicalDeviceType == "LogicalKeyboard" && ss_1[0].ToUpper().Equals("KEYBOARD"))
                                            {
                                                writelog("FWUpdate_Line 1963");
                                                _recode = true;
                                            }
                                            if (g.LogicalDeviceType == "LogicalHeadset" && ss_1[0].ToUpper().Equals("HEADSET"))
                                            {
                                                writelog("FWUpdate_Line 1963_1");
                                                _recode = true;
                                            }
                                            if (g.LogicalDeviceType == "LogicalWebcam" && ss_1[0].ToUpper().Equals("WEBCAM"))
                                            {
                                                writelog("FWUpdate_Line 1963_2");
                                                _recode = true;
                                            }
                                            if (g.LogicalDeviceType == "LogicalWiredAudio" && ss_1[0].ToUpper().Equals("SPEAKER"))
                                            {
                                                writelog("FWUpdate_Line 1963_2");
                                                _recode = true;
                                            }
                                            if (g.LogicalDeviceType == "LogicalPen" && ss_1[0].ToUpper().Equals("PEN"))
                                            {
                                                writelog("FWUpdate_Line 1963_2");
                                                _recode = true;
                                            }
                                            if (ss_1[0].ToUpper().Equals("DONGLE"))
                                            {
                                                writelog("FWUpdate_Line 1963_2");
                                                _recode = true;
                                            }
                                        }

                                        if (_recode)
                                        {
                                            writelog("FWUpdate_Line 1970");
                                            if (commandLineInput.Options.Count == 4)
                                            {
                                                writelog("FWUpdate_Line 1973");
                                                string[] ss_2 = commandLineInput.Options[1].Option_Value.Split(",");
                                                string[] ss_3 = commandLineInput.Options[2].Option_Value.Split(",");
                                                string[] ss_4 = commandLineInput.Options[3].Option_Value.Split(",");
                                                if (ss_2.Length == 2 && ss_3.Length == 2 && ss_4.Length == 2)
                                                {
                                                    writelog("FWUpdate_Line 1979");
                                                    if (!string.IsNullOrEmpty(ss_2[0]) && !string.IsNullOrEmpty(ss_2[1]))
                                                    {
                                                        writelog("FWUpdate_Line 1982");
                                                        if (!string.IsNullOrEmpty(ss_3[0]) && !string.IsNullOrEmpty(ss_3[1]))
                                                        {
                                                            writelog("FWUpdate_Line 1985");
                                                            if (!string.IsNullOrEmpty(ss_4[0]) && !string.IsNullOrEmpty(ss_4[1]))
                                                            {
                                                                writelog("FWUpdate_Line 1988");
                                                                //peripherals guid & miniver & model
                                                                if (ss_1[1].ToUpper().Equals("FORCEWITHNOTICE") && ss_2[1].ToUpper().Equals("GUID") && ss_3[1].ToUpper().Equals("MINIVERSION") && ss_4[1].ToUpper().Equals("MODEL"))
                                                                {
                                                                    writelog("FWUpdate_Line 1992");
                                                                    List<string> guid = new List<string>();
                                                                    string min = ss_3[0];
                                                                    List<string> model = new List<string> { ss_4[0] };

                                                                    foreach (var g in _deviceinfo)
                                                                    {
                                                                        writelog("FWUpdate_Line 1999");
                                                                        if (g.ID.ToString().ToUpper() == ss_2[0])
                                                                        {
                                                                            writelog("FWUpdate_Line 2002");
                                                                            Trace.WriteLine($"IN ============");
                                                                            guid = new List<string>() { commandLineInput.Options[1].Option_Value };
                                                                        }
                                                                    }

                                                                    isShowInfo = true;
                                                                    var fwupdate = Auto_FWUpdate2(commandLineInput, cLI_FWU_RESPONSE, false, installPath, isShowInfo, true, guid, model, min);

                                                                    result.ExitCode = fwupdate.code;
                                                                    result.serialize_Json_response = fwupdate.result;
                                                                    ret = true;
                                                                }
                                                                //peripherals guid & miniver& model
                                                                else if (ss_1[1].ToUpper().Equals("FORCEWITHNONOTICE") && ss_2[1].ToUpper().Equals("GUID") && ss_3[1].ToUpper().Equals("MINIVERSION") && ss_4[1].ToUpper().Equals("MODEL"))
                                                                {
                                                                    writelog("FWUpdate_Line 2018");
                                                                    List<string> guid = new List<string>();
                                                                    string min = ss_3[0];
                                                                    List<string> model = new List<string> { ss_4[0] };
                                                                    foreach (var g in _deviceinfo)
                                                                    {
                                                                        writelog("FWUpdate_Line 2024");
                                                                        if (g.ID.ToString().ToUpper() == ss_2[0])
                                                                        {
                                                                            writelog("FWUpdate_Line 2027");
                                                                            Trace.WriteLine($"IN ============");
                                                                            guid = new List<string>() { commandLineInput.Options[1].Option_Value };
                                                                        }
                                                                    }

                                                                    isShowInfo = true;
                                                                    var fwupdate = Auto_FWUpdate2(commandLineInput, cLI_FWU_RESPONSE, false, installPath, isShowInfo, true, guid, model, min);
                                                                    FWResultReceived_List += Download_Event_2;
                                                                    result.ExitCode = fwupdate.code;
                                                                    result.serialize_Json_response = fwupdate.result;
                                                                    ret = true;
                                                                }
                                                                //peripherals guid & miniver& model
                                                                else if (ss_1[1].ToUpper().Contains(":\\") && ss_2[1].ToUpper().Equals("GUID") && ss_2[1].ToUpper().Equals("MINIVERSION") && ss_4[1].ToUpper().Equals("MODEL"))
                                                                {
                                                                    writelog("FWUpdate_Line 2043");
                                                                    List<string> guid = new List<string>();
                                                                    string min = ss_3[0];
                                                                    List<string> model = new List<string> { ss_4[0] };
                                                                    foreach (var g in _deviceinfo)
                                                                    {
                                                                        writelog("FWUpdate_Line 2049");
                                                                        if (g.ID.ToString().ToUpper() == ss_2[0])
                                                                        {
                                                                            writelog("FWUpdate_Line 2052");
                                                                            Trace.WriteLine($"IN ============");
                                                                            guid = new List<string>() { commandLineInput.Options[1].Option_Value };
                                                                        }
                                                                    }
                                                                    installPath = Path.GetFullPath(ss_1[1]);
                                                                    Trace.WriteLine($"installPath = {installPath}");
                                                                    isShowInfo = true;
                                                                    var fwupdate = Auto_FWUpdate2(commandLineInput, cLI_FWU_RESPONSE, false, installPath, isShowInfo, true, guid, model, min);

                                                                    result.ExitCode = fwupdate.code;
                                                                    result.serialize_Json_response = fwupdate.result;
                                                                    ret = true;

                                                                }
                                                            }
                                                            writelog("FWUpdate_Line !string.IsNullOrEmpty(ss_4[0]) && !string.IsNullOrEmpty(ss_4[1])");
                                                        }
                                                        writelog("FWUpdate_Line !string.IsNullOrEmpty(ss_3[0]) && !string.IsNullOrEmpty(ss_3[1])");
                                                    }
                                                    writelog("FWUpdate_Line !string.IsNullOrEmpty(ss_2[0]) && !string.IsNullOrEmpty(ss_2[1])");
                                                }
                                                writelog("ss_2.Length == 2 && ss_3.Length == 2 && ss_4.Length == 2");
                                            }
                                            //peripherals (guid  && miniver) or (guid  && model) or (miniver  && model)
                                            if (commandLineInput.Options.Count == 3)
                                            {
                                                writelog("FWUpdate_Line 2076");
                                                string[] ss_2 = commandLineInput.Options[1].Option_Value.Split(",");
                                                string[] ss_3 = commandLineInput.Options[2].Option_Value.Split(",");

                                                if (ss_2.Length == 2 && ss_3.Length == 2)
                                                {
                                                    writelog("FWUpdate_Line 2082");
                                                    if (!string.IsNullOrEmpty(ss_2[0]) && !string.IsNullOrEmpty(ss_2[1]))
                                                    {
                                                        writelog("FWUpdate_Line 2085");
                                                        if (!string.IsNullOrEmpty(ss_3[0]) && !string.IsNullOrEmpty(ss_3[1]))
                                                        {
                                                            writelog("FWUpdate_Line 2088");
                                                            //peripherals guid  && miniver
                                                            if (ss_1[1].ToUpper().Equals("FORCEWITHNOTICE") && ss_2[1].ToUpper().Equals("GUID") && ss_3[1].ToUpper().Equals("MINIVERSION"))
                                                            {

                                                                writelog("FWUpdate_Line 2093");
                                                                List<string> guid = new List<string>();
                                                                string min = ss_3[0];
                                                                foreach (var g in _deviceinfo)
                                                                {
                                                                    writelog("FWUpdate_Line 2098");
                                                                    if (g.ID.ToString().ToUpper() == ss_2[0])
                                                                    {
                                                                        writelog("FWUpdate_Line 2101");
                                                                        Trace.WriteLine($"IN ============");
                                                                        guid = new List<string>() { commandLineInput.Options[1].Option_Value };
                                                                    }
                                                                }

                                                                isShowInfo = true;
                                                                var fwupdate = Auto_FWUpdate2(commandLineInput, cLI_FWU_RESPONSE, false, installPath, isShowInfo, true, guid, null, min);

                                                                result.ExitCode = fwupdate.code;
                                                                result.serialize_Json_response = fwupdate.result;
                                                                ret = true;
                                                            }
                                                            //peripherals guid  && miniver
                                                            else if (ss_1[1].ToUpper().Equals("FORCEWITHNONOTICE") && ss_2[1].ToUpper().Equals("GUID") && ss_3[1].ToUpper().Equals("MINIVERSION"))
                                                            {
                                                                writelog("FWUpdate_Line 2117");
                                                                List<string> guid = new List<string>();
                                                                string min = ss_3[0];
                                                                foreach (var g in _deviceinfo)
                                                                {
                                                                    writelog("FWUpdate_Line 2122");
                                                                    if (g.ID.ToString().ToUpper() == ss_2[0])
                                                                    {
                                                                        writelog("FWUpdate_Line 2125");
                                                                        Trace.WriteLine($"IN ============");
                                                                        guid = new List<string>() { commandLineInput.Options[1].Option_Value };
                                                                    }
                                                                }

                                                                isShowInfo = true;
                                                                var fwupdate = Auto_FWUpdate2(commandLineInput, cLI_FWU_RESPONSE, false, installPath, isShowInfo, true, guid, null, min);
                                                                FWResultReceived_List += Download_Event_2;
                                                                result.ExitCode = fwupdate.code;
                                                                result.serialize_Json_response = fwupdate.result;
                                                                ret = true;
                                                            }
                                                            //peripherals guid  && miniver
                                                            else if (ss_1[1].ToUpper().Contains(":\\") && ss_2[1].ToUpper().Equals("GUID") && ss_3[1].ToUpper().Equals("MINIVERSION"))
                                                            {
                                                                writelog("FWUpdate_Line 2141");
                                                                List<string> guid = new List<string>();
                                                                string min = ss_3[0];
                                                                foreach (var g in _deviceinfo)
                                                                {
                                                                    writelog("FWUpdate_Line 2146");
                                                                    if (g.ID.ToString().ToUpper() == ss_2[0])
                                                                    {
                                                                        writelog("FWUpdate_Line 2149");
                                                                        Trace.WriteLine($"IN ============");
                                                                        guid = new List<string>() { commandLineInput.Options[1].Option_Value };
                                                                    }
                                                                }
                                                                installPath = Path.GetFullPath(ss_1[1]);
                                                                Trace.WriteLine($"installPath = {installPath}");
                                                                isShowInfo = true;
                                                                var fwupdate = Auto_FWUpdate2(commandLineInput, cLI_FWU_RESPONSE, false, installPath, isShowInfo, true, guid, null, min);

                                                                result.ExitCode = fwupdate.code;
                                                                result.serialize_Json_response = fwupdate.result;
                                                                ret = true;

                                                            }
                                                            //peripherals guid  && model
                                                            else if (ss_1[1].ToUpper().Equals("FORCEWITHNOTICE") && ss_2[1].ToUpper().Equals("GUID") && ss_3[1].ToUpper().Equals("MODEL"))
                                                            {

                                                                writelog("FWUpdate_Line 2168");
                                                                List<string> model = new List<string> { ss_3[0] };
                                                                List<string> guid = new List<string>();

                                                                foreach (var g in _deviceinfo)
                                                                {
                                                                    writelog("FWUpdate_Line 2174");
                                                                    if (g.ID.ToString().ToUpper() == ss_2[0])
                                                                    {
                                                                        writelog("FWUpdate_Line 2177");
                                                                        Trace.WriteLine($"IN ============");
                                                                        guid = new List<string>() { commandLineInput.Options[1].Option_Value };
                                                                    }
                                                                }
                                                                isShowInfo = true;
                                                                var fwupdate = Auto_FWUpdate2(commandLineInput, cLI_FWU_RESPONSE, false, installPath, isShowInfo, true, guid, model, null);

                                                                result.ExitCode = fwupdate.code;
                                                                result.serialize_Json_response = fwupdate.result;
                                                                ret = true;
                                                            }
                                                            //peripherals guid  && model
                                                            else if (ss_1[1].ToUpper().Equals("FORCEWITHNONOTICE") && ss_2[1].ToUpper().Equals("GUID") && ss_3[1].ToUpper().Equals("MODEL"))
                                                            {

                                                                writelog("FWUpdate_Line 2193");
                                                                List<string> model = new List<string> { ss_3[0] };
                                                                List<string> guid = new List<string>();

                                                                foreach (var g in _deviceinfo)
                                                                {
                                                                    writelog("FWUpdate_Line 2199");
                                                                    if (g.ID.ToString().ToUpper() == ss_2[0])
                                                                    {
                                                                        writelog("FWUpdate_Line 2202");
                                                                        Trace.WriteLine($"IN ============");
                                                                        guid = new List<string>() { commandLineInput.Options[1].Option_Value };
                                                                    }
                                                                }
                                                                isShowInfo = true;
                                                                var fwupdate = Auto_FWUpdate2(commandLineInput, cLI_FWU_RESPONSE, false, installPath, isShowInfo, true, guid, model, null);
                                                                FWResultReceived_List += Download_Event_2;
                                                                result.ExitCode = fwupdate.code;
                                                                result.serialize_Json_response = fwupdate.result;
                                                                ret = true;
                                                            }
                                                            //peripherals guid  && model
                                                            else if (ss_1[1].ToUpper().Contains(":\\") && ss_2[1].ToUpper().Equals("GUID") && ss_3[1].ToUpper().Equals("MODEL"))


                                                            {
                                                                writelog("FWUpdate_Line 2219");
                                                                List<string> model = new List<string> { ss_3[0] };
                                                                List<string> guid = new List<string>();

                                                                foreach (var g in _deviceinfo)
                                                                {
                                                                    writelog("FWUpdate_Line 2225");
                                                                    if (g.ID.ToString().ToUpper() == ss_2[0])
                                                                    {
                                                                        writelog("FWUpdate_Line 2228");
                                                                        Trace.WriteLine($"IN ============");
                                                                        guid = new List<string>() { commandLineInput.Options[1].Option_Value };
                                                                    }
                                                                }
                                                                installPath = Path.GetFullPath(ss_1[1]);
                                                                Trace.WriteLine($"installPath = {installPath}");
                                                                isShowInfo = true;
                                                                var fwupdate = Auto_FWUpdate2(commandLineInput, cLI_FWU_RESPONSE, false, installPath, isShowInfo, true, guid, model, null);

                                                                result.ExitCode = fwupdate.code;
                                                                result.serialize_Json_response = fwupdate.result;
                                                                ret = true;

                                                            }
                                                            //peripherals miniver  && model
                                                            else if (ss_1[1].ToUpper().Equals("FORCEWITHNOTICE") && ss_2[1].ToUpper().Equals("MINIVERSION") && ss_3[1].ToUpper().Equals("MODEL"))
                                                            {

                                                                writelog("FWUpdate_Line 2247");
                                                                string min = ss_2[0];
                                                                List<string> model = new List<string> { ss_3[0] };

                                                                isShowInfo = true;
                                                                var fwupdate = Auto_FWUpdate2(commandLineInput, cLI_FWU_RESPONSE, false, installPath, isShowInfo, true, null, model, min);

                                                                result.ExitCode = fwupdate.code;
                                                                result.serialize_Json_response = fwupdate.result;
                                                                ret = true;
                                                            }
                                                            //peripherals miniver  && model
                                                            else if (ss_1[1].ToUpper().Equals("FORCEWITHNONOTICE") && ss_2[1].ToUpper().Equals("MINIVERSION") && ss_3[1].ToUpper().Equals("MODEL"))
                                                            {

                                                                writelog("FWUpdate_Line 2262");
                                                                string min = ss_2[0];
                                                                List<string> model = new List<string> { ss_3[0] };

                                                                isShowInfo = true;
                                                                var fwupdate = Auto_FWUpdate2(commandLineInput, cLI_FWU_RESPONSE, false, installPath, isShowInfo, true, null, model, min);
                                                                FWResultReceived_List += Download_Event_2;
                                                                result.ExitCode = fwupdate.code;
                                                                result.serialize_Json_response = fwupdate.result;
                                                                ret = true;
                                                            }
                                                            //peripherals miniver  && model
                                                            else if (ss_1[1].ToUpper().Contains(":\\") && ss_2[1].ToUpper().Equals("MINIVERSION") && ss_3[1].ToUpper().Equals("MODEL"))


                                                            {
                                                                writelog("FWUpdate_Line 2278");
                                                                string min = ss_2[0];
                                                                List<string> model = new List<string> { ss_3[0] };
                                                                installPath = Path.GetFullPath(ss_1[1]);
                                                                Trace.WriteLine($"installPath = {installPath}");
                                                                isShowInfo = true;
                                                                var fwupdate = Auto_FWUpdate2(commandLineInput, cLI_FWU_RESPONSE, false, installPath, isShowInfo, true, null, model, min);

                                                                result.ExitCode = fwupdate.code;
                                                                result.serialize_Json_response = fwupdate.result;
                                                                ret = true;

                                                            }
                                                        }
                                                        writelog("FWUpdate_Line !string.IsNullOrEmpty(ss_3[0]) && !string.IsNullOrEmpty(ss_3[1])");
                                                    }
                                                    writelog("FWUpdate_Line !string.IsNullOrEmpty(ss_2[0]) && !string.IsNullOrEmpty(ss_2[1])");
                                                }
                                                writelog("FWUpdate_Line ss_2.Length == 2 && ss_3.Length == 2");
                                            }

                                            if (commandLineInput.Options.Count == 2)
                                            {
                                                writelog("FWUpdate_Line 2298");
                                                string[] ss_2 = commandLineInput.Options[1].Option_Value.Split(",");
                                                if (ss_2.Length == 2)
                                                {
                                                    writelog("FWUpdate_Line 2302");
                                                    if (!string.IsNullOrEmpty(ss_2[0]) && !string.IsNullOrEmpty(ss_2[1]))
                                                    {

                                                        writelog("FWUpdate_Line 2306");

                                                        //peripherals only guid
                                                        if (ss_1[1].ToUpper().Equals("FORCEWITHNOTICE") && commandLineInput.Options.Count == 2 && ss_2[1].ToUpper().Equals("GUID"))
                                                        {

                                                            writelog("FWUpdate_Line 2312");
                                                            List<string> guid = new List<string>();
                                                            foreach (var g in _deviceinfo)
                                                            {
                                                                writelog("FWUpdate_Line 2316");
                                                                if (g.ID.ToString().ToUpper() == ss_2[0])
                                                                {
                                                                    writelog("FWUpdate_Line 2319");
                                                                    Trace.WriteLine($"IN ============");
                                                                    guid = new List<string>() { commandLineInput.Options[1].Option_Value };
                                                                }
                                                            }

                                                            isShowInfo = true;
                                                            var fwupdate = Auto_FWUpdate2(commandLineInput, cLI_FWU_RESPONSE, false, installPath, isShowInfo, true, guid, null);

                                                            result.ExitCode = fwupdate.code;
                                                            result.serialize_Json_response = fwupdate.result;
                                                            ret = true;
                                                        }
                                                        //peripherals only guid
                                                        else if (ss_1[1].ToUpper().Equals("FORCEWITHNONOTICE") && commandLineInput.Options.Count == 2 && ss_2[1].ToUpper().Equals("GUID"))
                                                        {
                                                            writelog("FWUpdate_Line 2335");
                                                            List<string> guid = new List<string>();
                                                            foreach (var g in _deviceinfo)
                                                            {
                                                                writelog("FWUpdate_Line 2339");
                                                                if (g.ID.ToString().ToUpper() == ss_2[0])
                                                                {
                                                                    writelog("FWUpdate_Line 2342");
                                                                    Trace.WriteLine($"IN ============");
                                                                    guid = new List<string>() { commandLineInput.Options[1].Option_Value };
                                                                }
                                                            }

                                                            isShowInfo = true;
                                                            var fwupdate = Auto_FWUpdate2(commandLineInput, cLI_FWU_RESPONSE, false, installPath, isShowInfo, true, guid, null);
                                                            FWResultReceived_List += Download_Event_2;
                                                            result.ExitCode = fwupdate.code;
                                                            result.serialize_Json_response = fwupdate.result;
                                                            ret = true;
                                                        }
                                                        //peripherals only guid
                                                        else if (ss_1[1].ToUpper().Contains(":\\") && commandLineInput.Options.Count == 2 && ss_2[1].ToUpper().Equals("GUID"))
                                                        {
                                                            writelog("FWUpdate_Line 2358");
                                                            List<string> guid = new List<string>();
                                                            foreach (var g in _deviceinfo)
                                                            {
                                                                writelog("FWUpdate_Line 2362");
                                                                if (g.ID.ToString().ToUpper() == ss_2[0])
                                                                {
                                                                    writelog("FWUpdate_Line 2365");
                                                                    Trace.WriteLine($"IN ============");
                                                                    guid = new List<string>() { commandLineInput.Options[1].Option_Value };
                                                                }
                                                            }
                                                            installPath = Path.GetFullPath(ss_1[1]);
                                                            Trace.WriteLine($"installPath = {installPath}");
                                                            isShowInfo = true;
                                                            var fwupdate = Auto_FWUpdate2(commandLineInput, cLI_FWU_RESPONSE, false, installPath, isShowInfo, true, guid, null);

                                                            result.ExitCode = fwupdate.code;
                                                            result.serialize_Json_response = fwupdate.result;
                                                            ret = true;

                                                        }
                                                        //peripherals only minversion
                                                        else if (ss_1[1].ToUpper().Equals("FORCEWITHNOTICE") && commandLineInput.Options.Count == 2 && ss_2[1].ToUpper().Equals("MINIVERSION"))
                                                        {
                                                            writelog("FWUpdate_Line 2383");
                                                            string min = ss_2[0];

                                                            isShowInfo = true;
                                                            var fwupdate = Auto_FWUpdate2(commandLineInput, cLI_FWU_RESPONSE, false, installPath, isShowInfo, true, null, null, min);

                                                            result.ExitCode = fwupdate.code;
                                                            result.serialize_Json_response = fwupdate.result;
                                                            ret = true;
                                                        }
                                                        //peripherals only minversion
                                                        else if (ss_1[1].ToUpper().Equals("FORCEWITHNONOTICE") && commandLineInput.Options.Count == 2 && ss_2[1].ToUpper().Equals("MINIVERSION"))
                                                        {
                                                            writelog("FWUpdate_Line 2396");
                                                            string min = ss_2[0];

                                                            isShowInfo = true;
                                                            var fwupdate = Auto_FWUpdate2(commandLineInput, cLI_FWU_RESPONSE, false, installPath, isShowInfo, true, null, null, min);
                                                            FWResultReceived_List += Download_Event_2;
                                                            result.ExitCode = fwupdate.code;
                                                            result.serialize_Json_response = fwupdate.result;
                                                            ret = true;
                                                        }
                                                        //peripherals only minversion
                                                        else if (ss_1[1].ToUpper().Contains(":\\") && commandLineInput.Options.Count == 2 && ss_2[1].ToUpper().Equals("MINIVERSION"))
                                                        {
                                                            writelog("FWUpdate_Line 2409");
                                                            string min = ss_2[0];
                                                            installPath = Path.GetFullPath(ss_1[1]);
                                                            Trace.WriteLine($"installPath = {installPath}");
                                                            isShowInfo = true;
                                                            var fwupdate = Auto_FWUpdate2(commandLineInput, cLI_FWU_RESPONSE, false, installPath, isShowInfo, true, null, null, min);

                                                            result.ExitCode = fwupdate.code;
                                                            result.serialize_Json_response = fwupdate.result;
                                                            ret = true;

                                                        }
                                                        //peripherals only model
                                                        else if (ss_1[1].ToUpper().Equals("FORCEWITHNOTICE") && commandLineInput.Options.Count == 2 && ss_2[1].ToUpper().Equals("MODEL"))
                                                        {
                                                            writelog("FWUpdate_Line 2424");
                                                            List<string> model = new List<string> { ss_2[0] };

                                                            isShowInfo = true;
                                                            var fwupdate = Auto_FWUpdate2(commandLineInput, cLI_FWU_RESPONSE, false, installPath, isShowInfo, true, null, model, null);

                                                            result.ExitCode = fwupdate.code;
                                                            result.serialize_Json_response = fwupdate.result;
                                                            ret = true;
                                                        }
                                                        //peripherals only model
                                                        else if (ss_1[1].ToUpper().Equals("FORCEWITHNONOTICE") && commandLineInput.Options.Count == 2 && ss_2[1].ToUpper().Equals("MODEL"))
                                                        {
                                                            writelog("FWUpdate_Line 2437");
                                                            List<string> model = new List<string> { ss_2[0] };

                                                            isShowInfo = true;
                                                            var fwupdate = Auto_FWUpdate2(commandLineInput, cLI_FWU_RESPONSE, false, installPath, isShowInfo, true, null, model, null);
                                                            FWResultReceived_List += Download_Event_2;
                                                            result.ExitCode = fwupdate.code;
                                                            result.serialize_Json_response = fwupdate.result;
                                                            ret = true;
                                                        }
                                                        //peripherals only model
                                                        else if (ss_1[1].ToUpper().Contains(":\\") && commandLineInput.Options.Count == 2 && ss_2[1].ToUpper().Equals("MODEL"))
                                                        {
                                                            writelog("FWUpdate_Line 2450");
                                                            List<string> model = new List<string> { ss_2[0] };
                                                            installPath = Path.GetFullPath(ss_1[1]);
                                                            Trace.WriteLine($"installPath = {installPath}");
                                                            isShowInfo = true;
                                                            var fwupdate = Auto_FWUpdate2(commandLineInput, cLI_FWU_RESPONSE, false, installPath, isShowInfo, true, null, model, null);

                                                            result.ExitCode = fwupdate.code;
                                                            result.serialize_Json_response = fwupdate.result;
                                                            ret = true;

                                                        }
                                                    }
                                                    writelog("FWUpdate_Line !string.IsNullOrEmpty(ss_2[0]) && !string.IsNullOrEmpty(ss_2[1])");
                                                }
                                                writelog("FWUpdate_Line  ss_2.Length == 2");
                                            }

                                            //peripherals no guid minversion model
                                            else if (ss_1[1].ToUpper().Equals("FORCEWITHNOTICE") && commandLineInput.Options.Count == 1)
                                            {
                                                writelog("FWUpdate_Line 2469");
                                                isShowInfo = true;
                                                var fwupdate = Auto_FWUpdate2(commandLineInput, cLI_FWU_RESPONSE, false, installPath, isShowInfo, true, null, null);

                                                result.ExitCode = fwupdate.code;
                                                result.serialize_Json_response = fwupdate.result;
                                                ret = true;
                                            }
                                            //peripherals no guid minversion model
                                            else if (ss_1[1].ToUpper().Equals("FORCEWITHNONOTICE") && commandLineInput.Options.Count == 1)
                                            {
                                                writelog("FWUpdate_Line 2480");
                                                isShowInfo = false;
                                                var fwupdate = Auto_FWUpdate2(commandLineInput, cLI_FWU_RESPONSE, false, installPath, isShowInfo, true, null, null);
                                                FWResultReceived_List += Download_Event_2;
                                                result.ExitCode = fwupdate.code;
                                                result.serialize_Json_response = fwupdate.result;
                                                ret = true;
                                            }
                                            //peripherals no guid minversion model
                                            else if (ss_1[1].ToUpper().Contains(":\\") && commandLineInput.Options.Count == 1)
                                            {
                                                writelog("FWUpdate_Line 2491");
                                                installPath = Path.GetFullPath(ss_1[1]);
                                                Trace.WriteLine($"installPath = {installPath}");
                                                isShowInfo = true;
                                                var fwupdate = Auto_FWUpdate2(commandLineInput, cLI_FWU_RESPONSE, false, installPath, isShowInfo, true);

                                                result.ExitCode = fwupdate.code;
                                                result.serialize_Json_response = fwupdate.result;
                                                ret = true;

                                            }
                                            //peripherals no guid minversion
                                            else if (ss_1[1].ToUpper().Equals("DEFER") && commandLineInput.Options.Count == 1)
                                            {
                                                writelog("FWUpdate_Line 2505");
                                                if (commandLineInput.Options.Count > 2)
                                                {
                                                    writelog("FWUpdate_Line 2508");
                                                    cLI_FWU_RESPONSE.Result = "FAIL";
                                                    cLI_FWU_RESPONSE.Message = "Bring in extra strings:";
                                                    for (int i = 0; i < commandLineInput.Options.Count; i++)
                                                    {
                                                        writelog("FWUpdate_Line 2513");
                                                        cLI_FWU_RESPONSE.Message += $"{commandLineInput.Options[i].Option_Name}={commandLineInput.Options[i].Option_Value}\n";
                                                    }
                                                    break;
                                                }
                                                ret = GetFWUpdateList(commandLineInput, cLI_FWU_RESPONSE, isShowInfo, true);
                                            }
                                        }
                                        else
                                        {
                                            writelog("FWUpdate_Line 2523");
                                            CLI_RESPONSE rsp = new CLI_RESPONSE()
                                            {
                                                Command = commandLineInput.Command,
                                                TargetFeature = commandLineInput.TargetFeature,
                                                Result = "FAIL",
                                                Message = "No Device connected",
                                            };
                                            System.Console.WriteLine(JsonConvert.SerializeObject(rsp, Formatting.Indented));
                                            return ((int)CLI_ExitCode.fail_FWUpdate, JsonConvert.SerializeObject(rsp, Formatting.Indented));
                                        }

                                    }
                                    else
                                    {
                                        writelog("FWUpdate_Line 2538");
                                        somethingError = true;
                                    }
                                }
                            }
                            else
                            {
                                writelog("FWUpdate_Line 2545");
                                somethingError = true;
                            }
                            if (somethingError)
                            {
                                writelog("FWUpdate_Line 2550");
                                cLI_FWU_RESPONSE.Result = "FAIL";
                                cLI_FWU_RESPONSE.Message = "Bring in extra strings:";
                                for (int i = 0; i < commandLineInput.Options.Count; i++)
                                {
                                    writelog("FWUpdate_Line 2555");
                                    cLI_FWU_RESPONSE.Message += $"{commandLineInput.Options[i].Option_Name}={commandLineInput.Options[i].Option_Value}\n";
                                }
                                    break;
                            }
                        }
                        else
                        {
                            writelog("FWUpdate_Line 2563");
                            cLI_FWU_RESPONSE.Message = "Input FAIL";
                            ret = false;
                        }
                            writelog("FWUpdate_Line 2567");

                        cLI_FWU_RESPONSE.Result = ret == true ? "PASS" : "FAIL";
                        break;
                    case "SILENTFWUPDATE":

                        _recode = false;
                        _deviceinfo = _devMgr.GetDevices().Result.deviceInfo;
                        writelog("FWUpdate_Line 2575");

                        foreach (var g in _deviceinfo)
                        {
                            writelog("FWUpdate_Line 2579");

                            if (g.LogicalDeviceType == "LogicalDock" && commandLineInput.TargetType.ToUpper().Equals("DOCK"))
                            {
                            writelog("FWUpdate_Line 2583");

                                _recode = true;
                            }
                        }
                        if (_recode)
                        {
                            writelog("FWUpdate_Line 2590");
                            cLI_FWU_RESPONSE = new CLI_FWU_RESPONSE(cLI_RESPONSE);
                            if (commandLineInput.Options.Count > 5)
                            {
                                writelog("FWUpdate_Line 2594");
                                cLI_FWU_RESPONSE.Result = "FAIL";
                                cLI_FWU_RESPONSE.Message = "Bring in extra strings:";
                                for (int i = 0; i < commandLineInput.Options.Count; i++)
                                {
                                    writelog("FWUpdate_Line 2599");
                                    cLI_FWU_RESPONSE.Message += $"{commandLineInput.Options[i].Option_Name}={commandLineInput.Options[i].Option_Value}\n";
                                }
                                break;
                            }

                            if (commandLineInput.Options.Count == 4)
                            {
                                writelog("FWUpdate_Line 2607");
                                if (!string.IsNullOrEmpty(commandLineInput.Options[0].Option_Value)) 
                                {
                                    writelog("FWUpdate_Line 2610");
                                    cLI_FWU_RESPONSE.Value = commandLineInput.Options[0].Option_Value;

                                    //string[] ss_1 = commandLineInput.Options[0].Option_Value.Split(",");
                                    string[] ss_2 = commandLineInput.Options[1].Option_Value.Split(",");
                                    string[] ss_3 = commandLineInput.Options[2].Option_Value.Split(",");
                                    string[] ss_4 = commandLineInput.Options[3].Option_Value.Split(",");

                                    
                                    if (ss_2.Length == 2 && ss_3.Length == 2 && ss_4.Length == 2)
                                    {
                                        writelog("FWUpdate_Line 2621");
                                      if (!string.IsNullOrEmpty(ss_2[0]) && !string.IsNullOrEmpty(ss_2[1]))
                                       {
                                            writelog("FWUpdate_Line 2624");
                                        if (!string.IsNullOrEmpty(ss_3[0]) && !string.IsNullOrEmpty(ss_3[1]))
                                         {
                                                writelog("FWUpdate_Line 2627");
                                                if (!string.IsNullOrEmpty(ss_4[0]) && !string.IsNullOrEmpty(ss_4[1]))
                                                {
                                                    writelog("FWUpdate_Line 2630");
                                                    //dock guid & miniver & model
                                                    if (commandLineInput.Options[0].Option_Value.ToUpper().Equals("UOD") && ss_2[1].ToUpper().Equals("GUID") && ss_3[1].ToUpper().Equals("MINIVERSION") && ss_4[1].ToUpper().Equals("MODEL"))
                                                    {
                                                        writelog("FWUpdate_Line 2634");
                                                        List<string> guid = new List<string>();
                                                        string min = ss_3[0];
                                                        List<string> model = new List<string> { ss_4[0] };

                                                        foreach (var g in _deviceinfo)
                                                        {
                                                            writelog("FWUpdate_Line 2641");
                                                            if (g.ID.ToString().ToUpper() == ss_2[0])
                                                            {
                                                                writelog("FWUpdate_Line 2644");
                                                                Trace.WriteLine($"IN ============");
                                                                guid = new List<string>() { ss_2[0] };
                                                            }
                                                        }
                                                        isShowInfo = true;
                                                        var fwupdate = Auto_FWUpdate2(commandLineInput, cLI_FWU_RESPONSE, true, installPath, isShowInfo, true, guid, model, min);

                                                        result.ExitCode = fwupdate.code;
                                                        result.serialize_Json_response = fwupdate.result;
                                                        ret = true;
                                                    }
                                                    else if (commandLineInput.Options[0].Option_Value.ToUpper().Contains(":\\") && ss_2[1].ToUpper().Equals("GUID") && ss_3[1].ToUpper().Equals("MINIVERSION") && ss_4[1].ToUpper().Equals("MODEL"))
                                                    {
                                                        writelog("FWUpdate_Line 2658");
                                                        List<string> guid = new List<string>();
                                                        string min = ss_3[0];
                                                        List<string> model = new List<string> { ss_4[0] };

                                                        foreach (var g in _deviceinfo)
                                                        {
                                                            writelog("FWUpdate_Line 2665");
                                                            if (g.ID.ToString().ToUpper() == ss_2[0])
                                                            {
                                                                writelog("FWUpdate_Line 2668");
                                                                Trace.WriteLine($"IN ============");
                                                                guid = new List<string>() { ss_2[0] };
                                                            }
                                                        }
                                                        installPath = Path.GetFullPath(commandLineInput.Options[0].Option_Value);
                                                        Trace.WriteLine($"installPath = {installPath}");
                                                        isShowInfo = true;
                                                        var fwupdate = Auto_FWUpdate2(commandLineInput, cLI_FWU_RESPONSE, false, installPath, isShowInfo, true, guid, model, min);

                                                        result.ExitCode = fwupdate.code;
                                                        result.serialize_Json_response = fwupdate.result;
                                                        ret = true;

                                                    }
                                                    else
                                                    {
                                                        writelog("FWUpdate_Line 2685");
                                                        somethingError = true;
                                                    }

                                                }
                                         }
                                            else
                                            {
                                                writelog("FWUpdate_Line 2693");
                                                somethingError = true;
                                            }
                                        }
                                        else
                                        {
                                            writelog("FWUpdate_Line 2699");
                                            somethingError = true;
                                        }
                                    }
                                    else
                                    {
                                        writelog("FWUpdate_Line 2705");
                                        somethingError = true;
                                    }
                                }                                 
                                writelog("FWUpdate_Line !string.IsNullOrEmpty(commandLineInput.Options[0].Option_Value)");
                            }
                            else if (commandLineInput.Options.Count == 3)
                            {
                                writelog("FWUpdate_Line 2712");
                                if (!string.IsNullOrEmpty(commandLineInput.Options[0].Option_Value))
                                {
                                    writelog("FWUpdate_Line 2715");
                                    cLI_FWU_RESPONSE.Value = commandLineInput.Options[0].Option_Value;

                                    //string[] ss_1 = commandLineInput.Options[0].Option_Value.Split(",");
                                    string[] ss_2 = commandLineInput.Options[1].Option_Value.Split(",");
                                    string[] ss_3 = commandLineInput.Options[2].Option_Value.Split(",");

                                    if (ss_2.Length == 2 && ss_3.Length == 2)
                                    {
                                        writelog("FWUpdate_Line 2724");
                                        if (!string.IsNullOrEmpty(ss_2[0]) && !string.IsNullOrEmpty(ss_2[1]))
                                        {
                                            writelog("FWUpdate_Line 2727");
                                            if (!string.IsNullOrEmpty(ss_3[0]) && !string.IsNullOrEmpty(ss_3[1]))
                                            {
                                                writelog("FWUpdate_Line 2730");
                                                //dock guid  && miniver
                                                if (commandLineInput.Options[0].Option_Value.ToUpper().Equals("UOD") && ss_2[1].ToUpper().Equals("GUID") && ss_3[1].ToUpper().Equals("MINIVERSION"))
                                                {
                                                    writelog("FWUpdate_Line 2734");
                                                    List<string> guid = new List<string>();
                                                    string min = ss_3[0];
                                                    foreach (var g in _deviceinfo)
                                                    {
                                                        writelog("FWUpdate_Line 2739");
                                                        if (g.ID.ToString().ToUpper() == ss_2[0])
                                                        {
                                                            writelog("FWUpdate_Line 2742");
                                                            Trace.WriteLine($"IN ============");
                                                            guid = new List<string>() { ss_2[0] };
                                                        }
                                                    }
                                                    isShowInfo = true;
                                                    var fwupdate = Auto_FWUpdate2(commandLineInput, cLI_FWU_RESPONSE, true, installPath, isShowInfo, true, guid, null, min);

                                                    result.ExitCode = fwupdate.code;
                                                    result.serialize_Json_response = fwupdate.result;
                                                    ret = true;
                                                }


                                                else if (commandLineInput.Options[0].Option_Value.ToUpper().Contains(":\\") && ss_2[1].ToUpper().Equals("GUID") && ss_3[1].ToUpper().Equals("MINIVERSION"))
                                                        {
                                                    writelog("FWUpdate_Line 2758");
                                                            List<string> guid = new List<string>();
                                                            string min = ss_3[0];
                                                            foreach (var g in _deviceinfo)
                                                            {
                                                        writelog("FWUpdate_Line 2763");
                                                        if (g.ID.ToString().ToUpper() == ss_2[0])
                                                                {
                                                            writelog("FWUpdate_Line 2766");
                                                                    Trace.WriteLine($"IN ============");
                                                                    guid = new List<string>() { ss_2[0] };
                                                                }
                                                            }
                                                            installPath = Path.GetFullPath(commandLineInput.Options[0].Option_Value);
                                                            Trace.WriteLine($"installPath = {installPath}");
                                                            isShowInfo = true;
                                                            var fwupdate = Auto_FWUpdate2(commandLineInput, cLI_FWU_RESPONSE, false, installPath, isShowInfo, true, guid, null, min);

                                                            result.ExitCode = fwupdate.code;
                                                            result.serialize_Json_response = fwupdate.result;
                                                            ret = true;

                                                        }


                                                //dock guid  && model
                                                else if (commandLineInput.Options[0].Option_Value.ToUpper().Equals("UOD") && ss_2[1].ToUpper().Equals("GUID") && ss_3[1].ToUpper().Equals("MODEL"))
                                                {
                                                    writelog("FWUpdate_Line 2786");
                                                    List<string> guid = new List<string>();
                                                    List<string> model = new List<string> { ss_3[0] };
                                                    foreach (var g in _deviceinfo)
                                                    {
                                                        writelog("FWUpdate_Line 2791");
                                                        if (g.ID.ToString().ToUpper() == ss_2[0])
                                                        {
                                                            writelog("FWUpdate_Line 2794");
                                                            Trace.WriteLine($"IN ============");
                                                            guid = new List<string>() { ss_2[0] };
                                                        }
                                                    }
                                                    isShowInfo = true;
                                                    var fwupdate = Auto_FWUpdate2(commandLineInput, cLI_FWU_RESPONSE, true, installPath, isShowInfo, true, guid, model, null);

                                                    result.ExitCode = fwupdate.code;
                                                    result.serialize_Json_response = fwupdate.result;
                                                    ret = true;
                                                }

                                                else if (commandLineInput.Options[0].Option_Value.ToUpper().Contains(":\\") && ss_2[1].ToUpper().Equals("GUID") && ss_3[1].ToUpper().Equals("MODEL"))
                                                        {
                                                    writelog("FWUpdate_Line 2809");
                                                            List<string> guid = new List<string>();
                                                            List<string> model = new List<string> { ss_3[0] };
                                                            foreach (var g in _deviceinfo)
                                                            {
                                                        writelog("FWUpdate_Line 2814");
                                                        if (g.ID.ToString().ToUpper() == ss_2[0])
                                                                {
                                                            writelog("FWUpdate_Line 2817");
                                                                    Trace.WriteLine($"IN ============");
                                                                    guid = new List<string>() { ss_2[0] };
                                                                }
                                                            }
                                                            installPath = Path.GetFullPath(commandLineInput.Options[0].Option_Value);
                                                            Trace.WriteLine($"installPath = {installPath}");
                                                            isShowInfo = true;
                                                            var fwupdate = Auto_FWUpdate2(commandLineInput, cLI_FWU_RESPONSE, false, installPath, isShowInfo, true, guid, model, null);

                                                            result.ExitCode = fwupdate.code;
                                                            result.serialize_Json_response = fwupdate.result;
                                                            ret = true;

                                                        }

                                                //dock miniver  && model
                                                else if (commandLineInput.Options[0].Option_Value.ToUpper().Equals("UOD") && ss_2[1].ToUpper().Equals("MINIVERSION") && ss_3[1].ToUpper().Equals("MODEL"))
                                                {
                                                    writelog("FWUpdate_Line 2836");
                                                    List<string> model = new List<string> { ss_3[0] };
                                                    string min = ss_2[0];

                                                    isShowInfo = true;
                                                    var fwupdate = Auto_FWUpdate2(commandLineInput, cLI_FWU_RESPONSE, true, installPath, isShowInfo, true, null, model, min);

                                                    result.ExitCode = fwupdate.code;
                                                    result.serialize_Json_response = fwupdate.result;
                                                    ret = true;
                                                }

                                                else if (commandLineInput.Options[0].Option_Value.ToUpper().Contains(":\\") && ss_2[1].ToUpper().Equals("MINIVERSION") && ss_3[1].ToUpper().Equals("MODEL"))
                                                        {
                                                    writelog("FWUpdate_Line 2850");
                                                            List<string> model = new List<string> { ss_3[0] };
                                                            string min = ss_2[0];
                                                            installPath = Path.GetFullPath(commandLineInput.Options[0].Option_Value);
                                                            Trace.WriteLine($"installPath = {installPath}");
                                                            isShowInfo = true;
                                                            var fwupdate = Auto_FWUpdate2(commandLineInput, cLI_FWU_RESPONSE, false, installPath, isShowInfo, true, null, model, min);

                                                            result.ExitCode = fwupdate.code;
                                                            result.serialize_Json_response = fwupdate.result;
                                                            ret = true;

                                                        }
                                                else
                                                {
                                                    writelog("FWUpdate_Line 2865");
                                                    somethingError = true;
                                                }

                                            }
                                            else
                                            {
                                                writelog("FWUpdate_Line 2872");
                                                somethingError = true;
                                            }
                                        }
                                        else
                                        {
                                            writelog("FWUpdate_Line 2878");
                                            somethingError = true;
                                        }
                                    }
                                    else
                                    {
                                        writelog("FWUpdate_Line 2884");
                                        somethingError = true;
                                    }
                                }
                                else
                                {
                                    writelog("FWUpdate_Line 2890");
                                    somethingError = true;
                                }
                            }
                            else if (commandLineInput.Options.Count == 2)
                            {
                                writelog("FWUpdate_Line 2896");
                                if (!string.IsNullOrEmpty(commandLineInput.Options[0].Option_Value))
                                {
                                    writelog("FWUpdate_Line 2899");
                                    cLI_FWU_RESPONSE.Value = commandLineInput.Options[0].Option_Value;

                                    //string[] ss_1 = commandLineInput.Options[0].Option_Value.Split(",");
                                    string[] ss_2 = commandLineInput.Options[1].Option_Value.Split(",");
                                    

                                    if (ss_2.Length == 2 )
                                    {
                                        writelog("FWUpdate_Line 2908");
                                        if (!string.IsNullOrEmpty(ss_2[0]) && !string.IsNullOrEmpty(ss_2[1]))
                                        {
                                            writelog("FWUpdate_Line 2911");
                                            //dock only guid
                                            if (commandLineInput.Options[0].Option_Value.ToUpper().Equals("UOD") && ss_2[1].ToUpper().Equals("GUID"))
                                            {
                                                writelog("FWUpdate_Line 2915");
                                                List<string> guid = new List<string>();



                                                foreach (var g in _deviceinfo)
                                                {
                                                    writelog("FWUpdate_Line 2922");
                                                    if (g.ID.ToString().ToUpper() == ss_2[0])
                                                    {
                                                        writelog("FWUpdate_Line 2925");
                                                        Trace.WriteLine($"IN ============");
                                                        guid = new List<string>() { ss_2[0] };
                                                    }
                                                }
                                                isShowInfo = true;
                                                var fwupdate = Auto_FWUpdate2(commandLineInput, cLI_FWU_RESPONSE, true, installPath, isShowInfo, true, guid, null, null);

                                                result.ExitCode = fwupdate.code;
                                                result.serialize_Json_response = fwupdate.result;
                                                ret = true;
                                            }

                                            else if (commandLineInput.Options[0].Option_Value.ToUpper().Contains(":\\") && ss_2[1].ToUpper().Equals("GUID"))
                                                    {
                                                writelog("FWUpdate_Line 2940");
                                                        List<string> guid = new List<string>();


                                                        foreach (var g in _deviceinfo)
                                                        {
                                                    writelog("FWUpdate_Line 2946");
                                                    if (g.ID.ToString().ToUpper() == ss_2[0])
                                                            {
                                                        writelog("FWUpdate_Line 2949");
                                                                Trace.WriteLine($"IN ============");
                                                                guid = new List<string>() { ss_2[0] };
                                                            }
                                                        }
                                                        installPath = Path.GetFullPath(commandLineInput.Options[0].Option_Value);
                                                        Trace.WriteLine($"installPath = {installPath}");
                                                        isShowInfo = true;
                                                        var fwupdate = Auto_FWUpdate2(commandLineInput, cLI_FWU_RESPONSE, false, installPath, isShowInfo, true, guid, null, null);

                                                        result.ExitCode = fwupdate.code;
                                                        result.serialize_Json_response = fwupdate.result;
                                                        ret = true;

                                                    }


                                            //dock only miniversion
                                            else if (commandLineInput.Options[0].Option_Value.ToUpper().Equals("UOD") && ss_2[1].ToUpper().Equals("MINIVERSION"))
                                            {
                                                writelog("FWUpdate_Line 2969");
                                                string min = ss_2[0];

                                                isShowInfo = true;
                                                var fwupdate = Auto_FWUpdate2(commandLineInput, cLI_FWU_RESPONSE, true, installPath, isShowInfo, true, null, null, min);

                                                result.ExitCode = fwupdate.code;
                                                result.serialize_Json_response = fwupdate.result;
                                                ret = true;
                                            }

                                            else if (commandLineInput.Options[0].Option_Value.ToUpper().Contains(":\\") && ss_2[1].ToUpper().Equals("MINIVERSION"))
                                                    {
                                                writelog("FWUpdate_Line 2982");
                                                        List<string> guid = new List<string>();
                                                        string min = ss_2[0];
                                                        

                                                        foreach (var g in _deviceinfo)
                                                        {
                                                    writelog("FWUpdate_Line 2989");
                                                    if (g.ID.ToString().ToUpper() == ss_2[0])
                                                            {
                                                        writelog("FWUpdate_Line 2992");
                                                                Trace.WriteLine($"IN ============");
                                                                guid = new List<string>() { commandLineInput.Options[1].Option_Value };
                                                            }
                                                        }
                                                        installPath = Path.GetFullPath(commandLineInput.Options[0].Option_Value);
                                                        Trace.WriteLine($"installPath = {installPath}");
                                                        isShowInfo = true;
                                                        var fwupdate = Auto_FWUpdate2(commandLineInput, cLI_FWU_RESPONSE, false, installPath, isShowInfo, true, null, null, min);

                                                        result.ExitCode = fwupdate.code;
                                                        result.serialize_Json_response = fwupdate.result;
                                                        ret = true;

                                                    }
 

                                            //dock only model
                                            else if (commandLineInput.Options[0].Option_Value.ToUpper().Equals("UOD") && ss_2[1].ToUpper().Equals("MODEL"))
                                            {
                                                writelog("FWUpdate_Line 3012");
                                                List<string> model = new List<string> { ss_2[0] };

                                                isShowInfo = true;
                                                var fwupdate = Auto_FWUpdate2(commandLineInput, cLI_FWU_RESPONSE, true, installPath, isShowInfo, true, null, model, null);

                                                result.ExitCode = fwupdate.code;
                                                result.serialize_Json_response = fwupdate.result;
                                                ret = true;
                                            }

                                            else if (commandLineInput.Options[0].Option_Value.ToUpper().Contains(":\\") && ss_2[1].ToUpper().Equals("MODEL"))
                                                    {
                                                writelog("FWUpdate_Line 3025");
                                                        List<string> model = new List<string> { ss_2[0] };


                                                        installPath = Path.GetFullPath(commandLineInput.Options[0].Option_Value);
                                                        Trace.WriteLine($"installPath = {installPath}");
                                                        isShowInfo = true;
                                                        var fwupdate = Auto_FWUpdate2(commandLineInput, cLI_FWU_RESPONSE, false, installPath, isShowInfo, true, null, model, null);

                                                        result.ExitCode = fwupdate.code;
                                                        result.serialize_Json_response = fwupdate.result;
                                                        ret = true;

                                                    }
                                            else
                                            {
                                                writelog("FWUpdate_Line 3041");
                                                somethingError = true;
                                            }

                                        }
                                        else
                                        {
                                            writelog("FWUpdate_Line 3048");
                                            somethingError = true;
                                        }
                                    }
                                    else
                                    {
                                        writelog("FWUpdate_Line 3054");
                                        somethingError = true;
                                    }
                                }
                                else
                                {
                                    writelog("FWUpdate_Line 3060");
                                    somethingError = true;
                                }
                            }
                            else if (commandLineInput.Options.Count == 1)
                            {
                                writelog("FWUpdate_Line 3066");
                                if (!string.IsNullOrEmpty(commandLineInput.Options[0].Option_Value))
                                {
                                    writelog("FWUpdate_Line 3069");
                                    cLI_FWU_RESPONSE.Value = commandLineInput.Options[0].Option_Value;

                                    //string[] ss_1 = commandLineInput.Options[0].Option_Value.Split(",");

                                        if (commandLineInput.Options[0].Option_Value.ToUpper().Equals("UOD"))
                                        {
                                        writelog("FWUpdate_Line 3076");
                                            isShowInfo = true;
                                            var fwupdate = Auto_FWUpdate2(commandLineInput, cLI_FWU_RESPONSE, true, installPath, isShowInfo, true);

                                            result.ExitCode = fwupdate.code;
                                            result.serialize_Json_response = fwupdate.result;
                                            ret = true;
                                        }

                                        else if (commandLineInput.Options[0].Option_Value.ToUpper().Contains(":\\"))
                                                {
                                        writelog("FWUpdate_Line 3087");
                                                    //string[] ss_1 = commandLineInput.Options[0].Option_Value.Split(",");
                                        Debug.WriteLine("commandLineInput.Options[0].Option_Value" + commandLineInput.Options[0].Option_Value);
                                        installPath = commandLineInput.Options[0].Option_Value;

                                        installPath = installPath.Replace("\"", "");

                                        //installPath = Path.GetFullPath(commandLineInput.Options[0].Option_Value);
                                                    Trace.WriteLine($"installPath = {installPath}");
                                                    isShowInfo = true;
                                                    var fwupdate = Auto_FWUpdate2(commandLineInput, cLI_FWU_RESPONSE, false, installPath, isShowInfo, true);

                                                    result.ExitCode = fwupdate.code;
                                                    result.serialize_Json_response = fwupdate.result;
                                                    ret = true;

                                                }
                                    else if (commandLineInput.Options[0].Option_Value.ToUpper().Equals("DEFER") )
                                    {
                                        writelog("FWUpdate_Line 3101");
                                        if (commandLineInput.Options.Count > 2)
                                        {
                                            writelog("FWUpdate_Line 3104");
                                            cLI_FWU_RESPONSE.Result = "FAIL";
                                            cLI_FWU_RESPONSE.Message = "Bring in extra strings:";
                                            for (int i = 0; i < commandLineInput.Options.Count; i++)
                                            {
                                                writelog("FWUpdate_Line 3109");
                                                cLI_FWU_RESPONSE.Message += $"{commandLineInput.Options[i].Option_Name}={commandLineInput.Options[i].Option_Value}\n";
                                            }
                                            break;
                                        }
                                        writelog("FWUpdate_Line 3114");
                                        ret = GetFWUpdateList(commandLineInput, cLI_FWU_RESPONSE, isShowInfo, true);
                                    }
                                    else
                                    {
                                        writelog("FWUpdate_Line 3118");
                                        somethingError = true;
                                    }
                                }
                                else
                                {
                                    writelog("FWUpdate_Line 3125");
                                    somethingError = true;
                                }
                            }
                            else if (commandLineInput.Options.Count == 0)
                            {
                                writelog("FWUpdate_Line 3131");
                                isShowInfo = true;
                                var fwupdate = Auto_FWUpdate2(commandLineInput, cLI_FWU_RESPONSE, true, installPath, isShowInfo, true);

                                result.ExitCode = fwupdate.code;
                                result.serialize_Json_response = fwupdate.result;
                                ret = true;
                            }
                            else
                            {
                                writelog("FWUpdate_Line 3141");
                                somethingError = true;
                            }
                            writelog("FWUpdate_Line 3144");
                            cLI_FWU_RESPONSE.Result = ret == true ? "PASS" : "FAIL";

                            if (somethingError)
                            {
                                writelog("FWUpdate_Line 3149");
                                cLI_FWU_RESPONSE.Result = "FAIL";
                                cLI_FWU_RESPONSE.Message = "Bring in extra strings:";
                                for (int i = 0; i < commandLineInput.Options.Count; i++)
                                {
                                    writelog("FWUpdate_Line 3154");
                                    cLI_FWU_RESPONSE.Message += $"{commandLineInput.Options[i].Option_Name}={commandLineInput.Options[i].Option_Value}\n";
                                }
                                break;
                            }
                        }
                        else
                        {
                            writelog("FWUpdate_Line 3162");
                            CLI_RESPONSE rsp = new CLI_RESPONSE()
                            {
                                Command = commandLineInput.Command,
                                TargetFeature = commandLineInput.TargetFeature,
                                Result = "FAIL",
                                Message = "No Device connected",
                            };
                            System.Console.WriteLine(JsonConvert.SerializeObject(rsp, Formatting.Indented));
                            return ((int)CLI_ExitCode.fail_FWUpdate, JsonConvert.SerializeObject(rsp, Formatting.Indented));
                        }
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
                        writelog("FWUpdate_Line 3246");
                        cLI_FWU_RESPONSE.Message = "Input FAIL";
                        ret = false;
                        break;
                }
                writelog("FWUpdate_Line 3251");
                cLI_RESPONSE.Result = ret == true ? "PASS" : "FAIL";
                if (cLI_FWU_RESPONSE != null)
                {
                    writelog("FWUpdate_Line 3255");
                    output = JsonConvert.SerializeObject(cLI_FWU_RESPONSE, Formatting.Indented);
                    //output = cLI_FWU_RESPONSE.OutputLog(cLI_FWU_RESPONSE, commandLineInput);
                }
                else
                {
                    writelog("FWUpdate_Line 3260");
                    output = JsonConvert.SerializeObject(cLI_RESPONSE, Formatting.Indented);
                    //output = cLI_RESPONSE.OutputLog(cLI_RESPONSE, commandLineInput);
                }
                if (ret == true)
                {
                    writelog("FWUpdate_Line 3265");
                    return ((int)CLI_ExitCode.success, output);
                }
                else if (somethingError)
                {
                    writelog("FWUpdate_Line 3270");
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
                    writelog("FWUpdate_Line 3283");
                    //CLI_RESPONSE rsp = new CLI_RESPONSE()
                    //{
                    //    Command = commandLineInput.Command,
                    //    TargetFeature = commandLineInput.TargetFeature,
                    //    Result = "FAIL",
                    //    Message = "FW update failure",
                    //};
                    return ((int)CLI_ExitCode.fail_FWUpdate, output);
                    //return ((int)CLI_ExitCode.fail_FWUpdate, JsonConvert.SerializeObject(output, Formatting.Indented));
                }
            }
            catch(Exception ex)
            {
                writelog("FWUpdate_Line 3297");
                writelog($"FWUpdate_Line 3297 Error : {ex.Message}");
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

        private (int code, string json) NoDeviceConnectResponse(CommandLineInput commandLineInput)
        {
            CLI_RESPONSE rsp = new CLI_RESPONSE()
            {
                Command = commandLineInput.Command,
                TargetFeature = commandLineInput.TargetFeature,
                Result = "FAIL",
                Message = "No device connected",
            };
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

            List<DeviceInfo> _deviceinfo = null;
            _deviceinfo = _devMgr.GetDevices().Result.deviceInfo;

            if (!commandLineInput.isCliRunAdmin)
            {
                cLI_RESPONSE.Result = "FAIL";
                cLI_RESPONSE.Message = "Not Admin";
                Console.WriteLine(JsonConvert.SerializeObject(cLI_RESPONSE, Formatting.Indented));
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
            bool _recode_pen = false;
            bool _recode_dongle = false;

            string miniver = null;
            List<string> model = null;
            List<string> guid = null;
            List<string> serviceTag = null;
            bool isUod = true;

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

                if (commandLineInput.Options.Count > 0)
                {
                    cLI_FWU_RESPONSE.Value = commandLineInput.Options[0].Option_Value;
                    string[] ss_1 = commandLineInput.Options[0].Option_Value.Split(",");

                    if (ss_1.Length == 2)
                    {
                        if (!string.IsNullOrEmpty(ss_1[0]) && !string.IsNullOrEmpty(ss_1[1]))
                        {
                            if (ss_1[0].ToUpper().Equals("DISPLAY"))
                            {
                                _AllInfoMonitors = _devMgr.GetMonitors().Result;
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

                                            if (ss_tag.Length == 2)
                                            {
                                                if (!string.IsNullOrEmpty(ss_tag[0]) && !string.IsNullOrEmpty(ss_tag[1]))
                                                {
                                                    var findDevice = false;
                                                    if (_AllInfoMonitors.Any(x => x.edid.ServiceTag.ToUpper().Equals(ss_tag[0].ToUpper())))
                                                    {
                                                        serviceTag = new List<string> { ss_tag[0] };
                                                        findDevice = true;
                                                    }
                                                    if (!findDevice)
                                                    {
                                                        return NoDeviceConnectResponse(commandLineInput);
                                                    }
                                                }
                                            }
                                        }
                                        if (commandLineInput.Options[i].Option_Value.ToUpper().Contains("MINIVERSION"))
                                        {
                                            string[] ss_min = commandLineInput.Options[i].Option_Value.Split(",");

                                            if (ss_min.Length == 2)
                                            {
                                                if (!string.IsNullOrEmpty(ss_min[0]) && !string.IsNullOrEmpty(ss_min[1]))
                                                {
                                                    var findDevice = false;
                                                    if (_AllInfoMonitors.Any(x => x.FwVersion.ToUpper().Equals(ss_min[0].ToUpper())))
                                                    {
                                                        miniver = ss_min[0];
                                                        findDevice = true;
                                                    }
                                                    if (!findDevice)
                                                    {
                                                        return NoDeviceConnectResponse(commandLineInput);
                                                    }
                                                }
                                            }
                                        }
                                        if (commandLineInput.Options[i].Option_Value.ToUpper().Contains("MODEL"))
                                        {
                                            string[] ss_mod = commandLineInput.Options[i].Option_Value.Split(",");

                                            if (ss_mod.Length == 2)
                                            {
                                                if (!string.IsNullOrEmpty(ss_mod[0]) && !string.IsNullOrEmpty(ss_mod[1]))
                                                {
                                                    var findDevice = false;
                                                    if (_AllInfoMonitors.Any(x => x.modelName.ToUpper().Equals(ss_mod[0].ToUpper())))
                                                    {
                                                        model = new List<string> { ss_mod[0] };
                                                        findDevice = true;
                                                    }
                                                    if (!findDevice)
                                                    {
                                                        return NoDeviceConnectResponse(commandLineInput);
                                                    }
                                                }
                                            }
                                        }
                                        if (commandLineInput.Options[i].Option_Value.ToUpper().Contains("FILEPATH"))
                                        {
                                            string[] ss_filepath = commandLineInput.Options[i].Option_Value.Split(",");

                                            if (ss_filepath.Length == 2)
                                            {
                                                if (!string.IsNullOrEmpty(ss_filepath[0]) && !string.IsNullOrEmpty(ss_filepath[1]))
                                                {
                                                    installPath = ss_filepath[0];
                                                    Trace.WriteLine($"installPath = {installPath}");
                                                }
                                            }
                                        }
                                    }
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

                                var fwupdate = Auto_FWUpdate_display(commandLineInput, cLI_FWU_RESPONSE, installPath, isShowInfo, isForce, serviceTag, model, miniver, isDefer);
                                result.ExitCode = fwupdate.code;
                                result.serialize_Json_response = fwupdate.result;
                                ret = true;
                            }
                            else if (!ss_1[0].ToUpper().Equals("DISPLAY"))
                            {
                                _deviceinfo = _devMgr.GetDevices().Result.deviceInfo;
                                foreach (var g in _deviceinfo)
                                {
                                    if (g.LogicalDeviceType == "LogicalMouse" && ss_1[0].ToUpper().Equals("MOUSE"))
                                    {
                                        _recode_mouse = true;
                                    }
                                    else if (g.LogicalDeviceType == "LogicalKeyboard" && ss_1[0].ToUpper().Equals("KEYBOARD"))
                                    {
                                        _recode_kb = true;
                                    }
                                    else if (g.LogicalDeviceType == "LogicalHeadset" && ss_1[0].ToUpper().Equals("HEADSET"))
                                    {
                                        _recode_headset = true;
                                    }
                                    else if (g.LogicalDeviceType == "LogicalWebcam" && ss_1[0].ToUpper().Equals("WEBCAM"))
                                    {
                                        _recode_webcam = true;
                                    }
                                    else if (g.LogicalDeviceType == "LogicalWiredAudio" && ss_1[0].ToUpper().Equals("SPEAKER"))
                                    {
                                        _recode_speaker = true;
                                    }
                                    else if (g.LogicalDeviceType == "LogicalPen" && ss_1[0].ToUpper().Equals("PEN"))
                                    {
                                        _recode_pen = true;
                                    }
                                    else if (g.LogicalDeviceType == "LogicalDock" && ss_1[0].ToUpper().Equals("DOCK"))
                                    {
                                        _recode_dock = true;
                                    }
                                    else if (ss_1[0].ToUpper().Equals("DONGLE"))
                                    {
                                        _recode_dongle = true;
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

                                            if (ss_guid.Length == 2)
                                            {
                                                if (!string.IsNullOrEmpty(ss_guid[0]) && !string.IsNullOrEmpty(ss_guid[1]))
                                                {
                                                    if (ss_1[0].ToUpper() == "DOCK")
                                                    {
                                                        CLI_RESPONSE rsp = new CLI_RESPONSE()
                                                        {
                                                            Command = commandLineInput.Command,
                                                            TargetFeature = commandLineInput.TargetFeature,
                                                            Result = "FAIL",
                                                            Message = "Dock not support GUID option",
                                                        };
                                                        Console.WriteLine(JsonConvert.SerializeObject(rsp, Formatting.Indented));
                                                        return ((int)CLI_ExitCode.fail_FWUpdate, JsonConvert.SerializeObject(rsp, Formatting.Indented));
                                                    }

                                                    var findDevice = false;
                                                    foreach (var g in _deviceinfo)
                                                    {
                                                        if (g.ID.ToString().ToUpper() == ss_guid[0].ToUpper())
                                                        {
                                                            guid = new List<string>() { ss_guid[0] };
                                                            findDevice = true;
                                                        }
                                                    }
                                                    if (!findDevice)
                                                    {
                                                        return NoDeviceConnectResponse(commandLineInput);
                                                    }
                                                }
                                            }
                                        }
                                        else if (commandLineInput.Options[i].Option_Value.ToUpper().Contains("MINIVERSION"))
                                        {
                                            string[] ss_min = commandLineInput.Options[i].Option_Value.Split(",");

                                            if (ss_min.Length == 2)
                                            {
                                                if (!string.IsNullOrEmpty(ss_min[0]) && !string.IsNullOrEmpty(ss_min[1]))
                                                {
                                                    var findDevice = false;
                                                    foreach (var g in _deviceinfo)
                                                    {
                                                        if (g.FirmwareVersion.ToUpper() == ss_min[0].ToUpper())
                                                        {
                                                            miniver = ss_min[0];
                                                            findDevice = true;
                                                        }
                                                    }
                                                    if (!findDevice)
                                                    {
                                                        return NoDeviceConnectResponse(commandLineInput);
                                                    }
                                                }
                                            }
                                        }
                                        else if (commandLineInput.Options[i].Option_Value.ToUpper().Contains("MODEL"))
                                        {
                                            string[] ss_mod = commandLineInput.Options[i].Option_Value.Split(",");

                                            if (ss_mod.Length == 2)
                                            {
                                                if (!string.IsNullOrEmpty(ss_mod[0]) && !string.IsNullOrEmpty(ss_mod[1]))
                                                {
                                                    var findDevice = false;
                                                    foreach (var g in _deviceinfo)
                                                    {
                                                        if (g.ModelNumber.ToUpper() == ss_mod[0].ToUpper())
                                                        {
                                                            model = new List<string> { ss_mod[0] };
                                                            findDevice = true;
                                                        }
                                                    }
                                                    if (!findDevice)
                                                    {
                                                        return NoDeviceConnectResponse(commandLineInput);
                                                    }
                                                }

                                            }
                                        }
                                        else if (commandLineInput.Options[i].Option_Value.ToUpper().Contains("SERVICETAG"))
                                        {
                                            string[] serviceTaInputValues = commandLineInput.Options[i].Option_Value.Split(",");

                                            if (serviceTaInputValues.Length == 2)
                                            {
                                                if (!string.IsNullOrEmpty(serviceTaInputValues[0]) && !string.IsNullOrEmpty(serviceTaInputValues[1]))
                                                {
                                                    var findDevice = false;
                                                    foreach (var g in _deviceinfo)
                                                    {
                                                        if (!string.IsNullOrWhiteSpace(g.DockServiceTag) && g.DockServiceTag.ToUpper() == serviceTaInputValues[0].ToUpper())
                                                        {
                                                            serviceTag = new List<string> { serviceTaInputValues[0] };
                                                            findDevice = true;
                                                        }
                                                    }
                                                    if (!findDevice)
                                                    {
                                                        return NoDeviceConnectResponse(commandLineInput);
                                                    }
                                                }
                                            }
                                        }
                                        else if (commandLineInput.Options[i].Option_Value.ToUpper().Contains("UOD"))
                                        {
                                            string[] uodInputValues = commandLineInput.Options[i].Option_Value.Split(",");

                                            if (uodInputValues.Length == 2)
                                            {
                                                if (!string.IsNullOrEmpty(uodInputValues[0]) && !string.IsNullOrEmpty(uodInputValues[1]))
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
                                                        Console.WriteLine(JsonConvert.SerializeObject(rsp, Formatting.Indented));
                                                        return ((int)CLI_ExitCode.fail_FWUpdate, JsonConvert.SerializeObject(rsp, Formatting.Indented));
                                                    }
                                                }
                                            }
                                        }
                                        else if (commandLineInput.Options[i].Option_Value.ToUpper().Contains("FILEPATH"))
                                        {
                                            string[] ss_filepath = commandLineInput.Options[i].Option_Value.Split(",");

                                            if (ss_filepath.Length == 2)
                                            {
                                                if (!string.IsNullOrEmpty(ss_filepath[0]) && !string.IsNullOrEmpty(ss_filepath[1]))
                                                {
                                                    installPath = ss_filepath[0];
                                                }
                                            }
                                        }
                                    }
                                    if (_recode_mouse || _recode_kb || _recode_dock || _recode_headset || _recode_webcam || _recode_speaker || _recode_pen || _recode_dongle)
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
                                                    Console.WriteLine(JsonConvert.SerializeObject(rsp, Formatting.Indented));
                                                    return ((int)CLI_ExitCode.fail_FWUpdate, JsonConvert.SerializeObject(rsp, Formatting.Indented));
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
                                                        var fwupdate = Auto_FWUpdate2(commandLineInput, cLI_FWU_RESPONSE, isUod, installPath, isShowInfo, isForce, guid, model, miniver, isDefer, serviceTag);
                                                        result.ExitCode = fwupdate.code;
                                                        result.serialize_Json_response = fwupdate.result;
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
                Console.WriteLine(JsonConvert.SerializeObject(rsp, Formatting.Indented));
                return ((int)CLI_ExitCode.fail_FWUpdate, JsonConvert.SerializeObject(rsp, Formatting.Indented));
            }
        }

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

            }else if (dock_recode)
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
                    //cli_FWU_RESPONSE.OutputLog(cli_FWU_RESPONSE, commandLineInput);
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
        
        private (int code, string result) Auto_FWUpdate2(CommandLineInput commandLineInput, CLI_FWU_RESPONSE cli_FWU_RESPONSE, bool isUODMode, string installPath, bool isShowInfo = true, bool isForce = false, List<string> guid = null, List<string> model = null, string miniver = null, bool isDefer = false, List<string> serviceTag = null)
        {
            try
            {
                List<DeviceType> deviceTypes = new List<DeviceType>();
                DeviceType deviceType = DeviceType.Unknown;
                if (commandLineInput.Options.Count > 0)
                {
                    (deviceType, deviceTypes) = SetDevice(commandLineInput);
                }

                _devMgr.ProgressUpdate_Notify -= _FWUpdatePlugin_ProgressUpdate;
                _devMgr.ProgressUpdate_Notify += _FWUpdatePlugin_ProgressUpdate;
                _devMgr.DownloadAndInstall_Result_Notify -= Download_Event;
                _devMgr.DownloadAndInstall_Result_Notify += Download_Event;
                if (installPath != "")
                {
                    cli_FWU_RESPONSE.Result = "PASS";
                    Task.Run(new Action(() =>
                    {
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
                    FWUpdateInfoPackage fwUpdateInfoPackage = _devMgr.GetFWUpdateInfo(isShowInfo, isForce, isDefer, deviceTypes, isUODMode, false, true, false, guid, serviceTag, model, miniver).Result;
                    if (fwUpdateInfoPackage.FWUpdateInfo.Count <= 0)
                    {
                        cli_FWU_RESPONSE.Message = "No updates available";
                        cli_FWU_RESPONSE.Result = "PASS";
                        return ((int)CLI_ExitCode.NoUpdate, JsonConvert.SerializeObject(cli_FWU_RESPONSE, Formatting.Indented));
                    }
                    if (deviceType != DeviceType.Unknown)
                    {
                        cli_FWU_RESPONSE.Model = string.Join(",", fwUpdateInfoPackage.FWUpdateInfo.Select(_ => _.Model));
                        cli_FWU_RESPONSE.ServiceTag = string.Join(",", fwUpdateInfoPackage.FWUpdateInfo.Select(_ => _.ServiceTag ?? "N/A"));
                        cli_FWU_RESPONSE.FWVersion = string.Join(",", fwUpdateInfoPackage.FWUpdateInfo.Select(_ => $"[{_.DeviceVersion}]"));
                        cli_FWU_RESPONSE.FWUpdateRESPONSE.AddRange(fwUpdateInfoPackage.FWUpdateInfo.Select(_ => $"Ready to start updating Device:{_.DeviceName} to Version:{_.TheLatestVersion}"));
                        cli_FWU_RESPONSE.Result = "PASS";
                        Task.Run(new Action(() =>
                        {
                            do
                            {
                                Thread.Sleep(100);
                            } while (retFWUpdateInfos == null);

                            foreach (FWUpdateInfo retFWUpdateInfo in retFWUpdateInfos)
                            {
                                //cli_FWU_RESPONSE.Model = retFWUpdateInfo.Model;
                                if (retFWUpdateInfo.FWUErrorCode == FWUErrorCode.NoError)
                                {
                                    cli_FWU_RESPONSE.FWUpdateRESPONSE.Add($"{retFWUpdateInfo.DeviceName} update success.");
                                    cli_FWU_RESPONSE.Result = "PASS";
                                }
                                else
                                {
                                    cli_FWU_RESPONSE.FWUpdateRESPONSE.Add($"{retFWUpdateInfo.DeviceName} update fail. Fail message:{retFWUpdateInfo.FWUErrorCode.ToString()}");
                                    cli_FWU_RESPONSE.Result = "FAIL";
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
                        cli_FWU_RESPONSE.Result = "FAIL";
                        return ((int)CLI_ExitCode.fail_NotSupport, JsonConvert.SerializeObject(cli_FWU_RESPONSE, Formatting.Indented));
                    }
                }
                return ((int)CLI_ExitCode.success, JsonConvert.SerializeObject(cli_FWU_RESPONSE, Formatting.Indented));
            }
            catch
            {
                cli_FWU_RESPONSE.Result = "FAIL";
                return ((int)CLI_ExitCode.fail_FWUpdate, JsonConvert.SerializeObject(cli_FWU_RESPONSE, Formatting.Indented));
            }
        }

        private (int code, string result) Auto_FWUpdate_display(CommandLineInput commandLineInput, CLI_FWU_RESPONSE cli_FWU_RESPONSE, string installPath, bool isShowInfo = true, bool isForce = false, List<string> serviceTags = null, List<string> model = null, string miniver = null, bool isDefer = false)
        {
            try
            {
                _devMgr.ProgressUpdate_Notify -= _FWUpdatePlugin_ProgressUpdate;
                _devMgr.ProgressUpdate_Notify += _FWUpdatePlugin_ProgressUpdate;
                _devMgr.DownloadAndInstall_Result_Notify -= Download_Event;
                _devMgr.DownloadAndInstall_Result_Notify += Download_Event;
                if (installPath != "")
                {
                    cli_FWU_RESPONSE.Result = "PASS";
                    Task.Run(new Action(() =>
                    {
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
                    FWUpdateInfoPackage fwUpdateInfoPackage = _devMgr.GetFWUpdateInfo(isShowInfo, isForce, isDefer, null, false, true, true, false, null, serviceTags, null, miniver).Result;

                    if (fwUpdateInfoPackage.FWUpdateInfo.Count <= 0)
                    {
                        cli_FWU_RESPONSE.Message = "No updates available";
                        cli_FWU_RESPONSE.Result = "PASS";
                        return ((int)CLI_ExitCode.NoUpdate, JsonConvert.SerializeObject(cli_FWU_RESPONSE, Formatting.Indented));
                    }

                    cli_FWU_RESPONSE.Model = string.Join(",", fwUpdateInfoPackage.FWUpdateInfo.Select(_ => _.Model));
                    cli_FWU_RESPONSE.ServiceTag = string.Join(",", fwUpdateInfoPackage.FWUpdateInfo.Select(_ => _.ServiceTag ?? "N/A"));
                    cli_FWU_RESPONSE.FWVersion = string.Join(",", fwUpdateInfoPackage.FWUpdateInfo.Select(_ => $"[{_.DeviceVersion}]"));
                    cli_FWU_RESPONSE.FWUpdateRESPONSE.AddRange(fwUpdateInfoPackage.FWUpdateInfo.Select(_ => $"Ready to start updating Device:{_.DeviceName} to Version:{_.TheLatestVersion}"));
                    cli_FWU_RESPONSE.Result = "PASS";
                    Task.Run(new Action(() =>
                    {
                        do
                        {
                            Thread.Sleep(100);
                        } while (retFWUpdateInfos == null);

                        foreach (FWUpdateInfo retFWUpdateInfo in retFWUpdateInfos)
                        {
                            //cli_FWU_RESPONSE.Model = retFWUpdateInfo.Model;
                            if (retFWUpdateInfo.FWUErrorCode == FWUErrorCode.NoError)
                            {
                                cli_FWU_RESPONSE.FWUpdateRESPONSE.Add($"{retFWUpdateInfo.DeviceName} update success.");
                                cli_FWU_RESPONSE.Result = "PASS";
                            }
                            else
                            {
                                cli_FWU_RESPONSE.FWUpdateRESPONSE.Add($"{retFWUpdateInfo.DeviceName} update fail. Fail message:{retFWUpdateInfo.FWUErrorCode.ToString()}");
                                cli_FWU_RESPONSE.Result = "FAIL";
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
                cli_FWU_RESPONSE.Result = "FAIL";
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
                                        SWUpdateInfoPackage swUpdateInfoPackage = _devMgr.SW_GetSWUpdateInfo(true, true, false, true, false).Result;
                                        Trace.WriteLine($"swUpdateInfoPackage {swUpdateInfoPackage}");
                                        ret = true;
                                        _devMgr.SW_DownloadAndInstall(swUpdateInfoPackage.SWUpdateInfo, false, installPath);
                                        ret = true;
                                    }
                                    //peripherals no guid minversion model
                                    else if (ss_1[1].ToUpper().Equals("FORCEWITHNONOTICE") && commandLineInput.Options.Count == 1)
                                    {
                                        writelog("FWUpdate_Line 3922");
                                        SWUpdateInfoPackage swUpdateInfoPackage = _devMgr.SW_GetSWUpdateInfo(false, true, false, true, false).Result;
                                        Trace.WriteLine($"swUpdateInfoPackage {swUpdateInfoPackage}");
                                        ret = true;
                                        _devMgr.SW_DownloadAndInstall(swUpdateInfoPackage.SWUpdateInfo, false, installPath);
                                        ret = true;
                                    }
                                    else if (ss_1[1].ToUpper().Equals("DEFER") && commandLineInput.Options.Count == 1)
                                    {
                                        writelog("FWUpdate_Line 3931");
                                        SWUpdateInfoPackage swUpdateInfoPackage = _devMgr.SW_GetSWUpdateInfo(true, false, true, true, false).Result;
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

                        }
                        else if (commandLineInput.Options.Count == 0)
                        {
                            writelog("FWUpdate_Line 3957");
                            SWUpdateInfoPackage swUpdateInfoPackage = _devMgr.SW_GetSWUpdateInfo(true, false, true).Result;
                            Trace.WriteLine($"swUpdateInfoPackage {swUpdateInfoPackage}");
                            ret = true;
                            _devMgr.SW_DownloadAndInstall(swUpdateInfoPackage.SWUpdateInfo, false, installPath);
                        }
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
                    output =  JsonConvert.SerializeObject(cLI_SWU_RESPONSE, Formatting.Indented);
                    //output = cLI_SWU_RESPONSE.OutputLog(cLI_SWU_RESPONSE, commandLineInput);
                }
                else
                {
                    writelog("FWUpdate_Line 4015");
                    output =  JsonConvert.SerializeObject(cLI_RESPONSE, Formatting.Indented);
                    //output = cLI_RESPONSE.OutputLog(cLI_RESPONSE, commandLineInput);
                }
                if (ret == true)
                {
                    writelog("FWUpdate_Line 4020");
                    return ((int)CLI_ExitCode.success, output);
                }
                else if(somethingError)
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
                    output = JsonConvert.SerializeObject(cLI_SWU_RESPONSE, Formatting.Indented);
                }
                else
                {
                    output = JsonConvert.SerializeObject(cLI_RESPONSE, Formatting.Indented);
                }
                if (ret == true)
                {
                    return ((int)CLI_ExitCode.success, output);
                }
                else
                {
                    Console.WriteLine(output);
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