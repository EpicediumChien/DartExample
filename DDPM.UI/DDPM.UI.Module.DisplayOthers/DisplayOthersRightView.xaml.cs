using DDPM.SA.Common.Settings;
using DDPM.UI.Common;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace DDPM.UI.Module.DisplayOthers
{
    /// <summary>
    /// Interaction logic for DisplayOthersRightView.xaml
    /// </summary>
    public partial class DisplayOthersRightView : UserControl
    {
        private DisplayOthersViewModel vm
        {
            get => (DisplayOthersViewModel)DataContext;
        }

        public DisplayOthersRightView(DisplayOthersViewModel vm)
        {
            InitializeComponent();
            DataContext = vm;

            if (DdpmCommonHelper.DeviceManagerSA != null)
            {
                DdpmCommonHelper.DeviceManagerSA.ITSettingsActionEvent += DeviceManagerSA_ITSettingsActionEvent;

                DDPMSettings data = DdpmCommonHelper.DeviceManagerSA.ReloadAppConfigData().Result;
                if (data != null)
                {
                    if (data.LockSettings.Lock_Display_ExportSettings)
                    {
                        //Do lock ui init here (direct set or binding via vm)
                    }
                    if (data.LockSettings.Lock_Display_PowerNap)
                    {
                        //Do lock ui init here (direct set or binding via vm)
                    }
                }
            }
        }

        ~DisplayOthersRightView()
        {
            if (DdpmCommonHelper.DeviceManagerSA != null)
            {
                DdpmCommonHelper.DeviceManagerSA.ITSettingsActionEvent -= DeviceManagerSA_ITSettingsActionEvent;
            }
        }

        private void DeviceManagerSA_ITSettingsActionEvent(object? sender, SA.Common.ITSettingEventArgs e)
        {
            //Using user data from config file if need
            DDPMSettings data = null;            

            bool? isLocked = DdpmCommonHelper.GetUINotifyPropertyValue_Boolean("Lock_Display_ExportSettings", e);
            if (isLocked != null)
            {
                Dispatcher.Invoke(new Action(() =>
                {
                    DisplayOthersViewModel vm = (DisplayOthersViewModel)this.DataContext;
                    if (vm != null)
                    {
                        //vm.LockMaskVisible = (bool)isLocked ? Visibility.Visible : Visibility.Collapsed;
                        Trace.WriteLine($"[SettingsPage] Apply Import/Export(Lock) : {isLocked}");                        
                    }
                }));
            }
            isLocked = DdpmCommonHelper.GetUINotifyPropertyValue_Boolean("Lock_Display_PowerNap", e);
            if (isLocked != null)
            {
                Dispatcher.Invoke(new Action(() =>
                {
                    DisplayOthersViewModel vm = (DisplayOthersViewModel)this.DataContext;
                    if (vm != null)
                    {
                        //vm.LockMaskVisible = (bool)isLocked ? Visibility.Visible : Visibility.Collapsed;
                        Trace.WriteLine($"[SettingsPage] Apply PowerNap(Lock) : {isLocked}");
                    }
                }));
            }
            //DDPMW-1366 9/7
            //Functionality: When a 1 or more settings are locked, automatically lock 'export/import'. 
            if (DdpmCommonHelper.DeviceManagerSA != null && data == null)
            {
                data = DdpmCommonHelper.DeviceManagerSA.ReloadAppConfigData().Result;
                if(DdpmCommonHelper.GetUINotifyPropertyValue_isAnyLocked(data) == true && data.LockSettings.Lock_Display_ExportSettings == false)
                {
                    //vm.LockMaskVisible = (bool)isLocked ? Visibility.Visible : Visibility.Collapsed;
                    Trace.WriteLine($"[SettingsPage] Apply Import/Export(Lock) to lock due to 1 or more settings be locked");
                }
            }
        }

            private void tbOpenScreensaverSettings_Click(object sender, RoutedEventArgs e)
        {
            var psi = new System.Diagnostics.ProcessStartInfo();
            psi.FileName = Environment.SystemDirectory + Path.DirectorySeparatorChar + @"rundll32.exe";
            psi.Arguments = @"shell32.dll,Control_RunDLL desk.cpl,,1";
            psi.UseShellExecute = true;

            System.Diagnostics.Process.Start(psi);
        }

        private void import_Click(object sender, RoutedEventArgs e)
        {
            vm.ImportSettings();
        }

        private void export_Click(object sender, RoutedEventArgs e)
        {
            vm.ExportSettings();
        }
    }
}