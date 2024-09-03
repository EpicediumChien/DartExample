using CommunityToolkit.Mvvm.Input;
using DDPM.UI.Common.EAEM;
using DDPM.UI.Common.UserControls;
using DDPM.UI.Plugin.DdpmHomePlugin;
using Dell.Client.Framework.UX.WPF;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace DDPM.UI.Module.PipPbp
{
    /// <summary>
    /// Interaction logic for PipPbpRightView.xaml
    /// </summary>
    public partial class PipPbpRightView : UserControl
    {
        #region Init

        public PipPbpRightView(PipPbpViewModel _vm)
        {
            InitializeComponent();
            DataContext = _vm;
            _vm.SplitItem_Off = pipOff;
            _vm.SplitItem_PipSmall = pipSmall;
            _vm.SplitItem_PipLarge = pipLarge;
            _vm.SplitListView_Pbp = spliListView_Pbp;

            //Assign SplitOwner, so we can identify a SplitItem is belong to which list
            pipOff.SplitOwner = eSplitOwner.PxpOff;
            pipSmall.SplitOwner = eSplitOwner.PipList;
            pipLarge.SplitOwner = eSplitOwner.PipList;
            spliListView_Pbp.SplitOwner = eSplitOwner.PbpList;

            pipOff.InnerContent = new SplitCtrl0A(null) { Description = "Full screen" };
            pipSmall.InnerContent = new SplitCtrl1A(null) { Description = "PIP Small" };
            pipLarge.InnerContent = new SplitCtrl1B(null) { Description = "PIP Large" };

            //_vm.On
            _vm.RefreshData();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            //Workaround, until I can fix issue: pipOff.ClickCommand always null using data binding
            //
            pipOff.ClickCommand = new RelayCommand<SplitItem>(vm.OnFullScreenClicked);
            pipSmall.ClickCommand = new RelayCommand<SplitItem>(vm.OnPipSmallClicked);
            pipLarge.ClickCommand = new RelayCommand<SplitItem>(vm.OnPipLargeClicked);
            spliListView_Pbp.ItemClickCommand = new RelayCommand<SplitItem>(vm.OnPbpItemClicked);

            ////Determine which SplitItem is the selected item, base on current monitor setting
            //if (vm.CurPxpMode == PipPbpViewModel.PipMode_Off)
            //    vm.SelectedSplitItem = pipOff;
            //else if (vm.CurPxpMode == PipPbpViewModel.PipMode_Small)
            //    vm.SelectedSplitItem = pipSmall;
            //else if (vm.CurPxpMode == PipPbpViewModel.PipMode_Large)
            //    vm.SelectedSplitItem = pipLarge;

            //Build the Pbp list base on the monitor PBP capabilities
            //spliListView_Pbp.ClearList();
            //foreach (ISplit isp in ISplit.PipClasses)
            //{
            //    //Check if SelectedHomeDevice have the capability

            //    spliListView_Pbp.AddSplitToList(isp);
            //}

            /*
            SplitItem sp2A = new SplitItem();
            sp2A.InnerContent = new SplitCtrl2A();
            spliListView_Pip.AddSplitToList(sp2A);

            SplitItem sp2B = new SplitItem();
            sp2B.InnerContent = new SplitCtrl2B();
            spliListView_Pip.AddSplitToList(sp2B);

            SplitItem sp5A = new SplitItem();
            sp5A.InnerContent = new SplitCtrl5A();
            spliListView_Pip.AddSplitToList(sp5A);
            */
        }

        #endregion Init

        private PipPbpViewModel vm
        {
            get => (PipPbpViewModel)this.DataContext;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            IConsole? console = DdpmHomePlugin.PluginIoc.GetService<IConsole>();
            console?.ShowPluginById(Common.Constants.WholeWindowPluginId);
        }

        #region ClickCommand for SplitItems

        private void OnFullScreenClicked(SplitItem spItem)
        {
            //spItem.IsSelected = !spItem.IsSelected;
            vm.OnFullScreenClicked(spItem);
        }

        private void OnPipSmallClicked(SplitItem spItem)
        {
            vm.OnPipSmallClicked((SplitItem)spItem);
        }

        private void OnPipLargeClicked(SplitItem spItem)
        {
            vm.OnPipLargeClicked((SplitItem)spItem);
        }

        #endregion ClickCommand for SplitItems

        private void HandleSplitItemClickCommand(SplitItem spItem)
        {
            if (vm.SelectedSplitItem != null)
            {
                if (vm.SelectedSplitItem == spItem)
                    return;
                vm.SelectedSplitItem.IsSelected = false;
            }
            vm.SelectedSplitItem = spItem;
            vm.SelectedSplitItem.IsSelected = true;
        }

        private void UserControl_GotFocus(object sender, RoutedEventArgs e)
        {
            Trace.WriteLine("PipPbpRightView.GotFocus");
        }

        private void UserControl_LostFocus(object sender, RoutedEventArgs e)
        {
            Trace.WriteLine("PipPbpRightView.LostFocus");
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            Trace.WriteLine("PipPbpRightView.Unloaded");
        }

        #region Capabilities event handler

        public event RoutedEventHandler OnCapabilitiesChanged;

        #endregion Capabilities event handler

        private void usbSwitchButton_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            vm.ExecuteUsbSwitch();
        }

        private void videoSwapButton_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            vm.ExecuteVideoSwap();
        }
    }
}