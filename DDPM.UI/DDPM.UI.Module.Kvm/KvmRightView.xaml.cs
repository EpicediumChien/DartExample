using DDPM.SA.Common.Settings;
using DDPM.UI.Common;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace DDPM.UI.Module.Kvm
{
    /// <summary>
    /// Interaction logic for KvmRightView.xaml
    /// </summary>
    public partial class KvmRightView : UserControl
    {
        //Robert_Lin, 2024-12-26, ViewModel is required for KvmRightView, keep is as a data member.
        private readonly KvmViewModel _vm;
        private KvmViewModel vm
        {
            //Robert_Lin, 2024-12-26
            //NEW:
            get => _vm;
            //OLD:
            //get => (KvmViewModel)DataContext != null ? (KvmViewModel)DataContext : null;
        }

        public KvmRightView(KvmViewModel vm)
        {
            InitializeComponent();
            //Robert_Lin, 2024-12-26
            _vm = vm;
            //DataContext = new KvmViewModel();
            DataContext = vm;
            //vm.Invoke_RefreshData();
            vm.Invoke_RefreshHotkeySettings();

            //lock/unlock
            if (DdpmCommonHelper.DeviceManagerSA != null)
            {
                DdpmCommonHelper.DeviceManagerSA.ITSettingsActionEvent += DeviceManagerSA_ITSettingsActionEvent;

                DDPMSettings data = DdpmCommonHelper.ReadDDPMSettings();//DeviceManagerSA.ReloadAppConfigData().Result;
                if (data != null)
                {
                    if (data.LockSettings.Lock_Display_NetworkKVM)
                    {
                        //Do lock ui init here (direct set or binding via vm)
                        IsLockinNKVMUI(true);
                    }
                    else
                    {
                        IsLockinNKVMUI(false);
                    }
                    if (data.LockSettings.Lock_Display_USBKVM) //CLI not ready
                    {
                        //Do lock ui init here (direct set or binding via vm)
                        IsLockinUSBKVMUI(true);
                    }
                    else
                    {
                        IsLockinUSBKVMUI(false);
                    }
                }
            }
        }

        ~KvmRightView()
        {
            if (DdpmCommonHelper.DeviceManagerSA != null)
            {
                DdpmCommonHelper.DeviceManagerSA.ITSettingsActionEvent -= DeviceManagerSA_ITSettingsActionEvent;
            }
        }

        private void IsLockinNKVMUI(bool isLocked)
        {
            if (vm != null)
            {
                //vm.LockNKVM_Visibility = isLocked ? Visibility.Visible : Visibility.Collapsed;
                vm.isNKVMEanble = isLocked ? false : true;
                vm.NKVM_Opacity = isLocked ? 0.5 : 1;
            }
        }

        private void IsLockinUSBKVMUI(bool isLocked)
        {
            if (vm != null)
            {
                vm.LockUSBKVM_Visibility = isLocked ? Visibility.Visible : Visibility.Collapsed;
                vm.isUSBKVMEanble = isLocked ? false : true;
                vm.USBKVM_Opacity = isLocked ? 0.5 : 1;
            }
        }

        private void DeviceManagerSA_ITSettingsActionEvent(object? sender, SA.Common.ITSettingEventArgs e)
        {
            //Using user data from config file if need
            //DDPMSettings data = null;
            //if (DdpmCommonHelper.DeviceManagerSA != null)
            //    data = DdpmCommonHelper.DeviceManagerSA.ReloadAppConfigData().Result;

            bool? isLocked = DdpmCommonHelper.GetUINotifyPropertyValue_Boolean("Lock_Display_NetworkKVM", e);
            if (isLocked != null)
            {
                Dispatcher.Invoke(new Action(() =>
                {
                    KvmViewModel localVm = (KvmViewModel)this.DataContext;
                    if (localVm != null)
                    {
                        IsLockinNKVMUI((bool)isLocked);
                        Trace.WriteLine($"[SettingsPage] Apply NetworkKVM(Lock) : {isLocked}");
                        localVm.OnPropertyChanged_Lock();
                    }
                }));
            }
            //CLI not ready
            isLocked = DdpmCommonHelper.GetUINotifyPropertyValue_Boolean("Lock_Display_USBKVM", e);
            if (isLocked != null)
            {
                Dispatcher.Invoke(new Action(() =>
                {
                    KvmViewModel localVm = (KvmViewModel)this.DataContext;
                    if (localVm != null)
                    {
                        //vm.LockMaskVisible = (bool)isLocked ? Visibility.Visible : Visibility.Collapsed;
                        IsLockinUSBKVMUI((bool)isLocked);
                        Trace.WriteLine($"[SettingsPage] Apply USBKVM(Lock) : {isLocked}");
                        localVm.OnPropertyChanged_Lock();
                    }
                }));
            }
            //NKVM CLI ON
            bool? isNKVMOn = DdpmCommonHelper.GetUINotifyPropertyValue_Boolean("Enable_Display_NetworkKVM", e);
            if (isNKVMOn != null && vm != null)
            {
                if ((bool)isNKVMOn)
                {
                    vm.isNKVM = true;
                }
                else
                {
                    if (!vm.LockSendNoKVM)
                    {
                        vm.SelectKVM();
                    }
                }
            }
        }

        private void OpenUSBKVM(object sender, RoutedEventArgs e)
        {
            if (vm != null)
            {
                if (vm.USBKVMisON)
                {
                    vm.SetInput = Visibility.Visible;
                    vm.SetPXP = Visibility.Visible;
                    vm.EditInput = Visibility.Collapsed;
                    vm.EditPXP = Visibility.Collapsed;
                    vm.UpdateArrow(false);
                    //Test Edit Input page
                    //vm.SetInput = Visibility.Collapsed;
                    //vm.SetPXP = Visibility.Collapsed;
                    //vm.EditInput = Visibility.Visible;
                    //vm.EditPXP = Visibility.Visible;
                    vm.ArrowinFullscreen = Visibility.Collapsed;
                    InputSourceFullView _inputSourceFullView = new InputSourceFullView();
                    _inputSourceFullView.DataContext = vm;
                    DdpmCommonHelper.ModuleOwner?.OpenFullView(_inputSourceFullView);
                }
                else
                {
                    vm.UpdateArrow(false);
                    vm.Invoke_USBKVM(true);
                }
            }
        }

        private void SelectUSBKVM(object sender, RoutedEventArgs e)
        {
            Button button_USB = (Button)FindName("USBKVM");
            Button button_OnUSB = (Button)FindName("ONUSBKVM");
            Button button_USBHotkeys = (Button)FindName("USBKVMHotkeys");
            Button button_Net = (Button)FindName("NetworkKVM");
            if (button_USB != null && button_Net != null)
            {
                button_Net.Visibility = Visibility.Collapsed;
                if (vm != null)
                {
                    if (vm.NKVMisON)
                    {
                        vm.isOnNKVM(false);
                        vm.NKVMisON = false;
                    }
                    vm.isOnNoKVM(false);
                    if (vm.USBKVMisON)
                    {
                        vm._log.Info("[SelectUSBKVM]USBKVM is on");
                        button_USB.Visibility = Visibility.Collapsed;
                        button_OnUSB.Visibility = Visibility.Visible;
                        button_USBHotkeys.Visibility = Visibility.Visible;
                        //if (DdpmCommonHelper.ModuleOwner.SelectedHomeDevice.MonitorInfo.CapabilityDic.ContainsKey("E8"))
                        //{
                        //    vm._log.Info("[SelectUSBKVM]Have E8");
                        //    button_USBHotkeys.Visibility = Visibility.Visible;
                        //}
                        //else
                        //{
                        //    vm._log.Info("[SelectUSBKVM]No E8");
                        //    button_USBHotkeys.Visibility = Visibility.Collapsed;
                        //}
                        vm.NoKVMisON = false;
                        vm.LoadnewLeftView(false);
                        if (!vm.firstinKVM)
                        {
                            vm.Invoke_USBKVM(false);
                        }
                    }
                    else
                    {
                        vm._log.Info("[SelectUSBKVM]USBKVM is off");
                        button_USB.Visibility = Visibility.Visible;
                        button_OnUSB.Visibility = Visibility.Collapsed;
                        button_USBHotkeys.Visibility = Visibility.Collapsed;
                    }
                    //vm.isNKVM = false;
                    //vm.isNoKVM = false;
                    vm.LockSendNoKVM = false;
                }
                else
                {
                    button_USB.Visibility = Visibility.Collapsed;
                    button_OnUSB.Visibility = Visibility.Collapsed;
                    button_USBHotkeys.Visibility = Visibility.Collapsed;
                }
            }
        }

        private void SelectNKVM(object sender, RoutedEventArgs e)
        {
            Button button_USB = (Button)FindName("USBKVM");
            Button button_OnUSB = (Button)FindName("ONUSBKVM");
            Button button_USBHotkeys = (Button)FindName("USBKVMHotkeys");
            Button button_Net = (Button)FindName("NetworkKVM");
            if (button_USB != null && button_Net != null)
            {
                button_USB.Visibility = Visibility.Collapsed;
                button_OnUSB.Visibility = Visibility.Collapsed;
                button_USBHotkeys.Visibility = Visibility.Collapsed;
                button_Net.Visibility = Visibility.Visible;
                //if (vm != null)
                //{
                //    vm.isNKVM = true;
                //}
                //vm.isOnNoKVM(false);
                //vm.NoKVMisON = false;
                vm.LoadnewLeftView(true);
                vm.LockSendNoKVM = false;
            }
        }

        private void SelectNoKVM(object sender, RoutedEventArgs e)
        {
            //RadioButton radioButton = sender as RadioButton;

            //radioButton.IsEnabled = false;

            Button button_USB = (Button)FindName("USBKVM");
            Button button_OnUSB = (Button)FindName("ONUSBKVM");
            Button button_USBHotkeys = (Button)FindName("USBKVMHotkeys");
            Button button_Net = (Button)FindName("NetworkKVM");
            if (button_USB != null && button_Net != null)
            {
                button_USB.Visibility = Visibility.Collapsed;
                button_OnUSB.Visibility = Visibility.Collapsed;
                button_USBHotkeys.Visibility = Visibility.Collapsed;
                button_Net.Visibility = Visibility.Collapsed;
                //if (vm != null)
                //{
                //    vm.isNoKVM = true;
                //}
                vm._log?.Info("[SelectNoKVM]LockSendNoKVM is " + vm.LockSendNoKVM.ToString());
                if (!vm.LockSendNoKVM)
                {
                    vm.isOnNoKVM(true);
                    if (vm.NKVMisON)
                    {
                        vm.isOnNKVM(false);
                        vm.NKVMisON = false;
                    }
                    vm.NoKVMisON = true;
                }
                vm.LoadnewLeftView(true);
                //DdpmCommonHelper.DeviceManagerSA.SentKVMtoTelementry(DdpmCommonHelper.ModuleOwner.SelectedHomeDevice.MonitorInfo, "KVMMode", "NoKVM");
            }

            //radioButton.IsEnabled = true;

            //Robert_Lin debug purpose, can be removed at release build
            //if (DdpmCommonHelper.ModuleOwner != null)
            //{
            //    DdpmCommonHelper.ModuleOwner.ShowSpecificModule(Constants.GroupName_InputSource, Constants.ModuleName_DisplayHotkeys);
            //}
        }

        private void OpenNKVM(object sender, RoutedEventArgs e)
        {
            if (vm != null)
            {
                vm.NKVMOpenUI();
                //vm.OpenNKVMUI(0, 100, 100);
                //vm.isOnNKVM(true);
            }
        }

        private void USBKVMHotkeys_Click(object sender, RoutedEventArgs e)
        {
            if (vm != null)
            {
                KVMHotkeyFullView kVMHotkeyFullView = new KVMHotkeyFullView();
                kVMHotkeyFullView.DataContext = vm;
                DdpmCommonHelper.ModuleOwner?.OpenFullView(kVMHotkeyFullView);
            }
        }

        //20250407 Elsa add for tooltip issue fix
        private double GetScreenScaleX()
        {
            var source = PresentationSource.FromVisual(this);
            if (source?.CompositionTarget != null)
            {
                return source.CompositionTarget.TransformToDevice.M11;
            }
            return 1;
        }

        //20250407 Elsa add for tooltip issue fix
        private void toolTip_Opened(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.ToolTip? target = sender as System.Windows.Controls.ToolTip;
            if (target == null)
                return;
            double screenScaleX = GetScreenScaleX();
            Window mainWindow = System.Windows.Application.Current.MainWindow;
            double winRightX = (mainWindow.Left + mainWindow!.ActualWidth) * screenScaleX;
            double mousepositionX = System.Windows.Forms.Cursor.Position.X;
            double mouseaddtooltip = mousepositionX + target.ActualWidth * screenScaleX;
            if (winRightX > mouseaddtooltip)
            {
                target.HorizontalOffset = 2;
            }
            else
            {
                target.HorizontalOffset = -1 * target.ActualWidth + 14;
            }
        }
    }
}