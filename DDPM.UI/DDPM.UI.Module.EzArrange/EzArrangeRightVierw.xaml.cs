#define ENABLE_CALL_SA
//Robert_Lin, 2024-8-14, comment out the #define line if you would like to disable calling to Subagent EAPlugin
using CommunityToolkit.Mvvm.Input;
using DDPM.SA.Common;
using DDPM.SA.Common.Display;
using DDPM.SA.Common.Settings;
using DDPM.UI.Common;
using DDPM.UI.Common.Models;
using DDPM.UI.Common.UserControls;
using DDPM.UI.Plugin.Common.ViewModels;
using Dell.Client.Framework.Common;
using Dell.Client.Framework.UX.WPF;
using Microsoft;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Threading;
using Windows.Media.AppRecording;
using static DDPM.UI.Common.User32;
using UserControl = System.Windows.Controls.UserControl;
using Rect = System.Windows.Rect;
using VcpCore.Common;
using User32 = DDPM.UI.Common.User32;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using DDPM.SA.Common.Popup;
using System.Windows.Interop;
using DDPM.UI.Resources.Helper;
using System.Diagnostics;
using DDPM.Easy.Common;

namespace DDPM.UI.Module.EzArrange
{
    /// <summary>
    /// Interaction logic for EzArrangeRightVierw.xaml
    /// </summary>
    public partial class EzArrangeRightVierw : UserControl, IDisposable
    {
        #region Private Members
        private HomeDevice _homeDevice;
        private IDeviceManagerSA _deviceManagerSA;
        private DDPM.UI.Common.ViewModels.EzArrangeViewModel _vm;
        private readonly DisplayViewModel _vmDisplay;
        private readonly IConsole _console;
        //private readonly ILog _log;
        #endregion Private Members

        #region Strings
        //[EazyMemory.10]
        private string CustomListTooltipText = LangHelper.Instance["EazyMemory.10"];// "You can arrange the windows on your screen and click + icon.\r\nAlternatively, select an existing layout below and click the pencil icon to edit the layout.";
        //[Warning]
        private string msgBox_Warning = LangHelper.Instance["Warning"];//"Warning";
        //[EazyMemory.42]
        private string msgBox_EAProfileWillBeDeleted = LangHelper.Instance["EazyMemory.42"];// "The corresponding Easy Memory profile will be deleted too. Do you want to continue?";
        //[Yes]
        private string msgBox_Yes = LangHelper.Instance["Yes"];// "Yes";
        //[No]
        private string msgBox_No = LangHelper.Instance["No"];// "No";
        #endregion

        #region ctor
        public EzArrangeRightVierw(DisplayViewModel vmDisplay)
        {
            Requires.NotNull(vmDisplay, nameof(vmDisplay));

            InitializeComponent();

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
            ////Robert_Lin 2025-1-21 Debug
            //string deviceName_vmDisplay = vmDisplay.SelectedHomeDevice.MonitorInfo.DisplayName;
            //string deviceName_Helper = DdpmCommonHelper.ModuleOwner.SelectedHomeDevice.MonitorInfo.DisplayName;
            //if (!deviceName_vmDisplay.Equals(deviceName_Helper))
            //{
            //    _homeDevice = DdpmCommonHelper.ModuleOwner.SelectedHomeDevice;
            //}
            _console = vmDisplay.Console;
            _deviceManagerSA = HomeDevice.DeviceManagerSA;

            if (_homeDevice.vmEzArrange == null)
            {
                _homeDevice.vmEzArrange = new DDPM.UI.Common.ViewModels.EzArrangeViewModel(_homeDevice);
            }
            _vm = _homeDevice.vmEzArrange;
            DataContext = _homeDevice.vmEzArrange;

            Screen? currentScreen = GetAttachedScreen(_homeDevice.MonitorInfo.DisplayName);
            _vm.IsVertical = (currentScreen != null) ? (currentScreen.Bounds.Width < currentScreen.Bounds.Height) : false;


            splitListView_Recent.SplitOwner = Common.EAEM.eSplitOwner.EaRecent;
            splitListView_Custom.SplitOwner = Common.EAEM.eSplitOwner.EaCustom;
            splitListView_2w.SplitOwner = Common.EAEM.eSplitOwner.EaWin;
            splitListView_3w.SplitOwner = Common.EAEM.eSplitOwner.EaWin;
            splitListView_4w.SplitOwner = Common.EAEM.eSplitOwner.EaWin;
            splitListView_5w.SplitOwner = Common.EAEM.eSplitOwner.EaWin;
            splitListView_6w.SplitOwner = Common.EAEM.eSplitOwner.EaWin;
            splitListView_7w.SplitOwner = Common.EAEM.eSplitOwner.EaWin;

            //Robert_Lin 2025-3-20 fix CS8622 Nullability of reference types in type of parameter 'spItem' of 'void EzArrangeRightVierw.OnListViewItemClicked(SplitItem spItem)' doesn't match the target delegate 'Action<SplitItem?>' (possibly because of nullability attributes).
            splitListView_Recent.ItemClickCommand = new RelayCommand<SplitItem>(execute: OnListViewItemClicked);
            splitListView_Custom.ItemClickCommand = new RelayCommand<SplitItem>(execute:OnListViewItemClicked);
            splitListView_2w.ItemClickCommand = new RelayCommand<SplitItem>(execute:OnListViewItemClicked);
            splitListView_3w.ItemClickCommand = new RelayCommand<SplitItem>(execute:OnListViewItemClicked);
            splitListView_4w.ItemClickCommand = new RelayCommand<SplitItem>(execute:OnListViewItemClicked);
            splitListView_5w.ItemClickCommand = new RelayCommand<SplitItem>(execute:OnListViewItemClicked);
            splitListView_6w.ItemClickCommand = new RelayCommand<SplitItem>(execute: OnListViewItemClicked);
            splitListView_7w.ItemClickCommand = new RelayCommand<SplitItem>(execute: OnListViewItemClicked);

            splitListView_Custom.ItemEditCommand = new RelayCommand<SplitItem>(execute:HandleSplitItemEditCommand);
            splitListView_2w.ItemEditCommand = new RelayCommand<SplitItem>(execute: HandleSplitItemEditCommand);
            splitListView_3w.ItemEditCommand = new RelayCommand<SplitItem>(execute: HandleSplitItemEditCommand);
            splitListView_4w.ItemEditCommand = new RelayCommand<SplitItem>(execute: HandleSplitItemEditCommand);
            splitListView_5w.ItemEditCommand = new RelayCommand<SplitItem>(execute: HandleSplitItemEditCommand);
            splitListView_6w.ItemEditCommand = new RelayCommand<SplitItem>(execute: HandleSplitItemEditCommand);
            splitListView_7w.ItemEditCommand = new RelayCommand<SplitItem>(execute: HandleSplitItemEditCommand);

            splitListView_Custom.ItemDeleteCommand = new RelayCommand<SplitItem>(execute: HandleSplitItemDeleteCommand);
            splitListView_Custom.HasAddButton = true;
            splitListView_Custom.AddButtonClickCommand = new RelayCommand<SplitListView>(execute:HandleAddButtonClickCommand);

            //InitRecentListView();
            InitListViewItems();

            customListTooltipText.Text = CustomListTooltipText;
        }
        #endregion ctor

        #region UI Init / Exit
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            //InitRecentListView();
            //CleanUpListViewItems();
            //InitListViewItems();

