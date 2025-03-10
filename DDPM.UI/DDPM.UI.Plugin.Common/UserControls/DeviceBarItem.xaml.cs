using Dell.Client.Framework.UX.WPF.Controls;
using System.ComponentModel;
using System.Resources;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace DDPM.UI.Common
{
    /// <summary>
    /// Interaction logic for DeviceBarItem.xaml
    /// </summary>
    public partial class DeviceBarItem : System.Windows.Controls.UserControl
    {
        private readonly VbarItemViewModel vm = new();

        private LinearGradientBrush FocusFillBrush;
        private LinearGradientBrush FocusBorderBrush;
        private SolidColorBrush whiteBrush = new SolidColorBrush(Colors.White);

        public DeviceBarItem(int id, ImageSource icon, string text, bool isHidden = false)
        {
            InitializeComponent();

            vm.Id = id;
            vm.Icon = icon;
            vm.Text = text;
            vm.Visibility = isHidden ? Visibility.Collapsed : Visibility.Visible;
            this.IsEnabled = !isHidden;
            this.DataContext = vm;

            FocusFillBrush = (LinearGradientBrush)FindResource("Brush_GradientButtonCyan");
            FocusBorderBrush = (LinearGradientBrush)FindResource("Brush_GradientBorderCyan");
            ((Canvas)this.FindName(resolveIconName(Id))).Tag = (SolidColorBrush)FindResource("DefaultTheme_PathColor");
        }

        public int Id => vm.Id;
        public string Text => vm.Text;

        private void rootGrid_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (bdRoot.Background == FocusFillBrush)
            { return; }

            if (DdpmCommonHelper.isDarkMode())
            {
                bdRoot.Background = (SolidColorBrush)FindResource("Vbar_BdBrush_Hover");
                bdRoot.BorderBrush = (SolidColorBrush)FindResource("Vbar_BdBrush_Hover");
            }
            else
            {
                //bdRoot.Background = (SolidColorBrush)FindResource("Vbar_BkBrush_Hover");
                bdRoot.BorderBrush = (SolidColorBrush)FindResource("MainNav_BdBrush_Hover_Light");
            }
        }

        private void rootGrid_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (IsSelected)
            { return; }
            bdRoot.Background = (SolidColorBrush)FindResource("Vbar_BkBrush_Default");
            bdRoot.BorderBrush = (SolidColorBrush)FindResource("Vbar_BdBrush_Default");
        }

        public ICommand? ClickCommand { get; set; }

        private void rootGrid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (IsSelected)
            { return; }
            bdRoot.BorderBrush = new SolidColorBrush((Color)FindResource("Vbar_BdColor_Hover"));

            if (ClickCommand != null)
                ClickCommand?.Execute(this);

            IsSelected = true;
        }

        public bool IsSelected
        {
            get => vm.IsSelected;
            set
            {
                vm.IsSelected = value;
                if (IsSelected)
                {
                    SelectBarItem();
                }
                else
                {
                    RenewBarItem();
                }
            }
        }

        private string resolveIconName(int Id)
        {
            switch (Id)
            {
                case 0:
                    return "Display_0";
                case 1:
                    return "Webcam_1";
                case 2:
                    return "KeyMouse_2";
                case 3:
                    return "Stylus_3";
                case 4:
                    return "HeadSet_4";
                case 5:
                    return "SpeakSound_5";
                case 6:
                    return "Dock_6";
                default:
                    return string.Empty;
            }
        }

        public void RenewBarItem()
        {
            bdRoot.Background = (SolidColorBrush)FindResource("Vbar_BkBrush_Default");
            bdRoot.BorderBrush = (SolidColorBrush)FindResource("Vbar_BdBrush_Default");
            ((Canvas)this.FindName(resolveIconName(Id))).Tag = (SolidColorBrush)FindResource("DefaultTheme_PathColor");
            IconName.ClearValue(TextBlock.ForegroundProperty);
        }

        public void SelectBarItem()
        {
            bdRoot.Background = FocusFillBrush;
            bdRoot.BorderBrush = FocusBorderBrush;
            ((Canvas)this.FindName(resolveIconName(Id))).Tag = whiteBrush;
            IconName.Foreground = whiteBrush;
        }

        //Robert_Lin 2025-2-8 added for Narrator to handle [Enter] key = Mouse.LeftButtonDown
        private void UserControl_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                e.Handled = true;
                if (IsSelected)
                { return; }
                bdRoot.BorderBrush = new SolidColorBrush((Color)FindResource("Vbar_BdColor_Hover"));

                if (ClickCommand != null)
                    ClickCommand?.Execute(this);

                IsSelected = true;
            }
        }
    }
}