using DDPM.SA.Common;
using DDPM.UI.Common;
using DDPM.UI.Plugin.DdpmHomePlugin.Interfaces;
using DDPM.UI.Resources.Helper;
using Dell.Client.Framework.Common;
using Dell.Client.Framework.UX.WPF;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using Windows.Devices.Geolocation;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;

namespace DDPM.UI.Plugin.WalkThroughPlugin
{
    /// <summary>
    /// SettingsPage.xaml
    /// </summary>
    public partial class WalkThroughPage : UserControl
    {
        private string PrivacyUrl = "https://www.dell.com/learn/us/en/uscorp1/policies-privacy-country-specific-privacy-policy";
        private WalkThroughPageViewModel ViewModel => (WalkThroughPageViewModel)DataContext;
        private WalkThroughBox? msgBox;
        public WalkThroughPage()
        {
            DdpmCommonHelper.WriteUILog($"[WalkThroughPage] WalkThroughPage Constructed ... in ");
            try
            {
                InitializeComponent();
                DataContext = new WalkThroughPageViewModel();

                //ViewModel.ControlIcon(true, false);

                Application.Current.MainWindow.MouseLeftButtonUp -= MouseDragEvent;
                Application.Current.MainWindow.MouseLeftButtonUp += MouseDragEvent;
            }
            catch (Exception ex)
            {
                DdpmCommonHelper.WriteUILog($"[WalkThroughPage] WalkThroughPage Constructed Exception: {ex.Message}");
            }
        }

        ~WalkThroughPage()
        {
            ViewModel.ControlIcon(true, true);
            Application.Current.MainWindow.MouseLeftButtonUp -= MouseDragEvent;
            DdpmCommonHelper.WriteUILog($"[WalkThroughPage] ~WalkThroughPage() ... ");
        }

        private void SkipBtn_Click(object sender, RoutedEventArgs e)
        {
            skip_WalkThroughUnit();
        }

        private void NextBtn_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.NextPage();
            DoProgressAnimation(true);
        }

