using DDPM.UI.Common.Interfaces;
using System.Windows.Controls;

namespace DDPM.UI.Common.EAEM
{
    /// <summary>
    /// Interaction logic for SplitCtrl1B.xaml
    /// </summary>
    public partial class SplitCtrl1B : System.Windows.Controls.UserControl, ISplit
    {
        #region Basic

        public Type CtrlType => typeof(SplitCtrl1B);
        ContentControl? ISplit.Content => this;

        #endregion Basic

        #region Creation

        public SplitCtrl1B()
        {
            InitializeComponent();
        }

        public SplitCtrl1B(List<double>? settings = null)
        {
            InitializeComponent();
            if (settings != null)
                SetSettings(settings);
        }

        public ISplit New(List<double>? settings = null)
        {
            return new SplitCtrl1B(settings);
        }

        #endregion Creation

        #region Settings

        private List<double> _defaultSettings = new List<double>() { 1, 1 };
        public int SettingsCount => _defaultSettings.Count;

        /// <summary>
        /// Get settings from UI layout
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public List<double> GetSettings()
        {
            return _defaultSettings;
        }

        /// <summary>
        /// Apply the input settings to UI
        /// </summary>
        /// <param name="settings"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public bool SetSettings(List<double> settings)
        {
            return true;
        }

        #endregion Settings

        #region For Pxp Usage

        /// <summary>
        /// The capability Code of PIP, the defult code is used for default settings
        /// </summary>
        public UInt16 PbpCapabilityCode { get; set; } = 0x0022;

        private PxpInfo _pxpInfo = new PxpInfo();

        public string Description
        {
            get
            {
                //if (String.IsNullOrEmpty(_pxpInfo.Description))
                //    return $"PIP Large";
                return _pxpInfo.Description;
            }
            set
            {
                _pxpInfo.Description = value;
            }
        }

        #endregion For Pxp Usage

    }
}