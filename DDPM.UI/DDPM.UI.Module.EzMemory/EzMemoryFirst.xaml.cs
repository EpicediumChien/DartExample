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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static DDPM.UI.Common.User32;
using ProgressBar = System.Windows.Controls.ProgressBar;
using UserControl = System.Windows.Controls.UserControl;
using DDPM.UI.Common.ViewModels;
using Rect = System.Windows.Rect;
using DDPM.UI.Common.Method;
using TextBox = System.Windows.Controls.TextBox;


namespace DDPM.UI.Module.EzMemory
{
    /// <summary>
    /// EzMemoryFirst.xaml 的互動邏輯
    /// </summary>
    public partial class EzMemoryFirst : UserControl
    {
        #region Private Members
        private HomeDevice _homeDevice;
        private IDeviceManagerSA _deviceManagerSA;
        private DDPM.UI.Common.ViewModels.EzArrangeViewModel _vm;    
        private readonly DisplayViewModel _vmDisplay;
        private readonly IConsole _console;
        private readonly ILog _log;
        private HomeDevice _selecthomeDevice;
        //Robert_Lin 2025-1-11 added. The ProfileName when entering this page
        //If the name (InputText) is the same as this (no changed), then we can skip check if duplicate
        private readonly string _orgProfileName = string.Empty;
        private double titleText_DefaultFontSize = 20;
        private double mainText_DefaultFontSize = 60;
        private double subText_DefaultFontSize = 15;
        #endregion Private Members

        public EzMemoryFirst(DisplayViewModel vmDisplay, EzArrangeViewModel vm, HomeDevice _homeDeviceSelect)
        {
            _vmDisplay = vmDisplay;
            _homeDevice = vmDisplay.SelectedHomeDevice;
            _console = vmDisplay.Console;
            _deviceManagerSA = HomeDevice.DeviceManagerSA;
            _selecthomeDevice = _homeDeviceSelect;
            _log = vmDisplay.Console.CreateLog("EzMemoryFirst");
            _log.Info($"{nameof(EzMemoryFirst)} - Constructed");
            Requires.NotNull(vmDisplay, nameof(vmDisplay));
            InitializeComponent();
            if (_homeDevice.vmEzArrange == null)
            {
                _homeDevice.vmEzArrange = new DDPM.UI.Common.ViewModels.EzArrangeViewModel(_homeDevice);
            }
            _vm = vm;// _homeDevice.vmEzArrange;
            DataContext = vm;// _homeDevice.vmEzArrange;

            Screen? currentScreen = GetAttachedScreen(_selecthomeDevice.MonitorInfo.DisplayName);
            _vm.IsVertical = (currentScreen != null) ? (currentScreen.Bounds.Width < currentScreen.Bounds.Height) : false;

            //Robert_Lin 2025-1-9 remove SplitOwner settings for all SplitListViews
            // because all of them do not need Pencel and delX icon
            //
            //splitListView_Recent.SplitOwner = Common.EAEM.eSplitOwner.EaRecent;
            //splitListView_Custom.SplitOwner = Common.EAEM.eSplitOwner.EaCustom;
            //splitListView_2w.SplitOwner = Common.EAEM.eSplitOwner.EaWin;
            //splitListView_3w.SplitOwner = Common.EAEM.eSplitOwner.EaWin;
            //splitListView_4w.SplitOwner = Common.EAEM.eSplitOwner.EaWin;
            //splitListView_5w.SplitOwner = Common.EAEM.eSplitOwner.EaWin;
            //splitListView_6w.SplitOwner = Common.EAEM.eSplitOwner.EaWin;
            //splitListView_7w.SplitOwner = Common.EAEM.eSplitOwner.EaWin;

            splitListView_Recent.ItemClickCommand = new RelayCommand<SplitItem>(OnListViewItemClicked);
            splitListView_Custom.ItemClickCommand = new RelayCommand<SplitItem>(OnListViewItemClicked);
            splitListView_2w.ItemClickCommand = new RelayCommand<SplitItem>(OnListViewItemClicked);
            splitListView_3w.ItemClickCommand = new RelayCommand<SplitItem>(OnListViewItemClicked);
            splitListView_4w.ItemClickCommand = new RelayCommand<SplitItem>(OnListViewItemClicked);
            splitListView_5w.ItemClickCommand = new RelayCommand<SplitItem>(OnListViewItemClicked);
            splitListView_6w.ItemClickCommand = new RelayCommand<SplitItem>(OnListViewItemClicked);
            splitListView_7w.ItemClickCommand = new RelayCommand<SplitItem>(OnListViewItemClicked);

            //SplitListViews in EM First view do not need Edit and Delete command

            //splitListView_Custom.ItemEditCommand = new RelayCommand<SplitItem>(HandleSplitItemEditCommand);
            //splitListView_2w.ItemEditCommand = new RelayCommand<SplitItem>(HandleSplitItemEditCommand);
            //splitListView_3w.ItemEditCommand = new RelayCommand<SplitItem>(HandleSplitItemEditCommand);
            //splitListView_4w.ItemEditCommand = new RelayCommand<SplitItem>(HandleSplitItemEditCommand);
            //splitListView_5w.ItemEditCommand = new RelayCommand<SplitItem>(HandleSplitItemEditCommand);
            //splitListView_6w.ItemEditCommand = new RelayCommand<SplitItem>(HandleSplitItemEditCommand);
            //splitListView_7w.ItemEditCommand = new RelayCommand<SplitItem>(HandleSplitItemEditCommand);

            //splitListView_Custom.ItemDeleteCommand = new RelayCommand<SplitItem>(HandleSplitItemDeleteCommand);

            //InitRecentListView();
            InitListViewItems();

            customListTooltipText.Text = Strings.CustomListTooltipText;

            _orgProfileName = _vm.InputText;

            InitializePage();

            if (_vm.IsEditProfile)
            {
                SyncEditStatusForFirstPage();
            }
            else
            {
                //Robert_Lin, 2025-1-11
                //NEW:
                //If InputText is empty, then generate an unused name for it
                if (string.IsNullOrWhiteSpace(_vm.InputText))
                    _vm.InputText = GenerateProfileName();

                //OLD:
                //CheckInputText();
            }



            //splitListView_Custom Always Collapsed
            //Check Custom split List View count
            //var customList = splitListView_Custom.SplitList.ToList();
            //foreach (var custom in customList)
            //{
            //    if (custom.CellCount == 0 && custom.SplitKey == 'B')
            //        splitListView_Custom.SplitList.Remove(custom);
            //}
            //if (splitListView_Custom.ItemCount == 0)
            //{
            //    splitListView_Custom.Visibility = Visibility.Collapsed;
            //    splitListView_Recent_StackPanel.Visibility = Visibility.Collapsed;
            //}

            ////Always Visible Recent split List View
            //splitListView_Recent_Grid.Visibility = Visibility.Collapsed;
            //splitListView_Recent.Visibility = Visibility.Collapsed;
            titleText_DefaultFontSize = TitleText.FontSize;
            mainText_DefaultFontSize = MainText.FontSize;
            subText_DefaultFontSize = SubText.FontSize;
            LeftGrid.SizeChanged -= AdjustFontSizeForWWO;
            LeftGrid.SizeChanged += AdjustFontSizeForWWO;
            System.Windows.Application.Current.MainWindow.SizeChanged -= MainWindow_SizeChanged;
            System.Windows.Application.Current.MainWindow.SizeChanged += MainWindow_SizeChanged;
        }

