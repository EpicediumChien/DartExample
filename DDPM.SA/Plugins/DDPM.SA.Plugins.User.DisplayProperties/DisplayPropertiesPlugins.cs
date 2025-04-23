using DDPM.SA.Common;
using Dell.Client.Framework.Common;
using Dell.Client.Framework.Common.Annotations;
using Dell.Client.Framework.Common.Extensions;
using Dell.Client.Framework.Interfaces;
using Microsoft;
using MS.WindowsAPICodePack.Internal;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using VcpCore.Common;
using static VcpCore.Common.dxva2;
using static VcpCore.Common.User32;
using IDs = DDPM.SA.Common.IDs;

namespace DDPM.SA.Plugins.User.DisplayProperties
{
    [Plugin(IDs.DisplayProperties_PLUGIN_ID, pluginName, PluginOrderGroupType.Core, Version = pluginVersion)]
    [Descriptor(Description = pluginDescription)]
    [Publisher(Name = publisherCompany, Website = publisherWebsite, Support = publisherSupport)]
    [PublishedUnelevatedInterface(new[] { typeof(IDisplayProperties) })]
    [DependencyKnownTypes(new[] { typeof(IDisplayProperties) })]
    public class DisplayPropertiesPlugins : BaseAgentPlugin, IDisposableObservable, IDisplayProperties
    {
        #region Private Members

        private const string pluginName = "DisplayPropertiesPlugin";
        private const string pluginVersion = "1.0.0";
        private const string pluginDescription = "This plugin implements Display Properties Plugin.";
        private const string publisherCompany = "Dell Technologies";
        private const string publisherWebsite = "https://www.dell.com";
        private const string publisherSupport = "This plugin implements Display Properties Plugin.";

        private IAgent _agent;

        #endregion Private Members

        private DisplayPropertiesInfo _displayPropertiesInfo;
        public const string PluginLogId = "DisplayProperties";
        private static Logs? _logs;

        /// <summary>
        /// HDR變更事件，回傳HDR狀態
        /// </summary>
        public event EventHandler<bool>? HDRChangeEvent;

        public DisplayPropertiesPlugins(IAgent agent) : base(agent, PluginLogId)
        {
            _agent = agent;
            _agent.PluginManager.PluginsStarted += PluginManagerOnPluginsStarted;
            _logs = new Logs(Log, PluginLogId);
            _displayPropertiesInfo = new DisplayPropertiesInfo();
        }
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
#if DEBUG
            Console.WriteLine($"Dispose: {disposing}");
#endif
            if (!IsDisposed)
            {
                IsDisposed = true;
                if (disposing)
                {
                    _agent.PluginManager.PluginsStarted -= PluginManagerOnPluginsStarted;
                    _agent = null;
                }
            }
            base.Dispose(disposing);
        }

        #endregion IDisposableObservable Support
        #region Event Handler

        private void PluginManagerOnPluginsStarted(object sender, PluginsStartedEventArgs e)
        {
            if (e == null)
                return;
            if (e.ChangedPlugins == null)
                return;
            if (e.ChangedPlugins.Any() == false)
                return;

            if (e.ChangedPlugins.OfType<IDisplayProperties>().Any())
            {
#if DEBUG
                Console.WriteLine("IDisplayProperties plugin started.");
#endif
            }
        }

        #endregion Event Handler
        /// <summary>
        /// 取得螢幕屬性(現在解析度、刷新率,螢幕所支援的解析度,HDR狀態, USBCPrioritization狀態)
        /// </summary>
        /// <param name="monitorInfo">螢幕資訊</param>
        /// <param name="s">傳入VCP的字串，用於解析是否支援HDR</param>
        /// <param name="USBCPrioritizationType">傳入USB-C Prioritization的狀態</param>
        /// <returns>回傳螢幕的所有屬性值</returns>
        public Task<DisplayPropertiesInfo> GetDisplayPropertiesInfo(MonitorInfo monitorInfo, string s, bool isSupportedHDR, bool isHDREnable, bool isSupportUSBCPrioritization, USBCPrioritizationType USBCPrioritizationType)
        {
            if (RefreshDisplayPropertiesInfo(monitorInfo, s, isSupportedHDR, isHDREnable, isSupportUSBCPrioritization, USBCPrioritizationType))
            {
                return Task.FromResult(_displayPropertiesInfo);
            }
            return Task.FromResult(new DisplayPropertiesInfo());
        }

