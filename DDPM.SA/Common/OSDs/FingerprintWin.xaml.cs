using System;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace DDPM.OSDs
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class FingerprintWin : Window
    {
        /*private DispatcherTimer? animationTimer = null;
        private TimeSpan time;*/

        public FingerprintWin()
        {
            InitializeComponent();
            DataContext = this;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            System.Windows.Interop.WindowInteropHelper wndHelper = new System.Windows.Interop.WindowInteropHelper(this);
            Win32Lib.Win32.HideWinFromAltTab(wndHelper.Handle);

            /*this.WindowState = WindowState.Maximized;
            this.Topmost = true;*/

            InvokeFadeOutAnimation();

            /*time = TimeSpan.FromMilliseconds(3000);
            animationTimer = new DispatcherTimer();
            animationTimer.Interval = TimeSpan.FromMilliseconds(1000);
            animationTimer.Tick += RunTimerTick;
            animationTimer.Start();*/
        }

        /* private void RunTimerTick(object sender, EventArgs e)
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
         }*/

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

        private void close_Click(object sender, MouseButtonEventArgs e)
        {
            this.Close();
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

        private void Window_Closed(object sender, EventArgs e)
        {
            if (System.Windows.Threading.Dispatcher.CurrentDispatcher != null)
            {
                System.Windows.Threading.Dispatcher.CurrentDispatcher.InvokeShutdown();
            }
        }
    }
}