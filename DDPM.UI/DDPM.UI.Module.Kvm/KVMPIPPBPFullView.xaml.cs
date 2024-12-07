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
                vm.FinishtoSetPCs();

                //set pxp
                SetPxP();
                //if (vm.isPipSmall)
                //{
                //    bool bpxp = DdpmCommonHelper.DeviceManagerSA.SetPipModeSmall(DdpmCommonHelper.ModuleOwner.SelectedHomeDevice.MonitorInfo).Result;
                //    if (bpxp)
                //    {
                //        Thread.Sleep(500);
                //    }
                //}
                //else if (vm.isPipLarge)
                //{
                //    bool bpxp = DdpmCommonHelper.DeviceManagerSA.SetPipModeLarge(DdpmCommonHelper.ModuleOwner.SelectedHomeDevice.MonitorInfo).Result;
                //    if (bpxp)
                //    {
                //        Thread.Sleep(500);
                //    }
                //}
                //else if (vm.isPBP)
                //{
                //    bool bpxp = DdpmCommonHelper.DeviceManagerSA.SetPbpMode(DdpmCommonHelper.ModuleOwner.SelectedHomeDevice.MonitorInfo, vm.PxPCode).Result;
                //    if (bpxp)
                //    {
                //        Thread.Sleep(500);
                //    }
                //}
                //else
                //{
                //    bool bpxp = DdpmCommonHelper.DeviceManagerSA.SetPipModeOff(DdpmCommonHelper.ModuleOwner.SelectedHomeDevice.MonitorInfo).Result;
                //    if (bpxp)
                //    {
                //        Thread.Sleep(500);
                //    }
                //}
                vm.isOnUSBKVM(true);//bool b = DdpmCommonHelper.DeviceManagerSA.SetOnUSBKVM(true).Result;
                vm.isOnNKVM(false);
                vm.USBKVMisON = true;
                vm.NKVMisON = false;
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
                if (vm.pcsList.TryGetValue("PC1", out var pc1))
                {
                    vm.PC1_Input = vm.pcsList["PC1"].InputType;
                }
                else
                {
                    vm._log.Debug("PC1 not found in pcsList.");
                }
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
                if (vm.pcsList.TryGetValue("PC1", out var pc1) && vm.pcsList.TryGetValue("PC2", out var pc2))
                {
                    vm.PC1_Input = vm.pcsList["PC1"].InputType;
                    vm.PC2_Input = vm.pcsList["PC2"].InputType;
                }
                else
                {
                    vm._log.Debug("PC1 or PC2 not found in pcsList.");
                }
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
                if (vm.pcsList.TryGetValue("PC1", out var pc1) && vm.pcsList.TryGetValue("PC2", out var pc2))
                {
                    vm.PC1_Input = vm.pcsList["PC1"].InputType;
                    vm.PC2_Input = vm.pcsList["PC2"].InputType;
                }
                else
                {
                    vm._log.Debug("PC1 or PC2 not found in pcsList.");
                }
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

        private void SavePxP(object sender, RoutedEventArgs e)
        {
            vm._log!.Info("[KVMPIPPBPFullView]SavePxP");
            SetPxP();
            //bool bt = false;
            //if (vm.isPipSmall)
            //{
            //    vm._log!.Info("[KVMPIPPBPFullView]SetPipModeSmall");
            //    bool bpxp = DdpmCommonHelper.DeviceManagerSA.SetPipModeSmall(DdpmCommonHelper.ModuleOwner.SelectedHomeDevice.MonitorInfo).Result;
            //    if (bpxp)
            //    {
            //        bt = DdpmCommonHelper.DeviceManagerSA.SentKVMtoTelementry(DdpmCommonHelper.ModuleOwner.SelectedHomeDevice.MonitorInfo, "USBKVMMode", "PIP-Small").Result;
            //        Thread.Sleep(500);
            //    }
            //}
            //else if (vm.isPipLarge)
            //{
            //    vm._log!.Info("[KVMPIPPBPFullView]SetPipModeLarge");
            //    bool bpxp = DdpmCommonHelper.DeviceManagerSA.SetPipModeLarge(DdpmCommonHelper.ModuleOwner.SelectedHomeDevice.MonitorInfo).Result;
            //    if (bpxp)
            //    {
            //        bt = DdpmCommonHelper.DeviceManagerSA.SentKVMtoTelementry(DdpmCommonHelper.ModuleOwner.SelectedHomeDevice.MonitorInfo, "USBKVMMode", "PIP-Large").Result;
            //        Thread.Sleep(500);
            //    }
            //}
            //else if (vm.isPBP)
            //{
            //    vm._log!.Info("[KVMPIPPBPFullView]SetPbpMode");
            //    bool bpxp = DdpmCommonHelper.DeviceManagerSA.SetPbpMode(DdpmCommonHelper.ModuleOwner.SelectedHomeDevice.MonitorInfo, vm.PxPCode).Result;
            //    if (bpxp)
            //    {
            //        if (vm.PxPCode >= 0x23 && vm.PxPCode <= 0x2F)
            //        {
            //            bt = bt = DdpmCommonHelper.DeviceManagerSA.SentKVMtoTelementry(DdpmCommonHelper.ModuleOwner.SelectedHomeDevice.MonitorInfo, "USBKVMMode", "2-PBP").Result;
            //        }
            //        else if (vm.PxPCode >= 0x31 && vm.PxPCode <= 0x35)
            //        {
            //            bt = bt = DdpmCommonHelper.DeviceManagerSA.SentKVMtoTelementry(DdpmCommonHelper.ModuleOwner.SelectedHomeDevice.MonitorInfo, "USBKVMMode", "3-PBP").Result;
            //        }
            //        else if (vm.PxPCode >= 0x41 && vm.PxPCode <= 0x42)
            //        {
            //            bt = bt = DdpmCommonHelper.DeviceManagerSA.SentKVMtoTelementry(DdpmCommonHelper.ModuleOwner.SelectedHomeDevice.MonitorInfo, "USBKVMMode", "4-PBP").Result;
            //        }
            //        Thread.Sleep(500);
            //    }
            //}
            //else
            //{
            //    vm._log!.Info("[KVMPIPPBPFullView]SaveFull");
            //    bool bpxp = DdpmCommonHelper.DeviceManagerSA.SetPipModeOff(DdpmCommonHelper.ModuleOwner.SelectedHomeDevice.MonitorInfo).Result;
            //    if (bpxp)
            //    {
            //        bt = DdpmCommonHelper.DeviceManagerSA.SentKVMtoTelementry(DdpmCommonHelper.ModuleOwner.SelectedHomeDevice.MonitorInfo, "USBKVMMode", "Full").Result;
            //        Thread.Sleep(500);
            //    }
            //}
            //Return to DdpmHomePage
            IConsole? console = DdpmHomePlugin.PluginIoc.GetService<IConsole>();
            console?.ShowPluginById(DDPM.UI.Common.Constants.DdpmHomePluginId);
        }

        private void CloseUSBKVM(object sender, RoutedEventArgs e)
        {
            DdpmCommonHelper.ModuleOwner?.CloseFullView();
        }

        private void SetPxP()
        {
            bool bt = false;
            if (vm.isPipSmall)
            {
                vm._log?.Info("[KVMPIPPBPFullView]SetPipModeSmall");
                bool bpxp = DdpmCommonHelper.DeviceManagerSA.SetPipModeSmall(DdpmCommonHelper.ModuleOwner.SelectedHomeDevice.MonitorInfo).Result;
                if (bpxp)
                {
                    bt = DdpmCommonHelper.DeviceManagerSA.SentKVMtoTelementry(DdpmCommonHelper.ModuleOwner.SelectedHomeDevice.MonitorInfo, "USBKVMMode", "PIP-Small").Result;
                    Thread.Sleep(1000);
                }
            }
            else if (vm.isPipLarge)
            {
                vm._log?.Info("[KVMPIPPBPFullView]SetPipModeLarge");
                bool bpxp = DdpmCommonHelper.DeviceManagerSA.SetPipModeLarge(DdpmCommonHelper.ModuleOwner.SelectedHomeDevice.MonitorInfo).Result;
                if (bpxp)
                {
                    bt = DdpmCommonHelper.DeviceManagerSA.SentKVMtoTelementry(DdpmCommonHelper.ModuleOwner.SelectedHomeDevice.MonitorInfo, "USBKVMMode", "PIP-Large").Result;
                    Thread.Sleep(1000);
                }
            }
            else if (vm.isPBP)
            {
                vm._log?.Info("[KVMPIPPBPFullView]SetPbpMode");
                bool bpxp = DdpmCommonHelper.DeviceManagerSA.SetPbpMode(DdpmCommonHelper.ModuleOwner.SelectedHomeDevice.MonitorInfo, vm.PxPCode).Result;
                if (bpxp)
                {
                    if (vm.PxPCode >= 0x23 && vm.PxPCode <= 0x2F)
                    {
                        bt = bt = DdpmCommonHelper.DeviceManagerSA.SentKVMtoTelementry(DdpmCommonHelper.ModuleOwner.SelectedHomeDevice.MonitorInfo, "USBKVMMode", "2-PBP").Result;
                    }
                    else if (vm.PxPCode >= 0x31 && vm.PxPCode <= 0x35)
                    {
                        bt = bt = DdpmCommonHelper.DeviceManagerSA.SentKVMtoTelementry(DdpmCommonHelper.ModuleOwner.SelectedHomeDevice.MonitorInfo, "USBKVMMode", "3-PBP").Result;
                    }
                    else if (vm.PxPCode >= 0x41 && vm.PxPCode <= 0x42)
                    {
                        bt = bt = DdpmCommonHelper.DeviceManagerSA.SentKVMtoTelementry(DdpmCommonHelper.ModuleOwner.SelectedHomeDevice.MonitorInfo, "USBKVMMode", "4-PBP").Result;
                    }
                    Thread.Sleep(1000);
                }
            }
            else
            {
                vm._log?.Info("[KVMPIPPBPFullView]SaveFull");
                bool bpxp = DdpmCommonHelper.DeviceManagerSA.SetPipModeOff(DdpmCommonHelper.ModuleOwner.SelectedHomeDevice.MonitorInfo).Result;
                if (bpxp)
                {
                    bt = DdpmCommonHelper.DeviceManagerSA.SentKVMtoTelementry(DdpmCommonHelper.ModuleOwner.SelectedHomeDevice.MonitorInfo, "USBKVMMode", "Full").Result;
                    Thread.Sleep(1000);
                }
            }
        }
    }
}