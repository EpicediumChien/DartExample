using Dell.Client.Framework.UX.WPF.Controls;
using System.Diagnostics;
using System.Windows;
using System.Windows.Media.Imaging;
using UserControl = System.Windows.Controls.UserControl;

namespace DDPM.UI.Common
{
    /// <summary>
    /// BatteryIndicator.xaml 的互動邏輯
    /// </summary>
    public partial class BatteryIndicator : UserControl
    {
        public static readonly DependencyProperty BatteryLevelProperty =
            DependencyProperty.Register("BatteryLevel", typeof(double), typeof(BatteryIndicator), new PropertyMetadata(50.0, OnBatteryChanged));

        public static readonly DependencyProperty BatteryStatusProperty =
            DependencyProperty.Register("BatteryStatus", typeof(string), typeof(BatteryIndicator), new PropertyMetadata("Full", OnBatteryChanged));

        public static readonly DependencyProperty ConnectionTypeProperty =
            DependencyProperty.Register("ConnectionType", typeof(string), typeof(BatteryIndicator), new PropertyMetadata("Bluetooth", OnConnectionTypeChanged));

        //public static readonly DependencyProperty ImageSourceProperty =
        //    DependencyProperty.Register("ImageSource", typeof(string), typeof(BatteryIndicator), new PropertyMetadata("/DDPM.UI.Common;component/Resources/Bluetooth.png"));

        public double BatteryLevel
        {
            get { return (double)GetValue(BatteryLevelProperty); }
            set { SetValue(BatteryLevelProperty, value); }
        }

        public string BatteryStatus
        {
            get { return (string)GetValue(BatteryStatusProperty); }
            set { SetValue(BatteryStatusProperty, value); }
        }

        public string ConnectionType
        {
            get { return (string)GetValue(ConnectionTypeProperty); }
            set { SetValue(ConnectionTypeProperty, value); }
        }

        //public double BatteryWidth {
        //  get { return (double)GetValue(BatteryLevelProperty); }
        //  set { SetValue(BatteryLevelProperty, value); }
        //}

        //public string BatteryLevelText {
        //  get { return (string)GetValue(BatteryLevelTextProperty); }
        //  set { SetValue(BatteryLevelTextProperty, value); }
        //}

        //public string ImageSource {
        //  get { return (string)GetValue(ImageSourceProperty); }
        //  set { SetValue(BatteryLevelProperty, value); }
        //}

        public BatteryIndicator()
        {
            InitializeComponent();
            UpdateConnectionType();
            DdpmCommonHelper.BitmapImageUpdated += ImageUpdate;

            // Unregister event
            this.Unloaded += OnUnloaded;
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            DdpmCommonHelper.BitmapImageUpdated -= ImageUpdate;
        }

        private void ImageUpdate(OSThemeEnum oSThemeEnum)
        {
            if (oSThemeEnum == OSThemeEnum.Dark)
            {
                ConnectionTypeImage.Source = DdpmCommonHelper.GetImageSourceFromCommonResource("Resources/Port.png");
            }
            else
            {
                ConnectionTypeImage.Source = DdpmCommonHelper.GetImageSourceFromCommonResource("Resources/LightMode/Port.png");
            }
        }

        private static void OnBatteryChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (BatteryIndicator)d;
            control.UpdateBatteryLevelIndicator();
        }

        private void UpdateBatteryLevelIndicator()
        {
            var charging = BatteryStatus == "Charging" ? "1" : "0";
            var level = "";
            if (BatteryLevel >= 70)
            {
                level = "4";
            }
            else if (BatteryLevel >= 40)
            {
                level = "3";
            }
            else if (BatteryLevel >= 10)
            {
                level = "2";
            }
            else if (BatteryLevel >= 0)
            {
                level = "1";
            }
            else
            {
                level = "0";
            }
            BitmapImage bitmapImage = new BitmapImage(new Uri($"/DDPM.UI.Resources;component/Resources/Images/Battery{charging}{level}.png", UriKind.Relative));
            BatteryLevelText.Text = level == "0" ? "" : BatteryLevel.ToString("##0") + "%";
            Debug.WriteLine($"Battery{charging}{level}");
            BatteryImage.Source = bitmapImage;
        }

        private static void OnConnectionTypeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (BatteryIndicator)d;
            control.UpdateConnectionType();
        }

        private void UpdateConnectionType()
        {
            //0617 Bruce 新增如判斷為有線也跟使用Port的圖片
            if (ConnectionType == "Port" || ConnectionType == "Wired" || ConnectionType == "WiredAudio" || ConnectionType.Contains("USB"))
            {
                if (DdpmCommonHelper.isDarkMode())
                {
                    ConnectionTypeImage.Source = DdpmCommonHelper.GetImageSourceFromCommonResource("Resources/Port.png");
                }
                else
                {

                    ConnectionTypeImage.Source = DdpmCommonHelper.GetImageSourceFromCommonResource("Resources/LightMode/Port.png");
                }
                stackPanel.Visibility = Visibility.Collapsed;
                if (ConnectionType == "Wired" || ConnectionType == "WiredAudio")
                {
                    txt1.Text = DDPM.UI.Resources.Helper.LangHelper.Instance["Wired"];
                }
                else if (ConnectionType.Contains("USB"))
                {
                    if (ConnectionType.Contains("TB 5"))
                    {
                        txt1.Text = Strings.DUSB__C_TB_5;
                    }
                    else if (ConnectionType.Contains("TB 4"))
                    {
                        txt1.Text = Strings.DUSB__C_TB_4;
                    }
                    else if (ConnectionType.Contains("Dual"))
                    {
                        txt1.Text = Strings.Dual_USB__C_DP_14;
                    }
                    else
                    {
                        txt1.Text = Strings.USB_C_DP_14;
                    }
                }
                return;
            }
            BitmapImage bitmapImage = new BitmapImage(new Uri($"/DDPM.UI.Resources;component/Resources/Images/{ConnectionType}.png", UriKind.Relative));
            ConnectionTypeImage.Source = bitmapImage;
            UpdateBatteryLevelIndicator();
        }

        //2024-5-15 Robert_Lin added
        //
        public string Text1
        {
            get { return (string)GetValue(Text1Property); }
            set { SetValue(Text1Property, value); }
        }

        // Using a DependencyProperty as the backing store for Text1.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty Text1Property =
            DependencyProperty.Register("Text1", typeof(string), typeof(BatteryIndicator), new PropertyMetadata(String.Empty, OnText1Changed));

        private static void OnText1Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (BatteryIndicator)d;
            control.UpdateText1();
        }

        private void UpdateText1()
        {
            txt1.Text = Text1;
        }

        public bool NoBattery
        {
            get { return (bool)GetValue(NoBatteryProperty); }
            set { SetValue(NoBatteryProperty, value); }
        }

        // Using a DependencyProperty as the backing store for NoBattery.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty NoBatteryProperty =
            DependencyProperty.Register("NoBattery", typeof(bool), typeof(BatteryIndicator), new PropertyMetadata(false, OnNoBatteryChanged));

        private static void OnNoBatteryChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (BatteryIndicator)d;
            control.UpdateNoBattery();
        }

        private void UpdateNoBattery()
        {
            if (NoBattery)
                stackPanel.Visibility = Visibility.Collapsed;
            else
                stackPanel.Visibility = Visibility.Visible;
        }
    }
}