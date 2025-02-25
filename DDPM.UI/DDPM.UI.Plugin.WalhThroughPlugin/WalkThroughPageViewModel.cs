using CommunityToolkit.Mvvm.ComponentModel;
using DDPM.SA.Common;
using DDPM.UI.Common;
using DDPM.UI.Common.Models;
using DDPM.UI.Plugin.DdpmHomePlugin.Interfaces;
using Dell.Client.Framework.Common;
using Dell.Client.Framework.UX.WPF;
using Dell.Client.Framework.UX.WPF.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using VcpCore.Common;
using static DDPM.UI.WalkThroughData.WalkThroughData;

namespace DDPM.UI.Plugin.WalkThroughPlugin
{
    public class WalkThroughPageViewModel : ObservableObject
    {
        private List<HomeDevice> _homeDevices = new List<HomeDevice>();
        public bool[] IsSelected { get; set; } = new bool[5];
        public int _currentTotalPage = 0;// Control button Visibility.Collapsed 
        public int _currentPageIndex = 0;
        public string _currentDeviceModel = string.Empty;
        private Dictionary<string, List<WalkThroughPageData>> _devicePages = DDPM.UI.WalkThroughData.WalkThroughData.GetDevicePages((int)DdpmCommonHelper.PreviousOsTheme);
        public object _currentDeviceinfo = string.Empty;
        public string last_logicalDeviceType = string.Empty;
        public MonitorInfo MInfo;
        public DeviceInfo DInfo;
        public WalkThroughPageViewModel()
        {
            Debug.WriteLine($"Queue Count: {DdpmHomePlugin.DdpmHomePlugin.WalkThroughQueue.Count}");
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
        }

