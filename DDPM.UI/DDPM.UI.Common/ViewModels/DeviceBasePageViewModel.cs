#define USE_VBARITEM1

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DDPM.SA.Common;
using DDPM.SA.Common.Settings;
using DDPM.UI.Common.Interfaces;
using DDPM.UI.Common.Models;
using DDPM.UI.Common.UserControls;
using DDPM.UI.Interfaces;
using Dell.Client.Framework.Common;
using Dell.Client.Framework.UX.WPF;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using UserControl = System.Windows.Controls.UserControl;

namespace DDPM.UI.Common.ViewModels
{
    public class DeviceBasePageViewModel : ObservableObject, IModuleOwner
    {
        #region ctor
        public DeviceBasePageViewModel()
        {
            IConsole _console = DdpmCommonHelper.MyConsole;
            if (_console != null)
            {
                //Robert_Lin, 2024-12-21 Register a event handler to handle when mainwindow
                // move to new position
                _console.RegisterForEvent(ConsoleEventNames.MainWindow_MoveToNewPosition, Handle_MainWindow_MoveToNewPosition);
            }

        }
        #endregion ctor

        #region ModuleGroups

        private List<ModuleGroup> _moduleGroups = new List<ModuleGroup>();
        private int _groupSelectedIndex = -1; //-1 = no selection, the DisplayPage is in Landing Mode

        public List<ModuleGroup> ModuleGroups
        {
            get => _moduleGroups;
            set
            {
                //Set IModuleOwner to each IDdpmModule
                //
                foreach (ModuleGroup g in value)
                {
                    foreach (RightViewHeader h in g.Headers)
                    {
                        if (h.DdpmModule != null)
                            h.DdpmModule.ModuleOwner = this;
                    }
                }

                SetProperty(ref _moduleGroups, value);
                //Rebuld VbarItems for varList ListControl.ItemsSource
                RebuildVbarItems();
                //Will rebuild all ModuleManager, reset to Landing mode
                GroupSelectedIndex = -1;

                RefreshGroupManagerUIByModuleCapabilities();
            }
        }

        public int GroupSelectedIndex
        {
            get => _groupSelectedIndex;

            //When VbarItem is clicked, DisplayPage will set to this property
            set
            {
                SetProperty(ref _groupSelectedIndex, value);
                OnPropertyChanged("IsLandingMode");

                //Validate value, allow set to -1 for reset to Landing mode, but should avoid
                //to access to Groups
                if ((_groupSelectedIndex < 0) || (_groupSelectedIndex >= GroupCount))
                    return;

                //When ModuleGroup selection changed, need to update Headers and its selection,
                ModuleGroup mg = ModuleGroups[_groupSelectedIndex];
                RightViewHeaders = mg.Headers;

                //Update VbarItem.IsSelected
                int idx = 0;
#if USE_VBARITEM1
                foreach (VbarItem1 vbItem in VbarItems)
                {
                    if (idx == GroupSelectedIndex)
                    {
                        vbItem.IsSelected = true;
                    }
                    else
                    {
                        vbItem.IsSelected = false;
                    }
                    idx++;
                }
#else
                foreach (VbarItem vbItem in VbarItems)
                {
                    if (idx == GroupSelectedIndex)
                    {
                        vbItem.IsSelected = true;
                    }
                    else
                    {
                        vbItem.IsSelected = false;
                    }
                    idx++;
                }
#endif
            }
        }

        public int GroupCount
        {
            get
            {
                return _moduleGroups.Count;
            }
        }

        public ModuleGroup? SelectedGroup
        {
            get
            {
                if (ModuleGroups.Count <= 0)
                    return null;

                if ((GroupSelectedIndex >= 0) && (GroupSelectedIndex < (ModuleGroups.Count)))
                {
                    return ModuleGroups[GroupSelectedIndex];
                }
                return null;
            }
        }

        public bool IsLandingMode { get => (GroupSelectedIndex < 0); }

        /// <summary>
        /// Find the index with the specific GroupName
        /// </summary>
        /// <param name="groupName">Use the string in Constants.GroupName_XXXX</param>
        /// <returns></returns>
        public int FindGroupIndexByGroupName(string groupName)
        {
            int idx = 0;
            foreach (ModuleGroup mg in ModuleGroups)
            {
                if (mg.GroupName.Equals(groupName))
                    return idx;
                idx++;
            }
            return -1;
        }

