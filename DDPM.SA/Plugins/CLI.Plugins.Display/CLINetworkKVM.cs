using DDPM.SA.Common;
using Newtonsoft.Json;
using System.Diagnostics;
using System;
using System.IO;
using static DDPM.SA.Common.ICLICommandTable;
using System.Collections.Generic;

namespace CLI.Plugins.Display;

internal class CLINetworkKVM
{
    private static CommandLineInput _commandLineInput;

    private static (int code, string result) NotSupportResponse()
    {
        var response = new NKVM_RESPONSE
        {
            Command = _commandLineInput.Command,
            TargetFeature = _commandLineInput.TargetFeature,
            Result = "FAIL",
            Message = "Un-supported command or value"
        };
        return ((int)CLI_ExitCode.unknow_command, response.ToJson());
    }

    public static (int code, string result) Execute(CommandLineInput commandLineInput)
    {
        _commandLineInput = commandLineInput;
        var validOptions = new List<string>();

        switch (_commandLineInput.TargetFeature)
        {
            case "NETWORKKVMVERSION":
                return NetworkKVMVersion();
            case "NETWORKKVM":
                validOptions = ["ON", "OFF", "ENABLE", "DISABLE"];
                return EntryNetworkKVM(_commandLineInput.TargetFeature, validOptions);
            case "NETWORKKVMAUTOCONNECT":
            case "NETWORKKVMCONTENTTRANSFER":
                validOptions = ["ON", "OFF", "DISABLE"];
                return EntryNetworkKVM(_commandLineInput.TargetFeature, validOptions);
            case "NETWORKKVMINCOMINGPORT":
            case "NETWORKKVMOUTGOINGPORT":
            case "NETWORKKVMCONTENTTRANSFERPORT":
                return EntryNetworkKVMPort(_commandLineInput.TargetFeature);
            case "NETWORKKVMACCESSRESET":
                return NetworkKVMAccessReset(_commandLineInput.TargetFeature);
            default:
                return NotSupportResponse();
        }
    }

    private static (int exitCode, string value, string message) RunDDMCommand(string command)
    {
        string filePath = @"C:\Program Files\Dell\Dell Display and Peripheral Manager\Plugins\NKVM\DDM.exe";
        int exitCode = -1;
        string value = "N/A";
        string message = "N/A";

        if (!File.Exists(filePath))
        {
            value = "Not Supported";
            message = "NetworkKVM is not support";
        }
        else
        {
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = filePath,
                Arguments = command.ToLower(),
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            using (Process process = new Process { StartInfo = startInfo })
            {
                process.Start();
                process.WaitForExit();
                exitCode = process.ExitCode;
            }

            Trace.WriteLine($"{DateTime.Now} {command} (Exit code: {exitCode})");
        }

        return (exitCode, value, message);
    }

    private static (int code, string result) NetworkKVMVersion()
    {
        if (_commandLineInput.Command != "GET" || _commandLineInput.Options.Count > 0)
        {
            return NotSupportResponse();
        }

        string output = string.Empty;
        bool retcode = false;

        var response = new NKVM_RESPONSE
        {
            Command = _commandLineInput.Command,
            TargetFeature = _commandLineInput.TargetFeature
        };

        string filePath = @"C:\Program Files\Dell\Dell Display and Peripheral Manager\Plugins\NKVM\DDM.exe";

        if (File.Exists(filePath))
        {
            FileVersionInfo fileVersionInfo = FileVersionInfo.GetVersionInfo(filePath);
            string version = fileVersionInfo.FileVersion;

            if (!string.IsNullOrWhiteSpace(version))
            {
                response.Value = version;
                response.Result = "PASS";
                retcode = true;
            }
            else
            {
                response.Result = "FAIL";
                retcode = false;
            }
        }
        else
        {
            response.Result = "FAIL";
            response.Message = "NetworkKVM is not support";
            response.Value = "Not Supported";
        }

        Console.WriteLine(JsonConvert.SerializeObject(response, Formatting.Indented));
        output = JsonConvert.SerializeObject(response, Formatting.Indented);
        return (retcode ? (int)CLI_ExitCode.success : (int)CLI_ExitCode.functional_error, output);
    }

