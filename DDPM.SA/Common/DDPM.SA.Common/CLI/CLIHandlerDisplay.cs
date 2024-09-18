using DDPM.SA.Common.Settings;
using Dell.Client.Framework.Common;
using MS.WindowsAPICodePack.Internal;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using static DDPM.SA.Common.ICLICommandTable;

namespace DDPM.SA.Common.CLI
{
    public class CLIHandlerDisplay : CLIHandlerApp
    {
        public static CLIEventResult CLI_Display_LockUnlock(ILog Log, object inputData, object settingsPlugin, CommandLineInput commandLineInput, string action_guid)
       => CLIHandlerApp.CLI_App_LockUnlock(Log, inputData, settingsPlugin, commandLineInput, action_guid);
        public static CLIEventResult CLI_Display_LockUnlockWithUserAction(ILog Log, object inputData, object settingsPlugin, CommandLineInput commandLineInput, string action_guid)
       => CLIHandlerApp.CLI_Common_LockUlockWithUserAction(Log, inputData, settingsPlugin, commandLineInput, action_guid);
        public static CLIEventResult CLI_Display_RestoreFactoryDefault(ILog Log, object inputData, object settingsPlugin, CommandLineInput commandLineInput, string action_guid)
       => CLIHandlerApp.CLI_App_LockUnlock(Log, inputData, settingsPlugin, commandLineInput, action_guid);
        private static void WriteLog(ILog Log, string text, log_type log_type = log_type.info)
        {
            text = "[CLIHandlerDisplay] " + text;
            Console.WriteLine(text);
            if (log_type == log_type.info)
                Log.Info(text);
            else
                Log.Error(text);
        }

        private enum log_type
        {
            info = 0,
            error
        }
    }
}