        #endregion ModuleGroups

        #region VbarCtrl

#if USE_VBARITEM1
        private List<VbarItem1> _vbarItems = new List<VbarItem1>();

        public List<VbarItem1> VbarItems
        {
            get => _vbarItems;
        }

        private void RebuildVbarItems()
        {
            _vbarItems.Clear();

            int idx = 0;
            foreach (ModuleGroup mg in ModuleGroups)
            {
                //Use IconImage
                VbarItem1 vbarItem = new VbarItem1()
                {
                    Index = idx++,
                    //Text = mg.GroupName, //Robert_Lin,2024-7-26, GroupName is ID used to identify a Group
                    Text = mg.VbarText,      // VbarText is the display string on VbarItem
                    //IconTemplate = mg.IconTemplate
                    IconImage = mg.GroupIcon,
                    IconCanvas = mg.GroupIconCanvas
                };
                //Use IconTemplate (But it not workable)
                //VbarItem1 vbarItem = new VbarItem1()
                //{
                //    Index = idx++,
                //    Text = mg.GroupName,
                //    IconTemplate = mg.IconTemplate
                //    //IconImage = mg.GroupIcon
                //};
                vbarItem.ClickCommand = new RelayCommand<VbarItem1>(OnVbarItemClicked);
                _vbarItems.Add(vbarItem);
            }
            OnPropertyChanged("VbarItems");
        }

        /// <summary>
        /// Handler when a VbarItem is clicked (MouseLeftButtonDown event)
        /// The event handler is installed with below code: (for each VbarItem)
        ///     vbarItem.ClickCommand = new RelayCommand<VbarItem>(OnVbarItemClicked);
        /// </summary>
        /// <param name="clickedItem">The VbarItem which is clicked</param>
        private void OnVbarItemClicked(VbarItem1? clickedItem)
        {
            //If we are in Landing mode
            if (IsLandingMode)
            {
                if (LeaveLandingMode != null)
                    LeaveLandingMode(this, new RoutedEventArgs());

                foreach (VbarItem1 vbarItem in _vbarItems)
                {
                    //vbarItem.SetLadningMode(false);
                    vbarItem.IsLandingMode = false;
                }
            }

            if (clickedItem == null)
                return;

            //Check if clickedItem is the same Group (selection is NOT changed)
            if (clickedItem.Index == GroupSelectedIndex)
                return;
            //VbarItem selection is changed

            GroupSelectedIndex = clickedItem.Index;
            //clickedItem.IsSelected = true;
            //_vbarItems[2].Visibility = Visibility.Collapsed;
        }

#else
        private List<VbarItem> _vbarItems = new List<VbarItem>();
        public List<VbarItem> VbarItems
        {
            get => _vbarItems;
        }

        /// <summary>
        /// Called by ModuleGroups setter when the ModuleGroups reset, will return to Landing Mode
        ///
        /// </summary>
        private void RebuildVbarItems()
        {
            _vbarItems.Clear();

            int idx = 0;
            foreach (ModuleGroup mg in ModuleGroups)
            {
                if (mg.GroupIcon != null)
                {
                    VbarItem vbarItem = new VbarItem(idx, mg.GroupIcon, mg.GroupName);
                    vbarItem.ClickCommand = new RelayCommand<VbarItem>(OnVbarItemClicked);
                    _vbarItems.Add(vbarItem);
                    idx++;
                }
            }
            OnPropertyChanged("VbarItems");
        }

        /// <summary>
        /// Handler when a VbarItem is clicked (MouseLeftButtonDown event)
        /// The event handler is installed with below code: (for each VbarItem)
        ///     vbarItem.ClickCommand = new RelayCommand<VbarItem>(OnVbarItemClicked);
        /// </summary>
        /// <param name="clickedItem">The VbarItem which is clicked</param>
        private void OnVbarItemClicked(VbarItem? clickedItem)
        {
            //If we are in Landing mode
            if (IsLandingMode)
            {
                if (LeaveLandingMode != null)
                    LeaveLandingMode(this, new RoutedEventArgs());

                foreach (VbarItem vbarItem in _vbarItems)
                {
                    vbarItem.SetLadningMode(false);
                }
            }

            if (clickedItem == null)
                return;

            //Check if clickedItem is the same Group (selection is NOT changed)
            if (clickedItem.Id == GroupSelectedIndex)
                return;
            //VbarItem selection is changed

            GroupSelectedIndex = clickedItem.Id;
        }

#endif

