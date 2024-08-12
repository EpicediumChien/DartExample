#region LicenceHeader
//
// Copyright © 2024, Dell Inc., All Rights Reserved.
// This material is confidential and a trade secret.  Permission to use this
// work for any purpose must be obtained in writing from Dell Inc.
//
// DisplayMangerPlugin.cs created on 24/04/2024T11:20 AM
//
#endregion

using Microsoft;
using System.Threading.Tasks;
using VcpCore.Common;
using System;
using System.Linq;
using System.Collections.Generic;
using Dell.Client.Framework.Common;
using Dell.Client.Framework.Common.Annotations;
using Dell.Client.Framework.Common.PluginConditions;
using Dell.Client.Framework.Interfaces;
using VcpCore.Interfaces;
using Newtonsoft.Json.Linq;
using DDPM.SA.Common;
using IDs = DDPM.SA.Common.IDs;
using System.Globalization;
using DDPM.SA.Common.Display;

namespace DDPM.SA.Plugins.User.PipPbpManger
{
    [Plugin(IDs.PipPbp_Manager_PLUGIN_ID, pluginName, PluginOrderGroupType.Core, Version = pluginVersion)]
    [Descriptor(Description = pluginDescription)]
    [Publisher(Name = publisherCompany, Website = publisherWebsite, Support = publisherSupport)]
    [PublishedUnelevatedInterface(new[] { typeof(IPipPbpService) })]
    //[PublishedInterface(new[] { typeof(IPipPbpService) })]
    [DependencyKnownTypes(new[] { typeof(IDisplayService) })]
    [PluginRequires(Id = IDs.Display_Manager_PLUGIN_ID, Version = "1.0.0", AllowDynamicResolving = true)]

    public class PipPbpMangerPlugin : BaseAgentPlugin, IDisposableObservable, IPipPbpService
    {
        #region Private Members
        private const string pluginName = "PipPbpMangerPlugin";
        private const string pluginVersion = "1.0.0";
        private const string pluginDescription = "This plugin implements PIP PBP Manager Plugin.";
        private const string publisherCompany = "Wistron";
        private const string publisherWebsite = "https://www.wistron.com";
        private const string publisherSupport = "This plugin implements PIP PBP Manager Plugin.";

        private bool _IsAdministrator = ProcessSecurityHelperWrapper.IsCurrentProcessRunningElevated();

        private IAgent _agent;
        private const string PluginLogId = "PipPbpManger";

        private Logs _logs;
        private IDisplayService _DisplayManagerPlugin;
        private PluginCondition _DisplayManagerPluginCondition;
        private bool _DisplayManagerPluginUsable = false;

        private readonly object _PluginConditionLock = new object();

        //The error string when the public method return error
        private string _lastError = "";
        #endregion

        #region Public Members
        public event EventHandler<VCPchangedEventArgs> VCPchanged;
        #endregion

        #region Constructor
        public PipPbpMangerPlugin(IAgent agent) : base(agent, PluginLogId)
        {
            _agent = agent;
            _IsAdministrator = ProcessSecurityHelperWrapper.IsCurrentProcessRunningElevated();
            _logs = new Logs(Log);
            //_logs.DebugMsg("[PipPbpMangerPlugin] Does PipPbpMangerPlugin have Administrator: " + _IsAdministrator.ToString());
        }
        #endregion

        #region Overriding methods
        protected override void OnPluginStarting()
        {
            PluginCondition = new PluginStartedCondition();
            _agent.PluginManager.PluginsStarted += PluginManagerOnPluginsStarted;
            InitializeDisplayManagerPlugin();
        }
        #endregion

        #region PIP Mode Code for VCP code 0xE9
        const UInt16 PipMode_Off = 0;
        const UInt16 PipMode_Small = 0x21;
        const UInt16 PipMode_Large = 0x22;

        const UInt16 PipMode_SizeToggle = 0x01;
        const UInt16 PipMode_PositionToggle = 0x02;
        #endregion

        #region IPipPbpService implementation
        public string LastError { get => _lastError; }