        /// <summary>
        /// 取得螢幕所支援的解析度
        /// </summary>
        /// <param name="monitorInfo">螢幕資訊</param>
        /// <returns>支援的解析度列表</returns>
        public Task<DisplayPropertiesInfo> GetDisplaySupportedProperties(MonitorInfo monitorInfo)
        {
            using (HDRSetting hDRSetting = new HDRSetting())
            {
                try
                {
                    _logs?.DebugMsg_1($"{nameof(GetDisplaySupportedProperties)} start");
                    DisplayPropertiesInfo displayPropertiesInfo = new DisplayPropertiesInfo();
                    Properties currentProperties = new Properties();
                    displayPropertiesInfo.DisplayName = monitorInfo.DisplayName;
                    if (!GetCurrentDisplaySetting(displayPropertiesInfo.DisplayName, out currentProperties, out displayPropertiesInfo.CurrentOrientation))
                    {
                        return Task.FromResult(new DisplayPropertiesInfo());
                    }
                    if (JudgmentList.AutoRotateOSMonitorList.Contains(monitorInfo.modelName))
                    {
                        displayPropertiesInfo.CurrentOrientation = DisplayOrientation.Unknow;
                    }
                    displayPropertiesInfo.SupportedProperties.Properties = GetSupportedResolutions(monitorInfo, currentProperties, displayPropertiesInfo.CurrentOrientation);
                    displayPropertiesInfo.SupportedProperties.Orientations = new DisplayOrientation[4]
                    {
                    DisplayOrientation.Angle0,DisplayOrientation.Angle90,DisplayOrientation.Angle180,DisplayOrientation.Angle270
                    };
                    _displayPropertiesInfo = (displayPropertiesInfo);
                    _logs?.DebugMsg_1($"{nameof(GetDisplaySupportedProperties)} done");
                    return Task.FromResult(_displayPropertiesInfo);
                }
                catch (Exception ex)
                {
                    _logs?.DebugMsg_1($"{nameof(GetDisplaySupportedProperties)} error : {ex.Message}");
                    return Task.FromResult(new DisplayPropertiesInfo());
                }
            }
        }

        /// <summary>
        /// 取得螢幕方向
        /// </summary>
        /// <param name="DisplayName">要取得的螢幕</param>
        /// <returns>回傳值:畫面旋轉角</returns>
        public Task<DisplayOrientation> GetCurrentDisplayOrientation(string DisplayName)
        {
            DEVMODE devMode = new DEVMODE();
            if (_EnumDisplaySettings(DisplayName, ENUM_CURRENT_SETTINGS, ref devMode))
            {
                return Task.FromResult((DisplayOrientation)devMode.dmDisplayOrientation);
            }
            return Task.FromResult(DisplayOrientation.Unknow);
        }
        public Task<DisplayCurrentPropertiesInfo> GetCurrentDisplayProperties(MonitorInfo monitorInfo)
        {
            DisplayCurrentPropertiesInfo ret = new DisplayCurrentPropertiesInfo();
            GetCurrentDisplaySetting(monitorInfo.DisplayName, out ret.CurrentProperties, out ret.CurrentOrientation);
            return Task.FromResult(ret);
        }

