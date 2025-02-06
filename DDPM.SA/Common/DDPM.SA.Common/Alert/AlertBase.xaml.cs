using System.Windows;
using System.Windows.Media;
using Color = System.Windows.Media.Color;
using ColorConverter = System.Windows.Media.ColorConverter;
using UserControl = System.Windows.Controls.UserControl;

namespace DDPM.SA.Common.Alert
{
    /// <summary>
    /// Interaction logic for AlertBase.xaml
    /// </summary>
    public partial class AlertBase : UserControl
    {
        public static readonly DependencyProperty AlertTypeProperty = DependencyProperty.Register("AlertType", typeof(AlertType), typeof(AlertBase), new PropertyMetadata(AlertType.Default, OnAlertTypeChanged));
        public static readonly DependencyProperty MessageProperty = DependencyProperty.Register("Message", typeof(string), typeof(AlertBase), new PropertyMetadata(string.Empty));

        public AlertType AlertType
        {
            get { return (AlertType)GetValue(AlertTypeProperty); }
            set { SetValue(AlertTypeProperty, value); }
        }

        public string Message
        {
            get { return (string)GetValue(MessageProperty); }
            set { SetValue(MessageProperty, value); }
        }

        public AlertBase()
        {
            InitializeComponent();
        }

        private static void OnAlertTypeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as AlertBase;
            var newAlertType = (AlertType)e.NewValue;
            switch (newAlertType)
            {
                //case AlertType.Info:
                //    control.AlertColorBorder.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#0E92F2"));
                //    control.AlertImagePath.Style = (Style)control.Resources["InfoPathStyle"];
                //    break;

                case AlertType.Error:
                    control.AlertColorBorder.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#D0353F"));
                    control.AlertImagePath.Style = (Style)control.Resources["ErrorPathStyle"];
                    break;

                case AlertType.Warning:
                    control.AlertColorBorder.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E67F01"));
                    control.AlertImagePath.Style = (Style)control.Resources["WarningPathStyle"];
                    break;

                default:
                    control.AlertColorBorder.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#0E92F2"));
                    control.AlertImagePath.Style = (Style)control.Resources["InfoPathStyle"];
                    break;
            }
        }
    }

    public enum AlertType
    {
        Info,
        Error,
        Warning,
        Default
    }
}