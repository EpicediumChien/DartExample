using DDPM.UI.Common;
using Dell.Client.Framework.UX.WPF;
using Dell.Client.Framework.Common;
using System.Windows;
using DDPM.UI.Plugin.Common.ViewModels;
using DDPM.SA.Common;
using DDPM.UI.Common.Models;
using DDPM.SA.Common.Settings;
using System.Windows.Forms;
using CommunityToolkit.Mvvm.Input;
using DDPM.UI.Common.UserControls;
using DDPM.Easy.Common;
using DDPM.SA.Common.Display;
using UserControl = System.Windows.Controls.UserControl;
using Microsoft;
using static DDPM.UI.Common.User32;
using DDPM.UI.Common.Method;

namespace DDPM.UI.Module.EzMemory
{
    /// <summary>
    /// Interaction logic for EzMemoryRightView.xaml
    /// </summary>
    public partial class EzMemoryRightView : UserControl
    {
        #region Private Members
        private HomeDevice _homeDevice;
        private IDeviceManagerSA _deviceManagerSA;
        private DDPM.UI.Common.ViewModels.EzArrangeViewModel _vm;    
        private readonly DisplayViewModel _vmDisplay;
        private readonly IConsole _console;
        private readonly ILog _log;
        private HomeDevice _homeDeviceSelect;//紀錄RightView切換CB的螢幕
        private Debouncer _debouncerEmRightView;
        #endregion Private Members

        public EzMemoryRightView(DisplayViewModel vmDisplay)
        {
            _vmDisplay = vmDisplay;
            //Robert_lin 2025-1-21 PIMS-339909 Unable to set East Arrange layout after switch from PBP = OFF to PBP = ON or vice versa
            //Root cause:
            // The homeDevice is not update in vmDisplay when DeviceChanged
            //Colution:
            // Get the upated SelectedHomeDevivce from DdpmCommonHelper.ModuleOwner
            //OLD:
            //_homeDevice = vmDisplay.SelectedHomeDevice;
            //NEW:
            if (DdpmCommonHelper.ModuleOwner != null && DdpmCommonHelper.ModuleOwner.SelectedHomeDevice != null)
                _homeDevice = DdpmCommonHelper.ModuleOwner.SelectedHomeDevice;

            _console = vmDisplay.Console;
            _deviceManagerSA = HomeDevice.DeviceManagerSA;
            _homeDeviceSelect = _homeDevice;
            _log = vmDisplay.Console.CreateLog("EzMemoryRightView");
            _log.Info($"{nameof(EzMemoryRightView)} - Constructed");
            Requires.NotNull(vmDisplay, nameof(vmDisplay));
            InitializeComponent();

            if (_homeDevice.vmEzArrange == null)
            {
                _homeDevice.vmEzArrange = new DDPM.UI.Common.ViewModels.EzArrangeViewModel(_homeDevice);
            }
            _vm = _homeDevice.vmEzArrange;

            DataContext = _vm;


            InitListViewItems();

            _debouncerEmRightView = new Debouncer(2000, ExecuteDebouncedAction);
        }

        private void ExecuteDebouncedAction(object param)
        {
            if (param is string mode)
            {
                switch (mode)
                {
                    case "EzMemoryStart_Click":
                        EzMemoryStartClick();
                        break;
                    //------------------------------------------------------------------------------------------
                    default:
                        break;
                }
            }
        }

        private void EzMemoryStart_Click(object sender, RoutedEventArgs e)
        {
            _log.Info($"[EzMemoryRightView] EzMemoryStart_Click ... debouncer ... in");
            _debouncerEmRightView.Debounce("EzMemoryStart_Click");
            _log.Info($"[EzMemoryRightView] EzMemoryStart_Click ... debouncer ... out");
        }

