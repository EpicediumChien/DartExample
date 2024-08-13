using DDPM.SA.Common;
using DDPM.SA.Common.Settings;
using DDPM.UI.Common;
using Dell.Client.Framework.UX.WPF.Controls;
using System.Diagnostics;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using VcpCore.Common;

namespace DDPM.UI.Module.Brightness
{
    /// <summary>
    /// Interaction logic for BrightnessRightView.xaml
    /// </summary>
    public partial class BrightnessRightView : UserControl
    {
        internal BrightnessViewModel? vm { get; set; }

        public BrightnessRightView()
        {
            InitializeComponent();

            //vm = BrightnessViewModel.GetInstance();
        }


        private void Reset_Click(object sender, RoutedEventArgs e)
        {
            BrightnessViewModel x = (BrightnessViewModel)DataContext;
            bool r = DdpmCommonHelper.DeviceManagerSA.SetVCPCapability(x.SelectedHomeDevice.MonitorInfo, 0x05, 1).Result;
            if (r) {

                ObjGetVCP rb_10 = DdpmCommonHelper.DeviceManagerSA.GetVCPCapability(x.SelectedHomeDevice.MonitorInfo, 0x10, 0).Result;
                if (rb_10.result)
                {
                    B_slider.Value = (uint)((long)rb_10.value);
                    LuminanceSlider.Value = (uint)((long)rb_10.value);
                }
                ObjGetVCP rb_12 = DdpmCommonHelper.DeviceManagerSA.GetVCPCapability(x.SelectedHomeDevice.MonitorInfo, 0x12, 0).Result;
                if (rb_12.result)
                    C_slider.Value = (uint)((long)rb_12.value);
            }
        }

        private void SynchronizeSwitch_Click(object sender, RoutedEventArgs e)
        {
            BrightnessViewModel x = (BrightnessViewModel)DataContext;

            DDPMSettings setting = DdpmCommonHelper.DeviceManagerSA.ReloadAppConfigData().Result;

            if ((bool)SynchronizeSwitch.IsChecked)
            {
                x.IsSynchronize = true;
                SynchronizeSwitch.Content = "ON";
            }
            else
            {
                x.IsSynchronize = false;
                SynchronizeSwitch.Content = "OFF";
            }

            setting.UserSettings.IsSynchronizemonitor = x.IsSynchronize;
            DdpmCommonHelper.DeviceManagerSA.SetAppConfigData(setting);
        }

        private void Expander_Manual_Expanded(object sender, RoutedEventArgs e)
        {
            Expander_Manual_Luminance.IsExpanded = false;
            Expander_Auto.IsExpanded = false;
            Expander_Schedule.IsExpanded = false;
            BrightnessViewModel vm = (BrightnessViewModel)DataContext;
            if (vm != null) 
            { 
                bool autoBrightnessStatus = vm.AutoBrightnessStatus;
                if (autoBrightnessStatus)
                {
                    //disable hotkey btn
                    btnManualBrightnessContrast.IsEnabled = false;
                }
                else
                {
                    btnManualBrightnessContrast.IsEnabled = true;
                }
            }
            /*Trace.WriteLine($"1. {DateTime.Now.ToString("MM/dd/yyyy hh:mm ss fff")}");
            BrightnessViewModel vm = (BrightnessViewModel)DataContext;
            Trace.WriteLine($"2. {DateTime.Now.ToString("MM/dd/yyyy hh:mm ss fff")}");
            if (vm != null)
            {
                vm.UpdateBrightnessContrast();                           
            }
            Trace.WriteLine($"3. {DateTime.Now.ToString("MM/dd/yyyy hh:mm ss fff")}");*/
            List<ALSConfig> alsSynchronizeList = DdpmCommonHelper.DeviceManagerSA.GetAllExistAlsConfig().Result;
            vm.CheckisShowSynchronize(alsSynchronizeList);
        }
        
        private void Expander_Manual_Expanded_Luminance(object sender, RoutedEventArgs e)
        {
            Expander_Auto.IsExpanded = false;
            Expander_Schedule.IsExpanded = false;
            Expander_Manual.IsExpanded = false;

            BrightnessViewModel vm = (BrightnessViewModel)DataContext;
            if (vm != null)
            {
                vm.UpdateLuminance();
            }
        }

        private void Expander_Auto_Expanded(object sender, RoutedEventArgs e)
        {
            Expander_Manual.IsExpanded = false;
            Expander_Schedule.IsExpanded = false;
            Expander_Manual_Luminance.IsExpanded = false;
        }

        private void Expander_Schedule_Expanded(object sender, RoutedEventArgs e)
        {
            Expander_Auto.IsExpanded = false;
            Expander_Manual.IsExpanded = false;
            Expander_Manual_Luminance.IsExpanded = false;
        }

        private void Hotkey_Click(object sender, RoutedEventArgs e)
        {
            DisplayHotkeyFullView displayHotkeyFullView = new DisplayHotkeyFullView();
            displayHotkeyFullView.DataContext = (BrightnessViewModel)DataContext;
            DdpmCommonHelper.ModuleOwner?.OpenFullView(displayHotkeyFullView);
        }

        private void Hyperlink_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
