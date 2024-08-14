using CommunityToolkit.Mvvm.ComponentModel;
using DDPM.SA.Common.Settings;
using DDPM.UI.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
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
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("DDPM.UI.Plugin.SettingsPlugin.Tests")]
namespace DDPM.UI.Plugin.SettingsPlugin
{
    internal class AnalyticsViewModel : ObservableObject, INotifyPropertyChanged
    {
        public new event PropertyChangedEventHandler? PropertyChanged;

        private string _strTitle = "Analytics";
        private string _strContent = "Help Dell improve its products and services automatically sending diagnostics and usage data.";
        private string _strUrlBtnContent = "Dell's Privacy Policy";
        private string _strPrivacyUrl = "https://www.dell.com/learn/us/en/uscorp1/policies-privacy-country-specific-privacy-policy";
        private string _strCheckBtnText = "Help Dell improve its products and services automatically";

        public string strCheckBtnText
        {
            get
            { return _strCheckBtnText; }
            set
            {
                _strCheckBtnText = value;
                NotifyPropertyChanged("strCheckBtnText");
            }
        }

        public string strUrlBtnContent
        {
            get
            { return _strUrlBtnContent; }
            set
            {
                _strUrlBtnContent = value;
                NotifyPropertyChanged("strUrlBtnContent");
            }
        }

        public string strPrivacyUrl
        {
            get
            { return _strPrivacyUrl; }
            set
            {
                _strPrivacyUrl = value;
                //NotifyPropertyChanged("strPrivacyUrl");
            }
        }

        public string strTitle
        {
            get
            { return _strTitle; }
            set
            {
                _strTitle = value;
                NotifyPropertyChanged("strTitle");
            }
        }

        public string strContent
        {
            get
            { return _strContent; }
            set
            {
                _strContent = value;
                NotifyPropertyChanged("strContent");
            }
        }

        private bool _isCheckEnable = true;

        public bool isCheckEnable
        {
            get { return _isCheckEnable; }
            set
            {
                _isCheckEnable = value;
                NotifyPropertyChanged("isCheckEnable");
            }
        }

        private bool _isConsentChecked = false;

        public bool isConsentChecked
        {
            get { return _isConsentChecked; }
            set
            {
                _isConsentChecked = value;
                NotifyPropertyChanged("isConsentChecked");
            }
        }


        public AnalyticsViewModel()
        {

        }

        private void NotifyPropertyChanged(string info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }
    }
    /// <summary>
    /// Interaction logic for AnalyticsPage.xaml
    /// </summary>
    public partial class AnalyticsPage : UserControl
    {
        public AnalyticsPage()
        {
            InitializeComponent();

            var vm = new AnalyticsViewModel();
            this.DataContext = vm;

            if (DdpmCommonHelper.DeviceManagerSA == null)
                return;
            try
            {
                DDPMSettings data = DdpmCommonHelper.DeviceManagerSA.ReloadAppConfigData().Result;
                if (data == null)
                    return;
                if (data.UserSettings == null)
                    return;

                vm.isCheckEnable = data.UserSettings.isTelemetryConsentAllow;
                vm.isConsentChecked = data.UserSettings.isTelemetryConsentOn;
            }
            catch (Exception)
            {

            }
        }

        private void url_btn_Click(object sender, RoutedEventArgs e)
        {
            AnalyticsViewModel vm = (AnalyticsViewModel)this.DataContext;
            if (vm == null)
                return;

            string url = vm.strPrivacyUrl;
            // Open the browser and navigate to specified url
            Process.Start(new ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true
            });
        }
    }
}