        /// <summary>
        /// Find APP and Launch
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EzMemoryStartClick()
        {
            try
            {
                _log.Info($"[EzMemoryRightView] EzMemoryStartClick ... in");

                List<EAProfileDDPM> startEAProfileDDPM = DdpmCommonHelper.DeviceManagerSA.ReadUserEAProfileDDPM().Result;
                List<object> rc = new List<object>();

                if (_vm.CurrentSelectspItem != null)
                {
                    foreach (var relist in _vm.CurrentSelectspItem.ISplitCtrl.CellList)
                    {
                        rc.Add(relist);
                    }
                }
                else
                    return;

                if (startEAProfileDDPM != null)
                {
                    // 找相同 ID 的 Profile ID
                    EAProfileDDPM profileTostart = startEAProfileDDPM.FirstOrDefault(p => p.ID == _vm.CurrentSelectspItem.ProfileID);

                    if (profileTostart != null)
                    {
                        Dictionary<string, Bind_AddFullPage_AppCollectionData> launchApp = new Dictionary<string, Bind_AddFullPage_AppCollectionData>();
                        foreach (var item in profileTostart.AppInfos)
                        {
                            Bind_AddFullPage_AppCollectionData app = new Bind_AddFullPage_AppCollectionData();
                            app.AppPath = item.Path;
                            app.AppName = item.Name;
                            app.AppUserModelID = item.AppUserModelID;
                            app.AppType = item.IsUWP == false ? "False" : "True";
                            launchApp.Add(app.AppName, app);
                        }
                        _vm.SortAppsByTextBlockNumber(launchApp);
                        //_deviceManagerSA.LaunchAndArrangeApps(launchApp);
                        _deviceManagerSA.LaunchAndArrangeAppsWithEzArrange(launchApp, _homeDeviceSelect.MonitorInfo, profileTostart.Layout);
                        _log.Info($"@[EzMemoryRightView] EzMemoryStart_Click, Profile with ID {_vm.CurrentSelectspItem.LayoutID} removed from UserSettings.");
                    }
                    else
                    {
                        _log.Info($"@[EzMemoryRightView] EzMemoryStart_Click, Profile with ID {_vm.CurrentSelectspItem.LayoutID} not found in UserSettings.");
                    }
                    //DdpmCommonHelper.DeviceManagerSA!.ShowOSD(_homeDeviceSelect.MonitorInfo, OSDType.EasyMemory);
                }
            }
            catch (Exception ex)
            {
                _log.Error($"[EzMemoryRightView] EzMemoryStart_Click Exception occurred: {ex.Message}");
            }
        }

        #region SplitItem Delete

        /// <summary>
        /// SplitItem Delete event
        /// </summary>
        /// <param name="spItem"></param>
        private void OnListViewItemDeleted(SplitItem spItem)
        {
            try
            {
                // User Setting Delete
                List<EAProfileDDPM> clickedEAProfileDDPM = DdpmCommonHelper.DeviceManagerSA.ReadUserEAProfileDDPM().Result;

                if (clickedEAProfileDDPM != null)
                {
                    // 找相同 ID 的 Profile ID
                    EAProfileDDPM profileToRemove = clickedEAProfileDDPM.FirstOrDefault(p => p.ID == spItem.ProfileID);

                    if (profileToRemove != null)
                    {
                        clickedEAProfileDDPM.Remove(profileToRemove);
                        DdpmCommonHelper.DeviceManagerSA.WriteUserListEAProfileDDPM(clickedEAProfileDDPM);
                        splitListView_RecentForEzM.DeleteSplitItem(spItem);
                        _log.Info($"@[EzMemoryRightView] OnListViewItemDeleted, Profile with ID {spItem.ProfileID} removed from UserSettings.");
                    }
                    else
                    {
                        _log.Info($"@[EzMemoryRightView] OnListViewItemDeleted, Profile with ID {spItem.ProfileID} not found in UserSettings.");
                    }
                }
                else
                {
                    _log.Info($"@[EzMemoryRightView] OnListViewItemDeleted, No EAProfileDDPM found in UserSettings.");
                }
            }
            catch (Exception ex)
            {
                _log.Error($"@[EzMemoryRightView] OnListViewItemDeleted, Error occurred while deleting from UserSettings: {ex.Message}");
            }

            try
            {
                // Monitor Setting Delete
                EasyArrangementDDPM _easyArrangementDDPM = DdpmCommonHelper.DeviceManagerSA.ReadMonitorEasyArrangement(_homeDevice.MonitorInfo).Result;

                if (_easyArrangementDDPM != null && _easyArrangementDDPM.Desktops.Count > 0)
                {
                    // 找相同 ID
                    EzProfileSettingDDPM profileSettingToRemove = _easyArrangementDDPM.Desktops[0].ProfileSettings.FirstOrDefault(ps => ps.ID == spItem.ProfileID);

                    if (profileSettingToRemove != null)
                    {
                        // 找到，則移除
                        _easyArrangementDDPM.Desktops[0].ProfileSettings.Remove(profileSettingToRemove);
                        DdpmCommonHelper.DeviceManagerSA.WriteMonitorEasyArrangement(_homeDevice.MonitorInfo, _easyArrangementDDPM);
                        _log.Info($"@[EzMemoryRightView] OnListViewItemDeleted, ProfileSetting with Monitor Model {_homeDevice.MonitorInfo.modelName}, ID {spItem.ProfileID} removed from MonitorSettings.");
                    }
                    else
                    {
                        _log.Info($"@[EzMemoryRightView] OnListViewItemDeleted, ProfileSetting with Monitor Model {_homeDevice.MonitorInfo.modelName}, ID {spItem.ProfileID} not found in MonitorSettings.");
                    }
                }
                else
                {
                    _log.Info($"@[EzMemoryRightView] OnListViewItemDeleted, No valid EasyArrangementDDPM or Desktops found in MonitorSettings.");
                }
            }
            catch (Exception ex)
            {
                _log.Error($"@[EzMemoryRightView] OnListViewItemDeleted, Error occurred while deleting from MonitorSettings: {ex.Message}");
            }
            _vm.RightViewDataClear();
            return;
        }

