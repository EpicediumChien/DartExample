using DDPM.SA.Common;
using DDPM.UI.Common;
using DDPM.UI.Common.Models;
using DDPM.UI.Plugin.Common.ViewModels;
using Dell.Client.Framework.Common;
using Dell.Client.Framework.UX.WPF;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DDPM.UI.Module.EzMemory
{
    /// <summary>
    /// EzMemoryAddApplication.xaml 的互動邏輯
    /// </summary>
    public partial class EzMemoryAddApplication : UserControl
    {
        #region Private Members
        private HomeDevice _homeDevice;
        private IDeviceManagerSA _deviceManagerSA;
        private DDPM.UI.Common.ViewModels.EzMemoryViewModel _vm;
        private readonly DisplayViewModel _vmDisplay;
        private readonly IConsole _console;
        private readonly ILog _log;
        #endregion Private Members

        private List<AppCollectionData> _apps { get; set; } = new List<AppCollectionData>();
        private ObservableCollection<Bind_AddFullPage_AppCollectionData> _bind_apps { get; set; } = new ObservableCollection<Bind_AddFullPage_AppCollectionData>();

        private IList<Bind_AddFullPage_AppCollectionData> _apps_all = new List<Bind_AddFullPage_AppCollectionData>();

        public EzMemoryAddApplication(DisplayViewModel vmDisplay)
        {
            _vmDisplay = vmDisplay;
            _homeDevice = vmDisplay.SelectedHomeDevice;
            _console = vmDisplay.Console;
            _deviceManagerSA = HomeDevice.DeviceManagerSA;

            InitializeComponent();

            if (_homeDevice.vmEzMemory == null)
            {
                _homeDevice.vmEzMemory = new DDPM.UI.Common.ViewModels.EzMemoryViewModel(_homeDevice);
            }
            _vm = _homeDevice.vmEzMemory;
            DataContext = _homeDevice.vmEzMemory;

            InitializeComponent();
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
            //btnSortbyName_Ascending.Visibility = System.Windows.Visibility.Collapsed;
            //btnSortbyName_Descending.Visibility = System.Windows.Visibility.Visible;

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
            //btnSortbyName_Descending.Visibility = System.Windows.Visibility.Collapsed;
            //btnSortbyName_Ascending.Visibility = System.Windows.Visibility.Visible;

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
            //btnSortbyDate_Ascending.Visibility = System.Windows.Visibility.Collapsed;
            //btnSortbyDate_Descending.Visibility = System.Windows.Visibility.Visible;

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
            //btnSortbyDate_Descending.Visibility = System.Windows.Visibility.Collapsed;
            //btnSortbyDate_Ascending.Visibility = System.Windows.Visibility.Visible;

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

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            EzMemoryAssignProgram _ezMemoryAssignProgram = new EzMemoryAssignProgram(_vmDisplay);
            _ezMemoryAssignProgram.DataContext = _vmDisplay;
            DdpmCommonHelper.ModuleOwner?.OpenFullView(_ezMemoryAssignProgram);
        }
        private void btnAdd_Click(object sender, EventArgs e)
        {
            _vm._currentPageIndex++;
            EzMemoryLaunchOption _ezMemoryLaunchOption = new EzMemoryLaunchOption(_vmDisplay);
            _ezMemoryLaunchOption.DataContext = _vmDisplay;
            DdpmCommonHelper.ModuleOwner?.OpenFullView(_ezMemoryLaunchOption);

            //ColorViewModel vm = (ColorViewModel)DataContext;

            //Test_AddAppCollectionData.GetInstance()._monitorConfigs = DdpmCommonHelper.DeviceManagerSA.ReadColorPresetSettings().Result;

            //int index = get_index_of_json_config_for_cur_monitor(DdpmCommonHelper.ModuleOwner.SelectedHomeDevice.MonitorInfo);

            //if (index < 0)
            //{
            //    Test_AddAppCollectionData.GetInstance()._monitorConfigs.Add(new ColorPresetSettings()
            //    {
            //        RunType = (int)ColorPresetRunType.Auto,
            //        AppInfo = new Dictionary<string, ColorPresetSettings_AppInfo>(),
            //        PresetForManual = "Standard/Native"
            //    });

            //    index = get_index_of_json_config_for_cur_monitor(DdpmCommonHelper.ModuleOwner.SelectedHomeDevice.MonitorInfo);
            //}

            //_supported_preset = vm.SupportColorPresets;

            //foreach (var item in lb_Installed_App.SelectedItems)
            //{
            //    Bind_AddFullPage_AppCollectionData temp_selApps = (Bind_AddFullPage_AppCollectionData)item;

            //    AppData? be = Test_AddAppCollectionData.GetInstance().AppsList.FirstOrDefault(x => x.AppName == (temp_selApps.AppName));

            //    if (be != null)
            //    { }
            //    else
            //    {
            //        Test_AddAppCollectionData.GetInstance().AppsList.Add(new AppData
            //        {
            //            AppIcon = temp_selApps.AppIcon,
            //            AppName = temp_selApps.AppName,
            //            AppPresetIdx = 0,
            //            IsDeleteAble = System.Windows.Visibility.Visible,
            //            SupportPreset = new List<string>(_supported_preset),
            //        });
            //    }

            //    if (!(Test_AddAppCollectionData.GetInstance()._monitorConfigs[index].AppInfo.ContainsKey(temp_selApps.AppName)))
            //    {
            //        Test_AddAppCollectionData.GetInstance()._monitorConfigs[index].AppInfo.Add(temp_selApps.AppName, new ColorPresetSettings_AppInfo()
            //        {
            //            ColorPresetName = "Standard/Native",
            //            IconName = temp_selApps.AppIcon,
            //        });
            //    }
            //}

            //DdpmCommonHelper.DeviceManagerSA.WriteColorPresetSettings(Test_AddAppCollectionData.GetInstance()._monitorConfigs);
            //Thread.Sleep(500);


            //DdpmCommonHelper.DeviceManagerSA.Notify_refresh_app_list();
            //Thread.Sleep(100);


            //DdpmCommonHelper.ModuleOwner?.CloseFullView();

            //vm.ModuleOwner?.CloseFullView();
        }
    }
}