        //Provide to DeviceBasePage to register a event hander. when we are levaing LandingMode
        public event RoutedEventHandler? LeaveLandingMode;

        #endregion VbarCtrl

        #region RightViewHeader

        private ObservableCollection<RightViewHeader> _rightViewHeaders
                                = new ObservableCollection<RightViewHeader>();

        public ObservableCollection<RightViewHeader> RightViewHeaders
        {
            get
            {
                //if (_rightViewHeaders.Count == 0)
                //{
                //    //If _vbarItems is empty, will build the list from ModuleGroups
                //    if ((_rightViewHeaders.Count == 0) && (ModuleGroups.Count > 0))
                //    {
                //        //Get the selected ModuleGroup
                //        if ((VbarSelectedIndex >= 0) || (VbarSelectedIndex < (ModuleGroups.Count)))
                //        {
                //            ModuleGroup mg = ModuleGroups[VbarSelectedIndex];
                //            _rightViewHeaders = mg.Headers;
                //        }
                //    }
                //}
                return _rightViewHeaders;
            }
            set
            {
                SetProperty(ref _rightViewHeaders, value);
                OnPropertyChanged(nameof(RightViewHeaderSelectedIndex));
                OnPropertyChanged(nameof(LeftView));
                OnPropertyChanged(nameof(RightView));

                //Expectation:
                // RightViewHeaders propety changed should be notify to RightViewHeaderCtrl.ItemsSource
                // And then RightViewHeaderCtrl.Headers should be updated.
                //But acyually not. So we will manual update with RightViewHeaderCtrl.SetHeaders()
                //
                if (RightViewHeaderChanged != null)
                    RightViewHeaderChanged(this, new RoutedEventArgs());
            }
        }

        public int RightViewHeaderSelectedIndex
        {
            get
            {
                ModuleGroup? selGroup = SelectedGroup;
                if (selGroup != null)
                {
                    return selGroup.HeaderSelectedIndex;
                }
                return 0;
            }
            set
            {
                ModuleGroup? selGroup = SelectedGroup;
                if (selGroup != null)
                {
                    selGroup.HeaderSelectedIndex = value;
                }
                OnPropertyChanged(nameof(RightViewHeaderSelectedIndex));
                OnPropertyChanged(nameof(LeftView));
                OnPropertyChanged(nameof(RightView));
            }
        }

        public RightViewHeader? SelRightViewHeader
        {
            get
            {
                if (SelectedGroup != null)
                {
                    return SelectedGroup.Headers[SelectedGroup.HeaderSelectedIndex];
                }
                return null;
            }
        }

        public event RoutedEventHandler? RightViewHeaderChanged;

        #endregion RightViewHeader

        #region LeftView

        private UserControl? _defaultLeftView;

        public UserControl? DefaultLeftView
        {
            get
            {
                //if (_defaultLeftView == null)
                //    _defaultLeftView = new DisplayDefaultLeftView();
                return _defaultLeftView;
            }
            set
            {
                SetProperty(ref _defaultLeftView, value);
            }
        }

        //Robert_Lin, 2024-6-26, fix SAST issue: [Bug] Refactor this getter so that it actually refers to the field '_leftView'
        //After checked, the _leftView and LeftVeiw.setter should be unused. remove it to verify.
        //OLD Code:
        /*
        private UserControl? _leftView;
        public UserControl? LeftView
        {
            get
            {
                ModuleGroup? selGroup = SelectedGroup;
                if (selGroup != null)
                {
                    RightViewHeader selHeader = selGroup.Headers[RightViewHeaderSelectedIndex];
                    if (selHeader != null)
                    {
                        IDdpmModule? mod = selHeader.DdpmModule;
                        if (mod != null)
                        {
                            UserControl? modLeftView = mod?.GetLeftView();
                            if (modLeftView != null)
                                return modLeftView;
                        }
                    }
                }
                return DefaultLeftView;
            }
            set
            {
                SetProperty(ref _leftView, value);
            }
        }
        */