    private static (int code, string result) EntryNetworkKVM(string command, List<string> validOptions)
    {
        string output = string.Empty;
        bool retcode = false;

        var response = new NKVM_RESPONSE
        {
            Command = _commandLineInput.Command,
            TargetFeature = _commandLineInput.TargetFeature
        };

        if (_commandLineInput.Command == "SET" && _commandLineInput.Options.Count > 0 && !string.IsNullOrWhiteSpace(_commandLineInput.Options[0].Option_Value))
        {
            response.Value = _commandLineInput.Options[0].Option_Value;

            if (!validOptions.Contains(_commandLineInput.Options[0].Option_Value.ToUpper()))
            {
                retcode = false;
                response.Result = "FAIL";
                response.Message = "Invalid option value";
            }
            else
            {
                command = $"/{command} {_commandLineInput.Options[0].Option_Value}";
                var commandResult = RunDDMCommand(command);

                if (commandResult.exitCode == 0)
                {
                    retcode = true;
                    response.Result = "PASS";
                }
                else
                {
                    retcode = false;
                    response.Result = "FAIL";
                    response.Message = commandResult.message;
                    response.Value = commandResult.value;
                }
            }
        }
        else if (_commandLineInput.Command == "GET" && _commandLineInput.Options.Count == 0)
        {
            command = $"/get {command}";
            var commandResult = RunDDMCommand(command);

            switch (commandResult.exitCode)
            {
                case 0:
                    retcode = true;
                    response.Result = "PASS";
                    response.Value = "OFF";
                    break;

                case 1:
                    retcode = true;
                    response.Result = "PASS";
                    response.Value = "ON";
                    break;

                case 2:
                    retcode = true;
                    response.Result = "PASS";
                    response.Value = "DISABLE";
                    break;

                default:
                    retcode = false;
                    response.Result = "FAIL";
                    response.Message = commandResult.message;
                    response.Value = commandResult.value;
                    break;
            }
        }
        else
        {
            return NotSupportResponse();
        }

        Console.WriteLine(JsonConvert.SerializeObject(response, Formatting.Indented));
        output += "\n" + JsonConvert.SerializeObject(response, Formatting.Indented);
        return (retcode ? (int)CLI_ExitCode.success : (int)CLI_ExitCode.functional_error, output);
    }

    private static (int code, string result) EntryNetworkKVMPort(string command)
    {
        string output = string.Empty;
        bool retcode = false;

        var response = new NKVM_RESPONSE
        {
            Command = _commandLineInput.Command,
            TargetFeature = _commandLineInput.TargetFeature
        };

        if (_commandLineInput.Command == "SET" && _commandLineInput.Options.Count > 0 && !string.IsNullOrWhiteSpace(_commandLineInput.Options[0].Option_Value))
        {
            response.Value = _commandLineInput.Options[0].Option_Value;

            if (int.TryParse(_commandLineInput.Options[0].Option_Value, out int port) && port >= 1024 && port <= 49151)
            {
                command = $"/{command} {_commandLineInput.Options[0].Option_Value}";
                var commandResult = RunDDMCommand(command);

                if (commandResult.exitCode == 0)
                {
                    retcode = true;
                    response.Result = "PASS";
                }
                else
                {
                    retcode = false;
                    response.Result = "FAIL";
                    response.Message = commandResult.message;
                    response.Value = commandResult.value;
                }
            }
            else
            {
                retcode = false;
                response.Result = "FAIL";
                response.Message = "Invalid option value";
            }
        }
        else if (_commandLineInput.Command == "GET" && _commandLineInput.Options.Count == 0)
        {
            command = $"/get {command}";
            var commandResult = RunDDMCommand(command);

            if (commandResult.exitCode == -1)
            {
                retcode = false;
                response.Result = "FAIL";
                response.Message = commandResult.message;
                response.Value = commandResult.value;
            }
            else
            {
                retcode = true;
                response.Result = "PASS";
                response.Value = commandResult.exitCode.ToString();
            }
        }
        else
        {
            return NotSupportResponse();
        }

        Console.WriteLine(JsonConvert.SerializeObject(response, Formatting.Indented));
        output += "\n" + JsonConvert.SerializeObject(response, Formatting.Indented);
        return (retcode ? (int)CLI_ExitCode.success : (int)CLI_ExitCode.functional_error, output);
    }

    private static (int code, string result) NetworkKVMAccessReset(string command)
    {
        string output = string.Empty;
        bool retcode = false;

        var response = new NKVM_RESPONSE
        {
            Command = _commandLineInput.Command,
            TargetFeature = _commandLineInput.TargetFeature
        };

        if (_commandLineInput.Command == "SET" && _commandLineInput.Options.Count == 0)
        {
            command = $"/{command}";
            var commandResult = RunDDMCommand(command);

            if (commandResult.exitCode == 0)
            {
                retcode = true;
                response.Result = "PASS";
            }
            else
            {
                retcode = false;
                response.Result = "FAIL";
                response.Message = commandResult.message;
                response.Value = commandResult.value;
            }
        }
        else
        {
            return NotSupportResponse();
        }

        Console.WriteLine(JsonConvert.SerializeObject(response, Formatting.Indented));
        output += "\n" + JsonConvert.SerializeObject(response, Formatting.Indented);
        return (retcode ? (int)CLI_ExitCode.success : (int)CLI_ExitCode.functional_error, output);
    }
}
