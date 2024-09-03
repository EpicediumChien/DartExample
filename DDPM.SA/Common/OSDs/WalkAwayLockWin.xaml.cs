using System;
using System.Windows;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace DDPM.OSDs
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class WalkAwayLockWin : Window
    {
        private DispatcherTimer? animationTimer = null;
        private TimeSpan time;
        private string showString = string.Empty;
        private int int_showString = 0;

        public WalkAwayLockWin(string Content)
        {
            InitializeComponent();
            DataContext = this;
            showString = Content;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            System.Windows.Interop.WindowInteropHelper wndHelper = new System.Windows.Interop.WindowInteropHelper(this);
            Win32Lib.Win32.HideWinFromAltTab(wndHelper.Handle);

            if (!string.IsNullOrWhiteSpace(showString) && (Int32.TryParse(showString, out int_showString)) && (int_showString > 0) && (int_showString <= 5))
            {
                this.WindowState = WindowState.Maximized;
                this.ShowStringText.Text = showString;
                this.Topmost = true;

                time = TimeSpan.FromMilliseconds(int_showString * 1000);
                animationTimer = new DispatcherTimer();
                animationTimer.Interval = TimeSpan.FromMilliseconds(1000);
                animationTimer.Tick += RunTimerTick;
                animationTimer.Start();
            }
            else
            {
                this.Close();
                return;
            }
        }

        private void RunTimerTick(object? sender, EventArgs e)
        {
            if (time == TimeSpan.Zero)
            {
                animationTimer?.Stop();
                this.Dispatcher.Invoke(() =>
                {
                    this.Close();
                });
            }
            else
            {
                time = time.Add(TimeSpan.FromMilliseconds(-1000));

                this.Dispatcher.Invoke(() =>
                {
                    this.ShowStringText.Text = (Convert.ToInt32(ShowStringText.Text) - 1).ToString();
                });
            }
        }

        private void InvokeFadeOutAnimation()
        {
            this.Dispatcher.Invoke(() =>
            {
                Storyboard? sb = Resources["FadeOut"] as Storyboard;
                if (sb == null)
                    return;

                sb.Completed += (o, s) =>
                {
                    this.Close();
                };

                sb.Begin();
            });
        }

        public void StopFadeOutAnimation()
        {
            this.Dispatcher.Invoke(() =>
            {
                Storyboard? sb = Resources["FadeOut"] as Storyboard;

                if (sb == null)
                    return;

                sb.Stop();
            });
        }
    }
}