using CommunityToolkit.Mvvm.Input;
using DDPM.SA.Common.Display;
using DDPM.UI.Common;
using DDPM.UI.Common.EAEM;
using DDPM.UI.Common.Interfaces;
using DDPM.UI.Common.Models;
using DDPM.UI.Common.UserControls;
using DDPM.UI.Plugin.DdpmHomePlugin;
using Dell.Client.Framework.UX.WPF;
using System.Windows;
using System.Windows.Controls;

namespace DDPM.UI.Module.Kvm
{
    /// <summary>
    /// Interaction logic for KVMPIPPBPFullView.xaml
    /// </summary>
    public partial class KVMPIPPBPFullView : UserControl
    {
        private bool isPipSmall = false;
        private bool isPipLarge = false;
        private bool isPBP = false;
        private UInt16 _pbpcode;

        private KvmViewModel vm
        {
            get
            {
                return (KvmViewModel)DataContext;
            }
        }

        #region Init

        public KVMPIPPBPFullView(KvmViewModel vm)
        {
            InitializeComponent();
            DataContext = vm;
            //Init for tha tabHeaders control
            tabHeaders.ItemsSource = new TabHeader[]
            {
                new TabHeader() { Text = "Fullscreen" },
                new TabHeader() { Text = "PIP" },
                new TabHeader() { Text = "PBP" }
            };

            //Assign SplitOwner to all SplitItems
            //  pipOff : PxpOff group
            //  pipSmall and pipLarge : PipList group
            //  all SplitItems in splitListView_Pbp : PbpList
            pipOff.SplitOwner = eSplitOwner.PxpOff;
            pipSmall.SplitOwner = eSplitOwner.PipList;
            pipLarge.SplitOwner = eSplitOwner.PipList;
            splitListView_Pbp.SplitOwner = eSplitOwner.PbpList;

            //Register the handlers when a SplitItem is clicked
            pipOff.ClickCommand = new RelayCommand<SplitItem?>(OnFullScreenClicked);
            pipSmall.ClickCommand = new RelayCommand<SplitItem?>(OnPipSmallClicked);
            pipLarge.ClickCommand = new RelayCommand<SplitItem?>(OnPipLargeClicked);
            splitListView_Pbp.ItemClickCommand = new RelayCommand<SplitItem?>(OnPbpItemClicked);
        }

        //Everytime when show this view will call to this method
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            //Determine the selected SplitItem depend on current PxP mode
            if (vm.CurPxpMode == KvmViewModel.PipMode_Off)
                vm.SelectedSplitItem = pipOff;
            else if (vm.CurPxpMode == KvmViewModel.PipMode_Small)
                vm.SelectedSplitItem = pipSmall;
            else if (vm.CurPxpMode == KvmViewModel.PipMode_Large)
                vm.SelectedSplitItem = pipLarge;

            //Rebuild splitListView based on vm.PipPbpCaps
            //It will also set IsSelected if the adding SplitItem is current PxpMode
            RefreshPbpSplitListView();
        }

        //Rebuild splitListView based on vm.PipPbpCaps
        //It will also set IsSelected if the adding SplitItem is current PxpMode
        private void RefreshPbpSplitListView()
        {
            splitListView_Pbp.ClearList();

            foreach (ISplit isp in ISplit.PipClasses)
            {
                //Check if SelectedHomeDevice have the capability
                if (vm.HasPxpCap(isp.PbpCapabilityCode))
                {
                    SplitItem spItem = splitListView_Pbp.AddSplitToList(isp);
                    if (vm.CurPxpMode == isp.PbpCapabilityCode)
                        vm.SelectedSplitItem = spItem;
                }
            }
        }

        #endregion Init

        private void BackMKFullView(object sender, RoutedEventArgs e)
        {
            ConnectMKFullView connectMKFullView = new ConnectMKFullView();
            connectMKFullView.DataContext = vm;
            DdpmCommonHelper.ModuleOwner?.OpenFullView(connectMKFullView);
            if (vm.ToProgressValue > 3 || vm.ToProgressValue < 0)
            {
                vm.ToProgressValue = 3;
            }
            vm.FromProgressValue = vm.ToProgressValue;
            vm.ToProgressValue = vm.ToProgressValue - 1;
        }

