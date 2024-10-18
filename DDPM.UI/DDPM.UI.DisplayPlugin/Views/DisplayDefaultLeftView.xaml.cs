using DDPM.SA.Common.Settings;
using DDPM.UI.Common;
using DDPM.UI.Module.InputSource;
using DDPM.UI.Plugin.Common;
using Dell.Client.Framework.UX.WPF;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace DDPM.UI.Plugin.DisplayPlugin.Views
{
    /// <summary>
    /// Interaction logic for DisplayDefaultLeftView.xaml
    /// </summary>
    public partial class DisplayDefaultLeftView : UserControl
    {
        private readonly string Restore = Strings.RestoreToDefault;// "Restore to default";

        public DisplayDefaultLeftView()
        {
            InitializeComponent();
            txtRestore.Text = Restore;

            if (DdpmCommonHelper.DeviceManagerSA != null)
            {
                DdpmCommonHelper.DeviceManagerSA.ITSettingsActionEvent += DeviceManagerSA_ITSettingsActionEvent;

                DDPMSettings data = DdpmCommonHelper.ReadDDPMSettings();// DeviceManagerSA.ReloadAppConfigData().Result;
                if (data != null)
                {
                    if (data.LockSettings.Lock_Setting_RestoreDefaults)
                    {
                        RestoreLockIcon.Visibility = Visibility.Visible;
                        txtRestore.IsEnabled = false;
                    }
                    else
                    {
                        txtRestore.IsEnabled = !data.LockSettings.Lock_Display_RestoreFactoryDefaults;
                        RestoreLockIcon.Visibility = data.LockSettings.Lock_Display_RestoreFactoryDefaults ? Visibility.Visible : Visibility.Collapsed;

                        //Lock Functionality 9/7
                        //When a 1 or more settings are locked, automatically lock 'Restore to default'/'factory reset' control [Display]
                        if (DdpmCommonHelper.GetUINotifyPropertyValue_isAnyLocked(data, "Lock_Display"))
                        {
                            RestoreLockIcon.Visibility = Visibility.Visible;
                            txtRestore.IsEnabled = false;
                        }
                    }
                }
            }
        }

        ~DisplayDefaultLeftView()
        {
            if (DdpmCommonHelper.DeviceManagerSA != null)
            {
                DdpmCommonHelper.DeviceManagerSA.ITSettingsActionEvent -= DeviceManagerSA_ITSettingsActionEvent;
            }
        }

        private void DeviceManagerSA_ITSettingsActionEvent(object? sender, SA.Common.ITSettingEventArgs e)
        {
            var rst = DdpmCommonHelper.ApplyRestoreFactoryDefaultsEventData(e, "Lock_Display_RestoreFactoryDefaults");
            Dispatcher.Invoke(new Action(() =>
            {
                RestoreLockIcon.Visibility = rst.isLocked;
                txtRestore.IsEnabled = rst.isEnabled;

                //Lock Functionality 9/7
                //When a 1 or more settings are locked, automatically lock 'Restore to default'/'factory reset' control [Display]
                DDPMSettings data = DdpmCommonHelper.ReadDDPMSettings(true);// DeviceManagerSA.ReloadAppConfigData().Result;
                if (data != null && data.LockSettings != null)
                {
                    if (DdpmCommonHelper.GetUINotifyPropertyValue_isAnyLocked(data, "Lock_Display"))
                    {
                        RestoreLockIcon.Visibility = Visibility.Visible;
                        txtRestore.IsEnabled = false;
                    }
                }
            }));            
        }

        // 20240617  jim add
        private void Restore_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (txtRestore.IsEnabled == false)
                return;

            MessageModalDialog dlg = new MessageModalDialog("Restore to default", "Do you want to reset your monitor to factory settings now?", "Yes", "No");
            Window parentWindow = Window.GetWindow(this);
            if (parentWindow != null)
            {
                dlg.Owner = parentWindow;
            }
            bool? dialogResult = dlg.ShowDialog();
           
            if ((dialogResult == true) && (DdpmCommonHelper.DeviceManagerSA != null) &&
                (DdpmCommonHelper.ModuleOwner != null) && (DdpmCommonHelper.ModuleOwner.SelectedHomeDevice != null))
            {
                bool r;

                // 20240627 jim modify
                r = DdpmCommonHelper.DeviceManagerSA.SetVCPCapability(DdpmCommonHelper.ModuleOwner.SelectedHomeDevice.MonitorInfo, 0x04, 1).Result;

                // 20240627 jim add
                //Return to DdpmHomePage
                IConsole? console = DisplayPlugin.PluginIoc.GetService<IConsole>();
                console?.ShowPluginById(DDPM.UI.Common.Constants.DdpmHomePluginId);

                //MessageBox.Show("OK button was clicked");
            }

            /*
            RestoreModalDialog restoreModalDialog = new();
            Window parentWindow = Window.GetWindow(this);
            if (parentWindow != null)
            {
                restoreModalDialog.Owner = parentWindow;
            }

            bool? dialogResult = restoreModalDialog.ShowDialog();
            if ((dialogResult == true) && (DdpmCommonHelper.DeviceManagerSA != null) &&
                (DdpmCommonHelper.ModuleOwner != null) && (DdpmCommonHelper.ModuleOwner.SelectedHomeDevice != null))
            {
                bool r;

                // 20240627 jim modify
                r = DdpmCommonHelper.DeviceManagerSA.SetVCPCapability(DdpmCommonHelper.ModuleOwner.SelectedHomeDevice.MonitorInfo, 0x04, 1).Result;

                // 20240627 jim add
                //Return to DdpmHomePage
                IConsole? console = DisplayPlugin.PluginIoc.GetService<IConsole>();
                console?.ShowPluginById(DDPM.UI.Common.Constants.DdpmHomePluginId);

                //MessageBox.Show("OK button was clicked");
            }
            */
        }
    }
}