#region LicenceHeader

//
// Copyright © 2024, Dell Inc., All Rights Reserved.
// This material is confidential and a trade secret.  Permission to use this
// work for any purpose must be obtained in writing from Dell Inc.
//
// CLIManagerPlugin.cs created on 24/07/2024T08:04 PM
//

#endregion

using DDPM.SA.Common;
using DDPM.SA.Common.CLI;
using DDPM.SA.Common.Settings;
using Dell.Client.Framework.Common;
using Dell.Client.Framework.Common.Annotations;
using Dell.Client.Framework.Common.PluginConditions;
using Dell.Client.Framework.Interfaces;
using Microsoft;
using static DDPM.SA.Common.ICLICommandTable;

namespace DDPM.SA.Plugin.User.CLIManager
{
    [Plugin(IDs.DDPM_CLI_Proxy_Plugin, pluginName, PluginOrderGroupType.Core, Version = pluginVersion)]
    [Descriptor(Description = pluginDescription)]
    [Publisher(Name = publisherCompany, Website = publisherWebsite, Support = publisherSupport)]
    public class CLIProxyPlugin : BaseAgentPlugin, IDisposableObservable, ICliProxy
    {
        public const string PluginLogId = "CLIProxy";

        #region Private Members

        private const string pluginName = "CLIProxyPlugin";
        private const string pluginVersion = "1.0.0";
        private const string pluginDescription = "This plugin implements CLI Proxy Plugin.";
        private const string publisherCompany = "Wistron";
        private const string publisherWebsite = "https://www.wistron.com";
        private const string publisherSupport = "This plugin implements CLI Proxy Plugin.";

        private IAgent _agent;

        private enum log_type
        {
            info = 0,
            error
        }

        private ICLIDisplay _CLIDisplay;
        private ICLIPeripherals _CLIPeripherals;
        private readonly object _pluginConditionLock_Display = new object();
        private readonly object _pluginConditionLock_Peripherals = new object();
        private PluginCondition _CLIDisplayPluginCondition;
        private PluginCondition _CLIPeripheralsPluginCondition;
        private const int TIMEOUT_IN_SECONDS = 60;

        private ICliManagerSA _CliManagerPlugin;
        private IDeviceManagerSA _DevManagerPlugin;
        private readonly object _PluginConditionLock_CliManager = new object();
        private readonly object _PluginConditionLock_DevManager = new object();
        private bool relay_registered = false;

        #endregion

        #region Constructor

        public CLIProxyPlugin(IAgent agent) : base(agent, PluginLogId)
        {
            _agent = agent;
        }

        #endregion

        #region Overriding methods

        protected override void OnPluginStarting()
        {
            _agent.PluginManager.PluginsStarted += PluginManagerOnPluginsStarted;

            PluginCondition = new PluginStartedCondition();
            WriteLog("Cli Proxy plugin report started");

            InitializeDevManagerPlugin();
            InitializeCLIDisplayPlugin();
            InitializeCLIPeripheralsPlugin();

            InitializeCliManagerPlugin();
        }

        #endregion

        #region IDisposableObservable Support

        /// <summary>
        /// To detect redundant calls
        /// </summary>
        public bool IsDisposed { get; private set; }

        /// <summary>
        /// Override for Dispose
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            WriteLog($"Dispose: {disposing}");
            if (!IsDisposed)
            {
                if (disposing)
                {
                    _agent.PluginManager.PluginsStarted -= PluginManagerOnPluginsStarted;
                    _agent = null;

                    if (_DevManagerPlugin != null && _CliManagerPlugin != null)
                    {
                        _CliManagerPlugin.CLIActionEvent -= _CliManagerPlugin_CLIActionEvent;
                        relay_registered = false;
                    }
                }

                IsDisposed = true;
            }
            base.Dispose(disposing);
        }

        #endregion

        #region Event Handler

