#define ENABLE_CALL_SA
//Robert_Lin, 2024-8-14, comment out the #define line if you would like to disable calling to Subagent EAPlugin

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DDPM.SA.Common;
using DDPM.SA.Common.Settings;
using DDPM.UI.Common.Models;
using DDPM.UI.Common.UserControls;
using Dell.Client.Framework.Common;
using Dell.Client.Framework.UX.WPF;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Input;
using VcpCore.Common;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TrayNotify;

namespace DDPM.UI.Common.ViewModels
{
    public class EzArrangeViewModel : ObservableObject
    {
        private HomeDevice _homeDevice;
        private readonly IDeviceManagerSA _deviceManagerSA;
        private ICommand? _listViewItemClickCommand;
        private ILog _log;

        #region EzMemory

        public Dictionary<string, List<EzMemoryPageData>> ezPages;
        public int _currentTotalPage = 0;// Control button Visibility.Collapsed 
        public int _currentPageIndex = 0;
        public string _currentDeviceModel = "EzMemory";
        public List<Bind_AddFullPage_AppCollectionData> _seletcApps = new List<Bind_AddFullPage_AppCollectionData>();
        public Dictionary<String, Bind_AddFullPage_AppCollectionData> _sortApps = new Dictionary<String, Bind_AddFullPage_AppCollectionData>();
        public EAProfileDDPM currentEditprofile;
        public EzProfileSettingDDPM currentEditprofileSetting;

        #endregion

        public EzArrangeViewModel(HomeDevice homeDev)
        {
            _homeDevice = homeDev;
            _deviceManagerSA = HomeDevice.DeviceManagerSA;

            //Any item in SplitListView is clicked, will notify to below handler
            //Default handler, but currently it will be handled by RightView
            _listViewItemClickCommand = new RelayCommand<SplitItem>(OnListViewItemClicked);

            _splitItemEditCommand = new RelayCommand<SplitItem>(OnSplitItemEditCommand);
            Invoke_InitData();

            Init_EzMemory();
        }

        private bool _isEaFunctionEnabled;

        public bool IsEaFunctionEnabled
        {
            get => _isEaFunctionEnabled;
            set
            {
                bool res = _deviceManagerSA.SetEAFunctionEnabled(value).Result;
                if (res)
                {
                    _isEaFunctionEnabled = value;
                }
            }
        }

        #region Init Data

        private void Invoke_InitData()
        {
            BackgroundWorker bw = new BackgroundWorker
            {
                WorkerReportsProgress = false,
                WorkerSupportsCancellation = false
            };
            bw.DoWork += DoWork_InitData;
            bw.RunWorkerCompleted += RunWorkerCompleted_InitData;

            bw.RunWorkerAsync();
        }

        private void DoWork_InitData(object? sender, DoWorkEventArgs e)
        {
#if ENABLE_CALL_SA
            ObjGetVCP ret = _deviceManagerSA.GetEAFunctionEnabled().Result;
            if (ret.result)
                _isEaFunctionEnabled = (bool)ret.value;
#endif
            e.Result = "OK";
        }

        private void RunWorkerCompleted_InitData(object sender, RunWorkerCompletedEventArgs e)
        {
            //If BackgroundWorker. WorkerSupportsCancellation is true, and you set e.Cancel=true in DoWorker
            if (e.Cancelled)
            {
                return;
            }
            if (e.Error != null)
            {
                //The message is e.Error.Message
                return;
            }
            //
            if (e.Result == null)
            {
                //In case that you never set value to e-Result
                //Log?.Info("** RefreshData abnormal stopped unknown reason.");
            }
            else
            {
                //Log?.Info($"** RefreshData result: {e.Result}");

                if (e.Result == "OK")
                {
                    //RefreshPbpSplitListView();
                    //Result is passed.
                    //UI_SetSelectedSplitItemByCurPxpMode();
                }
                else
                {
                    //Result is failed.
                }
            }
        }

        #endregion Init Data

        #region Get/Set to SA

