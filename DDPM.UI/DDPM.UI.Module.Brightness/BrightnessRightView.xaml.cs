using DDPM.SA.Common;
using DDPM.SA.Common.Settings;
using DDPM.UI.Common;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using VcpCore.Common;
using UserControl = System.Windows.Controls.UserControl;

namespace DDPM.UI.Module.Brightness
{
    /// <summary>
    /// Interaction logic for BrightnessRightView.xaml
    /// </summary>
    public partial class BrightnessRightView : UserControl
    {
        internal BrightnessViewModel? vm { get; set; }

        private Debouncer Leave_WriteToConfig_Debouncer;

        public BrightnessRightView()
        {
            InitializeComponent();

            Leave_WriteToConfig_Debouncer = new Debouncer(1000, WriteToConfig);
            //vm = BrightnessViewModel.GetInstance();
            if (DdpmCommonHelper.DeviceManagerSA != null)
            {
                DdpmCommonHelper.DeviceManagerSA.ITSettingsActionEvent += DeviceManagerSA_ITSettingsActionEvent;                
            }
        }

        ~BrightnessRightView()
        {
            if (DdpmCommonHelper.DeviceManagerSA != null)
            {
                DdpmCommonHelper.DeviceManagerSA.ITSettingsActionEvent -= DeviceManagerSA_ITSettingsActionEvent;    
            }
        }

        private void DeviceManagerSA_ITSettingsActionEvent(object? sender, SA.Common.ITSettingEventArgs e)
        {
            DDPMSettings data = null;
            if (DdpmCommonHelper.DeviceManagerSA != null)
                data = DdpmCommonHelper.ReadDDPMSettings(true);// DeviceManagerSA.ReloadAppConfigData().Result;

            bool? isLocked_BriCont = DdpmCommonHelper.GetUINotifyPropertyValue_Boolean("Lock_Display_BriCont", e);
            if (isLocked_BriCont != null)
            {
                Dispatcher.Invoke(new Action(() =>
                {
                    BrightnessViewModel vm = (BrightnessViewModel)this.DataContext;
                    if (vm != null)
                    {
                        vm.Update_BriContLockStatus(isLocked_BriCont ?? false);
                        Trace.WriteLine($"[SettingsPage] Apply Brightness/Contrast(Lock) : {isLocked_BriCont}");
                    }
                }));
            }
            bool? isLocked_ALS = DdpmCommonHelper.GetUINotifyPropertyValue_Boolean("Lock_Display_AutoBriTemp", e);
            if (isLocked_ALS != null)
            {
                Dispatcher.Invoke(new Action(() =>
                {
                    BrightnessViewModel vm = (BrightnessViewModel)this.DataContext;
                    if (vm != null)
                    {
                        //vm.LockMaskVisible = (bool)isLocked ? Visibility.Visible : Visibility.Collapsed;
                        vm.Update_ALSLockStatus(isLocked_ALS ?? false);
                        Trace.WriteLine($"[SettingsPage] Apply Auto Brightness(Lock) : {isLocked_ALS}");
                    }
                }));
            }
            if(data != null && data.LockSettings != null)
            {
                bool isSyncLocked = DdpmCommonHelper.GetUINotify_IsSynchronizeBetweenMonitors_Locked(data);
                Dispatcher.Invoke(new Action(() =>
                {
                    BrightnessViewModel vm = (BrightnessViewModel)this.DataContext;
                    if (vm != null)
                    {
                        vm.Update_SyncLockStatus(isSyncLocked);                        
                        Trace.WriteLine($"[SettingsPage] Apply Synchroniz Button(Lock) : {isSyncLocked}");
                    }
                }));
                
                //apply this lock result to "synchronize between monitors" toggle button
            }
        }