        private void PluginManagerOnPluginsStarted(object sender, PluginsStartedEventArgs e)
        {
            if (e == null)
                return;
            if (e.ChangedPlugins == null)
                return;
            if (e.ChangedPlugins.Any() == false)
                return;

            if (e.ChangedPlugins.OfType<ICliProxy>().Any())
            {
                WriteLog("ICliProxy plugin started.");
            }

            if (e.ChangedPlugins.OfType<ICliManagerSA>().Any())
                InitializeCliManagerPlugin();

            if (e.ChangedPlugins.OfType<IDeviceManagerSA>().Any())
                InitializeDevManagerPlugin();

            if (e.ChangedPlugins.OfType<ICLIDisplay>().Any())
                InitializeCLIDisplayPlugin();

            if (e.ChangedPlugins.OfType<ICLIPeripherals>().Any())
                InitializeCLIPeripheralsPlugin();
        }

        #endregion

        #region Private methods

        /// <summary>
        /// //
        /// </summary>
        /// <param name="text"></param>
        /// <param name="log_type">0 means info, others means error</param>
        private void WriteLog(string text, log_type log_type = log_type.info)
        {
            text = "[CLIProxyPlugin] " + text;
            Console.WriteLine(text);
            if (log_type == log_type.info)
                Log.Info(text);
            else
                Log.Error(text);
        }

        private void InitializeCliManagerPlugin()
        {
            if (_CliManagerPlugin != null)
                return;

            _CliManagerPlugin = _agent.PluginManager.FindPluginByType<ICliManagerSA>(PluginResolution.Dynamic);

            if (_CliManagerPlugin is IFrameworkPluginConditionNotification pluginCondition)
            {
                pluginCondition.PluginConditionChangeHandler += OnCliManagerPluginConditionChangeHandler;
                GetCurrentCliManagerPluginCondition();
            }
        }

        private void InitializeDevManagerPlugin()
        {
            if (_DevManagerPlugin != null)
                return;

            _DevManagerPlugin = _agent.PluginManager.FindPluginByType<IDeviceManagerSA>(PluginResolution.Dynamic);

            if (_DevManagerPlugin is IFrameworkPluginConditionNotification pluginCondition)
            {
                pluginCondition.PluginConditionChangeHandler += OnDevManagerPluginConditionChangeHandler;
                GetCurrentDevManagerPluginCondition();
            }
        }

        private void InitializeCLIDisplayPlugin()
        {
            if (_CLIDisplay != null)
                return;

            WriteLog($"{nameof(_CLIDisplay)} arrived for {nameof(ICLIDisplay)}");

            _CLIDisplay = _agent.PluginManager.FindPluginByType<ICLIDisplay>(PluginResolution.Dynamic);
            if (_CLIDisplay is IFrameworkPluginConditionNotification condition)
            {
                condition.PluginConditionChangeHandler += OnCLIDisplayPluginConditionChangeHandler;
                GetCurrentCLIDisplayPluginCondition();
            }
        }

        private void InitializeCLIPeripheralsPlugin()
        {
            if (_CLIPeripherals != null)
                return;

            WriteLog($"{nameof(_CLIPeripherals)} arrived for {nameof(ICLIPeripherals)}");

            _CLIPeripherals = _agent.PluginManager.FindPluginByType<ICLIPeripherals>(PluginResolution.Dynamic);
            if (_CLIPeripherals is IFrameworkPluginConditionNotification condition)
            {
                condition.PluginConditionChangeHandler += OnCLIPeripheralsPluginConditionChangeHandler;
                GetCurrentCLIPeripheralsPluginCondition();
            }
        }

        private void OnCliManagerPluginConditionChangeHandler(object sender, EventArgs e)
        {
            GetCurrentCliManagerPluginCondition();
        }

        private void OnDevManagerPluginConditionChangeHandler(object sender, EventArgs e)
        {
            GetCurrentDevManagerPluginCondition();
        }

        private void OnCLIDisplayPluginConditionChangeHandler(object sender, EventArgs e)
        {
            InitializeCLIDisplayPlugin();
        }

        private void OnCLIPeripheralsPluginConditionChangeHandler(object sender, EventArgs e)
        {
            InitializeCLIPeripheralsPlugin();
        }

