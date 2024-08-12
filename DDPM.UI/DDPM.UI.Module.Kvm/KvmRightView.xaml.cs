using DDPM.UI.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DDPM.UI.Module.Kvm
{
    /// <summary>
    /// Interaction logic for KvmRightView.xaml
    /// </summary>
    public partial class KvmRightView : UserControl
    {
        private KvmViewModel vm
        {
            get 
            {
                return (KvmViewModel)DataContext;
            }
        }
        public KvmRightView(KvmViewModel vm)
        {
            InitializeComponent();
            //DataContext = new KvmViewModel();
            DataContext = vm;
            vm.Invoke_RefreshData();
            vm.Invoke_RefreshHotkeySettings();
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
            vm.OpenNKVMUI(0, 100, 100);
        }

        private void USBKVMHotkeys_Click(object sender, RoutedEventArgs e)
        {
            KVMHotkeyFullView kVMHotkeyFullView = new KVMHotkeyFullView();
            kVMHotkeyFullView.DataContext = vm;
            DdpmCommonHelper.ModuleOwner?.OpenFullView(kVMHotkeyFullView);
        }
    }
}
