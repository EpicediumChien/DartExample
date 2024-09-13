using CommunityToolkit.Mvvm.ComponentModel;
using DDPM.UI.Common;
using DDPM.UI.Common.Models;
using Dell.Client.Framework.UX.WPF;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using static DDPM.UI.WalkThroughData.WalkThroughData;

namespace DDPM.UI.Plugin.WalkThroughPlugin
{
    public class WalkThroughPageViewModel : ObservableObject
    {
        private List<HomeDevice> _homeDevices = new List<HomeDevice>();
        public bool[] IsSelected { get; set; } = new bool[5];
        public int _currentTotalPage = 0;// Control button Visibility.Collapsed 
        public int _currentPageIndex = 0;
        private string _currentDeviceModel = string.Empty;
        private Dictionary<string, List<WalkThroughPageData>> _devicePages = DDPM.UI.WalkThroughData.WalkThroughData.GetDevicePages();

        public WalkThroughPageViewModel()
        {
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
                    InitializeDevice(device.ModelName);
                    break;
                }
                else
                {
                    // If _devicePages No ModelNumber, remove and next 
                    DdpmHomePlugin.DdpmHomePlugin.WalkThroughQueue.RemoveAt(0);
                }
            }

            if (DdpmHomePlugin.DdpmHomePlugin.WalkThroughQueue.Count == 0)
            {
                EndWalkThrough();
            }
        }

        public void InitializeDevice(string deviceModel)
        {
            _currentDeviceModel = deviceModel;
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

        private void EndWalkThrough()
        {
            DdpmHomePlugin.DdpmHomePlugin._showPluginById = false;
            IConsole? console = WalkThroughPlugin.PluginIoc.GetService<IConsole>();
            console?.ShowHomePage();
        }

        public void UpdateButtonVisibility()
        {
            // refresh button
            ArrowButtonVisibility = (_currentPageIndex == 0) ? Visibility.Collapsed : Visibility.Visible;
            SkipButtonVisibility = _currentPageIndex < _currentTotalPage ? Visibility.Visible : Visibility.Collapsed;
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
    }
}
