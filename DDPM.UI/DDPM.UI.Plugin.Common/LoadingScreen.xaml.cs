using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace DDPM.UI.Plugin.Common
{
    /// <summary>
    /// LoadingScreen.xaml 的互動邏輯
    /// </summary>
    public partial class LoadingScreen : Window
    {
        private DispatcherTimer _closeTimer;
        public LoadingScreen(double width, double height)
        {
            InitializeComponent();
            //this.Width = width;
            //this.Height = height;

            var bitmap = new BitmapImage(new Uri("pack://application:,,,/DDPM.UI.Common;component/Resources/Loading.png"));
            LoadingImage.Source = bitmap;

            var rotateTransform = new RotateTransform();
            LoadingImage.RenderTransform = rotateTransform;
            LoadingImage.RenderTransformOrigin = new Point(0.5, 0.5);

            var animation = new DoubleAnimation
            {
                From = 0,
                To = 360,
                Duration = new Duration(TimeSpan.FromSeconds(1)),
                RepeatBehavior = RepeatBehavior.Forever
            };

            rotateTransform.BeginAnimation(RotateTransform.AngleProperty, animation);

     
            _closeTimer = new DispatcherTimer();
            _closeTimer.Interval = TimeSpan.FromSeconds(3);
            _closeTimer.Tick += CloseTimer_Tick;
            _closeTimer.Start();
        }
        private void CloseTimer_Tick(object sender, EventArgs e)
        {
            _closeTimer.Stop();
            this.Close();
        }
    }
}