        /// <summary>
        /// Get the Capabilities string of PIP/PBP function.
        /// </summary>
        /// <param name="monitorInfo"></param>
        /// <returns>
        /// For example, original capability string: "(prot(monitor)type(LCD)...E9(00 01 02 21 22 24 ) EA..."
        /// then the return string will be "00 01 02 21 22 24 "    all characters between '(' and ')' 
        /// </returns>
        public Task<string> GetCapabilitiesString(MonitorInfo monitorInfo)
        {
            if (_DisplayManagerPlugin != null)
            {
                string monitorCaps = _DisplayManagerPlugin.GetCapabilitiesString(monitorInfo).Result;
                if (String.IsNullOrEmpty(monitorCaps))
                {
                    _lastError = $"GetCapabilitiesString({monitorInfo.AliasDeviceName}): monitor capabilities string is empty.";
                    _logs.DebugMsg($"[{pluginName}] {_lastError}");
                    return Task.FromResult(String.Empty);

                }
                //Find the start index of "E9("
                string signature = "E9(";
                int idxSignature = monitorCaps.IndexOf(signature);
                if (idxSignature < 0) //Not found, will return String.Empty
                {
                    _lastError = $"GetCapabilitiesString({monitorInfo.AliasDeviceName}): PIP/PBP capabilities (E9) not found.";
                    _logs.DebugMsg($"[{pluginName}] {_lastError}");
                    return Task.FromResult(String.Empty);
                }
                int idxPipPbpCapsStart = idxSignature + signature.Length;
                //Find the index of next ')' char
                int idxEnd = monitorCaps.IndexOf(')', idxPipPbpCapsStart);
                if (idxEnd < 0)
                {
                    _lastError = $"GetCapabilitiesString({monitorInfo.AliasDeviceName}): End of capabilties char ')' not found.";
                    _logs.DebugMsg($"[{pluginName}] {_lastError}");
                    return Task.FromResult(String.Empty);
                }
                int pipPbpCapsLen = idxEnd - idxPipPbpCapsStart;
                //Extract the sub string contains PIP/PBP capabilities
                string retString = monitorCaps.Substring(idxPipPbpCapsStart, pipPbpCapsLen);
                return Task.FromResult(retString);
            }
            _lastError = $"GetCapabilitiesString({monitorInfo.AliasDeviceName}): DeviceManagerPlugin is null.";
            _logs.DebugMsg($"[{pluginName}] {_lastError}");
            return Task.FromResult(String.Empty);
        }

        /// <summary>
        /// Get the PIP/PBP capabilities in WORD array format.
        /// </summary>
        /// <param name="monitorInfo"></param>
        /// <returns>WORD array contains capabilities; or an empty array if error.</returns>
        public Task<UInt16[]> GetCapabilitiesWords(MonitorInfo monitorInfo)
        {
            string pipPbpCapsStr = GetCapabilitiesString(monitorInfo).Result;
            if (String.IsNullOrEmpty(pipPbpCapsStr))
                return Task.FromResult(new UInt16[0]);
            UInt16[] caps = ParsingHexStringToWords(pipPbpCapsStr);
            if (caps == null)
            {
                _lastError = $"Invalid format in CapabilitiesString: {pipPbpCapsStr}";
                return Task.FromResult<UInt16[]>(null);
            }
            if (caps.Length == 0)
            {
                _lastError = $"CapabilitiesString is empty or no any valid values.";
            }
            return Task.FromResult(caps);
        }

        /// <summary>
        ///Parsing hex value blank separated string to a WORD array
        ///Support format:
        ///1 All Bytes: "02 04 05 08 10 12" 
        ///2 All Words: "0002 0004 0105 0208 1006 AE12"
        ///3 Mix: "02 0208 04 05 AE12"
        ///4 Multiple space chars: "  02 04  05     08 10 1006    12" 
        /// </summary>
        /// <param name="inStr"></param>
        /// <returns>
        /// null : Invalid format in inStr
        /// empty : no any token found
        /// </returns>
        public static UInt16[] ParsingHexStringToWords(string inStr)
        {
            //1 Split into tokens with white space
            string[] tokens = inStr.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (tokens == null || tokens.Length == 0)
                return new UInt16[0];

            List<UInt16> words = new List<UInt16>();
            //2 for each token will convert to UInt16 integer value
            foreach (string tok in tokens)
            {
                UInt16 wValue;
                if (!UInt16.TryParse(tok, NumberStyles.HexNumber, CultureInfo.CurrentCulture, out wValue))
                {
                    return null;
                }
                words.Add(wValue);
            }
            return words.ToArray();
        }