        private void MainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (System.Windows.Application.Current.MainWindow.ActualHeight > 765)
            {
                MainPanel.VerticalAlignment = VerticalAlignment.Center;
            }
            else
            {
                MainPanel.VerticalAlignment = VerticalAlignment.Top;
            }
        }

        ~EzMemoryFirst()
        {
            LeftGrid.SizeChanged -= AdjustFontSizeForWWO;
            System.Windows.Application.Current.MainWindow.SizeChanged -= MainWindow_SizeChanged;
        }

        /// <summary>
        /// Initialize Page
        /// </summary>
        public void InitializePage()
        {
            _vm._currentTotalPage = 0;
            _vm._currentPageIndex = 0;
            _vm.ProgressValue = 1;
            _vm.ezPages = _vm.GetEzPages();

            if (_vm.ezPages.ContainsKey(_vm._currentDeviceModel))
            {
                _vm.CurrentAnimationPage = _vm.ezPages[_vm._currentDeviceModel].Count;
                var pageData = _vm.ezPages[_vm._currentDeviceModel][0];
                MainText.Text = pageData.MainText!;
                SubText.Text = pageData.SubText!;
            }
        }

        /// <summary>
        /// Sync Edit Status
        /// </summary>
        public void SyncEditStatusForFirstPage()
        {
            //Robert_Lin, user may has changed the profile name
            //Then click Next, then <- from AssignProgram, then come back to FirstPage
            //The Profile name should not be restored but keep the user's change
            //Need to auto select
            //if (_vm.currentEditprofile != null)
            //    _vm.InputText = _vm.currentEditprofile.Name;

            //Robert_Lin 2025-1-9, CurrentSelectspItem will be selected in InitListViewItems()
            // so comment-out the following code
            //
            /*
            //_vm.ispCtrlForEm = _vm.SelectedSplitItem.ISplitCtrl.Clone();
            SplitItem profilwSplitItem = _vm.CurrentEditSelectspItem; //splitListView_Recent.FindSplitItem(_vm.CurrentSelectspItem.CellCount, _vm.CurrentSelectspItem.SplitKey);
            profilwSplitItem.IsSelected = true;
            OnListViewItemClicked(profilwSplitItem);
            */
        }

