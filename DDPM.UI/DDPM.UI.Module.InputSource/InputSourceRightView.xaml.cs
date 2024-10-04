using DDPM.UI.Common;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
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

        private void KeyDown_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (((e.KeyStates == Keyboard.GetKeyStates(Key.D1)) || (e.KeyStates == Keyboard.GetKeyStates(Key.D3))) && (Keyboard.Modifiers == ModifierKeys.Shift))
            {
                e.Handled = true;
            }
            else if ((e.KeyStates == Keyboard.GetKeyStates(Key.D2)) && (Keyboard.Modifiers == ModifierKeys.Shift))
            {
                // Handle "@"
            }
            else if ((Keyboard.Modifiers == ModifierKeys.Shift))
            {
                e.Handled = true;
            }
            else if (Keyboard.IsKeyDown(Key.D0) || Keyboard.IsKeyDown(Key.D1) || Keyboard.IsKeyDown(Key.D2) || Keyboard.IsKeyDown(Key.D3) || Keyboard.IsKeyDown(Key.D4) ||
                Keyboard.IsKeyDown(Key.D5) || Keyboard.IsKeyDown(Key.D6) || Keyboard.IsKeyDown(Key.D7) || Keyboard.IsKeyDown(Key.D8) || Keyboard.IsKeyDown(Key.D9) ||
                Keyboard.IsKeyDown(Key.A) || Keyboard.IsKeyDown(Key.B) || Keyboard.IsKeyDown(Key.C) || Keyboard.IsKeyDown(Key.D) || Keyboard.IsKeyDown(Key.E) ||
                Keyboard.IsKeyDown(Key.F) || Keyboard.IsKeyDown(Key.G) || Keyboard.IsKeyDown(Key.H) || Keyboard.IsKeyDown(Key.I) || Keyboard.IsKeyDown(Key.J) ||
                Keyboard.IsKeyDown(Key.K) || Keyboard.IsKeyDown(Key.L) || Keyboard.IsKeyDown(Key.M) || Keyboard.IsKeyDown(Key.N) || Keyboard.IsKeyDown(Key.O) ||
                Keyboard.IsKeyDown(Key.P) || Keyboard.IsKeyDown(Key.Q) || Keyboard.IsKeyDown(Key.R) || Keyboard.IsKeyDown(Key.S) || Keyboard.IsKeyDown(Key.T) ||
                Keyboard.IsKeyDown(Key.U) || Keyboard.IsKeyDown(Key.V) || Keyboard.IsKeyDown(Key.W) || Keyboard.IsKeyDown(Key.X) || Keyboard.IsKeyDown(Key.Y) ||
                Keyboard.IsKeyDown(Key.Z) || Keyboard.IsKeyDown(Key.OemMinus) || Keyboard.IsKeyDown(Key.Space))
            {
                // Handle 0-9, a-z, A-Z, " ", "-" 
            }
            else
            {
                e.Handled = true;
            }
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