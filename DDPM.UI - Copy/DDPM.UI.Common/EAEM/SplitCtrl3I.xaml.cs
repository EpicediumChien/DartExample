using DDPM.UI.Common.Interfaces;
using System.Windows;
using System.Windows.Controls;
using UserControl = System.Windows.Controls.UserControl;

namespace DDPM.UI.Common.EAEM
{
    /// <summary>
    /// Interaction logic for SplitCtrl3I.xaml
    /// </summary>
    public partial class SplitCtrl3I : UserControl, ISplit
    {
        #region Basic

        public Type CtrlType => typeof(SplitCtrl3I);
        ContentControl? ISplit.Content => this;

        #endregion Basic

        #region Creation

        public SplitCtrl3I(List<double>? settings = null)
        {
            InitializeComponent();
            if (settings != null)
                SetSettings(settings);
        }

        public ISplit New(List<double>? settings = null)
        {
            return new SplitCtrl3I(settings);
        }

        #endregion Creation

        #region Settings

        private List<double> _defaultSettings = new List<double>() { 1, 1, 1, 1 };
        public int SettingsCount => _defaultSettings.Count;

        /// <summary>
        /// Get settings from UI layout
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public List<double> GetSettings()
        {
            List<double> listOut = new List<double>();
            if (g0.RowDefinitions.Count >= 3)
            {
                listOut.Add(g0.RowDefinitions[0].Height.Value);
                listOut.Add(g0.RowDefinitions[2].Height.Value);
            }
            if (g0.ColumnDefinitions.Count >= 3)
            {
                listOut.Add(g0.ColumnDefinitions[0].Width.Value);
                listOut.Add(g0.ColumnDefinitions[2].Width.Value);
            }
            return listOut;
        }

        /// <summary>
        /// Apply the input settings to UI
        /// </summary>
        /// <param name="settings"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public bool SetSettings(List<double> settings)
        {
            if ((settings == null) || (settings.Count < SettingsCount))
                return false;
            g0.RowDefinitions[0].Height = new GridLength(settings[0], GridUnitType.Star);
            g0.RowDefinitions[2].Height = new GridLength(settings[1], GridUnitType.Star);
            g0.ColumnDefinitions[0].Width = new GridLength(settings[2], GridUnitType.Star);
            g0.ColumnDefinitions[2].Width = new GridLength(settings[3], GridUnitType.Star);

            return true;
        }

        #endregion Settings

        #region For Pxp Usage

        /// <summary>
        /// The capability Code of PIP, the defult code is used for default settings
        /// </summary>
        public UInt16 PbpCapabilityCode { get; set; } = 0x0031;

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
    }
}