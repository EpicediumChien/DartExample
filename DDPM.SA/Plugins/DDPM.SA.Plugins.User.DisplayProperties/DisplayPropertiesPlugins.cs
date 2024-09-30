using DDPM.SA.Common;
using Dell.Client.Framework.Common.Annotations;
using Dell.Client.Framework.Common.Extensions;
using Dell.Client.Framework.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
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
    public class DisplayPropertiesPlugins : BaseAgentPlugin, IDisplayProperties
    {
        #region Private Members

        private const string pluginName = "DisplayPropertiesPlugin";
        private const string pluginVersion = "1.0.0";
        private const string pluginDescription = "This plugin implements Display Properties Plugin.";
        private const string publisherCompany = "Wistron";
        private const string publisherWebsite = "https://www.wistron.com";
        private const string publisherSupport = "This plugin implements Display Properties Plugin.";

        private IAgent _agent;

        #endregion Private Members

        private DisplayPropertiesInfo _displayPropertiesInfo;
        public const string PluginLogId = "DisplayProperties";
        public static Logs? _logs;

        /// <summary>
        /// HDR變更事件，回傳HDR狀態
        /// </summary>
        public event EventHandler<bool>? HDRChangeEvent;

        public DisplayPropertiesPlugins(IAgent agent) : base(agent, PluginLogId)
        {
            _agent = agent;
            _logs = new Logs(Log, PluginLogId);
            _displayPropertiesInfo = new DisplayPropertiesInfo();
        }

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
        public Task<DisplaySupportedProperties> GetDisplaySupportedProperties(MonitorInfo monitorInfo)
        {
            try
            {
                _logs?.DebugMsg_1(nameof(GetDisplaySupportedProperties) + " start");
                DisplayPropertiesInfo displayPropertiesInfo = new DisplayPropertiesInfo();
                HDRSetting hDRSetting = new HDRSetting();
                Properties currentProperties = new Properties();
                displayPropertiesInfo.DisplayName = monitorInfo.DisplayName;
                if (!GetCurrentDisplaySetting(displayPropertiesInfo.DisplayName, out currentProperties, out displayPropertiesInfo.CurrentOrientation, monitorInfo.modelName))
                {
                    return Task.FromResult(new DisplaySupportedProperties());
                }
                displayPropertiesInfo.SupportedProperties.Properties = GetSupportedResolutions(monitorInfo, currentProperties, displayPropertiesInfo.CurrentOrientation);
                displayPropertiesInfo.SupportedProperties.Orientations = new DisplayOrientation[4]
                {
                    DisplayOrientation.Angle0,DisplayOrientation.Angle90,DisplayOrientation.Angle180,DisplayOrientation.Angle270
                };
                _displayPropertiesInfo = (displayPropertiesInfo);
                _logs?.DebugMsg_1(nameof(GetDisplaySupportedProperties) + " done");
                return Task.FromResult(_displayPropertiesInfo.SupportedProperties);
            }
            catch (Exception)
            {
                return Task.FromResult(new DisplaySupportedProperties());
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
                        result = _ChangeDisplaySettingsEx(DisplayName, ref devMode, IntPtr.Zero, ChangeDisplaySettingsFlags.CDS_TEST, IntPtr.Zero);
                        _logs?.DebugMsg_1($"{nameof(_ChangeDisplaySettingsEx)} test set devMode param: {DisplayName} :{devMode.dmPelsWidth}x{devMode.dmPelsHeight} Orientation:{((DisplayOrientation)devMode.dmDisplayOrientation).ToString()}");
                        _logs?.DebugMsg_1($"{nameof(_ChangeDisplaySettingsEx)} test set result:{result} try count:{retryCount}");
                        if (result != DISP_CHANGE_SUCCESSFUL)
                        {
                            if (retryCount == 1)
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
                            else if (retryCount == 3)
                            {
                                int temp = devMode.dmPelsWidth;
                                devMode.dmPelsWidth = devMode.dmPelsHeight;
                                devMode.dmPelsHeight = temp;
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
            HDRSetting hDRSetting = new HDRSetting();
            bool HDRStatus = false;
            hDRSetting.GetWindowsHDRStatus(monitorEdid, out HDRStatus);
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
            try
            {
                HDRSetting hDRSetting = new HDRSetting();
                bool result = hDRSetting.SetWindowsHDRStatus(monitorEdid, onoff);
                if (result)
                {
                    HDRChangeEvent?.AsyncFireAndForget(this, onoff, System.Threading.CancellationToken.None);
                }
                return Task.FromResult(result);
            }
            catch
            {
                return Task.FromResult(false);
            }
        }

        public void SetExtendMode(MonitorInfo info)
        {
            List<MonitorInfo> monitorInfos = _GetMonitors();
            List<MonitorInfo> monitorInfos1 = monitorInfos.FindAll(o => o.DisplayName == info.DisplayName);
            if (monitorInfos1.Count >= 2)
            {
                _SetDisplayConfig(0, IntPtr.Zero, 0, IntPtr.Zero, (uint)(SetDisplayConfigFlags.SDC_APPLY | SetDisplayConfigFlags.SDC_TOPOLOGY_EXTEND));
                Thread.Sleep(1000);
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
            try
            {
                _logs?.DebugMsg_1(nameof(RefreshDisplayPropertiesInfo) + " start");
                _displayPropertiesInfo = new DisplayPropertiesInfo();
                {
                    DisplayPropertiesInfo displayPropertiesInfo = new DisplayPropertiesInfo();
                    HDRSetting hDRSetting = new HDRSetting();
                    Properties currentProperties = new Properties();
                    displayPropertiesInfo.DisplayName = monitorInfo.DisplayName;
                    if (!GetCurrentDisplaySetting(displayPropertiesInfo.DisplayName, out currentProperties, out displayPropertiesInfo.CurrentOrientation, monitorInfo.modelName))
                    {
                        return false;
                    }
                    displayPropertiesInfo.SupportedHDR = isSupportedHDR;
                    hDRSetting.GetWindowsHDRStatus(monitorInfo.edid, out displayPropertiesInfo.isHDREnable);
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
                }
                _logs?.DebugMsg_1(nameof(RefreshDisplayPropertiesInfo) + " done");
                return true;
            }
            catch (Exception)
            {
                return false;
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
                if (!(devMode.dmPelsWidth >= 800 && devMode.dmPelsHeight >= 600) &&
                    !(devMode.dmPelsWidth >= 600 && devMode.dmPelsHeight >= 800))
                {
                    i++;
                    continue;
                }
                Properties resolution = new Properties
                {
                    Resolutions_Width = devMode.dmPelsWidth,
                    Resolutions_High = devMode.dmPelsHeight,
                    Frequency = devMode.dmDisplayFrequency
                };
                if (!resolutions.ContainsKey((resolution.Resolutions_Width, resolution.Resolutions_High, resolution.Frequency)))
                {
                    if (!found_Recommended)
                    {
                        if (GetOptimalScreenResolution(monitorInfo, resolution, currentOrientation))
                        {
                            resolution.isRecommended = true;
                            found_Recommended = true;
                        }
                    }
                    if (!found_Current)
                    {
                        if (resolution.Equals(currentProperties))
                        {
                            resolution.isCurrent = true;
                            found_Current = true;
                        }
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
        private bool GetCurrentDisplaySetting(string DisplayName, out Properties properties, out DisplayOrientation displayOrientation, string ModelName)
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