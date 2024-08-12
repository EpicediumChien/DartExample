using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using DDPM.UI.Common;
using DDPM.UI.Interfaces;
using DDPM.UI.Plugin.ViewModels;
using Dell.Client.Framework.UX.WPF;
using DDPM.UI.Module.AddDisplay;
using DDPM.UI.Module.AddWebcam;
using DDPM.UI.Module.AddKnM_BL;
using DDPM.UI.Module.AddKnM_Dongle;
using DDPM.UI.Module.AddKnM_Wired;
using DDPM.UI.Module.AddPen_BL;
using DDPM.UI.Module.AddPen_Other;
using DDPM.UI.Module.AddHeadset_BL;
using DDPM.UI.Module.AddHeadset_Dongle;
using DDPM.UI.Module.AddHeadset_Wired;
using DDPM.UI.Module.AddDock;
using DDPM.UI.Module.AddSpeaker;
using DPeMPublic.Common.Enums;
using DDPM.UI.Plugin.Common;
using static System.Net.Mime.MediaTypeNames;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;

namespace DDPM.UI.Plugin.AddDevicePlugin {
  /// <summary>
  /// AddDeviceView.xaml 的互動邏輯
  /// </summary>
  public partial class AddDeviceView : UserControl {
    private readonly IConsole _console;
    private readonly AddDeviceViewModel? _vm;

    private readonly string Caption = "Add Device";
    private int selectedTab = -1;
    private readonly string Display = "Display";
    private readonly string Webcam = "Webcam";
    private readonly string KnM = "Keyboard\nand Mouse";
    private readonly string Pen = "Pen";
    private readonly string Headset = "Headset";
    private readonly string Speaker = "Speakerphone\nand Soundbar";
    private readonly string Dock = "Dock";
    private readonly string Bluetooth = "Bluetooth";
    private readonly string WirelessReceiver = "Wireless receiver";
    private readonly string Wired = "Wired";
    private readonly string Other = "Other";
    readonly string CancelButtonCaption = "Cancel";
    readonly string WaitingCaption = "Adding device, please wait...";
    readonly string WaitingMessage = "Pairing";
    readonly string WaitingAlert = "Pairing may take some time. Do not disconnect your device";

    public AddDeviceView() {
      InitializeComponent();
      _console = AddDevicePlugin.PluginIoc?.GetService<IConsole>()!;
      _vm = (AddDeviceViewModel?)AddDevicePlugin.PluginIoc?.GetService<IAddDeviceViewModel>()!;

      DataContext = _vm;
      _vm.DeviceBarItemClickCommand = new RelayCommand<DeviceBarItem>(OnDeviceBarItemClicked!);

      BuildModuleGroups();
      if(_vm.RightViewHeaders != null) {
        rightViewHeaderCtrl.SetHeaders(_vm.RightViewHeaders.ToArray());
      }
      txtCaption.Text = Caption;
    }