        private void GetCurrentCliManagerPluginCondition()
        {
            _ = Task.Run(async () =>
            {
                var pluginCondition = await (_CliManagerPlugin as IFrameworkPluginConditionNotification)?.CurrentConditionAsync();

                lock (_PluginConditionLock_CliManager)
                {
                    if (pluginCondition is PluginErrorCondition)
                    {
                        WriteLog($"{nameof(GetCurrentCliManagerPluginCondition)} - CliManager Plugin is in an error condition");
                    }
                    else if (pluginCondition is PluginRunningCondition || pluginCondition is PluginStartedCondition)
                    {
                        WriteLog($"{nameof(GetCurrentCliManagerPluginCondition)} - CliManager Plugin is in a running/started condition");
                        if (!relay_registered && _DevManagerPlugin != null)
                        {
                            DoRelayRegister();
                        }
                    }
                }
            });
        }

        private void GetCurrentDevManagerPluginCondition()
        {
            _ = Task.Run(async () =>
            {
                var pluginCondition = await (_DevManagerPlugin as IFrameworkPluginConditionNotification)?.CurrentConditionAsync();

                lock (_PluginConditionLock_DevManager)
                {
                    if (pluginCondition is PluginErrorCondition)
                    {
                        WriteLog($"{nameof(GetCurrentDevManagerPluginCondition)} - DeviceManager Plugin is in an error condition");
                    }
                    else if (pluginCondition is PluginRunningCondition || pluginCondition is PluginStartedCondition)
                    {
                        WriteLog($"{nameof(GetCurrentDevManagerPluginCondition)} - DeviceManager Plugin is in a running/started condition");
                        if (!relay_registered && _CliManagerPlugin != null)
                        {
                            DoRelayRegister();
                        }
                    }
                }
            });
        }

        private void GetCurrentCLIDisplayPluginCondition()
        {
            _ = Task.Run(async () =>
            {
                var pluginCondition = await (_CLIDisplay as IFrameworkPluginConditionNotification)?.CurrentConditionAsync();

                lock (_pluginConditionLock_Display)
                {
                    _CLIDisplayPluginCondition = pluginCondition;

                    if (pluginCondition is PluginErrorCondition)
                    {
                        WriteLog($"{nameof(GetCurrentCLIDisplayPluginCondition)} - CLIDisplay Plugin is in an error condition");
                    }
                    else if (pluginCondition is PluginRunningCondition)
                    {
                        WriteLog($"{nameof(GetCurrentCLIDisplayPluginCondition)} - CLIDisplay Plugin is in running condition");
                        //_PluginAvailabilityTrigger_Display.Set();
                    }
                    else if (pluginCondition is PluginStartedCondition)
                    {
                        WriteLog($"{nameof(GetCurrentCLIDisplayPluginCondition)} - CLIDisplay Plugin is in started condition");
                        //_PluginAvailabilityTrigger_Display.Set();
                    }
                }
            });
        }

        private void GetCurrentCLIPeripheralsPluginCondition()
        {
            _ = Task.Run(async () =>
            {
                var pluginCondition = await (_CLIPeripherals as IFrameworkPluginConditionNotification)?.CurrentConditionAsync();

                lock (_pluginConditionLock_Peripherals)
                {
                    _CLIPeripheralsPluginCondition = pluginCondition;

                    if (pluginCondition is PluginErrorCondition)
                    {
                        WriteLog($"{nameof(GetCurrentCLIPeripheralsPluginCondition)} - CLIPeripherals Plugin is in an error condition");
                    }
                    else if (pluginCondition is PluginRunningCondition)
                    {
                        WriteLog($"{nameof(GetCurrentCLIPeripheralsPluginCondition)} - CLIPeripherals Plugin is in running condition");
                        //_PluginAvailabilityTrigger_Peripherals.Set();
                    }
                    else if (pluginCondition is PluginStartedCondition)
                    {
                        WriteLog($"{nameof(GetCurrentCLIPeripheralsPluginCondition)} - CLIPeripherals Plugin is in started condition");
                        //_PluginAvailabilityTrigger_Peripherals.Set();
                    }
                }
            });
        }

        private void DoRelayRegister()
        {
            if (_DevManagerPlugin == null || _CliManagerPlugin == null)
            {
                WriteLog("One of Device Manager and CLI Manager is null, do not register CLI relay");
                return;
            }
            _CliManagerPlugin.CLIActionEvent += _CliManagerPlugin_CLIActionEvent;
            relay_registered = true;
        }

