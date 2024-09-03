using System.Windows;
using UserControl = System.Windows.Controls.UserControl;

namespace DDPM.UI.Common.UserControls
{
    /// <summary>
    /// Interaction logic for MonitorIndicator.xaml
    /// </summary>
    public partial class MonitorIndicator : UserControl //Robert_Lin, 2024-7-25, Unused class
    {
        public MonitorIndicator()
        {
            InitializeComponent();
            DataContext = this;
        }

        public string InputSource
        {
            get { return (string)GetValue(InputSourceProperty); }
            set { SetValue(InputSourceProperty, value); }
        }

        // Using a DependencyProperty as the backing store for InputSource.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty InputSourceProperty =
            DependencyProperty.Register("InputSource", typeof(string), typeof(MonitorIndicator), new PropertyMetadata(String.Empty));
    }
}