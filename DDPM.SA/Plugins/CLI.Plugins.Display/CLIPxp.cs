using DDPM.SA.Common;
using DDPM.SA.Common.Display;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using VcpCore.Common;
using static DDPM.SA.Common.ICLICommandTable;
using Convert = System.Convert;

namespace CLI.Plugins.Display
{
    internal class CLIPxp
    {
        #region Input

        private static IDeviceManagerSA _devMgr;
        private static CommandLineInput _cmdLineInput;

        #endregion Input

        #region Output

        private static List<CLI_RESPONSE> _responses = new List<CLI_RESPONSE>();

        public static List<CLI_RESPONSE> Responses
        {
            get => _responses;
        }

        #endregion Output

        #region Working data

        private static List<MonitorInfo> _AllInfoMonitors;
        private static List<int> _monitorIndeies = new List<int>();

        #endregion Working data

        /// <summary>
        /// Main entry of CLIPxp
        /// </summary>
        /// <param name="devMgr">IDeviceManagerSA must not null</param>
        /// <param name="cmdLineInput">The command line arguments</param>
        /// <returns>one of CLI_ExitCode value</returns>
        /// <output>
        /// Property 'Responses': a list of CLI_RESPONSE, can be used to converted to Json format.
        /// </output>
        public static int Execute(IDeviceManagerSA devMgr, CommandLineInput cmdLineInput)
        {
            _responses.Clear();
            _cmdLineInput = cmdLineInput;

            CLI_RESPONSE response = new CLI_RESPONSE();
            response.Command = cmdLineInput.Command;
            response.TargetFeature = cmdLineInput.TargetFeature;
            response.Index = String.Join(",", cmdLineInput.DeviceIndex.ToArray());
            response.ServiceTag = String.Join(",", cmdLineInput.ServiceTag.ToArray());
            if (devMgr == null)
            {
                response.Result = "ERROR";
                response.Message = "DeviceManagerDA is null";
                _responses.Add(response);
                return (int)CLI_ExitCode.null_device_manager;
            }
            _devMgr = devMgr;

            _AllInfoMonitors = devMgr.GetMonitors().Result;
            _monitorIndeies = GetMonitorIndeies(cmdLineInput, _AllInfoMonitors);

            if (cmdLineInput.Command.Equals("Get", StringComparison.OrdinalIgnoreCase))
            {
                if (cmdLineInput.TargetFeature.Equals("PxP", StringComparison.OrdinalIgnoreCase))
                {
                    return GetPxpMode();
                }
                if (cmdLineInput.TargetFeature.Equals("SubInput", StringComparison.OrdinalIgnoreCase))
                {
                    return GetSubInput();
                }
                if (cmdLineInput.TargetFeature.Equals("PxPZoom", StringComparison.OrdinalIgnoreCase))
                {
                    return GetPxpZoom();
                }
            }
            if (cmdLineInput.Command.Equals("Set", StringComparison.OrdinalIgnoreCase))
            {
                if (cmdLineInput.TargetFeature.Equals("SwapVideo", StringComparison.OrdinalIgnoreCase))
                {
                    return SetSwapVideo();
                }
                if (cmdLineInput.TargetFeature.Equals("PxP", StringComparison.OrdinalIgnoreCase))
                {
                    return SetPxpMode();
                }
                if (cmdLineInput.TargetFeature.Equals("SubInput", StringComparison.OrdinalIgnoreCase) ||
                    cmdLineInput.TargetFeature.Equals("AssignSubInput", StringComparison.OrdinalIgnoreCase))
                {
                    return SetSubInput();
                }
                if (cmdLineInput.TargetFeature.Equals("PxPZoom", StringComparison.OrdinalIgnoreCase))
                {
                    return SetPxpZoom();
                }
                if (cmdLineInput.TargetFeature.Equals("SwapUSB", StringComparison.OrdinalIgnoreCase))
                {
                    return SetSwapUSB();
                }
            }

            response.Result = "ERROR";
            response.Message = "Unsupported command or name.";
            _responses.Add(response);

            return (int)CLI_ExitCode.fail_NotSupport;
        }
        public static string change_0base_to_1base(string value)
        {
            return (int.Parse(value) + 1).ToString();
        }

