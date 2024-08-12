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
    //Client fromat->/commands -Type -option1=value1 -option2=value2 -option3=value3 ... 
    //get -name=Display.BrightnessLevel 
    //set -name=Display.BrightnessLevel [Level]
    public class ICLICommandTable
    {
        readonly List<string> commands = new List<string>() { "GET", "SET", "CONFIGURE" };
        readonly List<string> types = new List<string>() { "NAME", "APPLYCONFIG" };
        readonly List<string> pluginType = new List<string>() { "DISPLAY", "COLOR", "MOUSE", "KEYBOARD", "APP", "DOCK", "HEADSET" };//a part of input Type, ex: -name=Display.Brightness
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

            public string TargetType { get; set; }//name, log, applyconfig; ex: -name=Display.Brighness, -log=Display.Clear
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
            if (args.Length < 2)
            {
                _Log.Error("[CLI] command length is too small");
                return null;
            }
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

            //parse command code
            var command = args[0].Replace("/", "").Replace("-", "").ToUpper();
            if (commands.Exists(v => v == command))
            {
                commandInput.Command = command;
            }
            var in_type = args[1].Trim().ToUpper();
            //parse target type and target feature
            string[] str = in_type.Split('=');
            if (str.Length < 2)
            {
                _Log.Error("[CLI] 2nd code should be the format like -name=Display.Feature");
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

            return commandInput;
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
    }
}
