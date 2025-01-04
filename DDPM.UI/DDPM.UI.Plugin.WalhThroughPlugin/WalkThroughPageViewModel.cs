using CommunityToolkit.Mvvm.ComponentModel;
using DDPM.UI.Common;
using DDPM.UI.Common.Models;
using DDPM.UI.Plugin.DdpmHomePlugin.Interfaces;
using Dell.Client.Framework.Common;
using Dell.Client.Framework.UX.WPF;
using Dell.Client.Framework.UX.WPF.Controls;
using System;
using System.Collections.Generic;
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

        public WalkThroughPageViewModel()
        {
            if (DdpmHomePlugin.DdpmHomePlugin.WalkThroughQueue.Exists(info => info.ModelName == "DDPM"))
            {
                //IsPeripheralVisible = false;
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
            else
            {
                IsPeripheralVisible = true;
                //IsDDPMVisibility = false;
            }
            InitializeDeviceFromQueue();
            UpdateButtonVisibility();
        }

        public void InitializeDeviceFromQueue()
        {
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
                    DdpmHomePlugin.DdpmHomePlugin.WalkThroughQueue.RemoveAt(0);
                }
            }

            if (DdpmHomePlugin.DdpmHomePlugin.WalkThroughQueue.Count == 0)
            {
                EndWalkThrough();
            }
        }

        public void InitializeDevice(string deviceModel, object info)
        {
            _currentDeviceModel = deviceModel;
            _currentDeviceinfo = info;
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
            _devicePages = DDPM.UI.WalkThroughData.WalkThroughData.GetDevicePages((int)DdpmCommonHelper.PreviousOsTheme);
            if (_devicePages.ContainsKey(_currentDeviceModel) && _currentPageIndex < _devicePages[_currentDeviceModel].Count)
            {
                var pageData = _devicePages[_currentDeviceModel][_currentPageIndex];
                MainText = pageData.MainText!;
                SubText = pageData.SubText!;
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
                DdpmHomePlugin.DdpmHomePlugin.WalkThroughQueue.RemoveAt(0);
                if (DdpmHomePlugin.DdpmHomePlugin.WalkThroughQueue.Count > 0)
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
            if (DdpmHomePlugin.DdpmHomePlugin.WalkThroughQueue.Count == 0)
                DdpmHomePlugin.DdpmHomePlugin.ShowPluginById = false;

            IShowPluginManager? _showPluginManager = WalkThroughPlugin.PluginIoc.GetService<IShowPluginManager>();
            //_showPluginManager?.ShowHomePage();
            switch (last_logicalDeviceType)
            {
                case "DDPM":
                    _showPluginManager?.ShowHomePage();
                    break;
                case "Displays":
                    _showPluginManager?.ShowHomePage(); //_showPluginManager?.ShowPluginById(DDPM.UI.Common.Constants.DisplayPluginId);
                    break;
                case "LogicalWebcam":
                    _showPluginManager?.ShowPluginById(DDPM.UI.Common.Constants.WebCameraPluginId, _currentDeviceinfo.ToString());
                    break;
                case "LogicalKeyboard":
                    _showPluginManager?.ShowPluginById(DDPM.UI.Common.Constants.KeyboardPluginId, _currentDeviceinfo.ToString());
                    break;
                case "LogicalMouse":
                    _showPluginManager?.ShowPluginById(DDPM.UI.Common.Constants.MousePluginId, _currentDeviceinfo.ToString());
                    break;
                case "LogicalPen":
                    _showPluginManager?.ShowPluginById(DDPM.UI.Common.Constants.PenPluginId, _currentDeviceinfo.ToString());
                    break;
                case "LogicalHeadset":
                    _showPluginManager?.ShowPluginById(DDPM.UI.Common.Constants.HeadsetPluginId, _currentDeviceinfo.ToString());
                    break;
                default:
                    _showPluginManager?.ShowHomePage();
                    break;
            }
            ControlIcon(true);
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

        public void ControlIcon(bool show_hide)
        {
            IConsole? console = DdpmCommonHelper.MyConsole;
            if (console != null)
            {

                var args = new EventManagerArgs();
                args.Tag = show_hide; //true=Show, false=Hide
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
        public bool _isPeripheralVisible = false;
        public bool IsPeripheralVisible
        {
            get => _isPeripheralVisible;
            set
            {
                SetProperty(ref _isPeripheralVisible, value);
                
                if (value)
                {
                    IsDDPMVisibility = false;
                    IsOtherVisibility = false;
                }
                OnPropertyChanged(nameof(PeripheralVisibility));
            }
        }

        public Visibility PeripheralVisibility => IsPeripheralVisible ? Visibility.Visible : Visibility.Collapsed;

        public bool _isDDPMVisibility = true;
        public bool IsDDPMVisibility
        {
            get => _isDDPMVisibility;
            set
            {
                SetProperty(ref _isDDPMVisibility, value);
                
                if (value)
                {
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
    }
}
