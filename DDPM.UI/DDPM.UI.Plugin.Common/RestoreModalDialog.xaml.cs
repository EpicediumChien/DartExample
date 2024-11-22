using System.Windows;
using System.Windows.Input;
using DDPM.UI.Common;

namespace DDPM.UI.Plugin.Common
{
    /// <summary>
    /// RestoreModalDialog.xaml 的互動邏輯
    /// </summary>
    public partial class RestoreModalDialog : Window
    {

        public RestoreModalDialog()
        {
            InitializeComponent();

            txtCaption.Text = Strings.RestoreToDefault;
            txtMessage.Text = Strings.RestoreToDefalutText;
            txtYes.Content = Strings.Yes;
            txtNo.Content = Strings.No;
        }
        /*
        private void Yes_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void No_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DialogResult = false;
            Close();
        }*/

        private void Yes_MouseLeftButtonDown(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void No_MouseLeftButtonDown(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}