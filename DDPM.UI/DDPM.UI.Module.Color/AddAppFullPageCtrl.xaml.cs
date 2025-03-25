using DDPM.SA.Common;
using DDPM.UI.Common;
using Dell.Client.Framework.UX.WPF.Controls;
using System;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using VcpCore.Common;

namespace DDPM.UI.Module.Color
{
    public partial class AddAppFullPageCtrl : UserControl
    {
        private List<string> _supported_preset = new List<string>();
        private bool _isSortByNameFirst = true;

        public AddAppFullPageCtrl()
        {
            InitializeComponent();
            RefreshAppList();
        }

        private void UserControl_Loaded(object sender, EventArgs e)
        {
            /*
            ColorViewModel vm = (ColorViewModel)DataContext;

            Dictionary<string, InstalledAppInfo> data = DdpmCommonHelper.DeviceManagerSA.FindAppsbyShell().Result;

            string strFolder = DdpmCommonHelper.DeviceManagerSA.GetAppIconFolderPath().Result;
            strFolder += "\\";

            string info = string.Empty;
            //DDPM.SA.Common.Settings.DDPMFileSecurity.SRemoveSymbolicFolder(strFolder, out info);   // 20241004 Add for Security

            if (!System.IO.Directory.Exists(strFolder))
                System.IO.Directory.CreateDirectory(strFolder);

            //Elsa Add Security
            //string FileInfo;
            //if (!DDPMFileSecurity.IsFolderPathValid(strFolder, out FileInfo))
            //{
            //    _log.Info($"{nameof(UserControl_Loaded)} {FileInfo}");
            //}

            //Robert_Lin 2025-3-14, after changed Theme, the UserControl_Loaded will be called again, so need to clear the list first
            // and refresh with filter and sort settings
            _bind_apps.Clear();
            _apps_all.Clear();


            if (DDPM.SA.Common.Settings.DDPMFileSecurity.ValidateFilePath(strFolder, out info))
            {

                foreach (KeyValuePair<string, InstalledAppInfo> kvp in data)
                {
                    Bind_AddFullPage_AppCollectionData new_Appdata = new Bind_AddFullPage_AppCollectionData();

                    new_Appdata.AppName = kvp.Value.AppName;
                    new_Appdata.InstalledDate = kvp.Value.lastModifyTime;

                    if (System.IO.File.Exists(strFolder + kvp.Value.IconName + ".png"))
                    {
                        new_Appdata.AppIcon = strFolder + kvp.Value.IconName + ".png";
                    }
                    else
                    {
                        new_Appdata.AppIcon = "Assets/palette.png";
                    }

                    //_bind_apps.Add(new_Appdata);
                    _apps_all.Add(new_Appdata);
                }
            }
            else
            {
                DdpmCommonHelper.WriteUILog($"[AddAppFullPageCtrl][UserControl_Loaded] ValidateFilePath failed: {info}, it cause app list empty");
            }

            //Robert_Lin 2025-3-14 apply filter
            string userInputText = "";
            if (txtSearchText != null)
            {
                userInputText = txtSearchText.Text;
            }
            List<Bind_AddFullPage_AppCollectionData> filterList = _apps_all.Where(contact => contact.AppName.Contains(userInputText, StringComparison.InvariantCultureIgnoreCase)).ToList(); 

            if ((filterList != null) && (filterList.Count > 0))
            {
                //Robert_Lin 2025-3-14 to sort the app list by AppName, default is sort by Name _Ascending
                List<Bind_AddFullPage_AppCollectionData> sortList;
                sortList = filterList.OrderBy(x => x.AppName).ToList();
                foreach (var item in sortList)
                {
                    if (!_bind_apps.Contains(item))
                    {
                        _bind_apps.Add(item);
                    }
                }
            }

            lb_Installed_App.ItemsSource = _bind_apps;
            */
        }

        private List<AppCollectionData> _apps { get; set; } = new List<AppCollectionData>();
        private ObservableCollection<Bind_AddFullPage_AppCollectionData> _bind_apps { get; set; } = new ObservableCollection<Bind_AddFullPage_AppCollectionData>();

        private IList<Bind_AddFullPage_AppCollectionData> _apps_all = new List<Bind_AddFullPage_AppCollectionData>();