        #endregion SplitItem Delete

        #region SplitItem Edit

        /// <summary>
        /// SplitItem Edite event
        /// </summary>
        /// <param name="spItem"></param>
        private void OnListViewItemEdited(SplitItem spItem)
        {
            //Find the EM Profile and ProfileSetting from SplitItem.ProfileID
            //
            //OUTPUT:
            // _vm.currentEditprofile  : The profile to be edited
            // _vm.currentEditprofileSetting : The profile setting to be edited
            try
            {
                _log.Info($"@[EzMemoryRightView] OnListViewItemEdited ... in");
                // User Setting Delete
                List<EAProfileDDPM> clickedEAProfileDDPM = DdpmCommonHelper.DeviceManagerSA.ReadUserEAProfileDDPM().Result;

                if (clickedEAProfileDDPM != null)
                {
                    // 找相同 ID 的 Profile ID
                    EAProfileDDPM profileToRemove = clickedEAProfileDDPM.FirstOrDefault(p => p.ID == spItem.ProfileID);//LayoutID);

                    if (profileToRemove != null)
                    {
                        _vm.currentEditprofile = profileToRemove;
                        _log.Info($"@[EzMemoryRightView] OnListViewItemEdited, Profile with ID {spItem.ProfileID} removed from UserSettings.");
                    }
                    else
                    {
                        _log.Info($"@[EzMemoryRightView] OnListViewItemEdited, Profile with ID {spItem.ProfileID} not found in UserSettings.");
                    }
                }
                else
                {
                    _log.Info($"@[EzMemoryRightView] OnListViewItemEdited, No EAProfileDDPM found in UserSettings.");
                }
            }
            catch (Exception ex)
            {
                _log.Error($"@[EzMemoryRightView] OnListViewItemEdited, Error occurred while deleting from UserSettings: {ex.Message}");
            }

            try
            {
                // Monitor Setting Edit keep data
                EasyArrangementDDPM _easyArrangementDDPM = DdpmCommonHelper.DeviceManagerSA.ReadMonitorEasyArrangement(_homeDevice.MonitorInfo).Result;

                if (_easyArrangementDDPM != null && _easyArrangementDDPM.Desktops.Count > 0)
                {
                    // 找相同 ID
                    EzProfileSettingDDPM profileSettingToRemove = _easyArrangementDDPM.Desktops[0].ProfileSettings.FirstOrDefault(ps => ps.ID == spItem.ProfileID);//LayoutID);

                    if (profileSettingToRemove != null)
                    {
                        _vm.currentEditprofileSetting = profileSettingToRemove;
                        _log.Info($"@[EzMemoryRightView] OnListViewItemEdited, ProfileSetting with Monitor Model {_homeDevice.MonitorInfo.modelName}, ID {spItem.ProfileID} removed from MonitorSettings.");
                    }
                    else
                    {
                        //Robert_Lin 2025-1-12 Create a default ProfileSetting for the editing Profile
                        _vm.currentEditprofileSetting = new EzProfileSettingDDPM(spItem.ProfileID, false, 0, false);
                        _log.Info($"@[EzMemoryRightView] OnListViewItemEdited, ProfileSetting with Monitor Model {_homeDevice.MonitorInfo.modelName}, ID {spItem.ProfileID} not found in MonitorSettings.");
                    }
                }
                else
                {
                    //Robert_Lin 2025-1-12 Create a default ProfileSetting for the editing Profile
                    _vm.currentEditprofileSetting = new EzProfileSettingDDPM(spItem.ProfileID, false, 0, false);
                    _log.Info($"@[EzMemoryRightView] OnListViewItemEdited, No valid EasyArrangementDDPM or Desktops found in MonitorSettings.");
                }
            }
            catch (Exception ex)
            {
                _log.Error($"@[EzMemoryRightView] OnListViewItemEdited, Error occurred while deleting from MonitorSettings: {ex.Message}");
            }

            _vm.OrgEditSplitItem = spItem;
            _vm.CurrentEditSelectspItem = spItem;
            _vm.IsEditProfile = true;
            //Robert_Lin 2025-1-9 add this flag so that the Profile appInfo can be Sync in AssignApp page
            _vm.IsAddPageBack = false;
            //Assign the Proile Name to be edited
            if (_vm.currentEditprofile != null)
                _vm.InputText = _vm.currentEditprofile.Name;

            EzMemoryFirst ezFirst = new EzMemoryFirst(_vmDisplay, _vm, _homeDeviceSelect);
            DdpmCommonHelper.ModuleOwner?.OpenFullView(ezFirst);
        }

