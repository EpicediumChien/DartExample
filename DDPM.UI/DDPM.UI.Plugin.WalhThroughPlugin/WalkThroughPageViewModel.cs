using CommunityToolkit.Mvvm.ComponentModel;
using DDPM.SA.Common;
using DDPM.SA.Common.Settings;
using DDPM.UI.Common;
using DDPM.UI.Common.Models;
using Dell.Client.Framework.Common;
using Dell.Client.Framework.UX.WPF;
using Dell.Client.Framework.UX.WPF.Controls;
using System.Diagnostics;
using System.Windows;
using System.Windows.Media;
using VcpCore.Common;
using static DDPM.UI.WalkThroughData.WalkThroughData;

namespace DDPM.UI.Plugin.WalkThroughPlugin
{
    public class WalkThroughPageViewModel : ObservableObject
    {
        private List<HomeDevice> _homeDevices = new List<HomeDevice>();
        public int _currentTotalPage = 0;// Control button Visibility.Collapsed 
        public int _currentPageIndex = 0;
        public string _currentDeviceModel = string.Empty;
        private Dictionary<string, List<WalkThroughPageData>> _devicePages = DDPM.UI.WalkThroughData.WalkThroughData.GetDevicePages((int)DdpmCommonHelper.PreviousOsTheme);
        public object _currentDeviceinfo = string.Empty;
        public string last_logicalDeviceType = string.Empty;
        public MonitorInfo MInfo = new MonitorInfo();
        public DeviceInfo DInfo = new DeviceInfo();

        private bool _isDarkTheme;
        public bool IsDarkTheme
        {
            get => _isDarkTheme;
            private set => SetProperty(ref _isDarkTheme, value); // Use private set if only updated internally
        }

        public WalkThroughPageViewModel()
        {
            Debug.WriteLine($"Queue Count: {DdpmHomePlugin.DdpmHomePlugin.WalkThroughQueue.Count}");
            DdpmCommonHelper.WriteUILog($"[WalkThroughPageViewModel] WalkThroughPageViewModel ... in ");
            try
            {
                if (DdpmHomePlugin.DdpmHomePlugin.WalkThroughQueue.Exists(info => info.ModelName == "CONSENT_PAGE"))
                {
                    IsConsentPageVisible = true;
                }
                else if (DdpmHomePlugin.DdpmHomePlugin.WalkThroughQueue.Exists(info => info.ModelName == "DDPM"))
                {
                    SwitchToDDPMPage();
                }
                else
                {
                    IsConsentPageVisible = false;
                    IsPeripheralVisible = true;
                    //IsDDPMVisibility = false;
                }
                InitializeDeviceFromQueue();
                UpdateButtonVisibility();

                // Set initial theme state
                UpdateThemeState(DdpmCommonHelper.PreviousOsTheme);

                // Subscribe to theme changes
                DdpmCommonHelper.BitmapImageUpdated += HandleThemeChange;
            }
            catch (Exception ex)
            {
                DdpmCommonHelper.WriteUILog($"[WalkThroughPageViewModel] WalkThroughPageViewModel Exception: {ex.Message}");
            }
            DdpmCommonHelper.WriteUILog($"[WalkThroughPageViewModel] WalkThroughPageViewModel ... end ");
        }

        // Destructor or an Unload method to unsubscribe
        ~WalkThroughPageViewModel()
        {
            DdpmCommonHelper.BitmapImageUpdated -= HandleThemeChange;
            DdpmCommonHelper.WriteUILog($"[WalkThroughPageViewModel] Unsubscribed from theme changes.");
        }

        private void HandleThemeChange(OSThemeEnum newTheme)
        {
            DdpmCommonHelper.WriteUILog($"[WalkThroughPageViewModel] Theme changed to: {newTheme}");
            UpdateThemeState(newTheme);
        }

        private void UpdateThemeState(OSThemeEnum currentTheme)
        {
            // Assuming Dark = 1, Light = 0 or other values based on OSThemeEnum definition
            IsDarkTheme = (currentTheme == OSThemeEnum.Dark); 
            DdpmCommonHelper.WriteUILog($"[WalkThroughPageViewModel] IsDarkTheme set to: {IsDarkTheme}");
            // Note: SetProperty in IsDarkTheme setter already triggers OnPropertyChanged
        }