        private void btnCancel_Click(object sender, EventArgs e)
        {
            // jim modify 20240604

            //ColorViewModel vm = (ColorViewModel)DataContext;
            //vm.ModuleOwner?.CloseFullView();

            DdpmCommonHelper.ModuleOwner?.CloseFullView();
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            ColorViewModel vm = (ColorViewModel)DataContext;

            Test_AddAppCollectionData.GetInstance()._monitorConfigs = DdpmCommonHelper.DeviceManagerSA.ReadColorPresetSettings().Result;

            int index = get_index_of_json_config_for_cur_monitor(DdpmCommonHelper.ModuleOwner.SelectedHomeDevice.MonitorInfo);

            if (index < 0)
            {
                Test_AddAppCollectionData.GetInstance()._monitorConfigs.Add(new ColorPresetSettings()
                {
                    RunType = (int)ColorPresetRunType.Auto,
                    AppInfo = new Dictionary<string, ColorPresetSettings_AppInfo>(),
                    //PresetForManual = "Standard/Native"
                    ColorForManual = 0
                });

                index = get_index_of_json_config_for_cur_monitor(DdpmCommonHelper.ModuleOwner.SelectedHomeDevice.MonitorInfo);
            }

            _supported_preset = vm.SupportColorPresets;

            //Robert_Lin 2025-3-17 Change the lsitview name to appListView from lb_Installed_App
            foreach (var item in appListView.SelectedItems)
            {
                Bind_AddFullPage_AppCollectionData temp_selApps = (Bind_AddFullPage_AppCollectionData)item;

                AppData? be = Test_AddAppCollectionData.GetInstance().AppsList.FirstOrDefault(x => x.AppName == (temp_selApps.AppName));

                if (be != null)
                { }
                else
                {
                    Test_AddAppCollectionData.GetInstance().AppsList.Add(new AppData
                    {
                        AppIcon = temp_selApps.AppIcon,
                        AppName = temp_selApps.AppName,
                        AppPresetIdx = 0,
                        IsDeleteAble = System.Windows.Visibility.Visible,
                        SupportPreset = new List<string>(_supported_preset),
                    });
                }

                if (!(Test_AddAppCollectionData.GetInstance()._monitorConfigs[index].AppInfo.ContainsKey(temp_selApps.AppName)))
                {
                    Test_AddAppCollectionData.GetInstance()._monitorConfigs[index].AppInfo.Add(temp_selApps.AppName, new ColorPresetSettings_AppInfo()
                    {
                        //ColorPresetName = "Standard/Native",
                        Color = 0,
                        HDRColor = -1,
                        IconName = temp_selApps.AppIcon,
                    });
                }
            }

            DdpmCommonHelper.DeviceManagerSA.WriteColorPresetSettings(Test_AddAppCollectionData.GetInstance()._monitorConfigs);
            Task.Delay(500).Wait();

            // jim add 20240621
            DdpmCommonHelper.DeviceManagerSA.Notify_refresh_app_list();
            Task.Delay(100).Wait();

            // jim modify 20240605
            DdpmCommonHelper.ModuleOwner?.CloseFullView();

            //vm.ModuleOwner?.CloseFullView();
        }

        private int get_index_of_json_config_for_cur_monitor(MonitorInfo mo)
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

        private void edFilter_TextChanged(object sender, TextChangedEventArgs e)
        {
            //Robert_Lin 2025-3-14 to get the user input text from sender, instead of static name 'edFilter'
            if (sender == null)
                return;
            string userInputText = "";
            if (sender is UXTextBox)
            {
                UXTextBox txtBox = sender as UXTextBox;
                userInputText = txtBox.Text;
            }
            else if (sender is TextBox)
            {
                TextBox txtBox = sender as TextBox;
                userInputText = txtBox.Text;
            }
            List<Bind_AddFullPage_AppCollectionData> TempFiltered;
            //TempFiltered = _apps_all.Where(contact => contact.AppName.Contains(edFilter.Text, StringComparison.InvariantCultureIgnoreCase)).ToList();
            TempFiltered = _apps_all.Where(contact => contact.AppName.Contains(userInputText, StringComparison.InvariantCultureIgnoreCase)).ToList();

            for (int i = _bind_apps.Count - 1; i >= 0; i--)
            {
                var item = _bind_apps[i];
                if (!TempFiltered.Contains(item))
                {
                    _bind_apps.Remove(item);
                }
            }

            foreach (var item in TempFiltered)
            {
                if (!_bind_apps.Contains(item))
                {
                    _bind_apps.Add(item);
                }
            }
        }