        #endregion SplitItem Edit

        #region SplitItem Selection

        /// <summary>
        /// SplitItem Click event
        /// </summary>
        /// <param name="spItem">The SplitItem uesr is clicking</param>
        private void OnListViewItemClicked(SplitItem spItem)
        {
            //Robert_Lin, 2025-1-12
            //1 spItem: the user clicking SplitItem will be the new selected item, assign tp _vm.CurrentSelectspItem
            //2 Get the ProfileId from spItem
            //3 LoadEmProfileSettings(profileId): Load the ProfileSettings from UserSettings and MonitorSettings
            //  UserSettings assign to _vm.CurrentSelectedProfile; update to UI (ProfileName, AppDocument)
            //  MonitorSettings assign to _vm.CurrentSelectedProfileSetting; update to UI (AutomaticStartup, LaunchByTime)
            //4 Refresh Selection of SplitListView
            //5 Enable Apply button

            try
            {
                _log.Info($"@[EzMemoryRightView] OnListViewItemClicked ... in");
                int profileID = spItem.ProfileID;

                /*
                EAProfileDDPM emProfile = new EAProfileDDPM();
                EzProfileSettingDDPM emProfileSettings = new EzProfileSettingDDPM();

                if (_vm.LoadEmProfileSettings(profileID, ref emProfile, ref emProfileSettings))
                {
                    _vm.CurrentSelectedProfile = emProfile;
                    _vm.CurrentSelectedProfileSetting = emProfileSettings;
                    _log.Info($"@EzMemoryRightView.OnListViewItemClicked,, Profile ID={profileID}, Load user settings OK.");
                }
                else
                {
                    _vm.CurrentSelectedProfile = null;
                    _vm.CurrentSelectedProfileSetting = null;
                    _log.Info($"@EzMemoryRightView.OnListViewItemClicked, Profile ID {profileID} not found in UserSettings.");
                }
                */

                if (_vm.CurrentSelectspItem != null)
                    _vm.CurrentSelectspItem.IsSelected = false;

                _vm.CurrentSelectspItem = spItem;
                _vm.CurrentSelectspItem.IsSelected = true;

                _vm.UpdateRightViewUIFromCurrentSelectspItem();

                
                /*
                // User Setting
                List<EAProfileDDPM> clickedEAProfileDDPM = DdpmCommonHelper.DeviceManagerSA.ReadUserEAProfileDDPM().Result;
                EAProfileDDPM matchingProfile;
                if (clickedEAProfileDDPM != null)
                {
                    // 在User中找相同的 ID
                    matchingProfile = clickedEAProfileDDPM.FirstOrDefault(profile => profile.ID == spItem.ProfileID);

                    if (matchingProfile != null)
                    {
                        _vm.ProfileTitleTextBlockValue = matchingProfile.Name;
                        _vm.AppDocumentValue = string.Empty;
                        int no = 1;
                        foreach (EAAppInfoDDPM profile in matchingProfile.AppInfos)
                        {
                            _vm.AppDocumentValue += no + ".  " + profile.Name + "\r\n";
                            no++;
                        }
                        //_vm.AppDocumentValue = matchingProfile.AppInfos[0].Name;
                        _log.Info($"@[EzMemoryRightView] OnListViewItemClicked, Profile ID {matchingProfile.ID} found and updated in UserSettings.");
                    }
                    else
                    {
                        _log.Info($"@[EzMemoryRightView] OnListViewItemClicked, Profile with ID {spItem.ProfileID} not found in UserSettings.");
                        return;
                    }
                }
                else
                {
                    _log.Info($"@[EzMemoryRightView] OnListViewItemClicked, No EAProfileDDPM found in UserSettings.");
                    return;
                }

                // Monitor Setting
                EasyArrangementDDPM clickedeasyArrangementDDPM = DdpmCommonHelper.DeviceManagerSA.ReadMonitorEasyArrangement(_homeDevice.MonitorInfo).Result;

                if (clickedeasyArrangementDDPM == null)
                {
                    _vm.AutomaticStartupValue = Strings.NATextForRightViewUI;
                    _vm.LaunchByTimeValue = Strings.NATextForRightViewUI;
                    _log.Info($"@[EzMemoryRightView] OnListViewItemClicked, No MonitorSettings found for monitor {_homeDevice.MonitorInfo.DisplayName}.");
                    return;
                }
                else
                {
                    // 在 Monitor Settings 中找相同的 Profile 設定
                    EzProfileSettingDDPM profileSetting = _vm.FindProfileSettingById(clickedeasyArrangementDDPM, matchingProfile.ID);

                    if (profileSetting != null)
                    {
                        if (profileSetting.StartUpLaunch)
                        {
                            _vm.AutomaticStartupValue = Strings.Yes;                          
                        }
                        else
                        {
                            _vm.AutomaticStartupValue = Strings.No;
                        }
                        if(profileSetting.Auto)
                        {
                            _vm.LaunchByTimeValue = _vm.ConvertAutoLaunchtimeToTime(profileSetting.AutoStartTime);
                        }
                        else
                        {
                            _vm.LaunchByTimeValue = "_";
                        }
                        _log.Info($"@[EzMemoryRightView] OnListViewItemClicked, MonitorSettings updated for Profile ID {matchingProfile.ID}.");
                    }
                    else
                    {
                        // 沒有monitor setting 數值 填否 跟 _
                        _vm.AutomaticStartupValue = Strings.No;
                        _vm.LaunchByTimeValue = "_";
                        _log.Info($"@[EzMemoryRightView] OnListViewItemClicked, ProfileSetting with ID {matchingProfile.ID} not found in MonitorSettings.");
                    }
                }
                */
            }
            catch (Exception ex)
            {
                _log.Error($"@[EzMemoryRightView] OnListViewItemClicked, Error occurred: {ex.Message}. StackTrace: {ex.StackTrace}");
            }

            _vm.IsApplyEnabled = (_vm.CurrentSelectspItem != null);

            //Only sopported for ISplitCtrl SplitItem (that is, EasyArrange) should be always
            //If it's NOT a ISplitCtrl, then noting to do and return
            //if (spItem.InnerContent is not ISplitCtrl)
            //    return;

            //ISplitCtrl spCtrl = spItem.InnerContent as ISplitCtrl;

            //Set as current Selected item
            //_vm.SelectedSplitItem = spItem;
            //Robert_Lin, 2024-11-27, comment out, Don't call this method, it will change the selection of Easy Arrange to SA
            //But UI do not get the notification.
            //_vm.SetWorkSplit(spCtrl.CellCount, spCtrl.SplitKey, spCtrl.Settings);

            //Need to set it's buddy as IsSelected

            //Robert_Lin, 2025-1-9, EasyMemory do not need to check the Buddy in RecentList

            /*
            //If the selected item is in RecentList, then it should be has Buddy
            if (spItem.SplitOwner == Common.EAEM.eSplitOwner.EaRecent)
            {
                //It should has Buddy, but if not (then ignored)
                if (spItem.Buddy != null)
                {
                    //Select its Buddy also
                    spItem.Buddy.IsSelected = true;
                }
            }
            else //Selected item is not in RecentList
            {
                //Check if it has Buddy in RecentList?
                if (spItem.Buddy != null)
                {
                    //has Buddy, its Buddy should be in RecentList, make a check
                    if (spItem.Buddy.SplitOwner == Common.EAEM.eSplitOwner.EaRecent)
                    {
                        //YES, the buddy of the selected item is in Recent, list
                        //then we will move the buddy to the second position of the Recent List
                        spItem.Buddy.IsSelected = true;
                        //splitListView_Recent.MoveSelectedItemToSecondPosition();
                    }
                }
                else
                {
                    //The selected item has no Buddy in Recent list => need to add
                    SplitItem? itemRecent = AddNewItemToRecentList2ndPosition(spItem);

                    if (itemRecent != null)
                    {
                        itemRecent.IsSelected = true;
                    }
                }

            }
            */
            //if(_vm.SelectedSplitItem != null)
            //{
            //    _vm.IsApplyEnabled = true;
            //}
            ////splitListView_RecentForEzM.MoveSelectedItemToSecondPosition();
            //SaveEaSettings();
        }