        //NEW Code:
        public UserControl? LeftView
        {
            get
            {
                ModuleGroup? selGroup = SelectedGroup;
                if (selGroup != null)
                {
                    RightViewHeader selHeader = selGroup.Headers[RightViewHeaderSelectedIndex];
                    if (selHeader != null)
                    {
                        IDdpmModule? mod = selHeader.DdpmModule;
                        if (mod != null)
                        {
                            UserControl? modLeftView = mod?.GetLeftView();
                            if (modLeftView != null)
                                return modLeftView;
                        }
                        else
                        {
                            selHeader.DdpmModule = (IDdpmModule)Activator.CreateInstance(selHeader.ModuleType, this);
                            ActiveModule = selHeader.DdpmModule;
                            return ActiveModule.GetLeftView();
                        }

                    }
                }

                return DefaultLeftView;
            }
        }

        //Jason 12/11 add LoadLeftView()
        public void LoadLeftView()
        {
            OnPropertyChanged(nameof(LeftView));
        }

        #endregion LeftView

        #region RightView

        //private UserControl? _rightView;
        public UserControl? RightView
        {
            get
            {
                ModuleGroup? selGroup = SelectedGroup;
                if (selGroup != null)
                {
                    RightViewHeader selHeader = selGroup.Headers[RightViewHeaderSelectedIndex];
                    if (selHeader != null)
                    {
                        IDdpmModule? mod = selHeader.DdpmModule;
                        if (mod != null)
                        {
                            ActiveModule = mod;
                            return mod?.GetRightView();
                        }
                        else
                        {
                            selHeader.DdpmModule = (IDdpmModule)Activator.CreateInstance(selHeader.ModuleType, this);
                            ActiveModule = selHeader.DdpmModule;
                            return ActiveModule.GetRightView();
                        }
                    }
                }
                return null;
            }
            //Robert_Lin, 2024-6-26, to fix SAST issue of LeftView, I remove _leftView, and found it's referenced by
            //RightView, and RightView.setter should be removed
            //set
            //{
            //    SetProperty(ref _leftView, value);
            //}
        }

        public string RightViewModuleName
        {
            get
            {
                RightViewHeader? header = SelRightViewHeader;
                if (header != null)
                {
                    if (header.DdpmModule != null)
                        return header.DdpmModule.ModuleName;
                }
                return "(ERROR)";
            }
        }

        #endregion RightView

        #region HomeDevices

        private List<HomeDevice>? _homeDevices;

        public List<HomeDevice>? HomeDevices
        {
            get => _homeDevices;
            set
            {
                SetProperty(ref _homeDevices, value);
                OnPropertyChanged("HomeDeviceCount");
                OnPropertyChanged("HomeDevicesComboBoxVisibility");
                OnPropertyChanged("SelectedHomeDeviceTextVisibiliity");
            }
        }

        private HomeDevice? _selectedHomeDevice;

        public HomeDevice? SelectedHomeDevice
        {
            get => _selectedHomeDevice;
            set
            {
                bool isOrgNull = (_selectedHomeDevice == null);
                bool isChanged = (value != _selectedHomeDevice);
                SetProperty(ref _selectedHomeDevice, value);
                if (isChanged)
                {
                    if (!isOrgNull)
                    {
                        //Refresh BatteryIndicator
                        //_selectedHomeDevice.UpdateBatteryIndicator();
                        //Selection changed
                        HandleSelectedHomeDeviceChanged();

                        //Robert_Lin, 2024-11-15 Show the OSD-Product on the selected Monitor
                        if ((DdpmCommonHelper.DeviceManagerSA != null) && (_selectedHomeDevice != null))
                        {
                            DdpmCommonHelper.DeviceManagerSA.ShowOSD(_selectedHomeDevice.MonitorInfo, OSDType.DisplayChanged);
                        }
                     }
                }
            }
        }

        public int HomeDeviceCount
        {
            get
            {
                if (HomeDevices != null)
                    return HomeDevices.Count;
                else
                    return 0;
            }
        }

        public Visibility HomeDevicesComboBoxVisibility
        {
            get
            {
                if (HomeDeviceCount > 1) return Visibility.Visible;
                else
                {
                    return Visibility.Collapsed;
                }
            }
        }

        public Visibility SelectedHomeDeviceTextVisibiliity
        {
            get
            {
                if (HomeDeviceCount == 1) return Visibility.Visible;
                else
                {
                    return Visibility.Collapsed;
                }
            }
        }

        public event EventHandler SelectedHomeDeviceChanged;
        #endregion HomeDevices

        #region LeftFrameWidth

        private double _leftFrameWidth = 680;

