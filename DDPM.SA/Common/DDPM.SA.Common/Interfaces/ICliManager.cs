using Dell.Client.Framework.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VcpCore.Common;
using static DDPM.SA.Common.ICLICommandTable;

namespace DDPM.SA.Common
{
    public enum CLI_ExitCode
    {
        success = 0,
        null_device_manager = 1,
        unknow_command = 2,
        fail_SetVCPCapability = 3,
        fail_SetPeripheralProperty = 4,
        fail_SetPeripheralProperty_Guid = 5,
        fail_SetPeripheralProperty_Property = 6,
        fail_SetPeripheralProperty_Value = 7,
        fail_GetPeripheralProperty = 8,
        fail_GetPeripheralProperty_Guid = 9,
        fail_GetPeripheralProperty_Property = 10,
        fail_GetPeripheralProperty_Value = 11,
        fail_GetPeripheralProperty_NoConnectDevice = 12,
        fail_configHDR_settingfail = 13,
        fail_configHDR_inputfail = 14,
        fail_SetSettings_UserSettingsValue = 15,
        fail_GetSettings_UserSettings = 16,
        fail_FWUpdate = 17,
        fail_SetAlsFeatureFail = 18,
        fail_GetAlsFeatureFail = 19,
        fail_GetInputListFail = 20,
        fail_Unpair = 101,
        fail_FormantError = 102,
        fail_NotSupport = 103,
        fail_Value = 104, //Jason add
        //Robert_Lin, 2024-6-5 added
        functional_error,
        invalide_cmdline_syntax,
        no_matched_monitor_found,
        no_this_capability,
        nothing_to_do,
        //End of Robert_Lin, 2024-6-5 added
        no_monitor_connected,
        //Analytics, Dean
        fail_read_settings,
        fail_write_settings,
        fail_no_analytics_options,
        fail_analytics_option_notsupport,
        fail_notAdmin = 9999, //CLI is an IT/Admin tool, not allow normal privilege
        target_subagent_timeout,
        null_cli_manager,
        wait_command_result_timeout,
        empty_event_args,
        empty_command_input,
        required_plugin_not_ready,
        command_not_support,
        command_targettype_not_support,
        command_targetfeature_not_support,
        input_monitor_index_abnormal,
        input_monitor_over_count,
        invalid_servicetag,
        null_settings_plugin_IT,
        fail_SetSettings_ITSettingsValue
    }

    public class CLIEventArgs : EventArgs //definition for ICliManagerIT
    {
        public string command_guid_string {  get; set; }
        public CommandLineInput commandLineInput { get; set; }        
    }

    public class CLIEventResult //definition for ICliManagerSA
    {
        public string command_guid_string { get; set; }
        public string serialize_Json_response { get; set; }
        public int ExitCode { get; set; }
        public DateTime ticket { get; set; } //use to check if it is garbage
    }

    /// <summary>
    /// Public interface for [CLIProxy] to get command line events and call function to set result of command line
    /// </summary>
    public interface ICliManagerSA : IFrameworkPlugin
    {
        Task WriteCommandResult(CLIEventResult result);
        event EventHandler<CLIEventArgs> CLIActionEvent;
    }

    /// <summary>
    /// Public interface for [CLI subagent] to run as elevated user role
    /// </summary>
    public interface ICliManagerIT : IFrameworkPlugin
    {
        //Input is command line parsing object, and the return integer is ExitCode
        Task<CLIEventResult> PerformCommandLineRelay(CommandLineInput commandLineInput);        
    }

    public interface ICliProxy : IFrameworkPlugin
    {
        //no action need
    }
}