        private void USBKVMFinish(object sender, RoutedEventArgs e)
        {
            try
            {
                bool b = DdpmCommonHelper.DeviceManagerSA.SetUSBKVMPCsList(DdpmCommonHelper.ModuleOwner.SelectedHomeDevice.MonitorInfo, vm.pcsList).Result;
                //set input source
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

                //set pxp
                if (vm.isPipSmall)
                {
                    bool bpxp = DdpmCommonHelper.DeviceManagerSA.SetPipModeSmall(DdpmCommonHelper.ModuleOwner.SelectedHomeDevice.MonitorInfo).Result;
                    if (bpxp)
                    {
                        Thread.Sleep(500);
                    }
                }
                else if (vm.isPipLarge)
                {
                    bool bpxp = DdpmCommonHelper.DeviceManagerSA.SetPipModeLarge(DdpmCommonHelper.ModuleOwner.SelectedHomeDevice.MonitorInfo).Result;
                    if (bpxp)
                    {
                        Thread.Sleep(500);
                    }
                }
                else if (vm.isPBP)
                {
                    bool bpxp = DdpmCommonHelper.DeviceManagerSA.SetPbpMode(DdpmCommonHelper.ModuleOwner.SelectedHomeDevice.MonitorInfo, vm.PxPCode).Result;
                    if (bpxp)
                    {
                        Thread.Sleep(500);
                    }
                }
                else
                {
                    bool bpxp = DdpmCommonHelper.DeviceManagerSA.SetPipModeOff(DdpmCommonHelper.ModuleOwner.SelectedHomeDevice.MonitorInfo).Result;
                    if (bpxp)
                    {
                        Thread.Sleep(500);
                    }
                }
                vm.isOnUSBKVM(true);//bool b = DdpmCommonHelper.DeviceManagerSA.SetOnUSBKVM(true).Result;
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

        #region SplitItem ClickCommand Handlers

        //For these handlers, need to handle below tasks
        // 1. Set the vm.SelectedSplitItem, this will (unselected previous SplitItem), (Set new SplitItem as Selected)
        // 2. Switch the SplitCtrl in SwapVideo content

        /// <summary>
        /// The handler when pipOff (SplitItem) is clicked
        /// </summary>
        /// <param name="spItem">The SplitItem of pipOff</param>
        private void OnFullScreenClicked(SplitItem? spItem)
        {
            //Update selectedSplitItem
            vm.SelectedSplitItem = spItem;
            if (spItem != null)
            {
                vm.isPipSmall = false;
                vm.isPipLarge = false;
                vm.isPBP = false;
                vm.PC1_Input = vm.pcsList["PC1"].InputType;
                //Get the Content of the new selected SplitItem
                vm.PxPCode = 0x0;
                vm.VideoSwapContent = vm.PxPcodeDictionary[0x0];
            }
        }

        private void OnPipSmallClicked(SplitItem? spItem)
        {
            vm.SelectedSplitItem = spItem;
            if (spItem != null)
            {
                vm.isPipSmall = true;
                vm.isPipLarge = false;
                vm.isPBP = false;
                vm.PC1_Input = vm.pcsList["PC1"].InputType;
                vm.PC2_Input = vm.pcsList["PC2"].InputType;
                //Get the Content of the new selected SplitItem
                PIPSplitCtrl1A splitCtrl1A = new PIPSplitCtrl1A();
                //splitCtrl1A.PC1_Input = vm.pcsList["PC1"].InputType;
                //splitCtrl1A.PC1_Visibility = Visibility.Visible;
                vm.PxPCode = 0x11;
                vm.VideoSwapContent = vm.PxPcodeDictionary[0x11];
            }
        }

        private void OnPipLargeClicked(SplitItem? spItem)
        {
            vm.SelectedSplitItem = spItem;
            if (spItem != null)
            {
                vm.isPipSmall = false;
                vm.isPipLarge = true;
                vm.isPBP = false;
                vm.PC1_Input = vm.pcsList["PC1"].InputType;
                vm.PC2_Input = vm.pcsList["PC2"].InputType;
                //Get the Content of the new selected SplitItem
                vm.PxPCode = 0x12;
                vm.VideoSwapContent = vm.PxPcodeDictionary[0x12];
            }
        }

        private void OnPbpItemClicked(SplitItem? spItem)
        {
            vm.SelectedSplitItem = spItem;
            if (spItem != null)
            {
                vm.isPipSmall = false;
                vm.isPipLarge = false;
                vm.isPBP = true;
                _pbpcode = vm.SelectedSplitItem.ISplit.PbpCapabilityCode;
                //Get the Content of the new selected SplitItem
                vm.PxPCode = _pbpcode;
                vm.VideoSwapContent = vm.PxPcodeDictionary[_pbpcode];
            }
        }

        #endregion SplitItem ClickCommand Handlers

        private void SaveInput(object sender, RoutedEventArgs e)
        {
            if (vm.isPipSmall)
            {
                bool bpxp = DdpmCommonHelper.DeviceManagerSA.SetPipModeSmall(DdpmCommonHelper.ModuleOwner.SelectedHomeDevice.MonitorInfo).Result;
                if (bpxp)
                {
                    Thread.Sleep(500);
                }
            }
            else if (vm.isPipLarge)
            {
                bool bpxp = DdpmCommonHelper.DeviceManagerSA.SetPipModeLarge(DdpmCommonHelper.ModuleOwner.SelectedHomeDevice.MonitorInfo).Result;
                if (bpxp)
                {
                    Thread.Sleep(500);
                }
            }
            else if (vm.isPBP)
            {
                bool bpxp = DdpmCommonHelper.DeviceManagerSA.SetPbpMode(DdpmCommonHelper.ModuleOwner.SelectedHomeDevice.MonitorInfo, vm.PxPCode).Result;
                if (bpxp)
                {
                    Thread.Sleep(500);
                }
            }
            else
            {
                bool bpxp = DdpmCommonHelper.DeviceManagerSA.SetPipModeOff(DdpmCommonHelper.ModuleOwner.SelectedHomeDevice.MonitorInfo).Result;
                if (bpxp)
                {
                    Thread.Sleep(500);
                }
            }
            //Return to DdpmHomePage
            IConsole? console = DdpmHomePlugin.PluginIoc.GetService<IConsole>();
            console?.ShowPluginById(DDPM.UI.Common.Constants.DdpmHomePluginId);
        }

        private void CloseUSBKVM(object sender, RoutedEventArgs e)
        {
            DdpmCommonHelper.ModuleOwner?.CloseFullView();
        }
    }
}