        #endregion SplitItem Selection

        #region Recent List Manager
        /// <summary>
        /// Clone and add an new item into RecentList's second position.
        /// 
        /// </summary>
        /// <param name="itemSource">The item of source to be added. It should be already in other (not Recent) list.</param>
        /// <return>The new added item in RecentList</return>
        private SplitItem? AddNewItemToRecentList2ndPosition(SplitItem itemSouce)
        {
            //Validatoin 
            //1 itemSource should not in RecentList
            if (itemSouce.SplitOwner == Common.EAEM.eSplitOwner.EaRecent)
                return null;
            //2 itemSoutve should not has Buddy (in RecentList)
            if (itemSouce.Buddy != null)
                return null;

            //Robert_Lin, 2024-10-11, Redefine RecentList MaxCount: (include "Off" -> Not include "Off"), so need +1 in DDPM.UI
            //That is, RecentList from Settings file is 5 items, but UI SplitListView of Recent is "Off" + 5 RecentList => 6 items
            //
            //If the RecentList item count has up to the limitation (always be true, but we will check anyway)
            if (splitListView_RecentForEzM.ItemCount >= EAEMConstants.MaxRecentItems + 1)
            {
                //Remove the last item
                SplitItem? itemLatest = splitListView_RecentForEzM.GetLatestItem();
                //Unbound with its Buddy
                if (itemLatest != null)
                {
                    if (itemLatest.Buddy != null)
                        itemLatest.Buddy.Buddy = null;
                    splitListView_RecentForEzM.DeleteSplitItem(itemLatest);
                }
            }

            //Duplicate a SplitItem from SelectedItem, and add to 2nd position of RecentList

            //Duplicate a new item from the itemSource
            ISplitCtrl? ispSource = itemSouce.ISplitCtrl;
            if (ispSource != null) //Support ISplitCtrl (EasyArrange only)
            {
                ISplitCtrl ispRecent = ispSource.Clone();
                if (ispRecent != null)
                {
                    ispRecent.FriendlyName = itemSouce.CustomName;
                    SplitItem itemRecent = splitListView_RecentForEzM.AddSplitCtrlTo2ndPosition(ispRecent);
                    itemRecent.CustomId = itemSouce.CustomId;

                    itemSouce.Buddy = itemRecent;
                    itemRecent.Buddy = itemSouce;

                    return itemRecent;
                }
            }

            return null;
        }
        #endregion