        /// <summary>
        /// 設定螢幕的解析度和畫面旋轉角
        /// </summary>
        /// <param name="DisplayName">要設定的螢幕</param>
        /// <param name="width">要設定的解析度寬</param>
        /// <param name="height">要設定的解析度高</param>
        /// <param name="orientation">要設定的畫面旋轉角</param>
        public Task<bool> SetDisplayPropertiest(string DisplayName, Properties properties, DisplayOrientation orientation)
        {
            int result = DISP_CHANGE_BADMODE;
            try
            {
                _logs?.DebugMsg_1(nameof(RefreshDisplayPropertiesInfo) + " start");
                _logs?.DebugMsg_1($"Setting param: {DisplayName}:{properties.Resolutions_Width}x{properties.Resolutions_High} Orientation:{orientation.ToString()}");
                DEVMODE devMode = new DEVMODE();
                devMode.dmSize = (short)Marshal.SizeOf(typeof(DEVMODE));
                if (_EnumDisplaySettings(DisplayName, ENUM_CURRENT_SETTINGS, ref devMode))
                {
                    if ((int)orientation >= 0)
                    {
                        devMode.dmDisplayOrientation = (int)orientation;
                    }
                    if (properties.Resolutions_Width > 0 && properties.Resolutions_High > 0)
                    {
                        devMode.dmPelsWidth = properties.Resolutions_Width;
                        devMode.dmPelsHeight = properties.Resolutions_High;
                    }
                    if (properties.Frequency > 0)
                    {
                        devMode.dmDisplayFrequency = properties.Frequency;
                    }
                    (devMode.dmPelsWidth, devMode.dmPelsHeight) = ConfirmResolution(devMode.dmPelsWidth, devMode.dmPelsHeight, orientation);
                    int retryCount = 1;
                    do
                    {
                        if (IsDisposed)
                        {
                            _logs?.DebugMsg_1($"SetDisplayPropertiest IsDisposed");
                            break;
                        }
                        result = _ChangeDisplaySettingsEx(DisplayName, ref devMode, IntPtr.Zero, ChangeDisplaySettingsFlags.CDS_TEST, IntPtr.Zero);
                        _logs?.DebugMsg_1($"{nameof(_ChangeDisplaySettingsEx)} test set devMode param: {DisplayName} :{devMode.dmPelsWidth}x{devMode.dmPelsHeight} Orientation:{((DisplayOrientation)devMode.dmDisplayOrientation).ToString()}");
                        _logs?.DebugMsg_1($"{nameof(_ChangeDisplaySettingsEx)} test set result:{result} try count:{retryCount}");
                        if (result != DISP_CHANGE_SUCCESSFUL)
                        {
                            devMode.dmDisplayFrequency = GetMaxRefreshRateByWidthAndHeight(DisplayName, devMode.dmPelsWidth, devMode.dmPelsHeight);
                        }
                        retryCount++;
                    } while (result != DISP_CHANGE_SUCCESSFUL && retryCount <= 2);
                    result = _ChangeDisplaySettingsEx(DisplayName, ref devMode, IntPtr.Zero, ChangeDisplaySettingsFlags.CDS_UPDATEREGISTRY, IntPtr.Zero);
                    _logs?.DebugMsg_1($"{nameof(_ChangeDisplaySettingsEx)} set result:{result}");
                    _logs?.DebugMsg_1($"devMode param: {DisplayName}:{devMode.dmPelsWidth}x{devMode.dmPelsHeight} Orientation:{((DisplayOrientation)devMode.dmDisplayOrientation).ToString()}");
                    _logs?.DebugMsg_1(nameof(RefreshDisplayPropertiesInfo) + " done");
                }
            }
            catch
            {
            }
            if (result == DISP_CHANGE_SUCCESSFUL)
            {
                return Task.FromResult(true);
            }
            else
            {
                return Task.FromResult(false);
            }
        }
        public Task<bool> SetResolutions(string DisplayName, Properties properties)
        {
            int result = DISP_CHANGE_BADMODE;
            try
            {
                _logs?.DebugMsg_1(nameof(SetResolutions) + " start");
                _logs?.DebugMsg_1($"Setting param:  DN:{DisplayName}:{properties.Resolutions_Width}x{properties.Resolutions_High}");
                DEVMODE devMode = new DEVMODE();
                devMode.dmSize = (short)Marshal.SizeOf(typeof(DEVMODE));
                if (_EnumDisplaySettings(DisplayName, ENUM_CURRENT_SETTINGS, ref devMode))
                {
                    _logs?.DebugMsg_1($"Current setting param:{DisplayName}--{devMode.dmPelsWidth}x{devMode.dmPelsHeight} Orientation:{((DisplayOrientation)devMode.dmDisplayOrientation).ToString()}");
                    if (properties.Resolutions_Width > 0 && properties.Resolutions_High > 0)
                    {
                        devMode.dmPelsWidth = properties.Resolutions_Width;
                        devMode.dmPelsHeight = properties.Resolutions_High;
                    }
                    if (properties.Frequency > 0)
                    {
                        devMode.dmDisplayFrequency = properties.Frequency;
                    }
                    (devMode.dmPelsWidth, devMode.dmPelsHeight) = ConfirmResolution(devMode.dmPelsWidth, devMode.dmPelsHeight, (DisplayOrientation)devMode.dmDisplayOrientation);
                    int retryCount = 1;
                    do
                    {
                        if (IsDisposed)
                        {
                            _logs?.DebugMsg_1($"SetResolutions IsDisposed");
                            break;
                        }
                        result = _ChangeDisplaySettingsEx(DisplayName, ref devMode, IntPtr.Zero, ChangeDisplaySettingsFlags.CDS_TEST, IntPtr.Zero);
                        _logs?.DebugMsg_1($"{nameof(_ChangeDisplaySettingsEx)} test set devMode param: {DisplayName} :{devMode.dmPelsWidth}x{devMode.dmPelsHeight} Orientation:{((DisplayOrientation)devMode.dmDisplayOrientation).ToString()}");
                        _logs?.DebugMsg_1($"{nameof(_ChangeDisplaySettingsEx)} test set result:{result} try count:{retryCount}");
                        if (result != DISP_CHANGE_SUCCESSFUL)
                        {
                            int temp = devMode.dmPelsWidth;
                            devMode.dmPelsWidth = devMode.dmPelsHeight;
                            devMode.dmPelsHeight = temp;
                            devMode.dmDisplayFrequency = GetMaxRefreshRateByWidthAndHeight(DisplayName, devMode.dmPelsWidth, devMode.dmPelsHeight);
                        }
                        retryCount++;
                    } while (result != DISP_CHANGE_SUCCESSFUL && retryCount <= 2);
                    result = _ChangeDisplaySettingsEx(DisplayName, ref devMode, IntPtr.Zero, ChangeDisplaySettingsFlags.CDS_UPDATEREGISTRY, IntPtr.Zero);
                    _logs?.DebugMsg_1($"{nameof(_ChangeDisplaySettingsEx)} set result:{result}");
                    _logs?.DebugMsg_1($"devMode param: {DisplayName}:{devMode.dmPelsWidth}x{devMode.dmPelsHeight} Orientation:{((DisplayOrientation)devMode.dmDisplayOrientation).ToString()}");
                    _logs?.DebugMsg_1(nameof(SetResolutions) + " done");
                }
                else
                {
                }
            }
            catch
            {
            }
            if (result == DISP_CHANGE_SUCCESSFUL)
            {
                return Task.FromResult(true);
            }
            else
            {
                return Task.FromResult(false);
            }
        }

