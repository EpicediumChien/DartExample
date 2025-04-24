using CLI.Plugins.Display;
using DDPM.RemoteManagement.Common.Interfaces;
using DDPM.SA.Common;
using DDPM.SA.Common.Display;
using DDPM.SA.Common.Settings;
using DDPM.SA.Plugins.CMAManager;
using Dell.Client.Framework.Common;
using Dell.Client.Framework.Common.Annotations;
using Dell.Client.Framework.Common.Extensions;
using Dell.Client.Framework.Interfaces;
using DPeMPublic.Common.Enums;
using Microsoft;
using Microsoft.VisualBasic.Logging;
using Microsoft.Win32;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Intrinsics.X86;
using System.Threading;
using System.Threading.Tasks;
using VcpCore.Common;
using Windows.Foundation.Collections;
using static DDPM.RemoteManagement.Common.Interfaces.Params;
using static DDPM.SA.Common.ICLICommandTable;
using static Microsoft.WindowsAPICodePack.Shell.PropertySystem.SystemProperties.System;
using Console = System.Console;
using Convert = System.Convert;
using IDs = DDPM.SA.Common.IDs;
using Task = System.Threading.Tasks.Task;

namespace DDPM.CLI.Plugins.Display
{
    [Plugin(IDs.CLI_Plugin_Display, pluginName, PluginOrderGroupType.Core, Version = pluginVersion)]
    [Descriptor(Description = pluginDescription)]
    [Publisher(Name = publisherCompany, Website = publisherWebsite, Support = publisherSupport)]
    [PublishedInterface(new[] { typeof(ICLIDisplay) })]
    [DependencyKnownTypes(new[] { typeof(ICLIDisplay) })]
    public class CLIDisplayPlugins : BaseAgentPlugin, IDisposableObservable, ICLIDisplay
    {
        #region Private Members

        private const string pluginName = "CLIDisplayPlugin";
        private const string pluginVersion = "1.0.0";
        private const string pluginDescription = "This plugin implements CLI Display Plugin.";
        private const string publisherCompany = "Dell Technologies";
        private const string publisherWebsite = "https://www.dell.com";
        private const string publisherSupport = "This plugin implements CLI Display Plugin.";

        private IAgent _agent;
        public const string PluginLogId = "CLIDisplay";

        //private readonly object _PluginConditionLock = new object();

        private enum log_type
        {
            info = 0,
            error
        }

        private List<MonitorInfo> _AllInfoMonitors;
        private IDeviceManagerSA _devMgr;
        private Dictionary<string, InputInfo> inputSourceList;
        private DeviceHelper _deviceHelper;

        // jim remove 20240607
        ///private List<string> _SupportedColorPreset = new List<string>();

        #endregion Private Members

        #region Constructor

        public CLIDisplayPlugins(IAgent agent) : base(agent, PluginLogId)
        {
            _agent = agent;
            writelog("CLIDisplayPlugins constructor ...");
        }

        #endregion Constructor

        #region interface implementation

        private bool input_param_validation(IDeviceManagerSA devMgr, CommandLineInput commandLineInput, ref CLIEventResult result)
        {
            //check if no monitor connected, direct response no monitor
            _AllInfoMonitors = devMgr.GetMonitors().Result;
            if ((_AllInfoMonitors == null || _AllInfoMonitors.Count == 0) && commandLineInput.TargetType != "APP") // && !commandLineInput.TargetFeature.Contains("NETWORKKVM"))
            {
                CLI_RESPONSE rsp = new CLI_RESPONSE()
                {
                    Command = commandLineInput.Command,
                    TargetFeature = commandLineInput.TargetFeature,
                    // Result = "FAIL",
                    Message = "No devices found",
                };
                result.serialize_Json_response = JsonConvert.SerializeObject(rsp, Formatting.Indented);
                result.ExitCode = (int)CLI_ExitCode.no_monitor_connected;
                return false;
            }
            if (commandLineInput != null && commandLineInput.DeviceIndex.Count > 0) // 20240827 SAST, to fix null at one path.
            {
                foreach (string idx in commandLineInput.DeviceIndex)
                {
                    int nIdx = -1;
                    if (int.TryParse(idx, out nIdx))
                    {
                        if (nIdx < 0)
                        {
                            CLI_RESPONSE rsp = new CLI_RESPONSE()
                            {
                                Command = commandLineInput.Command,
                                TargetFeature = commandLineInput.TargetFeature,
                                Result = "FAIL",
                                Message = $"Index number less than 0: {idx}"
                            };
                            result.serialize_Json_response = JsonConvert.SerializeObject(rsp, Formatting.Indented);
                            result.ExitCode = (int)CLI_ExitCode.input_monitor_index_abnormal;
                            return false;
                        }
                        if (nIdx >= _AllInfoMonitors.Count)
                        {
                            CLI_RESPONSE rsp = new CLI_RESPONSE()
                            {
                                Command = commandLineInput.Command,
                                TargetFeature = commandLineInput.TargetFeature,
                                Result = "FAIL",
                                Message = $"Index number larger than real count of monitor: {idx}"
                            };
                            result.serialize_Json_response = JsonConvert.SerializeObject(rsp, Formatting.Indented);
                            result.ExitCode = (int)CLI_ExitCode.input_monitor_over_count;
                            return false;
                        }
                    }
                    else
                    {
                        CLI_RESPONSE rsp = new CLI_RESPONSE()
                        {
                            Command = commandLineInput.Command,
                            TargetFeature = commandLineInput.TargetFeature,
                            Result = "FAIL",
                            Message = $"Index number abnormal: {idx}"
                        };
                        result.serialize_Json_response = JsonConvert.SerializeObject(rsp, Formatting.Indented);
                        result.ExitCode = (int)CLI_ExitCode.input_monitor_index_abnormal;
                        return false;
                    }
                }
            }
            if (commandLineInput != null && commandLineInput.ServiceTag.Count > 0)
            {
                bool is_match = false;
                foreach (string tag in commandLineInput.ServiceTag)
                {
                    int idx = default;

                    try
                    {
                        idx = _AllInfoMonitors.FindIndex(x => x.edid.ServiceTag.ToUpper().Equals(tag.ToUpper()));
                    }
                    catch (Exception ex)
                    {
                        Trace.WriteLine($"[input_param_validation] FindIndex failed, message: {ex.Message} ");
                        idx = -1;
                    }

                    if (idx < 0)
                    {
                        CLI_RESPONSE rsp = new CLI_RESPONSE()
                        {
                            Command = commandLineInput.Command,
                            TargetFeature = commandLineInput.TargetFeature,
                            Result = "FAIL",
                            Message = $"Invalid Service Tag: {tag}"
                        };
                        foreach (MonitorInfo monitor in _AllInfoMonitors)
                            Trace.WriteLine($"Service tag in Monitor {monitor.edid.ServiceTag} ");
                        result.serialize_Json_response = JsonConvert.SerializeObject(rsp, Formatting.Indented);
                        result.ExitCode = (int)CLI_ExitCode.invalid_servicetag;
                    }
                    else
                        is_match = true;
                }
                if (!is_match)
                    return false;
            }

            if (commandLineInput != null && commandLineInput.Model.Count > 0)
            {
                bool is_match = false;
                foreach (string model in commandLineInput.Model)
                {
                    int idx = default;

                    try
                    {
                        idx = _AllInfoMonitors.FindIndex(x => x.modelName.ToUpper().Equals(model.ToUpper()));
                    }
                    catch (Exception ex)
                    {
                        Trace.WriteLine($"[input_param_validation] FindIndex failed, message: {ex.Message} ");
                        idx = -1;
                    }

                    if (idx < 0)
                    {
                        CLI_RESPONSE rsp = new CLI_RESPONSE()
                        {
                            Command = commandLineInput.Command,
                            TargetFeature = commandLineInput.TargetFeature,
                            Result = "FAIL",
                            Message = $"Invalid Model Name: {model}"
                        };
                        foreach (MonitorInfo monitor in _AllInfoMonitors)
                            Trace.WriteLine($"Model Name in Monitor {monitor.modelName} ");
                        result.serialize_Json_response = JsonConvert.SerializeObject(rsp, Formatting.Indented);
                        result.ExitCode = (int)CLI_ExitCode.invalid_servicetag;
                    }
                    else
                        is_match = true;
                }
                if (!is_match)
                    return false;
            }
            return true;
        }

        public CLIEventResult SetCommandArgs(CLIEventArgs input, IDeviceManagerSA devMgr)
        {
            _devMgr = devMgr;
            CommandLineInput commandLineInput = input.commandLineInput;

            CLIEventResult result = new CLIEventResult();
            result.command_guid_string = input.command_guid_string;
            result.ticket = DateTime.Now;

            if (commandLineInput.TargetType.Equals("display", StringComparison.OrdinalIgnoreCase) && !input_param_validation(devMgr, commandLineInput, ref result))
            {
                return result;
            }

            if (commandLineInput.Options.Count > 0 &&
                commandLineInput.Options[0].Option_Value.Equals("Display", StringComparison.OrdinalIgnoreCase) &&
                !input_param_validation(devMgr, commandLineInput, ref result))
            {
                return result;
            }

            switch (commandLineInput.TargetFeature)
            {
                case "CONNECTEDDEVICES":
                    {
                        if (commandLineInput.TargetType == "APP")
                        {
                            int exitcode = 0;
                            result.serialize_Json_response = ConnectedDevicesX(commandLineInput, devMgr, ref exitcode);
                            writelog($"ConnectedDevice Result: {result.serialize_Json_response}");
                            result.ExitCode = exitcode;
                        }
                        else
                        {
                            CLI_RESPONSE rsp_device = new CLI_RESPONSE()
                            {
                                Command = commandLineInput.Command,
                                TargetFeature = commandLineInput.TargetFeature,
                                Result = "Un-supported feature",
                                Message = "Un-supported feature"
                            };
                            result.serialize_Json_response = JsonConvert.SerializeObject(rsp_device, Formatting.Indented);
                            result.ExitCode = (int)CLI_ExitCode.unknow_command;
                            return result;
                        }
                    }
                    break;

                case "DETECTMONITORS":
                    {
                        var ret = DetectMonitorsX(commandLineInput, devMgr);
                        result.serialize_Json_response = ret.result;
                        result.ExitCode = ret.code;
                        if (ret.code != 0)
                            return result;
                    }
                    break;

                case "BRIGHTNESSLEVEL"://change from brightness to BrightnessLevel to align with spec
                    {
                        var ret = BrightnessX(commandLineInput, devMgr);
                        result.serialize_Json_response = ret.result;
                        result.ExitCode = ret.code;
                        if (ret.code != 0)
                            return result;
                    }
                    break;
                /*case "LUMINUS":
                    {
                        var ret = LuminusX(commandLineInput, devMgr);
                        result.serialize_Json_response = ret.result;
                        result.ExitCode = ret.code;
                        if (ret.code != 0)
                            return result;
                    }
                    break;*/
                case "CONTRASTLEVEL"://change from contrast to ContrastLevel to align with spec
                    {
                        var ret = ContrastX(commandLineInput, devMgr);
                        result.serialize_Json_response = ret.result;
                        result.ExitCode = ret.code;
                        if (ret.code != 0)
                            return result;
                    }
                    break;

                case "EDID":
                    {
                        var ret = EDIDX(commandLineInput, devMgr);
                        result.serialize_Json_response = ret.result;
                        result.ExitCode = ret.code;
                        if (ret.code != 0)
                            return result;
                    }
                    break;

                case "DECODEDEDID":
                    {
                        var ret = DECODEDEDIDX(commandLineInput, devMgr);
                        result.serialize_Json_response = ret.result;
                        result.ExitCode = ret.code;
                        if (ret.code != 0)
                            return result;
                    }
                    break;

                case "FWVERSION":
                    {
                        var ret = FWVersionX(commandLineInput, devMgr);
                        result.serialize_Json_response = ret.result;
                        result.ExitCode = ret.code;
                        if (ret.code != 0)
                            return result;
                    }
                    break;

                /*
                // ADD @ Stephen for fwupdate
                case "FIRMWAREUPDATE":
                    {
                        var ret = FWUpdateX(commandLineInput, devMgr);
                        result.serialize_Json_response = ret.result;
                        result.ExitCode = ret.code;
                        if (ret.code != 0)
                            return result;
                    }
                    break;
                */
                case "ACTIVEINPUTSOURCE":
                    {
                        var ret = InputSource(devMgr, commandLineInput).Result;
                        result.serialize_Json_response = ret.result;
                        result.ExitCode = ret.code;
                        if (ret.code != 0)
                            return result;
                    }
                    break;

                case "INPUTSOURCELIST":
                    {
                        var ret = InputSourceList(devMgr, commandLineInput).Result;
                        result.serialize_Json_response = ret.result;
                        result.ExitCode = ret.code;
                        if (ret.code != 0)
                            return result;
                    }
                    break;

                #region display properties

                case "OPTIMALRESOLUTION":
                case "HDR":
                case "USBCPRIORITIZATION":
                case "RESOLUTION":
                case "REFRESHRATE":
                case "RESOLUTIONREFRESHRATE":
                case "ORIENTATION":
                case "CURRENTRESOLUTIONREFRESHRATE"://"CURRENTDISPLAYPROPERTIES":
                case "ALLRESOLUTIONREFRESHRATE":
                case "LOCKROTATE":
                case "ROTATEOSDMENU":
                    var value = SetDisplayProperties(_devMgr, commandLineInput);
                    result.serialize_Json_response = value.result;
                    result.ExitCode = value.code;
                    return result;

                #endregion display properties

                case "COLORPRESET":
                    if (commandLineInput.Command.Equals("GET"))
                    {
                        var ret = ReadColorPreset(devMgr, commandLineInput.Command, commandLineInput.DeviceIndex, commandLineInput.ServiceTag, commandLineInput.Model, "").Result;
                        result.ExitCode = ret.code;
                        result.serialize_Json_response = ret.result;
                    }
                    if (commandLineInput.Command.Equals("SET"))
                    {
                        int exitcode = 0;
                        string output = string.Empty;
                        for (int j = 0; j < commandLineInput.Options.Count; j++)//呼叫方法名稱
                        {
                            if (commandLineInput.Options[j].Option_Name.ToUpper().Equals("VALUE")) //ex: /set -name=Display.Brightness -index=[0] -value=60
                            {
                                var ret = ReadColorPreset(devMgr, commandLineInput.Command, commandLineInput.DeviceIndex, commandLineInput.ServiceTag, commandLineInput.Model, commandLineInput.Options[j].Option_Value).Result;
                                exitcode = ret.code;
                                output += "\n" + ret.result;
                            }
                            if (exitcode != 0)
                            {
                                result.serialize_Json_response = output;
                                result.ExitCode = exitcode;
                                return result;
                            }
                        }
                        result.serialize_Json_response = output;
                        result.ExitCode = exitcode;
                    }
                    break;

                case "AUTOBRIGHTNESS":
                case "AUTOBRIGHTNESSRANGELEVEL"://Mark 0723
                case "AUTOCOLORTEMP":
                case "PRIMARYMONITORSYNC":
                case "MULTIMONITORSYNC":
                    var tmp = ProcessAlsFunction(devMgr, commandLineInput);
                    result.ExitCode = tmp.code;
                    result.serialize_Json_response = tmp.result;
                    return result;
                //break;
                case "INAPPAUTOBRITEMP":
                    var tmpInAppAutoBriTemp = ProcessAlsFunction(devMgr, commandLineInput);
                    result.ExitCode = tmpInAppAutoBriTemp.code;
                    result.serialize_Json_response = tmpInAppAutoBriTemp.result;
                    return result;
                case "COLORPROFILE":
                    if (commandLineInput.Command.Equals("GET"))
                    {
                        var ret = ActiveColorProfile(devMgr, commandLineInput.Command, commandLineInput.DeviceIndex, commandLineInput.ServiceTag, commandLineInput.Model, "").Result;
                        result.ExitCode = ret.code;
                        result.serialize_Json_response = ret.result;
                    }
                    break;

                case "ICCPROFILEBASEDONCOLORPRESET"://colormanagement --bymonitor
                    if (commandLineInput.Command.Equals("SET"))
                    {
                        int exitcode = 0;
                        string output = string.Empty;
                        for (int j = 0; j < commandLineInput.Options.Count; j++)//呼叫方法名稱
                        {
                            if (commandLineInput.Options[j].Option_Name.ToUpper().Equals("VALUE")) //ex: /set -name=Display.Brightness -index=[0] -value=60
                            {
                                var ret = ActiveColorProfile(devMgr, commandLineInput.Command, commandLineInput.DeviceIndex, commandLineInput.ServiceTag, commandLineInput.Model, commandLineInput.Options[j].Option_Value).Result;
                                exitcode = ret.code;
                                output += "\n" + ret.result;
                            }
                            if (exitcode != 0)
                            {
                                result.serialize_Json_response = output;
                                result.ExitCode = exitcode;
                                return result;
                            }
                        }
                        result.serialize_Json_response = output;
                        result.ExitCode = exitcode;
                    }
                    break;

                case "COLORPRESETBASEDONICCPROFILE"://colormanagement --byhost
                    if (commandLineInput.Command.Equals("SET"))
                    {
                        int exitcode = 0;
                        string output = string.Empty;
                        for (int j = 0; j < commandLineInput.Options.Count; j++)//呼叫方法名稱
                        {
                            if (commandLineInput.Options[j].Option_Name.ToUpper().Equals("VALUE")) //ex: /set -name=Display.Brightness -index=[0] -value=60
                            {
                                var ret = ActiveColorPreset(devMgr, commandLineInput.Command, commandLineInput.DeviceIndex, commandLineInput.ServiceTag, commandLineInput.Model, commandLineInput.Options[j].Option_Value).Result;
                                exitcode = ret.code;
                                output += "\n" + ret.result;
                            }
                            if (exitcode != 0)
                            {
                                result.serialize_Json_response = output;
                                result.ExitCode = exitcode;
                                return result;
                            }
                        }
                        result.serialize_Json_response = output;
                        result.ExitCode = exitcode;
                    }
                    break;

                case "COLORMANAGEMENT":
                    if (commandLineInput.Command.Equals("GET"))
                    {
                        var ret = ColorManagement(devMgr, commandLineInput).Result;
                        result.ExitCode = ret.code;
                        result.serialize_Json_response = ret.result;
                    }
                    if (commandLineInput.Command.Equals("SET"))
                    {
                        int exitcode = 0;
                        string output = string.Empty;
                        for (int j = 0; j < commandLineInput.Options.Count; j++)//呼叫方法名稱
                        {
                            if (commandLineInput.Options[j].Option_Name.ToUpper().Equals("VALUE")) //ex: /set -name=Display.Brightness -index=[0] -value=60
                            {
                                var ret = ColorManagement(devMgr, commandLineInput).Result;
                                exitcode = ret.code;
                                output += "\n" + ret.result;
                            }
                            if (exitcode != 0)
                            {
                                result.serialize_Json_response = output;
                                result.ExitCode = exitcode;
                                return result;
                            }
                        }
                        result.serialize_Json_response = output;
                        result.ExitCode = exitcode;
                    }
                    break;

                case "RESTOREFACTORYDEFAULTS":
                    if (commandLineInput.Command.Equals("SET"))
                    {
                        var ret = RestoreFactoryDefaults(devMgr, commandLineInput.Command, commandLineInput.DeviceIndex, commandLineInput.ServiceTag, commandLineInput.Model, "").Result;
                        result.ExitCode = ret.code;
                        result.serialize_Json_response = ret.result;
                    }
                    else
                    {
                        CLI_RESPONSE rsp_fac = new CLI_RESPONSE()
                        {
                            Command = commandLineInput.Command,
                            TargetFeature = commandLineInput.TargetFeature,
                            Result = "Un-supported feature",
                            Message = "Un-supported feature"
                        };
                        result.serialize_Json_response = JsonConvert.SerializeObject(rsp_fac, Formatting.Indented);
                        result.ExitCode = (int)CLI_ExitCode.unknow_command;
                        return result;
                    }
                    break;

                case "RESTORELEVELDEFAULTS":
                    if (commandLineInput.Command.Equals("SET"))
                    {
                        var ret = RestoreLevelDefaults(devMgr, commandLineInput.Command, commandLineInput.DeviceIndex, commandLineInput.ServiceTag, commandLineInput.Model, "").Result;
                        result.ExitCode = ret.code;
                        result.serialize_Json_response = ret.result;
                    }
                    else
                    {
                        CLI_RESPONSE rsp_level = new CLI_RESPONSE()
                        {
                            Command = commandLineInput.Command,
                            TargetFeature = commandLineInput.TargetFeature,
                            Result = "Un-supported feature",
                            Message = "Un-supported feature"
                        };
                        result.serialize_Json_response = JsonConvert.SerializeObject(rsp_level, Formatting.Indented);
                        result.ExitCode = (int)CLI_ExitCode.unknow_command;
                        return result;
                    }
                    break;

                case "RESTORECOLORDEFAULTS":
                    if (commandLineInput.Command.Equals("SET"))
                    {
                        var ret = RestoreColorDefaults(devMgr, commandLineInput.Command, commandLineInput.DeviceIndex, commandLineInput.ServiceTag, commandLineInput.Model, "").Result;
                        result.ExitCode = ret.code;
                        result.serialize_Json_response = ret.result;
                    }
                    else
                    {
                        CLI_RESPONSE rsp_color = new CLI_RESPONSE()
                        {
                            Command = commandLineInput.Command,
                            TargetFeature = commandLineInput.TargetFeature,
                            Result = "Un-supported feature",
                            Message = "Un-supported feature"
                        };
                        result.serialize_Json_response = JsonConvert.SerializeObject(rsp_color, Formatting.Indented);
                        result.ExitCode = (int)CLI_ExitCode.unknow_command;
                        return result;
                    }
                    break;

                case "OSDACCESS":
                    if (commandLineInput.Command.Equals("GET"))
                    {
                        var ret = OSD(devMgr, commandLineInput.Command, commandLineInput.DeviceIndex, commandLineInput.ServiceTag, commandLineInput.Model, "").Result;
                        result.ExitCode = ret.code;
                        result.serialize_Json_response = ret.result;
                    }
                    if (commandLineInput.Command.Equals("SET"))
                    {
                        int exitcode = 0;
                        string output = string.Empty;
                        for (int j = 0; j < commandLineInput.Options.Count; j++)//呼叫方法名稱
                        {
                            if (commandLineInput.Options[j].Option_Name.ToUpper().Equals("VALUE")) //ex: /set -name=Display.Brightness -index=[0] -value=60
                            {
                                var ret = OSD(devMgr, commandLineInput.Command, commandLineInput.DeviceIndex, commandLineInput.ServiceTag, commandLineInput.Model, commandLineInput.Options[j].Option_Value).Result;
                                exitcode = ret.code;
                                output += "\n" + ret.result;
                            }
                            if (exitcode != 0)
                            {
                                result.serialize_Json_response = output;
                                result.ExitCode = exitcode;
                                return result;
                            }
                        }
                        result.serialize_Json_response = output;
                        result.ExitCode = exitcode;
                    }
                    break;

                #region PxP (Robert_Lin 2024-6-13)

                case "PXP":
                case "ASSIGNSUBINPUT":
                case "SUBINPUT":
                case "SWAPVIDEO":
                case "SWAPUSB":
                case "PXPZOOM":
                    writelog($"{commandLineInput.TargetFeature} CLI_Pxp entry");
                    var pxp = CLI_Pxp(devMgr, commandLineInput).Result;
                    writelog($"{commandLineInput.TargetFeature} CLI_Pxp exit");
                    result.ExitCode = pxp.code;
                    result.serialize_Json_response = pxp.result;
                    return result;

                #endregion PxP (Robert_Lin 2024-6-13)

                case "POWERNAP":
                    {
                        var powernap = PowerNapX(devMgr, commandLineInput);
                        result.ExitCode = powernap.code;
                        result.serialize_Json_response = powernap.result;
                    };
                    break;

                case "DEVICEDATA":
                    {
                        if (commandLineInput.TargetType == "APP")
                        {
                            var ret = GetDeviceData(devMgr, commandLineInput);
                            result.ExitCode = ret.code;
                            result.serialize_Json_response = ret.result;
                        }
                        else
                        {
                            CLI_RESPONSE rsp_device = new CLI_RESPONSE()
                            {
                                Command = commandLineInput.Command,
                                TargetFeature = commandLineInput.TargetFeature,
                                Result = "Un-supported feature",
                                Message = "Un-supported feature"
                            };
                            result.serialize_Json_response = JsonConvert.SerializeObject(rsp_device, Formatting.Indented);
                            result.ExitCode = (int)CLI_ExitCode.unknow_command;
                            return result;
                        }

                    }
                    break;

                case "SCREENNOTIFICATION":
                    {
                        if (commandLineInput.TargetType == "APP")
                        {
                            var ret = ScreenNotificationx(devMgr, commandLineInput);
                            result.ExitCode = ret.code;
                            result.serialize_Json_response = ret.result;
                        }
                        else
                        {
                            CLI_RESPONSE rsp_device = new CLI_RESPONSE()
                            {
                                Command = commandLineInput.Command,
                                TargetFeature = commandLineInput.TargetFeature,
                                Result = "Un-supported feature",
                                Message = "Un-supported feature"
                            };
                            result.serialize_Json_response = JsonConvert.SerializeObject(rsp_device, Formatting.Indented);
                            result.ExitCode = (int)CLI_ExitCode.unknow_command;
                            return result;
                        }

                    }
                    break;

                //case "ENERGYSAVER":
                //    {
                //        var energysaver = EnergysaverX(devMgr, commandLineInput);
                //        result.ExitCode = energysaver.code;
                //        result.serialize_Json_response = energysaver.result;
                //    };
                //    break;

                case "CAPABILITIESSTRING":
                    {
                        var ret = Getcapabilitystringx(devMgr, commandLineInput);
                        result.ExitCode = ret.code;
                        result.serialize_Json_response = ret.result;
                        return result;
                    }
                    break;

                case "AUTOCOLORPRESET":
                    {
                        var autocolorpreset = AutocolorpresetX(devMgr, commandLineInput);
                        result.ExitCode = autocolorpreset.code;
                        result.serialize_Json_response = autocolorpreset.result;
                    };
                    break;

                case "OSDLANGUAGE":
                    {
                        var ret = OSDLanguageX(devMgr, commandLineInput);
                        result.ExitCode = ret.code;
                        result.serialize_Json_response = ret.result;
                    }
                    break;

                case "MONITORCOUNT":
                    {
                        var ret = GetMonitorCount(devMgr, commandLineInput);
                        result.ExitCode = ret.code;
                        result.serialize_Json_response = ret.result;
                    }
                    break;

                case "DIAGNOSTICSREPORT":
                    {
                        var ret = GetDiagnosticReport(devMgr, commandLineInput);
                        result.ExitCode = ret.code;
                        result.serialize_Json_response = ret.result;
                    }
                    break;

                case "ACTIVEHOURS":
                    {
                        var ret = ActiveHours(devMgr, commandLineInput);
                        result.ExitCode = ret.code;
                        result.serialize_Json_response = ret.result;
                    }
                    break;

                case "DEVICECONFIGURATION":
                    {
                        var ret = ApplyConfigurationX(devMgr, commandLineInput);
                        result.ExitCode = ret.code;
                        result.serialize_Json_response = ret.result;
                    }
                    break;

                case "POWERSETTING":
                    {
                        var ret = SetPowerSetting(devMgr, commandLineInput);
                        result.ExitCode = ret.code;
                        result.serialize_Json_response = ret.result;
                    }
                    break;

                case "SPEAKERMICROPHONE":
                case "SPEAKERVOLUME":
                case "MICROPHONE":
                    {
                        var ret = Mute_UnmuteX(devMgr, commandLineInput);
                        result.ExitCode = ret.code;
                        result.serialize_Json_response = ret.result;
                    }
                    break;

                case "ADVANCEDCONTROL":
                    {
                        var ret = AdvancedcontrolX(devMgr, commandLineInput);
                        result.ExitCode = ret.code;
                        result.serialize_Json_response = ret.result;
                    }
                    break;

                case "ACTIVEHOUR":
                    {
                        var ret = ActivehourX(devMgr, commandLineInput);
                        result.ExitCode = ret.code;
                        result.serialize_Json_response = ret.result;
                    }
                    break;
                case "EASYARRANGELAYOUT":
                    {
                        var ret = EasyarrangeX(devMgr, commandLineInput);
                        result.ExitCode = ret.code;
                        result.serialize_Json_response = ret.result;
                    }
                    break;
                case "SETEA":
                    {
                        var ret = GetSetEA(devMgr, commandLineInput).Result;
                        result.ExitCode = ret.code;
                        result.serialize_Json_response = ret.result;
                    }
                    break;
                case "INAPPUSBKVM":
                    {
                        var ret = InAppUSBkvmx(devMgr, commandLineInput);
                        result.ExitCode = ret.code;
                        result.serialize_Json_response = ret.result;
                    }
                    break;
                case "EXPORTSETTINGS":
                case "IMPORTSETTINGS":
                    {
                        var ret = ExportSettingsx(devMgr, commandLineInput);
                        result.ExitCode = ret.code;
                        result.serialize_Json_response = ret.result;
                    }
                    break;

                default:
                    CLI_RESPONSE rsp = new CLI_RESPONSE()
                    {
                        Command = commandLineInput.Command,
                        TargetFeature = commandLineInput.TargetFeature,
                        Result = "Un-supported feature",
                        Message = "Un-supported feature"
                    };
                    result.serialize_Json_response = JsonConvert.SerializeObject(rsp, Formatting.Indented);
                    result.ExitCode = (int)CLI_ExitCode.unknow_command;
                    return result;
            }
            return result;
        }

        #endregion interface implementation

        #region Private methods

        private (int code, string result) ProcessAlsFunction(IDeviceManagerSA devMgr, CommandLineInput input)
        {
            if (devMgr == null)
            {
                return WriteALSResponse(_AllInfoMonitors, _AllInfoMonitors.Count.ToString(), input, CLI_ExitCode.null_device_manager, false, "");
            }

            if (_AllInfoMonitors == null)
                _AllInfoMonitors = devMgr.GetMonitors().Result;

            if (_AllInfoMonitors.Count == 0)
            {
                return WriteALSResponse(_AllInfoMonitors, _AllInfoMonitors.Count.ToString(), input, CLI_ExitCode.no_monitor_connected, false, "");
            }
            if (input == null)
            {
                return WriteALSResponse(_AllInfoMonitors, _AllInfoMonitors.Count.ToString(), input, CLI_ExitCode.unknow_command, false, "Null Input");
            }
            List<string> index = new List<string>();
            if (input.DeviceIndex.Count != 0)
                index.AddRange(input.DeviceIndex);
            else if (input.ServiceTag.Count != 0)
            {
                for (int i = 0; i < _AllInfoMonitors.Count; i++)
                {
                    if (input.ServiceTag.FindIndex(x => x.ToUpper().Trim().Equals(_AllInfoMonitors[i].edid.ServiceTag.ToUpper().Trim())) >= 0)
                    {
                        index.Add(i.ToString());
                    }
                }
            }
            else if (input.Model.Count != 0)
            {
                for (int i = 0; i < _AllInfoMonitors.Count; i++)
                {
                    if (input.Model.FindIndex(x => x.ToUpper().Trim().Equals(_AllInfoMonitors[i].edid.ModelName.ToUpper().Trim())) >= 0)
                    {
                        index.Add(i.ToString());
                    }
                }
            }
            else //do all
            {
                for (int i = 0; i < _AllInfoMonitors.Count; i++)
                    index.Add(i.ToString()); ;
            }

            int exit = 0;
            //start to get/set
            string output = string.Empty;
            for (int i = 0; i < index.Count; i++)
            {
                string idx = index[i];

                if (_AllInfoMonitors.Count <= int.Parse(idx))
                {
                    return WriteALSResponse(_AllInfoMonitors, idx, input, CLI_ExitCode.unknow_command, false, "Index out of range");
                }

                if (input.Command.Equals("GET"))
                {
                    writelog(input.TargetFeature + " get entry");
                    var ret = GetALSPropertiesAsync(devMgr, input, idx.ToString());
                    output += "\n" + ret.result;
                    if (ret.code != 0)
                        exit = ret.code;
                }
                else if (input.Command.Equals("SET"))
                {
                    writelog(input.TargetFeature + " set entry");
                    foreach (CommandType_Option opt in input.Options)
                    {
                        if (opt.Option_Name.ToUpper().Equals("VALUE"))
                        {
                            var ret = SetALSProperties(devMgr, input, idx.ToString(), opt.Option_Value);
                            output += "\n" + ret.result;
                            if (ret.code != 0)
                                exit = ret.code;
                        }
                    }
                }
            }

            writelog(input.TargetFeature + " Return value{output}");
            return (exit, output);
        }

        private string ConnectedDevicesX(CommandLineInput commandLineInput, IDeviceManagerSA devMgr, ref int exitcode)
        {
            var result = ConnectedDevices(devMgr, commandLineInput.Command, commandLineInput.DeviceIndex, commandLineInput.ServiceTag, commandLineInput.Model, commandLineInput).Result;
            exitcode = result.code;
            return result.result;
        }

        public string FirstCharSubstring(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return string.Empty;
            }
            return $"{input[0].ToString().ToUpper()}{input.Substring(1)}";
        }

        private (int code, string result) DDPMSettingsNullResponse(string command, string targetFeature)
        {
            CLI_RESPONSE cli_Response = new CLI_RESPONSE();
            cli_Response.Command = command;
            cli_Response.TargetFeature = targetFeature;
            cli_Response.Result = "FAIL";
            cli_Response.Message = "Failed to get DDPMSettings";
            Console.WriteLine(JsonConvert.SerializeObject(cli_Response, Formatting.Indented));
            return ((int)CLI_ExitCode.functional_error, JsonConvert.SerializeObject(cli_Response, Formatting.Indented));
        }

        private string GetPeripheralResponse(int index, string output, DeviceInfo g)
        {
            // modified @ 20241217 stephen: make string follow json format
            output += "{";
            output += $"\n  \"Device\": \"{g.LogicalDeviceType}\"";
            output += "}";
            PeripheralResponse cli_Response2 = new PeripheralResponse(index, g);
            cli_Response2.Connectiontype = (g.PhysicalDeviceType.ToString().Contains("Dongle") || g.PhysicalDeviceType.ToString().Contains("Bluetooth") || g.PhysicalDeviceType.ToString().Contains("Pen")) ? "Wireless" : "Wired";

            switch (g.LogicalDeviceType)
            {
                case "LogicalHeadset":
                    cli_Response2.SerialNumber = _devMgr.GetHeadsetSerialNumberAsync(g.ID.ToString()).Result ?? "N/A";
                    cli_Response2.Connectiontype = get_headsetconnection_type(_devMgr.GetConnectionTypeAsync(g.ID.ToString()).Result);
                    break;
                case "LogicalWebcam":
                    cli_Response2.SerialNumber = _devMgr.GetWebcamSerialNumber(g.ID.ToString()).Result ?? "N/A";
                    break;
                case "LogicalDock"://Elsa 20250307 add for ConnectedDevices get Dock servicetag
                    cli_Response2.ServiceTag = _devMgr.GetDockServiceTagForDock(g.ID.ToString()).Result ?? "N/A";
                    break;
            }
            output += "\n" + JsonConvert.SerializeObject(cli_Response2, Formatting.Indented);

            return output;
        }

        private PeripheralResponse GetDeviceDataPeripheralResponse(int index, DeviceInfo device)
        {
            writelog("GetDeviceDataPeripheralResponse entry");
            var guid = device.ID.ToString();

            switch (device.LogicalDeviceType)
            {
                case "LogicalWebcam":
                    writelog("LogicalWebcam entry");
                    var webcam = new DeviceDataWebcamResponse(index, device);
                    writelog("_devMgr.GetWebcamSerialNumber entry");
                    webcam.SerialNumber = _devMgr.GetWebcamSerialNumber(guid).Result ?? "N/A";

                    writelog("_devMgr.GetIsPropertyFOVSupportedByDTP entry");
                    if (_devMgr.GetIsPropertyFOVSupportedByDTP(guid).Result)
                    {
                        writelog("_devMgr.GetFieldOfView entry");
                        webcam.FieldOfView = _devMgr.GetFieldOfView(guid).Result.ToString();
                    }
                    writelog("_devMgr.GetIsPropertyHDRSupported entry");
                    if (_devMgr.GetIsPropertyHDRSupported(guid).Result)
                    {
                        writelog("_devMgr.GetIsHDROn entry");
                        webcam.HDR = _devMgr.GetIsHDROn(guid).Result ? "ON" : "OFF";
                    }
                    writelog("_devMgr.GetIsPropertyAntiFlickerSupported entry");
                    if (_devMgr.GetIsPropertyAntiFlickerSupported(guid).Result)
                    {
                        writelog("_devMgr.GetAntiFlicker entry");
                        webcam.AntiFlicker = _devMgr.GetAntiFlicker(guid).Result.ToString();
                        switch (webcam.AntiFlicker)
                        {
                            case "1":
                                webcam.AntiFlicker = "50";
                                break;
                            case "2":
                                webcam.AntiFlicker = "60";
                                break;
                            default:
                                break;
                        }
                    }
                    writelog("device.IsMicEnumerationSupported entry");
                    if (device.IsMicEnumerationSupported)
                    {
                        writelog("device.IsMicEnumerationOn set entry");
                        webcam.MicSwitch = device.IsMicEnumerationOn ? "ON" : "OFF";
                    }
                    writelog("_devMgr.GetIsPropertyAutoFramingSupported entry");
                    if (_devMgr.GetIsPropertyAutoFramingSupported(guid).Result)
                    {
                        writelog("_devMgr.GetIsAutoFramingOn entry");
                        webcam.AIAutoFraming = _devMgr.GetIsAutoFramingOn(guid).Result ? "ON" : "OFF";
                    }
                    writelog("device.IsESISupported entry");
                    if (device.IsESISupported)
                    {
                        writelog("_devMgr.GetIsProximitySensorEnable entry");
                        webcam.PresenceDetection = _devMgr.GetIsProximitySensorEnable(guid).Result ? "ON" : "OFF";
                    }

                    return webcam;
                case "LogicalHeadset":
                    writelog("LogicalHeadset entry");
                    var audio = new DeviceDataAudioResponse(index, device);
                    writelog("_devMgr.GetHeadsetSerialNumber entry");
                    audio.SerialNumber = _devMgr.GetHeadsetSerialNumberAsync(guid).Result ?? "N/A";
                    writelog("_devMgr.GetConnectionTypeAsync entry");
                    audio.Connectiontype = get_headsetconnection_type(_devMgr.GetConnectionTypeAsync(guid).Result);
                    writelog("_devMgr.GetIsANCSupportedAsync entry");
                    if (_devMgr.GetIsANCSupportedAsync(guid).Result)
                    {
                        writelog("device.AncMode set entry");
                        audio.ANCMode = device.AncMode > 0 ? "ON" : "OFF";
                    }
                    writelog("_devMgr.GetIsMicNoiseCancellationSupportedAsync entry");
                    if (_devMgr.GetIsMicNoiseCancellationSupportedAsync(guid).Result)
                    {
                        writelog("device.MicNoiseCancellation set entry");
                        audio.MicNoiseCancellation = device.MicNoiseCancellation ? "ON" : "OFF";
                    }
                    writelog("_devMgr.GetIsWearDetectionSupportedAsync entry");
                    if (_devMgr.GetIsWearDetectionSupportedAsync(guid).Result)
                    {
                        writelog("device.WearDetection set entry");
                        audio.WearDetection = device.WearDetection > 0 ? "ON" : "OFF";
                    }

                    return audio;
                case "LogicalAirAudio":
                    if (GlobalDefinitions.isSupport210)
                    {
                        writelog("LogicalAirAudio entry");
                        var airAudio = new DeviceDataAudioResponse(index, device);
                        writelog("_devMgr.GetAirAudioSerialNumberAsync entry");
                        airAudio.SerialNumber = _devMgr.GetAirAudioSerialNumberAsync(guid).Result ?? "N/A";
                        writelog("_devMgr.GetAirAudioIsMicNoiseCancellationSupportedAsync entry");
                        if (_devMgr.GetAirAudioIsMicNoiseCancellationSupportedAsync(guid).Result)
                        {
                            writelog("_devMgr.GetAirAudioMicNoiseCancellationAsync entry");
                            airAudio.MicNoiseCancellation = _devMgr.GetAirAudioMicNoiseCancellationAsync(guid).Result ? "ON" : "OFF";
                        }
                        return airAudio;
                    }
                    return default;
                    break;
                case "LogicalKeyboard":
                    return new DeviceDataKeyboardResponse(index, device);
                case "LogicalMouse":
                    return new DeviceDataMouseResponse(index, device);
                case "LogicalPen":
                    return new DeviceDataPenResponse(index, device);
                case "LogicalDock"://20250306Elsa add for dock get servicetag
                    writelog("LogicalDock entry");
                    var dock = new DeviceDataDockResponse(index, device);
                    writelog("_devMgr.GetDockServiceTagForDock entry");
                    dock.ServiceTag = _devMgr.GetDockServiceTagForDock(guid).Result ?? "N/A";
                    return dock;
                //return new DeviceDataDockResponse(index, device);
                default:
                    return new PeripheralResponse(index, device);
            }
        }

        private async Task<(int code, string result)> ConnectedDevices(IDeviceManagerSA devMgr, string type, List<string> index, List<string> serviceTag, List<string> model, CommandLineInput commandLineInput, string value = "")
        {
            ConnectedDevices G_ConnectedDevices_RESPONSE = new ConnectedDevices();
            G_ConnectedDevices_RESPONSE.Message = "No devices found";

            //if (devMgr == null)
            //{
            //    writelog("FWVersion: Null IDeviceManagerSA");
            //    return (int)CLI_ExitCode.null_device_manager;
            //}

            //if (_AllInfoMonitors == null)
            writelog($"Connecteddevices IN = {commandLineInput}");
            writelog($"Connecteddevices from CMA = {commandLineInput.fromcma}");
            _AllInfoMonitors = await devMgr.GetMonitors();
            writelog($"Connecteddevices _AllInfoMonitors count  = {_AllInfoMonitors.Count}");
            string output = string.Empty;
            string output_2 = string.Empty;
            _deviceHelper = new DeviceHelper
            {
                deviceInfo = new List<DeviceInfo>()
            };

            List<DeviceInfo> _deviceinfo = null;
            _deviceinfo = _devMgr.GetDevices().Result.deviceInfo;

            int index_per = 0;
            bool recode_per = false;

            if (type == "GET")
            {
                if (commandLineInput.Options.Count > 0)
                {
                    switch (commandLineInput.Options[0].Option_Value.ToUpper())
                    {
                        case "DISPLAY":
                            if (index.Count == 0 && serviceTag.Count == 0 && model.Count == 0)
                            {
                                writelog($"Connecteddevices DISPLAY IN");
                                bool IsFailhappened = false;
                                foreach (MonitorInfo monitor in _AllInfoMonitors)
                                {
                                    writelog($"Connecteddevices DISPLAY Index = {change_0base_to_1base((monitor.Index).ToString())}");
                                    writelog($"Connecteddevices DISPLAY FWversion = {monitor.FwVersion}");
                                    G_ConnectedDevices_RESPONSE = new ConnectedDevices();
                                    G_ConnectedDevices_RESPONSE.Model = monitor.edid.ModelName;
                                    G_ConnectedDevices_RESPONSE.SerialNumber = monitor.edid.SerialNumber;
                                    G_ConnectedDevices_RESPONSE.Index = change_0base_to_1base((monitor.Index).ToString());
                                    G_ConnectedDevices_RESPONSE.ServiceTag = monitor.edid.ServiceTag;
                                    G_ConnectedDevices_RESPONSE.FirmwareVersion = monitor.FwVersion;

                                    if (string.IsNullOrWhiteSpace(monitor.FwVersion))
                                    {
                                        writelog("FWVersion: IsNullOrWhiteSpace");
                                        G_ConnectedDevices_RESPONSE.PID = "N/A";
                                        G_ConnectedDevices_RESPONSE.Result = "Fail";
                                        G_ConnectedDevices_RESPONSE.Message = "Fail_VCPCapability";
                                        System.Console.WriteLine(JsonConvert.SerializeObject(G_ConnectedDevices_RESPONSE, Formatting.Indented));
                                        output += "\n" + JsonConvert.SerializeObject(G_ConnectedDevices_RESPONSE, Formatting.Indented);
                                        IsFailhappened = true;
                                    }
                                    else
                                    {
                                        writelog($"Connecteddevices DISPLAY complete index = {change_0base_to_1base((monitor.Index).ToString())}");
                                        G_ConnectedDevices_RESPONSE.PID = monitor.edid.PID.ToString();
                                        G_ConnectedDevices_RESPONSE.Result = "Pass";
                                        index_per = int.Parse(change_0base_to_1base((monitor.Index).ToString()));
                                        System.Console.WriteLine(JsonConvert.SerializeObject(G_ConnectedDevices_RESPONSE, Formatting.Indented));
                                        output += "\n" + JsonConvert.SerializeObject(G_ConnectedDevices_RESPONSE, Formatting.Indented);
                                        writelog($"Connecteddevices DISPLAY RESPONSE = {output}");
                                    }
                                }

                                if (IsFailhappened)
                                    return ((int)CLI_ExitCode.fail_SetVCPCapability, output);
                            }
                            else if (index.Count != 0)
                            {
                                bool IsFailhappened = false;
                                foreach (string idx in index)
                                {
                                    G_ConnectedDevices_RESPONSE = new ConnectedDevices();

                                    if (Convert.ToInt32(idx) < _AllInfoMonitors.Count)
                                    {
                                        G_ConnectedDevices_RESPONSE.Index = change_0base_to_1base(idx);
                                        G_ConnectedDevices_RESPONSE.ServiceTag = _AllInfoMonitors[Convert.ToInt32(idx)].edid.ServiceTag;
                                        G_ConnectedDevices_RESPONSE.Model = _AllInfoMonitors[Convert.ToInt32(idx)].edid.ModelName;
                                        G_ConnectedDevices_RESPONSE.SerialNumber = _AllInfoMonitors[Convert.ToInt32(idx)].edid.SerialNumber;
                                        G_ConnectedDevices_RESPONSE.FirmwareVersion = _AllInfoMonitors[Convert.ToInt32(idx)].FwVersion;

                                        if (string.IsNullOrWhiteSpace(_AllInfoMonitors[Convert.ToInt32(idx)].FwVersion))
                                        {
                                            writelog("FWVersion: IsNullOrWhiteSpace Fail_VCPCapability");
                                            G_ConnectedDevices_RESPONSE.PID = "N/A";
                                            G_ConnectedDevices_RESPONSE.Result = "Fail";
                                            G_ConnectedDevices_RESPONSE.Message = "Fail_VCPCapability";
                                            System.Console.WriteLine(JsonConvert.SerializeObject(G_ConnectedDevices_RESPONSE, Formatting.Indented));
                                            output += "\n" + JsonConvert.SerializeObject(G_ConnectedDevices_RESPONSE, Formatting.Indented);
                                            IsFailhappened = true;
                                        }
                                        else
                                        {
                                            G_ConnectedDevices_RESPONSE.PID = _AllInfoMonitors[Convert.ToInt32(idx)].edid.PID;
                                            G_ConnectedDevices_RESPONSE.Result = "Pass";
                                            index_per = int.Parse(change_0base_to_1base((_AllInfoMonitors[Convert.ToInt32(idx)].Index).ToString()));
                                            System.Console.WriteLine(JsonConvert.SerializeObject(G_ConnectedDevices_RESPONSE, Formatting.Indented));
                                            output += "\n" + JsonConvert.SerializeObject(G_ConnectedDevices_RESPONSE, Formatting.Indented);
                                        }
                                    }
                                    else
                                    {
                                        writelog("Convert.ToInt32(idx) >= _AllInfoMonitors.Count Fail_VCPCapability");
                                        G_ConnectedDevices_RESPONSE.Index = change_0base_to_1base(idx);
                                        G_ConnectedDevices_RESPONSE.ServiceTag = _AllInfoMonitors[Convert.ToInt32(idx)].edid.ServiceTag;
                                        G_ConnectedDevices_RESPONSE.Result = "Fail";
                                        G_ConnectedDevices_RESPONSE.Message = "Fail_VCPCapability";
                                        System.Console.WriteLine(JsonConvert.SerializeObject(G_ConnectedDevices_RESPONSE, Formatting.Indented));
                                        output += "\n" + JsonConvert.SerializeObject(G_ConnectedDevices_RESPONSE, Formatting.Indented);
                                        IsFailhappened = true;
                                    }
                                }
                            }
                            else if (serviceTag.Count != 0)
                            {
                                bool IsFailhappened = false;
                                foreach (string tag in serviceTag)
                                {
                                    var tmp = _AllInfoMonitors.FindAll(x => x.edid.ServiceTag.ToUpper().Equals(tag.ToUpper()));
                                    foreach (MonitorInfo mo in tmp)
                                    {
                                        G_ConnectedDevices_RESPONSE = new ConnectedDevices();

                                        G_ConnectedDevices_RESPONSE.Index = change_0base_to_1base((mo.Index).ToString());
                                        G_ConnectedDevices_RESPONSE.ServiceTag = mo.edid.ServiceTag;
                                        G_ConnectedDevices_RESPONSE.Model = mo.edid.ModelName;
                                        G_ConnectedDevices_RESPONSE.SerialNumber = mo.edid.SerialNumber;
                                        G_ConnectedDevices_RESPONSE.FirmwareVersion = mo.FwVersion;

                                        if (string.IsNullOrWhiteSpace(mo.FwVersion))
                                        {
                                            writelog("FWVersion: IsNullOrWhiteSpace Fail_VCPCapability");
                                            G_ConnectedDevices_RESPONSE.PID = "N/A";
                                            G_ConnectedDevices_RESPONSE.Result = "Fail";
                                            G_ConnectedDevices_RESPONSE.Message = "Fail_VCPCapability";
                                            System.Console.WriteLine(JsonConvert.SerializeObject(G_ConnectedDevices_RESPONSE, Formatting.Indented));
                                            output += "\n" + JsonConvert.SerializeObject(G_ConnectedDevices_RESPONSE, Formatting.Indented);
                                            IsFailhappened = true;
                                        }
                                        else
                                        {
                                            G_ConnectedDevices_RESPONSE.PID = mo.edid.PID;
                                            G_ConnectedDevices_RESPONSE.Result = "Pass";
                                            index_per = int.Parse(change_0base_to_1base((mo.Index).ToString()));
                                            System.Console.WriteLine(JsonConvert.SerializeObject(G_ConnectedDevices_RESPONSE, Formatting.Indented));
                                            output += "\n" + JsonConvert.SerializeObject(G_ConnectedDevices_RESPONSE, Formatting.Indented);
                                        }
                                    }
                                }

                                if (IsFailhappened)
                                    return ((int)CLI_ExitCode.fail_SetVCPCapability, output);
                            }
                            else if (model.Count != 0)
                            {
                                bool IsFailhappened = false;
                                foreach (string modelName in model)
                                {
                                    var tmp = _AllInfoMonitors.FindAll(x => x.edid.ModelName.ToUpper().Equals(modelName.ToUpper()));
                                    foreach (MonitorInfo mo in tmp)
                                    {
                                        G_ConnectedDevices_RESPONSE = new ConnectedDevices();

                                        G_ConnectedDevices_RESPONSE.Index = change_0base_to_1base((mo.Index).ToString());
                                        G_ConnectedDevices_RESPONSE.ServiceTag = mo.edid.ServiceTag;
                                        G_ConnectedDevices_RESPONSE.Model = mo.edid.ModelName;
                                        G_ConnectedDevices_RESPONSE.SerialNumber = mo.edid.SerialNumber;
                                        G_ConnectedDevices_RESPONSE.FirmwareVersion = mo.FwVersion;

                                        if (string.IsNullOrWhiteSpace(mo.FwVersion))
                                        {
                                            writelog("FWVersion: IsNullOrWhiteSpace Fail_VCPCapability");
                                            G_ConnectedDevices_RESPONSE.PID = "N/A";
                                            G_ConnectedDevices_RESPONSE.Result = "Fail";
                                            G_ConnectedDevices_RESPONSE.Message = "Fail_VCPCapability";
                                            System.Console.WriteLine(JsonConvert.SerializeObject(G_ConnectedDevices_RESPONSE, Formatting.Indented));
                                            output += "\n" + JsonConvert.SerializeObject(G_ConnectedDevices_RESPONSE, Formatting.Indented);
                                            IsFailhappened = true;
                                        }
                                        else
                                        {
                                            G_ConnectedDevices_RESPONSE.PID = mo.edid.PID;
                                            G_ConnectedDevices_RESPONSE.Result = "Pass";
                                            index_per = int.Parse(change_0base_to_1base((mo.Index).ToString()));
                                            System.Console.WriteLine(JsonConvert.SerializeObject(G_ConnectedDevices_RESPONSE, Formatting.Indented));
                                            output += "\n" + JsonConvert.SerializeObject(G_ConnectedDevices_RESPONSE, Formatting.Indented);
                                        }
                                    }
                                }

                                if (IsFailhappened)
                                    return ((int)CLI_ExitCode.fail_SetVCPCapability, output);
                            }
                            break;
                        case "MOUSE":
                        case "KEYBOARD":
                        case "WEBCAM":
                        case "WIREDAUDIO":
                        case "HEADSET":
                        case "PEN":
                        case "DOCK":
                        case "AIRAUDIO"://20250313 Elsa add for ConnectedDevices support guid/servicetag/model
                            if (GlobalDefinitions.isSupport210)
                            {
                                if (commandLineInput.GuidString != null && commandLineInput.GuidString.Count > 0 && !string.IsNullOrEmpty(commandLineInput.GuidString[0].ToString()))
                                {
                                    var match = _deviceinfo.SingleOrDefault(x => x.ID.ToString().Equals(commandLineInput.GuidString[0].ToString(), StringComparison.OrdinalIgnoreCase));
                                    if (match != null)
                                    {
                                        if (match.LogicalDeviceType.Equals($"Logical{commandLineInput.Options[0].Option_Value}", StringComparison.OrdinalIgnoreCase))
                                        {
                                            recode_per = true;
                                            output = GetPeripheralResponse(0, output, match);
                                        }
                                    }
                                    else
                                    {
                                        return Invalidcommand(commandLineInput, output, ref output_2, "guid");
                                    }
                                }
                                else if (commandLineInput.ServiceTag != null && commandLineInput.ServiceTag.Count > 0 && !string.IsNullOrEmpty(commandLineInput.ServiceTag[0].ToString()))
                                {
                                    var match = _deviceinfo.SingleOrDefault(x => x.DockServiceTag.ToString().Equals(commandLineInput.ServiceTag.ToString(), StringComparison.OrdinalIgnoreCase));
                                    if (match != null)
                                    {
                                        if (match.LogicalDeviceType.Equals($"Logical{commandLineInput.Options[0].Option_Value}", StringComparison.OrdinalIgnoreCase))
                                        {
                                            recode_per = true;
                                            output = GetPeripheralResponse(0, output, match);
                                        }
                                    }
                                    else
                                    {
                                        return Invalidcommand(commandLineInput, output, ref output_2, "ServiceTag");
                                    }
                                }
                                else if (commandLineInput.Model != null && commandLineInput.Model.Count > 0 && !string.IsNullOrEmpty(commandLineInput.Model[0].ToString()))
                                {
                                    var match = _deviceinfo.FindAll(x => x.ModelNumber.ToString().Equals(commandLineInput.Model[0].ToString(), StringComparison.OrdinalIgnoreCase));
                                    if (match.Count > 0)
                                    {
                                        foreach (var g in match)
                                        {
                                            if (g.LogicalDeviceType.Equals($"Logical{commandLineInput.Options[0].Option_Value}", StringComparison.OrdinalIgnoreCase))
                                            {
                                                index_per++;
                                                recode_per = true;
                                                output = GetPeripheralResponse(index_per, output, g);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        return Invalidcommand(commandLineInput, output, ref output_2, "Model");
                                    }
                                }
                                else
                                {
                                    foreach (var g in _deviceinfo)
                                    {
                                        if (g.LogicalDeviceType.Equals($"Logical{commandLineInput.Options[0].Option_Value}", StringComparison.OrdinalIgnoreCase))
                                        {
                                            index_per++;
                                            recode_per = true;
                                            output = GetPeripheralResponse(index_per, output, g);
                                        }
                                    }
                                }
                            }
                            break;
                        default:
                            break;
                    }
                    if (string.IsNullOrEmpty(output))
                    {
                        output = new CLI_RESPONSE3
                        {
                            Command = commandLineInput.Command,
                            TargetFeature = commandLineInput.TargetFeature,
                            Message = "No device found"
                        }.ToJson();
                    }
                    if ((_AllInfoMonitors == null || _AllInfoMonitors.Count == 0) && (_deviceinfo == null || _deviceinfo.Count == 0))
                    {
                        writelog($"No Devices Connected");
                        return ((int)CLI_ExitCode.null_device_manager, JsonConvert.SerializeObject(G_ConnectedDevices_RESPONSE, Formatting.Indented));
                    }
                }
                else if (commandLineInput.Options.Count == 0)
                {
                    if (index.Count == 0 && serviceTag.Count == 0)
                    {
                        bool IsFailhappened = false;
                        foreach (MonitorInfo monitor in _AllInfoMonitors)
                        {
                            G_ConnectedDevices_RESPONSE = new ConnectedDevices();
                            G_ConnectedDevices_RESPONSE.Model = monitor.edid.ModelName;
                            G_ConnectedDevices_RESPONSE.SerialNumber = monitor.edid.SerialNumber;
                            G_ConnectedDevices_RESPONSE.Index = change_0base_to_1base((monitor.Index).ToString());
                            G_ConnectedDevices_RESPONSE.ServiceTag = monitor.edid.ServiceTag;
                            G_ConnectedDevices_RESPONSE.FirmwareVersion = monitor.FwVersion;

                            if (string.IsNullOrWhiteSpace(monitor.FwVersion))
                            {
                                G_ConnectedDevices_RESPONSE.PID = "N/A";
                                G_ConnectedDevices_RESPONSE.Result = "Fail";
                                G_ConnectedDevices_RESPONSE.Message = "Fail_VCPCapability";
                                System.Console.WriteLine(JsonConvert.SerializeObject(G_ConnectedDevices_RESPONSE, Formatting.Indented));
                                output += "\n" + JsonConvert.SerializeObject(G_ConnectedDevices_RESPONSE, Formatting.Indented);
                                IsFailhappened = true;
                            }
                            else
                            {
                                G_ConnectedDevices_RESPONSE.PID = monitor.edid.PID.ToString();
                                G_ConnectedDevices_RESPONSE.Result = "Pass";
                                index_per = int.Parse(change_0base_to_1base((monitor.Index).ToString()));
                                System.Console.WriteLine(JsonConvert.SerializeObject(G_ConnectedDevices_RESPONSE, Formatting.Indented));
                                output += "\n" + JsonConvert.SerializeObject(G_ConnectedDevices_RESPONSE, Formatting.Indented);
                            }
                        }

                        if (IsFailhappened)
                            return ((int)CLI_ExitCode.fail_SetVCPCapability, output);
                    }
                    else if (index.Count != 0)
                    {
                        bool IsFailhappened = false;
                        foreach (string idx in index)
                        {
                            G_ConnectedDevices_RESPONSE = new ConnectedDevices();

                            if (Convert.ToInt32(idx) < _AllInfoMonitors.Count)
                            {
                                G_ConnectedDevices_RESPONSE.Index = change_0base_to_1base(idx);
                                G_ConnectedDevices_RESPONSE.ServiceTag = _AllInfoMonitors[Convert.ToInt32(idx)].edid.ServiceTag;
                                G_ConnectedDevices_RESPONSE.Model = _AllInfoMonitors[Convert.ToInt32(idx)].edid.ModelName;
                                G_ConnectedDevices_RESPONSE.SerialNumber = _AllInfoMonitors[Convert.ToInt32(idx)].edid.SerialNumber;
                                G_ConnectedDevices_RESPONSE.FirmwareVersion = _AllInfoMonitors[Convert.ToInt32(idx)].FwVersion;

                                if (string.IsNullOrWhiteSpace(_AllInfoMonitors[Convert.ToInt32(idx)].FwVersion))
                                {
                                    G_ConnectedDevices_RESPONSE.PID = "N/A";
                                    G_ConnectedDevices_RESPONSE.Result = "Fail";
                                    G_ConnectedDevices_RESPONSE.Message = "Fail_VCPCapability";
                                    System.Console.WriteLine(JsonConvert.SerializeObject(G_ConnectedDevices_RESPONSE, Formatting.Indented));
                                    output += "\n" + JsonConvert.SerializeObject(G_ConnectedDevices_RESPONSE, Formatting.Indented);
                                    IsFailhappened = true;
                                }
                                else
                                {
                                    G_ConnectedDevices_RESPONSE.PID = _AllInfoMonitors[Convert.ToInt32(idx)].edid.PID;
                                    G_ConnectedDevices_RESPONSE.Result = "Pass";
                                    index_per = int.Parse(change_0base_to_1base((_AllInfoMonitors[Convert.ToInt32(idx)].Index).ToString()));
                                    System.Console.WriteLine(JsonConvert.SerializeObject(G_ConnectedDevices_RESPONSE, Formatting.Indented));
                                    output += "\n" + JsonConvert.SerializeObject(G_ConnectedDevices_RESPONSE, Formatting.Indented);
                                }
                            }
                            else
                            {
                                G_ConnectedDevices_RESPONSE.Index = change_0base_to_1base(idx);
                                G_ConnectedDevices_RESPONSE.ServiceTag = _AllInfoMonitors[Convert.ToInt32(idx)].edid.ServiceTag;
                                G_ConnectedDevices_RESPONSE.Result = "Fail";
                                G_ConnectedDevices_RESPONSE.Message = "Fail_VCPCapability";
                                System.Console.WriteLine(JsonConvert.SerializeObject(G_ConnectedDevices_RESPONSE, Formatting.Indented));
                                output += "\n" + JsonConvert.SerializeObject(G_ConnectedDevices_RESPONSE, Formatting.Indented);
                                IsFailhappened = true;
                            }
                        }
                    }
                    else if (serviceTag.Count != 0)
                    {
                        bool IsFailhappened = false;
                        foreach (string tag in serviceTag)
                        {
                            var tmp = _AllInfoMonitors.FindAll(x => x.edid.ServiceTag.ToUpper().Equals(tag.ToUpper()));
                            foreach (MonitorInfo mo in tmp)
                            {
                                G_ConnectedDevices_RESPONSE = new ConnectedDevices();

                                G_ConnectedDevices_RESPONSE.Index = change_0base_to_1base((mo.Index).ToString());
                                G_ConnectedDevices_RESPONSE.ServiceTag = mo.edid.ServiceTag;
                                G_ConnectedDevices_RESPONSE.Model = mo.edid.ModelName;
                                G_ConnectedDevices_RESPONSE.SerialNumber = mo.edid.SerialNumber;
                                G_ConnectedDevices_RESPONSE.FirmwareVersion = mo.FwVersion;

                                if (string.IsNullOrWhiteSpace(mo.FwVersion))
                                {
                                    G_ConnectedDevices_RESPONSE.PID = "N/A";
                                    G_ConnectedDevices_RESPONSE.Result = "Fail";
                                    G_ConnectedDevices_RESPONSE.Message = "Fail_VCPCapability";
                                    System.Console.WriteLine(JsonConvert.SerializeObject(G_ConnectedDevices_RESPONSE, Formatting.Indented));
                                    output += "\n" + JsonConvert.SerializeObject(G_ConnectedDevices_RESPONSE, Formatting.Indented);
                                    IsFailhappened = true;
                                }
                                else
                                {
                                    G_ConnectedDevices_RESPONSE.PID = mo.edid.PID;
                                    G_ConnectedDevices_RESPONSE.Result = "Pass";
                                    index_per = int.Parse(change_0base_to_1base((mo.Index).ToString()));
                                    System.Console.WriteLine(JsonConvert.SerializeObject(G_ConnectedDevices_RESPONSE, Formatting.Indented));
                                    output += "\n" + JsonConvert.SerializeObject(G_ConnectedDevices_RESPONSE, Formatting.Indented);
                                }
                            }
                        }

                        if (IsFailhappened)
                            return ((int)CLI_ExitCode.fail_SetVCPCapability, output);
                    }
                    foreach (var g in _deviceinfo)
                    {
                        index_per++;
                        recode_per = true;
                        output = GetPeripheralResponse(index_per, output, g);
                    }
                    if ((_deviceinfo == null || _deviceinfo.Count == 0) && (_AllInfoMonitors == null || _AllInfoMonitors.Count == 0))
                        return ((int)CLI_ExitCode.null_device_manager, JsonConvert.SerializeObject(G_ConnectedDevices_RESPONSE, Formatting.Indented));
                }
                writelog($"Connecteddevices PASS = {output}");
                return ((int)CLI_ExitCode.success, output);
            }
            else
            {
                G_ConnectedDevices_RESPONSE.Result = "Fail";
                G_ConnectedDevices_RESPONSE.Message = $"Un-supported command: {type}";
            }
            writelog($"Connecteddevices unknow = {output}");
            return ((int)CLI_ExitCode.unknow_command, JsonConvert.SerializeObject(G_ConnectedDevices_RESPONSE, Formatting.Indented));
        }

        private static (int code, string result) Invalidcommand(CommandLineInput commandLineInput, string output, ref string output_2, string commandin)
        {
            CLI_RESPONSE3 cli_Response = new CLI_RESPONSE3();
            cli_Response.Command = commandLineInput.Command;
            cli_Response.TargetFeature = commandLineInput.TargetFeature;
            cli_Response.Result = "FAIL";
            cli_Response.Message = "Invalid " + commandin;
            output_2 += "\n" + JsonConvert.SerializeObject(cli_Response, Formatting.Indented);
            output_2 += output;
            return ((int)CLI_ExitCode.success, output_2);
        }

        private (int code, string result) ScreenNotificationx(IDeviceManagerSA devMgr, CommandLineInput commandLineInput)
        {
            if (commandLineInput.Command.Equals("GET"))
            {
                if (commandLineInput.Options.Count > 0)
                {
                    CLI_RESPONSE cli_Response = new CLI_RESPONSE();
                    cli_Response.Result = "FAIL";
                    cli_Response.Message = "Syntax error";
                    System.Console.WriteLine(JsonConvert.SerializeObject(cli_Response, Formatting.Indented));
                    return ((int)CLI_ExitCode.unknow_command, JsonConvert.SerializeObject(cli_Response, Formatting.Indented));
                }
                else
                {
                    return ScreenNotification(devMgr, commandLineInput).Result;
                }
            }
            else if (commandLineInput.Command.Equals("SET"))
            {
                if (commandLineInput.Options.Count > 1)
                {
                    CLI_RESPONSE cli_Response = new CLI_RESPONSE();
                    cli_Response.Result = "FAIL";
                    cli_Response.Message = "Syntax error";
                    System.Console.WriteLine(JsonConvert.SerializeObject(cli_Response, Formatting.Indented));
                    return ((int)CLI_ExitCode.unknow_command, JsonConvert.SerializeObject(cli_Response, Formatting.Indented));
                }
                else
                {
                    return ScreenNotification(devMgr, commandLineInput).Result;
                }
            }
            else
            {
                CLI_RESPONSE cli_Response = new CLI_RESPONSE();
                cli_Response.Result = "FAIL";
                cli_Response.Message = "Un-supported command";
                System.Console.WriteLine(JsonConvert.SerializeObject(cli_Response, Formatting.Indented));
                return ((int)CLI_ExitCode.unknow_command, JsonConvert.SerializeObject(cli_Response, Formatting.Indented));
            }
        }

        private async Task<(int code, string result)> ScreenNotification(IDeviceManagerSA devMgr, CommandLineInput commandLineInput)
        {
            string output = string.Empty;
            bool retcode = false;

            if (_AllInfoMonitors == null)
                _AllInfoMonitors = await devMgr.GetMonitors();

            DDPMSettings ddpmSettings = devMgr.ReloadAppConfigData().Result;

            if (ddpmSettings == null)
            {
                return DDPMSettingsNullResponse(commandLineInput.Command, commandLineInput.TargetFeature);
            }

            if (commandLineInput.Command == "SET" && commandLineInput.Options[0].Option_Value != null)
            {
                var result = string.Empty;
                var value = string.Empty;
                var message = "N/A";

                //commandLineInput.Options[0].Option_Value.Replace(".", ",");
                List<string> values = commandLineInput.Options[0].Option_Value.Replace(".", ",").Split(",").ToList();
                foreach (string v in values)
                {
                    switch (v.ToUpper())
                    {
                        case "ON":
                            writelog($"ScreenNotification on entry");
                            devMgr.Set_GlobalSetting_DisplayLowBatteryLevel(true);
                            devMgr.Set_GlobalSetting_DisplayKeyboardLockKey(true);
                            devMgr.Set_GlobalSetting_DisplayWB7022CoverState(true);
                            devMgr.Set_GlobalSetting_DisplayMuteState(true);
                            devMgr.Set_GlobalSetting_DisplayColorPresetAndEasyMemory(true);

                            result = "PASS";
                            value = "ON";
                            break;

                        case "OFF":
                            writelog($"ScreenNotification off entry");
                            devMgr.Set_GlobalSetting_DisplayLowBatteryLevel(false);
                            devMgr.Set_GlobalSetting_DisplayKeyboardLockKey(false);
                            devMgr.Set_GlobalSetting_DisplayWB7022CoverState(false);
                            devMgr.Set_GlobalSetting_DisplayMuteState(false);
                            devMgr.Set_GlobalSetting_DisplayColorPresetAndEasyMemory(false);
                            result = "PASS";
                            value = "OFF";
                            break;

                        case "LOCK":
                        case "UNLOCK":
                            if (v.ToUpper().Equals("LOCK")) ddpmSettings.LockSettings.Lock_Setting_ScreenNotification = true;
                            if (v.ToUpper().Equals("UNLOCK")) ddpmSettings.LockSettings.Lock_Setting_ScreenNotification = false;
                            await devMgr.SetAppConfigData(ddpmSettings);
                            break;

                        default:
                            writelog($"option value not support");

                            result = "FAIL";
                            message = "Un-supported command";
                            break;
                    }
                }
                //value += "," + (ddpmSettings.LockSettings.Lock_Setting_ScreenNotification ? "LOCK" : "UNLOCK");

                List<int> _monitorIndeies = new List<int>();

                if (_AllInfoMonitors == null)
                    _AllInfoMonitors = devMgr.GetMonitors().Result;

                if (_AllInfoMonitors.Count > 0)
                {
                    _monitorIndeies = GetMonitorIndeies(commandLineInput, _AllInfoMonitors);

                    foreach (int idx in _monitorIndeies)
                    {
                        MonitorInfo monitor = _AllInfoMonitors[idx];
                        CLI_RESPONSE cli_Response = new CLI_RESPONSE(monitor);
                        cli_Response.Command = commandLineInput.Command;
                        cli_Response.TargetFeature = commandLineInput.TargetFeature;
                        cli_Response.Result = result;
                        cli_Response.Value = value;
                        cli_Response.Message = message;

                        System.Console.WriteLine(JsonConvert.SerializeObject(cli_Response, Formatting.Indented));
                        output += "\n" + JsonConvert.SerializeObject(cli_Response, Formatting.Indented);
                    }
                }
                else
                {
                    CLI_RESPONSE cli_Response = new CLI_RESPONSE();
                    cli_Response.Command = commandLineInput.Command;
                    cli_Response.TargetFeature = commandLineInput.TargetFeature;
                    cli_Response.Result = result;
                    cli_Response.Value = value;
                    cli_Response.Message = message;

                    System.Console.WriteLine(JsonConvert.SerializeObject(cli_Response, Formatting.Indented));
                    output += "\n" + JsonConvert.SerializeObject(cli_Response, Formatting.Indented);
                }
            }
            if (commandLineInput.Command == "GET")
            {
                GlobalSettingParam param = new GlobalSettingParam();
                param = devMgr.GetGlobalSettingParam().Result;

                var result = "PASS";
                var value = (param.GlobalSetting_General.Low_Battery_Level.ToString().ToLower() == "true") ? "ON" : "OFF";
                //value += "," + (ddpmSettings.LockSettings.Lock_Setting_ScreenNotification ? "LOCK" : "UNLOCK");

                List<int> _monitorIndeies = new List<int>();
                if (_AllInfoMonitors == null)
                    _AllInfoMonitors = devMgr.GetMonitors().Result;

                if (_AllInfoMonitors.Count > 0)
                {
                    _monitorIndeies = GetMonitorIndeies(commandLineInput, _AllInfoMonitors);

                    foreach (int idx in _monitorIndeies)
                    {
                        writelog($"ScreenNotification get entry");

                        MonitorInfo monitor = _AllInfoMonitors[idx];

                        CLI_RESPONSE cli_Response = new CLI_RESPONSE(monitor);
                        cli_Response.Command = commandLineInput.Command;
                        cli_Response.TargetFeature = commandLineInput.TargetFeature;
                        cli_Response.Value = value;
                        cli_Response.Result = result;
                        System.Console.WriteLine(JsonConvert.SerializeObject(cli_Response, Formatting.Indented));
                        output += "\n" + JsonConvert.SerializeObject(cli_Response, Formatting.Indented);
                    }
                }
                else
                {
                    CLI_RESPONSE cli_Response = new CLI_RESPONSE();
                    cli_Response.Command = commandLineInput.Command;
                    cli_Response.TargetFeature = commandLineInput.TargetFeature;
                    cli_Response.Value = value;
                    cli_Response.Result = result;
                    System.Console.WriteLine(JsonConvert.SerializeObject(cli_Response, Formatting.Indented));
                    output += "\n" + JsonConvert.SerializeObject(cli_Response, Formatting.Indented);
                }
            }
            writelog($"ScreenNotification exit return value : {output}");
            return (retcode ? (int)CLI_ExitCode.success : (int)CLI_ExitCode.functional_error, output);
        }

        #region Jarvis methods

        private (int code, string result) DetectMonitorsX(CommandLineInput commandLineInput, IDeviceManagerSA devMgr)
        {
            if (commandLineInput.Command.Equals("GET"))
            {
                if (commandLineInput.Options.Count > 0 || commandLineInput.DeviceIndex.Count > 0 || commandLineInput.ServiceTag.Count > 0 || commandLineInput.Model.Count > 0)
                {
                    CLI_Get_MONITORS_RESPONSE G_MONITORS_RESPONSE = new CLI_Get_MONITORS_RESPONSE();
                    G_MONITORS_RESPONSE.Result = "FAIL";
                    G_MONITORS_RESPONSE.Message = "UNKNOWN COMMAND";
                    System.Console.WriteLine(JsonConvert.SerializeObject(G_MONITORS_RESPONSE, Formatting.Indented));
                    return ((int)CLI_ExitCode.unknow_command, JsonConvert.SerializeObject(G_MONITORS_RESPONSE, Formatting.Indented));
                }
                else
                {
                    var value = GetMonitors(devMgr, commandLineInput.Command, commandLineInput.DeviceIndex, commandLineInput.ServiceTag, commandLineInput.Model).Result;
                    return (value.code, value.result);
                }
            }
            else
            {
                CLI_Get_MONITORS_RESPONSE G_MONITORS_RESPONSE = new CLI_Get_MONITORS_RESPONSE();
                G_MONITORS_RESPONSE.Result = "FAIL";
                G_MONITORS_RESPONSE.Message = "UNKNOWN COMMAND";
                System.Console.WriteLine(JsonConvert.SerializeObject(G_MONITORS_RESPONSE, Formatting.Indented));
                return ((int)CLI_ExitCode.unknow_command, JsonConvert.SerializeObject(G_MONITORS_RESPONSE, Formatting.Indented));
            }
        }

        private (int code, string result) BrightnessX(CommandLineInput commandLineInput, IDeviceManagerSA devMgr)
        {
            if (commandLineInput.Command.Equals("GET"))
            {
                if (commandLineInput.Options.Count > 0)
                {
                    CLI_RESPONSE G_Brightness_RESPONSE = new CLI_RESPONSE();
                    G_Brightness_RESPONSE.Result = "FAIL";
                    G_Brightness_RESPONSE.Message = "UNKNOWN COMMAND";
                    System.Console.WriteLine(JsonConvert.SerializeObject(G_Brightness_RESPONSE, Formatting.Indented));
                    return ((int)CLI_ExitCode.unknow_command, JsonConvert.SerializeObject(G_Brightness_RESPONSE, Formatting.Indented));
                }
                else
                {
                    return Brightness(devMgr, commandLineInput.Command, commandLineInput.DeviceIndex, commandLineInput.ServiceTag, commandLineInput.Model, "").Result;
                }
            }
            else if (commandLineInput.Command.Equals("SET"))
            {
                if (commandLineInput.Options.Count == 1)
                {
                    string output = string.Empty;
                    int exitcode = 0;
                    for (int j = 0; j < commandLineInput.Options.Count; j++)
                    {
                        if (commandLineInput.Options[j].Option_Name.ToUpper().Equals("VALUE"))
                        {
                            var temp = Brightness(devMgr, commandLineInput.Command, commandLineInput.DeviceIndex, commandLineInput.ServiceTag, commandLineInput.Model, commandLineInput.Options[j].Option_Value).Result;
                            output += "\n" + temp.result;
                            exitcode = temp.code;
                            if (exitcode != 0)
                                break;
                        }
                    }
                    return (exitcode, output);
                }
                else
                {
                    CLI_RESPONSE S_Brightness_RESPONSE = new CLI_RESPONSE();
                    S_Brightness_RESPONSE.Result = "FAIL";
                    S_Brightness_RESPONSE.Message = "UNKNOWN COMMAND";
                    System.Console.WriteLine(JsonConvert.SerializeObject(S_Brightness_RESPONSE, Formatting.Indented));
                    return ((int)CLI_ExitCode.unknow_command, JsonConvert.SerializeObject(S_Brightness_RESPONSE, Formatting.Indented));
                }
            }
            else
            {
                CLI_RESPONSE S_Brightness_RESPONSE = new CLI_RESPONSE();
                S_Brightness_RESPONSE.Result = "FAIL";
                S_Brightness_RESPONSE.Message = "UNKNOWN COMMAND";
                System.Console.WriteLine(JsonConvert.SerializeObject(S_Brightness_RESPONSE, Formatting.Indented));
                return ((int)CLI_ExitCode.unknow_command, JsonConvert.SerializeObject(S_Brightness_RESPONSE, Formatting.Indented));
            }
        }

        private (int code, string result) ContrastX(CommandLineInput commandLineInput, IDeviceManagerSA devMgr)
        {
            if (commandLineInput.Command.Equals("GET"))
            {
                if (commandLineInput.Options.Count > 0)
                {
                    CLI_RESPONSE G_Contrast_RESPONSE = new CLI_RESPONSE();
                    G_Contrast_RESPONSE.Result = "FAIL";
                    G_Contrast_RESPONSE.Message = "UNKNOWN COMMAND";
                    System.Console.WriteLine(JsonConvert.SerializeObject(G_Contrast_RESPONSE, Formatting.Indented));
                    return ((int)CLI_ExitCode.unknow_command, JsonConvert.SerializeObject(G_Contrast_RESPONSE, Formatting.Indented));
                }
                else
                    return Contrast(devMgr, commandLineInput.Command, commandLineInput.DeviceIndex, commandLineInput.ServiceTag, commandLineInput.Model, "").Result;
            }
            else if (commandLineInput.Command.Equals("SET"))
            {
                if (commandLineInput.Options.Count == 1)
                {
                    int exitcode = 0;
                    string output = string.Empty;
                    for (int j = 0; j < commandLineInput.Options.Count; j++)
                    {
                        if (commandLineInput.Options[j].Option_Name.ToUpper().Equals("VALUE"))
                        {
                            var value = Contrast(devMgr, commandLineInput.Command, commandLineInput.DeviceIndex, commandLineInput.ServiceTag, commandLineInput.Model, commandLineInput.Options[j].Option_Value).Result;
                            exitcode = value.code;
                            output += "\n" + value.result;
                        }
                        if (exitcode != 0)
                            return (exitcode, output);
                    }
                    return (exitcode, output);
                }
                else
                {
                    CLI_RESPONSE S_Contrast_RESPONSE = new CLI_RESPONSE();
                    S_Contrast_RESPONSE.Result = "FAIL";
                    S_Contrast_RESPONSE.Message = "UNKNOWN COMMAND";
                    System.Console.WriteLine(JsonConvert.SerializeObject(S_Contrast_RESPONSE, Formatting.Indented));
                    return ((int)CLI_ExitCode.unknow_command, JsonConvert.SerializeObject(S_Contrast_RESPONSE, Formatting.Indented));
                }
            }
            else
            {
                CLI_RESPONSE S_Contrast_RESPONSE = new CLI_RESPONSE();
                S_Contrast_RESPONSE.Result = "FAIL";
                S_Contrast_RESPONSE.Message = "UNKNOWN COMMAND";
                System.Console.WriteLine(JsonConvert.SerializeObject(S_Contrast_RESPONSE, Formatting.Indented));
                return ((int)CLI_ExitCode.unknow_command, JsonConvert.SerializeObject(S_Contrast_RESPONSE, Formatting.Indented));
            }

            //return exitcode;
        }

        private (int code, string result) LuminusX(CommandLineInput commandLineInput, IDeviceManagerSA devMgr)
        {
            if (commandLineInput.Command.Equals("GET"))
            {
                if (commandLineInput.Options.Count > 0)
                {
                    CLI_Get_Luminus_RESPONSE G_Luminus_RESPONSE = new CLI_Get_Luminus_RESPONSE();
                    G_Luminus_RESPONSE.Result = "FAIL";
                    G_Luminus_RESPONSE.Message = "UNKNOWN COMMAND";
                    System.Console.WriteLine(JsonConvert.SerializeObject(G_Luminus_RESPONSE, Formatting.Indented));
                    return ((int)CLI_ExitCode.unknow_command, JsonConvert.SerializeObject(G_Luminus_RESPONSE, Formatting.Indented));
                }
                else
                    return Luminus(devMgr, commandLineInput.Command, commandLineInput.DeviceIndex, commandLineInput.ServiceTag, commandLineInput.Model, "").Result;
            }
            else if (commandLineInput.Command.Equals("SET"))
            {
                if (commandLineInput.Options.Count == 1)
                {
                    int exitcode = 0;
                    string output = string.Empty;
                    for (int j = 0; j < commandLineInput.Options.Count; j++)
                    {
                        if (commandLineInput.Options[j].Option_Name.ToUpper().Equals("VALUE"))
                        {
                            var value = Luminus(devMgr, commandLineInput.Command, commandLineInput.DeviceIndex, commandLineInput.ServiceTag, commandLineInput.Model, commandLineInput.Options[j].Option_Value).Result;
                            output += "\n" + value.result;
                            exitcode = value.code;
                        }
                        if (exitcode != 0)
                            return (exitcode, output);
                    }
                    return (exitcode, output);
                }
                else
                {
                    CLI_RESPONSE S_Luminus_RESPONSE = new CLI_RESPONSE();
                    S_Luminus_RESPONSE.Result = "FAIL";
                    S_Luminus_RESPONSE.Message = "UNKNOWN COMMAND";
                    System.Console.WriteLine(JsonConvert.SerializeObject(S_Luminus_RESPONSE, Formatting.Indented));
                    return ((int)CLI_ExitCode.unknow_command, JsonConvert.SerializeObject(S_Luminus_RESPONSE, Formatting.Indented));
                }
            }
            else
            {
                CLI_RESPONSE S_Luminus_RESPONSE = new CLI_RESPONSE();
                S_Luminus_RESPONSE.Result = "FAIL";
                S_Luminus_RESPONSE.Message = "UNKNOWN COMMAND";
                System.Console.WriteLine(JsonConvert.SerializeObject(S_Luminus_RESPONSE, Formatting.Indented));
                return ((int)CLI_ExitCode.unknow_command, JsonConvert.SerializeObject(S_Luminus_RESPONSE, Formatting.Indented));
            }

            //return exitcode;
        }

        private (int code, string result) EDIDX(CommandLineInput commandLineInput, IDeviceManagerSA devMgr)
        {
            if (commandLineInput.Command.Equals("GET"))
            {
                if (commandLineInput.Options.Count > 0)
                {
                    CLI_Get_EDID_RESPONSE G_EDID_RESPONSE = new CLI_Get_EDID_RESPONSE();
                    G_EDID_RESPONSE.Result = "FAIL";
                    G_EDID_RESPONSE.Message = "UNKNOWN COMMAND";
                    System.Console.WriteLine(JsonConvert.SerializeObject(G_EDID_RESPONSE, Formatting.Indented));
                    return ((int)CLI_ExitCode.unknow_command, JsonConvert.SerializeObject(G_EDID_RESPONSE, Formatting.Indented));
                }
                else
                {
                    return EDID(devMgr, commandLineInput.Command, commandLineInput.DeviceIndex, commandLineInput.ServiceTag, commandLineInput.Model).Result;
                }
            }
            else if (commandLineInput.Command.Equals("READ"))
            {
                if (commandLineInput.Options.Count > 0)
                {
                    CLI_Read_EDID_RESPONSE R_EDID_RESPONSE = new CLI_Read_EDID_RESPONSE();
                    R_EDID_RESPONSE.Result = "FAIL";
                    R_EDID_RESPONSE.Message = "UNKNOWN COMMAND";
                    System.Console.WriteLine(JsonConvert.SerializeObject(R_EDID_RESPONSE, Formatting.Indented));
                    return ((int)CLI_ExitCode.unknow_command, JsonConvert.SerializeObject(R_EDID_RESPONSE, Formatting.Indented));
                }
                else
                {
                    return EDID(devMgr, commandLineInput.Command, commandLineInput.DeviceIndex, commandLineInput.ServiceTag, commandLineInput.Model).Result;
                }
            }
            else
            {
                CLI_RESPONSE G_EDID_RESPONSE = new CLI_Get_EDID_RESPONSE();
                G_EDID_RESPONSE.Result = "FAIL";
                G_EDID_RESPONSE.Message = "UNKNOWN COMMAND";
                System.Console.WriteLine(JsonConvert.SerializeObject(G_EDID_RESPONSE, Formatting.Indented));
                return ((int)CLI_ExitCode.unknow_command, JsonConvert.SerializeObject(G_EDID_RESPONSE, Formatting.Indented));
            }
        }

        private (int code, string result) DECODEDEDIDX(CommandLineInput commandLineInput, IDeviceManagerSA devMgr)
        {
            if (commandLineInput.Command.Equals("GET"))
            {
                if (commandLineInput.Options.Count > 0)
                {
                    CLI_Get_EDID_RESPONSE G_EDID_RESPONSE = new CLI_Get_EDID_RESPONSE();
                    G_EDID_RESPONSE.Result = "FAIL";
                    G_EDID_RESPONSE.Message = "UNKNOWN COMMAND";
                    System.Console.WriteLine(JsonConvert.SerializeObject(G_EDID_RESPONSE, Formatting.Indented));
                    return ((int)CLI_ExitCode.unknow_command, JsonConvert.SerializeObject(G_EDID_RESPONSE, Formatting.Indented));
                }
                else
                {
                    return DECODEDEDID(devMgr, commandLineInput.Command, commandLineInput.DeviceIndex, commandLineInput.ServiceTag, commandLineInput.Model).Result;
                }
            }
            else
            {
                CLI_RESPONSE G_EDID_RESPONSE = new CLI_Get_EDID_RESPONSE();
                G_EDID_RESPONSE.Result = "FAIL";
                G_EDID_RESPONSE.Message = "UNKNOWN COMMAND";
                System.Console.WriteLine(JsonConvert.SerializeObject(G_EDID_RESPONSE, Formatting.Indented));
                return ((int)CLI_ExitCode.unknow_command, JsonConvert.SerializeObject(G_EDID_RESPONSE, Formatting.Indented));
            }
        }

        private (int code, string result) FWVersionX(CommandLineInput commandLineInput, IDeviceManagerSA devMgr)
        {
            if (commandLineInput.Command.Equals("GET"))
            {
                if (commandLineInput.Options.Count > 0)
                {
                    CLI_RESPONSE G_FW_RESPONSE = new CLI_RESPONSE();
                    G_FW_RESPONSE.Result = "FAIL";
                    G_FW_RESPONSE.Message = "UNKNOWN COMMAND";
                    System.Console.WriteLine(JsonConvert.SerializeObject(G_FW_RESPONSE, Formatting.Indented));
                    return ((int)CLI_ExitCode.unknow_command, JsonConvert.SerializeObject(G_FW_RESPONSE, Formatting.Indented));
                }
                else
                {
                    return FWVersion(devMgr, commandLineInput.Command, commandLineInput.DeviceIndex, commandLineInput.ServiceTag, commandLineInput.Model).Result;
                }
            }
            else
            {
                CLI_RESPONSE G_FW_RESPONSE = new CLI_RESPONSE();
                G_FW_RESPONSE.Result = "FAIL";
                G_FW_RESPONSE.Message = "UNKNOWN COMMAND";
                G_FW_RESPONSE.Command = commandLineInput.Command;
                G_FW_RESPONSE.TargetFeature = commandLineInput.TargetFeature;
                System.Console.WriteLine(JsonConvert.SerializeObject(G_FW_RESPONSE, Formatting.Indented));
                return ((int)CLI_ExitCode.unknow_command, JsonConvert.SerializeObject(G_FW_RESPONSE, Formatting.Indented));
            }
        }

        private bool IsHexNumeric(string maybeHex)
        {
            bool rc = false;
            int number;
            var hexStyle = System.Globalization.NumberStyles.HexNumber;
            if (int.TryParse(maybeHex, hexStyle, CultureInfo.CurrentCulture, out number))
                rc = true;
            return rc;
        }

        private bool IsNumeric(string str)
        {
            var IsNumeric = int.TryParse(str, out _);
            return IsNumeric;
        }

        private async Task<bool> SetVCPCode(IDeviceManagerSA devMgr, MonitorInfo mo, string vcpcode, string value)
        {
            writelog($"SetVCPCode entry, vcpcode: {vcpcode}, monitor: {mo?.modelName}, value: {value}");
            if (devMgr == null)
            {
                writelog("SetVCPCode: input null IDeviceManagerSA");
                return false;
            }

            if (_AllInfoMonitors == null)
                _AllInfoMonitors = await devMgr.GetMonitors();

            bool result = false;

            if (_AllInfoMonitors.Exists(t => t.edid.SerialNumber == mo.edid.SerialNumber))
            {
                bool IsHexNumeric_vcpcode = vcpcode.ToLower().Contains("0x") ? (IsHexNumeric(vcpcode.ToLower().Replace("0x", string.Empty)) ? true : false) : false;
                bool IsNumeric_vcpcode = IsHexNumeric_vcpcode ? true : (IsNumeric(vcpcode) ? true : false);
                bool IsHexNumeric_value = value.ToLower().Contains("0x") ? (IsHexNumeric(value.ToLower().Replace("0x", string.Empty)) ? true : false) : false;
                bool IsNumeric_value = IsHexNumeric_value ? true : (IsNumeric(value) ? true : false);

                byte byte_vcpcode = IsHexNumeric_vcpcode ? (Convert.ToByte(vcpcode, 16)) : (IsNumeric_vcpcode ? Convert.ToByte(vcpcode, 10) : default);
                uint uint_value = IsHexNumeric_value ? (Convert.ToUInt32(value, 16)) : (IsNumeric_value ? Convert.ToUInt32(value, 10) : default);

                if (IsNumeric_vcpcode && IsNumeric_value)
                    result = await devMgr.SetVCPCapability(mo, byte_vcpcode, uint_value);
                else
                    result = await devMgr.SetVCPCapability(mo, vcpcode, value);
            }
            else
                return false;

            writelog($"SetVCPCode exit, vcpcode: {vcpcode}, monitor: {mo?.modelName}, value: {value}");
            return result;
        }

        private async Task<bool> SetVCPCode(IDeviceManagerSA devMgr, int index, string vcpcode, string value)
        {
            writelog($"SetVCPCode entry, vcpcode: {vcpcode}, index: {index}, value: {value}");
            if (devMgr == null)
            {
                writelog("SetVCPCode: input null IDeviceManagerSA");
                return false;
            }

            if (_AllInfoMonitors == null)
                _AllInfoMonitors = await devMgr.GetMonitors();

            bool result = false;

            if (index < _AllInfoMonitors.Count)
                result = await SetVCPCode(devMgr, _AllInfoMonitors[index], vcpcode, value);
            else
                return false;

            writelog($"SetVCPCode exit, vcpcode: {vcpcode}, index: {index}, value: {value}");
            return result;
        }

        private async Task<ObjGetVCP> GetVCPCode(IDeviceManagerSA devMgr, MonitorInfo mo, string vcpcode, int opt = 0)
        {
            writelog($"GetVCPCode entry, vcpcode: {vcpcode}, monitor: {mo?.modelName}");
            if (devMgr == null)
            {
                writelog("GetVCPCode: input null IDeviceManagerSA");
                return new ObjGetVCP() { result = false, value = null };
            }

            if (_AllInfoMonitors == null)
                _AllInfoMonitors = await devMgr.GetMonitors();

            ObjGetVCP result = new ObjGetVCP();
            if (_AllInfoMonitors.Exists(t => t.edid.SerialNumber == mo.edid.SerialNumber))
            {
                bool IsHexNumeric_vcpcode = vcpcode.ToLower().Contains("0x") ? (IsHexNumeric(vcpcode.ToLower().Replace("0x", string.Empty)) ? true : false) : false;
                bool IsNumeric_vcpcode = IsHexNumeric_vcpcode ? true : (IsNumeric(vcpcode) ? true : false);
                byte byte_vcpcode = IsHexNumeric_vcpcode ? (Convert.ToByte(vcpcode, 16)) : (IsNumeric_vcpcode ? Convert.ToByte(vcpcode, 10) : default);

                if (IsNumeric_vcpcode)
                    result = await devMgr.GetVCPCapability(mo, byte_vcpcode, opt: opt);
                else
                    result = await devMgr.GetVCPCapability(mo, vcpcode, opt: opt);
            }
            else
                return new ObjGetVCP() { result = false, value = null };

            writelog($"GetVCPCode exit, vcpcode: {vcpcode}, monitor: {mo?.modelName}");
            return result;
        }

        private async Task<ObjGetVCP> GetVCPCodeMax(IDeviceManagerSA devMgr, MonitorInfo mo, string vcpcode)
        {
            writelog($"GetVCPCodeMax entry, vcpcode: {vcpcode}, monitor: {mo?.modelName}");
            if (devMgr == null)
            {
                writelog("GetVCPCodeMax: input null IDeviceManagerSA");
                return new ObjGetVCP() { result = false, value = null };
            }

            if (_AllInfoMonitors == null)
                _AllInfoMonitors = await devMgr.GetMonitors();

            ObjGetVCP result = new ObjGetVCP();
            if (_AllInfoMonitors.Exists(t => t.edid.SerialNumber == mo.edid.SerialNumber))
            {
                bool IsHexNumeric_vcpcode = vcpcode.ToLower().Contains("0x") ? (IsHexNumeric(vcpcode.ToLower().Replace("0x", string.Empty)) ? true : false) : false;
                bool IsNumeric_vcpcode = IsHexNumeric_vcpcode ? true : (IsNumeric(vcpcode) ? true : false);
                byte byte_vcpcode = IsHexNumeric_vcpcode ? (Convert.ToByte(vcpcode, 16)) : (IsNumeric_vcpcode ? Convert.ToByte(vcpcode, 10) : default);

                if (IsNumeric_vcpcode)
                    result = await devMgr.GetVCPCapability(mo, byte_vcpcode, opt: 1);
                else
                    result = await devMgr.GetVCPCapability(mo, vcpcode, opt: 1);
            }
            else
                return new ObjGetVCP() { result = false, value = null };

            writelog($"GetVCPCodeMax exit, vcpcode: {vcpcode}, monitor: {mo?.modelName}");
            return result;
        }

        private async Task<ObjGetVCP> GetVCPCode(IDeviceManagerSA devMgr, int index, string vcpcode)
        {
            writelog($"GetVCPCode entry, vcpcode: {vcpcode}, index: {index}");
            if (devMgr == null)
            {
                writelog("GetVCPCode: input null IDeviceManagerSA");
                return new ObjGetVCP() { result = false, value = null };
            }

            if (_AllInfoMonitors == null)
                _AllInfoMonitors = await devMgr.GetMonitors();

            ObjGetVCP result = new ObjGetVCP();
            if (index < _AllInfoMonitors.Count)
                result = await GetVCPCode(devMgr, _AllInfoMonitors[index], vcpcode);
            else
                return new ObjGetVCP() { result = false, value = null };

            return result;
        }

        private async Task<(int code, string result)> Brightness(IDeviceManagerSA devMgr, string type, List<string> index, List<string> serviceTag, List<string> model, string value = "")
        {
            CLI_RESPONSE S_Brightness_RESPONSE = new CLI_RESPONSE();
            CLI_RESPONSE G_Brightness_RESPONSE = new CLI_RESPONSE();

            //if (devMgr == null)
            //{
            //    writelog("Brightness: Null IDeviceManagerSA");
            //    return (int)CLI_ExitCode.null_device_manager;
            //}

            if (_AllInfoMonitors == null)
                _AllInfoMonitors = await devMgr.GetMonitors();
            string output = string.Empty;

            if (type == "SET")
            {
                writelog($"Brightness set entry");
                if (string.IsNullOrWhiteSpace(value) || !int.TryParse(value, out int intValue))
                {
                    S_Brightness_RESPONSE.Result = "Format Error";
                    S_Brightness_RESPONSE.Message = "Format Error";
                    return ((int)CLI_ExitCode.fail_FormantError, S_Brightness_RESPONSE.ToJson());
                }
                if (index.Count == 0 && serviceTag.Count == 0 && model.Count == 0)
                {
                    bool rc = false;
                    bool IsFailhappened = false;
                    foreach (MonitorInfo monitor in _AllInfoMonitors)
                    {
                        S_Brightness_RESPONSE = new CLI_RESPONSE(monitor);
                        if (intValue > 100)
                        {
                            S_Brightness_RESPONSE.Result = "Format Error, Brightness lager then 100";
                            S_Brightness_RESPONSE.Message = "Format Error";
                            return ((int)CLI_ExitCode.fail_FormantError, S_Brightness_RESPONSE.ToJson());
                        }

                        var setValue = value;

                        if (!monitor.CapabilityDic.ContainsKey("12"))
                        {
                            var maxLuminance = GetVCPCodeMax(devMgr, monitor, "0x10").Result;
                            setValue = (int.Parse(maxLuminance.value.ToString()) * intValue / 100).ToString();
                        }

                        rc = SetVCPCode(devMgr, monitor, "0x10", setValue).Result;
                        S_Brightness_RESPONSE.Command = "SET";
                        S_Brightness_RESPONSE.TargetFeature = "BRIGHTNESSLEVEL";//change from brightness to brightnesslevel to align with spec
                        S_Brightness_RESPONSE.Value = value;

                        if (!rc)
                        {
                            S_Brightness_RESPONSE.Result = "FAIL";
                            S_Brightness_RESPONSE.Message = "FAIL VCP";
                            Console.WriteLine(S_Brightness_RESPONSE.ToJson());
                            output += "\n" + S_Brightness_RESPONSE.ToJson();
                            IsFailhappened = true;
                        }
                        else
                        {
                            S_Brightness_RESPONSE.Result = "PASS";
                            Console.WriteLine(S_Brightness_RESPONSE.ToJson());
                            output += "\n" + S_Brightness_RESPONSE.ToJson();
                        }
                    }

                    if (IsFailhappened)
                        return ((int)CLI_ExitCode.fail_SetVCPCapability, output);
                }
                else
                {
                    bool IsFailhappened = false;
                    foreach (string idx in index)
                    {
                        writelog($"Brightness set idx entry");

                        bool rc = false;
                        int nidx = int.Parse(idx);
                        S_Brightness_RESPONSE = new CLI_RESPONSE(_AllInfoMonitors[nidx]);
                        if (intValue > 100)
                        {
                            S_Brightness_RESPONSE.Result = "Format Error, Brightness lager then 100";
                            S_Brightness_RESPONSE.Message = "Format Error";
                            return ((int)CLI_ExitCode.fail_FormantError, S_Brightness_RESPONSE.ToJson());
                        }

                        var setValue = value;

                        if (!_AllInfoMonitors[nidx].CapabilityDic.ContainsKey("12"))
                        {
                            var maxLuminance = GetVCPCodeMax(devMgr, _AllInfoMonitors[nidx], "0x10").Result;
                            setValue = (int.Parse(maxLuminance.value.ToString()) * intValue / 100).ToString();
                        }

                        rc = SetVCPCode(devMgr, nidx, "0x10", setValue).Result;

                        S_Brightness_RESPONSE.Command = "SET";
                        S_Brightness_RESPONSE.TargetFeature = "BRIGHTNESSLEVEL";
                        S_Brightness_RESPONSE.Value = value;

                        if (!rc)
                        {
                            S_Brightness_RESPONSE.Result = "FAIL";
                            S_Brightness_RESPONSE.Message = "FAIL VCP";
                            Console.WriteLine(S_Brightness_RESPONSE.ToJson());
                            output += "\n" + S_Brightness_RESPONSE.ToJson();
                            IsFailhappened = true;
                        }
                        else
                        {
                            S_Brightness_RESPONSE.Result = "PASS";
                            Console.WriteLine(S_Brightness_RESPONSE.ToJson());
                            output += "\n" + S_Brightness_RESPONSE.ToJson();
                        }
                    }

                    foreach (string tag in serviceTag)
                    {
                        writelog($"Brightness set tag entry");

                        bool rc = false;
                        var tmp = _AllInfoMonitors.FindAll(x => x.edid.ServiceTag.ToUpper().Equals(tag.ToUpper()));
                        foreach (MonitorInfo mo in tmp)
                        {
                            S_Brightness_RESPONSE = new CLI_RESPONSE(mo);
                            if (intValue > 100)
                            {
                                S_Brightness_RESPONSE.Result = "Format Error, Brightness lager then 100";
                                S_Brightness_RESPONSE.Message = "Format Error";
                                return ((int)CLI_ExitCode.fail_FormantError, S_Brightness_RESPONSE.ToJson());
                            }

                            var setValue = value;

                            if (!mo.CapabilityDic.ContainsKey("12"))
                            {
                                var maxLuminance = GetVCPCodeMax(devMgr, mo, "0x10").Result;
                                setValue = (int.Parse(maxLuminance.value.ToString()) * intValue / 100).ToString();
                            }

                            rc = SetVCPCode(devMgr, mo, "0x10", setValue).Result;

                            S_Brightness_RESPONSE.Command = "SET";
                            S_Brightness_RESPONSE.TargetFeature = "BRIGHTNESSLEVEL";
                            S_Brightness_RESPONSE.Value = value;

                            if (!rc)
                            {
                                S_Brightness_RESPONSE.Result = "FAIL";
                                S_Brightness_RESPONSE.Message = "FAIL VCP";
                                Console.WriteLine(S_Brightness_RESPONSE.ToJson());
                                output += "\n" + S_Brightness_RESPONSE.ToJson();
                                IsFailhappened = true;
                            }
                            else
                            {
                                S_Brightness_RESPONSE.Result = "PASS";
                                Console.WriteLine(S_Brightness_RESPONSE.ToJson());
                                output += "\n" + S_Brightness_RESPONSE.ToJson();
                            }
                        }
                    }

                    foreach (string modelName in model)
                    {
                        writelog($"Brightness set model entry");

                        bool rc = false;
                        var tmp = _AllInfoMonitors.FindAll(x => x.modelName.ToUpper().Equals(modelName.ToUpper()));
                        foreach (MonitorInfo mo in tmp)
                        {
                            S_Brightness_RESPONSE = new CLI_RESPONSE(mo);
                            if (intValue > 100)
                            {
                                S_Brightness_RESPONSE.Result = "Format Error, Brightness lager then 100";
                                S_Brightness_RESPONSE.Message = "Format Error";
                                return ((int)CLI_ExitCode.fail_FormantError, S_Brightness_RESPONSE.ToJson());
                            }

                            var setValue = value;

                            if (!mo.CapabilityDic.ContainsKey("12"))
                            {
                                var maxLuminance = GetVCPCodeMax(devMgr, mo, "0x10").Result;
                                setValue = (int.Parse(maxLuminance.value.ToString()) * intValue / 100).ToString();
                            }

                            rc = SetVCPCode(devMgr, mo, "0x10", setValue).Result;

                            S_Brightness_RESPONSE.Command = "SET";
                            S_Brightness_RESPONSE.TargetFeature = "BRIGHTNESSLEVEL";
                            S_Brightness_RESPONSE.Value = value;

                            if (!rc)
                            {
                                S_Brightness_RESPONSE.Result = "FAIL";
                                S_Brightness_RESPONSE.Message = "FAIL VCP";
                                Console.WriteLine(S_Brightness_RESPONSE.ToJson());
                                output += "\n" + S_Brightness_RESPONSE.ToJson();
                                IsFailhappened = true;
                            }
                            else
                            {
                                S_Brightness_RESPONSE.Result = "PASS";
                                Console.WriteLine(S_Brightness_RESPONSE.ToJson());
                                output += "\n" + S_Brightness_RESPONSE.ToJson();
                            }
                        }
                    }
                    if (IsFailhappened)
                    {
                        writelog($"Brightness set fail {output}");
                        return ((int)CLI_ExitCode.fail_SetVCPCapability, output);
                    }

                }
                writelog($"Brightness get return value {output}");
                return ((int)CLI_ExitCode.success, output);
            }
            else if (type == "GET")
            {
                if (index.Count == 0 && serviceTag.Count == 0 && model.Count == 0)
                {
                    ObjGetVCP rc = new ObjGetVCP();
                    bool IsFailhappened = false;
                    foreach (MonitorInfo monitor in _AllInfoMonitors)
                    {
                        writelog($"Brightness get entry");
                        G_Brightness_RESPONSE = new CLI_RESPONSE(monitor);

                        rc = GetVCPCode(devMgr, monitor, "0x10").Result;

                        G_Brightness_RESPONSE.Command = "GET";
                        G_Brightness_RESPONSE.TargetFeature = "BRIGHTNESSLEVEL";//change from brightness to brightnesslevel to align with spec

                        if (!rc.result)
                        {
                            G_Brightness_RESPONSE.Value = "N/A";
                            G_Brightness_RESPONSE.Result = "FAIL";
                            G_Brightness_RESPONSE.Message = "FAIL VCP";
                            Console.WriteLine(G_Brightness_RESPONSE.ToJson());
                            output += "\n" + G_Brightness_RESPONSE.ToJson();
                            IsFailhappened = true;
                        }
                        else
                        {
                            G_Brightness_RESPONSE.Value = $"{rc.value}";
                            if (!monitor.CapabilityDic.ContainsKey("12"))
                            {
                                var maxLuminance = GetVCPCodeMax(devMgr, monitor, "0x10").Result;
                                if (int.TryParse(rc?.value.ToString(), out int intRcValue) && int.TryParse(maxLuminance?.value.ToString(), out int intMaxLuminance))
                                {
                                    G_Brightness_RESPONSE.Value = $"{(int)((double)intRcValue / intMaxLuminance * 100)}";
                                }
                            }
                            G_Brightness_RESPONSE.Result = "PASS";
                            Console.WriteLine(G_Brightness_RESPONSE.ToJson());
                            output += "\n" + G_Brightness_RESPONSE.ToJson();
                        }
                    }

                    if (IsFailhappened)
                        return ((int)CLI_ExitCode.fail_SetVCPCapability, output);
                }
                else
                {
                    bool IsFailhappened = false;
                    foreach (string idx in index)
                    {
                        writelog($"Brightness get idx entry");

                        ObjGetVCP rc = new ObjGetVCP();
                        int nidx = int.Parse(idx);
                        rc = GetVCPCode(devMgr, nidx, "0x10").Result;

                        G_Brightness_RESPONSE = new CLI_RESPONSE(_AllInfoMonitors[nidx]);
                        G_Brightness_RESPONSE.Command = "GET";
                        G_Brightness_RESPONSE.TargetFeature = "BRIGHTNESSLEVEL";//change from brightness to brightnesslevel to align with spec

                        if (!rc.result)
                        {
                            G_Brightness_RESPONSE.Value = "N/A";
                            G_Brightness_RESPONSE.Result = "FAIL";
                            G_Brightness_RESPONSE.Message = "FAIL VCP";
                            Console.WriteLine(G_Brightness_RESPONSE.ToJson());
                            output += "\n" + G_Brightness_RESPONSE.ToJson();
                            IsFailhappened = true;
                        }
                        else
                        {
                            G_Brightness_RESPONSE.Value = $"{rc.value}";
                            if (!_AllInfoMonitors[nidx].CapabilityDic.ContainsKey("12"))
                            {
                                var maxLuminance = GetVCPCodeMax(devMgr, _AllInfoMonitors[nidx], "0x10").Result;
                                if (int.TryParse(rc?.value.ToString(), out int intRcValue) && int.TryParse(maxLuminance?.value.ToString(), out int intMaxLuminance))
                                {
                                    G_Brightness_RESPONSE.Value = $"{(int)((double)intRcValue / intMaxLuminance * 100)}";
                                }
                            }
                            G_Brightness_RESPONSE.Result = "PASS";
                            Console.WriteLine(G_Brightness_RESPONSE.ToJson());
                            output += "\n" + G_Brightness_RESPONSE.ToJson();
                        }
                    }

                    foreach (string tag in serviceTag)
                    {
                        ObjGetVCP rc = new ObjGetVCP();

                        var tmp = _AllInfoMonitors.FindAll(x => x.edid.ServiceTag.ToUpper().Equals(tag.ToUpper()));
                        foreach (MonitorInfo mo in tmp)
                        {
                            writelog($"Brightness get tag entry");
                            rc = GetVCPCode(devMgr, mo, "0x10").Result;

                            G_Brightness_RESPONSE = new CLI_RESPONSE(mo);
                            G_Brightness_RESPONSE.Command = "GET";
                            G_Brightness_RESPONSE.TargetFeature = "BRIGHTNESSLEVEL";//change from brightness to brightnesslevel to align with spec

                            if (!rc.result)
                            {
                                G_Brightness_RESPONSE.Value = "N/A";
                                G_Brightness_RESPONSE.Result = "FAIL";
                                G_Brightness_RESPONSE.Message = "FAIL VCP";
                                Console.WriteLine(G_Brightness_RESPONSE.ToJson());
                                output += "\n" + G_Brightness_RESPONSE.ToJson();
                                IsFailhappened = true;
                            }
                            else
                            {
                                G_Brightness_RESPONSE.Value = $"{rc.value}";
                                if (!mo.CapabilityDic.ContainsKey("12"))
                                {
                                    var maxLuminance = GetVCPCodeMax(devMgr, mo, "0x10").Result;
                                    if (int.TryParse(rc?.value.ToString(), out int intRcValue) && int.TryParse(maxLuminance?.value.ToString(), out int intMaxLuminance))
                                    {
                                        G_Brightness_RESPONSE.Value = $"{(int)((double)intRcValue / intMaxLuminance * 100)}";
                                    }
                                }
                                G_Brightness_RESPONSE.Result = "PASS";
                                Console.WriteLine(G_Brightness_RESPONSE.ToJson());
                                output += "\n" + G_Brightness_RESPONSE.ToJson();
                            }
                        }
                    }

                    foreach (string modelName in model)
                    {
                        ObjGetVCP rc = new ObjGetVCP();

                        var tmp = _AllInfoMonitors.FindAll(x => x.modelName.ToUpper().Equals(modelName.ToUpper()));
                        foreach (MonitorInfo mo in tmp)
                        {
                            writelog($"Brightness get tag entry");
                            rc = GetVCPCode(devMgr, mo, "0x10").Result;

                            G_Brightness_RESPONSE = new CLI_RESPONSE(mo);
                            G_Brightness_RESPONSE.Command = "GET";
                            G_Brightness_RESPONSE.TargetFeature = "BRIGHTNESSLEVEL";//change from brightness to brightnesslevel to align with spec

                            if (!rc.result)
                            {
                                G_Brightness_RESPONSE.Value = "N/A";
                                G_Brightness_RESPONSE.Result = "FAIL";
                                G_Brightness_RESPONSE.Message = "FAIL VCP";
                                Console.WriteLine(G_Brightness_RESPONSE.ToJson());
                                output += "\n" + G_Brightness_RESPONSE.ToJson();
                                IsFailhappened = true;
                            }
                            else
                            {
                                G_Brightness_RESPONSE.Value = $"{rc.value}";
                                if (!mo.CapabilityDic.ContainsKey("12"))
                                {
                                    var maxLuminance = GetVCPCodeMax(devMgr, mo, "0x10").Result;
                                    if (int.TryParse(rc?.value.ToString(), out int intRcValue) && int.TryParse(maxLuminance?.value.ToString(), out int intMaxLuminance))
                                    {
                                        G_Brightness_RESPONSE.Value = $"{(int)((double)intRcValue / intMaxLuminance * 100)}";
                                    }
                                }
                                G_Brightness_RESPONSE.Result = "PASS";
                                Console.WriteLine(G_Brightness_RESPONSE.ToJson());
                                output += "\n" + G_Brightness_RESPONSE.ToJson();
                            }
                        }
                    }

                    if (IsFailhappened)
                    {
                        writelog($"Brightness get fail {output}");
                        return ((int)CLI_ExitCode.fail_SetVCPCapability, output);
                    }
                }
                writelog($"Brightness get return value {output}");
                return ((int)CLI_ExitCode.success, output);
            }
            else
            {
                S_Brightness_RESPONSE.Command = type;
                S_Brightness_RESPONSE.TargetFeature = "BRIGHTNESSLEVEL";
                S_Brightness_RESPONSE.Result = "Format Error";
                S_Brightness_RESPONSE.Message = "Format Error";
                return ((int)CLI_ExitCode.unknow_command, S_Brightness_RESPONSE.ToJson());
            }
        }

        private async Task<(int code, string result)> Contrast(IDeviceManagerSA devMgr, string type, List<string> index, List<string> serviceTag, List<string> model, string value = "")
        {
            CLI_RESPONSE S_Contrast_RESPONSE = new CLI_RESPONSE();
            CLI_RESPONSE G_Contrast_RESPONSE = new CLI_RESPONSE();

            //if (devMgr == null)
            //{
            //    writelog("Contrast: Null IDeviceManagerSA");
            //    return (int)CLI_ExitCode.null_device_manager;
            //}

            if (_AllInfoMonitors == null)
                _AllInfoMonitors = await devMgr.GetMonitors();
            string output = string.Empty;

            if (type == "SET")
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    S_Contrast_RESPONSE.Result = "Format Error";
                    S_Contrast_RESPONSE.Message = "Format Error";
                    return ((int)CLI_ExitCode.fail_FormantError, JsonConvert.SerializeObject(S_Contrast_RESPONSE, Formatting.Indented));
                }

                if (index.Count == 0 && serviceTag.Count == 0 && model.Count == 0)
                {
                    bool rc = false;
                    bool IsFailhappened = false;
                    foreach (MonitorInfo monitor in _AllInfoMonitors)
                    {
                        writelog($"Contrast set entry");
                        S_Contrast_RESPONSE = new CLI_RESPONSE(monitor);
                        if (monitor.CapabilityDic.ContainsKey("12") && Int32.Parse(value) > 100)
                        {
                            S_Contrast_RESPONSE.Result = "Format Error, CONTRASTLEVEL lager then 100";
                            S_Contrast_RESPONSE.Message = "Format Error";
                            return ((int)CLI_ExitCode.fail_FormantError, JsonConvert.SerializeObject(S_Contrast_RESPONSE, Formatting.Indented));
                        }
                        else if (monitor.CapabilityDic.ContainsKey("12") && Int32.Parse(value) < 25)
                        {
                            S_Contrast_RESPONSE.Result = "Format Error, CONTRASTLEVEL samller then 25";
                            S_Contrast_RESPONSE.Message = "Format Error";
                            return ((int)CLI_ExitCode.fail_FormantError, JsonConvert.SerializeObject(S_Contrast_RESPONSE, Formatting.Indented));
                        }
                        else if (monitor.CapabilityDic.ContainsKey("12"))
                        {
                            rc = SetVCPCode(devMgr, monitor, "0x12", value).Result;
                        }
                        else
                        {
                            S_Contrast_RESPONSE.Command = "SET";
                            S_Contrast_RESPONSE.TargetFeature = "CONTRASTLEVEL";//change from contrast to contrastlevel to align with spec
                            S_Contrast_RESPONSE.Value = value;
                            S_Contrast_RESPONSE.Result = "FAIL";
                            S_Contrast_RESPONSE.Message = "This monitor isn't support setting Contrastlevel";
                            System.Console.WriteLine(JsonConvert.SerializeObject(S_Contrast_RESPONSE, Formatting.Indented));
                            output += "\n" + JsonConvert.SerializeObject(S_Contrast_RESPONSE, Formatting.Indented);
                        }

                        S_Contrast_RESPONSE.Command = "SET";
                        S_Contrast_RESPONSE.TargetFeature = "CONTRASTLEVEL";//change from contrast to contrastlevel to align with spec
                        S_Contrast_RESPONSE.Value = value;

                        if (!rc)
                        {
                            S_Contrast_RESPONSE.Result = "FAIL";
                            S_Contrast_RESPONSE.Message = "FAIL VCP";
                            System.Console.WriteLine(JsonConvert.SerializeObject(S_Contrast_RESPONSE, Formatting.Indented));
                            output += "\n" + JsonConvert.SerializeObject(S_Contrast_RESPONSE, Formatting.Indented);
                            IsFailhappened = true;
                        }
                        else
                        {
                            S_Contrast_RESPONSE.Result = "PASS";
                            System.Console.WriteLine(JsonConvert.SerializeObject(S_Contrast_RESPONSE, Formatting.Indented));
                            output += "\n" + JsonConvert.SerializeObject(S_Contrast_RESPONSE, Formatting.Indented);
                        }
                    }

                    if (IsFailhappened)
                        return ((int)CLI_ExitCode.fail_SetVCPCapability, output);
                }
                else
                {
                    bool IsFailhappened = false;
                    foreach (string idx in index)
                    {
                        writelog($"Contrast set idx entry");
                        S_Contrast_RESPONSE = new CLI_RESPONSE(_AllInfoMonitors[Convert.ToInt32(idx)]);

                        bool rc = false;
                        if (_AllInfoMonitors[Convert.ToInt32(idx)].CapabilityDic.ContainsKey("12") && Int32.Parse(value) > 100)
                        {
                            S_Contrast_RESPONSE.Result = "Format Error, CONTRASTLEVEL lager then 100";
                            S_Contrast_RESPONSE.Message = "Format Error";
                            return ((int)CLI_ExitCode.fail_FormantError, JsonConvert.SerializeObject(S_Contrast_RESPONSE, Formatting.Indented));
                        }
                        else if (_AllInfoMonitors[Convert.ToInt32(idx)].CapabilityDic.ContainsKey("12") && Int32.Parse(value) < 25)
                        {
                            S_Contrast_RESPONSE.Result = "Format Error, CONTRASTLEVEL samller then 25";
                            S_Contrast_RESPONSE.Message = "Format Error";
                            return ((int)CLI_ExitCode.fail_FormantError, JsonConvert.SerializeObject(S_Contrast_RESPONSE, Formatting.Indented));
                        }
                        else if (_AllInfoMonitors[Convert.ToInt32(idx)].CapabilityDic.ContainsKey("12"))
                        {
                            rc = SetVCPCode(devMgr, Convert.ToInt32(idx), "0x12", value).Result;
                        }
                        else
                        {
                            S_Contrast_RESPONSE.Command = "SET";
                            S_Contrast_RESPONSE.TargetFeature = "CONTRASTLEVEL";//change from contrast to contrastlevel to align with spec
                            S_Contrast_RESPONSE.Value = value;
                            S_Contrast_RESPONSE.Result = "FAIL";
                            S_Contrast_RESPONSE.Message = "This monitor isn't support setting Contrastlevel";
                            System.Console.WriteLine(JsonConvert.SerializeObject(S_Contrast_RESPONSE, Formatting.Indented));
                            output += "\n" + JsonConvert.SerializeObject(S_Contrast_RESPONSE, Formatting.Indented);
                        }

                        S_Contrast_RESPONSE.Command = "SET";
                        S_Contrast_RESPONSE.TargetFeature = "CONTRASTLEVEL";//change from contrast to contrastlevel to align with spec
                        S_Contrast_RESPONSE.Value = value;

                        if (!rc)
                        {
                            S_Contrast_RESPONSE.Result = "FAIL";
                            S_Contrast_RESPONSE.Message = "FAIL VCP";
                            System.Console.WriteLine(JsonConvert.SerializeObject(S_Contrast_RESPONSE, Formatting.Indented));
                            output += "\n" + JsonConvert.SerializeObject(S_Contrast_RESPONSE, Formatting.Indented);
                            IsFailhappened = true;
                        }
                        else
                        {
                            S_Contrast_RESPONSE.Result = "PASS";
                            System.Console.WriteLine(JsonConvert.SerializeObject(S_Contrast_RESPONSE, Formatting.Indented));
                            output += "\n" + JsonConvert.SerializeObject(S_Contrast_RESPONSE, Formatting.Indented);
                        }
                    }

                    foreach (string tag in serviceTag)
                    {
                        writelog($"Contrast set tag entry");

                        bool rc = false;
                        var tmp = _AllInfoMonitors.FindAll(x => x.edid.ServiceTag.ToUpper().Equals(tag.ToUpper()));
                        foreach (MonitorInfo mo in tmp)
                        {
                            S_Contrast_RESPONSE = new CLI_RESPONSE(mo);

                            if (mo.CapabilityDic.ContainsKey("12") && Int32.Parse(value) > 100)
                            {
                                S_Contrast_RESPONSE.Result = "Format Error, CONTRASTLEVEL lager then 100";
                                S_Contrast_RESPONSE.Message = "Format Error";
                                return ((int)CLI_ExitCode.fail_FormantError, JsonConvert.SerializeObject(S_Contrast_RESPONSE, Formatting.Indented));
                            }
                            else if (mo.CapabilityDic.ContainsKey("12") && Int32.Parse(value) < 25)
                            {
                                S_Contrast_RESPONSE.Result = "Format Error, CONTRASTLEVEL samller then 25";
                                S_Contrast_RESPONSE.Message = "Format Error";
                                return ((int)CLI_ExitCode.fail_FormantError, JsonConvert.SerializeObject(S_Contrast_RESPONSE, Formatting.Indented));
                            }
                            else if (mo.CapabilityDic.ContainsKey("12"))
                            {
                                rc = SetVCPCode(devMgr, mo, "0x12", value).Result;
                            }
                            else
                            {
                                S_Contrast_RESPONSE.Command = "SET";
                                S_Contrast_RESPONSE.TargetFeature = "CONTRASTLEVEL";//change from contrast to contrastlevel to align with spec
                                S_Contrast_RESPONSE.Value = value;
                                S_Contrast_RESPONSE.Result = "FAIL";
                                S_Contrast_RESPONSE.Message = "This monitor isn't support setting Contrastlevel";
                                System.Console.WriteLine(JsonConvert.SerializeObject(S_Contrast_RESPONSE, Formatting.Indented));
                                output += "\n" + JsonConvert.SerializeObject(S_Contrast_RESPONSE, Formatting.Indented);
                            }
                            S_Contrast_RESPONSE.Command = "SET";
                            S_Contrast_RESPONSE.TargetFeature = "CONTRASTLEVEL";//change from contrast to contrastlevel to align with spec
                            S_Contrast_RESPONSE.Value = value;

                            if (!rc)
                            {
                                S_Contrast_RESPONSE.Result = "FAIL";
                                S_Contrast_RESPONSE.Message = "FAIL VCP";
                                System.Console.WriteLine(JsonConvert.SerializeObject(S_Contrast_RESPONSE, Formatting.Indented));
                                output += "\n" + JsonConvert.SerializeObject(S_Contrast_RESPONSE, Formatting.Indented);
                                IsFailhappened = true;
                            }
                            else
                            {
                                S_Contrast_RESPONSE.Result = "PASS";
                                System.Console.WriteLine(JsonConvert.SerializeObject(S_Contrast_RESPONSE, Formatting.Indented));
                                output += "\n" + JsonConvert.SerializeObject(S_Contrast_RESPONSE, Formatting.Indented);
                            }
                        }
                    }

                    foreach (string modelName in model)
                    {
                        writelog($"Contrast set model entry");

                        bool rc = false;
                        var tmp = _AllInfoMonitors.FindAll(x => x.edid.ModelName.ToUpper().Equals(modelName.ToUpper()));
                        foreach (MonitorInfo mo in tmp)
                        {
                            S_Contrast_RESPONSE = new CLI_RESPONSE(mo);

                            if (mo.CapabilityDic.ContainsKey("12") && Int32.Parse(value) > 100)
                            {
                                S_Contrast_RESPONSE.Result = "Format Error, CONTRASTLEVEL lager then 100";
                                S_Contrast_RESPONSE.Message = "Format Error";
                                return ((int)CLI_ExitCode.fail_FormantError, JsonConvert.SerializeObject(S_Contrast_RESPONSE, Formatting.Indented));
                            }
                            else if (mo.CapabilityDic.ContainsKey("12") && Int32.Parse(value) < 25)
                            {
                                S_Contrast_RESPONSE.Result = "Format Error, CONTRASTLEVEL samller then 25";
                                S_Contrast_RESPONSE.Message = "Format Error";
                                return ((int)CLI_ExitCode.fail_FormantError, JsonConvert.SerializeObject(S_Contrast_RESPONSE, Formatting.Indented));
                            }
                            else if (mo.CapabilityDic.ContainsKey("12"))
                            {
                                rc = SetVCPCode(devMgr, mo, "0x12", value).Result;
                            }
                            else
                            {
                                S_Contrast_RESPONSE.Command = "SET";
                                S_Contrast_RESPONSE.TargetFeature = "CONTRASTLEVEL";//change from contrast to contrastlevel to align with spec
                                S_Contrast_RESPONSE.Value = value;
                                S_Contrast_RESPONSE.Result = "FAIL";
                                S_Contrast_RESPONSE.Message = "This monitor isn't support setting Contrastlevel";
                                System.Console.WriteLine(JsonConvert.SerializeObject(S_Contrast_RESPONSE, Formatting.Indented));
                                output += "\n" + JsonConvert.SerializeObject(S_Contrast_RESPONSE, Formatting.Indented);
                            }
                            S_Contrast_RESPONSE.Command = "SET";
                            S_Contrast_RESPONSE.TargetFeature = "CONTRASTLEVEL";//change from contrast to contrastlevel to align with spec
                            S_Contrast_RESPONSE.Value = value;

                            if (!rc)
                            {
                                S_Contrast_RESPONSE.Result = "FAIL";
                                S_Contrast_RESPONSE.Message = "FAIL VCP";
                                System.Console.WriteLine(JsonConvert.SerializeObject(S_Contrast_RESPONSE, Formatting.Indented));
                                output += "\n" + JsonConvert.SerializeObject(S_Contrast_RESPONSE, Formatting.Indented);
                                IsFailhappened = true;
                            }
                            else
                            {
                                S_Contrast_RESPONSE.Result = "PASS";
                                System.Console.WriteLine(JsonConvert.SerializeObject(S_Contrast_RESPONSE, Formatting.Indented));
                                output += "\n" + JsonConvert.SerializeObject(S_Contrast_RESPONSE, Formatting.Indented);
                            }
                        }
                    }
                    if (IsFailhappened)
                    {
                        writelog($"Contrast set fail");
                        return ((int)CLI_ExitCode.fail_SetVCPCapability, output);
                    }
                }
                writelog($"Contrast set return value {output}");
                return ((int)CLI_ExitCode.success, output);
            }
            else if (type == "GET")
            {
                if (index.Count == 0 && serviceTag.Count == 0)
                {
                    ObjGetVCP rc = new ObjGetVCP();
                    bool IsFailhappened = false;
                    foreach (MonitorInfo monitor in _AllInfoMonitors)
                    {
                        writelog($"Contrast get entry");
                        G_Contrast_RESPONSE = new CLI_RESPONSE(monitor);

                        rc = GetVCPCode(devMgr, monitor, "0x12").Result;

                        G_Contrast_RESPONSE.Command = "GET";
                        G_Contrast_RESPONSE.TargetFeature = "CONTRASTLEVEL";//change from contrast to contrastlevel to align with spec

                        if (!rc.result)
                        {
                            //G_Contrast_RESPONSE.Contrast = "N/A";
                            G_Contrast_RESPONSE.Value = "N/A";
                            G_Contrast_RESPONSE.Result = "FAIL";
                            G_Contrast_RESPONSE.Message = "FAIL VCP";
                            System.Console.WriteLine(JsonConvert.SerializeObject(G_Contrast_RESPONSE, Formatting.Indented));
                            output += "\n" + JsonConvert.SerializeObject(G_Contrast_RESPONSE, Formatting.Indented);
                            IsFailhappened = true;
                        }
                        else
                        {
                            G_Contrast_RESPONSE.Value = $"{rc.value}";
                            //G_Contrast_RESPONSE.Contrast = $"{rc.value}";// ((uint)(long)rc.value).ToString();
                            G_Contrast_RESPONSE.Result = "PASS";
                            System.Console.WriteLine(JsonConvert.SerializeObject(G_Contrast_RESPONSE, Formatting.Indented));
                            output += "\n" + JsonConvert.SerializeObject(G_Contrast_RESPONSE, Formatting.Indented);
                        }
                    }

                    if (IsFailhappened)
                    {
                        writelog($"Contrast set fail");
                        return ((int)CLI_ExitCode.fail_SetVCPCapability, output);
                    }
                }
                else
                {
                    bool IsFailhappened = false;
                    foreach (string idx in index)
                    {
                        writelog($"Contrast get idx entry");
                        G_Contrast_RESPONSE = new CLI_RESPONSE(_AllInfoMonitors[Convert.ToInt32(idx)]);

                        ObjGetVCP rc = new ObjGetVCP();

                        rc = GetVCPCode(devMgr, Convert.ToInt32(idx), "0x12").Result;

                        G_Contrast_RESPONSE.Command = "GET";
                        G_Contrast_RESPONSE.TargetFeature = "CONTRASTLEVEL";//change from contrast to contrastlevel to align with spec

                        if (!rc.result)
                        {
                            //G_Contrast_RESPONSE.Contrast = "N/A";
                            G_Contrast_RESPONSE.Value = "N/A";
                            G_Contrast_RESPONSE.Result = "FAIL";
                            G_Contrast_RESPONSE.Message = "FAIL VCP";
                            System.Console.WriteLine(JsonConvert.SerializeObject(G_Contrast_RESPONSE, Formatting.Indented));
                            output += "\n" + JsonConvert.SerializeObject(G_Contrast_RESPONSE, Formatting.Indented);
                            IsFailhappened = true;
                        }
                        else
                        {
                            G_Contrast_RESPONSE.Value = $"{rc.value}";
                            //G_Contrast_RESPONSE.Contrast = $"{rc.value}";// ((uint)(long)rc.value).ToString();
                            G_Contrast_RESPONSE.Result = "PASS";
                            System.Console.WriteLine(JsonConvert.SerializeObject(G_Contrast_RESPONSE, Formatting.Indented));
                            output += "\n" + JsonConvert.SerializeObject(G_Contrast_RESPONSE, Formatting.Indented);
                        }
                    }

                    foreach (string tag in serviceTag)
                    {
                        writelog($"Contrast get tag entry");

                        ObjGetVCP rc = new ObjGetVCP();

                        var tmp = _AllInfoMonitors.FindAll(x => x.edid.ServiceTag.ToUpper().Equals(tag.ToUpper()));
                        foreach (MonitorInfo mo in tmp)
                        {
                            G_Contrast_RESPONSE = new CLI_RESPONSE(mo);

                            rc = GetVCPCode(devMgr, mo, "0x12").Result;

                            G_Contrast_RESPONSE.Command = "GET";
                            G_Contrast_RESPONSE.TargetFeature = "CONTRASTLEVEL";//change from contrast to contrastlevel to align with spec

                            if (!rc.result)
                            {
                                //G_Contrast_RESPONSE.Contrast = "N/A";
                                G_Contrast_RESPONSE.Value = "N/A";
                                G_Contrast_RESPONSE.Result = "FAIL";
                                G_Contrast_RESPONSE.Message = "FAIL VCP";
                                System.Console.WriteLine(JsonConvert.SerializeObject(G_Contrast_RESPONSE, Formatting.Indented));
                                output += "\n" + JsonConvert.SerializeObject(G_Contrast_RESPONSE, Formatting.Indented);
                                IsFailhappened = true;
                            }
                            else
                            {
                                G_Contrast_RESPONSE.Value = $"{rc.value}";
                                //G_Contrast_RESPONSE.Contrast = $"{rc.value}";// ((uint)(long)rc.value).ToString();
                                G_Contrast_RESPONSE.Result = "PASS";
                                System.Console.WriteLine(JsonConvert.SerializeObject(G_Contrast_RESPONSE, Formatting.Indented));
                                output += "\n" + JsonConvert.SerializeObject(G_Contrast_RESPONSE, Formatting.Indented);
                            }
                        }
                    }

                    if (IsFailhappened)
                    {
                        writelog($"Contrast set fail");
                        return ((int)CLI_ExitCode.fail_SetVCPCapability, output);
                    }
                }
                writelog($"Contrast get return value {output}");
                return ((int)CLI_ExitCode.success, output);
            }
            else
            {
                S_Contrast_RESPONSE.Result = "Un-supported command";
                S_Contrast_RESPONSE.Message = "Un-supported command";
                S_Contrast_RESPONSE.Command = type;
                return ((int)CLI_ExitCode.unknow_command, JsonConvert.SerializeObject(S_Contrast_RESPONSE, Formatting.Indented));
            }
        }

        private async Task<(int code, string result)> Luminus(IDeviceManagerSA devMgr, string type, List<string> index, List<string> serviceTag, List<string> model, string value = "")
        {
            CLI_RESPONSE S_Luminus_RESPONSE = new CLI_RESPONSE();
            CLI_Get_Luminus_RESPONSE G_Luminus_RESPONSE = new CLI_Get_Luminus_RESPONSE();

            //if (devMgr == null)
            //{
            //    writelog("Luminus: Null IDeviceManagerSA");
            //    return (int)CLI_ExitCode.null_device_manager;
            //}

            if (_AllInfoMonitors == null)
                _AllInfoMonitors = await devMgr.GetMonitors();

            string output = string.Empty;

            if (type == "SET")
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    S_Luminus_RESPONSE.Result = "Empty value";
                    S_Luminus_RESPONSE.Message = "Empty value";
                    S_Luminus_RESPONSE.Command = type;
                    return ((int)CLI_ExitCode.fail_FormantError, JsonConvert.SerializeObject(S_Luminus_RESPONSE, Formatting.Indented));
                }

                if (index.Count == 0 && serviceTag.Count == 0 && model.Count == 0)
                {
                    bool rc = false;
                    bool IsFailhappened = false;
                    foreach (MonitorInfo monitor in _AllInfoMonitors)
                    {
                        S_Luminus_RESPONSE = new CLI_RESPONSE(monitor);

                        rc = SetVCPCode(devMgr, monitor, "0x10", value).Result;

                        S_Luminus_RESPONSE.Command = "SET";
                        S_Luminus_RESPONSE.TargetFeature = "LUMINUS";
                        S_Luminus_RESPONSE.Value = value;

                        if (!rc)
                        {
                            S_Luminus_RESPONSE.Result = "FAIL";
                            S_Luminus_RESPONSE.Message = "FAIL VCP";
                            System.Console.WriteLine(JsonConvert.SerializeObject(S_Luminus_RESPONSE, Formatting.Indented));
                            output += "\n" + JsonConvert.SerializeObject(S_Luminus_RESPONSE, Formatting.Indented);
                            IsFailhappened = true;
                        }
                        else
                        {
                            S_Luminus_RESPONSE.Result = "PASS";
                            System.Console.WriteLine(JsonConvert.SerializeObject(S_Luminus_RESPONSE, Formatting.Indented));
                            output += "\n" + JsonConvert.SerializeObject(S_Luminus_RESPONSE, Formatting.Indented);
                        }
                    }

                    if (IsFailhappened)
                        return ((int)CLI_ExitCode.fail_SetVCPCapability, output);
                }
                else
                {
                    bool IsFailhappened = false;
                    foreach (string idx in index)
                    {
                        S_Luminus_RESPONSE = new CLI_RESPONSE(_AllInfoMonitors[Convert.ToInt32(idx)]);

                        bool rc = false;

                        rc = SetVCPCode(devMgr, Convert.ToInt32(idx), "0x10", value).Result;

                        S_Luminus_RESPONSE.Command = "SET";
                        S_Luminus_RESPONSE.TargetFeature = "LUMINUS";
                        S_Luminus_RESPONSE.Value = value;

                        if (!rc)
                        {
                            S_Luminus_RESPONSE.Result = "FAIL";
                            S_Luminus_RESPONSE.Message = "FAIL VCP";
                            System.Console.WriteLine(JsonConvert.SerializeObject(S_Luminus_RESPONSE, Formatting.Indented));
                            output += "\n" + JsonConvert.SerializeObject(S_Luminus_RESPONSE, Formatting.Indented);
                            IsFailhappened = true;
                        }
                        else
                        {
                            S_Luminus_RESPONSE.Result = "PASS";
                            System.Console.WriteLine(JsonConvert.SerializeObject(S_Luminus_RESPONSE, Formatting.Indented));
                            output += "\n" + JsonConvert.SerializeObject(S_Luminus_RESPONSE, Formatting.Indented);
                        }
                    }

                    foreach (string tag in serviceTag)
                    {

                        bool rc = false;
                        var tmp = _AllInfoMonitors.FindAll(x => x.edid.ServiceTag.ToUpper().Equals(tag.ToUpper()));
                        foreach (MonitorInfo mo in tmp)
                        {
                            rc = SetVCPCode(devMgr, mo, "0x10", value).Result;

                            S_Luminus_RESPONSE = new CLI_RESPONSE(mo);
                            S_Luminus_RESPONSE.Command = "SET";
                            S_Luminus_RESPONSE.TargetFeature = "LUMINUS";
                            S_Luminus_RESPONSE.Value = value;

                            if (!rc)
                            {
                                S_Luminus_RESPONSE.Result = "FAIL";
                                S_Luminus_RESPONSE.Message = "FAIL VCP";
                                System.Console.WriteLine(JsonConvert.SerializeObject(S_Luminus_RESPONSE, Formatting.Indented));
                                output += "\n" + JsonConvert.SerializeObject(S_Luminus_RESPONSE, Formatting.Indented);
                                IsFailhappened = true;
                            }
                            else
                            {
                                S_Luminus_RESPONSE.Result = "PASS";
                                System.Console.WriteLine(JsonConvert.SerializeObject(S_Luminus_RESPONSE, Formatting.Indented));
                                output += "\n" + JsonConvert.SerializeObject(S_Luminus_RESPONSE, Formatting.Indented);
                            }
                        }
                    }

                    foreach (string modelName in model)
                    {

                        bool rc = false;
                        var tmp = _AllInfoMonitors.FindAll(x => x.edid.ModelName.ToUpper().Equals(modelName.ToUpper()));
                        foreach (MonitorInfo mo in tmp)
                        {
                            rc = SetVCPCode(devMgr, mo, "0x10", value).Result;

                            S_Luminus_RESPONSE = new CLI_RESPONSE(mo);
                            S_Luminus_RESPONSE.Command = "SET";
                            S_Luminus_RESPONSE.TargetFeature = "LUMINUS";
                            S_Luminus_RESPONSE.Value = value;

                            if (!rc)
                            {
                                S_Luminus_RESPONSE.Result = "FAIL";
                                S_Luminus_RESPONSE.Message = "FAIL VCP";
                                System.Console.WriteLine(JsonConvert.SerializeObject(S_Luminus_RESPONSE, Formatting.Indented));
                                output += "\n" + JsonConvert.SerializeObject(S_Luminus_RESPONSE, Formatting.Indented);
                                IsFailhappened = true;
                            }
                            else
                            {
                                S_Luminus_RESPONSE.Result = "PASS";
                                System.Console.WriteLine(JsonConvert.SerializeObject(S_Luminus_RESPONSE, Formatting.Indented));
                                output += "\n" + JsonConvert.SerializeObject(S_Luminus_RESPONSE, Formatting.Indented);
                            }
                        }
                    }
                    if (IsFailhappened)
                        return ((int)CLI_ExitCode.fail_SetVCPCapability, output);
                }
                return ((int)CLI_ExitCode.success, output);
            }
            else if (type == "GET")
            {
                if (index.Count == 0 && serviceTag.Count == 0)
                {
                    ObjGetVCP rc = new ObjGetVCP();
                    bool IsFailhappened = false;
                    foreach (MonitorInfo monitor in _AllInfoMonitors)
                    {
                        G_Luminus_RESPONSE = new CLI_Get_Luminus_RESPONSE(monitor);

                        rc = GetVCPCode(devMgr, monitor, "0x10").Result;

                        //G_Luminus_RESPONSE.Model = monitor.modelName;
                        //G_Luminus_RESPONSE.SerialNumber = monitor.edid.SerialNumber;
                        //G_Luminus_RESPONSE.Index = change_0base_to_1base((monitor.Index).ToString());
                        //G_Luminus_RESPONSE.ServiceTag = monitor.edid.ServiceTag;
                        G_Luminus_RESPONSE.Command = "GET";
                        G_Luminus_RESPONSE.TargetFeature = "LUMINUS";

                        if (!rc.result)
                        {
                            G_Luminus_RESPONSE.Luminus = "N/A";
                            G_Luminus_RESPONSE.Value = "N/A";
                            G_Luminus_RESPONSE.Result = "FAIL";
                            G_Luminus_RESPONSE.Message = "FAIL VCP";
                            System.Console.WriteLine(JsonConvert.SerializeObject(G_Luminus_RESPONSE, Formatting.Indented));
                            output += "\n" + JsonConvert.SerializeObject(G_Luminus_RESPONSE, Formatting.Indented);
                            IsFailhappened = true;
                        }
                        else
                        {
                            G_Luminus_RESPONSE.Value = ((uint)(long)rc.value).ToString();
                            G_Luminus_RESPONSE.Luminus = ((uint)(long)rc.value).ToString();
                            G_Luminus_RESPONSE.Result = "PASS";
                            System.Console.WriteLine(JsonConvert.SerializeObject(G_Luminus_RESPONSE, Formatting.Indented));
                            output += "\n" + JsonConvert.SerializeObject(G_Luminus_RESPONSE, Formatting.Indented);
                        }
                    }

                    if (IsFailhappened)
                        return ((int)CLI_ExitCode.fail_SetVCPCapability, output);
                }
                else
                {
                    bool IsFailhappened = false;
                    foreach (string idx in index)
                    {
                        G_Luminus_RESPONSE = new CLI_Get_Luminus_RESPONSE(_AllInfoMonitors[Convert.ToInt32(idx)]);

                        ObjGetVCP rc = new ObjGetVCP();

                        rc = GetVCPCode(devMgr, Convert.ToInt32(idx), "0x10").Result;

                        //G_Luminus_RESPONSE.Model = _AllInfoMonitors[Convert.ToInt32(idx)].modelName;
                        //G_Luminus_RESPONSE.SerialNumber = _AllInfoMonitors[Convert.ToInt32(idx)].edid.SerialNumber;
                        //G_Luminus_RESPONSE.Index = change_0base_to_1base(idx);
                        //G_Luminus_RESPONSE.ServiceTag = _AllInfoMonitors[Convert.ToInt32(idx)].edid.ServiceTag;
                        G_Luminus_RESPONSE.Command = "GET";
                        G_Luminus_RESPONSE.TargetFeature = "LUMINUS";

                        if (!rc.result)
                        {
                            G_Luminus_RESPONSE.Luminus = "N/A";
                            G_Luminus_RESPONSE.Value = "N/A";
                            G_Luminus_RESPONSE.Result = "FAIL";
                            G_Luminus_RESPONSE.Message = "FAIL VCP";
                            System.Console.WriteLine(JsonConvert.SerializeObject(G_Luminus_RESPONSE, Formatting.Indented));
                            output += "\n" + JsonConvert.SerializeObject(G_Luminus_RESPONSE, Formatting.Indented);
                            IsFailhappened = true;
                        }
                        else
                        {
                            G_Luminus_RESPONSE.Value = $"{rc.value}";
                            G_Luminus_RESPONSE.Luminus = $"{rc.value}";// ((uint)(long)rc.value).ToString();
                            G_Luminus_RESPONSE.Result = "PASS";
                            System.Console.WriteLine(JsonConvert.SerializeObject(G_Luminus_RESPONSE, Formatting.Indented));
                            output += "\n" + JsonConvert.SerializeObject(G_Luminus_RESPONSE, Formatting.Indented);
                        }
                    }

                    foreach (string tag in serviceTag)
                    {
                        ObjGetVCP rc = new ObjGetVCP();

                        var tmp = _AllInfoMonitors.FindAll(x => x.edid.ServiceTag.ToUpper().Equals(tag.ToUpper()));
                        foreach (MonitorInfo mo in tmp)
                        {
                            G_Luminus_RESPONSE = new CLI_Get_Luminus_RESPONSE(mo);
                            rc = GetVCPCode(devMgr, mo, "0x10").Result;

                            //G_Luminus_RESPONSE.Model = mo.modelName;
                            //G_Luminus_RESPONSE.SerialNumber = mo.edid.SerialNumber;
                            //G_Luminus_RESPONSE.Index = change_0base_to_1base((mo.Index).ToString());
                            //G_Luminus_RESPONSE.ServiceTag = mo.edid.ServiceTag;
                            G_Luminus_RESPONSE.Command = "GET";
                            G_Luminus_RESPONSE.TargetFeature = "LUMINUS";

                            if (!rc.result)
                            {
                                G_Luminus_RESPONSE.Luminus = "N/A";
                                G_Luminus_RESPONSE.Value = "N/A";
                                G_Luminus_RESPONSE.Result = "FAIL";
                                G_Luminus_RESPONSE.Message = "FAIL VCP";
                                System.Console.WriteLine(JsonConvert.SerializeObject(G_Luminus_RESPONSE, Formatting.Indented));
                                output += "\n" + JsonConvert.SerializeObject(G_Luminus_RESPONSE, Formatting.Indented);
                                IsFailhappened = true;
                            }
                            else
                            {
                                G_Luminus_RESPONSE.Value = $"{rc.value}";
                                G_Luminus_RESPONSE.Luminus = $"{rc.value}";// ((uint)(long)rc.value).ToString();
                                G_Luminus_RESPONSE.Result = "PASS";
                                System.Console.WriteLine(JsonConvert.SerializeObject(G_Luminus_RESPONSE, Formatting.Indented));
                                output += "\n" + JsonConvert.SerializeObject(G_Luminus_RESPONSE, Formatting.Indented);
                            }
                        }
                    }

                    if (IsFailhappened)
                        return ((int)CLI_ExitCode.fail_SetVCPCapability, output);
                }
                return ((int)CLI_ExitCode.success, output);
            }
            else
            {
                S_Luminus_RESPONSE.Result = "Un-supported command";
                S_Luminus_RESPONSE.Message = "Un-supported command";
                S_Luminus_RESPONSE.Command = type;
                return ((int)CLI_ExitCode.unknow_command, JsonConvert.SerializeObject(S_Luminus_RESPONSE, Formatting.Indented));
            }
        }

        private async Task<(int code, string result)> EDID(IDeviceManagerSA devMgr, string type, List<string> index, List<string> serviceTag, List<string> model, string value = "")
        {
            CLI_Get_EDID_RESPONSE G_EDID_RESPONSE = new CLI_Get_EDID_RESPONSE();
            CLI_Read_EDID_RESPONSE R_EDID_RESPONSE = new CLI_Read_EDID_RESPONSE();

            //if (devMgr == null)
            //{
            //    writelog("EDID: Null IDeviceManagerSA");
            //    return (int)CLI_ExitCode.null_device_manager;
            //}

            if (_AllInfoMonitors == null)
                _AllInfoMonitors = await devMgr.GetMonitors();

            string output = string.Empty;
            if (type == "GET")
            {
                if (index.Count == 0 && serviceTag.Count == 0 && model.Count == 0)
                {
                    bool IsFailhappened = false;
                    foreach (MonitorInfo monitor in _AllInfoMonitors)
                    {
                        G_EDID_RESPONSE = new CLI_Get_EDID_RESPONSE(monitor);

                        G_EDID_RESPONSE.Command = "GET";
                        G_EDID_RESPONSE.TargetFeature = "EDID";

                        if (string.IsNullOrWhiteSpace(monitor.edid.Edid))
                        {
                            G_EDID_RESPONSE.EDID_RAW = "N/A";
                            G_EDID_RESPONSE.Value = "N/A";
                            G_EDID_RESPONSE.Result = "FAIL";
                            G_EDID_RESPONSE.Message = "FAIL VCP";
                            System.Console.WriteLine(JsonConvert.SerializeObject(G_EDID_RESPONSE, Formatting.Indented));
                            output += "\n" + JsonConvert.SerializeObject(G_EDID_RESPONSE, Formatting.Indented);
                            IsFailhappened = true;
                        }
                        else
                        {
                            G_EDID_RESPONSE.Value = "N/A";
                            G_EDID_RESPONSE.EDID_RAW = monitor.edid.Edid;
                            G_EDID_RESPONSE.Result = "PASS";
                            System.Console.WriteLine(JsonConvert.SerializeObject(G_EDID_RESPONSE, Formatting.Indented));
                            output += "\n" + JsonConvert.SerializeObject(G_EDID_RESPONSE, Formatting.Indented);
                        }
                    }

                    if (IsFailhappened)
                        return ((int)CLI_ExitCode.fail_SetVCPCapability, output);
                }
                else
                {
                    bool IsFailhappened = false;
                    foreach (string idx in index)
                    {
                        if (Convert.ToInt32(idx) < _AllInfoMonitors.Count)
                        {
                            G_EDID_RESPONSE = new CLI_Get_EDID_RESPONSE(_AllInfoMonitors[Convert.ToInt32(idx)]);
                            G_EDID_RESPONSE.Command = "GET";
                            G_EDID_RESPONSE.TargetFeature = "EDID";

                            if (string.IsNullOrWhiteSpace(_AllInfoMonitors[Convert.ToInt32(idx)].edid.Edid))
                            {
                                G_EDID_RESPONSE.EDID_RAW = "N/A";
                                G_EDID_RESPONSE.Value = "N/A";
                                G_EDID_RESPONSE.Result = "FAIL";
                                G_EDID_RESPONSE.Message = "FAIL GET EDID";
                                System.Console.WriteLine(JsonConvert.SerializeObject(G_EDID_RESPONSE, Formatting.Indented));
                                output += "\n" + JsonConvert.SerializeObject(G_EDID_RESPONSE, Formatting.Indented);
                                IsFailhappened = true;
                            }
                            else
                            {
                                G_EDID_RESPONSE.Value = "N/A";
                                G_EDID_RESPONSE.EDID_RAW = _AllInfoMonitors[Convert.ToInt32(idx)].edid.Edid;
                                G_EDID_RESPONSE.Result = "PASS";
                                System.Console.WriteLine(JsonConvert.SerializeObject(G_EDID_RESPONSE, Formatting.Indented));
                                output += "\n" + JsonConvert.SerializeObject(G_EDID_RESPONSE, Formatting.Indented);
                            }
                        }
                        else
                        {
                            G_EDID_RESPONSE.Model = "N/A";
                            G_EDID_RESPONSE.SerialNumber = "N/A";
                            G_EDID_RESPONSE.Index = change_0base_to_1base(idx);
                            G_EDID_RESPONSE.ServiceTag = "N/A";
                            G_EDID_RESPONSE.Command = "GET";
                            G_EDID_RESPONSE.TargetFeature = "EDID";
                            G_EDID_RESPONSE.EDID_RAW = "N/A";
                            G_EDID_RESPONSE.Value = "N/A";
                            G_EDID_RESPONSE.Result = "FAIL";
                            G_EDID_RESPONSE.Message = "Index Out of Range";
                            System.Console.WriteLine(JsonConvert.SerializeObject(G_EDID_RESPONSE, Formatting.Indented));
                            output += "\n" + JsonConvert.SerializeObject(G_EDID_RESPONSE, Formatting.Indented);
                            IsFailhappened = true;
                        }
                    }

                    foreach (string tag in serviceTag)
                    {
                        var tmp = _AllInfoMonitors.FindAll(x => x.edid.ServiceTag.ToUpper().Equals(tag.ToUpper()));
                        foreach (MonitorInfo mo in tmp)
                        {
                            G_EDID_RESPONSE = new CLI_Get_EDID_RESPONSE(mo);

                            G_EDID_RESPONSE.Command = "GET";
                            G_EDID_RESPONSE.TargetFeature = "EDID";

                            if (string.IsNullOrWhiteSpace(mo.edid.Edid))
                            {
                                G_EDID_RESPONSE.EDID_RAW = "N/A";
                                G_EDID_RESPONSE.Value = "N/A";
                                G_EDID_RESPONSE.Result = "FAIL";
                                G_EDID_RESPONSE.Message = "FAIL GET EDID";
                                System.Console.WriteLine(JsonConvert.SerializeObject(G_EDID_RESPONSE, Formatting.Indented));
                                output += "\n" + JsonConvert.SerializeObject(G_EDID_RESPONSE, Formatting.Indented);
                                IsFailhappened = true;
                            }
                            else
                            {
                                G_EDID_RESPONSE.Value = "N/A";
                                G_EDID_RESPONSE.EDID_RAW = mo.edid.Edid;
                                G_EDID_RESPONSE.Result = "PASS";
                                System.Console.WriteLine(JsonConvert.SerializeObject(G_EDID_RESPONSE, Formatting.Indented));
                                output += "\n" + JsonConvert.SerializeObject(G_EDID_RESPONSE, Formatting.Indented);
                            }
                        }
                    }

                    foreach (string modelName in model)
                    {
                        var tmp = _AllInfoMonitors.FindAll(x => x.edid.ModelName.ToUpper().Equals(modelName.ToUpper()));
                        foreach (MonitorInfo mo in tmp)
                        {
                            G_EDID_RESPONSE = new CLI_Get_EDID_RESPONSE(mo);

                            G_EDID_RESPONSE.Command = "GET";
                            G_EDID_RESPONSE.TargetFeature = "EDID";

                            if (string.IsNullOrWhiteSpace(mo.edid.Edid))
                            {
                                G_EDID_RESPONSE.EDID_RAW = "N/A";
                                G_EDID_RESPONSE.Value = "N/A";
                                G_EDID_RESPONSE.Result = "FAIL";
                                G_EDID_RESPONSE.Message = "FAIL GET EDID";
                                System.Console.WriteLine(JsonConvert.SerializeObject(G_EDID_RESPONSE, Formatting.Indented));
                                output += "\n" + JsonConvert.SerializeObject(G_EDID_RESPONSE, Formatting.Indented);
                                IsFailhappened = true;
                            }
                            else
                            {
                                G_EDID_RESPONSE.Value = "N/A";
                                G_EDID_RESPONSE.EDID_RAW = mo.edid.Edid;
                                G_EDID_RESPONSE.Result = "PASS";
                                System.Console.WriteLine(JsonConvert.SerializeObject(G_EDID_RESPONSE, Formatting.Indented));
                                output += "\n" + JsonConvert.SerializeObject(G_EDID_RESPONSE, Formatting.Indented);
                            }
                        }
                    }

                    if (IsFailhappened)
                        return ((int)CLI_ExitCode.fail_SetVCPCapability, output);
                }
                return ((int)CLI_ExitCode.success, output);
            }
            else if (type == "READ")
            {
                if (index.Count == 0 && serviceTag.Count == 0)
                {
                    bool IsFailhappened = false;
                    foreach (MonitorInfo monitor in _AllInfoMonitors)
                    {
                        R_EDID_RESPONSE = new CLI_Read_EDID_RESPONSE(monitor);

                        R_EDID_RESPONSE.Command = "READ";
                        R_EDID_RESPONSE.TargetFeature = "EDID";

                        if (string.IsNullOrEmpty(monitor.edid.Edid))
                        {
                            R_EDID_RESPONSE.Value = "N/A";
                            R_EDID_RESPONSE.Result = "FAIL";
                            R_EDID_RESPONSE.Message = "FAIL GET EDID";
                            System.Console.WriteLine(JsonConvert.SerializeObject(R_EDID_RESPONSE, Formatting.Indented));
                            output += "\n" + JsonConvert.SerializeObject(R_EDID_RESPONSE, Formatting.Indented);
                            IsFailhappened = true;
                        }
                        else
                        {
                            R_EDID_RESPONSE.Value = "N/A";
                            R_EDID_RESPONSE.AliasDeviceName = monitor.modelName;
                            R_EDID_RESPONSE.Edid = monitor.edid.Edid;
                            R_EDID_RESPONSE.ManufactureID = monitor.edid.ManufactureID;
                            R_EDID_RESPONSE.VendorID = monitor.edid.VendorID;
                            R_EDID_RESPONSE.ModelName = monitor.edid.ModelName;
                            R_EDID_RESPONSE.SerialNumber = monitor.edid.SerialNumber;
                            R_EDID_RESPONSE.Week = monitor.edid.Week.ToString();
                            R_EDID_RESPONSE.Month = monitor.edid.Month.ToString();
                            R_EDID_RESPONSE.Year = monitor.edid.Year.ToString();
                            R_EDID_RESPONSE.EdidVersion = monitor.edid.EdidVersion;
                            R_EDID_RESPONSE.VideoInputType = monitor.edid.VideoInputType;
                            R_EDID_RESPONSE.Size = monitor.edid.Size.ToString();
                            R_EDID_RESPONSE.Result = "PASS";
                            System.Console.WriteLine(JsonConvert.SerializeObject(R_EDID_RESPONSE, Formatting.Indented));
                            output += "\n" + JsonConvert.SerializeObject(R_EDID_RESPONSE, Formatting.Indented);
                        }
                    }

                    if (IsFailhappened)
                        return ((int)CLI_ExitCode.fail_SetVCPCapability, output);
                }
                else
                {
                    bool IsFailhappened = false;
                    foreach (string idx in index)
                    {
                        if (Convert.ToInt32(idx) < _AllInfoMonitors.Count)
                        {
                            R_EDID_RESPONSE = new CLI_Read_EDID_RESPONSE(_AllInfoMonitors[Convert.ToInt32(idx)]);
                            //R_EDID_RESPONSE.Model = _AllInfoMonitors[Convert.ToInt32(idx)].modelName;
                            //R_EDID_RESPONSE.SerialNumber = _AllInfoMonitors[Convert.ToInt32(idx)].edid.SerialNumber;
                            //R_EDID_RESPONSE.Index = change_0base_to_1base(idx);
                            //R_EDID_RESPONSE.ServiceTag = _AllInfoMonitors[Convert.ToInt32(idx)].edid.ServiceTag;
                            R_EDID_RESPONSE.Command = "READ";
                            R_EDID_RESPONSE.TargetFeature = "EDID";

                            if (string.IsNullOrEmpty(_AllInfoMonitors[Convert.ToInt32(idx)].edid.Edid))
                            {
                                R_EDID_RESPONSE.Value = "N/A";
                                R_EDID_RESPONSE.Result = "FAIL";
                                R_EDID_RESPONSE.Message = "FAIL GET EDID";
                                System.Console.WriteLine(JsonConvert.SerializeObject(R_EDID_RESPONSE, Formatting.Indented));
                                output += "\n" + JsonConvert.SerializeObject(R_EDID_RESPONSE, Formatting.Indented);
                                IsFailhappened = true;
                            }
                            else
                            {
                                R_EDID_RESPONSE.Value = "N/A";
                                R_EDID_RESPONSE.AliasDeviceName = _AllInfoMonitors[Convert.ToInt32(idx)].modelName;
                                R_EDID_RESPONSE.Edid = _AllInfoMonitors[Convert.ToInt32(idx)].edid.Edid;
                                R_EDID_RESPONSE.ManufactureID = _AllInfoMonitors[Convert.ToInt32(idx)].edid.ManufactureID;
                                R_EDID_RESPONSE.VendorID = _AllInfoMonitors[Convert.ToInt32(idx)].edid.VendorID;
                                R_EDID_RESPONSE.ModelName = _AllInfoMonitors[Convert.ToInt32(idx)].edid.ModelName;
                                R_EDID_RESPONSE.SerialNumber = _AllInfoMonitors[Convert.ToInt32(idx)].edid.SerialNumber;
                                R_EDID_RESPONSE.Week = _AllInfoMonitors[Convert.ToInt32(idx)].edid.Week.ToString();
                                R_EDID_RESPONSE.Month = _AllInfoMonitors[Convert.ToInt32(idx)].edid.Month.ToString();
                                R_EDID_RESPONSE.Year = _AllInfoMonitors[Convert.ToInt32(idx)].edid.Year.ToString();
                                R_EDID_RESPONSE.EdidVersion = _AllInfoMonitors[Convert.ToInt32(idx)].edid.EdidVersion;
                                R_EDID_RESPONSE.VideoInputType = _AllInfoMonitors[Convert.ToInt32(idx)].edid.VideoInputType;
                                R_EDID_RESPONSE.Size = _AllInfoMonitors[Convert.ToInt32(idx)].edid.Size.ToString();
                                R_EDID_RESPONSE.Result = "PASS";
                                System.Console.WriteLine(JsonConvert.SerializeObject(R_EDID_RESPONSE, Formatting.Indented));
                                output += "\n" + JsonConvert.SerializeObject(R_EDID_RESPONSE, Formatting.Indented);
                            }
                        }
                        else
                        {
                            R_EDID_RESPONSE.Model = "N/A";
                            R_EDID_RESPONSE.SerialNumber = "N/A";
                            R_EDID_RESPONSE.Index = change_0base_to_1base(idx);
                            R_EDID_RESPONSE.ServiceTag = "N/A";
                            R_EDID_RESPONSE.Command = "READ";
                            R_EDID_RESPONSE.TargetFeature = "EDID";
                            R_EDID_RESPONSE.Value = "N/A";
                            R_EDID_RESPONSE.Result = "FAIL";
                            R_EDID_RESPONSE.Message = "Index Out of Range";
                            System.Console.WriteLine(JsonConvert.SerializeObject(R_EDID_RESPONSE, Formatting.Indented));
                            output += "\n" + JsonConvert.SerializeObject(R_EDID_RESPONSE, Formatting.Indented);
                            IsFailhappened = true;
                        }
                    }

                    foreach (string tag in serviceTag)
                    {
                        var tmp = _AllInfoMonitors.FindAll(x => x.edid.ServiceTag.ToUpper().Equals(tag.ToUpper()));
                        foreach (MonitorInfo monitor in tmp)
                        {
                            R_EDID_RESPONSE = new CLI_Read_EDID_RESPONSE(monitor);

                            //R_EDID_RESPONSE.Model = monitor.modelName;
                            //R_EDID_RESPONSE.SerialNumber = monitor.edid.SerialNumber;
                            //R_EDID_RESPONSE.Index = change_0base_to_1base((monitor.Index).ToString());
                            //R_EDID_RESPONSE.ServiceTag = monitor.edid.ServiceTag;
                            R_EDID_RESPONSE.Command = "READ";
                            R_EDID_RESPONSE.TargetFeature = "EDID";

                            if (string.IsNullOrEmpty(monitor.edid.Edid))
                            {
                                R_EDID_RESPONSE.Value = "N/A";
                                R_EDID_RESPONSE.Result = "FAIL";
                                R_EDID_RESPONSE.Message = "FAIL GET EDID";
                                System.Console.WriteLine(JsonConvert.SerializeObject(R_EDID_RESPONSE, Formatting.Indented));
                                output += "\n" + JsonConvert.SerializeObject(R_EDID_RESPONSE, Formatting.Indented);
                                IsFailhappened = true;
                            }
                            else
                            {
                                R_EDID_RESPONSE.Value = "N/A";
                                R_EDID_RESPONSE.AliasDeviceName = monitor.modelName;
                                R_EDID_RESPONSE.Edid = monitor.edid.Edid;
                                R_EDID_RESPONSE.ManufactureID = monitor.edid.ManufactureID;
                                R_EDID_RESPONSE.VendorID = monitor.edid.VendorID;
                                R_EDID_RESPONSE.ModelName = monitor.edid.ModelName;
                                R_EDID_RESPONSE.SerialNumber = monitor.edid.SerialNumber;
                                R_EDID_RESPONSE.Week = monitor.edid.Week.ToString();
                                R_EDID_RESPONSE.Month = monitor.edid.Month.ToString();
                                R_EDID_RESPONSE.Year = monitor.edid.Year.ToString();
                                R_EDID_RESPONSE.EdidVersion = monitor.edid.EdidVersion;
                                R_EDID_RESPONSE.VideoInputType = monitor.edid.VideoInputType;
                                R_EDID_RESPONSE.Size = monitor.edid.Size.ToString();
                                R_EDID_RESPONSE.Result = "PASS";
                                System.Console.WriteLine(JsonConvert.SerializeObject(R_EDID_RESPONSE, Formatting.Indented));
                                output += "\n" + JsonConvert.SerializeObject(R_EDID_RESPONSE, Formatting.Indented);
                            }
                        }
                    }

                    if (IsFailhappened)
                        return ((int)CLI_ExitCode.fail_SetVCPCapability, output);
                }
                return ((int)CLI_ExitCode.success, output);
            }
            else
            {
                G_EDID_RESPONSE = new CLI_Get_EDID_RESPONSE();
                G_EDID_RESPONSE.Model = "N/A";
                G_EDID_RESPONSE.SerialNumber = "N/A";
                G_EDID_RESPONSE.Index = "N/A";
                G_EDID_RESPONSE.ServiceTag = "N/A";
                G_EDID_RESPONSE.Command = type;
                G_EDID_RESPONSE.TargetFeature = "EDID";
                G_EDID_RESPONSE.EDID_RAW = "N/A";
                G_EDID_RESPONSE.Value = "N/A";
                G_EDID_RESPONSE.Result = "Un-supported command";
                G_EDID_RESPONSE.Message = "Un-supported command";
                return ((int)CLI_ExitCode.unknow_command, JsonConvert.SerializeObject(G_EDID_RESPONSE, Formatting.Indented));
            }
        }

        private async Task<(int code, string result)> DECODEDEDID(IDeviceManagerSA devMgr, string type, List<string> index, List<string> serviceTag, List<string> model, string value = "")
        {
            CLI_Get_EDID_RESPONSE G_EDID_RESPONSE = new CLI_Get_EDID_RESPONSE();
            CLI_Read_EDID_RESPONSE R_EDID_RESPONSE = new CLI_Read_EDID_RESPONSE();

            //if (devMgr == null)
            //{
            //    writelog("EDID: Null IDeviceManagerSA");
            //    return (int)CLI_ExitCode.null_device_manager;
            //}

            if (_AllInfoMonitors == null)
                _AllInfoMonitors = await devMgr.GetMonitors();

            string output = string.Empty;
            if (type == "GET")
            {
                if (index.Count == 0 && serviceTag.Count == 0 && model.Count == 0)
                {
                    bool IsFailhappened = false;
                    foreach (MonitorInfo monitor in _AllInfoMonitors)
                    {
                        R_EDID_RESPONSE = new CLI_Read_EDID_RESPONSE(monitor);

                        //R_EDID_RESPONSE.Model = monitor.modelName;
                        //R_EDID_RESPONSE.SerialNumber = monitor.edid.SerialNumber;
                        //R_EDID_RESPONSE.Index = change_0base_to_1base((monitor.Index).ToString());
                        //R_EDID_RESPONSE.ServiceTag = monitor.edid.ServiceTag;
                        R_EDID_RESPONSE.Command = "GET";
                        R_EDID_RESPONSE.TargetFeature = "DECODEDEDID";

                        if (string.IsNullOrEmpty(monitor.edid.Edid))
                        {
                            R_EDID_RESPONSE.Value = "N/A";
                            R_EDID_RESPONSE.Result = "FAIL";
                            R_EDID_RESPONSE.Message = "FAIL GET DECODEDEDID";
                            System.Console.WriteLine(JsonConvert.SerializeObject(R_EDID_RESPONSE, Formatting.Indented));
                            output += "\n" + JsonConvert.SerializeObject(R_EDID_RESPONSE, Formatting.Indented);
                            IsFailhappened = true;
                        }
                        else
                        {
                            R_EDID_RESPONSE.Value = "N/A";
                            R_EDID_RESPONSE.AliasDeviceName = monitor.modelName;
                            R_EDID_RESPONSE.Edid = monitor.edid.Edid;
                            R_EDID_RESPONSE.ManufactureID = monitor.edid.ManufactureID;
                            R_EDID_RESPONSE.VendorID = monitor.edid.VendorID;
                            R_EDID_RESPONSE.PID = monitor.edid.PID;
                            R_EDID_RESPONSE.ModelName = monitor.edid.ModelName;
                            R_EDID_RESPONSE.SerialNumber = monitor.edid.SerialNumber;
                            R_EDID_RESPONSE.Week = monitor.edid.Week.ToString();
                            R_EDID_RESPONSE.Month = monitor.edid.Month.ToString();
                            R_EDID_RESPONSE.Year = monitor.edid.Year.ToString();
                            R_EDID_RESPONSE.EdidVersion = monitor.edid.EdidVersion;
                            R_EDID_RESPONSE.VideoInputType = monitor.edid.VideoInputType;
                            R_EDID_RESPONSE.Size = monitor.edid.Size.ToString();
                            R_EDID_RESPONSE.Result = "PASS";
                            System.Console.WriteLine(JsonConvert.SerializeObject(R_EDID_RESPONSE, Formatting.Indented));
                            output += "\n" + JsonConvert.SerializeObject(R_EDID_RESPONSE, Formatting.Indented);
                        }
                    }

                    if (IsFailhappened)
                        return ((int)CLI_ExitCode.fail_SetVCPCapability, output);
                }
                else
                {
                    bool IsFailhappened = false;
                    foreach (string idx in index)
                    {
                        if (Convert.ToInt32(idx) < _AllInfoMonitors.Count)
                        {
                            R_EDID_RESPONSE = new CLI_Read_EDID_RESPONSE(_AllInfoMonitors[Convert.ToInt32(idx)]);
                            //R_EDID_RESPONSE.Model = _AllInfoMonitors[Convert.ToInt32(idx)].modelName;
                            //R_EDID_RESPONSE.SerialNumber = _AllInfoMonitors[Convert.ToInt32(idx)].edid.SerialNumber;
                            //R_EDID_RESPONSE.Index = change_0base_to_1base(idx);
                            //R_EDID_RESPONSE.ServiceTag = _AllInfoMonitors[Convert.ToInt32(idx)].edid.ServiceTag;
                            R_EDID_RESPONSE.Command = "GET";
                            R_EDID_RESPONSE.TargetFeature = "DECODEDEDID";

                            if (string.IsNullOrEmpty(_AllInfoMonitors[Convert.ToInt32(idx)].edid.Edid))
                            {
                                R_EDID_RESPONSE.Value = "N/A";
                                R_EDID_RESPONSE.Result = "FAIL";
                                R_EDID_RESPONSE.Message = "FAIL GET DECODEDEDID";
                                System.Console.WriteLine(JsonConvert.SerializeObject(R_EDID_RESPONSE, Formatting.Indented));
                                output += "\n" + JsonConvert.SerializeObject(R_EDID_RESPONSE, Formatting.Indented);
                                IsFailhappened = true;
                            }
                            else
                            {
                                R_EDID_RESPONSE.Value = "N/A";
                                R_EDID_RESPONSE.AliasDeviceName = _AllInfoMonitors[Convert.ToInt32(idx)].modelName;
                                R_EDID_RESPONSE.Edid = _AllInfoMonitors[Convert.ToInt32(idx)].edid.Edid;
                                R_EDID_RESPONSE.ManufactureID = _AllInfoMonitors[Convert.ToInt32(idx)].edid.ManufactureID;
                                R_EDID_RESPONSE.VendorID = _AllInfoMonitors[Convert.ToInt32(idx)].edid.VendorID;
                                R_EDID_RESPONSE.PID = _AllInfoMonitors[Convert.ToInt32(idx)].edid.PID;
                                R_EDID_RESPONSE.ModelName = _AllInfoMonitors[Convert.ToInt32(idx)].edid.ModelName;
                                R_EDID_RESPONSE.SerialNumber = _AllInfoMonitors[Convert.ToInt32(idx)].edid.SerialNumber;
                                R_EDID_RESPONSE.Week = _AllInfoMonitors[Convert.ToInt32(idx)].edid.Week.ToString();
                                R_EDID_RESPONSE.Month = _AllInfoMonitors[Convert.ToInt32(idx)].edid.Month.ToString();
                                R_EDID_RESPONSE.Year = _AllInfoMonitors[Convert.ToInt32(idx)].edid.Year.ToString();
                                R_EDID_RESPONSE.EdidVersion = _AllInfoMonitors[Convert.ToInt32(idx)].edid.EdidVersion;
                                R_EDID_RESPONSE.VideoInputType = _AllInfoMonitors[Convert.ToInt32(idx)].edid.VideoInputType;
                                R_EDID_RESPONSE.Size = _AllInfoMonitors[Convert.ToInt32(idx)].edid.Size.ToString();
                                R_EDID_RESPONSE.Result = "PASS";
                                System.Console.WriteLine(JsonConvert.SerializeObject(R_EDID_RESPONSE, Formatting.Indented));
                                output += "\n" + JsonConvert.SerializeObject(R_EDID_RESPONSE, Formatting.Indented);
                            }
                        }
                        else
                        {
                            R_EDID_RESPONSE.Model = "N/A";
                            R_EDID_RESPONSE.SerialNumber = "N/A";
                            R_EDID_RESPONSE.Index = change_0base_to_1base(idx);
                            R_EDID_RESPONSE.ServiceTag = "N/A";
                            R_EDID_RESPONSE.Command = "GET";
                            R_EDID_RESPONSE.TargetFeature = "DECODEDEDID";
                            R_EDID_RESPONSE.Value = "N/A";
                            R_EDID_RESPONSE.Result = "FAIL";
                            R_EDID_RESPONSE.Message = "Index Out of Range";
                            System.Console.WriteLine(JsonConvert.SerializeObject(R_EDID_RESPONSE, Formatting.Indented));
                            output += "\n" + JsonConvert.SerializeObject(R_EDID_RESPONSE, Formatting.Indented);
                            IsFailhappened = true;
                        }
                    }

                    foreach (string tag in serviceTag)
                    {
                        var tmp = _AllInfoMonitors.FindAll(x => x.edid.ServiceTag.ToUpper().Equals(tag.ToUpper()));
                        foreach (MonitorInfo monitor in tmp)
                        {
                            R_EDID_RESPONSE = new CLI_Read_EDID_RESPONSE(monitor);

                            //R_EDID_RESPONSE.Model = monitor.modelName;
                            //R_EDID_RESPONSE.SerialNumber = monitor.edid.SerialNumber;
                            //R_EDID_RESPONSE.Index = change_0base_to_1base((monitor.Index).ToString());
                            //R_EDID_RESPONSE.ServiceTag = monitor.edid.ServiceTag;
                            R_EDID_RESPONSE.Command = "GET";
                            R_EDID_RESPONSE.TargetFeature = "DECODEDEDID";

                            if (string.IsNullOrEmpty(monitor.edid.Edid))
                            {
                                R_EDID_RESPONSE.Value = "N/A";
                                R_EDID_RESPONSE.Result = "FAIL";
                                R_EDID_RESPONSE.Message = "FAIL GET DECODEDEDID";
                                System.Console.WriteLine(JsonConvert.SerializeObject(R_EDID_RESPONSE, Formatting.Indented));
                                output += "\n" + JsonConvert.SerializeObject(R_EDID_RESPONSE, Formatting.Indented);
                                IsFailhappened = true;
                            }
                            else
                            {
                                R_EDID_RESPONSE.Value = "N/A";
                                R_EDID_RESPONSE.AliasDeviceName = monitor.modelName;
                                R_EDID_RESPONSE.Edid = monitor.edid.Edid;
                                R_EDID_RESPONSE.ManufactureID = monitor.edid.ManufactureID;
                                R_EDID_RESPONSE.VendorID = monitor.edid.VendorID;
                                R_EDID_RESPONSE.PID = monitor.edid.PID;
                                R_EDID_RESPONSE.ModelName = monitor.edid.ModelName;
                                R_EDID_RESPONSE.SerialNumber = monitor.edid.SerialNumber;
                                R_EDID_RESPONSE.Week = monitor.edid.Week.ToString();
                                R_EDID_RESPONSE.Month = monitor.edid.Month.ToString();
                                R_EDID_RESPONSE.Year = monitor.edid.Year.ToString();
                                R_EDID_RESPONSE.EdidVersion = monitor.edid.EdidVersion;
                                R_EDID_RESPONSE.VideoInputType = monitor.edid.VideoInputType;
                                R_EDID_RESPONSE.Size = monitor.edid.Size.ToString();
                                R_EDID_RESPONSE.Result = "PASS";
                                System.Console.WriteLine(JsonConvert.SerializeObject(R_EDID_RESPONSE, Formatting.Indented));
                                output += "\n" + JsonConvert.SerializeObject(R_EDID_RESPONSE, Formatting.Indented);
                            }
                        }
                    }

                    foreach (string modelName in model)
                    {
                        var tmp = _AllInfoMonitors.FindAll(x => x.edid.ModelName.ToUpper().Equals(modelName.ToUpper()));
                        foreach (MonitorInfo monitor in tmp)
                        {
                            R_EDID_RESPONSE = new CLI_Read_EDID_RESPONSE(monitor);

                            //R_EDID_RESPONSE.Model = monitor.edid.ModelName;
                            //R_EDID_RESPONSE.SerialNumber = monitor.edid.SerialNumber;
                            //R_EDID_RESPONSE.Index = change_0base_to_1base((monitor.Index).ToString());
                            //R_EDID_RESPONSE.ServiceTag = monitor.edid.ServiceTag;
                            R_EDID_RESPONSE.Command = "GET";
                            R_EDID_RESPONSE.TargetFeature = "DECODEDEDID";

                            if (string.IsNullOrEmpty(monitor.edid.Edid))
                            {
                                R_EDID_RESPONSE.Value = "N/A";
                                R_EDID_RESPONSE.Result = "FAIL";
                                R_EDID_RESPONSE.Message = "FAIL GET DECODEDEDID";
                                System.Console.WriteLine(JsonConvert.SerializeObject(R_EDID_RESPONSE, Formatting.Indented));
                                output += "\n" + JsonConvert.SerializeObject(R_EDID_RESPONSE, Formatting.Indented);
                                IsFailhappened = true;
                            }
                            else
                            {
                                R_EDID_RESPONSE.Value = "N/A";
                                R_EDID_RESPONSE.AliasDeviceName = monitor.modelName;
                                R_EDID_RESPONSE.Edid = monitor.edid.Edid;
                                R_EDID_RESPONSE.ManufactureID = monitor.edid.ManufactureID;
                                R_EDID_RESPONSE.VendorID = monitor.edid.VendorID;
                                R_EDID_RESPONSE.PID = monitor.edid.PID;
                                R_EDID_RESPONSE.ModelName = monitor.edid.ModelName;
                                R_EDID_RESPONSE.SerialNumber = monitor.edid.SerialNumber;
                                R_EDID_RESPONSE.Week = monitor.edid.Week.ToString();
                                R_EDID_RESPONSE.Month = monitor.edid.Month.ToString();
                                R_EDID_RESPONSE.Year = monitor.edid.Year.ToString();
                                R_EDID_RESPONSE.EdidVersion = monitor.edid.EdidVersion;
                                R_EDID_RESPONSE.VideoInputType = monitor.edid.VideoInputType;
                                R_EDID_RESPONSE.Size = monitor.edid.Size.ToString();
                                R_EDID_RESPONSE.Result = "PASS";
                                System.Console.WriteLine(JsonConvert.SerializeObject(R_EDID_RESPONSE, Formatting.Indented));
                                output += "\n" + JsonConvert.SerializeObject(R_EDID_RESPONSE, Formatting.Indented);
                            }
                        }
                    }

                    if (IsFailhappened)
                        return ((int)CLI_ExitCode.fail_SetVCPCapability, output);
                }
                return ((int)CLI_ExitCode.success, output);
            }
            else
            {
                G_EDID_RESPONSE = new CLI_Get_EDID_RESPONSE();
                G_EDID_RESPONSE.Model = "N/A";
                G_EDID_RESPONSE.SerialNumber = "N/A";
                G_EDID_RESPONSE.Index = "N/A";
                G_EDID_RESPONSE.ServiceTag = "N/A";
                G_EDID_RESPONSE.Command = type;
                G_EDID_RESPONSE.TargetFeature = "DECODEDEDID";
                G_EDID_RESPONSE.EDID_RAW = "N/A";
                G_EDID_RESPONSE.Value = "N/A";
                G_EDID_RESPONSE.Result = "Un-supported command";
                G_EDID_RESPONSE.Message = "Un-supported command";
                return ((int)CLI_ExitCode.unknow_command, JsonConvert.SerializeObject(G_EDID_RESPONSE, Formatting.Indented));
            }
        }

        private async Task<(int code, string result)> GetMonitors(IDeviceManagerSA devMgr, string type, List<string> index, List<string> serviceTag, List<string> model, string value = "")
        {
            CLI_Get_MONITORS_RESPONSE G_Monitos_RESPONSE = new CLI_Get_MONITORS_RESPONSE();
            //G_Monitos_RESPONSE.Message = "No devices found";

            //if (devMgr == null)
            //{
            //    writelog("FWVersion: Null IDeviceManagerSA");
            //    return (int)CLI_ExitCode.null_device_manager;
            //}

            if (_AllInfoMonitors == null)
                _AllInfoMonitors = await devMgr.GetMonitors();

            if (type == "GET")
            {
                if (_AllInfoMonitors.Count > 0)
                {
                    G_Monitos_RESPONSE.Command = "GET";
                    G_Monitos_RESPONSE.Result = "PASS";
                    G_Monitos_RESPONSE.TargetFeature = "DETECTMONITORS";

                    int count = 0;
                    foreach (var mo in _AllInfoMonitors)
                    {
                        G_Monitos_RESPONSE.Monitors.Add("[" + count.ToString() + "] " + mo.modelName);
                        count++;
                    }

                    System.Console.WriteLine(JsonConvert.SerializeObject(G_Monitos_RESPONSE, Formatting.Indented));
                }
                else
                {
                    G_Monitos_RESPONSE.Command = "GET";
                    G_Monitos_RESPONSE.TargetFeature = "DETECTMONITORS";
                    G_Monitos_RESPONSE.Result = "FAIL";
                    G_Monitos_RESPONSE.Message = "No Monitor Detect";

                    System.Console.WriteLine(JsonConvert.SerializeObject(G_Monitos_RESPONSE, Formatting.Indented));
                }

                return ((int)CLI_ExitCode.success, JsonConvert.SerializeObject(G_Monitos_RESPONSE, Formatting.Indented));
            }
            else
                return ((int)CLI_ExitCode.unknow_command, JsonConvert.SerializeObject(G_Monitos_RESPONSE, Formatting.Indented));
        }

        private async Task<(int code, string result)> FWVersion(IDeviceManagerSA devMgr, string type, List<string> index, List<string> serviceTag, List<string> model, string value = "")
        {
            CLI_RESPONSE G_FW_RESPONSE = new CLI_RESPONSE();

            //if (devMgr == null)
            //{
            //    writelog("FWVersion: Null IDeviceManagerSA");
            //    return (int)CLI_ExitCode.null_device_manager;
            //}

            if (_AllInfoMonitors == null)
                _AllInfoMonitors = await devMgr.GetMonitors();

            string output = string.Empty;
            if (type == "GET")
            {
                if (index.Count == 0 && serviceTag.Count == 0 && model.Count == 0)
                {
                    writelog($"FWVersion entry");
                    bool IsFailhappened = false;
                    foreach (MonitorInfo monitor in _AllInfoMonitors)
                    {
                        G_FW_RESPONSE = new CLI_RESPONSE(monitor);

                        G_FW_RESPONSE.Command = "GET";
                        G_FW_RESPONSE.TargetFeature = "FWVERSION";

                        if (string.IsNullOrWhiteSpace(monitor.FwVersion))
                        {
                            G_FW_RESPONSE.Result = "FAIL";
                            G_FW_RESPONSE.Message = "FAIL GET FW VERSION";
                            System.Console.WriteLine(JsonConvert.SerializeObject(G_FW_RESPONSE, Formatting.Indented));
                            output += "\n" + JsonConvert.SerializeObject(G_FW_RESPONSE, Formatting.Indented);
                            IsFailhappened = true;
                        }
                        else
                        {
                            G_FW_RESPONSE.Value = monitor.FwVersion;
                            G_FW_RESPONSE.Result = "PASS";
                            System.Console.WriteLine(JsonConvert.SerializeObject(G_FW_RESPONSE, Formatting.Indented));
                            output += "\n" + JsonConvert.SerializeObject(G_FW_RESPONSE, Formatting.Indented);
                        }
                    }

                    if (IsFailhappened)
                    {
                        writelog($"FWVersion fail");
                        return ((int)CLI_ExitCode.fail_SetVCPCapability, output);
                    }

                }
                else
                {
                    bool IsFailhappened = false;
                    foreach (string idx in index)
                    {
                        writelog($"FWVersion idx entry");

                        if (Convert.ToInt32(idx) < _AllInfoMonitors.Count)
                        {
                            var monitor = _AllInfoMonitors[Convert.ToInt32(idx)];
                            G_FW_RESPONSE = new CLI_RESPONSE(monitor);

                            G_FW_RESPONSE.Command = "GET";
                            G_FW_RESPONSE.TargetFeature = "FWVERSION";

                            if (string.IsNullOrWhiteSpace(monitor.FwVersion))
                            {
                                G_FW_RESPONSE.Result = "FAIL";
                                G_FW_RESPONSE.Message = "FAIL GET FW VERSION";
                                System.Console.WriteLine(JsonConvert.SerializeObject(G_FW_RESPONSE, Formatting.Indented));
                                output += "\n" + JsonConvert.SerializeObject(G_FW_RESPONSE, Formatting.Indented);
                                IsFailhappened = true;
                            }
                            else
                            {
                                G_FW_RESPONSE.Value = _AllInfoMonitors[Convert.ToInt32(idx)].FwVersion;
                                G_FW_RESPONSE.Result = "PASS";
                                System.Console.WriteLine(JsonConvert.SerializeObject(G_FW_RESPONSE, Formatting.Indented));
                                output += "\n" + JsonConvert.SerializeObject(G_FW_RESPONSE, Formatting.Indented);
                            }
                        }
                        else
                        {
                            G_FW_RESPONSE.Index = change_0base_to_1base(idx);
                            G_FW_RESPONSE.Command = "GET";
                            G_FW_RESPONSE.TargetFeature = "FWVersion";
                            G_FW_RESPONSE.Result = "FAIL";
                            G_FW_RESPONSE.Message = "Index Out of Range";
                            System.Console.WriteLine(JsonConvert.SerializeObject(G_FW_RESPONSE, Formatting.Indented));
                            output += "\n" + JsonConvert.SerializeObject(G_FW_RESPONSE, Formatting.Indented);
                            IsFailhappened = true;
                        }
                    }

                    foreach (string tag in serviceTag)
                    {
                        writelog($"FWVersion tag entry");
                        var tmp = _AllInfoMonitors.FindAll(x => x.edid.ServiceTag.ToUpper().Equals(tag.ToUpper()));
                        foreach (MonitorInfo mo in tmp)
                        {
                            G_FW_RESPONSE = new CLI_RESPONSE(mo);

                            G_FW_RESPONSE.Command = "GET";
                            G_FW_RESPONSE.TargetFeature = "FWVERSION";

                            if (string.IsNullOrWhiteSpace(mo.FwVersion))
                            {
                                G_FW_RESPONSE.Result = "FAIL";
                                G_FW_RESPONSE.Message = "FAIL GET FW VERSION";
                                System.Console.WriteLine(JsonConvert.SerializeObject(G_FW_RESPONSE, Formatting.Indented));
                                output += "\n" + JsonConvert.SerializeObject(G_FW_RESPONSE, Formatting.Indented);
                                IsFailhappened = true;
                            }
                            else
                            {
                                G_FW_RESPONSE.Value = mo.FwVersion;
                                G_FW_RESPONSE.Result = "PASS";
                                System.Console.WriteLine(JsonConvert.SerializeObject(G_FW_RESPONSE, Formatting.Indented));
                                output += "\n" + JsonConvert.SerializeObject(G_FW_RESPONSE, Formatting.Indented);
                            }
                        }
                    }

                    foreach (string modelName in model)
                    {
                        writelog($"FWVersion tag entry");
                        var tmp = _AllInfoMonitors.FindAll(x => x.edid.ModelName.ToUpper().Equals(modelName.ToUpper()));
                        foreach (MonitorInfo mo in tmp)
                        {
                            G_FW_RESPONSE = new CLI_RESPONSE(mo);

                            G_FW_RESPONSE.Command = "GET";
                            G_FW_RESPONSE.TargetFeature = "FWVERSION";

                            if (string.IsNullOrWhiteSpace(mo.FwVersion))
                            {
                                G_FW_RESPONSE.Result = "FAIL";
                                G_FW_RESPONSE.Message = "FAIL GET FW VERSION";
                                System.Console.WriteLine(JsonConvert.SerializeObject(G_FW_RESPONSE, Formatting.Indented));
                                output += "\n" + JsonConvert.SerializeObject(G_FW_RESPONSE, Formatting.Indented);
                                IsFailhappened = true;
                            }
                            else
                            {
                                G_FW_RESPONSE.Value = mo.FwVersion;
                                G_FW_RESPONSE.Result = "PASS";
                                System.Console.WriteLine(JsonConvert.SerializeObject(G_FW_RESPONSE, Formatting.Indented));
                                output += "\n" + JsonConvert.SerializeObject(G_FW_RESPONSE, Formatting.Indented);
                            }
                        }
                    }

                    if (IsFailhappened)
                    {
                        writelog($"FWVersion idx tag fail");
                        return ((int)CLI_ExitCode.fail_SetVCPCapability, output);
                    }

                }
                writelog($"FWVersion return value exit {output}");
                return ((int)CLI_ExitCode.success, output);
            }
            else
            {
                G_FW_RESPONSE.Command = type;
                G_FW_RESPONSE.TargetFeature = "FWVERSION";
                G_FW_RESPONSE.Result = "Un-support command";
                G_FW_RESPONSE.Message = "Un-support command";
                return ((int)CLI_ExitCode.unknow_command, JsonConvert.SerializeObject(G_FW_RESPONSE, Formatting.Indented));
            }
        }

        #endregion Jarvis methods

        private async Task<int> ProcessListMonitorsOptionAsync(IDeviceManagerSA devMgr, bool reget = false)
        {
            if (devMgr == null)
            {
                writelog("ProcessListMonitorsOptionAsync: input null IDeviceManagerSA");
                return (int)CLI_ExitCode.null_device_manager;
            }
            if (_AllInfoMonitors == null)
            {
                if (reget)
                    _AllInfoMonitors = await devMgr.Re_GetMonitors();
                else
                    _AllInfoMonitors = await devMgr.GetMonitors();
            }

            int index = 0;
            foreach (var g in _AllInfoMonitors)
            {
                System.Console.WriteLine("[" + index.ToString() + "] : " + g.modelName);
                index++;
            }

            return (int)CLI_ExitCode.success;
        }

        private async Task<int> GetCapabilitiesString(IDeviceManagerSA devMgr, string index)
        {
            if (devMgr == null)
            {
                writelog("GetCapabilitiesString: input null IDeviceManagerSA");
                return (int)CLI_ExitCode.null_device_manager;
            }
            if (_AllInfoMonitors == null)
                _AllInfoMonitors = await devMgr.GetMonitors();//_DisplayPlugin.GetMonitors();

            int i_index = System.Convert.ToInt32(index);

            //string rc = await _DisplayPlugin.GetCapabilitiesString(_AllInfoMonitors[i_index]);
            string rc = await devMgr.GetCapabilitiesString(_AllInfoMonitors[i_index]);

            System.Console.WriteLine("{0} : CapabilitiesString is\n{1}", _AllInfoMonitors[i_index].modelName, rc);

            return (int)CLI_ExitCode.success;
        }

        private async Task<int> GetVCPCapabilities(IDeviceManagerSA devMgr, string index)
        {
            if (devMgr == null)
            {
                writelog("GetVCPCapabilities: input null IDeviceManagerSA");
                return (int)CLI_ExitCode.null_device_manager;
            }
            if (_AllInfoMonitors == null)
                _AllInfoMonitors = await devMgr.GetMonitors();//_DisplayPlugin.GetMonitors();

            int i_index = System.Convert.ToInt32(index);

            //string rc = await _DisplayPlugin.GetVCPCapabilities(_AllInfoMonitors[i_index]);
            string rc = await devMgr.GetVCPCapabilities(_AllInfoMonitors[i_index]);

            System.Console.WriteLine("{0} : GetVCPCapabilities:\n{1}", _AllInfoMonitors[i_index].modelName, rc);

            return (int)CLI_ExitCode.success;
        }

        /// <summary>
        /// //
        /// </summary>
        /// <param name="text"></param>
        /// <param name="log_type">0 means info, others means error</param>
        private void writelog(string text,
            [System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
            [System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "",
            [System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = 0,
            log_type log_type = log_type.info)
        {
            if (string.IsNullOrEmpty(text))
                text = "";

            string className = this.GetType().Name;

            text = $"[CLI Plugin Display] {text}, Calss:{className}, Caller Name:{memberName}, Source Line {sourceLineNumber}";
            //Console.WriteLine(text);
            if (Log != null)
            {
                if (log_type == log_type.info)
                    Log.Info(text);
                else
                    Log.Error(text);
            }
        }

        private static string get_inputsource_type(string index)
        {
            switch (index)
            {
                case "VGA": return "VGA1";
                case "VGA1": return "VGA1";
                case "VGA-1": return "VGA1";

                case "VGA2": return "VGA2";
                case "VGA-2": return "VGA2";

                case "HDMI": return "HDMI1";
                case "HDMI1": return "HDMI1";
                case "HDMI-1": return "HDMI1";

                case "HDMI2": return "HDMI2";
                case "HDMI-2": return "HDMI2";

                case "DP": return "DISPLAYPORT1";
                case "DP1": return "DISPLAYPORT1";
                case "DP-1": return "DISPLAYPORT1";
                case "DISPLAYPORT": return "DISPLAYPORT1";
                case "DISPLAYPORT1": return "DISPLAYPORT1";
                case "DISPLAYPORT-1": return "DISPLAYPORT1";

                case "DP2": return "DISPLAYPORT2";
                case "DP-2": return "DISPLAYPORT2";
                case "DISPLAYPORT2": return "DISPLAYPORT2";
                case "DISPLAYPORT-2": return "DISPLAYPORT2";

                case "USBC": return "USB-C1";
                case "USBC1": return "USB-C1";
                case "USB-C": return "USB-C1";
                case "USB-C1": return "USB-C1";

                case "USBC2": return "USB-C2";
                case "USB-C2": return "USB-C2";

                case "TBT": return "Thunderbolt1";
                case "TBT1": return "Thunderbolt1";
                case "THUNDERBOLT": return "Thunderbolt1";
                case "THUNDERBOLT1": return "Thunderbolt1";
                case "THUNDERBOLT-1": return "Thunderbolt1";

                case "TBT2": return "Thunderbolt2";
                case "THUNDERBOLT2": return "Thunderbolt2";
                case "THUNDERBOLT-2": return "Thunderbolt2";

                default: return "Unknown";
            }
        }

        private static int get_inputsource_vcp(string index)
        {
            switch (index)
            {
                case "HDMI-1": return 0x11;
                case "HDMI-2": return 0x12;

                case "DISPLAYPORT-1": return 0x0f;
                case "DISPLAYPORT-2": return 0x13;

                case "USB-C1": return 0x1b;
                case "USB-C2": return 0x1c;

                case "Thunderbolt-1": return 0x19;
                case "Thunderbolt-2": return 0x1a;

                case "VGA-1": return 0x01;
                case "VGA-2": return 0x02;

                default: return 0;
            }
        }

        //Input Source
        //06.07 Jason
        private async Task<(int code, string result)> InputSource(IDeviceManagerSA devMgr, CommandLineInput commandLineInput)
        {
            if (_AllInfoMonitors == null)
                _AllInfoMonitors = await devMgr.GetMonitors(); //_DisplayPlugin.GetMonitors();
            string output = string.Empty;
            bool ispass = true;
            DDPMSettings data = devMgr.ReloadAppConfigData().Result;

            if (data == null)
            {
                return DDPMSettingsNullResponse(commandLineInput.Command, commandLineInput.TargetFeature);
            }

            if (commandLineInput.Command == "SET")
            {
                if (commandLineInput.Options[0].Option_Name.ToUpper().Equals("VALUE")) //ex: /set -name=Display.Brightness -index=[0] -value=60
                {
                    Dictionary<string, string> serviceTagList = new Dictionary<string, string>();
                    List<string> targetList = new List<string>();
                    bool flag = true;
                    int count = 0;
                    int outCount = 0;
                    List<string> swapIsDone = new List<string>();
                    if (commandLineInput.DeviceIndex.Count == 0 && commandLineInput.ServiceTag.Count == 0 && commandLineInput.Model.Count == 0)
                    {
                        writelog($"ActiveInputSource set entry");
                        serviceTagList = _AllInfoMonitors.DistinctBy(_ => _.edid.ServiceTag).ToDictionary(_ => _.edid.ServiceTag, _ => _.edid.ServiceTag);
                        targetList = _AllInfoMonitors.Select(_ => _.edid.ServiceTag).ToList();
                        outCount = _AllInfoMonitors.Select(_ => _.edid.ServiceTag).Count();
                        //CLI_Set_Input_RESPONSE _Set_Input_RESPONSE = new CLI_Set_Input_RESPONSE();
                        //foreach (var monitor in _AllInfoMonitors)
                        //for (int i = 0; i < _AllInfoMonitors.Count; i++)
                        //{
                        //    writelog($"ActiveInputSource set entry");
                        //    var monitor = _AllInfoMonitors[i];

                        //    CLI_RESPONSE _Input_RESPONSE = new CLI_RESPONSE(monitor);
                        //    _Input_RESPONSE.Command = commandLineInput.Command;
                        //    _Input_RESPONSE.TargetFeature = commandLineInput.TargetFeature;

                        //    //commandLineInput.Options[0].Option_Value.Replace(".", ",");
                        //    string[] op_values = commandLineInput.Options[0].Option_Value.Replace(".", ",").Split(",");

                        //    foreach (string v in op_values)
                        //    {
                        //        switch (v.ToUpper())
                        //        {
                        //            case "LOCK":
                        //            case "UNLOCK":
                        //                if (v.ToUpper().Equals("LOCK")) data.LockSettings.Lock_Display_ActiveInputSource = true;
                        //                if (v.ToUpper().Equals("UNLOCK")) data.LockSettings.Lock_Display_ActiveInputSource = false;
                        //                await devMgr.SetAppConfigData(data);
                        //                break;
                        //        }
                        //    }
                        //    string get_inputvpccode = get_inputsource_type(op_values[0]);
                        //    // int getvcp = get_inputsource_vcp(get_inputvpccode);

                        //    if (get_inputvpccode != "Unknown")
                        //    {
                        //        if (commandLineInput.Options.Count == 1)
                        //        {
                        //            // bool retcode = SetVCPCode(devMgr, monitor, "0x60", "0x" + get_inputsource_vcp(get_inputvpccode).ToString("X2")).Result;
                        //            bool retcode = SetVCPCode(devMgr, monitor, "Input Select", get_inputvpccode).Result;
                        //            if (!retcode) ispass = false;

                        //            //_Input_RESPONSE.ActiveInputSource = commandLineInput.Options[0].Option_Value;
                        //            _Input_RESPONSE.Value = op_values[0];
                        //            if (!ispass)
                        //            {
                        //                _Input_RESPONSE.Result = "FAIL";
                        //                _Input_RESPONSE.Message = "FAIL_SetVCP";
                        //                System.Console.WriteLine(_Input_RESPONSE.ToJson());
                        //                output += "\n" + _Input_RESPONSE.ToJson();
                        //            }
                        //            else
                        //            {
                        //                _Input_RESPONSE.Result = "PASS";
                        //                //_Input_RESPONSE.Value += "," + (data.LockSettings.Lock_Display_ActiveInputSource ? "LOCK" : "UNLOCK");
                        //                System.Console.WriteLine(_Input_RESPONSE.ToJson());
                        //                output += "\n" + _Input_RESPONSE.ToJson();
                        //            }
                        //        }
                        //        else
                        //        {
                        //            //_Input_RESPONSE.ActiveInputSource = String.Empty;
                        //            _Input_RESPONSE.Result = "FAIL";
                        //            if (commandLineInput.Options.Count > 1)
                        //            {
                        //                _Input_RESPONSE.Message = "Too Many Value";
                        //            }
                        //            else
                        //            {
                        //                _Input_RESPONSE.Message = "No Value";
                        //            }
                        //            System.Console.WriteLine(_Input_RESPONSE.ToJson());
                        //            output += "\n" + _Input_RESPONSE.ToJson();
                        //            return ((int)CLI_ExitCode.fail_Value, output);
                        //        }
                        //    }
                        //    else
                        //    {
                        //        //_Input_RESPONSE.ActiveInputSource = String.Empty;
                        //        _Input_RESPONSE.Result = "FAIL";
                        //        _Input_RESPONSE.Message = "Wrong option value: ";
                        //        _Input_RESPONSE.Message += $"{op_values[0]}";//add error message if option value not exist in input source list
                        //        System.Console.WriteLine(_Input_RESPONSE.ToJson());
                        //        output += "\n" + _Input_RESPONSE.ToJson();
                        //        writelog($"ActiveInputSource set fail {output}");
                        //        return ((int)CLI_ExitCode.fail_Value, output);
                        //    }
                        //}
                    }
                    else if (commandLineInput.DeviceIndex.Count != 0)
                    {
                        writelog($"ActiveInputSource set index entry");
                        serviceTagList = _AllInfoMonitors.DistinctBy(_ => _.edid.ServiceTag).ToDictionary(_ => _.edid.ServiceTag, _ => _.Index.ToString());
                        targetList = commandLineInput.DeviceIndex;
                        outCount = commandLineInput.DeviceIndex.Count;
                    }
                    else if (commandLineInput.ServiceTag.Count != 0)
                    {
                        writelog($"ActiveInputSource set ServiceTag entry");
                        serviceTagList = _AllInfoMonitors.DistinctBy(_ => _.edid.ServiceTag).ToDictionary(_ => _.edid.ServiceTag, _ => _.edid.ServiceTag);
                        targetList = commandLineInput.ServiceTag;
                        outCount = commandLineInput.ServiceTag.Count;
                    }
                    else if (commandLineInput.Model.Count != 0)
                    {
                        writelog($"ActiveInputSource set Model entry");
                        serviceTagList = _AllInfoMonitors.DistinctBy(_ => _.edid.ServiceTag).ToDictionary(_ => _.edid.ServiceTag, _ => _.modelName);
                        targetList = commandLineInput.Model;
                        outCount = commandLineInput.Model.Count;
                    }
                    //swapIsDone = new List<string>();
                    flag = true;
                    while (flag && count < 1000)
                    {
                        foreach (string target in targetList)
                        {
                            writelog($"ActiveInputSource set in progress");
                            var targetTemp = serviceTagList.FirstOrDefault(_ => _.Value == target.ToUpper());
                            if (targetTemp.Value == null)
                            {
                                count++;
                                continue;
                            }
                            string stIsDone = swapIsDone.FirstOrDefault(_ => _ == targetTemp.Key);
                            if (!String.IsNullOrWhiteSpace(stIsDone))
                            {
                                count++;
                                continue;
                            }
                            MonitorInfo mo = _AllInfoMonitors.FirstOrDefault(_ => _.edid.ServiceTag == targetTemp.Key);
                            if (mo == null)
                            {
                                _AllInfoMonitors = _devMgr.GetMonitors().Result;
                                count++;
                                break;
                            }
                            CLI_RESPONSE _Input_RESPONSE = new CLI_RESPONSE(mo);
                            _Input_RESPONSE.Command = commandLineInput.Command;
                            _Input_RESPONSE.TargetFeature = commandLineInput.TargetFeature;

                            //commandLineInput.Options[0].Option_Value.Replace(".", ",");
                            string[] op_values = commandLineInput.Options[0].Option_Value.Replace(".", ",").Split(",");

                            foreach (string v in op_values)
                            {
                                switch (v.ToUpper())
                                {
                                    case "LOCK":
                                    case "UNLOCK":
                                        if (v.ToUpper().Equals("LOCK")) data.LockSettings.Lock_Display_ActiveInputSource = true;
                                        if (v.ToUpper().Equals("UNLOCK")) data.LockSettings.Lock_Display_ActiveInputSource = false;
                                        await devMgr.SetAppConfigData(data);
                                        break;
                                }
                            }
                            string get_inputvpccode = get_inputsource_type(op_values[0]);
                            if (get_inputvpccode != "Unknown")
                            {
                                if (commandLineInput.Options.Count == 1)
                                {
                                    bool retcode = SetVCPCode(devMgr, mo, "Input Select", get_inputvpccode).Result;
                                    if (!retcode) ispass = false;
                                    _Input_RESPONSE.Value = op_values[0];
                                    if (!ispass)
                                    {
                                        _Input_RESPONSE.Result = "FAIL";
                                        _Input_RESPONSE.Message = "FAIL_SetVCP";
                                        System.Console.WriteLine(_Input_RESPONSE.ToJson());
                                        output += "\n" + _Input_RESPONSE.ToJson();
                                    }
                                    else
                                    {
                                        _Input_RESPONSE.Result = "PASS";
                                        //_Input_RESPONSE.Value += "," + (data.LockSettings.Lock_Display_ActiveInputSource ? "LOCK" : "UNLOCK");
                                        System.Console.WriteLine(_Input_RESPONSE.ToJson());
                                        output += "\n" + _Input_RESPONSE.ToJson();
                                        swapIsDone.Add(targetTemp.Key);
                                    }
                                }
                                else
                                {
                                    _Input_RESPONSE.Result = "FAIL";
                                    if (commandLineInput.Options.Count > 1)
                                    {
                                        _Input_RESPONSE.Message = "Too Many Value";
                                    }
                                    else
                                    {
                                        _Input_RESPONSE.Message = "No Value";
                                    }
                                    System.Console.WriteLine(_Input_RESPONSE.ToJson());
                                    output += "\n" + _Input_RESPONSE.ToJson();
                                    writelog($"ActiveInputSource set fail {output}");
                                    return ((int)CLI_ExitCode.fail_Value, output);
                                }
                            }
                            else
                            {
                                _Input_RESPONSE.Result = "FAIL";
                                _Input_RESPONSE.Message = "Wrong option value: ";
                                _Input_RESPONSE.Message += $"{commandLineInput.Options[0].Option_Value}";
                                System.Console.WriteLine(_Input_RESPONSE.ToJson());
                                output += "\n" + _Input_RESPONSE.ToJson();
                                writelog($"ActiveInputSource set fail {output}");
                                return ((int)CLI_ExitCode.fail_Value, output);
                            }
                        }
                        if (outCount == swapIsDone.Count)
                            flag = false;
                        count++;
                    }
                }
                writelog($"ActiveInputSource set exit return value {output}");
                return ((int)CLI_ExitCode.success, output);
            }
            else if (commandLineInput.Command == "GET")
            {
                _AllInfoMonitors = await devMgr.GetMonitors();
                if (commandLineInput.DeviceIndex.Count == 0 && commandLineInput.ServiceTag.Count == 0 && commandLineInput.Model.Count == 0)
                {
                    foreach (MonitorInfo mo in _AllInfoMonitors)
                    {
                        writelog($"ActiveInputSource get entry");
                        CLI_RESPONSE _Input_RESPONSE = new CLI_RESPONSE(mo);
                        string src = String.Empty;
                        _Input_RESPONSE.Command = commandLineInput.Command;
                        _Input_RESPONSE.TargetFeature = commandLineInput.TargetFeature;
                        if (commandLineInput.Options.Count != 0)
                        {
                            _Input_RESPONSE.Result = "FAIL";
                            _Input_RESPONSE.Message = "Too Many Value";
                            System.Console.WriteLine(_Input_RESPONSE.ToJson());
                            output += "\n" + _Input_RESPONSE.ToJson();
                            writelog($"ActiveInputSource get fail {output}");
                            return ((int)CLI_ExitCode.fail_Value, output);
                        }
                        src = GetCurrentInput(devMgr, mo).Result;
                        if (src == String.Empty)
                        {
                            _Input_RESPONSE.Result = "FAIL";
                            _Input_RESPONSE.Message = "FAIL_SetVCP";
                            System.Console.WriteLine(_Input_RESPONSE.ToJson());
                            output += "\n" + _Input_RESPONSE.ToJson();
                        }
                        _Input_RESPONSE.Value = src;
                        _Input_RESPONSE.Result = "PASS";
                        //_Input_RESPONSE.Value += "," + (data.LockSettings.Lock_Display_ActiveInputSource ? "LOCK" : "UNLOCK");
                        System.Console.WriteLine(_Input_RESPONSE.ToJson());
                        output += "\n" + _Input_RESPONSE.ToJson();
                    }
                }
                else if (commandLineInput.DeviceIndex.Count != 0)
                {
                    foreach (string idx in commandLineInput.DeviceIndex)
                    {
                        writelog($"ActiveInputSource get entry");
                        CLI_RESPONSE _Input_RESPONSE = new CLI_RESPONSE();
                        string src = String.Empty;
                        _Input_RESPONSE.Command = commandLineInput.Command;
                        _Input_RESPONSE.TargetFeature = commandLineInput.TargetFeature;
                        if (commandLineInput.Options.Count != 0)
                        {
                            _Input_RESPONSE.Result = "FAIL";
                            _Input_RESPONSE.Message = "Too Many Value";
                            System.Console.WriteLine(_Input_RESPONSE.ToJson());
                            output += "\n" + _Input_RESPONSE.ToJson();
                            writelog($"ActiveInputSource get fail {output}");
                            return ((int)CLI_ExitCode.fail_Value, output);
                        }
                        if (!int.TryParse(idx, out _))
                        {
                            _Input_RESPONSE.Result = "FAIL";
                            _Input_RESPONSE.Message = "Incorrect Value";
                            System.Console.WriteLine(_Input_RESPONSE.ToJson());
                            output += "\n" + _Input_RESPONSE.ToJson();
                            writelog($"ActiveInputSource get fail {output}");
                            return ((int)CLI_ExitCode.fail_Value, output);
                        }
                        _Input_RESPONSE = new CLI_RESPONSE(_AllInfoMonitors[int.Parse(idx)]);
                        _Input_RESPONSE.Command = commandLineInput.Command;
                        _Input_RESPONSE.TargetFeature = commandLineInput.TargetFeature;

                        src = GetCurrentInput(devMgr, _AllInfoMonitors[int.Parse(idx)]).Result;
                        if (src == String.Empty)
                        {
                            _Input_RESPONSE.Result = "FAIL";
                            _Input_RESPONSE.Message = "FAIL_SetVCP";
                            System.Console.WriteLine(_Input_RESPONSE.ToJson());
                            output += "\n" + _Input_RESPONSE.ToJson();
                        }
                        _Input_RESPONSE.Value = src;
                        _Input_RESPONSE.Result = "PASS";
                        //_Input_RESPONSE.Value += "," + (data.LockSettings.Lock_Display_ActiveInputSource ? "LOCK" : "UNLOCK");
                        System.Console.WriteLine(_Input_RESPONSE.ToJson());
                        output += "\n" + _Input_RESPONSE.ToJson();
                    }
                }
                else if (commandLineInput.ServiceTag.Count != 0)
                {
                    foreach (string tag in commandLineInput.ServiceTag)
                    {
                        var moTemp = _AllInfoMonitors.FindAll(x => x.edid.ServiceTag.ToUpper().Equals(tag.ToUpper()));
                        CLI_RESPONSE _Input_RESPONSE = new CLI_RESPONSE();
                        _Input_RESPONSE.Command = commandLineInput.Command;
                        _Input_RESPONSE.TargetFeature = commandLineInput.TargetFeature;
                        if (moTemp == null)
                        {
                            _Input_RESPONSE.Result = "FAIL";
                            _Input_RESPONSE.Message = "Incorrect Value";
                            System.Console.WriteLine(_Input_RESPONSE.ToJson());
                            output += "\n" + _Input_RESPONSE.ToJson();
                            writelog($"ActiveInputSource get fail {output}");
                            return ((int)CLI_ExitCode.fail_Value, output);
                        }
                        foreach (MonitorInfo mt in moTemp)
                        {
                            writelog($"ActiveInputSource get entry");
                            string src = String.Empty;
                            _Input_RESPONSE = new CLI_RESPONSE(mt);
                            _Input_RESPONSE.Command = commandLineInput.Command;
                            _Input_RESPONSE.TargetFeature = commandLineInput.TargetFeature;
                            if (commandLineInput.Options.Count != 0)
                            {
                                _Input_RESPONSE.Result = "FAIL";
                                _Input_RESPONSE.Message = "Too Many Value";
                                System.Console.WriteLine(_Input_RESPONSE.ToJson());
                                output += "\n" + _Input_RESPONSE.ToJson();
                                writelog($"ActiveInputSource get fail {output}");
                                return ((int)CLI_ExitCode.fail_Value, output);
                            }
                            src = GetCurrentInput(devMgr, mt).Result;
                            if (src == String.Empty)
                            {
                                _Input_RESPONSE.Result = "FAIL";
                                _Input_RESPONSE.Message = "FAIL_SetVCP";
                                System.Console.WriteLine(_Input_RESPONSE.ToJson());
                                output += "\n" + _Input_RESPONSE.ToJson();
                            }
                            _Input_RESPONSE.Value = src;
                            _Input_RESPONSE.Result = "PASS";
                            //_Input_RESPONSE.Value += "," + (data.LockSettings.Lock_Display_ActiveInputSource ? "LOCK" : "UNLOCK");
                            System.Console.WriteLine(_Input_RESPONSE.ToJson());
                            output += "\n" + _Input_RESPONSE.ToJson();
                        }
                    }
                }
                else if (commandLineInput.Model.Count != 0)
                {
                    foreach (string md in commandLineInput.Model)
                    {
                        var moTemp = _AllInfoMonitors.FindAll(x => x.modelName.ToUpper().Equals(md.ToUpper()));
                        CLI_RESPONSE _Input_RESPONSE = new CLI_RESPONSE();
                        _Input_RESPONSE.Command = commandLineInput.Command;
                        _Input_RESPONSE.TargetFeature = commandLineInput.TargetFeature;
                        if (moTemp == null)
                        {
                            _Input_RESPONSE.Result = "FAIL";
                            _Input_RESPONSE.Message = "Incorrect Value";
                            System.Console.WriteLine(_Input_RESPONSE.ToJson());
                            output += "\n" + _Input_RESPONSE.ToJson();
                            writelog($"ActiveInputSource get fail {output}");
                            return ((int)CLI_ExitCode.fail_Value, output);
                        }
                        foreach (MonitorInfo mt in moTemp)
                        {
                            writelog($"ActiveInputSource get entry");
                            string src = String.Empty;
                            _Input_RESPONSE = new CLI_RESPONSE(mt);
                            _Input_RESPONSE.Command = commandLineInput.Command;
                            _Input_RESPONSE.TargetFeature = commandLineInput.TargetFeature;
                            if (commandLineInput.Options.Count != 0)
                            {
                                _Input_RESPONSE.Result = "FAIL";
                                _Input_RESPONSE.Message = "Too Many Value";
                                System.Console.WriteLine(_Input_RESPONSE.ToJson());
                                output += "\n" + _Input_RESPONSE.ToJson();
                                writelog($"ActiveInputSource get fail {output}");
                                return ((int)CLI_ExitCode.fail_Value, output);
                            }
                            src = GetCurrentInput(devMgr, mt).Result;
                            if (src == String.Empty)
                            {
                                _Input_RESPONSE.Result = "FAIL";
                                _Input_RESPONSE.Message = "FAIL_SetVCP";
                                System.Console.WriteLine(_Input_RESPONSE.ToJson());
                                output += "\n" + _Input_RESPONSE.ToJson();
                            }
                            _Input_RESPONSE.Value = src;
                            _Input_RESPONSE.Result = "PASS";
                            //_Input_RESPONSE.Value += "," + (data.LockSettings.Lock_Display_ActiveInputSource ? "LOCK" : "UNLOCK");
                            System.Console.WriteLine(_Input_RESPONSE.ToJson());
                            output += "\n" + _Input_RESPONSE.ToJson();
                        }
                    }
                }
                writelog($"ActiveInputSource get exit return value {output}");
                return ((int)CLI_ExitCode.success, output);
            }
            CLI_InputList_RESPONSE temp = new CLI_InputList_RESPONSE();
            temp.Command = commandLineInput.Command;
            temp.TargetFeature = commandLineInput.TargetFeature;
            temp.Result = "Un-supported command";
            temp.Message = "Un-supported command";
            return ((int)CLI_ExitCode.command_not_support, temp.ToJson());
        }

        //06.07 Jason
        private async Task<(int code, string result)> InputSourceList(IDeviceManagerSA devMgr, CommandLineInput commandLineInput)
        {
            //if (devMgr == null)
            //{
            //    writelog("SetVCPCode: input null IDeviceManagerSA");
            //    return (int)CLI_ExitCode.null_device_manager;
            //}
            if (_AllInfoMonitors == null)
                _AllInfoMonitors = await devMgr.GetMonitors(); //_DisplayPlugin.GetMonitors();

            string output = string.Empty;
            if (commandLineInput.Command == "GET")
            {
                List<string> inputs = new List<string>();
                if (commandLineInput.DeviceIndex.Count == 0 && commandLineInput.ServiceTag.Count == 0 && commandLineInput.Model.Count == 0)
                {
                    foreach (var monitor in _AllInfoMonitors)
                    {
                        CLI_InputList_RESPONSE _Get_InputList_RESPONSE = new CLI_InputList_RESPONSE(monitor);
                        inputs = new List<string>();
                        //_Get_InputList_RESPONSE.Model = monitor.edid.ModelName;
                        //_Get_InputList_RESPONSE.SerialNumber = monitor.edid.SerialNumber;
                        //_Get_InputList_RESPONSE.Index = change_0base_to_1base((monitor.Index).ToString());
                        //_Get_InputList_RESPONSE.ServiceTag = monitor.edid.ServiceTag;
                        _Get_InputList_RESPONSE.Command = commandLineInput.Command;
                        _Get_InputList_RESPONSE.TargetFeature = commandLineInput.TargetFeature;
                        if (commandLineInput.Options.Count == 0)
                        {
                            inputs = GetInputsList(devMgr, (monitor.Index).ToString()).Result;
                            _Get_InputList_RESPONSE.InputSourceList = inputs;
                            if (inputs.Count == 0)
                            {
                                _Get_InputList_RESPONSE.Result = "FAIL";
                                _Get_InputList_RESPONSE.Message = "fail_GetInputListFail";
                                System.Console.WriteLine(_Get_InputList_RESPONSE.ToJson());
                                output += "\n" + _Get_InputList_RESPONSE.ToJson();
                            }
                            else
                            {
                                _Get_InputList_RESPONSE.Result = "PASS";
                                System.Console.WriteLine(_Get_InputList_RESPONSE.ToJson());
                                output += "\n" + _Get_InputList_RESPONSE.ToJson();
                            }
                        }
                        else
                        {
                            _Get_InputList_RESPONSE.InputSourceList = inputs;
                            _Get_InputList_RESPONSE.Result = "FAIL";
                            _Get_InputList_RESPONSE.Message = "Too Many Value";
                            System.Console.WriteLine(_Get_InputList_RESPONSE.ToJson());
                            output += "\n" + _Get_InputList_RESPONSE.ToJson();
                            return ((int)CLI_ExitCode.fail_Value, output);
                        }
                    }
                }
                else if (commandLineInput.DeviceIndex.Count != 0)
                {
                    foreach (string idx in commandLineInput.DeviceIndex)
                    {
                        inputs = new List<string>();
                        MonitorInfo monitor = _AllInfoMonitors[int.Parse(idx)];
                        CLI_InputList_RESPONSE _Get_InputList_RESPONSE = new CLI_InputList_RESPONSE(monitor);
                        //_Get_InputList_RESPONSE.Model = monitor.edid.ModelName;
                        //_Get_InputList_RESPONSE.SerialNumber = monitor.edid.SerialNumber;
                        //_Get_InputList_RESPONSE.Index = change_0base_to_1base(idx);
                        //_Get_InputList_RESPONSE.ServiceTag = monitor.edid.ServiceTag;
                        _Get_InputList_RESPONSE.Command = commandLineInput.Command;
                        _Get_InputList_RESPONSE.TargetFeature = commandLineInput.TargetFeature;

                        if (commandLineInput.Options.Count == 0)
                        {
                            inputs = GetInputsList(devMgr, idx).Result;
                            _Get_InputList_RESPONSE.InputSourceList = inputs;
                            if (inputs.Count == 0)
                            {
                                _Get_InputList_RESPONSE.Result = "FAIL";
                                _Get_InputList_RESPONSE.Message = "fail_GetInputListFail";
                                System.Console.WriteLine(_Get_InputList_RESPONSE.ToJson());
                                output += "\n" + _Get_InputList_RESPONSE.ToJson();
                                return ((int)CLI_ExitCode.fail_GetInputListFail, output);
                            }
                            else
                            {
                                _Get_InputList_RESPONSE.Result = "PASS";
                                System.Console.WriteLine(_Get_InputList_RESPONSE.ToJson());
                                output += "\n" + _Get_InputList_RESPONSE.ToJson();
                            }
                        }
                        else
                        {
                            _Get_InputList_RESPONSE.InputSourceList = inputs;
                            _Get_InputList_RESPONSE.Result = "FAIL";
                            _Get_InputList_RESPONSE.Message = "Too Many Value";
                            System.Console.WriteLine(_Get_InputList_RESPONSE.ToJson());
                            output += "\n" + _Get_InputList_RESPONSE.ToJson();
                            return ((int)CLI_ExitCode.fail_Value, output);
                        }
                    }
                }
                else if (commandLineInput.ServiceTag.Count != 0)
                {
                    foreach (string tag in commandLineInput.ServiceTag)
                    {
                        var tmp = _AllInfoMonitors.FindAll(x => x.edid.ServiceTag.ToUpper().Equals(tag.ToUpper()));
                        foreach (MonitorInfo mo in tmp)
                        {
                            inputs = new List<string>();
                            CLI_InputList_RESPONSE _Get_InputList_RESPONSE = new CLI_InputList_RESPONSE(mo);
                            //_Get_InputList_RESPONSE.Model = mo.edid.ModelName;
                            //_Get_InputList_RESPONSE.SerialNumber = mo.edid.SerialNumber;
                            //_Get_InputList_RESPONSE.Index = change_0base_to_1base((mo.Index).ToString());
                            //_Get_InputList_RESPONSE.ServiceTag = tag;
                            _Get_InputList_RESPONSE.Command = commandLineInput.Command;
                            _Get_InputList_RESPONSE.TargetFeature = commandLineInput.TargetFeature;

                            if (commandLineInput.Options.Count == 0)
                            {
                                inputs = GetInputsList(devMgr, (mo.Index).ToString()).Result;
                                _Get_InputList_RESPONSE.InputSourceList = inputs;
                                if (inputs.Count == 0)
                                {
                                    _Get_InputList_RESPONSE.Result = "FAIL";
                                    _Get_InputList_RESPONSE.Message = "fail_VCPCapability";
                                    System.Console.WriteLine(_Get_InputList_RESPONSE.ToJson());
                                    output += "\n" + _Get_InputList_RESPONSE.ToJson();
                                }
                                else
                                {
                                    _Get_InputList_RESPONSE.Result = "PASS";
                                    System.Console.WriteLine(_Get_InputList_RESPONSE.ToJson());
                                    output += "\n" + _Get_InputList_RESPONSE.ToJson();
                                }
                            }
                            else
                            {
                                _Get_InputList_RESPONSE.InputSourceList = inputs;
                                _Get_InputList_RESPONSE.Result = "FAIL";
                                _Get_InputList_RESPONSE.Message = "Too Many Value";
                                System.Console.WriteLine(_Get_InputList_RESPONSE.ToJson());
                                output += "\n" + _Get_InputList_RESPONSE.ToJson();
                                return ((int)CLI_ExitCode.fail_Value, output);
                            }
                        }
                    }
                }
                else if (commandLineInput.Model.Count != 0)
                {
                    foreach (string modelName in commandLineInput.Model)
                    {
                        var tmp = _AllInfoMonitors.FindAll(x => x.edid.ModelName.ToUpper().Equals(modelName.ToUpper()));
                        foreach (MonitorInfo mo in tmp)
                        {
                            inputs = new List<string>();
                            CLI_InputList_RESPONSE _Get_InputList_RESPONSE = new CLI_InputList_RESPONSE(mo);
                            //_Get_InputList_RESPONSE.Model = mo.edid.ModelName;
                            //_Get_InputList_RESPONSE.SerialNumber = mo.edid.SerialNumber;
                            //_Get_InputList_RESPONSE.Index = change_0base_to_1base((mo.Index).ToString());
                            //_Get_InputList_RESPONSE.ServiceTag = mo.edid.ModelName;
                            _Get_InputList_RESPONSE.Command = commandLineInput.Command;
                            _Get_InputList_RESPONSE.TargetFeature = commandLineInput.TargetFeature;

                            if (commandLineInput.Options.Count == 0)
                            {
                                inputs = GetInputsList(devMgr, (mo.Index).ToString()).Result;
                                _Get_InputList_RESPONSE.InputSourceList = inputs;
                                if (inputs.Count == 0)
                                {
                                    _Get_InputList_RESPONSE.Result = "FAIL";
                                    _Get_InputList_RESPONSE.Message = "fail_VCPCapability";
                                    System.Console.WriteLine(_Get_InputList_RESPONSE.ToJson());
                                    output += "\n" + _Get_InputList_RESPONSE.ToJson();
                                }
                                else
                                {
                                    _Get_InputList_RESPONSE.Result = "PASS";
                                    System.Console.WriteLine(_Get_InputList_RESPONSE.ToJson());
                                    output += "\n" + _Get_InputList_RESPONSE.ToJson();
                                }
                            }
                            else
                            {
                                _Get_InputList_RESPONSE.InputSourceList = inputs;
                                _Get_InputList_RESPONSE.Result = "FAIL";
                                _Get_InputList_RESPONSE.Message = "Too Many Value";
                                System.Console.WriteLine(_Get_InputList_RESPONSE.ToJson());
                                output += "\n" + _Get_InputList_RESPONSE.ToJson();
                                return ((int)CLI_ExitCode.fail_Value, output);
                            }
                        }
                    }
                }
                return ((int)CLI_ExitCode.success, output);
            }
            CLI_InputList_RESPONSE temp = new CLI_InputList_RESPONSE();
            temp.Command = commandLineInput.Command;
            temp.TargetFeature = commandLineInput.TargetFeature;
            temp.Result = "Un-supported command";
            temp.Message = "Un-supported command";
            return ((int)CLI_ExitCode.fail_GetInputListFail, temp.ToJson());
        }

        //06.06 Jason
        private async Task<List<string>> GetInputsList(IDeviceManagerSA devMgr, string index)
        {
            List<string> inputs = new List<string>();
            if (devMgr == null)
            {
                writelog("GetVCPCapabilities: input null IDeviceManagerSA");
                return inputs;
            }
            if (_AllInfoMonitors == null)
                _AllInfoMonitors = await devMgr.GetMonitors();//_DisplayPlugin.GetMonitors();

            int i_index = System.Convert.ToInt32(index);

            //string rc = await _DeviceManagerPlugin.GetVCPCapabilities(_AllInfoMonitors[i_index]);

            inputSourceList = await devMgr.GetInputSourcelist(_AllInfoMonitors[i_index]);

            foreach (var i in inputSourceList)
            {
                inputs.Add(i.Key.ToString().ToUpper());
            }
            return inputs;
        }

        private async Task<int> GetInputName(IDeviceManagerSA devMgr, string index, string input)
        {
            if (devMgr == null)
            {
                writelog("GetVCPCapabilities: input null IDeviceManagerSA");
                return (int)CLI_ExitCode.null_device_manager;
            }
            if (_AllInfoMonitors == null)
                _AllInfoMonitors = await devMgr.GetMonitors();
            int i_index = System.Convert.ToInt32(index);
            if (inputSourceList == null)
                inputSourceList = await devMgr.GetInputSourcelist(_AllInfoMonitors[i_index]);
            System.Console.WriteLine("Input Name : " + inputSourceList[input].InputName);
            return (int)CLI_ExitCode.success;
        }

        private async Task<int> GetUSBUpstream(IDeviceManagerSA devMgr, string index, string input)
        {
            if (devMgr == null)
            {
                writelog("GetVCPCapabilities: input null IDeviceManagerSA");
                return (int)CLI_ExitCode.null_device_manager;
            }
            if (_AllInfoMonitors == null)
                _AllInfoMonitors = await devMgr.GetMonitors();
            int i_index = System.Convert.ToInt32(index);
            if (inputSourceList == null)
                inputSourceList = await devMgr.GetInputSourcelist(_AllInfoMonitors[i_index]);
            System.Console.WriteLine("USBUpstream : " + inputSourceList[input].USBUpstream);
            return (int)CLI_ExitCode.success;
        }

        //06.06 Jason add
        private async Task<string> GetCurrentInput(IDeviceManagerSA devMgr, string index)
        {
            if (devMgr == null)
            {
                writelog("GetVCPCapabilities: input null IDeviceManagerSA");
                return "";
            }
            if (_AllInfoMonitors == null)
                _AllInfoMonitors = await devMgr.GetMonitors();// _DisplayPlugin.GetMonitors();

            int i_index = System.Convert.ToInt32(index);

            string current = _AllInfoMonitors[i_index].inputSource;//await devMgr.GetCurrentInput(_AllInfoMonitors[i_index], b_vcpcode, 0);
            //System.Console.WriteLine("Current Input : " + current);
            return current;
        }

        //06.06 Jason
        private async Task<string> GetCurrentInput(IDeviceManagerSA devMgr, MonitorInfo mo)
        {
            if (devMgr == null)
            {
                writelog("GetVCPCapabilities: input null IDeviceManagerSA");
                return "";
            }
            if (_AllInfoMonitors == null)
                _AllInfoMonitors = await devMgr.GetMonitors();// _DisplayPlugin.GetMonitors();

            string current = mo.inputSource;//await devMgr.GetCurrentInput(_AllInfoMonitors[i_index], b_vcpcode, 0);
            System.Console.WriteLine("Current Input : " + current);
            return current;
        }

        //06.06 Jason
        private async Task<int> SetCurrentInput(IDeviceManagerSA devMgr, string index, string input)
        {
            try
            {
                if (devMgr == null)
                {
                    writelog("GetVCPCapabilities: input null IDeviceManagerSA");
                    return (int)CLI_ExitCode.null_device_manager;
                }
                if (_AllInfoMonitors == null)
                    _AllInfoMonitors = await devMgr.GetMonitors(); // _DisplayPlugin.GetMonitors();

                int i_index = System.Convert.ToInt32(index);
                bool reb = devMgr.SetVCPCapability(_AllInfoMonitors[i_index], "Input Select", input).Result;
                if (reb)
                {
                    return (int)CLI_ExitCode.success;
                }
                else
                {
                    return (int)CLI_ExitCode.fail_SetVCPCapability;
                }
            }
            catch
            {
                return (int)CLI_ExitCode.success;
            }
        }

        private async Task<(int code, string result)> ReadColorPreset(IDeviceManagerSA devMgr, string type, List<string> index, List<string> serviceTag, List<string> model, string value = "")
        {
            //if (devMgr == null)
            //{
            //    writelog("ReadColorPreset: input null IDeviceManagerSA");
            //    return (int)CLI_ExitCode.null_device_manager;
            //}

            writelog($"ReadColorPreset Entry");
            if (_AllInfoMonitors == null)
                _AllInfoMonitors = await devMgr.GetMonitors(); //_DisplayPlugin.GetMonitors();
            string output = string.Empty;

            if (type == "SET")
            {
                if (string.IsNullOrEmpty(value))
                {
                    CLI_RESPONSE ret = new CLI_RESPONSE()
                    {
                        Command = type,
                        Result = "FAIL",
                        Message = "Empty input value"
                    };
                    output = JsonConvert.SerializeObject(ret, Formatting.Indented);
                    writelog($"ReadColorPreset SET return value fail:{output}");
                    return ((int)CLI_ExitCode.fail_FormantError, output);
                }

                // jim modify 20240608
                bool r = false;
                bool ever_fail = false;
                Dictionary<string, string> colorPresetList = new Dictionary<string, string>()
                {
                    {"CUSTOM", "CUSTOM COLOR"},
                    {"FPSGAME", "FPS GAME"},
                    {"RTSGAME", "RTS GAME"},
                    {"RPGGAME", "RPG GAME"},
                    {"MOVIEHDR", "MOVIE HDR"},
                    {"GAMEHDR", "GAME HDR"},
                    {"DISPLAYHDR", "DISPLAY HDR"},
                    {"DISPLAYP3", "DISPLAY P3"},
                    {"SPORTSGAME", "SPORTS Game"},
                    {"GAME1", "Game1"},
                    {"GAME2", "Game2"},
                    {"GAME3", "Game3"},
                    {"SRGB", "sRGB"},
                    {"CUSTOMCOLORHDR", "Custom Color HDR"},
                    {"HDRPEAK1000", "HDR Peak 1000"},
                    {"REC709", "Rec.709"},//PIMS-356254
                    {"BT709", "BT.709"},//PIMS-356254
                    {"BT2020", "BT.2020"},
                    {"MULTISCREENMATCH", "MULTISCREEN MATCH"},
                    {"USER1", "User 1"},
                    {"USER2", "User 2"},
                    {"USER3", "User 3"},
                    {"CUSTOM1", "Custom 1"},
                    {"CUSTOM2", "Custom 2"},
                    {"CUSTOM3", "Custom 3"},
                };
                value = colorPresetList.ContainsKey(value.ToUpper()) ? colorPresetList[value.ToUpper()] : value;
                if (index.Count == 0 && serviceTag.Count == 0 && model.Count == 0)
                {
                    foreach (var monitor in _AllInfoMonitors)
                    {
                        // 20240619 jim modify
                        //n_index = _AllInfoMonitors.FindIndex(x => x.edid == monitor.edid);

                        //r = devMgr.WriteColorPreset(n_index.ToString(), monitor, value).Result;
                        writelog($"WriteColorPreset set Entry");

                        r = devMgr.WriteColorPreset(monitor, value).Result;
                        writelog($"WriteColorPreset set Exit");

                        CLI_RESPONSE _Set_SupportedColorPreset_RESPONSE = new CLI_RESPONSE(monitor);
                        //_Set_SupportedColorPreset_RESPONSE.Model = monitor.modelName;
                        //_Set_SupportedColorPreset_RESPONSE.SerialNumber = monitor.edid.SerialNumber;
                        //_Set_SupportedColorPreset_RESPONSE.Index = change_0base_to_1base((monitor.Index).ToString());
                        //_Set_SupportedColorPreset_RESPONSE.ServiceTag = monitor.edid.ServiceTag.ToString();
                        _Set_SupportedColorPreset_RESPONSE.Command = "SET";
                        _Set_SupportedColorPreset_RESPONSE.TargetFeature = "COLORPRESET";
                        //_Set_SupportedColorPreset_RESPONSE.Set_SupportedColorPreset = value;
                        _Set_SupportedColorPreset_RESPONSE.Value = value;

                        if (!r)
                        {
                            _Set_SupportedColorPreset_RESPONSE.Result = "FAIL";
                            System.Console.WriteLine(JsonConvert.SerializeObject(_Set_SupportedColorPreset_RESPONSE, Formatting.Indented));
                            output += "\n" + JsonConvert.SerializeObject(_Set_SupportedColorPreset_RESPONSE, Formatting.Indented);
                            ever_fail = true;
                        }
                        else
                        {
                            _Set_SupportedColorPreset_RESPONSE.Result = "PASS";
                            System.Console.WriteLine(JsonConvert.SerializeObject(_Set_SupportedColorPreset_RESPONSE, Formatting.Indented));
                            output += "\n" + JsonConvert.SerializeObject(_Set_SupportedColorPreset_RESPONSE, Formatting.Indented);
                        }
                    }
                }
                // 20240614 jim modify
                if (index.Count != 0)
                {
                    foreach (string idx in index)
                    {
                        // 20240619 jim modify
                        //r = devMgr.WriteColorPreset(idx, _AllInfoMonitors[System.Convert.ToInt32(idx)], value).Result;
                        writelog($"WriteColorPreset set idx Entry");
                        r = devMgr.WriteColorPreset(_AllInfoMonitors[System.Convert.ToInt32(idx)], value).Result;
                        writelog($"WriteColorPreset set idx Exit");

                        CLI_RESPONSE _Set_SupportedColorPreset_RESPONSE = new CLI_RESPONSE(_AllInfoMonitors[Convert.ToInt32(idx)]);
                        // jim modify 20240608
                        //_Set_SupportedColorPreset_RESPONSE.Model = _AllInfoMonitors[System.Convert.ToInt32(idx)].modelName;
                        //_Set_SupportedColorPreset_RESPONSE.SerialNumber = _AllInfoMonitors[System.Convert.ToInt32(idx)].edid.SerialNumber;
                        //_Set_SupportedColorPreset_RESPONSE.Index = change_0base_to_1base(idx);
                        //_Set_SupportedColorPreset_RESPONSE.ServiceTag = "";
                        _Set_SupportedColorPreset_RESPONSE.Command = "SET";
                        _Set_SupportedColorPreset_RESPONSE.TargetFeature = "COLORPRESET";
                        //_Set_SupportedColorPreset_RESPONSE.Set_SupportedColorPreset = value;
                        _Set_SupportedColorPreset_RESPONSE.Value = value;

                        if (!r)
                        {
                            _Set_SupportedColorPreset_RESPONSE.Result = "FAIL";
                            System.Console.WriteLine(JsonConvert.SerializeObject(_Set_SupportedColorPreset_RESPONSE, Formatting.Indented));
                            output += "\n" + JsonConvert.SerializeObject(_Set_SupportedColorPreset_RESPONSE, Formatting.Indented);
                            ever_fail = true;
                        }
                        else
                        {
                            _Set_SupportedColorPreset_RESPONSE.Result = "PASS";
                            System.Console.WriteLine(JsonConvert.SerializeObject(_Set_SupportedColorPreset_RESPONSE, Formatting.Indented));
                            output += "\n" + JsonConvert.SerializeObject(_Set_SupportedColorPreset_RESPONSE, Formatting.Indented);
                        }
                    }
                }
                // 20240614 jim modify
                if (serviceTag.Count != 0)
                {
                    foreach (string tag in serviceTag)
                    {
                        var tmp = _AllInfoMonitors.FindAll(x => x.edid.ServiceTag.ToUpper().Equals(tag.ToUpper()));

                        foreach (MonitorInfo mo in tmp)
                        {
                            // 20240619 jim remove
                            //n_index = _AllInfoMonitors.FindIndex(x => x.edid == mo.edid);

                            // 20240619 jim modify
                            //r = devMgr.WriteColorPreset(n_index.ToString(), mo, value).Result;
                            writelog($"WriteColorPreset set tag Entry");
                            r = devMgr.WriteColorPreset(mo, value).Result;
                            writelog($"WriteColorPreset set tag Exit");

                            CLI_RESPONSE _Set_SupportedColorPreset_RESPONSE = new CLI_RESPONSE(mo);
                            //_Set_SupportedColorPreset_RESPONSE.Model = mo.modelName;
                            //_Set_SupportedColorPreset_RESPONSE.SerialNumber = mo.edid.SerialNumber;
                            //_Set_SupportedColorPreset_RESPONSE.Index = change_0base_to_1base((mo.Index).ToString());
                            //_Set_SupportedColorPreset_RESPONSE.ServiceTag = tag;
                            _Set_SupportedColorPreset_RESPONSE.Command = "SET";
                            _Set_SupportedColorPreset_RESPONSE.TargetFeature = "COLORPRESET";
                            //_Set_SupportedColorPreset_RESPONSE.Set_SupportedColorPreset = value;
                            _Set_SupportedColorPreset_RESPONSE.Value = value;

                            if (!r)
                            {
                                _Set_SupportedColorPreset_RESPONSE.Result = "FAIL";
                                System.Console.WriteLine(JsonConvert.SerializeObject(_Set_SupportedColorPreset_RESPONSE, Formatting.Indented));
                                output += "\n" + JsonConvert.SerializeObject(_Set_SupportedColorPreset_RESPONSE, Formatting.Indented);
                                ever_fail = true;
                            }
                            else
                            {
                                _Set_SupportedColorPreset_RESPONSE.Result = "PASS";
                                System.Console.WriteLine(JsonConvert.SerializeObject(_Set_SupportedColorPreset_RESPONSE, Formatting.Indented));
                                output += "\n" + JsonConvert.SerializeObject(_Set_SupportedColorPreset_RESPONSE, Formatting.Indented);
                            }
                        }
                    }
                }
                if (ever_fail)
                {
                    writelog($"WriteColorPreset fail return exit value{output}");
                    return ((int)CLI_ExitCode.functional_error, output);
                }
                writelog($"WriteColorPreset return exit value{output}");
                return ((int)CLI_ExitCode.success, output);
            }
            else if (type == "GET")//Dean 0726, should check its wording, not only use "else" to do get command
            {
                // jim add 20240718

                //List<string> _SupportedColorPreset = new List<string>();
                string _ActiveColorPreset = string.Empty;

                if (index.Count == 0 && serviceTag.Count == 0 && model.Count == 0)
                {
                    foreach (var monitor in _AllInfoMonitors)
                    {
                        writelog($"ReadCurrentColorPreset Entry");
                        _ActiveColorPreset = devMgr.ReadCurrentColorPreset(monitor).Result;
                        writelog($"ReadCurrentColorPreset Exit");

                        CLI_RESPONSE _Get_ActiveColorPresetList_RESPONSE = new CLI_RESPONSE(monitor);
                        //_Get_ActiveColorPresetList_RESPONSE.Model = monitor.modelName;
                        //_Get_ActiveColorPresetList_RESPONSE.SerialNumber = monitor.edid.SerialNumber;
                        //_Get_ActiveColorPresetList_RESPONSE.Index = change_0base_to_1base((monitor.Index).ToString());
                        //_Get_ActiveColorPresetList_RESPONSE.ServiceTag = monitor.edid.ServiceTag.ToString();
                        _Get_ActiveColorPresetList_RESPONSE.Command = "GET";
                        _Get_ActiveColorPresetList_RESPONSE.TargetFeature = "COLORPRESET";
                        //_Get_ActiveColorPresetList_RESPONSE.Get_ActiveColorPresetList = _ActiveColorPreset;
                        _Get_ActiveColorPresetList_RESPONSE.Value = _ActiveColorPreset;

                        if (string.IsNullOrEmpty(_ActiveColorPreset))
                        {
                            _Get_ActiveColorPresetList_RESPONSE.Result = "FAIL";
                            System.Console.WriteLine(JsonConvert.SerializeObject(_Get_ActiveColorPresetList_RESPONSE, Formatting.Indented));
                            output += "\n" + JsonConvert.SerializeObject(_Get_ActiveColorPresetList_RESPONSE, Formatting.Indented);
                            return ((int)CLI_ExitCode.functional_error, output);
                        }
                        else
                        {
                            //_Get_AllSupportedColorPresetList_RESPONSE.Result = "pass";
                            //System.Console.WriteLine(JsonConvert.SerializeObject(_Get_AllSupportedColorPresetList_RESPONSE, Formatting.Indented));
                            _Get_ActiveColorPresetList_RESPONSE.Result = "PASS";
                            System.Console.WriteLine(JsonConvert.SerializeObject(_Get_ActiveColorPresetList_RESPONSE, Formatting.Indented));
                            output += "\n" + JsonConvert.SerializeObject(_Get_ActiveColorPresetList_RESPONSE, Formatting.Indented);
                        }
                    }
                }
                // 20240614 jim modify
                if (index.Count != 0)
                {
                    foreach (string idx in index)
                    {
                        writelog($"ReadCurrentColorPreset idx Entry");
                        _ActiveColorPreset = devMgr.ReadCurrentColorPreset(_AllInfoMonitors[System.Convert.ToInt32(idx)]).Result;
                        writelog($"ReadCurrentColorPreset idx Exit");

                        CLI_RESPONSE _Get_ActiveColorPresetList_RESPONSE = new CLI_RESPONSE(_AllInfoMonitors[Convert.ToInt32(idx)]);
                        //_Get_ActiveColorPresetList_RESPONSE.Model = _AllInfoMonitors[System.Convert.ToInt32(idx)].modelName;
                        //_Get_ActiveColorPresetList_RESPONSE.SerialNumber = _AllInfoMonitors[System.Convert.ToInt32(idx)].edid.SerialNumber;
                        //_Get_ActiveColorPresetList_RESPONSE.Index = change_0base_to_1base(idx);
                        //_Get_ActiveColorPresetList_RESPONSE.ServiceTag = "";
                        _Get_ActiveColorPresetList_RESPONSE.Command = "GET";
                        _Get_ActiveColorPresetList_RESPONSE.TargetFeature = "COLORPRESET";
                        //_Get_ActiveColorPresetList_RESPONSE.Get_ActiveColorPresetList = _ActiveColorPreset;
                        _Get_ActiveColorPresetList_RESPONSE.Value = _ActiveColorPreset;

                        if (string.IsNullOrEmpty(_ActiveColorPreset))
                        {
                            _Get_ActiveColorPresetList_RESPONSE.Result = "FAIL";
                            System.Console.WriteLine(JsonConvert.SerializeObject(_Get_ActiveColorPresetList_RESPONSE, Formatting.Indented));
                            output += "\n" + JsonConvert.SerializeObject(_Get_ActiveColorPresetList_RESPONSE, Formatting.Indented);
                            return ((int)CLI_ExitCode.functional_error, output);
                        }
                        else
                        {
                            _Get_ActiveColorPresetList_RESPONSE.Result = "PASS";
                            System.Console.WriteLine(JsonConvert.SerializeObject(_Get_ActiveColorPresetList_RESPONSE, Formatting.Indented));
                            output += "\n" + JsonConvert.SerializeObject(_Get_ActiveColorPresetList_RESPONSE, Formatting.Indented);
                        }
                    }
                }
                // 20240614 jim modify
                if (serviceTag.Count != 0)
                {
                    foreach (string tag in serviceTag)
                    {
                        var tmp = _AllInfoMonitors.FindAll(x => x.edid.ServiceTag.ToUpper().Equals(tag.ToUpper()));
                        foreach (MonitorInfo mo in tmp)
                        {
                            writelog($"ReadCurrentColorPreset tag Entry");
                            _ActiveColorPreset = devMgr.ReadCurrentColorPreset(mo).Result;
                            writelog($"ReadCurrentColorPreset tag Exit");

                            CLI_RESPONSE _Get_ActiveColorPresetList_RESPONSE = new CLI_RESPONSE(mo);
                            //_Get_ActiveColorPresetList_RESPONSE.Model = mo.modelName;
                            //_Get_ActiveColorPresetList_RESPONSE.SerialNumber = mo.edid.SerialNumber;
                            //_Get_ActiveColorPresetList_RESPONSE.Index = change_0base_to_1base((mo.Index).ToString());
                            //_Get_ActiveColorPresetList_RESPONSE.ServiceTag = tag;
                            _Get_ActiveColorPresetList_RESPONSE.Command = "GET";
                            _Get_ActiveColorPresetList_RESPONSE.TargetFeature = "COLORPRESET";
                            //_Get_ActiveColorPresetList_RESPONSE.Get_ActiveColorPresetList = _ActiveColorPreset;
                            _Get_ActiveColorPresetList_RESPONSE.Value = _ActiveColorPreset;

                            if (string.IsNullOrEmpty(_ActiveColorPreset))
                            {
                                _Get_ActiveColorPresetList_RESPONSE.Result = "FAIL";
                                System.Console.WriteLine(JsonConvert.SerializeObject(_Get_ActiveColorPresetList_RESPONSE, Formatting.Indented));
                                output += "\n" + JsonConvert.SerializeObject(_Get_ActiveColorPresetList_RESPONSE, Formatting.Indented);
                                return ((int)CLI_ExitCode.functional_error, output);
                            }
                            else
                            {
                                _Get_ActiveColorPresetList_RESPONSE.Result = "PASS";
                                System.Console.WriteLine(JsonConvert.SerializeObject(_Get_ActiveColorPresetList_RESPONSE, Formatting.Indented));
                                output += "\n" + JsonConvert.SerializeObject(_Get_ActiveColorPresetList_RESPONSE, Formatting.Indented);
                            }
                        }
                    }
                }

                // 20240614 jim modify
                if (model.Count != 0)
                {
                    foreach (string modelName in model)
                    {
                        var tmp = _AllInfoMonitors.FindAll(x => x.modelName.ToUpper().Equals(modelName.ToUpper()));
                        foreach (MonitorInfo mo in tmp)
                        {
                            writelog($"ReadCurrentColorPreset tag Entry");
                            _ActiveColorPreset = devMgr.ReadCurrentColorPreset(mo).Result;
                            writelog($"ReadCurrentColorPreset tag Exit");

                            CLI_RESPONSE _Get_ActiveColorPresetList_RESPONSE = new CLI_RESPONSE(mo);
                            //_Get_ActiveColorPresetList_RESPONSE.Model = mo.modelName;
                            //_Get_ActiveColorPresetList_RESPONSE.SerialNumber = mo.edid.SerialNumber;
                            //_Get_ActiveColorPresetList_RESPONSE.Index = change_0base_to_1base((mo.Index).ToString());
                            //_Get_ActiveColorPresetList_RESPONSE.ServiceTag = mo.edid.ServiceTag;
                            _Get_ActiveColorPresetList_RESPONSE.Command = "GET";
                            _Get_ActiveColorPresetList_RESPONSE.TargetFeature = "COLORPRESET";
                            //_Get_ActiveColorPresetList_RESPONSE.Get_ActiveColorPresetList = _ActiveColorPreset;
                            _Get_ActiveColorPresetList_RESPONSE.Value = _ActiveColorPreset;

                            if (string.IsNullOrEmpty(_ActiveColorPreset))
                            {
                                _Get_ActiveColorPresetList_RESPONSE.Result = "FAIL";
                                System.Console.WriteLine(JsonConvert.SerializeObject(_Get_ActiveColorPresetList_RESPONSE, Formatting.Indented));
                                output += "\n" + JsonConvert.SerializeObject(_Get_ActiveColorPresetList_RESPONSE, Formatting.Indented);
                                return ((int)CLI_ExitCode.functional_error, output);
                            }
                            else
                            {
                                _Get_ActiveColorPresetList_RESPONSE.Result = "PASS";
                                System.Console.WriteLine(JsonConvert.SerializeObject(_Get_ActiveColorPresetList_RESPONSE, Formatting.Indented));
                                output += "\n" + JsonConvert.SerializeObject(_Get_ActiveColorPresetList_RESPONSE, Formatting.Indented);
                            }
                        }
                    }
                }
                writelog($"ReadCurrentColorPreset return exit value{output}");
                return ((int)CLI_ExitCode.success, output);
            }
            else//Dean 0726 should check if command not support
            {
                CLI_RESPONSE ret = new CLI_RESPONSE()
                {
                    Command = type,
                    Result = "Un-supported command",
                    Message = "Un-supported command"
                };
                output = JsonConvert.SerializeObject(ret, Formatting.Indented);
                writelog($"ReadCurrentColorPreset fail return exit value{output}");
                return ((int)CLI_ExitCode.unknow_command, output);
            }
        }

        // jim add 20240607
        private async Task<(int code, string result)> ActiveColorProfile(IDeviceManagerSA devMgr, string type, List<string> index, List<string> serviceTag, List<string> model, string value = "")
        {
            //if (devMgr == null)
            //{
            //    writelog("ActiveColorProfile: input null IDeviceManagerSA");
            //    return (int)CLI_ExitCode.null_device_manager;
            //}

            if (_AllInfoMonitors == null)
                _AllInfoMonitors = await devMgr.GetMonitors(); //_DisplayPlugin.GetMonitors();
            string output = string.Empty;

            if (type == "SET")
            {
                if (string.IsNullOrEmpty(value))
                {
                    CLI_RESPONSE ret = new CLI_RESPONSE()
                    {
                        Command = type,
                        Result = "FAIL",
                        Message = "Empty input value"
                    };
                    output = JsonConvert.SerializeObject(ret, Formatting.Indented);
                    return ((int)CLI_ExitCode.fail_FormantError, output);
                }

                // jim modify 20240608
                bool r = false;
                int n_index = 0;

                if (index.Count == 0 && serviceTag.Count == 0 && model.Count == 0)
                {
                    foreach (var monitor in _AllInfoMonitors)
                    {
                        try
                        {
                            r = devMgr.SetMonitorProfile(monitor, value).Result;
                        }
                        catch (Exception ex)
                        {
                            writelog($"SetMonitorProfile Exception {ex.Message.ToString()}");
                        }

                        CLI_Set_AllMonitorProfile_RESPONSE _Set_AllMonitorProfile_RESPONSE = new CLI_Set_AllMonitorProfile_RESPONSE(monitor);
                        //_Set_AllMonitorProfile_RESPONSE.Index = change_0base_to_1base((monitor.Index).ToString());
                        //_Set_AllMonitorProfile_RESPONSE.ServiceTag = monitor.edid.ServiceTag.ToString();
                        _Set_AllMonitorProfile_RESPONSE.Command = "SET";
                        _Set_AllMonitorProfile_RESPONSE.TargetFeature = "ICCPROFILEBASEDONCOLORPRESET";
                        _Set_AllMonitorProfile_RESPONSE.Set_AllMonitorProfile = value;

                        if (!r)
                        {
                            _Set_AllMonitorProfile_RESPONSE.Result = "FAIL";
                            System.Console.WriteLine(JsonConvert.SerializeObject(_Set_AllMonitorProfile_RESPONSE, Formatting.Indented));
                            return ((int)CLI_ExitCode.functional_error, JsonConvert.SerializeObject(_Set_AllMonitorProfile_RESPONSE, Formatting.Indented));
                        }
                        else
                        {
                            _Set_AllMonitorProfile_RESPONSE.Result = "PASS";
                            System.Console.WriteLine(JsonConvert.SerializeObject(_Set_AllMonitorProfile_RESPONSE, Formatting.Indented));
                            output += "\n" + JsonConvert.SerializeObject(_Set_AllMonitorProfile_RESPONSE, Formatting.Indented);
                        }
                    }
                }
                // 20240614 jim modify
                if (index.Count != 0)
                {
                    foreach (string idx in index)
                    {
                        try
                        {
                            r = devMgr.SetMonitorProfile(_AllInfoMonitors[System.Convert.ToInt32(idx)], value).Result;
                        }
                        catch (Exception ex)
                        {
                            writelog($"SetMonitorProfile Exception {ex.Message.ToString()}");
                        }

                        // jim modify 20240608
                        CLI_Set_AllMonitorProfile_RESPONSE _Set_AllMonitorProfile_RESPONSE = new CLI_Set_AllMonitorProfile_RESPONSE(_AllInfoMonitors[Convert.ToInt32(idx)]);
                        //_Set_AllMonitorProfile_RESPONSE.Index = change_0base_to_1base(idx);
                        //_Set_AllMonitorProfile_RESPONSE.ServiceTag = "";
                        _Set_AllMonitorProfile_RESPONSE.Command = "SET";
                        _Set_AllMonitorProfile_RESPONSE.TargetFeature = "ICCPROFILEBASEDONCOLORPRESET";
                        _Set_AllMonitorProfile_RESPONSE.Set_AllMonitorProfile = value;

                        if (!r)
                        {
                            _Set_AllMonitorProfile_RESPONSE.Result = "FAIL";
                            System.Console.WriteLine(JsonConvert.SerializeObject(_Set_AllMonitorProfile_RESPONSE, Formatting.Indented));
                            return ((int)CLI_ExitCode.functional_error, JsonConvert.SerializeObject(_Set_AllMonitorProfile_RESPONSE, Formatting.Indented));
                        }
                        else
                        {
                            _Set_AllMonitorProfile_RESPONSE.Result = "PASS";
                            System.Console.WriteLine(JsonConvert.SerializeObject(_Set_AllMonitorProfile_RESPONSE, Formatting.Indented));
                            output += "\n" + JsonConvert.SerializeObject(_Set_AllMonitorProfile_RESPONSE, Formatting.Indented);
                        }
                    }
                }
                // 20240614 jim modify
                if (serviceTag.Count != 0)
                {
                    foreach (string tag in serviceTag)
                    {
                        var tmp = _AllInfoMonitors.FindAll(x => x.edid.ServiceTag.ToUpper().Equals(tag.ToUpper()));

                        foreach (MonitorInfo mo in tmp)
                        {
                            try
                            {
                                r = devMgr.SetMonitorProfile(mo, value).Result;
                            }
                            catch (Exception ex)
                            {
                                writelog($"SetMonitorProfile Exception {ex.Message.ToString()}");
                            }

                            CLI_Set_AllMonitorProfile_RESPONSE _Set_AllMonitorProfile_RESPONSE = new CLI_Set_AllMonitorProfile_RESPONSE(mo);
                            //_Set_AllMonitorProfile_RESPONSE.Index = change_0base_to_1base((mo.Index).ToString());
                            //_Set_AllMonitorProfile_RESPONSE.ServiceTag = tag;
                            _Set_AllMonitorProfile_RESPONSE.Command = "SET";
                            _Set_AllMonitorProfile_RESPONSE.TargetFeature = "ICCPROFILEBASEDONCOLORPRESET";
                            _Set_AllMonitorProfile_RESPONSE.Set_AllMonitorProfile = value;

                            if (!r)
                            {
                                _Set_AllMonitorProfile_RESPONSE.Result = "FAIL";
                                System.Console.WriteLine(JsonConvert.SerializeObject(_Set_AllMonitorProfile_RESPONSE, Formatting.Indented));
                                return ((int)CLI_ExitCode.functional_error, JsonConvert.SerializeObject(_Set_AllMonitorProfile_RESPONSE, Formatting.Indented));
                            }
                            else
                            {
                                _Set_AllMonitorProfile_RESPONSE.Result = "PASS";
                                System.Console.WriteLine(JsonConvert.SerializeObject(_Set_AllMonitorProfile_RESPONSE, Formatting.Indented));
                                output += "\n" + JsonConvert.SerializeObject(_Set_AllMonitorProfile_RESPONSE, Formatting.Indented);
                            }
                        }
                    }
                }

                // 20240614 jim modify
                if (model.Count != 0)
                {
                    foreach (string modelName in model)
                    {
                        var tmp = _AllInfoMonitors.FindAll(x => x.edid.ModelName.ToUpper().Equals(modelName.ToUpper()));

                        foreach (MonitorInfo mo in tmp)
                        {
                            try
                            {
                                r = devMgr.SetMonitorProfile(mo, value).Result;
                            }
                            catch (Exception ex)
                            {
                                writelog($"SetMonitorProfile Exception {ex.Message.ToString()}");
                            }

                            CLI_Set_AllMonitorProfile_RESPONSE _Set_AllMonitorProfile_RESPONSE = new CLI_Set_AllMonitorProfile_RESPONSE(mo);
                            //_Set_AllMonitorProfile_RESPONSE.Index = change_0base_to_1base((mo.Index).ToString());
                            //_Set_AllMonitorProfile_RESPONSE.ServiceTag = mo.edid.ServiceTag;
                            _Set_AllMonitorProfile_RESPONSE.Command = "SET";
                            _Set_AllMonitorProfile_RESPONSE.TargetFeature = "ICCPROFILEBASEDONCOLORPRESET";
                            _Set_AllMonitorProfile_RESPONSE.Set_AllMonitorProfile = value;

                            if (!r)
                            {
                                _Set_AllMonitorProfile_RESPONSE.Result = "FAIL";
                                System.Console.WriteLine(JsonConvert.SerializeObject(_Set_AllMonitorProfile_RESPONSE, Formatting.Indented));
                                return ((int)CLI_ExitCode.functional_error, JsonConvert.SerializeObject(_Set_AllMonitorProfile_RESPONSE, Formatting.Indented));
                            }
                            else
                            {
                                _Set_AllMonitorProfile_RESPONSE.Result = "PASS";
                                System.Console.WriteLine(JsonConvert.SerializeObject(_Set_AllMonitorProfile_RESPONSE, Formatting.Indented));
                                output += "\n" + JsonConvert.SerializeObject(_Set_AllMonitorProfile_RESPONSE, Formatting.Indented);
                            }
                        }
                    }
                }
                return ((int)CLI_ExitCode.success, output);
            }
            else if (type == "GET") //Dean 0726 should check if type matched as well
            {
                // jim add 20240608

                string Key_Profile_Name = string.Empty;

                if (index.Count == 0 && serviceTag.Count == 0)
                {
                    foreach (var monitor in _AllInfoMonitors)
                    {
                        Key_Profile_Name = string.Empty;
                        try
                        {
                            Key_Profile_Name = devMgr.GetMonitorProfile(monitor).Result;
                        }
                        catch (Exception ex)
                        {
                            writelog($"GetMonitorProfile Exception {ex.Message.ToString()}");
                        }

                        // jim modify 20240608
                        CLI_Get_AllMonitorProfile_RESPONSE _Get_AllMonitorProfile_RESPONSE = new CLI_Get_AllMonitorProfile_RESPONSE(monitor);
                        //_Get_AllMonitorProfile_RESPONSE.Index = change_0base_to_1base((monitor.Index).ToString());
                        //_Get_AllMonitorProfile_RESPONSE.ServiceTag = monitor.edid.ServiceTag.ToString();
                        _Get_AllMonitorProfile_RESPONSE.Command = "GET";
                        _Get_AllMonitorProfile_RESPONSE.TargetFeature = "COLORPROFILE";
                        _Get_AllMonitorProfile_RESPONSE.Get_AllMonitorProfile = Key_Profile_Name;

                        if (String.IsNullOrEmpty(Key_Profile_Name))
                        {
                            _Get_AllMonitorProfile_RESPONSE.Result = "FAIL";
                            System.Console.WriteLine(JsonConvert.SerializeObject(_Get_AllMonitorProfile_RESPONSE, Formatting.Indented));
                            return ((int)CLI_ExitCode.functional_error, JsonConvert.SerializeObject(_Get_AllMonitorProfile_RESPONSE, Formatting.Indented));
                        }
                        else
                        {
                            _Get_AllMonitorProfile_RESPONSE.Result = "PASS";
                            System.Console.WriteLine(JsonConvert.SerializeObject(_Get_AllMonitorProfile_RESPONSE, Formatting.Indented));
                            output += "\n" + JsonConvert.SerializeObject(_Get_AllMonitorProfile_RESPONSE, Formatting.Indented);
                        }
                    }
                }
                // 20240614 jim modify
                if (index.Count != 0)
                {
                    foreach (string idx in index)
                    {
                        Key_Profile_Name = string.Empty;

                        try
                        {
                            Key_Profile_Name = devMgr.GetMonitorProfile(_AllInfoMonitors[System.Convert.ToInt32(idx)]).Result;
                        }
                        catch (Exception ex)
                        {
                            writelog($"GetMonitorProfile Exception {ex.Message.ToString()}");
                        }

                        // jim modify 20240608
                        CLI_Get_AllMonitorProfile_RESPONSE _Get_AllMonitorProfile_RESPONSE = new CLI_Get_AllMonitorProfile_RESPONSE(_AllInfoMonitors[Convert.ToInt32(idx)]);
                        //_Get_AllMonitorProfile_RESPONSE.Index = change_0base_to_1base(idx);
                        //_Get_AllMonitorProfile_RESPONSE.ServiceTag = "";
                        _Get_AllMonitorProfile_RESPONSE.Command = "GET";
                        _Get_AllMonitorProfile_RESPONSE.TargetFeature = "COLORPROFILE";
                        _Get_AllMonitorProfile_RESPONSE.Get_AllMonitorProfile = Key_Profile_Name;

                        if (String.IsNullOrEmpty(Key_Profile_Name))
                        {
                            _Get_AllMonitorProfile_RESPONSE.Result = "FAIL";
                            System.Console.WriteLine(JsonConvert.SerializeObject(_Get_AllMonitorProfile_RESPONSE, Formatting.Indented));
                            return ((int)CLI_ExitCode.functional_error, JsonConvert.SerializeObject(_Get_AllMonitorProfile_RESPONSE, Formatting.Indented));
                        }
                        else
                        {
                            _Get_AllMonitorProfile_RESPONSE.Result = "PASS";
                            System.Console.WriteLine(JsonConvert.SerializeObject(_Get_AllMonitorProfile_RESPONSE, Formatting.Indented));
                            output += "\n" + JsonConvert.SerializeObject(_Get_AllMonitorProfile_RESPONSE, Formatting.Indented);
                        }
                    }
                }
                // 20240614 jim modify
                if (serviceTag.Count != 0)
                {
                    foreach (string tag in serviceTag)
                    {
                        int rc = 0;
                        var tmp = _AllInfoMonitors.FindAll(x => x.edid.ServiceTag.ToUpper().Equals(tag.ToUpper()));
                        foreach (MonitorInfo mo in tmp)
                        {
                            Key_Profile_Name = string.Empty;

                            try
                            {
                                Key_Profile_Name = devMgr.GetMonitorProfile(mo).Result;
                            }
                            catch (Exception ex)
                            {
                                writelog($"GetMonitorProfile Exception {ex.Message.ToString()}");
                            }

                            // jim modify 20240608
                            CLI_Get_AllMonitorProfile_RESPONSE _Get_AllMonitorProfile_RESPONSE = new CLI_Get_AllMonitorProfile_RESPONSE(mo);
                            //_Get_AllMonitorProfile_RESPONSE.Index = change_0base_to_1base((mo.Index).ToString());
                            //_Get_AllMonitorProfile_RESPONSE.ServiceTag = tag;
                            _Get_AllMonitorProfile_RESPONSE.Command = "GET";
                            _Get_AllMonitorProfile_RESPONSE.TargetFeature = "COLORPROFILE";
                            _Get_AllMonitorProfile_RESPONSE.Get_AllMonitorProfile = Key_Profile_Name;

                            if (String.IsNullOrEmpty(Key_Profile_Name))
                            {
                                _Get_AllMonitorProfile_RESPONSE.Result = "FAIL";
                                System.Console.WriteLine(JsonConvert.SerializeObject(_Get_AllMonitorProfile_RESPONSE, Formatting.Indented));
                                return ((int)CLI_ExitCode.functional_error, JsonConvert.SerializeObject(_Get_AllMonitorProfile_RESPONSE, Formatting.Indented));
                            }
                            else
                            {
                                _Get_AllMonitorProfile_RESPONSE.Result = "PASS";
                                System.Console.WriteLine(JsonConvert.SerializeObject(_Get_AllMonitorProfile_RESPONSE, Formatting.Indented));
                                output += "\n" + JsonConvert.SerializeObject(_Get_AllMonitorProfile_RESPONSE, Formatting.Indented);
                            }
                        }
                    }
                }
                return ((int)CLI_ExitCode.success, output);
            }
            else
            {
                CLI_RESPONSE ret = new CLI_RESPONSE()
                {
                    Command = type,
                    Result = "Un-supported command",
                    Message = "Un-supported command"
                };
                output = JsonConvert.SerializeObject(ret, Formatting.Indented);
                return ((int)CLI_ExitCode.unknow_command, output);
            }
        }

        // jim add 20240607
        private async Task<(int code, string result)> ActiveColorPreset(IDeviceManagerSA devMgr, string type, List<string> index, List<string> serviceTag, List<string> model, string value = "")
        {
            //if (devMgr == null)
            //{
            //    writelog("ActiveColorPreset: input null IDeviceManagerSA");
            //    return (int)CLI_ExitCode.null_device_manager;
            //}

            if (_AllInfoMonitors == null)
                _AllInfoMonitors = await devMgr.GetMonitors(); //_DisplayPlugin.GetMonitors();
            string output = string.Empty;

            if (type == "SET")
            {
                if (string.IsNullOrEmpty(value))
                {
                    CLI_RESPONSE ret = new CLI_RESPONSE()
                    {
                        Command = type,
                        Result = "FAIL",
                        Message = "Empty input value"
                    };
                    output = JsonConvert.SerializeObject(ret, Formatting.Indented);
                    return ((int)CLI_ExitCode.fail_FormantError, output);
                }

                // jim modify 20240608
                bool r = false;

                if (index.Count == 0 && serviceTag.Count == 0 && model.Count == 0)
                {
                    foreach (var monitor in _AllInfoMonitors)
                    {
                        r = devMgr.WriteColorPresetByColorProfile(monitor, value).Result;

                        CLI_Set_AllActiveColorPreset_RESPONSE _Set_AllActiveColorPreset_RESPONSE = new CLI_Set_AllActiveColorPreset_RESPONSE(monitor);
                        //_Set_AllActiveColorPreset_RESPONSE.Index = change_0base_to_1base((monitor.Index).ToString());
                        //_Set_AllActiveColorPreset_RESPONSE.ServiceTag = monitor.edid.ServiceTag.ToString();
                        _Set_AllActiveColorPreset_RESPONSE.Command = "SET";
                        _Set_AllActiveColorPreset_RESPONSE.TargetFeature = "COLORPRESETBASEDONICCPROFILE";
                        _Set_AllActiveColorPreset_RESPONSE.Set_AllActiveColorPreset = value;

                        if (!r)
                        {
                            ;
                            _Set_AllActiveColorPreset_RESPONSE.Result = "FAIL";
                            System.Console.WriteLine(JsonConvert.SerializeObject(_Set_AllActiveColorPreset_RESPONSE, Formatting.Indented));
                            return ((int)CLI_ExitCode.functional_error, JsonConvert.SerializeObject(_Set_AllActiveColorPreset_RESPONSE, Formatting.Indented));
                        }
                        else
                        {
                            _Set_AllActiveColorPreset_RESPONSE.Result = "PASS";
                            System.Console.WriteLine(JsonConvert.SerializeObject(_Set_AllActiveColorPreset_RESPONSE, Formatting.Indented));
                            output += "\n" + JsonConvert.SerializeObject(_Set_AllActiveColorPreset_RESPONSE, Formatting.Indented);
                        }
                    }
                }
                // 20240614 jim modify
                if (index.Count != 0)
                {
                    foreach (string idx in index)
                    {
                        r = devMgr.WriteColorPresetByColorProfile(_AllInfoMonitors[System.Convert.ToInt32(idx)], value).Result;

                        CLI_Set_AllActiveColorPreset_RESPONSE _Set_AllActiveColorPreset_RESPONSE = new CLI_Set_AllActiveColorPreset_RESPONSE(_AllInfoMonitors[Convert.ToInt32(idx)]);
                        //_Set_AllActiveColorPreset_RESPONSE.Index = change_0base_to_1base(idx);
                        //_Set_AllActiveColorPreset_RESPONSE.ServiceTag = "";
                        _Set_AllActiveColorPreset_RESPONSE.Command = "SET";
                        _Set_AllActiveColorPreset_RESPONSE.TargetFeature = "COLORPRESETBASEDONICCPROFILE";
                        _Set_AllActiveColorPreset_RESPONSE.Set_AllActiveColorPreset = value;

                        if (!r)
                        {
                            _Set_AllActiveColorPreset_RESPONSE.Result = "FAIL";
                            System.Console.WriteLine(JsonConvert.SerializeObject(_Set_AllActiveColorPreset_RESPONSE, Formatting.Indented));
                            return ((int)CLI_ExitCode.functional_error, JsonConvert.SerializeObject(_Set_AllActiveColorPreset_RESPONSE, Formatting.Indented));
                        }
                        else
                        {
                            _Set_AllActiveColorPreset_RESPONSE.Result = "PASS";
                            System.Console.WriteLine(JsonConvert.SerializeObject(_Set_AllActiveColorPreset_RESPONSE, Formatting.Indented));
                            output += "\n" + JsonConvert.SerializeObject(_Set_AllActiveColorPreset_RESPONSE, Formatting.Indented);
                        }
                    }
                }
                // 20240614 jim modify
                if (serviceTag.Count != 0)
                {
                    foreach (string tag in serviceTag)
                    {
                        var tmp = _AllInfoMonitors.FindAll(x => x.edid.ServiceTag.ToUpper().Equals(tag.ToUpper()));

                        foreach (MonitorInfo mo in tmp)
                        {
                            r = devMgr.WriteColorPresetByColorProfile(mo, value).Result;

                            CLI_Set_AllActiveColorPreset_RESPONSE _Set_AllActiveColorPreset_RESPONSE = new CLI_Set_AllActiveColorPreset_RESPONSE(mo);
                            //_Set_AllActiveColorPreset_RESPONSE.Index = change_0base_to_1base((mo.Index).ToString());
                            //_Set_AllActiveColorPreset_RESPONSE.ServiceTag = tag;
                            _Set_AllActiveColorPreset_RESPONSE.Command = "SET";
                            _Set_AllActiveColorPreset_RESPONSE.TargetFeature = "COLORPRESETBASEDONICCPROFILE";
                            _Set_AllActiveColorPreset_RESPONSE.Set_AllActiveColorPreset = value;

                            if (!r)
                            {
                                _Set_AllActiveColorPreset_RESPONSE.Result = "FAIL";
                                System.Console.WriteLine(JsonConvert.SerializeObject(_Set_AllActiveColorPreset_RESPONSE, Formatting.Indented));
                                return ((int)CLI_ExitCode.functional_error, JsonConvert.SerializeObject(_Set_AllActiveColorPreset_RESPONSE, Formatting.Indented));
                            }
                            else
                            {
                                _Set_AllActiveColorPreset_RESPONSE.Result = "PASS";
                                System.Console.WriteLine(JsonConvert.SerializeObject(_Set_AllActiveColorPreset_RESPONSE, Formatting.Indented));
                                output += "\n" + JsonConvert.SerializeObject(_Set_AllActiveColorPreset_RESPONSE, Formatting.Indented);
                            }
                        }
                    }
                }

                // 20240614 jim modify
                if (model.Count != 0)
                {
                    foreach (string modelName in model)
                    {
                        var tmp = _AllInfoMonitors.FindAll(x => x.edid.ModelName.ToUpper().Equals(modelName.ToUpper()));

                        foreach (MonitorInfo mo in tmp)
                        {
                            r = devMgr.WriteColorPresetByColorProfile(mo, value).Result;

                            CLI_Set_AllActiveColorPreset_RESPONSE _Set_AllActiveColorPreset_RESPONSE = new CLI_Set_AllActiveColorPreset_RESPONSE(mo);
                            //_Set_AllActiveColorPreset_RESPONSE.Index = change_0base_to_1base((mo.Index).ToString());
                            //_Set_AllActiveColorPreset_RESPONSE.ServiceTag = mo.edid.ServiceTag;
                            _Set_AllActiveColorPreset_RESPONSE.Command = "SET";
                            _Set_AllActiveColorPreset_RESPONSE.TargetFeature = "COLORPRESETBASEDONICCPROFILE";
                            _Set_AllActiveColorPreset_RESPONSE.Set_AllActiveColorPreset = value;

                            if (!r)
                            {
                                _Set_AllActiveColorPreset_RESPONSE.Result = "FAIL";
                                System.Console.WriteLine(JsonConvert.SerializeObject(_Set_AllActiveColorPreset_RESPONSE, Formatting.Indented));
                                return ((int)CLI_ExitCode.functional_error, JsonConvert.SerializeObject(_Set_AllActiveColorPreset_RESPONSE, Formatting.Indented));
                            }
                            else
                            {
                                _Set_AllActiveColorPreset_RESPONSE.Result = "PASS";
                                System.Console.WriteLine(JsonConvert.SerializeObject(_Set_AllActiveColorPreset_RESPONSE, Formatting.Indented));
                                output += "\n" + JsonConvert.SerializeObject(_Set_AllActiveColorPreset_RESPONSE, Formatting.Indented);
                            }
                        }
                    }
                }
                return ((int)CLI_ExitCode.success, output);
            }
            else
            {
                CLI_RESPONSE ret = new CLI_RESPONSE()
                {
                    Command = type,
                    Result = "Un-supported command",
                    Message = "Un-supported command"
                };
                output = JsonConvert.SerializeObject(ret, Formatting.Indented);
                return ((int)CLI_ExitCode.unknow_command, output);
            }
        }

        // jim add 20240607
        private async Task<(int code, string result)> ColorManagement(IDeviceManagerSA devMgr, CommandLineInput commandLineInput)
        {
            string output = string.Empty;
            string rc = string.Empty;
            string rc_ = string.Empty;
            string bymonitor_byhost = string.Empty;
            string ColorPreset = string.Empty;
            string ICC_profile = string.Empty;

            bool retcode = false;

            if (_AllInfoMonitors == null)
                _AllInfoMonitors = await devMgr.GetMonitors();

            if (commandLineInput.Command == "SET" && commandLineInput.Options[0].Option_Value != null)
            {
                List<int> _monitorIndeies = new List<int>();

                if (_AllInfoMonitors == null)
                    _AllInfoMonitors = devMgr.GetMonitors().Result;
                _monitorIndeies = GetMonitorIndeies(commandLineInput, _AllInfoMonitors);

                foreach (int idx in _monitorIndeies)
                {

                    MonitorInfo monitor = _AllInfoMonitors[idx];

                    CLI_RESPONSE cli_Response = new CLI_RESPONSE(monitor);
                    cli_Response.Command = commandLineInput.Command;
                    cli_Response.TargetFeature = commandLineInput.TargetFeature;

                    switch (commandLineInput.Options[0].Option_Value.ToUpper())
                    {
                        case "BYMONITOR":
                            writelog($"ColorManagement BYMONITOR set entry");
                            bymonitor_byhost = "BYMONITOR";
                            writelog("set devMgr.AutoColorManagementForMonitorConfig entry");
                            devMgr.AutoColorManagementForMonitorConfig(monitor, bymonitor_byhost, ColorPreset, ICC_profile);
                            writelog("set devMgr.AutoColorManagementForMonitorConfig exit");
                            writelog("set devMgr.GetColorManagementStatus entry");
                            rc_ = devMgr.GetColorManagementStatus(monitor).Result;
                            writelog("set devMgr.GetColorManagementStatus exit");
                            Trace.WriteLine($"Rc_ = {rc_}");
                            if (rc_.Equals("BYMONITOR", StringComparison.OrdinalIgnoreCase))
                            {
                                cli_Response.Result = "PASS";
                                cli_Response.Value = rc_;
                                retcode = true;
                            }
                            else
                            {
                                cli_Response.Result = "FAIL";
                                cli_Response.Value = "N/A";
                            }
                            break;

                        case "BYHOST":
                            writelog($"ColorManagement BYHOST set entry");
                            bymonitor_byhost = "Byhost";
                            writelog("set devMgr.AutoColorManagementForMonitorConfig entry");
                            devMgr.AutoColorManagementForMonitorConfig(monitor, bymonitor_byhost, ColorPreset, ICC_profile);
                            writelog("set devMgr.AutoColorManagementForMonitorConfig exit");
                            writelog("set devMgr.GetColorManagementStatus entry");
                            rc_ = devMgr.GetColorManagementStatus(monitor).Result;
                            writelog("set devMgr.GetColorManagementStatus exit");
                            if (rc_.Equals("BYHOST", StringComparison.OrdinalIgnoreCase))
                            {
                                cli_Response.Result = "PASS";
                                cli_Response.Value = rc_;
                                retcode = true;
                            }
                            else
                            {
                                cli_Response.Result = "FAIL";
                                cli_Response.Value = "N/A";
                            }
                            break;

                        case "OFF":
                            writelog($"ColorManagement OFF set entry");
                            bymonitor_byhost = "off";
                            writelog("set devMgr.AutoColorManagementForMonitorConfig entry");
                            devMgr.AutoColorManagementForMonitorConfig(monitor, bymonitor_byhost, ColorPreset, ICC_profile);
                            writelog("set devMgr.AutoColorManagementForMonitorConfig exit");
                            writelog("set devMgr.GetColorManagementStatus entry");
                            rc_ = devMgr.GetColorManagementStatus(monitor).Result;
                            writelog("set devMgr.GetColorManagementStatus exit");
                            if (rc_.Equals("OFF", StringComparison.OrdinalIgnoreCase))
                            {
                                cli_Response.Result = "PASS";
                                cli_Response.Value = rc_;
                                retcode = true;
                            }
                            else
                            {
                                cli_Response.Result = "FAIL";
                                cli_Response.Value = "N/A";
                            }
                            break;

                        default:
                            writelog($"option value not support");

                            cli_Response.Command = commandLineInput.Command;
                            cli_Response.TargetFeature = commandLineInput.TargetFeature;
                            cli_Response.Result = "FAIL";
                            cli_Response.Message = "Invalid command line syntax, missing -value=... or more than one -value=...";
                            break;
                    }

                    System.Console.WriteLine(JsonConvert.SerializeObject(cli_Response, Formatting.Indented));
                    output += "\n" + JsonConvert.SerializeObject(cli_Response, Formatting.Indented);
                }
            }
            if (commandLineInput.Command == "GET")
            {
                List<int> _monitorIndeies = new List<int>();
                if (_AllInfoMonitors == null)
                    _AllInfoMonitors = devMgr.GetMonitors().Result;
                _monitorIndeies = GetMonitorIndeies(commandLineInput, _AllInfoMonitors);

                foreach (int idx in _monitorIndeies)
                {
                    writelog($"ColorManagement get entry");
                    MonitorInfo monitor = _AllInfoMonitors[idx];
                    CLI_RESPONSE cli_Response = new CLI_RESPONSE(monitor);
                    cli_Response.Command = commandLineInput.Command;
                    cli_Response.TargetFeature = commandLineInput.TargetFeature;
                    writelog($"devMgr.GetColorManagementStatus get entry");
                    rc = devMgr.GetColorManagementStatus(monitor).Result;
                    writelog($"devMgr.GetColorManagementStatus get exit");
                    if (rc != null)
                    {
                        cli_Response.Result = "PASS";
                        cli_Response.Value = rc;
                        retcode = true;
                    }
                    else
                    {
                        cli_Response.Result = "FAIL";
                        cli_Response.Value = "N/A";
                    }

                    System.Console.WriteLine(JsonConvert.SerializeObject(cli_Response, Formatting.Indented));
                    output += "\n" + JsonConvert.SerializeObject(cli_Response, Formatting.Indented);
                }
            }
            writelog($"ColorManagement exit return value : {output}");
            return (retcode ? (int)CLI_ExitCode.success : (int)CLI_ExitCode.functional_error, output);
        }

        private async Task<(int code, string result)> RestoreFactoryDefaults(IDeviceManagerSA devMgr, string type, List<string> index, List<string> serviceTag, List<string> model, string value = "")
        {
            //if (devMgr == null)
            //{
            //    writelog("RestoreFactoryDefault: input null IDeviceManagerSA");
            //    return (int)CLI_ExitCode.null_device_manager;
            //}

            if (_AllInfoMonitors == null)
                _AllInfoMonitors = await devMgr.GetMonitors(); //_DisplayPlugin.GetMonitors();
            string output = string.Empty;

            if (type == "SET")
            {
                // jim modify 20240608
                bool r = false;
                int n_index = 0;
                int exit = 0;

                if (index.Count == 0 && serviceTag.Count == 0 && model.Count == 0)
                {
                    foreach (var monitor in _AllInfoMonitors)
                    {
                        CLI_RESPONSE _Set_CLI_RESPONSE_RESPONSE = new CLI_RESPONSE(monitor);
                        // jim modify 20240715
                        writelog($"RestoreFactoryDefaults entry");
                        r = SetVCPCode(devMgr, monitor, "0x04", "0x01").Result;

                        _Set_CLI_RESPONSE_RESPONSE.Command = "SET";
                        _Set_CLI_RESPONSE_RESPONSE.TargetFeature = "RESTOREFACTORYDEFAULTS";

                        if (!r)
                        {
                            _Set_CLI_RESPONSE_RESPONSE.Result = "FAIL";
                            System.Console.WriteLine(JsonConvert.SerializeObject(_Set_CLI_RESPONSE_RESPONSE, Formatting.Indented));
                            output += "\n" + JsonConvert.SerializeObject(_Set_CLI_RESPONSE_RESPONSE, Formatting.Indented);
                            writelog($"RestoreFactoryDefaults fail{output}");
                            if (exit == 0)
                                exit = (int)CLI_ExitCode.functional_error;
                        }
                        else
                        {
                            _Set_CLI_RESPONSE_RESPONSE.Result = "PASS";
                            System.Console.WriteLine(JsonConvert.SerializeObject(_Set_CLI_RESPONSE_RESPONSE, Formatting.Indented));
                            output += "\n" + JsonConvert.SerializeObject(_Set_CLI_RESPONSE_RESPONSE, Formatting.Indented);
                            writelog($"RestoreFactoryDefaults pass{output}");
                        }
                    }
                }
                // 20240614 jim modify
                if (index.Count != 0)
                {
                    foreach (string idx in index)
                    {
                        CLI_RESPONSE _Set_CLI_RESPONSE_RESPONSE = new CLI_RESPONSE(_AllInfoMonitors[Convert.ToInt32(idx)]);
                        // jim modify 20240802
                        writelog($"RestoreFactoryDefaults idx entry");
                        r = SetVCPCode(devMgr, _AllInfoMonitors[System.Convert.ToInt32(idx)], "0x04", "0x01").Result;

                        _Set_CLI_RESPONSE_RESPONSE.Command = "SET";
                        _Set_CLI_RESPONSE_RESPONSE.TargetFeature = "RESTOREFACTORYDEFAULTS";

                        if (!r)
                        {
                            _Set_CLI_RESPONSE_RESPONSE.Result = "FAIL";
                            System.Console.WriteLine(JsonConvert.SerializeObject(_Set_CLI_RESPONSE_RESPONSE, Formatting.Indented));
                            output += "\n" + JsonConvert.SerializeObject(_Set_CLI_RESPONSE_RESPONSE, Formatting.Indented);
                            writelog($"RestoreFactoryDefaults idx fail{output}");
                            if (exit == 0)
                                exit = (int)CLI_ExitCode.functional_error;
                        }
                        else
                        {
                            _Set_CLI_RESPONSE_RESPONSE.Result = "PASS";
                            System.Console.WriteLine(JsonConvert.SerializeObject(_Set_CLI_RESPONSE_RESPONSE, Formatting.Indented));
                            output += "\n" + JsonConvert.SerializeObject(_Set_CLI_RESPONSE_RESPONSE, Formatting.Indented);
                            writelog($"RestoreFactoryDefaults idx pass{output}");
                        }
                    }
                }
                // 20240614 jim modify
                if (serviceTag.Count != 0)
                {
                    foreach (string tag in serviceTag)
                    {
                        var tmp = _AllInfoMonitors.FindAll(x => x.edid.ServiceTag.ToUpper().Equals(tag.ToUpper()));

                        foreach (MonitorInfo mo in tmp)
                        {
                            CLI_RESPONSE _Set_CLI_RESPONSE_RESPONSE = new CLI_RESPONSE(mo);
                            // jim modify 20240802
                            writelog($"RestoreFactoryDefaults tag entry");
                            r = SetVCPCode(devMgr, mo, "0x04", "0x01").Result;

                            _Set_CLI_RESPONSE_RESPONSE.Command = "SET";
                            _Set_CLI_RESPONSE_RESPONSE.TargetFeature = "RESTOREFACTORYDEFAULTS";

                            if (!r)
                            {
                                _Set_CLI_RESPONSE_RESPONSE.Result = "FAIL";
                                System.Console.WriteLine(JsonConvert.SerializeObject(_Set_CLI_RESPONSE_RESPONSE, Formatting.Indented));
                                output += "\n" + JsonConvert.SerializeObject(_Set_CLI_RESPONSE_RESPONSE, Formatting.Indented);
                                writelog($"RestoreFactoryDefaults tag fail{output}");
                                if (exit == 0)
                                    exit = (int)CLI_ExitCode.functional_error;
                            }
                            else
                            {
                                _Set_CLI_RESPONSE_RESPONSE.Result = "PASS";
                                System.Console.WriteLine(JsonConvert.SerializeObject(_Set_CLI_RESPONSE_RESPONSE, Formatting.Indented));
                                output += "\n" + JsonConvert.SerializeObject(_Set_CLI_RESPONSE_RESPONSE, Formatting.Indented);
                                writelog($"RestoreFactoryDefaults tag pass{output}");
                            }
                        }
                    }
                }

                // 20240614 jim modify
                if (model.Count != 0)
                {
                    foreach (string modelName in model)
                    {
                        var tmp = _AllInfoMonitors.FindAll(x => x.edid.ModelName.ToUpper().Equals(modelName.ToUpper()));

                        foreach (MonitorInfo mo in tmp)
                        {
                            CLI_RESPONSE _Set_CLI_RESPONSE_RESPONSE = new CLI_RESPONSE(mo);
                            // jim modify 20240802
                            writelog($"RestoreFactoryDefaults tag entry");
                            r = SetVCPCode(devMgr, mo, "0x04", "0x01").Result;

                            _Set_CLI_RESPONSE_RESPONSE.Command = "SET";
                            _Set_CLI_RESPONSE_RESPONSE.TargetFeature = "RESTOREFACTORYDEFAULTS";

                            if (!r)
                            {
                                _Set_CLI_RESPONSE_RESPONSE.Result = "FAIL";
                                System.Console.WriteLine(JsonConvert.SerializeObject(_Set_CLI_RESPONSE_RESPONSE, Formatting.Indented));
                                output += "\n" + JsonConvert.SerializeObject(_Set_CLI_RESPONSE_RESPONSE, Formatting.Indented);
                                writelog($"RestoreFactoryDefaults tag fail{output}");
                                if (exit == 0)
                                    exit = (int)CLI_ExitCode.functional_error;
                            }
                            else
                            {
                                _Set_CLI_RESPONSE_RESPONSE.Result = "PASS";
                                System.Console.WriteLine(JsonConvert.SerializeObject(_Set_CLI_RESPONSE_RESPONSE, Formatting.Indented));
                                output += "\n" + JsonConvert.SerializeObject(_Set_CLI_RESPONSE_RESPONSE, Formatting.Indented);
                                writelog($"RestoreFactoryDefaults tag pass{output}");
                            }
                        }
                    }
                }
                writelog($"RestoreFactoryDefaults exit return value{output}");
                return (exit, output);
            }
            else
            {
                CLI_RESPONSE ret = new CLI_RESPONSE()
                {
                    Command = type,
                    Result = "Un-supported command",
                    Message = "Un-supported command"
                };
                output = JsonConvert.SerializeObject(ret, Formatting.Indented);
                return ((int)CLI_ExitCode.unknow_command, output);
            }
        }

        private async Task<(int code, string result)> RestoreLevelDefaults(IDeviceManagerSA devMgr, string type, List<string> index, List<string> serviceTag, List<string> model, string value = "")
        {
            if (_AllInfoMonitors == null)
                _AllInfoMonitors = await devMgr.GetMonitors(); //_DisplayPlugin.GetMonitors();
            string output = string.Empty;

            if (type == "SET")
            {
                bool r = false;
                int n_index = 0;
                int exit = 0;

                if (index.Count == 0 && serviceTag.Count == 0 && model.Count == 0)
                {
                    foreach (var monitor in _AllInfoMonitors)
                    {
                        CLI_RESPONSE _Set_CLI_RESPONSE_RESPONSE = new CLI_RESPONSE(monitor);
                        writelog($"RestoreLevelDefaults entry");
                        r = SetVCPCode(devMgr, monitor, "0x05", "0x01").Result;

                        _Set_CLI_RESPONSE_RESPONSE.Command = "SET";
                        _Set_CLI_RESPONSE_RESPONSE.TargetFeature = "RESTORELEVELDEFAULTS";

                        if (!r)
                        {
                            _Set_CLI_RESPONSE_RESPONSE.Result = "FAIL";
                            System.Console.WriteLine(JsonConvert.SerializeObject(_Set_CLI_RESPONSE_RESPONSE, Formatting.Indented));
                            output += "\n" + JsonConvert.SerializeObject(_Set_CLI_RESPONSE_RESPONSE, Formatting.Indented);
                            writelog($"RestoreLevelDefaults fail {output}");
                            if (exit == 0)
                                exit = (int)CLI_ExitCode.functional_error;
                        }
                        else
                        {
                            _Set_CLI_RESPONSE_RESPONSE.Result = "PASS";
                            System.Console.WriteLine(JsonConvert.SerializeObject(_Set_CLI_RESPONSE_RESPONSE, Formatting.Indented));
                            output += "\n" + JsonConvert.SerializeObject(_Set_CLI_RESPONSE_RESPONSE, Formatting.Indented);
                            writelog($"RestoreLevelDefaults pass{output}");
                        }
                    }
                }
                if (index.Count != 0)
                {
                    foreach (string idx in index)
                    {
                        CLI_RESPONSE _Set_CLI_RESPONSE_RESPONSE = new CLI_RESPONSE(_AllInfoMonitors[Convert.ToInt32(idx)]);
                        writelog($"RestoreLevelDefaults idx entry");
                        r = SetVCPCode(devMgr, _AllInfoMonitors[System.Convert.ToInt32(idx)], "0x05", "0x01").Result;

                        _Set_CLI_RESPONSE_RESPONSE.Command = "SET";
                        _Set_CLI_RESPONSE_RESPONSE.TargetFeature = "RESTORELEVELDEFAULTS";

                        if (!r)
                        {
                            _Set_CLI_RESPONSE_RESPONSE.Result = "FAIL";
                            System.Console.WriteLine(JsonConvert.SerializeObject(_Set_CLI_RESPONSE_RESPONSE, Formatting.Indented));
                            output += "\n" + JsonConvert.SerializeObject(_Set_CLI_RESPONSE_RESPONSE, Formatting.Indented);
                            writelog($"RestoreLevelDefaults idx fail {output}");
                            if (exit == 0)
                                exit = (int)CLI_ExitCode.functional_error;
                        }
                        else
                        {
                            _Set_CLI_RESPONSE_RESPONSE.Result = "PASS";
                            System.Console.WriteLine(JsonConvert.SerializeObject(_Set_CLI_RESPONSE_RESPONSE, Formatting.Indented));
                            output += "\n" + JsonConvert.SerializeObject(_Set_CLI_RESPONSE_RESPONSE, Formatting.Indented);
                            writelog($"RestoreLevelDefaults idx pass{output}");
                        }
                    }
                }
                if (serviceTag.Count != 0)
                {
                    foreach (string tag in serviceTag)
                    {
                        var tmp = _AllInfoMonitors.FindAll(x => x.edid.ServiceTag.ToUpper().Equals(tag.ToUpper()));

                        foreach (MonitorInfo mo in tmp)
                        {
                            CLI_RESPONSE _Set_CLI_RESPONSE_RESPONSE = new CLI_RESPONSE(mo);
                            writelog($"RestoreLevelDefaults tag entry");
                            r = SetVCPCode(devMgr, mo, "0x05", "0x01").Result;

                            _Set_CLI_RESPONSE_RESPONSE.Command = "SET";
                            _Set_CLI_RESPONSE_RESPONSE.TargetFeature = "RESTORELEVELDEFAULTS";

                            if (!r)
                            {
                                _Set_CLI_RESPONSE_RESPONSE.Result = "FAIL";
                                System.Console.WriteLine(JsonConvert.SerializeObject(_Set_CLI_RESPONSE_RESPONSE, Formatting.Indented));
                                output += "\n" + JsonConvert.SerializeObject(_Set_CLI_RESPONSE_RESPONSE, Formatting.Indented);
                                writelog($"RestoreLevelDefaults tag fail {output}");
                                if (exit == 0)
                                    exit = (int)CLI_ExitCode.functional_error;
                            }
                            else
                            {
                                _Set_CLI_RESPONSE_RESPONSE.Result = "PASS";
                                System.Console.WriteLine(JsonConvert.SerializeObject(_Set_CLI_RESPONSE_RESPONSE, Formatting.Indented));
                                output += "\n" + JsonConvert.SerializeObject(_Set_CLI_RESPONSE_RESPONSE, Formatting.Indented);
                                writelog($"RestoreLevelDefaults tag pass{output}");
                            }
                        }
                    }
                }

                if (model.Count != 0)
                {
                    foreach (string modelName in model)
                    {
                        var tmp = _AllInfoMonitors.FindAll(x => x.edid.ModelName.ToUpper().Equals(modelName.ToUpper()));

                        foreach (MonitorInfo mo in tmp)
                        {
                            CLI_RESPONSE _Set_CLI_RESPONSE_RESPONSE = new CLI_RESPONSE(mo);
                            writelog($"RestoreLevelDefaults tag entry");
                            r = SetVCPCode(devMgr, mo, "0x05", "0x01").Result;

                            _Set_CLI_RESPONSE_RESPONSE.Command = "SET";
                            _Set_CLI_RESPONSE_RESPONSE.TargetFeature = "RESTORELEVELDEFAULTS";

                            if (!r)
                            {
                                _Set_CLI_RESPONSE_RESPONSE.Result = "FAIL";
                                System.Console.WriteLine(JsonConvert.SerializeObject(_Set_CLI_RESPONSE_RESPONSE, Formatting.Indented));
                                output += "\n" + JsonConvert.SerializeObject(_Set_CLI_RESPONSE_RESPONSE, Formatting.Indented);
                                writelog($"RestoreLevelDefaults tag fail {output}");
                                if (exit == 0)
                                    exit = (int)CLI_ExitCode.functional_error;
                            }
                            else
                            {
                                _Set_CLI_RESPONSE_RESPONSE.Result = "PASS";
                                System.Console.WriteLine(JsonConvert.SerializeObject(_Set_CLI_RESPONSE_RESPONSE, Formatting.Indented));
                                output += "\n" + JsonConvert.SerializeObject(_Set_CLI_RESPONSE_RESPONSE, Formatting.Indented);
                                writelog($"RestoreLevelDefaults tag pass{output}");
                            }
                        }
                    }
                }
                writelog($"RestoreLevelDefaults exit return value pass{output}");
                return (exit, output);
            }
            else
            {
                CLI_RESPONSE ret = new CLI_RESPONSE()
                {
                    Command = type,
                    Result = "Un-supported command",
                    Message = "Un-supported command"
                };
                output = JsonConvert.SerializeObject(ret, Formatting.Indented);
                return ((int)CLI_ExitCode.unknow_command, output);
            }
        }

        private async Task<(int code, string result)> RestoreColorDefaults(IDeviceManagerSA devMgr, string type, List<string> index, List<string> serviceTag, List<string> model, string value = "")
        {
            if (_AllInfoMonitors == null)
                _AllInfoMonitors = await devMgr.GetMonitors(); //_DisplayPlugin.GetMonitors();
            string output = string.Empty;

            if (type == "SET")
            {
                bool r = false;
                int n_index = 0;
                int exit = 0;

                if (index.Count == 0 && serviceTag.Count == 0)
                {
                    foreach (var monitor in _AllInfoMonitors)
                    {
                        CLI_RESPONSE _Set_CLI_RESPONSE_RESPONSE = new CLI_RESPONSE(monitor);
                        writelog($"RestoreColorDefaults entry");
                        r = SetVCPCode(devMgr, monitor, "0x08", "0x01").Result;

                        _Set_CLI_RESPONSE_RESPONSE.Command = "SET";
                        _Set_CLI_RESPONSE_RESPONSE.TargetFeature = "RESTORECOLORDEFAULTS";

                        if (!r)
                        {
                            _Set_CLI_RESPONSE_RESPONSE.Result = "FAIL";
                            System.Console.WriteLine(JsonConvert.SerializeObject(_Set_CLI_RESPONSE_RESPONSE, Formatting.Indented));
                            output += "\n" + JsonConvert.SerializeObject(_Set_CLI_RESPONSE_RESPONSE, Formatting.Indented);
                            writelog($"RestoreColorDefaults fail{output}");
                            if (exit == 0)
                                exit = (int)CLI_ExitCode.functional_error;
                        }
                        else
                        {
                            _Set_CLI_RESPONSE_RESPONSE.Result = "PASS";
                            System.Console.WriteLine(JsonConvert.SerializeObject(_Set_CLI_RESPONSE_RESPONSE, Formatting.Indented));
                            output += "\n" + JsonConvert.SerializeObject(_Set_CLI_RESPONSE_RESPONSE, Formatting.Indented);
                            writelog($"RestoreColorDefaults pass{output}");
                        }
                    }
                }
                if (index.Count != 0)
                {
                    foreach (string idx in index)
                    {
                        CLI_RESPONSE _Set_CLI_RESPONSE_RESPONSE = new CLI_RESPONSE(_AllInfoMonitors[Convert.ToInt32(idx)]);
                        writelog($"RestoreColorDefaults idx entry");
                        r = SetVCPCode(devMgr, _AllInfoMonitors[System.Convert.ToInt32(idx)], "0x08", "0x01").Result;

                        _Set_CLI_RESPONSE_RESPONSE.Command = "SET";
                        _Set_CLI_RESPONSE_RESPONSE.TargetFeature = "RESTORECOLORDEFAULTS";

                        if (!r)
                        {
                            _Set_CLI_RESPONSE_RESPONSE.Result = "FAIL";
                            System.Console.WriteLine(JsonConvert.SerializeObject(_Set_CLI_RESPONSE_RESPONSE, Formatting.Indented));
                            output += "\n" + JsonConvert.SerializeObject(_Set_CLI_RESPONSE_RESPONSE, Formatting.Indented);
                            writelog($"RestoreColorDefaults idx fail{output}");
                            if (exit == 0)
                                exit = (int)CLI_ExitCode.functional_error;
                        }
                        else
                        {
                            _Set_CLI_RESPONSE_RESPONSE.Result = "PASS";
                            System.Console.WriteLine(JsonConvert.SerializeObject(_Set_CLI_RESPONSE_RESPONSE, Formatting.Indented));
                            output += "\n" + JsonConvert.SerializeObject(_Set_CLI_RESPONSE_RESPONSE, Formatting.Indented);
                            writelog($"RestoreColorDefaults idx pass{output}");
                        }
                    }
                }
                if (serviceTag.Count != 0)
                {
                    foreach (string tag in serviceTag)
                    {
                        var tmp = _AllInfoMonitors.FindAll(x => x.edid.ServiceTag.ToUpper().Equals(tag.ToUpper()));

                        foreach (MonitorInfo mo in tmp)
                        {
                            CLI_RESPONSE _Set_CLI_RESPONSE_RESPONSE = new CLI_RESPONSE(mo);
                            writelog($"RestoreColorDefaults tag entry");
                            r = SetVCPCode(devMgr, mo, "0x08", "0x01").Result;

                            _Set_CLI_RESPONSE_RESPONSE.Command = "SET";
                            _Set_CLI_RESPONSE_RESPONSE.TargetFeature = "RESTORECOLORDEFAULTS";

                            if (!r)
                            {
                                _Set_CLI_RESPONSE_RESPONSE.Result = "FAIL";
                                System.Console.WriteLine(JsonConvert.SerializeObject(_Set_CLI_RESPONSE_RESPONSE, Formatting.Indented));
                                output += "\n" + JsonConvert.SerializeObject(_Set_CLI_RESPONSE_RESPONSE, Formatting.Indented);
                                writelog($"RestoreColorDefaults tag fail{output}");
                                if (exit == 0)
                                    exit = (int)CLI_ExitCode.functional_error;
                            }
                            else
                            {
                                _Set_CLI_RESPONSE_RESPONSE.Result = "PASS";
                                System.Console.WriteLine(JsonConvert.SerializeObject(_Set_CLI_RESPONSE_RESPONSE, Formatting.Indented));
                                output += "\n" + JsonConvert.SerializeObject(_Set_CLI_RESPONSE_RESPONSE, Formatting.Indented);
                                writelog($"RestoreColorDefaults tag pass{output}");
                            }
                        }
                    }
                }
                if (model.Count != 0)
                {
                    foreach (string modelName in model)
                    {
                        var tmp = _AllInfoMonitors.FindAll(x => x.edid.ModelName.ToUpper().Equals(modelName.ToUpper()));

                        foreach (MonitorInfo mo in tmp)
                        {
                            CLI_RESPONSE _Set_CLI_RESPONSE_RESPONSE = new CLI_RESPONSE(mo);
                            writelog($"RestoreColorDefaults tag entry");
                            r = SetVCPCode(devMgr, mo, "0x08", "0x01").Result;

                            _Set_CLI_RESPONSE_RESPONSE.Command = "SET";
                            _Set_CLI_RESPONSE_RESPONSE.TargetFeature = "RESTORECOLORDEFAULTS";

                            if (!r)
                            {
                                _Set_CLI_RESPONSE_RESPONSE.Result = "FAIL";
                                System.Console.WriteLine(JsonConvert.SerializeObject(_Set_CLI_RESPONSE_RESPONSE, Formatting.Indented));
                                output += "\n" + JsonConvert.SerializeObject(_Set_CLI_RESPONSE_RESPONSE, Formatting.Indented);
                                writelog($"RestoreColorDefaults tag fail{output}");
                                if (exit == 0)
                                    exit = (int)CLI_ExitCode.functional_error;
                            }
                            else
                            {
                                _Set_CLI_RESPONSE_RESPONSE.Result = "PASS";
                                System.Console.WriteLine(JsonConvert.SerializeObject(_Set_CLI_RESPONSE_RESPONSE, Formatting.Indented));
                                output += "\n" + JsonConvert.SerializeObject(_Set_CLI_RESPONSE_RESPONSE, Formatting.Indented);
                                writelog($"RestoreColorDefaults tag pass{output}");
                            }
                        }
                    }
                }
                writelog($"RestoreColorDefaults exit return value {output}");
                return (exit, output);
            }
            else
            {
                CLI_RESPONSE ret = new CLI_RESPONSE()
                {
                    Command = type,
                    Result = "Un-supported command",
                    Message = "Un-supported command"
                };
                output = JsonConvert.SerializeObject(ret, Formatting.Indented);
                return ((int)CLI_ExitCode.unknow_command, output);
            }
        }

        private async Task<(int code, string result)> OSD(IDeviceManagerSA devMgr, string type, List<string> index, List<string> serviceTag, List<string> model, string value = "")
        {
            if (_AllInfoMonitors == null)
                _AllInfoMonitors = await devMgr.GetMonitors(); //_DisplayPlugin.GetMonitors();
            string output = string.Empty;

            if (type == "SET")
            {
                bool r = false;
                int n_index = 0;
                int exit = 0;

                if (index.Count == 0 && serviceTag.Count == 0 && model.Count == 0)
                {
                    foreach (var monitor in _AllInfoMonitors)
                    {
                        writelog($"OSDAccess entry");
                        CLI_RESPONSE _Set_CLI_RESPONSE_RESPONSE = new CLI_RESPONSE(monitor);
                        _Set_CLI_RESPONSE_RESPONSE.Command = "SET";
                        _Set_CLI_RESPONSE_RESPONSE.TargetFeature = "OSDACCESS";
                        _Set_CLI_RESPONSE_RESPONSE.Value = value;

                        switch (value.ToUpper())
                        {
                            case "OSDLOCK":
                                r = SetVCPCode(devMgr, monitor, "0xCA", "0x01").Result;
                                break;

                            case "OSDUNLOCK":
                                r = SetVCPCode(devMgr, monitor, "0xCA", "0x02").Result;
                                break;
                        }

                        if (!r)
                        {
                            _Set_CLI_RESPONSE_RESPONSE.Result = "FAIL";
                            if (exit == 0)
                                exit = (int)CLI_ExitCode.functional_error;
                            writelog($"OSDAccess Fail");
                        }
                        else
                        {
                            _Set_CLI_RESPONSE_RESPONSE.Result = "PASS";
                            writelog($"OSDAccess Pass");
                        }

                        System.Console.WriteLine(JsonConvert.SerializeObject(_Set_CLI_RESPONSE_RESPONSE, Formatting.Indented));
                        output += "\n" + JsonConvert.SerializeObject(_Set_CLI_RESPONSE_RESPONSE, Formatting.Indented);
                    }
                }
                if (index.Count != 0)
                {
                    foreach (string idx in index)
                    {
                        writelog($"OSDAccess entry");
                        CLI_RESPONSE _Set_CLI_RESPONSE_RESPONSE = new CLI_RESPONSE(_AllInfoMonitors[Convert.ToInt32(idx)]);
                        _Set_CLI_RESPONSE_RESPONSE.Command = "SET";
                        _Set_CLI_RESPONSE_RESPONSE.TargetFeature = "OSDACCESS";
                        _Set_CLI_RESPONSE_RESPONSE.Value = value;

                        switch (value.ToUpper())
                        {
                            case "OSDLOCK":
                                r = SetVCPCode(devMgr, _AllInfoMonitors[System.Convert.ToInt32(idx)], "0xCA", "0x01").Result;
                                break;

                            case "OSDUNLOCK":
                                r = SetVCPCode(devMgr, _AllInfoMonitors[System.Convert.ToInt32(idx)], "0xCA", "0x02").Result;
                                break;
                        }

                        if (!r)
                        {
                            _Set_CLI_RESPONSE_RESPONSE.Result = "FAIL";
                            if (exit == 0)
                                exit = (int)CLI_ExitCode.functional_error;
                            writelog($"OSDAccess Fail");
                        }
                        else
                        {
                            _Set_CLI_RESPONSE_RESPONSE.Result = "PASS";
                            writelog($"OSDAccess Pass");
                        }

                        System.Console.WriteLine(JsonConvert.SerializeObject(_Set_CLI_RESPONSE_RESPONSE, Formatting.Indented));
                        output += "\n" + JsonConvert.SerializeObject(_Set_CLI_RESPONSE_RESPONSE, Formatting.Indented);
                    }
                }
                if (serviceTag.Count != 0)
                {
                    foreach (string tag in serviceTag)
                    {
                        var tmp = _AllInfoMonitors.FindAll(x => x.edid.ServiceTag.ToUpper().Equals(tag.ToUpper()));

                        foreach (MonitorInfo mo in tmp)
                        {
                            writelog($"OSDAccess entry");
                            CLI_RESPONSE _Set_CLI_RESPONSE_RESPONSE = new CLI_RESPONSE(mo);
                            _Set_CLI_RESPONSE_RESPONSE.Command = "SET";
                            _Set_CLI_RESPONSE_RESPONSE.TargetFeature = "OSDACCESS";
                            _Set_CLI_RESPONSE_RESPONSE.Value = value;

                            switch (value.ToUpper())
                            {
                                case "OSDLOCK":
                                    r = SetVCPCode(devMgr, mo, "0xCA", "0x01").Result;
                                    break;

                                case "OSDUNLOCK":
                                    r = SetVCPCode(devMgr, mo, "0xCA", "0x02").Result;
                                    break;
                            }

                            if (!r)
                            {
                                _Set_CLI_RESPONSE_RESPONSE.Result = "FAIL";
                                if (exit == 0)
                                    exit = (int)CLI_ExitCode.functional_error;
                                writelog($"OSDAccess Fail");
                            }
                            else
                            {
                                _Set_CLI_RESPONSE_RESPONSE.Result = "PASS";
                                writelog($"OSDAccess Pass");
                            }

                            System.Console.WriteLine(JsonConvert.SerializeObject(_Set_CLI_RESPONSE_RESPONSE, Formatting.Indented));
                            output += "\n" + JsonConvert.SerializeObject(_Set_CLI_RESPONSE_RESPONSE, Formatting.Indented);
                        }
                    }
                }

                if (model.Count != 0)
                {
                    foreach (string modelName in model)
                    {
                        var tmp = _AllInfoMonitors.FindAll(x => x.edid.ModelName.ToUpper().Equals(modelName.ToUpper()));

                        foreach (MonitorInfo mo in tmp)
                        {
                            writelog($"OSDAccess entry");
                            CLI_RESPONSE _Set_CLI_RESPONSE_RESPONSE = new CLI_RESPONSE(mo);
                            _Set_CLI_RESPONSE_RESPONSE.Command = "SET";
                            _Set_CLI_RESPONSE_RESPONSE.TargetFeature = "OSDACCESS";
                            _Set_CLI_RESPONSE_RESPONSE.Value = value;

                            switch (value.ToUpper())
                            {
                                case "OSDLOCK":
                                    r = SetVCPCode(devMgr, mo, "0xCA", "0x01").Result;
                                    break;

                                case "OSDUNLOCK":
                                    r = SetVCPCode(devMgr, mo, "0xCA", "0x02").Result;
                                    break;
                            }

                            if (!r)
                            {
                                _Set_CLI_RESPONSE_RESPONSE.Result = "FAIL";
                                if (exit == 0)
                                    exit = (int)CLI_ExitCode.functional_error;
                                writelog($"OSDAccess Fail");
                            }
                            else
                            {
                                _Set_CLI_RESPONSE_RESPONSE.Result = "PASS";
                                writelog($"OSDAccess Pass");
                            }

                            System.Console.WriteLine(JsonConvert.SerializeObject(_Set_CLI_RESPONSE_RESPONSE, Formatting.Indented));
                            output += "\n" + JsonConvert.SerializeObject(_Set_CLI_RESPONSE_RESPONSE, Formatting.Indented);
                        }
                    }
                }
                return (exit, output);
            }
            else if (type == "GET") //Dean 0726 should check if type matched as well
            {
                // jim add 20240608

                ObjGetVCP rc = new ObjGetVCP();
                int exit = 0;

                if (index.Count == 0 && serviceTag.Count == 0)
                {
                    foreach (var monitor in _AllInfoMonitors)
                    {
                        writelog($"OSDAccess entry");
                        CLI_RESPONSE _Get_CLI_RESPONSE_RESPONSE = new CLI_RESPONSE(monitor);
                        _Get_CLI_RESPONSE_RESPONSE.Command = "GET";
                        _Get_CLI_RESPONSE_RESPONSE.TargetFeature = "OSDACCESS";

                        rc = GetVCPCode(devMgr, monitor, "0xCA").Result;
                        _Get_CLI_RESPONSE_RESPONSE.Value = get_osd(rc.value.ToString());
                        _Get_CLI_RESPONSE_RESPONSE.Result = "PASS";
                        writelog($"OSDAccess Pass");

                        System.Console.WriteLine(JsonConvert.SerializeObject(_Get_CLI_RESPONSE_RESPONSE, Formatting.Indented));
                        output += "\n" + JsonConvert.SerializeObject(_Get_CLI_RESPONSE_RESPONSE, Formatting.Indented);
                    }
                }
                // 20240614 jim modify
                if (index.Count != 0)
                {
                    foreach (string idx in index)
                    {
                        writelog($"OSDAccess entry");
                        CLI_RESPONSE _Get_CLI_RESPONSE_RESPONSE = new CLI_RESPONSE(_AllInfoMonitors[Convert.ToInt32(idx)]);
                        _Get_CLI_RESPONSE_RESPONSE.Command = "GET";
                        _Get_CLI_RESPONSE_RESPONSE.TargetFeature = "OSDACCESS";

                        rc = GetVCPCode(devMgr, _AllInfoMonitors[Convert.ToInt32(idx)], "0xCA").Result;
                        _Get_CLI_RESPONSE_RESPONSE.Value = get_osd(rc.value.ToString());
                        _Get_CLI_RESPONSE_RESPONSE.Result = "PASS";
                        writelog($"OSDAccess Pass");

                        System.Console.WriteLine(JsonConvert.SerializeObject(_Get_CLI_RESPONSE_RESPONSE, Formatting.Indented));
                        output += "\n" + JsonConvert.SerializeObject(_Get_CLI_RESPONSE_RESPONSE, Formatting.Indented);
                    }
                }
                // 20240614 jim modify
                if (serviceTag.Count != 0)
                {
                    foreach (string tag in serviceTag)
                    {
                        var tmp = _AllInfoMonitors.FindAll(x => x.edid.ServiceTag.ToUpper().Equals(tag.ToUpper()));
                        foreach (MonitorInfo mo in tmp)
                        {
                            writelog($"OSDAccess entry");
                            CLI_RESPONSE _Get_CLI_RESPONSE_RESPONSE = new CLI_RESPONSE(mo);
                            _Get_CLI_RESPONSE_RESPONSE.Command = "GET";
                            _Get_CLI_RESPONSE_RESPONSE.TargetFeature = "OSDACCESS";

                            rc = GetVCPCode(devMgr, mo, "0xCA").Result;
                            _Get_CLI_RESPONSE_RESPONSE.Value = get_osd(rc.value.ToString());
                            _Get_CLI_RESPONSE_RESPONSE.Result = "PASS";
                            writelog($"OSDAccess Pass");

                            System.Console.WriteLine(JsonConvert.SerializeObject(_Get_CLI_RESPONSE_RESPONSE, Formatting.Indented));
                            output += "\n" + JsonConvert.SerializeObject(_Get_CLI_RESPONSE_RESPONSE, Formatting.Indented);
                        }
                    }
                }
                return (exit, output);
            }
            else
            {
                CLI_RESPONSE ret = new CLI_RESPONSE()
                {
                    Command = type,
                    Result = "Un-supported command",
                    Message = "Un-supported command"
                };
                output = JsonConvert.SerializeObject(ret, Formatting.Indented);
                return ((int)CLI_ExitCode.unknow_command, output);
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
                if (disposing)
                {
                    _agent = null;
                }

                IsDisposed = true;
            }
            base.Dispose(disposing);
        }

        #endregion IDisposableObservable Support

        #region Bruce display properties

        private int GetSupportedDisplayProperties(DisplayPropertiesInfo displayPropertiesInfo, CLI_Get_Properties_SupportedResolutionRefreshRate_RESPONSE supportedResolutionRefreshRate_RESPONSE)
        {
            string[] Orientations_Str = new string[] { "Landscape", "Portrait", "Landscape(flipped)", "Portrait(flipped)" };
            for (int i = 0; i < displayPropertiesInfo.SupportedProperties.Properties.Count; i++)
            {
                int w = displayPropertiesInfo.SupportedProperties.Properties[i].Resolutions_Width,
                    h = displayPropertiesInfo.SupportedProperties.Properties[i].Resolutions_High,
                    f = displayPropertiesInfo.SupportedProperties.Properties[i].Frequency;
                supportedResolutionRefreshRate_RESPONSE.AllResolutionRefreshRate.Add($"{i + 1} {w}x{h}, {f}Hz{(displayPropertiesInfo.SupportedProperties.Properties[i].isCurrent ? "(Current)" : "")}{(displayPropertiesInfo.SupportedProperties.Properties[i].isRecommended ? "(Recommended)" : "")}");
            }

            return (int)CLI_ExitCode.success;
        }

        private int GetCurrentDisplayProperties(DisplayPropertiesInfo displayPropertiesInfo, object o)
        {
            CLI_Get_Properties_HDR_RESPONSE cli_HDR_RESPONSE = o as CLI_Get_Properties_HDR_RESPONSE;
            CLI_Get_Properties_USBCPrioritization_RESPONSE cli_USBCPrioritization_RESPONSE = o as CLI_Get_Properties_USBCPrioritization_RESPONSE;
            CLI_Get_Properties_Orientation_RESPONSE cli_Orientation_RESPONSE = o as CLI_Get_Properties_Orientation_RESPONSE;
            CLI_Get_Properties_CurrentResolutionRefreshRate_RESPONSE cli_CurrentResolutionRefreshRate_RESPONSE = o as CLI_Get_Properties_CurrentResolutionRefreshRate_RESPONSE;
            DDPMSettings ddpmSettings = _devMgr.ReloadAppConfigData().Result;

            string[] Orientations_Str = new string[] { "Landscape", "Portrait", "Landscape(flipped)", "Portrait(flipped)" };
            if (cli_HDR_RESPONSE != null)
            {
                cli_HDR_RESPONSE.HDR = displayPropertiesInfo.isHDREnable ? "ON" : "OFF";
            }
            else if (cli_USBCPrioritization_RESPONSE != null)
            {
                cli_USBCPrioritization_RESPONSE.Value = displayPropertiesInfo.USBCPrioritizationType == USBCPrioritizationType.HighDataSpeed ? "High Data Speed" : "High Resolution";
                //cli_USBCPrioritization_RESPONSE.Value += "," + (ddpmSettings.LockSettings.Lock_Display_USBCPrioritization ? "LOCK" : "UNLOCK");
            }
            else if (cli_Orientation_RESPONSE != null)
            {
                cli_Orientation_RESPONSE.Value = Orientations_Str[(int)displayPropertiesInfo.CurrentOrientation];
            }
            else if (cli_CurrentResolutionRefreshRate_RESPONSE != null)
            {
                foreach (Properties properties in displayPropertiesInfo.SupportedProperties.Properties)
                {
                    if (properties.isCurrent)
                    {
                        cli_CurrentResolutionRefreshRate_RESPONSE.Value = $"{properties.Resolutions_Width}x{properties.Resolutions_High}, {properties.Frequency}Hz";
                        cli_CurrentResolutionRefreshRate_RESPONSE.BitsPerPixel = properties.BitsPerPixel.ToString();
                        break;
                    }
                }
            }
            return (int)CLI_ExitCode.success;
        }

        private (int code, string result) SetDisplayProperties(IDeviceManagerSA devMgr, CommandLineInput commandLineInput)
        {
            writelog($"SetDisplayProperties {commandLineInput.TargetFeature} entry");
            //if (devMgr == null)
            //{
            //    writelog("ProcessListMonitorsOptionAsync: input null IDeviceManagerSA");
            //    return (int)CLI_ExitCode.null_device_manager;
            //}
            int monitorCount = 0;
            if (_AllInfoMonitors == null)
                _AllInfoMonitors = devMgr.GetMonitors().Result;

            monitorCount = _AllInfoMonitors.Count;

            string output = string.Empty;
            int ret = 0;

            if (commandLineInput.DeviceIndex.Count <= 0 && commandLineInput.ServiceTag.Count <= 0 && commandLineInput.Model.Count <= 0)
            {
                bool flag = true;
                var serviceTagList = _AllInfoMonitors.Select(_ => _.edid.ServiceTag).Distinct().ToList();
                Trace.WriteLine(serviceTagList.Count);
                List<string> swapIsDone = new List<string>();
                int count = 0;
                while (flag && count < 1000)
                {
                    for (int i = 0; i < serviceTagList.Count; i++)
                    {
                        Trace.WriteLine(serviceTagList[i]);
                        string stIsDone = swapIsDone.FirstOrDefault(_ => _ == serviceTagList[i]);
                        if (!String.IsNullOrWhiteSpace(stIsDone))
                            continue;
                        MonitorInfo mo = _AllInfoMonitors.FirstOrDefault(_ => _.edid.ServiceTag == serviceTagList[i]);
                        if (mo == null)
                        {
                            Task.Delay(5000).Wait();
                            _AllInfoMonitors = _devMgr.GetMonitors().Result;
                            break;
                        }
                        CLI_RESPONSE cLI_RESPONSE = new CLI_RESPONSE(mo);
                        cLI_RESPONSE.Command = commandLineInput.Command;
                        cLI_RESPONSE.TargetFeature = commandLineInput.TargetFeature;
                        //cLI_RESPONSE.Index = change_0base_to_1base(i++.ToString()) + ",";
                        //cLI_RESPONSE.ServiceTag = mo.edid.ServiceTag.ToString() + ",";
                        //cLI_RESPONSE.SerialNumber = mo.edid.SerialNumber;
                        //cLI_RESPONSE.Model = mo.edid.ModelName;
                        var tmpRet = PropertiesFunc(devMgr, mo, commandLineInput, cLI_RESPONSE);
                        if (tmpRet.code == 3)
                        {
                            _AllInfoMonitors = devMgr.GetMonitors().Result;
                            break;
                        }
                        swapIsDone.Add(mo.edid.ServiceTag);
                        ret += tmpRet.code;
                        output += "\n" + tmpRet.result;
                    }
                    if (serviceTagList.Count == swapIsDone.Count)
                        flag = false;
                    count++;
                }
                //while (flag)
                //{
                //    int i = 0;
                //    output = string.Empty;
                //    ret = 0;
                //    foreach (MonitorInfo mo in _AllInfoMonitors.ToList())
                //    {
                //        cLI_RESPONSE = new CLI_RESPONSE();
                //        cLI_RESPONSE.Command = commandLineInput.Command;
                //        cLI_RESPONSE.TargetFeature = commandLineInput.TargetFeature;
                //        cLI_RESPONSE.Index = change_0base_to_1base(i++.ToString()) + ",";
                //        cLI_RESPONSE.ServiceTag = mo.edid.ServiceTag.ToString() + ",";
                //        cLI_RESPONSE.SerialNumber = mo.edid.SerialNumber;
                //        cLI_RESPONSE.Model = mo.edid.ModelName;

                //        var tmpRet = PropertiesFunc(devMgr, mo, commandLineInput, cLI_RESPONSE);
                //        if (tmpRet.code == 3)
                //        {
                //            _AllInfoMonitors = devMgr.GetMonitors().Result;
                //            break;
                //        }
                //        ret += tmpRet.code;
                //        output += "\n" + tmpRet.result;
                //    }
                //    if (i == monitorCount)
                //    {
                //        flag = false;
                //        break;
                //    }
                //}
            }
            else
            {
                foreach (string idx in commandLineInput.DeviceIndex)
                {
                    writelog($"SetDisplayProperties idx {commandLineInput.TargetFeature} entry");
                    MonitorInfo monitorInfo = _AllInfoMonitors[int.Parse(idx)];
                    CLI_RESPONSE cLI_RESPONSE = new CLI_RESPONSE(monitorInfo);
                    cLI_RESPONSE.Command = commandLineInput.Command;
                    cLI_RESPONSE.TargetFeature = commandLineInput.TargetFeature;
                    //cLI_RESPONSE.Index = change_0base_to_1base(idx) + ",";
                    //cLI_RESPONSE.ServiceTag = monitorInfo.edid.ServiceTag.ToString() + ",";
                    //cLI_RESPONSE.SerialNumber = monitorInfo.edid.SerialNumber;
                    //cLI_RESPONSE.Model = monitorInfo.edid.ModelName;

                    var tmpRet = PropertiesFunc(devMgr, monitorInfo, commandLineInput, cLI_RESPONSE);
                    ret += tmpRet.code;
                    output += "\n" + tmpRet.result;
                }
                int i = 0;
                foreach (string tag in commandLineInput.ServiceTag)
                {
                    writelog($"SetDisplayProperties tag {commandLineInput.TargetFeature} entry");
                    var tmp = _AllInfoMonitors.FindAll(x => x.edid.ServiceTag.ToUpper().Equals(tag.ToUpper()));
                    foreach (MonitorInfo mo in tmp)
                    {
                        CLI_RESPONSE cLI_RESPONSE = new CLI_RESPONSE(mo);
                        cLI_RESPONSE.Command = commandLineInput.Command;
                        cLI_RESPONSE.TargetFeature = commandLineInput.TargetFeature;
                        //cLI_RESPONSE.Index = change_0base_to_1base(i++.ToString()) + ",";
                        //cLI_RESPONSE.ServiceTag = mo.edid.ServiceTag.ToString() + ",";
                        //cLI_RESPONSE.SerialNumber = mo.edid.SerialNumber;
                        //cLI_RESPONSE.Model = mo.edid.ModelName;

                        var tmpRet = PropertiesFunc(devMgr, mo, commandLineInput, cLI_RESPONSE);
                        ret += tmpRet.code;
                        output += "\n" + tmpRet.result;
                    }
                }
                int j = 0;
                foreach (string modelName in commandLineInput.Model)
                {
                    writelog($"SetDisplayProperties model {commandLineInput.TargetFeature} entry");
                    var tmp = _AllInfoMonitors.FindAll(x => x.edid.ModelName.ToUpper().Equals(modelName.ToUpper()));
                    foreach (MonitorInfo mo in tmp)
                    {
                        CLI_RESPONSE cLI_RESPONSE = new CLI_RESPONSE(mo);
                        cLI_RESPONSE.Command = commandLineInput.Command;
                        cLI_RESPONSE.TargetFeature = commandLineInput.TargetFeature;
                        //cLI_RESPONSE.Index = change_0base_to_1base(j++.ToString()) + ",";
                        //cLI_RESPONSE.ServiceTag = mo.edid.ServiceTag.ToString() + ",";
                        //cLI_RESPONSE.SerialNumber = mo.edid.SerialNumber;
                        //cLI_RESPONSE.Model = mo.edid.ModelName;

                        var tmpRet = PropertiesFunc(devMgr, mo, commandLineInput, cLI_RESPONSE);
                        ret += tmpRet.code;
                        output += "\n" + tmpRet.result;
                    }
                }
            }
            if (ret > 0)
            {
                writelog(output);
                return ((int)CLI_ExitCode.fail_configHDR_settingfail, output);
            }
            writelog(output);
            return ((int)CLI_ExitCode.success, output);
        }

        private (int code, string result) PropertiesFunc(IDeviceManagerSA devMgr, MonitorInfo monitorInfo, CommandLineInput commandLineInput, CLI_RESPONSE cLI_RESPONSE)
        {
            writelog($"PropertiesFunc {commandLineInput.TargetFeature} entry");
            bool? ret = false;
            bool? ret_osd = false;
            DisplayPropertiesInfo displayPropertiesInfo = _devMgr.GetDisplayPropertiesInfo(monitorInfo).Result;
            Properties displayProperties;
            CLI_Get_Properties_HDR_RESPONSE HDR_RESPONSE = null;
            CLI_Get_Properties_USBCPrioritization_RESPONSE USBCPrioritization_RESPONSE = null;
            CLI_Get_Properties_Orientation_RESPONSE CurrentOrientation_RESPONSE = null;
            CLI_Get_Properties_CurrentResolutionRefreshRate_RESPONSE CurrentResolutionRefreshRate_RESPONSE = null;
            CLI_Get_Properties_SupportedResolutionRefreshRate_RESPONSE SupportedResolutionRefreshRate_RESPONSE = null;
            string output = string.Empty;
            string output_osd = string.Empty;

            switch (commandLineInput.TargetFeature.ToUpper())
            {
                case "OPTIMALRESOLUTION":

                    if (commandLineInput.Options.Count > 0)
                    {
                        cLI_RESPONSE.Result = "FAIL";
                        cLI_RESPONSE.Message = "Bring in extra strings:";
                        for (int i = 0; i < commandLineInput.Options.Count; i++)
                        {
                            cLI_RESPONSE.Message += $"{commandLineInput.Options[i].Option_Name}={commandLineInput.Options[i].Option_Value}";
                        }
                        break;
                    }
                    displayProperties = displayPropertiesInfo.SupportedProperties.Properties.Find(x => x.isRecommended);
                    writelog($"PropertiesFunc {commandLineInput.TargetFeature} _devMgr.SetDisplayPropertiest entry");
                    ret = _devMgr.SetDisplayPropertiest(monitorInfo, displayProperties,
                            displayPropertiesInfo.CurrentOrientation).Result;
                    writelog($"PropertiesFunc {commandLineInput.TargetFeature} _devMgr.SetDisplayPropertiest exit");
                    cLI_RESPONSE.Result = ret == true ? "pass" : "FAIL";
                    break;

                case "HDR":
                    HDR_RESPONSE = new CLI_Get_Properties_HDR_RESPONSE(cLI_RESPONSE);
                    HDR_RESPONSE.IsSupportedHDR = displayPropertiesInfo.SupportedHDR ? "Yes" : "No";
                    if (displayPropertiesInfo.SupportedHDR)
                    {
                        if (commandLineInput.Command.Equals("GET"))
                        {
                            if (commandLineInput.Options.Count > 0)
                            {
                                HDR_RESPONSE.Result = "FAIL";
                                HDR_RESPONSE.Message = "Bring in extra strings:";
                                for (int i = 0; i < commandLineInput.Options.Count; i++)
                                {
                                    HDR_RESPONSE.Message += $"{commandLineInput.Options[i].Option_Name}={commandLineInput.Options[i].Option_Value}";
                                }
                                break;
                            }
                            ret = GetCurrentDisplayProperties(displayPropertiesInfo, HDR_RESPONSE) == 0 ? true : false;
                        }
                        else if (commandLineInput.Command.Equals("SET"))
                        {
                            if (commandLineInput.Options.Count > 1)
                            {
                                HDR_RESPONSE.Result = "FAIL";
                                HDR_RESPONSE.Message = "Bring in extra strings:";
                                for (int i = 0; i < commandLineInput.Options.Count; i++)
                                {
                                    HDR_RESPONSE.Message += $"{commandLineInput.Options[i].Option_Name}={commandLineInput.Options[i].Option_Value}";
                                }
                                break;
                            }
                            for (int i = 0; i < commandLineInput.Options.Count; i++)
                            {
                                if (commandLineInput.Options[i].Option_Name.ToUpper().Equals("VALUE")) //ex: /set -name=Display.Brightness -index=[0] -value=60
                                {
                                    HDR_RESPONSE.HDR = commandLineInput.Options[i].Option_Value;
                                    HDR_RESPONSE.Value = commandLineInput.Options[i].Option_Value;
                                    bool _OnOff = commandLineInput.Options[i].Option_Value.Equals("ON") ? true : false;
                                    writelog($"PropertiesFunc {commandLineInput.TargetFeature} _devMgr.SetHDRStatus entry");
                                    ret = _devMgr.SetHDRStatus(monitorInfo, _OnOff).Result;
                                    writelog($"PropertiesFunc {commandLineInput.TargetFeature} _devMgr.SetHDRStatus exit");
                                }
                            }
                        }
                    }
                    else
                    {
                        HDR_RESPONSE.Message = "HDR not supported";
                        HDR_RESPONSE.HDR = "N/A";
                        ret = null;
                    }
                    HDR_RESPONSE.Result = ret == true ? "PASS" : "FAIL";
                    break;

                case "USBCPRIORITIZATION":
                    USBCPrioritization_RESPONSE = new CLI_Get_Properties_USBCPrioritization_RESPONSE(cLI_RESPONSE);
                    DDPMSettings data = devMgr.ReloadAppConfigData().Result;
                    //USBCPrioritization_RESPONSE.SupportedUSBCPrioritization = displayPropertiesInfo.SupportedUSBCPrioritization ? "Yes" : "No";
                    int count = 0;
                    bool flag = true;
                    while (flag && count < 1000)
                    {
                        Task.Delay(3000).Wait();
                        _AllInfoMonitors = devMgr.GetMonitors().Result;
                        var serviceTag = _AllInfoMonitors.FirstOrDefault(_ => _.edid.ServiceTag == monitorInfo.edid.ServiceTag);
                        if (serviceTag != null)
                            flag = false;
                        count++;
                    }
                    if (displayPropertiesInfo.SupportedUSBCPrioritization)
                    {
                        if (commandLineInput.Command.Equals("GET"))
                        {
                            writelog("USBCPRIORITIZATION get entry");
                            if (commandLineInput.Options.Count > 0)
                            {
                                USBCPrioritization_RESPONSE.Result = "FAIL";
                                USBCPrioritization_RESPONSE.Message = "Bring in extra strings:";
                                for (int i = 0; i < commandLineInput.Options.Count; i++)
                                {
                                    USBCPrioritization_RESPONSE.Message += $"{commandLineInput.Options[i].Option_Name}={commandLineInput.Options[i].Option_Value}";
                                }
                                break;
                            }
                            ret = GetCurrentDisplayProperties(displayPropertiesInfo, USBCPrioritization_RESPONSE) == 0 ? true : false;
                        }
                        else if (commandLineInput.Command.Equals("SET"))
                        {
                            writelog("USBCPRIORITIZATION set entry");
                            if (commandLineInput.Options.Count > 1)
                            {
                                USBCPrioritization_RESPONSE.Result = "FAIL";
                                USBCPrioritization_RESPONSE.Message = "Bring in extra strings:";
                                for (int i = 0; i < commandLineInput.Options.Count; i++)
                                {
                                    USBCPrioritization_RESPONSE.Message += $"{commandLineInput.Options[i].Option_Name}={commandLineInput.Options[i].Option_Value}";
                                }
                                output = JsonConvert.SerializeObject(USBCPrioritization_RESPONSE, Formatting.Indented);
                                writelog(commandLineInput.TargetFeature + $" Return value{output}");
                                System.Console.WriteLine(output);
                                return ((int)CLI_ExitCode.fail_option_value, output);
                            }
                            else if (commandLineInput.Options.Count == 0)
                            {
                                USBCPrioritization_RESPONSE.Result = "FAIL";
                                USBCPrioritization_RESPONSE.Message = "Invalid command line syntax, missing -value=...";
                                output = JsonConvert.SerializeObject(USBCPrioritization_RESPONSE, Formatting.Indented);
                                writelog(commandLineInput.TargetFeature + $" Return value{output}");
                                System.Console.WriteLine(output);
                                return ((int)CLI_ExitCode.fail_option_value, output);
                            }

                            //commandLineInput.Options[0].Option_Value.Replace(".", ",");
                            string[] op_values = commandLineInput.Options[0].Option_Value.Replace(".", ",").Split(",");

                            foreach (string v in op_values)
                            {
                                switch (v.ToUpper())
                                {
                                    case "LOCK":
                                    case "UNLOCK":
                                        if (v.ToUpper().Equals("LOCK")) data.LockSettings.Lock_Display_USBCPrioritization = true;
                                        if (v.ToUpper().Equals("UNLOCK")) data.LockSettings.Lock_Display_USBCPrioritization = false;
                                        writelog($"PropertiesFunc {commandLineInput.TargetFeature} devMgr.SetAppConfigData entry");
                                        devMgr.SetAppConfigData(data);
                                        writelog($"PropertiesFunc {commandLineInput.TargetFeature} devMgr.SetAppConfigData exit");
                                        break;
                                }
                            }

                            if (!String.IsNullOrEmpty(commandLineInput.Options[0].Option_Name) && commandLineInput.Options[0].Option_Name.ToUpper().Equals("VALUE")) //ex: /set -name=Display.Brightness -index=[0] -value=60
                            {
                                //USBCPrioritization_RESPONSE.USBCPrioritizationType = commandLineInput.Options[i].Option_Value;

                                USBCPrioritization_RESPONSE.Value = op_values[0];
                                if (!String.IsNullOrEmpty(op_values[0]) && op_values[0].ToUpper().Equals("HIGHSPEED"))
                                {
                                    op_values[0] = "HighDataSpeed";
                                    USBCPrioritization_RESPONSE.Value = op_values[0];
                                }

                                //USBCPrioritization_RESPONSE.Value += "," + (data.LockSettings.Lock_Display_USBCPrioritization ? "LOCK" : "UNLOCK");

                                if (op_values[0].ToUpper() != "HIGHDATASPEED" && op_values[0].ToUpper() != "HIGHRESOLUTION")
                                {
                                    USBCPrioritization_RESPONSE.Result = "FAIL";
                                    USBCPrioritization_RESPONSE.Message = $"Option value [{commandLineInput.Options[0].Option_Value}] not support";
                                    output = JsonConvert.SerializeObject(USBCPrioritization_RESPONSE, Formatting.Indented);
                                    writelog(commandLineInput.TargetFeature + $" Return value{output}");
                                    System.Console.WriteLine(output);
                                    return ((int)CLI_ExitCode.fail_option_value, output);
                                }

                                USBCPrioritizationType usbcPrioritizationType = op_values[0].ToUpper().Equals(USBCPrioritizationType.HighDataSpeed.ToString().ToUpper()) ? USBCPrioritizationType.HighDataSpeed : USBCPrioritizationType.HighResolution;
                                if (displayPropertiesInfo.USBCPrioritizationType.ToString().ToUpper().Equals(op_values[0].ToUpper()))
                                {
                                    ret = true;
                                }
                                else
                                {
                                    writelog($"PropertiesFunc {commandLineInput.TargetFeature} _devMgr.SetUSBCPrioritizationType entry");
                                    ret = _devMgr.SetUSBCPrioritizationType(monitorInfo, usbcPrioritizationType).Result;
                                    writelog($"PropertiesFunc {commandLineInput.TargetFeature} _devMgr.SetUSBCPrioritizationType exit");
                                }
                            }
                            if (ret == false)
                                return ((int)CLI_ExitCode.fail_SetVCPCapability, null);
                        }
                    }
                    else
                    {
                        USBCPrioritization_RESPONSE.Message = "USB-C Prioritization not supported";
                        // USBCPrioritization_RESPONSE.USBCPrioritizationType = "N/A";
                        ret = null;
                    }
                    USBCPrioritization_RESPONSE.Result = ret == true ? "PASS" : "FAIL";
                    break;

                case "RESOLUTION":
                    try
                    {
                        writelog("RESOLUTION get entry");
                        if (commandLineInput.Options.Count > 1)
                        {
                            cLI_RESPONSE.Result = "FAIL";
                            cLI_RESPONSE.Message = "Bring in extra strings:";
                            for (int i = 0; i < commandLineInput.Options.Count; i++)
                            {
                                cLI_RESPONSE.Message += $"{commandLineInput.Options[i].Option_Name}={commandLineInput.Options[i].Option_Value}";
                            }
                            break;
                        }
                        for (int i = 0; i < commandLineInput.Options.Count; i++)
                        {
                            if (commandLineInput.Options[i].Option_Name.ToUpper().Equals("VALUE")) //ex: /set -name=Display.Brightness -index=[0] -value=60
                            {
                                cLI_RESPONSE.Value = commandLineInput.Options[i].Option_Value;
                                string[] ss = commandLineInput.Options[i].Option_Value.Split("X");
                                displayProperties = new Properties() { Resolutions_Width = int.Parse(ss[0]), Resolutions_High = int.Parse(ss[1]), Frequency = 0 };
                                writelog($"PropertiesFunc {commandLineInput.TargetFeature} _devMgr.SetDisplayPropertiest entry");
                                ret = _devMgr.SetDisplayPropertiest(monitorInfo,
                                    displayProperties,
                                    displayPropertiesInfo.CurrentOrientation).Result;
                                writelog($"PropertiesFunc {commandLineInput.TargetFeature} _devMgr.SetDisplayPropertiest exit");
                            }
                        }
                    }
                    catch
                    {
                        cLI_RESPONSE.Message = "Input fail";
                        ret = false;
                    }
                    cLI_RESPONSE.Result = ret == true ? "PASS" : "FAIL";
                    break;

                case "REFRESHRATE":
                    try
                    {
                        writelog("REFRESHRATE get entry");
                        if (commandLineInput.Options.Count > 1)
                        {
                            cLI_RESPONSE.Result = "FAIL";
                            cLI_RESPONSE.Message = "Bring in extra strings:";
                            for (int i = 0; i < commandLineInput.Options.Count; i++)
                            {
                                cLI_RESPONSE.Message += $"{commandLineInput.Options[i].Option_Name}={commandLineInput.Options[i].Option_Value}";
                            }
                            break;
                        }
                        for (int i = 0; i < commandLineInput.Options.Count; i++)
                        {
                            if (commandLineInput.Options[i].Option_Name.ToUpper().Equals("VALUE")) //ex: /set -name=Display.Brightness -index=[0] -value=60
                            {
                                cLI_RESPONSE.Value = commandLineInput.Options[i].Option_Value;
                                displayProperties = new Properties()
                                {
                                    Resolutions_Width = 0,
                                    Resolutions_High = 0,
                                    Frequency = int.Parse(commandLineInput.Options[i].Option_Value)
                                };
                                writelog($"PropertiesFunc {commandLineInput.TargetFeature} _devMgr.SetDisplayPropertiest entry");
                                ret = _devMgr.SetDisplayPropertiest(monitorInfo,
                                    displayProperties,
                                    displayPropertiesInfo.CurrentOrientation).Result;
                                writelog($"PropertiesFunc {commandLineInput.TargetFeature} _devMgr.SetDisplayPropertiest exit");
                            }
                        }
                    }
                    catch
                    {
                        cLI_RESPONSE.Message = "Input fail";
                    }
                    cLI_RESPONSE.Result = ret == true ? "PASS" : "FAIL";
                    break;

                case "ORIENTATION":
                    CurrentOrientation_RESPONSE = new CLI_Get_Properties_Orientation_RESPONSE(cLI_RESPONSE);
                    try
                    {
                        //if (monitorInfo.CapabilityDic.ContainsKey("AA") && monitorInfo.CapabilityDic["AA"] != null && monitorInfo.CapabilityDic["AA"].Contains("00"))
                        //{
                        if (commandLineInput.Command.Equals("GET"))
                        {
                            writelog("ORIENTATION get entry");
                            if (commandLineInput.Options.Count > 0)
                            {
                                CurrentOrientation_RESPONSE.Result = "FAIL";
                                CurrentOrientation_RESPONSE.Message = "Bring in extra strings:";
                                for (int i = 0; i < commandLineInput.Options.Count; i++)
                                {
                                    CurrentOrientation_RESPONSE.Message += $"{commandLineInput.Options[i].Option_Name}={commandLineInput.Options[i].Option_Value}";
                                }
                                break;
                            }
                            ret = GetCurrentDisplayProperties(displayPropertiesInfo, CurrentOrientation_RESPONSE) == 0 ? true : false;
                        }
                        else if (commandLineInput.Command.Equals("SET"))
                        {
                            writelog("ORIENTATION set entry");
                            if (commandLineInput.Options.Count > 1)
                            {
                                CurrentOrientation_RESPONSE.Result = "FAIL";
                                CurrentOrientation_RESPONSE.Message = "Bring in extra strings:";
                                for (int i = 0; i < commandLineInput.Options.Count; i++)
                                {
                                    CurrentOrientation_RESPONSE.Message += $"{commandLineInput.Options[i].Option_Name}={commandLineInput.Options[i].Option_Value}";
                                }
                                break;
                            }
                            displayProperties = new Properties()
                            {
                                Resolutions_Width = 0,
                                Resolutions_High = 0,
                                Frequency = 0
                            };
                            for (int i = 0; i < commandLineInput.Options.Count; i++)
                            {
                                if (commandLineInput.Options[i].Option_Name.ToUpper().Equals("VALUE")) //ex: /set -name=Display.Brightness -index=[0] -value=60
                                {
                                    CurrentOrientation_RESPONSE.Value = commandLineInput.Options[i].Option_Value;
                                    DisplayOrientation? displayOrientation = null;
                                    switch (commandLineInput.Options[i].Option_Value)
                                    {
                                        case "LANDSCAPE":
                                            displayOrientation = DisplayOrientation.Angle0;
                                            break;

                                        case "PORTRAIT":
                                            displayOrientation = DisplayOrientation.Angle90;
                                            break;

                                        case "LANDSCAPE_FLIPPED":
                                            displayOrientation = DisplayOrientation.Angle180;
                                            break;

                                        case "PORTRAIT_FLIPPED":
                                            displayOrientation = DisplayOrientation.Angle270;
                                            break;

                                        default:
                                            ret = false;
                                            break;
                                    }
                                    if (displayOrientation != null)
                                    {
                                        writelog($"PropertiesFunc {commandLineInput.TargetFeature} _devMgr.SetDisplayPropertiest entry");
                                        ret = _devMgr.SetDisplayPropertiest(monitorInfo, displayProperties, (DisplayOrientation)displayOrientation).Result;
                                        writelog($"PropertiesFunc {commandLineInput.TargetFeature} _devMgr.SetDisplayPropertiest exit");
                                    }
                                }
                            }
                        }
                        //}
                        //else
                        //{
                        //    writelog($"ORIENTATION VCP not support");
                        //    CurrentOrientation_RESPONSE.Message = "ORIENTATION VCP not support";
                        //    output += $"\n  \"Result: \": \"ORIENTATION VCP not support\"";
                        //}
                    }
                    catch
                    {
                        CurrentOrientation_RESPONSE.Message = "Input fail";
                        ret = false;
                    }
                    CurrentOrientation_RESPONSE.Result = ret == true ? "PASS" : "FAIL";
                    break;

                case "RESOLUTIONREFRESHRATE":
                    try
                    {
                        //CLI: Terry update 0723
                        if (commandLineInput.Command.ToUpper().Trim().Equals("SET"))
                        {
                            writelog("RESOLUTIONREFRESHRATE get entry");
                            if (commandLineInput.Options.Count > 1)//not allow more than one command code, print redundant commanmd
                            {
                                cLI_RESPONSE.Result = "FAIL";
                                cLI_RESPONSE.Message = "Bring in extra strings:";
                                for (int i = 0; i < commandLineInput.Options.Count; i++)
                                {
                                    cLI_RESPONSE.Message += $"{commandLineInput.Options[i].Option_Name}={commandLineInput.Options[i].Option_Value}";
                                }
                                break;
                            }
                            if (commandLineInput.Options.Count < 1)//need target set command
                            {
                                cLI_RESPONSE.Result = "FAIL";
                                cLI_RESPONSE.Message = "Invalid command line syntax ";

                                break;
                            }
                            if (commandLineInput.Options.Count == 1)
                            {
                                if (!string.IsNullOrEmpty(commandLineInput.Options[0].Option_Value))
                                {
                                    if (commandLineInput.Options[0].Option_Name.ToUpper().Equals("VALUE")) //ex: /set -name=Display.Brightness -index=[0] -value=60
                                    {


                                        cLI_RESPONSE.Value = commandLineInput.Options[0].Option_Value;
                                        string value = commandLineInput.Options[0].Option_Value;
                                        string frequency = null;
                                        //Debug.WriteLine($"{commandLineInput.Options[0].Option_Value}");
                                        string[] ss = commandLineInput.Options[0].Option_Value.Split("X");
                                        //Trace.WriteLine($"1: {ss[0]}  {ss[1]}");
                                        if (ss.Length > 1)
                                        {
                                            if (!string.IsNullOrEmpty(ss[1]))
                                            {
                                                string[] sss = ss[1].Split("@");
                                                //Trace.WriteLine($"2: {sss[0]} {sss[1]}");
                                                if (sss.Length > 1)
                                                {
                                                    if (!string.IsNullOrEmpty(sss[1]))
                                                    {
                                                        //sss[1].Replace(".", ",");
                                                        if (sss[1].Contains(","))
                                                        {
                                                            //Trace.WriteLine($"3:  {sss[1]}");
                                                            string[] ssss = sss[1].Split(",");
                                                            if (!string.IsNullOrEmpty(ssss[0]))
                                                            {
                                                                frequency = ssss[0];
                                                                //Trace.WriteLine($"4: {ssss[0]}");
                                                                //Trace.WriteLine($"4: {ssss[1]}");
                                                                //string lock_option = ssss[1];
                                                            }
                                                            else
                                                            {
                                                                cLI_RESPONSE.Result = "FAIL";
                                                                cLI_RESPONSE.Message = "Invalid command line syntax ";
                                                            }

                                                        }
                                                        else
                                                            frequency = sss[1];
                                                        //Debug.WriteLine($"frequency: {ssss[0]}, {ssss[1]}");
                                                        displayProperties = new Properties() { Resolutions_Width = int.Parse(ss[0]), Resolutions_High = int.Parse(sss[0]), Frequency = int.Parse(frequency) };
                                                        writelog($"PropertiesFunc {commandLineInput.TargetFeature} _devMgr.SetDisplayPropertiest entry");
                                                        ret = _devMgr.SetDisplayPropertiest(monitorInfo,
                                                            displayProperties,
                                                            displayPropertiesInfo.CurrentOrientation).Result;
                                                        writelog($"PropertiesFunc {commandLineInput.TargetFeature} _devMgr.SetDisplayPropertiest exit");

                                                    }
                                                }
                                                else
                                                {
                                                    cLI_RESPONSE.Result = "FAIL";
                                                    cLI_RESPONSE.Message = "Invalid command line syntax ";
                                                }

                                            }
                                            else
                                            {
                                                cLI_RESPONSE.Result = "FAIL";
                                                cLI_RESPONSE.Message = "Invalid command line syntax ";
                                            }
                                        }

                                    }
                                    else
                                    {
                                        cLI_RESPONSE.Result = "FAIL";
                                        cLI_RESPONSE.Message = "Invalid command line syntax ";
                                    }

                                }
                            }
                            else
                            {
                                cLI_RESPONSE.Result = "FAIL";
                                cLI_RESPONSE.Message = $"command {commandLineInput.Options[0].Option_Name} not support"; //not allow other option, only use value
                            }
                        }
                        else
                        {
                            //not allow other command, for example: /GET
                            cLI_RESPONSE.Result = "FAIL";
                            cLI_RESPONSE.Message = $"command {commandLineInput.Command} not support";
                        }
                    }
                    catch
                    {
                        cLI_RESPONSE.Message = "Input fail";
                        ret = false;
                    }
                    cLI_RESPONSE.Result = ret == true ? "PASS" : "FAIL";
                    break;

                case "CURRENTRESOLUTIONREFRESHRATE"://"CURRENTDISPLAYPROPERTIES":
                    writelog("CURRENTRESOLUTIONREFRESHRATE get entry");
                    if (commandLineInput.Options.Count > 0)
                    {
                        cLI_RESPONSE.Result = "FAIL";
                        cLI_RESPONSE.Message = "Bring in extra strings:";
                        for (int i = 0; i < commandLineInput.Options.Count; i++)
                        {
                            cLI_RESPONSE.Message += $"{commandLineInput.Options[i].Option_Name}={commandLineInput.Options[i].Option_Value}";
                        }
                        break;
                    }
                    CurrentResolutionRefreshRate_RESPONSE = new CLI_Get_Properties_CurrentResolutionRefreshRate_RESPONSE(cLI_RESPONSE);
                    ret = GetCurrentDisplayProperties(displayPropertiesInfo, CurrentResolutionRefreshRate_RESPONSE) == 0 ? true : false;
                    CurrentResolutionRefreshRate_RESPONSE.Result = ret == true ? "PASS" : "FAIL";
                    break;

                case "ALLRESOLUTIONREFRESHRATE":
                    writelog("ALLRESOLUTIONREFRESHRATE get entry");
                    if (commandLineInput.Options.Count > 0)
                    {
                        cLI_RESPONSE.Result = "FAIL";
                        cLI_RESPONSE.Message = "Bring in extra strings:";
                        for (int i = 0; i < commandLineInput.Options.Count; i++)
                        {
                            cLI_RESPONSE.Message += $"{commandLineInput.Options[i].Option_Name}={commandLineInput.Options[i].Option_Value}";
                        }
                        break;
                    }
                    SupportedResolutionRefreshRate_RESPONSE = new CLI_Get_Properties_SupportedResolutionRefreshRate_RESPONSE(cLI_RESPONSE);
                    ret = GetSupportedDisplayProperties(displayPropertiesInfo, SupportedResolutionRefreshRate_RESPONSE) == 0 ? true : false;
                    SupportedResolutionRefreshRate_RESPONSE.Result = ret == true ? "PASS" : "FAIL";
                    break;

                case "LOCKROTATE":
                    if (commandLineInput.Options.Count > 1)
                    {
                        cLI_RESPONSE.Result = "FAIL";
                        cLI_RESPONSE.Message = "Bring in extra strings:";
                        for (int i = 0; i < commandLineInput.Options.Count; i++)
                        {
                            cLI_RESPONSE.Message += $"{commandLineInput.Options[i].Option_Name}={commandLineInput.Options[i].Option_Value}";
                        }
                        break;
                    }

                    if (commandLineInput.Command.Equals("SET"))
                    {
                        writelog("LOCKROTATE set entry");
                        for (int i = 0; i < commandLineInput.Options.Count; i++)
                        {
                            if (commandLineInput.Options[i].Option_Name.ToUpper().Equals("VALUE")) //ex: /set -name=Display.Brightness -index=[0] -value=60
                            {
                                cLI_RESPONSE.Value = commandLineInput.Options[i].Option_Value;
                                writelog($"PropertiesFunc {commandLineInput.TargetFeature} devMgr.LockRotate entry");
                                ret = devMgr.LockRotate(commandLineInput.Options[i].Option_Value == "ON" ? true : false).Result;
                                writelog($"PropertiesFunc {commandLineInput.TargetFeature} devMgr.LockRotate exit");
                            }
                        }
                        cLI_RESPONSE.Result = ret == true ? "PASS" : "FAIL";
                    }
                    else if (commandLineInput.Command.Equals("GET"))
                    {
                        writelog("LOCKROTATE get entry");
                        for (int i = 0; i <= commandLineInput.Options.Count; i++)
                        {
                            writelog($"PropertiesFunc {commandLineInput.TargetFeature} devMgr.GetLockRotateStatus entry");
                            ret_osd = devMgr.GetLockRotateStatus().Result;
                            writelog($"PropertiesFunc {commandLineInput.TargetFeature} devMgr.GetLockRotateStatus exit");
                        }
                        cLI_RESPONSE.Value = ret_osd == true ? "ON" : "OFF";
                    }
                    break;

                case "ROTATEOSDMENU":
                    if (commandLineInput.Options.Count > 1)
                    {
                        cLI_RESPONSE.Result = "FAIL";
                        cLI_RESPONSE.Message = "Bring in extra strings:";
                        for (int i = 0; i < commandLineInput.Options.Count; i++)
                        {
                            cLI_RESPONSE.Message += $"{commandLineInput.Options[i].Option_Name}={commandLineInput.Options[i].Option_Value}";
                        }
                        break;
                    }

                    if (commandLineInput.Command.Equals("SET"))
                    {
                        writelog("ROTATEOSDMENU set entry");
                        for (int i = 0; i < commandLineInput.Options.Count; i++)
                        {
                            if (commandLineInput.Options[i].Option_Name.ToUpper().Equals("VALUE")) //ex: /set -name=Display.Brightness -index=[0] -value=60
                            {
                                cLI_RESPONSE.Value = commandLineInput.Options[i].Option_Value;
                                writelog($"PropertiesFunc {commandLineInput.TargetFeature} devMgr.SetOSDOrientation entry");
                                ret = devMgr.SetOSDOrientation(monitorInfo, commandLineInput.Options[i].Option_Value).Result;
                                writelog($"PropertiesFunc {commandLineInput.TargetFeature} devMgr.SetOSDOrientation exit");
                            }
                        }
                        cLI_RESPONSE.Result = ret == true ? "PASS" : "FAIL";
                    }
                    else if (commandLineInput.Command.Equals("GET"))
                    {
                        writelog("ROTATEOSDMENU get entry");
                        for (int i = 0; i <= commandLineInput.Options.Count; i++)
                        {
                            writelog($"PropertiesFunc {commandLineInput.TargetFeature} devMgr.GetOSDOrientation entry");
                            output_osd = devMgr.GetOSDOrientation(monitorInfo).Result;
                            writelog($"PropertiesFunc {commandLineInput.TargetFeature} devMgr.GetOSDOrientation exit");
                        }
                        cLI_RESPONSE.Value = output_osd;
                    }
                    break;

                default:
                    cLI_RESPONSE.Command = commandLineInput.Command;
                    cLI_RESPONSE.TargetFeature = commandLineInput.TargetFeature;
                    cLI_RESPONSE.Result = "Un-supported feature";
                    cLI_RESPONSE.Message = "Un-supported feature";
                    writelog(JsonConvert.SerializeObject(cLI_RESPONSE, Formatting.Indented));
                    return ((int)CLI_ExitCode.fail_configHDR_inputfail, JsonConvert.SerializeObject(cLI_RESPONSE, Formatting.Indented));
            }
            if (HDR_RESPONSE != null)
            {
                output = JsonConvert.SerializeObject(HDR_RESPONSE, Formatting.Indented);
                writelog(commandLineInput.TargetFeature + $" Return value{output}");
                System.Console.WriteLine(output);
            }
            else if (USBCPrioritization_RESPONSE != null)
            {
                output = JsonConvert.SerializeObject(USBCPrioritization_RESPONSE, Formatting.Indented);
                writelog(commandLineInput.TargetFeature + $" Return value{output}");
                System.Console.WriteLine(output);
            }
            else if (CurrentOrientation_RESPONSE != null)
            {
                output = JsonConvert.SerializeObject(CurrentOrientation_RESPONSE, Formatting.Indented);
                writelog(commandLineInput.TargetFeature + $" Return value{output}");
                System.Console.WriteLine(output);
            }
            else if (CurrentResolutionRefreshRate_RESPONSE != null)
            {
                output = JsonConvert.SerializeObject(CurrentResolutionRefreshRate_RESPONSE, Formatting.Indented);
                writelog(commandLineInput.TargetFeature + $" Return value{output}");
                System.Console.WriteLine(output);
            }
            else if (SupportedResolutionRefreshRate_RESPONSE != null)
            {
                output = JsonConvert.SerializeObject(SupportedResolutionRefreshRate_RESPONSE, Formatting.Indented);
                writelog(commandLineInput.TargetFeature + $" Return value{output}");
                System.Console.WriteLine(output);
            }
            else
            {
                output = JsonConvert.SerializeObject(cLI_RESPONSE, Formatting.Indented);
                writelog(commandLineInput.TargetFeature + $" Return value{output}");
                System.Console.WriteLine(output);
            }
            if (ret == true)
            {
                writelog(output);
                return ((int)CLI_ExitCode.success, output);
            }
            else if (ret == false)
            {
                writelog(output);
                return ((int)CLI_ExitCode.fail_configHDR_settingfail, output);
            }
            else
            {
                writelog(output);
                return ((int)CLI_ExitCode.fail_NotSupport, output);
            }
        }

        #endregion Bruce display properties

        private (int code, string result) GetALSPropertiesAsync(IDeviceManagerSA device, CommandLineInput target_type, string val)
        {
            writelog($"GetALSPropertiesAsync entry: {target_type.TargetFeature}");
            if (device == null)
            {
                return WriteALSResponse(_AllInfoMonitors, val.ToString(), target_type, CLI_ExitCode.null_device_manager, false, "Input null IDeviceManagerSA");
            }
            if (_AllInfoMonitors == null)
                _AllInfoMonitors = device.GetMonitors().Result;

            if (!int.TryParse(val, out _))
                return WriteALSResponse(_AllInfoMonitors, val.ToString(), target_type, CLI_ExitCode.unknow_command, false, "Unknow command");
            ALSConfig param;
            ALSFeatureQueryType type;
            switch (target_type.TargetFeature)
            {
                case "AUTOBRIGHTNESS":
                    type = ALSFeatureQueryType.AutoBrightness;
                    break;

                case "AUTOBRIGHTNESSRANGELEVEL": //CLI: Mark 0723
                    type = ALSFeatureQueryType.AutoBrightnessRangeLevel;
                    break;

                case "AUTOCOLORTEMP":
                    type = ALSFeatureQueryType.AutoColorTemperature;
                    break;

                case "PRIMARYMONITORSYNC":
                    type = ALSFeatureQueryType.PrimaryMonitorSync;
                    break;

                case "MULTIMONITORSYNC"://Dean 0611
                    type = ALSFeatureQueryType.MMS;
                    break;
                case "INAPPAUTOBRITEMP"://Dean 0611
                    type = ALSFeatureQueryType.InAppAutoBriTemp;
                    break;

                default:
                    return WriteALSResponse(_AllInfoMonitors, val.ToString(), target_type, CLI_ExitCode.unknow_command, false, "Unknow command");
            }
            try
            {
                param = device.GetALSFeatureValue(_AllInfoMonitors[int.Parse(val)], type, int.Parse(val)).Result;
                if (param.result == false)//get fail
                {
                    return WriteALSResponse(_AllInfoMonitors, val.ToString(), target_type, CLI_ExitCode.fail_GetAlsFeatureFail, false, "Fail Get Als Feature Fail");
                }
            }
            catch (Exception ex)
            {
                writelog("GetALSPropertiesAsync Exception : " + ex.Message);
                Console.WriteLine("Exception : " + ex.Message);
                return WriteALSResponse(_AllInfoMonitors, val.ToString(), target_type, CLI_ExitCode.fail_GetAlsFeatureFail, false, "Fail Get Als Feature Fail");
            }
            return WriteALSResponse(_AllInfoMonitors, val.ToString(), target_type, CLI_ExitCode.success, true, "Get Als Feature Success", param);
        }

        private (int code, string result) SetALSProperties(IDeviceManagerSA device, CommandLineInput target_type, string idx, string value)
        {
            writelog($"SetALSProperties entry: {target_type.TargetFeature}");
            if (device == null)
            {
                return WriteALSResponse(_AllInfoMonitors, idx, target_type, CLI_ExitCode.null_device_manager, false, "");
            }

            ALSFeatureQueryType type;
            switch (target_type.TargetFeature)
            {
                case "AUTOBRIGHTNESS":
                    type = ALSFeatureQueryType.AutoBrightness;
                    break;

                case "AUTOBRIGHTNESSRANGELEVEL": //CLI: Mark 0723
                    type = ALSFeatureQueryType.AutoBrightnessRangeLevel;
                    break;

                case "AUTOCOLORTEMP":
                    type = ALSFeatureQueryType.AutoColorTemperature;
                    break;

                case "PRIMARYMONITORSYNC":
                    type = ALSFeatureQueryType.PrimaryMonitorSync;
                    break;

                case "MULTIMONITORSYNC"://Dean 0611
                    type = ALSFeatureQueryType.MMS;
                    break;
                case "INAPPAUTOBRITEMP":
                    type = ALSFeatureQueryType.InAppAutoBriTemp;
                    break;
                default:
                    return WriteALSResponse(_AllInfoMonitors, idx, target_type, CLI_ExitCode.unknow_command, false, target_type.TargetFeature.ToString());
            }
            if (!(type == ALSFeatureQueryType.AutoBrightnessRangeLevel))
            {
                if (!string.Equals(value, "ON", StringComparison.OrdinalIgnoreCase) && !string.Equals(value, "OFF", StringComparison.OrdinalIgnoreCase))
                {
                    return WriteALSResponse(_AllInfoMonitors, idx, target_type, CLI_ExitCode.unknow_command, false, "The input value should be \"on\" or \"off\"");
                }
            }
            else
            {
                if (type == ALSFeatureQueryType.AutoBrightnessRangeLevel)
                {
                    switch (value.ToUpper())
                    {
                        case "LOW":
                            value = "0";
                            break;

                        case "MID":
                            value = "1";
                            break;

                        case "HIGH":
                            value = "2";
                            break;

                        default:
                            return WriteALSResponse(_AllInfoMonitors, idx, target_type, CLI_ExitCode.unknow_command, false, "The input AutoBrightnessRangeLevel value error");
                    }
                }
            }
            if (device == null)
            {
                return WriteALSResponse(_AllInfoMonitors, idx, target_type, CLI_ExitCode.null_device_manager, false, "");
            }
            if (_AllInfoMonitors == null)
                _AllInfoMonitors = device.GetMonitors().Result;

            ALSConfig param = new ALSConfig();
            bool set_result = false;
            try
            {
                switch (type)
                {
                    case ALSFeatureQueryType.MMS:
                        set_result = device.SetALSFeatureValue(_AllInfoMonitors[int.Parse(idx)], param, ALSFeatureQueryType.MMS, value).Result;
                        break;

                    case ALSFeatureQueryType.PrimaryMonitorSync:
                        set_result = device.SetALSFeatureValue(_AllInfoMonitors[int.Parse(idx)], param, ALSFeatureQueryType.PrimaryMonitorSync, value).Result;
                        break;

                    case ALSFeatureQueryType.AutoColorTemperature:
                        set_result = device.SetALSFeatureValue(_AllInfoMonitors[int.Parse(idx)], param, ALSFeatureQueryType.AutoColorTemperature, value).Result;
                        break;

                    case ALSFeatureQueryType.AutoBrightness:
                        set_result = device.SetALSFeatureValue(_AllInfoMonitors[int.Parse(idx)], param, ALSFeatureQueryType.AutoBrightness, value).Result;
                        break;

                    case ALSFeatureQueryType.AutoBrightnessRangeLevel:
                        set_result = device.SetALSFeatureValue(_AllInfoMonitors[int.Parse(idx)], param, ALSFeatureQueryType.AutoBrightnessRangeLevel, value).Result;
                        break;

                    case ALSFeatureQueryType.All:
                        set_result = device.SetALSFeatureValue(_AllInfoMonitors[int.Parse(idx)], param, ALSFeatureQueryType.All, value).Result;
                        break;
                    case ALSFeatureQueryType.InAppAutoBriTemp:
                        set_result = device.SetALSFeatureValue(_AllInfoMonitors[int.Parse(idx)], param, ALSFeatureQueryType.AutoColorTemperature, value).Result;
                        if (!set_result)
                            break;
                        set_result = device.SetALSFeatureValue(_AllInfoMonitors[int.Parse(idx)], param, ALSFeatureQueryType.AutoBrightness, value).Result;
                        break;

                    default:
                        return WriteALSResponse(_AllInfoMonitors, idx, target_type, CLI_ExitCode.unknow_command, false, "");
                }
            }
            catch (Exception ex)
            {
                writelog("SetALSProperties Exception : " + ex.Message);
            }

            if (set_result)
            {
                return WriteALSResponse(_AllInfoMonitors, idx, target_type, CLI_ExitCode.success, set_result, "");
            }
            else
            {
                return WriteALSResponse(_AllInfoMonitors, idx, target_type, CLI_ExitCode.fail_SetAlsFeatureFail, set_result, "");
            }
        }

        private (int code, string result) WriteALSResponse(List<MonitorInfo> monitorInfos, string idx, CommandLineInput input, CLI_ExitCode exidcode, bool result, string other_errorinfo, ALSConfig param = null)
        {
            CLI_RESPONSE ALS_RESPONSE = new CLI_RESPONSE(monitorInfos[int.Parse(idx)]);

            string msg = "";

            ALS_RESPONSE.Command = input.Command;
            ALS_RESPONSE.TargetFeature = input.TargetFeature;
            //ALS_RESPONSE.Index = change_0base_to_1base(idx);

            if (result)
            {
                //ALS_RESPONSE.Model = monitorInfos[int.Parse(idx)].modelName;
                //ALS_RESPONSE.SerialNumber = monitorInfos[int.Parse(idx)].edid.SerialNumber;
                //ALS_RESPONSE.ServiceTag = monitorInfos[int.Parse(idx)].edid.ServiceTag;
                ALS_RESPONSE.Result = "PASS";
                ALS_RESPONSE.Message = "SUCCESS";
            }
            else
            {
                ALS_RESPONSE.Result = "FAIL";
                switch (exidcode)
                {
                    case CLI_ExitCode.null_device_manager:
                        msg = "Null device manager";
                        break;

                    case CLI_ExitCode.no_monitor_connected:
                        msg = "No monitor connected";
                        break;

                    case CLI_ExitCode.fail_GetAlsFeatureFail:
                        msg = "Fail GetAlsFeatureFail";
                        break;

                    case CLI_ExitCode.fail_SetAlsFeatureFail:
                        msg = "Fail SetAlsFeatureFail";
                        break;

                    case CLI_ExitCode.no_matched_monitor_found:
                        msg = "No matched monitor";
                        break;

                    case CLI_ExitCode.unknow_command:
                        msg = "Unknow Command : " + other_errorinfo;
                        break;

                    default:
                        msg = other_errorinfo;
                        break;
                }
                ALS_RESPONSE.Message = msg;
            }

            if (input.Command == "SET")
                ALS_RESPONSE.Value = input.Options[0].Option_Value;
            else
            {
                if (param != null)
                {
                    switch (input.TargetFeature)
                    {
                        case "AUTOBRIGHTNESS":
                            ALS_RESPONSE.Value = param.isAutoBrightness ? "ON" : "OFF";
                            break;

                        case "AUTOBRIGHTNESSRANGELEVEL": //CLI: Mark0723
                            ALS_RESPONSE.Value = param.AutoBrightnessRangeLevel.level_name.ToUpper();
                            break;

                        case "AUTOCOLORTEMP":
                            ALS_RESPONSE.Value = param.isAutoColorTemp ? "ON" : "OFF";
                            break;

                        case "PRIMARYMONITORSYNC":
                            ALS_RESPONSE.Value = param.isPrimaryMonitorSync ? "ON" : "OFF";
                            break;

                        case "MULTIMONITORSYNC":
                            ALS_RESPONSE.Value = param.isMMSEnable ? "ON" : "OFF";
                            break;
                        case "INAPPAUTOBRITEMP":
                            ALS_RESPONSE.Value = param.isAutoBrightness && param.isAutoColorTemp ? "ON"
                                : !param.isAutoBrightness && !param.isAutoColorTemp ? "OFF"
                                : $"AutoBrightness: {(param.isAutoBrightness ? "ON" : "OFF")} AutoColorTemp: {(param.isAutoColorTemp ? "ON" : "OFF")}";
                            break;
                    }
                }
            }
            System.Console.WriteLine(ALS_RESPONSE.ToJson());
            writelog(input.TargetFeature + " Return value" + ALS_RESPONSE.ToJson());
            return ((int)exidcode, ALS_RESPONSE.ToJson());
        }

        private (int code, string result) PowerNapX(IDeviceManagerSA devMgr, CommandLineInput commandLineInput)
        {
            if (commandLineInput.Command.Equals("GET"))
            {
                writelog("PowerNap get entry");
                if (commandLineInput.Options.Count > 0)
                {
                    CLI_RESPONSE G_PopwerNap_RESPONSE = new CLI_RESPONSE();
                    G_PopwerNap_RESPONSE.Result = "FAIL";
                    G_PopwerNap_RESPONSE.Message = "UNKNOWN COMMAND";
                    System.Console.WriteLine(JsonConvert.SerializeObject(G_PopwerNap_RESPONSE, Formatting.Indented));
                    return ((int)CLI_ExitCode.unknow_command, JsonConvert.SerializeObject(G_PopwerNap_RESPONSE, Formatting.Indented));
                }
                else
                {
                    return PowerNap(devMgr, commandLineInput.Command, commandLineInput.DeviceIndex, commandLineInput.ServiceTag, commandLineInput.Model, "").Result;
                }
            }
            else if (commandLineInput.Command.Equals("SET"))
            {
                writelog("PowerNap set entry");
                if (commandLineInput.Options.Count == 1)
                {
                    string output = string.Empty;
                    int exitcode = 0;
                    for (int j = 0; j < commandLineInput.Options.Count; j++)
                    {
                        if (commandLineInput.Options[j].Option_Name.ToUpper().Equals("VALUE"))
                        {
                            var temp = PowerNap(devMgr, commandLineInput.Command, commandLineInput.DeviceIndex, commandLineInput.ServiceTag, commandLineInput.Model, commandLineInput.Options[j].Option_Value).Result;
                            output += "\n" + temp.result;
                            exitcode = temp.code;
                            if (exitcode != 0)
                                break;
                        }
                    }
                    return (exitcode, output);
                }
                else
                {
                    CLI_RESPONSE S_PopwerNap_RESPONSE = new CLI_RESPONSE();
                    S_PopwerNap_RESPONSE.Result = "FAIL";
                    S_PopwerNap_RESPONSE.Message = "UNKNOWN OPTION";
                    System.Console.WriteLine(JsonConvert.SerializeObject(S_PopwerNap_RESPONSE, Formatting.Indented));
                    return ((int)CLI_ExitCode.unknow_command, JsonConvert.SerializeObject(S_PopwerNap_RESPONSE, Formatting.Indented));
                }
            }
            else
            {
                CLI_RESPONSE PopwerNap_RESPONSE = new CLI_RESPONSE();
                PopwerNap_RESPONSE.Result = "FAIL";
                PopwerNap_RESPONSE.Message = "UNKNOWN COMMAND";
                System.Console.WriteLine(JsonConvert.SerializeObject(PopwerNap_RESPONSE, Formatting.Indented));
                return ((int)CLI_ExitCode.unknow_command, JsonConvert.SerializeObject(PopwerNap_RESPONSE, Formatting.Indented));
            }
        }

        private async Task<(int code, string result)> PowerNap(IDeviceManagerSA devMgr, string type, List<string> index, List<string> serviceTag, List<string> model, string value)
        {
            CLI_RESPONSE S_PowerNap_RESPONSE = new CLI_RESPONSE();
            CLI_RESPONSE G_PopwerNap_RESPONSE = new CLI_RESPONSE();

            if (_AllInfoMonitors == null)
                _AllInfoMonitors = await devMgr.GetMonitors();
            string output = string.Empty;
            List<PowerNapSetting> read_list = (devMgr.ReadPowerNapSettings().Result).ToList();
            DDPMSettings ddpmSettings = devMgr.ReloadAppConfigData().Result;

            if (ddpmSettings == null)
            {
                return DDPMSettingsNullResponse(type, "PowerNap");
            }

            if (type == "SET")
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    S_PowerNap_RESPONSE.Result = "Format Error";
                    S_PowerNap_RESPONSE.Message = "Format Error";
                    return ((int)CLI_ExitCode.fail_FormantError, JsonConvert.SerializeObject(S_PowerNap_RESPONSE, Formatting.Indented));
                }

                if (index.Count == 0 && serviceTag.Count == 0 && model.Count == 0)
                {
                    writelog("PowerNap set entry");
                    foreach (MonitorInfo monitor in _AllInfoMonitors)
                    {
                        S_PowerNap_RESPONSE = new CLI_RESPONSE(monitor);
                        S_PowerNap_RESPONSE.Command = "SET";
                        S_PowerNap_RESPONSE.TargetFeature = "PowerNap";

                        read_list.RemoveAll(x => x.ServiceTag == null);
                        int idx = read_list.FindIndex(x => x.ServiceTag.Equals(monitor.edid.ServiceTag));
                        if (read_list.Count == 0 || idx < 0)  //if no PowerNap setting exist, create a new setting
                        {
                            await SetPowerNapAsync(PowerNapType.Off, devMgr, monitor.edid.ModelName, monitor.edid.SerialNumber, monitor.edid.ServiceTag);
                            S_PowerNap_RESPONSE.Result = "PASS";
                            S_PowerNap_RESPONSE.Value = "Off";
                            read_list = (devMgr.ReadPowerNapSettings().Result).ToList();
                            read_list.RemoveAll(x => x.ServiceTag == null);
                            idx = read_list.FindIndex(x => x.ServiceTag.Equals(monitor.edid.ServiceTag));
                        }
                        if (read_list.Count > 0 || idx >= 0)
                        {

                            List<string> values = value.Replace(".", ",").Split(",").ToList();
                            PowerNapSetting temp = read_list[idx];
                            foreach (string v in values)
                            {
                                switch (v.ToUpper())
                                {
                                    case "LOCK":
                                    case "UNLOCK":
                                        if (v.ToUpper().Equals("LOCK")) ddpmSettings.LockSettings.Lock_Display_PowerNap = true;
                                        if (v.ToUpper().Equals("UNLOCK")) ddpmSettings.LockSettings.Lock_Display_PowerNap = false;
                                        await devMgr.SetAppConfigData(ddpmSettings);
                                        break;
                                    case "OFF":
                                        {
                                            await SetPowerNapAsync(PowerNapType.Off, devMgr, temp.ModelName, temp.SerialNumber, temp.ServiceTag);
                                            break;
                                        }
                                    case "SLEEP":
                                        {
                                            await SetPowerNapAsync(PowerNapType.SleepIfRunning, devMgr, temp.ModelName, temp.SerialNumber, temp.ServiceTag);
                                            break;
                                        }
                                    case "REDUCEBRIGHTNESS":
                                        {
                                            await SetPowerNapAsync(PowerNapType.ReduceBrightness, devMgr, temp.ModelName, temp.SerialNumber, temp.ServiceTag);
                                            break;
                                        }
                                    default:
                                        {
                                            S_PowerNap_RESPONSE.Result = "FAIL";
                                            S_PowerNap_RESPONSE.Message = "Un-supported Option";
                                            return ((int)CLI_ExitCode.unknow_command, JsonConvert.SerializeObject(S_PowerNap_RESPONSE, Formatting.Indented));
                                        }
                                }
                            }
                        }
                        List<PowerNapSetting> rst = devMgr.ReadPowerNapSettings().Result;
                        rst.RemoveAll(x => x.ServiceTag == null);
                        int idex = rst.FindIndex(x => x.ServiceTag.Equals(monitor.edid.ServiceTag));
                        PowerNapSetting tmp = rst[idex];
                        S_PowerNap_RESPONSE.Result = "PASS";
                        if (tmp.RunType.ToString().Equals("SleepIfRunning"))
                            S_PowerNap_RESPONSE.Value = "Sleep";
                        else
                            S_PowerNap_RESPONSE.Value = tmp.RunType.ToString();

                        //S_PowerNap_RESPONSE.Value += "," + (ddpmSettings.LockSettings.Lock_Display_PowerNap ? "LOCK" : "UNLOCK");
                        output += "\n" + JsonConvert.SerializeObject(S_PowerNap_RESPONSE, Formatting.Indented);
                    }
                }
                else if (index.Count != 0)
                {
                    writelog("PowerNap set idx entry");
                    foreach (string idex in index)
                    {
                        MonitorInfo monitor = _AllInfoMonitors[int.Parse(idex)];
                        S_PowerNap_RESPONSE = new CLI_RESPONSE(monitor);

                        S_PowerNap_RESPONSE.Command = "SET";
                        S_PowerNap_RESPONSE.TargetFeature = "PowerNap";

                        read_list.RemoveAll(x => x.ServiceTag == null);
                        int idx = read_list.FindIndex(x => x.ServiceTag.Equals(monitor.edid.ServiceTag));
                        if (read_list.Count == 0 || idx < 0)  //if no PowerNap setting exist, create a new setting
                        {
                            await SetPowerNapAsync(PowerNapType.Off, devMgr, monitor.edid.ModelName, monitor.edid.SerialNumber, monitor.edid.ServiceTag);
                            S_PowerNap_RESPONSE.Result = "PASS";
                            S_PowerNap_RESPONSE.Value = "Off";
                            read_list = (devMgr.ReadPowerNapSettings().Result).ToList();
                            read_list.RemoveAll(x => x.ServiceTag == null);
                            idx = read_list.FindIndex(x => x.ServiceTag.Equals(monitor.edid.ServiceTag));
                        }
                        if (read_list.Count > 0 || idx >= 0)
                        {
                            PowerNapSetting temp = read_list[idx];
                            List<string> values = value.Replace(".", ",").Split(",").ToList();
                            foreach (string v in values)
                            {
                                switch (v.ToUpper())
                                {
                                    case "LOCK":
                                    case "UNLOCK":
                                        if (v.ToUpper().Equals("LOCK")) ddpmSettings.LockSettings.Lock_Display_PowerNap = true;
                                        if (v.ToUpper().Equals("UNLOCK")) ddpmSettings.LockSettings.Lock_Display_PowerNap = false;
                                        await devMgr.SetAppConfigData(ddpmSettings);
                                        break;
                                    case "OFF":
                                        {
                                            await SetPowerNapAsync(PowerNapType.Off, devMgr, temp.ModelName, temp.SerialNumber, temp.ServiceTag);
                                            break;
                                        }
                                    case "SLEEP":
                                        {
                                            await SetPowerNapAsync(PowerNapType.SleepIfRunning, devMgr, temp.ModelName, temp.SerialNumber, temp.ServiceTag);
                                            break;
                                        }
                                    case "REDUCEBRIGHTNESS":
                                        {
                                            await SetPowerNapAsync(PowerNapType.ReduceBrightness, devMgr, temp.ModelName, temp.SerialNumber, temp.ServiceTag);
                                            break;
                                        }
                                    default:
                                        {
                                            S_PowerNap_RESPONSE.Result = "FAIL";
                                            S_PowerNap_RESPONSE.Message = "Unsupport Option";
                                            return ((int)CLI_ExitCode.unknow_command, JsonConvert.SerializeObject(S_PowerNap_RESPONSE, Formatting.Indented));
                                        }
                                }
                            }
                        }
                        List<PowerNapSetting> rst = devMgr.ReadPowerNapSettings().Result;
                        rst.RemoveAll(x => x.ServiceTag == null);
                        int rst_idex = rst.FindIndex(x => x.ServiceTag.Equals(monitor.edid.ServiceTag));
                        PowerNapSetting tmp = rst[rst_idex];
                        S_PowerNap_RESPONSE.Result = "PASS";
                        if (tmp.RunType.ToString().Equals("SleepIfRunning"))
                            S_PowerNap_RESPONSE.Value = "Sleep";
                        else
                            S_PowerNap_RESPONSE.Value = tmp.RunType.ToString();

                        //S_PowerNap_RESPONSE.Value += "," + (ddpmSettings.LockSettings.Lock_Display_PowerNap ? "LOCK" : "UNLOCK");
                        output += "\n" + JsonConvert.SerializeObject(S_PowerNap_RESPONSE, Formatting.Indented);
                    }
                }
                else if (serviceTag.Count != 0)
                {
                    foreach (string tag in serviceTag)
                    {
                        writelog("PowerNap set tag entry");
                        var tmp = _AllInfoMonitors.FindAll(x => x.edid.ServiceTag.ToUpper().Equals(tag.ToUpper()));
                        foreach (MonitorInfo monitor in tmp)
                        {
                            S_PowerNap_RESPONSE = new CLI_RESPONSE(monitor);

                            S_PowerNap_RESPONSE.Command = "SET";
                            S_PowerNap_RESPONSE.TargetFeature = "PowerNap";

                            read_list.RemoveAll(x => x.ServiceTag == null);
                            int idx = read_list.FindIndex(x => x.ServiceTag.Equals(monitor.edid.ServiceTag));
                            if (read_list.Count == 0 || idx < 0)  //if no PowerNap setting exist, create a new setting
                            {
                                await SetPowerNapAsync(PowerNapType.Off, devMgr, monitor.edid.ModelName, monitor.edid.SerialNumber, monitor.edid.ServiceTag);
                                S_PowerNap_RESPONSE.Result = "PASS";
                                S_PowerNap_RESPONSE.Value = "Off";
                                read_list = (devMgr.ReadPowerNapSettings().Result).ToList();
                                read_list.RemoveAll(x => x.ServiceTag == null);
                                idx = read_list.FindIndex(x => x.ServiceTag.Equals(monitor.edid.ServiceTag));
                            }
                            if (read_list.Count > 0 || idx >= 0)
                            {
                                PowerNapSetting temp = read_list[idx];

                                List<string> values = value.Replace(".", ",").Split(",").ToList();
                                foreach (string v in values)
                                {
                                    switch (v.ToUpper())
                                    {
                                        case "LOCK":
                                        case "UNLOCK":
                                            if (v.ToUpper().Equals("LOCK")) ddpmSettings.LockSettings.Lock_Display_PowerNap = true;
                                            if (v.ToUpper().Equals("UNLOCK")) ddpmSettings.LockSettings.Lock_Display_PowerNap = false;
                                            await devMgr.SetAppConfigData(ddpmSettings);
                                            break;
                                        case "OFF":
                                            {
                                                await SetPowerNapAsync(PowerNapType.Off, devMgr, temp.ModelName, temp.SerialNumber, temp.ServiceTag);
                                                break;
                                            }
                                        case "SLEEP":
                                            {
                                                await SetPowerNapAsync(PowerNapType.SleepIfRunning, devMgr, temp.ModelName, temp.SerialNumber, temp.ServiceTag);
                                                break;
                                            }
                                        case "REDUCEBRIGHTNESS":
                                            {
                                                await SetPowerNapAsync(PowerNapType.ReduceBrightness, devMgr, temp.ModelName, temp.SerialNumber, temp.ServiceTag);
                                                break;
                                            }
                                        default:
                                            {
                                                S_PowerNap_RESPONSE.Result = "FAIL";
                                                S_PowerNap_RESPONSE.Message = "Unsupport Option";
                                                return ((int)CLI_ExitCode.unknow_command, JsonConvert.SerializeObject(S_PowerNap_RESPONSE, Formatting.Indented));
                                            }
                                    }
                                }
                            }
                            List<PowerNapSetting> rst = devMgr.ReadPowerNapSettings().Result;
                            rst.RemoveAll(x => x.ServiceTag == null);
                            int idex = rst.FindIndex(x => x.ServiceTag.Equals(monitor.edid.ServiceTag));
                            PowerNapSetting tmp_rst = rst[idex];
                            S_PowerNap_RESPONSE.Result = "PASS";
                            if (tmp_rst.RunType.ToString().Equals("SleepIfRunning"))
                                S_PowerNap_RESPONSE.Value = "Sleep";
                            else
                                S_PowerNap_RESPONSE.Value = tmp_rst.RunType.ToString();

                            //S_PowerNap_RESPONSE.Value += "," + (ddpmSettings.LockSettings.Lock_Display_PowerNap ? "LOCK" : "UNLOCK");
                            output += "\n" + JsonConvert.SerializeObject(S_PowerNap_RESPONSE, Formatting.Indented);
                        }
                    }
                }
                else if (model.Count != 0)
                {
                    foreach (string modelName in model)
                    {
                        writelog("PowerNap set model entry");
                        var tmp = _AllInfoMonitors.FindAll(x => x.edid.ModelName.ToUpper().Equals(modelName.ToUpper()));
                        foreach (MonitorInfo monitor in tmp)
                        {
                            S_PowerNap_RESPONSE = new CLI_RESPONSE(monitor);

                            S_PowerNap_RESPONSE.Command = "SET";
                            S_PowerNap_RESPONSE.TargetFeature = "PowerNap";

                            read_list.RemoveAll(x => x.ServiceTag == null);
                            int idx = read_list.FindIndex(x => x.ServiceTag.Equals(monitor.edid.ServiceTag));
                            if (read_list.Count == 0 || idx < 0)  //if no PowerNap setting exist, create a new setting
                            {
                                await SetPowerNapAsync(PowerNapType.Off, devMgr, monitor.edid.ModelName, monitor.edid.SerialNumber, monitor.edid.ServiceTag);
                                S_PowerNap_RESPONSE.Result = "PASS";
                                S_PowerNap_RESPONSE.Value = "Off";
                                read_list = (devMgr.ReadPowerNapSettings().Result).ToList();
                                read_list.RemoveAll(x => x.ServiceTag == null);
                                idx = read_list.FindIndex(x => x.ServiceTag.Equals(monitor.edid.ServiceTag));
                            }
                            if (read_list.Count > 0 || idx >= 0)
                            {
                                PowerNapSetting temp = read_list[idx];

                                List<string> values = value.Replace(".", ",").Split(",").ToList();
                                foreach (string v in values)
                                {
                                    switch (v.ToUpper())
                                    {
                                        case "LOCK":
                                        case "UNLOCK":
                                            if (v.ToUpper().Equals("LOCK")) ddpmSettings.LockSettings.Lock_Display_PowerNap = true;
                                            if (v.ToUpper().Equals("UNLOCK")) ddpmSettings.LockSettings.Lock_Display_PowerNap = false;
                                            await devMgr.SetAppConfigData(ddpmSettings);
                                            break;
                                        case "OFF":
                                            {
                                                await SetPowerNapAsync(PowerNapType.Off, devMgr, temp.ModelName, temp.SerialNumber, temp.ServiceTag);
                                                break;
                                            }
                                        case "SLEEP":
                                            {
                                                await SetPowerNapAsync(PowerNapType.SleepIfRunning, devMgr, temp.ModelName, temp.SerialNumber, temp.ServiceTag);
                                                break;
                                            }
                                        case "REDUCEBRIGHTNESS":
                                            {
                                                await SetPowerNapAsync(PowerNapType.ReduceBrightness, devMgr, temp.ModelName, temp.SerialNumber, temp.ServiceTag);
                                                break;
                                            }
                                        default:
                                            {
                                                S_PowerNap_RESPONSE.Result = "FAIL";
                                                S_PowerNap_RESPONSE.Message = "Unsupport Option";
                                                return ((int)CLI_ExitCode.unknow_command, JsonConvert.SerializeObject(S_PowerNap_RESPONSE, Formatting.Indented));
                                            }
                                    }
                                }
                            }
                            List<PowerNapSetting> rst = devMgr.ReadPowerNapSettings().Result;
                            rst.RemoveAll(x => x.ServiceTag == null);
                            int idex = rst.FindIndex(x => x.ServiceTag.Equals(monitor.edid.ServiceTag));
                            PowerNapSetting tmp_rst = rst[idex];
                            S_PowerNap_RESPONSE.Result = "PASS";
                            if (tmp_rst.RunType.ToString().Equals("SleepIfRunning"))
                                S_PowerNap_RESPONSE.Value = "Sleep";
                            else
                                S_PowerNap_RESPONSE.Value = tmp_rst.RunType.ToString();

                            //S_PowerNap_RESPONSE.Value += "," + (ddpmSettings.LockSettings.Lock_Display_PowerNap ? "LOCK" : "UNLOCK");
                            output += "\n" + JsonConvert.SerializeObject(S_PowerNap_RESPONSE, Formatting.Indented);
                        }
                    }
                }

                writelog($"PowerNap return exit value{output}");
                return ((int)CLI_ExitCode.success, output);
            }
            if (type == "GET")
            {
                if (index.Count == 0 && serviceTag.Count == 0)
                {
                    writelog("PowerNap get entry");
                    foreach (MonitorInfo monitor in _AllInfoMonitors)
                    {
                        S_PowerNap_RESPONSE = new CLI_RESPONSE(monitor);

                        S_PowerNap_RESPONSE.Command = "GET";
                        S_PowerNap_RESPONSE.TargetFeature = "PowerNap";

                        read_list.RemoveAll(x => x.ServiceTag == null);
                        int idx = read_list.FindIndex(x => x.ServiceTag.Equals(monitor.edid.ServiceTag));
                        if (read_list.Count == 0 || idx < 0)  //if no PowerNap setting exist, create a new setting
                        {
                            await SetPowerNapAsync(PowerNapType.Off, devMgr, monitor.edid.ModelName, monitor.edid.SerialNumber, monitor.edid.ServiceTag);
                            S_PowerNap_RESPONSE.Result = "PASS";
                            S_PowerNap_RESPONSE.Value = "Off";
                        }
                        else
                        {
                            PowerNapSetting temp = read_list[idx];
                            S_PowerNap_RESPONSE.Result = "PASS";
                            if (temp.RunType.ToString().Equals("SleepIfRunning"))
                                S_PowerNap_RESPONSE.Value = "Sleep";
                            else
                                S_PowerNap_RESPONSE.Value = temp.RunType.ToString();
                        }
                        //S_PowerNap_RESPONSE.Value += "," + (ddpmSettings.LockSettings.Lock_Display_PowerNap ? "LOCK" : "UNLOCK");
                        output += "\n" + JsonConvert.SerializeObject(S_PowerNap_RESPONSE, Formatting.Indented);
                    }
                }
                else if (index.Count != 0)
                {
                    foreach (string idex in index)
                    {
                        writelog("PowerNap get idx entry");
                        MonitorInfo monitor = _AllInfoMonitors[int.Parse(idex)];
                        S_PowerNap_RESPONSE = new CLI_RESPONSE(monitor);

                        S_PowerNap_RESPONSE.Command = "GET";
                        S_PowerNap_RESPONSE.TargetFeature = "PowerNap";

                        read_list.RemoveAll(x => x.ServiceTag == null);
                        int idx = read_list.FindIndex(x => x.ServiceTag.Equals(monitor.edid.ServiceTag));
                        if (read_list.Count == 0 || idx < 0)  //if no PowerNap setting exist, create a new setting
                        {
                            await SetPowerNapAsync(PowerNapType.Off, devMgr, monitor.edid.ModelName, monitor.edid.SerialNumber, monitor.edid.ServiceTag);
                            S_PowerNap_RESPONSE.Result = "PASS";
                            S_PowerNap_RESPONSE.Value = "Off";
                        }
                        else
                        {
                            PowerNapSetting temp = read_list[idx];
                            S_PowerNap_RESPONSE.Result = "PASS";
                            if (temp.RunType.ToString().Equals("SleepIfRunning"))
                                S_PowerNap_RESPONSE.Value = "Sleep";
                            else
                                S_PowerNap_RESPONSE.Value = temp.RunType.ToString();
                        }
                        //S_PowerNap_RESPONSE.Value += "," + (ddpmSettings.LockSettings.Lock_Display_PowerNap ? "LOCK" : "UNLOCK");
                        output += "\n" + JsonConvert.SerializeObject(S_PowerNap_RESPONSE, Formatting.Indented);
                    }
                }
                else if (serviceTag.Count != 0)
                {
                    foreach (string tag in serviceTag)
                    {
                        writelog("PowerNap get tag entry");
                        var tmp = _AllInfoMonitors.FindAll(x => x.edid.ServiceTag.ToUpper().Equals(tag.ToUpper()));
                        foreach (MonitorInfo monitor in tmp)
                        {
                            S_PowerNap_RESPONSE = new CLI_RESPONSE(monitor);

                            S_PowerNap_RESPONSE.Command = "GET";
                            S_PowerNap_RESPONSE.TargetFeature = "PowerNap";

                            read_list.RemoveAll(x => x.ServiceTag == null);
                            int idx = read_list.FindIndex(x => x.ServiceTag.Equals(monitor.edid.ServiceTag));
                            if (read_list.Count == 0 || idx < 0)  //if no PowerNap setting exist, create a new setting
                            {
                                await SetPowerNapAsync(PowerNapType.Off, devMgr, monitor.edid.ModelName, monitor.edid.SerialNumber, monitor.edid.ServiceTag);

                                S_PowerNap_RESPONSE.Result = "PASS";
                                S_PowerNap_RESPONSE.Value = "Off";
                            }
                            else
                            {
                                PowerNapSetting temp = read_list[idx];
                                S_PowerNap_RESPONSE.Result = "PASS";
                                if (temp.RunType.ToString().Equals("SleepIfRunning"))
                                    S_PowerNap_RESPONSE.Value = "Sleep";
                                else
                                    S_PowerNap_RESPONSE.Value = temp.RunType.ToString();
                            }
                            //S_PowerNap_RESPONSE.Value += "," + (ddpmSettings.LockSettings.Lock_Display_PowerNap ? "LOCK" : "UNLOCK");
                            output += "\n" + JsonConvert.SerializeObject(S_PowerNap_RESPONSE, Formatting.Indented);
                        }
                    }
                }
            }

            writelog($"PowerNap return exit value{output}");
            return ((int)CLI_ExitCode.success, output);
        }

        #region PIP/PBP - (Robert_Lin 2024-6-13, Unused) (Added by Robert_Lin, 2024-6-5)

        //  arg[0]   arg[1]             arg[2]
        // /display /PxpGetCapabilities monitorIndex
        private async Task<int> PxpGetCapabilities(IDeviceManagerSA devMgr, string arg2)
        {
            Console.WriteLine($"PxpGetCapabilities: {arg2}");

            if (devMgr == null)
            {
                writelog("PxpSetMode: IDeviceManagerSA is null.");
                await Console.Out.WriteLineAsync("IDeviceManagerSA is null.");
                return (int)CLI_ExitCode.null_device_manager;
            }
            if (_AllInfoMonitors == null)
            {
                writelog("AllInfoMonitors is null, query again...");
                _AllInfoMonitors = await devMgr.GetMonitors();
                if (_AllInfoMonitors == null)
                {
                    writelog("No monitor connected.");
                    Console.WriteLine("No monitor connected.");
                    return (int)CLI_ExitCode.null_device_manager;
                }
            }
            writelog($"Monitor count = {_AllInfoMonitors.Count}");

            //arg2 = zero based index to Monitors
            int idxMonitor = 0;
            if (!ConvertToInt(arg2, out idxMonitor))
            {
                writelog("Invalid syntax of argument {monitorIndex}.");
                Console.WriteLine("Invalid syntax of argument {monitorIndex}.");
                return (int)CLI_ExitCode.invalide_cmdline_syntax;
            }

            if ((idxMonitor < 0) || (idxMonitor >= _AllInfoMonitors.Count))
            {
                writelog("{monitorIndex} out of range.");
                Console.WriteLine("{monitorIndex} out of range.");
                return (int)CLI_ExitCode.invalide_cmdline_syntax;
            }

            UInt16[] pxpCaps = devMgr.GetPipPbpCapabilitiesWords(_AllInfoMonitors[idxMonitor]).Result;
            if (pxpCaps == null)
            {
                writelog("Fail to get Pxp Capabilities");
                Console.WriteLine("Fail to get Pxp Capabilities");
                return (int)CLI_ExitCode.functional_error;
            }
            if (pxpCaps.Length <= 0)
            {
                writelog("Pxp Capabilities is empty.");
                Console.WriteLine("Pxp Capabilities is empty.");
                return (int)CLI_ExitCode.functional_error;
            }

            Console.WriteLine($"Monitor [{_AllInfoMonitors[idxMonitor].modelName}] : PXP Capabilities:");
            Console.WriteLine("\"Pxp.GetCapabilities\": { [");
            bool isFirst = true;
            foreach (UInt16 cap in pxpCaps)
            {
                if (isFirst)
                {
                    Console.Write($"\"0x{cap:X}\"");
                    isFirst = false;
                }
                else
                {
                    Console.Write($",\r\n\"0x{cap:X}\"");
                }
            }
            Console.WriteLine("\r\n] }");
            return (int)CLI_ExitCode.success;
        }

        //  arg[0]   arg[1]     arg[2]       arg[3]
        // /display /PxpSetMode monitorIndex modeCode
        private async Task<int> PxpSetMode(IDeviceManagerSA devMgr, string arg2, string arg3)
        {
            Console.WriteLine($"PxpSetMode: {arg2} {arg3}");
            if (devMgr == null)
            {
                writelog("PxpSetMode: IDeviceManagerSA is null.");
                await Console.Out.WriteLineAsync("IDeviceManagerSA is null.");
                return (int)CLI_ExitCode.null_device_manager;
            }
            if (_AllInfoMonitors == null)
            {
                writelog("AllInfoMonitors is null, query again...");
                _AllInfoMonitors = await devMgr.GetMonitors();
                if (_AllInfoMonitors == null)
                {
                    writelog("No monitor connected.");
                    Console.WriteLine("No monitor connected.");
                    return (int)CLI_ExitCode.null_device_manager;
                }
            }
            writelog($"Monitor count = {_AllInfoMonitors.Count}");

            //arg2 = zero based index to Monitors
            int idxMonitor = 0;
            if (!ConvertToInt(arg2, out idxMonitor))
            {
                writelog("Invalid syntax of argument {monitorIndex}.");
                Console.WriteLine("Invalid syntax of argument {monitorIndex}.");
                return (int)CLI_ExitCode.invalide_cmdline_syntax;
            }

            if ((idxMonitor < 0) || (idxMonitor >= _AllInfoMonitors.Count))
            {
                writelog("{monitorIndex} out of range.");
                Console.WriteLine("{monitorIndex} out of range.");
                return (int)CLI_ExitCode.invalide_cmdline_syntax;
            }

            //arg3 = {x}
            int modeCode = -1;
            if (!ConvertToInt(arg3, out modeCode))
            {
                writelog("Invalid syntax of argument {modeCode}.");
                Console.WriteLine("Invalid syntax of argument {modeCode}.");
                return (int)CLI_ExitCode.invalide_cmdline_syntax;
            }

            bool result = await devMgr.SetPbpMode(_AllInfoMonitors[idxMonitor], (UInt16)modeCode);

            if (result)
            {
                Console.WriteLine("Result=OK");
                return 0;
            }

            return (int)CLI_ExitCode.functional_error;
        }

        //  arg[0]   arg[1]     arg[2]
        // /display /PxpGetMode monitorIndex
        private async Task<int> PxpGetMode(IDeviceManagerSA devMgr, string arg2)
        {
            Console.WriteLine($"PxpGetMode: {arg2}");
            if (devMgr == null)
            {
                writelog("PxpSetMode: IDeviceManagerSA is null.");
                await Console.Out.WriteLineAsync("IDeviceManagerSA is null.");
                return (int)CLI_ExitCode.null_device_manager;
            }
            if (_AllInfoMonitors == null)
            {
                writelog("AllInfoMonitors is null, query again...");
                _AllInfoMonitors = await devMgr.GetMonitors();
                if (_AllInfoMonitors == null)
                {
                    writelog("No monitor connected.");
                    Console.WriteLine("No monitor connected.");
                    return (int)CLI_ExitCode.null_device_manager;
                }
            }
            writelog($"Monitor count = {_AllInfoMonitors.Count}");

            //arg2 = zero based index to Monitors
            int idxMonitor = 0;
            if (!ConvertToInt(arg2, out idxMonitor))
            {
                writelog("Invalid syntax of argument {monitorIndex}.");
                Console.WriteLine("Invalid syntax of argument {monitorIndex}.");
                return (int)CLI_ExitCode.invalide_cmdline_syntax;
            }

            if ((idxMonitor < 0) || (idxMonitor >= _AllInfoMonitors.Count))
            {
                writelog("{monitorIndex} out of range.");
                Console.WriteLine("{monitorIndex} out of range.");
                return (int)CLI_ExitCode.invalide_cmdline_syntax;
            }

            ObjGetVCP result = await devMgr.GetPxpMode(_AllInfoMonitors[idxMonitor]);

            if (result.result)
            {
                UInt16 mode = Convert.ToUInt16(result.value);
                Console.WriteLine($"{{ \"PxpMode\": \"0x{mode:X}\" }}");
                Console.WriteLine("Result=OK");
                return 0;
            }
            else
                Console.WriteLine("Result=ERROR");

            return (int)CLI_ExitCode.functional_error;
        }

        //  args[0]  args[1]      args[2]      args[3] args[4]
        // /display /PxpSwapVideo monitorIndex x       x
        private async Task<int> PxpSwapVideo(IDeviceManagerSA devMgr, string arg2, string arg3, string arg4)
        {
            Console.WriteLine($"PxpSwapVideo: {arg2} {arg3} {arg4}");
            if (devMgr == null)
            {
                writelog("PxpSwapVideo: IDeviceManagerSA is null.");
                await Console.Out.WriteLineAsync("IDeviceManagerSA is null.");
                return (int)CLI_ExitCode.null_device_manager;
            }
            if (_AllInfoMonitors == null)
            {
                writelog("AllInfoMonitors is null, query again...");
                _AllInfoMonitors = await devMgr.GetMonitors();
                if (_AllInfoMonitors == null)
                {
                    writelog("No monitor connected.");
                    Console.WriteLine("No monitor connected.");
                    return (int)CLI_ExitCode.null_device_manager;
                }
            }
            writelog($"Monitor count = {_AllInfoMonitors.Count}");

            //arg2 = zero based index to Monitors
            int idxMonitor = 0;
            if (!ConvertToInt(arg2, out idxMonitor))
            {
                writelog("Invalid syntax of argument {monitorIndex}.");
                Console.WriteLine("Invalid syntax of argument {monitorIndex}.");
                return (int)CLI_ExitCode.invalide_cmdline_syntax;
            }

            if ((idxMonitor < 0) || (idxMonitor >= _AllInfoMonitors.Count))
            {
                writelog("{monitorIndex} out of range.");
                Console.WriteLine("{monitorIndex} out of range.");
                return (int)CLI_ExitCode.invalide_cmdline_syntax;
            }

            //arg3 = {x}
            ePxpInputs x = ConvertPxpInputsFromString(arg3);
            if (x == ePxpInputs.invalid)
            {
                writelog("Invalid syntax of argument {x}.");
                Console.WriteLine("Invalid syntax of argument {x}.");
                return (int)CLI_ExitCode.invalide_cmdline_syntax;
            }

            //arg4 = {y}
            ePxpInputs y = ConvertPxpInputsFromString(arg4);
            if (y == ePxpInputs.invalid)
            {
                writelog("Invalid syntax of argument {y}.");
                Console.WriteLine("Invalid syntax of argument {y}.");
                return (int)CLI_ExitCode.invalide_cmdline_syntax;
            }

            bool result = await devMgr.VideoSwap(_AllInfoMonitors[idxMonitor], (UInt16)x, (UInt16)y);
            if (result)
            {
                Console.WriteLine("Result=OK");
                return 0;
            }

            return (int)CLI_ExitCode.functional_error;
        }

        //  args[0] args[1]         args[2]
        // /display /PxpGetSubInput monitorIndex
        private async Task<int> PxpGetSubInputs(IDeviceManagerSA devMgr, string arg2)
        {
            Console.WriteLine($"PxpGetSubInputs: {arg2}");
            writelog($"PxpGetSubInputs: {arg2}");

            if (devMgr == null)
            {
                writelog("PxpSetMode: IDeviceManagerSA is null.");
                await Console.Out.WriteLineAsync("IDeviceManagerSA is null.");
                return (int)CLI_ExitCode.null_device_manager;
            }
            if (_AllInfoMonitors == null)
            {
                writelog("AllInfoMonitors is null, query again...");
                _AllInfoMonitors = await devMgr.GetMonitors();
                if (_AllInfoMonitors == null)
                {
                    writelog("No monitor connected.");
                    Console.WriteLine("No monitor connected.");
                    return (int)CLI_ExitCode.null_device_manager;
                }
            }
            writelog($"Monitor count = {_AllInfoMonitors.Count}");

            //arg2 = zero based index to Monitors
            int idxMonitor = 0;
            if (!ConvertToInt(arg2, out idxMonitor))
            {
                writelog("Invalid syntax of argument {monitorIndex}.");
                Console.WriteLine("Invalid syntax of argument {monitorIndex}.");
                return (int)CLI_ExitCode.invalide_cmdline_syntax;
            }

            if ((idxMonitor < 0) || (idxMonitor >= _AllInfoMonitors.Count))
            {
                writelog("{monitorIndex} out of range.");
                Console.WriteLine("{monitorIndex} out of range.");
                return (int)CLI_ExitCode.invalide_cmdline_syntax;
            }

            List<InputSourceObj> inputSources = await devMgr.GetSubInputs(_AllInfoMonitors[idxMonitor]);

            if (inputSources == null)
            {
                writelog("Functional error.");
                Console.WriteLine("Functional error.");
                return (int)CLI_ExitCode.functional_error;
            }
            else if (inputSources.Count <= 0)
            {
                writelog("No Sub input.");
                Console.WriteLine("No Sub input.");
                return (int)CLI_ExitCode.success;
            }

            Console.WriteLine("\"Subinputs\" :[");
            int idx = 0;
            foreach (InputSourceObj source in inputSources)
            {
                if (idx > 0)
                {
                    Console.WriteLine(",");
                }
                Console.WriteLine("{");
                Console.WriteLine($"  \"Input\":\"sub{idx + 1}\", \"Code\":\"{source.Code}\", \"Name\":\"{source.Name}\"");
                Console.Write("}");
                idx++;
            }
            Console.WriteLine("]");
            Console.WriteLine("Result=OK");
            return (int)CLI_ExitCode.success;
        }

        //  args[0]  args[1]         args[2]        args[3~5]
        // /display /PxpSetSubInputs {monitorIndex} [-sub1={inputSource}] [-sub2={inputSource}]...
        private async Task<int> PxpSetSubInputs(IDeviceManagerSA devMgr, string[] args)
        {
            if (devMgr == null)
            {
                writelog("PxpSetMode: IDeviceManagerSA is null.");
                await Console.Out.WriteLineAsync("IDeviceManagerSA is null.");
                return (int)CLI_ExitCode.null_device_manager;
            }
            if (_AllInfoMonitors == null)
            {
                writelog("AllInfoMonitors is null, query again...");
                _AllInfoMonitors = await devMgr.GetMonitors();
                if (_AllInfoMonitors == null)
                {
                    writelog("No monitor connected.");
                    Console.WriteLine("No monitor connected.");
                    return (int)CLI_ExitCode.null_device_manager;
                }
            }
            writelog($"Monitor count = {_AllInfoMonitors.Count}");

            string arg2 = args[2];
            //arg2 = zero based index to Monitors
            int idxMonitor = 0;
            if (!ConvertToInt(arg2, out idxMonitor))
            {
                writelog("Invalid syntax of argument {monitorIndex}.");
                Console.WriteLine("Invalid syntax of argument {monitorIndex}.");
                return (int)CLI_ExitCode.invalide_cmdline_syntax;
            }

            if ((idxMonitor < 0) || (idxMonitor >= _AllInfoMonitors.Count))
            {
                writelog("{monitorIndex} out of range.");
                Console.WriteLine("{monitorIndex} out of range.");
                return (int)CLI_ExitCode.invalide_cmdline_syntax;
            }

            string cmdLine = $"PxpGetSubInputs: {arg2}";

            //Parsing args: -sub1=inputsource1 -sub2=inputsource2 -sub3=inputsource3
            InputSourceObj? sub1Obj = null, sub2Obj = null, sub3Obj = null;
            for (int idxArg = 3; idxArg < args.Length; idxArg++)
            {
                string arg = args[idxArg];
                string inputSource = "";
                //If arg start with "-sub1="
                if (arg.StartsWith("-sub1=", StringComparison.OrdinalIgnoreCase))
                {
                    int idxStartOfInputSource = 6; //Lendth of "-sub1="
                    inputSource = arg.Substring(idxStartOfInputSource);
                    sub1Obj = new InputSourceObj(inputSource);
                }
                else if (arg.StartsWith("-sub2=", StringComparison.OrdinalIgnoreCase))
                {
                    int idxStartOfInputSource = 6; //Lendth of "-sub2="
                    inputSource = arg.Substring(idxStartOfInputSource);
                    sub2Obj = new InputSourceObj(inputSource);
                }
                else if (arg.StartsWith("-sub3=", StringComparison.OrdinalIgnoreCase))
                {
                    int idxStartOfInputSource = 6; //Lendth of "-sub2="
                    inputSource = arg.Substring(idxStartOfInputSource);
                    sub3Obj = new InputSourceObj(inputSource);
                }
            }

            bool res = await devMgr.SetSubInputs(_AllInfoMonitors[idxMonitor], sub1Obj, sub2Obj, sub3Obj);
            if (res)
            {
                Console.WriteLine("Result=OK");
                return (int)CLI_ExitCode.success;
            }

            Console.WriteLine("Result=Error");
            return (int)CLI_ExitCode.functional_error;
        }

        #endregion PIP/PBP - (Robert_Lin 2024-6-13, Unused) (Added by Robert_Lin, 2024-6-5)i

        #region Helper functions (Robert_Lin 2024-6-13 Unused if remove PIP/PBP region)

        //Try to convert input string to int.
        // strIn    outValue
        // "20"     20
        // 20h"     32
        // "0x20"   32
        //Logic:
        // 1 Trim the starting/ending white-space characters
        // 2 If start with "0x" => base=16
        // 3 if end with 'h' => trim end 'h' and base=16
        // 4 otherwise => base=10
        // 5 return int.TryParse(strIn, base, outValue)
        //Unit Test: using below code to do test
        /*
            int outValue = 0;
            string[] testCases =
            {
                " 20", "20h", "0x20", "  0x20  ", "   20h  ", " 0x20h  "
            };
            foreach (string strIn in testCases)
            {
                Console.WriteLine($"ConvertToInt(\"{strIn}\") return {ConvertToInt(strIn, out outValue)}, outValue={outValue}");
            }
        */

        private bool ConvertToInt(string strIn, out int outValue)
        {
            strIn = strIn.Trim();
            NumberStyles numStyle = NumberStyles.Integer;
            if (strIn.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
            {
                strIn = strIn.Substring(2);
                numStyle = NumberStyles.HexNumber;
            }
            else if (strIn.EndsWith('h') || strIn.EndsWith('H'))
            {
                strIn = strIn.TrimEnd(new char[] { 'h', 'H' });
                numStyle = NumberStyles.HexNumber;
            }
            CultureInfo provider = new CultureInfo("en-US");
            return int.TryParse(strIn, numStyle, provider, out outValue);
        }

        //Helper function, convert string to ePxpInputs
        public static ePxpInputs ConvertPxpInputsFromString(string str)
        {
            try
            {
                ePxpInputs ret = (ePxpInputs)System.Enum.Parse(typeof(ePxpInputs), str, true);
                return ret;
            }
            catch (Exception e)
            {
            }
            return ePxpInputs.invalid;
        }

        #endregion Helper functions (Robert_Lin 2024-6-13 Unused if remove PIP/PBP region)

        #region PxP CLI - Robert_Lin 2024-6-13 for new CLI command syntax

        private async Task<(int code, string result)> CLI_Pxp(IDeviceManagerSA devMgr, CommandLineInput cmdLineInput)
        {
            int exitCode = CLIPxp.Execute(devMgr, cmdLineInput);
            System.Console.WriteLine(JsonConvert.SerializeObject(CLIPxp.Responses, Formatting.Indented));
            return (exitCode, JsonConvert.SerializeObject(CLIPxp.Responses, Formatting.Indented));
        }

        #endregion PxP CLI - Robert_Lin 2024-6-13 for new CLI command syntax

        #region Malik

        private (int code, string result) GetDeviceData(IDeviceManagerSA devMgr, CommandLineInput commandLineInput)
        {
            if (commandLineInput.Command == "GET" && commandLineInput.Options.Count < 2)
            {
                return DeviceData(devMgr, commandLineInput).Result;
            }
            //else if (commandLineInput.Command == "SET" && commandLineInput.Options.Count == 1)
            //{
            //    return ApplyConfiguration(devMgr, commandLineInput).Result;
            //}
            else
            {
                CLI_RESPONSE cli_Response = new CLI_RESPONSE();
                cli_Response.Command = commandLineInput.Command;
                cli_Response.TargetFeature = commandLineInput.TargetFeature;
                cli_Response.Result = "FAIL";
                cli_Response.Message = "Invalid command line syntax, missing -value=... or more than one -value=...";
                return ((int)CLI_ExitCode.invalide_cmdline_syntax, cli_Response.ToJson());
            }
        }

        private async Task<(int code, string result)> DeviceData(IDeviceManagerSA devMgr, CommandLineInput commandLineInput)
        {
            ObjGetVCP rc = new ObjGetVCP();
            DisplayPropertiesInfo displayPropertiesInfo = new DisplayPropertiesInfo();
            ALSConfig param = new ALSConfig();
            string[] Orientations_Str = new string[] { "Landscape", "Portrait", "Landscape(flipped)", "Portrait(flipped)" };
            string output = string.Empty;
            string output_2 = string.Empty;
            bool recode_dis = false;
            bool recode_per = false;

            List<int> _monitorIndeies = new List<int>();
            List<DeviceInfo> _deviceInfoFinal = new List<DeviceInfo>();
            /*_deviceHelper = new DeviceHelper
            {
                deviceInfo = new List<DeviceInfo>()
            };*/

            List<DeviceInfo> _deviceinfo = null;
            DeviceInfo match = null;
            _deviceinfo = _devMgr.GetDevices().Result.deviceInfo;

            //if (_AllInfoMonitors == null)
            _AllInfoMonitors = await devMgr.GetMonitors();
            _monitorIndeies = GetMonitorIndeies(commandLineInput, _AllInfoMonitors);
            _deviceInfoFinal = GetDDeviceIndeies(commandLineInput, _deviceinfo);

            int index = 0;

            if (commandLineInput.Options.Count > 0)
            {
                if (!string.IsNullOrEmpty(commandLineInput.Options[0].Option_Value))
                {
                    switch (commandLineInput.Options[0].Option_Value.ToUpper())
                    {
                        case "DISPLAY":
                            foreach (int idx in _monitorIndeies)
                            {
                                MonitorInfo monitor = _AllInfoMonitors[idx];
                                Get_DeviceData get_DeviceData = new Get_DeviceData(monitor);


                                //get_DeviceData.Model = monitor.modelName;
                                //get_DeviceData.SerialNumber = monitor.edid.SerialNumber;
                                //get_DeviceData.Index = change_0base_to_1base((monitor.Index).ToString());
                                //get_DeviceData.ServiceTag = monitor.edid.ServiceTag;

                                get_DeviceData.Manufacturer = modify_Manufactur(monitor.edid.ManufactureID);
                                get_DeviceData.PID = monitor.edid.PID.ToString();
                                get_DeviceData.ManufacturingYear = monitor.edid.Year.ToString();
                                get_DeviceData.ManufacturingWeek = "ISO week " + monitor.edid.Week.ToString();
                                get_DeviceData.FirmwareVersion = monitor.FwVersion;

                                Trace.WriteLine(monitor.edid.Edid);
                                //if (monitor.CapabilityDic.ContainsKey("C0"))
                                //{
                                writelog($"MonitorActiveHour Entry");
                                rc = GetVCPCode(devMgr, monitor, "0xC0").Result;
                                get_DeviceData.MonitorActiveHour = (rc.value).ToString() + " hours";
                                writelog($"MonitorActiveHour Exit return value: {(rc.value).ToString()}");
                                //}

                                if (monitor.CapabilityDic.ContainsKey("B6"))
                                {
                                    writelog($"DisplayTechnologyType Entry");
                                    rc = GetVCPCode(devMgr, monitor, "0xB6").Result;
                                    get_DeviceData.DisplayTechnologyType = get_display_technology_type((rc.value).ToString());
                                    writelog($"MonitorActiveHour Exit return value: {get_display_technology_type((rc.value).ToString())}");
                                }

                                string HexString = monitor.edid.Edid;
                                string text = HexString.Substring("00FFFFFFFFFFFF00".Length + 26, 4);
                                int num_x = 0;
                                int num_y = 0;

                                if (IsHexNumeric(text.Substring(0, 2)))
                                {
                                    num_x = int.Parse(text.Substring(0, 2), NumberStyles.HexNumber);
                                    num_y = int.Parse(text.Substring(2, 2), NumberStyles.HexNumber);
                                }
                                writelog($"ScreenSize Entry");
                                get_DeviceData.ScreenSize = $"{num_x}0 x {num_y}0 mm ({monitor.edid.Size.ToString("0.00")} in)";
                                writelog($"ScreenSize Exit return value: {$"{num_x}0 x {num_y}0 mm ({monitor.edid.Size.ToString("0.00")} in)"}");

                                int MaxWidth = 0;
                                int MaxHigh = 0;
                                float Frequency = 0;
                                string resolution = string.Empty;
                                writelog($"OptimalResolution, Resolution Entry");
                                displayPropertiesInfo = devMgr.GetDisplayPropertiesInfo(monitor).Result;
                                foreach (Properties displayProperties in displayPropertiesInfo.SupportedProperties.Properties)
                                {
                                    if (displayProperties.Resolutions_High > MaxHigh)
                                    {
                                        MaxHigh = displayProperties.Resolutions_High;
                                        MaxWidth = displayProperties.Resolutions_Width;
                                        Frequency = displayProperties.Frequency;
                                    }
                                    if (displayProperties.isCurrent)
                                        resolution = $"{displayProperties.Resolutions_Width} x {displayProperties.Resolutions_High} at {displayProperties.Frequency.ToString("0.00")}Hz";
                                }

                                get_DeviceData.OptimalResolution = $"{MaxWidth} x {MaxHigh} at {Frequency.ToString("0.00")}Hz";
                                get_DeviceData.Resolution = resolution;
                                writelog($"OptimalResolution, Resolution Exit return value: OptimalResolution{$"{MaxWidth} x {MaxHigh} at {Frequency.ToString("0.00")}Hz"} Resolution{resolution}");

                                get_DeviceData.ActiveInputSource = GetCurrentInput(devMgr, (monitor.Index).ToString()).Result;
                                writelog($"ActiveInputSource Exit return value: {GetCurrentInput(devMgr, (monitor.Index).ToString()).Result}");

                                writelog($"ReadCurrentColorPreset Entry");
                                get_DeviceData.ColorPreset = devMgr.ReadCurrentColorPreset(monitor).Result;
                                writelog($"ReadCurrentColorPreset Exit return value: {devMgr.ReadCurrentColorPreset(monitor).Result}");

                                //if (monitor.CapabilityDic.ContainsKey("AA") && monitor.CapabilityDic["AA"] != null && monitor.CapabilityDic["AA"].Contains("00"))
                                //{
                                writelog($"ScreenOrientation Entry");
                                get_DeviceData.Orientation = Orientations_Str[(int)displayPropertiesInfo.CurrentOrientation];
                                writelog($"ScreenOrientation Exit return value: {Orientations_Str[(int)displayPropertiesInfo.CurrentOrientation]}");
                                //}

                                if (monitor.CapabilityDic.ContainsKey("12"))
                                {
                                    writelog($"ContrastLevel Entry");
                                    rc = GetVCPCode(devMgr, monitor, "0x12").Result;
                                    get_DeviceData.ContrastLevel = (rc.value).ToString() + "%";
                                    writelog($"ContrastLevel Exit return value: {(rc.value).ToString() + "%"}");

                                    writelog($"BrightnessLevel Entry");
                                    rc = GetVCPCode(devMgr, monitor, "0x10").Result;
                                    get_DeviceData.BrightnessLevel = (rc.value).ToString() + "%";
                                    writelog($"BrightnessLevel Exit return value: {(rc.value).ToString() + "%"}");

                                    get_DeviceData.LuminanceLevel = "N/A";
                                }
                                else
                                {
                                    get_DeviceData.ContrastLevel = "N/A";
                                    get_DeviceData.BrightnessLevel = "N/A";

                                    writelog($"LuminanceLevel Entry");
                                    rc = GetVCPCode(devMgr, monitor, "0x10").Result;
                                    get_DeviceData.LuminanceLevel = rc.value?.ToString() ?? "N/A";
                                    writelog($"LuminanceLevel Exit return value: {get_DeviceData.LuminanceLevel}");
                                }

                                if (monitor.CapabilityDic.ContainsKey("66"))
                                {
                                    writelog($"AutoBrightness Entry");
                                    param = devMgr.GetALSFeatureValue(monitor, ALSFeatureQueryType.AutoBrightness, 0).Result;
                                    get_DeviceData.AutoBrightness = param.isAutoBrightness ? "on" : "off";
                                    writelog($"AutoBrightness Exit return value: {(param.isAutoBrightness ? "on" : "off")}");

                                    writelog($"AutoBrightnessRangeLevel Entry");
                                    param = devMgr.GetALSFeatureValue(monitor, ALSFeatureQueryType.AutoBrightnessRangeLevel, 0).Result;
                                    if (param.AutoBrightnessRangeLevel != null)
                                    {
                                        get_DeviceData.AutoBrightnessRangeLevel = param.AutoBrightnessRangeLevel.level_name;
                                        writelog($"AutoBrightness Exit return value: {(param.AutoBrightnessRangeLevel.level_name)}");
                                    }
                                    writelog($"AutoBrightness Exit return value: FAIL");

                                    writelog($"AutoColorTemp Entry");
                                    param = devMgr.GetALSFeatureValue(monitor, ALSFeatureQueryType.AutoColorTemperature, 0).Result;
                                    get_DeviceData.AutoColorTemp = param.isAutoColorTemp ? "on" : "off";
                                    writelog($"AutoColorTemp Exit return value: {(param.isAutoColorTemp ? "on" : "off")}");

                                    writelog($"PrimaryMonitorForSync Entry");
                                    param = devMgr.GetALSFeatureValue(monitor, ALSFeatureQueryType.PrimaryMonitorSync, 0).Result;
                                    get_DeviceData.PrimaryMonitorForSync = param.isPrimaryMonitorSync ? "on" : "off";
                                    writelog($"PrimaryMonitorForSync Exit return value: {(param.isPrimaryMonitorSync ? "on" : "off")}");
                                }

                                writelog($"AspectRatio Entry");
                                //int gcd = (int)GCD((ulong)MaxWidth, (ulong)MaxHigh);
                                //get_DeviceData.AspectRatio = $"{MaxWidth / gcd}:{MaxHigh / gcd}";
                                //writelog($"AspectRatio Exit return value: {$"{MaxWidth / gcd}:{MaxHigh / gcd}"}");
                                string gcd = "N/A";
                                if (num_x != 0 && num_y != 0)
                                    gcd = Get_AR((double)num_x / (double)num_y);
                                get_DeviceData.AspectRatio = $"{gcd}";
                                writelog($"AspectRatio Exit return value: {$"{gcd}"}");

                                writelog($"USB_CPrioritization, USBCPrioritizationType Entry");
                                if (displayPropertiesInfo.SupportedUSBCPrioritization)
                                {
                                    get_DeviceData.USB_CPrioritization = displayPropertiesInfo.USBCPrioritizationType == USBCPrioritizationType.HighDataSpeed ? "High Data Speed" : "High Resolution";
                                }
                                else
                                    get_DeviceData.USB_CPrioritization = "NOT SUPPORT";
                                writelog($"USB_CPrioritization, USBCPrioritizationType Exit return value: {get_DeviceData.USB_CPrioritization}");
                                writelog($"ColorManagement, ColorManagementType Entry");
                                get_DeviceData.ColorManagement = devMgr.GetColorManagementStatus(monitor).Result;

                                writelog($"SpeakerMicrophone Entry (62, 8D)");
                                if (CheckSpeakerSupported(monitor, commandLineInput) && CheckMicrophoneSupported(monitor, commandLineInput))
                                {
                                    rc = GetVCPCode(devMgr, monitor, "0x62").Result;
                                    string speaker = get_SpeakerMicrophone_status(Convert.ToInt32(rc.value));
                                    rc = GetVCPCode(devMgr, monitor, "0x8D").Result;
                                    string microphone = get_SpeakerMicrophone_status(Convert.ToInt32(rc.value));
                                    if (speaker == microphone)
                                        get_DeviceData.SpeakerMicrophone = speaker;
                                    else
                                        get_DeviceData.SpeakerMicrophone = "N/A";
                                }
                                else
                                    get_DeviceData.SpeakerMicrophone = "N/A";
                                writelog($"SpeakerMicrophone  (62, 8D) Exit return value: {get_DeviceData.SpeakerMicrophone}");

                                writelog($"SpeakerVolume Entry (62)");
                                if (CheckSpeakerSupported(monitor, commandLineInput))
                                {
                                    rc = GetVCPCode(devMgr, monitor, "0x62", 2).Result;
                                    int getvalue = Convert.ToInt32(rc.value);
                                    get_DeviceData.SpeakerVolume = get_SpeakerVolume_status(Convert.ToInt32(rc.value));
                                }
                                else
                                    get_DeviceData.SpeakerVolume = "N/A";
                                writelog($"SpeakerVolume  (62) Exit return value: {get_DeviceData.SpeakerVolume}");

                                writelog($"MicrophoneControl Entry");
                                if (CheckMicrophoneSupported(monitor, commandLineInput))
                                {
                                    rc = GetVCPCode(devMgr, monitor, "0x8D").Result;
                                    int getvalue = Convert.ToInt32(rc.value);
                                    get_DeviceData.MicrophoneControl = get_MicrophoneControl_status(Convert.ToInt32(rc.value));
                                }
                                else
                                    get_DeviceData.MicrophoneControl = "N/A";
                                writelog($"MicrophoneControl Exit return value: {get_DeviceData.MicrophoneControl}");

                                /*writelog($"Uniformity Entry");
                                if (monitor.CapabilityDic.ContainsKey("E4"))
                                {
                                    rc = GetVCPCode(devMgr, monitor, "0xE4").Result;
                                    get_DeviceData.Uniformity = (rc.value).ToString();
                                }
                                else
                                    get_DeviceData.Uniformity = "N/A";
                                writelog($"Uniformity Exit return value: {(rc.value).ToString()}");*/

                                writelog($"PowerNap Entry");
                                List<PowerNapSetting> read_list = devMgr.ReadPowerNapSettings().Result;
                                read_list.RemoveAll(x => x.SerialNumber == null);
                                int powernap_idx = read_list.FindIndex(x => x.SerialNumber.Equals(monitor.edid.SerialNumber));
                                if (read_list.Count == 0 || powernap_idx < 0)  //if no PowerNap setting exist, create a new setting
                                {
                                    await SetPowerNapAsync(PowerNapType.Off, devMgr, monitor.edid.ModelName, monitor.edid.SerialNumber, monitor.edid.ServiceTag);
                                    //PowerNapSetting setting = new PowerNapSetting
                                    //{
                                    //    Status = false,
                                    //    ModelName = monitor.edid.ModelName,
                                    //    SerialNumber = monitor.edid.SerialNumber,
                                    //    RunType = PowerNapType.Off
                                    //};
                                    //await devMgr.SavePowerNapSetting(setting);
                                    //UpdateUINotify off = new UpdateUINotify();
                                    //off.UI_Field_Name = "POWERNAP;OFF";
                                    //devMgr.OnUIUpdateNotify(off);
                                }
                                else if (read_list.Count != 0)
                                {
                                    if (read_list[powernap_idx].RunType.ToString().ToUpper() == "SLEEPIFRUNNING")
                                        get_DeviceData.PowerNap = "Sleep";
                                    else
                                        get_DeviceData.PowerNap = read_list[powernap_idx].RunType.ToString();
                                }
                                else
                                    get_DeviceData.PowerNap = "N/A";
                                writelog($"PowerNap Exit return value: {get_DeviceData.PowerNap}");

                                if (monitor.CapabilityDic.ContainsKey("CC"))
                                {
                                    writelog($"OSD_language Entry");
                                    rc = GetVCPCode(devMgr, monitor, "0xCC").Result;
                                    get_DeviceData.OSD_language = get_language((rc.value).ToString());
                                    writelog($"OSD_language Exit return value: {get_DeviceData.OSD_language}");
                                }

                                recode_dis = true;

                                index = int.Parse(change_0base_to_1base((monitor.Index).ToString()));

                                System.Console.WriteLine(JsonConvert.SerializeObject(get_DeviceData, Formatting.Indented));
                                output += "\n" + JsonConvert.SerializeObject(get_DeviceData, Formatting.Indented);
                            }

                            break;
                        case "MOUSE":
                        case "KEYBOARD":
                        case "WEBCAM":
                        case "WIREDAUDIO":
                        case "HEADSET":
                        case "PEN":
                        case "DOCK"://20250306Elsa add for DeviceData support guid
                            if (commandLineInput.GuidString != null && commandLineInput.GuidString.Count > 0 && !string.IsNullOrEmpty(commandLineInput.GuidString[0].ToString()))
                            {
                                match = _deviceinfo.SingleOrDefault(x => x.ID.ToString().Equals(commandLineInput.GuidString[0].ToString(), StringComparison.OrdinalIgnoreCase));
                                if (match != null)
                                {
                                    recode_per = true;
                                    output += "\n" + GetDeviceDataPeripheralResponse(0, match).ToJson();
                                }
                            }
                            else
                            {
                                foreach (var device in _deviceInfoFinal)
                                {
                                    if (device.LogicalDeviceType.Equals($"Logical{commandLineInput.Options[0].Option_Value}", StringComparison.OrdinalIgnoreCase))
                                    {
                                        index++;
                                        recode_per = true;
                                        output += "\n" + GetDeviceDataPeripheralResponse(index, device).ToJson();
                                    }
                                }
                            }
                            break;
                        default:
                            break;
                    }
                }
            }
            else if (commandLineInput.Options.Count == 0)
            {
                foreach (int idx in _monitorIndeies)
                {
                    MonitorInfo monitor = _AllInfoMonitors[idx];
                    Get_DeviceData get_DeviceData = new Get_DeviceData(monitor);


                    //get_DeviceData.Model = monitor.modelName;
                    //get_DeviceData.SerialNumber = monitor.edid.SerialNumber;
                    //get_DeviceData.Index = change_0base_to_1base((monitor.Index).ToString());
                    //get_DeviceData.ServiceTag = monitor.edid.ServiceTag;

                    get_DeviceData.Manufacturer = modify_Manufactur(monitor.edid.ManufactureID);
                    get_DeviceData.PID = monitor.edid.PID.ToString();
                    get_DeviceData.ManufacturingYear = monitor.edid.Year.ToString();
                    get_DeviceData.ManufacturingWeek = "ISO week " + monitor.edid.Week.ToString();
                    get_DeviceData.FirmwareVersion = monitor.FwVersion;

                    //if (monitor.CapabilityDic.ContainsKey("C0"))
                    //{
                    writelog($"MonitorActiveHour Entry");
                    rc = GetVCPCode(devMgr, monitor, "0xC0").Result;
                    get_DeviceData.MonitorActiveHour = (rc.value).ToString() + " hours";
                    writelog($"MonitorActiveHour Exit return value: {(rc.value).ToString()}");
                    //}

                    if (monitor.CapabilityDic.ContainsKey("B6"))
                    {
                        writelog($"DisplayTechnologyType Entry");
                        rc = GetVCPCode(devMgr, monitor, "0xB6").Result;
                        get_DeviceData.DisplayTechnologyType = get_display_technology_type((rc.value).ToString());
                        writelog($"MonitorActiveHour Exit return value: {get_display_technology_type((rc.value).ToString())}");
                    }

                    string HexString = monitor.edid.Edid;
                    string text = HexString.Substring("00FFFFFFFFFFFF00".Length + 26, 4);
                    int num_x = 0;
                    int num_y = 0;
                    if (IsHexNumeric(text.Substring(0, 2)))
                    {
                        num_x = int.Parse(text.Substring(0, 2), NumberStyles.HexNumber);
                        num_y = int.Parse(text.Substring(2, 2), NumberStyles.HexNumber);
                    }
                    writelog($"ScreenSize Entry");
                    get_DeviceData.ScreenSize = $"{num_x}0 x {num_y}0 mm ({monitor.edid.Size.ToString("0.00")} in)";
                    writelog($"ScreenSize Exit return value: {$"{num_x}0 x {num_y}0 mm ({monitor.edid.Size.ToString("0.00")} in)"}");

                    int MaxWidth = 0;
                    int MaxHigh = 0;
                    float Frequency = 0;
                    string resolution = string.Empty;
                    writelog($"OptimalResolution, Resolution Entry");
                    displayPropertiesInfo = devMgr.GetDisplayPropertiesInfo(monitor).Result;
                    foreach (Properties displayProperties in displayPropertiesInfo.SupportedProperties.Properties)
                    {
                        if (displayProperties.Resolutions_High > MaxHigh)
                        {
                            MaxHigh = displayProperties.Resolutions_High;
                            MaxWidth = displayProperties.Resolutions_Width;
                            Frequency = displayProperties.Frequency;
                        }
                        if (displayProperties.isCurrent)
                            resolution = $"{displayProperties.Resolutions_Width} x {displayProperties.Resolutions_High} at {displayProperties.Frequency.ToString("0.00")}Hz";
                    }

                    get_DeviceData.OptimalResolution = $"{MaxWidth} x {MaxHigh} at {Frequency.ToString("0.00")}Hz";
                    get_DeviceData.Resolution = resolution;
                    writelog($"OptimalResolution, Resolution Exit return value: OptimalResolution{$"{MaxWidth} x {MaxHigh} at {Frequency.ToString("0.00")}Hz"} Resolution{resolution}");

                    get_DeviceData.ActiveInputSource = GetCurrentInput(devMgr, (monitor.Index).ToString()).Result;
                    writelog($"ActiveInputSource Exit return value: {GetCurrentInput(devMgr, (monitor.Index).ToString()).Result}");

                    writelog($"ReadCurrentColorPreset Entry");
                    get_DeviceData.ColorPreset = devMgr.ReadCurrentColorPreset(monitor).Result;
                    writelog($"ReadCurrentColorPreset Exit return value: {devMgr.ReadCurrentColorPreset(monitor).Result}");

                    //if (monitor.CapabilityDic.ContainsKey("AA") && monitor.CapabilityDic["AA"] != null && monitor.CapabilityDic["AA"].Contains("00"))
                    //{
                    writelog($"ScreenOrientation Entry");
                    get_DeviceData.Orientation = Orientations_Str[(int)displayPropertiesInfo.CurrentOrientation];
                    writelog($"ScreenOrientation Exit return value: {Orientations_Str[(int)displayPropertiesInfo.CurrentOrientation]}");
                    //}

                    if (monitor.CapabilityDic.ContainsKey("12"))
                    {
                        writelog($"ContrastLevel Entry");
                        rc = GetVCPCode(devMgr, monitor, "0x12").Result;
                        get_DeviceData.ContrastLevel = (rc.value).ToString() + "%";
                        writelog($"ContrastLevel Exit return value: {(rc.value).ToString() + "%"}");

                        writelog($"BrightnessLevel Entry");
                        rc = GetVCPCode(devMgr, monitor, "0x10").Result;
                        get_DeviceData.BrightnessLevel = (rc.value).ToString() + "%";
                        writelog($"BrightnessLevel Exit return value: {(rc.value).ToString() + "%"}");

                        get_DeviceData.LuminanceLevel = "N/A";
                    }
                    else
                    {
                        get_DeviceData.ContrastLevel = "N/A";
                        get_DeviceData.BrightnessLevel = "N/A";

                        writelog($"LuminanceLevel Entry");
                        rc = GetVCPCode(devMgr, monitor, "0x10").Result;
                        get_DeviceData.LuminanceLevel = rc.value?.ToString() ?? "N/A";
                        writelog($"LuminanceLevel Exit return value: {get_DeviceData.LuminanceLevel}");
                    }

                    if (monitor.CapabilityDic.ContainsKey("66"))
                    {
                        writelog($"AutoBrightness Entry");
                        param = devMgr.GetALSFeatureValue(monitor, ALSFeatureQueryType.AutoBrightness, 0).Result;
                        get_DeviceData.AutoBrightness = param.isAutoBrightness ? "on" : "off";
                        writelog($"AutoBrightness Exit return value: {(param.isAutoBrightness ? "on" : "off")}");

                        writelog($"AutoBrightnessRangeLevel Entry");
                        param = devMgr.GetALSFeatureValue(monitor, ALSFeatureQueryType.AutoBrightnessRangeLevel, 0).Result;
                        if (param.AutoBrightnessRangeLevel != null)
                        {
                            get_DeviceData.AutoBrightnessRangeLevel = param.AutoBrightnessRangeLevel.level_name;
                            writelog($"AutoBrightness Exit return value: {(param.AutoBrightnessRangeLevel.level_name)}");
                        }
                        writelog($"AutoBrightness Exit return value: FAIL");

                        writelog($"AutoColorTemp Entry");
                        param = devMgr.GetALSFeatureValue(monitor, ALSFeatureQueryType.AutoColorTemperature, 0).Result;
                        get_DeviceData.AutoColorTemp = param.isAutoColorTemp ? "on" : "off";
                        writelog($"AutoColorTemp Exit return value: {(param.isAutoColorTemp ? "on" : "off")}");

                        writelog($"PrimaryMonitorForSync Entry");
                        param = devMgr.GetALSFeatureValue(monitor, ALSFeatureQueryType.PrimaryMonitorSync, 0).Result;
                        get_DeviceData.PrimaryMonitorForSync = param.isPrimaryMonitorSync ? "on" : "off";
                        writelog($"PrimaryMonitorForSync Exit return value: {(param.isPrimaryMonitorSync ? "on" : "off")}");
                    }

                    writelog($"AspectRatio Entry");
                    //int gcd = (int)GCD((ulong)MaxWidth, (ulong)MaxHigh);
                    //get_DeviceData.AspectRatio = $"{MaxWidth / gcd}:{MaxHigh / gcd}";
                    //writelog($"AspectRatio Exit return value: {$"{MaxWidth / gcd}:{MaxHigh / gcd}"}");
                    string gcd = "N/A";
                    if (num_x != 0 && num_y != 0)
                        gcd = Get_AR((double)num_x / (double)num_y);
                    get_DeviceData.AspectRatio = $"{gcd}";
                    writelog($"AspectRatio Exit return value: {$"{gcd}"}");

                    writelog($"USB_CPrioritization, USBCPrioritizationType Entry");
                    if (displayPropertiesInfo.SupportedUSBCPrioritization)
                    {
                        get_DeviceData.USB_CPrioritization = displayPropertiesInfo.USBCPrioritizationType == USBCPrioritizationType.HighDataSpeed ? "High Data Speed" : "High Resolution";
                    }
                    else
                        get_DeviceData.USB_CPrioritization = "NOT SUPPORT";
                    writelog($"USB_CPrioritization, USBCPrioritizationType Exit return value: {get_DeviceData.USB_CPrioritization}");
                    writelog($"ColorManagement, ColorManagementType Entry");
                    get_DeviceData.ColorManagement = devMgr.GetColorManagementStatus(monitor).Result;

                    writelog($"SpeakerMicrophone Entry (62, 8D)");
                    if (CheckSpeakerSupported(monitor, commandLineInput) && CheckMicrophoneSupported(monitor, commandLineInput))
                    {
                        rc = GetVCPCode(devMgr, monitor, "0x62").Result;
                        string speaker = get_SpeakerMicrophone_status(Convert.ToInt32(rc.value));
                        rc = GetVCPCode(devMgr, monitor, "0x8D").Result;
                        string microphone = get_SpeakerMicrophone_status(Convert.ToInt32(rc.value));
                        if (speaker == microphone)
                            get_DeviceData.SpeakerMicrophone = speaker;
                        else
                            get_DeviceData.SpeakerMicrophone = "N/A";
                    }
                    else
                        get_DeviceData.SpeakerMicrophone = "N/A";
                    writelog($"SpeakerMicrophone  (62, 8D) Exit return value: {get_DeviceData.SpeakerMicrophone}");

                    writelog($"SpeakerVolume Entry (62)");
                    if (CheckSpeakerSupported(monitor, commandLineInput))
                    {
                        rc = GetVCPCode(devMgr, monitor, "0x62", 2).Result;
                        int getvalue = Convert.ToInt32(rc.value);
                        get_DeviceData.SpeakerVolume = get_SpeakerVolume_status(Convert.ToInt32(rc.value));
                    }
                    else
                        get_DeviceData.SpeakerVolume = "N/A";
                    writelog($"SpeakerVolume  (62) Exit return value: {get_DeviceData.SpeakerVolume}");

                    writelog($"MicrophoneControl Entry");
                    if (CheckMicrophoneSupported(monitor, commandLineInput))
                    {
                        rc = GetVCPCode(devMgr, monitor, "0x8D").Result;
                        int getvalue = Convert.ToInt32(rc.value);
                        get_DeviceData.MicrophoneControl = get_MicrophoneControl_status(Convert.ToInt32(rc.value));
                    }
                    else
                        get_DeviceData.MicrophoneControl = "N/A";
                    writelog($"MicrophoneControl Exit return value: {get_DeviceData.MicrophoneControl}");

                    /*writelog($"Uniformity Entry");
                    if (monitor.CapabilityDic.ContainsKey("E4"))
                    {
                        rc = GetVCPCode(devMgr, monitor, "0xE4").Result;
                        get_DeviceData.Uniformity = (rc.value).ToString();
                    }
                    else
                        get_DeviceData.Uniformity = "N/A";
                    writelog($"Uniformity Exit return value: {(rc.value).ToString()}");*/

                    writelog($"PowerNap Entry");
                    List<PowerNapSetting> read_list = devMgr.ReadPowerNapSettings().Result;
                    read_list.RemoveAll(x => x.SerialNumber == null);
                    int powernap_idx = read_list.FindIndex(x => x.SerialNumber.Equals(monitor.edid.SerialNumber));
                    if (read_list.Count == 0 || powernap_idx < 0)  //if no PowerNap setting exist, create a new setting
                    {
                        await SetPowerNapAsync(PowerNapType.Off, devMgr, monitor.edid.ModelName, monitor.edid.SerialNumber, monitor.edid.ServiceTag);
                        //PowerNapSetting setting = new PowerNapSetting
                        //{
                        //    Status = false,
                        //    ModelName = monitor.edid.ModelName,
                        //    SerialNumber = monitor.edid.SerialNumber,
                        //    RunType = PowerNapType.Off
                        //};
                        //await devMgr.SavePowerNapSetting(setting);
                        //UpdateUINotify off = new UpdateUINotify();
                        //off.UI_Field_Name = "POWERNAP;OFF";
                        //devMgr.OnUIUpdateNotify(off);
                    }
                    else if (read_list.Count != 0)
                    {
                        if (read_list[powernap_idx].RunType.ToString().ToUpper() == "SLEEPIFRUNNING")
                            get_DeviceData.PowerNap = "Sleep";
                        else
                            get_DeviceData.PowerNap = read_list[powernap_idx].RunType.ToString();
                    }
                    else
                        get_DeviceData.PowerNap = "N/A";
                    writelog($"PowerNap Exit return value: {get_DeviceData.PowerNap}");

                    if (monitor.CapabilityDic.ContainsKey("CC"))
                    {
                        writelog($"OSD_language Entry");
                        rc = GetVCPCode(devMgr, monitor, "0xCC").Result;
                        get_DeviceData.OSD_language = get_language((rc.value).ToString());
                        writelog($"OSD_language Exit return value: {get_DeviceData.OSD_language}");
                    }

                    recode_dis = true;

                    index = int.Parse(change_0base_to_1base((monitor.Index).ToString()));

                    System.Console.WriteLine(JsonConvert.SerializeObject(get_DeviceData, Formatting.Indented));
                    output += "\n" + JsonConvert.SerializeObject(get_DeviceData, Formatting.Indented);
                }

                foreach (var device in _deviceinfo)
                {
                    index++;
                    recode_per = true;
                    output += "\n" + GetDeviceDataPeripheralResponse(index, device).ToJson();
                }
            }

            CLI_RESPONSE3 cli_Response = new CLI_RESPONSE3();
            cli_Response.Command = commandLineInput.Command;
            cli_Response.TargetFeature = commandLineInput.TargetFeature;
            if (recode_per || recode_dis)
            {
                cli_Response.Result = "PASS";
                cli_Response.Message = "N/A";
            }
            else if (_deviceinfo.Count == 0 && _monitorIndeies.Count == 0)
            {
                cli_Response.Message = "No devices found";
            }
            else if (!recode_per && match == null)
            {
                cli_Response.Result = "FAIL";
                cli_Response.Message = "Invalid guid";
            }
            else
            {
                cli_Response.Result = "FAIL";
                cli_Response.Message = "Invalid command line syntax.";
            }
            output_2 += "\n" + JsonConvert.SerializeObject(cli_Response, Formatting.Indented);
            output_2 += output;
            return ((int)CLI_ExitCode.success, output_2);
        }


        private static string get_headsetconnection_type(HeadsetConnectionType ConnectionType)
        {
            switch (ConnectionType.ToString())
            {
                case "HeadsetConnectionTypeWired": return "Wired";
                case "HeadsetConnectionTypeWirelessDongle": return "WirelessDongle";
                case "HeadsetConnectionTypeWirelessBLE": return "WirelessBLE";
                default: return "Unknown";
            }
        }

        private static string get_display_technology_type(string index)
        {
            switch (index)
            {
                case "1": return "CRT (shadow mask)";
                case "2": return "CRT (aperture grill)";
                case "3": return "LCD (active matrix)";
                case "4": return "LCoS";
                case "5": return "Plasma";
                case "6": return "OLED";
                case "7": return "EL";
                case "8": return "Dynamic MEM  e.g. DLP";
                case "9": return "Static MEM  e.g. iMOD";
                default: return "Unknown";
            }
        }

        private static string get_language(string index)
        {
            //Trace.WriteLine($"language = {(int.Parse(index)).ToString("x2")}");
            switch ((int.Parse(index)).ToString("x2"))
            {
                case "01": return "Chinese";
                case "02": return "English";
                case "03": return "French";   // "Francais";
                case "04": return "German";   // "Deutschi";
                case "05": return "Italian";
                case "06": return "Japanese"; // "Japan";
                case "07": return "Korean";
                case "08": return "Portuguese";
                case "09": return "Russian";
                case "0a": return "Spanish";  // "Espanol";
                case "0b": return "Swedish";
                case "0c": return "Turkish";
                case "0d": return "Chinese-Simplified";
                case "0e": return "BrazilianPortuguese";
                case "0f": return "Arabic";
                case "10": return "Bulgarian";
                case "11": return "Croatian";
                case "12": return "Czech";
                case "13": return "Danish";
                case "14": return "Dutch";
                case "15": return "Estonian";
                case "16": return "Finnish";
                case "17": return "Greek";
                case "18": return "Hebrew";
                case "19": return "Hindi";
                case "1a": return "Hungarian";
                case "1b": return "Latvian";
                case "1c": return "Lithuanian";
                case "1d": return "Norwegian";
                case "1e": return "Polish";
                case "1f": return "Romanian";
                case "20": return "Serbian";
                case "21": return "Slovak";
                case "22": return "Slovenian";
                case "23": return "Thai";
                case "24": return "Ukrainian";
                case "25": return "Vietnamese";
                default: return "Unknown";
            }
        }

        private static string get_osd(string index)
        {
            //Trace.WriteLine($"language = {(int.Parse(index)).ToString("x2")}");
            switch ((int.Parse(index)).ToString("x2"))
            {
                case "01": return "OSDLock";
                case "02": return "OSDUnlock";
                default: return "Unknown";
            }
        }

        private static ulong GCD(ulong a, ulong b)
        {
            while (a != 0 && b != 0)
            {
                if (a > b)
                    a %= b;
                else
                    b %= a;
            }
            return a | b;
        }

        private static string Get_AR(double value)
        {
            switch (value)
            {
                case double n when n == 1.25:
                    return "5:4";
                case double n when n > 1.25 && n < 1.5://1.333
                    return "4:3";
                case double n when n == 1.5:
                    return "3:2";
                case double n when n == 1.6:
                    return "16:10";
                case double n when n > 1.6 && n < 1.7://1.666
                    return "15:9";
                case double n when (n > 1.7 && n < 2.0)://1.777
                    return "16:9";
                case double n when n == 2.0:
                    return "18:9";
                case double n when n > 2.0 && n < 2.3://2.222
                    return "20:9";
                case double n when n > 2.2 && n < 3.5://2.3....
                    return "21:9";
                case double n when n > 3.5://3.555
                    return "32:9";
                default:
                    return "N/A";
            }
        }

        private (int code, string result) Getcapabilitystringx(IDeviceManagerSA devMgr, CommandLineInput commandLineInput)
        {
            if (commandLineInput.Command == "SET" || commandLineInput.Options.Count != 0)
            {
                CLI_RESPONSE cli_Response = new CLI_RESPONSE();
                cli_Response.Command = commandLineInput.Command;
                cli_Response.TargetFeature = commandLineInput.TargetFeature;
                cli_Response.Result = "FAIL";
                cli_Response.Message = "Invalid command line syntax.";
                return ((int)CLI_ExitCode.invalide_cmdline_syntax, cli_Response.ToJson());
            }
            else
            {
                return Capabilitystring(devMgr, commandLineInput).Result;
            }
        }

        private async Task<(int code, string result)> Capabilitystring(IDeviceManagerSA devMgr, CommandLineInput commandLineInput)
        {
            string output = string.Empty;
            if (_AllInfoMonitors == null)
                _AllInfoMonitors = await devMgr.GetMonitors();

            if (commandLineInput.DeviceIndex.Count == 0 && commandLineInput.ServiceTag.Count == 0 && commandLineInput.Model.Count == 0)
            {
                foreach (MonitorInfo monitor in _AllInfoMonitors)
                {
                    Get_Capabilitystring get_Capabilitystring = new Get_Capabilitystring(monitor);
                    get_Capabilitystring.Command = commandLineInput.Command;
                    //get_Capabilitystring.Model = monitor.modelName;
                    get_Capabilitystring.TargetFeature = commandLineInput.TargetFeature;
                    //get_Capabilitystring.SerialNumber = monitor.edid.SerialNumber;
                    //get_Capabilitystring.ServiceTag = monitor.edid.ServiceTag;
                    //get_Capabilitystring.Index = change_0base_to_1base((monitor.Index).ToString());
                    get_Capabilitystring.Result = "PASS";
                    get_Capabilitystring.Message = "N/A";
                    get_Capabilitystring.CapabilityString = monitor.CapabilityString;

                    output += "\n" + JsonConvert.SerializeObject(get_Capabilitystring, Formatting.Indented);
                    //return ((int)CLI_ExitCode.success, output);
                }
            }
            else if (commandLineInput.DeviceIndex.Count > 0)
            {
                foreach (string idx in commandLineInput.DeviceIndex)
                {
                    MonitorInfo monitor = _AllInfoMonitors[Convert.ToInt32(idx)];

                    Get_Capabilitystring get_Capabilitystring = new Get_Capabilitystring(monitor);
                    get_Capabilitystring.Command = commandLineInput.Command;
                    //get_Capabilitystring.Model = monitor.modelName;
                    get_Capabilitystring.TargetFeature = commandLineInput.TargetFeature;
                    //get_Capabilitystring.SerialNumber = monitor.edid.SerialNumber;
                    //get_Capabilitystring.ServiceTag = monitor.edid.ServiceTag;
                    //get_Capabilitystring.Index = change_0base_to_1base((monitor.Index).ToString());
                    get_Capabilitystring.Result = "PASS";
                    get_Capabilitystring.Message = "N/A";
                    get_Capabilitystring.CapabilityString = monitor.CapabilityString;

                    output += "\n" + JsonConvert.SerializeObject(get_Capabilitystring, Formatting.Indented);
                    //return ((int)CLI_ExitCode.success, output);
                }
            }
            else if (commandLineInput.ServiceTag.Count > 0)
            {
                foreach (string tag in commandLineInput.ServiceTag)
                {
                    var tmp = _AllInfoMonitors.FindAll(x => x.edid.ServiceTag.ToUpper().Equals(tag.ToUpper()));

                    foreach (MonitorInfo monitor in tmp)
                    {
                        Get_Capabilitystring get_Capabilitystring = new Get_Capabilitystring(monitor);
                        get_Capabilitystring.Command = commandLineInput.Command;
                        //get_Capabilitystring.Model = monitor.modelName;
                        get_Capabilitystring.TargetFeature = commandLineInput.TargetFeature;
                        //get_Capabilitystring.SerialNumber = monitor.edid.SerialNumber;
                        //get_Capabilitystring.ServiceTag = monitor.edid.ServiceTag;
                        //get_Capabilitystring.Index = change_0base_to_1base((monitor.Index).ToString());
                        get_Capabilitystring.Result = "PASS";
                        get_Capabilitystring.Message = "N/A";
                        get_Capabilitystring.CapabilityString = monitor.CapabilityString;

                        output += "\n" + JsonConvert.SerializeObject(get_Capabilitystring, Formatting.Indented);
                        //return ((int)CLI_ExitCode.success, output);
                    }
                }
            }
            else if (commandLineInput.Model.Count > 0)
            {
                foreach (string modelName in commandLineInput.Model)
                {
                    var tmp = _AllInfoMonitors.FindAll(x => x.edid.ModelName.ToUpper().Equals(modelName.ToUpper()));

                    foreach (MonitorInfo monitor in tmp)
                    {
                        Get_Capabilitystring get_Capabilitystring = new Get_Capabilitystring(monitor);
                        get_Capabilitystring.Command = commandLineInput.Command;
                        //get_Capabilitystring.Model = monitor.edid.ModelName;
                        get_Capabilitystring.TargetFeature = commandLineInput.TargetFeature;
                        //get_Capabilitystring.SerialNumber = monitor.edid.SerialNumber;
                        //get_Capabilitystring.ServiceTag = monitor.edid.ServiceTag;
                        //get_Capabilitystring.Index = change_0base_to_1base((monitor.Index).ToString());
                        get_Capabilitystring.Result = "PASS";
                        get_Capabilitystring.Message = "N/A";
                        get_Capabilitystring.CapabilityString = monitor.CapabilityString;

                        output += "\n" + JsonConvert.SerializeObject(get_Capabilitystring, Formatting.Indented);
                        //return ((int)CLI_ExitCode.success, output);
                    }
                }
            }

            return ((int)CLI_ExitCode.success, output);
        }

        private (int code, string result) EnergysaverX(IDeviceManagerSA devMgr, CommandLineInput commandLineInput)
        {
            if (commandLineInput.Command == "GET" && commandLineInput.Options.Count == 0)
            {
                writelog("Energysaver get entry");
                return Energysaver(devMgr, commandLineInput).Result;
            }
            else if (commandLineInput.Command == "SET" && commandLineInput.Options.Count == 1)
            {
                writelog("Energysaver set entry");
                return Energysaver(devMgr, commandLineInput).Result;
            }
            else
            {
                CLI_RESPONSE cli_Response = new CLI_RESPONSE();
                cli_Response.Command = commandLineInput.Command;
                cli_Response.TargetFeature = commandLineInput.TargetFeature;
                cli_Response.Result = "FAIL";
                cli_Response.Message = "Invalid command line syntax, missing -value=... or more than one -value=...";
                return ((int)CLI_ExitCode.invalide_cmdline_syntax, cli_Response.ToJson());
            }
        }

        private async Task<(int code, string result)> Energysaver(IDeviceManagerSA devMgr, CommandLineInput commandLineInput)
        {
            string output = string.Empty;
            bool retcode = false;
            int somethingfail = 0;
            ObjGetVCP rc = new ObjGetVCP();

            List<int> _monitorIndeies = new List<int>();

            if (_AllInfoMonitors == null)
                _AllInfoMonitors = devMgr.GetMonitors().Result;
            _monitorIndeies = GetMonitorIndeies(commandLineInput, _AllInfoMonitors);

            foreach (int idx in _monitorIndeies)
            {
                MonitorInfo monitor = _AllInfoMonitors[idx];
                CLI_RESPONSE cli_Response = new CLI_RESPONSE(monitor);
                cli_Response.Command = commandLineInput.Command;
                cli_Response.TargetFeature = commandLineInput.TargetFeature;

                string capability = monitor.CapabilityString;
                if (capability.Contains("E0("))
                {
                    string[] ss = capability.Split("E0(");
                    ss = ss[1].Split(")");
                    ss = ss[0].Split(" ");
                    if (ss[0] == "03" || ss[0] == "0F")
                    {
                        if (commandLineInput.Command == "SET")
                        {
                            if (get_Energysaver_code(commandLineInput.Options[0].Option_Value) != "unknown_command")
                            {
                                rc = GetVCPCode(devMgr, monitor, "0xE0").Result;
                                string setvalue = (int.Parse(get_Energysaver_code(commandLineInput.Options[0].Option_Value)) | (int.Parse(rc.value.ToString()) & 0x03)).ToString();
                                //Trace.WriteLine(setvalue.ToString());
                                retcode = SetVCPCode(devMgr, monitor, "0xE0", setvalue).Result;
                                cli_Response.Value = commandLineInput.Options[0].Option_Value;
                            }
                            else
                                somethingfail |= 0x01;
                        }
                        else if (commandLineInput.Command == "GET")
                        {
                            retcode = true;
                            cli_Response.Result = "PASS";
                            cli_Response.Message = "N/A";
                            rc = GetVCPCode(devMgr, monitor, "0xE0").Result;
                            if ((Convert.ToInt32(rc.value) & 0x0c) == 0x00)
                                cli_Response.Value = "OFF";
                            else if ((Convert.ToInt32(rc.value) & 0x0c) == 0x04)
                                cli_Response.Value = "ON";
                            else if ((Convert.ToInt32(rc.value) & 0x0c) == 0x08)
                                cli_Response.Value = "OFF,LOCK";
                            else if ((Convert.ToInt32(rc.value) & 0x0c) == 0x0c)
                                cli_Response.Value = "ON,LOCK";
                        }
                    }
                }
                else
                    somethingfail |= 0x10;

                if (retcode)
                {
                    cli_Response.Result = "PASS";
                    cli_Response.Message = "N/A";
                }
                else
                {
                    cli_Response.Result = "FAIL";
                    if ((somethingfail & 0x01) == 0x01)
                        cli_Response.Message = $"Unknown value ({commandLineInput.Options[0].Option_Value})";
                    else if ((somethingfail & 0x10) == 0x10)
                        cli_Response.Message = $"No Support {commandLineInput.TargetFeature}";
                    else
                        cli_Response.Message = $"Set {commandLineInput.Options[0].Option_Value} fail";
                }
                System.Console.WriteLine(JsonConvert.SerializeObject(cli_Response, Formatting.Indented));
                output += "\n" + JsonConvert.SerializeObject(cli_Response, Formatting.Indented);
            }

            writelog($"Energysaver return exit value{output}");
            return (retcode ? (int)CLI_ExitCode.success : (int)CLI_ExitCode.functional_error, output);
        }

        private static List<int>? GetMonitorIndeies(CommandLineInput cmdLineInput, List<MonitorInfo> allMonitors)
        {
            if (allMonitors == null)
                return null;

            List<int> listOut = new List<int>();
            bool isAllMonitors = true;

            //If -ServiceTag=[{tag0}],[{tag1}],[{tag2}],... is specified
            if (cmdLineInput.ServiceTag.Count > 0)
            {
                isAllMonitors = false;
                foreach (MonitorInfo mi in allMonitors)
                {
                    if (!String.IsNullOrWhiteSpace(mi.edid.ServiceTag))
                    {
                        //Check if this monitor's service tag in in the -ServiceTag list
                        string? match = cmdLineInput.ServiceTag.FirstOrDefault(x => x.Equals(mi.edid.ServiceTag, StringComparison.OrdinalIgnoreCase));
                        if (match != null) //If found, add index value to listOut
                            listOut.Add(mi.Index);
                    }
                    //Empty ServiceTage will not be added
                }
            }

            //If -model=[{model0}],[{model1}],[{model2}],... is specified
            if (cmdLineInput.Model.Count > 0)
            {
                isAllMonitors = false;
                foreach (MonitorInfo mi in allMonitors)
                {
                    if (!String.IsNullOrWhiteSpace(mi.modelName))
                    {
                        //Check if this monitor's model name in in the -model list
                        string? match = cmdLineInput.Model.FirstOrDefault(x => x.Equals(mi.modelName, StringComparison.OrdinalIgnoreCase));
                        if (match != null) //If found, add index value to listOut
                            listOut.Add(mi.Index);
                    }
                }
            }
            //If -Index=[{idx0}],[{idx1}],[{idx2}],... is specified
            if (cmdLineInput.DeviceIndex.Count > 0)
            {
                isAllMonitors = false;
                foreach (string idxString in cmdLineInput.DeviceIndex)
                {
                    int idx;
                    if (int.TryParse(idxString, out idx) &&
                        (idx >= 0) &&
                        (idx < allMonitors.Count))
                    {
                        listOut.Add(idx);
                    }
                }
            }

            if (isAllMonitors)
            {
                foreach (MonitorInfo mi in allMonitors)
                {
                    listOut.Add(mi.Index);
                }
            }
            else
            {
                //Remove duplicated
                listOut = listOut.Distinct().ToList();
            }
            return listOut;
        }
        private static List<DeviceInfo>? GetDDeviceIndeies(CommandLineInput cmdLineInput, List<DeviceInfo> allDevices)
        {
            if (allDevices == null)
                return null;

            List<DeviceInfo> listOut = new List<DeviceInfo>();
            bool isAllDevices = true;

            //If -ServiceTag=[{tag0}],[{tag1}],[{tag2}],... is specified
            if (cmdLineInput.ServiceTag.Count > 0)
            {
                isAllDevices = false;
                foreach (DeviceInfo mi in allDevices)
                {
                    if (!String.IsNullOrWhiteSpace(mi.DockServiceTag))
                    {
                        //Check if this monitor's service tag in in the -ServiceTag list
                        string? match = cmdLineInput.ServiceTag.FirstOrDefault(x => x.Equals(mi.DockServiceTag, StringComparison.OrdinalIgnoreCase));
                        if (match != null) //If found, add index value to listOut
                            listOut.Add(mi);
                    }
                    //Empty ServiceTage will not be added
                }
            }

            //If -model=[{model0}],[{model1}],[{model2}],... is specified
            if (cmdLineInput.Model.Count > 0)
            {
                isAllDevices = false;
                foreach (DeviceInfo mi in allDevices)
                {
                    if (!String.IsNullOrWhiteSpace(mi.ModelNumber))
                    {
                        //Check if this monitor's model name in in the -model list
                        string? match = cmdLineInput.Model.FirstOrDefault(x => x.Equals(mi.ModelNumber, StringComparison.OrdinalIgnoreCase));
                        if (match != null) //If found, add index value to listOut
                            listOut.Add(mi);
                    }
                }
            }

            if (isAllDevices)
            {
                foreach (DeviceInfo mi in allDevices)
                {
                    listOut.Add(mi);
                }
            }
            else
            {
                //Remove duplicated
                listOut = listOut.Distinct().ToList();
            }
            return listOut;
        }
        private (int code, string result) AutocolorpresetX(IDeviceManagerSA devMgr, CommandLineInput commandLineInput)
        {
            if (commandLineInput.Options.Count > 2)
            {
                CLI_RESPONSE cli_Response = new CLI_RESPONSE();
                cli_Response.Command = commandLineInput.Command;
                cli_Response.TargetFeature = commandLineInput.TargetFeature;
                cli_Response.Result = "FAIL";
                cli_Response.Message = "Invalid command line syntax, missing -value=... or more than one -value=...";
                return ((int)CLI_ExitCode.invalide_cmdline_syntax, cli_Response.ToJson());
            }
            else
            {
                return Autocolorpreset(devMgr, commandLineInput).Result;
            }
        }

        private async Task<(int code, string result)> Autocolorpreset(IDeviceManagerSA devMgr, CommandLineInput commandLineInput)
        {
            CLI_RESPONSE S_Autocolorpreset_RESPONSE = new CLI_RESPONSE();
            if (_AllInfoMonitors == null)
                _AllInfoMonitors = await devMgr.GetMonitors();
            string output = string.Empty;
            bool retcode = false;
            string on_off = string.Empty;
            bool lock_unlock = true;
            string restult_onoff = string.Empty;

            writelog($"Autocolorpreset Entry");
            if (commandLineInput.Command == "SET")
            {
                if (commandLineInput.DeviceIndex.Count == 0 && commandLineInput.ServiceTag.Count == 0 && commandLineInput.Model.Count == 0)
                {
                    foreach (MonitorInfo monitor in _AllInfoMonitors)
                    {
                        S_Autocolorpreset_RESPONSE = new CLI_RESPONSE(monitor);
                        S_Autocolorpreset_RESPONSE.Command = "SET";
                        S_Autocolorpreset_RESPONSE.TargetFeature = "AUTOCOLORPRESET";

                        switch (commandLineInput.Options[0].Option_Value)
                        {
                            case "ON":
                                {
                                    on_off = "on";
                                    lock_unlock = false;
                                    writelog($"Autocolorpreset ON Entry");
                                    devMgr.AutoSetColorPresetForMonitorConfig(monitor, on_off, lock_unlock);
                                    writelog($"Autocolorpreset ON Exit");
                                    retcode = true;
                                    S_Autocolorpreset_RESPONSE.Value = "ON";
                                    S_Autocolorpreset_RESPONSE.Result = "PASS";
                                    break;
                                }
                            case "OFF":
                                {
                                    on_off = "off";
                                    lock_unlock = false;
                                    writelog($"Autocolorpreset OFF Entry");
                                    devMgr.AutoSetColorPresetForMonitorConfig(monitor, on_off, lock_unlock);
                                    writelog($"Autocolorpreset OFF Exit");
                                    retcode = true;
                                    S_Autocolorpreset_RESPONSE.Value = "OFF";
                                    S_Autocolorpreset_RESPONSE.Result = "PASS";
                                    break;
                                }
                            default:
                                {
                                    writelog($"Autocolorpreset default Entry");
                                    S_Autocolorpreset_RESPONSE.Command = "SET";
                                    S_Autocolorpreset_RESPONSE.TargetFeature = "AUTOCOLORPRESET";
                                    S_Autocolorpreset_RESPONSE.Result = "FAIL";
                                    S_Autocolorpreset_RESPONSE.Message = "Invalid command line syntax, missing -value=... or more than one -value=...";
                                    retcode = false;
                                    break;
                                }
                        }
                        output += "\n" + JsonConvert.SerializeObject(S_Autocolorpreset_RESPONSE, Formatting.Indented);
                        writelog($"Autocolorpreset exit return value: {output}");
                    }
                }
                else
                {
                    foreach (string idx in commandLineInput.DeviceIndex)
                    {
                        MonitorInfo monitor = _AllInfoMonitors[Convert.ToInt32(idx)];
                        S_Autocolorpreset_RESPONSE = new CLI_RESPONSE(monitor);
                        S_Autocolorpreset_RESPONSE.Command = "SET";
                        S_Autocolorpreset_RESPONSE.TargetFeature = "AUTOCOLORPRESET";

                        switch (commandLineInput.Options[0].Option_Value)
                        {
                            case "ON":
                                {
                                    on_off = "on";
                                    lock_unlock = false;
                                    writelog($"Autocolorpreset ON Entry");
                                    devMgr.AutoSetColorPresetForMonitorConfig(monitor, on_off, lock_unlock);
                                    writelog($"Autocolorpreset ON Exit");
                                    retcode = true;
                                    S_Autocolorpreset_RESPONSE.Value = "ON";
                                    S_Autocolorpreset_RESPONSE.Result = "PASS";
                                    break;
                                }
                            case "OFF":
                                {
                                    on_off = "off";
                                    lock_unlock = false;
                                    writelog($"Autocolorpreset OFF Entry");
                                    devMgr.AutoSetColorPresetForMonitorConfig(monitor, on_off, lock_unlock);
                                    writelog($"Autocolorpreset OFF Exit");
                                    retcode = true;
                                    S_Autocolorpreset_RESPONSE.Value = "OFF";
                                    S_Autocolorpreset_RESPONSE.Result = "PASS";
                                    break;
                                }
                            default:
                                {
                                    writelog($"Autocolorpreset default Entry");
                                    S_Autocolorpreset_RESPONSE.Command = "SET";
                                    S_Autocolorpreset_RESPONSE.TargetFeature = "AUTOCOLORPRESET";
                                    S_Autocolorpreset_RESPONSE.Result = "FAIL";
                                    S_Autocolorpreset_RESPONSE.Message = "Invalid command line syntax, missing -value=... or more than one -value=...";
                                    retcode = false;
                                    break;
                                }
                        }
                        output += "\n" + JsonConvert.SerializeObject(S_Autocolorpreset_RESPONSE, Formatting.Indented);
                        writelog($"Autocolorpreset idx exit return value: {output}");
                    }
                    foreach (string tag in commandLineInput.ServiceTag)
                    {
                        var tmp = _AllInfoMonitors.FindAll(x => x.edid.ServiceTag.ToUpper().Equals(tag.ToUpper()));
                        foreach (MonitorInfo monitor in tmp)
                        {
                            S_Autocolorpreset_RESPONSE = new CLI_RESPONSE(monitor);

                            S_Autocolorpreset_RESPONSE.Command = "SET";
                            S_Autocolorpreset_RESPONSE.TargetFeature = "AUTOCOLORPRESET";

                            switch (commandLineInput.Options[0].Option_Value)
                            {
                                case "ON":
                                    {
                                        on_off = "on";
                                        lock_unlock = false;
                                        writelog($"Autocolorpreset ON Entry");
                                        devMgr.AutoSetColorPresetForMonitorConfig(monitor, on_off, lock_unlock);
                                        writelog($"Autocolorpreset ON Exit");
                                        retcode = true;
                                        S_Autocolorpreset_RESPONSE.Value = "ON";
                                        S_Autocolorpreset_RESPONSE.Result = "PASS";
                                        break;
                                    }
                                case "OFF":
                                    {
                                        on_off = "off";
                                        lock_unlock = false;
                                        writelog($"Autocolorpreset OFF Entry");
                                        devMgr.AutoSetColorPresetForMonitorConfig(monitor, on_off, lock_unlock);
                                        writelog($"Autocolorpreset OFF Exit");
                                        retcode = true;
                                        S_Autocolorpreset_RESPONSE.Value = "OFF";
                                        S_Autocolorpreset_RESPONSE.Result = "PASS";
                                        break;
                                    }
                                default:
                                    {
                                        writelog($"Autocolorpreset default Entry");
                                        S_Autocolorpreset_RESPONSE.Command = "SET";
                                        S_Autocolorpreset_RESPONSE.TargetFeature = "AUTOCOLORPRESET";
                                        S_Autocolorpreset_RESPONSE.Result = "FAIL";
                                        S_Autocolorpreset_RESPONSE.Message = "Invalid command line syntax, missing -value=... or more than one -value=...";
                                        retcode = false;
                                        break;
                                    }
                            }
                            output += "\n" + JsonConvert.SerializeObject(S_Autocolorpreset_RESPONSE, Formatting.Indented);
                            writelog($"Autocolorpreset tag exit return value: {output}");
                        }
                    }
                    foreach (string modelName in commandLineInput.Model)
                    {
                        var tmp = _AllInfoMonitors.FindAll(x => x.edid.ModelName.ToUpper().Equals(modelName.ToUpper()));
                        foreach (MonitorInfo monitor in tmp)
                        {
                            S_Autocolorpreset_RESPONSE = new CLI_RESPONSE(monitor);

                            S_Autocolorpreset_RESPONSE.Command = "SET";
                            S_Autocolorpreset_RESPONSE.TargetFeature = "AUTOCOLORPRESET";

                            switch (commandLineInput.Options[0].Option_Value)
                            {
                                case "ON":
                                    {
                                        on_off = "on";
                                        lock_unlock = false;
                                        writelog($"Autocolorpreset ON Entry");
                                        devMgr.AutoSetColorPresetForMonitorConfig(monitor, on_off, lock_unlock);
                                        writelog($"Autocolorpreset ON Exit");
                                        retcode = true;
                                        S_Autocolorpreset_RESPONSE.Value = "ON";
                                        S_Autocolorpreset_RESPONSE.Result = "PASS";
                                        break;
                                    }
                                case "OFF":
                                    {
                                        on_off = "off";
                                        lock_unlock = false;
                                        writelog($"Autocolorpreset OFF Entry");
                                        devMgr.AutoSetColorPresetForMonitorConfig(monitor, on_off, lock_unlock);
                                        writelog($"Autocolorpreset OFF Exit");
                                        retcode = true;
                                        S_Autocolorpreset_RESPONSE.Value = "OFF";
                                        S_Autocolorpreset_RESPONSE.Result = "PASS";
                                        break;
                                    }
                                default:
                                    {
                                        writelog($"Autocolorpreset default Entry");
                                        S_Autocolorpreset_RESPONSE.Command = "SET";
                                        S_Autocolorpreset_RESPONSE.TargetFeature = "AUTOCOLORPRESET";
                                        S_Autocolorpreset_RESPONSE.Result = "FAIL";
                                        S_Autocolorpreset_RESPONSE.Message = "Invalid command line syntax, missing -value=... or more than one -value=...";
                                        retcode = false;
                                        break;
                                    }
                            }
                            output += "\n" + JsonConvert.SerializeObject(S_Autocolorpreset_RESPONSE, Formatting.Indented);
                            writelog($"Autocolorpreset tag exit return value: {output}");
                        }
                    }
                }
            }
            else if (commandLineInput.Command == "GET" && commandLineInput.Options.Count == 0)
            {
                if (commandLineInput.DeviceIndex.Count == 0 && commandLineInput.ServiceTag.Count == 0 && commandLineInput.Model.Count == 0)
                {
                    foreach (MonitorInfo monitor in _AllInfoMonitors)
                    {
                        S_Autocolorpreset_RESPONSE = new CLI_RESPONSE(monitor);
                        S_Autocolorpreset_RESPONSE.Command = "SET";
                        S_Autocolorpreset_RESPONSE.TargetFeature = "AUTOCOLORPRESET";

                        writelog($"Autocolorpreset GET Entry");
                        restult_onoff = devMgr.GetAutoColorPresetStatus(monitor).Result.ToString();
                        writelog($"Autocolorpreset GET Exit");
                        Trace.WriteLine("restult_onoff:", restult_onoff);
                        if (restult_onoff == "ON" || restult_onoff == "OFF")
                        {
                            retcode = true;
                            S_Autocolorpreset_RESPONSE.Value = restult_onoff;
                            S_Autocolorpreset_RESPONSE.Result = "PASS";
                        }
                        else
                        {
                            writelog($"Autocolorpreset fail");
                            S_Autocolorpreset_RESPONSE.Command = "SET";
                            S_Autocolorpreset_RESPONSE.TargetFeature = "AUTOCOLORPRESET";
                            S_Autocolorpreset_RESPONSE.Result = "FAIL";
                            S_Autocolorpreset_RESPONSE.Message = "Invalid command line syntax, missing -value=... or more than one -value=...";
                            retcode = false;
                        }
                        output += "\n" + JsonConvert.SerializeObject(S_Autocolorpreset_RESPONSE, Formatting.Indented);
                        writelog($"Autocolorpreset exit return value: {output}");
                    }
                }
                else
                {
                    foreach (string idx in commandLineInput.DeviceIndex)
                    {
                        MonitorInfo monitor = _AllInfoMonitors[Convert.ToInt32(idx)];
                        S_Autocolorpreset_RESPONSE = new CLI_RESPONSE(monitor);
                        S_Autocolorpreset_RESPONSE.Command = "SET";
                        S_Autocolorpreset_RESPONSE.TargetFeature = "AUTOCOLORPRESET";

                        writelog($"Autocolorpreset GET idx Entry");
                        restult_onoff = devMgr.GetAutoColorPresetStatus(monitor).Result.ToString();
                        writelog($"Autocolorpreset GET idx Exit");
                        Trace.WriteLine("restult_onoff:", restult_onoff);
                        if (restult_onoff == "ON" || restult_onoff == "OFF")
                        {
                            retcode = true;
                            S_Autocolorpreset_RESPONSE.Value = restult_onoff;
                            S_Autocolorpreset_RESPONSE.Result = "PASS";
                        }
                        else
                        {
                            writelog($"Autocolorpreset fail");
                            S_Autocolorpreset_RESPONSE.Command = "SET";
                            S_Autocolorpreset_RESPONSE.TargetFeature = "AUTOCOLORPRESET";
                            S_Autocolorpreset_RESPONSE.Result = "FAIL";
                            S_Autocolorpreset_RESPONSE.Message = "Invalid command line syntax, missing -value=... or more than one -value=...";
                            retcode = false;
                        }
                        output += "\n" + JsonConvert.SerializeObject(S_Autocolorpreset_RESPONSE, Formatting.Indented);
                        writelog($"Autocolorpreset idx exit return value: {output}");
                    }
                    foreach (string tag in commandLineInput.ServiceTag)
                    {
                        var tmp = _AllInfoMonitors.FindAll(x => x.edid.ServiceTag.ToUpper().Equals(tag.ToUpper()));
                        foreach (MonitorInfo monitor in tmp)
                        {
                            S_Autocolorpreset_RESPONSE = new CLI_RESPONSE(monitor);

                            S_Autocolorpreset_RESPONSE.Command = "SET";
                            S_Autocolorpreset_RESPONSE.TargetFeature = "AUTOCOLORPRESET";

                            writelog($"Autocolorpreset GET Entry");
                            restult_onoff = devMgr.GetAutoColorPresetStatus(monitor).Result.ToString();
                            writelog($"Autocolorpreset GET Exit");
                            Trace.WriteLine("restult_onoff:", restult_onoff);
                            if (restult_onoff == "ON" || restult_onoff == "OFF")
                            {
                                retcode = true;
                                S_Autocolorpreset_RESPONSE.Value = restult_onoff;
                                S_Autocolorpreset_RESPONSE.Result = "PASS";
                            }
                            else
                            {
                                writelog($"Autocolorpreset fail");
                                S_Autocolorpreset_RESPONSE.Command = "SET";
                                S_Autocolorpreset_RESPONSE.TargetFeature = "AUTOCOLORPRESET";
                                S_Autocolorpreset_RESPONSE.Result = "FAIL";
                                S_Autocolorpreset_RESPONSE.Message = "Invalid command line syntax, missing -value=... or more than one -value=...";
                                retcode = false;
                            }
                            output += "\n" + JsonConvert.SerializeObject(S_Autocolorpreset_RESPONSE, Formatting.Indented);
                            writelog($"Autocolorpreset tag exit return value: {output}");
                        }
                    }
                    foreach (string modelName in commandLineInput.Model)
                    {
                        var tmp = _AllInfoMonitors.FindAll(x => x.edid.ModelName.ToUpper().Equals(modelName.ToUpper()));
                        foreach (MonitorInfo monitor in tmp)
                        {
                            S_Autocolorpreset_RESPONSE = new CLI_RESPONSE(monitor);

                            S_Autocolorpreset_RESPONSE.Command = "SET";
                            S_Autocolorpreset_RESPONSE.TargetFeature = "AUTOCOLORPRESET";

                            writelog($"Autocolorpreset GET Entry");
                            restult_onoff = devMgr.GetAutoColorPresetStatus(monitor).Result.ToString();
                            writelog($"Autocolorpreset GET Exit");
                            Trace.WriteLine("restult_onoff:", restult_onoff);
                            if (restult_onoff == "ON" || restult_onoff == "OFF")
                            {
                                retcode = true;
                                S_Autocolorpreset_RESPONSE.Value = restult_onoff;
                                S_Autocolorpreset_RESPONSE.Result = "PASS";
                            }
                            else
                            {
                                writelog($"Autocolorpreset fail");
                                S_Autocolorpreset_RESPONSE.Command = "SET";
                                S_Autocolorpreset_RESPONSE.TargetFeature = "AUTOCOLORPRESET";
                                S_Autocolorpreset_RESPONSE.Result = "FAIL";
                                S_Autocolorpreset_RESPONSE.Message = "Invalid command line syntax, missing -value=... or more than one -value=...";
                                retcode = false;
                            }
                            output += "\n" + JsonConvert.SerializeObject(S_Autocolorpreset_RESPONSE, Formatting.Indented);
                            writelog($"Autocolorpreset tag exit return value: {output}");
                        }
                    }
                }
            }
            else
            {
                S_Autocolorpreset_RESPONSE.Command = "SET";
                S_Autocolorpreset_RESPONSE.TargetFeature = "AUTOCOLORPRESET";
                S_Autocolorpreset_RESPONSE.Result = "FAIL";
                S_Autocolorpreset_RESPONSE.Message = "Invalid command line syntax, missing -value=... or more than one -value=...";
                output += "\n" + JsonConvert.SerializeObject(S_Autocolorpreset_RESPONSE, Formatting.Indented);
            }
            writelog($"Autocolorpreset exit return value: {output}");
            return ((int)CLI_ExitCode.success, output);
        }

        private (int code, string result) OSDLanguageX(IDeviceManagerSA devMgr, CommandLineInput commandLineInput)
        {
            if (commandLineInput.Command == "GET" && commandLineInput.Options.Count == 0)
            {
                return OSDLanguage(devMgr, commandLineInput).Result;
            }
            else if (commandLineInput.Command == "SET" && commandLineInput.Options.Count == 1)
            {
                if (GetOSDLanguage_index(commandLineInput.Options[0].Option_Value).ToString() == "255")
                {
                    CLI_RESPONSE cli_Response = new CLI_RESPONSE();
                    cli_Response.Command = commandLineInput.Command;
                    cli_Response.TargetFeature = commandLineInput.TargetFeature;
                    cli_Response.Result = "FAIL";
                    cli_Response.Message = "Invalid command line syntax, wrong OSDLanguage.";
                    return ((int)CLI_ExitCode.invalide_cmdline_syntax, cli_Response.ToJson());
                }
                else
                    return OSDLanguage(devMgr, commandLineInput).Result;
            }
            else
            {
                if (commandLineInput.Command == "SET" && (commandLineInput.Options == null || commandLineInput.Options.Count > 1))
                {
                    CLI_RESPONSE cli_Response = new CLI_RESPONSE();
                    cli_Response.Command = commandLineInput.Command;
                    cli_Response.TargetFeature = commandLineInput.TargetFeature;
                    cli_Response.Result = "FAIL";
                    cli_Response.Message = "Invalid command line syntax, missing -value=OSDLanguage or more than one -value=....";
                    return ((int)CLI_ExitCode.invalide_cmdline_syntax, cli_Response.ToJson());
                }
                else
                {
                    CLI_RESPONSE cli_Response = new CLI_RESPONSE();
                    cli_Response.Command = commandLineInput.Command;
                    cli_Response.TargetFeature = commandLineInput.TargetFeature;
                    cli_Response.Result = "FAIL";
                    cli_Response.Message = "Invalid command line syntax.";
                    return ((int)CLI_ExitCode.invalide_cmdline_syntax, cli_Response.ToJson());
                }
            }
        }

        private async Task<(int code, string result)> OSDLanguage(IDeviceManagerSA devMgr, CommandLineInput commandLineInput)
        {
            ObjGetVCP rc = new ObjGetVCP();
            bool retcode = false;
            string output = string.Empty;
            string capability = string.Empty;
            bool in_support_languages = false;

            List<int> _monitorIndeies = new List<int>();

            if (_AllInfoMonitors == null)
                _AllInfoMonitors = devMgr.GetMonitors().Result;
            _monitorIndeies = GetMonitorIndeies(commandLineInput, _AllInfoMonitors);

            foreach (int idx in _monitorIndeies)
            {
                writelog($"OSDLanguage entry");
                MonitorInfo monitor = _AllInfoMonitors[idx];
                CLI_RESPONSE cli_Response = new CLI_RESPONSE(monitor);
                cli_Response.Command = commandLineInput.Command;
                cli_Response.TargetFeature = commandLineInput.TargetFeature;

                capability = await devMgr.GetVCPCapabilities(monitor);
                var item = (JObject)JsonConvert.DeserializeObject(capability);
                if (item.ContainsKey("CapsDataMap"))
                {
                    if (commandLineInput.Command == "SET")
                    {
                        var item2 = (JObject)item["CapsDataMap"];
                        if (item2.ContainsKey("On Screen Display Language"))
                        {
                            var support_languages = (JArray)item2["On Screen Display Language"];
                            support_languages.ToObject<List<string>>().ToArray();
                            // portuguese change to brazilianportuguese
                            if (commandLineInput.Options[0].Option_Value.ToLower() == "portuguese" || commandLineInput.Options[0].Option_Value.ToLower() == "brazilianportuguese")
                            {
                                var portugueseTemp = support_languages.ToObject<List<string>>().Find(_ => _.ToLower() == "portuguese");

                                commandLineInput.Options[0].Option_Value = portugueseTemp is null ? "BRAZILIANPORTUGUESE" : "PORTUGUESE";
                            }
                            foreach (var support_language in support_languages)
                            {
                                writelog($"OSDLanguage SET entry");
                                if (get_language(GetOSDLanguage_index(commandLineInput.Options[0].Option_Value.ToLower()).ToString()).ToLower() == support_language.ToString().ToLower())
                                {
                                    in_support_languages = true;
                                    retcode = SetVCPCode(devMgr, monitor, "0xCC", GetOSDLanguage_index(commandLineInput.Options[0].Option_Value.ToLower()).ToString()).Result;
                                    cli_Response.Value = commandLineInput.Options[0].Option_Value;
                                }
                            }
                        }
                    }
                    else if (commandLineInput.Command == "GET")
                    {
                        writelog($"OSDLanguage GET entry");
                        rc = GetVCPCode(devMgr, monitor, "0xCC").Result;
                        cli_Response.Value = get_language(rc.value.ToString());
                        retcode = true;
                    }
                }

                if (retcode)
                {
                    cli_Response.Result = "PASS";
                }
                else
                {
                    cli_Response.Result = "FAIL";
                    if (!in_support_languages)
                        cli_Response.Message = $"No support this OSD language :{commandLineInput.Options[0].Option_Value}";
                    else
                        cli_Response.Message = "FAIL VCP";
                }

                System.Console.WriteLine(JsonConvert.SerializeObject(cli_Response, Formatting.Indented));
                output += "\n" + JsonConvert.SerializeObject(cli_Response, Formatting.Indented);
            }
            writelog($"OSDLanguage exit return value {output}");
            return (retcode ? (int)CLI_ExitCode.success : (int)CLI_ExitCode.functional_error, output);
        }

        private (int code, string result) GetMonitorCount(IDeviceManagerSA devMgr, CommandLineInput commandLineInput)
        {
            if (commandLineInput.Command == "SET" || commandLineInput.Options.Count != 0)
            {
                CLI_RESPONSE cli_Response = new CLI_RESPONSE();
                cli_Response.Command = commandLineInput.Command;
                cli_Response.TargetFeature = commandLineInput.TargetFeature;
                cli_Response.Result = "FAIL";
                cli_Response.Message = "Invalid command line syntax.";
                return ((int)CLI_ExitCode.invalide_cmdline_syntax, cli_Response.ToJson());
            }
            else
            {
                return MonitorCounts(devMgr, commandLineInput).Result;
            }
        }

        private async Task<(int code, string result)> MonitorCounts(IDeviceManagerSA devMgr, CommandLineInput commandLineInput)
        {
            writelog($"MonitorCounts default Entry");
            CLI_RESPONSE cli_Response = new CLI_RESPONSE();

            if (_AllInfoMonitors == null)
                _AllInfoMonitors = await devMgr.GetMonitors();

            cli_Response.Command = commandLineInput.Command;
            cli_Response.TargetFeature = commandLineInput.TargetFeature;
            cli_Response.Result = "PASS";

            string output = string.Empty;

            output += "{\n";
            output += $"  \"TotalMonitors\"： \"{_AllInfoMonitors.Count}\", \n";

            foreach (MonitorInfo monitor in _AllInfoMonitors)
            {
                if ((monitor.Index + 1) == _AllInfoMonitors.Count)
                    output += $"  \"Index_{change_0base_to_1base((monitor.Index).ToString())}\": \"{monitor.modelName}\",\"{monitor.edid.ServiceTag}\"";
                else
                    output += $"  \"Index_{change_0base_to_1base((monitor.Index).ToString())}\": \"{monitor.modelName}\",\"{monitor.edid.ServiceTag}\",\n";
            }
            output += "\n}";
            output += "\n" + JsonConvert.SerializeObject(cli_Response, Formatting.Indented);

            writelog($"MonitorCounts exit return value:{output}");
            return ((int)CLI_ExitCode.success, output);
        }

        private static int GetOSDLanguage_index(String language)
        {
            switch (language.ToLower())
            {
                case "chinese": return 0x01;
                case "english": return 0x02;
                case "francais": return 0x03;
                case "french": return 0x03;
                case "german": return 0x04;
                case "deutschi": return 0x04;
                case "italian": return 0x05;
                case "japanese": return 0x06;
                case "japan": return 0x06;
                case "korean": return 0x07;
                case "portuguese": return 0x08;
                case "russian": return 0x09;
                case "espanol": return 0x0a;
                case "spanish": return 0x0a;
                case "swedish": return 0x0b;
                case "turkish": return 0x0c;
                case "chinese_s": return 0x0d;
                case "chinese-simplified": return 0x0d;
                case "brazilianportuguese": return 0x0e;
                case "arabic": return 0x0f;
                case "bulgarian": return 0x10;
                case "croatian": return 0x11;
                case "czech": return 0x12;
                case "danish": return 0x13;
                case "dutch": return 0x14;
                case "estonian": return 0x15;
                case "finnish": return 0x16;
                case "greek": return 0x17;
                case "hebrew": return 0x18;
                case "hindi": return 0x19;
                case "hungarian": return 0x1a;
                case "latvian": return 0x1b;
                case "lithuanian": return 0x1c;
                case "norwegian": return 0x1d;
                case "polish": return 0x1e;
                case "romanian": return 0x1f;
                case "serbian": return 0x20;
                case "slovak": return 0x21;
                case "slovenian": return 0x22;
                case "thai": return 0x23;
                case "ukrainian": return 0x24;
                case "vietnamese": return 0x25;
                default: return 0xff;
            }
        }

        private static string modify_Manufactur(string temp)
        {
            if (temp.ToUpper() == "DEL")
                return "Dell";
            if (temp.ToUpper() == "AW")
                return "Alienware";
            return temp;
        }

        private (int code, string result) GetDiagnosticReport(IDeviceManagerSA devMgr, CommandLineInput commandLineInput)
        {
            if (commandLineInput.Command == "SET")
            {
                CLI_RESPONSE cli_Response = new CLI_RESPONSE();
                cli_Response.Command = commandLineInput.Command;
                cli_Response.TargetFeature = commandLineInput.TargetFeature;
                cli_Response.Result = "FAIL";
                cli_Response.Message = "Invalid command line syntax or missing -value=file";
                return ((int)CLI_ExitCode.invalide_cmdline_syntax, cli_Response.ToJson());
            }
            else if (commandLineInput.Command == "GET" && commandLineInput.Options.Count <= 1)
            {
                return DiagnosticReportv2(devMgr, commandLineInput).Result;
            }
            else
            {
                CLI_RESPONSE cli_Response = new CLI_RESPONSE();
                cli_Response.Command = commandLineInput.Command;
                cli_Response.TargetFeature = commandLineInput.TargetFeature;
                cli_Response.Result = "FAIL";
                cli_Response.Message = "Invalid command line syntax.";
                return ((int)CLI_ExitCode.invalide_cmdline_syntax, cli_Response.ToJson());
                //if (commandLineInput.Options.Count == 1)
                //{
                //    return DiagnosticReportv2(devMgr, commandLineInput).Result;
                //}
                //else
                //{
                //    CLI_RESPONSE cli_Response = new CLI_RESPONSE();
                //    cli_Response.Command = commandLineInput.Command;
                //    cli_Response.TargetFeature = commandLineInput.TargetFeature;
                //    cli_Response.Result = "FAIL";
                //    cli_Response.Message = "Invalid command line syntax.";
                //    return ((int)CLI_ExitCode.invalide_cmdline_syntax, cli_Response.ToJson());
                //}
            }
        }

        private async Task<(int code, string result)> DiagnosticReport(IDeviceManagerSA devMgr, CommandLineInput commandLineInput)
        {
            string output = string.Empty;

            StreamReader r = default;

            string jsonString = string.Empty;
            try
            {
                r = new StreamReader(commandLineInput.Options[0].Option_Value);
                jsonString = r.ReadToEnd();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DiagnosticReport] StreamReader read failed, message: {ex.Message}");
            }
            finally
            {
                r.Close();
            }


            if (_AllInfoMonitors == null)
                _AllInfoMonitors = await devMgr.GetMonitors();

            if (commandLineInput.DeviceIndex.Count == 0 && commandLineInput.ServiceTag.Count == 0 && commandLineInput.Model.Count == 0)
            {
                foreach (MonitorInfo monitor in _AllInfoMonitors)
                {
                    CLI_RESPONSE cli_Response = new CLI_RESPONSE(monitor);
                    cli_Response.Command = commandLineInput.Command;
                    cli_Response.TargetFeature = commandLineInput.TargetFeature;

                    cli_Response.Result = "PASS";
                    cli_Response.Message = "N/A";
                    output += "\n" + "{" + "\n" + jsonString + "\n" + "}";
                    output += "\n" + JsonConvert.SerializeObject(cli_Response, Formatting.Indented);
                }
            }
            else
            {
                foreach (string idx in commandLineInput.DeviceIndex)
                {
                    MonitorInfo monitor = _AllInfoMonitors[Convert.ToInt32(idx)];
                    CLI_RESPONSE cli_Response = new CLI_RESPONSE(monitor);
                    cli_Response.Command = commandLineInput.Command;
                    cli_Response.TargetFeature = commandLineInput.TargetFeature;

                    cli_Response.Result = "PASS";
                    cli_Response.Message = "N/A";
                    output += "\n" + "{" + "\n" + jsonString + "\n" + "}";
                    output += "\n" + JsonConvert.SerializeObject(cli_Response, Formatting.Indented);
                }
                foreach (string tag in commandLineInput.ServiceTag)
                {
                    var tmp = _AllInfoMonitors.FindAll(x => x.edid.ServiceTag.ToUpper().Equals(tag.ToUpper()));
                    foreach (MonitorInfo monitor in tmp)
                    {
                        CLI_RESPONSE cli_Response = new CLI_RESPONSE(monitor);
                        cli_Response.Command = commandLineInput.Command;
                        cli_Response.TargetFeature = commandLineInput.TargetFeature;

                        cli_Response.Result = "PASS";
                        cli_Response.Message = "N/A";
                        output += "\n" + "{" + "\n" + jsonString + "\n" + "}";
                        output += "\n" + JsonConvert.SerializeObject(cli_Response, Formatting.Indented);
                    }
                }
                foreach (string modelName in commandLineInput.Model)
                {
                    var tmp = _AllInfoMonitors.FindAll(x => x.edid.ModelName.ToUpper().Equals(modelName.ToUpper()));
                    foreach (MonitorInfo monitor in tmp)
                    {
                        CLI_RESPONSE cli_Response = new CLI_RESPONSE(monitor);
                        cli_Response.Command = commandLineInput.Command;
                        cli_Response.TargetFeature = commandLineInput.TargetFeature;

                        cli_Response.Result = "PASS";
                        cli_Response.Message = "N/A";
                        output += "\n" + "{" + "\n" + jsonString + "\n" + "}";
                        output += "\n" + JsonConvert.SerializeObject(cli_Response, Formatting.Indented);
                    }
                }
            }
            return ((int)CLI_ExitCode.success, output);
        }

        private async Task<(int code, string result)> DiagnosticReportv2(IDeviceManagerSA devMgr, CommandLineInput commandLineInput)
        {
            writelog("DiagnosticReportv2 entry");
            string output = string.Empty;
            string now = $"{DateTime.Now:yyyyMMdd_HHmmss}";
            string filepath = @$"C:\Temp\Log_{now}"; // 1. 不帶路徑, Path + 檔名 =  @$"C:\Temp\Log_{DateTime.Now:yyyyMMdd_HHmmss}"
            if (commandLineInput.Options.Count == 1)
            {
                writelog("DiagnosticReportv2 option entry");
                var inputPath = commandLineInput.Options[0].Option_Value.Replace("/", @"\");

                if (inputPath.EndsWith(@"\")) // 2. 帶路徑, 以斜線結尾, Path + 檔名 = 路徑 + $"Log_{DateTime.Now:yyyyMMdd_HHmmss}"
                {
                    writelog("DiagnosticReportv2 option is endswith slash");
                    inputPath += $"Log_{now}";
                }
                else if (!inputPath.EndsWith(".zip", StringComparison.OrdinalIgnoreCase) && Directory.Exists(inputPath)) // 3. 帶路徑, 不是以斜線或.zip結尾, 需檢查資料夾是否存在, 若存在, Path = 路徑 + @$"\Log_{DateTime.Now:yyyyMMdd_HHmmss}"
                {
                    writelog("DiagnosticReportv2 option is not endswith .zip and directory is exist");
                    inputPath += @$"\Log_{now}";
                }
                else if (inputPath.EndsWith(".zip", StringComparison.OrdinalIgnoreCase)) // 4. 帶路徑, 以.zip結尾, Path + 檔名 = 路徑[..^4]
                {
                    writelog("DiagnosticReportv2 option is endswith .zip");
                    inputPath = inputPath[..^4];
                }
                filepath = inputPath; // 此時格式為 路徑 + 檔名 (不包含.zip)
            }

            CLI_RESPONSE cli_Response = new CLI_RESPONSE();
            cli_Response.Command = commandLineInput.Command;
            cli_Response.TargetFeature = commandLineInput.TargetFeature;
            cli_Response.Result = "PASS";

            writelog("DiagnosticReportv2 devMgr.SaveLogFile entry");
            if (!devMgr.SaveLogFile(filepath).Result)
            {
                writelog("DiagnosticReportv2 devMgr.SaveLogFile fail");
                cli_Response.Result = "FAIL";
                cli_Response.Message = "Collect files to save report failed.";
                output += "\n" + cli_Response.ToJson();
                return ((int)CLI_ExitCode.Diagnostic_Report_fail, output);
            }
            writelog("DiagnosticReportv2 devMgr.SaveLogFile success exit");

            if (!File.Exists($"{filepath}.zip"))
            {
                writelog("DiagnosticReportv2 log file exist check fail");
                cli_Response.Result = "FAIL";
                cli_Response.Message = "Collect files to save report failed, log file is not exist.";
                output += "\n" + JsonConvert.SerializeObject(cli_Response, Formatting.Indented);
                return ((int)CLI_ExitCode.Diagnostic_Report_fail, output);
            }

            cli_Response.Message = $"Log path: {filepath}.zip";
            output += "\n" + cli_Response.ToJson();

            return ((int)CLI_ExitCode.success, output);
        }

        private (int code, string result) ActiveHours(IDeviceManagerSA devMgr, CommandLineInput commandLineInput)
        {
            if (commandLineInput.Command == "SET" || commandLineInput.Options.Count != 0)
            {
                CLI_RESPONSE cli_Response = new CLI_RESPONSE();
                cli_Response.Command = commandLineInput.Command;
                cli_Response.TargetFeature = commandLineInput.TargetFeature;
                cli_Response.Result = "FAIL";
                cli_Response.Message = "Invalid command line syntax.";
                writelog(cli_Response.ToJson());
                return ((int)CLI_ExitCode.invalide_cmdline_syntax, cli_Response.ToJson());
            }
            else
            {
                return ActiveHoursX(devMgr, commandLineInput).Result;
            }
        }

        private async Task<(int code, string result)> ActiveHoursX(IDeviceManagerSA devMgr, CommandLineInput commandLineInput)
        {
            string output = string.Empty;
            ObjGetVCP rc = new ObjGetVCP();

            if (_AllInfoMonitors == null)
                _AllInfoMonitors = await devMgr.GetMonitors();

            if (commandLineInput.DeviceIndex.Count == 0 && commandLineInput.ServiceTag.Count == 0 && commandLineInput.Model.Count == 0)
            {
                writelog("ActiveHours get entry");
                foreach (MonitorInfo monitor in _AllInfoMonitors)
                {
                    CLI_RESPONSE cli_Response = new CLI_RESPONSE(monitor);
                    cli_Response.Command = commandLineInput.Command;
                    cli_Response.TargetFeature = commandLineInput.TargetFeature;

                    //if (!monitor.CapabilityDic.ContainsKey("C0"))
                    //{
                    //    cli_Response.Result = "FAIL";
                    //    cli_Response.Message = "Un - supported Feature";
                    //}
                    //else
                    //{
                    //    rc = GetVCPCode(devMgr, monitor, "0xC0").Result;
                    //    cli_Response.Value = (rc.value).ToString() + " hours";

                    //    cli_Response.Result = "PASS";
                    //    cli_Response.Message = "N/A";
                    //}
                    rc = GetVCPCode(devMgr, monitor, "0xC0").Result;
                    cli_Response.Value = (rc.value).ToString() + " hours";

                    cli_Response.Result = "PASS";
                    cli_Response.Message = "N/A";
                    output += "\n" + JsonConvert.SerializeObject(cli_Response, Formatting.Indented);
                }
            }
            else
            {
                writelog("ActiveHours get entry");
                foreach (string idx in commandLineInput.DeviceIndex)
                {
                    MonitorInfo monitor = _AllInfoMonitors[Convert.ToInt32(idx)];
                    CLI_RESPONSE cli_Response = new CLI_RESPONSE(monitor);
                    cli_Response.Command = commandLineInput.Command;
                    cli_Response.TargetFeature = commandLineInput.TargetFeature;

                    //if (!monitor.CapabilityDic.ContainsKey("C0"))
                    //{
                    //    cli_Response.Result = "FAIL";
                    //    cli_Response.Message = "Un - supported Feature";
                    //}
                    //else
                    //{
                    //    rc = GetVCPCode(devMgr, monitor, "0xC0").Result;
                    //    cli_Response.Value = (rc.value).ToString() + " hours";

                    //    cli_Response.Result = "PASS";
                    //    cli_Response.Message = "N/A";
                    //}
                    rc = GetVCPCode(devMgr, monitor, "0xC0").Result;
                    cli_Response.Value = (rc.value).ToString() + " hours";

                    cli_Response.Result = "PASS";
                    cli_Response.Message = "N/A";
                    output += "\n" + JsonConvert.SerializeObject(cli_Response, Formatting.Indented);
                }
                foreach (string tag in commandLineInput.ServiceTag)
                {
                    var tmp = _AllInfoMonitors.FindAll(x => x.edid.ServiceTag.ToUpper().Equals(tag.ToUpper()));
                    writelog("ActiveHours get entry");
                    foreach (MonitorInfo monitor in tmp)
                    {
                        CLI_RESPONSE cli_Response = new CLI_RESPONSE(monitor);
                        cli_Response.Command = commandLineInput.Command;
                        cli_Response.TargetFeature = commandLineInput.TargetFeature;

                        //if (!monitor.CapabilityDic.ContainsKey("C0"))
                        //{
                        //    cli_Response.Result = "FAIL";
                        //    cli_Response.Message = "Un - supported Feature";
                        //}
                        //else
                        //{
                        //    rc = GetVCPCode(devMgr, monitor, "0xC0").Result;
                        //    cli_Response.Value = (rc.value).ToString() + " hours";

                        //    cli_Response.Result = "PASS";
                        //    cli_Response.Message = "N/A";
                        //}
                        rc = GetVCPCode(devMgr, monitor, "0xC0").Result;
                        cli_Response.Value = (rc.value).ToString() + " hours";

                        cli_Response.Result = "PASS";
                        cli_Response.Message = "N/A";
                        output += "\n" + JsonConvert.SerializeObject(cli_Response, Formatting.Indented);
                    }
                }
                foreach (string modelName in commandLineInput.Model)
                {
                    var tmp = _AllInfoMonitors.FindAll(x => x.edid.ModelName.ToUpper().Equals(modelName.ToUpper()));
                    writelog("ActiveHours get entry");
                    foreach (MonitorInfo monitor in tmp)
                    {
                        CLI_RESPONSE cli_Response = new CLI_RESPONSE(monitor);
                        cli_Response.Command = commandLineInput.Command;
                        cli_Response.TargetFeature = commandLineInput.TargetFeature;

                        //if (!monitor.CapabilityDic.ContainsKey("C0"))
                        //{
                        //    cli_Response.Result = "FAIL";
                        //    cli_Response.Message = "Un - supported Feature";
                        //}
                        //else
                        //{
                        //    rc = GetVCPCode(devMgr, monitor, "0xC0").Result;
                        //    cli_Response.Value = (rc.value).ToString() + " hours";

                        //    cli_Response.Result = "PASS";
                        //    cli_Response.Message = "N/A";
                        //}
                        rc = GetVCPCode(devMgr, monitor, "0xC0").Result;
                        cli_Response.Value = (rc.value).ToString() + " hours";

                        cli_Response.Result = "PASS";
                        cli_Response.Message = "N/A";
                        output += "\n" + JsonConvert.SerializeObject(cli_Response, Formatting.Indented);
                    }
                }
            }
            writelog($"ActiveHours return exit value{output}");
            return ((int)CLI_ExitCode.success, output);
        }

        private (int code, string result) ApplyConfigurationX(IDeviceManagerSA devMgr, CommandLineInput commandLineInput)
        {
            if (commandLineInput.Command == "GET" || commandLineInput.Options.Count == 0)
            {
                CLI_RESPONSE cli_Response = new CLI_RESPONSE();
                cli_Response.Command = commandLineInput.Command;
                cli_Response.TargetFeature = commandLineInput.TargetFeature;
                cli_Response.Result = "FAIL";
                cli_Response.Message = "Invalid command line syntax or missing -value=file.json";
                return ((int)CLI_ExitCode.invalide_cmdline_syntax, cli_Response.ToJson());
            }
            else
            {
                //string[] ss_1 = commandLineInput.Options[0].Option_Value.Split(",");
                if (commandLineInput.Options.Count == 1)
                {

                    if (commandLineInput.Options[0].Option_Value.Contains(","))
                    {
                        string[] ss_1 = commandLineInput.Options[0].Option_Value.Split(",");
                        string info = string.Empty;
                        string filename = ss_1[1];
                        if (!DDPM.SA.Common.Security.InputHelper.InputValidation_FilePathFileName(filename, true, out info))
                        {
                            CLI_RESPONSE cli_Response = new CLI_RESPONSE();
                            cli_Response.Command = commandLineInput.Command;
                            cli_Response.TargetFeature = commandLineInput.TargetFeature;
                            cli_Response.Result = "FAIL";
                            cli_Response.Message = "file.json is not exist.";
                            return ((int)CLI_ExitCode.invalide_cmdline_syntax, cli_Response.ToJson());
                        }
                        else
                        {
                            return ApplyConfiguration(devMgr, commandLineInput).Result;
                        }
                    }
                    else
                    {
                        CLI_RESPONSE cli_Response = new CLI_RESPONSE();
                        cli_Response.Command = commandLineInput.Command;
                        cli_Response.TargetFeature = commandLineInput.TargetFeature;
                        cli_Response.Result = "FAIL";
                        cli_Response.Message = "Invalid command line syntax.";
                        return ((int)CLI_ExitCode.invalide_cmdline_syntax, cli_Response.ToJson());
                    }
                }
                else
                {
                    CLI_RESPONSE cli_Response = new CLI_RESPONSE();
                    cli_Response.Command = commandLineInput.Command;
                    cli_Response.TargetFeature = commandLineInput.TargetFeature;
                    cli_Response.Result = "FAIL";
                    cli_Response.Message = "Invalid command line syntax.";
                    return ((int)CLI_ExitCode.invalide_cmdline_syntax, cli_Response.ToJson());
                }
            }
        }

        private async Task<(int code, string result)> ApplyConfiguration(IDeviceManagerSA devMgr, CommandLineInput commandLineInput)
        {
            DisplayPropertiesInfo displayPropertiesInfo = new DisplayPropertiesInfo();
            DisplayCurrentPropertiesInfo displayCurrentPropertiesInfo = new DisplayCurrentPropertiesInfo();
            Properties displayProperties;
            ALSConfig param = new ALSConfig();
            string output = string.Empty;
            ObjGetVCP rc = new ObjGetVCP();
            bool math_found = true;
            List<DeviceInfo> mathdevs = new List<DeviceInfo>();
            List<DeviceInfo> _deviceinfo = null;
            _deviceinfo = _devMgr.GetDevices().Result.deviceInfo;
            if (!string.IsNullOrEmpty(commandLineInput.Options[0].Option_Value))
            {
                string[] ss_1 = commandLineInput.Options[0].Option_Value.Split(",");
                if (ss_1.Length == 2 &&
                    !string.IsNullOrEmpty(ss_1[0]) &&
                    !string.IsNullOrEmpty(ss_1[1]))
                {
                    Trace.WriteLine(ss_1[0]);
                    Trace.WriteLine(ss_1[1]);

                    /*StreamReader r = new StreamReader(ss_1[1]);
                    string jsonString = r.ReadToEnd();;
                    r.Close();*/

                    // modify start @ 20241022 stephen: modify for CMA input config as json string
                    // x:\\config.json

                    string jsonString = String.Empty;

                    if ((@$"x:\config.json").ToLower().Equals(ss_1[1].ToLower()))
                    {
                        jsonString = commandLineInput.jsonDeviceConfig.ToString();
                    }
                    else
                    {
                        try
                        {
                            using (StreamReader r = new StreamReader(ss_1[1]))
                            {
                                jsonString = r.ReadToEnd();
                            }
                        }
                        catch (Exception ex)
                        {
                            writelog($"[CLIDisplayPlugins] ApplyConfiguration failed, exception message: {ex.Message}");
                        }
                    }
                    // modiffy end @ 20241022

                    jsonString = jsonString.ToUpper();
                    Trace.WriteLine(jsonString);
                    JObject jsonObject;
                    try
                    {
                        jsonObject = JObject.Parse(jsonString);
                    }
                    catch
                    {
                        CLI_RESPONSE3 cli_Response_ = new CLI_RESPONSE3();
                        cli_Response_.Command = commandLineInput.Command;
                        cli_Response_.TargetFeature = commandLineInput.TargetFeature;
                        cli_Response_.Result = "FAIL";
                        cli_Response_.Message = "file format is not valid.";
                        return ((int)CLI_ExitCode.invalide_cmdline_syntax, cli_Response_.ToJson());
                    }

                    //string[] jsonString_2 = jsonString.Split("\"DEVICE\":");
                    //int i = 0;
                    //int count = jsonString.Split("INDEX").Length - 1;

                    //switch (ss_1[0].ToUpper())
                    //{
                    //    case "DISPLAY":
                    //    case "MOUSE":
                    //    case "KEYBOARD":
                    //    case "WEBCAM":
                    //    case "WIREDAUDIO":
                    //    case "HEADSET":
                    //    case "PEN":
                    //    case "DOCK":
                    //        do
                    //        {
                    //            if (jsonString_2[i].Contains(ss_1[0].ToUpper(), StringComparison.OrdinalIgnoreCase))
                    //            {
                    //                jsonString_2[i] = jsonString_2[i].Replace($" \"LOGICAL{ss_1[0].ToUpper()}\"" + "}", "");
                    //                jsonString_2[i] = jsonString_2[i].Replace($" \"LOGICAL{ss_1[0].ToUpper()}\"", "");
                    //                if (jsonString_2[i].Contains("GET", StringComparison.OrdinalIgnoreCase) && count > 0)
                    //                {
                    //                    jsonString_2[i] = jsonString_2[i].Replace("{\r\n  \"COMMAND\": \"GET\",", "");
                    //                    jsonString_2[i] = jsonString_2[i].Replace("  \"TARGETFEATURE\": \"DEVICEDATA\",\r\n  \"RESULT\": \"PASS\",", "");
                    //                    jsonString_2[i] = jsonString_2[i].Replace("\"MESSAGE\": \"N/A\"\r\n}", "");
                    //                }
                    //                break;
                    //            }
                    //            i++;
                    //        } while (true);
                    //        break;
                    //}
                    //if (jsonString_2.Count() == 0 || i >= jsonString_2.Count() || string.IsNullOrWhiteSpace(jsonString_2[i]) || count < 1)
                    //{
                    //    CLI_RESPONSE cli_Response_ = new CLI_RESPONSE();
                    //    cli_Response_.Command = commandLineInput.Command;
                    //    cli_Response_.TargetFeature = commandLineInput.TargetFeature;
                    //    cli_Response_.Result = "FAIL";
                    //    cli_Response_.Message = "file format is not valid.";
                    //    return ((int)CLI_ExitCode.invalide_cmdline_syntax, cli_Response_.ToJson());
                    //}
                    ////Get_DeviceData devicedata = JsonConvert.DeserializeObject<Get_DeviceData>(jsonString_2[i]);

                    //// malik
                    //// dynamic jsonObject = JsonConvert.DeserializeObject<dynamic>(jsonString_2[i]);
                    //JObject jsonObject = JObject.Parse(jsonString_2[i]);
                    // malik

                    bool ispass = true;
                    //string[] not_support_list = new string[] { "ColorPreset", "ColorManagement"};

                    List<int> _monitorIndeies = new List<int>();

                    //if (_AllInfoMonitors == null)
                    _AllInfoMonitors = devMgr.GetMonitors().Result;

                    _monitorIndeies = GetMonitorIndeies(commandLineInput, _AllInfoMonitors);
                    writelog($"CLI /set -display=applyConfiguration -value={commandLineInput.Options[0].Option_Value}");

                    int index = 0;
                    var resultMessages = new List<string>();

                    switch (ss_1[0].ToUpper())
                    {
                        case "DISPLAY":  //20250315 Chewlin Add for Deviceconfiguration support ServiceTag & Model
                            writelog($"DISPLAY set entry");
                            if (commandLineInput.ServiceTag != null && commandLineInput.ServiceTag.Count > 0 && !string.IsNullOrEmpty(commandLineInput.ServiceTag[0].ToString()))
                            {
                                if (_monitorIndeies != null)
                                {
                                    var monitor = _AllInfoMonitors.SingleOrDefault(x => x.edid.ServiceTag.ToString().Equals(commandLineInput.ServiceTag[0].ToString(), StringComparison.OrdinalIgnoreCase));
                                    if (monitor != null)
                                    {
                                        index = 0;
                                        Apply_Configuration ApplyConfiguration = new Apply_Configuration(monitor);
                                        ApplyConfiguration.Command = commandLineInput.Command;
                                        ApplyConfiguration.TargetFeature = commandLineInput.TargetFeature;

                                        bool retcode = false;
                                        displayPropertiesInfo = devMgr.GetDisplayPropertiesInfo(monitor).Result;

                                        foreach (var property in jsonObject.Properties())
                                        {
                                            Console.WriteLine($"Key: {property.Name}, Value: {property.Value}");
                                            List<MonitorInfo> MonitorInfo_Reget = devMgr.GetMonitors().Result;
                                            MonitorInfo monitor_incase = null;
                                            try
                                            {
                                               monitor_incase = MonitorInfo_Reget.Single(x => x.edid.ServiceTag.ToString().Equals(commandLineInput.ServiceTag[0].ToString(), StringComparison.OrdinalIgnoreCase));
                                            }
                                            catch 
                                            {
                                                CLI_RESPONSE cli_Response___ = new CLI_RESPONSE();
                                                cli_Response___.Command = commandLineInput.Command;
                                                cli_Response___.TargetFeature = commandLineInput.TargetFeature;
                                                cli_Response___.Result = "FAIL";
                                                cli_Response___.Message = "No servicetag found.";
                                                return ((int)CLI_ExitCode.invalide_cmdline_syntax, cli_Response___.ToJson());
                                            }
                                            writelog($"Re-get Monitor Info before each configuration setting");

                                            switch (property.Name.ToString())
                                            {
                                                case "SCREENORIENTATION":
                                                    writelog($"ScreenOrientation entry");
                                                    //if (monitor.CapabilityDic.ContainsKey("AA") && monitor.CapabilityDic["AA"] != null && monitor.CapabilityDic["AA"].Contains("00"))
                                                    //{
                                                    DisplayOrientation? displayOrientation = null;
                                                    switch (property.Value.ToString())
                                                    {
                                                        case "LANDSCAPE":
                                                            displayOrientation = DisplayOrientation.Angle0;
                                                            break;

                                                        case "PORTRAIT":
                                                            displayOrientation = DisplayOrientation.Angle90;
                                                            break;

                                                        case "LANDSCAPE_FLIPPED":
                                                            displayOrientation = DisplayOrientation.Angle180;
                                                            break;

                                                        case "PORTRAIT_FLIPPED":
                                                            displayOrientation = DisplayOrientation.Angle270;
                                                            break;

                                                        default:
                                                            retcode = false;
                                                            break;
                                                    }
                                                    if (displayOrientation != null)
                                                    {                                                       
                                                        retcode = _devMgr.SetDisplayPropertiest(monitor_incase, new Properties(), (DisplayOrientation)displayOrientation).Result;
                                                    }
                                                    if (!retcode) ispass = false;
                                                    else ApplyConfiguration.ScreenOrientation = property.Value.ToString();
                                                    writelog($"ScreenOrientation={ApplyConfiguration.ScreenOrientation}");
                                                    //}
                                                    //else
                                                    //{
                                                    //    writelog($"ScreenOrientation VCP not support");
                                                    //    output += $"\n  \"Result: \": \"ScreenOrientation VCP not support\"";
                                                    //}
                                                    break;

                                                case "ACTIVEINPUTSOURCE":
                                                    writelog($"ActiveInputSource entry");

                                                    if (monitor.CapabilityDic.ContainsKey("60"))
                                                    {
                                                        retcode = SetVCPCode(devMgr, monitor_incase, "0x60", get_InputSource_code(get_inputsource_type(property.Value.ToString().ToUpper()).ToString())).Result;
                                                        if (!retcode) ispass = false;
                                                        else ApplyConfiguration.ActiveInputSource = property.Value.ToString();
                                                        writelog($"ActiveInputSource={ApplyConfiguration.ActiveInputSource}");
                                                    }
                                                    else
                                                    {
                                                        writelog($"ActiveInputSource VCP not support");
                                                        //output += $"\n  \"Result: \": \"ActiveInputSource VCP not support\"";
                                                        resultMessages.Add("ActiveInputSource VCP not support");
                                                    }
                                                    break;

                                                case "RESOLUTION":
                                                    writelog($"RESOLUTION entry");
                                                    //if (monitor.CapabilityDic.ContainsKey("AA") && monitor.CapabilityDic["AA"] != null && monitor.CapabilityDic["AA"].Contains("00"))
                                                    //{
                                                    string[] ss = property.Value.ToString().Split(" ");
                                                    displayProperties = new Properties() { Resolutions_Width = int.Parse(ss[0]), Resolutions_High = int.Parse(ss[2]), Frequency = int.Parse(ss[4].Split(".00HZ")[0]) };
                                                    retcode = devMgr.SetDisplayPropertiest(monitor_incase, displayProperties, displayPropertiesInfo.CurrentOrientation).Result;
                                                    if (!retcode) ispass = false;
                                                    else ApplyConfiguration.Resolution = property.Value.ToString();
                                                    writelog($"RESOLUTION={ApplyConfiguration.Resolution}");
                                                    //}
                                                    //else
                                                    //{
                                                    //    writelog($"RESOLUTION VCP not support");
                                                    //    output += $"\n  \"Result: \": \"RESOLUTION VCP not support\"";
                                                    //}
                                                    break;

                                                //case "AspectRatio":

                                                //    int gcd = (int)GCD((ulong)displayProperties.Resolutions_Width, (ulong)displayProperties.Resolutions_High);
                                                //    ApplyConfiguration.AspectRatio = $"{displayProperties.Resolutions_Width / gcd}:{displayProperties.Resolutions_High / gcd}";
                                                //    writelog($"AspectRatio={ApplyConfiguration.AspectRatio}");
                                                //    break;

                                                case "CONTRASTLEVEL":
                                                    writelog($"ContrastLevel entry");
                                                    if (monitor.CapabilityDic.ContainsKey("12"))
                                                    {
                                                        retcode = SetVCPCode(devMgr, monitor_incase, "0x12", property.Value.ToString().Substring(0, property.Value.ToString().Length - 1)).Result;
                                                        if (!retcode) ispass = false;
                                                        else ApplyConfiguration.ContrastLevel = property.Value.ToString();
                                                        writelog($"ContrastLevel={ApplyConfiguration.ContrastLevel}");
                                                    }
                                                    else
                                                    {
                                                        writelog($"ContrastLevel VCP not support");
                                                        //output += $"\n  \"Result: \": \"ContrastLevel VCP not support\"";
                                                        resultMessages.Add("ContrastLevel VCP not support");
                                                    }
                                                    break;

                                                case "BRIGHTNESSLEVEL":
                                                    writelog($"BrightnessLevel entry");
                                                    if (monitor.CapabilityDic.ContainsKey("10"))
                                                    {
                                                        retcode = SetVCPCode(devMgr, monitor_incase, "0x10", property.Value.ToString().Substring(0, property.Value.ToString().Length - 1)).Result;
                                                        if (!retcode) ispass = false;
                                                        else ApplyConfiguration.BrightnessLevel = property.Value.ToString();
                                                        writelog($"BrightnessLevel={ApplyConfiguration.BrightnessLevel}");
                                                    }
                                                    else
                                                    {
                                                        writelog($"BrightnessLevel VCP not support");
                                                        //output += $"\n  \"Result: \": \"BrightnessLevel VCP not support\"";
                                                        resultMessages.Add("BrightnessLevel VCP not support");
                                                    }
                                                    break;

                                                case "LUMINANCELEVEL":
                                                    writelog($"LuminanceLevel entry");
                                                    if (!monitor.CapabilityDic.ContainsKey("10"))
                                                    {
                                                        retcode = SetVCPCode(devMgr, monitor_incase, "0x10", property.Value.ToString().Substring(0, property.Value.ToString().Length - 1)).Result;
                                                        if (!retcode) ispass = false;
                                                        else ApplyConfiguration.LuminanceLevel = property.Value.ToString();
                                                        writelog($"LuminanceLevel={ApplyConfiguration.LuminanceLevel}");
                                                    }
                                                    else
                                                    {
                                                        writelog($"LuminanceLevel VCP not support");
                                                        //output += $"\n  \"Result: \": \"LuminanceLevel VCP not support\"";
                                                        resultMessages.Add("LuminanceLevel VCP not support");
                                                    }
                                                    break;

                                                case "AUTOBRIGHTNESS":
                                                    writelog($"AutoBrightness entry");
                                                    if (monitor.CapabilityDic.ContainsKey("66"))
                                                    {
                                                        retcode = await devMgr.SetALSFeatureValue(monitor_incase, param, ALSFeatureQueryType.AutoBrightness, property.Value.ToString().ToUpper());
                                                        if (!retcode) ispass = false;
                                                        else ApplyConfiguration.AutoBrightness = property.Value.ToString();
                                                        writelog($"AutoBrightness={ApplyConfiguration.AutoBrightness}");
                                                    }
                                                    else
                                                    {
                                                        writelog($"AutoBrightness VCP not support");
                                                        //output += $"\n  \"Result: \": \"AutoBrightness VCP not support\"";
                                                        resultMessages.Add("AutoBrightness VCP not support");
                                                    }
                                                    break;

                                                case "AUTOBRIGHTNESSRANGELEVEL":
                                                    writelog($"AutoBrightnessRangeLevel entry");
                                                    if (monitor.CapabilityDic.ContainsKey("66"))
                                                    {
                                                        retcode = await devMgr.SetALSFeatureValue(monitor_incase, param, ALSFeatureQueryType.AutoBrightnessRangeLevel, get_RangeLevel(property.Value.ToString().ToUpper()));
                                                        if (!retcode) ispass = false;
                                                        else ApplyConfiguration.AutoBrightnessRangeLevel = property.Value.ToString();
                                                        writelog($"AutoBrightnessRangeLevel={ApplyConfiguration.AutoBrightnessRangeLevel}");
                                                    }
                                                    else
                                                    {
                                                        writelog($"AutoBrightnessRangeLevel VCP not support");
                                                        //output += $"\n  \"Result: \": \"AutoBrightnessRangeLevel VCP not support\"";
                                                        resultMessages.Add("AutoBrightnessRangeLevel VCP not support");
                                                    }
                                                    break;

                                                case "AUTOCOLORTEMP":
                                                    writelog($"AutoColorTemp entry");
                                                    if (monitor.CapabilityDic.ContainsKey("66"))
                                                    {
                                                        retcode = await devMgr.SetALSFeatureValue(monitor_incase, param, ALSFeatureQueryType.AutoColorTemperature, property.Value.ToString().ToUpper());
                                                        if (!retcode) ispass = false;
                                                        else ApplyConfiguration.AutoColorTemp = property.Value.ToString();
                                                        writelog($"AutoColorTemp={ApplyConfiguration.AutoColorTemp}");
                                                    }
                                                    else
                                                    {
                                                        writelog($"AutoColorTemp VCP not support");
                                                        //output += $"\n  \"Result: \": \"AutoColorTemp VCP not support\"";
                                                        resultMessages.Add("AutoColorTemp VCP not support");
                                                    }
                                                    break;

                                                case "PRIMARYMONITORFORSYNC":
                                                    writelog($"PrimaryMonitorForSync entry");
                                                    if (monitor.CapabilityDic.ContainsKey("66"))
                                                    {
                                                        retcode = await devMgr.SetALSFeatureValue(monitor_incase, param, ALSFeatureQueryType.PrimaryMonitorSync, property.Value.ToString().ToUpper());
                                                        if (!retcode) ispass = false;
                                                        else ApplyConfiguration.PrimaryMonitorForSync = property.Value.ToString();
                                                        writelog($"PrimaryMonitorForSync={ApplyConfiguration.PrimaryMonitorForSync}");
                                                    }
                                                    else
                                                    {
                                                        writelog($"PrimaryMonitorForSync VCP not support");
                                                        //output += $"\n  \"Result: \": \"PrimaryMonitorForSync VCP not support\"";
                                                        resultMessages.Add("PrimaryMonitorForSync VCP not support");
                                                    }
                                                    break;

                                                case "USB_CPRIORITIZATION":
                                                    if (displayPropertiesInfo.SupportedUSBCPrioritization)
                                                    {
                                                        USBCPrioritizationType gettype = get_USBCPrioritization(property.Value.ToString());
                                                        if (gettype != USBCPrioritizationType.Unknow)
                                                        {
                                                            retcode = devMgr.SetUSBCPrioritizationType(monitor_incase, gettype).Result;
                                                            if (!retcode) ispass = false;
                                                            else ApplyConfiguration.USB_CPrioritization = property.Value.ToString();
                                                            writelog($"USB_CPrioritization={ApplyConfiguration.USB_CPrioritization}");
                                                        }
                                                    }
                                                    else
                                                    {
                                                        ApplyConfiguration.USB_CPrioritization = "NOT SUPPORT";
                                                        //output += $"\n  \"Result: \": \"USB_CPrioritization not support\"";
                                                        resultMessages.Add("USB_CPrioritization not support");
                                                    }

                                                    break;
                                                case "COLORMANAGEMENT":
                                                    ColorManagementRunType colorManagementType = get_ColorManagement(property.Value.ToString());
                                                    switch (colorManagementType)
                                                    {
                                                        case ColorManagementRunType.Off:
                                                        case ColorManagementRunType.Byhost:
                                                        case ColorManagementRunType.Bymonitor:
                                                            writelog($"ColorManagement {colorManagementType.ToString()} entry");
                                                            retcode = devMgr.AutoColorManagementForMonitorConfig(monitor_incase, colorManagementType.ToString().ToUpper(), string.Empty, string.Empty).Result;
                                                            if (!retcode) ispass = false;
                                                            else ApplyConfiguration.ColorManagement = property.Value.ToString();
                                                            writelog($"ColorManagement={ApplyConfiguration.ColorManagement}");
                                                            break;
                                                        default:
                                                            writelog($"option value not support");
                                                            ApplyConfiguration.ColorManagement = "NOT SUPPORT";
                                                            //output += $"\n  \"Result: \": \"ColorManagement not support\"";
                                                            resultMessages.Add("ColorManagement not support");
                                                            break;
                                                    }
                                                    break;
                                                case "SPEAKERMICROPHONE":
                                                    writelog($"SpeakerMicrophone entry");
                                                    if (CheckSpeakerSupported(monitor_incase, commandLineInput) && CheckMicrophoneSupported(monitor_incase, commandLineInput))
                                                    {
                                                        rc = GetVCPCode(devMgr, monitor_incase, "0x62").Result;
                                                        int getvalue = Convert.ToInt32(rc.value);
                                                        retcode = SetVCPCode(devMgr, monitor_incase, "0x62", get_SpeakerMicrophone(property.Value.ToString(), getvalue)).Result;

                                                        //rc = GetVCPCode(devMgr, monitor, "0x8D").Result;
                                                        //int getvalue2 = Convert.ToInt32(rc.value);
                                                        //bool retcode2 = SetVCPCode(devMgr, monitor, "0x8D", get_SpeakerMicrophone(property.Value.ToString(), getvalue2)).Result;

                                                        if (!retcode) ispass = false;
                                                        else ApplyConfiguration.SpeakerMicrophone = property.Value.ToString();
                                                        writelog($"SpeakerMicrophone={ApplyConfiguration.SpeakerMicrophone}");
                                                    }
                                                    else
                                                    {
                                                        ApplyConfiguration.SpeakerMicrophone = "N/A";
                                                        writelog($"SpeakerMicrophone VCP not support");
                                                        //output += $"\n  \"Result: \": \"SpeakerMicrophone VCP not support\"";
                                                        resultMessages.Add("SpeakerMicrophone VCP not support");
                                                    }

                                                    break;

                                                case "SPEAKERVOLUME":
                                                    writelog($"SpeakerVolume entry");
                                                    if (CheckSpeakerSupported(monitor_incase, commandLineInput))
                                                    {
                                                        rc = GetVCPCode(devMgr, monitor_incase, "0x62").Result;
                                                        int getvalue = Convert.ToInt32(rc.value);
                                                        string setvalue = get_SpeakerVolume(property.Value.ToString(), getvalue);
                                                        if (setvalue != "unknown_command")
                                                            retcode = SetVCPCode(devMgr, monitor_incase, "0x62", setvalue).Result;
                                                        else
                                                            retcode = SetVCPCode(devMgr, monitor_incase, "0x62", property.Value.ToString()).Result;
                                                        if (!retcode) ispass = false;
                                                        else ApplyConfiguration.SpeakerVolume = property.Value.ToString();
                                                        writelog($"SpeakerVolume={ApplyConfiguration.SpeakerVolume}");
                                                    }
                                                    else
                                                    {
                                                        ApplyConfiguration.SpeakerVolume = "N/A";
                                                        writelog($"SpeakerVolume VCP not support");
                                                        //output += $"\n  \"Result: \": \"SpeakerVolume VCP not support\"";
                                                        resultMessages.Add("SpeakerVolume VCP not support");
                                                    }

                                                    break;

                                                case "MICROPHONECONTROL":
                                                    writelog($"MicrophoneControl entry");
                                                    if (CheckMicrophoneSupported(monitor_incase, commandLineInput))
                                                    {
                                                        rc = GetVCPCode(devMgr, monitor_incase, "0x8D").Result;
                                                        int getvalue = Convert.ToInt32(rc.value);
                                                        retcode = SetVCPCode(devMgr, monitor_incase, "0x8D", get_MicrophoneControl(property.Value.ToString(), getvalue)).Result;
                                                        if (!retcode) ispass = false;
                                                        else ApplyConfiguration.MicrophoneControl = property.Value.ToString();
                                                        writelog($"MicrophoneControl={ApplyConfiguration.MicrophoneControl}");
                                                    }
                                                    else
                                                    {
                                                        ApplyConfiguration.MicrophoneControl = "N/A";
                                                        writelog($"MicrophoneControl VCP not support");
                                                        //output += $"\n  \"Result: \": \"MicrophoneControl VCP not support\"";
                                                        resultMessages.Add("MicrophoneControl VCP not support");
                                                    }

                                                    break;

                                                //case "UNIFORMITY":
                                                //    writelog($"Uniformity entry");
                                                //    if (monitor.CapabilityDic.ContainsKey("E4"))
                                                //    {
                                                //        retcode = SetVCPCode(devMgr, monitor, "0xE4", get_Uniformity(property.Value.ToString())).Result;
                                                //    if (!retcode) ispass = false;
                                                //        else ApplyConfiguration.Uniformity = property.Value.ToString();
                                                //        writelog($"Uniformity={ApplyConfiguration.Uniformity}");
                                                //    }
                                                //    else
                                                //    {
                                                //        ApplyConfiguration.Uniformity = "N/A";
                                                //        writelog($"Uniformity VCP not support");
                                                //        output += $"\n  \"Result: \": \"Uniformity VCP not support\"";
                                                //    }

                                                //    break;

                                                case "POWERNAP":
                                                    writelog($"PowerNap entry");
                                                    await SetPowerNapAsync(get_PowerNapType_code(property.Value.ToString()), devMgr, monitor_incase.edid.ModelName, monitor_incase.edid.SerialNumber, monitor_incase.edid.ServiceTag);
                                                    //PowerNapSetting setting = new PowerNapSetting
                                                    //{
                                                    //    Status = false,
                                                    //    ModelName = monitor.edid.ModelName,
                                                    //    SerialNumber = monitor.edid.SerialNumber,
                                                    //    RunType = get_PowerNapType_code(property.Value.ToString())
                                                    //};
                                                    //retcode = await devMgr.SavePowerNapSetting(setting);
                                                    //UpdateUINotify off = new UpdateUINotify();
                                                    //off.UI_Field_Name = "POWERNAP;"+ property.Value.ToString();
                                                    //devMgr.OnUIUpdateNotify(off);
                                                    if (!retcode) ispass = false;
                                                    else ApplyConfiguration.PowerNap = property.Value.ToString();
                                                    writelog($"PowerNap={ApplyConfiguration.PowerNap}");
                                                    break;

                                                case "OSD_LANGUAGE":
                                                    writelog($"OSD_language entry");
                                                    if (monitor.CapabilityDic.ContainsKey("CC"))
                                                    {
                                                        retcode = SetVCPCode(devMgr, monitor_incase, "0xCC", GetOSDLanguage_index(property.Value.ToString()).ToString()).Result;
                                                        writelog($"OSD_language={GetOSDLanguage_index(property.Value.ToString()).ToString()}");
                                                        if (!retcode) ispass = false;
                                                        else ApplyConfiguration.OSD_language = property.Value.ToString();
                                                        writelog($"OSD_language={ApplyConfiguration.OSD_language}");
                                                    }
                                                    else
                                                    {
                                                        writelog($"OSD_language VCP not support");
                                                        //output += $"\n  \"Result: \": \"OSD_language VCP not support\"";
                                                        resultMessages.Add("OSD_language VCP not support");
                                                    }

                                                    break;

                                                default:
                                                    break;
                                            }
                                        }

                                        if (ispass)
                                        {
                                            ApplyConfiguration.Result = "PASS";
                                            ApplyConfiguration.Message = "N/A";
                                        }
                                        else
                                        {
                                            ApplyConfiguration.Result = "FAIL";
                                            ApplyConfiguration.Message = "N/A";
                                        }
                                        System.Console.WriteLine(JsonConvert.SerializeObject(ApplyConfiguration, Formatting.Indented));
                                        output += "\n" + JsonConvert.SerializeObject(ApplyConfiguration, Formatting.Indented);

                                    }

                                    else
                                    {
                                        CLI_RESPONSE3 cli_Response_ = new CLI_RESPONSE3();
                                        cli_Response_.Command = commandLineInput.Command;
                                        cli_Response_.TargetFeature = commandLineInput.TargetFeature;
                                        cli_Response_.Result = "FAIL";
                                        cli_Response_.Message = "Invalid SERVICETAG";
                                        return ((int)CLI_ExitCode.invalide_cmdline_syntax, cli_Response_.ToJson());
                                    }
                                }
                                else
                                {
                                    writelog($"No DISPLAY connect");
                                    ispass = false;
                                    output = string.Empty;
                                }
                            }

                            else if (commandLineInput.Model != null && commandLineInput.Model.Count > 0 && !string.IsNullOrEmpty(commandLineInput.Model[0].ToString()))
                            {
                                if (_monitorIndeies != null)
                                {
                                    var matchmonitor = _AllInfoMonitors.FindAll(x => x.modelName.ToString().Equals(commandLineInput.Model[0].ToString(), StringComparison.OrdinalIgnoreCase));
                                    if (matchmonitor.Count > 0)
                                    {
                                        foreach (MonitorInfo monitor in matchmonitor)
                                        {
                                            index = 0;
                                            Apply_Configuration ApplyConfiguration = new Apply_Configuration(monitor);
                                            ApplyConfiguration.Command = commandLineInput.Command;
                                            ApplyConfiguration.TargetFeature = commandLineInput.TargetFeature;

                                            bool retcode = false;
                                            displayPropertiesInfo = devMgr.GetDisplayPropertiesInfo(monitor).Result;

                                            foreach (var property in jsonObject.Properties())
                                            {
                                                Console.WriteLine($"Key: {property.Name}, Value: {property.Value}");

                                                switch (property.Name.ToString())
                                                {
                                                    case "SCREENORIENTATION":
                                                        writelog($"ScreenOrientation entry");
                                                        //if (monitor.CapabilityDic.ContainsKey("AA") && monitor.CapabilityDic["AA"] != null && monitor.CapabilityDic["AA"].Contains("00"))
                                                        //{
                                                        DisplayOrientation? displayOrientation = null;
                                                        switch (property.Value.ToString())
                                                        {
                                                            case "LANDSCAPE":
                                                                displayOrientation = DisplayOrientation.Angle0;
                                                                break;

                                                            case "PORTRAIT":
                                                                displayOrientation = DisplayOrientation.Angle90;
                                                                break;

                                                            case "LANDSCAPE_FLIPPED":
                                                                displayOrientation = DisplayOrientation.Angle180;
                                                                break;

                                                            case "PORTRAIT_FLIPPED":
                                                                displayOrientation = DisplayOrientation.Angle270;
                                                                break;

                                                            default:
                                                                retcode = false;
                                                                break;
                                                        }
                                                        if (displayOrientation != null)
                                                        {
                                                            retcode = _devMgr.SetDisplayPropertiest(monitor, new Properties(), (DisplayOrientation)displayOrientation).Result;
                                                        }
                                                        if (!retcode) ispass = false;
                                                        else ApplyConfiguration.ScreenOrientation = property.Value.ToString();
                                                        writelog($"ScreenOrientation={ApplyConfiguration.ScreenOrientation}");
                                                        //}
                                                        //else
                                                        //{
                                                        //    writelog($"ScreenOrientation VCP not support");
                                                        //    output += $"\n  \"Result: \": \"ScreenOrientation VCP not support\"";
                                                        //}
                                                        break;

                                                    case "ACTIVEINPUTSOURCE":
                                                        writelog($"ActiveInputSource entry");
                                                        if (monitor.CapabilityDic.ContainsKey("60"))
                                                        {
                                                            retcode = SetVCPCode(devMgr, monitor, "0x60", get_InputSource_code(get_inputsource_type(property.Value.ToString().ToUpper()).ToString())).Result;
                                                            if (!retcode) ispass = false;
                                                            else ApplyConfiguration.ActiveInputSource = property.Value.ToString();
                                                            writelog($"ActiveInputSource={ApplyConfiguration.ActiveInputSource}");
                                                        }
                                                        else
                                                        {
                                                            writelog($"ActiveInputSource VCP not support");
                                                            //output += $"\n  \"Result: \": \"ActiveInputSource VCP not support\"";
                                                            resultMessages.Add("ActiveInputSource VCP not support");
                                                        }
                                                        break;

                                                    case "RESOLUTION":
                                                        writelog($"RESOLUTION entry");
                                                        //if (monitor.CapabilityDic.ContainsKey("AA") && monitor.CapabilityDic["AA"] != null && monitor.CapabilityDic["AA"].Contains("00"))
                                                        //{
                                                        string[] ss = property.Value.ToString().Split(" ");
                                                        displayProperties = new Properties() { Resolutions_Width = int.Parse(ss[0]), Resolutions_High = int.Parse(ss[2]), Frequency = int.Parse(ss[4].Split(".00HZ")[0]) };
                                                        retcode = devMgr.SetDisplayPropertiest(monitor, displayProperties, displayPropertiesInfo.CurrentOrientation).Result;
                                                        if (!retcode) ispass = false;
                                                        else ApplyConfiguration.Resolution = property.Value.ToString();
                                                        writelog($"RESOLUTION={ApplyConfiguration.Resolution}");
                                                        //}
                                                        //else
                                                        //{
                                                        //    writelog($"RESOLUTION VCP not support");
                                                        //    output += $"\n  \"Result: \": \"RESOLUTION VCP not support\"";
                                                        //}
                                                        break;

                                                    //case "AspectRatio":

                                                    //    int gcd = (int)GCD((ulong)displayProperties.Resolutions_Width, (ulong)displayProperties.Resolutions_High);
                                                    //    ApplyConfiguration.AspectRatio = $"{displayProperties.Resolutions_Width / gcd}:{displayProperties.Resolutions_High / gcd}";
                                                    //    writelog($"AspectRatio={ApplyConfiguration.AspectRatio}");
                                                    //    break;

                                                    case "CONTRASTLEVEL":
                                                        writelog($"ContrastLevel entry");
                                                        if (monitor.CapabilityDic.ContainsKey("12"))
                                                        {
                                                            retcode = SetVCPCode(devMgr, monitor, "0x12", property.Value.ToString().Substring(0, property.Value.ToString().Length - 1)).Result;
                                                            if (!retcode) ispass = false;
                                                            else ApplyConfiguration.ContrastLevel = property.Value.ToString();
                                                            writelog($"ContrastLevel={ApplyConfiguration.ContrastLevel}");
                                                        }
                                                        else
                                                        {
                                                            writelog($"ContrastLevel VCP not support");
                                                            //output += $"\n  \"Result: \": \"ContrastLevel VCP not support\"";
                                                            resultMessages.Add("ContrastLevel VCP not support");
                                                        }
                                                        break;

                                                    case "BRIGHTNESSLEVEL":
                                                        writelog($"BrightnessLevel entry");
                                                        if (monitor.CapabilityDic.ContainsKey("10"))
                                                        {
                                                            retcode = SetVCPCode(devMgr, monitor, "0x10", property.Value.ToString().Substring(0, property.Value.ToString().Length - 1)).Result;
                                                            if (!retcode) ispass = false;
                                                            else ApplyConfiguration.BrightnessLevel = property.Value.ToString();
                                                            writelog($"BrightnessLevel={ApplyConfiguration.BrightnessLevel}");
                                                        }
                                                        else
                                                        {
                                                            writelog($"BrightnessLevel VCP not support");
                                                            //output += $"\n  \"Result: \": \"BrightnessLevel VCP not support\"";
                                                            resultMessages.Add("BrightnessLevel VCP not support");
                                                        }
                                                        break;

                                                    case "LUMINANCELEVEL":
                                                        writelog($"LuminanceLevel entry");
                                                        if (!monitor.CapabilityDic.ContainsKey("10"))
                                                        {
                                                            retcode = SetVCPCode(devMgr, monitor, "0x10", property.Value.ToString().Substring(0, property.Value.ToString().Length - 1)).Result;
                                                            if (!retcode) ispass = false;
                                                            else ApplyConfiguration.LuminanceLevel = property.Value.ToString();
                                                            writelog($"LuminanceLevel={ApplyConfiguration.LuminanceLevel}");
                                                        }
                                                        else
                                                        {
                                                            writelog($"LuminanceLevel VCP not support");
                                                            //output += $"\n  \"Result: \": \"LuminanceLevel VCP not support\"";
                                                            resultMessages.Add("LuminanceLevel VCP not support");
                                                        }
                                                        break;

                                                    case "AUTOBRIGHTNESS":
                                                        writelog($"AutoBrightness entry");
                                                        if (monitor.CapabilityDic.ContainsKey("66"))
                                                        {
                                                            retcode = await devMgr.SetALSFeatureValue(monitor, param, ALSFeatureQueryType.AutoBrightness, property.Value.ToString().ToUpper());
                                                            if (!retcode) ispass = false;
                                                            else ApplyConfiguration.AutoBrightness = property.Value.ToString();
                                                            writelog($"AutoBrightness={ApplyConfiguration.AutoBrightness}");
                                                        }
                                                        else
                                                        {
                                                            writelog($"AutoBrightness VCP not support");
                                                            //output += $"\n  \"Result: \": \"AutoBrightness VCP not support\"";
                                                            resultMessages.Add("AutoBrightness VCP not support");
                                                        }
                                                        break;

                                                    case "AUTOBRIGHTNESSRANGELEVEL":
                                                        writelog($"AutoBrightnessRangeLevel entry");
                                                        if (monitor.CapabilityDic.ContainsKey("66"))
                                                        {
                                                            retcode = await devMgr.SetALSFeatureValue(monitor, param, ALSFeatureQueryType.AutoBrightnessRangeLevel, get_RangeLevel(property.Value.ToString().ToUpper()));
                                                            if (!retcode) ispass = false;
                                                            else ApplyConfiguration.AutoBrightnessRangeLevel = property.Value.ToString();
                                                            writelog($"AutoBrightnessRangeLevel={ApplyConfiguration.AutoBrightnessRangeLevel}");
                                                        }
                                                        else
                                                        {
                                                            writelog($"AutoBrightnessRangeLevel VCP not support");
                                                            //output += $"\n  \"Result: \": \"AutoBrightnessRangeLevel VCP not support\"";
                                                            resultMessages.Add("AutoBrightnessRangeLevel VCP not support");
                                                        }
                                                        break;

                                                    case "AUTOCOLORTEMP":
                                                        writelog($"AutoColorTemp entry");
                                                        if (monitor.CapabilityDic.ContainsKey("66"))
                                                        {
                                                            retcode = await devMgr.SetALSFeatureValue(monitor, param, ALSFeatureQueryType.AutoColorTemperature, property.Value.ToString().ToUpper());
                                                            if (!retcode) ispass = false;
                                                            else ApplyConfiguration.AutoColorTemp = property.Value.ToString();
                                                            writelog($"AutoColorTemp={ApplyConfiguration.AutoColorTemp}");
                                                        }
                                                        else
                                                        {
                                                            writelog($"AutoColorTemp VCP not support");
                                                            //output += $"\n  \"Result: \": \"AutoColorTemp VCP not support\"";
                                                            resultMessages.Add("AutoColorTemp VCP not support");
                                                        }
                                                        break;

                                                    case "PRIMARYMONITORFORSYNC":
                                                        writelog($"PrimaryMonitorForSync entry");
                                                        if (monitor.CapabilityDic.ContainsKey("66"))
                                                        {
                                                            retcode = await devMgr.SetALSFeatureValue(monitor, param, ALSFeatureQueryType.PrimaryMonitorSync, property.Value.ToString().ToUpper());
                                                            if (!retcode) ispass = false;
                                                            else ApplyConfiguration.PrimaryMonitorForSync = property.Value.ToString();
                                                            writelog($"PrimaryMonitorForSync={ApplyConfiguration.PrimaryMonitorForSync}");
                                                        }
                                                        else
                                                        {
                                                            writelog($"PrimaryMonitorForSync VCP not support");
                                                            //output += $"\n  \"Result: \": \"PrimaryMonitorForSync VCP not support\"";
                                                            resultMessages.Add("PrimaryMonitorForSync VCP not support");
                                                        }
                                                        break;

                                                    case "USB_CPRIORITIZATION":
                                                        if (displayPropertiesInfo.SupportedUSBCPrioritization)
                                                        {
                                                            USBCPrioritizationType gettype = get_USBCPrioritization(property.Value.ToString());
                                                            if (gettype != USBCPrioritizationType.Unknow)
                                                            {
                                                                retcode = devMgr.SetUSBCPrioritizationType(monitor, gettype).Result;
                                                                if (!retcode) ispass = false;
                                                                else ApplyConfiguration.USB_CPrioritization = property.Value.ToString();
                                                                writelog($"USB_CPrioritization={ApplyConfiguration.USB_CPrioritization}");
                                                            }
                                                        }
                                                        else
                                                        {
                                                            ApplyConfiguration.USB_CPrioritization = "NOT SUPPORT";
                                                            //output += $"\n  \"Result: \": \"USB_CPrioritization not support\"";
                                                            resultMessages.Add("USB_CPrioritization not support");
                                                        }

                                                        break;
                                                    case "COLORMANAGEMENT":
                                                        ColorManagementRunType colorManagementType = get_ColorManagement(property.Value.ToString());
                                                        switch (colorManagementType)
                                                        {
                                                            case ColorManagementRunType.Off:
                                                            case ColorManagementRunType.Byhost:
                                                            case ColorManagementRunType.Bymonitor:
                                                                writelog($"ColorManagement {colorManagementType.ToString()} entry");
                                                                retcode = devMgr.AutoColorManagementForMonitorConfig(monitor, colorManagementType.ToString().ToUpper(), string.Empty, string.Empty).Result;
                                                                if (!retcode) ispass = false;
                                                                else ApplyConfiguration.ColorManagement = property.Value.ToString();
                                                                writelog($"ColorManagement={ApplyConfiguration.ColorManagement}");
                                                                break;
                                                            default:
                                                                writelog($"option value not support");
                                                                ApplyConfiguration.ColorManagement = "NOT SUPPORT";
                                                                //output += $"\n  \"Result: \": \"ColorManagement not support\"";
                                                                resultMessages.Add("ColorManagement not support");
                                                                break;
                                                        }
                                                        break;
                                                    case "SPEAKERMICROPHONE":
                                                        writelog($"SpeakerMicrophone entry");
                                                        if (CheckSpeakerSupported(monitor, commandLineInput) && CheckMicrophoneSupported(monitor, commandLineInput))
                                                        {
                                                            rc = GetVCPCode(devMgr, monitor, "0x62").Result;
                                                            int getvalue = Convert.ToInt32(rc.value);
                                                            retcode = SetVCPCode(devMgr, monitor, "0x62", get_SpeakerMicrophone(property.Value.ToString(), getvalue)).Result;

                                                            //rc = GetVCPCode(devMgr, monitor, "0x8D").Result;
                                                            //int getvalue2 = Convert.ToInt32(rc.value);
                                                            //bool retcode2 = SetVCPCode(devMgr, monitor, "0x8D", get_SpeakerMicrophone(property.Value.ToString(), getvalue2)).Result;

                                                            if (!retcode) ispass = false;
                                                            else ApplyConfiguration.SpeakerMicrophone = property.Value.ToString();
                                                            writelog($"SpeakerMicrophone={ApplyConfiguration.SpeakerMicrophone}");
                                                        }
                                                        else
                                                        {
                                                            ApplyConfiguration.SpeakerMicrophone = "N/A";
                                                            writelog($"SpeakerMicrophone VCP not support");
                                                            //output += $"\n  \"Result: \": \"SpeakerMicrophone VCP not support\"";
                                                            resultMessages.Add("SpeakerMicrophone VCP not support");
                                                        }

                                                        break;

                                                    case "SPEAKERVOLUME":
                                                        writelog($"SpeakerVolume entry");
                                                        if (CheckSpeakerSupported(monitor, commandLineInput))
                                                        {
                                                            rc = GetVCPCode(devMgr, monitor, "0x62").Result;
                                                            int getvalue = Convert.ToInt32(rc.value);
                                                            string setvalue = get_SpeakerVolume(property.Value.ToString(), getvalue);
                                                            if (setvalue != "unknown_command")
                                                                retcode = SetVCPCode(devMgr, monitor, "0x62", setvalue).Result;
                                                            else
                                                                retcode = SetVCPCode(devMgr, monitor, "0x62", property.Value.ToString()).Result;
                                                            if (!retcode) ispass = false;
                                                            else ApplyConfiguration.SpeakerVolume = property.Value.ToString();
                                                            writelog($"SpeakerVolume={ApplyConfiguration.SpeakerVolume}");
                                                        }
                                                        else
                                                        {
                                                            ApplyConfiguration.SpeakerVolume = "N/A";
                                                            writelog($"SpeakerVolume VCP not support");
                                                            //output += $"\n  \"Result: \": \"SpeakerVolume VCP not support\"";
                                                            resultMessages.Add("SpeakerVolume VCP not support");
                                                        }

                                                        break;

                                                    case "MICROPHONECONTROL":
                                                        writelog($"MicrophoneControl entry");
                                                        if (CheckMicrophoneSupported(monitor, commandLineInput))
                                                        {
                                                            rc = GetVCPCode(devMgr, monitor, "0x8D").Result;
                                                            int getvalue = Convert.ToInt32(rc.value);
                                                            retcode = SetVCPCode(devMgr, monitor, "0x8D", get_MicrophoneControl(property.Value.ToString(), getvalue)).Result;
                                                            if (!retcode) ispass = false;
                                                            else ApplyConfiguration.MicrophoneControl = property.Value.ToString();
                                                            writelog($"MicrophoneControl={ApplyConfiguration.MicrophoneControl}");
                                                        }
                                                        else
                                                        {
                                                            ApplyConfiguration.MicrophoneControl = "N/A";
                                                            writelog($"MicrophoneControl VCP not support");
                                                            //output += $"\n  \"Result: \": \"MicrophoneControl VCP not support\"";
                                                            resultMessages.Add("MicrophoneControl VCP not support");
                                                        }

                                                        break;

                                                    //case "UNIFORMITY":
                                                    //    writelog($"Uniformity entry");
                                                    //    if (monitor.CapabilityDic.ContainsKey("E4"))
                                                    //    {
                                                    //        retcode = SetVCPCode(devMgr, monitor, "0xE4", get_Uniformity(property.Value.ToString())).Result;
                                                    //    if (!retcode) ispass = false;
                                                    //        else ApplyConfiguration.Uniformity = property.Value.ToString();
                                                    //        writelog($"Uniformity={ApplyConfiguration.Uniformity}");
                                                    //    }
                                                    //    else
                                                    //    {
                                                    //        ApplyConfiguration.Uniformity = "N/A";
                                                    //        writelog($"Uniformity VCP not support");
                                                    //        output += $"\n  \"Result: \": \"Uniformity VCP not support\"";
                                                    //    }

                                                    //    break;

                                                    case "POWERNAP":
                                                        writelog($"PowerNap entry");
                                                        await SetPowerNapAsync(get_PowerNapType_code(property.Value.ToString()), devMgr, monitor.edid.ModelName, monitor.edid.SerialNumber, monitor.edid.ServiceTag);
                                                        //PowerNapSetting setting = new PowerNapSetting
                                                        //{
                                                        //    Status = false,
                                                        //    ModelName = monitor.edid.ModelName,
                                                        //    SerialNumber = monitor.edid.SerialNumber,
                                                        //    RunType = get_PowerNapType_code(property.Value.ToString())
                                                        //};
                                                        //retcode = await devMgr.SavePowerNapSetting(setting);
                                                        //UpdateUINotify off = new UpdateUINotify();
                                                        //off.UI_Field_Name = "POWERNAP;"+ property.Value.ToString();
                                                        //devMgr.OnUIUpdateNotify(off);
                                                        if (!retcode) ispass = false;
                                                        else ApplyConfiguration.PowerNap = property.Value.ToString();
                                                        writelog($"PowerNap={ApplyConfiguration.PowerNap}");
                                                        break;

                                                    case "OSD_LANGUAGE":
                                                        writelog($"OSD_language entry");
                                                        if (monitor.CapabilityDic.ContainsKey("CC"))
                                                        {
                                                            retcode = SetVCPCode(devMgr, monitor, "0xCC", GetOSDLanguage_index(property.Value.ToString()).ToString()).Result;
                                                            writelog($"OSD_language={GetOSDLanguage_index(property.Value.ToString()).ToString()}");
                                                            if (!retcode) ispass = false;
                                                            else ApplyConfiguration.OSD_language = property.Value.ToString();
                                                            writelog($"OSD_language={ApplyConfiguration.OSD_language}");
                                                        }
                                                        else
                                                        {
                                                            writelog($"OSD_language VCP not support");
                                                            //output += $"\n  \"Result: \": \"OSD_language VCP not support\"";
                                                            resultMessages.Add("OSD_language VCP not support");
                                                        }

                                                        break;

                                                    default:
                                                        break;
                                                }
                                            }

                                            if (ispass)
                                            {
                                                ApplyConfiguration.Result = "PASS";
                                                ApplyConfiguration.Message = "N/A";
                                            }
                                            else
                                            {
                                                ApplyConfiguration.Result = "FAIL";
                                                ApplyConfiguration.Message = "N/A";
                                            }
                                            System.Console.WriteLine(JsonConvert.SerializeObject(ApplyConfiguration, Formatting.Indented));
                                            output += "\n" + JsonConvert.SerializeObject(ApplyConfiguration, Formatting.Indented);
                                            index++;
                                        }
                                    }
                                    else
                                    {
                                        CLI_RESPONSE3 cli_Response_ = new CLI_RESPONSE3();
                                        cli_Response_.Command = commandLineInput.Command;
                                        cli_Response_.TargetFeature = commandLineInput.TargetFeature;
                                        cli_Response_.Result = "FAIL";
                                        cli_Response_.Message = "Invalid Model";
                                        return ((int)CLI_ExitCode.invalide_cmdline_syntax, cli_Response_.ToJson());
                                    }
                                }
                                else
                                {
                                    writelog($"No DISPLAY connect");
                                    ispass = false;
                                    output = string.Empty;
                                }
                            }
                            else
                            {
                                foreach (int idx in _monitorIndeies)
                                {
                                    MonitorInfo monitor = _AllInfoMonitors[idx];
                                    Apply_Configuration ApplyConfiguration = new Apply_Configuration(monitor);
                                    ApplyConfiguration.Command = commandLineInput.Command;
                                    ApplyConfiguration.TargetFeature = commandLineInput.TargetFeature;
                                    //ApplyConfiguration.Model = monitor.modelName;
                                    //ApplyConfiguration.SerialNumber = monitor.edid.SerialNumber;
                                    //ApplyConfiguration.Index = change_0base_to_1base((monitor.Index).ToString());
                                    //ApplyConfiguration.ServiceTag = monitor.edid.ServiceTag;

                                    // malik
                                    bool retcode = false;
                                    displayPropertiesInfo = devMgr.GetDisplayPropertiesInfo(monitor).Result;

                                    foreach (var property in jsonObject.Properties())
                                    {
                                        Console.WriteLine($"Key: {property.Name}, Value: {property.Value}");

                                        switch (property.Name.ToString())
                                        {
                                            case "SCREENORIENTATION":
                                                writelog($"ScreenOrientation entry");
                                                //if (monitor.CapabilityDic.ContainsKey("AA") && monitor.CapabilityDic["AA"] != null && monitor.CapabilityDic["AA"].Contains("00"))
                                                //{
                                                DisplayOrientation? displayOrientation = null;
                                                switch (property.Value.ToString())
                                                {
                                                    case "LANDSCAPE":
                                                        displayOrientation = DisplayOrientation.Angle0;
                                                        break;

                                                    case "PORTRAIT":
                                                        displayOrientation = DisplayOrientation.Angle90;
                                                        break;

                                                    case "LANDSCAPE_FLIPPED":
                                                        displayOrientation = DisplayOrientation.Angle180;
                                                        break;

                                                    case "PORTRAIT_FLIPPED":
                                                        displayOrientation = DisplayOrientation.Angle270;
                                                        break;

                                                    default:
                                                        retcode = false;
                                                        break;
                                                }
                                                if (displayOrientation != null)
                                                {
                                                    retcode = _devMgr.SetDisplayPropertiest(monitor, new Properties(), (DisplayOrientation)displayOrientation).Result;
                                                }
                                                if (!retcode) ispass = false;
                                                else ApplyConfiguration.ScreenOrientation = property.Value.ToString();
                                                writelog($"ScreenOrientation={ApplyConfiguration.ScreenOrientation}");
                                                //}
                                                //else
                                                //{
                                                //    writelog($"ScreenOrientation VCP not support");
                                                //    output += $"\n  \"Result: \": \"ScreenOrientation VCP not support\"";
                                                //}
                                                break;

                                            case "ACTIVEINPUTSOURCE":
                                                writelog($"ActiveInputSource entry");
                                                if (monitor.CapabilityDic.ContainsKey("60"))
                                                {
                                                    retcode = SetVCPCode(devMgr, monitor, "0x60", get_InputSource_code(get_inputsource_type(property.Value.ToString().ToUpper()).ToString())).Result;
                                                    if (!retcode) ispass = false;
                                                    else ApplyConfiguration.ActiveInputSource = property.Value.ToString();
                                                    writelog($"ActiveInputSource={ApplyConfiguration.ActiveInputSource}");
                                                }
                                                else
                                                {
                                                    writelog($"ActiveInputSource VCP not support");
                                                    //output += $"\n  \"Result: \": \"ActiveInputSource VCP not support\"";
                                                    resultMessages.Add("ActiveInputSource VCP not support");
                                                }
                                                break;

                                            case "RESOLUTION":
                                                writelog($"RESOLUTION entry");
                                                //if (monitor.CapabilityDic.ContainsKey("AA") && monitor.CapabilityDic["AA"] != null && monitor.CapabilityDic["AA"].Contains("00"))
                                                //{
                                                string[] ss = property.Value.ToString().Split(" ");
                                                displayProperties = new Properties() { Resolutions_Width = int.Parse(ss[0]), Resolutions_High = int.Parse(ss[2]), Frequency = int.Parse(ss[4].Split(".00HZ")[0]) };
                                                retcode = devMgr.SetDisplayPropertiest(monitor, displayProperties, displayPropertiesInfo.CurrentOrientation).Result;
                                                if (!retcode) ispass = false;
                                                else ApplyConfiguration.Resolution = property.Value.ToString();
                                                writelog($"RESOLUTION={ApplyConfiguration.Resolution}");
                                                //}
                                                //else
                                                //{
                                                //    writelog($"RESOLUTION VCP not support");
                                                //    output += $"\n  \"Result: \": \"RESOLUTION VCP not support\"";
                                                //}
                                                break;

                                            //case "AspectRatio":

                                            //    int gcd = (int)GCD((ulong)displayProperties.Resolutions_Width, (ulong)displayProperties.Resolutions_High);
                                            //    ApplyConfiguration.AspectRatio = $"{displayProperties.Resolutions_Width / gcd}:{displayProperties.Resolutions_High / gcd}";
                                            //    writelog($"AspectRatio={ApplyConfiguration.AspectRatio}");
                                            //    break;

                                            case "CONTRASTLEVEL":
                                                writelog($"ContrastLevel entry");
                                                if (monitor.CapabilityDic.ContainsKey("12"))
                                                {
                                                    retcode = SetVCPCode(devMgr, monitor, "0x12", property.Value.ToString().Substring(0, property.Value.ToString().Length - 1)).Result;
                                                    if (!retcode) ispass = false;
                                                    else ApplyConfiguration.ContrastLevel = property.Value.ToString();
                                                    writelog($"ContrastLevel={ApplyConfiguration.ContrastLevel}");
                                                }
                                                else
                                                {
                                                    writelog($"ContrastLevel VCP not support");
                                                    //output += $"\n  \"Result: \": \"ContrastLevel VCP not support\"";
                                                    resultMessages.Add("ContrastLevel VCP not support");
                                                }
                                                break;

                                            case "BRIGHTNESSLEVEL":
                                                writelog($"BrightnessLevel entry");
                                                if (monitor.CapabilityDic.ContainsKey("10"))
                                                {
                                                    retcode = SetVCPCode(devMgr, monitor, "0x10", property.Value.ToString().Substring(0, property.Value.ToString().Length - 1)).Result;
                                                    if (!retcode) ispass = false;
                                                    else ApplyConfiguration.BrightnessLevel = property.Value.ToString();
                                                    writelog($"BrightnessLevel={ApplyConfiguration.BrightnessLevel}");
                                                }
                                                else
                                                {
                                                    writelog($"BrightnessLevel VCP not support");
                                                    //output += $"\n  \"Result: \": \"BrightnessLevel VCP not support\"";
                                                    resultMessages.Add("BrightnessLevel VCP not support");
                                                }
                                                break;

                                            case "LUMINANCELEVEL":
                                                writelog($"LuminanceLevel entry");
                                                if (!monitor.CapabilityDic.ContainsKey("10"))
                                                {
                                                    retcode = SetVCPCode(devMgr, monitor, "0x10", property.Value.ToString().Substring(0, property.Value.ToString().Length - 1)).Result;
                                                    if (!retcode) ispass = false;
                                                    else ApplyConfiguration.LuminanceLevel = property.Value.ToString();
                                                    writelog($"LuminanceLevel={ApplyConfiguration.LuminanceLevel}");
                                                }
                                                else
                                                {
                                                    writelog($"LuminanceLevel VCP not support");
                                                    //output += $"\n  \"Result: \": \"LuminanceLevel VCP not support\"";
                                                    resultMessages.Add("LuminanceLevel VCP not support");
                                                }
                                                break;

                                            case "AUTOBRIGHTNESS":
                                                writelog($"AutoBrightness entry");
                                                if (monitor.CapabilityDic.ContainsKey("66"))
                                                {
                                                    retcode = await devMgr.SetALSFeatureValue(monitor, param, ALSFeatureQueryType.AutoBrightness, property.Value.ToString().ToUpper());
                                                    if (!retcode) ispass = false;
                                                    else ApplyConfiguration.AutoBrightness = property.Value.ToString();
                                                    writelog($"AutoBrightness={ApplyConfiguration.AutoBrightness}");
                                                }
                                                else
                                                {
                                                    writelog($"AutoBrightness VCP not support");
                                                    //output += $"\n  \"Result: \": \"AutoBrightness VCP not support\"";
                                                    resultMessages.Add("AutoBrightness VCP not support");
                                                }
                                                break;

                                            case "AUTOBRIGHTNESSRANGELEVEL":
                                                writelog($"AutoBrightnessRangeLevel entry");
                                                if (monitor.CapabilityDic.ContainsKey("66"))
                                                {
                                                    retcode = await devMgr.SetALSFeatureValue(monitor, param, ALSFeatureQueryType.AutoBrightnessRangeLevel, get_RangeLevel(property.Value.ToString().ToUpper()));
                                                    if (!retcode) ispass = false;
                                                    else ApplyConfiguration.AutoBrightnessRangeLevel = property.Value.ToString();
                                                    writelog($"AutoBrightnessRangeLevel={ApplyConfiguration.AutoBrightnessRangeLevel}");
                                                }
                                                else
                                                {
                                                    writelog($"AutoBrightnessRangeLevel VCP not support");
                                                    //output += $"\n  \"Result: \": \"AutoBrightnessRangeLevel VCP not support\"";
                                                    resultMessages.Add("AutoBrightnessRangeLevel VCP not support");
                                                }
                                                break;

                                            case "AUTOCOLORTEMP":
                                                writelog($"AutoColorTemp entry");
                                                if (monitor.CapabilityDic.ContainsKey("66"))
                                                {
                                                    retcode = await devMgr.SetALSFeatureValue(monitor, param, ALSFeatureQueryType.AutoColorTemperature, property.Value.ToString().ToUpper());
                                                    if (!retcode) ispass = false;
                                                    else ApplyConfiguration.AutoColorTemp = property.Value.ToString();
                                                    writelog($"AutoColorTemp={ApplyConfiguration.AutoColorTemp}");
                                                }
                                                else
                                                {
                                                    writelog($"AutoColorTemp VCP not support");
                                                    //output += $"\n  \"Result: \": \"AutoColorTemp VCP not support\"";
                                                    resultMessages.Add("AutoColorTemp VCP not support");
                                                }
                                                break;

                                            case "PRIMARYMONITORFORSYNC":
                                                writelog($"PrimaryMonitorForSync entry");
                                                if (monitor.CapabilityDic.ContainsKey("66"))
                                                {
                                                    retcode = await devMgr.SetALSFeatureValue(monitor, param, ALSFeatureQueryType.PrimaryMonitorSync, property.Value.ToString().ToUpper());
                                                    if (!retcode) ispass = false;
                                                    else ApplyConfiguration.PrimaryMonitorForSync = property.Value.ToString();
                                                    writelog($"PrimaryMonitorForSync={ApplyConfiguration.PrimaryMonitorForSync}");
                                                }
                                                else
                                                {
                                                    writelog($"PrimaryMonitorForSync VCP not support");
                                                    //output += $"\n  \"Result: \": \"PrimaryMonitorForSync VCP not support\"";
                                                    resultMessages.Add("PrimaryMonitorForSync VCP not support");
                                                }
                                                break;

                                            case "USB_CPRIORITIZATION":
                                                if (displayPropertiesInfo.SupportedUSBCPrioritization)
                                                {
                                                    USBCPrioritizationType gettype = get_USBCPrioritization(property.Value.ToString());
                                                    if (gettype != USBCPrioritizationType.Unknow)
                                                    {
                                                        retcode = devMgr.SetUSBCPrioritizationType(monitor, gettype).Result;
                                                        if (!retcode) ispass = false;
                                                        else ApplyConfiguration.USB_CPrioritization = property.Value.ToString();
                                                        writelog($"USB_CPrioritization={ApplyConfiguration.USB_CPrioritization}");
                                                    }
                                                }
                                                else
                                                {
                                                    ApplyConfiguration.USB_CPrioritization = "NOT SUPPORT";
                                                    //output += $"\n  \"Result: \": \"USB_CPrioritization not support\"";
                                                    resultMessages.Add("USB_CPrioritization not support");
                                                }

                                                break;
                                            case "COLORMANAGEMENT":
                                                ColorManagementRunType colorManagementType = get_ColorManagement(property.Value.ToString());
                                                switch (colorManagementType)
                                                {
                                                    case ColorManagementRunType.Off:
                                                    case ColorManagementRunType.Byhost:
                                                    case ColorManagementRunType.Bymonitor:
                                                        writelog($"ColorManagement {colorManagementType.ToString()} entry");
                                                        retcode = devMgr.AutoColorManagementForMonitorConfig(monitor, colorManagementType.ToString().ToUpper(), string.Empty, string.Empty).Result;
                                                        if (!retcode) ispass = false;
                                                        else ApplyConfiguration.ColorManagement = property.Value.ToString();
                                                        writelog($"ColorManagement={ApplyConfiguration.ColorManagement}");
                                                        break;
                                                    default:
                                                        writelog($"option value not support");
                                                        ApplyConfiguration.ColorManagement = "NOT SUPPORT";
                                                        //output += $"\n  \"Result: \": \"ColorManagement not support\"";
                                                        resultMessages.Add("ColorManagement not support");
                                                        break;
                                                }
                                                break;
                                            case "SPEAKERMICROPHONE":
                                                writelog($"SpeakerMicrophone entry");
                                                if (CheckSpeakerSupported(monitor, commandLineInput) && CheckMicrophoneSupported(monitor, commandLineInput))
                                                {
                                                    rc = GetVCPCode(devMgr, monitor, "0x62").Result;
                                                    int getvalue = Convert.ToInt32(rc.value);
                                                    retcode = SetVCPCode(devMgr, monitor, "0x62", get_SpeakerMicrophone(property.Value.ToString(), getvalue)).Result;

                                                    //rc = GetVCPCode(devMgr, monitor, "0x8D").Result;
                                                    //int getvalue2 = Convert.ToInt32(rc.value);
                                                    //bool retcode2 = SetVCPCode(devMgr, monitor, "0x8D", get_SpeakerMicrophone(property.Value.ToString(), getvalue2)).Result;

                                                    if (!retcode) ispass = false;
                                                    else ApplyConfiguration.SpeakerMicrophone = property.Value.ToString();
                                                    writelog($"SpeakerMicrophone={ApplyConfiguration.SpeakerMicrophone}");
                                                }
                                                else
                                                {
                                                    ApplyConfiguration.SpeakerMicrophone = "N/A";
                                                    writelog($"SpeakerMicrophone VCP not support");
                                                    //output += $"\n  \"Result: \": \"SpeakerMicrophone VCP not support\"";
                                                    resultMessages.Add("SpeakerMicrophone VCP not support");
                                                }

                                                break;

                                            case "SPEAKERVOLUME":
                                                writelog($"SpeakerVolume entry");
                                                if (CheckSpeakerSupported(monitor, commandLineInput))
                                                {
                                                    rc = GetVCPCode(devMgr, monitor, "0x62").Result;
                                                    int getvalue = Convert.ToInt32(rc.value);
                                                    string setvalue = get_SpeakerVolume(property.Value.ToString(), getvalue);
                                                    if (setvalue != "unknown_command")
                                                        retcode = SetVCPCode(devMgr, monitor, "0x62", setvalue).Result;
                                                    else
                                                        retcode = SetVCPCode(devMgr, monitor, "0x62", property.Value.ToString()).Result;
                                                    if (!retcode) ispass = false;
                                                    else ApplyConfiguration.SpeakerVolume = property.Value.ToString();
                                                    writelog($"SpeakerVolume={ApplyConfiguration.SpeakerVolume}");
                                                }
                                                else
                                                {
                                                    ApplyConfiguration.SpeakerVolume = "N/A";
                                                    writelog($"SpeakerVolume VCP not support");
                                                    //output += $"\n  \"Result: \": \"SpeakerVolume VCP not support\"";
                                                    resultMessages.Add("SpeakerVolume VCP not support");
                                                }

                                                break;

                                            case "MICROPHONECONTROL":
                                                writelog($"MicrophoneControl entry");
                                                if (CheckMicrophoneSupported(monitor, commandLineInput))
                                                {
                                                    rc = GetVCPCode(devMgr, monitor, "0x8D").Result;
                                                    int getvalue = Convert.ToInt32(rc.value);
                                                    retcode = SetVCPCode(devMgr, monitor, "0x8D", get_MicrophoneControl(property.Value.ToString(), getvalue)).Result;
                                                    if (!retcode) ispass = false;
                                                    else ApplyConfiguration.MicrophoneControl = property.Value.ToString();
                                                    writelog($"MicrophoneControl={ApplyConfiguration.MicrophoneControl}");
                                                }
                                                else
                                                {
                                                    ApplyConfiguration.MicrophoneControl = "N/A";
                                                    writelog($"MicrophoneControl VCP not support");
                                                    //output += $"\n  \"Result: \": \"MicrophoneControl VCP not support\"";
                                                    resultMessages.Add("MicrophoneControl VCP not support");
                                                }

                                                break;

                                            //case "UNIFORMITY":
                                            //    writelog($"Uniformity entry");
                                            //    if (monitor.CapabilityDic.ContainsKey("E4"))
                                            //    {
                                            //        retcode = SetVCPCode(devMgr, monitor, "0xE4", get_Uniformity(property.Value.ToString())).Result;
                                            //    if (!retcode) ispass = false;
                                            //        else ApplyConfiguration.Uniformity = property.Value.ToString();
                                            //        writelog($"Uniformity={ApplyConfiguration.Uniformity}");
                                            //    }
                                            //    else
                                            //    {
                                            //        ApplyConfiguration.Uniformity = "N/A";
                                            //        writelog($"Uniformity VCP not support");
                                            //        output += $"\n  \"Result: \": \"Uniformity VCP not support\"";
                                            //    }

                                            //    break;

                                            case "POWERNAP":
                                                writelog($"PowerNap entry");
                                                await SetPowerNapAsync(get_PowerNapType_code(property.Value.ToString()), devMgr, monitor.edid.ModelName, monitor.edid.SerialNumber, monitor.edid.ServiceTag);
                                                //PowerNapSetting setting = new PowerNapSetting
                                                //{
                                                //    Status = false,
                                                //    ModelName = monitor.edid.ModelName,
                                                //    SerialNumber = monitor.edid.SerialNumber,
                                                //    RunType = get_PowerNapType_code(property.Value.ToString())
                                                //};
                                                //retcode = await devMgr.SavePowerNapSetting(setting);
                                                //UpdateUINotify off = new UpdateUINotify();
                                                //off.UI_Field_Name = "POWERNAP;"+ property.Value.ToString();
                                                //devMgr.OnUIUpdateNotify(off);
                                                if (!retcode) ispass = false;
                                                else ApplyConfiguration.PowerNap = property.Value.ToString();
                                                writelog($"PowerNap={ApplyConfiguration.PowerNap}");
                                                break;

                                            case "OSD_LANGUAGE":
                                                writelog($"OSD_language entry");
                                                if (monitor.CapabilityDic.ContainsKey("CC"))
                                                {
                                                    retcode = SetVCPCode(devMgr, monitor, "0xCC", GetOSDLanguage_index(property.Value.ToString()).ToString()).Result;
                                                    writelog($"OSD_language={GetOSDLanguage_index(property.Value.ToString()).ToString()}");
                                                    if (!retcode) ispass = false;
                                                    else ApplyConfiguration.OSD_language = property.Value.ToString();
                                                    writelog($"OSD_language={ApplyConfiguration.OSD_language}");
                                                }
                                                else
                                                {
                                                    writelog($"OSD_language VCP not support");
                                                    //output += $"\n  \"Result: \": \"OSD_language VCP not support\"";
                                                    resultMessages.Add("OSD_language VCP not support");
                                                }

                                                break;

                                            default:
                                                break;
                                        }
                                    }

                                    if (ispass)
                                    {
                                        ApplyConfiguration.Result = "PASS";
                                        ApplyConfiguration.Message = "N/A";
                                    }
                                    else
                                    {
                                        ApplyConfiguration.Result = "FAIL";
                                        ApplyConfiguration.Message = "N/A";
                                    }
                                    System.Console.WriteLine(JsonConvert.SerializeObject(ApplyConfiguration, Formatting.Indented));
                                    output += "\n" + JsonConvert.SerializeObject(ApplyConfiguration, Formatting.Indented);
                                }
                            }
                            break;
                        case "KEYBOARD"://202503012 Elsa add for Deviceconfiguration support guid&model
                            writelog("KEYBOARD set entry");
                            if (commandLineInput.GuidString != null && commandLineInput.GuidString.Count > 0 && !string.IsNullOrEmpty(commandLineInput.GuidString[0].ToString()))
                            {
                                var math = _deviceinfo.SingleOrDefault(x => x.ID.ToString().Equals(commandLineInput.GuidString[0].ToString(), StringComparison.OrdinalIgnoreCase));
                                if (math != null)
                                {
                                    mathdevs.Add(math);
                                    if (mathdevs[0].LogicalDeviceType.Contains(ss_1[0], StringComparison.OrdinalIgnoreCase))
                                    {
                                        ispass = DeviceconfigurationSetKeyboard(jsonObject, true, resultMessages, mathdevs[0]);
                                    }
                                    var response = GetDeviceDataPeripheralResponse(0, mathdevs[0]);
                                    output += "\n" + response.ToJson();
                                    break;
                                }
                                else
                                {
                                    math_found = false;
                                    ispass = false;
                                    resultMessages.Add("Invalid guid.");
                                }
                            }
                            else if (commandLineInput.Model != null && commandLineInput.Model.Count > 0 && !string.IsNullOrEmpty(commandLineInput.Model[0].ToString()))
                            {
                                var math = _deviceinfo.FindAll(x => x.ModelNumber.ToString().Equals(commandLineInput.Model[0].ToString(), StringComparison.OrdinalIgnoreCase));
                                if (math.Count != 0)
                                {
                                    mathdevs.AddRange(math);
                                    foreach (var device in mathdevs)
                                    {
                                        if (device.LogicalDeviceType.Contains(ss_1[0], StringComparison.OrdinalIgnoreCase))
                                        {
                                            ispass = DeviceconfigurationSetKeyboard(jsonObject, ispass, resultMessages, device);
                                            var response = GetDeviceDataPeripheralResponse(index, mathdevs[index]);
                                            output += "\n" + response.ToJson();
                                            index++;
                                        }
                                    }
                                }
                                else
                                {
                                    math_found = false;
                                    ispass = false;
                                    resultMessages.Add("Invalid model.");
                                }
                            }
                            else
                            {
                                foreach (var device in _deviceinfo)
                                {
                                    if (device.LogicalDeviceType.Contains(ss_1[0], StringComparison.OrdinalIgnoreCase))
                                    {
                                        ispass = DeviceconfigurationSetKeyboard(jsonObject, ispass, resultMessages, device);
                                        index++;
                                        var response = GetDeviceDataPeripheralResponse(index, device);
                                        output += "\n" + response.ToJson();
                                    }
                                }
                            }
                            break;
                        case "WEBCAM"://202503012 Elsa add for Deviceconfiguration support guid&model
                            writelog("WEBCAM set entry");
                            if (commandLineInput.GuidString != null && commandLineInput.GuidString.Count > 0 && !string.IsNullOrEmpty(commandLineInput.GuidString[0].ToString()))
                            {
                                //foreach (var propertys in jsonObject.Properties())
                                //{
                                //if (propertys.Name.ToString().ToUpper() == "ID" && propertys.Value.ToString().ToUpper() == commandLineInput.GuidString[0].ToString().ToUpper())
                                //{
                                var math = _deviceinfo.SingleOrDefault(x => x.ID.ToString().Equals(commandLineInput.GuidString[0].ToString(), StringComparison.OrdinalIgnoreCase));
                                if (math != null)
                                {
                                    mathdevs.Add(math);
                                    if (mathdevs[0].LogicalDeviceType.Contains(ss_1[0], StringComparison.OrdinalIgnoreCase))
                                    {
                                        ispass = DeviceconfigurationSetWebcam(jsonObject, true, resultMessages, mathdevs[0]);
                                    }
                                    var response = GetDeviceDataPeripheralResponse(0, mathdevs[0]);
                                    output += "\n" + response.ToJson();
                                    break;
                                }
                                //}
                                //}
                                else
                                {
                                    math_found = false;
                                    ispass = false;
                                    resultMessages.Add("Invalid guid.");
                                }
                            }
                            else if (commandLineInput.Model != null && commandLineInput.Model.Count > 0 && !string.IsNullOrEmpty(commandLineInput.Model[0].ToString()))
                            {
                                var math = _deviceinfo.FindAll(x => x.ModelNumber.ToString().Equals(commandLineInput.Model[0].ToString(), StringComparison.OrdinalIgnoreCase));
                                if (math.Count != 0)
                                {
                                    mathdevs.AddRange(math);
                                    foreach (var device in mathdevs)
                                    {
                                        if (device.LogicalDeviceType.Contains(ss_1[0], StringComparison.OrdinalIgnoreCase))
                                        {
                                            ispass = DeviceconfigurationSetWebcam(jsonObject, ispass, resultMessages, device);
                                            var response = GetDeviceDataPeripheralResponse(index, mathdevs[index]);
                                            output += "\n" + response.ToJson();
                                            index++;
                                        }
                                    }
                                }
                                else
                                {
                                    math_found = false;
                                    ispass = false;
                                    resultMessages.Add("Invalid model.");
                                }
                            }
                            else
                            {
                                foreach (var device in _deviceinfo)
                                {
                                    if (device.LogicalDeviceType.Contains(ss_1[0], StringComparison.OrdinalIgnoreCase))
                                    {
                                        ispass = DeviceconfigurationSetWebcam(jsonObject, ispass, resultMessages, device);
                                        index++;
                                        var response = GetDeviceDataPeripheralResponse(index, device);
                                        output += "\n" + response.ToJson();
                                    }
                                }
                            }
                            break;
                        case "WIREDAUDIO":
                        case "HEADSET":
                        case "AUDIO"://202503012 Elsa add for Deviceconfiguration support guid&model
                            writelog("AUDIO set entry");
                            if (commandLineInput.GuidString != null && commandLineInput.GuidString.Count > 0 && !string.IsNullOrEmpty(commandLineInput.GuidString[0].ToString()))
                            {
                                var math = _deviceinfo.SingleOrDefault(x => x.ID.ToString().Equals(commandLineInput.GuidString[0].ToString(), StringComparison.OrdinalIgnoreCase));
                                if (math != null)
                                {
                                    mathdevs.Add(math);
                                    if (mathdevs[0].LogicalDeviceType.Contains(ss_1[0], StringComparison.OrdinalIgnoreCase))
                                    {
                                        ispass = DeviceconfigurationSetAudio(jsonObject, true, resultMessages, mathdevs[0]);
                                    }
                                    var response = GetDeviceDataPeripheralResponse(0, mathdevs[0]);
                                    output += "\n" + response.ToJson();
                                    break;
                                }
                                else
                                {
                                    math_found = false;
                                    ispass = false;
                                    resultMessages.Add("Invalid guid.");
                                }
                            }
                            else if (commandLineInput.Model != null && commandLineInput.Model.Count > 0 && !string.IsNullOrEmpty(commandLineInput.Model[0].ToString()))
                            {
                                var math = _deviceinfo.FindAll(x => x.ModelNumber.ToString().Equals(commandLineInput.Model[0].ToString(), StringComparison.OrdinalIgnoreCase));
                                if (math.Count != 0)
                                {
                                    mathdevs.AddRange(math);
                                    foreach (var device in mathdevs)
                                    {
                                        if (device.LogicalDeviceType.Contains(ss_1[0], StringComparison.OrdinalIgnoreCase))
                                        {
                                            ispass = DeviceconfigurationSetAudio(jsonObject, ispass, resultMessages, device);
                                            var response = GetDeviceDataPeripheralResponse(index, mathdevs[index]);
                                            output += "\n" + response.ToJson();
                                            index++;
                                        }
                                    }
                                }
                                else
                                {
                                    math_found = false;
                                    ispass = false;
                                    resultMessages.Add("Invalid model.");
                                }
                            }
                            else
                            {
                                foreach (var device in _deviceinfo)
                                {
                                    if (device.LogicalDeviceType.Contains(ss_1[0], StringComparison.OrdinalIgnoreCase) || device.LogicalDeviceType.Contains("HEADSET", StringComparison.OrdinalIgnoreCase))
                                    {
                                        ispass = DeviceconfigurationSetAudio(jsonObject, ispass, resultMessages, device);
                                        index++;
                                        var response = GetDeviceDataPeripheralResponse(index, device);
                                        output += "\n" + response.ToJson();
                                    }
                                }
                            }
                            break;
                        case "MOUSE":
                        case "PEN":
                        case "DOCK"://202503012 Elsa add for Deviceconfiguration support guid&ServiceTag&model
                            if (commandLineInput.GuidString != null && commandLineInput.GuidString.Count > 0 && !string.IsNullOrEmpty(commandLineInput.GuidString[0].ToString()))
                            {
                                var math = _deviceinfo.SingleOrDefault(x => x.ID.ToString().Equals(commandLineInput.GuidString[0].ToString(), StringComparison.OrdinalIgnoreCase));
                                if (math != null)
                                {
                                    mathdevs.Add(math);
                                    if (mathdevs[0].LogicalDeviceType.Contains(ss_1[0], StringComparison.OrdinalIgnoreCase))
                                    {
                                        var response = GetDeviceDataPeripheralResponse(0, mathdevs[0]);
                                        output += "\n" + response.ToJson();
                                        break;
                                    }
                                }
                                else
                                {
                                    math_found = false;
                                    ispass = false;
                                    resultMessages.Add("Invalid guid.");
                                }
                            }
                            else if (commandLineInput.ServiceTag != null && commandLineInput.ServiceTag.Count > 0 && !string.IsNullOrEmpty(commandLineInput.ServiceTag[0].ToString()))
                            {
                                var math = _deviceinfo.SingleOrDefault(x => x.DockServiceTag.ToString().Equals(commandLineInput.ServiceTag[0].ToString(), StringComparison.OrdinalIgnoreCase));
                                if (math != null)
                                {
                                    mathdevs.Add(math);
                                    if (mathdevs[0].LogicalDeviceType.Contains(ss_1[0], StringComparison.OrdinalIgnoreCase))
                                    {
                                        var response = GetDeviceDataPeripheralResponse(0, mathdevs[0]);
                                        output += "\n" + response.ToJson();
                                        break;
                                    }
                                }
                                else
                                {
                                    math_found = false;
                                    ispass = false;
                                    resultMessages.Add("Invalid ServiceTag.");
                                }
                            }
                            else if (commandLineInput.Model != null && commandLineInput.Model.Count > 0 && !string.IsNullOrEmpty(commandLineInput.Model[0].ToString()))
                            {
                                var math = _deviceinfo.FindAll(x => x.ModelNumber.ToString().Equals(commandLineInput.Model[0].ToString(), StringComparison.OrdinalIgnoreCase));
                                if (math.Count != 0)
                                {
                                    mathdevs.AddRange(math);
                                    foreach (var device in mathdevs)
                                    {
                                        if (device.LogicalDeviceType.Contains(ss_1[0], StringComparison.OrdinalIgnoreCase))
                                        {
                                            var response = GetDeviceDataPeripheralResponse(index, mathdevs[index]);
                                            output += "\n" + response.ToJson();
                                            index++;
                                        }
                                    }
                                }
                                else
                                {
                                    math_found = false;
                                    ispass = false;
                                    resultMessages.Add("Invalid model.");
                                }
                            }
                            else
                            {
                                foreach (var device in _deviceinfo)
                                {
                                    if (device.LogicalDeviceType.Contains(ss_1[0], StringComparison.OrdinalIgnoreCase))
                                    {
                                        index++;
                                        var response = GetDeviceDataPeripheralResponse(index, device);
                                        output += "\n" + response.ToJson();
                                    }
                                }
                            }
                            break;
                        default:
                            CLI_RESPONSE3 cli_Response__ = new CLI_RESPONSE3();
                            cli_Response__.Command = commandLineInput.Command;
                            cli_Response__.TargetFeature = commandLineInput.TargetFeature;
                            cli_Response__.Result = "FAIL";
                            cli_Response__.Message = "Invalid command line syntax or missing -value=file.json";
                            return ((int)CLI_ExitCode.invalide_cmdline_syntax, cli_Response__.ToJson());
                    }
                    if (string.IsNullOrWhiteSpace(output) && math_found == true)
                    {
                        output = new CLI_RESPONSE3
                        {
                            Command = commandLineInput.Command,
                            TargetFeature = commandLineInput.TargetFeature,
                            Message = "No device found"
                        }.ToJson();
                    }
                    else
                    {
                        output = new SetDeviceConfigResponse
                        {
                            Command = commandLineInput.Command,
                            TargetFeature = commandLineInput.TargetFeature,
                            Result = ispass ? "PASS" : "FAIL",
                            Message = resultMessages
                        }.ToJson() + output;
                    }
                    return (ispass ? (int)CLI_ExitCode.success : (int)CLI_ExitCode.functional_error, output);
                }
            }
            else
            {
                CLI_RESPONSE cli_Response___ = new CLI_RESPONSE();
                cli_Response___.Command = commandLineInput.Command;
                cli_Response___.TargetFeature = commandLineInput.TargetFeature;
                cli_Response___.Result = "FAIL";
                cli_Response___.Message = "Invalid command line syntax or missing -value=file.json";
                return ((int)CLI_ExitCode.invalide_cmdline_syntax, cli_Response___.ToJson());
            }
            CLI_RESPONSE cli_Response = new CLI_RESPONSE();
            cli_Response.Command = commandLineInput.Command;
            cli_Response.TargetFeature = commandLineInput.TargetFeature;
            cli_Response.Result = "FAIL";
            cli_Response.Message = "Invalid command line syntax or missing -value=file.json";
            return ((int)CLI_ExitCode.invalide_cmdline_syntax, cli_Response.ToJson());

        }

        private bool DeviceconfigurationSetAudio(JObject jsonObject, bool ispass, List<string> resultMessages, DeviceInfo device)
        {
            foreach (var property in jsonObject.Properties())
            {
                writelog($"Key: {property.Name}, Value: {property.Value}");

                switch (property.Name.ToString())
                {
                    case "ANCMODE":
                        writelog("ANCMODE entry");
                        writelog("_devMgr.GetIsANCSupportedAsync entry");
                        if (_devMgr.GetIsANCSupportedAsync(device.ID.ToString()).Result)
                        {
                            if (property.Value.ToString() == "ON")
                            {
                                writelog("_devMgr.SetAncMode entry");
                                _devMgr.SetAncMode(1, device.ID);
                                writelog("_devMgr.SetAncMode exit");
                            }
                            else if (property.Value.ToString() == "OFF")
                            {
                                writelog("_devMgr.SetAncMode entry");
                                _devMgr.SetAncMode(0, device.ID);
                                writelog("_devMgr.SetAncMode exit");
                            }
                            else
                            {
                                resultMessages.Add("ANCMODE is wrong value");
                                ispass = false;
                            }
                        }
                        else
                        {
                            resultMessages.Add("ANCMODE not support");
                        }
                        break;
                    case "MICNOISECANCELLATION":
                        writelog("MICNOISECANCELLATION entry");
                        writelog("_devMgr.GetIsMicNoiseCancellationSupportedAsync entry");
                        if (GlobalDefinitions.isSupport210)
                        {
                            var isAirAudio = device.LogicalDeviceType.Equals("LogicalAirAudio", System.StringComparison.OrdinalIgnoreCase);
                            if (isAirAudio)
                            {
                                var Airsupport = _devMgr.GetAirAudioIsMicNoiseCancellationSupportedAsync(device.ID.ToString()).Result;

                                if (Airsupport)
                                {
                                    if (property.Value.ToString() == "ON")
                                    {
                                        if (isAirAudio)
                                        {
                                            writelog("_devMgr.SetAirAudioMicNoiseCancellationAsync entry");
                                            _devMgr.SetAirAudioMicNoiseCancellationAsync(device.ID.ToString(), true);
                                            writelog("_devMgr.SetAirAudioMicNoiseCancellationAsync exit");
                                        }
                                        else
                                        {
                                            writelog("_devMgr.SetMicNoiseCancellation entry");
                                            _devMgr.SetMicNoiseCancellation(true, device.ID);
                                            writelog("_devMgr.SetMicNoiseCancellation exit");
                                        }
                                    }
                                    else if (property.Value.ToString() == "OFF")
                                    {
                                        if (isAirAudio)
                                        {
                                            writelog("_devMgr.SetAirAudioMicNoiseCancellationAsync entry");
                                            _devMgr.SetAirAudioMicNoiseCancellationAsync(device.ID.ToString(), true);
                                            writelog("_devMgr.SetAirAudioMicNoiseCancellationAsync exit");
                                        }
                                        else
                                        {
                                            writelog("_devMgr.SetMicNoiseCancellation entry");
                                            _devMgr.SetMicNoiseCancellation(false, device.ID);
                                            writelog("_devMgr.SetMicNoiseCancellation exit");
                                        }
                                    }
                                    else
                                    {
                                        resultMessages.Add("MICNOISECANCELLATION is wrong value");
                                        ispass = false;
                                    }
                                }
                                else
                                {
                                    resultMessages.Add("MICNOISECANCELLATION not support");
                                }
                                break; 
                            }
                        }
                        var support = device.IsMicNoiseCancellationSupported;
                        if (support)
                        {
                            if (property.Value.ToString() == "ON")
                            {
                                if (device.ModelNumber == "WL7024")
                                {
                                    writelog("_devMgr.SetMicNoiseCancellationForMito entry");
                                    _devMgr.SetMicNoiseCancellationForMito(true, device.ID);
                                    writelog("_devMgr.SetMicNoiseCancellationForMito exit");
                                }
                                else
                                {
                                    writelog("_devMgr.SetMicNoiseCancellation entry");
                                    _devMgr.SetMicNoiseCancellation(true, device.ID);
                                    writelog("_devMgr.SetMicNoiseCancellation exit");
                                }
                            }
                            else if (property.Value.ToString() == "OFF")
                            {
                                if (device.ModelNumber == "WL7024")
                                {
                                    writelog("_devMgr.SetMicNoiseCancellationForMito entry");
                                    _devMgr.SetMicNoiseCancellationForMito(false, device.ID);
                                    writelog("_devMgr.SetMicNoiseCancellationForMito exit");
                                }
                                else
                                {
                                    writelog("_devMgr.SetMicNoiseCancellation entry");
                                    _devMgr.SetMicNoiseCancellation(false, device.ID);
                                    writelog("_devMgr.SetMicNoiseCancellation exit");
                                }
                            }
                            else
                            {
                                resultMessages.Add("MICNOISECANCELLATION is wrong value");
                                ispass = false;
                            }
                        }
                        else
                        {
                            resultMessages.Add("MICNOISECANCELLATION not support");
                        }
                        
                        break;
                    case "WEARDETECTION":
                        writelog("WEARDETECTION entry");
                        writelog("_devMgr.GetIsWearDetectionSupportedAsync entry");
                        if (_devMgr.GetIsWearDetectionSupportedAsync(device.ID.ToString()).Result)
                        {
                            if (property.Value.ToString() == "ON")
                            {
                                writelog("_devMgr.SetWearDetectionForCLI entry");
                                _devMgr.SetWearDetectionForCLI(1, device.ID);
                                writelog("_devMgr.SetWearDetectionForCLI exit");
                            }
                            else if (property.Value.ToString() == "OFF")
                            {
                                writelog("_devMgr.SetWearDetectionForCLI entry");
                                _devMgr.SetWearDetectionForCLI(0, device.ID);
                                writelog("_devMgr.SetWearDetectionForCLI exit");
                            }
                            else
                            {
                                resultMessages.Add("WEARDETECTION is wrong value");
                                ispass = false;
                            }
                        }
                        else
                        {
                            resultMessages.Add("WEARDETECTION not support");
                        }
                        break;
                    default:
                        break;
                }
            }

            return ispass;
        }

        private bool DeviceconfigurationSetKeyboard(JObject jsonObject, bool ispass, List<string> resultMessages, DeviceInfo device)
        {
            foreach (var property in jsonObject.Properties())
            {
                writelog($"Key: {property.Name}, Value: {property.Value}");

                switch (property.Name.ToString())
                {
                    case "COLLABSCREENSHARE":
                        writelog("COLLABSCREENSHARE entry");
                        writelog("device.IsCollaborationScreenShareEnable entry");
                        if (device.IsCollabsKeysSupported)
                        {
                            if (property.Value.ToString() == "ON")
                            {
                                writelog("_devMgr.SetCollaborationScreenShareEnable entry");
                                _devMgr.SetCollaborationScreenShareEnable(true, device.ID);
                                writelog("_devMgr.SetCollaborationScreenShareEnable exit");
                            }
                            else if (property.Value.ToString() == "OFF")
                            {
                                writelog("_devMgr.SetCollaborationScreenShareEnable entry");
                                _devMgr.SetCollaborationScreenShareEnable(false, device.ID);
                                writelog("_devMgr.SetCollaborationScreenShareEnable exit");
                            }
                            else
                            {
                                resultMessages.Add("COLLABSCREENSHARE is wrong value");
                                ispass = false;
                            }
                        }
                        else
                        {
                            resultMessages.Add("COLLABSCREENSHARE not support");
                        }
                        break;
                    default:
                        break;
                }
            }

            return ispass;
        }

        private bool DeviceconfigurationSetWebcam(JObject jsonObject, bool ispass, List<string> resultMessages, DeviceInfo device)
        {
            foreach (var property in jsonObject.Properties())
            {
                writelog($"Key: {property.Name}, Value: {property.Value}");

                switch (property.Name.ToString())
                {
                    case "HDR":
                        writelog("HDR entry");
                        writelog("_devMgr.GetIsPropertyHDRSupported entry");
                        if (_devMgr.GetIsPropertyHDRSupported(device.ID.ToString()).Result)
                        {
                            if (property.Value.ToString() == "ON")
                            {
                                writelog("_devMgr.SetIsHDROn entry");
                                _devMgr.SetIsHDROn(device.ID.ToString(), true);
                                writelog("_devMgr.SetIsHDROn exit");
                            }
                            else if (property.Value.ToString() == "OFF")
                            {
                                writelog("_devMgr.SetIsHDROn entry");
                                _devMgr.SetIsHDROn(device.ID.ToString(), false);
                                writelog("_devMgr.SetIsHDROn exit");
                            }
                            else
                            {
                                resultMessages.Add("HDR is wrong value");
                                ispass = false;
                            }
                        }
                        else
                        {
                            resultMessages.Add("HDR not support");
                        }
                        break;
                    case "ANTIFLICKER":
                        writelog("ANTIFLICKER entry");
                        writelog("_devMgr.GetIsPropertyAntiFlickerSupported entry");
                        if (_devMgr.GetIsPropertyAntiFlickerSupported(device.ID.ToString()).Result)
                        {
                            if (property.Value.ToString() == "50" || property.Value.ToString() == "60")
                            {
                                var antiflickerValue = property.Value.ToString() == "50" ? 1 : 2;
                                writelog("_devMgr.SetAntiFlicker entry");
                                _devMgr.SetAntiFlicker(device.ID.ToString(), antiflickerValue);
                                writelog("_devMgr.SetAntiFlicker exit");
                            }
                            else
                            {
                                resultMessages.Add("ANTIFLICKER is wrong value");
                                ispass = false;
                            }
                        }
                        else
                        {
                            resultMessages.Add("ANTIFLICKER not support");
                        }
                        break;
                    case "MICSWITCH":
                        writelog("MICSWITCH entry");
                        writelog("device.IsMicEnumerationSupported entry");
                        if (device.IsMicEnumerationSupported)
                        {
                            if (property.Value.ToString() == "ON")
                            {
                                writelog("_devMgr.SetIsMicEnumerationOn entry");
                                _devMgr.SetIsMicEnumerationOn(true, device.ID);
                                writelog("_devMgr.SetIsMicEnumerationOn exit");
                            }
                            else if (property.Value.ToString() == "OFF")
                            {
                                writelog("_devMgr.SetIsMicEnumerationOn entry");
                                _devMgr.SetIsMicEnumerationOn(false, device.ID);
                                writelog("_devMgr.SetIsMicEnumerationOn exit");
                            }
                            else
                            {
                                resultMessages.Add("MICSWITCH is wrong value");
                                ispass = false;
                            }
                        }
                        else
                        {
                            resultMessages.Add("MICSWITCH not support");
                        }
                        break;
                    case "AIAUTOFRAMING":
                        writelog("AIAUTOFRAMING entry");
                        writelog("_devMgr.GetIsPropertyAutoFramingSupported entry");
                        if (_devMgr.GetIsPropertyAutoFramingSupported(device.ID.ToString()).Result)
                        {
                            if (property.Value.ToString() == "ON")
                            {
                                writelog("_devMgr.SetIsAutoFramingOn entry");
                                _devMgr.SetIsAutoFramingOn(device.ID.ToString(), true);
                                writelog("_devMgr.SetIsAutoFramingOn exit");
                            }
                            else if (property.Value.ToString() == "OFF")
                            {
                                writelog("_devMgr.SetIsAutoFramingOn entry");
                                _devMgr.SetIsAutoFramingOn(device.ID.ToString(), false);
                                writelog("_devMgr.SetIsAutoFramingOn exit");
                            }
                            else
                            {
                                resultMessages.Add("AIAUTOFRAMING is wrong value");
                                ispass = false;
                            }
                        }
                        else
                        {
                            resultMessages.Add("AIAUTOFRAMING not support");
                        }
                        break;
                    case "PRESENCEDETECTION":
                        writelog("PRESENCEDETECTION entry");
                        writelog("device.IsESISupported entry");
                        if (device.IsESISupported)
                        {
                            if (property.Value.ToString() == "ON")
                            {
                                writelog("_devMgr.SetIsProximitySensorEnable entry");
                                _devMgr.SetIsProximitySensorEnable(device.ID.ToString(), true);
                                writelog("_devMgr.SetIsProximitySensorEnable exit");
                            }
                            else if (property.Value.ToString() == "OFF")
                            {
                                writelog("_devMgr.SetIsProximitySensorEnable entry");
                                _devMgr.SetIsProximitySensorEnable(device.ID.ToString(), false);
                                writelog("_devMgr.SetIsProximitySensorEnable exit");
                            }
                            else
                            {
                                resultMessages.Add("PRESENCEDETECTION is wrong value");
                                ispass = false;
                            }
                        }
                        else
                        {
                            resultMessages.Add("PRESENCEDETECTION not support");
                        }
                        break;
                    default:
                        break;
                }
            }

            return ispass;
        }

        private static string get_ScreenOrientation_code(string Orientation)
        {
            switch (Orientation)
            {
                case "Landscape": return "1";
                case "Portrait": return "2";
                case "Landscape_flipped": return "3";
                case "Portrait_flipped": return "4";
                default: return "1";
            }
        }

        private static string get_RangeLevel(string level)
        {
            switch (level.ToUpper())
            {
                case "LOW": return "0";
                case "MID": return "1";
                case "HIGH": return "2";
                default: return "1";
            }
        }

        private bool CheckSpeakerSupported(MonitorInfo monitor, CommandLineInput commandLineInput)
        {
            bool supported = false;

            writelog("Check monitor is support speaker");
            //Console.WriteLine($"--------TEST---------- {monitor.CapabilityDic.TryGetValue("62", out List<string> value1)} {value1.Count}");
            //foreach (var test in value1)
            //    Console.WriteLine($"value: {test.ToString()} test: {value1.Count}");

            if (monitor.CapabilityDic.TryGetValue("62", out List<string> value) &&
                value != null &&
                value.Contains("FF") &&
                value.Contains("FE"))
            {
                //if (value.Count > 1)
                //{

                writelog("Monitor is support speaker");
                supported = true;
                //}
            }
            else
            {
                writelog("Monitor is not support speaker");
                return supported;
            }

            if (commandLineInput.Options.Count > 0 &&
                commandLineInput.Options[0].Option_Value.Contains("LOCK"))
            {
                writelog("Check monitor is support speaker lock/unlock");
                supported = value.Contains("C000");
                writelog(supported ? "Monitor is support speaker lock/unlock" : "Monitor is not support speaker lock/unlock");
            }

            return supported;
        }

        private bool CheckMicrophoneSupported(MonitorInfo monitor, CommandLineInput commandLineInput)
        {
            bool supported = false;

            writelog("Check monitor is support microphone");
            if (monitor.CapabilityDic.TryGetValue("8D", out List<string> value) &&
                value != null &&
                value.Contains("01") &&
                value.Contains("02"))
            {
                writelog("Monitor is support microphone");
                supported = true;
            }
            else
            {
                writelog("Monitor is not support microphone");
                return supported;
            }

            if (commandLineInput.Options.Count > 0 &&
                commandLineInput.Options[0].Option_Value.Contains("LOCK"))
            {
                writelog("Check monitor is support microphone lock/unlock");
                supported = value.Contains("C000");
                writelog(supported ? "Monitor is support microphone lock/unlock" : "Monitor is not support microphone lock/unlock");
            }

            return supported;
        }

        private static string get_MicrophoneControl(string status, int value)
        {
            switch (status.ToUpper())
            {
                case "OSDDISABLE": return ((value & 0xFF00) | 0x0001).ToString();       // "xx01"
                case "OSDENABLE": return ((value & 0xFF00) | 0x0002).ToString();        // "xx02"
                default: return "unknown_command";
            }
        }

        private static string get_MicrophoneControl_status(int value)
        {
            string output = string.Empty;
            output += ((value & 0x03) == 0x01) ? "OSDDISABLE" : "";
            output += ((value & 0x03) == 0x02) ? "OSDENABLE" : "";

            return output;
        }

        private static string get_SpeakerVolume(string status, int value)
        {
            switch (status.ToUpper())
            {
                case "OSDDISABLE": return ((value & 0xFF00) | 0x00FF).ToString();       // "xxFF"
                case "OSDENABLE": return ((value & 0xFF00) | 0x00FE).ToString();        // "xxFE"
                default: return "unknown_command";
            }
        }

        private static string get_SpeakerVolume_status(int value)
        {
            string output = string.Empty;

            int value_tmp = value & 0x00FF;
            if (value_tmp == 0xFF)
            {
                output += "OSDDISABLE";
            }
            else if (value_tmp == 0xFE)
            {
                output += "OSDENABLE";
            }
            else if (value_tmp < 0x65)
            {
                output = $"Volume:{value & 0xFF}";
            }
            return output;
        }

        private static string get_SpeakerMicrophone(string status, int value)
        {
            string binaryString = Convert.ToString(value, 2).PadLeft(16, '0');

            switch (status.ToUpper())
            {
                case "OSDDISABLE":
                    binaryString = ModifyBitInString(binaryString, 14, '1');
                    break;
                case "OSDENABLE":
                    binaryString = ModifyBitInString(binaryString, 14, '0');
                    break;
                case "OSDUNLOCK":
                    binaryString = ModifyBitInString(binaryString, 15, '0');
                    break;
                case "OSDLOCK":
                    binaryString = ModifyBitInString(binaryString, 15, '1');
                    break;
                case "OSDUNLOCK,OSDDISABLE":
                    binaryString = ModifyBitInString(binaryString, 15, '0');
                    binaryString = ModifyBitInString(binaryString, 14, '1');
                    break;
                case "OSDUNLOCK,OSDENABLE":
                    binaryString = ModifyBitInString(binaryString, 15, '0');
                    binaryString = ModifyBitInString(binaryString, 14, '0');
                    break;
                case "OSDLOCK,OSDDISABLE":
                    binaryString = ModifyBitInString(binaryString, 15, '1');
                    binaryString = ModifyBitInString(binaryString, 14, '1');
                    break;
                case "OSDLOCK,OSDENABLE":
                    binaryString = ModifyBitInString(binaryString, 15, '1');
                    binaryString = ModifyBitInString(binaryString, 14, '0');
                    break;
                default:
                    return "unknown_command";
            }

            return "0x" + Convert.ToInt32(binaryString, 2).ToString("X");
        }

        private static string ModifyBitInString(string binaryString, int position, char newBitValue)
        {
            char[] binaryArray = binaryString.ToCharArray();

            int arrayPosition = binaryArray.Length - 1 - position;
            binaryArray[arrayPosition] = newBitValue;

            return new string(binaryArray);
        }

        private static string get_SpeakerMicrophone_status(int value)
        {
            string binaryString = Convert.ToString(value, 2).PadLeft(16, '0');
            string output = string.Empty;
            output += binaryString[binaryString.Length - 1 - 15] == '1' ? "OSDLOCK," : "OSDUNLOCK,";
            output += binaryString[binaryString.Length - 1 - 14] == '1' ? "OSDDISABLE" : "OSDENABLE";

            return output;
        }

        private static string get_Uniformity(string status)
        {
            switch (status.ToUpper())
            {
                case "OFF": return "0";
                case "HIGH": return "1";
                case "LOW": return "2";
                case "ON": return "2";
                default: return "0";
            }
        }

        private static ColorManagementRunType get_ColorManagement(string priority)
        {
            switch (priority)
            {
                case "OFF":
                    return ColorManagementRunType.Off;
                case "BYMONITOR":
                    return ColorManagementRunType.Bymonitor;
                case "BYHOST":
                    return ColorManagementRunType.Byhost;
                default: return ColorManagementRunType.Off;
            }
        }

        private static USBCPrioritizationType get_USBCPrioritization(string priority)
        {
            switch (priority)
            {
                case "High Speed":
                case "High Data Speed":
                    return USBCPrioritizationType.HighDataSpeed;
                case "High Resolution": return USBCPrioritizationType.HighResolution;
                default: return USBCPrioritizationType.Unknow;
            }
        }

        private static PowerNapType get_PowerNapType_code(string code)
        {
            switch (code.ToUpper())
            {
                case "OFF": return PowerNapType.Off;
                case "REDUCEBRIGHTNESS": return PowerNapType.ReduceBrightness;
                case "SLEEP": return PowerNapType.SleepIfRunning;
                default: return PowerNapType.SleepIfRunning;
            }
        }

        private static string get_InputSource_code(string input)
        {
            switch (input)
            {
                case "VGA1": return "0x01";
                case "VGA2": return "0x02";
                case "DVI-1": return "0x03";
                case "DVI-2": return "0x04";
                case "Composite video 1": return "0x05";
                case "Composite video 2": return "0x06";
                case "S-Video-1": return "0x07";
                case "S-Video-2": return "0x08";
                case "Tuner-1": return "0x09";
                case "Tuner-2": return "0x0a";
                case "Tuner-3": return "0x0b";
                case "Component video (YPrPb/YCrCb) 1": return "0x0c";
                case "Component video (YPrPb/YCrCb) 2": return "0x0d";
                case "Component video (YPrPb/YCrCb) 3": return "0x0e";
                case "DISPLAYPORT1": return "0x0f";
                case "Mini DisplayPort-1": return "0x10";
                case "HDMI1": return "0x11";
                case "HDMI2": return "0x12";
                case "DISPLAYPORT2": return "0x13";
                case "Mini DisplayPort-2": return "0x14";
                case "HDMI3": return "0x15";
                case "HDMI4": return "0x16";
                case "DISPLAYPORT3": return "0x17";
                case "Mini DisplayPort-3": return "0x18";
                case "Thunderbolt1": return "0x19";
                case "Thunderbolt2": return "0x1a";
                case "USB-C1": return "0x1b";
                case "USB-C2": return "0x1c";
                case "USB-C3": return "0x1d";
                case "USB-C4": return "0x1e";
                case "USB Comm from USB1 (Type-B, port 1)": return "0x80";
                case "USB Comm from USB2 (Type-B, port 2)": return "0x81";
                case "USB Comm from USB-C1 (Type-C, port 1)": return "0x82";
                case "USB Comm from USB-C2 (Type-C, port 2)": return "0x83";
                case "USB Comm from USB-C3 (Type-C, port 3)": return "0x84";
                case "USB Comm from USB-C4 (Type-C, port 4)": return "0x85";
                default: return "0x11";
            }
        }
        private (int code, string result) SetPowerSetting(IDeviceManagerSA devMgr, CommandLineInput commandLineInput)
        {
            if (commandLineInput.Command == "GET" && commandLineInput.Options.Count == 0)
            {
                return PowerSettingv2(devMgr, commandLineInput).Result;
            }
            else if (commandLineInput.Command == "SET" && commandLineInput.Options.Count == 1)
            {
                return PowerSettingv2(devMgr, commandLineInput).Result;
            }
            else
            {
                CLI_RESPONSE cli_Response = new CLI_RESPONSE();
                cli_Response.Command = commandLineInput.Command;
                cli_Response.TargetFeature = commandLineInput.TargetFeature;
                cli_Response.Result = "FAIL";
                cli_Response.Message = "Invalid command line syntax, missing -value=... or more than one -value=...";
                return ((int)CLI_ExitCode.invalide_cmdline_syntax, cli_Response.ToJson());
            }
        }

        private (bool code, string result) SetPower(IDeviceManagerSA devMgr, MonitorInfo monitor, CommandLineInput commandLineInput)
        {
            string output = string.Empty;
            bool retcode = false;
            int somethingfail = 0;
            //ObjGetVCP rc = new ObjGetVCP();

            //List<int> _monitorIndeies = new List<int>();

            //if (_AllInfoMonitors == null)
            //    _AllInfoMonitors = devMgr.GetMonitors().Result;
            //_monitorIndeies = GetMonitorIndeies(commandLineInput, _AllInfoMonitors);

            //foreach (int idx in _monitorIndeies)
            //{
            //    writelog($"PowerSetting entry");
            //    MonitorInfo monitor = _AllInfoMonitors[idx];
            if (monitor != null)
            {
                //CLI_RESPONSE cli_Response = new CLI_RESPONSE(monitor);
                //cli_Response.Command = commandLineInput.Command;
                //cli_Response.TargetFeature = commandLineInput.TargetFeature;

                string capability = monitor.CapabilityString;
                if (capability.Contains("D6("))
                {
                    writelog($"PowerSetting set entry");
                    string[] ss = capability.Split("D6(");
                    ss = ss[1].Split(")");
                    ss = ss[0].Split(" ");
                    Trace.WriteLine($"ss[0]:{ss[0]}, ss[1]:{ss[1]}, ss[2]:{ss[2]}");
                    if (ss[0] == "01" && ss[1] == "04" && ss[2] == "05")
                    {
                        switch (commandLineInput.Options[0].Option_Value.ToUpper())
                        {
                            case "OFF":
                                writelog($"PowerSetting D6 set off");
                                retcode = SetVCPCode(devMgr, monitor, "0xD6", "0x05").Result;
                                if (!retcode)
                                    output = "Set Power state fail";
                                //cLI_RESPONSE.Value = commandLineInput.Options[0].Option_Value;
                                return (retcode, output);

                            case "ON":
                                writelog($"PowerSetting D6 set on");
                                retcode = SetVCPCode(devMgr, monitor, "0xD6", "0x01").Result;
                                if (!retcode)
                                    output = "Set Power state fail";
                                //cLI_RESPONSE.Value = commandLineInput.Options[0].Option_Value;
                                return (retcode, output);

                            case "STANDBY":
                                writelog($"PowerSetting D6 set standby");
                                retcode = SetVCPCode(devMgr, monitor, "0xD6", "0x04").Result;
                                if (!retcode)
                                    output = "Set Power state fail";
                                //cLI_RESPONSE.Value = commandLineInput.Options[0].Option_Value;
                                return (retcode, output);
                            default:
                                retcode = false;
                                output = "Invalid command line syntax";
                                return (retcode, output);
                                //cLI_RESPONSE.Command = commandLineInput.Command;
                                //cLI_RESPONSE.TargetFeature = commandLineInput.TargetFeature;
                                //cLI_RESPONSE.Result = "FAIL";
                                //cLI_RESPONSE.Message = "Invalid command line syntax, missing -value=... or more than one -value=...";
                        }
                        //if (retcode == true)
                        //{
                        //    Console.WriteLine($"Anf_reget monitor info");
                        //    Sleep(5000);
                        //    _AllInfoMonitors = devMgr.GetMonitors().Result;
                        //}
                    }
                    else
                    {
                        retcode = false;
                        output = "PowerSetting command not support";
                        return (retcode, output);
                    }
                }
                else
                {
                    retcode = false;
                    output = "PowerSetting command not support";
                    return (retcode, output);
                }
            }
            else
            {
                retcode = false;
                output = "No Monitor found";
                return (retcode, output);
            }
        }
        private bool _display_changed = false;
        private void DevMgr_Displaychanged(object sender, DisplaychangedEventArgs e)
        {
            //do things while you get the new all monitors list
            _AllInfoMonitors = e.monitors;
            //finally change flag to leave while loop
            _display_changed = true;
        }
        private async Task<(int code, string result)> PowerSettingv2(IDeviceManagerSA devMgr, CommandLineInput commandLineInput)
        {
            string output = string.Empty;
            bool retcode = false;
            int somethingfail = 0;
            int monitorcount = 0;
            ObjGetVCP rc = new ObjGetVCP();

            List<int> _monitorIndeies = new List<int>();

            if (_AllInfoMonitors == null)
                _AllInfoMonitors = devMgr.GetMonitors().Result;
            monitorcount = _AllInfoMonitors.Count;
            _monitorIndeies = GetMonitorIndeies(commandLineInput, _AllInfoMonitors);
            int ret = 0;
            if (commandLineInput.Command == "SET")
            {
                //if (commandLineInput.DeviceIndex.Count <= 0 && commandLineInput.ServiceTag.Count <= 0 && commandLineInput.Model.Count <= 0)
                if (_monitorIndeies.Count > 1)
                {
                    bool flag = true;
                    var serviceTagList = _AllInfoMonitors.Select(_ => _.edid.ServiceTag).Distinct().ToList();

                    List<string> swapIsDone = new List<string>();
                    int count = 0;
                    while (flag && count < 1000)
                    {
                        for (int i = 0; i < serviceTagList.Count; i++)
                        {
                            string stIsDone = swapIsDone.FirstOrDefault(_ => _ == serviceTagList[i]);
                            if (!String.IsNullOrWhiteSpace(stIsDone))
                                continue;
                            MonitorInfo mo = _AllInfoMonitors.FirstOrDefault(_ => _.edid.ServiceTag == serviceTagList[i]);
                            if (mo == null)
                            {
                                Task.Delay(5000).Wait();
                                _AllInfoMonitors = _devMgr.GetMonitors().Result;
                                Console.WriteLine($"Anf_New Monitor count {_AllInfoMonitors.Count}");
                                break;
                            }
                            //var result = SetPower(devMgr, mo, commandLineInput);
                            CLI_RESPONSE cLI_RESPONSE = new CLI_RESPONSE(mo);
                            cLI_RESPONSE.Index = (i + 1).ToString();
                            cLI_RESPONSE.Command = commandLineInput.Command;
                            cLI_RESPONSE.TargetFeature = commandLineInput.TargetFeature;
                            cLI_RESPONSE.Value = commandLineInput.Options[0].Option_Value;
                            string capability = mo.CapabilityString;
                            if (capability.Contains("D6("))
                            {

                                writelog($"PowerSetting set entry");
                                string[] ss = capability.Split("D6(");
                                ss = ss[1].Split(")");
                                ss = ss[0].Split(" ");
                                Trace.WriteLine($"ss[0]:{ss[0]}, ss[1]:{ss[1]}, ss[2]:{ss[2]}");
                                if (ss[0] == "01" && ss[1] == "04" && ss[2] == "05")
                                {
                                    switch (commandLineInput.Options[0].Option_Value.ToUpper())
                                    {
                                        case "OFF":
                                            writelog($"PowerSetting D6 set off");
                                            _display_changed = false;
                                            devMgr.Displaychanged += DevMgr_Displaychanged;
                                            retcode = SetVCPCode(devMgr, mo, "0xD6", "0x05").Result;
                                            //cLI_RESPONSE.Value = commandLineInput.Options[0].Option_Value;
                                            int timeout_count = 0;
                                            while (_display_changed == false)
                                            {
                                                Task.Delay(1000).Wait();
                                                //check all monitors

                                                if (timeout_count++ > 20)
                                                {
                                                    writelog($"PowerSetting D6 set off timeout");
                                                    _AllInfoMonitors = devMgr.GetMonitors().Result;
                                                    break;
                                                }
                                            }
                                            if (_display_changed == true)
                                            {
                                                //do things here
                                                //_AllInfoMonitors
                                            }
                                            devMgr.Displaychanged -= DevMgr_Displaychanged;
                                            break;

                                        case "ON":
                                            writelog($"PowerSetting D6 set on");
                                            retcode = SetVCPCode(devMgr, mo, "0xD6", "0x01").Result;
                                            //cLI_RESPONSE.Value = commandLineInput.Options[0].Option_Value;
                                            break;

                                        case "STANDBY":
                                            writelog($"PowerSetting D6 set standby");
                                            retcode = SetVCPCode(devMgr, mo, "0xD6", "0x04").Result;
                                            //cLI_RESPONSE.Value = commandLineInput.Options[0].Option_Value;
                                            break;
                                        default:
                                            cLI_RESPONSE.Command = commandLineInput.Command;
                                            cLI_RESPONSE.TargetFeature = commandLineInput.TargetFeature;
                                            cLI_RESPONSE.Result = "FAIL";
                                            cLI_RESPONSE.Message = "Invalid command line syntax, missing -value=... or more than one -value=...";
                                            break;
                                    }
                                }
                                else
                                    somethingfail |= 0x10;
                                //Sleep(500);
                                swapIsDone.Add(mo.edid.ServiceTag);
                            }
                            else
                                somethingfail |= 0x10;
                            //if (result.code)
                            if (retcode)
                            {
                                cLI_RESPONSE.Result = "PASS";
                                cLI_RESPONSE.Message = "N/A";
                            }
                            else
                            {
                                cLI_RESPONSE.Result = "FAIL";
                                if ((somethingfail & 0x01) == 0x01)
                                    cLI_RESPONSE.Message = $"Unknown value ({commandLineInput.Options[0].Option_Value})";
                                else if ((somethingfail & 0x10) == 0x10)
                                    cLI_RESPONSE.Message = $"No Support {commandLineInput.TargetFeature}";
                                else
                                    cLI_RESPONSE.Message = $"Set {commandLineInput.Options[0].Option_Value} fail";
                                //cLI_RESPONSE.Message = result.result;
                            }
                            System.Console.WriteLine(JsonConvert.SerializeObject(cLI_RESPONSE, Formatting.Indented));
                            output += "\n" + JsonConvert.SerializeObject(cLI_RESPONSE, Formatting.Indented);
                        }
                        if (serviceTagList.Count == swapIsDone.Count)
                            flag = false;
                        count++;
                    }
                }
                else if (_monitorIndeies.Count == 1)
                {
                    foreach (int idx in _monitorIndeies)
                    {
                        writelog($"PowerSetting entry");
                        MonitorInfo monitor = _AllInfoMonitors[idx];
                        CLI_RESPONSE cli_Response = new CLI_RESPONSE(monitor);
                        cli_Response.Command = commandLineInput.Command;
                        cli_Response.TargetFeature = commandLineInput.TargetFeature;
                        cli_Response.Value = commandLineInput.Options[0].Option_Value;
                        string capability = monitor.CapabilityString;
                        if (capability.Contains("D6("))
                        {
                            if (commandLineInput.Command == "SET")
                            {
                                writelog($"PowerSetting set entry");
                                string[] ss = capability.Split("D6(");
                                ss = ss[1].Split(")");
                                ss = ss[0].Split(" ");
                                Trace.WriteLine($"ss[0]:{ss[0]}, ss[1]:{ss[1]}, ss[2]:{ss[2]}");
                                if (ss[0] == "01" && ss[1] == "04" && ss[2] == "05")
                                {
                                    switch (commandLineInput.Options[0].Option_Value.ToUpper())
                                    {
                                        case "OFF":
                                            writelog($"PowerSetting D6 set off");
                                            retcode = SetVCPCode(devMgr, monitor, "0xD6", "0x05").Result;
                                            //cli_Response.Value = commandLineInput.Options[0].Option_Value;                                           
                                            break;

                                        case "ON":
                                            writelog($"PowerSetting D6 set on");
                                            retcode = SetVCPCode(devMgr, monitor, "0xD6", "0x01").Result;
                                            //cli_Response.Value = commandLineInput.Options[0].Option_Value;
                                            break;

                                        case "STANDBY":
                                            writelog($"PowerSetting D6 set standby");
                                            retcode = SetVCPCode(devMgr, monitor, "0xD6", "0x04").Result;
                                            //cli_Response.Value = commandLineInput.Options[0].Option_Value;
                                            break;
                                        default:

                                            cli_Response.Command = commandLineInput.Command;
                                            cli_Response.TargetFeature = commandLineInput.TargetFeature;
                                            cli_Response.Result = "FAIL";
                                            cli_Response.Message = "Invalid command line syntax, missing -value=... or more than one -value=...";

                                            break;
                                    }
                                    Task.Delay(3000).Wait();
                                }
                                else
                                    somethingfail |= 0x10;
                            }
                        }
                        else
                            somethingfail |= 0x10;

                        retcode = true;

                        if (retcode)
                        {
                            cli_Response.Result = "PASS";
                            cli_Response.Message = "N/A";
                        }
                        else
                        {
                            cli_Response.Result = "FAIL";
                            if ((somethingfail & 0x01) == 0x01)
                                cli_Response.Message = $"Unknown value ({commandLineInput.Options[0].Option_Value})";
                            else if ((somethingfail & 0x10) == 0x10)
                                cli_Response.Message = $"No Support {commandLineInput.TargetFeature}";
                            else
                                cli_Response.Message = $"Set {commandLineInput.Options[0].Option_Value} fail";
                        }
                        System.Console.WriteLine(JsonConvert.SerializeObject(cli_Response, Formatting.Indented));
                        output += "\n" + JsonConvert.SerializeObject(cli_Response, Formatting.Indented);
                    }
                }
                else
                {
                    CLI_RESPONSE rsp = new CLI_RESPONSE()
                    {
                        Command = commandLineInput.Command,
                        TargetFeature = commandLineInput.TargetFeature,
                        // Result = "FAIL",
                        Message = "No devices found",
                    };
                    System.Console.WriteLine(JsonConvert.SerializeObject(rsp, Formatting.Indented));
                    output += "\n" + JsonConvert.SerializeObject(rsp, Formatting.Indented);
                }

            }
            else if (commandLineInput.Command == "GET")
            {
                foreach (int idx in _monitorIndeies)
                {
                    writelog($"PowerSetting get entry");
                    Console.WriteLine($"-------------------idx {idx}");
                    retcode = true;
                    MonitorInfo mo = _AllInfoMonitors[idx];
                    CLI_RESPONSE cLI_RESPONSE = new CLI_RESPONSE(mo);
                    cLI_RESPONSE.Command = commandLineInput.Command;
                    cLI_RESPONSE.TargetFeature = commandLineInput.TargetFeature;
                    //cLI_RESPONSE.Value = commandLineInput.Options[0].Option_Value;
                    cLI_RESPONSE.Result = "PASS";
                    cLI_RESPONSE.Message = "N/A";
                    string capability = mo.CapabilityString;
                    if (capability.Contains("D6("))
                    {
                        rc = GetVCPCode(devMgr, mo, "0xD6").Result;
                        if (rc != null && rc.value.ToString() == "1")
                            cLI_RESPONSE.Value = "ON";
                        else if (rc != null && rc.value.ToString() == "4")
                            cLI_RESPONSE.Value = "STANDBY";
                        else if (rc != null && rc.value.ToString() == "5")
                            cLI_RESPONSE.Value = "OFF";
                        else
                            cLI_RESPONSE.Value = "Get VCP fail";
                    }
                    else
                    {
                        cLI_RESPONSE.Result = "FAIL";
                        cLI_RESPONSE.Message = $"No Support {commandLineInput.TargetFeature}";
                    }
                    System.Console.WriteLine(JsonConvert.SerializeObject(cLI_RESPONSE, Formatting.Indented));
                    output += "\n" + JsonConvert.SerializeObject(cLI_RESPONSE, Formatting.Indented);
                }
            }
            //System.Console.WriteLine(JsonConvert.SerializeObject(cLI_RESPONSE, Formatting.Indented));
            //output += "\n" + JsonConvert.SerializeObject(cLI_RESPONSE, Formatting.Indented);
            //foreach (int idx in _monitorIndeies)
            //{
            //    writelog($"PowerSetting entry");
            //    MonitorInfo monitor = _AllInfoMonitors[idx];
            //    CLI_RESPONSE cli_Response = new CLI_RESPONSE(monitor);
            //    cli_Response.Command = commandLineInput.Command;
            //    cli_Response.TargetFeature = commandLineInput.TargetFeature;

            //    string capability = monitor.CapabilityString;
            //    if (capability.Contains("D6("))
            //    {
            //        if (commandLineInput.Command == "SET")
            //        {
            //            writelog($"PowerSetting set entry");
            //            string[] ss = capability.Split("D6(");
            //            ss = ss[1].Split(")");
            //            ss = ss[0].Split(" ");
            //            Trace.WriteLine($"ss[0]:{ss[0]}, ss[1]:{ss[1]}, ss[2]:{ss[2]}");
            //            if (ss[0] == "01" && ss[1] == "04" && ss[2] == "05")
            //            {
            //                switch (commandLineInput.Options[0].Option_Value.ToUpper())
            //                {
            //                    case "OFF":
            //                        writelog($"PowerSetting D6 set off");
            //                        retcode = SetVCPCode(devMgr, monitor, "0xD6", "0x05").Result;
            //                        cli_Response.Value = commandLineInput.Options[0].Option_Value;
            //                        break;

            //                    case "ON":
            //                        writelog($"PowerSetting D6 set on");
            //                        retcode = SetVCPCode(devMgr, monitor, "0xD6", "0x01").Result;
            //                        cli_Response.Value = commandLineInput.Options[0].Option_Value;
            //                        break;

            //                    case "STANDBY":
            //                        writelog($"PowerSetting D6 set standby");
            //                        retcode = SetVCPCode(devMgr, monitor, "0xD6", "0x04").Result;
            //                        cli_Response.Value = commandLineInput.Options[0].Option_Value;
            //                        break;
            //                    default:

            //                        cli_Response.Command = commandLineInput.Command;
            //                        cli_Response.TargetFeature = commandLineInput.TargetFeature;
            //                        cli_Response.Result = "FAIL";
            //                        cli_Response.Message = "Invalid command line syntax, missing -value=... or more than one -value=...";

            //                        break;
            //                }
            //                Sleep(3000);
            //            }
            //            else
            //                somethingfail |= 0x10;
            //        }
            //        else if (commandLineInput.Command == "GET")
            //        {
            //            writelog($"PowerSetting get entry");
            //            retcode = true;
            //            cli_Response.Result = "PASS";
            //            cli_Response.Message = "N/A";
            //            rc = GetVCPCode(devMgr, monitor, "0xD6").Result;
            //            if (rc != null && rc.value.ToString() == "1")
            //                cli_Response.Value = "ON";
            //            else if (rc != null && rc.value.ToString() == "4")
            //                cli_Response.Value = "STANDBY";
            //            else if (rc != null && rc.value.ToString() == "5")
            //                cli_Response.Value = "OFF";
            //            else
            //                cli_Response.Value = "Get VCP fail";
            //        }
            //    }
            //    else
            //        somethingfail |= 0x10;

            //    retcode = true;

            //    if (retcode)
            //    {
            //        cli_Response.Result = "PASS";
            //        cli_Response.Message = "N/A";
            //    }
            //    else
            //    {
            //        cli_Response.Result = "FAIL";
            //        if ((somethingfail & 0x01) == 0x01)
            //            cli_Response.Message = $"Unknown value ({commandLineInput.Options[0].Option_Value})";
            //        else if ((somethingfail & 0x10) == 0x10)
            //            cli_Response.Message = $"No Support {commandLineInput.TargetFeature}";
            //        else
            //            cli_Response.Message = $"Set {commandLineInput.Options[0].Option_Value} fail";
            //    }
            //    System.Console.WriteLine(JsonConvert.SerializeObject(cli_Response, Formatting.Indented));
            //    output += "\n" + JsonConvert.SerializeObject(cli_Response, Formatting.Indented);
            //}
            writelog($"PowerSetting exit return value{output}");
            return (retcode ? (int)CLI_ExitCode.success : (int)CLI_ExitCode.functional_error, output);
        }

        private static string get_PowerSetting_code(string code) //E0:[b1:b0]
        {
            switch (code.ToUpper())
            {
                case "ON": return "0";
                case "OFF": return "1";
                case "STANDBY": return "2";
                default: return "unknown_command";
            }
        }

        private static string get_Energysaver_code(string code) //E0:[b3:b2]
        {
            switch (code.ToUpper())
            {
                case "OFF": return "0";
                case "ON": return "4";
                case "OFF,LOCK": return "8";
                case "ON,LOCK": return "12";
                default: return "unknown_command";
            }
        }

        private (int code, string result) Mute_UnmuteX(IDeviceManagerSA devMgr, CommandLineInput commandLineInput)
        {
            if (commandLineInput.Command == "GET" && commandLineInput.Options.Count == 0)
            {
                return mute_unmute(devMgr, commandLineInput).Result;
            }
            else if (commandLineInput.Command == "SET" && commandLineInput.Options.Count == 1)
            {
                return mute_unmute(devMgr, commandLineInput).Result;
            }
            else
            {
                CLI_RESPONSE cli_Response = new CLI_RESPONSE();
                cli_Response.Command = commandLineInput.Command;
                cli_Response.TargetFeature = commandLineInput.TargetFeature;
                cli_Response.Result = "FAIL";
                cli_Response.Message = "Invalid command line syntax, missing -value=... or more than one -value=...";
                return ((int)CLI_ExitCode.invalide_cmdline_syntax, cli_Response.ToJson());
            }
        }

        private async Task<(int code, string result)> mute_unmute(IDeviceManagerSA devMgr, CommandLineInput commandLineInput)
        {
            string output = string.Empty;
            bool retcode = false;
            int somethingfail = 0;
            ObjGetVCP rc = new ObjGetVCP();

            List<int> _monitorIndeies = new List<int>();

            if (_AllInfoMonitors == null)
                _AllInfoMonitors = devMgr.GetMonitors().Result;

            var serviceTags = _AllInfoMonitors.Where(_ => commandLineInput.DeviceIndex.Count == 0 || commandLineInput.DeviceIndex.Contains((_.Index + 1).ToString()))
                                              .Where(_ => commandLineInput.ServiceTag.Count == 0 || commandLineInput.ServiceTag.Contains(_.edid.ServiceTag))
                                              .Where(_ => commandLineInput.Model.Count == 0 || commandLineInput.Model.Contains(_.modelName))
                                              .Select(_ => _.edid.ServiceTag)
                                              .Distinct()
                                              .ToList();

            foreach (string serviceTag in serviceTags)
            {
                MonitorInfo monitor = _AllInfoMonitors.FirstOrDefault(_ => _.edid.ServiceTag == serviceTag);

                CLI_RESPONSE cli_Response = new CLI_RESPONSE(monitor);
                cli_Response.Command = commandLineInput.Command;
                cli_Response.TargetFeature = commandLineInput.TargetFeature;

                switch (commandLineInput.TargetFeature)
                {
                    case "MICROPHONE":
                        writelog("MICROPHONE entry");
                        if (CheckMicrophoneSupported(monitor, commandLineInput))
                        {
                            if (commandLineInput.Command == "SET")
                            {
                                writelog("MICROPHONE SET entry");
                                rc = GetVCPCode(devMgr, monitor, "0x8D").Result;
                                int getvalue = Convert.ToInt32(rc.value);
                                string setvalue = get_MicrophoneControl(commandLineInput.Options[0].Option_Value, getvalue);

                                if (setvalue != "unknown_command")
                                {
                                    retcode = SetVCPCode(devMgr, monitor, "0x8D", setvalue).Result;
                                    cli_Response.Value = commandLineInput.Options[0].Option_Value;
                                }
                                else
                                    somethingfail |= 0x01;
                            }
                            else if (commandLineInput.Command == "GET")
                            {
                                writelog("MICROPHONE GET entry");
                                rc = GetVCPCode(devMgr, monitor, "0x8D").Result;
                                cli_Response.Value = get_MicrophoneControl_status(Convert.ToInt32(rc.value));
                                retcode = true;
                            }
                        }
                        else
                        {
                            somethingfail |= 0x10;
                        }
                        break;

                    case "SPEAKERVOLUME":
                        writelog("SPEAKERVOLUME entry");
                        if (CheckSpeakerSupported(monitor, commandLineInput))
                        {
                            if (commandLineInput.Command == "SET")
                            {
                                writelog("SPEAKERVOLUME SET entry");
                                rc = GetVCPCode(devMgr, monitor, "0x62").Result;
                                int getvalue = Convert.ToInt32(rc.value);
                                string setvalue = get_SpeakerVolume(commandLineInput.Options[0].Option_Value, getvalue);

                                if (setvalue != "unknown_command")
                                {
                                    retcode = SetVCPCode(devMgr, monitor, "0x62", setvalue).Result;
                                    cli_Response.Value = commandLineInput.Options[0].Option_Value;
                                }
                                else
                                    somethingfail |= 0x01;
                            }
                            else if (commandLineInput.Command == "GET")
                            {
                                writelog("SPEAKERVOLUME GET entry");
                                rc = GetVCPCode(devMgr, monitor, "0x62", 0).Result;
                                cli_Response.Value = get_SpeakerVolume_status(Convert.ToInt32(rc.value));
                                retcode = true;
                            }
                        }
                        else
                        {
                            somethingfail |= 0x10;
                        }
                        break;

                    case "SPEAKERMICROPHONE":
                        writelog("SPEAKERMICROPHONE entry");
                        if (CheckSpeakerSupported(monitor, commandLineInput) && CheckMicrophoneSupported(monitor, commandLineInput))
                        {
                            if (commandLineInput.Command == "SET")
                            {
                                writelog("SPEAKERMICROPHONE SET entry");
                                rc = GetVCPCode(devMgr, monitor, "0x62").Result;
                                int getvalue = Convert.ToInt32(rc.value);
                                string setvalue = get_SpeakerMicrophone(commandLineInput.Options[0].Option_Value, getvalue);

                                if (setvalue != "unknown_command")
                                {
                                    SetVCPCode(devMgr, monitor, "0x62", setvalue);
                                    retcode = true;
                                    cli_Response.Value = commandLineInput.Options[0].Option_Value;
                                }
                                else
                                    somethingfail |= 0x01;

                                //int count = 0;
                                //while (!_AllInfoMonitors.Any(_ => _.edid.ServiceTag == monitor.edid.ServiceTag) && count < 1000)
                                //{
                                //    _AllInfoMonitors = devMgr.GetMonitors().Result;
                                //    count++;
                                //}

                                //monitor = _AllInfoMonitors.FirstOrDefault(_ => _.edid.ServiceTag == serviceTag);

                                //rc = GetVCPCode(devMgr, monitor, "0x8D").Result;
                                //int getvalue2 = Convert.ToInt32(rc.value);
                                //string setvalue2 = get_SpeakerMicrophone(commandLineInput.Options[0].Option_Value, getvalue2);

                                //if (setvalue2 != "unknown_command")
                                //{
                                //    retcode = SetVCPCode(devMgr, monitor, "0x8D", setvalue2).Result;
                                //    cli_Response.Value = commandLineInput.Options[0].Option_Value;
                                //}
                                //else
                                //    somethingfail |= 0x02;
                            }
                            else if (commandLineInput.Command == "GET")
                            {
                                writelog("SPEAKERMICROPHONE GET entry");
                                rc = GetVCPCode(devMgr, monitor, "0x62").Result;
                                string ss0 = get_SpeakerMicrophone_status(Convert.ToInt32(rc.value));
                                rc = GetVCPCode(devMgr, monitor, "0x8D").Result;
                                string ss1 = get_SpeakerMicrophone_status(Convert.ToInt32(rc.value));
                                if (ss0 == ss1)
                                    cli_Response.Value = "SPEAKERMICROPHONE:" + ss0;
                                else
                                    cli_Response.Value = "SPEAKER:" + ss0 + "," + "MICROPHONE:" + ss1;
                                retcode = true;
                            }
                        }
                        else
                        {
                            somethingfail |= 0x10;
                        }
                        break;
                }

                if (retcode)
                {
                    cli_Response.Result = "PASS";
                    cli_Response.Message = "N/A";
                }
                else
                {
                    cli_Response.Result = "FAIL";
                    if ((somethingfail & 0x03) == 0x01 || (somethingfail & 0x03) == 0x02)
                        cli_Response.Message = $"Unknown value ({commandLineInput.Options[0].Option_Value})";
                    else if ((somethingfail & 0x10) == 0x10)
                        cli_Response.Message = $"No Support {commandLineInput.TargetFeature}";
                    else
                        cli_Response.Message = $"Set {commandLineInput.Options[0].Option_Value} fail";
                }
                System.Console.WriteLine(JsonConvert.SerializeObject(cli_Response, Formatting.Indented));
                output += "\n" + JsonConvert.SerializeObject(cli_Response, Formatting.Indented);
            }

            return (retcode ? (int)CLI_ExitCode.success : (int)CLI_ExitCode.functional_error, output);
        }

        private (int code, string result) AdvancedcontrolX(IDeviceManagerSA devMgr, CommandLineInput commandLineInput)
        {
            if (commandLineInput.Options.Count == 0)
            {
                CLI_RESPONSE cli_Response = new CLI_RESPONSE();
                cli_Response.Command = commandLineInput.Command;
                cli_Response.TargetFeature = commandLineInput.TargetFeature;
                cli_Response.Result = "FAIL";
                cli_Response.Message = "Invalid command line syntax.";
                return ((int)CLI_ExitCode.invalide_cmdline_syntax, cli_Response.ToJson());
            }
            else
            {
                if (commandLineInput.Options == null || commandLineInput.Options[0].Option_Value == string.Empty || commandLineInput.Options.Count > 2)
                {
                    CLI_RESPONSE cli_Response = new CLI_RESPONSE();
                    cli_Response.Command = commandLineInput.Command;
                    cli_Response.TargetFeature = commandLineInput.TargetFeature;
                    cli_Response.Result = "FAIL";
                    cli_Response.Message = "Invalid command line syntax, missing -value=... or more than one -value=...";
                    return ((int)CLI_ExitCode.invalide_cmdline_syntax, cli_Response.ToJson());
                }
                else
                {
                    return Advancedcontrol(devMgr, commandLineInput).Result;
                }
            }
        }

        private async Task<(int code, string result)> Advancedcontrol(IDeviceManagerSA devMgr, CommandLineInput commandLineInput)
        {
            string output = string.Empty;
            bool retcode = false;
            bool vcp_value = false;

            if (_AllInfoMonitors == null)
                _AllInfoMonitors = await devMgr.GetMonitors();

            switch (commandLineInput.Command)
            {
                case "GET":
                    if (commandLineInput.DeviceIndex.Count == 0 && commandLineInput.ServiceTag.Count == 0 && commandLineInput.Model.Count == 0)
                    {
                        foreach (MonitorInfo monitor in _AllInfoMonitors)
                        {
                            ObjGetVCP rc = new ObjGetVCP();
                            CLI_RESPONSE cli_Response = new CLI_RESPONSE(monitor);
                            cli_Response.Command = commandLineInput.Command;
                            cli_Response.TargetFeature = commandLineInput.TargetFeature;

                            string capability = monitor.CapabilityString;
                            string[] ss_1 = null;
                            if (commandLineInput.Options[0].Option_Name.ToUpper().Equals("OPCODE") &&
                                commandLineInput.Options[0].Option_Value.Contains("0X"))
                            {
                                ss_1 = commandLineInput.Options[0].Option_Value.Split("0X");
                                Trace.WriteLine($"monitor.CapabilityDic.ContainsKey : {monitor.CapabilityDic.ContainsKey(ss_1[1])}");
                                Trace.WriteLine($"ss_1[1] : {ss_1[1]}");
                                if (monitor.CapabilityDic.ContainsKey(ss_1[1]))
                                {
                                    rc = GetVCPCode(devMgr, monitor, commandLineInput.Options[0].Option_Value).Result;
                                    cli_Response.Value = (rc.value).ToString();
                                    retcode = true;
                                }
                            }
                            if (retcode)
                            {
                                cli_Response.Result = "PASS";
                                cli_Response.Message = "N/A";
                            }
                            else
                            {
                                cli_Response.Result = "FAIL";
                                cli_Response.Message = $"VCP code {commandLineInput.Options[0].Option_Value} is not supported";
                            }
                            System.Console.WriteLine(JsonConvert.SerializeObject(cli_Response, Formatting.Indented));
                            output += "\n" + JsonConvert.SerializeObject(cli_Response, Formatting.Indented);
                        }
                    }
                    else
                    {
                        foreach (string idx in commandLineInput.DeviceIndex)
                        {
                            MonitorInfo monitor = _AllInfoMonitors[Convert.ToInt32(idx)];
                            CLI_RESPONSE cli_Response = new CLI_RESPONSE(monitor);
                            ObjGetVCP rc = new ObjGetVCP();
                            cli_Response.Command = commandLineInput.Command;
                            cli_Response.TargetFeature = commandLineInput.TargetFeature;

                            string capability = monitor.CapabilityString;
                            string[] ss_1 = null;
                            if (commandLineInput.Options[0].Option_Name.ToUpper().Equals("OPCODE") &&
                                commandLineInput.Options[0].Option_Value.Contains("0X"))
                            {
                                ss_1 = commandLineInput.Options[0].Option_Value.Split("0X");
                                if (monitor.CapabilityDic.ContainsKey(ss_1[1]))
                                {
                                    rc = GetVCPCode(devMgr, monitor, commandLineInput.Options[0].Option_Value).Result;
                                    cli_Response.Value = (rc.value).ToString();
                                    retcode = true;
                                }
                            }
                            if (retcode)
                            {
                                cli_Response.Result = "PASS";
                                cli_Response.Message = "N/A";
                            }
                            else
                            {
                                cli_Response.Result = "FAIL";
                                cli_Response.Message = $"VCP code {commandLineInput.Options[0].Option_Value} is not supported";
                            }
                            System.Console.WriteLine(JsonConvert.SerializeObject(cli_Response, Formatting.Indented));
                            output += "\n" + JsonConvert.SerializeObject(cli_Response, Formatting.Indented);
                        }
                        foreach (string tag in commandLineInput.ServiceTag)
                        {
                            var tmp = _AllInfoMonitors.FindAll(x => x.edid.ServiceTag.ToUpper().Equals(tag.ToUpper()));
                            foreach (MonitorInfo monitor in tmp)
                            {
                                CLI_RESPONSE cli_Response = new CLI_RESPONSE(monitor);
                                ObjGetVCP rc = new ObjGetVCP();
                                cli_Response.Command = commandLineInput.Command;
                                cli_Response.TargetFeature = commandLineInput.TargetFeature;

                                string capability = monitor.CapabilityString;
                                string[] ss_1 = null;

                                if (commandLineInput.Options[0].Option_Name.ToUpper().Equals("OPCODE") &&
                                    commandLineInput.Options[0].Option_Value.Contains("0X"))
                                {
                                    ss_1 = commandLineInput.Options[0].Option_Value.Split("0X");
                                    if (monitor.CapabilityDic.ContainsKey(ss_1[1]))
                                    {
                                        rc = GetVCPCode(devMgr, monitor, commandLineInput.Options[0].Option_Value).Result;
                                        cli_Response.Value = (rc.value).ToString();
                                        retcode = true;
                                    }
                                }
                                if (retcode)
                                {
                                    cli_Response.Result = "PASS";
                                    cli_Response.Message = "N/A";
                                }
                                else
                                {
                                    cli_Response.Result = "FAIL";
                                    cli_Response.Message = $"VCP code {commandLineInput.Options[0].Option_Value} is not supported";
                                }
                                System.Console.WriteLine(JsonConvert.SerializeObject(cli_Response, Formatting.Indented));
                                output += "\n" + JsonConvert.SerializeObject(cli_Response, Formatting.Indented);
                            }
                        }
                        foreach (string modelName in commandLineInput.Model)
                        {
                            var tmp = _AllInfoMonitors.FindAll(x => x.edid.ModelName.ToUpper().Equals(modelName.ToUpper()));
                            foreach (MonitorInfo monitor in tmp)
                            {
                                CLI_RESPONSE cli_Response = new CLI_RESPONSE(monitor);
                                ObjGetVCP rc = new ObjGetVCP();
                                cli_Response.Command = commandLineInput.Command;
                                cli_Response.TargetFeature = commandLineInput.TargetFeature;

                                string capability = monitor.CapabilityString;
                                string[] ss_1 = null;

                                if (commandLineInput.Options[0].Option_Name.ToUpper().Equals("OPCODE") &&
                                    commandLineInput.Options[0].Option_Value.Contains("0X"))
                                {
                                    ss_1 = commandLineInput.Options[0].Option_Value.Split("0X");
                                    if (monitor.CapabilityDic.ContainsKey(ss_1[1]))
                                    {
                                        rc = GetVCPCode(devMgr, monitor, commandLineInput.Options[0].Option_Value).Result;
                                        cli_Response.Value = (rc.value).ToString();
                                        retcode = true;
                                    }
                                }
                                if (retcode)
                                {
                                    cli_Response.Result = "PASS";
                                    cli_Response.Message = "N/A";
                                }
                                else
                                {
                                    cli_Response.Result = "FAIL";
                                    cli_Response.Message = $"VCP code {commandLineInput.Options[0].Option_Value} is not supported";
                                }
                                System.Console.WriteLine(JsonConvert.SerializeObject(cli_Response, Formatting.Indented));
                                output += "\n" + JsonConvert.SerializeObject(cli_Response, Formatting.Indented);
                            }
                        }
                    }
                    break;

                case "SET":
                    if (commandLineInput.DeviceIndex.Count == 0 && commandLineInput.ServiceTag.Count == 0 && commandLineInput.Model.Count == 0)
                    {
                        foreach (MonitorInfo monitor in _AllInfoMonitors)
                        {
                            ObjGetVCP rc = new ObjGetVCP();
                            CLI_RESPONSE cli_Response = new CLI_RESPONSE(monitor);
                            cli_Response.Command = commandLineInput.Command;
                            cli_Response.TargetFeature = commandLineInput.TargetFeature;

                            string capability = monitor.CapabilityString;
                            string[] ss_1 = null;
                            Trace.WriteLine($"option value : {commandLineInput.Options[1].Option_Value}");
                            if (commandLineInput.Options[0].Option_Name.ToUpper().Equals("OPCODE") &&
                                commandLineInput.Options[0].Option_Value.Contains("0X"))
                            {
                                ss_1 = commandLineInput.Options[0].Option_Value.Split("0X");
                                if (monitor.CapabilityDic.ContainsKey(ss_1[1]))
                                {
                                    if (capability.Contains(ss_1[1] + "("))
                                    {
                                        string[] ss = capability.Split(ss_1[1] + "(");
                                        ss = ss[1].Split(")");
                                        if (commandLineInput.Options[1].Option_Name.ToUpper().Equals("VALUE") &&
                                            ss[0].Contains(commandLineInput.Options[1].Option_Value))
                                        {
                                            retcode = SetVCPCode(devMgr, monitor, commandLineInput.Options[0].Option_Value, commandLineInput.Options[1].Option_Value).Result;

                                            cli_Response.Value = $"{retcode.ToString()}, VCP is set.";
                                            vcp_value = true;
                                            Trace.WriteLine($"retcode : {retcode}");
                                        }
                                    }
                                    else if (capability.Contains(ss_1[1]))
                                    {
                                        retcode = SetVCPCode(devMgr, monitor, commandLineInput.Options[0].Option_Value, commandLineInput.Options[1].Option_Value).Result;
                                        cli_Response.Value = $"{retcode.ToString()}, VCP is set.";
                                        vcp_value = true;
                                        Trace.WriteLine($"retcode : {retcode}");
                                    }
                                }
                            }
                            if (retcode)
                            {
                                cli_Response.Result = "PASS";
                                cli_Response.Message = "N/A";
                            }
                            else if (!commandLineInput.Options[1].Option_Name.ToUpper().Equals("VALUE"))
                            {
                                cli_Response.Result = "FAIL";
                                cli_Response.Message = $"{commandLineInput.Options[1].Option_Name} option is not supported.";
                            }
                            else if (!vcp_value)
                            {
                                cli_Response.Result = "FAIL";
                                cli_Response.Message = $"Advanced VCP value didn't supported.";
                            }
                            else
                            {
                                cli_Response.Result = "FAIL";
                                cli_Response.Message = $"VCP code {commandLineInput.Options[0].Option_Value} is not supported";
                            }
                            System.Console.WriteLine(JsonConvert.SerializeObject(cli_Response, Formatting.Indented));
                            output += "\n" + JsonConvert.SerializeObject(cli_Response, Formatting.Indented);
                        }
                    }
                    else
                    {
                        foreach (string idx in commandLineInput.DeviceIndex)
                        {
                            MonitorInfo monitor = _AllInfoMonitors[Convert.ToInt32(idx)];
                            CLI_RESPONSE cli_Response = new CLI_RESPONSE(monitor);
                            cli_Response.Command = commandLineInput.Command;
                            cli_Response.TargetFeature = commandLineInput.TargetFeature;

                            string capability = monitor.CapabilityString;
                            string[] ss_1 = null;

                            if (commandLineInput.Options[0].Option_Name.ToUpper().Equals("OPCODE") &&
                                commandLineInput.Options[0].Option_Value.Contains("0X"))
                            {
                                ss_1 = commandLineInput.Options[0].Option_Value.Split("0X");
                                if (monitor.CapabilityDic.ContainsKey(ss_1[1]))
                                {
                                    if (capability.Contains(ss_1[1] + "("))
                                    {
                                        string[] ss = capability.Split(ss_1[1] + "(");
                                        ss = ss[1].Split(")");
                                        if (commandLineInput.Options[1].Option_Name.ToUpper().Equals("VALUE") &&
                                            ss[0].Contains(commandLineInput.Options[1].Option_Value))
                                        {
                                            retcode = SetVCPCode(devMgr, monitor, commandLineInput.Options[0].Option_Value, commandLineInput.Options[1].Option_Value).Result;

                                            cli_Response.Value = $"{retcode.ToString()}, VCP is set.";
                                            vcp_value = true;
                                            Trace.WriteLine($"retcode : {retcode}");
                                        }
                                    }
                                    else if (capability.Contains(ss_1[1]))
                                    {
                                        retcode = SetVCPCode(devMgr, monitor, commandLineInput.Options[0].Option_Value, commandLineInput.Options[1].Option_Value).Result;
                                        cli_Response.Value = $"{retcode.ToString()}, VCP is set.";
                                        vcp_value = true;
                                        Trace.WriteLine($"retcode : {retcode}");
                                    }
                                }
                            }
                            if (retcode)
                            {
                                cli_Response.Result = "PASS";
                                cli_Response.Message = "N/A";
                            }
                            else if (!commandLineInput.Options[1].Option_Name.ToUpper().Equals("VALUE"))
                            {
                                cli_Response.Result = "FAIL";
                                cli_Response.Message = $"{commandLineInput.Options[1].Option_Name} value is not supported.";
                            }
                            else if (!vcp_value)
                            {
                                cli_Response.Result = "FAIL";
                                cli_Response.Message = $"Advanced VCP value didn't supported.";
                            }
                            else
                            {
                                cli_Response.Result = "FAIL";
                                cli_Response.Message = $"VCP code {commandLineInput.Options[0].Option_Value} is not supported";
                            }
                            System.Console.WriteLine(JsonConvert.SerializeObject(cli_Response, Formatting.Indented));
                            output += "\n" + JsonConvert.SerializeObject(cli_Response, Formatting.Indented);
                        }
                        foreach (string tag in commandLineInput.ServiceTag)
                        {
                            var tmp = _AllInfoMonitors.FindAll(x => x.edid.ServiceTag.ToUpper().Equals(tag.ToUpper()));
                            foreach (MonitorInfo monitor in tmp)
                            {
                                CLI_RESPONSE cli_Response = new CLI_RESPONSE(monitor);
                                cli_Response.Command = commandLineInput.Command;
                                cli_Response.TargetFeature = commandLineInput.TargetFeature;

                                string capability = monitor.CapabilityString;
                                string[] ss_1 = null;

                                if (commandLineInput.Options[0].Option_Name.ToUpper().Equals("OPCODE") &&
                                    commandLineInput.Options[0].Option_Value.Contains("0X"))
                                {

                                    ss_1 = commandLineInput.Options[0].Option_Value.Split("0X");

                                    if (monitor.CapabilityDic.ContainsKey(ss_1[1]))
                                    {
                                        if (capability.Contains(ss_1[1] + "("))
                                        {
                                            string[] ss = capability.Split(ss_1[1] + "(");
                                            ss = ss[1].Split(")");
                                            if (commandLineInput.Options[1].Option_Name.ToUpper().Equals("VALUE") &&
                                                ss[0].Contains(commandLineInput.Options[1].Option_Value))
                                            {
                                                retcode = SetVCPCode(devMgr, monitor, commandLineInput.Options[0].Option_Value, commandLineInput.Options[1].Option_Value).Result;

                                                cli_Response.Value = $"{retcode.ToString()}, VCP is set.";
                                                vcp_value = true;
                                                Trace.WriteLine($"retcode : {retcode}");
                                            }
                                        }
                                        else if (capability.Contains(ss_1[1]))
                                        {
                                            retcode = SetVCPCode(devMgr, monitor, commandLineInput.Options[0].Option_Value, commandLineInput.Options[1].Option_Value).Result;
                                            cli_Response.Value = $"{retcode.ToString()}, VCP is set.";
                                            vcp_value = true;
                                            Trace.WriteLine($"retcode : {retcode}");
                                        }
                                    }

                                }
                                if (retcode)
                                {
                                    cli_Response.Result = "PASS";
                                    cli_Response.Message = "N/A";
                                }
                                else if (!vcp_value)
                                {
                                    cli_Response.Result = "FAIL";
                                    cli_Response.Message = $"Advanced VCP value didn't supported.";
                                }
                                else if (!commandLineInput.Options[1].Option_Name.ToUpper().Equals("VALUE"))
                                {
                                    cli_Response.Result = "FAIL";
                                    cli_Response.Message = $"{commandLineInput.Options[1].Option_Name} value is not supported.";
                                }
                                else
                                {
                                    cli_Response.Result = "FAIL";
                                    cli_Response.Message = $"VCP code {commandLineInput.Options[0].Option_Value} is not supported";
                                }
                                System.Console.WriteLine(JsonConvert.SerializeObject(cli_Response, Formatting.Indented));
                                output += "\n" + JsonConvert.SerializeObject(cli_Response, Formatting.Indented);
                            }
                        }

                        foreach (string modelName in commandLineInput.Model)
                        {
                            var tmp = _AllInfoMonitors.FindAll(x => x.edid.ModelName.ToUpper().Equals(modelName.ToUpper()));
                            foreach (MonitorInfo monitor in tmp)
                            {
                                CLI_RESPONSE cli_Response = new CLI_RESPONSE(monitor);
                                cli_Response.Command = commandLineInput.Command;
                                cli_Response.TargetFeature = commandLineInput.TargetFeature;

                                string capability = monitor.CapabilityString;
                                string[] ss_1 = null;

                                if (commandLineInput.Options[0].Option_Name.ToUpper().Equals("OPCODE") &&
                                    commandLineInput.Options[0].Option_Value.Contains("0X"))
                                {
                                    ss_1 = commandLineInput.Options[0].Option_Value.Split("0X");

                                    if (monitor.CapabilityDic.ContainsKey(ss_1[1]))
                                    {
                                        if (capability.Contains(ss_1[1] + "("))
                                        {
                                            string[] ss = capability.Split(ss_1[1] + "(");
                                            ss = ss[1].Split(")");
                                            if (commandLineInput.Options[1].Option_Name.ToUpper().Equals("VALUE") &&
                                                ss[0].Contains(commandLineInput.Options[1].Option_Value))
                                            {
                                                retcode = SetVCPCode(devMgr, monitor, commandLineInput.Options[0].Option_Value, commandLineInput.Options[1].Option_Value).Result;

                                                cli_Response.Value = $"{retcode.ToString()}, VCP is set.";
                                                vcp_value = true;
                                                Trace.WriteLine($"retcode : {retcode}");
                                            }
                                        }
                                        else if (capability.Contains(ss_1[1]))
                                        {
                                            retcode = SetVCPCode(devMgr, monitor, commandLineInput.Options[0].Option_Value, commandLineInput.Options[1].Option_Value).Result;
                                            cli_Response.Value = $"{retcode.ToString()}, VCP is set.";
                                            vcp_value = true;
                                            Trace.WriteLine($"retcode : {retcode}");
                                        }
                                    }
                                }
                                if (retcode)
                                {
                                    cli_Response.Result = "PASS";
                                    cli_Response.Message = "N/A";
                                }
                                else if (!vcp_value)
                                {
                                    cli_Response.Result = "FAIL";
                                    cli_Response.Message = $"Advanced VCP value didn't supported.";
                                }
                                else if (!commandLineInput.Options[1].Option_Name.ToUpper().Equals("VALUE"))
                                {
                                    cli_Response.Result = "FAIL";
                                    cli_Response.Message = $"{commandLineInput.Options[1].Option_Name} value is not supported.";
                                }
                                else
                                {
                                    cli_Response.Result = "FAIL";
                                    cli_Response.Message = $"VCP code {commandLineInput.Options[0].Option_Value} is not supported";
                                }
                                System.Console.WriteLine(JsonConvert.SerializeObject(cli_Response, Formatting.Indented));
                                output += "\n" + JsonConvert.SerializeObject(cli_Response, Formatting.Indented);
                            }
                        }
                    }
                    break;
            }
            return (retcode ? (int)CLI_ExitCode.success : (int)CLI_ExitCode.functional_error, output);
        }

        private (int code, string result) ActivehourX(IDeviceManagerSA devMgr, CommandLineInput commandLineInput)
        {
            if (commandLineInput.Options.Count != 0)
            {
                CLI_RESPONSE cli_Response = new CLI_RESPONSE();
                cli_Response.Command = commandLineInput.Command;
                cli_Response.TargetFeature = commandLineInput.TargetFeature;
                cli_Response.Result = "FAIL";
                cli_Response.Message = "Invalid command line syntax, missing -value=... or more than one -value=...";
                return ((int)CLI_ExitCode.invalide_cmdline_syntax, cli_Response.ToJson());
            }
            else
            {
                return Activehour(devMgr, commandLineInput).Result;
            }
        }

        private async Task<(int code, string result)> Activehour(IDeviceManagerSA devMgr, CommandLineInput commandLineInput)
        {
            //CLI_RESPONSE cli_Response = new CLI_RESPONSE();
            string output = string.Empty;
            bool retcode = false;

            if (_AllInfoMonitors == null)
                _AllInfoMonitors = await devMgr.GetMonitors();

            switch (commandLineInput.Command)
            {
                case "GET":
                    if (commandLineInput.DeviceIndex.Count == 0 && commandLineInput.ServiceTag.Count == 0 && commandLineInput.Model.Count == 0)
                    {
                        foreach (MonitorInfo monitor in _AllInfoMonitors)
                        {
                            ObjGetVCP rc = new ObjGetVCP();
                            CLI_RESPONSE cli_Response = new CLI_RESPONSE(monitor);
                            cli_Response.Command = commandLineInput.Command;
                            cli_Response.TargetFeature = commandLineInput.TargetFeature;

                            rc = GetVCPCode(devMgr, monitor, "0xC0").Result;
                            if (rc != null)
                            {
                                cli_Response.Value = (rc.value).ToString() + " hours";
                                retcode = true;
                            }
                            if (retcode)
                            {
                                cli_Response.Result = "PASS";
                                cli_Response.Message = "N/A";
                            }
                            else
                            {
                                cli_Response.Result = "FAIL";
                                cli_Response.Message = "Invalid command line syntax.";
                            }
                            System.Console.WriteLine(JsonConvert.SerializeObject(cli_Response, Formatting.Indented));
                            output += "\n" + JsonConvert.SerializeObject(cli_Response, Formatting.Indented);
                        }
                    }
                    else
                    {
                        foreach (string idx in commandLineInput.DeviceIndex)
                        {
                            MonitorInfo monitor = _AllInfoMonitors[Convert.ToInt32(idx)];
                            CLI_RESPONSE cli_Response = new CLI_RESPONSE(monitor);
                            ObjGetVCP rc = new ObjGetVCP();
                            cli_Response.Command = commandLineInput.Command;
                            cli_Response.TargetFeature = commandLineInput.TargetFeature;

                            rc = GetVCPCode(devMgr, monitor, "0xC0").Result;
                            if (rc != null)
                            {
                                cli_Response.Value = (rc.value).ToString() + " hours";
                                retcode = true;
                            }
                            if (retcode)
                            {
                                cli_Response.Result = "PASS";
                                cli_Response.Message = "N/A";
                            }
                            else
                            {
                                cli_Response.Result = "FAIL";
                                cli_Response.Message = "Invalid command line syntax.";
                            }
                            System.Console.WriteLine(JsonConvert.SerializeObject(cli_Response, Formatting.Indented));
                            output += "\n" + JsonConvert.SerializeObject(cli_Response, Formatting.Indented);
                        }
                        foreach (string tag in commandLineInput.ServiceTag)
                        {
                            var tmp = _AllInfoMonitors.FindAll(x => x.edid.ServiceTag.ToUpper().Equals(tag.ToUpper()));
                            foreach (MonitorInfo monitor in tmp)
                            {
                                CLI_RESPONSE cli_Response = new CLI_RESPONSE(monitor);
                                ObjGetVCP rc = new ObjGetVCP();
                                cli_Response.Command = commandLineInput.Command;
                                cli_Response.TargetFeature = commandLineInput.TargetFeature;

                                rc = GetVCPCode(devMgr, monitor, "0xC0").Result;
                                if (rc != null)
                                {
                                    cli_Response.Value = (rc.value).ToString() + " hours";
                                    retcode = true;
                                }
                                if (retcode)
                                {
                                    cli_Response.Result = "PASS";
                                    cli_Response.Message = "N/A";
                                }
                                else
                                {
                                    cli_Response.Result = "FAIL";
                                    cli_Response.Message = "Invalid command line syntax.";
                                }
                                System.Console.WriteLine(JsonConvert.SerializeObject(cli_Response, Formatting.Indented));
                                output += "\n" + JsonConvert.SerializeObject(cli_Response, Formatting.Indented);
                            }
                        }
                        foreach (string modelName in commandLineInput.Model)
                        {
                            var tmp = _AllInfoMonitors.FindAll(x => x.edid.ModelName.ToUpper().Equals(modelName.ToUpper()));
                            foreach (MonitorInfo monitor in tmp)
                            {
                                CLI_RESPONSE cli_Response = new CLI_RESPONSE(monitor);
                                ObjGetVCP rc = new ObjGetVCP();
                                cli_Response.Command = commandLineInput.Command;
                                cli_Response.TargetFeature = commandLineInput.TargetFeature;

                                rc = GetVCPCode(devMgr, monitor, "0xC0").Result;
                                if (rc != null)
                                {
                                    cli_Response.Value = (rc.value).ToString() + " hours";
                                    retcode = true;
                                }
                                if (retcode)
                                {
                                    cli_Response.Result = "PASS";
                                    cli_Response.Message = "N/A";
                                }
                                else
                                {
                                    cli_Response.Result = "FAIL";
                                    cli_Response.Message = "Invalid command line syntax.";
                                }
                                System.Console.WriteLine(JsonConvert.SerializeObject(cli_Response, Formatting.Indented));
                                output += "\n" + JsonConvert.SerializeObject(cli_Response, Formatting.Indented);
                            }
                        }
                    }
                    break;

                default:
                    CLI_RESPONSE f_cli_Response = new CLI_RESPONSE();
                    f_cli_Response.Command = commandLineInput.Command;
                    f_cli_Response.TargetFeature = commandLineInput.TargetFeature;
                    f_cli_Response.Result = "FAIL";
                    f_cli_Response.Message = "Invalid command line syntax, missing -value=... or more than one -value=...";
                    System.Console.WriteLine(JsonConvert.SerializeObject(f_cli_Response, Formatting.Indented));
                    output += "\n" + JsonConvert.SerializeObject(f_cli_Response, Formatting.Indented);

                    break;
            }
            return (retcode ? (int)CLI_ExitCode.success : (int)CLI_ExitCode.functional_error, output);
        }

        private (int code, string result) EasyarrangeX(IDeviceManagerSA devMgr, CommandLineInput commandLineInput)
        {

            if (commandLineInput.Command == "GET" || commandLineInput.Command == "SET")
            {
                return Easyarrange(devMgr, commandLineInput).Result;
            }
            else
            {
                CLI_RESPONSE cli_Response = new CLI_RESPONSE();
                cli_Response.Command = commandLineInput.Command;
                cli_Response.TargetFeature = commandLineInput.TargetFeature;
                cli_Response.Result = "FAIL";
                cli_Response.Message = "Invalid command line syntax, missing -value=... or more than one -value=...";
                return ((int)CLI_ExitCode.invalide_cmdline_syntax, cli_Response.ToJson());
            }

        }

        private async Task<(int code, string result)> Easyarrange(IDeviceManagerSA devMgr, CommandLineInput commandLineInput)
        {
            string output = string.Empty;
            bool retcode = false;

            if (_AllInfoMonitors == null)
                _AllInfoMonitors = await devMgr.GetMonitors();

            DDPMSettings ddpmSettings = devMgr.ReloadAppConfigData().Result;

            if (ddpmSettings == null)
            {
                return DDPMSettingsNullResponse(commandLineInput.Command, commandLineInput.TargetFeature);
            }

            if (commandLineInput.Command == "GET")
            {
                writelog("Easyarrange get entry");
                List<int> _monitorIndeies = new List<int>();

                if (_AllInfoMonitors == null)
                    _AllInfoMonitors = devMgr.GetMonitors().Result;
                _monitorIndeies = GetMonitorIndeies(commandLineInput, _AllInfoMonitors);

                foreach (int idx in _monitorIndeies)
                {
                    MonitorInfo monitor = _AllInfoMonitors[idx];
                    int rc = -1;
                    CLI_RESPONSE cli_Response = new CLI_RESPONSE(monitor);
                    cli_Response.Command = commandLineInput.Command;
                    cli_Response.TargetFeature = commandLineInput.TargetFeature;

                    writelog("devMgr.GetEASelectedLayout get entry");
                    rc = devMgr.GetEASelectedLayout(monitor).Result;
                    writelog("devMgr.GetEASelectedLayout get exit");

                    retcode = true;
                    cli_Response.Result = "PASS";
                    if (rc == 0)
                    {
                        cli_Response.Value = "OFF";
                    }
                    else if (rc > 0)
                    {
                        cli_Response.Value = "ON";
                    }
                    else
                    {
                        retcode = false;
                        cli_Response.Result = "FAIL";
                    }

                    //cli_Response.Value += "," + (ddpmSettings.LockSettings.Lock_Display_EasyArrangeLayout ? "LOCK" : "UNLOCK");
                    Console.WriteLine(cli_Response.ToJson());
                    output += "\n" + cli_Response.ToJson();
                }
            }
            if (commandLineInput.Command == "SET")
            {
                writelog("Easyarrange set entry");
                List<int> _monitorIndeies = new List<int>();

                if (_AllInfoMonitors == null)
                    _AllInfoMonitors = devMgr.GetMonitors().Result;
                _monitorIndeies = GetMonitorIndeies(commandLineInput, _AllInfoMonitors);

                foreach (int idx in _monitorIndeies)
                {
                    MonitorInfo monitor = _AllInfoMonitors[idx];
                    CLI_RESPONSE cli_Response = new CLI_RESPONSE(monitor);
                    cli_Response.Command = commandLineInput.Command;
                    cli_Response.TargetFeature = commandLineInput.TargetFeature;
                    cli_Response.Value = commandLineInput.Options[0].Option_Value;

                    //if (commandLineInput.Options[0].Option_Value == "ENABLE")
                    //{
                    //    elable_ea = true;
                    //    ddpmSettings.LockSettings.Lock_Display_EasyArrangeLayout = false;
                    //}
                    //else if (commandLineInput.Options[0].Option_Value == "DISABLE")
                    //{
                    //    elable_ea = false;
                    //    ddpmSettings.LockSettings.Lock_Display_EasyArrangeLayout = true;
                    //}
                    if (commandLineInput.Options[0].Option_Value == "OFF")
                    {
                        writelog("SetEAFunctionEnabled set entry");
                        retcode = devMgr.SetEASelectedLayout(monitor, 0).Result;
                        writelog("SetEAFunctionEnabled set exit");
                    }
                    else
                    {
                        cli_Response.Result = "FAIL";
                        cli_Response.Message = "Invalid command line syntax, missing -value=... or more than one -value=...";
                        return ((int)CLI_ExitCode.invalide_cmdline_syntax, cli_Response.ToJson());
                    }

                    //await devMgr.SetAppConfigData(ddpmSettings);

                    cli_Response.Result = retcode ? "PASS" : "FAIL";
                    //cli_Response.Value += "," + (ddpmSettings.LockSettings.Lock_Display_EasyArrangeLayout ? "LOCK" : "UNLOCK");
                    Console.WriteLine(cli_Response.ToJson());
                    output += "\n" + cli_Response.ToJson();
                }
            }
            writelog($"Output={output}");
            return (retcode ? (int)CLI_ExitCode.success : (int)CLI_ExitCode.functional_error, output);
        }

        // 提供給Robert debug使用
        private async Task<(int code, string result)> GetSetEA(IDeviceManagerSA devMgr, CommandLineInput commandLineInput)
        {
            string output = string.Empty;
            bool retcode = false;

            if (_AllInfoMonitors == null)
                _AllInfoMonitors = await devMgr.GetMonitors();

            DDPMSettings ddpmSettings = devMgr.ReloadAppConfigData().Result;

            if (ddpmSettings == null)
            {
                return DDPMSettingsNullResponse(commandLineInput.Command, commandLineInput.TargetFeature);
            }

            if (commandLineInput.Command == "GET")
            {
                writelog("Easyarrange get entry");
                List<int> _monitorIndeies = new List<int>();

                if (_AllInfoMonitors == null)
                    _AllInfoMonitors = devMgr.GetMonitors().Result;
                _monitorIndeies = GetMonitorIndeies(commandLineInput, _AllInfoMonitors);

                foreach (int idx in _monitorIndeies)
                {
                    MonitorInfo monitor = _AllInfoMonitors[idx];
                    int rc = -1;
                    CLI_RESPONSE cli_Response = new CLI_RESPONSE(monitor);
                    cli_Response.Command = commandLineInput.Command;
                    cli_Response.TargetFeature = commandLineInput.TargetFeature;

                    writelog("devMgr.GetEASelectedLayout get entry");
                    rc = devMgr.GetEASelectedLayout(monitor).Result;
                    writelog("devMgr.GetEASelectedLayout get exit");

                    retcode = true;
                    cli_Response.Result = "PASS";
                    if (rc >= 0)
                    {
                        cli_Response.Value = rc.ToString();
                    }
                    else
                    {
                        retcode = false;
                        cli_Response.Result = "FAIL";
                    }

                    Console.WriteLine(cli_Response.ToJson());
                    output += "\n" + cli_Response.ToJson();
                }
            }
            if (commandLineInput.Command == "SET")
            {
                writelog("Easyarrange set entry");
                List<int> _monitorIndeies = new List<int>();

                if (_AllInfoMonitors == null)
                    _AllInfoMonitors = devMgr.GetMonitors().Result;
                _monitorIndeies = GetMonitorIndeies(commandLineInput, _AllInfoMonitors);

                foreach (int idx in _monitorIndeies)
                {
                    MonitorInfo monitor = _AllInfoMonitors[idx];
                    CLI_RESPONSE cli_Response = new CLI_RESPONSE(monitor);
                    cli_Response.Command = commandLineInput.Command;
                    cli_Response.TargetFeature = commandLineInput.TargetFeature;
                    cli_Response.Value = commandLineInput.Options[0].Option_Value;

                    if (int.TryParse(commandLineInput.Options[0].Option_Value, out int value))
                    {
                        writelog("SetEAFunctionEnabled set entry");
                        retcode = devMgr.SetEASelectedLayout(monitor, value).Result;
                        writelog("SetEAFunctionEnabled set exit");
                    }
                    else
                    {
                        cli_Response.Result = "FAIL";
                        cli_Response.Message = "Invalid command line syntax, missing -value=... or more than one -value=...";
                        return ((int)CLI_ExitCode.invalide_cmdline_syntax, cli_Response.ToJson());
                    }


                    cli_Response.Result = retcode ? "PASS" : "FAIL";
                    Console.WriteLine(cli_Response.ToJson());
                    output += "\n" + cli_Response.ToJson();
                }
            }
            writelog($"Output={output}");
            return (retcode ? (int)CLI_ExitCode.success : (int)CLI_ExitCode.functional_error, output);
        }

        private static void LaunchNetworkkvmApp(ILog log)
        {
            string registryKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\" + GlobalDefinitions.DDMExeName;
            string valueName = "(Default)";

            using (RegistryKey key = Registry.LocalMachine.OpenSubKey(registryKey))
            {
                Trace.WriteLine("key: " + key);
                if (key != null)
                {
                    object value = key.GetValue(null);

                    if (value != null)
                    {
                        string executablePath = value.ToString();
                        string exeFileAndLocation = executablePath;
                        string arguments = "/console start";
                        //Process.Start(exeFileAndLocation, arguments);
                        //Trace.WriteLine("Executable Path: " + executablePath);
                        //DDPMFileSecurity.StartProcessSafely(log, exeFileAndLocation, arguments, true);
                        DDPMFileSecurity.StartProcessSafely(log, exeFileAndLocation, arguments, true, "", "", false);//lock nkvm
                    }
                    else
                    {
                        Trace.WriteLine("Value not found.");
                    }
                }
                else
                {
                    Trace.WriteLine("Registry key not found.");
                }
            }
        }

        private static void LaunchNetworkkvmApp_on(ILog log)
        {
            string registryKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\" + GlobalDefinitions.DDMExeName;
            string valueName = "(Default)";

            using (RegistryKey key = Registry.LocalMachine.OpenSubKey(registryKey))
            {
                Trace.WriteLine("key: " + key);
                if (key != null)
                {
                    object value = key.GetValue(null);

                    if (value != null)
                    {
                        string executablePath = value.ToString();
                        string exeFileAndLocation = executablePath;
                        string arguments = "/networkkvm on";
                        //Process.Start(exeFileAndLocation, arguments);
                        //Trace.WriteLine("Executable Path: " + executablePath);
                        //DDPMFileSecurity.StartProcessSafely(log, exeFileAndLocation, arguments, true);
                        DDPMFileSecurity.StartProcessSafely(log, exeFileAndLocation, arguments, true, "", "", false);//lock nkvm
                    }
                    else
                    {
                        Trace.WriteLine("Value not found.");
                    }
                }
                else
                {
                    Trace.WriteLine("Registry key not found.");
                }
            }
        }

        private static void LaunchNetworkkvmApp_off(ILog log)
        {
            string registryKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\" + GlobalDefinitions.DDMExeName;
            string valueName = "(Default)";

            using (RegistryKey key = Registry.LocalMachine.OpenSubKey(registryKey))
            {
                Trace.WriteLine("key: " + key);
                if (key != null)
                {
                    object value = key.GetValue(null);

                    if (value != null)
                    {
                        string executablePath = value.ToString();
                        string exeFileAndLocation = executablePath;
                        string arguments = "/networkkvm off";
                        //Process.Start(exeFileAndLocation, arguments);
                        //Trace.WriteLine("Executable Path: " + executablePath);
                        //DDPMFileSecurity.StartProcessSafely(log, exeFileAndLocation, arguments, true);
                        DDPMFileSecurity.StartProcessSafely(log, exeFileAndLocation, arguments, true, "", "", false);//lock nkvm
                    }
                    else
                    {
                        Trace.WriteLine("Value not found.");
                    }
                }
                else
                {
                    Trace.WriteLine("Registry key not found.");
                }
            }
        }

        private static void LaunchNetworkkvmautoconnectApp_on(ILog log)
        {
            string registryKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\" + GlobalDefinitions.DDMExeName;
            string valueName = "(Default)";

            using (RegistryKey key = Registry.LocalMachine.OpenSubKey(registryKey))
            {
                Trace.WriteLine("key: " + key);
                if (key != null)
                {
                    object value = key.GetValue(null);

                    if (value != null)
                    {
                        string executablePath = value.ToString();
                        string exeFileAndLocation = executablePath;
                        string arguments = "/networkkvmautoconnect on";
                        //Process.Start(exeFileAndLocation, arguments);
                        //Trace.WriteLine("Executable Path: " + executablePath);
                        //DDPMFileSecurity.StartProcessSafely(log, exeFileAndLocation, arguments, true);
                        DDPMFileSecurity.StartProcessSafely(log, exeFileAndLocation, arguments, true, "", "", false);//lock nkvm
                    }
                    else
                    {
                        Trace.WriteLine("Value not found.");
                    }
                }
                else
                {
                    Trace.WriteLine("Registry key not found.");
                }
            }
        }

        private static void LaunchNetworkkvmautoconnectApp_off(ILog log)
        {
            string registryKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\" + GlobalDefinitions.DDMExeName;
            string valueName = "(Default)";

            using (RegistryKey key = Registry.LocalMachine.OpenSubKey(registryKey))
            {
                Trace.WriteLine("key: " + key);
                if (key != null)
                {
                    object value = key.GetValue(null);

                    if (value != null)
                    {
                        string executablePath = value.ToString();
                        string exeFileAndLocation = executablePath;
                        string arguments = "/networkkvmautoconnect off";
                        //Process.Start(exeFileAndLocation, arguments);
                        //Trace.WriteLine("Executable Path: " + executablePath);
                        //DDPMFileSecurity.StartProcessSafely(log, exeFileAndLocation, arguments, true);
                        DDPMFileSecurity.StartProcessSafely(log, exeFileAndLocation, arguments, true, "", "", false);//lock nkvm
                    }
                    else
                    {
                        Trace.WriteLine("Value not found.");
                    }
                }
                else
                {
                    Trace.WriteLine("Registry key not found.");
                }
            }
        }

        private static void LaunchNetworkkvmcontenttransferApp_on(ILog log)
        {
            string registryKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\" + GlobalDefinitions.DDMExeName;
            string valueName = "(Default)";

            using (RegistryKey key = Registry.LocalMachine.OpenSubKey(registryKey))
            {
                Trace.WriteLine("key: " + key);
                if (key != null)
                {
                    object value = key.GetValue(null);

                    if (value != null)
                    {
                        string executablePath = value.ToString();
                        string exeFileAndLocation = executablePath;
                        string arguments = "/networkkvmcontenttransfer on";
                        //Process.Start(exeFileAndLocation, arguments);
                        //Trace.WriteLine("Executable Path: " + executablePath);
                        //DDPMFileSecurity.StartProcessSafely(log, exeFileAndLocation, arguments, true);
                        DDPMFileSecurity.StartProcessSafely(log, exeFileAndLocation, arguments, true, "", "", false);//lock nkvm
                    }
                    else
                    {
                        Trace.WriteLine("Value not found.");
                    }
                }
                else
                {
                    Trace.WriteLine("Registry key not found.");
                }
            }
        }

        private static void LaunchNetworkkvmcontenttransferApp_off(ILog log)
        {
            string registryKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\" + GlobalDefinitions.DDMExeName;
            string valueName = "(Default)";

            using (RegistryKey key = Registry.LocalMachine.OpenSubKey(registryKey))
            {
                Trace.WriteLine("key: " + key);
                if (key != null)
                {
                    object value = key.GetValue(null);

                    if (value != null)
                    {
                        string executablePath = value.ToString();
                        string exeFileAndLocation = executablePath;
                        string arguments = "/networkkvmcontenttransfer off";
                        //Process.Start(exeFileAndLocation, arguments);
                        //Trace.WriteLine("Executable Path: " + executablePath);
                        //DDPMFileSecurity.StartProcessSafely(log, exeFileAndLocation, arguments, true);
                        DDPMFileSecurity.StartProcessSafely(log, exeFileAndLocation, arguments, true, "", "", false);//lock nkvm
                    }
                    else
                    {
                        Trace.WriteLine("Value not found.");
                    }
                }
                else
                {
                    Trace.WriteLine("Registry key not found.");
                }
            }
        }

        private static void LaunchNetworkkvmincomingportApp(ILog log, CommandLineInput commandLineInput)
        {
            string registryKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\" + GlobalDefinitions.DDMExeName;
            string valueName = "(Default)";

            using (RegistryKey key = Registry.LocalMachine.OpenSubKey(registryKey))
            {
                Trace.WriteLine("key: " + key);
                if (key != null)
                {
                    object value = key.GetValue(null);

                    if (value != null)
                    {
                        string executablePath = value.ToString();
                        string exeFileAndLocation = executablePath;
                        string arguments = $"/networkkvmincomingport {commandLineInput.Options[0].Option_Value}";
                        //Process.Start(exeFileAndLocation, arguments);
                        //Trace.WriteLine("Executable Path: " + executablePath);
                        //DDPMFileSecurity.StartProcessSafely(log, exeFileAndLocation, arguments, true);
                        DDPMFileSecurity.StartProcessSafely(log, exeFileAndLocation, arguments, true, "", "", false);//lock nkvm
                    }
                    else
                    {
                        Trace.WriteLine("Value not found.");
                    }
                }
                else
                {
                    Trace.WriteLine("Registry key not found.");
                }
            }
        }

        private static void LaunchNetworkkvmoutgoingportApp(ILog log, CommandLineInput commandLineInput)
        {
            string registryKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\" + GlobalDefinitions.DDMExeName;
            string valueName = "(Default)";

            using (RegistryKey key = Registry.LocalMachine.OpenSubKey(registryKey))
            {
                Trace.WriteLine("key: " + key);
                if (key != null)
                {
                    object value = key.GetValue(null);

                    if (value != null)
                    {
                        string executablePath = value.ToString();
                        string exeFileAndLocation = executablePath;
                        string arguments = $"/networkkvmoutgoingport {commandLineInput.Options[0].Option_Value}";
                        //Process.Start(exeFileAndLocation, arguments);
                        //Trace.WriteLine("Executable Path: " + executablePath);
                        //DDPMFileSecurity.StartProcessSafely(log, exeFileAndLocation, arguments, true);
                        DDPMFileSecurity.StartProcessSafely(log, exeFileAndLocation, arguments, true, "", "", false);//lock nkvm
                    }
                    else
                    {
                        Trace.WriteLine("Value not found.");
                    }
                }
                else
                {
                    Trace.WriteLine("Registry key not found.");
                }
            }
        }

        private static void LaunchNetworkkvmcontenttransferportApp(ILog log, CommandLineInput commandLineInput)
        {
            string registryKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\" + GlobalDefinitions.DDMExeName;
            string valueName = "(Default)";

            using (RegistryKey key = Registry.LocalMachine.OpenSubKey(registryKey))
            {
                Trace.WriteLine("key: " + key);
                if (key != null)
                {
                    object value = key.GetValue(null);

                    if (value != null)
                    {
                        string executablePath = value.ToString();
                        string exeFileAndLocation = executablePath;
                        string arguments = $"/networkkvmcontenttransferport {commandLineInput.Options[0].Option_Value}";
                        //Process.Start(exeFileAndLocation, arguments);
                        //Trace.WriteLine("Executable Path: " + executablePath);
                        //DDPMFileSecurity.StartProcessSafely(log, exeFileAndLocation, arguments, true);
                        DDPMFileSecurity.StartProcessSafely(log, exeFileAndLocation, arguments, true, "", "", false);//lock nkvm
                    }
                    else
                    {
                        Trace.WriteLine("Value not found.");
                    }
                }
                else
                {
                    Trace.WriteLine("Registry key not found.");
                }
            }
        }

        private static void LaunchNetworkkvmaccessresetApp(ILog log)
        {
            string registryKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\" + GlobalDefinitions.DDMExeName;
            string valueName = "(Default)";

            using (RegistryKey key = Registry.LocalMachine.OpenSubKey(registryKey))
            {
                Trace.WriteLine("key: " + key);
                if (key != null)
                {
                    object value = key.GetValue(null);

                    if (value != null)
                    {
                        string executablePath = value.ToString();
                        string exeFileAndLocation = executablePath;
                        string arguments = $"/networkkvmaccessreset";
                        //Process.Start(exeFileAndLocation, arguments);
                        //Trace.WriteLine("Executable Path: " + executablePath);
                        //DDPMFileSecurity.StartProcessSafely(log, exeFileAndLocation, arguments, true);
                        DDPMFileSecurity.StartProcessSafely(log, exeFileAndLocation, arguments, true, "", "", false);//lock nkvm
                    }
                    else
                    {
                        Trace.WriteLine("Value not found.");
                    }
                }
                else
                {
                    Trace.WriteLine("Registry key not found.");
                }
            }
        }

        private (int code, string result) NetworkkvmVersionx(CommandLineInput commandLineInput)
        {
            if (commandLineInput.Command == "GET")
            {
                return NetworkkvmVersion(commandLineInput).Result;
            }
            else
            {
                CLI_RESPONSE cli_Response = new CLI_RESPONSE();
                cli_Response.Command = commandLineInput.Command;
                cli_Response.TargetFeature = commandLineInput.TargetFeature;
                cli_Response.Result = "FAIL";
                cli_Response.Message = $"Un-supported command: {commandLineInput.Command}";
                return ((int)CLI_ExitCode.unknow_command, cli_Response.ToJson());
            }
        }

        private async Task<(int code, string result)> NetworkkvmVersion(CommandLineInput commandLineInput)
        {
            string output = string.Empty;
            bool retcode = false;

            NKVM_RESPONSE cli_Response = new NKVM_RESPONSE();
            cli_Response.Command = commandLineInput.Command;
            cli_Response.TargetFeature = commandLineInput.TargetFeature;

            string filePath = @"C:\Program Files\Dell\Dell Display and Peripheral Manager\Plugins\NKVM\" + GlobalDefinitions.DDMExeName;

            if (File.Exists(filePath))
            {
                FileVersionInfo fileVersionInfo = FileVersionInfo.GetVersionInfo(filePath);
                string version = fileVersionInfo.FileVersion;

                if (!string.IsNullOrWhiteSpace(version))
                {
                    cli_Response.Value = version;
                    cli_Response.Result = "PASS";
                    cli_Response.Message = $"Version: {version}";
                    retcode = true;
                }
                else
                {
                    cli_Response.Result = "FAIL";
                    cli_Response.Message = $"Fail to get NetworkKVM Version";
                    retcode = false;
                }
            }
            else
            {
                cli_Response.Result = "FAIL";
                cli_Response.Message = $"{filePath} is not exists.";
            }

            Console.WriteLine(JsonConvert.SerializeObject(cli_Response, Formatting.Indented));
            output = JsonConvert.SerializeObject(cli_Response, Formatting.Indented);
            return (retcode ? (int)CLI_ExitCode.success : (int)CLI_ExitCode.functional_error, output);
        }

        private (int code, string result) Networkkvmx(IDeviceManagerSA devMgr, CommandLineInput commandLineInput)
        {
            if (commandLineInput.Command == "SET" || commandLineInput.Command == "GET")
            {
                return Networkkvm(devMgr, commandLineInput).Result;
            }
            else
            {
                CLI_RESPONSE cli_Response = new CLI_RESPONSE();
                cli_Response.Command = commandLineInput.Command;
                cli_Response.TargetFeature = commandLineInput.TargetFeature;
                cli_Response.Result = "FAIL";
                cli_Response.Message = "Invalid command line syntax, missing -value=... or more than one -value=...";
                return ((int)CLI_ExitCode.invalide_cmdline_syntax, cli_Response.ToJson());
            }
        }

        public event EventHandler<NKVMRespone> CLIActionEvent;
        private async Task<(int code, string result)> Networkkvm(IDeviceManagerSA devMgr, CommandLineInput commandLineInput)
        {
            string output = string.Empty;
            bool retcode = false;

            if (_AllInfoMonitors == null)
                _AllInfoMonitors = await devMgr.GetMonitors();

            if (commandLineInput.Command == "SET" && commandLineInput.Options[0].Option_Value != null)
            {

                List<int> _monitorIndeies = new List<int>();

                if (_AllInfoMonitors == null)
                    _AllInfoMonitors = devMgr.GetMonitors().Result;
                _monitorIndeies = GetMonitorIndeies(commandLineInput, _AllInfoMonitors);

                //foreach (int idx in _monitorIndeies)
                //{
                //    //LaunchNetworkkvmApp(); //Open DDM console for debug

                //    MonitorInfo monitor = _AllInfoMonitors[idx];

                NKVM_RESPONSE cli_Response = new NKVM_RESPONSE();
                cli_Response.Command = commandLineInput.Command;
                cli_Response.TargetFeature = commandLineInput.TargetFeature;
                //cli_Response.Model = monitor.modelName;
                //cli_Response.SerialNumber = monitor.edid.SerialNumber;
                //cli_Response.Index = change_0base_to_1base((monitor.Index).ToString());
                //cli_Response.ServiceTag = monitor.edid.ServiceTag;

                if (!string.IsNullOrEmpty(commandLineInput.Options[0].Option_Value))
                {
                    switch (commandLineInput.Options[0].Option_Value.ToUpper())
                    {
                        case "ON":
                            writelog($"Networkkvm on entry");
                            LaunchNetworkkvmApp_on(Log);
                            retcode = true;
                            cli_Response.Result = "PASS";
                            cli_Response.Value = "ON";
                            break;

                        case "OFF":
                            writelog($"Networkkvm off entry");
                            LaunchNetworkkvmApp_off(Log);
                            retcode = true;
                            cli_Response.Result = "PASS";
                            cli_Response.Value = "OFF";
                            break;

                        default:
                            writelog($"option value not support");

                            cli_Response.Command = commandLineInput.Command;
                            cli_Response.TargetFeature = commandLineInput.TargetFeature;
                            cli_Response.Result = "FAIL";
                            cli_Response.Message = "Invalid command line syntax, missing -value=... or more than one -value=...";
                            break;
                    }

                }
                System.Console.WriteLine(JsonConvert.SerializeObject(cli_Response, Formatting.Indented));
                output += "\n" + JsonConvert.SerializeObject(cli_Response, Formatting.Indented);
                //}
            }
            if (commandLineInput.Command == "GET")
            {
                List<int> _monitorIndeies = new List<int>();
                if (_AllInfoMonitors == null)
                    _AllInfoMonitors = devMgr.GetMonitors().Result;
                _monitorIndeies = GetMonitorIndeies(commandLineInput, _AllInfoMonitors);

                //foreach (int idx in _monitorIndeies)
                //{
                //LaunchNetworkkvmApp(); //Open DDM console for debug
                writelog($"Networkkvm get entry");

                //MonitorInfo monitor = _AllInfoMonitors[idx];

                NKVM_RESPONSE cli_Response = new NKVM_RESPONSE();
                cli_Response.Command = commandLineInput.Command;
                cli_Response.TargetFeature = commandLineInput.TargetFeature;
                //cli_Response.Model = monitor.modelName;
                //cli_Response.SerialNumber = monitor.edid.SerialNumber;
                //cli_Response.Index = change_0base_to_1base((monitor.Index).ToString());
                //cli_Response.ServiceTag = monitor.edid.ServiceTag;

                CLIActionEvent += OnCLINKVMv2;
                await devMgr.GetNKVMStatus();
                Task.Delay(10000).Wait();


                System.Console.WriteLine(JsonConvert.SerializeObject(cli_Response, Formatting.Indented));
                output += "\n" + JsonConvert.SerializeObject(cli_Response, Formatting.Indented);
            }
            //}
            writelog($"Networkkvm exit return value : {output}");
            return (retcode ? (int)CLI_ExitCode.success : (int)CLI_ExitCode.functional_error, output);
        }
        private void OnCLINKVMv2(object sender, NKVMRespone e)
        {
            //EventHandler<NKVMRespone> handler = CLIActionEvent;

            if (e.CLIName != null)
                Trace.WriteLine($"TRUE");
            else
                Trace.WriteLine($"FALSE");
        }
        private (int code, string result) InAppUSBkvmx(IDeviceManagerSA devMgr, CommandLineInput commandLineInput)
        {
            if (commandLineInput.Command == "SET" || commandLineInput.Command == "GET")
            {
                return InAppUSBkvm(devMgr, commandLineInput).Result;
            }
            else
            {
                CLI_RESPONSE cli_Response = new CLI_RESPONSE();
                cli_Response.Command = commandLineInput.Command;
                cli_Response.TargetFeature = commandLineInput.TargetFeature;
                cli_Response.Result = "FAIL";
                cli_Response.Message = "Invalid command line syntax, missing -value=... or more than one -value=...";
                return ((int)CLI_ExitCode.invalide_cmdline_syntax, cli_Response.ToJson());
            }
        }

        private async Task<(int code, string result)> InAppUSBkvm(IDeviceManagerSA devMgr, CommandLineInput commandLineInput)
        {
            string output = string.Empty;
            bool retcode = false;

            if (_AllInfoMonitors == null)
                _AllInfoMonitors = await devMgr.GetMonitors();

            DDPMSettings ddpmSettings = devMgr.ReloadAppConfigData().Result;

            if (ddpmSettings == null)
            {
                return DDPMSettingsNullResponse(commandLineInput.Command, commandLineInput.TargetFeature);
            }

            if (commandLineInput.Command == "SET" && commandLineInput.Options[0].Option_Value != null)
            {

                List<int> _monitorIndeies = new List<int>();

                if (_AllInfoMonitors == null)
                    _AllInfoMonitors = devMgr.GetMonitors().Result;
                _monitorIndeies = GetMonitorIndeies(commandLineInput, _AllInfoMonitors);

                foreach (int idx in _monitorIndeies)
                {
                    MonitorInfo monitor = _AllInfoMonitors[idx];

                    CLI_RESPONSE cli_Response = new CLI_RESPONSE(monitor);
                    cli_Response.Command = commandLineInput.Command;
                    cli_Response.TargetFeature = commandLineInput.TargetFeature;

                    switch (commandLineInput.Options[0].Option_Value.ToUpper())
                    {
                        //case "ENABLE":
                        //    writelog($"InAppUSBkvm on entry");
                        //    retcode = devMgr.SetOnUSBKVM(monitor, true).Result;
                        //    ddpmSettings.LockSettings.Lock_Display_USBKVM = false;
                        //    await devMgr.SetAppConfigData(ddpmSettings);
                        //    cli_Response.Result = "PASS";
                        //    cli_Response.Value = commandLineInput.Options[0].Option_Value.ToUpper();
                        //    break;

                        case "OFF"://"DISABLE":
                            writelog($"InAppUSBkvm off entry");
                            retcode = devMgr.SetOnUSBKVM(monitor, false).Result;
                            //ddpmSettings.LockSettings.Lock_Display_USBKVM = true;
                            //await devMgr.SetAppConfigData(ddpmSettings);
                            cli_Response.Result = "PASS";
                            cli_Response.Value = commandLineInput.Options[0].Option_Value.ToUpper();
                            break;

                        default:
                            writelog($"option value not support");

                            cli_Response.Command = commandLineInput.Command;
                            cli_Response.TargetFeature = commandLineInput.TargetFeature;
                            cli_Response.Result = "FAIL";
                            cli_Response.Message = "Invalid command line syntax, missing -value=... or more than one -value=...";
                            break;
                    }
                    //cli_Response.Value += "," + (ddpmSettings.LockSettings.Lock_Display_USBKVM ? "LOCK" : "UNLOCK");
                    System.Console.WriteLine(JsonConvert.SerializeObject(cli_Response, Formatting.Indented));
                    output += "\n" + JsonConvert.SerializeObject(cli_Response, Formatting.Indented);
                }
            }
            if (commandLineInput.Command == "GET")
            {
                List<int> _monitorIndeies = new List<int>();
                if (_AllInfoMonitors == null)
                    _AllInfoMonitors = devMgr.GetMonitors().Result;
                _monitorIndeies = GetMonitorIndeies(commandLineInput, _AllInfoMonitors);

                foreach (int idx in _monitorIndeies)
                {
                    writelog($"InAppUSBkvm get entry");

                    MonitorInfo monitor = _AllInfoMonitors[idx];
                    bool rc = true;
                    rc = devMgr.GetOnUSBKVM(monitor).Result;

                    CLI_RESPONSE cli_Response = new CLI_RESPONSE(monitor);
                    cli_Response.Command = commandLineInput.Command;
                    cli_Response.TargetFeature = commandLineInput.TargetFeature;
                    cli_Response.Result = "PASS";
                    //if (rc.ToString() == "True")
                    //    cli_Response.Value = "ENABLE";
                    //else
                    //    cli_Response.Value = "DISABLE";
                    if (rc.ToString() == "True")
                        cli_Response.Value = "ON";
                    else
                        cli_Response.Value = "OFF";

                    //cli_Response.Value += "," + (ddpmSettings.LockSettings.Lock_Display_USBKVM ? "LOCK" : "UNLOCK");
                    System.Console.WriteLine(JsonConvert.SerializeObject(cli_Response, Formatting.Indented));
                    output += "\n" + JsonConvert.SerializeObject(cli_Response, Formatting.Indented);
                }
            }
            writelog($"InAppUSBkvm exit return value : {output}");
            return (retcode ? (int)CLI_ExitCode.success : (int)CLI_ExitCode.functional_error, output);
        }

        private (int code, string result) Networkkvmautoconnectx(IDeviceManagerSA devMgr, CommandLineInput commandLineInput)
        {
            if (commandLineInput.Command == "SET" || commandLineInput.Command == "GET")
            {
                return Networkkvmautoconnect(devMgr, commandLineInput).Result;
            }
            else
            {
                NKVM_RESPONSE cli_Response = new NKVM_RESPONSE();
                cli_Response.Command = commandLineInput.Command;
                cli_Response.TargetFeature = commandLineInput.TargetFeature;
                cli_Response.Result = "FAIL";
                cli_Response.Message = "Invalid command line syntax, missing -value=... or more than one -value=...";
                return ((int)CLI_ExitCode.invalide_cmdline_syntax, cli_Response.ToJson());
            }
        }

        private async Task<(int code, string result)> Networkkvmautoconnect(IDeviceManagerSA devMgr, CommandLineInput commandLineInput)
        {
            string output = string.Empty;
            bool retcode = false;

            if (_AllInfoMonitors == null)
                _AllInfoMonitors = await devMgr.GetMonitors();

            if (commandLineInput.Command == "SET" && commandLineInput.Options[0].Option_Value != null)
            {
                List<int> _monitorIndeies = new List<int>();

                if (_AllInfoMonitors == null)
                    _AllInfoMonitors = devMgr.GetMonitors().Result;
                _monitorIndeies = GetMonitorIndeies(commandLineInput, _AllInfoMonitors);

                //foreach (int idx in _monitorIndeies)
                //{
                //LaunchNetworkkvmApp(); //Open DDM console for debug

                //MonitorInfo monitor = _AllInfoMonitors[idx];

                NKVM_RESPONSE cli_Response = new NKVM_RESPONSE();
                cli_Response.Command = commandLineInput.Command;
                cli_Response.TargetFeature = commandLineInput.TargetFeature;
                //cli_Response.Model = monitor.modelName;
                //cli_Response.SerialNumber = monitor.edid.SerialNumber;
                //cli_Response.Index = change_0base_to_1base((monitor.Index).ToString());
                //cli_Response.ServiceTag = monitor.edid.ServiceTag;
                if (!string.IsNullOrEmpty(commandLineInput.Options[0].Option_Value))
                {
                    switch (commandLineInput.Options[0].Option_Value.ToUpper())
                    {
                        case "ON":
                            writelog($"Networkkvmautoconnect on entry");
                            LaunchNetworkkvmautoconnectApp_on(Log);
                            retcode = true;
                            cli_Response.Result = "PASS";
                            cli_Response.Value = "ON";
                            break;

                        case "OFF":
                            writelog($"Networkkvmautoconnect off entry");
                            LaunchNetworkkvmautoconnectApp_off(Log);
                            retcode = true;
                            cli_Response.Result = "PASS";
                            cli_Response.Value = "OFF";
                            break;

                        default:
                            writelog($"option value not support");

                            cli_Response.Command = commandLineInput.Command;
                            cli_Response.TargetFeature = commandLineInput.TargetFeature;
                            cli_Response.Result = "FAIL";
                            cli_Response.Message = "Invalid command line syntax, missing -value=... or more than one -value=...";
                            break;
                    }
                }
                System.Console.WriteLine(JsonConvert.SerializeObject(cli_Response, Formatting.Indented));
                output += "\n" + JsonConvert.SerializeObject(cli_Response, Formatting.Indented);
                //}
            }
            if (commandLineInput.Command == "GET")
            {
                List<int> _monitorIndeies = new List<int>();
                if (_AllInfoMonitors == null)
                    _AllInfoMonitors = devMgr.GetMonitors().Result;
                _monitorIndeies = GetMonitorIndeies(commandLineInput, _AllInfoMonitors);

                //foreach (int idx in _monitorIndeies)
                //{
                //LaunchNetworkkvmApp(); //Open DDM console for debug
                writelog($"Networkkvmautoconnect get entry");
                //MonitorInfo monitor = _AllInfoMonitors[idx];
                NKVM_RESPONSE cli_Response = new NKVM_RESPONSE();
                cli_Response.Command = commandLineInput.Command;
                cli_Response.TargetFeature = commandLineInput.TargetFeature;
                //cli_Response.Model = monitor.modelName;
                //cli_Response.SerialNumber = monitor.edid.SerialNumber;
                //cli_Response.Index = change_0base_to_1base((monitor.Index).ToString());
                //cli_Response.ServiceTag = monitor.edid.ServiceTag;

                System.Console.WriteLine(JsonConvert.SerializeObject(cli_Response, Formatting.Indented));
                output += "\n" + JsonConvert.SerializeObject(cli_Response, Formatting.Indented);
                //}
            }
            writelog($"Networkkvmautoconnect exit return value : {output}");
            return (retcode ? (int)CLI_ExitCode.success : (int)CLI_ExitCode.functional_error, output);
        }

        private (int code, string result) Networkkvmcontenttransferx(IDeviceManagerSA devMgr, CommandLineInput commandLineInput)
        {
            if (commandLineInput.Command == "SET" || commandLineInput.Command == "GET")
            {
                return Networkkvmcontenttransfer(devMgr, commandLineInput).Result;
            }
            else
            {
                NKVM_RESPONSE cli_Response = new NKVM_RESPONSE();
                cli_Response.Command = commandLineInput.Command;
                cli_Response.TargetFeature = commandLineInput.TargetFeature;
                cli_Response.Result = "FAIL";
                cli_Response.Message = "Invalid command line syntax, missing -value=... or more than one -value=...";
                return ((int)CLI_ExitCode.invalide_cmdline_syntax, cli_Response.ToJson());
            }
        }

        private async Task<(int code, string result)> Networkkvmcontenttransfer(IDeviceManagerSA devMgr, CommandLineInput commandLineInput)
        {
            string output = string.Empty;
            bool retcode = false;

            if (_AllInfoMonitors == null)
                _AllInfoMonitors = await devMgr.GetMonitors();

            if (commandLineInput.Command == "SET" && commandLineInput.Options[0].Option_Value != null)
            {
                List<int> _monitorIndeies = new List<int>();

                if (_AllInfoMonitors == null)
                    _AllInfoMonitors = devMgr.GetMonitors().Result;
                _monitorIndeies = GetMonitorIndeies(commandLineInput, _AllInfoMonitors);

                //foreach (int idx in _monitorIndeies)
                //{
                //LaunchNetworkkvmApp(); //Open DDM console for debug

                //MonitorInfo monitor = _AllInfoMonitors[idx];

                NKVM_RESPONSE cli_Response = new NKVM_RESPONSE();
                cli_Response.Command = commandLineInput.Command;
                cli_Response.TargetFeature = commandLineInput.TargetFeature;
                //cli_Response.Model = monitor.modelName;
                //cli_Response.SerialNumber = monitor.edid.SerialNumber;
                //cli_Response.Index = change_0base_to_1base((monitor.Index).ToString());
                //cli_Response.ServiceTag = monitor.edid.ServiceTag;
                if (!string.IsNullOrEmpty(commandLineInput.Options[0].Option_Value))
                {
                    switch (commandLineInput.Options[0].Option_Value.ToUpper())
                    {
                        case "ON":
                            writelog($"Networkkvmcontenttransfer on entry");
                            LaunchNetworkkvmcontenttransferApp_on(Log);
                            retcode = true;
                            cli_Response.Result = "PASS";
                            cli_Response.Value = "ON";
                            break;

                        case "OFF":
                            writelog($"Networkkvmcontenttransfer off entry");
                            LaunchNetworkkvmcontenttransferApp_off(Log);
                            retcode = true;
                            cli_Response.Result = "PASS";
                            cli_Response.Value = "OFF";
                            break;

                        default:
                            writelog($"option value not support");

                            cli_Response.Command = commandLineInput.Command;
                            cli_Response.TargetFeature = commandLineInput.TargetFeature;
                            cli_Response.Result = "FAIL";
                            cli_Response.Message = "Invalid command line syntax, missing -value=... or more than one -value=...";
                            break;
                    }
                }
                System.Console.WriteLine(JsonConvert.SerializeObject(cli_Response, Formatting.Indented));
                output += "\n" + JsonConvert.SerializeObject(cli_Response, Formatting.Indented);
                //}
            }
            if (commandLineInput.Command == "GET")
            {
                List<int> _monitorIndeies = new List<int>();
                if (_AllInfoMonitors == null)
                    _AllInfoMonitors = devMgr.GetMonitors().Result;
                _monitorIndeies = GetMonitorIndeies(commandLineInput, _AllInfoMonitors);

                //foreach (int idx in _monitorIndeies)
                //{
                //LaunchNetworkkvmApp(); //Open DDM console for debug
                writelog($"Networkkvmcontenttransfer get entry");
                //MonitorInfo monitor = _AllInfoMonitors[idx];

                NKVM_RESPONSE cli_Response = new NKVM_RESPONSE();
                cli_Response.Command = commandLineInput.Command;
                cli_Response.TargetFeature = commandLineInput.TargetFeature;
                //cli_Response.Model = monitor.modelName;
                //cli_Response.SerialNumber = monitor.edid.SerialNumber;
                //cli_Response.Index = change_0base_to_1base((monitor.Index).ToString());
                //cli_Response.ServiceTag = monitor.edid.ServiceTag;

                System.Console.WriteLine(JsonConvert.SerializeObject(cli_Response, Formatting.Indented));
                output += "\n" + JsonConvert.SerializeObject(cli_Response, Formatting.Indented);
                //}

            }
            writelog($"Networkkvmcontenttransfer exit return value : {output}");
            return (retcode ? (int)CLI_ExitCode.success : (int)CLI_ExitCode.functional_error, output);
        }

        private (int code, string result) Networkkvmincomingportx(IDeviceManagerSA devMgr, CommandLineInput commandLineInput)
        {
            if (commandLineInput.Command == "SET" || commandLineInput.Command == "GET")
            {
                return Networkkvmincomingport(devMgr, commandLineInput).Result;
            }
            else
            {
                NKVM_RESPONSE cli_Response = new NKVM_RESPONSE();
                cli_Response.Command = commandLineInput.Command;
                cli_Response.TargetFeature = commandLineInput.TargetFeature;
                cli_Response.Result = "FAIL";
                cli_Response.Message = "Invalid command line syntax, missing -value=... or more than one -value=...";
                return ((int)CLI_ExitCode.invalide_cmdline_syntax, cli_Response.ToJson());
            }
        }

        private async Task<(int code, string result)> Networkkvmincomingport(IDeviceManagerSA devMgr, CommandLineInput commandLineInput)
        {
            string output = string.Empty;
            bool retcode = false;

            if (_AllInfoMonitors == null)
                _AllInfoMonitors = await devMgr.GetMonitors();

            if (commandLineInput.Command == "SET" && commandLineInput.Options[0].Option_Value != null)
            {
                List<int> _monitorIndeies = new List<int>();

                if (_AllInfoMonitors == null)
                    _AllInfoMonitors = devMgr.GetMonitors().Result;
                _monitorIndeies = GetMonitorIndeies(commandLineInput, _AllInfoMonitors);

                //foreach (int idx in _monitorIndeies)
                //{
                //LaunchNetworkkvmApp(); //Open DDM console for debug

                //MonitorInfo monitor = _AllInfoMonitors[idx];
                NKVM_RESPONSE cli_Response = new NKVM_RESPONSE();
                cli_Response.Command = commandLineInput.Command;
                cli_Response.TargetFeature = commandLineInput.TargetFeature;
                //cli_Response.Model = monitor.modelName;
                //cli_Response.SerialNumber = monitor.edid.SerialNumber;
                //cli_Response.Index = change_0base_to_1base((monitor.Index).ToString());
                //cli_Response.ServiceTag = monitor.edid.ServiceTag;
                int low = 1024;
                int high = 49151;
                if (!string.IsNullOrEmpty(commandLineInput.Options[0].Option_Value))
                {
                    if (low < Int32.Parse(commandLineInput.Options[0].Option_Value) && Int32.Parse(commandLineInput.Options[0].Option_Value) < high)
                    {
                        writelog($"Networkkvmincomingport entry");
                        LaunchNetworkkvmincomingportApp(Log, commandLineInput);
                        retcode = true;
                        cli_Response.Result = "PASS";
                        cli_Response.Value = $"{commandLineInput.Options[0].Option_Value}";
                    }
                    else
                    {
                        writelog($"option value not support");
                        cli_Response.Command = commandLineInput.Command;
                        cli_Response.TargetFeature = commandLineInput.TargetFeature;
                        cli_Response.Result = "FAIL";
                        cli_Response.Message = "Invalid command line syntax, missing -value=... or more than one -value=...";
                    }
                }
                else
                {
                    writelog($"option value not support");
                    cli_Response.Command = commandLineInput.Command;
                    cli_Response.TargetFeature = commandLineInput.TargetFeature;
                    cli_Response.Result = "FAIL";
                    cli_Response.Message = "Invalid command line syntax, missing -value=... or more than one -value=...";
                }
                System.Console.WriteLine(JsonConvert.SerializeObject(cli_Response, Formatting.Indented));
                output += "\n" + JsonConvert.SerializeObject(cli_Response, Formatting.Indented);
                //}
            }
            if (commandLineInput.Command == "GET")
            {
                List<int> _monitorIndeies = new List<int>();
                if (_AllInfoMonitors == null)
                    _AllInfoMonitors = devMgr.GetMonitors().Result;
                _monitorIndeies = GetMonitorIndeies(commandLineInput, _AllInfoMonitors);

                //foreach (int idx in _monitorIndeies)
                //{
                //LaunchNetworkkvmApp(); //Open DDM console for debug
                writelog($"Networkkvmincomingport get entry");
                //MonitorInfo monitor = _AllInfoMonitors[idx];

                NKVM_RESPONSE cli_Response = new NKVM_RESPONSE();
                cli_Response.Command = commandLineInput.Command;
                cli_Response.TargetFeature = commandLineInput.TargetFeature;
                //cli_Response.Model = monitor.modelName;
                //cli_Response.SerialNumber = monitor.edid.SerialNumber;
                //cli_Response.Index = change_0base_to_1base((monitor.Index).ToString());
                //cli_Response.ServiceTag = monitor.edid.ServiceTag;

                System.Console.WriteLine(JsonConvert.SerializeObject(cli_Response, Formatting.Indented));
                output += "\n" + JsonConvert.SerializeObject(cli_Response, Formatting.Indented);
                //}
            }
            writelog($"Networkkvmincomingport exit return value : {output}");
            return (retcode ? (int)CLI_ExitCode.success : (int)CLI_ExitCode.functional_error, output);
        }

        private (int code, string result) Networkkvmoutgoingportx(IDeviceManagerSA devMgr, CommandLineInput commandLineInput)
        {
            if (commandLineInput.Command == "SET" || commandLineInput.Command == "GET")
            {
                return Networkkvmoutgoingport(devMgr, commandLineInput).Result;
            }
            else
            {
                NKVM_RESPONSE cli_Response = new NKVM_RESPONSE();
                cli_Response.Command = commandLineInput.Command;
                cli_Response.TargetFeature = commandLineInput.TargetFeature;
                cli_Response.Result = "FAIL";
                cli_Response.Message = "Invalid command line syntax, missing -value=... or more than one -value=...";
                return ((int)CLI_ExitCode.invalide_cmdline_syntax, cli_Response.ToJson());
            }
        }

        private async Task<(int code, string result)> Networkkvmoutgoingport(IDeviceManagerSA devMgr, CommandLineInput commandLineInput)
        {
            string output = string.Empty;
            bool retcode = false;

            if (_AllInfoMonitors == null)
                _AllInfoMonitors = await devMgr.GetMonitors();

            if (commandLineInput.Command == "SET" && commandLineInput.Options[0].Option_Value != null)
            {
                List<int> _monitorIndeies = new List<int>();
                if (_AllInfoMonitors == null)
                    _AllInfoMonitors = devMgr.GetMonitors().Result;
                _monitorIndeies = GetMonitorIndeies(commandLineInput, _AllInfoMonitors);

                //foreach (int idx in _monitorIndeies)
                //{
                LaunchNetworkkvmApp(Log); //Open DDM console for debug
                                          //MonitorInfo monitor = _AllInfoMonitors[idx];
                NKVM_RESPONSE cli_Response = new NKVM_RESPONSE();
                cli_Response.Command = commandLineInput.Command;
                cli_Response.TargetFeature = commandLineInput.TargetFeature;
                //cli_Response.Model = monitor.modelName;
                //cli_Response.SerialNumber = monitor.edid.SerialNumber;
                //cli_Response.Index = change_0base_to_1base((monitor.Index).ToString());
                //cli_Response.ServiceTag = monitor.edid.ServiceTag;
                int low = 1024;
                int high = 49151;
                if (!string.IsNullOrEmpty(commandLineInput.Options[0].Option_Value))
                {
                    if (low < Int32.Parse(commandLineInput.Options[0].Option_Value) && Int32.Parse(commandLineInput.Options[0].Option_Value) < high)
                    {
                        writelog($"Networkkvmoutgoingport entry");
                        LaunchNetworkkvmoutgoingportApp(Log, commandLineInput);
                        retcode = true;
                        cli_Response.Result = "PASS";
                        cli_Response.Value = $"{commandLineInput.Options[0].Option_Value}";
                    }
                    else
                    {
                        writelog($"option value not support");
                        cli_Response.Command = commandLineInput.Command;
                        cli_Response.TargetFeature = commandLineInput.TargetFeature;
                        cli_Response.Result = "FAIL";
                        cli_Response.Message = "Invalid command line syntax, missing -value=... or more than one -value=...";
                    }
                }
                else
                {
                    writelog($"option value not support");
                    cli_Response.Command = commandLineInput.Command;
                    cli_Response.TargetFeature = commandLineInput.TargetFeature;
                    cli_Response.Result = "FAIL";
                    cli_Response.Message = "Invalid command line syntax, missing -value=... or more than one -value=...";
                }
                System.Console.WriteLine(JsonConvert.SerializeObject(cli_Response, Formatting.Indented));
                output += "\n" + JsonConvert.SerializeObject(cli_Response, Formatting.Indented);
                //}
            }
            if (commandLineInput.Command == "GET")
            {
                List<int> _monitorIndeies = new List<int>();
                if (_AllInfoMonitors == null)
                    _AllInfoMonitors = devMgr.GetMonitors().Result;
                _monitorIndeies = GetMonitorIndeies(commandLineInput, _AllInfoMonitors);

                //foreach (int idx in _monitorIndeies)
                //{
                //LaunchNetworkkvmApp(); //Open DDM console for debug
                writelog($"Networkkvmoutgoingport get entry");
                //MonitorInfo monitor = _AllInfoMonitors[idx];
                NKVM_RESPONSE cli_Response = new NKVM_RESPONSE();
                cli_Response.Command = commandLineInput.Command;
                cli_Response.TargetFeature = commandLineInput.TargetFeature;
                //cli_Response.Model = monitor.modelName;
                //cli_Response.SerialNumber = monitor.edid.SerialNumber;
                //cli_Response.Index = change_0base_to_1base((monitor.Index).ToString());
                //cli_Response.ServiceTag = monitor.edid.ServiceTag;

                System.Console.WriteLine(JsonConvert.SerializeObject(cli_Response, Formatting.Indented));
                output += "\n" + JsonConvert.SerializeObject(cli_Response, Formatting.Indented);
                //}
            }
            writelog($"Networkkvmoutgoingport exit return value : {output}");
            return (retcode ? (int)CLI_ExitCode.success : (int)CLI_ExitCode.functional_error, output);
        }

        private (int code, string result) Networkkvmcontenttransferportx(IDeviceManagerSA devMgr, CommandLineInput commandLineInput)
        {
            if (commandLineInput.Command == "SET" || commandLineInput.Command == "GET")
            {
                return Networkkvcontenttransferport(devMgr, commandLineInput).Result;
            }
            else
            {
                NKVM_RESPONSE cli_Response = new NKVM_RESPONSE();
                cli_Response.Command = commandLineInput.Command;
                cli_Response.TargetFeature = commandLineInput.TargetFeature;
                cli_Response.Result = "FAIL";
                cli_Response.Message = "Invalid command line syntax, missing -value=... or more than one -value=...";
                return ((int)CLI_ExitCode.invalide_cmdline_syntax, cli_Response.ToJson());
            }
        }

        private async Task<(int code, string result)> Networkkvcontenttransferport(IDeviceManagerSA devMgr, CommandLineInput commandLineInput)
        {
            string output = string.Empty;
            bool retcode = false;

            if (_AllInfoMonitors == null)
                _AllInfoMonitors = await devMgr.GetMonitors();
            NKVM_RESPONSE cli_Response = new NKVM_RESPONSE();
            if (!string.IsNullOrEmpty(commandLineInput.Options[0].Option_Value))
            {
                if (commandLineInput.Command == "SET" && commandLineInput.Options[0].Option_Value != null)
                {

                    List<int> _monitorIndeies = new List<int>();

                    if (_AllInfoMonitors == null)
                        _AllInfoMonitors = devMgr.GetMonitors().Result;
                    _monitorIndeies = GetMonitorIndeies(commandLineInput, _AllInfoMonitors);

                    //foreach (int idx in _monitorIndeies)
                    //{
                    LaunchNetworkkvmApp(Log); //Open DDM console for debug

                    //MonitorInfo monitor = _AllInfoMonitors[idx];

                    cli_Response.Command = commandLineInput.Command;
                    cli_Response.TargetFeature = commandLineInput.TargetFeature;
                    //cli_Response.Model = monitor.modelName;
                    //cli_Response.SerialNumber = monitor.edid.SerialNumber;
                    //cli_Response.Index = change_0base_to_1base((monitor.Index).ToString());
                    //cli_Response.ServiceTag = monitor.edid.ServiceTag;
                    int low = 1024;
                    int high = 49151;

                    if (low < Int32.Parse(commandLineInput.Options[0].Option_Value) && Int32.Parse(commandLineInput.Options[0].Option_Value) < high)
                    {
                        writelog($"Networkkvcontenttransferport entry");
                        LaunchNetworkkvmcontenttransferportApp(Log, commandLineInput);
                        retcode = true;
                        cli_Response.Result = "PASS";
                        cli_Response.Value = $"{commandLineInput.Options[0].Option_Value}";
                    }
                    else
                    {
                        writelog($"option value not support");

                        cli_Response.Command = commandLineInput.Command;
                        cli_Response.TargetFeature = commandLineInput.TargetFeature;
                        cli_Response.Result = "FAIL";
                        cli_Response.Message = "Invalid command line syntax, missing -value=... or more than one -value=...";

                    }
                    System.Console.WriteLine(JsonConvert.SerializeObject(cli_Response, Formatting.Indented));
                    output += "\n" + JsonConvert.SerializeObject(cli_Response, Formatting.Indented);
                    //}
                }
            }
            else
            {
                writelog($"option value not support");

                cli_Response.Command = commandLineInput.Command;
                cli_Response.TargetFeature = commandLineInput.TargetFeature;
                cli_Response.Result = "FAIL";
                cli_Response.Message = "Invalid command line syntax, missing -value=... or more than one -value=...";
            }

            if (commandLineInput.Command == "GET")
            {
                List<int> _monitorIndeies = new List<int>();
                if (_AllInfoMonitors == null)
                    _AllInfoMonitors = devMgr.GetMonitors().Result;
                _monitorIndeies = GetMonitorIndeies(commandLineInput, _AllInfoMonitors);

                //foreach (int idx in _monitorIndeies)
                //{
                //LaunchNetworkkvmApp(); //Open DDM console for debug
                writelog($"Networkkvcontenttransferport get entry");
                //MonitorInfo monitor = _AllInfoMonitors[idx];

                //NKVM_RESPONSE cli_Response = new NKVM_RESPONSE();
                cli_Response.Command = commandLineInput.Command;
                cli_Response.TargetFeature = commandLineInput.TargetFeature;
                //cli_Response.Model = monitor.modelName;
                //cli_Response.SerialNumber = monitor.edid.SerialNumber;
                //cli_Response.Index = change_0base_to_1base((monitor.Index).ToString());
                //cli_Response.ServiceTag = monitor.edid.ServiceTag;

                System.Console.WriteLine(JsonConvert.SerializeObject(cli_Response, Formatting.Indented));
                output += "\n" + JsonConvert.SerializeObject(cli_Response, Formatting.Indented);
                //}
            }
            writelog($"Networkkvcontenttransferport exit return value : {output}");
            return (retcode ? (int)CLI_ExitCode.success : (int)CLI_ExitCode.functional_error, output);
        }

        private (int code, string result) Networkkvmaccessresetx(IDeviceManagerSA devMgr, CommandLineInput commandLineInput)
        {
            if (commandLineInput.Command == "SET" || commandLineInput.Command == "GET")
            {
                return Networkkvmaccessreset(devMgr, commandLineInput).Result;
            }
            else
            {
                NKVM_RESPONSE cli_Response = new NKVM_RESPONSE();
                cli_Response.Command = commandLineInput.Command;
                cli_Response.TargetFeature = commandLineInput.TargetFeature;
                cli_Response.Result = "FAIL";
                cli_Response.Message = "Invalid command line syntax, missing -value=... or more than one -value=...";
                return ((int)CLI_ExitCode.invalide_cmdline_syntax, cli_Response.ToJson());
            }
        }

        private async Task<(int code, string result)> Networkkvmaccessreset(IDeviceManagerSA devMgr, CommandLineInput commandLineInput)
        {
            string output = string.Empty;
            bool retcode = false;

            if (_AllInfoMonitors == null)
                _AllInfoMonitors = await devMgr.GetMonitors();

            if (commandLineInput.Command == "SET")
            {
                List<int> _monitorIndeies = new List<int>();

                if (_AllInfoMonitors == null)
                    _AllInfoMonitors = devMgr.GetMonitors().Result;
                _monitorIndeies = GetMonitorIndeies(commandLineInput, _AllInfoMonitors);

                //foreach (int idx in _monitorIndeies)
                //{
                LaunchNetworkkvmApp(Log); //Open DDM console for debug

                //MonitorInfo monitor = _AllInfoMonitors[idx];
                NKVM_RESPONSE cli_Response = new NKVM_RESPONSE();
                cli_Response.Command = commandLineInput.Command;
                cli_Response.TargetFeature = commandLineInput.TargetFeature;
                //cli_Response.Model = monitor.modelName;
                //cli_Response.SerialNumber = monitor.edid.SerialNumber;
                //cli_Response.Index = change_0base_to_1base((monitor.Index).ToString());
                //cli_Response.ServiceTag = monitor.edid.ServiceTag;
                writelog($"Networkkvmaccessreset entry");
                LaunchNetworkkvmaccessresetApp(Log);
                retcode = true;
                cli_Response.Result = "PASS";
                cli_Response.Value = $"Networkkvmaccessreset";

                System.Console.WriteLine(JsonConvert.SerializeObject(cli_Response, Formatting.Indented));
                output += "\n" + JsonConvert.SerializeObject(cli_Response, Formatting.Indented);
                //}
            }
            if (commandLineInput.Command == "GET")
            {
                List<int> _monitorIndeies = new List<int>();
                if (_AllInfoMonitors == null)
                    _AllInfoMonitors = devMgr.GetMonitors().Result;
                _monitorIndeies = GetMonitorIndeies(commandLineInput, _AllInfoMonitors);

                //foreach (int idx in _monitorIndeies)
                //{
                //LaunchNetworkkvmApp(); //Open DDM console for debug
                writelog($"Networkkvmaccessreset get entry");
                //MonitorInfo monitor = _AllInfoMonitors[idx];
                NKVM_RESPONSE cli_Response = new NKVM_RESPONSE();
                cli_Response.Command = commandLineInput.Command;
                cli_Response.TargetFeature = commandLineInput.TargetFeature;
                //cli_Response.Model = monitor.modelName;
                //cli_Response.SerialNumber = monitor.edid.SerialNumber;
                //cli_Response.Index = change_0base_to_1base((monitor.Index).ToString());
                //cli_Response.ServiceTag = monitor.edid.ServiceTag;

                System.Console.WriteLine(JsonConvert.SerializeObject(cli_Response, Formatting.Indented));
                output += "\n" + JsonConvert.SerializeObject(cli_Response, Formatting.Indented);
                //}
            }
            writelog($"Networkkvmaccessreset exit return value : {output}");
            return (retcode ? (int)CLI_ExitCode.success : (int)CLI_ExitCode.functional_error, output);

        }
        #endregion Malik
        /*
        #region FW Update
        public event EventHandler<(List<FWUpdateInfo>, string)> FWResultReceived;
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
                            ret = Auto_FWUpdate(commandLineInput, cLI_FWU_RESPONSE, installPath, isShowInfo, true);
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
                    ret = Auto_FWUpdate(commandLineInput, cLI_FWU_RESPONSE, installPath, isShowInfo, true);
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
            List<string> tmpReport = new List<string>();
            FWUpdateInfoPackage fwUpdateInfoPackage = _devMgr.GetFWUpdateInfo(isShowInfo, false, isDefer, null, false, true).Result;
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

        //0531 Bruce 因應IL的現有安裝包修改判斷，CLIPeripheralsPlugins.cs中Auto_FWUpdate方法修改回傳值型態和新增判斷
        private List<FWUpdateInfo> retFWUpdateInfos;
        private bool? Auto_FWUpdate(CommandLineInput commandLineInput, CLI_FWU_RESPONSE cli_FWU_RESPONSE, string installPath, bool isShowInfo = true, bool isForce = false)
        {
            try
            {
                _devMgr.ProgressUpdate_Notify -= _FWUpdatePlugin_ProgressUpdate;
                _devMgr.ProgressUpdate_Notify += _FWUpdatePlugin_ProgressUpdate;
                _devMgr.DownloadAndInstall_Result_Notify -= Download_Event;
                _devMgr.DownloadAndInstall_Result_Notify += Download_Event;
                FWUpdateInfoPackage fwUpdateInfoPackage = _devMgr.GetFWUpdateInfo(isShowInfo, isForce, false, null, false, true).Result;
                if (fwUpdateInfoPackage.FWUpdateInfo.Count <= 0)
                {
                    cli_FWU_RESPONSE.Message = "No updates available";
                    return null;
                }
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
                    Task.Delay(100).Wait();
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
            catch
            {
                return false;
            }
        }
        private (int code, string result) Auto_FWUpdate_(CommandLineInput commandLineInput, CLI_FWU_RESPONSE cli_FWU_RESPONSE, string installPath, bool isShowInfo = true, bool isForce = false)
        {
            try
            {
                _devMgr.ProgressUpdate_Notify -= _FWUpdatePlugin_ProgressUpdate;
                _devMgr.ProgressUpdate_Notify += _FWUpdatePlugin_ProgressUpdate;
                _devMgr.DownloadAndInstall_Result_Notify -= Download_Event;
                _devMgr.DownloadAndInstall_Result_Notify += Download_Event;
                FWUpdateInfoPackage fwUpdateInfoPackage = _devMgr.GetFWUpdateInfo(isShowInfo, isForce, false, null, false, true).Result;
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
                        Task.Delay(100).Wait();
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
                    FWResultReceived?.Invoke(this, (retFWUpdateInfos, JsonConvert.SerializeObject(cli_FWU_RESPONSE, Formatting.Indented)));
                    _devMgr.ProgressUpdate_Notify -= _FWUpdatePlugin_ProgressUpdate;
                    _devMgr.DownloadAndInstall_Result_Notify -= Download_Event;
                }));
                return ((int)CLI_ExitCode.success, JsonConvert.SerializeObject(cli_FWU_RESPONSE, Formatting.Indented));

            }
            catch
            {
                return ((int)CLI_ExitCode.fail_FWUpdate, JsonConvert.SerializeObject(cli_FWU_RESPONSE, Formatting.Indented));
            }
        }
        private void Download_Event(object o, List<FWUpdateInfo> e)
        {
            retFWUpdateInfos = e;
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
        
        // add @ stephen for fwupdate
        private (int code, string result) FWUpdateX(CommandLineInput commandLineInput, IDeviceManagerSA devMgr)
        {
            if (commandLineInput.Command.Equals("SET"))
            {
                if (commandLineInput.Options.Count > 1)
                {
                    CLI_RESPONSE G_FW_RESPONSE = new CLI_RESPONSE();
                    G_FW_RESPONSE.Result = "FAIL";
                    G_FW_RESPONSE.Message = "UNKNOWN COMMAND FWUpdateX";
                    System.Console.WriteLine(JsonConvert.SerializeObject(G_FW_RESPONSE, Formatting.Indented));
                    return ((int)CLI_ExitCode.unknow_command, JsonConvert.SerializeObject(G_FW_RESPONSE, Formatting.Indented));
                }

                List<DeviceType> deviceTypeList = new List<DeviceType>();
                //deviceTypeList.Add(DeviceType.LogicalKeyboard);
                foreach (var option in commandLineInput.Options)
                {
                    switch (option.Option_Value)
                    {
                        case Params.DeviceType.WEBCAM:
                            deviceTypeList.Add(DeviceType.LogicalWebcam);
                            deviceTypeList.Add(DeviceType.PhysicalWebcam);
                            break;

                        case Params.DeviceType.AUDIO:
                            deviceTypeList.Add(DeviceType.LogicalWiredAudio);
                            deviceTypeList.Add(DeviceType.PhysicalAudioDongle);
                            deviceTypeList.Add(DeviceType.PhysicalBluetoothAudio);
                            deviceTypeList.Add(DeviceType.PhysicalWiredAudio);
                            break;

                        case Params.DeviceType.KEYBOARD:
                            deviceTypeList.Add(DeviceType.LogicalKeyboard);
                            deviceTypeList.Add(DeviceType.PhysicalDongle);
                            break;

                        case Params.DeviceType.MOUSE:
                            deviceTypeList.Add(DeviceType.LogicalMouse);
                            deviceTypeList.Add(DeviceType.PhysicalDongle);
                            break;

                        case Params.DeviceType.PEN:
                            deviceTypeList.Add(DeviceType.LogicalPen);
                            deviceTypeList.Add(DeviceType.PhysicalPen);
                            break;

                        case Params.DeviceType.DOCK:
                            deviceTypeList.Add(DeviceType.LogicalDock);
                            deviceTypeList.Add(DeviceType.PhysicalWiredDock);
                            break;

                        default:
                            deviceTypeList.Clear();
                            break;
                    }
                }

                return FWUpdate(devMgr, commandLineInput.Command, commandLineInput.DeviceIndex, commandLineInput.ServiceTag, commandLineInput.Model, deviceTypeList).Result;


            }
            else
            {
                CLI_RESPONSE G_FW_RESPONSE = new CLI_RESPONSE();
                G_FW_RESPONSE.Result = "FAIL";
                G_FW_RESPONSE.Message = "UNKNOWN COMMAND";
                G_FW_RESPONSE.Command = commandLineInput.Command;
                G_FW_RESPONSE.TargetFeature = commandLineInput.TargetFeature;
                System.Console.WriteLine(JsonConvert.SerializeObject(G_FW_RESPONSE, Formatting.Indented));
                return ((int)CLI_ExitCode.unknow_command, JsonConvert.SerializeObject(G_FW_RESPONSE, Formatting.Indented));
            }
        }

        // add @ stephen
        private async Task<(int code, string result)> FWUpdate(IDeviceManagerSA devMgr, string type, List<string> index, List<string> serviceTag, List<string> model, List<DeviceType> deviceTypeList, string value = "")
        {

            CLI_RESPONSE S_FWUpdate_RESPONSE = new CLI_RESPONSE();

            string output = string.Empty;

            Console.WriteLine($"FWUpdate(IDeviceManagerSA devMgr...)");

            if (type == "SET")
            {

                FWUpdateInfoPackage fwUpdateInfoPackage;

                if (deviceTypeList.Count > 0)
                {
                    fwUpdateInfoPackage = devMgr.GetFWUpdateInfo(true, false, false, deviceTypeList, false, false).Result;
                }
                else
                {
                    fwUpdateInfoPackage = devMgr.GetFWUpdateInfo(true, false, false, null, false, true).Result;
                }


                foreach (FWUpdateInfo info in fwUpdateInfoPackage.FWUpdateInfo)
                {

                    Console.WriteLine($"info.DeviceName = " + info.DeviceName);
                    Console.WriteLine($"info.DevicePath = " + info.DevicePath);
                    Console.WriteLine($"info.DeviceVersion = " + info.DeviceVersion);
                    Console.WriteLine($"info.FileSavepath = " + info.FileSavepath);
                }

                List<FWUpdateInfo> result = devMgr.DownloadAndInstall(fwUpdateInfoPackage.FWUpdateInfo, false, "").Result;

                foreach (FWUpdateInfo info in result)
                {
                    Console.WriteLine($"result info.DeviceName = " + info.DeviceName);
                    Console.WriteLine($"result info.DevicePath = " + info.DevicePath);
                    Console.WriteLine($"result info.DeviceVersion = " + info.DeviceVersion);
                    Console.WriteLine($"result info.FileSavepath = " + info.FileSavepath);
                    Console.WriteLine($"result info.DeviceType = " + info.DeviceType);
                }

                return ((int)CLI_ExitCode.success, output);
            }
            else
            {
                S_FWUpdate_RESPONSE.Command = type;
                S_FWUpdate_RESPONSE.TargetFeature = "FIRMWAREUPDATE";
                S_FWUpdate_RESPONSE.Result = "Format Error";
                S_FWUpdate_RESPONSE.Message = "Format Error";
                return ((int)CLI_ExitCode.unknow_command, JsonConvert.SerializeObject(S_FWUpdate_RESPONSE, Formatting.Indented));
            }
        }
        */

        private (int code, string result) ExportSettingsx(IDeviceManagerSA devMgr, CommandLineInput commandLineInput)
        {
            if ((commandLineInput.Command == "SET" || commandLineInput.Command == "GET") && commandLineInput.Options.Count > 0)
            {
                return ExportSettings(devMgr, commandLineInput).Result;
            }
            else
            {
                CLI_RESPONSE cli_Response = new CLI_RESPONSE();
                cli_Response.Command = commandLineInput.Command;
                cli_Response.TargetFeature = commandLineInput.TargetFeature;
                cli_Response.Result = "FAIL";
                cli_Response.Message = "Invalid command line syntax, missing -value=... or more than one -value=...";
                return ((int)CLI_ExitCode.invalide_cmdline_syntax, cli_Response.ToJson());
            }
        }
        private async Task<(int code, string result)> ExportSettings(IDeviceManagerSA devMgr, CommandLineInput commandLineInput)
        {
            string output = string.Empty;
            string filepath = string.Empty;
            if (!string.IsNullOrEmpty(commandLineInput.Options[0].Option_Value))
            {
                Trace.WriteLine(commandLineInput.Options[0].Option_Value);
                filepath = commandLineInput.Options[0].Option_Value;
            }
            else
            {
                CLI_RESPONSE cli_Response = new CLI_RESPONSE();
                cli_Response.Command = commandLineInput.Command;
                cli_Response.TargetFeature = commandLineInput.TargetFeature;
                cli_Response.Result = "FAIL";
                cli_Response.Message = "Invalid command line syntax, missing -value=... or more than one -value=...";
                return ((int)CLI_ExitCode.invalide_cmdline_syntax, cli_Response.ToJson());
            }

            bool retcode = false;

            if (_AllInfoMonitors == null)
                _AllInfoMonitors = await devMgr.GetMonitors();

            if (commandLineInput.Command == "SET" && commandLineInput.TargetFeature == "IMPORTSETTINGS")
            {
                List<int> _monitorIndeies = new List<int>();

                if (_AllInfoMonitors == null)
                    _AllInfoMonitors = devMgr.GetMonitors().Result;
                _monitorIndeies = GetMonitorIndeies(commandLineInput, _AllInfoMonitors);

                foreach (int idx in _monitorIndeies)
                {
                    MonitorInfo monitor = _AllInfoMonitors[idx];
                    CLI_RESPONSE cli_Response = new CLI_RESPONSE(monitor);
                    cli_Response.Command = commandLineInput.Command;
                    cli_Response.TargetFeature = commandLineInput.TargetFeature;
                    writelog($"DisplayImportSettings set entry");
                    if (!File.Exists(filepath))
                    {
                        cli_Response.Result = "FAIL";
                        cli_Response.Message = "file is not exist.";
                    }
                    else
                    {
                        writelog("devMgr.DisplayImportSettings set entry");
                        retcode = (int)devMgr.DisplayImportSettings(monitor, false, filepath).Result > 0;
                        writelog("devMgr.DisplayImportSettings set exit");
                    }


                    cli_Response.Result = retcode == true ? "PASS" : "FAIL";
                    //cli_Response.Value = $"IMPORTSETTINGS";

                    System.Console.WriteLine(JsonConvert.SerializeObject(cli_Response, Formatting.Indented));
                    output += "\n" + JsonConvert.SerializeObject(cli_Response, Formatting.Indented);
                }
            }
            else if (commandLineInput.Command == "GET" && commandLineInput.TargetFeature == "EXPORTSETTINGS")
            {
                List<int> _monitorIndeies = new List<int>();
                if (_AllInfoMonitors == null)
                    _AllInfoMonitors = devMgr.GetMonitors().Result;
                _monitorIndeies = GetMonitorIndeies(commandLineInput, _AllInfoMonitors);

                foreach (int idx in _monitorIndeies)
                {
                    //LaunchNetworkkvmApp(); //Open DDM console for debug
                    writelog($"DisplayExportSettings get entry");
                    MonitorInfo monitor = _AllInfoMonitors[idx];
                    CLI_RESPONSE cli_Response = new CLI_RESPONSE(monitor);
                    cli_Response.Command = commandLineInput.Command;
                    cli_Response.TargetFeature = commandLineInput.TargetFeature;
                    writelog("devMgr.DisplayExportSettings get entry");
                    retcode = devMgr.DisplayExportSettings(monitor, filepath).Result;
                    writelog("devMgr.DisplayExportSettings get exit");

                    cli_Response.Result = retcode == true ? "PASS" : "FAIL";

                    System.Console.WriteLine(JsonConvert.SerializeObject(cli_Response, Formatting.Indented));
                    output += "\n" + JsonConvert.SerializeObject(cli_Response, Formatting.Indented);
                }
            }
            else
            {
                CLI_RESPONSE cli_Response = new CLI_RESPONSE();
                cli_Response.Command = commandLineInput.Command;
                cli_Response.TargetFeature = commandLineInput.TargetFeature;
                cli_Response.Result = "FAIL";
                cli_Response.Message = "Invalid command line syntax, missing -value=... or more than one -value=...";
                return ((int)CLI_ExitCode.invalide_cmdline_syntax, cli_Response.ToJson());
            }
            writelog($"Display Import Export Settings exit return value : {output}");
            return (retcode ? (int)CLI_ExitCode.success : (int)CLI_ExitCode.functional_error, output);

        }

        private async Task<int> SetPowerNapAsync(PowerNapType type, IDeviceManagerSA devMgr, string modelName, string serialNumber, string serviceTag)
        {
            if (devMgr == null)
                return 0;
            PowerNapSetting setting = new PowerNapSetting
            {
                Status = type == PowerNapType.Off ? false : true,//temp.Status,
                ModelName = modelName,
                SerialNumber = serialNumber,
                RunType = type,
                ServiceTag = serviceTag
            };
            writelog("devMgr.SavePowerNapSetting entry");
            await devMgr.SavePowerNapSetting(setting);
            writelog("devMgr.SavePowerNapSetting exit");
            UpdateUINotify powerNapNotify = new UpdateUINotify();
            powerNapNotify.UI_Field_Name = "POWERNAP;" + get_PowerNapType_code_ToUpper(type.ToString().ToUpper());
            writelog("devMgr.OnUIUpdateNotify entry");
            devMgr.OnUIUpdateNotify(powerNapNotify);
            writelog("devMgr.OnUIUpdateNotify exit");
            return 1;
        }
        private static string get_PowerNapType_code_ToUpper(string code)
        {
            switch (code.ToUpper())
            {
                case "OFF": return "OFF";
                case "REDUCEBRIGHTNESS": return "REDUCE";
                case "SLEEP": return "SLEEP";
                default: return "SLEEP";
            }
        }
    }
}