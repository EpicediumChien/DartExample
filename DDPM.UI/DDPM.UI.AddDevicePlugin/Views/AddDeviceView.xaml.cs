using CommunityToolkit.Mvvm.Input;
using DDPM.UI.Common;
using DDPM.UI.Interfaces;
using DDPM.UI.Module.AddDisplay;
using DDPM.UI.Module.AddDock;
using DDPM.UI.Module.AddHeadset_BL;
using DDPM.UI.Module.AddHeadset_Dongle;
using DDPM.UI.Module.AddHeadset_Wired;
using DDPM.UI.Module.AddKnM_BL;
using DDPM.UI.Module.AddKnM_Dongle;
using DDPM.UI.Module.AddKnM_Wired;
using DDPM.UI.Module.AddPen_BL;
using DDPM.UI.Module.AddPen_Other;
using DDPM.UI.Module.AddSpeaker;
using DDPM.UI.Module.AddWebcam;
using DDPM.UI.Plugin.Common;
using DDPM.UI.Plugin.ViewModels;
using Dell.Client.Framework.UX.WPF;
using Dell.Client.Framework.UX.WPF.Controls;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace DDPM.UI.Plugin.AddDevicePlugin
{
    /// <summary>
    /// AddDeviceView.xaml 的互動邏輯
    /// </summary>
    public partial class AddDeviceView : UserControl
    {
        private readonly IConsole _console;
        private readonly AddDeviceViewModel? _vm;

        private readonly string Caption = Strings.AddDevice;
        private int selectedTab = -1;
        private readonly string Display = Strings.AddDevice_Display;
        private readonly string Webcam = Strings.AddDevice_Webcam;
        private readonly string KnM = Strings.AddDevice_KnM;
        private readonly string Pen = Strings.AddDevice_Pen;
        private readonly string Headset = Strings.AddDevice_Headset;
        private readonly string Speaker = Strings.AddDevice_Speaker;
        private readonly string Dock = Strings.AddDevice_Dock;
        private readonly string Bluetooth = Strings.AddDeviceTypeBluetooth;
        private readonly string WirelessReceiver = Strings.AddDeviceTypeWireless;
        private readonly string Wired = Strings.AddDeviceTypeWired;
        private readonly string Other = Strings.AddDeviceTypeOther;
        readonly string CancelButtonCaption = Strings.AddDeviceMsgCancelBtn;
        readonly string WaitingCaption = Strings.AddDeviceMsgWaitingCap;
        readonly string WaitingMessage = Strings.AddDeviceMsgWaitingMsg;
        readonly string WaitingAlert = Strings.AddDeviceMsgWaitingAlert;

        public AddDeviceView()
        {
            InitializeComponent();
            _console = AddDevicePlugin.PluginIoc?.GetService<IConsole>()!;
            _vm = (AddDeviceViewModel?)AddDevicePlugin.PluginIoc?.GetService<IAddDeviceViewModel>()!;

            DataContext = _vm;
            _vm.DeviceBarItemClickCommand = new RelayCommand<DeviceBarItem>(OnDeviceBarItemClicked!);

            BuildModuleGroups();
            if (_vm.RightViewHeaders != null)
            {
                rightViewHeaderCtrl.SetHeaders(_vm.RightViewHeaders.ToArray());
            }
            txtCaption.Text = Caption;
            DdpmCommonHelper.BitmapImageUpdated += ImageUpdate;
        }

        private void ImageUpdate(OSThemeEnum oSThemeEnum)
        {
            ArrowLeft.Source = null;
            ArrowLeft.Source = (BitmapImage)Application.Current.Resources["Arrow_Left"];
        }

        private void BuildModuleGroups()
        {
            List<ModuleGroup> groups = new();
            ModuleGroup moduleGroup;

            moduleGroup = new ModuleGroup()
            {
                GroupName = Display,
                GroupIcon = DdpmCommonHelper.GetImageSourceFromCommonResource("Resources/Images/Monitor.png", "DDPM.UI.Resources")
            };
            moduleGroup.AddHeader(Display, new AddDisplayModule(_vm!));
            groups.Add(moduleGroup);

            moduleGroup = new ModuleGroup()
            {
                GroupName = Webcam,
                GroupIcon = DdpmCommonHelper.GetImageSourceFromCommonResource("Resources/Images/Webcamera.png", "DDPM.UI.Resources")
            };
            moduleGroup.AddHeader(Webcam, new AddWebcamModule(_vm!));
            groups.Add(moduleGroup);

            moduleGroup = new ModuleGroup()
            {
                GroupName = KnM,
                GroupIcon = DdpmCommonHelper.GetImageSourceFromCommonResource("Resources/Images/KnM.png", "DDPM.UI.Resources")
            };
            moduleGroup.AddHeader(Bluetooth, new AddKnM_BLModule(_vm!));
            moduleGroup.Headers[0].ImageFile = "Bluetooth.png";
            moduleGroup.AddHeader(WirelessReceiver, new AddKnM_DongleModule(_vm!));
            moduleGroup.Headers[1].ImageFile = "Dongle.png";
            moduleGroup.AddHeader(Wired, new AddKnM_WiredModule(_vm!));
            moduleGroup.Headers[2].ImageFile = "Port.png";
            groups.Add(moduleGroup);

            moduleGroup = new ModuleGroup()
            {
                GroupName = Pen,
                GroupIcon = DdpmCommonHelper.GetImageSourceFromCommonResource("Resources/Images/Pen.png", "DDPM.UI.Resources")
            };
            moduleGroup.AddHeader(Bluetooth, new AddPen_BLModule(_vm!));
            moduleGroup.Headers[0].ImageFile = "Bluetooth.png";
            moduleGroup.AddHeader(Other, new AddPen_OtherModule(_vm!));
            moduleGroup.Headers[1].ImageFile = "";
            groups.Add(moduleGroup);

            moduleGroup = new ModuleGroup()
            {
                GroupName = Headset,
                GroupIcon = DdpmCommonHelper.GetImageSourceFromCommonResource("Resources/Images/Headset.png", "DDPM.UI.Resources")
            };
            moduleGroup.AddHeader(Bluetooth, new AddHeadset_BLModule(_vm!));
            moduleGroup.Headers[0].ImageFile = "Bluetooth.png";
            moduleGroup.AddHeader(WirelessReceiver, new AddHeadset_DongleModule(_vm!));
            moduleGroup.Headers[1].ImageFile = "Dongle.png";
            moduleGroup.AddHeader(Wired, new AddHeadset_WiredModule(_vm!));
            moduleGroup.Headers[2].ImageFile = "Port.png";
            groups.Add(moduleGroup);

            moduleGroup = new ModuleGroup()
            {
                GroupName = Speaker,
                GroupIcon = DdpmCommonHelper.GetImageSourceFromCommonResource("Resources/Images/SpeakerOn.png", "DDPM.UI.Resources")
            };
            moduleGroup.AddHeader(Speaker, new AddSpeakerModule(_vm!));
            groups.Add(moduleGroup);

            moduleGroup = new ModuleGroup()
            {
                GroupName = Dock,
                GroupIcon = DdpmCommonHelper.GetImageSourceFromCommonResource("Resources/Images/Dock.png", "DDPM.UI.Resources")
            };
            moduleGroup.AddHeader(Dock, new AddDockModule(_vm!));
            groups.Add(moduleGroup);

            _vm!.ModuleGroups = groups;
        }

        private void OnDeviceBarItemClicked(DeviceBarItem newItem)
        {
            //if(newItem.Id == _vm!.DeviceBarSelectedIndex) { return; }

            if (_vm!.DeviceBarSelectedIndex >= 0 && _vm!.DeviceBarItems.Find(x => x.Id == _vm!.DeviceBarSelectedIndex) != null)
            {
                if (_vm.DeviceBarItems.Find(item => item.IsSelected == true) == null)
                {
                    _vm.DeviceBarItems[0].SelectBarItem();
                }
                else
                {
                    if (_vm.DeviceBarSelectedIndex == newItem.Id)
                        return;
                    else
                    {
                        _vm.DeviceBarItems[_vm.DeviceBarSelectedIndex].IsSelected = false;
                        _vm.DeviceBarItems[_vm.DeviceBarSelectedIndex].RenewBarItem();
                        _vm.DeviceBarItems[newItem.Id].SelectBarItem();
                    }
                }
            }

            _vm.DeviceBarSelectedIndex = newItem.Id;

            if (_vm.RightViewHeaders != null)
            {
                rightViewHeaderCtrl.SetHeaders(_vm.RightViewHeaders.ToArray());
                rightViewHeaderCtrl.SelectedIndex = _vm.SelectedGroup!.HeaderSelectedIndex;
            }
        }

        private void GoBackHomepage(object sender, MouseButtonEventArgs e)
        {
            _console.ShowHomePage();
        }

        private void RightViewHeaderCtrl_SelectionChanged(object sender, RoutedEventArgs e)
        {
            //if(sender == null)
            //  return;

            int newSelId = rightViewHeaderCtrl.SelectedIndex;
            if (newSelId != selectedTab)
            {
                if (_vm != null)
                {
                    _vm.RightViewHeaderSelectedIndex = newSelId;
                }
                selectedTab = newSelId;
            }

            if (newSelId == 1 && (_vm!.DeviceBarSelectedIndex == 2 || _vm!.DeviceBarSelectedIndex == 3 || _vm!.DeviceBarSelectedIndex == 4))
            {
                StartPairing();
            }
            else
            {
                _vm!.StopPairing();
            }
        }

        private void StartPairing()
        {
            if (_vm!.DeviceBarSelectedIndex == 2 && _vm.DongleInfos.Count == 1)
            {
                if (_vm.DongleInfos.Values.First().PairedDeviceCount == _vm.DongleInfos.Values.First().MaxPairingSlots)
                {
                    ShowMessage(Strings.Error, Strings.DongleSlotFull, "");
                    return;
                }
                else
                {
                    _vm.CurrentDongle = _vm.DongleInfos.Values.First();
                    _vm!.StartPairing(_vm!.DongleInfos.Keys.First());
                }
            }
            if (_vm!.DeviceBarSelectedIndex == 4 && _vm.AudioDongleInfos.Count == 1)
            {
                if (_vm.AudioDongleInfos.Values.First().PairedDeviceCount == _vm.AudioDongleInfos.Values.First().MaxPairingSlots)
                {
                    ShowMessage(Strings.Error, Strings.DongleSlotFull, "");
                    return;
                }
                else
                {
                    _vm.CurrentDongle = _vm.AudioDongleInfos.Values.First();
                    _vm!.StartPairing(_vm!.AudioDongleInfos.Keys.First());
                }
            }
            //if(_vm!.DeviceBarSelectedIndex == 3)
            //    _vm!.StartPairingPen();
        }

        private void ShowMessage(string caption, string text, string button1Caption, string button2Caption = "")
        {
            MessageModalDialog messageModalDialog = new(caption, text, button1Caption, button2Caption);
            Window mainWindow = System.Windows.Application.Current.MainWindow;
            if (mainWindow != null)
            {
                messageModalDialog.Owner = mainWindow;
                messageModalDialog.Left = mainWindow.Left + (mainWindow!.ActualWidth - 417) / 2;
                messageModalDialog.Top = mainWindow.Top + 300;
            }
            messageModalDialog.ShowDialog();
        }

        private void PairingStatusChanged(object sender, TextChangedEventArgs e)
        {
            switch (txtPairingStatus.Text)
            {
                case "Request":
                    if (_vm!.IsPairing)
                    {
                        WaitingModalDialog waitingModalDialog = new(WaitingCaption, $"{WaitingMessage} {_vm.RequestDeviceName}", WaitingAlert);
                        Window mainWindow = System.Windows.Application.Current.MainWindow;
                        waitingModalDialog.WindowStartupLocation = WindowStartupLocation.Manual;
                        if (mainWindow != null)
                        {
                            waitingModalDialog.Owner = mainWindow;
                            waitingModalDialog.Left = mainWindow.Left + (mainWindow.ActualWidth - 587) / 2;
                            waitingModalDialog.Top = mainWindow.Top + (mainWindow.ActualHeight - 349) / 2;
                        }
                        waitingModalDialog.ShowDialog();
                        _vm.GotoNewDevice();
                    }
                    break;

                case "Already Paired":
                    ShowMessage(Strings.Error, Strings.AlreadyPaired, "");
                    _vm!.StopPairing();
                    rightViewHeaderCtrl.SelectedIndex = -1;
                    rightViewHeaderCtrl.SelectedIndex = 1;
                    break;

                case "Old Device":
                    ShowMessage(Strings.Error, Strings.NotSupportedDevice, CancelButtonCaption);
                    _vm!.StopPairing();
                    rightViewHeaderCtrl.SelectedIndex = -1;
                    rightViewHeaderCtrl.SelectedIndex = 1;
                    break;

                case "Stopped":
                    break;

                case "TimeOut":
                    if (_vm.IsPairing)
                    {
                        ShowMessage(Strings.Error, Strings.NoDeviceFound, "");
                        _vm!.StopPairing();
                    }
                    rightViewHeaderCtrl.SelectedIndex = -1;
                    rightViewHeaderCtrl.SelectedIndex = 1;
                    break;

                default:
                    break;
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            OnDeviceBarItemClicked(_vm!.DeviceBarItems[_vm.DeviceBarSelectedIndex]);
            _vm.DeviceBarItems[_vm.DeviceBarSelectedIndex].IsSelected = true;
        }

        private void Pairing(object sender, StylusDownEventArgs e)
        {
            if (_vm!.DeviceBarSelectedIndex != 3 || rightViewHeaderCtrl.SelectedIndex != 1)
                return;
            MessageModalDialog messageModalDialog;
            Window parentWindow = Window.GetWindow(this);
            if (_vm!.IsPandoraPaired)
            {
                messageModalDialog = new(Strings.Error, Strings.PenAlreadyPaired, Strings.Cancel);
                if (parentWindow != null)
                {
                    messageModalDialog.Owner = parentWindow;
                }
                messageModalDialog.ShowDialog();
                return;
            }
            messageModalDialog = new(Strings.PairYourPen, Strings.PairYourPenMessage, Strings.No, Strings.Yes);
            if (parentWindow != null)
            {
                messageModalDialog.Owner = parentWindow;
            }
            if (messageModalDialog.ShowDialog()!.Value)
            {
                _vm.StartPairingPen();
            }
        }
    }
}