        public double LeftFrameWidth
        {
            get => _leftFrameWidth;
            set => SetProperty(ref _leftFrameWidth, value);
        }

        #endregion LeftFrameWidth

        #region FullView

        private ContentControl? _fullView;

        public ContentControl? FullView
        {
            get => _fullView;
            set => SetProperty(ref _fullView, value);
        }

        //public ICommand? OpenFullViewCommand { get; set; }
        //public ICommand? CloseFullViewCommand { get; set; }

        public void OpenFullView(ContentControl content)
        {
            //if (OpenFullViewCommand != null)
            //    OpenFullViewCommand?.Execute(this);
            FullView = content;
            FullView.Visibility = Visibility.Visible;
        }

        public void CloseFullView()
        {
            FullView = null;
        }

        #endregion FullView

        #region HandleSelectedHomeDeviceChanged

        public void HandleSelectedHomeDeviceChanged()
        {
            RefreshGroupManagerUIByModuleCapabilities();
            if ((_selectedHomeDevice != null) && (_selectedHomeDevice.MonitorInfo != null))
                HandleDdcCiOffEvent(_selectedHomeDevice.MonitorInfo.DDCisON);

            if (SelectedHomeDeviceChanged != null)
            {
               SelectedHomeDeviceChanged(this, EventArgs.Empty);
            }

            //handle the last select monitor
            if (DdpmCommonHelper.DeviceManagerSA != null)
            {
                if( DdpmCommonHelper.ModuleOwner != null && 
                    DdpmCommonHelper.ModuleOwner.SelectedHomeDevice != null &&
                    DdpmCommonHelper.ModuleOwner.SelectedHomeDevice.MonitorInfo != null)
                {
                    DdpmCommonHelper.DeviceManagerSA.SetLastSelectedMonitorFromUI(DdpmCommonHelper.ModuleOwner.SelectedHomeDevice.MonitorInfo);
                }
            }

            foreach (ModuleGroup group in ModuleGroups)
            {
                foreach (RightViewHeader header in group.Headers)
                {
                    if (header.DdpmModule != null)
                        header.DdpmModule.OnSelectedHomeDeviceChanged();
                }
            }
        }

        #endregion HandleSelectedHomeDeviceChanged

        #region Handle Module Activated/Deactivated

        private IDdpmModule? _activeModule;

        public IDdpmModule? ActiveModule
        {
            get => _activeModule;
            set
            {
                if (_activeModule == value)
                    return;
                if (_activeModule != null)
                {
                    _activeModule.IsModuleActive = false;
                    _activeModule.OnDeactivated();

                }
                SetProperty(ref _activeModule, value);
                _activeModule.IsModuleActive = true;
                _activeModule?.OnActivated();
            }
        }

        #endregion Handle Module Activated/Deactivated

        #region Module Capabilities
        public event EventHandler ModuleHeaderChanged;

