using CommunityToolkit.Mvvm.Input;
using DDPM.SA.Common;
using DDPM.SA.Common.Settings;
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
using DDPM.UI.Resources.Helper;
using Dell.Client.Framework.Common;
using Dell.Client.Framework.UX.WPF;
using Dell.Client.Framework.UX.WPF.Controls;
using DPeMPublic.Common.Enums;
using Microsoft.VisualBasic.Logging;
using System.Diagnostics;
using System.Drawing;
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
        private readonly AddDeviceViewModel _vm;

        private int selectedTab = -1;
        private readonly string Display = LangHelper.Instance["AddDevice.Display"];
        private readonly string Webcam = LangHelper.Instance["AddDevice.Webcam"];
        private readonly string KnM = LangHelper.Instance["AddDevice.KnM"];
        private readonly string Pen = LangHelper.Instance["AddDevice.Pen"];
        private readonly string Headset = LangHelper.Instance["AddDevice.Headset"];
        private readonly string Speaker = LangHelper.Instance["AddDevice.Speaker"];
        private readonly string Dock = LangHelper.Instance["AddDevice.Dock"];
        private readonly string Bluetooth = LangHelper.Instance["AddDevice.Type.Bluetooth"];
        private readonly string WirelessReceiver = LangHelper.Instance["AddDevice.Type.Wireless"];
        private readonly string Wired = LangHelper.Instance["AddDevice.Type.Wired"];
        private readonly string Other = LangHelper.Instance["AddDevice.Type.Other"];
        readonly string CancelButtonCaption = LangHelper.Instance["AddDevice.Msg.CancelBtn"];
        readonly string WaitingCaption = LangHelper.Instance["AddDevice.Msg.WaitingCap"];
        readonly string WaitingMessage = LangHelper.Instance["AddDevice.Msg.WaitingMsg"];
        readonly string WaitingAlert = LangHelper.Instance["AddDevice.Msg.WaitingAlert"];

        private WaitingModalDialog? waitingModalDialog;
        private bool WaitingModalDialogIsOpen = false;
        private ModuleGroup moduleGroup;

        public AddDeviceView()
        {
            InitializeComponent();
            _console = AddDevicePlugin.PluginIoc?.GetService<IConsole>()!;
            var vm = (AddDeviceViewModel?)AddDevicePlugin.PluginIoc?.GetService<IAddDeviceViewModel>()!;
            if (vm == null)
            { return; }

            _vm = vm;
            DataContext = _vm;
            _vm.DeviceBarItemClickCommand = new RelayCommand<DeviceBarItem>(OnDeviceBarItemClicked!);

            BuildModuleGroups();
            if (_vm.RightViewHeaders != null)
            {
                rightViewHeaderCtrl.SetHeaders(_vm.RightViewHeaders.ToArray());
            }
            //DdpmCommonHelper.BitmapImageUpdated += ImageUpdate;
            //DdpmCommonHelper.DeviceManagerSA.DeviceChanged += AddDeviceView_DeviceChanged;
        }

        ~AddDeviceView()
        {
            moduleGroup.Dispose();
        }

        bool IsRequested = false;
        private void AddDeviceView_DeviceChanged(object? sender, SA.Common.DeviceChangedEventArgs e)
        {
            Task.Run(() =>
            {
                switch (e.type)
                {
                    case DeviceChangedType.Peripherals_PlugIn:
                        DdpmCommonHelper.WriteUILog($"AddDevice Device PlugIn event received.");
                        if (_vm.CurrentDongle != null && e.device_peripherals != null && e.device_peripherals.PhyscialDeviceID == _vm.CurrentDongle.ID)
                        {
                            //_vm.NewDevice = e.device_peripherals;
                            //if(e.device_peripherals.PhysicalDeviceType == DeviceType.PhysicalAudioDongle)
                            while (IsRequested && !_vm.IsPairingLoaded)
                            {
                                Task.Delay(1000).Wait();
                            }
                            if (WaitingModalDialogIsOpen)
                            {
                                WaitingModalDialogIsOpen = false;
                                Dispatcher.Invoke(new Action(() =>
                                {
                                    waitingModalDialog?.Close();
                                }));
                            }
                            Dispatcher.Invoke(new Action(() =>
                            {
                                DdpmCommonHelper.WriteUILog($"AddDevice Device paired: ID: {e.device_peripherals.ID} Name: {e.device_peripherals.Name}");
                                //_vm.GotoNewDevice();
                                _console.ShowHomePage();
                            }));
                        }
                        else
                            Dispatcher.Invoke(new Action(() =>
                            {
                                WaitingModalDialogIsOpen = false;
                                waitingModalDialog?.Close();
                                DdpmCommonHelper.WriteUILog($"AddDevice Device added.");
                                _console.ShowHomePage();
                            }));
                        break;

                    case DeviceChangedType.Peripherals_UnPlug:
                        break;

                    case DeviceChangedType.Peripherals_SettingsChange:
                        var di = e.device_peripherals;
                        switch (e.changedProperty)
                        {
                            case "DonglePairedDeviceCountChanged":
                                //if (di.PhysicalDeviceType == DeviceType.PhysicalAudioDongle)
                                //{
                                //    //GotoNewDevice();
                                //    PairingStatus = "Request";
                                //    IsPairing = true;
                                //    OnPropertyChanged(nameof(PairingStatus));
                                //    return;
                                //}
                                break;

                            case "DonglePairingStatusChanged":
                                switch (di.PairingStatusName)
                                {
                                    case "Request":
                                        IsRequested = true;
                                        Dispatcher.Invoke(new Action(() =>
                                        {
                                            waitingModalDialog = new(WaitingCaption, $"{WaitingMessage} {di.Message}", WaitingAlert, _vm);
                                            Window mainWindow = System.Windows.Application.Current.MainWindow;
                                            waitingModalDialog.WindowStartupLocation = WindowStartupLocation.Manual;
                                            if (mainWindow != null)
                                            {
                                                waitingModalDialog.Owner = mainWindow;
                                                waitingModalDialog.Left = mainWindow.Left + (mainWindow.ActualWidth - 587) / 2;
                                                waitingModalDialog.Top = mainWindow.Top + (mainWindow.ActualHeight - 349) / 2;
                                            }
                                            WaitingModalDialogIsOpen = true;
                                            waitingModalDialog.ShowDialog();
                                            //_vm.GotoNewDevice();
                                        }));
                                        break;
                                    case "Already Paired":
                                        Dispatcher.Invoke(new Action(() =>
                                        {
                                            ShowMessage(Strings.Error, Strings.AlreadyPaired, "");
                                            _vm.StopPairing();
                                            StartPairing();
                                        }));
                                        break;
                                    case "Old Device":
                                        Dispatcher.Invoke(new Action(() =>
                                        {
                                            ShowMessage(Strings.Error, Strings.NotSupportedDevice, CancelButtonCaption);
                                            _vm.StopPairing();
                                            StartPairing();
                                        }));
                                        break;
                                    case "Stopped":
                                        if (WaitingModalDialogIsOpen)
                                        {
                                            WaitingModalDialogIsOpen = false;
                                            Dispatcher.Invoke(new Action(() =>
                                            {
                                                waitingModalDialog?.Close();
                                                DdpmCommonHelper.WriteUILog($"AddDevice pairing closed by Stopped status");
                                            }));
                                        }
                                        IsRequested = false;
                                        break;
                                    case "TimeOut":
                                        if (WaitingModalDialogIsOpen)
                                        {
                                            WaitingModalDialogIsOpen = false;
                                            Dispatcher.Invoke(new Action(() =>
                                            {
                                                waitingModalDialog?.Close();
                                                DdpmCommonHelper.WriteUILog($"AddDevice pairing closed by TimeOut status");
                                            }));
                                        }
                                        Dispatcher.Invoke(new Action(() =>
                                        {
                                            ShowMessage(Strings.Error, Strings.NoDeviceFound, "");
                                            _vm.StopPairing();
                                            rightViewHeaderCtrl.SelectedIndex = -1;
                                            rightViewHeaderCtrl.SelectedIndex = 1;
                                        }));
                                        IsRequested = false;
                                        break;
                                    default:
                                        break;
                                }
                                break;

                            default:

                                break;
                        }
                        break;

                    default:
                        break;
                }
            });

        }

        //private void ImageUpdate(OSThemeEnum oSThemeEnum)
        //{
        //    ArrowLeft.Source = null;
        //    ArrowLeft.Source = (BitmapImage)Application.Current.Resources["Arrow_Left"];
        //}

        private void BuildModuleGroups()
        {
            List<ModuleGroup> groups = new();

            moduleGroup = new ModuleGroup()
            {
                GroupName = Display,
                GroupIcon = DdpmCommonHelper.GetImageSourceFromCommonResource("Resources/Images/Monitor.png", "DDPM.UI.Resources")
            };
            moduleGroup.AddHeader(Display, new AddDisplayModule(_vm));
            groups.Add(moduleGroup);

            moduleGroup = new ModuleGroup()
            {
                GroupName = Webcam,
                GroupIcon = DdpmCommonHelper.GetImageSourceFromCommonResource("Resources/Images/Webcamera.png", "DDPM.UI.Resources")
            };
            moduleGroup.AddHeader(Webcam, new AddWebcamModule(_vm));
            groups.Add(moduleGroup);

            moduleGroup = new ModuleGroup()
            {
                GroupName = KnM,
                GroupIcon = DdpmCommonHelper.GetImageSourceFromCommonResource("Resources/Images/KnM.png", "DDPM.UI.Resources")
            };
            moduleGroup.AddHeader(Bluetooth, new AddKnM_BLModule(_vm));
            moduleGroup.Headers[0].ImageFile = "Bluetooth.png";
            moduleGroup.AddHeader(WirelessReceiver, new AddKnM_DongleModule(_vm));
            moduleGroup.Headers[1].ImageFile = "Dongle.png";
            moduleGroup.AddHeader(Wired, new AddKnM_WiredModule(_vm));
            moduleGroup.Headers[2].ImageFile = "Port.png";
            groups.Add(moduleGroup);

            moduleGroup = new ModuleGroup()
            {
                GroupName = Pen,
                GroupIcon = DdpmCommonHelper.GetImageSourceFromCommonResource("Resources/Images/Pen.png", "DDPM.UI.Resources")
            };
            moduleGroup.AddHeader(Bluetooth, new AddPen_BLModule(_vm));
            moduleGroup.Headers[0].ImageFile = "Bluetooth.png";
            moduleGroup.AddHeader(Other, new AddPen_OtherModule(_vm));
            moduleGroup.Headers[1].ImageFile = "";
            groups.Add(moduleGroup);

            moduleGroup = new ModuleGroup()
            {
                GroupName = Headset,
                GroupIcon = DdpmCommonHelper.GetImageSourceFromCommonResource("Resources/Images/Headset.png", "DDPM.UI.Resources")
            };
            moduleGroup.AddHeader(Bluetooth, new AddHeadset_BLModule(_vm));
            moduleGroup.Headers[0].ImageFile = "Bluetooth.png";
            moduleGroup.AddHeader(WirelessReceiver, new AddHeadset_DongleModule(_vm));
            moduleGroup.Headers[1].ImageFile = "Dongle.png";
            moduleGroup.AddHeader(Wired, new AddHeadset_WiredModule(_vm));
            moduleGroup.Headers[2].ImageFile = "Port.png";
            groups.Add(moduleGroup);

            moduleGroup = new ModuleGroup()
            {
                GroupName = Speaker,
                GroupIcon = DdpmCommonHelper.GetImageSourceFromCommonResource("Resources/Images/SpeakerOn.png", "DDPM.UI.Resources")
            };
            moduleGroup.AddHeader(Speaker, new AddSpeakerModule(_vm));
            groups.Add(moduleGroup);

            if (!GlobalDefinitions.isSupport200)
            {
                moduleGroup = new ModuleGroup()
                {
                    GroupName = Dock,
                    GroupIcon = DdpmCommonHelper.GetImageSourceFromCommonResource("Resources/Images/Dock.png", "DDPM.UI.Resources")
                };
                moduleGroup.AddHeader(Dock, new AddDockModule(_vm));
                groups.Add(moduleGroup);
            }
            _vm.ModuleGroups = groups;
        }

        private void OnDeviceBarItemClicked(DeviceBarItem newItem)
        {
            //if(newItem.Id == _vm.DeviceBarSelectedIndex) { return; }
            if (_vm == null)
                return;

            if (_vm.DeviceBarSelectedIndex >= 0 && _vm.DeviceBarItems.Find(x => x.Id == _vm.DeviceBarSelectedIndex) != null)
            {
                _vm.DeviceBarItems[_vm.DeviceBarSelectedIndex].IsSelected = false;
                _vm.DeviceBarItems[_vm.DeviceBarSelectedIndex].RenewBarItem();
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
            //Robert_Lin 2025-4-16, move to new added method GoBackToPreviousPage()
            GoBackToPreviousPage();

            //Robert_Lin 2025-3-5 for the back arrow to go back to the "previous" page.
            //PIMS-301474 Clicking back arrow didn't bring the UI back to the DDPM page previously, before the
            // settings gear icon is selected
            //PIMS-314264 The DDPM can not back to previous page after select the Back Arrow in top left.
            //
            //OLD:
            //_console.ShowHomePage();
            //NEW:
            //IShowPluginManager? showPluginManager = AddDevicePlugin.PluginIoc.GetService<IShowPluginManager>();
            //if (showPluginManager == null)
            //    return;
            //if (showPluginManager.HideTakeoverPlugin())
            //{
            //    //Request to show "AddDevice" icon on Masthead
            //    if (_console != null)
            //    {
            //        var args = new EventManagerArgs();
            //        bool isShow = true;
            //        bool isEnabled = true;
            //        args.Tag = new List<bool> { isShow, IsEnabled }; //true=Show, false=Hide
            //        _console.RaiseEvent(ConsoleEventNames.Masthead_ShowAddDeviceIcon, this, args);
            //    }
            //}
        }

        private void RightViewHeaderCtrl_SelectionChanged(object sender, RoutedEventArgs e)
        {
            //if(sender == null)
            //  return;
            if (_vm == null)
                return;

            int newSelId = rightViewHeaderCtrl.SelectedIndex;
            if (newSelId != selectedTab)
            {
                _vm.RightViewHeaderSelectedIndex = newSelId;
                selectedTab = newSelId;
            }

            if (newSelId == 1 && (_vm.DeviceBarSelectedIndex == 2 || _vm.DeviceBarSelectedIndex == 3 || _vm.DeviceBarSelectedIndex == 4) && _vm.DongleAlertKnMVisibility == Visibility.Collapsed)
            {
                StartPairing();
            }
            else
            {
                _vm.StopPairing();
            }
        }

        private void StartPairing()
        {
            if (_vm == null)
                return;

            if (_vm.DeviceBarSelectedIndex == 2 && _vm.DongleInfos.Count == 1)
            {
                if (_vm.DongleInfos.Values.First().PairedDeviceCount == _vm.DongleInfos.Values.First().MaxPairingSlots)
                {
                    ShowMessage(Strings.Error, Strings.DongleSlotFull, "");
                    DdpmCommonHelper.WriteUILog($"Dongle Slot Full PairedDeviceCount = {_vm.DongleInfos.Values.First().PairedDeviceCount.ToString()}");
                    return;
                }
                else
                {
                    _vm.CurrentDongle = _vm.DongleInfos.Values.First();
                    _vm.StartPairing(_vm.DongleInfos.Keys.First());
                    DdpmCommonHelper.WriteUILog($"Dongle StartPairing ...");
                }
            }
            if (_vm.DeviceBarSelectedIndex == 4 && _vm.AudioDongleInfos.Count == 1)
            {
                if (_vm.AudioDongleInfos.Values.First().PairedDeviceCount == _vm.AudioDongleInfos.Values.First().MaxPairingSlots)
                {
                    ShowMessage(Strings.Error, Strings.DongleSlotFull, "");
                    DdpmCommonHelper.WriteUILog($"Audio Dongle Slot Full PairedDeviceCount = {_vm.AudioDongleInfos.Values.First().PairedDeviceCount.ToString()}");
                    return;
                }
                else
                {
                    _vm.CurrentDongle = _vm.AudioDongleInfos.Values.First();
                    _vm.StartPairing(_vm.AudioDongleInfos.Keys.First());
                    DdpmCommonHelper.WriteUILog($"Audio Dongle StartPairing ...");
                }
            }
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

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (_vm == null)
                return;

            OnDeviceBarItemClicked(_vm.DeviceBarItems[_vm.DeviceBarSelectedIndex]);
            _vm.DeviceBarItems[_vm.DeviceBarSelectedIndex].IsSelected = true;

            //Robert_Ln 2025-2-19 for Narrator, setup the focus to the left Arrow at start up
            //ArrowLeft.Focus(); Robert_Lin 2025-4-16 change to backArrow button.
            backArrow.Focus();

            //DdpmCommonHelper.BitmapImageUpdated += ImageUpdate;
            if (DdpmCommonHelper.DeviceManagerSA != null)
                DdpmCommonHelper.DeviceManagerSA.DeviceChanged += AddDeviceView_DeviceChanged;
        }

        private void Pairing(object sender, StylusDownEventArgs e)
        {
            if (_vm == null || _vm.DeviceBarSelectedIndex != 3 || rightViewHeaderCtrl.SelectedIndex != 1)
                return;

            MessageModalDialog messageModalDialog;
            Window mainWindow = System.Windows.Application.Current.MainWindow;
            messageModalDialog = new(Strings.PairYourPen, Strings.PairYourPenMessage, Strings.No, Strings.Yes);
            if (mainWindow != null)
            {
                messageModalDialog.Owner = mainWindow;
                messageModalDialog.Left = mainWindow.Left + (mainWindow!.ActualWidth - 417) / 2;
                messageModalDialog.Top = mainWindow.Top + 300;
            }
            if (messageModalDialog.ShowDialog()!.Value)
            {
                _vm.StartPairingPen();
            }
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            //DdpmCommonHelper.BitmapImageUpdated -= ImageUpdate;
            if (DdpmCommonHelper.DeviceManagerSA != null)
                DdpmCommonHelper.DeviceManagerSA.DeviceChanged -= AddDeviceView_DeviceChanged;
            _vm.StopPairing();
            _vm.RightViewHeaderSelectedIndex = -1;
            WaitingModalDialogIsOpen = false;
            waitingModalDialog?.Close();
            moduleGroup?.Dispose();
        }

        //Robert_Lin, 2025-4-16 unused method.
        private void ArrowLeft_PreviewKeyDown(object sender, RoutedEventArgs e)
        {
            //if (e.Key == Key.Enter)
            //{
            //    e.Handled = true;
            //    _console.ShowHomePage();
            //}
        }

        private bool _hasInitialized = false;
        private void UserControl_LayoutUpdated(object sender, EventArgs e)
        {
            if (!_hasInitialized && IsVisible)
            {
                _hasInitialized = true;
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    GetRFDongleAsync();
                }), System.Windows.Threading.DispatcherPriority.ApplicationIdle);
            }
        }
        private void GetRFDongleAsync()
        {
            if (DdpmCommonHelper.DeviceManagerSA == null)
            {
                DdpmCommonHelper.WriteUILog($"Error: DeviceManagerSA is null");
            }
            else
            {
                var _deviceHelper = DdpmCommonHelper.DeviceManagerSA.GetRFDongleDevices().Result;
                _vm?.PrepareDongleInfo(_deviceHelper.dongleInfo);
            }
        }

        private void backArrowButton_Click(object sender, RoutedEventArgs e)
        {
            GoBackToPreviousPage();
        }
        private void GoBackToPreviousPage()
        {
            //Robert_Lin 2025-3-5 for the back arrow to go back to the "previous" page.
            //PIMS-301474 Clicking back arrow didn't bring the UI back to the DDPM page previously, before the
            // settings gear icon is selected
            //PIMS-314264 The DDPM can not back to previous page after select the Back Arrow in top left.
            //
            //OLD:
            //_console.ShowHomePage();
            //NEW:
            IShowPluginManager? showPluginManager = AddDevicePlugin.PluginIoc.GetService<IShowPluginManager>();
            if (showPluginManager == null)
                return;
            if (showPluginManager.HideTakeoverPlugin())
            {
                //Request to show "AddDevice" icon on Masthead
                if (_console != null)
                {
                    var args = new EventManagerArgs();
                    bool isShow = true;
                    bool isEnabled = true;
                    args.Tag = new List<bool> { isShow, IsEnabled }; //true=Show, false=Hide
                    _console.RaiseEvent(ConsoleEventNames.Masthead_ShowAddDeviceIcon, this, args);
                }
            }
        }
    }
}