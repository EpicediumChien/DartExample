using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;

namespace DDPM.UI.Plugin.Common
{
    /// <summary>
    /// MessageModalDialog.xaml 的互動邏輯
    /// </summary>
    public partial class MessageModalDialog : Window
    {
        public MessageModalDialog(string Caption, string Message, string Button1Caption, string Button2Caption = "", string checkboxDescription = "", double width = 420.0, bool btnLogicToggle = false)
        {
            InitializeComponent();

            this.Width = width;
            txtCaption.Text = Caption;
            txtMessage.Text = Message;
            txtMessage.Width = width - 70;
            txtButton1.Content = Button1Caption;
            if (Button1Caption == "")
            {
                txtButton1.Visibility = Visibility.Collapsed;
                txtButton2.Width = 82;
                txtButton2.Margin = new Thickness(0, 5, 0, 0);
            }
            else
            {
                txtButton1.Content = Button1Caption;
            }
            if (Button2Caption == "")
            {
                txtButton2.Visibility = Visibility.Collapsed;
            }
            else
            {
                txtButton2.Content = Button2Caption;
            }
            if (btnLogicToggle) {
                txtButton1.Click -= No_MouseLeftButtonDown;
                txtButton1.Click += Yes_MouseLeftButtonDown;

                txtButton2.Click -= Yes_MouseLeftButtonDown;
                txtButton2.Click += No_MouseLeftButtonDown;
            }
        }

        private bool _btnToggle { get; set; } = false;
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

        public void CloseByCaller()
        {
            Close();
        }

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