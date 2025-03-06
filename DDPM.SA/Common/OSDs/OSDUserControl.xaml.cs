using CommunityToolkit.Mvvm.ComponentModel;
using DDPM.SA.Common;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace DDPM.OSDs
{
    public class OSDWinInfo : ObservableObject
    {
        public required string GUID { get; set; }
        public OSDType_Device OSDType_Device { get; set; }
        public required string ShowStringTitle { get; set; }
        public required string ShowStringContent { get; set; }

        private bool isFadeOut = false;
        public bool IsFadeOut
        {
            get { return isFadeOut; }
            set
            {
                isFadeOut = value;
                OnPropertyChanged("IsFadeOut");
            }
        }
    }

    /// <summary>
    /// Interaction logic for OSDUserControl.xaml
    /// </summary>
    public partial class OSDUserControl : UserControl
    {
        private DispatcherTimer? animationTimer = null;
        private TimeSpan time;

        public string GUID
        {
            get { return (string)GetValue(GUIDProperty); }
            set { SetValue(GUIDProperty, value); }
        }

        // Using a DependencyProperty as the backing store for GUID.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty GUIDProperty =
            DependencyProperty.Register("GUID", typeof(string), typeof(OSDUserControl), new PropertyMetadata(""));


        public OSDType_Device OSDType_Device
        {
            get { return (OSDType_Device)GetValue(OSDType_DeviceProperty); }
            set { SetValue(OSDType_DeviceProperty, value); }
        }

        // Using a DependencyProperty as the backing store for OSDType.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty OSDType_DeviceProperty =
            DependencyProperty.Register("OSDType_Device", typeof(OSDType_Device), typeof(OSDUserControl), new PropertyMetadata(OSDType_Device.Unknown));


        public string ShowStringTitle
        {
            get { return (string)GetValue(ShowStringTitleProperty); }
            set { SetValue(ShowStringTitleProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ShowStringTitle.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ShowStringTitleProperty =
            DependencyProperty.Register("ShowStringTitle", typeof(string), typeof(OSDUserControl), new PropertyMetadata(""));


        public string ShowStringContent
        {
            get { return (string)GetValue(ShowStringContentProperty); }
            set { SetValue(ShowStringContentProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ShowStringContent.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ShowStringContentProperty =
            DependencyProperty.Register("ShowStringContent", typeof(string), typeof(OSDUserControl), new PropertyMetadata(""));



        public bool IsFadeOut
        {
            get { return (bool)GetValue(IsFadeOutProperty); }
            set { SetValue(IsFadeOutProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsFadeOut.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsFadeOutProperty =
            DependencyProperty.Register("IsFadeOut", typeof(bool), typeof(OSDUserControl), new PropertyMetadata(false, OnPropertyChanged));

        private static void OnPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            OSDUserControl osdUserControl = (OSDUserControl)d;
            osdUserControl.InvokeFadeOutAnimation();
        }

        public ICommand? Closed
        {
            get { return (ICommand)GetValue(ClosedProperty); }
            set { SetValue(ClosedProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Closed.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ClosedProperty =
            DependencyProperty.Register("Closed", typeof(ICommand), typeof(OSDUserControl), new PropertyMetadata(null));

        public OSDUserControl()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            /* time = TimeSpan.FromMilliseconds(3000);
             animationTimer = new DispatcherTimer();
             animationTimer.Interval = TimeSpan.FromMilliseconds(1000);
             animationTimer.Tick += RunTimerTick;
             animationTimer.Start();*/
        }
        private void RunTimerTick(object? sender, EventArgs e)
        {
            if (time == TimeSpan.Zero)
            {
                animationTimer?.Stop();
                this.Dispatcher.Invoke(() =>
                {
                    Closed?.Execute(GUID);
                });
            }
            else
            {
                time = time.Add(TimeSpan.FromMilliseconds(-1000));
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
                    Closed?.Execute(GUID);
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

        private void close_Click(object sender, MouseButtonEventArgs e)
        {
            Closed?.Execute(GUID);
        }
    }
}
