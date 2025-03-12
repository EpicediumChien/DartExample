using DDPM.UI.Resources.Helper;
using Dell.Client.Framework.UX.WPF.Controls;
using Newtonsoft.Json.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using Windows.UI.Popups;
using Color = System.Windows.Media.Color;
using Point = System.Windows.Point;
using UserControl = System.Windows.Controls.UserControl;

namespace DDPM.UI.Common
{
    /// <summary>
    /// TabButton.xaml 的互動邏輯
    /// </summary>
    public partial class TabButton : UserControl
    {
        public static readonly DependencyProperty CaptionProperty =
            DependencyProperty.Register("Caption", typeof(string), typeof(TabButton), new PropertyMetadata("TabButton"));

        public static readonly DependencyProperty FocusedProperty =
            DependencyProperty.Register("Focused", typeof(bool), typeof(TabButton), new PropertyMetadata(false, OnFocusChanged));

        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.Register("Command", typeof(ICommand), typeof(TabButton));

        public static readonly DependencyProperty HasInfoIconProperty =
            DependencyProperty.Register("HasInfoIcon", typeof(bool), typeof(TabButton), new PropertyMetadata(false, OnHasInfoIconChanged));

        public static readonly DependencyProperty TabInfoProperty =
            DependencyProperty.Register("TabInfo", typeof(string), typeof(TabButton), new PropertyMetadata("", OnTabInfoChanged));

        public static readonly DependencyProperty CaptionSizeProperty =
            DependencyProperty.Register("CaptionSize", typeof(double), typeof(TabButton), new PropertyMetadata(16.0));

        public new static readonly DependencyProperty BorderThicknessProperty =
            DependencyProperty.Register("BorderThickness", typeof(Thickness), typeof(TabButton), new PropertyMetadata(new Thickness(1.5)));

        public static readonly DependencyProperty CornerRadiusProperty =
            DependencyProperty.Register("CornerRadius", typeof(CornerRadius), typeof(TabButton), new PropertyMetadata(new CornerRadius(4)));

        public new static readonly DependencyProperty BackgroundProperty =
            DependencyProperty.Register("Background", typeof(Color), typeof(TabButton), new PropertyMetadata(Color.FromArgb(0x99, 0x13, 0x2F, 0x54), OnBackgroundChanged));

        public static readonly DependencyProperty TooltipOffsetHProperty =
            DependencyProperty.Register("TooltipOffsetH", typeof(double), typeof(TabButton), new PropertyMetadata(6.0, OnTooltipOffsetHChanged));

        public static readonly DependencyProperty MainImageSourceProperty =
            DependencyProperty.Register("MainImageSource", typeof(ImageSource), typeof(TabButton), new PropertyMetadata(null));

        private readonly SolidColorBrush NormalFillBrush = new();
        private readonly SolidColorBrush NormalBorderBrush = new();
        private readonly SolidColorBrush NormalTxtBrush = new();
        private readonly SolidColorBrush FocusWhiteTxtBrush = new();
        private readonly LinearGradientBrush FocusFillBrush = new();
        private readonly LinearGradientBrush FocusBorderBrush = new();

        public TabButton()
        {
            InitializeComponent();
            //NormalFillBrush.Color = Color.FromArgb(0x99, 0x13, 0x2F, 0x54);
            //NormalFillBrush.Color = Background;
            NormalBorderBrush.Color = Color.FromArgb(0x0D, 0xFF, 0xFF, 0xFF);
            FocusFillBrush.StartPoint = new Point(0, 0);
            FocusFillBrush.EndPoint = new Point(1, 0);
            FocusFillBrush.GradientStops.Add(new GradientStop(Color.FromArgb(0xFF, 0x06, 0x72, 0xCB), 0));
            FocusFillBrush.GradientStops.Add(new GradientStop(Color.FromArgb(0xFF, 0x00, 0x78, 0xD4), 0.5));
            FocusFillBrush.GradientStops.Add(new GradientStop(Color.FromArgb(0xFF, 0x6E, 0x69, 0xCF), 1));
            FocusBorderBrush.StartPoint = new Point(0, 0);
            FocusBorderBrush.EndPoint = new Point(1, 0);
            FocusBorderBrush.GradientStops.Add(new GradientStop(Color.FromArgb(0xFF, 0x55, 0xB4, 0xFD), 0));
            FocusBorderBrush.GradientStops.Add(new GradientStop(Color.FromArgb(0xFF, 0x6E, 0x69, 0xCF), 1));
            FocusWhiteTxtBrush.Color = Color.FromArgb(0xFF, 0xFF, 0xFF, 0xFF);
            NormalTxtBrush = (SolidColorBrush)System.Windows.Application.Current.Resources["DefaultTheme_TxtColor"];
        }