        /// <summary>
        /// Check Input Text, the default is set to "Profile", automatically numbered from 1 to 9, and cannot exceed 9 entries.
        /// </summary>
        public void CheckInputText()
        {
            try
            {
                List<EAProfileDDPM> newEAProfileDDPM = DdpmCommonHelper.DeviceManagerSA.ReadUserEAProfileDDPM().Result;

                if (newEAProfileDDPM != null)
                {
                    int profileNumber = 1;
                    bool isDuplicate = false;

                    // 檢查並自動跳號
                    do
                    {
                        string profileNameToCheck = $"Profile {profileNumber}";
                        isDuplicate = newEAProfileDDPM.Any(p => p.Name.Equals(profileNameToCheck, StringComparison.OrdinalIgnoreCase));

                        if (isDuplicate)
                        {
                            profileNumber++;
                        }

                        // 超過 Profile 9設為string.Empty
                        if (profileNumber > 9)
                        {
                            _vm.InputText = string.Empty;
                            _log.Error($"{nameof(EzMemoryFirst)} Exceeded Profile 9. InputText set to string.Empty.");
                            break;
                        }
                        else
                        {
                            _vm.InputText = profileNameToCheck;
                        }
                    }
                    while (isDuplicate);

                    if (!isDuplicate)
                    {
                        _log.Info($"{nameof(EzMemoryFirst)} Unique profile name found: {_vm.InputText}");
                    }
                }
                else
                {
                    _log.Info($"{nameof(EzMemoryFirst)} No EAProfileDDPM found.");
                }
            }
            catch (Exception ex)
            {
                _log.Error($"{nameof(EzMemoryFirst)} Error in CheckInputText: {ex.Message}");
                _vm.InputText = string.Empty; 
            }
        }

        /// <summary>
        /// Next Page
        /// </summary>
        public void NextPage()
        {
            _vm._currentPageIndex++;
            EzMemoryAssignProgram _ezMemoryAssignProgram = new EzMemoryAssignProgram(_vmDisplay, _vm, _selecthomeDevice);
            DdpmCommonHelper.ModuleOwner?.OpenFullView(_ezMemoryAssignProgram);
        }

        /// <summary>
        /// Previous Page
        /// </summary>
        public void PreviousPage()
        {
            if (_vm._currentPageIndex > 0)
            {
                _vm._currentPageIndex--;
                UpdatePageContent();
            }
        }

        /// <summary>
        /// Update Page Content
        /// </summary>
        public void UpdatePageContent()
        {
            if(_vm._currentPageIndex >= 3)
            {
                CancelBtn_Click(null!, null!);               
            }
            var pageData = _vm.ezPages["EzMemory"][_vm._currentPageIndex];
            MainText.Text = pageData.MainText!;
            SubText.Text = pageData.SubText!;
        }

        /// <summary>
        /// Back
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ArrowButton_Click(object sender, RoutedEventArgs e)
        {
            //Robert_Lin 2025-1-11 redefine the meaning of IsEditProfile
            //Only RightView will assign value to it
            // Need to Re-set Edit Profile status
            //_vm.IsEditProfile = false;

            _vm.ClearTextBlockAppName();

            //Robert_Lin 2025-1-11 should not clear RightViewData
            //_vm.RightViewDataClear();

            if (_vm._currentPageIndex == 0)
            {
                DdpmCommonHelper.ModuleOwner?.CloseFullView();
                return;
            }
            PreviousPage();
            DoProgressAnimation(false);
        }

