using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DDPM.SA.Common;
using DDPM.SA.Common.Interfaces;
using DDPM.UI.Common;
using DDPM.UI.Common.Interfaces;
using DDPM.UI.Common.Interfaces.ViewModels;
using DDPM.UI.Common.Models;
using DDPM.UI.Common.UserControls;
using DDPM.UI.Interfaces;
using Dell.Client.Framework.Common;
using Dell.Client.Framework.UX.WPF;
using Microsoft;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using UserControl = System.Windows.Controls.UserControl;

namespace DDPM.UI.Plugin.Common.ViewModels
{
    public class DisplayViewModel : ObservableObject, IDisplayViewModel, IModuleOwner
    {
        #region private members

        private readonly IConsole _console;
        private readonly ILog _log;
        private readonly IDeviceManagerSA _deviceMnagerSA;
        private readonly IEasyArrangeService _easyArrange;

        #endregion private members

        public DisplayViewModel(IConsole console, ILog log, IDeviceManagerSA? deviceManagerSA = null, IEasyArrangeService? easyArrange = null)
        {
            Requires.NotNull(console, nameof(console));
            Requires.NotNull(log, nameof(log));
            //Requires.NotNull(deviceManagerSA, nameof(deviceManagerSA));
            //Requires.NotNull(easyArrange, nameof(easyArrange));

            _console = console;
            _log = log;
            _deviceMnagerSA = deviceManagerSA;
            _easyArrange = easyArrange;
        }

        #region DCF/DUCA Interfaces

        public IConsole Console
        { get { return _console; } }
        public ILog Log
        { get { return _log; } }

        #endregion DCF/DUCA Interfaces

        #region DDPM.SA Interfaces

        public IDeviceManagerSA DeviceManagerSA
        { get { return _deviceMnagerSA; } }
        public IEasyArrangeService EasyArrangeService
        { get { return _easyArrange; } }

        #endregion DDPM.SA Interfaces

        #region HomeDevices

        private HomeDevice _selectedHomeDevice;
        private List<HomeDevice> _homeDevices;

        public HomeDevice? SelectedHomeDevice
        {
            get => _selectedHomeDevice;
            set
            {
                if (value != _selectedHomeDevice)
                {
                    SetProperty(ref _selectedHomeDevice, value);
                    if (_selectedHomeDevice != null)
                    {
                        //Selection changed
                        HandleSelectedHomeDeviceChanged();
                    }
                }
            }
        }

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

        #endregion HomeDevices

        #region Module Manager

        #region Groups

        //All modules of DisplayPlugin
        private List<ModuleGroup> _moduleGroups = new List<ModuleGroup>();

        private List<VbarItem> _vbarItems = new List<VbarItem>();
        private List<VbarItem1> _vbarItems1 = new List<VbarItem1>();

        public int GroupCount
        {
            get
            {
                return _moduleGroups.Count;
            }
        }

        public List<VbarItem> VbarItems
        {
            get => _vbarItems;
        }

        public List<VbarItem1> VbarItems1
        {
            get => _vbarItems1;
        }

        #endregion Groups

        #region Init - Module Manager

        //This method must be set once from UI thread
        public void SetModuleGroups(List<ModuleGroup> moduleGroups)
        {
            _moduleGroups = moduleGroups;
            //Set IModuleOwner to each IDdpmModule
            //
            foreach (ModuleGroup g in moduleGroups)
            {
                foreach (RightViewHeader h in g.Headers)
                {
                    if (h.DdpmModule != null)
                        h.DdpmModule.ModuleOwner = this;
                }
            }

            //SetProperty(ref _moduleGroups, value);
            //Rebuld VbarItems for varList ListControl.ItemsSource
            RebuildVbarItems1();
            //Will rebuild all ModuleManager, reset to Landing mode
            GroupSelectedIndex = -1;
        }

