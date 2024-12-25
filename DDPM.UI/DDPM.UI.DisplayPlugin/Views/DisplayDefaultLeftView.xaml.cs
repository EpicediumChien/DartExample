using DDPM.SA.Common.Settings;
using DDPM.UI.Common;
using DDPM.UI.Common.ViewModels;
using DDPM.UI.Module.InputSource;
using DDPM.UI.Plugin.Common;
using DDPM.UI.Plugin.DisplayPlugin.ViewModels;
using Dell.Client.Framework.Common;
using Dell.Client.Framework.UX.WPF;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace DDPM.UI.Plugin.DisplayPlugin.Views
{
    /// <summary>
    /// Interaction logic for DisplayDefaultLeftView.xaml
    /// </summary>
    public partial class DisplayDefaultLeftView : UserControl
    {
        private readonly string Restore = Strings.RestoreToDefault;// "Restore to default";'

        private DisplayPageViewModel viewModel;

        //private ILog? _log;

        public DisplayDefaultLeftView()
        {
            InitializeComponent();
            txtRestore.Text = Restore;
            //if (DdpmCommonHelper.MyConsole != null)
            //{
            //    _log = DdpmCommonHelper.MyConsole.CreateLog("DisplayDefaultLeftView");
            //    _log.Info("DisplayDefaultLeftView ctor");
            //}

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

            //Robert_Lin, 2024-12-8, force the Restore to default button be locked, for debugging and LightMode design
            if (DDPM.UI.Common.User32.IniReadInt("DDPMDebug", "DisplayDefaultLeftView.RestoreToDefaultButton.IsLocked", 0, @"C:\temp\DDPMDebug.txt") == 1)
            {
                RestoreLockIcon.Visibility = Visibility.Visible;
                txtRestore.IsEnabled = false;
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
        private void Restore_Click(object sender, RoutedEventArgs e)
        {
            if (txtRestore.IsEnabled == false)
                return;

            MessageModalDialog dlg = new MessageModalDialog(Strings.RestoreToDefault, Strings.DisplayDefault0, Strings.No, Strings.Yes);
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

        public void CollapseRestoreToDefaultbtn()
        {
            btnRestore.Visibility = Visibility.Collapsed;
        }

        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            //_log?.Info($"this.ActualWidth = {this.ActualWidth}");
            //DDPM.UI.Common.Models.HomeDevice deviceBasePageViewModel = (DDPM.UI.Common.Models.HomeDevice)this.DataContext;
            //_log?.Info($"LandingMarketName = {deviceBasePageViewModel.LandingMarketName}");

            //BasePage: vBar.ActualWidth = 224
            //devImg.Width = this.ActualWidth - 224;
        }
    }
}