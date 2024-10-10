using DDPM.SA.Common;
using DDPM.SA.Common.Settings;
using DDPM.UI.Common;
using DDPM.UI.Common.Models;
using DDPM.UI.Common.ViewModels;
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
using Microsoft.Win32;
using System.Diagnostics;
using System.IO;
using Microsoft;
using DDPM.UI.Common.UserControls;

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
        private DDPM.UI.Common.ViewModels.EzArrangeViewModel _vm;
        private readonly DisplayViewModel _vmDisplay;
        private readonly IConsole _console;
        private readonly ILog _log;
        private HomeDevice _selecthomeDevice;
        #endregion Private Members

        private List<AppCollectionData> _apps { get; set; } = new List<AppCollectionData>();
        private ObservableCollection<Bind_AddFullPage_AppCollectionData> _bind_apps { get; set; } = new ObservableCollection<Bind_AddFullPage_AppCollectionData>();
        private IList<Bind_AddFullPage_AppCollectionData> _apps_all = new List<Bind_AddFullPage_AppCollectionData>();
        public ObservableCollection<ApplicationItem> InstalledApplications { get; set; } = new ObservableCollection<ApplicationItem>();
        public EzMemoryAddApplication(DisplayViewModel vmDisplay, HomeDevice _homeDeviceSelect)
        {
            _vmDisplay = vmDisplay;
            _homeDevice = vmDisplay.SelectedHomeDevice;
            _console = vmDisplay.Console;
            _selecthomeDevice = _homeDeviceSelect;
            _log = vmDisplay.Console.CreateLog("EzMemoryAddApplication");
            _log.Info($"{nameof(EzMemoryAddApplication)} - Constructed");
            _deviceManagerSA = HomeDevice.DeviceManagerSA;
            Requires.NotNull(vmDisplay, nameof(vmDisplay));
            InitializeComponent();

            if (_homeDevice.vmEzArrange == null)
            {
                _homeDevice.vmEzArrange = new DDPM.UI.Common.ViewModels.EzArrangeViewModel(_homeDevice);
            }
            _vm = _homeDevice.vmEzArrange;

            DataContext = _homeDevice.vmEzArrange;

        }

        /// <summary>
        /// "Search field control code
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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

        /// <summary>
        /// SortbyName Ascending
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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

        /// <summary>
        /// SortbyName Descending
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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

        /// <summary>
        /// SortbyDate Ascending
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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

        /// <summary>
        /// SortbyDate Descending
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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

        /// <summary>
        /// UserControl Loaded, Get application list
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            Dictionary<string, InstalledAppInfo> data = DdpmCommonHelper.DeviceManagerSA.GetAllAppList().Result;


            string strFolder = DdpmCommonHelper.DeviceManagerSA.GetAppIconFolderPath().Result;
            strFolder += "\\";

            if (!System.IO.Directory.Exists(strFolder))
                System.IO.Directory.CreateDirectory(strFolder);

            foreach (KeyValuePair<string, InstalledAppInfo> kvp in data)
            {
                Bind_AddFullPage_AppCollectionData new_Appdata = new Bind_AddFullPage_AppCollectionData();

                new_Appdata.AppName = kvp.Value.AppName;
                new_Appdata.InstalledDate = kvp.Value.lastModifyTime;
                new_Appdata.AppPath = kvp.Value.AppInstallPath;
                new_Appdata.AppUserModelID = kvp.Value.AppUserModelID;
                new_Appdata.AppType = kvp.Value.isDesktopApp.ToString();

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

                ApplicationItem newAdd = new ApplicationItem();
                newAdd.AppName = new_Appdata.AppName;
                newAdd.AppIcon = new_Appdata.AppIcon;
                newAdd.AppPath = new_Appdata.AppPath;
                InstalledApplications.Add(newAdd);
            }
            lb_Installed_App.ItemsSource = _bind_apps;
        }

        /// <summary>
        /// Cancel
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            EzMemoryAssignProgram _ezMemoryAssignProgram = new EzMemoryAssignProgram(_vmDisplay, _selecthomeDevice);
            DdpmCommonHelper.ModuleOwner?.OpenFullView(_ezMemoryAssignProgram);
        }

        /// <summary>
        /// Add app
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            if (lb_Installed_App.SelectedItems.Count == 0)
                return;
            var app = lb_Installed_App.SelectedItems.Cast<Bind_AddFullPage_AppCollectionData>().ToList();

            // 如果有重複的應用程式，直接返回
            if (_vm._sortApps.Values.Any(a =>
                a.AppName.Equals(app[0].AppName, StringComparison.OrdinalIgnoreCase) ||
                a.AppUserModelID.Equals(app[0].AppUserModelID, StringComparison.OrdinalIgnoreCase) ||
                a.AppPath.Equals(app[0].AppPath, StringComparison.OrdinalIgnoreCase)))
            {
                Thickness headMargin = new Thickness(24, 30, 45, 24);
                Thickness subMargin = new Thickness(24, -16, 24, 8);
                DdpmCommonHelper.DDPMEzMesssageBox(Strings.msgboxTitleForFirstPage, Strings.subTitleForFirstPage, true, Window.GetWindow(this), 417, 148, headMargin, subMargin);
                return;
            }

            // 同樣的button重選
            if (_vm._sortApps.ContainsKey(_vm.ButtonName))
            {
                _vm._sortApps.Remove(_vm.ButtonName);
            }
            
            _vm._sortApps.Add(_vm.ButtonName, app[0]);

            _vm.UpdateTextBlockAppName(_vm.ButtonName, app[0].AppName);

            EzMemoryAssignProgram _ezMemoryAssignProgram = new EzMemoryAssignProgram(_vmDisplay, _selecthomeDevice);
            DdpmCommonHelper.ModuleOwner?.OpenFullView(_ezMemoryAssignProgram);
        }

        /// <summary>
        /// Select, open dialog to select app or file
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSelect_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // OpenFileDialog
                OpenFileDialog openFileDialog = new OpenFileDialog
                {
                    Title = "",
                    Filter = "All (*.*)|*.*",
                    InitialDirectory = @"C:\",
                    //InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
                    Multiselect = false // only choose one
                };

                if (openFileDialog.ShowDialog() == true)
                {
                    string selectedFilePath = openFileDialog.FileName;
                    string appName = System.IO.Path.GetFileNameWithoutExtension(selectedFilePath);

                    // 檢查是否有重複
                    if (_vm._sortApps.Values.Any(a =>
                        a.AppName.Equals(appName, StringComparison.OrdinalIgnoreCase) ||
                        a.AppPath.Equals(selectedFilePath, StringComparison.OrdinalIgnoreCase)))
                    {
                        Thickness headMargin = new Thickness(24, 30, 45, 24);
                        Thickness subMargin = new Thickness(24, -16, 24, 8);
                        DdpmCommonHelper.DDPMEzMesssageBox(Strings.msgboxTitleForFirstPage, Strings.subTitleForFirstPage, true, Window.GetWindow(this), 417, 148, headMargin, subMargin);
                        return;
                    }

                    // new Bind_AddFullPage_AppCollectionData
                    string fileName = System.IO.Path.GetFileName(selectedFilePath);
                    Bind_AddFullPage_AppCollectionData newApp = new Bind_AddFullPage_AppCollectionData
                    {
                        AppName = fileName,
                        AppPath = selectedFilePath,
                        AppUserModelID = string.Empty, // UserModelID
                        AppType = "True", // "True" = Desktop 
                        InstalledDate = DateTime.Now, // 使用目前時間
                        AppIcon = "Assets/palette.png" // 預設圖示
                    };

                    // 同樣的button重選
                    if (_vm._sortApps.ContainsKey(_vm.ButtonName))
                    {
                        _vm._sortApps.Remove(_vm.ButtonName);
                    }

                    _vm._sortApps.Add(_vm.ButtonName, newApp);

                    _vm.UpdateTextBlockAppName(_vm.ButtonName, fileName);

                    EzMemoryAssignProgram _ezMemoryAssignProgram = new EzMemoryAssignProgram(_vmDisplay, _selecthomeDevice);
                    DdpmCommonHelper.ModuleOwner?.OpenFullView(_ezMemoryAssignProgram);
                }
            }
            catch (Exception ex)
            {
                _log.Error($"{nameof(EzMemoryAddApplication)} btnSelect_Click: Error - {ex.Message}");
            }
        }
    }
}