        //CmdLine: -set -name=Display.SwapVideo [-value[=source,target]]
        //Examples: [-value[=source,target]]
        // not specfied : set source=mainInput, set target=sub1
        // -value       : not specify source and target, set source=mainInput, set target=sub1
        // -value=mainInput,sub2 : specifys source=mainInput and target=sub2
        // The value of -value can be:
        //    (main or mainInput), sub1, sub2, and sub3
        private static int SetSwapVideo()
        {
            //Phase 1. Determine source and target from the -value= option
            //1 Not specified => source=main, target=sub1
            //2 only -value or -value=, but no source,taget => source=main, target=sub1
            //3 specify -value=source,target
            //  3.1. Item count != 2 => Invaild syntax
            //  3.2. Try to parse source and target to main,sub1,sub2,sub3 (type of ePxpInputs)
            //       1) fail to parse => Invalid value
            //       2) source = target => noting to do
            //       3) others => accept the source,target values
            //4 If there are multiple -value=source,target specified => Accespt the first one only
            ePxpInputs source = ePxpInputs.main;
            ePxpInputs target = ePxpInputs.sub1;
            string rawValue = source + "," + target;

            //Find the first -value option
            CommandType_Option? valueOption = _cmdLineInput.Options.FirstOrDefault(x => x.Option_Name.Equals("value", StringComparison.OrdinalIgnoreCase));
            //Dean 0626 fix SAST issue
            if (_cmdLineInput.Options.Count != 0 && valueOption != null)
            {
                /*
                CLI_RESPONSE response = new CLI_RESPONSE()
                {
                    Command = _cmdLineInput.Command,
                    TargetFeature = _cmdLineInput.TargetFeature
                };
                response.Value = "";
                response.Index = String.Join(",", _cmdLineInput.DeviceIndex.ToArray());
                response.ServiceTag = String.Join(",", _cmdLineInput.ServiceTag.ToArray());
                response.Result = "No keyword -value";
                response.Message = "source=target in (-value=source,target).";
                _responses.Add(response);
                return (int)CLI_ExitCode.nothing_to_do;
                */
                rawValue = valueOption.Option_Value;
            }
            //string rawValue = valueOption.Option_Value;

            //If not Case 1, has -value
            if (valueOption != null)
            {
                //If not Case 2, has -value and "=source,target"
                if (!String.IsNullOrWhiteSpace(valueOption.Option_Value))
                {
                    string[] opValues = valueOption.Option_Value.Split(',');
                    //Case 3.1
                    if (opValues.Length != 2)
                    {
                        CLI_RESPONSE response = new CLI_RESPONSE()
                        {
                            Command = _cmdLineInput.Command,
                            TargetFeature = _cmdLineInput.TargetFeature
                        };
                        response.Value = rawValue;
                        response.Index = change_0base_to_1base(String.Join(",", _cmdLineInput.DeviceIndex.ToArray()));
                        response.ServiceTag = String.Join(",", _cmdLineInput.ServiceTag.ToArray());
                        response.Result = "ERROR";
                        response.Message = "Invalid command line syntax (-value=source,target).";
                        _responses.Add(response);
                        return (int)CLI_ExitCode.invalide_cmdline_syntax;
                    }

                    source = GetPxpInputFromString(opValues[0]);
                    target = GetPxpInputFromString(opValues[1]);

                    //case 3.2 1)
                    if ((source == ePxpInputs.invalid) || (target == ePxpInputs.invalid))
                    {
                        CLI_RESPONSE response = new CLI_RESPONSE()
                        {
                            Command = _cmdLineInput.Command,
                            TargetFeature = _cmdLineInput.TargetFeature
                        };
                        response.Value = rawValue;
                        response.Index = change_0base_to_1base(String.Join(",", _cmdLineInput.DeviceIndex.ToArray()));
                        response.ServiceTag = String.Join(",", _cmdLineInput.ServiceTag.ToArray());
                        response.Result = "ERROR";
                        response.Message = "Invalid input name of (-value=source,target).";
                        _responses.Add(response);
                        return (int)CLI_ExitCode.invalide_cmdline_syntax;
                    }

                    //Case 3.2 2)
                    if (source == target)
                    {
                        CLI_RESPONSE response = new CLI_RESPONSE()
                        {
                            Command = _cmdLineInput.Command,
                            TargetFeature = _cmdLineInput.TargetFeature
                        };
                        response.Value = rawValue;
                        response.Index = change_0base_to_1base(String.Join(",", _cmdLineInput.DeviceIndex.ToArray()));
                        response.ServiceTag = String.Join(",", _cmdLineInput.ServiceTag.ToArray());
                        response.Result = "DO nothing";
                        response.Message = "source=target in (-value=source,target).";
                        _responses.Add(response);
                        return (int)CLI_ExitCode.nothing_to_do;
                    }
                } //If not Case 2,
            } //if (valueOption != null)

            //Phase 2. Do the function for each monitor
            int errCount = 0;
            foreach (int idx in _monitorIndeies)
            {
                bool isPass = _devMgr.VideoSwap(_AllInfoMonitors[idx], (UInt16)source, (UInt16)target).Result;

                CLI_RESPONSE response = new CLI_RESPONSE()
                {
                    Command = _cmdLineInput.Command,
                    TargetFeature = _cmdLineInput.TargetFeature
                };
                response.Value = rawValue;
                response.Index = change_0base_to_1base(idx.ToString());
                response.ServiceTag = _AllInfoMonitors[idx].edid.ServiceTag;
                if (isPass)
                {
                    response.Result = "PASS";
                    response.Message = "";
                }
                else
                {
                    response.Result = "FAIL";
                    response.Message = "Fail to swap video.";
                    errCount++;
                }
                _responses.Add(response);
            }

            if (errCount == 0)
                return (int)CLI_ExitCode.success;
            else
                return (int)CLI_ExitCode.functional_error;
        }

