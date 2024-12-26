using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace DDPM.SA.Common.Popup
{
    //PopBase usage example:
    //See sample code in DDPM.UI.Module.EzArrange/EzArrangeRightVierw.xaml.cs, HandleSplitItemDeleteCommand()
    //
    // PopupBase popBase = new PopupBase( headerText, subHeaderText, leftButtonContent, rightButtonContent, ob, isStayOny, autoCloseTimeSec);
    // popBase.Owner = System.Windows.Application.Current.MainWindow; //To show the PopBase at center of MainWindow
    // bool? popResult = popBase.ShowDialog();
    //  //popResult: Close=null; LeftButton=false; RightButton=true

    /// <summary>
    /// Interaction logic for PopupBase.xaml
    /// </summary>
    public partial class PopupBase : Window
    {
        //Robert_Lin, 2024-11-8, the return value of PopBase.ShowDialog()
        private bool? _dialogResult_Close = null; //User click "X" close button
        private bool? _dialogResult_Left = false;  //User click LeftButton
        private bool? _dialogResult_Right = true; //User click RightButton

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
        public PopupBase(string HeaderText, string SubHeaderText, string LeftButtonContent, string RightButtonContent, object ob, bool IsStayOnly, int autoCloseTimeInSeconds, string Horizontal = "L")
        {
            InitializeComponent();
            //Header.Text = HeaderText;
            //SubHeader.Text = SubHeaderText;
            Header1.Text = HeaderText;
            SubHeader1.Text = SubHeaderText;

            _object = ob;
            //LeftButton.Visibility = Visibility.Collapsed;
            //RightButton.Visibility = Visibility.Collapsed;
            if (!string.IsNullOrEmpty(LeftButtonContent))
            {
                //LeftButton.Visibility = Visibility.Visible;
                //LeftButton.Content = LeftButtonContent;

                LeftButton1.Visibility = Visibility.Visible;
                LeftButton1.Content = LeftButtonContent;
            }
            if (!string.IsNullOrEmpty(RightButtonContent))
            {
                //RightButton.Visibility = Visibility.Visible;
                //RightButton.Content = RightButtonContent;
                RightButton1.Visibility = Visibility.Visible;
                RightButton1.Content = RightButtonContent;
            }

            if (Horizontal == "C")
            {
                LeftButton1.Visibility = Visibility.Collapsed;
                RightButton1.Visibility = Visibility.Visible;
                ButtonPanel1.HorizontalAlignment = HorizontalAlignment.Center;
            }
            if (!IsStayOnly && autoCloseTimeInSeconds > 0)
            {
                Task.Delay(autoCloseTimeInSeconds * 1000).ContinueWith(t => this.Dispatcher.Invoke(Close));
            }
        }
        public void UpdateContent(string HeaderText, string SubHeaderText)
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(UpdateContent);
                return;
            }
            Header1.Text = HeaderText;
            SubHeader1.Text = SubHeaderText;
        }
        public void ShowWindow()
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(ShowWindow);
                return;
            }
            Show();
        }

        public void CloseWindow()
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(CloseWindow);
                return;
            }
            Close();
        }
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            //Robert_Lin, 2024-11-8, the return value of PopBase.ShowDialog()
            DialogResult = _dialogResult_Close;
            LeftButtonClick = null;
            RightButtonClick = null;
            this.Close();
        }

        private void LeftButton_Click(object sender, RoutedEventArgs e)
        {
            //Robert_Lin, 2024-11-8, the return value of PopBase.ShowDialog()
            DialogResult = _dialogResult_Left;
            Default_Event = null;
            RightButtonClick = null;
            this.Close();
            LeftButtonClick?.Invoke(this, _object);
            LeftButtonClick = null;
        }

        private void RightButton_Click(object sender, RoutedEventArgs e)
        {
            //Robert_Lin, 2024-11-8, the return value of PopBase.ShowDialog()
            DialogResult = _dialogResult_Right;
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

        private void closeX_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            //DialogResult = _dialogResult_Close;
            //LeftButtonClick = null;
            //RightButtonClick = null;
            //this.Close();
        }

        private void rootBorder_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            //Support move window
            if ((e.ChangedButton == MouseButton.Left) && (e.ClickCount == 1))
            {
                this.DragMove();
            }
        }

        private void closeX_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DialogResult = _dialogResult_Close;
            LeftButtonClick = null;
            RightButtonClick = null;
            this.Close();
        }
    }
}