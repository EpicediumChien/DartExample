using System;
using System.Threading.Tasks;
using System.Windows;

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
    }
}