        //CmdLine: -get -name=Display.PxP
        private static int GetPxpMode()
        {
            int errCount = 0;
            foreach (int idx in _monitorIndeies)
            {
                CLI_RESPONSE_PxpMode response = new CLI_RESPONSE_PxpMode();
                response.Command = _cmdLineInput.Command;
                response.TargetFeature = _cmdLineInput.TargetFeature;
                response.Index = change_0base_to_1base(idx.ToString());

                //1 Get supported modes
                UInt16[] caps = _devMgr.GetPipPbpCapabilitiesWords(_AllInfoMonitors[idx]).Result;
                if (caps == null)
                {
                    response.SupportedModes = new string[] { "Fail to get PIP/PBP Capabilities" };
                    response.Result = "ERROR";
                    response.Message = "Fail to get PIP/PBP Capabilities.";
                }
                else
                {
                    string[] capsStr = PxpModeObj.GetModeArgListFromModes(caps);
                    response.SupportedModes = capsStr;
                }

                //2 Get current mode
                ObjGetVCP objGetVCP = _devMgr.GetPxpMode(_AllInfoMonitors[idx]).Result;
                if ((objGetVCP == null) || (!objGetVCP.result))
                {
                    response.Result = "ERROR";
                    response.Message = "Fail to current PXP mode.";
                    errCount++;
                }
                else
                {
                    UInt16 modeCode = (UInt16)Convert.ToUInt16(objGetVCP.value);
                    response.CurrentMode = PxpModeObj.GetArgFromModeCode(modeCode);
                    response.CurrentModeCode = modeCode;
                    response.Result = "PASS";
                    response.Message = "";
                }

                _responses.Add(response);
            }

            if (errCount == 0)
                return (int)CLI_ExitCode.success;
            else
                return (int)CLI_ExitCode.functional_error;
        }