        public void RefreshGroupManagerUIByModuleCapabilities()
        {
            if (SelectedHomeDevice == null)
            {
                LogInfo("@ RefreshGroupManagerUIByModuleCapabilities => SelectedHomeDevice is null.");
                return;
            }
            LogInfo("@ RefreshGroupManagerUIByModuleCapabilities");

            HomeDevice homeDev = SelectedHomeDevice as HomeDevice;
            LogInfo($"  * HomeDevice: {homeDev.DisplayName}");



            //PIP/PBP capability
            LogInfo($"  * Has PIP/PBP Capability={homeDev.HasCapability_PipPbp}");

            foreach (ModuleGroup mg in ModuleGroups)
            {
                RightViewHeader? rightHeader = mg.FindRightViewHeaderByModuleName(Constants.ModuleName_PipPbp);
                if (rightHeader != null)
                {
                    rightHeader.IsShown = homeDev.HasCapability_PipPbp;

                    //If PIP/PBP is not shown, AND current selected module is PIP/PBP
                    if ((!rightHeader.IsShown) && (mg.HeaderSelectedIndex == 1))
                    {
                        //Need update the index to 0 (InputSource)
                        mg.HeaderSelectedIndex = 0;

                        if (SelectedGroup == mg)
                        {
                            RightViewHeaderSelectedIndex = mg.HeaderSelectedIndex;
                        }
                    }
                }
            }

            //KVM Capability
            bool hasCapability_KVM = homeDev.HasCapability_KVM;
            LogInfo($"  * Has KVM Capability={hasCapability_KVM}");

            //Search for ModuleGroup which ModuleName is "KVM"
            ModuleGroup? mgKvm = ModuleGroups.FirstOrDefault(x => x.GroupName.Equals(Constants.GroupName_KVM));
            if (mgKvm != null)
            {
                VbarItem1? vbarItem = VbarItems.Find(x => x.Text.Equals(mgKvm.VbarText));
                if (vbarItem != null)
                {
                    vbarItem.Visibility = (hasCapability_KVM ? Visibility.Visible : Visibility.Collapsed);

                    //Robert_Lin, 2024-8-28, If "KVM" vbar item become Collapsed, and it's current selected Group
                    //Then we will change the selected Group to another visible vbarItem
                    if ((!hasCapability_KVM) && (SelectedGroup != null))
                    {
                        //if (SelectedGroup.GroupName.Equals("KVM"))
                        if (SelectedGroup.GroupName.Equals(Constants.GroupName_KVM))
                        {
                            //Change to EasyArrange
                            //int idxEaGroup = FindGroupIndexByGroupName("EasyArrange");
                            int idxEaGroup = FindGroupIndexByGroupName(Constants.GroupName_EasyArrange);
                            if (idxEaGroup < 0)
                                idxEaGroup = 0;
                            GroupSelectedIndex = idxEaGroup;
                        }
                    }
                }
            }

            //Gaming & VisionEngine
            // Gaming is basic, VisionEngine is additional
            //If there is no Gaming, then hide the Gaming Group
            LogInfo($"  * Has Gaming Capability={homeDev.HasCapability_Gaming}");
            ModuleGroup? mgGaming = ModuleGroups.FirstOrDefault(x => x.GroupName.Equals(Constants.GroupName_Gaming));
            if (mgGaming != null)
            {
                VbarItem1? vbarItem = VbarItems.Find(x => x.Text.Equals(mgGaming.VbarText));
                if (vbarItem != null)
                {
                    vbarItem.Visibility = (homeDev.HasCapability_Gaming ? Visibility.Visible : Visibility.Collapsed);

                    //Robert_Lin, 2024-8-28, If "KVM" vbar item become Collapsed, and it's current selected Group
                    //Then we will change the selected Group to another visible vbarItem
                    if ((homeDev.HasCapability_Gaming) && (SelectedGroup != null))
                    {
                        if (SelectedGroup.GroupName.Equals(Constants.GroupName_Gaming))
                        {
                            RightViewHeader? rightHeader = SelectedGroup.FindRightViewHeaderByModuleName(Constants.ModuleName_VisionEngine);
                            //If Vision Engine is shown, AND current selected module is Vision Engine
                            if ((rightHeader.IsShown) && (SelectedGroup.HeaderSelectedIndex == 1))
                            {
                                //Need update the index to 0 (InputSource)
                                SelectedGroup.HeaderSelectedIndex = 0;

                                if (SelectedGroup == SelectedGroup)
                                {
                                    RightViewHeaderSelectedIndex = SelectedGroup.HeaderSelectedIndex;
                                }
                            }
                            //else
                            //{
                            //    //Change to EasyArrange
                            //    int idxEaGroup = FindGroupIndexByGroupName("EasyArrange");
                            //    if (idxEaGroup < 0)
                            //        idxEaGroup = 0;
                            //    GroupSelectedIndex = idxEaGroup;
                            //}
                        }
                    }
                    else if (SelectedGroup != null && SelectedGroup.GroupName.Equals(Constants.GroupName_Gaming))
                    {
                        //Change to EasyArrange
                        int idxEaGroup = FindGroupIndexByGroupName(Constants.GroupName_EasyArrange);
                        if (idxEaGroup < 0)
                            idxEaGroup = 0;
                        GroupSelectedIndex = idxEaGroup;
                    }
                }
            }

            //Determine if need to show VisionEngine
            if (homeDev.HasCapability_Gaming)
            {
                foreach (ModuleGroup mg in ModuleGroups)
                {
                    RightViewHeader? rightHeader = mg.FindRightViewHeaderByModuleName(Constants.ModuleName_VisionEngine);
                    if (rightHeader != null)
                    {
                        rightHeader.IsShown = homeDev.HasCapability_VisionEngine;
                    }
                }
            }

            //DisplayProperties capability
            //
            //Determine if need to show/hide DisplayProperties header
            //Rule: If has Gaming capability then hide DisplayProperties
            //      Else show DisplayProperies

            //Looking for "DisplayPropertiesModule" module
            foreach (ModuleGroup mg in ModuleGroups)
            {
                RightViewHeader? rightHeader = mg.FindRightViewHeaderByModuleName(Constants.ModuleName_DisplayProperties);
                if (rightHeader != null)
                {
                    rightHeader.IsShown = !homeDev.HasCapability_Gaming;
                    LogInfo($"  * DisplayProperties page isShown={rightHeader.IsShown}");

                    //If CurrentSelected module is "DisplayProperties" 
                    if (!rightHeader.IsShown && mg.HeaderSelectedIndex == 2)
                    {
                        //Need update the index to 1
                        mg.HeaderSelectedIndex = 1;

                        if (SelectedGroup == mg)
                        {
                            RightViewHeaderSelectedIndex = mg.HeaderSelectedIndex;
                        }
                    }
                }
            }

            //Notify DeviceBasePage.xaml.cs to change selected Group/Header
            if (RightViewHeaderChanged != null)
            {
                RightViewHeaderChanged(this, new RoutedEventArgs());
            }
        }