        #region Add Profile

        /// <summary>
        /// SplitItem Add event
        /// </summary>
        /// <param name="spItem"></param>
        private void OnListViewItemAddClicked(SplitListView spItem)
        {
            _vm.IsAddPageBack = true; //If add btn trigger, it is mean do not sync any profile
            List<EAProfileDDPM> checkEAProfileDDPM = DdpmCommonHelper.DeviceManagerSA.ReadUserEAProfileDDPM().Result;
            if (checkEAProfileDDPM != null &&
                checkEAProfileDDPM.Count >= 9)
            {
                Thickness headMargin = new Thickness(24, 30, 45, 24);
                Thickness subMargin = new Thickness(24, -16, 24, 8);
                DdpmCommonHelper.DDPMEzMesssageBox(Strings.msgboxTitle, Strings.subTitle, true, Window.GetWindow(this), 417, 148, headMargin, subMargin);
                return;
            }

            //Robert_Lin 2025-1-11 Reset Editing data to defauls
            //
            //No current edit SplitItem
            _vm.CurrentEditSelectspItem = null;

            //No added apps
            _vm.ClearTextBlockAppName();
            if (_vm.currentEditprofile != null)
            {
                _vm.currentEditprofile = null;
            }

            //ProfileSetting: allocate a new default value
            _vm.currentEditprofileSetting = null;

            //Is Edit mode = false
            _vm.IsEditProfile = false;

            //Clear ProfileName to be generated a new/unused one
            _vm.InputText = string.Empty;

            //reset Launch options to
            _vm.IsManualLaunch = true;
            _vm.IsAutoLaunch = false;
            _vm.IsLaunchAtStartup = false;
            //Time will be refresh in LaunchOptions page

            EzMemoryFirst ezFirst = new EzMemoryFirst(_vmDisplay, _vm, _homeDeviceSelect);
            DdpmCommonHelper.ModuleOwner?.OpenFullView(ezFirst);
        }
        #endregion

        #region Refresh Data
        public void HandleSelectedHomeDeviceChanged()
        {
            if (DdpmCommonHelper.ModuleOwner != null)
            {
                _homeDevice = DdpmCommonHelper.ModuleOwner.SelectedHomeDevice;
                if (_homeDevice.vmEzArrange == null)
                {
                    _homeDevice.vmEzArrange = new DDPM.UI.Common.ViewModels.EzArrangeViewModel(_homeDevice);
                }
                _vm = _homeDevice.vmEzArrange;
                DataContext = _homeDevice.vmEzArrange;
                _homeDeviceSelect = _homeDevice;
            }
            CleanUpListViewItems();
            InitListViewItems();
        }
        #endregion Refresh Data

        #region Clean up SplitListView and Items
        private void CleanUpListViewItems()
        {
            splitListView_RecentForEzM.ClearList();
        }
        #endregion Clean up SplitListView and Items