        //CmdLine: -set -name=Display.PxP -value=pxpMode
        private static int SetPxpMode()
        {
            //Phase 1. Get the -value=pxpMode
            // Case_1. No -value specify => return error.
            // Case_2. -value=pxpArg, for example: -value=pip-large
            // Case_3. -value=pxpMode, for example: -value=34
            // Case_4. pxpMode is hexdecimal integiter, for example: -value=0x22
            // Case_5. pxpArg or pxpMode is not a valide value => return error

            bool isOK = false;
            InputSourceObj? sub1 = null, sub2 = null, sub3 = null;
            //Find the first -value option
            CommandType_Option? valueOption = _cmdLineInput.Options.FirstOrDefault(x => x.Option_Name.Equals("value", StringComparison.OrdinalIgnoreCase));

            // Case_1. No -value specify => return error.
            if (valueOption == null)
            {
                CLI_RESPONSE response = new CLI_RESPONSE()
                {
                    Command = _cmdLineInput.Command,
                    TargetFeature = _cmdLineInput.TargetFeature
                };
                response.Index = change_0base_to_1base(String.Join(",", _cmdLineInput.DeviceIndex.ToArray()));
                response.ServiceTag = String.Join(",", _cmdLineInput.ServiceTag.ToArray());
                response.Result = "ERROR";
                response.Message = "Invalid command line syntax, missing (-value=pxpMode).";
                _responses.Add(response);
                return (int)CLI_ExitCode.invalide_cmdline_syntax;
            }

            // more than one -value => return error.
            if (_cmdLineInput.Options.Count > 2)
            {
                CLI_RESPONSE response = new CLI_RESPONSE()
                {
                    Command = _cmdLineInput.Command,
                    TargetFeature = _cmdLineInput.TargetFeature
                };
                response.Index = change_0base_to_1base(String.Join(",", _cmdLineInput.DeviceIndex.ToArray()));
                response.ServiceTag = String.Join(",", _cmdLineInput.ServiceTag.ToArray());
                response.Result = "ERROR";
                response.Message = "Invalid command line syntax, only one value (-value=pxpMode).";
                _responses.Add(response);
                return (int)CLI_ExitCode.invalide_cmdline_syntax;
            }

            string rawValue = valueOption.Option_Value;
            //Try for Case_2
            PxpModeObj? pxpModeObj = Array.Find(PxpModeObj.Table, x => x.Arg.Equals(rawValue, StringComparison.OrdinalIgnoreCase));
            //It's Case_2. -value=pxpArg, for example: -value=pip-large
            //Not Case_2
            if (pxpModeObj == null)
            {
                UInt16 modeCode = 0xffff;
                //Try Case_4
                if (rawValue.StartsWith("0x") || rawValue.StartsWith("0X"))
                {
                    //It's not Case_4, but start with "0x" => error
                    if (!UInt16.TryParse(rawValue.Substring(2), System.Globalization.NumberStyles.HexNumber,
                        CultureInfo.CurrentCulture, out modeCode))
                    {
                        CLI_RESPONSE response = new CLI_RESPONSE()
                        {
                            Command = _cmdLineInput.Command,
                            TargetFeature = _cmdLineInput.TargetFeature
                        };
                        response.Index = change_0base_to_1base(String.Join(",", _cmdLineInput.DeviceIndex.ToArray()));
                        response.ServiceTag = String.Join(",", _cmdLineInput.ServiceTag.ToArray());
                        response.Result = "ERROR";
                        response.Message = "Invalid pxpMode value in (-value=pxpMode).";
                        response.Value = rawValue;
                        _responses.Add(response);
                        return (int)CLI_ExitCode.invalide_cmdline_syntax;
                    }
                    //It's Case_4
                } //END of Try Case_4
                else
                {
                    //Try Case_3
                    if (!UInt16.TryParse(rawValue, out modeCode))
                    {
                        //It's not Case_3, return error
                        CLI_RESPONSE response = new CLI_RESPONSE()
                        {
                            Command = _cmdLineInput.Command,
                            TargetFeature = _cmdLineInput.TargetFeature
                        };
                        response.Index = change_0base_to_1base(string.Join(",", _cmdLineInput.DeviceIndex.ToArray()));
                        response.ServiceTag = String.Join(",", _cmdLineInput.ServiceTag.ToArray());
                        response.Result = "ERROR";
                        response.Message = "Invalid pxpMode value in (-value=pxpMode).";
                        response.Value = rawValue;
                        _responses.Add(response);
                        return (int)CLI_ExitCode.invalide_cmdline_syntax;
                    }
                    //It's Case_3
                }

                //Case_3 or Case_4, value in modeCode, need to verify if it's a valide code
                string pxpArg = PxpModeObj.GetArgFromModeCode(modeCode);
                if (String.IsNullOrWhiteSpace(pxpArg))
                {
                    //It's not Case_3, return error
                    CLI_RESPONSE response = new CLI_RESPONSE()
                    {
                        Command = _cmdLineInput.Command,
                        TargetFeature = _cmdLineInput.TargetFeature
                    };
                    response.Index = change_0base_to_1base(String.Join(",", _cmdLineInput.DeviceIndex.ToArray()));
                    response.ServiceTag = String.Join(",", _cmdLineInput.ServiceTag.ToArray());
                    response.Result = "ERROR";
                    response.Message = "Invalid pxpMode value in (-value=pxpMode). Try /get command.";
                    response.Value = rawValue;
                    _responses.Add(response);
                    return (int)CLI_ExitCode.invalide_cmdline_syntax;
                }

                pxpModeObj = new PxpModeObj(modeCode);
            } //END of Not Case_2

            //Phase 2. Call SA method to set PXP mode
            int errCount = 0;
            foreach (int idx in _monitorIndeies)
            {
                bool isPass = _devMgr.SetPbpMode(_AllInfoMonitors[idx], (UInt16)pxpModeObj.ModeCode).Result;
                if (_cmdLineInput.Options.Count == 2)
                {
                    if (!String.IsNullOrWhiteSpace(_cmdLineInput.Options[1].Option_Value))
                    {
                        string[] ss = _cmdLineInput.Options[1].Option_Value.Split(',');
                        if (ss.Length == 2)
                        {
                            sub1 = InputSourceObj.FindFirstByName(get_inputsource_type(ss[0]));
                            sub2 = InputSourceObj.FindFirstByName(get_inputsource_type(ss[1]));
                            sub3 = InputSourceObj.FindFirstByName(get_inputsource_type(ss[0]));
                            isOK = _devMgr.SetSubInputs(_AllInfoMonitors[idx], sub2, null, null).Result;
                        }
                    }
                }
                CLI_RESPONSE response = new CLI_RESPONSE()
                {
                    Command = _cmdLineInput.Command,
                    TargetFeature = _cmdLineInput.TargetFeature
                };
                response.Index = change_0base_to_1base(idx.ToString());
                response.ServiceTag = _AllInfoMonitors[idx].edid.ServiceTag;
                response.Value = rawValue;
                if (isPass || isOK)
                {
                    response.Result = "PASS";
                    response.Message = "";
                }
                else
                {
                    response.Result = "FAIL";
                    response.Message = "Fail to SetPxPMode.";
                    errCount++;
                }
                _responses.Add(response);
            } //for (idx)

            if (errCount == 0)
                return (int)CLI_ExitCode.success;
            else
                return (int)CLI_ExitCode.functional_error;
        }

