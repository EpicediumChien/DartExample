using DDPM.SA.Common;
using DDPM.UI.Common;
using DDPM.UI.Common.Interfaces;
using Dell.Client.Framework.UX.WPF.Controls;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using UserControl = System.Windows.Controls.UserControl;
using Newtonsoft.Json;
using VcpCore.Common;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using static DDPM.SA.Common.IPlugin;
using Dell.Client.Framework.Common;
using Microsoft.Win32;
using Windows.Data.Json;
using Windows.System;
using System.Windows.Threading;
using System.Runtime.InteropServices;
using RegistryUtils;
using System.Windows.Media.Animation;
using DDPM.SA.Common.Settings;
using System.Management;
using static DDPM.UI.Module.Color.ColorViewModel;
using WinRT;
using MonitorProfile = DDPM.SA.Common.MonitorProfile;
using static System.Windows.Forms.LinkLabel;

namespace DDPM.UI.Module.Color
{

    /// <summary>
    /// Interaction logic for ColorRightView.xaml
    /// </summary>
    public partial class ColorRightView : UserControl
    {
        //  Jim remove 20240604
        //private List<string> _Support_DeviceName  = new List<string> { "U4021QW", "U2723QE", "U3223QE", "U3223QZ", "U3423WE", "U3824DW", "U4924DW", "U3224KB", "U2724D", "U2724DE", "U3425WE", "U4025QW" , "UP2720Q" , "UP3221Q" };
                  

        public ColorRightView()
        {
            InitializeComponent();        

        }

        //  Jim remove 20240604
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
           ColorViewModel vm = (ColorViewModel)DataContext;

            vm.WatchForProcessStart();
            vm.WatchForProcessEnd();

        }        

        //  Jim add 20240606
        private void UserControl_UnLoaded(object sender, RoutedEventArgs e)
        {
            DdpmCommonHelper.DeviceManagerSA.AutoSetColorPresetForMonitorConfig((DdpmCommonHelper.ModuleOwner.SelectedHomeDevice.MonitorInfo.Index).ToString(), "off");
            
            //Jim remove 20240621
            //Thread.Sleep(200);

            ColorViewModel vm = (ColorViewModel)DataContext;
            
            vm.WatchForProcessStart_Stop();
            vm.WatchForProcessEnd_Stop();

            vm.StopRegistryMonitor();
        }   

        void RefreshUI()
        {
            ColorViewModel vm = (ColorViewModel)DataContext;
            vm.RefreshUI();
               
        }       

        public int get_index_of_json_config_for_cur_monitor(MonitorInfo mo)
        {     
            int index = -1;

            if (Test_AddAppCollectionData.GetInstance()._monitorConfigs != null)
            {
                if (Test_AddAppCollectionData.GetInstance()._monitorConfigs.Count > 0)
                {

                    index = Test_AddAppCollectionData.GetInstance()._monitorConfigs.FindIndex(x =>
                                                    x.DeviceInfo.ModelName.Trim() == mo.edid.ModelName.Trim() &&
                                                    x.DeviceInfo.SerialNumber.Trim() == mo.edid.SerialNumber.Trim());

                }
            }
            return index;
        }

        private void appPreset_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ColorViewModel vm = (ColorViewModel)DataContext;

            AppData selected_app;
            System.Windows.Controls.ComboBox cb = sender as System.Windows.Controls.ComboBox;
          