        private void btnSortbyName_Ascending_Click(object sender, EventArgs e)
        {
            btnSortbyName_Ascending.Visibility = System.Windows.Visibility.Collapsed;
            btnSortbyName_Descending.Visibility = System.Windows.Visibility.Visible;

            List<Bind_AddFullPage_AppCollectionData> TempSorted;

            TempSorted = _bind_apps.OrderBy(x => x.AppName).ToList();

            _bind_apps.Clear();

            foreach (var item in TempSorted)
            {
                if (!_bind_apps.Contains(item))
                {
                    _bind_apps.Add(item);
                }
            }
        }

        private void btnSortbyName_Descending_Click(object sender, EventArgs e)
        {
            btnSortbyName_Descending.Visibility = System.Windows.Visibility.Collapsed;
            btnSortbyName_Ascending.Visibility = System.Windows.Visibility.Visible;

            List<Bind_AddFullPage_AppCollectionData> TempSorted;

            TempSorted = _bind_apps.OrderByDescending(x => x.AppName).ToList();

            _bind_apps.Clear();

            foreach (var item in TempSorted)
            {
                if (!_bind_apps.Contains(item))
                {
                    _bind_apps.Add(item);
                }
            }
        }

        private void btnSortbyDate_Ascending_Click(object sender, EventArgs e)
        {
            btnSortbyDate_Ascending.Visibility = System.Windows.Visibility.Collapsed;
            btnSortbyDate_Descending.Visibility = System.Windows.Visibility.Visible;

            List<Bind_AddFullPage_AppCollectionData> TempSorted;

            TempSorted = _bind_apps.OrderBy(x => x.InstalledDate).ToList();

            _bind_apps.Clear();

            foreach (var item in TempSorted)
            {
                if (!_bind_apps.Contains(item))
                {
                    _bind_apps.Add(item);
                }
            }
        }

        private void btnSortbyDate_Descending_Click(object sender, EventArgs e)
        {
            btnSortbyDate_Descending.Visibility = System.Windows.Visibility.Collapsed;
            btnSortbyDate_Ascending.Visibility = System.Windows.Visibility.Visible;

            List<Bind_AddFullPage_AppCollectionData> TempSorted;

            TempSorted = _bind_apps.OrderByDescending(x => x.InstalledDate).ToList();

            _bind_apps.Clear();

            foreach (var item in TempSorted)
            {
                if (!_bind_apps.Contains(item))
                {
                    _bind_apps.Add(item);
                }
            }
        }

        private void txtSearchText_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {

        }

        private void sortByNameButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (sender == null)
                return;
            _isSortByNameFirst = true;
            if (sender is ToggleButton)
            {
                ToggleButton btn = sender as ToggleButton;
                if (btn.IsChecked == true)
                {
                    btnSortbyName_Ascending_Click(sender, e);
                }
                else
                {
                    btnSortbyName_Descending_Click(sender, e);
                }
            }
        }