        //CmdLine: -get -name=Display.SubInput
        private static int GetSubInput()
        {
            int errCount = 0;
            foreach (int idx in _monitorIndeies)
            {
                CLI_RESPONSE_SubInput response = new CLI_RESPONSE_SubInput();
                response.Command = _cmdLineInput.Command;
                response.TargetFeature = _cmdLineInput.TargetFeature;
                response.Index = change_0base_to_1base(idx.ToString());

                List<InputSourceObj> inputSources = _devMgr.GetSubInputs(_AllInfoMonitors[idx]).Result;
                if (inputSources == null)
                {
                    errCount++;
                    response.Result = "ERROR";
                    response.Message = "Fail to get SubInputs.";
                    response.SubInputCount = 0;
                }
                else if (inputSources.Count <= 0)
                {
                    response.Result = "No sub input.";
                    response.Message = "No sub input.";
                    response.SubInputCount = 0;
                }
                else
                {
                    response.SubInputCount = inputSources.Count;
                    response.Sub1InputSource = inputSources[0].Name;
                    if (inputSources.Count > 1)
                        response.Sub2InputSource = inputSources[1].Name;
                    if (inputSources.Count > 2)
                        response.Sub3InputSource = inputSources[2].Name;
                    response.Result = "PASS";
                    response.Message = "";
                }
                _responses.Add(response);
            } //foreach (idx)

            if (errCount == 0)
                return (int)CLI_ExitCode.success;
            else
                return (int)CLI_ExitCode.functional_error;
        }

