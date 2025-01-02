using DDPM.UI.Common;
using System.Windows.Input;
using UserControl = System.Windows.Controls.UserControl;

namespace DDPM.UI.Module.Kvm
{
    /// <summary>
    /// Interaction logic for SplitCtrl2A.xaml
    /// </summary>
    public partial class PBPSplitCtrl2A : UserControl
    {
        private KvmViewModel vm
        {
            get
            {
                return (KvmViewModel)DataContext;
            }
        }

        #region Basic

        public Type CtrlType => typeof(PBPSplitCtrl2A);
        //ContentControl? ISplit.Content => this;

        #endregion Basic

        #region Creation

        public PBPSplitCtrl2A()
        {
            InitializeComponent();

            //SetSettings(settings);
        }

        //public ISplit New(List<double>? settings = null)
        //{
        //    return new PBPSplitCtrl2A(settings);
        //}

        #endregion Creation

        //#region Settings
        //private List<double> _defaultSettings = new List<double>() { 1, 1 };
        //public int SettingsCount => _defaultSettings.Count;

        ///// <summary>
        ///// Get settings from UI layout
        ///// </summary>
        ///// <returns></returns>
        ///// <exception cref="NotImplementedException"></exception>
        //public List<double> GetSettings()
        //{
        //    List<double> listOut = new List<double>();
        //    if (g0.ColumnDefinitions.Count >= 3)
        //    {
        //        listOut.Add(g0.ColumnDefinitions[0].Width.Value);
        //        listOut.Add(g0.ColumnDefinitions[2].Width.Value);
        //    }
        //    return listOut;
        //}

        ///// <summary>
        ///// Apply the input settings to UI
        ///// </summary>
        ///// <param name="settings"></param>
        ///// <returns></returns>
        ///// <exception cref="NotImplementedException"></exception>
        //public bool SetSettings(List<double> settings)
        //{
        //    if ((settings == null) || (settings.Count < SettingsCount))
        //        return false;
        //    g0.ColumnDefinitions[0].Width = new GridLength(settings[0], GridUnitType.Star);
        //    g0.ColumnDefinitions[2].Width = new GridLength(settings[1], GridUnitType.Star);

        //    return true;
        //}
        //#endregion

        #region For Pxp Usage

        /// <summary>
        /// The capability Code of PIP, the defult code is used for default settings
        /// </summary>
        public UInt16 PbpCapabilityCode { get; set; } = 0x0023;

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

        //public ICommand? ClickCommand { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        //public bool IsSelected { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        private void PBP_MouseLeftDown1(object sender, MouseButtonEventArgs e)
        {
            if (vm != null)
            {
                vm.PC1Click();
                vm.VideoSwapContent = new PBPSplitCtrl2A();
                vm.VideoSwapContent_Left = new PBPSplitCtrl2A();
            }
        }

        private void PBP_MouseLeftDown2(object sender, MouseButtonEventArgs e)
        {
            if (vm != null)
            {
                vm.PC2Click();
                vm.VideoSwapContent = new PBPSplitCtrl2A();
                vm.VideoSwapContent_Left = new PBPSplitCtrl2A();
            }
        }

        private void PBP_SwapClick(object sender, MouseButtonEventArgs e)
        {
            if (vm != null)
            {
                vm.PCSwap("PC1", "PC2");
                //vm.VideoSwapContent = new PBPSplitCtrl2A();
                if (!vm.isPxPFullView)
                {
                    DdpmCommonHelper.DeviceManagerSA.VideoSwap(vm.KvmModule.SelectedHomeDevice.MonitorInfo, 0, 1);
                }
            }
        }
    }
}