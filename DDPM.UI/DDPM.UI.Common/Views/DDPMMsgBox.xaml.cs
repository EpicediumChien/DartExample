using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace DDPM.UI.Common.Views
{
    /// <summary>
    /// Interaction logic for DDPMMsgBox.xaml
    /// </summary>
    public partial class DDPMMsgBox : Window
    {
        public enum DDPMMsgBox_btn_result
        {
            close, 
            left,  //generally means yes
            right  //generally means no
        }

        internal class DDPMMsgBoxViewModel : ObservableObject, INotifyPropertyChanged
        {
            public new event PropertyChangedEventHandler? PropertyChanged;

            private string _strTitle = "";
            private string _strContent = "";

            public string strTitle
            {
                get
                { return _strTitle; }
                set
                {
                    _strTitle = value;
                    NotifyPropertyChanged("strTitle");
                }
            }

            public string strContent
            {
                get
                { return _strContent; }
                set
                {
                    _strContent = value;
                    NotifyPropertyChanged("strContent");
                }
            }

            private Visibility _isButtonsShown = Visibility.Visible;
            public Visibility isButtonsShown
            {
                get { return _isButtonsShown; }
                set
                {
                    _isButtonsShown = value;
                    NotifyPropertyChanged("isButtonsShown");
                }
            }

            public void DisableBottomButtons()
            {
                isButtonsShown = Visibility.Collapsed;
            }

            public DDPMMsgBoxViewModel(string strTitle, string strContent)
            {
                this.strTitle = strTitle;
                this.strContent = strContent;
            }

            private void NotifyPropertyChanged(string info)
            {
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs(info));
                }
            }
        }


        public DDPMMsgBox_btn_result result { get; set; } = DDPMMsgBox_btn_result.close;

        public DDPMMsgBox(string strTitle, string strContent)
        {
            InitializeComponent();

            DDPMMsgBoxViewModel vm = new DDPMMsgBoxViewModel(strTitle, strContent);
            this.DataContext = vm;
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

        private void lefe_btn_Click(object sender, RoutedEventArgs e)
        {
            result = DDPMMsgBox_btn_result.left;
            this.Close();
        }

        private void right_btn_Click(object sender, RoutedEventArgs e)
        {
            result = DDPMMsgBox_btn_result.right;
            this.Close();
        }

        private void close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
