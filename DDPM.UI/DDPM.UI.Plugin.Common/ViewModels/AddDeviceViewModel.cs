using CommunityToolkit.Mvvm.ComponentModel;
using DDPM.SA.Common;
using DDPM.SA.Common.Settings;
using DDPM.UI.Common;
using DDPM.UI.Common.Models;
using DDPM.UI.Common.Views;
using DDPM.UI.Interfaces;
using Dell.Client.Framework.Common;
using Dell.Client.Framework.UX.WPF;
using DPeMPublic.Common.Enums;
using Microsoft;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using UserControl = System.Windows.Controls.UserControl;

namespace DDPM.UI.Plugin.ViewModels
{
    public class AddDeviceViewModel : ObservableObject, INotifyPropertyChanged, IAddDeviceViewModel
    {
        private readonly IConsole _console;
        private readonly IShowPluginManager _showPluginManager;
        private readonly ILog _log;

        private int _groupSelIdx = -1;
        private readonly List<DeviceBarItem> _deviceBarItems = new();
        private int _deviceBarSelectedIndex = 0;
        private ICommand? _deviceBarItemClickCommand;

        private readonly string multiDongleAlert = Strings.AddDeviceKnMmultiDongleAlert;
        private readonly string noDongleAlertKnM = Strings.AddDeviceKnMnoDongleAlertKnM;
        private readonly string noDongleAlertHeadset = Strings.AddDeviceKnMnoDongleAlertHeadset;

        public AddDeviceViewModel(IShowPluginManager showPluginManager, IConsole console, ILog log)
        {
            Requires.NotNull(console, nameof(console));
            Requires.NotNull(log, nameof(log));

            _showPluginManager = showPluginManager;
            _console = console;
            _log = log;
        }

        public bool IsPandoraPaired = false; 

        public List<ModuleGroup> _moduleGroups = new();

        public List<ModuleGroup> ModuleGroups
        {
            get => _moduleGroups;
            set
            {
                SetProperty(ref _moduleGroups, value);

                RebuildDeviceBarItems();
                _groupSelIdx = 0;
            }
        }

        public string PairingStatus { get; set; } = "";

        private string _isModuleLoaded = "";

        public string IsModuleLoaded
        {
            get => _isModuleLoaded;
            set
            {
                _isModuleLoaded = value;
                OnPropertyChanged(nameof(IsModuleLoaded));
            }
        }

