using DDPM.SA.Common;
using DDPM.SA.Common.Settings;
using DDPM.UI.Common;
using System.Windows;
using System.Windows.Controls;
using VcpCore.Common;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using UserControl = System.Windows.Controls.UserControl;
using System.IO;
using Dell.Client.Framework.UX.WPF.Controls;
using System.Reflection;
using System.Diagnostics;
using System.Windows.Forms;
using System.Windows.Input;
using Window = System.Windows.Window;

namespace DDPM.UI.Module.Color
{
    /// <summary>
    /// Interaction logic for ColorRightView.xaml
    /// </summary>
    public partial class ColorRightView : UserControl
    {
        //  Jim remove 20240604
        //private List<string> _Support_DeviceName  = new List<string> { "U4021QW", "U2723QE", "U3223QE", "U3223QZ", "U3423WE", "U3824DW", "U4924DW", "U3224KB", "U2724D", "U2724DE", "U3425WE", "U4025QW" , "UP2720Q" , "UP3221Q" };

        //private static Log _log;


        public ColorRightView()
        {
            InitializeComponent();
        }

        //  Jim remove 20240604
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            //ColorViewModel vm = (ColorViewModel)DataContext;
            //vm.WatchForProcessStart();
            //vm.WatchForProcessEnd();

            //Lock/unlock
            if (DdpmCommonHelper.DeviceManagerSA == null)
                return;
            try
            {
                //DDPMSettings data = DdpmCommonHelper.ReadDDPMSettings();//DeviceManagerSA.ReloadAppConfigData().Result;
                //if (data == null)
                //    return;
                //if (data.LockSettings == null)
                //    return;

                /*vm.ShowLockMask = data.LockSettings.Lock_Display_ColorPreset;
                vm.isTabStoppable = !data.LockSettings.Lock_Display_ColorPreset;

                if (vm.ShowLockMask)
                    vm.TabNavigation = "None";
                else
                    vm.TabNavigation = "Cycle";

                vm.LockMaskVisible = vm.ShowLockMask ? Visibility.Visible : Visibility.Collapsed;*/
                //PerformLockUnlockUIAction(data.LockSettings.Lock_Display_ColorPreset, data.LockSettings.Lock_Display_AutoBriTemp);

                DdpmCommonHelper.DeviceManagerSA.ITSettingsActionEvent += DeviceManagerSA_ITSettingsActionEvent;
                //DdpmCommonHelper.DeviceManagerSA.UIUpdateNotify += DeviceManagerSA_UIUpdateNotifyEvent;
            }
            catch (Exception)
            {

            }
        }

        //If caller is not the same as UI main thread, please using Dispatcher to execute it
        private void PerformLockUnlockUIAction(bool isColorLocked, bool isAutoBriTempLocked)
        {
            try
            {
                ColorViewModel vm = (ColorViewModel)DataContext;
                if (vm != null)
                {
                    ALSConfig cfg = DdpmCommonHelper.DeviceManagerSA?.GetALSFeatureValue(
                        DdpmCommonHelper.ModuleOwner.SelectedHomeDevice.MonitorInfo,
                        ALSFeatureQueryType.All, 0).Result;
                    //Lock functionality:
                    //Locking InAppAutoBriTemp' with "Auto Color Temperature" as "on" should lock 
                    // 1) "Auto Color Temperature", 
                    // 2) Manual color controls, Auto color controls, Color management found under the "Color" Tab,
                    // 3) the conditions listed in the first 3 bullet points
                    if (cfg != null && cfg.isAutoColorTemp && isAutoBriTempLocked)
                    {
                        isColorLocked = true;
                    }

                    vm.ShowLockMask = isColorLocked;
                    vm.isTabStoppable = !isColorLocked;

                    if (vm.ShowLockMask)
                        vm.TabNavigation = "None";
                    else
                        vm.TabNavigation = ColorViewModel.DefaultTabNavigation;// "Cycle";

                    vm.LockMaskVisible = isColorLocked ? Visibility.Visible : Visibility.Collapsed;
                }
            }
            catch (Exception)
            {
            }
        }

