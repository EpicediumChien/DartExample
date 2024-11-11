using DDPM.Easy.Common;
using DDPM.SA.Common;
using DDPM.UI.Common;
using DDPM.UI.Common.Models;
using DDPM.UI.Common.ViewModels;
using DDPM.UI.Plugin.Common.ViewModels;
using Dell.Client.Framework.Common;
using Dell.Client.Framework.UX.WPF;
using Microsoft;
using System;
using System.Globalization;
using System.Runtime.ConstrainedExecution;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media.Animation;
using DragEventArgs = System.Windows.DragEventArgs;
using ProgressBar = System.Windows.Controls.ProgressBar;
using UserControl = System.Windows.Controls.UserControl;

namespace DDPM.UI.Module.EzMemory
{
    /// <summary>
    /// EzMemoryAssignProgram.xaml 的互動邏輯
    /// </summary>
    public partial class EzMemoryAssignProgram : UserControl
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
        public EzMemoryAssignProgram(DisplayViewModel vmDisplay, EzArrangeViewModel vm, HomeDevice _homeDeviceSelect)
        {
            _vmDisplay = vmDisplay;
            _homeDevice = vmDisplay.SelectedHomeDevice;
            _console = vmDisplay.Console;
            _selecthomeDevice = _homeDeviceSelect;
            _log = vmDisplay.Console.CreateLog("EzMemoryAssignProgram");
            _log.Info($"{nameof(EzMemoryAssignProgram)} - Constructed");
            _deviceManagerSA = HomeDevice.DeviceManagerSA;
            Requires.NotNull(vmDisplay, nameof(vmDisplay));
            InitializeComponent();

            if (_homeDevice.vmEzArrange == null)
            {
                _homeDevice.vmEzArrange = new DDPM.UI.Common.ViewModels.EzArrangeViewModel(_homeDevice);
            }
            _vm = vm;
            DataContext = vm;

            //Screen? currentScreen = GetAttachedScreen(_homeDevice.MonitorInfo.DisplayName);
            //_vm.IsVertical = (currentScreen != null) ? (currentScreen.Bounds.Width < currentScreen.Bounds.Height) : false;

            InitializePage();

            // 這裡排編號
            _vm.ispCtrl = ISplitCtrl.Create(_vm.SelectedSplitItem.CellCount, _vm.SelectedSplitItem.SplitKey);
            _vm.ispCtrl!.IsEditable = true;
            _vm.ispCtrl.SplitMode = eSplitModes.Em;
            EMsplitCtrl.Content = _vm.ispCtrl.UC;

            int _no = 1;
            foreach (var cellBorder in _vm.ispCtrl.CellList)
            {
                cellBorder.CellBd.CellNumber = _no;
                cellBorder.CellBd.MemoryText = _no.ToString();
                //cellBorder.CellBd.MemoryImage = _vm.ImageSource;
                _vm.AlignCellNumberAndAppName(_no, cellBorder);
                _vm?.RegisterCellBorder(cellBorder.CellBd, _no);
                _no++;
            }
        }

        /// <summary>
        /// Initialize Page, get Split window count, set string
        /// </summary>
        public void InitializePage()
        {
            //這裡加入分割視窗的個數
            if (_vm.SelectedSplitItem.CellCount == 2)
            {
                _vm.IsRightGridPage2Visible = true;
                _vm.SelectedValue = 2;
            }
            else
            {
                _vm.IsRightGridPage2Visible = false;
                _vm.SelectedValue = _vm.SelectedSplitItem.CellCount;
            }
            _vm.ezPages = _vm.GetEzPages();

            if (_vm.ezPages.ContainsKey(_vm._currentDeviceModel))
            {
                _vm.CurrentAnimationPage = _vm.ezPages[_vm._currentDeviceModel].Count;
                var pageData = _vm.ezPages[_vm._currentDeviceModel][1];
                MainText.Text = pageData.MainText!;
                SubText.Text = pageData.SubText!;
            }

            //編輯模式但不是由AddPage返回才執行
            if (_vm.IsEditProfile && !_vm.IsAddPageBack)
            {
                SyncEditStatusForAssignPage();
            }
        }

