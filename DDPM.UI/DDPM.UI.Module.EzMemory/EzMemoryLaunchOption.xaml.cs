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
using DDPM.UI.Common.ViewModels;
using static System.Reflection.Metadata.BlobBuilder;
using System.Globalization;

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
        private HomeDevice _selecthomeDevice;
        #endregion Private Members

        public EzMemoryLaunchOption(DisplayViewModel vmDisplay, EzArrangeViewModel vm, HomeDevice _homeDeviceSelect)
        {
            _vmDisplay = vmDisplay;
            _homeDevice = vmDisplay.SelectedHomeDevice;
            _console = vmDisplay.Console;
            _deviceManagerSA = HomeDevice.DeviceManagerSA;
            _selecthomeDevice = _homeDeviceSelect;
            _log = vmDisplay.Console.CreateLog("EzMemoryLaunchOption");
            _log.Info($"{nameof(EzMemoryLaunchOption)} - Constructed");
            InitializeComponent();

            if (_homeDevice.vmEzArrange == null)
            {
                _homeDevice.vmEzArrange = new DDPM.UI.Common.ViewModels.EzArrangeViewModel(_homeDevice);
            }
            _vm = vm;// _homeDevice.vmEzArrange;
            DataContext = vm;// _homeDevice.vmEzArrange;

            InitializePage();

            _vm.ProgressValue = 3;
        }

        /// <summary>
        /// Initialize Page
        /// </summary>
        public void InitializePage()
        {
            try
            {
                _log.Error($"@{nameof(EzMemoryLaunchOption)} InitializePage: ... in");

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

                if (_vm.IsEditProfile)
                {
                    _vm.IsLaunchAtStartup = _vm.currentEditprofileSetting.StartUpLaunch;
                    if (_vm.currentEditprofileSetting.Auto)
                    {
                        _vm.IsAutoLaunch = true;
                        _vm.IsManualLaunch = false;
                    }
                    else
                    {
                        _vm.IsAutoLaunch = false;
                        _vm.IsManualLaunch = true;
                    }

                    long autoStartTimeInSeconds = (long)_vm.currentEditprofileSetting.AutoStartTime!;
                    TimeSpan time = TimeSpan.FromSeconds(autoStartTimeInSeconds);

                    // 使用 CultureInfo 來取得 AM 和 PM
                    string amDesignator = CultureInfo.CurrentCulture.DateTimeFormat.AMDesignator;
                    string pmDesignator = CultureInfo.CurrentCulture.DateTimeFormat.PMDesignator;

                    int hourValue = time.Hours;
                    if (hourValue == 0)
                    {
                        _vm.SelectedHour = "12";
                        _vm.SelectedAMPM = amDesignator; // AM
                    }
                    else if (hourValue >= 12)
                    {
                        _vm.SelectedAMPM = pmDesignator; // PM
                        if (hourValue > 12)
                        {
                            _vm.SelectedHour = (hourValue - 12).ToString("D2");
                        }
                        else
                        {
                            _vm.SelectedHour = "12";
                        }
                    }
                    else
                    {
                        _vm.SelectedAMPM = amDesignator; // AM
                        _vm.SelectedHour = hourValue.ToString("D2");
                    }

                    _vm.SelectedMinute = time.Minutes.ToString("D2");
                }
                else
                {
                    DateTime now = DateTime.Now;

                    string ampm = now.Hour >= 12
                        ? CultureInfo.CurrentCulture.DateTimeFormat.PMDesignator
                        : CultureInfo.CurrentCulture.DateTimeFormat.AMDesignator;

                    string hour = now.ToString("hh");
                    string minute = now.ToString("mm");

                    _vm.SelectedHour = hour;
                    _vm.SelectedMinute = minute;
                    _vm.SelectedAMPM = ampm;

                    _vm.IsLaunchAtStartup = false;
                }
            }
            catch (Exception ex)
            {
                _log.Error($"@{nameof(EzMemoryLaunchOption)} InitializePage: Error occurred - {ex.Message}");
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
            EzMemoryAssignProgram _ezMemoryAssignProgram = new EzMemoryAssignProgram(_vmDisplay, _vm, _selecthomeDevice);
            DdpmCommonHelper.ModuleOwner?.OpenFullView(_ezMemoryAssignProgram);
        }

        private void FinishBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _log.Info($"@{nameof(EzMemoryLaunchOption)} FinishBtn_Click: ... in");

                CheckedAutoLunchTime();

                // Determine if adding a new profile or editing an existing one
                bool isEditMode = _vm.IsEditProfile;
                int profileID = isEditMode ? _vm.currentEditprofile.ID : GetNewProfileID();
                string profileName = _vm.InputText;
                //int layout = _vm.ConvertToLayout(_vm.SelectedSplitItem.CellCount, _vm.SelectedSplitItem.SplitKey);
                int layout = _vm.ispCtrlForEm!.EAID;
                if (_vm._sortApps.Count < 2)
                    return; // Proceed only if there are two or more apps

                _vm._sortApps = _vm.SortAppsByTextBlockNumber(_vm._sortApps);
                // Collect App Info
                List<EAAppInfoDDPM> _appInfos = _vm._sortApps.Values.Select(app => new EAAppInfoDDPM(
                    app.AppName, app.AppPath, bool.Parse(app.AppType), app.AppUserModelID, string.Empty)).ToList();

                // Create the EAProfile object
                EAProfileDDPM _eAProfileDDPM = new EAProfileDDPM(profileID, profileName, layout, _appInfos);

                // Handle User Settings
                bool userSettingsSuccess = isEditMode
                    ? DdpmCommonHelper.DeviceManagerSA.UpdateUserEAProfileDDPM(_selecthomeDevice.MonitorInfo, _eAProfileDDPM).Result
                    : DdpmCommonHelper.DeviceManagerSA.WriteUserEAProfileDDPM(_selecthomeDevice.MonitorInfo, _eAProfileDDPM).Result;

                if (userSettingsSuccess)
                {
                    UpdateSplitListUI(profileID, _vm.ispCtrlForEm, isEditMode);

                    _log.Info($"@{nameof(EzMemoryLaunchOption)} FinishBtn_Click: User Settings PASS");
                }
                else
                {
                    _log.Info($"@{nameof(EzMemoryLaunchOption)} FinishBtn_Click: User Settings FAIL");
                    return;
                }

                // Handle Monitor Settings
                if(_vm.IsAutoLaunch)
                {
                    if(_vm.SelectedHour == string.Empty || _vm.SelectedHour == string.Empty || _vm.SelectedAMPM == string.Empty) return;
                }
                if(_vm.IsLaunchAtStartup)
                {
                    _log.Info($"@{nameof(EzMemoryLaunchOption)} _vm.IsLaunchAtStartup True ");

                    EasyArrangementDDPM clickedeasyArrangementDDPM = DdpmCommonHelper.DeviceManagerSA.ReadMonitorEasyArrangement(_selecthomeDevice.MonitorInfo).Result;

                    if (clickedeasyArrangementDDPM != null && clickedeasyArrangementDDPM.Desktops.Count > 0)
                    {
                        foreach (var ps in clickedeasyArrangementDDPM.Desktops[0].ProfileSettings)
                        {
                            if (ps.StartUpLaunch)
                            {
                                ps.StartUpLaunch = false;
                                if(DdpmCommonHelper.DeviceManagerSA.UpdateMonitorEzProfileSettingDDPM(_selecthomeDevice.MonitorInfo, ps).Result)
                                {
                                    _log.Info($"@{nameof(EzMemoryLaunchOption)} _vm.IsLaunchAtStartup update success ");
                                }
                            }
                        }
                    }
                    else
                    {
                        _log.Info($"@{nameof(EzMemoryLaunchOption)} _vm.IsLaunchAtStartup update error ");
                    }

                }

                long autoLaunchTime = _vm.IsAutoLaunch ? GetAutoLaunchTime() : default;
                EzProfileSettingDDPM _ezProfileSettingDDPM = new EzProfileSettingDDPM(profileID, _vm.IsAutoLaunch, autoLaunchTime, _vm.IsLaunchAtStartup);

                bool monitorSettingsSuccess = isEditMode
                    ? DdpmCommonHelper.DeviceManagerSA.UpdateMonitorEzProfileSettingDDPM(_selecthomeDevice.MonitorInfo, _ezProfileSettingDDPM).Result
                    : HandleMonitorEasyArrangement(_selecthomeDevice.MonitorInfo, _ezProfileSettingDDPM);

                if (monitorSettingsSuccess)
                {
                    _log.Info($"@{nameof(EzMemoryLaunchOption)} FinishBtn_Click: Monitor Settings PASS");
                }
                else
                {
                    _log.Info($"@{nameof(EzMemoryLaunchOption)} FinishBtn_Click: Monitor Settings FAIL");
                }

                // Clear UI and close view
                _vm.ClearTextBlockAppName();
                _vm.IsEditProfile = false;
                DdpmCommonHelper.ModuleOwner?.CloseFullView();
            }
            catch (Exception ex)
            {
                _log.Error($"@{nameof(EzMemoryLaunchOption)} FinishBtn_Click: Error occurred - {ex.Message}");
            }
        }

        private int GetNewProfileID()
        {
            List<EAProfileDDPM> existingProfiles = DdpmCommonHelper.DeviceManagerSA.ReadUserEAProfileDDPM().Result;
            if (existingProfiles == null) return 0;

            List<int> existingIds = existingProfiles.Select(p => p.ID).ToList();
            return Enumerable.Range(0, 9).Except(existingIds).FirstOrDefault();
        }

        private long GetAutoLaunchTime()
        {
            int hour = int.TryParse(_vm.SelectedHour, out var h) ? h : 0;
            int minute = int.TryParse(_vm.SelectedMinute, out var m) ? m : 0;

            // PM
            if (_vm.SelectedAMPM == "PM")
            {
                hour += 12;
            }
            // AM
            else if (_vm.SelectedAMPM == "AM" && hour == 12)
            {
                hour = 0;
            }

            // 轉換為秒
            return (long)(hour * 3600 + minute * 60);
        }

        private bool HandleMonitorEasyArrangement(MonitorInfo monitorInfo, EzProfileSettingDDPM ezProfileSetting)
        {
            try
            {
                _log.Info($"[EzMemoryLaunchOption] HandleMonitorEasyArrangement ... in");

                EasyArrangementDDPM easyArrangement = DdpmCommonHelper.DeviceManagerSA.ReadMonitorEasyArrangement(monitorInfo).Result;
                if (easyArrangement == null)
                {
                    _log.Info($"@{nameof(EzMemoryLaunchOption)} ReadMonitorEasyArrangement returned null. Initializing a new EasyArrangementDDPM.");
                    easyArrangement = new EasyArrangementDDPM();
                }

                if (easyArrangement.Desktops == null || easyArrangement.Desktops.Count == 0)
                {
                    DesktopDDPM newDesktop = new DesktopDDPM(string.Empty, 0);
                    newDesktop.ProfileSettings.Add(ezProfileSetting);
                    easyArrangement.Desktops = new List<DesktopDDPM> { newDesktop };
                }
                else
                {
                    easyArrangement.Desktops[0].ProfileSettings.Add(ezProfileSetting);
                }

                return DdpmCommonHelper.DeviceManagerSA.WriteMonitorEasyArrangement(monitorInfo, easyArrangement).Result;
            }
            catch (Exception ex)
            {
                _log.Error($"[EzMemoryLaunchOption] HandleMonitorEasyArrangement Exception occurred: {ex.Message}");
                return false;
            }
        }

        //Robert_Lin, 2024-11-19 Change to pass ISpitCtrl (origial) into this method
        //private void UpdateSplitListUI(int profileID, int layout, bool isEditMode)
        private void UpdateSplitListUI(int profileID, ISplitCtrl ispAdd, bool isEditMode)
        {
            try
            {
                _log.Info("[EzMemoryLaunchOption] UpdateSplitListUI  ... in");
                //1 Duplicate a ISplitCtrl from current editing/adding
                ISplitCtrl? ispNew = ispAdd.Clone();
                ispNew.SplitMode = eSplitModes.Icon;

                //If it's Edit mode, then replace current edit selected item with ispNew
                if (isEditMode)
                {
                    _log.Info("[EzMemoryLaunchOption] UpdateSplitListUI  ... isEditMode True");
                    SplitItem? spItem = _vm.splitListRightView.FindItemByCustomId(ispAdd.EAID);
                    if (spItem != null)
                    {
                        spItem.ReplaceWithISplitICtrl(ispNew);
                    }
                }
                else
                {
                    _log.Info("[EzMemoryLaunchOption] UpdateSplitListUI  ... isEditMode False");
                    //Add new item into SplitListView
                    SplitItem newItem = _vm.splitListRightView.AddItemToList(ispNew.UC);
                    newItem.SplitOwner = Common.EAEM.eSplitOwner.EaRecent;
                    newItem.ProfileID = profileID;
                    newItem.IsHoverable = true;
                    newItem.IsDeleteEnabled = true;
                    newItem.IsEditEnabled = true;
                    newItem.LayoutID = ispNew.EAID;
                }

                /* OLD Code by Wayn 
                //ISplitCtrl? splitCtrl = ISplitCtrl.Create(_vm.SelectedSplitItem.CellCount, _vm.SelectedSplitItem.SplitKey);
                ISplitCtrl? splitCtrl = ISplitCtrl.Create(layout);
                splitCtrl.FriendlyName = "Off"; // Need multilingual support
                splitCtrl.SplitMode = eSplitModes.Icon;
                
                SplitItem newItem = _vm.splitListRightView.AddItemToList(splitCtrl.UC);
                newItem.SplitOwner = Common.EAEM.eSplitOwner.EaRecent;
                newItem.ProfileID = profileID;
                newItem.IsHoverable = true;
                newItem.IsDeleteEnabled = true;
                newItem.IsEditEnabled = true;
                newItem.LayoutID = layout;
                
                if (isEditMode)
                {
                    _vm.splitListRightView.DeleteSplitItem(_vm.CurrentEditSelectspItem);
                }
                */
                //NEW code by Robert

            }
            catch (Exception ex)
            {
                _log.Error($"[EzMemoryLaunchOption] UpdateSplitListUI Exception occurred: {ex.Message}");
            }

        }

        /// <summary>
        /// Cancel
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            _vm.ClearTextBlockAppName();
            DdpmCommonHelper.ModuleOwner?.CloseFullView();
        }

        /// <summary>
        /// When 'Launch during PC startup' is checked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StartupCB_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                _log.Info("[EzMemoryLaunchOption] StartupCB_Checked ... in");

                EasyArrangementDDPM clickedeasyArrangementDDPM = DdpmCommonHelper.DeviceManagerSA.ReadMonitorEasyArrangement(_selecthomeDevice.MonitorInfo).Result;

                if (clickedeasyArrangementDDPM != null && clickedeasyArrangementDDPM.Desktops.Count > 0)
                {
                    foreach (var ps in clickedeasyArrangementDDPM.Desktops[0].ProfileSettings)
                    {
                        if (ps.StartUpLaunch)
                        {
                            if (_vm.currentEditprofileSetting == null || _vm.currentEditprofileSetting.ID != ps.ID)
                            {
                                if (DdpmCommonHelper.DDPMMesssageBox(Strings.ezMemoryStartupErrorTitleStringForLaunchOptionPage, Strings.ezMemoryStartupErrorStringForLaunchOptionPage))
                                {
                                    _vm.IsLaunchAtStartup = true;
                                    //break;
                                }
                                else
                                {
                                    _vm.IsLaunchAtStartup = false;
                                }
                            }
                        }
                    }
                }
                else
                {
                    _log.Info("[EzMemoryLaunchOption] StartupCB_Checked  can not found StartUp Launch = True file");
                }
            }
            catch (Exception ex)
            {
                _log.Error($"[EzMemoryLaunchOption] StartupCB_Checked Exception occurred: {ex.Message}");
            }
        }

        private void CheckedAutoLunchTime()
        {
            try
            {
                _log.Info("[EzMemoryLaunchOption] AutoRB_Checked ... in");

                EasyArrangementDDPM clickedeasyArrangementDDPM = DdpmCommonHelper.DeviceManagerSA.ReadMonitorEasyArrangement(_selecthomeDevice.MonitorInfo).Result;

                if (clickedeasyArrangementDDPM != null && clickedeasyArrangementDDPM.Desktops.Count > 0)
                {
                    long autoLaunchTime = _vm.IsAutoLaunch ? GetAutoLaunchTime() : default;

                    foreach (var ps in clickedeasyArrangementDDPM.Desktops[0].ProfileSettings)
                    {
                        if (ps.AutoStartTime == autoLaunchTime && autoLaunchTime !=0)
                        {
                            if (_vm.currentEditprofileSetting == null || _vm.currentEditprofileSetting.ID != ps.ID)
                            {
                                if (DdpmCommonHelper.DDPMMesssageBox(Strings.ezMemoryStartupErrorTitleStringForLaunchOptionPage, Strings.ezMemoryAutoLaunchErrorStringForLaunchOptionPage))
                                {
                                    _vm.IsLaunchAtStartup = true;
                                    //break;
                                }
                                else
                                {
                                    _vm.IsLaunchAtStartup = false;
                                }
                            }
                        }
                    }
                }
                else
                {
                    _log.Info("[EzMemoryLaunchOption] AutoRB_Checked  can not found Auto Launch By Time = True file");
                }
            }
            catch (Exception ex)
            {
                _log.Error($"[EzMemoryLaunchOption] AutoRB_Checked Exception occurred: {ex.Message}");
            }
        }
    }
}
