using DDPM.SA.Common.Display;
using DDPM.UI.Common;
using DDPM.UI.Plugin.DdpmHomePlugin;
using Dell.Client.Framework.UX.WPF;
using System.Windows;
using System.Windows.Controls;

namespace DDPM.UI.Module.Kvm
{
    /// <summary>
    /// Interaction logic for ConnectMKFullView.xaml
    /// </summary>
    public partial class ConnectMKFullView : UserControl
    {
        private KvmViewModel vm
        {
            get
            {
                return (KvmViewModel)DataContext;
            }
        }

        public ConnectMKFullView()
        {
            InitializeComponent();
        }

        private void BackInputFullView(object sender, RoutedEventArgs e)
        {
            InputSourceFullView inputSourceFullView = new InputSourceFullView();
            inputSourceFullView.DataContext = vm;
            DdpmCommonHelper.ModuleOwner?.OpenFullView(inputSourceFullView);
            if (vm.ToProgressValue > 3 || vm.ToProgressValue < 0)
            {
                vm.ToProgressValue = 2;
            }
            vm.FromProgressValue = vm.ToProgressValue;
            vm.ToProgressValue = vm.ToProgressValue - 1;
        }

        private void OpenPxPFullView(object sender, RoutedEventArgs e)
        {
            KVMPIPPBPFullView kvmPIPPBPFullView = new KVMPIPPBPFullView(vm);
            kvmPIPPBPFullView.DataContext = vm;
            DdpmCommonHelper.ModuleOwner?.OpenFullView(kvmPIPPBPFullView);
            if (vm.ToProgressValue > 3 || vm.ToProgressValue < 0)
            {
                vm.ToProgressValue = 2;
            }
            vm.FromProgressValue = vm.ToProgressValue;
            vm.ToProgressValue = vm.ToProgressValue + 1;
        }

        private void CloseUSBKVM(object sender, RoutedEventArgs e)
        {
            DdpmCommonHelper.ModuleOwner?.CloseFullView();
            vm.FromProgressValue = 0;
            vm.ToProgressValue = 1;
        }

        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            //splitGrid.ActualWidth
        }

        private void USBKVMFinish(object sender, RoutedEventArgs e)
        {
            try
            {
                bool b = DdpmCommonHelper.DeviceManagerSA.SetUSBKVMPCsList(DdpmCommonHelper.ModuleOwner.SelectedHomeDevice.MonitorInfo, vm.pcsList).Result;
                //set input source
                if (vm.pcsList != vm.original_pcsList)
                {
                    if (vm.pcsList.TryGetValue("PC1", out var pc1) && vm.pcsList.TryGetValue("PC2", out var pc2))
                    {
                        InputSourceObj pc1input = new InputSourceObj(vm.pcsList["PC1"].InputType);
                        InputSourceObj pc2input = new InputSourceObj(vm.pcsList["PC2"].InputType);
                        if (vm.pcsList.Count >= 3)
                        {
                            if (vm.pcsList.TryGetValue("PC3", out var pc3))
                            {
                                InputSourceObj pc3input = new InputSourceObj(vm.pcsList["PC3"].InputType);
                                if (vm.pcsList.Count == 4)
                                {
                                    if (vm.pcsList.TryGetValue("PC4", out var pc4))
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
                                        vm._log.Debug("PC4 not found in pcsList.");
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
                                vm._log.Debug("PC3 not found in pcsList.");
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
                        vm.isOnUSBKVM(true);//bool b = DdpmCommonHelper.DeviceManagerSA.SetOnUSBKVM(true).Result;
                    }
                    else
                    {
                        vm._log.Debug("PC1 or PC2 not found in pcsList.");
                    }
                }
                //Return to DdpmHomePage              
                IConsole? console = DdpmHomePlugin.PluginIoc.GetService<IConsole>();
                console?.ShowPluginById(DDPM.UI.Common.Constants.DdpmHomePluginId);
                vm.FromProgressValue = 0;
                vm.ToProgressValue = 1;
            }
            catch (Exception ex)
            {
                //Return to DdpmHomePage
                IConsole? console = DdpmHomePlugin.PluginIoc.GetService<IConsole>();
                console?.ShowPluginById(DDPM.UI.Common.Constants.DdpmHomePluginId);
                vm.FromProgressValue = 0;
                vm.ToProgressValue = 1;
            }
        }
    }
}