        //_moduleGroups must be build at first before calling to this method.
        private void RebuildVbarItems()
        {
            _vbarItems.Clear();

            int idx = 0;
            foreach (ModuleGroup mg in _moduleGroups)
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

        private void RebuildVbarItems1()
        {
            _vbarItems1.Clear();

            int idx = 0;
            foreach (ModuleGroup mg in _moduleGroups)
            {
                if (mg.GroupIcon != null)
                {
                    //VbarItem vbarItem = new VbarItem(idx, mg.GroupIcon, mg.GroupName);
                    //vbarItem.ClickCommand = new RelayCommand<VbarItem>(OnVbarItemClicked);
                    //_vbarItems.Add(vbarItem);

                    if (mg.IconTemplate != null)
                    {
                        VbarItem1 vbarItem = new VbarItem1()
                        {
                            Index = idx++,
                            Text = mg.GroupName,
                            IconTemplate = mg.IconTemplate
                        };
                        _vbarItems1.Add(vbarItem);
                    }
                    else
                    {
                    }
                }
            }
            OnPropertyChanged("VbarItems1");
        }

        #endregion Init - Module Manager

        #region Group Selection

        private int _groupSelectedIndex = -1; //-1 = no selection, the DisplayPage is in Landing Mode

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
                ModuleGroup mg = _moduleGroups[_groupSelectedIndex];
                RightViewHeaders = mg.Headers;

                //Update VbarItem.IsSelected
                int idx = 0;
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
            }
        }

        public ModuleGroup? SelectedGroup
        {
            get
            {
                if (GroupCount <= 0)
                    return null;

                if ((GroupSelectedIndex >= 0) && (GroupSelectedIndex < GroupCount))
                {
                    return _moduleGroups[GroupSelectedIndex];
                }
                return null;
            }
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

                //    //Transit to TwoView mode
                //    InvokeGotoTwoViewModeAnimation();

                //    //Set the RightViewHeader seklection to 0
                //    //_ivm.RightViewHeaderSelectedIndex = 0;

                //    RightFrame.Visibility = Visibility.Visible;
            }

            if (clickedItem == null)
                return;

            //Check if clickedItem is the same Group (selection is NOT changed)
            if (clickedItem.Id == GroupSelectedIndex)
                return;
            //VbarItem selection is changed

            GroupSelectedIndex = clickedItem.Id;

            ////Determine the selection index of Vbar items[]
            //int newSelectedVarItem = -1;
            //if ((newItem != null) && (_ivm != null))
            //{
            //    newSelectedVarItem = newItem.Id;
            //}

            ////Check if VbarItem selection is NOT changed, if Yes, noting to do
            //if (newSelectedVarItem == _ivm?.VbarSelectedIndex)
            //    return;

            //if (_ivm != null)
            //{
            //    //Change the Vbar item selection index
            //    _ivm.VbarSelectedIndex = newSelectedVarItem;

            //    //Due to RightViewHeaderCtrl has no SelectionChanged event
            //    //
            //    if (_ivm != null)
            //    {
            //        if (_ivm.RightViewHeaders != null)
            //        {
            //            rightViewHeaderCtrl.SetHeaders(_ivm.RightViewHeaders.ToArray());
            //        }
            //    }
            //}
        }

        #endregion Group Selection

        #region Landing Mode

        public bool IsLandingMode { get => (GroupSelectedIndex < 0); }

        //Provide to DeviceBasePage to register a event hander. when we are levaing LandingMode
        public event RoutedEventHandler? LeaveLandingMode;

        #endregion Landing Mode

        #region RightViewHeaders

        private ObservableCollection<RightViewHeader> _rightViewHeaders
                                = new ObservableCollection<RightViewHeader>();

        public ObservableCollection<RightViewHeader> RightViewHeaders
        {
            get
            {
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

        #endregion RightViewHeaders

        #region LeftView

        private UserControl? _defaultLeftView;

        public UserControl? DefaultLeftView
        {
            get => _defaultLeftView;
            set
            {
                SetProperty(ref _defaultLeftView, value);
            }
        }

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
                            selHeader.DdpmModule = Activator.CreateInstance(selHeader.ModuleType, this) as IDdpmModule;
                            ActiveModule = selHeader.DdpmModule;
                            return ActiveModule.GetRightView();
                        }
                    }
                }
                return null;
            }
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

        #endregion Module Manager

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
            foreach (ModuleGroup group in _moduleGroups)
            {
                foreach (RightViewHeader header in group.Headers)
                {
                    if (header.DdpmModule != null)
                        //header.DdpmModule.OnSelectedHomeDeviceChanged(SelectedHomeDevice);
                        header.DdpmModule.OnSelectedHomeDeviceChanged();
                }
            }

            //[Dean 1001]for hotkey to set current selected display device to SA
            if (_deviceMnagerSA != null && _selectedHomeDevice != null && _selectedHomeDevice.MonitorInfo != null)
                _deviceMnagerSA.SetLastSelectedMonitorFromUI(_selectedHomeDevice.MonitorInfo);
        }

        #endregion HandleSelectedHomeDeviceChanged
    }
}