using CommunityToolkit.Mvvm.ComponentModel;
using DDPM.UI.Common;
using DDPM.UI.Common.Models;
using DDPM.UI.Interfaces;
using DDPM.UI.Plugin.DisplayPlugin.Interfaces;
using DDPM.UI.Plugin.DisplayPlugin.Views;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using System.Windows.Input;
using VcpCore.Common;

[assembly: InternalsVisibleTo("DDPM.UI.Plugin.DisplayPlugin.Tests")]

namespace DDPM.UI.Plugin.DisplayPlugin.ViewModels
{
    internal class DisplayPageViewModel : ObservableObject, IDisplayPageViewModel
    {
        #region ModuleManager

        private List<ModuleGroup> _moduleGroups = new List<ModuleGroup>();
        private int _groupSelIdx = -1; //-1 = no selection, the DisplayPage is in Landing Mode

        public List<ModuleGroup> ModuleGroups
        {
            get => _moduleGroups;
            set
            {
                SetProperty(ref _moduleGroups, value);

                //Will rebuild all ModuleManager, reset to Landing mode
                //

                //1 Reebuld VbarItems for varList ListControl.ItemsSource
                RebuildVbarItems();
                _groupSelIdx = -1;
            }
        }

        public int GroupSelIdx
        {
            get => _groupSelIdx;

            //When VbarItem is clicked, DisplayPage will set to this property
            set
            {
                SetProperty(ref _groupSelIdx, value);

                //Validate value, allow set to -1 for reset to Landing mode, but should avoid
                //to access to Groups
                if ((_groupSelIdx < 0) || (_groupSelIdx >= GroupCount))
                    return;

                //When ModuleGroup selection changed, need to update Headers and its selection,
                ModuleGroup mg = ModuleGroups[_groupSelIdx];
                RightViewHeaders = mg.Headers;
            }
        }

        public int GroupCount
        {
            get
            {
                return _moduleGroups.Count;
            }
        }

        #endregion ModuleManager

        #region Vbar

        private int _vbarSelectedIndex = -1;
        private ICommand? _vbarItemClickCommand;

        /// <summary>
        /// Used by varList ListControl.ItemsSource only
        /// </summary>
        private List<VbarItem> _vbarItems = new List<VbarItem>();

        /// <summary>
        /// Called when the ModuleGroups reset, will return to Landing Mode
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
                    vbarItem.ClickCommand = VbarItemClickCommand;
                    _vbarItems.Add(vbarItem);
                    idx++;
                }
            }
            OnPropertyChanged("VbarItems");
        }

        public List<VbarItem> VbarItems
        {
            get => _vbarItems;
        }

        public int VbarSelectedIndex
        {
            get => _vbarSelectedIndex;
            set
            {
                SetProperty(ref _vbarSelectedIndex, value);

                if ((value < 0) || (value >= GroupCount))
                    return;

                //OnPropertyChanged("RightViewHeaders");
                ModuleGroup mg = ModuleGroups[VbarSelectedIndex];
                RightViewHeaders = mg.Headers;
            }
        }

        public ICommand? VbarItemClickCommand
        {
            get => _vbarItemClickCommand;
            set => SetProperty(ref _vbarItemClickCommand, value);
        }

        #endregion Vbar

        #region LeftView

        private UserControl? _defaultLeftView;

        public UserControl DefaultLeftView
        {
            get
            {
                if (_defaultLeftView == null)
                    _defaultLeftView = new DisplayDefaultLeftView();
                return _defaultLeftView;
            }
        }

        private UserControl? _leftView;

        public UserControl? LeftView
        {
            get
            {
                if (_leftView == null)
                    return DefaultLeftView;

                ModuleGroup? selGroup = SelectedGroup;
                if (selGroup != null)
                {
                    RightViewHeader selHeader = selGroup.Headers[RightViewHeaderSelectedIndex];
                    if (selHeader != null)
                    {
                        IDdpmModule? mod = selHeader.DdpmModule;
                        if (mod != null)
                            return mod?.GetLeftView();
                    }
                }
                return _leftView;
            }
            set
            {
                SetProperty(ref _leftView, value);
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
                            return mod?.GetRightView();
                    }
                }
                return null;
            }
            set
            {
                SetProperty(ref _leftView, value);
            }
        }

        public string RightViewModuleName
        {
            get
            {
                RightViewHeader? header = SelRightViewHeader;
                if (header != null && header.DdpmModule != null)
                    return header.DdpmModule.ModuleName;
                return "(ERROR)";
            }
        }

        #endregion RightView

        #region RightViewHeader

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

        #endregion RightViewHeader

        // After ModuleGroups is build,

        private ObservableCollection<RightViewHeader> _rightViewHeaders = new ObservableCollection<RightViewHeader>();

        public ObservableCollection<RightViewHeader> RightViewHeaders
        {
            get
            {
                if (_rightViewHeaders.Count == 0)
                {
                    //If _vbarItems is empty, will build the list from ModuleGroups
                    if ((_rightViewHeaders.Count == 0) && (ModuleGroups.Count > 0))
                    {
                        //Get the selected ModuleGroup
                        if ((VbarSelectedIndex >= 0) || (VbarSelectedIndex < (ModuleGroups.Count)))
                        {
                            ModuleGroup mg = ModuleGroups[VbarSelectedIndex];
                            _rightViewHeaders = mg.Headers;
                        }
                    }
                }
                return _rightViewHeaders;
            }
            set
            {
                SetProperty(ref _rightViewHeaders, value);
                OnPropertyChanged(nameof(RightViewHeaderSelectedIndex));
                OnPropertyChanged(nameof(LeftView));
                OnPropertyChanged(nameof(RightView));
            }
        }

        public ModuleGroup? SelectedGroup
        {
            get
            {
                if (ModuleGroups.Count <= 0)
                    return null;

                if ((VbarSelectedIndex >= 0) && (VbarSelectedIndex < (ModuleGroups.Count)))
                {
                    return ModuleGroups[VbarSelectedIndex];
                }
                return null;
            }
        }

        //public event RoutedEventHandler? VbarItemClicked;

        public void Reset()
        {
            foreach (ModuleGroup mg in _moduleGroups)
            {
                mg.Headers.Clear();
            }
            _moduleGroups.Clear();
            GroupSelIdx = -1;
            VbarSelectedIndex = -1;
        }

        private List<HomeDevice> _homeDevices = new List<HomeDevice>();

        public List<HomeDevice> HomeDevices
        {
            get => _homeDevices;
            set
            {
                SetProperty(ref _homeDevices, value);
                OnPropertyChanged("HomeDeviceCount");
            }
        }

        public int HomeDeviceCount { get => HomeDevices.Count; }

        private HomeDevice? _selectedHomeDevice;

        public HomeDevice? SelectedHomeDevice
        {
            get => _selectedHomeDevice;
            set => SetProperty(ref _selectedHomeDevice, value);
        }

        private MonitorInfo? _selectedMonitorInfo;

        public MonitorInfo? SelectedMonitorInfo
        {
            get => _selectedMonitorInfo;
            set => SetProperty(ref _selectedMonitorInfo, value);
        }
    }
}