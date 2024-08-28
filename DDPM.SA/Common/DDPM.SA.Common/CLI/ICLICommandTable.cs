using Dell.Client.Framework.Common;
using Microsoft.VisualBasic.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ObjectiveC;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;

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
        //IT feature table
        private readonly List<string> Supported_IT_Feature = new List<string>()//ex: /get -app="telemetryconsent"
        {
            "TELEMETRYCONSENT", 
            "ENGERSAVER"
        };
        //IT value table of IT feature
        private readonly List<string> Supported_IT_Value_Keyword = new List<string>()//ex: /set -app=telemetryconsent -value=on,"lock"
        {
            "UNLOCK",
            "LOCK"
        };
        //---
        private readonly List<string> commands = new List<string>() 
        { 
            "GET", 
            "SET", 
            "CONFIGURE",
            "HELP"
        };
        //a part of input Type: target feature, ex: -Display=BrightnessLevel
        private readonly List<string> pluginType = new List<string>()
        { 
            "DISPLAY", 
            "COLOR", 
            "MOUSE", 
            "KEYBOARD", 
            "APP", 
            "DOCK",
            "HEADSET",
            "AUDIO"
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
            // 3.both IT and normal commands in one request (process cli at CLIManager and then bypass command to CLIProxy)
            // It's not possible that both isITCommands and isNormalCommands are false.
            public bool isITCommands { get; set; } = false;
            public bool isNormalCommands { get; set; } = false;

            public CommandLineInput()
            {
                Options = new List<CommandType_Option>(); //others optional input
                ServiceTag = new List<string>();
                DeviceIndex = new List<string>();
                GuidString = new List<string>();
                LogPath = Path.GetFullPath("CLI_Log\\" + DateTime.Now.ToString("yyyy - MM - dd - HH - mm - ss") + ".txt");
            }
        }
        public CommandLineInput StringProcessing(string[] args)
        {
            ICLICommandTable iCLICommandTable = new ICLICommandTable(_Log);

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
            CommandLineInput commandInput = new CommandLineInput();
            var command = args[0].Replace("/", "").Replace("-", "").ToUpper();

            //parse command code
            if (commands.Exists(v => v == command))
            {
                commandInput.Command = command;
            }

            if ((args.Length < 2))
            {
                if (commandInput.Command.Equals("HELP"))
                {
                    return commandInput;
                }
                _Log.Error("[CLI] command length is too small");
                return null;
            }

            var in_type = args[1].Trim().ToUpper();
            //parse target type and target feature
            string[] str = in_type.Split('=');
            if (str.Length < 2)
            {
                _Log.Error("[CLI] 2nd code should be the format like -Display=Feature");
                return null;
            }
            /*string[] str2 = str[1].Split(".");
            if (str.Length < 2)
            {
                _Log.Error("[CLI] 2nd code should be the format like -name=Display.Feature");
                return null;
            }*/
            commandInput.TargetType = str[0].Replace("-", "");
            commandInput.TargetFeature = str[1];
            commandInput.PluginsType = "";
            //parse target plugin
            foreach (string plugin in pluginType)
            {
                if (str[0].Replace("-", "").ToUpper().Trim().Equals(plugin))
                {
                    commandInput.PluginsType = plugin;
                    break;
                }
            }
            if (string.IsNullOrEmpty(commandInput.PluginsType))
            {
                _Log.Error("[CLI] no target be found");
                return null;
            }
            try
            {
                //for (int i = 0; i < args.Length; i++)
                for (int i = 2; i < args.Length; i++) //arg[0] should be command (ex: get, set, ...), arg[1] should be Type (ex: -name=...)
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
            CheckCommandRoutePath(commandInput);

            return commandInput;
        }

        private void CheckCommandRoutePath(CommandLineInput commandInput)
        {
            commandInput.isNormalCommands = false;
            commandInput.isITCommands = false;

            int feature_idx = Supported_IT_Feature.FindIndex(x => x.ToUpper().Trim().Equals(commandInput.TargetFeature.ToUpper().Trim()));
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
                            if(option.Option_Value.Length <= 0)
                            {
                                commandInput.isNormalCommands = true;//recognized as normal command -> CLIProxy
                                return;
                            }
                            option.Option_Value.Trim().Replace(".", ",");//maybe user type wrong sep symbol from , to be .
                            List<string> parse = option.Option_Value.Split(",").ToList();
                            foreach(string value in parse)
                            {
                                //currently only "LOCK" and "UNLOCK" be recognized as IT global settings
                                //other new global setting should be add to below
                                //must match length to avoid some error parsing like OSD"LOCK"
                                int value_keyword_idx = Supported_IT_Value_Keyword.FindIndex(x => x.ToUpper().Trim().Equals(value.ToUpper().Trim()));
                                _Log.Info($"[ICLICommandTable] index of [Supported_IT_Value_Keyword] table is [{value_keyword_idx}]");
                                if (value_keyword_idx >= 0)
                                //if (value.Trim().ToUpper().Equals("LOCK") || value.Trim().ToUpper().Equals("UNLOCK"))
                                {
                                    commandInput.isITCommands = true;//recognized has IT command -> CLIManager
                                }
                                else
                                {
                                    commandInput.isNormalCommands = true;//recognized has normal command -> CLIProxy
                                }

                                if (commandInput.isNormalCommands == true && commandInput.isITCommands == true)
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
            //    CLI_COMMAND_TYPE_CONFIG = 2,
            //    CLI_COMMAND_TYPE_GET_SET = 3,
            //    CLI_COMMAND_TYPE_MAX
            //}

            public static readonly List<Dictionary<string, object>> FeatureList = new List<Dictionary<string, object>>
            {
                // Display | Basic Device Feature
                // - GET 
                new Dictionary<string, object> {{ "Feature", "DISPLAY" }, { "TargetFeature", "FWVersion" },                 { "Value", "N/A" }, { "Type", 0 }},
                new Dictionary<string, object> {{ "Feature", "DISPLAY" }, { "TargetFeature", "ActiveHours" },               { "Value", "N/A" }, { "Type", 0 }},
                new Dictionary<string, object> {{ "Feature", "DISPLAY" }, { "TargetFeature", "AutoBrightness" },            { "Value", "N/A" }, { "Type", 0 }},

                // - SET
                new Dictionary<string, object> {{ "Feature", "DISPLAY" }, { "TargetFeature", "SwapVideo" },                 { "Value", "N/A" }, { "Type", 1 }},
                new Dictionary<string, object> {{ "Feature", "DISPLAY" }, { "TargetFeature", "SwapUSB" },                   { "Value", "N/A" }, { "Type", 1 }},
                new Dictionary<string, object> {{ "Feature", "DISPLAY" }, { "TargetFeature", "UniformityCompensation" },    { "Value", "N/A" }, { "Type", 1 }}, // TO DROP

                // - GET/SET
                new Dictionary<string, object> {{ "Feature", "DISPLAY" }, { "TargetFeature", "BrightnessLevel" },           { "Value", "N/A" }, { "Type", 3 }},
                new Dictionary<string, object> {{ "Feature", "DISPLAY" }, { "TargetFeature", "ContrastLevel" },             { "Value", "N/A" }, { "Type", 3 }},
                new Dictionary<string, object> {{ "Feature", "DISPLAY" }, { "TargetFeature", "ColorPreset" },               { "Value", "N/A" }, { "Type", 3 }},
                new Dictionary<string, object> {{ "Feature", "DISPLAY" }, { "TargetFeature", "ActiveInputSource" },         { "Value", "N/A" }, { "Type", 3 }},
                new Dictionary<string, object> {{ "Feature", "DISPLAY" }, { "TargetFeature", "PxP" },                       { "Value", "N/A" }, { "Type", 3 }},
                new Dictionary<string, object> {{ "Feature", "DISPLAY" }, { "TargetFeature", "SubInput" },                  { "Value", "N/A" }, { "Type", 3 }},
                new Dictionary<string, object> {{ "Feature", "DISPLAY" }, { "TargetFeature", "AudioProfile" },              { "Value", "N/A" }, { "Type", 3 }}, //TO DROP
                new Dictionary<string, object> {{ "Feature", "DISPLAY" }, { "TargetFeature", "PxPZoom" },                   { "Value", "N/A" }, { "Type", 3 }},
                new Dictionary<string, object> {{ "Feature", "DISPLAY" }, { "TargetFeature", "PowerSetting" },              { "Value", "N/A" }, { "Type", 3 }},
                new Dictionary<string, object> {{ "Feature", "DISPLAY" }, { "TargetFeature", "OSDLanguage" },               { "Value", "N/A" }, { "Type", 3 }},
                new Dictionary<string, object> {{ "Feature", "DISPLAY" }, { "TargetFeature", "OSDAccess" },                 { "Value", "N/A" }, { "Type", 3 }},
                new Dictionary<string, object> {{ "Feature", "DISPLAY" }, { "TargetFeature", "AutoBrightness" },            { "Value", "N/A" }, { "Type", 3 }},
                new Dictionary<string, object> {{ "Feature", "DISPLAY" }, { "TargetFeature", "AutoBrightnessRangeLevel" },  { "Value", "N/A" }, { "Type", 3 }},
                new Dictionary<string, object> {{ "Feature", "DISPLAY" }, { "TargetFeature", "AutoTemp" },                  { "Value", "N/A" }, { "Type", 3 }},
                new Dictionary<string, object> {{ "Feature", "DISPLAY" }, { "TargetFeature", "PrimaryMonitorSync" },        { "Value", "N/A" }, { "Type", 3 }},
                new Dictionary<string, object> {{ "Feature", "DISPLAY" }, { "TargetFeature", "USBCPrioritization" },        { "Value", "N/A" }, { "Type", 3 }},
                new Dictionary<string, object> {{ "Feature", "DISPLAY" }, { "TargetFeature", "SpeakerMicrophone" },         { "Value", "N/A" }, { "Type", 3 }},
                new Dictionary<string, object> {{ "Feature", "DISPLAY" }, { "TargetFeature", "SpeakerVolume" },             { "Value", "N/A" }, { "Type", 3 }},
                new Dictionary<string, object> {{ "Feature", "DISPLAY" }, { "TargetFeature", "Microphone" },                { "Value", "N/A" }, { "Type", 3 }},
                new Dictionary<string, object> {{ "Feature", "DISPLAY" }, { "TargetFeature", "EnergySaver" },               { "Value", "N/A" }, { "Type", 3 }},
                new Dictionary<string, object> {{ "Feature", "DISPLAY" }, { "TargetFeature", "ExportSettings" },            { "Value", "N/A" }, { "Type", 3 }}, // TO DROP
                new Dictionary<string, object> {{ "Feature", "DISPLAY" }, { "TargetFeature", "PowerNap" },                  { "Value", "N/A" }, { "Type", 3 }},
                new Dictionary<string, object> {{ "Feature", "DISPLAY" }, { "TargetFeature", "ColorManagement" },           { "Value", "N/A" }, { "Type", 3 }},
                new Dictionary<string, object> {{ "Feature", "DISPLAY" }, { "TargetFeature", "RestoreColorDefaults" },      { "Value", "N/A" }, { "Type", 3 }},

                // Display | Application or OS set
                // - GET
                new Dictionary<string, object> {{ "Feature", "DISPLAY" }, { "TargetFeature", "EasyArrangeLayout" },         { "Value", "N/A" }, { "Type", 0 }}, // TODO: EasyArrangeLayout also lands in Config
                new Dictionary<string, object> {{ "Feature", "DISPLAY" }, { "TargetFeature", "AllResolutionRefreshRate" },  { "Value", "N/A" }, { "Type", 0 }}, // TO DROP
                new Dictionary<string, object> {{ "Feature", "DISPLAY" }, { "TargetFeature", "CurrentResolutionRefreshRate" }, { "Value", "N/A" }, { "Type", 0 }},

                // - CONFIG
                new Dictionary<string, object> {{ "Feature", "DISPLAY" }, { "TargetFeature", "Resolution" },                { "Value", "N/A" }, { "Type", 2 }},
                new Dictionary<string, object> {{ "Feature", "DISPLAY" }, { "TargetFeature", "RefreshRate" },               { "Value", "N/A" }, { "Type", 2 }},
                new Dictionary<string, object> {{ "Feature", "DISPLAY" }, { "TargetFeature", "ResolutionRefreshRate" },     { "Value", "N/A" }, { "Type", 2 }},
                new Dictionary<string, object> {{ "Feature", "DISPLAY" }, { "TargetFeature", "EasyArrangeLayout" },         { "Value", "N/A" }, { "Type", 2 }},

                // Display | Advanced Feature
                // - GET
                new Dictionary<string, object> {{ "Feature", "ADVANCED" }, { "TargetFeature", "Display.Control" },          { "Value", "N/A" }, { "Type", 0 }}, // TO DROP
                new Dictionary<string, object> {{ "Feature", "DISPLAY" }, { "TargetFeature", "AdvancedControl" },           { "Value", "N/A" }, { "Type", 0 }}, // TO DROP
                new Dictionary<string, object> {{ "Feature", "DISPLAY" }, { "TargetFeature", "EDID" },                      { "Value", "N/A" }, { "Type", 0 }}, // TO CHECK
                new Dictionary<string, object> {{ "Feature", "DISPLAY" }, { "TargetFeature", "DecodedEDID" },               { "Value", "N/A" }, { "Type", 0 }}, // TO CHECK
                new Dictionary<string, object> {{ "Feature", "DISPLAY" }, { "TargetFeature", "CapabilitiesString" },        { "Value", "N/A" }, { "Type", 0 }}, // TO CHECK

                // - SET
                new Dictionary<string, object> {{ "Feature", "DISPLAY" }, { "TargetFeature", "AdvancedControl" },           { "Value", "N/A" }, { "Type", 1 }}, // TO DROP

                // - GET/SET
                new Dictionary<string, object> {{ "Feature", "DISPLAY" }, { "TargetFeature", "Orientation" },               { "Value", "N/A" }, { "Type", 3 }},
                new Dictionary<string, object> {{ "Feature", "DISPLAY" }, { "TargetFeature", "LockRotate" },                { "Value", "N/A" }, { "Type", 3 }},
                new Dictionary<string, object> {{ "Feature", "DISPLAY" }, { "TargetFeature", "RotateOSDMenu" },             { "Value", "N/A" }, { "Type", 3 }},

                // - CONFIG
                new Dictionary<string, object> {{ "Feature", "DISPLAY" }, { "TargetFeature", "MonitorPower" },              { "Value", "N/A" }, { "Type", 2 }},

                // === Application Level CLI ===
                new Dictionary<string, object> {{ "Feature", "APP" }, { "TargetFeature", "Update" },                        { "Value", "N/A" }, { "Type", 0 }},
                new Dictionary<string, object> {{ "Feature", "APP" }, { "TargetFeature", "Update" },                        { "Value", "N/A" }, { "Type", 2 }},
                new Dictionary<string, object> {{ "Feature", "APP" }, { "TargetFeature", "UpdateSourceLocation" },          { "Value", "N/A" }, { "Type", 0 }},
                new Dictionary<string, object> {{ "Feature", "APP" }, { "TargetFeature", "UpdateSourceLocation" },          { "Value", "N/A" }, { "Type", 2 }},
                new Dictionary<string, object> {{ "Feature", "DISPLAY" }, { "TargetFeature", "FirmwareUpdate" },            { "Value", "N/A" }, { "Type", 2 }}, // TO DROP
                new Dictionary<string, object> {{ "Feature", "APP" }, { "TargetFeature", "FirmwareUpdate" },                { "Value", "N/A" }, { "Type", 2 }},
                new Dictionary<string, object> {{ "Feature", "APP" }, { "TargetFeature", "UpdateAccess" },                  { "Value", "N/A" }, { "Type", 0 }},
                new Dictionary<string, object> {{ "Feature", "APP" }, { "TargetFeature", "UpdateAccess" },                  { "Value", "N/A" }, { "Type", 2 }},
                new Dictionary<string, object> {{ "Feature", "APP" }, { "TargetFeature", "TelemetryConsent" },              { "Value", "N/A" }, { "Type", 0 }},
                new Dictionary<string, object> {{ "Feature", "APP" }, { "TargetFeature", "TelemetryConsent" },              { "Value", "N/A" }, { "Type", 2 }},
                new Dictionary<string, object> {{ "Feature", "APP" }, { "TargetFeature", "ScreenNotification" },            { "Value", "N/A" }, { "Type", 0 }},
                new Dictionary<string, object> {{ "Feature", "APP" }, { "TargetFeature", "ScreenNotification" },            { "Value", "N/A" }, { "Type", 2 }},
                new Dictionary<string, object> {{ "Feature", "APP" }, { "TargetFeature", "DeviceConnected" },               { "Value", "N/A" }, { "Type", 0 }}, // TO DROP
                new Dictionary<string, object> {{ "Feature", "APP" }, { "TargetFeature", "ExportSettings" },                { "Value", "N/A" }, { "Type", 0 }},
                new Dictionary<string, object> {{ "Feature", "APP" }, { "TargetFeature", "ExportSettings" },                { "Value", "N/A" }, { "Type", 1 }},
                new Dictionary<string, object> {{ "Feature", "APP" }, { "TargetFeature", "RestoreFactoryDefaults" },        { "Value", "N/A" }, { "Type", 1 }}, 

                // === CLI apply to all devices ===
                new Dictionary<string, object> {{ "Feature", "APP" }, { "TargetFeature", "ConnectedDevices" },              { "Value", "N/A" }, { "Type", 0 }},
                new Dictionary<string, object> {{ "Feature", "APP" }, { "TargetFeature", "DeviceData" },                    { "Value", "N/A" }, { "Type", 0 }},
                new Dictionary<string, object> {{ "Feature", "APP" }, { "TargetFeature", "DeviceConfiguration" },           { "Value", "N/A" }, { "Type", 0 }},
                new Dictionary<string, object> {{ "Feature", "APP" }, { "TargetFeature", "DiagnosticsReport" },             { "Value", "N/A" }, { "Type", 0 }}, 
                
                // ==== Client Peripherals (CP) and Docks CLI ===
                // - WEBCAM
                new Dictionary<string, object> {{ "Feature", "WEBCAM" }, { "TargetFeature", "FWVersion" },                  { "Value", "N/A" }, { "Type", 0 }},
                new Dictionary<string, object> {{ "Feature", "WEBCAM" }, { "TargetFeature", "RestoreFactoryDefaults" },     { "Value", "N/A" }, { "Type", 1 }},
                new Dictionary<string, object> {{ "Feature", "WEBCAM" }, { "TargetFeature", "FieldOfView" },                { "Value", "N/A" }, { "Type", 0 }},
                new Dictionary<string, object> {{ "Feature", "WEBCAM" }, { "TargetFeature", "hdr" },                        { "Value", "N/A" }, { "Type", 0 }},
                new Dictionary<string, object> {{ "Feature", "WEBCAM" }, { "TargetFeature", "hdr" },                        { "Value", "N/A" }, { "Type", 1 }},
                new Dictionary<string, object> {{ "Feature", "WEBCAM" }, { "TargetFeature", "AntiFlicker" },                { "Value", "N/A" }, { "Type", 0 }},
                new Dictionary<string, object> {{ "Feature", "WEBCAM" }, { "TargetFeature", "AntiFlicker" },                { "Value", "N/A" }, { "Type", 1 }},
                new Dictionary<string, object> {{ "Feature", "WEBCAM" }, { "TargetFeature", "MicSwitch" },                  { "Value", "N/A" }, { "Type", 0 }},
                new Dictionary<string, object> {{ "Feature", "WEBCAM" }, { "TargetFeature", "MicSwitch" },                  { "Value", "N/A" }, { "Type", 1 }},
                new Dictionary<string, object> {{ "Feature", "WEBCAM" }, { "TargetFeature", "AIAutoFraming" },              { "Value", "N/A" }, { "Type", 0 }},
                new Dictionary<string, object> {{ "Feature", "WEBCAM" }, { "TargetFeature", "AIAutoFraming" },              { "Value", "N/A" }, { "Type", 1 }},
                new Dictionary<string, object> {{ "Feature", "WEBCAM" }, { "TargetFeature", "PresenceDetection" },          { "Value", "N/A" }, { "Type", 0 }},
                new Dictionary<string, object> {{ "Feature", "WEBCAM" }, { "TargetFeature", "PresenceDetection" },          { "Value", "N/A" }, { "Type", 1 }},

                // - HEADSET
                new Dictionary<string, object> {{ "Feature", "AUDIO" }, { "TargetFeature", "FWVersion" },                   { "Value", "N/A" }, { "Type", 0 }},
                new Dictionary<string, object> {{ "Feature", "AUDIO" }, { "TargetFeature", "RestoreFactoryDefaults" },      { "Value", "N/A" }, { "Type", 1 }},
                new Dictionary<string, object> {{ "Feature", "AUDIO" }, { "TargetFeature", "ancMode" },                     { "Value", "N/A" }, { "Type", 0 }},
                new Dictionary<string, object> {{ "Feature", "AUDIO" }, { "TargetFeature", "ancMode" },                     { "Value", "N/A" }, { "Type", 1 }},
                new Dictionary<string, object> {{ "Feature", "AUDIO" }, { "TargetFeature", "micNoiseCancellation" },        { "Value", "N/A" }, { "Type", 0 }},
                new Dictionary<string, object> {{ "Feature", "AUDIO" }, { "TargetFeature", "micNoiseCancellation" },        { "Value", "N/A" }, { "Type", 1 }},
                new Dictionary<string, object> {{ "Feature", "AUDIO" }, { "TargetFeature", "wearDetection" },               { "Value", "N/A" }, { "Type", 0 }},
                new Dictionary<string, object> {{ "Feature", "AUDIO" }, { "TargetFeature", "wearDetection" },               { "Value", "N/A" }, { "Type", 1 }},

                // - KEYBOARD
                new Dictionary<string, object> {{ "Feature", "KEYBOARD" }, { "TargetFeature", "FWVersion" },                { "Value", "N/A" }, { "Type", 0 }},
                new Dictionary<string, object> {{ "Feature", "KEYBOARD" }, { "TargetFeature", "RestoreFactoryDefaults" },   { "Value", "N/A" }, { "Type", 1 }},
                new Dictionary<string, object> {{ "Feature", "KEYBOARD" }, { "TargetFeature", "CollabCameraEnable" },       { "Value", "N/A" }, { "Type", 0 }},
                new Dictionary<string, object> {{ "Feature", "KEYBOARD" }, { "TargetFeature", "CollabMicMute" },            { "Value", "N/A" }, { "Type", 0 }},
                new Dictionary<string, object> {{ "Feature", "KEYBOARD" }, { "TargetFeature", "CallabScreenShare" },        { "Value", "N/A" }, { "Type", 0 }},
                new Dictionary<string, object> {{ "Feature", "KEYBOARD" }, { "TargetFeature", "CallabScreenShare" },        { "Value", "N/A" }, { "Type", 1 }},
                new Dictionary<string, object> {{ "Feature", "KEYBOARD" }, { "TargetFeature", "CollabChatEnable" },         { "Value", "N/A" }, { "Type", 0 }},

                // - MOUSE
                new Dictionary<string, object> {{ "Feature", "MOUSE" }, { "TargetFeature", "FWVersion" },                   { "Value", "N/A" }, { "Type", 0 }},
                new Dictionary<string, object> {{ "Feature", "MOUSE" }, { "TargetFeature", "RestoreFactoryDefaults" },      { "Value", "N/A" }, { "Type", 1 }},
                                                
                // - PEN
                new Dictionary<string, object> {{ "Feature", "PEN" }, { "TargetFeature", "FWVersion" },                     { "Value", "N/A" }, { "Type", 0 }},
                new Dictionary<string, object> {{ "Feature", "PEN" }, { "TargetFeature", "RestoreFactoryDefaults" },        { "Value", "N/A" }, { "Type", 1 }},

                // - DOCK
                new Dictionary<string, object> {{ "Feature", "DOCK" }, { "TargetFeature", "FWVersion" },                    { "Value", "N/A" }, { "Type", 0 }},
                new Dictionary<string, object> {{ "Feature", "DOCK" }, { "TargetFeature", "RestoreFactoryDefaults" },       { "Value", "N/A" }, { "Type", 1 }},
                new Dictionary<string, object> {{ "Feature", "DOCK" }, { "TargetFeature", "SilentFWUpdate" },               { "Value", "N/A" }, { "Type", 1 }},

            };

            public static void PrintFormattedJson()
            {
                var groupedFeatures = FeatureList
                    .GroupBy(f => f["Feature"].ToString())
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(f => f["TargetFeature"].ToString()).Distinct().ToList()
                    );

                string json = JsonConvert.SerializeObject(groupedFeatures, Formatting.Indented);
                Console.WriteLine(json);
            }

            public static void PrintFormattedJsonTargetFeature(string feature)
            {
                var targetFeatures = FeatureList
                    .Where(f => f["Feature"].ToString().Equals(feature, StringComparison.OrdinalIgnoreCase))
                    .Select(f => f["TargetFeature"].ToString())
                    .Distinct()
                    .ToList();
             
                var result = new Dictionary<string, List<string>>
        {
            { feature, targetFeatures }
        };
                string json = JsonConvert.SerializeObject(result, Formatting.Indented);
                Console.WriteLine(json);
            }
        }

        public static int Response_HelpCommand(CommandLineInput commandLineInput)
        {
            if (null == commandLineInput.TargetType) {
                // no argument for help function. dump all targetFeature 
                CLIHelpCommandStructure.PrintFormattedJson();
            } else if (commandLineInput.TargetType.Equals("VALUE"))
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
            } else {
                // delivered wrong argument for help function, return error  
                Response_FormatError();
                return (int)CLI_ExitCode.fail_FormantError;
            }
            return (int)CLI_ExitCode.success;

        }
    }
}