        /// <summary>
        /// Next
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NextBtn_Click(object sender, RoutedEventArgs e)
        {
            if (InputText.Text == "")
            {
                Thickness headMargin = new Thickness(24, 30, 45, 24);
                Thickness subMargin = new Thickness(24, -16, 24, 8);
                DdpmCommonHelper.DDPMEzMesssageBox("", Strings.BlankProfileSubText, true, Window.GetWindow(this), 417, 148, headMargin, subMargin);
                return;
            }
               
            //Robert_Lin 2025-1-9 fix, should be CurrentEditSelectspItem
            //NEW:
            if ((_vm.CurrentEditSelectspItem != null) && (_vm.CurrentEditSelectspItem.CellCount < 2))
            //OLD:
            //if (_vm.SelectedSplitItem.CellCount < 2)
            {
                return;
            }

            //Robert_Lin 2025-1-11 only if user change the profile name, then we need to do duplicate check
            if (!_vm.InputText.Equals(_orgProfileName))

                //Robert_Lin 2025-1-11 No mater Add or Edit mode, both need to check the name is unique or not
                //if (!_vm.IsEditProfile)
            {
                List<EAProfileDDPM> checkEAProfileDDPM = DdpmCommonHelper.DeviceManagerSA.ReadUserEAProfileDDPM().Result;

                if (checkEAProfileDDPM != null &&
                    checkEAProfileDDPM.Any(p => p.Name.Equals(_vm.InputText, StringComparison.OrdinalIgnoreCase)))
                {
                    //Robert_Lin 2025-1-11 follow DDM, will allow case-insensitive duplicate
                    //NEW:
                    if (checkEAProfileDDPM.Any(p => p.Name.Equals(_vm.InputText)))
                        //OLD:
                    //if (checkEAProfileDDPM.Any(p => p.Name.Equals(_vm.InputText, StringComparison.OrdinalIgnoreCase)))
                    {
                        Thickness headMargin = new Thickness(24, 30, 45, 24);
                        Thickness subMargin = new Thickness(24, -16, 24, 8);
                        DdpmCommonHelper.DDPMEzMesssageBox(Strings.msgboxTitleForFirstPage, Strings.subTitleForFirstPage, true, Window.GetWindow(this), 417, 148, headMargin, subMargin);
                        return;
                    }
                }
            }

            NextPage();
            DoProgressAnimation(true);
        }