        //CmdLine: -set -name=Display.SubInput [options]
        //CmdLine: -set -name=Display.AssignSubInput [options]
        //Where [options]:
        //  [-value=inputSource] [-sub1=inputSource] [-sub2=inputSource] [-sub3=inputSource]
        //  value and sub1 will set sub1=inputSource
        //Exmaples
        // Ex1. -value=USB-C1       and     -sub1=USB-C1
        //      Set sub1's inputSource to USB-C1
        // Ex2. -value=USB-C -sub3=HDMI2
        //      Set sub1 to USB-C (or USB-C1), and sub3 to HDMI-2
        // Ex3. -sub1=HDMI-2 -sub2=USB
        //      Set sub1 to HDMI-2 and sub2 to USB-C1
        // Ex4. -sub3=USB-C1 -sub1=HDMI1 -sub2=HDMI-2
        //      Set sub1 to HDIM-1, sub2 to HDMI-2, and sub3 to USB-C1
        private static int SetSubInput()
        {
            InputSourceObj? sub1 = null, sub2 = null, sub3 = null;

            //Phase 1. Get -value=inputSource or -sub1=inputSource
            // Case_1. No -value, or no inputSource => return error
            // Case_2. Find matched inputSource from input Source List
            //         "HDMI" = "HDMI1"; "USBC" = "USB-C" = "USB-C1"; ,....

            //Find the first -value option            
            CommandType_Option? valueOption = _cmdLineInput.Options.FirstOrDefault(x => x.Option_Name.Equals("value", StringComparison.OrdinalIgnoreCase));
            string[] ss = null;

            ss = _cmdLineInput.Options[0].Option_Value.Split(new string[] { "," }, StringSplitOptions.None);
            //-value is specified
            if (ss.Length >= 1 && ss.Length < 2)
            {
                if (ss[0] != null)
                {
                    sub1 = InputSourceObj.FindFirstByName(get_inputsource_type(ss[0]));
                }
            }

            if (ss.Length > 1 && ss.Length < 3)
            {
                if (ss[1] != null)
                {
                    sub1 = InputSourceObj.FindFirstByName(get_inputsource_type(ss[0]));
                    sub2 = InputSourceObj.FindFirstByName(get_inputsource_type(ss[1]));
                }
            }

            if (ss.Length > 2 && ss.Length < 4)
            {
                if (ss[2] != null)
                {
                    sub1 = InputSourceObj.FindFirstByName(get_inputsource_type(ss[0]));
                    sub2 = InputSourceObj.FindFirstByName(get_inputsource_type(ss[1]));
                    sub3 = InputSourceObj.FindFirstByName(get_inputsource_type(ss[2]));
                }
            }

            //Phase 4. If no any option
            if ((sub1 == null) && (sub2 == null) && (sub3 == null))
            {
                CLI_RESPONSE response = new CLI_RESPONSE()
                {
                    Command = _cmdLineInput.Command,
                    TargetFeature = _cmdLineInput.TargetFeature
                };
                response.Index = change_0base_to_1base(String.Join(",", _cmdLineInput.DeviceIndex.ToArray()));
                response.ServiceTag = String.Join(",", _cmdLineInput.ServiceTag.ToArray());
                response.Result = "ERROR";
                response.Message = "Invalid command line syntax, missing options (-value,-sub1,-sub2, or -sub3).";
                response.Value = "";
                _responses.Add(response);
                return (int)CLI_ExitCode.invalide_cmdline_syntax;
            }

            //Phase 5. Call to SA (monitor) to set sub inputs
            int errCount = 0;
            foreach (int idx in _monitorIndeies)
            {
                CLI_RESPONSE_SubInput response = new CLI_RESPONSE_SubInput();
                response.Command = _cmdLineInput.Command;
                response.TargetFeature = _cmdLineInput.TargetFeature;
                response.Index = change_0base_to_1base(idx.ToString());
                response.Sub1InputSource = (sub1 == null) ? "" : sub1.Name;
                response.Sub2InputSource = (sub2 == null) ? "" : sub2.Name;
                response.Sub3InputSource = (sub3 == null) ? "" : sub3.Name;

                bool isOK = _devMgr.SetSubInputs(_AllInfoMonitors[idx], sub1, sub2, sub3).Result;

                if (isOK)
                {
                    response.Result = "PASS";
                    response.Message = "";
                }
                else
                {
                    response.Result = "ERROR";
                    response.Message = "Fail to set sub-inputs.";
                    errCount++;
                }
                _responses.Add(response);
            }

            if (errCount == 0)
                return (int)CLI_ExitCode.success;
            else
                return (int)CLI_ExitCode.functional_error;
        }

