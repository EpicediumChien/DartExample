using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace DDPM.OSDs
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class UnMuteWin : Window
    {
        private DispatcherTimer? animationTimer = null;
        private TimeSpan time;

        private string showString = string.Empty;

        public UnMuteWin(string Content)
        {
            InitializeComponent();
            DataContext = this;
            showString = Content + " is Unmuted";
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            System.Windows.Interop.WindowInteropHelper wndHelper = new System.Windows.Interop.WindowInteropHelper(this);
            Win32Lib.Win32.HideWinFromAltTab(wndHelper.Handle);

            if (!string.IsNullOrWhiteSpace(showString))
            {
                this.WindowState = WindowState.Maximized;
                this.ShowStringText.Text = showString;
                this.Topmost = true;

                time = TimeSpan.FromMilliseconds(3000);
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

        private void RunTimerTick(object sender, EventArgs e)
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
            }
        }

        private void close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }
        public void ShowWindow()
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(ShowWindow);
                return;
            }
            Show();
        }
        public void CloseWindow()
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(CloseWindow);
                return;
            }
            Close();
        }
    }
}