        public void InitializeDeviceFromQueue()
        {
            DdpmCommonHelper.WriteUILog($"[WalkThroughPageViewModel] InitializeDeviceFromQueue in ...");
            try
            {
                // Check WalkThroughQueue
                while (DdpmHomePlugin.DdpmHomePlugin.WalkThroughQueue.Count > 0)
                {
                    var device = DdpmHomePlugin.DdpmHomePlugin.WalkThroughQueue.First();

                    if (_devicePages.ContainsKey(device.ModelName))
                    {
                        if (device.ModelName == "DDPM") // App walkthrough show
                        {
                            ControlIcon(true, false);
                        }
                        else // Peripheral walkthrough not show
                        {
                            ControlIcon(false, false);
                        }
                        InitializeDevice(device.ModelName, device.DeviceInfo);
                        break;
                    }
                    else
                    {
                        UpdateLastlogicalDeviceType();
                        // If _devicePages No ModelNumber, remove and next 
                        if (DdpmHomePlugin.DdpmHomePlugin.WalkThroughQueue.Count != 0) // Error handling
                        {
                            WriteWalkThroughReg(DdpmHomePlugin.DdpmHomePlugin.WalkThroughQueue[0].ModelName);
                            DdpmHomePlugin.DdpmHomePlugin.WalkThroughEndList.Add(DdpmHomePlugin.DdpmHomePlugin.WalkThroughQueue[0]);
                            DdpmCommonHelper.WriteUILog($"[WalkThroughPageViewModel] InitializeDeviceFromQueue RemoveAt {DdpmHomePlugin.DdpmHomePlugin.WalkThroughQueue[0].ModelName}");
                            DdpmHomePlugin.DdpmHomePlugin.WalkThroughQueue.RemoveAt(0);
                        }
                    }
                }

                if (DdpmHomePlugin.DdpmHomePlugin.WalkThroughQueue.Count == 0)
                {
                    DdpmCommonHelper.WriteUILog($"[WalkThroughPageViewModel] InitializeDeviceFromQueue WalkThroughQueue.Count = 0 ... ");
                    EndWalkThrough();
                }
            }
            catch (Exception ex)
            {
                DdpmCommonHelper.WriteUILog($"[WalkThroughPageViewModel] InitializeDeviceFromQueue Exception: {ex.Message}");
            }
        }

        public void InitializeDevice(string deviceModel, object info)
        {
            // 檢查 deviceModel 是否為 null、空字串或只包含空白
            if (string.IsNullOrWhiteSpace(deviceModel))
            {
                DdpmCommonHelper.WriteUILog("[WalkThroughPageViewModel] InitializeDevice deviceModel is null or whitespace.");
                EndWalkThrough();
                return;
            }

            try
            {
                _currentDeviceModel = deviceModel;
                DdpmCommonHelper.WriteUILog($"[WalkThroughPageViewModel] InitializeDevice deviceModel '{deviceModel}' ...");

                // 依不同型別處理 
                switch (info)
                {
                    case DeviceInfo deviceInfo:
                        DInfo = deviceInfo;
                        _currentDeviceinfo = DInfo.ID;
                        DdpmCommonHelper.WriteUILog($"[WalkThroughPageViewModel] InitializeDevice DeviceInfo (ID = {_currentDeviceinfo}) ...");

                        // Kevin Add set default value for air audio device
                        switch (deviceModel)
                        {
                            case "SB725":
                            case "SL525":
                            case "SP325":
                                IsAirAudioDevice = true;
                                break;
                            default:
                                IsAirAudioDevice = false;
                                break;
                        }
                             
                        // << 250328 added by Hess to set default values
                        switch (DInfo.LogicalDeviceType)
                        {
                            case "LogicalWebcam":
                                DInfo.Message = "NEW";
                                WebcamSettings ws = new(DInfo, DdpmCommonHelper.DeviceManagerSA, DdpmCommonHelper.Log);
                                WebcamSettings.ExportWebcamSettings(ws, DInfo.ModelNumber, DdpmCommonHelper.DeviceManagerSA, DdpmCommonHelper.Log);
                                break;
                            default:
                                break;
                        }
                        // >>

                        // Init and set default statuses for AirAudio devices
                        if (IsAirAudioDevice)
                        {
                            DdpmCommonHelper.WriteUILog($"[WalkThroughPageViewModel] Initializing default audio states for {deviceModel}");
                            // Set ViewModel state directly first
                            _outgoingAudioStatus = true;
                            _incomingAudioStatus = false;
                            // Notify UI of the initial state
                            OnPropertyChanged(nameof(OutgoingAudioStatus));
                            OnPropertyChanged(nameof(OutgoingAudio_String));
                            OnPropertyChanged(nameof(IncomingAudioStatus));
                            OnPropertyChanged(nameof(IncomingAudio_String));

                            // Call device manager to set hardware state
                            var currentDeviceId = DInfo?.ID.ToString();
                            if (!string.IsNullOrEmpty(currentDeviceId) && DdpmCommonHelper.DeviceManagerSA != null)
                            {
                                DdpmCommonHelper.DeviceManagerSA.SetAirAudioMicNoiseCancellationAsync(currentDeviceId, true)
                                    .ContinueWith(t => { 
                                        if (t.IsFaulted) DdpmCommonHelper.WriteUILog($"[WalkThroughPageViewModel] Failed to set initial OutgoingAudioStatus: {t.Exception?.InnerException?.Message}");
                                        else DdpmCommonHelper.WriteUILog($"[WalkThroughPageViewModel] Initial SetAirAudioMicNoiseCancellationAsync called with true");
                                    });
                                DdpmCommonHelper.DeviceManagerSA.SetAirAudioMicNCIncomingAsync(currentDeviceId, false)
                                    .ContinueWith(t => { 
                                        if (t.IsFaulted) DdpmCommonHelper.WriteUILog($"[WalkThroughPageViewModel] Failed to set initial IncomingAudioStatus: {t.Exception?.InnerException?.Message}");
                                        else DdpmCommonHelper.WriteUILog($"[WalkThroughPageViewModel] Initial SetAirAudioMicNCIncomingAsync called with false");
                                    });

                                // Set default audio preset
                                DdpmCommonHelper.WriteUILog($"[WalkThroughPageViewModel] Setting initial audio preset to Default (1)");
                                SetAudioSelectedPresetInBackgroundAsync("DefaultCheck", 1);
                                // Update ViewModel state for presets (already defaults to DefaultChecked = true)
                                OnPropertyChanged(nameof(IsDefaultChecked));
                                OnPropertyChanged(nameof(IsBassBoostChecked));
                                OnPropertyChanged(nameof(IsSpeechBoostChecked));
                                OnPropertyChanged(nameof(IsTrebleBoostChecked));
                            }
                            else
                            {
                                DdpmCommonHelper.WriteUILog($"[WalkThroughPageViewModel] DeviceManagerSA or DeviceID is null during initial audio state setting.");
                            }
                        }
                        break;

                    case MonitorInfo monitorInfo:
                        MInfo = monitorInfo;
                        DdpmCommonHelper.WriteUILog("[WalkThroughPageViewModel] InitializeDevice MonitorInfo ...");
                        break;

                    default:
                        // 其他或 null
                        _currentDeviceinfo = info;
                        DdpmCommonHelper.WriteUILog("[WalkThroughPageViewModel] InitializeDevice General DDPM info ...");
                        break;
                }

                // 初始化
                _currentPageIndex = 0;

                // 確認 _devicePages 是否為 null
                if (_devicePages == null)
                {
                    DdpmCommonHelper.WriteUILog("[WalkThroughPageViewModel] _devicePages is null, EndWalkThrough ...");
                    EndWalkThrough();
                    return;
                }

                // 檢查 Dictionary 中是否包含指定的 key
                if (!_devicePages.ContainsKey(deviceModel))
                {
                    DdpmCommonHelper.WriteUILog("[WalkThroughPageViewModel] InitializeDevice - No pages found for this device model, EndWalkThrough ...");
                    EndWalkThrough();
                    return;
                }

                var pages = _devicePages[deviceModel];
                // 如果 pages 為 null 或內容是空的，也可以做錯誤處理
                if (pages == null || pages.Count == 0)
                {
                    DdpmCommonHelper.WriteUILog("[WalkThroughPageViewModel] InitializeDevice - Page list is null/empty, EndWalkThrough ...");
                    EndWalkThrough();
                    return;
                }

                // 設定頁面數量後，載入對應資訊
                DdpmCommonHelper.WriteUILog("[WalkThroughPageViewModel] InitializeDevice - Pages found, updating ...");
                CurrentAnimationPage = pages.Count;
                _currentTotalPage = pages.Count - 1;

                UpdatePageContent();
                UpdateButtonVisibility(); // refresh
            }
            catch (Exception ex)
            {
                DdpmCommonHelper.WriteUILog($"[WalkThroughPageViewModel] InitializeDevice Exception: {ex.Message}\n{ex.StackTrace}");
            }
        }