        public Task<bool> SetOrientation_New(string DisplayName, DisplayOrientation Orientation)
        {
            int result = DISP_CHANGE_BADMODE;

            try
            {
                _logs?.DebugMsg_1("SetOrientation_New start");
                _logs?.DebugMsg_1($"     Setting param:  DN:{DisplayName} Orientation:{Orientation.ToString()}");
                DEVMODE devMode = new DEVMODE();
                devMode.dmSize = (short)Marshal.SizeOf(typeof(DEVMODE));
                if (_EnumDisplaySettings(DisplayName, ENUM_CURRENT_SETTINGS, ref devMode))
                {
                    if ((int)(devMode.dmDisplayOrientation + Orientation) % 2 == 1)
                    {
                        int dmPelsHeight = devMode.dmPelsHeight;
                        devMode.dmPelsHeight = devMode.dmPelsWidth;
                        devMode.dmPelsWidth = dmPelsHeight;
                    }
                    switch (Orientation)
                    {
                        case DisplayOrientation.Angle90:
                            devMode.dmDisplayOrientation = 1;//3;
                            break;
                        case DisplayOrientation.Angle180:
                            devMode.dmDisplayOrientation = 2;
                            break;
                        case DisplayOrientation.Angle270:
                            devMode.dmDisplayOrientation = 3;// 1;
                            break;
                        case DisplayOrientation.Angle0:
                            devMode.dmDisplayOrientation = 0;
                            break;
                    }

                    result = _ChangeDisplaySettingsEx(DisplayName, ref devMode, IntPtr.Zero, ChangeDisplaySettingsFlags.CDS_UPDATEREGISTRY, IntPtr.Zero);
                }
                else
                {
                    long errorCode = _GetLastError();
                    _logs?.DebugMsg_1($"     EnumDisplaySettings error. 0x{errorCode:X}");
                }
            }
            catch (Exception ex)
            {
                _logs?.DebugMsg_1($"     SetOrientation_New, got exception: {ex.ToString()}"); ;
            }

            _logs?.DebugMsg_1($"SetOrientation_New, End...(result:{result})");
            if (result == DISP_CHANGE_SUCCESSFUL)
            {
                return Task.FromResult(true);
            }
            else
            {
                return Task.FromResult(false);
            }
        }