        public Task<bool> SetPipModeOff(MonitorInfo monitorInfo)
        {
            if (_DisplayManagerPlugin != null)
            {
                return _DisplayManagerPlugin.SetVCPCapability(monitorInfo, 0xE9, PipMode_Off);
            }
            _lastError = $"SetPxpModeOff({monitorInfo.AliasDeviceName}): DeviceManagerPlugin is null.";
            _logs.DebugMsg($"[{pluginName}] {_lastError}");
            return Task.FromResult(false);
        }

        public Task<bool> SetPipModeSmall(MonitorInfo monitorInfo)
        {
            if (_DisplayManagerPlugin != null)
            {
                return _DisplayManagerPlugin.SetVCPCapability(monitorInfo, 0xE9, PipMode_Small);
            }
            _lastError = $"SetPxpModeSmall({monitorInfo.AliasDeviceName}): DeviceManagerPlugin is null.";
            _logs.DebugMsg($"[{pluginName}] {_lastError}");
            return Task.FromResult(false);
        }

        public Task<bool> SetPipModeLarge(MonitorInfo monitorInfo)
        {
            if (_DisplayManagerPlugin != null)
            {
                return _DisplayManagerPlugin.SetVCPCapability(monitorInfo, 0xE9, PipMode_Large);
            }
            _lastError = $"SetPxpModeLarge({monitorInfo.AliasDeviceName}): DeviceManagerPlugin is null.";
            _logs.DebugMsg($"[{pluginName}] {_lastError}");
            return Task.FromResult(false);
        }

        public Task<bool> TogglePipSize(MonitorInfo monitorInfo)
        {
            if (_DisplayManagerPlugin != null)
            {
                return _DisplayManagerPlugin.SetVCPCapability(monitorInfo, 0xE9, PipMode_SizeToggle);
            }
            _lastError = $"TogglePipSize({monitorInfo.AliasDeviceName}): DeviceManagerPlugin is null.";
            _logs.DebugMsg($"[{pluginName}] {_lastError}");
            return Task.FromResult(false);
        }

        public Task<bool> TogglePipPosition(MonitorInfo monitorInfo)
        {
            if (_DisplayManagerPlugin != null)
            {
                return _DisplayManagerPlugin.SetVCPCapability(monitorInfo, 0xE9, PipMode_PositionToggle);
            }
            _lastError = $"TogglePipPosition({monitorInfo.AliasDeviceName}): DeviceManagerPlugin is null.";
            _logs.DebugMsg($"[{pluginName}] {_lastError}");
            return Task.FromResult(false);
        }

        public Task<bool> SetPbpMode(MonitorInfo monitorInfo, UInt16 modeCode)
        {
            if (_DisplayManagerPlugin != null)
            {
                return _DisplayManagerPlugin.SetVCPCapability(monitorInfo, 0xE9, modeCode);
            }
            _lastError = $"SetPbpMode({monitorInfo.AliasDeviceName}): DeviceManagerPlugin is null.";
            _logs.DebugMsg($"[{pluginName}] {_lastError}");
            return Task.FromResult(false);
        }

        /// <summary>
        /// VideoSwap(x,y)
        /// x and y: 0=main, 1=sub1, 2=sub2, 3=sub3
        /// </summary>
        /// <param name="monitorInfo"></param>
        /// <param name="x">0=main, 1=sub1, 2=sub2, 3=sub3</param>
        /// <param name="y">0=main, 1=sub1, 2=sub2, 3=sub3</param>
        /// <returns></returns>

        public Task<bool> VideoSwap(MonitorInfo monitorInfo, UInt16 x, UInt16 y)
        {
            if (_DisplayManagerPlugin != null)
            {
                //Write value: 0xF0xy, x and y is 0=main, 1=sub1, 2=sub2, 3=sub3
                UInt16 wX = (UInt16)((x & 3) << 4);
                UInt16 wY = (UInt16)((y & 3));
                UInt16 wValue = (UInt16)(0xF000 | wX | wY);
                return _DisplayManagerPlugin.SetVCPCapability(monitorInfo, 0xE5, wValue);
            }
            _lastError = $"VideoSwap({monitorInfo.AliasDeviceName}): DeviceManagerPlugin is null.";
            _logs.DebugMsg($"[{pluginName}] {_lastError}");
            return Task.FromResult(false);
        }

