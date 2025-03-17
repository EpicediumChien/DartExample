using System;
using System.Windows;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace DDPM.OSDs
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class ScrollLockOffWin : Window
    {
        /*private DispatcherTimer? animationTimer = null;
        private TimeSpan time;*/

        public ScrollLockOffWin()
        {
            InitializeComponent();
            DataContext = this;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            System.Windows.Interop.WindowInteropHelper wndHelper = new System.Windows.Interop.WindowInteropHelper(this);
            Win32Lib.Win32.HideWinFromAltTab(wndHelper.Handle);

            this.WindowState = WindowState.Maximized;
            this.Topmost = true;

            InvokeFadeOutAnimation();

            //time = TimeSpan.FromMilliseconds(1200);
            //animationTimer = new DispatcherTimer();
            //animationTimer.Interval = TimeSpan.FromMilliseconds(100);
            //animationTimer.Tick += RunTimerTick;
            //animationTimer.Start();
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
        //private void RunTimerTick(object sender, EventArgs e)
        //{
        //    if (time == TimeSpan.Zero)
        //    {
        //        animationTimer?.Stop();
        //        this.Dispatcher.Invoke(() =>
        //        {
        //            this.Close();
        //        });
        //    }
        //    else
        //    {
        //        time = time.Add(TimeSpan.FromMilliseconds(-100));

        //        //if (time.TotalMilliseconds < 800)
        //        if (time.TotalMilliseconds < 500)
        //        {
        //            this.Dispatcher.Invoke(() =>
        //            {
        //            });
        //        }
        //    }
        //}

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

        private void Window_Closed(object sender, EventArgs e)
        {
            if (System.Windows.Threading.Dispatcher.CurrentDispatcher != null)
            {
                System.Windows.Threading.Dispatcher.CurrentDispatcher.InvokeShutdown();
            }
        }

        //public void StopFadeOutAnimation()
        //{
        //    this.Dispatcher.Invoke(() =>
        //    {
        //        Storyboard? sb = Resources["FadeOut"] as Storyboard;

        //        if (sb == null)
        //            return;

        //        sb.Stop();
        //    });
        //}
    }
}