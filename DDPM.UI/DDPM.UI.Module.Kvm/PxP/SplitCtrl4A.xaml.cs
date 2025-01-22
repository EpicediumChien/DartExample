using DDPM.UI.Common;
using System.Windows.Input;
using UserControl = System.Windows.Controls.UserControl;

namespace DDPM.UI.Module.Kvm
{
    /// <summary>
    /// Interaction logic for SplitCtrl4A.xaml
    /// </summary>
    public partial class PBPSplitCtrl4A : UserControl
    {
        private KvmViewModel vm
        {
            get
            {
                return (KvmViewModel)DataContext;
            }
        }

        #region Basic

        public Type CtrlType => typeof(PBPSplitCtrl4A);

        #endregion Basic

        #region Creation

        public PBPSplitCtrl4A(List<double>? settings = null)
        {
            InitializeComponent();
        }

        #endregion Creation

        #region For Pxp Usage

        /// <summary>
        /// The capability Code of PIP, the defult code is used for default settings
        /// </summary>
        public UInt16 PbpCapabilityCode { get; set; } = 0x0041;

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
                vm.VideoSwapContent = new PBPSplitCtrl4A();
                vm.VideoSwapContent_Left = new PBPSplitCtrl4A();
            }
        }

        private void PBP_MouseLeftDown2(object sender, MouseButtonEventArgs e)
        {
            if (vm != null)
            {
                vm.PC2Click();
                vm.VideoSwapContent = new PBPSplitCtrl4A();
                vm.VideoSwapContent_Left = new PBPSplitCtrl4A();
            }
        }

        private void PBP_MouseLeftDown3(object sender, MouseButtonEventArgs e)
        {
            if (vm != null)
            {
                vm.PC3Click();
                vm.VideoSwapContent = new PBPSplitCtrl4A();
                vm.VideoSwapContent_Left = new PBPSplitCtrl4A();
            }
        }

        private void PBP_MouseLeftDown4(object sender, MouseButtonEventArgs e)
        {
            if (vm != null)
            {
                vm.PC4Click();
                vm.VideoSwapContent = new PBPSplitCtrl4A();
                vm.VideoSwapContent_Left = new PBPSplitCtrl4A();
            }
        }

        private void PBP_SwapClick1(object sender, MouseButtonEventArgs e)
        {
            if (vm != null)
            {
                vm.PCSwap("PC1", "PC2");
                if (!vm.isPxPFullView)
                {
                    DdpmCommonHelper.DeviceManagerSA.VideoSwap(vm.KvmModule.SelectedHomeDevice.MonitorInfo, 0, 1);
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
                    DdpmCommonHelper.DeviceManagerSA.VideoSwap(vm.KvmModule.SelectedHomeDevice.MonitorInfo, 1, 2);
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
                    DdpmCommonHelper.DeviceManagerSA.VideoSwap(vm.KvmModule.SelectedHomeDevice.MonitorInfo, 0, 2);
                }
            }
        }

        private void PBP_SwapClick4(object sender, MouseButtonEventArgs e)
        {
            if (vm != null)
            {
                vm.PCSwap("PC1", "PC4");
                if (!vm.isPxPFullView)
                {
                    DdpmCommonHelper.DeviceManagerSA.VideoSwap(vm.KvmModule.SelectedHomeDevice.MonitorInfo, 0, 3);
                }
            }
        }

        private void PBP_SwapClick5(object sender, MouseButtonEventArgs e)
        {
            if (vm != null)
            {
                vm.PCSwap("PC2", "PC4");
                if (!vm.isPxPFullView)
                {
                    DdpmCommonHelper.DeviceManagerSA.VideoSwap(vm.KvmModule.SelectedHomeDevice.MonitorInfo, 1, 3);
                }
            }
        }

        private void PBP_SwapClick6(object sender, MouseButtonEventArgs e)
        {
            if (vm != null)
            {
                vm.PCSwap("PC3", "PC4");
                if (!vm.isPxPFullView)
                {
                    DdpmCommonHelper.DeviceManagerSA.VideoSwap(vm.KvmModule.SelectedHomeDevice.MonitorInfo, 2, 3);
                }
            }
        }
    }
}