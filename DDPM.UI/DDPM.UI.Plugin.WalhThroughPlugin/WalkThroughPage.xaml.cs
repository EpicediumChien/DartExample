using DDPM.SA.Common;
using DDPM.UI.Common;
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
        public WalkThroughPage()
        {
            InitializeComponent();
            DataContext = new WalkThroughPageViewModel();

            ViewModel.ControlIcon(true);

            // Consent Page wording
            txtYes.Content = LangHelper.Instance["ConsentYes"];
            txtNo.Content = LangHelper.Instance["ConsentNo"];
            txtCaption.Text = LangHelper.Instance["Consent.1"];
            txtCaption2.Text = LangHelper.Instance["AppName"];
            txt1.Text = LangHelper.Instance["Consent.2"];
            txt2.Text = LangHelper.Instance["Analytics.2"];
        }

        ~WalkThroughPage()
        {
        }

        private void SkipBtn_Click(object sender, RoutedEventArgs e)
        {
            if (DdpmHomePlugin.DdpmHomePlugin.WalkThroughQueue.Count > 0)
            {
                ViewModel.UpdateLastlogicalDeviceType();
                ViewModel.WriteWalkThroughReg(DdpmHomePlugin.DdpmHomePlugin.WalkThroughQueue[0].ModelName);
                DdpmHomePlugin.DdpmHomePlugin.WalkThroughEndList.Add(DdpmHomePlugin.DdpmHomePlugin.WalkThroughQueue[0]);
                DdpmHomePlugin.DdpmHomePlugin.WalkThroughQueue.RemoveAt(0);
                DdpmCommonHelper.WriteUILog($"[WalkThroughPageViewModel] InitializeDeviceFromQueue RemoveAt {DdpmHomePlugin.DdpmHomePlugin.WalkThroughQueue[0].ModelName}");
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

        private void MainNextBtn_Click(object sender, RoutedEventArgs e)
        {
            WalkThroughBox msgBox = new WalkThroughBox(ViewModel, Window.GetWindow(this));
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
            _ = DdpmCommonHelper.DeviceManagerSA.Set_GlobalSetting_EnableTelemetryConsent(false).Result;
            //DialogResult = false;
            Close();
        }

        private void Yes_MouseLeftButtonDown(object sender, RoutedEventArgs e)
        {
            _ = DdpmCommonHelper.DeviceManagerSA.Set_GlobalSetting_EnableTelemetryConsent(true).Result;
            //DialogResult = true;
            Close();
        }

        private void Close()
        {
            ViewModel.ConsentPageVisibility = Visibility.Collapsed;
        }
    }
}