        public Task<bool> SetOrientation(string DisplayName, DisplayOrientation orientation)
        {
            int result = DISP_CHANGE_BADMODE;
            try
            {
                _logs?.DebugMsg_1(nameof(SetOrientation) + " start");
                _logs?.DebugMsg_1($"Setting param:  DN:{DisplayName} Orientation:{orientation.ToString()}");
                DEVMODE devMode = new DEVMODE();
                devMode.dmSize = (short)Marshal.SizeOf(typeof(DEVMODE));
                if (_EnumDisplaySettings(DisplayName, ENUM_CURRENT_SETTINGS, ref devMode))
                {
                    _logs?.DebugMsg_1($"Current setting param:{DisplayName}--{devMode.dmPelsWidth}x{devMode.dmPelsHeight} Orientation:{((DisplayOrientation)devMode.dmDisplayOrientation).ToString()}");
                    if ((int)orientation >= 0)
                    {
                        devMode.dmDisplayOrientation = (int)orientation;
                    }
                    int retryCount = 1;
                    do
                    {
                        if (IsDisposed)
                        {
                            _logs?.DebugMsg_1($"SetOrientation IsDisposed");
                            break;
                        }
                        result = _ChangeDisplaySettingsEx(DisplayName, ref devMode, IntPtr.Zero, ChangeDisplaySettingsFlags.CDS_TEST, IntPtr.Zero);
                        _logs?.DebugMsg_1($"{nameof(_ChangeDisplaySettingsEx)} test set devMode param: {DisplayName} :{devMode.dmPelsWidth}x{devMode.dmPelsHeight} Orientation:{((DisplayOrientation)devMode.dmDisplayOrientation).ToString()}");
                        _logs?.DebugMsg_1($"{nameof(_ChangeDisplaySettingsEx)} test set result:{result} try count:{retryCount}");
                        if (result != DISP_CHANGE_SUCCESSFUL)
                        {
                            if (retryCount == 1 || retryCount == 3)
                            {
                                int temp = devMode.dmPelsWidth;
                                devMode.dmPelsWidth = devMode.dmPelsHeight;
                                devMode.dmPelsHeight = temp;
                                devMode.dmDisplayFrequency = GetMaxRefreshRateByWidthAndHeight(DisplayName, devMode.dmPelsWidth, devMode.dmPelsHeight);
                            }
                            else if (retryCount == 2)
                            {
                                (devMode.dmPelsWidth, devMode.dmPelsHeight) = GetBestResolution(DisplayName, orientation);
                                devMode.dmDisplayFrequency = GetMaxRefreshRateByWidthAndHeight(DisplayName, devMode.dmPelsWidth, devMode.dmPelsHeight);
                            }
                        }
                        retryCount++;
                    } while (result != DISP_CHANGE_SUCCESSFUL && retryCount <= 3);
                    result = _ChangeDisplaySettingsEx(DisplayName, ref devMode, IntPtr.Zero, ChangeDisplaySettingsFlags.CDS_UPDATEREGISTRY, IntPtr.Zero);
                    _logs?.DebugMsg_1($"{nameof(_ChangeDisplaySettingsEx)} set result:{result}");
                    _logs?.DebugMsg_1($"devMode param: {DisplayName}:{devMode.dmPelsWidth}x{devMode.dmPelsHeight} Orientation:{((DisplayOrientation)devMode.dmDisplayOrientation).ToString()}");
                    _logs?.DebugMsg_1(nameof(SetOrientation) + " done");
                }
                else
                {
                }
            }
            catch (Exception ex)
            {
                _logs?.DebugMsg_1($"{nameof(SetOrientation)}, got exception: {ex.ToString()}");
            }

            if (result == DISP_CHANGE_SUCCESSFUL)
            {
                return Task.FromResult(true);
            }
            else
            {
                return Task.FromResult(false);
            }
        }
        /// <summary>
        /// 呼叫windows的顯示器設定畫面
        /// </summary>
        /// <returns>是否正常執行呼叫</returns>
        public Task<bool> CallWindowsDisplaySetting()
        {
            try
            {
                Process.Start("control", "desk.cpl");
                return Task.FromResult(true);
            }
            catch
            {
                return Task.FromResult(false);
            }
        }

        /// <summary>
        /// Get monitor HDR status
        /// </summary>
        /// <param name="monitorEdid">monitor's edid</param>
        /// <returns>true is HDR on; false is HDR off</returns>
        public Task<bool> GetHDRStatus(EDID monitorEdid)
        {
            _logs?.DebugMsg_1($"{nameof(GetHDRStatus)} start");
            bool HDRStatus = false;
            using (HDRSetting hDRSetting = new HDRSetting())
            {
                hDRSetting.GetWindowsHDRStatus(_logs, monitorEdid, out HDRStatus);
            }
            _logs?.DebugMsg_1($"{nameof(GetHDRStatus)} HDRStatus : {HDRStatus}");
            _logs?.DebugMsg_1($"{nameof(GetHDRStatus)} done");
            return Task.FromResult(HDRStatus);
        }

        /// <summary>
        /// 設定螢幕的HDR狀態
        /// </summary>
        /// <param name="monitorEdid">螢幕的edid</param>
        /// <param name="onoff">是否啟用</param>
        /// <returns></returns>
        public Task<bool> SetHDRStatus(EDID monitorEdid, bool onoff)
        {
            _logs?.DebugMsg_1($"{nameof(SetHDRStatus)} start");
            bool ret = false;
            try
            {
                using (HDRSetting hDRSetting = new HDRSetting())
                {
                    _logs?.DebugMsg_1($"{nameof(SetHDRStatus)} SetWindowsHDRStatus go");
                    ret = hDRSetting.SetWindowsHDRStatus(_logs, monitorEdid, onoff);
                    if (ret)
                    {
                        HDRChangeEvent?.AsyncFireAndForget(this, onoff, System.Threading.CancellationToken.None);
                    }
                }
            }
            catch (Exception ex)
            {
                _logs?.DebugMsg_1($"{nameof(SetHDRStatus)} Error : {ex.Message}");
            }
            _logs?.DebugMsg_1($"{nameof(SetHDRStatus)} ret : {ret}");
            _logs?.DebugMsg_1($"{nameof(SetHDRStatus)} done");
            return Task.FromResult(ret);
        }

