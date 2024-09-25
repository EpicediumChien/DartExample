using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;

namespace DDPM.ShowOSD
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class ShowOSDWin : Window
    {
        private DispatcherTimer? animationTimer = null;
        private TimeSpan time;
        private string showString = string.Empty;
        private double ShowTextFontSize = 80;
        double tempW = 224.0/ 7.0;
        double tempH = 80;

        public ShowOSDWin(string str, double dbFontSize = 80)
        {
            InitializeComponent();
            DataContext = this;
            showString = str;

            ShowTextFontSize = dbFontSize;
            SetOstTextFontSize(ShowTextFontSize);

            //devEdidDEMO = devEdid;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //[2023/06/14] Elie: Add new code for hide window from Alt+tab
            System.Windows.Interop.WindowInteropHelper wndHelper = new System.Windows.Interop.WindowInteropHelper(this);
            Win32Lib.Win32.HideWinFromAltTab(wndHelper.Handle);

            if (string.IsNullOrEmpty(showString))
            {
                this.Close();
                return;
            }

            this.tbShowText.Text = showString;

            this.Width = showString.Length * tempW;
            this.Height = tempH;
            this.Topmost = false;
            this.Topmost = true;

            //time = TimeSpan.FromMilliseconds(1800);
            time = TimeSpan.FromMilliseconds(1200);
            animationTimer = new DispatcherTimer();
            animationTimer.Interval = TimeSpan.FromMilliseconds(100);//.FromSeconds(1);
            animationTimer.Tick += RunTimerTick;
            animationTimer.Start();
        }

        public void SetOstTextFontSize(double dbSize)
        {
            this.tbShowText.FontSize = dbSize;
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
                time = time.Add(TimeSpan.FromMilliseconds(-100));

                //if (time.TotalMilliseconds < 800)
                if (time.TotalMilliseconds < 500)
                {
                    this.Dispatcher.Invoke(() =>
                    {
                        System.Windows.Media.Brush b = this.tbShowText.Foreground;

                        System.Windows.Media.Color MyColor = ((SolidColorBrush)b).Color;
                        int a = (int)MyColor.A - (int)60;

                        if (a < 0)
                            a = 0;
                        MyColor.A = (byte)a;

                        SolidColorBrush brush = new SolidColorBrush(MyColor);

                        this.tbShowText.Foreground = brush;//new SolidColorBrush(System.Windows.Media.Color.FromArgb(125, 0, 0, 255));
                    });
                }
            }
        }
    } // public partial class MainWindow : Window
}