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

namespace DDPM.UI.Common.EAEM
{
    /// <summary>
    /// Interaction logic for SplitCtrl2D.xaml
    /// </summary>
    public partial class SplitCtrl2D : UserControl, ISplit
    {
        #region Basic
        public Type CtrlType => typeof(SplitCtrl2D);
        ContentControl? ISplit.Content => this;
        #endregion

        #region Creation
        public SplitCtrl2D(List<double>? settings = null)
        {
            InitializeComponent();
            if (settings != null)
                SetSettings(settings);
        }
        public ISplit New(List<double>? settings = null)
        {
            return new SplitCtrl2A(settings);
        }
        #endregion

        #region Settings
        private List<double> _defaultSettings = new List<double>() { 3, 7 };
        public int SettingsCount => _defaultSettings.Count;

        /// <summary>
        /// Get settings from UI layout
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public List<double> GetSettings()
        {
            List<double> listOut = new List<double>();
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
            g0.ColumnDefinitions[0].Width = new GridLength(settings[0], GridUnitType.Star);
            g0.ColumnDefinitions[2].Width = new GridLength(settings[1], GridUnitType.Star);

            return true;
        }
        #endregion

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
        #endregion

    }
}