        public Task<ObjGetVCP> GetPxpMode(MonitorInfo monitorInfo)
        {
            if (_DisplayManagerPlugin != null)
            {
                return _DisplayManagerPlugin.GetVCPCapability(monitorInfo, 0xE9);
            }
            _lastError = $"SetPbpMode({monitorInfo.AliasDeviceName}): DeviceManagerPlugin is null.";
            _logs.DebugMsg($"[{pluginName}] {_lastError}");
            return Task.FromResult<ObjGetVCP>(new ObjGetVCP() { result = false, value = 0xff });
        }

        public Task<List<UInt16>> GetSubInputList(MonitorInfo monitorInfo)
        {
            if (_DisplayManagerPlugin != null)
            {
                ObjGetVCP ret = _DisplayManagerPlugin.GetVCPCapability(monitorInfo, 0xE8).Result;
                if (ret.result)
                {
                    List<UInt16> listOut = new List<UInt16>();
                    UInt16 wValue = Convert.ToUInt16(ret.value);
                    //
                    // bit 15 14~10  9~5   4~0
                    //      x sub3   sub2  sub1
                    UInt16 sub1 = (UInt16)(wValue & 0x001F);
                    UInt16 sub2 = (UInt16)((wValue >> 5) & 0x001F);
                    UInt16 sub3 = (UInt16)((wValue >> 10) & 0x001F);
                    if (sub1 > 0)
                    {
                        //VcpCodeList.VCP60
                        listOut.Add(sub1);
                    }
                    if (sub2 > 0)
                    {
                        listOut.Add(sub2);
                    }
                    if (sub3 > 0)
                    {
                        listOut.Add(sub3);
                    }
                    return Task.FromResult<List<UInt16>>(listOut);
                }
            }
            return Task.FromResult<List<UInt16>>(null);
        }

        public Task<List<InputSourceObj>> GetSubInputs(MonitorInfo monitorInfo)
        {
            List<UInt16> subList = GetSubInputList(monitorInfo).Result;
            if (subList == null)
            {
                return Task.FromResult<List<InputSourceObj>>(null);
            }

            List<InputSourceObj> listOut = new List<InputSourceObj>();
            foreach (UInt16 code in subList)
            {
                listOut.Add(new InputSourceObj(code));
            }
            return Task.FromResult<List<InputSourceObj>>(listOut);
        }

        public Task<bool> SetSubInputs(MonitorInfo monitorInfo, InputSourceObj? sub1, InputSourceObj? sub2, InputSourceObj? sub3)
        {
            if (_DisplayManagerPlugin != null)
            {
                //Get original WORD
                ObjGetVCP ret = _DisplayManagerPlugin.GetVCPCapability(monitorInfo, 0xE8).Result;
                if (ret.result)
                {
                    List<UInt16> listOut = new List<UInt16>();
                    UInt16 wValue = Convert.ToUInt16(ret.value);
                    //
                    // bit 15 14~10  9~5   4~0
                    //      x sub3   sub2  sub1
                    UInt16 wSub1 = (UInt16)(wValue & 0x001F);
                    UInt16 wSub2 = (UInt16)((wValue >> 5) & 0x001F);
                    UInt16 wSub3 = (UInt16)((wValue >> 10) & 0x001F);

                    UInt16 wSetValue = 0;
                    if (sub1 != null)
                    {
                        UInt16 sub1Code = sub1.Code;
                        sub1Code &= 0x001F;
                        wSetValue |= sub1Code;
                    }
                    if (sub2 != null)
                    {
                        UInt16 sub2Code = (UInt16)(sub2.Code & 0x001F);
                        sub2Code = (UInt16)(sub2Code << 5);
                        wSetValue |= sub2Code;
                    }
                    if (sub3 != null)
                    {
                        UInt16 sub3Code = (UInt16)(sub3.Code & 0x001F);
                        sub3Code = (UInt16)(sub3Code << 10);
                        wSetValue |= sub3Code;
                    }

                    bool res = _DisplayManagerPlugin.SetVCPCapability(monitorInfo, 0xE8, wSetValue).Result;
                    return Task.FromResult<bool>(res);
                }
            }
            return Task.FromResult<bool>(false);
        }