        public void SetWorkSplit(int cellCount, char splitKey, List<double>? settings = null)
        {
            BackgroundWorker bw = new BackgroundWorker()
            {
                WorkerReportsProgress = false,
                WorkerSupportsCancellation = false
            };
            bw.DoWork += delegate
            {
#if ENABLE_CALL_SA
                if (_deviceManagerSA != null)
                {
                    bool res = _deviceManagerSA.SetEAWrokSplit(_homeDevice.MonitorInfo, cellCount, splitKey, settings).Result;
                }
#endif
            };
            bw.RunWorkerCompleted += delegate
            {
                //IsBusy = false;
            };
            //IsBusy = true;
            bw.RunWorkerAsync();
        }

        #endregion Get/Set to SA

        #region ListViewItem Click Commands

        public ICommand? ListViewItemClickCommand
        {
            get => _listViewItemClickCommand;
            set => SetProperty(ref _listViewItemClickCommand, value);
        }

        public void OnListViewItemClicked(SplitItem spItem)
        {
            SelectedSplitItem = spItem;
        }

        #endregion ListViewItem Click Commands

        #region Selected Item

        private SplitItem? _selectedSplitItem;

        public SplitItem? SelectedSplitItem
        {
            get => _selectedSplitItem;
            set
            {
                //Unselected origainl selection
                if (_selectedSplitItem != null)
                {
                    //If selected item is the same (no changed)
                    if (_selectedSplitItem == value)
                        return;
                    //Selection changed => unselection origial item
                    _selectedSplitItem.IsSelected = false;
                    if (_selectedSplitItem.Buddy != null)
                    {
                        _selectedSplitItem.Buddy.IsSelected = false;
                    }
                }
                if (value != null)
                {
                    SetProperty(ref _selectedSplitItem, value);
                    _selectedSplitItem.IsSelected = true;
                    if (_selectedSplitItem.Buddy != null)
                        _selectedSplitItem.Buddy.IsSelected = true;

                }
                else
                {
                    //Selection none
                    SetProperty(ref _selectedSplitItem, value);
                }
            }
        }

        #endregion Selected Item

        #region SplitItem Edit Command

        private ICommand? _splitItemEditCommand;

        public ICommand? SplitItemEditCommand
        {
            get => _splitItemEditCommand;
            set => SetProperty(ref _splitItemEditCommand, value);
        }

        public void OnSplitItemEditCommand(SplitItem splitItem)
        {
            //Not handled here, it will be handled by HandleSplitItemEditCommand() in RightView
        }

        #endregion SplitItem Edit Command

        #region Log

        public void CreateLog(IConsole console, string logName)
        {
            _log = console.CreateLog(logName);
        }

        public void LogInfo(string message)
        {
            if (_log != null)
            {
                _log.Info(message);
            }
        }

        #endregion Log

        #region Screen Orientation
        private bool _isVertical = false;
        public bool IsVertical
        {
            get => _isVertical;
            set => SetProperty(ref _isVertical, value);
        }
        #endregion

        //////////////////////////////////EzMemoryViewModel////////////////////////////////////////////

        #region EzMemoryViewModel

        //存取 RightView 的 SplitListView
        public SplitListView _splitListRightView;
        public SplitListView splitListRightView
        {
            get => _splitListRightView;
            set => _splitListRightView = value;
        }

