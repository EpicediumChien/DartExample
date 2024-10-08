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
        private KvmViewModel vm
        {
            get => (KvmViewModel)DataContext != null ? (KvmViewModel)DataContext : null;
        }

        public KvmRightView(KvmViewModel vm)
        {
            InitializeComponent();
            //DataContext = new KvmViewModel();
            DataContext = vm;
            vm.Invoke_RefreshData();
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
                vm.LockNKVM_Visibility = isLocked ? Visibility.Visible : Visibility.Collapsed;
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
                    KvmViewModel vm = (KvmViewModel)this.DataContext;
                    if (vm != null)
                    {
                        IsLockinNKVMUI((bool)isLocked);
                        Trace.WriteLine($"[SettingsPage] Apply NetworkKVM(Lock) : {isLocked}");
                        vm.OnPropertyChanged_Lock();
                    }
                }));
            }
            //CLI not ready
            isLocked = DdpmCommonHelper.GetUINotifyPropertyValue_Boolean("Lock_Display_USBKVM", e);
            if (isLocked != null)
            {
                Dispatcher.Invoke(new Action(() =>
                {
                    KvmViewModel vm = (KvmViewModel)this.DataContext;
                    if (vm != null)
                    {
                        //vm.LockMaskVisible = (bool)isLocked ? Visibility.Visible : Visibility.Collapsed;
                        IsLockinUSBKVMUI((bool)isLocked);
                        Trace.WriteLine($"[SettingsPage] Apply USBKVM(Lock) : {isLocked}");
                        vm.OnPropertyChanged_Lock();
                    }
                }));
            }
        }

        private void OpenUSBKVM(object sender, RoutedEventArgs e)
        {
            vm.SetInput = Visibility.Visible;
            vm.SetPXP = Visibility.Visible;
            vm.EditInput = Visibility.Collapsed;
            vm.EditPXP = Visibility.Collapsed;
            //Test Edit Input page
            //vm.SetInput = Visibility.Collapsed;
            //vm.SetPXP = Visibility.Collapsed;
            //vm.EditInput = Visibility.Visible;
            //vm.EditPXP = Visibility.Visible;
            InputSourceFullView _inputSourceFullView = new InputSourceFullView();
            _inputSourceFullView.DataContext = vm;
            DdpmCommonHelper.ModuleOwner?.OpenFullView(_inputSourceFullView);
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
                if (vm.USBKVMisON)
                {
                    button_USB.Visibility = Visibility.Collapsed;
                    button_OnUSB.Visibility = Visibility.Visible;
                    button_USBHotkeys.Visibility = Visibility.Visible;
                }
                else
                {
                    button_USB.Visibility = Visibility.Visible;
                    button_OnUSB.Visibility = Visibility.Collapsed;
                    button_USBHotkeys.Visibility = Visibility.Collapsed;
                }
                vm.isNKVM = false;
                vm.isNoKVM = false;
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
                vm.isNKVM = true;
            }
        }

        private void SelectNoKVM(object sender, RoutedEventArgs e)
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
                button_Net.Visibility = Visibility.Collapsed;
                vm.isNoKVM = true;
            }
        }

        private void OpenNKVM(object sender, RoutedEventArgs e)
        {
            DdpmCommonHelper.DeviceManagerSA.NKVM_State(true).Wait();
            if (!DdpmCommonHelper.DeviceManagerSA.IsNamedpipeConnected().Result)
            {
                DdpmCommonHelper.DeviceManagerSA.CreatNewNamedpipe().Wait();
            }
            vm.OpenNKVMUI(0, 100, 100);
            vm.isOnNKVM(true);
        }

        private void USBKVMHotkeys_Click(object sender, RoutedEventArgs e)
        {
            KVMHotkeyFullView kVMHotkeyFullView = new KVMHotkeyFullView();
            kVMHotkeyFullView.DataContext = vm;
            DdpmCommonHelper.ModuleOwner?.OpenFullView(kVMHotkeyFullView);
        }
    }
}