        public void InitializeDeviceFromQueue()
        {
            DdpmCommonHelper.WriteUILog($"[WalkThroughPageViewModel] InitializeDeviceFromQueue in ...");
            // Check WalkThroughQueue
            while (DdpmHomePlugin.DdpmHomePlugin.WalkThroughQueue.Count > 0)
            {
                var device = DdpmHomePlugin.DdpmHomePlugin.WalkThroughQueue.First();

                // If _devicePages ContainsKey ModelNumber
                if (_devicePages.ContainsKey(device.ModelName))
                {
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
                EndWalkThrough();
            }
        }

        public void InitializeDevice(string deviceModel, object info)
        {
            DdpmCommonHelper.WriteUILog($"[WalkThroughPageViewModel] InitializeDevice in ...");
            _currentDeviceModel = deviceModel;

            if (info is DeviceInfo)
            {
                DInfo = (DeviceInfo)info;
                _currentDeviceinfo = DInfo.ID;
                DdpmCommonHelper.WriteUILog($"[WalkThroughPageViewModel] InitializeDevice DeviceInfo ...");
            }
            else
            {
                MInfo = (MonitorInfo)info;
                DdpmCommonHelper.WriteUILog($"[WalkThroughPageViewModel] InitializeDevice MonitorInfo ...");
            }

            _currentPageIndex = 0;

            if (_devicePages.ContainsKey(deviceModel))
            {
                CurrentAnimationPage = _devicePages[deviceModel].Count;
                _currentTotalPage = _devicePages[deviceModel].Count - 1;
                UpdatePageContent();
                UpdateButtonVisibility(); // refresh
            }
            else
            {
                EndWalkThrough();
            }
        }

        /// <summary>
        /// Update WalkThrough Page Content
        /// </summary>
        private void UpdatePageContent()
        {
            DdpmCommonHelper.WriteUILog($"[WalkThroughPageViewModel] UpdatePageContent in ...");
            _devicePages = DDPM.UI.WalkThroughData.WalkThroughData.GetDevicePages((int)DdpmCommonHelper.PreviousOsTheme);
            if (_devicePages.ContainsKey(_currentDeviceModel) && _currentPageIndex < _devicePages[_currentDeviceModel].Count)
            {
                var pageData = _devicePages[_currentDeviceModel][_currentPageIndex];
                MainText = pageData.MainText!;
                DdpmCommonHelper.WriteUILog($"[WalkThroughPageViewModel] UpdatePageContent : {DdpmHomePlugin.DdpmHomePlugin.WalkThroughQueue[0].ModelType.ToString()} ...");
                if (DdpmHomePlugin.DdpmHomePlugin.WalkThroughQueue[0].ModelType.Contains("Pen"))
                {
                    SubText = pageData.SubText!.Replace("X %", DInfo.BatteryLevel.ToString()).Replace("X%", DInfo.BatteryLevel.ToString());
                    DdpmCommonHelper.WriteUILog($"[WalkThroughPageViewModel] UpdatePageContent DInfo.BatteryLevel : {DInfo.BatteryLevel.ToString()} ...");
                }
                else
                {
                    SubText = pageData.SubText!;
                }
                DeviceImage = DdpmCommonHelper.GetImageSourceFromCommonResource(pageData.MainImageSource!, "DDPM.UI.WalkThroughData");
            }
        }

        /// <summary>
        /// Next Page
        /// </summary>
        public void NextPage()
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

        /// <summary>
        /// Previous Page
        /// </summary>
        public void PreviousPage()
        {
            if (_currentPageIndex > 0)
            {
                _currentPageIndex--;
                UpdatePageContent();
            }
            UpdateButtonVisibility(); // refresh button
        }

        public void EndWalkThrough()
        {
            DdpmCommonHelper.WriteUILog($"[WalkThroughPageViewModel] EndWalkThrough in ...");
            if (DdpmHomePlugin.DdpmHomePlugin.WalkThroughQueue.Count == 0)
                DdpmHomePlugin.DdpmHomePlugin.ShowPluginById = false;

            IShowPluginManager? _showPluginManager = WalkThroughPlugin.PluginIoc.GetService<IShowPluginManager>();
            //_showPluginManager?.ShowHomePage();
            switch (last_logicalDeviceType)
            {
                case "CONSENT_PAGE":
                    _showPluginManager?.ShowHomePage();
                    DdpmCommonHelper.WriteUILog($"[WalkThroughPageViewModel] EndWalkThrough: CONSENT_PAGE : CONSENT_PAGE");
                    break;
                case "DDPM":
                    _showPluginManager?.ShowHomePage();
                    DdpmCommonHelper.WriteUILog($"[WalkThroughPageViewModel] EndWalkThrough: DDPM : DDPM");
                    break;
                case "Displays":
                    _showPluginManager?.ShowHomePage();
                    DdpmCommonHelper.WriteUILog($"[WalkThroughPageViewModel] EndWalkThrough: Displays : Displays");
                    break;
                case "LogicalWebcam":
                    _showPluginManager?.ShowPluginById(DDPM.UI.Common.Constants.WebCameraPluginId, _currentDeviceinfo.ToString());
                    DdpmCommonHelper.WriteUILog($"[WalkThroughPageViewModel] EndWalkThrough: LogicalWebcam : " + _currentDeviceinfo.ToString());
                    break;
                case "LogicalKeyboard":
                    _showPluginManager?.ShowPluginById(DDPM.UI.Common.Constants.KeyboardPluginId, _currentDeviceinfo.ToString());
                    DdpmCommonHelper.WriteUILog($"[WalkThroughPageViewModel] EndWalkThrough: LogicalKeyboard : " + _currentDeviceinfo.ToString());
                    break;
                case "LogicalMouse":
                    _showPluginManager?.ShowPluginById(DDPM.UI.Common.Constants.MousePluginId, _currentDeviceinfo.ToString());
                    DdpmCommonHelper.WriteUILog($"[WalkThroughPageViewModel] EndWalkThrough: LogicalMouse : " + _currentDeviceinfo.ToString());
                    break;
                case "LogicalPen":
                    _showPluginManager?.ShowPluginById(DDPM.UI.Common.Constants.PenPluginId, _currentDeviceinfo.ToString());
                    DdpmCommonHelper.WriteUILog($"[WalkThroughPageViewModel] EndWalkThrough: LogicalPen : " + _currentDeviceinfo.ToString());
                    break;
                case "LogicalHeadset":
                    _showPluginManager?.ShowPluginById(DDPM.UI.Common.Constants.HeadsetPluginId, _currentDeviceinfo.ToString());
                    DdpmCommonHelper.WriteUILog($"[WalkThroughPageViewModel] EndWalkThrough: LogicalHeadset : " + _currentDeviceinfo.ToString());
                    break;
                default:
                    _showPluginManager?.ShowHomePage();
                    DdpmCommonHelper.WriteUILog($"[WalkThroughPageViewModel] EndWalkThrough: default : default");
                    break;
            }
            ControlIcon(true);
            DdpmCommonHelper.WriteUILog($"[WalkThroughPageViewModel] EndWalkThrough End ...");
        }

        public bool WriteWalkThroughReg(string Model)
        {
            string regPath = $@"SOFTWARE\Dell\Dell Display And Peripheral Manager\UserSettings\Local\{DdpmHomePlugin.DdpmHomePlugin.UserId}";
            string regKey = $"IsFirstTimeWalkThroughDone_com.dell.DPM.Plugin.LogicalDevice.{Model}";
            if (Model == "CONSENT_PAGE")
            {
                regPath = @"SOFTWARE\Dell\Dell Display And Peripheral Manager\UserSettings\Local";
                return DdpmCommonHelper.DeviceManagerSA!.WriteRegistryData(DDPM.SA.Common.Settings.RegistryHive.LocalMachine, regPath, regKey, true).Result;
            }
            DdpmCommonHelper.WriteUILog($"[WalkThroughPageViewModel] WriteWalkThroughReg UserId : {DdpmHomePlugin.DdpmHomePlugin.UserId}, Model: {Model} ...");
            return DdpmCommonHelper.DeviceManagerSA!.WriteRegistryData(DDPM.SA.Common.Settings.RegistryHive.LocalMachine, regPath, regKey, true).Result;
        }

        public void UpdateLastlogicalDeviceType()
        {
            if (DdpmHomePlugin.DdpmHomePlugin.WalkThroughQueue.Count == 1)
            {
                last_logicalDeviceType = DdpmHomePlugin.DdpmHomePlugin.WalkThroughQueue[0].ModelType;
            }
        }

        public void UpdateButtonVisibility()
        {
            // refresh button
            ArrowButtonVisibility = (_currentPageIndex == 0) ? Visibility.Collapsed : Visibility.Visible;
            SkipButtonVisibility = _currentPageIndex < _currentTotalPage ? Visibility.Visible : Visibility.Collapsed;
        }

        public void ControlIcon(bool show_hide, bool IsEnabled = true)
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
                if (ConsentPageVisibility == Visibility.Visible)
                    return Visibility.Collapsed;
                return Visibility.Visible;
            }
        }

        public void SwitchToDDPMPage()
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
    }
}
