using DDPM.UI.Common;
using System.Windows.Input;
using UserControl = System.Windows.Controls.UserControl;

namespace DDPM.UI.Module.Kvm
{
    /// <summary>
    /// Interaction logic for SplitCtrl2B.xaml
    /// </summary>
    public partial class PBPSplitCtrl2B : UserControl
    {
        private KvmViewModel vm
        {
            get
            {
                return (KvmViewModel)DataContext;
            }
        }

        #region Basic

        public Type CtrlType => typeof(PBPSplitCtrl2B);
        //ContentControl? ISplit.Content => this;

        #endregion Basic

        #region Creation

        public PBPSplitCtrl2B(List<double>? settings = null)
        {
            InitializeComponent();

            //if (settings != null)
            //    SetSettings(settings);
        }

        //public ISplit New(List<double>? settings = null)
        //{
        //    return new SplitCtrl2B(settings);
        //}

        #endregion Creation

        #region Settings

        private List<double> _defaultSettings = new List<double>() { 1, 1 };
        public int SettingsCount => _defaultSettings.Count;

        /// <summary>
        /// Get settings from UI layout
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        //public List<double> GetSettings()
        //{
        //    List<double> listOut = new List<double>();
        //    if (g0.RowDefinitions.Count >= 3)
        //    {
        //        listOut.Add(g0.RowDefinitions[0].Height.Value);
        //        listOut.Add(g0.RowDefinitions[2].Height.Value);
        //    }
        //    return listOut;
        //}

        /// <summary>
        /// Apply the input settings to UI
        /// </summary>
        /// <param name="settings"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        //public bool SetSettings(List<double> settings)
        //{
        //    if ((settings == null) || (settings.Count < SettingsCount))
        //        return false;
        //    g0.RowDefinitions[0].Height = new GridLength(settings[0], GridUnitType.Star);
        //    g0.RowDefinitions[2].Height = new GridLength(settings[1], GridUnitType.Star);

        //    return true;
        //}

        #endregion Settings

        #region For Pxp Usage

        /// <summary>
        /// The capability Code of PIP, the defult code is used for default settings
        /// </summary>
        public UInt16 PbpCapabilityCode { get; set; } = 0x002F;

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
                vm.VideoSwapContent = new PBPSplitCtrl2B();
                vm.VideoSwapContent_Left = new PBPSplitCtrl2B();
            }
        }

        private void PBP_MouseLeftDown2(object sender, MouseButtonEventArgs e)
        {
            if (vm != null)
            {
                vm.PC2Click();
                vm.VideoSwapContent = new PBPSplitCtrl2B();
                vm.VideoSwapContent_Left = new PBPSplitCtrl2B();
            }
        }

        private void PBP_SwapClick(object sender, MouseButtonEventArgs e)
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
    }
}