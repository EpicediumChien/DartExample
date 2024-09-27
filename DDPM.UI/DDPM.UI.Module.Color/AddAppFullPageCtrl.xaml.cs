using DDPM.SA.Common;
using DDPM.UI.Common;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using VcpCore.Common;

namespace DDPM.UI.Module.Color
{
    public partial class AddAppFullPageCtrl : UserControl
    {
        private List<string> _supported_preset = new List<string>();
        public AddAppFullPageCtrl()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, EventArgs e)
        {
            ColorViewModel vm = (ColorViewModel)DataContext;

            Dictionary<string, InstalledAppInfo> data = DdpmCommonHelper.DeviceManagerSA.FindAppsbyShell().Result;

            string strFolder = DdpmCommonHelper.DeviceManagerSA.GetAppIconFolderPath().Result;
            strFolder += "\\";

            if (!System.IO.Directory.Exists(strFolder))
                System.IO.Directory.CreateDirectory(strFolder);

            //Elsa Add Security
            //string FileInfo;
            //if (!DDPMFileSecurity.IsFolderPathValid(strFolder, out FileInfo))
            //{
            //    _log.Info($"{nameof(UserControl_Loaded)} {FileInfo}");
            //}

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

                _bind_apps.Add(new_Appdata);
                _apps_all.Add(new_Appdata);
            }

            lb_Installed_App.ItemsSource = _bind_apps;
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
                    PresetForManual = "Standard/Native"
                });

                index = get_index_of_json_config_for_cur_monitor(DdpmCommonHelper.ModuleOwner.SelectedHomeDevice.MonitorInfo);
            }

            _supported_preset = vm.SupportColorPresets;

            foreach (var item in lb_Installed_App.SelectedItems)
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
                        ColorPresetName = "Standard/Native",
                        IconName = temp_selApps.AppIcon,
                    });
                }
            }

            DdpmCommonHelper.DeviceManagerSA.WriteColorPresetSettings(Test_AddAppCollectionData.GetInstance()._monitorConfigs);
            Thread.Sleep(500);

            // jim add 20240621
            DdpmCommonHelper.DeviceManagerSA.Notify_refresh_app_list();
            Thread.Sleep(100);

            // jim modify 20240605
            DdpmCommonHelper.ModuleOwner?.CloseFullView();

            //vm.ModuleOwner?.CloseFullView();
        }

        private int get_index_of_json_config_for_cur_monitor(MonitorInfo mo)
        {
            int index = Test_AddAppCollectionData.GetInstance()._monitorConfigs.FindIndex(x =>
                                            x.ModelName.Trim() == mo.edid.ModelName.Trim() &&
                                            x.SerialNumber.Trim() == mo.edid.SerialNumber.Trim());

            return index;
        }

        private void edFilter_TextChanged(object sender, TextChangedEventArgs e)
        {
            List<Bind_AddFullPage_AppCollectionData> TempFiltered;
            TempFiltered = _apps_all.Where(contact => contact.AppName.Contains(edFilter.Text, StringComparison.InvariantCultureIgnoreCase)).ToList();

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
    }
}