        /// <summary>
        /// Sync Edit Status 回填App Name
        /// </summary>
        public void SyncEditStatusForAssignPage()
        {
            _vm._sortApps.Clear();

            int loopCount = Math.Min(_vm.SelectedValue, _vm.currentEditprofile.AppInfos.Count);

            for (int i = 0; i < loopCount; i++)
            {
                Bind_AddFullPage_AppCollectionData newApp;// = new Bind_AddFullPage_AppCollectionData
                var appInfo = _vm.currentEditprofile.AppInfos[i];
                // 檢查 _totalApps 中是否有相同的 AppName
                var existingApp = _vm._bind_apps.FirstOrDefault(app => app.AppPath.ToUpper() == appInfo.Path.ToUpper());
                if (existingApp != null)
                {
                    //appData.AppIcon = existingApp.AppIcon;
                    newApp = new Bind_AddFullPage_AppCollectionData
                    {
                        AppName = existingApp.AppName,
                        AppPath = existingApp.AppPath,
                        AppUserModelID = existingApp.AppUserModelID,
                        AppType = existingApp.AppType,// ? "True" : "False",
                        InstalledDate = DateTime.Now,
                        AppIcon = existingApp.AppIcon
                    };
                }
                else
                {
                    //appData.AppIcon = fileName[index].Image.ToString();
                    newApp = new Bind_AddFullPage_AppCollectionData
                    {
                        AppName = appInfo.Name,
                        AppPath = appInfo.Path,
                        AppUserModelID = appInfo.AppUserModelID,
                        AppType = appInfo.IsUWP ? "True" : "False",
                        InstalledDate = DateTime.Now,
                        AppIcon = "Assets/palette.png"
                    };
                }

                string buttonName = "AddButton" + (i + 1).ToString();

                if (_vm.SelectedValue <= 2)
                {
                    buttonName = "AddButton2_" + (i + 1).ToString();
                }

                _vm.UpdateTextBlockAppName(buttonName, appInfo.Name);
                _vm._sortApps.Add(buttonName, newApp);
            }
        }

        /// <summary>
        /// Next Page
        /// </summary>
        public void NextPage()
        {
            //前進 AddPage 前設False
            _vm.IsAddPageBack = false;
            _vm._currentPageIndex++;
            EzMemoryAssignProgram _ezMemoryAssignProgram = new EzMemoryAssignProgram(_vmDisplay, _vm, _selecthomeDevice);
            DdpmCommonHelper.ModuleOwner?.OpenFullView(_ezMemoryAssignProgram);
            //UpdatePageContent();
        }

        /// <summary>
        /// Previous Page
        /// </summary>
        public void PreviousPage()
        {
            if (_vm._currentPageIndex > 0)
            {
                _vm._currentPageIndex--;
                UpdatePageContent();
            }
        }

        /// <summary>
        /// Update Page Content
        /// </summary>
        public void UpdatePageContent()
        {
            if (_vm._currentPageIndex >= 3)
            {
                CancelBtn_Click(null!, null!);
            }
            var pageData = _vm.ezPages["EzMemory"][_vm._currentPageIndex];
            MainText.Text = pageData.MainText!;
            SubText.Text = pageData.SubText!;
        }

        /// <summary>
        /// Back
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ArrowButton_Click(object sender, RoutedEventArgs e)
        {
            _vm.ClearTextBlockAppName();
            //_vm.IsEditProfile = true;// 從Aassign退回First
            EzMemoryFirst ezMemoryFirst = new EzMemoryFirst(_vmDisplay, _vm, _selecthomeDevice);
            DdpmCommonHelper.ModuleOwner?.OpenFullView(ezMemoryFirst);
        }

        /// <summary>
        /// Next
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NextBtn_Click(object sender, RoutedEventArgs e)
        {
            _vm._currentPageIndex++;
            EzMemoryLaunchOption _ezMemoryLaunchOption = new EzMemoryLaunchOption(_vmDisplay, _vm, _selecthomeDevice);
            DdpmCommonHelper.ModuleOwner?.OpenFullView(_ezMemoryLaunchOption);
        }