        //  Jim add 20240606
        private void UserControl_UnLoaded(object sender, RoutedEventArgs e)
        {
            ColorViewModel vm = (ColorViewModel)DataContext;

            //DdpmCommonHelper.DeviceManagerSA.AutoSetColorPresetForMonitorConfig(DdpmCommonHelper.ModuleOwner.SelectedHomeDevice.MonitorInfo, "OFF" , vm.IsAutoColorPreset_Lock);

            //Jim remove 20240621
            //Thread.Sleep(200);           

            vm.WatchForProcessStart_Stop();
            vm.WatchForProcessEnd_Stop();

            DdpmCommonHelper.DeviceManagerSA.StopRegistryMonitor_NightLight();
            DdpmCommonHelper.DeviceManagerSA.StopRegistryMonitor_NightLightScheduler();
            DdpmCommonHelper.DeviceManagerSA.StopRegistryMonitor_ICC();
            //vm.StopRegistryMonitor();

            //Lock/unlock
            if (DdpmCommonHelper.DeviceManagerSA != null)
            {
                DdpmCommonHelper.DeviceManagerSA.ITSettingsActionEvent -= DeviceManagerSA_ITSettingsActionEvent;
            }
        }

        private void DeviceManagerSA_ITSettingsActionEvent(object? sender, SA.Common.ITSettingEventArgs e)
        {
            bool? isLockColor = DdpmCommonHelper.GetUINotifyPropertyValue_Boolean("Lock_Display_ColorPreset", e);
            bool? isLockAutoTemp = DdpmCommonHelper.GetUINotifyPropertyValue_Boolean("Lock_Display_AutoBriTemp", e);

            if (isLockColor != null || isLockAutoTemp != null)
            {
                DDPMSettings data = DdpmCommonHelper.DeviceManagerSA.ReloadAppConfigData().Result;
                if (data == null)
                    return;
                if (data.LockSettings == null)
                    return;

                bool isLock_Color = false;
                if (isLockColor.HasValue)
                {
                    isLock_Color = isLockColor.Value;
                }
                else
                {
                    isLock_Color = data.LockSettings.Lock_Display_ColorPreset;
                }
                bool isLock_AutoBriTemp = false;
                if (isLockAutoTemp.HasValue)
                {
                    isLock_AutoBriTemp = isLockAutoTemp.Value;
                }
                else
                {
                    isLock_AutoBriTemp = data.LockSettings.Lock_Display_AutoBriTemp;
                }

                Dispatcher.Invoke(new Action(() =>
                {
                    /*ColorViewModel vm = (ColorViewModel)this.DataContext;
                    if (vm != null)
                    {
                        vm.isTabStoppable = !(bool)isLocked;
                        vm.ShowLockMask = (bool)isLocked;

                        if (vm.ShowLockMask)
                            vm.TabNavigation = "None";
                        else
                            vm.TabNavigation = "Cycle";

                        vm.LockMaskVisible = (bool)isLocked ? Visibility.Visible : Visibility.Collapsed;
                        Trace.WriteLine($"[SettingsPage] Color right view(Lock) : {isLocked}");
                    }*/
                    PerformLockUnlockUIAction(isLock_Color, isLock_AutoBriTemp);
                }));
            }
        }

        private void RefreshUI()
        {
            ColorViewModel vm = (ColorViewModel)DataContext;
            vm.RefreshUI();
        }