        /// <summary>
        /// Update WalkThrough Page Content
        /// </summary>
        private void UpdatePageContent()
        {
            DdpmCommonHelper.WriteUILog($"[WalkThroughPageViewModel] UpdatePageContent in ...");
            try
            {
                _devicePages = DDPM.UI.WalkThroughData.WalkThroughData.GetDevicePages((int)DdpmCommonHelper.PreviousOsTheme);
                if (_devicePages.ContainsKey(_currentDeviceModel) && _currentPageIndex < _devicePages[_currentDeviceModel].Count)
                {
                    var pageData = _devicePages[_currentDeviceModel][_currentPageIndex];
                    MainText = pageData.MainText!;
                    DdpmCommonHelper.WriteUILog($"[WalkThroughPageViewModel] UpdatePageContent : {DdpmHomePlugin.DdpmHomePlugin.WalkThroughQueue[0].ModelType.ToString()} ...");
                    if (DdpmHomePlugin.DdpmHomePlugin.WalkThroughQueue[0].ModelType.Contains("Pen"))
                    {
                        SubText = pageData.SubText!.Replace("X %", DInfo.BatteryLevel.ToString() + "%").Replace("X%", DInfo.BatteryLevel.ToString() + "%");
                        DdpmCommonHelper.WriteUILog($"[WalkThroughPageViewModel] UpdatePageContent DInfo.BatteryLevel : {DInfo.BatteryLevel.ToString()} ...");
                    }
                    else
                    {
                        SubText = pageData.SubText!;
                    }
                    DeviceImage = DdpmCommonHelper.GetImageSourceFromCommonResource(pageData.MainImageSource!, "DDPM.UI.WalkThroughData");
                }
            }
            catch (Exception ex)
            {
                DdpmCommonHelper.WriteUILog($"[WalkThroughPageViewModel] UpdatePageContent Exception: {ex.Message}");
            }
        }