        #region Init SplitListView and SplitItems
        private void InitListViewItems()
        {
            try
            {
                Screen? currentScreen = GetAttachedScreen(_homeDevice.MonitorInfo.DisplayName);
                _vm.IsVertical = (currentScreen != null) ? (currentScreen.Bounds.Width < currentScreen.Bounds.Height) : false;


                _log.Info($"@[EzMemoryRightView] InitListViewItems ... in");
                _vm.splitListRightView = splitListView_RecentForEzM;
                splitListView_RecentForEzM.SplitOwner = Common.EAEM.eSplitOwner.EaRecent;
                splitListView_RecentForEzM.ItemClickCommand = new RelayCommand<SplitItem>(OnListViewItemClicked);
                splitListView_RecentForEzM.ItemEditCommand = new RelayCommand<SplitItem>(OnListViewItemEdited);
                splitListView_RecentForEzM.ItemDeleteCommand = new RelayCommand<SplitItem>(OnListViewItemDeleted);
                splitListView_RecentForEzM.AddButtonClickCommand = new RelayCommand<SplitListView>(OnListViewItemAddClicked);
                splitListView_RecentForEzM.HasAddButton = true;
                splitListView_RecentForEzM.IsVertical = _vm.IsVertical;

                //Default Selected Profile item
                int initProfileId = _vm.SelectedProfileId;
                EAProfileDDPM? selectedProfile = _vm.CurrentSelectedProfile;

                _vm.RightViewDataClear();

                SplitItem? initSelItem = null;

                // 取得User EAProfiles
                List<EAProfileDDPM> initListViewIEAProfileDDPM = DdpmCommonHelper.DeviceManagerSA.ReadUserEAProfileDDPM().Result;

                // 1228拿掉 monitor setting 判斷
                // 取得Monitor EasyArrangement
                //EasyArrangementDDPM initListVieweasyArrangementDDPM = DdpmCommonHelper.DeviceManagerSA.ReadMonitorEasyArrangement(_homeDevice.MonitorInfo).Result;

                if (initListViewIEAProfileDDPM != null)// && initListVieweasyArrangementDDPM != null)
                {
                    // ProfileSettings 不為 null
                    //if (initListVieweasyArrangementDDPM.Desktops.Count > 0 && initListVieweasyArrangementDDPM.Desktops[0].ProfileSettings != null)
                    //{
                    foreach (var profile in initListViewIEAProfileDDPM)
                    {
                        // 在 ProfileSettings 中找是否有對應的 ID
                        //EzProfileSettingDDPM profileSetting = initListVieweasyArrangementDDPM.Desktops[0].ProfileSettings.FirstOrDefault(ps => ps.ID == profile.ID);

                        //if (profileSetting != null)
                        //{
                        //Robert_Lin, 2025-1-9, use new property to check
                        //NEW:
                        if (profile.IsCustomLayout)
                        //OLD:
                        //if (profile.Layout >= 1000)
                        {
                            SplitJson[] customList = _deviceManagerSA.ReadEACustomList().Result;
                            foreach (SplitJson spj in customList)
                            {
                                //Robert_Lin, 2025-4-23 Do not load the Overlap layout
                                if (spj.IsOverlapLayout)
                                {
                                    continue;
                                }

                                if (profile.Layout == spj.EAID)
                                {
                                    //Validate settings
                                    //Robert_Lin, 2025-1-9, EasyMemory do not need to validate these
                                    ////1 CustomId must > 0
                                    //if (spj.CustomId == 0)
                                    //    continue;
                                    ////2 CustomName cannot be empty
                                    //if (String.IsNullOrWhiteSpace(spj.CustomName))
                                    //    continue;
                                    ////3 CustomName length
                                    //if (spj.CustomName.Length > EAEMConstants.MaxCustomNameLenth)
                                    //    continue;

                                    ISplitCtrl? spCtrl = ISplitCtrl.Create(spj.CellCount, spj.SplitKey);
                                    if (spCtrl == null)
                                        continue;



                                    spCtrl.Settings = new List<double>(spj.Settings);
                                    spCtrl.SplitMode = eSplitModes.Icon;
                                    //Robert_Lin, 2025-1-9, Tooltip text show the Profile name instead of Custom name
                                    spCtrl.FriendlyName = profile.Name;
                                    //spCtrl.FriendlyName = spj.CustomName;

                                    spCtrl.EAID = spj.EAID;


                                    SplitItem item = splitListView_RecentForEzM.AddItemToList(spCtrl.UC);
                                    // splitListView_RecentForEzM.AddItemToList(spCtrl.UC);
                                    item.IsDeleteEnabled = true;
                                    item.IsEditEnabled = true;

                                    //Robert_Lin, 2025-1-9, SplitItem need ProfileID to identify the Profile
                                    item.ProfileID = profile.ID;

                                    if (item.ProfileID == initProfileId)
                                        initSelItem = item;
                                }
                            }
                        }
                        else //Preset Layout
                        {
                            // 找到才繼續處理
                            //(int cellCount, char splitKey) = _vm.ParseFromLayout(profile.Layout);
                            //ISplitCtrl? spCtrl = ISplitCtrl.Create(cellCount, splitKey);
                            ISplitCtrl? spCtrl = ISplitCtrl.Create(profile.Layout);
                            if (spCtrl != null)
                            {
                                //Robert_Lin, 2025-1-9, Tooltip text show the Profile name instead of Custom name
                                spCtrl.FriendlyName = profile.Name;
                                SplitItem item = splitListView_RecentForEzM.AddItemToList(spCtrl.UC);
                                //item.SplitOwner = Common.EAEM.eSplitOwner.EaRecent;
                                item.ProfileID = profile.ID;
                                item.IsHoverable = true;
                                item.IsDeleteEnabled = true;
                                item.IsEditEnabled = true;
                                item.LayoutID = profile.Layout;

                                if (profile.ID == initProfileId)
                                    initSelItem = item;
                            }
                        }
                        //}
                        //else
                        //{
                        //    _log.Info($"@[EzMemoryRightView] InitListViewItems: Profile ID {profile.ID} not found in MonitorSettings.");
                        //}
                    }
                    //}
                    //else
                    //{
                    //    _log.Info($"@[EzMemoryRightView] InitListViewItems: No valid ProfileSettings found in MonitorSettings.");
                    //}

                    if (initSelItem != null)
                    {
                        if (_vm.CurrentSelectspItem != null)
                        {
                            _vm.CurrentSelectspItem.IsSelected = false;
                        }
                        _vm.CurrentSelectspItem = initSelItem;
                        _vm.CurrentSelectspItem.IsSelected = true;

                        _vm.UpdateRightViewUIFromCurrentSelectspItem();
                        _vm.IsApplyEnabled = (_vm.CurrentSelectspItem != null);
                    }
                }
                else
                {
                    _log.Info($"@[EzMemoryRightView] InitListViewItems: No EAProfileDDPM or EasyArrangementDDPM found.");
                }
            }
            catch (Exception ex)
            {
                _log.Error($"@[EzMemoryRightView] InitListViewItems: Error occurred during initialization - {ex.Message}");
            }

        }