        #endregion Module Capabilities

        #region Handler when DDC/CI off

        public void HandleDdcCiOffEvent(bool isDdcCiOn)
        {
            LogInfo($"@ HandleDdcCiOffEvent(isDdcCiOn={isDdcCiOn})");

            //Find the "EasyArrange" group
            int idxEA = FindGroupIndexByGroupName(Constants.GroupName_EasyArrange);
            string vbarText_EA = "Easy Arrange";
            //Get if EasyArrange group is locked
            bool isEaLocked = false;
            if (idxEA >= 0)
            {
                //If EasyArrange group is NOT locked
                isEaLocked = VbarItems[idxEA].IsLocked;
                vbarText_EA = VbarItems[idxEA].Text;
            }

            //If DDCI is off and EasyArrange group is locked then go to homepage
            if (!isDdcCiOn && isEaLocked)
            {
                LogInfo("  * DDC/CI is off and EasyArrange group is locked, will go back to Homepage.");
                GotoHomepage();
                return;
            }

            //If DDC/CI is off, then switch to Easy Arrange group
            if (!isDdcCiOn)
            {
                //If current is not Landing mode
                if (!IsLandingMode)
                {
                    if (idxEA >= 0)
                    {
                        //If EasyArrange group is locked
                        if (isEaLocked)
                        {
                            //If DDCI is off and EasyArrange group is locked then go to homepage
                            LogInfo("  * DDC/CI is off and EasyArrange group is locked, will go back to Homepage.");
                            GotoHomepage();
                            return;
                        }
                        //Else Change Group selection to "EasyArrange"
                        GroupSelectedIndex = idxEA;
                    }
                }
            }

            //Disable/Enable all other (non EA) Groups (it it's visible)
            for (int i = 0; i < VbarItems.Count; i++)
            {
                //For the non-visible groupes. we don't need to change them
                if (VbarItems[i].Visibility != Visibility.Visible)
                    continue;
                //If it's not EasyArrange group
                if (i != idxEA)
                {
                    VbarItems[i].LeaveHoverState();
                    VbarItems[i].IsEnabled = isDdcCiOn;
                }
            }
        }

        #endregion Handler when DDC/CI off

        #region Log
        private ILog? _log;
        public void InitLog()
        {
            IConsole console = DdpmCommonHelper.MyConsole;
            if (console != null)
            {
                _log = console.CreateLog("BasePageViewModel");
            }
        }
        public void LogInfo(string msg)
        {
            if (_log != null)
                _log.Info(msg);
        }
        #endregion

        #region DCF / DUCA related
        public void GotoHomepage()
        {
            if (DdpmCommonHelper.MyConsole != null)
            {
                DdpmCommonHelper.MyConsole.ShowHomePage();
            }
        }
        #endregion

        #region Set Selected Group/Module 
        public bool ShowDisplayHotkeysModule()
        {
            //Find the terget Group
            int idxGroup = FindGroupIndexByGroupName(Constants.GroupName_InputSource);
            if (idxGroup < 0) //Not found
                return false;
            ModuleGroup mg = ModuleGroups[idxGroup];

            //Set the selected Module
            int idxHeader = mg.SetSelectedRightVewHeaderByModuleName(Constants.ModuleName_DisplayHotkeys);
            if (idxHeader < 0)
                return false;

            //Switch to the terget Group
            GroupSelectedIndex = idxHeader;
            return true;
        }