        /// <summary>
        /// Cancel
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            _vm.ProgressValue = 1;
            _vm.RightViewDataClear();
            DdpmCommonHelper.ModuleOwner?.CloseFullView();
            return;
        }

        /// <summary>
        /// Do Progressbar Animation
        /// </summary>
        /// <param name="isForward"></param>
        private void DoProgressAnimation(bool isForward)
        {
            double newProgressValue;
            if (isForward)
            {
                // Move
                newProgressValue = Math.Min(_vm.ProgressValue + 1, _vm.CurrentAnimationPage);
            }
            else
            {
                // Back
                newProgressValue = Math.Max(_vm.ProgressValue - 1, 1);
            }

            DoubleAnimation progressAnimation = new DoubleAnimation
            {
                From = _vm.ProgressValue,
                To = newProgressValue,
                Duration = new Duration(TimeSpan.FromSeconds(0.5)), // Time
                FillBehavior = FillBehavior.HoldEnd
            };

            EzMemoryProgressbar.BeginAnimation(ProgressBar.ValueProperty, progressAnimation);

            // refresh ProgressValue
            _vm.ProgressValue = newProgressValue;
        }


        #region Init SplitListView and SplitItems
        private void InitListViewItems()
        {
            
            //0. Prepare
            //
            if (_deviceManagerSA == null) return;

            EAMonitorSettings eaSettings = _deviceManagerSA.ReadEAMonitorSettings(_selecthomeDevice.MonitorInfo).Result;

            Screen? currentScreen = GetAttachedScreen(_selecthomeDevice.MonitorInfo.DisplayName);
            _vm.IsVertical = (currentScreen != null) ? (currentScreen.Bounds.Width < currentScreen.Bounds.Height) : false;

            //DisplayOrientation orient = GetDisplayOrientation(_selecthomeDevice.MonitorInfo.DisplayName);
            //_vm.IsVertical = (orient == DisplayOrientation.Angle90) || (orient == DisplayOrientation.Angle270);

            //Update IsVertical to listViews
            splitListView_Recent.IsVertical = _vm.IsVertical;
            splitListView_Custom.IsVertical = _vm.IsVertical;
            splitListView_2w.IsVertical = _vm.IsVertical;
            splitListView_3w.IsVertical = _vm.IsVertical;
            splitListView_4w.IsVertical = _vm.IsVertical;
            splitListView_5w.IsVertical = _vm.IsVertical;
            splitListView_6w.IsVertical = _vm.IsVertical;
            splitListView_7w.IsVertical = _vm.IsVertical;


            //Robert_Lin 2025-1-9, will set selected item based on vm.CurrentEditSelectspItem
            // we will used EAID (Layout) to match
            int eaIdSelected = 0; //Default 0 : No init selected item
            if (_vm.CurrentEditSelectspItem != null && _vm.CurrentEditSelectspItem.ISplitCtrl != null)
            {
                eaIdSelected = _vm.CurrentEditSelectspItem.ISplitCtrl.EAID;
            }

            //A Build WindowLists
            //
            //Robert_Lin 2025-4-15 "Splits_EA" has been changed to AllSplitCtrls
            //foreach (ISplitCtrl spCtrl in ISplitCtrl.Splits_EA)
            foreach (ISplitCtrl spCtrl in ISplitCtrl.AllSplitCtrls)
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
                    spItem.IsEditEnabled = false;//Do not need pencil icon
                    spItem.SplitOwner = Common.EAEM.eSplitOwner.EaWin;

                    //Robert_Lin 2025-1-9, will set selected item based on vm.CurrentEditSelectspItem
                    if (spCtrl.EAID == eaIdSelected)
                    {
                        spItem.IsSelected = true;
                        _vm.CurrentEditSelectspItem = spItem;
                        _vm.CurrentSelectsEAID = spCtrl.EAID;
                    }
                }
            } //foreach(ISplitCtrl spCtrl in ISplitCtrl.Splits_EA)

            //B Load CustomList from settings file
            //
            
            //Robert_Lin, 2024-10-12 modify due to CustomList has move into UserSettings from MonitorSettings, 
            SplitJson[] customList = _deviceManagerSA.ReadEACustomList().Result;
            if (customList != null)
            {
                    //Add saved custom list to custom list view
                foreach (SplitJson spj in customList)
                {
                    //Validate settings
                    //Robert_Lin 2025-4-23, CustomId is not used anymore
                    //1 CustomId must > 0
                    //if (spj.CustomId == 0)
                    //{
                    //    //_vm.LogInfo($"  * InitListViewItems({_homeDevice.MonitorInfo?.modelName},{_homeDevice.MonitorInfo?.edid.ServiceTag}) Settings.CustomList[{spj.CellCount}{spj.SplitKey}], CustomId=[{spj.CustomId}], CustomName=[{spj.CustomName}], Msg=[Invalid setting, CustomId is zero]");
                    //    continue;
                    //}
                    //2 CustomName cannot be empty
                    if (String.IsNullOrWhiteSpace(spj.CustomName))
                    {
                        //_vm.LogInfo($"  * InitListViewItems({_homeDevice.MonitorInfo?.modelName},{_homeDevice.MonitorInfo?.edid.ServiceTag}) Settings.CustomList[{spj.CellCount}{spj.SplitKey}], CustomId=[{spj.CustomId}], CustomName=[{spj.CustomName}], Msg=[Invalid setting, CustomName is empty]");
                        continue;
                    }
                    //3 CustomName length
                    if (spj.CustomName.Length > EAEMConstants.MaxCustomNameLenth)
                    {
                        //_vm.LogInfo($"  * InitListViewItems({_homeDevice.MonitorInfo?.modelName},{_homeDevice.MonitorInfo?.edid.ServiceTag}) Settings.CustomList[{spj.CellCount}{spj.SplitKey}], CustomId=[{spj.CustomId}], CustomName=[{spj.CustomName}], Msg=[Invalid setting, CustomName length is invalid]");
                        continue;
                    }

                    //EM does not support Overlap Custom layouts
                    if (spj.IsOverlapLayout)
                        continue;

                    ISplitCtrl? spCtrl = ISplitCtrl.Create(spj.CellCount, spj.SplitKey);
                    if (spCtrl == null)
                        continue;
                    spCtrl.Settings = new List<double>(spj.Settings);
                    spCtrl.SplitMode = eSplitModes.Icon;
                    spCtrl.FriendlyName = spj.CustomName;
                    spCtrl.EAID = spj.EAID;

                    //Robert_Lin 2025-1-9, EM does not support OverlayLayouts
                    //if (spCtrl.IsAddedCustomLayout)
                    //{
                    //    CreateCellBorderListToSplitCtrlFromCellJsons(spj.Cells, ref spCtrl);
                    //}

                    SplitItem itemCustom = splitListView_Custom.AddItemToList(spCtrl.UC);
                    itemCustom.SplitOwner = Common.EAEM.eSplitOwner.EaCustom;
                    itemCustom.CustomId = (int)spj.CustomId;

                    //Robert_Lin 2025-1-9, will set selected item based on vm.CurrentEditSelectspItem
                    if (spCtrl.EAID == eaIdSelected)
                    {
                        itemCustom.IsSelected = true;
                        //_vm.SelectedSplitItem = itemCustom;
                        _vm.CurrentEditSelectspItem = itemCustom;
                        _vm.CurrentSelectsEAID = spCtrl.EAID;
                    }


                    //Robert_Lin, 2024-10-4 add max items check
                    if (splitListView_Custom.ItemCount >= EAEMConstants.MaxCustomItems)
                        break;
                } //foreach
            } //if (eaSettings != null)

            //Robert_Lin 2025-1-9, EM does not support Recent List 
            /*
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
                    //Validate RectentList items, skip the invalid items
                    //1 CustomId=0 and CustomName is empty is invalid
                    if ((spj.CustomId == 0) && (!String.IsNullOrWhiteSpace(spj.CustomName)))
                    {
                        //_vm.LogInfo($"  * InitListViewItems({_homeDevice.MonitorInfo?.modelName},{_homeDevice.MonitorInfo?.edid.ServiceTag}) Settings.RecentList[{spj.CellCount}{spj.SplitKey}], CustomId=[{spj.CustomId}], CustomName=[{spj.CustomName}], Msg=[Invalid setting, CustomId is zero]");
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
                        //_vm.LogInfo($"  * InitListViewItems({_homeDevice.MonitorInfo?.modelName},{_homeDevice.MonitorInfo?.edid.ServiceTag}) Settings.RecentList[{spj.CellCount}{spj.SplitKey}], CustomId=[{spj.CustomId}], CustomName=[{spj.CustomName}], Msg=[Cannot find Buddy]");
                        idxRecentList++;
                        continue;
                    }

                    //Create a Recent item, and add to RecentList
                    //
                    ISplitCtrl? spCtrl = ISplitCtrl.Create(spj.CellCount, spj.SplitKey);
                    if (spCtrl == null)
                    {
                        //_vm.LogInfo($"  * InitListViewItems({_homeDevice.MonitorInfo?.modelName},{_homeDevice.MonitorInfo?.edid.ServiceTag}) Settings.RecentList[{spj.CellCount}{spj.SplitKey}], CustomId=[{spj.CustomId}], CustomName=[{spj.CustomName}], Msg=[Fail to create ISplitCtrl]");
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
                            //_vm.LogInfo($"  * InitListViewItems({_homeDevice.MonitorInfo?.modelName},{_homeDevice.MonitorInfo?.edid.ServiceTag}) Settings.RecentList[{spj.CellCount}{spj.SplitKey}], CustomId=[{spj.CustomId}], CustomName=[{spj.CustomName}], Msg=[SelectedCount>1]");

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
            */

            //Robert_Lin 2025-1-9, EM does not support Recent List and Buddy
            /*
            //D Add all custom items which has no Buddy into Recent list
            //
            foreach (SplitItem itemCustom in splitListView_Custom.SplitList)
            {
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

                    //_vm.LogInfo($"  * InitListViewItems({_homeDevice.MonitorInfo?.modelName},{_homeDevice.MonitorInfo?.edid.ServiceTag}) CustomList[{ispCustom.CellCount}{ispCustom.SplitKey}], CustomId={itemCustom.CustomId}, CustomName=[{itemCustom.CustomName}], No Buddy setup to RecentList");
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
            */

            //Robert_Lin, 2025-1-9, We has set the SelectedItem in the above code
            /*
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
            */
        }

        //Unused
        //private void CreateCellBorderListToSplitCtrlFromCellJsons(CellJson[] cellJsons, ref ISplitCtrl ispCtrl)
        //{
        //    if (!ispCtrl.IsAddedCustomLayout)
        //        return;

        //    SplitCtrl0B spCtrl0B = (SplitCtrl0B)ispCtrl;
        //    spCtrl0B.CellList.Clear();
        //    if (spCtrl0B.CellBorders != null)
        //        spCtrl0B.CellBorders.Clear();
        //    else
        //        spCtrl0B.CellBorders = new List<CellBorder>();

        //    foreach (CellJson cellJson in cellJsons)
        //    {
        //        CellBorder cellBorder = new CellBorder();
        //        cellBorder.CellName = cellJson.Name;
        //        cellBorder.rcRatio = new Rect(cellJson.x, cellJson.y, cellJson.w, cellJson.h);
        //        spCtrl0B.CellBorders.Add(cellBorder);

        //        CellObj cellObj = new CellObj(cellJson.Name);
        //        cellObj.rcRatio = new Rect(cellJson.x, cellJson.y, cellJson.w, cellJson.h);
        //        spCtrl0B.CellList.Add(cellObj);
        //    }

        //}

        #endregion
        #region SplitListView Operations - Unused
        //private SplitItem? FindSplitItemFromWindowLists(int cellCount, char splitKey)
        //{
        //    switch (cellCount)
        //    {
        //        case 2:
        //            return splitListView_2w.FindSplitItem(cellCount, splitKey);
        //        case 3:
        //            return splitListView_3w.FindSplitItem(cellCount, splitKey);
        //        case 4:
        //            return splitListView_4w.FindSplitItem(cellCount, splitKey);
        //        case 5:
        //            return splitListView_5w.FindSplitItem(cellCount, splitKey);
        //        case 6:
        //            return splitListView_6w.FindSplitItem(cellCount, splitKey);
        //        case 7:
        //            return splitListView_7w.FindSplitItem(cellCount, splitKey);
        //        default: return null;
        //    }
        //}
        #endregion SplitListView Operations
        #region Delete Custom Layout item
        /*
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

            //Robert_Lin 2025-1-10, If delting item is selected, then force to select none
            if (spItem.IsSelected)
            {
                _vm.CurrentEditSelectspItem = null;
            }
            splitListView_Custom.DeleteSplitItem(spItem);

            //If the deleted Custom item is Selected
            //if (spItem.IsSelected)
            //{
            //    //Force to select none
            //    //SplitItem item0A = splitListView_Recent.SplitList[0];
            //    //_vm.SelectedSplitItem = item0A;
            //    //_vm.SetWorkSplit(_vm.SelectedSplitItem.CellCount, _vm.SelectedSplitItem.SplitKey, _vm.SelectedSplitItem.Settings);
            //}
            //SaveEaSettings();
        }
        */
        #endregion Delete Custom Layout item
        #region Screen
        private Screen? GetAttachedScreen(string deviceName)
        {
            return Screen.AllScreens.FirstOrDefault(x => x.DeviceName.Equals(deviceName));
        }

        private static DisplayOrientation GetDisplayOrientation(string deviceName)
        {
            int ENUM_CURRENT_SETTINGS = -1;
            Common.User32.DEVMODE devMode = new Common.User32.DEVMODE();
            if (User32._EnumDisplaySettings(deviceName, ENUM_CURRENT_SETTINGS, ref devMode))
            {
                return (DisplayOrientation)devMode.dmDisplayOrientation;
            }
            return DisplayOrientation.Unknow;
        }
        #endregion
        #region Clean up SplitListView and Items - Unused
        //private void CleanUpListViewItems()
        //{
        //    splitListView_Recent.ClearList();
        //    splitListView_Custom.ClearList();
        //    splitListView_2w.ClearList();
        //    splitListView_4w.ClearList();
        //}
        #endregion Clean up SplitListView and Items
        #region SplitItem Selection
        private void OnListViewItemClicked(SplitItem spItem)
        {
            //if (spItem.InnerContent is ISplitCtrl)
            //{
            //    //ISplitCtrl spCtrl = spItem.InnerContent as ISplitCtrl;
            //    //_vm.CurrentEditSelectspItem = spItem;
            //    //_vm.CurrentSelectsEAID = spCtrl.EAID;
            //    ////_vm.SetWorkSplit(spCtrl.CellCount, spCtrl.SplitKey, spCtrl.Settings);//EM no need

            //    //splitListView_Recent.MoveSelectedItemToSecondPosition();//EM no need
            //    //SaveEaSettings();
            //    //_deviceManagerSA.WriteEasyArrangeSettings()
            //}

            //If Edit，need to recoerd
            //_vm.CurrentSelectspItem = spItem;

            //Robert_Lin 2025-1-9, will copy selected item to vm.CurrentEditSelectspItem
            if (_vm.CurrentEditSelectspItem != null)
                _vm.CurrentEditSelectspItem.IsSelected = false;
            _vm.CurrentEditSelectspItem = spItem;
            _vm.CurrentEditSelectspItem.IsSelected = true;

            if (_vm.CurrentEditSelectspItem != null && _vm.CurrentEditSelectspItem.ISplitCtrl != null)
            {
                _vm.CurrentSelectsEAID = _vm.CurrentEditSelectspItem.ISplitCtrl.EAID;
            }
        }
        #endregion SplitItem Selection
        #region Edit Layout
        /*
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
        */
        #endregion
        #region Custom Layout Manager
        /*
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
                for (int j = 1; j <= MaxCustomItems; j++)
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
        */
        #endregion

        #region For security

        private void InputName_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            TextString textString = new TextString();
            e.Handled = !textString.CheckChar(e.Text);
        }
        #endregion

        #region Profile Name
        private static string GenerateProfileName()
        {
            if (DdpmCommonHelper.DeviceManagerSA == null)
                return string.Empty;

            //Load all EM Profiles from DeviceManager
            List<EAProfileDDPM> emProfiles = DdpmCommonHelper.DeviceManagerSA.ReadUserEAProfileDDPM().Result;
            //Robert_Lin 2025-1-21, if there is no UserSettings in this PC, then will output "Profile 1"
            if (emProfiles == null)
                return "Profile 1";

            //Generate a new Profile Name and check if exists
            const int maxProfileId = 9; //Profille No: 1~9
            for (int i = 1; i <= 9; i++)
            {
                string profileName = $"Profile {i}";
                //If no dupliate, then return this name
                if (!emProfiles.Any(p => p.Name.Equals(profileName, StringComparison.OrdinalIgnoreCase)))
                {
                    return profileName;
                }
            }   
            return string.Empty;
        }
        #endregion

        #region Adjust Text for RWD
        private void AdjustFontSizeForWWO(object sender, RoutedEventArgs e)
        {
            if (TitleText != null && MainText != null && SubText != null)
            {
                // Smaller
                Typeface typeface = new Typeface(new FontFamily("Arial"), FontStyles.Normal, FontWeights.Normal, FontStretches.Normal);
                double txtRowMinWidth = GetLongestWordPixelLength(TitleText.Text, typeface, TitleText.FontSize);
                if (txtRowMinWidth > LeftGrid.ActualWidth)
                {
                    TitleText.FontSize -= 2;
                    txtRowMinWidth = GetLongestWordPixelLength(TitleText.Text, typeface, TitleText.FontSize);
                }
                else if (TitleText.FontSize < titleText_DefaultFontSize)
                {
                    // Bigger
                    txtRowMinWidth = GetLongestWordPixelLength(TitleText.Text, typeface, TitleText.FontSize + 2);
                    if (txtRowMinWidth <= LeftGrid.ActualWidth)
                    {
                        TitleText.FontSize += 2;
                    }
                }

                txtRowMinWidth = GetLongestWordPixelLength(MainText.Text, typeface, MainText.FontSize);
                if (txtRowMinWidth > LeftGrid.ActualWidth)
                {
                    MainText.FontSize -= 2;
                    txtRowMinWidth = GetLongestWordPixelLength(MainText.Text, typeface, MainText.FontSize);
                }
                else if (MainText.FontSize < mainText_DefaultFontSize)
                {
                    // Bigger
                    txtRowMinWidth = GetLongestWordPixelLength(MainText.Text, typeface, MainText.FontSize + 2);
                    if (txtRowMinWidth <= LeftGrid.ActualWidth)
                    {
                        MainText.FontSize += 2;
                    }
                }

                txtRowMinWidth = GetLongestWordPixelLength(SubText.Text, typeface, SubText.FontSize);
                if (txtRowMinWidth > LeftGrid.ActualWidth)
                {
                    SubText.FontSize -= 2;
                    txtRowMinWidth = GetLongestWordPixelLength(SubText.Text, typeface, SubText.FontSize);
                }
                else if (SubText.FontSize < subText_DefaultFontSize)
                {
                    // Bigger
                    txtRowMinWidth = GetLongestWordPixelLength(SubText.Text, typeface, SubText.FontSize + 2);
                    if (txtRowMinWidth <= LeftGrid.ActualWidth)
                    {
                        SubText.FontSize += 2;
                    }
                }
            }
        }

        private double GetLongestWordPixelLength(string text, Typeface typeface, double fontSize)
        {
            if (string.IsNullOrEmpty(text))
                return 0;

            // Split the text into words
            string[] words = text.Split(new[] { ' ', '\t', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            double maxPixelWidth = 0;

            foreach (string word in words)
            {
                double wordWidth = 0;

                if (typeface.TryGetGlyphTypeface(out GlyphTypeface glyphTypeface))
                {
                    foreach (char c in word)
                    {
                        if (glyphTypeface.CharacterToGlyphMap.TryGetValue(c, out ushort glyphIndex))
                        {
                            // Calculate width based on advance widths
                            double advanceWidth = glyphTypeface.AdvanceWidths[glyphIndex];
                            wordWidth += advanceWidth * fontSize;
                        }
                    }
                }

                maxPixelWidth = Math.Max(maxPixelWidth, wordWidth);
            }

            return maxPixelWidth;
        }
        #endregion
    }
}