            //Robert_Lin, 2025-1-7, the DDPMDebug.txt solution will be removed, use DevSettings instaed.
            //NEW:
            if (DevSettings.IsEASaveSplitCtrlsToPngFilesButtonEnabled())
            //OLD
            //if (IniReadInt("DDPMDebug", "EzArrange.SaveSplitCtrlsToPngFilesButtonEnabled", 0, @"C:\temp\DDPMDebug.txt") == 1)
            {
                saveSplitCtrlsToPngImagesButton.Visibility = Visibility.Visible;
            }

        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                splitListView_Recent.Dispose();
                splitListView_Custom.Dispose();
                splitListView_2w.Dispose();
                splitListView_3w.Dispose();
                splitListView_4w.Dispose();
                splitListView_5w.Dispose();
                splitListView_6w.Dispose();
                splitListView_7w.Dispose();
            }
        }

        ~EzArrangeRightVierw()
        {
            Dispose(false);
        }
        #endregion

        #region Init SplitListView and SplitItems
        private void InitListViewItems()
        {
            Stopwatch sw = Stopwatch.StartNew();
            _vm.LogInfo("EzArrangeRightVierw.InitListViewItems() Start.");

            //0. Prepare
            //
            if (_deviceManagerSA == null)
            {
                _vm.LogInfo("EzArrangeRightVierw.InitListViewItems() exit, DeviceManagerSA is null.");
                return;
            }



            Screen? currentScreen = GetAttachedScreen(_homeDevice.MonitorInfo.DisplayName);
            _vm.IsVertical = (currentScreen != null) ? (currentScreen.Bounds.Width < currentScreen.Bounds.Height) : false;

            //Robert_Lin 2025-4-10, for some monitors which are not support DisplayOrientation (for example, a window of a PBP mode)
            //We will get Angle0. So the IsVertical flag will be determined by Screen.Bound as above, comment out below code.
            //
            //DisplayOrientation orient = GetDisplayOrientation(_homeDevice.MonitorInfo.DisplayName);
            //_vm.IsVertical = (orient == DisplayOrientation.Angle90) || (orient == DisplayOrientation.Angle270);
            //_vm.LogInfo($"@InitListViewItems, GetDisplayOrientation() return {orient}, IsVertical={_vm.IsVertical}");

            //Update IsVertical to listViews
            splitListView_Recent.IsVertical = _vm.IsVertical;
            splitListView_Custom.IsVertical = _vm.IsVertical;
            splitListView_2w.IsVertical = _vm.IsVertical;
            splitListView_3w.IsVertical = _vm.IsVertical;
            splitListView_4w.IsVertical = _vm.IsVertical;
            splitListView_5w.IsVertical = _vm.IsVertical;
            splitListView_6w.IsVertical = _vm.IsVertical;
            splitListView_7w.IsVertical = _vm.IsVertical;

            //A Build WindowLists and add Preset layouts
            //
            //Robert_Lin 2024-12-7, DDPMW-866 Note.
            // Easy arrange window arrangement preset limited to 4 windows for all displays
            // below 19 inches in size (Reference: MDDM-3039)
            //Robert_Lin, 2024-12-11 update, for the smaller monitor,
            // all SpliItems will be added, but hide these SplitListViews
            //i
            float monitorSize = _homeDevice.MonitorInfo.edid.Size;
            bool isSmallSizeMonitor = monitorSize < 19.0000;
            _vm.LogInfo($"DDPMW-866(MDDM-3039): Easy arrange window arrangement preset limited to 4 windows for all displays   below 19 inches in size. EDID.MonitorSize={monitorSize}");


            try
            {
                _vm.LogInfo("Building preset layout ListViews - Start");
                sw.Restart();
                //Robert_Lin 2025-4-15 "Splits_EA" has been changed to AllSplitCtrls
                //foreach (ISplitCtrl spCtrl in ISplitCtrl.Splits_EA)
                foreach (ISplitCtrl spCtrl in ISplitCtrl.AllSplitCtrls)
                {
                    SplitItem? spItem = null;
                    //Reused
                    ISplitCtrl newSplit = spCtrl;
                    //ISplitCtrl newSplit = spCtrl.New();
                    if (newSplit == null)
                        continue;
                    newSplit.SplitMode = eSplitModes.Icon;
                    newSplit.IsVertical = _vm.IsVertical;

                    //Robert_Lin, 2024-12-11 adde all SplitItems, but hide the SplitItems more than 4 later
                    //
                    //if (isSmallSizeMonitor)
                    //{
                    //    if (newSplit.CellCount > 4)
                    //        continue;
                    //}
                    switch (newSplit.CellCount)
                    {
                        case 2:
                            spItem = splitListView_2w.AddItemToList(newSplit.UC);
                            break;

                        case 3:
                            spItem = splitListView_3w.AddItemToList(newSplit.UC);
                            break;
                        case 4:
                            spItem = splitListView_4w.AddItemToList(newSplit.UC);
                            break;

                        case 5:
                            spItem = splitListView_5w.AddItemToList(newSplit.UC);
                            break;

                        case 6:
                            spItem = splitListView_6w.AddItemToList(newSplit.UC);
                            break;

                        case 7:
                            spItem = splitListView_7w.AddItemToList(newSplit.UC);
                            break;

                        default:
                            break;
                    }
                    if (spItem != null)
                    {
                        spItem.IsEditEnabled = true;
                        spItem.SplitOwner = Common.EAEM.eSplitOwner.EaWin;
                    }
                } //foreach(ISplitCtrl spCtrl in ISplitCtrl.Splits_EA)
                sw.Stop();
                _vm.LogInfo($"Building preset layout ListViews - done, elapsed={sw.ElapsedMilliseconds} msec.");
            }
            catch (Exception exA1)
            {
                _vm.LogInfo("Building preset layout ListViews - Exception", exA1);
            }


            //B Load CustomList from settings file
            //

            try
            {
                _vm.LogInfo("Building custom layout ListView - Start");
                sw.Restart();
                //Robert_Lin, 2024-12-7, CustomId is not used anymore (use EAID instead)
                //So the check for CustomId will be comment-out
                //
                SplitJson[] customList = _deviceManagerSA.ReadEACustomList().Result;
                sw.Stop();
                _vm.LogInfo($"Read custom layouts from per-user settings, elapsed={sw.ElapsedMilliseconds} msec. IsCustomLayoutExist={(customList != null)}");
                if (customList != null)
                {
                    _vm.LogInfo($"Adding custom layouts into ListView, item count={customList.Length} - Start");
                    int idxAddedCustom = 0;
                    sw.Restart();
                    //Add saved custom list to custom list view
                    foreach (SplitJson spj in customList)
                    {
                        idxAddedCustom++;
                        //Validate settings
                        //1 CustomId must > 0
                        //if (spj.CustomId == 0)
                        //{
                        //    _vm.LogInfo($"  * InitListViewItems({_homeDevice.MonitorInfo?.modelName},{_homeDevice.MonitorInfo?.edid.ServiceTag}) Settings.CustomList[{spj.CellCount}{spj.SplitKey}], CustomId=[{spj.CustomId}], CustomName=[{spj.CustomName}], Msg=[Invalid setting, CustomId is zero]");
                        //    continue;
                        //}
                        //2 CustomName cannot be empty
                        //Robert_Lin, 2024-12-7,
                        //  DDM allow empty CustomName to be loaded. so comment-out below checking
                        //if (String.IsNullOrWhiteSpace(spj.CustomName))
                        //{
                        //    _vm.LogInfo($"  * InitListViewItems({_homeDevice.MonitorInfo?.modelName},{_homeDevice.MonitorInfo?.edid.ServiceTag}) Settings.CustomList[{spj.CellCount}{spj.SplitKey}], CustomId=[{spj.CustomId}], CustomName=[{spj.CustomName}], Msg=[Invalid setting, CustomName is empty]");
                        //    continue;
                        //}
                        //3 CustomName length
                        // Robert_Lin, 2024-12-7, maximum length checking is required by SDL.
                        if (spj.CustomName.Length > EAEMConstants.MaxCustomNameLenth)
                        {
                            _vm.LogInfo($"  * InitListViewItems({_homeDevice.MonitorInfo?.modelName},{_homeDevice.MonitorInfo?.edid.ServiceTag}) Settings.CustomList[{spj.CellCount}{spj.SplitKey}], CustomId=[{spj.CustomId}], CustomName=[{spj.CustomName}], Msg=[Invalid setting, CustomName length is invalid]");
                            continue;
                        }

                        _vm.LogInfo($"CustomLayout[{idxAddedCustom}]: Class={spj.CellCount}{spj.SplitKey}, EAID={spj.EAID}, CustomName=[{spj.CustomName}]");

                        ISplitCtrl? spCtrl = ISplitCtrl.Create(spj.CellCount, spj.SplitKey);
                        if (spCtrl == null)
                        {
                            _vm.LogInfo($"  * InitListViewItems({_homeDevice.MonitorInfo?.modelName},{_homeDevice.MonitorInfo?.edid.ServiceTag}) Settings.CustomList[{spj.CellCount}{spj.SplitKey}], EAID=[{spj.EAID}], CustomName=[{spj.CustomName}], Msg=[Invalid CellName + SplitKey]");
                            continue;
                        }
                        spCtrl.Settings = new List<double>(spj.Settings);
                        spCtrl.SplitMode = eSplitModes.Icon;
                        spCtrl.FriendlyName = spj.CustomName;
                        spCtrl.EAID = spj.EAID;

                        if (spCtrl.IsAddedCustomLayout)
                        {
                            Rect unitRect = new Rect(0,0, 1, 1);
                            SplitCtrl0B spc0B = (SplitCtrl0B)spCtrl;
                            spc0B.ApplySettingsToCellList(unitRect);
                            //CreateCellBorderListToSplitCtrlFromCellJsons(spj.Cells, ref spCtrl);
                        }

                        SplitItem itemCustom = splitListView_Custom.AddItemToList(spCtrl.UC);
                        itemCustom.SplitOwner = Common.EAEM.eSplitOwner.EaCustom;
                        itemCustom.CustomId = (int)spj.CustomId;

                        //Robert_Lin, 2024-10-4 add max items check
                        if (splitListView_Custom.ItemCount >= EAEMConstants.MaxCustomItems)
                            break;

                    } //foreach (SplitJson spj in customList)
                    sw.Stop();
                    _vm.LogInfo($"Adding custom layouts into ListView - done, elapsed={sw.ElapsedMilliseconds} msec.");
                } //if (customList != null)
            }
            catch (Exception exB1)
            {
                _vm.LogInfo("Building custom layout ListView - Exception", exB1);
            }

            //C Load & Build Recent List
            //

            EAMonitorSettings? eaSettings = null;
            //Used to set flag if the Resecnt items are different with Custom items
            // Case: DUT1 Edit/Change a Custom layout "A", switch to DUT2,
            // DUT2's Custom list are reloaded from the saved custom list
            // But DUT2's custom layout "A" in recent list settings file not been refreshed
            bool isRecentListChangedByCustomSettingsFile = false;
            _vm.LogInfo("Building recent layout ListView - Start");

            try
            {

                _vm.LogInfo("Add EmptyLayout item into recent layout ListView - Start");
                sw.Restart();
                //Use to trace count of slected item
                int selectedCount = 0;

                //C01. Add "Off" SplitCtrl0A as the first item of RecentList
                ISplitCtrl? sp0A = ISplitCtrl.Create(0, 'A');
                SplitItem item0A;

                if (sp0A != null)
                {
                    //sp0A.FriendlyName = "Off"; //Need Multilogual support
                    sp0A.SplitMode = eSplitModes.Icon;
                    item0A = splitListView_Recent.AddItemToList(sp0A.UC);
                    item0A.SplitOwner = Common.EAEM.eSplitOwner.EaRecent;

                    //if (eaSettings.SelectedSplit.CellCount == 0)
                    //{
                    //    item0A.IsSelected = true;
                    //    _vm.SelectedSplitItem = item0A;
                    //    selectedCount++;
                    //}
                    sw.Stop();
                    _vm.LogInfo($"Add EmptyLayout item into recent layout ListView - done, elapsed={sw.ElapsedMilliseconds} msec.");
                }
                else
                {
                    sw.Stop();
                    _vm.LogInfo("Add EmptyLayout item into recent layout ListView - ERROR, fail to create the EmptyLayouy.");
                }

                //C02. If saved recent list is not empty, then add them into Recent listview
                _vm.LogInfo("Calling to ReadEAMonitorSettings() - Start");
                sw.Restart();
                eaSettings = _deviceManagerSA.ReadEAMonitorSettings(_homeDevice.MonitorInfo).Result;
                sw.Stop();
                _vm.LogInfo($"ReadEAMonitorSettings() done. Elapsed={sw.ElapsedMilliseconds} msec. Result={(eaSettings != null)}");

                if ((eaSettings != null) && (eaSettings?.RecentList != null))
                {
                    _vm.LogInfo($"Add saved Recent layouts to ListView, itemCount={eaSettings.RecentList.Length}");
                    int idxRecentList = 0;
                    foreach (SplitJson spj in eaSettings?.RecentList)
                    {
                        idxRecentList++;
                        //Robert_Lin, 2024-10-4 Check maximun items
                        //since sp0A is not null,the splitListView_Recent.ItemCount init with 1.
                        if (splitListView_Recent.ItemCount > EAEMConstants.MaxRecentItems)
                            break;

                        if (spj.EAID >= EAEMConstants.EAID_FirstCustom)
                            _vm.LogInfo($"RecentLayout[{idxRecentList}]: Class={spj.CellCount}{spj.SplitKey}, EAID={spj.EAID}");
                        else
                            _vm.LogInfo($"RecentLayout[{idxRecentList}]: Class={spj.CellCount}{spj.SplitKey}, CustomId={spj.CustomId}, CustomName=[{spj.CustomName}]");

                        //Validate RectentList items, skip the invalid items
                        //1 CustomId=0 and CustomName is empty is invalid
                        //if ((spj.CustomId == 0) && (!String.IsNullOrWhiteSpace(spj.CustomName)))
                        //{
                        //    _vm.LogInfo($"  * InitListViewItems({_homeDevice.MonitorInfo?.modelName},{_homeDevice.MonitorInfo?.edid.ServiceTag}) Settings.RecentList[{spj.CellCount}{spj.SplitKey}], CustomId=[{spj.CustomId}], CustomName=[{spj.CustomName}], Msg=[Invalid setting, CustomId is zero]");
                        //    idxRecentList++;
                        //    continue;
                        //}

                        //Add CustomName for the default RecentList item which is created by EAPlugin
                        //Robert_Lin, 2024-12-7, DDM v2 allow save/load a custom layout which CustomName is empty
                        //if (String.IsNullOrWhiteSpace(spj.CustomName))
                        //{
                        //    ISplitCtrl? ispGetName = ISplitCtrl.Create(spj.CellCount, spj.SplitKey);
                        //    spj.CustomName = ispGetName.FriendlyName;
                        //}

                        //2 All Recent item must has Buddy
                        SplitItem? itemBuddy = null;
                        //If the recent item is a preset layout
                        //Robert_Lin, 2024-12-7, CustomId will be unused, change to EAID instead
                        //  EAID [1~49]=> Preset layout; [1000~1004]=> Custom layout
                        if (spj.EAID < EAEMConstants.EAID_FirstCustom)
                        //if (spj.CustomId == 0)
                        {
                            //Find Buddy from WinLists
                            itemBuddy = FindSplitItemFromWindowLists(spj.CellCount, spj.SplitKey);
                        }
                        else
                        {
                            //Find Buddy from CustomList
                            //itemBuddy = splitListView_Custom.FindItemByCustomId(spj.CustomId);
                            itemBuddy = splitListView_Custom.FindItemByEAID(spj.EAID);

                            //Check if Buddy (custom item) has the same settings with recent
                            if (itemBuddy != null)
                            {
                                //If any different between Recent item and its Buddy in Custom list

                                if ((itemBuddy.CellCount != spj.CellCount) || (itemBuddy.SplitKey != spj.SplitKey) ||
                                    (!itemBuddy.CustomName.Equals(spj.CustomName)) ||
                                    (!DDPM.SA.Common.Display.SplitJson.AreSettingsEqual(itemBuddy.Settings, spj.Settings)))
                                {
                                    //Copy properties from Custom to Recent
                                    spj.CellCount = itemBuddy.CellCount;
                                    spj.SplitKey = itemBuddy.SplitKey;
                                    spj.CustomName = itemBuddy.CustomName;

                                    if (itemBuddy.Settings != null)
                                    {
                                        spj.Settings = new List<double>(itemBuddy.Settings);
                                    }

                                    isRecentListChangedByCustomSettingsFile = true;
                                }
                            }
                        }
                        //If cannot find a Buddy, then will be discard
                        if (itemBuddy == null)
                        {
                            _vm.LogInfo($"  * InitListViewItems({_homeDevice.MonitorInfo?.modelName},{_homeDevice.MonitorInfo?.edid.ServiceTag}) Settings.RecentList[{spj.CellCount}{spj.SplitKey}], EAID=[{spj.EAID}], CustomName=[{spj.CustomName}], Msg=[Cannot find Buddy]");
                            //idxRecentList++;
                            continue;
                        }

                        //Create a Recent item, and add to RecentList
                        //
                        ISplitCtrl? spCtrl = ISplitCtrl.Create(spj.CellCount, spj.SplitKey);
                        if (spCtrl == null)
                        {
                            _vm.LogInfo($"  * InitListViewItems({_homeDevice.MonitorInfo?.modelName},{_homeDevice.MonitorInfo?.edid.ServiceTag}) Settings.RecentList[{spj.CellCount}{spj.SplitKey}], EAID=[{spj.EAID}], CustomName=[{spj.CustomName}], Msg=[Fail to create ISplitCtrl]");
                            //idxRecentList++;
                            continue;
                        }
                        if (spj.Settings == null)
                            spCtrl.Settings = new List<double>();
                        else
                            spCtrl.Settings = new List<double>(spj.Settings);
                        spCtrl.SplitMode = eSplitModes.Icon;
                        spCtrl.FriendlyName = spj.CustomName;
                        spCtrl.EAID = spj.EAID;

                        if (spCtrl.IsOverlapCustomLayout)
                        {
                            Rect unitRect = new Rect(0, 0, 1, 1);
                            SplitCtrl0B spc0B = (SplitCtrl0B)spCtrl;
                            spc0B.ApplySettingsToCellList(unitRect);
                            //CreateCellBorderListToSplitCtrlFromCellJsons(spj.Cells, ref spCtrl);
                        }
                        SplitItem itemRecent = splitListView_Recent.AddItemToList(spCtrl.UC);
                        itemRecent.SplitOwner = Common.EAEM.eSplitOwner.EaRecent;
                        itemRecent.CustomId = spj.CustomId;

                        //bool isSelected = (eaSettings.SelectedSplit.CellCount == spj.CellCount) &&
                        //    (eaSettings.SelectedSplit.SplitKey == spj.SplitKey);

                        //Setup Buddy
                        //

                        //If it's a Window item
                        if (spj.CustomId == 0)
                        { // do same thing?
                            itemRecent.Buddy = itemBuddy;
                            itemBuddy.Buddy = itemRecent;
                        }
                        else //It's a Custom item
                        { // do same thing?
                            itemRecent.Buddy = itemBuddy;
                            itemBuddy.Buddy = itemRecent;
                        }

                        //Setup IsSelected flag
                        bool isSelected = spj.IsEquals(eaSettings.SelectedSplit);
                        //Validate Selected Count
                        if (isSelected)
                        {
                            selectedCount++;
                            if (selectedCount > 1)
                            {
                                _vm.LogInfo($"  * InitListViewItems({_homeDevice.MonitorInfo?.modelName},{_homeDevice.MonitorInfo?.edid.ServiceTag}) Settings.RecentList[{spj.CellCount}{spj.SplitKey}], EAID=[{spj.EAID}], CustomName=[{spj.CustomName}], Msg=[SelectedCount>1]");

                                //Add to list but do not set it as Selected
                                isSelected = false;
                            }
                        }
                        //itemRecent.IsSelected = isSelected;
                        //itemBuddy.IsSelected = isSelected;
                        //_vm.SelectedSplitItem = itemBuddy;

                        idxRecentList++;
                    } //foreach
                } //if (eaSettings?.RecentList != null)
                else
                {
                    _vm.LogInfo($"RecentList is empty, will add app default list later.");
                }
            }
            catch (Exception exC1)
            {

                _vm.LogInfo("Building recent layout ListView - Exception", exC1);
            }


            //C1 Complement Recent List to 5 items
            //
            //Return the item count has been added into RecentList
            int addCount = ComplementRecentListItem();


            //D Add all custom items which has no Buddy into Recent list
            //
            /*
            foreach (SplitItem itemCustom in splitListView_Custom.SplitList)
            {
                //Robert_Lin, 2024-10-4 Check maximun items
                if (splitListView_Recent.ItemCount >= EAEMConstants.MaxRecentItems+1)
                    break;

                if (itemCustom.Buddy == null)
                {
                    //Duplicate a new SplitItem as item Buddy and add to ListRecent
                    ISplitCtrl? ispCustom = itemCustom.ISplitCtrl;
                    if (ispCustom == null)
                        continue;

                    ISplitCtrl? ispRecent = ispCustom.New();
                    ispRecent.Settings = ispCustom.Settings;
                    ispRecent.FriendlyName = ispCustom.FriendlyName;
                    ispRecent.SplitMode = eSplitModes.Icon;

                    SplitItem itemRecent = splitListView_Recent.AddItemToList(ispRecent.UC);
                    itemRecent.Buddy = itemCustom;
                    itemCustom.Buddy = itemRecent;
                    itemRecent.CustomId = itemCustom.CustomId;

                    _vm.LogInfo($"  * InitListViewItems({_homeDevice.MonitorInfo?.modelName},{_homeDevice.MonitorInfo?.edid.ServiceTag}) CustomList[{ispCustom.CellCount}{ispCustom.SplitKey}], CustomId={itemCustom.CustomId}, CustomName=[{itemCustom.CustomName}], No Buddy setup to RecentList");
                }
            }
            */

            //E Add all Window items which has no buddy into recent list
            /*
            foreach (SplitItem itemWin in splitListView_2w.SplitList)
            {
                //Robert_Lin, 2024-10-4 Check maximun items
                if (splitListView_Recent.ItemCount >= EAEMConstants.MaxRecentItems + 1)
                    break;

                if (itemWin.Buddy == null)
                {
                    //Duplicate a new SplitItem as item Buddy and add to ListRecent
                    ISplitCtrl? ispWin = itemWin.ISplitCtrl;
                    ISplitCtrl? ispRecent = ispWin.New();
                    ispRecent.Settings = ispWin.Settings;
                    ispRecent.FriendlyName = ispWin.FriendlyName;
                    ispRecent.SplitMode = eSplitModes.Icon;

                    SplitItem itemRecent = splitListView_Recent.AddItemToList(ispRecent.UC);
                    itemRecent.Buddy = itemWin;
                    itemWin.Buddy = itemRecent;
                    itemRecent.CustomId = itemWin.CustomId;
                }
            }
            foreach (SplitItem itemWin in splitListView_3w.SplitList)
            {
                //Robert_Lin, 2024-10-4 Check maximun items
                if (splitListView_Recent.ItemCount >= EAEMConstants.MaxRecentItems + 1)
                    break;
                if (itemWin.Buddy == null)
                {
                    //Duplicate a new SplitItem as item Buddy and add to ListRecent
                    ISplitCtrl? ispWin = itemWin.ISplitCtrl;
                    ISplitCtrl? ispRecent = ispWin.New();
                    ispRecent.Settings = ispWin.Settings;
                    ispRecent.FriendlyName = ispWin.FriendlyName;
                    ispRecent.SplitMode = eSplitModes.Icon;

                    SplitItem itemRecent = splitListView_Recent.AddItemToList(ispRecent.UC);
                    itemRecent.Buddy = itemWin;
                    itemWin.Buddy = itemRecent;
                    itemRecent.CustomId = itemWin.CustomId;
                }
            }
            foreach (SplitItem itemWin in splitListView_4w.SplitList)
            {
                //Robert_Lin, 2024-10-4 Check maximun items
                if (splitListView_Recent.ItemCount >= EAEMConstants.MaxRecentItems + 1)
                    break;
                if (itemWin.Buddy == null)
                {
                    //Duplicate a new SplitItem as item Buddy and add to ListRecent
                    ISplitCtrl? ispWin = itemWin.ISplitCtrl;
                    ISplitCtrl? ispRecent = ispWin.New();
                    ispRecent.Settings = ispWin.Settings;
                    ispRecent.FriendlyName = ispWin.FriendlyName;
                    ispRecent.SplitMode = eSplitModes.Icon;

                    SplitItem itemRecent = splitListView_Recent.AddItemToList(ispRecent.UC);
                    itemRecent.Buddy = itemWin;
                    itemWin.Buddy = itemRecent;
                    itemRecent.CustomId = itemWin.CustomId;
                }
            }
            */

            //Setup the Selected Item
            if (eaSettings != null)
            {
                SplitItem? itemSelected = splitListView_Recent.FindItemBySplitJson(eaSettings.SelectedSplit);
                if (itemSelected != null)
                {
                    _vm.SelectedSplitItem = itemSelected;
                }
                else
                {
                    _vm.SelectedSplitItem = splitListView_Recent.GetAt(0);
                }
            }

            //Workaround, if RecentList[0] is not selected layout, then let ite move to 2nd position 
            splitListView_Recent.MoveSelectedItemToSecondPosition();

            //Check if RecentList has been modified
            if ((addCount > 0) || isRecentListChangedByCustomSettingsFile)
            {
                SaveEaSettings();
            }
            sw.Stop();
            _vm.LogInfo($"@InitListViewItems() Elapsed time: {sw.ElapsedMilliseconds} msec.");
            _vm.LogInfo("");
        }

        private void CreateCellBorderListToSplitCtrlFromCellJsons(CellJson[] cellJsons, ref ISplitCtrl ispCtrl)
        {
            if (!ispCtrl.IsOverlapCustomLayout)
                return;

            SplitCtrl0B spCtrl0B = (SplitCtrl0B)ispCtrl;
            spCtrl0B.CellList.Clear();
            if (spCtrl0B.CellBorders != null)
                spCtrl0B.CellBorders.Clear();
            else
                spCtrl0B.CellBorders = new List<CellBorder>();

            foreach (CellJson cellJson in cellJsons)
            {
                CellBorder cellBorder = new CellBorder();
                cellBorder.CellName = cellJson.Name;
                cellBorder.rcRatio = new Rect(cellJson.x, cellJson.y, cellJson.w, cellJson.h);
                spCtrl0B.CellBorders.Add(cellBorder);

                CellObj cellObj = new CellObj(cellJson.Name);
                cellObj.rcRatio = new Rect(cellJson.x, cellJson.y, cellJson.w, cellJson.h);
                spCtrl0B.CellList.Add(cellObj);
            }

        }

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
                SplitItem spItem0A = splitListView_Recent.AddItemToList(sp0A.UC);
                spItem0A.SplitOwner = Common.EAEM.eSplitOwner.EaRecent;
            }


        }

        #endregion

        #region Clean up SplitListView and Items
        private void CleanUpListViewItems()
        {
            splitListView_Recent.ClearList();
            splitListView_Custom.ClearList();
            splitListView_2w.ClearList();
            splitListView_3w.ClearList();
            splitListView_4w.ClearList();
            splitListView_5w.ClearList();
            splitListView_6w.ClearList();
            splitListView_7w.ClearList();
        }
        #endregion Clean up SplitListView and Items

        #region SplitItem Selection
        private void OnListViewItemClicked(SplitItem spItem)
        {
            //Only sopported for ISplitCtrl SplitItem (that is, EasyArrange) should be always
            //If it's NOT a ISplitCtrl, then noting to do and return
            if (spItem.InnerContent is not ISplitCtrl)
                return;

            ISplitCtrl spCtrl = spItem.InnerContent as ISplitCtrl;

            //Set as current Selected item
            _vm.SelectedSplitItem = spItem;
            //_vm.SetWorkSplit(spCtrl.CellCount, spCtrl.SplitKey, spCtrl.Settings);
            _vm.NotifySelectedLayoutChangedToSA();

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

            splitListView_Recent.MoveSelectedItemToSecondPosition();
            SaveEaSettings();
        }
        #endregion SplitItem Selection

        #region Edit Layout
        /// <summary>
        /// The event handler when the 'pencil' icon is clicked on the SplitItem
        /// </summary>
        /// <param name="spItem"></param>
        private void HandleSplitItemEditCommand(SplitItem spItem)
        {
            if (spItem.InnerContent is ISplitCtrl spCtrl)
            {
                //ISplitCtrl spCtrl = spItem.InnerContent as ISplitCtrl;

                if (_deviceManagerSA != null)
                {
                    //Robert_Lin, 2024-8-4 Use new interface
                    //OLD:
                    //_deviceManagerSA.RequestEditSplit(_homeDevice.MonitorInfo, spCtrl.CellCount, spCtrl.SplitKey,
                    //    spCtrl.FriendlyName, spCtrl.Settings);
                    //_deviceManagerSA.EAEditStarted += _deviceManagerSA_EAEditStarted;
                    //_deviceManagerSA.EAEditCompleted += _deviceManagerSA_EAEditCompleted;
                    //SplitJson spj = SplitJson.CreateFromSplitItem(spCtrl);
                    //_deviceManagerSA.RequestEditSplit(spj);
                    //
                    //NEW:
                    // 1 DDPM.UI call to EAEditCommand(monitorInfo, EAArgs)
                    // 2 DDPM.SA.EAPlugin will handle this command
                    //   If it's not available to handle (for example, it's already in Edit mode)
                    //   then will return false.
                    //   Others will invoke a UI thread to handle the command, and return true.
                    // 3 DDPM.SA (UI thread) will try the create a EditWindow to serve the EditCommand.
                    //   If it failed (for example, invalid argument values)
                    //      it will signal a 'EditStart<string>' event and the <string> contains the error message.
                    //   Otherwise, when it shows the EditWindow
                    //      it will signal a 'EditStart<string> event with <string> is empty ("").
                    //4 DDPM.UI get the EditStart' event and <sting> is empty, then DDPM.UI should be
                    //  minimize itselft (based on UI team's requirements), until a 'EditReturn' event.

                    //Get the CustomName list and the index to the first unused name
                    int idxFirstUnused = 0;
                    List<string> customNameList = GenerateCustomNames(out idxFirstUnused);

                    //index to friendlyNameList of current editing
                    //Case_1: edit a preset layout
                    //  selectedIndex <- idxFirstUnused
                    //Case_2: edit a custom layout
                    //  selectedIndex <- the name of clicking
                    int selectedIndex = idxFirstUnused;
                    //If Case_2, edit a custom layout
                    if (spItem.SplitOwner == Common.EAEM.eSplitOwner.EaCustom)
                    {
                        //Use the CustomName to find the index to friendlyNameList
                        selectedIndex = customNameList.IndexOf(spItem.CustomName);
                        //If not found (should be never
                        if (selectedIndex < 0)
                        {
                            selectedIndex = 0; //will replace to the first one
                        }
                    }

                    //If (idxFirstUnused<0), that is CustomList is full, then sel the selectedIndex to 0.
                    if ((idxFirstUnused < 0) || (selectedIndex >= customNameList.Count))
                    {
                        selectedIndex = 0;
                    }

                    //Create a default EAArgs, for pre-defined layout
                    EAArgs args = new EAArgs()
                    {
                        Command = "EditCommnd",
                        CustomNames = customNameList,
                        CellCount = spCtrl.CellCount,
                        SplitKey = spCtrl.SplitKey,
                        CustomId = spItem.CustomId,
                        CustomName = customNameList[selectedIndex],
                        Settings = spCtrl.Settings

                    };
                    args.SplitJson = new SplitJson()
                    {
                        CellCount = spCtrl.CellCount,
                        SplitKey = spCtrl.SplitKey,
                        CustomId = spItem.CustomId,
                        CustomName = customNameList[selectedIndex],
                        Settings = new List<double>(spCtrl.Settings),
                        EAID = spCtrl.EAID,
                        Cells = GetCellsFromISplitCtrl(spCtrl)
                    };
                    //If editing SplitItem is NOT a Pre-defined layout (edit from CustomList)
                    if (spItem.CustomId != 0)
                    {
                        //CustomId=spItem.CustomId, CustomName=spCtrl.FriendlyName
                    }
#if ENABLE_CALL_SA

                    //Register a event handler for EditStarted event
                    _deviceManagerSA.EAEditStarted += _deviceManagerSA_EAEditStarted;
                    bool isSaAccepted = _deviceManagerSA.EAEditCommand(_homeDevice.MonitorInfo, args).Result;
                    if (!isSaAccepted)
                    {
                        //If DDPM.SA.EAPlugin cannot addcept the EditCommand, we will unregister the EditStarted
                        // handler, because, we will never receive this event from DDPM.SA.EAplugin
                        _deviceManagerSA.EAEditStarted -= _deviceManagerSA_EAEditStarted;
                        return;
                    }
#endif
                }
            }
        }

        private CellJson[] GetCellsFromISplitCtrl(ISplitCtrl splitCtrl)
        {
            List<CellJson> cellList = new List<CellJson>();
            foreach (CellObj objCell in splitCtrl.CellList)
            {
                CellJson cellJson = new CellJson();
                cellJson.Name = objCell.Name;
                cellJson.x = objCell.rcRatio.Left;
                cellJson.y = objCell.rcRatio.Top;
                cellJson.w = objCell.rcRatio.Width;
                cellJson.h = objCell.rcRatio.Height;
                cellList.Add(cellJson);
            }
            return cellList.ToArray();
        }

        //DDPM.SA.EAPlugin notify us the result of our previous EditCommand request.
        //1 If e is empty => The EditCommand is accept and the EditWindow is opened.
        //2 If e is not empty => The EditCommand is faled to open the EditWindow, and return the error message in e.
        private void _deviceManagerSA_EAEditStarted(object? sender, string e)
        {
#if ENABLE_CALL_SA
            if (_deviceManagerSA != null)
            {
                //Unregister the event handler now, until the next EditCommand called
                _deviceManagerSA.EAEditStarted -= _deviceManagerSA_EAEditStarted;

                if (!String.IsNullOrEmpty(e)) //If fail to start editing
                {
                    _vm.LogInfo($"EAEditStarted event, fail to Start Edit: {e}");
                    return;
                }
                //Register next event 'EditReturn'
                _deviceManagerSA.EAEditReturn += _deviceManagerSA_EAEditReturn;

                if (_console != null)
                {
                    Dispatcher.Invoke(new Action(() =>
                    {
                        //What option to DDPM.UI druing EditCommand is processing (when EditWindow is working)?
                        //
                        //_console.RaiseEvent("MainWindow.SetToBottomWindow", this, new EventManagerArgs());
                        _console.RaiseEvent("MainWindow.Minimize", this, new EventManagerArgs());
                        //_console.RaiseEvent("MainWindow.Hide", this, new EventManagerArgs());
                    }
                    ));
                }
            }
#endif
        }
        //Robert_Lin, 2024-11-10, 
        // Rewite this method after study DDM and DDPM stories
        //Robert_Lin, 2024-8-4
        /// <summary>
        /// Handle the 'EditReturn' message from DDPM.SA.EAPlugin, after EasyArrange EditWindow has finished its job.
        /// We will need to check the EAArgs.Result to determine if user close the EditWindow by 'Save" or 'Cancel' button.
        /// </summary>
        /// <param name="sender">The EAPlugin</param>
        /// <param name="e">
        /// A EAArgs object, most of proparties will be the same with the EAArgs when we calling EAEditCommand().
        /// EAArgs.Result:
        ///   True=User close EditWindow by click 'Save' button. We can get 1) EAArgs.Settings=>New settings,
        ///        2) EAArgs.CustomName => the CustomName which is entered by user.
        ///   False=User close EditWindow by click 'Cancel'. No new settings/values are returned.
        /// </param>
        /// <exception cref="NotImplementedException"></exception>
        private void _deviceManagerSA_EAEditReturn(object? sender, EAArgs e)
        {
#if ENABLE_CALL_SA
            if (_deviceManagerSA != null)
            {
                _deviceManagerSA.EAEditReturn -= _deviceManagerSA_EAEditReturn;
            }
            //Restore DDPM.UI mainwindow.
            if (_console != null)
            {
                _console.RaiseEvent("MainWindow.Normal", this, new EventManagerArgs());
                //_console.RaiseEvent("MainWindow.Activate", this, new EventManagerArgs());
            }

            if (!e.Result)
            {
                return;
            }

            Dispatcher.Invoke(new Action(() =>
            {
                bool isHandled = false;
                SplitItem? itemCustom = null;

                //Check if returned layout is edited from Preset Custom Layout (EAID=[1000~1004]
                //then we will replace to the existing ListViewItem
                if (e.SplitJson.EAID >= EAEMConstants.EAID_FirstCustom)
                {
                    //Find the customItem to be replaced
                    itemCustom = splitListView_Custom.FindItemByEAID(e.SplitJson.EAID);
                    //If found in CustomList
                    if ((itemCustom != null) && (itemCustom.ISplitCtrl != null))
                    {
                        //Replace data from return data
                        itemCustom.ReplaceByEAArgs(e);

                        //Check if itemCustom has Buddy (in RecentList)
                        SplitItem? itemRecent = null;
                        if (itemCustom.Buddy != null)
                        {
                            itemRecent = itemCustom.Buddy;
                            //Validation: itemRecent should be owned by ReceList
                            bool shouleBeTrue = (itemRecent.SplitOwner == Common.EAEM.eSplitOwner.EaRecent);
                            //Replace with returned data
                            itemCustom.Buddy.ReplaceByEAArgs(e);
                        }
                        else //No buddy in RecentList, need to add
                        {
                            itemRecent = AddNewItemToRecentList2ndPosition(itemCustom);
                        }
                        //Set it as current selected
                        _vm.SelectedSplitItem = itemCustom;
                        isHandled = true;
                    }
                    else //return layout not found in CustomListView
                    {
                        //will add a new item to CustomListVew
                    }
                }

                if (!isHandled)
                {
                    //Returned layout do not have same EAID item in CustomList
                    //We will add a new one or replace to first one if reach the maximum count

                    //If the CustomListView count < 5, We can add one new item to tail of the list 
                    if (splitListView_Custom.ItemCount < EAEMConstants.MaxCustomItems)
                    {
                        //Create a custom item
                        ISplitCtrl ispCustom = ISplitCtrl.Create(e.SplitJson.CellCount, e.SplitJson.SplitKey);
                        if (ispCustom == null)
                        {
                            return;
                        }
                        ispCustom.Settings = e.SplitJson.Settings;
                        ispCustom.FriendlyName = e.SplitJson.CustomName;
                        int newEAID = GetUnusedCustomEAID();
                        ispCustom.EAID = newEAID;
                        //Insert to the first (DDPMW-861)
                        itemCustom = splitListView_Custom.InsertSplitCtrlToList(ispCustom, 0);
                        itemCustom.CustomId = GenerateCustomId();

                        //Add a Buddy to Recent List
                        SplitItem? itemRecent = AddNewItemToRecentList2ndPosition(itemCustom);
                        if (itemRecent != null)
                        {
                            //Setup Buddy
                            itemCustom.Buddy = itemRecent;
                            itemRecent.Buddy = itemCustom;
                        }
                        //Set it as current selected
                        _vm.SelectedSplitItem = itemCustom;
                        isHandled = true;
                    }
                    else //CustomList Count >= max, we cannot add new item, will replace the latest item
                    {
                        SplitItem? delCustom = splitListView_Custom.GetLatestItem();
                        if (delCustom != null)
                        {
                            //Delete its Buddy from RecentList
                            if (itemCustom != null && itemCustom.Buddy != null)
                            {
                                splitListView_Recent.DeleteSplitItem(itemCustom.Buddy);
                            }
                            //Remove the CustomItem
                            splitListView_Custom.DeleteSplitItem(itemCustom);
                        }

                        //Create a new custom item
                        ISplitCtrl ispCustom = ISplitCtrl.Create(e.SplitJson.CellCount, e.SplitJson.SplitKey);
                        if (ispCustom == null)
                        {
                            return;
                        }
                        ispCustom.Settings = e.SplitJson.Settings;
                        ispCustom.FriendlyName = e.SplitJson.CustomName;
                        int newEAID = GetUnusedCustomEAID();
                        ispCustom.EAID = newEAID;
                        //Insert to the first (DDPMW-861)
                        itemCustom = splitListView_Custom.InsertSplitCtrlToList(ispCustom, 0);
                        if (itemCustom == null) return;
                        itemCustom.CustomId = GenerateCustomId();

                        //Add a Buddy to Recent List
                        SplitItem? itemRecent = AddNewItemToRecentList2ndPosition(itemCustom);
                        if (itemRecent != null)
                        {
                            //Setup Buddy
                            itemCustom.Buddy = itemRecent;
                            itemRecent.Buddy = itemCustom;
                        }
                        //Set it as current selected
                        _vm.SelectedSplitItem = itemCustom;
                        isHandled = true;

                        ////Check if itemCustom has Buddy (in RecentList)
                        //if (itemCustom.Buddy != null)
                        //{
                        //    //Replace with returned data
                        //    itemCustom.Buddy.ReplaceByEAArgs(e);
                        //}
                        //Set it as current selected
                        _vm.SelectedSplitItem = itemCustom;
                        isHandled = true;
                    } //if (splitListView_Custom.ItemCount < EAEMConstants.MaxCustomItems)
                }
                //Returned item has been add/replace to CustomListView

                //For OverlapLayout, convert the Settings to RatioRects and store in CellBorder.rcRatio
                if (itemCustom == null)
                    return;

                if (itemCustom.IsOverlapCustomLayout &&
                    itemCustom.ISplitCtrl != null)
                {
                    SplitCtrl0B sp0B = (SplitCtrl0B)itemCustom.ISplitCtrl;

                    List<CellBorder> cellBorders = new List<CellBorder>();
                    if (e.SplitJson.Cells != null)
                    {
                        foreach (CellJson cellJson in e.SplitJson.Cells)
                        {
                            CellBorder cellBorder = new CellBorder();
                            cellBorder.rcRatio = new Rect(cellJson.x, cellJson.y, cellJson.w, cellJson.h);
                            cellBorder.CellName = cellJson.Name;
                            cellBorders.Add(cellBorder);
                        }
                    }
                    sp0B.CellBorders.Clear();
                    sp0B.CellBorders.AddRange(cellBorders);                    
                }

                splitListView_Recent.MoveSelectedItemToSecondPosition();
                _vm.NotifySelectedLayoutChangedToSA();
                SaveEaSettings(true);

            }));
        }