        private static string get_inputsource_type(string index)
        {
            switch (index)
            {
                case "HDMI": return "HDMI-1";
                case "HDMI1": return "HDMI-1";
                case "HDMI-1": return "HDMI-1";

                case "HDMI2": return "HDMI-2";
                case "HDMI-2": return "HDMI-2";

                case "DP": return "DISPLAYPORT-1";
                case "DP1": return "DISPLAYPORT-1";
                case "DP-1": return "DISPLAYPORT-1";
                case "DISPLAYPORT": return "DISPLAYPORT-1";
                case "DISPLAYPORT1": return "DISPLAYPORT-1";
                case "DISPLAYPORT-1": return "DISPLAYPORT-1";

                case "DP2": return "DISPLAYPORT-2";
                case "DP-2": return "DISPLAYPORT-2";
                case "DISPLAYPORT2": return "DISPLAYPORT-2";
                case "DISPLAYPORT-2": return "DISPLAYPORT-2";

                case "USBC": return "USB-C1";
                case "USBC1": return "USB-C1";
                case "USB-C": return "USB-C1";
                case "USB-C1": return "USB-C1";

                case "USBC2": return "USB-C2";
                case "USB-C2": return "USB-C2";

                case "TBT": return "Thunderbolt-1";
                case "TBT1": return "Thunderbolt-1";
                case "THUNDERBOLT": return "Thunderbolt-1";
                case "THUNDERBOLT1": return "Thunderbolt-1";
                case "THUNDERBOLT-1": return "Thunderbolt-1";

                case "TBT2": return "Thunderbolt-2";
                case "THUNDERBOLT2": return "Thunderbolt-2";
                case "THUNDERBOLT-2": return "Thunderbolt-2";

                default: return "Unknown";
            }
        }

        //CmdLine: -set -name=Display.PxPZoom
        private static int SetPxpZoom()
        {
            int errCount = 0;
            foreach (int idx in _monitorIndeies)
            {
                bool isPass = _devMgr.SetVCPCapability(_AllInfoMonitors[idx], 0xE5, 0x02).Result;

                CLI_RESPONSE response = new CLI_RESPONSE()
                {
                    Command = _cmdLineInput.Command,
                    TargetFeature = _cmdLineInput.TargetFeature
                };
                response.Index = change_0base_to_1base(idx.ToString());
                response.ServiceTag = _AllInfoMonitors[idx].edid.ServiceTag;
                if (isPass)
                {
                    response.Result = "PASS";
                    response.Message = "";
                }
                else
                {
                    response.Result = "FAIL";
                    response.Message = "Fail to set PXP Zoom.";
                    errCount++;
                }
                _responses.Add(response);
            }
            if (errCount == 0)
                return (int)CLI_ExitCode.success;
            else
                return (int)CLI_ExitCode.functional_error;
        }

        private static int GetPxpZoom()
        {
            int errCount = 0;
            bool isPass = false;
            foreach (int idx in _monitorIndeies)
            {
                ObjGetVCP rc = new ObjGetVCP();
                rc = _devMgr.GetVCPCapability(_AllInfoMonitors[idx], 0xE5, 0x02).Result;
                if (rc != null)
                    isPass = true;


                CLI_RESPONSE response = new CLI_RESPONSE()
                {
                    Command = _cmdLineInput.Command,
                    TargetFeature = _cmdLineInput.TargetFeature
                };
                response.Value = (rc.value).ToString();
                response.Index = change_0base_to_1base(idx.ToString());
                response.ServiceTag = _AllInfoMonitors[idx].edid.ServiceTag;
                if (isPass)
                {
                    response.Result = "PASS";
                    response.Message = "";
                }
                else
                {
                    response.Result = "FAIL";
                    response.Message = "Fail to set PXP Zoom.";
                    errCount++;
                }
                _responses.Add(response);
            }
            if (errCount == 0)
                return (int)CLI_ExitCode.success;
            else
                return (int)CLI_ExitCode.functional_error;
        }

