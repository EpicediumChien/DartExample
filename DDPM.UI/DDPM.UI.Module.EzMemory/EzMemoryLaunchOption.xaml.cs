using DDPM.SA.Common;
using DDPM.SA.Common.Settings;
using DDPM.UI.Common;
using DDPM.UI.Common.Models;
using DDPM.UI.Plugin.Common.ViewModels;
using Dell.Client.Framework.Common;
using Dell.Client.Framework.UX.WPF;
using System.Windows;
using System.Windows.Controls;
using VcpCore.Common;
using DDPM.UI.Common.UserControls;
using DDPM.Easy.Common;
using DDPM.UI.Common.ViewModels;
using System.Globalization;
using System.Data;
//using System.Windows.Forms;

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
            Application.Current.MainWindow.SizeChanged -= MainWindow_SizeChanged;
            Application.Current.MainWindow.SizeChanged += MainWindow_SizeChanged;
        }

        private void MainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (Application.Current.MainWindow.ActualHeight > 765)
            {
                MainPanel.VerticalAlignment = VerticalAlignment.Center;
            }
            else
            {
                MainPanel.VerticalAlignment = VerticalAlignment.Top;
            }
        }

        ~EzMemoryLaunchOption()
        {
            Application.Current.MainWindow.SizeChanged -= MainWindow_SizeChanged;
        }

        /// <summary>
        /// Initialize Page
        /// </summary>
        public void InitializePage()
        {
            try
            {
                _log.Error($"@{nameof(EzMemoryLaunchOption)} InitializePage: ... in");

                //string lorem = "Lorem ipsum dolor sit amet, consectetur adipiscing eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation";
                //string lorem2 = "Lorem ipsum dolor sit amet, consectetur adipiscing eiusmod";

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

                //Robert_Lin 2025-1-13 Based on DDM behavior, every time when entering this page,
                // the settings will be reloaded from per-monitor settings file.
                //Even if user has changed the settings, Back to Assign programs, then "Next" return to here.
                //(Changed settings will be lost, and will be reloaded from settings file)
                //
                //In Add mode, the new ProfileId will be generted when clicking Finish button.
                //             So the per-monitor settings will be always null. we can skip it (to restore)
                //But in Edit mode, we can restore its monitor settings with current editing profileId.
                // 使用 CultureInfo 來取得 AM 和 PM
                string amDesignator = Strings.Am;//CultureInfo.CurrentCulture.DateTimeFormat.AMDesignator;
                string pmDesignator = Strings.Pm;//CultureInfo.CurrentCulture.DateTimeFormat.PMDesignator;

                //Add mode: Always reset to defaults
                if (!_vm.IsEditProfile)
                {
                    _vm.IsManualLaunch = true;
                    _vm.IsAutoLaunch = false;
                    _vm.IsLaunchAtStartup = false;

                    //Always use current time
                    DateTime now = DateTime.Now;
                    string hour = now.ToString("hh");
                    string minute = now.ToString("mm");
                    string ampm = now.Hour >= 12 ? pmDesignator : amDesignator;

                    _vm.SelectedHour = hour;
                    _vm.SelectedMinute = minute;
                    _vm.SelectedAMPM = ampm;
                    return;
                }

                //Edit mode: Always restore from per-monitor settings
                _vm.currentEditprofileSetting = _vm.LoadEmMonitorSettings(_vm.currentEditprofile.ID);
                //If no per-monitor settings for current profile.ID, then assign default settings (Manual/Current Time)
                if (_vm.currentEditprofileSetting == null)
                {
                    _vm.IsManualLaunch = true;
                    _vm.IsAutoLaunch = false;
                    _vm.IsLaunchAtStartup = false;

                    //Always use current time
                    DateTime now = DateTime.Now;
                    string hour = now.ToString("hh");
                    string minute = now.ToString("mm");
                    string ampm = now.Hour >= 12 ? pmDesignator : amDesignator;

                    _vm.SelectedHour = hour;
                    _vm.SelectedMinute = minute;
                    _vm.SelectedAMPM = ampm;
                    return;
                }

                //Has saved settings, then apply the settings
                _vm.IsManualLaunch = !_vm.currentEditprofileSetting.Auto;
                _vm.IsAutoLaunch = _vm.currentEditprofileSetting.Auto;

                if (_vm.currentEditprofileSetting.Auto)
                {
                    //Use the Time from currentEditprofileSetting
                    long autoStartTimeInSeconds = (long)_vm.currentEditprofileSetting.AutoStartTime!;

                    //Robert_Lin 2025-4023 fix by referencing EzArrangeViewModel.ConvertAutoLaunchtimeToTime()
                    //OLD:
                    //TimeSpan timeSpan = TimeSpan.FromSeconds(autoStartTimeInSeconds);
                    //
                    //NEW:
                    double totalSeconds = 0;
                    if (autoStartTimeInSeconds > 1000000)
                        totalSeconds = (double)autoStartTimeInSeconds / 10000000; // Corrected conversion for DDM
                    else
                        totalSeconds = (double)autoStartTimeInSeconds;
                    TimeSpan timeSpan = TimeSpan.FromSeconds(totalSeconds);
                    //
                    ///////////////
                    

                    int hourValue = timeSpan.Hours;
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
                    //Robert_Lin 20205-4-9 add minitues
                    _vm.SelectedMinute = timeSpan.Minutes.ToString("D2");
                }
                else
                {
                    _vm.IsManualLaunch = true;
                    _vm.IsAutoLaunch = false;
                    _vm.IsLaunchAtStartup = false;

                    //Always use current time
                    DateTime now = DateTime.Now;
                    string hour = now.ToString("hh");
                    string minute = now.ToString("mm");
                    string ampm = now.Hour >= 12 ? pmDesignator : amDesignator;

                    _vm.SelectedHour = hour;
                    _vm.SelectedMinute = minute;
                    _vm.SelectedAMPM = ampm;
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

                if (_vm.IsAutoLaunch &&
                    !CheckedAutoLunchTime())
                {
                    DDPM.SA.Common.Popup.PopupBase popBase = new DDPM.SA.Common.Popup.PopupBase(
                        Strings.ezMemoryStartupErrorTitleStringForLaunchOptionPage,
                        Strings.ezMemoryAutoLaunchErrorStringForLaunchOptionPage,
                        "",
                        Strings.OK,
                        null, 
                        false, 
                        0,
                        "C");
                    popBase.Owner = System.Windows.Application.Current.MainWindow;
                    bool? popResult = popBase.ShowDialog();
                    //popResult: Close=null; LeftButton=false; RightButton=true
                    if (popResult != true)
                    {
                        _log.Info("[EzMemoryLaunchOption] AutoLunchTime_Checked ... chooice No");
                        return;
                    }
                    else
                    {
                        _log.Info("[EzMemoryLaunchOption] AutoLunchTime_Checked ... chooice Yes");
                        return;
                        //EasyArrangementDDPM clickedeasyArrangementDDPM = DdpmCommonHelper.DeviceManagerSA.ReadMonitorEasyArrangement(_selecthomeDevice.MonitorInfo).Result;

                        //if (clickedeasyArrangementDDPM != null && clickedeasyArrangementDDPM.Desktops.Count > 0)
                        //{
                        //    foreach (var ps in clickedeasyArrangementDDPM.Desktops[0].ProfileSettings)
                        //    {
                        //        if (ps.AutoStartTime == GetAutoLaunchTime())
                        //        {
                        //            ps.Auto = false;
                        //            ps.AutoStartTime = 0;
                        //            if (DdpmCommonHelper.DeviceManagerSA.UpdateMonitorEzProfileSettingDDPM(_selecthomeDevice.MonitorInfo, ps).Result)
                        //            {
                        //                _log.Info($"@{nameof(EzMemoryLaunchOption)} _vm.IsLaunchAtStartup update success ");
                        //            }
                        //        }
                        //    }
                        //}
                        //else
                        //{
                        //    _log.Info($"@{nameof(EzMemoryLaunchOption)} CheckedAutoLunchTime update error ");
                        //}


                    }                    
                }

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

                    //Robert_Lin 2025-1-11, The tooltip of SplitItem in RightView shoud be the
                    //ProfileName instead of EA Layout description.
                    _vm.ispCtrlForEm.FriendlyName = profileName;
                    UpdateSplitListUI(profileID, _vm.ispCtrlForEm, isEditMode);

                    _log.Info($"@{nameof(EzMemoryLaunchOption)} FinishBtn_Click: User Settings PASS");
                }
                else
                {
                    _log.Info($"@{nameof(EzMemoryLaunchOption)} FinishBtn_Click: User Settings FAIL");
                    return;
                }

                // Handle Monitor Settings
                string instance = _selecthomeDevice.MonitorInfo.edid.Instance;

                if (_vm.IsAutoLaunch && (_vm.SelectedHour == string.Empty || _vm.SelectedAMPM == string.Empty))
                    return;

                if (_vm.IsLaunchAtStartup)
                {
                    _log.Info($"@{nameof(EzMemoryLaunchOption)} _vm.IsLaunchAtStartup True ");

                    EasyArrangementDDPM clickedeasyArrangementDDPM = DdpmCommonHelper.DeviceManagerSA.ReadMonitorEasyArrangement(_selecthomeDevice.MonitorInfo).Result;

                    if (clickedeasyArrangementDDPM != null && clickedeasyArrangementDDPM.Desktops.Count > 0)
                    {
                        int idxDesktop = clickedeasyArrangementDDPM.FindIndexOfDesktop(_selecthomeDevice.MonitorInfo.edid.Instance);

                        if (idxDesktop < 0)
                        {
                            //?? what to do?
                        }
                        else
                        {
                            foreach (var ps in clickedeasyArrangementDDPM.Desktops[idxDesktop].ProfileSettings)
                            {
                                if (ps.StartUpLaunch)
                                {
                                    ps.StartUpLaunch = false;
                                    if (DdpmCommonHelper.DeviceManagerSA.UpdateMonitorEzProfileSettingDDPM(_selecthomeDevice.MonitorInfo, ps).Result)
                                    {
                                        _log.Info($"@{nameof(EzMemoryLaunchOption)} _vm.IsLaunchAtStartup update success ");
                                    }
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

                #region Update to Per-monitor settings
                //Robert_Lin 2025-4-9
                //NEW:
                //Read all EM MonitorSettings for selected monitor
                EasyArrangementDDPM easyArrangement = DdpmCommonHelper.DeviceManagerSA.ReadMonitorEasyArrangement(_selecthomeDevice.MonitorInfo).Result;
                if (easyArrangement != null)
                {
                    int idxDesktop = easyArrangement.FindIndexOfDesktop(instance);
                    //if (isEditMode)
                    {
                        if (idxDesktop < 0)
                        {
                            List<DesktopDDPM> desktopList = new List<DesktopDDPM>(easyArrangement.Desktops);
                            DesktopDDPM newAddDesktop = new DesktopDDPM(instance, layout);
                            desktopList.Add(newAddDesktop);
                            easyArrangement.Desktops = desktopList;
                            idxDesktop = easyArrangement.Desktops.IndexOf(newAddDesktop);
                        }
                        EzProfileSettingDDPM? profileSettings = easyArrangement.Desktops[idxDesktop].ProfileSettings.Find(x => x.ID == profileID);
                        if (profileSettings == null)
                        {
                            profileSettings = new EzProfileSettingDDPM(profileID, _vm.IsAutoLaunch, autoLaunchTime, _vm.IsLaunchAtStartup);
                            easyArrangement.Desktops[idxDesktop].ProfileSettings.Add(profileSettings);
                        }
                        else
                        {
                            profileSettings.Auto = _vm.IsAutoLaunch;
                            profileSettings.AutoStartTime = autoLaunchTime;
                            profileSettings.StartUpLaunch = _vm.IsLaunchAtStartup;
                        }
                        _ = DdpmCommonHelper.DeviceManagerSA.WriteMonitorEasyArrangement(_selecthomeDevice.MonitorInfo, easyArrangement).Result;
                    }
                    _log.Info($"@{nameof(EzMemoryLaunchOption)} FinishBtn_Click: Monitor Settings PASS");
                }
                else
                {
                    _log.Info($"@{nameof(EzMemoryLaunchOption)} FinishBtn_Click: Monitor Settings FAIL");
                }

                //OLD:
                //bool monitorSettingsSuccess = isEditMode
                //        ? DdpmCommonHelper.DeviceManagerSA.UpdateMonitorEzProfileSettingDDPM(_selecthomeDevice.MonitorInfo, _ezProfileSettingDDPM).Result
                //        : HandleMonitorEasyArrangement(_selecthomeDevice.MonitorInfo, _ezProfileSettingDDPM);

                //if (monitorSettingsSuccess)
                //{
                //    _log.Info($"@{nameof(EzMemoryLaunchOption)} FinishBtn_Click: Monitor Settings PASS");
                //}
                //else
                //{
                //    _log.Info($"@{nameof(EzMemoryLaunchOption)} FinishBtn_Click: Monitor Settings FAIL");
                //}

                #endregion Update to Per-monitor settings

                if (isEditMode)
                {
                    //Check if layout is changed, if yes, then remove the old layout from SplitList
                    if (_vm.currentEditprofile.Layout != layout && _vm.OrgEditSplitItem != null)
                    {
                        _vm.CurrentEditSelectspItem.ProfileID = _vm.OrgEditSplitItem.ProfileID;
                        _vm.CurrentEditSelectspItem.ISplitCtrl.FriendlyName = profileName;
                        _vm.OrgEditSplitItem.ReplaceWithISplitICtrl(_vm.CurrentEditSelectspItem.ISplitCtrl);
                    }
                    //SplitItem tooltip
                    _vm.CurrentEditSelectspItem.ISplitCtrl.FriendlyName = profileName;
                    //Profile name at RightView Header
                    _vm.ProfileTitleTextBlockValue = profileName;
                    //_vm.LoadEmProfileSettings(profileID);
                }
                else
                {

                    _vm.CurrentEditSelectspItem.ProfileID = profileID;
                    _vm.CurrentEditSelectspItem.ISplitCtrl.FriendlyName = profileName;
                   // _vm.LoadEmProfileSettings(profileID);

                }

                _vm.UpdateRightViewUIFromCurrentSelectspItem();

                // Clear UI and close view
                _vm.ClearTextBlockAppName();
                _vm.IsEditProfile = false;
                //Robert_Lin 2025-1-10 Dont clean up ReightView Profile info,
                //When return back to RightView, curranet added/edit profile info will be shown and selected
                //_vm.RightViewDataClear();
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
            string amDesignator = CultureInfo.CurrentCulture.DateTimeFormat.AMDesignator;
            string pmDesignator = CultureInfo.CurrentCulture.DateTimeFormat.PMDesignator;

            int hour = int.TryParse(_vm.SelectedHour, out var h) ? h : 0;
            int minute = int.TryParse(_vm.SelectedMinute, out var m) ? m : 0;

            // PM
            if (_vm.SelectedAMPM == pmDesignator)
            {
                hour += 12;
            }
            // AM
            else if (_vm.SelectedAMPM == amDesignator && hour == 12)
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

                string instance = monitorInfo.edid.Instance;

                if (easyArrangement.Desktops == null || easyArrangement.Desktops.Count == 0)
                {
                    //Robert_Lin 2025-4-8 to support multiple partitions
                    //DesktopDDPM newDesktop = new DesktopDDPM(string.Empty, 0);
                    DesktopDDPM newDesktop = new DesktopDDPM(instance, 0);
                    newDesktop.ProfileSettings.Add(ezProfileSetting);
                    easyArrangement.Desktops = new List<DesktopDDPM> { newDesktop };
                }
                else
                {
                    //OLD:
                    //easyArrangement.Desktops[0].ProfileSettings.Add(ezProfileSetting);
                    //Check if the Instance is already exist
                    int idxDesktop = easyArrangement.FindIndexOfDesktop(instance);
                    //No this Instance found => add into Desktops[]
                    if (idxDesktop < 0)
                    {
                        List<DesktopDDPM> desktopList = new List<DesktopDDPM>(easyArrangement.Desktops);
                        DesktopDDPM newAddDesktop = new DesktopDDPM(instance, 0);
                        desktopList.Add(newAddDesktop);
                        easyArrangement.Desktops = desktopList;
                        idxDesktop = easyArrangement.Desktops.IndexOf(newAddDesktop);
                    }
                    //Existing insrance
                    easyArrangement.Desktops[idxDesktop].ProfileSettings.Add(ezProfileSetting);

                    
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
                    //Robert_Lin 2025-1-9 fix, find by EAID
                    //NEW:
                    SplitItem? spItem = _vm.splitListRightView.FindItemByProfileId(profileID);
                    //OLD:
                    //SplitItem? spItem = _vm.splitListRightView.FindItemByCustomId(ispAdd.EAID);
                    if (spItem != null)
                    {
                        spItem.ReplaceWithISplitICtrl(ispNew);
                        spItem.IsSelected = true;

                        //Set new added SplitItem as Current selected
                        if (_vm.CurrentSelectspItem != null && _vm.CurrentSelectspItem.ProfileID != profileID)
                            _vm.CurrentSelectspItem.IsSelected = false;
                        _vm.CurrentSelectspItem = spItem;
                        //_vm.CurrentSelectspItem.IsSelected = true;
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

                    //Set new added SplitItem as Current selected
                    if (_vm.CurrentSelectspItem != null)
                    {
                        _vm.CurrentSelectspItem.IsSelected = false;
                    }
                    _vm.CurrentSelectspItem = newItem;
                    _vm.CurrentSelectspItem.IsSelected = true;
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
            //Robert_Lin 2025-4-23 Issue steps:
            //1 Edit an exiting profile and goes to LaunchOption page; 2 Click "Cancel"
            //Problem: When return to EzMemoryRightView, the profile data is not shown.
            //Root cause: Below instruction, has clear the displaying data
            //OLD:
            //_vm.RightViewDataClear();
            //NEW: Remove _vm.RightViewDataClear(); or add below to refresh data:
            //_vm.RefreshProfileSettingsToRightView();
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
                        if (ps.StartUpLaunch &&
                            (_vm.currentEditprofileSetting == null || _vm.currentEditprofileSetting.ID != ps.ID) )
                        {
                            DDPM.SA.Common.Popup.PopupBase popBase = new DDPM.SA.Common.Popup.PopupBase(
                                Strings.ezMemoryStartupErrorTitleStringForLaunchOptionPage,
                                Strings.ezMemoryStartupErrorStringForLaunchOptionPage,
                                Strings.No,
                                Strings.Yes,
                                null, 
                                false, 
                                0);
                            popBase.Owner = System.Windows.Application.Current.MainWindow;
                            bool? popResult = popBase.ShowDialog();

                            //if (DdpmCommonHelper.DDPMMesssageBox(Strings.ezMemoryStartupErrorTitleStringForLaunchOptionPage, Strings.ezMemoryStartupErrorStringForLaunchOptionPage))
                            if(popResult != true)
                            {
                                _log.Info("[EzMemoryLaunchOption] StartupCB_Checked ... choice No");
                                _vm.IsLaunchAtStartup = false;
                            }
                            else
                            {
                                _log.Info("[EzMemoryLaunchOption] StartupCB_Checked ... choice Yes");
                                _vm.IsLaunchAtStartup = true;
                                return;
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

        private bool CheckedAutoLunchTime()
        {
            try
            {
                _log.Info("[EzMemoryLaunchOption] CheckedAutoLunchTime ... in");

                EasyArrangementDDPM clickedeasyArrangementDDPM = DdpmCommonHelper.DeviceManagerSA.ReadMonitorEasyArrangement(_selecthomeDevice.MonitorInfo).Result;

                if (clickedeasyArrangementDDPM != null && clickedeasyArrangementDDPM.Desktops.Count > 0)
                {
                    long autoLaunchTime = _vm.IsAutoLaunch ? GetAutoLaunchTime() : default;

                    foreach (var ps in clickedeasyArrangementDDPM.Desktops[0].ProfileSettings)
                    {
                        if (ps.AutoStartTime == autoLaunchTime && autoLaunchTime !=0 &&
                            (_vm.currentEditprofileSetting == null || _vm.currentEditprofileSetting.ID != ps.ID) )
                        {
                            return false;
                            //    _vm.IsLaunchAtStartup = true;
                            //    _vm.IsLaunchAtStartup = false;                            
                        }
                    }
                }
                else
                {
                    _log.Info("[EzMemoryLaunchOption] AutoRB_Checked  can not found Auto Launch By Time = True file");
                }
                return true;
            }
            catch (Exception ex)
            {
                _log.Error($"[EzMemoryLaunchOption] AutoRB_Checked Exception occurred: {ex.Message}");
                return false;
            }
        }
    }
}
