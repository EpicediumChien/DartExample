using DDPM.UI.Common;
using System.Windows.Input;
using UserControl = System.Windows.Controls.UserControl;

namespace DDPM.UI.Module.Kvm
{
    /// <summary>
    /// Interaction logic for SplitCtrl3I.xaml
    /// </summary>
    public partial class PBPSplitCtrl3I : UserControl
    {
        private KvmViewModel vm
        {
            get
            {
                return (KvmViewModel)DataContext;
            }
        }

        #region Basic

        public Type CtrlType => typeof(PBPSplitCtrl3I);

        #endregion Basic

        #region Creation

        public PBPSplitCtrl3I()
        {
            InitializeComponent();
        }

        #endregion Creation

        #region For Pxp Usage

        /// <summary>
        /// The capability Code of PIP, the defult code is used for default settings
        /// </summary>
        public UInt16 PbpCapabilityCode { get; set; } = 0x0035;

        private PxpInfo _pxpInfo = new PxpInfo();

        public string Description
        {
            get
            {
                if (String.IsNullOrEmpty(_pxpInfo.Description))
                    return $"PBP Capability Code={PbpCapabilityCode:X02}h";
                return _pxpInfo.Description;
            }
            set
            {
                _pxpInfo.Description = value;
            }
        }

        #endregion For Pxp Usage

        private void PBP_MouseLeftDown1(object sender, MouseButtonEventArgs e)
        {
            if (vm != null)
            {
                vm.PC1Click();
                vm.VideoSwapContent = new PBPSplitCtrl3I();
                vm.VideoSwapContent_Left = new PBPSplitCtrl3I();
            }
        }

        private void PBP_MouseLeftDown2(object sender, MouseButtonEventArgs e)
        {
            if (vm != null)
            {
                vm.PC2Click();
                vm.VideoSwapContent = new PBPSplitCtrl3I();
                vm.VideoSwapContent_Left = new PBPSplitCtrl3I();
            }
        }

        private void PBP_MouseLeftDown3(object sender, MouseButtonEventArgs e)
        {
            if (vm != null)
            {
                vm.PC3Click();
                vm.VideoSwapContent = new PBPSplitCtrl3I();
                vm.VideoSwapContent_Left = new PBPSplitCtrl3I();
            }
        }

        private void PBP_SwapClick1(object sender, MouseButtonEventArgs e)
        {
            if (vm != null)
            {
                vm.PCSwap("PC1", "PC2");
                if (!vm.isPxPFullView)
                {
                    if (DdpmCommonHelper.DeviceManagerSA.VideoSwap(vm.KvmModule.SelectedHomeDevice.MonitorInfo, 0, 1).Result)
                    {
                        vm.UpdatePCList();
                    }
                }
            }
        }

        private void PBP_SwapClick2(object sender, MouseButtonEventArgs e)
        {
            if (vm != null)
            {
                vm.PCSwap("PC2", "PC3");
                if (!vm.isPxPFullView)
                {
                    if (DdpmCommonHelper.DeviceManagerSA.VideoSwap(vm.KvmModule.SelectedHomeDevice.MonitorInfo, 1, 2).Result)
                    {
                        vm.UpdatePCList();
                    }
                }
            }
        }

        private void PBP_SwapClick3(object sender, MouseButtonEventArgs e)
        {
            if (vm != null)
            {
                vm.PCSwap("PC1", "PC3");
                if (!vm.isPxPFullView)
                {
                    if (DdpmCommonHelper.DeviceManagerSA.VideoSwap(vm.KvmModule.SelectedHomeDevice.MonitorInfo, 0, 2).Result)
                    { 
                        vm.UpdatePCList(); 
                    }
                }
            }
        }
    }
}