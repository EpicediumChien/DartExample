using DDPM.SA.Common;
using DDPM.SA.Common.Settings;
using DDPM.UI.Common;
using DDPM.UI.Common.Method;
using DDPM.UI.Common.Models;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using UserControl = System.Windows.Controls.UserControl;

namespace DDPM.UI.Module.Brightness
{
    /// <summary>
    /// Interaction logic for BrightnessRightView.xaml
    /// </summary>
    public partial class BrightnessRightView : UserControl
    {
        private Debouncer Leave_Luminance_WriteToConfig_Debouncer;
        private Debouncer Leave_WriteToConfig_Debouncer;

        public BrightnessRightView()
        {
            InitializeComponent();

            Leave_WriteToConfig_Debouncer = new Debouncer(1000, WriteToConfig);
            Leave_Luminance_WriteToConfig_Debouncer = new Debouncer(1000, WriteToConfig);

            //vm = BrightnessViewModel.GetInstance();
            if (DdpmCommonHelper.DeviceManagerSA != null)
            {
                DdpmCommonHelper.DeviceManagerSA.ITSettingsActionEvent += DeviceManagerSA_ITSettingsActionEvent;
            }
            DdpmCommonHelper.WriteUILog("BrightnessRightView, init");
        }

        ~BrightnessRightView()
        {
            if (DdpmCommonHelper.DeviceManagerSA != null)
            {
                DdpmCommonHelper.DeviceManagerSA.ITSettingsActionEvent -= DeviceManagerSA_ITSettingsActionEvent;
            }
            DdpmCommonHelper.WriteUILog("~BrightnessRightView, exit");
        }

        internal BrightnessViewModel? vm { get; set; }