        /// <summary>
        /// Next Page
        /// </summary>
        public void NextPage()
        {
            try
            {
                if (_currentPageIndex < _devicePages[_currentDeviceModel].Count - 1)
                {
                    _currentPageIndex++;
                    UpdatePageContent();
                }
                else
                {
                    UpdateLastlogicalDeviceType();
                    if (DdpmHomePlugin.DdpmHomePlugin.WalkThroughQueue.Count != 0) // Error handling
                    {
                        WriteWalkThroughReg(DdpmHomePlugin.DdpmHomePlugin.WalkThroughQueue[0].ModelName);
                        DdpmHomePlugin.DdpmHomePlugin.WalkThroughEndList.Add(DdpmHomePlugin.DdpmHomePlugin.WalkThroughQueue[0]);
                        DdpmCommonHelper.WriteUILog($"[WalkThroughPageViewModel] InitializeDeviceFromQueue RemoveAt {DdpmHomePlugin.DdpmHomePlugin.WalkThroughQueue[0].ModelName}");
                        DdpmHomePlugin.DdpmHomePlugin.WalkThroughQueue.RemoveAt(0);
                    }
                    if (DdpmHomePlugin.DdpmHomePlugin.WalkThroughQueue.Count > 0)//After remove, still WalkThrough need to show
                    {
                        ProgressValue = 0;// second round set 0
                        InitializeDeviceFromQueue();
                    }
                    else
                    {
                        EndWalkThrough();
                    }
                }
                UpdateButtonVisibility(); // refresh button
            }
            catch (Exception ex)
            {
                DdpmCommonHelper.WriteUILog($"[WalkThroughPageViewModel] NextPage Exception: {ex.Message}");
            }
        }

        /// <summary>
        /// Previous Page
        /// </summary>
        public void PreviousPage()
        {
            try
            {
                if (_currentPageIndex > 0)
                {
                    _currentPageIndex--;
                    UpdatePageContent();
                }
                UpdateButtonVisibility(); // refresh button
            }
            catch (Exception ex)
            {
                DdpmCommonHelper.WriteUILog($"[WalkThroughPageViewModel] PreviousPage Exception: {ex.Message}");
            }
        }

        public void EndWalkThrough()
        {
            DdpmCommonHelper.WriteUILog("[WalkThroughPageViewModel] EndWalkThrough in ...");
            try
            {
                if (DdpmHomePlugin.DdpmHomePlugin.WalkThroughQueue.Count == 0)
                {
                    DdpmCommonHelper.WriteUILog("[WalkThroughPageViewModel] No items in WalkThroughQueue => set ShowPluginById to false.");
                    DdpmHomePlugin.DdpmHomePlugin.ShowPluginById = false;
                }

                IShowPluginManager? showPluginManager = WalkThroughPlugin.PluginIoc.GetService<IShowPluginManager>();
                if (showPluginManager == null)
                {
                    DdpmCommonHelper.WriteUILog("[WalkThroughPageViewModel] IShowPluginManager is null.");
                    ControlIcon(true);
                    DdpmCommonHelper.WriteUILog("[WalkThroughPageViewModel] EndWalkThrough end (no plugin manager).");
                    return;
                }

                DdpmCommonHelper.WriteUILog($"[WalkThroughPageViewModel] EndWalkThrough: last_logicalDeviceType='{last_logicalDeviceType}', deviceInfo='{_currentDeviceinfo}'.");

                switch (last_logicalDeviceType)
                {
                    case "CONSENT_PAGE":
                    case "DDPM":
                    case "Displays":
                        DdpmCommonHelper.WriteUILog($"[WalkThroughPageViewModel] ShowHomePage for '{last_logicalDeviceType}'.");
                        showPluginManager.ShowHomePage();
                        break;

                    case "LogicalWebcam":
                        ShowPluginOrHome(showPluginManager, DDPM.UI.Common.Constants.WebCameraPluginId, _currentDeviceinfo);
                        break;

                    case "LogicalKeyboard":
                        ShowPluginOrHome(showPluginManager, DDPM.UI.Common.Constants.KeyboardPluginId, _currentDeviceinfo);
                        break;

                    case "LogicalMouse":
                        ShowPluginOrHome(showPluginManager, DDPM.UI.Common.Constants.MousePluginId, _currentDeviceinfo);
                        break;

                    case "LogicalPen":
                        ShowPluginOrHome(showPluginManager, DDPM.UI.Common.Constants.PenPluginId, _currentDeviceinfo);
                        break;

                    case "LogicalHeadset":
                        ShowPluginOrHome(showPluginManager, DDPM.UI.Common.Constants.HeadsetPluginId, _currentDeviceinfo);
                        break;

                    default:
                        DdpmCommonHelper.WriteUILog($"[WalkThroughPageViewModel] EndWalkThrough default => ShowHomePage.");
                        showPluginManager.ShowHomePage();
                        break;
                }

                ControlIcon(true);

                DdpmCommonHelper.WriteUILog("[WalkThroughPageViewModel] EndWalkThrough end ...");
            }
            catch (Exception ex)
            {
                DdpmCommonHelper.WriteUILog($"[WalkThroughPageViewModel] EndWalkThrough Exception: {ex.Message}, {ex.StackTrace}");
            }
        }