        public EzProfileSettingDDPM? FindProfileSettingById(EasyArrangementDDPM easyArrangementDDPM, int profileID)
        {
            if (easyArrangementDDPM.Desktops != null && easyArrangementDDPM.Desktops.Count > 0)
            {
                // 從 Desktops[0].ProfileSettings 中找 ID
                EzProfileSettingDDPM matchingProfileSetting = easyArrangementDDPM.Desktops[0].ProfileSettings.FirstOrDefault(ps => ps.ID == profileID);

                if (matchingProfileSetting != null)
                {
                    return matchingProfileSetting;
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }

        public string ConvertAutoLaunchtimeToTime(long? autoLaunchtime)
        {
            if (!autoLaunchtime.HasValue)
            {
                return String.Empty;
            }

            double totalSeconds = (double)autoLaunchtime.Value;

            TimeSpan time = TimeSpan.FromSeconds(totalSeconds);

            DateTime launchTime = DateTime.Today.Add(time);

            string formattedTime = launchTime.ToString("h:mm tt", System.Globalization.CultureInfo.InvariantCulture);

            return formattedTime;
        }

        public int ConvertToLayout(int cellCount, char splitKey)
        {
            if (cellCount < 0 || cellCount > 12)
            {
                return 0;
            }

            if (splitKey < 'A' || splitKey > 'Z')
            {
                return 0;
            }

            // CellCount 左移 5 位，SplitKey 轉換為 0 到 25 之間的值
            return (cellCount << 5) | (splitKey - 'A');
        }

        public (int cellCount, char splitKey) ParseFromLayout(int layout)
        {
            // 取高 5 位
            int cellCount = (layout >> 5) & 0xF; // 0xF 代表只取前 4 個位元

            // 取低 5 位，並加回 'A' 得到字母
            char splitKey = (char)('A' + (layout & 0x1F)); // 0x1F 代表只取低 5 位元

            return (cellCount, splitKey);
        }

        private void Init_EzMemory()
        {
            HourList = Enumerable.Range(1, 12).Select(i => i.ToString("D2")).ToList();
            MinuteList = Enumerable.Range(0, 60).Select(i => i.ToString("D2")).ToList();//將數字格式化成兩位數，單位數自動補 0

            string amDesignator = CultureInfo.CurrentCulture.DateTimeFormat.AMDesignator;
            string pmDesignator = CultureInfo.CurrentCulture.DateTimeFormat.PMDesignator;
            AMPMList = new List<string> { amDesignator, pmDesignator };
        }

        private double _progressValue = 1;
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

        public Dictionary<string, List<EzMemoryPageData>> GetEzPages()
        {
            return new Dictionary<string, List<EzMemoryPageData>>
            {
                { "EzMemory", new List<EzMemoryPageData>
                    {
                        new EzMemoryPageData { MainText = Strings.FirstPageMainText, SubText = Strings.FirstPageSubText},
                        new EzMemoryPageData { MainText = Strings.AssignPageMainText, SubText = Strings.AssignPageSubText},
                        new EzMemoryPageData { MainText = Strings.LaunchOptionPageMainText, SubText = Strings.LaunchOptionPageSubText}
                    }
                },

            };
        }

        #endregion

        #region First page
       
        private SplitItem _currentSelectspItem;
        public SplitItem CurrentSelectspItem
        {
            get => _currentSelectspItem;
            set => SetProperty(ref _currentSelectspItem, value);
        }

        private SplitItem _currentEditSelectspItem;
        public SplitItem CurrentEditSelectspItem
        {
            get => _currentEditSelectspItem;
            set => SetProperty(ref _currentEditSelectspItem, value);
        }

        //記錄進入 Edit status
        private bool _iseditProfile = false;
        public bool IsEditProfile
        {
            get => _iseditProfile;
            set => SetProperty(ref _iseditProfile, value);
        }

        //記錄 Edit status 由 AddPage 返回
        private bool _isAddPageBack = false;
        public bool IsAddPageBack
        {
            get => _isAddPageBack;
            set => SetProperty(ref _isAddPageBack, value);
        }

        private string _inputText = "Profile 1";
        public string InputText
        {
            get => _inputText;
            set => SetProperty(ref _inputText, value);
        }
        #endregion First page

        #region RightView page

        private string _profileTitleTextBlockValue;
        public string ProfileTitleTextBlockValue
        {
            get => _profileTitleTextBlockValue;
            set
            {
                SetProperty(ref _profileTitleTextBlockValue, value);
                OnPropertyChanged("ProfileTitleTextBlockValue");
            }
        }

        private string _automaticStartupValue;
        public string AutomaticStartupValue
        {
            get => _automaticStartupValue;
            set
            {
                SetProperty(ref _automaticStartupValue, value);
                OnPropertyChanged("AutomaticStartupValue");
            }
        }

        private string _launchByTimeValue;
        public string LaunchByTimeValue
        {
            get => _launchByTimeValue;
            set
            {
                SetProperty(ref _launchByTimeValue, value);
                OnPropertyChanged("LaunchByTimeValue");
            }
        }

        private string _appDocumentValue;
        public string AppDocumentValue
        {
            get => _appDocumentValue;
            set
            {
                SetProperty(ref _appDocumentValue, value);
                OnPropertyChanged("AppDocumentValue");
            }
        }

        #endregion RightView page

        #region Assign page

        public void UpdateTextBlockAppName(string btnName, string appName)
        {
            switch (btnName)
            {
                case "AddButton2_1":
                    Window2_1AppName = appName;
                    break;
                case "AddButton2_2":
                    Window2_2AppName = appName;
                    break;
                case "AddButton1":
                    Window1AppName = appName;
                    break;
                case "AddButton2":
                    Window2AppName = appName;
                    break;
                case "AddButton3":
                    Window3AppName = appName;
                    break;
                case "AddButton4":
                    Window4AppName = appName;
                    break;
                case "AddButton5":
                    Window5AppName = appName;
                    break;
                case "AddButton6":
                    Window6AppName = appName;
                    break;
                case "AddButton7":
                    Window7AppName = appName;
                    break;
                case "AddButton8":
                    Window8AppName = appName;
                    break;
                case "AddButton9":
                    Window9AppName = appName;
                    break;
                case "AddButton10":
                    Window10AppName = appName;
                    break;
                case "AddButton11":
                    Window11AppName = appName;
                    break;
                case "AddButton12":
                    Window12AppName = appName;
                    break;
                default:
                    break;
            }

        }

        public void ClearTextBlockAppName()
        {
            _sortApps.Clear();
            _seletcApps.Clear();
            Window2_1AppName = "";
            Window2_2AppName = "";
            Window1AppName = "";
            Window2AppName = "";
            Window3AppName = "";
            Window4AppName = "";
            Window5AppName = "";
            Window6AppName = "";
            Window7AppName = "";
            Window8AppName = "";
            Window9AppName = "";
            Window10AppName = "";
            Window11AppName = "";
            Window12AppName = "";
        }

        private string _window2_1AppName;
        public string Window2_1AppName
        {
            get => _window2_1AppName;
            set
            {
                SetProperty(ref _window2_1AppName, value);
                OnPropertyChanged("Window2_1AppName");
            }
        }

        private string _window2_2AppName;
        public string Window2_2AppName
        {
            get => _window2_2AppName;
            set
            {
                SetProperty(ref _window2_2AppName, value);
                OnPropertyChanged("Window2_2AppName");
            }
        }

        private string _window1AppName;
        public string Window1AppName
        {
            get => _window1AppName;
            set
            {
                SetProperty(ref _window1AppName, value);
                OnPropertyChanged("Window1AppName");
            }
        }

        private string _window2AppName;
        public string Window2AppName
        {
            get => _window2AppName;
            set
            {
                SetProperty(ref _window2AppName, value);
                OnPropertyChanged("Window2AppName");
            }
        }

        private string _window3AppName;
        public string Window3AppName
        {
            get => _window3AppName;
            set
            {
                SetProperty(ref _window3AppName, value);
                OnPropertyChanged("Window3AppName");
            }
        }

        private string _window4AppName;
        public string Window4AppName
        {
            get => _window4AppName;
            set
            {
                SetProperty(ref _window4AppName, value);
                OnPropertyChanged("Window4AppName");
            }
        }

        private string _window5AppName;
        public string Window5AppName
        {
            get => _window5AppName;
            set
            {
                SetProperty(ref _window5AppName, value);
                OnPropertyChanged("Window5AppName");
            }
        }

        private string _window6AppName;
        public string Window6AppName
        {
            get => _window6AppName;
            set
            {
                SetProperty(ref _window6AppName, value);
                OnPropertyChanged("Window6AppName");
            }
        }

        private string _window7AppName;
        public string Window7AppName
        {
            get => _window7AppName;
            set
            {
                SetProperty(ref _window7AppName, value);
                OnPropertyChanged("Window7AppName");
            }
        }

        private string _window8AppName;
        public string Window8AppName
        {
            get => _window8AppName;
            set
            {
                SetProperty(ref _window8AppName, value);
                OnPropertyChanged("Window8AppName");
            }
        }

        private string _window9AppName;
        public string Window9AppName
        {
            get => _window9AppName;
            set
            {
                SetProperty(ref _window9AppName, value);
                OnPropertyChanged("Window9AppName");
            }
        }

        private string _window10AppName;
        public string Window10AppName
        {
            get => _window10AppName;
            set
            {
                SetProperty(ref _window10AppName, value);
                OnPropertyChanged("Window10AppName");
            }
        }

        private string _window11AppName;
        public string Window11AppName
        {
            get => _window11AppName;
            set
            {
                SetProperty(ref _window11AppName, value);
                OnPropertyChanged("Window11AppName");
            }
        }

        private string _window12AppName;
        public string Window12AppName
        {
            get => _window12AppName;
            set
            {
                SetProperty(ref _window12AppName, value);
                OnPropertyChanged("Window12AppName");
            }
        }

        private bool _isRightGridPage2Visible = true;

        public bool IsRightGridPage2Visible
        {
            get { return _isRightGridPage2Visible; }
            set
            {
                _isRightGridPage2Visible = value;
                OnPropertyChanged(nameof(IsRightGridPage2Visible));
                OnPropertyChanged(nameof(IsRightGridPageTotalVisible));  // 同步
            }
        }

        public bool IsRightGridPageTotalVisible => !IsRightGridPage2Visible;

        /// <summary>
        /// Control Window 1 ~ 12 
        /// </summary>
        private int _selectedValue;

        public int SelectedValue
        {
            get { return _selectedValue; }
            set
            {
                _selectedValue = value;
                OnPropertyChanged(nameof(SelectedValue));
            }
        }

        //AssignPage trigger 的 buttonName
        private string _buttonName;
        public string ButtonName
        {
            get => _buttonName;
            set
            {
                SetProperty(ref _buttonName, value);
                OnPropertyChanged("ButtonName");
            }
        }
        #endregion

        #region LaunchOption

        private List<string> _hourList;
        public List<string> HourList
        {
            get => _hourList;
            set => SetProperty(ref _hourList, value);
        }

        private List<string> _minuteList;
        public List<string> MinuteList
        {
            get => _minuteList;
            set => SetProperty(ref _minuteList, value);
        }

        private List<string> _ampmList;
        public List<string> AMPMList
        {
            get => _ampmList;
            set => SetProperty(ref _ampmList, value);
        }
       
        private string _selectedHour;
        public string SelectedHour
        {
            get => _selectedHour;
            set => SetProperty(ref _selectedHour, value);
        }

        private string _selectedMinute;
        public string SelectedMinute
        {
            get => _selectedMinute;
            set => SetProperty(ref _selectedMinute, value);
        }

        private string _selectedAMPM;
        public string SelectedAMPM
        {
            get => _selectedAMPM;
            set => SetProperty(ref _selectedAMPM, value);
        }
        private bool _isLaunchAtStartup = false;
        public bool IsLaunchAtStartup
        {
            get => _isLaunchAtStartup;
            set => SetProperty(ref _isLaunchAtStartup, value);
        }

        private bool _isManualLaunch = true;
        public bool IsManualLaunch
        {
            get => _isManualLaunch;
            set
            {
                if (SetProperty(ref _isManualLaunch, value))
                {
                    if (_isManualLaunch)
                    {
                        IsAutoLaunch = false;
                    }
                }
            }
        }

        private bool _isAutoLaunch = false;
        public bool IsAutoLaunch
        {
            get => _isAutoLaunch;
            set
            {
                if (SetProperty(ref _isAutoLaunch, value))
                {
                    if (_isAutoLaunch)
                    {
                        IsManualLaunch = false;
                    }
                }
            }
        }

        #endregion LaunchOption

    }

    #region EzMemory Class
    public class EzMemoryPageData
    {
        public string? MainText { get; set; }
        public string? SubText { get; set; }
    }
    public class ApplicationItem
    {
        public string AppName { get; set; }
        public string AppIcon { get; set; }
        public string AppPath { get; set; }
    }
    #endregion
}