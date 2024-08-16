using System;
using System.Threading.Tasks;
using System.Windows;

namespace DDPM.SA.Common.Popup
{
    /// <summary>
    /// Interaction logic for PopupBase.xaml
    /// </summary>
    public partial class PopupBase : Window
    {
        public event EventHandler<object> LeftButtonClick;

        public event EventHandler<object> RightButtonClick;

        /// <summary>
        /// Event triggered if the user directly presses X or waits for the window to close automatically
        /// </summary>
        public event EventHandler<object> Default_Event;

        private object _object;

        /// <summary>
        ///
        /// </summary>
        /// <param name="HeaderText"></param>
        /// <param name="SubHeaderText"></param>
        /// <param name="LeftButtonContent"></param>
        /// <param name="RightButtonContent"></param>
        /// <param name="ob">The object returned by the button event</param>
        /// <param name="IsStayOnly"></param>
        /// <param name="autoCloseTimeInSeconds"></param>
        public PopupBase(string HeaderText, string SubHeaderText, string LeftButtonContent, string RightButtonContent, object ob, bool IsStayOnly, int autoCloseTimeInSeconds)
        {
            InitializeComponent();
            Header.Text = HeaderText;
            SubHeader.Text = SubHeaderText;
            _object = ob;
            LeftButton.Visibility = Visibility.Collapsed;
            RightButton.Visibility = Visibility.Collapsed;
            if (!string.IsNullOrEmpty(LeftButtonContent))
            {
                LeftButton.Visibility = Visibility.Visible;
                LeftButton.Content = LeftButtonContent;
            }
            if (!string.IsNullOrEmpty(RightButtonContent))
            {
                RightButton.Visibility = Visibility.Visible;
                RightButton.Content = RightButtonContent;
            }
            if (!IsStayOnly && autoCloseTimeInSeconds > 0)
            {
                Task.Delay(autoCloseTimeInSeconds * 1000).ContinueWith(t => this.Dispatcher.Invoke(Close));
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            LeftButtonClick = null;
            RightButtonClick = null;
            this.Close();
        }

        private void LeftButton_Click(object sender, RoutedEventArgs e)
        {
            Default_Event = null;
            RightButtonClick = null;
            this.Close();
            LeftButtonClick?.Invoke(this, _object);
            LeftButtonClick = null;
        }

        private void RightButton_Click(object sender, RoutedEventArgs e)
        {
            Default_Event = null;
            LeftButtonClick = null;
            this.Close();
            RightButtonClick?.Invoke(this, _object);
            RightButtonClick = null;
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            Default_Event?.Invoke(this, _object);
            Default_Event = null;
        }
    }
}