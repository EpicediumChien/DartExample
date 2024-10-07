using DDPM.UI.Common;
using System.Windows.Controls;
using Dell.Client.Framework.UX.WPF;
using Dell.Client.Framework.Common;
using System.Windows;
using DDPM.UI.Plugin.Common.ViewModels;
using DDPM.SA.Common;
using DDPM.UI.Common.Models;
using DDPM.SA.Common.Settings;
using System.Windows.Media.Media3D;
using System.Windows.Forms;
using CommunityToolkit.Mvvm.Input;
using DDPM.UI.Common.UserControls;
using DDPM.Easy.Common;
using DDPM.SA.Common.Display;
using UserControl = System.Windows.Controls.UserControl;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Microsoft;

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
        #endregion Private Members

        //string msgboxTitle = "Error";
        //string subTitle = "You can only save up to 9 profiles. Delete an existing profile or edit it in the main menu.";

        public EzMemoryRightView(DisplayViewModel vmDisplay)
        {
            _vmDisplay = vmDisplay;
            _homeDevice = vmDisplay.SelectedHomeDevice;
            _console = vmDisplay.Console;
            _deviceManagerSA = HomeDevice.DeviceManagerSA;
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

            InitializeTextBlocks();

            Screen? currentScreen = GetAttachedScreen(_homeDevice.MonitorInfo.DisplayName);
            _vm.IsVertical = (currentScreen != null) ? (currentScreen.Bounds.Width < currentScreen.Bounds.Height) : false;

            splitListView_RecentForEzM.SplitOwner = Common.EAEM.eSplitOwner.EaRecent;
            splitListView_RecentForEzM.ItemClickCommand = new RelayCommand<SplitItem>(OnListViewItemClicked);
            splitListView_RecentForEzM.ItemEditCommand = new RelayCommand<SplitItem>(OnListViewItemEdited);
            splitListView_RecentForEzM.ItemDeleteCommand = new RelayCommand<SplitItem>(OnListViewItemDeleted);
            //_splitItemEditCommand;
            splitListView_RecentForEzM.IsVertical = _vm.IsVertical;

            List<EAProfileDDPM> rightviewEAProfileDDPM = DdpmCommonHelper.DeviceManagerSA.ReadUserEAProfileDDPM().Result;
            if (rightviewEAProfileDDPM != null)
            {
                foreach (var app in rightviewEAProfileDDPM)
                {
                    (int cellCount, char splitKey) = _vm.ParseFromLayout(app.Layout);
                    ISplitCtrl? sp0A = ISplitCtrl.Create(cellCount, splitKey);
                    SplitItem item0A;
                    sp0A.FriendlyName = "Off"; //Need Multilogual support
                    sp0A.SplitMode = eSplitModes.Icon;
                    item0A = splitListView_RecentForEzM.AddItemToList(sp0A.UC);
                    item0A.SplitOwner = Common.EAEM.eSplitOwner.EaRecent;
                    item0A.CustomId = app.ID;
                    item0A.IsHoverable = true;
                    item0A.IsDeleteEnabled = true;
                    item0A.IsEditEnabled = true;
                    item0A.LayoutID = app.ID;
                }
            }
        }

        private void InitializeTextBlocks()
        {
            AutomaticStartupTextBlock.Text = _vm.AutomaticStartupTextBlockForRightViewUI;
            LaunchByTimeTextBlock.Text = _vm.LaunchByTimeTextBlockForRightViewUI;
            AppDocumentTextBlock.Text = _vm.AppDocumentTextBlockForRightViewUI;
            applybtn.Content = _vm.applybtnForRightViewUI;
        }

        private void EzMemoryStart_Click(object sender, RoutedEventArgs e)
        {
            List<EAProfileDDPM> startEAProfileDDPM = DdpmCommonHelper.DeviceManagerSA.ReadUserEAProfileDDPM().Result;


            if (startEAProfileDDPM != null)
            {
                // 找相同 ID 的 Profile ID
                EAProfileDDPM profileTostart = startEAProfileDDPM.FirstOrDefault(p => p.ID == _vm.currenySelectspItem.LayoutID);

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

                    _deviceManagerSA.LaunchAndArrangeApps(launchApp);
                    _log.Info($"@[EzMemoryRightView] EzMemoryStart_Click, Profile with ID {_vm.currenySelectspItem.LayoutID} removed from UserSettings.");
                }
                else
                {
                    _log.Info($"@[EzMemoryRightView] EzMemoryStart_Click, Profile with ID {_vm.currenySelectspItem.LayoutID} not found in UserSettings.");
                }
            }          
        }

        private void AddNewButton_Click(object sender, RoutedEventArgs e)
        {
            List<EAProfileDDPM> checkEAProfileDDPM = DdpmCommonHelper.DeviceManagerSA.ReadUserEAProfileDDPM().Result;
            if (checkEAProfileDDPM != null)
            {
                if (checkEAProfileDDPM.Count >= 9)
                {
                    Thickness headMargin = new Thickness(24, 30, 45, 24);
                    Thickness subMargin = new Thickness(24, -16, 24, 8);
                    DdpmCommonHelper.DDPMEzMesssageBox(_vm.msgboxTitle, _vm.subTitle, true, Window.GetWindow(this), 417, 148, headMargin, subMargin);
                    return;

                }
            }
            EzMemoryFirst ezFirst = new EzMemoryFirst(_vmDisplay);
            DdpmCommonHelper.ModuleOwner?.OpenFullView(ezFirst);
        }

        #region SplitItem Delete
        private void OnListViewItemDeleted(SplitItem spItem)
        {
            try
            {
                // User Setting Delete
                List<EAProfileDDPM> clickedEAProfileDDPM = DdpmCommonHelper.DeviceManagerSA.ReadUserEAProfileDDPM().Result;

                if (clickedEAProfileDDPM != null)
                {
                    // 找相同 ID 的 Profile ID
                    EAProfileDDPM profileToRemove = clickedEAProfileDDPM.FirstOrDefault(p => p.ID == spItem.LayoutID);

                    if (profileToRemove != null)
                    {
                        clickedEAProfileDDPM.Remove(profileToRemove);
                        DdpmCommonHelper.DeviceManagerSA.WriteUserListEAProfileDDPM(clickedEAProfileDDPM);
                        _log.Info($"@[EzMemoryRightView] OnListViewItemDeleted, Profile with ID {spItem.LayoutID} removed from UserSettings.");
                    }
                    else
                    {
                        _log.Info($"@[EzMemoryRightView] OnListViewItemDeleted, Profile with ID {spItem.LayoutID} not found in UserSettings.");
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
                    EzProfileSettingDDPM profileSettingToRemove = _easyArrangementDDPM.Desktops[0].ProfileSettings.FirstOrDefault(ps => ps.ID == spItem.LayoutID);

                    if (profileSettingToRemove != null)
                    {
                        // 找到，則移除
                        _easyArrangementDDPM.Desktops[0].ProfileSettings.Remove(profileSettingToRemove);
                        DdpmCommonHelper.DeviceManagerSA.WriteMonitorEasyArrangement(_homeDevice.MonitorInfo, _easyArrangementDDPM);
                        _log.Info($"@[EzMemoryRightView] OnListViewItemDeleted, ProfileSetting with Monitor Model {_homeDevice.MonitorInfo.modelName}, ID {spItem.LayoutID} removed from MonitorSettings.");
                    }
                    else
                    {
                        _log.Info($"@[EzMemoryRightView] OnListViewItemDeleted, ProfileSetting with Monitor Model {_homeDevice.MonitorInfo.modelName}, ID {spItem.LayoutID} not found in MonitorSettings.");
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
            return;
        }

        #endregion SplitItem Delete

        #region SplitItem Edit

        private void OnListViewItemEdited(SplitItem spItem)
        {
            return;
        }
        #endregion SplitItem Edit

        #region SplitItem Selection
        private void OnListViewItemClicked(SplitItem spItem)
        {
            _vm.currenySelectspItem = spItem;

            //User Setting
            List<EAProfileDDPM> clickedEAProfileDDPM = DdpmCommonHelper.DeviceManagerSA.ReadUserEAProfileDDPM().Result;
            EAProfileDDPM matchingProfile = clickedEAProfileDDPM.FirstOrDefault(profile => profile.ID == spItem.CustomId);

            if (matchingProfile != null)
            {
                _vm.ProfileTitleTextBlockValue = matchingProfile.Name; ;
                _vm.AppDocumentValue = matchingProfile.AppInfos[0].Name;
            }
            else
            {
                return;
            }

            //Monitor Setting
            EasyArrangementDDPM clickedeasyArrangementDDPM = DdpmCommonHelper.DeviceManagerSA.ReadMonitorEasyArrangement(_homeDevice.MonitorInfo).Result;
            EzProfileSettingDDPM profileSetting = _vm.FindProfileSettingById(clickedeasyArrangementDDPM, matchingProfile.ID);
            if (clickedeasyArrangementDDPM == null)
            {
                _vm.AutomaticStartupValue = _vm.NATextForRightViewUI;
                _vm.LaunchByTimeValue = _vm.NATextForRightViewUI;
            }
            else
            {
                if(clickedeasyArrangementDDPM.Desktops != null || clickedeasyArrangementDDPM.Desktops.Count != 0)
                {
                    _vm.AutomaticStartupValue = profileSetting.Auto.ToString();
                    if (profileSetting.Auto)
                    {
                        _vm.LaunchByTimeValue = _vm.ConvertAutoLaunchtimeToTime(profileSetting.AutoStartTime);
                    }
                    else
                    {
                        _vm.LaunchByTimeValue = string.Empty;
                    }                    
                }
            }

            //Only sopported for ISplitCtrl SplitItem (that is, EasyArrange) should be always
            //If it's NOT a ISplitCtrl, then noting to do and return
            if (spItem.InnerContent is not ISplitCtrl)
                return;

            ISplitCtrl spCtrl = spItem.InnerContent as ISplitCtrl;

            //Set as current Selected item
            _vm.SelectedSplitItem = spItem;
            _vm.SetWorkSplit(spCtrl.CellCount, spCtrl.SplitKey, spCtrl.Settings);

            //Need to set it's buddy as IsSelected

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

            splitListView_RecentForEzM.MoveSelectedItemToSecondPosition();
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

            //If the RecentList item count has up to the limitation (always be true, but we will check anyway)
            if (splitListView_RecentForEzM.ItemCount >= EAEMConstants.MaxRecentItems)
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

            }
            //CleanUpListViewItems();
            //InitListViewItems();
        }
        #endregion Refresh Data

        #region Screen
        private Screen? GetAttachedScreen(string deviceName)
        {
            return Screen.AllScreens.FirstOrDefault(x => x.DeviceName.Equals(deviceName));
        }
        #endregion

        //private void CheckBox_Click_1(object sender, System.Windows.RoutedEventArgs e)
        //{
        //    IConsole? console = DdpmCommonHelper.MyConsole;
        //    if (console != null)
        //    {
        //        if  (ck.IsChecked != null)
        //        {
        //            var args = new EventManagerArgs();
        //            args.Tag = (bool)ck.IsChecked; //true=Show, false=Hide
        //            console.RaiseEvent(ConsoleEventNames.Masthead_ShowAddDeviceIcon, this, args);
        //        }
        //    }
        //}

        //private void ckSettings_Click(object sender, System.Windows.RoutedEventArgs e)
        //{
        //    IConsole? console = DdpmCommonHelper.MyConsole;
        //    if (console != null)
        //    {
        //        if (ckSettings.IsChecked != null)
        //        {
        //            var args = new EventManagerArgs();
        //            args.Tag = (bool)ckSettings.IsChecked; //true=Show, false=Hide
        //            console.RaiseEvent(ConsoleEventNames.Masthead_ShowSettingsIcon, this, args);
        //        }
        //    }
        //}
    }
}