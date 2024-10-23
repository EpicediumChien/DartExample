using DDPM.SA.Common.Display;
using DDPM.UI.Common;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace DDPM.UI.Module.Kvm
{
    /// <summary>
    /// Interaction logic for Test1FullView.xaml
    /// </summary>
    public partial class InputSourceFullView : UserControl
    {
        private KvmViewModel vm
        {
            get
            {
                return (KvmViewModel)DataContext;
            }
        }

        public InputSourceFullView()
        {
            InitializeComponent();
        }

        private void OpenMKFullView(object sender, RoutedEventArgs e)
        {
            ConnectMKFullView connectMKFullView = new ConnectMKFullView();
            connectMKFullView.DataContext = vm;
            DdpmCommonHelper.ModuleOwner?.OpenFullView(connectMKFullView);
            vm.FromProgressValue = vm.ToProgressValue;
            vm.ToProgressValue = vm.ToProgressValue + 1;
        }

        private void CloseUSBKVM(object sender, RoutedEventArgs e)
        {
            DdpmCommonHelper.ModuleOwner?.CloseFullView();
            vm.FromProgressValue = 0;
            vm.ToProgressValue = 1;
        }

        private void SaveInput(object sender, RoutedEventArgs e)
        {
            if (vm.pcsList != vm.original_pcsList)
            {
                InputSourceObj pc1input = new InputSourceObj(vm.pcsList["PC1"].InputType);
                InputSourceObj pc2input = new InputSourceObj(vm.pcsList["PC2"].InputType);
                if (vm.pcsList.Count >= 3)
                {
                    InputSourceObj pc3input = new InputSourceObj(vm.pcsList["PC3"].InputType);
                    if (vm.pcsList.Count == 4)
                    {
                        InputSourceObj pc4input = new InputSourceObj(vm.pcsList["PC4"].InputType);
                        bool res = DdpmCommonHelper.DeviceManagerSA.SetSubInputs(
                            DdpmCommonHelper.ModuleOwner.SelectedHomeDevice.MonitorInfo,
                            pc2input, pc3input, pc4input).Result;
                        if (res)
                        {
                            Thread.Sleep(500);
                        }
                    }
                    else
                    {
                        bool res = DdpmCommonHelper.DeviceManagerSA.SetSubInputs(
                            DdpmCommonHelper.ModuleOwner.SelectedHomeDevice.MonitorInfo,
                            pc2input, pc3input, null).Result;
                        if (res)
                        {
                            Thread.Sleep(500);
                        }
                    }
                }
                else
                {
                    bool res = DdpmCommonHelper.DeviceManagerSA.SetSubInputs(
                            DdpmCommonHelper.ModuleOwner.SelectedHomeDevice.MonitorInfo,
                            pc2input, null, null).Result;
                    if (res)
                    {
                        Thread.Sleep(500);
                    }
                }

                foreach (var pcs in vm.pcsList)
                {
                    if (pcs.Key == "PC1" && pcs.Value.InputType != vm.original_pcsList["PC1"].InputType)
                    {
                        vm.CurrentInputChange();
                    }
                    //if (pcs.Value.InputName != vm.original_pcsList[pcs.Key].InputName)
                    //{
                    //    bool binputname = DdpmCommonHelper.DeviceManagerSA.SetInputName(pcs.Value.InputType, pcs.Value.InputName).Result;
                    //}
                    if (pcs.Value.USBUpstream != vm.original_pcsList[pcs.Key].USBUpstream)
                    {
                        bool bUSBuptream = DdpmCommonHelper.DeviceManagerSA.SetUSBUpstream(vm.KvmModule.SelectedHomeDevice.MonitorInfo, pcs.Value.InputType, pcs.Value.USBUpstream).Result;
                        if (bUSBuptream)
                        {
                            Thread.Sleep(500);
                        }
                    }
                }
                bool bpcs = DdpmCommonHelper.DeviceManagerSA.SetUSBKVMPCsList(vm.KvmModule.SelectedHomeDevice.MonitorInfo, vm.pcsList).Result;
            }
        }

        private void KeyDown_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if ( ((e.KeyStates == Keyboard.GetKeyStates(Key.D1)) || (e.KeyStates == Keyboard.GetKeyStates(Key.D3))) && (Keyboard.Modifiers == ModifierKeys.Shift) )
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

        private void KeyDown_KeyDown1(object sender, KeyEventArgs e)
        {
            KeyDown_KeyDown(sender, e);
        }

        private void KeyDown_KeyDown2(object sender, KeyEventArgs e)
        {
            KeyDown_KeyDown(sender, e);
        }

        private void KeyDown_KeyDown3(object sender, KeyEventArgs e)
        {
            KeyDown_KeyDown(sender, e);
        }

        private void KeyDown_KeyDown4(object sender, KeyEventArgs e)
        {
            KeyDown_KeyDown(sender, e);
        }
    }
}