        private void ArrowButton_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.PreviousPage();
            DoProgressAnimation(false);
        }

        private void DoProgressAnimation(bool isForward)
        {
            double newProgressValue;
            try
            {
                if (isForward)
                {
                    // Move
                    newProgressValue = Math.Min(ViewModel.ProgressValue + 1, ViewModel.CurrentAnimationPage);
                }
                else
                {
                    // Back
                    newProgressValue = Math.Max(ViewModel.ProgressValue - 1, 1);
                }

                DoubleAnimation progressAnimation = new DoubleAnimation
                {
                    From = ViewModel.ProgressValue,
                    To = newProgressValue,
                    Duration = new Duration(TimeSpan.FromSeconds(0.5)), // Time
                    FillBehavior = FillBehavior.HoldEnd
                };

                WalkThroughProgressbar.BeginAnimation(ProgressBar.ValueProperty, progressAnimation);

                // refresh ProgressValue
                ViewModel.ProgressValue = newProgressValue;
            }
            catch (Exception ex)
            {
                DdpmCommonHelper.WriteUILog($"[WalkThroughPage] DoProgressAnimation Exception: {ex.Message}");
            }
        }

        private void MainNextBtn_Click(object sender, RoutedEventArgs e)
        {
            DdpmCommonHelper.WriteUILog($"[WalkThroughPage] MainNextBtn_Click in ... ");
            double scalingFactor = DdpmCommonHelper.GetScalingFactor(Application.Current.MainWindow);
            msgBox = new WalkThroughBox(ViewModel, Application.Current.MainWindow, scalingFactor);
            msgBox.WindowStartupLocation = WindowStartupLocation.Manual;
            //msgBox.ShowDialog();
            msgBox.Show();
            //if (DdpmHomePlugin.DdpmHomePlugin.WalkThroughQueue.Count == 0)
            //{
            //    ViewModel.EndWalkThrough();
            //}
            //else
            //{
            //    if (!DdpmHomePlugin.DdpmHomePlugin.WalkThroughQueue.Exists(info => info.ModelName == "DDPM"))
            //    {
            //        ViewModel.IsPeripheralVisible = true;
            //        ViewModel.IsDDPMVisibility = false;
            //        ViewModel.InitializeDeviceFromQueue();
            //        ViewModel.UpdateButtonVisibility();
            //    }
            //}
        }

        private void OpenPrivacy(object sender, MouseButtonEventArgs e)
        {
            DdpmCommonHelper.WriteUILog($"[WalkThroughPage] OpenPrivacy in ... ");
            DDPM.SA.Common.Settings.DDPMFileSecurity.StartProcessSafely(
                null,
                new ProcessStartInfo
                {
                    FileName = PrivacyUrl,
                    UseShellExecute = true
                });
        }

        private void No_MouseLeftButtonDown(object sender, RoutedEventArgs e)
        {
            try
            {
                if (DdpmCommonHelper.DeviceManagerSA != null)
                {
                    DdpmCommonHelper.WriteUILog($"[WalkThroughPage] No_MouseLeftButtonDown Set_GlobalSetting_EnableTelemetryConsent set false in ... ");
                    //bool result = Task.Run(() => DdpmCommonHelper.DeviceManagerSA.Set_GlobalSetting_EnableTelemetryConsent(false)).Result;
                    //DdpmCommonHelper.WriteUILog($"[WalkThroughPage] No_MouseLeftButtonDown Set_GlobalSetting_EnableTelemetryConsent set false out : {result.ToString()} ... ");
                    DdpmCommonHelper.Set_GlobalSettings(DdpmCommonHelper.GlobalSettingsType.Consent, false);
                    //DialogResult = false;
                    Close();
                }
                else
                {
                    DdpmCommonHelper.WriteUILog($"[WalkThroughPage] No_MouseLeftButtonDown Set_GlobalSetting_EnableTelemetryConsent DdpmCommonHelper.DeviceManagerSA null ... ");
                }
            }
            catch (Exception ex)
            {
                DdpmCommonHelper.WriteUILog($"[WalkThroughPage] No_MouseLeftButtonDown Exception: {ex.Message}");
            }
        }

        private void Yes_MouseLeftButtonDown(object sender, RoutedEventArgs e)
        {
            try
            {
                if (DdpmCommonHelper.DeviceManagerSA != null)
                {
                    DdpmCommonHelper.WriteUILog($"[WalkThroughPage] Yes_MouseLeftButtonDown Set_GlobalSetting_EnableTelemetryConsent set true in ... ");
                    //bool result = Task.Run(() => DdpmCommonHelper.DeviceManagerSA.Set_GlobalSetting_EnableTelemetryConsent(true)).Result;
                    //DdpmCommonHelper.WriteUILog($"[WalkThroughPage] Yes_MouseLeftButtonDown Set_GlobalSetting_EnableTelemetryConsent set true out : {result.ToString()} ... ");
                    DdpmCommonHelper.Set_GlobalSettings(DdpmCommonHelper.GlobalSettingsType.Consent, true);
                    Close();
                }
                else
                {
                    DdpmCommonHelper.WriteUILog($"[WalkThroughPage] Yes_MouseLeftButtonDown Set_GlobalSetting_EnableTelemetryConsent DdpmCommonHelper.DeviceManagerSA null ... ");
                }
            }
            catch (Exception ex)
            {
                DdpmCommonHelper.WriteUILog($"[WalkThroughPage] Yes_MouseLeftButtonDown Exception: {ex.Message}");
            }
        }

        private void Close()
        {
            try
            {
                DdpmCommonHelper.WriteUILog($"[WalkThroughPage] Close in ... ");
                ViewModel.WriteWalkThroughReg("CONSENT_PAGE");
                DdpmHomePlugin.DdpmHomePlugin.WalkThroughEndList.Add(new WalkThroughInfo("CONSENT_PAGE", "CONSENT_PAGE", null)); // Add DDPM to the end of the queue
                DdpmHomePlugin.DdpmHomePlugin.WalkThroughQueue.RemoveAll(item => item.ModelName == "CONSENT_PAGE"); // Remove all DDPM from the queue
                if (DdpmHomePlugin.DdpmHomePlugin.WalkThroughQueue.Exists(info => info.ModelName == "DDPM"))
                {
                    ViewModel.SwitchToDDPMPage();
                    DdpmCommonHelper.WriteUILog($"[WalkThroughPage] Close, SwitchToDDPMPage ... ");
                }
                else
                {
                    ViewModel.IsConsentPageVisible = false;
                    ViewModel.IsPeripheralVisible = true;
                    skip_WalkThroughUnit();
                    DdpmCommonHelper.WriteUILog($"[WalkThroughPage] Close, skip_WalkThroughUnit ... ");
                }
            }
            catch (Exception ex)
            {
                DdpmCommonHelper.WriteUILog($"[WalkThroughPage] Close Exception: {ex.Message}");
            }
        }

        private void MouseDragEvent(object sender, RoutedEventArgs e)
        {
            if (msgBox != null)
            {
                msgBox.RefreshWalkThroughBoxPosition();
            }
        }

        private void skip_WalkThroughUnit()
        {
            try
            {
                DdpmCommonHelper.WriteUILog($"[WalkThroughPage] skip_WalkThroughUnit, WalkThroughQueue.Count : {DdpmHomePlugin.DdpmHomePlugin.WalkThroughQueue.Count.ToString()} in ...");
                if (DdpmHomePlugin.DdpmHomePlugin.WalkThroughQueue.Count > 0)
                {
                    ViewModel.UpdateLastlogicalDeviceType();
                    ViewModel.WriteWalkThroughReg(DdpmHomePlugin.DdpmHomePlugin.WalkThroughQueue[0].ModelName);
                    DdpmHomePlugin.DdpmHomePlugin.WalkThroughEndList.Add(DdpmHomePlugin.DdpmHomePlugin.WalkThroughQueue[0]);
                    DdpmCommonHelper.WriteUILog($"[WalkThroughPage] InitializeDeviceFromQueue RemoveAt {DdpmHomePlugin.DdpmHomePlugin.WalkThroughQueue[0].ModelName}");
                    DdpmHomePlugin.DdpmHomePlugin.WalkThroughQueue.RemoveAt(0);
                    if (DdpmHomePlugin.DdpmHomePlugin.WalkThroughQueue.Count > 0)
                    {
                        ViewModel.InitializeDeviceFromQueue();
                    }
                    else
                    {
                        ViewModel.EndWalkThrough();
                    }
                }
                else
                {
                    ViewModel.EndWalkThrough();
                }
            }
            catch (Exception ex)
            {
                DdpmCommonHelper.WriteUILog($"[WalkThroughPage] skip_WalkThroughUnit Exception: {ex.Message}");
            }
        }

        private void AppleStore_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Process.Start(new ProcessStartInfo("https://apps.apple.com/us/app/dell-audio/id6472411862") { UseShellExecute = true });

        }

        private void GooglePlay_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Process.Start(new ProcessStartInfo("https://play.google.com/store/apps/details?id=com.dell.dellaudio&pli=1") { UseShellExecute = true });

        }

        private void CNAppleStore_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
             Process.Start(new ProcessStartInfo("https://apps.apple.com/cn/app/dell-audio-%E8%BD%AF%E4%BB%B6/id6677017100") { UseShellExecute = true });

        }

        private void CNGooglePlay_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Process.Start(new ProcessStartInfo("https://sj.qq.com/appdetail/com.dell.dellaudio") { UseShellExecute = true });

        }


        private void UXTextBlock_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ViewModel.IsFullQRCode = true;
        }

        private void ArrowLeft_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ViewModel.IsFullQRCode = false;

        }

    }
}