    private void BuildModuleGroups() {
      List<ModuleGroup> groups = new();
      ModuleGroup moduleGroup;

      moduleGroup = new ModuleGroup() {
        GroupName = Display,
        GroupIcon = DdpmCommonHelper.GetImageSourceFromCommonResource("Resources/Images/Monitor.png", "DDPM.UI.Resources")
      };
      moduleGroup.AddHeader(Display, new AddDisplayModule(_vm!));
      groups.Add(moduleGroup);

      moduleGroup = new ModuleGroup() {
        GroupName = Webcam,
        GroupIcon = DdpmCommonHelper.GetImageSourceFromCommonResource("Resources/Images/Webcamera.png", "DDPM.UI.Resources")
      };
      moduleGroup.AddHeader(Webcam, new AddWebcamModule(_vm!));
      groups.Add(moduleGroup);

      moduleGroup = new ModuleGroup() {
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

      moduleGroup = new ModuleGroup() {
        GroupName = Pen,
        GroupIcon = DdpmCommonHelper.GetImageSourceFromCommonResource("Resources/Images/Pen.png", "DDPM.UI.Resources")
      };
      moduleGroup.AddHeader(Bluetooth, new AddPen_BLModule(_vm!));
      moduleGroup.Headers[0].ImageFile = "Bluetooth.png"; 
      moduleGroup.AddHeader(Other, new AddPen_OtherModule(_vm!));
      moduleGroup.Headers[1].ImageFile = "";
      groups.Add(moduleGroup);

      moduleGroup = new ModuleGroup() {
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

      moduleGroup = new ModuleGroup() {
        GroupName = Speaker,
        GroupIcon = DdpmCommonHelper.GetImageSourceFromCommonResource("Resources/Images/SpeakerOn.png", "DDPM.UI.Resources")
      };
      moduleGroup.AddHeader(Speaker, new AddSpeakerModule(_vm!));
      groups.Add(moduleGroup);

      moduleGroup = new ModuleGroup() {
        GroupName = Dock,
        GroupIcon = DdpmCommonHelper.GetImageSourceFromCommonResource("Resources/Images/Dock.png", "DDPM.UI.Resources")
      };
      moduleGroup.AddHeader(Dock, new AddDockModule(_vm!));
      groups.Add(moduleGroup);

      _vm!.ModuleGroups = groups;
    }

    private void OnDeviceBarItemClicked(DeviceBarItem newItem) {
      //if(newItem.Id == _vm!.DeviceBarSelectedIndex) { return; }

      if(_vm!.DeviceBarSelectedIndex >= 0)
        _vm.DeviceBarItems[_vm.DeviceBarSelectedIndex].IsSelected = false;

      _vm.DeviceBarSelectedIndex = newItem.Id;

      if(_vm.RightViewHeaders != null) {
        rightViewHeaderCtrl.SetHeaders(_vm.RightViewHeaders.ToArray());
        rightViewHeaderCtrl.SelectedIndex = _vm.SelectedGroup!.HeaderSelectedIndex;
      }
    }

    private void GoBackHomepage(object sender, MouseButtonEventArgs e) {
      _console.ShowHomePage();
    }
    private void RightViewHeaderCtrl_SelectionChanged(object sender, RoutedEventArgs e) {
      //if(sender == null)
      //  return;

      int newSelId = rightViewHeaderCtrl.SelectedIndex;
      if(newSelId != selectedTab) {
        if(_vm != null) {
          _vm.RightViewHeaderSelectedIndex = newSelId;
        }
        selectedTab = newSelId;
      }

      if(newSelId == 1 && (_vm!.DeviceBarSelectedIndex == 2 || _vm!.DeviceBarSelectedIndex == 4)) {
        StartPairing();
      }
      else {
        _vm!.StopPairing();
      }
    }

    private void StartPairing() {
      if(_vm!.DeviceBarSelectedIndex == 2 && _vm.DongleInfos.Count == 1) {
        if(_vm.DongleInfos.Values.First().PairedDeviceCount== _vm.DongleInfos.Values.First().MaxPairingSlots) {
          ShowMessage(Strings.Error, Strings.DongleSlotFull, "");
          return;
        }
        else {
          _vm.CurrentDongle = _vm.DongleInfos.Values.First();
          _vm!.StartPairing(_vm!.DongleInfos.Keys.First());
        }
      }
      if(_vm!.DeviceBarSelectedIndex == 4 && _vm.AudioDongleInfos.Count == 1) {
        if(_vm.AudioDongleInfos.Values.First().PairedDeviceCount == _vm.AudioDongleInfos.Values.First().MaxPairingSlots) {
          ShowMessage(Strings.Error, Strings.DongleSlotFull, "");
          return;
        }
        else {
          _vm.CurrentDongle = _vm.AudioDongleInfos.Values.First();
          _vm!.StartPairing(_vm!.AudioDongleInfos.Keys.First());
        }
      }
    }

    private void ShowMessage(string caption, string text, string button1Caption, string button2Caption = "") {
      MessageModalDialog messageModalDialog = new(caption, text, button1Caption, button2Caption);
      Window parentWindow = Window.GetWindow(this);
      if(parentWindow != null) {
        messageModalDialog.Owner = parentWindow;
      }
      messageModalDialog.ShowDialog();
    }

    private void PairingStatusChanged(object sender, TextChangedEventArgs e) {
      switch(txtPairingStatus.Text) {
        case "Request":
          if(_vm!.IsPairing) {
            WaitingModalDialog waitingModalDialog = new(WaitingCaption, $"{WaitingMessage} {_vm.RequestDeviceName}", WaitingAlert);
            Window parentWindow = Window.GetWindow(this);
            if(parentWindow != null) {
              waitingModalDialog.Owner = parentWindow;
            }
            waitingModalDialog.ShowDialog();
            if(_vm.NewDevice == null) {
              ShowMessage(Strings.Error, Strings.NoDeviceFound, "");
              _vm!.StopPairing();
              rightViewHeaderCtrl.SelectedIndex = 0;
            }
            else {
              _vm.GotoNewDevice();
            }
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
          ShowMessage(Strings.Error, Strings.NoDeviceFound, "");
          _vm!.StopPairing();
          rightViewHeaderCtrl.SelectedIndex = -1;
          rightViewHeaderCtrl.SelectedIndex = 1;
          break;
        default:
          break;
      }
    }

    private void UserControl_Loaded(object sender, RoutedEventArgs e) {
      OnDeviceBarItemClicked(_vm!.DeviceBarItems[_vm.DeviceBarSelectedIndex]);
      _vm.DeviceBarItems[_vm.DeviceBarSelectedIndex].IsSelected = true;
    }
  }
}