#endif

        //        private void _deviceManagerSA_EAEditCompleted(object? sender, string e)
        //        {
        //#if ENABLE_CALL_SA

        //            if (_deviceManagerSA != null)
        //            {
        //                _deviceManagerSA.EAEditCompleted -= _deviceManagerSA_EAEditCompleted;
        //                if (_console != null)
        //                {
        //                    Dispatcher.Invoke(new Action(() =>
        //                    {
        //                        _console.RaiseEvent("MainWindow.Activate", this, new EventManagerArgs());
        //                        //_console.RaiseEvent("MainWindow.Normal", this, new EventManagerArgs());
        //                        //_console.RaiseEvent("MainWindow.Show", this, new EventManagerArgs());
        //                    }
        //                    ));
        //                }
        //            }
        //#endif
        //        }

        #endregion Edit Layout

        #region Delete Custom Layout item
        private void HandleSplitItemDeleteCommand(SplitItem spItem)
        {
            //Must be a EA SplitItem
            if (spItem.ISplitCtrl == null)
                return;

            //IDeviceManagerSA must be ready
            if (_deviceManagerSA == null) return;

            //Check if this custom layout is used by EasyMemory?
            //Debug, assume YES
            bool isLayoutUsedByEM = false;
            int eaId = 0;
            try
            {
                if (spItem.ISplitCtrl != null)
                {
                    eaId = spItem.ISplitCtrl.EAID;
                }
                if (eaId != 0)
                {
                    _vm.LogInfo($"EasyArrange, CheckEAIDExit(EAID={eaId})");
                    isLayoutUsedByEM = _deviceManagerSA.CheckEAIDExit(_homeDevice.MonitorInfo, eaId).Result;
                }
            }
            catch (Exception e1)
            {
                _vm.LogInfo("CheckEAIDExit()", e1);
            }
            if (isLayoutUsedByEM)
            {
                //Try to get hWnd of MainWindow
                Window mainWindow = System.Windows.Application.Current.MainWindow;
                //Show a message box to get comfirm from user
                string headerText = msgBox_Warning;
                string subHeaderText = msgBox_EAProfileWillBeDeleted;
                string leftButtonContent = msgBox_No;
                string rightButtonContent = msgBox_Yes;
                object ob = null;
                bool isStayOny = false;
                int autoCloseTimeSec = 0;
                DDPM.SA.Common.Popup.PopupBase popBase = new DDPM.SA.Common.Popup.PopupBase(
                    headerText, subHeaderText, leftButtonContent, rightButtonContent, ob, isStayOny, autoCloseTimeSec);
                popBase.Owner = mainWindow;

                bool? popResult = popBase.ShowDialog();
                //popResult: Close=null; LeftButton=false; RightButton=true
                if (popResult != true)
                    return;

                //Delete the EM Profile
                bool isDelOK = _deviceManagerSA.DeleteEAID(_homeDevice.MonitorInfo, eaId).Result;
            }

            //Find its Buddy in RecentList
            SplitItem? itemRecent = spItem.Buddy;
            //If Buddy exist (should be true)
            if (itemRecent != null)
            {
                //Remove the buddy from Recent list
                if (splitListView_Recent.DeleteSplitItem(itemRecent))
                {
                    //Rober_Lin, 2024-12-7, need to add Complement 
                    //to make sure RecentList always have 5 items
                    int addCount = ComplementRecentListItem();
                    //addCount should be =1

                }
            }
            splitListView_Custom.DeleteSplitItem(spItem);


            //If the deleted Custom item is Selected
            if (spItem.IsSelected)
            {
                //Force to selected Split0A
                SplitItem item0A = splitListView_Recent.SplitList[0];
                _vm.SelectedSplitItem = item0A;
                //_vm.SetWorkSplit(_vm.SelectedSplitItem.CellCount, _vm.SelectedSplitItem.SplitKey, _vm.SelectedSplitItem.Settings);
                _vm.NotifySelectedLayoutChangedToSA();
            }
            SaveEaSettings(true);
        }
        #endregion Delete Custom Layout item

        #region Settings File
        private bool SaveEaSettings(bool includeCustomList = false)
        {
            //Robert_Lin 2025-3-20 fix CS8602 Dereference of a possibly null reference.
            if (_vm.SelectedSplitItem == null)
                return false;

            //Save MonitorSettings: Selected, RecentList
            EAMonitorSettings eaSettings = new EAMonitorSettings();

            //Robert_Lin, 2025-4-8 Change DDPMMonitorSettings to v1 (from V0)
            try
            {
                eaSettings.Instance = _homeDevice.MonitorInfo.edid.Instance;
            }
            catch (Exception)
            {
            }
            finally
            {

            }

            SplitItem spItem = (SplitItem)_vm.SelectedSplitItem;
            eaSettings.SelectedSplit = spItem.ToSplitJson;

            List<SplitJson> recentList = new List<SplitJson>();
            foreach (SplitItem itemRecent in splitListView_Recent.SplitList.Skip(1))
            {
                recentList.Add(itemRecent.ToSplitJson);
            }
            eaSettings.RecentList = recentList.ToArray();

            bool res = false;
            if (_deviceManagerSA != null)
            {
                res = _deviceManagerSA.WriteEAMonitorSettings(_homeDevice.MonitorInfo, eaSettings).Result;
            }

            //Save UserSettings: CustomList
            if (includeCustomList)
            {
                //splitListView_Custom.RefreshCustomEAID();

                List<SplitJson> customList = new List<SplitJson>();
                foreach (SplitItem itemCustom in splitListView_Custom.SplitList)
                {
                    customList.Add(itemCustom.ToSplitJson);
                }

                if(_deviceManagerSA != null)
                {
                    res &= _deviceManagerSA.WriteEACustomList(customList.ToArray()).Result;
                }
            }
            return res;
        }
        #endregion Settings File

        #region SplitListView Operations
        private SplitItem? FindSplitItemFromWindowLists(int cellCount, char splitKey)
        {
            switch (cellCount)
            {
                case 2:
                    return splitListView_2w.FindSplitItem(cellCount, splitKey);
                case 3:
                    return splitListView_3w.FindSplitItem(cellCount, splitKey);
                case 4:
                    return splitListView_4w.FindSplitItem(cellCount, splitKey);
                case 5:
                    return splitListView_5w.FindSplitItem(cellCount, splitKey);
                case 6:
                    return splitListView_6w.FindSplitItem(cellCount, splitKey);
                case 7:
                    return splitListView_7w.FindSplitItem(cellCount, splitKey);
                default: return null;
            }
        }
        #endregion SplitListView Operations

        #region Custom Layout Manager
        private const int MaxCustomItems = 5;

        /// <summary>
        /// Generate the CustomNames as the ItemSource of ComboBox in SaveCustomWindow.
        /// This method must be executed in UI thread.
        /// </summary>
        /// <param name="selectedIndex">
        /// the index to the returning list that the first unused name.
        /// -1 = no unused name
        /// </param>
        /// <returns>The CustomName list</returns>
        private List<string> GenerateCustomNames(out int idxFirstUnused)
        {
            List<string> listOut = new List<string>();

            //Step A. Add existing names
            //A1. Get the friendNames from the CustomListView
            List<string> existNames = splitListView_Custom.GetCustomFriendlyNames();
            //A2 add existNames into listOut
            listOut.AddRange(existNames);

            //Step B. If all custom names are in ListOut
            const int localMaxCustomItems = DDPM.SA.Common.Display.EAEMConstants.MaxCustomItems; //=5
            if (listOut.Count >= localMaxCustomItems)
            {
                idxFirstUnused = -1; //No unused name in listOut
                return listOut;
            }

            //Step C. Add remaining names
            //C1. SelectedIndex <- the first available index
            idxFirstUnused = listOut.Count;
            //C2. Generate unused names
            for (int i = idxFirstUnused; i < localMaxCustomItems; i++)
            {
                //C3. Get the next available custom name
                for (int j = 1; j <= localMaxCustomItems; j++)
                {
                    //C3.1. Generate a customName
                    string customName = $"Custom Layout ({j})";
                    //C3.2. Check if this customName is already used by other custom item 
                    if (!listOut.Contains(customName))
                    {
                        //C3.3. No, this customName is available to use => add to list
                        listOut.Add(customName);
                        break; //Find a customName for next item
                    }
                    //C3.3. Yes it's already in used => generate next name
                }
            }

            //Step D
            return listOut;
        }


        public static long GenerateCustomId()
        {
            long unixTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            return unixTime;
        }

        private int GetUnusedCustomEAID()
        {
            int eaid = EAEMConstants.EAID_FirstCustom;
            for (int i = 0; i < EAEMConstants.MaxCustomItems; i++)
            {
                SplitItem? spItem = splitListView_Custom.FindItemByEAID(eaid);
                if (spItem == null)
                    return eaid;
                eaid++;
            }
            return 0;
        }

        //private string GenerateCustomFriendlyName()
        //{
        //    //n = 1,2,3,.... to find the first non-duplicate name as the return string
        //    int maxN = int.MaxValue - 1;
        //    for (int n = 1; n < maxN; n++)
        //    {
        //        string retName = $"Custom Layout ({n})";
        //        SplitItem? spItem = splitListView_Custom.FindItemByFriendlyName(retName);
        //        if (spItem == null)
        //        {
        //            return retName;
        //        }
        //    }
        //    return "Custom Layout";
        //}
        #endregion

        #region Refresh Data
        public void HandleSelectedHomeDeviceChanged()
        {
            this.Dispatcher.Invoke(() =>
            {
                if (DdpmCommonHelper.ModuleOwner != null)
                {
                    _homeDevice = DdpmCommonHelper.ModuleOwner.SelectedHomeDevice;
                    if ((_homeDevice != null) && (_homeDevice.vmEzArrange == null))
                    {
                        _homeDevice.vmEzArrange = new DDPM.UI.Common.ViewModels.EzArrangeViewModel(_homeDevice);
                    }
                    _vm = _homeDevice.vmEzArrange;
                    DataContext = _homeDevice.vmEzArrange;

                }
                CleanUpListViewItems();
                InitListViewItems();
            });
        }

        /// <summary>
        /// Check if screen orientation is changed, and then refresh to ISplitCtrls
        /// </summary>
        public void RefreshScreenOrientation()
        {
            //Origial IsVertical settings
            bool isVerticalOrg = _vm.IsVertical;

            //Get new IsVertical
            Screen? currentScreen = GetAttachedScreen(_homeDevice.MonitorInfo.DisplayName);
            bool isVerticalNew = (currentScreen != null) ? (currentScreen.Bounds.Width < currentScreen.Bounds.Height) : false;

            //If it's changed, then refresh to all ListViews
            if (isVerticalOrg != isVerticalNew)
            {
                _vm.IsVertical = isVerticalNew;
                //Robert_Lin 2025-2-19 try to improve performance when switch Vbar quickly
                //NEW:
                InvokeRefreshLViewsIsVeritical(isVerticalNew);
                //OLD:               
                //Dispatcher.Invoke(new Action(() =>
                //{
                //    Recent ListView
                //    splitListView_Recent.IsVertical = isVerticalNew;

                //    Custom ListView
                //    splitListView_Custom.IsVertical = isVerticalNew;

                //    Reset ListViews
                //    splitListView_2w.IsVertical = isVerticalNew;
                //    splitListView_3w.IsVertical = isVerticalNew;
                //    splitListView_4w.IsVertical = isVerticalNew;
                //    splitListView_5w.IsVertical = isVerticalNew;
                //    splitListView_6w.IsVertical = isVerticalNew;
                //    splitListView_7w.IsVertical = isVerticalNew;
                //}));


            }
        }

        private async void InvokeRefreshLViewsIsVeritical(bool isVerticalNew)
        {
            await Task.Run(() =>
            {
                Dispatcher.Invoke(new Action(() =>
                {
                    //Recent ListView
                    splitListView_Recent.IsVertical = isVerticalNew;

                    //Custom ListView
                    splitListView_Custom.IsVertical = isVerticalNew;

                    //Reset ListViews
                    splitListView_2w.IsVertical = isVerticalNew;
                    splitListView_3w.IsVertical = isVerticalNew;
                    splitListView_4w.IsVertical = isVerticalNew;
                    splitListView_5w.IsVertical = isVerticalNew;
                    splitListView_6w.IsVertical = isVerticalNew;
                    splitListView_7w.IsVertical = isVerticalNew;
                }));
            });
        }
        #endregion Refresh Data

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

        #region Add Custom Layout
        private void HandleAddButtonClickCommand(SplitListView lv)
        {
            if (_deviceManagerSA != null)
            {
                //Get the CustomName list and the index to the first unused name
                int idxFirstUnused = 0;
                List<string> customNameList = GenerateCustomNames(out idxFirstUnused);

                //index to customNameList of current editing
                // always use the first unused name
                int selectedIndex = idxFirstUnused;

                // But if custom list if full, then + button should not be enabled.
                // Use the name of first item
                if ((selectedIndex < 0) || (selectedIndex >= customNameList.Count))
                {
                    selectedIndex = 0;
                }
                //Create a default EAArgs, for SplitCtrl0B layout
                EAArgs args = new EAArgs()
                {
                    Command = "EditCommnd",
                    CellCount = 0,
                    SplitKey = 'B',
                    CustomId = 0,
                    CustomName = customNameList[selectedIndex],
                    CustomNames = customNameList,
                    Settings = new List<double>()
                };
                args.SplitJson = new SplitJson()
                {
                    CellCount = 0,
                    SplitKey = 'B',
                    CustomId = 0,
                    CustomName = customNameList[selectedIndex],
                    Settings = new List<double>(),
                    EAID = 0,
                    Cells = new CellJson[] { }
                };
                //Register a event handler for EditStarted event
                _deviceManagerSA.EAEditStarted += _deviceManagerSA_EAEditStarted;
                bool isSaAccepted = _deviceManagerSA.EAEditCommand(_homeDevice.MonitorInfo, args).Result;
                if (!isSaAccepted)
                {
                    //If DDPM.SA.EAPlugin cannot addcept the EditCommand, we will unregister the EditStarted
                    // handler, because, we will never receive this event from DDPM.SA.EAplugin
                    _deviceManagerSA.EAEditStarted -= _deviceManagerSA_EAEditStarted;
                    return;
                }
            }

        }
        #endregion Add Custom Layout

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
            if (splitListView_Recent.ItemCount >= EAEMConstants.MaxRecentItems + 1)
            {
                //Remove the last item
                SplitItem? itemLatest = splitListView_Recent.GetLatestItem();
                //Unbound with its Buddy
                if (itemLatest != null)
                {
                    if (itemLatest.Buddy != null)
                        itemLatest.Buddy.Buddy = null;
                    splitListView_Recent.DeleteSplitItem(itemLatest);
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
                    SplitItem itemRecent = splitListView_Recent.AddSplitCtrlTo2ndPosition(ispRecent);
                    itemRecent.CustomId = itemSouce.CustomId;

                    itemSouce.Buddy = itemRecent;
                    itemRecent.Buddy = itemSouce;

                    return itemRecent;
                }
            }

            return null;
        }
        #endregion

        #region Save Layout Icons to PNG files
        private void saveSplitCtrlsToPngImagesButton_Click(object sender, RoutedEventArgs e)
        {
            //DDPM.SA.Common.Popup.PopupBase popupBase = new DDPM.SA.Common.Popup.PopupBase(true, true, "HeaderText", "SubHeaderText");
            //popupBase.ShowDialog(this);

            OpenFolderDialog ofd = new OpenFolderDialog()
            {
                Title = "Select a folder",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Personal)
            };
            if (ofd.ShowDialog() == true)
            {
                SaveLayoutIconsToPngFiles(ofd.FolderName);
            }
        }
        private void SaveLayoutIconsToPngFiles(string folderPath)
        {
            //2 Windows
            foreach (SplitItem spItem in splitListView_2w.SplitList)
            {
                if (spItem.ISplitCtrl == null)
                    continue;
                ISplitCtrl isp = spItem.ISplitCtrl;
                BitmapSource bmpSrc = isp.CreateBitmapSource();
                if (bmpSrc != null)
                {
                    string pathName = System.IO.Path.Combine(folderPath, $"EA-{isp.EAID}.PNG");
                    ISplitCtrl.SaveBitmapSourceAsPngFile(bmpSrc, pathName);
                }
            }
            //3 Windows
            foreach (SplitItem spItem in splitListView_3w.SplitList)
            {
                if (spItem.ISplitCtrl == null)
                    continue;
                ISplitCtrl isp = spItem.ISplitCtrl;
                BitmapSource bmpSrc = isp.CreateBitmapSource();
                if (bmpSrc != null)
                {
                    string pathName = System.IO.Path.Combine(folderPath, $"EA-{isp.EAID}.PNG");
                    ISplitCtrl.SaveBitmapSourceAsPngFile(bmpSrc, pathName);
                }
            }
            //4 Windows
            foreach (SplitItem spItem in splitListView_4w.SplitList)
            {
                if (spItem.ISplitCtrl == null)
                    continue;
                ISplitCtrl isp = spItem.ISplitCtrl;
                BitmapSource bmpSrc = isp.CreateBitmapSource();
                if (bmpSrc != null)
                {
                    string pathName = System.IO.Path.Combine(folderPath, $"EA-{isp.EAID}.PNG");
                    ISplitCtrl.SaveBitmapSourceAsPngFile(bmpSrc, pathName);
                }
            }
            //5 Windows
            foreach (SplitItem spItem in splitListView_5w.SplitList)
            {


                if (spItem.ISplitCtrl == null)
                    continue;
                ISplitCtrl isp = spItem.ISplitCtrl;
                BitmapSource bmpSrc = isp.CreateBitmapSource();
                if (bmpSrc != null)
                {
                    string pathName = System.IO.Path.Combine(folderPath, $"EA-{isp.EAID}.PNG");
                    ISplitCtrl.SaveBitmapSourceAsPngFile(bmpSrc, pathName);
                }
            }
            //6 Windows
            foreach (SplitItem spItem in splitListView_6w.SplitList)
            {
                if (spItem.ISplitCtrl == null)
                    continue;
                ISplitCtrl isp = spItem.ISplitCtrl;
                BitmapSource bmpSrc = isp.CreateBitmapSource();
                if (bmpSrc != null)
                {
                    string pathName = System.IO.Path.Combine(folderPath, $"EA-{isp.EAID}.PNG");
                    ISplitCtrl.SaveBitmapSourceAsPngFile(bmpSrc, pathName);
                }
            }
            //7 Windows
            foreach (SplitItem spItem in splitListView_7w.SplitList)
            {
                if (spItem.ISplitCtrl == null)
                    continue;
                ISplitCtrl isp = spItem.ISplitCtrl;
                BitmapSource bmpSrc = isp.CreateBitmapSource();
                if (bmpSrc != null)
                {
                    string pathName = System.IO.Path.Combine(folderPath, $"EA-{isp.EAID}.PNG");
                    ISplitCtrl.SaveBitmapSourceAsPngFile(bmpSrc, pathName);
                }
            }
        }
        #endregion

        #region Complement Recent List Item
        //Robert_Lin, 2024-12-31 Use below method to replace FindPresetItemWhichNoBuddy()
        //The Complement recent list source will be the default RecentList
        private SplitItem? FindComplementItem()
        {
            List<SplitJson> defaultRecentList = SplitJson.DefaultRecentList;
            foreach (SplitJson spjRecent in defaultRecentList)
            {
                //Check if this ISplitCtrl is already existed
                SplitItem? spjItem = splitListView_Recent.FindItemByEAID(spjRecent.EAID);
                //If not found, then it's not in RecentList, will use this as the Complement item
                if (spjItem == null)
                {
                    SplitItem? compItem = FindSplitItemFromWindowLists(spjRecent.CellCount, spjRecent.SplitKey);
                    return compItem;
                }
            }
            return null;
        }

        /// <summary>
        /// Find a preset layout as the complement recent item, which is no Buddy.
        /// </summary>
        /// <returns></returns>
        private SplitItem? FindPresetItemWhichNoBuddy()
        {
            SplitItem? itemOut = null;
            itemOut = splitListView_2w.FindFirstNoBuddyItem();
            if (itemOut != null)
                return itemOut;
            itemOut = splitListView_3w.FindFirstNoBuddyItem();
            if (itemOut != null)
                return itemOut;
            itemOut = splitListView_4w.FindFirstNoBuddyItem();
            if (itemOut != null)
                return itemOut;

            if (_homeDevice != null &&
                _homeDevice.MonitorInfo != null)
            {
                float monitorSize = _homeDevice.MonitorInfo.edid.Size;
                bool isSmallSizeMonitor = monitorSize < 19.0000;
                if (isSmallSizeMonitor)
                    return null;                
            }

            itemOut = splitListView_5w.FindFirstNoBuddyItem();
            if (itemOut != null)
                return itemOut;

            itemOut = splitListView_6w.FindFirstNoBuddyItem();
            if (itemOut != null)
                return itemOut;
            itemOut = splitListView_7w.FindFirstNoBuddyItem();
            if (itemOut != null)
                return itemOut;

            return null;
        }
        /// <summary>
        /// Get a preset layout which is not in current RecentList
        /// in order of EAID (1,2,3, ..., 49). Add the complement item into
        /// RecentListView, ans setup for it and its Buddy.
        /// </summary>
        /// <returns>true if add one item successed.</returns>
        private int ComplementRecentListItem()
        {
            int addCount = 0;
            while (splitListView_Recent.ItemCount < (EAEMConstants.MaxRecentItems+1))
            {
                //To find a complement candidate
                SplitItem? itemPreset = FindComplementItem();
                //SplitItem? itemPreset = FindPresetItemWhichNoBuddy();
                //If found
                if ((itemPreset != null) && (itemPreset.ISplitCtrl != null))
                {
                    //Create a Recent splitCtrl from the presetItem
                    ISplitCtrl? ispRecent = itemPreset.ISplitCtrl.Clone();

                    //Add the splitCtrl to Recent List
                    SplitItem itemRecent = splitListView_Recent.AddItemToList(ispRecent.UC);
                    itemRecent.SplitOwner = Common.EAEM.eSplitOwner.EaRecent;

                    //Setup buddy to both
                    itemRecent.Buddy = itemPreset;
                    itemPreset.Buddy = itemRecent;

                    addCount++;
                }
            }
            return addCount;
        }
        #endregion

     }
}