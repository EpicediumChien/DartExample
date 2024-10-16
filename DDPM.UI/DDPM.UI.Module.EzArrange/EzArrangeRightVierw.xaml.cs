#define ENABLE_CALL_SA
//Robert_Lin, 2024-8-14, comment out the #define line if you would like to disable calling to Subagent EAPlugin
using CommunityToolkit.Mvvm.Input;
using DDPM.Easy.Common;
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

namespace DDPM.UI.Module.EzArrange
{
    /// <summary>
    /// Interaction logic for EzArrangeRightVierw.xaml
    /// </summary>
    public partial class EzArrangeRightVierw : UserControl
    {
        #region Private Members
        private HomeDevice _homeDevice;
        private IDeviceManagerSA _deviceManagerSA;
        private DDPM.UI.Common.ViewModels.EzArrangeViewModel _vm;
        private readonly DisplayViewModel _vmDisplay;
        private readonly IConsole _console;
        private readonly ILog _log;
        private const string CustomListTooltipText = "You can arrange the windows on your screen and click + icon.\r\nAlternatively, select an existing layout below and click the pencil icon to edit the layout.";
        #endregion Private Members

        #region ctor
        public EzArrangeRightVierw(DisplayViewModel vmDisplay)
        {
            Requires.NotNull(vmDisplay, nameof(vmDisplay));

            InitializeComponent();

            _vmDisplay = vmDisplay;
            _homeDevice = vmDisplay.SelectedHomeDevice;
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

            splitListView_Recent.ItemClickCommand = new RelayCommand<SplitItem>(OnListViewItemClicked);
            splitListView_Custom.ItemClickCommand = new RelayCommand<SplitItem>(OnListViewItemClicked);
            splitListView_2w.ItemClickCommand = new RelayCommand<SplitItem>(OnListViewItemClicked);
            splitListView_3w.ItemClickCommand = new RelayCommand<SplitItem>(OnListViewItemClicked);
            splitListView_4w.ItemClickCommand = new RelayCommand<SplitItem>(OnListViewItemClicked);
            splitListView_5w.ItemClickCommand = new RelayCommand<SplitItem>(OnListViewItemClicked);
            splitListView_6w.ItemClickCommand = new RelayCommand<SplitItem>(OnListViewItemClicked);
            splitListView_7w.ItemClickCommand = new RelayCommand<SplitItem>(OnListViewItemClicked);

            splitListView_Custom.ItemEditCommand = new RelayCommand<SplitItem>(HandleSplitItemEditCommand);
            splitListView_2w.ItemEditCommand = new RelayCommand<SplitItem>(HandleSplitItemEditCommand);
            splitListView_3w.ItemEditCommand = new RelayCommand<SplitItem>(HandleSplitItemEditCommand);
            splitListView_4w.ItemEditCommand = new RelayCommand<SplitItem>(HandleSplitItemEditCommand);
            splitListView_5w.ItemEditCommand = new RelayCommand<SplitItem>(HandleSplitItemEditCommand);
            splitListView_6w.ItemEditCommand = new RelayCommand<SplitItem>(HandleSplitItemEditCommand);
            splitListView_7w.ItemEditCommand = new RelayCommand<SplitItem>(HandleSplitItemEditCommand);

            splitListView_Custom.ItemDeleteCommand = new RelayCommand<SplitItem>(HandleSplitItemDeleteCommand);
            splitListView_Custom.HasAddButton = true;
            splitListView_Custom.AddButtonClickCommand = new RelayCommand<SplitListView>(HandleAddButtonClickCommand);

            //InitRecentListView();
            InitListViewItems();

            customListTooltipText.Text = CustomListTooltipText;
        }
        #endregion ctor

        #region UI Init / Exit
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            //InitRecentListView();
            //InitListViewItems();
        }
        #endregion