        /// <summary>
        /// Cancel
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            _vm.ProgressValue = 1;
            DdpmCommonHelper.ModuleOwner?.CloseFullView();
            return;
        }

        /// <summary>
        /// Do Progress Animation
        /// </summary>
        /// <param name="isForward"></param>
        private void DoProgressAnimation(bool isForward)
        {
            double newProgressValue;
            if (isForward)
            {
                // Move
                newProgressValue = Math.Min(_vm.ProgressValue + 1, _vm.CurrentAnimationPage);
            }
            else
            {
                // Back
                newProgressValue = Math.Max(_vm.ProgressValue - 1, 1);
            }

            DoubleAnimation progressAnimation = new DoubleAnimation
            {
                From = _vm.ProgressValue,
                To = newProgressValue,
                Duration = new Duration(TimeSpan.FromSeconds(0.5)), // Time
                FillBehavior = FillBehavior.HoldEnd
            };

            EzMemoryProgressbar.BeginAnimation(ProgressBar.ValueProperty, progressAnimation);

            // refresh ProgressValue
            _vm.ProgressValue = newProgressValue;
        }

        /// <summary>
        /// Add application Button1 Click
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AddButton1_Click(object sender, RoutedEventArgs e)
        {
            if (sender is System.Windows.Controls.Button button)
            {
                string buttonName = button.Name;
                var image = button.Template.FindName("PART_Image", button) as Image;

                if (image != null)
                {
                    string source = image.Source.ToString();
                    string imageState = source.Contains("EzAdd.png") ? "EzAdd" : "EzRemove";

                    if (imageState == "EzRemove")
                    {
                        if(buttonName == "AddButton2_1" || buttonName == "AddButton1")
                        {
                            _vm.UpdateTextBlockAppName("AddButton2_1", "");
                            _vm.UpdateTextBlockAppName("AddButton1", "");
                            _vm._sortApps.Remove("AddButton2_1");
                            _vm._sortApps.Remove("AddButton1");
                        }
                        else if (buttonName == "AddButton2_2" || buttonName == "AddButton2")
                        {
                            _vm.UpdateTextBlockAppName("AddButton2_2", "");
                            _vm.UpdateTextBlockAppName("AddButton2", "");
                            _vm._sortApps.Remove("AddButton2_2");
                            _vm._sortApps.Remove("AddButton2");
                        }
                        else
                        {
                            _vm.UpdateTextBlockAppName(buttonName, "");
                            _vm._sortApps.Remove(buttonName);
                        }

                        int cellno = _vm.GetTextBlockNumber(buttonName);

                        int _no = 1;
                        foreach (var cellBorder in _vm.ispCtrl.CellList)
                        {
                            if(_no == cellno)
                            {
                                cellBorder.CellBd.CellNumber = _no;
                                cellBorder.CellBd.MemoryText = _no.ToString();
                                cellBorder.CellBd.MemoryImage = null;
                                //_vm.AlignCellNumberAndAppName(_no, cellBorder);
                            }
                            _no++;
                        }
                        return;
                    }
                    else
                    {
                        _vm.ButtonName = button.Name;
                        EzMemoryAddApplication _ezMemoryAddApplication = new EzMemoryAddApplication(_vmDisplay, _vm, _selecthomeDevice);
                        DdpmCommonHelper.ModuleOwner?.OpenFullView(_ezMemoryAddApplication);
                    }
                }

            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            _vm._bind_apps.Clear();
            _vm._apps_all.Clear();
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

                _vm._bind_apps.Add(new_Appdata);
                _vm._apps_all.Add(new_Appdata);

            }
        }
    }

    /// <summary>
    /// Binding change value
    /// </summary>
    public class WindowGridVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int selectedValue && parameter is string gridIndexString && int.TryParse(gridIndexString, out int gridIndex))
            {
                // 如果 selectedValue 大於等於 gridIndex，則顯示 (Visible)，否則隱藏 (Collapsed)
                return selectedValue >= gridIndex ? Visibility.Visible : Visibility.Collapsed;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