        private void Reset_Click(object sender, RoutedEventArgs e)
        {
            BrightnessViewModel x = (BrightnessViewModel)DataContext;
            bool r = DdpmCommonHelper.DeviceManagerSA.SetVCPCapability(x.SelectedHomeDevice.MonitorInfo, 0x05, 1).Result;
            if (r)
            {
                ObjGetVCP rb_10 = DdpmCommonHelper.DeviceManagerSA.GetVCPCapability(x.SelectedHomeDevice.MonitorInfo, 0x10, 0).Result;
                if (rb_10.result)
                {
                    B_slider.Value = (uint)((long)rb_10.value);
                    LuminanceSlider.Value = (uint)((long)rb_10.value);
                }
                ObjGetVCP rb_12 = DdpmCommonHelper.DeviceManagerSA.GetVCPCapability(x.SelectedHomeDevice.MonitorInfo, 0x12, 0).Result;
                if (rb_12.result)
                    C_slider.Value = (uint)((long)rb_12.value);
            }
        }

        private void SynchronizeSwitch_Click(object sender, RoutedEventArgs e)
        {
            BrightnessViewModel x = (BrightnessViewModel)DataContext;

            DDPMSettings setting = DdpmCommonHelper.ReadDDPMSettings();// DeviceManagerSA.ReloadAppConfigData().Result;

            if ((bool)SynchronizeSwitch.IsChecked)
            {
                x.IsSynchronize = true;
                SynchronizeSwitch.Content = Strings.On;
            }
            else
            {
                x.IsSynchronize = false;
                SynchronizeSwitch.Content = Strings.Off;
            }

            setting.UserSettings.IsSynchronizemonitor = x.IsSynchronize;
            DdpmCommonHelper.WriteDDPMSettings(setting);// DeviceManagerSA.SetAppConfigData(setting);
        }

        private void Expander_Manual_Expanded(object sender, RoutedEventArgs e)
        {
            Expander_Manual_Luminance.IsExpanded = false;
            Expander_Auto.IsExpanded = false;
            Expander_Schedule.IsExpanded = false;

            try
            {
                BrightnessViewModel vm = (BrightnessViewModel)DataContext;
                if (vm != null)
                {
                    bool autoBrightnessStatus = vm.AutoBrightnessStatus;
                    if (autoBrightnessStatus)
                    {
                        //disable hotkey btn
                        btnManualBrightnessContrast.IsEnabled = false;
                    }
                    else
                    {
                        btnManualBrightnessContrast.IsEnabled = true;
                    }

                    Task.Run(() => vm.CloseSchedule());
                }
                /*Trace.WriteLine($"1. {DateTime.Now.ToString("MM/dd/yyyy hh:mm ss fff")}");
                BrightnessViewModel vm = (BrightnessViewModel)DataContext;
                Trace.WriteLine($"2. {DateTime.Now.ToString("MM/dd/yyyy hh:mm ss fff")}");
                if (vm != null)
                {
                    vm.UpdateBrightnessContrast();
                }
                Trace.WriteLine($"3. {DateTime.Now.ToString("MM/dd/yyyy hh:mm ss fff")}");*/
                List<ALSConfig> alsSynchronizeList = DdpmCommonHelper.DeviceManagerSA.GetAllExistAlsConfig().Result;
                vm.CheckisShowSynchronize(alsSynchronizeList);
            }
            catch (Exception) { }
        }

        private void Expander_Manual_Expanded_Luminance(object sender, RoutedEventArgs e)
        {
            Expander_Auto.IsExpanded = false;
            Expander_Schedule.IsExpanded = false;
            Expander_Manual.IsExpanded = false;

            BrightnessViewModel vm = (BrightnessViewModel)DataContext;
            if (vm != null)
            {
                vm.UpdateLuminance();
            }
        }

        private void Expander_Auto_Expanded(object sender, RoutedEventArgs e)
        {
            Expander_Manual.IsExpanded = false;
            Expander_Schedule.IsExpanded = false;
            Expander_Manual_Luminance.IsExpanded = false;
        }

        private void Expander_Schedule_Expanded(object sender, RoutedEventArgs e)
        {
            Expander_Auto.IsExpanded = false;
            Expander_Manual.IsExpanded = false;
            Expander_Manual_Luminance.IsExpanded = false;
        }

        private void Hotkey_Click(object sender, RoutedEventArgs e)
        {
            DisplayHotkeyFullView displayHotkeyFullView = new DisplayHotkeyFullView();
            displayHotkeyFullView.DataContext = (BrightnessViewModel)DataContext;
            DdpmCommonHelper.ModuleOwner?.OpenFullView(displayHotkeyFullView);
        }