        private void ShowPluginOrHome(IShowPluginManager showPluginManager, string pluginId, object? deviceInfo)
        {
            if (deviceInfo != null)
            {
                var deviceGuid = deviceInfo.ToString();
                DdpmCommonHelper.WriteUILog($"[WalkThroughPageViewModel] ShowPluginById('{pluginId}', GUID='{deviceGuid}').");
                showPluginManager.ShowPluginById(pluginId, deviceGuid);
            }
            else
            {
                DdpmCommonHelper.WriteUILog($"[WalkThroughPageViewModel] deviceInfo is null => ShowHomePage.");
                showPluginManager.ShowHomePage();
            }
        }

        public void WriteWalkThroughReg(string Model)
        {
            //try
            //{
            DdpmCommonHelper.WriteUILog($"[WalkThroughPageViewModel] WriteWalkThroughReg, Model : {Model} ... ");
            string regPath = $@"SOFTWARE\Dell\Dell Display And Peripheral Manager\UserSettings\Local\{DdpmHomePlugin.DdpmHomePlugin.UserId}";
            string regKey = $"IsFirstTimeWalkThroughDone_com.dell.DPM.Plugin.LogicalDevice.{Model}";
            if (Model == "MS700/7")
            {
                regKey = $"IsFirstTimeWalkThroughDone_com.dell.DPM.Plugin.LogicalDevice.MS700";
            }
            if (Model == "CONSENT_PAGE")
            {
                regPath = @"SOFTWARE\Dell\Dell Display And Peripheral Manager\UserSettings\Local";
                //return DdpmCommonHelper.DeviceManagerSA!.WriteRegistryData(DDPM.SA.Common.Settings.RegistryHive.LocalMachine, regPath, regKey, true).Result;
                //DdpmCommonHelper.WriteRegistryData(DDPM.SA.Common.Settings.RegistryHive.LocalMachine, regPath, regKey, true);
                //return;// true;
            }
            DdpmCommonHelper.WriteUILog($"[WalkThroughPageViewModel] WriteWalkThroughReg UserId : {DdpmHomePlugin.DdpmHomePlugin.UserId}, Model: {Model} ...");
            //return DdpmCommonHelper.DeviceManagerSA!.WriteRegistryData(DDPM.SA.Common.Settings.RegistryHive.LocalMachine, regPath, regKey, true).Result;
            DdpmCommonHelper.WriteRegistryData(DDPM.SA.Common.Settings.RegistryHive.LocalMachine, regPath, regKey, true);
            //return true;
            //}
            //catch (Exception ex)
            //{
            //    DdpmCommonHelper.WriteUILog($"[WalkThroughPageViewModel] WriteWalkThroughReg Exception: {ex.Message}");
            //    return false;
            //}
        }

        public void UpdateLastlogicalDeviceType()
        {
            DdpmCommonHelper.WriteUILog($"[WalkThroughPageViewModel] UpdateLastlogicalDeviceType in ... ");
            try
            {
                if (DdpmHomePlugin.DdpmHomePlugin.WalkThroughQueue.Count == 1)
                {
                    last_logicalDeviceType = DdpmHomePlugin.DdpmHomePlugin.WalkThroughQueue[0].ModelType;
                    DdpmCommonHelper.WriteUILog($"[WalkThroughPageViewModel] UpdateLastlogicalDeviceType last_logicalDeviceType : {last_logicalDeviceType} ... ");
                }
            }
            catch (Exception ex)
            {
                DdpmCommonHelper.WriteUILog($"[WalkThroughPageViewModel] UpdateLastlogicalDeviceType Exception: {ex.Message}");
            }
        }

        public void UpdateButtonVisibility()
        {
            // refresh button
            ArrowButtonVisibility = (_currentPageIndex == 0) ? Visibility.Collapsed : Visibility.Visible;
            SkipButtonVisibility = _currentPageIndex < _currentTotalPage ? Visibility.Visible : Visibility.Collapsed;
            FinishButtonVisibility = SkipButtonVisibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
        }

        public void ControlIcon(bool show_hide, bool IsEnabled = true)
        {
            DdpmCommonHelper.WriteUILog($"[WalkThroughPageViewModel] ControlIcon ... in ");
            try
            {
                IConsole? console = DdpmCommonHelper.MyConsole;
                if (console != null)
                {
                    var args = new EventManagerArgs();
                    args.Tag = new List<bool> { show_hide, IsEnabled }; //true=Show, false=Hide
                    console.RaiseEvent(ConsoleEventNames.Masthead_ShowAddDeviceIcon, this, args);
                    console.RaiseEvent(ConsoleEventNames.Masthead_ShowSettingsIcon, this, args);
                }
            }
            catch (Exception ex)
            {
                DdpmCommonHelper.WriteUILog($"[WalkThroughPageViewModel] ControlIcon Exception: {ex.Message}");
            }
        }