        private void _CliManagerPlugin_CLIActionEvent(object? sender, CLIEventArgs e)
        {
            _ = Task.Run(() =>
            {
                if (_DevManagerPlugin == null)
                {
                    WriteLog("[Command line event] null Device Manager object!");
                    return;
                }

                if (e == null || e == EventArgs.Empty || e.commandLineInput == null)
                {
                    WriteLog("[Command line event] Got Empty CLIEventArgs!");
                    //_CliManagerPlugin.WriteCommandResult(Response_EmptyEventArg()); //no guid caused occupy memory
                    return;
                }

                //please edit upper table "peripheral_DeviceType" if new device supported
                List<string> peripheral_DeviceType = new List<string>()
                {
                    "MOUSE",
                    "KEYBOARD",
                    "AUDIO",
                    "PEN",
                    "DOCK",
                    "WEBCAM",
                };
                List<string> Display_Lock_WithoutAction = new List<string>()
                {
                    "INAPPBRICONT",
                    "INAPPAUTOBRITEMP",
                    "INAPPAUTOBRIGHTNESSCOLOR",//1004 InAppAutoBrightnessColor DDPMW1341, same as INAPPAUTOBRITEMP
                    "INAPPNETWORKKVM",
                    "INAPPCOLORPRESET",
                    //"POWERNAP", //do not add powernap here, go throw normal process via CLI Display plugin as well
                };

                //Do command line action
                CLIEventResult? cliEventResult;
                CommandLineInput commandLineInput = e.commandLineInput;
                if (commandLineInput.PluginsType.Equals("DISPLAY"))
                {
                    if (_CLIDisplay != null)
                    {
                        if (Display_Lock_WithoutAction.FindIndex(x => x.Equals(commandLineInput.TargetFeature)) >= 0)
                        {
                            DDPMSettings data_inappdisplaylock = _DevManagerPlugin.ReloadAppConfigData().Result;
                            cliEventResult = CLIHandlerDisplay.CLI_Display_LockUnlock(Log, data_inappdisplaylock, _DevManagerPlugin, commandLineInput, e.command_guid_string);
                        }
                        else
                            cliEventResult = _CLIDisplay.SetCommandArgs(e, _DevManagerPlugin);
                    }
                    else
                    {
                        WriteLog($"{nameof(ICLIDisplay)} was missing.");
                        _CliManagerPlugin.WriteCommandResult(Response_PluginNotReady(commandLineInput, nameof(ICLIDisplay), e.command_guid_string));
                        return;
                    }
                }
                //else if (commandLineInput.PluginsType.Equals("AUDIO") || commandLineInput.PluginsType.Equals("MOUSE") || commandLineInput.PluginsType.Equals("KEYBOARD") || commandLineInput.PluginsType.Equals("DOCK") || commandLineInput.PluginsType.Equals("HEADSET"))
                else if (peripheral_DeviceType.FindIndex(x => x.Equals(commandLineInput.PluginsType)) >= 0)
                {
                    if (_CLIPeripherals != null)
                    {
                        if (commandLineInput.TargetFeature.Equals("RESTOREFACTORYDEFAULTS"))
                        {
                            DDPMSettings data_restorefactorydefault = _DevManagerPlugin.ReloadAppConfigData().Result;
                            cliEventResult = CLIHandlerPeripheral.CLI_Peripheral_RestoreFactoryDefault(Log, data_restorefactorydefault, _DevManagerPlugin, commandLineInput, e.command_guid_string);
                        }
                        else
                            cliEventResult = _CLIPeripherals.SetCommandArgs(e, _DevManagerPlugin);
                    }
                    else
                    {
                        WriteLog($"{nameof(ICLIPeripherals)} was missing.");
                        _CliManagerPlugin.WriteCommandResult(Response_PluginNotReady(commandLineInput, nameof(ICLIPeripherals), e.command_guid_string));
                        return;
                    }
                }
                else if (commandLineInput.PluginsType.Equals("APP"))
                {
                    switch (commandLineInput.TargetFeature)
                    {
                        case "TELEMETRYCONSENT":
                            //cliEventResult = CLI_Analytics_Consent(commandLineInput, e.command_guid_string);
                            DDPMSettings data = _DevManagerPlugin.ReloadAppConfigData().Result;
                            cliEventResult = CLIHandlerApp.CLI_Analytics_Consent(Log, data, _DevManagerPlugin, commandLineInput, e.command_guid_string);
                            if (cliEventResult.ExitCode == (int)CLI_ExitCode.success)
                            {
                                UpdateUINotify no = new UpdateUINotify();
                                no.UI_Field_Name = "TELEMETRYCONSENT";
                                _DevManagerPlugin.OnUIUpdateNotify(no);
                            }
                            break;

                        case "INAPPUPDATE":
                            //cliEventResult = CLI_Analytics_Consent(commandLineInput, e.command_guid_string);
                            DDPMSettings data_update = _DevManagerPlugin.ReloadAppConfigData().Result;
                            cliEventResult = CLIHandlerApp.CLI_App_LockUnlock(Log, data_update, _DevManagerPlugin, commandLineInput, e.command_guid_string);
                            //if (cliEventResult.ExitCode == (int)CLI_ExitCode.success)
                            //{
                            //UpdateUINotify no = new UpdateUINotify();
                            //no.UI_Field_Name = "INAPPUPDATE";
                            //_DevManagerPlugin.OnUIUpdateNotify(no);
                            //}
                            break;
                        case "INAPPEXPORTSETTINGS":
                            //cliEventResult = CLI_Analytics_Consent(commandLineInput, e.command_guid_string);
                            DDPMSettings data_exportsettings = _DevManagerPlugin.ReloadAppConfigData().Result;
                            cliEventResult = CLIHandlerDisplay.CLI_Display_LockUnlock(Log, data_exportsettings, _DevManagerPlugin, commandLineInput, e.command_guid_string);
                            break;

                        case "INAPPRESTOREDEFAULTS":
                            //cliEventResult = CLI_Analytics_Consent(commandLineInput, e.command_guid_string);
                            DDPMSettings data_restoredefaults = _DevManagerPlugin.ReloadAppConfigData().Result;
                            cliEventResult = CLIHandlerApp.CLI_App_LockUnlock(Log, data_restoredefaults, _DevManagerPlugin, commandLineInput, e.command_guid_string);
                            break;
                        case "DEVICEDATA":
                        case "DEVICECONFIGURATION":
                        case "CONNECTEDDEVICES":
                        case "SCREENNOTIFICATION":
                        case "DIAGNOSTICSREPORT":
                            //cliEventResult = CLI_Analytics_Consent(commandLineInput, e.command_guid_string);
                            cliEventResult = _CLIDisplay.SetCommandArgs(e, _DevManagerPlugin);
                            break;
                        case "FIRMWAREUPDATE":
                            cliEventResult = _CLIPeripherals.SetCommandArgs(e, _DevManagerPlugin);

                            // add @ stephen
                            DDPMSettings data_fwupdate = _DevManagerPlugin.ReloadAppConfigData().Result;
                            cliEventResult = CLIHandlerApp.CLI_FW_Update(Log, data_fwupdate, _DevManagerPlugin, commandLineInput, e.command_guid_string);
                            break;
                        case "DISABLECA":
                            cliEventResult = CLIHandlerApp.CLI_Common_DisableCA(Log, _DevManagerPlugin, commandLineInput, e.command_guid_string);
                            break;

                        default:
                            _CliManagerPlugin.WriteCommandResult(Response_TargetFeatureNotSupport(commandLineInput, e.command_guid_string));
                            return;
                    }
                }
                else
                {
                    CLIEventResult result = new CLIEventResult()
                    {
                        command_guid_string = e.command_guid_string,
                        serialize_Json_response = Response_TargetTypeNotSupport(commandLineInput, commandLineInput.PluginsType),
                        ExitCode = (int)CLI_ExitCode.command_targettype_not_support,
                        ticket = DateTime.Now
                    };
                    _CliManagerPlugin.WriteCommandResult(result);
                    return;
                }

                //write result back
                if (cliEventResult != null)
                    _CliManagerPlugin.WriteCommandResult(cliEventResult);
            });
        }
        #endregion

        #region ICLIProxy implementation

        //no action need in this plugin

        #endregion
    }
}