        private void sortByDateButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (sender == null)
                return;
            _isSortByNameFirst = false;
            if (sender is ToggleButton)
            {
                ToggleButton btn = sender as ToggleButton;
                if (btn.IsChecked == true)
                {
                    btnSortbyDate_Ascending_Click(sender, e);
                }
                else
                {
                    btnSortbyDate_Descending_Click(sender, e);
                }
            }
        }

        private List<Bind_AddFullPage_AppCollectionData> GetAllAppList()
        {
            List<Bind_AddFullPage_AppCollectionData> listOut = new List<Bind_AddFullPage_AppCollectionData>();
            if (DdpmCommonHelper.DeviceManagerSA != null)
            {
                string strFolder = DdpmCommonHelper.DeviceManagerSA.GetAppIconFolderPath().Result;
                strFolder += "\\";

                string info = string.Empty;
                //DDPM.SA.Common.Settings.DDPMFileSecurity.SRemoveSymbolicFolder(strFolder, out info);   // 20241004 Add for Security

                if (!System.IO.Directory.Exists(strFolder))
                    System.IO.Directory.CreateDirectory(strFolder);

                if (!DDPM.SA.Common.Settings.DDPMFileSecurity.ValidateFilePath(strFolder, out info))
                {
                    DdpmCommonHelper.WriteUILog($"[AddAppFullPageCtrl][UserControl_Loaded] ValidateFilePath failed: {info}, it cause app list empty");
                    return listOut;
                }

                Dictionary<string, InstalledAppInfo> rawData = DdpmCommonHelper.DeviceManagerSA.FindAppsbyShell().Result;

                foreach (KeyValuePair<string, InstalledAppInfo> kvp in rawData)
                {
                    Bind_AddFullPage_AppCollectionData new_Appdata = new Bind_AddFullPage_AppCollectionData();

                    new_Appdata.AppName = kvp.Value.AppName;
                    new_Appdata.InstalledDate = kvp.Value.lastModifyTime;

                    if (System.IO.File.Exists(strFolder + kvp.Value.IconName + ".png"))
                    {
                        new_Appdata.AppIcon = strFolder + kvp.Value.IconName + ".png";
                    }
                    else
                    {
                        new_Appdata.AppIcon = "Assets/palette.png";
                    }

                    //_bind_apps.Add(new_Appdata);
                    listOut.Add(new_Appdata);
                }
            }
            return listOut;
        }

        private void RefreshAppList()
        {
            /*
            if (DdpmCommonHelper.DeviceManagerSA == null)
                return;


            string strFolder = DdpmCommonHelper.DeviceManagerSA.GetAppIconFolderPath().Result;
            strFolder += "\\";

            string info = string.Empty;
            //DDPM.SA.Common.Settings.DDPMFileSecurity.SRemoveSymbolicFolder(strFolder, out info);   // 20241004 Add for Security

            if (!System.IO.Directory.Exists(strFolder))
                System.IO.Directory.CreateDirectory(strFolder);

            Dictionary<string, InstalledAppInfo> rawData = DdpmCommonHelper.DeviceManagerSA.FindAppsbyShell().Result;

            //Robert_Lin 2025-3-14, after changed Theme, the UserControl_Loaded will be called again, so need to clear the list first
            // and refresh with filter and sort settings
            _bind_apps.Clear();
            _apps_all.Clear();


            if (DDPM.SA.Common.Settings.DDPMFileSecurity.ValidateFilePath(strFolder, out info))
            {

                foreach (KeyValuePair<string, InstalledAppInfo> kvp in rawData)
                {
                    Bind_AddFullPage_AppCollectionData new_Appdata = new Bind_AddFullPage_AppCollectionData();

                    new_Appdata.AppName = kvp.Value.AppName;
                    new_Appdata.InstalledDate = kvp.Value.lastModifyTime;

                    if (System.IO.File.Exists(strFolder + kvp.Value.IconName + ".png"))
                    {
                        new_Appdata.AppIcon = strFolder + kvp.Value.IconName + ".png";
                    }
                    else
                    {
                        new_Appdata.AppIcon = "Assets/palette.png";
                    }

                    //_bind_apps.Add(new_Appdata);
                    _apps_all.Add(new_Appdata);
                }
            }
            else
            {
                DdpmCommonHelper.WriteUILog($"[AddAppFullPageCtrl][UserControl_Loaded] ValidateFilePath failed: {info}, it cause app list empty");
            }
            */

            _bind_apps.Clear();
            _apps_all.Clear();

            _apps_all = GetAllAppList();
            if (_apps_all == null || _apps_all.Count == 0)
            {
                return;
            }

            //Robert_Lin 2025-3-14 apply filter
            string userInputText = "";
            if (txtSearchText != null)
            {
                userInputText = txtSearchText.Text;
            }
            List<Bind_AddFullPage_AppCollectionData> filterList = _apps_all.Where(contact => contact.AppName.Contains(userInputText, StringComparison.InvariantCultureIgnoreCase)).ToList();

            if ((filterList != null) && (filterList.Count > 0))
            {
                //Robert_Lin 2025-3-14 to sort the app list by AppName, default is sort by Name _Ascending
                List<Bind_AddFullPage_AppCollectionData> sortList;

                if (_isSortByNameFirst)
                {
                    if (sortByNameButton.IsChecked == true)
                    {
                        sortList = filterList.OrderBy(x => x.AppName).ToList();
                    }
                    else
                    {
                        sortList = filterList.OrderByDescending(x => x.AppName).ToList();
                    }
                }
                else
                {
                    if (sortByDateButton.IsChecked == true)
                    {
                        sortList = filterList.OrderBy(x => x.InstalledDate).ToList();
                    }
                    else
                    {
                        sortList = filterList.OrderByDescending(x => x.InstalledDate).ToList();
                    }
                }

                foreach (var item in sortList)
                {
                    if (!_bind_apps.Contains(item))
                    {
                        _bind_apps.Add(item);
                    }
                }
            }

            lb_Installed_App.ItemsSource = _bind_apps;
            appListView.ItemsSource = _bind_apps;
        }

        private void appListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            btnAdd.IsEnabled = appListView.SelectedItems.Count > 0;
        }
    }
}