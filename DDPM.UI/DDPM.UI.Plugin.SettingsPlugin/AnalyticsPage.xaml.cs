using CommunityToolkit.Mvvm.ComponentModel;
using DDPM.SA.Common.Settings;
using DDPM.UI.Common;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;

[assembly: InternalsVisibleTo("DDPM.UI.Plugin.SettingsPlugin.Tests")]

namespace DDPM.UI.Plugin.SettingsPlugin
{
    internal class AnalyticsViewModel : ObservableObject, INotifyPropertyChanged
    {
        public new event PropertyChangedEventHandler? PropertyChanged;

        //private string _strTitle = "Analytics";
        //private string _strContent = "Help Dell improve its products and services automatically sending diagnostics and usage data.";
        //private string _strUrlBtnContent = "Dell's Privacy Policy";
        private string _strPrivacyUrl = "https://www.dell.com/learn/us/en/uscorp1/policies-privacy-country-specific-privacy-policy";
        //private string _strCheckBtnText = "Help Dell improve its products and services automatically";


        public string strPrivacyUrl
        {
            get
            { return _strPrivacyUrl; }
            set
            {
                _strPrivacyUrl = value;
            }
        }

        private bool showLockMask = false;

        public bool ShowLockMask
        {
            get { return showLockMask; }
            set 
            { 
                showLockMask = value;
                LockMaskVisible = showLockMask ? Visibility.Visible : Visibility.Collapsed;
                NotifyPropertyChanged("ShowLockMask");
            }
        }

        private Visibility lockMaskVisible = Visibility.Collapsed;

        public Visibility LockMaskVisible
        {
            get { return lockMaskVisible; }
            set
            {
                lockMaskVisible = value;
                NotifyPropertyChanged("LockMaskVisible");
            }
        }

        private bool _isTabStoppable = true;

        public bool isTabStoppable
        {
            get { return _isTabStoppable; }
            set
            {
                _isTabStoppable = value;
                NotifyPropertyChanged("isTabStoppable");
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
                DDPMSettings data = DdpmCommonHelper.ReadDDPMSettings();//DeviceManagerSA.ReloadAppConfigData().Result;
                if (data == null)
                    return;
                if (data.UserSettings == null)
                    return;

                vm.ShowLockMask = data.LockSettings.Lock_Settings_TelemetryConsent;                
                vm.isTabStoppable = !data.LockSettings.Lock_Settings_TelemetryConsent;
                vm.isConsentChecked = data.UserSettings.isTelemetryConsentOn;

                DdpmCommonHelper.DeviceManagerSA.ITSettingsActionEvent += DeviceManagerSA_ITSettingsActionEvent;
                DdpmCommonHelper.DeviceManagerSA.UIUpdateNotify += DeviceManagerSA_UIUpdateNotifyEvent;
            }
            catch (Exception)
            {

            }
        }

        ~AnalyticsPage() 
        {
            if (DdpmCommonHelper.DeviceManagerSA != null)
            {
                DdpmCommonHelper.DeviceManagerSA.ITSettingsActionEvent -= DeviceManagerSA_ITSettingsActionEvent;
                DdpmCommonHelper.DeviceManagerSA.UIUpdateNotify -= DeviceManagerSA_UIUpdateNotifyEvent;
            }
        }

        private void DeviceManagerSA_UIUpdateNotifyEvent(object? sender, SA.Common.UpdateUINotify e)
        {
            if (e == null || string.IsNullOrEmpty(e.UI_Field_Name))
            {
                Trace.WriteLine("Got [DeviceManagerSA_UIUpdateNotifyEvent] event but its argument is empty!");
                return;
            }
            //Catch event if belong to telemetry consent
            if(e.UI_Field_Name.ToUpper().Trim().Equals("TELEMETRYCONSENT"))
            {
                DDPMSettings data = DdpmCommonHelper.ReadDDPMSettings(true);//DeviceManagerSA.ReloadAppConfigData().Result;
                Dispatcher.Invoke(new Action(() =>
                {
                    AnalyticsViewModel vm = (AnalyticsViewModel)this.DataContext;
                    if (vm != null)
                    {
                        vm.isConsentChecked = data.UserSettings.isTelemetryConsentOn;                        
                        Trace.WriteLine($"Apply TelemetryConsent(check) : {data.UserSettings.isTelemetryConsentOn}");
                    }
                }));
            }
        }

        private void DeviceManagerSA_ITSettingsActionEvent(object? sender, SA.Common.ITSettingEventArgs e)
        {
            bool? isLocked = DdpmCommonHelper.GetUINotifyPropertyValue_Boolean("Lock_Settings_TelemetryConsent", e);
            if (isLocked != null)
            {
                Dispatcher.Invoke(new Action(() =>
                {
                    AnalyticsViewModel vm = (AnalyticsViewModel)this.DataContext;
                    if (vm != null)
                    {
                        vm.isTabStoppable = !(bool)isLocked;
                        vm.ShowLockMask = (bool)isLocked;
                        Trace.WriteLine($"Apply TelemetryConsent(Lock) : {isLocked}");
                    }
                }));
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