using System.Windows;
using UserControl = System.Windows.Controls.UserControl;

namespace DDPM.UI.Plugin.KeyboardPlugin
{
    /// <summary>
    /// BatteryIndicator.xaml 的互動邏輯
    /// </summary>
    public partial class BatteryIndicator : UserControl
    {
        public static readonly DependencyProperty BatteryLevelProperty =
                   DependencyProperty.Register("BatteryLevel", typeof(double), typeof(BatteryIndicator), new PropertyMetadata(100.0, OnBatteryLevelChanged));

        public double BatteryLevel
        {
            get { return (double)GetValue(BatteryLevelProperty); }
            set { SetValue(BatteryLevelProperty, value); }
        }

        public BatteryIndicator()
        {
            InitializeComponent();
        }

        private static void OnBatteryLevelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (BatteryIndicator)d;
            control.UpdateBatteryLevelIndicator();
        }

        private void UpdateBatteryLevelIndicator()
        {
            // Update the width of the BatteryLevelIndicator rectangle based on the BatteryLevel property
            BatteryLevelIndicator.Width = (BatteryLevel / 100.0) * 40; // 40 is the width of the battery outline
        }
    }
}