        /// <summary>
        /// Change to the specific Group/Module
        /// </summary>
        /// <param name="groupName">
        /// One of Constants.GroupName_XXX which defined in DDPM.UI.Common.Constants
        /// </param>
        /// <param name="moduleName"></param>
        public bool ShowSpecificModule(string groupName, string moduleName)
        {
            //Find the terget Group
            int idxGroup = FindGroupIndexByGroupName(groupName);
            if (idxGroup < 0) //Not found
                return false;
            ModuleGroup mg = ModuleGroups[idxGroup];

            //Set the selected Module
            int idxHeader = mg.SetSelectedRightVewHeaderByModuleName(moduleName);
            if (idxHeader < 0)
                return false;

            //Switch to the terget Group
            GroupSelectedIndex = idxGroup;
            return true;
        }
        #endregion  Set Selected Group/Module 

        #region MainWindow Move To new position
        /// <summary>
        /// Handle the IConsole Event, MainWindow_MoveToNewPosition, when DDPM main window
        /// move to a new position.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e">
        /// e.Tag (bool): True if the window is moved by hotkey.
        /// </param>
        private void Handle_MainWindow_MoveToNewPosition(object sender, EventManagerArgs e)
        {
            //Conditions to handle this event
            //1 DisplayPlugin is activate
            if (!DdpmCommonHelper.IsDisplayPluginActivated)
                return;
            //2 DeviceManagerPlugin is ready
            if (DdpmCommonHelper.DeviceManagerSA == null)
                return;
            //3 Has Monitor
            if (HomeDeviceCount == 0) 
                return;

            //TO DO:
            //Action_1: Determine the Monitor where DDPM is moved to
            //Action_2: Show the model name OSD (if the monitor is supported monitor)
            //Action_3: Change the SelectedItem of Monitors combobox (if DisplayPlugin is activated)
            _log?.Info($"@Handle_MainWindow_MoveToNewPosition() is called.");

            //Action_1: Determine the Monitor where DDPM is moved to
            string screenDeviceName = "";

            //If e.Tag contains a DeviceName, then move the specified screen
            if (e.Tag != null)
            {
                if (e.Tag is string)
                {
                    screenDeviceName = e.Tag.ToString();
                }
            }
            //Else the screen will be get from mouse cursor position
            if (String.IsNullOrEmpty(screenDeviceName))
            {
                //Get mouse cursor position
                System.Drawing.Point cursorPosition = System.Windows.Forms.Cursor.Position;
                //Get the Screen of the cursor
                System.Windows.Forms.Screen screenOfCursor = System.Windows.Forms.Screen.FromPoint(cursorPosition);
                screenDeviceName = screenOfCursor.DeviceName;
            }

            //screenName = new move in Screen
            //LastShowOsdScreenDeviceName =  Screen show OSD last time
            //SelectedHomeDevice.MonitorInfo = Current selected monitor

            //Get the current selected monitor's DeviceName
            string selScreenName = "";
            if (SelectedHomeDevice != null)
            {
                if (SelectedHomeDevice.MonitorInfo != null)
                {
                    selScreenName = SelectedHomeDevice.MonitorInfo.DisplayName;
                }
            }
 
            //Check if screen is different with last show
            //if (!screenDeviceName.Equals(DdpmCommonHelper.LastShowOsdScreenDeviceName))

            //If screenName != SelectedHomeDevice
            if (!screenDeviceName.Equals(selScreenName))
            {
                DdpmCommonHelper.LastShowOsdScreenDeviceName = screenDeviceName;

                //Find the monitor of the screen
                HomeDevice? newSelectedDevice = HomeDevices.Find(x => x.MonitorInfo.DisplayName == screenDeviceName);
                if (newSelectedDevice == null)
                {
                    return;
                }
                //Check if newSelected is the same with current Selected
                if (SelectedHomeDevice != null)
                {
                    if (SelectedHomeDevice.MonitorInfo.DisplayName.Equals(newSelectedDevice.MonitorInfo.DisplayName))
                        return;
                }

                SelectedHomeDevice = newSelectedDevice;
            }
        }
        #endregion  MainWindow Move To new position

    }
}