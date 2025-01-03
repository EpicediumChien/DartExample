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
        private bool SortbyNameDescenAscen = false;
        private bool SortbyDateDescenAscen = false;
        #endregion Private Members

        public EzMemoryAddApplication(DisplayViewModel vmDisplay, EzArrangeViewModel vm, HomeDevice _homeDeviceSelect)
        {
            _vmDisplay = vmDisplay;
            _homeDevice = vmDisplay.SelectedHomeDevice;
            _console = vmDisplay.Console;
            _selecthomeDevice = _homeDeviceSelect;
            _log = vmDisplay.Console.CreateLog("EzMemoryAddApplication");
            _log.Info($"{nameof(EzMemoryAddApplication)} - Constructed");
            _deviceManagerSA = HomeDevice.DeviceManagerSA;
            Requires.NotNull(vmDisplay, nameof(vmDisplay));

            if (_homeDevice.vmEzArrange == null)
            {
                _homeDevice.vmEzArrange = new DDPM.UI.Common.ViewModels.EzArrangeViewModel(_homeDevice);
            }
            _vm = vm;
            DataContext = vm;
            
            InitializeComponent();
            edFilter_TextChanged(edFilter, new TextChangedEventArgs(TextBox.TextChangedEvent, UndoAction.None));
        }

        /// <summary>
        /// "Search field control code
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void edFilter_TextChanged(object sender, TextChangedEventArgs e)
        {
            List<Bind_AddFullPage_AppCollectionData> TempFiltered;
            TempFiltered = _vm._apps_all.Where(contact => contact.AppName.Contains(edFilter.Text, StringComparison.InvariantCultureIgnoreCase)).ToList();

            for (int i = _vm._bind_apps.Count - 1; i >= 0; i--)
            {
                var item = _vm._bind_apps[i];
                if (!TempFiltered.Contains(item))
                {
                    _vm._bind_apps.Remove(item);
                }
            }

            foreach (var item in TempFiltered)
            {
                if (!_vm._bind_apps.Contains(item))
                {
                    _vm._bind_apps.Add(item);
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
            if (!SortbyNameDescenAscen)
            {
                List<Bind_AddFullPage_AppCollectionData> TempSorted;

                TempSorted = _vm._bind_apps.OrderBy(x => x.AppName).ToList();

                _vm._bind_apps.Clear();

                foreach (var item in TempSorted)
                {
                    if (!_vm._bind_apps.Contains(item))
                    {
                        _vm._bind_apps.Add(item);
                    }
                }
                SortbyNameDescenAscen = true;
            }
            else
                btnSortbyName_Descending_Click(sender, e);

        }

        /// <summary>
        /// SortbyName Descending
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSortbyName_Descending_Click(object sender, EventArgs e)
        {
            List<Bind_AddFullPage_AppCollectionData> TempSorted;

            TempSorted = _vm._bind_apps.OrderByDescending(x => x.AppName).ToList();

            _vm._bind_apps.Clear();

            foreach (var item in TempSorted)
            {
                if (!_vm._bind_apps.Contains(item))
                {
                    _vm._bind_apps.Add(item);
                }
            }
            SortbyNameDescenAscen = false;
        }

        /// <summary>
        /// SortbyDate Ascending
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSortbyDate_Ascending_Click(object sender, EventArgs e)
        {
            if (!SortbyDateDescenAscen)
            {
                List<Bind_AddFullPage_AppCollectionData> TempSorted;

                TempSorted = _vm._bind_apps.OrderBy(x => x.InstalledDate).ToList();

                _vm._bind_apps.Clear();

                foreach (var item in TempSorted)
                {
                    if (!_vm._bind_apps.Contains(item))
                    {
                        _vm._bind_apps.Add(item);
                    }
                }
                SortbyDateDescenAscen = true;
            }
            else
                btnSortbyDate_Descending_Click(sender, e);
        }

        /// <summary>
        /// SortbyDate Descending
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSortbyDate_Descending_Click(object sender, EventArgs e)
        {

            List<Bind_AddFullPage_AppCollectionData> TempSorted;

            TempSorted = _vm._bind_apps.OrderByDescending(x => x.InstalledDate).ToList();

            _vm._bind_apps.Clear();

            foreach (var item in TempSorted)
            {
                if (!_vm._bind_apps.Contains(item))
                {
                    _vm._bind_apps.Add(item);
                }
            }
            SortbyDateDescenAscen = false;
        }

        /// <summary>
        /// UserControl Loaded, Get application list
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            lb_Installed_App.ItemsSource = _vm._bind_apps;
        }

        /// <summary>
        /// Cancel
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            EzMemoryAssignProgram _ezMemoryAssignProgram = new EzMemoryAssignProgram(_vmDisplay, _vm, _selecthomeDevice);
            DdpmCommonHelper.ModuleOwner?.OpenFullView(_ezMemoryAssignProgram);
        }

        /// <summary>
        /// Add app
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _log.Info($"@{nameof(EzMemoryAddApplication)} btnAdd_Click: ... in");

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

                if (_vm.IsEditProfile)
                {
                    _vm.IsAddPageBack = true;
                }

                EzMemoryAssignProgram _ezMemoryAssignProgram = new EzMemoryAssignProgram(_vmDisplay, _vm, _selecthomeDevice);
                DdpmCommonHelper.ModuleOwner?.OpenFullView(_ezMemoryAssignProgram);
            }
            catch (Exception ex)
            {
                _log.Error($"@{nameof(EzMemoryAddApplication)} btnAdd_Click: Error occurred - {ex.Message}");
            }
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
                _log.Info($"@{nameof(EzMemoryAddApplication)} btnSelect_Click: ... in");

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

                    EzMemoryAssignProgram _ezMemoryAssignProgram = new EzMemoryAssignProgram(_vmDisplay, _vm, _selecthomeDevice);
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
