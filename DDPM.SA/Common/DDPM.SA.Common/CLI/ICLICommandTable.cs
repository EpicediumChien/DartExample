using Dell.Client.Framework.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DDPM.SA.Common
{
    /*public class IT_Command_Global
    {
        public string command { get; set; } = string.Empty;
        public string type { get; set; } = string.Empty;
        public string feature { get; set; } = string.Empty;
        public string option_name { get; set; } = "None";
        public string option_value { get; set; } = "None";

        public IT_Command_Global(string cmd, string dev_type, string dev_feature, string op_name, string op_value)
        {
            command = cmd;
            type = dev_type;
            feature = dev_feature;
            option_name = op_name;
            option_value = op_value;
        }
    }*/

    //get -Display=BrightnessLevel
    //set -Display=BrightnessLevel -option=value
    public class ICLICommandTable
    {
        private readonly Dictionary<string, int> Command_Timeout_Value = new Dictionary<string, int>()
        {
            { "DEVICEDATA", 300 },
            { "DEVICECONFIGURATION", 300 },
            { "EXPORTSETTINGS", 300 },
            { "IMPORTSETTINGS", 300 },
            { "PXP", 300 }
        };

        //IT feature table
        private readonly List<string> Supported_IT_Feature = new List<string>()//ex: /get -app="telemetryconsent"
        {
            "TELEMETRYCONSENT",         //TelemetryConsent          DDPMW-1339
            "INAPPUPDATE",              //InAppUpdate               DDPMW-1329/1330
            "INAPPBRICONT",             //InAppBriCont              DDPMW-1342/1343
            "INAPPAUTOBRITEMP",         //InAppAutoBriTemp          DDPMW-1341
            "INAPPAUTOBRIGHTNESSCOLOR",//1004 InAppAutoBrightnessColor DDPMW1341, same as InAppAutoBriTemp
            "INAPPRESTOREDEFAULTS",     //InAppRestoreDefaults      DDPMW-1333
            "INAPPRESTORE",             //InAppRestore              Same as InAppRestoreDefaults
            "RESTOREFACTORYDEFAULTS",   //RestoreFactoryDefaults    DDPMW-2013/2014/2015/2111/2114
            "SCREENNOTIFICATION",       //ScreenNotification        DDPMW-1901
            "RESOLUTIONREFRESHRATE",    //ResolutionRefreshRate     DDPMW-1344
            "USBCPRIORITIZATION",       //USBCPrioritization        DDPMW-1345
            "ACTIVEINPUTSOURCE",        //ActiveInputSource         DDPMW-1346
            "INAPPUSBKVM",              //InAppUSBKVM               DDPMW-1347
            "INAPPNETWORKKVM",          //InAppNetworkKVM           DDPMW-1599
            "EASYARRANGELAYOUT",        //EasyArrangeLayout         DDPMW-1350
            "INAPPCOLORPRESET",         //InAppColorPreset          DDPMW-1351/1352
            "POWERNAP",                 //PowerNap                  DDPMW-1361
            "INAPPEXPORTIMPORT",        //INAPPEXPORTIMPORT       DDPMW-1335
            "COLLABSCREENSHARE",        //CollabScreenShare         DDPMW-1843
            "HDR",                      //hdr                       DDPMW-1729
            "ANTIFLICKER",              //AntiFlicker               DDPMW-1735
            "MICSWITCH",                //MicSwitch                 DDPMW-1736
            "AIAUTOFRAMING",            //AIAutoFraming             DDPMW-1742
            "PRESENCEDETECTION",        //PresenceDetection         DDPMW-1747
            "ANCMODE",                  //ancMode                   DDPMW-1853
            "MICNOISECANCELLATION",     //micNoiseCancellation      DDPMW-1855
            "WEARDETECTION"             //wearDetection             DDPMW-2093
        };

        //IT value table of IT feature
        private readonly List<string> Supported_IT_Value_Keyword = new List<string>()//ex: /set -app=telemetryconsent -value=on,"lock"
        {
            "UNLOCK",
            "LOCK",
            "ENABLE",
            "DISABLE"
        };

        //---
        private readonly List<string> commands = new List<string>()
        {
            "GET",
            "SET",
            "HELP"
        };

        //a part of input Type: target feature, ex: -Display=BrightnessLevel
        private readonly List<string> pluginType = new List<string>()
        {
            "DISPLAY",
            //"COLOR",
            "MOUSE",
            "KEYBOARD",
            "APP",
            "DOCK",
            //"HEADSET",
            "AUDIO",
            "VALUE",
            "PEN",
            "WEBCAM",
            "SPEAKERPHONE",
        };

        private ILog _Log;

        public ICLICommandTable(ILog Log)
        {
            _Log = Log;
        }

        public class CommandType_Option
        {
            /// <summary>
            /// 呼叫的方法
            /// </summary>
            public string Option_Name { get; set; }

            /// <summary>
            /// 設定的數值，如果不是設定(set)，為空值
            /// </summary>
            public string Option_Value { get; set; }

            public CommandType_Option(string Model, string Value = "")
            {
                this.Option_Name = Model;
                this.Option_Value = Value;
            }
        }

        public class CommandType_Name
        {
            public string target { get; set; }
            public string feature { get; set; }

            public CommandType_Name(string name, string Value)
            {
                this.target = name;
                this.feature = Value;
            }
        }

        public class CommandLineInput
        {
            //Used to judge target command support or not (please everyone refer to your own JIRA story)
            public bool isCliRunAdmin { get; set; } = false;

            /// <summary>
            /// 設定 get, set, config, or new others的功能型態
            /// </summary>
            public string Command { get; set; }

            public string TargetType { get; set; }//name, log, applyconfig; ex: -Display=BrightnessLevel
            public string TargetFeature { get; set; }

            /// <summary>
            /// 要呼叫的插件
            /// </summary>
            public string PluginsType { get; set; }

            /// <summary>
            /// 呼叫的方法
            /// </summary>
            public List<CommandType_Option> Options { get; set; }//use to store options to get/set device features

            public List<string> ServiceTag { get; set; }//for display with servicetag
            public List<string> DeviceIndex { get; set; }//for display with index
            public List<string> GuidString { get; set; }//for peripherals
            public string LogPath { get; set; }

            //Here are 3 possible conditions,
            // 1.only normal command (pass to CLIProxy)
            // 2.only IT command (process it at CLIManager)
            // 3.both IT and normal commands in one request (process cli at CLIManager and then pass command to CLIProxy)
            // It's not possible that both isITCommands and isNormalCommands are false.
            public bool isITCommands { get; set; } = false;

            public bool isNormalCommands { get; set; } = false;

            //2024-08-28 Casper: Add isCliCommandsProcessCompleted for CLIAgent to judge more situation
            // rather than null commandLineInput
            public bool isCliCommandsProcessCompleted { get; set; } = false;

            // 2024-10-22 Stephen: Add jsonDeviceConfig for CMA while using DeviceConfiguration and pass json string 
            public string jsonDeviceConfig { get; set; }    =   String.Empty;

            public CommandLineInput()
            {
                Options = new List<CommandType_Option>(); //others optional input
                ServiceTag = new List<string>();
                DeviceIndex = new List<string>();
                GuidString = new List<string>();
                LogPath = Path.GetFullPath("CLI_Log\\" + DateTime.Now.ToString("yyyy - MM - dd - HH - mm - ss") + ".txt");
            }

            public int nTimeOutValue { get; set; } = 60;
        }

        public CommandLineInput StringProcessing(string[] args)
        {
            ICLICommandTable iCLICommandTable = new ICLICommandTable(_Log);
            CommandLineInput commandInput = new CommandLineInput();
            commandInput.isCliCommandsProcessCompleted = false;

            // [0824_CASPER]: marked for HELP function parsing
            //if (args.Length < 2)
            //{
            //    _Log.Error("[CLI] command length is too small");
            //    return null;
            //}
            foreach (string arg in args)
            {
                if (string.IsNullOrWhiteSpace(arg))
                {
                    _Log.Error("[ICLICommandTable] Exception error: string is null or white space");
                    return null;
                }
                if (arg.Length > 260)
                {
                    _Log.Error("[ICLICommandTable] Exception error: string too long. Arg:" + arg.ToString());
                    return null;
                }
            }
            var command = args[0].Replace("/", "").Replace("-", "").ToUpper();

            //parse command code
            if (commands.Exists(v => v == command))
            {
                commandInput.Command = command;
            }
            else
            {
                _Log.Error("[CLI] input unknown command");
                return commandInput;
            }

            if ((args.Length < 2))
            {
                if (commandInput.Command.Equals("HELP"))
                {
                    commandInput.isCliCommandsProcessCompleted = true;
                    return commandInput;
                }
                _Log.Error("[CLI] command length is too small");
                return commandInput;
            }

            var in_type = args[1].Trim().ToUpper();
            //parse target type and target feature
            string[] str = in_type.Split('=');
            if (str.Length < 2)
            {
                _Log.Error("[CLI] 2nd code should be the format like -Display=targetFeature ");
                return commandInput;
            }

            commandInput.TargetType = str[0].Replace("-", "");
            commandInput.TargetFeature = str[1];
            // 08-24 Casper: fine tune the string parser process
            //   check if no input value for targetFeature .\CLI.Subagent.exe /get -Display=
            if (string.IsNullOrEmpty(commandInput.TargetFeature))
            {
                return commandInput;
            }
            commandInput.PluginsType = "";
            //parse target plugin
            foreach (string plugin in pluginType)
            {
                if (commandInput.TargetType.ToUpper().Trim().Equals(plugin))
                {
                    commandInput.PluginsType = plugin;
                    break;
                }
            }
            if (string.IsNullOrEmpty(commandInput.PluginsType))
            {
                _Log.Error("[CLI] no target be found");
                return commandInput;
            }
            try
            {
                for (int i = 2; i < args.Length; i++) //arg[0] should be command (ex: get, set, ...), arg[1] should be Type (ex: -display= or -keyboard=...)
                {
                    if (args[i] != "")
                    {
                        args[i] = args[i].Trim();
                        string tmp = args[i];
                        //0627 Bruce 新增先將所有帶入的參數將-濾掉
                        //args[i] = args[i].Replace("-", ""); //[Dean] 0716 remove this line to avoid abnormal GUID
                        if (args[i].IndexOf("/") == 0 || args[i].IndexOf("-") == 0)
                        {
                            args[i] = args[i].Substring(1);
                        }

                        if (args[i].ToUpper().IndexOf("SERVICETAG") == 0 || args[i].ToUpper().IndexOf("INDEX") == 0 || args[i].ToUpper().IndexOf("GUID") == 0)//判斷是那些裝置
                        {
                            string[] tmpSS = args[i].Split("=");
                            if (tmpSS.Length != 2)
                            {
                                _Log.Warning($"[CLI] ignore a part of commands => {tmp}");
                                continue;
                            }

                            if (tmpSS[1].Contains("]."))
                            {
                                tmpSS[1] = tmpSS[1].Replace("].", "],");
                            }
                            string[] tmpS = tmpSS[1].Split(",");
                            {
                                foreach (string tS in tmpS)
                                {
                                    string t = tS;
                                    if (tS.Length > 1 && tS.StartsWith("["))
                                        t = tS.Substring(1);
                                    else
                                        t = tS;
                                    if (t.Length > 1 && t.EndsWith("]"))
                                        t = t.Substring(0, t.Length - 1);
                                    if (tmpSS[0].ToUpper().Contains("SERVICETAG"))
                                    {
                                        commandInput.ServiceTag.Add(t);
                                    }
                                    else if (tmpSS[0].ToUpper().Contains("GUID"))
                                    {
                                        commandInput.GuidString.Add(t);
                                    }
                                    else if (tmpSS[0].ToUpper().Contains("INDEX"))
                                    {
                                        int temp = int.Parse(t) - 1;
                                        commandInput.DeviceIndex.Add(temp.ToString());
                                    }
                                    else
                                    {
                                        //_Log.Error("[ICLICommandTable] ");
                                    }
                                }
                            }
                        }
                        else if (args[i].ToUpper().Contains("LOGPATH"))
                        {
                            string[] tmpSS = args[i].Split("=");
                            if (tmpSS.Length != 2)
                            {
                                _Log.Warning($"[CLI] ignore a part of commands => {tmp}");
                                continue;
                            }
                            if (!tmpSS[1].Contains(".txt"))
                            {
                                tmpSS[1] += ".txt";
                            }
                            commandInput.LogPath = Path.GetFullPath(tmpSS[1]);
                        }
                        else
                        {
                            //判斷呼叫的方法
                            //0606 將commandInput.Options的來源改成大寫
                            //0716 [Dean] add code that just remove first "-" or "/" as well
                            if (args[i].IndexOf("/") == 0 || args[i].IndexOf("-") == 0)
                            {
                                args[i] = args[i].Substring(1);
                            }
                            string[] tmpSS = args[i].ToUpper().Split("=");
                            if (tmpSS.Length > 1)
                            {
                                commandInput.Options.Add(new CommandType_Option(tmpSS[0], tmpSS[1]));
                            }
                            else
                            {
                                commandInput.Options.Add(new CommandType_Option(tmpSS[0]));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _Log.Error("[ICLICommandTable] Exception error:" + ex.ToString());
            }

            //Dean 0816 check the command is belong to IT/normal or both
            CheckCommandRoutePath(ref commandInput);
            CheckTimeoutAndAssignValue(ref commandInput);
            commandInput.isCliCommandsProcessCompleted = true;

            return commandInput;
        }

        private void CheckTimeoutAndAssignValue(ref CommandLineInput commandInput)
        {
            CommandLineInput commandInput_temp = commandInput;
            if (Command_Timeout_Value.ContainsKey(commandInput_temp.TargetFeature.ToUpper()))
            {
                commandInput.nTimeOutValue = Command_Timeout_Value[commandInput_temp.TargetFeature.ToUpper()];
                return;
            }
        }

        private void CheckCommandRoutePath(ref CommandLineInput commandInput)
        {
            commandInput.isNormalCommands = false;
            commandInput.isITCommands = false;
            CommandLineInput commandInput_temp = commandInput;

            int feature_idx = Supported_IT_Feature.FindIndex(x => x.ToUpper().Trim().Equals(commandInput_temp.TargetFeature.ToUpper().Trim()));
            if (feature_idx >= 0) //has IT feature
            {
                if (commandInput.Options.Count == 0)//recognized only normal command -> CLIProxy
                {
                    commandInput.isNormalCommands = true;
                    return;
                }
                else
                {
                    //assume the option would be -value=on, -value=off,lock, -value=lock, -value=off,unlock
                    //in this situation the Option_Value would be on / off / on,lock / off,lock / on,unlock / off,unlock
                    for (int i = 0; i < commandInput.Options.Count; i++)
                    {
                        CommandType_Option option = commandInput.Options[i];
                        try
                        {
                            if (option.Option_Value.Length <= 0)
                            {
                                commandInput.isNormalCommands = true;//recognized as normal command -> CLIProxy
                                return;
                            }
                            option.Option_Value.Trim().Replace(".", ",");//maybe user type wrong sep symbol from , to be .
                            List<string> parse = option.Option_Value.Split(",").ToList();
                            foreach (string value in parse)
                            {
                                //currently only "LOCK" and "UNLOCK" be recognized as IT global settings
                                //other new global setting should be add to below
                                //must match length to avoid some error parsing like OSD"LOCK"
                                int value_keyword_idx = Supported_IT_Value_Keyword.FindIndex(x => x.ToUpper().Trim().Equals(value.ToUpper().Trim()));
                                _Log?.Info($"[ICLICommandTable] index of [Supported_IT_Value_Keyword] table is [{value_keyword_idx}]");
                                if (value_keyword_idx >= 0)
                                //if (value.Trim().ToUpper().Equals("LOCK") || value.Trim().ToUpper().Equals("UNLOCK"))
                                {
                                    commandInput.isITCommands = true;//recognized has IT command -> CLIManager
                                    if (value.Trim().ToUpper().Equals("ENABLE") || value.Trim().ToUpper().Equals("DISABLE"))
                                    {
                                        commandInput.isNormalCommands = true;
                                        //break;
                                    }
                                }
                                else
                                {
                                    commandInput.isNormalCommands = true;//recognized has normal command -> CLIProxy
                                }

                                if (commandInput.isNormalCommands == true && commandInput.isITCommands == true)
                                    //if (!(commandInput.TargetFeature.ToUpper().Equals("INAPPUSBKVM")))
                                        break;
                            }
                        }
                        catch (Exception ex)
                        {
                            _Log.Error("[CheckCommandRoutePath] exception: " + ex.Message);
                            //recognized as normal command
                            commandInput.isNormalCommands = true;
                            return;
                        }
                    }
                }
            }
            else
                commandInput.isNormalCommands = true;
        }

        public static int Response_FormatError()
        {
            CLI_RESPONSE result = new CLI_RESPONSE()
            {
                Model = "N/A",
                SerialNumber = "N/A",
                Command = "N/A",
                TargetFeature = "N/A",
                Result = "Format error",
                Index = "N/A",
                ServiceTag = "N/A",
                Value = "N/A",
                Message = "Command line format error"
            };
            System.Console.WriteLine(JsonConvert.SerializeObject(result, Formatting.Indented));
            return (int)CLI_ExitCode.fail_FormantError;
        }

        public static int Response_UnelevatedError(CommandLineInput commandLineInput)
        {
            CLI_RESPONSE result = new CLI_RESPONSE()
            {
                Model = "N/A",
                SerialNumber = "N/A",
                Command = commandLineInput.Command,
                TargetFeature = commandLineInput.TargetFeature,
                Result = "Process unelevated",
                Index = "N/A",
                ServiceTag = "N/A",
                Value = "N/A",
                Message = "DDPM CLI should be executed as elevated process"
            };
            System.Console.WriteLine(JsonConvert.SerializeObject(result, Formatting.Indented));
            return (int)CLI_ExitCode.fail_notAdmin;
        }

        public static int Response_TimeoutError(CommandLineInput commandLineInput)
        {
            CLI_RESPONSE result = new CLI_RESPONSE()
            {
                Model = "N/A",
                SerialNumber = "N/A",
                Command = commandLineInput.Command,
                TargetFeature = commandLineInput.TargetFeature,
                Result = "Timeout for obtaining DDPM SA",
                Index = "N/A",
                ServiceTag = "N/A",
                Value = "N/A",
                Message = "Timeout for obtaining DDPM SA (CLI Manager)"
            };
            System.Console.WriteLine(JsonConvert.SerializeObject(result, Formatting.Indented));
            return (int)CLI_ExitCode.target_subagent_timeout;
        }

        public static int Response_WrongIndex(CommandLineInput commandLineInput)
        {
            CLI_RESPONSE result = new CLI_RESPONSE()
            {
                Model = "N/A",
                SerialNumber = "N/A",
                Command = commandLineInput.Command,
                TargetFeature = commandLineInput.TargetFeature,
                Result = "Wrong ID value",
                Index = "N/A",
                ServiceTag = "N/A",
                Value = "N/A",
                Message = "Wrong ID value"
            };
            System.Console.WriteLine(JsonConvert.SerializeObject(result, Formatting.Indented));
            return (int)CLI_ExitCode.fail_Value;
        }

        public static CLIEventResult Response_EmptyCommandInput(string action_guid = null)
        {
            CLI_RESPONSE result = new CLI_RESPONSE()
            {
                Model = "N/A",
                SerialNumber = "N/A",
                Command = "N/A",
                TargetFeature = "N/A",
                Result = "Got empty input from CLI subagent",
                Index = "N/A",
                ServiceTag = "N/A",
                Value = "N/A",
                Message = "Got empty input from CLI subagent (CLI Manager)"
            };
            CLIEventResult arg = new CLIEventResult()
            {
                serialize_Json_response = JsonConvert.SerializeObject(result, Formatting.Indented),
                ExitCode = (int)CLI_ExitCode.empty_event_args,
                ticket = DateTime.Now,
                command_guid_string = action_guid == null ? string.Empty : action_guid
            };
            return arg;
        }

        public static string Response_NoResultTimeout(CommandLineInput commandLineInput)
        {
            CLI_RESPONSE result = new CLI_RESPONSE()
            {
                Model = "N/A",
                SerialNumber = "N/A",
                Command = commandLineInput.Command,
                TargetFeature = commandLineInput.TargetFeature,
                Result = "Timeout for obtaining result",
                Index = "N/A",
                ServiceTag = "N/A",
                Value = "N/A",
                Message = "Timeout for obtaining result (CLI Manager)"
            };
            return JsonConvert.SerializeObject(result, Formatting.Indented);
        }

        public static CLIEventResult Response_PluginNotReady(CommandLineInput commandLineInput, string pluginName, string action_guid)
        {
            CLI_RESPONSE result = new CLI_RESPONSE()
            {
                Model = "N/A",
                SerialNumber = "N/A",
                Command = commandLineInput.Command,
                TargetFeature = commandLineInput.TargetFeature,
                Result = $"Required plugin {pluginName} not ready",
                Index = "N/A",
                ServiceTag = "N/A",
                Value = "N/A",
                Message = $"Required plugin {pluginName} not ready"
            };
            CLIEventResult arg = new CLIEventResult()
            {
                command_guid_string = action_guid,
                serialize_Json_response = JsonConvert.SerializeObject(result, Formatting.Indented),
                ExitCode = (int)CLI_ExitCode.required_plugin_not_ready,
                ticket = DateTime.Now
            };
            return arg;
        }

        public static string Response_TargetTypeNotSupport(CommandLineInput commandLineInput, string target_type)
        {
            CLI_RESPONSE result = new CLI_RESPONSE()
            {
                Model = "N/A",
                SerialNumber = "N/A",
                Command = commandLineInput.Command,
                TargetFeature = commandLineInput.TargetFeature,
                Result = $"Target type {target_type} not support",
                Index = "N/A",
                ServiceTag = "N/A",
                Value = "N/A",
                Message = $"Target type {target_type} not support"
            };
            return JsonConvert.SerializeObject(result, Formatting.Indented);
        }

        //
        // get/set - display/peripheral - recommandString
        //
        public static CLIEventResult Response_TargetFeatureNotSupport(CommandLineInput commandLineInput, string action_guid)
        {
            CLI_RESPONSE result = new CLI_RESPONSE()
            {
                Model = "N/A",
                SerialNumber = "N/A",
                Command = commandLineInput.Command,
                TargetFeature = commandLineInput.TargetFeature,
                Result = $"Type {commandLineInput.TargetType} not support feature {commandLineInput.TargetFeature}",
                Index = "N/A",
                ServiceTag = "N/A",
                Value = "N/A",
                Message = $"Type {commandLineInput.TargetType} not support feature {commandLineInput.TargetFeature}"
            };
            CLIEventResult arg = new CLIEventResult()
            {
                serialize_Json_response = JsonConvert.SerializeObject(result, Formatting.Indented),
                ExitCode = (int)CLI_ExitCode.command_targetfeature_not_support,
                ticket = DateTime.Now,
                command_guid_string = action_guid
            };
            return arg;
        }

        public static string change_0base_to_1base(string value)
        {
            return (int.Parse(value) + 1).ToString();
        }


        public class CLIHelpCommandStructure
        {
            //private enum CLI_COMMAND_TYPE {
            //    CLI_COMMAND_TYPE_GET = 0,
            //    CLI_COMMAND_TYPE_SET = 1,
            //    CLI_COMMAND_TYPE_MAX
            //}
            // [HELP]: Store all the command set
            public static readonly List<Dictionary<string, object>> FeatureList = new List<Dictionary<string, object>>
            {
                // Display | Application settings 
                new Dictionary<string, object> {{ "TargetType", "DISPLAY" }, { "TargetFeature", "AutoColorPreset" },            { "Value", "N/A" },         { "Type", 0 }},
                new Dictionary<string, object> {{ "TargetType", "DISPLAY" }, { "TargetFeature", "AutoColorPreset" },            { "Value", "N/A" },         { "Type", 1 }},
                new Dictionary<string, object> {{ "TargetType", "DISPLAY" }, { "TargetFeature", "NetworkKVMVersion" },          { "Value", "N/A" },         { "Type", 0 }},
                new Dictionary<string, object> {{ "TargetType", "DISPLAY" }, { "TargetFeature", "ChangeMonitorId" },            { "Value", "[id]" },        { "Type", 1 }},  // ChangeMonitorId[id]
                new Dictionary<string, object> {{ "TargetType", "DISPLAY" }, { "TargetFeature", "NetworkKVM" },                 { "Value", "[on/off]" },    { "Type", 1 }},  // NetworkKVM[on/off]
                new Dictionary<string, object> {{ "TargetType", "DISPLAY" }, { "TargetFeature", "InAppNetworkKVM" },            { "Value", "N/A" },         { "Type", 1 }},  // InAppNetworkKVM[on/off]
                new Dictionary<string, object> {{ "TargetType", "DISPLAY" }, { "TargetFeature", "NetworkKVM" },                 { "Value", "N/A" },         { "Type", 0 }},  // NetworkKVM
                new Dictionary<string, object> {{ "TargetType", "DISPLAY" }, { "TargetFeature", "NetworkKVMAutoConnect" },      { "Value", "[on/off]" },    { "Type", 1 }},  // NetworkKVMAutoConnect [on/off]
                new Dictionary<string, object> {{ "TargetType", "DISPLAY" }, { "TargetFeature", "NetworkKVMAutoConnect" },      { "Value", "N/A" },         { "Type", 0 }},
                new Dictionary<string, object> {{ "TargetType", "DISPLAY" }, { "TargetFeature", "NetworkKVMContentTransfer" },  { "Value", "[on/off]" },    { "Type", 1 }}, // NetworkKVMContentTransfer [on/off]
                new Dictionary<string, object> {{ "TargetType", "DISPLAY" }, { "TargetFeature", "NetworkKVMContentTransfer" },  { "Value", "N/A" },         { "Type", 0 }},
                new Dictionary<string, object> {{ "TargetType", "DISPLAY" }, { "TargetFeature", "NetworkKVMIncomingPort" },     { "Value", "[1024-49151]" }, { "Type", 1 }}, // NetworkKVMIncomingPort[1024-49151]
                new Dictionary<string, object> {{ "TargetType", "DISPLAY" }, { "TargetFeature", "GetNetworkKVMIncomingPort" },  { "Value", "N/A" },         { "Type", 0 }},
                new Dictionary<string, object> {{ "TargetType", "DISPLAY" }, { "TargetFeature", "NetworkKVMOutgoingPort" },     { "Value", "[1024-49151]" }, { "Type", 1 }}, // NetworkKVMOutgoingPort[1024-49151]
                new Dictionary<string, object> {{ "TargetType", "DISPLAY" }, { "TargetFeature", "GetNetworkKVMOutgoingPort" }, { "Value", "N/A" },          { "Type", 0 }},
                new Dictionary<string, object> {{ "TargetType", "DISPLAY" }, { "TargetFeature", "NetworkKVMContentTransferPort " },{ "Value", "[1024-49151]" }, { "Type", 1 }}, // NetworkKVMContentTransferPort [1024 - 49151]
                new Dictionary<string, object> {{ "TargetType", "DISPLAY" }, { "TargetFeature", "GetNetworkKVMContentTransferPort" },{ "Value", "N/A" },    { "Type", 1 }},
                new Dictionary<string, object> {{ "TargetType", "DISPLAY" }, { "TargetFeature", "NetworkKVMAccessReset" },     { "Value", "N/A" },          { "Type", 0 }},

                // Display | General Asset Management
                //new Dictionary<string, object> {{ "TargetType", "DISPLAY" }, { "TargetFeature", "MonitorCount" },               { "Value", "N/A" }, { "Type", 0 }}, // Under discussion to drop
                new Dictionary<string, object> {{ "TargetType", "DISPLAY" }, { "TargetFeature", "ColorPreset" },                { "Value", "N/A" }, { "Type", 1 }}, // /set -Display=ColorPreset -Index=1 or /set -Display=ColorPreset -ServiceTag=abcdef

                 // Display | Basic Device Feature
                new Dictionary<string, object> {{ "TargetType", "DISPLAY" }, { "TargetFeature", "FWVersion" },                  { "Value", "N/A" }, { "Type", 0 }},
                new Dictionary<string, object> {{ "TargetType", "DISPLAY" }, { "TargetFeature", "BrightnessLevel" },            { "Value", "N/A" }, { "Type", 0 }},
                new Dictionary<string, object> {{ "TargetType", "DISPLAY" }, { "TargetFeature", "BrightnessLevel" },            { "Value", "[Level]" }, { "Type", 1 }},
                new Dictionary<string, object> {{ "TargetType", "DISPLAY" }, { "TargetFeature", "ContrastLevel" },              { "Value", "N/A" }, { "Type", 0 }},
                new Dictionary<string, object> {{ "TargetType", "DISPLAY" }, { "TargetFeature", "ContrastLevel" },              { "Value", "[Level]" }, { "Type", 1 }},
                new Dictionary<string, object> {{ "TargetType", "DISPLAY" }, { "TargetFeature", "ColorPreset" },                { "Value", "N/A" }, { "Type", 0 }},
                new Dictionary<string, object> {{ "TargetType", "DISPLAY" }, { "TargetFeature", "ColorPreset" },                { "Value", "[BT.709 or movie or etc.]" }, { "Type", 1 }},
                new Dictionary<string, object> {{ "TargetType", "DISPLAY" }, { "TargetFeature", "ActiveInputSource" },          { "Value", "N/A" }, { "Type", 0 }},
                new Dictionary<string, object> {{ "TargetType", "DISPLAY" }, { "TargetFeature", "ActiveInputSource" },          { "Value", "[DP1, HDMI...]" }, { "Type", 1 }},
                new Dictionary<string, object> {{ "TargetType", "DISPLAY" }, { "TargetFeature", "SubInput" },                   { "Value", "N/A" }, { "Type", 0 }},
                new Dictionary<string, object> {{ "TargetType", "DISPLAY" }, { "TargetFeature", "SubInput" },                   { "Value", "[TBT, USB-C, HDMI1, HDMI2, DP1, DP2, ...]" }, { "Type", 1 }},
                new Dictionary<string, object> {{ "TargetType", "DISPLAY" }, { "TargetFeature", "SwapVideo" },                  { "Value", "[source, target]" }, { "Type", 1 }},
                new Dictionary<string, object> {{ "TargetType", "DISPLAY" }, { "TargetFeature", "SwapUSB" },                    { "Value", "[target]" }, { "Type", 1 }},
                new Dictionary<string, object> {{ "TargetType", "DISPLAY" }, { "TargetFeature", "PxP" },                        { "Value", "N/A" }, { "Type", 0 }},
                new Dictionary<string, object> {{ "TargetType", "DISPLAY" }, { "TargetFeature", "PxP" },                        { "Value", "N/A" }, { "Type", 1 }},
                new Dictionary<string, object> {{ "TargetType", "DISPLAY" }, { "TargetFeature", "PxPZoom" },                    { "Value", "N/A" }, { "Type", 0 }},
                new Dictionary<string, object> {{ "TargetType", "DISPLAY" }, { "TargetFeature", "PxPZoom" },                    { "Value", "N/A" }, { "Type", 1 }},
                new Dictionary<string, object> {{ "TargetType", "DISPLAY" }, { "TargetFeature", "RestoreFactoryDefaults" },     { "Value", "[Defer, ForceWithNotice, -ForceWithNoNotice]" }, { "Type", 1 }},
                new Dictionary<string, object> {{ "TargetType", "DISPLAY" }, { "TargetFeature", "RestoreLevelDefaults" },       { "Value", "[Defer, ForceWithNotice, -ForceWithNoNotice]" }, { "Type", 1 }},
                new Dictionary<string, object> {{ "TargetType", "DISPLAY" }, { "TargetFeature", "RestoreColorDefaults" },       { "Value", "[Defer, ForceWithNotice, -ForceWithNoNotice]" }, { "Type", 1 }},
                new Dictionary<string, object> {{ "TargetType", "DISPLAY" }, { "TargetFeature", "PowerSetting" },               { "Value", "N/A" }, { "Type", 0 }},
                new Dictionary<string, object> {{ "TargetType", "DISPLAY" }, { "TargetFeature", "PowerSetting" },               { "Value", "[on, standby, off]" }, { "Type", 1 }},
                new Dictionary<string, object> {{ "TargetType", "DISPLAY" }, { "TargetFeature", "OSDLanguage" },                { "Value", "N/A" }, { "Type", 0 }},
                new Dictionary<string, object> {{ "TargetType", "DISPLAY" }, { "TargetFeature", "OSDLanguage" },                { "Value", "[English, German, French, Japanese, BrazilianPortuguese, Russian, Spanish, Chinese-Simplified]" }, { "Type", 1 }},
                new Dictionary<string, object> {{ "TargetType", "DISPLAY" }, { "TargetFeature", "OSDAccess" },                  { "Value", "N/A" }, { "Type", 0 }},
                new Dictionary<string, object> {{ "TargetType", "DISPLAY" }, { "TargetFeature", "OSDAccess" },                  { "Value", "[OSDLock, OSDUnLock]" }, { "Type", 1 }},
                new Dictionary<string, object> {{ "TargetType", "DISPLAY" }, { "TargetFeature", "AutoBrightness" },             { "Value", "N/A" }, { "Type", 0 }},
                new Dictionary<string, object> {{ "TargetType", "DISPLAY" }, { "TargetFeature", "AutoBrightness" },             { "Value", "[on, off]" }, { "Type", 1 }},
                new Dictionary<string, object> {{ "TargetType", "DISPLAY" }, { "TargetFeature", "AutoBrightnessRangeLevel" },   { "Value", "N/A" }, { "Type", 0 }},
                new Dictionary<string, object> {{ "TargetType", "DISPLAY" }, { "TargetFeature", "AutoBrightnessRangeLevel" },   { "Value", "[Low, Mid, High]" }, { "Type", 1 }},
                new Dictionary<string, object> {{ "TargetType", "DISPLAY" }, { "TargetFeature", "AutoColorTemp" },                   { "Value", "N/A" }, { "Type", 0 }},
                new Dictionary<string, object> {{ "TargetType", "DISPLAY" }, { "TargetFeature", "AutoColorTemp" },                   { "Value", "[on, off]" }, { "Type", 1 }},
                new Dictionary<string, object> {{ "TargetType", "DISPLAY" }, { "TargetFeature", "PrimaryMonitorSync" },         { "Value", "N/A" }, { "Type", 0 }},
                new Dictionary<string, object> {{ "TargetType", "DISPLAY" }, { "TargetFeature", "PrimaryMonitorSync" },         { "Value", "[on, off]" }, { "Type", 1 }},
                new Dictionary<string, object> {{ "TargetType", "DISPLAY" }, { "TargetFeature", "USBCPrioritization" },         { "Value", "N/A" }, { "Type", 0 }},
                new Dictionary<string, object> {{ "TargetType", "DISPLAY" }, { "TargetFeature", "USBCPrioritization" },         { "Value", "[HighSpeed, HighResolution]" }, { "Type", 1 }},
                new Dictionary<string, object> {{ "TargetType", "DISPLAY" }, { "TargetFeature", "SpeakerMicrophone" },          { "Value", "N/A" }, { "Type", 0 }},
                new Dictionary<string, object> {{ "TargetType", "DISPLAY" }, { "TargetFeature", "SpeakerMicrophone" },          { "Value", "N/A" }, { "Type", 1 }},
                new Dictionary<string, object> {{ "TargetType", "DISPLAY" }, { "TargetFeature", "SpeakerVolume" },              { "Value", "N/A" }, { "Type", 0 }},
                new Dictionary<string, object> {{ "TargetType", "DISPLAY" }, { "TargetFeature", "SpeakerVolume" },              { "Value", "[OSDenable, OSDDisable]" }, { "Type", 1 }},
                new Dictionary<string, object> {{ "TargetType", "DISPLAY" }, { "TargetFeature", "Microphone" },                 { "Value", "N/A" }, { "Type", 0 }},
                new Dictionary<string, object> {{ "TargetType", "DISPLAY" }, { "TargetFeature", "Microphone" },                 { "Value", "N/A" }, { "Type", 1 }},
                // new Dictionary<string, object> {{ "TargetType", "DISPLAY" }, { "TargetFeature", "AudioProfile" },             { "Value", "N/A" }, { "Type", 1 }}, //TO DROP
                // new Dictionary<string, object> {{ "TargetType", "DISPLAY" }, { "TargetFeature", "AudioProfile" },             { "Value", "N/A" }, { "Type", 0 }}, //TO DROP
                //new Dictionary<string, object> {{ "TargetType", "DISPLAY" }, { "TargetFeature", "EnergySaver" },                { "Value", "N/A" }, { "Type", 0 }},
                //new Dictionary<string, object> {{ "TargetType", "DISPLAY" }, { "TargetFeature", "EnergySaver" },                { "Value", "[On, Off, Lock, Unlock]" }, { "Type", 1 }},
                 new Dictionary<string, object> {{ "TargetType", "DISPLAY" }, { "TargetFeature", "ExportSettings" },           { "Value", "N/A" }, { "Type", 0 }},
                 new Dictionary<string, object> {{ "TargetType", "DISPLAY" }, { "TargetFeature", "ExportSettings" },           { "Value", "N/A" }, { "Type", 1 }},
                 new Dictionary<string, object> {{ "TargetType", "DISPLAY" }, { "TargetFeature", "ImportSettings" },           { "Value", "N/A" }, { "Type", 0 }},
                 new Dictionary<string, object> {{ "TargetType", "DISPLAY" }, { "TargetFeature", "ImportSettings" },           { "Value", "N/A" }, { "Type", 1 }},
                new Dictionary<string, object> {{ "TargetType", "DISPLAY" }, { "TargetFeature", "PowerNap" },                   { "Value", "N/A" }, { "Type", 0 }},
                new Dictionary<string, object> {{ "TargetType", "DISPLAY" }, { "TargetFeature", "PowerNap" },                   { "Value", "[off, sleep, ReduceBrightness]" }, { "Type", 1 }},
                new Dictionary<string, object> {{ "TargetType", "DISPLAY" }, { "TargetFeature", "ColorManagement" },            { "Value", "N/A" }, { "Type", 0 }},
                new Dictionary<string, object> {{ "TargetType", "DISPLAY" }, { "TargetFeature", "ColorManagement" },            { "Value", "[Off, bymonitor, byhost]" }, { "Type", 1 }},
                new Dictionary<string, object> {{ "TargetType", "DISPLAY" }, { "TargetFeature", "ActiveHours" },                { "Value", "N/A" }, { "Type", 0 }},

                //Display | Application or OS settings
                new Dictionary<string, object> {{ "TargetType", "DISPLAY" }, { "TargetFeature", "AllResolutionRefreshRate" },  { "Value", "N/A" }, { "Type", 0 }}, 
                 new Dictionary<string, object> {{ "TargetType", "DISPLAY" }, { "TargetFeature", "CurrentResolutionRefreshRate" }, { "Value", "N/A" }, { "Type", 0 }},
                new Dictionary<string, object> {{ "TargetType", "DISPLAY" }, { "TargetFeature", "Resolution" },                { "Value", "N/A" }, { "Type", 1 }},
                new Dictionary<string, object> {{ "TargetType", "DISPLAY" }, { "TargetFeature", "RefreshRate" },               { "Value", "N/A" }, { "Type", 1 }},
                new Dictionary<string, object> {{ "TargetType", "DISPLAY" }, { "TargetFeature", "ResolutionRefreshRate" },     { "Value", "N/A" }, { "Type", 1 }},
                new Dictionary<string, object> {{ "TargetType", "DISPLAY" }, { "TargetFeature", "EasyArrangeLayout" },         { "Value", "N/A" }, { "Type", 0 }},
                new Dictionary<string, object> {{ "TargetType", "DISPLAY" }, { "TargetFeature", "EasyArrangeLayout" },         { "Value", "N/A" }, { "Type", 1 }},


                // Design for advanced user, not publish to public
                //new Dictionary<string, object> {{ "TargetType", "ADVANCED" }, { "TargetFeature", "Display.Control" },          { "Value", "N/A" }, { "Type", 0 }}, // TO DROP
                //new Dictionary<string, object> {{ "TargetType", "DISPLAY" }, { "TargetFeature", "AdvancedControl" },           { "Value", "N/A" }, { "Type", 0 }}, // TO DROP
                //new Dictionary<string, object> {{ "TargetType", "DISPLAY" }, { "TargetFeature", "EDID" },                      { "Value", "N/A" }, { "Type", 0 }}, // TO CHECK
                //new Dictionary<string, object> {{ "TargetType", "DISPLAY" }, { "TargetFeature", "DecodedEDID" },               { "Value", "N/A" }, { "Type", 0 }}, // TO CHECK
                //new Dictionary<string, object> {{ "TargetType", "DISPLAY" }, { "TargetFeature", "CapabilitiesString" },        { "Value", "N/A" }, { "Type", 0 }}, // TO CHECK
                //new Dictionary<string, object> {{ "TargetType", "DISPLAY" }, { "TargetFeature", "AdvancedControl" },           { "Value", "N/A" }, { "Type", 1 }}, // TO DROP

                new Dictionary<string, object> {{ "TargetType", "DISPLAY" }, { "TargetFeature", "Orientation" },               { "Value", "N/A" }, { "Type", 0 }},
                new Dictionary<string, object> {{ "TargetType", "DISPLAY" }, { "TargetFeature", "Orientation" },               { "Value", "N/A" }, { "Type", 1 }},

                // new Dictionary<string, object> {{ "TargetType", "DISPLAY" }, { "TargetFeature", "LockRotate" },                { "Value", "N/A" }, { "Type", 0 }},
                // new Dictionary<string, object> {{ "TargetType", "DISPLAY" }, { "TargetFeature", "LockRotate" },                { "Value", "N/A" }, { "Type", 1 }},
                // new Dictionary<string, object> {{ "TargetType", "DISPLAY" }, { "TargetFeature", "RotateOSDMenu" },             { "Value", "N/A" }, { "Type", 0 }},
                // new Dictionary<string, object> {{ "TargetType", "DISPLAY" }, { "TargetFeature", "RotateOSDMenu" },             { "Value", "N/A" }, { "Type", 1 }},
                // new Dictionary<string, object> {{ "TargetType", "DISPLAY" }, { "TargetFeature", "MonitorPower" },              { "Value", "N/A" }, { "Type", 2 }},

                // === Application Level CLI ===
                new Dictionary<string, object> {{ "TargetType", "APP" }, { "TargetFeature", "Update" },                        { "Value", "N/A" }, { "Type", 0 }},
                new Dictionary<string, object> {{ "TargetType", "APP" }, { "TargetFeature", "Update" },                        { "Value", "N/A" }, { "Type", 1 }},
                new Dictionary<string, object> {{ "TargetType", "APP" }, { "TargetFeature", "UpdateSourceLocation" },          { "Value", "N/A" }, { "Type", 0 }},
                new Dictionary<string, object> {{ "TargetType", "APP" }, { "TargetFeature", "UpdateSourceLocation" },          { "Value", "N/A" }, { "Type", 1 }},
                //new Dictionary<string, object> {{ "TargetType", "DISPLAY" }, { "TargetFeature", "FirmwareUpdate" },            { "Value", "N/A" }, { "Type", 2 }}, // TO DROP
                new Dictionary<string, object> {{ "TargetType", "APP" }, { "TargetFeature", "FirmwareUpdate" },                { "Value", "N/A" }, { "Type", 1 }},
                //new Dictionary<string, object> {{ "TargetType", "APP" }, { "TargetFeature", "UpdateAccess" },                  { "Value", "N/A" }, { "Type", 0 }},
                //new Dictionary<string, object> {{ "TargetType", "APP" }, { "TargetFeature", "UpdateAccess" },                  { "Value", "N/A" }, { "Type", 1 }},
                new Dictionary<string, object> {{ "TargetType", "APP" }, { "TargetFeature", "TelemetryConsent" },              { "Value", "N/A" }, { "Type", 0 }},
                new Dictionary<string, object> {{ "TargetType", "APP" }, { "TargetFeature", "TelemetryConsent" },              { "Value", "N/A" }, { "Type", 1 }},
                new Dictionary<string, object> {{ "TargetType", "APP" }, { "TargetFeature", "ScreenNotification" },            { "Value", "N/A" }, { "Type", 0 }},
                new Dictionary<string, object> {{ "TargetType", "APP" }, { "TargetFeature", "ScreenNotification" },            { "Value", "N/A" }, { "Type", 1 }},
                //new Dictionary<string, object> {{ "TargetType", "APP" }, { "TargetFeature", "DeviceConnected" },               { "Value", "N/A" }, { "Type", 0 }}, // TO DROP
                new Dictionary<string, object> {{ "TargetType", "APP" }, { "TargetFeature", "ExportSettings" },                { "Value", "N/A" }, { "Type", 0 }},
                new Dictionary<string, object> {{ "TargetType", "APP" }, { "TargetFeature", "ExportSettings" },                { "Value", "N/A" }, { "Type", 1 }},
                new Dictionary<string, object> {{ "TargetType", "APP" }, { "TargetFeature", "RestoreFactoryDefaults" },        { "Value", "N/A" }, { "Type", 1 }},

                // === CLI apply to all devices ===
                new Dictionary<string, object> {{ "TargetType", "APP" }, { "TargetFeature", "ConnectedDevices" },              { "Value", "N/A" }, { "Type", 0 }},
                new Dictionary<string, object> {{ "TargetType", "APP" }, { "TargetFeature", "DeviceData" },                    { "Value", "N/A" }, { "Type", 0 }},
                new Dictionary<string, object> {{ "TargetType", "APP" }, { "TargetFeature", "DeviceConfiguration" },           { "Value", "N/A" }, { "Type", 0 }},
                new Dictionary<string, object> {{ "TargetType", "APP" }, { "TargetFeature", "DiagnosticsReport" },             { "Value", "N/A" }, { "Type", 0 }},

                // ==== Client Peripherals (CP) and Docks CLI ===
                // - WEBCAM
                new Dictionary<string, object> {{ "TargetType", "WEBCAM" }, { "TargetFeature", "FWVersion" },                  { "Value", "N/A" }, { "Type", 0 }},
                new Dictionary<string, object> {{ "TargetType", "WEBCAM" }, { "TargetFeature", "RestoreFactoryDefaults" },     { "Value", "N/A" }, { "Type", 1 }},
                new Dictionary<string, object> {{ "TargetType", "WEBCAM" }, { "TargetFeature", "FieldOfView" },                { "Value", "N/A" }, { "Type", 0 }},
                new Dictionary<string, object> {{ "TargetType", "WEBCAM" }, { "TargetFeature", "hdr" },                        { "Value", "N/A" }, { "Type", 0 }},
                new Dictionary<string, object> {{ "TargetType", "WEBCAM" }, { "TargetFeature", "hdr" },                        { "Value", "N/A" }, { "Type", 1 }},
                new Dictionary<string, object> {{ "TargetType", "WEBCAM" }, { "TargetFeature", "AntiFlicker" },                { "Value", "N/A" }, { "Type", 0 }},
                new Dictionary<string, object> {{ "TargetType", "WEBCAM" }, { "TargetFeature", "AntiFlicker" },                { "Value", "N/A" }, { "Type", 1 }},
                new Dictionary<string, object> {{ "TargetType", "WEBCAM" }, { "TargetFeature", "MicSwitch" },                  { "Value", "N/A" }, { "Type", 0 }},
                new Dictionary<string, object> {{ "TargetType", "WEBCAM" }, { "TargetFeature", "MicSwitch" },                  { "Value", "N/A" }, { "Type", 1 }},
                new Dictionary<string, object> {{ "TargetType", "WEBCAM" }, { "TargetFeature", "AIAutoFraming" },              { "Value", "N/A" }, { "Type", 0 }},
                new Dictionary<string, object> {{ "TargetType", "WEBCAM" }, { "TargetFeature", "AIAutoFraming" },              { "Value", "N/A" }, { "Type", 1 }},
                new Dictionary<string, object> {{ "TargetType", "WEBCAM" }, { "TargetFeature", "PresenceDetection" },          { "Value", "N/A" }, { "Type", 0 }},
                new Dictionary<string, object> {{ "TargetType", "WEBCAM" }, { "TargetFeature", "PresenceDetection" },          { "Value", "N/A" }, { "Type", 1 }},

                // - HEADSET
                new Dictionary<string, object> {{ "TargetType", "AUDIO" }, { "TargetFeature", "FWVersion" },                   { "Value", "N/A" }, { "Type", 0 }},
                new Dictionary<string, object> {{ "TargetType", "AUDIO" }, { "TargetFeature", "RestoreFactoryDefaults" },      { "Value", "N/A" }, { "Type", 1 }},
                new Dictionary<string, object> {{ "TargetType", "AUDIO" }, { "TargetFeature", "ancMode" },                     { "Value", "N/A" }, { "Type", 0 }},
                new Dictionary<string, object> {{ "TargetType", "AUDIO" }, { "TargetFeature", "ancMode" },                     { "Value", "N/A" }, { "Type", 1 }},
                new Dictionary<string, object> {{ "TargetType", "AUDIO" }, { "TargetFeature", "micNoiseCancellation" },        { "Value", "N/A" }, { "Type", 0 }},
                new Dictionary<string, object> {{ "TargetType", "AUDIO" }, { "TargetFeature", "micNoiseCancellation" },        { "Value", "N/A" }, { "Type", 1 }},
                new Dictionary<string, object> {{ "TargetType", "AUDIO" }, { "TargetFeature", "wearDetection" },               { "Value", "N/A" }, { "Type", 0 }},
                new Dictionary<string, object> {{ "TargetType", "AUDIO" }, { "TargetFeature", "wearDetection" },               { "Value", "N/A" }, { "Type", 1 }},

                // - KEYBOARD
                new Dictionary<string, object> {{ "TargetType", "KEYBOARD" }, { "TargetFeature", "FWVersion" },                { "Value", "N/A" }, { "Type", 0 }},
                new Dictionary<string, object> {{ "TargetType", "KEYBOARD" }, { "TargetFeature", "RestoreFactoryDefaults" },   { "Value", "N/A" }, { "Type", 1 }},
                new Dictionary<string, object> {{ "TargetType", "KEYBOARD" }, { "TargetFeature", "CollabCameraEnable" },       { "Value", "N/A" }, { "Type", 0 }},
                new Dictionary<string, object> {{ "TargetType", "KEYBOARD" }, { "TargetFeature", "CollabMicMute" },            { "Value", "N/A" }, { "Type", 0 }},
                new Dictionary<string, object> {{ "TargetType", "KEYBOARD" }, { "TargetFeature", "CallabScreenShare" },        { "Value", "N/A" }, { "Type", 0 }},
                new Dictionary<string, object> {{ "TargetType", "KEYBOARD" }, { "TargetFeature", "CallabScreenShare" },        { "Value", "N/A" }, { "Type", 1 }},
                //new Dictionary<string, object> {{ "TargetType", "KEYBOARD" }, { "TargetFeature", "CollabChatEnable" },         { "Value", "N/A" }, { "Type", 0 }},

                // - MOUSE
                new Dictionary<string, object> {{ "TargetType", "MOUSE" }, { "TargetFeature", "FWVersion" },                   { "Value", "N/A" }, { "Type", 0 }},
                new Dictionary<string, object> {{ "TargetType", "MOUSE" }, { "TargetFeature", "RestoreFactoryDefaults" },      { "Value", "N/A" }, { "Type", 1 }},

                // - PEN
                new Dictionary<string, object> {{ "TargetType", "PEN" }, { "TargetFeature", "FWVersion" },                     { "Value", "N/A" }, { "Type", 0 }},
                new Dictionary<string, object> {{ "TargetType", "PEN" }, { "TargetFeature", "RestoreFactoryDefaults" },        { "Value", "N/A" }, { "Type", 1 }},

                // - DOCK
                new Dictionary<string, object> {{ "TargetType", "DOCK" }, { "TargetFeature", "FWVersion" },                    { "Value", "N/A" }, { "Type", 0 }},
                new Dictionary<string, object> {{ "TargetType", "DOCK" }, { "TargetFeature", "SilentFWUpdate" },               { "Value", "N/A" }, { "Type", 1 }},

                // - IT_FEATURE
                new Dictionary<string, object> {{ "TargetType", "APP" },        { "TargetFeature", "TelemetryConsent" },            { "Value", "N/A" }, { "Type", 2 }},
                new Dictionary<string, object> {{ "TargetType", "APP" },        { "TargetFeature", "InAppUpdate" },                 { "Value", "N/A" }, { "Type", 2 }},
                new Dictionary<string, object> {{ "TargetType", "APP" },        { "TargetFeature", "InAppRestoreDefaults" },        { "Value", "N/A" }, { "Type", 2 }},
                new Dictionary<string, object> {{ "TargetType", "APP" },        { "TargetFeature", "ScreenNotification" },          { "Value", "N/A" }, { "Type", 2 }},
                new Dictionary<string, object> {{ "TargetType", "APP" },        { "TargetFeature", "InAppExportImport" },           { "Value", "N/A" }, { "Type", 2 }},

                new Dictionary<string, object> {{ "TargetType", "DISPLAY" },    { "TargetFeature", "InAppBriCont" },                { "Value", "N/A" }, { "Type", 2 }},
                new Dictionary<string, object> {{ "TargetType", "DISPLAY" },    { "TargetFeature", "InAppAutoBriTemp" },            { "Value", "N/A" }, { "Type", 2 }},
                //new Dictionary<string, object> {{ "TargetType", "DISPLAY" },    { "TargetFeature", "InAppAutoBrightnessColor" },    { "Value", "N/A" }, { "Type", 2 }},
                new Dictionary<string, object> {{ "TargetType", "DISPLAY" },    { "TargetFeature", "RestoreFactoryDefaults" },      { "Value", "N/A" }, { "Type", 2 }},
                new Dictionary<string, object> {{ "TargetType", "DISPLAY" },    { "TargetFeature", "ResolutionRefreshRate" },       { "Value", "N/A" }, { "Type", 2 }},
                new Dictionary<string, object> {{ "TargetType", "DISPLAY" },    { "TargetFeature", "USBCPrioritization" },          { "Value", "N/A" }, { "Type", 2 }},
                new Dictionary<string, object> {{ "TargetType", "DISPLAY" },    { "TargetFeature", "ActiveInputSource" },           { "Value", "N/A" }, { "Type", 2 }},
                new Dictionary<string, object> {{ "TargetType", "DISPLAY" },    { "TargetFeature", "InAppUSBKVM" },                 { "Value", "N/A" }, { "Type", 2 }},
                new Dictionary<string, object> {{ "TargetType", "DISPLAY" },    { "TargetFeature", "InAppNetworkKVM" },             { "Value", "N/A" }, { "Type", 2 }},
                new Dictionary<string, object> {{ "TargetType", "DISPLAY" },    { "TargetFeature", "EasyArrangeLayout" },           { "Value", "N/A" }, { "Type", 2 }},
                new Dictionary<string, object> {{ "TargetType", "DISPLAY" },    { "TargetFeature", "InAppColorPreset" },            { "Value", "N/A" }, { "Type", 2 }},
                new Dictionary<string, object> {{ "TargetType", "DISPLAY" },    { "TargetFeature", "PowerNap" },                    { "Value", "N/A" }, { "Type", 2 }},

                new Dictionary<string, object> {{ "TargetType", "KEYBOARD" },   { "TargetFeature", "RestoreFactoryDefaults" },      { "Value", "N/A" }, { "Type", 2 }},
                new Dictionary<string, object> {{ "TargetType", "KEYBOARD" },   { "TargetFeature", "CollabScreenShare" },           { "Value", "N/A" }, { "Type", 2 }},

                new Dictionary<string, object> {{ "TargetType", "MOUSE" },      { "TargetFeature", "RestoreFactoryDefaults" },      { "Value", "N/A" }, { "Type", 2 }},

                new Dictionary<string, object> {{ "TargetType", "PEN" },        { "TargetFeature", "RestoreFactoryDefaults" },      { "Value", "N/A" }, { "Type", 2 }},

                new Dictionary<string, object> {{ "TargetType", "WEBCAM" },     { "TargetFeature", "hdr" },                         { "Value", "N/A" }, { "Type", 2 }},
                new Dictionary<string, object> {{ "TargetType", "WEBCAM" },     { "TargetFeature", "AntiFlicker" },                 { "Value", "N/A" }, { "Type", 2 }},
                new Dictionary<string, object> {{ "TargetType", "WEBCAM" },     { "TargetFeature", "MicSwitch" },                   { "Value", "N/A" }, { "Type", 2 }},
                new Dictionary<string, object> {{ "TargetType", "WEBCAM" },     { "TargetFeature", "AIAutoFraming" },               { "Value", "N/A" }, { "Type", 2 }},
                new Dictionary<string, object> {{ "TargetType", "WEBCAM" },     { "TargetFeature", "PresenceDetection" },           { "Value", "N/A" }, { "Type", 2 }},

                new Dictionary<string, object> {{ "TargetType", "AUDIO" },      { "TargetFeature", "ancMode" },                     { "Value", "N/A" }, { "Type", 2 }},
                new Dictionary<string, object> {{ "TargetType", "AUDIO" },      { "TargetFeature", "micNoiseCancellation" },        { "Value", "N/A" }, { "Type", 2 }},
                new Dictionary<string, object> {{ "TargetType", "AUDIO" },      { "TargetFeature", "wearDetection" },               { "Value", "N/A" }, { "Type", 2 }},
                new Dictionary<string, object> {{ "TargetType", "AUDIO" },      { "TargetFeature", "RestoreFactoryDefaults" },      { "Value", "N/A" }, { "Type", 2 }},

            };

            // [HELP]: Print all the data in the command set
            public static void PrintFormattedJson()
            {
                var groupedFeatures = FeatureList
                    .GroupBy(f => f["TargetType"].ToString())
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(f => f["TargetFeature"].ToString()).Distinct().ToList()
                    );

                string json = JsonConvert.SerializeObject(groupedFeatures, Formatting.Indented);
                Console.WriteLine(json);
            }

            // [HELP]: Print the command according to the feature in list<pluginType> aka. targetFeature
            public static void PrintFormattedJsonTargetFeature(string targetType)
            {
                var targetFeatures = FeatureList
                    .Where(f => f["TargetType"].ToString().Equals(targetType, StringComparison.OrdinalIgnoreCase))
                    .Select(f => f["TargetFeature"].ToString())
                    .Distinct()
                    .ToList();

                var result = new Dictionary<string, List<string>>
                {
                    { targetType, targetFeatures }
                };
                string json = JsonConvert.SerializeObject(result, Formatting.Indented);
                Console.WriteLine(json);
            }

            public static bool IsTargetFeatureAndPluginsTypeExists(CommandLineInput commandLineInput)
            {
                return FeatureList.Any(f =>
                    f["TargetType"].ToString().Equals(commandLineInput.TargetFeature, StringComparison.OrdinalIgnoreCase) &&
                    f["TargetFeature"].ToString().Equals(commandLineInput.PluginsType, StringComparison.OrdinalIgnoreCase));
            }

            public static bool IsTargetTypeExists(CommandLineInput commandLineInput)
            {
                return FeatureList.Any(f =>
                    f["TargetType"].ToString().Equals(commandLineInput.TargetType, StringComparison.OrdinalIgnoreCase));
            }
        }

        public static int Response_HelpCommand(CommandLineInput commandLineInput)
        {
            if (null == commandLineInput.TargetType)
            {
                // no argument for help function. dump all targetFeature
                CLIHelpCommandStructure.PrintFormattedJson();
            }
            else if (commandLineInput.TargetType.Equals("VALUE"))
            {
                switch (commandLineInput.TargetFeature)
                {
                    case "ADVANCED":
                    case "APP":
                    case "AUDIO":
                    case "DOCK":
                    case "DISPLAY":
                    case "MOUSE":
                    case "KEYBOARD":
                    case "PEN":
                    case "WEBCAM":
                        CLIHelpCommandStructure.PrintFormattedJsonTargetFeature(commandLineInput.TargetFeature);
                        break;

                    default:
                        Response_FormatError();
                        return (int)CLI_ExitCode.fail_FormantError;
                }
            }
            else
            {
                // delivered wrong argument for help function, return error
                Response_FormatError();
                return (int)CLI_ExitCode.fail_FormantError;
            }
            return (int)CLI_ExitCode.success;
        }

        public static int Response_FormatErrorRecommendation(CommandLineInput commandLineInput)
        {
            if (true == CLIHelpCommandStructure.IsTargetTypeExists(commandLineInput))
            {
                CLIHelpCommandStructure.PrintFormattedJsonTargetFeature(commandLineInput.TargetType);
            }

            // Nothing I can help
            CLI_RESPONSE result = new CLI_RESPONSE()
            {
                Model = "N/A",
                SerialNumber = "N/A",
                Command = "N/A",
                TargetFeature = "N/A",
                Result = "Format error",
                Index = "N/A",
                ServiceTag = "N/A",
                Value = "N/A",
                Message = "Command line format error"
            };

            System.Console.WriteLine(JsonConvert.SerializeObject(result, Formatting.Indented));
            return (int)CLI_ExitCode.fail_FormantError;
        }
    }
}