        private void Schedule_prest1_Border_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            BrightnessViewModel vm = (BrightnessViewModel)DataContext;
            vm.IsMouseEnterSchedule_1 = true;
            vm.UpdataScheduleBoaderUI();

            if (!vm.IsPR1Preview && !vm.IsPR2Preview)
            {
                Task.Run(() =>
                {
                    vm.StopScheduleManger();
                    vm.BrightnessValue = vm.PR1BrightnessValue;
                    vm.ContrastValue = vm.PR1ContrastValue;
                });
            }
        }

        private void Schedule_prest2_Border_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            BrightnessViewModel vm = (BrightnessViewModel)DataContext;
            vm.IsMouseEnterSchedule_2 = true;
            vm.UpdataScheduleBoaderUI();

            if (!vm.IsPR1Preview && !vm.IsPR2Preview)
            {
                Task.Run(() =>
                {
                    vm.StopScheduleManger();
                    vm.BrightnessValue = vm.PR2BrightnessValue;
                    vm.ContrastValue = vm.PR2ContrastValue;
                });
            }
        }

        private void Schedule_prest1_Border_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            BrightnessViewModel vm = (BrightnessViewModel)DataContext;
            vm.IsMouseEnterSchedule_1 = false;
            vm.UpdataScheduleBoaderUI();

            if (!vm.IsPR1Preview && !vm.IsPR2Preview)
            {
                Task.Run(() =>
                {
                    vm.CalculateNowValue();
                    Leave_WriteToConfig_Debouncer.Debounce(vm);
                });
            }
        }

        private void Schedule_prest2_Border_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            BrightnessViewModel vm = (BrightnessViewModel)DataContext;
            vm.IsMouseEnterSchedule_2 = false;
            vm.UpdataScheduleBoaderUI();

            if (!vm.IsPR1Preview && !vm.IsPR2Preview)
            {
                Task.Run(() =>
                {
                    vm.CalculateNowValue();
                    Leave_WriteToConfig_Debouncer.Debounce(vm);
                });
            }
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            BrightnessViewModel vm = (BrightnessViewModel)DataContext;
            var rc = vm.CheckIsTimeOverlap();

            if (rc)
                DdpmCommonHelper.DDPMPureMesssageBox(Strings.Error, Strings.BrightnessErrorMsg0, true, Window.GetWindow(this));
        }

        private async void PR1_Preview_UXButton_Click(object sender, RoutedEventArgs e)
        {
            BrightnessViewModel vm = (BrightnessViewModel)DataContext;
            if (!vm.IsPR1Preview)
            {
                vm.IsPR1Preview = true;
                vm.IsPR2Preview = false;

                using (var tokenSource = new CancellationTokenSource())
                {
                    try
                    {
                        vm.PreviewToken = tokenSource;
                        var token = vm.PreviewToken.Token;

                        //TODO: May be you'll want to add .ConfigureAwait(false);
                        await Task.Run(() => ShowPreview(1, vm, token), token).ConfigureAwait(false);
                    }
                    catch (TaskCanceledException)
                    {
                        // Task was canceled before running.
                        // Cancelled due to timeout
                    }
                    catch (OperationCanceledException)
                    {
                        // Task was canceled while running.
                        // Cancelled due to timeout
                    }
                    catch (Exception ex)
                    {
                        // Failed to complete due to e exception

                        //Done: let's be nice and don't swallow the exception
                        //throw;
                    }
                    finally
                    {
                        vm.IsPR1Preview = false;
                        vm.IsPR2Preview = false;

                        vm.BrightnessValue = vm.PR1BrightnessValue;
                        vm.ContrastValue = vm.PR1ContrastValue;

                        vm.PreviewToken.Dispose();
                        tokenSource.Dispose();
                    }
                }
            }
            else
            {
                try
                {
                    if (vm.PreviewToken != null && !vm.PreviewToken.IsCancellationRequested)
                        vm.PreviewToken.Cancel();
                }
                catch (Exception) { }
                finally
                {
                    vm.IsPR1Preview = false;
                    vm.IsPR2Preview = false;

                    vm.BrightnessValue = vm.PR1BrightnessValue;
                    vm.ContrastValue = vm.PR1ContrastValue;
                }
            }
        }

        private async void PR2_Preview_UXButton_Click(object sender, RoutedEventArgs e)
        {
            BrightnessViewModel vm = (BrightnessViewModel)DataContext;
            if (!vm.IsPR2Preview)
            {
                vm.IsPR2Preview = true;
                vm.IsPR1Preview = false;

                using (var tokenSource = new CancellationTokenSource())
                {
                    try
                    {
                        vm.PreviewToken = tokenSource;
                        var token = vm.PreviewToken.Token;

                        //TODO: May be you'll want to add .ConfigureAwait(false);
                        await Task.Run(() => ShowPreview(2, vm, token), token).ConfigureAwait(false);
                    }
                    catch (TaskCanceledException)
                    {
                        // Task was canceled before running.
                        // Cancelled due to timeout
                    }
                    catch (OperationCanceledException)
                    {
                        // Task was canceled while running.
                        // Cancelled due to timeout
                    }
                    catch (Exception ex)
                    {
                        // Failed to complete due to e exception

                        //Done: let's be nice and don't swallow the exception
                        //throw;
                    }
                    finally
                    {
                        vm.IsPR2Preview = false;
                        vm.IsPR1Preview = false;

                        vm.BrightnessValue = vm.PR2BrightnessValue;
                        vm.ContrastValue = vm.PR2ContrastValue;

                        vm.PreviewToken.Dispose();
                        tokenSource.Dispose();
                    }
                }
            }
            else
            {
                try
                {
                    if (vm.PreviewToken != null && !vm.PreviewToken.IsCancellationRequested)
                        vm.PreviewToken.Cancel();
                }
                catch (Exception) { }
                finally
                {
                    vm.IsPR2Preview = false;
                    vm.IsPR1Preview = false;

                    vm.BrightnessValue = vm.PR2BrightnessValue;
                    vm.ContrastValue = vm.PR2ContrastValue;
                }
            }
        }

        private void WriteToConfig(object VM_)
        {
            BrightnessViewModel vm = (BrightnessViewModel)VM_;

            if (vm.hOurs1 > -1 && vm.hOurs2 > -1 && vm.mIns1 > -1 && vm.mIns2 > -1 && vm.dUration1 > -1 && vm.dUration2 > -1)
            {
                if (vm.ScheduleMap == null)
                    vm.ScheduleMap = new scheduleInfo();

                vm.ScheduleMap.IsEnable = true;
                vm.ScheduleMap.model = vm.SelectedHomeDevice.MonitorInfo.modelName;
                vm.ScheduleMap.serviceTag = vm.SelectedHomeDevice.MonitorInfo.edid.ServiceTag;
                vm.ScheduleMap.Pre1Name = vm.PR1Name;
                vm.ScheduleMap.Pre2Name = vm.PR2Name;
                vm.ScheduleMap.Hours1 = vm.hOurs1;
                vm.ScheduleMap.Mins1 = vm.mIns1;
                vm.ScheduleMap.Duration1 = vm.dUration1;
                vm.ScheduleMap.Hours2 = vm.hOurs2;
                vm.ScheduleMap.Mins2 = vm.mIns2;
                vm.ScheduleMap.Duration2 = vm.dUration2;
                vm.ScheduleMap.Brightness1 = vm.PR1BrightnessValue;
                vm.ScheduleMap.Contrast1 = vm.PR1ContrastValue;
                vm.ScheduleMap.Brightness2 = vm.PR2BrightnessValue;
                vm.ScheduleMap.Contrast2 = vm.PR2ContrastValue;

                if ((!vm.IsMouseEnterSchedule_1 && !vm.IsMouseEnterSchedule_2) && (!vm.CheckIsTimeOverlap()) && (!vm.IsPR1Preview && !vm.IsPR2Preview))
                {
                    DdpmCommonHelper.DeviceManagerSA.WriteScheduleMonitorSettings(vm.SelectedHomeDevice.MonitorInfo, vm.ScheduleMap);
                    vm.StartScheduleManger(60000);
                }
            }
        }

        private void ShowPreview(int pr, BrightnessViewModel vm, CancellationToken token)
        {
            var Brightness_PR1 = vm.PR1BrightnessValue;
            var Brightness_PR2 = vm.PR2BrightnessValue;
            var Brightness_difference = Brightness_PR1 - Brightness_PR2;
            var Contrast_PR1 = vm.PR1ContrastValue;
            var Contrast_PR2 = vm.PR2ContrastValue;
            var Contrast_difference = Contrast_PR1 - Contrast_PR2;
            bool IsBrightnessPR1Plus = Brightness_difference > 0 ? true : false;
            bool IsBrightnessPR2Plus = Brightness_difference > 0 ? false : true;
            bool IsContrastPR1Plus = Contrast_difference > 0 ? true : false;
            bool IsContrastPR2Plus = Contrast_difference > 0 ? false : true;
            if (Brightness_difference < 0) Brightness_difference = Brightness_difference * -1;
            if (Contrast_difference < 0) Contrast_difference = Contrast_difference * -1;

            var TimeDemoPreview_msec = 10000;
            var PerStepValue = 20;
            var BrightnessSteps = Brightness_difference / PerStepValue;
            var BrightnessSteps_msec = Convert.ToInt32(TimeDemoPreview_msec / BrightnessSteps);

            var ContrastSteps = Contrast_difference / PerStepValue;
            var ContrastSteps_msec = Convert.ToInt32(TimeDemoPreview_msec / ContrastSteps);

            int BrightnessSteps_msec_counter = 0;
            int ContrastSteps_msec_counter = 0;

            switch (pr)
            {
                case 1:
                    {
                        vm.BrightnessValue = Brightness_PR2;
                        vm.ContrastValue = Contrast_PR2;

                        int Count = 0;
                        while (Count <= TimeDemoPreview_msec)
                        {
                            if (BrightnessSteps_msec_counter == BrightnessSteps_msec)
                            {
                                if (IsBrightnessPR1Plus)
                                    vm.BrightnessValue += PerStepValue;
                                else
                                    vm.BrightnessValue -= PerStepValue;

                                BrightnessSteps_msec_counter = 0;
                            }

                            if (ContrastSteps_msec_counter == ContrastSteps_msec)
                            {
                                if (IsContrastPR1Plus)
                                    vm.ContrastValue += PerStepValue;
                                else
                                    vm.ContrastValue -= PerStepValue;

                                ContrastSteps_msec_counter = 0;
                            }

                            Thread.Sleep(1);
                            BrightnessSteps_msec_counter++;
                            ContrastSteps_msec_counter++;
                            Count++;
                        }

                        vm.BrightnessValue = Brightness_PR1;
                        vm.ContrastValue = Contrast_PR1;
                    }
                    break;

                case 2:
                    {
                        vm.BrightnessValue = Brightness_PR1;
                        vm.ContrastValue = Contrast_PR1;

                        int Count = 0;
                        while (Count <= TimeDemoPreview_msec)
                        {
                            if (BrightnessSteps_msec_counter == BrightnessSteps_msec)
                            {
                                if (IsBrightnessPR2Plus)
                                    vm.BrightnessValue += PerStepValue;
                                else
                                    vm.BrightnessValue -= PerStepValue;

                                BrightnessSteps_msec_counter = 0;
                            }

                            if (ContrastSteps_msec_counter == ContrastSteps_msec)
                            {
                                if (IsContrastPR2Plus)
                                    vm.ContrastValue += PerStepValue;
                                else
                                    vm.ContrastValue -= PerStepValue;

                                ContrastSteps_msec_counter = 0;
                            }

                            Thread.Sleep(1);
                            BrightnessSteps_msec_counter++;
                            ContrastSteps_msec_counter++;
                            Count++;
                        }

                        vm.BrightnessValue = Brightness_PR2;
                        vm.ContrastValue = Contrast_PR2;
                    }
                    break;

                default:
                    break;
            }
        }

        private void Hyperlink_Click(object sender, RoutedEventArgs e)
        {
        }
    }
}