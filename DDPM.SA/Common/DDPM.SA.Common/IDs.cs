namespace DDPM.SA.Common
{
    public class IDs
    {
        //DDPM SA
        public const string DDPM_AGENT_ID = "{9BBE5845-8C58-45B3-BAEA-C2C061CD8465}";

        public const string DDPM_MUTEX_ID = "{59D9F3CB-FCEF-4C7C-BAB9-8597EA7159EE}";

        //DDPM SA User
        public const string DDPM_USER_AGENT_ID = "{2E365D80-A333-42E9-B855-F326DFEF2C34}";

        public const string DDPM_USER_MUTEX_ID = "{6F2EB650-8307-42CC-B69B-D94976C24692}";
        //-----------------------
        //Plugins for DDPM.SA
        //Settings Manager
        public const string DDPM_SETTINGS_MANAGER_PLUGIN_ID = "{F6909D0F-C70B-4B84-8EC3-9E506550A998}";

        //SWUpdate
        public const string SWUpdate_PLUGIN_ID = "{F716E8C1-1F8D-4BC6-83DA-51CD26031335}";//Should locate at system SA as SDL requirement

        //FWUpdate
        public const string FWUPDATE_PLUGIN_ID = "{7D53B92E-5648-4ADB-9E33-4C74FFE7BE8C}";//Should move to System SA as SDL requirement

        //CLI Manager Plugin
        public const string CLI_Manager_Plugin = "{2B76DC4B-39E7-4DBE-946E-5112CDAF37EA}";//Used to relay elevated CLI subagent command to SA device manager

        //PlatinumSDK Plugin
        public const string PlatinumSDK_Plugin = "{BFAA77E8-CADF-4CE4-9473-363E65C6B4E0}";

        //CMA Manager Plugin, open interface for CMA team to get/set config and receive event from DDPM
        public const string CMA_Manager_Plugin = "{A9C07BA5-6499-4730-B5E1-3143F6A9409F}";
        //-----------------------
        //Plugins for DDPM.SA.User
        //EAPlugin (DDPM.SA.Plugins.User.EasyArrange project) - Robert_Lin created 2024-6-18
        public const string DDPM_EAPlugin_PLUGIN_ID = "{998CE5F9-19FE-4EDF-841A-2497F39623BF}";

        //PipPbpManger Robert_Lin 2024-5-23
        public const string PipPbp_Manager_PLUGIN_ID = "{AFF8831F-5BEB-49F0-9098-DD59D313CB28}";

        //Peripherals
        public const string DDPM_PERIPHERALS_PLUGIN_ID = "{CF223214-FAF4-4595-8ED4-C6C1F65FA02C}";

        //VcpCore
        public const string VCP_CORE_PLUGIN_ID = "{A409E0AF-E2C3-4568-A194-B2D173DA26D4}";

        //DisplayManager
        public const string Display_Manager_PLUGIN_ID = "{39A9CF54-2EC0-434E-A0BF-49FF43F8C824}";

        //DeviceManager
        public const string Device_Manager_Plugin_ID = "{9829A9C5-E129-488A-A522-B8FF705051EE}";

        //SchedulerManager
        public const string Scheduler_Manager_Plugin_ID = "{EEE0AA91-11EE-46BD-B1FD-88D590C3052C}";

        //TelementryScheduler
        public const string Telementry_Scheduler_Plugin_ID = "{EEB96C41-01ED-48DE-A7C7-B01E7A081CAD}";

        //DisplayProperties
        public const string DisplayProperties_PLUGIN_ID = "{67A0D126-10BE-4EDB-95DC-8A4162AA3F6B}";

        //ColorPreset
        public const string DDPM_COLOR_PRESET_PLUGIN_ID = "{AC2BD6A8-0678-482A-8274-3B8E80E12A81}";

        //USBKVM
        public const string DDPM_USBKVM_PLUGIN_ID = "{633ED971-086A-49E7-91E1-F6EB6E15CC13}";

        //NetworkKVM
        public const string DDPM_NKVM_PLUGIN_ID = "{149EF7F9-BF22-4E00-86B1-44CAC25CCA7E}";

        //Hotkey
        public const string DDPM_HOTKEY_PLUGIN_ID = "{C01C5C25-B7F8-4F08-8CD8-4E16928CC254}";

        //CLIProxy
        public const string DDPM_CLI_Proxy_Plugin = "{70F44C60-49D5-4127-A2D1-BE46D0D8D6FD}";

        //Plugins for User.CLIProxy
        //Display
        public const string CLI_Plugin_Display = "{E993A925-BAF0-42DF-B878-7EC075D5B941}";

        //Peripherals
        public const string CLI_Plugin_Peripherals = "{FE361209-1BEB-4B52-AC1A-D0227B7641CB}";

        //User.SettingsManager
        public const string DDPM_SETTINGSMANAGER_SA_PLUGIN_ID = "{99DB213B-220B-41C7-8B3A-0131CA7A4DD1}";

        //Actions Manger
        public const string DDPM_ACTIONS_MANGER_PLUGIN_ID = "{E5DA6004-21DC-4058-917F-A0ECF838EA03}";

        //CLIProxy
        public const string DDPM_CMA_Proxy_Plugin = "{09F670EB-3F2B-4005-9A8B-D4BF1033A425}";

        /// <summary>
        /// UniqueId for the thick client (NGA)
        /// </summary>
        public const string ThickClientUniqueGuid = "{5AB199D2-F7BF-4021-8741-A0FDE397B8D5}";

        //DTPProxy
        public const string DDPM_DTP_Proxy_Plugin = "{d034ee8f-7c8a-4296-8b5b-33b4e978c6b5}";

        //EzMemory
        public const string DDPM_EMPlugin_PLUGIN_ID = "{388F486D-2A86-421C-B571-3AAF9F839BA2}";
    }
}