        private Visibility _arrowButtonVisibility = Visibility.Visible;
        public Visibility ArrowButtonVisibility
        {
            get => _arrowButtonVisibility;
            set => SetProperty(ref _arrowButtonVisibility, value);
        }

        private Visibility _skipButtonVisibility = Visibility.Visible;
        public Visibility SkipButtonVisibility
        {
            get => _skipButtonVisibility;
            set => SetProperty(ref _skipButtonVisibility, value);
        }

        private Visibility _finishButtonVisibility = Visibility.Collapsed;
        public Visibility FinishButtonVisibility
        {
            get => _finishButtonVisibility;
            set => SetProperty(ref _finishButtonVisibility, value);
        }

        public List<HomeDevice> HomeDevices
        {
            get => _homeDevices;
            set
            {
                SetProperty(ref _homeDevices, value);
                OnPropertyChanged("HomeDeviceCount");
            }
        }

        private HomeDevice? _selectedHomeDevice;
        public HomeDevice? SelectedHomeDevice
        {
            get => _selectedHomeDevice;
            set => SetProperty(ref _selectedHomeDevice, value);
        }

        private double _progressValue = 1;//first round set 1
        public double ProgressValue
        {
            get
            {
                return _progressValue;
            }
            set
            {
                _progressValue = value;
                OnPropertyChanged(nameof(ProgressValue));
            }
        }

        private int _currentAnimationPage;
        public int CurrentAnimationPage
        {
            get => _currentAnimationPage;
            set => SetProperty(ref _currentAnimationPage, value);
        }

        private string _mainText = string.Empty;
        public string MainText
        {
            get => _mainText;
            set => SetProperty(ref _mainText, value);
        }

        private string _subText = string.Empty;
        public string SubText
        {
            get => _subText;
            set => SetProperty(ref _subText, value);
        }

        private ImageSource? _deviceImage;
        public ImageSource? DeviceImage
        {
            get => _deviceImage;
            set => SetProperty(ref _deviceImage, value);
        }


        public bool _isConsentPageVisible = true;
        public bool IsConsentPageVisible
        {
            get => _isConsentPageVisible;
            set
            {
                SetProperty(ref _isConsentPageVisible, value);

                if (value)
                {
                    IsDDPMVisibility = false;
                    IsOtherVisibility = false;
                    IsPeripheralVisible = false;
                }
                OnPropertyChanged(nameof(ConsentPageVisibility));
            }
        }

        public Visibility ConsentPageVisibility => IsConsentPageVisible ? Visibility.Visible : Visibility.Collapsed;

        public bool _isPeripheralVisible = false;
        public bool IsPeripheralVisible
        {
            get => _isPeripheralVisible;
            set
            {
                SetProperty(ref _isPeripheralVisible, value);

                if (value)
                {
                    IsConsentPageVisible = false;
                    IsDDPMVisibility = false;
                    IsOtherVisibility = false;
                }
                OnPropertyChanged(nameof(PeripheralVisibility));
            }
        }

        public Visibility PeripheralVisibility => IsPeripheralVisible ? Visibility.Visible : Visibility.Collapsed;

        public bool _isDDPMVisibility = false;
        public bool IsDDPMVisibility
        {
            get => _isDDPMVisibility;
            set
            {
                SetProperty(ref _isDDPMVisibility, value);

                if (value)
                {
                    IsConsentPageVisible = false;
                    IsPeripheralVisible = false;
                    IsOtherVisibility = false;
                }
                OnPropertyChanged(nameof(DDPMVisibility));
            }
        }

        public Visibility DDPMVisibility => IsDDPMVisibility ? Visibility.Visible : Visibility.Collapsed;

        public bool _isOtherVisibility = false;
        public bool IsOtherVisibility
        {
            get => _isOtherVisibility;
            set
            {
                SetProperty(ref _isOtherVisibility, value);

                if (value)
                {
                    IsPeripheralVisible = false;
                    IsDDPMVisibility = false;
                    //Img3Source = DdpmCommonHelper.GetImageSourceFromCommonResource("WalkThrough/DDPM/DDPM2.png", "DDPM.UI.WalkThroughData");
                }
                OnPropertyChanged(nameof(OtherVisibility));
            }
        }
        public Visibility OtherVisibility => IsOtherVisibility ? Visibility.Visible : Visibility.Collapsed;

        private ImageSource? _img1Source;
        public ImageSource? Img1Source
        {
            get => _img1Source;
            set => SetProperty(ref _img1Source, value);
        }

        private ImageSource? _img2Source;
        public ImageSource? Img2Source
        {
            get => _img2Source;
            set => SetProperty(ref _img2Source, value);
        }

        private ImageSource? _img3Source;
        public ImageSource? Img3Source
        {
            get => _img3Source;
            set => SetProperty(ref _img3Source, value);
        }

