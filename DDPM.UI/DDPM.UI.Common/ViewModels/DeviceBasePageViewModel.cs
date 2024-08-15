#define USE_VBARITEM1

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DDPM.UI.Common.Interfaces;
using DDPM.UI.Common.Models;
using DDPM.UI.Common.UserControls;
using DDPM.UI.Interfaces;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using UserControl = System.Windows.Controls.UserControl;

namespace DDPM.UI.Common.ViewModels
{
    public class DeviceBasePageViewModel : ObservableObject, IModuleOwner
    {
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
                    IconImage = mg.GroupIcon
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
                    }
                }
                return DefaultLeftView;
            }
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
                        //Selection changed
                        HandleSelectedHomeDeviceChanged();
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
                    _activeModule.OnDeactivated();
                SetProperty(ref _activeModule, value);
                _activeModule?.OnActivated();
            }
        }

        #endregion Handle Module Activated/Deactivated

        #region Module Capabilities

        public void RefreshGroupManagerUIByModuleCapabilities()
        {
            if (SelectedHomeDevice == null)
            {
                return;
            }

            HomeDevice homeDev = SelectedHomeDevice as HomeDevice;

            //PIP/PBP capability
            foreach (ModuleGroup mg in ModuleGroups)
            {
                RightViewHeader? rightHeader = mg.FindRightViewHeaderByModuleName("PipPbpModule");
                if (rightHeader != null)
                {
                    rightHeader.IsShown = homeDev.HasCapability_PipPbp;
                }
            }

            //KVM Capability
            bool hasCapability_KVM = homeDev.HasCapability_KVM;

            //Search for ModuleGroup which ModuleName is "KVM"
            ModuleGroup? mgKvm = ModuleGroups.FirstOrDefault(x => x.GroupName.Equals("KVM"));
            if (mgKvm != null)
            {
                VbarItem1? vbarItem = VbarItems.Find(x => x.Text.Equals(mgKvm.VbarText));
                if (vbarItem != null)
                {
                    vbarItem.Visibility = (hasCapability_KVM ? Visibility.Visible : Visibility.Collapsed);
                }
            }

            //Gaming & VisionEngine
            // Gaming is basic, VisionEngine is additional
            //If there is no Gaming, then hide the Gaming Group
            ModuleGroup? mgGaming = ModuleGroups.FirstOrDefault(x => x.GroupName.Equals("Gaming"));
            if (mgGaming != null)
            {
                VbarItem1? vbarItem = VbarItems.Find(x => x.Text.Equals(mgGaming.VbarText));
                if (vbarItem != null)
                {
                    vbarItem.Visibility = (homeDev.HasCapability_Gaming ? Visibility.Visible : Visibility.Collapsed);
                }
            }

            //Determine if need to show VisionEngine
            if (homeDev.HasCapability_Gaming)
            {
                foreach (ModuleGroup mg in ModuleGroups)
                {
                    RightViewHeader? rightHeader = mg.FindRightViewHeaderByModuleName("VisionEngineModule");
                    if (rightHeader != null)
                    {
                        rightHeader.IsShown = homeDev.HasCapability_VisionEngine;
                    }
                }
            }
        }

        #endregion Module Capabilities

        #region Handler when DDC/CI off

        public void HandleDdcCiOffEvent(bool isDdcCiOn)
        {
            //Find the "EasyArrange" group
            int idxEA = -1;
            string vbarText_EA = "Easy Arrange";

            for (int idx = 0; idx < ModuleGroups.Count; idx++)
            {
                if (ModuleGroups[idx].GroupName.Equals("EasyArrange", StringComparison.OrdinalIgnoreCase))
                {
                    idxEA = idx;
                    vbarText_EA = ModuleGroups[idx].VbarText;
                    break;
                }
            }

            //If DDC/CI is off, then switch to Easy Arrange group
            if (!isDdcCiOn)
            {
                //If current is LandingMode, then no selectied item
                //If not Landing mode
                if (!IsLandingMode)
                {
                    //Change Group selection to "EasyArrange"
                    if (idxEA >= 0)
                        GroupSelectedIndex = idxEA;
                }
            }

            //Disable/Enable all other (non EA) Groups (it it's visible)
            foreach (VbarItem1 vbar in VbarItems)
            {
                //For the non-visible groupes. we don't need to change them
                if (vbar.Visibility != Visibility.Visible)
                    continue;
                //If it's not EA
                if (!vbar.Text.Equals(vbarText_EA, StringComparison.OrdinalIgnoreCase))
                {
                    vbar.LeaveHoverState();
                    vbar.IsEnabled = isDdcCiOn;
                }
            }
        }

        #endregion Handler when DDC/CI off
    }
}