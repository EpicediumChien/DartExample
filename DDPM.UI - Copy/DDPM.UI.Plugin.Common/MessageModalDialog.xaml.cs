using System.Windows;
using System.Windows.Input;

namespace DDPM.UI.Plugin.Common
{
    /// <summary>
    /// MessageModalDialog.xaml 的互動邏輯
    /// </summary>
    public partial class MessageModalDialog : Window
    {
        public MessageModalDialog(string Caption, string Message, string Button1Caption, string Button2Caption = "", double width = 417.0)
        {
            InitializeComponent();

            this.Width = width;
            txtCaption.Text = Caption;
            txtMessage.Text = Message;
            txtMessage.Width = width - 70;
            txtButton1.Text = Button1Caption;
            if (Button1Caption == "")
            {
                Button1.Visibility = Visibility.Collapsed;
                Button2.Width = 82;
                Button2.Margin = new Thickness(0, 5, 0, 0);
            }
            else
            {
                txtButton1.Text = Button1Caption;
            }
            if (Button2Caption == "")
            {
                Button2.Visibility = Visibility.Collapsed;
            }
            else
            {
                txtButton2.Text = Button2Caption;
            }
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