        public Visibility AppWalkThroughVisibility
        {
            get
            {
                if (IsDDPMVisibility)
                    return Visibility.Visible;
                else
                    return Visibility.Collapsed;
            }
        }

        public Visibility AirAudioDevice => IsAirAudioDevice ? Visibility.Visible : Visibility.Collapsed;

        public bool _isAirAudioDeviceVisibility = false;

        public bool IsAirAudioDevice
        {
            get => _isAirAudioDeviceVisibility;
            set
            {
                SetProperty(ref _isAirAudioDeviceVisibility, value);

                OnPropertyChanged(nameof(IsAirAudioDevice));
            }
        }

        // <<< Additions for Audio Toggle Status Strings >>>
        private bool _outgoingAudioStatus = true; // Default to true
        public bool OutgoingAudioStatus
        {
            get => _outgoingAudioStatus;
            set
            {
                var currentDeviceId = DInfo?.ID.ToString();
                if (SetProperty(ref _outgoingAudioStatus, value))
                {
                    OnPropertyChanged(nameof(OutgoingAudio_String));
                    // Call the device manager
                    if (!string.IsNullOrEmpty(currentDeviceId) && DdpmCommonHelper.DeviceManagerSA != null)
                    {
                        DdpmCommonHelper.DeviceManagerSA.SetAirAudioMicNoiseCancellationAsync(currentDeviceId, value)
                            .ContinueWith(t => { 
                                if (t.IsFaulted)
                                    DdpmCommonHelper.WriteUILog($"[WalkThroughPageViewModel] Failed to set OutgoingAudioStatus: {t.Exception?.InnerException?.Message}");
                                else
                                    DdpmCommonHelper.WriteUILog($"[WalkThroughPageViewModel] SetAirAudioMicNoiseCancellationAsync for Outgoing called with {value}");
                            });
                    }
                    else
                    {
                        DdpmCommonHelper.WriteUILog($"[WalkThroughPageViewModel] DeviceManagerSA or DeviceID is null, cannot set OutgoingAudioStatus.");
                    }
                    DdpmCommonHelper.WriteUILog($"[WalkThroughPageViewModel] OutgoingAudioStatus property set to: {value}");
                }
            }
        }

        public string OutgoingAudio_String => OutgoingAudioStatus ? "ON" : "OFF";

        private bool _incomingAudioStatus = false; // Default to false
        public bool IncomingAudioStatus
        {
            get => _incomingAudioStatus;
            set
            {
                var currentDeviceId = DInfo?.ID.ToString();
                if (SetProperty(ref _incomingAudioStatus, value))
                {
                    OnPropertyChanged(nameof(IncomingAudio_String));
                    // Call the device manager
                    if (!string.IsNullOrEmpty(currentDeviceId) && DdpmCommonHelper.DeviceManagerSA != null)
                    {
                        DdpmCommonHelper.DeviceManagerSA.SetAirAudioMicNCIncomingAsync(currentDeviceId, value) // <-- Example, likely needs correction
                            .ContinueWith(t => { 
                                if (t.IsFaulted)
                                    DdpmCommonHelper.WriteUILog($"[WalkThroughPageViewModel] Failed to set IncomingAudioStatus: {t.Exception?.InnerException?.Message}");
                                else
                                    DdpmCommonHelper.WriteUILog($"[WalkThroughPageViewModel] SetAirAudioMicNCIncomingAsync for Incoming called with {value}"); // <-- Log needs correction if method changes
                            }); 
                    }
                    else
                    {
                        DdpmCommonHelper.WriteUILog($"[WalkThroughPageViewModel] DeviceManagerSA or DeviceID is null, cannot set IncomingAudioStatus.");
                    }
                    DdpmCommonHelper.WriteUILog($"[WalkThroughPageViewModel] IncomingAudioStatus property set to: {value}");
                }
            }
        }
        public string IncomingAudio_String => IncomingAudioStatus ? "ON" : "OFF";


        private bool _isDefaultChecked = true; // Assuming Default is the initial state
        private bool _isBassBoostChecked = false;
        private bool _isSpeechBoostChecked = false;
        private bool _isTrebleBoostChecked = false;

        public bool IsDefaultChecked
        {
            get => _isDefaultChecked;
            set
            {
                if (_isDefaultChecked != value && value)
                {
                    SetProperty(ref _isDefaultChecked, true, nameof(IsDefaultChecked));
                    SetProperty(ref _isBassBoostChecked, false, nameof(IsBassBoostChecked));
                    SetProperty(ref _isSpeechBoostChecked, false, nameof(IsSpeechBoostChecked));
                    SetProperty(ref _isTrebleBoostChecked, false, nameof(IsTrebleBoostChecked));
                    SetAudioSelectedPresetInBackgroundAsync("DefaultCheck", 1);
                }
            }
        }

