using CommunityToolkit.Mvvm.ComponentModel;
using System.ComponentModel;
using System.Drawing.Printing;
using System.Windows;
using System.Windows.Input;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;

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
            //Elsa 20250410 change Thickness(24, 10, 45, 24) to Thickness(24, 10, 45, 14) for hotkey warning title cliping issue
            private Thickness _headerMargin = new Thickness(24, 10, 45, 14);

            public Thickness HeaderMargin
            {
                get { return _headerMargin; }
                set
                {
                    _headerMargin = value;
                    NotifyPropertyChanged("HeaderMargin");
                }
            }

            private Thickness _subHeaderMargin = new Thickness(24, 0, 24, 8);

            public Thickness SubHeaderMargin
            {
                get { return _subHeaderMargin; }
                set
                {
                    _subHeaderMargin = value;
                    NotifyPropertyChanged("SubHeaderMargin");
                }
            }


            private Thickness _leftBtnMargin = new Thickness(8, 0, 24, 24);

            public Thickness LeftBtnMargin
            {
                get { return _leftBtnMargin; }
                set
                {
                    _leftBtnMargin = value;
                    NotifyPropertyChanged("LeftBtnMargin");
                }
            }

            private Thickness _rightBtnMargin = new Thickness(24, 0, 8, 24);

            public Thickness RightBtnrMargin
            {
                get { return _rightBtnMargin; }
                set
                {
                    _rightBtnMargin = value;
                    NotifyPropertyChanged("RightBtnrMargin");
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

            public DDPMMsgBoxViewModel(string strTitle, string strContent, bool IsCloseButton)
            {
                this.strTitle = strTitle;
                this.strContent = strContent;
                if (IsCloseButton)
                    DisableBottomButtons();
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

        public DDPMMsgBox(string strTitle, string strContent, Window owner)
        {
            InitializeComponent();
            this.Owner = owner;
            SetStartPosition(owner);

            DDPMMsgBoxViewModel vm = new DDPMMsgBoxViewModel(strTitle, strContent);
            this.DataContext = vm;
        }

        public DDPMMsgBox(string strTitle, string strContent, bool IsCloseButton, Window owner)
        {
            InitializeComponent();
            this.Owner = owner;
            SetStartPosition(owner);

            DDPMMsgBoxViewModel vm = new DDPMMsgBoxViewModel(strTitle, strContent, IsCloseButton);
            this.DataContext = vm;
        }

        public DDPMMsgBox(string strTitle, string strContent, bool IsCloseButton, Window owner, double width, double height, Thickness titlemargin, Thickness submargin)
        {
            InitializeComponent();
            this.Owner = owner;
            this.Width = width;
            this.Height = height;
            SetStartPosition(owner);

            DDPMMsgBoxViewModel vm = new DDPMMsgBoxViewModel(strTitle, strContent, IsCloseButton)
            {
                HeaderMargin = titlemargin,
                SubHeaderMargin = submargin
            };
            this.DataContext = vm;
        }

        public DDPMMsgBox(string strTitle, string strContent, bool IsCloseButton, Window owner, double width, double height, Thickness titlemargin, Thickness submargin, Thickness leftbtn, Thickness rightbtn)
        {
            InitializeComponent();
            this.Owner = owner;
            this.Width = width;
            this.Height = height;
            SetStartPosition(owner);

            DDPMMsgBoxViewModel vm = new DDPMMsgBoxViewModel(strTitle, strContent, IsCloseButton)
            {
                HeaderMargin = titlemargin,
                SubHeaderMargin = submargin,
                LeftBtnMargin = leftbtn,
                RightBtnrMargin = rightbtn
            };
            this.DataContext = vm;
        }

        private void SetStartPosition(Window owner)
        {
            if (owner != null)
            {
                var screenWidth = SystemParameters.PrimaryScreenWidth;
                var screenHeight = SystemParameters.PrimaryScreenHeight;

                var windowWidth = this.Width;
                var windowHeight = this.Height;

                // Center on the owner window
                this.Left = owner.Left + (owner.Width - windowWidth) / 2;
                this.Top = owner.Top + (owner.Height - windowHeight) / 2;
            }
            else
            {
                // Center on screen if no owner
                this.Left = (SystemParameters.PrimaryScreenWidth - this.Width) / 2;
                this.Top = (SystemParameters.PrimaryScreenHeight - this.Height) / 2;
            }
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