        public void SetExtendMode(MonitorInfo monitorInfo)
        {
            List<MonitorInfo> monitorInfos = _GetMonitors();
            List<MonitorInfo> monitorInfos1 = monitorInfos.FindAll(o => o.DisplayName == monitorInfo.DisplayName);
            if (monitorInfos1.Count >= 2)
            {
                _SetDisplayConfig(0, IntPtr.Zero, 0, IntPtr.Zero, (uint)(SetDisplayConfigFlags.SDC_APPLY | SetDisplayConfigFlags.SDC_TOPOLOGY_EXTEND));
                Task.Delay(1000).Wait();
            }
        }

        private List<MonitorInfo> _GetMonitors()
        {
            try
            {
                List<MonitorInfo> monitors = new List<MonitorInfo>();

                int MoIndexCounter = 0;

                bool _Get_Monitors(IntPtr hMonitor, IntPtr hdcMonitor, ref Rectangle lprcMonitor, IntPtr dwData)
                {
                    try
                    {
                        _logs?.DebugMsg_1("_Get_Monitors collection start ...");

                        System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
                        watch.Start();
                        var info = new MonitorInfoEx();
                        _GetMonitorInfo(new HandleRef(null, hMonitor), info);
                        string DeviceName = new string(info.szDevice).Trim('\0');
                        //----
                        uint cPhysicalMonitors = 0;
                        bool bSuccess = _GetNumberOfPhysicalMonitorsFromHMONITOR(hMonitor, ref cPhysicalMonitors);
                        PHYSICAL_MONITOR[] pPhysicalMonitors = new PHYSICAL_MONITOR[cPhysicalMonitors];
                        bSuccess = _GetPhysicalMonitorsFromHMONITOR(hMonitor, cPhysicalMonitors, pPhysicalMonitors);
                        DISPLAY_DEVICE dd = new DISPLAY_DEVICE();
                        dd.cb = Marshal.SizeOf(dd);
                        //----
                        int realindex = -1;
                        for (int jj = 0; _EnumDisplayDevices(DeviceName, (uint)jj, ref dd, 0); jj++)
                        {
                            if (IsDisposed)
                            {
                                _logs?.DebugMsg_1($"_GetMonitors IsDisposed");
                                break;
                            }
                            if ((dd.StateFlags & DisplayDeviceStateFlags.AttachedToDesktop) == 0)
                                continue;

                            realindex++;

                            DEVMODE devmode = new DEVMODE();
                            bool success = _EnumDisplaySettings(DeviceName, ENUM_CURRENT_SETTINGS, ref devmode);
                            MonitorInfo _TargetMonitor = new MonitorInfo();
                            _TargetMonitor.DisplayName = DeviceName;
                            if (!string.IsNullOrWhiteSpace(_TargetMonitor.DisplayName))
                                monitors.Add(_TargetMonitor);
                            MoIndexCounter++;
                        }
                        watch.Stop();
                        return true;
                    }
                    catch (Exception ex)
                    {
                        _logs?.DebugMsg_1("_Get_Monitors collection exception : " + ex.Message);
                        return false;
                    }
                }

                //MonitorEnumDelegate lpfnEnum1 = _Get_Monitors;

                if ((!_EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero, _Get_Monitors, IntPtr.Zero)))
                    throw new Win32Exception(Marshal.GetLastWin32Error());

                _logs?.DebugMsg_1("_GetMonitors() ... done");
                return monitors;
            }
            catch (Exception e)
            {
                _logs?.DebugMsg_1("_GetMonitors() happened exception : " + e.Message);
                List<MonitorInfo_complex> monitors = new List<MonitorInfo_complex>();
                //monitors.Clear(); //Dean 0626 fix SAST issue, remove this line since the object just created and it's empty
                return new List<MonitorInfo>();
            }
        }
        /// <summary>
        /// 刷新螢幕屬性
        /// </summary>
        /// <param name="monitorInfo"></param>
        /// <param name="s"></param>
        /// <param name="isSupportedHDR"></param>
        /// <param name="isHDREnable"></param>
        /// <param name="isSupportUSBCPrioritization"></param>
        /// <param name="USBCPrioritizationType"></param>
        /// <returns></returns>
        private bool RefreshDisplayPropertiesInfo(MonitorInfo monitorInfo, string s, bool isSupportedHDR, bool isHDREnable, bool isSupportUSBCPrioritization, USBCPrioritizationType USBCPrioritizationType)
        {
            //Bruce 0605 修改註記:因讀取時間過長(約5000mS)，故修改軟體目前降至(約2800mS)
            HDRSetting hDRSetting = new HDRSetting();

            try
            {
                _logs?.DebugMsg_1(nameof(RefreshDisplayPropertiesInfo) + " start");
                _displayPropertiesInfo = new DisplayPropertiesInfo();
                {
                    DisplayPropertiesInfo displayPropertiesInfo = new DisplayPropertiesInfo();
                    Properties currentProperties = new Properties();
                    displayPropertiesInfo.DisplayName = monitorInfo.DisplayName;
                    if (!GetCurrentDisplaySetting(displayPropertiesInfo.DisplayName, out currentProperties, out displayPropertiesInfo.CurrentOrientation))
                    {
                        return false;
                    }
                    if (JudgmentList.AutoRotateOSMonitorList.Contains(monitorInfo.modelName))
                    {
                        displayPropertiesInfo.CurrentOrientation = DisplayOrientation.Unknow;
                    }
                    displayPropertiesInfo.SupportedHDR = isSupportedHDR;
                    hDRSetting.GetWindowsHDRStatus(_logs, monitorInfo.edid, out displayPropertiesInfo.isHDREnable);
                    /*if (!displayPropertiesInfo.isHDREnable)
                    {
                        if (!hDRSetting.SetWindowsHDRStatus(monitorInfo.edid, false))
                        {
                            displayPropertiesInfo.SupportedHDR = false;
                        }
                    }
                    else
                    {
                        if (!isHDREnable)
                        {
                            displayPropertiesInfo.isHDREnable = false;
                        }
                    }*/
                    displayPropertiesInfo.SupportedUSBCPrioritization = isSupportUSBCPrioritization;
                    displayPropertiesInfo.USBCPrioritizationType = USBCPrioritizationType;
                    displayPropertiesInfo.SupportedProperties.Properties = GetSupportedResolutions(monitorInfo, currentProperties, displayPropertiesInfo.CurrentOrientation);
                    displayPropertiesInfo.SupportedProperties.Orientations = new DisplayOrientation[4]
                    {
                    DisplayOrientation.Angle0,DisplayOrientation.Angle90,DisplayOrientation.Angle180,DisplayOrientation.Angle270
                    };
                    _displayPropertiesInfo = (displayPropertiesInfo);
                    _logs?.DebugMsg_1("displayPropertiesInfo.SupportedHDR:" + displayPropertiesInfo.SupportedHDR);
                }
                _logs?.DebugMsg_1(nameof(RefreshDisplayPropertiesInfo) + " done");
                return true;
            }
            catch (Exception)
            {
                return false;
            }
            finally
            {
                hDRSetting.Dispose();
            }
        }
        /// <summary>
        /// 取得螢幕的所有可支援的解析度
        /// </summary>
        /// <param name="monitorInfo">螢幕資訊</param>
        /// <param name="currentProperties">現在的螢幕解析度屬性</param>
        /// <param name="currentOrientation">現在的螢幕畫面方向</param>
        /// <returns></returns>
        private List<Properties> GetSupportedResolutions(MonitorInfo monitorInfo, Properties currentProperties, DisplayOrientation currentOrientation)
        {
            SortedList<(int, int, int), Properties> resolutions = new SortedList<(int, int, int), Properties>(Comparer<(int, int, int)>.Create((x, y) =>
            {
                if (x.Item1 != y.Item1)
                    return y.Item1.CompareTo(x.Item1); // Width descending
                if (x.Item2 != y.Item2)
                    return y.Item2.CompareTo(x.Item2); // Height descending
                return y.Item3.CompareTo(x.Item3); // Frequency descending
            }));
            DEVMODE devMode = new DEVMODE();
            int i = 0;
            bool found_Current = false, found_Recommended = false;
            while (_EnumDisplaySettings(monitorInfo.DisplayName, i, ref devMode))
            {
                if (IsDisposed)
                {
                    _logs?.DebugMsg_1($"GetSupportedResolutions IsDisposed");
                    break;
                }
                Properties resolution = new Properties
                {
                    Resolutions_Width = devMode.dmPelsWidth,
                    Resolutions_High = devMode.dmPelsHeight,
                    Frequency = devMode.dmDisplayFrequency
                };
                if (!resolutions.ContainsKey((resolution.Resolutions_Width, resolution.Resolutions_High, resolution.Frequency)))
                {
                    if (!found_Recommended &&
                        GetOptimalScreenResolution(monitorInfo, resolution, currentOrientation))
                    {
                        resolution.isRecommended = true;
                        found_Recommended = true;
                    }
                    if (!found_Current &&
                        resolution.Equals(currentProperties))
                    {
                        resolution.isCurrent = true;
                        found_Current = true;
                    }
                    resolution.BitsPerPixel = currentProperties.BitsPerPixel;
                    resolutions.Add((resolution.Resolutions_Width, resolution.Resolutions_High, resolution.Frequency), resolution);
                }
                i++;
            }
            return resolutions.Values.ToList();
        }
        /// <summary>
        /// 取得螢幕現在屬性
        /// </summary>
        /// <param name="DisplayName">要取得的螢幕</param>
        /// <param name="Resolution">回傳值:解析度</param>
        /// <param name="displayOrientation">回傳值:畫面旋轉角</param>
        /// <returns>是否成功取得</returns>
        private bool GetCurrentDisplaySetting(string DisplayName, out Properties properties, out DisplayOrientation displayOrientation)
        {
            DEVMODE devMode = new DEVMODE();
            if (_EnumDisplaySettings(DisplayName, ENUM_CURRENT_SETTINGS, ref devMode))
            {
                properties = new Properties()
                {
                    Resolutions_Width = devMode.dmPelsWidth,
                    Resolutions_High = devMode.dmPelsHeight,
                    Frequency = devMode.dmDisplayFrequency,
                    BitsPerPixel = devMode.dmBitsPerPel
                };
                displayOrientation = (DisplayOrientation)devMode.dmDisplayOrientation;
                return true;
            }
            properties = new Properties();
            displayOrientation = DisplayOrientation.Unknow;
            return false;
        }
        /// <summary>
        /// 比對傳入的螢幕屬性並比對，用於找出建議解析度、頻率
        /// </summary>
        /// <param name="monitorInfo">要取得的螢幕資訊</param>
        /// <param name="Properties">要比對的螢幕參數</param>
        /// <param name="displayOrientation">現在螢幕的畫面方向，因涉及寬高的數值</param>
        /// <returns>如果比對成功則回傳true代表傳入的螢幕參數是建議值，否則false</returns>
        private bool GetOptimalScreenResolution(MonitorInfo monitorInfo, Properties Properties, DisplayOrientation displayOrientation)
        {
            var (Width, Height) = GetBestResolution(monitorInfo.DisplayName, displayOrientation);
            int integerFrequency = GetMaxRefreshRateByWidthAndHeight(monitorInfo.DisplayName, Width, Height);
            if (Properties.Equals(new Properties()
            {
                Resolutions_Width = Width,
                Resolutions_High = Height,
                Frequency = integerFrequency
            }))
            {
                return true;
            }
            return false;
        }
        /// <summary>
        /// 取得螢幕在特定解析度下的最高刷新率，用於找到建議值
        /// </summary>
        /// <param name="deviceName">要計算的螢幕</param>
        /// <param name="dispWidth">要計算的螢幕的解析度寬</param>
        /// <param name="dispHeight">要計算的螢幕的解析度高</param>
        /// /// <returns>回傳該螢幕該解析度下最高的刷新率值</returns>
        private int GetMaxRefreshRateByWidthAndHeight(string deviceName, int dispWidth, int dispHeight)
        {
            int refreshRate = 0;
            DEVMODE deviceMode = new DEVMODE();
            for (int i = 0; _EnumDisplaySettings(deviceName, i, ref deviceMode) != false; i++)
            {
                if (IsDisposed)
                {
                    _logs?.DebugMsg_1($"GetMaxRefreshRateByWidthAndHeight IsDisposed");
                    break;
                }
                if (deviceMode.dmDisplayFrequency >= refreshRate &&
                    (deviceMode.dmPelsWidth == dispWidth && deviceMode.dmPelsHeight == dispHeight))
                {
                    refreshRate = deviceMode.dmDisplayFrequency;
                }
            }
            return refreshRate;
        }
        /// <summary>
        /// 取得螢幕在特定畫面方向的最佳解析度，用於找到建議值
        /// </summary>
        /// <param name="displayName"></param>
        /// <param name="orientation"></param>
        /// <returns></returns>
        private (int Width, int Height) GetBestResolution(string displayName, DisplayOrientation orientation)
        {
            var bestResolution = (0, 0);
            DEVMODE dm = new DEVMODE();
            dm.dmSize = (short)Marshal.SizeOf(dm);
            int modeIndex = 0;
            List<(int Width, int Height)> resolutions = new List<(int Width, int Height)>();
            while (_EnumDisplaySettings(displayName, modeIndex, ref dm))
            {
                if (IsDisposed)
                {
                    _logs?.DebugMsg_1($"GetBestResolution IsDisposed");
                    break;
                }
                resolutions.Add((dm.dmPelsWidth, dm.dmPelsHeight));
                modeIndex++;
            }
            // Find the best resolution based on area
            if (resolutions.Count > 0)
            {
                bestResolution = resolutions.OrderByDescending(r => r.Width).FirstOrDefault();
            }
            return bestResolution;
        }
        private (int W, int H) ConfirmResolution(int w, int h, DisplayOrientation orientation)
        {
            switch (orientation)
            {
                case DisplayOrientation.Angle90:
                case DisplayOrientation.Angle270:
                    if (w > h)
                    {
                        return (h, w);
                    }
                    return (w, h);
                case DisplayOrientation.Angle180:
                default:
                    if (w < h)
                    {
                        return (h, w);
                    }
                    return (w, h);
            }
        }
    }
}