using DDPM.UI.Common;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace DDPM.UI.Module.InputSource
{
    /// <summary>
    /// Interaction logic for InputSourceRightView.xaml
    /// </summary>
    public partial class InputSourceRightView : UserControl
    {
        public InputSourceRightView(InputSourceViewModel vm)
        {
            InitializeComponent();
            DataContext = vm;
        }

        private void UXComboBox_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {
            ComboBox cb = sender as ComboBox;
            string setString = cb.SelectedValue.ToString();
            InputSourceViewModel vm = (InputSourceViewModel)DataContext;
            bool bre = DdpmCommonHelper.DeviceManagerSA.SetUSBUpstream(vm.InputSourceModule.SelectedHomeDevice.MonitorInfo, vm.items[(int)cb.Tag].InputType, setString).Result;
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox tb = sender as TextBox;
            InputSourceViewModel vm = (InputSourceViewModel)DataContext;
            //vm.items[(int)tb.Tag].InputName = tb.Text;
            vm.inputList[vm.items[(int)tb.Tag].InputType].InputName = tb.Text;
            //DdpmCommonHelper.DeviceManagerSA.SetInputName(vm.items[(int)tb.Tag].InputType, vm.items[(int)tb.Tag].InputName);
            bool b = DdpmCommonHelper.DeviceManagerSA.SetInputSourcelist(vm.InputSourceModule.SelectedHomeDevice.MonitorInfo, vm.inputList).Result;
        }
    }

    public class Item
    {
        public int Index { get; set; }
        public string InputImage { get; set; }
        public string InputType { get; set; }
        public string InputName { get; set; }
        public ObservableCollection<string> USBUpstream { get; set; }
        public int UpstreamIndex { get; set; }
        public Visibility IsUSBCB { get; set; }
        public string NameWidth { get; set; }
        public string USBWidth { get; set; }
        public string NameColumn { get; set; }
    }
}