            if (cb != null)
            {
                object item = cb.DataContext;

                if (item != null)
                {
                    if (Expander_Auto.IsExpanded == true)
                    {
                        int index = this.lb_AppList.Items.IndexOf(item);
                        selected_app = (AppData)lb_AppList.Items[index];

                        // jim add 20240627
                        Test_AddAppCollectionData.GetInstance()._monitorConfigs = DdpmCommonHelper.DeviceManagerSA.ReadColorPresetSettings().Result;

                        int index_config = get_index_of_json_config_for_cur_monitor(DdpmCommonHelper.ModuleOwner.SelectedHomeDevice.MonitorInfo);
                        if (index_config >= 0)
                        {
                            Test_AddAppCollectionData.GetInstance()._monitorConfigs[index_config].RunType = (int)ColorPresetRunType.Auto;
                            Test_AddAppCollectionData.GetInstance()._monitorConfigs[index_config].AppInfo[selected_app.AppName].ColorPresetName = vm.SupportColorPresets[cb.SelectedIndex];


                            DdpmCommonHelper.DeviceManagerSA.WriteColorPresetSettings(Test_AddAppCollectionData.GetInstance()._monitorConfigs);
                            Thread.Sleep(100);

                            // jim add 20240627
                            DdpmCommonHelper.DeviceManagerSA.Notify_refresh_app_list();

                            // jim add 20240806
                            Thread.Sleep(100);
                        }
               
                    }
                }
            }
        }

        private void appDeleteBtn_Click(object sender, RoutedEventArgs e)
        {
            ColorViewModel vm = (ColorViewModel)DataContext;

            AppData selected_app;
            System.Windows.Controls.Button btn = (System.Windows.Controls.Button)sender;
            if (btn != null)
            {
                object item = btn.DataContext;

                if (item != null)
                {
                    if (Expander_Auto.IsExpanded == true)
                    {
                        int index = this.lb_AppList.Items.IndexOf(item);
                        selected_app = (AppData)lb_AppList.Items[index];
                        int index_config = get_index_of_json_config_for_cur_monitor(DdpmCommonHelper.ModuleOwner.SelectedHomeDevice.MonitorInfo);
                        if (index_config >= 0)
                        {
                            Test_AddAppCollectionData.GetInstance()._monitorConfigs[index_config].RunType = (int)ColorPresetRunType.Auto;
                            Test_AddAppCollectionData.GetInstance()._monitorConfigs[index_config].AppInfo.Remove(selected_app.AppName);

                            DdpmCommonHelper.DeviceManagerSA.WriteColorPresetSettings(Test_AddAppCollectionData.GetInstance()._monitorConfigs);
                            Thread.Sleep(100);

                        }               

                        Test_AddAppCollectionData.GetInstance().AppsList.Remove(selected_app);                  

                    }
                }
            }
        }

        private void btn_AddApp_Click(object sender, RoutedEventArgs e)
        {
            ColorViewModel vm = (ColorViewModel)DataContext;

            AddAppFullPageCtrl _addAppFullPage = new AddAppFullPageCtrl();
            _addAppFullPage.DataContext = vm;
            DdpmCommonHelper.ModuleOwner?.OpenFullView(_addAppFullPage);
            //m.ModuleOwner?.OpenFullView(_addAppFullPage);     
        }

        private void nightlight_config_Click(object sender, RoutedEventArgs e)
        {            
            string keyName = string.Format("{0}\\{1}", "HKEY_CURRENT_USER", "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\CloudStore\\Store\\DefaultAccount\\Current\\default$windows.data.bluelightreduction.bluelightreductionstate\\windows.data.bluelightreduction.bluelightreductionstate");

            ColorViewModel vm = (ColorViewModel)DataContext;


            if (vm.registryMonitor_NightLight == null)
            {
                vm.registryMonitor_NightLight = new RegistryUtils.RegistryMonitor_NightLight(keyName);
                vm.registryMonitor_NightLight.RegChanged += new EventHandler(vm.OnRegChanged_NightLight);
                vm.registryMonitor_NightLight.Error += new System.IO.ErrorEventHandler(vm.OnError_NightLight);
                vm.registryMonitor_NightLight.Start();
            }
            

            var psi = new System.Diagnostics.ProcessStartInfo();

            psi.FileName = "ms-settings:nightlight";
            psi.UseShellExecute = true;

            System.Diagnostics.Process.Start(psi);     
        }

        private void ICC_profile_config_Click(object sender, RoutedEventArgs e)
        {   
            var psi = new System.Diagnostics.ProcessStartInfo();

            psi.FileName = "ms-settings:display";
            psi.UseShellExecute = true;

            System.Diagnostics.Process.Start(psi);
        }        

        private void expanderHasExpanded(object sender, RoutedEventArgs args)
        {
            Expander expander_sender = (Expander)sender;            

            if (expander_sender.Name == "Expander_Manual" )
            {
                Expander_Auto.IsExpanded = false;

                this.Dispatcher.Invoke((Action)(() =>
                {
                    //ColorViewModel vm = (ColorViewModel)DataContext;
                    DdpmCommonHelper.DeviceManagerSA.AutoSetColorPresetForMonitorConfig((DdpmCommonHelper.ModuleOwner.SelectedHomeDevice.MonitorInfo.Index).ToString(), "off");
                    //DdpmCommonHelper.DeviceManagerSA.AutoSetColorPresetForMonitorConfig((vm.MyModule.SelectedHomeDevice.MonitorInfo.Index).ToString(), "off");


                }));
             
            }
            else if (expander_sender.Name == "Expander_Auto")
            {
                Expander_Manual.IsExpanded = false;

                this.Dispatcher.Invoke((Action)(() =>
                {
                    //ColorViewModel vm = (ColorViewModel)DataContext;
                    DdpmCommonHelper.DeviceManagerSA.AutoSetColorPresetForMonitorConfig((DdpmCommonHelper.ModuleOwner.SelectedHomeDevice.MonitorInfo.Index).ToString(), "on");
					//DdpmCommonHelper.DeviceManagerSA.AutoSetColorPresetForMonitorConfig((vm.MyModule.SelectedHomeDevice.MonitorInfo.Index).ToString(), "on");

                }));             
            
            }
          
        }

        private void Color_Management_Switch_Click(object sender, RoutedEventArgs e)
        {
            ColorViewModel vm = (ColorViewModel)DataContext;
            if ((bool)ColorManagement_ToggleSwitch.IsChecked)
            {
                rb_ICCprofile_based_Colorpreset.IsEnabled = true;
                rb_Colorpreset_based_ICCprofile.IsEnabled = true;
                //Dean add for ALS
                if (vm != null)
                {
                    vm.ColorManagement_isChecked = true;
                    vm.ICCprofile_based_Colorpreset_enable = true;
                }
            }
            else
            {
                rb_ICCprofile_based_Colorpreset.IsEnabled = false;
                rb_Colorpreset_based_ICCprofile.IsEnabled = false;
                //Dean Add For ALS
                if (vm != null)
                {
                    vm.ColorManagement_isChecked = false;
                    vm.ICCprofile_based_Colorpreset_enable = false;
                }
            }            
        }

        private void rb_ICCprofile_based_Colorpreset_click(object sender, RoutedEventArgs e)
        {
            if ((bool)rb_Colorpreset_based_ICCprofile.IsChecked)
            {
                rb_Colorpreset_based_ICCprofile.IsChecked = false;

                ColorViewModel vm = (ColorViewModel)DataContext;

                if (vm._ICC_Metadata.Is_Support_ICC_DeviceName)
                {
                    if (vm.registryMonitor_ICC != null)
                    {
                        if (vm.registryMonitor_ICC.IsMonitoring)
                            vm.registryMonitor_ICC.Dispose();
                        vm.registryMonitor_ICC = null;
                    }

                    int count = vm._ICC_Metadata._support_ICC_DeviceName[DdpmCommonHelper.ModuleOwner.SelectedHomeDevice.MonitorInfo.modelName].Count;

                    for (int i = 0; i < count; i++)
                    {
                        RegistryUtils.MonitorProfile.IntsallMonitorProfile(vm._ICC_Metadata.strICC_Folder + vm._ICC_Metadata._support_ICC_DeviceName[DdpmCommonHelper.ModuleOwner.SelectedHomeDevice.MonitorInfo.modelName][i].File);
                    }
                }
            }
           
        }

        private void rb_Colorpreset_based_ICCprofile_click(object sender, RoutedEventArgs e)
        {
            ColorViewModel vm = (ColorViewModel)DataContext;

            if ((bool)rb_ICCprofile_based_Colorpreset.IsChecked)
            {
                rb_ICCprofile_based_Colorpreset.IsChecked = false;
                //Dean 0612 add
                if (vm != null)
                    vm.ICCprofile_based_Colorpreset_enable = false;
            }

            //ColorViewModel vm = (ColorViewModel)DataContext;

            //Robert_Lin, 2024-6-26, fix SAST issue: [Bug] 'vm' is null on at least one execution path.
            //OLD Code:
            //  if (vm.Is_Support_ICC_DeviceName)
            //NEW Code:
            if ((vm!=null) && (vm._ICC_Metadata.Is_Support_ICC_DeviceName))
            {
                if (vm._ICC_Metadata._support_ICC_DeviceName[DdpmCommonHelper.ModuleOwner.SelectedHomeDevice.MonitorInfo.modelName] != null)
                {

                    int count = vm._ICC_Metadata._support_ICC_DeviceName[DdpmCommonHelper.ModuleOwner.SelectedHomeDevice.MonitorInfo.modelName].Count;

                    for (int i = 0; i < count; i++)
                    {
                        RegistryUtils.MonitorProfile.IntsallMonitorProfile(vm._ICC_Metadata.strICC_Folder + vm._ICC_Metadata._support_ICC_DeviceName[DdpmCommonHelper.ModuleOwner.SelectedHomeDevice.MonitorInfo.modelName][i].File);
                    }

                    string keyName = string.Format("{0}\\{1}", "HKEY_CURRENT_USER", @"Software\Microsoft\Windows NT\CurrentVersion\ICM\ProfileAssociations\Display\{4d36e96e-e325-11ce-bfc1-08002be10318}");

                    vm.registryMonitor_ICC = new RegistryUtils.RegistryMonitor_ICC(keyName);
                    vm.registryMonitor_ICC.RegChanged += new EventHandler(vm.OnRegChanged_ICC);
                    vm.registryMonitor_ICC.Error += new System.IO.ErrorEventHandler(vm.OnError_ICC);
                    vm.registryMonitor_ICC.Start();
                }
            }

        }

        private void lb_AppList_PreviewDragEnter(object sender, System.Windows.DragEventArgs e)
        {
            System.Windows.DataObject dataObj = (System.Windows.DataObject)e.Data;
            if (!dataObj.ContainsFileDropList())
            {
                //pathName = "Dragging object is not file list.";
                //return false;
            }
            string[] dropFileNames = (string[])dataObj.GetData(System.Windows.DataFormats.FileDrop);
            if (dropFileNames == null)
            {
                //pathName = "Dragging object is a file list, but it\'s empty.";
                //return false;
            }
            if (dropFileNames.Length != 1)
            {
                //pathName = "Dragging object is not \"single\" file.";
                //return false;
            }
            string extName = System.IO.Path.GetExtension(dropFileNames[0]);
            if (!extName.Equals(".CSV", StringComparison.OrdinalIgnoreCase))
            {
                //pathName = "Dragging object is not a \".CSV\" file.";
                //return false;
            }
            //pathName = dropFileNames[0];

            string targetPath = dropFileNames[0];

           

            if (targetPath.EndsWith(".lnk"))
            {
                //ShellLinkObject linkedLnk = (ShellLinkObject)shell.NameSpace(targetPath).Items().Item().GetLink;
                //targetPath = linkedLnk.Target.Path;
                //targetPath = linkedLnk.Target.Path;

                // IWshRuntimeLibrary is in the COM library "Windows Script Host Object Model"
                IWshRuntimeLibrary.WshShell shell = new IWshRuntimeLibrary.WshShell();

                IWshRuntimeLibrary.IWshShortcut shortcut = (IWshRuntimeLibrary.IWshShortcut)shell.CreateShortcut(targetPath);

                string temp;
                
                temp = shortcut.TargetPath;
            }

        }


    }
}
