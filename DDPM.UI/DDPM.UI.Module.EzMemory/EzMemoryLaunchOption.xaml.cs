using DDPM.SA.Common;
using DDPM.SA.Common.Settings;
using DDPM.UI.Common;
using DDPM.UI.Common.Models;
using DDPM.UI.Plugin.Common.ViewModels;
using Dell.Client.Framework.Common;
using Dell.Client.Framework.UX.WPF;
using System;
using System.Collections.Generic;
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
using System.Diagnostics;
using System.Runtime.InteropServices;
using Windows.Management.Deployment;
using Microsoft.VisualBasic.Logging;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using Windows.ApplicationModel;
using VcpCore.Common;
using DDPM.UI.Common.UserControls;
using DDPM.Easy.Common;

namespace DDPM.UI.Module.EzMemory
{
    /// <summary>
    /// EzMemoryLaunchOption.xaml 的互動邏輯
    /// </summary>
    public partial class EzMemoryLaunchOption : UserControl
    {
        #region Private Members
        private HomeDevice _homeDevice;
        private IDeviceManagerSA _deviceManagerSA;
        private DDPM.UI.Common.ViewModels.EzArrangeViewModel _vm;
        private readonly DisplayViewModel _vmDisplay;
        private readonly IConsole _console;
        private readonly ILog _log;
        #endregion Private Members

        public EzMemoryLaunchOption(DisplayViewModel vmDisplay)
        {
            _vmDisplay = vmDisplay;
            _homeDevice = vmDisplay.SelectedHomeDevice;
            _console = vmDisplay.Console;
            _deviceManagerSA = HomeDevice.DeviceManagerSA;
            _log = vmDisplay.Console.CreateLog("EzMemoryLaunchOption");
            _log.Info($"{nameof(EzMemoryLaunchOption)} - Constructed");
            InitializeComponent();

            if (_homeDevice.vmEzArrange == null)
            {
                _homeDevice.vmEzArrange = new DDPM.UI.Common.ViewModels.EzArrangeViewModel(_homeDevice);
            }
            _vm = _homeDevice.vmEzArrange;
            DataContext = _homeDevice.vmEzArrange;

            InitializePage();

            _vm.ProgressValue = 3;
        }

        /// <summary>
        /// Initialize Page
        /// </summary>
        public void InitializePage()
        {
            TitleTB.Text = Strings.TitleTBForLaunchOptionPage;
            StartupCB.Content = Strings.StartupCBContentForLaunchOptionPage;
            ManulRB.Content = Strings.ManulRBContentForLaunchOptionPage;
            AutoRB.Content = Strings.AutoRBContentForLaunchOptionPage;
            _vm.ezPages = _vm.GetEzPages();

            if (_vm.ezPages.ContainsKey(_vm._currentDeviceModel))
            {
                _vm.CurrentAnimationPage = _vm.ezPages[_vm._currentDeviceModel].Count;
                var pageData = _vm.ezPages[_vm._currentDeviceModel][2];
                MainText.Text = pageData.MainText!;
                SubText.Text = pageData.SubText!;
            }
        }

        /// <summary>
        /// Back
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ArrowButton_Click(object sender, RoutedEventArgs e)
        {
            _vm.ProgressValue = 2;
            EzMemoryAssignProgram _ezMemoryAssignProgram = new EzMemoryAssignProgram(_vmDisplay);
            DdpmCommonHelper.ModuleOwner?.OpenFullView(_ezMemoryAssignProgram);
        }

