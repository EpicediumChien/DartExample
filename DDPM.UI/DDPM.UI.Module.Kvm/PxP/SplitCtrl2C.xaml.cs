using System.Windows.Input;
using UserControl = System.Windows.Controls.UserControl;

namespace DDPM.UI.Module.Kvm
{
    /// <summary>
    /// Interaction logic for SplitCtrl2C.xaml
    /// </summary>
    public partial class PBPSplitCtrl2C : UserControl
    {
        private KvmViewModel vm
        {
            get
            {
                return (KvmViewModel)DataContext;
            }
        }

        #region Basic

        public Type CtrlType => typeof(PBPSplitCtrl2C);
        //ContentControl? ISplit.Content => this;

        #endregion Basic

        #region Creation

        public PBPSplitCtrl2C(List<double>? settings = null)
        {
            InitializeComponent();
            //if (settings != null)
            //    SetSettings(settings);
        }

        //public ISplit New(List<double>? settings = null)
        //{
        //    return new SplitCtrl2C(settings);
        //}

        #endregion Creation

        //#region Settings
        //private List<double> _defaultSettings = new List<double>() { 7, 3 };
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
        public UInt16 PbpCapabilityCode { get; set; } = 0x0026;

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
            vm.PC1Click();
            vm.VideoSwapContent = new PBPSplitCtrl2C();
        }

        private void PBP_MouseLeftDown2(object sender, MouseButtonEventArgs e)
        {
            vm.PC2Click();
            vm.VideoSwapContent = new PBPSplitCtrl2C();
        }

        private void PBP_SwapClick(object sender, MouseButtonEventArgs e)
        {
            vm.PCSwap("PC1", "PC2");
        }
    }
}