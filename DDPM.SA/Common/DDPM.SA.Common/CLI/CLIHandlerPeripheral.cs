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
    public class CLIHandlerPeripheral : CLIHandlerApp
    {
        public static CLIEventResult CLI_Peripheral_RestoreFactoryDefault(ILog Log, object inputData, object settingsPlugin, CommandLineInput commandLineInput, string action_guid)
       => CLIHandlerApp.CLI_App_LockUnlock(Log, inputData, settingsPlugin, commandLineInput, action_guid);
        public static CLIEventResult CLI_Peripheral_LockUlockWithUserAction(ILog Log, object inputData, object settingsPlugin, CommandLineInput commandLineInput, string action_guid)
       => CLIHandlerApp.CLI_Common_LockUlockWithUserAction(Log, inputData, settingsPlugin, commandLineInput, action_guid);
    }
}