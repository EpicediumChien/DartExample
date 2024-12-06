using DDPM.SA.Common;
using DDPM.UI.Common;
using Dell.Client.Framework.Common;
using Dell.Client.Framework.UX.WPF;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using Windows.Devices.Geolocation;

namespace DDPM.UI.Plugin.WalkThroughPlugin
{
    /// <summary>
    /// SettingsPage.xaml
    /// </summary>
    public partial class WalkThroughPage : UserControl
    {
        private WalkThroughPageViewModel ViewModel => (WalkThroughPageViewModel)DataContext;
        public WalkThroughPage()
        {
            InitializeComponent();
            DataContext = new WalkThroughPageViewModel();

            ViewModel.ControlIcon(false);
        }

        ~WalkThroughPage()
        {
        }

        private void SkipBtn_Click(object sender, RoutedEventArgs e)
        {
            if (DdpmHomePlugin.DdpmHomePlugin.WalkThroughQueue.Count > 0)
            {
                ViewModel.UpdateLastlogicalDeviceType();
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

            msgBox.ShowDialog();

            if (DdpmHomePlugin.DdpmHomePlugin.WalkThroughQueue.Count == 0)
            {
                ViewModel.EndWalkThrough();
            }
            else
            {
                if (!DdpmHomePlugin.DdpmHomePlugin.WalkThroughQueue.Exists(info => info.ModelName == "DDPM"))
                {
                    ViewModel.IsPeripheralVisible = true;
                    ViewModel.IsDDPMVisibility = false;
                    ViewModel.InitializeDeviceFromQueue();
                    ViewModel.UpdateButtonVisibility();
                }
            }
        }
    }
}
