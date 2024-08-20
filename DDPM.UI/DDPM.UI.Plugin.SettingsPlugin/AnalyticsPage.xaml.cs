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

        //public string strCheckBtnText
        //{
        //    get
        //    { return _strCheckBtnText; }
        //    set
        //    {
        //        _strCheckBtnText = value;
        //        NotifyPropertyChanged("strCheckBtnText");
        //    }
        //}

        //public string strUrlBtnContent
        //{
        //    get
        //    { return _strUrlBtnContent; }
        //    set
        //    {
        //        _strUrlBtnContent = value;
        //        NotifyPropertyChanged("strUrlBtnContent");
        //    }
        //}

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

        //public string strTitle
        //{
        //    get
        //    { return _strTitle; }
        //    set
        //    {
        //        _strTitle = value;
        //        NotifyPropertyChanged("strTitle");
        //    }
        //}

        //public string strContent
        //{
        //    get
        //    { return _strContent; }
        //    set
        //    {
        //        _strContent = value;
        //        NotifyPropertyChanged("strContent");
        //    }
        //}

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

                vm.isCheckEnable = !data.LockSettings.Lock_TelemetryConsent;
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
                DDPMSettings data = DdpmCommonHelper.DeviceManagerSA.ReloadAppConfigData().Result;
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
            if (e == null || e.IT_Feature_TriggerList == null || e.target_object == null)
            {
                Trace.WriteLine("Got [DeviceManagerSA_ITSettingsActionEvent] event but its argument is empty!");
                return;
            }
            int idx = e.IT_Feature_TriggerList.FindIndex(x => x.Trim().Equals("Lock_TelemetryConsent"));
            if(idx >= 0)
            {
                string feature = e.IT_Feature_TriggerList[idx];
                PropertyInfo propertyInfo = e.target_object.GetType().GetProperty(feature);
                Trace.WriteLine($"Got [IT settings event] {feature} : {propertyInfo.GetValue(e.target_object)}");
                //DDPMSettings data = DdpmCommonHelper.DeviceManagerSA.ReloadAppConfigData().Result;
                Dispatcher.Invoke(new Action(() =>
                {
                    AnalyticsViewModel vm = (AnalyticsViewModel)this.DataContext;
                    if (vm != null)
                    {
                        vm.isCheckEnable = !(bool)propertyInfo.GetValue(e.target_object);
                        Trace.WriteLine($"Apply TelemetryConsent(Lock) : {propertyInfo.GetValue(e.target_object)}");
                        //vm.isConsentChecked = data.UserSettings.isTelemetryConsentOn;
                        //Trace.WriteLine($"Apply TelemetryConsent(check) : {data.UserSettings.isTelemetryConsentOn}");
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