        public ImageSource MainImageSource
        {
            get { return (ImageSource)GetValue(MainImageSourceProperty); }
            set { SetValue(MainImageSourceProperty, value); }
        }

        public string Caption
        {
            get { return (string)GetValue(CaptionProperty); }
            set { SetValue(CaptionProperty, value); }
        }

        public string TabInfo
        {
            get { return (string)GetValue(TabInfoProperty); }
            set { SetValue(TabInfoProperty, value); }
        }

        public bool Focused
        {
            get { return (bool)GetValue(FocusedProperty); }
            set { SetValue(FocusedProperty, value); }
        }

        public bool HasInfoIcon
        {
            get { return (bool)GetValue(HasInfoIconProperty); }
            set { SetValue(HasInfoIconProperty, value); }
        }

        public ICommand Command
        {
            get { return (ICommand)GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }

        public double CaptionSize
        {
            get { return (double)GetValue(CaptionSizeProperty); }
            set { SetValue(CaptionSizeProperty, value); }
        }

        public new Thickness BorderThickness
        {
            get { return (Thickness)GetValue(BorderThicknessProperty); }
            set { SetValue(BorderThicknessProperty, value); }
        }

        public CornerRadius CornerRadius
        {
            get { return (CornerRadius)GetValue(CornerRadiusProperty); }
            set { SetValue(CornerRadiusProperty, value); }
        }

        public new Color Background
        {
            get { return (Color)GetValue(BackgroundProperty); }
            set { SetValue(BackgroundProperty, value); }
        }

        public double TooltipOffsetH
        {
            get { return (double)GetValue(TooltipOffsetHProperty); }
            set { SetValue(TooltipOffsetHProperty, value); }
        }

        private static void OnTabInfoChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (TabButton)d;
            control.UpdateTabInfo();
        }

        private void UpdateTabInfo()
        {
            txtToolTip.Text = TabInfo;
        }

        private static void OnFocusChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (TabButton)d;
            control.UpdateBrush();
        }

        private static void OnHasInfoIconChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (TabButton)d;
            control.UpdateInfoIcon();
        }

        private static void OnBackgroundChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (TabButton)d;
            control.UpdateBackground();
        }

        private void UpdateBackground()
        {
            NormalFillBrush.Color = Background;
            UpdateBrush();
        }

        private void UpdateInfoIcon()
        {
            if (HasInfoIcon)
            {
                InfoIcon.Visibility = Visibility.Visible;
            }
            else
            {
                InfoIcon.Visibility = Visibility.Hidden;
            }
        }

        private void UpdateBrush()
        {
            if (Focused)
            {
                Border.Background = FocusFillBrush;
                Border.BorderBrush = FocusBorderBrush;
                TabCaption.Foreground = FocusWhiteTxtBrush;
            }
            else
            {
                Border.Background = NormalFillBrush;
                Border.BorderBrush = NormalBorderBrush;
                TabCaption.ClearValue(ForegroundProperty);
            }
        }

        private void OnClick(object sender, RoutedEventArgs e)
        {
            if (Command != null && Command.CanExecute(null))
                Command.Execute(null);
        }

        private static void OnTooltipOffsetHChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (TabButton)d;
            control.UpdateTooltipOffsetH();
        }

        private void UpdateTooltipOffsetH()
        {
        }

        private void TabCaption_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (Caption == LangHelper.Instance["Off2"])
                toolTip.HorizontalOffset = TooltipOffsetH;
            else
            {
                var offset = GetTextWidth(txtToolTip.Text, 12);
                if (offset > 328)
                    offset = -350;
                else
                    offset = -24 - offset;
                toolTip.HorizontalOffset = offset;
            }
        }

        private double GetTextWidth(string text, double fontSize, string fontFamily = "Roboto")
        {
            var typeface = new Typeface(new System.Windows.Media.FontFamily(fontFamily), FontStyles.Normal, FontWeights.Normal, FontStretches.Normal);

            var formattedText = new FormattedText(
                text,
                System.Globalization.CultureInfo.CurrentUICulture,
                System.Windows.FlowDirection.LeftToRight,
                typeface,
                fontSize,
                System.Windows.Media.Brushes.Black,
                new NumberSubstitution(),
                1.0);
            return formattedText.Width;
        }
    }
}