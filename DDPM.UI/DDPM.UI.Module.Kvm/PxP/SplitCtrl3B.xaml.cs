using DDPM.UI.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using UserControl = System.Windows.Controls.UserControl;

namespace DDPM.UI.Module.Kvm
{
    /// <summary>
    /// Interaction logic for SplitCtrl3B.xaml
    /// </summary>
    public partial class PBPSplitCtrl3B : UserControl
    {
        private KvmViewModel vm
        {
            get
            {
                return (KvmViewModel)DataContext;
            }
        }

        #region Basic
        public Type CtrlType => typeof(PBPSplitCtrl3B);
        
        #endregion

        #region Creation
        public PBPSplitCtrl3B(List<double>? settings = null)
        {
            InitializeComponent();
        }
        
        #endregion

        #region For Pxp Usage
        /// <summary>
        /// The capability Code of PIP, the defult code is used for default settings
        /// </summary>
        public UInt16 PbpCapabilityCode { get; set; } = 0x0034;

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
        #endregion

        private void PBP_MouseLeftDown1(object sender, MouseButtonEventArgs e)
        {
            vm.PC1Click();
            vm.VideoSwapContent = vm.PxPcodeDictionary[PbpCapabilityCode]/*new PBPSplitCtrl2A()*/;
        }

        private void PBP_MouseLeftDown2(object sender, MouseButtonEventArgs e)
        {
            vm.PC2Click();
            vm.VideoSwapContent = vm.PxPcodeDictionary[PbpCapabilityCode];
        }
        private void PBP_MouseLeftDown3(object sender, MouseButtonEventArgs e)
        {
            vm.PC3Click();
            vm.VideoSwapContent = vm.PxPcodeDictionary[PbpCapabilityCode];
        }

        private void PBP_SwapClick1(object sender, MouseButtonEventArgs e)
        {
            vm.PCSwap("PC1", "PC2");
        }
        private void PBP_SwapClick2(object sender, MouseButtonEventArgs e)
        {
            vm.PCSwap("PC2", "PC3");
        }
    }
}
