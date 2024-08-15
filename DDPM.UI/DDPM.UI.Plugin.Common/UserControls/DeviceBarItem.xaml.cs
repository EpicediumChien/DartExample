using System.Windows;
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

        private readonly SolidColorBrush NormalFillBrush = new();
        private readonly SolidColorBrush NormalBorderBrush = new();
        private readonly LinearGradientBrush FocusFillBrush = new();
        private readonly LinearGradientBrush FocusBorderBrush = new();

        public DeviceBarItem(int id, ImageSource icon, string text)
        {
            InitializeComponent();
            vm.Id = id;
            vm.Icon = icon;
            vm.Text = text;
            DataContext = vm;

            NormalFillBrush.Color = Color.FromArgb(0x99, 0x13, 0x2F, 0x54);
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

            bdRoot.Background = NormalFillBrush;
            bdRoot.BorderBrush = NormalBorderBrush;
        }

        public int Id => vm.Id;
        public string Text => vm.Text;

        private void rootGrid_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            bdRoot.Background = FocusFillBrush;
            bdRoot.BorderBrush = FocusBorderBrush;
        }

        private void rootGrid_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (IsSelected) { return; }

            bdRoot.Background = NormalFillBrush;
            bdRoot.BorderBrush = NormalBorderBrush;
        }

        public ICommand? ClickCommand { get; set; }

        private void rootGrid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (IsSelected) { return; }

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
                    bdRoot.Background = FocusFillBrush;
                    bdRoot.BorderBrush = FocusBorderBrush;
                }
                else
                {
                    bdRoot.Background = NormalFillBrush;
                    bdRoot.BorderBrush = NormalBorderBrush;
                }
            }
        }
    }
}