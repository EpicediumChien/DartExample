using DDPM.SA.Common.Defer;
using Dell.Client.Framework.Common;
using System;
using System.Threading.Tasks;
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
        fail_SWUpdate = 105, //Jerry add
        fail_option_missing,
        fail_option_name,
        fail_option_value,
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
        fail_analytics_command_notsupport,
        null_settings_plugin_IT,
        fail_SetSettings_ITSettingsValue,
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
        IT_Command_Not_Support,
        NoUpdate,
        Diagnostic_Report_fail
    }

    public class CLIEventArgs : EventArgs //definition for ICliManagerIT
    {
        public string command_guid_string { get; set; }
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

        void sendToastResult(string defer_id, bool isDefer); // add @ 20241210 stephen

        void sendDeviceCheckResult(bool result); // add @ 20250116 stephen


        event EventHandler<CLIEventArgs> CLIActionEvent;
        event EventHandler<CLIEventToastArgs> CLIToastEvent;    // add @ 20241210 stephen
        event EventHandler<CLIEventDeviceConnArgs> CLIDeviceCheckEvent;    // add @ 20250116 stephen

    }

    /// <summary>
    /// Public interface for [CLI subagent] to run as elevated user role
    /// </summary>
    public interface ICliManagerIT : IFrameworkPlugin
    {
        //Input is command line parsing object, and the return integer is ExitCode
        Task<CLIEventResult> PerformCommandLineRelay(CommandLineInput commandLineInput);

        Task<bool> checkDefer(int from, string guid, string commanddata); // add @ 20241210 stephen
        Task<bool> checkDeferSchedule(int from, string guid, DeferItem item); // add @ 20241210 stephen
        Task showNotification(int from, string guid, DeferItem item); // add @ 20241219 stephen

        Task<bool> checkDeviceConn(int from, string guid, string commanddata, string str_command); // add @ 20250116 stephen


        //For remote management to subscribe event with result
        public event EventHandler<CLIEventResult> CLIActionResult;
    }

    public interface ICliProxy : IFrameworkPlugin
    {
        //no action need
    }

    // add @ 20241210 stephen
    public class CLIEventToastArgs : EventArgs
    {
        public string defer_id { get; set; }
        public string toast_message { get; set; }
        public bool is_defer { get; set; }
    }

    // add @ 20250116 stephen
    public class CLIEventDeviceConnArgs : EventArgs
    {
        public string commands { get; set; }
    }
}
