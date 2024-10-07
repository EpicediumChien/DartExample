#define ENABLE_CALL_SA
//Robert_Lin, 2024-8-14, comment out the #define line if you would like to disable calling to Subagent EAPlugin

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DDPM.SA.Common;
using DDPM.UI.Common.Models;
using DDPM.UI.Common.UserControls;
using Dell.Client.Framework.Common;
using Dell.Client.Framework.UX.WPF;
using System.ComponentModel;
using System.Windows.Input;
using VcpCore.Common;

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
        public readonly String msgboxTitleForFirstPage = "Warning";
        public readonly String subTitleForFirstPage = "Duplicate entries. Enter different documentation or applications for Easy Memory profile.";
        public readonly String CustomListTooltipText = "You can arrange the windows on your screen and click + icon.\r\nAlternatively, select an existing layout below and click the pencil icon to edit the layout.";

        public readonly String ezMemoryStartupErrorTitleStringForLaunchOptionPage = "Error";
        public readonly String ezMemoryStartupErrorStringForLaunchOptionPage = "Another profile is set to launch during PC startup. Do you want to replace it with this profile?";
        public readonly String TitleTBForLaunchOptionPage = "Select a launch option";
        public readonly String StartupCBContentForLaunchOptionPage = "Launch during PC startup";
        public readonly String ManulRBContentForLaunchOptionPage = "Manually select the profiles created";
        public readonly String AutoRBContentForLaunchOptionPage = "Automatically launch by time";

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

        //EzMemoryViewModel

        #region EzMemoryViewModel

        private void Init_EzMemory()
        {
            HourList = Enumerable.Range(1, 12).Select(i => i.ToString("D2")).ToList();
            MinuteList = Enumerable.Range(0, 60).Select(i => i.ToString("D2")).ToList();//將數字格式化成兩位數，單位數自動補 0
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
                        new EzMemoryPageData { MainText = "Easy Memory", SubText = "Save different profiles and restore them manually, by scheduled time or at system start-up.\r\n\r\nBegin by assigning a name to your Easy Memory Profile and selecting a layout."},
                        new EzMemoryPageData { MainText = "Assign programs", SubText = "Assign applications/documents to windows or drag the application icon to the respective partition.\r\n\r\nNote: Easy Arrange Memory usability may vary according to application type and launch behavior."},
                        new EzMemoryPageData { MainText = "Launch options", SubText = "Select a launch type"}
                    }
                },

            };
        }

        #endregion

        #region First page

        private string _inputText = "Profile 1";
        public string InputText
        {
            get => _inputText;
            set => SetProperty(ref _inputText, value);
        }
        #endregion First page

        #region RightView page

        private string _automaticStartupValue = "N/A";
        public string AutomaticStartupValue
        {
            get => _automaticStartupValue;
            set => SetProperty(ref _automaticStartupValue, value);
        }

        private string _launchByTimeValue = "N/A";
        public string LaunchByTimeValue
        {
            get => _launchByTimeValue;
            set => SetProperty(ref _launchByTimeValue, value);
        }

        private string _appDocumentValue = "N/A";
        public string AppDocumentValue
        {
            get => _appDocumentValue;
            set => SetProperty(ref _appDocumentValue, value);
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

        private string _selectedHour = "1";
        public string SelectedHour
        {
            get => _selectedHour;
            set => SetProperty(ref _selectedHour, value);
        }

        private string _selectedMinute = "00";
        public string SelectedMinute
        {
            get => _selectedMinute;
            set => SetProperty(ref _selectedMinute, value);
        }

        private string _selectedAMPM = "AM";
        public string SelectedAMPM
        {
            get => _selectedAMPM;
            set => SetProperty(ref _selectedAMPM, value);
        }

        private bool _isManualLaunch;
        public bool IsManualLaunch
        {
            get => _isManualLaunch;
            set => SetProperty(ref _isManualLaunch, value);
        }

        private bool _isAutoLaunch;
        public bool IsAutoLaunch
        {
            get => _isAutoLaunch;
            set => SetProperty(ref _isAutoLaunch, value);
        }

        private bool _isLaunchAtStartup;
        public bool IsLaunchAtStartup
        {
            get => _isLaunchAtStartup;
            set => SetProperty(ref _isLaunchAtStartup, value);
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