        //CmdLine: -set -name=Display.SwapUSB [-value=target]
        private static int SetSwapUSB()
        {
            //Phase 1. Get -value=target
            // Case_1. No -value=target => set target=0
            // Case_2. target should be 0~4

            //Find the first -value option
            CommandType_Option? valueOption = _cmdLineInput.Options.FirstOrDefault(x => x.Option_Name.Equals("value", StringComparison.OrdinalIgnoreCase));
            //Dean 0626 fix SAST issue
            string rawValue = string.Empty;
            if (valueOption == null)
            {
                rawValue = "1";
                /*CLI_RESPONSE response = new CLI_RESPONSE()
                {
                    Command = _cmdLineInput.Command,
                    TargetFeature = _cmdLineInput.TargetFeature
                };
                response.Value = "";
                response.Index = change_0base_to_1base(String.Join(",", _cmdLineInput.DeviceIndex.ToArray()));
                response.ServiceTag = String.Join(",", _cmdLineInput.ServiceTag.ToArray());
                response.Result = "No keyword -value";
                response.Message = "source=target in (-value=source,target).";
                _responses.Add(response);
                return (int)CLI_ExitCode.nothing_to_do;*/
            }
            else
            {
                rawValue = valueOption.Option_Value;
            }
            int target = 0;

            //Not Case_1
            if (valueOption != null)
            {
                //Try to parse target
                // Not empty
                if (!String.IsNullOrWhiteSpace(rawValue))
                {
                    bool targetOK = int.TryParse(rawValue, out target);

                    //target is NOT an integer or not in range of [0~4]
                    if ((!targetOK) || (target < 0) || (target > 4))
                    {
                        CLI_RESPONSE response = new CLI_RESPONSE()
                        {
                            Command = _cmdLineInput.Command,
                            TargetFeature = _cmdLineInput.TargetFeature
                        };
                        response.Value = rawValue;
                        response.Index = change_0base_to_1base(String.Join(",", _cmdLineInput.DeviceIndex.ToArray()));
                        response.ServiceTag = String.Join(",", _cmdLineInput.ServiceTag.ToArray());
                        response.Result = "ERROR";
                        response.Message = "Invalid command line syntax, target should be 0~4 in (-value=target).";
                        _responses.Add(response);
                        return (int)CLI_ExitCode.invalide_cmdline_syntax;
                    } //if (!targetOK
                } //if (Not empty)
            } //if has -value

            //Prepase for the VCP Code Word
            // VCP 0xE7 writeValue=FF0x where is x=target
            UInt16 writeValue = (UInt16)(0xFF00 + target);

            //Phase 2. Set command to monitor
            int errCount = 0;
            foreach (int idx in _monitorIndeies)
            {
                bool isPass = _devMgr.SetVCPCapability(_AllInfoMonitors[idx], (byte)0xE7, writeValue).Result;
                CLI_RESPONSE response = new CLI_RESPONSE()
                {
                    Command = _cmdLineInput.Command,
                    TargetFeature = _cmdLineInput.TargetFeature
                };
                response.Index = change_0base_to_1base(idx.ToString());
                response.ServiceTag = _AllInfoMonitors[idx].edid.ServiceTag;
                response.Value = rawValue;
                if (isPass)
                {
                    response.Result = "PASS";
                    response.Message = "";
                }
                else
                {
                    response.Result = "FAIL";
                    response.Message = "Fail to SetSwapUSB.";
                    errCount++;
                }
                _responses.Add(response);
            } //for (idx)

            if (errCount == 0)
                return (int)CLI_ExitCode.success;
            else
                return (int)CLI_ExitCode.functional_error;
        }

        #region Helpers

        /// <summary>
        /// Get the list of monitorInfos index from commandLineInput
        /// </summary>
        /// <param name="cmdLineInput"></param>
        /// <param name="allMonitors"></param>
        /// <returns>
        /// Return list of monitor index. or below error:
        /// null = error, _AllMonitors is null
        /// empty list = No matched monitor is found
        /// </returns>
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
            //If -Index=[{idx0}],[{idx1}],[{idx2}],... is specified
            if (cmdLineInput.DeviceIndex.Count > 0)
            {
                isAllMonitors = false;
                foreach (string idxString in cmdLineInput.DeviceIndex)
                {
                    int idx;
                    if (int.TryParse(idxString, out idx))
                    {
                        if ((idx >= 0) && (idx < allMonitors.Count))
                        {
                            listOut.Add(idx);
                        }
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

        /// <summary>
        /// Parsing string to ePxpInpus
        /// </summary>
        /// <param name="input">
        /// Should be either one: "main", "mainInput", "sub1", "sub2", or "sub3"
        /// </param>
        /// <returns>ePxpInpus value</returns>
        private static ePxpInputs GetPxpInputFromString(string input)
        {
            if (input.Equals("main", StringComparison.OrdinalIgnoreCase))
                return ePxpInputs.main;
            if (input.Equals("mainInput", StringComparison.OrdinalIgnoreCase))
                return ePxpInputs.main;
            if (input.Equals("sub1", StringComparison.OrdinalIgnoreCase))
                return ePxpInputs.sub1;
            if (input.Equals("sub2", StringComparison.OrdinalIgnoreCase))
                return ePxpInputs.sub2;
            if (input.Equals("sub3", StringComparison.OrdinalIgnoreCase))
                return ePxpInputs.sub3;
            return ePxpInputs.invalid;
        }

        #endregion Helpers
    }
}