        //Robert_Lin 2024-7-30 added, USB Select Switch, VCPE7, HHLL=FF0X whert X=target, 0=switch to Next, 1~4 specific the target
        /// <summary>
        /// USB Select Switch.
        /// </summary>
        /// <param name="monitorInfo">The monitor to be switched.</param>
        /// <param name="target">
        /// 0 (defualt) switch to Next USB port; 1~4 the USB port# to be swicthed.
        /// </param>
        /// <returns></returns>
        public Task<bool> UsbSwitch(MonitorInfo monitorInfo, UInt16 target=0)
        {
            if (target > 4)
                return Task.FromResult<bool>(false);
            if (_DisplayManagerPlugin != null)
            {
                //Prepase for the VCP Code Word
                // VCP 0xE7 writeValue=FF0x where is x=target
                UInt16 writeValue = (UInt16)(0xFF00 + target);
                return _DisplayManagerPlugin.SetVCPCapability(monitorInfo, (byte)0xE7, writeValue);
            }
            _lastError = $"VideoSwap({monitorInfo.AliasDeviceName}): DeviceManagerPlugin is null.";
            _logs.DebugMsg($"[{pluginName}] {_lastError}");
            return Task.FromResult(false);
        }
        #endregion

        #region Private Methods

        private void InitializeDisplayManagerPlugin()
        {
            if (_DisplayManagerPlugin != null)
                return;

            _DisplayManagerPlugin = _agent.PluginManager.FindPluginByType<IDisplayService>(PluginResolution.Dynamic);

            if (_DisplayManagerPlugin is IFrameworkPluginConditionNotification DisplayManagerCondition)
            {
                DisplayManagerCondition.PluginConditionChangeHandler += OnDisplayManagerPluginConditionChangeHandler;
                GetCurrentDisplayManagerPluginCondition();
            }
        }

        private void GetCurrentDisplayManagerPluginCondition()
        {
            _ = Task.Run(async () =>
            {
                var pluginCondition = await (_DisplayManagerPlugin as IFrameworkPluginConditionNotification)?.CurrentConditionAsync();

                lock (_PluginConditionLock)
                {
                    if (pluginCondition is PluginErrorCondition)
                    {
                        _logs.DebugMsg($"[PipPbpMangerPlugin] {nameof(GetCurrentDisplayManagerPluginCondition)} - Display ManagerPlugin is in an error condition");
                        _DisplayManagerPluginCondition = pluginCondition;
                        _DisplayManagerPluginUsable = false;
                    }
                    else if (pluginCondition is PluginStartedCondition)
                    {
                        _logs.DebugMsg($"[PipPbpMangerPlugin] {nameof(GetCurrentDisplayManagerPluginCondition)} -Display ManagerPlugin is in a started condition");
                        _DisplayManagerPluginCondition = pluginCondition;
                        _DisplayManagerPluginUsable = true;

                        //Test1();
                    }
                }
            });
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
            if (!IsDisposed)
            {
                if (disposing)
                {
                    _agent.PluginManager.PluginsStarted -= PluginManagerOnPluginsStarted;
                    _agent = null;
                }

                IsDisposed = true;
            }
            base.Dispose(disposing);
        }
        #endregion

        #region Event Handler
        private void OnDisplayManagerPluginConditionChangeHandler(object sender, EventArgs e)
        {
            GetCurrentDisplayManagerPluginCondition();
        }

        private void PluginManagerOnPluginsStarted(object sender, PluginsStartedEventArgs e)
        {
            if (e == null)
                return;
            if (e.ChangedPlugins == null)
                return;
            if (e.ChangedPlugins.Any() == false)
                return;
            if (!e.ChangedPlugins.OfType<IDisplayService>().Any()) return;

            InitializeDisplayManagerPlugin();
        }
        #endregion
    }
}