        public bool IsBassBoostChecked
        {
            get => _isBassBoostChecked;
            set
            {
                if (_isBassBoostChecked != value && value)
                {
                    SetProperty(ref _isBassBoostChecked, true, nameof(IsBassBoostChecked));
                    SetProperty(ref _isDefaultChecked, false, nameof(IsDefaultChecked));
                    SetProperty(ref _isSpeechBoostChecked, false, nameof(IsSpeechBoostChecked));
                    SetProperty(ref _isTrebleBoostChecked, false, nameof(IsTrebleBoostChecked));
                    SetAudioSelectedPresetInBackgroundAsync("BassBoostCheck", 3);
                }
            }
        }

        public bool IsSpeechBoostChecked
        {
            get => _isSpeechBoostChecked;
            set
            {
                if (_isSpeechBoostChecked != value && value)
                {
                    SetProperty(ref _isSpeechBoostChecked, true, nameof(IsSpeechBoostChecked));
                    SetProperty(ref _isDefaultChecked, false, nameof(IsDefaultChecked));
                    SetProperty(ref _isBassBoostChecked, false, nameof(IsBassBoostChecked));
                    SetProperty(ref _isTrebleBoostChecked, false, nameof(IsTrebleBoostChecked));
                    SetAudioSelectedPresetInBackgroundAsync("SpeechBoostCheck", 2);
                }
            }
        }

        public bool IsTrebleBoostChecked
        {
            get => _isTrebleBoostChecked;
            set
            {
                if (_isTrebleBoostChecked != value && value)
                {
                    SetProperty(ref _isTrebleBoostChecked, true, nameof(IsTrebleBoostChecked));
                    SetProperty(ref _isDefaultChecked, false, nameof(IsDefaultChecked));
                    SetProperty(ref _isBassBoostChecked, false, nameof(IsBassBoostChecked));
                    SetProperty(ref _isSpeechBoostChecked, false, nameof(IsSpeechBoostChecked));
                    SetAudioSelectedPresetInBackgroundAsync("TrebleBoostCheck", 4);
                }
            }
        }

        private async void SetAudioSelectedPresetInBackgroundAsync(string Debounce, int Preset)
        {
            var currentDeviceId = DInfo?.ID.ToString();
            if (string.IsNullOrEmpty(currentDeviceId) || DdpmCommonHelper.DeviceManagerSA == null)
            {
                DdpmCommonHelper.WriteUILog($"[WalkThroughPageViewModel] DeviceManagerSA or DeviceID is null, cannot set Audio Preset.");
                return;
            }

            try
            {
                DdpmCommonHelper.WriteUILog($"[WalkThroughPageViewModel] Setting Audio Preset to {Preset} for device {currentDeviceId}...");
                await DdpmCommonHelper.DeviceManagerSA.SetAirAudioSelectedPresetAsync(currentDeviceId, Preset);
                DdpmCommonHelper.WriteUILog($"[WalkThroughPageViewModel] Successfully set Audio Preset to {Preset}.");
            }
            catch (Exception ex)
            {
                DdpmCommonHelper.WriteUILog($"[WalkThroughPageViewModel] Failed to set Audio Preset {Preset}: {ex.Message}");
            }
        }


        // Assume SupportedOutgoingAudio is already defined elsewhere or add if needed
        // We might need to initialize these statuses based on the current device
        // when InitializeDevice is called.
        // Example: OutgoingAudioStatus = GetInitialOutgoingStatusFromDevice(DInfo);
        // <<< End Additions >>>

        public void SwitchToDDPMPage()
        {
            DdpmCommonHelper.WriteUILog($"[WalkThroughPageViewModel] SwitchToDDPMPage ... in ");
            try
            {
                //IsPeripheralVisible = false;
                IsConsentPageVisible = false;
                IsDDPMVisibility = true;
                //IsOtherVisibility = false;
                //初始頁固定
                if (DdpmCommonHelper.PreviousOsTheme == (OSThemeEnum)1)
                {
                    Img1Source = DdpmCommonHelper.GetImageSourceFromCommonResource("WalkThrough/DDPM/DDPM1-1.png", "DDPM.UI.WalkThroughData");
                    Img2Source = DdpmCommonHelper.GetImageSourceFromCommonResource("WalkThrough/DDPM/DDPM1-2.png", "DDPM.UI.WalkThroughData");
                    //Img3Source = DdpmCommonHelper.GetImageSourceFromCommonResource("WalkThrough/DDPM/DDPM2.png", "DDPM.UI.WalkThroughData");
                }
                else
                {
                    Img1Source = DdpmCommonHelper.GetImageSourceFromCommonResource("WalkThrough/DDPM/Light_Mode/DDPM1-1.png", "DDPM.UI.WalkThroughData");
                    Img2Source = DdpmCommonHelper.GetImageSourceFromCommonResource("WalkThrough/DDPM/Light_Mode/DDPM1-2.png", "DDPM.UI.WalkThroughData");
                }
            }
            catch (Exception ex)
            {
                DdpmCommonHelper.WriteUILog($"[WalkThroughPageViewModel] SwitchToDDPMPage Exception: {ex.Message}");
            }
        }
    }
}