        public int GroupSelIdx
        {
            get => _groupSelIdx;
            set
            {
                SetProperty(ref _groupSelIdx, value);
                if((_groupSelIdx < 0) || (_groupSelIdx >= GroupCount))
                    return;

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

        public ICommand? DeviceBarItemClickCommand
        {
            get => _deviceBarItemClickCommand;
            set => SetProperty(ref _deviceBarItemClickCommand, value);
        }

        private void RebuildDeviceBarItems()
        {
            _deviceBarItems.Clear();

            int idx = 0;
            foreach(ModuleGroup mg in ModuleGroups)
            {
                if(mg.GroupIcon != null)
                {
                    DeviceBarItem deviceBarItem = new(idx, mg.GroupIcon, mg.GroupName, idx == 3 && WacomVersion == "")
                    {
                        ClickCommand = DeviceBarItemClickCommand
                    };
                    _deviceBarItems.Add(deviceBarItem);
                    idx++;
                }
            }
            OnPropertyChanged("DeviceBarItem");
        }

        public List<DeviceBarItem> DeviceBarItems
        {
            get => _deviceBarItems;
        }

        public int DeviceBarSelectedIndex
        {
            get => _deviceBarSelectedIndex;
            set
            {
                SetProperty(ref _deviceBarSelectedIndex, value);
                if((value < 0) || (value >= GroupCount))
                    return;

                ModuleGroup mg = ModuleGroups[_deviceBarSelectedIndex];
                RightViewHeaders = mg.Headers;
            }
        }

        #region LeftView

        private UserControl? _defaultLeftView;

        public UserControl DefaultLeftView
        {
            get
            {
                _defaultLeftView ??= new DefaultLeftView();
                return _defaultLeftView;
            }
        }

        #endregion LeftView

        #region RightView

        public UserControl? RightView
        {
            get
            {
                ModuleGroup? selGroup = SelectedGroup;
                if(selGroup != null)
                {
                    RightViewHeader selHeader = selGroup.Headers[RightViewHeaderSelectedIndex];
                    if(selHeader != null)
                    {
                        IDdpmModule? mod = selHeader.DdpmModule;
                        if(mod != null)
                            return mod?.GetRightView();
                    }
                }
                return null;
            }
            //set {
            //  SetProperty(ref _leftView, value);
            //}
        }

        private ObservableCollection<RightViewHeader> _rightViewHeaders = new();

        public ObservableCollection<RightViewHeader> RightViewHeaders
        {
            get
            {
                if(_rightViewHeaders.Count == 0)
                {
                    if(ModuleGroups.Count > 0)
                    {
                        if((DeviceBarSelectedIndex >= 0) && (DeviceBarSelectedIndex < (ModuleGroups.Count)))
                        {
                            ModuleGroup mg = ModuleGroups[DeviceBarSelectedIndex];
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
                OnPropertyChanged(nameof(RightView));
            }
        }

        public int RightViewHeaderSelectedIndex
        {
            get
            {
                ModuleGroup? selGroup = SelectedGroup;
                if(selGroup != null)
                {
                    return selGroup.HeaderSelectedIndex;
                }
                return 0;
            }
            set
            {
                ModuleGroup? selGroup = SelectedGroup;
                if(selGroup != null)
                {
                    selGroup.HeaderSelectedIndex = value;
                }
                OnPropertyChanged(nameof(RightViewHeaderSelectedIndex));
                OnPropertyChanged(nameof(RightView));
            }
        }

        #endregion RightView

        public ModuleGroup? SelectedGroup
        {
            get
            {
                if(ModuleGroups.Count <= 0)
                    return null;

                if((DeviceBarSelectedIndex >= 0) && (DeviceBarSelectedIndex < (ModuleGroups.Count)))
                {
                    return ModuleGroups[DeviceBarSelectedIndex];
                }
                return null;
            }
        }

        //public volatile Dictionary<DeviceType, Dictionary<Guid, DongleInfo>> DongleInfos = new();
        public volatile Dictionary<Guid, DongleInfo> DongleInfos = new();

        public volatile Dictionary<Guid, DongleInfo> AudioDongleInfos = new();
        public volatile Dictionary<Guid, List<Guid>> DongleDevices = new();
        public DongleInfo? CurrentDongle = null;
        public string DongleAlertKnM { get; set; } = "";
        public Visibility DongleAlertKnMVisibility { get; set; } = Visibility.Visible;
        public string DongleAlertHeadset { get; set; } = "";
        public Visibility DongleAlertHeadsetVisibility { get; set; } = Visibility.Visible;
        public string AlertText { get; set; } = "";
        public Visibility AlertVisibility { get; set; } = Visibility.Collapsed;

        public void CheckPandora(List<DeviceInfo> DeviceInfos)
        {
            foreach (var info in DeviceInfos)
            {
                if (info.ModelNumber == "PN5122W")
                {
                    IsPandoraPaired = true;
                    return;
                }
            }
            IsPandoraPaired = false;
        }

        public void PrepareDongleInfo(List<DongleInfo> dongleInfos)
        {
            DongleInfos.Clear();
            AudioDongleInfos.Clear();

            foreach(var info in dongleInfos)
            {
                if(info.DeviceType == DeviceType.PhysicalDongle && !DongleInfos.ContainsKey(info.ID))
                {
                    DongleInfos.Add(info.ID, info);
                }
                if(info.DeviceType == DeviceType.PhysicalAudioDongle && !AudioDongleInfos.ContainsKey(info.ID))
                {
                    AudioDongleInfos.Add(info.ID, info);
                }
            }
            //if(DongleInfos.Count == 1) {
            //  CurrentDongle = DongleInfos[DongleInfos.Keys.FirstOrDefault()];
            //}
            //else {
            //  CurrentDongle = null;
            //}
            PrepareDongleInfo();
        }

        //public void PrepareDongleInfo(List<DeviceInfo> deviceInfos) {
        //  DongleInfos.Clear();
        //  AudioDongleInfos.Clear();
        //  foreach(var info in deviceInfos) {
        //    if(info.PhysicalDeviceType == DeviceType.PhysicalDongle) {
        //      if(!DongleInfos.ContainsKey(info.PhyscialDeviceID))
        //        DongleInfos.Add(info.PhyscialDeviceID, info.PairedDeviceCount == info.MaxPairingSlots);
        //    }
        //    else if(info.PhysicalDeviceType == DeviceType.PhysicalAudioDongle) {
        //      AudioDongleInfos.Add(info.PhyscialDeviceID, info.PairedDeviceCount == info.MaxPairingSlots);
        //    }
        //    if(!DongleDevices.ContainsKey(info.PhyscialDeviceID))
        //      DongleDevices.Add(info.PhyscialDeviceID, new List<Guid>());
        //    DongleDevices[info.PhyscialDeviceID].Add(info.ID);
        //  }
        //  PrepareDongleInfo();
        //}

        private void PrepareDongleInfo()
        {
            DongleAlertKnMVisibility = Visibility.Collapsed;
            DongleAlertHeadsetVisibility = Visibility.Collapsed;

            if(DongleInfos.Count == 0)
            {
                DongleAlertKnM = noDongleAlertKnM;
                DongleAlertKnMVisibility = Visibility.Visible;
            }
            if(DongleInfos.Count > 1)
            {
                DongleAlertKnM = multiDongleAlert;
                DongleAlertKnMVisibility = Visibility.Visible;
            }
            if(AudioDongleInfos.Count == 0)
            {
                DongleAlertHeadset = noDongleAlertHeadset;
                DongleAlertHeadsetVisibility = Visibility.Visible;
            }
            if(AudioDongleInfos.Count > 1)
            {
                DongleAlertHeadset = multiDongleAlert;
                DongleAlertHeadsetVisibility = Visibility.Visible;
            }
            OnPropertyChanged(nameof(DongleAlertKnM));
            OnPropertyChanged(nameof(DongleAlertHeadset));
            OnPropertyChanged(nameof(DongleAlertKnMVisibility));
            OnPropertyChanged(nameof(DongleAlertHeadsetVisibility));

            if(DeviceBarSelectedIndex == 2 && DongleAlertKnMVisibility == Visibility.Collapsed && RightViewHeaderSelectedIndex == 1)
            {
                CurrentDongle = DongleInfos.Values.First();
                StartPairing(CurrentDongle.ID);
            }
            if (DeviceBarSelectedIndex == 4 && DongleAlertHeadsetVisibility == Visibility.Collapsed && RightViewHeaderSelectedIndex == 1)
            {
                CurrentDongle = AudioDongleInfos.Values.First();
                StartPairing(CurrentDongle.ID);
            }
            //if (DeviceBarSelectedIndex == 3 && RightViewHeaderSelectedIndex == 1)
            //{
            //    StartPairingPen();
            //}
        }

        public virtual void HandleNotification(DeviceChangedType changeType, DeviceInfo di, string property = "")
        {
            switch(changeType)
            {
                case DeviceChangedType.Peripherals_PlugIn:

                    break;

                case DeviceChangedType.Peripherals_UnPlug:

                    break;

                case DeviceChangedType.Peripherals_SettingsChange:
                    var properties = property.Split('|');
                    switch(properties[0])
                    {
                        case "DonglePairedDeviceCountChanged":
                            if(di.PhysicalDeviceType == DeviceType.PhysicalAudioDongle)
                            {
                                //GotoNewDevice();
                                PairingStatus = "Request";
                                IsPairing = true;
                                OnPropertyChanged(nameof(PairingStatus));
                                return;
                            }
                            break;

                        case "DonglePairingStatusChanged":
                            RequestDeviceName = properties[1];
                            switch(di.PairingStatusName)
                            {
                                case "Request":
                                    break;

                                case "Already Paired":
                                    break;

                                case "Stopped":
                                    break;

                                default:
                                    break;
                            }
                            PairingStatus = di.PairingStatusName;
                            OnPropertyChanged(nameof(PairingStatus));
                            break;

                        default:

                            break;
                    }
                    break;

                default:
                    break;
            }
        }

        public string RequestDeviceName = "";

        public bool IsPairing = false;

        public void StartPairing(Guid guid)
        {
            DdpmCommonHelper.DeviceManagerSA!.StartPairing(guid);
            IsPairing = true;
        }
        public void StartPairingPen()
        {
            DdpmCommonHelper.DeviceManagerSA!.PairingPen();
        }

        public void StopPairing()
        {
            if (IsPairing && CurrentDongle != null)
            {
                DdpmCommonHelper.DeviceManagerSA!.StopPairing(CurrentDongle.ID);
            }
            IsPairing = false;
        }
        public void StopPairingPen()
        {
            if (IsPairing)
            {
                //DdpmCommonHelper.DeviceManagerSA!.StopPairingPen();
            }
            IsPairing = false;
        }

        public DeviceInfo? NewDevice = null;
        public string WacomVersion = "";

        public void GotoNewDevice()
        {
            switch(NewDevice!.LogicalDeviceType.ToUpper())
            {
                case "LOGICALKEYBOARD":
                    _showPluginManager?.ShowPluginById(DDPM.UI.Common.Constants.KeyboardPluginId, NewDevice.ID.ToString());
                    break;

                case "LOGICALMOUSE":
                    _showPluginManager?.ShowPluginById(DDPM.UI.Common.Constants.MousePluginId, NewDevice.ID.ToString());
                    break;

                case "LOGICALHEADSET":
                    _showPluginManager?.ShowPluginById(DDPM.UI.Common.Constants.HeadsetPluginId, NewDevice.ID.ToString());
                    break;

                case "LOGICALWIREDAUDIO":
                    _showPluginManager?.ShowPluginById(DDPM.UI.Common.Constants.SoundBarPluginId, NewDevice.ID.ToString());
                    break;
            }
            NewDevice = null;
            IsPairing = false;
            PairingStatus = "Stopped";
            OnPropertyChanged(nameof(PairingStatus));
        }
    }
}