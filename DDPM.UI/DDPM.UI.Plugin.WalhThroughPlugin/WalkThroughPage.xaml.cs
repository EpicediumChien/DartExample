using DDPM.SA.Common.Settings;
using DDPM.UI.Common;
using Dell.Client.Framework.UX.WPF;
using System.Diagnostics;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace DDPM.UI.Plugin.WalkThroughPlugin
{
    /// <summary>
    /// SettingsPage.xaml 的互動邏輯
    /// </summary>
    public partial class WalkThroughPage : UserControl
    {
        private WalkThroughPageViewModel vm
        {
            get { return (WalkThroughPageViewModel)DataContext; }
        }
        public WalkThroughPage()
        {
            InitializeComponent();
            DataContext = new WalkThroughPageViewModel();
        }

        ~WalkThroughPage()
        {
        }

        private void NextBtn_Click(object sender, RoutedEventArgs e)
        {
            var viewModel = (WalkThroughPageViewModel)this.DataContext;

            if (viewModel.ProgressValue < 5)
            {
                viewModel.ProgressValue += 1;

                DoubleAnimation progressAnimation = new DoubleAnimation
                {
                    From = viewModel.ProgressValue - 1,
                    To = viewModel.ProgressValue,
                    Duration = new Duration(TimeSpan.FromSeconds(1)),
                    FillBehavior = FillBehavior.HoldEnd
                };
                WalkThroughProgressbar.BeginAnimation(ProgressBar.ValueProperty, progressAnimation);
            }

            // 更新
            viewModel.MainText = "Convenient Meeting Controls";
            viewModel.SubText = "Streamline your meetings with Collaboration Touch Controls that offer shortcuts to essential features in Zoom and Microsoft Teams";
            viewModel.MainImageSource = "pack://application:,,,/DDPM.UI.Common;component/Resources/WalkThrough/Keyboard/Trident (KB900)/Walkthrough Image KB900_2.png";
        }
    }
}