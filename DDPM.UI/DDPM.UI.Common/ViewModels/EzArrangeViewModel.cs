#define ENABLE_CALL_SA
//Robert_Lin, 2024-8-14, comment out the #define line if you would like to disable calling to Subagent EAPlugin

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DDPM.Easy.Common;
using DDPM.SA.Common;
using DDPM.SA.Common.Settings;
using DDPM.UI.Common.Models;
using DDPM.UI.Common.UserControls;
using DDPM.UI.Resources.Helper;
using Dell.Client.Framework.Common;
using Dell.Client.Framework.UX.WPF;
using Dell.Client.Framework.UX.WPF.Controls;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using static DDPM.Easy.Common.CellBorder;

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
        public Dictionary<String, Bind_AddFullPage_AppCollectionData> _sortApps = new Dictionary<String, Bind_AddFullPage_AppCollectionData>();
        public EAProfileDDPM currentEditprofile;
        public EzProfileSettingDDPM currentEditprofileSetting;
        public ISplitCtrl? ispCtrlForEm;
        public UXTextBox currentUXTextBoxInfo;

        //Robert_Lin 2025-1-10, current selected Profile and ProfileSetting in RightView
        public EAProfileDDPM? _currentSelectedProfile;
        public EzProfileSettingDDPM? _currentSelectedProfileSetting;
        #endregion

        //Robert_Lin 2025-1-13 Fix _log is null bug: 
        //And all _log.Info() has been changed to LogInfo() in the EzArrangeViewModel


        //Robert_Lin, 2025-1-10 add a default ctor for design-time data binding,
        // do not use it in code
        public EzArrangeViewModel()
        {
            throw new NotImplementedException();
        }
        public EzArrangeViewModel(HomeDevice homeDev)
        {
            _homeDevice = homeDev;
            _deviceManagerSA = HomeDevice.DeviceManagerSA;

            //Any item in SplitListView is clicked, will notify to below handler
            //Default handler, but currently it will be handled by RightView
            _listViewItemClickCommand = new RelayCommand<SplitItem>(OnListViewItemClicked);

            _splitItemEditCommand = new RelayCommand<SplitItem>(OnSplitItemEditCommand);
            //Robert_Lin, 2025-1-9 Comment out unused call
            //Invoke_InitData();

            //Robert_Lin, 2025-4-10, refresh IsVertical
            if ((homeDev != null) && (homeDev.MonitorInfo != null))
            {
                Screen scr = Screen.AllScreens.FirstOrDefault(x => x.DeviceName.Equals(homeDev.MonitorInfo.DisplayName));
                IsVertical = (scr != null) ? (scr.Bounds.Width < scr.Bounds.Height) : false;
            }

            Init_EzMemory();
        }

        //Robert_Lin, 2025-1-7 IsEAFunctionEnabled is deleted

        //private bool _isEaFunctionEnabled;

        //public bool IsEaFunctionEnabled
        //{
        //    get => _isEaFunctionEnabled;
        //    set
        //    {
        //        bool res = _deviceManagerSA.SetEAFunctionEnabled(value).Result;
        //        if (res)
        //        {
        //            _isEaFunctionEnabled = value;
        //        }
        //    }
        //}

        #region Init Data (Robert_Lin, 2025-1-9 Mark this region as Unused)

        private void Invoke_InitData()
        {
            BackgroundWorker bw = new BackgroundWorker
            {
                WorkerReportsProgress = false,
                WorkerSupportsCancellation = false
            };
            //bw.DoWork += DoWork_InitData;
            //bw.RunWorkerCompleted += RunWorkerCompleted_InitData;

            //bw.RunWorkerAsync();
        }

        private void DoWork_InitData(object? sender, DoWorkEventArgs e)
        {
            //#if ENABLE_CALL_SA
            //            ObjGetVCP ret = _deviceManagerSA.GetEAFunctionEnabled().Result;
            //            if (ret.result)
            //                _isEaFunctionEnabled = (bool)ret.value;
            //#endif
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

                if (e.Result.ToString() == "OK")
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

        public void NotifySelectedLayoutChangedToSA()
        {
            BackgroundWorker bw = new BackgroundWorker()
            {
                WorkerReportsProgress = false,
                WorkerSupportsCancellation = false
            };
            bw.DoWork += delegate
            {
#if ENABLE_CALL_SA
                if ((_deviceManagerSA != null) && (SelectedSplitItem != null))
                {
                    bool res = _deviceManagerSA.NotifyEASelectedLayoutChanged(_homeDevice.MonitorInfo, SelectedSplitItem.ToSplitJson).Result;
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

        public void LogInfo(string message, Exception ex = null,
            [System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
            [System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = 0)
        {
            if (_log != null)
            {
                if (ex != null)
                {
                    _log.Error(ex,  $"{message} : Caller=[{memberName}], Line#=[{sourceLineNumber}]");
                }
                else
                {
                    _log.Info($"{message} : Caller=[{memberName}], Line#=[{sourceLineNumber}]");
                }
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

        #region Monitor Size
        //Robert_Lin 2024-12-7, DDPMW-866 Note.
        // Easy arrange window arrangement preset limited to 4 windows for all displays
        // below 19 inches in size (Reference: MDDM-3039)
        public bool IsMonitorSizeSmallerThan19Inches
        {
            get
            {
                if (_homeDevice != null &&
                    _homeDevice.MonitorInfo != null &&
                    _homeDevice.MonitorInfo.edid != null)
                {
                    return (_homeDevice.MonitorInfo.edid.Size < 19.000);
                }
                return false;
            }
        }
        #endregion

        //////////////////////////////////EzMemoryViewModel////////////////////////////////////////////

        #region EzMemoryViewModel

        public void PopUpAlreadyexistsMessage(Window Owner)
        {
            Thickness headMargin = new Thickness(24, 30, 45, 24);
            Thickness subMargin = new Thickness(24, -16, 24, 8);
            DdpmCommonHelper.DDPMEzMesssageBox(Strings.msgboxTitleForFirstPage, Strings.subTitleForFirstPage, true, Owner, 417, 148, headMargin, subMargin);
        }

        public ObservableCollection<Bind_AddFullPage_AppCollectionData> _bind_apps = new ObservableCollection<Bind_AddFullPage_AppCollectionData>();

        public IList<Bind_AddFullPage_AppCollectionData> _apps_all = new List<Bind_AddFullPage_AppCollectionData>();

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
                //Robert_Lin 2025-4-23, The Desktop.ID, typically will be like "DEL429E".
                //But if the Desktop.ID is "AAAAAAAA-AAAA-AAAA-AAAA-AAAAAAAAAAAA", then this Profile is migrated from DDM.
                //And should be Desktops.Count==1.
                //Robert_Lin, to support multiple "Partitions" (multiple Desktops)
                //Need to determine the index to Desktops
                int idxDesktop = -1;
                //Get the Instance of current monitor
                if (_homeDevice != null)
                {
                    string instance = _homeDevice.MonitorInfo.edid.Instance;
                    for (int idx=0; idx< easyArrangementDDPM.Desktops.Count; idx++)
                    {
                        if (easyArrangementDDPM.Desktops[idx].ID == instance)
                        {
                            idxDesktop = idx;
                            break;
                        }
                    }
                }
                if (idxDesktop < 0)
                {
                    //Check for Desktop.ID is "AAAAAAAA-AAAA-AAAA-AAAA-AAAAAAAAAAAA"
                    if (easyArrangementDDPM.Desktops.Count == 1)
                    {
                        if (easyArrangementDDPM.Desktops[0].ID.Equals("AAAAAAAA-AAAA-AAAA-AAAA-AAAAAAAAAAAA"))
                        {
                            //It's migrated from DDM, and it's the only one, we will use this Desktop[0]
                            idxDesktop = 0;
                        }
                    }
                    if (idxDesktop < 0)
                        return null;
                }
                // 從 Desktops[0].ProfileSettings 中找 ID
                EzProfileSettingDDPM matchingProfileSetting = easyArrangementDDPM.Desktops[idxDesktop].ProfileSettings.FirstOrDefault(ps => ps.ID == profileID);

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

            double totalSeconds = 0;
            if (autoLaunchtime > 1000000)
                totalSeconds = (double)autoLaunchtime.Value / 10000000; // Corrected conversion for DPeM
            else
                totalSeconds = (double)autoLaunchtime.Value; 

            TimeSpan time = TimeSpan.FromSeconds(totalSeconds);

            DateTime launchTime = DateTime.Today.Add(time);

            //CultureInfo.InvariantCulture must be AM/PM
            string formattedTime = launchTime.ToString("h:mm tt", CultureInfo.InvariantCulture);

            string amDesignator = Strings.Am;
            string pmDesignator = Strings.Pm;
            formattedTime = formattedTime.Replace("AM", amDesignator).Replace("PM", pmDesignator);

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

            string amDesignator = Strings.Am; //CultureInfo.CurrentCulture.DateTimeFormat.AMDesignator;
            string pmDesignator = Strings.Pm; //CultureInfo.CurrentCulture.DateTimeFormat.PMDesignator;
            AMPMList = new List<string> { amDesignator, pmDesignator };

            _isApplyEnabled = false;
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

        /// <summary>
        /// The original SpitItem data before editing
        /// </summary>
        public SplitItem? OrgEditSplitItem { get; set; } = null;
        

        #endregion

        #region First page

        public void RightViewDataClear()
        {
            ProfileTitleTextBlockValue = "N/A";
            AutomaticStartupValue = "N/A";
            LaunchByTimeValue = "N/A";
            AppDocumentValue = "N/A";
            IsApplyEnabled = false;
        }

        private bool _isApplyEnabled = false;
        public bool IsApplyEnabled
        {
            get => _isApplyEnabled;
            set => SetProperty(ref _isApplyEnabled, value);
        }

        private int _currentSelectsEAID;
        public int CurrentSelectsEAID
        {
            get => _currentSelectsEAID;
            set => SetProperty(ref _currentSelectsEAID, value);
        }

        //Robert_Lin 2025-1-10, this is the selected SplitItem in RightView
        private SplitItem _currentSelectspItem;
        public SplitItem CurrentSelectspItem
        {
            get => _currentSelectspItem;
            set
            {
                SetProperty(ref _currentSelectspItem, value);
                //Robert_Lin 2025-1-9, EzMemoryFirst.Next button use this propery to enable/disable 
                //OnPropertyChanged("IsFirstNextButtonEnabled");
                IsApplyEnabled = (value != null);
            }
        }

        private SplitItem _currentEditSelectspItem;
        public SplitItem CurrentEditSelectspItem
        {
            get => _currentEditSelectspItem;
            set
            {
                SetProperty(ref _currentEditSelectspItem, value);
                //Robert_Lin 2025-1-9, EzMemoryFirst.Next button use this propery to enable/disable 
                OnPropertyChanged("IsFirstNextButtonEnabled");
            }
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

        private string _inputText = LangHelper.Instance["EazyMemory.6"] + " 1";// "Profile 1";
        public string InputText
        {
            get => _inputText;
            set => SetProperty(ref _inputText, value);
        }
        #endregion First page

        #region RightView page

        private string _profileTitleTextBlockValue = Strings.NATextForRightViewUI; //"N/A";

        /// <summary>
        /// The Profile name of current selected SplitItem (profile) in RightView
        /// </summary>
        public string ProfileTitleTextBlockValue
        {
            get => _profileTitleTextBlockValue;
            set
            {
                SetProperty(ref _profileTitleTextBlockValue, value);
                OnPropertyChanged("ProfileTitleTextBlockValue");
            }
        }

        private string _automaticStartupValue = Strings.NATextForRightViewUI; //"N/A";
        public string AutomaticStartupValue
        {
            get => _automaticStartupValue;
            set
            {
                SetProperty(ref _automaticStartupValue, value);
                OnPropertyChanged("AutomaticStartupValue");
            }
        }

        private string _launchByTimeValue = Strings.NATextForRightViewUI; //"N/A";
        public string LaunchByTimeValue
        {
            get => _launchByTimeValue;
            set
            {
                SetProperty(ref _launchByTimeValue, value);
                OnPropertyChanged("LaunchByTimeValue");
            }
        }

        private string _appDocumentValue = Strings.NATextForRightViewUI; //"N/A";
        public string AppDocumentValue
        {
            get => _appDocumentValue;
            set
            {
                SetProperty(ref _appDocumentValue, value);
                OnPropertyChanged("AppDocumentValue");
            }
        }

        //Robert_Lin 2025-1-9 added for Next button IsEnabled proerty in EasyMemory. EzMemoryFirst view
        public bool IsFirstNextButtonEnabled
        {
            get
            {
                return CurrentEditSelectspItem != null;
            }
        }

        /// <summary>
        /// The profile data of current selected SplitItem (profile) in RightView
        /// When CurrentSelectspItem is changed, need to call LoadEmProfile() to update this property.
        /// </summary>
        public EAProfileDDPM? CurrentSelectedProfile
        {
            get => _currentSelectedProfile;
            set
            {
                SetProperty(ref _currentSelectedProfile, value);
            }
        }

        public EzProfileSettingDDPM? CurrentSelectedProfileSetting
        {
            get => _currentSelectedProfileSetting;
            set
            {
                SetProperty(ref _currentSelectedProfileSetting, value);
                if (_currentSelectedProfile != null)
                    SelectedProfileId = _currentSelectedProfile.ID;
            }
        }

        //Robert_Lin 2025-4-9 The selected ProfileId at init state
        public int SelectedProfileId { get; set; } = -1;

        /// <summary>
        /// Called after you updated the CurrentSelectspItem, it will:
        /// 1. Update CurrentSelectedProfile and CurrentSelectedProfileSetting from ProfilID in CurrentSelectspItem
        /// 2. Update UI from CurrentSelectedProfile and CurrentSelectedProfileSetting
        /// </summary>
        public void UpdateRightViewUIFromCurrentSelectspItem()
        {
            if (CurrentSelectspItem == null)
            {
                CurrentSelectedProfile = null;
                CurrentSelectedProfileSetting = null;
            }
            else
            {
                int profileID = CurrentSelectspItem.ProfileID;  

                EAProfileDDPM emProfile = new EAProfileDDPM();
                EzProfileSettingDDPM emProfileSettings = new EzProfileSettingDDPM(profileID, false, 0, false);

                if (LoadEmProfileSettings(profileID, ref emProfile, ref emProfileSettings))
                {
                    CurrentSelectedProfile = emProfile;
                    CurrentSelectedProfileSetting = emProfileSettings;
                    LogInfo($"@EzMemoryRightView.OnListViewItemClicked,, Profile ID={profileID}, Load user settings OK.");
                }
                else
                {
                    CurrentSelectedProfile = null;
                    CurrentSelectedProfileSetting = null;
                    LogInfo($"@EzMemoryRightView.OnListViewItemClicked, Profile ID {profileID} not found in UserSettings.");
                }
            }
            RefreshProfileSettingsToRightView();
        }


        /// <summary>
        ///Refresh to RightView UI.
        ///1 CurrentSelectedProfile: update to ProfileTitleTextBlockValue, AppDocumentValue
        ///2 CurrentSelectedProfileSetting: update to AutomaticStartupValue, LaunchByTimeValue
        /// </summary>
        public void RefreshProfileSettingsToRightView()
        {
            //1. efresh CurrentSelectedProfile data
            //If no selected SplitItem, then show default value ("N/A")
            if (CurrentSelectedProfile == null)
            {
                //1.1 Profile Name
                ProfileTitleTextBlockValue = Strings.NATextForRightViewUI; //"N/A";
                //1.2 App/Document
                AppDocumentValue = Strings.NATextForRightViewUI; //"N/A";
                //2.1 Automatic Startup
                AutomaticStartupValue = Strings.NATextForRightViewUI; //"N/A";
                //2.2 Launch By Time
                LaunchByTimeValue = Strings.NATextForRightViewUI; //"N/A";
                return;
            }

            //1.1 Profile Name
            ProfileTitleTextBlockValue = CurrentSelectedProfile.Name;
            //1.2 App/Document
            AppDocumentValue = string.Empty;
            if (CurrentSelectedProfile != null)
            {
                int no = 1;
                foreach (EAAppInfoDDPM profile in CurrentSelectedProfile.AppInfos)
                {
                    AppDocumentValue += no + ".  " + profile.Name + "\r\n";
                    no++;
                }
            }

            //2. Refresh CurrentSelectedProfileSetting data
            if (CurrentSelectedProfileSetting == null)
            {
                //2.1 Automatic Startup
                AutomaticStartupValue = Strings.No; // "No";
                //2.2 Launch By Time
                LaunchByTimeValue = "_";
            }
            else
            {
                //2.1 Automatic Startup
                if (CurrentSelectedProfileSetting.StartUpLaunch)
                    AutomaticStartupValue = Strings.Yes; // "Yes";
                else
                    AutomaticStartupValue = Strings.No; // "No";

                //2.2 Launch By Time
                if (CurrentSelectedProfileSetting.Auto)
                {
                    //2.2 Launch By Time
                    LaunchByTimeValue = ConvertAutoLaunchtimeToTime(CurrentSelectedProfileSetting.AutoStartTime);
                }
                else
                {
                    //2.2 Launch By Time
                    LaunchByTimeValue = "_";
                }
            }
        }

        #endregion RightView page

        #region Assign page

        public Dictionary<string, Bind_AddFullPage_AppCollectionData> SortAppsByTextBlockNumber(Dictionary<string, Bind_AddFullPage_AppCollectionData> sorrAppDic)
        {
            var sortedApps = sorrAppDic
                .OrderBy(pair => GetTextBlockNumber(pair.Key))
                .ToDictionary(pair => pair.Key, pair => pair.Value);

            return sortedApps;
        }

        public string UXTextBoxNameToUXButtonName(string btnName)
        {
            switch (btnName)
            {
                case "Window1_1TextBlock":
                    return "AddButton2_1";
                case "Window1_2TextBlock":
                    return "AddButton2_2";
                case "Window1TextBlock":
                    return "AddButton1";
                case "Window2TextBlock":
                    return "AddButton2";
                case "Window3TextBlock":
                    return "AddButton3";
                case "Window4TextBlock":
                    return "AddButton4";
                case "Window5TextBlock":
                    return "AddButton5";
                case "Window6TextBlock":
                    return "AddButton6";
                case "Window7TextBlock":
                    return "AddButton7";
                case "Window8TextBlock":
                    return "AddButton8";
                case "Window9TextBlock":
                    return "AddButton9";
                case "Window10TextBlock":
                    return "AddButton10";
                case "Window11TextBlock":
                    return "AddButton11";
                case "Window12TextBlock":
                    return "AddButton12";
                default:
                    return "AddButton1";
            }

        }

        public int GetTextBlockNumber(string btnName)
        {
            switch (btnName)
            {
                case "AddButton2_1":
                    return 1;
                case "AddButton2_2":
                    return 2;
                case "AddButton1":
                    return 1;
                case "AddButton2":
                    return 2;
                case "AddButton3":
                    return 3;
                case "AddButton4":
                    return 4;
                case "AddButton5":
                    return 5;
                case "AddButton6":
                    return 6;
                case "AddButton7":
                    return 7;
                case "AddButton8":
                    return 8;
                case "AddButton9":
                    return 9;
                case "AddButton10":
                    return 10;
                case "AddButton11":
                    return 11;
                case "AddButton12":
                    return 12;
                default:
                    return 0;
            }

        }

        public double GetAvailableWidth(UXTextBox textBox)
        {
            if (textBox == null)
                return 0;


            if (textBox.ActualWidth == 0)
            {
                textBox.Measure(new System.Windows.Size(double.PositiveInfinity, double.PositiveInfinity));
            }

            double availableWidth = Math.Abs(textBox.ActualWidth - textBox.Padding.Left - textBox.Padding.Right);
            return availableWidth > 0 ? availableWidth : 0;
        }

        /// <summary>
        /// Calculate String Width
        /// </summary>
        /// <param name="text">String</param>
        /// <param name="textBox"> UXTextBox class</param>
        /// <returns>WidthIncludingTrailingWhitespace</returns>
        public static double MeasureStringWidth(string text, UXTextBox textBox)
        {
            if (string.IsNullOrEmpty(text) || textBox == null)
                return 0;


            Typeface typeface = new Typeface(
                textBox.FontFamily,
                textBox.FontStyle,
                textBox.FontWeight,
                textBox.FontStretch);


            FormattedText formattedText = new FormattedText(
                text,
                System.Globalization.CultureInfo.CurrentCulture,
                textBox.FlowDirection,
                typeface,
                textBox.FontSize,
                textBox.Foreground,
                VisualTreeHelper.GetDpi(textBox).PixelsPerDip);

            return formattedText.WidthIncludingTrailingWhitespace;
        }

        /// <summary>
        /// Truncate String With Ellipsis
        /// </summary>
        /// <param name="text">String</param>
        /// <param name="textBox">UXTextBox class</param>
        /// <returns>return final String</returns>
        public static string TruncateStringWithEllipsis(string text, UXTextBox textBox)
        {
            if (string.IsNullOrEmpty(text) || textBox == null)
                return string.Empty;

            const string ellipsis = "...";
            double ellipsisWidth = MeasureStringWidth(ellipsis, textBox);
            double textWidth = MeasureStringWidth(text, textBox);
            double availableWidth = textBox.Width - ellipsisWidth;

            if (textWidth <= availableWidth)
            {
                return text;
            }
            else
            {
                double targetWidth = availableWidth - ellipsisWidth;
                if (targetWidth <= 0)
                {
                    return ellipsis;
                }
                int start = 0;
                int end = text.Length;
                string result = "";
                while (start < end)
                {
                    int mid = (start + end) / 2;
                    string substring = text.Substring(0, mid);
                    double substringWidth = MeasureStringWidth(substring, textBox);
                    if (substringWidth + ellipsisWidth <= availableWidth)
                    {
                        start = mid + 1;
                        result = substring;
                    }
                    else
                    {
                        end = mid;
                    }
                }
                if (result.Length >= 3)
                {
                    result = result.Substring(0, result.Length - 3) + ellipsis;
                }
                else
                {
                    result = ellipsis;
                }
                return result;
            }
        }

        public void UpdateTextBlockAppName(string btnName, string textAppName)
        {
            string appName = TruncateStringWithEllipsis(textAppName, currentUXTextBoxInfo);
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

        private void UpdateWindowAppName(int index, string textAppName)
        {
            string appName = TruncateStringWithEllipsis(textAppName, currentUXTextBoxInfo);
            switch (index)
            {
                case 1:
                    Window2_1AppName = appName;
                    Window1AppName = appName;
                    break;
                case 2:
                    Window2_2AppName = appName;
                    Window2AppName = appName;
                    break;
                case 3:
                    Window3AppName = appName;
                    break;
                case 4:
                    Window4AppName = appName;
                    break;
                case 5:
                    Window5AppName = appName;
                    break;
                case 6:
                    Window6AppName = appName;
                    break;
                case 7:
                    Window7AppName = appName;
                    break;
                case 8:
                    Window8AppName = appName;
                    break;
                case 9:
                    Window9AppName = appName;
                    break;
                case 10:
                    Window10AppName = appName;
                    break;
                case 11:
                    Window11AppName = appName;
                    break;
                case 12:
                    Window12AppName = appName;
                    break;
            }
        }

        public void AlignCellNumberAndAppName(int no, CellObj cel)
        {
            switch (no.ToString())
            {
                case "1":
                    FillOut(cel, "AddButton2_1", no);
                    FillOut(cel, "AddButton1", no);
                    break;
                case "2":
                    FillOut(cel, "AddButton2_2", no);
                    FillOut(cel, "AddButton2", no);
                    break;
                case "3":
                    FillOut(cel, "AddButton3", no);
                    break;
                case "4":
                    FillOut(cel, "AddButton4", no);
                    break;
                case "5":
                    FillOut(cel, "AddButton5", no);
                    break;
                case "6":
                    FillOut(cel, "AddButton6", no);
                    break;
                case "7":
                    FillOut(cel, "AddButton7", no);
                    break;
                case "8":
                    FillOut(cel, "AddButton8", no);
                    break;
                case "9":
                    FillOut(cel, "AddButton9", no);
                    break;
                case "10":
                    FillOut(cel, "AddButton10", no);
                    break;
                case "11":
                    FillOut(cel, "AddButton11", no);
                    break;
                case "12":
                    FillOut(cel, "AddButton12", no);
                    break;
                default:
                    break;
            }

        }

        public void FillOut(CellObj _cel, string _btnName, int _no)
        {
            if (_sortApps.ContainsKey(_btnName))
            {
                _cel.CellBd.CellNumber = _no;
                _cel.CellBd.MemoryText = _no.ToString();
                _cel.CellBd.MemoryImage = LoadImage(_sortApps[_btnName].AppIcon);
            }
            else
            {
                _cel.CellBd.CellNumber = _no;
            }
        }

        public void RegisterCellBorder(CellBorder cellBorder, int index)
        {
            cellBorder.DropOccurred += (sender, fileName) =>
            {
                if (HasDuplicateApp(fileName.First().Value.FileName)) // fileName KEY值為cell編號而且只會有1個觸發進來，所以判斷第一個即可
                {
                    LogInfo($"@[EzArrangeViewModel] HasDuplicateApp {cellBorder.Name} {fileName.First().Value.FileName}");
                    PopUpAlreadyexistsMessage(null);
                    return;
                }

                if (_sortApps.Values.Any(a => a.AppName.Equals(fileName.First().Value.FileName, StringComparison.OrdinalIgnoreCase) || a.AppPath.Equals(fileName.First().Value.FilePath, StringComparison.OrdinalIgnoreCase)))
                {
                    LogInfo($"@[EzArrangeViewModel] HasDuplicateApp {cellBorder.Name} {fileName.First().Value.FileName}");
                    PopUpAlreadyexistsMessage(null);
                    return;
                }
                var appData = new Bind_AddFullPage_AppCollectionData
                {
                    AppType = "True" // Desktop
                };

                UpdateAppInfo(index, fileName, appData);
            };
        }

        private void UpdateAppInfo(int index, Dictionary<int, CellAppData> fileName, Bind_AddFullPage_AppCollectionData appData)
        {
            string buttonKey = $"AddButton{index}";
            string additionalButtonKey = index == 1 ? "AddButton2_1" : index == 2 ? "AddButton2_2" : null;

            if (additionalButtonKey != null && (_sortApps.ContainsKey(additionalButtonKey) || _sortApps.ContainsKey(buttonKey)))
            {
                _sortApps.Remove(additionalButtonKey);
                _sortApps.Remove(buttonKey);
            }
            else if (_sortApps.ContainsKey(buttonKey))
            {
                _sortApps.Remove(buttonKey);
            }

            // 設定 Cell 的 MemoryImage
            fileName[index].Cell.MemoryImage = fileName[index].Image;

            // 檢查 _totalApps 中是否有相同的 AppName
            var existingApp = _bind_apps.FirstOrDefault(app => app.AppPath.ToUpper() == fileName[index].FilePath.ToUpper());
            if (existingApp != null)
            {
                appData.AppIcon = existingApp.AppIcon;
            }
            else
            {
                appData.AppIcon = fileName[index].Image.ToString();
            }

            // 更新 appData 的屬性
            appData.AppName = fileName[index].FileName;
            appData.AppPath = fileName[index].FilePath;

            // 將 appData 加入到 _sortApps
            if (additionalButtonKey != null && ispCtrlForEm.CellList.Count <= 2)
            {
                _sortApps.Add(additionalButtonKey, appData);
            }
            else
            {
                _sortApps.Add(buttonKey, appData);
            }

            // 更新對應的 WindowAppName 屬性
            UpdateWindowAppName(index, fileName[index].FileName);
            RefreshAssignPageButtons(); //PIMS-345539
        }

        public void ClearTextBlockAppName()
        {
            _bind_apps.Clear();
            _apps_all.Clear();
            _sortApps.Clear();
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

        public ImageSource LoadImage(string filePath)
        {
            if (System.IO.File.Exists(filePath))
            {
                ImageSource _imageSource;
                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(filePath, UriKind.Absolute);
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
                bitmap.Freeze();
                _imageSource = bitmap;
                return _imageSource;
            }
            return null;
        }

        public bool HasDuplicateApp(string appName)
        {
            bool hasDuplicate = false;

            foreach (var app in _sortApps.Values)
            {
                if (app.AppName == appName)
                {
                    hasDuplicate = true;
                    return hasDuplicate;
                }
            }
            return hasDuplicate;
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

        //Robert_Lin 2025-1-10 for Next button IsEnabled proerty in EzAssignProgram view
        public bool IsAssignNextButtonEnabled
        {
            get
            {
                if (_sortApps == null)
                    return false;

                return _sortApps.Count >= SelectedValue;
            }
        }
        public void RefreshAssignPageButtons()
        {
            OnPropertyChanged("IsAssignNextButtonEnabled");
        }

        /// <summary>
        /// Call this method to convert _sortapps to new Dictinary which change its keys
        /// For example, If cellCount<=2 => Keys will be AddButton2_1, AddButton2_2
        /// Else (cellCount> 2) then Keys will be AddButton1, AddButton2, AddButton3, ...
        /// </summary>
        /// <param name="cellCount"></param>
        public void RefreshSortAppsKeysForNewCellCount(int cellCount)
        {
            if (_sortApps.Count == 0)
                return;

            if (cellCount <= 2)
            {
                string key0 = _sortApps.ElementAt(0).Key;
                //If the format of key is "AddButton2_?" then it's 2 splits => nothing to do
                if (key0.StartsWith("AddButton2_"))
                    return;
            }
            else //CellCount > 2
            {
                string key0 = _sortApps.ElementAt(0).Key;
                //If the format of key is NOT  "AddButton2_?" then it's >2 splits => nothing to do
                if (!key0.StartsWith("AddButton2_"))
                    return;
            }

            Dictionary<String, Bind_AddFullPage_AppCollectionData> newSortApps = new Dictionary<String, Bind_AddFullPage_AppCollectionData>();

            // For each keys in _sortApps, update its key
            int no = 1;
            foreach(KeyValuePair<string, Bind_AddFullPage_AppCollectionData> item in _sortApps)
            {
                string newKey = string.Empty;
                if (cellCount <= 2)
                {
                    newKey = $"AddButton2_{no}";
                }
                else
                {
                    newKey = $"AddButton{no}";
                }
                newSortApps.Add(newKey, item.Value);
                no++;
            }
            _sortApps.Clear();
            _sortApps = newSortApps;
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
            set
            {
                if (value)
                {
                    _isLaunchAtStartup = value;
                }
                else
                {
                    SetProperty(ref _isLaunchAtStartup, value);
                }
            }
        }

        private bool _isManualLaunch = true;
        public bool IsManualLaunch
        {
            get => _isManualLaunch;
            set
            {
                if (SetProperty(ref _isManualLaunch, value) &&
                    _isManualLaunch)
                {
                    IsAutoLaunch = false;
                }
            }
        }

        private bool _isAutoLaunch = false;
        public bool IsAutoLaunch
        {
            get => _isAutoLaunch;
            set
            {
                if (SetProperty(ref _isAutoLaunch, value) &&
                    _isAutoLaunch)
                {
                    IsManualLaunch = false;
                }
            }
        }

        #endregion LaunchOption

        #region EM Profile / Settings
        public EAProfileDDPM? LoadEmUserSetting(int profileId)
        {
            if (DdpmCommonHelper.DeviceManagerSA == null)
            {
                LogInfo("@EMVM.LoadEmUserSetting(), DeviceManagerSA is null");
                return null;
            }
            //Load EM Profiles from User Setting
            List<EAProfileDDPM> emProfiles = DdpmCommonHelper.DeviceManagerSA.ReadUserEAProfileDDPM().Result;
            if (emProfiles == null)
            {

                LogInfo("@EMVM.LoadEmUserSetting(), Load EM Profiles return null.");
                return null;
            }
            //Find the profile by profileId
            EAProfileDDPM? foundProfile = emProfiles.FirstOrDefault(p => p.ID == profileId);
            if (foundProfile == null)
            {
                LogInfo($"@EMVM.LoadEmUserSetting(), ProfileId={profileId} not found in EM UserSettings.");
                return null;
            }
            return foundProfile;
        }

        public EzProfileSettingDDPM? LoadEmMonitorSettings(int profileId, HomeDevice? homeDevice=null)
        {
            if (DdpmCommonHelper.DeviceManagerSA == null)
            {
                LogInfo("@LoadEmMonitorSettings(), DeviceManagerSA is null");
                return null;
            }
            if (homeDevice == null)
            {
                homeDevice = _homeDevice;
                if (_homeDevice == null)
                {
                    LogInfo("@EMVM.LoadEmMonitorSettings(), HomeDevice is null");
                    return null;
                }
            }
            if (homeDevice.MonitorInfo == null)
            {
                LogInfo("@EMVM.LoadEmMonitorSettings(), HomeDevice.MonitorInfo is null.");
                return null;
            }

            //Load EM ProfileSettings from Per-Monitor Setting
            EasyArrangementDDPM emMonitorSettings = DdpmCommonHelper.DeviceManagerSA.ReadMonitorEasyArrangement(homeDevice.MonitorInfo).Result;
            if (emMonitorSettings == null)
            {
                LogInfo($"@EMVM.LoadEmMonitorSettings(), Monitor={homeDevice.MonitorInfo.modelName}/{homeDevice.MonitorInfo.edid.ServiceTag} not found.");
                return null;
            }

            EzProfileSettingDDPM? foundProfileSetting = FindProfileSettingById(emMonitorSettings, profileId);
            if (foundProfileSetting == null)
            {
                LogInfo($"@EMVM.LoadEmMonitorSettings(), ProfileId={profileId} not found in EM MonitorSettings.");
                return null;
            }

            return foundProfileSetting;
        }

        /// <summary>
        /// Load Profile data (EAProfileDDPM) from SettingsManager, 
        /// User settings to CurrentSelectedProfile
        /// Per-monitor settings to CurrentSelectedProfileSetting
        /// </summary>
        /// <param name="profileId">The profile ID to be loaded</param>
        /// <param name="profile">The per-user EM profile data</param>
        /// <param name="profileSettings">The per-monitor EM profile settings</param>
        /// <returns></returns>
        public bool LoadEmProfileSettings(int profileId, ref EAProfileDDPM profile, ref EzProfileSettingDDPM profileSettings)
        {
            if (DdpmCommonHelper.DeviceManagerSA == null)
            {
                LogInfo("@LoadEmProfileSettings(), DeviceManagerSA is null");
                return false;
            }
            if (_homeDevice == null)
            {
                LogInfo("@LoadEmProfileSettings(), HomeDevice is null");
                return false;
            }

            //Load EM Profiles from User Setting
            List<EAProfileDDPM> emProfiles = DdpmCommonHelper.DeviceManagerSA.ReadUserEAProfileDDPM().Result;
            if (emProfiles == null)
            {

                LogInfo("@LoadEmProfileSettings(), Load EM Profiles return null.");
                return false;
            }

            //Find the profile by profileId
            EAProfileDDPM? foundProfile = emProfiles.FirstOrDefault(p => p.ID == profileId);
            if (foundProfile == null)
            {
                LogInfo($"@LoadEmProfileSettings(), Profile with ID {profileId} not found.");
                return false;
            }
            profile = foundProfile;

            ////Refresh UI of Profile info
            ////1 Profile Name
            //ProfileTitleTextBlockValue = (CurrentSelectedProfile == null) ? "" : CurrentSelectedProfile.Name;

            ////2 App/Document
            //AppDocumentValue = string.Empty;
            //if (CurrentSelectedProfile != null)
            //{
            //    int no = 1;
            //    foreach (EAAppInfoDDPM appInfo in CurrentSelectedProfile.AppInfos)
            //    {
            //        AppDocumentValue += no + ".  " + appInfo.Name + "\r\n";
            //        no++;
            //    }
            //}

            //Load EM ProfileSettings from Per-Monitor Setting
            EasyArrangementDDPM emMonitorSettings = DdpmCommonHelper.DeviceManagerSA.ReadMonitorEasyArrangement(_homeDevice.MonitorInfo).Result;
            if (emMonitorSettings == null)
            {
                LogInfo($"@LoadEmProfileSettings(), Per-monitor settings, ProfileID={profileId} not found.");
                //Output the default values
                profileSettings = null;

                //AutomaticStartupValue = Strings.NATextForRightViewUI;
                //LaunchByTimeValue = Strings.NATextForRightViewUI;

                //Return true, as there is no per-monitor settings for this monitor
                return true;
            }

            profileSettings = FindProfileSettingById(emMonitorSettings, profileId);
           //else
           // {

           //     //No per-monitor settingsm then apply default settings
           //     if (CurrentSelectedProfileSetting == null)
           //     {
           //         AutomaticStartupValue = Strings.NATextForRightViewUI;
           //         LaunchByTimeValue = Strings.NATextForRightViewUI;
           //     }
           //     else
           //     {
           //         //AutomaticStartupValue
           //         if (CurrentSelectedProfile == null)
           //         {
           //             // 沒有monitor setting 數值 填否 跟 _
           //             AutomaticStartupValue = Strings.No;
           //             LaunchByTimeValue = "_";
           //             //_log.Info($"@[EzMemoryRightView] OnListViewItemClicked, ProfileSetting with ID {matchingProfile.ID} not found in MonitorSettings.");
           //         }
           //         else
           //         {
           //             AutomaticStartupValue = CurrentSelectedProfileSetting.StartUpLaunch ? Strings.Yes : Strings.No;
           //             LaunchByTimeValue = CurrentSelectedProfileSetting.Auto ? ConvertAutoLaunchtimeToTime(CurrentSelectedProfileSetting.AutoStartTime) : "_";
           //         }
           //     }
           // }
            return true;
        }
        #endregion EM Profile / Settings
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
    #endregion EzMemory Class
    
}