        #region Init SplitListView and SplitItems
        private void InitListViewItems()
        {
            //0. Prepare
            //
            if (_deviceManagerSA == null) return;

            EAMonitorSettings eaSettings = _deviceManagerSA.ReadEAMonitorSettings(_homeDevice.MonitorInfo).Result;

            Screen? currentScreen = GetAttachedScreen(_homeDevice.MonitorInfo.DisplayName);
            _vm.IsVertical = (currentScreen != null) ? (currentScreen.Bounds.Width < currentScreen.Bounds.Height) : false;

            DisplayOrientation orient = GetDisplayOrientation(_homeDevice.MonitorInfo.DisplayName);
            _vm.IsVertical = (orient == DisplayOrientation.Angle90) || (orient == DisplayOrientation.Angle270);

            //Update IsVertical to listViews
            splitListView_Recent.IsVertical = _vm.IsVertical;
            splitListView_Custom.IsVertical = _vm.IsVertical;
            splitListView_2w.IsVertical = _vm.IsVertical;
            splitListView_3w.IsVertical = _vm.IsVertical;
            splitListView_4w.IsVertical = _vm.IsVertical;
            splitListView_5w.IsVertical = _vm.IsVertical;
            splitListView_6w.IsVertical = _vm.IsVertical;
            splitListView_7w.IsVertical = _vm.IsVertical;

            //A Build WindowLists
            //
            foreach (ISplitCtrl spCtrl in ISplitCtrl.Splits_EA)
            {
                SplitItem? spItem = null;
                ISplitCtrl newSplit = spCtrl.New();
                if (newSplit == null)
                    continue;
                newSplit.SplitMode = eSplitModes.Icon;
                newSplit.IsVertical = _vm.IsVertical;

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

                    default:
                        break;
                }
                if (spItem != null)
                {
                    spItem.IsEditEnabled = true;
                    spItem.SplitOwner = Common.EAEM.eSplitOwner.EaWin;
                }
            } //foreach(ISplitCtrl spCtrl in ISplitCtrl.Splits_EA)

            //B Load CustomList from settings file
            //
            if (eaSettings != null)
            {
                if (eaSettings.CustomList != null)
                {
                    //Add saved custom list to custom list view
                    foreach (SplitJson spj in eaSettings.CustomList)
                    {
                        //Validate settings
                        //1 CustomId must > 0
                        if (spj.CustomId == 0)
                        {
                            _vm.LogInfo($"  * InitListViewItems({_homeDevice.MonitorInfo?.modelName},{_homeDevice.MonitorInfo?.edid.ServiceTag}) Settings.CustomList[{spj.CellCount}{spj.SplitKey}], CustomId=[{spj.CustomId}], CustomName=[{spj.CustomName}], Msg=[Invalid setting, CustomId is zero]");
                            continue;
                        }
                        //2 CustomName cannot be empty
                        if (String.IsNullOrWhiteSpace(spj.CustomName))
                        {
                            _vm.LogInfo($"  * InitListViewItems({_homeDevice.MonitorInfo?.modelName},{_homeDevice.MonitorInfo?.edid.ServiceTag}) Settings.CustomList[{spj.CellCount}{spj.SplitKey}], CustomId=[{spj.CustomId}], CustomName=[{spj.CustomName}], Msg=[Invalid setting, CustomName is empty]");
                            continue;
                        }
                        //3 CustomName length
                        if (spj.CustomName.Length > EAEMConstants.MaxCustomNameLenth)
                        {
                            _vm.LogInfo($"  * InitListViewItems({_homeDevice.MonitorInfo?.modelName},{_homeDevice.MonitorInfo?.edid.ServiceTag}) Settings.CustomList[{spj.CellCount}{spj.SplitKey}], CustomId=[{spj.CustomId}], CustomName=[{spj.CustomName}], Msg=[Invalid setting, CustomName length is invalid]");
                            continue;
                        }

                        ISplitCtrl? spCtrl = ISplitCtrl.Create(spj.CellCount, spj.SplitKey);
                        if (spCtrl == null)
                            continue;
                        spCtrl.Settings = new List<double>(spj.Settings);
                        spCtrl.SplitMode = eSplitModes.Icon;
                        spCtrl.FriendlyName = spj.CustomName;

                        SplitItem itemCustom = splitListView_Custom.AddItemToList(spCtrl.UC);
                        itemCustom.SplitOwner = Common.EAEM.eSplitOwner.EaCustom;
                        itemCustom.CustomId = (int)spj.CustomId;

                        //Robert_Lin, 2024-10-4 add max items check
                        if (splitListView_Custom.ItemCount >= EAEMConstants.MaxCustomItems)
                            break;

                    } //(SplitJson spj in eaSettings.CustomList)
                }
            } //if (eaSettings != null)