        /// <summary>
        /// Finish Btn, add profile to spilt item
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FinishBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // 只有當 _sortApps 有兩個或以上的項目時才進行處理
                if (_vm._sortApps.Count >= 2)
                {
                    #region User Settings

                    // 讀取 User 的 EasyArrangement Profile
                    int profileID = 0;
                    string profileName = _vm.InputText;
                    // 重要!!!把分割數量與SplitKey代號轉換成layout
                    int layout = _vm.ConvertToLayout(_vm.SelectedSplitItem.CellCount, _vm.SelectedSplitItem.SplitKey);
                    int _number = 0;
                    // 取得現有的 EAProfile
                    List<EAProfileDDPM> newEAProfileDDPM = DdpmCommonHelper.DeviceManagerSA.ReadUserEAProfileDDPM().Result;
                    if (newEAProfileDDPM != null)
                    {
                        // 計算已有的 profile 數量
                        List<int> existingIds = newEAProfileDDPM.Select(p => p.ID).ToList();

                        // 找到缺的最小ID
                        profileID = Enumerable.Range(0, 9).Except(existingIds).FirstOrDefault();

                        if (profileID == -1)
                        {
                            _log.Error($"@{nameof(EzMemoryLaunchOption)} No available ID for new profile.");
                            return;
                        }
                    }

                    // 收集 App 資訊並準備添加到新的 EAProfile 中
                    List<EAAppInfoDDPM> _appInfos = new List<EAAppInfoDDPM>();
                    foreach (var app in _vm._sortApps.Values)
                    {
                        _appInfos.Add(new EAAppInfoDDPM(app.AppName, app.AppPath, bool.Parse(app.AppType), app.AppUserModelID, String.Empty));
                    }

                    // 建立新的 EAProfile 物件
                    EAProfileDDPM _eAProfileDDPM = new EAProfileDDPM(
                        id: profileID,
                        name: profileName,
                        layout: layout,
                        apps: _appInfos);

                    // 將新的 EAProfile 寫入 User Settings
                    if (DdpmCommonHelper.DeviceManagerSA.WriteUserEAProfileDDPM(_eAProfileDDPM).Result)
                    {
                        ISplitCtrl? sp0A = ISplitCtrl.Create(_vm.SelectedSplitItem.CellCount, _vm.SelectedSplitItem.SplitKey);
                        SplitItem item0A;
                        sp0A.FriendlyName = "Off"; //Need Multilogual support
                        sp0A.SplitMode = eSplitModes.Icon;
                        item0A = _vm.splitListRightView.AddItemToList(sp0A.UC);
                        item0A.SplitOwner = Common.EAEM.eSplitOwner.EaRecent;
                        item0A.CustomId = _eAProfileDDPM.ID;
                        item0A.IsHoverable = true;
                        item0A.IsDeleteEnabled = true;
                        item0A.IsEditEnabled = true;
                        item0A.LayoutID = _eAProfileDDPM.ID;

                        _log.Info($"@{nameof(EzMemoryLaunchOption)} FinishBtn_Click: User Settings PASS");
                    }
                    else
                    {
                        _log.Info($"@{nameof(EzMemoryLaunchOption)} FinishBtn_Click: User Settings FAIL");
                    }

                    #endregion

                    #region Monitor Settings

                    // 計算 AutoStartTime
                    long autoLaunchtime = default(long);
                    if (_vm.IsAutoLaunch)
                    {
                        int hour = int.TryParse(_vm.SelectedHour, out var h) ? h : 0;
                        int minute = int.TryParse(_vm.SelectedMinute, out var m) ? m : 0;
                        autoLaunchtime = (long)(hour * 3600 + minute * 60); // 將小時和分鐘轉換為秒數
                    }

                    EasyArrangementDDPM _easyArrangementDDPM = DdpmCommonHelper.DeviceManagerSA.ReadMonitorEasyArrangement(_homeDevice.MonitorInfo).Result;

                    // 檢查 _easyArrangementDDPM 是否為 null，如果是，則new
                    if (_easyArrangementDDPM == null)
                    {
                        _log.Info($"@{nameof(EzMemoryLaunchOption)} ReadMonitorEasyArrangement returned null. Initializing a new EasyArrangementDDPM.");
                        _easyArrangementDDPM = new EasyArrangementDDPM();
                    }

                    // new EasyArrangement 設定
                    EzProfileSettingDDPM _ezProfileSettingDDPM = new EzProfileSettingDDPM(
                        id: profileID,
                        auto: _vm.IsAutoLaunch,
                        autostarttime: autoLaunchtime,
                        startuplaunch: _vm.IsLaunchAtStartup
                    );

                    // 檢查Desktops，如果沒有就新增一個
                    if (_easyArrangementDDPM.Desktops == null || _easyArrangementDDPM.Desktops.Count == 0)
                    {
                        DesktopDDPM _desktopDDPM = new DesktopDDPM(string.Empty, 0);
                        _desktopDDPM.ProfileSettings.Add(_ezProfileSettingDDPM);
                        _easyArrangementDDPM.Desktops = new List<DesktopDDPM> { _desktopDDPM };
                    }
                    else
                    {
                        _easyArrangementDDPM.Desktops[0].ProfileSettings.Add(_ezProfileSettingDDPM);
                    }

                    // 將更新寫回
                    if (DdpmCommonHelper.DeviceManagerSA.WriteMonitorEasyArrangement(_homeDevice.MonitorInfo, _easyArrangementDDPM).Result)
                    {
                        _log.Info($"@{nameof(EzMemoryLaunchOption)} FinishBtn_Click: Monitor Settings PASS");
                    }
                    else
                    {
                        _log.Info($"@{nameof(EzMemoryLaunchOption)} FinishBtn_Click: Monitor Settings FAIL");
                    }

                    #endregion
                    //LaunchAndArrangeApps(_vm._sortApps);
                    //_deviceManagerSA.LaunchAndArrangeApps(_vm._sortApps);
                }

                // 清除TextBlock
                _vm.ClearTextBlockAppName();

                DdpmCommonHelper.ModuleOwner?.CloseFullView();
            }
            catch (Exception ex)
            {
                _log.Error($"@{nameof(EzMemoryLaunchOption)} FinishBtn_Click: Error occurred - {ex.Message}");
            }
        }

        /// <summary>
        /// Cancel
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            DdpmCommonHelper.ModuleOwner?.CloseFullView();
        }

        /// <summary>
        /// When 'Launch during PC startup' is checked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StartupCB_Checked(object sender, RoutedEventArgs e)
        {
            DdpmCommonHelper.DDPMMesssageBox(Strings.ezMemoryStartupErrorTitleStringForLaunchOptionPage, Strings.ezMemoryStartupErrorStringForLaunchOptionPage);
        }
    }
}
