using System.Windows;
using System.Windows.Input;

namespace DDPM.UI.Plugin.Common
{
    /// <summary>
    /// RestoreModalDialog.xaml 的互動邏輯
    /// </summary>
    public partial class RestoreModalDialog : Window
    {
        private readonly string Caption = "Restore to default";
        private readonly string Message = "Do you want to reset your monitor to factory settings now?";
        private readonly string Yes = "Yes";
        private readonly string No = "No";

        public RestoreModalDialog()
        {
            InitializeComponent();

            txtCaption.Text = Caption;
            txtMessage.Text = Message;
            txtYes.Text = Yes;
            txtNo.Text = No;
        }

        private void Yes_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void No_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}