        public int get_index_of_json_config_for_cur_monitor(MonitorInfo mo)
        {
            int index = -1;

            if (Test_AddAppCollectionData.GetInstance()._monitorConfigs != null)
            {
                // chech if ModelName and SerialNumber is null
                if (Test_AddAppCollectionData.GetInstance()._monitorConfigs.Count > 0)
                {
                    for (int i = 0; i < Test_AddAppCollectionData.GetInstance()._monitorConfigs.Count; i++)
                    {
                        if (String.IsNullOrEmpty(Test_AddAppCollectionData.GetInstance()._monitorConfigs[i].ModelName))
                            return -1;

                        if (String.IsNullOrEmpty(Test_AddAppCollectionData.GetInstance()._monitorConfigs[i].SerialNumber))
                            return -1;
                    }
                }

                index = Test_AddAppCollectionData.GetInstance()._monitorConfigs.FindIndex(x =>
                                                      x.ModelName.Trim() == mo.edid.ModelName.Trim() &&
                                                      x.SerialNumber.Trim() == mo.edid.SerialNumber.Trim());

                if (index == -1)
                {
                    // chech if ModelName and ServiceTag is null
                    if (Test_AddAppCollectionData.GetInstance()._monitorConfigs.Count > 0)
                    {
                        for (int i = 0; i < Test_AddAppCollectionData.GetInstance()._monitorConfigs.Count; i++)
                        {
                            if (String.IsNullOrEmpty(Test_AddAppCollectionData.GetInstance()._monitorConfigs[i].ModelName))
                                return -1;

                            if (String.IsNullOrEmpty(Test_AddAppCollectionData.GetInstance()._monitorConfigs[i].ServiceTag))
                                return -1;
                        }
                    }

                    index = Test_AddAppCollectionData.GetInstance()._monitorConfigs.FindIndex(x =>
                                               x.ModelName.Trim() == mo.edid.ModelName.Trim() &&
                                               x.ServiceTag.Trim() == mo.edid.ServiceTag.Trim());
                }

                //int index = Test_AddAppCollectionData.GetInstance()._monitorConfigs.FindIndex(x =>
                //x.ModelName.Trim() == mo.edid.ModelName.Trim() &&
                //x.SerialNumber.Trim() == mo.edid.SerialNumber.Trim());
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

                if (item != null &&
                    Expander_Auto.IsExpanded == true)
                {
                    int index = this.lb_AppList.Items.IndexOf(item);
                    selected_app = (AppData)lb_AppList.Items[index];

                    // jim add 20240627
                    Test_AddAppCollectionData.GetInstance()._monitorConfigs = DdpmCommonHelper.DeviceManagerSA.ReadColorPresetSettings().Result;

                    int index_config = get_index_of_json_config_for_cur_monitor(DdpmCommonHelper.ModuleOwner.SelectedHomeDevice.MonitorInfo);
                    if (index_config >= 0)
                    {
                        Test_AddAppCollectionData.GetInstance()._monitorConfigs[index_config].RunType = (int)ColorPresetRunType.Auto;
                        //Test_AddAppCollectionData.GetInstance()._monitorConfigs[index_config].AppInfo[selected_app.AppName].ColorPresetName = vm.SupportColorPresets[cb.SelectedIndex];

                        string colorPresetName = vm.SupportColorPresets[cb.SelectedIndex];

                        var nColorVCPCoreValue = DdpmCommonHelper.DeviceManagerSA.GetColorVCPCoreValue(colorPresetName).Result;

                        if (vm.SmartHDR_ON)
                            Test_AddAppCollectionData.GetInstance()._monitorConfigs[index_config].AppInfo[selected_app.AppName].HDRColor = nColorVCPCoreValue;
                        else
                            Test_AddAppCollectionData.GetInstance()._monitorConfigs[index_config].AppInfo[selected_app.AppName].Color = nColorVCPCoreValue;

                        DdpmCommonHelper.DeviceManagerSA.WriteColorPresetSettings(Test_AddAppCollectionData.GetInstance()._monitorConfigs);
                        Task.Delay(500).Wait();

                        // jim add 20240627
                        DdpmCommonHelper.DeviceManagerSA.Notify_refresh_app_list();

                        // jim add 20240806
                        Task.Delay(500).Wait();
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

                if (item != null &&
                    Expander_Auto.IsExpanded == true)
                {
                    int index = this.lb_AppList.Items.IndexOf(item);
                    selected_app = (AppData)lb_AppList.Items[index];
                    int index_config = get_index_of_json_config_for_cur_monitor(DdpmCommonHelper.ModuleOwner.SelectedHomeDevice.MonitorInfo);
                    if (index_config >= 0)
                    {
                        Test_AddAppCollectionData.GetInstance()._monitorConfigs[index_config].RunType = (int)ColorPresetRunType.Auto;
                        Test_AddAppCollectionData.GetInstance()._monitorConfigs[index_config].AppInfo.Remove(selected_app.AppName);

                        DdpmCommonHelper.DeviceManagerSA.WriteColorPresetSettings(Test_AddAppCollectionData.GetInstance()._monitorConfigs);
                        Task.Delay(500).Wait();
                    }

                    Test_AddAppCollectionData.GetInstance().AppsList.Remove(selected_app);
                    Task.Delay(100).Wait();
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
            //string keyName = string.Format("{0}\\{1}", "HKEY_CURRENT_USER", "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\CloudStore\\Store\\DefaultAccount\\Current\\default$windows.data.bluelightreduction.bluelightreductionstate\\windows.data.bluelightreduction.bluelightreductionstate");

            //ColorViewModel vm = (ColorViewModel)DataContext;

            /*
            if (vm.registryMonitor_NightLight == null)
            {
                vm.registryMonitor_NightLight = new RegistryMonitor_NightLight(keyName);
                vm.registryMonitor_NightLight.RegChanged += new EventHandler(vm.OnRegChanged_NightLight);
                vm.registryMonitor_NightLight.Error += new System.IO.ErrorEventHandler(vm.OnError_NightLight);
                vm.registryMonitor_NightLight.Start();
            }
            */

            var psi = new System.Diagnostics.ProcessStartInfo();

            psi.FileName = "ms-settings:nightlight";
            psi.UseShellExecute = true;

            //System.Diagnostics.Process.Start(psi);
            DDPM.SA.Common.Settings.DDPMFileSecurity.StartProcessSafely(null, psi);
        }       

        private void expanderHasExpanded(object sender, RoutedEventArgs args)
        {
            ColorViewModel vm = (ColorViewModel)DataContext;

            Expander expander_sender = (Expander)sender;

            if (expander_sender.Name == "Expander_Manual")
            {
                Expander_Auto.IsExpanded = false;

                this.Dispatcher.Invoke((Action)(() =>
                {
                    //ColorViewModel vm = (ColorViewModel)DataContext;
                    if (DdpmCommonHelper.ModuleOwner.SelectedHomeDevice.MonitorInfo != null)
                        DdpmCommonHelper.DeviceManagerSA.AutoSetColorPresetForMonitorConfig(DdpmCommonHelper.ModuleOwner.SelectedHomeDevice.MonitorInfo, "OFF", vm.Is_Game_DeviceName, vm.IsAutoColorPreset_Lock); // jim 20241207 modify for The DDPM color profile can not be applied by DDPM on Smart HDR mode.(Gaming monitor ex: AW2724DM)
                    //DdpmCommonHelper.DeviceManagerSA.AutoSetColorPresetForMonitorConfig((vm.MyModule.SelectedHomeDevice.MonitorInfo.Index).ToString(), "off");
                    //int i = 0;

                    if (!vm.Is_ColorPreset_ManualFirst)
                    {
                        Test_AddAppCollectionData.GetInstance()._monitorConfigs = DdpmCommonHelper.DeviceManagerSA.ReadColorPresetSettings().Result;

                        int index = -1;

                        if (DdpmCommonHelper.ModuleOwner.SelectedHomeDevice.MonitorInfo != null)
                            index = get_index_of_json_config_for_cur_monitor(DdpmCommonHelper.ModuleOwner.SelectedHomeDevice.MonitorInfo);

                        if (index >= 0)
                        {
                            int nVCPE2_Value = Test_AddAppCollectionData.GetInstance()._monitorConfigs[index].ColorForManual;
                            string strColorPresetName_Manual = DdpmCommonHelper.DeviceManagerSA.GetColorPresetName(nVCPE2_Value).Result;

                            string strSync_ColorPresetName_Manual = string.Empty;
                            //strSync_CurrentColorPreset = Sync_CurrentColorPreset(curPreset);
                            if (DdpmCommonHelper.ModuleOwner?.SelectedHomeDevice?.MonitorInfo != null)
                                strSync_ColorPresetName_Manual = DdpmCommonHelper.DeviceManagerSA?.Sync_ColorPresetName(DdpmCommonHelper.ModuleOwner?.SelectedHomeDevice?.MonitorInfo, strColorPresetName_Manual).Result;

                            if (!string.IsNullOrEmpty(strSync_ColorPresetName_Manual))
                            {
                                int idx = vm.ColorPresets_ItemsCollection.FindIndex(x => x.ToUpper().Equals(strSync_ColorPresetName_Manual.ToUpper()));
                                if (idx >= 0)
                                {
                                    vm.ColorPresetSelectedIndex = idx;
                                }
                            }
                        }
                    }
                    else
                        vm.Is_ColorPreset_ManualFirst = false;
                }));
            }
            else if (expander_sender.Name == "Expander_Auto")
            {
                Expander_Manual.IsExpanded = false;

                this.Dispatcher.Invoke((Action)(() =>
                {
                    if (DdpmCommonHelper.ModuleOwner.SelectedHomeDevice.MonitorInfo != null)
                        DdpmCommonHelper.DeviceManagerSA.AutoSetColorPresetForMonitorConfig(DdpmCommonHelper.ModuleOwner.SelectedHomeDevice.MonitorInfo, "ON", vm.Is_Game_DeviceName, vm.IsAutoColorPreset_Lock); // jim 20241207 modify for The DDPM color profile can not be applied by DDPM on Smart HDR mode.(Gaming monitor ex: AW2724DM)
                    //DdpmCommonHelper.DeviceManagerSA.AutoSetColorPresetForMonitorConfig((vm.MyModule.SelectedHomeDevice.MonitorInfo.Index).ToString(), "on");
                    //int j = 0;
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

                DdpmCommonHelper.DeviceManagerSA.AutoColorManagementForMonitorConfig(DdpmCommonHelper.ModuleOwner.SelectedHomeDevice.MonitorInfo, "ON");

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

                DdpmCommonHelper.DeviceManagerSA.AutoColorManagementForMonitorConfig(DdpmCommonHelper.ModuleOwner.SelectedHomeDevice.MonitorInfo, "OFF");

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
            DdpmCommonHelper.DeviceManagerSA.AutoColorManagementForMonitorConfig(DdpmCommonHelper.ModuleOwner.SelectedHomeDevice.MonitorInfo, "BYMONITOR");

            if ((bool)rb_Colorpreset_based_ICCprofile.IsChecked)
            {
                rb_Colorpreset_based_ICCprofile.IsChecked = false;
            }
        }

        private void rb_Colorpreset_based_ICCprofile_click(object sender, RoutedEventArgs e)
        {
            DdpmCommonHelper.DeviceManagerSA.AutoColorManagementForMonitorConfig(DdpmCommonHelper.ModuleOwner.SelectedHomeDevice.MonitorInfo, "BYHOST");

            ColorViewModel vm = (ColorViewModel)DataContext;

            if ((bool)rb_ICCprofile_based_Colorpreset.IsChecked)
            {
                rb_ICCprofile_based_Colorpreset.IsChecked = false;

                //Dean 0612 add
                if (vm != null)
                    vm.ICCprofile_based_Colorpreset_enable = false;
            }
        }

        private void lb_AppList_PreviewDragEnter(object sender, System.Windows.DragEventArgs e)
        {
            System.Windows.DataObject dataObj = (System.Windows.DataObject)e.Data;
            if (!dataObj.ContainsFileDropList())
            {
                //pathName = "Dragging object is not file list.";          
                return;
            }
            string[] dropFileNames = (string[])dataObj.GetData(System.Windows.DataFormats.FileDrop);
            if (dropFileNames == null)
            {
                //pathName = "Dragging object is a file list, but it\'s empty.";                
                return;
            }
            if (dropFileNames.Length != 1)
            {
                //pathName = "Dragging object is not \"single\" file.";               
                return;
            }

            string targetPath = dropFileNames[0];

            //Elsa Add Security
            //string FileInfo;
            //if (!DDPMFileSecurity.IsFilePathValid(targetPath, out FileInfo))
            //{
            //    _log.Info($"{nameof(lb_AppList_PreviewDragEnter)} {FileInfo}");
            //    return;
            //}

            if (targetPath.EndsWith(".lnk") || targetPath.EndsWith(".exe"))
            {
                string strAppName = string.Empty;
                string strFileName = string.Empty;
                string strAppIcon = string.Empty;

                if (targetPath.EndsWith(".lnk"))
                {
                    // IWshRuntimeLibrary is in the COM library "Windows Script Host Object Model"
                    //IWshRuntimeLibrary.WshShell shell = new IWshRuntimeLibrary.WshShell();

                    //IWshRuntimeLibrary.IWshShortcut shortcut = (IWshRuntimeLibrary.IWshShortcut)shell.CreateShortcut(targetPath);

                    //strAppName = System.IO.Path.GetFileNameWithoutExtension(shortcut.FullName);
                    //strFileName = System.IO.Path.GetFileName(shortcut.TargetPath);
                    //strFileName = shortcut.TargetPath;

                    FileStream fileStream = File.Open(targetPath, FileMode.Open, FileAccess.Read);

                    try
                    {
                        using (System.IO.BinaryReader fileReader = new BinaryReader(fileStream))
                        {
                            fileStream.Seek(0x14, SeekOrigin.Begin);     // Seek to flags
                            uint flags = fileReader.ReadUInt32();        // Read flags
                            if ((flags & 1) == 1)
                            {                      // Bit 1 set means we have to
                                                   // skip the shell item ID list
                                fileStream.Seek(0x4c, SeekOrigin.Begin); // Seek to the end of the header
                                uint offset = fileReader.ReadUInt16();   // Read the length of the Shell item ID list
                                fileStream.Seek(offset, SeekOrigin.Current); // Seek past it (to the file locator info)
                            }

                            long fileInfoStartsAt = fileStream.Position; // Store the offset where the file info
                                                                         // structure begins
                            uint totalStructLength = fileReader.ReadUInt32(); // read the length of the whole struct
                            fileStream.Seek(0xc, SeekOrigin.Current); // seek to offset to base pathname
                            uint fileOffset = fileReader.ReadUInt32(); // read offset to base pathname
                                                                       // the offset is from the beginning of the file info struct (fileInfoStartsAt)
                            fileStream.Seek((fileInfoStartsAt + fileOffset), SeekOrigin.Begin); // Seek to beginning of
                                                                                                // base pathname (target)
                            long pathLength = (totalStructLength + fileInfoStartsAt) - fileStream.Position - 2; // read
                                                                                                                // the base pathname. I don't need the 2 terminating nulls.
                            char[] linkTarget = fileReader.ReadChars((int)pathLength); // should be unicode safe
                            var link = new string(linkTarget);

                            int begin = link.IndexOf("\0\0");
                            if (begin > -1)
                            {
                                int end = link.IndexOf("\\\\", begin + 2) + 2;
                                end = link.IndexOf('\0', end) + 1;

                                string firstPart = link.Substring(0, begin);
                                string secondPart = link.Substring(end);

                                strFileName = firstPart + secondPart;
                            }
                            else
                            {
                                strFileName = link;
                            }
                        }
                    }
                    catch
                    {
                        // No log function in this project
                    }

                }

                if (targetPath.EndsWith(".exe"))
                {
                    strFileName = targetPath;
                }

                Dictionary<string, InstalledAppInfo> data = DdpmCommonHelper.DeviceManagerSA.FindAppsbyShell().Result;

                foreach (KeyValuePair<string, InstalledAppInfo> kvp in data)
                {
                    if (kvp.Value.AppInstallPath.Equals(strFileName, StringComparison.OrdinalIgnoreCase))
                    {
                        strAppName = kvp.Value.AppName;

                        string strFolder = DdpmCommonHelper.DeviceManagerSA.GetAppIconFolderPath().Result;
                        strFolder += "\\";

                        string info = string.Empty;
                        //DDPM.SA.Common.Settings.DDPMFileSecurity.SRemoveSymbolicFolder(strFolder, out info);   // 20241004 Add for Security
                        if (!System.IO.Directory.Exists(strFolder))
                            System.IO.Directory.CreateDirectory(strFolder);

                        if (!DDPM.SA.Common.Settings.DDPMFileSecurity.ValidateFilePath(strFolder, out info))
                        {
                            DdpmCommonHelper.WriteUILog($"[ColorRightView.xaml.cs][lb_AppList_PreviewDragEnter] ValidateFilePath failed: {info}, it cause no app icon");
                            continue;
                        }
                        if (System.IO.File.Exists(strFolder + kvp.Value.IconName + ".png"))
                        {
                            strAppIcon = strFolder + kvp.Value.IconName + ".png";
                        }

                        AppData? be = Test_AddAppCollectionData.GetInstance().AppsList.FirstOrDefault(x => x.AppName == (strAppName));

                        if (be != null)
                        { }
                        else
                        {
                            ColorViewModel vm = (ColorViewModel)DataContext;

                            List<string> _supported_preset = new List<string>();
                            _supported_preset = vm.SupportColorPresets;

                            Test_AddAppCollectionData.GetInstance().AppsList.Add(new AppData
                            {
                                AppIcon = strAppIcon,
                                AppName = strAppName,
                                AppPresetIdx = 0,
                                IsDeleteAble = System.Windows.Visibility.Visible,
                                SupportPreset = new List<string>(_supported_preset),
                            });
                        }

                        Test_AddAppCollectionData.GetInstance()._monitorConfigs = DdpmCommonHelper.DeviceManagerSA.ReadColorPresetSettings().Result;

                        int index = get_index_of_json_config_for_cur_monitor(DdpmCommonHelper.ModuleOwner.SelectedHomeDevice.MonitorInfo);

                        if (index >= 0)
                        {
                            if (!(Test_AddAppCollectionData.GetInstance()._monitorConfigs[index].AppInfo.ContainsKey(strAppName)))
                            {
                                Test_AddAppCollectionData.GetInstance()._monitorConfigs[index].AppInfo.Add(strAppName, new ColorPresetSettings_AppInfo()
                                {
                                    //ColorPresetName = "Standard/Native",
                                    Color = 0,
                                    HDRColor = -1,
                                    IconName = strAppIcon,

                                });

                            }

                            DdpmCommonHelper.DeviceManagerSA.WriteColorPresetSettings(Test_AddAppCollectionData.GetInstance()._monitorConfigs);
                            Task.Delay(500).Wait();

                            DdpmCommonHelper.DeviceManagerSA.Notify_refresh_app_list();
                        }
                    }
                }

            }
        }

        private void Hyperlink_Click(object sender, RoutedEventArgs e)
        {
        }

        private void cbManualPreset_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var comboBox = sender as System.Windows.Controls.ComboBox;
            ColorViewModel vm = (ColorViewModel)DataContext;
            if (vm != null && comboBox != null &&
               !string.IsNullOrEmpty(vm.last_selected_value))
            {
                comboBox.SelectedValue = vm.last_selected_value;
            }
        }

        private void colorManualExpander_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Expander expander = sender as Expander;
                if (expander != null)
                {
                    e.Handled = true;
                    //Robert_Lin 2025-2-26 Narrator. If Expander is already expanded, then do nothing
                    if (expander.IsExpanded)
                    {
                        //expander.IsExpanded = !expander.IsExpanded;
                    }
                    else
                    {
                        expander.IsExpanded = !expander.IsExpanded;
                    }
                }
            }
        }

        private void colorAutoExpander_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Expander expander = sender as Expander;
                if (expander != null)
                {
                    e.Handled = true;
                    //Robert_Lin 2025-2-26 Narrator. If Expander is already expanded, then do nothing
                    if (expander.IsExpanded)
                    {
                        //expander.IsExpanded = !expander.IsExpanded;
                    }
                    else
                    {
                        expander.IsExpanded = !expander.IsExpanded;
                    }
                }
            }
        }

        private void advancedSettingsExpander_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Expander expander = sender as Expander;
                if (expander != null)
                {
                    e.Handled = true;
                    //Robert_Lin 2025-2-26 Narrator. If Expander is already expanded, then do nothing
                    if (expander.IsExpanded)
                    {
                        //expander.IsExpanded = !expander.IsExpanded;
                    }
                    else
                    {
                        expander.IsExpanded = !expander.IsExpanded;
                    }
                }
            }
        }

        private void iccProfileHyperlink_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                System.Windows.Documents.Hyperlink hyperlink = sender as System.Windows.Documents.Hyperlink;
                if (hyperlink != null)
                {
                    e.Handled = true;
                    //Robert_Lin 2025-2-26 Narrator. If Hyperlink is already expanded, then do nothing
                    if (hyperlink.IsEnabled)
                    {
                        ColorViewModel vm = (ColorViewModel)DataContext;
                        if (vm != null)
                            vm.Handle_ICC_profile_hyperlink_ClickCommand();
                    }
                }
            }
        }

        private double GetScreenScaleX()
        {
            var source = PresentationSource.FromVisual(this);
            if (source?.CompositionTarget != null)
            {
                return source.CompositionTarget.TransformToDevice.M11;
            }
            return 1;
        }

        private void toolTip_Opened(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.ToolTip? target = sender as System.Windows.Controls.ToolTip;
            if (target == null)
                return;
            double screenScaleX = GetScreenScaleX();
            Window mainWindow = System.Windows.Application.Current.MainWindow;
            double winRightX = (mainWindow.Left + mainWindow!.ActualWidth) * screenScaleX;
            double mousepositionX = System.Windows.Forms.Cursor.Position.X;
            double mouseaddtooltip = mousepositionX + target.ActualWidth * screenScaleX;
            if (winRightX > mouseaddtooltip)
            {
                target.HorizontalOffset = 2;
            }
            else
            {
                target.HorizontalOffset = -1 * target.ActualWidth + 14;
            }
        }
    }
}