            //C Load & Build Recent List
            //

            //Use to trace count of slected item
            int selectedCount = 0;

            //C01. Add "Off" SplitCtrl0A as the first item of RecentList
            ISplitCtrl? sp0A = ISplitCtrl.Create(0, 'A');
            SplitItem item0A;

            if (sp0A != null)
            {
                sp0A.FriendlyName = "Off"; //Need Multilogual support
                sp0A.SplitMode = eSplitModes.Icon;
                item0A = splitListView_Recent.AddItemToList(sp0A.UC);
                item0A.SplitOwner = Common.EAEM.eSplitOwner.EaRecent;

                //if (eaSettings.SelectedSplit.CellCount == 0)
                //{
                //    item0A.IsSelected = true;
                //    _vm.SelectedSplitItem = item0A;
                //    selectedCount++;
                //}
            }
            //C02. If saved recent list is not empty, then add them into Recent listview
            if (eaSettings?.RecentList != null)
            {
                int idxRecentList = 0;
                foreach (DDPM.SA.Common.Display.SplitJson spj in eaSettings.RecentList)
                {
                    //Robert_Lin, 2024-10-4 Check maximun items, +1:Off 
                    if (splitListView_Recent.ItemCount >= EAEMConstants.MaxRecentItems+1)
                        break;

                    //Validate RectentList items, skip the invalid items
                    //1 CustomId=0 and CustomName is empty is invalid
                    if ((spj.CustomId == 0) && (!String.IsNullOrWhiteSpace(spj.CustomName)))
                    {
                        _vm.LogInfo($"  * InitListViewItems({_homeDevice.MonitorInfo?.modelName},{_homeDevice.MonitorInfo?.edid.ServiceTag}) Settings.RecentList[{spj.CellCount}{spj.SplitKey}], CustomId=[{spj.CustomId}], CustomName=[{spj.CustomName}], Msg=[Invalid setting, CustomId is zero]");
                        idxRecentList++;
                        continue;
                    }

                    //2 All Recent item must has Buddy
                    SplitItem? itemBuddy = null;
                    if (spj.CustomId == 0)
                    {
                        //Find Buddy from WinLists
                        itemBuddy = FindSplitItemFromWindowLists(spj.CellCount, spj.SplitKey);
                    }
                    else
                    {
                        //Find Buddy from CustomList
                        itemBuddy = splitListView_Custom.FindItemByCustomId(spj.CustomId);
                    }
                    //If cannot find a Buddy, then will be discard
                    if (itemBuddy == null)
                    {
                        _vm.LogInfo($"  * InitListViewItems({_homeDevice.MonitorInfo?.modelName},{_homeDevice.MonitorInfo?.edid.ServiceTag}) Settings.RecentList[{spj.CellCount}{spj.SplitKey}], CustomId=[{spj.CustomId}], CustomName=[{spj.CustomName}], Msg=[Cannot find Buddy]");
                        idxRecentList++;
                        continue;
                    }

                    //Create a Recent item, and add to RecentList
                    //
                    ISplitCtrl? spCtrl = ISplitCtrl.Create(spj.CellCount, spj.SplitKey);
                    if (spCtrl == null)
                    {
                        _vm.LogInfo($"  * InitListViewItems({_homeDevice.MonitorInfo?.modelName},{_homeDevice.MonitorInfo?.edid.ServiceTag}) Settings.RecentList[{spj.CellCount}{spj.SplitKey}], CustomId=[{spj.CustomId}], CustomName=[{spj.CustomName}], Msg=[Fail to create ISplitCtrl]");
                        idxRecentList++;
                        continue;
                    }
                    if (spj.Settings == null)
                        spCtrl.Settings = new List<double>();
                    else
                        spCtrl.Settings = new List<double>(spj.Settings);
                    spCtrl.SplitMode = eSplitModes.Icon;
                    spCtrl.FriendlyName = spj.CustomName;

                    SplitItem itemRecent = splitListView_Recent.AddItemToList(spCtrl.UC);
                    itemRecent.SplitOwner = Common.EAEM.eSplitOwner.EaRecent;
                    itemRecent.CustomId = spj.CustomId;

                    //bool isSelected = (eaSettings.SelectedSplit.CellCount == spj.CellCount) &&
                    //    (eaSettings.SelectedSplit.SplitKey == spj.SplitKey);

                    //Setup Buddy
                    //

                    //If it's a Window item
                    if (spj.CustomId == 0)
                    {
                        itemRecent.Buddy = itemBuddy;
                        itemBuddy.Buddy = itemRecent;
                    }
                    else //It's a Custom item
                    {
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
                            _vm.LogInfo($"  * InitListViewItems({_homeDevice.MonitorInfo?.modelName},{_homeDevice.MonitorInfo?.edid.ServiceTag}) Settings.RecentList[{spj.CellCount}{spj.SplitKey}], CustomId=[{spj.CustomId}], CustomName=[{spj.CustomName}], Msg=[SelectedCount>1]");

                            //Add to list but do not set it as Selected
                            isSelected = false;
                        }
                    }
                    //itemRecent.IsSelected = isSelected;
                    //itemBuddy.IsSelected = isSelected;
                    //_vm.SelectedSplitItem = itemBuddy;

                    idxRecentList++;
                } //foreach
            } //if (eaSettings?.CustomList != null)

            //D Add all custom items which has no Buddy into Recent list
            //
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

            //E Add all Window items which has no buddy into recent list
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
        }

        private void InitSplitListViews_Unused()
        {
            //A Build WindowLists
            //
            foreach (ISplitCtrl isp in ISplitCtrl.Splits_EA)
            {
            }
        }

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
            splitListView_4w.ClearList();
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
            if (spItem.InnerContent is ISplitCtrl)
            {
                ISplitCtrl spCtrl = spItem.InnerContent as ISplitCtrl;
                //_vm.SelectedSplitItem = spItem;
                //_vm.SetWorkSplit(spCtrl.CellCount, spCtrl.SplitKey);

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

                    //Prepare for the EAArgs
                    int selectedIndex = 0;
                    List<string> friendlyNameList = GenerateCustomNames(out selectedIndex);

                    //Create a defulte EAArgs, for pre-defined layout
                    EAArgs args = new EAArgs()
                    {
                        Command = "EditCommnd",
                        CellCount = spCtrl.CellCount,
                        SplitKey = spCtrl.SplitKey,
                        CustomId = spItem.CustomId,
                        CustomName = friendlyNameList[selectedIndex],
                        CustomNames = friendlyNameList,
                        Settings = spCtrl.Settings

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
                //Find in CustomList, for the item with the same CustomName
                SplitItem? itemCustom = splitListView_Custom.FindItemByFriendlyName(e.CustomName);
                //If found in CustomList
                if (itemCustom != null)
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
                    //splitListView_Recent.MoveSelectedItemToSecondPosition();
                    //_vm.SetWorkSplit(itemCustom.CellCount, itemCustom.SplitKey, itemCustom.Settings);
                }
                else //itemCustom==null
                {
                    //Returned layout do not have same CustomName item in CustomList
                    //We will add a new one or replace to first one if reach the maximum count

                    if (splitListView_Custom.ItemCount < EAEMConstants.MaxCustomItems)
                    {
                        //Create a custom item
                        ISplitCtrl ispCustom = ISplitCtrl.Create(e.CellCount, e.SplitKey);
                        if (ispCustom == null)
                        {
                            if ((e.CellCount == 0) && (e.SplitKey == 'B'))
                            {
                                ispCustom = new SplitCtrl0B();
                            }
                            else
                                return;
                        }
                        ispCustom.Settings = e.Settings;
                        ispCustom.FriendlyName = e.CustomName;
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
                        //_vm.SetWorkSplit(itemCustom.CellCount, itemCustom.SplitKey, itemCustom.Settings);
                        //SaveEaSettings();
                    }
                    else //CustomList Count >= max, we cannot add new item, will replace the first item
                    {
                        itemCustom = splitListView_Custom.GetAt(0);
                        if (itemCustom == null) return;

                        //Replace data from return data
                        itemCustom.ReplaceByEAArgs(e);

                        //Check if itemCustom has Buddy (in RecentList)
                        if (itemCustom.Buddy != null)
                        {
                            //Replace with returned data
                            itemCustom.Buddy.ReplaceByEAArgs(e);
                        }
                        //Set it as current selected
                        _vm.SelectedSplitItem = itemCustom;
                        //splitListView_Recent.MoveSelectedItemToSecondPosition();
                        //_vm.SetWorkSplit(itemCustom.CellCount, itemCustom.SplitKey, itemCustom.Settings);
                    } //if (splitListView_Custom.ItemCount < EAEMConstants.MaxCustomItems)
                }

                splitListView_Recent.MoveSelectedItemToSecondPosition();
                _vm.SetWorkSplit(e.CellCount, e.SplitKey, e.Settings);
                SaveEaSettings();

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

            //Find its Buddy in RecentList
            SplitItem? itemRecent = spItem.Buddy;
            //If Buddy exist (should be true)
            if (itemRecent != null)
            {
                //Remove the buddy from Recent list
                splitListView_Recent.DeleteSplitItem(itemRecent);
            }
            splitListView_Custom.DeleteSplitItem(spItem);

            //If the deleted Custom item is Selected
            if (spItem.IsSelected)
            {
                //Force to selected Split0A
                SplitItem item0A = splitListView_Recent.SplitList[0];
                _vm.SelectedSplitItem = item0A;
                _vm.SetWorkSplit(_vm.SelectedSplitItem.CellCount, _vm.SelectedSplitItem.SplitKey, _vm.SelectedSplitItem.Settings);
            }
            SaveEaSettings();
        }
        #endregion Delete Custom Layout item

        #region Settings File
        private bool SaveEaSettings()
        {
            EAMonitorSettings eaSettings = new EAMonitorSettings();
            eaSettings.SelectedSplit = _vm.SelectedSplitItem.ToSplitJson;
            eaSettings.CustomList = new List<SA.Common.Display.SplitJson>();
            foreach (SplitItem itemCustom in splitListView_Custom.SplitList)
            {
                eaSettings.CustomList.Add(itemCustom.ToSplitJson);
            }

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
        private const int MaxCustomItems  = 5;

        /// <summary>
        /// Generate the CustomNames as the ItemSource of ComboBox in SaveCustomWindow.
        /// This method must be executed in UI thread.
        /// </summary>
        /// <param name="selectedIndex">
        /// Output selectedIndex for current custom item
        /// </param>
        /// <returns>The CustomName list</returns>
        private List<string> GenerateCustomNames(out int selectedIndex)
        {
            List<string> listOut = new List<string>();

            //Step A. Add existing names
            //A1. Get the friendNames from the CustomListView
            List<string> existNames = splitListView_Custom.GetCustomFriendlyNames();
            //A2 add existNames into listOut
            listOut.AddRange(existNames);

            //Step B. If all custom names are in ListOut
            const int MaxCustomItems = DDPM.SA.Common.Display.EAEMConstants.MaxCustomItems; //=5
            if (listOut.Count >= MaxCustomItems)
            {
                selectedIndex = 0;
                return listOut;
            }

            //Step C. Add remaining names
            //C1. SelectedIndex <- the first available index
            selectedIndex = listOut.Count;
            //C2. Generate unused names
            for (int i = selectedIndex; i < MaxCustomItems; i++)
            {
                //C3. Get the next available custom name
                for (int j=1; j<=MaxCustomItems; j++)
                {
                    //C3.1. Generate a customName
                    string customName = $"Custom Layout ({j})";
                    //C3.2. Check if this customName is already used by other custom item 
                    if (!listOut.Contains(customName))
                    {
                        //C3.3. No, this customName is avaiable to use => add to list
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
                    if (_homeDevice.vmEzArrange == null)
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
                int selectedIndex = 0;
                List<string> friendlyNameList = GenerateCustomNames(out selectedIndex);
                //Create a defulte EAArgs, for pre-defined layout
                EAArgs args = new EAArgs()
                {
                    Command = "EditCommnd",
                    CellCount = 0,
                    SplitKey = 'B',
                    CustomId = 0,
                    CustomName = friendlyNameList[selectedIndex],
                    CustomNames = friendlyNameList,
                    Settings = new List<double>()
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
    }
}