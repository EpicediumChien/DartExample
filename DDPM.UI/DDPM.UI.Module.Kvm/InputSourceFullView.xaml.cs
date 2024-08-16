using DDPM.SA.Common.Display;
using DDPM.UI.Common;
using System.Windows;
using System.Windows.Controls;

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
        }

        private void CloseUSBKVM(object sender, RoutedEventArgs e)
        {
            DdpmCommonHelper.ModuleOwner?.CloseFullView();
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
    }
}