        #endregion

        //private void InitSplitListViews_Unused()
        //{
        //    //A Build WindowLists
        //    //
        //    foreach (ISplitCtrl isp in ISplitCtrl.Splits_EA)
        //    {
        //    }
        //}

        private void InitRecentListView_Unused()
        {
            ISplitCtrl? sp0A = ISplitCtrl.Create(0, 'A');
            if (sp0A != null)
            {
                sp0A.SplitMode = eSplitModes.Icon;
                SplitItem spItem0A = splitListView_RecentForEzM.AddItemToList(sp0A.UC);
                spItem0A.SplitOwner = Common.EAEM.eSplitOwner.EaRecent;
            }


        }


        #region Screen
        private Screen? GetAttachedScreen(string deviceName)
        {
            return Screen.AllScreens.FirstOrDefault(x => x.DeviceName.Equals(deviceName));
        }

        private static DisplayOrientation GetDisplayOrientation(string deviceName)
        {
            int ENUM_CURRENT_SETTINGS = -1;
            DEVMODE devMode = new DEVMODE();
            if (User32._EnumDisplaySettings(deviceName, ENUM_CURRENT_SETTINGS, ref devMode))
            {
                return (DisplayOrientation)devMode.dmDisplayOrientation;
            }
            return DisplayOrientation.Unknow;
        }
        #endregion

        //private void CheckBox_Click_1(object sender, System.Windows.RoutedEventArgs e)
        //{
        //    IConsole? console = DdpmCommonHelper.MyConsole;
        //    if (console != null)
        //    {
        //        //if (ck.IsChecked != null)
        //        //{
        //        //    var args = new EventManagerArgs();
        //        //    args.Tag = (bool)ck.IsChecked; //true=Show, false=Hide
        //        //    console.RaiseEvent(ConsoleEventNames.Masthead_ShowAddDeviceIcon, this, args);
        //        //}
        //    }
        //}

        //private void ckSettings_Click(object sender, System.Windows.RoutedEventArgs e)
        //{
        //    IConsole? console = DdpmCommonHelper.MyConsole;
        //    if (console != null)
        //    {
        //        //if (ckSettings.IsChecked != null)
        //        //{
        //        //    var args = new EventManagerArgs();
        //        //    args.Tag = (bool)ckSettings.IsChecked; //true=Show, false=Hide
        //        //    console.RaiseEvent(ConsoleEventNames.Masthead_ShowSettingsIcon, this, args);
        //        //}
        //    }
        //}
    }
}