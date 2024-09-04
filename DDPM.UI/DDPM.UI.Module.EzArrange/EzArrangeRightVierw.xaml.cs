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

            //InitRecentListView();
            InitListViewItems();
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

            //A Build WindowLists
            //
            foreach (ISplitCtrl spCtrl in ISplitCtrl.Splits_EA)
            {
                SplitItem? spItem = null;
                ISplitCtrl newSplit = spCtrl.New();
                if (newSplit == null)
                    continue;
                newSplit.SplitMode = eSplitModes.Icon;

                switch (newSplit.CellCount)
                {
                    case 2:
                        spItem = splitListView_2w.AddItemToList(newSplit.UC);
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
                        ISplitCtrl? spCtrl = ISplitCtrl.Create(spj.CellCount, spj.SplitKey);
                        if (spCtrl == null)
                            continue;
                        spCtrl.Settings = new List<double>(spj.Settings);
                        spCtrl.SplitMode = eSplitModes.Icon;
                        spCtrl.FriendlyName = spj.CustomName;

                        SplitItem itemCustom = splitListView_Custom.AddItemToList(spCtrl.UC);
                        itemCustom.SplitOwner = Common.EAEM.eSplitOwner.EaCustom;
                        itemCustom.CustomId = (int)spj.CustomId;
                    }
                }
            } //if (eaSettings != null)

            //C Load & Build Recent List
            //
            //C01. Add "Off" SplitCtrl0A as the first item of RecentList
            ISplitCtrl? sp0A = ISplitCtrl.Create(0, 'A');
            SplitItem item0A;

            if (sp0A != null)
            {
                sp0A.SplitMode = eSplitModes.Icon;
                item0A = splitListView_Recent.AddItemToList(sp0A.UC);
                item0A.SplitOwner = Common.EAEM.eSplitOwner.EaRecent;

                if (eaSettings.SelectedSplit.CellCount == 0)
                {
                    item0A.IsSelected = true;
                    _vm.SelectedSplitItem = item0A;
                }
            }
            //C02. If saved recent list is not empty, then add them into Recent listview
            if (eaSettings?.RecentList != null)
            {
                foreach (DDPM.SA.Common.Display.SplitJson spj in eaSettings.RecentList)
                {
                    ISplitCtrl? spCtrl = ISplitCtrl.Create(spj.CellCount, spj.SplitKey);
                    if (spCtrl == null)
                        continue;
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
                    bool isSelected = spj.IsEquals(eaSettings.SelectedSplit);

                    //If it's a Window item
                    if (spj.CustomId == 0)
                    {
                        SplitItem? itemWin = FindSplitItemFromWindowLists(spj.CellCount, spj.SplitKey);
                        if (itemWin != null)
                        {
                            itemRecent.Buddy = itemWin;
                            itemWin.Buddy = itemRecent;

                            itemRecent.IsSelected = isSelected;
                            itemWin.IsSelected = isSelected;

                            // SplitItem itemRecent = splitListView_Recent.AddItemToList(itemRecent);
                            if (isSelected)
                                _vm.SelectedSplitItem = itemWin;
                        }
                    }
                    else //itemRecent is a custom item
                    {
                        SplitItem? itemCustom = splitListView_Custom.FindItemByCustomId(spj.CustomId);
                        if (itemCustom != null)
                        {
                            itemRecent.Buddy = itemCustom;
                            itemCustom.Buddy = itemRecent;

                            itemRecent.IsSelected = isSelected;
                            itemCustom.IsSelected = isSelected;
                            if (isSelected)
                                _vm.SelectedSplitItem = itemCustom;
                        }
                    }
                }
            } //if (eaSettings?.CustomList != null)

            //D Add all custom items which has no Buddy into Recent list
            //
            foreach (SplitItem itemCustom in splitListView_Custom.SplitList)
            {
                if (itemCustom.Buddy == null)
                {
                    //Duplicate a new SplitItem as item Buddy and add to ListRecent
                    ISplitCtrl? ispCustom = itemCustom.ISplitCtrl;
                    ISplitCtrl? ispRecent = ispCustom.New();
                    ispRecent.Settings = ispCustom.Settings;
                    ispRecent.FriendlyName = ispCustom.FriendlyName;
                    ispRecent.SplitMode = eSplitModes.Icon;

                    SplitItem itemRecent = splitListView_Recent.AddItemToList(ispRecent.UC);
                    itemRecent.Buddy = itemCustom;
                    itemCustom.Buddy = itemRecent;
                    itemRecent.CustomId = itemCustom.CustomId;
                }
            }

            //E Add all Window items which has no buddy into recent list
            foreach (SplitItem itemWin in splitListView_2w.SplitList)
            {
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
            if (spItem.InnerContent is ISplitCtrl)
            {
                ISplitCtrl spCtrl = spItem.InnerContent as ISplitCtrl;
                _vm.SelectedSplitItem = spItem;
                _vm.SetWorkSplit(spCtrl.CellCount, spCtrl.SplitKey);

                splitListView_Recent.MoveSelectedItemToSecondPosition();
                SaveEaSettings();
                //_deviceManagerSA.WriteEasyArrangeSettings()
            }
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

            //If the return item is come from predefined layout (winList)
            if (e.CustomId == 0)
            {
                Dispatcher.Invoke(new Action(() =>
                {
                    //Create a custom item
                    ISplitCtrl ispCustom = ISplitCtrl.Create(e.CellCount, e.SplitKey);
                    if (ispCustom != null)
                    {
                        ispCustom.Settings = e.Settings;
                        ispCustom.FriendlyName = e.CustomName;
                        SplitItem itemCustom = splitListView_Custom.AddItemToList(ispCustom.UC);
                        itemCustom.CustomId = GenerateCustomId();


                        //Add a Buddy to Recent List
                        ISplitCtrl ispRecent = ISplitCtrl.Create(e.CellCount, e.SplitKey);
                        if (ispRecent != null)
                        {
                            ispRecent.Settings = e.Settings;
                            ispRecent.FriendlyName = e.CustomName;
                            SplitItem itemRecent = splitListView_Recent.AddSplitCtrlTo2ndPosition(ispRecent);
                            itemRecent.CustomId = itemCustom.CustomId;

                            itemCustom.Buddy = itemRecent;
                            itemRecent.Buddy = itemCustom;

                            //Set it as current selected
                            _vm.SelectedSplitItem = itemCustom;
                            _vm.SetWorkSplit(itemCustom.CellCount, itemCustom.SplitKey, itemCustom.Settings);
                        }

                        SaveEaSettings();
                    }

                }));
            }
#endif
        }

        private void _deviceManagerSA_EAEditCompleted(object? sender, string e)
        {
#if ENABLE_CALL_SA

            if (_deviceManagerSA != null)
            {
                _deviceManagerSA.EAEditCompleted -= _deviceManagerSA_EAEditCompleted;
                if (_console != null)
                {
                    Dispatcher.Invoke(new Action(() =>
                    {
                        _console.RaiseEvent("MainWindow.Activate", this, new EventManagerArgs());
                        //_console.RaiseEvent("MainWindow.Normal", this, new EventManagerArgs());
                        //_console.RaiseEvent("MainWindow.Show", this, new EventManagerArgs());
                    }
                    ));
                }
            }
#endif
        }

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
            eaSettings.RecentList = new List<SA.Common.Display.SplitJson>();
            foreach (SplitItem itemRecent in splitListView_Recent.SplitList.Skip(1))
            {
                eaSettings.RecentList.Add(itemRecent.ToSplitJson);
            }

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
        }
        #endregion Refresh Data
 
    }
}