        private void brightnessScheduledExpander_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Expander expander = sender as Expander;
                if (expander != null)
                {
                    e.Handled = true;
                    //Robert_Lin 2025-2-26 Narrator. If Expander is already expanded, then do nothing
                    if (expander.IsExpanded)
                    {
                        //expander.IsExpanded = !expander.IsExpanded;
                    }
                    else
                    {
                        expander.IsExpanded = !expander.IsExpanded;
                    }
                }
            }
        }

        private void brightnewwManulExpander_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Expander expander = sender as Expander;
                if (expander != null)
                {
                    e.Handled = true;
                    //Robert_Lin 2025-2-26 Narrator. If Expander is already expanded, then do nothing
                    if (expander.IsExpanded)
                    {
                        //expander.IsExpanded = !expander.IsExpanded;
                    }
                    else
                    {
                        expander.IsExpanded = !expander.IsExpanded;
                    }
                }
            }
        }

        private void cbAutoBrightness_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int _previousSelectedIndex = 0;
            var comboBox = sender as ComboBox;
            if (comboBox == null) return;
            BrightnessViewModel localVm = (BrightnessViewModel)DataContext;
            if (localVm != null && localVm.Start_ALSConfig != null && localVm.Start_ALSConfig.AutoBrightnessRangeLevel != null)
            {
                _previousSelectedIndex = (int)localVm.Start_ALSConfig.AutoBrightnessRangeLevel.level_value;

                int temp = (int)comboBox.SelectedIndex;// SelectedIndex;

                if (temp == 0)
                    comboBox.SelectedValue = Strings.ALSRangeLevelLow; //"Low";
                else if (temp == 1)
                    comboBox.SelectedValue = Strings.ALSRangeLevelMid; // "Mid";
                else
                    comboBox.SelectedValue = Strings.ALSRangeLevelHigh; //"High";
            }
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            BrightnessViewModel localVm = (BrightnessViewModel)DataContext;
            var rc = localVm.CheckIsTimeOverlap();

            if (rc)
            {
                Thickness headMargin = new Thickness(36, 32, 36, 16);
                Thickness subMargin = new Thickness(36, 0, 55, 32);
                DdpmCommonHelper.DDPMEzMesssageBox(Strings.Error, Strings.BrightnessErrorMsg0, true, Window.GetWindow(this), 419, 180, headMargin, subMargin);
            }

            //if (rc)
            //    DdpmCommonHelper.DDPMPureMesssageBox(Strings.Error, Strings.BrightnessErrorMsg0, true, Window.GetWindow(this));
        }

        private void DeviceManagerSA_ITSettingsActionEvent(object? sender, SA.Common.ITSettingEventArgs e)
        {
            DDPMSettings data = null;
            if (DdpmCommonHelper.DeviceManagerSA != null)
                data = DdpmCommonHelper.ReadDDPMSettings(true);// DeviceManagerSA.ReloadAppConfigData().Result;
            string log = string.Empty;
            bool? isLocked_BriCont = DdpmCommonHelper.GetUINotifyPropertyValue_Boolean("Lock_Display_BriCont", e);
            if (isLocked_BriCont != null)
            {
                Dispatcher.Invoke(new Action(() =>
                {
                    BrightnessViewModel localVm = (BrightnessViewModel)this.DataContext;
                    if (localVm != null)
                    {
                        localVm.Update_BriContLockStatus(isLocked_BriCont ?? false);
                        log = $"[SettingsPage] Apply Brightness/Contrast(Lock) : {isLocked_BriCont}";
                        DdpmCommonHelper.WriteUILog(log);
                    }
                }));
            }
            bool? isLocked_ALS = DdpmCommonHelper.GetUINotifyPropertyValue_Boolean("Lock_Display_AutoBriTemp", e);
            if (isLocked_ALS != null)
            {
                Dispatcher.Invoke(new Action(() =>
                {
                    BrightnessViewModel localVm = (BrightnessViewModel)this.DataContext;
                    if (localVm != null)
                    {
                        //vm.LockMaskVisible = (bool)isLocked ? Visibility.Visible : Visibility.Collapsed;
                        localVm.Update_ALSLockStatus(isLocked_ALS ?? false);
                        log = $"[SettingsPage] Apply Auto Brightness(Lock) : {isLocked_ALS}";
                        DdpmCommonHelper.WriteUILog(log);
                    }
                }));
            }
            if (data != null && data.LockSettings != null)
            {
                bool isSyncLocked = DdpmCommonHelper.GetUINotify_IsSynchronizeBetweenMonitors_Locked(data);
                Dispatcher.Invoke(new Action(() =>
                {
                    BrightnessViewModel localVm = (BrightnessViewModel)this.DataContext;
                    if (localVm != null)
                    {
                        localVm.Update_SyncLockStatus(isSyncLocked);
                        log = $"[SettingsPage] Apply Synchronize Button(Lock) : {isSyncLocked}";
                        DdpmCommonHelper.WriteUILog(log);
                    }
                }));

                //apply this lock result to "synchronize between monitors" toggle button
            }
        }

        private void Expander_Auto_Expanded(object sender, RoutedEventArgs e)
        {
            Expander_Manual.IsExpanded = false;
            Expander_Schedule.IsExpanded = false;
            Expander_Manual_Luminance.IsExpanded = false;
            Expander_Schedule_Luminance.IsExpanded = false;
        }

        private void Expander_Manual_Expanded(object sender, RoutedEventArgs e)
        {
            Expander_Manual_Luminance.IsExpanded = false;
            Expander_Auto.IsExpanded = false;
            Expander_Schedule.IsExpanded = false;
            Expander_Schedule_Luminance.IsExpanded = false;

            try
            {
                BrightnessViewModel local_vm = (BrightnessViewModel)DataContext;
                if (local_vm != null)
                {
                    /*bool autoBrightnessStatus = vm.AutoBrightnessStatus;
                    if (autoBrightnessStatus)
                    {
                        //disable hotkey btn
                        btnManualBrightnessContrast.IsEnabled = false;
                    }
                    else
                    {
                        btnManualBrightnessContrast.IsEnabled = true;
                    }*/
                    local_vm.updateHotkeyBtn();

                    Task.Run(() => local_vm.CloseSchedule());
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
                local_vm.SynchronizeBtnExpectedResult(DdpmCommonHelper.DeviceManagerSA.CheckisShowSynchronize(local_vm.SelectedHomeDevice.MonitorInfo, alsSynchronizeList).Result);
                //vm.CheckisShowSynchronize(alsSynchronizeList);
            }
            catch (Exception) { }
        }

        private void Expander_Manual_Expanded_Luminance(object sender, RoutedEventArgs e)
        {
            Expander_Auto.IsExpanded = false;
            Expander_Schedule.IsExpanded = false;
            Expander_Manual.IsExpanded = false;
            Expander_Schedule_Luminance.IsExpanded = false;

            BrightnessViewModel local_vm = (BrightnessViewModel)DataContext;
            if (local_vm != null)
            {
                local_vm.UpdateLuminance();
                Task.Run(() => local_vm.CloseSchedule());
            }
        }

        private void Expander_Schedule_Expanded(object sender, RoutedEventArgs e)
        {
            Expander_Auto.IsExpanded = false;
            Expander_Manual.IsExpanded = false;
            Expander_Manual_Luminance.IsExpanded = false;
            Expander_Schedule_Luminance.IsExpanded = false;
        }

        private void Expander_Schedule_Luminance_Expanded(object sender, RoutedEventArgs e)
        {
            Expander_Auto.IsExpanded = false;
            Expander_Manual.IsExpanded = false;
            Expander_Manual_Luminance.IsExpanded = false;
            Expander_Schedule.IsExpanded = false;
        }

        //Elsa add for tooltip issue fix
        private double GetScreenScaleX()
        {
            var source = PresentationSource.FromVisual(this);
            if (source?.CompositionTarget != null)
            {
                return source.CompositionTarget.TransformToDevice.M11;
            }
            return 1;
        }

        private void Hotkey_Click(object sender, RoutedEventArgs e)
        {
            DisplayHotkeyFullView displayHotkeyFullView = new DisplayHotkeyFullView();
            displayHotkeyFullView.DataContext = (BrightnessViewModel)DataContext;
            DdpmCommonHelper.ModuleOwner?.OpenFullView(displayHotkeyFullView);
        }

        private void Hyperlink_Click(object sender, RoutedEventArgs e)
        {
        }

        private void InputName_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            TextString textString = new TextString();
            e.Handled = !textString.CheckChar(e.Text);
        }

        private async void PR1_Luminance_Preview_UXButton_Click(object sender, RoutedEventArgs e)
        {
            BrightnessViewModel localVm = (BrightnessViewModel)DataContext;
            if (!localVm.IsPR1_Luminance_Preview)
            {
                localVm.IsPR1_Luminance_Preview = true;
                localVm.IsPR2_Luminance_Preview = false;

                using (var tokenSource = new CancellationTokenSource())
                {
                    try
                    {
                        localVm.PreviewToken = tokenSource;
                        var token = localVm.PreviewToken.Token;

                        //TODO: May be you'll want to add .ConfigureAwait(false);
                        await Task.Run(() => ShowPreview(1, localVm, token), token).ConfigureAwait(false);
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
                        localVm.IsPR1_Luminance_Preview = false;
                        localVm.IsPR2_Luminance_Preview = false;

                        if (!(localVm.isLuminanceSupport == Visibility.Visible))
                        {
                            localVm.BrightnessValue = localVm.PR1BrightnessValue;
                            localVm.ContrastValue = localVm.PR1ContrastValue;
                        }
                        else
                            localVm.LuminanceValue = localVm.PR1LuminanceValue;

                        localVm.PreviewToken.Dispose();
                        tokenSource.Dispose();
                    }
                }
            }
            else
            {
                try
                {
                    if (localVm.PreviewToken != null && !localVm.PreviewToken.IsCancellationRequested)
                        localVm.PreviewToken.Cancel();
                }
                catch (Exception) { }
                finally
                {
                    localVm.IsPR1_Luminance_Preview = false;
                    localVm.IsPR2_Luminance_Preview = false;

                    if (!(localVm.isLuminanceSupport == Visibility.Visible))
                    {
                        localVm.BrightnessValue = localVm.PR1BrightnessValue;
                        localVm.ContrastValue = localVm.PR1ContrastValue;
                    }
                    else
                        localVm.LuminanceValue = localVm.PR1LuminanceValue;
                }
            }
        }

        private async void PR1_Preview_UXButton_Click(object sender, RoutedEventArgs e)
        {
            BrightnessViewModel localVm = (BrightnessViewModel)DataContext;
            if (!localVm.IsPR1Preview)
            {
                localVm.IsPR1Preview = true;
                localVm.IsPR2Preview = false;

                using (var tokenSource = new CancellationTokenSource())
                {
                    try
                    {
                        localVm.PreviewToken = tokenSource;
                        var token = localVm.PreviewToken.Token;

                        //TODO: May be you'll want to add .ConfigureAwait(false);
                        await Task.Run(() => ShowPreview(1, localVm, token), token).ConfigureAwait(false);
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
                        localVm.IsPR1Preview = false;
                        localVm.IsPR2Preview = false;

                        localVm.BrightnessValue = localVm.PR1BrightnessValue;
                        localVm.ContrastValue = localVm.PR1ContrastValue;

                        localVm.PreviewToken.Dispose();
                        tokenSource.Dispose();
                    }
                }
            }
            else
            {
                try
                {
                    if (localVm.PreviewToken != null && !localVm.PreviewToken.IsCancellationRequested)
                        localVm.PreviewToken.Cancel();
                }
                catch (Exception) { }
                finally
                {
                    localVm.IsPR1Preview = false;
                    localVm.IsPR2Preview = false;

                    localVm.BrightnessValue = localVm.PR1BrightnessValue;
                    localVm.ContrastValue = localVm.PR1ContrastValue;
                }
            }
        }

        private void PR1Name_TextChanged(object sender, TextChangedEventArgs e)
        {
            BrightnessViewModel localVm = (BrightnessViewModel)DataContext;
            TextString textString = new TextString();
            TextBox tb = sender as TextBox;
            if (textString.CheckChar(tb.Text))
                localVm.PR1Name = tb.Text;
        }

        private async void PR2_Luminance_Preview_UXButton_Click(object sender, RoutedEventArgs e)
        {
            BrightnessViewModel localVm = (BrightnessViewModel)DataContext;
            if (!localVm.IsPR2_Luminance_Preview)
            {
                localVm.IsPR2_Luminance_Preview = true;
                localVm.IsPR1_Luminance_Preview = false;

                using (var tokenSource = new CancellationTokenSource())
                {
                    try
                    {
                        localVm.PreviewToken = tokenSource;
                        var token = localVm.PreviewToken.Token;

                        //TODO: May be you'll want to add .ConfigureAwait(false);
                        await Task.Run(() => ShowPreview(2, localVm, token), token).ConfigureAwait(false);
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
                        localVm.IsPR2_Luminance_Preview = false;
                        localVm.IsPR1_Luminance_Preview = false;

                        if (!(localVm.isLuminanceSupport == Visibility.Visible))
                        {
                            localVm.BrightnessValue = localVm.PR2BrightnessValue;
                            localVm.ContrastValue = localVm.PR2ContrastValue;
                        }
                        else
                            localVm.LuminanceValue = localVm.PR2LuminanceValue;

                        localVm.PreviewToken.Dispose();
                        tokenSource.Dispose();
                    }
                }
            }
            else
            {
                try
                {
                    if (localVm.PreviewToken != null && !localVm.PreviewToken.IsCancellationRequested)
                        localVm.PreviewToken.Cancel();
                }
                catch (Exception) { }
                finally
                {
                    localVm.IsPR2_Luminance_Preview = false;
                    localVm.IsPR1_Luminance_Preview = false;

                    if (!(localVm.isLuminanceSupport == Visibility.Visible))
                    {
                        localVm.BrightnessValue = localVm.PR2BrightnessValue;
                        localVm.ContrastValue = localVm.PR2ContrastValue;
                    }
                    else
                        localVm.LuminanceValue = localVm.PR2LuminanceValue;
                }
            }
        }

        private async void PR2_Preview_UXButton_Click(object sender, RoutedEventArgs e)
        {
            BrightnessViewModel localVm = (BrightnessViewModel)DataContext;
            if (!localVm.IsPR2Preview)
            {
                localVm.IsPR2Preview = true;
                localVm.IsPR1Preview = false;

                using (var tokenSource = new CancellationTokenSource())
                {
                    try
                    {
                        localVm.PreviewToken = tokenSource;
                        var token = localVm.PreviewToken.Token;

                        //TODO: May be you'll want to add .ConfigureAwait(false);
                        await Task.Run(() => ShowPreview(2, localVm, token), token).ConfigureAwait(false);
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
                        localVm.IsPR2Preview = false;
                        localVm.IsPR1Preview = false;

                        localVm.BrightnessValue = localVm.PR2BrightnessValue;
                        localVm.ContrastValue = localVm.PR2ContrastValue;

                        localVm.PreviewToken.Dispose();
                        tokenSource.Dispose();
                    }
                }
            }
            else
            {
                try
                {
                    if (localVm.PreviewToken != null && !localVm.PreviewToken.IsCancellationRequested)
                        localVm.PreviewToken.Cancel();
                }
                catch (Exception) { }
                finally
                {
                    localVm.IsPR2Preview = false;
                    localVm.IsPR1Preview = false;

                    localVm.BrightnessValue = localVm.PR2BrightnessValue;
                    localVm.ContrastValue = localVm.PR2ContrastValue;
                }
            }
        }

        private void PR2Name_TextChanged(object sender, TextChangedEventArgs e)
        {
            BrightnessViewModel localVm = (BrightnessViewModel)DataContext;
            TextString textString = new TextString();
            TextBox tb = sender as TextBox;
            if (textString.CheckChar(tb.Text))
                localVm.PR2Name = tb.Text;
        }

        private void Reset_Click(object sender, RoutedEventArgs e)
        {
            BrightnessViewModel x = (BrightnessViewModel)DataContext;
            x.ResetClick();
        }

        private void Schedule_Luminance_prest1_Border_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            BrightnessViewModel localVm = (BrightnessViewModel)DataContext;
            localVm.IsMouseEnterSchedule_1 = true;
            localVm.UpdataScheduleBoaderUI();

            if (!localVm.IsPR1_Luminance_Preview && !localVm.IsPR2_Luminance_Preview)
            {
                Task.Run(() =>
                {
                    localVm.StopScheduleManger();
                    localVm.LuminanceValue = localVm.PR1LuminanceValue;
                });
            }
        }

        private void Schedule_Luminance_prest1_Border_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            BrightnessViewModel localVm = (BrightnessViewModel)DataContext;
            localVm.IsMouseEnterSchedule_1 = false;
            localVm.UpdataScheduleBoaderUI();

            if (!localVm.IsPR1_Luminance_Preview && !localVm.IsPR2_Luminance_Preview)
            {
                Task.Run(() =>
                {
                    localVm.CalculateNowValue();
                    Leave_Luminance_WriteToConfig_Debouncer.Debounce(localVm);
                });
            }
        }

        private void Schedule_Luminance_prest2_Border_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            BrightnessViewModel localVm = (BrightnessViewModel)DataContext;
            localVm.IsMouseEnterSchedule_2 = true;
            localVm.UpdataScheduleBoaderUI();

            if (!localVm.IsPR1_Luminance_Preview && !localVm.IsPR2_Luminance_Preview)
            {
                Task.Run(() =>
                {
                    localVm.StopScheduleManger();
                    localVm.LuminanceValue = localVm.PR2LuminanceValue;
                });
            }
        }

        private void Schedule_Luminance_prest2_Border_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            BrightnessViewModel localVm = (BrightnessViewModel)DataContext;
            localVm.IsMouseEnterSchedule_2 = false;
            localVm.UpdataScheduleBoaderUI();

            if (!localVm.IsPR1_Luminance_Preview && !localVm.IsPR2_Luminance_Preview)
            {
                Task.Run(() =>
                {
                    localVm.CalculateNowValue();
                    Leave_Luminance_WriteToConfig_Debouncer.Debounce(localVm);
                });
            }
        }

        private void Schedule_prest1_Border_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            BrightnessViewModel localVm = (BrightnessViewModel)DataContext;
            localVm.IsMouseEnterSchedule_1 = true;
            localVm.UpdataScheduleBoaderUI();

            if (!localVm.IsPR1Preview && !localVm.IsPR2Preview)
            {
                Task.Run(() =>
                {
                    localVm.StopScheduleManger();
                    localVm.BrightnessValue = localVm.PR1BrightnessValue;
                    localVm.ContrastValue = localVm.PR1ContrastValue;
                });
            }
        }

        private void Schedule_prest1_Border_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            BrightnessViewModel localVm = (BrightnessViewModel)DataContext;
            localVm.IsMouseEnterSchedule_1 = false;
            localVm.UpdataScheduleBoaderUI();

            if (!localVm.IsPR1Preview && !localVm.IsPR2Preview)
            {
                Task.Run(() =>
                {
                    localVm.CalculateNowValue();
                    Leave_WriteToConfig_Debouncer.Debounce(localVm);
                });
            }
        }

        private void Schedule_prest2_Border_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            BrightnessViewModel localVm = (BrightnessViewModel)DataContext;
            localVm.IsMouseEnterSchedule_2 = true;
            localVm.UpdataScheduleBoaderUI();

            if (!localVm.IsPR1Preview && !localVm.IsPR2Preview)
            {
                Task.Run(() =>
                {
                    localVm.StopScheduleManger();
                    localVm.BrightnessValue = localVm.PR2BrightnessValue;
                    localVm.ContrastValue = localVm.PR2ContrastValue;
                });
            }
        }

        private void Schedule_prest2_Border_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            BrightnessViewModel localVm = (BrightnessViewModel)DataContext;
            localVm.IsMouseEnterSchedule_2 = false;
            localVm.UpdataScheduleBoaderUI();

            if (!localVm.IsPR1Preview && !localVm.IsPR2Preview)
            {
                Task.Run(() =>
                {
                    localVm.CalculateNowValue();
                    Leave_WriteToConfig_Debouncer.Debounce(localVm);
                });
            }
        }

        private void ShowPreview(int pr, BrightnessViewModel vm, CancellationToken token)
        {
            var Brightness_PR1 = vm.PR1BrightnessValue;
            var Brightness_PR2 = vm.PR2BrightnessValue;
            var Luminance_PR1 = vm.PR1LuminanceValue;
            var Luminance_PR2 = vm.PR2LuminanceValue;
            var Brightness_difference = (vm.isLuminanceSupport == Visibility.Visible) ? (Luminance_PR1 - Luminance_PR2) : (Brightness_PR1 - Brightness_PR2);
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
            var PerStepValue = (!(vm.isLuminanceSupport == Visibility.Visible)) ? 20 : 50;
            var BrightnessSteps = Brightness_difference / PerStepValue;
            var BrightnessSteps_msec = (BrightnessSteps == 0) ? 0 : Convert.ToInt32(TimeDemoPreview_msec / BrightnessSteps);

            var ContrastSteps = Contrast_difference / PerStepValue;
            var ContrastSteps_msec = (ContrastSteps == 0) ? 0 : Convert.ToInt32(TimeDemoPreview_msec / ContrastSteps);

            int BrightnessSteps_msec_counter = 0;
            int ContrastSteps_msec_counter = 0;

            switch (pr)
            {
                case 1:
                    {
                        if (!(vm.isLuminanceSupport == Visibility.Visible))
                        {
                            vm.BrightnessValue = Brightness_PR2;
                            vm.ContrastValue = Contrast_PR2;
                        }
                        else
                            vm.LuminanceValue = Luminance_PR2;

                        if ((!(BrightnessSteps_msec == 0 && ContrastSteps_msec == 0 && (!(vm.isLuminanceSupport == Visibility.Visible)))) || (!((BrightnessSteps_msec == 0) && (vm.isLuminanceSupport == Visibility.Visible))))
                        {
                            int Count = 0;
                            while (Count <= TimeDemoPreview_msec)
                            {
                                if (BrightnessSteps_msec_counter == BrightnessSteps_msec)
                                {
                                    if (IsBrightnessPR1Plus)
                                    {
                                        if ((!(vm.isLuminanceSupport == Visibility.Visible)))
                                            vm.BrightnessValue += PerStepValue;
                                        else
                                            vm.LuminanceValue += PerStepValue;
                                    }
                                    else
                                    {
                                        if ((!(vm.isLuminanceSupport == Visibility.Visible)))
                                            vm.BrightnessValue -= PerStepValue;
                                        else
                                            vm.LuminanceValue -= PerStepValue;
                                    }

                                    BrightnessSteps_msec_counter = 0;
                                }

                                if (!(vm.isLuminanceSupport == Visibility.Visible) &&
                                    ContrastSteps_msec_counter == ContrastSteps_msec)
                                {
                                    if (IsContrastPR1Plus)
                                        vm.ContrastValue += PerStepValue;
                                    else
                                        vm.ContrastValue -= PerStepValue;

                                    ContrastSteps_msec_counter = 0;
                                }

                                Task.Delay(1000).Wait();
                                BrightnessSteps_msec_counter++;
                                ContrastSteps_msec_counter++;
                                Count++;
                            }
                        }

                        if (!(vm.isLuminanceSupport == Visibility.Visible))
                        {
                            vm.BrightnessValue = Brightness_PR1;
                            vm.ContrastValue = Contrast_PR1;
                        }
                        else
                            vm.LuminanceValue = Luminance_PR1;
                    }
                    break;

                case 2:
                    {
                        if (!(vm.isLuminanceSupport == Visibility.Visible))
                        {
                            vm.BrightnessValue = Brightness_PR1;
                            vm.ContrastValue = Contrast_PR1;
                        }
                        else
                            vm.LuminanceValue = Luminance_PR1;

                        if ((!(BrightnessSteps_msec == 0 && ContrastSteps_msec == 0 && (!(vm.isLuminanceSupport == Visibility.Visible)))) || (!((BrightnessSteps_msec == 0) && (vm.isLuminanceSupport == Visibility.Visible))))
                        {
                            int Count = 0;
                            while (Count <= TimeDemoPreview_msec)
                            {
                                if (BrightnessSteps_msec_counter == BrightnessSteps_msec)
                                {
                                    if (IsBrightnessPR2Plus)
                                    {
                                        if ((!(vm.isLuminanceSupport == Visibility.Visible)))
                                            vm.BrightnessValue += PerStepValue;
                                        else
                                            vm.LuminanceValue += PerStepValue;
                                    }
                                    else
                                    {
                                        if ((!(vm.isLuminanceSupport == Visibility.Visible)))
                                            vm.BrightnessValue -= PerStepValue;
                                        else
                                            vm.LuminanceValue -= PerStepValue;
                                    }

                                    BrightnessSteps_msec_counter = 0;
                                }

                                if (!(vm.isLuminanceSupport == Visibility.Visible) &&
                                    ContrastSteps_msec_counter == ContrastSteps_msec)
                                {
                                    if (IsContrastPR2Plus)
                                        vm.ContrastValue += PerStepValue;
                                    else
                                        vm.ContrastValue -= PerStepValue;

                                    ContrastSteps_msec_counter = 0;
                                }

                                Task.Delay(1000).Wait();
                                BrightnessSteps_msec_counter++;
                                ContrastSteps_msec_counter++;
                                Count++;
                            }
                        }

                        if (!(vm.isLuminanceSupport == Visibility.Visible))
                        {
                            vm.BrightnessValue = Brightness_PR2;
                            vm.ContrastValue = Contrast_PR2;
                        }
                        else
                            vm.LuminanceValue = Luminance_PR2;
                    }
                    break;

                default:
                    break;
            }
        }

        private void Synchronize_LuminanceScheduled(BrightnessViewModel _vm)
        {
            if (_vm.IsSynchronize_Scheduled)
            {
                Task.Run(() =>
                {
                    foreach (HomeDevice hd in _vm.ModuleOwner.HomeDevices)
                    {
                        if (hd.MonitorInfo.IsDellMonitor)
                        {
                            if (!hd.MonitorInfo.CapabilityDic.ContainsKey("12"))
                            {
                                if (hd.MonitorInfo.modelName.ToUpper().Equals("UP2720Q") && ((vm.ScheduleMap.Brightness1 > 250) || (vm.ScheduleMap.Brightness2 > 250)))
                                {
                                    var ScheduleMap = new scheduleInfo(vm.ScheduleMap);

                                    if (vm.ScheduleMap.Brightness1 > 250)
                                        ScheduleMap.Brightness1 = 250;  // UP2720Q max luminance is 250

                                    if (vm.ScheduleMap.Brightness2 > 250)
                                        ScheduleMap.Brightness2 = 250;  // UP2720Q max luminance is 250

                                    DdpmCommonHelper.DeviceManagerSA.WriteScheduleMonitorSettings(hd.MonitorInfo, ScheduleMap);
                                }
                                else
                                    DdpmCommonHelper.DeviceManagerSA.WriteScheduleMonitorSettings(hd.MonitorInfo, _vm.ScheduleMap);
                            }
                        }
                    }
                });
            }
        }

        private void Synchronize_LuminanceScheduledSwitch_Click(object sender, RoutedEventArgs e)
        {
            BrightnessViewModel _vm = (BrightnessViewModel)DataContext;

            DDPMSettings setting = DdpmCommonHelper.ReadDDPMSettings();// DeviceManagerSA.ReloadAppConfigData().Result;

            if ((bool)ScheduledLuminanceSynchronizeSwitch.IsChecked)
            {
                _vm.IsSynchronize_Scheduled = true;
                //SynchronizeSwitch.Content = Strings.On;

                Synchronize_LuminanceScheduled(_vm);
            }
            else
            {
                _vm.IsSynchronize_Scheduled = false;
                //SynchronizeSwitch.Content = Strings.Off;
            }

            setting.UserSettings.IsSynchronizemonitor_Scheduled = _vm.IsSynchronize_Scheduled;
            DdpmCommonHelper.WriteDDPMSettings(setting);// DeviceManagerSA.SetAppConfigData(setting);
        }

        private void Synchronize_LuminanceSwitch_Click(object sender, RoutedEventArgs e)
        {
            BrightnessViewModel _vm = (BrightnessViewModel)DataContext;

            DDPMSettings setting = DdpmCommonHelper.ReadDDPMSettings();// DeviceManagerSA.ReloadAppConfigData().Result;

            if ((bool)SynchronizeSwitch_Luminance.IsChecked)
            {
                _vm.IsSynchronize = true;
                //SynchronizeSwitch.Content = Strings.On;

                // Luminance
                _vm.Luminance_Sync();

                // Color
                _vm.Invoke_ColorPreset_Sync();
            }
            else
            {
                _vm.IsSynchronize = false;
                //SynchronizeSwitch.Content = Strings.Off;
            }

            setting.UserSettings.IsSynchronizemonitor = _vm.IsSynchronize;
            DdpmCommonHelper.WriteDDPMSettings(setting);// DeviceManagerSA.SetAppConfigData(setting);
        }

        private void Synchronize_Scheduled(BrightnessViewModel _vm)
        {
            if (_vm.IsSynchronize_Scheduled)
            {
                Task.Run(() =>
                {
                    foreach (HomeDevice hd in _vm.ModuleOwner.HomeDevices)
                    {
                        if (hd.MonitorInfo.IsDellMonitor &&
                            hd.MonitorInfo.CapabilityDic.ContainsKey("12") &&
                            !hd.MonitorInfo.CapabilityDic.ContainsKey("66"))
                            DdpmCommonHelper.DeviceManagerSA.WriteScheduleMonitorSettings(hd.MonitorInfo, _vm.ScheduleMap);
                    }
                });
            }
        }

        private void Synchronize_ScheduledSwitch_Click(object sender, RoutedEventArgs e)
        {
            BrightnessViewModel _vm = (BrightnessViewModel)DataContext;

            DDPMSettings setting = DdpmCommonHelper.ReadDDPMSettings();// DeviceManagerSA.ReloadAppConfigData().Result;

            if ((bool)ScheduledSynchronizeSwitch.IsChecked)
            {
                _vm.IsSynchronize_Scheduled = true;
                //SynchronizeSwitch.Content = Strings.On;

                Synchronize_Scheduled(_vm);
            }
            else
            {
                _vm.IsSynchronize_Scheduled = false;
                //SynchronizeSwitch.Content = Strings.Off;
            }

            setting.UserSettings.IsSynchronizemonitor_Scheduled = _vm.IsSynchronize_Scheduled;
            DdpmCommonHelper.WriteDDPMSettings(setting);// DeviceManagerSA.SetAppConfigData(setting);
        }

        private void SynchronizeSwitch_Click(object sender, RoutedEventArgs e)
        {
            BrightnessViewModel _vm = (BrightnessViewModel)DataContext;

            DDPMSettings setting = DdpmCommonHelper.ReadDDPMSettings();// DeviceManagerSA.ReloadAppConfigData().Result;

            if ((bool)SynchronizeSwitch.IsChecked)
            {
                _vm.IsSynchronize = true;
                //SynchronizeSwitch.Content = Strings.On;

                // Brightness and contrast
                _vm.BR_Con_Sync();

                // Color
                _vm.Invoke_ColorPreset_Sync();
            }
            else
            {
                _vm.IsSynchronize = false;
                //SynchronizeSwitch.Content = Strings.Off;
            }

            setting.UserSettings.IsSynchronizemonitor = _vm.IsSynchronize;
            DdpmCommonHelper.WriteDDPMSettings(setting);// DeviceManagerSA.SetAppConfigData(setting);
        }

        //Elsa add for tooltip issue fix
        private void toolTip_Opened(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.ToolTip? target = sender as System.Windows.Controls.ToolTip;
            if (target == null)
                return;
            double screenScaleX = GetScreenScaleX();
            Window mainWindow = System.Windows.Application.Current.MainWindow;
            double winRightX = (mainWindow.Left + mainWindow!.ActualWidth) * screenScaleX;
            double mousepositionX = System.Windows.Forms.Cursor.Position.X;
            double mouseaddtooltip = mousepositionX + target.ActualWidth * screenScaleX;
            if (winRightX > mouseaddtooltip)
            {
                target.HorizontalOffset = 2;
            }
            else
            {
                target.HorizontalOffset = -1 * target.ActualWidth + 14;
            }
        }

        private void WriteToConfig(object VM_)
        {
            BrightnessViewModel localVm = (BrightnessViewModel)VM_;

            localVm.WriteToConfig();

            if (localVm.SelectedHomeDevice.MonitorInfo.CapabilityDic.ContainsKey("12"))
                Synchronize_Scheduled(localVm);
            else
                Synchronize_LuminanceScheduled(localVm);
        }
    }
}